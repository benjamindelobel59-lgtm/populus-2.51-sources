<?php
/*
 * Panel admin Populus 2.51 : dialogues de PNJ « comme le 1.29 ».
 *  - recherche de questions / réponses dans tous les textes de PNJ du jeu (début d'un mot) ;
 *  - n'importe quel texte du jeu pour n'importe quel PNJ, et questions libres écrites dans le panel.
 * Numéros spéciaux (compris par l'aide du client populus.NpcTextFix, module Roleplay) :
 *   0x40000000 + n : texte n° n des fichiers de langue ; 0x50000000 + k : question libre (texte 29920 « #1 » + paramètre).
 * Liaison PNJ <-> questions ajoutées : table admin_npc_messages.
 */
if (!defined('POPULUS_ADMIN'))
    exit;

const MSG_DIRECT = 0x40000000;
const MSG_CUSTOM = 0x50000000;
const TEXT_TEMPLATE = 29920;

function npc2_tables()
{
    static $done = false;
    if ($done)
        return;
    $done = true;
    db()->exec('CREATE TABLE IF NOT EXISTS admin_npc_messages (NpcId INT NOT NULL, MessageId INT NOT NULL, PRIMARY KEY (NpcId, MessageId)) DEFAULT CHARSET=utf8');
    // index des textes de PNJ (questions / réponses de tous les PNJ du jeu), construit une fois
    db()->exec('CREATE TABLE IF NOT EXISTS admin_npc_texts (TextId INT NOT NULL, Kind CHAR(1) NOT NULL, Npcs INT NOT NULL DEFAULT 0, French TEXT NULL,
        PRIMARY KEY (TextId, Kind), FULLTEXT KEY ft (French)) DEFAULT CHARSET=utf8mb4');
    if (!q('SELECT 1 FROM admin_npc_texts LIMIT 1')->fetch()) {
        $counts = ['q' => [], 'r' => []];
        foreach (q('SELECT DialogMessagesIdCSV m, DialogRepliesIdCSV r FROM npcs_templates') as $t) {
            foreach (['q' => $t['m'], 'r' => $t['r']] as $kind => $csv)
                foreach (parse_pairs($csv) as $p)
                    $counts[$kind][$p[1]] = ($counts[$kind][$p[1]] ?? 0) + 1;
        }
        foreach ($counts as $kind => $list) {
            foreach (array_chunk(array_keys($list), 500) as $chunk) {
                $texts = lang_map($chunk);
                $rows = [];
                $args = [];
                foreach ($chunk as $id) {
                    if (empty($texts[$id]))
                        continue;
                    $rows[] = '(?, ?, ?, ?)';
                    array_push($args, $id, $kind, $list[$id], $texts[$id]);
                }
                if ($rows)
                    q('INSERT IGNORE INTO admin_npc_texts (TextId, Kind, Npcs, French) VALUES ' . implode(',', $rows), $args);
            }
        }
    }
}

/** Questions ajoutées à un PNJ depuis le panel (textes du jeu ou libres), avec leur texte. */
function npc_extra_messages($npcId)
{
    npc2_tables();
    $rows = q('SELECT m.Id, m.MessageId, m.ParametersCSV FROM admin_npc_messages a JOIN npcs_messages m ON m.Id = a.MessageId WHERE a.NpcId = ? ORDER BY m.Id', [$npcId])->fetchAll();
    $texts = lang_map(array_map(fn($r) => $r['MessageId'], array_filter($rows, fn($r) => $r['Id'] < MSG_CUSTOM)));
    return array_map(fn($r) => [
        'id' => (int)$r['Id'],
        'textId' => (int)$r['MessageId'],
        'custom' => $r['Id'] >= MSG_CUSTOM,
        'text' => $r['Id'] >= MSG_CUSTOM ? (string)$r['ParametersCSV'] : ($texts[(int)$r['MessageId']] ?? null),
    ], $rows);
}

/** Texte d'une réponse au numéro spécial (0x40000000 + texte). */
function reply_text_for($replyId)
{
    if ($replyId >= MSG_DIRECT && $replyId < MSG_CUSTOM)
        return lang_map([$replyId - MSG_DIRECT])[$replyId - MSG_DIRECT] ?? null;
    return null;
}

