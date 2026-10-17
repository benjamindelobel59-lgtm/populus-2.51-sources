# -*- coding: utf-8 -*-
"""
NEKFEU : salles de donjon en 8 contre 8.
Le serveur refuse / exclut les combattants en trop quand une équipe a moins de cases de placement que de combattants
(Fight.FindRandomFreeCell → LeaveFight, FightTeam.IsFull) : le combat « se coupe ». Ce script vérifie toutes les salles
(monsters_spawns_dungeons) et complète les placements à 8 cases rouges (joueurs) + 8 cases bleues (monstres).

Placements utilisés par le serveur (Map.UpdateFightPlacementsOld) :
  - BlueCellsCSV / RedCellsCSV de world_maps s'ils sont remplis tous les deux,
  - sinon les cases marquées bleu / rouge dans les cellules de la map (CompressedCells).
Le script écrit les deux listes complétées dans BlueCellsCSV / RedCellsCSV (les cases d'origine sont gardées).

Utilisation :  python -I placements_8v8.py            → rapport seulement
               python -I placements_8v8.py --sql F    → écrit le script SQL F (UPDATE world_maps …)
"""
import gzip, subprocess, sys, os, math
sys.stdout.reconfigure(encoding="utf-8", errors="replace")

MYSQL = os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..', 'logiciels', 'MariaDB', 'bin', 'mysql.exe')
CIBLE = 8
LARGEUR = 14


def requete(sql):
    out = subprocess.run([MYSQL, '-uroot', '-h127.0.0.1', '--default-character-set=utf8mb4', '-N', '-B', 'world', '-e', sql],
                         capture_output=True, check=True).stdout.decode('utf-8')
    return [l.split('\t') for l in out.splitlines() if l]


def cellules(version, hexa):
    raw = gzip.decompress(bytes.fromhex(hexa))
    res = []
    for i in range(0, len(raw) - 10, 11):
        cid = raw[i] << 8 | raw[i + 1]
        los = raw[i + 4]
        change = raw[i + 5]
        if version >= 9:
            walk = (los & 1) == 0
            nwf = (los & 2) != 0 or (los & 128) != 0
            nwrp = (los & 4) != 0
            red = (los & 32) != 0
            blue = (los & 16) != 0
        else:
            walk = (los & 1) == 1
            nwf = ((los & 4) >> 2) == 1 or ((los & 32) >> 5) == 1
            nwrp = ((los & 128) >> 7) == 1
            red = ((los & 8) >> 3) == 1
            blue = ((los & 16) >> 4) == 1
        res.append(dict(id=cid, ok=walk and not nwf and not nwrp and change == 0, red=red, blue=blue))
    return res


def pos(cid):
    # centre de la case (grille isométrique décalée : une ligne sur deux décalée d'une demi-case)
    ligne, col = divmod(cid, LARGEUR)
    return (col + 0.5 * (ligne % 2), ligne * 0.5)


def dist(a, b):
    pa, pb = pos(a), pos(b)
    return math.hypot(pa[0] - pb[0], pa[1] - pb[1])


def voisins(cid):
    # 4 voisins diagonaux (déplacement en combat)
    ligne, col = divmod(cid, LARGEUR)
    impair = ligne % 2
    res = []
    for dl, dc in ((-1, impair - 1), (-1, impair), (1, impair - 1), (1, impair)):
        l, c = ligne + dl, col + dc
        if 0 <= l < 40 and 0 <= c < LARGEUR:
            res.append(l * LARGEUR + c)
    return res


def interieure(c, libres):
    """case entourée de cases praticables (pas au bord de la zone ni de l'écran)"""
    ligne = c // LARGEUR
    return 2 <= ligne <= 37 and sum(v in libres for v in voisins(c)) >= 3


def completer(equipe, autre, libres, interdites):
    """complète `equipe` à CIBLE cases : à tour de rôle, une case libre collée à chaque case d'origine
    (garde la répartition officielle), en préférant l'intérieur de la zone et en restant loin de l'autre équipe"""
    equipe = list(equipe)
    origine = list(equipe)
    groupes = {o: [o] for o in origine}
    tour = 0
    while len(equipe) < CIBLE:
        prises = set(equipe) | set(autre) | interdites
        ajout = None
        for essai in range(len(origine)):
            o = origine[(tour + essai) % len(origine)]
            bord = {v for c in groupes[o] for v in voisins(c) if v in libres and v not in prises}
            # pas collée à l'autre équipe
            bord = {c for c in bord if min((dist(c, a) for a in autre), default=99) > 1.2}
            if not bord:
                continue
            ajout = min(bord, key=lambda c: (not interieure(c, libres), dist(c, o)))
            groupes[o].append(ajout)
            break
        tour += 1
        if ajout is None:
            # plus rien autour des cases d'origine : case libre intérieure la plus proche de l'équipe
            candidats = [c for c in libres if c not in prises and min((dist(c, a) for a in autre), default=99) > 1.2]
            if not candidats:
                break
            ajout = min(candidats, key=lambda c: (not interieure(c, libres), min(dist(c, e) for e in equipe)))
        equipe.append(ajout)
    return equipe


