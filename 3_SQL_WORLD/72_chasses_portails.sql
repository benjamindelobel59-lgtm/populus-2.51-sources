-- MYSTIC 19/10/2026 : SQL 72 — chasses aux portails (dimensions divines).
-- Base world, World ARRÊTÉ. Rejouable. Nécessite le WorldServer « portails » (type de compétence ChassePortail).
-- Salle de localisation des portails (Tour des Voyageurs Dimensionnels, carte 130025985) : les 4 consoles ne téléportent plus
-- directement ; elles donnent la position du portail ouvert, ou lancent une chasse aux portails (pas de combat, pas d'XP, pas de
-- récompense). À la fin, le portail apparaît pour tout le serveur : 8 heures ou 110 passages au maximum (table mystic_portails,
-- créée par le serveur). Les entrées des dimensions sont dans game/Chasses/Portails.cs.
--   876 Enutrosor (53), 878 Srambad (54), 877 Xélorium (55), 1930 Ecaflipus (59)
SET NAMES utf8mb4;
USE `world`;
START TRANSACTION;
UPDATE interactives_skills SET Type = 'ChassePortail', Parameter0 = '53', Parameter1 = NULL, Parameter2 = NULL WHERE Id = 876;
UPDATE interactives_skills SET Type = 'ChassePortail', Parameter0 = '54', Parameter1 = NULL, Parameter2 = NULL WHERE Id = 878;
UPDATE interactives_skills SET Type = 'ChassePortail', Parameter0 = '55', Parameter1 = NULL, Parameter2 = NULL WHERE Id = 877;
UPDATE interactives_skills SET Type = 'ChassePortail', Parameter0 = '59', Parameter1 = NULL, Parameter2 = NULL WHERE Id = 1930;
COMMIT;
SELECT Id, Type, Parameter0 FROM interactives_skills WHERE Id IN (876, 877, 878, 1930);
