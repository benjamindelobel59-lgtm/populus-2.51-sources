-- PADGREF 23/10/2026 : SQL 101 — Eliocalypse : effets du client 2.74 absents de effects_templates (2868 « Augmente la taille », 2872
-- « Seuil : #1 PV »), copiés de l'effet 1223. Gérés par game/effects/handlers/Spells/Others/EffetsEliocalypse.cs. Base world. Rejouable.
START TRANSACTION;
DELETE FROM effects_templates WHERE Id IN (2868, 2872);
INSERT INTO effects_templates SELECT 2868, DescriptionId, IconId, Characteristic, Category, Operator, 0, UseDice, ForceMinMax, Boost, Active, ShowInSet, BonusType, UseInFight, OppositeId, EffectPriority FROM effects_templates WHERE Id = 1223;
INSERT INTO effects_templates SELECT 2872, DescriptionId, IconId, Characteristic, Category, Operator, 0, UseDice, ForceMinMax, Boost, Active, ShowInSet, BonusType, UseInFight, OppositeId, EffectPriority FROM effects_templates WHERE Id = 1223;
COMMIT;
