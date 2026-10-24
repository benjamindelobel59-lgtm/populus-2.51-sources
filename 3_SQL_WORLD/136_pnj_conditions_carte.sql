-- PADGREF 23/10/2026 : SQL 136 (log VPS 18:11 : « Cannot parse 134221826 : No operator found » -> le joueur est DÉCONNECTÉ en parlant
-- à un PNJ). Une condition d'action de PNJ réduite à un numéro de carte (créée sur le VPS) n'est pas lisible par le serveur : elle
-- devient « Mp=<carte> » (le PNJ ne parle que sur cette carte). Les anciennes valeurs sont gardées dans npcs_actions_conditions_136.
USE `world`;
CREATE TABLE IF NOT EXISTS npcs_actions_conditions_136 (Id INT PRIMARY KEY, AncienneCondition VARCHAR(255));
INSERT IGNORE INTO npcs_actions_conditions_136 SELECT Id, `Condition` FROM npcs_actions WHERE `Condition` REGEXP '^[0-9]+$';
UPDATE npcs_actions SET `Condition` = CONCAT('Mp=', `Condition`) WHERE `Condition` REGEXP '^[0-9]+$';
SELECT a.Id, a.NpcId, a.`Condition` FROM npcs_actions a JOIN npcs_actions_conditions_136 c ON c.Id = a.Id;
