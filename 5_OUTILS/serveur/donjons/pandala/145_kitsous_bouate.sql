-- PADGREF 23/10 SQL 145 (Pandala).
-- 1) Kitsous : Kitsnition (529) a 5 niveaux = 5 éléments (1 Air, 2 Eau, 3 Terre, 4 Feu, 5 Neutre). La base donnait le niveau = grade :
--    le Kitsou Nakwa (Chance) frappait en Air avec 0 d'Agilité (6 à 30 de dégâts). Chaque Kitsou a maintenant le niveau de son élément
--    (celui de son Illusion spectrale) à tous les grades, archimonstres compris.
UPDATE monsters_spells s JOIN monsters_grades g ON g.Id = s.MonsterGradeId SET s.Level = 2 WHERE s.SpellId = 529 AND g.MonsterId IN (523, 2329);   -- Nakwa : Eau
UPDATE monsters_spells s JOIN monsters_grades g ON g.Id = s.MonsterGradeId SET s.Level = 3 WHERE s.SpellId = 529 AND g.MonsterId IN (530, 2511);   -- Nere : Terre
UPDATE monsters_spells s JOIN monsters_grades g ON g.Id = s.MonsterGradeId SET s.Level = 1 WHERE s.SpellId = 529 AND g.MonsterId IN (531, 2510);   -- Nae : Air
UPDATE monsters_spells s JOIN monsters_grades g ON g.Id = s.MonsterGradeId SET s.Level = 4 WHERE s.SpellId = 529 AND g.MonsterId IN (532, 2512);   -- Nufeu : Feu
-- 2) Bouate de Pandore (606) : 0 PM (demande utilisateur : elle ne doit pas bouger).
UPDATE monsters_grades SET MovementPoints = 0 WHERE MonsterId = 606;
