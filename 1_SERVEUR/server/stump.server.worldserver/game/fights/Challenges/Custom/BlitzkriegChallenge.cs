using System.Linq;
using Stump.DofusProtocol.Enums.Custom;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Fights.Teams;

namespace Stump.Server.WorldServer.Game.Fights.Challenges.Custom
{
    [ChallengeIdentifier((int)ChallengeEnum.BLITZKRIEG)]
    [ChallengeIdentifier((int)ChallengeEnum.BOUFTOU_ROYAL__CHALLENGE_2_)]
    [ChallengeIdentifier((int)ChallengeEnum.CORAILLEUR_MAGISTRAL__CHALLENGE_2_)]
    [ChallengeIdentifier((int)ChallengeEnum.WA_WABBIT__CHALLENGE_2_)]
    [ChallengeIdentifier((int)ChallengeEnum.WA_WOBOT__CHALLENGE_2_)]
    [ChallengeIdentifier((int)ChallengeEnum.MALLÉFISK__CHALLENGE_2_)]
    [ChallengeIdentifier((int)ChallengeEnum.RAT_NOIR__CHALLENGE_2_)]
    [ChallengeIdentifier((int)ChallengeEnum.POUNICHEUR__CHALLENGE_2_)]
    [ChallengeIdentifier((int)ChallengeEnum.CROCABULIA__CHALLENGE_2_)]
    [ChallengeIdentifier((int)ChallengeEnum.SKEUNK__CHALLENGE_2_)]
    [ChallengeIdentifier((int)ChallengeEnum.FRAKTALE__CHALLENGE_2_)]
    [ChallengeIdentifier((int)ChallengeEnum.EL_PIKO__CHALLENGE_1_)]
    [ChallengeIdentifier((int)ChallengeEnum.KRALAMOUR_GÉANT__CHALLENGE_2_)]
    [ChallengeIdentifier((int)ChallengeEnum.BWORKER__CHALLENGE_2_)]
    [ChallengeIdentifier((int)ChallengeEnum.GROLLOUM__CHALLENGE_2_)]
    [ChallengeIdentifier((int)ChallengeEnum.CAPITAINE_MENO__CHALLENGE_1_)]
    public class BlitzkriegChallenge : DefaultChallenge
    {
        public BlitzkriegChallenge(int id, IFight fight)
            : base(id, fight)
        {
            BonusMin = 80;
            BonusMax = 125;
        }

        // MYSTIC 17/10 : tous les ennemis touchés sont suivis (avant : seul le dernier touché, la cible « passait » de l'un à l'autre) ;
        // chacun doit mourir avant le début de son tour. Les invocations ennemies arrivées en cours de combat sont suivies aussi.
        readonly System.Collections.Generic.HashSet<FightActor> m_touches = new System.Collections.Generic.HashSet<FightActor>();
        readonly System.Collections.Generic.List<FightActor> m_suivis = new System.Collections.Generic.List<FightActor>();
        FightTeam m_equipeEnnemie;

        public override void Initialize()
        {
            base.Initialize();

            foreach (var fighter in Fight.GetAllFighters<MonsterFighter>())
                Suivre(fighter);

            m_equipeEnnemie = Fight.GetAllFighters<MonsterFighter>().FirstOrDefault()?.Team;
            if (m_equipeEnnemie != null)
                m_equipeEnnemie.FighterAdded += OnEnnemiAjoute;

            Fight.TurnStarted += OnTurnStarted;
        }

        void Suivre(FightActor fighter)
        {
            if (m_suivis.Contains(fighter))
                return;

            m_suivis.Add(fighter);
            fighter.DamageInflicted += OnDamageInflicted;
            fighter.Dead += OnEnnemiMort;
        }

        void OnEnnemiAjoute(FightTeam team, FightActor fighter) => Suivre(fighter);

        void OnEnnemiMort(FightActor fighter, FightActor killer) => m_touches.Remove(fighter);

        public override bool IsEligible() => Fight.GetAllFighters<MonsterFighter>().Count() > 1;

        // PADGREF 23/10 (retour utilisateur : « Blitzkrieg échoue quand je tape le Kralamoure à 900 % de résistances, il prend 0 ») :
        // un ennemi n'est « attaqué » que s'il perd vraiment des PV (avant : dès qu'un coup partait sur lui, même à 0).
        void OnDamageInflicted(FightActor fighter, Damage damage)
        {
            if (damage == null || damage.Source == null || fighter.IsFriendlyWith(damage.Source) || damage.Amount <= 0)
                return;

            m_touches.Add(fighter);
            Target = fighter;
        }

        void OnTurnStarted(IFight fight, FightActor fighter)
        {
            if (m_touches.Contains(fighter) && fighter.IsAlive())
                UpdateStatus(ChallengeStatusEnum.FAILED);
        }

        protected override void OnWinnersDetermined(IFight fight, FightTeam winners, FightTeam losers, bool draw)
        {
            base.OnWinnersDetermined(fight, winners, losers, draw);

            foreach (var fighter in m_suivis)
            {
                fighter.DamageInflicted -= OnDamageInflicted;
                fighter.Dead -= OnEnnemiMort;
            }

            if (m_equipeEnnemie != null)
                m_equipeEnnemie.FighterAdded -= OnEnnemiAjoute;

            Fight.TurnStarted -= OnTurnStarted;
        }
    }
}
