-- annule ajouter_vagues_dimensions.sql
DELETE FROM monsters_spawns_dungeons_waves WHERE MonsterName = 'NEKFEU vagues';
-- groupe d'origine du spawn 449 : restaurer depuis la sauvegarde de monsters_spawns_dungeons_groups
DELETE FROM monsters_spawns_dungeons_groups WHERE DungeonSpawnId = 449 AND Informations = 'NEKFEU vagues';
