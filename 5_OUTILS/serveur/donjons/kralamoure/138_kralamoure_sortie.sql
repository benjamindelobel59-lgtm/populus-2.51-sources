-- PADGREF 23/10/2026 : SQL 138 (retour utilisateur : « quand on finit le Kralamoure on doit réapparaître devant, pas de map de sortie »).
-- Antre du Kralamoure Géant (donjon 27) : la salle du boss 26740736 envoyait après la victoire dans la salle de sortie 990000011 ;
-- elle envoie maintenant directement devant le donjon (162056, case 318 = sortie déjà enregistrée dans dungeons_exits). Rejouable.
USE `world`;
UPDATE monsters_spawns_dungeons SET TeleportMapId = 162056, TeleportCell = 318 WHERE MapId = 26740736 AND TeleportMapId = 990000011;
