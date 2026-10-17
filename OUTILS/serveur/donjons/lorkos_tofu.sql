-- NEKFEU 2026-10-16 : Lorkos (labyrinthe du Minotoror / salle du Minotot) + gardiens Tofus / Tofu Royal.
-- A appliquer World ARRETE, avec le serveur compile « lorkos-tofu » (Talk : Parameter1 = case du PNJ ; DungeonEnter : KeeperCellId).
-- Annulation : lorkos_tofu-annuler.sql

-- ===== Lorkos =====
-- Devant le labyrinthe (34473220) : message 3204 + « Accepter et utiliser la clef du labyrinthe. » -> 1re salle du Minotoror
INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES (900500, 783, 'Talk', 'Mp=34473220', -1, '3204');
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) VALUES (900500, 'DungeonEnter', 2838, 3204);

-- Apres le Minotoror (34473476) : message 3210 + « Affronter le Minotot. » (sans clef) ou « Sortir. »
INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES (900501, 783, 'Talk', 'Mp=34473476', -1, '3210');
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) VALUES (900501, 'DungeonEnter', 2841, 3210);
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) VALUES (900502, 'DungeonExit', 2835, 3210);
UPDATE dungeons_entries SET KeyItemId = 0 WHERE Id = 40;           -- salle du Minotot : pas de 2e clef (la clef du labyrinthe est deja consommee)
UPDATE dungeons_exits SET DungeonId = 3 WHERE Id = 81;              -- etait 4 (Dragon Cochon) par erreur, sans effet en jeu
DELETE FROM npcs_spawns WHERE Id = 900300;                          -- PNJ « Minotoror » 10058 : remplace par Lorkos (deja sur la case 281)

-- Apres le Minotot (34472452) : message 3213 + « Répondre que vous êtes prêt à sortir. » -> 152849 case 301
INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES (900502, 783, 'Talk', 'Mp=34472452', -1, '3213');
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) VALUES (900503, 'DungeonExit', 2843, 3213);
UPDATE dungeons_exits SET ExitMapId = 152849, ExitCellId = 301 WHERE Id = 19;

-- ===== Kazy Ingalsse (96338944) : un PNJ par donjon =====
-- case 302 : Donjon des Tofus (clef 7918), message 17102 « T'veux vraimin utiliser un'clef pour intrer ? » Oui / Non
UPDATE npcs_spawns SET CellId = 302 WHERE Id = 10701;
UPDATE dungeons_entries SET KeeperCellId = 302 WHERE Id = 36;
UPDATE npcs_actions SET Parameter0 = '17102', Parameter1 = '302' WHERE Id = 900432;
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) VALUES (900504, 'DungeonEnter', 20455, 17102);
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) VALUES (900505, 'EndDialog', 20456, 17102);
-- case 313 : Tofulailler Royal (clef 8142), message 3176 + « Donner la clef du Tofulailler Royal. »
INSERT INTO npcs_spawns (Id, NpcId, MapId, CellId, Direction) VALUES (900500, 778, 96338944, 313, 1);
UPDATE dungeons_entries SET KeeperCellId = 313 WHERE Id = 37;
UPDATE npcs_actions SET Parameter0 = '3176', Parameter1 = '313' WHERE Id = 900434;
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) VALUES (900506, 'DungeonEnter', 2798, 3176);
