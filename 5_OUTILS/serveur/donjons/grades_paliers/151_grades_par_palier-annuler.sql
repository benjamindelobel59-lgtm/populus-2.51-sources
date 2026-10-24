-- PADGREF 23/10 annulation du SQL 151 : remet les grades recopiés par le SQL 151 dans padgref_sauvegarde_151.
UPDATE monsters_spawns_dungeons_groups g JOIN padgref_sauvegarde_151 p ON p.Id = g.Id SET g.MonsterGradeId = p.MonsterGradeId;
DROP TABLE padgref_sauvegarde_151;
