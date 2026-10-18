<?php
// Mystic : classements (joueurs, pvp, guildes)
// GET classement.php?type=joueurs|pvp|guildes&classe=1..18&camp=1|2
declare(strict_types=1);
require __DIR__ . '/commun.php';

$type   = $_GET['type'] ?? 'joueurs';
$classe = (int) ($_GET['classe'] ?? 0);
$camp   = (int) ($_GET['camp'] ?? 0);
if (!in_array($type, ['joueurs', 'pvp', 'guildes'], true)) erreur('Type de classement inconnu.');
if ($classe && !isset(CLASSES[$classe])) erreur('Classe inconnue.');
if ($camp && !in_array($camp, [1, 2], true)) erreur('Camp inconnu.');

$cfg = reglages(); $n = (int) $cfg['taille_classement']; $role = (int) $cfg['role_max_classement'];
$A = base('auth'); $W = base('monde');

try {
    $lignes = en_cache("classement_{$type}_{$classe}_{$camp}", function () use ($type, $classe, $camp, $n, $role, $A, $W) {
        $pdo = bdd();
        if ($type === 'guildes') {
            $sql = "SELECT g.Name AS nom, g.Experience AS xp,
                      (SELECT MAX(e.Level) FROM $W.experiences e WHERE e.GuildExp IS NOT NULL AND e.GuildExp <= g.Experience) AS niveau,
                      (SELECT COUNT(*) FROM $W.guild_members m WHERE m.GuildId = g.Id) AS membres
                    FROM $W.guilds g ORDER BY g.Experience DESC LIMIT $n";
            return $pdo->query($sql)->fetchAll();
        }
        $filtre = ' AND a.Role <= :role'; $params = [':role' => $role];
        if ($classe) { $filtre .= ' AND c.Breed = :classe'; $params[':classe'] = $classe; }
        if ($type === 'pvp') {
            $filtre .= $camp ? ' AND c.AlignmentSide = :camp' : ' AND c.AlignmentSide IN (1,2)';
            if ($camp) $params[':camp'] = $camp;
            $sql = "SELECT c.Name AS nom, c.Breed AS classe, c.Sex AS sexe, c.AlignmentSide AS camp, c.Honor AS honneur, c.LastLookString AS look,
                      (SELECT MAX(e.Level) FROM $W.experiences e WHERE e.AlignmentHonor IS NOT NULL AND e.AlignmentHonor <= c.Honor) AS grade,
                      (SELECT MAX(e.Level) FROM $W.experiences e WHERE e.CharacterExp <= c.Experience) AS niveau
                    FROM $W.characters c JOIN $A.worlds_characters wc ON wc.CharacterId = c.Id JOIN $A.accounts a ON a.Id = wc.AccountId
                    WHERE a.IsBanned = 0 $filtre ORDER BY c.Honor DESC, c.Experience DESC LIMIT $n";
        } else {
            if ($camp) { $filtre .= ' AND c.AlignmentSide = :camp'; $params[':camp'] = $camp; }
            $sql = "SELECT c.Name AS nom, c.Breed AS classe, c.Sex AS sexe, c.AlignmentSide AS camp, c.Experience AS xp, c.LastLookString AS look,
                      (SELECT MAX(e.Level) FROM $W.experiences e WHERE e.CharacterExp <= c.Experience) AS niveau
                    FROM $W.characters c JOIN $A.worlds_characters wc ON wc.CharacterId = c.Id JOIN $A.accounts a ON a.Id = wc.AccountId
                    WHERE a.IsBanned = 0 $filtre ORDER BY c.Experience DESC LIMIT $n";
        }
        $st = $pdo->prepare($sql); $st->execute($params);
        return $st->fetchAll();
    });
} catch (Throwable $e) {
    erreur('Classement indisponible pour le moment.', 503);
}

$sortie = [];
foreach ($lignes as $i => $l) {
    $o = ['rang' => $i + 1, 'nom' => $l['nom'], 'niveau' => min(200, (int) ($l['niveau'] ?? 1))];
    if ($type === 'guildes') { $o['membres'] = (int) $l['membres']; $o['xp'] = (int) $l['xp']; }
    else {
        $o['classe'] = CLASSES[(int) $l['classe']] ?? '?';
        $o['camp'] = CAMPS[(int) $l['camp']] ?? 'Neutre';
        $o['avatar'] = avatar($l['look'] ?? null);
        if ($type === 'pvp') { $o['honneur'] = (int) $l['honneur']; $o['grade'] = (int) ($l['grade'] ?? 0); }
        else $o['xp'] = (int) $l['xp'];
    }
    $sortie[] = $o;
}
json(['ok' => true, 'type' => $type, 'lignes' => $sortie, 'classes' => CLASSES]);
