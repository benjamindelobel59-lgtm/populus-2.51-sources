using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Commands.Commands.Patterns;
using Stump.Server.WorldServer.Commands.Trigger;

namespace Stump.Server.WorldServer.Commands.Commands
{
    /// <summary>
    /// Nekfeu : Sasa auto (bouton du panneau One Window) → les points de caractéristique disponibles vont dans la sagesse,
    /// tout de suite puis à chaque niveau gagné (hors combat, sinon à la sortie du combat). Voir Character.ApplyAutoWisdom.
    /// Le texte « Sasa auto activé / désactivé » est lu par le panneau du client (bouton vert / gris).
    /// </summary>
    public class SasaCommand : InGameCommand
    {
        public SasaCommand()
        {
            Aliases = new[] { "sasa" };
            Description = "Sasa auto : met les points de caractéristique disponibles dans la sagesse.";
            RequiredRole = RoleEnum.Player;
        }

        public override void Execute(GameTrigger trigger)
        {
            var character = trigger.Character;
            character.AutoWisdom = !character.AutoWisdom;

            if (!character.AutoWisdom)
            {
                character.SendServerMessage("Sasa auto désactivé : vous répartissez de nouveau vos points.");
                return;
            }

            var used = character.ApplyAutoWisdom();
            character.SendServerMessage("Sasa auto activé : les points de caractéristique de " + character.Name + " vont dans la sagesse" +
                                        (used > 0 ? " (" + used + " points utilisés)." : "."));
        }
    }
}
