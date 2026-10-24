# PADGREF 21/10 (demande utilisateur : « en jaune ça pique les yeux ») : icônes des challenges de SUCCÈS de boss (objectif EH>id des
# succès « <Boss> (<Challenge>) ») en orange : le fond jaune (ou vert pour les icônes copiées d'un challenge générique) passe à la
# teinte orange, luminosité et saturation gardées ; le pictogramme sombre ne bouge pas. Les challenges normaux (vert) ne changent pas.
# Sortie : succes_orange/client/content/gfx/challenges/<id>.png (identique 32 / 64 bits).
# Usage : py -I generer.py
import colorsys, os, re, sys
from PIL import Image
sys.stdout.reconfigure(encoding='utf-8')
ICI = os.path.dirname(os.path.abspath(__file__))
R = os.path.dirname(os.path.dirname(os.path.dirname(os.path.dirname(ICI))))
sys.path.insert(0, os.path.join(R, '_OUTILS', 'serveur', 'donjons'))
import d2o
APP = os.path.join(R, 'CLIENT_64bits', 'app')
_, A = d2o.load(os.path.join(APP, 'data', 'common', 'Achievements.d2o'))
_, O = d2o.load(os.path.join(APP, 'data', 'common', 'AchievementObjectives.d2o'))
ORANGE = 28 / 360.0

ids = set()
for a in A.values():
    for oid in a['objectiveIds']:
        for cid in re.findall(r'EH>(\d+)', (O.get(oid) or {}).get('criterion') or ''):
            ids.add(int(cid))

sortie = os.path.join(ICI, 'client', 'content', 'gfx', 'challenges')
os.makedirs(sortie, exist_ok=True)
n = 0
for cid in sorted(ids):
    src = os.path.join(APP, 'content', 'gfx', 'challenges', f'{cid}.png')
    if not os.path.isfile(src):
        continue
    im = Image.open(src).convert('RGBA')
    px = im.load()
    for y in range(im.size[1]):
        for x in range(im.size[0]):
            r, g, b, a = px[x, y]
            h, s, v = colorsys.rgb_to_hsv(r / 255, g / 255, b / 255)
            # fond coloré jaune → vert (35° à 110°), assez saturé et lumineux ; le pictogramme sombre reste tel quel
            if a and s > 0.35 and v > 0.35 and 35 / 360 <= h <= 110 / 360:
                r2, g2, b2 = colorsys.hsv_to_rgb(ORANGE, s, v)
                px[x, y] = (round(r2 * 255), round(g2 * 255), round(b2 * 255), a)
    im.save(os.path.join(sortie, f'{cid}.png'), optimize=True)
    n += 1
print(n, 'icônes de succès passées en orange ->', sortie)
