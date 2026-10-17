using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Commands.Commands.Patterns;
using Stump.Server.WorldServer.Commands.Trigger;
using Stump.Server.WorldServer.Game.Admin;

namespace Stump.Server.WorldServer.Commands.Commands
{
    /// <summary>
    /// .admin : envoie dans le chat un lien vers le panel admin web, deja connecte (jeton a usage unique).
    /// </summary>
    public class AdminPanelCommand : InGameCommand
    {
        public AdminPanelCommand()
        {
            Aliases = new[] { "admin", "panel" };
            Description = "Ouvre le panel admin (PNJ, dialogues, placements...) dans le navigateur.";
            RequiredRole = RoleEnum.Administrator;
        }

        public override void Execute(GameTrigger trigger)
        {
            var token = AdminPanelBridge.Instance.CreateToken(trigger.Character);
            var url = AdminPanelBridge.PanelUrl + "?t=" + token;

            trigger.Character.SendServerMessage("Panel admin : {url," + url + "::Clique ici pour ouvrir le panel}" +
                " (lien valable " + AdminPanelBridge.TokenLifetimeMinutes + " min, une seule fois)");
        }
    }
}
