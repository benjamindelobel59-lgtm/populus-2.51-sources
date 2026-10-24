-- PADGREF 23/10/2026 : SQL 130 (retour utilisateur : « sur la map 186646528 il n'y a pas le PNJ qui nous ramène… »).
-- Mer Kantil, pont du bateau (186646528) : le Marin frigostien (PNJ 4176, objectif de la quête « Toucher le fond » sur cette carte,
-- client 2.51) manquait. Dialogue officiel (message 29533) : « S'installer dans la cloche » (35461) -> fond de la Mer Kantil
-- (186384904, Épaves Silencieuses, à côté de la cloche qui remonte, case 394) ; « Rester sur le pont du bateau » (35462) -> fin.
-- Rejouable. Annulation : 130_marin_frigostien-annuler.sql
USE `world`;
DELETE FROM npcs_replies WHERE Id IN (907101,907102);
DELETE FROM npcs_actions WHERE Id = 907101;
DELETE FROM npcs_spawns WHERE Id = 907101;
INSERT INTO npcs_spawns (Id, NpcId, MapId, CellId, Direction) VALUES (907101, 4176, 186646528, 315, 3);
INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES (907101, 4176, 'Talk', NULL, 0, '29533');
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId, Criteria, Parameter0, Parameter1, Parameter2) VALUES
 (907101, 'Teleport', 35461, 29533, NULL, '186384904', '380', '1'),
 (907102, 'EndDialog', 35462, 29533, NULL, NULL, NULL, NULL);
