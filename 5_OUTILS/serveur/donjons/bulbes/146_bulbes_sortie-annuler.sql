-- PADGREF 23/10 annulation du SQL 146
UPDATE monsters_spawns_dungeons SET TeleportMapId = 990000008, TeleportCell = -1 WHERE Id = 50;
DELETE FROM npcs_replies WHERE Id IN (902413, 902414);
DELETE FROM dungeons_exits WHERE DungeonId = 10 AND EndMapId = 17566977;
