# NEKFEU : images des challenges de boss manquantes (carre violet dans les succes, ex. « Haute Truche (Barbare) » -> challenge 304).
# Les succes « <Boss> (<Challenge>) » ont un objectif EH>id : le challenge id est la version boss du challenge generique <Challenge>.
# -> copie de content/gfx/challenges/<id generique>.png vers <id>.png quand <id>.png manque.
# Usage : python -I icones_challenges.py <dossier d2o.py/d2i.py> <dossier app du client> [<autre dossier app>...]
import os, re, shutil, sys
sys.path.insert(0, sys.argv[1])
import d2o, d2i

ref = sys.argv[2]
_, ach = d2o.load(os.path.join(ref, 'data/common/Achievements.d2o'))
_, obj = d2o.load(os.path.join(ref, 'data/common/AchievementObjectives.d2o'))
_, ch = d2o.load(os.path.join(ref, 'data/common/Challenge.d2o'))
t = d2i.load(os.path.join(ref, 'data/i18n/i18n_fr.d2i'))
generic = {}
for k, c in ch.items():
    generic.setdefault(t.get(c['nameId'], '').strip().lower(), k)

pairs = {}
for a in ach.values():
    m = re.search(r'\(([^()]+)\)\s*$', t.get(a['nameId'], ''))
    if not m or m.group(1).strip().lower() not in generic:
        continue
    for oid in a['objectiveIds']:
        for cid in re.findall(r'EH>(\d+)', (obj.get(oid) or {}).get('criterion') or ''):
            pairs.setdefault(int(cid), generic[m.group(1).strip().lower()])

for app in sys.argv[2:]:
    d = os.path.join(app, 'content/gfx/challenges')
    copied = 0
    for cid, gid in sorted(pairs.items()):
        dst, src = os.path.join(d, '%d.png' % cid), os.path.join(d, '%d.png' % gid)
        if not os.path.exists(dst) and os.path.exists(src):
            shutil.copyfile(src, dst)
            copied += 1
            print('%s : %d.png <- %d.png (%s)' % (os.path.basename(os.path.dirname(app)) or app, cid, gid, t.get(ch[cid]['nameId'], '') if cid in ch else ''))
    left = [k for k in ch if not os.path.exists(os.path.join(d, '%d.png' % k))]
    print('%s : %d copies, %d challenges encore sans image' % (app, copied, len(left)))
