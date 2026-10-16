using System.Collections.Concurrent;
using System.Linq;
using Stump.DofusProtocol.Enums;
using Stump.Server.BaseServer.Commands;
using Stump.Server.WorldServer.Commands.Trigger;
using Stump.Server.WorldServer.Database.World.Triggers;
using Stump.Server.WorldServer.Game;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Maps;
using Stump.Server.WorldServer.Game.Maps.Cells;
using Stump.Server.WorldServer.Game.Maps.Cells.Triggers;

namespace Stump.Server.WorldServer.Commands.Commands
{
    /// <summary>
    /// In-game helper to create building doors (cell triggers) without typing map and cell ids.
    ///   .porte debut     on the outside door cell
    ///   .porte fin       inside, where the player must appear (creates the entrance)
    ///   .porte sortie    inside, on the exit cell (creates the exit, back in front of the door)
    ///   .porte supprimer removes the door trigger on the current cell
    ///   .porte info      shows the triggers of the current map
    /// </summary>
    public class DoorCommands : SubCommandContainer
    {
        internal static readonly ConcurrentDictionary<int, DoorStart> PendingDoors = new ConcurrentDictionary<int, DoorStart>();

        public DoorCommands()
        {
            Aliases = new[] { "porte", "door" };
            Description = "Créer les portes des bâtiments (debut, fin, sortie, supprimer, info)";
            RequiredRole = RoleEnum.Administrator;
        }

        internal static Character GetCharacter(TriggerBase trigger)
        {
            var character = (trigger as GameTrigger)?.Character;
            if (character == null)
                trigger.ReplyError("Cette commande doit être utilisée en jeu.");
            return character;
        }

        internal static void CreateTeleportTrigger(Map fromMap, short fromCell, Map toMap, short toCell)
        {
            if (fromMap.GetTriggers(fromMap.Cells[fromCell]).Any())
                CellTriggerManager.Instance.DeleteCellTrigger(fromMap.Id, fromCell);

            CellTriggerManager.Instance.AddCellTrigger(new CellTriggerRecord
            {
                CellId = fromCell,
                MapId = fromMap.Id,
                Type = "Teleport",
                TriggerType = CellTriggerType.END_MOVE_ON,
                Parameter0 = toCell.ToString(),
                Parameter1 = toMap.Id.ToString()
            });
        }

        internal sealed class DoorStart
        {
            public int MapId;
            public short CellId;
        }
    }

    public class DoorStartCommand : SubCommand
    {
        public DoorStartCommand()
        {
            Aliases = new[] { "debut", "start" };
            Description = "A taper sur la case de la porte, à l'extérieur";
            RequiredRole = RoleEnum.Administrator;
            ParentCommandType = typeof(DoorCommands);
        }

        public override void Execute(TriggerBase trigger)
        {
            var character = DoorCommands.GetCharacter(trigger);
            if (character == null)
                return;

            DoorCommands.PendingDoors[character.Id] = new DoorCommands.DoorStart { MapId = character.Map.Id, CellId = character.Cell.Id };

            trigger.ReplyBold("Porte enregistrée : map {0}, case {1}.", character.Map.Id, character.Cell.Id);
            trigger.Reply("Va à l'intérieur, place-toi où le joueur doit apparaître, puis tape .porte fin");
        }
    }

    public class DoorEndCommand : SubCommand
    {
        public DoorEndCommand()
        {
            Aliases = new[] { "fin", "end" };
            Description = "A taper à l'intérieur, à l'endroit où le joueur doit arriver";
            RequiredRole = RoleEnum.Administrator;
            ParentCommandType = typeof(DoorCommands);
        }

        public override void Execute(TriggerBase trigger)
        {
            var character = DoorCommands.GetCharacter(trigger);
            if (character == null)
                return;

            if (!DoorCommands.PendingDoors.TryGetValue(character.Id, out var start))
            {
                trigger.ReplyError("Tape d'abord .porte debut sur la case de la porte.");
                return;
            }

            var outside = World.Instance.GetMap(start.MapId);
            var inside = character.Map;
            var arrival = character.Cell.Id;

            if (outside.Id == inside.Id && start.CellId == arrival)
            {
                trigger.ReplyError("Tu es encore sur la case de la porte : entre dans le bâtiment d'abord.");
                return;
            }

            WorldServer.Instance.IOTaskPool.ExecuteInContext(() =>
            {
                DoorCommands.CreateTeleportTrigger(outside, start.CellId, inside, arrival);
                trigger.ReplyBold("Entrée créée : map {0} case {1} -> map {2} case {3}.", outside.Id, start.CellId, inside.Id, arrival);
                trigger.Reply("Pour la sortie, place-toi sur la case de sortie de l'intérieur et tape .porte sortie");
            });
        }
    }

    public class DoorExitCommand : SubCommand
    {
        public DoorExitCommand()
        {
            Aliases = new[] { "sortie", "exit" };
            Description = "A taper à l'intérieur, sur la case qui doit faire ressortir";
            RequiredRole = RoleEnum.Administrator;
            ParentCommandType = typeof(DoorCommands);
        }

