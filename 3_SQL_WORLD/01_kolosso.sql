-- NEKFEU 2026-10-16 : Cavernes du Kolosso (donjon 56), fonctionnement officiel (tofus.fr + textes du client)
-- Rejouable. Necessite le serveur World avec RewardItemId/RewardMax (DungeonSpawningPool) et POQ< (HasManyItemsCriterion).
--
-- Parcours : cle donnee au Professeur Xa (54164293) -> salle principale 61865984 (sans monstres)
--   4 portes -> salles des reliques (ordre libre, retour en salle principale apres le combat) :
--     Blerices 61998084 -> Relique de Blerice 11960 | Bleroms 61998082 -> Relique de Blerom 11961
--     Blerauves 61998338 -> Relique de Blerauve 11963 | Fleuros 61998340 -> Relique de Fleuro 11964
--   chaque gagnant recoit la relique de la salle (max 5) ; porte fermee a 5 reliques de la salle
--   Xa (salle principale) : 1 relique de chaque -> elles sont consommees -> 2e salle 61867008
--   couloirs sans combat (declencheurs 905/906/907) -> boss 61868036 (Kolosso + Professeur Xa) -> sortie 61869060

-- 0) Colonnes de recompense de salle
ALTER TABLE monsters_spawns_dungeons ADD COLUMN IF NOT EXISTS RewardItemId INT NULL, ADD COLUMN IF NOT EXISTS RewardMax INT NULL;

-- 1) Portes de la salle principale : utilisables, fermees a 5 reliques de la salle
INSERT IGNORE INTO interactives_spawns_skills (Id, InteractiveSpawnId, SkillId) VALUES
 (261564,458477,101228),(261566,458478,101230),(261568,458480,101232),(261570,458481,101234);
UPDATE interactives_skills SET `Condition`='POQ<11960-5' WHERE Id=101228; -- -> Blerices
UPDATE interactives_skills SET `Condition`='POQ<11961-5' WHERE Id=101230; -- -> Bleroms
UPDATE interactives_skills SET `Condition`='POQ<11963-5' WHERE Id=101232; -- -> Blerauves
UPDATE interactives_skills SET `Condition`='POQ<11964-5' WHERE Id=101234; -- -> Fleuros

-- 2) Soleils : ce sont des cases a declencheur dans le jeu officiel (pas de menu "Utiliser"),
--    geres par _OUTILS/interactifs/portes_worldgraph.sql (a passer APRES ce script)

-- 3) Pas de combat dans la salle principale (337) ni dans les couloirs (338 + ajouts 509/510 du 16/10)
DELETE g FROM monsters_spawns_dungeons_groups g JOIN monsters_spawns_dungeons s ON s.Id=g.DungeonSpawnId
 WHERE s.Id IN (337,338) OR s.Descripcion LIKE 'NEKFEU kolosso salle %';
DELETE FROM monsters_spawns_dungeons WHERE Id IN (337,338) OR Descripcion LIKE 'NEKFEU kolosso salle %';

-- 4) Salles des reliques : monstres officiels (captures tofus.fr), grade 3, 4 monstres + 1 par joueur au-dela de 4
--    grades 3 : Blerice 6903, Blerom 6535, Wolvero 6540, Croleur 6545, Blerauve 6530, Fleuro 6898
DELETE FROM monsters_spawns_dungeons_groups WHERE DungeonSpawnId IN (447,448,503,504);
DROP TEMPORARY TABLE IF EXISTS kolo_compo;
CREATE TEMPORARY TABLE kolo_compo (SpawnId INT, Pos INT, GradeId INT);
INSERT INTO kolo_compo VALUES
 (448,1,6903),(448,2,6903),(448,3,6535),(448,4,6540),(448,5,6903),(448,6,6903),(448,7,6903),(448,8,6545),
 (447,1,6535),(447,2,6535),(447,3,6535),(447,4,6545),(447,5,6535),(447,6,6535),(447,7,6535),(447,8,6540),
 (503,1,6535),(503,2,6530),(503,3,6530),(503,4,6540),(503,5,6545),(503,6,6530),(503,7,6530),(503,8,6530),
 (504,1,6535),(504,2,6545),(504,3,6898),(504,4,6898),(504,5,6540),(504,6,6898),(504,7,6898),(504,8,6898);
