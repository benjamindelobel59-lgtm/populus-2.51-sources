using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using Stump.Core.Reflection;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Messages;
using Stump.DofusProtocol.Types;
using Stump.Server.WorldServer.Database.Items.Templates;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Effects.Instances;
using Stump.Server.WorldServer.Game.Exchanges;
using Stump.Server.WorldServer.Game.Exchanges.Trades;
using Stump.Server.WorldServer.Game.Exchanges.Trades.Players;
using Stump.Server.WorldServer.Game.Items;
using Stump.Server.WorldServer.Game.Items.Player;
using Stump.Server.WorldServer.Handlers.Inventory;

namespace Stump.Server.WorldServer.Game.Alliances
{
    /// <summary>
    /// MYSTIC 17/10 : alliances phase 4 (données officielles du client 2.51) :
    /// - modules de prisme (objets de type 112, effet 1154 = emplacement : 0 téléportation, 1 recyclage), gérés depuis le menu du
    ///   prisme « Gérer les modules » (échange de type ALLIANCE_PRISM, comme une banque) par les membres ayant le droit de gérer les prismes ;
    /// - téléportation de prisme à prisme seulement, si le prisme de départ ET celui d'arrivée ont un module de téléportation (14552) ;
    /// - recyclage (menu du prisme « Recycler », recycleur installé) : pépites de chaque objet = Items.d2o nuggetsBySubarea
    ///   (table nekfeu_items_pepites), doublées dans les sous-zones de l'alliance équipées d'un recycleur, réparties selon l'effet 2021
    ///   du recycleur (équitable 50 / 50, collectiviste 30 / 90, Kokoko 10 / 130 : personnage / alliance) ; la part de l'alliance
    ///   s'accumule sur le prisme, celle du personnage lui est donnée en Pépites (14635) ;
    /// - fin de KOH : les pépites du prisme sont partagées entre les participants de l'alliance gagnante (AvAManager).
    /// </summary>
    public class PrismModules : Singleton<PrismModules>
    {
        static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public const int ModuleTeleportation = 14552;
        public const int Pepite = 14635;
        public const int EffetEmplacement = 1154;
        public const int EffetRepartition = 2021;
        public const int TypeModule = 112;
        public const int ModuleTypeTeleporteur = 0; // AlliancePrismModuleTypeEnum du client
        public const int ModuleTypeRecycleur = 1;

        Dictionary<int, List<(int subArea, double pepites)>> m_pepites = new Dictionary<int, List<(int, double)>>();

        public void Charger()
        {
            try
            {
                m_pepites = WorldServer.Instance.DBAccessor.Database.Fetch<PepiteRow>("SELECT * FROM nekfeu_items_pepites")
                    .GroupBy(x => x.ItemId).ToDictionary(g => g.Key, g => g.Select(x => (x.SubAreaId, x.Pepites)).ToList());
                logger.Info("[Alliances] pépites de recyclage : {0} objets", m_pepites.Count);
            }
            catch (Exception e)
            {
                logger.Error("[Alliances] table nekfeu_items_pepites absente (SQL 41) : {0}", e.Message);
            }
        }

        public class PepiteRow
        {
            public int ItemId { get; set; }
            public int SubAreaId { get; set; }
            public double Pepites { get; set; }
        }

        #region Modules

        public static List<int> Modules(PrismeAlliance p) =>
            string.IsNullOrEmpty(p.Modules) ? new List<int>() : p.Modules.Split(',').Select(int.Parse).ToList();

        static int? Emplacement(ItemTemplate t)
        {
            if (t == null || t.TypeId != TypeModule)
                return null;

            var effet = t.PossibleEffects?.FirstOrDefault(x => x.EffectId == EffetEmplacement);
            return effet == null ? (int?)null : (effet as DofusProtocol.D2oClasses.EffectInstanceInteger)?.Value ?? 0;
        }

        public static bool ATeleportation(PrismeAlliance p) => Modules(p).Contains(ModuleTeleportation);

