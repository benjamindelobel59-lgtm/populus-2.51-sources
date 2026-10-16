using System.Linq;
using Stump.DofusProtocol.Enums;
using Stump.Server.BaseServer.Database;
using Stump.Server.WorldServer.Database.Interactives;
using Stump.Server.WorldServer.Database.World.Triggers;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Dialogs.Paddocks;
using Stump.Server.WorldServer.Game.Exchanges.Paddock;
using Stump.Server.WorldServer.Game.Maps.Cells.Triggers;
using Stump.Server.WorldServer.Game.Maps.Paddocks;
using MapPaddock = Stump.Server.WorldServer.Game.Maps.Paddocks.Paddock;

namespace Stump.Server.WorldServer.Game.Interactives.Skills
{
    /// <summary>
    /// NEKFEU : base des competences de la barriere d'un enclos. L'enclos depend du joueur : enclos public de la map,
    /// ou (map d'enclos derriere un hub) l'enclos de la porte d'entree par laquelle il est arrive.
    /// </summary>
    public abstract class SkillPaddockBase : CustomSkill
    {
        protected SkillPaddockBase(int id, InteractiveCustomSkillRecord record, InteractiveObject interactiveObject)
            : base(id, record, interactiveObject)
        {
        }

        protected MapPaddock GetPaddock(Character character) => PaddockManager.Instance.GetPaddockFor(character, InteractiveObject.Map);

        public override bool AreConditionsFilled(Character character)
        {
            var paddock = GetPaddock(character);
            return paddock != null && IsAvailable(paddock, character) && base.AreConditionsFilled(character);
        }

        protected abstract bool IsAvailable(MapPaddock paddock, Character character);
    }

    /// <summary>"Acceder" (gestion des montures) : enclos public, ou membre de la guilde proprietaire avec le droit d'utiliser les enclos.</summary>
    [Discriminator("Paddock", typeof(Skill), typeof(int), typeof(InteractiveCustomSkillRecord), typeof(InteractiveObject))]
    public class SkillPaddock : SkillPaddockBase
    {
        public SkillPaddock(int id, InteractiveCustomSkillRecord record, InteractiveObject interactiveObject)
            : base (id, record, interactiveObject)
        {
        }

        protected override bool IsAvailable(MapPaddock paddock, Character character) => paddock.IsPublicPaddock() || paddock.HasOwner;

        public override int StartExecute(Character character)
        {
            if (character.IsBusy())
                return -1;

            if (!Record.AreConditionsFilled(character))
            {
                character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 1);
                return -1;
            }

            var paddock = GetPaddock(character);
            if (paddock == null)
            {
                character.SendServerMessage("Ressortez et entrez par la porte d'un enclos.");
                return -1;
            }

            if (!paddock.CanUse(character))
            {
                character.SendServerMessage(paddock.HasOwner
                    ? $"Cet enclos appartient à la guilde {paddock.Guild.Name} : il faut en être membre avec le droit d'utiliser les enclos."
                    : "Cet enclos n'a pas de propriétaire.");
                return -1;
            }

            var exchange = new PaddockExchange(character, paddock);
            exchange.Open();

