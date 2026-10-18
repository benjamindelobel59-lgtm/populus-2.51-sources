using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Messages;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Dialogs;

namespace Stump.Server.WorldServer.Game.Alliances
{
    /// <summary>MYSTIC 17/10 : invitation d'une guilde (son meneur) dans l'alliance du chef qui recrute.</summary>
    public class AllianceInvitationRequest : RequestBox
    {
        public AllianceInvitationRequest(Character source, Character target)
            : base(source, target)
        {
        }

        void Etat(SocialGroupInvitationStateEnum etat)
        {
            Source.Client.Send(new AllianceInvitationStateRecruterMessage(Target.Name, (sbyte)etat));
            Target.Client.Send(new AllianceInvitationStateRecrutedMessage((sbyte)etat));
        }

        protected override void OnOpen()
        {
            Etat(SocialGroupInvitationStateEnum.SOCIAL_GROUP_INVITATION_SENT);
            var alliance = Source.Guild?.Alliance;
            if (alliance != null)
                Target.Client.Send(new AllianceInvitedMessage((ulong)Source.Id, Source.Name, alliance.GetBasicNamedAllianceInformations()));
        }

        protected override void OnAccept()
        {
            Etat(SocialGroupInvitationStateEnum.SOCIAL_GROUP_INVITATION_OK);

            var alliance = Source.Guild?.Alliance;
            var guild = Target.Guild;
            if (alliance == null || guild == null || guild.Alliance != null || Target.GuildMember == null || !Target.GuildMember.IsBoss)
                return;

            AllianceManager.Instance.AjouterGuilde(alliance, guild);
        }

        protected override void OnDeny()
        {
            Etat(SocialGroupInvitationStateEnum.SOCIAL_GROUP_INVITATION_CANCELED);
        }

        protected override void OnCancel()
        {
            Etat(SocialGroupInvitationStateEnum.SOCIAL_GROUP_INVITATION_CANCELED);
        }
    }
}