        /// <summary>Recycleur installé : (part du personnage, part de l'alliance) en %, sinon null.</summary>
        public static (int joueur, int alliance)? Repartition(PrismeAlliance p)
        {
            foreach (var id in Modules(p))
            {
                var effet = ItemManager.Instance.TryGetTemplate(id)?.PossibleEffects?.OfType<DofusProtocol.D2oClasses.EffectInstanceDice>()
                    .FirstOrDefault(x => x.EffectId == EffetRepartition);
                if (effet != null)
                    return ((int)effet.DiceNum, (int)effet.DiceSide);
            }
            return null;
        }

        public static ObjectItem[] Objets(PrismeAlliance p) =>
            Modules(p).Select(id => ItemManager.Instance.TryGetTemplate(id)).Where(t => t != null)
                .Select(t => new ObjectItem(63, (ushort)t.Id, t.Effects.Select(e => e.GetObjectEffect()).ToArray(), (uint)t.Id, 1)).ToArray();

        static void Sauver(PrismeAlliance p)
        {
            PrismManager.Executer("UPDATE nekfeu_prisms SET Modules = @1, Pepites = @2 WHERE SubAreaId = @0", p.SubAreaId, p.Modules ?? "", p.Pepites);
            PrismManager.Instance.Annoncer(p);
        }

        public string Installer(Character c, PrismeAlliance p, BasePlayerItem item)
        {
            var emplacement = Emplacement(item.Template);
            if (emplacement == null)
                return "Seuls les modules de prisme peuvent être installés.";

            var modules = Modules(p);
            if (modules.Any(id => Emplacement(ItemManager.Instance.TryGetTemplate(id)) == emplacement))
                return "Un module de ce type est déjà installé sur ce prisme : retirez-le d'abord.";

            c.Inventory.RemoveItem(item, 1);
            modules.Add(item.Template.Id);
            p.Modules = string.Join(",", modules);
            Sauver(p);
            return null;
        }

        public bool Retirer(Character c, PrismeAlliance p, int templateId)
        {
            var modules = Modules(p);
            if (!modules.Remove(templateId))
                return false;

            var template = ItemManager.Instance.TryGetTemplate(templateId);
            if (template != null)
                c.Inventory.AddItem(template);
            p.Modules = string.Join(",", modules);
            Sauver(p);
            return true;
        }

        #endregion

        #region Pépites

        /// <summary>Pépites d'un exemplaire de l'objet (règle du client : ×2 dans les sous-zones de l'alliance équipées d'un recycleur).</summary>
        public double PepitesDe(int itemId, ICollection<int> sousZonesRecycleur)
        {
            if (!m_pepites.TryGetValue(itemId, out var lignes))
                return 0;

            return lignes.Sum(x => sousZonesRecycleur.Contains(x.subArea) ? 2 * x.pepites : x.pepites);
        }

        public bool EstRecyclable(int itemId) => m_pepites.ContainsKey(itemId);

        public static HashSet<int> SousZonesRecycleur(Alliance alliance) =>
            new HashSet<int>(PrismManager.Instance.PrismesDe(alliance).Where(x => Repartition(x) != null).Select(x => x.SubAreaId));

        public static void DonnerPepites(Character c, int nombre)
        {
            var pepite = ItemManager.Instance.TryGetTemplate(Pepite);
            if (pepite != null && nombre > 0)
                c.Inventory.AddItem(pepite, nombre);
        }

        #endregion

        #region Utilisation du prisme (menu : Téléporter / Recycler)

