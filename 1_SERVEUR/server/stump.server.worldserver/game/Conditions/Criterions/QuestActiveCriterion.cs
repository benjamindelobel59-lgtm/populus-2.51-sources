using System;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using System.Linq;

namespace Stump.Server.WorldServer.Game.Conditions.Criterions
{
    public class QuestActiveCriterion : Criterion
    {
        public const string Identifier = "Qa";

        public int QuestId
        {
            get;
            set;
        }

        public override bool Eval(Character character)
        {
            var vrai = character.Quests.Where(x => !x.Finished && x.Id == QuestId).Count() > 0;
            return Operator == ComparaisonOperatorEnum.INEQUALS ? !vrai : vrai; // PADGREF 21/10 : « ! » était ignoré
        }

        public override void Build()
        {
            int questId;

            if (!int.TryParse(Literal, out questId))
                throw new Exception(string.Format("Cannot build QuestActiveCriterion, {0} is not a valid quest id", Literal));

            QuestId = questId;
        }

        public override string ToString()
        {
            return FormatToString(Identifier);
        }
    }
}