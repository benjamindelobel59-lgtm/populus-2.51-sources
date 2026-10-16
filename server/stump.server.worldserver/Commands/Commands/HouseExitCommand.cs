using Stump.DofusProtocol.Enums;
using Stump.Server.BaseServer.Commands;
using Stump.Server.WorldServer.Commands.Trigger;
using Stump.Server.WorldServer.Game.Houses;

namespace Stump.Server.WorldServer.Commands.Commands
{
    /// <summary>
    /// Leaves the house the player is in and goes back in front of its door.
    /// </summary>
    public class HouseExitCommand : CommandBase
    {
        public HouseExitCommand()
        {
            Aliases = new[] { "sortirmaison", "quittermaison" };
            Description = "Sortir de la maison et revenir devant la porte";
            RequiredRole = RoleEnum.Player;
        }

        public override void Execute(TriggerBase trigger)
        {
            var character = (trigger as GameTrigger)?.Character;
            if (character == null)
                return;

            if (character.IsFighting())
            {
                trigger.ReplyError("Impossible en combat.");
                return;
            }

            if (!HouseManager.Instance.LeaveHouse(character))
                trigger.ReplyError("Vous n'êtes pas dans une maison.");
        }
    }
}
