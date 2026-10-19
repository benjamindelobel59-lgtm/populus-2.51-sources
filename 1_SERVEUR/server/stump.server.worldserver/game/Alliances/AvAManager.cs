using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using Stump.Core.Extensions;
using Stump.Core.Reflection;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Messages;
using Stump.DofusProtocol.Types;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Monsters;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Maps;
using Stump.Server.WorldServer.Game.Maps.Cells;

namespace Stump.Server.WorldServer.Game.Alliances
{
    /// <summary>
    /// MYSTIC 17/10 : AvA (alliances phase 3). Règles officielles retenues :
    /// - attaque d'un prisme normal en 5 contre 5 (FightPvPr) ; prisme tué = affaibli ;
    /// - sabotage par un membre autorisé de l'alliance propriétaire (déclenche lui-même le KOH) ;
    /// - un prisme affaibli / saboté devient vulnérable au premier créneau choisi au moins 24 h après (commande .koh pour tester) ;
    /// - KOH : chaque minute, 1 point à l'alliance qui tient le plus de cartes de la sous-zone (personnages en mode AvA, présents
    ///   1 min 30 sur la carte, hors combat ; égalité départagée par la somme des niveaux) ; 30 points = victoire ; 2 h maximum
    ///   (sinon le propriétaire garde la zone) ;
    /// - mode AvA toujours actif pour les membres d'une alliance (demande utilisateur 17/10) : agression entre alliances dans la zone
    ///   vulnérable (FightKoh), le perdant est éliminé du KOH ; une seule participation par personnage à la fois ;
    /// - en entrant dans la zone vulnérable : horloge au-dessus de la tête pendant 1 min, puis le personnage compte pour les points ;
    /// - zaaps / zaapis / prismes bloqués pour entrer ou sortir d'une zone vulnérable (membres d'une alliance) ; connexion dans une
    ///   zone vulnérable = renvoi au zaap de sauvegarde (ou au zaap d'Astrub s'il est dans la zone) ;
    /// - victoire d'une autre alliance : le prisme disparaît, la zone lui est réservée 2 h pour poser son prisme.
    /// </summary>
    public class AvAManager : Singleton<AvAManager>
    {
        static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public const int PointsVictoire = 30;
        public static readonly TimeSpan DureeMax = TimeSpan.FromHours(2);
        public static readonly TimeSpan PresenceMini = TimeSpan.FromSeconds(60);
        public static readonly TimeSpan DelaiAvantVulnerabilite = TimeSpan.FromHours(24);
        public static readonly TimeSpan Reservation = TimeSpan.FromHours(2);
        const int TickMs = 5000;
        const int TicksParPoint = 12; // 12 × 5 s = 1 point par minute
        public const int CarteZaapAstrub = 191104002;

        class Koh
        {
            public PrismeAlliance Prisme;
            public DateTime Debut;
            public int Ticks;
            public DateTime ProchainPoint;
            public readonly Dictionary<int, int> Points = new Dictionary<int, int>();
            public readonly Dictionary<int, DateTime> Presences = new Dictionary<int, DateTime>(); // arrivée dans la zone
            public readonly HashSet<int> Elimines = new HashSet<int>();
            public readonly Dictionary<int, int> CartesDuTour = new Dictionary<int, int>();
            public readonly Dictionary<int, int> GagnantParCarte = new Dictionary<int, int>();
        }

        readonly object m_lock = new object();
        readonly Dictionary<int, sbyte> m_statuts = new Dictionary<int, sbyte>();
        readonly Dictionary<int, Koh> m_koh = new Dictionary<int, Koh>();
        readonly Dictionary<int, FightPvPr> m_combats = new Dictionary<int, FightPvPr>();
        readonly Dictionary<int, (int alliance, DateTime fin)> m_reservations = new Dictionary<int, (int, DateTime)>();
        bool m_demarre;

        public void Demarrer()
        {
            if (m_demarre)
                return;
            m_demarre = true;

            // un KOH en cours au moment de l'arrêt du serveur reprend (même heure de début : 2 h maximum au total ; points perdus)
            foreach (var p in PrismManager.Instance.Prismes.Where(x => x.State == (int)PrismStateEnum.PRISM_STATE_VULNERABLE).ToArray())
                DemarrerKoh(p, p.VulnerableAt ?? DateTime.Now);

            WorldServer.Instance.IOTaskPool.CallPeriodically(TickMs, Tick);
        }

        #region Mode AvA

        public bool EstEnModeAvA(Character c) => c?.Guild?.Alliance != null;

        public static string NomZone(int subAreaId) => World.Instance.GetSubArea(subAreaId)?.Record?.Name ?? subAreaId.ToString();

        /// <summary>Lien de chat officiel du client : affiche [Nom de la zone], un clic l'ouvre sur la carte.</summary>
        public static string Lien(int subAreaId) => "{subArea," + subAreaId + "}";

