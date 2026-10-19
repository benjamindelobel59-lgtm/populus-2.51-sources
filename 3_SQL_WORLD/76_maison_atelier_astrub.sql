-- MYSTIC 19/10/2026 : SQL 76 — maison-atelier d'Astrub [5,-17] (maison 785) : intérieur 191890437 = atelier des Lacs enchantés
-- + 6 ateliers de craft + 4 ateliers de forgemagie (FM limitée au niveau 100 dans le code). Généré par
-- _OUTILS/client/client-build/atelier_astrub/construire.py. Base world, World ARRÊTÉ. Rejouable. Client : maps*.d2p (carte 191890437).
SET NAMES utf8mb4;
USE `world`;
START TRANSACTION;
DELETE FROM interactives_spawns_skills WHERE InteractiveSpawnId IN (SELECT Id FROM interactives_spawns WHERE MapId = 191890437);
DELETE FROM interactives_spawns WHERE MapId = 191890437 OR Id BETWEEN 517300 AND 517399;
DELETE FROM world_maps_triggers WHERE MapId = 191890437;
UPDATE world_maps SET CompressedCells = 0x1f8b08000000000002ff45d877d4d0631407f0f73eed487bd15eb453591529d2a4cc9286969154a4ac322ad156465448282aca6821a2a1149a1489942885484592e3c5fbf9fdf539f7dce73eb7f336be3d191919194d32fefd021366c3ec980373622ecc8d79302f9e907961e67722e6d37012e6d75040b52016c2c258048b62312c8e25b0a4db4ec653b094ded25806cb6239c7caab56c08a1a2aa956c62a78aaded3b02a56c3ea58036b1a564bb5b66a1d3c1deb623dac8f671876269e8567e339d8001b622373cf553d4fb5319eafa1896a53bc40c385d80c2fc2e6d8025b1ad60a5b6b68a37ab1ea25d856433bbc54c36578395e8157e255d8deb00e783576d47b0d76c2ced8c5b1ae78ad866ed81d7b604fec65c275783ddea0f746ec8d37611fc76e56ed8bfdb03fde82b7e200c36ec3813848efed7807de8977e1dd260cc6211aeec17bf13ebdf7e3501ca677383e8023f0417c08471a364a75348ec1b138ceb1f1f8304ed03b111fc147f1317c1c27e113e63ea93a19a768988a4fe1d37a9fc169f82c4ec7e7f0797c0167b862a6ea8bf812ced23b1be76878195fc1b9380f5fc5d7f0757c03e7e3025c688745b858c39bf816be8d4bf01d7c17979afb9eeafbb80c97e30a5c69c207aaab70357e886b706d66c6c8fc3e52fd183fc175b81e37e046eb6c52dd8c9fe267b805b7e2e7f88525b7b9e24b0ddbf12bfc1a77e037b81377e1b7b81bbfc3efffc962c733b9c73a7b33ff41cdfc7ed0bb0ff7e38ff813fe8c07f017fc150fe26f76386487c378047f77ec0f3c8a7fe231fc0b8f673132303065dd16d954b3abe6c09c982beb671cb91dcb837951fe0df937f2e14998dfdc026e2b8885b030cabf51d4b062581c4ba0fc1bf26fc8bf510a4b63191797cdf8ff8b72585e4305ac68c94a8655c62a28ff86fc1bf26f54735b7557d450ad89b5b0b6de3a28ff465d57d4c3fa28ff86fc1bf26fc8bf714ed61fbd68e0b686d669a4f75c0de7616313e4df68824d51fe0df9379a1926ff4673d516d669e9775f2bbdadf5b6d17b315ee2585bc7daa1fc1bf26fc8bf71852baec4abb0bd2b3a3876b5868ed6917fa39386ce7ec65d3474c56b5dd1cdb1eed8037beaed6582fc1bd75b47fe0df9377aeb957fa30fcabfd1d7c5fdb0bf2b6e51957f6380256f531da8779009f26fdc6133f937e4dfb8dbdcc1860d31ec1ebc5783fc1bf26f0cd530ccdce11ae4df18a12aff86fc1b237194ded1d61983635d3c0ec7a3fc1b131c9b68c94734c8bf21ff86fc1b93f4cabf21ffc664574cb1c354d5a7549f36e1998cbcf1dfdf0fd354e5df98ae2aff86fc1bf26fccc0997e992fa2fc1bb3ac33db927354e5df907f63ae75e6a1fc1bf26fbcae41fe8df9d659800bedb048ef62ebc8bff196b9f26f2c51957f43fe8da528ff86fc1bcb4c586eb31576586949f9375669586d82fc1b6b54d7a2fc1bf26fc8bfb14eef7adce0b68d7a37d96cb306f9373e33610b6e45f937bec06d28ffc676c7be42f93776d841fe8d9daabb1c937f63b7ea77f8bd863db817e5dfd8e7d87e947f43fe0df9370ee8957fe357d58328ffc621bd87f108cabff187634751fe8d63aaf26f1ccfaa26efbfc9fb6ff2fe9bbcffa6ec5913520ecc89b9d0fb6ff2fe9bbcff26f937c9bfc9fb6ff2fe9bf2a3f7df54d0b14258188ba0fc9be4dfe4fd3779ff4d2551fe4df26ff2fe9bbcff26efbfa9ac5eefbfa93c5640efbf49fe4dde7f93f7df24ff26f93755d5500daba3f7df5413bdffa6da59ff35fa1bb4b7be7210180000, BlueCellsCSV = (SELECT b FROM (SELECT BlueCellsCSV b FROM world_maps WHERE Id = 85461511) x), RedCellsCSV = (SELECT r FROM (SELECT RedCellsCSV r FROM world_maps WHERE Id = 85461511) y) WHERE Id = 191890437;
INSERT INTO interactives_spawns (Id, TemplateId, MapId, CellId, ElementId, Animated) VALUES (517300, 94, 191890437, 217, 27635, 1);
INSERT INTO interactives_spawns (Id, TemplateId, MapId, CellId, ElementId, Animated) VALUES (517302, 94, 191890437, 258, 27651, 1);
INSERT INTO interactives_spawns (Id, TemplateId, MapId, CellId, ElementId, Animated) VALUES (517303, 97, 191890437, 285, 27624, 1);
INSERT INTO interactives_spawns (Id, TemplateId, MapId, CellId, ElementId, Animated) VALUES (517304, 41, 191890437, 305, 27589, 1);
INSERT INTO interactives_spawns (Id, TemplateId, MapId, CellId, ElementId, Animated) VALUES (517305, 97, 191890437, 325, 27640, 1);
INSERT INTO interactives_spawns (Id, TemplateId, MapId, CellId, ElementId, Animated) VALUES (517306, 27, 191890437, 334, 27587, 1);
INSERT INTO interactives_spawns (Id, TemplateId, MapId, CellId, ElementId, Animated) VALUES (517307, 22, 191890437, 352, 27582, 1);
INSERT INTO interactives_spawns (Id, TemplateId, MapId, CellId, ElementId, Animated) VALUES (517308, 90, 191890437, 387, 27739, 1);
INSERT INTO interactives_spawns (Id, TemplateId, MapId, CellId, ElementId, Animated) VALUES (517309, 50, 191890437, 454, 27650, 1);
INSERT INTO interactives_spawns (Id, TemplateId, MapId, CellId, ElementId, Animated) VALUES (517310, 2, 191890437, 481, 27653, 1);
INSERT INTO interactives_spawns (Id, TemplateId, MapId, CellId, ElementId, Animated) VALUES (517311, 11, 191890437, 388, 27627, 1);
INSERT INTO interactives_spawns (Id, TemplateId, MapId, CellId, ElementId, Animated) VALUES (517312, 12, 191890437, 408, 27620, 1);
INSERT INTO interactives_spawns (Id, TemplateId, MapId, CellId, ElementId, Animated) VALUES (517313, 13, 191890437, 415, 27590, 1);
INSERT INTO interactives_spawns (Id, TemplateId, MapId, CellId, ElementId, Animated) VALUES (517314, 57, 191890437, 245, 27632, 1);
INSERT INTO interactives_spawns (Id, TemplateId, MapId, CellId, ElementId, Animated) VALUES (517315, 58, 191890437, 276, 27592, 1);
INSERT INTO interactives_spawns (Id, TemplateId, MapId, CellId, ElementId, Animated) VALUES (517316, 122, 191890437, 347, 27626, 1);
INSERT INTO interactives_spawns (Id, TemplateId, MapId, CellId, ElementId, Animated) VALUES (517317, 92, 191890437, 358, 27645, 1);
INSERT INTO interactives_spawns (Id, TemplateId, MapId, CellId, ElementId, Animated) VALUES (517318, 116, 191890437, 384, 27629, 1);
INSERT INTO interactives_spawns (Id, TemplateId, MapId, CellId, ElementId, Animated) VALUES (517319, 117, 191890437, 400, 27648, 1);
INSERT INTO interactives_spawns (Id, TemplateId, MapId, CellId, ElementId, Animated) VALUES (517320, 118, 191890437, 409, 27630, 1);
INSERT INTO world_maps_triggers (Type, CellId, MapId, TriggerType, Parameter0, Parameter1) VALUES ('HouseExit', 234, 191890437, 0, 222, 191105028);
COMMIT;
SELECT Id, TemplateId, CellId, ElementId FROM interactives_spawns WHERE MapId = 191890437 ORDER BY Id;
