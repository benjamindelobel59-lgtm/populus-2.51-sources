-- PADGREF 24/10 SQL 163 (demande utilisateur : « refais le donjon du Maître Corbac en entier : 5 salles, 3 normales, 1 avec
-- Capsaloque puis le boss ») : les 5 salles existaient (159121408 -> 159122432 -> 159123456 -> 159123460 Capsaloque -> 159125512
-- Maître Corbac -> sortie 159121666) mais l'entrée envoyait sur l'ancienne carte du labyrinthe 159124486 (sans monstres, pas reliée
-- à la salle 1). Entrée et reprise -> salle 1 ; liste des cartes du donjon = les 5 salles + la sortie.
UPDATE dungeons SET FirstRoom = 159121408, MapsIdsCSV = '159121408,159122432,159123456,159123460,159125512,159121666' WHERE Id = 2;
UPDATE dungeons_entries SET FirstRoomMapId = 159121408, FirstRoomCellId = -1 WHERE DungeonId = 2;
