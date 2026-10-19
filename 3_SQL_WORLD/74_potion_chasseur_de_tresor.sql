-- MYSTIC 19/10/2026 : SQL 74 — la Potion de Chasseur de Trésor (15236) redevient normale : plus de potion VIP (code VipPotion désactivé),
-- elle téléporte dans le bâtiment des chasseurs de trésors (Malle aux Trésors, carte 128452097, case 489) et se consomme.
-- Base world, World ARRÊTÉ. Rejouable.
SET NAMES utf8mb4;
USE `world`;
START TRANSACTION;
DELETE FROM items_teleport_potions WHERE ItemId = 15236;
INSERT INTO items_teleport_potions (ItemId, MapId, CellId, Orientation) VALUES (15236, 128452097, 489, 1);
COMMIT;
SELECT * FROM items_teleport_potions WHERE ItemId = 15236;
