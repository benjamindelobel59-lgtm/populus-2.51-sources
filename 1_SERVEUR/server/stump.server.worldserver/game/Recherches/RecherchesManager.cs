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

        // PADGREF 24/10 (demande utilisateur : « un bouton par recherché, sur plusieurs pages, au lieu de tout afficher dans le dialogue,
        // et payer 50 000 kamas pour savoir s'il est là ») : une réponse par recherché (client : Npcs.d2o PNJ 10062, réponses 60100+,
        // SQL 156 : action « voir:<MonsterId> »), 10 par page (« suivant » / « retour ») ; le renseignement dit seulement s'il est en
        // liberté ou non (choix utilisateur), contre 50 000 kamas.
        public const int MessageResume = 60003, MessageListe = 60004;
        public const int LignesParPage = 10;
        public const int PrixRenseignement = 50000;

        // Le client ne redessine pas un dialogue quand on lui renvoie le MÊME message : chaque réponse alterne entre 60003 et 60004.
        class Consultation { public int Page; public string Resultat; public bool EnCours; }
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

        static readonly StringComparer s_ordreNoms = StringComparer.Create(new System.Globalization.CultureInfo("fr-FR"), true);

        List<Recherche> Tous()
        {
            lock (m_lock)
                return m_recherches.Values.OrderBy(x => x.Row.Nom, s_ordreNoms).ThenBy(x => x.Row.MonsterId).ToList();
        }

        int NombrePages() => Math.Max(1, (Tous().Count + LignesParPage - 1) / LignesParPage);

        /// <summary>
        /// Paramètres (#1) des messages du Chasseur de primes, null pour tout autre message. Appelé AVANT le calcul des réponses
        /// (NpcDialog.ChangeMessage) : une nouvelle discussion (pas déclenchée par une réponse) repart de la première page.
        /// </summary>
        public string[] ParametresDialogue(int messageId, Character character)
        {
            if (messageId != MessageResume && messageId != MessageListe)
                return null;

            var c = Etat(character);
            if (!c.EnCours) { c.Page = 0; c.Resultat = null; }
            c.EnCours = false;

            var pages = NombrePages();
            if (c.Page >= pages) c.Page = 0;
            var entete = c.Resultat ?? $"Salut {character.Name} ! Je tiens le registre des avis de recherche.";
            return new[] { entete + "\n\n" + $"Choisis un recherché : pour {PrixRenseignement:N0} kamas, je te dis s'il est en liberté en ce moment." +
                           (pages > 1 ? $"\n(page {c.Page + 1}/{pages})" : "") };
        }

        /// <summary>Réponse choisie (« voir:<MonsterId> », « suivant », « retour ») : renvoie le message à afficher (alterné).</summary>
        public int Consulter(Character character, string action, int messageActuel)
        {
            var c = Etat(character);
            c.Resultat = null;
            if (action.StartsWith("voir:") && int.TryParse(action.Substring(5), out var monstre))
            {
                Recherche r;
                lock (m_lock)
                    m_recherches.TryGetValue(monstre, out r);
                if (r == null)
                    c.Resultat = "Je n'ai pas d'avis de recherche pour celui-là.";
                else if (character.Kamas < PrixRenseignement)
                    c.Resultat = $"Ce renseignement coûte {PrixRenseignement:N0} kamas, et tu ne les as pas.";
                else
                {
                    character.Inventory.SubKamas(PrixRenseignement);
                    c.Resultat = IsAlive(r)
                        ? $"Le recherché {r.Row.Nom} est en liberté en ce moment : à toi de le trouver !"
                        : $"Le recherché {r.Row.Nom} n'est pas en liberté pour le moment : il a déjà été vaincu.";
                }
            }
            else if (action == "suivant") c.Page = (c.Page + 1) % NombrePages();
            else c.Page = 0;      // « retour » (et anciennes réponses « libres » / « vaincus »)
            c.EnCours = true;
            return messageActuel == MessageResume ? MessageListe : MessageResume;
        }

        /// <summary>Réponse visible dans l'état actuel ? (un recherché : seulement sur sa page)</summary>
        public bool ReponseVisible(Character character, string action)
        {
            var c = Etat(character);
            if (action.StartsWith("voir:") && int.TryParse(action.Substring(5), out var monstre))
            {
                var rang = Tous().FindIndex(x => x.Row.MonsterId == monstre);
                return rang >= c.Page * LignesParPage && rang < (c.Page + 1) * LignesParPage;
            }
            switch (action)
            {
                case "suivant": return (c.Page + 1) * LignesParPage < Tous().Count;
                case "retour": return c.Page > 0;
                default: return false;   // « libres » / « vaincus » : remplacées par un bouton par recherché
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

            foreach (var fighter in fight.ChallengersTeam.GetAllFighters<CharacterFighter>().Where(x => !x.HasLeft()))
                DonnerSucces(fighter.Character, r);
        }

        /// <summary>
        /// PADGREF 24/10 (demande utilisateur : « les succès recherchés doivent aussi être débloqués si on les tue après le combat de leur
        /// chasse légendaire ») : appelé par ChasseManager quand le gardien d'une chasse légendaire est un recherché et qu'il est vaincu.
        /// Mêmes règles que le recherché en zone (niveau minimum, succès une seule fois) ; ne touche pas à son apparition en zone.
        /// </summary>
        public void ValiderSucces(Character character, int monsterId)
        {
            Recherche r;
            lock (m_lock)
                m_recherches.TryGetValue(monsterId, out r);
            if (r != null)
                DonnerSucces(character, r);
        }

        private void DonnerSucces(Character character, Recherche r)
        {
            if (character == null)
                return;

            if (character.Level < r.Row.NiveauMin)
            {
                character.SendServerMessage(string.Format("Recherché {0} vaincu, mais il faut être niveau {1} pour valider son succès.", r.Row.Nom, r.Row.NiveauMin));
                return;
            }

            var achievement = Singleton<AchievementManager>.Instance.TryGetAchievement((uint)r.Row.AchievementId);
            if (achievement == null || character.Achievement.AchievementIsCompleted((uint)achievement.Id))
                return;

            character.Achievement.CompleteAchievement(achievement);
            character.SendServerMessage(string.Format("Recherché {0} capturé ! Succès gagné : récupère ta récompense dans les succès (Avis de recherche).", r.Row.Nom));
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