        public override void Execute(TriggerBase trigger)
        {
            var character = DoorCommands.GetCharacter(trigger);
            if (character == null)
                return;

            if (!DoorCommands.PendingDoors.TryGetValue(character.Id, out var start))
            {
                trigger.ReplyError("Aucune porte en cours : tape d'abord .porte debut dehors.");
                return;
            }

            var outside = World.Instance.GetMap(start.MapId);
            var inside = character.Map;
            var exitCell = character.Cell.Id;

            // Arrive next to the door, never on it, otherwise the player would walk straight back in.
            var landing = MapPoint.GetPoint(start.CellId)
                .GetAdjacentCells(cellId => cellId != start.CellId && outside.Cells[cellId].Walkable && !outside.GetTriggers(outside.Cells[cellId]).Any(), true)
                .Select(p => p.CellId)
                .DefaultIfEmpty((short)-1)
                .First();

            if (landing < 0)
            {
                trigger.ReplyError("Impossible de trouver une case libre à côté de la porte (map {0}, case {1}).", outside.Id, start.CellId);
                return;
            }

            WorldServer.Instance.IOTaskPool.ExecuteInContext(() =>
            {
                DoorCommands.CreateTeleportTrigger(inside, exitCell, outside, landing);
                trigger.ReplyBold("Sortie créée : map {0} case {1} -> map {2} case {3}.", inside.Id, exitCell, outside.Id, landing);
            });
        }
    }

    public class DoorRemoveCommand : SubCommand
    {
        public DoorRemoveCommand()
        {
            Aliases = new[] { "supprimer", "remove" };
            Description = "Supprime le trigger de la case actuelle";
            RequiredRole = RoleEnum.Administrator;
            ParentCommandType = typeof(DoorCommands);
        }

        public override void Execute(TriggerBase trigger)
        {
            var character = DoorCommands.GetCharacter(trigger);
            if (character == null)
                return;

            var map = character.Map;
            var cell = character.Cell;

            if (!map.GetTriggers(cell).Any())
            {
                trigger.ReplyError("Aucun trigger sur cette case ({0}).", cell.Id);
                return;
            }

            WorldServer.Instance.IOTaskPool.ExecuteInContext(() =>
            {
                CellTriggerManager.Instance.DeleteCellTrigger(map.Id, cell.Id);
                trigger.ReplyBold("Trigger supprimé : map {0} case {1}.", map.Id, cell.Id);
            });
        }
    }

    public class DoorHouseExitCommand : SubCommand
    {
        public DoorHouseExitCommand()
        {
            Aliases = new[] { "sortiemaison", "houseexit" };
            Description = "Dans une maison : la case actuelle devient la sortie (ramène devant la porte d'entrée)";
            RequiredRole = RoleEnum.Administrator;
            ParentCommandType = typeof(DoorCommands);
        }

        public override void Execute(TriggerBase trigger)
        {
            var character = DoorCommands.GetCharacter(trigger);
            if (character == null)
                return;

            var map = character.Map;
            if (Game.Houses.HouseManager.Instance.GetHouseByInterior(map.Id) == null)
            {
                trigger.ReplyError("Cette map n'est l'intérieur d'aucune maison.");
                return;
            }

            var cellId = character.Cell.Id;
            WorldServer.Instance.IOTaskPool.ExecuteInContext(() =>
            {
                if (map.GetTriggers(map.Cells[cellId]).Any())
                    CellTriggerManager.Instance.DeleteCellTrigger(map.Id, cellId);

                CellTriggerManager.Instance.AddCellTrigger(new CellTriggerRecord
                {
                    CellId = cellId,
                    MapId = map.Id,
                    Type = "HouseExit",
                    TriggerType = CellTriggerType.END_MOVE_ON
                });
                trigger.ReplyBold("Sortie de maison créée sur la case {0} (map {1}).", cellId, map.Id);
            });
        }
    }

    public class DoorInfoCommand : SubCommand
    {
        public DoorInfoCommand()
        {
            Aliases = new[] { "info" };
            Description = "Liste les triggers de la map actuelle";
            RequiredRole = RoleEnum.Administrator;
            ParentCommandType = typeof(DoorCommands);
        }

        public override void Execute(TriggerBase trigger)
        {
            var character = DoorCommands.GetCharacter(trigger);
            if (character == null)
                return;

            var triggers = character.Map.GetTriggers().ToArray();
            trigger.ReplyBold("Map {0} : {1} trigger(s). Tu es sur la case {2}.", character.Map.Id, triggers.Length, character.Cell.Id);

            foreach (var cellTrigger in triggers.OfType<TeleportTrigger>())
                trigger.Reply("  case {0} -> map {1} case {2}", cellTrigger.Position.Cell.Id, cellTrigger.DestinationMapId, cellTrigger.DestinationCellId);

            if (DoorCommands.PendingDoors.TryGetValue(character.Id, out var start))
                trigger.Reply("Porte en cours : map {0} case {1}", start.MapId, start.CellId);
        }
    }
}
