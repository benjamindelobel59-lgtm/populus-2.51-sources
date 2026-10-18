-- MYSTIC 18/10 : audit des donjons (dofuspourlesnoobs), phase 2 : données fausses en base.
-- Généré par _OUTILS/serveur/donjons/dpln/generer_45_audit.py. Annulation : 45_audit_donjons-annuler.sql.
-- À appliquer World arrêté (ou redémarrer le World ensuite : monstres et salles sont chargés au démarrage).

-- Maître Corbac 289 : Carapace d'Ailes 630 au niveau 1 (le niveau 3 n'est que le sous-sort de dégâts)
UPDATE monsters_spells s JOIN monsters_grades g ON g.Id=s.MonsterGradeId SET s.Level=1 WHERE g.MonsterId=289 AND s.SpellId=630;

-- Blops / Biblops : niveau de sort = couleur, pas le grade
UPDATE monsters_spells s JOIN monsters_grades g ON g.Id=s.MonsterGradeId SET s.Level = CASE g.MonsterId WHEN 273 THEN 1 WHEN 278 THEN 1 WHEN 275 THEN 2 WHEN 279 THEN 2 WHEN 274 THEN 3 WHEN 277 THEN 3 WHEN 276 THEN 4 WHEN 280 THEN 4 END WHERE g.MonsterId IN (273,274,275,276,277,278,279,280) AND s.SpellId IN (471,473,475,476);

