-- MYSTIC 17/10/2026 : tous les métiers niveau 200 pour les personnages déjà créés (les nouveaux les ont à la création, DLL 17h50+).
-- À passer World ARRÊTÉ (sinon les personnages connectés réécrivent leur ancienne expérience). Rejouable.
-- Expérience du niveau 200 lue dans la table experiences ; métier 1 (Base) exclu.
SET @xp200 = (SELECT JobExp FROM experiences WHERE Level = 200);
UPDATE characters_jobs SET Experience = @xp200 WHERE TemplateId <> 1 AND Experience < @xp200;
INSERT INTO characters_jobs (OwnerId, TemplateId, Experience, WorkForFree, MinLevelCraftSetting)
SELECT c.Id, j.Id, @xp200, 0, 1
FROM characters c CROSS JOIN jobs_templates j
WHERE j.Id <> 1
  AND NOT EXISTS (SELECT 1 FROM characters_jobs cj WHERE cj.OwnerId = c.Id AND cj.TemplateId = j.Id);
-- Contrôle : metiers_sous_200 = 0, lignes = personnages x 19
SELECT (SELECT COUNT(*) FROM characters_jobs WHERE TemplateId <> 1 AND Experience < @xp200) AS metiers_sous_200,
       (SELECT COUNT(*) FROM characters_jobs WHERE TemplateId <> 1) AS lignes,
       (SELECT COUNT(*) FROM characters) * (SELECT COUNT(*) FROM jobs_templates WHERE Id <> 1) AS attendu;