        public void Utiliser(Character c, int typeModule)
        {
            var npc = c.Map.GetActors<PrismNpc>().FirstOrDefault();
            var alliance = c.Guild?.Alliance;
            if (npc == null || alliance == null || npc.Prisme.AllianceId != alliance.Id)
                return;

            var p = npc.Prisme;
            if (p.State == (int)PrismStateEnum.PRISM_STATE_VULNERABLE)
            {
                AvAManager.Message(c, "Ce prisme est vulnérable : il ne peut pas être utilisé.");
                return;
            }

            if (c.Dialog != null)
                c.Dialog.Close();

            if (typeModule == ModuleTypeTeleporteur)
            {
                if (!ATeleportation(p))
                {
                    AvAManager.Message(c, "Ce prisme n'a pas de module de téléportation.");
                    return;
                }

                // de prisme à prisme seulement : les destinations doivent aussi avoir un module de téléportation
                var destinations = PrismManager.Instance.PrismesDe(alliance).Where(x => x != p && ATeleportation(x)).ToList();
                if (destinations.Count == 0)
                {
                    AvAManager.Message(c, "Aucun autre prisme de votre alliance n'a de module de téléportation.");
                    return;
                }

                new PrismTeleportDialog(c, destinations).Open();
            }
            else if (typeModule == ModuleTypeRecycleur)
            {
                var repartition = Repartition(p);
                if (repartition == null)
                {
                    AvAManager.Message(c, "Ce prisme n'a pas de recycleur.");
                    return;
                }

                new RecycleTrade(c, p, repartition.Value.joueur, repartition.Value.alliance).Open();
            }
        }

        public void OuvrirModules(Character c)
        {
            var npc = c.Map.GetActors<PrismNpc>().FirstOrDefault();
            var alliance = c.Guild?.Alliance;
            if (npc == null || alliance == null || npc.Prisme.AllianceId != alliance.Id)
                return;

            if (!Conditions.Criterions.AllianceRightsCriterion.AUnDroit(c, AllianceRightsBitEnum.ALLIANCE_RIGHT_MANAGE_PRISMS))
            {
                AvAManager.Message(c, "Vous n'avez pas le droit de gérer les modules des prismes.");
                return;
            }

            if (c.Dialog != null)
                c.Dialog.Close();

            new PrismModulesDialog(c, npc.Prisme).Open();
        }

        #endregion
    }

    /// <summary>MYSTIC 17/10 : « Gérer les modules » (échange officiel ALLIANCE_PRISM, affiché comme un coffre).</summary>
    public class PrismModulesDialog : IExchange
    {
        public PrismModulesDialog(Character character, PrismeAlliance prisme)
        {
            Prisme = prisme;
            Customer = new PrismModulesCustomer(character, this);
        }

        public PrismeAlliance Prisme { get; }

        public PrismModulesCustomer Customer { get; }

        public Character Character => Customer.Character;

        public ExchangeTypeEnum ExchangeType => ExchangeTypeEnum.ALLIANCE_PRISM;

        public DialogTypeEnum DialogType => DialogTypeEnum.DIALOG_EXCHANGE;

        public void Open()
        {
            Character.Client.Send(new ExchangeStartedWithStorageMessage((sbyte)ExchangeType, 2));
            Character.Client.Send(new StorageInventoryContentMessage(PrismModules.Objets(Prisme), 0));
            Character.SetDialoger(Customer);
        }

        public void Close()
        {
            InventoryHandler.SendExchangeLeaveMessage(Character.Client, DialogType, false);
            Character.ResetDialog();
        }
    }

    public class PrismModulesCustomer : Exchanger
    {
        readonly PrismModulesDialog m_dialog;

        public PrismModulesCustomer(Character character, PrismModulesDialog dialog) : base(dialog)
        {
            Character = character;
            m_dialog = dialog;
        }

        public Character Character { get; }

        public override bool MoveItem(uint id, int quantity)
        {
            var p = m_dialog.Prisme;
            if (quantity > 0)
            {
                var item = Character.Inventory.TryGetItem((int)id);
                if (item == null)
                    return false;

                var erreur = PrismModules.Instance.Installer(Character, p, item);
                if (erreur != null)
                {
                    AvAManager.Message(Character, erreur);
                    return false;
                }

                var objet = PrismModules.Objets(p).FirstOrDefault(x => x.ObjectGID == item.Template.Id);
                if (objet != null)
                    Character.Client.Send(new StorageObjectUpdateMessage(objet));
                return true;
            }

            if (quantity < 0 && PrismModules.Instance.Retirer(Character, p, (int)id))
            {
                Character.Client.Send(new StorageObjectRemoveMessage(id));
                return true;
            }

            return false;
        }

        public override bool SetKamas(long amount) => false;
    }

