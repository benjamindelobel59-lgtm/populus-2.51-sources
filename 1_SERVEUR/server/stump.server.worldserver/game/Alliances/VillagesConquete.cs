using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using Stump.Core.Reflection;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Messages;
using Stump.DofusProtocol.Types;
using Stump.ORM;
using Stump.Server.WorldServer.Game.Actors.Interfaces;
using Stump.Server.WorldServer.Game.Actors.Look;
using Stump.Server.WorldServer.Game.Actors.RolePlay;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Dialogs;
using Stump.Server.WorldServer.Game.Maps;
using Stump.Server.WorldServer.Game.Maps.Cells;
using Stump.Server.WorldServer.Handlers.Dialogs;

namespace Stump.Server.WorldServer.Game.Alliances
{
    /// <summary>
    /// MYSTIC 17/10 : économie des villages de conquête (Brigandin / Foreuse), d'après _OUTILS/serveur/alliances/ETUDE_FOREUSE_BRIGANDIN.md.
    /// - coffre d'alliance (kamas) : dépôt par tout membre, retrait par le chef ; tous les achats sont payés par le coffre ;
    /// - 1 PNJ par alliance (Brigandin ou Foreuse) dans un de ses 11 villages de conquête, 10 M, ne débloque rien ;
    /// - destinations officielles (repères Hints.d2o) : Brigandin = 12 transporteurs + 4 diligences, Foreuse = 12 foreuses + 4 diligences ;
    ///   prix = 5 M × (1 + 0,25 × destinations déjà possédées) ; une destination n'est active que sur le PNJ (licence unique) ;
    /// - revente 25 % du prix payé, seulement du début de l'attaque du prisme du village jusqu'au début du KOH ;
    /// - village perdu (KOH) : PNJ et destinations installées perdus ; le gagnant choisit une destination parmi celles du perdant :
    ///   non possédée = débloquée gratuitement (en réserve ou sur son PNJ compatible), possédée = 15 % du prix initial dans son coffre ;
    ///   une seule récompense par conquête (statut en base), à réclamer sous 48 h ;
    /// - membres de l'alliance seulement, téléportation gratuite.
    /// Toutes les opérations économiques passent par une transaction SQL sur une connexion dédiée, l'état en mémoire n'est modifié
    /// qu'après validation. Kamas d'un personnage : retirés en mémoire AVANT la transaction (dépôt), ajoutés APRÈS (retrait) : un
    /// plantage ne peut jamais dupliquer de kamas.
    /// </summary>
    public class VillagesConquete : Singleton<VillagesConquete>
    {
        static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public const int TypeBrigandin = 1;
        public const int TypeForeuse = 2;
        public const int ReseauDiligence = 3;

        public static readonly HashSet<int> Villages = new HashSet<int> { 62, 76, 115, 116, 117, 118, 468, 798, 809, 826, 833 };

        /// <summary>
        /// Portes des villages = cartes officielles où un prisme est autorisé (MapPositions.d2o, allowPrism) : seul endroit où poser le
        /// prisme et seule partie du village ouverte à tous. Fort Thune (826) et le Quartier des Conquérants (833) en ont plusieurs.
        /// </summary>
        public static readonly Dictionary<int, int[]> Portes = new Dictionary<int, int[]>
        {
            { 62, new[] { 100141056 } }, { 76, new[] { 14551301 } }, { 115, new[] { 10016 } }, { 116, new[] { 14128 } },
            { 117, new[] { 11034 } }, { 118, new[] { 16163 } }, { 468, new[] { 28050432 } }, { 798, new[] { 115606025 } },
            { 809, new[] { 126093080 } },
            { 826, new[] { 129237504, 131334660, 131335169, 131335171, 131335680, 131335684, 131336195, 131336704, 131336706, 131336708, 131337217 } },
            { 833, new[] { 138674176, 138805760, 138806273, 139198469, 139198982, 139199491, 139200002, 139200004, 139201026, 139201028, 139202050 } },
        };

        public static bool EstVillage(Map map) => map?.SubArea != null && Villages.Contains(map.SubArea.Id);

        public static bool EstPorte(Map map) => map?.SubArea != null && Portes.TryGetValue(map.SubArea.Id, out var portes) && portes.Contains(map.Id);

        /// <summary>Staff en mode MJ seulement (.god on ou .god invisible) : sinon un compte admin suit les règles des joueurs (tests).</summary>
        static bool EstStaff(Character c) => c?.Account != null && c.Account.UserGroupId >= (int)RoleEnum.Moderator && (c.GodMode || c.Invisible);

        static bool EstProprietaire(Character c, int subAreaId)
        {
            var p = PrismManager.Instance.TryGetPrisme(subAreaId);
            return p != null && c?.Guild?.Alliance != null && c.Guild.Alliance.Id == p.AllianceId;
        }

        /// <summary>
        /// Entrée dans un village de conquête (hors porte) : alliance propriétaire seulement ; pendant le KOH, tout membre d'une alliance
        /// (combattant AvA). Village sans propriétaire : ouvert à tous (demande utilisateur 17/10).
        /// </summary>
        public bool PeutEntrer(Character c, Map destination, out string message)
        {
            message = null;
            if (!EstVillage(destination) || EstPorte(destination) || EstStaff(c))
                return true;

            var sousZone = destination.SubArea.Id;
            if (EstProprietaire(c, sousZone))
                return true;

            var p = PrismManager.Instance.TryGetPrisme(sousZone);
            if (p == null)
                return true;
            if (p != null && p.State == (int)PrismStateEnum.PRISM_STATE_VULNERABLE && c.Guild?.Alliance != null)
                return true;

            message = "Impossible d'accéder à un village de conquête qui n'appartient pas à votre alliance.";
            return false;
        }

        /// <summary>Interactifs (ressources de métier, ateliers…) d'un village : alliance propriétaire seulement ; village sans propriétaire : tous.</summary>
        public bool PeutUtiliserInteractif(Character c, Map map, out string message)
        {
            message = null;
            if (!EstVillage(map) || EstStaff(c) || EstProprietaire(c, map.SubArea.Id) || PrismManager.Instance.TryGetPrisme(map.SubArea.Id) == null)
                return true;

            message = "Les ressources et interactifs de ce village de conquête sont réservés à l'alliance qui le possède.";
            return false;
        }

        public static ObjectPosition PositionPorte(int subAreaId)
        {
            if (!Portes.TryGetValue(subAreaId, out var portes))
                return null;

            var map = World.Instance.GetMap(portes[0]);
            var cell = map?.GetRandomFreeCell(false, true);
            return cell == null ? null : new ObjectPosition(map, cell, DirectionsEnum.DIRECTION_SOUTH_EAST);
        }

