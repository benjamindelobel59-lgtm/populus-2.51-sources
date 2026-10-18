using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Alliances;
using Stump.Server.WorldServer.Handlers.Alliances;
using Stump.Server.WorldServer.Handlers.Dialogs;
using NetworkGuildEmblem = Stump.DofusProtocol.Types.GuildEmblem;

namespace Stump.Server.WorldServer.Game.Dialogs.Alliances
{
    /// <summary>MYSTIC 17/10 : fenêtre de création d'alliance (livre du Temple des alliances).</summary>
    public class AllianceCreationPanel : IDialog
    {
        public AllianceCreationPanel(Character character)
        {
            Character = character;
        }

        public Character Character { get; }

        public DialogTypeEnum DialogType => DialogTypeEnum.DIALOG_ALLIANCE_CREATE;

        public void Open()
        {
            if (Character.Guild?.Alliance != null)
            {
                AllianceHandler.SendAllianceCreationResultMessage(Character.Client, SocialGroupCreationResultEnum.SOCIAL_GROUP_CREATE_ERROR_ALREADY_IN_GROUP);
                return;
            }

            if (Character.GuildMember == null || !Character.GuildMember.IsBoss)
            {
                Character.SendServerMessage("Il faut être meneur d'une guilde pour fonder une alliance.");
                return;
            }

            Character.SetDialog(this);
            AllianceHandler.SendAllianceCreationStartedMessage(Character.Client);
        }

        public void Close()
        {
            Character.CloseDialog(this);
            DialogHandler.SendLeaveDialogMessage(Character.Client, DialogType);
        }

        public void CreateAlliance(string name, string tag, NetworkGuildEmblem emblem)
        {
            var result = AllianceManager.Instance.CreateAlliance(Character, name, tag, emblem);
            AllianceHandler.SendAllianceCreationResultMessage(Character.Client, result);

            if (result == SocialGroupCreationResultEnum.SOCIAL_GROUP_CREATE_OK)
                Close();
        }
    }

    /// <summary>MYSTIC 17/10 : fenêtre de renommage / changement de blason (potions d'alliance, chef de l'alliance).</summary>
    public class AllianceModificationPanel : IDialog
    {
        public AllianceModificationPanel(Character character)
        {
            Character = character;
        }

        public Character Character { get; }

        public bool ChangeName { get; set; }

        public bool ChangeEmblem { get; set; }

        public DialogTypeEnum DialogType => DialogTypeEnum.DIALOG_ALLIANCE_RENAME;

        public void Open()
        {
            Character.SetDialog(this);
            AllianceHandler.SendAllianceModificationStartedMessage(Character.Client, ChangeName, ChangeEmblem);
        }

        public void Close()
        {
            Character.CloseDialog(this);
            DialogHandler.SendLeaveDialogMessage(Character.Client, DialogType);
        }

        public void Modify(string name, string tag, NetworkGuildEmblem emblem)
        {
            if (!ChangeName)
            {
                name = null;
                tag = null;
            }

            if (!ChangeEmblem)
                emblem = null;

            if (name == null && tag == null && emblem == null)
                return;

            var result = AllianceManager.Instance.Modifier(Character, name, tag, emblem);
            AllianceHandler.SendAllianceCreationResultMessage(Character.Client, result);

            if (result == SocialGroupCreationResultEnum.SOCIAL_GROUP_CREATE_OK)
                Close();
        }
    }
}
