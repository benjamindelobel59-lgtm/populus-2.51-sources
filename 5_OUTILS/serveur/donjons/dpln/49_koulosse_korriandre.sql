-- MYSTIC 18/10 : PNJ du donjon Koulosse posé sur une case non praticable (81) : on ne pouvait lui parler que collé à lui.
-- Déplacé sur la case praticable 109 juste devant (4 cases voisines praticables).
UPDATE npcs_spawns SET CellId = 109 WHERE Id = 10348 AND MapId = 139784;

-- Map de sortie du Korriandre (62920704) : interactif case 136 qui emmenait vers une salle vide (62920706) : supprimé
-- avec son retour (507639) (demande utilisateur).
DELETE FROM interactives_spawns_skills WHERE InteractiveSpawnId IN (507388, 507639);
DELETE FROM interactives_skills WHERE Id IN (3001905, 3001919);
DELETE FROM interactives_spawns WHERE Id IN (507388, 507639);
