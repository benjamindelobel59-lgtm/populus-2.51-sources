-- PADGREF 23/10 : annule 124 (gardien du Trône de Sang dehors, 201591042)
START TRANSACTION;
UPDATE npcs_spawns SET MapId = 201591042, CellId = 273 WHERE Id = 900656;
UPDATE dungeons_entries SET EntranceMapId = 201591042, KeeperCellId = 273 WHERE Id = 120;
UPDATE dungeons_exits SET ExitMapId = 201591042, ExitCellId = 259 WHERE Id = 102;
UPDATE dungeons SET EntranceMapId = 201591042, ExitMapId = 201591042 WHERE Id = 120;
UPDATE npcs_replies SET Parameter0 = '201591042', Parameter1 = '259' WHERE Id IN (901123, 901125);
COMMIT;
