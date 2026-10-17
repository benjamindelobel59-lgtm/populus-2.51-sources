-- MYSTIC 17/10 : Oeil de Vortex, gardien (PNJ 2662, carte 144445702, message 19539) : « Utiliser le trousseau de clefs »
-- (choix de l'utilisateur : trousseau autorise en dimension). Texte ajoute au PNJ dans Npcs.d2o du client (reponse 8450). Rejouable.
DELETE FROM npcs_replies WHERE MessageId=19539 AND Type='DungeonEnterTrousseau';
INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnterTrousseau', 8450, 19539);
