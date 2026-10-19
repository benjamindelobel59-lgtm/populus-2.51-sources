using System.Linq;
using Stump.DofusProtocol.Enums;
using Stump.Server.BaseServer.Commands;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Heroique;

namespace Stump.Server.WorldServer.Commands.Commands
{
    /// <summary>
    /// MYSTIC : .resu &lt;personnage&gt; : ressuscite un personnage mort en mode héroïque (mort par bug).
    /// Mort en PvM : il revient avec tout son inventaire. Mort en agression : ses objets ont été donnés aux vainqueurs.
    /// </summary>
    public class ResurrectCommand : CommandBase
    {
        public ResurrectCommand()
        {
            Aliases = new[] { "resu", "ressusciter" };
            RequiredRole = RoleEnum.Administrator;
            Description = "Mode héroïque : ressuscite un personnage mort (par un bug), avec son inventaire s'il est mort en PvM.";
            AddParameter<string>("personnage", "p", "Nom du personnage mort");
        }

        public override void Execute(TriggerBase trigger)
        {
            var nom = trigger.Get<string>("personnage");
            WorldServer.Instance.IOTaskPool.AddMessage(() =>
            {
                var record = CharacterManager.Instance.GetCharacterByName(nom);
                if (record == null)
                {
                    trigger.ReplyError("Personnage « {0} » introuvable.", nom);
                    return;
                }

                trigger.Reply(HeroicManager.Instance.Resurrect(record));
            });
        }
    }

    /// <summary>
    /// MYSTIC 19/10 : .modeheroique on|off : active ou désactive le mode héroïque (mort définitive) pour tout le serveur, tout de suite
    /// et après redémarrage (gardé en base). Désactivé : plus aucune défaite n'est mortelle et les personnages déjà morts redeviennent
    /// jouables ; réactivé : ils sont de nouveau morts. Sans paramètre : affiche l'état.
    /// </summary>
    public class ModeHeroiqueCommand : CommandBase
    {
        public ModeHeroiqueCommand()
        {
            Aliases = new[] { "modeheroique", "heroiquemode", "hardcore" };
            RequiredRole = RoleEnum.Administrator;
            Description = "Mode héroïque (mort définitive) : on = activer, off = désactiver, sans paramètre = état actuel.";
            AddParameter<string>("etat", "e", "on ou off", isOptional: true);
        }

        public override void Execute(TriggerBase trigger)
        {
            var etat = trigger.IsArgumentDefined("etat") ? trigger.Get<string>("etat")?.Trim().ToLowerInvariant() : null;
            if (string.IsNullOrEmpty(etat))
            {
                trigger.Reply("Mode héroïque (mort définitive) : {0}.", HeroicManager.Enabled ? "ACTIVÉ" : "DÉSACTIVÉ");
                return;
            }

            bool actif;
            if (etat == "on" || etat == "1" || etat == "oui" || etat == "activer")
                actif = true;
            else if (etat == "off" || etat == "0" || etat == "non" || etat == "desactiver" || etat == "désactiver")
                actif = false;
            else
            {
                trigger.ReplyError("Utilisation : .modeheroique on | off");
                return;
            }

            HeroicManager.Instance.Activer(actif);
            Stump.Server.WorldServer.Game.World.Instance.SendAnnounce(actif
                ? "<b>Le mode héroïque est ACTIVÉ : une défaite est désormais une mort définitive.</b>"
                : "<b>Le mode héroïque est DÉSACTIVÉ : les défaites ne sont plus mortelles.</b>", System.Drawing.Color.OrangeRed);
            trigger.Reply("Mode héroïque {0}.", actif ? "activé" : "désactivé");
        }
    }

    /// <summary>MYSTIC : .heroique &lt;personnage&gt; : état héroïque d'un personnage (vivant ou mort, nombre de morts, record).</summary>
    public class HeroiqueInfoCommand : CommandBase
    {
        public HeroiqueInfoCommand()
        {
            Aliases = new[] { "heroique", "hero" };
            RequiredRole = RoleEnum.Moderator;
            Description = "Mode héroïque : affiche l'état d'un personnage (vivant ou mort, morts, record de niveau).";
            AddParameter<string>("personnage", "p", "Nom du personnage");
        }

        public override void Execute(TriggerBase trigger)
        {
            var nom = trigger.Get<string>("personnage");
            WorldServer.Instance.IOTaskPool.AddMessage(() =>
            {
                var record = CharacterManager.Instance.GetCharacterByName(nom);
                if (record == null)
                {
                    trigger.ReplyError("Personnage « {0} » introuvable.", nom);
                    return;
                }

                var e = HeroicManager.Instance.GetEtat(record.Id);
                if (e == null)
                {
                    trigger.Reply("{0} : vivant, jamais mort.", record.Name);
                    return;
                }

                var mort = e.DeathState == (sbyte)HardcoreOrEpicDeathStateEnum.DEATH_STATE_DEAD;
                trigger.Reply("{0} : {1}, {2} mort(s), record niveau {3}{4}",
                    record.Name, mort ? "MORT" : "vivant", e.DeathCount, e.DeathMaxLevel,
                    e.DerniereMort.HasValue ? $", dernière mort le {e.DerniereMort:dd/MM/yyyy HH:mm} ({(e.MortPvP ? "agression" : "PvM")})" : "");
            });
        }
    }
}
