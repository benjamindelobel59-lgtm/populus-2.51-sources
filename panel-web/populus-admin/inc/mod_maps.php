<?php
/*
 * Panel admin Populus 2.51 : onglet Cartes (grille des cases, placements de combat, déclencheurs de téléportation,
 * voisins, apparition des monstres, PNJ / interactifs / groupes / joueurs présents). Rechargé à chaud par map_reload.
 */
if (!defined('POPULUS_ADMIN'))
    exit;

function map_position($id)
{
    $p = q('SELECT p.PosX, p.PosY, p.Outdoor, p.WorldMap, p.SubAreaId, s.NameId, s.AreaId FROM world_maps_positions p
        LEFT JOIN world_subareas s ON s.Id = p.SubAreaId WHERE p.Id = ?', [$id])->fetch();
    if (!$p)
        return null;
    return ['x' => (int)$p['PosX'], 'y' => (int)$p['PosY'], 'outdoor' => (int)$p['Outdoor'], 'worldMap' => (int)$p['WorldMap'],
        'subAreaId' => (int)$p['SubAreaId'], 'zone' => lang_map([$p['NameId']])[(int)$p['NameId']] ?? ''];
}

function handle_maps($action, $me)
{
    $in = $GLOBALS['in'];
    switch ($action) {

        case 'maps_at':
            $x = (int)($_GET['x'] ?? 0);
            $y = (int)($_GET['y'] ?? 0);
            $rows = q('SELECT p.Id, p.Outdoor, p.WorldMap, p.SubAreaId, s.NameId FROM world_maps_positions p JOIN world_maps m ON m.Id = p.Id
                LEFT JOIN world_subareas s ON s.Id = p.SubAreaId WHERE p.PosX = ? AND p.PosY = ? ORDER BY p.WorldMap, p.Outdoor DESC LIMIT 50', [$x, $y])->fetchAll();
            $names = lang_map(array_column($rows, 'NameId'));
            foreach ($rows as &$r)
                $r['Zone'] = $names[(int)$r['NameId']] ?? '';
            unset($r);
            json_out(['maps' => $rows]);

        case 'map':
            $id = (int)($_GET['id'] ?? 0);
            $m = q('SELECT Id, Version, SubAreaId, TopNeighbourId, BottomNeighbourId, LeftNeighbourId, RightNeighbourId, BlueCellsCSV, RedCellsCSV,
                TopNeighbourCellId, BottomNeighbourCellId, LeftNeighbourCellId, RightNeighbourCellId,
                ClientTopNeighbourId, ClientBottomNeighbourId, ClientLeftNeighbourId, ClientRightNeighbourId,
                SpawnDisabled, CompressedCells FROM world_maps WHERE Id = ?', [$id])->fetch();
            if (!$m)
                fail("La map $id n'existe pas.");
            $raw = @gzdecode($m['CompressedCells']);
            if ($raw === false)
                $raw = @gzuncompress($m['CompressedCells']);
            $cells = [];
            if ($raw !== false)
                for ($i = 0; $i + 11 <= strlen($raw); $i += 11)
                    $cells[] = [ord($raw[$i + 4]), ord($raw[$i + 5]), (ord($raw[$i + 2]) << 8 | ord($raw[$i + 3])) << 16 >> 16];  // losMov, mapChange, floor
            unset($m['CompressedCells']);
            $neighbours = [];
            foreach (['Top', 'Bottom', 'Left', 'Right'] as $d) {
                $nid = (int)$m[$d . 'NeighbourId'];
                $neighbours[$d] = $nid > 0 ? ['id' => $nid, 'pos' => map_position($nid)] : null;
            }
            $triggers = q('SELECT Id, Type, CellId, Parameter0, Parameter1, Parameter2, `Condition` FROM world_maps_triggers WHERE MapId = ? ORDER BY CellId', [$id])->fetchAll();
            $npcs = q("SELECT s.Id, s.NpcId, s.CellId, s.Direction, COALESCE(NULLIF(l.French,''), t.Name) Name FROM npcs_spawns s
                JOIN npcs_templates t ON t.Id = s.NpcId LEFT JOIN langs l ON l.Id = t.NameId WHERE s.MapId = ?", [$id])->fetchAll();
            $inter = q('SELECT i.Id, i.CellId, i.ElementId, t.NameId FROM interactives_spawns i LEFT JOIN interactives_templates t ON t.Id = i.TemplateId WHERE i.MapId = ?', [$id])->fetchAll();
            $iNames = lang_map(array_column($inter, 'NameId'));
            // NEKFEU : actions des interactifs (téléportations modifiables dans le panel)
            $iSkills = [];
            if ($inter) {
                $ids = implode(',', array_map('intval', array_column($inter, 'Id')));
                foreach (q("SELECT ss.Id LinkId, ss.InteractiveSpawnId, k.Id SkillId, k.Type, k.Parameter0, k.Parameter1, k.`Condition`, k.CustomTemplateId,
                        (SELECT COUNT(*) FROM interactives_spawns_skills x WHERE x.SkillId = k.Id) Shared
                        FROM interactives_spawns_skills ss JOIN interactives_skills k ON k.Id = ss.SkillId WHERE ss.InteractiveSpawnId IN ($ids) ORDER BY ss.Id")->fetchAll() as $s)
                    $iSkills[(int)$s['InteractiveSpawnId']][] = $s;
            }
            foreach ($inter as &$it) {
                $it['Name'] = $iNames[(int)$it['NameId']] ?? 'élément ' . $it['ElementId'];
                $it['Skills'] = $iSkills[(int)$it['Id']] ?? [];
            }
            unset($it);
            // arrivées : déclencheurs d'autres maps qui mènent ici
            $arrivals = q("SELECT MapId, CellId, Parameter0 DestCell FROM world_maps_triggers WHERE Type = 'Teleport' AND Parameter1 = ? LIMIT 50", [(string)$id])->fetchAll();
            $pos = map_position($id);
            $mobs = $pos ? q('SELECT s.Id, s.MonsterId, s.Frequency, s.IsDisabled, s.MapId, l.French Name FROM monsters_spawns s JOIN monsters_templates t ON t.Id = s.MonsterId
                LEFT JOIN langs l ON l.Id = t.NameId WHERE s.MapId = ? OR ((s.MapId IS NULL OR s.MapId = 0) AND s.SubAreaId = ?) ORDER BY s.IsDisabled, l.French', [$id, $pos['subAreaId']])->fetchAll() : [];
            $live = enqueue('map_actors', (string)$id);
            $image = map_image($id);
            json_out(['map' => $m, 'pos' => $pos, 'cells' => $cells, 'neighbours' => $neighbours, 'triggers' => $triggers, 'npcs' => $npcs,
                'interactives' => $inter, 'arrivals' => $arrivals, 'mobs' => $mobs, 'live' => !empty($live['ok']) ? json_decode($live['result'], true) : null, 'image' => $image]);

        case 'map_image':
            json_out(map_image((int)($_GET['id'] ?? 0)));

        case 'map_image_redo':
            $id = (int)($in['id'] ?? 0);
            @unlink(__DIR__ . "/../renders/$id.jpg");
            render_tables();
            q("REPLACE INTO admin_map_renders (MapId, Status, RequestedAt) VALUES (?, 'demande', NOW())", [$id]);
            json_out(['ok' => true]);

        case 'map_places_save':
            $id = (int)($in['id'] ?? 0);
            if (!q('SELECT 1 FROM world_maps WHERE Id = ?', [$id])->fetch())
                fail('Map introuvable');
            $clean = fn($a) => implode(',', array_values(array_unique(array_filter(array_map('intval', (array)$a), fn($c) => $c >= 0 && $c < 560))));
            $blue = $clean($in['blue'] ?? []);
            $red = $clean($in['red'] ?? []);
            $before = snapshot('world_maps', $id);
            q('UPDATE world_maps SET BlueCellsCSV = ?, RedCellsCSV = ? WHERE Id = ?', [$blue === '' ? null : $blue, $red === '' ? null : $red, $id]);
            log_change('Placements de combat', 'world_maps', $id, $before, "map $id");
            json_out(['ok' => true, 'reload' => enqueue('map_reload', (string)$id)]);

        case 'map_props_save':
            $id = (int)($in['id'] ?? 0);
            if (!q('SELECT 1 FROM world_maps WHERE Id = ?', [$id])->fetch())
                fail('Map introuvable');
            $vals = [];
            $current = q('SELECT TopNeighbourId, BottomNeighbourId, LeftNeighbourId, RightNeighbourId FROM world_maps WHERE Id = ?', [$id])->fetch();
            foreach (['TopNeighbourId', 'BottomNeighbourId', 'LeftNeighbourId', 'RightNeighbourId'] as $f) {
                $v = (int)($in[$f] ?? -1);
                // un voisin déjà en base (même absent de world_maps) reste accepté tel quel
                if ($v > 0 && $v !== (int)$current[$f] && !q('SELECT 1 FROM world_maps WHERE Id = ?', [$v])->fetch())
                    fail("La map voisine $v n'existe pas.");
                $vals[$f] = $v > 0 ? $v : -1;
            }
            foreach (['Top', 'Bottom', 'Left', 'Right'] as $d) {
                $c = $in[$d . 'NeighbourCellId'] ?? null;
                if ($c === null || $c === '' || (int)$c < 0)
                    $vals[$d . 'NeighbourCellId'] = null;     // automatique : case d'en face
                elseif ((int)$c > 559)
                    fail('Case d’arrivée invalide (0 à 559).');
                else
                    $vals[$d . 'NeighbourCellId'] = (int)$c;
            }
            $vals['SpawnDisabled'] = !empty($in['SpawnDisabled']) ? 1 : 0;
            $before = snapshot('world_maps', $id);
            q('UPDATE world_maps SET TopNeighbourId = ?, BottomNeighbourId = ?, LeftNeighbourId = ?, RightNeighbourId = ?,
                TopNeighbourCellId = ?, BottomNeighbourCellId = ?, LeftNeighbourCellId = ?, RightNeighbourCellId = ?, SpawnDisabled = ? WHERE Id = ?',
                array_merge(array_values($vals), [$id]));
            log_change('Voisins / apparition de la map', 'world_maps', $id, $before, "map $id");
            json_out(['ok' => true, 'reload' => enqueue('map_reload', (string)$id)]);

        case 'map_cells_save':
            // cases modifiées : { c: numéro, los: octet praticable / vue / combat, mc: octet des passages de bord }
            $id = (int)($in['id'] ?? 0);
            $row = q('SELECT CompressedCells FROM world_maps WHERE Id = ?', [$id])->fetch();
            if (!$row)
                fail('Map introuvable');
            $raw = @gzdecode($row['CompressedCells']);
            if ($raw === false)
                fail('Cases de la map illisibles.');
            $n = 0;
            foreach ((array)($in['cells'] ?? []) as $c) {
                $cell = (int)($c['c'] ?? -1);
                if ($cell < 0 || ($cell + 1) * 11 > strlen($raw))
                    continue;
                $raw[$cell * 11 + 4] = chr((int)$c['los'] & 255);
                $raw[$cell * 11 + 5] = chr((int)$c['mc'] & 255);
                $n++;
            }
            if (!$n)
                fail('Aucune case à enregistrer.');
            $before = snapshot('world_maps', $id);
            q('UPDATE world_maps SET CompressedCells = ? WHERE Id = ?', [gzencode($raw), $id]);
            log_change('Cases de la map modifiées', 'world_maps', $id, $before, "map $id : $n case(s)");
            // NEKFEU : le client garde sa copie des cases ; le serveur lui envoie ces cases comme obstacles (praticable / bloquée)
            // -> effet immédiat en jeu, sans relancer le client (Mettre-a-jour-cases-client.bat reste utile pour les passages de bord)
            $version = (int)q('SELECT Version FROM world_maps WHERE Id = ?', [$id])->fetchColumn();
            db()->exec('CREATE TABLE IF NOT EXISTS nekfeu_map_cells_live (MapId INT NOT NULL, CellId SMALLINT NOT NULL,
                Walkable TINYINT NOT NULL, PRIMARY KEY (MapId, CellId)) DEFAULT CHARSET=utf8');
            foreach ((array)($in['cells'] ?? []) as $c) {
                $cell = (int)($c['c'] ?? -1);
                if ($cell < 0 || $cell > 559)
                    continue;
                $los = (int)$c['los'] & 255;
                $walk = $version >= 9 ? (($los & 1) === 0 && ($los & 4) === 0) : (($los & 1) === 1 && ($los & 128) === 0);
                q('REPLACE INTO nekfeu_map_cells_live (MapId, CellId, Walkable) VALUES (?, ?, ?)', [$id, $cell, $walk ? 1 : 0]);
            }
            json_out(['ok' => true, 'reload' => enqueue('map_reload', (string)$id)]);

        case 'trigger_save':
            $map = (int)($in['mapId'] ?? 0);
            $tid = (int)($in['id'] ?? 0);
            $cell = (int)($in['cellId'] ?? -1);
            $dest = (int)($in['destMap'] ?? 0);
            $destCell = (int)($in['destCell'] ?? -1);
            if ($cell < 0 || $cell > 559 || $destCell < 0 || $destCell > 559)
                fail('Case invalide (0 à 559).');
            if (!q('SELECT 1 FROM world_maps WHERE Id = ?', [$map])->fetch())
                fail("La map $map n’existe pas sur le serveur.");
            if (!q('SELECT 1 FROM world_maps WHERE Id = ?', [$dest])->fetch())
                fail("La map d’arrivée $dest n’existe pas sur le serveur (vérifie le numéro, ou ouvre-la dans l’onglet Cartes).");
            $cond = trim((string)($in['condition'] ?? ''));
            if ($tid) {
                $before = snapshot('world_maps_triggers', $tid);
                if (!$before || (int)$before['MapId'] !== $map)
                    fail('Déclencheur introuvable');
                q("UPDATE world_maps_triggers SET CellId = ?, Parameter0 = ?, Parameter1 = ?, `Condition` = ? WHERE Id = ?", [$cell, $destCell, $dest, $cond === '' ? null : $cond, $tid]);
                log_change('Téléportation modifiée', 'world_maps_triggers', $tid, $before, "map $map case $cell → $dest");
            } else {
                q("INSERT INTO world_maps_triggers (Type, CellId, MapId, TriggerType, `Condition`, Parameter0, Parameter1) VALUES ('Teleport', ?, ?, 0, ?, ?, ?)",
                    [$cell, $map, $cond === '' ? null : $cond, $destCell, $dest]);
                $tid = (int)db()->lastInsertId();
                log_change('Téléportation ajoutée', 'world_maps_triggers', $tid, null, "map $map case $cell → $dest");
            }
            json_out(['ok' => true, 'id' => $tid, 'reload' => enqueue('map_reload', (string)$map)]);

        case 'trigger_delete':
            $tid = (int)($in['id'] ?? 0);
            $before = snapshot('world_maps_triggers', $tid);
            if (!$before)
                fail('Déclencheur introuvable');
            q('DELETE FROM world_maps_triggers WHERE Id = ?', [$tid]);
            log_change('Téléportation supprimée', 'world_maps_triggers', $tid, $before, "map {$before['MapId']} case {$before['CellId']}");
            json_out(['ok' => true, 'reload' => enqueue('map_reload', (string)$before['MapId'])]);

        // NEKFEU : téléportation d'un interactif (porte, escalier, portail...). Une action partagée avec d'autres
        // interactifs (ex. zaaps) n'est jamais modifiée : l'interactif reçoit sa propre copie.
        case 'inter_tp_save':
            $spawnId = (int)($in['spawnId'] ?? 0);
            $linkId = (int)($in['linkId'] ?? 0);
            $dest = (int)($in['destMap'] ?? 0);
            $destCell = (int)($in['destCell'] ?? -1);
            $cond = trim((string)($in['condition'] ?? ''));
            $spawn = q('SELECT Id, MapId, CellId, Animated, TemplateId FROM interactives_spawns WHERE Id = ?', [$spawnId])->fetch();
            if (!$spawn)
                fail('Interactif introuvable.');
            if ($destCell < -1 || $destCell > 559)
                fail('Case d’arrivée invalide (0 à 559, ou -1 = centre de la map).');
            if (!q('SELECT 1 FROM world_maps WHERE Id = ?', [$dest])->fetch())
                fail("La map d’arrivée $dest n’existe pas sur le serveur.");
            $label = "interactif $spawnId (map {$spawn['MapId']} case {$spawn['CellId']}) → $dest case $destCell";
            if ($linkId) {
                $link = q('SELECT Id, InteractiveSpawnId, SkillId FROM interactives_spawns_skills WHERE Id = ?', [$linkId])->fetch();
                if (!$link || (int)$link['InteractiveSpawnId'] !== $spawnId)
                    fail('Action introuvable sur cet interactif.');
                $skill = q('SELECT * FROM interactives_skills WHERE Id = ?', [$link['SkillId']])->fetch();
                if (!$skill || $skill['Type'] !== 'Teleport')
                    fail('Cette action n’est pas une téléportation.');
                $shared = (int)q('SELECT COUNT(*) FROM interactives_spawns_skills WHERE SkillId = ?', [$skill['Id']])->fetchColumn();
                if ($shared > 1) {
                    q("INSERT INTO interactives_skills (Type, Duration, `Condition`, CustomTemplateId, Parameter0, Parameter1, Parameter2, Parameter3, Parameter4, AdditionalParameters)
                        VALUES ('Teleport', ?, ?, ?, ?, ?, ?, ?, ?, ?)", [$skill['Duration'], $cond === '' ? null : $cond, $skill['CustomTemplateId'], (string)$dest, (string)$destCell,
                        $skill['Parameter2'] ?? '1', $skill['Parameter3'], $skill['Parameter4'], $skill['AdditionalParameters']]);
                    $newSkill = (int)db()->lastInsertId();
                    log_change('Téléportation d’interactif (copie d’une action partagée)', 'interactives_skills', $newSkill, null, $label);
                    $before = snapshot('interactives_spawns_skills', $linkId);
                    q('UPDATE interactives_spawns_skills SET SkillId = ? WHERE Id = ?', [$newSkill, $linkId]);
                    log_change('Téléportation d’interactif modifiée', 'interactives_spawns_skills', $linkId, $before, $label);
                } else {
                    $before = snapshot('interactives_skills', $skill['Id']);
                    q('UPDATE interactives_skills SET Parameter0 = ?, Parameter1 = ?, `Condition` = ? WHERE Id = ?', [(string)$dest, (string)$destCell, $cond === '' ? null : $cond, $skill['Id']]);
                    log_change('Téléportation d’interactif modifiée', 'interactives_skills', (int)$skill['Id'], $before, $label);
                }
            } else {
                q("INSERT INTO interactives_skills (Type, Duration, `Condition`, CustomTemplateId, Parameter0, Parameter1, Parameter2) VALUES ('Teleport', 60, ?, 184, ?, ?, '1')",
                    [$cond === '' ? null : $cond, (string)$dest, (string)$destCell]);
                $newSkill = (int)db()->lastInsertId();
                log_change('Téléportation d’interactif ajoutée', 'interactives_skills', $newSkill, null, $label);
                q('INSERT INTO interactives_spawns_skills (InteractiveSpawnId, SkillId) VALUES (?, ?)', [$spawnId, $newSkill]);
                $linkId = (int)db()->lastInsertId();
                log_change('Téléportation d’interactif ajoutée', 'interactives_spawns_skills', $linkId, null, $label);
            }
            // un interactif « animé » n'est jamais activé par le client (il attend une animation qui n'existe pas) : on le désanime
            if ((int)$spawn['Animated'] && in_array((int)$spawn['TemplateId'], [253, 280], true)) {   // portails Eliotrope / plates-formes seulement (une porte ordinaire disparaitrait)
                $before = snapshot('interactives_spawns', $spawnId);
                q('UPDATE interactives_spawns SET Animated = 0 WHERE Id = ?', [$spawnId]);
                log_change('Interactif désanimé (téléportation)', 'interactives_spawns', $spawnId, $before, $label);
            }
            json_out(['ok' => true, 'reload' => enqueue('map_reload', (string)$spawn['MapId'])]);

        case 'inter_tp_delete':
            $linkId = (int)($in['linkId'] ?? 0);
            $link = q('SELECT ss.Id, ss.InteractiveSpawnId, ss.SkillId, s.MapId FROM interactives_spawns_skills ss JOIN interactives_spawns s ON s.Id = ss.InteractiveSpawnId WHERE ss.Id = ?', [$linkId])->fetch();
            if (!$link)
                fail('Action introuvable.');
            $before = snapshot('interactives_spawns_skills', $linkId);
            q('DELETE FROM interactives_spawns_skills WHERE Id = ?', [$linkId]);
            log_change('Téléportation d’interactif supprimée', 'interactives_spawns_skills', $linkId, $before, "interactif {$link['InteractiveSpawnId']}");
            if (!(int)q('SELECT COUNT(*) FROM interactives_spawns_skills WHERE SkillId = ?', [$link['SkillId']])->fetchColumn()) {
                $before = snapshot('interactives_skills', $link['SkillId']);
                q('DELETE FROM interactives_skills WHERE Id = ?', [$link['SkillId']]);
                log_change('Action d’interactif supprimée (plus utilisée)', 'interactives_skills', (int)$link['SkillId'], $before, "interactif {$link['InteractiveSpawnId']}");
            }
            json_out(['ok' => true, 'reload' => enqueue('map_reload', (string)$link['MapId'])]);
    }
    return false;
}
