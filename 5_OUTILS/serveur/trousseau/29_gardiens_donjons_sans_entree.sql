-- MYSTIC 17/10/2026 : donjons sans entrée déclarée (dungeons_entries) -> « Donner la clef » + « Utiliser le trousseau » chez leur
-- gardien officiel (dialogues du client 2.51). Choix de l'utilisateur : Blops = une entrée par donjon ; Sanazero (Koutoulou)
-- placé sur une case libre ; Dragoeufs / Larves laissés (à vérifier en jeu). Rejouable.
-- Nécessite le WorldServer du 17/10 15h45+ (Parameter0 d'une réponse DungeonEnter = DungeonId, pour Bibiblop).

-- 1) Entrées des donjons (clef = objet officiel, 1re salle = celle de la réponse Teleport officielle / du donjon)
INSERT INTO dungeons_entries (DungeonId, EntranceMapId, KeyItemId, FirstRoomMapId, FirstRoomCellId, KeeperCellId)
SELECT v.d, v.e, v.k, v.f, -1, v.c FROM (
          SELECT   1 d, 120063489 e,  1568 k, 121373185 f, 275 c  -- Cour du Bouftou Royal (Rotabla)
    UNION SELECT   7, 147851777,  7557, 149684224, 358            -- Domaine Ancestral (Tronkny Baussope)
    UNION SELECT   8, 149422080,  8436, 149423104, 204            -- Clairière du Chêne Mou (Josette Taplane)
    UNION SELECT   9, 166985728,  9248, 166986752, 178            -- Clos des Blops (Bibiblop)
    UNION SELECT  45, 166985728,  9254, 166990850, 178            -- Antre du Blop Multicolore Royal (Bibiblop)
    UNION SELECT  39,  28443909,  8438,  40108544, 150            -- Donjon des Rats de Brâkmar (Portier)
    UNION SELECT  51,  55836672, 11175,  56098816, 369            -- Excavation du Mansot Royal (Waldos)
    UNION SELECT  93, 168560640, 18068, 169345024, 310            -- Temple de Koutoulou (Sanazero)
    UNION SELECT 106, 179831808, 19049, 182714368, 370            -- Manoir des Katrepat (Bozime)
    UNION SELECT 108, 186384896, 19515, 187432960, 281            -- Tour de Bethel (Tourier Submergé)
    UNION SELECT 109, 183108098, 19514, 187957506, 275            -- Tour de Solar (Tourier Calciné)
) v WHERE NOT EXISTS (SELECT 1 FROM dungeons_entries x WHERE x.DungeonId = v.d AND x.EntranceMapId = v.e);

-- 2) Bouftou Royal : « Donner la clef des bouftous » entre directement (avant : confirmation puis téléport sans clef) + trousseau
UPDATE npcs_replies SET Type = 'DungeonEnter', Parameter0 = NULL WHERE MessageId = 646 AND ReplyId = 10529;
INSERT INTO npcs_replies (Type, ReplyId, MessageId) SELECT 'DungeonEnterTrousseau', 10530, 646 FROM DUAL
 WHERE NOT EXISTS (SELECT 1 FROM npcs_replies WHERE MessageId = 646 AND Type = 'DungeonEnterTrousseau');

-- 3) Mansot : Waldos ne parlait pas sur la carte d'entrée
INSERT INTO npcs_actions (NpcId, Type, `Condition`, Priority, Parameter0)
 SELECT 1300, 'Talk', 'Mp=55836672', 0, '7834' FROM DUAL
 WHERE NOT EXISTS (SELECT 1 FROM npcs_actions WHERE NpcId = 1300 AND Type = 'Talk' AND `Condition` = 'Mp=55836672');

-- 4) Bibiblop (carte d'entrée) : dialogue 5402 avec les deux donjons (Parameter0 = DungeonId)
INSERT INTO npcs_actions (NpcId, Type, `Condition`, Priority, Parameter0)
 SELECT 1034, 'Talk', 'Mp=166985728', 0, '5402' FROM DUAL
 WHERE NOT EXISTS (SELECT 1 FROM npcs_actions WHERE NpcId = 1034 AND Type = 'Talk' AND `Condition` = 'Mp=166985728');
DELETE FROM npcs_replies WHERE MessageId = 5402 AND Type IN ('DungeonEnter', 'DungeonEnterTrousseau');
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES
 ('DungeonEnter', 4531, 5402, '9'), ('DungeonEnterTrousseau', 6623, 5402, '9'),
 ('DungeonEnter', 4544, 5402, '45'), ('DungeonEnterTrousseau', 6624, 5402, '45'),
 ('DungeonEnter', 11159, 5402, '9');

-- 5) Temple de Koutoulou : Sanazero placé à l'entrée (case 310, à déplacer au panel si besoin) ; sa réponse clef = entrée
INSERT INTO npcs_spawns (NpcId, MapId, CellId, Direction)
 SELECT 3259, 168560640, 310, 3 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM npcs_spawns WHERE NpcId = 3259 AND MapId = 168560640);

