-- PADGREF 23/10 SQL 144 : Repaire des Pandikazes.
-- Salle 2 (17304576, 2e plate-forme) : 2 plates-formes non reliées ; les monstres apparaissaient parfois sur celle de droite -> combat
-- impossible. Monstres (case 185), arrivée du joueur (case 354) et placements de combat tous sur la plate-forme de gauche.
UPDATE world_maps SET BlueCellsCSV = '172,175,197,242,282', RedCellsCSV = '145,202,212,238,311,42,43,56' WHERE Id = 17304576;
UPDATE monsters_spawns_dungeons SET CellId = 185 WHERE Id = 215;
UPDATE monsters_spawns_dungeons SET TeleportCell = 354 WHERE Id = 214;
-- Salle 4 (17308672, 4e plate-forme) : 1 seule case bleue par côté (4 monstres max) : haut 35, gauche 266, droite 279, bas 511.
UPDATE world_maps SET BlueCellsCSV = '35,266,279,511' WHERE Id = 17308672;
