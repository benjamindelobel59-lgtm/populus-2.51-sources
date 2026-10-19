# MYSTIC 19/10 (v2, retour utilisateur : ateliers dans le décor, pas de sortie) : l'intérieur 191890437 devient une copie de la salle
# de forgemagie 163055618 (4 ateliers magiques + concasseur, 185 cases) + les 13 ateliers de craft posés le long des murs, au moins
# 2 cases d'écart entre deux ateliers. Sortie : cases du bord bas (531, 544, 545, 557-559) = téléporteurs vers la porte verte d'Astrub
# (191105028 case 215) ; les flèches de changement de carte sont retirées. Sorties : client/191890437.dlm + SQL 78.
import gzip, json, os, struct, sys
ICI = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, ICI); sys.path.insert(0, os.path.dirname(ICI))
import dlm, maj_cases_client as mc
RACINE = os.path.abspath(os.path.join(ICI, '..', '..', '..', '..'))
MAPS = os.path.join(RACINE, 'CLIENT_64bits', 'app', 'content', 'maps')
BASE, CIBLE, PORTE_EXT, CASE_RETOUR = 163055618, 191890437, 191105028, 215
SORTIES = [531, 544, 545, 557, 558, 559]
NOUVEAUX = [2, 22, 27, 41, 90, 94, 97, 11, 12, 13, 57, 58, 122]
ID0 = 517400

cells = bytearray(gzip.decompress(bytes.fromhex(open(os.path.join(ICI, 'base2_cells.hex')).read().strip())))
lm = lambda c: cells[c * 11 + 4]
praticable = lambda c: (lm(c) & 1) == 0 and not (lm(c) & 4)
XY, n, x, y = {}, 0, 0, 0
for _ in range(20):
    for k in range(14): XY[n] = (x + k, y + k); n += 1
    x += 1
    for k in range(14): XY[n] = (x + k, y + k); n += 1
    y -= 1
PAR = {v: k for k, v in XY.items()}
def voisins(c):
    a, b = XY[c]
    return [PAR[p] for p in ((a + 1, b), (a - 1, b), (a, b + 1), (a, b - 1)) if p in PAR]
def dist(a, b): return abs(XY[a][0] - XY[b][0]) + abs(XY[a][1] - XY[b][1])

idx = dlm.index_cartes(MAPS)
carte = dlm.Carte(dlm.lire(MAPS, idx, BASE))
existants = [cell for li, cell, e, i, g in carte.elements() if i]
arrivee = 529 if praticable(529) else next(v for s in SORTIES for v in voisins(s) if praticable(v) and v not in SORTIES)

def connexe(bloques):
    libres = {c for c in range(560) if praticable(c) and c not in bloques}
    vu, pile = {arrivee}, [arrivee]
    while pile:
        for v in voisins(pile.pop()):
            if v in libres and v not in vu: vu.add(v); pile.append(v)
    return vu == libres and all(any(v in vu for v in voisins(c)) for c in existants + list(bloques))

zone_sortie = set(SORTIES) | {v for s in SORTIES for v in voisins(s)} | {arrivee} | set(voisins(arrivee))
# « dehors » = cases non praticables reliées au bord de la grille (le vide autour de la salle) : on ne pose les ateliers que contre
# ce mur extérieur, jamais à côté d'un obstacle du décor au milieu de la salle
bord = [c for c in range(560) if len(voisins(c)) < 4 and not praticable(c)]
dehors, pile = set(bord), list(bord)
while pile:
    for v in voisins(pile.pop()):
        if not praticable(v) and v not in dehors: dehors.add(v); pile.append(v)
cand = [c for c in range(560) if praticable(c) and c not in zone_sortie and any(v in dehors for v in voisins(c))
        and not any(not praticable(v) and v not in dehors for v in voisins(c))]
cand.sort(key=lambda c: (-sum(v in dehors for v in voisins(c)), c))
choisis = []
for ecart in (2, 1):                 # d'abord 2 cases d'écart, puis 1 s'il manque des places (jamais collés)
    for c in cand:
        if len(choisis) == len(NOUVEAUX): break
        if c in choisis or any(dist(c, o) <= ecart for o in choisis + existants): continue
        if connexe(set(choisis + [c])): choisis.append(c)
assert len(choisis) == len(NOUVEAUX), f'{len(choisis)} places trouvées seulement'

# éléments graphiques des ateliers : salle des Lacs enchantés (85461511) + sources_elements.json
sources = json.load(open(os.path.join(ICI, 'sources_elements.json')))
lacs = dlm.Carte(dlm.lire(MAPS, idx, 85461511))
par_ident = {i: e for li, cell, e, i, g in lacs.elements() if i}
LACS = {2: 464914, 22: 464917, 27: 464910, 41: 464911, 90: 464939, 94: 464912, 97: 464916}
def brut(t):
    if t in LACS: return par_ident[LACS[t]]
    return bytes.fromhex(sources[str(t)][5])

