-- PADGREF 23/10 : annule 114 (retour à l'enchaînement du SQL 112 : Dorléans après la salle 1)
START TRANSACTION;
UPDATE monsters_spawns_dungeons SET TeleportMapId = 112202241, TeleportCell = 466 WHERE Id = 97010;
UPDATE monsters_spawns_dungeons SET TeleportMapId = 112203523, TeleportCell = NULL WHERE Id = 380;
UPDATE npcs_replies SET Parameter0 = '112199169', Parameter1 = '314', Parameter2 = '1' WHERE Id = 907002;
COMMIT;
