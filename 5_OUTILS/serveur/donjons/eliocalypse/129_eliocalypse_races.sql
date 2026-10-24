-- PADGREF 23/10/2026 : SQL 129 — Eliocalypse : races des monstres (Asservis, Miséreux, Sanguinaires, Corrompus) et super-race
-- « Créatures de l'Eliocalypse » : présentes en local mais jamais envoyées au VPS -> invocation du Germinion (Nheur'Gueule) en
-- échec côté serveur. Base world. Rejouable. Lu au démarrage du World.
SET NAMES utf8mb4;
REPLACE INTO monsters_superraces (Id, NameId) VALUES (38, 9020084);
REPLACE INTO monsters_races (Id, Name, SuperRaceId, NameId) VALUES
(215, 'Asservis', 38, 9020080),
(216, 'Miséreux', 38, 9020081),
(217, 'Sanguinaires', 38, 9020082),
(218, 'Corrompus', 38, 9020083);
INSERT IGNORE INTO langs (Id, French) VALUES (9020080, 'Asservis'), (9020081, 'Miséreux'), (9020082, 'Sanguinaires'), (9020083, 'Corrompus'),
(9020084, 'Créatures de l''Eliocalypse');
