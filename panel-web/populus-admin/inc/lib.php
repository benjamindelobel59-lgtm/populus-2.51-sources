<?php
/*
 * Panel admin Populus 2.51 : base de données, session (ouverte par le jeton de la commande .admin),
 * file de demandes vers le serveur de jeu (admin_panel_queue) et journal des modifications.
 */
if (!defined('POPULUS_ADMIN'))
    exit;

$CFG = require __DIR__ . '/config.php';

ini_set('session.use_strict_mode', '1');
session_name('populus_admin');
session_set_cookie_params(['lifetime' => 0, 'path' => '/', 'httponly' => true, 'samesite' => 'Strict']);
session_start();

function db()
{
    static $pdo = null;
    global $CFG;
    if ($pdo === null) {
        $pdo = new PDO(
            "mysql:host={$CFG['db_host']};port={$CFG['db_port']};dbname={$CFG['db_name']};charset=utf8mb4",
            $CFG['db_user'], $CFG['db_pass'],
            [PDO::ATTR_ERRMODE => PDO::ERRMODE_EXCEPTION, PDO::ATTR_DEFAULT_FETCH_MODE => PDO::FETCH_ASSOC]
        );
        $pdo->exec("CREATE TABLE IF NOT EXISTS admin_panel_log (Id INT AUTO_INCREMENT PRIMARY KEY, At DATETIME NOT NULL,
            CharacterName VARCHAR(64) NOT NULL, Action VARCHAR(64) NOT NULL, Details TEXT NULL) DEFAULT CHARSET=utf8mb4");
        // historique annulable : table + ligne modifiées, état avant / après
        $pdo->exec("ALTER TABLE admin_panel_log ADD COLUMN IF NOT EXISTS TableName VARCHAR(64) NULL, ADD COLUMN IF NOT EXISTS RowId INT NULL,
            ADD COLUMN IF NOT EXISTS RowBefore MEDIUMTEXT NULL, ADD COLUMN IF NOT EXISTS RowAfter MEDIUMTEXT NULL,
            ADD COLUMN IF NOT EXISTS Undone TINYINT NOT NULL DEFAULT 0");
    }
    return $pdo;
}

function q($sql, $args = [])
{
    $st = db()->prepare($sql);
    $st->execute($args);
    return $st;
}

/** Échange le jeton de .admin contre une session. Renvoie true si la session est ouverte. */
function login_with_token($token)
{
    global $CFG;
    if (!preg_match('/^[a-f0-9]{48}$/', $token))
        return false;
    $row = q('SELECT * FROM admin_panel_tokens WHERE Token = ? AND Used = 0 AND ExpiresAt > NOW()', [$token])->fetch();
    if (!$row)
        return false;
    q('UPDATE admin_panel_tokens SET Used = 1 WHERE Token = ?', [$token]);
    session_regenerate_id(true);
    $_SESSION['admin'] = [
        'account' => (int)$row['AccountId'],
        'character' => (int)$row['CharacterId'],
        'name' => $row['CharacterName'],
        'until' => time() + $CFG['session_hours'] * 3600,
        'csrf' => bin2hex(random_bytes(16)),
    ];
    return true;
}

function admin()
{
    $a = $_SESSION['admin'] ?? null;
    if (!$a || $a['until'] < time())
        return null;
    return $a;
}

function json_out($data, $code = 200)
{
    http_response_code($code);
    header('Content-Type: application/json; charset=utf-8');
    header('Cache-Control: no-store');
    echo json_encode($data, JSON_UNESCAPED_UNICODE);
    exit;
}

function fail($message, $code = 400)
{
    json_out(['error' => $message], $code);
}

function log_action($action, $details)
{
    $a = admin();
    q('INSERT INTO admin_panel_log (At, CharacterName, Action, Details) VALUES (NOW(), ?, ?, ?)',
        [$a ? $a['name'] : '?', $action, json_encode($details, JSON_UNESCAPED_UNICODE)]);
}

/** Dépose une demande pour le serveur de jeu. $wait : attend le résultat (ou null si le serveur ne répond pas). */
function enqueue($command, $args = '', $wait = true)
{
    global $CFG;
    $a = admin();
    q('INSERT INTO admin_panel_queue (Command, Args, CharacterId, CreatedAt) VALUES (?, ?, ?, NOW())',
        [$command, $args, $a ? $a['character'] : 0]);
    $id = (int)db()->lastInsertId();
    if (!$wait)
        return ['id' => $id];
    $end = microtime(true) + $CFG['queue_wait_ms'] / 1000;
    while (microtime(true) < $end) {
        usleep(250000);
        $row = q('SELECT Done, Ok, Result FROM admin_panel_queue WHERE Id = ?', [$id])->fetch();
        if ($row && $row['Done'])
            return ['id' => $id, 'ok' => (bool)$row['Ok'], 'result' => $row['Result']];
    }
    return ['id' => $id, 'ok' => false, 'result' => null, 'timeout' => true];
}

/** Rechargement des PNJ en jeu après une modification (une seule demande en attente à la fois). */
function reload_npcs($items = false)
{
    $pending = q("SELECT Id FROM admin_panel_queue WHERE Done = 0 AND Command = 'reload_npcs' AND Args = ?", [$items ? 'items' : ''])->fetch();
    if ($pending)
        return ['ok' => true, 'result' => 'rechargement déjà demandé'];
    return enqueue('reload_npcs', $items ? 'items' : '', true);
}

/** Nom français d'un texte (table langs). */
function lang_map(array $ids)
{
    $ids = array_values(array_unique(array_filter(array_map('intval', $ids))));
    if (!$ids)
        return [];
    $out = [];
    foreach (array_chunk($ids, 900) as $chunk) {
        $in = implode(',', array_fill(0, count($chunk), '?'));
        foreach (q("SELECT Id, COALESCE(NULLIF(French,''), NULLIF(English,''), Spanish) t FROM langs WHERE Id IN ($in)", $chunk) as $r)
            $out[(int)$r['Id']] = $r['t'];
    }
    return $out;
}

/** "id,texte;id,texte" (colonnes DialogMessagesIdCSV / DialogRepliesIdCSV) -> [[id, textId], ...] */
function parse_pairs($csv)
{
    $out = [];
    foreach (explode(';', (string)$csv) as $pair) {
        $p = explode(',', $pair);
        if (count($p) >= 2 && is_numeric($p[0]))
            $out[] = [(int)$p[0], (int)$p[1]];
    }
    return $out;
}

/* ------------------------------------------------------------------ historique annulable */

/** Tables modifiables par le panel (clé primaire Id) et ce qu'il faut recharger en jeu après un changement. */
const UNDO_TABLES = [
    'npcs_templates' => 'npcs', 'npcs_spawns' => 'npcs', 'npcs_actions' => 'npcs', 'npcs_replies' => 'npcs', 'npcs_messages' => 'npcs',
    'npcs_items' => 'npcs_items', 'monsters_grades' => 'monsters', 'monsters_templates' => 'monsters',
    'monsters_drops' => 'monsters', 'monsters_spawns' => 'monsters', 'monsters_spells' => 'monsters', 'items_templates' => 'items', 'items_templates_weapons' => 'items',
    'spells_levels' => 'spells', 'items_sets' => 'sets', 'world_maps' => 'maps', 'world_maps_triggers' => 'maps',
    'interactives_skills' => 'maps', 'interactives_spawns_skills' => 'maps', 'interactives_spawns' => 'maps',   // NEKFEU : téléportations des interactifs
];

function snapshot($table, $id)
{
    if (!isset(UNDO_TABLES[$table]))
        throw new Exception("table $table non suivie");
    $row = q("SELECT * FROM `$table` WHERE Id = ?", [$id])->fetch();
    if (!$row)
        return null;
    foreach ($row as $k => $v)
        if (is_string($v) && !mb_check_encoding($v, 'UTF-8'))
            $row[$k] = ['b64' => base64_encode($v)];   // colonnes binaires (effets, cellules)
    return $row;
}

/** Enregistre une modification : $before = snapshot pris AVANT l'écriture (null pour un ajout). */
function log_change($action, $table, $id, $before, $label = '')
{
    $a = admin();
    $after = snapshot($table, $id);
    q('INSERT INTO admin_panel_log (At, CharacterName, Action, Details, TableName, RowId, RowBefore, RowAfter) VALUES (NOW(), ?, ?, ?, ?, ?, ?, ?)', [
        $a ? $a['name'] : '?', $action, $label, $table, $id,
        $before === null ? null : json_encode($before, JSON_UNESCAPED_UNICODE),
        $after === null ? null : json_encode($after, JSON_UNESCAPED_UNICODE),
    ]);
}

function row_values(array $row)
{
    foreach ($row as $k => $v)
        if (is_array($v) && isset($v['b64']))
            $row[$k] = base64_decode($v['b64']);
    return $row;
}

/** Remet une ligne dans l'état $row (null = la supprimer). */
function restore_row($table, $id, $row)
{
    if ($row === null) {
        q("DELETE FROM `$table` WHERE Id = ?", [$id]);
        return;
    }
    $row = row_values($row);
    $cols = array_keys($row);
    $exists = q("SELECT 1 FROM `$table` WHERE Id = ?", [$id])->fetch();
    if ($exists) {
        $set = implode(', ', array_map(fn($c) => "`$c` = ?", $cols));
        q("UPDATE `$table` SET $set WHERE Id = ?", array_merge(array_values($row), [$id]));
    } else {
        $list = implode(', ', array_map(fn($c) => "`$c`", $cols));
        $ph = implode(', ', array_fill(0, count($cols), '?'));
        q("INSERT INTO `$table` ($list) VALUES ($ph)", array_values($row));
    }
}

/* ------------------------------------------------------------------ rendu visuel des cartes (service rendu_cartes.py) */
function render_tables()
{
    db()->exec("CREATE TABLE IF NOT EXISTS admin_map_renders (MapId INT NOT NULL PRIMARY KEY, Status VARCHAR(10) NOT NULL,
        RequestedAt DATETIME NOT NULL, DoneAt DATETIME NULL) DEFAULT CHARSET=utf8");
}

/** Image de la carte si elle est dessinée, sinon demande au service de rendu (une seule fois). */
function map_image($mapId)
{
    render_tables();
    $file = __DIR__ . "/../renders/$mapId.jpg";
    if (is_file($file))
        return ['url' => "renders/$mapId.jpg?v=" . filemtime($file), 'status' => 'ok'];
    $row = q('SELECT Status FROM admin_map_renders WHERE MapId = ?', [$mapId])->fetch();
    if ($row && $row['Status'] === 'absent')
        return ['url' => null, 'status' => 'absent'];
    q("INSERT INTO admin_map_renders (MapId, Status, RequestedAt) VALUES (?, 'demande', NOW()) ON DUPLICATE KEY UPDATE Status = IF(Status = 'ok', 'demande', Status)", [$mapId]);
    return ['url' => null, 'status' => 'demande'];
}

