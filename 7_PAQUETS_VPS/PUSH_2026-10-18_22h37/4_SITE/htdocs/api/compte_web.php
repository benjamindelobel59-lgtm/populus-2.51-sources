<?php
// Mystic : compte web (regroupe les comptes de jeu ; le launcher s'y connecte aussi, via l'AuthServer).
//   GET  compte_web.php?a=jeton        -> jeton signé du formulaire d'inscription (anti-robot : délai minimal)
//   GET  compte_web.php?a=moi          -> compte web connecté + comptes de jeu liés et leurs personnages
//   POST compte_web.php?a=inscription  {email, pseudo, mot_de_passe, confirmation, jeton, site_web}
//   POST compte_web.php?a=connexion    {email, mot_de_passe}
//   POST compte_web.php?a=deconnexion
//   POST compte_web.php?a=creer_jeu    {login, pseudo, mot_de_passe, confirmation, reponse_secrete} -> compte de jeu lié
//   POST compte_web.php?a=rattacher    {login, mot_de_passe} -> rattache un compte de jeu existant (mot de passe du jeu vérifié)
// Sécurité : requêtes préparées uniquement, session PHP HttpOnly + SameSite=Strict, en-tête X-Mystic obligatoire sur les POST
// (anti-CSRF : impossible à envoyer depuis un autre site sans CORS), limite d'essais par IP / e-mail, mots de passe web en
// PBKDF2-SHA256 (même format vérifié par l'AuthServer : Launcher/LauncherApiWeb.cs).
declare(strict_types=1);
require __DIR__ . '/commun.php';

const MAX_COMPTES_JEU = 8;
const ITERATIONS = 120000;

session_name('mystic_web');
session_set_cookie_params(['lifetime' => 0, 'path' => '/', 'httponly' => true, 'samesite' => 'Strict',
    'secure' => !empty($_SERVER['HTTPS']) && $_SERVER['HTTPS'] !== 'off']);
session_start();

$a = (string) ($_GET['a'] ?? '');
$methode = $_SERVER['REQUEST_METHOD'];

function signe(string $v): string { return hash_hmac('sha256', 'web|' . $v, reglages()['secret']); }

function hacher(string $mdp): string {
    $sel = random_bytes(16);
    return 'pbkdf2_sha256$' . ITERATIONS . '$' . base64_encode($sel) . '$' . base64_encode(hash_pbkdf2('sha256', $mdp, $sel, ITERATIONS, 32, true));
}

function verifier(string $mdp, string $stocke): bool {
    $p = explode('$', $stocke);
    if (count($p) !== 4 || $p[0] !== 'pbkdf2_sha256') return false;
    $it = (int) $p[1];
    if ($it < 10000 || $it > 2000000) return false;
    $attendu = base64_decode($p[3], true); $sel = base64_decode($p[2], true);
    if ($attendu === false || $sel === false) return false;
    return hash_equals($attendu, hash_pbkdf2('sha256', $mdp, $sel, $it, strlen($attendu), true));
}

function tables(PDO $pdo): void {
    $A = base('auth');
    $pdo->exec("CREATE TABLE IF NOT EXISTS $A.nekfeu_comptes_web (
        Id INT NOT NULL AUTO_INCREMENT PRIMARY KEY, Email VARCHAR(190) NOT NULL, Pseudo VARCHAR(40) NOT NULL,
        MotDePasse VARCHAR(255) NOT NULL, CreeLe DATETIME NOT NULL, DerniereConnexion DATETIME NULL,
        Bloque TINYINT NOT NULL DEFAULT 0, UNIQUE KEY (Email), UNIQUE KEY (Pseudo)) DEFAULT CHARSET=utf8");
    $pdo->exec("CREATE TABLE IF NOT EXISTS $A.nekfeu_comptes_web_jeu (
        AccountId INT NOT NULL PRIMARY KEY, CompteWebId INT NOT NULL, LieLe DATETIME NOT NULL, KEY (CompteWebId)) DEFAULT CHARSET=utf8");
}

