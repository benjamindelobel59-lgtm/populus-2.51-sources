-- MYSTIC 17/10/2026 : gardiens de donjon (signalé : le PNJ du Royalmouth lançait un combat de Dopeul et déconnectait).
-- Rejouable.
-- 1) Royalmouth (map 54176049) : le gardien est Truffo (PNJ 1285). Sa réponse « Donner la clef. » (7702) avait été importée
--    en type Dopeul (combat contre un monstre inexistant -> NullReferenceException, déconnexion). -> entrée de donjon,
--    + « Utiliser le trousseau de clefs. » (7703, déjà dans le dialogue du client).
UPDATE npcs_replies SET Type = 'DungeonEnter', Parameter0 = NULL, Parameter1 = NULL, Parameter2 = NULL
 WHERE MessageId = 7685 AND ReplyId = 7702 AND Type = 'Dopeul';
INSERT INTO npcs_replies (Type, ReplyId, MessageId)
 SELECT 'DungeonEnterTrousseau', 7703, 7685 FROM DUAL
 WHERE NOT EXISTS (SELECT 1 FROM npcs_replies WHERE MessageId = 7685 AND ReplyId = 7703 AND Type = 'DungeonEnterTrousseau');

-- 2) Donjons dont le gardien a déjà « clef » + « trousseau » mais sans entrée déclarée (dungeons_entries) : le trousseau
--    restait invisible et « Téléporter les membres du groupe » ne faisait rien. Clef et 1re salle = celles du client / de la
--    réponse Teleport officielle du gardien.
INSERT INTO dungeons_entries (DungeonId, EntranceMapId, KeyItemId, FirstRoomMapId, FirstRoomCellId, KeeperCellId)
SELECT v.d, v.e, v.k, v.f, -1, v.c FROM (
          SELECT  50 d,  54176049 e, 11174 k,  55050240 f, 468 c  -- Serre du Royalmouth
    UNION SELECT  52,  54163796, 11176,  56360960, 271            -- Épave du Grolandais violent (Ben le Ripate)
    UNION SELECT  53,  56890883, 11177,  57149697, 327            -- Hypogée de l'Obsidiantre
    UNION SELECT  60,  79167488, 12735,  79430145, 315            -- Repaire de Daïgoro
    UNION SELECT  71, 125829633, 15093, 125830657, 344            -- Grotte de Kanigroula
    UNION SELECT  85, 153881600,  8545, 152829952, 357            -- Crypte de Kardorim
    UNION SELECT  95, 173280513, 18421, 174326272, 398            -- Repaire de Masto
    UNION SELECT  96, 173540352, 18422, 174064128, 191            -- Caverne d'El Piko
    UNION SELECT 110, 194779138, 19963, 195035136, 247            -- Brasserie du roi Dazak
) v WHERE NOT EXISTS (SELECT 1 FROM dungeons_entries x WHERE x.DungeonId = v.d AND x.EntranceMapId = v.e);

-- 3) « Utiliser le trousseau de clefs » chez 12 gardiens qui ne l'ont pas dans les données 2.51 (Firefoux, Nidas,
--    Malléfisk, Phossile, Ekarlatte, Toxoliath, Cour Sombre, Fraktale, XLII, Pounicheur, Ush, Chalœil), comme pour le Vortex :
--    réponse 8450 ajoutée à ces PNJ dans le Npcs.d2o du client (OBLIGATOIRE : client à jour, sinon la réponse est vide).
INSERT INTO npcs_replies (Type, ReplyId, MessageId)
SELECT 'DungeonEnterTrousseau', 8450, m.Id FROM (
          SELECT 2943 Id UNION SELECT 17885 UNION SELECT 17887 UNION SELECT 17637 UNION SELECT 17710 UNION SELECT 18286
    UNION SELECT 18292 UNION SELECT 18289 UNION SELECT 19557 UNION SELECT 19397 UNION SELECT 22674 UNION SELECT 22678
    UNION SELECT 22696) m
WHERE NOT EXISTS (SELECT 1 FROM npcs_replies r WHERE r.MessageId = m.Id AND r.Type = 'DungeonEnterTrousseau');

-- 4) Contrôle : attendu royalmouth_dopeul = 0, royalmouth_trousseau = 1, entrees_ajoutees = 9, gardiens_trousseau_ajoutes = 13
SELECT (SELECT COUNT(*) FROM npcs_replies WHERE MessageId = 7685 AND Type = 'Dopeul') AS royalmouth_dopeul,
       (SELECT COUNT(*) FROM npcs_replies WHERE MessageId = 7685 AND Type = 'DungeonEnterTrousseau') AS royalmouth_trousseau,
       (SELECT COUNT(*) FROM dungeons_entries WHERE DungeonId IN (50,52,53,60,71,85,95,96,110)) AS entrees_ajoutees,
       (SELECT COUNT(*) FROM npcs_replies WHERE ReplyId = 8450 AND Type = 'DungeonEnterTrousseau' AND MessageId IN (2943,17885,17887,17637,17710,18286,18292,18289,19557,19397,22674,22678,22696)) AS gardiens_trousseau_ajoutes;
