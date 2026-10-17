# NEKFEU 2026-10-16 : garde les modeles ou les equipes sont separees (pas de corps a corps, pas de melange).
# Utilisation : py -I filtrer_modeles.py <dossier modeles> <dossier sortie> [distance min bleu-rouge]
import glob, os, shutil, sys
import xml.etree.ElementTree as ET

src, out = sys.argv[1], sys.argv[2]
DMIN = int(sys.argv[3]) if len(sys.argv) > 3 else 3
os.makedirs(out, exist_ok=True)


def dist(a, b):
    # distance en cases sur la grille Stump (x, y) : max des ecarts sur les 2 axes diagonaux = distance de Manhattan en cases
    return abs(a[0] - b[0]) + abs(a[1] - b[1])


kept = 0
for f in glob.glob(os.path.join(src, "*.xml")):
    t = ET.parse(f).getroot()
    pts = lambda tag: [(int(p.find("X").text), int(p.find("Y").text)) for p in t.find(tag).findall("Point")]
    b, r = pts("Blues"), pts("Reds")
    inter = min(dist(x, y) for x in b for y in r)
    cb = (sum(x for x, _ in b) / len(b), sum(y for _, y in b) / len(b))
    cr = (sum(x for x, _ in r) / len(r), sum(y for _, y in r) / len(r))
    centres = abs(cb[0] - cr[0]) + abs(cb[1] - cr[1])
    # chaque case est plus proche du centre de son equipe que du centre adverse (equipes de chaque cote, pas melangees)
    side_ok = all(dist(p, cb) < dist(p, cr) for p in b) and all(dist(p, cr) < dist(p, cb) for p in r)
    if inter >= DMIN and side_ok and centres >= 6:
        shutil.copy(f, out)
        kept += 1
print("modeles gardes :", kept)