def graines(libres, rouge, bleu):
    """équipe vide : on la place loin de l'autre (ou, si les deux sont vides, aux deux bouts de la zone praticable)"""
    centre = (sum(pos(c)[0] for c in libres) / len(libres), sum(pos(c)[1] for c in libres) / len(libres))
    def d_centre(c):
        p = pos(c)
        return math.hypot(p[0] - centre[0], p[1] - centre[1])
    # on évite les extrêmes (bords de l'écran) : on prend dans les 70 % les plus proches du centre
    proches = sorted(libres, key=d_centre)[:max(CIBLE * 2, int(len(libres) * 0.7))]
    if not rouge and not bleu:
        a = max(proches, key=d_centre)
        b = max(proches, key=lambda c: dist(c, a))
        return [a], [b]
    if not rouge:
        return [max(proches, key=lambda c: min(dist(c, x) for x in bleu))], bleu
    return rouge, [max(proches, key=lambda c: min(dist(c, x) for x in rouge))]


def voir(libres, r, b, r0, b0):
    """grille : R/B = cases d'origine, r/b = cases ajoutées, . = praticable"""
    for ligne in range(40):
        txt = ' ' if ligne % 2 else ''
        for col in range(LARGEUR):
            c = ligne * LARGEUR + col
            txt += ('R' if c in r0 else 'r') if c in r else ('B' if c in b0 else 'b') if c in b else '.' if c in libres else ' '
            txt += ' '
        print('    ' + txt)


def main():
    sql_out = sys.argv[sys.argv.index('--sql') + 1] if '--sql' in sys.argv else None
    lignes = requete("""select m.Id, m.Version, ifnull(m.BlueCellsCSV,''), ifnull(m.RedCellsCSV,''), hex(m.CompressedCells),
                               group_concat(distinct d.Descripcion separator ' / ')
                        from world_maps m join monsters_spawns_dungeons d on d.MapId = m.Id group by m.Id order by m.Id""")
    # cases interdites : déclencheurs (téléporteurs) et cellules des interactifs
    trig = {}
    for mid, cid in requete("select MapId, CellId from world_maps_triggers"):
        trig.setdefault(int(mid), set()).add(int(cid))
    for mid, cid in requete("select MapId, CellId from interactives_spawns where CellId is not null"):
        trig.setdefault(int(mid), set()).add(int(cid))

    # salles de boss (un boss dans la salle, ou duo Grasmera / salle 6 Kolosso) : on ne touche pas aux monstres
    cartes_boss = {int(x[0]) for x in requete("""select distinct d.MapId from monsters_spawns_dungeons d
        join monsters_spawns_dungeons_groups g on g.DungeonSpawnId = d.Id join monsters_grades gr on gr.Id = g.MonsterGradeId
        join monsters_templates t on t.Id = gr.MonsterId where t.IsBoss = 1""")} | {74977793, 74977797, 195822592}
    a_corriger = []
    for mid, version, bcsv, rcsv, hexa, descr in lignes:
        mid, version = int(mid), int(version)
        cells = cellules(version, hexa)
        bleu = [int(x) for x in bcsv.split(',') if x.strip()]
        rouge = [int(x) for x in rcsv.split(',') if x.strip()]
        source = 'base'
        if not bleu or not rouge:
            bleu = [c['id'] for c in cells if c['blue']]
            rouge = [c['id'] for c in cells if c['red']]
            source = 'map'
        if len(rouge) >= CIBLE and (len(bleu) >= CIBLE or mid in cartes_boss):
            continue
        if source == 'map' and not bleu and not rouge:
            continue  # aucune case nulle part : le serveur tire déjà une grande zone au hasard (assez pour 8 contre 8)
        interdites = trig.get(mid, set())
        libres = {c['id'] for c in cells if c['ok'] and c['id'] not in interdites}
        rouge_ok = [c for c in rouge if c in libres]
        bleu_ok = [c for c in bleu if c in libres and c not in rouge_ok]
        if len(libres) < CIBLE * 2:
            print(f'!! {mid} {descr} : seulement {len(libres)} cases praticables, non corrigée')
            continue
        r, b = rouge_ok, bleu_ok
        if not r or not b:
            r, b = graines(sorted(libres), r, b)
        r = completer(r, b, libres, interdites)
        if mid not in cartes_boss:
            b = completer(b, r, libres, interdites)
        else:
            b = bleu  # salle de boss : cases des monstres inchangées (seules les cases joueurs sont complétées)
        print(f'{mid:>10} {descr[:55]:<55} {source} rouge {len(rouge):>2}->{len(r):>2}  bleu {len(bleu):>2}->{len(b):>2}')
        a_corriger.append((mid, descr, b, r))
        if '--voir' in sys.argv:
            voir(libres, r, b, rouge, bleu)

    print(f'{len(a_corriger)} salle(s) à compléter sur {len(lignes)} maps de donjon.')
    if sql_out and a_corriger:
        with open(sql_out, 'w', encoding='utf-8', newline='\n') as f:
            f.write('-- NEKFEU : salles de donjon en 8 contre 8 (8 cases rouges joueurs + 8 cases bleues monstres). Généré par placements_8v8.py\n')
            for mid, descr, b, r in a_corriger:
                f.write(f"-- {descr}\nUPDATE world_maps SET BlueCellsCSV='{','.join(map(str, b))}', RedCellsCSV='{','.join(map(str, r))}' WHERE Id={mid};\n")
        print('SQL écrit :', sql_out)


main()
