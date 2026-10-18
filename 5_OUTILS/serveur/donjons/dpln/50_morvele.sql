-- MYSTIC 18/10 (demande utilisateur) : Morvèle (2518, posé par Kirbili du Professeur Xa) : +100 dommages cumulables 3 fois au maximum
-- (avant : MaxStack 0 = cumul illimité). Au plafond, le buff le plus court est remplacé.
UPDATE spells_levels SET MaxStack = 3 WHERE Id = 17021 AND SpellId = 2518;
