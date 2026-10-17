-- Anciens declencheurs de l'emulateur contraires au graphe officiel (liaison inexistante, ou pose sur la case d'une porte officielle)
-- detail : declencheurs_faux_detail.tsv
DELETE FROM world_maps_triggers WHERE Id IN (9,26,27,28,29,30,42,43,44,45,46,47,48,49,50,51,76,90,91,104,105,106,108,111,112,113,114,116,118,119,120,333,334,403,404,405,406,434,435,625,662,663,664,705,707,708,709,711,713,714,744,745,940,944,982,985,993,995,1034,1042,1078,1079,1152,1159,1161,1171,1176,1177,1178,1179,1181,1182,1185,1186,1189,1192,1195,1196,1197,1198,1203,1215,1229,1275,1287,1364,1480,1481,1500,1501,1525,1526,1534,1535);
-- Plus aucune condition sur les portes / escaliers / cases de passage (demande utilisateur 16/10),
-- sauf les 4 portes des salles de reliques du Kolosso (fermees a 5 reliques, kolosso.sql)
UPDATE interactives_skills SET `Condition`=NULL WHERE Type='Teleport' AND Id NOT IN (101228,101230,101232,101234) AND COALESCE(`Condition`,'') NOT REGEXP '^PG='; -- PG = classe : statues de temple (une destination par classe)
UPDATE world_maps_triggers SET `Condition`=NULL WHERE Type='Teleport';
-- Interactifs avec encore plusieurs Teleport (hors statues de classe PG=) : une seule action gardee (destination officielle sinon la premiere)
DELETE FROM interactives_spawns_skills WHERE Id IN (260438,260728,260730,260838,260847,260848,260856,260909,260920,261038,261085,261086,261098,261099,261126,261128,261131,261134,261135,261143,261144,261147,261148,261313,261375,261376,261425,261435,261456,261460,261542,261545,261552,261583,261588);
-- Statues de classe (489318 map 153092354, 489503 map 154010883) : destination Ouginak (PG=18 -> 152044553) absente,
-- a la place un 2e lien Sram (PG=15) -> le 2e lien Sram devient l'Ouginak
DELETE FROM interactives_skills WHERE Id IN (3600000, 3600001);
INSERT INTO interactives_skills (Id, Type, Duration, `Condition`, CustomTemplateId, Parameter0, Parameter1, Parameter2) VALUES
 (3600000, 'Teleport', 60, 'PG=18', NULL, '152044553', '290', '1'),
 (3600001, 'Teleport', 60, 'PG=18', NULL, '152044553', '516', '1');
UPDATE interactives_spawns_skills SET SkillId=3600000 WHERE Id=221272 AND InteractiveSpawnId=489318;
UPDATE interactives_spawns_skills SET SkillId=3600001 WHERE Id=221254 AND InteractiveSpawnId=489503;
-- Portails Eliotrope : 2 choix sur chaque portail (demande utilisateur 16/10)
--  Amakna 488048 (88082199 c274) : Se rendre au temple (200) / Se rendre a Sufokia (212)
--  Sufokia 488049 (95420418 c146) : Se rendre au temple (200) / Se rendre en Amakna (213)
--  Temple 488050 (148637187 c498) : Se rendre en Amakna (213) / Se rendre a Sufokia (212)
--  arrivee a cote du portail d'arrivee (GetArrivalCell evite la case du portail)
DELETE FROM interactives_spawns_skills WHERE InteractiveSpawnId IN (488048,488049,488050);
DELETE FROM interactives_skills WHERE Id BETWEEN 3600010 AND 3600015;
INSERT INTO interactives_skills (Id, Type, Duration, `Condition`, CustomTemplateId, Parameter0, Parameter1, Parameter2) VALUES
 (3600010, 'Teleport', 60, NULL, 200, '148637187', '498', '1'),
 (3600011, 'Teleport', 60, NULL, 212, '95420418', '146', '1'),
 (3600012, 'Teleport', 60, NULL, 200, '148637187', '498', '1'),
 (3600013, 'Teleport', 60, NULL, 213, '88082199', '274', '1'),
 (3600014, 'Teleport', 60, NULL, 213, '88082199', '274', '1'),
 (3600015, 'Teleport', 60, NULL, 212, '95420418', '146', '1');
INSERT INTO interactives_spawns_skills (Id, InteractiveSpawnId, SkillId) VALUES
 (3600010, 488048, 3600010), (3600011, 488048, 3600011),
 (3600012, 488049, 3600012), (3600013, 488049, 3600013),
 (3600014, 488050, 3600014), (3600015, 488050, 3600015);
-- Portails Eliotrope (253) / plates-formes Huppermage (280) avec une action Teleport : plus d etat anime (Animated=0). PAS les portes ordinaires (modele 0) : leur dessin depend de l etat anime (sinon porte invisible, ex. laboratoire 115606025).
-- Le client considere un element anime "en train de changer d'etat" tant que son animation AnimState<etat> n'a pas ete jouee ;
-- si le dessin n'a pas cette animation, il n'envoie jamais l'utilisation (portails Eliotrope : 2 choix affiches, aucune teleportation).
UPDATE interactives_spawns s SET s.Animated=0
 WHERE s.Animated=1 AND s.TemplateId IN (253,280)
   AND EXISTS (SELECT 1 FROM interactives_spawns_skills ss JOIN interactives_skills k ON k.Id=ss.SkillId WHERE ss.InteractiveSpawnId=s.Id AND k.Type='Teleport');
-- Sorties des dimensions (demande utilisateur 16/10) : retour a l'avant-poste des Voyageurs (130025985, Routes Rocailleuses),
-- action officielle « Quitter la Dimension » (258), arrivee a cote du portail d'entree de la meme dimension.
-- Avant : Srambad / Xelorium / Ecaflipus ouvraient la liste des zaaps (ZaapTeleport, action 1 partagee avec les vrais zaaps).
DELETE FROM interactives_spawns_skills WHERE InteractiveSpawnId IN (484987,485372,486780,503232);
DELETE FROM interactives_skills WHERE Id BETWEEN 3600020 AND 3600023;
INSERT INTO interactives_skills (Id, Type, Duration, `Condition`, CustomTemplateId, Parameter0, Parameter1, Parameter2) VALUES
 (3600020, 'Teleport', 60, NULL, 258, '130025985', '457', '1'), -- Enutrosor 484987 (131596288)
 (3600021, 'Teleport', 60, NULL, 258, '130025985', '345', '1'), -- Srambad 485372 (134351108)
 (3600022, 'Teleport', 60, NULL, 258, '130025985', '341', '1'), -- Xelorium 486780 (145100034)
 (3600023, 'Teleport', 60, NULL, 258, '130025985', '453', '1'); -- Ecaflipus 503232 (161351684)
INSERT INTO interactives_spawns_skills (Id, InteractiveSpawnId, SkillId) VALUES
 (3600020, 484987, 3600020), (3600021, 485372, 3600021), (3600022, 486780, 3600022), (3600023, 503232, 3600023);
