using System.Linq;
using NLog;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Messages;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Interactives;
using Stump.Server.WorldServer.Game.Maps.Paddocks;
using Stump.Server.WorldServer.Handlers.Dialogs;
using Stump.Server.WorldServer.Handlers.Paddocks;
using MapPaddock = Stump.Server.WorldServer.Game.Maps.Paddocks.Paddock;

namespace Stump.Server.WorldServer.Game.Dialogs.Paddocks
{
    /// <summary>
    /// NEKFEU : fenetre d'achat d'un enclos (meneur de guilde) ou de mise en vente / changement de prix (meneur de la guilde proprietaire).
    /// Client : PaddockSellBuyDialogMessage -> PaddockBuyRequestMessage / PaddockSellRequestMessage -> LeaveDialogMessage (DIALOG_PURCHASABLE).
    /// </summary>
    public class PaddockBuySell : IDialog
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        readonly InteractiveObject m_door;

        public PaddockBuySell(Character character, MapPaddock paddock, bool sell, ulong price, InteractiveObject door)
        {
            Character = character;
            Paddock = paddock;
            Sell = sell;
            Price = price;
            m_door = door;
        }

        public Character Character { get; }

        public MapPaddock Paddock { get; }

        public bool Sell { get; }

        public ulong Price { get; }

        public DialogTypeEnum DialogType => DialogTypeEnum.DIALOG_PURCHASABLE;

        public void Open()
        {
            Character.SetDialog(this);
            // la fenetre du client lit la taille de l'enclos dans le dernier PaddockPropertiesMessage recu
            Character.Client.Send(Paddock.GetPaddockPropertiesMessage());
            Character.Client.Send(new PaddockSellBuyDialogMessage(Sell, (uint)(Paddock.HasOwner ? Paddock.Guild.Id : 0), Price));
        }

        public void Close()
        {
            Character.CloseDialog(this);
            DialogHandler.SendLeaveDialogMessage(Character.Client, DialogType);
        }

        public void BuyPaddock(ulong proposedPrice)
        {
            if (Sell || !Paddock.CanBuy(Character))
            {
                Close();
                return;
            }

            var price = (ulong)Paddock.Price;
            if (proposedPrice != price)
            {
                Character.Client.Send(new PaddockBuyResultMessage(Paddock.Id, false, price));
                Close();
                return;
            }

            if (Character.Kamas < price)
            {
                // Vous n'avez pas assez de kamas pour effectuer cette action.
                Character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 82);
                Character.Client.Send(new PaddockBuyResultMessage(Paddock.Id, false, price));
                Close();
                return;
            }

            var oldGuild = Paddock.HasOwner ? Paddock.Guild : null;
            if (oldGuild != null)
            {
                // les kamas vont au meneur de la guilde vendeuse
                var sellerId = oldGuild.Boss?.Id;
                if (sellerId != null)
                {
                    var seller = World.Instance.GetCharacter(sellerId.Value);
                    if (seller != null)
                    {
                        seller.Inventory.AddKamas(price);
                        seller.SendServerMessage($"L'enclos de votre guilde a été vendu à la guilde {Character.Guild.Name} pour {price} kamas.");
                    }
                    else
                        WorldServer.Instance.IOTaskPool.ExecuteInContext(() => PaddockManager.Instance.Database.Execute("UPDATE characters SET Kamas = Kamas + @0 WHERE Id = @1", (long)price, sellerId.Value));
                }
                else
                    logger.Warn("[Enclos] vente de l'enclos {0} : la guilde {1} n'a pas de meneur, {2} kamas non verses", Paddock.Id, oldGuild.Id, price);
            }

            Character.Inventory.SubKamas(price);

            PaddockHandler.MoveStabledMountsToPublicPaddock(Paddock);
            Paddock.SetOwner(Character.Guild);
            WorldServer.Instance.IOTaskPool.ExecuteInContext(() => Paddock.Save(PaddockManager.Instance.Database));

            logger.Info("[Enclos] {0} (guilde {1}) achete l'enclos {2} (map {3}) pour {4} kamas (ancienne guilde : {5})",
                Character.Name, Character.Guild.Id, Paddock.Id, Paddock.Map.Id, price, oldGuild?.Id.ToString() ?? "aucune");

            Character.Client.Send(new PaddockBuyResultMessage(Paddock.Id, true, price));
            Close();
            PaddockHandler.SendPaddockRefresh(m_door?.Map ?? Paddock.Map, Paddock);
        }

        public void SellPaddock(ulong price, bool forSale)
        {
            if (!Sell || !Paddock.IsOwnerBoss(Character))
            {
                Close();
                return;
            }

            if (forSale)
            {
                if (price == 0 || price > int.MaxValue)
                {
                    Character.SendServerMessage("Prix de vente invalide.");
                    Close();
                    return;
                }

                if (Paddock.MountsCount > 0)
                {
                    Character.SendServerMessage("Il faut d'abord retirer toutes les montures de l'enclos avant de le mettre en vente.");
                    Close();
                    return;
                }

                Paddock.SetForSale(true, (int)price);
                Character.SendServerMessage($"Votre enclos est maintenant en vente pour {price} kamas.");
            }
            else
            {
                Paddock.SetForSale(false, Paddock.Price);
                Character.SendServerMessage("Votre enclos n'est plus en vente.");
            }

            WorldServer.Instance.IOTaskPool.ExecuteInContext(() => Paddock.Save(PaddockManager.Instance.Database));
            logger.Info("[Enclos] {0} (guilde {1}) : enclos {2} en vente = {3}, prix {4}", Character.Name, Character.Guild.Id, Paddock.Id, Paddock.OnSale, Paddock.Price);

            Close();
            PaddockHandler.SendPaddockRefresh(m_door?.Map ?? Paddock.Map, Paddock);
        }
    }
}
