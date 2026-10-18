-- MYSTIC 18/10 : retours de la recette (durées affichées, buffs perdus à la mort, Marchand de Doplons).
-- Annulation : DELETE FROM spells_effects_fix WHERE Id >= 900000 ; UPDATE npcs_actions SET Parameter1 = NULL WHERE Id = 900525.
-- spells_effects_fix est lue au démarrage du World.
INSERT INTO spells_effects_fix (Id, SpellId, EffectId, TriggerBuffDuration, Dispellable) VALUES
 (900001, 2356, 952, 1, NULL),   -- Royalmouth : vulnérable 1 tour (le chat affichait « Infini »)
 (900002, 2356, 128, 1, NULL),   -- Royalmouth : +4 PM 1 tour
 (900003, 2523, 952, 1, NULL),   -- Kolosso : vulnérable 1 tour
 (900004, 2523, 754, 1, NULL),   -- Kolosso : -10 fuite 1 tour (était infini et cumulable)
 (900005, 4344, 950, 1, NULL),   -- Haute Truche : Truchage invulnérable 1 tour (demande utilisateur)
 (900006, 4342, 111, NULL, 4),   -- Haute Truche tuée : +2 PA infini aux alliés (était retiré aussitôt avec la mort de la Truche)
 (900007, 2404, 149, NULL, 4),   -- Grolloum : apparence de déverrouillage (retirée aussitôt avec la mort du zombi)
 (900008, 7369, 1077, NULL, 4),  -- Père Ver : -% résistances par monstre mort, cumulable (retiré aussitôt avec la mort du monstre)
 (900009, 7369, 168, NULL, 4),   -- Père Ver : -1 PA aux joueurs
 (900010, 7369, 169, NULL, 4);   -- Père Ver : -1 PM aux joueurs
-- Marchand de Doplons : revente autorisée
UPDATE npcs_actions SET Parameter1 = '1' WHERE Id = 900525;
