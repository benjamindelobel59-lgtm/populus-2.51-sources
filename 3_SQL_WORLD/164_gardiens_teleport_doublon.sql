-- PADGREF 24/10 SQL 164 (retour utilisateur : « le PNJ de la map 139784 ne fonctionne pas, devant lui je ne peux pas lui parler ») :
-- 7 gardiens de donjon avaient, sur la MÊME réponse « Entrer », l'ancienne téléportation officielle (clé en inventaire ou quête
-- en cours) ET l'entrée de donjon Padgref : toutes les lignes d'une réponse doivent être valides -> sans la clé (trousseau…) rien
-- ne se passait, avec la clé les deux s'exécutaient. L'entrée Padgref (DungeonEnter, gère clé et trousseau) reste seule.
-- Gardien Koalak 139784 (Caverne du Koulosse), Lorkos x2 (Minotoror / Minotot), Tina Montini, Berthawa, Dorléans, Chambelle.
DELETE FROM npcs_replies WHERE Id IN (476,535,538,544,592,620,642) AND Type = 'Teleport';