        /// <summary>Connexion à l'intérieur d'un village où l'on n'a pas le droit d'être : renvoi au zaap d'Astrub (demande utilisateur).</summary>
        public ObjectPosition PositionALaConnexion(Character c)
        {
            var map = c.Map;
            if (map == null || PeutEntrer(c, map, out _))
                return null;

            var astrub = World.Instance.GetMap(AvAManager.CarteZaapAstrub);
            if (astrub == null)
                return PositionPorte(map.SubArea.Id);

            var cell = (astrub.Zaap != null ? astrub.GetRandomAdjacentFreeCell(astrub.Zaap.Position.Point) : null) ?? astrub.GetRandomFreeCell();
            WorldServer.Instance.IOTaskPool.CallDelayed(4000, () =>
            {
                if (c.IsInWorld)
                    c.DisplayNotification("Ce village de conquête n'appartient pas à votre alliance : vous avez été téléporté au zaap d'Astrub.");
            });
            return new ObjectPosition(astrub, cell, c.Direction);
        }

        /// <summary>Après un changement de propriétaire ou la fin du KOH : ceux qui n'ont plus le droit d'être dans le village vont à la porte.</summary>
        public void Expulser(int subAreaId)
        {
            if (!Villages.Contains(subAreaId))
                return;

            foreach (var c in World.Instance.GetCharacters().Where(x => x.Map?.SubArea?.Id == subAreaId && !x.IsFighting()).ToArray())
            {
                if (PeutEntrer(c, c.Map, out _))
                    continue;

                var porte = PositionPorte(subAreaId);
                if (porte == null)
                    continue;

                var perso = c;
                perso.Area.ExecuteInContext(() =>
                {
                    perso.Teleport(porte, false);
                    AvAManager.Message(perso, "Ce village de conquête n'appartient pas à votre alliance : vous avez été placé à sa porte.");
                });
            }
        }

        // réglages (table nekfeu_village_config)
        public long PrixPnj = 10_000_000;
        public long PrixDestination = 5_000_000;
        public double Progression = 0.25;
        public double Revente = 0.15; // 15 % partout (demande utilisateur 17/10)
        public double ReventeHorsAttaque = 0.15; // revente à l'unité en temps normal (demande utilisateur 17/10)
        public double Compensation = 0.15;
        public int DelaiRecompenseHeures = 48;

        public class Destination
        {
            public int Id { get; set; }
            public int Reseau { get; set; }
            public int MapId { get; set; }
            public int CellId { get; set; }
            public string Nom { get; set; }
        }

        public class Pnj
        {
            public int AllianceId { get; set; }
            public int SubAreaId { get; set; }
            public int Type { get; set; }
            public int MapId { get; set; }
            public int CellId { get; set; }
            public long PrixPaye { get; set; }
            public DateTime Date { get; set; }

            [Ignore]
            public VillageNpc Acteur { get; set; }
        }

        public class Licence
        {
            public int Id { get; set; }
            public int AllianceId { get; set; }
            public int DestId { get; set; }
            public int? SubAreaId { get; set; }
            public long PrixPaye { get; set; }
            public long PrixInitial { get; set; }
            public string Origine { get; set; }
            public DateTime Date { get; set; }
        }

        public class Conquete
        {
            public int Id { get; set; }
            public int SubAreaId { get; set; }
            public int Perdant { get; set; }
            public int Gagnant { get; set; }
            public string Snapshot { get; set; }
            public string Statut { get; set; }
            public DateTime Date { get; set; }

            [Ignore]
            public Dictionary<int, long> Destinations => string.IsNullOrEmpty(Snapshot) ? new Dictionary<int, long>()
                : Snapshot.Split(',').Select(x => x.Split(':')).ToDictionary(x => int.Parse(x[0]), x => long.Parse(x[1]));
        }

        class ConfigRow
        {
            public string Cle { get; set; }
            public string Valeur { get; set; }
        }

        readonly object m_lock = new object();
        Dictionary<int, Destination> m_destinations = new Dictionary<int, Destination>();
        readonly Dictionary<int, Pnj> m_pnj = new Dictionary<int, Pnj>();
        readonly List<Licence> m_licences = new List<Licence>();

        // MYSTIC 18/10 : récompenses de conquête en attente gardées en mémoire. Avant : lues en base à chaque ouverture de l'onglet Village,
        // en même temps que le serveur utilisait la connexion (« There is already an open DataReader ») → déconnexion du joueur.
        readonly List<Conquete> m_attente = new List<Conquete>();
        readonly Dictionary<int, long> m_coffres = new Dictionary<int, long>();
        readonly Dictionary<int, (string action, int valeur, DateTime fin)> m_confirmations = new Dictionary<int, (string, int, DateTime)>();

        static Stump.ORM.Database Db => WorldServer.Instance.DBAccessor.Database;

        #region Chargement et transactions

        public void Charger()
        {
            try
            {
                Db.Execute(@"CREATE TABLE IF NOT EXISTS nekfeu_alliance_coffre (AllianceId INT NOT NULL PRIMARY KEY, Kamas BIGINT NOT NULL DEFAULT 0)");
                Db.Execute(@"CREATE TABLE IF NOT EXISTS nekfeu_village_config (Cle VARCHAR(64) NOT NULL PRIMARY KEY, Valeur VARCHAR(64) NOT NULL)");
                Db.Execute(@"CREATE TABLE IF NOT EXISTS nekfeu_village_destinations (Id INT NOT NULL PRIMARY KEY, Reseau INT NOT NULL, MapId INT NOT NULL,
                    CellId INT NOT NULL DEFAULT 0, Nom VARCHAR(128) NOT NULL)");
                Db.Execute(@"CREATE TABLE IF NOT EXISTS nekfeu_village_pnj (AllianceId INT NOT NULL PRIMARY KEY, SubAreaId INT NOT NULL, Type INT NOT NULL,
                    MapId INT NOT NULL, CellId INT NOT NULL, PrixPaye BIGINT NOT NULL, Date DATETIME NOT NULL)");
                Db.Execute(@"CREATE TABLE IF NOT EXISTS nekfeu_village_licences (Id INT NOT NULL AUTO_INCREMENT PRIMARY KEY, AllianceId INT NOT NULL,
                    DestId INT NOT NULL, SubAreaId INT NULL, PrixPaye BIGINT NOT NULL, PrixInitial BIGINT NOT NULL, Origine VARCHAR(16) NOT NULL,
                    Date DATETIME NOT NULL, UNIQUE KEY alliance_dest (AllianceId, DestId))");
                Db.Execute(@"CREATE TABLE IF NOT EXISTS nekfeu_village_conquetes (Id INT NOT NULL AUTO_INCREMENT PRIMARY KEY, SubAreaId INT NOT NULL,
                    Perdant INT NOT NULL, Gagnant INT NOT NULL, Snapshot TEXT NOT NULL, Statut VARCHAR(16) NOT NULL, Choix INT NULL, Date DATETIME NOT NULL)");
                Db.Execute(@"CREATE TABLE IF NOT EXISTS nekfeu_village_journal (Id INT NOT NULL AUTO_INCREMENT PRIMARY KEY, Date DATETIME NOT NULL,
                    AllianceId INT NOT NULL, CharacterId INT NOT NULL, Operation VARCHAR(32) NOT NULL, Montant BIGINT NOT NULL, Details VARCHAR(255) NOT NULL)");

                foreach (var c in Db.Fetch<ConfigRow>("SELECT * FROM nekfeu_village_config"))
                    AppliquerConfig(c.Cle, c.Valeur);

                m_destinations = Db.Fetch<Destination>("SELECT * FROM nekfeu_village_destinations").ToDictionary(x => x.Id);
                foreach (var c in Db.Fetch<CoffreRow>("SELECT * FROM nekfeu_alliance_coffre"))
                    m_coffres[c.AllianceId] = c.Kamas;
                m_licences.AddRange(Db.Fetch<Licence>("SELECT * FROM nekfeu_village_licences"));
                m_attente.AddRange(Db.Fetch<Conquete>("SELECT * FROM nekfeu_village_conquetes WHERE Statut = 'en_attente'"));

                foreach (var pnj in Db.Fetch<Pnj>("SELECT * FROM nekfeu_village_pnj"))
                {
                    // le village a pu être perdu pendant un arrêt : PNJ et licences installées perdus
                    var prisme = PrismManager.Instance.TryGetPrisme(pnj.SubAreaId);
                    if (prisme == null || prisme.AllianceId != pnj.AllianceId)
                    {
                        logger.Warn("[Villages] PNJ de l'alliance {0} sans village (sous-zone {1}) : supprimé", pnj.AllianceId, pnj.SubAreaId);
                        Transaction(db => SupprimerPnjSql(db, pnj, 0, "pnj_orphelin"));
                        m_licences.RemoveAll(x => x.AllianceId == pnj.AllianceId && x.SubAreaId == pnj.SubAreaId);
                        continue;
                    }
                    m_pnj[pnj.AllianceId] = pnj;
                    Apparaitre(pnj, true);
                }

                logger.Info("[Villages] {0} destinations, {1} PNJ, {2} licences, {3} coffres", m_destinations.Count, m_pnj.Count, m_licences.Count, m_coffres.Count);
            }
            catch (Exception e)
            {
                logger.Error("[Villages] chargement impossible : {0}", e);
            }
        }

