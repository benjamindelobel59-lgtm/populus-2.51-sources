-- PADGREF 21/10 : objets à condition PA < 12 ou PM < 6 : les deux conditions, l'une OU l'autre (CP<12|CM<6). Base world.
UPDATE items_templates SET Criteria = 'CP<12|CM<6' WHERE Criteria IN ('CP<12', 'CM<6');
