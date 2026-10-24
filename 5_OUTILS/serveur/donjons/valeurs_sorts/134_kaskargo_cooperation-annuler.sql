-- PADGREF 23/10/2026 : annule 134_kaskargo_cooperation.sql
USE `world`;
UPDATE spells_levels SET InitialCooldown = 0 WHERE SpellId = 3305;
