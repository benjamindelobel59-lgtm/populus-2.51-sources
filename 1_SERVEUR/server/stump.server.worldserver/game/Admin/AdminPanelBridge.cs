using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using NLog;
using Stump.Core.Attributes;
using Stump.Core.Reflection;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Messages;
using Stump.Server.BaseServer.Initialization;
using Stump.Server.WorldServer.Game.Actors.Look;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Npcs;
using Stump.Server.WorldServer.Game.Items;
using Stump.Server.WorldServer.Game.Maps.Cells;

namespace Stump.Server.WorldServer.Game.Admin
{
    /// <summary>
    /// Pont entre le panel admin web (populus-admin) et le serveur de jeu.
    /// - .admin cree un jeton a usage unique (admin_panel_tokens) que le panel echange contre une session ;
    /// - le panel depose des demandes dans admin_panel_queue (rechargement des PNJ, position du MJ, teleportation,
    ///   apercu d'apparence) que le serveur execute toutes les secondes et dont il ecrit le resultat.
    /// </summary>
    public class AdminPanelBridge : Singleton<AdminPanelBridge>
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        [Variable(true)]
        public static string PanelUrl = "http://127.0.0.1/populus-admin/";

        [Variable(true)]
        public static int PollInterval = 250;

        [Variable(true)]
        public static int TokenLifetimeMinutes = 10;

        [Variable(true)]
        public static int PreviewSeconds = 60;

        private bool m_busy;

        private static Stump.ORM.Database Database => WorldServer.Instance.DBAccessor.Database;

        [Initialization(InitializationPass.Last)]
        public void Initialize()
        {
            Database.Execute(@"CREATE TABLE IF NOT EXISTS admin_panel_tokens (
                Token VARCHAR(64) NOT NULL PRIMARY KEY, AccountId INT NOT NULL, CharacterId INT NOT NULL,
                CharacterName VARCHAR(64) NOT NULL, CreatedAt DATETIME NOT NULL, ExpiresAt DATETIME NOT NULL,
                Used TINYINT NOT NULL DEFAULT 0) DEFAULT CHARSET=utf8");
            Database.Execute(@"CREATE TABLE IF NOT EXISTS admin_panel_queue (
                Id INT NOT NULL AUTO_INCREMENT PRIMARY KEY, Command VARCHAR(32) NOT NULL, Args TEXT NULL,
                CharacterId INT NOT NULL DEFAULT 0, CreatedAt DATETIME NOT NULL, Done TINYINT NOT NULL DEFAULT 0,
                Ok TINYINT NOT NULL DEFAULT 0, Result TEXT NULL, DoneAt DATETIME NULL, KEY (Done)) DEFAULT CHARSET=utf8");

            // anciennes demandes jamais traitees (serveur arrete) : on les abandonne
            Database.Execute("UPDATE admin_panel_queue SET Done=1, Ok=0, Result='serveur redemarre', DoneAt=NOW() WHERE Done=0");

            // NEKFEU : cases modifiees dans le panel, envoyees aux clients comme obstacles (voir Map.LiveCells)
            Database.Execute(@"CREATE TABLE IF NOT EXISTS nekfeu_map_cells_live (MapId INT NOT NULL, CellId SMALLINT NOT NULL,
                Walkable TINYINT NOT NULL, PRIMARY KEY (MapId, CellId)) DEFAULT CHARSET=utf8");
            var live = 0;
            foreach (var row in Database.Fetch<LiveCellRow>("SELECT MapId, CellId, Walkable FROM nekfeu_map_cells_live"))
            {
                var map = World.Instance.GetMap(row.MapId);
                if (map == null)
                    continue;
                map.LiveCells[row.CellId] = row.Walkable != 0;
                live++;
            }
            logger.Info("[Panel admin] {0} case(s) modifiee(s) envoyee(s) aux clients comme obstacles", live);

            WorldServer.Instance.IOTaskPool.CallPeriodically(PollInterval, Poll);
            logger.Info("[Panel admin] pret ({0})", PanelUrl);
        }