-- Darkli Moon grade 1 (invoqué) : Totems 298-301 au niveau 2 (le niveau 1 n'invoque rien)
UPDATE monsters_spells SET Level=2 WHERE MonsterGradeId=710 AND SpellId IN (298,299,300,301);

-- Tortues de Sphincter Cell (grade 1 invoqué) : Kawabunga 1019 au niveau de leur élément
UPDATE monsters_spells SET Level=3 WHERE MonsterGradeId=3242 AND SpellId=1019; UPDATE monsters_spells SET Level=4 WHERE MonsterGradeId=3247 AND SpellId=1019; UPDATE monsters_spells SET Level=1 WHERE MonsterGradeId=3252 AND SpellId=1019; UPDATE monsters_spells SET Level=2 WHERE MonsterGradeId=3257 AND SpellId=1019;

-- Craqueleurs : Manteau Tellurique 7120 au niveau 1 (niveaux 2-3 = sous-sorts)
UPDATE monsters_spells SET Level=1 WHERE SpellId=7120 AND Id IN (5311,5312,5313,14530,14531,14532,19220,19221,19222,48864,48865,48866);

-- Grotte Hesque : élément des Crustorails / Palmifleurs selon le monstre (comme les Tynrils)
UPDATE monsters_spells s JOIN monsters_grades g ON g.Id=s.MonsterGradeId SET s.Level=3 WHERE g.MonsterId IN (1060,1064) AND s.SpellId IN (1030,1031,1032); UPDATE monsters_spells s JOIN monsters_grades g ON g.Id=s.MonsterGradeId SET s.Level=5 WHERE g.MonsterId IN (1061,1065) AND s.SpellId IN (1030,1031,1032); UPDATE monsters_spells s JOIN monsters_grades g ON g.Id=s.MonsterGradeId SET s.Level=2 WHERE g.MonsterId IN (1063,1067) AND s.SpellId IN (1030,1031,1032); UPDATE monsters_spells s JOIN monsters_grades g ON g.Id=s.MonsterGradeId SET s.Level=4 WHERE g.MonsterId IN (1062,1066) AND s.SpellId IN (1030,1031,1032);

-- Bibliothèque du Maître Corbac : donjon classique de 4 salles (vrais monstres ; Capsaaloocke dans la salle avant le boss)
DELETE FROM monsters_spawns_dungeons_groups WHERE DungeonSpawnId BETWEEN 466 AND 498;
DELETE FROM monsters_spawns_dungeons WHERE Id BETWEEN 469 AND 497;
UPDATE monsters_spawns_dungeons SET TeleportMapId=159122432, TeleportCell=268, Descripcion='MYSTIC 45 Bibliotheque salle 1 - salle 2' WHERE Id=466;
UPDATE monsters_spawns_dungeons SET TeleportMapId=159123456, TeleportCell=226, Descripcion='MYSTIC 45 Bibliotheque salle 2 - salle 3' WHERE Id=467;
UPDATE monsters_spawns_dungeons SET TeleportMapId=159123460, TeleportCell=199, Descripcion='MYSTIC 45 Bibliotheque salle 3 - salle Capsaaloocke' WHERE Id=468;
UPDATE monsters_spawns_dungeons SET TeleportMapId=159125512, TeleportCell=296, Descripcion='MYSTIC 45 Bibliotheque salle Capsaaloocke - Maitre Corbac' WHERE Id=498;
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId,MonsterGradeId,MinPartyMembers,Informations) VALUES
  (466,14001,1,'MYSTIC 45 biblio salle 1'),
  (466,14006,1,'MYSTIC 45 biblio salle 1'),
  (466,13996,1,'MYSTIC 45 biblio salle 1'),
  (466,14011,1,'MYSTIC 45 biblio salle 1'),
  (466,14001,5,'MYSTIC 45 biblio salle 1'),
  (466,14006,5,'MYSTIC 45 biblio salle 1'),
  (466,13996,5,'MYSTIC 45 biblio salle 1'),
  (466,14011,5,'MYSTIC 45 biblio salle 1'),
  (466,14001,5,'MYSTIC 45 biblio salle 1'),
  (466,14001,6,'MYSTIC 45 biblio salle 1'),
  (466,14006,6,'MYSTIC 45 biblio salle 1'),
  (466,13996,6,'MYSTIC 45 biblio salle 1'),
  (466,14011,6,'MYSTIC 45 biblio salle 1'),
  (466,14001,6,'MYSTIC 45 biblio salle 1'),
  (466,14006,6,'MYSTIC 45 biblio salle 1'),
  (466,14001,7,'MYSTIC 45 biblio salle 1'),
  (466,14006,7,'MYSTIC 45 biblio salle 1'),
  (466,13996,7,'MYSTIC 45 biblio salle 1'),
  (466,14011,7,'MYSTIC 45 biblio salle 1'),
  (466,14001,7,'MYSTIC 45 biblio salle 1'),
  (466,14006,7,'MYSTIC 45 biblio salle 1'),
  (466,13996,7,'MYSTIC 45 biblio salle 1'),
  (466,14001,8,'MYSTIC 45 biblio salle 1'),
  (466,14006,8,'MYSTIC 45 biblio salle 1'),
  (466,13996,8,'MYSTIC 45 biblio salle 1'),
  (466,14011,8,'MYSTIC 45 biblio salle 1'),
  (466,14001,8,'MYSTIC 45 biblio salle 1'),
  (466,14006,8,'MYSTIC 45 biblio salle 1'),
  (466,13996,8,'MYSTIC 45 biblio salle 1'),
  (466,14011,8,'MYSTIC 45 biblio salle 1');
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId,MonsterGradeId,MinPartyMembers,Informations) VALUES
  (467,14002,1,'MYSTIC 45 biblio salle 2'),
  (467,14007,1,'MYSTIC 45 biblio salle 2'),
  (467,13997,1,'MYSTIC 45 biblio salle 2'),
  (467,14012,1,'MYSTIC 45 biblio salle 2'),
  (467,14002,5,'MYSTIC 45 biblio salle 2'),
  (467,14007,5,'MYSTIC 45 biblio salle 2'),
  (467,13997,5,'MYSTIC 45 biblio salle 2'),
  (467,14012,5,'MYSTIC 45 biblio salle 2'),
  (467,14002,5,'MYSTIC 45 biblio salle 2'),
  (467,14002,6,'MYSTIC 45 biblio salle 2'),
  (467,14007,6,'MYSTIC 45 biblio salle 2'),
  (467,13997,6,'MYSTIC 45 biblio salle 2'),
  (467,14012,6,'MYSTIC 45 biblio salle 2'),
  (467,14002,6,'MYSTIC 45 biblio salle 2'),
  (467,14007,6,'MYSTIC 45 biblio salle 2'),
  (467,14002,7,'MYSTIC 45 biblio salle 2'),
  (467,14007,7,'MYSTIC 45 biblio salle 2'),
  (467,13997,7,'MYSTIC 45 biblio salle 2'),
  (467,14012,7,'MYSTIC 45 biblio salle 2'),
  (467,14002,7,'MYSTIC 45 biblio salle 2'),
  (467,14007,7,'MYSTIC 45 biblio salle 2'),
  (467,13997,7,'MYSTIC 45 biblio salle 2'),
  (467,14002,8,'MYSTIC 45 biblio salle 2'),
  (467,14007,8,'MYSTIC 45 biblio salle 2'),
  (467,13997,8,'MYSTIC 45 biblio salle 2'),
  (467,14012,8,'MYSTIC 45 biblio salle 2'),
  (467,14002,8,'MYSTIC 45 biblio salle 2'),
  (467,14007,8,'MYSTIC 45 biblio salle 2'),
  (467,13997,8,'MYSTIC 45 biblio salle 2'),
  (467,14012,8,'MYSTIC 45 biblio salle 2');
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId,MonsterGradeId,MinPartyMembers,Informations) VALUES
  (468,14003,1,'MYSTIC 45 biblio salle 3'),
  (468,14008,1,'MYSTIC 45 biblio salle 3'),
  (468,13998,1,'MYSTIC 45 biblio salle 3'),
  (468,14013,1,'MYSTIC 45 biblio salle 3'),
  (468,14003,5,'MYSTIC 45 biblio salle 3'),
  (468,14008,5,'MYSTIC 45 biblio salle 3'),
  (468,13998,5,'MYSTIC 45 biblio salle 3'),
  (468,14013,5,'MYSTIC 45 biblio salle 3'),
  (468,14003,5,'MYSTIC 45 biblio salle 3'),
  (468,14003,6,'MYSTIC 45 biblio salle 3'),
  (468,14008,6,'MYSTIC 45 biblio salle 3'),
  (468,13998,6,'MYSTIC 45 biblio salle 3'),
  (468,14013,6,'MYSTIC 45 biblio salle 3'),
  (468,14003,6,'MYSTIC 45 biblio salle 3'),
  (468,14008,6,'MYSTIC 45 biblio salle 3'),
  (468,14003,7,'MYSTIC 45 biblio salle 3'),
  (468,14008,7,'MYSTIC 45 biblio salle 3'),
  (468,13998,7,'MYSTIC 45 biblio salle 3'),
  (468,14013,7,'MYSTIC 45 biblio salle 3'),
  (468,14003,7,'MYSTIC 45 biblio salle 3'),
  (468,14008,7,'MYSTIC 45 biblio salle 3'),
  (468,13998,7,'MYSTIC 45 biblio salle 3'),
  (468,14003,8,'MYSTIC 45 biblio salle 3'),
  (468,14008,8,'MYSTIC 45 biblio salle 3'),
  (468,13998,8,'MYSTIC 45 biblio salle 3'),
  (468,14013,8,'MYSTIC 45 biblio salle 3'),
  (468,14003,8,'MYSTIC 45 biblio salle 3'),
  (468,14008,8,'MYSTIC 45 biblio salle 3'),
  (468,13998,8,'MYSTIC 45 biblio salle 3'),
  (468,14013,8,'MYSTIC 45 biblio salle 3');
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId,MonsterGradeId,MinPartyMembers,Informations) VALUES
  (498,13977,1,'MYSTIC 45 biblio salle Capsaaloocke'),
  (498,14009,1,'MYSTIC 45 biblio salle Capsaaloocke'),
  (498,13999,1,'MYSTIC 45 biblio salle Capsaaloocke'),
  (498,14014,1,'MYSTIC 45 biblio salle Capsaaloocke'),
  (498,13977,5,'MYSTIC 45 biblio salle Capsaaloocke'),
  (498,14009,5,'MYSTIC 45 biblio salle Capsaaloocke'),
  (498,13999,5,'MYSTIC 45 biblio salle Capsaaloocke'),
  (498,14014,5,'MYSTIC 45 biblio salle Capsaaloocke'),
  (498,14004,5,'MYSTIC 45 biblio salle Capsaaloocke'),
  (498,13977,6,'MYSTIC 45 biblio salle Capsaaloocke'),
  (498,14009,6,'MYSTIC 45 biblio salle Capsaaloocke'),
  (498,13999,6,'MYSTIC 45 biblio salle Capsaaloocke'),
  (498,14014,6,'MYSTIC 45 biblio salle Capsaaloocke'),
  (498,14004,6,'MYSTIC 45 biblio salle Capsaaloocke'),
  (498,14009,6,'MYSTIC 45 biblio salle Capsaaloocke'),
  (498,13977,7,'MYSTIC 45 biblio salle Capsaaloocke'),
  (498,14009,7,'MYSTIC 45 biblio salle Capsaaloocke'),
  (498,13999,7,'MYSTIC 45 biblio salle Capsaaloocke'),
  (498,14014,7,'MYSTIC 45 biblio salle Capsaaloocke'),
  (498,14004,7,'MYSTIC 45 biblio salle Capsaaloocke'),
  (498,14009,7,'MYSTIC 45 biblio salle Capsaaloocke'),
  (498,13999,7,'MYSTIC 45 biblio salle Capsaaloocke'),
  (498,13977,8,'MYSTIC 45 biblio salle Capsaaloocke'),
  (498,14009,8,'MYSTIC 45 biblio salle Capsaaloocke'),
  (498,13999,8,'MYSTIC 45 biblio salle Capsaaloocke'),
  (498,14014,8,'MYSTIC 45 biblio salle Capsaaloocke'),
  (498,14004,8,'MYSTIC 45 biblio salle Capsaaloocke'),
  (498,14009,8,'MYSTIC 45 biblio salle Capsaaloocke'),
  (498,13999,8,'MYSTIC 45 biblio salle Capsaaloocke'),
  (498,14014,8,'MYSTIC 45 biblio salle Capsaaloocke');

