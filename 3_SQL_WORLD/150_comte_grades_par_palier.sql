-- PADGREF 23/10 SQL 150 (retour utilisateur : « boss lancé à 5 joueurs mais les monstres ont les grades de la version à 8 ; vérifie 6/7 »).
-- Donjon du Comte Harebourg, système modulaire : les accompagnateurs étaient au grade 5 à tous les paliers (seuls les boss suivaient le
-- palier). Tous les monstres suivent maintenant le palier comme le Comte : 1-4 joueurs = grade 1, 5 = grade 2, 6 = grade 3, 7 = grade 4,
-- 8 = grade 5. Salles 1 à 5 (97010, 382, 383, 380, 384) et duos de boss (97001-97004) ; l'arène des 5 boss réunis (97005) ne change pas.
UPDATE monsters_spawns_dungeons_groups g
 JOIN monsters_grades cur ON cur.Id = g.MonsterGradeId
 JOIN monsters_grades cible ON cible.MonsterId = cur.MonsterId
  AND cible.GradeId = CASE g.MinPartyMembers WHEN 5 THEN 2 WHEN 6 THEN 3 WHEN 7 THEN 4 WHEN 8 THEN 5 ELSE 1 END
 SET g.MonsterGradeId = cible.Id
 WHERE g.DungeonSpawnId IN (97010,382,383,380,384,97001,97002,97003,97004);
