-- MYSTIC 17/10 : donjons limités à 4 salles de combat + salle du boss (consigne utilisateur « sinon trop long »).
-- Choix utilisateur « Garder les salles officielles » : on saute les salles de remplissage (sans groupe sur tofus).
-- Chaque sortie modifiée reprend la case d'arrivée qu'utilisait la salle sautée pour aller au même endroit.
-- Les salles sautées ne sont pas supprimées (plus atteintes). Rejouable. Annulation : raccourcir_donjons-annuler.sql
-- Hypogée de l'Obsidiantre : 446, 284, 285, 286 + boss 288 (saute 287 = Cinquième salle)
UPDATE monsters_spawns_dungeons SET TeleportMapId=57155841, TeleportCell=252, TeleportDirection=7 WHERE Id=286;
-- Antre du Dragon Cochon : 155 (salle d'entrée, gardée), 157, 159 + boss 444 (saute 156 et 158, remplissage)
UPDATE monsters_spawns_dungeons SET TeleportMapId=72353792, TeleportCell=227, TeleportDirection=7 WHERE Id=155;
UPDATE monsters_spawns_dungeons SET TeleportMapId=72355840, TeleportCell=254, TeleportDirection=7 WHERE Id=157;
-- Temple du Grand Ougah : 313, 314, 317, 318 + boss 319 (saute 315 et 316, remplissage)
UPDATE monsters_spawns_dungeons SET TeleportMapId=182323201, TeleportCell=NULL, TeleportDirection=7 WHERE Id=314;
