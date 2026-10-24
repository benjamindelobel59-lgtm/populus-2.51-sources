-- PADGREF 23/10/2026 : SQL 123 — Eliocalypse (audit DPLN, Fers de la Tyrannie) : limites d'invocation : Servitude jusqu'à 6
-- Armécréantes (3 avant), Armécréante jusqu'à 2 Iopprimés (1 avant). Base world. Rejouable.
UPDATE monsters_grades SET StatsJSON = JSON_SET(StatsJSON, '$.SummonLimit', 6) WHERE MonsterId = 5955;
UPDATE monsters_grades SET StatsJSON = JSON_SET(StatsJSON, '$.SummonLimit', 2) WHERE MonsterId = 5979;
