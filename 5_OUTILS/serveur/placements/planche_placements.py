# PADGREF 24/10 : planche des placements fixes (demande utilisateur : « crée-moi un HTML avec tous les donjons 80 à 200 qu'on a, avec l'ordre
# actuel des mobs, et je rajoute la cell id où il doit apparaître »). Une carte par salle : groupe du palier 8 (sinon le plus grand),
# monstres dans l'ordre de la base (Id des lignes), une case à remplir par monstre. Le bouton « Copier » donne une ligne par salle :
#   SALLE <DungeonSpawnId> carte <MapId> : <MonsterId>=<case>, <MonsterId>=<case>, ...   (dans l'ordre)
# Usage : py -I planche_placements.py <sortie.html> [niveau min] [niveau max]
import sys, os, struct, subprocess, html, json, collections
sys.stdout.reconfigure(encoding='utf-8')
R = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..', '..'))
sys.path.insert(0, os.path.join(R, '_OUTILS', 'client', 'client-build'))
from d2o_ecrire import D2O
C = os.path.join(R, 'CLIENT_64bits', 'app', 'data')
M = D2O(C + '/common/Monsters.d2o')
d = open(C + '/i18n/i18n_fr.d2i', 'rb').read()
io = struct.unpack_from('>i', d, 0)[0]; n = struct.unpack_from('>i', d, io)[0]; p = io + 4; T = {}
while p < io + 4 + n:
    k, dia, ptr = struct.unpack_from('>i?i', d, p); p += 9
    if dia: p += 4
    T[k] = ptr
def t(k):
    q = T.get(k)
    if q is None: return None
    l = struct.unpack_from('>H', d, q)[0]; return d[q + 2:q + 2 + l].decode('utf-8')

MY = os.path.join(R, '_OUTILS', 'logiciels', 'MariaDB', 'bin', 'mysql.exe')
def sql(q):
    out = subprocess.run([MY, '-uroot', '-N', '-B', '--default-character-set=utf8mb4', 'world', '-e', q], capture_output=True).stdout.decode('utf-8', 'replace')
    return [l.split('\t') for l in out.splitlines() if l]

nmin = int(sys.argv[2]) if len(sys.argv) > 2 else 80
nmax = int(sys.argv[3]) if len(sys.argv) > 3 else 200
nomsBase = {int(i): nm for i, nm in sql('select Id, Name from monsters_templates')}
def nom(m):
    o = M.objet(m)
    return (t(o['nameId']) if o else None) or nomsBase.get(m, f'monstre {m}')
boss = {m for m in nomsBase if (M.objet(m) or {}).get('isBoss')} | {int(i) for (i,) in sql('select Id from monsters_templates where IsBoss = 1')}

donjons = sql(f"select Id, Name, OptimalPlayerLevel, IFNULL(MapsIdsCSV,''), IFNULL(FirstRoom,0) from dungeons where OptimalPlayerLevel between {nmin} and {nmax} order by OptimalPlayerLevel, Name")
spawns = collections.defaultdict(list)
for sid, mid, tp, desc in sql('select Id, MapId, IFNULL(TeleportMapId,0), IFNULL(Descripcion,"") from monsters_spawns_dungeons'):
    spawns[int(mid)].append((int(sid), int(tp), desc))
groupes = collections.defaultdict(lambda: collections.defaultdict(list))
for sid, mp, m, gid in sql('select g.DungeonSpawnId, g.MinPartyMembers, gr.MonsterId, g.Id from monsters_spawns_dungeons_groups g '
                           'join monsters_grades gr on gr.Id = g.MonsterGradeId order by g.DungeonSpawnId, g.MinPartyMembers, g.Id'):
    groupes[int(sid)][int(mp)].append(int(m))

