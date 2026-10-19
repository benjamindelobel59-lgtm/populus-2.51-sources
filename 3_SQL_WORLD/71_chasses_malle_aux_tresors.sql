-- MYSTIC 19/10/2026 : SQL 71 — chasses au trésor : interactifs de la Malle aux Trésors (Champs de Cania [-25,-36], carte 128452097).
-- Base world, World ARRÊTÉ. Rejouable. À passer APRÈS le SQL 70.
-- Attribution des 3 interactifs (graphismes uniques, non confirmée : À VÉRIFIER EN JEU ; si les noms sont inversés, échanger les TemplateId) :
--   484993 (case 290) = Distributeur de missions (231) : « Chercher un trésor (niveau 20 … 200) » (compétences 248 à 257)
--   485497 (case 340) = Table de planification (237)   : « Chercher un trésor légendaire » (277)
--   485495 (case 398) = Pupitre (236)                  : « Assembler une carte » (275, fabrication des cartes légendaires)
SET NAMES utf8mb4;
USE `world`;
START TRANSACTION;

UPDATE interactives_spawns SET TemplateId = 231 WHERE Id = 484993;
UPDATE interactives_spawns SET TemplateId = 237 WHERE Id = 485497;
UPDATE interactives_spawns SET TemplateId = 236 WHERE Id = 485495;

DELETE FROM interactives_spawns_skills WHERE InteractiveSpawnId IN (484993, 485497, 485495);
DELETE FROM interactives_skills WHERE Id BETWEEN 5100001 AND 5100020;

-- Distributeur : une compétence par niveau (Parameter0 = niveau de la chasse)
INSERT INTO interactives_skills (Id, Type, Duration, `Condition`, CustomTemplateId, Parameter0) VALUES
 (5100001, 'ChasseTresor', 0, NULL, 248, '20'),
 (5100002, 'ChasseTresor', 0, NULL, 249, '40'),
 (5100003, 'ChasseTresor', 0, NULL, 250, '60'),
 (5100004, 'ChasseTresor', 0, NULL, 251, '80'),
 (5100005, 'ChasseTresor', 0, NULL, 252, '100'),
 (5100006, 'ChasseTresor', 0, NULL, 253, '120'),
 (5100007, 'ChasseTresor', 0, NULL, 254, '140'),
 (5100008, 'ChasseTresor', 0, NULL, 255, '160'),
 (5100009, 'ChasseTresor', 0, NULL, 256, '180'),
 (5100010, 'ChasseTresor', 0, NULL, 257, '200'),
 (5100011, 'ChasseLegendaire', 0, NULL, 277, NULL),
 (5100012, 'Template', 0, NULL, 275, NULL);

INSERT INTO interactives_spawns_skills (InteractiveSpawnId, SkillId) VALUES
 (484993, 5100001), (484993, 5100002), (484993, 5100003), (484993, 5100004), (484993, 5100005),
 (484993, 5100006), (484993, 5100007), (484993, 5100008), (484993, 5100009), (484993, 5100010),
 (485497, 5100011),
 (485495, 5100012);

COMMIT;
SELECT s.InteractiveSpawnId, k.Id, k.Type, k.CustomTemplateId, k.Parameter0 FROM interactives_spawns_skills s JOIN interactives_skills k ON k.Id = s.SkillId
WHERE s.InteractiveSpawnId IN (484993, 485497, 485495) ORDER BY k.Id;
