using System.Linq;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Enums.Custom;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Fights.Teams;

namespace Stump.Server.WorldServer.Game.Fights.Challenges.Custom
{
    [ChallengeIdentifier((int)ChallengeEnum.CIRCULEZ)]
    [ChallengeIdentifier((int)ChallengeEnum.MALLÉFISK__CHALLENGE_1_)]
    [ChallengeIdentifier((int)ChallengeEnum.BEN_LE_RIPATE__CHALLENGE_1_)]
    [ChallengeIdentifier((int)ChallengeEnum.MINOTOT__CHALLENGE_1_)]
    [ChallengeIdentifier((int)ChallengeEnum.TOXOLIATH__CHALLENGE_1_)]

    // Liberty criterion
    [ChallengeIdentifier((int)ChallengeEnum.CHÊNE_MOU__CHALLENGE_1_)]
    [ChallengeIdentifier((int)ChallengeEnum.PHOSSILE__CHALLENGE_2_)]
    [ChallengeIdentifier((int)ChallengeEnum.COMTE_RAZOF__CHALLENGE_1_)]
    [ChallengeIdentifier((int)ChallengeEnum.CHALOEIL__CHALLENGE_1_)]
    [ChallengeIdentifier((int)ChallengeEnum.ILYZAELLE__CHALLENGE_1_)]
    [ChallengeIdentifier((int)ChallengeEnum.DAZAK_MARTEGEL__CHALLENGE_1_)]
    public class KeepMovingChallenge : DefaultChallenge
    {
        public KeepMovingChallenge(int id, IFight fight)
            : base(id, fight)
        {
            BonusMin = 20;
            BonusMax = 20;
        }

        public override void Initialize()
        {
            base.Initialize();

            foreach (var fighter in Fight.GetAllFighters<MonsterFighter>())
                Suivre(fighter);

            m_equipeEnnemie = Fight.GetAllFighters<MonsterFighter>().FirstOrDefault()?.Team;
            if (m_equipeEnnemie != null)
                m_equipeEnnemie.FighterAdded += OnEnnemiAjoute;
        }

        // MYSTIC 17/10 : tous les retraits de PM (sorts avec durée = buff, vols de PM, retraits directs) font échouer le challenge ;
        // avant, seuls les retraits directs sans durée étaient vus. Les invocations ennemies sont suivies aussi.
        readonly System.Collections.Generic.List<FightActor> m_suivis = new System.Collections.Generic.List<FightActor>();
        FightTeam m_equipeEnnemie;

        void Suivre(FightActor fighter)
        {
            if (m_suivis.Contains(fighter))
                return;

            m_suivis.Add(fighter);
            fighter.FightPointsVariation += OnFightPointsVariation;
            fighter.PMRetires += OnPMRetires;
        }

        void OnEnnemiAjoute(FightTeam team, FightActor fighter) => Suivre(fighter);

        void OnPMRetires(FightActor cible, FightActor source)
        {
            if (source == null || source == cible || source.IsFriendlyWith(cible))
                return;

            UpdateStatus(ChallengeStatusEnum.FAILED, source);
        }

        public override bool IsEligible() => Fight.GetAllCharacters().Any(x => x.BreedId != PlayableBreedEnum.Pandawa);

        void OnFightPointsVariation(FightActor actor, ActionsEnum action, FightActor source, FightActor target, short delta)
        {
            if (delta >= 0)
                return;

            if (actor == source)
                return;

            if (action != ActionsEnum.ACTION_CHARACTER_MOVEMENT_POINTS_LOST)
                return;

            UpdateStatus(ChallengeStatusEnum.FAILED, source);
        }

        protected override void OnWinnersDetermined(IFight fight, FightTeam winners, FightTeam losers, bool draw)
        {
            base.OnWinnersDetermined(fight, winners, losers, draw);

            foreach (var fighter in m_suivis)
            {
                fighter.FightPointsVariation -= OnFightPointsVariation;
                fighter.PMRetires -= OnPMRetires;
            }

            if (m_equipeEnnemie != null)
                m_equipeEnnemie.FighterAdded -= OnEnnemiAjoute;
        }
    }
}
