-- PADGREF 23/10 SQL 147 : Donjon des Firefoux comme en 2.51 (retour utilisateur : « on arrive directement devant le Péki Péki ; il doit y
-- avoir 6 salles, chaque salle donne 1 artefact farmable à l'infini, puis une salle avec un PNJ qui prend les 6 artefacts »).
-- Worldgraph 2.51 : entrée 16515072 -> 6 salles reliées par les bords -> 16516608 (passage : clef 7312 + 6 Firefacts 7373-7378) -> 16516867
-- (Péki Péki) -> 16515331 (sortie).
-- 1) Entrée chez le gardien : gratuite, vers 16515072 (la clef est donnée à Dikaprio).
UPDATE dungeons_entries SET KeyItemId = 0, FirstRoomMapId = 16515072, FirstRoomCellId = -1 WHERE Id = 12;
-- 2) Dikaprio (16516608) : nouvelle « entrée » du donjon 17 vers Péki Péki, clef 7312.
INSERT INTO dungeons_entries (DungeonId, EntranceMapId, KeyItemId, FirstRoomMapId, FirstRoomCellId, KeeperCellId) VALUES (17, 16516608, 7312, 16516867, -1, 343);
-- 3) 6 salles : un groupe de Firefoux (réapparaît après chaque combat, pas de téléportation) ; chaque gagnant reçoit le Firefact de la salle, sans limite.
UPDATE world_maps SET SpawnDisabled = 1 WHERE Id IN (16515072,16515584,16515585,16515841,16516096,16516097,16516353,16516608,16516867);
UPDATE monsters_spawns SET IsDisabled = 1 WHERE Id = 738;   -- Péki Péki (boss) en groupe aléatoire de la sous-zone 144
INSERT INTO monsters_spawns_dungeons (Id, MapId, CellId, TeleportEvent, TeleportMapId, TeleportCell, TeleportDirection, Descripcion, RewardItemId, RewardMax) VALUES (900170, 16515584, NULL, 0, 0, -1, 1, 'PADGREF Firefoux salle 1 (Premier Firefact)', 7374, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (900170, 1781, 1, 'PADGREF Firefoux salle 1'), (900170, 1786, 1, 'PADGREF Firefoux salle 1'), (900170, 1791, 1, 'PADGREF Firefoux salle 1'), (900170, 1816, 1, 'PADGREF Firefoux salle 1'), (900170, 1783, 4, 'PADGREF Firefoux salle 1'), (900170, 1793, 6, 'PADGREF Firefoux salle 1'), (900170, 1788, 8, 'PADGREF Firefoux salle 1');
INSERT INTO monsters_spawns_dungeons (Id, MapId, CellId, TeleportEvent, TeleportMapId, TeleportCell, TeleportDirection, Descripcion, RewardItemId, RewardMax) VALUES (900171, 16515841, NULL, 0, 0, -1, 1, 'PADGREF Firefoux salle 2 (Deuxieme Firefact)', 7373, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (900171, 1781, 1, 'PADGREF Firefoux salle 2'), (900171, 1786, 1, 'PADGREF Firefoux salle 2'), (900171, 1791, 1, 'PADGREF Firefoux salle 2'), (900171, 1816, 1, 'PADGREF Firefoux salle 2'), (900171, 1783, 4, 'PADGREF Firefoux salle 2'), (900171, 1793, 6, 'PADGREF Firefoux salle 2'), (900171, 1788, 8, 'PADGREF Firefoux salle 2');
INSERT INTO monsters_spawns_dungeons (Id, MapId, CellId, TeleportEvent, TeleportMapId, TeleportCell, TeleportDirection, Descripcion, RewardItemId, RewardMax) VALUES (900172, 16516353, NULL, 0, 0, -1, 1, 'PADGREF Firefoux salle 3 (Troisieme Firefact)', 7375, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (900172, 1781, 1, 'PADGREF Firefoux salle 3'), (900172, 1786, 1, 'PADGREF Firefoux salle 3'), (900172, 1791, 1, 'PADGREF Firefoux salle 3'), (900172, 1816, 1, 'PADGREF Firefoux salle 3'), (900172, 1783, 4, 'PADGREF Firefoux salle 3'), (900172, 1793, 6, 'PADGREF Firefoux salle 3'), (900172, 1788, 8, 'PADGREF Firefoux salle 3');
INSERT INTO monsters_spawns_dungeons (Id, MapId, CellId, TeleportEvent, TeleportMapId, TeleportCell, TeleportDirection, Descripcion, RewardItemId, RewardMax) VALUES (900173, 16516096, NULL, 0, 0, -1, 1, 'PADGREF Firefoux salle 4 (Quatrieme Firefact)', 7377, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (900173, 1781, 1, 'PADGREF Firefoux salle 4'), (900173, 1786, 1, 'PADGREF Firefoux salle 4'), (900173, 1791, 1, 'PADGREF Firefoux salle 4'), (900173, 2075, 1, 'PADGREF Firefoux salle 4'), (900173, 1783, 4, 'PADGREF Firefoux salle 4'), (900173, 1793, 6, 'PADGREF Firefoux salle 4'), (900173, 1788, 8, 'PADGREF Firefoux salle 4');
INSERT INTO monsters_spawns_dungeons (Id, MapId, CellId, TeleportEvent, TeleportMapId, TeleportCell, TeleportDirection, Descripcion, RewardItemId, RewardMax) VALUES (900174, 16516097, NULL, 0, 0, -1, 1, 'PADGREF Firefoux salle 5 (Cinquieme Firefact)', 7376, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (900174, 1781, 1, 'PADGREF Firefoux salle 5'), (900174, 1786, 1, 'PADGREF Firefoux salle 5'), (900174, 1791, 1, 'PADGREF Firefoux salle 5'), (900174, 2075, 1, 'PADGREF Firefoux salle 5'), (900174, 1783, 4, 'PADGREF Firefoux salle 5'), (900174, 1793, 6, 'PADGREF Firefoux salle 5'), (900174, 1788, 8, 'PADGREF Firefoux salle 5');
INSERT INTO monsters_spawns_dungeons (Id, MapId, CellId, TeleportEvent, TeleportMapId, TeleportCell, TeleportDirection, Descripcion, RewardItemId, RewardMax) VALUES (900175, 16515585, NULL, 0, 0, -1, 1, 'PADGREF Firefoux salle 6 (Sixieme Firefact)', 7378, NULL);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (900175, 1781, 1, 'PADGREF Firefoux salle 6'), (900175, 1786, 1, 'PADGREF Firefoux salle 6'), (900175, 1791, 1, 'PADGREF Firefoux salle 6'), (900175, 2075, 1, 'PADGREF Firefoux salle 6'), (900175, 1783, 4, 'PADGREF Firefoux salle 6'), (900175, 1793, 6, 'PADGREF Firefoux salle 6'), (900175, 1788, 8, 'PADGREF Firefoux salle 6');
-- 4) Dikaprio : « Donner les reliques, la clef et entrer » / « …le trousseau et entrer » (ses réponses officielles 6640 / 6641) ;
--    avant : réponses d'un autre PNJ (reliques 16025-16029 -> objet 13010).
DELETE FROM npcs_replies WHERE Id BETWEEN 309 AND 314;
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId, Criteria, Parameter0, Parameter1, Parameter2) VALUES
 (902420, 'DeleteItem', 6640, 7073, 'PO=7312&PO=7373&PO=7374&PO=7375&PO=7376&PO=7377&PO=7378', '7373', '1', NULL),
 (902421, 'DeleteItem', 6640, 7073, 'PO=7312&PO=7373&PO=7374&PO=7375&PO=7376&PO=7377&PO=7378', '7374', '1', NULL),
 (902422, 'DeleteItem', 6640, 7073, 'PO=7312&PO=7373&PO=7374&PO=7375&PO=7376&PO=7377&PO=7378', '7375', '1', NULL),
 (902423, 'DeleteItem', 6640, 7073, 'PO=7312&PO=7373&PO=7374&PO=7375&PO=7376&PO=7377&PO=7378', '7376', '1', NULL),
 (902424, 'DeleteItem', 6640, 7073, 'PO=7312&PO=7373&PO=7374&PO=7375&PO=7376&PO=7377&PO=7378', '7377', '1', NULL),
 (902425, 'DeleteItem', 6640, 7073, 'PO=7312&PO=7373&PO=7374&PO=7375&PO=7376&PO=7377&PO=7378', '7378', '1', NULL),
 (902426, 'DungeonEnter', 6640, 7073, 'PO=7312&PO=7373&PO=7374&PO=7375&PO=7376&PO=7377&PO=7378', '17', NULL, NULL),
 (902427, 'DeleteItem', 6641, 7073, 'PO=7373&PO=7374&PO=7375&PO=7376&PO=7377&PO=7378', '7373', '1', NULL),
 (902428, 'DeleteItem', 6641, 7073, 'PO=7373&PO=7374&PO=7375&PO=7376&PO=7377&PO=7378', '7374', '1', NULL),
 (902429, 'DeleteItem', 6641, 7073, 'PO=7373&PO=7374&PO=7375&PO=7376&PO=7377&PO=7378', '7375', '1', NULL),
 (902430, 'DeleteItem', 6641, 7073, 'PO=7373&PO=7374&PO=7375&PO=7376&PO=7377&PO=7378', '7376', '1', NULL),
 (902431, 'DeleteItem', 6641, 7073, 'PO=7373&PO=7374&PO=7375&PO=7376&PO=7377&PO=7378', '7377', '1', NULL),
 (902432, 'DeleteItem', 6641, 7073, 'PO=7373&PO=7374&PO=7375&PO=7376&PO=7377&PO=7378', '7378', '1', NULL),
 (902433, 'DungeonEnterTrousseau', 6641, 7073, 'PO=7373&PO=7374&PO=7375&PO=7376&PO=7377&PO=7378', '17', NULL, NULL);
-- 5) La porte 443784 (16516608 -> 16516867) laissait passer sans clef ni Firefact : retirée (on passe par Dikaprio).
DELETE FROM interactives_spawns_skills WHERE InteractiveSpawnId = 443784 AND SkillId = 3000052;
