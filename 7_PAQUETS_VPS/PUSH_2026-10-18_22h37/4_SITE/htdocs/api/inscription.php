<?php
// Mystic : inscription
// GET  inscription.php           -> jeton signé du formulaire (anti-robot : délai minimal)
// POST inscription.php (JSON)    -> crée le compte dans la base auth
declare(strict_types=1);
require __DIR__ . '/commun.php';

// MYSTIC 18/10 : les comptes de jeu se créent désormais depuis un compte web (compte_web.php?a=creer_jeu)
erreur('Crée d\'abord ton compte web, puis tes comptes de jeu depuis « Mon compte ».', 410);
$cfg = reglages(); $ins = $cfg['inscription'];
if (!$ins['ouverte']) erreur('Les inscriptions sont fermées pour le moment.', 403);

function signe(string $v): string { return hash_hmac('sha256', $v, reglages()['secret']); }

if ($_SERVER['REQUEST_METHOD'] === 'GET') {
    $t = (string) time();
    json(['ok' => true, 'jeton' => $t . '.' . signe($t)]);
}
if ($_SERVER['REQUEST_METHOD'] !== 'POST') erreur('Méthode non autorisée.', 405);

$d = json_decode((string) file_get_contents('php://input'), true);
if (!is_array($d)) erreur('Formulaire illisible.');

// anti-robot : champ piège vide + jeton signé + délai minimal
if (!empty($d['site_web'])) erreur('Inscription refusée.');
[$t, $sig] = array_pad(explode('.', (string) ($d['jeton'] ?? ''), 2), 2, '');
if (!ctype_digit($t) || !hash_equals(signe($t), $sig)) erreur('Le formulaire a expiré, recharge la page.');
$age = time() - (int) $t;
if ($age < (int) $ins['delai_min_s']) erreur('Doucement : vérifie tes informations puis réessaie.');
if ($age > 3600) erreur('Le formulaire a expiré, recharge la page.');

$login   = trim((string) ($d['login'] ?? ''));
$pseudo  = trim((string) ($d['pseudo'] ?? ''));
$mdp     = (string) ($d['mot_de_passe'] ?? '');
$mdp2    = (string) ($d['confirmation'] ?? '');
$email   = trim((string) ($d['email'] ?? ''));
$reponse = trim((string) ($d['reponse_secrete'] ?? ''));

$erreurs = [];
if (!preg_match('/^[a-zA-Z0-9_-]{3,20}$/', $login)) $erreurs['login'] = 'De 3 à 20 caractères : lettres, chiffres, - ou _.';
if (!preg_match('/^[a-zA-Z][a-zA-Z0-9-]{2,19}$/', $pseudo)) $erreurs['pseudo'] = 'De 3 à 20 caractères, en commençant par une lettre.';
if (strcasecmp($login, $pseudo) === 0) $erreurs['pseudo'] = 'Le pseudo doit être différent du nom de compte.';
if (mb_strlen($mdp) < 8) $erreurs['mot_de_passe'] = 'Au moins 8 caractères.';
elseif (strcasecmp($mdp, $login) === 0) $erreurs['mot_de_passe'] = 'Le mot de passe ne doit pas être le nom de compte.';
if ($mdp !== $mdp2) $erreurs['confirmation'] = 'Les deux mots de passe ne sont pas identiques.';
if (!filter_var($email, FILTER_VALIDATE_EMAIL)) $erreurs['email'] = 'Adresse e-mail invalide.';
if (mb_strlen($reponse) < 2 || mb_strlen($reponse) > 40) $erreurs['reponse_secrete'] = 'De 2 à 40 caractères.';
if ($erreurs) json(['ok' => false, 'erreur' => 'Corrige les champs en rouge.', 'champs' => $erreurs], 422);

// limite par adresse IP
$f = __DIR__ . '/cache/inscriptions_ip.json';
if (!is_dir(dirname($f))) @mkdir(dirname($f), 0775, true);
$ips = is_file($f) ? (json_decode((string) file_get_contents($f), true) ?: []) : [];
$ip = ip_client(); $maintenant = time();
$ips = array_filter(array_map(fn($l) => array_values(array_filter($l, fn($x) => $x > $maintenant - 3600)), $ips));
if (count($ips[$ip] ?? []) >= (int) $ins['max_par_ip_heure']) erreur('Trop de comptes créés depuis ta connexion. Réessaie dans une heure.', 429);

try {
    $pdo = bdd(); $A = base('auth');
    $st = $pdo->prepare("SELECT LOWER(Login) AS l, LOWER(Nickname) AS n FROM $A.accounts WHERE LOWER(Login) = LOWER(?) OR LOWER(Nickname) = LOWER(?) OR LOWER(Email) = LOWER(?)");
    $st->execute([$login, $pseudo, $email]);
    $champs = [];
    foreach ($st->fetchAll() as $r) {
        if ($r['l'] === strtolower($login)) $champs['login'] = 'Ce nom de compte est déjà pris.';
        if ($r['n'] === strtolower($pseudo)) $champs['pseudo'] = 'Ce pseudo est déjà pris.';
    }
    if (!$champs) {
        $st = $pdo->prepare("SELECT COUNT(*) FROM $A.accounts WHERE LOWER(Email) = LOWER(?)"); $st->execute([$email]);
        if ((int) $st->fetchColumn() > 0) $champs['email'] = 'Cette adresse est déjà utilisée.';
    }
    if ($champs) json(['ok' => false, 'erreur' => 'Corrige les champs en rouge.', 'champs' => $champs], 409);

    // le serveur d'authentification accepte le mot de passe en MD5 (CredentialManager)
    $st = $pdo->prepare("INSERT INTO $A.accounts (Login, PasswordHash, Nickname, Role, SecretQuestion, SecretAnswer, Lang, Email, CreationDate, Tokens, NewTokens, Time, IsJailed, IsBanned, last_ip_web)
                         VALUES (?, ?, ?, 1, 'DELETE?', ?, 'fr', ?, NOW(), 0, 0, '0', 0, 0, ?)");
    $st->execute([$login, md5($mdp), $pseudo, $reponse, $email, $ip]);
} catch (Throwable $e) {
    erreur("L'inscription est indisponible pour le moment. Réessaie plus tard.", 503);
}

$ips[$ip][] = $maintenant;
@file_put_contents($f, json_encode($ips), LOCK_EX);
json(['ok' => true, 'message' => "Compte créé ! Tu peux te connecter en jeu avec le nom de compte « $login »."]);
