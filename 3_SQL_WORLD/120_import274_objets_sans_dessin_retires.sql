-- PADGREF 23/10/2026 : SQL 120 — import 2.74 : objets déjà en jeu dont le dessin porté n'a pas été trouvé (demande utilisateur :
-- « supprime les objets non faits, dans un dossier, pour qu'ils ne soient pas en jeu ») : Cape de Glourdorak (32237, panoplie 1046)
-- et Manteau de Servitude (22201, Cavalier Servitude, panoplie 476). On retire tout ce qui permet de les obtenir (butin, recettes,
-- panoplie) ; les fiches d'objet restent (un joueur qui en a déjà un ne doit pas être bloqué à la connexion).
-- Pour les remettre : objets_en_attente/remettre_32237_22201.sql. Base world. Rejouable.
START TRANSACTION;
DELETE FROM monsters_drops WHERE ItemId IN (32237, 22201);
DELETE FROM recipes WHERE Id IN (32237, 22201);
UPDATE items_sets SET ItemsCSV = '22200,22202' WHERE Id = 476;
UPDATE items_sets SET ItemsCSV = '32238,32239' WHERE Id = 1046;
COMMIT;
