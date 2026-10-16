<?php
/*
 * Panel admin Populus 2.51 : onglets Accueil, Joueurs, Console, Historique (annulable) et Journal.
 * Appelé par api.php pour les actions qu'il ne traite pas lui-même.
 */
if (!defined('POPULUS_ADMIN'))
    exit;

/** Réponse JSON du serveur de jeu, ou erreur lisible. */
function server_json($command, $args = '')
{
    $r = enqueue($command, $args);
    if (!empty($r['timeout']))
        fail('Le serveur de jeu ne répond pas (est-il lancé ?).');
    if (!$r['ok'])
        fail($r['result'] ?: 'Erreur du serveur de jeu');
    return json_decode($r['result'], true);
}

function breed_names()
{
    static $names = null;
    if ($names === null) {
        $rows = q('SELECT Id, ShortNameId FROM breeds')->fetchAll();
        $texts = lang_map(array_column($rows, 'ShortNameId'));
        $names = [];
        foreach ($rows as $r)
            $names[(int)$r['Id']] = $texts[(int)$r['ShortNameId']] ?? ('classe ' . $r['Id']);
    }
    return $names;
}

/** Niveau d'après l'expérience (table experiences). */
function level_for_xp($xp)
{
    static $levels = null;
    if ($levels === null)
        $levels = q('SELECT Level, CharacterExp FROM experiences WHERE CharacterExp IS NOT NULL ORDER BY Level')->fetchAll();
    $lvl = 1;
    foreach ($levels as $l)
        if ((float)$xp >= (float)$l['CharacterExp'])
            $lvl = (int)$l['Level'];
    return $lvl;
}

