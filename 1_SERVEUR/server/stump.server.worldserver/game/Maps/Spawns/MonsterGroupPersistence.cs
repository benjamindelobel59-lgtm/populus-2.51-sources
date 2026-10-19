using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using NLog;
using Stump.Core.Attributes;
using Stump.Core.Reflection;
using Stump.DofusProtocol.Enums;
using Stump.Server.BaseServer.Initialization;
using Stump.Server.WorldServer.Database;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Monsters;
using Stump.Server.WorldServer.Game.Maps.Cells;
using Monster = Stump.Server.WorldServer.Game.Actors.RolePlay.Monsters.Monster;

namespace Stump.Server.WorldServer.Game.Maps.Spawns
{
    /// <summary>
    /// NEKFEU : les groupes de monstres sont gardes d'un redemarrage a l'autre. A chaque sauvegarde du monde (et a l'arret),
    /// tous les groupes poses sur une map par un pool d'apparition sont ecrits dans un fichier ; au demarrage suivant :
    /// - groupes normaux (ClassicalSpawningPool) : recrees a l'identique (composition, grade, case, orientation) ;
    /// - groupes de donjon / apparitions fixes (composition fixee par la base) : date d'apparition reprise.
    /// La date d'apparition est conservee partout -> le bonus d'etoiles (MonsterGroup.AgeBonus) continue de monter.
    /// Les groupes en combat au moment de la sauvegarde ne sont pas enregistres (ils seront tires a nouveau).
    /// </summary>
    public class MonsterGroupPersistence : Singleton<MonsterGroupPersistence>, ISaveable
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        [Variable(true)]
        public static bool Enabled = true;

        [Variable]
        public static string SaveFile = "./monstergroups.save";

        public class Entry
        {
            public int MapId;
            public string Key;          // C<taille> (groupe normal), D<id spawn donjon>, S<id spawn fixe>
            public short CellId;
            public DirectionsEnum Direction;
            public DateTime CreationDate;
            public List<Tuple<int, uint>> Monsters = new List<Tuple<int, uint>>(); // (monstre, grade)
        }

        private readonly Dictionary<string, Queue<Entry>> m_entries = new Dictionary<string, Queue<Entry>>();
        private readonly object m_lock = new object();
        private int m_restored;
        private int m_retires; // MYSTIC 19/10 : monstres retires des groupes restaures (plus de spawn actif)

        [Initialization(InitializationPass.Fourth)]
        public void Initialize()
        {
            Load();
            World.Instance.RegisterSaveableInstance(this);
        }

        private static string Id(int mapId, string key) => mapId + "|" + key;

