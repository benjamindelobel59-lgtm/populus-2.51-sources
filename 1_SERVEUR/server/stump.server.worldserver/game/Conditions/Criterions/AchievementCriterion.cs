using System;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;

namespace Stump.Server.WorldServer.Game.Conditions.Criterions
{
    /// <summary>MYSTIC 19/10 : « OA=id » = succès validé, « OA!id » = succès non validé (avant : toujours vrai).</summary>
    public class AchievementCriterion : Criterion
    {
        public const string Identifier = "OA";

        public uint AchievementId
        {
            get;
            set;
        }

        public override void Build()
        {
            if (!uint.TryParse(Literal, out var id))
                throw new Exception(string.Format("Cannot build AchievementCriterion, {0} is not a valid achievement id", Literal));

            AchievementId = id;
        }

        public override bool Eval(Character character)
        {
            var fait = character.Achievement != null && character.Achievement.AchievementIsCompleted(AchievementId);
            return Operator == ComparaisonOperatorEnum.INEQUALS ? !fait : fait;
        }

        public override string ToString() => FormatToString(Identifier);
    }
}