        public class CoffreRow
        {
            public int AllianceId { get; set; }
            public long Kamas { get; set; }
        }

        void AppliquerConfig(string cle, string valeur)
        {
            var inv = System.Globalization.CultureInfo.InvariantCulture;
            switch (cle)
            {
                case "PrixPnj": PrixPnj = long.Parse(valeur); break;
                case "PrixDestination": PrixDestination = long.Parse(valeur); break;
                case "Progression": Progression = double.Parse(valeur, inv); break;
                case "Revente": Revente = double.Parse(valeur, inv); break;
                case "ReventeHorsAttaque": ReventeHorsAttaque = double.Parse(valeur, inv); break;
                case "Compensation": Compensation = double.Parse(valeur, inv); break;
                case "DelaiRecompenseHeures": DelaiRecompenseHeures = int.Parse(valeur); break;
            }
        }

        /// <summary>Transaction sur une connexion dédiée (isolée de la connexion partagée du serveur).</summary>
        static void Transaction(Action<Stump.ORM.Database> operations)
        {
            var conf = WorldServer.DatabaseConfiguration;
            using (var db = new Stump.ORM.Database(conf.GetConnectionString(), conf.ProviderName))
            {
                db.BeginTransaction();
                try
                {
                    operations(db);
                    db.CompleteTransaction();
                }
                catch
                {
                    db.AbortTransaction();
                    throw;
                }
            }
        }

        static void Journal(Stump.ORM.Database db, int allianceId, Character c, string operation, long montant, string details) =>
            db.Execute("INSERT INTO nekfeu_village_journal (Date, AllianceId, CharacterId, Operation, Montant, Details) VALUES (@0, @1, @2, @3, @4, @5)",
                DateTime.Now, allianceId, c?.Id ?? 0, operation, montant, details ?? "");

        /// <summary>Débit du coffre : échoue (exception, donc annulation) si le coffre n'a pas assez.</summary>
        static void DebiterCoffre(Stump.ORM.Database db, int allianceId, long montant)
        {
            if (db.Execute("UPDATE nekfeu_alliance_coffre SET Kamas = Kamas - @1 WHERE AllianceId = @0 AND Kamas >= @1", allianceId, montant) != 1)
                throw new InvalidOperationException("coffre insuffisant");
        }

        static void CrediterCoffre(Stump.ORM.Database db, int allianceId, long montant) =>
            db.Execute("INSERT INTO nekfeu_alliance_coffre (AllianceId, Kamas) VALUES (@0, @1) ON DUPLICATE KEY UPDATE Kamas = Kamas + @1", allianceId, montant);

        #endregion

        #region Lecture

        public long Coffre(int allianceId)
        {
            lock (m_lock)
                return m_coffres.TryGetValue(allianceId, out var k) ? k : 0;
        }

        public Pnj PnjDe(int allianceId)
        {
            lock (m_lock)
                return m_pnj.TryGetValue(allianceId, out var p) ? p : null;
        }

        List<Licence> LicencesDe(int allianceId) => m_licences.Where(x => x.AllianceId == allianceId).ToList();

        /// <summary>
        /// Demande utilisateur (17/10) : le type du PNJ (Brigandin / Foreuse) n'est qu'une apparence ; toutes les destinations
        /// (brigandins, foreuses et diligences) s'achètent sur le PNJ unique de l'alliance.
        /// </summary>
        public static bool Compatible(int typePnj, Destination d) => true;

        public long PrixProchaine(int allianceId)
        {
            lock (m_lock)
                return (long)Math.Round(PrixDestination * (1 + Progression * LicencesDe(allianceId).Count));
        }

        static string Kamas(long k) => k.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("fr-FR")) + " k";

        static string NomType(int type) => "Téléporteur de l'alliance";

        static bool Gestionnaire(Character c) =>
            Conditions.Criterions.AllianceRightsCriterion.AUnDroit(c, AllianceRightsBitEnum.ALLIANCE_RIGHT_MANAGE_PRISMS);

        #endregion

        #region Coffre d'alliance

        public string Deposer(Character c, long montant)
        {
            var alliance = c.Guild?.Alliance;
            if (alliance == null)
                return "Il faut une alliance.";
            if (montant <= 0 || (ulong)montant > c.Inventory.Kamas)
                return "Montant invalide ou kamas insuffisants.";

            lock (m_lock)
            {
                // kamas retirés en mémoire AVANT : un plantage ne peut que faire perdre, jamais dupliquer
                c.Inventory.SubKamas((ulong)montant);
                try
                {
                    Transaction(db =>
                    {
                        db.Execute("UPDATE characters SET Kamas = @1 WHERE Id = @0", c.Id, (long)c.Inventory.Kamas);
                        CrediterCoffre(db, alliance.Id, montant);
                        Journal(db, alliance.Id, c, "depot", montant, c.Name);
                    });
                }
                catch (Exception e)
                {
                    c.Inventory.AddKamas((ulong)montant);
                    logger.Error("[Villages] dépôt annulé : {0}", e.Message);
                    return "Dépôt annulé (erreur de base de données), vos kamas vous ont été rendus.";
                }
                m_coffres[alliance.Id] = Coffre(alliance.Id) + montant;
            }

            return $"Dépôt de {Kamas(montant)} dans le coffre de l'alliance (coffre : {Kamas(Coffre(alliance.Id))}).";
        }

