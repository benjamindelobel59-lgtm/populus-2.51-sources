-- PADGREF 23/10 : annule 123
UPDATE monsters_grades SET StatsJSON = JSON_SET(StatsJSON, '$.SummonLimit', 3) WHERE MonsterId = 5955;
UPDATE monsters_grades SET StatsJSON = JSON_SET(StatsJSON, '$.SummonLimit', 1) WHERE MonsterId = 5979;
