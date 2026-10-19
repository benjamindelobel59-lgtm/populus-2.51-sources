-- MYSTIC 19/10/2026 : SQL 75 — Agent de la compagnie Asfog et Fils à Sakaï (carte 54161193) : aucune action « Parler » (dialogue vide).
-- Son dialogue officiel (message 15195 : « … liaison entre Frigost et Sakaï … jusqu'aux champs de glace », réponse 395 -> Champs de
-- glace 54167842, sans condition) était rattaché à la carte 17047552, absente de cette version. Base world, World ARRÊTÉ. Rejouable.
SET NAMES utf8mb4;
USE `world`;
START TRANSACTION;
UPDATE npcs_actions SET `Condition` = 'Mp=17047552|Mp=54161193' WHERE Id = 12 AND NpcId = 1286;
COMMIT;
SELECT Id, NpcId, Type, `Condition`, Parameter0 FROM npcs_actions WHERE NpcId = 1286 ORDER BY Id;
