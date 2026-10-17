-- MYSTIC 17/10/2026 : zaap côtier d'Otomaï (-46,18) inutilisable et position de la carte du monde figée (signalé).
-- Cause : 15 maisons (11 cartes autour du Village côtier) ont un modèle 414 / 425 / 435 que le client 2.51 n'a pas
-- (Houses.d2o s'arrête à 400) -> le client plante en recevant la carte (TypeError #1009) et n'installe ni les objets
-- interactifs (zaap) ni la position. Rattachées au modèle « village côtier » connu du client : 425 (2,5 M) -> 389 Moyenne,
-- 414 et 435 (5 M) -> 390 Grande. Prix inchangés. Rejouable.
UPDATE houses SET ModelId = 389 WHERE ModelId = 425;
UPDATE houses SET ModelId = 390 WHERE ModelId IN (414, 435);
-- Contrôle : attendu modeles_inconnus = 0 (aucun modèle au-delà de 400)
SELECT COUNT(*) AS modeles_inconnus FROM houses WHERE ModelId > 400;
