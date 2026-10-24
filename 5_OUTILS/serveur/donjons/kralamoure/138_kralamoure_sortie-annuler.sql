-- PADGREF 23/10/2026 : annule 138_kralamoure_sortie.sql
USE `world`;
UPDATE monsters_spawns_dungeons SET TeleportMapId = 990000011, TeleportCell = -1 WHERE MapId = 26740736 AND TeleportMapId = 162056 AND TeleportCell = 318;
