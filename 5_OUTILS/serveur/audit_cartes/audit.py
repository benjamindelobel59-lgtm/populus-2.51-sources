# MYSTIC 19/10 : audit des changements de carte et téléportations (toutes les cartes, dimensions comprises).
# Croise la base world (voisins serveur, voisins « client » reconnus par le serveur, redirections, cases) et les fichiers de cartes
# du client (voisins inscrits dans chaque .dlm). Sortie : rapport.json + resume.txt (même dossier).
import gzip, json, os, struct, subprocess, sys, zlib, collections
ICI = os.path.dirname(os.path.abspath(__file__))
RACINE = os.path.abspath(os.path.join(ICI, '..', '..', '..'))
sys.path.insert(0, os.path.join(RACINE, '_OUTILS', 'client', 'client-build'))
sys.path.insert(0, os.path.join(RACINE, '_OUTILS', 'client', 'client-build', 'atelier_astrub'))
import dlm, maps_elements as me
MYSQL = os.path.join(RACINE, '_OUTILS', 'logiciels', 'MariaDB', 'bin', 'mysql.exe')


def q(sql):
    out = subprocess.run([MYSQL, '-u', 'root', 'world', '--default-character-set=utf8mb4', '-N', '-B', '-e', sql],
                         capture_output=True).stdout.decode('utf-8')
    # lignes complétées par des champs vides (valeurs de texte avec retours à la ligne = lignes coupées)
    return [l.split('\t') + [''] * 12 for l in out.splitlines() if l]


def N(v):
    try:
        return int(v)
    except (TypeError, ValueError):
        return 0


maps = {}
for r in q("select Id,SubAreaId,TopNeighbourId,BottomNeighbourId,LeftNeighbourId,RightNeighbourId,ClientTopNeighbourId,"
           "ClientBottomNeighbourId,ClientLeftNeighbourId,ClientRightNeighbourId,hex(CompressedCells) from world_maps"):
    raw = bytes.fromhex(r[10]) if r[10] != 'NULL' else b''
    try:
        raw = gzip.decompress(raw)
    except Exception:
        try:
            raw = zlib.decompress(raw)
        except Exception:
            raw = b''
    cells = [(raw[i + 4], raw[i + 5]) for i in range(0, len(raw) - 10, 11)]
    maps[int(r[0])] = dict(sz=N(r[1]), srv=[N(x) for x in r[2:6]], cli_srv=[N(x) for x in r[6:10]], cells=cells)
pos = {int(r[0]): dict(x=N(r[1]), y=N(r[2]), wm=N(r[3]), out=N(r[4])) for r in q("select Id,PosX,PosY,WorldMap,Outdoor from world_maps_positions")}
noms = {int(r[0]): r[1] for r in q("select s.Id,l.French from world_subareas s join langs l on l.Id=s.NameId")}
areas = {int(r[0]): int(r[1]) for r in q("select Id,AreaId from world_subareas")}
anoms = {int(r[0]): r[1] for r in q("select a.Id,l.French from world_areas a join langs l on l.Id=a.NameId")}
# redirections : ordre haut, bas, gauche, droite
scroll = {int(r[0]): [N(r[4]), N(r[2]), N(r[3]), N(r[1])] for r in q(
    "select Id,RightMapId,BottomMapId,LeftMapId,TopMapId from world_maps_scroll_actions")}

MAPS = os.path.join(RACINE, 'CLIENT_64bits', 'app', 'content', 'maps')
cli = {}
for f in sorted(os.listdir(MAPS)):
    if not f.endswith('.d2p'):
        continue
    for name, blob in me.read_d2p(os.path.join(MAPS, f)):
        if not name.endswith('.dlm'):
            continue
        try:
            c = dlm.Carte(zlib.decompress(blob))
            cli[c.map_id] = list(struct.unpack_from('>iiii', c.pre, 9))
        except Exception:
            pass

DIRS = ['haut', 'bas', 'gauche', 'droite']
DELTA = [(0, -1), (0, 1), (-1, 0), (1, 0)]
OPP = [1, 0, 3, 2]


def bord(c, d):
    return [c < 28, c >= 532, c % 14 == 0, c % 14 == 13][d]


def walk(v):
    return (v & 1) == 0 and not (v & 4)


def zone(m):
    return anoms.get(areas.get(maps.get(m, {}).get('sz', 0)), '?')


def lieu(m):
    p = pos.get(m, {})
    sz = maps.get(m, {}).get('sz', 0)
    return f"{m} [{p.get('x')},{p.get('y')}] {zone(m)} / {noms.get(sz, '?')}"


