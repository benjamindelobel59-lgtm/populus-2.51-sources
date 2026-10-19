using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NLog;
using Stump.Core.Reflection;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Messages;
using Stump.DofusProtocol.Types;
using Stump.Server.BaseServer.Initialization;
using Stump.Server.WorldServer.Database.I18n;
using Stump.Server.WorldServer.Database.Monsters;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Monsters;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Fights.Results;
using Stump.Server.WorldServer.Game.Items;
using Stump.Server.WorldServer.Game.Maps;
using Stump.Server.WorldServer.Game.Maps.Cells;
using Stump.Server.WorldServer.Handlers.Context;

namespace Stump.Server.WorldServer.Game.Chasses
{
    /// <summary>
    /// MYSTIC 19/10 : chasses au trésor rapides (cahier des charges : _OUTILS/serveur/chasses/CAHIER_DES_CHARGES.md).
    /// - Distributeur de la Malle aux Trésors : chasse classique du niveau choisi (20 à 200) : 3-4 étapes de 3-5 indices
    ///   (direction cardinale + 1 à 10 cartes, nom d'indice au hasard) ; la position de chaque indice est donnée dans le chat
    ///   avec un lien « Y aller » (autopilote réservé aux chasses) ; le joueur pose le jalon puis fouille ; 3 essais par étape ;
    ///   dernière fouille = combat contre le Trésor (3724) ; récompenses : XP, Boîte de fragments, 1 / 2 / 4 coffres.
    /// - Table de planification : chasse légendaire avec une carte assemblée (combat contre le monstre légendaire, coffre légendaire).
    /// </summary>
    public class ChasseManager : Singleton<ChasseManager>
    {
        static readonly Logger logger = LogManager.GetCurrentClassLogger();
        static Stump.ORM.Database Database => WorldServer.Instance.DBAccessor.Database;
        static readonly System.Drawing.Color Couleur = System.Drawing.Color.FromArgb(0xE8, 0xC5, 0x47);

        public const int EssaisParEtape = 3;
        public const int MonstreTresor = 3724;
        public const int RoseDesSables = 15263;
        const int PremiereBoite = 15558;          // Boîte de fragments insignifiante (niv. 20) … magistrale (15567, niv. 200)
        const int TypeFragment = 175;
        // coffres de niveau : 20, 40, 60, 80, 100, 120, 140, 160, 180, 200
        static readonly int[] CoffresDeNiveau = { 15248, 15260, 15261, 15262, 15264, 15265, 15266, 15267, 15268, 15269 };
        static readonly int[] RosesDeBase = { 40, 60, 80, 100, 150, 200, 250, 300, 350, 400 };
        // multiplicateurs rares d'un coffre (chance en %)
        static readonly Tuple<int, double>[] Multiplicateurs =
            { Tuple.Create(500, 0.005), Tuple.Create(100, 0.03), Tuple.Create(10, 0.3), Tuple.Create(5, 1.0), Tuple.Create(2, 5.0) };
        const int MultiplicateurCoffreLegendaire = 3;

        // zones bonus : cités (2 coffres) et Frigost à partir du Ben le Ripate (4 coffres)
        static readonly int[] ZonesCites = { 7, 11 };               // Bonta, Brâkmar
        static readonly int[] SousZonesCites = { 32 };              // Sufokia (ville)
        static readonly int[] SousZonesFrigostBonus =
            { 609, 619, 610, 620, 606, 622, 621, 605, 623, 608, 624, 611, 752, 753, 616, 785, 786, 787, 788, 789, 790, 791, 792, 793, 794, 795 };

        public class IndiceRecord { public int Id { get; set; } public int NameId { get; set; } }
        public class Legendaire
        {
            public int Id { get; set; } public int NameId { get; set; } public int Niveau { get; set; }
            public int CoffreId { get; set; } public int MonstreId { get; set; } public int CarteId { get; set; } public double RatioXp { get; set; }
        }
        class Ligne { public int CharacterId { get; set; } public string Donnees { get; set; } }

        readonly Random m_random = new Random();
        readonly object m_lock = new object();
        readonly Dictionary<int, Chasse> m_chasses = new Dictionary<int, Chasse>();
        readonly HashSet<int> m_enCombat = new HashSet<int>();
        List<IndiceRecord> m_indices = new List<IndiceRecord>();
        Dictionary<int, Legendaire> m_legendaires = new Dictionary<int, Legendaire>();
        List<int> m_fragments = new List<int>();
        List<Map> m_cartesCites, m_cartesFrigost, m_zaaps;

