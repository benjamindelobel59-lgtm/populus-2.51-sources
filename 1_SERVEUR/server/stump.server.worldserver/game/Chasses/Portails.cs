using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using Stump.Core.Reflection;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Types;
using Stump.Server.BaseServer.Initialization;
using Stump.Server.WorldServer.Game.Actors.Interfaces;
using Stump.Server.WorldServer.Game.Actors.Look;
using Stump.Server.WorldServer.Game.Actors.RolePlay;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Maps;
using Stump.Server.WorldServer.Game.Maps.Cells;

namespace Stump.Server.WorldServer.Game.Chasses
{
    /// <summary>
    /// MYSTIC 19/10 : portails des dimensions divines (Enutrosor, Srambad, Xélorium, Ecaflipus).
    /// Un portail apparaît à la fin d'une chasse aux portails (lancée depuis la Salle de localisation des portails) sur la carte du
    /// dernier indice, pour tout le serveur. Il se referme au bout de 8 heures ou après 110 passages.
    /// Un seul portail par dimension à la fois. Sauvegardé dans mystic_portails (reprend au redémarrage).
    /// Côté client : acteur GameRolePlayPortalInformations (menu Parler / Utiliser, confirmation, PortalUseRequestMessage).
    /// </summary>
    public class PortailManager : Singleton<PortailManager>
    {
        static readonly Logger logger = LogManager.GetCurrentClassLogger();
        static Stump.ORM.Database Database => WorldServer.Instance.DBAccessor.Database;
        static readonly System.Drawing.Color Couleur = System.Drawing.Color.FromArgb(0x9B, 0x7B, 0xE8);

        public static readonly TimeSpan DureeDeVie = TimeSpan.FromHours(8);
        public const int PassagesMax = 110;

        public class Dimension
        {
            public int AreaId;
            public string Nom;
            public string Look;
            public int CarteEntree;
            public int CaseEntree;
        }

        /// <summary>Les 4 dimensions : apparence du portail (Npcs.d2o du client) et entrée (anciens téléporteurs de la Salle de localisation).</summary>
        public static readonly Dictionary<int, Dimension> Dimensions = new[]
        {
            new Dimension { AreaId = 53, Nom = "Enutrosor", Look = "{2720}", CarteEntree = 131597312, CaseEntree = 483 },
            new Dimension { AreaId = 54, Nom = "Srambad",   Look = "{2893}", CarteEntree = 134351108, CaseEntree = 372 },
            new Dimension { AreaId = 55, Nom = "Xélorium",  Look = "{3031}", CarteEntree = 145100034, CaseEntree = 269 },
            new Dimension { AreaId = 59, Nom = "Ecaflipus", Look = "{3489}", CarteEntree = 161351684, CaseEntree = 371 },
        }.ToDictionary(x => x.AreaId);

        class Ligne
        {
            public int AreaId { get; set; }
            public int MapId { get; set; }
            public int CellId { get; set; }
            public DateTime Apparition { get; set; }
            public int Passages { get; set; }
        }

        readonly object m_lock = new object();
        readonly Dictionary<int, Portail> m_portails = new Dictionary<int, Portail>();

        [Initialization(typeof(World))]
        public void Initialize()
        {
            try
            {
                Database.Execute("CREATE TABLE IF NOT EXISTS mystic_portails (AreaId INT NOT NULL PRIMARY KEY, MapId INT NOT NULL, CellId INT NOT NULL, " +
                                 "Apparition DATETIME NOT NULL, Passages INT NOT NULL DEFAULT 0) DEFAULT CHARSET=utf8");
                foreach (var l in Database.Fetch<Ligne>("SELECT AreaId, MapId, CellId, Apparition, Passages FROM mystic_portails"))
                {
                    var map = World.Instance.GetMap(l.MapId);
                    if (map == null || !Dimensions.ContainsKey(l.AreaId) || DateTime.Now - l.Apparition >= DureeDeVie || l.Passages >= PassagesMax)
                    {
                        Supprimer(l.AreaId);
                        continue;
                    }
                    Placer(l.AreaId, map, l.CellId, l.Apparition, l.Passages);
                }
                WorldServer.Instance.IOTaskPool.CallPeriodically(60 * 1000, VerifierExpirations);
                logger.Info($"[Portails] {m_portails.Count} portail(s) ouvert(s)");
            }
            catch (Exception ex)
            {
                logger.Error($"[Portails] initialisation impossible : {ex}");
            }
        }

        public Portail GetPortail(int areaId)
        {
            lock (m_lock)
                return m_portails.TryGetValue(areaId, out var p) ? p : null;
        }

        /// <summary>Fait apparaître le portail de la dimension sur cette carte (s'il n'est pas déjà ouvert). Renvoie le portail ouvert.</summary>
        public Portail Ouvrir(int areaId, Map map)
        {
            var existant = GetPortail(areaId);
            if (existant != null)
                return existant;
            var cell = map.GetRandomFreeCell();
            var p = Placer(areaId, map, cell.Id, DateTime.Now, 0);
            Database.Execute("REPLACE INTO mystic_portails (AreaId, MapId, CellId, Apparition, Passages) VALUES (@0, @1, @2, @3, 0)",
                areaId, map.Id, cell.Id, p.Apparition);
            logger.Info($"[Portails] portail de {Dimensions[areaId].Nom} ouvert sur la carte {map.Id} [{map.Position.X},{map.Position.Y}]");
            return p;
        }

