-- Annulation du SQL 161 (on entre de nouveau directement dans la salle du Minotoror)
DELETE FROM monsters_spawns_dungeons_groups WHERE DungeonSpawnId BETWEEN 900180 AND 900189;
DELETE FROM monsters_spawns_dungeons WHERE Id BETWEEN 900180 AND 900189;
UPDATE dungeons SET FirstRoom = 34473474, MapsIdsCSV = '34473476,34473474' WHERE Id = 3;
UPDATE dungeons_entries SET FirstRoomMapId = 34473474, FirstRoomCellId = -1 WHERE DungeonId = 3;
