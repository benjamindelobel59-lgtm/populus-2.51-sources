-- NEKFEU 2026-10-16 : Antichambre des Gloursons (donjon 57) selon tofus.fr (captures des salles et du boss).
-- Entree (Professeur Xa, cle 11181) -> Deuxieme salle 62131720. Premiere salle 62130696 = passage sans combat.
-- 2e (341, grade 1) -> 3e 62132744 (342, g2) -> 4e 62133768 (343, g3) -> 5e 62134792 (344, g4)
--   -> Chambre du Gloursaleste 62135816 (nouveau combat, g5) -> Sortie 62136840.
-- Avant : le boss etait dans le combat de la 5e salle, qui envoyait directement a la sortie (chambre du boss sautee).
-- grades : Aperiglours 2861 g1..5 = 6443..6447, Boulglours 2862 = 6448..6452, Gloursaya 2863 = 6453..6457,
--          Glourseleste 2864 g5 = 6462, Glourmand 2987 = 6995..6999, Glouragan 2988 = 7000..7004, Meliglours 2989 = 7005..7009
DELETE g FROM monsters_spawns_dungeons_groups g JOIN monsters_spawns_dungeons s ON s.Id=g.DungeonSpawnId
 WHERE s.Id IN (340,341,342,343,344) OR s.Descripcion='NEKFEU glourseleste boss';
DELETE FROM monsters_spawns_dungeons WHERE Id=340 OR Descripcion='NEKFEU glourseleste boss';
INSERT INTO monsters_spawns_dungeons (MapId, CellId, TeleportEvent, TeleportMapId, TeleportCell, TeleportDirection, Descripcion)
 VALUES (62135816, NULL, 1, 62136840, NULL, 7, 'NEKFEU glourseleste boss');
UPDATE monsters_spawns_dungeons SET TeleportMapId=62135816 WHERE Id=344;
DROP TEMPORARY TABLE IF EXISTS gl_compo;
CREATE TEMPORARY TABLE gl_compo (SpawnId INT, Pos INT, GradeId INT);
INSERT INTO gl_compo VALUES
 (341,1,6995),(341,2,6448),(341,3,6443),(341,4,6453),(341,5,6995),(341,6,6448),(341,7,6443),(341,8,6453),
 (342,1,6996),(342,2,6444),(342,3,6444),(342,4,6454),(342,5,6996),(342,6,6444),(342,7,6444),(342,8,6444),
 (343,1,7002),(343,2,7002),(343,3,6450),(343,4,6997),(343,5,7002),(343,6,7002),(343,7,7002),(343,8,6450),
 (344,1,7008),(344,2,7008),(344,3,6998),(344,4,6998),(344,5,7008),(344,6,7008),(344,7,6998),(344,8,6998);
INSERT INTO gl_compo SELECT Id, 1, 6462 FROM monsters_spawns_dungeons WHERE Descripcion='NEKFEU glourseleste boss';
INSERT INTO gl_compo SELECT Id, p, g FROM monsters_spawns_dungeons
 JOIN (SELECT 2 p, 7009 g UNION ALL SELECT 3, 7004 UNION ALL SELECT 4, 6999 UNION ALL SELECT 5, 6452
       UNION ALL SELECT 6, 6447 UNION ALL SELECT 7, 6447 UNION ALL SELECT 8, 6457) b
 WHERE Descripcion='NEKFEU glourseleste boss';
DROP TEMPORARY TABLE IF EXISTS gl_paliers;
CREATE TEMPORARY TABLE gl_paliers (Palier INT, NbMonstres INT);
INSERT INTO gl_paliers VALUES (1,4),(5,5),(6,6),(7,7),(8,8);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations)
 SELECT c.SpawnId, c.GradeId, p.Palier, 'NEKFEU glourseleste'
 FROM gl_paliers p JOIN gl_compo c ON c.Pos <= p.NbMonstres ORDER BY c.SpawnId, p.Palier, c.Pos;
UPDATE dungeons_exits SET DungeonId=57 WHERE EndMapId=62136840;