-- Grozilla et Grasmera : vrais monstres (Krokilles), boss Grozilla + Grasmera, la salle du boss mène à la sortie
DELETE FROM monsters_spawns_dungeons_groups WHERE DungeonSpawnId IN (499,500,501,502);
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId,MonsterGradeId,MinPartyMembers,Informations) VALUES
  (502,7292,1,'MYSTIC 45 grozilla salle 1'),
  (502,7297,1,'MYSTIC 45 grozilla salle 1'),
  (502,7302,1,'MYSTIC 45 grozilla salle 1'),
  (502,7307,1,'MYSTIC 45 grozilla salle 1'),
  (502,7312,1,'MYSTIC 45 grozilla salle 1');
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId,MonsterGradeId,MinPartyMembers,Informations) VALUES
  (501,7294,1,'MYSTIC 45 grozilla salle 2'),
  (501,7299,1,'MYSTIC 45 grozilla salle 2'),
  (501,7304,1,'MYSTIC 45 grozilla salle 2'),
  (501,7309,1,'MYSTIC 45 grozilla salle 2'),
  (501,7314,1,'MYSTIC 45 grozilla salle 2');
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId,MonsterGradeId,MinPartyMembers,Informations) VALUES
  (500,7202,1,'MYSTIC 45 grozilla boss'),
  (500,7207,1,'MYSTIC 45 grozilla boss'),
  (500,7203,5,'MYSTIC 45 grozilla boss'),
  (500,7208,5,'MYSTIC 45 grozilla boss'),
  (500,7204,6,'MYSTIC 45 grozilla boss'),
  (500,7209,6,'MYSTIC 45 grozilla boss'),
  (500,7205,7,'MYSTIC 45 grozilla boss'),
  (500,7210,7,'MYSTIC 45 grozilla boss'),
  (500,7206,8,'MYSTIC 45 grozilla boss'),
  (500,7211,8,'MYSTIC 45 grozilla boss');
