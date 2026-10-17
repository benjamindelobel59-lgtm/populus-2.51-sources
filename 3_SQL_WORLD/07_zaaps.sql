-- NEKFEU 2026-10-16 : 37 zaaps sur 72 sans action (non cliquables, ex. Village cotier / Routes Rocailleuses 164364304, Astrub, Chateau d'Amakna).
-- Le modele Zaap (16) n'a pas d'action par defaut : on relie l'action partagee ZaapTeleport (interactives_skills Id 1),
-- comme les 35 zaaps qui marchent, y compris les zaaps des havre-sacs (demande utilisateur). Rejouable.
-- Ids des liens = 4000000 + Id du zaap (hors de la plage 3000000-3999999 reservee a portes_worldgraph.sql).
DELETE FROM interactives_spawns_skills WHERE SkillId = 1 AND Id BETWEEN 3000000 AND 3999999;
INSERT IGNORE INTO interactives_spawns_skills (Id, InteractiveSpawnId, SkillId)
 SELECT 4000000 + s.Id, s.Id, 1 FROM interactives_spawns s
 JOIN world_maps m ON m.Id = s.MapId
 JOIN world_subareas sa ON sa.Id = m.SubAreaId
 LEFT JOIN langs l ON l.Id = sa.NameId
 WHERE s.TemplateId = 16

   AND NOT EXISTS (SELECT 1 FROM interactives_spawns_skills ss WHERE ss.InteractiveSpawnId = s.Id);
-- les prochains Ids automatiques (panel admin) hors des plages reservees
ALTER TABLE interactives_spawns_skills AUTO_INCREMENT = 5000000;
ALTER TABLE interactives_skills AUTO_INCREMENT = 5000000;
ALTER TABLE world_maps_triggers AUTO_INCREMENT = 5000000;
