-- PADGREF 23/10/2026 : SQL 125 — noms français des objets importés (NameId 9000000+ : recherchés de Frigost, Eliocalypse, apparats
-- 2.74…) ajoutés à la table langs quand ils y manquent : la recherche d'objets du panel admin (PopulusAdmin) passe par langs et ne
-- les trouvait pas. Le nom vient de la fiche de l'objet (colonne Name). Textes existants jamais modifiés. Base world. Rejouable.
SET NAMES utf8mb4;
INSERT IGNORE INTO langs (Id, French)
SELECT NameId, Name FROM items_templates WHERE NameId >= 9000000 AND Name IS NOT NULL AND Name <> ''
UNION
SELECT NameId, Name FROM items_templates_weapons WHERE NameId >= 9000000 AND Name IS NOT NULL AND Name <> '';
