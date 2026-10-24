-- PADGREF 24/10 SQL 165 (retour utilisateur : « les maisons à Pandala n'ont pas fonctionné ») : les 10 portes de Pandala avaient
-- encore leur ancienne compétence « Téléportation » (entrée libre vers l'intérieur) en plus des compétences de maison du SQL 159.
-- On la retire : la porte n'a plus que Entrer / Acheter / Vendre / Verrouiller / Déverrouiller, comme toutes les maisons.
-- Maison 1100 (porte 446242, 13090) : intérieur officiel = celui de son ancienne téléportation (8126464, avant 8128512).
DELETE FROM interactives_spawns_skills WHERE InteractiveSpawnId = 446242 AND SkillId = 757;
DELETE FROM interactives_spawns_skills WHERE InteractiveSpawnId = 437865 AND SkillId = 754;
DELETE FROM interactives_spawns_skills WHERE InteractiveSpawnId = 437866 AND SkillId = 749;
DELETE FROM interactives_spawns_skills WHERE InteractiveSpawnId = 446252 AND SkillId = 3000054;
DELETE FROM interactives_spawns_skills WHERE InteractiveSpawnId = 437856 AND SkillId = 751;
DELETE FROM interactives_spawns_skills WHERE InteractiveSpawnId = 437867 AND SkillId = 750;
DELETE FROM interactives_spawns_skills WHERE InteractiveSpawnId = 437857 AND SkillId = 1519;
DELETE FROM interactives_spawns_skills WHERE InteractiveSpawnId = 437868 AND SkillId = 1520;
DELETE FROM interactives_spawns_skills WHERE InteractiveSpawnId = 437862 AND SkillId = 1514;
DELETE FROM interactives_spawns_skills WHERE InteractiveSpawnId = 437869 AND SkillId = 3500018;
UPDATE houses SET InteriorMapId = 8126464 WHERE Id = 1100;