        [Initialization(typeof(ItemManager))]
        public void Initialize()
        {
            try
            {
                Database.Execute("CREATE TABLE IF NOT EXISTS mystic_chasses (CharacterId INT NOT NULL PRIMARY KEY, Donnees MEDIUMTEXT NOT NULL) DEFAULT CHARSET=utf8");
                foreach (var l in Database.Fetch<Ligne>("SELECT CharacterId, Donnees FROM mystic_chasses"))
                {
                    try { m_chasses[l.CharacterId] = JsonConvert.DeserializeObject<Chasse>(l.Donnees); }
                    catch (Exception ex) { logger.Warn($"[Chasses] chasse illisible pour le personnage {l.CharacterId} : {ex.Message}"); }
                }
                m_indices = Database.Fetch<IndiceRecord>("SELECT Id, NameId FROM mystic_chasses_indices");
                m_legendaires = Database.Fetch<Legendaire>("SELECT Id, NameId, Niveau, CoffreId, MonstreId, CarteId, RatioXp FROM mystic_chasses_legendaires").ToDictionary(x => x.Id);
                m_fragments = ItemManager.Instance.GetTemplates().Where(x => x.TypeId == TypeFragment).Select(x => x.Id).ToList();

                // objets : coffres de niveau et légendaires, boîtes de fragments
                foreach (var id in CoffresDeNiveau.Concat(m_legendaires.Values.Select(x => x.CoffreId)).Distinct())
                    Enregistrer(typeof(CoffreChasse), id);
                for (var id = PremiereBoite; id < PremiereBoite + 10; id++)
                    Enregistrer(typeof(BoiteFragments), id);

                logger.Info($"[Chasses] {m_chasses.Count} chasse(s) en cours, {m_indices.Count} indices, {m_legendaires.Count} chasses légendaires, {m_fragments.Count} fragments");
            }
            catch (Exception ex)
            {
                logger.Error($"[Chasses] initialisation impossible (SQL 70 passé ?) : {ex}");
            }
        }

        static void Enregistrer(Type type, int itemId)
        {
            try { ItemManager.Instance.AddItemIdConstructor(type, (ItemIdEnum)itemId); }
            catch (Exception ex) { logger.Warn($"[Chasses] objet {itemId} déjà géré ailleurs : {ex.Message}"); }
        }

        public Chasse GetChasse(Character c)
        {
            lock (m_lock)
                return m_chasses.TryGetValue(c.Id, out var ch) ? ch : null;
        }

        #region Démarrage

        public void DemarrerClassique(Character c, int niveau)
        {
            var max = Math.Max(20, (int)Math.Ceiling(c.Level / 20.0) * 20);
            if (niveau > max)
            {
                Message(c, $"Cette chasse est trop difficile pour toi : niveau {max} au maximum.");
                c.Client?.Send(new TreasureHuntRequestAnswerMessage((sbyte)TreasureHuntTypeEnum.TREASURE_HUNT_CLASSIC, (sbyte)TreasureHuntRequestEnum.TREASURE_HUNT_ERROR_NOT_AVAILABLE));
                return;
            }
            Demarrer(c, (int)TreasureHuntTypeEnum.TREASURE_HUNT_CLASSIC, niveau, 0);
        }

        /// <summary>MYSTIC 19/10 : Salle de localisation des portails : état du portail, ou chasse aux portails s'il n'est pas ouvert
        /// (pas de combat, pas d'XP, pas de récompense : le portail apparaît à la fin).</summary>
        public void DemarrerPortail(Character c, int areaId)
        {
            if (!PortailManager.Dimensions.ContainsKey(areaId))
                return;
            if (PortailManager.Instance.GetPortail(areaId) != null)
            {
                PortailManager.Instance.Informer(c, areaId);
                return;
            }
            Demarrer(c, (int)TreasureHuntTypeEnum.TREASURE_HUNT_PORTAL, 0, 0, areaId);
        }

        void Demarrer(Character c, int type, int niveau, int legendaireId, int dimension = 0)
        {
            if (GetChasse(c) != null)
            {
                Message(c, "Tu as déjà une chasse au trésor en cours : termine-la ou abandonne-la (fenêtre de chasse).");
                c.Client?.Send(new TreasureHuntRequestAnswerMessage((sbyte)type, (sbyte)TreasureHuntRequestEnum.TREASURE_HUNT_ERROR_ALREADY_HAVE_QUEST));
                return;
            }

            var attente = AttenteAbandon(c);
            if (attente > TimeSpan.Zero)
            {
                var min = (int)attente.TotalMinutes;
                var sec = attente.Seconds;
                Message(c, $"Tu as abandonné une chasse récemment : attends encore {(min > 0 ? $"{min} min " : "")}{sec} s avant d'en commencer une nouvelle.");
                c.Client?.Send(new TreasureHuntRequestAnswerMessage((sbyte)type, (sbyte)TreasureHuntRequestEnum.TREASURE_HUNT_ERROR_NOT_AVAILABLE));
                return;
            }

            Chasse ch = null;
            for (var essai = 0; essai < 25 && ch == null; essai++)
                ch = Generer(c, type, niveau, legendaireId);
            if (ch == null)
            {
                Message(c, "Impossible de préparer une chasse pour le moment, réessaie.");
                c.Client?.Send(new TreasureHuntRequestAnswerMessage((sbyte)type, (sbyte)TreasureHuntRequestEnum.TREASURE_HUNT_ERROR_UNDEFINED));
                return;
            }

            ch.Dimension = dimension;
            lock (m_lock)
                m_chasses[c.Id] = ch;
            Sauver(c.Id, ch);
            c.Client?.Send(new TreasureHuntRequestAnswerMessage((sbyte)type, (sbyte)TreasureHuntRequestEnum.TREASURE_HUNT_OK));
            Envoyer(c, ch);
            var depart = World.Instance.GetMap(ch.CarteDepart);
            var zaapProche = ZaapProche(c, depart);
            var titre = type == 1 ? $"Chasse au portail de {PortailManager.Dimensions[dimension].Nom} commencée</b> ({ch.Etapes.Count} étapes, pas de combat : le portail apparaît à la fin)"
                                  : $"Chasse au trésor{(type == 2 ? " légendaire" : "")} commencée</b> (niveau {niveau}, {ch.Etapes.Count} étapes)";
            Message(c, $"<b>{titre}. " +
                       $"Départ en {{map,{depart?.Position.X},{depart?.Position.Y},1::[{depart?.Position.X},{depart?.Position.Y}]}} ({depart?.SubArea?.Record?.Name})" +
                       (zaapProche != "" ? $", {zaapProche}" : "") + ". Les positions des indices s'afficheront ici : clique sur « Y aller » pour le trajet automatique.");
            AnnoncerIndice(c, ch);
        }

