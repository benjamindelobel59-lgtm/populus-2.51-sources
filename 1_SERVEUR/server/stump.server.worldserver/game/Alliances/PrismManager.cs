using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using Stump.Core.Extensions;
using Stump.Core.Reflection;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Messages;
using Stump.DofusProtocol.Types;
using Stump.Server.WorldServer.Game.Actors.Look;
using Stump.Server.WorldServer.Game.Actors.RolePlay;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Actors.Interfaces;
using Stump.Server.WorldServer.Game.Maps;
using Stump.Server.WorldServer.Game.Maps.Cells;
using Stump.Server.WorldServer.Handlers.Dialogs;
using Stump.Server.WorldServer.Game.Dialogs;

namespace Stump.Server.WorldServer.Game.Alliances
{
    /// <summary>MYSTIC 17/10 : prisme d'alliance posé sur une sous-zone (phase 2 : territoires).</summary>
    public class PrismeAlliance
    {
        public int SubAreaId { get; set; }
        public int AllianceId { get; set; }
        public int MapId { get; set; }
        public int CellId { get; set; }
        public int State { get; set; } = (int)PrismStateEnum.PRISM_STATE_NORMAL;
        public DateTime PlacementDate { get; set; }
        public int VulnHour { get; set; }

        /// <summary>
        /// MYSTIC 17/10 : début de la vulnérabilité en quarts d'heure UTC depuis minuit (0 à 95), unité du client (AllianceAreas :
        /// PrismSettingsRequest.startDefenseTime). -1 = ancienne donnée en heure locale (VulnHour), convertie au chargement.
        /// </summary>
        public int VulnQuarter { get; set; } = -1;

        /// <summary>MYSTIC 17/10 (AvA) : date de vulnérabilité fixée quand le prisme est affaibli ou saboté.</summary>
        public DateTime? VulnerableAt { get; set; }
        public DateTime LastTimeSlotModificationDate { get; set; }
        public int AuthorGuildId { get; set; }
        public int AuthorId { get; set; }
        public string AuthorName { get; set; }
        public int Pepites { get; set; }

        /// <summary>MYSTIC 17/10 (phase 4) : modules installés (ids des objets, séparés par des virgules).</summary>
        public string Modules { get; set; }

        /// <summary>MYSTIC 17/10 : invulnérable jusqu'à cette date (24 h après la pose, 5 h après une défense gagnée).</summary>
        public DateTime? InvulnerableUntil { get; set; }

        public PrismNpc Npc { get; set; }

        public Alliance Alliance => AllianceManager.Instance.TryGetAlliance(AllianceId);

        public Map Map => World.Instance.GetMap(MapId);

        /// <summary>Prochaine vulnérabilité : à l'heure choisie, au moins 24 h après la pose (règle officielle).</summary>
        public DateTime NextVulnerabilityDate
        {
            get
            {
                if (VulnerableAt.HasValue)
                    return VulnerableAt.Value;

                var mini = PlacementDate.ToUniversalTime().AddHours(24);
                var maintenant = DateTime.UtcNow;
                var depart = maintenant > mini ? maintenant : mini;
                var date = depart.Date.AddMinutes(VulnQuarter * 15);
                if (date < depart)
                    date = date.AddDays(1);
                return DateTime.SpecifyKind(date, DateTimeKind.Utc).ToLocalTime();
            }
        }

        public PrismInformation GetPrismInformation(bool interne)
        {
            var alliance = Alliance;
            if (interne)
                return new AllianceInsiderPrismInformation(0, (sbyte)State, NextVulnerabilityDate.GetUnixTimeStamp(), PlacementDate.GetUnixTimeStamp(),
                    (uint)Pepites, LastTimeSlotModificationDate.GetUnixTimeStamp(), (uint)AuthorGuildId, (ulong)AuthorId, AuthorName ?? "", PrismModules.Objets(this));

            return new AlliancePrismInformation(0, (sbyte)State, NextVulnerabilityDate.GetUnixTimeStamp(), PlacementDate.GetUnixTimeStamp(),
                (uint)Pepites, alliance?.GetAllianceInformations() ?? new AllianceInformations(0, "", "#NONAME#", new DofusProtocol.Types.GuildEmblem()));
        }

