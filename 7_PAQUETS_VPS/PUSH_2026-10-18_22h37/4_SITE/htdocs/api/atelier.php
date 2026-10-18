<?php
// MYSTIC 18/10/2026 : « Lier mon perso » de l'Atelier (lecture seule).
//   GET atelier.php?a=persos          -> personnages des comptes de jeu du compte web connecté (possédés + partagés)
//   GET atelier.php?a=perso&id=<id>   -> un personnage : caracs de base, parchos, équipement (jets réels), inventaire, banque du compte
// Sécurité : session du compte web (même cookie que compte_web.php), en-tête X-Mystic obligatoire, requêtes préparées,
// le personnage demandé doit appartenir à un compte de jeu lié (ou partagé) à ce compte web. Aucune écriture.
declare(strict_types=1);
require __DIR__ . '/commun.php';

session_name('mystic_web');
session_set_cookie_params(['lifetime' => 0, 'path' => '/', 'httponly' => true, 'samesite' => 'Strict',
    'secure' => !empty($_SERVER['HTTPS']) && $_SERVER['HTTPS'] !== 'off']);
session_start();
session_write_close(); // lecture seule : on libère la session tout de suite

if ($_SERVER['REQUEST_METHOD'] !== 'GET') erreur('Méthode non autorisée.', 405);
if (($_SERVER['HTTP_X_MYSTIC'] ?? '') !== '1') erreur('Requête refusée.', 403);
$web = (int) ($_SESSION['web_id'] ?? 0);
if (!$web) erreur('Connecte-toi à ton compte Mystic.', 401);

// positions d'équipement du serveur (CharacterInventoryPositionEnum) -> emplacements de l'Atelier
const POSITIONS = [6 => 'coiffe', 7 => 'cape', 0 => 'amulette', 2 => 'anneau1', 3 => 'ceinture', 1 => 'arme', 15 => 'bouclier', 4 => 'anneau2',
    5 => 'bottes', 8 => 'familier', 9 => 'dofus1', 10 => 'dofus2', 11 => 'dofus3', 12 => 'dofus4', 13 => 'dofus5', 14 => 'dofus6'];
const NON_EQUIPE = 63;

/** Lit les effets sérialisés d'un objet (format Stump : EffectManager.SerializeEffects) -> [[effet, valeur]…]. */
function effets(?string $b): array {
    $r = [];
    if ($b === null || $b === '') return $r;
    $n = strlen($b); $i = 0;
    $u8 = function () use ($b, &$i) { return ord($b[$i++]); };
    $i16 = function () use ($b, &$i) { $v = unpack('v', substr($b, $i, 2))[1]; $i += 2; return $v >= 0x8000 ? $v - 0x10000 : $v; };
    $i32 = function () use ($b, &$i) { $v = unpack('V', substr($b, $i, 4))[1]; $i += 4; return $v >= 0x80000000 ? $v - 0x100000000 : $v; };
    $str = function () use ($b, &$i, $u8) { $l = 0; $s = 0; do { $c = $u8(); $l |= ($c & 0x7f) << $s; $s += 7; } while ($c & 0x80); $v = substr($b, $i, $l); $i += $l; return $v; };
    try {
        while ($i + 1 < $n) {
            $type = $u8();
            $entete = chr($u8());
            if ($entete === 'C') $id = $i16();
            elseif ($entete === 'F') {
                // masque, id, uid + durée + délai + aléa + groupe + modificateur, déclencheur, déclencheurs, caché,
                // zone, ordre, sort, élément, désenvoûtable, client seulement (EffectBase.InternalDeserialize)
                $str(); $id = $i16(); $i += 4 * 6; $i += 1; $str(); $i += 1;
                $str(); $i += 4 * 4; $i += 1;
            } else break;
            $v = 0;
            switch ($type) {
                case 1: break;                                  // EffectBase
                case 2: $v = $i16(); break;                     // EffectCreature
                case 3: $i += 10; break;                        // EffectDate
                case 4: $v = $i32(); $a = $i32(); $c = $i32(); if (!$v) $v = $a; break; // EffectDice (value, dicenum, diceface)
                case 5: $i += 6; break;                         // EffectDuration
                case 6: $v = $i32(); break;                     // EffectInteger
                case 7: $v = $i16(); $i16(); break;             // EffectLadder
                case 8: $max = $i16(); $min = $i16(); $v = $max; break; // EffectMinMax
                case 10: $str(); break;                         // EffectString
                default: return $r;                             // EffectMount et inconnus : on s'arrête là
            }
            $r[] = [$id, $v];
        }
    } catch (Throwable $e) { }
    return $r;
}

