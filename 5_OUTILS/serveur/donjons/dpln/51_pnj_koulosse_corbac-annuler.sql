UPDATE npcs_spawns SET CellId = 109 WHERE Id = 10348 AND MapId = 139784;
UPDATE npcs_actions SET `Condition` = 'ex.mp=158334976' WHERE Id = 204429 AND NpcId = 774;
INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Parameter0) VALUES (204430, 774, 'Talk', 'ex.mp=158334976', '3181');
