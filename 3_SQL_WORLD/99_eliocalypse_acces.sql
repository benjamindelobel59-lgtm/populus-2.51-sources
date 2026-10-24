-- PADGREF 23/10/2026 : SQL 99 — Eliocalypse : accès (passeurs Astrub / Arche de Vili / zones), gardiens officiels, entrées / sorties,
-- salles des 4 donjons (spawns 9700+), monstres de zone (9700001+). Généré par eliocalypse/acces_salles.py. Base world. Rejouable.
SET NAMES utf8mb4;
START TRANSACTION;
DELETE FROM npcs_templates WHERE Id IN (5325,5363,5367,5319,10070,10071);
DELETE FROM npcs_actions WHERE NpcId IN (5325,5363,5367,5319,10070,10071);
DELETE FROM npcs_spawns WHERE NpcId IN (5325,5363,5367,5319,10070,10071);
INSERT INTO npcs_templates (Id, Name, NameId, DialogMessagesIdCSV, DialogRepliesIdCSV, ActionsIdsCSV, Gender, LookAsString, SpecialArtworkId, TokenShop) VALUES (5325, 'Kapelache', 9040011, '39011,9040000;39012,9040001;39013,9040002;39014,9040003', '50798,9040004;50796,9040005;50793,9040006;50799,9040007;50794,9040008;50795,9040009;50791,9040008;50792,9040009;50797,9040010', '3', 0, '{5543}', 0, 0);
INSERT INTO npcs_templates (Id, Name, NameId, DialogMessagesIdCSV, DialogRepliesIdCSV, ActionsIdsCSV, Gender, LookAsString, SpecialArtworkId, TokenShop) VALUES (5363, 'Bree Caule', 9040014, '39334,9040012;39335,9040003;39336,9040002;39337,9040001', '51191,9040013;51194,9040005;51197,9040006;51198,9040007;51190,9040010;51192,9040008;51193,9040009;51195,9040008;51196,9040009', '3', 0, '{5608}', 0, 0);
INSERT INTO npcs_templates (Id, Name, NameId, DialogMessagesIdCSV, DialogRepliesIdCSV, ActionsIdsCSV, Gender, LookAsString, SpecialArtworkId, TokenShop) VALUES (5367, 'Rafflesia la Fétide', 9040020, '39057,9040015;39058,9040016;39059,9040017;39060,9040018', '50862,9040019;50860,9040005;50857,9040006;50863,9040007;50858,9040008;50859,9040009;50855,9040008;50856,9040009;50861,9040010', '3', 0, '{5603|||120}', 0, 0);
INSERT INTO npcs_templates (Id, Name, NameId, DialogMessagesIdCSV, DialogRepliesIdCSV, ActionsIdsCSV, Gender, LookAsString, SpecialArtworkId, TokenShop) VALUES (5319, 'Miradar', 9040025, '38913,9040021;38914,9040003;38915,9040002;38916,9040001;38917,9040022;38918,9040023', '51951,9040024;50661,9040005;50664,9040006;50665,9040007;50657,9040010;50659,9040008;50660,9040009;50662,9040008;50663,9040009', '3', 0, '{5524||1=13754578,2=7746112,3=5985394,4=5912128,5=4949131}', 0, 0);
INSERT INTO npcs_templates (Id, Name, NameId, DialogMessagesIdCSV, DialogRepliesIdCSV, ActionsIdsCSV, Gender, LookAsString, SpecialArtworkId, TokenShop) VALUES (10070, "Passeur de l'Eliocalypse", 9040026, '60020,9040027', '60030,9040030', '3', 0, '{1001|||90}', 0, 0);
INSERT INTO npcs_templates (Id, Name, NameId, DialogMessagesIdCSV, DialogRepliesIdCSV, ActionsIdsCSV, Gender, LookAsString, SpecialArtworkId, TokenShop) VALUES (10071, "Passeur de l'Eliocalypse", 9040026, '60021,9040028;60022,9040029', '60032,9040032;60033,9040033;60034,9040034;60035,9040035;60031,9040031;60030,9040030', '3', 0, '{1001|||90}', 0, 0);
DELETE FROM npcs_messages WHERE Id IN (38913,38914,38915,38916,38917,38918,39011,39012,39013,39014,39057,39058,39059,39060,39334,39335,39336,39337,60020,60021,60022);
DELETE FROM npcs_replies WHERE MessageId IN (38913,38914,38915,38916,38917,38918,39011,39012,39013,39014,39057,39058,39059,39060,39334,39335,39336,39337,60020,60021,60022);
INSERT INTO npcs_messages (Id, MessageId, ParametersCSV) VALUES (38913, 9040021, ''), (38914, 9040003, ''), (38915, 9040002, ''), (38916, 9040001, ''), (38917, 9040022, ''), (38918, 9040023, ''), (39011, 9040000, ''), (39012, 9040001, ''), (39013, 9040002, ''), (39014, 9040003, ''), (39057, 9040015, ''), (39058, 9040016, ''), (39059, 9040017, ''), (39060, 9040018, ''), (39334, 9040012, ''), (39335, 9040003, ''), (39336, 9040002, ''), (39337, 9040001, ''), (60020, 9040027, ''), (60021, 9040028, ''), (60022, 9040029, '');
INSERT INTO npcs_spawns (NpcId, MapId, CellId, Direction, Npc) VALUES (10070, 191105026, 315, 1, 'Padgref passeur Eliocalypse (Astrub)');
INSERT INTO npcs_actions (NpcId, Type, `Condition`, Priority, Parameter0) VALUES (10070, 'Talk', NULL, 0, '60020');
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0, Parameter1, Parameter2) VALUES ('Teleport', 60030, 60020, '202899464', '259', '1');
INSERT INTO npcs_spawns (NpcId, MapId, CellId, Direction, Npc) VALUES (10071, 202899464, 273, 1, 'Padgref passeur Eliocalypse (Arche de Vili)');
INSERT INTO npcs_actions (NpcId, Type, `Condition`, Priority, Parameter0) VALUES (10071, 'Talk', 'Mp=202899464', 1, '60021');
INSERT INTO npcs_actions (NpcId, Type, `Condition`, Priority, Parameter0) VALUES (10071, 'Talk', NULL, 0, '60022');
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0, Parameter1, Parameter2) VALUES ('Teleport', 60031, 60021, '191105026', '260', '1');
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0, Parameter1, Parameter2) VALUES ('Teleport', 60030, 60022, '202899464', '259', '1');
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0, Parameter1, Parameter2) VALUES ('Teleport', 60032, 60021, '201588736', '259', '1');
INSERT INTO npcs_spawns (NpcId, MapId, CellId, Direction, Npc) VALUES (10071, 201588736, 273, 1, 'Padgref passeur Eliocalypse (Blessures de Guerre)');
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0, Parameter1, Parameter2) VALUES ('Teleport', 60033, 60021, '201850880', '287', '1');
INSERT INTO npcs_spawns (NpcId, MapId, CellId, Direction, Npc) VALUES (10071, 201850880, 273, 1, 'Padgref passeur Eliocalypse (Désert de Misère)');
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0, Parameter1, Parameter2) VALUES ('Teleport', 60034, 60021, '201326592', '275', '1');
INSERT INTO npcs_spawns (NpcId, MapId, CellId, Direction, Npc) VALUES (10071, 201326592, 315, 1, 'Padgref passeur Eliocalypse (Royaume Corrompu)');
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0, Parameter1, Parameter2) VALUES ('Teleport', 60035, 60021, '202115594', '259', '1');
INSERT INTO npcs_spawns (NpcId, MapId, CellId, Direction, Npc) VALUES (10071, 202115594, 273, 1, 'Padgref passeur Eliocalypse (Galère de Servitude)');
DELETE FROM dungeons_entries WHERE DungeonId IN (120,119,117,118);
DELETE FROM dungeons_exits WHERE DungeonId IN (120,119,117,118);
DELETE FROM monsters_spawns_dungeons_groups WHERE DungeonSpawnId BETWEEN 9700 AND 9799;
DELETE FROM monsters_spawns_dungeons WHERE Id BETWEEN 9700 AND 9799;
INSERT INTO npcs_spawns (NpcId, MapId, CellId, Direction, Npc) VALUES (5325, 201591042, 273, 1, 'Padgref gardien Trône de Sang');
INSERT INTO npcs_actions (NpcId, Type, `Condition`, Priority, Parameter0) VALUES (5325, 'Talk', NULL, 0, '39011');
INSERT INTO npcs_spawns (NpcId, MapId, CellId, Direction, Npc) VALUES (5325, 202379266, 273, 1, 'Padgref gardien Trône de Sang (sortie)');
INSERT INTO npcs_actions (NpcId, Type, `Condition`, Priority, Parameter0) VALUES (5325, 'Talk', 'Mp=202379266', -1, '39014');
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0, Parameter1, Parameter2) VALUES ('DungeonResume', 50798, 39011, '120', NULL, NULL);
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0, Parameter1, Parameter2) VALUES ('DungeonEnter', 50796, 39011, NULL, NULL, NULL);
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0, Parameter1, Parameter2) VALUES ('DungeonEnterTrousseau', 50793, 39011, NULL, NULL, NULL);
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0, Parameter1, Parameter2) VALUES ('Teleport', 50794, 39014, '201591042', '259', '1');
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0, Parameter1, Parameter2) VALUES ('DungeonExit', 50794, 39014, NULL, NULL, NULL);
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0, Parameter1, Parameter2) VALUES ('Teleport', 50791, 39014, '201591042', '259', '1');
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0, Parameter1, Parameter2) VALUES ('DungeonExit', 50791, 39014, NULL, NULL, NULL);
INSERT INTO dungeons_entries (DungeonId, EntranceMapId, KeyItemId, FirstRoomMapId, FirstRoomCellId, KeeperCellId) VALUES (120, 201591042, 22147, 202375168, 186, 273);
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (120, 202379266, 201591042, 259, 1);
UPDATE dungeons SET EntranceMapId = 201591042, ExitMapId = 201591042, FirstRoom = 202375168 WHERE Id = 120;
INSERT INTO monsters_spawns_dungeons (Id, MapId, CellId, TeleportEvent, TeleportMapId, TeleportCell, TeleportDirection, Descripcion, RewardItemId, RewardMax) VALUES (9700, 202375168, NULL, 1, 202376192, 229, 7, 'Trône de Sang - Salle 1', NULL, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9700, 970112, 1, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9700, 970117, 1, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9700, 970127, 1, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9700, 970132, 1, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9700, 970122, 5, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9700, 970112, 6, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9700, 970117, 7, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9700, 970127, 8, NULL);
INSERT INTO monsters_spawns_dungeons (Id, MapId, CellId, TeleportEvent, TeleportMapId, TeleportCell, TeleportDirection, Descripcion, RewardItemId, RewardMax) VALUES (9701, 202376192, NULL, 1, 202377216, 296, 7, 'Trône de Sang - Salle 2', NULL, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9701, 970117, 1, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9701, 970127, 1, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9701, 970132, 1, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9701, 970112, 1, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9701, 970112, 5, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9701, 970117, 6, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9701, 970127, 7, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9701, 970132, 8, NULL);
INSERT INTO monsters_spawns_dungeons (Id, MapId, CellId, TeleportEvent, TeleportMapId, TeleportCell, TeleportDirection, Descripcion, RewardItemId, RewardMax) VALUES (9702, 202377216, NULL, 1, 202378240, 421, 7, 'Trône de Sang - Salle 3', NULL, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9702, 970127, 1, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9702, 970132, 1, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9702, 970112, 1, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9702, 970122, 1, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9702, 970117, 5, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9702, 970127, 6, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9702, 970132, 7, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9702, 970122, 8, NULL);
INSERT INTO monsters_spawns_dungeons (Id, MapId, CellId, TeleportEvent, TeleportMapId, TeleportCell, TeleportDirection, Descripcion, RewardItemId, RewardMax) VALUES (9703, 202378240, NULL, 1, 202379264, 393, 7, 'Trône de Sang - Salle 4', NULL, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9703, 970132, 1, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9703, 970112, 1, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9703, 970117, 1, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9703, 970122, 1, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9703, 970127, 5, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9703, 970132, 6, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9703, 970122, 7, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9703, 970112, 8, NULL);
INSERT INTO monsters_spawns_dungeons (Id, MapId, CellId, TeleportEvent, TeleportMapId, TeleportCell, TeleportDirection, Descripcion, RewardItemId, RewardMax) VALUES (9704, 202379264, NULL, 1, 202379266, 259, 7, 'Trône de Sang - Salle 5', NULL, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9704, 970100, 1, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9704, 970122, 1, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9704, 970112, 1, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9704, 970117, 1, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9704, 970132, 5, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9704, 970122, 6, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9704, 970112, 7, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9704, 970117, 8, NULL);
INSERT INTO npcs_spawns (NpcId, MapId, CellId, Direction, Npc) VALUES (5363, 201851392, 273, 1, 'Padgref gardien Sentence de la Balance');
INSERT INTO npcs_actions (NpcId, Type, `Condition`, Priority, Parameter0) VALUES (5363, 'Talk', NULL, 0, '39334');
INSERT INTO npcs_spawns (NpcId, MapId, CellId, Direction, Npc) VALUES (5363, 203427842, 273, 1, 'Padgref gardien Sentence de la Balance (sortie)');
INSERT INTO npcs_actions (NpcId, Type, `Condition`, Priority, Parameter0) VALUES (5363, 'Talk', 'Mp=203427842', -1, '39335');
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0, Parameter1, Parameter2) VALUES ('DungeonResume', 51191, 39334, '119', NULL, NULL);
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0, Parameter1, Parameter2) VALUES ('DungeonEnter', 51194, 39334, NULL, NULL, NULL);
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0, Parameter1, Parameter2) VALUES ('DungeonEnterTrousseau', 51197, 39334, NULL, NULL, NULL);
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0, Parameter1, Parameter2) VALUES ('Teleport', 51192, 39335, '201851392', '259', '1');
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0, Parameter1, Parameter2) VALUES ('DungeonExit', 51192, 39335, NULL, NULL, NULL);
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0, Parameter1, Parameter2) VALUES ('Teleport', 51195, 39335, '201851392', '259', '1');
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0, Parameter1, Parameter2) VALUES ('DungeonExit', 51195, 39335, NULL, NULL, NULL);
INSERT INTO dungeons_entries (DungeonId, EntranceMapId, KeyItemId, FirstRoomMapId, FirstRoomCellId, KeeperCellId) VALUES (119, 201851392, 22149, 203423744, 327, 273);
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (119, 203427842, 201851392, 259, 1);
UPDATE dungeons SET EntranceMapId = 201851392, ExitMapId = 201851392, FirstRoom = 203423744 WHERE Id = 119;
INSERT INTO monsters_spawns_dungeons (Id, MapId, CellId, TeleportEvent, TeleportMapId, TeleportCell, TeleportDirection, Descripcion, RewardItemId, RewardMax) VALUES (9705, 203423744, NULL, 1, 203424768, 202, 7, 'Sentence de la Balance - Salle 1', NULL, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9705, 970057, 1, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9705, 970062, 1, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9705, 970067, 1, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9705, 970072, 1, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9705, 970052, 5, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9705, 970057, 6, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9705, 970062, 7, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9705, 970067, 8, NULL);
INSERT INTO monsters_spawns_dungeons (Id, MapId, CellId, TeleportEvent, TeleportMapId, TeleportCell, TeleportDirection, Descripcion, RewardItemId, RewardMax) VALUES (9706, 203424768, NULL, 1, 203425792, 324, 7, 'Sentence de la Balance - Salle 2', NULL, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9706, 970062, 1, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9706, 970067, 1, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9706, 970072, 1, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9706, 970057, 1, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9706, 970057, 5, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9706, 970062, 6, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9706, 970067, 7, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9706, 970072, 8, NULL);
INSERT INTO monsters_spawns_dungeons (Id, MapId, CellId, TeleportEvent, TeleportMapId, TeleportCell, TeleportDirection, Descripcion, RewardItemId, RewardMax) VALUES (9707, 203425792, NULL, 1, 203426816, 276, 7, 'Sentence de la Balance - Salle 3', NULL, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9707, 970067, 1, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9707, 970072, 1, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9707, 970057, 1, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9707, 970052, 1, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9707, 970062, 5, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9707, 970067, 6, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9707, 970072, 7, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9707, 970052, 8, NULL);
INSERT INTO monsters_spawns_dungeons (Id, MapId, CellId, TeleportEvent, TeleportMapId, TeleportCell, TeleportDirection, Descripcion, RewardItemId, RewardMax) VALUES (9708, 203426816, NULL, 1, 203427840, 368, 7, 'Sentence de la Balance - Salle 4', NULL, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9708, 970072, 1, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9708, 970057, 1, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9708, 970062, 1, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9708, 970052, 1, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9708, 970067, 5, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9708, 970072, 6, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9708, 970052, 7, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9708, 970057, 8, NULL);
INSERT INTO monsters_spawns_dungeons (Id, MapId, CellId, TeleportEvent, TeleportMapId, TeleportCell, TeleportDirection, Descripcion, RewardItemId, RewardMax) VALUES (9709, 203427840, NULL, 1, 203427842, 259, 7, 'Sentence de la Balance - Salle 5', NULL, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9709, 970040, 1, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9709, 970052, 1, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9709, 970057, 1, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9709, 970062, 1, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9709, 970072, 5, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9709, 970052, 6, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9709, 970057, 7, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9709, 970062, 8, NULL);
INSERT INTO npcs_spawns (NpcId, MapId, CellId, Direction, Npc) VALUES (5367, 201328640, 413, 1, 'Padgref gardien Arbre de Mort');
INSERT INTO npcs_actions (NpcId, Type, `Condition`, Priority, Parameter0) VALUES (5367, 'Talk', NULL, 0, '39057');
INSERT INTO npcs_spawns (NpcId, MapId, CellId, Direction, Npc) VALUES (5367, 202642432, 273, 1, 'Padgref gardien Arbre de Mort (sortie)');
INSERT INTO npcs_actions (NpcId, Type, `Condition`, Priority, Parameter0) VALUES (5367, 'Talk', 'Mp=202642432', -1, '39060');
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0, Parameter1, Parameter2) VALUES ('DungeonResume', 50862, 39057, '117', NULL, NULL);
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0, Parameter1, Parameter2) VALUES ('DungeonEnter', 50860, 39057, NULL, NULL, NULL);
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0, Parameter1, Parameter2) VALUES ('DungeonEnterTrousseau', 50857, 39057, NULL, NULL, NULL);
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0, Parameter1, Parameter2) VALUES ('Teleport', 50858, 39060, '201328640', '282', '1');
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0, Parameter1, Parameter2) VALUES ('DungeonExit', 50858, 39060, NULL, NULL, NULL);
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0, Parameter1, Parameter2) VALUES ('Teleport', 50855, 39060, '201328640', '282', '1');
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0, Parameter1, Parameter2) VALUES ('DungeonExit', 50855, 39060, NULL, NULL, NULL);
INSERT INTO dungeons_entries (DungeonId, EntranceMapId, KeyItemId, FirstRoomMapId, FirstRoomCellId, KeeperCellId) VALUES (117, 201328640, 22148, 202637312, 339, 413);
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (117, 202642432, 201328640, 282, 1);
UPDATE dungeons SET EntranceMapId = 201328640, ExitMapId = 201328640, FirstRoom = 202637312 WHERE Id = 117;
INSERT INTO monsters_spawns_dungeons (Id, MapId, CellId, TeleportEvent, TeleportMapId, TeleportCell, TeleportDirection, Descripcion, RewardItemId, RewardMax) VALUES (9710, 202637312, NULL, 1, 202638336, 332, 7, 'Arbre de Mort - Salle 1', NULL, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9710, 970152, 1, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9710, 970157, 1, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9710, 970162, 1, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9710, 970167, 1, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9710, 970147, 5, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9710, 970152, 6, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9710, 970157, 7, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9710, 970162, 8, NULL);
INSERT INTO monsters_spawns_dungeons (Id, MapId, CellId, TeleportEvent, TeleportMapId, TeleportCell, TeleportDirection, Descripcion, RewardItemId, RewardMax) VALUES (9711, 202638336, NULL, 1, 202639360, 356, 7, 'Arbre de Mort - Salle 2', NULL, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9711, 970157, 1, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9711, 970162, 1, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9711, 970167, 1, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9711, 970152, 1, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9711, 970152, 5, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9711, 970157, 6, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9711, 970162, 7, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9711, 970167, 8, NULL);
INSERT INTO monsters_spawns_dungeons (Id, MapId, CellId, TeleportEvent, TeleportMapId, TeleportCell, TeleportDirection, Descripcion, RewardItemId, RewardMax) VALUES (9712, 202639360, NULL, 1, 202640384, 424, 7, 'Arbre de Mort - Salle 3', NULL, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9712, 970162, 1, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9712, 970167, 1, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9712, 970152, 1, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9712, 970147, 1, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9712, 970157, 5, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9712, 970162, 6, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9712, 970167, 7, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9712, 970147, 8, NULL);
INSERT INTO monsters_spawns_dungeons (Id, MapId, CellId, TeleportEvent, TeleportMapId, TeleportCell, TeleportDirection, Descripcion, RewardItemId, RewardMax) VALUES (9713, 202640384, NULL, 1, 202641408, 360, 7, 'Arbre de Mort - Salle 4', NULL, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9713, 970167, 1, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9713, 970152, 1, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9713, 970157, 1, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9713, 970147, 1, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9713, 970162, 5, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9713, 970167, 6, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9713, 970147, 7, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9713, 970152, 8, NULL);
INSERT INTO monsters_spawns_dungeons (Id, MapId, CellId, TeleportEvent, TeleportMapId, TeleportCell, TeleportDirection, Descripcion, RewardItemId, RewardMax) VALUES (9714, 202641408, NULL, 1, 202642432, 259, 7, 'Arbre de Mort - Salle 5', NULL, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9714, 970135, 1, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9714, 970147, 1, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9714, 970152, 1, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9714, 970157, 1, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9714, 970167, 5, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9714, 970147, 6, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9714, 970152, 7, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9714, 970157, 8, NULL);
INSERT INTO npcs_spawns (NpcId, MapId, CellId, Direction, Npc) VALUES (5319, 202115584, 245, 1, 'Padgref gardien Fers de la Tyrannie');
INSERT INTO npcs_actions (NpcId, Type, `Condition`, Priority, Parameter0) VALUES (5319, 'Talk', NULL, 0, '38913');
INSERT INTO npcs_spawns (NpcId, MapId, CellId, Direction, Npc) VALUES (5319, 203165698, 273, 1, 'Padgref gardien Fers de la Tyrannie (sortie)');
INSERT INTO npcs_actions (NpcId, Type, `Condition`, Priority, Parameter0) VALUES (5319, 'Talk', 'Mp=203165698', -1, '38914');
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0, Parameter1, Parameter2) VALUES ('DungeonResume', 51951, 38913, '118', NULL, NULL);
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0, Parameter1, Parameter2) VALUES ('DungeonEnter', 50661, 38913, NULL, NULL, NULL);
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0, Parameter1, Parameter2) VALUES ('DungeonEnterTrousseau', 50664, 38913, NULL, NULL, NULL);
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0, Parameter1, Parameter2) VALUES ('Teleport', 50659, 38914, '202115584', '274', '1');
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0, Parameter1, Parameter2) VALUES ('DungeonExit', 50659, 38914, NULL, NULL, NULL);
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0, Parameter1, Parameter2) VALUES ('Teleport', 50662, 38914, '202115584', '274', '1');
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0, Parameter1, Parameter2) VALUES ('DungeonExit', 50662, 38914, NULL, NULL, NULL);
INSERT INTO dungeons_entries (DungeonId, EntranceMapId, KeyItemId, FirstRoomMapId, FirstRoomCellId, KeeperCellId) VALUES (118, 202115584, 22028, 203161600, 369, 245);
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (118, 203165698, 202115584, 274, 1);
UPDATE dungeons SET EntranceMapId = 202115584, ExitMapId = 202115584, FirstRoom = 203161600 WHERE Id = 118;
INSERT INTO monsters_spawns_dungeons (Id, MapId, CellId, TeleportEvent, TeleportMapId, TeleportCell, TeleportDirection, Descripcion, RewardItemId, RewardMax) VALUES (9715, 203161600, NULL, 1, 203162624, 394, 7, 'Fers de la Tyrannie - Salle 1', NULL, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9715, 970012, 1, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9715, 970022, 1, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9715, 970032, 1, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9715, 970037, 1, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9715, 970012, 5, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9715, 970022, 6, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9715, 970032, 7, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9715, 970037, 8, NULL);
INSERT INTO monsters_spawns_dungeons (Id, MapId, CellId, TeleportEvent, TeleportMapId, TeleportCell, TeleportDirection, Descripcion, RewardItemId, RewardMax) VALUES (9716, 203162624, NULL, 1, 203163648, 341, 7, 'Fers de la Tyrannie - Salle 2', NULL, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9716, 970022, 1, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9716, 970032, 1, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9716, 970037, 1, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9716, 970012, 1, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9716, 970022, 5, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9716, 970032, 6, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9716, 970037, 7, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9716, 970012, 8, NULL);
INSERT INTO monsters_spawns_dungeons (Id, MapId, CellId, TeleportEvent, TeleportMapId, TeleportCell, TeleportDirection, Descripcion, RewardItemId, RewardMax) VALUES (9717, 203163648, NULL, 1, 203164672, 435, 7, 'Fers de la Tyrannie - Salle 3', NULL, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9717, 970032, 1, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9717, 970037, 1, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9717, 970012, 1, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9717, 970022, 1, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9717, 970032, 5, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9717, 970037, 6, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9717, 970012, 7, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9717, 970022, 8, NULL);
INSERT INTO monsters_spawns_dungeons (Id, MapId, CellId, TeleportEvent, TeleportMapId, TeleportCell, TeleportDirection, Descripcion, RewardItemId, RewardMax) VALUES (9718, 203164672, NULL, 1, 203165696, 263, 7, 'Fers de la Tyrannie - Salle 4', NULL, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9718, 970037, 1, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9718, 970012, 1, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9718, 970022, 1, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9718, 970032, 1, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9718, 970037, 5, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9718, 970012, 6, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9718, 970022, 7, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9718, 970032, 8, NULL);
INSERT INTO monsters_spawns_dungeons (Id, MapId, CellId, TeleportEvent, TeleportMapId, TeleportCell, TeleportDirection, Descripcion, RewardItemId, RewardMax) VALUES (9719, 203165696, NULL, 1, 203165698, 259, 7, 'Fers de la Tyrannie - Salle 5', NULL, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9719, 970000, 1, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9719, 970012, 1, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9719, 970022, 1, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9719, 970032, 1, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9719, 970012, 5, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9719, 970022, 6, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9719, 970032, 7, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (9719, 970037, 8, NULL);
DELETE FROM monsters_spawns WHERE Id BETWEEN 9700001 AND 9700099;
INSERT INTO monsters_spawns (Id, MapId, SubAreaId, MonsterId, Frequency, MinGrade, MaxGrade, IsDisabled) VALUES (9700001, NULL, 926, 6021, 1.00, 1, 5, 0);
INSERT INTO monsters_spawns (Id, MapId, SubAreaId, MonsterId, Frequency, MinGrade, MaxGrade, IsDisabled) VALUES (9700002, NULL, 926, 6022, 1.00, 1, 5, 0);
INSERT INTO monsters_spawns (Id, MapId, SubAreaId, MonsterId, Frequency, MinGrade, MaxGrade, IsDisabled) VALUES (9700003, NULL, 926, 6024, 1.00, 1, 5, 0);
INSERT INTO monsters_spawns (Id, MapId, SubAreaId, MonsterId, Frequency, MinGrade, MaxGrade, IsDisabled) VALUES (9700004, NULL, 926, 6025, 1.00, 1, 5, 0);
INSERT INTO monsters_spawns (Id, MapId, SubAreaId, MonsterId, Frequency, MinGrade, MaxGrade, IsDisabled) VALUES (9700005, NULL, 927, 5992, 1.00, 1, 5, 0);
INSERT INTO monsters_spawns (Id, MapId, SubAreaId, MonsterId, Frequency, MinGrade, MaxGrade, IsDisabled) VALUES (9700006, NULL, 927, 5993, 1.00, 1, 5, 0);
INSERT INTO monsters_spawns (Id, MapId, SubAreaId, MonsterId, Frequency, MinGrade, MaxGrade, IsDisabled) VALUES (9700007, NULL, 927, 5994, 1.00, 1, 5, 0);
INSERT INTO monsters_spawns (Id, MapId, SubAreaId, MonsterId, Frequency, MinGrade, MaxGrade, IsDisabled) VALUES (9700008, NULL, 927, 5995, 1.00, 1, 5, 0);
INSERT INTO monsters_spawns (Id, MapId, SubAreaId, MonsterId, Frequency, MinGrade, MaxGrade, IsDisabled) VALUES (9700009, NULL, 928, 6028, 1.00, 1, 5, 0);
INSERT INTO monsters_spawns (Id, MapId, SubAreaId, MonsterId, Frequency, MinGrade, MaxGrade, IsDisabled) VALUES (9700010, NULL, 928, 6029, 1.00, 1, 5, 0);
INSERT INTO monsters_spawns (Id, MapId, SubAreaId, MonsterId, Frequency, MinGrade, MaxGrade, IsDisabled) VALUES (9700011, NULL, 928, 6030, 1.00, 1, 5, 0);
INSERT INTO monsters_spawns (Id, MapId, SubAreaId, MonsterId, Frequency, MinGrade, MaxGrade, IsDisabled) VALUES (9700012, NULL, 928, 6031, 1.00, 1, 5, 0);
INSERT INTO monsters_spawns (Id, MapId, SubAreaId, MonsterId, Frequency, MinGrade, MaxGrade, IsDisabled) VALUES (9700013, NULL, 925, 5978, 1.00, 1, 5, 0);
INSERT INTO monsters_spawns (Id, MapId, SubAreaId, MonsterId, Frequency, MinGrade, MaxGrade, IsDisabled) VALUES (9700014, NULL, 925, 5980, 1.00, 1, 5, 0);
INSERT INTO monsters_spawns (Id, MapId, SubAreaId, MonsterId, Frequency, MinGrade, MaxGrade, IsDisabled) VALUES (9700015, NULL, 925, 5983, 1.00, 1, 5, 0);
INSERT INTO monsters_spawns (Id, MapId, SubAreaId, MonsterId, Frequency, MinGrade, MaxGrade, IsDisabled) VALUES (9700016, NULL, 925, 5989, 1.00, 1, 5, 0);
DELETE FROM langs WHERE Id BETWEEN 9040000 AND 9049999;
INSERT INTO langs (Id, French) VALUES (9040000, 'Derrière moi se trouve le trône de la plus grande guerrière du Monde des Douze. Ceux qui veulent l''affronter devront obtenir une clé au préalable afin de me prouver qu''ils sauront éveiller son intérêt. Elle déteste perdre son temps en futilités et serait d''humeur exécrable par la suite.'),
 (9040001, 'Voulez-vous vraiment utiliser le trousseau de clefs pour entrer ?'),
 (9040002, 'Voulez-vous vraiment utiliser une clef pour entrer ?'),
 (9040003, 'Voulez-vous retourner à $map#1 ?'),
 (9040004, 'Reprendre le Trône de Sang où vous l''avez quitté.'),
 (9040005, 'Donner la clef.'),
 (9040006, 'Utiliser le trousseau de clefs.'),
 (9040007, 'Téléporter les membres du groupe.'),
 (9040008, 'Oui.'),
 (9040009, 'Non.'),
 (9040010, 'Retourner dans cette salle.'),
 (9040011, 'Kapelache'),
 (9040012, 'J''ai fait en sorte qu''il soit possible de se rendre au plus près de la Sentence de la Balance en passant par ce tuyau. Malheureusement, l''accès est verrouillé par une porte. Sans clef, impossible d''accéder à l''intérieur.'),
 (9040013, 'Reprendre la Sentence de la Balance où vous l''avez quitté.'),
 (9040014, 'Bree Caule'),
 (9040015, 'Derrière moi s''étendent les profondeurs de l''arbre de mort, c''est un lieu sacré pour nous les Sadidas obscurs, rares sont ceux ayant eu la chance de s''approcher du cœur de l''arbre. Si tu veux t''y rendre, il faudra me donner une clé afin de prouver ta bonne foi.'),
 (9040016, 'Veux-tu vraiment utiliser le trousseau de clefs pour entrer ?'),
 (9040017, 'Veux-tu vraiment utiliser une clef pour entrer ?'),
 (9040018, 'Veux-tu retourner à $map#1 ?'),
 (9040019, 'Reprendre l''Arbre de Mort où vous l''avez quitté.'),
 (9040020, 'Rafflesia la Fétide'),
 (9040021, 'Notre Maître à tous se trouve tout en haut de cette tour d''où il peut observer notre loyauté et notre dévotion. Ceux qui souhaitent le rencontrer devront se montrer dignes en acceptant de recevoir son châtiment. Seuls les possesseurs d''une clé pourront prétendre à ce privilège.'),
 (9040022, 'Je ne peux pas vous permettre l''accès sans preuve de ce que tu avances.'),
 (9040023, 'Comment ? Des esclaves s''échappent ? Ça ne va pas du tout, je n''avais rien repéré de tout ça ! Servitude doit immédiatement en être informé !'),
 (9040024, 'Reprendre Fers de la Tyrannie où vous l''avez quitté.'),
 (9040025, 'Miradar'),
 (9040026, 'Passeur de l''Eliocalypse'),
 (9040027, 'Les quatre Cavaliers de l''Eliocalypse ont ouvert des failles vers leurs dimensions. Par l''Arche de Vili, tu pourras les atteindre... si tu es prêt à affronter Guerre, Misère, Corruption et Servitude (niveau 200).'),
 (9040028, 'Voici l''Arche de Vili. Chaque faille mène au domaine d''un Cavalier. Lequel veux-tu rejoindre ?'),
 (9040029, 'Je peux te ramener à l''Arche de Vili quand tu le souhaites.'),
 (9040030, 'Aller à l''Arche de Vili.'),
 (9040031, 'Retourner à Astrub.'),
 (9040032, 'Rejoindre Blessures de Guerre.'),
 (9040033, 'Rejoindre Désert de Misère.'),
 (9040034, 'Rejoindre Royaume Corrompu.'),
 (9040035, 'Rejoindre Galère de Servitude.');
COMMIT;
