-- NEKFEU 2026-10-16 : salles de SORTIE sans monstres pour les 12 donjons dont la salle finale = salle du boss
-- (copies des salles de boss, ids 990000001-990000012, ajoutees aussi au client : maps*.d2p + MapPositions.d2o,
--  outil _OUTILS/client-build/dupliquer_maps_client.py). Apres le boss : teleportation dans la copie, ou se trouvent
-- le Portier (PNJ 800, « Sortir du donjon ») et la sortie. A passer APRES 08_sorties_salles_boss.sql. Rejouable.
DROP TEMPORARY TABLE IF EXISTS sortie_copies;
CREATE TEMPORARY TABLE sortie_copies (Orig INT PRIMARY KEY, Copie INT);
INSERT INTO sortie_copies VALUES
 (190318592, 990000001),
 (193729536, 990000002),
 (121374211, 990000003),
 (163582464, 990000004),
 (87033346, 990000005),
 (96209922, 990000006),
 (146670592, 990000007),
 (17565953, 990000008),
 (87296515, 990000009),
 (166990850, 990000010),
 (26740736, 990000011),
 (137102336, 990000012);
-- maps serveur (cases, placements, version identiques ; pas de voisins ; pas de monstres)
DELETE FROM world_maps WHERE Id IN (SELECT Copie FROM sortie_copies);
INSERT INTO world_maps (Id,Version,RelativeId,MapType,SubAreaId,TopNeighbourId,BottomNeighbourId,LeftNeighbourId,RightNeighbourId,
  TopNeighbourCellId,BottomNeighbourCellId,LeftNeighbourCellId,RightNeighbourCellId,ClientTopNeighbourId,ClientBottomNeighbourId,
  ClientLeftNeighbourId,ClientRightNeighbourId,ShadowBonusOnEntities,UseLowpassFilter,UseReverb,PresetId,BlueCellsCSV,RedCellsCSV,
  CompressedCells,SpawnDisabled,IsInstantiated)
 SELECT c.Copie,m.Version,m.RelativeId,m.MapType,m.SubAreaId,-1,-1,-1,-1,NULL,NULL,NULL,NULL,-1,-1,-1,-1,
  m.ShadowBonusOnEntities,m.UseLowpassFilter,m.UseReverb,m.PresetId,m.BlueCellsCSV,m.RedCellsCSV,m.CompressedCells,1,m.IsInstantiated
 FROM world_maps m JOIN sortie_copies c ON c.Orig = m.Id;
DELETE FROM world_maps_positions WHERE Id IN (SELECT Copie FROM sortie_copies);
INSERT INTO world_maps_positions (Id,PosX,PosY,Outdoor,SubAreaId,Capabilities,WorldMap,NameId,HasPriorityOnWorldmap)
 SELECT c.Copie,p.PosX,p.PosY,p.Outdoor,p.SubAreaId,p.Capabilities,p.WorldMap,p.NameId,p.HasPriorityOnWorldmap
 FROM world_maps_positions p JOIN sortie_copies c ON c.Orig = p.Id;
-- combat du boss : de nouveau une teleportation, vers la copie (centre de la salle)
UPDATE monsters_spawns_dungeons s JOIN sortie_copies c ON c.Orig = s.MapId
 SET s.TeleportEvent = 1, s.TeleportMapId = c.Copie, s.TeleportCell = -1
 WHERE s.Id IN (10,15,20,25,35,40,45,50,60,89,322,391);
-- Portier et sortie deplaces dans la copie
UPDATE npcs_spawns n JOIN sortie_copies c ON c.Orig = n.MapId SET n.MapId = c.Copie WHERE n.Id BETWEEN 900600 AND 900699;
UPDATE dungeons_exits e JOIN sortie_copies c ON c.Orig = e.EndMapId SET e.EndMapId = c.Copie;
-- la copie fait partie du donjon
UPDATE dungeons d JOIN sortie_copies c ON FIND_IN_SET(c.Orig, d.MapsIdsCSV) AND NOT FIND_IN_SET(c.Copie, d.MapsIdsCSV)
 SET d.MapsIdsCSV = CONCAT(d.MapsIdsCSV, ',', c.Copie);