function handle_npc2($action, $me)
{
    $in = $GLOBALS['in'];
    switch ($action) {

        case 'dialog_texts':
            npc2_tables();
            $s = trim((string)($_GET['q'] ?? ''));
            $kind = ($_GET['kind'] ?? 'q') === 'r' ? 'r' : 'q';
            if (mb_strlen($s) < 2)
                json_out(['texts' => []]);
            // début d'un mot : en tête du texte ou après un espace / une ponctuation
            $like = str_replace(['\\', '%', '_'], ['\\\\', '\%', '\_'], $s);
            $rows = q("SELECT MIN(TextId) TextId, SUM(Npcs) Npcs, French FROM admin_npc_texts WHERE Kind = ? AND (French LIKE ? OR French LIKE ? OR French LIKE ? OR French LIKE ?)
                GROUP BY French ORDER BY Npcs DESC, CHAR_LENGTH(French) LIMIT 60", [$kind, "$like%", "% $like%", "%'$like%", "%\n$like%"])->fetchAll();
            json_out(['texts' => $rows]);

        case 'npc_message_add':
            npc2_tables();
            $npc = (int)($in['npcId'] ?? 0);
            if (!q('SELECT 1 FROM npcs_templates WHERE Id = ?', [$npc])->fetch())
                fail('PNJ introuvable');
            $custom = trim(str_replace(['|', "\r"], ['/', ''], (string)($in['custom'] ?? '')));
            if ($custom !== '') {
                $next = max(MSG_CUSTOM, (int)q('SELECT MAX(Id) FROM npcs_messages WHERE Id >= ?', [MSG_CUSTOM])->fetchColumn() + 1);
                q('INSERT INTO npcs_messages (Id, MessageId, ParametersCSV) VALUES (?, ?, ?)', [$next, TEXT_TEMPLATE, mb_substr($custom, 0, 2000)]);
                $msgId = $next;
                log_change('Question libre créée', 'npcs_messages', $msgId, null, mb_substr($custom, 0, 60));
            } else {
                $textId = (int)($in['textId'] ?? 0);
                if (!$textId || !q('SELECT 1 FROM langs WHERE Id = ?', [$textId])->fetch())
                    fail('Texte introuvable');
                $msgId = MSG_DIRECT + $textId;
                if (!q('SELECT 1 FROM npcs_messages WHERE Id = ?', [$msgId])->fetch()) {
                    q("INSERT INTO npcs_messages (Id, MessageId, ParametersCSV) VALUES (?, ?, '')", [$msgId, $textId]);
                    log_change('Question du jeu ajoutée', 'npcs_messages', $msgId, null, "texte $textId");
                }
            }
            q('INSERT IGNORE INTO admin_npc_messages (NpcId, MessageId) VALUES (?, ?)', [$npc, $msgId]);
            json_out(['ok' => true, 'id' => $msgId, 'reload' => reload_npcs()]);

        case 'npc_message_custom_save':
            $id = (int)($in['id'] ?? 0);
            if ($id < MSG_CUSTOM)
                fail('Seules les questions libres se modifient');
            $text = trim(str_replace(['|', "\r"], ['/', ''], (string)($in['text'] ?? '')));
            if ($text === '')
                fail('Texte vide');
            $before = snapshot('npcs_messages', $id);
            if (!$before)
                fail('Question introuvable');
            q('UPDATE npcs_messages SET ParametersCSV = ? WHERE Id = ?', [mb_substr($text, 0, 2000), $id]);
            log_change('Question libre modifiée', 'npcs_messages', $id, $before, mb_substr($text, 0, 60));
            json_out(['ok' => true, 'reload' => reload_npcs()]);

        case 'npc_message_detach':
            npc2_tables();
            q('DELETE FROM admin_npc_messages WHERE NpcId = ? AND MessageId = ?', [(int)($in['npcId'] ?? 0), (int)($in['id'] ?? 0)]);
            json_out(['ok' => true]);

        case 'look_items':
            $s = trim((string)($_GET['q'] ?? ''));
            if (mb_strlen($s) < 2)
                json_out(['items' => []]);
            $sql = 'SELECT i.Id, i.Level, i.AppearanceId, l.French Name FROM (SELECT Id, NameId, Level, AppearanceId FROM items_templates UNION ALL
                SELECT Id, NameId, Level, AppearanceId FROM items_templates_weapons) i JOIN langs l ON l.Id = i.NameId WHERE i.AppearanceId > 0 AND l.French LIKE ? ORDER BY i.Level LIMIT 40';
            json_out(['items' => q($sql, ["%$s%"])->fetchAll()]);

        case 'map_npcs':
            $st = server_json('state');
            $map = (int)($st['me']['mapId'] ?? 0);
            if (!$map)
                fail('Ton personnage doit être connecté.');
            $rows = q("SELECT s.Id, s.NpcId, s.CellId, s.Direction, COALESCE(NULLIF(l.French,''), t.Name) Name,
                (SELECT GROUP_CONCAT(DISTINCT a.Type) FROM npcs_actions a WHERE a.NpcId = s.NpcId) Actions
                FROM npcs_spawns s JOIN npcs_templates t ON t.Id = s.NpcId LEFT JOIN langs l ON l.Id = t.NameId WHERE s.MapId = ? ORDER BY s.CellId", [$map])->fetchAll();
            json_out(['map' => $map, 'cell' => $st['me']['cellId'], 'direction' => $st['me']['direction'] ?? null, 'npcs' => $rows]);
    }
    return false;
}
