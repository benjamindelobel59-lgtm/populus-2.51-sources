using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using NLog;
using Stump.Core.Reflection;
using Stump.DofusProtocol.Enums;
using Stump.Server.BaseServer.Initialization;
using Stump.Server.WorldServer.Database.Characters;
using Stump.Server.WorldServer.Database.Items;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Monsters;
using Stump.Server.WorldServer.Game.Effects;
using Stump.Server.WorldServer.Game.Effects.Instances;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Fights.Results;
using Stump.Server.WorldServer.Game.Items;

namespace Stump.Server.WorldServer.Game.Heroique
{
    /// <summary>
    /// MYSTIC : butin héroïque (comme le 2.51 officiel). Un joueur mort en PvM perd tout son inventaire et ses kamas, répartis au hasard
    /// entre tous les groupes de monstres de la sous-zone où il est mort. Un groupe qui porte du butin affiche un coffre
    /// (HasHardcoreDrop) ; les joueurs qui le battent récupèrent ces objets, avec leurs jets d'origine. Un admin qui ressuscite le mort
    /// (.resu) lui rend ce que les monstres portent encore. Table mystic_butin (survit aux redémarrages).
    /// </summary>
    public class ButinHeroique : Singleton<ButinHeroique>
    {
        static readonly Logger logger = LogManager.GetCurrentClassLogger();

        static Stump.ORM.Database Database => WorldServer.Instance.DBAccessor.Database;

        public class Objet
        {
            public long Id { get; set; }
            public int MapId { get; set; }
            public long GroupeTicks { get; set; }
            public int MortId { get; set; }
            public string Mort { get; set; }
            public int TemplateId { get; set; } // 0 = kamas
            public int Stack { get; set; }
            public byte[] Effets { get; set; }
            public long Kamas { get; set; }
        }

        readonly object m_verrou = new object();
        readonly Dictionary<MonsterGroup, List<Objet>> m_parGroupe = new Dictionary<MonsterGroup, List<Objet>>();
        readonly List<Objet> m_enAttente = new List<Objet>(); // chargés depuis la base, pas encore rattachés à un groupe
        long m_prochainId;

        [Initialization(InitializationPass.Last)]
        public void Initialize()
        {
            Database.Execute(@"CREATE TABLE IF NOT EXISTS mystic_butin (Id BIGINT NOT NULL PRIMARY KEY, MapId INT NOT NULL, GroupeTicks BIGINT NOT NULL,
                MortId INT NOT NULL, Mort VARCHAR(50) NOT NULL, TemplateId INT NOT NULL, Stack INT NOT NULL, Effets LONGBLOB NULL,
                Kamas BIGINT NOT NULL DEFAULT 0, Date DATETIME NOT NULL, KEY (MapId), KEY (MortId)) DEFAULT CHARSET=utf8");

            m_enAttente.AddRange(Database.Fetch<Objet>("SELECT Id, MapId, GroupeTicks, MortId, Mort, TemplateId, Stack, Effets, Kamas FROM mystic_butin"));
            m_prochainId = Math.Max(Database.ExecuteScalar<long>("SELECT IFNULL(MAX(Id), 0) FROM mystic_butin"), DateTime.Now.Ticks / 10000) + 1;

            logger.Info("[Héroïque] {0} objet(s) de butin portés par des groupes de monstres", m_enAttente.Count);
        }

        long NouvelId() => System.Threading.Interlocked.Increment(ref m_prochainId);

        #region Coffre sur les groupes

        /// <summary>Vrai si le groupe porte du butin héroïque (coffre affiché au-dessus du groupe).</summary>
        public bool PorteButin(MonsterGroup groupe)
        {
            if (!HeroicManager.Enabled || groupe == null)
                return false;

            lock (m_verrou)
            {
                Rattacher(groupe);
                return m_parGroupe.TryGetValue(groupe, out var liste) && liste.Count > 0;
            }
        }

