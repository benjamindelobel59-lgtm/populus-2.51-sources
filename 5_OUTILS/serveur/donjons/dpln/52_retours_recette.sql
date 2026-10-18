-- MYSTIC 18/10 : retours de la recette (audits). Annulation : restaurer les tables depuis
-- SAVE/1_BASE_DE_DONNEES/2026-10-18_15h50_world_avant-52/tables.sql

-- Darkli Moon grade 1 : Obscure Singerie au niveau 2 (sort actif à 3 PA) ; au niveau 1 (réaction à 1 PA) l'IA la lançait 12 fois par tour
UPDATE monsters_spells SET Level = 2 WHERE Id = 2699 AND SpellId = 920;

-- Plateau d'Ush : un Chargus de trop à chaque palier (boss + 3 pour 4 joueurs, +1 par joueur)
DELETE FROM monsters_spawns_dungeons_groups WHERE Id IN (27402, 27406, 27411, 27417) AND DungeonSpawnId = 294;

-- Kitsounes : Tanukouï San + 3 Kitsous pour 4 joueurs, +1 par joueur (composition JOL à 8 : 2 Nufeu, 2 Nere, 2 Nae, 1 Nakwa)
DELETE FROM monsters_spawns_dungeons_groups WHERE DungeonSpawnId = 511;
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId,MonsterGradeId,MinPartyMembers,Informations) VALUES
(511,1948,1,'MYSTIC kitsou boss'),(511,1808,1,'MYSTIC kitsou boss'),(511,1798,1,'MYSTIC kitsou boss'),(511,1803,1,'MYSTIC kitsou boss'),
(511,1949,5,'MYSTIC kitsou boss'),(511,1808,5,'MYSTIC kitsou boss'),(511,1798,5,'MYSTIC kitsou boss'),(511,1803,5,'MYSTIC kitsou boss'),(511,1768,5,'MYSTIC kitsou boss'),
(511,1950,6,'MYSTIC kitsou boss'),(511,1808,6,'MYSTIC kitsou boss'),(511,1798,6,'MYSTIC kitsou boss'),(511,1803,6,'MYSTIC kitsou boss'),(511,1768,6,'MYSTIC kitsou boss'),(511,1808,6,'MYSTIC kitsou boss'),
(511,1951,7,'MYSTIC kitsou boss'),(511,1808,7,'MYSTIC kitsou boss'),(511,1798,7,'MYSTIC kitsou boss'),(511,1803,7,'MYSTIC kitsou boss'),(511,1768,7,'MYSTIC kitsou boss'),(511,1808,7,'MYSTIC kitsou boss'),(511,1798,7,'MYSTIC kitsou boss'),
(511,1952,8,'MYSTIC kitsou boss'),(511,1808,8,'MYSTIC kitsou boss'),(511,1798,8,'MYSTIC kitsou boss'),(511,1803,8,'MYSTIC kitsou boss'),(511,1768,8,'MYSTIC kitsou boss'),(511,1808,8,'MYSTIC kitsou boss'),(511,1798,8,'MYSTIC kitsou boss'),(511,1803,8,'MYSTIC kitsou boss');

-- Roi Nidas : groupe de départ du site (Nidas, Kamastérisk, Barbétoal, Lévitrof ; ajouts par joueur)
DELETE FROM monsters_spawns_dungeons_groups WHERE DungeonSpawnId = 390;
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId,MonsterGradeId,MinPartyMembers,Informations) VALUES
(390,9621,1,'MYSTIC nidas site'),(390,9650,1,'MYSTIC nidas site'),(390,9655,1,'MYSTIC nidas site'),(390,9660,1,'MYSTIC nidas site'),
(390,9622,5,'MYSTIC nidas site'),(390,9650,5,'MYSTIC nidas site'),(390,9655,5,'MYSTIC nidas site'),(390,9660,5,'MYSTIC nidas site'),(390,9665,5,'MYSTIC nidas site'),
(390,9623,6,'MYSTIC nidas site'),(390,9650,6,'MYSTIC nidas site'),(390,9655,6,'MYSTIC nidas site'),(390,9660,6,'MYSTIC nidas site'),(390,9665,6,'MYSTIC nidas site'),(390,9670,6,'MYSTIC nidas site'),
(390,9624,7,'MYSTIC nidas site'),(390,9650,7,'MYSTIC nidas site'),(390,9655,7,'MYSTIC nidas site'),(390,9660,7,'MYSTIC nidas site'),(390,9665,7,'MYSTIC nidas site'),(390,9670,7,'MYSTIC nidas site'),(390,9650,7,'MYSTIC nidas site'),
(390,9625,8,'MYSTIC nidas site'),(390,9650,8,'MYSTIC nidas site'),(390,9655,8,'MYSTIC nidas site'),(390,9660,8,'MYSTIC nidas site'),(390,9665,8,'MYSTIC nidas site'),(390,9670,8,'MYSTIC nidas site'),(390,9650,8,'MYSTIC nidas site'),(390,9660,8,'MYSTIC nidas site');
