-- Nekfeu : sorties des donjons (salles de fin sans sortie) — généré par _OUTILS/donjons/generer_sorties_donjons.py
CREATE TABLE IF NOT EXISTS dungeons_exits (Id INT NOT NULL AUTO_INCREMENT PRIMARY KEY, DungeonId INT NOT NULL, EndMapId INT NOT NULL, ExitMapId INT NOT NULL, ExitCellId INT NOT NULL, ExitDirection INT NOT NULL DEFAULT 1, KEY (EndMapId)) ENGINE=MyISAM DEFAULT CHARSET=utf8;
-- 3 Bibliothèque du Maître Corbac : salle de fin 159121666, PNJ 800
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (3, 159121666, 158334976, 326, 1);
INSERT INTO npcs_spawns (Id, NpcId, MapId, CellId, Direction, Npc) VALUES (900001, 800, 159121666, 286, 1, 'Nekfeu sortie donjon 3');
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) VALUES (900002, 'DungeonExit', 2962, 3294);
-- 13 Donjon des Bworks : salle de fin 104598019, PNJ 712
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (13, 104598019, 104862211, 327, 1);
INSERT INTO npcs_spawns (Id, NpcId, MapId, CellId, Direction, Npc) VALUES (900003, 712, 104598019, 286, 1, 'Nekfeu sortie donjon 13');
INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES (900004, 712, 'Talk', 'Mp=104598019', 0, '2960');
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) VALUES (900005, 'DungeonExit', 14834, 2960);
-- 14 Grotte du Bworker : salle de fin 104334851, PNJ 712
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (14, 104334851, 172493057, 294, 1);
INSERT INTO npcs_spawns (Id, NpcId, MapId, CellId, Direction, Npc) VALUES (900006, 712, 104334851, 300, 1, 'Nekfeu sortie donjon 14');
INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES (900007, 712, 'Talk', 'Mp=104334851', 0, '2960');
-- 15 Tanière du Meulou : salle de fin 155719680, PNJ 800
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (15, 155719680, 171706891, 407, 1);
INSERT INTO npcs_spawns (Id, NpcId, MapId, CellId, Direction, Npc) VALUES (900008, 800, 155719680, 300, 1, 'Nekfeu sortie donjon 15');
-- 17 Terrier du Wa Wabbit : salle de fin 116655619, PNJ 137
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (17, 116655619, 68420613, 327, 1);
INSERT INTO npcs_spawns (Id, NpcId, MapId, CellId, Direction, Npc) VALUES (900009, 137, 116655619, 300, 1, 'Nekfeu sortie donjon 17');
INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES (900010, 137, 'Talk', 'Mp=116655619', 0, '470');
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) VALUES (900011, 'DungeonExit', 18856, 470);
-- 18 Pitons Rocheux des Craqueleurs : salle de fin 106961920, PNJ 800
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (18, 106961920, 185861634, 270, 1);
INSERT INTO npcs_spawns (Id, NpcId, MapId, CellId, Direction, Npc) VALUES (900012, 800, 106961920, 300, 1, 'Nekfeu sortie donjon 18');
-- 22 Temple du Grand Ougah : salle de fin 182193153, PNJ 800
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (22, 182193153, 181146624, 266, 1);
INSERT INTO npcs_spawns (Id, NpcId, MapId, CellId, Direction, Npc) VALUES (900013, 800, 182193153, 314, 1, 'Nekfeu sortie donjon 22');
-- 27 Village Kanniboul : salle de fin 157553664, PNJ 1384
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (27, 157553664, 156499971, 382, 1);
INSERT INTO npcs_spawns (Id, NpcId, MapId, CellId, Direction, Npc) VALUES (900014, 1384, 157553664, 286, 1, 'Nekfeu sortie donjon 27');
INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES (900015, 1384, 'Talk', 'Mp=157553664', 0, '8271');
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) VALUES (900016, 'DungeonExit', 8307, 8271);
-- 28 Canopée du Kimbo : salle de fin 21498882, PNJ 800
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (28, 21498882, 25693696, 170, 1);
INSERT INTO npcs_spawns (Id, NpcId, MapId, CellId, Direction, Npc) VALUES (900017, 800, 21498882, 300, 1, 'Nekfeu sortie donjon 28');
-- 30 Caverne du Koulosse : salle de fin 107228160, PNJ 800
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (30, 107228160, 139784, 95, 1);
INSERT INTO npcs_spawns (Id, NpcId, MapId, CellId, Direction, Npc) VALUES (900018, 800, 107228160, 286, 1, 'Nekfeu sortie donjon 30');
-- 36 Donjon de Nowel : salle de fin 66590208, PNJ 795
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (36, 66590208, 66061569, 351, 1);
INSERT INTO npcs_spawns (Id, NpcId, MapId, CellId, Direction, Npc) VALUES (900019, 795, 66590208, 287, 1, 'Nekfeu sortie donjon 36');
INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES (900020, 795, 'Talk', 'Mp=66590208', 0, '3254');
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) VALUES (900021, 'DungeonExit', 8720, 3254);
-- 37 Caverne de Nowel : salle de fin 66851840, PNJ 795
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (37, 66851840, 66192641, 266, 1);
INSERT INTO npcs_spawns (Id, NpcId, MapId, CellId, Direction, Npc) VALUES (900022, 795, 66851840, 300, 1, 'Nekfeu sortie donjon 37');
INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES (900023, 795, 'Talk', 'Mp=66851840', 0, '3254');
-- 38 Maison du Papa Nowel : salle de fin 66327552, PNJ 795
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (38, 66327552, 66060545, 266, 1);
INSERT INTO npcs_spawns (Id, NpcId, MapId, CellId, Direction, Npc) VALUES (900024, 795, 66327552, 286, 1, 'Nekfeu sortie donjon 38');
INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES (900025, 795, 'Talk', 'Mp=66327552', 0, '3254');
-- 44 Donjon des Rats du Château d'Amakna : salle de fin 102761987, PNJ 800
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (44, 102761987, 101714451, 242, 1);
INSERT INTO npcs_spawns (Id, NpcId, MapId, CellId, Direction, Npc) VALUES (900026, 800, 102761987, 300, 1, 'Nekfeu sortie donjon 44');
-- 46 Repaire de Skeunk : salle de fin 107486208, PNJ 800
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (46, 107486208, 63310595, 287, 1);
INSERT INTO npcs_spawns (Id, NpcId, MapId, CellId, Direction, Npc) VALUES (900027, 800, 107486208, 300, 1, 'Nekfeu sortie donjon 46');
-- 50 Tofulailler Royal : salle de fin 96210950, PNJ 800
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (50, 96210950, 96338944, 326, 1);
INSERT INTO npcs_spawns (Id, NpcId, MapId, CellId, Direction, Npc) VALUES (900028, 800, 96210950, 300, 1, 'Nekfeu sortie donjon 50');
-- 51 Laboratoire du Tynril : salle de fin 89393154, PNJ 800
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (51, 89393154, 63964672, 299, 1);
INSERT INTO npcs_spawns (Id, NpcId, MapId, CellId, Direction, Npc) VALUES (900029, 800, 89393154, 314, 1, 'Nekfeu sortie donjon 51');
-- 52 Château du Wa Wabbit : salle de fin 116393474, PNJ 2151
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (52, 116393474, 99614979, 266, 1);
INSERT INTO npcs_spawns (Id, NpcId, MapId, CellId, Direction, Npc) VALUES (900030, 2151, 116393474, 300, 1, 'Nekfeu sortie donjon 52');
INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES (900031, 2151, 'Talk', 'Mp=116393474', 0, '16006');
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) VALUES (900032, 'DungeonExit', 18921, 16006);
-- 53 Salle du Minotot : salle de fin 34472452, PNJ 783
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (53, 34472452, 34476294, 286, 1);
INSERT INTO npcs_spawns (Id, NpcId, MapId, CellId, Direction, Npc) VALUES (900033, 783, 34472452, 300, 1, 'Nekfeu sortie donjon 53');
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) VALUES (900034, 'DungeonExit', 2835, 3201);
-- 66 Potager d'Halouine : salle de fin 101192704, PNJ 800
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (66, 101192704, 101187584, 324, 1);
INSERT INTO npcs_spawns (Id, NpcId, MapId, CellId, Direction, Npc) VALUES (900035, 800, 101192704, 286, 1, 'Nekfeu sortie donjon 66');
-- 67 Transporteur de Sylargh : salle de fin 110102530, PNJ 800
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (67, 110102530, 108793118, 294, 1);
INSERT INTO npcs_spawns (Id, NpcId, MapId, CellId, Direction, Npc) VALUES (900036, 800, 110102530, 300, 1, 'Nekfeu sortie donjon 67');
-- 68 Salons privés de Klime : salle de fin 110364674, PNJ 800
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (68, 110364674, 108925717, 266, 1);
INSERT INTO npcs_spawns (Id, NpcId, MapId, CellId, Direction, Npc) VALUES (900037, 800, 110364674, 300, 1, 'Nekfeu sortie donjon 68');
-- 69 Forgefroide de Missiz Frizz : salle de fin 109841923, PNJ 800
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (69, 109841923, 108927238, 243, 1);
INSERT INTO npcs_spawns (Id, NpcId, MapId, CellId, Direction, Npc) VALUES (900038, 800, 109841923, 300, 1, 'Nekfeu sortie donjon 69');
-- 70 Laboratoire de Nileza : salle de fin 109577731, PNJ 800
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (70, 109577731, 108794630, 358, 1);
INSERT INTO npcs_spawns (Id, NpcId, MapId, CellId, Direction, Npc) VALUES (900039, 800, 109577731, 300, 1, 'Nekfeu sortie donjon 70');
-- 71 Donjon du Comte Harebourg : salle de fin 112204547, PNJ 800
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (71, 112204547, 112460048, 308, 1);
INSERT INTO npcs_spawns (Id, NpcId, MapId, CellId, Direction, Npc) VALUES (900040, 800, 112204547, 286, 1, 'Nekfeu sortie donjon 71');
-- 72 Théâtre de Dramak : salle de fin 118232064, PNJ 800
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (72, 118232064, 117965057, 266, 1);
INSERT INTO npcs_spawns (Id, NpcId, MapId, CellId, Direction, Npc) VALUES (900041, 800, 118232064, 300, 1, 'Nekfeu sortie donjon 72');
-- 73 Aquadôme de Merkator : salle de fin 119278083, PNJ 800
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (73, 119278083, 117706767, 338, 1);
INSERT INTO npcs_spawns (Id, NpcId, MapId, CellId, Direction, Npc) VALUES (900042, 800, 119278083, 300, 1, 'Nekfeu sortie donjon 73');
-- 74 Pyramide d'Ombre : salle de fin 123213824, PNJ 800
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (74, 123213824, 121636870, 266, 1);
INSERT INTO npcs_spawns (Id, NpcId, MapId, CellId, Direction, Npc) VALUES (900043, 800, 123213824, 287, 1, 'Nekfeu sortie donjon 74');
-- 76 Palais du roi Nidas : salle de fin 129501184, PNJ 800
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (76, 129501184, 129236992, 314, 1);
INSERT INTO npcs_spawns (Id, NpcId, MapId, CellId, Direction, Npc) VALUES (900044, 800, 129501184, 299, 1, 'Nekfeu sortie donjon 76');
-- 77 Fabrique de Malléfisk : salle de fin 130287616, PNJ 800
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (77, 130287616, 129368578, 286, 1);
INSERT INTO npcs_spawns (Id, NpcId, MapId, CellId, Direction, Npc) VALUES (900045, 800, 130287616, 300, 1, 'Nekfeu sortie donjon 77');
-- 79 Galerie du Phossile : salle de fin 130549760, PNJ 2352
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (79, 130549760, 129238529, 286, 1);
INSERT INTO npcs_spawns (Id, NpcId, MapId, CellId, Direction, Npc) VALUES (900046, 2352, 130549760, 300, 1, 'Nekfeu sortie donjon 79');
INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES (900047, 2352, 'Talk', 'Mp=130549760', 0, '17710');
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) VALUES (900048, 'DungeonExit', 21438, 17710);
-- 80 Volière de la Haute Truche : salle de fin 132912128, PNJ 800
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (80, 132912128, 133171202, 343, 1);
INSERT INTO npcs_spawns (Id, NpcId, MapId, CellId, Direction, Npc) VALUES (900049, 800, 132912128, 300, 1, 'Nekfeu sortie donjon 80');
-- 82 Cave du Toxoliath : salle de fin 136841216, PNJ 800
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (82, 136841216, 135269891, 286, 1);
INSERT INTO npcs_spawns (Id, NpcId, MapId, CellId, Direction, Npc) VALUES (900050, 800, 136841216, 300, 1, 'Nekfeu sortie donjon 82');
-- 84 Ventre de la Baleine : salle de fin 140777472, PNJ 800
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (84, 140777472, 140641537, 372, 1);
INSERT INTO npcs_spawns (Id, NpcId, MapId, CellId, Direction, Npc) VALUES (900051, 800, 140777472, 315, 1, 'Nekfeu sortie donjon 84');
-- 85 Mégalithe de Fraktale : salle de fin 143139847, PNJ 800
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (85, 143139847, 144969990, 393, 1);
INSERT INTO npcs_spawns (Id, NpcId, MapId, CellId, Direction, Npc) VALUES (900052, 800, 143139847, 300, 1, 'Nekfeu sortie donjon 85');
-- 86 Horologium de XLII : salle de fin 143918593, PNJ 800
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (86, 143918593, 144707846, 406, 1);
INSERT INTO npcs_spawns (Id, NpcId, MapId, CellId, Direction, Npc) VALUES (900053, 800, 143918593, 300, 1, 'Nekfeu sortie donjon 86');
-- 87 Œil de Vortex : salle de fin 143394305, PNJ 800
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (87, 143394305, 144445702, 395, 1);
INSERT INTO npcs_spawns (Id, NpcId, MapId, CellId, Direction, Npc) VALUES (900054, 800, 143394305, 300, 1, 'Nekfeu sortie donjon 87');
-- 89 Antre de la Reine Nyée : salle de fin 149166080, PNJ 800
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (89, 149166080, 147853312, 286, 1);
INSERT INTO npcs_spawns (Id, NpcId, MapId, CellId, Direction, Npc) VALUES (900055, 800, 149166080, 300, 1, 'Nekfeu sortie donjon 89');
-- 91 Bateau du Chouque : salle de fin 157029376, PNJ 800
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (91, 157029376, 158078976, 353, 1);
INSERT INTO npcs_spawns (Id, NpcId, MapId, CellId, Direction, Npc) VALUES (900056, 800, 157029376, 286, 1, 'Nekfeu sortie donjon 91');
-- 92 Arbre de Moon : salle de fin 157291520, PNJ 800
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (92, 157291520, 156499968, 313, 1);
INSERT INTO npcs_spawns (Id, NpcId, MapId, CellId, Direction, Npc) VALUES (900057, 800, 157291520, 300, 1, 'Nekfeu sortie donjon 92');
-- 96 Temple Maudit d'Araknas : salle de fin 182456320, PNJ 800
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (96, 182456320, 172757504, 286, 1);
INSERT INTO npcs_spawns (Id, NpcId, MapId, CellId, Direction, Npc) VALUES (900058, 800, 182456320, 300, 1, 'Nekfeu sortie donjon 96');
-- 97 Vaisseau du Capitaine Meno : salle de fin 169874432, PNJ 800
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (97, 169874432, 169084930, 324, 1);
INSERT INTO npcs_spawns (Id, NpcId, MapId, CellId, Direction, Npc) VALUES (900059, 800, 169874432, 300, 1, 'Nekfeu sortie donjon 97');
-- 98 Temple de Koutoulou : salle de fin 169350144, PNJ 800
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (98, 169350144, 168560640, 285, 1);
INSERT INTO npcs_spawns (Id, NpcId, MapId, CellId, Direction, Npc) VALUES (900060, 800, 169350144, 300, 1, 'Nekfeu sortie donjon 98');
-- 99 Palais de Dantinéa : salle de fin 169612288, PNJ 800
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (99, 169612288, 168820736, 382, 1);
INSERT INTO npcs_spawns (Id, NpcId, MapId, CellId, Direction, Npc) VALUES (900061, 800, 169612288, 329, 1, 'Nekfeu sortie donjon 99');
-- 102 Boyau du Père Ver : salle de fin 175903232, PNJ 800
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (102, 175903232, 174850048, 462, 1);
INSERT INTO npcs_spawns (Id, NpcId, MapId, CellId, Direction, Npc) VALUES (900062, 800, 175903232, 300, 1, 'Nekfeu sortie donjon 102');
-- 103 Chambre de Tal Kasha : salle de fin 176165888, PNJ 800
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (103, 176165888, 175637504, 266, 1);
INSERT INTO npcs_spawns (Id, NpcId, MapId, CellId, Direction, Npc) VALUES (900063, 800, 176165888, 300, 1, 'Nekfeu sortie donjon 103');
-- 104 Camp du Comte Razof : salle de fin 179573760, PNJ 800
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (104, 179573760, 179438082, 295, 1);
INSERT INTO npcs_spawns (Id, NpcId, MapId, CellId, Direction, Npc) VALUES (900064, 800, 179573760, 300, 1, 'Nekfeu sortie donjon 104');
-- 105 Chapiteau des Magik Riktus : salle de fin 181670912, PNJ 3994
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (105, 181670912, 181797888, 315, 1);
INSERT INTO npcs_spawns (Id, NpcId, MapId, CellId, Direction, Npc) VALUES (900065, 3994, 181670912, 300, 1, 'Nekfeu sortie donjon 105');
INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES (900066, 3994, 'Talk', 'Mp=181670912', 0, '27802');
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) VALUES (900067, 'DungeonExit', 33360, 27802);
-- 106 Manoir des Katrepat : salle de fin 182719488, PNJ 800
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (106, 182719488, 179831808, 378, 1);
INSERT INTO npcs_spawns (Id, NpcId, MapId, CellId, Direction, Npc) VALUES (900068, 800, 182719488, 314, 1, 'Nekfeu sortie donjon 106');
-- 107 Belvédère d'Ilyzaelle : salle de fin 184685313, PNJ 800
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (107, 184685313, 183501836, 353, 1);
INSERT INTO npcs_spawns (Id, NpcId, MapId, CellId, Direction, Npc) VALUES (900069, 800, 184685313, 300, 1, 'Nekfeu sortie donjon 107');
-- 108 Tour de Bethel : salle de fin 187438080, PNJ 800
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (108, 187438080, 186384896, 267, 1);
INSERT INTO npcs_spawns (Id, NpcId, MapId, CellId, Direction, Npc) VALUES (900070, 800, 187438080, 300, 1, 'Nekfeu sortie donjon 108');
-- 109 Tour de Solar : salle de fin 187957514, PNJ 800
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (109, 187957514, 183108098, 411, 1);
INSERT INTO npcs_spawns (Id, NpcId, MapId, CellId, Direction, Npc) VALUES (900071, 800, 187957514, 300, 1, 'Nekfeu sortie donjon 109');
-- 110 Brasserie du roi Dazak : salle de fin 195040256, PNJ 800
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (110, 195040256, 194779138, 266, 1);
INSERT INTO npcs_spawns (Id, NpcId, MapId, CellId, Direction, Npc) VALUES (900072, 800, 195040256, 300, 1, 'Nekfeu sortie donjon 110');
-- cases d'arrivée après le boss / Canopée du Kimbo
UPDATE monsters_spawns_dungeons SET TeleportCell = 271 WHERE Id = 312;
UPDATE monsters_spawns_dungeons SET TeleportCell = 327 WHERE Id = 319;
UPDATE monsters_spawns_dungeons SET TeleportMapId = 21498882, TeleportCell = 271 WHERE Id = 282;
UPDATE monsters_spawns_dungeons SET TeleportCell = 271 WHERE Id = 191;
UPDATE monsters_spawns_dungeons SET TeleportCell = 271 WHERE Id = 327;
UPDATE monsters_spawns_dungeons SET TeleportCell = 271 WHERE Id = 271;
UPDATE monsters_spawns_dungeons SET TeleportCell = 271 WHERE Id = 223;
UPDATE monsters_spawns_dungeons SET TeleportCell = 273 WHERE Id = 174;
UPDATE monsters_spawns_dungeons SET TeleportCell = 273 WHERE Id = 364;
UPDATE monsters_spawns_dungeons SET TeleportCell = 271 WHERE Id = 374;
UPDATE monsters_spawns_dungeons SET TeleportCell = 271 WHERE Id = 384;
UPDATE monsters_spawns_dungeons SET TeleportCell = 271 WHERE Id = 389;
UPDATE monsters_spawns_dungeons SET TeleportCell = 273 WHERE Id = 354;
UPDATE monsters_spawns_dungeons SET TeleportCell = 327 WHERE Id = 390;
UPDATE monsters_spawns_dungeons SET TeleportCell = 271 WHERE Id = 277;
UPDATE monsters_spawns_dungeons SET TeleportCell = 271 WHERE Id = 282;
UPDATE monsters_spawns_dungeons SET TeleportCell = 271 WHERE Id = 333;
UPDATE monsters_spawns_dungeons SET TeleportCell = 327 WHERE Id = 396;
UPDATE monsters_spawns_dungeons SET TeleportCell = 273 WHERE Id = 302;
UPDATE monsters_spawns_dungeons SET TeleportCell = 271 WHERE Id = 397;
UPDATE monsters_spawns_dungeons SET TeleportCell = 271 WHERE Id = 336;
UPDATE monsters_spawns_dungeons SET TeleportCell = 271 WHERE Id = 403;
UPDATE monsters_spawns_dungeons SET TeleportCell = 273 WHERE Id = 408;
UPDATE monsters_spawns_dungeons SET TeleportCell = 342 WHERE Id = 413;
UPDATE monsters_spawns_dungeons SET TeleportCell = 271 WHERE Id = 307;
UPDATE monsters_spawns_dungeons SET TeleportCell = 271 WHERE Id = 359;
UPDATE monsters_spawns_dungeons SET TeleportCell = 271 WHERE Id = 423;
UPDATE monsters_spawns_dungeons SET TeleportCell = 271 WHERE Id = 438;
UPDATE monsters_spawns_dungeons SET TeleportCell = 273 WHERE Id = 443;
-- donjons absents de la table dungeons (d'après Dungeons.d2o)
INSERT INTO dungeons (Id, Name, NameId, MapsIdsCSV, OptimalPlayerLevel, FirstRoom, EntranceMapId, ExitMapId) VALUES (106, 'Manoir des Katrepat', 726799, '182714368,182715392,182716416,182717440,182718464', 200, 182714368, 179831808, 179831808);
INSERT INTO dungeons (Id, Name, NameId, MapsIdsCSV, OptimalPlayerLevel, FirstRoom, EntranceMapId, ExitMapId) VALUES (107, 'Belvédère d''Ilyzaelle', 732919, '184690945,184689921,184688385,184687361,184686337', 200, 184690945, 183501836, 183501836);
INSERT INTO dungeons (Id, Name, NameId, MapsIdsCSV, OptimalPlayerLevel, FirstRoom, EntranceMapId, ExitMapId) VALUES (108, 'Tour de Bethel', 744300, '187432960,187433984,187435008,187436032,187437056', 200, 187432960, 186384896, 186384896);
INSERT INTO dungeons (Id, Name, NameId, MapsIdsCSV, OptimalPlayerLevel, FirstRoom, EntranceMapId, ExitMapId) VALUES (109, 'Tour de Solar', 744768, '187957506,187957508,187957248,187957510,187957512', 200, 187957506, 183108098, 183108098);
INSERT INTO dungeons (Id, Name, NameId, MapsIdsCSV, OptimalPlayerLevel, FirstRoom, EntranceMapId, ExitMapId) VALUES (110, 'Brasserie du roi Dazak', 759633, '195035136,195036160,195037184,195038208,195039232', 200, 195035136, 194779138, 194779138);
INSERT INTO dungeons (Id, Name, NameId, MapsIdsCSV, OptimalPlayerLevel, FirstRoom, EntranceMapId, ExitMapId) VALUES (111, 'Donjon 111', 789095, '197660672,197659648,197658624,197657600,197656576', 200, 197656576, 197133314, 197133314);
INSERT INTO dungeons (Id, Name, NameId, MapsIdsCSV, OptimalPlayerLevel, FirstRoom, EntranceMapId, ExitMapId) VALUES (112, 'Donjon 112', 789947, '197394432,197395456,197396480,197397504,197398528', 130, 197394432, 197526528, 197526528);
INSERT INTO dungeons (Id, Name, NameId, MapsIdsCSV, OptimalPlayerLevel, FirstRoom, EntranceMapId, ExitMapId) VALUES (113, 'Donjon 113', 789304, '106430464,106431488,106432512,106433536,106434560', 60, 106430464, 196346371, 196346371);
