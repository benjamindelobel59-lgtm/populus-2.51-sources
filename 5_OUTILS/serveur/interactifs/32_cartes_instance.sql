-- MYSTIC 17/10/2026 : zaap du Village côtier / Routes Rocailleuses (map 164364304) inutilisable sur le VPS, et « comme une instance »
-- sur les cartes autour (signalé). Une carte IsInstantiated = 1 affiche « Cette carte est soumise à une instance temporelle… »,
-- cache les autres joueurs et peut cacher des objets interactifs. En local, seules ces 8 cartes de quête le sont :
-- 152306690,153093380,152307712,152305664,152307714,153092354,152043777,152306688
-- Ce script remet toutes les autres cartes en normal (rejouable). Lire d'abord le 1er SELECT (cartes concernées sur le VPS).
SELECT Id, SubAreaId FROM world_maps WHERE IsInstantiated = 1 AND Id NOT IN (152306690,153093380,152307712,152305664,152307714,153092354,152043777,152306688);
UPDATE world_maps SET IsInstantiated = 0 WHERE IsInstantiated = 1 AND Id NOT IN (152306690,153093380,152307712,152305664,152307714,153092354,152043777,152306688);
-- Contrôle : attendu zaap_cotier_instance = 0, cartes_instance = 8
SELECT (SELECT IsInstantiated FROM world_maps WHERE Id = 164364304) AS zaap_cotier_instance,
       (SELECT COUNT(*) FROM world_maps WHERE IsInstantiated = 1) AS cartes_instance;
