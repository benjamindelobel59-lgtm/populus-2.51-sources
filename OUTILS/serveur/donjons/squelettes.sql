-- NEKFEU 2026-10-16 : Donjon des Squelettes (43, « donjon Chafer ») inaccessible.
-- Le gardien Klik Klak (1163) dehors (map 82314497, case 287) n'avait pas de dialogue : son action Parler etait limitee a Mp=87032320
-- et l'entree de donjon n'etait declaree que pour 87032320. On ajoute les deux pour 82314497 (meme message 7070 : cle 1570 / groupe).
DELETE FROM npcs_actions WHERE NpcId=1163 AND Type='Talk' AND `Condition`='Mp=82314497';
INSERT INTO npcs_actions (NpcId, Type, `Condition`, Priority, Parameter0) VALUES (1163, 'Talk', 'Mp=82314497', 0, '7070');
DELETE FROM dungeons_entries WHERE DungeonId=43 AND EntranceMapId=82314497;
INSERT INTO dungeons_entries (DungeonId, EntranceMapId, KeyItemId, FirstRoomMapId, FirstRoomCellId, KeeperCellId)
 SELECT DungeonId, 82314497, KeyItemId, FirstRoomMapId, FirstRoomCellId, 287 FROM dungeons_entries WHERE Id=35;
