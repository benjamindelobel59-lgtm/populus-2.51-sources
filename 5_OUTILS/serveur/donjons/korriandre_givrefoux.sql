-- MYSTIC 17/10 : Antre du Korriandre (donjon 55) et Tanière Givrefoux (donjon 54). Rejouable.

-- ===== Antre du Korriandre : entrée chez Sylvie Barbe (PNJ 1391, [-73,-69], carte 60036612, cellule 299) =====
-- « Donner la clef » (8449) prend la Clef de l'Antre du Korriandre (11179) et mène à la 1re salle (62915584, cellule 157) ;
-- « Utiliser le trousseau de clefs » (8450) visible seulement si le trousseau a encore la clef.
-- Avant : le donjon n'était pas dans dungeons_entries (trousseau inutilisable) et « Téléporter les membres du groupe » (11147) en trop.
DELETE FROM dungeons_entries WHERE DungeonId=55;
INSERT INTO dungeons_entries (DungeonId, EntranceMapId, KeyItemId, FirstRoomMapId, FirstRoomCellId, KeeperCellId) VALUES
 (55, 60036612, 11179, 62915584, 157, 299);
DELETE FROM npcs_replies WHERE MessageId=8427 AND (Type IN ('Teleport','DungeonEnter','DungeonEnterTrousseau') AND ReplyId IN (8449,8450,11147));
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnter', 8449, 8427), ('DungeonEnterTrousseau', 8450, 8427);

-- ===== Tanière Givrefoux : Givrihaltès (PNJ 1385) =====
-- Ses dialogues visaient d'anciens numéros de carte (152067...) : muet en [-80,-75] et à l'entrée [-76,-75],
-- et ses téléportations allaient vers une carte inexistante (191105026).
DELETE FROM npcs_actions WHERE NpcId=1385 AND Type='Talk' AND `Condition` IN ('Mp=54174027','Mp=59506688','Mp=59510784');
INSERT INTO npcs_actions (NpcId, Type, `Condition`, Priority, Parameter0) VALUES
 (1385, 'Talk', 'Mp=54174027', 0, '8293'),   -- surface [-80,-75] : « Je te laisse entrer dans la caverne... »
 (1385, 'Talk', 'Mp=59506688', 0, '8294'),   -- cavernes [-80,-75] : « Tu veux retourner à la surface ? »
 (1385, 'Talk', 'Mp=59510784', 0, '8296');   -- entrée du donjon [-76,-75] : « Donne-moi la clef... »
DELETE FROM npcs_replies WHERE MessageId IN (8293,8294,8296);
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0, Parameter1, Parameter2) VALUES
 ('Teleport', 8324, 8293, '59506688', '369', '7'),   -- Descendre dans les cavernes
 ('Teleport', 8325, 8294, '54174027', '340', '1');   -- Sortir des cavernes
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES
 ('DungeonEnter', 8347, 8296),                       -- Donner la clef (Clef de la Tanière Givrefoux 11178)
 ('DungeonEnterTrousseau', 8348, 8296);              -- Utiliser le trousseau de clefs
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0, Parameter1, Parameter2) VALUES
 ('Teleport', 8326, 8296, '59506688', '369', '7');   -- Retourner à l'entrée des cavernes
-- « Tu veux partir ? » (8295) : bon libellé « Retourner à l'entrée des cavernes » au lieu de « Vérifier la présence de Fuji »
UPDATE npcs_replies SET ReplyId=8326 WHERE MessageId=8295 AND Type='Teleport' AND ReplyId=9077;

DELETE FROM dungeons_entries WHERE DungeonId=54;
INSERT INTO dungeons_entries (DungeonId, EntranceMapId, KeyItemId, FirstRoomMapId, FirstRoomCellId, KeeperCellId) VALUES
 (54, 59510784, 11178, 59511808, 396, 343);

-- ===== Tanière Givrefoux : les 2 salles de boss (choix chez Givrihaltès, carte 59515138) =====
-- Avant : « Rendre hommage à Tengu » menait à la salle Fuji SANS Tengu, « Déranger Tengu et Fuji » à la salle Tengu SANS Fuji.
-- Salle Tengu (59515904, Tengu + Kami) : « Rendre hommage à Tengu » (9092 → 13776 → 15928).
-- Salle Tengu + Fuji (59516162) : « Déranger Tengu et Fuji » (9093 → 13777 → 15930) ; le Kami y est remplacé par Tengu
-- (Fuji prend la place du Kami), et au palier 1 (sans Kami) Tengu est ajouté.
UPDATE npcs_replies SET Parameter0='59515904', Parameter1='424' WHERE MessageId=13776 AND Type='Teleport' AND ReplyId=15928;
UPDATE npcs_replies SET Parameter0='59516162', Parameter1='451' WHERE MessageId=13777 AND Type='Teleport' AND ReplyId=15930;
DELETE FROM monsters_spawns_dungeons_groups WHERE DungeonSpawnId=301 AND (MonsterGradeId=6972 OR MonsterGradeId BETWEEN 6906 AND 6910);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES
 (301, 6906, 1, 'MYSTIC Tengu salle Tengu+Fuji'),
 (301, 6907, 5, 'MYSTIC Tengu salle Tengu+Fuji'),
 (301, 6908, 6, 'MYSTIC Tengu salle Tengu+Fuji'),
 (301, 6909, 7, 'MYSTIC Tengu salle Tengu+Fuji'),
 (301, 6910, 8, 'MYSTIC Tengu salle Tengu+Fuji');
