-- MYSTIC 17/10 : Antichambre des Gloursons (donjon 57) — entrée chez Willy La Bille (PNJ 1394, [-63,-75]) comme sur l'officiel.
-- « Donner la clef » (réponse 8476) demande la Clef de l'Antichambre des Gloursons (11181) et mène à la 2e salle (1re = passage) ;
-- « Utiliser le trousseau de clefs » (8477) reste visible seulement si le trousseau a encore cette clef.
-- Avant : « Donner la clef » téléportait sans clef, et le donjon n'était pas dans dungeons_entries (pas de clef ni de trousseau). Rejouable.
DELETE FROM dungeons_entries WHERE DungeonId=57;
INSERT INTO dungeons_entries (DungeonId, EntranceMapId, KeyItemId, FirstRoomMapId, FirstRoomCellId, KeeperCellId) VALUES
 (57, 62129672, 11181, 62131720, 534, 317),
 (57, 62128648, 11181, 62131720, 534, 260);
-- dialogue de Willy (message 8459) : plus de téléportation sans clef ni de « Téléporter les membres du groupe »
DELETE FROM npcs_replies WHERE MessageId=8459 AND ((Type='Teleport' AND ReplyId=8476) OR Type='DungeonEnter');
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnter', 8476, 8459);
DELETE FROM npcs_replies WHERE MessageId=8459 AND Type='DungeonEnterTrousseau';
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnterTrousseau', 8477, 8459);
