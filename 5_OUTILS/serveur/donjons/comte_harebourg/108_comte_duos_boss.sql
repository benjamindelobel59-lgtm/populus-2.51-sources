-- PADGREF 23/10 (demande utilisateur) : DUOS DE BOSS du donjon du Comte Harebourg.
-- Dorleans (PNJ 2008) de l'entree propose « Monter en haut de la tour. » (reponse 17826) une fois le succes 892 « Donjon du Comte »
-- obtenu -> salle de Dorleans (112202241). La, Dorleans (message 15169) propose : le Comte seul (Grande aiguille 112203523) ou
-- avec Nileza / Sylargh / Klime / Missiz Frizz (4 arenes du sommet de la tour, jamais utilisees : decor choisi a l'oeil ;
-- Nileza = globes de verre). Chaque arene : Comte + partenaire, grade 1 a 5 selon le nombre de joueurs (comme la Grande
-- aiguille) ; victoire -> sortie du Comte (112204547). « Descendre. » (18021) ramene a l'entree.
-- Identifiants fixes 907001+ / 97001+ (pas de conflit avec les autres paquets).

-- entree : « Monter en haut de la tour. » (succes 892 obligatoire)
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId, Criteria, Parameter0, Parameter1, Parameter2) VALUES (907001, 'Teleport', 17826, 15157, 'OA=892', '112202241', '466', '1');
-- Dorleans dans sa salle
INSERT INTO npcs_spawns (Id, NpcId, MapId, CellId, Direction, Npc) VALUES (907001, 2008, 112202241, 437, 1, 'Dorleans - duos de boss du Comte');
INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES (907001, 2008, 'Talk', 'Mp=112202241', 0, '15169');
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId, Criteria, Parameter0, Parameter1, Parameter2) VALUES
  (907002, 'Teleport', 17827, 15169, NULL, '112203523', '468', '7'),
  (907003, 'Teleport', 17828, 15169, NULL, '112203271', '468', '7'),   -- Nileza
  (907004, 'Teleport', 17829, 15169, NULL, '112203521', '468', '7'),   -- Sylargh
  (907005, 'Teleport', 17830, 15169, NULL, '112203265', '468', '7'),   -- Klime
  (907006, 'Teleport', 17831, 15169, NULL, '112203269', '468', '7'),   -- Missiz Frizz
  (907007, 'Teleport', 18021, 15169, NULL, '112460048', '470', '1');   -- Descendre.

-- arenes des duos
INSERT INTO monsters_spawns_dungeons (Id, MapId, CellId, TeleportEvent, TeleportMapId, TeleportCell, TeleportDirection, Descripcion) VALUES (97001, 112203271, NULL, 1, 112204547, 271, 7, 'Donjon du Comte Harebourg - duo Comte + Nileza - Sortie');
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (97001, 8493, 1, 'PADGREF duo Comte + Nileza'), (97001, 8388, 1, 'PADGREF duo Comte + Nileza'), (97001, 8494, 5, 'PADGREF duo Comte + Nileza'), (97001, 8389, 5, 'PADGREF duo Comte + Nileza'), (97001, 8495, 6, 'PADGREF duo Comte + Nileza'), (97001, 8390, 6, 'PADGREF duo Comte + Nileza'), (97001, 8496, 7, 'PADGREF duo Comte + Nileza'), (97001, 8391, 7, 'PADGREF duo Comte + Nileza'), (97001, 8497, 8, 'PADGREF duo Comte + Nileza'), (97001, 8392, 8, 'PADGREF duo Comte + Nileza');
INSERT INTO monsters_spawns_dungeons (Id, MapId, CellId, TeleportEvent, TeleportMapId, TeleportCell, TeleportDirection, Descripcion) VALUES (97002, 112203521, NULL, 1, 112204547, 271, 7, 'Donjon du Comte Harebourg - duo Comte + Sylargh - Sortie');
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (97002, 8493, 1, 'PADGREF duo Comte + Sylargh'), (97002, 8456, 1, 'PADGREF duo Comte + Sylargh'), (97002, 8494, 5, 'PADGREF duo Comte + Sylargh'), (97002, 8457, 5, 'PADGREF duo Comte + Sylargh'), (97002, 8495, 6, 'PADGREF duo Comte + Sylargh'), (97002, 8458, 6, 'PADGREF duo Comte + Sylargh'), (97002, 8496, 7, 'PADGREF duo Comte + Sylargh'), (97002, 8459, 7, 'PADGREF duo Comte + Sylargh'), (97002, 8497, 8, 'PADGREF duo Comte + Sylargh'), (97002, 8460, 8, 'PADGREF duo Comte + Sylargh');
INSERT INTO monsters_spawns_dungeons (Id, MapId, CellId, TeleportEvent, TeleportMapId, TeleportCell, TeleportDirection, Descripcion) VALUES (97003, 112203265, NULL, 1, 112204547, 271, 7, 'Donjon du Comte Harebourg - duo Comte + Klime - Sortie');
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (97003, 8493, 1, 'PADGREF duo Comte + Klime'), (97003, 8328, 1, 'PADGREF duo Comte + Klime'), (97003, 8494, 5, 'PADGREF duo Comte + Klime'), (97003, 8329, 5, 'PADGREF duo Comte + Klime'), (97003, 8495, 6, 'PADGREF duo Comte + Klime'), (97003, 8330, 6, 'PADGREF duo Comte + Klime'), (97003, 8496, 7, 'PADGREF duo Comte + Klime'), (97003, 8331, 7, 'PADGREF duo Comte + Klime'), (97003, 8497, 8, 'PADGREF duo Comte + Klime'), (97003, 8332, 8, 'PADGREF duo Comte + Klime');
INSERT INTO monsters_spawns_dungeons (Id, MapId, CellId, TeleportEvent, TeleportMapId, TeleportCell, TeleportDirection, Descripcion) VALUES (97004, 112203269, NULL, 1, 112204547, 271, 7, 'Donjon du Comte Harebourg - duo Comte + Missiz Frizz - Sortie');
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (97004, 8493, 1, 'PADGREF duo Comte + Missiz Frizz'), (97004, 8358, 1, 'PADGREF duo Comte + Missiz Frizz'), (97004, 8494, 5, 'PADGREF duo Comte + Missiz Frizz'), (97004, 8359, 5, 'PADGREF duo Comte + Missiz Frizz'), (97004, 8495, 6, 'PADGREF duo Comte + Missiz Frizz'), (97004, 8360, 6, 'PADGREF duo Comte + Missiz Frizz'), (97004, 8496, 7, 'PADGREF duo Comte + Missiz Frizz'), (97004, 8361, 7, 'PADGREF duo Comte + Missiz Frizz'), (97004, 8497, 8, 'PADGREF duo Comte + Missiz Frizz'), (97004, 8362, 8, 'PADGREF duo Comte + Missiz Frizz');
SELECT 'duos Comte Harebourg installes' AS resultat, (SELECT COUNT(*) FROM monsters_spawns_dungeons_groups WHERE DungeonSpawnId BETWEEN 97001 AND 97004) AS monstres;