        public string RetirerCoffre(Character c, long montant)
        {
            var alliance = c.Guild?.Alliance;
            if (alliance == null || !alliance.IsLeader(c))
                return "Seul le chef de l'alliance peut retirer des kamas du coffre.";
            if (montant <= 0)
                return "Montant invalide.";

            lock (m_lock)
            {
                if (Coffre(alliance.Id) < montant)
                    return "Le coffre ne contient pas assez de kamas.";

                // base d'abord, mémoire ensuite : un plantage entre les deux ne peut que faire perdre, jamais dupliquer
                var apres = (long)c.Inventory.Kamas + montant;
                try
                {
                    Transaction(db =>
                    {
                        DebiterCoffre(db, alliance.Id, montant);
                        db.Execute("UPDATE characters SET Kamas = @1 WHERE Id = @0", c.Id, apres);
                        Journal(db, alliance.Id, c, "retrait", montant, c.Name);
                    });
                }
                catch (Exception e)
                {
                    logger.Error("[Villages] retrait annulé : {0}", e.Message);
                    return "Retrait annulé.";
                }
                m_coffres[alliance.Id] = Coffre(alliance.Id) - montant;
                c.Inventory.AddKamas((ulong)montant);
            }

            return $"Retrait de {Kamas(montant)} du coffre (coffre : {Kamas(Coffre(alliance.Id))}).";
        }

        #endregion

        #region PNJ

        public string PreparerPnj(Character c, int type)
        {
            var alliance = c.Guild?.Alliance;
            if (alliance == null || !Gestionnaire(c))
                return "Il faut le droit de gérer les prismes de l'alliance.";

            var sousZone = c.Map?.SubArea?.Id ?? 0;
            if (!Villages.Contains(sousZone))
                return "Le PNJ s'installe dans un village de conquête (Brigandins, Dopeuls, Pandala, Zoths, Kanigs…).";
            if (c.Map.Zaap == null)
                return "Le Téléporteur s'installe uniquement à côté du zaap du village de conquête.";

            var prisme = PrismManager.Instance.TryGetPrisme(sousZone);
            if (prisme == null || prisme.AllianceId != alliance.Id)
                return "Ce village n'appartient pas à votre alliance.";
            if (prisme.State != (int)PrismStateEnum.PRISM_STATE_NORMAL)
                return "Impossible pendant une attaque ou une conquête.";
            if (PnjDe(alliance.Id) != null)
                return "Votre alliance a déjà un PNJ (1 maximum).";
            if (Coffre(alliance.Id) < PrixPnj)
                return $"Le coffre de l'alliance doit contenir {Kamas(PrixPnj)} (coffre : {Kamas(Coffre(alliance.Id))}).";

            Demander(c, "pnj", type);
            return $"Installer le Téléporteur de l'alliance ici pour {Kamas(PrixPnj)} (payé par le coffre, ne débloque aucune destination ; toutes les destinations Brigandin, Foreuse et Diligence s'y achètent) ? Tapez .village confirmer";
        }

        string InstallerPnj(Character c, int type)
        {
            var alliance = c.Guild.Alliance;
            var map = c.Map;
            var prisme = PrismManager.Instance.TryGetPrisme(map.SubArea.Id);
            if (prisme == null || prisme.AllianceId != alliance.Id || PnjDe(alliance.Id) != null)
                return "Installation impossible.";

            if (map.Zaap == null)
                return "Le Téléporteur s'installe uniquement à côté du zaap du village de conquête.";
            var cell = map.GetRandomAdjacentFreeCell(map.Zaap.Position.Point, true) ?? c.Cell;
            var pnj = new Pnj { AllianceId = alliance.Id, SubAreaId = map.SubArea.Id, Type = type, MapId = map.Id, CellId = cell.Id, PrixPaye = PrixPnj, Date = DateTime.Now };
            var aInstaller = LicencesDe(alliance.Id).Where(x => x.SubAreaId == null && m_destinations.TryGetValue(x.DestId, out var d) && Compatible(type, d)).ToList();

            try
            {
                Transaction(db =>
                {
                    DebiterCoffre(db, alliance.Id, PrixPnj);
                    db.Execute(@"INSERT INTO nekfeu_village_pnj (AllianceId, SubAreaId, Type, MapId, CellId, PrixPaye, Date) VALUES (@0, @1, @2, @3, @4, @5, @6)",
                        pnj.AllianceId, pnj.SubAreaId, pnj.Type, pnj.MapId, pnj.CellId, pnj.PrixPaye, pnj.Date);
                    foreach (var l in aInstaller)
                        db.Execute("UPDATE nekfeu_village_licences SET SubAreaId = @1 WHERE Id = @0", l.Id, pnj.SubAreaId);
                    Journal(db, alliance.Id, c, "achat_pnj", PrixPnj, NomType(type) + " sous-zone " + pnj.SubAreaId);
                });
            }
            catch (Exception e)
            {
                logger.Error("[Villages] achat du PNJ annulé : {0}", e.Message);
                return "Achat annulé (coffre insuffisant ou erreur de base de données).";
            }

            m_coffres[alliance.Id] = Coffre(alliance.Id) - PrixPnj;
            foreach (var l in aInstaller)
                l.SubAreaId = pnj.SubAreaId;
            m_pnj[alliance.Id] = pnj;
            Apparaitre(pnj);
            return $"{NomType(type)} installé ! {aInstaller.Count} destination(s) en réserve activée(s). Tapez .village destinations";
        }

        /// <param name="demarrage">au chargement du serveur (comme les prismes) : entrée directe sur la carte</param>
        void Apparaitre(Pnj pnj, bool demarrage = false)
        {
            var map = World.Instance.GetMap(pnj.MapId);
            if (map == null)
                return;

            void Entrer()
            {
                var acteur = new VillageNpc(map.GetNextContextualId(), pnj, new ObjectPosition(map, map.Cells[pnj.CellId], DirectionsEnum.DIRECTION_SOUTH_WEST));
                pnj.Acteur = acteur;
                map.Enter(acteur);
            }

            if (demarrage)
                Entrer();
            else
                map.Area.ExecuteInContext(Entrer);
        }

        static void Disparaitre(Pnj pnj)
        {
            var acteur = pnj.Acteur;
            pnj.Acteur = null;
            if (acteur?.Map != null)
                acteur.Map.Area.ExecuteInContext(() => acteur.Map?.Leave(acteur));
        }

        static void SupprimerPnjSql(Stump.ORM.Database db, Pnj pnj, int characterId, string raison)
        {
            db.Execute("DELETE FROM nekfeu_village_pnj WHERE AllianceId = @0", pnj.AllianceId);
            db.Execute("DELETE FROM nekfeu_village_licences WHERE AllianceId = @0 AND SubAreaId = @1", pnj.AllianceId, pnj.SubAreaId);
            db.Execute("INSERT INTO nekfeu_village_journal (Date, AllianceId, CharacterId, Operation, Montant, Details) VALUES (@0, @1, @2, @3, 0, @4)",
                DateTime.Now, pnj.AllianceId, characterId, raison, "sous-zone " + pnj.SubAreaId);
        }

        #endregion

        #region Destinations

