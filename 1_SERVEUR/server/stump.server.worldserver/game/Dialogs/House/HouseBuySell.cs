using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Messages;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Houses;
using Stump.Server.WorldServer.Game.Interactives;
using Stump.Server.WorldServer.Handlers.Dialogs;
using Stump.Server.WorldServer.Handlers.Houses;

namespace Stump.Server.WorldServer.Game.Dialogs.House
{
    /// <summary>
    /// Client purchasable dialog used to buy a house or to set its sale price.
    /// </summary>
    public class HouseBuySell : IDialog
    {
        readonly InteractiveObject m_door;

        public HouseBuySell(Character character, Houses.House house, bool sell, ulong price, InteractiveObject door)
        {
            Character = character;
            House = house;
            Sell = sell;
            Price = price;
            m_door = door;
        }

        public Character Character { get; }

        public Houses.House House { get; }

        public bool Sell { get; }

        public ulong Price { get; }

        public DialogTypeEnum DialogType => DialogTypeEnum.DIALOG_PURCHASABLE;

        public void Open()
        {
            Character.SetDialog(this);
            Character.Client.Send(new PurchasableDialogMessage(!Sell, false, House.Id, House.Id, Price));
        }

        public void Close()
        {
            Character.CloseDialog(this);
            DialogHandler.SendLeaveDialogMessage(Character.Client, DialogType);
        }

        public void BuyHouse(ulong proposedPrice)
        {
            if (Sell || House.IsOwner(Character) || (House.HasOwner && !House.ForSale))
            {
                Close();
                return;
            }

            var price = (ulong)House.CurrentPrice;
            if (proposedPrice != price)
            {
                Close();
                return;
            }

            if (Character.Kamas < price)
            {
                Character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 82);
                Close();
                return;
            }

            if (House.HasOwner)
            {
                var seller = World.Instance.GetCharacter(House.Record.OwnerId.Value);
                if (seller != null)
                    seller.Inventory.AddKamas(price);
                else
                    HouseManager.Instance.Database.Execute("UPDATE characters SET Kamas = Kamas + @0 WHERE Id = @1", (long)price, House.Record.OwnerId.Value);
            }

            Character.Inventory.SubKamas(price);
            House.SetOwner(Character);
            House.Save(HouseManager.Instance.Database);

            Character.Client.Send(new HouseBuyResultMessage(false, true, (uint)House.Id, House.Id, price));
            Character.SendServerMessage("Félicitations, cette maison est maintenant à vous !");
            Close();
            HouseHandler.SendHouseProperties(m_door?.Map, House);
        }

        public void SellHouse(ulong price, bool forSale)
        {
            if (!House.IsOwner(Character))
            {
                Close();
                return;
            }

            House.SetForSale(forSale, (long)price);
            House.Save(HouseManager.Instance.Database);
            Close();
            HouseHandler.SendHouseProperties(m_door?.Map, House);
        }
    }
}
