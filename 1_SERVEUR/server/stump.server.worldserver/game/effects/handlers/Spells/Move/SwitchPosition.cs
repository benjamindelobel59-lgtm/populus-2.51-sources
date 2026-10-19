using System.Linq;
using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Database.World;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Effects.Instances;
using Stump.Server.WorldServer.Game.Spells.Casts;
using Stump.Server.WorldServer.Game.Fights.Buffs;

namespace Stump.Server.WorldServer.Game.Effects.Handlers.Spells.Move
{
    [EffectHandler(EffectsEnum.Effect_SwitchPosition)]
    [EffectHandler(EffectsEnum.Effect_SwitchPosition_1023)]
    public class SwitchPosition : SpellEffectHandler
    {
        public SwitchPosition(EffectDice effect, FightActor caster, SpellCastHandler castHandler, Cell targetedCell, bool critical) : base(effect, caster, castHandler, targetedCell, critical)
        {
        }

        protected override bool InternalApply()
        {
            var target = GetAffectedActors().FirstOrDefault();

            if (target == null)
                return false;

            // MYSTIC 18/10 : les Wobots Tamponneurs sont indéplaçables, sauf pour la Substitution du Wa Wobot (3824)
            if (!target.CanSwitchPos() && Spell.Id != 3824)
                return false;

            if (target.IsCarrying())
                return false;

            Caster.ExchangePositions(target);

            //Caster.TriggerBuffs(Caster, BuffTriggerType.OnMoved);
            target.TriggerBuffs(Caster, BuffTriggerType.OnMoved);
            // MYSTIC 18/10 : déclencheur « MS » (échangé de place) jamais émis : Louwdeuw des Wobots (Terrier du Wa Wabbit)
            target.TriggerBuffs(Caster, BuffTriggerType.Unknown_6);

            return true;
        }
    }
}