        #endregion

        #region Génération

        static readonly MapNeighbour[] Directions = { MapNeighbour.Right, MapNeighbour.Bottom, MapNeighbour.Left, MapNeighbour.Top };

        static int DirectionClient(MapNeighbour d)
        {
            switch (d)
            {
                case MapNeighbour.Right: return 0;
                case MapNeighbour.Bottom: return 2;
                case MapNeighbour.Left: return 4;
                default: return 6;
            }
        }

        /// <summary>Carte où une chasse (et l'autopilote) peut passer pour ce personnage.</summary>
        public static bool CartePraticable(Character c, Map m)
        {
            if (m == null || !m.Outdoor || m.WorldMap != 1 || m.SubArea == null || m.IsDungeon() || SousLEau.Est(m))
                return false;
            if (!AccesFrigost.ZoneAutorisee(c, m))
                return false;
            return Alliances.VillagesConquete.Instance.PeutEntrer(c, m, out _);
        }

        Chasse Generer(Character c, int type, int niveau, int legendaireId)
        {
            var depart = ChoisirDepart(c);
            if (depart == null || m_indices.Count == 0)
                return null;

            var ch = new Chasse { Type = type, Niveau = niveau, LegendaireId = legendaireId, CarteDepart = depart.Id };
            var parcours = new List<Map> { depart };
            var courante = depart;
            var nbEtapes = m_random.Next(3, 5);                     // 3 ou 4 étapes
            var dejaVues = new HashSet<int> { depart.Id };
            for (var e = 0; e < nbEtapes; e++)
            {
                var etape = new List<Indice>();
                var nbIndices = m_random.Next(3, 6);                // 3, 4 ou 5 indices
                for (var i = 0; i < nbIndices; i++)
                {
                    var choix = Directions.OrderBy(x => m_random.Next()).Select(d => new { d, cartes = Marche(c, courante, d) })
                        .Where(x => x.cartes.Any(m => !dejaVues.Contains(m.Id))).FirstOrDefault();
                    if (choix == null)
                        return null;
                    // 1 à 10 cartes (au plus ce que la marche permet), sans revenir sur une carte d'indice déjà utilisée
                    var possibles = Enumerable.Range(1, choix.cartes.Count).Where(k => !dejaVues.Contains(choix.cartes[k - 1].Id)).ToList();
                    if (possibles.Count == 0)
                        return null;
                    var k2 = possibles[m_random.Next(possibles.Count)];
                    var arrivee = choix.cartes[k2 - 1];
                    parcours.AddRange(choix.cartes.Take(k2));
                    dejaVues.Add(arrivee.Id);
                    etape.Add(new Indice
                    {
                        Direction = DirectionClient(choix.d),
                        Distance = k2,
                        PointInteret = m_indices[m_random.Next(m_indices.Count)].Id,
                        Carte = arrivee.Id
                    });
                    courante = arrivee;
                }
                ch.Etapes.Add(etape);
            }

            // coffres : 4 si passage à Frigost (zone du Ben le Ripate ou plus loin), 2 si passage par une cité, 1 sinon
            if (parcours.Any(m => SousZonesFrigostBonus.Contains(m.SubArea.Id)))
                ch.Coffres = 4;
            else if (parcours.Any(m => ZonesCites.Contains(m.Area?.Id ?? -1) || SousZonesCites.Contains(m.SubArea.Id)))
                ch.Coffres = 2;
            return ch;
        }

        /// <summary>Cartes traversées en allant tout droit dans une direction (10 au plus), en s'arrêtant au premier obstacle.</summary>
        static List<Map> Marche(Character c, Map depuis, MapNeighbour d)
        {
            var cartes = new List<Map>();
            var m = depuis;
            for (var i = 0; i < 10; i++)
            {
                if (!m.Cells.Any(x => x.Walkable && !x.NonWalkableDuringRP && EstSortie(x, d)))
                    break;
                Map v;
                try { v = m.GetNeighbouringMap(d); } catch { v = null; }
                if (!CartePraticable(c, v))
                    break;
                cartes.Add(v);
                m = v;
            }
            return cartes;
        }