// limite d'essais (fichier de cache, comme l'inscription)
function limite(string $quoi, string $cle, int $max, int $fenetre, bool $compter): bool {
    $f = __DIR__ . '/cache/limite_' . $quoi . '.json';
    if (!is_dir(dirname($f))) @mkdir(dirname($f), 0775, true);
    $d = is_file($f) ? (json_decode((string) file_get_contents($f), true) ?: []) : [];
    $t = time();
    $d = array_filter(array_map(fn($l) => array_values(array_filter($l, fn($x) => $x > $t - $fenetre)), $d));
    $depasse = count($d[$cle] ?? []) >= $max;
    if ($compter) { $d[$cle][] = $t; @file_put_contents($f, json_encode($d), LOCK_EX); }
    return $depasse;
}

function corps(): array {
    if (($_SERVER['HTTP_X_MYSTIC'] ?? '') !== '1') erreur('Requête refusée.', 403);
    $d = json_decode((string) file_get_contents('php://input'), true);
    if (!is_array($d)) erreur('Formulaire illisible.');
    return $d;
}

function connecte(): int { return (int) ($_SESSION['web_id'] ?? 0); }

try {
    $pdo = bdd(); $A = base('auth'); $M = base('monde');
    tables($pdo);

    if ($methode === 'GET' && $a === 'jeton') {
        $t = (string) time();
        json(['ok' => true, 'jeton' => $t . '.' . signe($t)]);
    }

    if ($methode === 'GET' && $a === 'moi') {
        $id = connecte();
        if (!$id) json(['ok' => true, 'connecte' => false]);
        $st = $pdo->prepare("SELECT Email, Pseudo FROM $A.nekfeu_comptes_web WHERE Id = ? AND Bloque = 0"); $st->execute([$id]);
        $w = $st->fetch();
        if (!$w) { $_SESSION = []; session_destroy(); json(['ok' => true, 'connecte' => false]); }
        $st = $pdo->prepare("SELECT a.Id, a.Login, a.Nickname FROM $A.nekfeu_comptes_web_jeu l JOIN $A.accounts a ON a.Id = l.AccountId WHERE l.CompteWebId = ? ORDER BY l.LieLe");
        $st->execute([$id]);
        $comptes = [];
        $perso = $pdo->prepare("SELECT c.Name, c.Breed, c.Sex, IFNULL(NULLIF(c.LastLookString, ''), c.DefaultLookString) AS Look,
            (SELECT IFNULL(MAX(e.Level), 1) FROM $M.experiences e WHERE e.CharacterExp <= c.Experience) AS Niveau
            FROM $M.characters c WHERE (c.AccountId = ? OR c.Id IN (SELECT w.CharacterId FROM $A.worlds_characters w WHERE w.AccountId = ?))
            AND c.DeletedDate IS NULL ORDER BY c.LastUsage DESC LIMIT 20");
        foreach ($st->fetchAll() as $c) {
            $perso->execute([(int) $c['Id'], (int) $c['Id']]);
            $comptes[] = ['login' => $c['Login'], 'pseudo' => $c['Nickname'], 'personnages' => array_map(fn($p) => [
                'nom' => $p['Name'], 'classe' => CLASSES[(int) $p['Breed']] ?? '?', 'niveau' => min(200, (int) $p['Niveau']),
                'avatar' => avatar($p['Look'])], $perso->fetchAll())];
        }
        json(['ok' => true, 'connecte' => true, 'email' => $w['Email'], 'pseudo' => $w['Pseudo'], 'comptes' => $comptes, 'max' => MAX_COMPTES_JEU]);
    }

    if ($methode !== 'POST') erreur('Méthode non autorisée.', 405);
    $ip = ip_client();

    if ($a === 'inscription') {
        $d = corps();
        if (!empty($d['site_web'])) erreur('Inscription refusée.');
        [$t, $sig] = array_pad(explode('.', (string) ($d['jeton'] ?? ''), 2), 2, '');
        if (!ctype_digit($t) || !hash_equals(signe($t), $sig)) erreur('Le formulaire a expiré, recharge la page.');
        $age = time() - (int) $t;
        if ($age < (int) reglages()['inscription']['delai_min_s']) erreur('Doucement : vérifie tes informations puis réessaie.');
        if ($age > 3600) erreur('Le formulaire a expiré, recharge la page.');
        if (!reglages()['inscription']['ouverte']) erreur('Les inscriptions sont fermées pour le moment.', 403);

        $email = trim((string) ($d['email'] ?? '')); $pseudo = trim((string) ($d['pseudo'] ?? ''));
        $mdp = (string) ($d['mot_de_passe'] ?? ''); $mdp2 = (string) ($d['confirmation'] ?? '');
        $e = [];
        if (!filter_var($email, FILTER_VALIDATE_EMAIL) || strlen($email) > 190) $e['email'] = 'Adresse e-mail invalide.';
        if (!preg_match('/^[a-zA-Z][a-zA-Z0-9-]{2,19}$/', $pseudo)) $e['pseudo'] = 'De 3 à 20 caractères, en commençant par une lettre.';
        if (mb_strlen($mdp) < 8) $e['mot_de_passe'] = 'Au moins 8 caractères.';
        elseif (strcasecmp($mdp, $email) === 0) $e['mot_de_passe'] = "Le mot de passe ne doit pas être l'e-mail.";
        if ($mdp !== $mdp2) $e['confirmation'] = 'Les deux mots de passe ne sont pas identiques.';
        if ($e) json(['ok' => false, 'erreur' => 'Corrige les champs en rouge.', 'champs' => $e], 422);
        if (limite('inscription_web', $ip, (int) reglages()['inscription']['max_par_ip_heure'], 3600, false))
            erreur('Trop de comptes créés depuis ta connexion. Réessaie dans une heure.', 429);

        $st = $pdo->prepare("SELECT LOWER(Email) e, LOWER(Pseudo) p FROM $A.nekfeu_comptes_web WHERE LOWER(Email) = LOWER(?) OR LOWER(Pseudo) = LOWER(?)");
        $st->execute([$email, $pseudo]);
        foreach ($st->fetchAll() as $r) {
            if ($r['e'] === strtolower($email)) $e['email'] = 'Cette adresse est déjà utilisée.';
            if ($r['p'] === strtolower($pseudo)) $e['pseudo'] = 'Ce pseudo est déjà pris.';
        }
        if ($e) json(['ok' => false, 'erreur' => 'Corrige les champs en rouge.', 'champs' => $e], 409);

        $pdo->prepare("INSERT INTO $A.nekfeu_comptes_web (Email, Pseudo, MotDePasse, CreeLe) VALUES (?, ?, ?, NOW())")
            ->execute([$email, $pseudo, hacher($mdp)]);
        limite('inscription_web', $ip, 0, 3600, true);
        session_regenerate_id(true);
        $_SESSION['web_id'] = (int) $pdo->lastInsertId();
        json(['ok' => true, 'message' => 'Compte web créé ! Crée maintenant ton premier compte de jeu.']);
    }

    if ($a === 'connexion') {
        $d = corps();
        $email = trim((string) ($d['email'] ?? '')); $mdp = (string) ($d['mot_de_passe'] ?? '');
        if ($email === '' || $mdp === '') erreur('Indique ton e-mail et ton mot de passe.');
        if (limite('connexion_ip', $ip, 10, 600, false) || limite('connexion_email', strtolower($email), 10, 900, false))
            erreur('Trop de tentatives. Réessaie dans quelques minutes.', 429);
        $st = $pdo->prepare("SELECT Id, MotDePasse, Bloque FROM $A.nekfeu_comptes_web WHERE Email = ?"); $st->execute([$email]);
        $w = $st->fetch();
        if (!$w || !verifier($mdp, (string) $w['MotDePasse'])) {
            limite('connexion_ip', $ip, 0, 600, true); limite('connexion_email', strtolower($email), 0, 900, true);
            erreur('E-mail ou mot de passe incorrect.', 401);
        }
        if ((int) $w['Bloque']) erreur('Ce compte web est bloqué.', 403);
        session_regenerate_id(true);
        $_SESSION['web_id'] = (int) $w['Id'];
        $pdo->prepare("UPDATE $A.nekfeu_comptes_web SET DerniereConnexion = NOW() WHERE Id = ?")->execute([(int) $w['Id']]);
        json(['ok' => true]);
    }

    if ($a === 'deconnexion') {
        corps();
        $_SESSION = []; session_destroy();
        json(['ok' => true]);
    }

    if ($a === 'creer_jeu') {
        $d = corps();
        $id = connecte();
        if (!$id) erreur('Connecte-toi à ton compte web.', 401);
        $login = trim((string) ($d['login'] ?? '')); $pseudo = trim((string) ($d['pseudo'] ?? ''));
        $mdp = (string) ($d['mot_de_passe'] ?? ''); $mdp2 = (string) ($d['confirmation'] ?? '');
        $reponse = trim((string) ($d['reponse_secrete'] ?? ''));
        $e = [];
        if (!preg_match('/^[a-zA-Z0-9_-]{3,20}$/', $login)) $e['login'] = 'De 3 à 20 caractères : lettres, chiffres, - ou _.';
        if (!preg_match('/^[a-zA-Z][a-zA-Z0-9-]{2,19}$/', $pseudo)) $e['pseudo'] = 'De 3 à 20 caractères, en commençant par une lettre.';
        if (strcasecmp($login, $pseudo) === 0) $e['pseudo'] = 'Le pseudo doit être différent du nom de compte.';
        if (mb_strlen($mdp) < 8) $e['mot_de_passe'] = 'Au moins 8 caractères.';
        elseif (strcasecmp($mdp, $login) === 0) $e['mot_de_passe'] = 'Le mot de passe ne doit pas être le nom de compte.';
        if ($mdp !== $mdp2) $e['confirmation'] = 'Les deux mots de passe ne sont pas identiques.';
        if (mb_strlen($reponse) < 2 || mb_strlen($reponse) > 40) $e['reponse_secrete'] = 'De 2 à 40 caractères.';
        if ($e) json(['ok' => false, 'erreur' => 'Corrige les champs en rouge.', 'champs' => $e], 422);

        $st = $pdo->prepare("SELECT COUNT(*) FROM $A.nekfeu_comptes_web_jeu WHERE CompteWebId = ?"); $st->execute([$id]);
        if ((int) $st->fetchColumn() >= MAX_COMPTES_JEU) erreur('Tu as déjà ' . MAX_COMPTES_JEU . ' comptes de jeu.', 409);
        $st = $pdo->prepare("SELECT LOWER(Login) l, LOWER(Nickname) n FROM $A.accounts WHERE LOWER(Login) = LOWER(?) OR LOWER(Nickname) = LOWER(?)");
        $st->execute([$login, $pseudo]);
        foreach ($st->fetchAll() as $r) {
            if ($r['l'] === strtolower($login)) $e['login'] = 'Ce nom de compte est déjà pris.';
            if ($r['n'] === strtolower($pseudo)) $e['pseudo'] = 'Ce pseudo est déjà pris.';
        }
        if ($e) json(['ok' => false, 'erreur' => 'Corrige les champs en rouge.', 'champs' => $e], 409);

        $st = $pdo->prepare("SELECT Email FROM $A.nekfeu_comptes_web WHERE Id = ?"); $st->execute([$id]);
        $email = (string) $st->fetchColumn();
        $pdo->beginTransaction();
        // le serveur d'authentification accepte le mot de passe en MD5 (CredentialManager), comme l'ancienne inscription
        $pdo->prepare("INSERT INTO $A.accounts (Login, PasswordHash, Nickname, Role, SecretQuestion, SecretAnswer, Lang, Email, CreationDate, Tokens, NewTokens, Time, IsJailed, IsBanned, last_ip_web)
                       VALUES (?, ?, ?, 1, 'DELETE?', ?, 'fr', ?, NOW(), 0, 0, '0', 0, 0, ?)")
            ->execute([$login, md5($mdp), $pseudo, $reponse, $email, $ip]);
        $compte = (int) $pdo->lastInsertId();
        $pdo->prepare("INSERT INTO $A.nekfeu_comptes_web_jeu (AccountId, CompteWebId, LieLe) VALUES (?, ?, NOW())")->execute([$compte, $id]);
        $pdo->commit();
        json(['ok' => true, 'message' => "Compte de jeu « $login » créé ! Il apparaît aussi dans le launcher."]);
    }

    // MYSTIC 18/10 : rattacher un compte de jeu qui existait avant les comptes web (identifiant + mot de passe du jeu)
    if ($a === 'rattacher') {
        $d = corps();
        $id = connecte();
        if (!$id) erreur('Connecte-toi à ton compte web.', 401);
        $login = trim((string) ($d['login'] ?? '')); $mdp = (string) ($d['mot_de_passe'] ?? '');
        if ($login === '' || $mdp === '' || strlen($login) > 50 || strlen($mdp) > 100) erreur("Indique le nom de compte et le mot de passe du jeu.");
        // essais limités : par IP, par compte de jeu visé et par compte web
        if (limite('rattacher_ip', $ip, 8, 900, false) || limite('rattacher_login', strtolower($login), 6, 1800, false) || limite('rattacher_web', (string) $id, 10, 1800, false))
            erreur('Trop de tentatives. Réessaie dans quelques minutes.', 429);
        $echec = function () use ($ip, $login, $id) {
            limite('rattacher_ip', $ip, 0, 900, true); limite('rattacher_login', strtolower($login), 0, 1800, true); limite('rattacher_web', (string) $id, 0, 1800, true);
            erreur('Nom de compte ou mot de passe incorrect.', 401);
        };
        $st = $pdo->prepare("SELECT Id, Login, PasswordHash, IsBanned, BanEndDate FROM $A.accounts WHERE Login = ? LIMIT 1"); $st->execute([$login]);
        $c = $st->fetch();
        // même règle que le serveur (CredentialManager) : mot de passe en clair, ou ancien hachage MD5
        $h = (string) ($c['PasswordHash'] ?? '');
        if (!$c || $h === '' || !(hash_equals($h, $mdp) || hash_equals(strtolower($h), md5($mdp)))) $echec();
        if ((int) $c['IsBanned'] && ($c['BanEndDate'] === null || strtotime((string) $c['BanEndDate']) > time())) erreur('Ce compte de jeu est banni : il ne peut pas être rattaché.', 403);
        $st = $pdo->prepare("SELECT CompteWebId FROM $A.nekfeu_comptes_web_jeu WHERE AccountId = ?"); $st->execute([(int) $c['Id']]);
        $deja = $st->fetchColumn();
        if ($deja !== false) erreur((int) $deja === $id ? 'Ce compte de jeu est déjà dans ton compte web.' : 'Ce compte de jeu est déjà rattaché à un autre compte web.', 409);
        $st = $pdo->prepare("SELECT COUNT(*) FROM $A.nekfeu_comptes_web_jeu WHERE CompteWebId = ?"); $st->execute([$id]);
        if ((int) $st->fetchColumn() >= MAX_COMPTES_JEU) erreur('Tu as déjà ' . MAX_COMPTES_JEU . ' comptes de jeu.', 409);
        try {
            $pdo->prepare("INSERT INTO $A.nekfeu_comptes_web_jeu (AccountId, CompteWebId, LieLe) VALUES (?, ?, NOW())")->execute([(int) $c['Id'], $id]);
        } catch (PDOException $x) { erreur('Ce compte de jeu vient d\'être rattaché ailleurs.', 409); }
        // un partage reçu pour ce même compte devient inutile
        try { $pdo->prepare("DELETE FROM $A.nekfeu_partages WHERE AccountId = ? AND CompteWebId = ?")->execute([(int) $c['Id'], $id]); } catch (Throwable $x) { }
        json(['ok' => true, 'message' => "Compte de jeu « {$c['Login']} » rattaché ! Il apparaît aussi dans le launcher."]);
    }

    erreur('Action inconnue.', 404);
} catch (Throwable $e) {
    if (isset($pdo) && $pdo->inTransaction()) $pdo->rollBack();
    erreur("Le compte web est indisponible pour le moment. Réessaie plus tard.", 503);
}