function handle_module($action, $me)
{
    global $CFG;
    switch ($action) {

        /* ------------------------------------------------------------ accueil */
        case 'state':
            $s = server_json('state');
            if ($s['me'] && $s['me']['mapId']) {
                $info = q("SELECT s.NameId FROM world_maps_positions p LEFT JOIN world_subareas s ON s.Id = p.SubAreaId WHERE p.Id = ?", [$s['me']['mapId']])->fetch();
                $s['me']['zone'] = $info ? (lang_map([$info['NameId']])[(int)$info['NameId']] ?? '') : '';
            }
            $s['changes24h'] = (int)q('SELECT COUNT(*) FROM admin_panel_log WHERE At > NOW() - INTERVAL 1 DAY AND TableName IS NOT NULL')->fetchColumn();
            json_out($s);

        /* ------------------------------------------------------------ joueurs */
        case 'players':
            $search = trim((string)($_GET['q'] ?? ''));
            $online = server_json('players');
            $byId = [];
            foreach ($online as $p)
                $byId[$p['id']] = $p;
            $breeds = breed_names();
            if ($search === '') {
                $list = array_map(fn($p) => $p + ['online' => true, 'breedName' => $breeds[(int)$p['breed']] ?? ''], $online);
                usort($list, fn($a, $b) => strcasecmp($a['name'], $b['name']));
                json_out(['players' => $list]);
            }
            $authDb = preg_replace('/\W/', '', $CFG['auth_db']);
            $rows = q("SELECT c.Id, c.Name, c.Breed, c.Experience, c.MapId, c.Kamas, c.LastUsage, a.Login, a.Id AccountId
                FROM characters c LEFT JOIN `$authDb`.accounts a ON a.Id = c.AccountId
                WHERE c.Name LIKE ? OR a.Login LIKE ? OR a.Nickname LIKE ? ORDER BY c.LastUsage DESC LIMIT 100", ["%$search%", "%$search%", "%$search%"])->fetchAll();
            $list = [];
            foreach ($rows as $r) {
                $o = $byId[(int)$r['Id']] ?? null;
                $list[] = $o ? $o + ['online' => true, 'breedName' => $breeds[(int)$r['Breed']] ?? ''] : [
                    'id' => (int)$r['Id'], 'name' => $r['Name'], 'level' => level_for_xp($r['Experience']),
                    'breedName' => $breeds[(int)$r['Breed']] ?? '', 'account' => $r['Login'], 'accountId' => (int)$r['AccountId'],
                    'mapId' => (int)$r['MapId'], 'kamas' => (int)$r['Kamas'], 'lastUsage' => $r['LastUsage'], 'online' => false,
                ];
            }
            json_out(['players' => $list]);

        /* ------------------------------------------------------------ console (commandes du serveur avec tes droits) */
        case 'cmd':
            $text = trim((string)($GLOBALS['in']['cmd'] ?? ''));
            if ($text === '')
                fail('Commande vide');
            $r = enqueue('cmd', $text);
            if (!empty($r['timeout']))
                fail('Le serveur de jeu ne répond pas (ou la commande est trop longue).');
            q('INSERT INTO admin_panel_log (At, CharacterName, Action, Details) VALUES (NOW(), ?, ?, ?)', [$me['name'], 'Commande', '.' . ltrim($text, '.')]);
            json_out(['ok' => $r['ok'], 'out' => $r['result']]);

        /* ------------------------------------------------------------ historique annulable */
        case 'history':
            $rows = q('SELECT Id, At, CharacterName, Action, Details, TableName, RowId, Undone, RowBefore IS NULL AS WasInsert, RowAfter IS NULL AS WasDelete
                FROM admin_panel_log WHERE TableName IS NOT NULL ORDER BY Id DESC LIMIT 200')->fetchAll();
            json_out(['history' => $rows]);

        case 'history_entry':
            $row = q('SELECT * FROM admin_panel_log WHERE Id = ?', [(int)($_GET['id'] ?? 0)])->fetch();
            if (!$row)
                fail('Entrée introuvable');
            json_out(['entry' => $row]);

        case 'history_undo':
            $id = (int)($GLOBALS['in']['id'] ?? 0);
            $row = q('SELECT * FROM admin_panel_log WHERE Id = ? AND TableName IS NOT NULL', [$id])->fetch();
            if (!$row)
                fail('Entrée introuvable');
            if ($row['Undone'])
                fail('Déjà annulé.');
            $table = $row['TableName'];
            if (!isset(UNDO_TABLES[$table]))
                fail('Table non annulable');
            // la ligne a-t-elle encore bougé depuis ? on prévient sans bloquer l'annulation la plus récente
            $newer = q('SELECT COUNT(*) FROM admin_panel_log WHERE TableName = ? AND RowId = ? AND Id > ? AND Undone = 0', [$table, $row['RowId'], $id])->fetchColumn();
            if ($newer && empty($GLOBALS['in']['force']))
                json_out(['confirm' => "Cette ligne a été modifiée $newer fois depuis. Annuler quand même (les modifications suivantes seront écrasées) ?"]);
            $current = snapshot($table, (int)$row['RowId']);
            restore_row($table, (int)$row['RowId'], $row['RowBefore'] === null ? null : json_decode($row['RowBefore'], true));
            q('UPDATE admin_panel_log SET Undone = 1 WHERE Id = ?', [$id]);
            log_change('Annulation : ' . $row['Action'], $table, (int)$row['RowId'], $current, 'annule l\'entrée #' . $id);
            $reload = undo_reload($table, (int)$row['RowId'], $row);
            json_out(['ok' => true, 'reload' => $reload]);

        /* ------------------------------------------------------------ journal */
        case 'log':
            json_out(['log' => q('SELECT At, CharacterName, Action, Details FROM admin_panel_log ORDER BY Id DESC LIMIT 200')->fetchAll()]);
    }
    return handle_monsters($action, $me) ?: handle_items_spells($action, $me) ?: handle_maps($action, $me) ?: handle_ops($action, $me) ?: handle_npc2($action, $me) ?: handle_lot6($action, $me);
}

/** Ce qu'il faut recharger en jeu après l'annulation d'une modification. */
function undo_reload($table, $rowId, $logRow)
{
    $kind = UNDO_TABLES[$table];
    if ($kind === 'npcs' || $kind === 'npcs_items')
        return reload_npcs($kind === 'npcs_items');
    if ($kind === 'maps') {
        $data = json_decode($logRow['RowBefore'] ?? 'null', true) ?: json_decode($logRow['RowAfter'] ?? 'null', true) ?: [];
        $mapId = $table === 'world_maps' ? $rowId : ($data['MapId'] ?? 0);
        return $mapId ? enqueue('map_reload', (string)(int)$mapId) : null;
    }
    if ($kind === 'sets')
        return enqueue('reload_set', (string)$rowId);
    if ($kind === 'items')
        return enqueue('reload_item', (string)$rowId);
    if ($kind === 'spells') {
        $spell = q('SELECT SpellId FROM spells_levels WHERE Id = ?', [$rowId])->fetchColumn();
        return $spell ? enqueue('reload_spell', (string)$spell) : null;
    }
    if ($kind !== 'monsters')
        return null;
    $data = json_decode($logRow['RowBefore'] ?? 'null', true) ?: json_decode($logRow['RowAfter'] ?? 'null', true) ?: [];
    if ($table === 'monsters_spawns')
        return enqueue('mob_zone', $rowId . ';1');
    $mob = $data['MonsterId'] ?? $data['MonsterOwnerId'] ?? null;
    if ($table === 'monsters_templates')
        $mob = $rowId;
    if ($table === 'monsters_spells' && isset($data['MonsterGradeId']))
        $mob = q('SELECT MonsterId FROM monsters_grades WHERE Id = ?', [$data['MonsterGradeId']])->fetchColumn();
    return $mob ? enqueue('reload_monster', (string)(int)$mob) : null;
}
