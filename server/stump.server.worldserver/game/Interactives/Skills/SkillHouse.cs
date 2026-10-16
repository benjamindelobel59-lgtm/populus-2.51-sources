using Stump.DofusProtocol.Enums;
using Stump.Server.BaseServer.Database;
using Stump.Server.WorldServer.Database.Interactives;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Dialogs.House;
using Stump.Server.WorldServer.Game.Houses;
using Stump.Server.WorldServer.Handlers.Houses;

namespace Stump.Server.WorldServer.Game.Interactives.Skills
{
    /// <summary>
    /// Base of the skills placed on a house door (the door interactive id is the house DoorElementId).
    /// </summary>
    public abstract class SkillHouseBase : CustomSkill
    {
        protected SkillHouseBase(int id, InteractiveCustomSkillRecord record, InteractiveObject interactiveObject)
            : base(id, record, interactiveObject)
        {
        }

        protected House House => HouseManager.Instance.GetHouseByDoor(InteractiveObject.Map.Id, InteractiveObject.Id);

        public override bool AreConditionsFilled(Character character)
        {
            var house = House;
            return house != null && IsAvailable(house, character) && base.AreConditionsFilled(character);
        }

        protected abstract bool IsAvailable(House house, Character character);
    }

    /// <summary>"Entrer" : the owner, or anybody when the house is not locked.</summary>
    [Discriminator("House", typeof(Skill), typeof(int), typeof(InteractiveCustomSkillRecord), typeof(InteractiveObject))]
    public class SkillHouseEnter : SkillHouseBase
    {
        public SkillHouseEnter(int id, InteractiveCustomSkillRecord record, InteractiveObject interactiveObject)
            : base(id, record, interactiveObject)
        {
        }

        protected override bool IsAvailable(House house, Character character) => house.Record.InteriorMap != null;

        public override int StartExecute(Character character)
        {
            var house = House;
            if (house == null || character.IsBusy())
                return -1;

            if (!house.CanEnter(character))
            {
                character.SendServerMessage("Cette maison est verrouillée.");
                return -1;
            }

            var interior = house.Record.InteriorMap;
            var cell = interior == null ? null : HouseManager.Instance.GetArrivalCell(interior);
            if (cell == null)
            {
                character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 1);
                return -1;
            }

            HouseManager.Instance.EnterHouse(character, house, InteractiveObject.Map, character.Cell.Id);
            character.Teleport(interior, cell);
            character.SendServerMessage("Pour sortir de la maison, marchez sur la sortie ou tapez .sortirmaison");
            return base.StartExecute(character);
        }
    }

    /// <summary>"Acheter" : a house without owner, or one put up for sale by its owner.</summary>
    [Discriminator("HouseBuy", typeof(Skill), typeof(int), typeof(InteractiveCustomSkillRecord), typeof(InteractiveObject))]
    public class SkillHouseBuy : SkillHouseBase
    {
        public SkillHouseBuy(int id, InteractiveCustomSkillRecord record, InteractiveObject interactiveObject)
            : base(id, record, interactiveObject)
        {
        }

        protected override bool IsAvailable(House house, Character character)
            => !house.IsOwner(character) && (!house.HasOwner || house.ForSale);

        public override int StartExecute(Character character)
        {
            var house = House;
            if (house == null || character.IsBusy() || !IsAvailable(house, character))
                return -1;

            new HouseBuySell(character, house, false, (ulong)house.CurrentPrice, InteractiveObject).Open();
            return base.StartExecute(character);
        }
    }

    /// <summary>"Vendre" : the owner sets or cancels a sale price.</summary>
    [Discriminator("HouseSell", typeof(Skill), typeof(int), typeof(InteractiveCustomSkillRecord), typeof(InteractiveObject))]
    public class SkillHouseSell : SkillHouseBase
    {
        public SkillHouseSell(int id, InteractiveCustomSkillRecord record, InteractiveObject interactiveObject)
            : base(id, record, interactiveObject)
        {
        }

        protected override bool IsAvailable(House house, Character character) => house.IsOwner(character);

        public override int StartExecute(Character character)
        {
            var house = House;
            if (house == null || character.IsBusy() || !house.IsOwner(character))
                return -1;

            new HouseBuySell(character, house, true, (ulong)house.Record.Price, InteractiveObject).Open();
            return base.StartExecute(character);
        }
    }

    /// <summary>"Poser le verrou" (owner).</summary>
    [Discriminator("HouseLock", typeof(Skill), typeof(int), typeof(InteractiveCustomSkillRecord), typeof(InteractiveObject))]
    public class SkillHouseLock : SkillHouseBase
    {
        public SkillHouseLock(int id, InteractiveCustomSkillRecord record, InteractiveObject interactiveObject)
            : base(id, record, interactiveObject)
        {
        }

        protected override bool IsAvailable(House house, Character character) => house.IsOwner(character) && !house.Locked;

        public override int StartExecute(Character character)
        {
            var house = House;
            if (house == null || !IsAvailable(house, character))
                return -1;

            house.Locked = true;
            HouseHandler.SendHouseProperties(InteractiveObject.Map, house);
            character.SendServerMessage("Votre maison est maintenant verrouillée.");
            return base.StartExecute(character);
        }
    }

    /// <summary>"Retirer le verrou" (owner).</summary>
    [Discriminator("HouseUnlock", typeof(Skill), typeof(int), typeof(InteractiveCustomSkillRecord), typeof(InteractiveObject))]
    public class SkillHouseUnlock : SkillHouseBase
    {
        public SkillHouseUnlock(int id, InteractiveCustomSkillRecord record, InteractiveObject interactiveObject)
            : base(id, record, interactiveObject)
        {
        }

        protected override bool IsAvailable(House house, Character character) => house.IsOwner(character) && house.Locked;

        public override int StartExecute(Character character)
        {
            var house = House;
            if (house == null || !IsAvailable(house, character))
                return -1;

            house.Locked = false;
            HouseHandler.SendHouseProperties(InteractiveObject.Map, house);
            character.SendServerMessage("Votre maison n'est plus verrouillée.");
            return base.StartExecute(character);
        }
    }
}
