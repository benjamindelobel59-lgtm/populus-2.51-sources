-- PADGREF 23/10 annulation du SQL 147
UPDATE dungeons_entries SET KeyItemId = 7312, FirstRoomMapId = 16516867, FirstRoomCellId = -1 WHERE Id = 12;
DELETE FROM dungeons_entries WHERE DungeonId = 17 AND EntranceMapId = 16516608;
UPDATE world_maps SET SpawnDisabled = 0 WHERE Id IN (16515072,16515584,16515585,16515841,16516096,16516097,16516353,16516608,16516867);
UPDATE monsters_spawns SET IsDisabled = 0 WHERE Id = 738;
DELETE FROM monsters_spawns_dungeons_groups WHERE DungeonSpawnId = 900170;
DELETE FROM monsters_spawns_dungeons WHERE Id = 900170;
DELETE FROM monsters_spawns_dungeons_groups WHERE DungeonSpawnId = 900171;
DELETE FROM monsters_spawns_dungeons WHERE Id = 900171;
DELETE FROM monsters_spawns_dungeons_groups WHERE DungeonSpawnId = 900172;
DELETE FROM monsters_spawns_dungeons WHERE Id = 900172;
DELETE FROM monsters_spawns_dungeons_groups WHERE DungeonSpawnId = 900173;
DELETE FROM monsters_spawns_dungeons WHERE Id = 900173;
DELETE FROM monsters_spawns_dungeons_groups WHERE DungeonSpawnId = 900174;
DELETE FROM monsters_spawns_dungeons WHERE Id = 900174;
DELETE FROM monsters_spawns_dungeons_groups WHERE DungeonSpawnId = 900175;
DELETE FROM monsters_spawns_dungeons WHERE Id = 900175;
DELETE FROM npcs_replies WHERE Id BETWEEN 902420 AND 902433;
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId, Criteria, Parameter0, Parameter1, Parameter2, Parameter3, Parameter4, AdditionalParameters) VALUES
 (309,'DeleteItem',6640,7073,'PO=16025&PO=16026&PO=16027&PO=16028&PO=16029','16025','1','','','',''),
 (310,'DeleteItem',6640,7073,'PO=16025&PO=16026&PO=16027&PO=16028&PO=16029','16026','1','','','',''),
 (311,'DeleteItem',6640,7073,'PO=16025&PO=16026&PO=16027&PO=16028&PO=16029','16027','1','','','',''),
 (312,'AddItem',6640,7073,'PO=16025&PO=16026&PO=16027&PO=16028&PO=16029','13010','1','','','',''),
 (313,'DeleteItem',6640,7073,'PO=16025&PO=16026&PO=16027&PO=16028&PO=16029','16028','1','','','',''),
 (314,'DeleteItem',6640,7073,'PO=16025&PO=16026&PO=16027&PO=16028&PO=16029','16029','1','','','','');
INSERT IGNORE INTO interactives_spawns_skills (Id, InteractiveSpawnId, SkillId) VALUES (3000052, 443784, 3000052);
