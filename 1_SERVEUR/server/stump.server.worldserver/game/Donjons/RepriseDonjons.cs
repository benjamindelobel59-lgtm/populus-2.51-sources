using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using NLog;
using Stump.Core.Reflection;
using Stump.Server.BaseServer.Initialization;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Maps;

namespace Stump.Server.WorldServer.Game.Donjons
{
    /// <summary>
    /// MYSTIC 17/10 : « Reprendre le donjon où vous l'avez quitté » (réponse officielle des gardiens). Chaque personnage garde la
    /// dernière salle de donjon atteinte (table nekfeu_reprise_donjon) ; s'il meurt, boit une potion ou se déconnecte, il y revient
    /// gratuitement en reparlant au gardien. La sauvegarde est effacée quand le boss est battu (sortie du donjon) et remplacée à
    /// chaque nouvelle entrée (1re salle). Choix de l'utilisateur : gratuit, dernière salle atteinte, valable jusqu'à la fin du
    /// donjon, chaque personnage la sienne.
    /// </summary>
    public class RepriseDonjons : Singleton<RepriseDonjons>
    {
        static readonly Logger logger = LogManager.GetCurrentClassLogger();

        static Stump.ORM.Database Database => WorldServer.Instance.DBAccessor.Database;

        // carte de salle -> donjon (donjons qui ont une entrée déclarée)
        readonly Dictionary<int, int> m_donjonParCarte = new Dictionary<int, int>();

        // MYSTIC 18/10 : salles de sortie (dungeons_exits.EndMapId) -> donjon (ou -1 si la salle n'est dans aucun donjon déclaré)
        readonly Dictionary<int, int> m_sortieDe = new Dictionary<int, int>();

        // (personnage, donjon) -> (carte, case)
        readonly ConcurrentDictionary<(int, int), (int carte, short cellule)> m_sauvegardes = new ConcurrentDictionary<(int, int), (int carte, short cellule)>();

        public class LigneDonjon
        {
            public int Id { get; set; }
            public string MapsIdsCSV { get; set; }
            public int EntranceMapId { get; set; }
        }

        public class LigneSauvegarde
        {
            public int CharacterId { get; set; }
            public int DungeonId { get; set; }
            public int MapId { get; set; }
            public short CellId { get; set; }
        }

        [Initialization(InitializationPass.Last)]
        public void Initialize()
        {
            Database.Execute(@"CREATE TABLE IF NOT EXISTS nekfeu_reprise_donjon (CharacterId INT NOT NULL, DungeonId INT NOT NULL,
                MapId INT NOT NULL, CellId SMALLINT NOT NULL, MajAt DATETIME NOT NULL, PRIMARY KEY (CharacterId, DungeonId)) DEFAULT CHARSET=utf8");

            var avecEntree = new HashSet<int>(Database.Fetch<int>("SELECT DISTINCT DungeonId FROM dungeons_entries"));
            foreach (var d in Database.Fetch<LigneDonjon>("SELECT Id, MapsIdsCSV, EntranceMapId FROM dungeons"))
            {
                if (!avecEntree.Contains(d.Id) || string.IsNullOrEmpty(d.MapsIdsCSV))
                    continue;

                foreach (var morceau in d.MapsIdsCSV.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    // salles de sortie copiées (99000000x) et carte d'entrée exclues
                    if (int.TryParse(morceau.Trim(), out var carte) && carte < 990000000 && carte != d.EntranceMapId && !m_donjonParCarte.ContainsKey(carte))
                        m_donjonParCarte[carte] = d.Id;
                }
            }

            // MYSTIC 18/10 : les salles de sortie étaient listées avec les salles du donjon : on « reprenait » le donjon dans la salle
            // de sortie. Elles ne sont plus des salles à reprendre ; y arriver = donjon terminé (sauvegarde effacée).
            foreach (var sortie in Database.Fetch<int>("SELECT DISTINCT EndMapId FROM dungeons_exits"))
            {
                if (m_donjonParCarte.TryGetValue(sortie, out var donjonSortie))
                {
                    m_sortieDe[sortie] = donjonSortie;
                    m_donjonParCarte.Remove(sortie);
                }
                else
                    m_sortieDe[sortie] = -1;
            }

            foreach (var s in Database.Fetch<LigneSauvegarde>("SELECT CharacterId, DungeonId, MapId, CellId FROM nekfeu_reprise_donjon"))
                m_sauvegardes[(s.CharacterId, s.DungeonId)] = (s.MapId, s.CellId);

            logger.Info("[Reprise donjon] {0} salles de donjon, {1} salles de sortie, {2} sauvegardes", m_donjonParCarte.Count, m_sortieDe.Count, m_sauvegardes.Count);
        }