        static bool EstSortie(Database.World.Cell cell, MapNeighbour d)
        {
            switch (d)
            {
                case MapNeighbour.Right: return cell.IsRightChange;
                case MapNeighbour.Left: return cell.IsLeftChange;
                case MapNeighbour.Top: return cell.IsTopChange;
                case MapNeighbour.Bottom: return cell.IsBotChange;
                default: return false;
            }
        }

        /// <summary>Départ : 2 à 8 cartes autour d'un zaap tiré au hasard dans tout le monde ; parfois une cité (15 %) ou Frigost bonus (10 %, si autorisé).</summary>
        Map ChoisirDepart(Character c)
        {
            ConstruirePools();
            var tirage = m_random.NextDouble();
            if (tirage < 0.10)
            {
                var frigost = m_cartesFrigost.Where(m => CartePraticable(c, m)).ToList();
                if (frigost.Count > 0)
                    return frigost[m_random.Next(frigost.Count)];
            }
            if (tirage < 0.25)
            {
                var cites = m_cartesCites.Where(m => CartePraticable(c, m)).ToList();
                if (cites.Count > 0)
                    return cites[m_random.Next(cites.Count)];
            }

            // partout dans le monde : 2 à 8 cartes autour d'un zaap tiré au hasard (toutes les régions ont la même chance)
            var zaaps = ZaapsAutorises(c);
            for (var essai = 0; essai < 6 && zaaps.Count > 0; essai++)
            {
                var zaap = zaaps[m_random.Next(zaaps.Count)];
                var autour = Autour(c, zaap, 2, 8);
                if (autour.Count > 0)
                    return autour[m_random.Next(autour.Count)];
            }

            // secours : autour de la Malle aux Trésors
            var malle = World.Instance.GetMap(Autopilote.CarteSortieMalle);
            if (malle == null)
                return null;
            var candidates = Autour(c, malle, 3, 12);
            return candidates.Count > 0 ? candidates[m_random.Next(candidates.Count)] : malle;
        }

        /// <summary>Cartes praticables entre <paramref name="min"/> et <paramref name="max"/> cartes de <paramref name="centre"/> (à pied).</summary>
        List<Map> Autour(Character c, Map centre, int min, int max)
        {
            var distance = new Dictionary<int, int> { [centre.Id] = 0 };
            var file = new Queue<Map>();
            file.Enqueue(centre);
            var candidates = new List<Map>();
            while (file.Count > 0)
            {
                var m = file.Dequeue();
                var dist = distance[m.Id];
                if (dist >= min)
                    candidates.Add(m);
                if (dist >= max)
                    continue;
                foreach (var d in Directions)
                {
                    Map v;
                    try { v = m.GetNeighbouringMap(d); } catch { v = null; }
                    if (v == null || distance.ContainsKey(v.Id) || !CartePraticable(c, v))
                        continue;
                    distance[v.Id] = dist + 1;
                    file.Enqueue(v);
                }
            }
            return candidates;
        }

        /// <summary>Zaaps extérieurs que ce personnage peut utiliser (conditions Frigost, villages…).</summary>
        List<Map> ZaapsAutorises(Character c)
        {
            ConstruirePools();
            return m_zaaps.Where(z => CartePraticable(c, z) && AccesFrigost.ZaapAutorise(c, z, false)).ToList();
        }

        /// <summary>Texte « zaap le plus proche : Nom [x,y] » pour aller à une carte (vide si aucun).</summary>
        public string ZaapProche(Character c, Map cible)
        {
            if (cible == null)
                return "";
            var z = ZaapsAutorises(c).OrderBy(m => Math.Abs(m.Position.X - cible.Position.X) + Math.Abs(m.Position.Y - cible.Position.Y)).FirstOrDefault();
            return z == null ? "" : $"zaap le plus proche : {z.SubArea?.Record?.Name} {{map,{z.Position.X},{z.Position.Y},1::[{z.Position.X},{z.Position.Y}]}}";
        }

        void ConstruirePools()
        {
            if (m_cartesCites != null)
                return;
            var exterieures = World.Instance.GetMaps().Where(m => m.Outdoor && m.WorldMap == 1 && m.SubArea != null && !m.IsDungeon()).ToList();
            m_cartesCites = exterieures.Where(m => ZonesCites.Contains(m.Area?.Id ?? -1) || SousZonesCites.Contains(m.SubArea.Id)).ToList();
            m_cartesFrigost = exterieures.Where(m => SousZonesFrigostBonus.Contains(m.SubArea.Id)).ToList();
            m_zaaps = exterieures.Where(m => m.Zaap != null).ToList();
            logger.Info($"[Chasses] {m_zaaps.Count} zaaps de départ possibles");
        }

        #endregion

        #region Client : fenêtre de chasse et chat

