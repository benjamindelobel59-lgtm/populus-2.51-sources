using Stump.Server.BaseServer.Database;
using Stump.Server.WorldServer.Database.World.Triggers;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Houses;

namespace Stump.Server.WorldServer.Game.Maps.Cells.Triggers
{
    /// <summary>
    /// Exit cell of a house interior. Interiors are shared by several houses, so the player is sent back
    /// in front of the door he came in from, not to a fixed map.
    /// </summary>
    [Discriminator("HouseExit", typeof(CellTrigger), typeof(CellTriggerRecord))]
    public class HouseExitTrigger : CellTrigger
    {
        public HouseExitTrigger(CellTriggerRecord record)
            : base(record)
        {
        }

        public override void Apply(Character character)
        {
            HouseManager.Instance.LeaveHouse(character);
        }
    }
}
