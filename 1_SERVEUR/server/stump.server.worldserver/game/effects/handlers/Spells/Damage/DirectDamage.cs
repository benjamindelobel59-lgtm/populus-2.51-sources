using System;
using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Database.World;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Effects.Instances;
using Stump.Server.WorldServer.Game.Fights.Buffs;
using Stump.Server.WorldServer.Handlers.Actions;
using Spell = Stump.Server.WorldServer.Game.Spells.Spell;
using Stump.Server.WorldServer.Game.Spells.Casts;
using System.Collections.Generic;
using System.Linq;

namespace Stump.Server.WorldServer.Game.Effects.Handlers.Spells.Damage
{
    [EffectHandler(EffectsEnum.Effect_DamageWater)]
    [EffectHandler(EffectsEnum.Effect_DamageEarth)]
    [EffectHandler(EffectsEnum.Effect_DamageAir)]
    [EffectHandler(EffectsEnum.Effect_DamageFire)]
    [EffectHandler(EffectsEnum.Effect_DamageNeutral)]
    public class DirectDamage : SpellEffectHandler
    {
        const int GriffureOndulante = 6449;
        const int NovaBonbombe = 4689, EtatDesamorcee = 198;

        public DirectDamage(EffectDice effect, FightActor caster, SpellCastHandler castHandler, Cell targetedCell, bool critical)
            : base(effect, caster, castHandler, targetedCell, critical)
        {
            BuffTriggerType = BuffTriggerType.Unknown;
        }

        public BuffTriggerType BuffTriggerType
        {
            get;
            set;
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
                if (Effect.Duration != 0 || Effect.Delay != 0)
                {
                    // MYSTIC 20/10 (Plateau de Ush) : Griffure Ondulante (Chakaroze) : un seul poison infini par cible (relancer remplace
                    // l'ancien) ; avant, chaque lancer en ajoutait un et les dégâts de fin de tour s'accumulaient
                    if (Spell.Id == GriffureOndulante)
                        foreach (var ancien in actor.GetBuffs(b => b is TriggerBuff && b.Spell.Id == Spell.Id && b.Effect.EffectId == Effect.EffectId).ToArray())
                            actor.RemoveBuff(ancien);

                    if(Caster.IsPoisonSpellCast(Spell)) BuffTriggerType = BuffTriggerType.OnTurnBegin;
                    if (BuffTriggerType == BuffTriggerType.Unknown)
                        AddTriggerBuff(actor, DamageBuffTrigger);
                    else
                        AddTriggerBuff(actor, BuffTriggerType, DamageBuffTrigger);
                }
                else
                {
                    // spell reflected
                    var buff = actor.GetBestReflectionBuff();
                    if (buff != null && buff.ReflectedLevel >= Spell.CurrentLevel
                        && Spell.Template.Id != 0 && !Caster.IsIndirectSpellCast(Spell) && !Caster.IsPoisonSpellCast(Spell))
                    {
                        NotifySpellReflected(actor);

                        var damage = new Fights.Damage(Dice, GetEffectSchool(Dice.EffectId), Caster, Spell, Caster.Cell)
                        {
                            ReflectedDamages = true,
                            MarkTrigger = MarkTrigger,
                            IsCritical = Critical
                        };
                        damage.GenerateDamages();
                        damage.Amount = (short)(damage.Amount * Efficiency);

                        Caster.InflictDamage(damage);

                        if (buff.Duration <= 0)
                            actor.RemoveBuff(buff);
                    }
                    else
                    {                        
                        var damage = new Fights.Damage(Dice, GetEffectSchool(Dice.EffectId), Caster, Spell, TargetedCell, EffectZone)
                        {
                            MarkTrigger = MarkTrigger,
                            IsCritical = Critical
                        };
                        damage.GenerateDamages();
                        damage.Amount = (short)(damage.Amount * Efficiency);

                        // PADGREF 20/10 (demande utilisateur) : explosion d'une Bonbombe (Nova, sort 4689) = degats fixes : 1000 si la
                        // bombe est rouge, 1200 si elle est bleue (Desamorcee, etat 198). La Reine reste immunisee (masque f3726 du sort).
                        if (Spell.Id == NovaBonbombe)
                        {
                            damage.Amount = Caster.HasState(EtatDesamorcee) ? 1200 : 1000;
                            damage.IgnoreDamageReduction = true;
                            damage.IgnoreDamageBoost = true;   // sinon les bonus de dommages de la bombe s'ajoutaient (1200 -> 2100 en test)
                            damage.Zone = null;                // pas de baisse selon la distance dans la zone
                        }
                        actor.InflictDamage(damage);
                    }
                }
            }

            return true;
        }

        void DamageBuffTrigger(TriggerBuff buff, FightActor triggerrer, BuffTriggerType trigger, object token)
        {
            var damages = token as Fights.Damage;

            if (damages != null && (damages.Spell == null || damages.ReflectedDamages))
                return;

            var damage = new Fights.Damage(buff.Dice, GetEffectSchool(Dice.EffectId), buff.Caster, null, buff.Target.Cell)
            {
                IsCritical = buff.Critical,
                ReflectedDamages = true,
                // MYSTIC 20/10 : poison infini de Griffure Ondulante : 1 à 5 dégâts bruts (sans la puissance du Chakaroze), comme le site
                IgnoreDamageBoost = buff.Spell.Id == GriffureOndulante
            };

            damage.GenerateDamages();
            damage.Amount = (short)(damage.Amount * buff.Efficiency);

            buff.Target.InflictDamage(damage);
        }

        static EffectSchoolEnum GetEffectSchool(EffectsEnum effect)
        {
            switch (effect)
            {
                case EffectsEnum.Effect_DamageWater:
                    return EffectSchoolEnum.Water;
                case EffectsEnum.Effect_DamageEarth:
                    return EffectSchoolEnum.Earth;
                case EffectsEnum.Effect_DamageAir:
                    return EffectSchoolEnum.Air;
                case EffectsEnum.Effect_DamageFire:
                    return EffectSchoolEnum.Fire;
                case EffectsEnum.Effect_DamageNeutral:
                    return EffectSchoolEnum.Neutral;
                default:
                    throw new Exception(string.Format("Effect {0} has not associated School Type", effect));
            }
        }

        

        void NotifySpellReflected(FightActor source)
        {
            ActionsHandler.SendGameActionFightReflectSpellMessage(Fight.Clients, Caster, source);
        }
    }
}