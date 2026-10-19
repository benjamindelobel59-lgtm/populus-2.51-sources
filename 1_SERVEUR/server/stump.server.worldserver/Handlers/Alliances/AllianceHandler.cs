using System;
using System.Linq;
using Stump.Core.Extensions;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Messages;
using Stump.DofusProtocol.Types;
using Stump.Server.BaseServer.Network;
using Stump.Server.WorldServer.Core.Network;
using Stump.Server.WorldServer.Game;
using Stump.Server.WorldServer.Game.Alliances;
using Stump.Server.WorldServer.Game.Dialogs.Alliances;

namespace Stump.Server.WorldServer.Handlers.Alliances
{
    /// <summary>MYSTIC 17/10 : messages d'alliance du protocole 2.51 (phase 1 : cœur des alliances).</summary>
    public class AllianceHandler : WorldHandlerContainer
    {
        #region Reçus du client

        [WorldHandler(AllianceCreationValidMessage.Id)]
        public static void HandleAllianceCreationValidMessage(WorldClient client, AllianceCreationValidMessage message)
        {
            var panel = client.Character.Dialog as AllianceCreationPanel;
            panel?.CreateAlliance(message.AllianceName, message.AllianceTag, message.AllianceEmblem);
        }

        [WorldHandler(AllianceModificationValidMessage.Id)]
        public static void HandleAllianceModificationValidMessage(WorldClient client, AllianceModificationValidMessage message)
        {
            var panel = client.Character.Dialog as AllianceModificationPanel;
            panel?.Modify(message.AllianceName, message.AllianceTag, message.Alliancemblem);
        }

        [WorldHandler(AllianceModificationNameAndTagValidMessage.Id)]
        public static void HandleAllianceModificationNameAndTagValidMessage(WorldClient client, AllianceModificationNameAndTagValidMessage message)
        {
            var panel = client.Character.Dialog as AllianceModificationPanel;
            panel?.Modify(message.AllianceName, message.AllianceTag, null);
        }

        [WorldHandler(AllianceModificationEmblemValidMessage.Id)]
        public static void HandleAllianceModificationEmblemValidMessage(WorldClient client, AllianceModificationEmblemValidMessage message)
        {
            var panel = client.Character.Dialog as AllianceModificationPanel;
            panel?.Modify(null, null, message.Alliancemblem);
        }

        [WorldHandler(AllianceInvitationMessage.Id)]
        public static void HandleAllianceInvitationMessage(WorldClient client, AllianceInvitationMessage message)
        {
            var character = client.Character;
            var alliance = character.Guild?.Alliance;
            if (alliance == null)
                return;

            // règle officielle : seul le chef de l'alliance (meneur de la guilde meneuse) recrute
            if (!alliance.IsLeader(character))
            {
                character.SendServerMessage("Seul le chef de l'alliance peut inviter une guilde.");
                return;
            }

            var target = World.Instance.GetCharacter((int)message.TargetId);
            if (target == null)
            {
                // Impossible d'inviter, ce joueur est inconnu ou non connecté.
                character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 208);
                return;
            }

            if (target.GuildMember == null || !target.GuildMember.IsBoss)
            {
                character.SendServerMessage("Il faut inviter le meneur d'une guilde.");
                return;
            }

            if (target.Guild.Alliance != null)
            {
                character.SendServerMessage("Cette guilde fait déjà partie d'une alliance.");
                return;
            }

            if (target.IsBusy())
            {
                // Ce joueur est occupé. Impossible de l'inviter.
                character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 209);
                return;
            }

