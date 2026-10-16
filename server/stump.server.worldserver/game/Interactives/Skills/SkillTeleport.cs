using System;
using Stump.DofusProtocol.Enums;
using Stump.Server.BaseServer.Database;
using Stump.Server.WorldServer.Database.Interactives;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Maps.Cells;
using Stump.Server.WorldServer.Game.Maps.Cells.Triggers;
using Stump.Server.WorldServer.Database.World.Triggers;
using System.Collections.Generic;
using Stump.Server.WorldServer.Game.Items.Player;
using System.Linq;
using Stump.Server.WorldServer.Game.Items;

namespace Stump.Server.WorldServer.Game.Interactives.Skills
{
    [Discriminator("Teleport", typeof(Skill), typeof(int), typeof(InteractiveCustomSkillRecord), typeof(InteractiveObject))]
    public class SkillTeleport : CustomSkill
    {
        static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        bool m_mustRefreshPosition;
        ObjectPosition m_position;

        public SkillTeleport(int id, InteractiveCustomSkillRecord record, InteractiveObject interactiveObject)
            : base (id, record, interactiveObject)
        {
            AddWalkOnTrigger(record, interactiveObject);
        }

        /// <summary>
        /// In the 2.x client most doors are not clickable : the player walks into the doorway.
        /// The door cell therefore also gets a walk-on trigger with the same destination,
        /// unless a trigger stored in world_maps_triggers already handles that cell.
        /// </summary>
        static void AddWalkOnTrigger(InteractiveCustomSkillRecord record, InteractiveObject interactiveObject)
        {
            var map = interactiveObject.Map;
            var cell = interactiveObject.Cell;

            if (map == null || cell == null || !cell.Walkable)
                return;

            // NEKFEU : plusieurs destinations (ex. portail Eliotrope : 2 choix) -> pas de passage automatique en marchant dessus,
            // le joueur choisit dans le menu de l'interactif
            if (interactiveObject.Spawn.GetSkills().OfType<InteractiveCustomSkillRecord>().Count(x => x.Type == "Teleport") > 1)
                return;

            if (CellTriggerManager.Instance.GetOneCellTrigger(x => x.MapId == map.Id && x.CellId == cell.Id) != null)
                return;

            if (map.GetTriggers(cell).Any())
                return;

            var triggerRecord = new CellTriggerRecord
            {
                Type = "Teleport",
                CellId = cell.Id,
                MapId = map.Id,
                TriggerType = CellTriggerType.END_MOVE_ON,
                Condition = record.Condition,
                Parameter0 = record.Parameter1,
                Parameter1 = record.Parameter0
            };

            map.AddTrigger(triggerRecord.GenerateTrigger());
        }

        public override int StartExecute(Character character)
        {
            if (!Record.AreConditionsFilled(character))
            {
                character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 1);
                return -1;
            }

            var position = GetPosition();
            if (position == null)
            {
                character.SendServerMessage("Cette destination n'est pas disponible pour le moment.");
                return -1;
            }

            if (ItemsParameter != null)
            {
                var parameter = ItemsParameter.Split(',');
                var itemsToDelete = new Dictionary<BasePlayerItem, int>();

                foreach (var itemParameter in parameter.Select(x => x.Split('_')))
                {
                    int itemId;
                    int amount;

                    if (!int.TryParse(itemParameter[0], out itemId))
                        return -1;

                    if (!int.TryParse(itemParameter[1], out amount))
                        return -1;

                    var template = ItemManager.Instance.TryGetTemplate(itemId);
                    if (template == null)
                        return -1;

                    var item = character.Inventory.TryGetItem(template);

                    if (item == null)
                    {
                        //Vous ne poss�dez pas l'objet n�cessaire.
                        character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 4);
                        return -1;
                    }

                    if (item.Stack < amount)
                    {
                        //Vous ne poss�dez pas l'objet en quantit� suffisante.
                        character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 252);
                        return -1;
                    }

                    itemsToDelete.Add(item, amount);
                }

                foreach (var itemToDelete in itemsToDelete)
                {
                    character.Inventory.RemoveItem(itemToDelete.Key, itemToDelete.Value);
                }
            }

            var ok = character.Teleport(position);
            logger.Info($"[Interactif] SkillTeleport {Record.Id} : {character.Name} -> map {position.Map?.Id} case {position.Cell?.Id} : {(ok ? "OK" : "ECHEC")}");
            return base.StartExecute(character);
        }

        void RefreshPosition()
        {
            var map = World.Instance.GetMap(MapId);

            if (map == null || CellId >= map.Cells.Length)
            {
                logger.Warn($"SkillTeleport id={Id} (record {Record.Id}) points to an unknown destination: map {MapId} cell {CellId}");
                m_position = null;
                return;
            }

            // NEKFEU : case -1 (centre) ou case du retour (porte, declencheur) -> case libre la plus proche
            m_position = new ObjectPosition(map, map.GetArrivalCell(CellId), Direction);
        }

        public ObjectPosition GetPosition()
        {
            if (m_position == null || m_mustRefreshPosition)
                RefreshPosition();

            m_mustRefreshPosition = false;

            return m_position;
        }

        public int MapId => Record.GetParameter<int>(0);

        public int CellId => Record.GetParameter<int>(1);

        public DirectionsEnum Direction => (DirectionsEnum)Record.GetParameter<int>(2, true);

        public string ItemsParameter => Record.GetParameter<string>(3, true);
    }
}