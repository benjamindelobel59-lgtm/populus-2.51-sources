# PADGREF 23/10 : Donjon des Kitsounes, salle 18091008 (« toutes les portes sont fermées, aucun accès aux mobs ») : la carte est coupée en
# zones séparées par des cases de porte bloquées. On ouvre (praticable) les cases bloquées qui touchent au moins 2 zones différentes,
# comme le panel admin : world_maps.CompressedCells (serveur) + nekfeu_map_cells_live (envoyé au client comme obstacle ouvert).
# Seulement les cases voisines d'un élément de porte (sinon on ouvrirait aussi des murs fins).
# Usage : py -I portes.py <mapId> <sortie.sql> <annuler.sql> <cases des éléments de porte, séparées par des virgules>
import sys, subprocess, os, gzip
R = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..', '..', '..'))
MYSQL = os.path.join(R, '_OUTILS', 'logiciels', 'MariaDB', 'bin', 'mysql.exe')
carte = int(sys.argv[1])
out = subprocess.run([MYSQL, '-uroot', '-h127.0.0.1', '-N', '-B', 'world', '-e', f'SELECT Version, HEX(CompressedCells) FROM world_maps WHERE Id={carte}'], capture_output=True).stdout.decode().split()
version, hexa = int(out[0]), out[1]
avant = bytes.fromhex(hexa); raw = bytearray(gzip.decompress(avant))
def marche(c):
    b = raw[c * 11 + 4]
    return ((b & 1) == 0 and (b & 4) == 0) if version >= 9 else ((b & 1) == 1 and (b & 128) == 0)
def ouvrir(c):
    if version >= 9: raw[c * 11 + 4] &= ~(1 | 4) & 255
    else: raw[c * 11 + 4] = (raw[c * 11 + 4] | 1) & ~128 & 255
def voisins(c):
    pair = (c // 14) % 2 == 0
    for d in ((-14, -15, 14, 13) if pair else (-13, -14, 15, 14)):
        n = c + d
        if 0 <= n < 560 and abs((n % 14) - (c % 14)) <= 1: yield n
zone = {}
for c in range(560):
    if not marche(c) or c in zone: continue
    pile = [c]; k = len(set(zone.values()))
    while pile:
        x = pile.pop()
        if x in zone or not marche(x): continue
        zone[x] = c; pile += [n for n in voisins(x) if n not in zone]
tailles = {}
for c, z in zone.items(): tailles[z] = tailles.get(z, 0) + 1
print('zones :', sorted(tailles.values(), reverse=True))
elements = set(map(int, sys.argv[4].split(',')))
portes = [c for c in range(560) if not marche(c) and len({zone[n] for n in voisins(c) if n in zone}) >= 2
          and (c in elements or any(n in elements for n in voisins(c)))]
print('cases a ouvrir :', portes)
for c in portes: ouvrir(c)
apres = {}
for c in range(560):
    if not marche(c) or c in apres: continue
    pile = [c]
    while pile:
        x = pile.pop()
        if x in apres or not marche(x): continue
        apres[x] = c; pile += [n for n in voisins(x) if n not in apres]
print('zones apres ouverture :', sorted(__import__('collections').Counter(apres.values()).values(), reverse=True))
nouveau = gzip.compress(bytes(raw))
sql = [f'-- PADGREF 23/10 : portes de la salle {carte} (Kitsounes) ouvertes : cases {",".join(map(str, portes))}',
       f'UPDATE world_maps SET CompressedCells = 0x{nouveau.hex().upper()} WHERE Id = {carte};']
sql += [f'REPLACE INTO nekfeu_map_cells_live (MapId, CellId, Walkable) VALUES ({carte}, {c}, 1);' for c in portes]
ann = [f'-- annulation', f'UPDATE world_maps SET CompressedCells = 0x{hexa} WHERE Id = {carte};']
ann += [f'DELETE FROM nekfeu_map_cells_live WHERE MapId = {carte} AND CellId = {c};' for c in portes]
open(sys.argv[2], 'w', encoding='utf-8').write('\n'.join(sql) + '\n')
open(sys.argv[3], 'w', encoding='utf-8').write('\n'.join(ann) + '\n')