    /// <summary>MYSTIC 17/10 : recyclage au prisme (échange officiel RECYCLE_TRADE, interface « Recycler » du client).</summary>
    public class RecycleTrade : ITrade
    {
        readonly PrismeAlliance m_prisme;
        readonly int m_partJoueur;
        readonly int m_partAlliance;
        bool m_recyclage;

        public RecycleTrade(Character character, PrismeAlliance prisme, int partJoueur, int partAlliance)
        {
            m_prisme = prisme;
            m_partJoueur = partJoueur;
            m_partAlliance = partAlliance;
            Trader = new RecycleTrader(character, this);
        }

        public RecycleTrader Trader { get; }

        public Character Character => Trader.Character;

        public Trader FirstTrader => Trader;

        public Trader SecondTrader => Trader;

        public DialogTypeEnum DialogType => DialogTypeEnum.DIALOG_EXCHANGE;

        public ExchangeTypeEnum ExchangeType => ExchangeTypeEnum.RECYCLE_TRADE;

        public void Open()
        {
            Character.Client.Send(new ExchangeStartOkRecycleTradeMessage((short)m_partAlliance, (short)m_partJoueur));
            Character.SetDialoger(Trader);
            Trader.ReadyStatusChanged += OnReadyStatusChanged;
            Trader.ItemMoved += OnItemMoved;
        }

        void OnItemMoved(Trader trader, TradeItem item, bool modified, int difference)
        {
            if (m_recyclage)
                return;

            if (!modified && item.Stack > 0)
                InventoryHandler.SendExchangeObjectAddedMessage(Character.Client, false, item);
            else if (item.Stack <= 0)
                InventoryHandler.SendExchangeObjectRemovedMessage(Character.Client, false, item.Guid);
            else
                InventoryHandler.SendExchangeObjectModifiedMessage(Character.Client, false, item);
        }

        void OnReadyStatusChanged(Trader trader, bool pret)
        {
            if (!pret)
                return;

            Recycler();
            trader.ToggleReady(false);
        }

        void Recycler()
        {
            var items = Trader.Items.OfType<PlayerTradeItem>().ToArray();
            if (items.Length == 0)
                return;

            m_recyclage = true;
            var sousZones = PrismModules.SousZonesRecycleur(Character.Guild?.Alliance);
            double joueur = 0, alliance = 0;
            foreach (var item in items)
            {
                var pepites = PrismModules.Instance.PepitesDe(item.Template.Id, sousZones) * item.Stack;
                joueur += pepites * m_partJoueur / 100;
                alliance += pepites * m_partAlliance / 100;
            }

            foreach (var item in items)
            {
                Character.Inventory.RemoveItem(item.PlayerItem, (int)item.Stack);
                Trader.MoveItem((uint)item.Guid, 0);
            }

            var pourJoueur = (int)Math.Floor(joueur);
            var pourPrisme = (int)Math.Floor(alliance);
            PrismModules.DonnerPepites(Character, pourJoueur);
            if (pourPrisme > 0)
            {
                m_prisme.Pepites += pourPrisme;
                PrismManager.Executer("UPDATE nekfeu_prisms SET Pepites = @1 WHERE SubAreaId = @0", m_prisme.SubAreaId, m_prisme.Pepites);
                PrismManager.Instance.Annoncer(m_prisme);
            }

            Character.Client.Send(new RecycleResultMessage((uint)pourPrisme, (uint)pourJoueur));
            m_recyclage = false;
        }

        public void Close()
        {
            Character.ResetDialog();
            InventoryHandler.SendExchangeLeaveMessage(Character.Client, DialogType, false);
        }
    }

    public class RecycleTrader : PlayerTrader
    {
        public RecycleTrader(Character character, RecycleTrade trade) : base(character, trade)
        {
        }

        public override bool MoveItemToPanel(BasePlayerItem item, int amount)
        {
            if (item.IsEquiped() || !PrismModules.Instance.EstRecyclable(item.Template.Id))
                return false;

            return base.MoveItemToPanel(item, amount);
        }

        public override bool SetKamas(long amount) => false;
    }
}
