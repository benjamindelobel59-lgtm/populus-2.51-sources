<?php
/*
 * Panel admin Populus 2.51 : API JSON (PNJ, placements, apparence, actions, dialogues, boutiques).
 * Chaque modification est écrite en base puis le serveur de jeu recharge les PNJ (admin_panel_queue).
 */
define('POPULUS_ADMIN', true);
require __DIR__ . '/inc/lib.php';
require __DIR__ . '/inc/modules.php';
require __DIR__ . '/inc/mod_monsters.php';
require __DIR__ . '/inc/mod_items_spells.php';
require __DIR__ . '/inc/mod_maps.php';
require __DIR__ . '/inc/mod_ops.php';
require __DIR__ . '/inc/mod_npc2.php';
require __DIR__ . '/inc/mod_lot6.php';

$me = admin();
if (!$me)
    fail('Session expirée : tape .admin en jeu pour rouvrir le panel.', 401);

$action = $_GET['a'] ?? '';
$in = [];
if ($_SERVER['REQUEST_METHOD'] === 'POST') {
    if (($_SERVER['HTTP_X_CSRF'] ?? '') !== $me['csrf'])
        fail('Jeton de sécurité invalide, recharge la page.', 403);
    $in = json_decode(file_get_contents('php://input'), true) ?: [];
}

function arg($name, $default = null)
{
    global $in;
    return array_key_exists($name, $in) ? $in[$name] : ($_GET[$name] ?? $default);
}

function int_arg($name, $default = 0)
{
    $v = arg($name, $default);
    return is_numeric($v) ? (int)$v : $default;
}

function str_or_null($v)
{
    $v = trim((string)$v);
    return $v === '' ? null : $v;
}

function npc_exists($id)
{
    return (bool)q('SELECT 1 FROM npcs_templates WHERE Id = ?', [$id])->fetch();
}

