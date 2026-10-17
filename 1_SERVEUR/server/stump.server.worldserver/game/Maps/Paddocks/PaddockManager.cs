using System.Collections.Generic;
using System.Linq;
using Stump.Server.BaseServer.Database;
using Stump.Server.BaseServer.Initialization;
using Stump.Server.WorldServer.Database;
using Stump.Server.WorldServer.Database.Mounts;
using Stump.Server.WorldServer.Database.World;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Mounts;
using Stump.Server.WorldServer.Game.Guilds;
using NLog;
using Stump.DofusProtocol.Messages;
using Stump.DofusProtocol.Types;
using Stump.Server.WorldServer.Database.Interactives;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Interactives;

namespace Stump.Server.WorldServer.Game.Maps.Paddocks
{
    public class PaddockManager : DataManager<PaddockManager>, ISaveable
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private Dictionary<int, Paddock> m_paddocks = new Dictionary<int, Paddock>();

        [Initialization(InitializationPass.Eighth)]
        public override void Initialize()
        {
            m_paddocks = Database.Query<WorldMapPaddockRecord, MountRecord, WorldMapPaddockRecord>(new WorldMapPaddockRelator().Map, WorldMapPaddockRelator.FetchQuery).Where(x=>x.Map !=null).ToDictionary(entry => entry.Id, x => new Paddock(x));

            // NEKFEU : enclos d'une guilde qui n'existe plus -> de nouveau a vendre
            foreach (var paddock in m_paddocks.Values.Where(x => !x.IsPublicPaddock() && x.GuildId > 0 && x.Guild == null))
            {
                logger.Warn("[Enclos] enclos {0} (map {1}) : guilde {2} introuvable -> remis en vente", paddock.Id, paddock.Map.Id, paddock.GuildId);
                paddock.Release();
            }

            World.Instance.RegisterSaveableInstance(this);
        }

        public IEnumerable<Paddock> GetPaddocks() => m_paddocks.Values;

        public IEnumerable<Paddock> GetGuildPaddocks(Guild guild) => m_paddocks.Values.Where(x => x.HasOwner && x.Guild.Id == guild.Id);

        /// <summary>NEKFEU : une guilde est supprimee -> ses enclos redeviennent a vendre.</summary>
        public void OnGuildDeleted(Guild guild)
        {
            foreach (var paddock in GetGuildPaddocks(guild).ToArray())
            {
                logger.Info("[Enclos] guilde {0} supprimee -> enclos {1} (map {2}) remis en vente", guild.Id, paddock.Id, paddock.Map.Id);
                paddock.Release();
            }
        }

        
        public Paddock GetPaddock(int id)
        {
            Paddock paddock;
            return m_paddocks.TryGetValue(id, out paddock) ? paddock : null;
        }

        /// <summary>Enclos "sur sa map" (Slot 0 : les enclos publics).</summary>
        public Paddock GetPaddockByMap(int mapId)
        {
            return m_paddocks.Values.FirstOrDefault(x => x.Map.Id == mapId && x.Slot == 0);
        }

        #region NEKFEU : portes d'entree -> hub -> 4 enclos

        // Porte d'enclos exterieure ("Acceder") -> hub (sous-zone "Enclos Instance") -> 4 passages -> 4 maps d'enclos -> retour au hub
        // -> escalier -> retour devant la porte. Les maps du hub et des enclos sont partagees par toutes les portes :
        // l'enclos utilise est celui de la porte par laquelle le joueur est entre (MapId = map de la porte, Slot = passage choisi).

        private class PaddockVisit
        {
            public Map EntranceMap;
            public short EntranceCellId;
        }

        private readonly Dictionary<int, PaddockVisit> m_visits = new Dictionary<int, PaddockVisit>();
        private Dictionary<int, int> m_slotMaps;

        /// <summary>Map d'enclos (derriere un hub) -> numero d'emplacement 1-4, lu dans les competences "PaddockHub" des passages.</summary>
        private Dictionary<int, int> SlotMaps
        {
            get
            {
                if (m_slotMaps != null)
                    return m_slotMaps;

                var slots = new Dictionary<int, int>();
                foreach (var record in InteractiveManager.Instance.GetInteractiveSpawns().SelectMany(x => x.CustomSkills).OfType<InteractiveCustomSkillRecord>()
                             .Where(x => x.Type == "PaddockHub"))
                {
                    if (int.TryParse(record.Parameter0, out var slot) && slot > 0 && int.TryParse(record.Parameter1, out var mapId))
                        slots[mapId] = slot;
                }

                logger.Info("[Enclos] {0} maps d'enclos derriere les hubs", slots.Count);
                return m_slotMaps = slots;
            }
        }

