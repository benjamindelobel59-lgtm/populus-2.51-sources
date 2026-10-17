-- MYSTIC 17/10 : gros chantiers des donjons (Frigost / Otomaï, dimensions, donjons récents). Règle : données 2.51.
-- Tynril : chaque Tynril a son élément (niveau de Hiffe / Helse) quel que soit son grade
-- Consterné 1072 = Air (1), Déconcerté 1085 = Eau (2), Perfide 1086 = Feu (3), Ahuri 1087 = Terre (5).
UPDATE monsters_spells s JOIN monsters_grades g ON g.Id = s.MonsterGradeId
SET s.Level = CASE g.MonsterId WHEN 1072 THEN 1 WHEN 1085 THEN 2 WHEN 1086 THEN 3 WHEN 1087 THEN 5 END
WHERE g.MonsterId IN (1072, 1085, 1086, 1087) AND s.SpellId IN (1058, 1059);

-- Ush : Ush Galesh noir (4265, Frappe Noire) manquait dans la salle du boss (map 162004992). Le passif Rouge et Noir de l'Ush
-- rouge (4264) agit sur les deux (partage des dommages, Rouge / Noir, Négativisme, mort de l'un = mort de l'autre).
-- Un Ush noir du même grade que l'Ush rouge dans chaque variante du groupe ; à 8, il remplace un Chacha (8 cases). Rejouable.
SET @ush := (SELECT Id FROM monsters_spawns_dungeons WHERE MapId = 162004992 LIMIT 1);
DELETE FROM monsters_spawns_dungeons_groups WHERE DungeonSpawnId = @ush AND MinPartyMembers < 8 AND MonsterGradeId BETWEEN 12525 AND 12529;
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations)
SELECT @ush, v.g, v.n, 'MYSTIC Ush noir' FROM (SELECT 12525 g, 1 n UNION SELECT 12526, 5 UNION SELECT 12527, 6 UNION SELECT 12528, 7) v
WHERE EXISTS (SELECT 1 FROM monsters_spawns_dungeons_groups x WHERE x.DungeonSpawnId = @ush AND x.MinPartyMembers = v.n);
SET @deja := (SELECT COUNT(*) FROM monsters_spawns_dungeons_groups WHERE DungeonSpawnId = @ush AND MinPartyMembers = 8 AND MonsterGradeId BETWEEN 12525 AND 12529);
UPDATE monsters_spawns_dungeons_groups SET MonsterGradeId = 12529, Informations = 'MYSTIC Ush noir'
WHERE DungeonSpawnId = @ush AND MinPartyMembers = 8 AND @deja = 0 AND MonsterGradeId NOT BETWEEN 12520 AND 12524
ORDER BY Id DESC LIMIT 1;
