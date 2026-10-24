-- PADGREF 23/10/2026 : SQL 119 — import 2.74 (lot 2) : coiffes / capes / boucliers / armes d'apparat choisis sur la planche des
-- skins (objets d'apparat type 177 sur le modèle d'un apparat 2.51 de même emplacement, AppearanceId = skin choisi) + dessin des
-- équipements importés sans apparence (recherchés de Frigost, Cavaliers). Généré par client-build/import274/lot2_import_skins.py.
-- Base world. Rejouable.
SET NAMES utf8mb4;
START TRANSACTION;
UPDATE items_templates SET AppearanceId = 5741 WHERE Id = 32207;
COMMIT;
