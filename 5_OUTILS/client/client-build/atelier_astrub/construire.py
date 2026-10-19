# MYSTIC 19/10 : maison-atelier d'Astrub [5,-17] (maison 785, intérieur 191890437).
# L'intérieur devient une copie de l'atelier des Lacs enchantés (85461511 : scie, four, moule, meule, concasseur, alambic, plans de
# travail) + 6 ateliers de craft (table de confection, atelier, établi, enclume, machine à coudre, atelier de bricolage) + 4 ateliers de
# forgemagie (enclume, table, machine à coudre et atelier magiques). Les éléments viennent de cartes existantes (sources_elements.json).
# Sorties : client/ (maps*.d2p modifiés, 64 = 32 bits) et le SQL 76 (_OUTILS/serveur/sql/76_maison_atelier_astrub.sql).
import gzip, json, os, shutil, struct, sys, zlib
ICI = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, ICI); sys.path.insert(0, os.path.dirname(ICI))
import dlm, maps_elements as me
RACINE = os.path.abspath(os.path.join(ICI, '..', '..', '..', '..'))
MAPS = os.path.join(RACINE, 'CLIENT_64bits', 'app', 'content', 'maps')
BASE, CIBLE, PORTE_EXT, CASE_RETOUR = 85461511, 191890437, 191105028, 222
NOUVEAUX = [11, 12, 13, 57, 58, 122, 92, 116, 117, 118]          # modèles d'interactifs à ajouter
ID0 = 517300                                                     # nouveaux identifiants (spawns)

# --- cases serveur de la base ---
hexa = open(os.path.join(ICI, 'base_cells.hex')).read().strip()
cells = bytearray(gzip.decompress(bytes.fromhex(hexa)))
lm = lambda c: cells[c * 11 + 4]
praticable = lambda c: (lm(c) & 1) == 0 and not (lm(c) & 4)

def coords():
    ref, x, y, n = {}, 0, 0, 0
    for _ in range(20):
        for k in range(14): ref[n] = (x + k, y + k); n += 1
        x += 1
        for k in range(14): ref[n] = (x + k, y + k); n += 1
        y -= 1
    return ref
XY = coords(); PAR = {v: k for k, v in XY.items()}
def voisins(c):
    x, y = XY[c]
    return [PAR[p] for p in ((x + 1, y), (x - 1, y), (x, y + 1), (x, y - 1)) if p in PAR]

carte = dlm.Carte(dlm.lire(MAPS, dlm.index_cartes(MAPS), BASE))
existants = {cell: ident for li, cell, e, ident, gfx in carte.elements() if ident}
PORTE = 220
# case d'arrivée / sortie : case praticable voisine de la porte
sortie = next(v for v in voisins(PORTE) if praticable(v))

def connexe(bloques):
    libres = {c for c in range(560) if praticable(c) and c not in bloques}
    if not libres: return False
    vu, pile = {sortie}, [sortie]
    while pile:
        for v in voisins(pile.pop()):
            if v in libres and v not in vu: vu.add(v); pile.append(v)
    if vu != libres: return False
    # chaque atelier doit garder une case libre atteignable à côté
    return all(any(v in vu for v in voisins(c)) for c in list(existants) + list(bloques))

# candidates : cases praticables de bord (au moins un voisin non praticable), loin de la porte et des ateliers existants
interdits = {PORTE, sortie} | set(voisins(sortie)) | set(voisins(PORTE))
cand = [c for c in range(560) if praticable(c) and c not in interdits and c not in existants
        and any(not praticable(v) for v in voisins(c))]
choisis = []
for c in sorted(cand, key=lambda c: -sum(not praticable(v) for v in voisins(c))):
    if len(choisis) == len(NOUVEAUX): break
    if any(c in voisins(o) for o in choisis + list(existants)): continue
    if connexe(set(choisis + [c])): choisis.append(c)
assert len(choisis) == len(NOUVEAUX), f'seulement {len(choisis)} cases trouvées'

