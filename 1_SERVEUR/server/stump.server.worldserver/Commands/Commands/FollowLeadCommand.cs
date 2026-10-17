using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Commands.Commands.Patterns;
using Stump.Server.WorldServer.Commands.Trigger;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;

namespace Stump.Server.WorldServer.Commands.Commands
{
    public class FollowLeadCommand : InGameCommand
    {
        public FollowLeadCommand()
        {
            Aliases = new[] { "join", "master" };
            Description = "Vos autres personnages suivent celui-ci de map en map et rejoignent ses combats.";
            RequiredRole = RoleEnum.Player;
        }

        public override void Execute(GameTrigger trigger)
        {
            var character = trigger.Character;

            if (!MultiLeader.IsLeader(character))
            {
                MultiLeader.Enable(character);
                character.SendServerMessage("Master activé : vos autres personnages vous suivent de map en map et rejoignent vos combats.");
            }
            else
            {
                MultiLeader.Disable(character);
                character.SendServerMessage("Master désactivé : vos personnages ne vous suivent plus.");
            }
        }
    }
}
