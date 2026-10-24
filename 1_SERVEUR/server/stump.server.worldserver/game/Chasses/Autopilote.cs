using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using Stump.Core.Timers;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Messages;
using Stump.DofusProtocol.Types;
using Stump.Server.WorldServer.Database.World;
using Stump.Server.WorldServer.Game.Actors;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Maps;
using Stump.Server.WorldServer.Game.Maps.Cells;
using Stump.Server.WorldServer.Game.Maps.Pathfinding;

namespace Stump.Server.WorldServer.Game.Chasses
{
    /// <summary>
    /// MYSTIC 19/10 : autopilote RÉSERVÉ aux chasses au trésor (lien « Y aller » du chat / commande .chasse aller).
    /// Le personnage marche tout seul jusqu'au bord de chaque carte puis passe à la carte voisine, jusqu'à la carte de l'indice en cours.
    /// Arrêt : déplacement du joueur (ContextHandler), combat, dialogue, déconnexion, carte refusée (Frigost, villages…).
    /// </summary>
    public static class Autopilote
    {
        static readonly Logger logger = LogManager.GetCurrentClassLogger();
        static readonly System.Drawing.Color Couleur = System.Drawing.Color.FromArgb(0xE8, 0xC5, 0x47);
        const int MaxCartesRecherche = 6000;
        const int AttenteApresChangement = 1500;   // ms : le client charge la nouvelle carte

        /// <summary>Cartes de la Malle aux Trésors (intérieur) -> sortie devant la porte.</summary>
        static readonly int[] CartesMalle = { 128452097, 128453121, 128451073 };
        public const int CarteSortieMalle = 142088718;

        class Trajet
        {
            public Character Perso;
            public int Cible;
            public MapNeighbour Direction;
            public Path Chemin;
            public TimedTimerEntry Secours;
            public bool Actif = true;
            public readonly HashSet<int> Points = new HashSet<int>();   // cartes marquées sur la minimap
            public bool Boussole;
        }

        static readonly Dictionary<int, Trajet> m_trajets = new Dictionary<int, Trajet>();
        static readonly HashSet<int> m_abonnes = new HashSet<int>();

        public static bool EstEnCours(Character c)
        {
            lock (m_trajets)
                return m_trajets.TryGetValue(c.Id, out var t) && t.Actif;
        }

        public static void Lancer(Character c, int carteCible)
        {
            Arreter(c, null);
            if (c.IsFighting() || c.IsBusy())
            {
                Message(c, "Impossible de lancer le trajet pendant un combat ou une action en cours.");
                return;
            }
            if (c.Map.Id == carteCible)
            {
                Message(c, "Tu es déjà sur la carte de l'indice : pose ton jalon !");
                return;
            }
            // PADGREF 24/10 (retour utilisateur : « Y aller depuis la Malle aux Trésors nous téléporte dehors, faut pas ») : plus de
            // téléportation hors de la Malle ; le trajet automatique part seulement de l'extérieur
            if (CartesMalle.Contains(c.Map.Id))
            {
                Message(c, "Tu es dans la Malle aux Trésors : sors par la porte, puis reclique sur « Y aller ».");
                return;
            }

            var t = new Trajet { Perso = c, Cible = carteCible };
            lock (m_trajets)
                m_trajets[c.Id] = t;

            lock (m_abonnes)
                if (m_abonnes.Add(c.Id))
                    c.StopMoving += SurFinDeplacement;

            var cible = World.Instance.GetMap(carteCible);
            Message(c, $"Trajet automatique vers [{cible?.Position.X},{cible?.Position.Y}] : bouge toi-même ou entre en combat pour l'arrêter.");
            c.Map.Area.ExecuteInContext(() => Etape(t));
        }

        public static void Arreter(Character c, string raison)
        {
            Trajet t;
            lock (m_trajets)
            {
                if (!m_trajets.TryGetValue(c.Id, out t))
                    return;
                m_trajets.Remove(c.Id);
            }
            t.Actif = false;
            t.Secours?.Stop();
            EffacerTrajet(t);
            if (raison != null)
                Message(c, raison);
        }

        static bool Courant(Trajet t)
        {
            lock (m_trajets)
                return t.Actif && m_trajets.TryGetValue(t.Perso.Id, out var x) && x == t;
        }