        private void Load()
        {
            if (!Enabled || !File.Exists(SaveFile))
            {
                logger.Info("[Groupes] aucun fichier de groupes a restaurer ({0})", SaveFile);
                return;
            }

            var count = 0;
            foreach (var line in File.ReadAllLines(SaveFile, Encoding.UTF8))
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                        continue;

                    var p = line.Split('|');
                    var entry = new Entry
                    {
                        MapId = int.Parse(p[0], CultureInfo.InvariantCulture),
                        Key = p[1],
                        CellId = short.Parse(p[2], CultureInfo.InvariantCulture),
                        Direction = (DirectionsEnum)int.Parse(p[3], CultureInfo.InvariantCulture),
                        CreationDate = new DateTime(long.Parse(p[4], CultureInfo.InvariantCulture))
                    };
                    if (p.Length > 5 && p[5].Length > 0)
                        foreach (var m in p[5].Split(','))
                        {
                            var mg = m.Split(':');
                            entry.Monsters.Add(Tuple.Create(int.Parse(mg[0], CultureInfo.InvariantCulture), uint.Parse(mg[1], CultureInfo.InvariantCulture)));
                        }

                    // une date dans le futur (horloge changee) n'a pas de sens
                    if (entry.CreationDate > DateTime.Now)
                        entry.CreationDate = DateTime.Now;

                    var id = Id(entry.MapId, entry.Key);
                    if (!m_entries.TryGetValue(id, out var queue))
                        m_entries[id] = queue = new Queue<Entry>();
                    queue.Enqueue(entry);
                    count++;
                }
                catch (Exception ex)
                {
                    logger.Warn("[Groupes] ligne ignoree ({0}) : {1}", ex.Message, line);
                }
            }

            logger.Info("[Groupes] {0} groupes de monstres a restaurer (fichier {1})", count, SaveFile);
        }

        public Entry Take(int mapId, string key)
        {
            lock (m_lock)
            {
                if (!m_entries.TryGetValue(Id(mapId, key), out var queue) || queue.Count == 0)
                    return null;
                return queue.Dequeue();
            }
        }

        /// <summary>Groupes normaux enregistres pour cette map (cles C...), retires de la liste.</summary>
        public List<Entry> TakeClassical(int mapId)
        {
            var result = new List<Entry>();
            lock (m_lock)
            {
                foreach (var pair in m_entries.Where(x => x.Key.StartsWith(mapId + "|C")).ToArray())
                {
                    while (pair.Value.Count > 0)
                        result.Add(pair.Value.Dequeue());
                }
            }
            return result;
        }

        /// <summary>Recree un groupe normal enregistre (null si un monstre n'existe plus).</summary>
        public MonsterGroup BuildGroup(Map map, Entry entry)
        {
            if (entry.Monsters.Count == 0)
                return null;

            // MYSTIC : les recherches ne sont plus dans les groupes normaux (RecherchesManager) : retires des groupes sauvegardes
            if (entry.Monsters.Any(x => Recherches.RecherchesManager.IsWanted(x.Item1)))
            {
                entry.Monsters = entry.Monsters.Where(x => !Recherches.RecherchesManager.IsWanted(x.Item1)).ToList();
                if (entry.Monsters.Count == 0)
                    return null;
            }

            // MYSTIC 19/10 : un groupe sauvegarde ne garde que les monstres qui ont encore un spawn ACTIF sur cette map
            // (monstres de quete, Poutch, boss ou recherches dont le spawn a ete desactive restaient pour toujours : restaures puis
            // re-sauvegardes a chaque redemarrage)
            var permis = new HashSet<int>(map.MonsterSpawns.Where(x => !x.IsDisabled).Select(x => x.MonsterId));
            if (entry.Monsters.Any(x => !permis.Contains(x.Item1)))
            {
                m_retires += entry.Monsters.Count(x => !permis.Contains(x.Item1));
                entry.Monsters = entry.Monsters.Where(x => permis.Contains(x.Item1)).ToList();
                if (entry.Monsters.Count == 0)
                    return null;
            }

            // MYSTIC 19/10 : monstres rares limités par sous-zone (Leprechaun, Hell Mina : 1 max)
            if (entry.Monsters.Any(x => MonstresLimites.EstLimite(x.Item1)))
            {
                var gardes = new List<Tuple<int, uint>>();
                foreach (var x in entry.Monsters)
                {
                    if (MonstresLimites.EstLimite(x.Item1) && MonstresLimites.Atteinte(map, x.Item1, gardes.Select(y => y.Item1)))
                    {
                        m_retires++;
                        continue;
                    }
                    gardes.Add(x);
                }
                entry.Monsters = gardes;
                if (entry.Monsters.Count == 0)
                    return null;
            }

            var grades = entry.Monsters.Select(x => MonsterManager.Instance.GetMonsterGrade(x.Item1, (int)x.Item2)).ToArray();
            if (grades.Any(x => x == null))
                return null;

            var cell = entry.CellId >= 0 && entry.CellId < map.Cells.Length ? map.Cells[entry.CellId] : null;
            if (cell == null || !cell.Walkable || map.GetActors<MonsterGroup>().Any(x => x.Cell != null && x.Cell.Id == cell.Id))
                cell = map.GetRandomFreeCell();

            var group = new MonsterGroup(map.GetNextContextualId(), new ObjectPosition(map, cell, entry.Direction));
            foreach (var grade in grades)
                group.AddMonster(new Monster(grade, group));

            group.CreationDate = entry.CreationDate;
            m_restored++;
            return group;
        }

        /// <summary>Groupe de donjon / fixe : reprend la date d'apparition enregistree (donc le bonus d'etoiles).</summary>
        public void ApplyFixed(MonsterGroup group, Map map, string key)
        {
            var entry = Take(map.Id, key);
            if (entry == null)
                return;

            group.CreationDate = entry.CreationDate;
            m_restored++;
        }

        public void Save()
        {
            if (!Enabled)
                return;

            var sb = new StringBuilder();
            sb.AppendLine("# NEKFEU : groupes de monstres (map|cle|case|orientation|date d'apparition (ticks)|monstre:grade,...) - " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            var count = 0;
            try
            {
                foreach (var map in World.Instance.GetMaps())
                {
                    foreach (var pool in map.SpawningPools)
                    {
                        foreach (var group in pool.GetSpawnedGroupsSnapshot())
                        {
                            var key = pool.GetPersistenceKey(group);
                            if (key == null || group.Map != map || group.Cell == null)
                                continue;

                            var monsters = string.Join(",", group.GetMonsters().Select(x => x.Grade.MonsterId.ToString(CultureInfo.InvariantCulture) + ":" + x.Grade.GradeId.ToString(CultureInfo.InvariantCulture)));
                            sb.Append(map.Id.ToString(CultureInfo.InvariantCulture)).Append('|').Append(key).Append('|')
                              .Append(group.Cell.Id.ToString(CultureInfo.InvariantCulture)).Append('|').Append(((int)group.Direction).ToString(CultureInfo.InvariantCulture)).Append('|')
                              .Append(group.CreationDate.Ticks.ToString(CultureInfo.InvariantCulture)).Append('|').Append(monsters).AppendLine();
                            count++;
                        }
                    }
                }

                // ecriture dans un fichier temporaire puis remplacement : jamais de fichier a moitie ecrit
                var tmp = SaveFile + ".tmp";
                File.WriteAllText(tmp, sb.ToString(), new UTF8Encoding(false));
                if (File.Exists(SaveFile))
                    File.Replace(tmp, SaveFile, null);
                else
                    File.Move(tmp, SaveFile);

                logger.Info("[Groupes] {0} groupes de monstres enregistres ({1} restaures depuis le demarrage, {2} monstres sans spawn actif retires)", count, m_restored, m_retires);
            }
            catch (Exception ex)
            {
                logger.Error("[Groupes] sauvegarde impossible : {0}", ex);
            }
        }
    }
}
