-- Annule 80_doublage_tour1.sql : délai initial officiel 2.51 (3 tours) remis.
UPDATE spells_levels SET InitialCooldown = 3 WHERE SpellId = 4777 AND Id IN (22582, 33697);