        public static readonly System.Drawing.Color CouleurAlliance = System.Drawing.Color.FromArgb(0xFF, 0xAD, 0x42);

        /// <summary>Message AvA au style des messages officiels du client (gras, couleur du canal alliance).</summary>
        public static void Message(Character c, string texte) => c?.SendServerMessage("<b>" + texte + "</b>", CouleurAlliance);

        public static void MessageATous(string texte) => World.Instance.SendAnnounce("<b>" + texte + "</b>", CouleurAlliance);

        /// <summary>Lien de chat officiel vers une alliance : un clic ouvre sa fiche.</summary>
        public static string Lien(Alliance alliance) =>
            alliance == null ? "?" : "{alliance," + alliance.Id + "," + alliance.Tag + "::" + alliance.Name + "}";

        Koh KohDe(int subAreaId)
        {
            lock (m_lock)
                return m_koh.TryGetValue(subAreaId, out var k) ? k : null;
        }

        public bool ZoneVulnerable(Map map) => map?.SubArea != null && KohDe(map.SubArea.Id) != null;

        /// <summary>Statut d'agression affiché (HumanOptionAlliance, UpdateSelfAgressableStatus).</summary>
        public sbyte Statut(Character c)
        {
            if (!EstEnModeAvA(c))
                return (sbyte)AggressableStatusEnum.NON_AGGRESSABLE;

            var koh = c.Map?.SubArea == null ? null : KohDe(c.Map.SubArea.Id);
            if (koh == null)
                return (sbyte)AggressableStatusEnum.AvA_ENABLED_NON_AGGRESSABLE;

            lock (m_lock)
            {
                if (koh.Elimines.Contains(c.Id))
                    return (sbyte)AggressableStatusEnum.AvA_DISQUALIFIED;

                // horloge au-dessus de la tête tant que la minute de présence n'est pas faite
                if (!koh.Presences.TryGetValue(c.Id, out var depuis) || DateTime.Now - depuis < PresenceMini)
                    return (sbyte)AggressableStatusEnum.AvA_PREQUALIFIED_AGGRESSABLE;
            }

            return (sbyte)AggressableStatusEnum.AvA_ENABLED_AGGRESSABLE;
        }

        /// <summary>Fin de la minute de présence (compte à rebours de l'horloge dans l'interface du KOH), 0 sinon.</summary>
        public int FinPresence(Character c)
        {
            var koh = c?.Map?.SubArea == null ? null : KohDe(c.Map.SubArea.Id);
            if (koh == null)
                return 0;

            lock (m_lock)
                return ((koh.Presences.TryGetValue(c.Id, out var depuis) ? depuis : DateTime.Now) + PresenceMini).GetUnixTimeStamp();
        }

        public void EnvoyerStatut(Character c)
        {
            var statut = Statut(c);
            lock (m_lock)
                m_statuts[c.Id] = statut;

            c.Client.Send(new UpdateSelfAgressableStatusMessage(statut,
                statut == (sbyte)AggressableStatusEnum.AvA_PREQUALIFIED_AGGRESSABLE ? FinPresence(c) : 0));
            if (!c.IsFighting())
                c.RefreshActor();
        }

        /// <summary>Renvoie le statut seulement s'il a changé (tick du KOH).</summary>
        void MajStatut(Character c)
        {
            if (c == null || !c.IsInWorld)
                return;

            var statut = Statut(c);
            lock (m_lock)
                if (m_statuts.TryGetValue(c.Id, out var ancien) && ancien == statut)
                    return;

            EnvoyerStatut(c);
        }

        /// <summary>Arrivée sur une carte : la carte envoie déjà le statut du moment, on le mémorise.</summary>
        public void SurArrivee(Character c)
        {
            if (c?.Guild?.Alliance == null)
                return;

            var statut = Statut(c);
            lock (m_lock)
                m_statuts[c.Id] = statut;
        }

        /// <summary>Le mode AvA ne se désactive pas (demande utilisateur).</summary>
        public void ActiverAvA(Character c, bool actif)
        {
            if (c.Guild?.Alliance == null)
                c.SendServerMessage("Le mode AvA est réservé aux membres d'une alliance.");
            else if (!actif)
                c.SendServerMessage("Le mode AvA est toujours actif pour les membres d'une alliance.");

            EnvoyerStatut(c);
        }

        /// <summary>Zaaps, zaapis et prismes : pas d'entrée ni de sortie d'une zone vulnérable pour les membres d'une alliance.</summary>
        public string ZaapBloque(Character c, Map depart, Map arrivee)
        {
            // MYSTIC 17/10 : villages de conquête fermés aux autres alliances (vérifié avant de payer le zaap)
            if (c != null && !VillagesConquete.Instance.PeutEntrer(c, arrivee, out var village))
                return village;

            if (c?.Guild?.Alliance == null)
                return null;

            var zone = ZoneVulnerable(depart) ? depart.SubArea : ZoneVulnerable(arrivee) ? arrivee.SubArea : null;
            return zone == null ? null
                : $"La zone {NomZone(zone.Id)} est vulnérable (conquête en cours) : les membres d'une alliance ne peuvent pas y entrer ni en sortir par zaap.";
        }

