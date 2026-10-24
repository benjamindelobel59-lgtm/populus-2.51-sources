-- PADGREF 23/10/2026 : SQL 134 (retour utilisateur : « Kaskargo lance [Coopération Baveuse] au premier tour, le joueur est OS ») :
-- Coopération Baveuse (3305 : échange de place, souvent sur la glyphe de Bave = mort au début du tour) pas avant le tour 2
-- (règle : « le Kaskargo ne peut pas lancer ce sort au premier tour »). Concerne aussi Kaskapointhe et Gary. Base world. Rejouable.
USE `world`;
UPDATE spells_levels SET InitialCooldown = 1 WHERE SpellId = 3305;
