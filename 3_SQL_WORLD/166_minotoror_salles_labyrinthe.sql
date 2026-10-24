-- PADGREF 24/10 SQL 166 (retour utilisateur : « Minotoror, les salles 1, 2 et 3 ne sont pas du donjon, mets des salles du
-- labyrinthe du Minotoror à la place ») : les 3 salles du SQL 161 passent sur des salles du Labyrinthe du Minotoror (sous-zone 319),
-- choix utilisateur : salles du labyrinthe elles-mêmes (les promeneurs du labyrinthe voient les groupes du donjon et peuvent les
-- attaquer sans clé). 34472960 [-38,-13] (salle 1) -> 34475008 [-40,-13] (salle 2) -> 34342152 [-36,-17] (salle 3) -> Minotoror.
UPDATE monsters_spawns_dungeons SET MapId = 34472960, TeleportMapId = 34475008 WHERE Id = 900180;
UPDATE monsters_spawns_dungeons SET MapId = 34475008, TeleportMapId = 34342152 WHERE Id = 900181;
UPDATE monsters_spawns_dungeons SET MapId = 34342152, TeleportMapId = 34473474 WHERE Id = 900182;
UPDATE dungeons SET FirstRoom = 34472960, MapsIdsCSV = '34472960,34475008,34342152,34473474,34473476' WHERE Id = 3;
UPDATE dungeons_entries SET FirstRoomMapId = 34472960, FirstRoomCellId = -1 WHERE DungeonId = 3;
