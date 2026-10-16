using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Commands.Commands.Patterns;
using Stump.Server.WorldServer.Commands.Trigger;

namespace Stump.Server.WorldServer.Commands.Commands
{
    /// <summary>
    /// MYSTIC : contrôle des invocations (activé par défaut). .invo on / .invo off / .invo (bascule) ou bouton de la barre One Window.
    /// Le texte « Contrôle invocations activé / désactivé » est lu par la barre du client (bouton vert / gris).
    /// S'applique aux invocations lancées ensuite (celles déjà en combat gardent leur mode).
    /// </summary>
    public class InvoCommand : InGameCommand
    {
        public InvoCommand()
        {
            Aliases = new[] { "invo" };
            Description = "Contrôle de vos invocations : .invo on / .invo off (activé par défaut).";
            RequiredRole = RoleEnum.Player;
        }

        public override void Execute(GameTrigger trigger)
        {
            var character = trigger.Character;
            var arg = (trigger.Args.NextWord() ?? "").Trim().ToLowerInvariant();

            if (arg == "on")
                character.ControleInvocations = true;
            else if (arg == "off")
                character.ControleInvocations = false;
            else
                character.ControleInvocations = !character.ControleInvocations;

            character.SendServerMessage(character.ControleInvocations
                ? "Contrôle invocations activé : vous jouez vous-même vos prochaines invocations."
                : "Contrôle invocations désactivé : vos prochaines invocations jouent seules.");
        }
    }
}