        public string CreateToken(Character character)
        {
            var bytes = new byte[24];
            using (var rng = new RNGCryptoServiceProvider())
                rng.GetBytes(bytes);
            var token = string.Concat(bytes.Select(b => b.ToString("x2")));

            Database.Execute("DELETE FROM admin_panel_tokens WHERE ExpiresAt < NOW() - INTERVAL 1 DAY");
            Database.Execute("INSERT INTO admin_panel_tokens (Token, AccountId, CharacterId, CharacterName, CreatedAt, ExpiresAt) VALUES (@0, @1, @2, @3, NOW(), NOW() + INTERVAL @4 MINUTE)",
                token, character.Account.Id, character.Id, character.Name, TokenLifetimeMinutes);
            return token;
        }

        private class LiveCellRow
        {
            public int MapId { get; set; }
            public short CellId { get; set; }
            public int Walkable { get; set; }
        }

        private class MapPanelRow
        {
            public string BlueCellsCSV { get; set; }
            public string RedCellsCSV { get; set; }
            public int TopNeighbourId { get; set; }
            public int BottomNeighbourId { get; set; }
            public int LeftNeighbourId { get; set; }
            public int RightNeighbourId { get; set; }
            public bool SpawnDisabled { get; set; }
            public short? TopNeighbourCellId { get; set; }
            public short? BottomNeighbourCellId { get; set; }
            public short? LeftNeighbourCellId { get; set; }
            public short? RightNeighbourCellId { get; set; }
            public byte[] CompressedCells { get; set; }
        }

        private class QueueEntry
        {
            public int Id { get; set; }
            public string Command { get; set; }
            public string Args { get; set; }
            public int CharacterId { get; set; }
        }

        private void Poll()
        {
            if (m_busy)
                return;
            m_busy = true;
            try
            {
                foreach (var entry in Database.Fetch<QueueEntry>("SELECT Id, Command, Args, CharacterId FROM admin_panel_queue WHERE Done=0 ORDER BY Id LIMIT 20"))
                {
                    Database.Execute("UPDATE admin_panel_queue SET Done=2 WHERE Id=@0", entry.Id);
                    string result;
                    bool ok;
                    try
                    {
                        ok = Execute(entry, out result);
                        if (result == null)
                            continue; // termine plus tard dans le contexte du personnage (Complete)
                    }
                    catch (Exception ex)
                    {
                        ok = false;
                        result = "erreur : " + ex.Message;
                        logger.Error("[Panel admin] {0} : {1}", entry.Command, ex);
                    }
                    Database.Execute("UPDATE admin_panel_queue SET Done=1, Ok=@0, Result=@1, DoneAt=NOW() WHERE Id=@2", ok ? 1 : 0, result, entry.Id);
                }
            }
            catch (Exception ex)
            {
                logger.Error("[Panel admin] lecture des demandes impossible : {0}", ex.Message);
            }
            finally
            {
                m_busy = false;
            }
        }

        /// <summary>Fin d'une demande executee de facon asynchrone (dans le contexte de la zone du personnage).</summary>
        private static void Complete(int id, bool ok, string result)
        {
            try
            {
                Database.Execute("UPDATE admin_panel_queue SET Done=1, Ok=@0, Result=@1, DoneAt=NOW() WHERE Id=@2", ok ? 1 : 0, result, id);
            }
            catch (Exception ex)
            {
                logger.Error("[Panel admin] resultat {0} non ecrit : {1}", id, ex.Message);
            }
        }