        public PrismGeolocalizedInformation GetGeolocalized(bool interne)
        {
            var map = Map;
            return new PrismGeolocalizedInformation((ushort)SubAreaId, (uint)AllianceId, (short)(map?.Position.X ?? 0), (short)(map?.Position.Y ?? 0),
                MapId, GetPrismInformation(interne));
        }
    }

    /// <summary>MYSTIC 17/10 : le prisme sur la carte (apparence officielle {2211}, PNJ 2141).</summary>
    public class PrismNpc : RolePlayActor, IInteractNpc
    {
        public PrismNpc(int id, PrismeAlliance prisme, ObjectPosition position)
        {
            Id = id;
            Prisme = prisme;
            Position = position;
            Look = LookDe(prisme.Alliance, prisme.State);
        }

        /// <summary>
        /// MYSTIC 17/10 : apparence du prisme. Le graphisme officiel (os 2211) colore la sphère (noyau) avec la couleur 1 et le logo
        /// avec la couleur 9, et accroche le blason d'alliance (« AllianceUp ») : couleur du noyau selon l'état, blason de l'alliance
        /// (motif du blason en skin, couleurs 7 / 8 / 9 comme le percepteur). Couleurs 2 à 4 = celles du monstre officiel 3451.
        /// </summary>
        public static ActorLook LookDe(Alliance alliance, int etat = (int)PrismStateEnum.PRISM_STATE_NORMAL)
        {
            var look = ActorLook.Parse("{2211||2=16752644,3=14301485,4=12148110}");
            look.AddColor(1, System.Drawing.Color.FromArgb(CouleurNoyau(etat)));
            if (alliance?.Emblem == null)
                return look;

            var motif = Guilds.GuildManager.Instance.TryGetEmblem(alliance.Emblem.SymbolShape);
            if (motif != null)
                look.AddSkin((short)motif.SkinId);
            look.AddColor(9, System.Drawing.Color.FromArgb(alliance.Emblem.SymbolColor));
            look.AddColor(8, System.Drawing.Color.FromArgb(alliance.Emblem.SymbolColor));
            look.AddColor(7, System.Drawing.Color.FromArgb(alliance.Emblem.BackgroundColor));
            return look;
        }

        /// <summary>Couleur du noyau selon l'état (proche des couleurs de la carte des conquêtes).</summary>
        public static int CouleurNoyau(int etat)
        {
            switch ((PrismStateEnum)etat)
            {
                case PrismStateEnum.PRISM_STATE_ATTACKED:
                case PrismStateEnum.PRISM_STATE_FIGHTING:
                    return 0xFF8C00; // orange : attaqué
                case PrismStateEnum.PRISM_STATE_WEAKENED:
                    return 0xFFD700; // jaune : affaibli
                case PrismStateEnum.PRISM_STATE_VULNERABLE:
                    return 0xE01010; // rouge : vulnérable (KOH)
                case PrismStateEnum.PRISM_STATE_SABOTAGED:
                    return 0x9B30FF; // violet : saboté
                default:
                    return 0xC2E6EE; // bleu clair officiel : normal
            }
        }

        public PrismeAlliance Prisme { get; }

        public override int Id { get; protected set; }

        public override ActorLook Look { get; set; }

        public override GameContextActorInformations GetGameContextActorInformations(Character character)
        {
            var interne = character?.Guild?.Alliance != null && character.Guild.Alliance.Id == Prisme.AllianceId;
            return new GameRolePlayPrismInformations(Id, Look.GetEntityLook(), GetEntityDispositionInformations(), Prisme.GetPrismInformation(interne));
        }

