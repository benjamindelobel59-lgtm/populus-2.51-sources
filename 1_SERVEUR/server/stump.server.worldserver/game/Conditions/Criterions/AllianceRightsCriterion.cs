using System;
using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;

namespace Stump.Server.WorldServer.Game.Conditions.Criterions
{
    /// <summary>
    /// MYSTIC 17/10 : « Ox » = droit d'alliance (objets d'alliance : potions de renommage / blason Ox=1, Prisme Ox=2).
    /// Droits officiels simplifiés : le chef de l'alliance (meneur de la guilde meneuse) a tous les droits ; les meneurs des autres
    /// guildes de l'alliance gèrent les prismes ; tout membre peut parler sur le canal.
    /// </summary>
    public class AllianceRightsCriterion : Criterion
    {
        public const string Identifier = "Ox";

        public AllianceRightsBitEnum Right { get; set; }

        public static bool AUnDroit(Character character, AllianceRightsBitEnum right)
        {
            var alliance = character?.Guild?.Alliance;
            if (alliance == null)
                return false;

            if (alliance.IsLeader(character))
                return true;

            switch (right)
            {
                case AllianceRightsBitEnum.ALLIANCE_RIGHT_NONE:
                case AllianceRightsBitEnum.ALLIANCE_RIGHT_TALK_IN_CHAN:
                    return true;
                case AllianceRightsBitEnum.ALLIANCE_RIGHT_MANAGE_PRISMS:
                    // meneur de guilde, ou membre ayant le droit de guilde officiel « Poser / modifier un prisme d'alliance »
                    return character.GuildMember != null && (character.GuildMember.IsBoss ||
                        character.GuildMember.HasRight(GuildRightsBitEnum.GUILD_RIGHT_SET_ALLIANCE_PRISM));
                default:
                    return false;
            }
        }

        public override bool Eval(Character character) => AUnDroit(character, Right);

        public override void Build()
        {
            if (!int.TryParse(Literal, out var right))
                throw new Exception(string.Format("Cannot build AllianceRightsCriterion, {0} is not a valid right", Literal));

            Right = (AllianceRightsBitEnum)right;
        }

        public override string ToString() => FormatToString(Identifier);
    }

    /// <summary>MYSTIC 17/10 : « Oc » (critère du Prisme d'alliance, Oc=R) : sens inconnu, toujours vrai ; la pose vérifie le reste.</summary>
    public class OcCriterion : Criterion
    {
        public const string Identifier = "Oc";

        public override bool Eval(Character character) => true;

        public override void Build()
        {
        }

        public override string ToString() => FormatToString(Identifier);
    }
}
