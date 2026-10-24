-- PADGREF 20/10 : Trône de la Cour Sombre — après le combat contre la Reine des Voleurs (spawn 391, carte 137102336),
-- téléportation sur la carte 137103360 (demande utilisateur ; avant : salle de sortie copiée 990000012).
-- Base : world. Effet au prochain démarrage du World.
UPDATE monsters_spawns_dungeons SET TeleportMapId = 137103360, TeleportCell = -1 WHERE Id = 391;
SELECT Id, MapId, TeleportMapId, TeleportCell FROM monsters_spawns_dungeons WHERE Id = 391;
