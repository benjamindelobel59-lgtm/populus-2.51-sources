using Stump.DofusProtocol.Enums;
using Stump.Server.BaseServer.Commands;
using Stump.Server.WorldServer.Commands.Commands.Patterns;
using Stump.Server.WorldServer.Commands.Trigger;
using Stump.Server.WorldServer.Game.Alliances;

namespace Stump.Server.WorldServer.Commands.Commands
{
    /// <summary>
    /// MYSTIC 17/10 : villages de conquête (Brigandin / Foreuse) :
    /// .village (aide), .village destinations, .village pnj brigandin|foreuse, .village acheter N, .village revendre,
    /// .village recompense [n° conquête] [n° destination], .village confirmer.
    /// </summary>
    public class VillageCommand : InGameCommand
    {
        public VillageCommand()
        {
            Aliases = new[] { "village" };
            Description = "Villages de conquête : PNJ Brigandin / Foreuse, destinations, revente, récompenses";
            RequiredRole = RoleEnum.Player;
            AddParameter("action", "a", "destinations, pnj, acheter, revendre, recompense, confirmer", "", true);
            AddParameter("v1", "v1", "", "", true);
            AddParameter("v2", "v2", "", "", true);
        }

        public override void Execute(GameTrigger trigger)
        {
            var c = trigger.Character;
            var v = VillagesConquete.Instance;
            var action = (trigger.Get<string>("action") ?? "").ToLowerInvariant();
            var v1 = trigger.Get<string>("v1") ?? "";
            var v2 = trigger.Get<string>("v2") ?? "";
            int.TryParse(v1, out var n1);
            int.TryParse(v2, out var n2);

            string reponse;
            switch (action)
            {
                case "destinations":
                case "liste":
                    reponse = v.ListeDestinations(c);
                    break;
                case "pnj":
                    reponse = v1.ToLowerInvariant() == "foreuse" ? v.PreparerPnj(c, VillagesConquete.TypeForeuse)
                        : v1.ToLowerInvariant() == "brigandin" ? v.PreparerPnj(c, VillagesConquete.TypeBrigandin)
                        : "Usage : .village pnj brigandin  ou  .village pnj foreuse";
                    break;
                case "acheter":
                    reponse = v.PreparerAchat(c, n1);
                    break;
                case "revendre":
                    reponse = v.PreparerRevente(c);
                    break;
                case "recompense":
                    reponse = n1 > 0 && n2 > 0 ? v.ChoisirRecompense(c, n1, n2) : v.ListeRecompenses(c);
                    break;
                case "confirmer":
                    reponse = v.Confirmer(c);
                    break;
                default:
                    var alliance = c.Guild?.Alliance;
                    var pnj = alliance == null ? null : v.PnjDe(alliance.Id);
                    reponse = "Villages de conquête — " + (alliance == null ? "il faut une alliance." :
                        $"coffre : {v.Coffre(alliance.Id):N0} k ; PNJ : {(pnj == null ? "aucun" : (pnj.Type == VillagesConquete.TypeForeuse ? "Foreuse" : "Brigandin"))} ; " +
                        $"prochaine destination : {v.PrixProchaine(alliance.Id):N0} k.") +
                        "\n.village destinations | .village pnj brigandin|foreuse | .village acheter <n°> | .village revendre | .village recompense | .village confirmer" +
                        "\n.coffre | .coffre deposer <kamas> | .coffre retirer <kamas> (chef)";
                    break;
            }

            AvAManager.Message(c, reponse);
        }
    }

    /// <summary>MYSTIC 17/10 : coffre d'alliance (.coffre, .coffre deposer N, .coffre retirer N pour le chef).</summary>
    public class CoffreCommand : InGameCommand
    {
        public CoffreCommand()
        {
            Aliases = new[] { "coffre" };
            Description = "Coffre de l'alliance : .coffre deposer <kamas> | .coffre retirer <kamas> (chef)";
            RequiredRole = RoleEnum.Player;
            AddParameter("action", "a", "deposer, retirer", "", true);
            AddParameter("montant", "m", "Kamas", 0L, true);
        }

        public override void Execute(GameTrigger trigger)
        {
            var c = trigger.Character;
            var v = VillagesConquete.Instance;
            var action = (trigger.Get<string>("action") ?? "").ToLowerInvariant();
            var montant = trigger.Get<long>("montant");

            string reponse;
            if (action == "deposer" || action == "déposer")
                reponse = v.Deposer(c, montant);
            else if (action == "retirer")
                reponse = v.RetirerCoffre(c, montant);
            else
                reponse = c.Guild?.Alliance == null ? "Il faut une alliance." :
                    $"Coffre de l'alliance : {v.Coffre(c.Guild.Alliance.Id):N0} k. .coffre deposer <kamas> | .coffre retirer <kamas> (chef)";

            AvAManager.Message(c, reponse);
        }
    }
}
