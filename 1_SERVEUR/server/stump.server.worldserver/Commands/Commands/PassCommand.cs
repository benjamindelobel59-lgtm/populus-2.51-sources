using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Commands.Commands.Patterns;
using Stump.Server.WorldServer.Commands.Trigger;

namespace Stump.Server.WorldServer.Commands.Commands
{
    /// <summary>
    /// Passe-tour automatique (bouton du panneau One Window). Le tour est passé dans Fight.OnTurnStarted.
    /// Le texte « Passe-tour auto activé / désactivé » est lu par le panneau du client (bouton vert / gris).
    /// </summary>
    public class PassCommand : InGameCommand
    {
        public PassCommand()
        {
            Aliases = new[] { "pass", "passturn" };
            Description = "Passe automatiquement les tours de ce personnage.";
            RequiredRole = RoleEnum.Player;
        }

        public override void Execute(GameTrigger trigger)
        {
            var character = trigger.Character;
            character.ForcePassTurn = !character.ForcePassTurn;

            if (!character.ForcePassTurn)
            {
                character.SendServerMessage("Passe-tour auto désactivé : vous jouez de nouveau les tours de " + character.Name + ".");
                return;
            }

            character.SendServerMessage("Passe-tour auto activé : les tours de " + character.Name + " seront passés automatiquement.");

            // activé pendant son propre tour : on le passe tout de suite
            if (character.IsFighting() && character.Fighter != null && character.Fight.FighterPlaying == character.Fighter)
                character.Fighter.PassTurn();
        }
    }
}