pb = collections.defaultdict(list)
for m, d in maps.items():
    p = pos.get(m)
    for k in range(4):
        if not any(bord(i, k) and mc and walk(lm) for i, (lm, mc) in enumerate(d['cells'])):
            continue
        dest_cli = cli.get(m, [0] * 4)[k]
        redir = scroll.get(m, [0] * 4)[k]
        if dest_cli and dest_cli != d['cli_srv'][k] and not redir:
            pb['bloque_client_inconnu'].append((m, DIRS[k], dest_cli, d['cli_srv'][k]))
        cible = redir or d['srv'][k]
        if not cible or cible not in maps:
            # bloquant seulement si le client affiche une sortie de ce côté (voisin inscrit dans la carte du client)
            if dest_cli:
                pb['sortie_sans_destination'].append((m, DIRS[k], dest_cli, cible))
            continue
        if dest_cli and cible != dest_cli and not redir:
            pb['destination_differente_du_client'].append((m, DIRS[k], dest_cli, cible))
        q2 = pos.get(cible)
        if p and q2 and p['wm'] == q2['wm'] and p['out'] and q2['out'] and not redir:
            if (q2['x'] - p['x'], q2['y'] - p['y']) != DELTA[k] and (q2['x'], q2['y']) != (p['x'], p['y']):
                pb['coordonnees_incoherentes'].append((m, DIRS[k], cible, (p['x'], p['y']), (q2['x'], q2['y'])))
        back = scroll.get(cible, [0] * 4)[OPP[k]] or maps[cible]['srv'][OPP[k]]
        retour = any(bord(i, OPP[k]) and mc and walk(lm) for i, (lm, mc) in enumerate(maps[cible]['cells']))
        pb_ = pos.get(back)
        if retour and back and back != m and p and pb_ and (pb_['x'], pb_['y']) != (p['x'], p['y']):
            pb['retour_vers_autre_carte'].append((m, DIRS[k], cible, back))
        proche = [lambda c: c < 70, lambda c: c >= 490, lambda c: c % 14 <= 1, lambda c: c % 14 >= 12][OPP[k]]
        if maps[cible]['cells'] and not any(proche(i) and walk(lm) for i, (lm, mc) in enumerate(maps[cible]['cells'])):
            pb['arrivee_sans_case_praticable'].append((m, DIRS[k], cible))


def verif_tp(source_map, source, m_dest, cell, quoi):
    m_dest, cell = N(m_dest), N(cell)
    if m_dest not in maps:
        pb['teleport_carte_inexistante'].append((source_map, quoi, source, m_dest, cell))
        return
    cs = maps[m_dest]['cells']
    # cases et interactifs passent par Map.GetArrivalCell (case libre la plus proche) : seuls PNJ, potions et donjons comptent
    if quoi in ('pnj', 'potion', 'donjon') and cs and (cell < 0 or cell >= len(cs) or not walk(cs[cell][0])):
        pb['teleport_case_non_praticable'].append((source_map, quoi, source, m_dest, cell))


for r in q("select MapId,CellId,Parameter0,Parameter1 from world_maps_triggers where Type='Teleport'"):
    verif_tp(N(r[0]), f"case {r[1]}", r[3], r[2], 'case')
for r in q("select s.MapId,k.Id,k.Parameter0,k.Parameter1 from interactives_skills k join interactives_spawns_skills ss on ss.SkillId=k.Id "
           "join interactives_spawns s on s.Id=ss.InteractiveSpawnId where k.Type='Teleport'"):
    verif_tp(N(r[0]), f"compétence {r[1]}", r[2], r[3], 'interactif')
for r in q("select Id,MessageId,Parameter0,Parameter1 from npcs_replies where Type='Teleport'"):
    verif_tp(0, f"réponse {r[0]} (message {r[1]})", r[2], r[3], 'pnj')
for r in q("select ItemId,MapId,CellId from items_teleport_potions"):
    verif_tp(0, f"objet {r[0]}", r[1], r[2], 'potion')
for r in q("select Id,MapId,TeleportMapId,TeleportCell from monsters_spawns_dungeons where TeleportMapId is not null"):
    verif_tp(N(r[1]), f"donjon salle {r[0]}", r[2], r[3], 'donjon')

json.dump({k: v for k, v in pb.items()}, open(os.path.join(ICI, 'rapport.json'), 'w', encoding='utf-8'), ensure_ascii=False, default=list)
with open(os.path.join(ICI, 'resume.txt'), 'w', encoding='utf-8') as o:
    o.write(f"cartes serveur {len(maps)} ; cartes client {len(cli)} ; redirections {len(scroll)}\n")
    for k, v in sorted(pb.items()):
        o.write(f"\n== {k} : {len(v)}\n")
        par = collections.Counter(zone(x[0]) for x in v)
        o.write('   par zone : ' + ', '.join(f"{z} {n}" for z, n in par.most_common(15)) + '\n')
        for x in v[:12]:
            o.write('   ' + (lieu(x[0]) if x[0] else '-') + ' ' + str(x[1:]) + '\n')
print(open(os.path.join(ICI, 'resume.txt'), encoding='utf-8').read())
