-- PADGREF 21/10 : Anneau Aimgéroks (11738) : PA < 11 → PA < 12 ou PM < 6, comme les autres objets du SQL 84 et comme DofusDB. Base world.
UPDATE items_templates SET Criteria = 'CP<12|CM<6' WHERE Criteria = 'CP<11';