        public bool CanInteractWith(NpcActionTypeEnum action, Character dialoguer) => action == NpcActionTypeEnum.ACTION_TALK;

        public void InteractWith(NpcActionTypeEnum actionType, Character dialoguer)
        {
            if (!CanInteractWith(actionType, dialoguer))
                return;

            new PrismDialog(dialoguer, this).Open();
        }
    }

    /// <summary>MYSTIC 17/10 : dialogue ouvert en cliquant sur un prisme (menu du client : utiliser, et en phase 3 attaquer / saboter).</summary>
    public class PrismDialog : IDialog
    {
        public PrismDialog(Character character, PrismNpc prism)
        {
            Character = character;
            Prism = prism;
        }

        public Character Character { get; }

        public PrismNpc Prism { get; }

        public DialogTypeEnum DialogType => DialogTypeEnum.DIALOG_DIALOG;

        public void Open()
        {
            Character.SetDialog(this);
            Character.Client.Send(new NpcDialogCreationMessage(Prism.Map.Id, Prism.Id));
            Character.Client.Send(new AlliancePrismDialogQuestionMessage());
        }

        public void Close()
        {
            Character.CloseDialog(this);
            DialogHandler.SendLeaveDialogMessage(Character.Client, DialogType);
        }
    }

    /// <summary>MYSTIC 17/10 : téléportation d'un prisme de l'alliance vers ses autres prismes (bonus choisi : prisme = zaap).</summary>
    public class PrismTeleportDialog : IDialog
    {
        readonly List<PrismeAlliance> m_destinations;

        public PrismTeleportDialog(Character character, IEnumerable<PrismeAlliance> destinations)
        {
            Character = character;
            m_destinations = destinations.ToList();
        }

        public Character Character { get; }

        public DialogTypeEnum DialogType => DialogTypeEnum.DIALOG_TELEPORTER;

        public void Open()
        {
            Character.SetDialog(this);
            Character.Client.Send(new TeleportDestinationsMessage((sbyte)TeleporterTypeEnum.TELEPORTER_PRISM,
                m_destinations.Where(x => x.Map != null && Maps.AccesFrigost.PrismeAutorise(Character, x.Map, false)).Select(x => new TeleportDestination((sbyte)TeleporterTypeEnum.TELEPORTER_PRISM, x.MapId,
                    (ushort)x.SubAreaId, (ushort)x.Map.SubArea.Record.Level, 0)).ToArray()));
        }

        public void Close()
        {
            Character.CloseDialog(this);
            DialogHandler.SendLeaveDialogMessage(Character.Client, DialogType);
        }

        public void Teleport(Map map)
        {
            var prisme = m_destinations.FirstOrDefault(x => x.MapId == map.Id);
            if (prisme == null)
                return;

            // PADGREF 24/10 : quêtes d'accès Frigost / Otomaï et zones de Frigost
            if (!Maps.AccesFrigost.PrismeAutorise(Character, map, true))
                return;

            var bloque = AvAManager.Instance.ZaapBloque(Character, Character.Map, map);
            if (bloque != null)
            {
                Character.SendServerMessage(bloque);
                return;
            }

            var cell = map.GetRandomAdjacentFreeCell(new MapPoint((short)prisme.CellId), true) ?? map.Cells[prisme.CellId];
            Close();
            Character.Teleport(map, cell);
        }
    }

    /// <summary>
    /// MYSTIC 17/10 : prismes d'alliance (table nekfeu_prisms, créée au démarrage). Pose avec l'objet Prisme d'alliance (14293) dans une
    /// sous-zone capturable sans prisme ; heure de vulnérabilité réglable ; bonus XP / drop pour l'alliance dans ses sous-zones ;
    /// téléportation entre les prismes de l'alliance. Attaques, sabotage, KOH : phase 3.
    /// </summary>
    public class PrismManager : Singleton<PrismManager>
    {
        static readonly Logger logger = LogManager.GetCurrentClassLogger();

