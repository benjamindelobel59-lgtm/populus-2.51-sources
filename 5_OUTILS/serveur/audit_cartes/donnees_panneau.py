# MYSTIC 19/10 : données du panneau de validation de l'audit des cartes (une proposition par problème).
# Entrées : rapport.json, corrigeables.json, corrigeables_refuses.json + base world. Sortie : panneau_donnees.json
import gzip, json, os, subprocess, zlib, collections
ICI = os.path.dirname(os.path.abspath(__file__)); RACINE = os.path.abspath(os.path.join(ICI, '..', '..', '..'))
MYSQL = os.path.join(RACINE, '_OUTILS', 'logiciels', 'MariaDB', 'bin', 'mysql.exe')


def q(sql):
    out = subprocess.run([MYSQL, '-u', 'root', 'world', '--default-character-set=utf8mb4', '-N', '-B', '-e', sql],
                         capture_output=True).stdout.decode('utf-8')
    return [l.split('\t') + [''] * 8 for l in out.splitlines() if l]


def N(v):
    try:
        return int(v)
    except (TypeError, ValueError):
        return 0


cells, voisins = {}, {}
for r in q("select Id,hex(CompressedCells),TopNeighbourId,BottomNeighbourId,LeftNeighbourId,RightNeighbourId from world_maps"):
    raw = bytes.fromhex(r[1]) if r[1] not in ('', 'NULL') else b''
    try:
        raw = gzip.decompress(raw)
    except Exception:
        try:
            raw = zlib.decompress(raw)
        except Exception:
            raw = b''
    cells[N(r[0])] = [raw[i + 4] for i in range(0, len(raw) - 10, 11)]
    voisins[N(r[0])] = [N(x) for x in r[2:6]]
pos = {N(r[0]): (N(r[1]), N(r[2]), N(r[3])) for r in q("select p.Id,p.PosX,p.PosY,m.SubAreaId from world_maps_positions p join world_maps m on m.Id=p.Id")}
sz_nom = {N(r[0]): r[1] for r in q("select s.Id,l.French from world_subareas s join langs l on l.Id=s.NameId")}
sz_area = {N(r[0]): N(r[1]) for r in q("select Id,AreaId from world_subareas")}
a_nom = {N(r[0]): r[1] for r in q("select a.Id,l.French from world_areas a join langs l on l.Id=a.NameId")}

XY, n, x, y = {}, 0, 0, 0
for _ in range(20):
    for k in range(14):
        XY[n] = (x + k, y + k); n += 1
    x += 1
    for k in range(14):
        XY[n] = (x + k, y + k); n += 1
    y -= 1


def walk(m, c):
    cs = cells.get(m) or []
    return 0 <= c < len(cs) and (cs[c] & 1) == 0 and not (cs[c] & 4)


def plus_proche(m, c, filtre=lambda i: True):
    cs = cells.get(m) or []
    ref = XY.get(c, (0, 0))
    best = [i for i in range(len(cs)) if walk(m, i) and filtre(i)]
    if not best:
        return None
    return min(best, key=lambda i: abs(XY[i][0] - ref[0]) + abs(XY[i][1] - ref[1]))


def lieu(m):
    if m not in pos:
        return f"carte {m} (inexistante)"
    x, y, sz = pos[m]
    return f"{m} [{x},{y}] {sz_nom.get(sz, '?')}"


def zone(m):
    return a_nom.get(sz_area.get(pos.get(m, (0, 0, 0))[2]), '?') if m else 'PNJ / objets'


DIRS = {'haut': 0, 'bas': 1, 'gauche': 2, 'droite': 3}
OPP = {'haut': 'bas', 'bas': 'haut', 'gauche': 'droite', 'droite': 'gauche'}
BORD = {'haut': lambda c: c < 56, 'bas': lambda c: c >= 504, 'gauche': lambda c: c % 14 <= 1, 'droite': lambda c: c % 14 >= 12}
MILIEU = {'haut': 7, 'bas': 553, 'gauche': 280, 'droite': 293}

d = json.load(open(os.path.join(ICI, 'rapport.json'), encoding='utf-8'))
c = json.load(open(os.path.join(ICI, 'corrigeables.json'), encoding='utf-8'))
# après le SQL 79, les « corrigeables » restants sont ceux écartés par prudence
refus = [(m, sens, mort, vraie, voisins.get(vraie, [0] * 4)[DIRS[OPP[sens]]]) for m, sens, mort, vraie in c['corrigeable']]
items = []


def ajout(cat, cle, m, titre, detail, options, defaut=None, saisie=None):
    items.append(dict(cat=cat, id=f"{cat}-{cle}", zone=zone(m), titre=titre, detail=detail, options=options, defaut=defaut, saisie=saisie))


