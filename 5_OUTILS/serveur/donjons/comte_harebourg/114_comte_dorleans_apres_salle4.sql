-- PADGREF 23/10/2026 : SQL 114 — donjon du Comte Harebourg : la salle de Dorléans (Salle de la Tocante 112202241) vient APRÈS LA
-- SALLE 4 (correction de l'utilisateur ; le SQL 112 la mettait après la salle 1). Parcours : Hall des Sinistros -> Banquet ->
-- Galerie des glaçons -> Atelier d'horlogerie -> Dorléans -> au choix : « le Comte seul » -> Grande aiguille, un duo de boss ou les
-- 5 boss réunis (succès du Comte). Base world. Rejouable.
START TRANSACTION;
UPDATE monsters_spawns_dungeons SET TeleportMapId = 112199169, TeleportCell = NULL WHERE Id = 97010;
UPDATE monsters_spawns_dungeons SET TeleportMapId = 112202241, TeleportCell = 466 WHERE Id = 380;
UPDATE npcs_replies SET Parameter0 = '112203523', Parameter1 = '468', Parameter2 = '7' WHERE Id = 907002;
COMMIT;
