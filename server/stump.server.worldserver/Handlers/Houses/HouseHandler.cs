using Stump.DofusProtocol.Messages;
using Stump.Server.WorldServer.Core.Network;
using Stump.Server.WorldServer.Game.Dialogs.House;
using Stump.Server.WorldServer.Game.Houses;
using Stump.Server.WorldServer.Game.Maps;

namespace Stump.Server.WorldServer.Handlers.Houses
{
    public class HouseHandler : WorldHandlerContainer
    {
        [WorldHandler(HouseBuyRequestMessage.Id)]
        public static void HandleHouseBuyRequestMessage(WorldClient client, HouseBuyRequestMessage message)
        {
            (client.Character.Dialog as HouseBuySell)?.BuyHouse(message.ProposedPrice);
        }

        [WorldHandler(HouseSellRequestMessage.Id)]
        public static void HandleHouseSellRequestMessage(WorldClient client, HouseSellRequestMessage message)
        {
            (client.Character.Dialog as HouseBuySell)?.SellHouse(message.Amount, message.ForSale);
        }

        [WorldHandler(HouseSellFromInsideRequestMessage.Id)]
        public static void HandleHouseSellFromInsideRequestMessage(WorldClient client, HouseSellFromInsideRequestMessage message)
        {
            var house = HouseManager.Instance.GetHouseCharacterIsIn(client.Character, out _, out _);
            if (house == null || !house.IsOwner(client.Character))
                return;

            house.SetForSale(message.ForSale, (long)message.Amount);
            house.Save(HouseManager.Instance.Database);
        }

        [WorldHandler(HouseTeleportRequestMessage.Id)]
        public static void HandleHouseTeleportRequestMessage(WorldClient client, HouseTeleportRequestMessage message)
        {
            var character = client.Character;
            if (character.IsBusy() || character.IsFighting())
                return;

            var house = HouseManager.Instance.GetHouse((int)message.HouseId);
            if (house == null || !house.IsOwner(character))
                return;

            var interior = house.Record.InteriorMap;
            var cell = interior?.GetFirstFreeCellNearMiddle();
            if (cell == null)
                return;

            HouseManager.Instance.EnterHouse(character, house, character.Map, character.Cell.Id);
            character.Teleport(interior, cell);
        }

        /// <summary>
        /// Refreshes the door state (owner, lock, price) for everybody on the map.
        /// </summary>
        public static void SendHouseProperties(Map map, House house)
        {
            if (map == null)
                return;

            map.Clients.Send(new HousePropertiesMessage((uint)house.Id, new[] { house.DoorElementId }, house.GetHouseInstanceInformations()));
        }
    }
}