DROP TEMPORARY TABLE IF EXISTS kolo_paliers;
CREATE TEMPORARY TABLE kolo_paliers (Palier INT, NbMonstres INT);
INSERT INTO kolo_paliers VALUES (1,4),(5,5),(6,6),(7,7),(8,8);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations)
 SELECT c.SpawnId, c.GradeId, p.Palier, 'NEKFEU kolosso reliques'
 FROM kolo_paliers p JOIN kolo_compo c ON c.Pos <= p.NbMonstres
 ORDER BY c.SpawnId, p.Palier, c.Pos;
UPDATE monsters_spawns_dungeons SET RewardItemId=11960, RewardMax=5 WHERE Id=448;
UPDATE monsters_spawns_dungeons SET RewardItemId=11961, RewardMax=5 WHERE Id=447;
UPDATE monsters_spawns_dungeons SET RewardItemId=11963, RewardMax=5 WHERE Id=503;
UPDATE monsters_spawns_dungeons SET RewardItemId=11964, RewardMax=5 WHERE Id=504;

-- 5) Professeur Xa (1393) dans la salle principale : message 8462 (les 4 reliques)
--    reponse 8494 : 1 relique de chaque -> consommees -> 2e salle (case de placement 201)
--    reponse 8516 : message 8483 (on peut refaire une salle tant qu'on a moins de 5 reliques)
DELETE FROM npcs_actions WHERE NpcId=1393 AND Type='Talk' AND `Condition`='Mp=61865984';
INSERT INTO npcs_actions (NpcId, Type, `Condition`, Priority, Parameter0) VALUES (1393, 'Talk', 'Mp=61865984', -1, '8462');
DELETE FROM npcs_replies WHERE MessageId IN (8462, 8483);
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Criteria, Parameter0, Parameter1, Parameter2, Parameter3) VALUES
 ('Teleport', 8494, 8462, 'POQ=11960-1&POQ=11961-1&POQ=11963-1&POQ=11964-1', '61867008', '201', '1', '11960_1,11961_1,11963_1,11964_1'),
 ('Dialog', 8516, 8462, NULL, '8483', NULL, NULL, NULL);

-- 6) Entree : arrivee dans la salle principale
UPDATE dungeons_entries SET FirstRoomMapId=61865984, FirstRoomCellId=351 WHERE Id=41 AND DungeonId=56;
UPDATE npcs_replies SET Parameter0='61865984', Parameter1='351' WHERE Id=415 AND Type='Teleport';

-- 7) Combat du boss (spawn 339, map 61868036) = capture officielle (tofus.fr), grade 5 :
--    4 monstres : Kolosso 6994, Professeur Xa 7025, Blerom 6537, Croleur 6547
--    + 1 par joueur au-dela de 4 : Blerice 6905, Blerauve 6532, Wolvero 6542, Fleuro 6900
DELETE FROM monsters_spawns_dungeons_groups WHERE DungeonSpawnId=339;
DROP TEMPORARY TABLE IF EXISTS kolo_boss;
CREATE TEMPORARY TABLE kolo_boss (Pos INT, GradeId INT);
INSERT INTO kolo_boss VALUES (1,6994),(2,7025),(3,6537),(4,6547),(5,6905),(6,6532),(7,6542),(8,6900);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations)
 SELECT 339, b.GradeId, p.Palier, 'NEKFEU kolosso boss'
 FROM kolo_paliers p JOIN kolo_boss b ON b.Pos <= p.NbMonstres
 ORDER BY p.Palier, b.Pos;

-- 8) Sortie (PNJ Professeur Xa, salle 61869060) : une seule action -> 54164807 case 257
DELETE FROM npcs_replies WHERE Id=507 AND Type='Teleport' AND MessageId=8458;
DELETE FROM dungeons_exits WHERE EndMapId=61869060;
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (56, 61869060, 54164807, 257, 1);
