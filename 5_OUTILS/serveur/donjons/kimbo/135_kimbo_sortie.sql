-- PADGREF 23/10/2026 : SQL 135 (retour utilisateur : « le PNJ de fin du Kimbo ne propose pas de sortir du donjon »).
-- Canopée du Kimbo (donjon 24), salle de fin 21498882 :
--  1) Chachka (946) n'avait que son dialogue d'ENTRÉE (message 4206). Officiel 2.51 : dans la salle de fin, message 4208 (« J'ai perdu
--     mon pari… ») avec « Quitter la Canopée du Kimbo » (réponse 3686) -> action Talk 4208 si carte 21498882 (priorité -1 = avant 4206).
--  2) Sortie : carte d'entrée 25693696, case 199 (choix utilisateur), aussi pour la réponse 3686 et pour « Sortir du donjon » (dungeons_exits).
-- Rejouable.
USE `world`;
DELETE FROM npcs_actions WHERE Id = 907102;
INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES (907102, 946, 'Talk', 'Mp=21498882', -1, '4208');
UPDATE npcs_replies SET Parameter1 = '199' WHERE ReplyId = 3686 AND MessageId = 4208 AND Type = 'Teleport';
DELETE FROM dungeons_exits WHERE EndMapId = 21498882;
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (24, 21498882, 25693696, 199, 1);
