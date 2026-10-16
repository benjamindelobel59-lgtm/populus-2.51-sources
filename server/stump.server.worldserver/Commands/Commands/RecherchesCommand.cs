using System.Linq;
using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Commands.Commands.Patterns;
using Stump.Server.WorldServer.Commands.Trigger;
using Stump.Server.WorldServer.Game.Recherches;

namespace Stump.Server.WorldServer.Commands.Commands
{
    /// <summary>
    /// MYSTIC : .recherches [nom] → position de chaque recherché (ou heure de réapparition). Réservé au staff (tests).
    /// </summary>
    public class RecherchesCommand : InGameCommand
    {
        public RecherchesCommand()
        {
            Aliases = new[] { "recherches" };
            Description = "Position des recherchés (ou heure de réapparition). Ex. : .recherches carlita";
            RequiredRole = RoleEnum.GameMaster;
        }

        public override void Execute(GameTrigger trigger)
        {
            var filtre = (trigger.Args.NextWord() ?? "").Trim().ToLowerInvariant();
            var lignes = RecherchesManager.Instance.GetEtat().Where(x => filtre.Length == 0 || x.ToLowerInvariant().Contains(filtre)).ToList();
            if (lignes.Count == 0)
            {
                trigger.Character.SendServerMessage("Aucun recherché trouvé.");
                return;
            }

            foreach (var ligne in lignes)
                trigger.Character.SendServerMessage(ligne);
        }
    }
}
