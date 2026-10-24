-- PADGREF 23/10/2026 : annule 135_kimbo_sortie.sql (retour à l'état d'avant : sortie case 170, réponse 3686 case 213)
USE `world`;
DELETE FROM npcs_actions WHERE Id = 907102;
UPDATE npcs_replies SET Parameter1 = '213' WHERE ReplyId = 3686 AND MessageId = 4208 AND Type = 'Teleport';
DELETE FROM dungeons_exits WHERE EndMapId = 21498882;
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (28, 21498882, 25693696, 170, 1);
