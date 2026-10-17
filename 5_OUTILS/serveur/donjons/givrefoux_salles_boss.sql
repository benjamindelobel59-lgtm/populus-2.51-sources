-- MYSTIC 17/10 : Tanière Givrefoux, salles de boss (test utilisateur + noms de cartes du client).
-- « Sixième salle bis » (59516162) = Tengu seul, « Sixième salle » (59515904) = Tengu + Fuji (tofus.fr).
-- Les téléportations d'origine étaient justes, les groupes étaient inversés : on remet les téléportations
-- et on échange les cartes des groupes 300 (Tengu) et 301 (Tengu + Fuji). Rejouable.
UPDATE npcs_replies SET Parameter0='59516162', Parameter1='451' WHERE MessageId=13776 AND Type='Teleport' AND ReplyId=15928; -- Rendre hommage à Tengu
UPDATE npcs_replies SET Parameter0='59515904', Parameter1='424' WHERE MessageId=13777 AND Type='Teleport' AND ReplyId=15930; -- Déranger Tengu et Fuji
UPDATE monsters_spawns_dungeons SET MapId=59516162, Descripcion='Givrefoux Tengu (Sixieme salle bis)' WHERE Id=300;
UPDATE monsters_spawns_dungeons SET MapId=59515904, Descripcion='Givrefoux Tengu + Fuji (Sixieme salle)' WHERE Id=301;
-- PNJ du choix (Givrihaltès, 59515138, message 8988) : plus de « Sortir des cavernes » (demande utilisateur)
DELETE FROM npcs_replies WHERE MessageId=8988 AND Type='DungeonExit' AND ReplyId=8325;