UPDATE monsters_spawns_dungeons SET TeleportMapId=76285958, TeleportCell=371 WHERE Id=500;
DELETE FROM monsters_spawns_dungeons WHERE Id=499;

-- Hypogée de l'Obsidiantre : salle 1 = vrais monstres du donjon
DELETE FROM monsters_spawns_dungeons_groups WHERE DungeonSpawnId=446;
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId,MonsterGradeId,MinPartyMembers,Informations) VALUES
  (446,6463,1,'MYSTIC 45 obsi salle 1'),
  (446,6468,1,'MYSTIC 45 obsi salle 1'),
  (446,6473,1,'MYSTIC 45 obsi salle 1'),
  (446,6478,1,'MYSTIC 45 obsi salle 1'),
  (446,6463,5,'MYSTIC 45 obsi salle 1'),
  (446,6468,5,'MYSTIC 45 obsi salle 1'),
  (446,6473,5,'MYSTIC 45 obsi salle 1'),
  (446,6478,5,'MYSTIC 45 obsi salle 1'),
  (446,6483,5,'MYSTIC 45 obsi salle 1'),
  (446,6463,6,'MYSTIC 45 obsi salle 1'),
  (446,6468,6,'MYSTIC 45 obsi salle 1'),
  (446,6473,6,'MYSTIC 45 obsi salle 1'),
  (446,6478,6,'MYSTIC 45 obsi salle 1'),
  (446,6483,6,'MYSTIC 45 obsi salle 1'),
  (446,6463,6,'MYSTIC 45 obsi salle 1'),
  (446,6463,7,'MYSTIC 45 obsi salle 1'),
  (446,6468,7,'MYSTIC 45 obsi salle 1'),
  (446,6473,7,'MYSTIC 45 obsi salle 1'),
  (446,6478,7,'MYSTIC 45 obsi salle 1'),
  (446,6483,7,'MYSTIC 45 obsi salle 1'),
  (446,6463,7,'MYSTIC 45 obsi salle 1'),
  (446,6473,7,'MYSTIC 45 obsi salle 1'),
  (446,6463,8,'MYSTIC 45 obsi salle 1'),
  (446,6468,8,'MYSTIC 45 obsi salle 1'),
  (446,6473,8,'MYSTIC 45 obsi salle 1'),
  (446,6478,8,'MYSTIC 45 obsi salle 1'),
  (446,6483,8,'MYSTIC 45 obsi salle 1'),
  (446,6463,8,'MYSTIC 45 obsi salle 1'),
  (446,6473,8,'MYSTIC 45 obsi salle 1'),
  (446,6478,8,'MYSTIC 45 obsi salle 1');

