using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Commands.Commands.Patterns;
using Stump.Server.WorldServer.Commands.Trigger;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;

namespace Stump.Server.WorldServer.Commands.Commands
{
    class IpDropCommand : InGameCommand
    {
        public IpDropCommand()
        {
            base.Aliases = new[] { "ipdrop" };
            base.Description = "Le personnage ayant activé cette commande recevra tous le drop de tous vos personnages.";
            base.RequiredRole = RoleEnum.Player;
        }
        public override void Execute(GameTrigger trigger)
        {
            Character player = trigger.Character;
            if (!player.IsIpDrop)
            {
                foreach (var other in WorldServer.Instance.FindClients(x => x.IP == player.Client.IP && x.Character != null && x.Character != player && x.Character.IsIpDrop))
                {
                    other.Character.IsIpDrop = false;
                    other.Character.SendServerMessage("IP Drop désactivé : c'est maintenant " + player.Name + " qui reçoit le drop.");
                }
                player.IsIpDrop = true;
                player.SendServerMessage("IP Drop activé : " + player.Name + " reçoit le drop de tous vos personnages.");
            }
            else
            {
                player.IsIpDrop = false;
                player.SendServerMessage("IP Drop désactivé : chaque personnage garde son drop.");
            }
        }
    }
}
