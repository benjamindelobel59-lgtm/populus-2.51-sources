-- MYSTIC 18/10 : PNJ du Koulosse remis sur sa case d'origine (81) : on ne lui parle que depuis la case 124 (contrôle serveur, Npc.cs).
UPDATE npcs_spawns SET CellId = 81 WHERE Id = 10348 AND MapId = 139784;
-- PNJ de la Bibliothèque du Maître Corbac (774) : condition « ex.mp » inconnue du serveur → déconnexion au clic. On garde le dialogue
-- d'entrée du donjon (message 3200 : clef, trousseau, reprise) sans condition et on retire le doublon (message 3181, simple fin).
UPDATE npcs_actions SET `Condition` = '' WHERE Id = 204429 AND NpcId = 774;
DELETE FROM npcs_actions WHERE Id = 204430 AND NpcId = 774;
