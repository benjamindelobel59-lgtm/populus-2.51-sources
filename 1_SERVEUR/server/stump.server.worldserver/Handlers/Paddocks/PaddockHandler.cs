using System.Linq;
using NLog;
using Stump.DofusProtocol.Messages;
using Stump.Server.WorldServer.Core.Network;
using Stump.Server.WorldServer.Game;
using Stump.Server.WorldServer.Game.Dialogs.Paddocks;
using Stump.Server.WorldServer.Game.Interactives.Skills;
using Stump.Server.WorldServer.Game.Maps;
using Stump.Server.WorldServer.Game.Maps.Paddocks;
using Stump.Server.WorldServer.Handlers.Interactives;

namespace Stump.Server.WorldServer.Handlers.Paddocks
{
    /// <summary>
    /// NEKFEU : achat / vente des enclos par les guildes.
    /// </summary>
    public class PaddockHandler : WorldHandlerContainer
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        [WorldHandler(PaddockBuyRequestMessage.Id)]
        public static void HandlePaddockBuyRequestMessage(WorldClient client, PaddockBuyRequestMessage message)
        {
            (client.Character.Dialog as PaddockBuySell)?.BuyPaddock(message.ProposedPrice);
        }

        [WorldHandler(PaddockSellRequestMessage.Id)]
        public static void HandlePaddockSellRequestMessage(WorldClient client, PaddockSellRequestMessage message)
        {
            (client.Character.Dialog as PaddockBuySell)?.SellPaddock(message.Price, message.ForSale);
        }

        /// <summary>
        /// Met a jour, pour chaque joueur de la map, les infos de l'enclos et les competences de ses portes
        /// (Acheter / Vendre / Modifier le prix / Acceder dependent du joueur).
        /// </summary>
        public static void SendPaddockRefresh(Map map, Paddock paddock)
        {
            if (map == null)
                return;

            // l'enclos depend du joueur (maps d'enclos partagees derriere les hubs) -> envoi joueur par joueur
            var doors = map.GetInteractiveObjects().Where(x => x.GetSkills().Any(s => s is SkillPaddockBase)).ToArray();
            foreach (var character in map.GetAllCharacters())
            {
                var properties = PaddockManager.Instance.GetPaddockPropertiesMessage(character, map);
                if (properties != null)
                    character.Client.Send(properties);

                foreach (var door in doors)
                    InteractiveHandler.SendInteractiveElementUpdatedMessage(character.Client, character, door);
            }
        }

        /// <summary>
        /// Changement de proprietaire : les montures en etable de cet enclos (qui appartiennent aux personnages de l'ancienne guilde)
        /// deviendraient inaccessibles -> elles sont deplacees dans l'etable d'un enclos public. Idem pour les montures restees dans l'enclos.
        /// </summary>
        public static void MoveStabledMountsToPublicPaddock(Paddock paddock)
        {
            var target = PaddockManager.Instance.GetPaddocks().Where(x => x.IsPublicPaddock() && x != paddock).OrderBy(x => x.Id).FirstOrDefault();
            if (target == null)
            {
                logger.Error("[Enclos] aucun enclos public pour recevoir les montures de l'enclos {0}", paddock.Id);
                return;
            }

            // personnages connectes : objets en memoire
            foreach (var character in World.Instance.GetCharacters())
            {
                var moved = character.StabledMounts.Where(x => x.Paddock == paddock).ToArray();
                foreach (var mount in moved)
                    mount.Paddock = target;

                if (moved.Length > 0)
                    character.SendServerMessage($"{moved.Length} monture(s) de votre étable ont été déplacées dans l'étable de l'enclos public (map {target.Map.Id}) : l'enclos a changé de propriétaire.");
            }

            // montures restees dans l'enclos (normalement aucune : un enclos doit etre vide pour etre mis en vente)
            foreach (var mount in paddock.PaddockedMounts.ToArray())
            {
                paddock.RemoveMountFromPaddock(mount);
                mount.Paddock = target;
                mount.IsInStable = true;
                mount.Save(PaddockManager.Instance.Database);
            }

            // personnages deconnectes : directement en base
            WorldServer.Instance.IOTaskPool.ExecuteInContext(() =>
            {
                var count = PaddockManager.Instance.Database.Execute("UPDATE mounts SET PaddockId = @0 WHERE PaddockId = @1 AND IsInStable = 1", target.Id, paddock.Id);
                if (count > 0)
                    logger.Info("[Enclos] enclos {0} : {1} monture(s) en etable deplacee(s) vers l'enclos public {2}", paddock.Id, count, target.Id);
            });
        }
    }
}
