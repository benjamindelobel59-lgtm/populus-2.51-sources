<?php
/*
 * Mystic : « Signaler un bug » (bouton de la barre en jeu / commande .bug).
 * Le World crée une ligne dans nekfeu_bugs (jeton à usage unique + journal du combat) et envoie ce lien au joueur ;
 * le client l'ouvre dans sa fenêtre web et téléverse la fin de son fichier de log (POST ?t=…&log=1).
 * À l'envoi du formulaire, le bug part sur le webhook Discord du salon des bugs ('bug_webhook' dans inc/config.local.php,
 * jamais partagé), avec le journal du combat et le log du client en pièces jointes si le joueur les a cochés.
 * Page publique (pas de session admin) : tout passe par le jeton.
 */
define('POPULUS_ADMIN', true);
$CFG = require __DIR__ . '/inc/config.php';

header('X-Frame-Options: SAMEORIGIN');
header('Referrer-Policy: no-referrer');
header('X-Robots-Tag: noindex, nofollow');

const LOG_CLIENT_MAX = 2000000;   // octets gardés du log du client (la fin du fichier)

function db()
{
    static $pdo = null;
    global $CFG;
    if ($pdo === null)
        $pdo = new PDO(
            "mysql:host={$CFG['db_host']};port={$CFG['db_port']};dbname={$CFG['db_name']};charset=utf8mb4",
            $CFG['db_user'], $CFG['db_pass'],
            [PDO::ATTR_ERRMODE => PDO::ERRMODE_EXCEPTION, PDO::ATTR_DEFAULT_FETCH_MODE => PDO::FETCH_ASSOC]
        );
    return $pdo;
}

function q($sql, $args = [])
{
    $st = db()->prepare($sql);
    $st->execute($args);
    return $st;
}

function h($s)
{
    return htmlspecialchars((string)$s, ENT_QUOTES, 'UTF-8');
}

function cut($s, $n)
{
    return mb_strlen($s) > $n ? mb_substr($s, 0, $n - 1) . '…' : $s;
}

/** Envoie le message (embed + pièces jointes) sur le webhook Discord. Renvoie [ok, détail]. */
function discord($url, array $payload, array $fichiers)
{
    $b = '----mystic' . bin2hex(random_bytes(8));
    $corps = "--$b\r\nContent-Disposition: form-data; name=\"payload_json\"\r\nContent-Type: application/json\r\n\r\n"
        . json_encode($payload, JSON_UNESCAPED_UNICODE) . "\r\n";
    $i = 0;
    foreach ($fichiers as $nom => $contenu) {
        $corps .= "--$b\r\nContent-Disposition: form-data; name=\"files[$i]\"; filename=\"$nom\"\r\nContent-Type: text/plain; charset=utf-8\r\n\r\n"
            . $contenu . "\r\n";
        $i++;
    }
    $corps .= "--$b--\r\n";
    $entete = "Content-Type: multipart/form-data; boundary=$b";

    if (function_exists('curl_init')) {
        $c = curl_init($url . (strpos($url, '?') === false ? '?wait=true' : '&wait=true'));
        curl_setopt_array($c, [CURLOPT_POST => true, CURLOPT_POSTFIELDS => $corps, CURLOPT_HTTPHEADER => [$entete],
            CURLOPT_RETURNTRANSFER => true, CURLOPT_TIMEOUT => 20]);
        $rep = curl_exec($c);
        $code = (int)curl_getinfo($c, CURLINFO_HTTP_CODE);
        $err = curl_error($c);
        curl_close($c);
        return [$code >= 200 && $code < 300, $code ? "HTTP $code " . cut((string)$rep, 300) : $err];
    }

    $ctx = stream_context_create(['http' => ['method' => 'POST', 'header' => $entete, 'content' => $corps,
        'timeout' => 20, 'ignore_errors' => true]]);
    $rep = @file_get_contents($url . (strpos($url, '?') === false ? '?wait=true' : '&wait=true'), false, $ctx);
    $code = 0;
    foreach ($http_response_header ?? [] as $l)
        if (preg_match('#^HTTP/\S+ (\d+)#', $l, $m))
            $code = (int)$m[1];
    return [$code >= 200 && $code < 300, "HTTP $code " . cut((string)$rep, 300)];
}

