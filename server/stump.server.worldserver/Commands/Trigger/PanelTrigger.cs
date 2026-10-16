using System.Text;
using System.Text.RegularExpressions;
using Stump.Core.IO;
using Stump.Server.BaseServer.Commands;
using Stump.Server.BaseServer.Network;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;

namespace Stump.Server.WorldServer.Commands.Trigger
{
    /// <summary>
    /// Commande lancee depuis le panel admin web : executee avec les droits du personnage MJ,
    /// les reponses sont gardees (sans HTML) pour etre renvoyees au panel au lieu d'aller dans le chat.
    /// </summary>
    public class PanelTrigger : GameTrigger
    {
        public PanelTrigger(string args, Character character)
            : base(new StringStream(args), character)
        {
        }

        public StringBuilder Output { get; } = new StringBuilder();

        public bool HadError { get; private set; }

        public override ICommandsUser User => Character;

        public override void Reply(string text)
        {
            Output.AppendLine(Regex.Replace(text ?? "", "<[^>]+>", ""));
        }

        public override void ReplyError(string message)
        {
            HadError = true;
            Reply(message);
        }

        public override BaseClient GetSource() => Character?.Client;
    }
}
