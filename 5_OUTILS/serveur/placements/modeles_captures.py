# NEKFEU 2026-10-16 : modeles de placement reproduits d'apres les captures de placements officiels envoyees par l'utilisateur.
# Coordonnees de la grille Stump (MapPoint) : a l'ecran, +x = vers le bas-droite, +y = vers le haut-droite
# (ligne ecran = x - y, colonne ecran = x + y). Le serveur tourne / retourne / deplace / ecarte ensuite les modeles.
# Utilisation : py -I modeles_captures.py <dossier de sortie>
import os, sys

out = sys.argv[1]
os.makedirs(out, exist_ok=True)


def block(x0, y0, w, h):
    return [(x0 + i, y0 + j) for i in range(w) for j in range(h)]


def line_x(x0, y, n, step=1):
    return [(x0 + i * step, y) for i in range(n)]


def line_y(x, y0, n, step=1):
    return [(x, y0 + j * step) for j in range(n)]


P = {}

# 1. damier (previsualisation) : bloc 4x4, bleu / rouge alternes
dam = block(-2, -2, 4, 4)
P["capture_damier"] = ([c for c in dam if (c[0] + c[1]) % 2 == 0], [c for c in dam if (c[0] + c[1]) % 2 == 1])

# 2. foret d'Astrub : rouges en 3 petits triangles, bleus en ligne brisee de l'autre cote
tri = lambda x, y: [(x, y), (x + 1, y), (x, y + 1)]
P["capture_foret"] = (
    [(6, -4), (6, -3), (7, -2), (7, -1), (6, 0), (6, 1), (7, 2), (7, 3)],
    tri(-6, 1) + tri(-4, -3) + tri(-2, 2))

# 3. cadre pointille : contour d'un rectangle, rouges sur 2 cotes, bleus sur les 2 autres (une case sur deux)
P["capture_cadre"] = (
    line_x(-3, 4, 4, 2) + line_y(5, -2, 4, 2),          # bleus : cote haut-droit
    line_x(-5, -4, 4, 2) + line_y(-5, -2, 4, 2))         # rouges : cote bas-gauche

# 4. galere : chaque equipe en deux carres 2x2
P["capture_galere"] = (block(-7, -1, 2, 2) + block(-1, 5, 2, 2), block(5, -1, 2, 2) + block(-1, -7, 2, 2))

# 5a. avant : crochet face a un bloc 2x5
P["capture_crochet"] = (block(4, -2, 2, 5), line_y(-4, -2, 5) + [(-3, -2), (-2, -2), (-3, 2), (-2, 2)])

# 5b. apres : deux lignes paralleles de 4 face a un bloc 2x5
P["capture_lignes"] = (block(4, -2, 2, 5), line_y(-5, -4, 4) + line_y(-3, 1, 4))

# 6. deux carres 3x3 face a face
P["capture_carres"] = (block(-5, 2, 3, 3), block(3, -4, 3, 3))

# 7. champs de Cania : deux rectangles 2x4 eloignes
P["capture_rectangles"] = (block(1, 5, 4, 2), block(-6, -4, 4, 2))

# 8. coins : chaque equipe en deux L, aux coins opposes
P["capture_coins"] = (
    [(-5, 5), (-4, 5), (-3, 5), (-5, 4)] + [(5, -5), (4, -5), (3, -5), (5, -4)],
    [(-5, -5), (-5, -4), (-5, -3), (-4, -5)] + [(5, 5), (5, 4), (5, 3), (4, 5)])

# 9. plage : chaque equipe en escalier de 3 petits blocs 2x2
stairs = lambda x, y, dx, dy: block(x, y, 2, 2) + block(x + dx, y + dy, 2, 2) + block(x + 2 * dx, y + 2 * dy, 2, 2)
P["capture_escaliers"] = (stairs(3, -5, 1, 2), stairs(-6, -1, 1, 2))

for name, (blues, reds) in P.items():
    assert not set(blues) & set(reds), name
    pts = lambda l: "".join("    <Point>\n      <X>%d</X>\n      <Y>%d</Y>\n    </Point>\n" % xy for xy in l)
    xml = ('<?xml version="1.0" encoding="utf-8"?>\n<PlacementPattern xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" '
           'xmlns:xsd="http://www.w3.org/2001/XMLSchema">\n  <Relative>true</Relative>\n  <Blues>\n%s  </Blues>\n  <Reds>\n%s  </Reds>\n'
           '  <Center>\n    <X>0</X>\n    <Y>0</Y>\n  </Center>\n</PlacementPattern>\n') % (pts(blues), pts(reds))
    open(os.path.join(out, name + ".xml"), "w", encoding="utf-8").write(xml)
    print(name, len(blues), "/", len(reds))
