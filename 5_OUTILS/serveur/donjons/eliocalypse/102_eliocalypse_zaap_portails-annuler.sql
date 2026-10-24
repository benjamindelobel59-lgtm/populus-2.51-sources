-- PADGREF 23/10 : annule 102_eliocalypse_zaap_portails.sql
DELETE FROM interactives_spawns_skills WHERE Id BETWEEN 5100200 AND 5100210;
DELETE FROM interactives_skills WHERE Id BETWEEN 5100200 AND 5100210;
DELETE FROM interactives_spawns WHERE Id IN (517965, 517982, 517984, 517950, 517980);