$jeton = (string)($_GET['t'] ?? '');
$bug = null;
if (preg_match('/^[a-f0-9]{48}$/', $jeton))
    $bug = q("SELECT * FROM nekfeu_bugs WHERE Token = ? AND ExpiresAt > NOW()", [$jeton])->fetch();

// 1) le client téléverse la fin de son log (corps brut)
if (isset($_GET['log'])) {
    header('Content-Type: text/plain; charset=utf-8');
    if (!$bug || $bug['Envoye']) {
        http_response_code(403);
        exit('refus');
    }
    $log = (string)file_get_contents('php://input', false, null, 0, LOG_CLIENT_MAX + 1);
    if (strlen($log) > LOG_CLIENT_MAX)
        $log = substr($log, -LOG_CLIENT_MAX);
    if (!mb_check_encoding($log, 'UTF-8'))
        $log = mb_convert_encoding($log, 'UTF-8', 'Windows-1252');
    q("UPDATE nekfeu_bugs SET LogClient = ? WHERE Token = ?", [$log, $jeton]);
    exit('ok');
}

$message = null;
$erreur = null;

// 2) envoi du formulaire
if ($_SERVER['REQUEST_METHOD'] === 'POST' && $bug && !$bug['Envoye']) {
    $titre = trim((string)($_POST['titre'] ?? ''));
    $desc = trim((string)($_POST['description'] ?? ''));
    $avecCombat = !empty($_POST['combat']);
    $avecClient = !empty($_POST['client']);

    if ($titre === '' || $desc === '') {
        $erreur = 'Le titre et la description sont obligatoires.';
    } else {
        // le log du client arrive en parallèle : on l'attend jusqu'à 6 s
        for ($i = 0; $avecClient && $i < 12 && $bug['LogClient'] === null; $i++) {
            usleep(500000);
            $bug = q("SELECT * FROM nekfeu_bugs WHERE Token = ?", [$jeton])->fetch();
        }

        q("UPDATE nekfeu_bugs SET Titre = ?, Description = ? WHERE Token = ?", [cut($titre, 200), $desc, $jeton]);

        $fichiers = [];
        if ($avecCombat && $bug['JournalCombat'])
            $fichiers['journal_combat.txt'] = $bug['JournalCombat'];
        if ($avecClient && $bug['LogClient'])
            $fichiers['log_client.txt'] = $bug['LogClient'];

        $champs = [
            ['name' => 'Personnage', 'value' => cut($bug['CharacterName'], 100) . ' (id ' . $bug['CharacterId'] . ', compte ' . $bug['AccountId'] . ')', 'inline' => true],
            ['name' => 'Carte', 'value' => (string)$bug['MapId'], 'inline' => true],
            ['name' => 'Logs', 'value' => ($avecCombat ? ($bug['JournalCombat'] ? 'combat ✔' : 'combat : aucun combat récent') : 'combat ✘')
                . ' · ' . ($avecClient ? ($bug['LogClient'] ? 'client ✔' : 'client : non reçu') : 'client ✘'), 'inline' => false],
        ];
        $payload = [
            'username' => 'Mystic - Signalement',
            'content' => '🐞 **' . cut($titre, 180) . '**',
            'embeds' => [[
                'title' => cut($titre, 250),
                'description' => cut($desc, 4000),
                'color' => 0xC8A24A,
                'fields' => $champs,
                'timestamp' => date('c'),
            ]],
            'allowed_mentions' => ['parse' => []],
        ];

        $url = trim((string)($CFG['bug_webhook'] ?? ''));
        if ($url === '') {
            q("UPDATE nekfeu_bugs SET Envoye = 1, EnvoyeAt = NOW() WHERE Token = ?", [$jeton]);
            $message = "Merci ! Ton signalement est enregistré (le salon Discord n'est pas encore configuré : l'équipe le verra dans la base).";
        } else {
            [$ok, $detail] = discord($url, $payload, $fichiers);
            if ($ok) {
                q("UPDATE nekfeu_bugs SET Envoye = 1, EnvoyeAt = NOW() WHERE Token = ?", [$jeton]);
                $message = 'Merci ! Ton signalement a été envoyé à l\'équipe.';
            } else {
                error_log('[Mystic bug.php] webhook Discord : ' . $detail);
                $erreur = "L'envoi a échoué, réessaie dans un instant (le texte est gardé).";
            }
        }
        $bug['Envoye'] = $message ? 1 : 0;
    }
}

