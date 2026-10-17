-- MYSTIC 17/10/2026 : Boostache (2975) ni poussé, ni porté, ni échangé de place (demande utilisateur).
-- Les états Inébranlable (157) + Lourd (63) sont posés par le cerveau BoostacheBrain (WorldServer du 17/10 17h30+). Rejouable.
UPDATE monsters_templates SET CanBePushed = 0, CanSwitchPos = 0, canBeCarried = 0 WHERE Id = 2975;
-- Contrôle : attendu 0 0 0
SELECT CanBePushed, CanSwitchPos, canBeCarried FROM monsters_templates WHERE Id = 2975;