            new AllianceInvitationRequest(character, target).Open();
        }

        [WorldHandler(AllianceInvitationAnswerMessage.Id)]
        public static void HandleAllianceInvitationAnswerMessage(WorldClient client, AllianceInvitationAnswerMessage message)
        {
            var request = client.Character.RequestBox as AllianceInvitationRequest;
            if (request == null)
                return;

            if (client.Character == request.Source && !message.Accept)
                request.Cancel();
            else if (client.Character == request.Target)
            {
                if (message.Accept)
                    request.Accept();
                else
                    request.Deny();
            }
        }

        /// <summary>Quitter (sa propre guilde) ou exclure une guilde (chef de l'alliance).</summary>
        [WorldHandler(AllianceKickRequestMessage.Id)]
        public static void HandleAllianceKickRequestMessage(WorldClient client, AllianceKickRequestMessage message)
        {
            var character = client.Character;
            var alliance = character.Guild?.Alliance;
            if (alliance == null || character.GuildMember == null || !character.GuildMember.IsBoss)
                return;

            var guild = alliance.Guilds.FirstOrDefault(x => x.Id == (int)message.KickedId);
            if (guild == null)
                return;

            if (guild == character.Guild)
                AllianceManager.Instance.RetirerGuilde(alliance, guild, false);
            else if (alliance.IsLeader(character))
                AllianceManager.Instance.RetirerGuilde(alliance, guild, true);
        }

        /// <summary>Le chef donne la tête de l'alliance à une autre guilde (droit « meneur »).</summary>
        [WorldHandler(AllianceChangeGuildRightsMessage.Id)]
        public static void HandleAllianceChangeGuildRightsMessage(WorldClient client, AllianceChangeGuildRightsMessage message)
        {
            var character = client.Character;
            var alliance = character.Guild?.Alliance;
            if (alliance == null || !alliance.IsLeader(character))
                return;

            var guild = alliance.Guilds.FirstOrDefault(x => x.Id == (int)message.GuildId);
            if (guild == null || guild == alliance.LeadingGuild)
                return;

            if ((message.Rights & (sbyte)AllianceRightsBitEnum.ALLIANCE_RIGHT_BOSS) != 0)
                AllianceManager.Instance.ChangerGuildeMeneuse(alliance, guild);
        }

        [WorldHandler(AllianceFactsRequestMessage.Id)]
        public static void HandleAllianceFactsRequestMessage(WorldClient client, AllianceFactsRequestMessage message)
        {
            var alliance = AllianceManager.Instance.TryGetAlliance((int)message.AllianceId);
            if (alliance == null)
            {
                client.Send(new AllianceFactsErrorMessage(message.AllianceId));
                return;
            }

            var leader = alliance.Leader;
            client.Send(new AllianceFactsMessage(alliance.GetAllianceFactSheetInformations(),
                alliance.Guilds.Select(alliance.GetGuildInAllianceInformations).ToArray(),
                AllianceManager.Instance.ControlledSubAreas(alliance).Select(x => (ushort)x).ToArray(),
                (ulong)(leader?.Id ?? 0), leader?.Name ?? ""));
        }

        [WorldHandler(AllianceInsiderInfoRequestMessage.Id)]
        public static void HandleAllianceInsiderInfoRequestMessage(WorldClient client, AllianceInsiderInfoRequestMessage message)
        {
            var alliance = client.Character.Guild?.Alliance;
            if (alliance == null)
                return;

            client.Send(new AllianceInsiderInfoMessage(alliance.GetAllianceFactSheetInformations(),
                alliance.Guilds.Select(alliance.GetGuildInsiderFactSheetInformations).ToArray(),
                PrismManager.Instance.PrismesDe(alliance).Select(x => (PrismSubareaEmptyInfo)x.GetGeolocalized(true)).ToArray()));
        }

        [WorldHandler(AllianceMotdSetRequestMessage.Id)]
        public static void HandleAllianceMotdSetRequestMessage(WorldClient client, AllianceMotdSetRequestMessage message)
        {
            // MYSTIC 17/10 : ordres de l'onglet « Village » du panneau d'alliance (le message du jour n'est pas modifié)
            // MYSTIC 19/10 : demande de l'onglet « Balance » (pourcentages AvA, lecture seule)
            if (message.Content != null && message.Content.StartsWith(BalanceAvA.Marqueur))
            {
                client.Character.SendServerMessage(BalanceAvA.Instance.Donnees(client.Character));
                return;
            }

            if (message.Content != null && message.Content.StartsWith(VillagesConquete.Marqueur))
            {
                VillagesConquete.Instance.Onglet(client.Character, message.Content.Substring(VillagesConquete.Marqueur.Length));
                return;
            }

            var alliance = client.Character.Guild?.Alliance;
            if (alliance == null)
                return;

            if (!alliance.IsLeader(client.Character))
            {
                client.Send(new AllianceMotdSetErrorMessage((sbyte)SocialNoticeErrorEnum.SOCIAL_NOTICE_INVALID_RIGHTS));
                return;
            }

            AllianceManager.Instance.ChangerMotd(alliance, client.Character, message.Content ?? "");
        }

        [WorldHandler(AllianceBulletinSetRequestMessage.Id)]
        public static void HandleAllianceBulletinSetRequestMessage(WorldClient client, AllianceBulletinSetRequestMessage message)
        {
            var alliance = client.Character.Guild?.Alliance;
            if (alliance == null)
                return;

            if (!alliance.IsLeader(client.Character))
            {
                client.Send(new AllianceBulletinSetErrorMessage((sbyte)SocialNoticeErrorEnum.SOCIAL_NOTICE_INVALID_RIGHTS));
                return;
            }

            AllianceManager.Instance.ChangerBulletin(alliance, client.Character, message.Content ?? "", message.NotifyMembers);
        }

        // MYSTIC 17/10 : prismes (phase 2)

        [WorldHandler(PrismsListRegisterMessage.Id)]
        public static void HandlePrismsListRegisterMessage(WorldClient client, PrismsListRegisterMessage message)
        {
            if (message.Listen != 0)
                PrismManager.Instance.EnvoyerListe(client.Character);
        }

        [WorldHandler(PrismSettingsRequestMessage.Id)]
        public static void HandlePrismSettingsRequestMessage(WorldClient client, PrismSettingsRequestMessage message)
        {
            if (!PrismManager.Instance.ReglerHeure(client.Character, message.SubAreaId, message.StartDefenseTime))
                client.Send(new PrismSettingsErrorMessage());
        }

        // MYSTIC 17/10 : AvA (phase 3)

        [WorldHandler(SetEnableAVARequestMessage.Id)]
        public static void HandleSetEnableAVARequestMessage(WorldClient client, SetEnableAVARequestMessage message)
        {
            AvAManager.Instance.ActiverAvA(client.Character, message.Enable);
        }

        [WorldHandler(PrismAttackRequestMessage.Id)]
        public static void HandlePrismAttackRequestMessage(WorldClient client, PrismAttackRequestMessage message)
        {
            AvAManager.Instance.Attaquer(client.Character);
        }

        [WorldHandler(PrismFightJoinLeaveRequestMessage.Id)]
        public static void HandlePrismFightJoinLeaveRequestMessage(WorldClient client, PrismFightJoinLeaveRequestMessage message)
        {
            var fight = AvAManager.Instance.CombatDe(message.SubAreaId);
            if (fight == null)
                return;

            if (message.Join)
            {
                var result = fight.AddDefender(client.Character);
                if (result != FighterRefusedReasonEnum.FIGHTER_ACCEPTED)
                    Context.ContextHandler.SendChallengeFightJoinRefusedMessage(client, client.Character, result);
            }
            else
            {
                fight.RemoveDefender(client.Character);
            }
        }

        [WorldHandler(PrismInfoJoinLeaveRequestMessage.Id)]
        public static void HandlePrismInfoJoinLeaveRequestMessage(WorldClient client, PrismInfoJoinLeaveRequestMessage message)
        {
            if (message.Join)
                AvAManager.Instance.EnvoyerCombats(client.Character);
        }

        [WorldHandler(PrismSetSabotagedRequestMessage.Id)]
        public static void HandlePrismSetSabotagedRequestMessage(WorldClient client, PrismSetSabotagedRequestMessage message)
        {
            AvAManager.Instance.Saboter(client.Character, message.SubAreaId);
        }

        [WorldHandler(PrismUseRequestMessage.Id)]
        public static void HandlePrismUseRequestMessage(WorldClient client, PrismUseRequestMessage message)
        {
            // MYSTIC 17/10 (phase 4) : 0 = téléportation (prisme à prisme, modules des deux côtés), 1 = recyclage
            PrismModules.Instance.Utiliser(client.Character, message.ModuleToUse);
        }

        [WorldHandler(PrismModuleExchangeRequestMessage.Id)]
        public static void HandlePrismModuleExchangeRequestMessage(WorldClient client, PrismModuleExchangeRequestMessage message)
        {
            PrismModules.Instance.OuvrirModules(client.Character);
        }

        #endregion

        #region Envoyés au client

        public static void SendAllianceCreationStartedMessage(IPacketReceiver client) => client.Send(new AllianceCreationStartedMessage());

        public static void SendAllianceModificationStartedMessage(IPacketReceiver client, bool name, bool emblem) =>
            client.Send(new AllianceModificationStartedMessage(name, name, emblem));

        public static void SendAllianceCreationResultMessage(IPacketReceiver client, SocialGroupCreationResultEnum result) =>
            client.Send(new AllianceCreationResultMessage((sbyte)result));

        public static void SendAllianceJoinedMessage(IPacketReceiver client, Alliance alliance) =>
            client.Send(new AllianceJoinedMessage(alliance.GetAllianceInformations(), true, (uint)alliance.LeadingGuildId));

        public static void SendAllianceMembershipMessage(IPacketReceiver client, Alliance alliance) =>
            client.Send(new AllianceMembershipMessage(alliance.GetAllianceInformations(), true, (uint)alliance.LeadingGuildId));

        public static void SendAllianceLeftMessage(IPacketReceiver client) => client.Send(new AllianceLeftMessage());

        public static AllianceMotdMessage MotdMessage(Alliance a) =>
            new AllianceMotdMessage(a.MotdContent ?? "", a.MotdDate.GetUnixTimeStamp(), (ulong)a.MotdMemberId, a.MotdMemberName ?? "");

        public static AllianceBulletinMessage BulletinMessage(Alliance a) =>
            new AllianceBulletinMessage(a.BulletinContent ?? "", a.BulletinDate.GetUnixTimeStamp(), (ulong)a.BulletinMemberId,
                a.BulletinMemberName ?? "", a.LastNotifiedDate.GetUnixTimeStamp());

        public static void SendMotdAndBulletin(IPacketReceiver client, Alliance alliance)
        {
            if (!string.IsNullOrEmpty(alliance.MotdContent))
                client.Send(MotdMessage(alliance));
            if (!string.IsNullOrEmpty(alliance.BulletinContent))
                client.Send(BulletinMessage(alliance));
        }

        /// <summary>À la connexion du personnage.</summary>
        public static void SendConnectionMessages(WorldClient client)
        {
            var alliance = client.Character.Guild?.Alliance;
            if (alliance != null)
            {
                SendAllianceMembershipMessage(client, alliance);
                SendMotdAndBulletin(client, alliance);
            }

            // MYSTIC 17/10 : liste des prismes (carte du monde, minimap, bonus de zone). Toujours APRÈS l'alliance : le client range les
            // prismes de son alliance dans celle-ci et plantait sans elle (aucun prisme affiché). Renvoyée après le chargement pour la minimap.
            PrismManager.Instance.EnvoyerListe(client.Character);
            var perso = client.Character;
            WorldServer.Instance.IOTaskPool.CallDelayed(5000, () =>
            {
                if (perso.IsInWorld)
                    PrismManager.Instance.EnvoyerListe(perso);
            });
        }

        /// <summary>Annuaire des alliances (envoyé avec la liste d'amis, comme l'annuaire des guildes).</summary>
        public static void SendAllianceLists(IPacketReceiver client)
        {
            var alliances = AllianceManager.Instance.Alliances.ToArray();
            client.Send(new AllianceListMessage(alliances.Select(x => x.GetAllianceFactSheetInformations()).ToArray()));
            client.Send(new AllianceVersatileInfoListMessage(alliances.Select(x => x.GetAllianceVersatileInformations()).ToArray()));
        }

        #endregion
    }
}