        static Stump.ORM.Database Database => WorldServer.Instance.DBAccessor.Database;

        /// <summary>Bonus d'XP et de drop pour les membres d'une alliance dans ses sous-zones (choix utilisateur : 10 %).</summary>
        public static double BonusTerritoire = 0.10;

        // sous-zones capturables d'après le client 2.51 (SubAreas.d2o, champ capturable)
        static readonly HashSet<int> Capturables = new HashSet<int>
        {
            1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 22, 23, 27, 31, 32, 33, 43, 44, 46, 47, 48, 49, 50, 51, 54, 55, 56, 57, 59, 61, 62, 68, 69, 70,
            71, 73, 75, 76, 93, 103, 105, 106, 107, 108, 109, 115, 116, 117, 118, 119, 143, 152, 161, 162, 163, 164, 165, 166, 167, 168, 169, 170,
            171, 178, 179, 180, 181, 182, 209, 230, 231, 232, 233, 234, 235, 253, 275, 276, 277, 279, 280, 314, 315, 316, 319, 334, 453, 454, 455,
            457, 461, 464, 465, 466, 468, 469, 471, 472, 476, 479, 480, 481, 482, 485, 490, 492, 495, 502, 503, 505, 506, 507, 508, 509, 511, 513,
            517, 518, 519, 521, 522, 523, 525, 526, 529, 530, 531, 532, 533, 534, 535, 600, 601, 602, 603, 604, 605, 606, 608, 609, 610, 611, 615,
            622, 651, 652, 753, 777, 785, 786, 787, 788, 789, 798, 799, 804, 805, 806, 808, 809, 813, 816, 817, 820, 821, 822, 826, 827, 828, 829,
            833, 834, 836, 837, 838, 850, 855, 856, 857, 860, 861, 862, 863, 864, 865, 866, 867, 868, 872, 873, 874, 878, 879, 881, 883, 886, 889,
            892, 896, 897, 902
        };

        // MYSTIC 17/10 : zones interdites aux prismes (demande utilisateur) : Astrub (18, dont Tainéla et les Calanques) et Incarnam (45)
        static readonly HashSet<int> ZonesInterdites = new HashSet<int> { 18, 45 };

        // cartes devant les donjons (dungeons_entries.EntranceMapId), interdites aux prismes
        readonly HashSet<int> m_entreesDonjons = new HashSet<int>();

        readonly Dictionary<int, PrismeAlliance> m_prismes = new Dictionary<int, PrismeAlliance>();
        readonly object m_lock = new object();

        public static bool EstCapturable(int subAreaId) => Capturables.Contains(subAreaId);

        /// <summary>Heure locale (0-23) vers quarts d'heure UTC (0-95).</summary>
        static int QuartsUtc(int heureLocale)
        {
            var decalage = (int)TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalMinutes;
            return (((heureLocale * 60 - decalage) / 15) % 96 + 96) % 96;
        }

        /// <summary>Quarts d'heure UTC vers heure locale (colonne VulnHour, lisible dans la base).</summary>
        static int HeureLocale(int quarts)
        {
            var decalage = (int)TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalMinutes;
            return ((quarts * 15 + decalage) / 60 % 24 + 24) % 24;
        }

        public IEnumerable<PrismeAlliance> Prismes
        {
            get { lock (m_lock) return m_prismes.Values.ToArray(); }
        }

        public PrismeAlliance TryGetPrisme(int subAreaId)
        {
            lock (m_lock)
                return m_prismes.TryGetValue(subAreaId, out var p) ? p : null;
        }

        public IEnumerable<PrismeAlliance> PrismesDe(Alliance alliance) => Prismes.Where(x => x.AllianceId == alliance.Id);

        public static void Executer(string sql, params object[] args)
        {
            WorldServer.Instance.IOTaskPool.AddMessage(() => Database.Execute(sql, args));
        }

