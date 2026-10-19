# MYSTIC 19/10 : pour chaque sortie vers une carte inexistante, cherche la carte réelle aux coordonnées voisines (même monde, même
# extérieur/intérieur, et dans la même zone si possible). Sortie : corrigeables.json
import json, os, subprocess, collections
ICI = os.path.dirname(os.path.abspath(__file__)); RACINE = os.path.abspath(os.path.join(ICI, '..', '..', '..'))
MYSQL = os.path.join(RACINE, '_OUTILS', 'logiciels', 'MariaDB', 'bin', 'mysql.exe')
def q(sql):
    out = subprocess.run([MYSQL, '-u', 'root', 'world', '-N', '-B', '-e', sql], capture_output=True).stdout.decode('utf-8')
    return [l.split('\t') for l in out.splitlines() if l]
pos = {int(r[0]): (int(r[1]), int(r[2]), int(r[3]), int(r[4]), int(r[5])) for r in q("select p.Id,p.PosX,p.PosY,p.WorldMap,p.Outdoor,m.SubAreaId from world_maps_positions p join world_maps m on m.Id=p.Id")}
area = {int(r[0]): int(r[1]) for r in q("select Id,AreaId from world_subareas")}
par_coord = collections.defaultdict(list)
for m, (x, y, wm, out, sz) in pos.items():
    par_coord[(x, y, wm)].append(m)
DELTA = {'haut': (0, -1), 'bas': (0, 1), 'gauche': (-1, 0), 'droite': (1, 0)}
d = json.load(open(os.path.join(ICI, 'rapport.json'), encoding='utf-8'))
res = {'corrigeable': [], 'ambigu': [], 'rien': []}
for m, sens, dest_cli, cible in d['sortie_sans_destination']:
    x, y, wm, out, sz = pos[m]
    dx, dy = DELTA[sens]
    cands = [c for c in par_coord[(x + dx, y + dy, wm)] if pos[c][3] == out]
    meme = [c for c in cands if area.get(pos[c][4]) == area.get(sz)]
    cands = meme or cands
    if len(cands) == 1:
        res['corrigeable'].append((m, sens, cible, cands[0]))
    elif cands:
        res['ambigu'].append((m, sens, cible, cands))
    else:
        res['rien'].append((m, sens, cible))
json.dump(res, open(os.path.join(ICI, 'corrigeables.json'), 'w', encoding='utf-8'))
print({k: len(v) for k, v in res.items()})
print('exemples corrigeables', res['corrigeable'][:8])
