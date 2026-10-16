using System.Collections.Generic;
using System.Linq;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Messages;
using Stump.Server.BaseServer.Network;
using Stump.Server.WorldServer.Core.Network;
using Shortcut = Stump.Server.WorldServer.Database.Shortcuts.Shortcut;

namespace Stump.Server.WorldServer.Handlers.Shortcuts
{
    public class ShortcutHandler : WorldHandlerContainer
    {
        [WorldHandler(ShortcutBarAddRequestMessage.Id)]
        public static void HandleShortcutBarAddRequestMessage(WorldClient client, ShortcutBarAddRequestMessage message)
        {
            var barType = (ShortcutBarEnum)message.BarType;
            var slave = GetOneWindowSlave(client, barType);
            (slave ?? client.Character).Shortcuts.AddShortcut(barType, message.Shortcut);
            if (slave != null)
                SendSlotsToMaster(client, slave, barType, message.Shortcut.Slot);
        }

        [WorldHandler(ShortcutBarRemoveRequestMessage.Id)]
        public static void HandleShortcutBarRemoveRequestMessage(WorldClient client, ShortcutBarRemoveRequestMessage message)
        {
            var barType = (ShortcutBarEnum)message.BarType;
            var slave = GetOneWindowSlave(client, barType);
            (slave ?? client.Character).Shortcuts.RemoveShortcut(barType, message.Slot);
            if (slave != null)
                SendSlotsToMaster(client, slave, barType, message.Slot);
        }

        [WorldHandler(ShortcutBarSwapRequestMessage.Id)]
        public static void HandleShortcutBarSwapRequestMessage(WorldClient client, ShortcutBarSwapRequestMessage message)
        {
            var barType = (ShortcutBarEnum)message.BarType;
            var slave = GetOneWindowSlave(client, barType);
            (slave ?? client.Character).Shortcuts.SwapShortcuts(barType, message.FirstSlot, message.SecondSlot);
            if (slave != null)
                SendSlotsToMaster(client, slave, barType, message.FirstSlot, message.SecondSlot);
        }

        /// <summary>
        /// NEKFEU (one window) : pendant le tour d'un esclave, le maitre voit et modifie la barre de sorts de l'esclave ;
        /// renvoie l'esclave dont c'est le tour et que ce client controle, sinon null (barre du personnage lui-meme).
        /// </summary>
        static Game.Actors.RolePlay.Characters.Character GetOneWindowSlave(WorldClient client, ShortcutBarEnum barType)
        {
            var master = client.Character?.Fighter;
            if (barType != ShortcutBarEnum.SPELL_SHORTCUT_BAR || master == null || master.Fight == null)
                return null;
            var playing = master.Fight.FighterPlaying as Game.Actors.Fight.CharacterFighter;
            if (playing == null || playing == master || playing.GetOneWindowMaster() != master)
                return null;
            return playing.Character;
        }

        // le client du maitre met a jour la barre affichee (celle de l'esclave) sans toucher a la sienne
        static void SendSlotsToMaster(WorldClient client, Game.Actors.RolePlay.Characters.Character slave, ShortcutBarEnum barType, params int[] slots)
        {
            foreach (var slot in slots.Distinct())
            {
                var shortcut = slave.Shortcuts.GetShortcut(barType, slot);
                if (shortcut != null)
                    SendShortcutBarRefreshMessage(client, barType, shortcut);
                else
                    SendShortcutBarRemovedMessage(client, barType, slot);
            }
        }

        public static void SendShortcutBarContentMessage(WorldClient client, ShortcutBarEnum barType)
        {
            client.Send(new ShortcutBarContentMessage((sbyte)barType,
                client.Character.Shortcuts.GetShortcuts(barType).Select(entry => entry.GetNetworkShortcut()).ToArray()));
        }

        public static void SendShortcutBarContentMessage(WorldClient client, IEnumerable<DofusProtocol.Types.Shortcut> shortcuts, ShortcutBarEnum barType)
        {
            client.Send(new ShortcutBarContentMessage((sbyte)barType, shortcuts.ToArray()));
        }

        public static void SendShortcutBarRefreshMessage(IPacketReceiver client, ShortcutBarEnum barType, Shortcut shortcut)
        {
            client.Send(new ShortcutBarRefreshMessage((sbyte)barType, shortcut.GetNetworkShortcut()));
        }

        public static void SendShortcutBarRemovedMessage(IPacketReceiver client, ShortcutBarEnum barType, int slot)
        {
            client.Send(new ShortcutBarRemovedMessage((sbyte)barType, (sbyte)slot));
        }

        public static void SendShortcutBarRemoveErrorMessage(IPacketReceiver client)
        {
            client.Send(new ShortcutBarRemoveErrorMessage());
        }

        public static void SendShortcutBarSwapErrorMessage(IPacketReceiver client)
        {
            client.Send(new ShortcutBarSwapErrorMessage());
        }

        public static void SendShortcutBarAddErrorMessage(IPacketReceiver client)
        {
            client.Send(new ShortcutBarAddErrorMessage());
        }
    }
}