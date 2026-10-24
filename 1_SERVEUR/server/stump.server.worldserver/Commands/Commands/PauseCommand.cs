using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Commands.Commands.Patterns;
using Stump.Server.WorldServer.Commands.Trigger;

namespace Stump.Server.WorldServer.Commands.Commands
{
    /// <summary>
    /// PADGREF 23/10 : .pause (admins) : met en pause le combat dans lequel on est (ou qu'on regarde), puis le reprend au
    /// 2e appel. Détail dans Fight.BasculerPause.
    /// </summary>
    public class PauseCommand : InGameCommand
    {
        public PauseCommand()
        {
            Aliases = new[] { "pause" };
            Description = "Met en pause le combat en cours, puis le reprend (2e appel).";
            RequiredRole = RoleEnum.GameMaster;
        }

        public override void Execute(GameTrigger trigger)
        {
            var perso = trigger.Character;
            var combat = perso.Fight ?? perso.Spectator?.Fight;
            if (combat == null)
            {
                trigger.ReplyError("Pause : tu n'es dans aucun combat.");
                return;
            }

            if (!combat.BasculerPause(perso))
                trigger.ReplyError("Pause : possible seulement une fois le combat commencé (pas pendant le placement).");
        }
    }
}
