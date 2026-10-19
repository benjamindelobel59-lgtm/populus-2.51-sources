using System.Collections.Generic;
using System.Linq;
using Stump.Server.BaseServer.Database;
using Stump.Server.BaseServer.Initialization;
using Stump.Server.WorldServer.Database;
using Stump.Server.WorldServer.Database.Houses;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Maps;

namespace Stump.Server.WorldServer.Game.Houses
{
    public class HouseManager : DataManager<HouseManager>, ISaveable
    {
        Dictionary<int, House> m_houses = new Dictionary<int, House>();
        Dictionary<(int mapId, int elementId), House> m_housesByDoor = new Dictionary<(int, int), House>();
        Dictionary<int, House[]> m_housesByMap = new Dictionary<int, House[]>();
        readonly Dictionary<int, HouseVisit> m_charactersInside = new Dictionary<int, HouseVisit>();

        struct HouseVisit
        {
            public House House;
            public Map ReturnMap;
            public short ReturnCellId;
        }

        [Initialization(InitializationPass.Eighth, "Load houses")]
        public override void Initialize()
        {
            m_houses = Database.Query<HouseRecord>(HouseRelator.FetchQuery).ToDictionary(x => x.Id, x => new House(x));
            m_housesByDoor = m_houses.Values.GroupBy(x => (x.DoorMapId, x.DoorElementId)).ToDictionary(x => x.Key, x => x.First());
            m_housesByMap = m_houses.Values.GroupBy(x => x.DoorMapId).ToDictionary(x => x.Key, x => x.ToArray());
            m_interieurs = new HashSet<int>(m_houses.Values.Select(x => x.InteriorMapId));
            World.Instance.RegisterSaveableInstance(this);
        }

        // MYSTIC 19/10 : 797 maisons pour 105 cartes d'intérieur (jusqu'à 38 maisons sur la même carte). Chaque maison a son propre
        // intérieur : deux joueurs sur une même carte d'intérieur ne se voient que s'ils sont entrés par la MÊME maison (comme les
        // havre-sacs). L'atelier public d'Astrub (191890437) reste commun à tous.
        HashSet<int> m_interieurs = new HashSet<int>();
        const int AtelierPublicAstrub = 191890437;

        public bool CanBeSeenInHouse(WorldObject x, Character character)
        {
            if (!(x is Character autre) || autre == character || autre.Map == null || autre.Map.Id == AtelierPublicAstrub ||
                !m_interieurs.Contains(autre.Map.Id))
                return true;
            try
            {
                int? ma = m_charactersInside.TryGetValue(autre.Id, out var a) ? a.House?.Record.Id : null;
                int? mb = m_charactersInside.TryGetValue(character.Id, out var b) ? b.House?.Record.Id : null;
                return ma == mb;
            }
            catch (System.InvalidOperationException)
            {
                return true;   // liste modifiée pendant la lecture : on ne cache personne
            }
        }

        public House GetHouse(int id) => m_houses.TryGetValue(id, out var house) ? house : null;

        // MYSTIC 19/10 : toutes les maisons (Potion de Foyer de Guilde infinie)
        public IEnumerable<House> GetHouses() => m_houses.Values;

        public House GetHouseByDoor(int mapId, int elementId) => m_housesByDoor.TryGetValue((mapId, elementId), out var house) ? house : null;

        public House[] GetHousesOnMap(int mapId) => m_housesByMap.TryGetValue(mapId, out var houses) ? houses : new House[0];

        public House GetHouseByInterior(int mapId) => m_houses.Values.FirstOrDefault(x => x.InteriorMapId == mapId);

        public void EnterHouse(Character character, House house, Map returnMap, short returnCellId)
        {
            m_charactersInside[character.Id] = new HouseVisit { House = house, ReturnMap = returnMap, ReturnCellId = returnCellId };
        }

        public House GetHouseCharacterIsIn(Character character, out Map returnMap, out short returnCellId)
        {
            if (m_charactersInside.TryGetValue(character.Id, out var visit))
            {
                returnMap = visit.ReturnMap;
                returnCellId = visit.ReturnCellId;
                return visit.House;
            }

            returnMap = null;
            returnCellId = 0;
            return null;
        }

        public void ExitHouse(Character character) => m_charactersInside.Remove(character.Id);

        /// <summary>
        /// Sends the character back in front of the door he entered from. When he did not come through
        /// a door (teleport, reconnection), the first house using this interior is used. Returns false when
        /// the character is not in a house interior.
        /// </summary>
        public bool LeaveHouse(Character character)
        {
            var house = GetHouseCharacterIsIn(character, out var returnMap, out var returnCellId);
            if (house == null || character.Map.Id != house.InteriorMapId)
            {
                house = GetHouseByInterior(character.Map.Id);
                returnMap = house?.Record.DoorMap;
                returnCellId = -1;
            }

            if (house == null || returnMap == null)
                return false;

            var cell = returnCellId >= 0 ? returnMap.GetCell(returnCellId) : null;
            if (cell == null || !cell.Walkable)
                cell = returnMap.GetFirstFreeCellNearMiddle();

            ExitHouse(character);
            character.Teleport(returnMap, cell);
            return true;
        }

        /// <summary>
        /// Cell where a player entering the interior appears : next to a house exit cell when there is one.
        /// </summary>
        public Database.World.Cell GetArrivalCell(Map interior)
        {
            var exit = interior.GetTriggers().OfType<Maps.Cells.Triggers.HouseExitTrigger>().FirstOrDefault();
            if (exit != null)
            {
                var near = Maps.Cells.MapPoint.GetPoint(exit.Position.Cell)
                    .GetAdjacentCells(id => interior.Cells[id].Walkable && !interior.GetTriggers(interior.Cells[id]).Any(), true)
                    .FirstOrDefault();
                if (near != null)
                    return interior.Cells[near.CellId];
            }

            return interior.GetFirstFreeCellNearMiddle();
        }

        public void Save()
        {
            foreach (var house in m_houses.Values)
                house.Save(Database);
        }
    }
}
