using System;
using System.Collections.Generic;
using System.Linq;
using Stump.Core.Mathematics;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Types;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Effects;
using Stump.Server.WorldServer.Game.Effects.Instances;
using Stump.Server.WorldServer.Game.Exchanges.Trades;
using Stump.Server.WorldServer.Game.Exchanges.Trades.Players;
using Stump.Server.WorldServer.Handlers.Inventory;

namespace Stump.Server.WorldServer.Game.Exchanges.Craft.Runes
{
    public class RuneTrade : ITrade
    {
        public RuneTrade(Character character)
        {
            Trader = new RuneTrader(character, this);
        }

        public DialogTypeEnum DialogType => DialogTypeEnum.DIALOG_EXCHANGE;
        public ExchangeTypeEnum ExchangeType => ExchangeTypeEnum.RUNES_TRADE;

        private bool m_decrafting;

        public void Open()
        {
            InventoryHandler.SendExchangeStartOkRunesTradeMessage(Trader.Character.Client);
            Trader.Character.SetDialoger(Trader);

            Trader.ReadyStatusChanged += OnReadyStatusChanged;
            Trader.ItemMoved += OnItemMoved;
        }

        private void OnItemMoved(Trader trader, TradeItem item, bool modified, int difference)
        {
            if (m_decrafting)
                return;

            if (!modified && item.Stack > 0)
                InventoryHandler.SendExchangeObjectAddedMessage(Character.Client, false, item);

            else if (item.Stack <= 0)
                InventoryHandler.SendExchangeObjectRemovedMessage(Character.Client, false, item.Guid);

            else
                InventoryHandler.SendExchangeObjectModifiedMessage(Character.Client, false, item);
        }

        private void OnReadyStatusChanged(Trader trader, bool isready)
        {
            if (isready)
            {
                Decraft();

                trader.ToggleReady(false);
            }
        }

        public void Decraft()
        {
            m_decrafting = true;
            var results = new Dictionary<PlayerTradeItem, DecraftResult>();

            foreach(var item in Trader.Items.OfType<PlayerTradeItem>())
            {
                var result = new DecraftResult(item);
                results.Add(item, result);
                for (int i = 0; i < item.Stack; i++)
                {
                    // MYSTIC 17/10 : brisage officiel (BrisageOfficiel) : coefficient propre a l'objet, formule 0,015, focus 50 %
                    var coeff = BrisageOfficiel.Instance.Coefficient(item.Template);
                    BrisageOfficiel.Instance.Calculer(item.Template, item.Effects, coeff, Trader.Focus, result.Runes);
                    BrisageOfficiel.Instance.Briser(item.Template);

                    if (!result.MinCoeff.HasValue || coeff < result.MinCoeff)
                        result.MinCoeff = coeff;
                    if (!result.MaxCoeff.HasValue || coeff > result.MaxCoeff)
                        result.MaxCoeff = coeff;


                    Trader.Character.OnDecraftItem(item.Template, result.Runes.Sum(x => x.Value));
                }
            }
            InventoryHandler.SendDecraftResultMessage(Character.Client,
                results.Select(x => new DecraftedItemStackInfo((uint)x.Key.Guid, (float)(x.Value.MinCoeff ?? 0.5), (float)(x.Value.MaxCoeff ?? 0.5), x.Value.Runes.Select(y => (ushort)y.Key.Id).ToArray(), x.Value.Runes.Select(y => (uint)y.Value).ToArray())));

            foreach (PlayerTradeItem item in results.Keys)
            {
                Character.Inventory.RemoveItem(item.PlayerItem, (int)item.Stack);
                Trader.MoveItem((uint)item.Guid, 0);
            }

            foreach(var group in results.Values.SelectMany(x => x.Runes).GroupBy(x => x.Key))
            {
                var rune = group.Key;
                var amount = group.Sum(x => x.Value);

                if(amount < 0)
                    amount = 1;

                Character.Inventory.AddItem(rune, amount);
            }
            m_decrafting = false;
            Trader.Focus = 0;
        }


        public void Close()
        {
            Character.ResetDialog();
            InventoryHandler.SendExchangeLeaveMessage(Character.Client, DialogType, false);
        }

        public Character Character => Trader.Character;

        public RuneTrader Trader
        {
            get;
        }

        public Trader FirstTrader => Trader;

        public Trader SecondTrader => Trader;


    }
}