        static void Etape(Trajet t)
        {
            if (!Courant(t))
                return;
            var c = t.Perso;
            try
            {
                if (c.Client == null || !c.IsInWorld)
                {
                    Arreter(c, null);
                    return;
                }
                if (c.IsFighting() || c.IsBusy())
                {
                    Arreter(c, "Trajet automatique arrêté (combat ou action en cours). Reclique sur le lien pour repartir.");
                    return;
                }
                if (c.Map.Id == t.Cible)
                {
                    Arreter(c, "Tu es arrivé sur la carte de l'indice : pose ton jalon dans la fenêtre de chasse !");
                    return;
                }

                // dans la Malle aux Trésors (intérieur) : plus de téléportation dehors (PADGREF 24/10)
                if (CartesMalle.Contains(c.Map.Id))
                {
                    Arreter(c, "Tu es dans la Malle aux Trésors : sors par la porte, puis reclique sur « Y aller ».");
                    return;
                }

                var route = Route(c, c.Map, t.Cible, out var cartes);
                if (route == null || route.Count == 0)
                {
                    var zaap = ChasseManager.Instance.ZaapProche(c, World.Instance.GetMap(t.Cible));
                    Arreter(c, "Aucun chemin à pied vers l'indice depuis ici : rapproche-toi" + (zaap != "" ? $" ({zaap})" : "") + " puis reclique sur « Y aller ».");
                    return;
                }

                AfficherTrajet(t, cartes);
                t.Direction = route[0];
                var sorties = c.Map.Cells.Where(x => x.Walkable && !x.NonWalkableDuringRP && EstSortie(x, t.Direction)).ToList();
                if (sorties.Count == 0)
                {
                    Arreter(c, "Le trajet automatique est bloqué sur cette carte : avance un peu puis reclique sur le lien.");
                    return;
                }

                if (sorties.Any(x => x.Id == c.Cell.Id))
                {
                    ChangerDeCarte(t);
                    return;
                }

                var pf = new Pathfinder(c.Map.CellsInfoProvider);
                Path chemin = null;
                foreach (var s in sorties.OrderBy(x => new MapPoint(x).ManhattanDistanceTo(new MapPoint(c.Cell))).Take(12))
                {
                    var p = pf.FindPath(c.Cell.Id, s.Id, true);
                    if (p != null && !p.IsEmpty() && p.EndCell.Id == s.Id)
                    {
                        chemin = p;
                        break;
                    }
                }
                if (chemin == null)
                {
                    Arreter(c, "Le trajet automatique ne trouve pas de sortie : avance un peu puis reclique sur le lien.");
                    return;
                }

                t.Chemin = chemin;
                if (!c.StartMove(chemin))
                {
                    Arreter(c, "Le trajet automatique ne peut pas avancer (personnage bloqué).");
                    return;
                }
                // secours si le client ne confirme pas la fin du déplacement
                var duree = chemin.MPCost * 520 + 2500;
                t.Secours?.Stop();
                t.Secours = c.Map.Area.CallDelayed(duree, () =>
                {
                    if (Courant(t) && t.Chemin == chemin)
                    {
                        if (c.IsMoving())
                            c.StopMove();
                        else
                            SurFinDeplacement(c, chemin, false);
                    }
                });
            }
            catch (Exception ex)
            {
                logger.Error($"[Chasses] autopilote de {c.Name} : {ex}");
                Arreter(c, "Trajet automatique arrêté (erreur).");
            }
        }

        static void SurFinDeplacement(ContextActor acteur, Path chemin, bool annule)
        {
            if (!(acteur is Character c))
                return;
            Trajet t;
            lock (m_trajets)
                if (!m_trajets.TryGetValue(c.Id, out t))
                    return;
            if (!t.Actif || t.Chemin == null || chemin != t.Chemin)
                return;
            t.Chemin = null;
            t.Secours?.Stop();
            if (annule)
            {
                Arreter(c, "Trajet automatique arrêté. Reclique sur le lien pour repartir.");
                return;
            }
            ChangerDeCarte(t);
        }

        static void ChangerDeCarte(Trajet t)
        {
            var c = t.Perso;
            if (!Courant(t))
                return;
            if (!c.Teleport(t.Direction))
            {
                Arreter(c, null);   // Character.Teleport a déjà expliqué le refus (Frigost, village…)
                return;
            }
            Planifier(t, AttenteApresChangement);
        }

        static void Planifier(Trajet t, int delai)
        {
            var c = t.Perso;
            c.Map.Area.CallDelayed(delai, () => Etape(t));
        }

        static bool EstSortie(Cell cell, MapNeighbour d)
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

        static readonly MapNeighbour[] Directions = { MapNeighbour.Right, MapNeighbour.Bottom, MapNeighbour.Left, MapNeighbour.Top };

