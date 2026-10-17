using System;
using System.Linq;
using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Database.World;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Effects.Instances;
using Stump.Server.WorldServer.Game.Spells.Casts;

namespace Stump.Server.WorldServer.Game.Effects.Handlers.Spells.Damage
{
    /// <summary>
    /// MYSTIC : « #1 à #2 % des PV de la cible (dommages Terre / Neutre) » (effets 1070 / 1071, ex. Glouragan : ses coups
    /// en zone touchent aussi ses alliés). Dommages fixes = pourcentage tiré entre DiceNum et DiceFace des PV actuels de chaque cible.
    /// </summary>
    [EffectHandler(EffectsEnum.Effect_1067)]
    [EffectHandler(EffectsEnum.Effect_1068)]
    [EffectHandler(EffectsEnum.Effect_1069)]
    [EffectHandler(EffectsEnum.Effect_1070)]
    [EffectHandler(EffectsEnum.Effect_1071)]
    public class DamagePercentTargetLife : SpellEffectHandler
    {
        private static readonly Random m_random = new Random();

        public DamagePercentTargetLife(EffectDice effect, FightActor caster, SpellCastHandler castHandler, Cell targetedCell, bool critical)
            : base(effect, caster, castHandler, targetedCell, critical)
        {
        }

        protected override bool InternalApply()
        {
            // 1067 Air, 1068 Eau, 1069 Feu (MYSTIC 17/10 : Daigoro, Dramak, Haute Truche), 1070 Terre, 1071 Neutre
            var school = Dice.EffectId == EffectsEnum.Effect_1067 ? EffectSchoolEnum.Air :
                         Dice.EffectId == EffectsEnum.Effect_1068 ? EffectSchoolEnum.Water :
                         Dice.EffectId == EffectsEnum.Effect_1069 ? EffectSchoolEnum.Fire :
                         Dice.EffectId == EffectsEnum.Effect_1070 ? EffectSchoolEnum.Earth : EffectSchoolEnum.Neutral;
            var min = Math.Min((int)Dice.DiceNum, Math.Max((int)Dice.DiceNum, (int)Dice.DiceFace));
            var max = Math.Max((int)Dice.DiceNum, (int)Dice.DiceFace);

            foreach (var actor in GetAffectedActors().ToArray())
            {
                int percent;
                lock (m_random)
                    percent = m_random.Next(min, max + 1);

                var damage = new Fights.Damage(Dice, school, Caster, Spell, TargetedCell, EffectZone)
                {
                    IgnoreDamageBoost = true,
                    MarkTrigger = MarkTrigger,
                    IsCritical = Critical,
                };
                damage.GenerateDamages();
                damage.Amount = (int)Math.Floor(actor.LifePoints * percent / 100d);
                actor.InflictDamage(damage);
            }

            return true;
        }
    }
}
