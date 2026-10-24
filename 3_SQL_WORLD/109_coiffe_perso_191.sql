-- PADGREF 23/10 (demande utilisateur) : la coiffe du personnage 191 bugue et bloque le compte -> on la DESEQUIPE
-- (elle retourne dans l'inventaire, case 63 ; l'objet est garde). Le personnage doit etre DECONNECTE.
-- Copie de la ligne d'origine dans save_coiffe_perso_191 (pour l'annulation).
CREATE TABLE IF NOT EXISTS save_coiffe_perso_191 AS SELECT * FROM characters_items WHERE OwnerId = 191 AND Position = 6;
SELECT 'coiffe equipee avant' AS etape, Id, ItemId, Position FROM characters_items WHERE OwnerId = 191 AND Position = 6;
UPDATE characters_items SET Position = 63 WHERE OwnerId = 191 AND Position = 6;
SELECT 'apres' AS etape, (SELECT COUNT(*) FROM characters_items WHERE OwnerId = 191 AND Position = 6) AS coiffes_encore_equipees,
       (SELECT COUNT(*) FROM save_coiffe_perso_191) AS lignes_sauvegardees, (SELECT Name FROM characters WHERE Id = 191) AS personnage;
