-- PADGREF 23/10/2026 : annule le SQL 130 (Marin frigostien, Mer Kantil).
USE `world`;
DELETE FROM npcs_replies WHERE Id IN (907101,907102);
DELETE FROM npcs_actions WHERE Id = 907101;
DELETE FROM npcs_spawns WHERE Id = 907101;
