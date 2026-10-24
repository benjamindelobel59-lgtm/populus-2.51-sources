-- PADGREF 24/10 SQL 153 : variantes de classe oubliées par le SQL 149 : lançables sur une case vide (client : SpellLevels.d2o + Spells.d2o,
-- zone affichée en entier). Agular 9921 (Zobal), puis choix utilisateur : Flèche Écrasante 9308, Flèche Assaillante 9310 (Crâ),
-- Rassemblement 9632 (Iop).
UPDATE spells_levels SET NeedTakenCell = 0 WHERE Id IN (35520, 34783, 34785, 35178);
