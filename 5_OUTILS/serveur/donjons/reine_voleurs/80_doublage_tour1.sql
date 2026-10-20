-- PADGREF 20/10 : Trône de la Cour Sombre (Reine des Voleurs) — la Doublure (3748) peut lancer Doublage (4777) dès le tour 1.
-- Choix de l'utilisateur, écart volontaire avec la 2.51 (délai initial officiel : 3 tours). La relance de 3 tours est gardée.
-- Base : world. Effet au prochain démarrage du World (ou commande du panel « reload_spell 4777 »).
UPDATE spells_levels SET InitialCooldown = 0 WHERE SpellId = 4777 AND Id IN (22582, 33697);
SELECT Id, SpellId, InitialCooldown, MinCastInterval FROM spells_levels WHERE SpellId = 4777;
