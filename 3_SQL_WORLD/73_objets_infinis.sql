-- MYSTIC 19/10/2026 : SQL 73 — objets infinis (généré par _OUTILS/client/client-build/objets_infinis/generer_objets.py).
-- Base world, World ARRÊTÉ. Rejouable. Effets côté serveur : game/Items/ObjetsInfinis.cs (ne se consomment jamais).
-- Client : Items.d2o + i18n_fr.d2i + items/bitmap1_1.d2p + items/vector1_1.d2p (dossier objets_infinis/client).
SET NAMES utf8mb4;
USE `world`;
START TRANSACTION;
-- 31200 : Pain Infini (modèle 11503, icône 990001)
DELETE FROM items_templates WHERE Id = 31200;
INSERT INTO items_templates SELECT 31200, 'Pain Infini', `Type`, AppearanceId, Weight, RealWeight, 9001200, TypeId, 9001201, 990001, 1, Cursed, UseAnimationId, 1, Targetable, 0, TwoHanded, Etheral, ItemSetId, '', HideEffects, RecipeIdsCSV, FavoriteSubAreasCSV, BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, IsLinkedToOwner FROM items_templates WHERE Id = 11503;
-- 31201 : Potion de Rappel infinie (modèle 548, icône 990002)
DELETE FROM items_templates WHERE Id = 31201;
INSERT INTO items_templates SELECT 31201, 'Potion de Rappel infinie', `Type`, AppearanceId, Weight, RealWeight, 9001202, TypeId, 9001203, 990002, 1, Cursed, UseAnimationId, 1, Targetable, 0, TwoHanded, Etheral, ItemSetId, 'null', HideEffects, RecipeIdsCSV, FavoriteSubAreasCSV, BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, IsLinkedToOwner FROM items_templates WHERE Id = 548;
-- 31202 : Potion de Foyer de Guilde infinie (modèle 8883, icône 990003)
DELETE FROM items_templates WHERE Id = 31202;
INSERT INTO items_templates SELECT 31202, 'Potion de Foyer de Guilde infinie', `Type`, AppearanceId, Weight, RealWeight, 9001204, TypeId, 9001205, 990003, 1, Cursed, UseAnimationId, 1, Targetable, 0, TwoHanded, Etheral, ItemSetId, 'PZ=1&Po!45', HideEffects, RecipeIdsCSV, FavoriteSubAreasCSV, BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, IsLinkedToOwner FROM items_templates WHERE Id = 8883;
-- 31203 : Potion d'enclos de Guilde infinie (modèle 9035, icône 990004)
DELETE FROM items_templates WHERE Id = 31203;
INSERT INTO items_templates SELECT 31203, 'Potion d''enclos de Guilde infinie', `Type`, AppearanceId, Weight, RealWeight, 9001206, TypeId, 9001207, 990004, 1, Cursed, UseAnimationId, 1, Targetable, 0, TwoHanded, Etheral, ItemSetId, 'PZ=1&Po!45', HideEffects, RecipeIdsCSV, FavoriteSubAreasCSV, BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, IsLinkedToOwner FROM items_templates WHERE Id = 9035;
-- 31204 : Potion de Chasse au Trésor infinie (modèle 548, icône 990005)
DELETE FROM items_templates WHERE Id = 31204;
INSERT INTO items_templates SELECT 31204, 'Potion de Chasse au Trésor infinie', `Type`, AppearanceId, Weight, RealWeight, 9001208, TypeId, 9001209, 990005, 1, Cursed, UseAnimationId, 1, Targetable, 0, TwoHanded, Etheral, ItemSetId, 'null', HideEffects, RecipeIdsCSV, FavoriteSubAreasCSV, BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, IsLinkedToOwner FROM items_templates WHERE Id = 548;
DELETE FROM langs WHERE Id BETWEEN 9001200 AND 9001209;
INSERT INTO langs (Id, French) VALUES (9001200, 'Pain Infini'),
 (9001201, 'Un pain qui ne s''épuise jamais : il rend 50 000 points de vie et reste dans l''inventaire.
Tant qu''il est dans votre inventaire, il est mangé automatiquement à la fin de chaque combat.'),
 (9001202, 'Potion de Rappel infinie'),
 (9001203, 'Vous téléporte à votre point de sauvegarde (dernier zaap enregistré).
Cette potion ne se vide jamais.'),
 (9001204, 'Potion de Foyer de Guilde infinie'),
 (9001205, 'Vous téléporte devant la maison de votre guilde.
Cette potion ne se vide jamais.'),
 (9001206, 'Potion d''enclos de Guilde infinie'),
 (9001207, 'Vous téléporte dans un enclos de votre guilde.
Cette potion ne se vide jamais.'),
 (9001208, 'Potion de Chasse au Trésor infinie'),
 (9001209, 'Vous téléporte devant la Malle aux Trésors (Champs de Cania).
Cette potion ne se vide jamais.');
COMMIT;
SELECT Id, Name, IconId, Criteria FROM items_templates WHERE Id BETWEEN 31200 AND 31204;
