<?php
// Mystic : statut du serveur (ports d'authentification et de jeu, chiffres de la base)
declare(strict_types=1);
require __DIR__ . '/commun.php';

function port_ouvert(string $hote, int $port): bool {
    $s = @fsockopen($hote, $port, $errno, $errstr, 1.5);
    if ($s) { fclose($s); return true; }
    return false;
}

$cfg = reglages();
$etat = en_cache('statut', function () use ($cfg) {
    $sv = $cfg['serveur'];
    $r = ['auth' => port_ouvert($sv['hote'], (int) $sv['port_auth']), 'monde' => port_ouvert($sv['hote'], (int) $sv['port_monde'])];
    try {
        $A = base('auth'); $W = base('monde'); $pdo = bdd();
        $r['comptes'] = (int) $pdo->query("SELECT COUNT(*) FROM $A.accounts")->fetchColumn();
        $r['personnages'] = (int) $pdo->query("SELECT COUNT(*) FROM $W.characters")->fetchColumn();
        $r['actifs_24h'] = (int) $pdo->query("SELECT COUNT(*) FROM $W.characters WHERE LastUsage > NOW() - INTERVAL 1 DAY")->fetchColumn();
        $r['guildes'] = (int) $pdo->query("SELECT COUNT(*) FROM $W.guilds")->fetchColumn();
        $r['bonta'] = (int) $pdo->query("SELECT COUNT(*) FROM $W.characters WHERE AlignmentSide = 1")->fetchColumn();
        $r['brakmar'] = (int) $pdo->query("SELECT COUNT(*) FROM $W.characters WHERE AlignmentSide = 2")->fetchColumn();
        $r['bdd'] = true;
    } catch (Throwable $e) { $r['bdd'] = false; }
    $r['verifie_a'] = date('H:i');
    return $r;
});
json(['ok' => true] + $etat);
