-- MYSTIC 17/10 : dialogues des vrais gardiens de donjon (clef / trousseau). Genere par gardiens_dialogues.py. Rejouable.
-- donjon 2 : Virgil (PNJ 774), message 3200
DELETE FROM npcs_replies WHERE MessageId=3200 AND (Type IN ('DungeonEnter','DungeonEnterTrousseau') OR (Type='Teleport' AND Parameter0='159124486'));
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnter', 2829, 3200); -- Donner la clef et entrer.
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnterTrousseau', 6611, 3200); -- Utiliser le trousseau de clefs.
DELETE FROM npcs_spawns WHERE NpcId=707 AND MapId=158334976; -- Zamawa remplace par le vrai gardien
-- donjon 4 : Fwoued (PNJ 784), message 3216
DELETE FROM npcs_replies WHERE MessageId=3216 AND (Type IN ('DungeonEnter','DungeonEnterTrousseau') OR (Type='Teleport' AND Parameter0='72352768'));
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnter', 2847, 3216); -- Présenter la clef et rentrer
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnterTrousseau', 8787, 3216); -- Utiliser uniquement le trousseau et entrer.
-- donjon 6 : Mawy Ingalsse (PNJ 780), message 3178
DELETE FROM npcs_replies WHERE MessageId=3178 AND (Type IN ('DungeonEnter') OR (Type='Teleport' AND Parameter0='190449664'));
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnter', 2802, 3178); -- Donner la clef et entrer.
-- donjon 10 : Iturik (PNJ 1165), message 7072
DELETE FROM npcs_replies WHERE MessageId=7072 AND (Type IN ('DungeonEnter','DungeonEnterTrousseau') OR (Type='Teleport' AND Parameter0='17564931'));
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnter', 6638, 7072); -- Donner la clef et entrer.
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnterTrousseau', 6639, 7072); -- Utiliser le trousseau de clefs.
-- donjon 11 : Bowork (PNJ 775), message 3172
DELETE FROM npcs_replies WHERE MessageId=3172 AND (Type IN ('DungeonEnter','DungeonEnterTrousseau') OR (Type='Teleport' AND Parameter0='104595969'));
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnter', 2794, 3172); -- Donner la clef du Donjon des Bworks.
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnterTrousseau', 6613, 3172); -- Utiliser le trousseau de clefs.
-- donjon 12 : Bworkie (PNJ 712), message 17027
DELETE FROM npcs_replies WHERE MessageId=17027 AND (Type IN ('DungeonEnter','DungeonEnterTrousseau') OR (Type='Teleport' AND Parameter0='104333825'));
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnter', 6636, 17027); -- Donner la clef et entrer.
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnterTrousseau', 6637, 17027); -- Utiliser le trousseau de clefs.
-- donjon 14 : Gawde Wabbit (PNJ 147), message 478
DELETE FROM npcs_replies WHERE MessageId=478 AND (Type IN ('DungeonEnter','DungeonEnterTrousseau') OR (Type='Teleport' AND Parameter0='116654593'));
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnter', 438, 478); -- Utiliser la clef.
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnterTrousseau', 18909, 478); -- Utiliser le trousseau de clefs.
-- donjon 15 : Krakotte (PNJ 772), message 3163
DELETE FROM npcs_replies WHERE MessageId=3163 AND (Type IN ('DungeonEnter','DungeonEnterTrousseau') OR (Type='Teleport' AND Parameter0='106954752'));
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnter', 2781, 3163); -- Donner la clef et entrer.
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnterTrousseau', 6612, 3163); -- Utiliser le trousseau de clefs.
-- donjon 16 : Capitaine Igloute (PNJ 798), message 3289
DELETE FROM npcs_replies WHERE MessageId=3289 AND (Type IN ('DungeonEnter','DungeonEnterTrousseau') OR (Type='Teleport' AND Parameter0='193725440'));
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnter', 2944, 3289); -- Donner la clef et entrer.
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnterTrousseau', 6617, 3289); -- Utiliser le trousseau de clefs.
-- donjon 18 : Pavu Papry (PNJ 1162), message 7069
DELETE FROM npcs_replies WHERE MessageId=7069 AND (Type IN ('DungeonEnter','DungeonEnterTrousseau') OR (Type='Teleport' AND Parameter0='87295489'));
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnter', 6632, 7069); -- Donner la clef et entrer.
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnterTrousseau', 6633, 7069); -- Utiliser le trousseau de clefs.
-- donjon 21 : Tina Montini (PNJ 941), message 4190
DELETE FROM npcs_replies WHERE MessageId=4190 AND (Type IN ('DungeonEnter','DungeonEnterTrousseau') OR (Type='Teleport' AND Parameter0='5243139'));
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnter', 3668, 4190); -- Donner la clef et entrer.
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnterTrousseau', 6620, 4190); -- Utiliser le trousseau de clefs.
-- donjon 23 : Gentil Organisateur Kanniboul (PNJ 1384), message 8271
DELETE FROM npcs_replies WHERE MessageId=8271 AND (Type IN ('DungeonEnter','DungeonEnterTrousseau') OR (Type='Teleport' AND Parameter0='157548544'));
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnter', 8306, 8271); -- Donner la clef.
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnterTrousseau', 8352, 8271); -- Utiliser le trousseau de clefs.
-- donjon 24 : Chachka (PNJ 946), message 4206
DELETE FROM npcs_replies WHERE MessageId=4206 AND (Type IN ('DungeonEnter','DungeonEnterTrousseau') OR (Type='Teleport' AND Parameter0='21495808'));
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnter', 3685, 4206); -- Utiliser la clef.
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnterTrousseau', 8909, 4206); -- Utiliser le trousseau de clefs.
-- donjon 26 : Gardien Koalak (PNJ 754), message 3119
DELETE FROM npcs_replies WHERE MessageId=3119 AND (Type IN ('DungeonEnterTrousseau'));
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnterTrousseau', 6607, 3119); -- Utiliser le trousseau de clefs.
-- donjon 28 : Gentil Organisateur Kwakerbak (PNJ 1404), message 8507
DELETE FROM npcs_replies WHERE MessageId=8507 AND (Type IN ('DungeonEnter','DungeonEnterTrousseau') OR (Type='Teleport' AND Parameter0='64749568'));
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnter', 8552, 8507); -- Utiliser la clef.
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnterTrousseau', 8554, 8507); -- Utiliser le trousseau de clefs.
DELETE FROM npcs_spawns WHERE NpcId=707 AND MapId=185861122; -- Zamawa remplace par le vrai gardien
-- donjon 30 : Gentil Organisateur Chafer (PNJ 1387), message 8321
DELETE FROM npcs_replies WHERE MessageId=8321 AND (Type IN ('DungeonEnter','DungeonEnterTrousseau') OR (Type='Teleport' AND Parameter0='163578368'));
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnter', 8358, 8321); -- Donner la clef.
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnterTrousseau', 8359, 8321); -- Utiliser le trousseau de clefs.
-- donjon 31 : Tassa (PNJ 1410), message 8667
DELETE FROM npcs_replies WHERE MessageId=8667 AND (Type IN ('DungeonEnter','DungeonEnterTrousseau') OR (Type='Teleport' AND Parameter0='176947200'));
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnter', 8706, 8667); -- Donner la clef.
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnterTrousseau', 8708, 8667); -- Utiliser le trousseau de clefs.
-- donjon 32 : Hector Nowel (PNJ 795), message 5494
DELETE FROM npcs_replies WHERE MessageId=5494 AND (Type IN ('DungeonEnter','DungeonEnterTrousseau') OR (Type='Teleport' AND Parameter0='66585088'));
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnter', 4613, 5494); -- Donner la clef et entrer.
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnterTrousseau', 8730, 5494); -- Utiliser le trousseau de clefs.
-- donjon 33 : Hector Nowel (PNJ 795), message 5494
DELETE FROM npcs_replies WHERE MessageId=5494 AND (Type IN ('DungeonEnter','DungeonEnterTrousseau') OR (Type='Teleport' AND Parameter0='66846720'));
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnter', 4613, 5494); -- Donner la clef et entrer.
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnterTrousseau', 8730, 5494); -- Utiliser le trousseau de clefs.
-- donjon 34 : Hector Nowel (PNJ 795), message 5494
DELETE FROM npcs_replies WHERE MessageId=5494 AND (Type IN ('DungeonEnter','DungeonEnterTrousseau') OR (Type='Teleport' AND Parameter0='66322432'));
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnter', 4613, 5494); -- Donner la clef et entrer.
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnterTrousseau', 8730, 5494); -- Utiliser le trousseau de clefs.
-- donjon 35 : Capitaine Flams (PNJ 942), message 4192
DELETE FROM npcs_replies WHERE MessageId=4192 AND (Type IN ('DungeonEnter','DungeonEnterTrousseau') OR (Type='Teleport' AND Parameter0='22282240'));
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnter', 3673, 4192); -- Utiliser la clef.
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnterTrousseau', 8936, 4192); -- Utiliser le trousseau de clefs.
-- donjon 36 : Mamowa (PNJ 706), message 2940
DELETE FROM npcs_replies WHERE MessageId=2940 AND (Type IN ('DungeonEnter','DungeonEnterTrousseau') OR (Type='Teleport' AND Parameter0='17302528'));
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnter', 2576, 2940); -- Donner la clef et entrer.
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnterTrousseau', 6603, 2940); -- Utiliser le trousseau de clefs.
-- donjon 37 : Razbitume (PNJ 945), message 4203
DELETE FROM npcs_replies WHERE MessageId=4203 AND (Type IN ('DungeonEnter','DungeonEnterTrousseau') OR (Type='Teleport' AND Parameter0='22808576'));
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnter', 3682, 4203); -- Utiliser la clef.
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnterTrousseau', 8941, 4203); -- Utiliser le trousseau de clefs.
-- donjon 38 : Portier du Donjon des Rats de Bonta (PNJ 799), message 3292
DELETE FROM npcs_replies WHERE MessageId=3292 AND (Type IN ('DungeonEnter','DungeonEnterTrousseau') OR (Type='Teleport' AND Parameter0='27000832'));
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnter', 2947, 3292); -- Donner la clef et entrer.
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnterTrousseau', 6619, 3292); -- Utiliser le trousseau de clefs.
-- donjon 40 : Aperirel (PNJ 818), message 3415
DELETE FROM npcs_replies WHERE MessageId=3415 AND (Type IN ('DungeonEnter','DungeonEnterTrousseau') OR (Type='Teleport' AND Parameter0='102760961'));
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnter', 3032, 3415); -- Donner la clef et entrer dans le donjon.
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnterTrousseau', 8847, 3415); -- Utiliser le trousseau de clefs.
-- donjon 41 : El Scarador (PNJ 781), message 3179
DELETE FROM npcs_replies WHERE MessageId=3179 AND (Type IN ('DungeonEnter','DungeonEnterTrousseau') OR (Type='Teleport' AND Parameter0='94110720'));
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnter', 2803, 3179); -- Donner la clef et entrer.
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnterTrousseau', 6614, 3179); -- Utiliser le trousseau de clefs.
-- donjon 42 : Kakolak (PNJ 758), message 3138
DELETE FROM npcs_replies WHERE MessageId=3138 AND (Type IN ('DungeonEnter','DungeonEnterTrousseau') OR (Type='Teleport' AND Parameter0='107481088'));
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnter', 15919, 3138); -- Donner la clef et entrer.
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnterTrousseau', 2762, 3138); -- Utiliser le trousseau de clefs.
-- donjon 43 : Klik Klak (PNJ 1163), message 7070
DELETE FROM npcs_replies WHERE MessageId=7070 AND (Type IN ('DungeonEnter','DungeonEnterTrousseau') OR (Type='Teleport' AND Parameter0='87033344'));
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnter', 6634, 7070); -- Donner la clef et entrer.
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnterTrousseau', 6635, 7070); -- Utiliser le trousseau de clefs.
-- donjon 43 : Klik Klak (PNJ 1163), message 7070
DELETE FROM npcs_replies WHERE MessageId=7070 AND (Type IN ('DungeonEnter','DungeonEnterTrousseau') OR (Type='Teleport' AND Parameter0='87033344'));
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnter', 6634, 7070); -- Donner la clef et entrer.
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnterTrousseau', 6635, 7070); -- Utiliser le trousseau de clefs.
-- donjon 44 : Kourk Ingalsse (PNJ 777), message 3175
DELETE FROM npcs_replies WHERE MessageId=3175 AND (Type IN ('DungeonEnter','DungeonEnterTrousseau') OR (Type='Teleport' AND Parameter0='96338946'));
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnter', 2797, 3175); -- Donner la clef du Tofulailler.
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnterTrousseau', 6608, 3175); -- Utiliser le trousseau de clefs.
-- donjon 44 : Kazy Ingalsse (PNJ 778), message 3176
DELETE FROM npcs_replies WHERE MessageId=3176 AND (Type IN ('DungeonEnter','DungeonEnterTrousseau') OR (Type='Teleport' AND Parameter0='96338946'));
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnter', 2798, 3176); -- Donner la clef du Tofulailler Royal.
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnterTrousseau', 6630, 3176); -- Utiliser le trousseau de clefs.
-- donjon 46 : Kourk Ingalsse (PNJ 777), message 3175
DELETE FROM npcs_replies WHERE MessageId=3175 AND (Type IN ('DungeonEnter','DungeonEnterTrousseau') OR (Type='Teleport' AND Parameter0='96338948'));
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnter', 2797, 3175); -- Donner la clef du Tofulailler.
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnterTrousseau', 6608, 3175); -- Utiliser le trousseau de clefs.
-- donjon 46 : Kazy Ingalsse (PNJ 778), message 3176
DELETE FROM npcs_replies WHERE MessageId=3176 AND (Type IN ('DungeonEnter','DungeonEnterTrousseau') OR (Type='Teleport' AND Parameter0='96338948'));
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnter', 2798, 3176); -- Donner la clef du Tofulailler Royal.
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnterTrousseau', 6630, 3176); -- Utiliser le trousseau de clefs.
-- donjon 47 : Mulèze (PNJ 943), message 4197
DELETE FROM npcs_replies WHERE MessageId=4197 AND (Type IN ('DungeonEnter','DungeonEnterTrousseau') OR (Type='Teleport' AND Parameter0='89391104'));
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnter', 3678, 4197); -- Utiliser la clef.
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnterTrousseau', 8960, 4197); -- Utiliser le trousseau de clefs.
-- donjon 48 : Powte-clef (PNJ 1439), message 8886
DELETE FROM npcs_replies WHERE MessageId=8886 AND (Type IN ('DungeonEnter','DungeonEnterTrousseau') OR (Type='Teleport' AND Parameter0='116392448'));
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnter', 18853, 8886); -- Donner la clef et entrer.
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnterTrousseau', 18854, 8886); -- Utiliser le trousseau de clefs.
-- donjon 56 : Professeur Xa (PNJ 1393), message 8457
DELETE FROM npcs_replies WHERE MessageId=8457 AND (Type IN ('DungeonEnter','DungeonEnterTrousseau') OR (Type='Teleport' AND Parameter0='61865984'));
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnter', 8474, 8457); -- Donner la clef.
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnterTrousseau', 8475, 8457); -- Utiliser le trousseau de clefs.
-- donjon 58 : Hywan Kenoby (PNJ 1363), message 8153
DELETE FROM npcs_replies WHERE MessageId=8153 AND (Type IN ('DungeonEnter','DungeonEnterTrousseau') OR (Type='Teleport' AND Parameter0='57934593'));
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnter', 8145, 8153); -- Donner la clef pour entrer.
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnterTrousseau', 8146, 8153); -- Utiliser le trousseau de clefs.
-- donjon 62 : Disciple d'Asid (PNJ 1768), message 12671
DELETE FROM npcs_replies WHERE MessageId=12671 AND (Type IN ('DungeonEnter','DungeonEnterTrousseau') OR (Type='Teleport' AND Parameter0='101188608'));
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnter', 14424, 12671); -- Donner la Clef du Potager d'Halouine.
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnterTrousseau', 14425, 12671); -- Utiliser le trousseau de clefs.
-- donjon 63 : Hiroze (PNJ 2005), message 14522
DELETE FROM npcs_replies WHERE MessageId=14522 AND (Type IN ('DungeonEnter','DungeonEnterTrousseau') OR (Type='Teleport' AND Parameter0='110100480'));
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnter', 16968, 14522); -- Donner la clef.
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnterTrousseau', 16969, 14522); -- Utiliser le trousseau de clefs.
-- donjon 64 : Domie Natrix (PNJ 2006), message 14524
DELETE FROM npcs_replies WHERE MessageId=14524 AND (Type IN ('DungeonEnter','DungeonEnterTrousseau') OR (Type='Teleport' AND Parameter0='110362624'));
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnter', 16973, 14524); -- Donner la clef.
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnterTrousseau', 16974, 14524); -- Utiliser le trousseau de clefs.
-- donjon 65 : Noo (PNJ 2007), message 14528
DELETE FROM npcs_replies WHERE MessageId=14528 AND (Type IN ('DungeonEnter','DungeonEnterTrousseau') OR (Type='Teleport' AND Parameter0='109838849'));
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnter', 16983, 14528); -- Donner la clef.
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnterTrousseau', 16984, 14528); -- Utiliser le trousseau de clefs.
-- donjon 66 : André Vauld (PNJ 2004), message 14520
DELETE FROM npcs_replies WHERE MessageId=14520 AND (Type IN ('DungeonEnter','DungeonEnterTrousseau') OR (Type='Teleport' AND Parameter0='109576705'));
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnter', 16963, 14520); -- Donner la clef.
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnterTrousseau', 16964, 14520); -- Utiliser le trousseau de clefs.
-- donjon 67 : Dorléans (PNJ 2008), message 15157
DELETE FROM npcs_replies WHERE MessageId=15157 AND (Type IN ('DungeonEnter','DungeonEnterTrousseau') OR (Type='Teleport' AND Parameter0='112201217'));
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnter', 17821, 15157); -- Donner la clef.
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnterTrousseau', 17822, 15157); -- Utiliser le trousseau de clefs.
-- donjon 68 : Otto Rhino (PNJ 2175), message 16317
DELETE FROM npcs_replies WHERE MessageId=16317 AND (Type IN ('DungeonEnter','DungeonEnterTrousseau') OR (Type='Teleport' AND Parameter0='118226944'));
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnter', 19373, 16317); -- Donner la clef et entrer.
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnterTrousseau', 19374, 16317); -- Utiliser le trousseau de clefs.
-- donjon 69 : Théo Dolith (PNJ 2196), message 16404
DELETE FROM npcs_replies WHERE MessageId=16404 AND (Type IN ('DungeonEnter','DungeonEnterTrousseau') OR (Type='Teleport' AND Parameter0='119277057'));
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnter', 19519, 16404); -- Donner la clef et franchir les portes.
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnterTrousseau', 19520, 16404); -- Utiliser le trousseau de clefs.
-- donjon 70 : Vhé (PNJ 2215), message 16553
DELETE FROM npcs_replies WHERE MessageId=16553 AND (Type IN ('DungeonEnter','DungeonEnterTrousseau') OR (Type='Teleport' AND Parameter0='123207680'));
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnter', 19715, 16553); -- Donner la clef et entrer.
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnterTrousseau', 19716, 16553); -- Utiliser le trousseau de clefs.
-- donjon 72 : Léonard (PNJ 2351), message 17885
DELETE FROM npcs_replies WHERE MessageId=17885 AND (Type IN ('DungeonEnter') OR (Type='Teleport' AND Parameter0='129500160'));
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnter', 21433, 17885); -- Donner la clef et entrer.
-- donjon 73 : Inès Karcel (PNJ 2353), message 17637
DELETE FROM npcs_replies WHERE MessageId=17637 AND (Type IN ('DungeonEnter') OR (Type='Teleport' AND Parameter0='130286592'));
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnter', 21153, 17637); -- Donner la clef et entrer.
-- donjon 74 : Dyane Phossé (PNJ 2352), message 17710
DELETE FROM npcs_replies WHERE MessageId=17710 AND (Type IN ('DungeonEnter') OR (Type='Teleport' AND Parameter0='130548736'));
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnter', 21222, 17710); -- Donner la clef et entrer.
-- donjon 75 : Orno Tolig (PNJ 2436), message 17977
DELETE FROM npcs_replies WHERE MessageId=17977 AND (Type IN ('DungeonEnter','DungeonEnterTrousseau') OR (Type='Teleport' AND Parameter0='132907008'));
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnter', 21570, 17977); -- Donner la clef et entrer.
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnterTrousseau', 21573, 17977); -- Utiliser le trousseau de clefs.
-- donjon 76 : Guy Cheutié (PNJ 2444), message 18286
DELETE FROM npcs_replies WHERE MessageId=18286 AND (Type IN ('DungeonEnter') OR (Type='Teleport' AND Parameter0='136578048'));
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnter', 21902, 18286); -- Donner la clef et entrer.
-- donjon 77 : Starchy Tropeur (PNJ 2445), message 18292
DELETE FROM npcs_replies WHERE MessageId=18292 AND (Type IN ('DungeonEnter') OR (Type='Teleport' AND Parameter0='136840192'));
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnter', 21912, 18292); -- Donner la clef et entrer.
-- donjon 78 : Chambelle (PNJ 2446), message 18289
DELETE FROM npcs_replies WHERE MessageId=18289 AND (Type IN ('DungeonEnter') OR (Type='Teleport' AND Parameter0='137102336'));
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnter', 21907, 18289); -- Donner la clef et entrer.
-- donjon 79 : Mathilde Oualtgniz (PNJ 2636), message 19017
DELETE FROM npcs_replies WHERE MessageId=19017 AND (Type IN ('DungeonEnter','DungeonEnterTrousseau') OR (Type='Teleport' AND Parameter0='140771328'));
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnter', 22750, 19017); -- Donner la clef et entrer.
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnterTrousseau', 22753, 19017); -- Utiliser le trousseau de clefs.
-- donjon 80 : Hypéria Gritch (PNJ 2661), message 19557
DELETE FROM npcs_replies WHERE MessageId=19557 AND (Type IN ('DungeonEnter') OR (Type='Teleport' AND Parameter0='143138823'));
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnter', 23376, 19557); -- Donner la clef et entrer.
-- donjon 81 : Gardien de l'Horologium (PNJ 2660), message 19397
DELETE FROM npcs_replies WHERE MessageId=19397 AND (Type IN ('DungeonEnter') OR (Type='Teleport' AND Parameter0='143917569'));
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnter', 23144, 19397); -- Donner la clef et entrer.
-- donjon 82 : Tessie Oude (PNJ 2662), message 19539
DELETE FROM npcs_replies WHERE MessageId=19539 AND (Type IN ('DungeonEnter') OR (Type='Teleport' AND Parameter0='143393281'));
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnter', 23354, 19539); -- Donner la clef et entrer.
-- donjon 83 : Cloportier (PNJ 2766), message 19697
DELETE FROM npcs_replies WHERE MessageId=19697 AND (Type IN ('DungeonEnter','DungeonEnterTrousseau') OR (Type='Teleport' AND Parameter0='146675712'));
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnter', 23588, 19697); -- Utiliser la clef et monter.
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnterTrousseau', 23589, 19697); -- Utiliser le trousseau de clefs.
-- donjon 84 : Saule (PNJ 2845), message 20453
DELETE FROM npcs_replies WHERE MessageId=20453 AND (Type IN ('DungeonEnter','DungeonEnterTrousseau') OR (Type='Teleport' AND Parameter0='149160960'));
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnter', 24473, 20453); -- Donner la clef et entrer.
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnterTrousseau', 24476, 20453); -- Utiliser le trousseau de clefs.
-- donjon 86 : Jack Martel (PNJ 3008), message 21818
DELETE FROM npcs_replies WHERE MessageId=21818 AND (Type IN ('DungeonEnter','DungeonEnterTrousseau') OR (Type='Teleport' AND Parameter0='157024256'));
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnter', 26249, 21818); -- Donner la clef et entrer.
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnterTrousseau', 26252, 21818); -- Utiliser le trousseau de clefs.
-- donjon 87 : Servante de Moon (PNJ 3007), message 21820
DELETE FROM npcs_replies WHERE MessageId=21820 AND (Type IN ('DungeonEnter','DungeonEnterTrousseau') OR (Type='Teleport' AND Parameter0='157286400'));
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnter', 26240, 21820); -- Donner la clef et entrer.
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnterTrousseau', 26243, 21820); -- Utiliser le trousseau de clefs.
-- donjon 88 : Caporal Tomonte (PNJ 3097), message 22674
DELETE FROM npcs_replies WHERE MessageId=22674 AND (Type IN ('DungeonEnter') OR (Type='Teleport' AND Parameter0='161743872'));
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnter', 27270, 22674); -- Donner la clef et entrer.
-- donjon 89 : Chalfred (PNJ 3103), message 22678
DELETE FROM npcs_replies WHERE MessageId=22678 AND (Type IN ('DungeonEnter') OR (Type='Teleport' AND Parameter0='162004992'));
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnter', 27276, 22678); -- Donner la clef et entrer.
-- donjon 90 : Filouface (PNJ 3099), message 22696
DELETE FROM npcs_replies WHERE MessageId=22696 AND (Type IN ('DungeonEnter') OR (Type='Teleport' AND Parameter0='160564224'));
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnter', 27295, 22696); -- Donner la clef et entrer.
-- donjon 92 : Nora Nax (PNJ 3256), message 23764
DELETE FROM npcs_replies WHERE MessageId=23764 AND (Type IN ('DungeonEnter','DungeonEnterTrousseau') OR (Type='Teleport' AND Parameter0='169869312'));
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnter', 28554, 23764); -- Donner la clef et entrer.
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnterTrousseau', 28557, 23764); -- Utiliser le trousseau de clefs.
-- donjon 94 : Kruchula (PNJ 3255), message 24326
DELETE FROM npcs_replies WHERE MessageId=24326 AND (Type IN ('DungeonEnter','DungeonEnterTrousseau') OR (Type='Teleport' AND Parameter0='169607168'));
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnter', 29231, 24326); -- Donner la clef et entrer.
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnterTrousseau', 29234, 24326); -- Utiliser le trousseau de clefs.
-- donjon 97 : Goba Fett (PNJ 3679), message 26439
DELETE FROM npcs_replies WHERE MessageId=26439 AND (Type IN ('DungeonEnter','DungeonEnterTrousseau') OR (Type='Teleport' AND Parameter0='176030208'));
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnter', 31731, 26439); -- Donner la clef et entrer.
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnterTrousseau', 31734, 26439); -- Utiliser le trousseau de clefs.
-- donjon 98 : Drol Novranrac (PNJ 3686), message 26444
DELETE FROM npcs_replies WHERE MessageId=26444 AND (Type IN ('DungeonEnter','DungeonEnterTrousseau') OR (Type='Teleport' AND Parameter0='176160768'));
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnter', 31742, 26444); -- Donner la clef et entrer.
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnterTrousseau', 31745, 26444); -- Utiliser le trousseau de clefs.
-- donjon 99 : Dédé (PNJ 3747), message 26892
DELETE FROM npcs_replies WHERE MessageId=26892 AND (Type IN ('DungeonEnter','DungeonEnterTrousseau') OR (Type='Teleport' AND Parameter0='179568640'));
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnter', 32330, 26892); -- Donner la clef et entrer.
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnterTrousseau', 32333, 26892); -- Utiliser le trousseau de clefs.
-- donjon 100 : Guichetière du Magik Riktus (PNJ 3966), message 27619
DELETE FROM npcs_replies WHERE MessageId=27619 AND (Type IN ('DungeonEnter','DungeonEnterTrousseau') OR (Type='Teleport' AND Parameter0='181665792'));
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnter', 33150, 27619); -- Donner la clef et entrer.
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnterTrousseau', 33153, 27619); -- Utiliser le trousseau de clefs.
-- donjon 105 : Triboulette (PNJ 4499), message 31789
DELETE FROM npcs_replies WHERE MessageId=31789 AND (Type IN ('DungeonEnter','DungeonEnterTrousseau') OR (Type='Teleport' AND Parameter0='195035136'));
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnter', 38444, 31789); -- Donner la clef.
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnterTrousseau', 38447, 31789); -- Utiliser le trousseau de clefs.