        public string ListeDestinations(Character c)
        {
            var alliance = c.Guild?.Alliance;
            if (alliance == null)
                return "Il faut une alliance.";

            lock (m_lock)
            {
                var pnj = PnjDe(alliance.Id);
                var licences = LicencesDe(alliance.Id).ToDictionary(x => x.DestId);
                var type = pnj?.Type ?? 0;
                var lignes = m_destinations.Values.Where(d => type == 0 || Compatible(type, d)).OrderBy(d => d.Reseau).ThenBy(d => d.Id).Select(d =>
                {
                    var etat = licences.TryGetValue(d.Id, out var l) ? (l.SubAreaId != null ? "<b>active</b>" : "en réserve") : "à débloquer";
                    var reseau = d.Reseau == ReseauDiligence ? "Diligence" : NomType(d.Reseau);
                    return $"{d.Id}. [{reseau}] {d.Nom} — {etat}";
                });
                return (pnj == null ? "Votre alliance n'a pas de PNJ (toutes les destinations sont listées).\n" : $"PNJ : {NomType(pnj.Type)}.\n")
                    + string.Join("\n", lignes) + $"\nPrix de la prochaine destination : {Kamas(PrixProchaine(alliance.Id))}. Achat : .village acheter <numéro>";
            }
        }

        public string PreparerAchat(Character c, int destId)
        {
            var alliance = c.Guild?.Alliance;
            if (alliance == null || !Gestionnaire(c))
                return "Il faut le droit de gérer les prismes de l'alliance.";

            lock (m_lock)
            {
                var pnj = PnjDe(alliance.Id);
                if (pnj == null)
                    return "Installez d'abord un PNJ (.village pnj brigandin|foreuse).";
                if (!m_destinations.TryGetValue(destId, out var d) || !Compatible(pnj.Type, d))
                    return "Cette destination n'existe pas pour votre PNJ (.village destinations).";
                if (LicencesDe(alliance.Id).Any(x => x.DestId == destId))
                    return "Destination déjà débloquée.";
                var etat = PrismManager.Instance.TryGetPrisme(pnj.SubAreaId)?.State;
                if (etat != (int)PrismStateEnum.PRISM_STATE_NORMAL)
                    return "Impossible pendant une attaque ou une conquête du village.";

                var prix = PrixProchaine(alliance.Id);
                if (Coffre(alliance.Id) < prix)
                    return $"Le coffre doit contenir {Kamas(prix)} (coffre : {Kamas(Coffre(alliance.Id))}).";

                Demander(c, "acheter", destId);
                return $"Débloquer « {d.Nom} » pour {Kamas(prix)} (payé par le coffre) ? Tapez .village confirmer";
            }
        }

        string Acheter(Character c, int destId)
        {
            var alliance = c.Guild.Alliance;
            var pnj = PnjDe(alliance.Id);
            if (pnj == null || !m_destinations.TryGetValue(destId, out var d) || !Compatible(pnj.Type, d) || LicencesDe(alliance.Id).Any(x => x.DestId == destId))
                return "Achat impossible.";

            var prix = PrixProchaine(alliance.Id);
            var licence = new Licence { AllianceId = alliance.Id, DestId = destId, SubAreaId = pnj.SubAreaId, PrixPaye = prix, PrixInitial = prix, Origine = "achat", Date = DateTime.Now };
            try
            {
                Transaction(db =>
                {
                    DebiterCoffre(db, alliance.Id, prix);
                    licence.Id = Convert.ToInt32(db.ExecuteScalar<long>(@"INSERT INTO nekfeu_village_licences (AllianceId, DestId, SubAreaId, PrixPaye, PrixInitial, Origine, Date)
                        VALUES (@0, @1, @2, @3, @4, @5, @6); SELECT LAST_INSERT_ID();", licence.AllianceId, licence.DestId, licence.SubAreaId, licence.PrixPaye,
                        licence.PrixInitial, licence.Origine, licence.Date));
                    Journal(db, alliance.Id, c, "achat_destination", prix, d.Nom);
                });
            }
            catch (Exception e)
            {
                logger.Error("[Villages] achat de destination annulé : {0}", e.Message);
                return "Achat annulé (coffre insuffisant ou erreur de base de données).";
            }

            m_coffres[alliance.Id] = Coffre(alliance.Id) - prix;
            m_licences.Add(licence);
            return $"« {d.Nom} » débloquée pour {Kamas(prix)}. Prochaine destination : {Kamas(PrixProchaine(alliance.Id))}.";
        }

        #endregion

        #region Revente (début de l'attaque → début du KOH)

        static bool FenetreRevente(PrismeAlliance p) => p != null && (p.State == (int)PrismStateEnum.PRISM_STATE_ATTACKED ||
            p.State == (int)PrismStateEnum.PRISM_STATE_FIGHTING || p.State == (int)PrismStateEnum.PRISM_STATE_WEAKENED);

        /// <summary>Taux de revente à l'unité : 25 % pendant la fenêtre d'attaque, 15 % sinon, null pendant le KOH (village vulnérable).</summary>
        double? TauxReventeUnite(int allianceId)
        {
            var pnj = PnjDe(allianceId);
            var prisme = pnj == null ? null : PrismManager.Instance.TryGetPrisme(pnj.SubAreaId);
            if (prisme != null && prisme.State == (int)PrismStateEnum.PRISM_STATE_VULNERABLE)
                return null;
            return FenetreRevente(prisme) ? Revente : ReventeHorsAttaque;
        }

        public string ReventeUnite(Character c, int destId)
        {
            var alliance = c.Guild?.Alliance;
            if (alliance == null || !Gestionnaire(c))
                return "Il faut le droit de gérer les prismes de l'alliance.";

            lock (m_lock)
            {
                var licence = LicencesDe(alliance.Id).FirstOrDefault(x => x.DestId == destId);
                if (licence == null)
                    return "Cette destination n'est pas débloquée.";
                var taux = TauxReventeUnite(alliance.Id);
                if (taux == null)
                    return "Revente impossible pendant la conquête (KOH) du village.";

                var montant = (long)Math.Floor(licence.PrixPaye * taux.Value);
                var nom = m_destinations.TryGetValue(destId, out var d) ? d.Nom : destId.ToString();
                try
                {
                    Transaction(db =>
                    {
                        if (db.Execute("DELETE FROM nekfeu_village_licences WHERE Id = @0", licence.Id) != 1)
                            throw new InvalidOperationException("licence déjà retirée");
                        if (montant > 0)
                            CrediterCoffre(db, alliance.Id, montant);
                        Journal(db, alliance.Id, c, "revente_unite", montant, nom + " (" + (int)Math.Round(taux.Value * 100) + " %)");
                    });
                }
                catch (Exception e)
                {
                    logger.Error("[Villages] revente à l'unité annulée : {0}", e.Message);
                    return "Revente annulée.";
                }

                m_licences.Remove(licence);
                m_coffres[alliance.Id] = Coffre(alliance.Id) + montant;
                return $"« {nom} » revendue : {Kamas(montant)} ajoutés au coffre ({(int)Math.Round(taux.Value * 100)} % du prix payé).";
            }
        }

