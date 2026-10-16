<?php
/*
 * Panel admin Populus 2.51 : onglets Serveur (santé, redémarrage propre, événements programmés),
 * Surveillance (alertes, progression, IP, échanges, HDV, kamas, sanctions, commandes MJ) et
 * Support (copies quotidiennes comparées à maintenant, objets rendus avec leurs jets exacts).
 * Les données viennent du journal tenu par le serveur de jeu (AdminJournal) et de la base.
 */
if (!defined('POPULUS_ADMIN'))
    exit;

function cfg($key, $default)
{
    global $CFG;
    return $CFG[$key] ?? $default;
}

function copies_list()
{
    $rows = q("SELECT TABLE_NAME FROM information_schema.TABLES WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME LIKE 'admin\\_copy\\_%\\_c' ORDER BY TABLE_NAME DESC")->fetchAll(PDO::FETCH_COLUMN);
    return array_map(function ($t) {
        $s = substr($t, 11, 12);
        return ['stamp' => $s, 'label' => substr($s, 6, 2) . '/' . substr($s, 4, 2) . '/' . substr($s, 0, 4) . ' ' . substr($s, 8, 2) . 'h' . substr($s, 10, 2)];
    }, $rows);
}

function item_names(array $ids)
{
    $ids = array_values(array_unique(array_map('intval', $ids)));
    if (!$ids)
        return [];
    $in = implode(',', $ids);
    $rows = q("SELECT Id, NameId, Level FROM items_templates WHERE Id IN ($in) UNION ALL SELECT Id, NameId, Level FROM items_templates_weapons WHERE Id IN ($in)")->fetchAll();
    $t = lang_map(array_column($rows, 'NameId'));
    $out = [];
    foreach ($rows as $r)
        $out[(int)$r['Id']] = ['name' => $t[(int)$r['NameId']] ?? ('objet ' . $r['Id']), 'level' => (int)$r['Level']];
    return $out;
}

