-- annule lorkos_tofu.sql (World ARRETE)
DELETE FROM npcs_actions WHERE Id IN (900500, 900501, 900502);
DELETE FROM npcs_replies WHERE Id IN (900500, 900501, 900502, 900503, 900504, 900505, 900506);
UPDATE dungeons_entries SET KeyItemId = 7924 WHERE Id = 40;
UPDATE dungeons_exits SET DungeonId = 4 WHERE Id = 81;
INSERT INTO npcs_spawns (Id, NpcId, MapId, CellId, Direction) VALUES (900300, 10058, 34473476, 300, 1);
UPDATE dungeons_exits SET ExitMapId = 34476294, ExitCellId = 286 WHERE Id = 19;
UPDATE npcs_spawns SET CellId = 340 WHERE Id = 10701;
DELETE FROM npcs_spawns WHERE Id = 900500;
UPDATE dungeons_entries SET KeeperCellId = 340 WHERE Id IN (36, 37);
UPDATE npcs_actions SET Parameter0 = '17101', Parameter1 = NULL WHERE Id IN (900432, 900434);
