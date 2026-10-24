using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Types;
using Stump.Server.WorldServer.Database.Spells;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Effects.Handlers.Spells;
using Stump.Server.WorldServer.Game.Effects.Instances;
using Stump.Server.WorldServer.Game.Spells;

namespace Stump.Server.WorldServer.Game.Fights.Buffs
{
    public abstract class Buff
    {

        public const int CHARACTERISTIC_STATE = 71;

        protected Buff(int id, FightActor target, FightActor caster, SpellEffectHandler effectHandler, Spell spell, bool critical, FightDispellableEnum dispelable, FightActor triggerer = null)
        {
            Id = id;
            Target = target;
            Caster = caster;
            EffectHandler = effectHandler;
            Spell = spell;
            Critical = critical;

            // MYSTIC 18/10 : déclencheur d'un sous-sort connu avant la durée (Truchage de la Haute Truche : correctif de durée 1 tour)
            if (triggerer == null && effectHandler.CastHandler?.Informations.Triggerer != null)
                triggerer = effectHandler.CastHandler.Informations.Triggerer;

            Duration = triggerer != null && EffectFix?.TriggerBuffDuration != null ? (short)EffectFix.TriggerBuffDuration : (short)(EffectHandler.Duration == -1 ? -1000 : Effect.Duration);
            Dispellable = EffectFix?.Dispellable != null ? (FightDispellableEnum)EffectFix.Dispellable : dispelable;

            Delay = (short)EffectHandler.Delay;
            CustomActionId = (short?)EffectFix?.ActionId;

            Efficiency = 1.0d;

            if (triggerer == null && effectHandler.CastHandler?.Informations.Triggerer != null)
                triggerer = effectHandler.CastHandler.Informations.Triggerer;

            BuffTriggered = triggerer != null;
            DecrementReference = triggerer ?? Caster;

            // PADGREF 21/10 (Deuil 2675 de la Fuji Givrefoux, Mansomon 2609 du Mansot Royal… : bonus aux alliés lancé par l'allié qui
            // MEURT) : un effet lancé par un combattant déjà mort était retiré aussitôt (RemoveAllCastedBuffs à sa mort) et décompté sur
            // lui, qui ne joue plus. Il reste maintenant, et se décompte au tour du joueur en cours (sinon sur la cible, avec 1 tour de
            // plus) : les alliés profitent du PM pendant leur prochain tour. Vaut pour tous les passifs « à la mort ».
            if (caster != null && caster.IsDead())
            {
                LanceParUnMort = true;
                var joueur = caster.Fight?.FighterPlaying;
                if (joueur != null && joueur != caster && joueur.IsAlive())
                    DecrementReference = joueur;
                else
                {
                    DecrementReference = target;
                    if (Duration > 0)
                        Duration++;
                }
            }
        }

        /// <summary>PADGREF 21/10 : effet lancé par un combattant déjà mort (passif « à la mort ») : pas retiré avec les effets du mort.</summary>
        public bool LanceParUnMort
        {
            get;
        }

        public SpellEffectHandler EffectHandler
        {
            get;
        }

        public int Id
        {
            get;
        }

        public FightActor Target
        {
            get;
        }

        public FightActor Caster
        {
            get;
        }

        public FightActor DecrementReference
        {
            get;
            set;
        }

        public EffectDice Dice => EffectHandler.Dice;

        public EffectBase Effect => EffectHandler.Effect;

        public Spell Spell
        {
            get;
        }

        public short Duration
        {
            get;
            set;
        }

        public short Delay
        {
            get;
            set;
        }

        public int Priority => EffectFix?.Priority ?? (EffectHandler.Priority);

        public bool Critical
        {
            get;
        }

        public FightDispellableEnum Dispellable
        {
            get;
            set;
        }

        public short? CustomActionId
        {
            get;
            set;
        }

        /// <summary>
        /// Efficiency factor, increase or decrease effect's efficiency. Default is 1.0
        /// </summary>
        public double Efficiency
        {
            get;
            set;
        }

        public bool Applied
        {
            get;
            private set;
        }

        /// <summary>PADGREF 23/10 : état caché (Lourd du Morsquale, « non visible » en 2.51) : le buff agit mais n'est pas envoyé au client.</summary>
        public bool NonAffiche
        {
            get;
            set;
        }

        public bool BuffTriggered
        {
            get;
            private set;
        }

        public SpellEffectFix EffectFix => Effect.EffectFix;

        public virtual BuffType Type
        {
            get
            {
                if (Effect.Template.Characteristic == CHARACTERISTIC_STATE)
                    return BuffType.CATEGORY_STATE;

                if (Effect.Template.Operator == "-")
                    return Effect.Template.Active ? BuffType.CATEGORY_ACTIVE_MALUS : BuffType.CATEGORY_PASSIVE_MALUS;

                if (Effect.Template.Operator == "+")
                    return Effect.Template.Active ? BuffType.CATEGORY_ACTIVE_BONUS : BuffType.CATEGORY_PASSIVE_BONUS;

                return BuffType.CATEGORY_OTHER;
            }
        }

        /// <summary>
        /// Decrement Duration and return true whenever the buff is over
        /// </summary>
        /// <returns></returns>
        public bool DecrementDuration()
        {
            if (Delay > 0)
            {
                if (--Delay == 0)
                {
                    var fight = Caster.Fight;

                    using (fight.StartSequence(SequenceTypeEnum.SEQUENCE_TRIGGERED))
                    {
                        TriggerBuff buffTrigger = this as TriggerBuff;
                        if (buffTrigger != null && buffTrigger.ShouldTrigger(BuffTriggerType.Instant))
                            buffTrigger.Apply(Target, BuffTriggerType.Instant);
                        else
                            Apply();

                        fight.UpdateBuff(this, false);
                    }
                }

                return false;
            }

            if (Duration <= -1) // Duration = -1000 => unlimited buff
                return false;

            return --Duration <= 0;
        }

        public virtual void Apply() => Applied = true;
        public virtual void Dispell() => Applied = false;

        public virtual short GetActionId()
        {
            if (CustomActionId.HasValue)
                return CustomActionId.Value;

            return (short)Effect.EffectId;
        }

        public EffectInteger GenerateEffect()
        {
            var effect = Effect.GenerateEffect(EffectGenerationContext.Spell) as EffectInteger;

            if (effect != null)
                effect.Value = (short)(effect.Value * Efficiency);

            return effect;
        }

        public FightDispellableEffectExtendedInformations GetFightDispellableEffectExtendedInformations()
            => new FightDispellableEffectExtendedInformations((ushort)GetActionId(), Caster.Id, GetAbstractFightDispellableEffect());

        /// <summary>
        /// PADGREF 23/10 : le client 2.51 range ce champ dans « dataUid » et le compare à l'effectUid des effets du sort pour savoir si le
        /// buff s'affiche (visibleInBuffUi), son ordre, etc. On envoyait le numéro d'action (ex. 792) : jamais trouvé, tout s'affichait
        /// (« _PUT HUB PLAYER », « _IF HEAL OR SELF HEAL » des sorts techniques de l'Eliocalypse). Numéro unique de l'effet s'il existe.
        /// </summary>
        protected int EffetUidClient => Effect.Uid > 0 ? Effect.Uid : Effect.Id;

        public abstract AbstractFightDispellableEffect GetAbstractFightDispellableEffect();
    }
}