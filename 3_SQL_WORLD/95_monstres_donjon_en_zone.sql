-- PADGREF 22/10 (demande utilisateur, serveur héroïque) : les monstres qu'on ne trouvait QUE en donjon apparaissent aussi dans les
-- groupes de la zone où est l'entrée de leur donjon, plus rarement (fréquence 0,30 ; un monstre normal de zone = 1 à 1,25),
-- mêmes grades qu'en donjon. Ex. Mérulette -> Forêt pétrifiée. Pas de boss, pas les peluches de Nowel, pas les familiers du Corbac.
-- Donjons dont l'entrée n'est pas dans une zone à monstres (Clos des Blops, Larves, Halouine…) : rien.
-- 91 ajouts. Base world. Rejouable. Annulation : 95_monstres_donjon_en_zone-annuler.sql. Redémarrer le World.
--   Renarbo Parleur (4581) -> Pénates du Corbac (55), donjon Bibliothèque du Maître Corbac
--   Buveur de Sang (4578) -> Pénates du Corbac (55), donjon Bibliothèque du Maître Corbac
--   Corbac Dressé (4580) -> Pénates du Corbac (55), donjon Bibliothèque du Maître Corbac
--   Corbac Chef (4579) -> Pénates du Corbac (55), donjon Bibliothèque du Maître Corbac
--   Capsaaloocke (4572) -> Pénates du Corbac (55), donjon Bibliothèque du Maître Corbac
--   Araknotron (917) -> Forêt Sombre (168), donjon Domaine Ancestral
--   Cybwork (488) -> Gisgoul (71), donjon Grotte du Bworker
--   Muloubard (4866) -> Hauts des Hurlements (522), donjon Tanière du Meulou
--   Wobot Kiafin (3462) -> Îlot de Waldo (162), donjon Terrier du Wa Wabbit
--   Tiwobot (3474) -> Îlot de Waldo (162), donjon Terrier du Wa Wabbit
--   Black Wo Wabbit (3461) -> Îlot de Waldo (162), donjon Terrier du Wa Wabbit
--   Wobot (181) -> Îlot de Waldo (162), donjon Terrier du Wa Wabbit
--   Blanc Pa Wabbit (3463) -> Îlot de Waldo (162), donjon Terrier du Wa Wabbit
--   Pichon Kloune (924) -> Calanques d'Astrub (335), donjon Château Ensablé
--   Champa Explosif (1178) -> Caverne des Fungus (495), donjon Temple du Grand Ougah
--   Gelée Citron (429) -> Péninsule des gelées (12), donjon Gelaxième Dimension
--   Kanniboul Tam (4151) -> Forêt des Masques (166), donjon Village Kanniboul
--   Bouftou des Cavernes (671) -> Canyon sauvage (253), donjon Caverne du Koulosse
--   Bwak de Vent (3281) -> Montagne des Craqueleurs (2), donjon Nid du Kwakwa
--   Bwak de Terre (3284) -> Montagne des Craqueleurs (2), donjon Nid du Kwakwa
--   Bwak de Flamme (3283) -> Montagne des Craqueleurs (2), donjon Nid du Kwakwa
--   Bwak de Glace (3282) -> Montagne des Craqueleurs (2), donjon Nid du Kwakwa
--   Macien (301) -> Désolation de Sidimote (57), donjon Laboratoire de Brumen Tinctorias
--   Pandikaze Aérien (578) -> Bordure d'Aerdala (108), donjon Repaire des Pandikazes
--   Pandikaze Vertige (579) -> Bordure d'Aerdala (108), donjon Repaire des Pandikazes
--   Pandikwakaze (580) -> Bordure d'Aerdala (108), donjon Repaire des Pandikazes
--   Pandulkaze (581) -> Bordure d'Aerdala (108), donjon Repaire des Pandikazes
--   Guerrier Pandikaze (607) -> Bordure d'Aerdala (108), donjon Repaire des Pandikazes
--   Rat Basher (938) -> Égouts de Bonta (73), donjon Donjon des Rats de Bonta
--   Rat Croc (942) -> Égouts de Bonta (73), donjon Donjon des Rats de Bonta
--   Rat Bajoie (941) -> Égouts de Bonta (73), donjon Donjon des Rats de Bonta
--   Rat Klure (937) -> Égouts de Brâkmar (75), donjon Donjon des Rats de Brâkmar
--   Rat de Marais (935) -> Égouts de Brâkmar (75), donjon Donjon des Rats de Brâkmar
--   Rat Batteur (936) -> Égouts de Brâkmar (75), donjon Donjon des Rats de Brâkmar
--   Rat Goûtant (3346) -> Souterrains du Château d'Amakna (181), donjon Donjon des Rats du Château d'Amakna
--   Rat Masseur (3347) -> Souterrains du Château d'Amakna (181), donjon Donjon des Rats du Château d'Amakna
--   Rat Colleur (3348) -> Souterrains du Château d'Amakna (181), donjon Donjon des Rats du Château d'Amakna
--   Rat Pine (3349) -> Souterrains du Château d'Amakna (181), donjon Donjon des Rats du Château d'Amakna
--   Rat Fraîchi (3350) -> Souterrains du Château d'Amakna (181), donjon Donjon des Rats du Château d'Amakna
--   Rat Caille (3351) -> Souterrains du Château d'Amakna (181), donjon Donjon des Rats du Château d'Amakna
--   Rat Botteur (3352) -> Souterrains du Château d'Amakna (181), donjon Donjon des Rats du Château d'Amakna
--   Ramane (3345) -> Souterrains du Château d'Amakna (181), donjon Donjon des Rats du Château d'Amakna
--   Scarafeuille Immature (798) -> Plaine des Scarafeuilles (170), donjon Donjon des Scarafeuilles
--   Scarafeuille Noir (795) -> Plaine des Scarafeuilles (170), donjon Donjon des Scarafeuilles
--   Rubise (677) -> Vallée de la Morh'Kitu (275), donjon Repaire de Skeunk
--   Saphira (675) -> Vallée de la Morh'Kitu (275), donjon Repaire de Skeunk
--   Émeraude (673) -> Vallée de la Morh'Kitu (275), donjon Repaire de Skeunk
--   Poupée Affamée (676) -> Vallée de la Morh'Kitu (275), donjon Repaire de Skeunk
--   Diamantine (681) -> Vallée de la Morh'Kitu (275), donjon Repaire de Skeunk
--   Poupée Émeraude (674) -> Vallée de la Morh'Kitu (275), donjon Repaire de Skeunk
--   Tofu Ventripotent (808) -> Champ des Ingalsse (3), donjon Donjon des Tofus
--   Vilain Petit Tofu (3299) -> Champ des Ingalsse (3), donjon Tofulailler Royal
--   Tofutoflamme (3300) -> Champ des Ingalsse (3), donjon Tofulailler Royal
--   Tofuzmo (807) -> Champ des Ingalsse (3), donjon Tofulailler Royal
--   Tofubine (3301) -> Champ des Ingalsse (3), donjon Tofulailler Royal
--   Tofu Dodu (3302) -> Champ des Ingalsse (3), donjon Tofulailler Royal
--   Fumrirolle (2868) -> Larmes d'Ouronigride (610), donjon Hypogée de l'Obsidiantre
--   Mofette (2867) -> Larmes d'Ouronigride (610), donjon Hypogée de l'Obsidiantre
--   Kami Givrefoux (2982) -> Cavernes des Givrefoux (622), donjon Tanière Givrefoux
--   Mérulette (2900) -> Forêt pétrifiée (605), donjon Antre du Korriandre
--   Blérom (2884) -> Crocs de verre (608), donjon Cavernes du Kolosso
--   Glourmand (2987) -> Ruche des Gloursons (753), donjon Antichambre des Gloursons
--   Perkü (2936) -> Forêt enneigée (652), donjon Donjon de la mine de Sakaï
--   Courtilieur (2937) -> Forêt enneigée (652), donjon Donjon de la mine de Sakaï
--   Brikoglours (3405) -> Remparts à vent (786), donjon Transporteur de Sylargh
--   Peunch (3381) -> Tannerie Écarlate (787), donjon Salons privés de Klime
--   Frimar (3390) -> Bastion des froides légions (788), donjon Forgefroide de Missiz Frizz
--   Termystique (3395) -> Jardins d'Hiver (785), donjon Laboratoire de Nileza
--   Strigide (3411) -> Tour de la Clepsydre (789), donjon Donjon du Comte Harebourg
--   Dramak (3482) -> Île de Kartonpath (799), donjon Théâtre de Dramak
--   Harpo (3538) -> Salles des Abîmes (806), donjon Aquadôme de Merkator
--   Sombléro (3570) -> Dimension Obscure (813), donjon Pyramide d'Ombre
--   Magouille (3750) -> Hauts Ténébreux (829), donjon Trône de la Cour Sombre
--   Pataugerme (3827) -> Roc des Salbatroces (834), donjon Ventre de la Baleine
--   Segmantid (3858) -> Chemins d'hier (836), donjon Mégalithe de Fraktale
--   Soissanth (3844) -> Jour présent (837), donjon Horologium de XLII
--   Brabuzar (3839) -> Lendemains incertains (838), donjon Œil de Vortex
--   Sergent Chafer (4050) -> Cimetière (449), donjon Crypte de Kardorim
--   Ivremor (4161) -> Chemin du Crâne (167), donjon Bateau du Chouque
--   Domoizelle (4163) -> Jungle Interdite (165), donjon Arbre de Moon
--   Boulépique (4617) -> Dunes des ossements (872), donjon NULL
--   Lévito (4608) -> Territoire Cacterre (873), donjon Caverne d'El Piko
--   Pikténia (4727) -> Gorge des Vents Hurlants (878), donjon Boyau du Père Ver
--   Griffotep (4743) -> Pyramide Maudite (881), donjon Chambre de Tal Kasha
--   Viandargh (4804) -> Nimotopia (883), donjon Camp du Comte Razof
--   Bozoteur (4861) -> Route des Roulottes (526), donjon Chapiteau des Magik Riktus
--   Goulafre (4877) -> Terres Désacrées (517), donjon Manoir des Katrepat
--   Chause (4968) -> Caserne du Jour sans fin (892), donjon Belvédère d'Ilyzaelle
--   Zombruth (5111) -> Épaves Silencieuses (897), donjon Tour de Bethel
--   Klémort (5101) -> Marches Magmatiques (896), donjon Tour de Solar
--   Tanklume (5314) -> Royaume des Martegel (902), donjon Brasserie du roi Dazak
DELETE FROM monsters_spawns WHERE Id BETWEEN 9500001 AND 9509999;
INSERT INTO monsters_spawns (Id, MapId, SubAreaId, MonsterId, Frequency, MinGrade, MaxGrade, IsDisabled) VALUES
(9500001, NULL, 55, 4581, 0.30, 2, 5, 0),
(9500002, NULL, 55, 4578, 0.30, 2, 5, 0),
(9500003, NULL, 55, 4580, 0.30, 2, 5, 0),
(9500004, NULL, 55, 4579, 0.30, 2, 5, 0),
(9500005, NULL, 55, 4572, 0.30, 3, 3, 0),
(9500006, NULL, 168, 917, 0.30, 2, 5, 0),
(9500007, NULL, 71, 488, 0.30, 1, 3, 0),
(9500008, NULL, 522, 4866, 0.30, 2, 5, 0),
(9500009, NULL, 162, 3462, 0.30, 1, 5, 0),
(9500010, NULL, 162, 3474, 0.30, 1, 5, 0),
(9500011, NULL, 162, 3461, 0.30, 1, 5, 0),
(9500012, NULL, 162, 181, 0.30, 1, 5, 0),
(9500013, NULL, 162, 3463, 0.30, 2, 5, 0),
(9500014, NULL, 335, 924, 0.30, 1, 5, 0),
(9500015, NULL, 495, 1178, 0.30, 3, 4, 0),
(9500016, NULL, 12, 429, 0.30, 1, 5, 0),
(9500017, NULL, 166, 4151, 0.30, 1, 3, 0),
(9500018, NULL, 253, 671, 0.30, 5, 5, 0),
(9500019, NULL, 2, 3281, 0.30, 1, 1, 0),
(9500020, NULL, 2, 3284, 0.30, 1, 1, 0),
(9500021, NULL, 2, 3283, 0.30, 1, 1, 0),
(9500022, NULL, 2, 3282, 0.30, 1, 1, 0),
(9500023, NULL, 57, 301, 0.30, 2, 5, 0),
(9500024, NULL, 108, 578, 0.30, 1, 5, 0),
(9500025, NULL, 108, 579, 0.30, 1, 1, 0),
(9500026, NULL, 108, 580, 0.30, 1, 1, 0),
(9500027, NULL, 108, 581, 0.30, 2, 5, 0),
(9500028, NULL, 108, 607, 0.30, 1, 1, 0),
(9500029, NULL, 73, 938, 0.30, 1, 5, 0),
(9500030, NULL, 73, 942, 0.30, 1, 5, 0),
(9500031, NULL, 73, 941, 0.30, 3, 5, 0),
(9500032, NULL, 75, 937, 0.30, 1, 5, 0),
(9500033, NULL, 75, 935, 0.30, 1, 5, 0),
(9500034, NULL, 75, 936, 0.30, 3, 5, 0),
(9500035, NULL, 181, 3346, 0.30, 1, 5, 0),
(9500036, NULL, 181, 3347, 0.30, 1, 5, 0),
(9500037, NULL, 181, 3348, 0.30, 1, 5, 0),
(9500038, NULL, 181, 3349, 0.30, 1, 4, 0),
(9500039, NULL, 181, 3350, 0.30, 1, 5, 0),
(9500040, NULL, 181, 3351, 0.30, 1, 4, 0),
(9500041, NULL, 181, 3352, 0.30, 1, 4, 0),
(9500042, NULL, 181, 3345, 0.30, 1, 5, 0),
(9500043, NULL, 170, 798, 0.30, 1, 1, 0),
(9500044, NULL, 170, 795, 0.30, 1, 2, 0),
(9500045, NULL, 275, 677, 0.30, 1, 6, 0),
(9500046, NULL, 275, 675, 0.30, 1, 6, 0),
(9500047, NULL, 275, 673, 0.30, 1, 6, 0),
(9500048, NULL, 275, 676, 0.30, 1, 1, 0),
(9500049, NULL, 275, 681, 0.30, 1, 6, 0),
(9500050, NULL, 275, 674, 0.30, 1, 1, 0),
(9500051, NULL, 3, 808, 0.30, 1, 3, 0),
(9500052, NULL, 3, 3299, 0.30, 1, 5, 0),
(9500053, NULL, 3, 3300, 0.30, 1, 5, 0),
(9500054, NULL, 3, 807, 0.30, 2, 4, 0),
(9500055, NULL, 3, 3301, 0.30, 3, 5, 0),
(9500056, NULL, 3, 3302, 0.30, 4, 5, 0),
(9500057, NULL, 610, 2868, 0.30, 1, 5, 0),
(9500058, NULL, 610, 2867, 0.30, 1, 5, 0),
(9500059, NULL, 622, 2982, 0.30, 1, 5, 0),
(9500060, NULL, 605, 2900, 0.30, 1, 5, 0),
(9500061, NULL, 608, 2884, 0.30, 3, 5, 0),
(9500062, NULL, 753, 2987, 0.30, 1, 5, 0),
(9500063, NULL, 652, 2936, 0.30, 2, 5, 0),
(9500064, NULL, 652, 2937, 0.30, 3, 5, 0),
(9500065, NULL, 786, 3405, 0.30, 1, 5, 0),
(9500066, NULL, 787, 3381, 0.30, 1, 5, 0),
(9500067, NULL, 788, 3390, 0.30, 2, 5, 0),
(9500068, NULL, 785, 3395, 0.30, 1, 5, 0),
(9500069, NULL, 789, 3411, 0.30, 2, 5, 0),
(9500070, NULL, 799, 3482, 0.30, 1, 1, 0),
(9500071, NULL, 806, 3538, 0.30, 1, 5, 0),
(9500072, NULL, 813, 3570, 0.30, 1, 5, 0),
(9500073, NULL, 829, 3750, 0.30, 5, 5, 0),
(9500074, NULL, 834, 3827, 0.30, 1, 5, 0),
(9500075, NULL, 836, 3858, 0.30, 5, 5, 0),
(9500076, NULL, 837, 3844, 0.30, 5, 5, 0),
(9500077, NULL, 838, 3839, 0.30, 5, 5, 0),
(9500078, NULL, 449, 4050, 0.30, 3, 5, 0),
(9500079, NULL, 167, 4161, 0.30, 1, 5, 0),
(9500080, NULL, 165, 4163, 0.30, 1, 5, 0),
(9500081, NULL, 872, 4617, 0.30, 2, 5, 0),
(9500082, NULL, 873, 4608, 0.30, 2, 5, 0),
(9500083, NULL, 878, 4727, 0.30, 1, 5, 0),
(9500084, NULL, 881, 4743, 0.30, 1, 5, 0),
(9500085, NULL, 883, 4804, 0.30, 1, 5, 0),
(9500086, NULL, 526, 4861, 0.30, 2, 5, 0),
(9500087, NULL, 517, 4877, 0.30, 1, 5, 0),
(9500088, NULL, 892, 4968, 0.30, 1, 5, 0),
(9500089, NULL, 897, 5111, 0.30, 2, 5, 0),
(9500090, NULL, 896, 5101, 0.30, 1, 5, 0),
(9500091, NULL, 902, 5314, 0.30, 1, 5, 0);
