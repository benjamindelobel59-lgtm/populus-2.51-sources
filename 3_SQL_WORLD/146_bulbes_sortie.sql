-- PADGREF 23/10 SQL 146 : Caverne des Bulbes. La dernière salle envoyait vers la salle de sortie générique (990000008, sans le bon PNJ).
-- Comme en 2.51 : la dernière salle mène à 17566977 (case 150), où Bulbig Brozeur (PNJ 701) propose « Quitter la caverne » et
-- « Apprendre le sort Capture d'âmes et quitter la caverne » (réponse 19633 du client : sort 413 + téléportation dehors).
UPDATE monsters_spawns_dungeons SET TeleportMapId = 17566977, TeleportCell = 150 WHERE Id = 50;
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId, Criteria, Parameter0, Parameter1, Parameter2) VALUES
 (902413, 'AddSpell', 19633, 2888, NULL, '413', NULL, NULL),
 (902414, 'Teleport', 19633, 2888, NULL, '10006', '272', '1');
INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection)
 SELECT 10, 17566977, 10006, 272, 1 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM dungeons_exits WHERE DungeonId = 10 AND EndMapId = 17566977);
