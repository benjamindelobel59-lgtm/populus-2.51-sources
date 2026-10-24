-- Annulation du SQL 154 : ces salles reviennent au placement aléatoire
DELETE FROM padgref_placements_fixes WHERE DungeonSpawnId IN (300,301,332,333,312,319,510,359,339,349,354,389,9714,428,443,418,398,384,9719,374,379,423,413,390,369,9709,408,433,438,364,391,9704,403,396,397);
