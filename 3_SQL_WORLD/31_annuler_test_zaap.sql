-- MYSTIC 17/10/2026 : ANNULE le test 31 du 17/10 (zaap 517181 en -20,-20 « Routes Rocailleuses » passé non animé par erreur :
-- le zaap côtier signalé est en -46,18). À jouer si l'ancien 31_zaap_cotier_test.sql a été joué sur le VPS (sans risque sinon).
UPDATE interactives_spawns SET Animated = 1 WHERE Id = 517181;
SELECT Id, MapId, Animated FROM interactives_spawns WHERE Id = 517181;
