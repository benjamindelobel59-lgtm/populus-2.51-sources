using System.Linq;
using Stump.DofusProtocol.Enums;
using Stump.Server.BaseServer.Commands;
using Stump.Server.WorldServer.Commands.Trigger;
using Stump.Server.WorldServer.Database.Monsters;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Monsters;
using Stump.Server.WorldServer.Game.Maps;

namespace Stump.Server.WorldServer.Commands.Commands
{
    /// <summary>
    /// Monsters that should not appear in a zone, managed from the map where they are :
    ///   .mobs liste            monsters that can spawn here (sub area and map) and groups present
    ///   .mobs retirer <id>     removes the monster from the sub area / map spawns (saved) and its groups in the zone
    ///   .mobs remettre <id>    puts it back
    /// </summary>
    public class MobsCommands : SubCommandContainer
    {
        public MobsCommands()
        {
            Aliases = new[] { "mobs" };
            Description = "Monstres d'une zone : liste, retirer, remettre";
            RequiredRole = RoleEnum.Administrator;
        }

        internal static Character GetCharacter(TriggerBase trigger)
        {
            var character = (trigger as GameTrigger)?.Character;
            if (character == null)
                trigger.ReplyError("Cette commande doit être utilisée en jeu.");
            return character;
        }

        // spawns of the zone : those of the sub area and those of the map itself
        internal static MonsterSpawn[] GetZoneSpawns(Map map, bool disabled) =>
            MonsterManager.Instance.GetMonsterSpawns()
                .Where(x => x.IsDisabled == disabled && (x.MapId == map.Id || (x.MapId == null || x.MapId == 0) && x.SubAreaId == map.SubArea?.Id))
                .ToArray();

        internal static string MonsterName(int id) => MonsterManager.Instance.GetTemplate(id)?.Name ?? "?";

        internal static void SetDisabled(MonsterSpawn spawn, bool disabled)
        {
            spawn.IsDisabled = disabled;
            WorldServer.Instance.IOTaskPool.ExecuteInContext(() =>
                WorldServer.Instance.DBAccessor.Database.Execute("UPDATE monsters_spawns SET IsDisabled=@0 WHERE Id=@1", disabled ? 1 : 0, spawn.Id));
        }
    }

    public class MobsListCommand : SubCommand
    {
        public MobsListCommand()
        {
            Aliases = new[] { "liste", "list" };
            Description = "Monstres qui peuvent apparaître sur cette map";
            RequiredRole = RoleEnum.Administrator;
            ParentCommandType = typeof(MobsCommands);
        }

        public override void Execute(TriggerBase trigger)
        {
            var character = MobsCommands.GetCharacter(trigger);
            if (character == null)
                return;

            var map = character.Map;
            var spawns = MobsCommands.GetZoneSpawns(map, false);
            trigger.ReplyBold("Map {0}, sous-zone {1} : {2} monstre(s) peuvent apparaître.", map.Id, map.SubArea?.Id, spawns.Length);

            foreach (var spawn in spawns.OrderBy(x => MobsCommands.MonsterName(x.MonsterId)))
                trigger.Reply("  {0} - {1} ({2}, fréquence {3})", spawn.MonsterId, MobsCommands.MonsterName(spawn.MonsterId),
                    spawn.MapId == map.Id ? "cette map" : "toute la sous-zone", spawn.Frequency);

            var groups = map.GetActors<MonsterGroup>().ToArray();
            if (groups.Length > 0)
            {
                trigger.ReplyBold("Groupes présents sur la map :");
                foreach (var group in groups)
                    trigger.Reply("  " + string.Join(", ", group.GetMonsters().Select(x => $"{x.Template.Id} {x.Template.Name}")));
            }

            var disabled = MobsCommands.GetZoneSpawns(map, true);
            if (disabled.Length > 0)
                trigger.Reply("Retirés de cette zone : " + string.Join(", ", disabled.Select(x => $"{x.MonsterId} {MobsCommands.MonsterName(x.MonsterId)}").Distinct()));
        }
    }

    public class MobsRemoveCommand : SubCommand
    {
        public MobsRemoveCommand()
        {
            Aliases = new[] { "retirer", "remove" };
            Description = "Retire un monstre de la zone (id du monstre, voir .mobs liste)";
            RequiredRole = RoleEnum.Administrator;
            ParentCommandType = typeof(MobsCommands);
            AddParameter<int>("monster", "m", "Id du monstre");
        }

        public override void Execute(TriggerBase trigger)
        {
            var character = MobsCommands.GetCharacter(trigger);
            if (character == null)
                return;

            var monsterId = trigger.Get<int>("monster");
            var map = character.Map;
            var spawns = MobsCommands.GetZoneSpawns(map, false).Where(x => x.MonsterId == monsterId).ToArray();

            if (spawns.Length == 0)
            {
                trigger.ReplyError("Le monstre {0} n'apparaît pas dans cette zone (voir .mobs liste). S'il est dans un groupe fixe, utilise .monsterstatic.", monsterId);
                return;
            }

            foreach (var spawn in spawns)
            {
                MobsCommands.SetDisabled(spawn, true);
                if (spawn.MapId == map.Id)
                    map.RemoveMonsterSpawn(spawn);
                else
                    map.SubArea?.RemoveMonsterSpawn(spawn);
            }

            // groups already on the maps of the zone
            var maps = map.SubArea?.Maps.ToArray() ?? new[] { map };
            var removed = 0;
            foreach (var zoneMap in maps)
            {
                foreach (var group in zoneMap.GetActors<MonsterGroup>().Where(g => g.GetMonsters().Any(x => x.Template.Id == monsterId)).ToArray())
                {
                    zoneMap.Leave(group);
                    removed++;
                }
            }

            trigger.ReplyBold("{0} ({1}) retiré de la zone : {2} groupe(s) supprimé(s). Pour annuler : .mobs remettre {1}",
                MobsCommands.MonsterName(monsterId), monsterId, removed);
        }
    }

    public class MobsRestoreCommand : SubCommand
    {
        public MobsRestoreCommand()
        {
            Aliases = new[] { "remettre", "restore" };
            Description = "Remet un monstre retiré de cette zone";
            RequiredRole = RoleEnum.Administrator;
            ParentCommandType = typeof(MobsCommands);
            AddParameter<int>("monster", "m", "Id du monstre");
        }

        public override void Execute(TriggerBase trigger)
        {
            var character = MobsCommands.GetCharacter(trigger);
            if (character == null)
                return;

            var monsterId = trigger.Get<int>("monster");
            var map = character.Map;
            var spawns = MobsCommands.GetZoneSpawns(map, true).Where(x => x.MonsterId == monsterId).ToArray();

            if (spawns.Length == 0)
            {
                trigger.ReplyError("Le monstre {0} n'a pas été retiré de cette zone.", monsterId);
                return;
            }

            foreach (var spawn in spawns)
            {
                MobsCommands.SetDisabled(spawn, false);
                if (spawn.MapId == map.Id)
                    map.AddMonsterSpawn(spawn);
                else
                    map.SubArea?.AddMonsterSpawn(spawn);
            }

            trigger.ReplyBold("{0} ({1}) remis dans la zone, il réapparaîtra aux prochains spawns.", MobsCommands.MonsterName(monsterId), monsterId);
        }
    }
}
