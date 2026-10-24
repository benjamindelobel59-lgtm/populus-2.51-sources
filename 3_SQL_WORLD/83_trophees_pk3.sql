-- PADGREF 21/10 : trophées : condition « moins de 2 bonus de panoplie » → « moins de 3 » (Pk<2 → Pk<3). Base world.
UPDATE items_templates SET Criteria = 'Pk<3' WHERE TypeId = 151 AND Criteria = 'Pk<2';
