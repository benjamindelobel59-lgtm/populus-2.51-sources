-- MYSTIC 18/10 : annulation de 46_gorgouille.sql
DELETE g FROM monsters_spawns_dungeons_groups g JOIN monsters_spawns_dungeons d ON d.Id=g.DungeonSpawnId WHERE d.Descripcion LIKE 'MYSTIC 46%';
DELETE FROM monsters_spawns_dungeons WHERE Descripcion LIKE 'MYSTIC 46%';
UPDATE monsters_spawns_dungeons SET TeleportMapId=72361984, TeleportCell=430 WHERE Id=159;
