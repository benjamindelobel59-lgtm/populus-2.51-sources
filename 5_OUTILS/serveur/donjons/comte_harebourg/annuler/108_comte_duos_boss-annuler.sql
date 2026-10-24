-- PADGREF 23/10 : ANNULE 108 (duos de boss du Comte Harebourg)
DELETE FROM monsters_spawns_dungeons_groups WHERE DungeonSpawnId BETWEEN 97001 AND 97004;
DELETE FROM monsters_spawns_dungeons WHERE Id BETWEEN 97001 AND 97004;
DELETE FROM npcs_replies WHERE Id BETWEEN 907001 AND 907007;
DELETE FROM npcs_actions WHERE Id = 907001;
DELETE FROM npcs_spawns WHERE Id = 907001;
