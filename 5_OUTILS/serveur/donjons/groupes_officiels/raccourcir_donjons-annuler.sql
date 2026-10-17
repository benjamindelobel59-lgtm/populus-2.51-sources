-- MYSTIC 17/10 : annule raccourcir_donjons.sql (remet l'enchaînement d'origine des salles)
UPDATE monsters_spawns_dungeons SET TeleportMapId=57154305, TeleportCell=252, TeleportDirection=7 WHERE Id=286;
UPDATE monsters_spawns_dungeons SET TeleportMapId=72352768, TeleportCell=309, TeleportDirection=7 WHERE Id=155;
UPDATE monsters_spawns_dungeons SET TeleportMapId=72354816, TeleportCell=254, TeleportDirection=7 WHERE Id=157;
UPDATE monsters_spawns_dungeons SET TeleportMapId=182325249, TeleportCell=NULL, TeleportDirection=7 WHERE Id=314;
