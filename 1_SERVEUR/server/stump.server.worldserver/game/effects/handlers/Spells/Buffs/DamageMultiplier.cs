using System;
using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Database.World;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Effects.Instances;
using Stump.Server.WorldServer.Game.Fights.Buffs;
using Stump.Server.WorldServer.Game.Spells.Casts;

namespace Stump.Server.WorldServer.Game.Effects.Handlers.Spells.Buffs
{
    [EffectHandler(EffectsEnum.Effect_DamageMultiplier)]
    public class DamageMultiplier : SpellEffectHandler
    {
        public DamageMultiplier(EffectDice effect, FightActor caster, SpellCastHandler castHandler, Cell targetedCell, bool critical)
            : base(effect, caster, castHandler, targetedCell, critical)
        {
            DefaultDispellableStatus = FightDispellableEnum.DISPELLABLE_BY_DEATH;
            // MYSTIC 18/10 : Retour de flamme (Shin Larve, sort 13151) désenvoûtable comme sur le site
            if (castHandler?.Spell?.Id == 13151)
                DefaultDispellableStatus = FightDispellableEnum.DISPELLABLE;
        }

        protected override bool InternalApply()
        {
            foreach (var actor in GetAffectedActors())
            {
                AddTriggerBuff(actor, OnBuffTriggered);
            }

            return true;
        }

        const int EtatGouleAnerice = 534, SortResilienceVampyrique = 8727, SortGoulification = 8728;

        void OnBuffTriggered(TriggerBuff buff, FightActor triggerer, BuffTriggerType trigger, object token)
        {
            var damage = token as Fights.Damage;
            if (damage == null)
                return;

            if (damage.Spell != null && Caster.IsPoisonSpellCast(damage.Spell))
                return;

            // MYSTIC 19/10 : Anérice en Goule (état 534) : sa Résilience vampyrique (8727, x10 %) et le multiplicateur de la
            // Goulification (8728) ne s'appliquent plus ; les dégâts réels sont ceux de la prévisualisation du client (avant : 300 affichés, 60 subis)
            if (buff.Target.HasState(EtatGouleAnerice) && (buff.Spell.Id == SortResilienceVampyrique || buff.Spell.Id == SortGoulification))
                return;

            damage.Amount = (int)Math.Round(damage.Amount * (Dice.DiceNum / 100.0));
        }
    }
}