-- Donjon des Kitsounes : salle du Tanukouï San (carte 18088964) entre la salle 5 et la sortie
INSERT INTO monsters_spawns_dungeons (MapId,CellId,TeleportEvent,TeleportMapId,TeleportCell,TeleportDirection,Descripcion) VALUES (18088964,NULL,1,18089988,NULL,7,'MYSTIC 45 Kitsun boss-sortie');
SET @b = LAST_INSERT_ID();
UPDATE monsters_spawns_dungeons SET TeleportMapId=18088964 WHERE Id=239;
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId,MonsterGradeId,MinPartyMembers,Informations) VALUES
  (@b,1948,1,'MYSTIC 45 kitsou boss'),
  (@b,1803,1,'MYSTIC 45 kitsou boss'),
  (@b,1798,1,'MYSTIC 45 kitsou boss'),
  (@b,1949,5,'MYSTIC 45 kitsou boss'),
  (@b,1803,5,'MYSTIC 45 kitsou boss'),
  (@b,1798,5,'MYSTIC 45 kitsou boss'),
  (@b,1808,5,'MYSTIC 45 kitsou boss'),
  (@b,1950,6,'MYSTIC 45 kitsou boss'),
  (@b,1803,6,'MYSTIC 45 kitsou boss'),
  (@b,1798,6,'MYSTIC 45 kitsou boss'),
  (@b,1808,6,'MYSTIC 45 kitsou boss'),
  (@b,1768,6,'MYSTIC 45 kitsou boss'),
  (@b,1951,7,'MYSTIC 45 kitsou boss'),
  (@b,1803,7,'MYSTIC 45 kitsou boss'),
  (@b,1798,7,'MYSTIC 45 kitsou boss'),
  (@b,1808,7,'MYSTIC 45 kitsou boss'),
  (@b,1768,7,'MYSTIC 45 kitsou boss'),
  (@b,1803,7,'MYSTIC 45 kitsou boss'),
  (@b,1952,8,'MYSTIC 45 kitsou boss'),
  (@b,1803,8,'MYSTIC 45 kitsou boss'),
  (@b,1798,8,'MYSTIC 45 kitsou boss'),
  (@b,1808,8,'MYSTIC 45 kitsou boss'),
  (@b,1768,8,'MYSTIC 45 kitsou boss'),
  (@b,1803,8,'MYSTIC 45 kitsou boss'),
  (@b,1798,8,'MYSTIC 45 kitsou boss');

