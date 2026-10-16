<?php
/*
 * Panel admin Populus 2.51 : page d'accueil. S'ouvre avec le lien envoyé par la commande .admin en jeu.
 */
define('POPULUS_ADMIN', true);
require __DIR__ . '/inc/lib.php';

header('X-Frame-Options: SAMEORIGIN');
header('Referrer-Policy: no-referrer');
header('X-Robots-Tag: noindex, nofollow');

if (isset($_GET['t'])) {
    $ok = login_with_token((string)$_GET['t']);
    header('Location: ' . strtok($_SERVER['REQUEST_URI'], '?') . ($ok ? '' : '?refus=1'), true, 303);
    exit;
}

$me = admin();
$v = max(array_map('filemtime', glob(__DIR__ . '/assets/*')));
?><!doctype html>
<html lang="fr">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<title>Panel admin Populus</title>
<link rel="stylesheet" href="assets/app.css?v=<?= $v ?>">
</head>
<body>
<?php if (!$me): ?>
<div class="gate">
  <h1>Panel admin Populus</h1>
  <?php if (isset($_GET['refus'])): ?>
    <p class="err">Ce lien n'est plus valable (il ne sert qu'une fois et expire au bout de quelques minutes).</p>
  <?php endif; ?>
  <p>Pour ouvrir le panel, connecte-toi en jeu avec un personnage administrateur et tape <code>.admin</code> dans le chat,
     puis clique sur le lien.</p>
</div>
<?php else: ?>
<div id="app">
  <header>
    <div class="brand">Populus <span>admin</span></div>
    <div id="where" class="where">…</div>
    <div id="status" class="status"></div>
    <div class="hbtn">
      <button id="btnState" title="Mettre à jour ta position">◎ Position</button>
      <button id="btnReload" title="Recharger tous les PNJ en jeu">⟳ PNJ</button>
    </div>
  </header>
  <nav id="tabs">
    <button data-tab="home">🏠 Accueil</button>
    <button data-tab="players">👥 Joueurs</button>
    <button data-tab="items">🎒 Objets</button>
    <button data-tab="sets">🧩 Panoplies</button>
    <button data-tab="mobs">🐾 Monstres</button>
    <button data-tab="npcs">🧙 PNJ</button>
    <button data-tab="spells">✨ Sorts</button>
    <button data-tab="maps">🗺️ Cartes</button>
    <button data-tab="server">🩺 Serveur</button>
    <button data-tab="watch">🕵️ Surveillance</button>
    <button data-tab="support">🛟 Support</button>
    <button data-tab="history">🕘 Historique</button>
    <button data-tab="console">⌨ Console</button>
    <button data-tab="log">📜 Journal</button>
    <button data-tab="help">❓ Aide</button>
  </nav>
  <main id="main"></main>
  <footer id="out"><div class="outh"><b>Réponses du serveur</b><button id="outClear" class="small">effacer</button></div><div id="outBody"></div></footer>
</div>
<div id="toast"></div>
<script>window.CSRF = <?= json_encode($me['csrf']) ?>; window.ME = <?= json_encode($me['name'], JSON_UNESCAPED_UNICODE) ?>;</script>
<script src="assets/app.js?v=<?= $v ?>"></script>
<script src="assets/lot1.js?v=<?= $v ?>"></script>
<script src="assets/npcplus.js?v=<?= $v ?>"></script>
<script src="assets/lot2.js?v=<?= $v ?>"></script>
<script src="assets/lot3.js?v=<?= $v ?>"></script>
<script src="assets/lot4.js?v=<?= $v ?>"></script>
<script src="assets/lot5.js?v=<?= $v ?>"></script>
<script src="assets/lot6.js?v=<?= $v ?>"></script>
<script src="assets/help.js?v=<?= $v ?>"></script>
<script src="assets/ingame.js?v=<?= $v ?>"></script>
<?php endif; ?>
</body>
</html>