        /// <summary>Appelé à la fin du chargement des alliances.</summary>
        public void Charger()
        {
            Database.Execute(@"CREATE TABLE IF NOT EXISTS nekfeu_prisms (SubAreaId INT NOT NULL PRIMARY KEY, AllianceId INT NOT NULL, MapId INT NOT NULL,
                CellId INT NOT NULL, State INT NOT NULL, PlacementDate DATETIME NOT NULL, VulnHour INT NOT NULL, LastTimeSlotModificationDate DATETIME NOT NULL,
                AuthorGuildId INT NOT NULL, AuthorId INT NOT NULL, AuthorName VARCHAR(64) NULL, Pepites INT NOT NULL DEFAULT 0) DEFAULT CHARSET=utf8");
            Database.Execute("ALTER TABLE nekfeu_prisms ADD COLUMN IF NOT EXISTS VulnQuarter INT NOT NULL DEFAULT -1");
            Database.Execute("ALTER TABLE nekfeu_prisms ADD COLUMN IF NOT EXISTS VulnerableAt DATETIME NULL");
            Database.Execute("ALTER TABLE nekfeu_prisms ADD COLUMN IF NOT EXISTS Modules VARCHAR(255) NULL");
            Database.Execute("ALTER TABLE nekfeu_prisms ADD COLUMN IF NOT EXISTS InvulnerableUntil DATETIME NULL");

            foreach (var mapId in Database.Fetch<int>("SELECT DISTINCT EntranceMapId FROM dungeons_entries"))
                m_entreesDonjons.Add(mapId);

            foreach (var p in Database.Fetch<PrismeAlliance>("SELECT * FROM nekfeu_prisms"))
            {
                if (p.Alliance == null || p.Map == null)
                {
                    Executer("DELETE FROM nekfeu_prisms WHERE SubAreaId = @0", p.SubAreaId);
                    continue;
                }

                if (p.VulnQuarter < 0 || p.VulnQuarter > 95)
                {
                    p.VulnQuarter = QuartsUtc(p.VulnHour);
                    Executer("UPDATE nekfeu_prisms SET VulnQuarter = @1 WHERE SubAreaId = @0", p.SubAreaId, p.VulnQuarter);
                }

                m_prismes[p.SubAreaId] = p;
                Apparaitre(p);
            }

            logger.Info("[Alliances] {0} prismes", m_prismes.Count);

            PrismModules.Instance.Charger();
            VillagesConquete.Instance.Charger();
            AvAManager.Instance.Demarrer();
        }

        void Apparaitre(PrismeAlliance p)
        {
            var map = p.Map;
            if (map == null)
                return;

            var npc = new PrismNpc(map.GetNextContextualId(), p, new ObjectPosition(map, map.Cells[p.CellId], DirectionsEnum.DIRECTION_SOUTH_EAST));
            p.Npc = npc;
            map.Enter(npc);
        }

        void Disparaitre(PrismeAlliance p)
        {
            if (p.Npc == null)
                return;

            p.Npc.Map?.Leave(p.Npc);
            p.Npc = null;
        }

