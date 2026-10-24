-- Annulation du SQL 166 (retour aux salles du SQL 161)
UPDATE monsters_spawns_dungeons SET MapId = 136315650, TeleportMapId = 136315392 WHERE Id = 900180;
UPDATE monsters_spawns_dungeons SET MapId = 136315392, TeleportMapId = 136317698 WHERE Id = 900181;
UPDATE monsters_spawns_dungeons SET MapId = 136317698, TeleportMapId = 34473474 WHERE Id = 900182;
UPDATE dungeons SET FirstRoom = 136315650, MapsIdsCSV = '136315650,136315392,136317698,34473474,34473476' WHERE Id = 3;
UPDATE dungeons_entries SET FirstRoomMapId = 136315650, FirstRoomCellId = -1 WHERE DungeonId = 3;
