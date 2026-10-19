using System;
using System.Linq;
using NLog;
using Stump.DofusProtocol.Enums;
using Stump.Server.BaseServer.Initialization;
using Stump.Server.WorldServer.Database.Items;
using Stump.Server.WorldServer.Database.World;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Houses;
using Stump.Server.WorldServer.Game.Maps;
using Stump.Server.WorldServer.Game.Maps.Cells;
using Stump.Server.WorldServer.Game.Maps.Paddocks;
using Stump.Server.WorldServer.Handlers.Basic;

namespace Stump.Server.WorldServer.Game.Items.Player.Custom
{
    /// <summary>
    /// MYSTIC 19/10 : objets infinis (SQL 73, client : _OUTILS/client/client-build/objets_infinis) : ne se consomment jamais.
    ///   31200 Pain Infini : rend 50 000 PV ; mangé automatiquement à la fin de chaque combat s'il est dans l'inventaire (Character.RejoinMap).
    ///   31201 Potion de Rappel infinie : point de sauvegarde.
    ///   31202 Potion de Foyer de Guilde infinie : maison d'un membre de la guilde (le meneur d'abord), devant la porte.
    ///   31203 Potion d'enclos de Guilde infinie : enclos de la guilde.
    ///   31204 Potion de Chasse au Trésor infinie : devant la Malle aux Trésors.
    /// </summary>
    public class ObjetInfini : BasePlayerItem
    {
        static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public const int PainInfini = 31200, Rappel = 31201, FoyerGuilde = 31202, EnclosGuilde = 31203, ChasseTresor = 31204;
        public const int SoinPain = 50000;
        const int CarteMalle = 142088718;   // devant la Malle aux Trésors (= Autopilote.CarteSortieMalle)

        public ObjetInfini(Character owner, PlayerItemRecord record) : base(owner, record) { }

        [Initialization(typeof(ItemManager))]
        public static void Enregistrer()
        {
            foreach (var id in new[] { PainInfini, Rappel, FoyerGuilde, EnclosGuilde, ChasseTresor })
            {
                try { ItemManager.Instance.AddItemIdConstructor(typeof(ObjetInfini), (ItemIdEnum)id); }
                catch (Exception ex) { logger.Warn($"[Objets infinis] objet {id} déjà géré ailleurs : {ex.Message}"); }
            }
        }

        public override uint UseItem(int amount = 1, Cell targetCell = null, Character target = null)
        {
            switch (Template.Id)
            {
                case PainInfini: Manger(Owner, true); break;
                case Rappel: Owner.Teleport(Owner.GetSpawnPoint()); break;
                case FoyerGuilde: MaisonDeGuilde(); break;
                case EnclosGuilde: EnclosDeGuilde(); break;
                case ChasseTresor: Aller(World.Instance.GetMap(CarteMalle)); break;
            }
            return 0;   // jamais consommé
        }

        /// <summary>Rend jusqu'à 50 000 PV. <paramref name="manuel"/> : clic dans l'inventaire (message si déjà plein).</summary>
        public static bool Manger(Character c, bool manuel)
        {
            var manque = c.Stats.Health.DamageTaken;
            if (manque <= 0)
            {
                if (manuel)
                    BasicHandler.SendTextInformationMessage(c.Client, TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 225);
                return false;
            }
            var soin = Math.Min(manque, SoinPain);
            c.Stats.Health.DamageTaken -= soin;
            c.RefreshStats();
            BasicHandler.SendTextInformationMessage(c.Client, TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 1, soin);
            return true;
        }

        /// <summary>Fin de combat : Pain Infini dans l'inventaire -> mangé tout seul (pas pour un mort / une tombe / un fantôme).</summary>
        public static void ApresCombat(Character c)
        {
            try
            {
                if (c.IsGhost() || c.PlayerLifeStatus != PlayerLifeStatusEnum.STATUS_ALIVE_AND_KICKING)
                    return;
                var pain = ItemManager.Instance.TryGetTemplate(PainInfini);
                if (pain != null && c.Inventory.HasItem(pain))
                    Manger(c, false);
            }
            catch (Exception ex)
            {
                logger.Error($"[Objets infinis] pain de fin de combat ({c.Name}) : {ex.Message}");
            }
        }

        void Aller(Map map, int cellId = -1)
        {
            if (map == null)
            {
                Owner.SendServerMessage("Destination introuvable.");
                return;
            }
            var cell = cellId >= 0 && cellId < map.Cells.Length && map.Cells[cellId].Walkable ? map.Cells[cellId] : map.GetRandomFreeCell();
            Owner.Teleport(new ObjectPosition(map, cell, Owner.Direction));
        }

        void MaisonDeGuilde()
        {
            var guilde = Owner.Guild;
            if (guilde == null)
            {
                Owner.SendServerMessage("Tu n'as pas de guilde.");
                return;
            }
            var membres = guilde.Members.Select(x => x.Id).ToList();
            var boss = guilde.Boss?.Id;
            var maison = HouseManager.Instance.GetHouses()
                .Where(h => h.Record.OwnerId.HasValue && membres.Contains(h.Record.OwnerId.Value) && h.Record.DoorMap != null)
                .OrderBy(h => h.Record.OwnerId == boss ? 0 : 1).ThenBy(h => h.Record.Id).FirstOrDefault();
            if (maison == null)
            {
                Owner.SendServerMessage("Aucun membre de ta guilde ne possède de maison.");
                return;
            }
            Aller(maison.Record.DoorMap);
        }

        void EnclosDeGuilde()
        {
            var guilde = Owner.Guild;
            if (guilde == null)
            {
                Owner.SendServerMessage("Tu n'as pas de guilde.");
                return;
            }
            var enclos = PaddockManager.Instance.GetGuildPaddocks(guilde).FirstOrDefault(x => x.Map != null);
            if (enclos == null)
            {
                Owner.SendServerMessage("Ta guilde ne possède pas d'enclos.");
                return;
            }
            Aller(enclos.Map);
        }
    }
}
