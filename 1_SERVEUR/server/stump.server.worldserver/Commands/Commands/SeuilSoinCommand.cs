using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.AI.Fights.Spells;
using Stump.Server.WorldServer.Commands.Commands.Patterns;
using Stump.Server.WorldServer.Commands.Trigger;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Monsters;

namespace Stump.Server.WorldServer.Commands.Commands
{
    /// <summary>
    /// MYSTIC : .seuilsoin &lt;id monstre&gt; [%|defaut] → seuil de PV sous lequel ce monstre soigne avant d'attaquer
    /// (lui ou un allié). Sans %, affiche le seuil. 0 = jamais en priorité, « defaut » = seuil par défaut (50 %). Effet immédiat.
    /// </summary>
    public class SeuilSoinCommand : InGameCommand
    {
        public SeuilSoinCommand()
        {
            Aliases = new[] { "seuilsoin" };
            Description = "Seuil de soin d'urgence d'un monstre : .seuilsoin <id monstre> [% | defaut]";
            RequiredRole = RoleEnum.GameMaster;
        }

        public override void Execute(GameTrigger trigger)
        {
            var character = trigger.Character;
            if (!int.TryParse(trigger.Args.NextWord() ?? "", out var monsterId))
            {
                character.SendServerMessage("Utilisation : .seuilsoin <id monstre> [% | defaut]  (0 = ne soigne jamais en priorité)");
                return;
            }

            var template = MonsterManager.Instance.GetTemplate(monsterId);
            var nom = template != null ? template.Name : "monstre " + monsterId;
            var valeur = (trigger.Args.NextWord() ?? "").Trim().ToLowerInvariant();

            if (valeur.Length == 0)
            {
                var seuil = SeuilsSoinManager.Instance.GetSeuil(monsterId);
                character.SendServerMessage(string.Format("{0} ({1}) : soigne en priorité sous {2} % de PV{3}.", nom, monsterId, seuil,
                    SeuilsSoinManager.Instance.HasCustom(monsterId) ? "" : " (seuil par défaut)"));
                return;
            }

            if (valeur == "defaut" || valeur == "défaut")
            {
                SeuilsSoinManager.Instance.SetSeuil(monsterId, -1);
                character.SendServerMessage(string.Format("{0} ({1}) : retour au seuil par défaut ({2} %).", nom, monsterId, SpellSelector.HealUrgencyPercent));
                return;
            }

            if (!int.TryParse(valeur.TrimEnd('%'), out var pct) || pct < 0 || pct > 100)
            {
                character.SendServerMessage("Seuil invalide : un nombre entre 0 et 100, ou « defaut ».");
                return;
            }

            SeuilsSoinManager.Instance.SetSeuil(monsterId, pct);
            character.SendServerMessage(pct == 0
                ? string.Format("{0} ({1}) : ne soignera plus en priorité.", nom, monsterId)
                : string.Format("{0} ({1}) : soigne en priorité sous {2} % de PV.", nom, monsterId, pct));
        }
    }
}
