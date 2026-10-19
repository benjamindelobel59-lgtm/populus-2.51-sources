using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Fights;

namespace Stump.Server.WorldServer.AI.Fights.Brain.Custom.Boss
{
    /// <summary>MYSTIC 17/10 : Fongeur : IA fuyarde (après ses sorts, il s'éloigne des ennemis).</summary>
    [BrainIdentifier((int)MonsterIdEnum.FONGEUR)]
    public class FongeurBrain : Brain
    {
        public FongeurBrain(AIFighter fighter)
            : base(fighter)
        {
        }

        public override bool Fuyard => true;
    }

    /// <summary>
    /// MYSTIC 17/10 : Tynril : icône de l'élément sans résistance au-dessus de la tête (états officiels « Élément … » 731 à 734 du client,
    /// icônes stateAir / stateWater / stateFire / stateEarth). Résistances en base : 0 % dans cet élément, 200 % dans les autres.
    /// </summary>
    public abstract class TynrilBrain : Brain
    {
        public const int EtatAir = 731;
        public const int EtatEau = 732;
        public const int EtatFeu = 733;
        public const int EtatTerre = 734;
        const int SortTynril = 1054;

        protected TynrilBrain(AIFighter fighter)
            : base(fighter)
        {
            if (fighter.Fight.State == FightState.Fighting)
                Poser();
            else
                fighter.Fight.FightStarted += OnFightStarted;
        }

        protected abstract int EtatElement { get; }

        void OnFightStarted(IFight fight)
        {
            fight.FightStarted -= OnFightStarted;
            using (fight.StartSequence(SequenceTypeEnum.SEQUENCE_TRIGGERED))
                Poser();
        }

        void Poser() => EtatsBoss.Ajouter(Fighter, Fighter, EtatElement, SortTynril);
    }

    [BrainIdentifier((int)MonsterIdEnum.TYNRIL_AHURI)]
    public class TynrilAhuriBrain : TynrilBrain
    {
        public TynrilAhuriBrain(AIFighter fighter) : base(fighter) { }
        protected override int EtatElement => EtatTerre;
    }

    [BrainIdentifier((int)MonsterIdEnum.TYNRIL_CONSTERNE)]
    public class TynrilConsterneBrain : TynrilBrain
    {
        public TynrilConsterneBrain(AIFighter fighter) : base(fighter) { }
        protected override int EtatElement => EtatAir;
    }

    [BrainIdentifier((int)MonsterIdEnum.TYNRIL_DECONCERTE)]
    public class TynrilDeconcerteBrain : TynrilBrain
    {
        public TynrilDeconcerteBrain(AIFighter fighter) : base(fighter) { }
        protected override int EtatElement => EtatEau;
    }

    [BrainIdentifier((int)MonsterIdEnum.TYNRIL_PERFIDE)]
    public class TynrilPerfideBrain : TynrilBrain
    {
        public TynrilPerfideBrain(AIFighter fighter) : base(fighter) { }
        protected override int EtatElement => EtatFeu;
    }
}