            return base.StartExecute(character);
        }
    }

    /// <summary>"Acheter" : un meneur de guilde, enclos sans proprietaire ou mis en vente par une autre guilde.</summary>
    [Discriminator("PaddockBuy", typeof(Skill), typeof(int), typeof(InteractiveCustomSkillRecord), typeof(InteractiveObject))]
    public class SkillPaddockBuy : SkillPaddockBase
    {
        public SkillPaddockBuy(int id, InteractiveCustomSkillRecord record, InteractiveObject interactiveObject)
            : base(id, record, interactiveObject)
        {
        }

        // visible pour tous quand l'enclos est a vendre : le refus (pas meneur, pas de guilde...) est explique a l'utilisation
        protected override bool IsAvailable(MapPaddock paddock, Character character)
            => paddock.IsForSale && !(paddock.HasOwner && character.Guild?.Id == paddock.Guild.Id);

        public override int StartExecute(Character character)
        {
            var paddock = GetPaddock(character);
            if (paddock == null || character.IsBusy() || !paddock.IsForSale)
                return -1;

            if (!MapPaddock.IsGuildBoss(character))
            {
                character.SendServerMessage("Seul le meneur d'une guilde peut acheter un enclos.");
                return -1;
            }

            if (!paddock.CanBuy(character))
                return -1;

            new PaddockBuySell(character, paddock, false, (ulong)paddock.Price, InteractiveObject).Open();
            return base.StartExecute(character);
        }
    }

    /// <summary>"Vendre" : le meneur de la guilde proprietaire met l'enclos en vente.</summary>
    [Discriminator("PaddockSell", typeof(Skill), typeof(int), typeof(InteractiveCustomSkillRecord), typeof(InteractiveObject))]
    public class SkillPaddockSell : SkillPaddockBase
    {
        public SkillPaddockSell(int id, InteractiveCustomSkillRecord record, InteractiveObject interactiveObject)
            : base(id, record, interactiveObject)
        {
        }

        protected override bool IsAvailable(MapPaddock paddock, Character character) => paddock.IsOwnerBoss(character) && !paddock.OnSale;

        public override int StartExecute(Character character)
        {
            var paddock = GetPaddock(character);
            if (paddock == null || character.IsBusy() || !IsAvailable(paddock, character))
                return -1;

            new PaddockBuySell(character, paddock, true, (ulong)paddock.Price, InteractiveObject).Open();
            return base.StartExecute(character);
        }
    }

    /// <summary>"Modifier le prix de vente" (ou annuler la vente) : le meneur de la guilde proprietaire, enclos deja en vente.</summary>
    [Discriminator("PaddockPrice", typeof(Skill), typeof(int), typeof(InteractiveCustomSkillRecord), typeof(InteractiveObject))]
    public class SkillPaddockPrice : SkillPaddockBase
    {
        public SkillPaddockPrice(int id, InteractiveCustomSkillRecord record, InteractiveObject interactiveObject)
            : base(id, record, interactiveObject)
        {
        }

        protected override bool IsAvailable(MapPaddock paddock, Character character) => paddock.IsOwnerBoss(character) && paddock.OnSale;

        public override int StartExecute(Character character)
        {
            var paddock = GetPaddock(character);
            if (paddock == null || character.IsBusy() || !IsAvailable(paddock, character))
                return -1;

            new PaddockBuySell(character, paddock, true, (ulong)paddock.Price, InteractiveObject).Open();
            return base.StartExecute(character);
        }
    }

    /// <summary>
    /// NEKFEU : porte d'enclos exterieure, "Acceder" -> teleporte dans le hub (Parameter0 = map du hub, Parameter1 = cellule)
    /// et retient la porte pour donner ensuite les 4 enclos de cette porte.
    /// </summary>
    [Discriminator("PaddockEnter", typeof(Skill), typeof(int), typeof(InteractiveCustomSkillRecord), typeof(InteractiveObject))]
    public class SkillPaddockEnter : CustomSkill
    {
        public SkillPaddockEnter(int id, InteractiveCustomSkillRecord record, InteractiveObject interactiveObject)
            : base(id, record, interactiveObject)
        {
        }

        public override bool AreConditionsFilled(Character character)
            => PaddockManager.Instance.GetEntrancePaddocks(InteractiveObject.Map.Id).Length > 0 && base.AreConditionsFilled(character);

        public override int StartExecute(Character character)
        {
            if (character.IsBusy() || !AreConditionsFilled(character))
                return -1;

            var hub = int.TryParse(Record.Parameter0, out var hubId) ? World.Instance.GetMap(hubId) : null;
            if (hub == null)
            {
                character.SendServerMessage("Cette destination n'est pas disponible pour le moment.");
                return -1;
            }

            short.TryParse(Record.Parameter1, out var cellId);
            PaddockManager.Instance.EnterHub(character, character.Map, character.Cell.Id);
            character.Teleport(hub, PaddockManager.GetArrivalCell(hub, cellId));
            return base.StartExecute(character);
        }
    }

    /// <summary>
    /// NEKFEU : passage du hub. Parameter0 = emplacement 1-4 (0 = sortie vers la porte d'entree), Parameter1 = map, Parameter2 = cellule.
    /// Le passage marche au clic et quand on marche sur sa case.
    /// </summary>
    [Discriminator("PaddockHub", typeof(Skill), typeof(int), typeof(InteractiveCustomSkillRecord), typeof(InteractiveObject))]
    public class SkillPaddockHub : CustomSkill
    {
        public SkillPaddockHub(int id, InteractiveCustomSkillRecord record, InteractiveObject interactiveObject)
            : base(id, record, interactiveObject)
        {
            var map = interactiveObject.Map;
            var cell = interactiveObject.Cell;
            if (map != null && cell != null && cell.Walkable && !map.GetTriggers(cell).Any())
            {
                map.AddTrigger(new PaddockHubTrigger(new CellTriggerRecord
                {
                    Type = "PaddockHub",
                    CellId = cell.Id,
                    MapId = map.Id,
                    TriggerType = CellTriggerType.END_MOVE_ON,
                    Parameter0 = record.Parameter0,
                    Parameter1 = record.Parameter1,
                    Parameter2 = record.Parameter2
                }));
            }
        }

        public override int StartExecute(Character character)
        {
            if (character.IsBusy())
                return -1;

            if (!PaddockHubTrigger.Use(character, Record.Parameter0, Record.Parameter1, Record.Parameter2))
                return -1;

            return base.StartExecute(character);
        }
    }

    /// <summary>NEKFEU : meme chose qu'un passage du hub, sur une case (world_maps_triggers Type "PaddockHub" possible aussi).</summary>
    [Discriminator("PaddockHub", typeof(CellTrigger), typeof(CellTriggerRecord))]
    public class PaddockHubTrigger : CellTrigger
    {
        public PaddockHubTrigger(CellTriggerRecord record)
            : base(record)
        {
        }

        public override void Apply(Character character) => Use(character, Record.Parameter0, Record.Parameter1, Record.Parameter2);

        public static bool Use(Character character, string slotParam, string mapParam, string cellParam)
        {
            int.TryParse(slotParam, out var slot);
            var map = int.TryParse(mapParam, out var mapId) ? World.Instance.GetMap(mapId) : null;
            short.TryParse(cellParam, out var cellId);

            if (slot <= 0)
            {
                PaddockManager.Instance.LeaveHub(character, map, cellId);
                return true;
            }

            if (map == null)
                return false;

            if (PaddockManager.Instance.GetEntranceMap(character) == null)
            {
                character.SendServerMessage("Ressortez et entrez par la porte d'un enclos pour accéder à ses enclos.");
                return false;
            }

            character.Teleport(map, PaddockManager.GetArrivalCell(map, cellId));
            return true;
        }
    }
}
