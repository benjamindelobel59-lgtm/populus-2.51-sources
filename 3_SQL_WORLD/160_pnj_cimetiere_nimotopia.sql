-- PADGREF 24/10 SQL 160 : Warko Linch (Kaliptus <-> Cimetière primitif) et Agathe Mainate (Cania <-> Nimotopia).
-- Généré par _OUTILS/client/client-build/pnj_passages/generer.py (client : Npcs.d2o + i18n_fr.d2i).
DELETE FROM npcs_actions WHERE Id BETWEEN 907400 AND 907449;
DELETE FROM npcs_spawns WHERE Id BETWEEN 907400 AND 907449;
DELETE FROM npcs_replies WHERE Id BETWEEN 907400 AND 907499;
DELETE FROM npcs_messages WHERE Id = 907450;
INSERT INTO npcs_messages (Id, MessageId, ParametersCSV) VALUES (907450, 9121000, '');
INSERT INTO npcs_spawns (Id, NpcId, MapId, CellId, Direction) VALUES
 (907400, 1219, 64487686, 370, 1),     -- Warko du cimetière (retour)
 (907401, 3732, 164102659, 291, 1),       -- Agathe devant le Muldobus de Cania
 (907402, 3732, 179309058, 283, 1);        -- Agathe sur Nimotopia (retour)
INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES
 (907403, 1219, 'Talk', 'Mp=73402891', 0, '5325'),
 (907404, 1219, 'Talk', 'Mp=64487686', 0, '907450'),
 (907405, 3732, 'Talk', 'Mp=164102659', 0, '26861'),
 (907406, 3732, 'Talk', 'Mp=179309058', 0, '26860');
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId, Criteria, Parameter0, Parameter1, Parameter2) VALUES
 (907410, 'Teleport', 907451, 907450, NULL, '73402891', '372', '1'),
 (907411, 'Dialog', 32280, 26861, 'PL>39', '26862', NULL, NULL),          -- « Oui ! » -> couleur préférée
 (907412, 'Teleport', 32281, 26861, 'PL>39', '179309058', '297', '1'),     -- déjà répondu -> l'île
 (907413, 'EndDialog', 32282, 26861, NULL, NULL, NULL, NULL),
 (907414, 'Dialog', 32277, 26862, NULL, '26864', NULL, NULL),             -- « Le rose. » (bonne réponse)
 (907415, 'Dialog', 32278, 26862, NULL, '26863', NULL, NULL),
 (907416, 'Dialog', 32279, 26862, NULL, '26863', NULL, NULL),
 (907417, 'Dialog', 32283, 26863, NULL, '26861', NULL, NULL),             -- reprendre depuis le début
 (907418, 'Dialog', 32288, 26864, NULL, '26866', NULL, NULL),             -- « tout plein de bisous » (bonne réponse)
 (907419, 'Dialog', 32286, 26864, NULL, '26865', NULL, NULL),
 (907420, 'Dialog', 32287, 26864, NULL, '26865', NULL, NULL),
 (907421, 'EndDialog', 32284, 26864, NULL, NULL, NULL, NULL),
 (907422, 'EndDialog', 32285, 26864, NULL, NULL, NULL, NULL),
 (907423, 'Dialog', 32289, 26865, NULL, '26861', NULL, NULL),
 (907424, 'Dialog', 32293, 26866, NULL, '26867', NULL, NULL),             -- « aventurier avant tout » (bonne réponse)
 (907425, 'Dialog', 32290, 26866, NULL, '27298', NULL, NULL),
 (907426, 'Dialog', 32291, 26866, NULL, '27298', NULL, NULL),
 (907427, 'Dialog', 32772, 27298, NULL, '26861', NULL, NULL),
 (907428, 'Teleport', 32292, 26867, NULL, '179309058', '297', '1'),       -- Muldobus -> Nimotopia
 (907429, 'Teleport', 32276, 26860, NULL, '164102659', '305', '1');      -- quitter Nimotopia -> Cania