        public void Envoyer(Character c, Chasse ch)
        {
            var indices = ch.IndicesEnCours;
            var connus = indices.Take(Math.Min(ch.Jalons.Count + 1, indices.Count))
                .Select(i => (TreasureHuntStep)new TreasureHuntStepFollowDirectionToPOI((sbyte)i.Direction, (ushort)i.PointInteret)).ToArray();
            var jalons = ch.Jalons.Select((m, i) => new TreasureHuntFlag(m, (sbyte)(ch.JalonsFaux.Contains(i)
                ? TreasureHuntFlagStateEnum.TREASURE_HUNT_FLAG_STATE_WRONG : TreasureHuntFlagStateEnum.TREASURE_HUNT_FLAG_STATE_UNKNOWN))).ToArray();
            c.Client?.Send(new TreasureHuntMessage((sbyte)ch.Type, ch.DepartEtape, connus, (sbyte)indices.Count,
                (uint)ch.EtapeCourante, (uint)(ch.Etapes.Count + 1), ch.Essais, jalons));
        }

        public void AnnoncerIndice(Character c, Chasse ch)
        {
            if (ch.EtapeDuCombat)
            {
                Message(c, ch.Type == 1 ? "<b>Toutes les étapes sont terminées !</b> Clique sur « Fouiller » dans la fenêtre de chasse pour révéler le portail."
                                        : "<b>Toutes les étapes sont terminées !</b> Clique sur « Fouiller » dans la fenêtre de chasse pour affronter le Trésor.");
                return;
            }
            var indice = ch.IndiceCourant;
            if (indice == null)
            {
                Message(c, $"Tous les jalons de l'étape {ch.EtapeCourante + 1}/{ch.Etapes.Count} sont posés : clique sur « Fouiller » dans la fenêtre de chasse.");
                return;
            }
            var carte = World.Instance.GetMap(indice.Carte);
            var nom = NomIndice(indice.PointInteret);
            int x = carte?.Position.X ?? 0, y = carte?.Position.Y ?? 0;
            Message(c, $"<b>Chasse au trésor</b> — étape {ch.EtapeCourante + 1}/{ch.Etapes.Count}, indice {ch.Jalons.Count + 1}/{ch.IndicesEnCours.Count} : " +
                       $"« {nom} » se trouve en {{map,{x},{y},1::[{x},{y}]}} {{chasse,aller::[Y aller]}} (ou tape .chasse aller)");
        }

        string NomIndice(int poi)
        {
            var r = m_indices.FirstOrDefault(x => x.Id == poi);
            return r == null ? "Indice" : TextManager.Instance.GetText(r.NameId) ?? "Indice";
        }

        public void SurConnexion(Character c)
        {
            var ch = GetChasse(c);
            if (ch == null)
                return;
            Envoyer(c, ch);
            AnnoncerIndice(c, ch);
        }

        /// <summary>Commande .chasse aller / lien « Y aller » : trajet automatique vers l'indice en cours de SA chasse.</summary>
        public void Aller(Character c)
        {
            var ch = GetChasse(c);
            var indice = ch?.IndiceCourant;
            if (ch == null || indice == null)
            {
                Message(c, ch == null ? "Tu n'as pas de chasse au trésor en cours." : "Aucun indice à rejoindre : fouille (fenêtre de chasse).");
                return;
            }
            Autopilote.Lancer(c, indice.Carte);
        }

        #endregion

        #region Jalons, fouille, abandon

        public void PoserJalon(Character c, int index)
        {
            var ch = GetChasse(c);
            sbyte type = (sbyte)(ch?.Type ?? 0);
            if (ch == null || ch.EtapeDuCombat || ch.IndiceCourant == null)
            {
                c.Client?.Send(new TreasureHuntFlagRequestAnswerMessage(type, (sbyte)TreasureHuntFlagRequestEnum.TREASURE_HUNT_FLAG_ERROR_IMPOSSIBLE, (sbyte)index));
                return;
            }
            if (index != ch.Jalons.Count)
            {
                c.Client?.Send(new TreasureHuntFlagRequestAnswerMessage(type, (sbyte)TreasureHuntFlagRequestEnum.TREASURE_HUNT_FLAG_WRONG_INDEX, (sbyte)index));
                return;
            }
            if (ch.Jalons.Contains(c.Map.Id))
            {
                c.Client?.Send(new TreasureHuntFlagRequestAnswerMessage(type, (sbyte)TreasureHuntFlagRequestEnum.TREASURE_HUNT_FLAG_SAME_MAP, (sbyte)index));
                return;
            }
            ch.Jalons.Add(c.Map.Id);
            Sauver(c.Id, ch);
            c.Client?.Send(new TreasureHuntFlagRequestAnswerMessage(type, (sbyte)TreasureHuntFlagRequestEnum.TREASURE_HUNT_FLAG_OK, (sbyte)index));
            Envoyer(c, ch);
            AnnoncerIndice(c, ch);
        }

        public void RetirerJalon(Character c, int index)
        {
            var ch = GetChasse(c);
            if (ch == null || index < 0 || index >= ch.Jalons.Count)
                return;
            ch.Jalons.RemoveRange(index, ch.Jalons.Count - index);
            ch.JalonsFaux.RemoveAll(x => x >= index);
            Sauver(c.Id, ch);
            Envoyer(c, ch);
            AnnoncerIndice(c, ch);
        }

