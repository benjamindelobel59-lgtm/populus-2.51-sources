-- PADGREF 23/10 : « ReferenceError #1065 : Variable ColeUL_6 is not defined » (ecran de choix du personnage, compte bloque) depuis
-- qu'un joueur a mis un APPARAT. Cause : l'import Eliocalypse (paquet eliocalypse-4) a remplace dans le jeu 19 dessins 2.51 jugés
-- inutilises par des dessins de monstres 2.74 du meme numero. Deux d'entre eux etaient ceux de nos apparats de boutique :
--   30774 (dessin 5603, « cape de lapin ») et 30781 (dessin 5610, « cape de gladiateur »).
-- Le jeu applique alors un dessin de monstre sur le personnage -> erreur et blocage.
-- Correctif provisoire (serveur seulement, pas de conflit avec les paquets du jeu en attente) : ces 2 apparats n'ont plus de dessin
-- (AppearanceId 0, comme d'autres apparats), et les dessins 5603 / 5610 sont retires des apparences enregistrees des personnages.
-- Les objets restent chez les joueurs. Les vrais dessins pourront revenir sous un autre numero (paquet du jeu, plus tard).
CREATE TABLE IF NOT EXISTS save_apparats_110 AS
  SELECT Id, LastLookString, CustomEntityLookString FROM characters
  WHERE LastLookString REGEXP '(^|[{|,])(5603|5610)([,|}]|$)' OR CustomEntityLookString REGEXP '(^|[{|,])(5603|5610)([,|}]|$)';
UPDATE items_templates SET AppearanceId = 0 WHERE Id IN (30774, 30781);
UPDATE characters SET LastLookString = REGEXP_REPLACE(REGEXP_REPLACE(REGEXP_REPLACE(LastLookString, '(?<=[|,])(5603|5610)(?=[,|}])', ''), ',(?=[,|}])', ''), '[|],', '|')
  WHERE LastLookString REGEXP '(^|[{|,])(5603|5610)([,|}]|$)';
UPDATE characters SET CustomEntityLookString = REGEXP_REPLACE(REGEXP_REPLACE(REGEXP_REPLACE(CustomEntityLookString, '(?<=[|,])(5603|5610)(?=[,|}])', ''), ',(?=[,|}])', ''), '[|],', '|')
  WHERE CustomEntityLookString REGEXP '(^|[{|,])(5603|5610)([,|}]|$)';
SELECT (SELECT COUNT(*) FROM save_apparats_110) AS personnages_corriges,
       (SELECT GROUP_CONCAT(Id) FROM save_apparats_110) AS ids,
       (SELECT COUNT(*) FROM characters WHERE LastLookString REGEXP '(^|[{|,])(5603|5610)([,|}]|$)') AS restants;
