# PADGREF 23/10 (retour utilisateur : « Servitude : à partir du portail on ne peut aller sur aucune carte ») : Eliocalypse — PASSAGES
# entre les cartes des 4 zones, tirés du world-graph.binary du client 2.74 (graphe.py). Serveur seulement (SQL 104) :
#  - bords « scriptés » (type 2) -> world_maps_scroll_actions (le client envoie le voisin de sa carte, le serveur redirige) ;
#  - éléments interactifs (type 32 : escaliers, portes, trappes du bateau…) -> interactives_spawns + compétence Teleport ; arrivée
#    à côté de l'élément de retour sur la carte d'arrivée (sinon près du centre) ;
#  - action de case (type 8) -> world_maps_triggers (Teleport en marchant dessus) ; conditions de quêtes 2.74 ignorées.
# Écartés : passages vers / depuis les salles de donjon (gérées par les gardiens), vers des cartes absentes (Arche 2.74, Astrub 2.74).
# Portails : 517982 Guerre, 517984 Misère, 518009 Royaume Corrompu (le 517950 pris le 23/10 est une PORTE), 517980 Servitude -> Arche.
import json, os, sys, zlib
sys.stdout.reconfigure(encoding='utf-8')
ICI = os.path.dirname(os.path.abspath(__file__))
CB = os.path.dirname(ICI)
R = os.path.abspath(os.path.join(CB, '..', '..', '..'))
sys.path.insert(0, CB); sys.path.insert(0, ICI)
import maps_elements as me
import cellules
from graphe import lire as lire_graphe
SQLDIR = os.path.join(R, '_OUTILS', 'serveur', 'donjons', 'eliocalypse')
ARCHE, ARCHE_CASE = 202899464, 259
PORTAILS = {517982, 517984, 518009, 517980}
DEJA = {517965}                                    # zaap de l'Arche (SQL 102)
SALLES = {202375168, 202376192, 202377216, 202378240, 202379264, 203423744, 203424768, 203425792, 203426816, 203427840,
          202637312, 202638336, 202639360, 202640384, 202641408, 203161600, 203162624, 203163648, 203164672, 203165696,
          202379266, 203427842, 202642432, 203165698}
inv = json.load(open(os.path.join(R, '_OUTILS', 'serveur', 'donjons', 'eliocalypse', 'inventaire.json'), encoding='utf-8'))
CARTES = set(inv['cartes']) - SALLES

# cases et éléments identifiés des cartes (client converti)
cells, elements = {}, {}
for nom, b in me.read_d2p(os.path.join(ICI, 'client', 'content', 'maps', 'maps11_2.d2p')):
    mid = int(os.path.basename(nom)[:-4])
    raw = zlib.decompress(b)
    cells[mid] = cellules.lire(raw)[1]
    for ident, gfx, cell in me.parse_map(raw)[1]:
        elements.setdefault(ident, []).append((mid, cell, gfx))


def praticable(mid, i):
    c = cells[mid][i] if 0 <= i < 560 else None
    return bool(c) and (c['f'] & 1) == 0 and not (c['f'] & 4) and not (c['f'] & 0x80)


def voisines(i):
    r, c = divmod(i, 14)
    pair = r % 2 == 0
    d = [-14, 14, -13, 15] if not pair else [-15, 13, -14, 14]   # 4 diagonales (cases adjacentes en losange)
    return [i + x for x in d] + [i - 28, i + 28, i - 1, i + 1]


def arrivee(mid, pres_de=None):
    if pres_de is not None:
        for v in voisines(pres_de):
            if praticable(mid, v):
                return v
    def dist(i):
        r, c = divmod(i, 14); return abs(r - 19) * 0.5 + abs(c - 7)
    return min((i for i in range(560) if praticable(mid, i)), key=dist)


graphe = [a for a in lire_graphe(os.path.join(R, '_OUTILS', 'client', 'client_27x', 'content', 'maps', 'world-graph.binary'))
          if a['de'] in CARTES]
DIRS = {0: 'Right', 2: 'Bottom', 4: 'Left', 6: 'Top'}
scroll, inter, cases = {}, {}, []
for a in graphe:
    for t in a['transitions']:
        if t['type'] == 2 and t['direction'] in DIRS and a['vers'] in CARTES:
            scroll.setdefault(a['de'], {})[DIRS[t['direction']]] = a['vers']
        elif t['type'] == 32 and t['id'] > 0 and t['id'] not in DEJA | PORTAILS and a['vers'] in CARTES:
            inter[t['id']] = (a['de'], t['case'], a['vers'])
        elif t['type'] == 8 and a['vers'] in CARTES:
            cases.append((a['de'], t['case'], a['vers']))
# retour : élément qui va de la carte d'arrivée vers la carte de départ
retour = {(de, vers): c for i, (de, c, vers) in inter.items()}
sql = ["-- PADGREF 23/10/2026 : SQL 104 — Eliocalypse : passages entre les cartes des 4 zones (world-graph 2.74) + portail du Royaume",
       "-- Corrompu (518009 ; le 517950 du SQL 102 est une porte). Généré par eliocalypse/passages.py. Base world. Rejouable.",
       "SET NAMES utf8mb4;", "START TRANSACTION;"]
