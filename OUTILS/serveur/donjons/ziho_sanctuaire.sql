-- NEKFEU 2026-10-16 : Sanctuaire des Dragoeufs inaccessible. Ziho (PNJ 789, village 86248450) ouvre le message 3236
-- qui n'avait AUCUNE reponse -> impossible d'atteindre « Rentrer dans le Sanctuaire des Dragoeufs » (reponse 2876, message 3237).
DELETE FROM npcs_replies WHERE MessageId=3236 AND ReplyId IN (2876, 2877);
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Criteria, Parameter0, Parameter1, Parameter2) VALUES
 ('Teleport', 2876, 3236, NULL, '33686528', '127', '1'),      -- Rentrer dans le Sanctuaire (sans clef, demande utilisateur)
 ('Dialog', 2877, 3236, NULL, '3237', NULL, NULL);            -- Se renseigner a propos du gros trou -> message 3237
UPDATE npcs_replies SET Criteria=NULL WHERE Id=409 AND ReplyId=2876 AND MessageId=3237; -- sans clef aussi depuis le message 3237