try {
    $pdo = bdd(); $A = base('auth'); $M = base('monde');
    // comptes de jeu du compte web : possédés + reçus en partage
    $st = $pdo->prepare("SELECT l.AccountId, a.Login, 0 AS Partage FROM $A.nekfeu_comptes_web_jeu l JOIN $A.accounts a ON a.Id = l.AccountId WHERE l.CompteWebId = ?
        UNION SELECT p.AccountId, a.Login, 1 FROM $A.nekfeu_partages p JOIN $A.accounts a ON a.Id = p.AccountId WHERE p.CompteWebId = ?");
    try { $st->execute([$web, $web]); $comptes = $st->fetchAll(); }
    catch (Throwable $e) { // table des partages absente (AuthServer pas encore à jour)
        $st = $pdo->prepare("SELECT l.AccountId, a.Login, 0 AS Partage FROM $A.nekfeu_comptes_web_jeu l JOIN $A.accounts a ON a.Id = l.AccountId WHERE l.CompteWebId = ?");
        $st->execute([$web]); $comptes = $st->fetchAll();
    }
    $ids = array_map(fn($c) => (int) $c['AccountId'], $comptes);
    if (!$ids) json(['ok' => true, 'persos' => []]);
    $place = implode(',', array_fill(0, count($ids), '?'));
    $niveau = "(SELECT IFNULL(MAX(e.Level), 1) FROM $M.experiences e WHERE e.CharacterExp <= c.Experience)";
    // un personnage appartient à un compte par auth.worlds_characters (ou characters.AccountId sur les anciennes bases)
    $st = $pdo->prepare("SELECT CharacterId, AccountId FROM $A.worlds_characters WHERE AccountId IN ($place)");
    $st->execute($ids);
    $proprio = [];
    foreach ($st->fetchAll() as $w) $proprio[(int) $w['CharacterId']] = (int) $w['AccountId'];
    $st = $pdo->prepare("SELECT Id, AccountId FROM $M.characters WHERE AccountId IN ($place) AND DeletedDate IS NULL");
    $st->execute($ids);
    foreach ($st->fetchAll() as $w) $proprio[(int) $w['Id']] ??= (int) $w['AccountId'];
    if (!$proprio) json(['ok' => true, 'persos' => []]);
    $placeP = implode(',', array_fill(0, count($proprio), '?'));

    $logins = array_column($comptes, 'Login', 'AccountId');
    $a = (string) ($_GET['a'] ?? '');
    if ($a === 'persos') {
        $st = $pdo->prepare("SELECT c.Id, c.Name, c.Breed, c.Sex, $niveau AS Niveau, IFNULL(NULLIF(c.LastLookString, ''), c.DefaultLookString) AS Look FROM $M.characters c
            WHERE c.DeletedDate IS NULL AND c.Id IN ($placeP) ORDER BY c.LastUsage DESC LIMIT 60");
        $st->execute(array_keys($proprio));
        json(['ok' => true, 'persos' => array_map(fn($p) => ['id' => (int) $p['Id'], 'nom' => $p['Name'], 'classe' => (int) $p['Breed'], 'sexe' => (int) $p['Sex'],
            'niveau' => min(200, (int) $p['Niveau']), 'compte' => $logins[$proprio[(int) $p['Id']]] ?? '', 'look' => (string) $p['Look']], $st->fetchAll())]);
    }

    if ($a === 'perso') {
        $id = (int) ($_GET['id'] ?? 0);
        if (!isset($proprio[$id])) erreur('Personnage introuvable sur tes comptes.', 404);
        $st = $pdo->prepare("SELECT c.Id, c.Name, c.Breed, c.Sex, $niveau AS Niveau, IFNULL(NULLIF(c.LastLookString, ''), c.DefaultLookString) AS Look, c.Strength, c.Chance, c.Vitality, c.Wisdom, c.Intelligence, c.Agility,
            c.PermanentAddedStrength, c.PermanentAddedChance, c.PermanentAddedVitality, c.PermanentAddedWisdom, c.PermanentAddedIntelligence, c.PermanentAddedAgility, c.Kamas
            FROM $M.characters c WHERE c.Id = ? AND c.DeletedDate IS NULL");
        $st->execute([$id]);
        $p = $st->fetch();
        if (!$p) erreur('Personnage introuvable sur tes comptes.', 404);

        $st = $pdo->prepare("SELECT Position, ItemId, Stack, SerializedEffects FROM $M.characters_items WHERE OwnerId = ?");
        $st->execute([$id]);
        $equipement = []; $inventaire = [];
        foreach ($st->fetchAll() as $o) {
            $pos = (int) $o['Position'];
            if (isset(POSITIONS[$pos])) $equipement[POSITIONS[$pos]] = ['id' => (int) $o['ItemId'], 'effets' => effets($o['SerializedEffects'])];
            elseif ($pos === NON_EQUIPE) $inventaire[(int) $o['ItemId']] = ($inventaire[(int) $o['ItemId']] ?? 0) + (int) $o['Stack'];
        }
        $st = $pdo->prepare("SELECT ItemId, SUM(Stack) AS N FROM $M.accounts_items_bank WHERE OwnerAccountId = ? GROUP BY ItemId");
        $st->execute([$proprio[$id]]);
        $banque = [];
        foreach ($st->fetchAll() as $o) $banque[(int) $o['ItemId']] = (int) $o['N'];

        json(['ok' => true, 'perso' => [
            'id' => (int) $p['Id'], 'nom' => $p['Name'], 'classe' => (int) $p['Breed'], 'sexe' => (int) $p['Sex'], 'niveau' => min(200, (int) $p['Niveau']),
            'base' => ['vita' => (int) $p['Vitality'], 'sag' => (int) $p['Wisdom'], 'for' => (int) $p['Strength'], 'int' => (int) $p['Intelligence'], 'cha' => (int) $p['Chance'], 'agi' => (int) $p['Agility']],
            'parcho' => ['vita' => (int) $p['PermanentAddedVitality'], 'sag' => (int) $p['PermanentAddedWisdom'], 'for' => (int) $p['PermanentAddedStrength'],
                'int' => (int) $p['PermanentAddedIntelligence'], 'cha' => (int) $p['PermanentAddedChance'], 'agi' => (int) $p['PermanentAddedAgility']],
            'compte' => $logins[$proprio[$id]] ?? '', 'look' => (string) $p['Look'], 'equipement' => $equipement, 'inventaire' => (object) $inventaire, 'banque' => (object) $banque, 'kamas' => (int) $p['Kamas'],
        ]]);
    }
    erreur('Action inconnue.', 404);
} catch (Throwable $e) {
    error_log('atelier.php : ' . $e->getMessage());
    erreur('Service indisponible.', 503);
}
