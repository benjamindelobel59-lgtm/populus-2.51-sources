# PADGREF 23/10 (retour utilisateur : « dans la minimap aucune icône pour l'endroit du donjon ») : Eliocalypse — INDICES de carte
# (Hints.d2o : icônes de la carte du monde / minimap) du 2.74 sur nos cartes : entrées des 4 donjons + zaap et donjon de l'Arche.
# Identifiants 9101+ (ceux du 2.74 sont déjà pris chez nous), textes 9050000+ ajoutés à l'i18n cumulé de l'Eliocalypse.
import json, os, shutil, subprocess, sys
sys.stdout.reconfigure(encoding='utf-8')
ICI = os.path.dirname(os.path.abspath(__file__))
CB = os.path.dirname(ICI)
R = os.path.abspath(os.path.join(CB, '..', '..', '..'))
sys.path.insert(0, CB)
from d2o_ecrire import D2O
sys.path.insert(0, os.path.join(R, '_OUTILS', 'serveur', 'donjons'))
import d2i
APP = os.path.join(R, 'CLIENT_64bits', 'app', 'data')
N = os.path.join(R, '_OUTILS', 'client', 'client_27x', 'data')
SORTIE = os.path.join(ICI, 'client', 'data')
T = d2i.load(os.path.join(N, 'i18n', 'i18n_fr.d2i'))
inv = json.load(open(os.path.join(R, '_OUTILS', 'serveur', 'donjons', 'eliocalypse', 'inventaire.json'), encoding='utf-8'))
cartes = set(inv['cartes']) | {202899464}
src, dst = D2O(os.path.join(N, 'common', 'Hints.d2o')), D2O(os.path.join(APP, 'common', 'Hints.d2o'))
leurs = [h for h in (src.objet(k) for k in list(src.index)) if h and int(h['mapId']) in cartes and not (int(h['mapId']) == 202899464 and h['categoryId'] == 6)]  # pas la Tempête (donjon 2.74 absent)
textes, ID0, TEXTE0 = {}, 9101, 9050000
for n, h in enumerate(sorted(leurs, key=lambda h: h['id'])):
    h = dict(h)
    textes[TEXTE0 + n] = T.get(h['nameId']) or 'Eliocalypse'
    h['id'] = ID0 + n
    h['nameId'] = TEXTE0 + n
    dst.poser(h['id'], h)
    print(h['id'], textes[TEXTE0 + n], h['mapId'], h['x'], h['y'], 'carte du monde', h['worldMapId'], 'icône', h['gfx'])
os.makedirs(os.path.join(SORTIE, 'common'), exist_ok=True)
dst.ecrire(os.path.join(SORTIE, 'common', 'Hints.d2o'))
json.dump({str(k): v for k, v in textes.items()}, open(os.path.join(ICI, 'textes_indices.json'), 'w', encoding='utf-8'), ensure_ascii=False)
i18n = os.path.join(SORTIE, 'i18n', 'i18n_fr.d2i')
tmp = i18n + '.tmp'
subprocess.run([sys.executable, '-I', os.path.join(CB, 'd2i_ajout.py'), i18n, tmp, os.path.join(ICI, 'textes_indices.json')], check=True)
shutil.move(tmp, i18n)
