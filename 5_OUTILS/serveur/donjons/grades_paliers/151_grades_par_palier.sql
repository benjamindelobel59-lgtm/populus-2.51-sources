-- PADGREF 23/10 SQL 151 (demande utilisateur : « fais ça pour tous les donjons, pas que le Comte »).
-- Système modulaire : dans TOUTES les salles de donjon à plusieurs paliers (501 salles), le grade de chaque monstre suit le palier :
-- 1-4 joueurs = grade 1, 5 = grade 2, 6 = grade 3, 7 = grade 4, 8 = grade 5. Avant : grade fixe (souvent 1, parfois 3 ou 5) quel que
-- soit le nombre de joueurs. Les salles à un seul palier (arène des 5 boss…) ne changent pas.
-- Les grades d'avant sont d'abord recopiés dans padgref_sauvegarde_151 (utilisée par l'annulation).
CREATE TABLE IF NOT EXISTS padgref_sauvegarde_151 (Id INT NOT NULL PRIMARY KEY, MonsterGradeId INT NOT NULL);
CREATE TEMPORARY TABLE padgref_151_salles AS
 SELECT DungeonSpawnId FROM monsters_spawns_dungeons_groups GROUP BY DungeonSpawnId HAVING COUNT(DISTINCT MinPartyMembers) > 1;
INSERT IGNORE INTO padgref_sauvegarde_151 (Id, MonsterGradeId)
 SELECT g.Id, g.MonsterGradeId FROM monsters_spawns_dungeons_groups g JOIN padgref_151_salles s ON s.DungeonSpawnId = g.DungeonSpawnId;
UPDATE monsters_spawns_dungeons_groups g
 JOIN padgref_151_salles s ON s.DungeonSpawnId = g.DungeonSpawnId
 JOIN monsters_grades cur ON cur.Id = g.MonsterGradeId
 JOIN monsters_grades cible ON cible.MonsterId = cur.MonsterId
  AND cible.GradeId = CASE g.MinPartyMembers WHEN 5 THEN 2 WHEN 6 THEN 3 WHEN 7 THEN 4 WHEN 8 THEN 5 ELSE 1 END
 SET g.MonsterGradeId = cible.Id;
DROP TEMPORARY TABLE padgref_151_salles;