        /// <summary>Connexion dans une zone vulnérable : renvoi au zaap de sauvegarde (zaap d'Astrub s'il est dans cette zone).</summary>
        public ObjectPosition PositionALaConnexion(Character c)
        {
            if (c?.Guild?.Alliance == null || !ZoneVulnerable(c.Map))
                return null;

            var zone = c.Map.SubArea;
            var dest = c.GetSpawnPoint();
            if (dest?.Map == null || dest.Map.SubArea?.Id == zone.Id || ZoneVulnerable(dest.Map))
            {
                var astrub = World.Instance.GetMap(CarteZaapAstrub);
                if (astrub == null)
                    return null;

                var cell = astrub.Zaap != null ? astrub.GetRandomAdjacentFreeCell(astrub.Zaap.Position.Point) : null;
                dest = new ObjectPosition(astrub, cell ?? astrub.GetRandomFreeCell(), c.Direction);
            }

            var nom = NomZone(zone.Id);
            WorldServer.Instance.IOTaskPool.CallDelayed(4000, () =>
            {
                if (c.IsInWorld)
                    c.DisplayNotification($"La zone {nom} est vulnérable (conquête en cours) : vous avez été téléporté à votre zaap.");
            });

            return dest;
        }

        public FighterRefusedReasonEnum PeutAgresser(Character agresseur, Character cible)
        {
            if (cible == agresseur)
                return FighterRefusedReasonEnum.FIGHT_MYSELF;

            if (!EstEnModeAvA(agresseur) || !EstEnModeAvA(cible))
                return FighterRefusedReasonEnum.INSUFFICIENT_RIGHTS;

            var a1 = agresseur.Guild?.Alliance;
            var a2 = cible.Guild?.Alliance;
            if (a1 == null || a2 == null || a1.Id == a2.Id)
                return FighterRefusedReasonEnum.WRONG_ALIGNMENT;

            if (cible.Map != agresseur.Map || !ZoneVulnerable(agresseur.Map))
                return FighterRefusedReasonEnum.WRONG_MAP;

            var koh = KohDe(agresseur.Map.SubArea.Id);
            if (koh.Elimines.Contains(agresseur.Id) || koh.Elimines.Contains(cible.Id))
                return FighterRefusedReasonEnum.INSUFFICIENT_RIGHTS;

            if (!cible.IsInWorld || cible.IsFighting() || cible.IsSpectator() || cible.IsBusy())
                return FighterRefusedReasonEnum.OPPONENT_OCCUPIED;

            if (!agresseur.IsInWorld || agresseur.IsFighting() || agresseur.IsSpectator() || agresseur.IsBusy())
                return FighterRefusedReasonEnum.IM_OCCUPIED;

            if (string.Equals(cible.Client.IP, agresseur.Client.IP) && !agresseur.IsGameMaster())
                return FighterRefusedReasonEnum.MULTIACCOUNT_NOT_ALLOWED;

            return FighterRefusedReasonEnum.FIGHTER_ACCEPTED;
        }

        public void Agresser(Character agresseur, Character cible)
        {
            var fight = FightManager.Instance.CreateKohFight(cible.Map);
            fight.ChallengersTeam.AddFighter(agresseur.CreateFighter(fight.ChallengersTeam));
            fight.DefendersTeam.AddFighter(cible.CreateFighter(fight.DefendersTeam));
            fight.StartPlacement();
        }

        public void Eliminer(Character c, Map map)
        {
            var koh = map?.SubArea == null ? null : KohDe(map.SubArea.Id);
            if (koh == null)
                return;

            lock (m_lock)
            {
                koh.Elimines.Add(c.Id);
                koh.Presences.Remove(c.Id);
            }

            c.SendServerMessage("Vous avez perdu : vous êtes éliminé de ce KOH.");
        }

        #endregion

        #region Attaque de prisme

        public bool ZoneReserveePourAutre(int subAreaId, int allianceId, out string message)
        {
            message = null;
            lock (m_lock)
            {
                if (!m_reservations.TryGetValue(subAreaId, out var r))
                    return false;

                if (r.fin < DateTime.Now)
                {
                    m_reservations.Remove(subAreaId);
                    return false;
                }

                if (r.alliance == allianceId)
                    return false;

                var minutes = (int)Math.Ceiling((r.fin - DateTime.Now).TotalMinutes);
                message = $"Cette zone vient d'être conquise : seule l'alliance gagnante peut y poser un prisme (encore {minutes} min).";
                return true;
            }
        }

