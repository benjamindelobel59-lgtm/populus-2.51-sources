using System;
using Stump.DofusProtocol.Types;
using Stump.Server.WorldServer.Database.Houses;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;

namespace Stump.Server.WorldServer.Game.Houses
{
    public class House
    {
        public House(HouseRecord record)
        {
            Record = record;
        }

        public HouseRecord Record { get; }

        public int Id => Record.Id;

        public int DoorMapId => Record.DoorMapId;

        public int DoorElementId => Record.DoorElementId;

        public int InteriorMapId => Record.InteriorMapId;

        public int ModelId => Record.ModelId;

        public bool IsRecordDirty { get; private set; }

        public bool HasOwner => Record.OwnerId.HasValue;

        public bool Locked
        {
            get { return Record.Locked; }
            set { Record.Locked = value; IsRecordDirty = true; }
        }

        public bool ForSale => Record.ForSale;

        public long SalePrice => Record.SalePrice;

        /// <summary>
        /// Price shown to a buyer : the base price when nobody owns the house, the owner's price when it is for sale.
        /// </summary>
        public long CurrentPrice => HasOwner ? (ForSale ? Record.SalePrice : 0) : Record.Price;

        public bool IsOwner(Character character) => HasOwner && character != null && Record.OwnerId == character.Id;

        public bool CanEnter(Character character) => IsOwner(character) || !Locked;

        public void SetOwner(Character character)
        {
            Record.OwnerId = character.Id;
            Record.OwnerName = character.Name;
            Record.PurchaseDate = DateTime.Now;
            Record.Locked = false;
            Record.ForSale = false;
            Record.SalePrice = 0;
            IsRecordDirty = true;
        }

        public void SetForSale(bool forSale, long price)
        {
            Record.ForSale = forSale;
            Record.SalePrice = forSale ? price : 0;
            IsRecordDirty = true;
        }

        public HouseInstanceInformations GetHouseInstanceInformations()
            => new HouseInstanceInformations(false, Locked, false, Id, Record.OwnerName ?? "", CurrentPrice);

        public HouseOnMapInformations GetHouseOnMapInformations()
            => new HouseOnMapInformations((uint)Id, (ushort)ModelId, new[] { DoorElementId }, new[] { GetHouseInstanceInformations() });

        public HouseInformationsInside GetHouseInformationsInside()
        {
            var map = Record.DoorMap;
            return new HouseInformationsInside((uint)Id, (ushort)ModelId, GetHouseInstanceInformations(),
                (short)(map?.Position.X ?? 0), (short)(map?.Position.Y ?? 0));
        }

        public void Save(ORM.Database database)
        {
            if (!IsRecordDirty)
                return;

            database.Update(Record);
            IsRecordDirty = false;
        }
    }
}
