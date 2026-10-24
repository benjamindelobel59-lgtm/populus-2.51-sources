using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Commands.Commands.Patterns;
using Stump.Server.WorldServer.Commands.Trigger;
using Stump.Server.WorldServer.Game.Bugs;

namespace Stump.Server.WorldServer.Commands.Commands
{
    /// <summary>
    /// PADGREF 23/10 : commande INTERNE du bouton « Replay » de la fenêtre de fin de combat (client : ui::FightResult et
    /// ui::FightResultSimple, à côté de « Fermer »). Pas une commande pour les joueurs : elle n'est pas annoncée.
    ///   replaycombat          lien de la page du site qui rejoue le dernier combat (le client l'ouvre dans le navigateur)
    ///   replaycombat stop     arrête une relecture en jeu (plus lancée par le bouton, gardée par sécurité)
    /// </summary>
    public class ReplayCommand : InGameCommand
    {
        public ReplayCommand()
        {
            Aliases = new[] { "replaycombat" };
            Description = "Interne : bouton « Replay » de la fin de combat.";
            RequiredRole = RoleEnum.Player;
            AddParameter<string>("option", "o", "stop / code d'un replay (admins)", isOptional: true);
        }

        public override void Execute(GameTrigger trigger)
        {
            var perso = trigger.Character;
            var option = trigger.IsArgumentDefined("option") ? (trigger.Get<string>("option") ?? "").Trim().ToLowerInvariant() : "";

            if (option == "stop")
            {
                ReplayLecteur.Arreter(perso);
                return;
            }

            var lien = ReplayCombat.CreerLien(perso, out var erreur);
            if (lien == null)
            {
                perso.SendServerMessage("Replay : " + erreur);
                return;
            }

            // PADGREF 23/10 (demande utilisateur) : le replay se regarde SEULEMENT sur le site ; le client 64 bits ouvre ce
            // lien tout seul dans le navigateur (PopulusButtons), le 32 bits l'a dans le chat
            perso.SendServerMessage("Replay : ouverture du site {url," + lien + "::Clique ici si la page ne s'est pas ouverte}" +
                " (lien valable " + ReplayCombat.ConservationJours + " jours, à joindre à un signalement de bug)");
        }
    }
}