        public void Attaquer(Character c)
        {
            var npc = c.Map.GetActors<PrismNpc>().FirstOrDefault();
            if (npc == null)
                return;

            var p = npc.Prisme;
            var alliance = c.Guild?.Alliance;
            if (alliance == null || alliance.Id == p.AllianceId)
            {
                c.SendServerMessage("Seule une autre alliance peut attaquer ce prisme.");
                return;
            }

            if (p.State != (int)PrismStateEnum.PRISM_STATE_NORMAL)
            {
                c.SendServerMessage("Ce prisme ne peut pas être attaqué pour le moment.");
                return;
            }

            // MYSTIC 17/10 : invulnérable 24 h après la pose, 5 h après une défense gagnée
            if (p.InvulnerableUntil.HasValue && p.InvulnerableUntil.Value > DateTime.Now)
            {
                var reste = p.InvulnerableUntil.Value - DateTime.Now;
                Message(c, $"Ce prisme est invulnérable encore {(int)reste.TotalHours} h {reste.Minutes:00} min.");
                return;
            }

            if (c.IsBusy() || c.IsFighting())
                return;

            lock (m_lock)
            {
                if (m_combats.ContainsKey(p.SubAreaId))
                {
                    c.SendServerMessage("Ce prisme est déjà attaqué : rejoignez le combat.");
                    return;
                }
            }

            if (c.Dialog != null)
                c.Dialog.Close();

            var fight = FightManager.Instance.CreatePvPrFight(c.Map, p);
            lock (m_lock)
                m_combats[p.SubAreaId] = fight;

            fight.ChallengersTeam.AddFighter(c.CreateFighter(fight.ChallengersTeam));

            var grade = MonsterManager.Instance.GetMonsterGrade(3451, 1);
            var cellule = c.Map.Cells[p.CellId];
            var groupe = new MonsterGroup(c.Map.GetNextContextualId(), new ObjectPosition(c.Map, cellule, DirectionsEnum.DIRECTION_SOUTH_EAST));
            var monstre = new Monster(grade, groupe);
            fight.DefendersTeam.AddFighter(monstre.CreateFighter(fight.DefendersTeam));

            fight.StartPlacement();
        }

        public FightPvPr CombatDe(int subAreaId)
        {
            lock (m_lock)
                return m_combats.TryGetValue(subAreaId, out var f) ? f : null;
        }

        PrismFightersInformation InfosCombat(FightPvPr fight) =>
            new PrismFightersInformation((ushort)fight.Prisme.SubAreaId,
                new ProtectedEntityWaitingForHelpInfo(fight.TimeLeftBeforeFight, FightPvPr.AttackersPlacementTime / 100,
                    (sbyte)Math.Max(0, FightPvPr.MaxFighters - fight.DefendersQueue.Count)),
                new[] { InfosPrisme(fight.Prisme) }.Concat(fight.DefendersQueue.Select(x => x.GetCharacterMinimalPlusLookInformations())).ToArray(),
                fight.Attackers.Select(x => x.GetCharacterMinimalPlusLookInformations()).ToArray());

        static CharacterMinimalPlusLookInformations InfosPrisme(PrismeAlliance p) =>
            new CharacterMinimalPlusLookInformations((ulong)(int.MaxValue - p.SubAreaId), "Prisme " + (p.Alliance?.Name ?? ""), 200,
                PrismNpc.LookDe(p.Alliance, p.State).GetEntityLook(), 0);

        public void EnvoyerCombats(Character c)
        {
            var alliance = c.Guild?.Alliance;
            if (alliance == null)
                return;

            FightPvPr[] combats;
            lock (m_lock)
                combats = m_combats.Values.Where(x => x.Prisme.AllianceId == alliance.Id).ToArray();

            c.Client.Send(new PrismsInfoValidMessage(combats.Select(InfosCombat).ToArray()));
        }

        public void OnPrismAttacked(FightPvPr fight)
        {
            var p = fight.Prisme;
            p.State = (int)PrismStateEnum.PRISM_STATE_ATTACKED;
            PrismManager.Instance.SauverEtat(p);

            var alliance = p.Alliance;
            if (alliance == null)
                return;

            alliance.Send(new PrismFightAddedMessage(InfosCombat(fight)));
            // le message d'attaque est affiché par le client lui-même (« Le prisme de la zone '...' est attaqué en [x,y] ! » + notification) :
            // pas de message serveur en plus (doublon)
        }

        public void OnDefendersChanged(FightPvPr fight, Character c, bool ajoute)
        {
            var alliance = fight.Prisme.Alliance;
            if (alliance == null)
                return;

            if (ajoute)
                alliance.Send(new PrismFightDefenderAddMessage((ushort)fight.Prisme.SubAreaId, (ushort)fight.Id, c.GetCharacterMinimalPlusLookInformations()));
            else
                alliance.Send(new PrismFightDefenderLeaveMessage((ushort)fight.Prisme.SubAreaId, (ushort)fight.Id, (ulong)c.Id));
        }

