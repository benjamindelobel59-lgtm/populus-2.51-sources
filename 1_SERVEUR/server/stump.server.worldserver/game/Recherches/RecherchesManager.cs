using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using Stump.Core.Attributes;
using Stump.Core.Reflection;
using Stump.Server.BaseServer.Initialization;
using Stump.Server.WorldServer.Database.Monsters;
using Stump.Server.WorldServer.Game.Achievements;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Monsters;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Maps;
using Stump.Server.WorldServer.Game.Maps.Cells;
using Stump.Server.WorldServer.Game.Maps.Spawns;

namespace Stump.Server.WorldServer.Game.Recherches
{
    /// <summary>
    /// MYSTIC : recherchés (avis de recherche) sans quête. Table nekfeu_recherches (générée par _OUTILS/serveur/recherches/generer_recherches.py).
    /// - un seul exemplaire de chaque recherché, seul dans son groupe, sur une map à monstres de sa zone (tirée au hasard) ;
    /// - tué par des joueurs : chaque personnage du combat gagnant qui a le niveau requis gagne le succès du recherché
    ///   (récompense = jetons, à récupérer dans les succès), puis il réapparaît 4 à 6 h plus tard (heure enregistrée en base) ;
    /// - si les joueurs perdent, il réapparaît quelques minutes après ;
    /// - il n'apparaît plus dans les groupes normaux (monsters_spawns désactivés, groupes sauvegardés filtrés au redémarrage).
    /// </summary>
    public class RecherchesManager : Singleton<RecherchesManager>
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>Délai de réapparition après la mort du recherché (minutes, tiré entre les deux).</summary>
        [Variable(true)]
        public static int RespawnMinMinutes = 240;

        [Variable(true)]
        public static int RespawnMaxMinutes = 360;

        /// <summary>Réapparition quand les joueurs perdent / abandonnent (minutes).</summary>
        [Variable(true)]
        public static int RetryMinutes = 3;

        private static Stump.ORM.Database Database => WorldServer.Instance.DBAccessor.Database;

        private static HashSet<int> m_wantedIds = new HashSet<int>();

        private readonly Dictionary<int, Recherche> m_recherches = new Dictionary<int, Recherche>();
        private readonly object m_lock = new object();
        private readonly Random m_random = new Random();

        public static bool IsWanted(int monsterId) => m_wantedIds.Contains(monsterId);

        private class RechercheRow
        {
            public int MonsterId { get; set; }
            public string Nom { get; set; }
            public string SubAreasCSV { get; set; }
            public int GradeMin { get; set; }
            public int GradeMax { get; set; }
            public int NiveauMin { get; set; }
            public int AchievementId { get; set; }
            public DateTime? ProchainSpawn { get; set; }
        }

        private class Recherche
        {
            public RechercheRow Row;
            public int[] SubAreas;
            public MonsterGroup Group;
            public bool Spawning;
        }

        /// <summary>Liste chargée avant l'apparition des groupes des maps (World = Seventh) : les groupes sauvegardés sont filtrés.</summary>
        [Initialization(InitializationPass.Sixth)]
        public void Initialize()
        {
            try
            {
                foreach (var row in Database.Fetch<RechercheRow>("SELECT MonsterId, Nom, SubAreasCSV, GradeMin, GradeMax, NiveauMin, AchievementId, ProchainSpawn FROM nekfeu_recherches"))
                {
                    m_recherches[row.MonsterId] = new Recherche
                    {
                        Row = row,
                        SubAreas = (row.SubAreasCSV ?? "").Split(',').Select(x => int.TryParse(x.Trim(), out var id) ? id : -1).Where(x => x > 0).ToArray()
                    };
                }
                m_wantedIds = new HashSet<int>(m_recherches.Keys);
            }
            catch (Exception ex)
            {
                logger.Error("[Recherches] table nekfeu_recherches illisible (script recherches.sql passe ?) : {0}", ex.Message);
            }
        }

