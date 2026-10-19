using System.Collections.Generic;
using Stump.DofusProtocol.Messages;
using Stump.DofusProtocol.Types;
using Stump.Server.BaseServer.Network;
using Stump.Server.WorldServer.Core.Network;
using Stump.Server.WorldServer.Game;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Exchanges.Paddock;
using Stump.Server.WorldServer.Game.Interactives;
using Stump.Server.WorldServer.Game.Interactives.Skills;
using Stump.Server.WorldServer.Game.Maps.Paddocks;
using System.Linq;

namespace Stump.Server.WorldServer.Handlers.Interactives
{
    public class InteractiveHandler : WorldHandlerContainer
    {
        [WorldHandler(InteractiveUseRequestMessage.Id)]
        public static void HandleInteractiveUseRequestMessage(WorldClient client, InteractiveUseRequestMessage message)
        {
            // MYSTIC 17/10 : interactifs des villages de conquête réservés à l'alliance propriétaire
            if (!Game.Alliances.VillagesConquete.Instance.PeutUtiliserInteractif(client.Character, client.Character.Map, out var refus))
            {
                Game.Alliances.AvAManager.Message(client.Character, refus);
                return;
            }

            client.Character.Map.UseInteractiveObject(client.Character, (int)message.ElemId, (int)message.SkillInstanceUid);
        }

        /// <summary>
        /// Same as InteractiveUseRequestMessage with an extra parameter (house instance for house doors).
        /// Each house has a single instance here, so the parameter is not needed.
        /// </summary>
        [WorldHandler(InteractiveUseWithParamRequestMessage.Id)]
        public static void HandleInteractiveUseWithParamRequestMessage(WorldClient client, InteractiveUseWithParamRequestMessage message)
        {
            if (!Game.Alliances.VillagesConquete.Instance.PeutUtiliserInteractif(client.Character, client.Character.Map, out var refus))
            {
                Game.Alliances.AvAManager.Message(client.Character, refus);
                return;
            }

            client.Character.Map.UseInteractiveObject(client.Character, (int)message.ElemId, (int)message.SkillInstanceUid);
        }

        [WorldHandler(TeleportRequestMessage.Id)]
        public static void HandleTeleportRequestMessage(WorldClient client, TeleportRequestMessage message)
        {
            // MYSTIC 17/10 : PNJ de village de conquête (Brigandin / Foreuse)
            if (client.Character.Dialog is Game.Alliances.VillageTeleportDialog villageDialog)
            {
                var villageMap = World.Instance.GetMap((int)message.MapId);
                if (villageMap != null)
                    villageDialog.Teleport(villageMap);
                return;
            }

            // MYSTIC 17/10 : téléportation entre les prismes de l'alliance
            if (client.Character.Dialog is Game.Alliances.PrismTeleportDialog prismDialog)
            {
                var prismMap = World.Instance.GetMap((int)message.MapId);
                if (prismMap != null)
                    prismDialog.Teleport(prismMap);
                return;
            }

            if (client.Character.IsInZaapDialog())
            {
                var map = World.Instance.GetMap((int)message.MapId);

                if (map == null)
                    return;

                client.Character.ZaapDialog.Teleport(map);
            }
            else if (client.Character.IsInZaapiDialog())
            {
                var map = World.Instance.GetMap((int)message.MapId);

                if (map == null)
                    return;

                client.Character.ZaapiDialog.Teleport(map);
            }
            else if (client.Character.IsInDungsDialog())
            {
                var map = World.Instance.GetMap((int)message.MapId);

                if (map == null)
                    return;

                client.Character.DungsDialog.Teleport(map);
            }
        }

        [WorldHandler(ZaapRespawnSaveRequestMessage.Id)]
        public static void HandleZaapRespawnSaveRequestMessage(WorldClient client, ZaapRespawnSaveRequestMessage message)
        {
            if (client.Character.Map.Zaap == null)
                return;

            client.Character.SetSpawnPoint(client.Character.Map);
        }

        public static void SendZaapRespawnUpdatedMessage(WorldClient client)
        {
            client.Send(new ZaapRespawnUpdatedMessage(client.Character.Record.SpawnMapId ?? 0));
        }

        public static void SendInteractiveUsedMessage(IPacketReceiver client, Character user, InteractiveObject interactiveObject, Skill skill)
        {
            client.Send(new InteractiveUsedMessage((uint)user.Id, (uint)interactiveObject.Id, (ushort) skill.SkillTemplate.Id, (ushort) (skill.GetDuration(user, true)/100), true));
        }

        public static void SendInteractiveUseErrorMessage(IPacketReceiver client, int interactiveId, int skillId)
        {
            client.Send(new InteractiveUseErrorMessage((uint)interactiveId, (uint)skillId));
        }

        public static void SendStatedElementUpdatedMessage(IPacketReceiver client, int elementId, short elementCellId, int state)
        {
            client.Send(new StatedElementUpdatedMessage(new StatedElement(elementId, (ushort)elementCellId, (uint)state, true)));
        }

        public static void SendMapObstacleUpdatedMessage(IPacketReceiver client, IEnumerable<MapObstacle> obstacles)
        {
            client.Send(new MapObstacleUpdateMessage(obstacles.ToArray()));
        }

        public static void SendInteractiveElementUpdatedMessage(IPacketReceiver client, Character character, InteractiveObject interactive)
        {
            client.Send(new InteractiveElementUpdatedMessage(interactive.GetInteractiveElement(character)));
        }

        public static void SendInteractiveUseEndedMessage(IPacketReceiver client, InteractiveObject interactive, Skill skill)
        {
            client.Send(new InteractiveUseEndedMessage((uint)interactive.Id, (ushort)skill.SkillTemplate.Id));
        }
    }
}