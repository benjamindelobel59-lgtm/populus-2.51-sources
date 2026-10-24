-- PADGREF 23/10/2026 : SQL 133 (retours utilisateur). Base world. Rejouable. Généré à la main + valeurs_sort.py.
USE `world`;
-- 1) Ivremor (4161, Énervé 4224) : Cocktail Rhumotov 6068 au niveau 1 pour tous les grades (le niveau 2 = dégâts de la glyphe seulement :
--    aux grades 2-5 il ne posait jamais la glyphe).
UPDATE monsters_spells SET Level = 1 WHERE SpellId = 6068 AND MonsterGradeId IN (12022,12023,12024,12025,12026,12318,12319,12320,12321,12322);
-- 2) Sramouraï : Coupe-Vent (4724, niveau 22528) 35-39 -> 42-47 vol Air (critique au même rapport).
UPDATE spells_levels SET EffectsBin = 0x044601415D006CE60100000000000000000000000000000000000000000000014900024C320000000074120000040000000100000000000000002A0000002F000000, CriticalEffectsBin = 0x044601415D006DE60100000000000000000000000000000000000000000000014900024C320100000074120000040000000100000000000000003000000035000000 WHERE Id = 22528;
