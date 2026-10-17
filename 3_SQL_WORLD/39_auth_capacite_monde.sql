-- MYSTIC 17/10/2026 : fluidité / capacité (document de l'autre dev, section 1). Base **auth** (pas world) !
-- Quand CharsCount >= CharCapacity, le serveur passe « complet » et refuse les connexions. Rejouable.
UPDATE worlds SET CharCapacity = 5000 WHERE Id = 1;
-- Contrôle : attendu 5000
SELECT Id, Name, CharCapacity FROM worlds;
