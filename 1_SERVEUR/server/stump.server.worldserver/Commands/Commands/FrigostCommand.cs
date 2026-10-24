using Stump.DofusProtocol.Enums;
using Stump.Server.BaseServer.Commands;
using Stump.Server.WorldServer.Commands.Commands.Patterns;
using Stump.Server.WorldServer.Game.Maps;

namespace Stump.Server.WorldServer.Commands.Commands
{
    /// <summary>PADGREF 23/10 (demande utilisateur) : accès libre à toutes les zones de Frigost pour des joueurs qui veulent tester
    /// (sans niveau 50 ni boss vaincus). « .frigost acces [nom] », « .frigost retirer [nom] », « .frigost voir [nom] ».
    /// Sans nom : soi-même. Gardé en base (mystic_acces_frigost), donc après une reconnexion ou un redémarrage.</summary>
    public class FrigostCommand : SubCommandContainer
    {
        public FrigostCommand()
        {
            Aliases = new[] { "frigost" };
            RequiredRole = RoleEnum.Moderator;
            Description = "Accès libre à toutes les zones de Frigost (testeurs).";
        }
    }

    public class FrigostAccesCommand : TargetSubCommand
    {
        public FrigostAccesCommand()
        {
            Aliases = new[] { "acces", "accès", "on" };
            RequiredRole = RoleEnum.Moderator;
            ParentCommandType = typeof(FrigostCommand);
            Description = "Donne l'accès libre à toutes les zones de Frigost.";
            AddTargetParameter(true);
        }

        public override void Execute(TriggerBase trigger)
        {
            foreach (var cible in GetTargets(trigger))
            {
                AccesFrigost.DonnerAccesLibre(cible, true, (trigger.User as Stump.Server.WorldServer.Game.Actors.RolePlay.Characters.Character)?.Name ?? "console");
                cible.SendServerMessage("<b>Vous avez maintenant accès à toutes les zones de Frigost.</b>", System.Drawing.Color.FromArgb(0x7F, 0xD3, 0xF7));
                trigger.Reply("Accès libre à Frigost donné à {0}.", cible.Name);
            }
        }
    }

    public class FrigostRetirerCommand : TargetSubCommand
    {
        public FrigostRetirerCommand()
        {
            Aliases = new[] { "retirer", "off" };
            RequiredRole = RoleEnum.Moderator;
            ParentCommandType = typeof(FrigostCommand);
            Description = "Retire l'accès libre à Frigost (les conditions normales reviennent).";
            AddTargetParameter(true);
        }

        public override void Execute(TriggerBase trigger)
        {
            foreach (var cible in GetTargets(trigger))
            {
                AccesFrigost.DonnerAccesLibre(cible, false, null);
                trigger.Reply("Accès libre à Frigost retiré à {0}.", cible.Name);
            }
        }
    }

    public class FrigostVoirCommand : TargetSubCommand
    {
        public FrigostVoirCommand()
        {
            Aliases = new[] { "voir" };
            RequiredRole = RoleEnum.Moderator;
            ParentCommandType = typeof(FrigostCommand);
            Description = "Indique si le personnage a l'accès libre à Frigost.";
            AddTargetParameter(true);
        }

        public override void Execute(TriggerBase trigger)
        {
            foreach (var cible in GetTargets(trigger))
                trigger.Reply("{0} : accès libre à Frigost {1}.", cible.Name, AccesFrigost.AccesLibre(cible) ? "OUI" : "non");
        }
    }
}
