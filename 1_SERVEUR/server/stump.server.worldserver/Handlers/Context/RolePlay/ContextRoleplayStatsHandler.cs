using System;
using System.Collections.Generic;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Messages;
using Stump.Server.BaseServer.Network;
using Stump.Server.WorldServer.Core.Network;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;

namespace Stump.Server.WorldServer.Handlers.Context.RolePlay
{
    public partial class ContextRoleplayHandler : WorldHandlerContainer
    {
        private readonly static Dictionary<StatsBoostTypeEnum, PlayerFields> m_statsEnumRelations = new Dictionary<StatsBoostTypeEnum, PlayerFields>
        {
                {StatsBoostTypeEnum.Strength, PlayerFields.Strength},
                {StatsBoostTypeEnum.Agility, PlayerFields.Agility},
                {StatsBoostTypeEnum.Chance, PlayerFields.Chance},
                {StatsBoostTypeEnum.Wisdom, PlayerFields.Wisdom},
                {StatsBoostTypeEnum.Intelligence, PlayerFields.Intelligence},
                {StatsBoostTypeEnum.Vitality, PlayerFields.Vitality},
            };

        [WorldHandler(StatsUpgradeRequestMessage.Id)]
        public static void HandleStatsUpgradeRequestMessage(WorldClient client, StatsUpgradeRequestMessage message)
        {
            if (client.Character.IsInFight())
            {
                SendStatsUpgradeResultMessage(client, StatsUpgradeResultEnum.IN_FIGHT, (short)message.BoostPoint);
                return;
            }

            if(client.Character.IsInIncarnation)
            {
                return;
            }

            var statsid = (StatsBoostTypeEnum)message.StatId;

            if (statsid < StatsBoostTypeEnum.Strength ||
                statsid > StatsBoostTypeEnum.Intelligence)
                throw new Exception("Wrong statsid");

            if (message.BoostPoint <= 0)
            {
                SendStatsUpgradeResultMessage(client, StatsUpgradeResultEnum.NOT_ENOUGH_POINT, (short)message.BoostPoint);
                return;
            }

            if (message.BoostPoint > client.Character.StatsPoints)
            {
                SendStatsUpgradeResultMessage(client, StatsUpgradeResultEnum.NOT_ENOUGH_POINT, (short)message.BoostPoint);
                return;
            }

            BoostStat(client.Character, statsid, (ushort)message.BoostPoint);

            SendStatsUpgradeResultMessage(client, StatsUpgradeResultEnum.SUCCESS, (short)message.BoostPoint);
            client.Character.RefreshStats();
        }

        /// <summary>
        /// Nekfeu : depense <paramref name="points"/> points de caracteristique dans <paramref name="statsid"/> (paliers de la classe).
        /// Retourne le nombre de points reellement utilises. Utilise aussi par le Sasa auto (.sasa).
        /// </summary>
        public static ushort BoostStat(Character character, StatsBoostTypeEnum statsid, ushort points)
        {
            if (points < 1 || points > character.StatsPoints)
                return 0;

            var breed = character.Breed;
            var actualPoints = character.Stats[m_statsEnumRelations[statsid]].Base;
            var pts = (int)points;

            var thresholds = breed.GetThresholds(statsid);
            var index = breed.GetThresholdIndex(actualPoints, thresholds);

            // [0] => limit
            // [1] => pts for 1
            // [2] => boosts with 1

            // while enough pts to boost once
            while (pts >= thresholds[index][1])
            {
                ushort ptsUsed;
                ushort boost;
                // if not last threshold and enough pts to reach the next threshold we fill this first
                if (index < thresholds.Length - 1 && (pts / (double)thresholds[index][1]) > (thresholds[index + 1][0] - actualPoints))
                {
                    boost = (ushort) (thresholds[index + 1][0] - actualPoints);
                    ptsUsed = (ushort)( boost * thresholds[index][1] );

                    if (thresholds[index].Length > 2)
                        boost = (ushort) (boost * thresholds[index][2]);
                }
                else
                {
                    boost = (ushort)Math.Floor( pts / (double)thresholds[index][1] );
                    ptsUsed = (ushort)( boost * thresholds[index][1] );

                    if (thresholds[index].Length > 2)
                        boost = (ushort)(boost * thresholds[index][2]);
                }

                actualPoints += boost;
                pts -= ptsUsed;

                index = breed.GetThresholdIndex(actualPoints, thresholds);
            }

            character.Stats[m_statsEnumRelations[statsid]].Base = actualPoints;
            character.StatsPoints -= (ushort)(points - pts);
            character.RefreshStats();

            return (ushort)(points - pts);
        }

        public static void SendStatsUpgradeResultMessage(IPacketReceiver client, StatsUpgradeResultEnum result, short usedpts)
        {
            client.Send(new StatsUpgradeResultMessage((sbyte)result, (ushort)usedpts));
        }
        [WorldHandler(ResetCharacterStatsRequestMessage.Id)]
        public static void HandleResetCharacterStatsRequestMessage(WorldClient client, ResetCharacterStatsRequestMessage message)
        {
            if (client.Character.IsInFight() && client.Character.Fight.State == Game.Fights.FightState.Fighting)
            {
                client.Character.SendServerMessage("Action impossible en combat.");
                return;
            }
            client.Character.ResetStats();
        }
    }
}