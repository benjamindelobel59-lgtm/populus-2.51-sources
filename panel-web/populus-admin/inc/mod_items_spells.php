<?php
/*
 * Panel admin Populus 2.51 : onglets Objets et Sorts.
 * Les champs simples sont écrits directement en base ; les effets (stockés en binaire) passent par le serveur de jeu
 * (item_effects / spell_effects), qui sait les lire et les écrire. Tout est rechargé à chaud (reload_item / reload_spell).
 */
if (!defined('POPULUS_ADMIN'))
    exit;

const ITEM_FIELDS = ['Level', 'Price', 'Weight', 'RealWeight', 'Criteria', 'Usable', 'Targetable', 'Etheral', 'Cursed', 'TwoHanded',
    'IsLinkedToOwner', 'HideEffects', 'BonusIsSecret', 'CraftXpRatio', 'AppearanceId'];
const WEAPON_FIELDS = ['ApCost', 'MinRange', 'WeaponRange', 'CastInLine', 'CastInDiagonal', 'CastTestLos', 'CriticalHitProbability',
    'CriticalHitBonus', 'CriticalFailureProbability', 'MaxCastPerTurn'];
const SPELL_LEVEL_FIELDS = ['ApCost', 'MinRange', 'Range', 'RangeCanBeBoosted', 'CastInLine', 'CastInDiagonal', 'CastTestLos',
    'CriticalHitProbability', 'CriticalFailureProbability', 'CriticalFailureEndsTurn', 'NeedFreeCell', 'NeedTakenCell', 'NeedFreeTrapCell',
    'MaxStack', 'MaxCastPerTurn', 'MaxCastPerTarget', 'MinCastInterval', 'InitialCooldown', 'GlobalCooldown', 'MinPlayerLevel', 'HideEffects', 'Hidden'];

function item_table($id)
{
    if (q('SELECT 1 FROM items_templates_weapons WHERE Id = ?', [$id])->fetch())
        return 'items_templates_weapons';
    if (q('SELECT 1 FROM items_templates WHERE Id = ?', [$id])->fetch())
        return 'items_templates';
    return null;
}

/** Ligne sans les colonnes binaires (pour le JSON). */
function no_binary(array $row)
{
    foreach ($row as $k => $v)
        if (is_string($v) && !mb_check_encoding($v, 'UTF-8'))
            unset($row[$k]);
    return $row;
}

/** Textes des effets (« #1{~1~2 à }#2 Force »), par numéro d'effet. */
function effect_texts(array $ids = null)
{
    $sql = 'SELECT Id, DescriptionId, Characteristic, Boost, ShowInTooltip, UseDice, Operator FROM effects_templates';
    $rows = $ids === null ? q($sql)->fetchAll() : ($ids ? q($sql . ' WHERE Id IN (' . implode(',', array_map('intval', $ids)) . ')')->fetchAll() : []);
    $t = lang_map(array_column($rows, 'DescriptionId'));
    $out = [];
    foreach ($rows as $r)
        $out[(int)$r['Id']] = ['text' => $t[(int)$r['DescriptionId']] ?? null, 'stat' => (int)$r['Characteristic'], 'tooltip' => (int)$r['ShowInTooltip'], 'op' => $r['Operator']];
    return $out;
}

