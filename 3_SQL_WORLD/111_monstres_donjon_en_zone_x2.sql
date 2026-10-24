-- PADGREF 23/10/2026 : SQL 111 — monstres de donjon en zone (SQL 95, monsters_spawns 9500001+) : taux d'apparition x2
-- (Frequency 0,30 -> 0,60). Demande utilisateur. Base world. Rejouable.
UPDATE monsters_spawns SET Frequency = 0.60 WHERE Id BETWEEN 9500001 AND 9599999;