$v = @filemtime(__DIR__ . '/assets/ingame.js');
?><!doctype html>
<html lang="fr">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<title>Signaler un bug - Mystic</title>
<link rel="stylesheet" href="assets/app.css?v=<?= (int)@filemtime(__DIR__ . '/assets/app.css') ?>">
<style>
    body { margin: 0; }
    #app > header { display: flex; align-items: center; gap: 8px; padding: 8px 12px; }
    #app > header h1 { font-size: 1.05rem; margin: 0; flex: 1; }
    main { padding: 14px 18px; display: flex; flex-direction: column; gap: 10px; }
    label.ch { display: block; font-weight: 600; margin-bottom: 4px; }
    input[type=text], textarea { width: 100%; box-sizing: border-box; font: inherit; padding: 7px 9px; }
    textarea { min-height: 150px; resize: vertical; }
    .cases label { display: flex; gap: 8px; align-items: center; margin: 4px 0; }
    .note { opacity: .75; font-size: .85rem; }
    .msg { padding: 10px 12px; border-radius: 6px; }
    .msg.ok { background: rgba(60, 160, 90, .2); }
    .msg.err { background: rgba(200, 60, 60, .2); }
    .actions { display: flex; gap: 8px; justify-content: flex-end; }
    button.envoyer { font-weight: 700; padding: 8px 18px; }
</style>
</head>
<body>
<div id="app">
    <header><h1>🐞 Signaler un bug</h1></header>
    <main>
<?php if (!$bug): ?>
        <p class="msg err">Lien expiré ou invalide. Reclique sur le bouton « Signaler un bug » de la barre (ou tape .bug).</p>
<?php elseif ($bug['Envoye'] && $message): ?>
        <p class="msg ok"><?= h($message) ?></p>
        <div class="actions"><button type="button" onclick="fermer()">Fermer</button></div>
<?php elseif ($bug['Envoye']): ?>
        <p class="msg ok">Ce signalement a déjà été envoyé. Merci !</p>
<?php else: ?>
        <?php if ($erreur): ?><p class="msg err"><?= h($erreur) ?></p><?php endif ?>
        <form method="post" id="f">
            <div>
                <label class="ch" for="titre">Titre du bug</label>
                <input type="text" id="titre" name="titre" maxlength="200" required autofocus
                       placeholder="Ex. : le Comte reste invulnérable" value="<?= h($_POST['titre'] ?? '') ?>">
            </div>
            <div>
                <label class="ch" for="description">Description du bug</label>
                <textarea id="description" name="description" required
                          placeholder="Ce que tu faisais, ce qui s'est passé, ce qui aurait dû se passer…"><?= h($_POST['description'] ?? '') ?></textarea>
            </div>
            <div class="cases">
                <label><input type="checkbox" name="combat" value="1" checked> Envoyer les logs du combat
                    <span class="note">(<?= $bug['JournalCombat'] ? 'combat en cours ou dernier combat' : 'aucun combat récent' ?>)</span></label>
                <label><input type="checkbox" name="client" value="1" checked> Envoyer le log du client
                    <span class="note">(fin du fichier de log de ta fenêtre de jeu)</span></label>
            </div>
            <p class="note">Personnage : <?= h($bug['CharacterName']) ?> · carte <?= (int)$bug['MapId'] ?></p>
            <div class="actions">
                <button type="button" onclick="fermer()">Annuler</button>
                <button type="submit" class="envoyer" id="go">Envoyer</button>
            </div>
        </form>
<?php endif ?>
    </main>
</div>
<script>
    const $ = (sel, root = document) => root.querySelector(sel);
    function fermer() {
        const wv = window.chrome && window.chrome.webview;
        if (wv) { try { wv.postMessage('close'); } catch { } } else window.close();
    }
    const f = $('#f');
    if (f) f.addEventListener('submit', () => { $('#go').disabled = true; $('#go').textContent = 'Envoi…'; });
</script>
<script src="assets/ingame.js?v=<?= (int)$v ?>"></script>
</body>
</html>
