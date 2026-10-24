using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Stump.Server.WorldServer.Game.Items;

namespace Stump.Server.WorldServer.Game.Fights.Results
{
    public class FightLoot
    {
        private readonly Dictionary<int, DroppedItem> m_items = new Dictionary<int, DroppedItem>();

        public FightLoot()
        {
            
        }
        
        public IReadOnlyDictionary<int, DroppedItem> Items
        {
            get { return new ReadOnlyDictionary<int, DroppedItem>(m_items); }
        }

        public long Kamas
        {
            get;
            set;
        }

        public void AddItem(int itemId)
        {
            AddItem(itemId, 1);
        }

        public void AddItem(int itemId, uint amount)
        {
            if (m_items.ContainsKey(itemId))
                m_items[itemId].Amount++;
            else
                m_items.Add(itemId, new DroppedItem(itemId, amount));
        }

        public void AddItem(DroppedItem item)
        {
            if (m_items.ContainsKey(item.ItemId))
                m_items[item.ItemId].Amount += item.Amount;
            else
                m_items.Add(item.ItemId, item);
        }

        public DofusProtocol.Types.FightLoot GetFightLoot()
        {
            // PADGREF 22/10 (demande utilisateur) : dans la ligne de fin de combat, les box de boss d'abord, puis les Dofus, puis le
            // reste dans l'ordre du drop (le client affiche les objets dans l'ordre reçu)
            return new DofusProtocol.Types.FightLoot(m_items.Values.OrderBy(Priorite).SelectMany(entry => new[] {
                (uint) entry.ItemId, entry.Amount
            }).ToArray(), (ulong)Kamas);
        }

        static int Priorite(DroppedItem item)
        {
            if (Game.Items.Player.Custom.BoxDeBoss.EstBox(item.ItemId))
                return 0;
            return Game.Items.ItemManager.Instance.TryGetTemplate(item.ItemId)?.TypeId == (int)DofusProtocol.Enums.ItemTypeEnum.DOFUS ? 1 : 2;
        }

        public string FightItemsString()
        {
            return string.Join("|", m_items.Select(item => item.Value.ItemId + "_" + item.Value.Amount).ToList());
        }
    }
}