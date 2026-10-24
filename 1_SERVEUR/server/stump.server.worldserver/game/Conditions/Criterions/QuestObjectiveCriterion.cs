using System;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using System.Linq;
using Stump.Server.WorldServer.Game.Quests;

namespace Stump.Server.WorldServer.Game.Conditions.Criterions
{
    public class QuestObjectiveCriterion : Criterion
    {
        public const string Identifier = "Qo";

        public int QuestObjectiveId
        {
            get;
            set;
        }

        public override bool Eval(Character character)
        {
            // PADGREF 21/10 : « Qo » = objectif de quête EN COURS (critères des drops de quête du jeu : « Qo=N », « Qo>A&Qo<B ») ;
            // avant : objectif terminé (jamais vrai au moment où la ressource est utile). Non utilisé ailleurs en base.
            var enCours = character.Quests.Where(x => !x.Finished && x.CurrentStep != null)
                .SelectMany(x => x.CurrentStep.Objectives.Where(y => !y.Finished).Select(y => (int)y.ObjectiveRecord.ObjectiveId)).ToList();
            switch (Operator)
            {
                case ComparaisonOperatorEnum.INEQUALS: return !enCours.Contains(QuestObjectiveId);
                case ComparaisonOperatorEnum.SUPERIOR: return enCours.Any(o => o > QuestObjectiveId);
                case ComparaisonOperatorEnum.INFERIOR: return enCours.Any(o => o < QuestObjectiveId);
                default: return enCours.Contains(QuestObjectiveId);
            }
        }

        public override void Build()
        {
            int questObjectiveId;

            if (!int.TryParse(Literal, out questObjectiveId))
                throw new Exception(string.Format("Cannot build QuestActiveCriterion, {0} is not a valid quest objective id", Literal));

            QuestObjectiveId = questObjectiveId;
        }

        public override string ToString()
        {
            return FormatToString(Identifier);
        }
    }
}