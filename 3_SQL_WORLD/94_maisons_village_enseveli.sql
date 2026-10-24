-- PADGREF 22/10 (retour utilisateur, Village enseveli) : 3 cartes d'intérieur des maisons du Village enseveli n'avaient AUCUNE
-- sortie (59770880 : 13 maisons, 59770882 : 13, 59773440 : 12) -> le joueur restait bloqué dedans. Sorties ajoutées sur les 2 cases
-- centrales du mur du bas, comme les 4 autres intérieurs de la même série (59768832 : 396/410…). La sortie ramène devant la porte
-- par laquelle on est entré (HouseExitTrigger -> LeaveHouse).
-- + demande utilisateur : la maison de la carte 54174537 (maison 11) coûte 599 999 999 kamas.
-- Base world. Rejouable. Annulation : 94_maisons_village_enseveli-annuler.sql
DELETE FROM world_maps_triggers WHERE Id BETWEEN 5100001 AND 5100006;
INSERT INTO world_maps_triggers (Id, Type, CellId, MapId, TriggerType, `Condition`, Parameter0, Parameter1) VALUES
(5100001, 'HouseExit', 396, 59770880, 0, NULL, NULL, NULL),
(5100002, 'HouseExit', 410, 59770880, 0, NULL, NULL, NULL),
(5100003, 'HouseExit', 396, 59770882, 0, NULL, NULL, NULL),
(5100004, 'HouseExit', 410, 59770882, 0, NULL, NULL, NULL),
(5100005, 'HouseExit', 369, 59773440, 0, NULL, NULL, NULL),
(5100006, 'HouseExit', 383, 59773440, 0, NULL, NULL, NULL);
UPDATE houses SET Price = 599999999 WHERE Id = 11 AND DoorMapId = 54174537;
