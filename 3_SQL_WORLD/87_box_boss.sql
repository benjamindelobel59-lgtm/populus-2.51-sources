-- PADGREF 21/10/2026 : SQL 87 — box de boss (généré par _OUTILS/client/client-build/box_boss/generer.py).
-- 108 box (objets 31300 à 31407). Base world. Rejouable. Serveur : game/items/player/Custom/BoxDeBoss.cs.
-- Client : Items.d2o + i18n_fr.d2i + items/bitmap1_1.d2p + items/vector1_1.d2p (dossier box_boss/client).
SET NAMES utf8mb4;
USE `world`;
START TRANSACTION;
CREATE TABLE IF NOT EXISTS padgref_box_boss (ItemId INT NOT NULL PRIMARY KEY, BossIdsCSV VARCHAR(255) NOT NULL DEFAULT '', RaresCSV VARCHAR(1000) NOT NULL DEFAULT '', PanoplieCSV VARCHAR(1000) NOT NULL DEFAULT '');
DELETE FROM padgref_box_boss;
DELETE FROM items_templates WHERE Id BETWEEN 31300 AND 31499;
DELETE FROM items_templates WHERE Id = 31300;
INSERT INTO items_templates SELECT 31300, 'Box de Bouftou Royal', `Type`, AppearanceId, Weight, RealWeight, 9001300, TypeId, 9001301, 990100, 32, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31300, '147', '880', '2438,2440,2441,2442,2443,2444,2445');
DELETE FROM items_templates WHERE Id = 31301;
INSERT INTO items_templates SELECT 31301, 'Box de Maître Corbac', `Type`, AppearanceId, Weight, RealWeight, 9001302, TypeId, 9001303, 990101, 110, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31301, '289', '2525,2527', '2544,2545,2546,6449');
DELETE FROM items_templates WHERE Id = 31302;
INSERT INTO items_templates SELECT 31302, 'Box de Minotoror', `Type`, AppearanceId, Weight, RealWeight, 9001304, TypeId, 9001305, 990102, 120, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31302, '121', '1680,2666,2667,2999,8313', '707,8268,8269,8274,8276,8279,8282');
DELETE FROM items_templates WHERE Id = 31303;
INSERT INTO items_templates SELECT 31303, 'Box de Dragon Cochon', `Type`, AppearanceId, Weight, RealWeight, 9001306, TypeId, 9001307, 990103, 108, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31303, '113', '477,481,487,2267,2643,2644,8393', '233,2808,8271,8273,8278,8281,8285');
DELETE FROM items_templates WHERE Id = 31304;
INSERT INTO items_templates SELECT 31304, 'Box de Gorgouille', `Type`, AppearanceId, Weight, RealWeight, 9001308, TypeId, 9001309, 990104, 103, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31304, '3137', '12374', '12370,12371,12372,12373');
DELETE FROM items_templates WHERE Id = 31305;
INSERT INTO items_templates SELECT 31305, 'Box de Crocabulia', `Type`, AppearanceId, Weight, RealWeight, 9001310, TypeId, 9001311, 990105, 128, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31305, '854', '8365,8368', '8286,8287,8289');
DELETE FROM items_templates WHERE Id = 31306;
INSERT INTO items_templates SELECT 31306, 'Box de Tournesol Affamé', `Type`, AppearanceId, Weight, RealWeight, 9001312, TypeId, 9001313, 990106, 20, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31306, '799', '8157', '8123,8124,8125,8126,8127,8128,8131');
DELETE FROM items_templates WHERE Id = 31307;
INSERT INTO items_templates SELECT 31307, 'Box de Abraknyde Ancestral', `Type`, AppearanceId, Weight, RealWeight, 9001314, TypeId, 9001315, 990107, 114, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31307, '173', '8495', '8463,8464,8465,8466,8467,8468');
DELETE FROM items_templates WHERE Id = 31308;
INSERT INTO items_templates SELECT 31308, 'Box de Chêne Mou', `Type`, AppearanceId, Weight, RealWeight, 9001316, TypeId, 9001317, 990108, 145, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31308, '257', '6487,6490', '8469,8470,8471,8472,8473,8474');
DELETE FROM items_templates WHERE Id = 31309;
INSERT INTO items_templates SELECT 31309, 'Box de Blop Coco Royal', `Type`, AppearanceId, Weight, RealWeight, 9001318, TypeId, 9001319, 990109, 80, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31309, '1184', '9385', '9128,9155,9164,9173');
DELETE FROM items_templates WHERE Id = 31310;
INSERT INTO items_templates SELECT 31310, 'Box de Blop Griotte Royal', `Type`, AppearanceId, Weight, RealWeight, 9001320, TypeId, 9001321, 990110, 80, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31310, '1185', '9384', '9126,9153,9162,9171');
DELETE FROM items_templates WHERE Id = 31311;
INSERT INTO items_templates SELECT 31311, 'Box de Blop Indigo Royal', `Type`, AppearanceId, Weight, RealWeight, 9001322, TypeId, 9001323, 990111, 80, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31311, '1186', '9386', '9129,9156,9165,9174');
DELETE FROM items_templates WHERE Id = 31312;
INSERT INTO items_templates SELECT 31312, 'Box de Blop Reinette Royal', `Type`, AppearanceId, Weight, RealWeight, 9001324, TypeId, 9001325, 990112, 80, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31312, '1187', '9387', '9127,9154,9163,9172');
DELETE FROM items_templates WHERE Id = 31313;
INSERT INTO items_templates SELECT 31313, 'Box de Bulbig Brozeur', `Type`, AppearanceId, Weight, RealWeight, 9001326, TypeId, 9001327, 990113, 50, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31313, '3232', '16211', '');
DELETE FROM items_templates WHERE Id = 31314;
INSERT INTO items_templates SELECT 31314, 'Box de Bworkette', `Type`, AppearanceId, Weight, RealWeight, 9001328, TypeId, 9001329, 990114, 50, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31314, '792', '8389', '');
DELETE FROM items_templates WHERE Id = 31315;
INSERT INTO items_templates SELECT 31315, 'Box de Bworker', `Type`, AppearanceId, Weight, RealWeight, 9001330, TypeId, 9001331, 990115, 189, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31315, '478', '9401', '9178,9181,9182,9183');
DELETE FROM items_templates WHERE Id = 31316;
INSERT INTO items_templates SELECT 31316, 'Box de Meulou', `Type`, AppearanceId, Weight, RealWeight, 9001332, TypeId, 9001333, 990116, 143, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31316, '232', '2580,2581,2582,2806,8144', '8262,8263,8264,8265,8266,8267');
DELETE FROM items_templates WHERE Id = 31317;
INSERT INTO items_templates SELECT 31317, 'Box de Wa Wobot', `Type`, AppearanceId, Weight, RealWeight, 9001334, TypeId, 9001335, 990117, 83, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31317, '3460', '14468', '14491,14492,14493');
DELETE FROM items_templates WHERE Id = 31318;
INSERT INTO items_templates SELECT 31318, 'Box de Craqueleur Légendaire', `Type`, AppearanceId, Weight, RealWeight, 9001336, TypeId, 9001337, 990118, 86, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31318, '669', '8394', '8146,8147,8148,8149,8150,8152,8181');
DELETE FROM items_templates WHERE Id = 31319;
INSERT INTO items_templates SELECT 31319, 'Box de Mob l''Éponge', `Type`, AppearanceId, Weight, RealWeight, 9001338, TypeId, 9001339, 990119, 25, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31319, '928', '8682', '8253,8254,8255,8256,8257,8258,8259,18659');
DELETE FROM items_templates WHERE Id = 31320;
INSERT INTO items_templates SELECT 31320, 'Box de Péki Péki', `Type`, AppearanceId, Weight, RealWeight, 9001340, TypeId, 9001341, 990120, 140, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31320, '605', '8380,8381', '');
DELETE FROM items_templates WHERE Id = 31321;
INSERT INTO items_templates SELECT 31321, 'Box de Coffre des Forgerons', `Type`, AppearanceId, Weight, RealWeight, 9001342, TypeId, 9001343, 990121, 56, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31321, '252', '13140', '13136,13137,13138,13139');
DELETE FROM items_templates WHERE Id = 31322;
INSERT INTO items_templates SELECT 31322, 'Box de Ougah', `Type`, AppearanceId, Weight, RealWeight, 9001344, TypeId, 9001345, 990122, 185, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31322, '1159', '9281', '7680,9117,9130,9146');
DELETE FROM items_templates WHERE Id = 31323;
INSERT INTO items_templates SELECT 31323, 'Box de Gelée Royale Bleue', `Type`, AppearanceId, Weight, RealWeight, 9001346, TypeId, 9001347, 990123, 60, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31323, '58', '370', '2446,2447,2469,2470,2471,2472');
DELETE FROM items_templates WHERE Id = 31324;
INSERT INTO items_templates SELECT 31324, 'Box de Gelée Royale Menthe', `Type`, AppearanceId, Weight, RealWeight, 9001348, TypeId, 9001349, 990124, 60, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31324, '85', '2241', '2446,2447,2469,2470,2471,2472');
DELETE FROM items_templates WHERE Id = 31325;
INSERT INTO items_templates SELECT 31325, 'Box de Gelée Royale Fraise', `Type`, AppearanceId, Weight, RealWeight, 9001350, TypeId, 9001351, 990125, 60, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31325, '86', '2242', '2446,2447,2469,2470,2471,2472');
DELETE FROM items_templates WHERE Id = 31326;
INSERT INTO items_templates SELECT 31326, 'Box de Gelée Royale Citron', `Type`, AppearanceId, Weight, RealWeight, 9001352, TypeId, 9001353, 990126, 60, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31326, '430', '2437', '2446,2447,2469,2470,2471,2472');
DELETE FROM items_templates WHERE Id = 31327;
INSERT INTO items_templates SELECT 31327, 'Box de Corailleur Magistral', `Type`, AppearanceId, Weight, RealWeight, 9001354, TypeId, 9001355, 990127, 50, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31327, '1027', '8731,8994', '');
DELETE FROM items_templates WHERE Id = 31328;
INSERT INTO items_templates SELECT 31328, 'Box de Kanniboul Ebil', `Type`, AppearanceId, Weight, RealWeight, 9001356, TypeId, 9001357, 990128, 60, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31328, '2960', '11696', '11977,11979,11982,11983');
DELETE FROM items_templates WHERE Id = 31329;
INSERT INTO items_templates SELECT 31329, 'Box de Kimbo', `Type`, AppearanceId, Weight, RealWeight, 9001358, TypeId, 9001359, 990129, 196, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31329, '1045', '8789,8998', '8861,9461');
DELETE FROM items_templates WHERE Id = 31330;
INSERT INTO items_templates SELECT 31330, 'Box de Tanukouï San', `Type`, AppearanceId, Weight, RealWeight, 9001360, TypeId, 9001361, 990130, 130, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31330, '568', '7386', '');
DELETE FROM items_templates WHERE Id = 31331;
INSERT INTO items_templates SELECT 31331, 'Box de Koulosse', `Type`, AppearanceId, Weight, RealWeight, 9001362, TypeId, 9001363, 990131, 100, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31331, '670', '8402', '7887,7888,7889,7890');
DELETE FROM items_templates WHERE Id = 31332;
INSERT INTO items_templates SELECT 31332, 'Box de Kralamoure Géant', `Type`, AppearanceId, Weight, RealWeight, 9001364, TypeId, 9001365, 990132, 200, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31332, '423', '8812', '8877,8991,9464');
DELETE FROM items_templates WHERE Id = 31333;
INSERT INTO items_templates SELECT 31333, 'Box de Kwakwa', `Type`, AppearanceId, Weight, RealWeight, 9001366, TypeId, 9001367, 990133, 60, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31333, '2995', '12015', '12018,12019,12020,12021');
DELETE FROM items_templates WHERE Id = 31334;
INSERT INTO items_templates SELECT 31334, 'Box de Shin Larve', `Type`, AppearanceId, Weight, RealWeight, 9001368, TypeId, 9001369, 990134, 59, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31334, '457', '8386', '11384,11386');
DELETE FROM items_templates WHERE Id = 31335;
INSERT INTO items_templates SELECT 31335, 'Box de Boostache', `Type`, AppearanceId, Weight, RealWeight, 9001370, TypeId, 9001371, 990135, 48, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31335, '2975', '11814', '11976,11978,11980,11981');
DELETE FROM items_templates WHERE Id = 31336;
INSERT INTO items_templates SELECT 31336, 'Box de Nelween', `Type`, AppearanceId, Weight, RealWeight, 9001372, TypeId, 9001373, 990136, 78, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31336, '3100', '12075', '12069,12070,12071,12072');
DELETE FROM items_templates WHERE Id = 31337;
INSERT INTO items_templates SELECT 31337, 'Box de Père Fwetar', `Type`, AppearanceId, Weight, RealWeight, 9001374, TypeId, 9001375, 990137, 199, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31337, '1194', '', '15009,15010,15011');
DELETE FROM items_templates WHERE Id = 31338;
INSERT INTO items_templates SELECT 31338, 'Box de Gourlo le Terrible', `Type`, AppearanceId, Weight, RealWeight, 9001376, TypeId, 9001377, 990138, 70, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31338, '1051', '8761,8995', '');
DELETE FROM items_templates WHERE Id = 31339;
INSERT INTO items_templates SELECT 31339, 'Box de Maître Pandore', `Type`, AppearanceId, Weight, RealWeight, 9001378, TypeId, 9001379, 990139, 200, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31339, '612', '7307,8399,8400,8401', '');
DELETE FROM items_templates WHERE Id = 31340;
INSERT INTO items_templates SELECT 31340, 'Box de Silf le Rasboul Majeur', `Type`, AppearanceId, Weight, RealWeight, 9001380, TypeId, 9001381, 990140, 110, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31340, '1071', '8798,8996', '');
DELETE FROM items_templates WHERE Id = 31341;
INSERT INTO items_templates SELECT 31341, 'Box de Rat Blanc', `Type`, AppearanceId, Weight, RealWeight, 9001382, TypeId, 9001383, 990141, 110, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31341, '940', '8489', '8450,8451,8452,8453,8454,8455,8456');
DELETE FROM items_templates WHERE Id = 31342;
INSERT INTO items_templates SELECT 31342, 'Box de Rat Noir', `Type`, AppearanceId, Weight, RealWeight, 9001384, TypeId, 9001385, 990142, 110, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31342, '939', '8485', '8442,8443,8444,8445,8446,8447,8448');
DELETE FROM items_templates WHERE Id = 31343;
INSERT INTO items_templates SELECT 31343, 'Box de Sphincter Cell', `Type`, AppearanceId, Weight, RealWeight, 9001386, TypeId, 9001387, 990143, 150, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31343, '943', '8487', '8457,8458,8459,8460,8461,8462');
DELETE FROM items_templates WHERE Id = 31344;
INSERT INTO items_templates SELECT 31344, 'Box de Scarabosse Doré', `Type`, AppearanceId, Weight, RealWeight, 9001388, TypeId, 9001389, 990144, 59, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31344, '797', '8137', '8116,8117,8118,8119,8120,8121,8122');
DELETE FROM items_templates WHERE Id = 31345;
INSERT INTO items_templates SELECT 31345, 'Box de Skeunk', `Type`, AppearanceId, Weight, RealWeight, 9001390, TypeId, 9001391, 990145, 120, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31345, '780', '8404', '');
DELETE FROM items_templates WHERE Id = 31346;
INSERT INTO items_templates SELECT 31346, 'Box de Chafer Ronin', `Type`, AppearanceId, Weight, RealWeight, 9001392, TypeId, 9001393, 990146, 45, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31346, '3238', '13096', '13091,13092,13093,13094');
DELETE FROM items_templates WHERE Id = 31347;
INSERT INTO items_templates SELECT 31347, 'Box de Batofu', `Type`, AppearanceId, Weight, RealWeight, 9001394, TypeId, 9001395, 990147, 40, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31347, '800', '8557', '');
DELETE FROM items_templates WHERE Id = 31348;
INSERT INTO items_templates SELECT 31348, 'Box de Blop Multicolore Royal', `Type`, AppearanceId, Weight, RealWeight, 9001396, TypeId, 9001397, 990148, 150, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31348, '1188', '9391', '9148,9157,9166,9175');
DELETE FROM items_templates WHERE Id = 31349;
INSERT INTO items_templates SELECT 31349, 'Box de Tofu Royal', `Type`, AppearanceId, Weight, RealWeight, 9001398, TypeId, 9001399, 990149, 120, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31349, '382', '2246,8387', '');
DELETE FROM items_templates WHERE Id = 31350;
INSERT INTO items_templates SELECT 31350, 'Box de Tynril Perfide', `Type`, AppearanceId, Weight, RealWeight, 9001400, TypeId, 9001401, 990150, 140, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31350, '1086,1072,1085,1087', '8770,8916,8997', '');
DELETE FROM items_templates WHERE Id = 31351;
INSERT INTO items_templates SELECT 31351, 'Box de Wa Wabbit', `Type`, AppearanceId, Weight, RealWeight, 9001402, TypeId, 9001403, 990151, 60, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31351, '180', '8398', '6492,6498,6499');
DELETE FROM items_templates WHERE Id = 31352;
INSERT INTO items_templates SELECT 31352, 'Box de Minotot', `Type`, AppearanceId, Weight, RealWeight, 9001404, TypeId, 9001405, 990152, 160, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31352, '827', '8406,8407,8408,8409,8410', '8270,8272,8275,8277,8280,8283,8284');
DELETE FROM items_templates WHERE Id = 31353;
INSERT INTO items_templates SELECT 31353, 'Box de Royalmouth', `Type`, AppearanceId, Weight, RealWeight, 9001406, TypeId, 9001407, 990153, 130, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31353, '2854', '11219', '11571,11575,11582,11584');
DELETE FROM items_templates WHERE Id = 31354;
INSERT INTO items_templates SELECT 31354, 'Box de Mansot Royal', `Type`, AppearanceId, Weight, RealWeight, 9001408, TypeId, 9001409, 990154, 144, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31354, '2848', '11233', '11572,11577,11580');
DELETE FROM items_templates WHERE Id = 31355;
INSERT INTO items_templates SELECT 31355, 'Box de Ben le Ripate', `Type`, AppearanceId, Weight, RealWeight, 9001410, TypeId, 9001411, 990155, 165, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31355, '2877', '11322', '11573,11578,11585,11586');
DELETE FROM items_templates WHERE Id = 31356;
INSERT INTO items_templates SELECT 31356, 'Box de Obsidiantre', `Type`, AppearanceId, Weight, RealWeight, 9001412, TypeId, 9001413, 990156, 185, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31356, '2924', '11334', '11574,11576,11579,11581,11583');
DELETE FROM items_templates WHERE Id = 31357;
INSERT INTO items_templates SELECT 31357, 'Box de Tengu Givrefoux', `Type`, AppearanceId, Weight, RealWeight, 9001414, TypeId, 9001415, 990157, 177, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31357, '2967', '11342', '11707,11708,11709,11710');
DELETE FROM items_templates WHERE Id = 31358;
INSERT INTO items_templates SELECT 31358, 'Box de Fuji Givrefoux Nourricière', `Type`, AppearanceId, Weight, RealWeight, 9001416, TypeId, 9001417, 990158, 195, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31358, '3234', '11883', '12117,12118,12119,12120');
DELETE FROM items_templates WHERE Id = 31359;
INSERT INTO items_templates SELECT 31359, 'Box de Korriandre', `Type`, AppearanceId, Weight, RealWeight, 9001418, TypeId, 9001419, 990159, 188, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31359, '2968', '11896', '11711,11712,11713,11714');
DELETE FROM items_templates WHERE Id = 31360;
INSERT INTO items_templates SELECT 31360, 'Box de Kolosso', `Type`, AppearanceId, Weight, RealWeight, 9001420, TypeId, 9001421, 990160, 193, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31360, '2986', '11932', '11715,11716,11717');
DELETE FROM items_templates WHERE Id = 31361;
INSERT INTO items_templates SELECT 31361, 'Box de Professeur Xa', `Type`, AppearanceId, Weight, RealWeight, 9001422, TypeId, 9001423, 990161, 195, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31361, '2992', '', '12108,12109,12110,12111,12112');
DELETE FROM items_templates WHERE Id = 31362;
INSERT INTO items_templates SELECT 31362, 'Box de Glourséleste', `Type`, AppearanceId, Weight, RealWeight, 9001424, TypeId, 9001425, 990162, 199, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31362, '2864', '11944', '11718,11719,11720,11721,11722');
DELETE FROM items_templates WHERE Id = 31363;
INSERT INTO items_templates SELECT 31363, 'Box de Grolloum', `Type`, AppearanceId, Weight, RealWeight, 9001426, TypeId, 9001427, 990163, 196, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31363, '2942', '11516,11517', '11470,11471,11472,11514');
DELETE FROM items_templates WHERE Id = 31364;
INSERT INTO items_templates SELECT 31364, 'Box de Daïgoro', `Type`, AppearanceId, Weight, RealWeight, 9001428, TypeId, 9001429, 990164, 88, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31364, '3208', '12742', '12674,12675,12676,12677');
DELETE FROM items_templates WHERE Id = 31365;
INSERT INTO items_templates SELECT 31365, 'Box de Halouine', `Type`, AppearanceId, Weight, RealWeight, 9001430, TypeId, 9001431, 990165, 106, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31365, '3306', '13334', '13326,13327,13328,13329');
DELETE FROM items_templates WHERE Id = 31366;
INSERT INTO items_templates SELECT 31366, 'Box de Sylargh', `Type`, AppearanceId, Weight, RealWeight, 9001432, TypeId, 9001433, 990166, 200, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31366, '3409', '13983', '14070,14071,14072');
DELETE FROM items_templates WHERE Id = 31367;
INSERT INTO items_templates SELECT 31367, 'Box de Klime', `Type`, AppearanceId, Weight, RealWeight, 9001434, TypeId, 9001435, 990167, 200, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31367, '3384', '13924', '14050,14051,14052');
DELETE FROM items_templates WHERE Id = 31368;
INSERT INTO items_templates SELECT 31368, 'Box de Missiz Frizz', `Type`, AppearanceId, Weight, RealWeight, 9001436, TypeId, 9001437, 990168, 200, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31368, '3391', '13936', '14060,14061,14062');
DELETE FROM items_templates WHERE Id = 31369;
INSERT INTO items_templates SELECT 31369, 'Box de Nileza', `Type`, AppearanceId, Weight, RealWeight, 9001438, TypeId, 9001439, 990169, 200, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31369, '3397', '13948', '14064,14065,14066');
DELETE FROM items_templates WHERE Id = 31370;
INSERT INTO items_templates SELECT 31370, 'Box de Comte Harebourg', `Type`, AppearanceId, Weight, RealWeight, 9001440, TypeId, 9001441, 990170, 200, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31370, '3416', '13995', '14076,14077,14078');
DELETE FROM items_templates WHERE Id = 31371;
INSERT INTO items_templates SELECT 31371, 'Box de Maître des Pantins', `Type`, AppearanceId, Weight, RealWeight, 9001442, TypeId, 9001443, 990171, 110, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31371, '3476', '14517', '15012,15013,15014');
DELETE FROM items_templates WHERE Id = 31372;
INSERT INTO items_templates SELECT 31372, 'Box de Merkator', `Type`, AppearanceId, Weight, RealWeight, 9001444, TypeId, 9001445, 990172, 200, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31372, '3534', '14865', '14877,14878,14879');
DELETE FROM items_templates WHERE Id = 31373;
INSERT INTO items_templates SELECT 31373, 'Box de Ombre', `Type`, AppearanceId, Weight, RealWeight, 9001446, TypeId, 9001447, 990173, 198, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31373, '3564', '14928', '14932,14933,14934');
DELETE FROM items_templates WHERE Id = 31374;
INSERT INTO items_templates SELECT 31374, 'Box de Kanigroula', `Type`, AppearanceId, Weight, RealWeight, 9001448, TypeId, 9001449, 990174, 160, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31374, '3556', '15040', '');
DELETE FROM items_templates WHERE Id = 31375;
INSERT INTO items_templates SELECT 31375, 'Box de Roi Nidas', `Type`, AppearanceId, Weight, RealWeight, 9001450, TypeId, 9001451, 990175, 200, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31375, '3648', '15176', '15186,15187,15188');
DELETE FROM items_templates WHERE Id = 31376;
INSERT INTO items_templates SELECT 31376, 'Box de Malléfisk', `Type`, AppearanceId, Weight, RealWeight, 9001452, TypeId, 9001453, 990176, 104, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31376, '3652', '15164', '15207,15208,15210');
DELETE FROM items_templates WHERE Id = 31377;
INSERT INTO items_templates SELECT 31377, 'Box de Phossile', `Type`, AppearanceId, Weight, RealWeight, 9001454, TypeId, 9001455, 990177, 151, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31377, '3651', '15170', '15198,15199,15200');
DELETE FROM items_templates WHERE Id = 31378;
INSERT INTO items_templates SELECT 31378, 'Box de Haute Truche', `Type`, AppearanceId, Weight, RealWeight, 9001456, TypeId, 9001457, 990178, 131, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31378, '3618', '15066', '15359,15360,15361');
DELETE FROM items_templates WHERE Id = 31379;
INSERT INTO items_templates SELECT 31379, 'Box de Capitaine Ekarlatte', `Type`, AppearanceId, Weight, RealWeight, 9001458, TypeId, 9001459, 990179, 132, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31379, '3753', '15457', '15506,15507,15508');
DELETE FROM items_templates WHERE Id = 31380;
INSERT INTO items_templates SELECT 31380, 'Box de Toxoliath', `Type`, AppearanceId, Weight, RealWeight, 9001460, TypeId, 9001461, 990180, 180, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31380, '3752', '15451', '');
DELETE FROM items_templates WHERE Id = 31381;
INSERT INTO items_templates SELECT 31381, 'Box de Reine des Voleurs', `Type`, AppearanceId, Weight, RealWeight, 9001462, TypeId, 9001463, 990181, 200, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31381, '3726', '15445', '15434,15435,15436');
DELETE FROM items_templates WHERE Id = 31382;
INSERT INTO items_templates SELECT 31382, 'Box de Protozorreur', `Type`, AppearanceId, Weight, RealWeight, 9001464, TypeId, 9001465, 990182, 200, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31382, '3828', '15689', '15692,15694,15696,15699');
DELETE FROM items_templates WHERE Id = 31383;
INSERT INTO items_templates SELECT 31383, 'Box de Fraktale', `Type`, AppearanceId, Weight, RealWeight, 9001466, TypeId, 9001467, 990183, 124, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31383, '3852', '15727', '15778,15780,15783');
DELETE FROM items_templates WHERE Id = 31384;
INSERT INTO items_templates SELECT 31384, 'Box de XLII', `Type`, AppearanceId, Weight, RealWeight, 9001468, TypeId, 9001469, 990184, 176, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31384, '3849', '15721', '15762,15764,15768');
DELETE FROM items_templates WHERE Id = 31385;
INSERT INTO items_templates SELECT 31385, 'Box de Vortex', `Type`, AppearanceId, Weight, RealWeight, 9001470, TypeId, 9001471, 990185, 200, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31385, '3835', '15715', '15743,15744,15745');
DELETE FROM items_templates WHERE Id = 31386;
INSERT INTO items_templates SELECT 31386, 'Box de Kankreblath', `Type`, AppearanceId, Weight, RealWeight, 9001472, TypeId, 9001473, 990186, 40, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31386, '3945', '16002', '15992,15993,15994,15995');
DELETE FROM items_templates WHERE Id = 31387;
INSERT INTO items_templates SELECT 31387, 'Box de Reine Nyée', `Type`, AppearanceId, Weight, RealWeight, 9001474, TypeId, 9001475, 990187, 94, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31387, '3996', '16181', '16176,16177,16178');
DELETE FROM items_templates WHERE Id = 31388;
INSERT INTO items_templates SELECT 31388, 'Box de Kardorim', `Type`, AppearanceId, Weight, RealWeight, 9001476, TypeId, 9001477, 990188, 12, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31388, '4051', '16525', '16500,16501,16502');
DELETE FROM items_templates WHERE Id = 31389;
INSERT INTO items_templates SELECT 31389, 'Box de Le Chouque', `Type`, AppearanceId, Weight, RealWeight, 9001478, TypeId, 9001479, 990189, 94, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31389, '230', '2623', '17109,17110,17111,18667');
DELETE FROM items_templates WHERE Id = 31390;
INSERT INTO items_templates SELECT 31390, 'Box de Moon', `Type`, AppearanceId, Weight, RealWeight, 9001480, TypeId, 9001481, 990190, 103, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31390, '226', '8384', '17092,17093,17094');
DELETE FROM items_templates WHERE Id = 31391;
INSERT INTO items_templates SELECT 31391, 'Box de Pounicheur', `Type`, AppearanceId, Weight, RealWeight, 9001482, TypeId, 9001483, 990191, 118, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31391, '4278', '17614', '17593,17594,17595');
DELETE FROM items_templates WHERE Id = 31392;
INSERT INTO items_templates SELECT 31392, 'Box de Ush Galesh', `Type`, AppearanceId, Weight, RealWeight, 9001484, TypeId, 9001485, 990192, 162, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31392, '4264', '17620', '17590,17591,17592');
DELETE FROM items_templates WHERE Id = 31393;
INSERT INTO items_templates SELECT 31393, 'Box de Chalœil', `Type`, AppearanceId, Weight, RealWeight, 9001486, TypeId, 9001487, 990193, 200, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31393, '4263', '17626', '17578,17579,17580');
DELETE FROM items_templates WHERE Id = 31394;
INSERT INTO items_templates SELECT 31394, 'Box de Capitaine Meno', `Type`, AppearanceId, Weight, RealWeight, 9001488, TypeId, 9001489, 990194, 200, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31394, '4460', '17976', '18001,18003');
DELETE FROM items_templates WHERE Id = 31395;
INSERT INTO items_templates SELECT 31395, 'Box de Larve de Koutoulou', `Type`, AppearanceId, Weight, RealWeight, 9001490, TypeId, 9001491, 990195, 200, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31395, '4453', '17989', '18004,18005');
DELETE FROM items_templates WHERE Id = 31396;
INSERT INTO items_templates SELECT 31396, 'Box de Dantinéa', `Type`, AppearanceId, Weight, RealWeight, 9001492, TypeId, 9001493, 990196, 200, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31396, '4444', '17982', '18029,18030,18031');
DELETE FROM items_templates WHERE Id = 31397;
INSERT INTO items_templates SELECT 31397, 'Box de Mantiscore', `Type`, AppearanceId, Weight, RealWeight, 9001494, TypeId, 9001495, 990197, 82, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31397, '4621', '18385', '18426,18427,18428');
DELETE FROM items_templates WHERE Id = 31398;
INSERT INTO items_templates SELECT 31398, 'Box de El Piko', `Type`, AppearanceId, Weight, RealWeight, 9001496, TypeId, 9001497, 990198, 132, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31398, '4609', '18364', '18434,18435,18436');
DELETE FROM items_templates WHERE Id = 31399;
INSERT INTO items_templates SELECT 31399, 'Box de Père Ver', `Type`, AppearanceId, Weight, RealWeight, 9001498, TypeId, 9001499, 990199, 173, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31399, '4726', '18568', '18587,18588,18589');
DELETE FROM items_templates WHERE Id = 31400;
INSERT INTO items_templates SELECT 31400, 'Box de Tal Kasha', `Type`, AppearanceId, Weight, RealWeight, 9001500, TypeId, 9001501, 990200, 200, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31400, '4744', '18542', '18590,18591,18592');
DELETE FROM items_templates WHERE Id = 31401;
INSERT INTO items_templates SELECT 31401, 'Box de Comte Razof', `Type`, AppearanceId, Weight, RealWeight, 9001502, TypeId, 9001503, 990201, 197, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31401, '4803', '18734', '18774,18775,18776');
DELETE FROM items_templates WHERE Id = 31402;
INSERT INTO items_templates SELECT 31402, 'Box de Choudini', `Type`, AppearanceId, Weight, RealWeight, 9001504, TypeId, 9001505, 990202, 92, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31402, '4860', '1696', '19084,19085,19086');
DELETE FROM items_templates WHERE Id = 31403;
INSERT INTO items_templates SELECT 31403, 'Box de Anerice la Shushess', `Type`, AppearanceId, Weight, RealWeight, 9001506, TypeId, 9001507, 990203, 200, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31403, '4882', '19068', '19078,19079,19080');
DELETE FROM items_templates WHERE Id = 31404;
INSERT INTO items_templates SELECT 31404, 'Box de Ilyzaelle', `Type`, AppearanceId, Weight, RealWeight, 9001508, TypeId, 9001509, 990204, 200, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31404, '4967', '19237', '19261,19262,19263');
DELETE FROM items_templates WHERE Id = 31405;
INSERT INTO items_templates SELECT 31405, 'Box de Bethel Akarna', `Type`, AppearanceId, Weight, RealWeight, 9001510, TypeId, 9001511, 990205, 200, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31405, '5110', '19411', '19608,19609');
DELETE FROM items_templates WHERE Id = 31406;
INSERT INTO items_templates SELECT 31406, 'Box de Solar', `Type`, AppearanceId, Weight, RealWeight, 9001512, TypeId, 9001513, 990206, 200, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31406, '5100', '19404', '19590,19591,19592,19598');
DELETE FROM items_templates WHERE Id = 31407;
INSERT INTO items_templates SELECT 31407, 'Box de Dazak Martegel', `Type`, AppearanceId, Weight, RealWeight, 9001514, TypeId, 9001515, 990207, 200, Cursed, UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = 15266;
INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES (31407, '5319', '19969', '20463,20464,20465');
DELETE FROM langs WHERE Id BETWEEN 9001300 AND 9001699;
INSERT INTO langs (Id, French) VALUES (9001300, 'Box de Bouftou Royal'),
 (9001301, 'Butin du donjon Cour du Bouftou Royal.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie du Bouftou Royal
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Laine de Bouftou Royal.'),
 (9001302, 'Box de Maître Corbac'),
 (9001303, 'Butin du donjon Bibliothèque du Maître Corbac.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie du Corbac
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Cœur du Maître Corbac, Clakoss.'),
 (9001304, 'Box de Minotoror'),
 (9001305, 'Butin du donjon Centre du Labyrinthe du Minotoror.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie du Minotoror
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Corne Brûlée du Minotoror, Sabot du Minotoror, Queue de Minotoror, Cuir du Minotoror, Fémur du Minotoror.'),
 (9001306, 'Box de Dragon Cochon'),
 (9001307, 'Butin du donjon Antre du Dragon Cochon.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie du Dragon Cochon
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Sang de Dragon Cochon, Crâne du Dragon Cochon, Cuir du Dragon Cochon, Dragolait, Défense de Dragon Cochon, Queue de Dragon Cochon, Poil de Dragon Cochon.'),
 (9001308, 'Box de Gorgouille'),
 (9001309, 'Butin du donjon Antre du Dragon Cochon.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie du Gorgouille
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Poignée du Gorgouille.'),
 (9001310, 'Box de Crocabulia'),
 (9001311, 'Butin du donjon Donjon des Dragoeufs.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie du Dragoeuf Cuirassé
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Corne brisée de Crocabulia, Dent de Crocabulia.'),
 (9001312, 'Box de Tournesol Affamé'),
 (9001313, 'Butin du donjon Grange du Tournesol Affamé.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie du Paysan
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Pétale Magique du Tournesol Affamé.'),
 (9001314, 'Box de Abraknyde Ancestral'),
 (9001315, 'Butin du donjon Domaine Ancestral.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie Ancestrale
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Bourgeon d''Abraknyde Ancestral.'),
 (9001316, 'Box de Chêne Mou'),
 (9001317, 'Butin du donjon Clairière du Chêne Mou.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie du Chêne Mou
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Bourgeon du Chêne Mou, Ambre du Chêne Mou.'),
 (9001318, 'Box de Blop Coco Royal'),
 (9001319, 'Butin du donjon Clos des Blops.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie Blop Coco Royale
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Fleur de Blop Coco Royal.'),
 (9001320, 'Box de Blop Griotte Royal'),
 (9001321, 'Butin du donjon Clos des Blops.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie Blop Griotte Royale
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Fleur de Blop Griotte Royal.'),
 (9001322, 'Box de Blop Indigo Royal'),
 (9001323, 'Butin du donjon Clos des Blops.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie Blop Indigo Royale
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Fleur de Blop Indigo Royal.'),
 (9001324, 'Box de Blop Reinette Royal'),
 (9001325, 'Butin du donjon Clos des Blops.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie Blop Reinette Royale
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Fleur de Blop Reinette Royal.'),
 (9001326, 'Box de Bulbig Brozeur'),
 (9001327, 'Butin du donjon Caverne des Bulbes.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 5 ressources rares
Ressources rares : Écorce magique de Bulbig Brozeur.'),
 (9001328, 'Box de Bworkette'),
 (9001329, 'Butin du donjon Donjon des Bworks.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 5 ressources rares
Ressources rares : Poil de Bworkette.'),
 (9001330, 'Box de Bworker'),
 (9001331, 'Butin du donjon Grotte du Bworker.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie du Bworker Gladiateur
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Peau du Bworker.'),
 (9001332, 'Box de Meulou'),
 (9001333, 'Butin du donjon Tanière du Meulou.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie du Meulou
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Œil de Meulou, Dents de Meulou, Testicules de Meulou, Testicules Magiques du Meulou, Mojo du Meulou.'),
 (9001334, 'Box de Wa Wobot'),
 (9001335, 'Butin du donjon Terrier du Wa Wabbit.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie du Wa Wobot
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Barbe du Wa Wobot.'),
 (9001336, 'Box de Craqueleur Légendaire'),
 (9001337, 'Butin du donjon Pitons Rocheux des Craqueleurs.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie du Craqueleur Légendaire
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Méga Pierre du Craqueleur Légendaire.'),
 (9001338, 'Box de Mob l''Éponge'),
 (9001339, 'Butin du donjon Château Ensablé.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie en Mousse
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Peau de Mob l''Éponge.'),
 (9001340, 'Box de Péki Péki'),
 (9001341, 'Butin du donjon Donjon des Firefoux.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 5 ressources rares
Ressources rares : Étoffe de Péki, Queue Magique du Péki.'),
 (9001342, 'Box de Coffre des Forgerons'),
 (9001343, 'Butin du donjon Donjon des Forgerons.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie Coffre
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Serrure du Coffre des Forgerons.'),
 (9001344, 'Box de Ougah'),
 (9001345, 'Butin du donjon Temple du Grand Ougah.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie Ougah
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Fragment d''Ougalurette.'),
 (9001346, 'Box de Gelée Royale Bleue'),
 (9001347, 'Butin du donjon Gelaxième Dimension.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie Gelax
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Gelée Bleutée Royale.'),
 (9001348, 'Box de Gelée Royale Menthe'),
 (9001349, 'Butin du donjon Gelaxième Dimension.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie Gelax
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Gelée Menthe Royale.'),
 (9001350, 'Box de Gelée Royale Fraise'),
 (9001351, 'Butin du donjon Gelaxième Dimension.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie Gelax
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Gelée Fraise Royale.'),
 (9001352, 'Box de Gelée Royale Citron'),
 (9001353, 'Butin du donjon Gelaxième Dimension.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie Gelax
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Gelée Citron Royale.'),
 (9001354, 'Box de Corailleur Magistral'),
 (9001355, 'Butin du donjon Grotte Hesque.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 5 ressources rares
Ressources rares : Corail Magistral, Étoffe Corailleuse.'),
 (9001356, 'Box de Kanniboul Ebil'),
 (9001357, 'Butin du donjon Village Kanniboul.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie du Kanniboul Ebil
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Cagoule du Kanniboul Ebil.'),
 (9001358, 'Box de Kimbo'),
 (9001359, 'Butin du donjon Canopée du Kimbo.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie du Kimbo
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Chaussette du Kimbo, Pierre précieuse du Kimbo.'),
 (9001360, 'Box de Tanukouï San'),
 (9001361, 'Butin du donjon Donjon des Kitsounes.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 5 ressources rares
Ressources rares : Testicules du Tanukouï San.'),
 (9001362, 'Box de Koulosse'),
 (9001363, 'Butin du donjon Caverne du Koulosse.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie du Koulosse
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Pierre du Koulosse.'),
 (9001364, 'Box de Kralamoure Géant'),
 (9001365, 'Butin du donjon Antre du Kralamoure Géant.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie Ventouse
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Encre du Kralamoure géant.'),
 (9001366, 'Box de Kwakwa'),
 (9001367, 'Butin du donjon Nid du Kwakwa.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie du Kwakwa
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Bec du Kwakwa.'),
 (9001368, 'Box de Shin Larve'),
 (9001369, 'Butin du donjon Donjon des Larves.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoppinz
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Dent de Shin Larve.'),
 (9001370, 'Box de Boostache'),
 (9001371, 'Butin du donjon Maison Fantôme.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie du Boostache
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Moustache de Boostache.'),
 (9001372, 'Box de Nelween'),
 (9001373, 'Butin du donjon Laboratoire de Brumen Tinctorias.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie de Nelween
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Fiole de fumée de Nelween.'),
 (9001374, 'Box de Père Fwetar'),
 (9001375, 'Butin du donjon Maison du Papa Nowel.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie du Père Fwetar
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : aucune.'),
 (9001376, 'Box de Gourlo le Terrible'),
 (9001377, 'Butin du donjon Cale de l''Arche d''Otomaï.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 5 ressources rares
Ressources rares : Poudre explosive de Gourlo, Étoffe de Gourlo le Terrible.'),
 (9001378, 'Box de Maître Pandore'),
 (9001379, 'Butin du donjon Repaire des Pandikazes.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 5 ressources rares
Ressources rares : Étoffe de Pandore, Poil de Maître Pandore, Étoffe de Maître Pandore, Peau de Maître Pandore.'),
 (9001380, 'Box de Silf le Rasboul Majeur'),
 (9001381, 'Butin du donjon Goulet du Rasboul.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 5 ressources rares
Ressources rares : Queue emplumée du Rasboul Majeur, Peau du Rasboul Majeur.'),
 (9001382, 'Box de Rat Blanc'),
 (9001383, 'Butin du donjon Donjon des Rats de Bonta.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie du Rat Blanc
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Dent de Rat Blanc.'),
 (9001384, 'Box de Rat Noir'),
 (9001385, 'Butin du donjon Donjon des Rats de Brâkmar.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie du Rat Noir
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Étoffe de Rat Noir.'),
 (9001386, 'Box de Sphincter Cell'),
 (9001387, 'Butin du donjon Donjon des Rats du Château d''Amakna.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie Cérémoniale
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Étoffe de Sphincter Cell.'),
 (9001388, 'Box de Scarabosse Doré'),
 (9001389, 'Butin du donjon Donjon des Scarafeuilles.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie Scarabosse Doré
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Viscères du Scarabosse Doré.'),
 (9001390, 'Box de Skeunk'),
 (9001391, 'Butin du donjon Repaire de Skeunk.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 5 ressources rares
Ressources rares : Peau de Skeunk.'),
 (9001392, 'Box de Chafer Ronin'),
 (9001393, 'Butin du donjon Donjon des Squelettes.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie des Anciens Chafers
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Fémur du Chafer Ronin.'),
 (9001394, 'Box de Batofu'),
 (9001395, 'Butin du donjon Donjon des Tofus.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 5 ressources rares
Ressources rares : Plume de Batofu.'),
 (9001396, 'Box de Blop Multicolore Royal'),
 (9001397, 'Butin du donjon Antre du Blop Multicolore Royal.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie Blop Multicolore Royale
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Fleur de Blop Multicolore Royal.'),
 (9001398, 'Box de Tofu Royal'),
 (9001399, 'Butin du donjon Tofulailler Royal.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 5 ressources rares
Ressources rares : Couronne Brisée du Tofu Royal, Plume Vibrante du Tofu Royal.'),
 (9001400, 'Box de Tynril Perfide'),
 (9001401, 'Butin du donjon Laboratoire du Tynril.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 5 ressources rares
Ressources rares : Écorce du Tynril, Ambre du Tynril, Peau lacérée du Tynril.'),
 (9001402, 'Box de Wa Wabbit'),
 (9001403, 'Butin du donjon Château du Wa Wabbit.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie du Wabbit
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Étoffe du Wa Wabbit.'),
 (9001404, 'Box de Minotot'),
 (9001405, 'Butin du donjon Salle du Minotot.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie du Minotot
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Laine du Minotot, Cuir du Minotot, Sabot du Minotot, Corne du Minotot, Fémur du Minotot.'),
 (9001406, 'Box de Royalmouth'),
 (9001407, 'Butin du donjon Serre du Royalmouth.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie du Royalmouth
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Queue du Royalmouth.'),
 (9001408, 'Box de Mansot Royal'),
 (9001409, 'Butin du donjon Excavation du Mansot Royal.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie du Mansot Royal
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Œuf du Mansot Royal.'),
 (9001410, 'Box de Ben le Ripate'),
 (9001411, 'Butin du donjon Épave du Grolandais violent.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie de Ben le Ripate
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Scapula de Ben le Ripate.'),
 (9001412, 'Box de Obsidiantre'),
 (9001413, 'Butin du donjon Hypogée de l''Obsidiantre.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie de l''Obsidiantre
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Vertèbre d''Obsidiantre.'),
 (9001414, 'Box de Tengu Givrefoux'),
 (9001415, 'Butin du donjon Tanière Givrefoux.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie du Tengu Givrefoux
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Cuir de Tengu Givrefoux.'),
 (9001416, 'Box de Fuji Givrefoux Nourricière'),
 (9001417, 'Butin du donjon Tanière Givrefoux.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie de la Fuji Givrefoux
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Condyle de Fuji Givrefoux.'),
 (9001418, 'Box de Korriandre'),
 (9001419, 'Butin du donjon Antre du Korriandre.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie du Korriandre
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Œil de Korriandre.'),
 (9001420, 'Box de Kolosso'),
 (9001421, 'Butin du donjon Cavernes du Kolosso.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie de Kolosso
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Étoffe de Kolosso.'),
 (9001422, 'Box de Professeur Xa'),
 (9001423, 'Butin du donjon Cavernes du Kolosso.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie du Professeur Xa
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : aucune.'),
 (9001424, 'Box de Glourséleste'),
 (9001425, 'Butin du donjon Antichambre des Gloursons.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie Glourséleste
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Hamatum de Glourséleste.'),
 (9001426, 'Box de Grolloum'),
 (9001427, 'Butin du donjon Donjon de la mine de Sakaï.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie Grésil
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Étoffe givrée, Eclat temporel.'),
 (9001428, 'Box de Daïgoro'),
 (9001429, 'Butin du donjon Repaire de Daïgoro.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie de Daïgoro
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Couche de Daïgoro.'),
 (9001430, 'Box de Halouine'),
 (9001431, 'Butin du donjon Potager d''Halouine.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplitrouille
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Chicots d''Halouine.'),
 (9001432, 'Box de Sylargh'),
 (9001433, 'Butin du donjon Transporteur de Sylargh.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie de Sylargh
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Incisive de Sylargh.'),
 (9001434, 'Box de Klime'),
 (9001435, 'Butin du donjon Salons privés de Klime.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie de Klime
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Scalp de Klime.'),
 (9001436, 'Box de Missiz Frizz'),
 (9001437, 'Butin du donjon Forgefroide de Missiz Frizz.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie de Missiz Frizz
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Culotte de Missiz Frizz.'),
 (9001438, 'Box de Nileza'),
 (9001439, 'Butin du donjon Laboratoire de Nileza.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie de Nileza
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Braguette de Nileza.'),
 (9001440, 'Box de Comte Harebourg'),
 (9001441, 'Butin du donjon Donjon du Comte Harebourg.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie du Comte Harebourg
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Bandelette du Comte Harebourg.'),
 (9001442, 'Box de Maître des Pantins'),
 (9001443, 'Butin du donjon Théâtre de Dramak.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie de Flèche Mauve
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Slip troué de Dramak.'),
 (9001444, 'Box de Merkator'),
 (9001445, 'Butin du donjon Aquadôme de Merkator.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie des Abysses
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Cœur de Merkator.'),
 (9001446, 'Box de Ombre'),
 (9001447, 'Butin du donjon Pyramide d''Ombre.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie d''Ombre
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Corne d''Ombre.'),
 (9001448, 'Box de Kanigroula'),
 (9001449, 'Butin du donjon Grotte de Kanigroula.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 5 ressources rares
Ressources rares : Queue de Kanigroula.'),
 (9001450, 'Box de Roi Nidas'),
 (9001451, 'Butin du donjon Palais du roi Nidas.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie de Nidas
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Joyau de la couronne du roi Nidas.'),
 (9001452, 'Box de Malléfisk'),
 (9001453, 'Butin du donjon Fabrique de Malléfisk.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie Précieuse
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Broderie de Malléfisk.'),
 (9001454, 'Box de Phossile'),
 (9001455, 'Butin du donjon Galerie du Phossile.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie Phossile
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Défense du Phossile.'),
 (9001456, 'Box de Haute Truche'),
 (9001457, 'Butin du donjon Volière de la Haute Truche.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Gruchoplie
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Œuf de Haute Truche.'),
 (9001458, 'Box de Capitaine Ekarlatte'),
 (9001459, 'Butin du donjon Ring du Capitaine Ekarlatte.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie du Capitaine Ekarlatte
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Cœur du Capitaine Ekarlatte.'),
 (9001460, 'Box de Toxoliath'),
 (9001461, 'Butin du donjon Cave du Toxoliath.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 5 ressources rares
Ressources rares : Sang du Toxoliath.'),
 (9001462, 'Box de Reine des Voleurs'),
 (9001463, 'Butin du donjon Trône de la Cour Sombre.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie de la Reine des Voleurs
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Broderie de la Reine des Voleurs.'),
 (9001464, 'Box de Protozorreur'),
 (9001465, 'Butin du donjon Ventre de la Baleine.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie de la Baleine
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Noyau de Protozorreur.'),
 (9001466, 'Box de Fraktale'),
 (9001467, 'Butin du donjon Mégalithe de Fraktale.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie de Fraktale
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Pixel de Fraktale.'),
 (9001468, 'Box de XLII'),
 (9001469, 'Butin du donjon Horologium de XLII.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie de XLII
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Corne de XLII.'),
 (9001470, 'Box de Vortex'),
 (9001471, 'Butin du donjon Œil de Vortex.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie de Vortex
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Aile de Vortex.'),
 (9001472, 'Box de Kankreblath'),
 (9001473, 'Butin du donjon Cache de Kankreblath.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Microplie
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Patte de Kankreblath.'),
 (9001474, 'Box de Reine Nyée'),
 (9001475, 'Butin du donjon Antre de la Reine Nyée.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie de la Pondeuse
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Globule de Nyée.'),
 (9001476, 'Box de Kardorim'),
 (9001477, 'Butin du donjon Crypte de Kardorim.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie de Kardorim
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Crâne de Kardorim.'),
 (9001478, 'Box de Le Chouque'),
 (9001479, 'Butin du donjon Bateau du Chouque.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie du Capitaine Pirate
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Bwak Squelettique.'),
 (9001480, 'Box de Moon'),
 (9001481, 'Butin du donjon Arbre de Moon.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie de Moon
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Étoffe Magique de Moon.'),
 (9001482, 'Box de Pounicheur'),
 (9001483, 'Butin du donjon Miausolée du Pounicheur.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie du Pounicheur
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Glande du Pounicheur.'),
 (9001484, 'Box de Ush Galesh'),
 (9001485, 'Butin du donjon Plateau de Ush.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie des Matougarous
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Moustache de Ush.'),
 (9001486, 'Box de Chalœil'),
 (9001487, 'Butin du donjon Défi du Chalœil.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie du Chalœil
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Dé du Chalœil.'),
 (9001488, 'Box de Capitaine Meno'),
 (9001489, 'Butin du donjon Vaisseau du Capitaine Meno.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie des Profondeurs
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Poil de Meno.'),
 (9001490, 'Box de Larve de Koutoulou'),
 (9001491, 'Butin du donjon Temple de Koutoulou.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie de la Fosse
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Aile de Larve de Koutoulou.'),
 (9001492, 'Box de Dantinéa'),
 (9001493, 'Butin du donjon Palais de Dantinéa.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie Trithon
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Dentier de Dantinéa.'),
 (9001494, 'Box de Mantiscore'),
 (9001495, 'Butin de donjon.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie du Désert
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Cœur du Mantiscore.'),
 (9001496, 'Box de El Piko'),
 (9001497, 'Butin du donjon Caverne d''El Piko.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie d''El Piko
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Étoile d''Or d''El Piko.'),
 (9001498, 'Box de Père Ver'),
 (9001499, 'Butin du donjon Boyau du Père Ver.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Pernoplie
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Œil du Père Ver.'),
 (9001500, 'Box de Tal Kasha'),
 (9001501, 'Butin du donjon Chambre de Tal Kasha.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie de Tal Kasha
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Cheveux de Tal Kasha.'),
 (9001502, 'Box de Comte Razof'),
 (9001503, 'Butin du donjon Camp du Comte Razof.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie du Comte Razof
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Poils du Comte Razof.'),
 (9001504, 'Box de Choudini'),
 (9001505, 'Butin du donjon Chapiteau des Magik Riktus.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie de Choudini
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Étoffe de Choudini.'),
 (9001506, 'Box de Anerice la Shushess'),
 (9001507, 'Butin du donjon Manoir des Katrepat.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie de la Goule
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Pic d''Anerice.'),
 (9001508, 'Box de Ilyzaelle'),
 (9001509, 'Butin du donjon Belvédère d''Ilyzaelle.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie d''Ilyzaelle
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Plume d''Ilyzaelle.'),
 (9001510, 'Box de Bethel Akarna'),
 (9001511, 'Butin du donjon Tour de Bethel.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie Noyée
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Queue de Bethel Akarna.'),
 (9001512, 'Box de Solar'),
 (9001513, 'Butin du donjon Tour de Solar.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie Volcanique
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Cervelle de Solar.'),
 (9001514, 'Box de Dazak Martegel'),
 (9001515, 'Butin du donjon Brasserie du roi Dazak.
À ouvrir (la box disparaît) :
• 50 % : 1 ressource rare
• 25 % : 2 ressources rares
• 20 % : 3 ressources rares
• 5 % : 1 objet de la Panoplie Paztek
• JACKPOT 0,0001 % : la panoplie entière !
Ressources rares : Epaulette de Dazak Martegel.');
COMMIT;
SELECT COUNT(*) AS box FROM padgref_box_boss;
