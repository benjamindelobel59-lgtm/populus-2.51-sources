using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Dialogs.Alliances;
using Stump.Server.WorldServer.Game.Effects.Instances;
using Stump.Server.WorldServer.Game.Items.Player;

namespace Stump.Server.WorldServer.Game.Effects.Handlers.Usables
{
    /// <summary>MYSTIC 17/10 : Potion de renommage d'alliance (14291, effet 1146) : nom et tag. La potion est retirée à la validation.</summary>
    [EffectHandler(EffectsEnum.Effect_1146)]
    public class AlliancePotionName : UsableEffectHandler
    {
        public AlliancePotionName(EffectBase effect, Character target, BasePlayerItem item)
            : base(effect, target, item)
        {
        }

        protected override bool InternalApply()
        {
            var alliance = Target.Guild?.Alliance;
            if (alliance == null || !alliance.IsLeader(Target))
            {
                Target.SendServerMessage("Seul le chef de l'alliance peut utiliser cette potion.");
                return false;
            }

            new AllianceModificationPanel(Target) { ChangeName = true, ChangeEmblem = false }.Open();
            UsedItems = 0;
            return true;
        }
    }

    /// <summary>MYSTIC 17/10 : Potion de changement de blason d'alliance (14292, effet 1145).</summary>
    [EffectHandler(EffectsEnum.Effect_1145)]
    public class AlliancePotionEmblem : UsableEffectHandler
    {
        public AlliancePotionEmblem(EffectBase effect, Character target, BasePlayerItem item)
            : base(effect, target, item)
        {
        }

        protected override bool InternalApply()
        {
            var alliance = Target.Guild?.Alliance;
            if (alliance == null || !alliance.IsLeader(Target))
            {
                Target.SendServerMessage("Seul le chef de l'alliance peut utiliser cette potion.");
                return false;
            }

            new AllianceModificationPanel(Target) { ChangeName = false, ChangeEmblem = true }.Open();
            UsedItems = 0;
            return true;
        }
    }
}
