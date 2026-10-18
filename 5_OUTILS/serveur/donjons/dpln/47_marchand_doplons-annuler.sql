-- MYSTIC 18/10 : annulation de 47_marchand_doplons.sql
DELETE FROM npcs_spawns WHERE NpcId=42 AND MapId=191105026;
DELETE i FROM npcs_items i JOIN npcs_actions a ON a.Id=i.NpcShopId WHERE a.NpcId=42 AND a.Type='Shop' AND a.Parameter0='13052';
DELETE FROM npcs_actions WHERE NpcId=42 AND Type='Shop' AND Parameter0='13052';
UPDATE npcs_templates SET Name='Zeurg', LookAsString='{42}', TokenShop=0 WHERE Id=42;
DELETE FROM items_templates WHERE Id=31018;
