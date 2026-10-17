# NEKFEU : donne une action (Teleport) aux interactifs du jeu qui n'en ont pas (portes, escaliers, trappes...)
# a partir du graphe du monde du client 2.51 (worldgraph.tsv, voir lire_worldgraph.py).
#
# Usage : python generer_portes.py <worldgraph.tsv> <spawns_sans_skill.tsv> <criteres_ok.txt> <sortie.sql> <rapport.tsv> [declencheurs_existants.tsv] [liens_teleport.tsv]
#   liens_teleport.tsv : lien  spawn  map  case  skill  map_destination (Teleport existants) -> soleils sans "Utiliser", doublons nettoyes
#   declencheurs_existants.tsv : MapId  CellId  (world_maps_triggers) -> ajoute aussi les cases a declencheur manquantes (Ids 3000000+)
#   spawns_sans_skill.tsv : Id  MapId  CellId  (interactives_spawns sans ligne dans interactives_spawns_skills)
#   criteres_ok.txt       : identifiants de conditions reconnus par le serveur (separes par des espaces), ou SANS = aucune condition
#
# - plusieurs destinations (selon condition) = une competence par destination
# - un interactif = l'element_id d'une transition de type 32 (INTERACTIVE) partant de sa map
# - condition officielle gardee si TOUS ses identifiants sont reconnus, sinon l'interactif est ignore (rien d'ouvert par erreur)
# - case d'arrivee = case de l'interactif (ou du declencheur) qui ramene en sens inverse ; le serveur arrive a cote (SkillTeleport)
#   sans passage retour : -1 = centre de la zone praticable
# - Ids reserves : interactives_skills / interactives_spawns_skills 3000000+ (le script les supprime avant de les recreer)
import csv
import re
import sys
from collections import defaultdict

BASE_ID = 3000000
SANS_CONDITION = False  # criteres_ok.txt = "SANS" : aucune condition (demande utilisateur 16/10 20h4x : tout ouvrir)
ATOM = re.compile(r'^([A-Za-z¨]+)([=!<>~])(.*)$')


def criterion_ok(criterion, known):
    if not criterion or SANS_CONDITION:
        return True
    for atom in re.split(r'[&|()]', criterion):
        atom = atom.strip()
        if not atom:
            continue
        m = ATOM.match(atom)
        if not m or m.group(1) not in known:
            return False
    return True


