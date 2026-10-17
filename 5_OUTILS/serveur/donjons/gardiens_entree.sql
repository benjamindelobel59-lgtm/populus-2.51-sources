-- NEKFEU : vrais gardiens d'entree de donjon a la place de Zamawa (707). Genere par generer_gardiens_entree.py. Serveur World ARRETE.
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) VALUES (900400, 'DungeonEnter', 2844, 3201);
DELETE FROM npcs_spawns WHERE Id = 10686; -- Zamawa (gardien 783 deja sur la map)
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) VALUES (900401, 'DungeonEnter', 6626, 3216);
DELETE FROM npcs_spawns WHERE Id = 10695; -- Zamawa (gardien 784 deja sur la map)
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) VALUES (900402, 'DungeonEnter', 6638, 7072);
DELETE FROM npcs_spawns WHERE Id = 10677; -- Zamawa (gardien 1165 deja sur la map)
INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES (900403, 775, 'Talk', 'Mp=104862211', 0, '3172');
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) VALUES (900404, 'DungeonEnter', 6613, 3172);
UPDATE npcs_spawns SET NpcId = 775 WHERE Id = 10705; -- Zamawa -> Bowork
INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES (900405, 712, 'Talk', 'Mp=172493057', 0, '17027');
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) VALUES (900406, 'DungeonEnter', 6636, 17027);
UPDATE npcs_spawns SET NpcId = 712 WHERE Id = 10739; -- Zamawa -> Bworkie
INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES (900407, 772, 'Talk', 'Mp=185861634', 0, '3163');
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) VALUES (900408, 'DungeonEnter', 2781, 3163);
UPDATE npcs_spawns SET NpcId = 772 WHERE Id = 10751; -- Zamawa -> Krakotte
INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES (900409, 798, 'Talk', 'Mp=190056961', 0, '3289');
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) VALUES (900410, 'DungeonEnter', 2944, 3289);
UPDATE npcs_spawns SET NpcId = 798 WHERE Id = 10753; -- Zamawa -> Capitaine Igloute
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) VALUES (900411, 'DungeonEnter', 2579, 2943);
DELETE FROM npcs_spawns WHERE Id = 10678; -- Zamawa (gardien 709 deja sur la map)
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) VALUES (900412, 'DungeonEnter', 3668, 4190);
DELETE FROM npcs_spawns WHERE Id = 10681; -- Zamawa (gardien 941 deja sur la map)
INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES (900413, 1384, 'Talk', 'Mp=156499971', 0, '8271');
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) VALUES (900414, 'DungeonEnter', 8352, 8271);
UPDATE npcs_spawns SET NpcId = 1384 WHERE Id = 10730; -- Zamawa -> Gentil Organisateur Kanniboul
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) VALUES (900415, 'DungeonEnter', 3685, 4206);
DELETE FROM npcs_spawns WHERE Id = 10685; -- Zamawa (gardien 946 deja sur la map)
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) VALUES (900416, 'DungeonEnter', 2742, 3119);
DELETE FROM npcs_spawns WHERE Id = 10680; -- Zamawa (gardien 754 deja sur la map)
INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES (900417, 1410, 'Talk', 'Mp=177079296', 0, '8667');
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) VALUES (900418, 'DungeonEnter', 8705, 8667);
UPDATE npcs_spawns SET NpcId = 1410 WHERE Id = 10742; -- Zamawa -> Tassa
INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES (900419, 795, 'Talk', 'Mp=66061569', 0, '5494');
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) VALUES (900420, 'DungeonEnter', 4613, 5494);
UPDATE npcs_spawns SET NpcId = 795 WHERE Id = 10693; -- Zamawa -> Hector Nowel
INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES (900421, 795, 'Talk', 'Mp=66192641', 0, '5494');
UPDATE npcs_spawns SET NpcId = 795 WHERE Id = 10694; -- Zamawa -> Hector Nowel
INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES (900422, 795, 'Talk', 'Mp=66060545', 0, '5494');
UPDATE npcs_spawns SET NpcId = 795 WHERE Id = 10692; -- Zamawa -> Hector Nowel
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) VALUES (900423, 'DungeonEnter', 3673, 4192);
DELETE FROM npcs_spawns WHERE Id = 10683; -- Zamawa (gardien 942 deja sur la map)
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) VALUES (900424, 'DungeonEnter', 2576, 2940);
DELETE FROM npcs_spawns WHERE Id = 10676; -- Zamawa (gardien 706 deja sur la map)
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) VALUES (900425, 'DungeonEnter', 3682, 4203);
UPDATE npcs_spawns SET NpcId = 945 WHERE Id = 10700; -- Zamawa -> Razbitume
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) VALUES (900426, 'DungeonEnter', 2947, 3292);
DELETE FROM npcs_spawns WHERE Id = 10684; -- Zamawa (gardien 799 deja sur la map)
INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES (900427, 818, 'Talk', 'Mp=101714451', 0, '3416');
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) VALUES (900428, 'DungeonEnter', 3032, 3416);
UPDATE npcs_spawns SET NpcId = 818 WHERE Id = 10704; -- Zamawa -> Aperirel
INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES (900429, 781, 'Talk', 'Mp=88080387', 0, '3179');
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) VALUES (900430, 'DungeonEnter', 2803, 3179);
UPDATE npcs_spawns SET NpcId = 781 WHERE Id = 10697; -- Zamawa -> El Scarador
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) VALUES (900431, 'DungeonEnter', 15919, 3138);
DELETE FROM npcs_spawns WHERE Id = 10690; -- Zamawa (gardien 758 deja sur la map)
INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES (900432, 778, 'Talk', 'Mp=96338944', 0, '17101');
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) VALUES (900433, 'DungeonEnter', 6630, 17101);
UPDATE npcs_spawns SET NpcId = 778 WHERE Id = 10701; -- Zamawa -> Kazy Ingalsse
INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES (900434, 778, 'Talk', 'Mp=96338944', 0, '17101');
UPDATE npcs_spawns SET NpcId = 778 WHERE Id = 10701; -- Zamawa -> Kazy Ingalsse
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) VALUES (900435, 'DungeonEnter', 3678, 4197);
DELETE FROM npcs_spawns WHERE Id = 10691; -- Zamawa (gardien 943 deja sur la map)
UPDATE npcs_spawns SET NpcId = 783 WHERE Id = 10687; -- Zamawa -> Lorkos
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) VALUES (900436, 'DungeonEnter', 8475, 8457);
DELETE FROM npcs_spawns WHERE Id = 10689; -- Zamawa (gardien 1393 deja sur la map)
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) VALUES (900437, 'DungeonEnter', 8146, 8153);
DELETE FROM npcs_spawns WHERE Id = 10688; -- Zamawa (gardien 1363 deja sur la map)
INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES (900438, 1768, 'Talk', 'Mp=101187584', 0, '12672');
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) VALUES (900439, 'DungeonEnter', 14425, 12672);
UPDATE npcs_spawns SET NpcId = 1768 WHERE Id = 10703; -- Zamawa -> Disciple d'Asid
INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES (900440, 2005, 'Talk', 'Mp=108793118', 0, '17196');
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) VALUES (900441, 'DungeonEnter', 16969, 17196);
UPDATE npcs_spawns SET NpcId = 2005 WHERE Id = 10707; -- Zamawa -> Hiroze
INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES (900442, 2196, 'Talk', 'Mp=117706767', 0, '16404');
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) VALUES (900443, 'DungeonEnter', 19520, 16404);
UPDATE npcs_spawns SET NpcId = 2196 WHERE Id = 10712; -- Zamawa -> Théo Dolith
INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES (900444, 2215, 'Talk', 'Mp=121636870', 0, '16553');
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) VALUES (900445, 'DungeonEnter', 19715, 16553);
UPDATE npcs_spawns SET NpcId = 2215 WHERE Id = 10714; -- Zamawa -> Vhé
INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES (900446, 2351, 'Talk', 'Mp=129499136', 0, '17887');
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) VALUES (900447, 'DungeonEnter', 21433, 17887);
UPDATE npcs_spawns SET NpcId = 2351 WHERE Id = 10715; -- Zamawa -> Léonard
INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES (900448, 2353, 'Talk', 'Mp=130285568', 0, '17637');
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) VALUES (900449, 'DungeonEnter', 21153, 17637);
UPDATE npcs_spawns SET NpcId = 2353 WHERE Id = 10716; -- Zamawa -> Inès Karcel
INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES (900450, 2352, 'Talk', 'Mp=130547712', 0, '17710');
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) VALUES (900451, 'DungeonEnter', 21222, 17710);
UPDATE npcs_spawns SET NpcId = 2352 WHERE Id = 10717; -- Zamawa -> Dyane Phossé
INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES (900452, 2436, 'Talk', 'Mp=133171202', 0, '17977');
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) VALUES (900453, 'DungeonEnter', 21570, 17977);
UPDATE npcs_spawns SET NpcId = 2436 WHERE Id = 10718; -- Zamawa -> Orno Tolig
INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES (900454, 2445, 'Talk', 'Mp=136839168', 0, '18292');
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) VALUES (900455, 'DungeonEnter', 21912, 18292);
UPDATE npcs_spawns SET NpcId = 2445 WHERE Id = 10721; -- Zamawa -> Starchy Tropeur
INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES (900456, 2636, 'Talk', 'Mp=140641537', 0, '19079');
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) VALUES (900457, 'DungeonEnter', 22750, 19079);
UPDATE npcs_spawns SET NpcId = 2636 WHERE Id = 10723; -- Zamawa -> Mathilde Oualtgniz
INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES (900458, 2661, 'Talk', 'Mp=144969990', 0, '19557');
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) VALUES (900459, 'DungeonEnter', 23376, 19557);
UPDATE npcs_spawns SET NpcId = 2661 WHERE Id = 10726; -- Zamawa -> Hypéria Gritch
INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES (900460, 2660, 'Talk', 'Mp=144707846', 0, '19397');
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) VALUES (900461, 'DungeonEnter', 23144, 19397);
UPDATE npcs_spawns SET NpcId = 2660 WHERE Id = 10725; -- Zamawa -> Gardien de l'Horologium
INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES (900462, 2662, 'Talk', 'Mp=144445702', 0, '19539');
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) VALUES (900463, 'DungeonEnter', 23354, 19539);
UPDATE npcs_spawns SET NpcId = 2662 WHERE Id = 10724; -- Zamawa -> Tessie Oude
INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES (900464, 3097, 'Talk', 'Mp=161742848', 0, '22674');
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) VALUES (900465, 'DungeonEnter', 27270, 22674);
UPDATE npcs_spawns SET NpcId = 3097 WHERE Id = 10735; -- Zamawa -> Caporal Tomonte
INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES (900466, 3679, 'Talk', 'Mp=174850048', 0, '26439');
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) VALUES (900467, 'DungeonEnter', 31731, 26439);
UPDATE npcs_spawns SET NpcId = 3679 WHERE Id = 10740; -- Zamawa -> Goba Fett
INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES (900468, 3686, 'Talk', 'Mp=175637504', 0, '26444');
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) VALUES (900469, 'DungeonEnter', 31742, 26444);
UPDATE npcs_spawns SET NpcId = 3686 WHERE Id = 10741; -- Zamawa -> Drol Novranrac
INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES (900470, 3747, 'Talk', 'Mp=179438082', 0, '26892');
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) VALUES (900471, 'DungeonEnter', 32330, 26892);
UPDATE npcs_spawns SET NpcId = 3747 WHERE Id = 10743; -- Zamawa -> Dédé
INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES (900472, 3966, 'Talk', 'Mp=181797888', 0, '27619');
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) VALUES (900473, 'DungeonEnter', 33150, 27619);
UPDATE npcs_spawns SET NpcId = 3966 WHERE Id = 10746; -- Zamawa -> Guichetière du Magik Riktus
INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES (900474, 4499, 'Talk', 'Mp=194779138', 0, '31789');
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) VALUES (900475, 'DungeonEnter', 38447, 31789);
UPDATE npcs_spawns SET NpcId = 4499 WHERE Id = 10755; -- Zamawa -> Triboulette
