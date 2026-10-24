-- PADGREF 23/10 SQL 149 (demande utilisateur) : sorts de classe à ZONE qui exigeaient une cible (case occupée) : lançables sur une case
-- vide, la zone s'affiche directement dans le client (SpellLevels.d2o modifié aussi, 64 et 32 bits).
-- Poussière Temporelle 96, Perception 113, Mot Sélectif 130, Flèche Ralentissante 177, Flèche Explosive 179, Aimantation 2801, Fulgurance 9956.
UPDATE spells_levels SET NeedTakenCell = 0 WHERE Id IN (476,477,478,561,562,563,646,647,648,881,882,883,891,892,893,18330,18331,18332,35577,35578,35579);
