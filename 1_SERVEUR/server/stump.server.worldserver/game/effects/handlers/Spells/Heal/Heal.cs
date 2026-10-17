using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Database.World;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Effects.Instances;
using Stump.Server.WorldServer.Game.Fights.Buffs;
using Spell = Stump.Server.WorldServer.Game.Spells.Spell;

using Stump.Server.WorldServer.Game.Spells.Casts;
namespace Stump.Server.WorldServer.Game.Effects.Handlers.Spells.Heal
{
    [EffectHandler(EffectsEnum.Effect_HealHP_108)]
    [EffectHandler(EffectsEnum.Effect_HealHP_143)]
    [EffectHandler(EffectsEnum.Effect_HealHP_81)]
    [EffectHandler(EffectsEnum.Effect_407)] // MYSTIC 17/10 : « #1 à #2 (PV rendus) » (Phossile, Ombre, Baleine…)
    public class Heal : SpellEffectHandler
    {
        public Heal(EffectDice effect, FightActor caster, SpellCastHandler castHandler, Cell targetedCell, bool critical)
            : base(effect, caster, castHandler, targetedCell, critical)
        {
        }

        protected override bool InternalApply()
        {
            if (IsCastByPortal)
            {
                double coef = (double)(((double)(2 * CastHandler.m_CastDistance) + 25d) / 100);
                Efficiency += coef;
            }
            foreach (var actor in GetAffectedActors())
            {
                var integerEffect = GenerateEffect();

                if (integerEffect == null)
                    return false;

                if (Effect.Duration != 0 || Effect.Delay != 0)
                {
                    // MYSTIC 17/10 : un soin avec ses propres declencheurs (D, APA, MPA, DE|DN...) part a chaque declenchement ;
                    // avant, il etait force en « Instant » et partait une seule fois au lancement (Ougah, Rat Noir : audit des donjons)
                    if (!string.IsNullOrEmpty(Effect.Triggers) && Effect.Triggers != "I" && Spell.Id != (int)SpellIdEnum.SPORK_2687)
                    {
                        AddTriggerBuff(actor, HealBuffTrigger);
                        continue;
                    }

                    var triggerType = BuffTriggerType.Instant;

                    switch (Spell.Id)
                    {
                        case (int)SpellIdEnum.SPORK_2687:
                            triggerType = BuffTriggerType.OnTackle;
                            break;
                    }

                    AddTriggerBuff(actor, triggerType, HealBuffTrigger);
                }
                else
                {
                    if (actor.IsAlive())
                    {
                        var damage = new Fights.Damage(Dice, EffectSchoolEnum.Healing, Caster, Spell, TargetedCell, EffectZone)
                        {
                            MarkTrigger = MarkTrigger,
                            IsCritical = Critical
                        };
                        damage.GenerateDamages();
                        damage.Amount = (short)(damage.Amount * Efficiency);
                        actor.Heal(damage);
                    }
                }
            }

            return true;
        }

        private static void HealBuffTrigger(TriggerBuff buff, FightActor triggerrer, BuffTriggerType trigger, object token)
        {
            var integerEffect = buff.GenerateEffect();

            if (integerEffect == null)
                return;

            if (buff.Target.IsAlive())
                buff.Target.Heal(integerEffect.Value, buff.Caster);
        }
    }
}