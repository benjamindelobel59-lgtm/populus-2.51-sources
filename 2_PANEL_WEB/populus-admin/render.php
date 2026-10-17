<?php
/*
 * Panel admin Populus 2.51 : échange avec le service de rendu des cartes (rendu_cartes.py, sur le PC qui a le client).
 *   GET  render.php?a=wanted            -> cartes demandées par les admins et pas encore dessinées
 *   POST render.php?a=upload&id=<map>   -> image JPG de la carte (corps de la requête)
 * Accès par la clé partagée (en-tête X-Render-Key = 'render_key' de inc/config.local.php), pas de session.
 */
define('POPULUS_ADMIN', true);
require __DIR__ . '/inc/lib.php';

header('Content-Type: application/json; charset=utf-8');
header('Cache-Control: no-store');

$key = (string)($CFG['render_key'] ?? '');
if ($key === '' || !hash_equals($key, (string)($_SERVER['HTTP_X_RENDER_KEY'] ?? '')))
    fail('Clé de rendu invalide', 403);

render_tables();
$dir = __DIR__ . '/renders';
if (!is_dir($dir))
    mkdir($dir, 0775, true);

switch ($_GET['a'] ?? '') {
    case 'wanted':
        $ids = q("SELECT MapId FROM admin_map_renders WHERE Status = 'demande' ORDER BY RequestedAt LIMIT 20")->fetchAll(PDO::FETCH_COLUMN);
        json_out(['maps' => array_map('intval', $ids)]);

    case 'cells_changed':
        // cartes dont les cases ont été modifiées (ou rétablies) depuis le panel, avec la date de la dernière modification
        $rows = q("SELECT RowId id, MAX(At) at FROM admin_panel_log WHERE TableName = 'world_maps'
            AND (Action LIKE 'Cases de la map%' OR Action LIKE 'Annulation : Cases de la map%') GROUP BY RowId")->fetchAll();
        json_out(['maps' => array_map(fn($r) => ['id' => (int)$r['id'], 'at' => $r['at']], $rows)]);

    case 'cells':
        $id = (int)($_GET['id'] ?? 0);
        $blob = q('SELECT CompressedCells FROM world_maps WHERE Id = ?', [$id])->fetchColumn();
        $raw = $blob === false ? false : @gzdecode($blob);
        if ($raw === false)
            fail('Carte introuvable');
        $cells = [];
        for ($i = 0; $i + 11 <= strlen($raw); $i += 11)
            $cells[] = [ord($raw[$i + 4]), ord($raw[$i + 5])];
        json_out(['cells' => $cells]);

    case 'upload':
        $id = (int)($_GET['id'] ?? 0);
        if ($id <= 0)
            fail('Carte invalide');
        if (!empty($_GET['absent'])) {
            q("UPDATE admin_map_renders SET Status = 'absent', DoneAt = NOW() WHERE MapId = ?", [$id]);
            json_out(['ok' => true]);
        }
        $data = file_get_contents('php://input');
        if (strlen($data) < 100 || substr($data, 0, 2) !== "\xFF\xD8")
            fail('Image JPG attendue');
        file_put_contents("$dir/$id.jpg", $data);
        q("INSERT INTO admin_map_renders (MapId, Status, RequestedAt, DoneAt) VALUES (?, 'ok', NOW(), NOW())
            ON DUPLICATE KEY UPDATE Status = 'ok', DoneAt = NOW()", [$id]);
        json_out(['ok' => true]);
}
fail('Action inconnue');
