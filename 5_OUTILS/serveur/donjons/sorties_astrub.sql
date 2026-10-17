-- NEKFEU : donjons dont le boss renvoyait a Astrub (191105026). Genere par _OUTILS/donjons/generer_sorties_astrub.py. Serveur World ARRETE.
-- 4 Centre du Labyrinthe du Minotoror
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (4, 34473476, 34476294, 286, 1);
INSERT INTO npcs_spawns (Id, NpcId, MapId, CellId, Direction, Npc) VALUES (900300, 800, 34473476, 300, 1, 'Nekfeu sortie donjon 4');
UPDATE monsters_spawns_dungeons SET TeleportMapId = 34473476, TeleportCell = 271 WHERE Id = 208; -- avant 191105026 / 273
-- 35 Laboratoire de Brumen Tinctorias
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (35, 176952320, 177079296, 282, 1);
INSERT INTO npcs_spawns (Id, NpcId, MapId, CellId, Direction, Npc) VALUES (900301, 800, 176952320, 300, 1, 'Nekfeu sortie donjon 35');
UPDATE monsters_spawns_dungeons SET TeleportMapId = 176952320, TeleportCell = 271 WHERE Id = 114; -- avant 191105026 / 260
-- 81 Ring du Capitaine Ekarlatte
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (81, 136579072, 134221826, 273, 1);
INSERT INTO npcs_spawns (Id, NpcId, MapId, CellId, Direction, Npc) VALUES (900302, 800, 136579072, 286, 1, 'Nekfeu sortie donjon 81');
UPDATE monsters_spawns_dungeons SET TeleportMapId = 136579072, TeleportCell = 271 WHERE Id = 245; -- avant 191105026 / NULL
-- 31 Antre du Kralamoure Geant
UPDATE monsters_spawns_dungeons SET TeleportMapId = 162056, TeleportCell = 318 WHERE Id = 322; -- avant 191105026 / 273
