using Stump.DofusProtocol.Messages;
using Stump.Server.WorldServer.Core.Network;
using Stump.Server.WorldServer.Game.Chasses;

namespace Stump.Server.WorldServer.Handlers.Context.RolePlay
{
    /// <summary>MYSTIC 19/10 : fenêtre de chasse au trésor du client (jalons, fouille, abandon, chasse légendaire).</summary>
    public class ChasseHandler : WorldHandlerContainer
    {
        [WorldHandler(TreasureHuntFlagRequestMessage.Id)]
        public static void HandleTreasureHuntFlagRequestMessage(WorldClient client, TreasureHuntFlagRequestMessage message)
            => ChasseManager.Instance.PoserJalon(client.Character, message.Index);

        [WorldHandler(TreasureHuntFlagRemoveRequestMessage.Id)]
        public static void HandleTreasureHuntFlagRemoveRequestMessage(WorldClient client, TreasureHuntFlagRemoveRequestMessage message)
            => ChasseManager.Instance.RetirerJalon(client.Character, message.Index);

        [WorldHandler(TreasureHuntDigRequestMessage.Id)]
        public static void HandleTreasureHuntDigRequestMessage(WorldClient client, TreasureHuntDigRequestMessage message)
            => ChasseManager.Instance.Fouiller(client.Character);

        [WorldHandler(TreasureHuntGiveUpRequestMessage.Id)]
        public static void HandleTreasureHuntGiveUpRequestMessage(WorldClient client, TreasureHuntGiveUpRequestMessage message)
            => ChasseManager.Instance.Abandonner(client.Character);

        [WorldHandler(PortalUseRequestMessage.Id)]
        public static void HandlePortalUseRequestMessage(WorldClient client, PortalUseRequestMessage message)
            => PortailManager.Instance.Utiliser(client.Character, (int)message.PortalId);

        [WorldHandler(TreasureHuntLegendaryRequestMessage.Id)]
        public static void HandleTreasureHuntLegendaryRequestMessage(WorldClient client, TreasureHuntLegendaryRequestMessage message)
            => ChasseManager.Instance.DemarrerLegendaire(client.Character, message.LegendaryId);
    }
}
