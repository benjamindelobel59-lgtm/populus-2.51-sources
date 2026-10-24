-- PADGREF 23/10 SQL 140 (donjon du Pere Ver) : Pikténia (4727) Gyroskopik 7387 / Pik a Saut 7388 et Morsquale (4730, 4766) Charge Croquante 7847.
-- Aux grades 2/3+ la base donnait les niveaux 2 a 5 de ces sorts : ce sont des sous-sorts internes (glyphe seule, degats seuls,
-- resistances, avancee seule) que le niveau 1 relance lui-meme. Le vrai sort (echange + glyphe, saut, charge) est le niveau 1.
UPDATE monsters_spells s JOIN monsters_grades g ON g.Id = s.MonsterGradeId SET s.Level = 1
WHERE ((g.MonsterId = 4727 AND s.SpellId IN (7387, 7388)) OR (g.MonsterId IN (4730, 4766) AND s.SpellId = 7847)) AND s.Level > 1;
