-- PADGREF 21/10 : SQL 90 - enclos (salle commune 78905344) : retire les 2 teleporteurs de case croises des passages 3 et 4
-- (case 382 = passage 3 envoyait vers l'enclos 4, case 386 = passage 4 vers l'enclos 3, en plus du passage normal).
-- Les passages (PaddockHub) gardent leur case : enclos 3 et 4 corrects, avec la verification de la porte d'entree. Rejouable.
USE `world`;
DELETE FROM world_maps_triggers WHERE Id IN (3000013, 3000871);
