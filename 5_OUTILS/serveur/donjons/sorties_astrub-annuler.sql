-- annulation de sorties_astrub.sql
DELETE FROM dungeons_exits WHERE EndMapId = 34473476 AND DungeonId = 4;
DELETE FROM npcs_spawns WHERE Id = 900300;
UPDATE monsters_spawns_dungeons SET TeleportMapId = 191105026, TeleportCell = 273 WHERE Id = 208;
DELETE FROM dungeons_exits WHERE EndMapId = 176952320 AND DungeonId = 35;
DELETE FROM npcs_spawns WHERE Id = 900301;
UPDATE monsters_spawns_dungeons SET TeleportMapId = 191105026, TeleportCell = 260 WHERE Id = 114;
DELETE FROM dungeons_exits WHERE EndMapId = 136579072 AND DungeonId = 81;
DELETE FROM npcs_spawns WHERE Id = 900302;
UPDATE monsters_spawns_dungeons SET TeleportMapId = 191105026, TeleportCell = NULL WHERE Id = 245;
UPDATE monsters_spawns_dungeons SET TeleportMapId = 191105026, TeleportCell = 273 WHERE Id = 322;
