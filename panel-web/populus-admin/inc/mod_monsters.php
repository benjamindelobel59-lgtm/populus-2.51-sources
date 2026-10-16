<?php
/*
 * Panel admin Populus 2.51 : onglet Monstres (modèle, grades, sorts des grades, drops, zones d'apparition,
 * groupes sur ta carte). Chaque enregistrement est appliqué en jeu sans redémarrage (reload_monster / mob_zone).
 */
if (!defined('POPULUS_ADMIN'))
    exit;

const GRADE_FIELDS = ['Level', 'GradeXp', 'LifePoints', 'ActionPoints', 'MovementPoints', 'PaDodge', 'PmDodge', 'TackleEvade', 'TackleBlock',
    'Strength', 'Chance', 'Vitality', 'Wisdom', 'Intelligence', 'Agility',
    'EarthResistance', 'AirResistance', 'FireResistance', 'WaterResistance', 'NeutralResistance', 'HiddenLevel'];
const TEMPLATE_FIELDS = ['GfxId', 'MinDroppedKamas', 'MaxDroppedKamas', 'LookAsString', 'CanPlay', 'CanTackle', 'CanSwitchPos', 'CanBePushed',
    'IsBoss', 'IsActive', 'UseSummonSlot', 'UseBombSlot', 'canBeCarried'];
const DROP_FIELDS = ['DropRateForGrade1', 'DropRateForGrade2', 'DropRateForGrade3', 'DropRateForGrade4', 'DropRateForGrade5', 'DropLimit',
    'RollsCounter', 'ProspectingLock', 'Condition', 'DropGroup', 'TaxCollectorCannotLoot'];

function monster_reload($monsterId)
{
    $r = enqueue('reload_monster', (string)(int)$monsterId);
    return $r;
}

function subarea_names(array $ids)
{
    $ids = array_values(array_unique(array_filter(array_map('intval', $ids))));
    if (!$ids)
        return [];
    $in = implode(',', array_fill(0, count($ids), '?'));
    $rows = q("SELECT Id, NameId FROM world_subareas WHERE Id IN ($in)", $ids)->fetchAll();
    $t = lang_map(array_column($rows, 'NameId'));
    $out = [];
    foreach ($rows as $r)
        $out[(int)$r['Id']] = $t[(int)$r['NameId']] ?? ('sous-zone ' . $r['Id']);
    return $out;
}