        public void OnPrismFightEnded(FightPvPr fight, bool attaquantsGagnent)
        {
            var p = fight.Prisme;
            lock (m_lock)
                m_combats.Remove(p.SubAreaId);

            if (attaquantsGagnent)
            {
                p.State = (int)PrismStateEnum.PRISM_STATE_WEAKENED;
                p.VulnerableAt = ProchainCreneau(p, DateTime.Now + DelaiAvantVulnerabilite);
            }
            else
            {
                p.State = (int)PrismStateEnum.PRISM_STATE_NORMAL;
                p.InvulnerableUntil = DateTime.Now.AddHours(5); // défense gagnée : 5 h d'invulnérabilité
            }

            PrismManager.Instance.SauverEtat(p);
            p.Alliance?.Send(new PrismFightRemovedMessage((ushort)p.SubAreaId));

            if (p.Alliance != null)
                foreach (var membre in p.Alliance.OnlineCharacters)
                    if (attaquantsGagnent)
                        Message(membre, $"Le prisme de la zone {Lien(p.SubAreaId)} n'a pas survécu ! La zone sera vulnérable le {p.VulnerableAt:dd/MM à HH:mm}.");
                    else
                        Message(membre, $"Le prisme de la zone {Lien(p.SubAreaId)} a survécu à l'attaque !");
        }

        /// <summary>Premier créneau de vulnérabilité du prisme (heure choisie) à partir de la date donnée.</summary>
        static DateTime ProchainCreneau(PrismeAlliance p, DateTime aPartirDe)
        {
            var depart = aPartirDe.ToUniversalTime();
            var date = depart.Date.AddMinutes(p.VulnQuarter * 15);
            if (date < depart)
                date = date.AddDays(1);
            return DateTime.SpecifyKind(date, DateTimeKind.Utc).ToLocalTime();
        }

        #endregion

        #region Sabotage

        public void Saboter(Character c, int subAreaId)
        {
            var p = PrismManager.Instance.TryGetPrisme(subAreaId);
            var alliance = c.Guild?.Alliance;
            PrismSetSabotagedRefusedReasonEnum? refus = null;

            if (p == null)
                refus = PrismSetSabotagedRefusedReasonEnum.SABOTAGE_NO_PRISM;
            else if (alliance == null || alliance.Id != p.AllianceId)
                refus = PrismSetSabotagedRefusedReasonEnum.SABOTAGE_WRONG_ALLIANCE;
            else if (!Conditions.Criterions.AllianceRightsCriterion.AUnDroit(c, AllianceRightsBitEnum.ALLIANCE_RIGHT_MANAGE_PRISMS))
                refus = PrismSetSabotagedRefusedReasonEnum.SABOTAGE_INSUFFICIENT_RIGHTS;
            else if (p.State != (int)PrismStateEnum.PRISM_STATE_NORMAL)
                refus = PrismSetSabotagedRefusedReasonEnum.SABOTAGE_WRONG_STATE;

            if (refus != null)
            {
                c.Client.Send(new PrismSetSabotagedRefusedMessage((ushort)subAreaId, (sbyte)refus.Value));
                return;
            }

            p.State = (int)PrismStateEnum.PRISM_STATE_SABOTAGED;
            p.VulnerableAt = ProchainCreneau(p, DateTime.Now + DelaiAvantVulnerabilite);
            PrismManager.Instance.SauverEtat(p);
            c.SendServerMessage($"Prisme saboté : il sera vulnérable le {p.VulnerableAt:dd/MM à HH:mm}.");
        }

