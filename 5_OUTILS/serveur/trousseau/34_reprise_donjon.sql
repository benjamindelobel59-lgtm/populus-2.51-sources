-- MYSTIC 17/10/2026 : « Reprendre le donjon où vous l'avez quitté » (DungeonResume, Parameter0 = donjon) chez les gardiens,
-- et suppression de « Téléporter les membres du groupe » (choix de l'utilisateur). Généré par _OUTILS/serveur/trousseau/reprise_donjon.py.
-- Nécessite le WorldServer du 17/10 17h10+ (RepriseDonjons). Rejouable.
DELETE FROM npcs_replies WHERE Type = 'DungeonResume';
DELETE FROM npcs_replies WHERE Type = 'DungeonEnter' AND ReplyId IN (11068,11145,11146,11147,11148,11149,11150,11151,11152,11153,11154,11156,11157,11158,11159,11160,11161,11162,11163,11164,11165,11166,11167,11168,11169,11170,11171,11172,11174,11175,11176,11177,11178,11179,11180,11181,11182,11183,11184,11185,11186,11187,11188,11189,11190,11191,11192,11193,11194,11195,11196,11197,11366,13404,13465,14428,16967,16972,16977,16982,16987,17825,18855,18913,19377,19474,19538,19724,20725,21574,22754,23592,24477,24980,26244,26253,28558,29214,29235,31187,31385,31735,31746,32334,33073,33154,33177,34511,36507,36516,38239,38448,39150,43393,43401,43410);
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 8872, 3200, '2'); -- Reprendre la Bibliothèque du Maître Corbac où vous l'avez quittée.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 8881, 3216, '4'); -- Reprendre l'Antre du Dragon Cochon où vous l'avez quittée.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 8870, 3178, '6'); -- Reprendre le Donjon des Champs où vous l'avez quitté.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 8860, 7072, '10'); -- Reprendre la Caverne des Bulbes où vous l'avez quittée.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 8862, 3172, '11'); -- Reprendre le Donjon des Bworks où vous l'avez quitté.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 8864, 17027, '12'); -- Reprendre la Grotte du Bworker où vous l'avez quittée.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 8866, 3177, '13'); -- Reprendre le Donjon des Canidés où vous l'avez quitté.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 8868, 478, '14'); -- Reprendre le Terrier du Wa Wabbit où vous l'avez quitté.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 8885, 3289, '16'); -- Reprendre le Château Ensablé où vous l'avez quitté.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 8889, 2943, '17'); -- Reprendre le Donjon des Firefoux où vous l'avez quitté.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 8891, 7069, '18'); -- Reprendre le Donjon des Forgerons où vous l'avez quitté.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 8893, 5471, '19'); -- Reprendre le Temple du Grand Ougah où vous l'avez quitté.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 8913, 2941, '20'); -- Reprendre le Donjon des Kitsounes où vous l'avez quitté.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 8895, 4190, '21'); -- Reprendre la Grotte où vous l'avez quittée.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 8902, 8271, '23'); -- Reprendre le Donjon Kanniboul où vous l'avez quitté.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 8911, 4206, '24'); -- Reprendre la Canopée du Kimbo où vous l'avez quittée.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 8913, 2941, '25'); -- Reprendre le Donjon des Kitsounes où vous l'avez quitté.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 8915, 3119, '26'); -- Reprendre le Donjon du Koulosse où vous l'avez quitté.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 8913, 2941, '27'); -- Reprendre le Donjon des Kitsounes où vous l'avez quitté.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 8917, 8507, '28'); -- Reprendre le Nid du Kwakwa où vous l'avez quitté.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 8921, 8321, '30'); -- Reprendre la Maison Fantôme où vous l'avez quittée.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 8929, 8667, '31'); -- Reprendre le Laboratoire de Brumen Tinctorias où vous l'avez quitté.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 8931, 5494, '32'); -- Reprendre le Donjon de de Nowel où vous l'avez quitté.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 8931, 5494, '33'); -- Reprendre le Donjon de de Nowel où vous l'avez quitté.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 8931, 5494, '34'); -- Reprendre le Donjon de de Nowel où vous l'avez quitté.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 8938, 4192, '35'); -- Reprendre l'Arche d'Otomaï où vous l'avez quittée.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 8940, 2940, '36'); -- Reprendre le Repaire des Pandikazes où vous l'avez quitté.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 8943, 4203, '37'); -- Reprendre le Goulet du Rasboul où vous l'avez quitté.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 8945, 3292, '38'); -- Reprendre le Donjon des Rats de Bonta où vous l'avez quitté.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 8949, 3415, '40'); -- Reprendre le Donjon des Rats du Château d'Amakna où vous l'avez quitté.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 8949, 3416, '40'); -- Reprendre le Donjon des Rats du Château d'Amakna où vous l'avez quitté.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 8951, 3179, '41'); -- Reprendre le Donjon des Scarafeuilles où vous l'avez quitté.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 8953, 3138, '42'); -- Reprendre le Repaire de Skeunk où vous l'avez quitté.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 8955, 7070, '43'); -- Reprendre le Donjon des Squelettes où vous l'avez quitté.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 8957, 3175, '44'); -- Reprendre le Donjon des Tofus où vous l'avez quitté.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 13403, 3176, '44'); -- Reprendre le Tofulailler Royal où vous l'avez quitté.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 13403, 17102, '44'); -- Reprendre le Tofulailler Royal où vous l'avez quitté.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 8957, 3175, '46'); -- Reprendre le Donjon des Tofus où vous l'avez quitté.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 13403, 3176, '46'); -- Reprendre le Tofulailler Royal où vous l'avez quitté.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 13403, 17102, '46'); -- Reprendre le Tofulailler Royal où vous l'avez quitté.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 8962, 4197, '47'); -- Reprendre le Laboratoire du Tynril où vous l'avez quitté.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 8982, 8886, '48'); -- Reprendre le Château du Wa Wabbit où vous l'avez quitté.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 8989, 8153, '58'); -- Reprendre le Donjon de la mine de Sakaï où vous l'avez quitté.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 14427, 12671, '62'); -- Reprendre le Potager d'Halouine où vous l'avez quitté.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 14427, 12672, '62'); -- Reprendre le Potager d'Halouine où vous l'avez quitté.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 16971, 14522, '63'); -- Reprendre le Transporteur de Sylargh où vous l'avez quitté.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 16971, 17196, '63'); -- Reprendre le Transporteur de Sylargh où vous l'avez quitté.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 16986, 14528, '65'); -- Reprendre la Forgefroide de Missiz Frizz où vous l'avez quittée.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 16966, 14520, '66'); -- Reprendre le Laboratoire de Nileza où vous l'avez quitté.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 17824, 15157, '67'); -- Reprendre le Donjon du Comte Harebourg où vous l'avez quitté.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 19376, 16317, '68'); -- Reprendre le Théâtre de Dramak où vous l'avez quitté.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 19537, 16404, '69'); -- Reprendre l'Aquadôme de Merkator où vous l'avez quitté.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 19718, 16553, '70'); -- Reprendre la Pyramide d'Ombre où vous l'avez quittée.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 21432, 17885, '72'); -- Reprendre le Palais du roi Nidas là où vous l'avez quitté.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 21432, 17887, '72'); -- Reprendre le Palais du roi Nidas là où vous l'avez quitté.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 21150, 17637, '73'); -- Reprendre la Fabrique de Malléfisk où vous l'avez quittée.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 21219, 17710, '74'); -- Reprendre la galerie du Phossile où vous l'avez quittée.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 21567, 17977, '75'); -- Reprendre la Volière de la Haute Truche où vous l'avez quittée.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 21899, 18286, '76'); -- Reprendre le Ring du Capitaine Ekarlatte où vous l'avez quitté.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 21909, 18292, '77'); -- Reprendre la Cave du Toxoliath où vous l'avez quittée.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 21904, 18289, '78'); -- Reprendre le Trône de la Cour Sombre où vous l'avez quitté.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 22746, 19017, '79'); -- Reprendre le Ventre de la Baleine où vous l'avez quitté.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 22746, 19079, '79'); -- Reprendre le Ventre de la Baleine où vous l'avez quitté.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 23373, 19557, '80'); -- Reprendre le Mégalithe de Fraktale où vous l'avez quitté.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 23141, 19397, '81'); -- Reprendre l'Horologium de XLII où vous l'avez quitté.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 23351, 19539, '82'); -- Reprendre l'Œil de Vortex où vous l'avez quitté.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 23585, 19697, '83'); -- Reprendre la Cache de Kankreblath où vous l'avez quittée.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 24470, 20453, '84'); -- Reprendre l'Antre de la Reine Nyée où vous l'avez quitté.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 26246, 21818, '86'); -- Reprendre le Bateau du Chouque où vous l'avez quitté.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 26237, 21820, '87'); -- Reprendre l'Arbre de Moon où vous l'avez quitté.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 27267, 22674, '88'); -- Reprendre le Miausolée du Pounicheur où vous l'avez quitté.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 27273, 22678, '89'); -- Reprendre le Plateau de Ush où vous l'avez quitté.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 27292, 22696, '90'); -- Reprendre le Défi du Chalœil où vous l'avez quitté.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 8913, 2941, '91'); -- Reprendre le Donjon des Kitsounes où vous l'avez quitté.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 28551, 23764, '92'); -- Reprendre le Vaisseau du Capitaine Meno où vous l'avez quitté.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 29228, 24326, '94'); -- Reprendre le Palais de Dantinéa où vous l'avez quitté.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 31728, 26439, '97'); -- Reprendre le Boyau du Père Ver où vous l'avez quitté.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 31739, 26444, '98'); -- Reprendre La Chambre de Tal Kasha où vous l'avez quittée.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 32327, 26892, '99'); -- Reprendre Le Camp du Comte Razof où vous l'avez quitté.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 33147, 27619, '100'); -- Reprendre le Chapiteau des Magik Riktus où vous l'avez quitté.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 33170, 27635, '101'); -- Reprendre Le Manoir des Katrepat où vous l'avez quitté.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 8913, 2941, '102'); -- Reprendre le Donjon des Kitsounes où vous l'avez quitté.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 36509, 30312, '103'); -- Reprendre la Tour de Bethel où vous l'avez quittée.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 36500, 30308, '104'); -- Reprendre la Tour de Solar où vous l'avez quittée.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 38441, 31789, '105'); -- Reprendre la Brasserie du roi Dazak où vous l'avez quittée.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 8978, 8459, '57'); -- Reprendre l'Antichambre des Gloursons où vous l'avez quittée.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 8974, 8427, '55'); -- Reprendre l'Antre du Korriandre où vous l'avez quittée.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 8964, 7685, '50'); -- Reprendre la Serre du Royalmouth où vous l'avez quittée.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 8968, 8107, '52'); -- Reprendre l'Épave du Grolandais violent où vous l'avez quittée.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 8970, 8109, '53'); -- Reprendre l'Hypogée de l'Obsidiantre où vous l'avez quittée.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 10731, 9945, '60'); -- Reprendre le Repaire de Daïgoro où vous l'avez quitté.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 20722, 17316, '71'); -- Reprendre la Grotte de Kanigroula où vous l'avez quittée.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 24967, 20830, '85'); -- Reprendre la Crypte de Kardorim où vous l'avez quittée.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 31179, 26005, '95'); -- Reprendre le Cimetière des mastodontes où vous l'avez quitté.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 31378, 26148, '96'); -- Reprendre la Caverne d'El Piko où vous l'avez quittée.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 38441, 31789, '110'); -- Reprendre la Brasserie du roi Dazak où vous l'avez quittée.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 10532, 646, '1'); -- Reprendre la Cour du Bouftou Royal où vous l'avez quittée.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 13464, 11970, '7'); -- Reprendre le Domaine Ancestral où vous l'avez quitté.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 8854, 5402, '9'); -- Reprendre le Clos des Blops où vous l'avez quitté.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 8854, 5402, '45'); -- Reprendre le Clos des Blops où vous l'avez quitté.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 8947, 3294, '39'); -- Reprendre le Donjon des Rats de Brâkmar où vous l'avez quitté.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 8966, 7834, '51'); -- Reprendre l'Excavation du Mansot Royal où vous l'avez quittée.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 29207, 24312, '93'); -- Reprendre le Temple de Koutoulou où vous l'avez quitté.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 33170, 27635, '106'); -- Reprendre Le Manoir des Katrepat où vous l'avez quitté.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 43390, 33783, '111'); -- Reprendre le Temple du dieu Kao où vous l'avez quittée.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 36509, 30312, '108'); -- Reprendre la Tour de Bethel où vous l'avez quittée.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 36500, 30308, '109'); -- Reprendre la Tour de Solar où vous l'avez quittée.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 43407, 33782, '112'); -- Reprendre la Croquanterie où vous l'avez quittée.
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', 43398, 33781, '113'); -- Reprendre la Fonderie des Waddicts où vous l'avez quittée.
-- Contrôle : attendu groupe_restants = 0
SELECT (SELECT COUNT(*) FROM npcs_replies WHERE Type = 'DungeonResume') AS reprises,
       (SELECT COUNT(*) FROM npcs_replies WHERE Type = 'DungeonEnter' AND ReplyId IN (11068,11145,11146,11147,11148,11149,11150,11151,11152,11153,11154,11156,11157,11158,11159,11160,11161,11162,11163,11164,11165,11166,11167,11168,11169,11170,11171,11172,11174,11175,11176,11177,11178,11179,11180,11181,11182,11183,11184,11185,11186,11187,11188,11189,11190,11191,11192,11193,11194,11195,11196,11197,11366,13404,13465,14428,16967,16972,16977,16982,16987,17825,18855,18913,19377,19474,19538,19724,20725,21574,22754,23592,24477,24980,26244,26253,28558,29214,29235,31187,31385,31735,31746,32334,33073,33154,33177,34511,36507,36516,38239,38448,39150,43393,43401,43410)) AS groupe_restants;