        Portail Placer(int areaId, Map map, int cellId, DateTime apparition, int passages)
        {
            var d = Dimensions[areaId];
            var cell = map.Cells[cellId];
            var p = new Portail(map.GetNextContextualId(), d, new ObjectPosition(map, cell, DirectionsEnum.DIRECTION_SOUTH_EAST), apparition, passages);
            lock (m_lock)
                m_portails[areaId] = p;
            map.Area.ExecuteInContext(() => map.Enter(p));
            return p;
        }

        void Fermer(Portail p, string raison)
        {
            lock (m_lock)
            {
                if (!m_portails.TryGetValue(p.Dimension.AreaId, out var x) || x != p)
                    return;
                m_portails.Remove(p.Dimension.AreaId);
            }
            var map = p.Map;
            map?.Area.ExecuteInContext(() => { if (map.IsActor(p)) map.Leave(p); });
            Supprimer(p.Dimension.AreaId);
            logger.Info($"[Portails] portail de {p.Dimension.Nom} refermé ({raison})");
        }

        static void Supprimer(int areaId)
            => WorldServer.Instance.IOTaskPool.AddMessage(() => Database.Execute("DELETE FROM mystic_portails WHERE AreaId = @0", areaId));

        void VerifierExpirations()
        {
            List<Portail> liste;
            lock (m_lock)
                liste = m_portails.Values.ToList();
            foreach (var p in liste.Where(x => x.Restant <= TimeSpan.Zero))
                Fermer(p, "8 heures écoulées");
        }

        /// <summary>Clic « Utiliser » sur le portail (PortalUseRequestMessage).</summary>
        public void Utiliser(Character c, int portalId)
        {
            Portail p;
            lock (m_lock)
                p = m_portails.Values.FirstOrDefault(x => x.Dimension.AreaId == portalId);
            if (p == null || p.Map != c.Map)
            {
                c.SendServerMessage("Impossible, le portail s'est refermé.", Couleur);
                return;
            }
            if (c.IsFighting() || c.IsBusy())
                return;
            if (p.Restant <= TimeSpan.Zero || p.Passages >= PassagesMax)
            {
                Fermer(p, "épuisé");
                c.SendServerMessage("Impossible, le portail s'est refermé.", Couleur);
                return;
            }
            var entree = World.Instance.GetMap(p.Dimension.CarteEntree);
            if (entree == null)
            {
                c.SendServerMessage("Cette dimension n'est pas disponible pour le moment.", Couleur);
                return;
            }
            if (!c.Teleport(new ObjectPosition(entree, entree.Cells[p.Dimension.CaseEntree], c.Direction)))
                return;
            p.Passages++;
            var passages = p.Passages;
            WorldServer.Instance.IOTaskPool.AddMessage(() => Database.Execute("UPDATE mystic_portails SET Passages = @1 WHERE AreaId = @0", p.Dimension.AreaId, passages));
            if (passages >= PassagesMax)
                Fermer(p, $"{PassagesMax} passages");
        }

        /// <summary>Texte d'état du portail d'une dimension (chat).</summary>
        public string Etat(int areaId)
        {
            var d = Dimensions[areaId];
            var p = GetPortail(areaId);
            if (p == null)
                return $"Aucun portail vers <b>{d.Nom}</b> n'est ouvert : lance une chasse aux portails pour le trouver.";
            var pos = p.Map.Position;
            var reste = p.Restant;
            return $"Le portail vers <b>{d.Nom}</b> est ouvert en {{map,{pos.X},{pos.Y},1::[{pos.X},{pos.Y}]}} ({p.Map.SubArea?.Record?.Name}) : " +
                   $"{PassagesMax - p.Passages} passage(s) restant(s), se referme dans {(int)reste.TotalHours} h {reste.Minutes:00} min.";
        }

        public void Informer(Character c, int areaId) => c.SendServerMessage(Etat(areaId), Couleur);
    }

    /// <summary>MYSTIC 19/10 : portail de dimension posé sur une carte (acteur vu par le client comme un vrai portail).</summary>
    public class Portail : RolePlayActor, IInteractNpc
    {
        public Portail(int id, PortailManager.Dimension dimension, ObjectPosition position, DateTime apparition, int passages)
        {
            Id = id;
            Dimension = dimension;
            Position = position;
            Apparition = apparition;
            Passages = passages;
            Look = ActorLook.Parse(dimension.Look);
        }

        public override int Id { get; protected set; }
        public override ActorLook Look { get; set; }
        public PortailManager.Dimension Dimension { get; }
        public DateTime Apparition { get; }
        public int Passages { get; set; }
        public TimeSpan Restant => Apparition + PortailManager.DureeDeVie - DateTime.Now;

        public override GameContextActorInformations GetGameContextActorInformations(Character character)
            => new GameRolePlayPortalInformations(Id, Look.GetEntityLook(), GetEntityDispositionInformations(),
                new PortalInformation(Dimension.AreaId, (short)Dimension.AreaId));

        // « Parler » : état du portail dans le chat
        public void InteractWith(NpcActionTypeEnum actionType, Character dialoguer) => PortailManager.Instance.Informer(dialoguer, Dimension.AreaId);

        public bool CanInteractWith(NpcActionTypeEnum action, Character dialoguer) => dialoguer.Map == Map;

        public override string ToString() => $"Portail {Dimension.Nom} ({Id})";
    }
}
