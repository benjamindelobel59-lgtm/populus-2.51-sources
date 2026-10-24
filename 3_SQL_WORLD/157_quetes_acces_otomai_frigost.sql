-- PADGREF 24/10 SQL 157 (demande utilisateur) : quêtes d'accès à Otomaï et à Frigost.
-- OTOMAÏ (Capitaine Kradoc, PNJ 925) : « Le nouveau monde » (231, étape 422, donnée à la création du personnage) : rapporter au
-- capitaine (port de Sufokia 95420420) 2 x 429, 2553, 2551, 399 ; puis « L'île des naufragés » (232, étape 423) : le capitaine
-- t'emmène sur l'île des naufragés (153090) où il faut lui rapporter 1002, 8759, 2618, 2617 ; ensuite il emmène sur Otomaï. Quand
-- les deux sont faites, il emmène directement sur Otomaï (comme avant). Les objets sont pris quand on lui parle (objectif officiel).
-- FRIGOST (Capitaine Ardier, PNJ 1236, baie de Cania) : « Se rendre sur l'île de Frigost » donne « Bienvenue à Frigost » (649,
-- étape 1208) et lance le récit du voyage (4 messages du client) jusqu'au port de Frigost ; lui reparler à l'arrivée termine la quête.
-- Sans ces quêtes : ni maison, ni enclos, ni zaap, ni prisme de l'île (code serveur AccesFrigost.QueteAcces).
-- Réponses « afficher:<critères> » : condition d'affichage (NpcReply.CanShow, PADGREF 24/10).

-- ------------------------------------------------------------------ Otomaï : Capitaine Kradoc
DELETE FROM npcs_actions WHERE Id IN (203, 907300, 907301, 907302, 907303, 907304, 907305, 907306);
INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES
 (907300, 925, 'Talk', 'Mp=95420420&Qf=232', 1, '4159'),             -- tout fait : « Alors comme ça tu veux repartir sur cette île ? »
 (907301, 925, 'Talk', 'Mp=95420420&Qf=231', 2, '4127'),             -- 231 faite : « Voilà tout est prêt pour ton départ »
 (907302, 925, 'Talk', 'Mp=95420420&Qa=231', 3, '4158'),             -- 231 en cours : « Alors, as-tu trouvé les fournitures ? »
 (907303, 925, 'Talk', 'Mp=95420420', 4, '4123'),                    -- rien : présentation, « Accepter »
 (907304, 925, 'Talk', 'Mp=153090&Qf=232', 1, '4134'),               -- île des naufragés, 232 faite : on repart
 (907305, 925, 'Talk', 'Mp=153090&Qa=232', 2, '4132'),               -- 232 en cours : « j'ai soif, et j'ai un petit creux »
 (907306, 925, 'Talk', 'Mp=153090', 3, '4130');                      -- arrivée : « nous nous sommes échoués… »
DELETE FROM npcs_spawns WHERE Id = 907330;
INSERT INTO npcs_spawns (Id, NpcId, MapId, CellId, Direction) VALUES (907330, 925, 153090, 245, 1);
DELETE FROM npcs_replies WHERE Id BETWEEN 907310 AND 907316;
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId, Criteria, Parameter0, Parameter1, Parameter2) VALUES
 (907310, 'Quest', 3618, 4123, NULL, '422', NULL, NULL),             -- Accepter -> « Le nouveau monde »
 (907311, 'Dialog', 3645, 4158, 'Qf=231', '4127', NULL, NULL),       -- Donner les fournitures (prises en parlant) -> départ
 (907313, 'Teleport', 3622, 4127, 'Qf=231', '153090', '260', '1'),   -- Partir vers l'île -> île des naufragés
 (907314, 'EndDialog', 3627, 4132, NULL, NULL, NULL, NULL),          -- Donner les objets (pris en parlant)
 (907315, 'Quest', 3624, 4130, NULL, '423', NULL, NULL),             -- Continuer -> « L'île des naufragés »
 (907316, 'Teleport', 3628, 4134, 'Qf=232', '153621', '175', '3');   -- Partir de l'île des naufragés -> Otomaï

-- ------------------------------------------------------------------ Frigost : Capitaine Ardier
DELETE FROM npcs_actions WHERE Id = 907340;
INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES
 (907340, 1236, 'Talk', 'Mp=54175012&Qa=649', -1, '7701');          -- au port de Frigost, quête en cours : « Et voilà… »
UPDATE npcs_replies SET AdditionalParameters = 'afficher:Qf=649|Qa=649', Criteria = 'Qf=649|Qa=649' WHERE Id = 66;  -- aller direct
DELETE FROM npcs_replies WHERE Id BETWEEN 907320 AND 907325;
INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId, Criteria, Parameter0, Parameter1, Parameter2, AdditionalParameters) VALUES
 (907320, 'Quest', 7760, 7697, NULL, '1208', NULL, NULL, 'afficher:Qf!649&Qa!649'),  -- Se rendre sur Frigost -> quête 649
 (907321, 'Dialog', 7760, 7697, NULL, '7698', NULL, NULL, 'afficher:Qf!649&Qa!649'), --   + récit du voyage
 (907322, 'Dialog', 7761, 7698, NULL, '7699', NULL, NULL, NULL),
 (907323, 'Dialog', 7762, 7699, NULL, '7700', NULL, NULL, NULL),
 (907324, 'Teleport', 7763, 7700, NULL, '54175012', '272', '1', NULL),            -- arrivée au port de Frigost
 (907325, 'QuestFinish', 19355, 7701, NULL, '649', NULL, NULL, NULL);             -- « Le saluer » -> quête terminée