ids = sorted(set(inter) | PORTAILS)
sql += [f"DELETE FROM interactives_spawns_skills WHERE Id BETWEEN 5100200 AND 5100499;",
        f"DELETE FROM interactives_skills WHERE Id BETWEEN 5100201 AND 5100499;",
        f"DELETE FROM interactives_spawns WHERE Id IN ({', '.join(map(str, ids + [517950]))});",
        "DELETE FROM world_maps_triggers WHERE Id BETWEEN 5100100 AND 5100199;",
        f"DELETE FROM world_maps_scroll_actions WHERE Id IN ({', '.join(map(str, sorted(scroll)))});"]
spawns, skills, liens = [], [], []
sk, lk = 5100201, 5100200
liens.append(f"({lk}, 517965, 1)"); lk += 1                           # zaap de l'Arche (SQL 102 l'avait déjà)
for i in sorted(PORTAILS):
    mid, cell, gfx = elements[i][0]
    spawns.append(f"({i}, 0, {mid}, {cell}, {gfx}, 0, NULL)")
skills.append(f"({sk}, 'Teleport', 0, NULL, NULL, {ARCHE}, {ARCHE_CASE}, 1)")
for i in sorted(PORTAILS):
    liens.append(f"({lk}, {i}, {sk})"); lk += 1
sk += 1
manque = []
for i, (de, cell, vers) in sorted(inter.items()):
    el = [e for e in elements.get(i, []) if e[0] == de]
    if not el:
        manque.append(i); continue
    _, c_el, gfx = el[0]
    c_ret = retour.get((vers, de))
    dest = arrivee(vers, c_ret)
    spawns.append(f"({i}, 0, {de}, {c_el}, {gfx}, 0, NULL)")
    skills.append(f"({sk}, 'Teleport', 0, NULL, NULL, {vers}, {dest}, 1)")
    liens.append(f"({lk}, {i}, {sk})"); sk += 1; lk += 1
sql.append("INSERT INTO interactives_spawns (Id, TemplateId, MapId, CellId, ElementId, Animated, Template) VALUES\n" + ",\n".join(spawns) + ";")
sql.append("INSERT INTO interactives_skills (Id, Type, Duration, `Condition`, CustomTemplateId, Parameter0, Parameter1, Parameter2) VALUES\n" +
           ",\n".join(skills) + ";")
sql.append("INSERT INTO interactives_spawns_skills (Id, InteractiveSpawnId, SkillId) VALUES\n" + ",\n".join(liens) + ";")
trig = []
for n, (de, cell, vers) in enumerate(cases):
    trig.append(f"({5100100 + n}, 'Teleport', {cell}, {de}, 0, NULL, '{arrivee(vers)}', '{vers}', '1', NULL, NULL, NULL)")
if trig:
    sql.append("INSERT INTO world_maps_triggers (Id, Type, CellId, MapId, TriggerType, `Condition`, Parameter0, Parameter1, Parameter2, "
               "Parameter3, Parameter4, AdditionalParameters) VALUES\n" + ",\n".join(trig) + ";")
rows = []
for m, d in sorted(scroll.items()):
    g = lambda k: d.get(k, 0)
    rows.append(f"({m}, {int('Right' in d)}, {int('Bottom' in d)}, {int('Left' in d)}, {int('Top' in d)}, {g('Right')}, {g('Bottom')}, "
                f"{g('Left')}, {g('Top')})")
sql.append("INSERT INTO world_maps_scroll_actions (Id, RightExists, BottomExists, LeftExists, TopExists, RightMapId, BottomMapId, "
           "LeftMapId, TopMapId) VALUES\n" + ",\n".join(rows) + ";")
sql.append("COMMIT;")
open(os.path.join(SQLDIR, '104_eliocalypse_passages.sql'), 'w', encoding='utf-8', newline='\n').write('\n'.join(sql) + '\n')
open(os.path.join(SQLDIR, '104_eliocalypse_passages-annuler.sql'), 'w', encoding='utf-8', newline='\n').write('\n'.join([
    "-- PADGREF 23/10 : annule 104_eliocalypse_passages.sql (puis rejouer 102 pour retrouver le zaap et les portails du 23/10)",
    "DELETE FROM interactives_spawns_skills WHERE Id BETWEEN 5100200 AND 5100499;",
    "DELETE FROM interactives_skills WHERE Id BETWEEN 5100201 AND 5100499;",
    f"DELETE FROM interactives_spawns WHERE Id IN ({', '.join(map(str, ids))});",
    "DELETE FROM world_maps_triggers WHERE Id BETWEEN 5100100 AND 5100199;",
    f"DELETE FROM world_maps_scroll_actions WHERE Id IN ({', '.join(map(str, sorted(scroll)))});"]) + '\n')
print('bords scriptés :', len(scroll), 'cartes | interactifs :', len(inter) - len(manque), '(+ 4 portails) | cases :', len(cases),
      '| éléments introuvables :', manque)
