-- MYSTIC 17/10/2026 : ateliers de forgemagie. Les 29 postes des salles de forgemagie étaient tous en « Atelier magique » (117 =
-- Joaillomagie). Type rendu d'après l'image du poste dans les cartes du client (elements.ele -> gfx 14606 à 14609) :
--   27628 / 27645 (gfx 14606, enclume ailée)        -> 92  Enclume magique (Forgemagie, Sculptemagie, Bouclier)
--   27629 / 27646 (gfx 14607, métier à tisser)      -> 116 Machine à coudre magique (Couturemagie)
--   27630 / 27647 (gfx 14608, table cuir / chapeau) -> 118 Table magique (Cordomagie)
--   27631 / 27648 (gfx 14609, table loupe / bijoux) -> 117 Atelier magique (Joaillomagie, inchangé)
-- Rejouable. Nécessite un redémarrage du World.
UPDATE interactives_spawns SET TemplateId = 92  WHERE TemplateId IN (92, 116, 117, 118) AND ElementId IN (27628, 27645);
UPDATE interactives_spawns SET TemplateId = 116 WHERE TemplateId IN (92, 116, 117, 118) AND ElementId IN (27629, 27646);
UPDATE interactives_spawns SET TemplateId = 118 WHERE TemplateId IN (92, 116, 117, 118) AND ElementId IN (27630, 27647);
UPDATE interactives_spawns SET TemplateId = 117 WHERE TemplateId IN (92, 116, 117, 118) AND ElementId IN (27631, 27648);
-- Contrôle : attendu 92 = 8, 116 = 7, 117 = 7, 118 = 7
SELECT TemplateId, COUNT(*) FROM interactives_spawns WHERE TemplateId IN (92, 116, 117, 118) GROUP BY TemplateId;