def main(graph_path, spawns_path, known_path, sql_path, report_path, existing_path=None, links_path=None):
    global SANS_CONDITION
    known = set(open(known_path, encoding='utf-8').read().split())
    SANS_CONDITION = known == {'SANS'}
    spawns = {}
    with open(spawns_path, encoding='utf-8') as f:
        for row in csv.reader(f, delimiter='\t'):
            if row and row[0].isdigit():
                spawns[int(row[0])] = (int(row[1]), int(row[2]))

    transitions = []
    back = defaultdict(list)  # (from_map, to_map) -> [(type, cell)]
    with open(graph_path, encoding='utf-8') as f:
        for row in csv.DictReader(f, delimiter='\t'):
            t = {k: row[k] for k in row}
            for k in ('from_map', 'to_map', 'type', 'skill_id', 'cell', 'element_id'):
                t[k] = int(t[k])
            transitions.append(t)
            back[(t['from_map'], t['to_map'])].append((t['type'], t['cell']))

    if SANS_CONDITION:
        # une seule destination par interactif / par case : d'abord celle sans condition, jamais celle des fantomes (CE<1) en premier
        def rank(t):
            c = t['criterion']
            return (c != '', 'CE<' in c)
        best = {}
        for t in sorted(transitions, key=rank):
            key = ('e', t['element_id']) if t['type'] == 32 else ('c', t['from_map'], t['cell'], t['type'])
            best.setdefault(key, t)
        kept = set(id(t) for t in best.values())
        transitions = [t for t in transitions if id(t) in kept]
        for t in transitions:
            t['criterion'] = ''

    done = []  # (element_id, to_map, arrivee, condition, skill)
    seen = set()
    report = []
    stats = defaultdict(int)
    for t in transitions:
        if t['type'] != 32:
            continue
        eid = t['element_id']
        if eid not in spawns:
            stats['deja une action ou absent'] += 1
            continue
        map_id, cell_id = spawns[eid]
        if map_id != t['from_map']:
            stats['map differente'] += 1
            report.append((eid, map_id, cell_id, t['to_map'], '', t['criterion'], 'ignore : map differente'))
            continue
        key = (eid, t['to_map'], t['criterion'])
        if key in seen:
            stats['doublon'] += 1
            continue
        seen.add(key)
        if not criterion_ok(t['criterion'], known):
            stats['condition non geree'] += 1
            report.append((eid, map_id, cell_id, t['to_map'], '', t['criterion'], 'ignore : condition non geree'))
            continue
        candidates = back.get((t['to_map'], t['from_map']), [])
        candidates = sorted(candidates, key=lambda c: (c[0] != 32, c[0] != 8))
        arrival = next((c[1] for c in candidates if c[1] >= 0 and c[0] in (32, 8, 2)), -1)
        done.append((eid, t['to_map'], arrival, t['criterion'], t['skill_id']))
        stats['action ajoutee'] += 1
        report.append((eid, map_id, cell_id, t['to_map'], arrival, t['criterion'], 'ajoute' + ('' if arrival >= 0 else ' (arrivee au centre)')))

    with open(sql_path, 'w', encoding='utf-8', newline='\n') as out:
        out.write('-- NEKFEU : actions Teleport des interactifs sans action, d\'apres worldgraph.bin (client 2.51). Genere par generer_portes.py.\n')
        out.write('-- %s\n' % ', '.join('%s : %d' % kv for kv in sorted(stats.items())))
        out.write('DELETE FROM interactives_spawns_skills WHERE Id >= %d AND Id < %d;\n' % (BASE_ID, BASE_ID + 1000000))
        out.write('DELETE FROM interactives_skills WHERE Id >= %d AND Id < %d;\n' % (BASE_ID, BASE_ID + 1000000))
        rows = sorted(done)
        for i in range(0, len(rows), 500):
            chunk = rows[i:i + 500]
            out.write('INSERT INTO interactives_skills (Id, Type, Duration, `Condition`, CustomTemplateId, Parameter0, Parameter1, Parameter2) VALUES\n')
            out.write(',\n'.join(
                " (%d, 'Teleport', 60, %s, %d, '%d', '%d', '1')" % (
                    BASE_ID + i + n, "'" + crit.replace("'", "''") + "'" if crit else 'NULL', skill, to_map, arrival)
                for n, (eid, to_map, arrival, crit, skill) in enumerate(chunk)) + ';\n')
            out.write('INSERT INTO interactives_spawns_skills (Id, InteractiveSpawnId, SkillId) VALUES\n')
            out.write(',\n'.join(
                ' (%d, %d, %d)' % (BASE_ID + i + n, eid, BASE_ID + i + n)
                for n, (eid, *_rest) in enumerate(chunk)) + ';\n')

    # cases a declencheur (type 8 MAP_ACTION : escaliers, trous, passages) absentes de world_maps_triggers
    triggers = []
    if existing_path:
        existing = set()
        with open(existing_path, encoding='utf-8') as f:
            for row in csv.reader(f, delimiter='\t'):
                if row and row[0].isdigit():
                    existing.add((int(row[0]), int(row[1])))
        seen_cells = set()
        for t in transitions:
            if t['type'] != 8 or t['cell'] < 0:
                continue
            key = (t['from_map'], t['cell'])
            if key in existing or key in seen_cells:
                continue
            if not criterion_ok(t['criterion'], known):
                stats['declencheur : condition non geree'] += 1
                report.append(('case', t['from_map'], t['cell'], t['to_map'], '', t['criterion'], 'ignore : condition non geree'))
                continue
            seen_cells.add(key)
            candidates = sorted(back.get((t['to_map'], t['from_map']), []), key=lambda c: (c[0] != 8, c[0] != 32))
            arrival = next((c[1] for c in candidates if c[1] >= 0 and c[0] in (32, 8, 2)), -1)
            triggers.append((t['from_map'], t['cell'], t['to_map'], arrival, t['criterion']))
            stats['declencheur ajoute'] += 1
            report.append(('case', t['from_map'], t['cell'], t['to_map'], arrival, t['criterion'], 'declencheur ajoute'))
        with open(sql_path, 'a', encoding='utf-8', newline='\n') as out:
            out.write('DELETE FROM world_maps_triggers WHERE Id >= %d AND Id < %d;\n' % (BASE_ID, BASE_ID + 1000000))
            for i in range(0, len(triggers), 500):
                chunk = triggers[i:i + 500]
                out.write('INSERT INTO world_maps_triggers (Id, Type, CellId, MapId, TriggerType, `Condition`, Parameter0, Parameter1) VALUES\n')
                out.write(',\n'.join(
                    " (%d, 'Teleport', %d, %d, 0, %s, '%d', '%d')" % (
                        BASE_ID + i + n, cell, map_id, "'" + crit.replace("'", "''") + "'" if crit else 'NULL', arrival, to_map)
                    for n, (map_id, cell, to_map, arrival, crit) in enumerate(chunk)) + ';\n')

    # nettoyage des actions Teleport existantes (liens_teleport.tsv : lien  spawn  map  case  skill  map_destination)
    #  - soleils & co : interactif pose sur une case officielle de type 8 (et pas interactif officiel) -> plus d'action
    #    (plus de menu "Utiliser", le declencheur officiel de la case fait le passage)
    #  - plusieurs Teleport sur un meme interactif : on garde ceux dont la destination est celle du graphe officiel
    if links_path:
        cells8 = set((t['from_map'], t['cell']) for t in transitions if t['type'] == 8 and t['cell'] >= 0)
        official = defaultdict(set)
        for t in transitions:
            if t['type'] == 32:
                official[t['element_id']].add(t['to_map'])
        by_spawn = defaultdict(list)
        with open(links_path, encoding='utf-8') as f:
            for row in csv.reader(f, delimiter='\t'):
                if row and row[0].isdigit():
                    link, spawn, map_id, cell, skill, dest = (int(x) for x in row[:6])
                    by_spawn[spawn].append((link, map_id, cell, skill, dest))
        remove = []
        for spawn, links in sorted(by_spawn.items()):
            map_id, cell = links[0][1], links[0][2]
            if (map_id, cell) in cells8 and spawn not in official:
                remove.extend(l[0] for l in links)
                stats['soleil : action retiree'] += len(links)
                report.append((spawn, map_id, cell, ','.join(str(l[4]) for l in links), '', '', 'soleil : action(s) retiree(s), declencheur officiel'))
            elif len(links) > 1 and spawn in official:
                bad = [l for l in links if l[4] not in official[spawn]]
                if len(bad) < len(links):
                    remove.extend(l[0] for l in bad)
                    stats['doublon : action retiree'] += len(bad)
                    if bad:
                        report.append((spawn, map_id, cell, ','.join(str(l[4]) for l in bad), '', '', 'doublon : destination non officielle retiree'))
        # mauvaise destination (ex. mine 458113 de 54159148 qui renvoyait sur sa propre map) : interactif a UNE action Teleport,
        # present dans le graphe, mais vers une autre map -> nouvelle action (Ids 3500000+) avec la destination officielle
        # (nouvelle ligne car certaines actions sont partagees entre plusieurs interactifs qui ne vont pas au meme endroit)
        first32 = {}
        for t in transitions:
            if t['type'] == 32:
                first32.setdefault(t['element_id'], t)
        removed = set(remove)
        fixes = []
        for spawn, links in sorted(by_spawn.items()):
            left = [l for l in links if l[0] not in removed]
            if len(left) != 1 or spawn not in first32:
                continue
            link, map_id, cell, skill, dest = left[0]
            if dest in official[spawn]:
                continue
            t = first32[spawn]
            candidates = sorted(back.get((t['to_map'], map_id), []), key=lambda c: (c[0] != 32, c[0] != 8))
            arrival = next((c[1] for c in candidates if c[1] >= 0 and c[0] in (32, 8, 2)), -1)
            fixes.append((link, t['to_map'], arrival, t['criterion'], t['skill_id']))
            stats['mauvaise destination corrigee'] += 1
            report.append((spawn, map_id, cell, t['to_map'], arrival, t['criterion'], 'destination corrigee (avant : %d)' % dest))

        with open(sql_path, 'a', encoding='utf-8', newline='\n') as out:
            for i in range(0, len(remove), 1000):
                out.write('DELETE FROM interactives_spawns_skills WHERE Id IN (%s);\n' % ','.join(str(x) for x in remove[i:i + 1000]))
            out.write('DELETE FROM interactives_skills WHERE Id >= %d AND Id < %d;\n' % (BASE_ID + 500000, BASE_ID + 1000000))
            for n, (link, to_map, arrival, crit, skill) in enumerate(fixes):
                new_id = BASE_ID + 500000 + n
                out.write("INSERT INTO interactives_skills (Id, Type, Duration, `Condition`, CustomTemplateId, Parameter0, Parameter1, Parameter2) VALUES (%d, 'Teleport', 60, %s, %d, '%d', '%d', '1');\n" % (
                    new_id, "'" + crit.replace("'", "''") + "'" if crit else 'NULL', skill, to_map, arrival))
                out.write('UPDATE interactives_spawns_skills SET SkillId=%d WHERE Id=%d;\n' % (new_id, link))

    with open(report_path, 'w', encoding='utf-8', newline='') as out:
        out.write('element_id\tmap\tcase\tvers_map\tcase_arrivee\tcondition\tresultat\n')
        for r in report:
            out.write('\t'.join(str(x) for x in r) + '\n')
    for k, v in sorted(stats.items()):
        print('%s : %d' % (k, v))


if __name__ == '__main__':
    main(*sys.argv[1:8])