        /// <summary>Butin rechargé après un redémarrage : rattaché au groupe d'origine (même carte, même date d'apparition), sinon à un groupe de la même carte.</summary>
        void Rattacher(MonsterGroup groupe)
        {
            if (m_enAttente.Count == 0 || groupe.Map == null)
                return;

            var ticks = groupe.CreationDate.Ticks;
            var pourCeGroupe = m_enAttente.Where(x => x.MapId == groupe.Map.Id && x.GroupeTicks == ticks).ToList();
            if (pourCeGroupe.Count == 0)
            {
                // groupe d'origine disparu (carte rechargée, date modifiée...) : le premier groupe de la carte le reprend
                var tickOrigine = m_enAttente.Where(x => x.MapId == groupe.Map.Id).Select(x => x.GroupeTicks).FirstOrDefault();
                if (tickOrigine == 0 || groupe.Map.GetActors<MonsterGroup>().Any(g => g != groupe && g.CreationDate.Ticks == tickOrigine))
                    return;
                pourCeGroupe = m_enAttente.Where(x => x.MapId == groupe.Map.Id && x.GroupeTicks == tickOrigine).ToList();
            }

            foreach (var o in pourCeGroupe)
            {
                m_enAttente.Remove(o);
                Ajouter(groupe, o, false);
            }
        }

        void Ajouter(MonsterGroup groupe, Objet o, bool nouveau)
        {
            if (!m_parGroupe.TryGetValue(groupe, out var liste))
                m_parGroupe[groupe] = liste = new List<Objet>();
            liste.Add(o);

            var mapId = groupe.Map?.Id ?? o.MapId;
            var ticks = groupe.CreationDate.Ticks;
            if (!nouveau && o.MapId == mapId && o.GroupeTicks == ticks)
                return;

            o.MapId = mapId;
            o.GroupeTicks = ticks;
            var copie = o;
            WorldServer.Instance.IOTaskPool.AddMessage(() =>
            {
                if (nouveau)
                    Database.Execute(@"INSERT INTO mystic_butin (Id, MapId, GroupeTicks, MortId, Mort, TemplateId, Stack, Effets, Kamas, Date)
                        VALUES (@0, @1, @2, @3, @4, @5, @6, @7, @8, NOW())", copie.Id, copie.MapId, copie.GroupeTicks, copie.MortId, copie.Mort,
                        copie.TemplateId, copie.Stack, copie.Effets, copie.Kamas);
                else
                    Database.Execute("UPDATE mystic_butin SET MapId = @1, GroupeTicks = @2 WHERE Id = @0", copie.Id, copie.MapId, copie.GroupeTicks);
            });
        }

        static void Rafraichir(MonsterGroup groupe)
        {
            var carte = groupe?.Map;
            if (carte == null || groupe.IsDisposed)
                return;
            carte.Area.ExecuteInContext(() =>
            {
                if (groupe.Map == carte && !groupe.IsDisposed)
                    carte.Refresh(groupe);
            });
        }

        #endregion

        #region Mort en PvM : le butin part aux monstres de la sous-zone

