-- MYSTIC 19/10/2026 : SQL 77 — maison-atelier d'Astrub [5,-17] : c'est la PORTE VERTE (élément 515307, case 201, compétence 101238)
-- que l'utilisateur voulait (elle menait à 192675846) : elle mène maintenant à l'atelier 191890437 (case 248, à côté de la sortie).
-- La sortie de l'atelier (case 234) ramène devant la porte verte (191105028 case 215) au lieu de la maison 785.
-- Base world, World ARRÊTÉ. Rejouable.
SET NAMES utf8mb4;
USE `world`;
START TRANSACTION;
UPDATE interactives_skills SET Parameter0 = '191890437', Parameter1 = '248' WHERE Id = 101238;
DELETE FROM world_maps_triggers WHERE MapId = 191890437 AND CellId = 234;
INSERT INTO world_maps_triggers (Type, CellId, MapId, TriggerType, Parameter0, Parameter1) VALUES ('Teleport', 234, 191890437, 0, 215, 191105028);
COMMIT;
SELECT Id, Type, Parameter0, Parameter1 FROM interactives_skills WHERE Id = 101238;
SELECT Id, Type, CellId, Parameter0, Parameter1 FROM world_maps_triggers WHERE MapId = 191890437;
