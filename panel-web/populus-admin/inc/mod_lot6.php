<?php
/*
 * Panel admin Populus 2.51 : panoplies (objets et bonus par nombre d'objets portés) et vue d'une zone (toutes ses maps avec leur image).
 */
if (!defined('POPULUS_ADMIN'))
    exit;

function handle_lot6($action, $me)
{
    $in = $GLOBALS['in'];
    switch ($action) {

        case 'sets':
            $s = trim((string)($_GET['q'] ?? ''));
            $sql = 'SELECT s.Id, l.French Name, s.ItemsCSV FROM items_sets s LEFT JOIN langs l ON l.Id = s.NameId ';
            $rows = ctype_digit($s) ? q($sql . 'WHERE s.Id = ?', [(int)$s])->fetchAll()
                : q($sql . 'WHERE l.French LIKE ? ORDER BY l.French LIMIT 150', ["%$s%"])->fetchAll();
            foreach ($rows as &$r)
                $r['Count'] = $r['ItemsCSV'] === '' || $r['ItemsCSV'] === null ? 0 : count(explode(',', $r['ItemsCSV']));
            unset($r);
            json_out(['sets' => $rows]);

        case 'set':
            $id = (int)($_GET['id'] ?? 0);
            $set = q('SELECT s.Id, l.French Name, s.ItemsCSV, s.BonusIsSecret FROM items_sets s LEFT JOIN langs l ON l.Id = s.NameId WHERE s.Id = ?', [$id])->fetch();
            if (!$set)
                fail('Panoplie introuvable');
            $ids = array_values(array_filter(array_map('intval', explode(',', (string)$set['ItemsCSV']))));
            $items = [];
            if ($ids) {
                $ph = implode(',', array_fill(0, count($ids), '?'));
                $rows = q("SELECT i.Id, i.Level, l.French Name FROM items_templates i LEFT JOIN langs l ON l.Id = i.NameId WHERE i.Id IN ($ph)
                    UNION SELECT w.Id, w.Level, l.French Name FROM items_templates_weapons w LEFT JOIN langs l ON l.Id = w.NameId WHERE w.Id IN ($ph)", array_merge($ids, $ids))->fetchAll();
                $byId = array_column($rows, null, 'Id');
                foreach ($ids as $i)
                    $items[] = $byId[$i] ?? ['Id' => $i, 'Level' => null, 'Name' => null];
            }
            $r = enqueue('set_effects', (string)$id);
            $data = !empty($r['ok']) ? json_decode($r['result'], true) : null;
            $fxIds = [];
            foreach ($data['effects'] ?? [] as $step)
                foreach ($step as $e)
                    $fxIds[] = (int)$e['effectId'];
            json_out(['set' => $set, 'items' => $items, 'effects' => $data['effects'] ?? null,
                'effectsError' => $data === null ? ($r['result'] ?? 'serveur de jeu injoignable') : null, 'effectTexts' => effect_texts(array_unique($fxIds))]);

        case 'set_effects_save':
            $id = (int)($in['id'] ?? 0);
            if (!q('SELECT 1 FROM items_sets WHERE Id = ?', [$id])->fetch())
                fail('Panoplie introuvable');
            $steps = [];
            foreach (array_slice((array)($in['effects'] ?? []), 0, 16) as $step)
                $steps[] = array_values(array_map(fn($e) => ['effectId' => (int)$e['effectId'], 'value' => (int)($e['value'] ?? 0)],
                    array_filter((array)$step, fn($e) => (int)($e['effectId'] ?? 0) > 0)));
            $before = snapshot('items_sets', $id);
            $r = enqueue('set_effects_set', $id . ';' . json_encode($steps));
            if (empty($r['ok']))
                fail(!empty($r['timeout']) ? 'Le serveur de jeu ne répond pas.' : ($r['result'] ?: 'Échec'));
            log_change('Bonus de panoplie modifiés', 'items_sets', $id, $before, "panoplie $id");
            json_out(['ok' => true]);

        case 'subarea_maps':
            $sub = (int)($_GET['id'] ?? 0);
            $name = q('SELECT NameId FROM world_subareas WHERE Id = ?', [$sub])->fetchColumn();
            $rows = q('SELECT p.Id, p.PosX, p.PosY, p.Outdoor, p.WorldMap FROM world_maps_positions p JOIN world_maps m ON m.Id = p.Id
                WHERE p.SubAreaId = ? ORDER BY p.WorldMap, p.PosY, p.PosX LIMIT 400', [$sub])->fetchAll();
            if (!$rows)
                fail('Aucune map dans cette zone.');
            $mobs = q('SELECT MapId, COUNT(*) n FROM monsters_spawns WHERE MapId IN (' . implode(',', array_map(fn($r) => (int)$r['Id'], $rows)) . ') GROUP BY MapId')->fetchAll(PDO::FETCH_KEY_PAIR);
            $npcs = q('SELECT MapId, COUNT(*) n FROM npcs_spawns WHERE MapId IN (' . implode(',', array_map(fn($r) => (int)$r['Id'], $rows)) . ') GROUP BY MapId')->fetchAll(PDO::FETCH_KEY_PAIR);
            $maps = [];
            foreach ($rows as $r) {
                $img = map_image((int)$r['Id']);   // demande au passage le rendu des images manquantes
                $maps[] = ['id' => (int)$r['Id'], 'x' => (int)$r['PosX'], 'y' => (int)$r['PosY'], 'outdoor' => (int)$r['Outdoor'], 'world' => (int)$r['WorldMap'],
                    'url' => $img['url'], 'status' => $img['status'], 'mobs' => (int)($mobs[$r['Id']] ?? 0), 'npcs' => (int)($npcs[$r['Id']] ?? 0)];
            }
            json_out(['subArea' => $sub, 'name' => $name ? (lang_map([$name])[(int)$name] ?? '') : '', 'maps' => $maps]);
    }
    return false;
}