        /// <summary>Commande .koh (tests) : la sous-zone devient vulnérable tout de suite.</summary>
        /// <summary>Commandes .koh de test (admin) : start, win, perdu, points N, pret, info.</summary>
        public string CommandeKoh(Character c, string action, int valeur)
        {
            var subArea = c.Map?.SubArea;
            if (subArea == null)
                return "Aucune sous-zone ici.";

            action = (action ?? "").Trim().ToLowerInvariant();
            if (action == "" || action == "start")
                return ForcerVulnerable(subArea.Id);

            // MYSTIC 17/10 : tests : retire l'invulnérabilité (24 h après la pose / 5 h après une défense) de tous les prismes
            if (action == "invu")
            {
                var prismes = PrismManager.Instance.Prismes.Where(x => x.InvulnerableUntil.HasValue).ToArray();
                foreach (var p in prismes)
                {
                    p.InvulnerableUntil = null;
                    PrismManager.Instance.SauverEtat(p);
                }
                return $"Invulnérabilité retirée de {prismes.Length} prisme(s) : ils peuvent être attaqués tout de suite.";
            }

            var koh = KohDe(subArea.Id);
            if (koh == null)
                return "Aucun KOH en cours ici (tapez .koh pour en lancer un).";

            var alliance = c.Guild?.Alliance;
            switch (action)
            {
                case "win":
                case "gagner":
                    if (alliance == null)
                        return "Il faut une alliance pour gagner le KOH.";
                    TerminerKoh(koh, alliance.Id);
                    return $"KOH gagné par votre alliance ({alliance.Name}).";

                case "perdu":
                case "lose":
                case "stop":
                case "fin":
                    TerminerKoh(koh, koh.Prisme.AllianceId);
                    return "KOH terminé : le propriétaire garde la zone (comme à la fin des 2 h).";

                case "points":
                    if (alliance == null)
                        return "Il faut une alliance pour recevoir des points.";
                    var ajout = valeur > 0 ? valeur : 10;
                    int total;
                    lock (m_lock)
                        total = koh.Points[alliance.Id] = (koh.Points.TryGetValue(alliance.Id, out var pts) ? pts : 0) + ajout;
                    EnvoyerKoh(koh, PersonnagesDe(subArea.Id).ToArray());
                    if (total >= PointsVictoire)
                        TerminerKoh(koh, alliance.Id);
                    return $"+{ajout} points pour {alliance.Name} (total {Math.Min(total, PointsVictoire)} / {PointsVictoire}).";

                case "pret":
                case "presence":
                    var persos = PersonnagesDe(subArea.Id).Where(EstEnModeAvA).ToArray();
                    lock (m_lock)
                        foreach (var perso in persos)
                            if (!koh.Elimines.Contains(perso.Id))
                                koh.Presences[perso.Id] = DateTime.Now - PresenceMini;
                    foreach (var perso in persos)
                        MajStatut(perso);
                    return $"Minute de présence passée pour {persos.Length} personnage(s) de la zone.";

                case "info":
                    string scores;
                    lock (m_lock)
                        scores = string.Join(", ", koh.Points.Select(x => $"{AllianceManager.Instance.TryGetAlliance(x.Key)?.Name ?? x.Key.ToString()} {x.Value}"));
                    var reste = DureeMax - (DateTime.Now - koh.Debut);
                    return $"KOH : {(scores == "" ? "aucun point" : scores)} ; {koh.Presences.Count} participant(s) ; {Math.Max(0, (int)reste.TotalMinutes)} min restantes.";

                default:
                    return "Usage : .koh [start | win | perdu | points N | pret | info | invu]";
            }
        }

        public string ForcerVulnerable(int subAreaId)
        {
            var p = PrismManager.Instance.TryGetPrisme(subAreaId);
            if (p == null)
                return "Aucun prisme dans cette sous-zone.";

            if (KohDe(subAreaId) != null)
                return "Un KOH est déjà en cours ici.";

            DemarrerKoh(p);
            return "KOH lancé dans cette sous-zone.";
        }

        #endregion

        #region KOH

        void DemarrerKoh(PrismeAlliance p, DateTime? reprise = null)
        {
            var debut = reprise ?? DateTime.Now;
            var koh = new Koh { Prisme = p, Debut = debut, ProchainPoint = DateTime.Now.AddMilliseconds(TickMs * TicksParPoint) };
            lock (m_lock)
                m_koh[p.SubAreaId] = koh;

            // pendant la vulnérabilité, la « date de vulnérabilité » du prisme = début du KOH : le client affiche le temps restant
            // (début + 2 h − maintenant) ; enregistrée pour reprendre le KOH après un redémarrage
            p.State = (int)PrismStateEnum.PRISM_STATE_VULNERABLE;
            p.VulnerableAt = debut;
            PrismManager.Instance.SauverEtat(p);

            if (reprise != null)
            {
                logger.Info("[AvA] KOH repris dans la sous-zone {0} (commencé le {1})", p.SubAreaId, debut);
                return;
            }

            // textes officiels du client (i18n) : l'alliance propriétaire défend, tous les autres peuvent conquérir
            var lien = Lien(p.SubAreaId);
            foreach (var perso in World.Instance.GetCharacters())
                Message(perso, perso.Guild?.Alliance?.Id == p.AllianceId
                    ? $"Votre zone {lien} est vulnérable. Vous pouvez la défendre."
                    : $"La zone {lien} est vulnérable. Vous pouvez tenter de la conquérir.");

            foreach (var c in PersonnagesDe(p.SubAreaId))
                EnvoyerStatut(c);

            logger.Info("[AvA] KOH ouvert dans la sous-zone {0} (prisme de l'alliance {1})", p.SubAreaId, p.AllianceId);
        }

        static IEnumerable<Character> PersonnagesDe(int subAreaId) =>
            World.Instance.GetCharacters().Where(x => x.Map?.SubArea != null && x.Map.SubArea.Id == subAreaId);

