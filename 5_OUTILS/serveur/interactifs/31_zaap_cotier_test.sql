-- MYSTIC 17/10/2026 : TEST zaap du Village côtier / Routes Rocailleuses (517181, map 164364304 case 271) toujours non cliquable
-- après le SQL 27 (action ZaapTeleport bien liée). Hypothèse : comme les portails Eliotrope, l'image de zaap 91640 envoyée
-- « animée » (Animated=1) n'est pas cliquable dans le client ; les zaaps d'origine qui marchent sont presque tous à 0.
-- Ce script ne touche QUE ce zaap. Relancer le World puis tester. Pour annuler : même requête avec Animated = 1.
UPDATE interactives_spawns SET Animated = 0 WHERE Id = 517181;
SELECT Id, TemplateId, MapId, CellId, ElementId, Animated FROM interactives_spawns WHERE Id = 517181;
