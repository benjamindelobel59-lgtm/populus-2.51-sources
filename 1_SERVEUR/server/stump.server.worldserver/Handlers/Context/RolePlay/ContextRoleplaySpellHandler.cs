using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Messages;
using Stump.DofusProtocol.Types;
using Stump.Server.BaseServer.Network;
using Stump.Server.WorldServer.Core.Network;
using Stump.Server.WorldServer.Database.Shortcuts;
using Stump.Server.WorldServer.Game.Spells;
using System.Linq;

namespace Stump.Server.WorldServer.Handlers.Context.RolePlay
{
    public partial class ContextRoleplayHandler : WorldHandlerContainer
    {
        //[WorldHandler(SpellModifyRequestMessage.Id)]
        //public static void HandleSpellModifyRequestMessage(WorldClient client, SpellModifyRequestMessage message)
        //{
        //    client.Character.Spells.BoostSpell(message.spellId, (ushort)message.spellLevel);
        //    client.Character.RefreshStats();
        //}

        //public static void SendSpellModifySuccessMessage(IPacketReceiver client, Spell spell)
        //{
        //    client.Send(new SpellModifySuccessMessage(spell.Id, (sbyte)spell.CurrentLevel));
        //}

        //public static void SendSpellModifySuccessMessage(IPacketReceiver client, int spellId, sbyte level)
        //{
        //    client.Send(new SpellModifySuccessMessage(spellId, level));
        //}

        //public static void SendSpellModifyFailureMessage(IPacketReceiver client)
        //{
        //    client.Send(new SpellModifyFailureMessage());
        //}

        //public static void SendSpellItemBoostMessage(IPacketReceiver client, int statId, short spellId, short value)
        //{
        //    client.Send(new SpellItemBoostMessage(statId, spellId, value));
        //}

        [WorldHandler(SpellVariantActivationRequestMessage.Id)]
        public static void HandleSpellVariantActivationMessage(WorldClient client, SpellVariantActivationRequestMessage message)
        {
            var spellVariants = SpellManager.Instance.GetSpellVariants();

            // MYSTIC 18/10 : variante changeable aussi pendant le PLACEMENT d'un combat (demande utilisateur, comme en officiel) ; refus
            // explicite sinon (avant : demande ignorée sans réponse). La position 63 (sort actif) change même si aucun des deux sorts
            // n'est dans la barre de raccourcis, et plus de plantage si le personnage n'a pas l'un des deux sorts.
            var personnage = client.Character;
            var autorise = !personnage.IsFighting() || personnage.Fight.State == Game.Fights.FightState.Placement;
            var variante = spellVariants.FirstOrDefault(v => v.SpellIds[0] == message.SpellId || v.SpellIds[1] == message.SpellId);
            var nouveau = personnage.Spells.FirstOrDefault(x => x.Template.Id == message.SpellId);
            if (!autorise || variante == null || nouveau == null)
            {
                client.Send(new SpellVariantActivationMessage(message.SpellId, false));
                return;
            }

            var ancienId = variante.SpellIds[0] == message.SpellId ? variante.SpellIds[1] : variante.SpellIds[0];
            var ancien = personnage.Spells.FirstOrDefault(x => x.Template.Id == ancienId);
            if (ancien != null)
                ancien.Record.Position = 0;
            nouveau.Record.Position = 63;

            foreach (SpellShortcut sh in personnage.Shortcuts.GetShortcuts(ShortcutBarEnum.SPELL_SHORTCUT_BAR).ToArray())
            {
                if (sh.SpellId == ancienId)
                {
                    client.Send(new ShortcutBarReplacedMessage((sbyte)ShortcutBarEnum.SPELL_SHORTCUT_BAR, new ShortcutSpell((sbyte)sh.Slot, message.SpellId)));
                    personnage.Shortcuts.SwapSpellShortcuts(sh.SpellId, (short)message.SpellId);
                    break;
                }
            }
            personnage.Shortcuts.Save();
            client.Send(new SpellVariantActivationMessage(message.SpellId, true));

        }
    }
}