        /// <summary>Plus court chemin (en cartes) par les cartes voisines ; renvoie la liste des directions à prendre.</summary>
        static List<MapNeighbour> Route(Character c, Map depart, int cible, out List<Map> cartes)
        {
            cartes = null;
            var precedent = new Dictionary<int, Tuple<int, MapNeighbour>> { [depart.Id] = null };
            var file = new Queue<Map>();
            file.Enqueue(depart);
            while (file.Count > 0 && precedent.Count < MaxCartesRecherche)
            {
                var m = file.Dequeue();
                if (m.Id == cible)
                    break;
                foreach (var d in Directions)
                {
                    Map v;
                    try { v = m.GetNeighbouringMap(d); } catch { v = null; }
                    if (v == null || precedent.ContainsKey(v.Id))
                        continue;
                    if (v.Id != cible && !ChasseManager.CartePraticable(c, v))
                        continue;
                    if (!m.Cells.Any(x => x.Walkable && !x.NonWalkableDuringRP && EstSortie(x, d)))
                        continue;
                    precedent[v.Id] = Tuple.Create(m.Id, d);
                    file.Enqueue(v);
                }
            }
            if (!precedent.ContainsKey(cible))
                return null;
            var route = new List<MapNeighbour>();
            cartes = new List<Map>();
            for (var id = cible; precedent[id] != null; id = precedent[id].Item1)
            {
                route.Add(precedent[id].Item2);
                cartes.Add(World.Instance.GetMap(id));
            }
            route.Reverse();
            cartes.Reverse();
            return route;
        }

        // ---- trajet sur la minimap / carte du monde ----
        // Points bleus : messages « suivi d'un membre de groupe » (CompassUpdatePartyMemberMessage) avec un identifiant
        // fictif par carte (BaseIdPoint + id de carte) : le client accepte autant de points qu'on veut, sans modification.
        // Boussole jaune (COMPASS_TYPE_SIMPLE) sur la carte de l'indice. Tout est effacé à l'arrêt ou à l'arrivée.
        const ulong BaseIdPoint = 9000000000000UL;

        static void AfficherTrajet(Trajet t, List<Map> cartes)
        {
            var client = t.Perso.Client;
            if (client == null || cartes == null)
                return;
            var voulues = new HashSet<int>(cartes.Where(m => m != null && m.Id != t.Cible).Select(m => m.Id));
            foreach (var id in t.Points.Where(x => !voulues.Contains(x)).ToList())
            {
                var m = World.Instance.GetMap(id);
                client.Send(new CompassUpdatePartyMemberMessage((sbyte)CompassTypeEnum.COMPASS_TYPE_PARTY,
                    new MapCoordinates((short)(m?.Position.X ?? 0), (short)(m?.Position.Y ?? 0)), BaseIdPoint + (ulong)id, false));
                t.Points.Remove(id);
            }
            foreach (var m in cartes.Where(m => m != null && voulues.Contains(m.Id) && !t.Points.Contains(m.Id)))
            {
                client.Send(new CompassUpdatePartyMemberMessage((sbyte)CompassTypeEnum.COMPASS_TYPE_PARTY,
                    new MapCoordinates((short)m.Position.X, (short)m.Position.Y), BaseIdPoint + (ulong)m.Id, true));
                t.Points.Add(m.Id);
            }
            if (!t.Boussole)
            {
                var cible = World.Instance.GetMap(t.Cible);
                if (cible != null)
                {
                    client.Send(new CompassUpdateMessage((sbyte)CompassTypeEnum.COMPASS_TYPE_SIMPLE,
                        new MapCoordinates((short)cible.Position.X, (short)cible.Position.Y)));
                    t.Boussole = true;
                }
            }
        }

        static void EffacerTrajet(Trajet t)
        {
            var client = t.Perso?.Client;
            if (client == null)
                return;
            try
            {
                foreach (var id in t.Points)
                {
                    var m = World.Instance.GetMap(id);
                    client.Send(new CompassUpdatePartyMemberMessage((sbyte)CompassTypeEnum.COMPASS_TYPE_PARTY,
                        new MapCoordinates((short)(m?.Position.X ?? 0), (short)(m?.Position.Y ?? 0)), BaseIdPoint + (ulong)id, false));
                }
                if (t.Boussole)
                    client.Send(new CompassResetMessage((sbyte)CompassTypeEnum.COMPASS_TYPE_SIMPLE));
            }
            catch (Exception ex)
            {
                logger.Warn($"[Chasses] effacement du trajet : {ex.Message}");
            }
            t.Points.Clear();
            t.Boussole = false;
        }

        static void Message(Character c, string texte) => c?.SendServerMessage(texte, Couleur);
    }
}