        /// <summary>Retourne le résumé du butin laissé (journal des morts).</summary>
        public string DonnerAuxMonstres(Character mort, IFight combat)
        {
            var sousZone = mort.Map?.SubArea ?? mort.SubArea;
            var groupes = new List<MonsterGroup>();
            if (sousZone != null)
                groupes.AddRange(sousZone.Maps.SelectMany(m => m.GetActors<MonsterGroup>()).Where(g => !g.IsDisposed));

            // le groupe vainqueur (encore en combat, il revient sur la carte ensuite)
            var vainqueur = combat?.Winners?.Fighters.OfType<MonsterFighter>().Select(x => x.Monster.Group).FirstOrDefault(g => g != null && !g.IsDisposed);
            if (vainqueur != null && !groupes.Contains(vainqueur))
                groupes.Add(vainqueur);

            if (groupes.Count == 0)
            {
                logger.Warn("[Héroïque] aucun groupe de monstres pour recevoir le butin de {0} : il reste sur sa tombe", mort.Name);
                return "aucun groupe de monstres : butin resté sur la tombe";
            }

            var rnd = new Random();
            var touches = new HashSet<MonsterGroup>();
            var nb = 0;
            long kamas = 0;

            lock (m_verrou)
            {
                foreach (var objet in mort.Inventory.ToArray())
                {
                    try
                    {
                        var o = new Objet
                        {
                            Id = NouvelId(),
                            MortId = mort.Id,
                            Mort = mort.Name,
                            TemplateId = objet.Template.Id,
                            Stack = (int)objet.Stack,
                            Effets = EffectManager.Instance.SerializeEffects(objet.Effects)
                        };
                        mort.Inventory.RemoveItem(objet, true, false);
                        var groupe = groupes[rnd.Next(groupes.Count)];
                        Ajouter(groupe, o, true);
                        touches.Add(groupe);
                        nb++;
                    }
                    catch (Exception ex)
                    {
                        logger.Error("[Héroïque] butin de {0} : objet {1} : {2}", mort.Name, objet.Guid, ex.Message);
                    }
                }

                kamas = (long)mort.Inventory.Kamas;
                if (kamas > 0)
                {
                    mort.Inventory.SetKamas(0);
                    var part = kamas / groupes.Count;
                    var reste = kamas - part * groupes.Count;
                    for (var i = 0; i < groupes.Count; i++)
                    {
                        var montant = part + (i == 0 ? reste : 0);
                        if (montant <= 0)
                            continue;
                        Ajouter(groupes[i], new Objet { Id = NouvelId(), MortId = mort.Id, Mort = mort.Name, TemplateId = 0, Stack = 0, Kamas = montant }, true);
                        touches.Add(groupes[i]);
                    }
                }
            }

            mort.RefreshActor();
            foreach (var g in touches)
                Rafraichir(g);

            logger.Info("[Héroïque] butin de {0} : {1} objet(s) et {2} kamas répartis sur {3} groupe(s) de monstres de la sous-zone",
                mort.Name, nb, kamas, touches.Count);
            return $"{nb} objet(s) et {kamas} kamas répartis sur {touches.Count} groupe(s) de monstres de la sous-zone";
        }

        #endregion

        #region Victoire contre un groupe qui porte un coffre

        /// <summary>Appelé à la fin d'un combat PvM gagné par les joueurs : le butin du groupe est réparti entre les vainqueurs.</summary>
        public void RecupererButin(IFight combat, List<IFightResult> resultats)
        {
            if (!HeroicManager.Enabled || combat?.Winners == null)
                return;

            var groupe = combat.DefendersTeam.Fighters.OfType<MonsterFighter>().Select(x => x.Monster.Group).FirstOrDefault(g => g != null)
                         ?? combat.ChallengersTeam.Fighters.OfType<MonsterFighter>().Select(x => x.Monster.Group).FirstOrDefault(g => g != null);
            if (groupe == null)
                return;

            var gagnants = resultats.OfType<FightPlayerResult>().Where(r => r.Fighter.Team == combat.Winners && r.Character != null).ToList();
            if (gagnants.Count == 0)
                return;

            List<Objet> butin;
            lock (m_verrou)
            {
                Rattacher(groupe);
                if (!m_parGroupe.TryGetValue(groupe, out butin) || butin.Count == 0)
                    return;
                m_parGroupe.Remove(groupe);
            }

            var rnd = new Random();
            var parGagnant = gagnants.ToDictionary(x => x, x => new List<string>());
            foreach (var o in butin)
            {
                var r = gagnants[rnd.Next(gagnants.Count)];
                try
                {
                    if (o.TemplateId == 0)
                    {
                        // kamas : répartis entre tous les vainqueurs, ajoutés par l'écran de fin de combat
                        var part = o.Kamas / gagnants.Count;
                        foreach (var g in gagnants)
                        {
                            g.Loot.Kamas += part;
                            parGagnant[g].Add($"{part} kamas");
                        }
                        continue;
                    }

                    var modele = ItemManager.Instance.TryGetTemplate(o.TemplateId);
                    if (modele == null)
                        continue;
                    var effets = o.Effets != null ? EffectManager.Instance.DeserializeEffects(o.Effets) : new List<EffectBase>();
                    var objet = ItemManager.Instance.CreatePlayerItem(r.Character, modele, o.Stack, effets);
                    r.Character.Inventory.AddItem(objet);
                    // affichage seul dans l'écran de fin de combat (l'objet exact est déjà dans l'inventaire)
                    r.Loot.AddItem(new DroppedItem(o.TemplateId, (uint)o.Stack) { IgnoreGeneration = true });
                    parGagnant[r].Add($"{modele.Name} x{o.Stack}");
                }
                catch (Exception ex)
                {
                    logger.Error("[Héroïque] récupération du butin {0} : {1}", o.Id, ex.Message);
                }
            }

            var ids = string.Join(",", butin.Select(x => x.Id));
            WorldServer.Instance.IOTaskPool.AddMessage(() => Database.Execute($"DELETE FROM mystic_butin WHERE Id IN ({ids})"));

            var morts = string.Join(", ", butin.Select(x => x.Mort).Distinct());
            foreach (var g in parGagnant.Where(x => x.Value.Count > 0))
                g.Key.Character.SendServerMessage($"Vous récupérez dans le coffre des monstres (butin de {morts}) : {string.Join(", ", g.Value.Take(15))}" +
                    (g.Value.Count > 15 ? $" et {g.Value.Count - 15} autre(s)" : "") + ".", System.Drawing.Color.Gold);

            logger.Info("[Héroïque] coffre de {0} récupéré par {1}", morts, string.Join(", ", gagnants.Select(x => x.Character.Name)));
        }

