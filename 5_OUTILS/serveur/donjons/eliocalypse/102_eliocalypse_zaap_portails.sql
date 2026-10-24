-- PADGREF 23/10/2026 : SQL 102 — Eliocalypse : zaap de l'Arche de Vili + portails des 4 zones. Base world. Rejouable.
-- Le zaap (élément 517965, case 344) ne propose QUE les 4 salles des portails, gratuitement (code : ZaapDialog.SallesPortails,
-- DLL du même paquet). Chaque portail ramène à l'Arche de Vili.
SET NAMES utf8mb4;
START TRANSACTION;
DELETE FROM interactives_spawns_skills WHERE Id BETWEEN 5100200 AND 5100210;
DELETE FROM interactives_skills WHERE Id BETWEEN 5100200 AND 5100210;
DELETE FROM interactives_spawns WHERE Id IN (517965, 517982, 517984, 517950, 517980);
INSERT INTO interactives_spawns (Id, TemplateId, MapId, CellId, ElementId, Animated, Template) VALUES
(517965, 16, 202899464, 344, 52319, 0, 'Zaap'),
(517982, 0, 201588736, 438, 93909, 0, NULL),
(517984, 0, 201850882, 340, 93909, 0, NULL),
(517950, 0, 201326592, 312, 93886, 0, NULL),
(517980, 0, 202246150, 273, 93909, 0, NULL);
INSERT INTO interactives_skills (Id, Type, Duration, `Condition`, CustomTemplateId, Parameter0, Parameter1, Parameter2) VALUES
(5100201, 'Teleport', 0, NULL, NULL, 202899464, 259, 1);
INSERT INTO interactives_spawns_skills (Id, InteractiveSpawnId, SkillId) VALUES
(5100200, 517965, 1),
(5100201, 517982, 5100201),
(5100202, 517984, 5100201),
(5100203, 517950, 5100201),
(5100204, 517980, 5100201);
COMMIT;