        /// <summary>Retire une zone d'apparition du monde et les groupes de ce monstre deja poses dans la zone.</summary>
        private static void RemoveSpawnFromWorld(Database.Monsters.MonsterSpawn spawn)
        {
            var maps = spawn.Map != null ? new[] { spawn.Map } : spawn.SubArea?.Maps.ToArray() ?? new Maps.Map[0];
            if (spawn.Map != null)
                spawn.Map.RemoveMonsterSpawn(spawn);
            else
                spawn.SubArea?.RemoveMonsterSpawn(spawn);
            foreach (var map in maps)
                foreach (var group in map.GetActors<Actors.RolePlay.Monsters.MonsterGroup>().Where(g => g.GetMonsters().Any(x => x.Template.Id == spawn.MonsterId)).ToArray())
                    map.Area.ExecuteInContext(() => map.Leave(group));
        }

        /// <summary>NEKFEU : relit en base les actions des interactifs de la map et les regenere (destinations modifiees dans le panel).</summary>
        private static int ReloadMapInteractives(Maps.Map map)
        {
            var interactives = map.GetInteractiveObjects().ToList();
            if (interactives.Count == 0)
                return 0;
            var ids = string.Join(",", interactives.Select(i => i.Id));
            var links = Database.Fetch<Stump.Server.WorldServer.Database.Interactives.InteractiveSpawnSkills>(
                "SELECT * FROM interactives_spawns_skills WHERE InteractiveSpawnId IN (" + ids + ")");
            var skills = links.Count == 0
                ? new Dictionary<int, Stump.Server.WorldServer.Database.Interactives.InteractiveCustomSkillRecord>()
                : Database.Fetch<Stump.Server.WorldServer.Database.Interactives.InteractiveCustomSkillRecord>(
                    "SELECT * FROM interactives_skills WHERE Id IN (" + string.Join(",", links.Select(l => l.SkillId).Distinct()) + ")").ToDictionary(s => s.Id);
            map.Area.ExecuteInContext(() =>
            {
                foreach (var interactive in interactives)
                {
                    // interactif sans action personnalisee en base : on garde celles de son modele (recolte, zaap...)
                    var records = links.Where(l => l.InteractiveSpawnId == interactive.Id && skills.ContainsKey(l.SkillId))
                        .Select(l => (Stump.Server.WorldServer.Database.Interactives.ISkillRecord)skills[l.SkillId]).ToList();
                    if (records.Count == 0 && interactive.Spawn.CustomSkills.Count == 0)
                        continue;
                    interactive.ReloadSkills(records);
                    map.Refresh(interactive);
                }
            });
            return interactives.Count;
        }

        private static object CharacterInfo(Character c) => new
        {
            id = c.Id,
            name = c.Name,
            level = c.Level,
            breed = (int)c.BreedId,
            sex = c.Sex.ToString(),
            account = c.Account?.Login,
            accountId = c.Account?.Id,
            role = c.UserGroup?.Role.ToString(),
            ip = c.Client?.IP,
            mapId = c.Map?.Id,
            cellId = c.Cell?.Id,
            x = c.Map?.Position.X,
            y = c.Map?.Position.Y,
            subAreaId = c.Map?.SubArea?.Id,
            fighting = c.IsFighting(),
            kamas = c.Kamas,
            oneWindow = c.OneWindow,
        };