        void Tick()
        {
            try
            {
                foreach (var p in PrismManager.Instance.Prismes.Where(x =>
                    (x.State == (int)PrismStateEnum.PRISM_STATE_WEAKENED || x.State == (int)PrismStateEnum.PRISM_STATE_SABOTAGED) &&
                    x.VulnerableAt.HasValue && x.VulnerableAt.Value <= DateTime.Now).ToArray())
                {
                    if (KohDe(p.SubAreaId) == null)
                        DemarrerKoh(p);
                }

                Koh[] sessions;
                lock (m_lock)
                    sessions = m_koh.Values.ToArray();

                foreach (var koh in sessions)
                    TickKoh(koh);
            }
            catch (Exception e)
            {
                logger.Error("[AvA] erreur du tick : {0}", e);
            }
        }

        void TickKoh(Koh koh)
        {
            var maintenant = DateTime.Now;
            var persos = PersonnagesDe(koh.Prisme.SubAreaId).ToArray();
            var partis = new List<int>();

            lock (m_lock)
            {
                // présence : depuis quand chaque participant est sur sa carte (hors combat)
                var vus = new HashSet<int>();
                foreach (var c in persos)
                {
                    if (!EstEnModeAvA(c) || koh.Elimines.Contains(c.Id))
                        continue;

                    // une seule participation à la fois
                    if (m_koh.Values.Any(k => k != koh && k.Presences.ContainsKey(c.Id)))
                        continue;

                    vus.Add(c.Id);
                    if (!koh.Presences.ContainsKey(c.Id))
                        koh.Presences[c.Id] = maintenant;
                }

                foreach (var id in koh.Presences.Keys.Where(x => !vus.Contains(x)).ToArray())
                {
                    koh.Presences.Remove(id);
                    partis.Add(id);
                }
            }

            // horloge → participant (et retour à la normale pour ceux qui sont sortis)
            foreach (var c in persos)
                MajStatut(c);
            foreach (var id in partis)
                MajStatut(World.Instance.GetCharacter(id));

            koh.Ticks++;
            if (koh.Ticks % TicksParPoint != 0)
                return;

            koh.ProchainPoint = maintenant.AddMilliseconds(TickMs * TicksParPoint);

            // contrôle des cartes : alliance la plus nombreuse (égalité : somme des niveaux)
            koh.CartesDuTour.Clear();
            koh.GagnantParCarte.Clear();
            Character[] presents;
            lock (m_lock)
                presents = persos.Where(c => !c.IsFighting() && koh.Presences.TryGetValue(c.Id, out var depuis) && maintenant - depuis >= PresenceMini).ToArray();
            foreach (var carte in presents.GroupBy(c => c.Map.Id))
            {
                var classement = carte.GroupBy(c => c.Guild.Alliance.Id)
                    .Select(g => new { Alliance = g.Key, Nombre = g.Count(), Niveaux = g.Sum(x => (int)x.Level) })
                    .OrderByDescending(x => x.Nombre).ThenByDescending(x => x.Niveaux).ToArray();

                if (classement.Length == 0)
                    continue;

                if (classement.Length > 1 && classement[0].Nombre == classement[1].Nombre && classement[0].Niveaux == classement[1].Niveaux)
                    continue;

                koh.GagnantParCarte[carte.Key] = classement[0].Alliance;
                koh.CartesDuTour[classement[0].Alliance] = (koh.CartesDuTour.TryGetValue(classement[0].Alliance, out var n) ? n : 0) + 1;
            }

            var tour = koh.CartesDuTour.OrderByDescending(x => x.Value).ToArray();
            if (tour.Length > 0 && (tour.Length == 1 || tour[0].Value > tour[1].Value))
                koh.Points[tour[0].Key] = (koh.Points.TryGetValue(tour[0].Key, out var pts) ? pts : 0) + 1;

            EnvoyerKoh(koh, persos);

            var gagnant = koh.Points.Where(x => x.Value >= PointsVictoire).Select(x => (int?)x.Key).FirstOrDefault();
            if (gagnant != null)
                TerminerKoh(koh, gagnant.Value);
            else if (maintenant - koh.Debut >= DureeMax)
                TerminerKoh(koh, koh.Prisme.AllianceId);
        }

