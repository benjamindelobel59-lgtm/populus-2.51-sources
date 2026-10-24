# PADGREF 23/10 : Pyramide Maudite (Saharach, sous-zone 881) : les passages entre salles sont des changements de carte par les bords
# vers une carte de transition vide (worldgraph type 2). Le serveur redirige via world_maps.*NeighbourId / *NeighbourCellId :
# on y met la vraie destination ; case d'arrivée = case de départ du passage retour (la case du worldgraph est celle du DÉPART). Usage : py -I generer.py <worldgraph.tsv> <sortie.sql> <annuler.sql>
import sys, subprocess, os, collections
R = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..', '..', '..'))
MYSQL = os.path.join(R, '_OUTILS', 'logiciels', 'MariaDB', 'bin', 'mysql.exe')
def q(s): return [l.split('\t') for l in subprocess.run([MYSQL, '-uroot', '-h127.0.0.1', '-N', '-B', 'world', '-e', s], capture_output=True).stdout.decode().splitlines() if l]
cartes = {int(r[0]) for r in q('select Id from world_maps_positions where SubAreaId=881')}
SENS = {0: 'Right', 2: 'Bottom', 4: 'Left', 6: 'Top'}
bords = collections.defaultdict(set)
for l in open(sys.argv[1], encoding='utf-8').read().splitlines()[1:]:
    c = l.split('\t')
    if c[4] != '2' or int(c[0]) not in cartes or int(c[5]) not in SENS: continue
    bords[(int(c[0]), SENS[int(c[5])])].add((int(c[2]), int(c[9]), int(c[8])))
OPP = {'Right': 'Left', 'Left': 'Right', 'Top': 'Bottom', 'Bottom': 'Top'}
def arrivee(m, s, dest):
    retour = bords.get((dest, OPP[s]))
    if retour and any(r[0] == m for r in retour): return next(r[1] for r in retour if r[0] == m)
    autres = [r[1] for (dm, ds), v in bords.items() if dm == dest for r in v if r[0] == m]
    return autres[0] if autres else 'NULL'
out, ann = [], []
for (m, s), v in sorted(bords.items()):
    if len(v) > 1: print('ambigu', m, s, v); continue
    dest, _, trans = next(iter(v))
    cell = arrivee(m, s, dest)
    cur = q(f'select {s}NeighbourId, ifnull({s}NeighbourCellId,"NULL"), Client{s}NeighbourId from world_maps where Id={m}')[0]
    if int(cur[2]) != trans: print('client different', m, s, cur, trans); continue
    if int(cur[0]) == dest and cur[1] == str(cell): continue
    out.append(f'UPDATE world_maps SET {s}NeighbourId = {dest}, {s}NeighbourCellId = {cell} WHERE Id = {m};')
    ann.append(f'UPDATE world_maps SET {s}NeighbourId = {cur[0]}, {s}NeighbourCellId = {cur[1]} WHERE Id = {m};')
entete = '-- PADGREF 23/10 SQL 142 : Pyramide Maudite (Saharach, sous-zone 881) : passages par les bords -> vraie salle + case (worldgraph 2.51)\n'
open(sys.argv[2], 'w', encoding='utf-8').write(entete + '\n'.join(out) + '\n')
open(sys.argv[3], 'w', encoding='utf-8').write('-- PADGREF 23/10 annulation du SQL 142\n' + '\n'.join(ann) + '\n')
print(len(out), 'passages')