        private static bool Execute(QueueEntry entry, out string result)
        {
            var args = (entry.Args ?? "").Split(';');
            var character = entry.CharacterId > 0 ? World.Instance.GetCharacter(entry.CharacterId) : null;

            switch (entry.Command)
            {
                case "reload_npcs":
                    World.Instance.UnSpawnNpcs();
                    NpcManager.Instance.Initialize();
                    if (args.Contains("items"))
                        ItemManager.Instance.ReloadNpcShopItems();
                    World.SpawnNpcs();
                    result = "PNJ recharges";
                    return true;

                case "position":
                    if (character?.Map == null)
                    {
                        result = "personnage hors ligne";
                        return false;
                    }
                    result = string.Join(";", character.Map.Id, character.Cell.Id, (int)character.Direction,
                        character.Map.Position.X, character.Map.Position.Y, character.Map.SubArea?.Id ?? 0);
                    return true;

                case "teleport":
                {
                    if (character == null)
                    {
                        result = "personnage hors ligne";
                        return false;
                    }
                    var map = World.Instance.GetMap(int.Parse(args[0], CultureInfo.InvariantCulture));
                    if (map == null)
                    {
                        result = "map introuvable";
                        return false;
                    }
                    var cellId = args.Length > 1 && int.TryParse(args[1], out var c) ? c : -1;
                    var cell = cellId >= 0 && cellId < map.Cells.Length ? map.GetRandomAdjacentFreeCell(map.Cells[cellId]) ?? map.Cells[cellId] : map.GetFirstFreeCellNearMiddle();
                    character.Area.ExecuteInContext(() => character.Teleport(map, cell));
                    result = "teleporte sur " + map.Id;
                    return true;
                }

                case "preview_look":
                {
                    if (character?.Map == null)
                    {
                        result = "personnage hors ligne";
                        return false;
                    }
                    var template = NpcManager.Instance.GetNpcTemplate(int.Parse(args[0], CultureInfo.InvariantCulture));
                    var lookString = string.Join(";", args.Skip(1));
                    var look = string.IsNullOrWhiteSpace(lookString) ? template?.Look : ActorLook.Parse(lookString);
                    if (template == null || look == null)
                    {
                        result = "PNJ ou apparence invalide";
                        return false;
                    }
                    var map = character.Map;
                    var cell = map.GetRandomAdjacentFreeCell(character.Cell) ?? character.Cell;
                    map.Area.ExecuteInContext(() =>
                    {
                        var npc = map.SpawnNpc(template, new ObjectPosition(map, cell, character.Direction), look);
                        map.Area.CallDelayed(PreviewSeconds * 1000, () =>
                        {
                            if (map.GetActor<Npc>(npc.Id) != null)
                                map.UnSpawnNpc(npc);
                        });
                    });
                    result = "apercu pose a cote de " + character.Name + " pendant " + PreviewSeconds + " s";
                    return true;
                }

                case "cmd":
                {
                    if (character == null)
                    {
                        result = "Ton personnage doit etre connecte en jeu pour lancer une commande.";
                        return false;
                    }
                    var text = (entry.Args ?? "").Trim();
                    if (text.StartsWith("."))
                        text = text.Substring(1);
                    var id = entry.Id;
                    character.Area.ExecuteInContext(() =>
                    {
                        var trigger = new Commands.Trigger.PanelTrigger(text, character);
                        var ok = true;
                        try
                        {
                            WorldServer.Instance.CommandManager.HandleCommand(trigger);
                        }
                        catch (Exception ex)
                        {
                            trigger.ReplyError("erreur : " + ex.Message);
                            ok = false;
                        }
                        Complete(id, ok && !trigger.HadError, trigger.Output.Length == 0 ? "(commande executee, pas de reponse)" : trigger.Output.ToString().TrimEnd());
                    });
                    result = null;
                    return true;
                }

                case "players":
                    result = Newtonsoft.Json.JsonConvert.SerializeObject(World.Instance.GetCharacters().Select(CharacterInfo).ToList());
                    return true;

                case "state":
                    result = Newtonsoft.Json.JsonConvert.SerializeObject(new
                    {
                        online = World.Instance.CharacterCount,
                        me = character == null ? null : CharacterInfo(character),
                        uptime = (DateTime.Now - System.Diagnostics.Process.GetCurrentProcess().StartTime).TotalSeconds,
                        memoryMb = GC.GetTotalMemory(false) / 1048576,
                    });
                    return true;

                case "reload_monster":
                    Actors.RolePlay.Monsters.MonsterManager.Instance.ReloadMonster(int.Parse(args[0], CultureInfo.InvariantCulture));
                    result = "monstre recharge";
                    return true;

                case "item_effects":
                    result = AdminPanelData.ItemEffects(int.Parse(args[0], CultureInfo.InvariantCulture));
                    return true;

                case "item_effects_set":
                    AdminPanelData.SetItemEffects(int.Parse(args[0], CultureInfo.InvariantCulture), string.Join(";", args.Skip(1)));
                    result = "effets enregistres";
                    return true;

                case "reload_item":
                    AdminPanelData.ReloadItem(int.Parse(args[0], CultureInfo.InvariantCulture));
                    result = "objet recharge";
                    return true;

                case "spell_effects":
                    result = AdminPanelData.SpellLevelEffects(int.Parse(args[0], CultureInfo.InvariantCulture));
                    return true;

                case "spell_all_effects":
                {
                    var ids = (entry.Args ?? "").Split(',').Where(x => x.Length > 0).Select(x => int.Parse(x, CultureInfo.InvariantCulture));
                    result = "{" + string.Join(",", ids.Select(id => "\"" + id + "\":" + AdminPanelData.SpellLevelEffects(id))) + "}";
                    return true;
                }

                case "spell_effects_set":
                    AdminPanelData.SetSpellLevelEffects(int.Parse(args[0], CultureInfo.InvariantCulture), args[1] == "1", string.Join(";", args.Skip(2)));
                    result = "effets enregistres";
                    return true;

                case "set_effects":
                    result = AdminPanelData.ItemSetEffects(int.Parse(args[0], CultureInfo.InvariantCulture));
                    return true;

                case "set_effects_set":
                    AdminPanelData.SetItemSetEffects(int.Parse(args[0], CultureInfo.InvariantCulture), string.Join(";", args.Skip(1)));
                    result = "bonus enregistres";
                    return true;

                case "reload_set":
                    AdminPanelData.ReloadItemSet(int.Parse(args[0], CultureInfo.InvariantCulture));
                    result = "panoplie rechargee";
                    return true;

                case "reload_spell":
                    AdminPanelData.ReloadSpell(int.Parse(args[0], CultureInfo.InvariantCulture));
                    result = "sort recharge";
                    return true;

                case "health":
                {
                    var p = System.Diagnostics.Process.GetCurrentProcess();
                    var j = AdminJournal.Instance;
                    result = Newtonsoft.Json.JsonConvert.SerializeObject(new
                    {
                        online = World.Instance.CharacterCount,
                        fights = j.Fights,
                        memoryMb = p.WorkingSet64 / 1048576,
                        threads = p.Threads.Count,
                        uptime = (DateTime.Now - p.StartTime).TotalSeconds,
                        restartIn = j.RestartAt == DateTime.MaxValue ? (double?)null : (j.RestartAt - DateTime.Now).TotalSeconds,
                        restartReason = j.RestartReason,
                        rates = new { xp = Rates.XpRate, drops = Rates.DropsRate, kamas = Rates.KamasRate, job = Rates.JobXpRate },
                    });
                    return true;
                }

                case "restart":
                    AdminJournal.Instance.ScheduleRestart(int.Parse(args[0], CultureInfo.InvariantCulture), string.Join(";", args.Skip(1)));
                    result = "redemarrage programme";
                    return true;

                case "restart_cancel":
                    AdminJournal.Instance.CancelRestart();
                    result = "redemarrage annule";
                    return true;

                case "event_end":
                    AdminJournal.Instance.EndRatesEvent(int.Parse(args[0], CultureInfo.InvariantCulture), "annule");
                    result = "evenement arrete";
                    return true;

                case "decode_effects":
                    result = AdminPanelData.DecodeItemEffects(entry.Args);
                    return true;

                case "give_copy_item":
                    result = AdminPanelData.GiveItemFromCopy(int.Parse(args[0], CultureInfo.InvariantCulture), args[1], int.Parse(args[2], CultureInfo.InvariantCulture));
                    return true;

                case "make_copy":
                    result = AdminPanelData.MakeCopy();
                    return true;

                case "announce":
                    AdminJournal.Announce(entry.Args);
                    result = "annonce envoyee";
                    return true;

                case "map_reload":
                {
                    // args : idMap -> relit placements de combat, voisins, apparition des monstres et déclencheurs
                    var map = World.Instance.GetMap(int.Parse(args[0], CultureInfo.InvariantCulture));
                    if (map == null)
                    {
                        result = "map introuvable";
                        return false;
                    }
                    var row = Database.Fetch<MapPanelRow>("SELECT BlueCellsCSV, RedCellsCSV, TopNeighbourId, BottomNeighbourId, LeftNeighbourId, RightNeighbourId, SpawnDisabled, TopNeighbourCellId, BottomNeighbourCellId, LeftNeighbourCellId, RightNeighbourCellId, CompressedCells FROM world_maps WHERE Id=@0", map.Id).FirstOrDefault();
                    if (row == null)
                    {
                        result = "map absente de la base";
                        return false;
                    }
                    var triggers = 0;
                    map.Area.ExecuteInContext(() =>
                    {
                        map.Record.BlueFightCells = string.IsNullOrEmpty(row.BlueCellsCSV) ? new short[0] : row.BlueCellsCSV.Split(',').Select(short.Parse).ToArray();
                        map.Record.RedFightCells = string.IsNullOrEmpty(row.RedCellsCSV) ? new short[0] : row.RedCellsCSV.Split(',').Select(short.Parse).ToArray();
                        map.Record.TopNeighbourId = row.TopNeighbourId;
                        map.Record.BottomNeighbourId = row.BottomNeighbourId;
                        map.Record.LeftNeighbourId = row.LeftNeighbourId;
                        map.Record.RightNeighbourId = row.RightNeighbourId;
                        map.TopNeighbour = null;
                        map.BottomNeighbour = null;
                        map.LeftNeighbour = null;
                        map.RightNeighbour = null;
                        map.Record.SpawnDisabled = row.SpawnDisabled;
                        map.Record.TopNeighbourCellId = row.TopNeighbourCellId;
                        map.Record.BottomNeighbourCellId = row.BottomNeighbourCellId;
                        map.Record.LeftNeighbourCellId = row.LeftNeighbourCellId;
                        map.Record.RightNeighbourCellId = row.RightNeighbourCellId;
                        // cases (praticable, ligne de vue, passages de bord) : mises à jour sur place, les objets Cell restent les mêmes
                        if (row.CompressedCells != null)
                        {
                            var raw = Stump.Core.IO.ZipHelper.Uncompress(row.CompressedCells);
                            for (int i = 0; i + Stump.Server.WorldServer.Database.World.Cell.StructSize <= raw.Length && i / Stump.Server.WorldServer.Database.World.Cell.StructSize < map.Cells.Length; i += Stump.Server.WorldServer.Database.World.Cell.StructSize)
                            {
                                var cell = map.Cells[i / Stump.Server.WorldServer.Database.World.Cell.StructSize];
                                cell.LosMov = raw[i + 4];
                                cell.MapChangeData = raw[i + 5];
                            }
                        }
                        map.ReloadFightPlacementsFromRecord();
                    });
                    triggers = Maps.Cells.Triggers.CellTriggerManager.Instance.ReloadMapTriggers(map);
                    // NEKFEU : actions des interactifs (portes, escaliers...) relues en base ; ReloadMapTriggers a aussi efface
                    // leurs passages « marcher dessus », que la regeneration des actions remet
                    var interactivesReloaded = ReloadMapInteractives(map);
                    // NEKFEU : cases modifiees -> obstacles envoyes tout de suite aux joueurs de la map (sans relancer le client)
                    var liveRows = Database.Fetch<LiveCellRow>("SELECT MapId, CellId, Walkable FROM nekfeu_map_cells_live WHERE MapId=@0", map.Id);
                    map.Area.ExecuteInContext(() =>
                    {
                        map.LiveCells.Clear();
                        foreach (var liveRow in liveRows)
                            map.LiveCells[liveRow.CellId] = liveRow.Walkable != 0;
                        if (map.LiveCells.Count > 0)
                            map.Clients.Send(new MapObstacleUpdateMessage(map.GetLiveCellObstacles()));
                    });
                    result = "map rechargee (" + triggers + " declencheur(s), " + interactivesReloaded + " interactif(s))";
                    return true;
                }

                case "map_actors":
                {
                    var map = World.Instance.GetMap(int.Parse(args[0], CultureInfo.InvariantCulture));
                    if (map == null)
                    {
                        result = "map introuvable";
                        return false;
                    }
                    result = Newtonsoft.Json.JsonConvert.SerializeObject(new
                    {
                        players = map.GetAllCharacters().Select(c => new { id = c.Id, name = c.Name, cell = c.Cell?.Id }).ToList(),
                        groups = map.GetActors<Actors.RolePlay.Monsters.MonsterGroup>().Select(g => new
                        {
                            cell = g.Cell?.Id,
                            monsters = g.GetMonsters().Select(m => new { id = m.Template.Id, name = m.Template.Name, level = m.Grade.Level }).ToList(),
                        }).ToList(),
                        blue = map.GetBlueFightPlacement().Select(c => c.Id).ToList(),
                        red = map.GetRedFightPlacement().Select(c => c.Id).ToList(),
                        keepsRecordPlacements = map.KeepOriginalPlacements(),
                    });
                    return true;
                }

                case "map_mobs":
                {
                    var map = character?.Map;
                    if (map == null)
                    {
                        result = "personnage hors ligne";
                        return false;
                    }
                    result = Newtonsoft.Json.JsonConvert.SerializeObject(new
                    {
                        mapId = map.Id,
                        subAreaId = map.SubArea?.Id,
                        groups = map.GetActors<Actors.RolePlay.Monsters.MonsterGroup>().Select(g => new
                        {
                            id = g.Id,
                            cell = g.Cell?.Id,
                            monsters = g.GetMonsters().Select(m => new { id = m.Template.Id, name = m.Template.Name, grade = m.Grade.GradeId, level = m.Grade.Level }).ToList(),
                        }).ToList(),
                    });
                    return true;
                }

                case "mob_zone":
                {
                    // args : idSpawn ; 1 = actif / 0 = desactive / -1 = supprime en base (deja fait par le panel)
                    var spawnId = int.Parse(args[0], CultureInfo.InvariantCulture);
                    var mode = int.Parse(args[1], CultureInfo.InvariantCulture);
                    var manager = Actors.RolePlay.Monsters.MonsterManager.Instance;
                    var old = manager.GetMonsterSpawns().FirstOrDefault(x => x.Id == spawnId);
                    if (old != null)
                        RemoveSpawnFromWorld(old);
                    var spawn = manager.LoadMonsterSpawn(spawnId);
                    if (spawn != null && !spawn.IsDisabled)
                    {
                        if (spawn.Map != null)
                            spawn.Map.AddMonsterSpawn(spawn);
                        else
                            spawn.SubArea?.AddMonsterSpawn(spawn);
                        foreach (var m in (spawn.Map != null ? new[] { spawn.Map } : spawn.SubArea?.Maps.ToArray() ?? new Maps.Map[0]).Where(m => m.MonsterSpawnsCount > 0))
                            m.EnableClassicalMonsterSpawns();
                    }
                    result = spawn == null ? "zone retiree" : spawn.IsDisabled ? "zone desactivee" : "zone active";
                    return true;
                }

                default:
                    result = "commande inconnue";
                    return false;
            }
        }
    }
}
