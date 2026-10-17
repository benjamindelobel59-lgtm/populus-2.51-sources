using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Commands.Commands.Patterns;
using Stump.Server.WorldServer.Commands.Trigger;
using Stump.Server.WorldServer.Game.Bugs;

namespace Stump.Server.WorldServer.Commands.Commands
{
    /// <summary>
    /// MYSTIC 17/10 : .bug (bouton « Signaler un bug » de la barre) : envoie le lien du formulaire de signalement ; le client
    /// l'ouvre tout seul dans une fenêtre du jeu (PopulusButtons, comme le panel admin).
    /// </summary>
    public class BugCommand : InGameCommand
    {
        public BugCommand()
        {
            Aliases = new[] { "bug" };
            Description = "Signaler un bug (titre, description, logs du combat et du client) à l'équipe.";
            RequiredRole = RoleEnum.Player;
        }

        public override void Execute(GameTrigger trigger)
        {
            var lien = SignalementsBugs.Instance.CreerLien(trigger.Character, out var attente);
            if (lien == null)
            {
                trigger.Character.SendServerMessage("Signaler un bug : attendez encore " + attente + " s avant un nouveau signalement.");
                return;
            }

            trigger.Character.SendServerMessage("Signaler un bug : {url," + lien + "::Clique ici si la fenêtre ne s'est pas ouverte}" +
                " (lien valable " + SignalementsBugs.DureeLienMinutes + " min)");
        }
    }
}