-- Sphincter Cell : le Rat Noir et le Rat Blanc remplacent 2 rats à chaque palier (même grade que le Cell)
UPDATE monsters_spawns_dungeons_groups SET MonsterGradeId=3215, Informations='MYSTIC 45 rat noir' WHERE Id=27341;
UPDATE monsters_spawns_dungeons_groups SET MonsterGradeId=3221, Informations='MYSTIC 45 rat blanc' WHERE Id=27342;
UPDATE monsters_spawns_dungeons_groups SET MonsterGradeId=3216, Informations='MYSTIC 45 rat noir' WHERE Id=27345;
UPDATE monsters_spawns_dungeons_groups SET MonsterGradeId=3222, Informations='MYSTIC 45 rat blanc' WHERE Id=27346;
UPDATE monsters_spawns_dungeons_groups SET MonsterGradeId=3217, Informations='MYSTIC 45 rat noir' WHERE Id=27350;
UPDATE monsters_spawns_dungeons_groups SET MonsterGradeId=3223, Informations='MYSTIC 45 rat blanc' WHERE Id=27351;
UPDATE monsters_spawns_dungeons_groups SET MonsterGradeId=3218, Informations='MYSTIC 45 rat noir' WHERE Id=27356;
UPDATE monsters_spawns_dungeons_groups SET MonsterGradeId=3224, Informations='MYSTIC 45 rat blanc' WHERE Id=27357;
UPDATE monsters_spawns_dungeons_groups SET MonsterGradeId=3219, Informations='MYSTIC 45 rat noir' WHERE Id=27363;
UPDATE monsters_spawns_dungeons_groups SET MonsterGradeId=3225, Informations='MYSTIC 45 rat blanc' WHERE Id=27364;
-- Batofu : le Tofu Mutant (grade 5) remplace un tofu à partir de 5 joueurs
UPDATE monsters_spawns_dungeons_groups SET MonsterGradeId=15159, Informations='MYSTIC 45 tofu mutant' WHERE Id IN (48342,48347,48353,48360);

