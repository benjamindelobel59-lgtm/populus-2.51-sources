# NEKFEU 2026-10-16 : modeles releves case par case sur les captures de placements officiels de l'utilisateur.
# Releves en coordonnees ECRAN (cs = colonne, rs = ligne ; une case a droite = cs+2, une case en dessous = rs+2,
# diagonales = cs+-1 / rs+-1), convertis en grille Stump : x = (cs + rs) / 2, y = (cs - rs) / 2.
# Utilisation : py -I modeles_officiels.py <dossier de sortie>
import os, sys

out = sys.argv[1]
os.makedirs(out, exist_ok=True)


def s(cells):
    res = []
    for cs, rs in cells:
        assert (cs + rs) % 2 == 0, (cs, rs)
        res.append(((cs + rs) // 2, (cs - rs) // 2))
    return res


def block(x0, y0, w, h, hole=None):
    return [(x0 + i, y0 + j) for i in range(w) for j in range(h) if (x0 + i, y0 + j) != hole]


def diamond(cs, rs):      # losange de 4 cases dont la case du haut est (cs, rs)
    return [(cs, rs), (cs + 1, rs + 1), (cs - 1, rs + 1), (cs, rs + 2)]


P = {}

# 1. previsualisation : motif symetrique, equipes melangees
B, R = [], []
rows = {-3: "BRRB", -2: "BRB", -1: "RBBR", 0: "R.R", 1: "RBBR", 2: "BRB", 3: "BRRB"}
for rs, txt in rows.items():
    start = -3 if len(txt) == 4 else -2
    for k, ch in enumerate(txt):
        cs = start + 2 * k
        if ch == "B": B.append((cs, rs))
        elif ch == "R": R.append((cs, rs))
P["officiel_previsu"] = (s(B), s(R))

# 2. foret d'Astrub : rouges = 3 losanges de 4 en diagonale ; bleus = bande 2 x 5 loin en dessous
red = diamond(-5, -5) + diamond(0, 0) + diamond(5, 5)
blue = [(-3 + i, 15 - i) for i in range(5)] + [(-2 + i, 16 - i) for i in range(5)]
P["officiel_foret"] = (s(blue), s(red))

# 3. cadre (map d'herbe) : rouges a gauche + en bas + losange au coin, bleus en haut + a droite + crochet
red = [(1, -3), (0, -2), (0, 0), (0, 2), (1, 3), (3, 3), (5, 3), (7, 3), (8, 4), (9, 5), (8, 6), (7, 7)]
blue = [(3, -5), (4, -6), (6, -6), (8, -6), (9, -5), (9, -3), (9, -1), (9, 1), (10, 2), (11, 3), (12, 2)]
P["officiel_cadre"] = (s(blue), s(red))

# 4. galere : chaque equipe = 2 blocs 2 x 3 ; bleus coins haut + gauche, rouges coins droite + bas
P["officiel_galere"] = (block(-2, -7, 2, 3) + block(-3, 5, 2, 3), block(2, 5, 2, 3) + block(3, -7, 2, 3))

# 5a. avant : bande 2 x 6 / deux bras en crochet
grey = [(i, i) for i in range(6)] + [(1 + i, -1 + i) for i in range(6)]
red = [(-1, 7), (0, 6), (1, 7), (2, 8), (3, 9), (2, 10), (-4, 10), (-3, 9), (-3, 11), (-2, 12), (-1, 13), (0, 12)]
P["officiel_avant"] = (s(grey), s(red))

# 5b. apres : deux blocs (ligne de 4 + ligne de 2) / bande 2 x 6
red = [(11, 9), (12, 8), (13, 7), (14, 6), (11, 7), (12, 6), (14, 12), (15, 11), (16, 10), (17, 9), (16, 12), (17, 11)]
grey = [(6 + i, 20 - i) for i in range(6)] + [(7 + i, 21 - i) for i in range(6)]
P["officiel_apres"] = (s(grey), s(red))

# 6. parquet : deux anneaux 3 x 3 (centre vide)
P["officiel_anneaux"] = (block(-1, -1, 3, 3, hole=(0, 0)), block(5, -9, 3, 3, hole=(6, -8)))

# 7. champs de Cania : deux blocs 3 x 4 tres eloignes
P["officiel_cania"] = (block(-3, 11, 4, 3), block(0, -2, 4, 3))

# 8. foret aux trous : 2 blocs 2 x 3 par equipe, coins opposes (bleus haut / bas, rouges gauche / droite)
P["officiel_coins"] = (block(-6, 4, 2, 3) + block(5, -6, 2, 3), block(-6, -1, 2, 3) + block(5, -1, 2, 3))

# 9. plage : rouges = 3 blocs 2 x 2 en escalier ; bleus = 3 blocs 2 x 2 de l'autre cote du bassin
P["officiel_plage"] = (block(8, 1, 2, 2) + block(9, 4, 2, 2) + block(12, 3, 2, 2), block(0, 0, 2, 2) + block(0, 4, 2, 2) + block(0, 8, 2, 2))

for name, (blues, reds) in P.items():
    assert not set(blues) & set(reds), name
    assert len(set(blues)) == len(blues) and len(set(reds)) == len(reds), name
    pts = lambda l: "".join("    <Point>\n      <X>%d</X>\n      <Y>%d</Y>\n    </Point>\n" % xy for xy in l)
    xml = ('<?xml version="1.0" encoding="utf-8"?>\n<PlacementPattern xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" '
           'xmlns:xsd="http://www.w3.org/2001/XMLSchema">\n  <Relative>true</Relative>\n  <Blues>\n%s  </Blues>\n  <Reds>\n%s  </Reds>\n'
           '  <Center>\n    <X>0</X>\n    <Y>0</Y>\n  </Center>\n</PlacementPattern>\n') % (pts(blues), pts(reds))
    open(os.path.join(out, name + ".xml"), "w", encoding="utf-8").write(xml)
    print(name, len(blues), "/", len(reds))
