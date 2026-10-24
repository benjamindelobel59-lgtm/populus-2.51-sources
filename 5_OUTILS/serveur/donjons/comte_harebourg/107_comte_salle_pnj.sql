-- PADGREF 23/10 (retour utilisateur) : donjon du Comte Harebourg. La carte 112202241 n'est PAS la salle 2 : c'est seulement la
-- salle du PNJ des duos de boss (visible quand on a deja fait le Comte). Elle avait un groupe de « remplissage » (spawn 381,
-- ajoute du temps de Nekfeu) entre l'Atelier d'horlogerie et la Salle de Banquet.
-- -> l'Atelier d'horlogerie (salle 1, spawn 380) mene directement a la Salle de Banquet (112199169) ; spawn 381 et ses monstres retires.
-- Ordre officiel : Atelier d'horlogerie -> Salle de Banquet -> Galerie des glacons -> Grande aiguille (Comte).
UPDATE monsters_spawns_dungeons SET TeleportMapId = 112199169, Descripcion = 'Donjon du Comte Harebourg - Atelier d''horlogerie - Salle de Banquet' WHERE Id = 380;
DELETE FROM monsters_spawns_dungeons_groups WHERE DungeonSpawnId = 381;
DELETE FROM monsters_spawns_dungeons WHERE Id = 381;
SELECT Id, MapId, TeleportMapId FROM monsters_spawns_dungeons WHERE Id IN (380, 381, 382);