function map_infos(array $mapIds)
{
    $mapIds = array_values(array_unique(array_map('intval', $mapIds)));
    if (!$mapIds)
        return [];
    $in = implode(',', array_fill(0, count($mapIds), '?'));
    $rows = q("SELECT p.Id, p.PosX, p.PosY, p.SubAreaId, s.NameId FROM world_maps_positions p
        LEFT JOIN world_subareas s ON s.Id = p.SubAreaId WHERE p.Id IN ($in)", $mapIds)->fetchAll();
    $names = lang_map(array_column($rows, 'NameId'));
    $out = [];
    foreach ($rows as $r)
        $out[(int)$r['Id']] = ['x' => (int)$r['PosX'], 'y' => (int)$r['PosY'], 'zone' => $names[(int)$r['NameId']] ?? ''];
    $exists = q("SELECT Id FROM world_maps WHERE Id IN ($in)", $mapIds)->fetchAll(PDO::FETCH_COLUMN);
    foreach ($mapIds as $m)
        $out[$m] = ($out[$m] ?? ['x' => null, 'y' => null, 'zone' => '']) + ['exists' => in_array($m, array_map('intval', $exists), true)];
    return $out;
}

switch ($action) {

    /* ------------------------------------------------------------------ session */
    case 'me':
        $last = q("SELECT MAX(DoneAt) d FROM admin_panel_queue WHERE Done = 1 AND Result <> 'serveur redemarre'")->fetch();
        json_out(['name' => $me['name'], 'csrf' => $me['csrf'], 'lastServerAnswer' => $last['d']]);

    /* ------------------------------------------------------------------ liste des PNJ */
    case 'npcs':
        $search = trim((string)arg('q', ''));
        $filter = arg('filter', 'all');
        $where = [];
        $args = [];
        if ($search !== '') {
            if (ctype_digit($search)) {
                $where[] = 't.Id = ?';
                $args[] = (int)$search;
            } else {
                $where[] = '(l.French LIKE ? OR t.Name LIKE ?)';
                $args[] = "%$search%";
                $args[] = "%$search%";
            }
        }
        if ($filter === 'placed')
            $where[] = 'EXISTS (SELECT 1 FROM npcs_spawns s WHERE s.NpcId = t.Id)';
        elseif ($filter === 'mute')
            $where[] = 'EXISTS (SELECT 1 FROM npcs_spawns s WHERE s.NpcId = t.Id) AND NOT EXISTS (SELECT 1 FROM npcs_actions a WHERE a.NpcId = t.Id)';
        elseif ($filter === 'actions')
            $where[] = 'EXISTS (SELECT 1 FROM npcs_actions a WHERE a.NpcId = t.Id)';
        $sql = 'SELECT t.Id, COALESCE(NULLIF(l.French, \'\'), t.Name) Name,
                (SELECT COUNT(*) FROM npcs_spawns s WHERE s.NpcId = t.Id) Spawns,
                (SELECT GROUP_CONCAT(DISTINCT a.Type) FROM npcs_actions a WHERE a.NpcId = t.Id) Actions
            FROM npcs_templates t LEFT JOIN langs l ON l.Id = t.NameId'
            . ($where ? ' WHERE ' . implode(' AND ', $where) : '')
            . ' ORDER BY Name, t.Id LIMIT 300';
        json_out(['npcs' => q($sql, $args)->fetchAll()]);

    /* ------------------------------------------------------------------ fiche d'un PNJ */
    case 'npc':
        $id = int_arg('id');
        $t = q('SELECT * FROM npcs_templates WHERE Id = ?', [$id])->fetch();
        if (!$t)
            fail('PNJ introuvable');
        $messages = parse_pairs($t['DialogMessagesIdCSV']);
        $replies = parse_pairs($t['DialogRepliesIdCSV']);
        $texts = lang_map(array_merge([$t['NameId']], array_column($messages, 1), array_column($replies, 1)));

        $spawns = q('SELECT Id, MapId, CellId, Direction FROM npcs_spawns WHERE NpcId = ? ORDER BY Id', [$id])->fetchAll();
        $maps = map_infos(array_column($spawns, 'MapId'));
        foreach ($spawns as &$s)
            $s['map'] = $maps[(int)$s['MapId']] ?? null;
        unset($s);

        $actions = q('SELECT Id, Type, `Condition`, Priority, Parameter0, Parameter1, Parameter2, Parameter3, Parameter4, AdditionalParameters
            FROM npcs_actions WHERE NpcId = ? ORDER BY Priority, Id', [$id])->fetchAll();
        $shopItems = [];
        foreach ($actions as $act) {
            if ($act['Type'] !== 'Shop')
                continue;
            $items = q('SELECT n.Id, n.ItemId, n.CustomPrice, n.BuyCriterion, n.MaxStats, i.NameId, i.Level, i.Price
                FROM npcs_items n LEFT JOIN items_templates i ON i.Id = n.ItemId WHERE n.NpcShopId = ? ORDER BY n.Id', [$act['Id']])->fetchAll();
            $names = lang_map(array_column($items, 'NameId'));
            foreach ($items as &$it)
                $it['Name'] = $names[(int)$it['NameId']] ?? ('objet ' . $it['ItemId']);
            unset($it);
            $shopItems[$act['Id']] = $items;
        }

        // réponses rattachées aux messages de ce PNJ (et aux messages utilisés par ses actions Talk)
        $extra = npc_extra_messages($id);
        $msgIds = array_merge(array_column($messages, 0), array_column($extra, 'id'));
        foreach ($actions as $act)
            if ($act['Type'] === 'Talk' && is_numeric($act['Parameter0']))
                $msgIds[] = (int)$act['Parameter0'];
        $msgIds = array_values(array_unique($msgIds));
        $dialog = [];
        if ($msgIds) {
            $inq = implode(',', array_fill(0, count($msgIds), '?'));
            $dialog = q("SELECT Id, Type, ReplyId, MessageId, Criteria, Parameter0, Parameter1, Parameter2, Parameter3, Parameter4, AdditionalParameters
                FROM npcs_replies WHERE MessageId IN ($inq) ORDER BY MessageId, Id", $msgIds)->fetchAll();
        }

        json_out([
            'id' => (int)$t['Id'],
            'name' => $texts[(int)$t['NameId']] ?? $t['Name'],
            'look' => $t['LookAsString'],
            'gender' => (int)$t['Gender'],
            'messages' => array_map(fn($m) => ['id' => $m[0], 'textId' => $m[1], 'text' => $texts[$m[1]] ?? null], $messages),
            'replies' => array_map(fn($r) => ['id' => $r[0], 'textId' => $r[1], 'text' => $texts[$r[1]] ?? null], $replies),
            'spawns' => $spawns,
            'actions' => $actions,
            'shopItems' => $shopItems,
            'dialog' => array_map(fn($r) => $r + ['ReplyText' => reply_text_for((int)$r['ReplyId'])], $dialog),
            'extraMessages' => $extra,
        ]);

    /* ------------------------------------------------------------------ apparence */
    case 'look_save':
        $id = int_arg('id');
        $look = trim((string)arg('look', ''));
        if (!npc_exists($id) || !preg_match('/^\{[0-9a-fA-F#|,=@{}\-]*\}$/', $look))
            fail('Apparence invalide : elle doit ressembler à {1|1102||140}');
        $before = snapshot('npcs_templates', $id);
        q('UPDATE npcs_templates SET LookAsString = ? WHERE Id = ?', [$look, $id]);
        log_change('Apparence PNJ', 'npcs_templates', $id, $before, "PNJ $id");
        json_out(['ok' => true, 'reload' => reload_npcs()]);

    case 'preview_look':
        $id = int_arg('id');
        $look = trim((string)arg('look', ''));
        json_out(enqueue('preview_look', $id . ';' . $look));

    /* ------------------------------------------------------------------ placements */
    case 'position':
        $r = enqueue('position');
        if (!$r['ok'])
            fail($r['timeout'] ?? false ? 'Le serveur de jeu ne répond pas.' : ($r['result'] ?: 'Position inconnue'));
        [$map, $cell, $dir] = array_map('intval', explode(';', $r['result']));
        json_out(['mapId' => $map, 'cellId' => $cell, 'direction' => $dir, 'map' => map_infos([$map])[$map] ?? null]);

    case 'spawn_save':
        $npc = int_arg('npcId');
        $spawnId = int_arg('id');
        $map = int_arg('mapId');
        $cell = int_arg('cellId', -1);
        $dir = int_arg('direction', 1);
        if (!npc_exists($npc))
            fail('PNJ introuvable');
        if (!q('SELECT 1 FROM world_maps WHERE Id = ?', [$map])->fetch())
            fail("La map $map n'existe pas.");
        if ($cell < 0 || $cell > 559)
            fail('Case invalide (0 à 559).');
        if ($dir < 0 || $dir > 7)
            fail('Direction invalide (0 à 7).');
        $taken = q('SELECT Id FROM npcs_spawns WHERE MapId = ? AND CellId = ? AND Id <> ?', [$map, $cell, $spawnId])->fetch();
        if ($taken)
            fail('Un autre PNJ est déjà sur cette case.');
        $name = q("SELECT COALESCE(NULLIF(l.French,''), t.Name) FROM npcs_templates t LEFT JOIN langs l ON l.Id = t.NameId WHERE t.Id = ?", [$npc])->fetchColumn();
        $before = $spawnId ? snapshot('npcs_spawns', $spawnId) : null;
        if ($spawnId) {
            q('UPDATE npcs_spawns SET MapId = ?, CellId = ?, Direction = ? WHERE Id = ? AND NpcId = ?', [$map, $cell, $dir, $spawnId, $npc]);
        } else {
            q('INSERT INTO npcs_spawns (NpcId, MapId, CellId, Direction, Npc) VALUES (?, ?, ?, ?, ?)', [$npc, $map, $cell, $dir, mb_substr((string)$name, 0, 255)]);
            $spawnId = (int)db()->lastInsertId();
        }
        log_change($before ? 'Placement PNJ modifié' : 'PNJ placé', 'npcs_spawns', $spawnId, $before, "$name sur la map $map case $cell");
        json_out(['ok' => true, 'id' => $spawnId, 'reload' => reload_npcs()]);

    case 'spawn_delete':
        $spawnId = int_arg('id');
        $row = q('SELECT * FROM npcs_spawns WHERE Id = ?', [$spawnId])->fetch();
        if (!$row)
            fail('Placement introuvable');
        $before = snapshot('npcs_spawns', $spawnId);
        q('DELETE FROM npcs_spawns WHERE Id = ?', [$spawnId]);
        log_change('Placement PNJ retiré', 'npcs_spawns', $spawnId, $before, "PNJ {$row['NpcId']} map {$row['MapId']}");
        json_out(['ok' => true, 'reload' => reload_npcs()]);

    case 'teleport':
        json_out(enqueue('teleport', int_arg('mapId') . ';' . int_arg('cellId', -1)));

    case 'map_info':
        $m = int_arg('mapId');
        json_out(['map' => map_infos([$m])[$m] ?? null]);

    /* ------------------------------------------------------------------ actions (clic sur le PNJ) */
    case 'action_save':
        $npc = int_arg('npcId');
        $id = int_arg('id');
        $type = (string)arg('type', '');
        if (!npc_exists($npc))
            fail('PNJ introuvable');
        if (!in_array($type, ['Talk', 'Shop', 'BidBuy', 'BidSell', 'Exchange'], true))
            fail('Type d\'action inconnu');
        $p = [];
        for ($i = 0; $i <= 4; $i++)
            $p[$i] = str_or_null(arg('p' . $i, ''));
        if ($type === 'Talk' && !is_numeric($p[0]))
            fail('Choisis le message de départ du dialogue.');
        $vals = [$type, str_or_null(arg('condition', '')), int_arg('priority', 0), $p[0], $p[1], $p[2], $p[3], $p[4]];
        if ($id) {
            $old = q('SELECT * FROM npcs_actions WHERE Id = ? AND NpcId = ?', [$id, $npc])->fetch();
            if (!$old)
                fail('Action introuvable');
            $before = snapshot('npcs_actions', $id);
            q('UPDATE npcs_actions SET Type = ?, `Condition` = ?, Priority = ?, Parameter0 = ?, Parameter1 = ?, Parameter2 = ?, Parameter3 = ?, Parameter4 = ? WHERE Id = ?', array_merge($vals, [$id]));
            log_change('Action PNJ modifiée', 'npcs_actions', $id, $before, "PNJ $npc : $type");
        } else {
            q('INSERT INTO npcs_actions (NpcId, Type, `Condition`, Priority, Parameter0, Parameter1, Parameter2, Parameter3, Parameter4) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)', array_merge([$npc], $vals));
            $id = (int)db()->lastInsertId();
            log_change('Action PNJ ajoutée', 'npcs_actions', $id, null, "PNJ $npc : $type");
        }
        json_out(['ok' => true, 'id' => $id, 'reload' => reload_npcs($type === 'Shop')]);

    case 'action_delete':
        $id = int_arg('id');
        $row = q('SELECT * FROM npcs_actions WHERE Id = ?', [$id])->fetch();
        if (!$row)
            fail('Action introuvable');
        if ($row['Type'] === 'Shop')
            foreach (q('SELECT Id FROM npcs_items WHERE NpcShopId = ?', [$id])->fetchAll(PDO::FETCH_COLUMN) as $itemRow) {
                $b = snapshot('npcs_items', $itemRow);
                q('DELETE FROM npcs_items WHERE Id = ?', [$itemRow]);
                log_change('Article de boutique retiré', 'npcs_items', $itemRow, $b, "boutique $id");
            }
        $before = snapshot('npcs_actions', $id);
        q('DELETE FROM npcs_actions WHERE Id = ?', [$id]);
        log_change('Action PNJ supprimée', 'npcs_actions', $id, $before, "PNJ {$row['NpcId']} : {$row['Type']}");
        json_out(['ok' => true, 'reload' => reload_npcs($row['Type'] === 'Shop')]);

    /* ------------------------------------------------------------------ boutiques */
    case 'item_search':
        $s = trim((string)arg('q', ''));
        if ($s === '')
            json_out(['items' => []]);
        if (ctype_digit($s))
            $rows = q('SELECT i.Id, i.Level, i.Price, l.French Name FROM items_templates i LEFT JOIN langs l ON l.Id = i.NameId WHERE i.Id = ?', [(int)$s])->fetchAll();
        else
            $rows = q('SELECT i.Id, i.Level, i.Price, l.French Name FROM items_templates i JOIN langs l ON l.Id = i.NameId WHERE l.French LIKE ? ORDER BY i.Level, l.French LIMIT 40', ["%$s%"])->fetchAll();
        json_out(['items' => $rows]);

    case 'shop_item_add':
        $shop = int_arg('shopId');
        $item = int_arg('itemId');
        if (!q("SELECT 1 FROM npcs_actions WHERE Id = ? AND Type = 'Shop'", [$shop])->fetch())
            fail('Boutique introuvable (enregistre d\'abord l\'action Boutique).');
        if (!q('SELECT 1 FROM items_templates WHERE Id = ?', [$item])->fetch())
            fail("L'objet $item n'existe pas.");
        $price = str_or_null(arg('price', ''));
        q('INSERT INTO npcs_items (NpcShopId, ItemId, CustomPrice, BuyCriterion, MaxStats) VALUES (?, ?, ?, ?, ?)',
            [$shop, $item, $price === null ? null : (int)$price, str_or_null(arg('criterion', '')), arg('maxStats') ? 1 : 0]);
        log_change('Article de boutique ajouté', 'npcs_items', (int)db()->lastInsertId(), null, "objet $item dans la boutique $shop");
        json_out(['ok' => true, 'reload' => reload_npcs(true)]);

    case 'shop_item_delete':
        $id = int_arg('id');
        $row = q('SELECT * FROM npcs_items WHERE Id = ?', [$id])->fetch();
        if (!$row)
            fail('Article introuvable');
        $before = snapshot('npcs_items', $id);
        q('DELETE FROM npcs_items WHERE Id = ?', [$id]);
        log_change('Article de boutique retiré', 'npcs_items', $id, $before, "objet {$row['ItemId']}");
        json_out(['ok' => true, 'reload' => reload_npcs(true)]);

    /* ------------------------------------------------------------------ dialogues (réponses) */
    case 'reply_save':
        $id = int_arg('id');
        $type = (string)arg('type', '');
        if (!preg_match('/^[A-Za-z0-9]{1,32}$/', $type))
            fail('Type de réponse invalide');
        $replyId = int_arg('replyId');
        $messageId = int_arg('messageId');
        if (!$replyId || !$messageId)
            fail('Choisis la question et le texte de la réponse.');
        $p = [];
        for ($i = 0; $i <= 4; $i++)
            $p[$i] = str_or_null(arg('p' . $i, ''));
        if ($type === 'Dialog' && !is_numeric($p[0]))
            fail('Choisis la question suivante.');
        if ($type === 'Teleport' && !q('SELECT 1 FROM world_maps WHERE Id = ?', [(int)$p[0]])->fetch())
            fail('La map de téléportation n\'existe pas.');
        $vals = [$type, $replyId, $messageId, str_or_null(arg('criteria', '')), $p[0], $p[1], $p[2], $p[3], $p[4], str_or_null(arg('additional', ''))];
        if ($id) {
            $old = q('SELECT * FROM npcs_replies WHERE Id = ?', [$id])->fetch();
            if (!$old)
                fail('Réponse introuvable');
            $before = snapshot('npcs_replies', $id);
            q('UPDATE npcs_replies SET Type = ?, ReplyId = ?, MessageId = ?, Criteria = ?, Parameter0 = ?, Parameter1 = ?, Parameter2 = ?, Parameter3 = ?, Parameter4 = ?, AdditionalParameters = ? WHERE Id = ?', array_merge($vals, [$id]));
            log_change('Réponse de dialogue modifiée', 'npcs_replies', $id, $before, "question $messageId : $type");
        } else {
            q('INSERT INTO npcs_replies (Type, ReplyId, MessageId, Criteria, Parameter0, Parameter1, Parameter2, Parameter3, Parameter4, AdditionalParameters) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)', $vals);
            $id = (int)db()->lastInsertId();
            log_change('Réponse de dialogue ajoutée', 'npcs_replies', $id, null, "question $messageId : $type");
        }
        json_out(['ok' => true, 'id' => $id, 'reload' => reload_npcs()]);

    case 'reply_delete':
        $id = int_arg('id');
        $row = q('SELECT * FROM npcs_replies WHERE Id = ?', [$id])->fetch();
        if (!$row)
            fail('Réponse introuvable');
        $before = snapshot('npcs_replies', $id);
        q('DELETE FROM npcs_replies WHERE Id = ?', [$id]);
        log_change('Réponse de dialogue supprimée', 'npcs_replies', $id, $before, "question {$row['MessageId']} : {$row['Type']}");
        json_out(['ok' => true, 'reload' => reload_npcs()]);

    /* ------------------------------------------------------------------ divers */
    case 'reload':
        json_out(reload_npcs(true));

    default:
        if (!handle_module($action, $me))
            fail('Action inconnue');
}
