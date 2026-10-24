-- PADGREF 22/10/2026 : Klime (Galuchat 3485) : la perte d'invulnérabilité (effet 952, poussée indirecte) s'affichait
-- « infinie » dans le chat et l'infobulle alors qu'elle dure 1 tour (VulnerableJusquAuTourDuDeclencheur, KlimeBrain).
-- Même correctif d'affichage que Royalmouth (2356) / Mansot Royal (2523) : durée du buff déclenché = 1.
-- Rejouable.
DELETE FROM spells_effects_fix WHERE Id = 900011;
INSERT INTO spells_effects_fix (Id, SpellId, SpellLevelId, EffectId, EffectIndex, TriggerBuffDuration, Dispellable, ActionId, ClientEffectId, Priority)
VALUES (900011, 3485, NULL, 952, NULL, 1, NULL, NULL, NULL, NULL);
