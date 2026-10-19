using Stump.Server.WorldServer.Game.Actors.Fight;

namespace Stump.Server.WorldServer.Game.Effects.Handlers.Spells.Targets
{
    public class LifeCriterion : TargetCriterion
    {
        public LifeCriterion(int lifePercent, bool mustBeGreater)
        {
            LifePercent = lifePercent;
            MustBeGreater = mustBeGreater;
        }

        public int LifePercent
        {
            get;
            set;
        }

        public bool MustBeGreater
        {
            get;
            set;
        }

        /// <summary>
        /// MYSTIC 18/10 : plusieurs critères de vie forment une TRANCHE (ex. « v75,V90 » = PV &gt; 75 % ET ≤ 90 %). Avant : combinés par
        /// OU (critère par défaut) → toujours vrai : la Comtoise du Comte Harebourg posait les 6 confusions à la fois.
        /// </summary>
        public override bool IsDisjonction => false;

        /// <summary>MYSTIC 18/10 : préfixe « * » : on teste les PV du lanceur, pas ceux de la cible.</summary>
        public bool SurLanceur
        {
            get;
            set;
        }

        public override bool IsTargetValid(FightActor actor, SpellEffectHandler handler)
        {
            var reference = SurLanceur ? handler.Caster : actor;
            return MustBeGreater ? reference.LifePoints / (double)reference.MaxLifePoints * 100d > LifePercent :
                reference.LifePoints / (double)reference.MaxLifePoints * 100d <= LifePercent;
        }
    }
}
