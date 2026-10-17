-- NEKFEU : supprime le Malléfisk en double du groupe de départ (spawn 180)
DELETE FROM monsters_spawns_dungeons_groups WHERE Id BETWEEN 23328 AND 23332 AND DungeonSpawnId = 180;