        public void Fouiller(Character c)
        {
            var ch = GetChasse(c);
            if (ch == null)
                return;
            var type = (sbyte)ch.Type;
            if (ch.EtapeDuCombat)
            {
                if (ch.Type == 1)
                    RevelerPortail(c, ch);
                else
                    LancerCombat(c, ch);
                return;
            }
            var indices = ch.IndicesEnCours;
            if (ch.Jalons.Count < indices.Count)
            {
                c.Client?.Send(new TreasureHuntDigRequestAnswerMessage(type, (sbyte)TreasureHuntDigRequestEnum.TREASURE_HUNT_DIG_ERROR_IMPOSSIBLE));
                Message(c, "Pose d'abord tous les jalons de l'étape.");
                return;
            }

            var faux = indices.Select((ind, i) => ch.Jalons[i] == ind.Carte ? -1 : i).Where(i => i >= 0).ToList();
            if (faux.Count > 0)
            {
                ch.Essais--;
                ch.JalonsFaux = faux;
                if (ch.Essais <= 0)
                {
                    c.Client?.Send(new TreasureHuntDigRequestAnswerMessage(type, (sbyte)TreasureHuntDigRequestEnum.TREASURE_HUNT_DIG_LOST));
                    Terminer(c, "La chasse est perdue : plus d'essais.");
                    return;
                }
                Sauver(c.Id, ch);
                c.Client?.Send(new TreasureHuntDigRequestAnswerFailedMessage(type, (sbyte)TreasureHuntDigRequestEnum.TREASURE_HUNT_DIG_WRONG, (sbyte)faux.Count));
                Envoyer(c, ch);
                Message(c, $"{faux.Count} jalon(s) faux (en rouge) : retire-les et repose-les sur la bonne carte. Essais restants : {ch.Essais}.");
                return;
            }

            ch.EtapeCourante++;
            ch.Jalons.Clear();
            ch.JalonsFaux.Clear();
            ch.Essais = EssaisParEtape;
            Sauver(c.Id, ch);
            c.Client?.Send(new TreasureHuntDigRequestAnswerMessage(type, (sbyte)TreasureHuntDigRequestEnum.TREASURE_HUNT_DIG_NEW_HINT));
            Envoyer(c, ch);
            AnnoncerIndice(c, ch);
        }

        // MYSTIC 19/10 : 5 minutes d'attente après un abandon (évite d'abandonner en boucle pour choisir sa chasse).
        // En mémoire, par personnage : remis à zéro au redémarrage du serveur.
        static readonly TimeSpan DelaiApresAbandon = TimeSpan.FromMinutes(5);
        readonly Dictionary<int, DateTime> m_abandons = new Dictionary<int, DateTime>();

        public void Abandonner(Character c)
        {
            if (GetChasse(c) == null)
                return;
            lock (m_lock)
                m_abandons[c.Id] = DateTime.Now;
            Terminer(c, "Chasse au trésor abandonnée. Tu pourras en commencer une nouvelle dans 5 minutes.");
        }

        /// <summary>Temps restant avant de pouvoir recommencer une chasse après un abandon (zéro si aucun).</summary>
        TimeSpan AttenteAbandon(Character c)
        {
            lock (m_lock)
            {
                if (!m_abandons.TryGetValue(c.Id, out var quand))
                    return TimeSpan.Zero;
                var reste = quand + DelaiApresAbandon - DateTime.Now;
                if (reste > TimeSpan.Zero)
                    return reste;
                m_abandons.Remove(c.Id);
                return TimeSpan.Zero;
            }
        }

        void Terminer(Character c, string message)
        {
            Chasse ch;
            lock (m_lock)
            {
                m_chasses.TryGetValue(c.Id, out ch);
                m_chasses.Remove(c.Id);
            }
            Autopilote.Arreter(c, null);
            WorldServer.Instance.IOTaskPool.AddMessage(() => Database.Execute("DELETE FROM mystic_chasses WHERE CharacterId = @0", c.Id));
            c.Client?.Send(new TreasureHuntFinishedMessage((sbyte)(ch?.Type ?? 0)));
            if (message != null)
                Message(c, message);
        }

        void Sauver(int characterId, Chasse ch)
        {
            var json = JsonConvert.SerializeObject(ch);
            WorldServer.Instance.IOTaskPool.AddMessage(() =>
                Database.Execute("REPLACE INTO mystic_chasses (CharacterId, Donnees) VALUES (@0, @1)", characterId, json));
        }

        #endregion

        #region Combat final et récompenses

        /// <summary>Fin d'une chasse aux portails : le portail apparaît sur la carte du dernier indice (ou est déjà ouvert ailleurs).</summary>
        void RevelerPortail(Character c, Chasse ch)
        {
            if (!PortailManager.Dimensions.ContainsKey(ch.Dimension))
            {
                Terminer(c, "Dimension inconnue : chasse terminée.");
                return;
            }
            var deja = PortailManager.Instance.GetPortail(ch.Dimension) != null;
            var carte = World.Instance.GetMap(ch.DepartEtape) ?? c.Map;
            PortailManager.Instance.Ouvrir(ch.Dimension, carte);
            c.Client?.Send(new TreasureHuntDigRequestAnswerMessage((sbyte)ch.Type, (sbyte)TreasureHuntDigRequestEnum.TREASURE_HUNT_DIG_FINISHED));
            Terminer(c, (deja ? "Un autre aventurier a trouvé le portail avant toi. " : "<b>Tu as trouvé le portail !</b> ") + PortailManager.Instance.Etat(ch.Dimension));
        }