        public int? DonjonDe(int carte) => m_donjonParCarte.TryGetValue(carte, out var d) ? d : (int?)null;

        /// <summary>MYSTIC 18/10 : salle de sortie de donjon (après le boss).</summary>
        public bool EstSalleDeSortie(int carte) => m_sortieDe.ContainsKey(carte);

        /// <summary>Appelé à chaque arrivée d'un personnage sur une carte.</summary>
        public void SurArrivee(Character perso, Map carte)
        {
            if (perso == null || carte == null)
                return;

            if (m_sortieDe.TryGetValue(carte.Id, out var termine))
            {
                if (termine >= 0)
                    Effacer(perso, termine);
                return;
            }

            var donjon = DonjonDe(carte.Id);
            if (donjon == null)
                return;

            var cellule = perso.Cell?.Id ?? (short)-1;
            var id = perso.Id;
            m_sauvegardes[(id, donjon.Value)] = (carte.Id, cellule);
            WorldServer.Instance.IOTaskPool.AddMessage(() =>
                Database.Execute("REPLACE INTO nekfeu_reprise_donjon (CharacterId, DungeonId, MapId, CellId, MajAt) VALUES (@0, @1, @2, @3, NOW())",
                    id, donjon.Value, carte.Id, cellule));
        }

        public (int carte, short cellule)? Obtenir(Character perso, int donjon)
        {
            if (m_sauvegardes.TryGetValue((perso.Id, donjon), out var s))
                return s;
            return null;
        }

        /// <summary>Donjon terminé (boss battu, sortie) : plus de reprise.</summary>
        public void Effacer(Character perso, int donjon)
        {
            if (!m_sauvegardes.TryRemove((perso.Id, donjon), out _))
                return;

            var id = perso.Id;
            WorldServer.Instance.IOTaskPool.AddMessage(() =>
                Database.Execute("DELETE FROM nekfeu_reprise_donjon WHERE CharacterId = @0 AND DungeonId = @1", id, donjon));
        }
    }
}

namespace Stump.Server.WorldServer.Game.Donjons
{
    /// <summary>
    /// MYSTIC 18/10 : premier combat contre un boss de donjon (demande utilisateur). Si le personnage QUI LANCE le combat n'a
    /// jamais affronté ce boss, les monstres sont posés dans l'ordre fixe des cases bleues de la salle (boss sur la 1re, puis le
    /// groupe dans son ordre) ; sinon placement aléatoire. Chaque personnage du combat est noté (table nekfeu_boss_combattus).
    /// </summary>
    public class PremierCombatBoss : Singleton<PremierCombatBoss>
    {
        static Stump.ORM.Database Database => WorldServer.Instance.DBAccessor.Database;

        readonly ConcurrentDictionary<(int, int), bool> m_combattus = new ConcurrentDictionary<(int, int), bool>();

        public class Ligne
        {
            public int CharacterId { get; set; }
            public int MonsterId { get; set; }
        }

        [Initialization(InitializationPass.Last)]
        public void Initialize()
        {
            Database.Execute(@"CREATE TABLE IF NOT EXISTS nekfeu_boss_combattus (CharacterId INT NOT NULL, MonsterId INT NOT NULL,
                PremiereFois DATETIME NOT NULL, PRIMARY KEY (CharacterId, MonsterId)) DEFAULT CHARSET=utf8");
            foreach (var l in Database.Fetch<Ligne>("SELECT CharacterId, MonsterId FROM nekfeu_boss_combattus"))
                m_combattus[(l.CharacterId, l.MonsterId)] = true;
        }

        public bool DejaCombattu(int perso, int boss) => m_combattus.ContainsKey((perso, boss));

        public void Noter(int perso, int boss)
        {
            if (!m_combattus.TryAdd((perso, boss), true))
                return;
            WorldServer.Instance.IOTaskPool.AddMessage(() =>
                Database.Execute("INSERT IGNORE INTO nekfeu_boss_combattus (CharacterId, MonsterId, PremiereFois) VALUES (@0, @1, NOW())", perso, boss));
        }
    }
}
