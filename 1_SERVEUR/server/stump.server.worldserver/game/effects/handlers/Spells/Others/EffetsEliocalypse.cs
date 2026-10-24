using System.Linq;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Types;
using Stump.Server.WorldServer.Database.World;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Actors.Stats;
using Stump.Server.WorldServer.Game.Effects.Instances;
using Stump.Server.WorldServer.Game.Fights.Buffs;
using Stump.Server.WorldServer.Game.Spells;
using Stump.Server.WorldServer.Game.Spells.Casts;

namespace Stump.Server.WorldServer.Game.Effects.Handlers.Spells.Others
{
    /// <summary>
    /// PADGREF 23/10 (Eliocalypse, effets du client 2.74 absents de notre moteur) : effet 2872 « Seuil : #1 PV ».
    /// Pose sur la cible un seuil à diceSide % de ses PV max (Arsenal de Guerre : 80 / 60 / 40 / 20 % ; Esclavagisme de Servitude…).
    /// Quand des dégâts la feraient passer sous ce seuil, ses PV sont BLOQUÉS au seuil (le reste du coup est perdu : « PV bloqués au
    /// seuil », DPLN) et les déclencheurs « TR&lt;sort&gt; » (sort = celui qui contient le seuil) de ses buffs partent (FightActor).
    /// </summary>
    [EffectHandler((EffectsEnum)2872)]
    public class SeuilPV : SpellEffectHandler
    {
        public SeuilPV(EffectDice effect, FightActor caster, SpellCastHandler castHandler, Cell targetedCell, bool critical)
            : base(effect, caster, castHandler, targetedCell, critical)
        {
        }

        protected override bool InternalApply()
        {
            var pourcent = Dice.DiceFace > 0 ? Dice.DiceFace : Dice.DiceNum;
            foreach (var actor in GetAffectedActors())
            {
                // un seul seuil par sort et par combattant
                foreach (var ancien in actor.GetBuffs(b => b is SeuilBuff s && s.SortSeuil == Spell.Id).ToArray())
                    actor.RemoveBuff(ancien);
                actor.AddBuff(new SeuilBuff(actor.PopNextBuffId(), actor, Caster, this, Spell, pourcent, Spell.Id));
            }
            return true;
        }
    }

    public class SeuilBuff : Buff
    {
        public SeuilBuff(int id, FightActor target, FightActor caster, SpellEffectHandler effect, Spell spell, int pourcent, int sortSeuil)
            : base(id, target, caster, effect, spell, false, FightDispellableEnum.DISPELLABLE_BY_DEATH)
        {
            Pourcent = pourcent;
            SortSeuil = sortSeuil;
        }

        public int Pourcent { get; }
        public int SortSeuil { get; }

        /// <summary>PV du seuil (au moins 1 PV : le seuil « 0 % » de Servitude = 1 PV).</summary>
        public int PV => System.Math.Max(1, Target.MaxLifePoints * Pourcent / 100);

        public override AbstractFightDispellableEffect GetAbstractFightDispellableEffect() =>
            new FightTemporaryBoostEffect((uint)Id, Target.Id, Duration, (sbyte)Dispellable, (ushort)Spell.Id, (uint)(EffectFix?.ClientEffectId ?? EffetUidClient), 0,
                (short)Pourcent);
    }

    /// <summary>
    /// PADGREF 23/10 (Eliocalypse) : effet 1223 « Renvoie #1 % des dommages subis » (Servitude : 200 %). Tant que le buff dure, chaque
    /// dommage DIRECT reçu d'un ennemi est renvoyé à l'attaquant à #1 % (pas les poisons ni les renvois). Les états « Traître 1-5 »
    /// (1537-1541) de l'attaquant l'en protègent (le sort du jeu retire ensuite un niveau de Traître).
    /// </summary>
    [EffectHandler((EffectsEnum)1223)]
    public class RenvoiPourcent : SpellEffectHandler
    {
        public static readonly int[] EtatsTraitre = { 1537, 1538, 1539, 1540, 1541 };

        public RenvoiPourcent(EffectDice effect, FightActor caster, SpellCastHandler castHandler, Cell targetedCell, bool critical)
            : base(effect, caster, castHandler, targetedCell, critical)
        {
        }

        protected override bool InternalApply()
        {
            foreach (var actor in GetAffectedActors())
                AddTriggerBuff(actor, BuffTriggerType.AfterDamaged, Renvoyer);
            return true;
        }

        static void Renvoyer(TriggerBuff buff, FightActor declencheur, BuffTriggerType trigger, object token)
        {
            var degats = token as Fights.Damage;
            var cible = buff.Target;
            var attaquant = degats?.Source;
            if (degats == null || attaquant == null || attaquant == cible || !attaquant.IsAlive() || !cible.IsEnnemyWith(attaquant) ||
                degats.ReflectedDamages || degats.Buff != null || degats.Amount <= 0)
                return;
            if (EtatsTraitre.Any(attaquant.HasState))
                return;
            var pourcent = buff.Dice.DiceNum > 0 ? buff.Dice.DiceNum : buff.Dice.DiceFace;
            attaquant.InflictDamage(new Fights.Damage(degats.Amount * pourcent / 100)
            {
                Source = cible,
                School = degats.School,
                IgnoreDamageBoost = true,
                IgnoreDamageReduction = true,
                ReflectedDamages = true,
            });
        }
    }

    /// <summary>
    /// PADGREF 23/10 (Eliocalypse, Brutalyte) : effet 2822 « #1 à #2 (dommages Meilleur élément) » : dommages dans l'élément où le
    /// lanceur a la plus forte caractéristique (Force = Terre, Intelligence = Feu, Chance = Eau, Agilité = Air).
    /// </summary>
    [EffectHandler((EffectsEnum)2822)]
    public class DegatsMeilleurElement : SpellEffectHandler
    {
        public DegatsMeilleurElement(EffectDice effect, FightActor caster, SpellCastHandler castHandler, Cell targetedCell, bool critical)
            : base(effect, caster, castHandler, targetedCell, critical)
        {
        }

        protected override bool InternalApply()
        {
            var s = Caster.Stats;
            var choix = new[]
            {
                (s[PlayerFields.Strength].Total, EffectSchoolEnum.Earth), (s[PlayerFields.Intelligence].Total, EffectSchoolEnum.Fire),
                (s[PlayerFields.Chance].Total, EffectSchoolEnum.Water), (s[PlayerFields.Agility].Total, EffectSchoolEnum.Air)
            }.OrderByDescending(x => x.Item1).First().Item2;
            foreach (var actor in GetAffectedActors())
            {
                var d = new Fights.Damage(Dice, choix, Caster, Spell, TargetedCell, EffectZone) { IsCritical = Critical };
                d.GenerateDamages();
                actor.InflictDamage(d);
            }
            return true;
        }
    }

    /// <summary>PADGREF 23/10 : effet 2868 « Augmente la taille de #1 % » (Mégalérien de l'Ecaptif) : purement visuel, ignoré.</summary>
    [EffectHandler((EffectsEnum)2868)]
    public class TailleVisuelle : SpellEffectHandler
    {
        public TailleVisuelle(EffectDice effect, FightActor caster, SpellCastHandler castHandler, Cell targetedCell, bool critical)
            : base(effect, caster, castHandler, targetedCell, critical)
        {
        }

        protected override bool InternalApply() => true;
    }
}