        public string PreparerRevente(Character c)
        {
            var alliance = c.Guild?.Alliance;
            if (alliance == null || !Gestionnaire(c))
                return "Il faut le droit de gérer les prismes de l'alliance.";

            lock (m_lock)
            {
                var pnj = PnjDe(alliance.Id);
                if (pnj == null)
                    return "Votre alliance n'a pas de PNJ.";
                if (!FenetreRevente(PrismManager.Instance.TryGetPrisme(pnj.SubAreaId)))
                    return "La revente n'est possible que du début de l'attaque du prisme du village jusqu'au début de la conquête (KOH).";

                var actives = LicencesDe(alliance.Id).Where(x => x.SubAreaId == pnj.SubAreaId).ToList();
                if (actives.Count == 0)
                    return "Aucune destination active à revendre.";

                var total = actives.Sum(x => (long)Math.Floor(x.PrixPaye * Revente));
                Demander(c, "revendre", 0);
                return $"Revendre les {actives.Count} destination(s) du village pour {Kamas(total)} (15 % du prix payé, dans le coffre) ? " +
                       "C'est DÉFINITIF, même si vous gagnez la défense. Tapez .village confirmer";
            }
        }

        string Revendre(Character c)
        {
            var alliance = c.Guild.Alliance;
            var pnj = PnjDe(alliance.Id);
            if (pnj == null || !FenetreRevente(PrismManager.Instance.TryGetPrisme(pnj.SubAreaId)))
                return "Revente impossible (la fenêtre est fermée).";

            var actives = LicencesDe(alliance.Id).Where(x => x.SubAreaId == pnj.SubAreaId).ToList();
            var total = actives.Sum(x => (long)Math.Floor(x.PrixPaye * Revente));
            try
            {
                Transaction(db =>
                {
                    foreach (var l in actives)
                        if (db.Execute("DELETE FROM nekfeu_village_licences WHERE Id = @0", l.Id) != 1)
                            throw new InvalidOperationException("licence déjà retirée");
                    if (total > 0)
                        CrediterCoffre(db, alliance.Id, total);
                    Journal(db, alliance.Id, c, "revente", total, actives.Count + " destination(s)");
                });
            }
            catch (Exception e)
            {
                logger.Error("[Villages] revente annulée : {0}", e.Message);
                return "Revente annulée.";
            }

            m_licences.RemoveAll(actives.Contains);
            m_coffres[alliance.Id] = Coffre(alliance.Id) + total;
            return $"{actives.Count} destination(s) revendue(s) : {Kamas(total)} ajoutés au coffre.";
        }

        #endregion

        #region Conquête et récompense