        /// <summary>Pose d'un prisme par un membre autorisé (objet 14293). Renvoie un message d'erreur, ou null si posé.</summary>
        public string Poser(Character character)
        {
            var alliance = character.Guild?.Alliance;
            if (alliance == null)
                return "Il faut faire partie d'une alliance.";

            if (!Conditions.Criterions.AllianceRightsCriterion.AUnDroit(character, AllianceRightsBitEnum.ALLIANCE_RIGHT_MANAGE_PRISMS))
                return "Seuls le chef de l'alliance et les meneurs de ses guildes peuvent poser un prisme.";

            if (character.IsFighting() || character.IsBusy())
                return "Impossible pour le moment.";

            var map = character.Map;
            var subArea = map.SubArea;
            if (subArea == null || !EstCapturable(subArea.Id))
                return "Cette zone ne peut pas accueillir de prisme.";

            if (map.IsDungeon() || character.Record.IsInHavenBag)
                return "Impossible de poser un prisme ici.";

            // MYSTIC 17/10 : village de conquête : le prisme se pose uniquement à la porte (carte officielle allowPrism)
            if (VillagesConquete.Villages.Contains(subArea.Id) && !VillagesConquete.EstPorte(map))
                return "Dans un village de conquête, le prisme se pose uniquement à la porte du village.";

            // MYSTIC 17/10 : interdits sur une carte avec un zaap, à Astrub / Incarnam et devant les donjons
            if (map.Zaap != null)
                return "Impossible de poser un prisme sur la carte d'un zaap.";

            if (ZonesInterdites.Contains(subArea.Record.AreaId))
                return "Les prismes sont interdits à Astrub et à Incarnam.";

            if (m_entreesDonjons.Contains(map.Id))
                return "Impossible de poser un prisme devant l'entrée d'un donjon.";

            var cell = map.GetRandomAdjacentFreeCell(character.Position.Point, true);
            if (cell == null)
                return "Il n'y a pas de place libre à côté de vous.";

            var p = new PrismeAlliance
            {
                SubAreaId = subArea.Id,
                AllianceId = alliance.Id,
                MapId = map.Id,
                CellId = cell.Id,
                PlacementDate = DateTime.Now,
                VulnHour = 20,
                VulnQuarter = QuartsUtc(20),
                LastTimeSlotModificationDate = DateTime.Now,
                InvulnerableUntil = DateTime.Now.AddHours(24),
                AuthorGuildId = character.Guild.Id,
                AuthorId = character.Id,
                AuthorName = character.Name,
            };

            if (AvAManager.Instance.ZoneReserveePourAutre(p.SubAreaId, alliance.Id, out var reservee))
                return reservee;

            lock (m_lock)
            {
                if (m_prismes.ContainsKey(p.SubAreaId))
                    return "Cette zone a déjà un prisme.";
                m_prismes[p.SubAreaId] = p;
            }

            Executer(@"REPLACE INTO nekfeu_prisms (SubAreaId, AllianceId, MapId, CellId, State, PlacementDate, VulnHour, LastTimeSlotModificationDate,
                AuthorGuildId, AuthorId, AuthorName, Pepites, VulnQuarter, InvulnerableUntil) VALUES (@0, @1, @2, @3, @4, @5, @6, @7, @8, @9, @10, @11, @12, @13)",
                p.SubAreaId, p.AllianceId, p.MapId, p.CellId, p.State, p.PlacementDate, p.VulnHour, p.LastTimeSlotModificationDate,
                p.AuthorGuildId, p.AuthorId, p.AuthorName, p.Pepites, p.VulnQuarter, p.InvulnerableUntil);

            Apparaitre(p);
            Annoncer(p);
            AvAManager.MessageATous($"La zone {AvAManager.Lien(p.SubAreaId)} a été prismée par l'alliance {AvAManager.Lien(alliance)} !");
            return null;
        }

        /// <summary>MYSTIC 17/10 (AvA) : enregistre l'état et la date de vulnérabilité, et prévient tout le monde.</summary>
        public void SauverEtat(PrismeAlliance p)
        {
            Executer("UPDATE nekfeu_prisms SET State = @1, VulnerableAt = @2, InvulnerableUntil = @3 WHERE SubAreaId = @0", p.SubAreaId, p.State, p.VulnerableAt,
                p.InvulnerableUntil);
            Annoncer(p);
        }

        public void Retirer(PrismeAlliance p)
        {
            lock (m_lock)
                m_prismes.Remove(p.SubAreaId);

            Disparaitre(p);
            Executer("DELETE FROM nekfeu_prisms WHERE SubAreaId = @0", p.SubAreaId);
            Annoncer(p, true);
        }

        /// <summary>L'alliance est dissoute : ses prismes disparaissent.</summary>
        public void OnAllianceDeleted(Alliance alliance)
        {
            VillagesConquete.Instance.SurAllianceSupprimee(alliance.Id);
            foreach (var p in PrismesDe(alliance).ToArray())
                Retirer(p);
        }