# --- client : nouveaux identifiants pour les éléments existants + ajout des ateliers ---
sources = json.load(open(os.path.join(ICI, 'sources_elements.json')))
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
    i, m, gfx, scell, li, brut = sources[str(t)]
    carte.ajouter(1, cell, bytes.fromhex(brut), nid)
    ajouts.append((nid, t, gfx, cell)); nid += 1

# cases : ateliers ajoutés = non praticables (bit 0), le reste identique
for c in choisis:
    cells[c * 11 + 4] |= 1
raw = carte.octets(CIBLE)
srv = [(cells[i + 4], cells[i + 5]) for i in range(0, len(cells), 11)]
sys.path.insert(0, os.path.dirname(ICI)); import maj_cases_client as mc
rapport = []
raw = mc.patch_dlm(raw, srv, rapport) or raw
os.makedirs(os.path.join(ICI, 'client'), exist_ok=True)
open(os.path.join(ICI, 'client', f'{CIBLE}.dlm'), 'wb').write(raw)

# --- SQL ---
base_spawns = [l.split('\t') for l in open(os.path.join(ICI, 'base_spawns.tsv')).read().strip().splitlines()]
sql = ["-- MYSTIC 19/10/2026 : SQL 76 — maison-atelier d'Astrub [5,-17] (maison 785) : intérieur 191890437 = atelier des Lacs enchantés",
       "-- + 6 ateliers de craft + 4 ateliers de forgemagie (FM limitée au niveau 100 dans le code). Généré par",
       "-- _OUTILS/client/client-build/atelier_astrub/construire.py. Base world, World ARRÊTÉ. Rejouable. Client : maps*.d2p (carte 191890437).",
       "SET NAMES utf8mb4;", "USE `world`;", "START TRANSACTION;",
       f"DELETE FROM interactives_spawns_skills WHERE InteractiveSpawnId IN (SELECT Id FROM interactives_spawns WHERE MapId = {CIBLE});",
       f"DELETE FROM interactives_spawns WHERE MapId = {CIBLE} OR Id BETWEEN {ID0} AND {ID0 + 99};",
       f"DELETE FROM world_maps_triggers WHERE MapId = {CIBLE};",
       f"UPDATE world_maps SET CompressedCells = 0x{gzip.compress(bytes(cells)).hex()}, BlueCellsCSV = (SELECT b FROM (SELECT BlueCellsCSV b FROM world_maps WHERE Id = {BASE}) x), "
       f"RedCellsCSV = (SELECT r FROM (SELECT RedCellsCSV r FROM world_maps WHERE Id = {BASE}) y) WHERE Id = {CIBLE};"]
cols = None
for s in base_spawns:
    sid, tpl, elem, cell = map(int, s)
    if sid not in renum or sid == existants.get(PORTE): continue
    sql.append(f"INSERT INTO interactives_spawns (Id, TemplateId, MapId, CellId, ElementId, Animated) VALUES ({renum[sid]}, {tpl}, {CIBLE}, {cell}, {elem}, 1);")
for i, t, gfx, cell in ajouts:
    sql.append(f"INSERT INTO interactives_spawns (Id, TemplateId, MapId, CellId, ElementId, Animated) VALUES ({i}, {t}, {CIBLE}, {cell}, {gfx}, 1);")
sql.append(f"INSERT INTO world_maps_triggers (Type, CellId, MapId, TriggerType, Parameter0, Parameter1) VALUES ('HouseExit', {sortie}, {CIBLE}, 0, {CASE_RETOUR}, {PORTE_EXT});")
sql += ["COMMIT;", f"SELECT Id, TemplateId, CellId, ElementId FROM interactives_spawns WHERE MapId = {CIBLE} ORDER BY Id;"]
open(os.path.join(RACINE, '_OUTILS', 'serveur', 'sql', '76_maison_atelier_astrub.sql'), 'w', encoding='utf-8', newline='\n').write('\n'.join(sql) + '\n')
json.dump({'sortie': sortie, 'choisis': choisis, 'ajouts': ajouts, 'renum': renum}, open(os.path.join(ICI, 'resultat.json'), 'w'), indent=1)
print('sortie', sortie, 'cases ateliers', choisis, 'rapport cases client', rapport)