        void LancerCombat(Character c, Chasse ch)
        {
            if (c.IsFighting() || c.IsBusy())
                return;
            lock (m_lock)
                if (!m_enCombat.Add(c.Id))
                    return;
            try
            {
                Autopilote.Arreter(c, null);
                var grade = GradeDuMonstre(ch, out var monstreId);
                if (grade == null)
                {
                    Message(c, "Le gardien du trésor est introuvable (données manquantes).");
                    lock (m_lock) m_enCombat.Remove(c.Id);
                    return;
                }

                var fight = FightManager.Instance.CreatePvMFight(c.Map);
                fight.ChallengersTeam.AddFighter(c.CreateFighter(fight.ChallengersTeam));
                var groupe = new MonsterGroup(0, new ObjectPosition(c.Map, c.Cell, (DirectionsEnum)3));
                fight.DefendersTeam.AddFighter(new MonsterFighter(fight.DefendersTeam, new Monster(grade, groupe)));

                var xp = Experience(c, ch);
                fight.ResultsGenerated += (f, resultats) =>
                {
                    var gagne = f.Winners != null && f.Winners == f.ChallengersTeam;
                    foreach (var r in resultats)
                    {
                        r.Loot.Kamas = 0;
                        if (!gagne || !(r is FightPlayerResult p) || p.Character != c)
                            continue;
                        if (p.ExperienceData != null)
                            p.ExperienceData.ExperienceFightDelta = 0;
                        p.AddEarnedExperience(xp);
                        foreach (var objet in Recompenses(ch))
                            p.Loot.AddItem(new DroppedItem(objet.Item1, (uint)objet.Item2) { IgnoreGeneration = true });
                    }
                };
                fight.FightEnded += f =>
                {
                    lock (m_lock) m_enCombat.Remove(c.Id);
                    var gagne = f.Winners != null && f.Winners == f.ChallengersTeam;
                    if (gagne)
                        DonnerRecompenses(c, ch);
                    else
                        Message(c, "Le Trésor t'a vaincu… Clique à nouveau sur « Fouiller » pour retenter le combat.");
                };

                fight.StartPlacement();
                ContextHandler.HandleGameFightJoinRequestMessage(c.Client, new GameFightJoinRequestMessage(c.Fighter.Id, (ushort)fight.Id));
            }
            catch (Exception ex)
            {
                lock (m_lock) m_enCombat.Remove(c.Id);
                logger.Error($"[Chasses] combat du trésor pour {c.Name} : {ex}");
            }
        }

        MonsterGrade GradeDuMonstre(Chasse ch, out int monstreId)
        {
            monstreId = MonstreTresor;
            if (ch.Type == (int)TreasureHuntTypeEnum.TREASURE_HUNT_LEGENDARY && m_legendaires.TryGetValue(ch.LegendaireId, out var l))
            {
                var grades = Enumerable.Range(1, 10).Select(g => MonsterManager.Instance.GetMonsterGrade(l.MonstreId, g)).Where(g => g != null).ToList();
                if (grades.Count > 0)
                {
                    monstreId = l.MonstreId;
                    return grades.OrderBy(g => Math.Abs(g.Level - l.Niveau)).First();
                }
            }
            return MonsterManager.Instance.GetMonsterGrade(MonstreTresor, Math.Min(10, Math.Max(1, ch.Niveau / 20)));
        }

        static long ExperienceDeBase(int niveau) => (long)niveau * (100 + 2 * niveau) * (100 + 2 * niveau) / 20;

        long Experience(Character c, Chasse ch)
        {
            var niveau = Math.Max(1, Math.Min(c.Level, ch.Niveau));
            if (ch.Type == (int)TreasureHuntTypeEnum.TREASURE_HUNT_LEGENDARY && m_legendaires.TryGetValue(ch.LegendaireId, out var l))
                return (long)(ExperienceDeBase(niveau) * l.RatioXp);
            return ExperienceDeBase(niveau) * 2;
        }

        static int IndexNiveau(int niveau) => Math.Min(9, Math.Max(0, niveau / 20 - 1));

        List<Tuple<int, int>> Recompenses(Chasse ch)
        {
            var liste = new List<Tuple<int, int>>();
            if (ch.Type == (int)TreasureHuntTypeEnum.TREASURE_HUNT_LEGENDARY && m_legendaires.TryGetValue(ch.LegendaireId, out var l))
            {
                liste.Add(Tuple.Create(l.CoffreId, 1));
                return liste;
            }
            liste.Add(Tuple.Create(CoffresDeNiveau[IndexNiveau(ch.Niveau)], ch.Coffres));
            liste.Add(Tuple.Create(PremiereBoite + IndexNiveau(ch.Niveau), 1));
            return liste;
        }