        #endregion

        #region Résurrection : rendre ce que les monstres portent encore

        /// <summary>Rend au personnage ressuscité les objets et kamas encore portés par les monstres. Retourne (objets, kamas) rendus.</summary>
        public Tuple<int, long> RendreAuMort(CharacterRecord record)
        {
            var rendus = new List<Objet>();
            var groupesTouches = new List<MonsterGroup>();
            lock (m_verrou)
            {
                foreach (var pair in m_parGroupe.ToArray())
                {
                    var siens = pair.Value.Where(x => x.MortId == record.Id).ToList();
                    if (siens.Count == 0)
                        continue;
                    rendus.AddRange(siens);
                    pair.Value.RemoveAll(x => x.MortId == record.Id);
                    groupesTouches.Add(pair.Key);
                }

                rendus.AddRange(m_enAttente.Where(x => x.MortId == record.Id));
                m_enAttente.RemoveAll(x => x.MortId == record.Id);
            }

            if (rendus.Count == 0)
                return Tuple.Create(0, 0L);

            var enLigne = World.Instance.GetCharacter(record.Id);
            var nbObjets = 0;
            long kamas = 0;
            foreach (var o in rendus)
            {
                if (o.TemplateId == 0)
                {
                    kamas += o.Kamas;
                    continue;
                }

                var modele = ItemManager.Instance.TryGetTemplate(o.TemplateId);
                if (modele == null)
                    continue;
                var effets = o.Effets != null ? EffectManager.Instance.DeserializeEffects(o.Effets) : new List<EffectBase>();
                if (enLigne != null)
                    enLigne.Inventory.AddItem(ItemManager.Instance.CreatePlayerItem(enLigne, modele, o.Stack, effets));
                else
                    Database.Insert(new PlayerItemRecord
                    {
                        Id = PlayerItemRecord.PopNextId(),
                        OwnerId = record.Id,
                        Template = modele,
                        Stack = (uint)o.Stack,
                        Position = CharacterInventoryPositionEnum.INVENTORY_POSITION_NOT_EQUIPED,
                        Effects = effets
                    });
                nbObjets++;
            }

            if (kamas > 0)
            {
                if (enLigne != null)
                    enLigne.Inventory.AddKamas((ulong)kamas);
                else
                    Database.Execute("UPDATE characters SET Kamas = Kamas + @0 WHERE Id = @1", kamas, record.Id);
            }

            Database.Execute($"DELETE FROM mystic_butin WHERE Id IN ({string.Join(",", rendus.Select(x => x.Id))})");
            foreach (var g in groupesTouches)
                Rafraichir(g);

            return Tuple.Create(nbObjets, kamas);
        }

        #endregion
    }
}