        [Initialization(InitializationPass.Last)]
        public void Start()
        {
            if (m_recherches.Count == 0)
                return;

            WorldServer.Instance.IOTaskPool.CallPeriodically(60 * 1000, Tick);
            Tick();
            logger.Info("[Recherches] {0} recherches geres (1 par zone, reapparition {1} a {2} min)", m_recherches.Count, RespawnMinMinutes, RespawnMaxMinutes);
        }

        private void Tick()
        {
            var now = DateTime.Now;
            List<Recherche> aFaire;
            lock (m_lock)
            {
                aFaire = m_recherches.Values.Where(r => !r.Spawning && !IsAlive(r) && (r.Row.ProchainSpawn == null || r.Row.ProchainSpawn <= now)).ToList();
            }

            foreach (var r in aFaire)
            {
                try
                {
                    Spawn(r);
                }
                catch (Exception ex)
                {
                    logger.Error("[Recherches] apparition de {0} impossible : {1}", r.Row.Nom, ex);
                }
            }
        }

        /// <summary>Commande .recherches : position de chaque recherche (ou heure de reapparition).</summary>
        public IEnumerable<string> GetEtat()
        {
            List<Recherche> liste;
            lock (m_lock)
                liste = m_recherches.Values.OrderBy(x => x.Row.Nom).ToList();

            foreach (var r in liste)
            {
                var group = r.Group;
                if (IsAlive(r))
                {
                    var map = group.Fight != null ? group.Fight.Map : group.Map;
                    yield return string.Format("{0} (niv. {1}+) : [{2},{3}] map {4}{5}", r.Row.Nom, r.Row.NiveauMin, map.Position.X, map.Position.Y, map.Id,
                        group.Fight != null ? " (en combat)" : "");
                }
                else
                    yield return string.Format("{0} : reapparait {1}", r.Row.Nom,
                        r.Row.ProchainSpawn == null || r.Row.ProchainSpawn <= DateTime.Now ? "bientot" : "a " + r.Row.ProchainSpawn.Value.ToString("HH:mm"));
            }
        }

        #region MYSTIC 19/10 : Chasseur de primes (PNJ du zaap d'Astrub)

        public const int MessageResume = 60003, MessageListe = 60004;
        public const int LignesParPage = 10;

        // Mode : 0 = résumé, 1 = en liberté, 2 = vaincus. Le client ne redessine pas un dialogue quand on lui renvoie le MÊME
        // message : chaque réponse alterne donc entre les messages 60003 et 60004 (même texte « #1 », mêmes réponses).
        class Consultation { public int Mode; public int Page; public bool EnCours; }
        readonly Dictionary<int, Consultation> m_consultations = new Dictionary<int, Consultation>();

        Consultation Etat(Character character)
        {
            lock (m_consultations)
            {
                if (!m_consultations.TryGetValue(character.Id, out var c))
                    m_consultations[character.Id] = c = new Consultation();
                return c;
            }
        }

        List<string> Lignes(bool libres)
        {
            List<Recherche> liste;
            lock (m_lock)
                liste = m_recherches.Values.OrderBy(x => x.Row.Nom).ToList();

            var lignes = new List<string>();
            foreach (var r in liste)
            {
                var vivant = IsAlive(r);
                if (vivant != libres)
                    continue;

                if (vivant)
                {
                    var map = r.Group.Fight != null ? r.Group.Fight.Map : r.Group.Map;
                    var zone = map?.SubArea?.Record?.Name ?? "?";
                    lignes.Add($"• Le recherché {r.Row.Nom} est présent dans sa zone ({zone}), attrape-le !" + (r.Group.Fight != null ? " (en combat)" : ""));
                }
                else
                {
                    var retour = r.Row.ProchainSpawn == null || r.Row.ProchainSpawn <= DateTime.Now ? "retour imminent" : "retour vers " + r.Row.ProchainSpawn.Value.ToString("HH:mm");
                    lignes.Add($"• Le recherché {r.Row.Nom} a déjà été vaincu ({retour}).");
                }
            }
            return lignes;
        }