        /// <summary>Réglage du début de la vulnérabilité (quarts d'heure UTC 0 à 95, comme l'envoie le client), prisme normal seulement.</summary>
        public bool ReglerHeure(Character character, int subAreaId, int quarts)
        {
            var p = TryGetPrisme(subAreaId);
            var alliance = character.Guild?.Alliance;
            if (p == null || alliance == null || p.AllianceId != alliance.Id || quarts < 0 || quarts > 95 ||
                p.State != (int)PrismStateEnum.PRISM_STATE_NORMAL ||
                !Conditions.Criterions.AllianceRightsCriterion.AUnDroit(character, AllianceRightsBitEnum.ALLIANCE_RIGHT_MANAGE_PRISMS))
                return false;

            p.VulnQuarter = quarts;
            p.VulnHour = HeureLocale(quarts);
            p.LastTimeSlotModificationDate = DateTime.Now;
            p.AuthorGuildId = character.Guild.Id;
            p.AuthorId = character.Id;
            p.AuthorName = character.Name;
            Executer("UPDATE nekfeu_prisms SET VulnHour = @1, LastTimeSlotModificationDate = @2, AuthorGuildId = @3, AuthorId = @4, AuthorName = @5, VulnQuarter = @6 WHERE SubAreaId = @0",
                p.SubAreaId, p.VulnHour, p.LastTimeSlotModificationDate, p.AuthorGuildId, p.AuthorId, p.AuthorName, p.VulnQuarter);
            Annoncer(p);
            return true;
        }

        /// <summary>Mise à jour de la liste des prismes pour tous les joueurs connectés (carte du monde, interface d'alliance).</summary>
        public void Annoncer(PrismeAlliance p, bool retire = false)
        {
            foreach (var perso in World.Instance.GetCharacters())
            {
                var interne = perso.Guild?.Alliance != null && perso.Guild.Alliance.Id == p.AllianceId;
                var info = retire ? new PrismSubareaEmptyInfo((ushort)p.SubAreaId, 0) : (PrismSubareaEmptyInfo)p.GetGeolocalized(interne);
                perso.Client.Send(new PrismsListUpdateMessage(new[] { info }));
            }

            if (!retire)
            {
                if (p.Npc != null)
                    p.Npc.Look = PrismNpc.LookDe(p.Alliance, p.State);
                p.Npc?.Map?.Refresh(p.Npc);
            }
        }

        public void EnvoyerListe(Character character)
        {
            var allianceId = character.Guild?.Alliance?.Id ?? -1;
            character.Client.Send(new PrismsListMessage(Prismes.Select(x => (PrismSubareaEmptyInfo)x.GetGeolocalized(x.AllianceId == allianceId)).ToArray()));
        }

        /// <summary>Bonus d'XP / drop du personnage sur cette carte (0 ou BonusTerritoire).</summary>
        public double Bonus(int characterId, Map map)
        {
            if (map?.SubArea == null)
                return 0;

            var p = TryGetPrisme(map.SubArea.Id);
            if (p == null)
                return 0;

            var character = World.Instance.GetCharacter(characterId);
            return character?.Guild?.Alliance != null && character.Guild.Alliance.Id == p.AllianceId ? BonusTerritoire : 0;
        }

        /// <summary>Utilisation d'un prisme de son alliance : téléportation vers les autres prismes de l'alliance.</summary>
        public void Utiliser(Character character)
        {
            var prism = character.Map.GetActors<PrismNpc>().FirstOrDefault();
            var alliance = character.Guild?.Alliance;
            if (prism == null || alliance == null || prism.Prisme.AllianceId != alliance.Id)
                return;

            if (character.Dialog is PrismDialog)
                character.Dialog.Close();

            new PrismTeleportDialog(character, PrismesDe(alliance).Where(x => x != prism.Prisme)).Open();
        }
    }
}
