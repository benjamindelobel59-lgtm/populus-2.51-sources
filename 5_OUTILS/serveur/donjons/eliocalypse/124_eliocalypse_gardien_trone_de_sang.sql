-- PADGREF 23/10/2026 : SQL 124 — Trône de Sang : le gardien Kapelache est déplacé là où pointe l'icône du donjon sur la carte du
-- monde (indice 2.74) : carte intérieure 201591809 [4,-3] (on y entre par la porte de 201592065), au lieu de l'extérieur 201591042
-- [4,-2] (demande utilisateur). Entrée, sortie et retours du gardien de sortie suivent. Base world. Rejouable.
START TRANSACTION;
UPDATE npcs_spawns SET MapId = 201591809, CellId = 273 WHERE Id = 900656;
UPDATE dungeons_entries SET EntranceMapId = 201591809, KeeperCellId = 273 WHERE Id = 120;
UPDATE dungeons_exits SET ExitMapId = 201591809, ExitCellId = 259 WHERE Id = 102;
UPDATE dungeons SET EntranceMapId = 201591809, ExitMapId = 201591809 WHERE Id = 120;
UPDATE npcs_replies SET Parameter0 = '201591809', Parameter1 = '259' WHERE Id IN (901123, 901125);
COMMIT;