function handle_ops($action, $me)
{
    $in = $GLOBALS['in'];
    $auth = preg_replace('/\W/', '', cfg('auth_db', 'auth'));
    switch ($action) {

        /* ============================================================ SERVEUR */
        case 'server_health':
            $live = enqueue('health');
            $history = q('SELECT DATE_FORMAT(At, "%d/%m %H:%i") t, Online, Fights, MemoryMb FROM admin_panel_health WHERE At > NOW() - INTERVAL 24 HOUR ORDER BY At')->fetchAll();
            $events = q('SELECT * FROM admin_panel_events ORDER BY FIELD(Status, "en cours", "prevu") DESC, StartAt DESC LIMIT 50')->fetchAll();
            json_out(['live' => !empty($live['ok']) ? json_decode($live['result'], true) : null, 'history' => $history, 'events' => $events,
                'peak' => q('SELECT MAX(Online) FROM admin_panel_health WHERE At > NOW() - INTERVAL 24 HOUR')->fetchColumn()]);

        case 'server_restart':
            $min = max(0, min(120, (int)($in['minutes'] ?? 5)));
            $reason = mb_substr(trim((string)($in['reason'] ?? '')), 0, 200);
            log_action('Redémarrage programmé', "dans $min min : $reason");
            json_out(enqueue('restart', "$min;$reason"));

        case 'server_restart_cancel':
            log_action('Redémarrage annulé', '');
            json_out(enqueue('restart_cancel'));

        case 'server_announce':
            $msg = trim((string)($in['message'] ?? ''));
            if ($msg === '')
                fail('Message vide');
            log_action('Annonce', $msg);
            json_out(enqueue('announce', $msg));

        case 'event_save':
            $kind = ($in['kind'] ?? '') === 'annonce' ? 'annonce' : 'taux';
            $start = date('Y-m-d H:i:s', strtotime((string)($in['start'] ?? 'now')) ?: time());
            $end = trim((string)($in['end'] ?? '')) === '' ? null : date('Y-m-d H:i:s', strtotime((string)$in['end']));
            if ($kind === 'taux' && !$end)
                fail('Un événement de taux a besoin d\'une date de fin.');
            if ($end && $end <= $start)
                fail('La fin doit être après le début.');
            $rate = fn($k) => isset($in[$k]) && $in[$k] !== '' && (float)$in[$k] > 0 ? (float)str_replace(',', '.', (string)$in[$k]) : null;
            q('INSERT INTO admin_panel_events (Kind, StartAt, EndAt, XpRate, DropsRate, KamasRate, Message, RepeatMinutes, CreatedBy) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)',
                [$kind, $start, $end, $rate('xp'), $rate('drops'), $rate('kamas'), trim((string)($in['message'] ?? '')) ?: null, max(0, (int)($in['repeat'] ?? 0)), $me['name']]);
            log_action('Événement programmé', "$kind du $start au " . ($end ?? '—'));
            json_out(['ok' => true]);

        case 'event_cancel':
            $id = (int)($in['id'] ?? 0);
            $e = q('SELECT * FROM admin_panel_events WHERE Id = ?', [$id])->fetch();
            if (!$e)
                fail('Événement introuvable');
            if ($e['Status'] === 'en cours' && $e['Kind'] === 'taux')
                enqueue('event_end', (string)$id);
            else
                q("UPDATE admin_panel_events SET Status = 'annule' WHERE Id = ?", [$id]);
            log_action('Événement annulé', "#$id");
            json_out(['ok' => true]);

        /* ============================================================ SURVEILLANCE */
        case 'watch':
            $what = $_GET['what'] ?? 'alerts';
            switch ($what) {
                case 'alerts':
                    $lv = (int)cfg('alert_levels_24h', 40);
                    $km = (int)cfg('alert_kamas_24h', 20000000);
                    $ipMax = (int)cfg('alert_accounts_per_ip', 4);
                    $levels = q("SELECT CharacterId, Name, MIN(Level) a, MAX(Level) b, MAX(Level) - MIN(Level) d FROM admin_journal_snapshots
                        WHERE At > NOW() - INTERVAL 24 HOUR GROUP BY CharacterId, Name HAVING d >= ? ORDER BY d DESC LIMIT 50", [$lv])->fetchAll();
                    $money = q("SELECT s.CharacterId, s.Name, MAX(s.Kamas + s.BankKamas) - MIN(s.Kamas + s.BankKamas) d FROM admin_journal_snapshots s
                        WHERE s.At > NOW() - INTERVAL 24 HOUR GROUP BY s.CharacterId, s.Name HAVING d >= ? ORDER BY d DESC LIMIT 50", [$km])->fetchAll();
                    $ips = q("SELECT Ip, COUNT(DISTINCT AccountId) n, GROUP_CONCAT(DISTINCT Name ORDER BY Name SEPARATOR ', ') names FROM admin_journal_events
                        WHERE Kind = 'connexion' AND At > NOW() - INTERVAL 24 HOUR AND Ip IS NOT NULL GROUP BY Ip HAVING n >= ? ORDER BY n DESC", [$ipMax])->fetchAll();
                    $trades = q("SELECT Id, At, Name, Value, Details, Handled FROM admin_journal_events WHERE Kind = 'echange' AND Value >= ? AND At > NOW() - INTERVAL 3 DAY ORDER BY Value DESC LIMIT 30",
                        [(int)cfg('alert_trade_kamas', 1000000)])->fetchAll();
                    json_out(['levels' => $levels, 'money' => $money, 'ips' => $ips, 'trades' => $trades,
                        'thresholds' => ['levels' => $lv, 'kamas' => $km, 'ip' => $ipMax, 'trade' => (int)cfg('alert_trade_kamas', 1000000)]]);

                case 'progress':
                    $name = trim((string)($_GET['q'] ?? ''));
                    $rows = q("SELECT CharacterId, Name, MIN(Level) a, MAX(Level) b, MIN(Kamas + BankKamas) ka, MAX(Kamas + BankKamas) kb, MAX(At) last
                        FROM admin_journal_snapshots WHERE At > NOW() - INTERVAL 7 DAY AND Name LIKE ? GROUP BY CharacterId, Name ORDER BY (MAX(Level) - MIN(Level)) DESC LIMIT 100", ["%$name%"])->fetchAll();
                    json_out(['rows' => $rows]);

                case 'ip':
                    $rows = q("SELECT Ip, COUNT(DISTINCT AccountId) accounts, COUNT(DISTINCT CharacterId) chars, GROUP_CONCAT(DISTINCT Name ORDER BY Name SEPARATOR ', ') names, MAX(At) last
                        FROM admin_journal_events WHERE Kind = 'connexion' AND At > NOW() - INTERVAL 7 DAY AND Ip IS NOT NULL GROUP BY Ip ORDER BY accounts DESC, last DESC LIMIT 200")->fetchAll();
                    json_out(['rows' => $rows]);

                case 'trades':
                    $name = trim((string)($_GET['q'] ?? ''));
                    $rows = q("SELECT Id, At, Name, Ip, Value, Details FROM admin_journal_events WHERE Kind = 'echange' AND (Name LIKE ? OR Details LIKE ?) ORDER BY Id DESC LIMIT 150",
                        ["%$name%", "%$name%"])->fetchAll();
                    $ids = [];
                    foreach ($rows as &$r) {
                        $r['Details'] = json_decode($r['Details'], true);
                        foreach (array_merge($r['Details']['donne'] ?? [], $r['Details']['recoit'] ?? []) as $it)
                            $ids[] = $it['id'];
                    }
                    unset($r);
                    json_out(['rows' => $rows, 'items' => item_names($ids)]);

                case 'hdv':
                    $rows = q("SELECT b.Id, b.ItemId, b.Stack, b.Price, b.SellDate, b.OwnerId,
                        (SELECT AVG(x.Price / GREATEST(x.Stack, 1)) FROM bidhouse_items x WHERE x.ItemId = b.ItemId AND x.Sold = 1) avgUnit
                        FROM bidhouse_items b WHERE b.Sold = 1 ORDER BY b.SellDate DESC LIMIT 150")->fetchAll();
                    $names = item_names(array_column($rows, 'ItemId'));
                    $ratio = (float)cfg('alert_hdv_price_ratio', 10);
                    foreach ($rows as &$r) {
                        $r['Name'] = $names[(int)$r['ItemId']]['name'] ?? ('objet ' . $r['ItemId']);
                        $unit = $r['Price'] / max(1, $r['Stack']);
                        $r['suspect'] = $r['avgUnit'] > 0 && $unit >= $ratio * $r['avgUnit'];
                    }
                    unset($r);
                    $onSale = q('SELECT COUNT(*) n, SUM(Price) total FROM bidhouse_items WHERE Sold = 0')->fetch();
                    json_out(['rows' => $rows, 'onSale' => $onSale, 'ratio' => $ratio]);

                case 'money':
                    $days = q("SELECT DATE(s.At) d, SUM(s.Kamas + s.BankKamas) total, COUNT(*) n FROM admin_journal_snapshots s
                        JOIN (SELECT CharacterId, DATE(At) d, MAX(At) m FROM admin_journal_snapshots GROUP BY CharacterId, DATE(At)) l
                          ON l.CharacterId = s.CharacterId AND l.m = s.At GROUP BY DATE(s.At) ORDER BY d")->fetchAll();
                    $rich = q("SELECT c.Id, c.Name, c.Kamas, COALESCE(a.BankKamas, 0) Bank, c.Kamas + COALESCE(a.BankKamas, 0) Total FROM characters c
                        LEFT JOIN accounts a ON a.Id = c.AccountId ORDER BY Total DESC LIMIT 25")->fetchAll();
                    $total = q('SELECT SUM(Kamas) FROM characters')->fetchColumn() + q('SELECT SUM(BankKamas) FROM accounts')->fetchColumn();
                    json_out(['days' => $days, 'rich' => $rich, 'total' => $total]);

                case 'sanctions':
                    $bans = q("SELECT Id, Login, Nickname, BanReason, BanEndDate, IsJailed, IsBanned FROM `$auth`.accounts WHERE IsBanned = 1 OR IsJailed = 1 ORDER BY BanEndDate DESC LIMIT 200")->fetchAll();
                    $cmds = q("SELECT At, Name, Details FROM admin_journal_events WHERE Kind = 'commande' AND (Details LIKE '.ban%' OR Details LIKE '.unban%' OR Details LIKE '.mute%'
                        OR Details LIKE '.unmute%' OR Details LIKE '.jail%' OR Details LIKE '.unjail%' OR Details LIKE '.kick%') ORDER BY Id DESC LIMIT 100")->fetchAll();
                    json_out(['bans' => $bans, 'history' => $cmds]);

                case 'commands':
                    $name = trim((string)($_GET['q'] ?? ''));
                    json_out(['rows' => q("SELECT At, Name, Ip, Details FROM admin_journal_events WHERE Kind = 'commande' AND (Name LIKE ? OR Details LIKE ?) ORDER BY Id DESC LIMIT 200",
                        ["%$name%", "%$name%"])->fetchAll()]);
            }
            fail('Section inconnue');

        case 'watch_handle':
            q('UPDATE admin_journal_events SET Handled = 1 WHERE Id = ?', [(int)($in['id'] ?? 0)]);
            json_out(['ok' => true]);

        /* ============================================================ SUPPORT */
        case 'support_copies':
            json_out(['copies' => copies_list()]);

        case 'support_copy_now':
            $r = enqueue('make_copy');
            if (empty($r['ok']))
                fail($r['result'] ?: 'Le serveur de jeu ne répond pas.');
            log_action('Copie de sauvegarde', $r['result']);
            json_out(['ok' => true, 'copies' => copies_list()]);

        case 'support_compare':
            $stamp = preg_replace('/\D/', '', (string)($_GET['copy'] ?? ''));
            $cid = (int)($_GET['id'] ?? 0);
            if (strlen($stamp) !== 12 || !q("SELECT 1 FROM information_schema.TABLES WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = ?", ["admin_copy_{$stamp}_c"])->fetch())
                fail('Copie introuvable');
            $p = "admin_copy_$stamp";
            $then = q("SELECT c.*, COALESCE(a.BankKamas, 0) BankKamas FROM {$p}_c c LEFT JOIN {$p}_a a ON a.Id = c.AccountId WHERE c.Id = ?", [$cid])->fetch();
            $now = q("SELECT c.Id, c.Name, c.AccountId, c.Experience, c.Kamas, c.MapId, COALESCE(a.BankKamas, 0) BankKamas FROM characters c LEFT JOIN accounts a ON a.Id = c.AccountId WHERE c.Id = ?", [$cid])->fetch();
            if (!$now && !$then)
                fail('Personnage introuvable');
            $acc = (int)($now['AccountId'] ?? $then['AccountId']);
            $load = function ($sqlInv, $sqlBank) use ($cid, $acc) {
                $out = [];
                foreach (q($sqlInv, [$cid])->fetchAll() as $r)
                    $out[] = $r + ['where' => 'inventaire'];
                foreach (q($sqlBank, [$acc])->fetchAll() as $r)
                    $out[] = $r + ['where' => 'banque'];
                return $out;
            };
            $before = $load("SELECT Id, ItemId, Stack, SerializedEffects FROM {$p}_i WHERE OwnerId = ?", "SELECT Id, ItemId, Stack, SerializedEffects FROM {$p}_b WHERE OwnerAccountId = ?");
            $after = $load('SELECT Id, ItemId, Stack, SerializedEffects FROM characters_items WHERE OwnerId = ?', 'SELECT Id, ItemId, Stack, SerializedEffects FROM accounts_items_bank WHERE OwnerAccountId = ?');
            // regroupement par modèle + jets : un objet rangé en banque ou empilé n'est pas compté perdu
            $group = function ($list) {
                $g = [];
                foreach ($list as $r) {
                    $k = $r['ItemId'] . ':' . md5((string)$r['SerializedEffects']);
                    if (!isset($g[$k]))
                        $g[$k] = ['ItemId' => (int)$r['ItemId'], 'qty' => 0, 'rows' => [], 'fx' => base64_encode((string)$r['SerializedEffects'])];
                    $g[$k]['qty'] += (int)$r['Stack'];
                    $g[$k]['rows'][] = ['id' => (int)$r['Id'], 'stack' => (int)$r['Stack'], 'where' => $r['where']];
                }
                return $g;
            };
            $gb = $group($before);
            $ga = $group($after);
            $lost = [];
            $gained = [];
            foreach ($gb as $k => $v)
                if (($d = $v['qty'] - ($ga[$k]['qty'] ?? 0)) > 0)
                    $lost[] = $v + ['diff' => $d];
            foreach ($ga as $k => $v)
                if (($d = $v['qty'] - ($gb[$k]['qty'] ?? 0)) > 0)
                    $gained[] = $v + ['diff' => $d];
            $fx = enqueue('decode_effects', json_encode(array_merge(array_column($lost, 'fx'), array_column($gained, 'fx'))));
            $decoded = !empty($fx['ok']) ? json_decode($fx['result'], true) : [];
            foreach ($lost as $i => &$l)
                $l['effects'] = $decoded[$i] ?? null;
            unset($l);
            foreach ($gained as $i => &$g)
                $g['effects'] = $decoded[count($lost) + $i] ?? null;
            unset($g);
            $names = item_names(array_merge(array_column($lost, 'ItemId'), array_column($gained, 'ItemId')));
            $since = date('Y-m-d H:i:s', strtotime(substr($stamp, 0, 8) . ' ' . substr($stamp, 8, 2) . ':' . substr($stamp, 10, 2)));
            $trades = q("SELECT At, Name, Value, Details FROM admin_journal_events WHERE Kind = 'echange' AND At >= ? AND (CharacterId = ? OR Details LIKE ?) ORDER BY At",
                [$since, $cid, '%"avecId":' . $cid . ',%'])->fetchAll();
            json_out(['then' => $then, 'now' => $now, 'lost' => $lost, 'gained' => $gained, 'items' => $names, 'trades' => $trades, 'copy' => $stamp,
                'online' => (bool)q("SELECT 1 FROM admin_journal_events WHERE Kind = 'connexion' AND CharacterId = ? LIMIT 1", [$cid])->fetch()]);

        case 'support_give':
            $stamp = preg_replace('/\D/', '', (string)($in['copy'] ?? ''));
            $table = "admin_copy_{$stamp}_" . (($in['where'] ?? '') === 'banque' ? 'b' : 'i');
            $r = enqueue('give_copy_item', (int)($in['characterId'] ?? 0) . ";$table;" . (int)($in['rowId'] ?? 0));
            if (empty($r['ok']))
                fail(!empty($r['timeout']) ? 'Le serveur de jeu ne répond pas.' : ($r['result'] ?: 'Échec'));
            log_action('Objet rendu (support)', $r['result']);
            json_out(['ok' => true, 'message' => $r['result']]);
    }
    return false;
}