function handle_items_spells($action, $me)
{
    $in = $GLOBALS['in'];
    switch ($action) {

        /* ============================================================ objets */
        case 'items':
            $s = trim((string)($_GET['q'] ?? ''));
            if ($s === '')
                json_out(['items' => []]);
            $sql = "SELECT i.Id, l.French Name, i.Level, i.TypeId, lt.French TypeName, (w.Id IS NOT NULL) IsWeapon
                FROM (SELECT Id, NameId, Level, TypeId FROM items_templates UNION ALL SELECT Id, NameId, Level, TypeId FROM items_templates_weapons) i
                LEFT JOIN items_templates_weapons w ON w.Id = i.Id
                LEFT JOIN langs l ON l.Id = i.NameId LEFT JOIN items_types t ON t.Id = i.TypeId LEFT JOIN langs lt ON lt.Id = t.NameId ";
            $rows = ctype_digit($s) ? q($sql . 'WHERE i.Id = ?', [(int)$s])->fetchAll()
                : q($sql . 'WHERE l.French LIKE ? ORDER BY i.Level, l.French LIMIT 150', ["%$s%"])->fetchAll();
            json_out(['items' => $rows]);

        case 'item':
            $id = (int)($_GET['id'] ?? 0);
            $table = item_table($id);
            if (!$table)
                fail('Objet introuvable');
            $row = no_binary(q("SELECT * FROM `$table` WHERE Id = ?", [$id])->fetch());
            $texts = lang_map([$row['NameId'], $row['DescriptionId']]);
            $type = q('SELECT t.Id, l.French Name FROM items_types t LEFT JOIN langs l ON l.Id = t.NameId WHERE t.Id = ?', [$row['TypeId']])->fetch();
            $set = $row['ItemSetId'] > 0 ? q('SELECT s.Id, l.French Name, s.ItemsCSV FROM items_sets s LEFT JOIN langs l ON l.Id = s.NameId WHERE s.Id = ?', [$row['ItemSetId']])->fetch() : null;
            $r = enqueue('item_effects', (string)$id);
            $effects = !empty($r['ok']) ? json_decode($r['result'], true) : null;
            // où le trouver
            $drops = q('SELECT d.MonsterOwnerId, d.DropRateForGrade1, d.DropRateForGrade5, l.French Name FROM monsters_drops d
                JOIN monsters_templates m ON m.Id = d.MonsterOwnerId LEFT JOIN langs l ON l.Id = m.NameId WHERE d.ItemId = ? ORDER BY d.DropRateForGrade5 DESC LIMIT 30', [$id])->fetchAll();
            $shops = q("SELECT a.NpcId, l.French Name, n.CustomPrice FROM npcs_items n JOIN npcs_actions a ON a.Id = n.NpcShopId
                JOIN npcs_templates t ON t.Id = a.NpcId LEFT JOIN langs l ON l.Id = t.NameId WHERE n.ItemId = ? LIMIT 30", [$id])->fetchAll();
            json_out(['item' => $row, 'table' => $table, 'name' => $texts[(int)$row['NameId']] ?? $row['Name'], 'description' => $texts[(int)$row['DescriptionId']] ?? '',
                'type' => $type, 'set' => $set, 'effects' => $effects, 'effectsError' => $effects === null ? ($r['result'] ?? 'serveur de jeu injoignable') : null,
                'effectTexts' => effect_texts($effects ? array_column($effects, 'effectId') : []), 'drops' => $drops, 'shops' => $shops]);

        case 'effects_list':
            json_out(['effects' => effect_texts()]);

        case 'item_save':
            $id = (int)($in['id'] ?? 0);
            $table = item_table($id);
            if (!$table)
                fail('Objet introuvable');
            $fields = $table === 'items_templates_weapons' ? array_merge(ITEM_FIELDS, WEAPON_FIELDS) : ITEM_FIELDS;
            $set = [];
            $vals = [];
            foreach ($fields as $f)
                if (array_key_exists($f, $in['fields'] ?? [])) {
                    $v = $in['fields'][$f];
                    $set[] = "`$f` = ?";
                    $vals[] = $f === 'Criteria' ? (trim((string)$v) === '' ? null : trim((string)$v)) : (is_numeric($v) ? $v + 0 : 0);
                }
            if (!$set)
                fail('Rien à enregistrer');
            $before = snapshot($table, $id);
            q("UPDATE `$table` SET " . implode(', ', $set) . ' WHERE Id = ?', array_merge($vals, [$id]));
            log_change('Objet modifié', $table, $id, $before, "objet $id");
            json_out(['ok' => true, 'reload' => enqueue('reload_item', (string)$id)]);

        case 'item_effects_save':
            $id = (int)($in['id'] ?? 0);
            $table = item_table($id);
            if (!$table)
                fail('Objet introuvable');
            $effects = array_map(fn($e) => [
                'i' => (int)($e['i'] ?? -1), 'effectId' => (int)$e['effectId'], 'min' => (int)($e['min'] ?? 0), 'max' => (int)($e['max'] ?? 0),
                'value' => isset($e['value']) && $e['value'] !== '' ? (int)$e['value'] : null, 'text' => $e['text'] ?? null,
                'visible' => !isset($e['visible']) || (bool)$e['visible'],
            ], $in['effects'] ?? []);
            $before = snapshot($table, $id);
            $r = enqueue('item_effects_set', $id . ';' . json_encode($effects));
            if (empty($r['ok']))
                fail(!empty($r['timeout']) ? 'Le serveur de jeu ne répond pas.' : ($r['result'] ?: 'Échec'));
            log_change('Effets d\'objet modifiés', $table, $id, $before, "objet $id");
            json_out(['ok' => true, 'reload' => $r]);

        /* ============================================================ sorts */
        case 'spells':
            $s = trim((string)($_GET['q'] ?? ''));
            $breed = (int)($_GET['breed'] ?? 0);
            if ($breed) {
                $rows = q('SELECT st.Id, l.French Name, bs.ObtainLevel FROM breeds_spells bs JOIN spells_templates st ON st.Id = bs.Spell
                    LEFT JOIN langs l ON l.Id = st.NameId WHERE bs.BreedId = ? ORDER BY bs.ObtainLevel, l.French', [$breed])->fetchAll();
                json_out(['spells' => $rows]);
            }
            if ($s === '')
                json_out(['spells' => []]);
            $rows = ctype_digit($s)
                ? q('SELECT st.Id, l.French Name FROM spells_templates st LEFT JOIN langs l ON l.Id = st.NameId WHERE st.Id = ?', [(int)$s])->fetchAll()
                : q('SELECT st.Id, l.French Name FROM spells_templates st JOIN langs l ON l.Id = st.NameId WHERE l.French LIKE ? ORDER BY l.French LIMIT 150', ["%$s%"])->fetchAll();
            json_out(['spells' => $rows]);

        case 'breeds':
            json_out(['breeds' => array_map(fn($k, $v) => ['Id' => $k, 'Name' => $v], array_keys(breed_names()), breed_names())]);

        case 'spell':
            $id = (int)($_GET['id'] ?? 0);
            $t = q('SELECT * FROM spells_templates WHERE Id = ?', [$id])->fetch();
            if (!$t)
                fail('Sort introuvable');
            $texts = lang_map([$t['NameId'], $t['DescriptionId']]);
            $levels = array_map('no_binary', q('SELECT * FROM spells_levels WHERE SpellId = ? ORDER BY Id', [$id])->fetchAll());
            $effectIds = [];
            $all = $levels ? enqueue('spell_all_effects', implode(',', array_column($levels, 'Id'))) : null;
            $allFx = !empty($all['ok']) ? json_decode($all['result'], true) : [];
            foreach ($levels as &$lv) {
                $lv['fx'] = $allFx[$lv['Id']] ?? null;
                if ($lv['fx'])
                    foreach (array_merge($lv['fx']['effects'], $lv['fx']['critical']) as $e)
                        $effectIds[] = $e['effectId'];
            }
            unset($lv);
            $users = q('SELECT COUNT(*) FROM monsters_spells WHERE SpellId = ?', [$id])->fetchColumn();
            $breeds = q('SELECT BreedId, ObtainLevel FROM breeds_spells WHERE Spell = ?', [$id])->fetchAll();
            json_out(['spell' => $t, 'name' => $texts[(int)$t['NameId']] ?? '', 'description' => $texts[(int)$t['DescriptionId']] ?? '',
                'levels' => $levels, 'effectTexts' => effect_texts(array_values(array_unique($effectIds))), 'monsters' => (int)$users,
                'breeds' => array_map(fn($b) => ['breed' => breed_names()[(int)$b['BreedId']] ?? $b['BreedId'], 'level' => $b['ObtainLevel']], $breeds)]);

        case 'spell_level_save':
            $lid = (int)($in['id'] ?? 0);
            $row = q('SELECT SpellId FROM spells_levels WHERE Id = ?', [$lid])->fetch();
            if (!$row)
                fail('Niveau introuvable');
            $set = [];
            $vals = [];
            foreach (SPELL_LEVEL_FIELDS as $f)
                if (array_key_exists($f, $in['fields'] ?? []) && is_numeric($in['fields'][$f])) {
                    $set[] = "`$f` = ?";
                    $vals[] = (int)$in['fields'][$f];
                }
            if (!$set)
                fail('Rien à enregistrer');
            $before = snapshot('spells_levels', $lid);
            q('UPDATE spells_levels SET ' . implode(', ', $set) . ' WHERE Id = ?', array_merge($vals, [$lid]));
            log_change('Niveau de sort modifié', 'spells_levels', $lid, $before, "sort {$row['SpellId']} niveau $lid");
            json_out(['ok' => true, 'reload' => enqueue('reload_spell', (string)$row['SpellId'])]);

        case 'spell_effects_save':
            $lid = (int)($in['id'] ?? 0);
            $row = q('SELECT SpellId FROM spells_levels WHERE Id = ?', [$lid])->fetch();
            if (!$row)
                fail('Niveau introuvable');
            $keys = ['effectId', 'min', 'max', 'value', 'duration', 'delay', 'random', 'group', 'zoneShape', 'zoneSize', 'zoneMinSize', 'zoneEfficiency', 'zoneMaxEfficiency', 'order'];
            $effects = array_map(function ($e) use ($keys) {
                $o = ['i' => (int)($e['i'] ?? -1)];
                foreach ($keys as $k)
                    $o[$k] = (int)($e[$k] ?? 0);
                $o['targetMask'] = (string)($e['targetMask'] ?? '');
                $o['triggers'] = (string)($e['triggers'] ?? 'I');
                $o['hidden'] = !empty($e['hidden']);
                return $o;
            }, $in['effects'] ?? []);
            $crit = !empty($in['critical']) ? 1 : 0;
            $before = snapshot('spells_levels', $lid);
            $r = enqueue('spell_effects_set', "$lid;$crit;" . json_encode($effects));
            if (empty($r['ok']))
                fail(!empty($r['timeout']) ? 'Le serveur de jeu ne répond pas.' : ($r['result'] ?: 'Échec'));
            log_change($crit ? 'Effets critiques de sort modifiés' : 'Effets de sort modifiés', 'spells_levels', $lid, $before, "sort {$row['SpellId']} niveau $lid");
            json_out(['ok' => true, 'reload' => $r]);
    }
    return false;
}