        public int GetSlotOfMap(int mapId) => SlotMaps.TryGetValue(mapId, out var slot) ? slot : 0;

        public Paddock GetPaddock(int entranceMapId, int slot) => m_paddocks.Values.FirstOrDefault(x => x.Map.Id == entranceMapId && x.Slot == slot);

        /// <summary>Les 4 enclos d'une porte d'entree.</summary>
        public Paddock[] GetEntrancePaddocks(int entranceMapId) => m_paddocks.Values.Where(x => x.Map.Id == entranceMapId && x.Slot > 0).OrderBy(x => x.Slot).ToArray();

        public void EnterHub(Character character, Map entranceMap, short entranceCellId)
        {
            lock (m_visits)
                m_visits[character.Id] = new PaddockVisit { EntranceMap = entranceMap, EntranceCellId = entranceCellId };
        }

        public Map GetEntranceMap(Character character)
        {
            lock (m_visits)
                return m_visits.TryGetValue(character.Id, out var visit) ? visit.EntranceMap : null;
        }

        /// <summary>Retour devant la porte d'entree ; sans porte memorisee (reconnexion...), vers fallbackMap.</summary>
        public void LeaveHub(Character character, Map fallbackMap, short fallbackCellId)
        {
            PaddockVisit visit;
            lock (m_visits)
            {
                m_visits.TryGetValue(character.Id, out visit);
                m_visits.Remove(character.Id);
            }

            var map = visit?.EntranceMap ?? fallbackMap;
            var cellId = visit?.EntranceCellId ?? fallbackCellId;
            if (map == null)
            {
                character.SendServerMessage("Impossible de retrouver la sortie, utilisez une potion de rappel.");
                return;
            }

            character.Teleport(map, GetArrivalCell(map, cellId));
        }

        /// <summary>
        /// Enclos auquel ce joueur a affaire sur cette map : enclos de sa porte d'entree s'il est dans une map d'enclos
        /// derriere un hub, sinon l'enclos (public) de la map.
        /// </summary>
        public Paddock GetPaddockFor(Character character, Map map)
        {
            var slot = GetSlotOfMap(map.Id);
            if (slot == 0)
                return GetPaddockByMap(map.Id);

            var entrance = GetEntranceMap(character);
            return entrance == null ? null : GetPaddock(entrance.Id, slot);
        }

        /// <summary>Infos d'enclos envoyees a l'arrivee sur une map (null : rien a envoyer).</summary>
        public PaddockPropertiesMessage GetPaddockPropertiesMessage(Character character, Map map)
        {
            var paddock = GetPaddockFor(character, map);
            if (paddock != null)
                return paddock.GetPaddockPropertiesMessage();

            // porte d'entree : les 4 enclos (proprietaires / prix) dans l'infobulle
            var entrance = GetEntrancePaddocks(map.Id);
            if (entrance.Length == 0)
                return null;

            return new PaddockPropertiesMessage(new PaddockInstancesInformations((ushort)entrance.Max(x => x.MaxOutdoorMount),
                (ushort)entrance.Max(x => x.MaxItems), entrance.Select(x => x.GetBuyableInformations()).ToArray()));
        }

        /// <summary>Cellule libre : celle demandee si elle est praticable, sinon une voisine, sinon pres du milieu.</summary>
        public static Database.World.Cell GetArrivalCell(Map map, short cellId)
        {
            if (cellId >= 0 && cellId < map.Cells.Length)
            {
                var cell = map.Cells[cellId];
                if (cell.Walkable && !map.GetTriggers(cell).Any())
                    return cell;

                var near = Cells.MapPoint.GetPoint(cell)
                    .GetAdjacentCells(id => map.Cells[id].Walkable && !map.GetTriggers(map.Cells[id]).Any(), true)
                    .FirstOrDefault();
                if (near != null)
                    return map.Cells[near.CellId];
            }

            return map.GetFirstFreeCellNearMiddle();
        }

        #endregion

        public void Save()
        {
            // NEKFEU : avant, seulement les enclos publics ("les autres sont sauvegardes avec les guildes"),
            // mais Guild.Paddocks n'est jamais rempli -> on sauvegarde tous les enclos ici.
            foreach (var paddock in m_paddocks.Values)
            {
                paddock.Save(Database);
            }
        }
    }
}