        /// <summary>AvAManager.TerminerKoh : le village change de propriétaire (appelé AVANT le retrait du prisme).</summary>
        public void SurConquete(PrismeAlliance p, int gagnant)
        {
            lock (m_lock)
            {
                var pnj = m_pnj.Values.FirstOrDefault(x => x.SubAreaId == p.SubAreaId && x.AllianceId == p.AllianceId);
                if (pnj == null)
                    return;

                var perdues = LicencesDe(pnj.AllianceId).Where(x => x.SubAreaId == pnj.SubAreaId).ToList();
                var snapshot = string.Join(",", perdues.Select(x => x.DestId + ":" + x.PrixInitial));
                Conquete nouvelle = null;
                try
                {
                    // changement de propriétaire et récompense en attente dans la même transaction
                    Transaction(db =>
                    {
                        SupprimerPnjSql(db, pnj, 0, "village_perdu");
                        if (perdues.Count > 0)
                        {
                            var date = DateTime.Now;
                            var id = Convert.ToInt32(db.ExecuteScalar<long>(@"INSERT INTO nekfeu_village_conquetes (SubAreaId, Perdant, Gagnant, Snapshot, Statut, Date)
                                VALUES (@0, @1, @2, @3, 'en_attente', @4); SELECT LAST_INSERT_ID();", p.SubAreaId, pnj.AllianceId, gagnant, snapshot, date));
                            nouvelle = new Conquete { Id = id, SubAreaId = p.SubAreaId, Perdant = pnj.AllianceId, Gagnant = gagnant, Snapshot = snapshot,
                                Statut = "en_attente", Date = date };
                        }
                    });
                    if (nouvelle != null)
                        m_attente.Add(nouvelle);
                }
                catch (Exception e)
                {
                    logger.Error("[Villages] conquête du village {0} : erreur {1}", p.SubAreaId, e);
                    return;
                }

                m_pnj.Remove(pnj.AllianceId);
                m_licences.RemoveAll(perdues.Contains);
                Disparaitre(pnj);

                foreach (var membre in AllianceManager.Instance.TryGetAlliance(pnj.AllianceId)?.OnlineCharacters ?? Enumerable.Empty<Character>())
                    AvAManager.Message(membre, $"Village {AvAManager.Lien(p.SubAreaId)} perdu : votre {NomType(pnj.Type)} et ses {perdues.Count} destination(s) sont perdus.");

                if (perdues.Count > 0)
                    foreach (var membre in AllianceManager.Instance.TryGetAlliance(gagnant)?.OnlineCharacters ?? Enumerable.Empty<Character>())
                        AvAManager.Message(membre, $"Récompense de conquête du village {AvAManager.Lien(p.SubAreaId)} : choisissez une destination avec .village recompense " +
                            $"(sous {DelaiRecompenseHeures} h, droit de gérer les prismes).");
            }
        }

        List<Conquete> RecompensesEnAttente(int allianceId)
        {
            var limite = DateTime.Now.AddHours(-DelaiRecompenseHeures);
            lock (m_lock)
                return m_attente.Where(x => x.Gagnant == allianceId && x.Statut == "en_attente" && x.Date >= limite).OrderBy(x => x.Id).ToList();
        }

        public string ListeRecompenses(Character c)
        {
            var alliance = c.Guild?.Alliance;
            if (alliance == null)
                return "Il faut une alliance.";

            var attente = RecompensesEnAttente(alliance.Id);
            if (attente.Count == 0)
                return "Aucune récompense de conquête en attente.";

            var possedees = new HashSet<int>(LicencesDe(alliance.Id).Select(x => x.DestId));
            return string.Join("\n", attente.Select(q => $"Conquête n° {q.Id} ({AvAManager.Lien(q.SubAreaId)}) :\n" + string.Join("\n",
                q.Destinations.Select(kv => $"  {kv.Key}. {(m_destinations.TryGetValue(kv.Key, out var d) ? d.Nom : "?")} — " +
                    (possedees.Contains(kv.Key) ? $"déjà possédée : {Kamas((long)Math.Floor(kv.Value * Compensation))} au coffre" : "débloquée gratuitement")))))
                + "\nChoix : .village recompense <n° de conquête> <n° de destination>";
        }

        public string ChoisirRecompense(Character c, int conqueteId, int destId)
        {
            var alliance = c.Guild?.Alliance;
            if (alliance == null || !Gestionnaire(c))
                return "Il faut le droit de gérer les prismes de l'alliance.";

            lock (m_lock)
            {
                var q = RecompensesEnAttente(alliance.Id).FirstOrDefault(x => x.Id == conqueteId);
                if (q == null)
                    return "Cette récompense n'existe pas, a expiré ou a déjà été prise.";
                if (!q.Destinations.TryGetValue(destId, out var prixInitial))
                    return "Cette destination ne fait pas partie de la récompense.";

                var dejaPossedee = LicencesDe(alliance.Id).Any(x => x.DestId == destId);
                var pnj = PnjDe(alliance.Id);
                var installer = pnj != null && m_destinations.TryGetValue(destId, out var d) && Compatible(pnj.Type, d);
                var compensation = (long)Math.Floor(prixInitial * Compensation);
                Licence licence = null;

                try
                {
                    Transaction(db =>
                    {
                        // une seule récompense par conquête : le premier UPDATE gagne, tout autre essai est annulé
                        if (db.Execute("UPDATE nekfeu_village_conquetes SET Statut = 'reclamee', Choix = @1 WHERE Id = @0 AND Statut = 'en_attente'", q.Id, destId) != 1)
                            throw new InvalidOperationException("déjà réclamée");

                        if (dejaPossedee)
                        {
                            CrediterCoffre(db, alliance.Id, compensation);
                            Journal(db, alliance.Id, c, "recompense_kamas", compensation, "conquête " + q.Id);
                        }
                        else
                        {
                            licence = new Licence { AllianceId = alliance.Id, DestId = destId, SubAreaId = installer ? pnj.SubAreaId : (int?)null,
                                PrixPaye = 0, PrixInitial = prixInitial, Origine = "recompense", Date = DateTime.Now };
                            licence.Id = Convert.ToInt32(db.ExecuteScalar<long>(@"INSERT INTO nekfeu_village_licences (AllianceId, DestId, SubAreaId, PrixPaye, PrixInitial, Origine, Date)
                                VALUES (@0, @1, @2, @3, @4, @5, @6); SELECT LAST_INSERT_ID();", licence.AllianceId, licence.DestId, licence.SubAreaId,
                                licence.PrixPaye, licence.PrixInitial, licence.Origine, licence.Date));
                            Journal(db, alliance.Id, c, "recompense_destination", 0, "conquête " + q.Id + " destination " + destId);
                        }
                    });
                }
                catch (Exception e)
                {
                    logger.Warn("[Villages] récompense {0} refusée : {1}", q.Id, e.Message);
                    return "Récompense déjà prise ou erreur : rien n'a été donné.";
                }

                m_attente.RemoveAll(x => x.Id == q.Id);

                if (dejaPossedee)
                {
                    m_coffres[alliance.Id] = Coffre(alliance.Id) + compensation;
                    return $"Destination déjà possédée : {Kamas(compensation)} ajoutés au coffre de l'alliance.";
                }

                m_licences.Add(licence);
                return licence.SubAreaId != null ? "Destination débloquée gratuitement et active sur votre PNJ !"
                    : "Destination débloquée gratuitement : elle sera active quand vous installerez un PNJ compatible.";
            }
        }

        /// <summary>L'alliance est dissoute : PNJ et licences supprimés (le coffre reste en base pour les admins).</summary>
        public void SurAllianceSupprimee(int allianceId)
        {
            lock (m_lock)
            {
                var pnj = PnjDe(allianceId);
                try
                {
                    Transaction(db =>
                    {
                        if (pnj != null)
                            SupprimerPnjSql(db, pnj, 0, "alliance_dissoute");
                        db.Execute("DELETE FROM nekfeu_village_licences WHERE AllianceId = @0", allianceId);
                    });
                }
                catch (Exception e)
                {
                    logger.Error("[Villages] dissolution {0} : {1}", allianceId, e.Message);
                    return;
                }
                if (pnj != null)
                {
                    m_pnj.Remove(allianceId);
                    Disparaitre(pnj);
                }
                m_licences.RemoveAll(x => x.AllianceId == allianceId);
            }
        }

        #endregion

        #region Onglet « Village » du panneau d'alliance (client)

        public const string Marqueur = "§VIL§";

        static string Propre(string s) => (s ?? "").Replace("~", "-").Replace(";", ",").Replace("|", "/").Replace("^", " ").Replace("{", "(").Replace("}", ")");

        /// <summary>
        /// Données de l'onglet, champs séparés par ; (le client découpe les messages d'information au « ~ ») : coffre ~ type PNJ (0 / 1 / 2) ~ village du PNJ ~ prix de la prochaine ~ revente
        /// possible (0 / 1) ~ gestionnaire (0 / 1) ~ chef (0 / 1) ~ destinations (id^réseau^nom^état 0 à débloquer / 1 active / 2 réserve,
        /// séparées par |) ~ récompenses (conquête^zone^destination^nom^possédée^kamas, séparées par |) ~ total de la revente.
        /// </summary>
        public string DonneesOnglet(Character c)
        {
            var alliance = c.Guild?.Alliance;
            if (alliance == null)
                return Marqueur + "0;0;;0;0;0;0;;;0";

            lock (m_lock)
            {
                var pnj = PnjDe(alliance.Id);
                var licences = LicencesDe(alliance.Id).ToDictionary(x => x.DestId);
                var type = pnj?.Type ?? 0;
                var taux = TauxReventeUnite(alliance.Id);
                var dests = string.Join("|", m_destinations.Values.Where(d => type == 0 || Compatible(type, d)).OrderBy(d => d.Reseau).ThenBy(d => d.Id)
                    .Select(d => $"{d.Id}^{d.Reseau}^{Propre(d.Nom)}^" + (licences.TryGetValue(d.Id, out var l) ? (l.SubAreaId != null ? "1" : "2") : "0")
                        + "^" + (licences.TryGetValue(d.Id, out var l2) && taux != null ? (long)Math.Floor(l2.PrixPaye * taux.Value) : -1)));

                var possedees = new HashSet<int>(licences.Keys);
                var recs = string.Join("|", RecompensesEnAttente(alliance.Id).SelectMany(q => q.Destinations.Select(kv =>
                    $"{q.Id}^{Propre(World.Instance.GetSubArea(q.SubAreaId)?.Record?.Name ?? q.SubAreaId.ToString())}^{kv.Key}^" +
                    $"{Propre(m_destinations.TryGetValue(kv.Key, out var d) ? d.Nom : "?")}^{(possedees.Contains(kv.Key) ? 1 : 0)}^{(long)Math.Floor(kv.Value * Compensation)}")));

                var prisme = pnj == null ? null : PrismManager.Instance.TryGetPrisme(pnj.SubAreaId);
                var revente = pnj == null ? 0 : LicencesDe(alliance.Id).Where(x => x.SubAreaId == pnj.SubAreaId).Sum(x => (long)Math.Floor(x.PrixPaye * Revente));
                var village = pnj == null ? "" : Propre(World.Instance.GetSubArea(pnj.SubAreaId)?.Record?.Name ?? "");

                return Marqueur + string.Join(";", Coffre(alliance.Id), type, village, PrixProchaine(alliance.Id), FenetreRevente(prisme) ? 1 : 0,
                    Gestionnaire(c) ? 1 : 0, alliance.IsLeader(c) ? 1 : 0, dests, recs, revente);
            }
        }

        /// <summary>Ordre envoyé par l'onglet (« §VIL§action v1 v2 ») : l'onglet a déjà demandé la confirmation au joueur.</summary>
        public void Onglet(Character c, string ligne)
        {
            var mots = (ligne ?? "").Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var action = mots.Length > 0 ? mots[0].ToLowerInvariant() : "data";
            long.TryParse(mots.Length > 1 ? mots[1] : "", out var n1);
            long.TryParse(mots.Length > 2 ? mots[2] : "", out var n2);

            string reponse = null;
            switch (action)
            {
                case "deposer": reponse = Deposer(c, n1); break;
                case "retirer": reponse = RetirerCoffre(c, n1); break;
                case "pnj": reponse = Directement(c, () => PreparerPnj(c, n1 == TypeForeuse ? TypeForeuse : TypeBrigandin)); break;
                case "acheter": reponse = Directement(c, () => PreparerAchat(c, (int)n1)); break;
                case "revendre": reponse = Directement(c, () => PreparerRevente(c)); break;
                case "revendre1": reponse = ReventeUnite(c, (int)n1); break;
                case "recompense": reponse = ChoisirRecompense(c, (int)n1, (int)n2); break;
            }

            if (reponse != null)
                AvAManager.Message(c, reponse);
            c.SendServerMessage(DonneesOnglet(c));
        }

        /// <summary>Vérifications de la commande, puis exécution sans seconde confirmation (déjà faite dans l'onglet).</summary>
        string Directement(Character c, Func<string> preparer)
        {
            lock (m_lock)
            {
                m_confirmations.Remove(c.Id);
                var message = preparer();
                return m_confirmations.ContainsKey(c.Id) ? Confirmer(c) : message;
            }
        }

        #endregion

        #region Confirmation et téléportation

        void Demander(Character c, string action, int valeur)
        {
            lock (m_lock)
                m_confirmations[c.Id] = (action, valeur, DateTime.Now.AddSeconds(60));
        }

        public string Confirmer(Character c)
        {
            lock (m_lock)
            {
                if (!m_confirmations.TryGetValue(c.Id, out var dem) || dem.fin < DateTime.Now)
                    return "Rien à confirmer (ou plus de 60 s écoulées).";
                m_confirmations.Remove(c.Id);

                if (c.Guild?.Alliance == null || !Gestionnaire(c))
                    return "Il faut le droit de gérer les prismes de l'alliance.";

                switch (dem.action)
                {
                    case "pnj": return InstallerPnj(c, dem.valeur);
                    case "acheter": return Acheter(c, dem.valeur);
                    case "revendre": return Revendre(c);
                    default: return "Rien à confirmer.";
                }
            }
        }

        public void Parler(Character c, VillageNpc acteur)
        {
            var pnj = acteur.Donnees;
            if (c.Guild?.Alliance == null || c.Guild.Alliance.Id != pnj.AllianceId)
            {
                AvAManager.Message(c, $"Le {NomType(pnj.Type)} est réservé à l'alliance {AvAManager.Lien(AllianceManager.Instance.TryGetAlliance(pnj.AllianceId))}.");
                return;
            }

            List<Destination> destinations;
            lock (m_lock)
                destinations = LicencesDe(pnj.AllianceId).Where(x => x.SubAreaId == pnj.SubAreaId && m_destinations.ContainsKey(x.DestId))
                    .Select(x => m_destinations[x.DestId]).Where(d => World.Instance.GetMap(d.MapId) != null).ToList();

            if (destinations.Count == 0)
            {
                AvAManager.Message(c, "Aucune destination débloquée. Gestion : .village destinations / .village acheter <numéro> (payé par le coffre, .coffre).");
                return;
            }

            if (c.Dialog != null)
                c.Dialog.Close();
            new VillageTeleportDialog(c, destinations).Open();
        }

        #endregion
    }

    /// <summary>
    /// MYSTIC 17/10 : « Téléporteur de l'alliance » (PNJ 2984 renommé dans les textes du client), apparence du Krosmoglob {2625|||75}.
    /// </summary>
    public class VillageNpc : RolePlayActor, IInteractNpc
    {
        public const int NpcIdBrigandin = 2984;

        public VillageNpc(int id, VillagesConquete.Pnj donnees, ObjectPosition position)
        {
            Id = id;
            Donnees = donnees;
            Position = position;
            Look = LookDe(AllianceManager.Instance.TryGetAlliance(donnees.AllianceId));
        }

        public VillagesConquete.Pnj Donnees { get; }

        /// <summary>Apparence officielle du Krosmoglob (PNJ « Krosmoglob des Voyageurs » 2435), choisie par l'utilisateur.</summary>
        public static ActorLook LookDe(Alliance alliance) => ActorLook.Parse("{2625|||75}");

        public override int Id { get; protected set; }

        public override ActorLook Look { get; set; }

        public override GameContextActorInformations GetGameContextActorInformations(Character character) =>
            new GameRolePlayNpcInformations(Id, Look.GetEntityLook(), GetEntityDispositionInformations(), NpcIdBrigandin, false, 0);

        public bool CanInteractWith(NpcActionTypeEnum action, Character dialoguer) => action == NpcActionTypeEnum.ACTION_TALK;

        public void InteractWith(NpcActionTypeEnum actionType, Character dialoguer)
        {
            if (CanInteractWith(actionType, dialoguer))
                VillagesConquete.Instance.Parler(dialoguer, this);
        }
    }

    /// <summary>MYSTIC 17/10 : liste des destinations débloquées (fenêtre de téléportation du client), gratuite.</summary>
    public class VillageTeleportDialog : IDialog
    {
        readonly List<VillagesConquete.Destination> m_destinations;

        public VillageTeleportDialog(Character character, List<VillagesConquete.Destination> destinations)
        {
            Character = character;
            m_destinations = destinations;
        }

        public Character Character { get; }

        public DialogTypeEnum DialogType => DialogTypeEnum.DIALOG_TELEPORTER;

        public void Open()
        {
            Character.SetDialog(this);
            Character.Client.Send(new TeleportDestinationsMessage((sbyte)TeleporterTypeEnum.TELEPORTER_ZAAP,
                m_destinations.Select(d => World.Instance.GetMap(d.MapId)).Where(m => m?.SubArea != null)
                    .Select(m => new TeleportDestination((sbyte)TeleporterTypeEnum.TELEPORTER_ZAAP, m.Id, (ushort)m.SubArea.Id, (ushort)m.SubArea.Record.Level, 0))
                    .ToArray()));
        }

        public void Close()
        {
            Character.CloseDialog(this);
            DialogHandler.SendLeaveDialogMessage(Character.Client, DialogType);
        }

        public void Teleport(Map map)
        {
            var d = m_destinations.FirstOrDefault(x => x.MapId == map.Id);
            if (d == null)
                return;

            var bloque = AvAManager.Instance.ZaapBloque(Character, Character.Map, map);
            if (bloque != null)
            {
                AvAManager.Message(Character, bloque);
                return;
            }

            var cell = (d.CellId > 0 ? map.Cells[d.CellId] : null);
            if (cell == null || !cell.Walkable)
                cell = map.GetRandomFreeCell(false, true);
            Close();
            Character.Teleport(map, cell);
        }
    }
}