for m, sens, mort, vraie, retour in refus:
    ajout('refuses', f"{m}-{sens}", m, f"{lieu(m)} — sortie {sens}",
          f"Mène à {mort} (inexistante). Carte à côté : {lieu(vraie)}, mais son passage retour mène à {lieu(retour)}.",
          [{'k': 'relier', 'l': f"Relier à {lieu(vraie)}", 'sql': ['voisin', m, sens, vraie]}, {'k': 'ignorer', 'l': 'Ne rien changer'}])
for m, sens, mort, cands in c['ambigu']:
    ajout('ambigus', f"{m}-{sens}", m, f"{lieu(m)} — sortie {sens}", f"Mène à {mort} (inexistante). Plusieurs cartes à côté :",
          [{'k': f"c{k}", 'l': f"Relier à {lieu(k)}", 'sql': ['voisin', m, sens, k]} for k in cands] + [{'k': 'ignorer', 'l': 'Ne rien changer'}])
for m, sens, mort in c['rien']:
    ajout('vide', f"{m}-{sens}", m, f"{lieu(m)} — sortie {sens}", f"Mène à {mort} (inexistante), aucune carte à côté.",
          [{'k': 'ignorer', 'l': 'Laisser (bord de monde)'}, {'k': 'saisie', 'l': 'Relier à la carte n° :', 'sql': ['voisin', m, sens, None]}],
          defaut='ignorer', saisie='carte')
for m, sens, cible, retour in d.get('retour_vers_autre_carte', []):
    ajout('sensunique', f"{m}-{sens}", m, f"{lieu(m)} → {sens} → {lieu(cible)}",
          f"Le retour depuis {lieu(cible)} ({OPP[sens]}) mène à {lieu(retour)} au lieu de revenir ici.",
          [{'k': 'reciproque', 'l': f"Rendre réciproque (retour vers {m})", 'sql': ['voisin', cible, OPP[sens], m]}, {'k': 'ignorer', 'l': 'Laisser (sens unique voulu)'}],
          defaut='ignorer')
for m, sens, cible in d.get('arrivee_sans_case_praticable', []):
    prop = plus_proche(cible, MILIEU[OPP[sens]], BORD[OPP[sens]]) or plus_proche(cible, MILIEU[OPP[sens]])
    opts = [{'k': 'ignorer', 'l': 'Laisser'}]
    if prop is not None:
        opts.insert(0, {'k': 'arrivee', 'l': f"Arrivée sur la case {prop} de {lieu(cible)}", 'sql': ['arrivee', m, sens, prop]})
    ajout('arrivees', f"{m}-{sens}", m, f"{lieu(m)} → {sens} → {lieu(cible)}", "Aucune case praticable près du bord d'arrivée.", opts)
for src, quoi, source, mdest, cell in d.get('teleport_case_non_praticable', []):
    if cell == -1 or not mdest:
        continue
    prop = plus_proche(mdest, cell if 0 <= cell < 560 else 280)
    if prop is None:
        continue
    table = {'pnj': 'npcs_replies', 'potion': 'items_teleport_potions', 'donjon': 'monsters_spawns_dungeons'}[quoi]
    ajout('teleports', f"{quoi}-{source}", src, f"{quoi} {source} → {lieu(mdest)} case {cell}", "La case d'arrivée n'est pas praticable.",
          [{'k': 'corriger', 'l': f"Arriver sur la case {prop}", 'sql': ['tp', table, source, mdest, cell, prop]}, {'k': 'ignorer', 'l': 'Ne rien changer'}])
for src, quoi, source, mdest, cell in d.get('teleport_carte_inexistante', []):
    ajout('inexistant', f"{quoi}-{source}", src, f"{quoi} {source}", f"Téléporte vers la carte {mdest} case {cell}, qui n'existe pas.",
          [{'k': 'saisie', 'l': 'Remplacer par la carte n° :', 'sql': ['tpcarte', source, None]}, {'k': 'ignorer', 'l': 'Ne rien changer'}], saisie='carte')

CATS = [('refuses', 'Sorties écartées par prudence'), ('ambigus', 'Sorties ambiguës'), ('teleports', 'Téléportations sur case non praticable'),
        ('inexistant', 'Téléportation vers carte inexistante'), ('arrivees', 'Arrivées sans case praticable'),
        ('sensunique', 'Passages à sens unique'), ('vide', 'Sorties vers le vide')]
json.dump({'cats': CATS, 'items': items}, open(os.path.join(ICI, 'panneau_donnees.json'), 'w', encoding='utf-8'), ensure_ascii=False)
print(collections.Counter(i['cat'] for i in items))