sections, total = [], 0
dejaVus = set()  # un spawn présent dans deux donjons n'est montré qu'une fois
for did, dnom, niv, csv, first in donjons:
    cartes = [int(x) for x in csv.split(',') if x.strip().lstrip('-').isdigit()]
    # ordre des salles : chaîne des téléportations depuis la 1re salle, puis le reste
    ordre, vu, cur = [], set(), int(first or 0)
    while cur and cur not in vu and cur in spawns:
        vu.add(cur); ordre.append(cur)
        cur = spawns[cur][0][1]
    ordre += [c for c in cartes if c not in vu and c in spawns]
    salles, cartesBoss = [], []
    for i, carte in enumerate(ordre, 1):
        for sid, tp, desc in spawns[carte]:
            g = groupes.get(sid)
            if not g or sid in dejaVus:
                continue
            palier = 8 if 8 in g else max(g)
            mons = g[palier]
            # 24/10 (demande utilisateur) : seulement les salles de boss
            if not any(m in boss for m in mons):
                continue
            dejaVus.add(sid)
            cartesBoss.append(str(carte))
            # 24/10 (demande utilisateur) : ordre MODULAIRE : groupe du plus petit palier (boss + ses monstres), puis le monstre ajouté
            # au palier suivant, etc. Chaque ligne indique dès quel palier le monstre est là. Clé de saisie = monstre#n-ième exemplaire.
            modul, avant = [], collections.Counter()
            hors = False
            for k in sorted(g):
                cpt = collections.Counter(g[k])
                if any(cpt[m] < avant[m] for m in avant):
                    hors = True  # un palier plus grand perd un monstre : pas emboîté
                for m in g[k]:
                    if cpt[m] > avant[m] and sum(1 for x, _ in modul if x == m) < cpt[m]:
                        modul.append((m, k))
                avant = cpt
            modul = [(m, k) for m, k in modul][:len(mons)] if not hors else [(m, palier) for m in mons]
            occ = collections.Counter(); lignes = ''
            for m, k in modul:
                occ[m] += 1
                lignes += (f'<li><span class="nm{" b" if m in boss else ""}">{html.escape(nom(m))}</span><span class="pk">{"1-4" if k == 1 else k}</span>'
                           f'<span class="id">{m}</span><input inputmode="numeric" pattern="[0-9]*" maxlength="3" data-m="{m}" data-k="{m}#{occ[m]}" '
                           f'aria-label="case de {html.escape(nom(m))}"></li>')
            ancien = ','.join(str(m) for m in mons)
            paliers = ' · '.join(f'{k if k > 1 else "1-4"} : {len(v)}' for k, v in sorted(g.items()))
            salles.append(f'<section class="salle" data-spawn="{sid}" data-carte="{carte}" data-ancien="{ancien}"><header><b>Salle {i}</b>'
                          f'<span class="map">MapID {carte}</span><span class="meta">spawn {sid}</span><span class="etat"></span></header>'
                          f'<div class="pal">ordre modulaire{" (paliers non emboîtés : ordre du palier 8)" if hors else ""} · monstres par palier : {paliers}</div>'
                          f'<ol>{lignes}</ol></section>')
            total += 1
    if salles:
        sections.append(f'<details class="dj" data-nom="{html.escape(dnom.lower())}"><summary><span class="niv">niv. {niv}</span>'
                        f'<span class="dn">{html.escape(dnom)}</span><span class="map">MapID {", ".join(cartesBoss)}</span><span class="cpt" data-n="{len(salles)}"></span></summary>'
                        f'<div class="salles">{"".join(salles)}</div></details>')

page = open(os.path.join(os.path.dirname(os.path.abspath(__file__)), 'planche_placements_modele.html'), encoding='utf-8').read()
page = page.replace('{{SECTIONS}}', '\n'.join(sections)).replace('{{NB_DJ}}', str(len(sections))).replace('{{NB_SALLES}}', str(total)) \
           .replace('{{NMIN}}', str(nmin)).replace('{{NMAX}}', str(nmax))
open(sys.argv[1], 'w', encoding='utf-8').write(page)
print(len(sections), 'donjons,', total, 'salles')