-- 6) Katrepat, Solar, Bethel : vrais gardiens à la place de Zamawa, avec leur dialogue officiel
DELETE FROM npcs_spawns WHERE NpcId = 707 AND MapId IN (179831808, 183108098, 186384896);
INSERT INTO npcs_spawns (NpcId, MapId, CellId, Direction)
 SELECT 4189, 186384896, 281, 3 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM npcs_spawns WHERE NpcId = 4189 AND MapId = 186384896);
INSERT INTO npcs_actions (NpcId, Type, `Condition`, Priority, Parameter0)
 SELECT v.n, 'Talk', NULL, 0, v.m FROM (SELECT 3962 n, '27635' m UNION SELECT 4188, '30308' UNION SELECT 4189, '30312') v
 WHERE NOT EXISTS (SELECT 1 FROM npcs_actions a WHERE a.NpcId = v.n AND a.Type = 'Talk');
DELETE FROM npcs_replies WHERE MessageId IN (27635, 30308, 30312) AND Type IN ('DungeonEnter', 'DungeonEnterTrousseau');
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES
 ('DungeonEnter', 33173, 27635), ('DungeonEnterTrousseau', 33176, 27635), ('DungeonEnter', 33177, 27635),
 ('DungeonEnter', 36503, 30308), ('DungeonEnterTrousseau', 36506, 30308), ('DungeonEnter', 36507, 30308),
 ('DungeonEnter', 36512, 30312), ('DungeonEnterTrousseau', 36515, 30312), ('DungeonEnter', 36516, 30312);

-- 7) Belvédère d'Ilyzaelle : déjà déclaré sous le donjon 102 -> pas de 2e entrée (107) sur la même carte
DELETE FROM dungeons_entries WHERE DungeonId = 107 AND EntranceMapId = 183501836;

-- 8) Meulou (Halda Moccione) et Ougah (Ernesto Rouste) : le vrai gardien, ajouté le 17/10, ne parlait pas sur la carte
--    d'entrée et n'avait pas « Donner la clef » ; c'était Zamawa qui faisait l'entrée -> Zamawa retiré.
INSERT INTO npcs_actions (NpcId, Type, `Condition`, Priority, Parameter0)
 SELECT v.n, 'Talk', v.c, 0, v.m FROM (SELECT 779 n, 'Mp=171706891' c, '3177' m UNION SELECT 1036, 'Mp=181146624', '5471') v
 WHERE NOT EXISTS (SELECT 1 FROM npcs_actions a WHERE a.NpcId = v.n AND a.Type = 'Talk' AND a.`Condition` = v.c);
INSERT INTO npcs_replies (Type, ReplyId, MessageId)
 SELECT 'DungeonEnter', v.r, v.m FROM (SELECT 2801 r, 3177 m UNION SELECT 4580, 5471) v
 WHERE NOT EXISTS (SELECT 1 FROM npcs_replies x WHERE x.MessageId = v.m AND x.ReplyId = v.r AND x.Type = 'DungeonEnter');
DELETE FROM npcs_spawns WHERE NpcId = 707 AND MapId IN (171706891, 181146624);
DELETE FROM npcs_spawns WHERE NpcId = 1036 AND MapId = 181146624 AND CellId = 371;  -- doublon d'Ernesto
UPDATE dungeons_entries SET KeeperCellId = 358 WHERE DungeonId = 13 AND EntranceMapId = 171706891;
UPDATE dungeons_entries SET KeeperCellId = 316 WHERE DungeonId = 19 AND EntranceMapId = 181146624;

-- 9) Contrôle : attendu entrees = 11, bibiblop = 5, gardiens_places = 2, zamawa_restants = 0, meulou_ougah = 2
SELECT (SELECT COUNT(*) FROM dungeons_entries WHERE DungeonId IN (1,7,8,9,45,39,51,93,106,107,108,109)) AS entrees,
       (SELECT COUNT(*) FROM npcs_replies WHERE (MessageId = 3177 AND ReplyId = 2801) OR (MessageId = 5471 AND ReplyId = 4580)) AS meulou_ougah,
       (SELECT COUNT(*) FROM npcs_replies WHERE MessageId = 5402 AND Type LIKE 'DungeonEnter%') AS bibiblop,
       (SELECT COUNT(*) FROM npcs_spawns WHERE (NpcId = 3259 AND MapId = 168560640) OR (NpcId = 4189 AND MapId = 186384896)) AS gardiens_places,
       (SELECT COUNT(*) FROM npcs_spawns WHERE NpcId = 707 AND MapId IN (179831808, 183108098, 186384896, 171706891, 181146624)) AS zamawa_restants;