function handle_monsters($action, $me)
{
    $in = $GLOBALS['in'];
    switch ($action) {

        case 'mobs':
            $s = trim((string)($_GET['q'] ?? ''));
            if ($s === '')
                json_out(['mobs' => []]);
            $where = ctype_digit($s) ? 't.Id = ?' : 'l.French LIKE ?';
            $arg = ctype_digit($s) ? (int)$s : "%$s%";
            $rows = q("SELECT t.Id, COALESCE(NULLIF(l.French,''), t.Name) Name, t.IsBoss,
                    (SELECT MIN(Level) FROM monsters_grades g WHERE g.MonsterId = t.Id) MinLevel,
                    (SELECT MAX(Level) FROM monsters_grades g WHERE g.MonsterId = t.Id) MaxLevel
                FROM monsters_templates t LEFT JOIN langs l ON l.Id = t.NameId WHERE $where ORDER BY MinLevel, Name LIMIT 150", [$arg])->fetchAll();
            json_out(['mobs' => $rows]);

        case 'mob':
            $id = (int)($_GET['id'] ?? 0);
            $t = q('SELECT * FROM monsters_templates WHERE Id = ?', [$id])->fetch();
            if (!$t)
                fail('Monstre introuvable');
            $race = q('SELECT NameId FROM monsters_races WHERE Id = ?', [$t['RaceId']])->fetchColumn();
            $texts = lang_map([$t['NameId'], $race]);
            $grades = q('SELECT * FROM monsters_grades WHERE MonsterId = ? ORDER BY GradeId', [$id])->fetchAll();
            $spells = [];
            if ($grades) {
                $ids = array_column($grades, 'Id');
                $inq = implode(',', array_fill(0, count($ids), '?'));
                $rows = q("SELECT ms.Id, ms.MonsterGradeId, ms.SpellId, ms.Level, st.NameId FROM monsters_spells ms
                    LEFT JOIN spells_templates st ON st.Id = ms.SpellId WHERE ms.MonsterGradeId IN ($inq) ORDER BY ms.Id", $ids)->fetchAll();
                $names = lang_map(array_column($rows, 'NameId'));
                foreach ($rows as $r) {
                    $r['Name'] = $names[(int)$r['NameId']] ?? ('sort ' . $r['SpellId']);
                    $spells[(int)$r['MonsterGradeId']][] = $r;
                }
            }
            $drops = q('SELECT d.*, i.NameId ItemNameId, i.Level ItemLevel FROM monsters_drops d LEFT JOIN items_templates i ON i.Id = d.ItemId
                WHERE d.MonsterOwnerId = ? ORDER BY d.Id', [$id])->fetchAll();
            $itemNames = lang_map(array_column($drops, 'ItemNameId'));
            foreach ($drops as &$d)
                $d['ItemName'] = $itemNames[(int)$d['ItemNameId']] ?? ('objet ' . $d['ItemId']);
            unset($d);
            $zones = q('SELECT * FROM monsters_spawns WHERE MonsterId = ? ORDER BY IsDisabled, Id', [$id])->fetchAll();
            $saNames = subarea_names(array_column($zones, 'SubAreaId'));
            foreach ($zones as &$z)
                $z['SubAreaName'] = $saNames[(int)$z['SubAreaId']] ?? '';
            unset($z);
            $fixed = (int)q('SELECT COUNT(*) FROM monsters_spawns_static_groups sg JOIN monsters_grades g ON g.Id = sg.MonsterGradeId WHERE g.MonsterId = ?', [$id])->fetchColumn();
            $dungeon = (int)q('SELECT COUNT(DISTINCT dg.DungeonSpawnId) FROM monsters_spawns_dungeons_groups dg JOIN monsters_grades g ON g.Id = dg.MonsterGradeId WHERE g.MonsterId = ?', [$id])->fetchColumn();
            json_out(['template' => $t, 'name' => $texts[(int)$t['NameId']] ?? $t['Name'], 'race' => $texts[(int)$race] ?? '',
                'grades' => $grades, 'spells' => $spells, 'drops' => $drops, 'zones' => $zones, 'fixed' => $fixed, 'dungeonRooms' => $dungeon]);

        case 'mob_save':
            $id = (int)($in['id'] ?? 0);
            if (!q('SELECT 1 FROM monsters_templates WHERE Id = ?', [$id])->fetch())
                fail('Monstre introuvable');
            $changes = 0;
            $tpl = $in['template'] ?? [];
            $set = [];
            $vals = [];
            foreach (TEMPLATE_FIELDS as $f)
                if (array_key_exists($f, $tpl)) {
                    $set[] = "`$f` = ?";
                    $vals[] = $f === 'LookAsString' ? (string)$tpl[$f] : (int)$tpl[$f];
                }
            if ($set) {
                $before = snapshot('monsters_templates', $id);
                q('UPDATE monsters_templates SET ' . implode(', ', $set) . ' WHERE Id = ?', array_merge($vals, [$id]));
                if (json_encode($before) !== json_encode(snapshot('monsters_templates', $id))) {
                    log_change('Monstre modifié', 'monsters_templates', $id, $before, "monstre $id");
                    $changes++;
                }
            }
            foreach ($in['grades'] ?? [] as $g) {
                $gid = (int)($g['Id'] ?? 0);
                if (!q('SELECT 1 FROM monsters_grades WHERE Id = ? AND MonsterId = ?', [$gid, $id])->fetch())
                    continue;
                $set = [];
                $vals = [];
                foreach (GRADE_FIELDS as $f)
                    if (array_key_exists($f, $g) && is_numeric($g[$f])) {
                        $set[] = "`$f` = ?";
                        $vals[] = (int)$g[$f];
                    }
                if (!$set)
                    continue;
                $before = snapshot('monsters_grades', $gid);
                q('UPDATE monsters_grades SET ' . implode(', ', $set) . ' WHERE Id = ?', array_merge($vals, [$gid]));
                if (json_encode($before) !== json_encode(snapshot('monsters_grades', $gid))) {
                    log_change('Grade de monstre modifié', 'monsters_grades', $gid, $before, "monstre $id grade {$before['GradeId']}");
                    $changes++;
                }
            }
            json_out(['ok' => true, 'changes' => $changes, 'reload' => $changes ? monster_reload($id) : null]);

        case 'mob_spell_add':
            $grade = (int)($in['gradeId'] ?? 0);
            $spell = (int)($in['spellId'] ?? 0);
            $g = q('SELECT MonsterId FROM monsters_grades WHERE Id = ?', [$grade])->fetch();
            if (!$g)
                fail('Grade introuvable');
            if (!q('SELECT 1 FROM spells_templates WHERE Id = ?', [$spell])->fetch())
                fail("Le sort $spell n'existe pas");
            q('INSERT INTO monsters_spells (MonsterGradeId, SpellId, Level) VALUES (?, ?, ?)', [$grade, $spell, max(1, (int)($in['level'] ?? 1))]);
            log_change('Sort de monstre ajouté', 'monsters_spells', (int)db()->lastInsertId(), null, "sort $spell au grade $grade");
            json_out(['ok' => true, 'reload' => monster_reload($g['MonsterId'])]);

        case 'mob_spell_delete':
            $sid = (int)($in['id'] ?? 0);
            $row = q('SELECT ms.*, g.MonsterId FROM monsters_spells ms JOIN monsters_grades g ON g.Id = ms.MonsterGradeId WHERE ms.Id = ?', [$sid])->fetch();
            if (!$row)
                fail('Sort introuvable');
            $before = snapshot('monsters_spells', $sid);
            q('DELETE FROM monsters_spells WHERE Id = ?', [$sid]);
            log_change('Sort de monstre retiré', 'monsters_spells', $sid, $before, "sort {$row['SpellId']} du grade {$row['MonsterGradeId']}");
            json_out(['ok' => true, 'reload' => monster_reload($row['MonsterId'])]);

        case 'spell_search':
            $s = trim((string)($_GET['q'] ?? ''));
            if ($s === '')
                json_out(['spells' => []]);
            $rows = ctype_digit($s)
                ? q('SELECT st.Id, l.French Name FROM spells_templates st LEFT JOIN langs l ON l.Id = st.NameId WHERE st.Id = ?', [(int)$s])->fetchAll()
                : q('SELECT st.Id, l.French Name FROM spells_templates st JOIN langs l ON l.Id = st.NameId WHERE l.French LIKE ? ORDER BY l.French LIMIT 40', ["%$s%"])->fetchAll();
            json_out(['spells' => $rows]);

        case 'drop_save':
            $mob = (int)($in['monsterId'] ?? 0);
            $did = (int)($in['id'] ?? 0);
            $item = (int)($in['itemId'] ?? 0);
            if (!q('SELECT 1 FROM monsters_templates WHERE Id = ?', [$mob])->fetch())
                fail('Monstre introuvable');
            if (!$did && !q('SELECT 1 FROM items_templates WHERE Id = ?', [$item])->fetch())
                fail("L'objet $item n'existe pas");
            $vals = [];
            foreach (DROP_FIELDS as $f) {
                $v = $in[$f] ?? null;
                if ($f === 'Condition')
                    $vals[$f] = trim((string)$v) === '' ? null : trim((string)$v);
                elseif (strpos($f, 'DropRate') === 0)
                    $vals[$f] = (float)str_replace(',', '.', (string)$v);
                else
                    $vals[$f] = (int)$v;
            }
            if ($did) {
                $before = snapshot('monsters_drops', $did);
                if (!$before || (int)$before['MonsterOwnerId'] !== $mob)
                    fail('Drop introuvable');
                $set = implode(', ', array_map(fn($f) => "`$f` = ?", array_keys($vals)));
                q("UPDATE monsters_drops SET $set WHERE Id = ?", array_merge(array_values($vals), [$did]));
                log_change('Drop modifié', 'monsters_drops', $did, $before, "monstre $mob objet {$before['ItemId']}");
            } else {
                $cols = implode(', ', array_map(fn($f) => "`$f`", array_keys($vals)));
                $ph = implode(', ', array_fill(0, count($vals) + 2, '?'));
                q("INSERT INTO monsters_drops (MonsterOwnerId, ItemId, $cols) VALUES ($ph)", array_merge([$mob, $item], array_values($vals)));
                $did = (int)db()->lastInsertId();
                log_change('Drop ajouté', 'monsters_drops', $did, null, "monstre $mob objet $item");
            }
            json_out(['ok' => true, 'id' => $did, 'reload' => monster_reload($mob)]);

        case 'drop_delete':
            $did = (int)($in['id'] ?? 0);
            $before = snapshot('monsters_drops', $did);
            if (!$before)
                fail('Drop introuvable');
            q('DELETE FROM monsters_drops WHERE Id = ?', [$did]);
            log_change('Drop retiré', 'monsters_drops', $did, $before, "monstre {$before['MonsterOwnerId']} objet {$before['ItemId']}");
            json_out(['ok' => true, 'reload' => monster_reload($before['MonsterOwnerId'])]);

        case 'zone_save':
            $mob = (int)($in['monsterId'] ?? 0);
            $zid = (int)($in['id'] ?? 0);
            $sub = (int)($in['subAreaId'] ?? 0);
            $map = (int)($in['mapId'] ?? 0);
            if (!q('SELECT 1 FROM monsters_templates WHERE Id = ?', [$mob])->fetch())
                fail('Monstre introuvable');
            if ($map && !q('SELECT 1 FROM world_maps WHERE Id = ?', [$map])->fetch())
                fail("La map $map n'existe pas");
            if (!$map && !q('SELECT 1 FROM world_subareas WHERE Id = ?', [$sub])->fetch())
                fail('Choisis une sous-zone ou une map');
            $freq = (float)str_replace(',', '.', (string)($in['frequency'] ?? 1));
            $min = max(1, (int)($in['minGrade'] ?? 1));
            $max = max($min, (int)($in['maxGrade'] ?? 5));
            $dis = !empty($in['disabled']) ? 1 : 0;
            if ($zid) {
                $before = snapshot('monsters_spawns', $zid);
                if (!$before)
                    fail('Zone introuvable');
                q('UPDATE monsters_spawns SET MapId = ?, SubAreaId = ?, Frequency = ?, MinGrade = ?, MaxGrade = ?, IsDisabled = ? WHERE Id = ?',
                    [$map ?: null, $map ? null : $sub, $freq, $min, $max, $dis, $zid]);
                log_change('Zone de monstre modifiée', 'monsters_spawns', $zid, $before, "monstre $mob");
            } else {
                q('INSERT INTO monsters_spawns (MapId, SubAreaId, MonsterId, Frequency, MinGrade, MaxGrade, IsDisabled) VALUES (?, ?, ?, ?, ?, ?, ?)',
                    [$map ?: null, $map ? null : $sub, $mob, $freq, $min, $max, $dis]);
                $zid = (int)db()->lastInsertId();
                log_change('Zone de monstre ajoutée', 'monsters_spawns', $zid, null, "monstre $mob");
            }
            json_out(['ok' => true, 'id' => $zid, 'reload' => enqueue('mob_zone', "$zid;" . ($dis ? 0 : 1))]);

        case 'zone_delete':
            $zid = (int)($in['id'] ?? 0);
            $before = snapshot('monsters_spawns', $zid);
            if (!$before)
                fail('Zone introuvable');
            q('DELETE FROM monsters_spawns WHERE Id = ?', [$zid]);
            log_change('Zone de monstre supprimée', 'monsters_spawns', $zid, $before, "monstre {$before['MonsterId']}");
            json_out(['ok' => true, 'reload' => enqueue('mob_zone', "$zid;-1")]);

        case 'subarea_search':
            $s = trim((string)($_GET['q'] ?? ''));
            if ($s === '')
                json_out(['subareas' => []]);
            $rows = ctype_digit($s)
                ? q('SELECT s.Id, l.French Name FROM world_subareas s LEFT JOIN langs l ON l.Id = s.NameId WHERE s.Id = ?', [(int)$s])->fetchAll()
                : q('SELECT s.Id, l.French Name FROM world_subareas s JOIN langs l ON l.Id = s.NameId WHERE l.French LIKE ? ORDER BY l.French LIMIT 40', ["%$s%"])->fetchAll();
            json_out(['subareas' => $rows]);

        case 'map_mobs':
            $r = server_json('map_mobs');
            $ids = [];
            foreach ($r['groups'] as $g)
                foreach ($g['monsters'] as $m)
                    $ids[] = $m['id'];
            $names = [];
            if ($ids) {
                $ids = array_values(array_unique($ids));
                $inq = implode(',', array_fill(0, count($ids), '?'));
                $rows = q("SELECT Id, NameId FROM monsters_templates WHERE Id IN ($inq)", $ids)->fetchAll();
                $t = lang_map(array_column($rows, 'NameId'));
                foreach ($rows as $row)
                    $names[(int)$row['Id']] = $t[(int)$row['NameId']] ?? null;
            }
            $r['names'] = $names;
            $r['subAreaName'] = $r['subAreaId'] ? (subarea_names([$r['subAreaId']])[(int)$r['subAreaId']] ?? '') : '';
            json_out($r);
    }
    return false;
}
