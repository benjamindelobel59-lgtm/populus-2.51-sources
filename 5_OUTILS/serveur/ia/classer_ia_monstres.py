# MYSTIC 19/10 : classe les monstres (hors boss) selon la portée de leurs sorts de dégâts (niveau 1, client 2.51) :
#   distance = aucune attaque au contact (portée min >= 2) et une attaque à 4 cases ou plus -> MonstresFuyards.cs (fuite à portée)
#   contact = toutes les attaques à 1 case ; mixte = le reste ; sans_degats = aucun sort de dégâts.
# Usage : py -I _OUTILS/serveur/ia/classer_ia_monstres.py  -> affiche les comptes et écrit categories.json à côté.
import json, os, sys
ICI = os.path.dirname(os.path.abspath(__file__))
RACINE = os.path.abspath(os.path.join(ICI, '..', '..', '..'))
sys.path.insert(0, os.path.join(RACINE, '_OUTILS', 'client', 'client-build'))
from d2o_ecrire import D2O
C = os.path.join(RACINE, 'CLIENT_64bits', 'app', 'data', 'common')
mo, sp, sl = (D2O(os.path.join(C, f)) for f in ('Monsters.d2o', 'Spells.d2o', 'SpellLevels.d2o'))
DMG = {82, 91, 92, 93, 94, 95, 96, 97, 98, 99, 100, 1012, 1013, 1014, 1015, 1016, 1063, 1064, 1065, 1066, 1067, 1068, 1069, 1070, 1071,
       1123, 1124, 1125, 1126, 1127}
cat = {'distance': [], 'contact': [], 'mixte': [], 'sans_degats': []}
for k in mo.index:
    m = mo.objet(k)
    if m.get('isBoss') or not m.get('spells'):
        continue
    mins, maxs = [], []
    for s in m['spells']:
        try:
            L = sl.objet(sp.objet(s)['spellLevels'][0])
        except Exception:
            continue
        if any(e['effectId'] in DMG for e in L.get('effects', [])):
            mins.append(L.get('minRange', 0)); maxs.append(L.get('range', 0))
    if not mins:
        cat['sans_degats'].append(k)
    elif min(mins) >= 2 and max(maxs) >= 4:
        cat['distance'].append(k)
    elif max(maxs) <= 1:
        cat['contact'].append(k)
    else:
        cat['mixte'].append(k)
for c, v in cat.items():
    print(c, len(v))
json.dump(cat, open(os.path.join(ICI, 'categories.json'), 'w'))
