using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Database.World;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using System.Linq;

namespace Stump.Server.WorldServer.Game.Fights.Teams
{
    public class FightMonsterTeam : FightTeamWithLeader<MonsterFighter>
    {
        public FightMonsterTeam(TeamEnum id, Cell[] placementCells) : base(id, placementCells)
        {
        }

        public FightMonsterTeam(TeamEnum id, Cell[] placementCells, AlignmentSideEnum alignmentSide)
            : base(id, placementCells, alignmentSide)
        {
        }

        public override TeamTypeEnum TeamType
        {
            get { return TeamTypeEnum.TEAM_TYPE_MONSTER; }
        }

        public override FighterRefusedReasonEnum CanJoin(Character character)
        {
            if (this == Fight.DefendersTeam)
            {
                if (Fight.Map.GetBlueFightPlacement().Count() < this.Fighters.Count() + 1) return FighterRefusedReasonEnum.TEAM_LIMITED_BY_MAINCHARACTER;
            }
            else
            {
                if (Fight.Map.GetRedFightPlacement().Count() < this.Fighters.Count() + 1) return FighterRefusedReasonEnum.TEAM_LIMITED_BY_MAINCHARACTER;
            }
            // PADGREF 22/10 (demande utilisateur) : personne ne rejoint l'équipe des monstres par leur épée, même un MJ (avant : MJ
            // autorisés, ce qui faisait entrer en combat dans une zone bloquée)
            return FighterRefusedReasonEnum.WRONG_ALIGNMENT;
        }
    }
}