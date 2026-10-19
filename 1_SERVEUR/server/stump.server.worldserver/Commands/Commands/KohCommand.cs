using Stump.DofusProtocol.Enums;
using Stump.Server.BaseServer.Commands;
using Stump.Server.WorldServer.Commands.Trigger;
using Stump.Server.WorldServer.Game.Alliances;

namespace Stump.Server.WorldServer.Commands.Commands
{
    /// <summary>
    /// MYSTIC 17/10 : commandes de test de l'AvA dans la sous-zone de l'admin (sans attendre les créneaux ni les 30 minutes) :
    /// .koh (lance le KOH), .koh win (votre alliance gagne), .koh perdu (le propriétaire garde la zone), .koh points N,
    /// .koh pret (saute la minute de présence), .koh info, .koh invu (retire l'invulnérabilité de tous les prismes).
    /// </summary>
    public class KohCommand : CommandBase
    {
        public KohCommand()
        {
            Aliases = new[] { "koh" };
            Description = "Tests AvA : .koh [start | win | perdu | points N | pret | info | invu]";
            RequiredRole = RoleEnum.Administrator;
            AddParameter("action", "a", "start, win, perdu, points, pret, info", "start", true);
            AddParameter("valeur", "v", "Nombre de points (.koh points N)", 0, true);
        }

        public override void Execute(TriggerBase trigger)
        {
            if (!(trigger is GameTrigger game))
                return;

            trigger.Reply(AvAManager.Instance.CommandeKoh(game.Character, trigger.Get<string>("action"), trigger.Get<int>("valeur")));
        }
    }
}
