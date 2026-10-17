-- NEKFEU : sorties par les vrais PNJ de fin de donjon (généré par generer_sorties_pnj_officiels.py), après sorties_donjons.sql
-- 6 Antre du Dragon Cochon : PNJ 784
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (6, 72363008, 72221696, 199, 1);
INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES (900200, 784, 'Talk', 'Mp=72363008', -1, '3231');
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) SELECT 900201, 'DungeonExit', 2855, 3231 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM npcs_replies WHERE Type = 'DungeonExit' AND ReplyId = 2855 AND MessageId = 3231);
-- 7 Donjon des Dragoeufs : PNJ 789
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (7, 27789318, 86248450, 274, 1);
INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES (900202, 789, 'Talk', 'Mp=27789318', -1, '3253');
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) SELECT 900203, 'DungeonExit', 2893, 3253 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM npcs_replies WHERE Type = 'DungeonExit' AND ReplyId = 2893 AND MessageId = 3253);
-- 9 Domaine Ancestral : PNJ 1619
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (9, 149689344, 147851777, 372, 1);
INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES (900204, 1619, 'Talk', 'Mp=149689344', -1, '11972');
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) SELECT 900205, 'DungeonExit', 13466, 11972 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM npcs_replies WHERE Type = 'DungeonExit' AND ReplyId = 13466 AND MessageId = 11972);
-- 10 Clairière du Chêne Mou : PNJ 797
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (10, 149428224, 149422080, 189, 1);
INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES (900206, 797, 'Talk', 'Mp=149428224', -1, '3278');
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) SELECT 900207, 'DungeonExit', 2929, 3278 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM npcs_replies WHERE Type = 'DungeonExit' AND ReplyId = 2929 AND MessageId = 3278);
-- 20 Donjon des Firefoux : PNJ 710
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (20, 16515331, 13613, 204, 1);
INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES (900208, 710, 'Talk', 'Mp=16515331', -1, '2944');
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) SELECT 900209, 'DungeonExit', 2580, 2944 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM npcs_replies WHERE Type = 'DungeonExit' AND ReplyId = 2580 AND MessageId = 2944);
-- 25 Grotte Hesque : PNJ 941
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (25, 5244422, 161295, 359, 1);
INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES (900210, 941, 'Talk', 'Mp=5244422', -1, '4191');
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) SELECT 900211, 'DungeonExit', 3669, 4191 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM npcs_replies WHERE Type = 'DungeonExit' AND ReplyId = 3669 AND MessageId = 4191);
-- 29 Donjon des Kitsounes : PNJ 708
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (29, 18089988, 16678, 398, 1);
INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES (900212, 708, 'Talk', 'Mp=18089988', -1, '2942');
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) SELECT 900213, 'DungeonExit', 2578, 2942 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM npcs_replies WHERE Type = 'DungeonExit' AND ReplyId = 2578 AND MessageId = 2942);
-- 32 Nid du Kwakwa : PNJ 1404
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (32, 64754688, 185861122, 266, 1);
INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES (900214, 1404, 'Talk', 'Mp=64754688', -1, '8507');
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) SELECT 900215, 'DungeonExit', 8555, 8507 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM npcs_replies WHERE Type = 'DungeonExit' AND ReplyId = 8555 AND MessageId = 8507);
-- 39 Cale de l'Arche d'Otomaï : PNJ 942
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (39, 22285314, 22546944, 226, 1);
INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES (900216, 942, 'Talk', 'Mp=22285314', -1, '4196');
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) SELECT 900217, 'DungeonExit', 3674, 4196 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM npcs_replies WHERE Type = 'DungeonExit' AND ReplyId = 3674 AND MessageId = 4196);
-- 40 Repaire des Pandikazes : PNJ 705
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (40, 17310720, 9508, 271, 1);
INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES (900218, 705, 'Talk', 'Mp=17310720', -1, '2938');
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) SELECT 900219, 'DungeonExit', 2575, 2938 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM npcs_replies WHERE Type = 'DungeonExit' AND ReplyId = 2575 AND MessageId = 2938);
-- 41 Goulet du Rasboul : PNJ 945
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (41, 22810626, 89916416, 325, 1);
INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES (900220, 945, 'Talk', 'Mp=22810626', -1, '4205');
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) SELECT 900221, 'DungeonExit', 3683, 4205 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM npcs_replies WHERE Type = 'DungeonExit' AND ReplyId = 3683 AND MessageId = 4205);
-- 42 Donjon des Rats de Bonta : PNJ 799
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (42, 27001860, 25035524, 178, 1);
INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES (900222, 799, 'Talk', 'Mp=27001860', -1, '3292');
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) SELECT 900223, 'DungeonExit', 2963, 3292 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM npcs_replies WHERE Type = 'DungeonExit' AND ReplyId = 2963 AND MessageId = 3292);
-- 43 Donjon des Rats de Brâkmar : PNJ 800
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (43, 40110087, 28443909, 163, 1);
INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES (900224, 800, 'Talk', 'Mp=40110087', -1, '3294');
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) SELECT 900225, 'DungeonExit', 2962, 3294 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM npcs_replies WHERE Type = 'DungeonExit' AND ReplyId = 2962 AND MessageId = 3294);
-- 54 Serre du Royalmouth : PNJ 1285
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (54, 55054336, 54175024, 268, 1);
INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES (900226, 1285, 'Talk', 'Mp=55054336', -1, '7695');
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) SELECT 900227, 'DungeonExit', 7712, 7695 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM npcs_replies WHERE Type = 'DungeonExit' AND ReplyId = 7712 AND MessageId = 7695);
-- 55 Excavation du Mansot Royal : PNJ 1300
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (55, 56103936, 55836672, 355, 1);
INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES (900228, 1300, 'Talk', 'Mp=56103936', -1, '7837');
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) SELECT 900229, 'DungeonExit', 7902, 7837 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM npcs_replies WHERE Type = 'DungeonExit' AND ReplyId = 7902 AND MessageId = 7837);
-- 56 Épave du Grolandais violent : PNJ 1357
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (56, 56365058, 54165334, 325, 1);
INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES (900230, 1357, 'Talk', 'Mp=56365058', -1, '8108');
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) SELECT 900231, 'DungeonExit', 8216, 8108 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM npcs_replies WHERE Type = 'DungeonExit' AND ReplyId = 8216 AND MessageId = 8108);
-- 57 Hypogée de l'Obsidiantre : PNJ 1358
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (57, 57157377, 54170450, 287, 1);
INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES (900232, 1358, 'Talk', 'Mp=57157377', -1, '8110');
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) SELECT 900233, 'DungeonExit', 8220, 8110 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM npcs_replies WHERE Type = 'DungeonExit' AND ReplyId = 8220 AND MessageId = 8110);
-- 58 Tanière Givrefoux : PNJ 1385
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (58, 59515138, 60031488, 286, 1);
INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES (900234, 1385, 'Talk', 'Mp=59515138', -1, '8988');
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) SELECT 900235, 'DungeonExit', 8325, 8988 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM npcs_replies WHERE Type = 'DungeonExit' AND ReplyId = 8325 AND MessageId = 8988);
-- 59 Antre du Korriandre : PNJ 1391
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (59, 62920704, 60036612, 285, 1);
INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES (900236, 1391, 'Talk', 'Mp=62920704', -1, '8428');
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) SELECT 900237, 'DungeonExit', 8451, 8428 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM npcs_replies WHERE Type = 'DungeonExit' AND ReplyId = 8451 AND MessageId = 8428);
-- 60 Cavernes du Kolosso : PNJ 1393
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (60, 61869060, 54164807, 286, 1);
INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES (900238, 1393, 'Talk', 'Mp=61869060', -1, '8458');
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) SELECT 900239, 'DungeonExit', 8478, 8458 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM npcs_replies WHERE Type = 'DungeonExit' AND ReplyId = 8478 AND MessageId = 8458);
-- 61 Antichambre des Gloursons : PNJ 1394
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (61, 62136840, 54165323, 286, 1);
INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES (900240, 1394, 'Talk', 'Mp=62136840', -1, '8460');
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) SELECT 900241, 'DungeonExit', 8479, 8460 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM npcs_replies WHERE Type = 'DungeonExit' AND ReplyId = 8479 AND MessageId = 8460);
-- 62 Donjon de la mine de Sakaï : PNJ 1363
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (62, 57939713, 54159661, 189, 1);
INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES (900242, 1363, 'Talk', 'Mp=57939713', -1, '8154');
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) SELECT 900243, 'DungeonExit', 8147, 8154 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM npcs_replies WHERE Type = 'DungeonExit' AND ReplyId = 8147 AND MessageId = 8154);
-- 64 Repaire de Daïgoro : PNJ 1509
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (64, 79435265, 79167488, 300, 1);
INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES (900244, 1509, 'Talk', 'Mp=79435265', -1, '9947');
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) SELECT 900245, 'DungeonExit', 10732, 9947 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM npcs_replies WHERE Type = 'DungeonExit' AND ReplyId = 10732 AND MessageId = 9947);
-- 75 Grotte de Kanigroula : PNJ 2294
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (75, 125832707, 125829633, 329, 1);
INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES (900246, 2294, 'Talk', 'Mp=125832707', -1, '17261');
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) SELECT 900247, 'DungeonExit', 20656, 17261 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM npcs_replies WHERE Type = 'DungeonExit' AND ReplyId = 20656 AND MessageId = 17261);
-- 90 Crypte de Kardorim : PNJ 2936
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (90, 152835072, 153881600, 329, 1);
INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES (900248, 2936, 'Talk', 'Mp=152835072', -1, '20842');
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) SELECT 900249, 'DungeonExit', 24994, 20842 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM npcs_replies WHERE Type = 'DungeonExit' AND ReplyId = 24994 AND MessageId = 20842);
-- 100 Cimetière des Mastodontes : PNJ 3646
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (100, 174331392, 173280513, 383, 1);
INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES (900250, 3646, 'Talk', 'Mp=174331392', -1, '26019');
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) SELECT 900251, 'DungeonExit', 31197, 26019 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM npcs_replies WHERE Type = 'DungeonExit' AND ReplyId = 31197 AND MessageId = 26019);
-- 101 Caverne d'El Piko : PNJ 3657
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (101, 174071808, 173540352, 177, 1);
INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES (900252, 3657, 'Talk', 'Mp=174071808', -1, '26234');
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) SELECT 900253, 'DungeonExit', 31477, 26234 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM npcs_replies WHERE Type = 'DungeonExit' AND ReplyId = 31477 AND MessageId = 26234);