nid = ID0; renum = {}
for li, (lid, cs) in enumerate(carte.layers):
    for c in cs:
        for k, e in enumerate(c[1]):
            if e[0] == 2:
                ident = struct.unpack_from('>I', e, len(e) - 4)[0]
                if ident:
                    renum[ident] = nid
                    b = bytearray(e); struct.pack_into('>I', b, len(b) - 4, nid); c[1][k] = bytes(b); nid += 1
ajouts = []
for t, cell in zip(NOUVEAUX, choisis):
    e = brut(t)
    carte.ajouter(1, cell, e, nid)
    ajouts.append((nid, t, struct.unpack_from('>I', e, 1)[0], cell)); nid += 1

for c in choisis:
    cells[c * 11 + 4] |= 1          # atelier = case non praticable
for c in SORTIES:
    cells[c * 11 + 5] = 0            # plus de changement de carte (téléporteurs à la place)
raw = carte.octets(CIBLE)
raw = mc.patch_dlm(raw, [(cells[i + 4], cells[i + 5]) for i in range(0, len(cells), 11)], []) or raw
os.makedirs(os.path.join(ICI, 'client'), exist_ok=True)
open(os.path.join(ICI, 'client', f'{CIBLE}.dlm'), 'wb').write(raw)

M = os.path.join(RACINE, '_OUTILS', 'logiciels', 'MariaDB', 'bin', 'mysql.exe')
import subprocess
base_spawns = [l.split('\t') for l in subprocess.run([M, '-u', 'root', 'world', '-N', '-e',
               f'select Id,TemplateId,ElementId,CellId,Animated from interactives_spawns where MapId={BASE}'], capture_output=True).stdout.decode().strip().splitlines()]
sql = ["-- MYSTIC 19/10/2026 : SQL 78 — maison-atelier d'Astrub v2 : intérieur 191890437 = salle de forgemagie 163055618 (4 ateliers",
       "-- magiques + concasseur) + 13 ateliers de craft le long des murs ; sortie par le bas (téléporteurs vers la porte verte).",
       "-- Généré par _OUTILS/client/client-build/atelier_astrub/construire2.py. Base world, World ARRÊTÉ. Rejouable. Client : maps0.d2p.",
       "SET NAMES utf8mb4;", "USE `world`;", "START TRANSACTION;",
       f"DELETE FROM interactives_spawns_skills WHERE InteractiveSpawnId IN (SELECT Id FROM interactives_spawns WHERE MapId = {CIBLE});",
       f"DELETE FROM interactives_spawns WHERE MapId = {CIBLE} OR Id BETWEEN {ID0} AND {ID0 + 99};",
       f"DELETE FROM world_maps_triggers WHERE MapId = {CIBLE};",
       f"UPDATE world_maps SET CompressedCells = 0x{gzip.compress(bytes(cells)).hex()}, BlueCellsCSV = (SELECT b FROM (SELECT BlueCellsCSV b FROM world_maps WHERE Id = {BASE}) x), "
       f"RedCellsCSV = (SELECT r FROM (SELECT RedCellsCSV r FROM world_maps WHERE Id = {BASE}) y) WHERE Id = {CIBLE};"]
for sid, tpl, elem, cell, anim in base_spawns:
    sid = int(sid)
    if sid in renum:
        sql.append(f"INSERT INTO interactives_spawns (Id, TemplateId, MapId, CellId, ElementId, Animated) VALUES ({renum[sid]}, {tpl}, {CIBLE}, {cell}, {elem}, {anim});")
        sql.append(f"INSERT INTO interactives_spawns_skills (InteractiveSpawnId, SkillId) SELECT {renum[sid]}, SkillId FROM interactives_spawns_skills WHERE InteractiveSpawnId = {sid};")
for i, t, gfx, cell in ajouts:
    sql.append(f"INSERT INTO interactives_spawns (Id, TemplateId, MapId, CellId, ElementId, Animated) VALUES ({i}, {t}, {CIBLE}, {cell}, {gfx}, 1);")
for c in SORTIES:
    sql.append(f"INSERT INTO world_maps_triggers (Type, CellId, MapId, TriggerType, Parameter0, Parameter1) VALUES ('Teleport', {c}, {CIBLE}, 0, {CASE_RETOUR}, {PORTE_EXT});")
sql.append(f"UPDATE interactives_skills SET Parameter0 = '{CIBLE}', Parameter1 = '{arrivee}' WHERE Id = 101238;")
sql += ["COMMIT;", f"SELECT Id, TemplateId, CellId FROM interactives_spawns WHERE MapId = {CIBLE} ORDER BY Id;"]
open(os.path.join(RACINE, '_OUTILS', 'serveur', 'sql', '78_maison_atelier_v2.sql'), 'w', encoding='utf-8', newline='\n').write('\n'.join(sql) + '\n')
print('arrivée', arrivee, '; ateliers', list(zip(NOUVEAUX, choisis)))
