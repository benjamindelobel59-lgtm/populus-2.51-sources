<?php
// Mystic : fonctions communes de l'API (réglages, base de données, cache, réponses JSON)
declare(strict_types=1);

function reglages(): array {
    static $c = null;
    if ($c === null) {
        $c = require __DIR__ . '/config.php';
        if (is_file(__DIR__ . '/config.local.php')) $c = array_replace_recursive($c, require __DIR__ . '/config.local.php');
    }
    return $c;
}

function bdd(): PDO {
    static $pdo = null;
    if ($pdo === null) {
        $b = reglages()['bdd'];
        $pdo = new PDO("mysql:host={$b['hote']};port={$b['port']};charset=utf8", $b['utilisateur'], $b['mot_de_passe'], [
            PDO::ATTR_ERRMODE => PDO::ERRMODE_EXCEPTION,
            PDO::ATTR_DEFAULT_FETCH_MODE => PDO::FETCH_ASSOC,
            PDO::ATTR_EMULATE_PREPARES => false,
            PDO::ATTR_TIMEOUT => 3,
        ]);
    }
    return $pdo;
}

function base(string $quoi): string {
    $n = reglages()['bdd'][$quoi === 'auth' ? 'base_auth' : 'base_monde'];
    return '`' . str_replace('`', '', $n) . '`';
}

function json(array $donnees, int $code = 200) {
    http_response_code($code);
    header('Content-Type: application/json; charset=utf-8');
    header('Cache-Control: no-store');
    echo json_encode($donnees, JSON_UNESCAPED_UNICODE | JSON_UNESCAPED_SLASHES);
    exit;
}

function erreur(string $message, int $code = 400) { json(['ok' => false, 'erreur' => $message], $code); }

// cache fichier très simple (évite de solliciter la base à chaque visite)
function en_cache(string $cle, callable $calcul) {
    $dossier = __DIR__ . '/cache';
    if (!is_dir($dossier)) @mkdir($dossier, 0775, true);
    $f = $dossier . '/' . preg_replace('/[^a-z0-9_-]/i', '_', $cle) . '.json';
    $duree = (int) reglages()['cache_s'];
    if (is_file($f) && filemtime($f) > time() - $duree) {
        $v = json_decode((string) file_get_contents($f), true);
        if (is_array($v)) return $v;
    }
    $v = $calcul();
    @file_put_contents($f, json_encode($v, JSON_UNESCAPED_UNICODE), LOCK_EX);
    return $v;
}

function ip_client(): string { return $_SERVER['REMOTE_ADDR'] ?? '0.0.0.0'; }

const CLASSES = [1=>'Féca',2=>'Osamodas',3=>'Enutrof',4=>'Sram',5=>'Xélor',6=>'Ecaflip',7=>'Eniripsa',8=>'Iop',9=>'Crâ',10=>'Sadida',
    11=>'Sacrieur',12=>'Pandawa',13=>'Roublard',14=>'Zobal',15=>'Steamer',16=>'Eliotrope',17=>'Huppermage',18=>'Ouginak'];
const CAMPS = [0=>'Neutre',1=>'Bonta',2=>'Brâkmar',3=>'Mercenaire'];

// adresse du rendu officiel d'un personnage à partir de sa chaîne d'apparence
function avatar(?string $look): ?string {
    if (!$look) return null;
    return 'https://static.ankama.com/dofus/renderer/look/' . bin2hex($look) . '/face/1/60_60-0.png';
}
