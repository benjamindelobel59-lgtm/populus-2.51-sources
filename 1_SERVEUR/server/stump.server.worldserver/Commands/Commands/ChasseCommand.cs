using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Commands.Commands.Patterns;
using Stump.Server.WorldServer.Commands.Trigger;
using Stump.Server.WorldServer.Game.Chasses;

namespace Stump.Server.WorldServer.Commands.Commands
{
    /// <summary>
    /// MYSTIC 19/10 : .chasse aller = trajet automatique vers l'indice en cours (envoyé aussi par le lien « Y aller » du chat) ;
    /// .chasse = rappel de l'indice en cours. Réservé à sa propre chasse au trésor : ne permet pas d'aller ailleurs.
    /// </summary>
    public class ChasseCommand : InGameCommand
    {
        public ChasseCommand()
        {
            Aliases = new[] { "chasse" };
            Description = "Chasse au trésor : « .chasse aller » = trajet automatique vers l'indice en cours ; « .chasse » = rappel de l'indice.";
            RequiredRole = RoleEnum.Player;
        }

        public override void Execute(GameTrigger trigger)
        {
            var c = trigger.Character;
            var arg = trigger.Args.HasNext ? trigger.Args.NextWord()?.ToLowerInvariant() : null;
            if (arg == "aller")
            {
                ChasseManager.Instance.Aller(c);
                return;
            }
            var ch = ChasseManager.Instance.GetChasse(c);
            if (ch == null)
            {
                c.SendServerMessage("Tu n'as pas de chasse au trésor en cours (Malle aux Trésors, Champs de Cania [-25,-36]).");
                return;
            }
            ChasseManager.Instance.Envoyer(c, ch);
            ChasseManager.Instance.AnnoncerIndice(c, ch);
        }
    }
}