        /// <summary>
        /// Paramètres (#1) des messages du Chasseur de primes, null pour tout autre message. Appelé AVANT le calcul des réponses
        /// (NpcDialog.ChangeMessage) : une nouvelle discussion (pas déclenchée par une réponse) repart du résumé.
        /// </summary>
        public string[] ParametresDialogue(int messageId, Character character)
        {
            if (messageId != MessageResume && messageId != MessageListe)
                return null;

            var c = Etat(character);
            if (!c.EnCours) { c.Mode = 0; c.Page = 0; }
            c.EnCours = false;

            if (c.Mode == 0)
            {
                var libres = Lignes(true).Count;
                var vaincus = Lignes(false).Count;
                return new[] { $"Salut {character.Name} ! Je tiens le registre des avis de recherche.\n" +
                               $"{libres} recherché(s) rôdent en ce moment dans le monde, {vaincus} ont déjà été vaincus et reviendront plus tard.\n" +
                               "Lequel veux-tu traquer ?" };
            }

            var lignes = Lignes(c.Mode == 1);
            var pages = Math.Max(1, (lignes.Count + LignesParPage - 1) / LignesParPage);
            if (c.Page >= pages) c.Page = 0;
            var titre = (c.Mode == 1 ? "Recherchés en liberté" : "Recherchés déjà vaincus") + (pages > 1 ? $" (page {c.Page + 1}/{pages})" : "") + " :\n";
            var corps = lignes.Count == 0
                ? (c.Mode == 1 ? "Aucun recherché n'est en liberté pour le moment." : "Aucun recherché n'a été vaincu récemment.")
                : string.Join("\n", lignes.Skip(c.Page * LignesParPage).Take(LignesParPage));
            return new[] { titre + corps };
        }

        /// <summary>Réponse choisie (« libres », « vaincus », « suivant », « retour ») : renvoie le message à afficher (alterné).</summary>
        public int Consulter(Character character, string action, int messageActuel)
        {
            var c = Etat(character);
            if (action == "libres") { c.Mode = 1; c.Page = 0; }
            else if (action == "vaincus") { c.Mode = 2; c.Page = 0; }
            else if (action == "suivant") c.Page++;
            else if (action == "retour") { c.Mode = 0; c.Page = 0; }
            c.EnCours = true;
            return messageActuel == MessageResume ? MessageListe : MessageResume;
        }

        /// <summary>Réponse visible dans l'état actuel ?</summary>
        public bool ReponseVisible(Character character, string action)
        {
            var c = Etat(character);
            switch (action)
            {
                case "libres": return c.Mode != 1;
                case "vaincus": return c.Mode != 2;
                case "retour": return c.Mode != 0;
                case "suivant": return c.Mode != 0 && (c.Page + 1) * LignesParPage < Lignes(c.Mode == 1).Count;
                default: return false;
            }
        }

        #endregion

        private static bool IsAlive(Recherche r)
        {
            var group = r.Group;
            if (group == null)
                return false;
            if (group.Fight != null)
                return true;
            var map = group.Map;
            return map != null && map.GetActors<MonsterGroup>().Contains(group);
        }

        // map à monstres normaux (pas donjon, pas de spawn coupé) ; les cases de combat ne sont pas encore calculées sur les maps
        // jamais visitées (CanSpawnMonsters lève alors une exception) : on regarde les placements par GetBlue/RedFightPlacement
        private static bool IsEligible(Map map)
        {
            try
            {
                return map != null && map.Record != null && !map.IsDungeon() && !map.Record.SpawnDisabled &&
                       map.SpawningPools.Any(x => x is ClassicalSpawningPool) &&
                       map.GetBlueFightPlacement().Length > 0 && map.GetRedFightPlacement().Length > 0;
            }
            catch
            {
                return false;
            }
        }

