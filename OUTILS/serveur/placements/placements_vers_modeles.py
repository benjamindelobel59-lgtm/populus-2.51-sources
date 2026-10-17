# NEKFEU 2026-10-16 : transforme les placements de combat de la base (world_maps.BlueCellsCSV / RedCellsCSV,
# relevés des placements officiels) en modeles relatifs (format patterns/*.xml de Stump), doublons retires.
# Utilisation : py -I placements_vers_modeles.py <placements.tsv (Id, SubAreaId, Blue, Red)> <dossier de sortie> [min cases par equipe]
import os, sys

src, out = sys.argv[1], sys.argv[2]
MIN = int(sys.argv[3]) if len(sys.argv) > 3 else 8
W = 14


def coord(cell):
    # inverse de MapPoint.CoordToCellId (grille Stump) : cellId = (x - y) * 14 + y + (x - y) / 2
    for x in range(-20, 60):
        for y in range(-30, 30):
            if (x - y) * W + y + (x - y) // 2 == cell and x + y >= 0 and x - y >= 0 and x - y < 40 and x + y < 28:
                return x, y
    raise ValueError(cell)


cache = {}
def c(cell):
    if cell not in cache:
        cache[cell] = coord(cell)
    return cache[cell]


os.makedirs(out, exist_ok=True)
seen = {}
for line in open(src, encoding="utf-8-sig"):
    p = line.rstrip("\n").split("\t")
    if len(p) < 4:
        continue
    blue = [c(int(v)) for v in p[2].split(",") if v.strip()]
    red = [c(int(v)) for v in p[3].split(",") if v.strip()]
    if len(blue) < MIN or len(red) < MIN:
        continue
    allp = blue + red
    cx = round(sum(x for x, _ in allp) / len(allp)); cy = round(sum(y for _, y in allp) / len(allp))
    b = sorted((x - cx, y - cy) for x, y in blue); r = sorted((x - cx, y - cy) for x, y in red)
    key = (tuple(b), tuple(r)) if (tuple(b), tuple(r)) <= (tuple(r), tuple(b)) else (tuple(r), tuple(b))
    if key in seen:
        continue
    seen[key] = p[0]
    pts = lambda l: "".join("    <Point>\n      <X>%d</X>\n      <Y>%d</Y>\n    </Point>\n" % xy for xy in l)
    xml = ('<?xml version="1.0" encoding="utf-8"?>\n<PlacementPattern xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" '
           'xmlns:xsd="http://www.w3.org/2001/XMLSchema">\n  <Relative>true</Relative>\n  <Blues>\n%s  </Blues>\n  <Reds>\n%s  </Reds>\n'
           '  <Center>\n    <X>0</X>\n    <Y>0</Y>\n  </Center>\n</PlacementPattern>\n') % (pts(b), pts(r))
    open(os.path.join(out, "ankama_%s.xml" % p[0]), "w", encoding="utf-8").write(xml)
print("modeles uniques :", len(seen), "->", out)
