-- NEKFEU 2026-10-16 : retours de la recette 3 (World ARRETE). Annulation : recette3-annuler.sql
-- Lorkos devant le labyrinthe = map 152849 (case 176) : clef du Labyrinthe du Minotoror (7924) -> salle du Minotoror
INSERT INTO dungeons_entries (Id, DungeonId, EntranceMapId, KeyItemId, FirstRoomMapId, FirstRoomCellId, KeeperCellId) VALUES (82, 3, 152849, 7924, 34473474, -1, 176);
INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES (900503, 783, 'Talk', 'Mp=152849', -1, '3204');
-- Salle du Minotot : clef de la Salle du Minotot (8307)
UPDATE dungeons_entries SET KeyItemId = 8307 WHERE Id = 40;
-- Cour Sombre : une seule salle de combat (137102336, spawn 391) = Reine des Voleurs + vagues, puis sortie directe devant le donjon
DELETE FROM monsters_spawns_dungeons_groups WHERE DungeonSpawnId = 391;
UPDATE monsters_spawns_dungeons_groups SET DungeonSpawnId = 391 WHERE DungeonSpawnId = 449;
UPDATE monsters_spawns_dungeons_waves SET DungeonSpawnId = 391 WHERE DungeonSpawnId = 449;
UPDATE monsters_spawns_dungeons SET TeleportMapId = 136056074, TeleportCell = 411 WHERE Id = 391;
DELETE FROM monsters_spawns_dungeons WHERE Id = 449;