        private void Spawn(Recherche r)
        {
            var maps = r.SubAreas.Select(id => World.Instance.GetSubArea(id)).Where(x => x != null)
                .SelectMany(x => x.Maps).Where(IsEligible).ToList();
            if (maps.Count == 0)
            {
                // zone sans map à monstres : on réessaie plus tard sans remplir le journal
                r.Row.ProchainSpawn = DateTime.Now.AddHours(1);
                logger.Warn("[Recherches] {0} : aucune map a monstres dans la zone {1}", r.Row.Nom, r.Row.SubAreasCSV);
                return;
            }

            MonsterGrade grade;
            lock (m_random)
            {
                var gradeId = m_random.Next(Math.Max(1, r.Row.GradeMin), Math.Max(r.Row.GradeMin, r.Row.GradeMax) + 1);
                grade = MonsterManager.Instance.GetMonsterGrade(r.Row.MonsterId, gradeId) ?? MonsterManager.Instance.GetMonsterGrade(r.Row.MonsterId, 1);
            }
            if (grade == null)
            {
                r.Row.ProchainSpawn = DateTime.Now.AddHours(1);
                logger.Warn("[Recherches] {0} : monstre {1} introuvable", r.Row.Nom, r.Row.MonsterId);
                return;
            }

            Map map;
            lock (m_random)
                map = maps[m_random.Next(maps.Count)];

            r.Spawning = true;
            // zone sans joueur (Area arretee) : ExecuteInContext attendrait l'arrivee d'un joueur -> on place le groupe tout de suite,
            // comme les groupes normaux au demarrage
            Action apparition = () =>
            {
                try
                {
                    var cell = map.GetRandomFreeCell();
                    if (cell == null)
                        return;

                    var group = new MonsterGroup(map.GetNextContextualId(), new ObjectPosition(map, cell, map.GetRandomDirection()));
                    group.AddMonster(new Monster(grade, group));
                    group.ExitFight += OnExitFight;
                    r.Group = group;
                    map.Enter(group);
                    logger.Debug("[Recherches] {0} apparait sur la map {1}", r.Row.Nom, map.Id);
                }
                finally
                {
                    r.Spawning = false;
                }
            };

            if (map.Area.IsRunning)
                map.Area.ExecuteInContext(apparition);
            else
                apparition();
        }

        private void OnExitFight(MonsterGroup group, IFight fight)
        {
            Recherche r;
            lock (m_lock)
                r = m_recherches.Values.FirstOrDefault(x => x.Group == group);
            if (r == null)
                return;

            group.ExitFight -= OnExitFight;
            r.Group = null;

            var gagne = fight.Winners != null && fight.Winners == fight.ChallengersTeam;
            if (!gagne)
            {
                SetNextSpawn(r, DateTime.Now.AddMinutes(RetryMinutes), false);
                return;
            }

            int delai;
            lock (m_random)
                delai = m_random.Next(RespawnMinMinutes, Math.Max(RespawnMinMinutes, RespawnMaxMinutes) + 1);
            SetNextSpawn(r, DateTime.Now.AddMinutes(delai), true);

            var achievement = Singleton<AchievementManager>.Instance.TryGetAchievement((uint)r.Row.AchievementId);
            foreach (var fighter in fight.ChallengersTeam.GetAllFighters<CharacterFighter>().Where(x => !x.HasLeft()))
            {
                var character = fighter.Character;
                if (character == null)
                    continue;

                if (character.Level < r.Row.NiveauMin)
                {
                    character.SendServerMessage(string.Format("Recherché {0} vaincu, mais il faut être niveau {1} pour valider son succès.", r.Row.Nom, r.Row.NiveauMin));
                    continue;
                }

                if (achievement == null || character.Achievement.AchievementIsCompleted((uint)achievement.Id))
                    continue;

                character.Achievement.CompleteAchievement(achievement);
                character.SendServerMessage(string.Format("Recherché {0} capturé ! Succès gagné : récupère ta récompense dans les succès (Avis de recherche).", r.Row.Nom));
            }
        }

        private void SetNextSpawn(Recherche r, DateTime date, bool save)
        {
            r.Row.ProchainSpawn = date;
            if (!save)
                return;

            WorldServer.Instance.IOTaskPool.AddMessage(() =>
            {
                try
                {
                    Database.Execute("UPDATE nekfeu_recherches SET ProchainSpawn=@0 WHERE MonsterId=@1", date, r.Row.MonsterId);
                }
                catch (Exception ex)
                {
                    logger.Error("[Recherches] sauvegarde de {0} impossible : {1}", r.Row.Nom, ex.Message);
                }
            });
        }
    }
}