        void EnvoyerKoh(Koh koh, Character[] persos)
        {
            var ids = koh.Points.Keys.Union(koh.CartesDuTour.Keys).Union(new[] { koh.Prisme.AllianceId }).Distinct()
                .Select(AllianceManager.Instance.TryGetAlliance).Where(x => x != null).ToArray();

            var infos = ids.Select(a => a.GetAllianceInformations()).ToArray();
            var nombres = ids.Select(a => (ushort)koh.Presences.Keys.Count(id => World.Instance.GetCharacter(id)?.Guild?.Alliance?.Id == a.Id)).ToArray();
            var poids = ids.Select(a => (uint)(koh.CartesDuTour.TryGetValue(a.Id, out var n) ? n : 0)).ToArray();
            var scores = ids.Select(a => (byte)Math.Min(255, koh.Points.TryGetValue(a.Id, out var n) ? n : 0)).ToArray();
            var prochain = koh.ProchainPoint.GetUnixTimeStampLong();

            foreach (var c in persos)
            {
                var gagnantCarte = koh.GagnantParCarte.TryGetValue(c.Map.Id, out var g) ? AllianceManager.Instance.TryGetAlliance(g) : null;
                var surCarte = persos.Where(x => x.Map == c.Map && koh.Presences.ContainsKey(x.Id)).ToArray();
                var scoreGagnant = gagnantCarte == null ? 0 : surCarte.Count(x => x.Guild?.Alliance?.Id == gagnantCarte.Id);
                var monScore = c.Guild?.Alliance == null ? 0 : surCarte.Count(x => x.Guild?.Alliance?.Id == c.Guild.Alliance.Id);

                c.Client.Send(new KohUpdateMessage(infos, nombres, poids, scores,
                    gagnantCarte == null ? new BasicAllianceInformations[0] : new[] { gagnantCarte.GetBasicAllianceInformations() },
                    (uint)scoreGagnant, (uint)monScore, prochain));
            }
        }

        /// <summary>
        /// MYSTIC 17/10 (phase 4) : fin de KOH, les pépites accumulées sur le prisme sont partagées à parts égales entre les
        /// participants de l'alliance gagnante encore dans la zone (minute de présence faite, non éliminés).
        /// </summary>
        void PartagerPepites(Koh koh, int allianceGagnante, Character[] persos)
        {
            var p = koh.Prisme;
            if (p.Pepites <= 0)
                return;

            Character[] gagnants;
            lock (m_lock)
                gagnants = persos.Where(c => c.Guild?.Alliance?.Id == allianceGagnante && !koh.Elimines.Contains(c.Id) &&
                    koh.Presences.TryGetValue(c.Id, out var depuis) && DateTime.Now - depuis >= PresenceMini).ToArray();

            if (gagnants.Length == 0)
                return;

            var part = p.Pepites / gagnants.Length;
            if (part <= 0)
                return;

            foreach (var c in gagnants)
            {
                PrismModules.DonnerPepites(c, part);
                Message(c, $"Vous recevez {part} pépite(s) du prisme de la zone {Lien(p.SubAreaId)}.");
            }

            p.Pepites -= part * gagnants.Length;
            PrismManager.Executer("UPDATE nekfeu_prisms SET Pepites = @1 WHERE SubAreaId = @0", p.SubAreaId, p.Pepites);
        }

        void TerminerKoh(Koh koh, int allianceGagnante)
        {
            lock (m_lock)
                m_koh.Remove(koh.Prisme.SubAreaId);

            var p = koh.Prisme;
            var gagnante = AllianceManager.Instance.TryGetAlliance(allianceGagnante);
            var persos = PersonnagesDe(p.SubAreaId).ToArray();

            PartagerPepites(koh, allianceGagnante, persos);

            if (allianceGagnante == p.AllianceId)
            {
                p.State = (int)PrismStateEnum.PRISM_STATE_NORMAL;
                p.VulnerableAt = null;
                p.InvulnerableUntil = DateTime.Now.AddHours(5); // défense gagnée : 5 h d'invulnérabilité
                PrismManager.Instance.SauverEtat(p);
                MessageATous($"Conquête terminée : l'alliance {Lien(gagnante)} garde la zone {Lien(p.SubAreaId)}.");
            }
            else
            {
                lock (m_lock)
                    m_reservations[p.SubAreaId] = (allianceGagnante, DateTime.Now + Reservation);

                // MYSTIC 17/10 : village de conquête : PNJ et destinations du perdant perdus, récompense en attente pour le gagnant
                VillagesConquete.Instance.SurConquete(p, allianceGagnante);

                var map = p.Map;
                if (map != null)
                    map.Area.ExecuteInContext(() => PrismManager.Instance.Retirer(p));
                else
                    PrismManager.Instance.Retirer(p);

                MessageATous($"Conquête terminée : l'alliance {Lien(gagnante)} a conquis la zone {Lien(p.SubAreaId)} ! Elle a 2 h pour y poser son prisme.");
            }

            foreach (var c in persos)
                EnvoyerStatut(c);

            // MYSTIC 17/10 : fin du KOH : ceux qui n'ont plus le droit d'être dans le village vont à sa porte
            var sousZone = p.SubAreaId;
            WorldServer.Instance.IOTaskPool.CallDelayed(2000, () => VillagesConquete.Instance.Expulser(sousZone));

            logger.Info("[AvA] KOH terminé dans la sous-zone {0} : alliance {1}", p.SubAreaId, allianceGagnante);
        }

        #endregion
    }
}