        void DonnerRecompenses(Character c, Chasse ch)
        {
            foreach (var r in Recompenses(ch))
            {
                var t = ItemManager.Instance.TryGetTemplate(r.Item1);
                if (t == null)
                    continue;
                c.Inventory.AddItem(t, r.Item2);
                c.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 21, r.Item2, r.Item1);
            }
            c.Client?.Send(new TreasureHuntDigRequestAnswerMessage((sbyte)ch.Type, (sbyte)TreasureHuntDigRequestEnum.TREASURE_HUNT_DIG_FINISHED));
            Terminer(c, ch.Coffres > 1 && ch.Type == 0
                ? $"<b>Trésor trouvé !</b> Bonus de parcours : {ch.Coffres} coffres."
                : "<b>Trésor trouvé !</b>");
        }

        #endregion

        #region Coffres et boîtes de fragments

        public uint OuvrirCoffre(Character c, int coffreId)
        {
            int baseRoses, multiplicateurCoffre = 1;
            var idx = Array.IndexOf(CoffresDeNiveau, coffreId);
            if (idx >= 0)
                baseRoses = RosesDeBase[idx];
            else
            {
                var l = m_legendaires.Values.FirstOrDefault(x => x.CoffreId == coffreId);
                if (l == null)
                    return 0;
                baseRoses = RosesDeBase[IndexNiveau(Math.Max(20, l.Niveau))];
                multiplicateurCoffre = MultiplicateurCoffreLegendaire;
            }

            int quantite;
            double tirage;
            lock (m_random)
            {
                quantite = (int)Math.Round(baseRoses * multiplicateurCoffre * (0.8 + 0.4 * m_random.NextDouble()));
                tirage = m_random.NextDouble() * 100;
            }
            var multi = 1;
            var cumul = 0.0;
            foreach (var m in Multiplicateurs)
            {
                cumul += m.Item2;
                if (tirage < cumul)
                {
                    multi = m.Item1;
                    break;
                }
            }
            quantite = Math.Max(1, quantite * multi);

            var rose = ItemManager.Instance.TryGetTemplate(RoseDesSables);
            if (rose == null)
                return 0;
            c.Inventory.AddItem(rose, quantite);
            c.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 21, quantite, RoseDesSables);
            if (multi > 1)
                Message(c, $"<b>Coffre exceptionnel : ×{multi} !</b> {quantite} Roses des sables.");
            if (multi >= 100)
                World.Instance.SendAnnounce($"<b>{c.Name} a ouvert un coffre ×{multi} : {quantite} Roses des sables !</b>", Couleur);
            return 1;
        }

        public uint OuvrirBoite(Character c)
        {
            if (m_fragments.Count == 0)
                return 0;
            int nombre;
            List<int> tires = new List<int>();
            lock (m_random)
            {
                nombre = m_random.Next(0, 3);   // 0, 1 ou 2 fragments
                for (var i = 0; i < nombre; i++)
                    tires.Add(m_fragments[m_random.Next(m_fragments.Count)]);
            }
            if (tires.Count == 0)
                Message(c, "La boîte de fragments était vide…");
            foreach (var g in tires.GroupBy(x => x))
            {
                var t = ItemManager.Instance.TryGetTemplate(g.Key);
                if (t == null)
                    continue;
                c.Inventory.AddItem(t, g.Count());
                c.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 21, g.Count(), g.Key);
            }
            return 1;
        }

        #endregion

        #region Chasses légendaires

        public void OuvrirTableLegendaire(Character c)
        {
            if (GetChasse(c) != null)
            {
                Message(c, "Tu as déjà une chasse au trésor en cours : termine-la ou abandonne-la d'abord.");
                return;
            }
            var possibles = m_legendaires.Values.Where(l => c.Inventory.Any(i => i.Template.Id == l.CarteId)).Select(l => (ushort)l.Id).ToArray();
            if (possibles.Length == 0)
            {
                Message(c, "Il te faut une carte de chasse légendaire : assemble-la au Pupitre avec ses fragments (trouvés dans les Boîtes de fragments).");
                return;
            }
            c.Client?.Send(new TreasureHuntShowLegendaryUIMessage(possibles));
        }

        public void DemarrerLegendaire(Character c, int legendaireId)
        {
            if (!m_legendaires.TryGetValue(legendaireId, out var l))
                return;
            if (GetChasse(c) != null)
            {
                Message(c, "Tu as déjà une chasse au trésor en cours.");
                return;
            }
            var carte = c.Inventory.FirstOrDefault(i => i.Template.Id == l.CarteId);
            if (carte == null)
            {
                Message(c, "Tu n'as pas la carte de cette chasse légendaire.");
                return;
            }
            c.Inventory.RemoveItem(carte, 1);
            Demarrer(c, (int)TreasureHuntTypeEnum.TREASURE_HUNT_LEGENDARY, Math.Max(20, l.Niveau), l.Id);
        }

        #endregion

        static void Message(Character c, string texte) => c?.SendServerMessage(texte, Couleur);
    }
}
