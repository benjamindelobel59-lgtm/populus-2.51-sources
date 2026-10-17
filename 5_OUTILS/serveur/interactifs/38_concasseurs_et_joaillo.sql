-- MYSTIC 17/10/2026 : ateliers de forgemagie.
-- 1) 3 concasseurs sans compétence (dont l'île de Kramdam, carte 163055618) prenaient celles du modèle « Moulin » (Moudre /
--    Engrener). On leur donne les mêmes compétences que les autres concasseurs : Briser des objets (skill 10356 -> 181) et
--    Fusionner des ressources (skill 10357 -> 121). Spawns : 502536 (Kramdam), 457800 (carte 59769864), 464893 (carte 85459463).
-- 2) Compétence 169 (Atelier magique, sans nom dans le client : « UNKNOWN_TEXT_ID_60209 ») masquée.
-- Rejouable. Redémarrage du World nécessaire.
DELETE FROM interactives_spawns_skills WHERE InteractiveSpawnId IN (502536, 457800, 464893) AND SkillId IN (10356, 10357);
INSERT INTO interactives_spawns_skills (InteractiveSpawnId, SkillId) VALUES
  (502536, 10356), (502536, 10357),
  (457800, 10356), (457800, 10357),
  (464893, 10356), (464893, 10357);
UPDATE interactives_skills_templates SET ClientDisplay = 0 WHERE Id = 169;
-- Contrôle : attendu concasseurs_sans_briser = 0, skill169_affiche = 0
SELECT (SELECT COUNT(*) FROM interactives_spawns s WHERE s.TemplateId = 50 AND s.ElementId IN (19509, 21800, 71121)
          AND NOT EXISTS (SELECT 1 FROM interactives_spawns_skills x WHERE x.InteractiveSpawnId = s.Id AND x.SkillId = 10356)) AS concasseurs_sans_briser,
       (SELECT ClientDisplay FROM interactives_skills_templates WHERE Id = 169) AS skill169_affiche;
