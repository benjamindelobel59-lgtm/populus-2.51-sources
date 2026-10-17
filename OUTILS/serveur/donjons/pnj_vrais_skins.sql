-- NEKFEU : PNJ de sortie de donjon avec le nom + l'apparence du vrai PNJ (genere par maj_pnj_vrais_skins.py). Apres pnj_boss.sql et sorties_astrub.sql.
UPDATE npcs_templates SET Name='Moon', NameId=8407, LookAsString='{375|||90}', Gender=0 WHERE Id=10016; -- vrai PNJ 160
UPDATE npcs_templates SET Name='Le Chouque', NameId=565555, LookAsString='{370}', Gender=0 WHERE Id=10017; -- vrai PNJ 3017
UPDATE npcs_templates SET Name='Koulosse', NameId=8367, LookAsString='{486|||100}', Gender=0 WHERE Id=10022; -- vrai PNJ 755
UPDATE npcs_templates SET Name='Skeunk', NameId=7648, LookAsString='{529|||130}', Gender=0 WHERE Id=10023; -- vrai PNJ 760
UPDATE npcs_templates SET Name='Qu''Tan', NameId=625365, LookAsString='{826}', Gender=0 WHERE Id=10028; -- vrai PNJ 3225
UPDATE npcs_templates SET Name='Halouine', NameId=360558, LookAsString='{1995}', Gender=0 WHERE Id=10029; -- vrai PNJ 1769
UPDATE npcs_templates SET Name='Klime', NameId=360982, LookAsString='{2051}', Gender=0 WHERE Id=10030; -- vrai PNJ 2045
UPDATE npcs_templates SET Name='Missiz Frizz', NameId=360991, LookAsString='{2079}', Gender=1 WHERE Id=10031; -- vrai PNJ 2040
UPDATE npcs_templates SET Name='Nileza', NameId=361020, LookAsString='{2057}', Gender=0 WHERE Id=10032; -- vrai PNJ 2028
UPDATE npcs_templates SET Name='Sylargh', NameId=360983, LookAsString='{2063}', Gender=0 WHERE Id=10033; -- vrai PNJ 2046
UPDATE npcs_templates SET Name='Comte Harebourg', NameId=360984, LookAsString='{2069|||150}', Gender=0 WHERE Id=10034; -- vrai PNJ 2047
UPDATE npcs_templates SET Name='Merkator', NameId=360809, LookAsString='{2432}', Gender=0 WHERE Id=10036; -- vrai PNJ 2197
UPDATE npcs_templates SET Name='Ombre', NameId=360863, LookAsString='{2256|||80}', Gender=0 WHERE Id=10037; -- vrai PNJ 2216
UPDATE npcs_templates SET Name='Roi Nidas', NameId=413691, LookAsString='{2730}', Gender=0 WHERE Id=10039; -- vrai PNJ 2355
UPDATE npcs_templates SET Name='Malléfisk', NameId=504908, LookAsString='{2675}', Gender=1 WHERE Id=10040; -- vrai PNJ 2395
UPDATE npcs_templates SET Name='Vortex', NameId=505101, LookAsString='{2966}', Gender=0 WHERE Id=10043; -- vrai PNJ 2659
UPDATE npcs_templates SET Name='XLII', NameId=505102, LookAsString='{2973}', Gender=0 WHERE Id=10044; -- vrai PNJ 2658
UPDATE npcs_templates SET Name='Reine Nyée', NameId=511614, LookAsString='{3135|||110}', Gender=1 WHERE Id=10046; -- vrai PNJ 2846
UPDATE npcs_templates SET Name='Dantinéa', NameId=634778, LookAsString='{3581|||80}', Gender=1 WHERE Id=10047; -- vrai PNJ 3257
UPDATE npcs_templates SET Name='Capitaine Meno', NameId=634749, LookAsString='{3605}', Gender=0 WHERE Id=10049; -- vrai PNJ 3258
UPDATE npcs_templates SET Name='Tal Kasha', NameId=713162, LookAsString='{3844|||87}', Gender=1 WHERE Id=10051; -- vrai PNJ 3685
UPDATE npcs_templates SET Name='Comte Razof', NameId=721744, LookAsString='{3985|||70}', Gender=0 WHERE Id=10052; -- vrai PNJ 3748
UPDATE npcs_templates SET Name='Anerice la Shushess', NameId=725658, LookAsString='{4030|||120}', Gender=1 WHERE Id=10053; -- vrai PNJ 3963
UPDATE npcs_templates SET Name='Ilyzaelle', NameId=732321, LookAsString='{4118}', Gender=1 WHERE Id=10054; -- vrai PNJ 4057
UPDATE npcs_templates SET Name='Solar', NameId=744866, LookAsString='{4514}', Gender=0 WHERE Id=10055; -- vrai PNJ 4301
UPDATE npcs_templates SET Name='Bethel Akarna', NameId=744867, LookAsString='{4669|||150}', Gender=0 WHERE Id=10056; -- vrai PNJ 4302
UPDATE npcs_templates SET Name='Dazak Martegel', NameId=757424, LookAsString='{5029}', Gender=0 WHERE Id=10057; -- vrai PNJ 4500
INSERT INTO npcs_templates (Id, Name, NameId, DialogMessagesIdCSV, DialogRepliesIdCSV, ActionsIdsCSV, Gender, LookAsString, SpecialArtworkId, TokenShop) VALUES (10058, 'Minotoror', 5994, '60001,24359', '2962,767152', '3', 0, '{344|||90}', 0, 0);
INSERT INTO npcs_actions (Id, NpcId, Type, Priority, Parameter0) VALUES (900253, 10058, 'Talk', 0, '60001');
INSERT INTO npcs_templates (Id, Name, NameId, DialogMessagesIdCSV, DialogRepliesIdCSV, ActionsIdsCSV, Gender, LookAsString, SpecialArtworkId, TokenShop) VALUES (10059, 'Brumen Tinctorias', 360448, '60001,24359', '2962,767152', '3', 0, '{2604|||110}', 0, 0);
INSERT INTO npcs_actions (Id, NpcId, Type, Priority, Parameter0) VALUES (900254, 10059, 'Talk', 0, '60001');
INSERT INTO npcs_templates (Id, Name, NameId, DialogMessagesIdCSV, DialogRepliesIdCSV, ActionsIdsCSV, Gender, LookAsString, SpecialArtworkId, TokenShop) VALUES (10060, 'Capitaine Ekarlatte', 505285, '60001,24359', '2962,767152', '3', 0, '{2793|||95}', 0, 0);
INSERT INTO npcs_actions (Id, NpcId, Type, Priority, Parameter0) VALUES (900255, 10060, 'Talk', 0, '60001');
UPDATE npcs_spawns SET NpcId=10058 WHERE Id=900300; -- donjon 4
UPDATE npcs_spawns SET NpcId=10059 WHERE Id=900301; -- donjon 35
UPDATE npcs_spawns SET NpcId=10060 WHERE Id=900302; -- donjon 81
