# PADGREF 23/10 (import 2.74, point 3 « Emblèmes de guilde / alliance » validé par l'utilisateur) : symboles d'emblème du 2.74
# absents du nôtre (170, dont la nouvelle catégorie « Dofus »).
#   - client : EmblemSymbols.d2o + EmblemSymbolCategories.d2o (texte de la catégorie dans l'i18n cumulé, id 9098000),
#     images gfx/emblems/up/<id>.swf (grandes) et gfx/emblems/icons/up/<id>.png (petites), fichiers libres comme les nôtres ;
#   - serveur : SQL 126 (guilds_emblems, lu au démarrage : contrôle des emblèmes à la création de guilde / d'alliance).
# Sortie : import274/client/... + _OUTILS/serveur/import274/126_emblemes_274.sql (+ annuler).
import json, os, shutil, subprocess, sys
sys.stdout.reconfigure(encoding='utf-8')
ICI = os.path.dirname(os.path.abspath(__file__))
CB = os.path.dirname(ICI)
R = os.path.abspath(os.path.join(CB, '..', '..', '..'))
sys.path.insert(0, CB); sys.path.insert(0, os.path.join(R, '_OUTILS', 'serveur', 'donjons'))
import d2o, d2i
from d2o_ecrire import D2O
A = os.path.join(R, 'CLIENT_64bits', 'app')
N = os.path.join(R, '_OUTILS', 'client', 'client_27x')
SORTIE = os.path.join(ICI, 'client')
I18N = os.path.join(SORTIE, 'data', 'i18n', 'i18n_fr.d2i')          # i18n cumulé (lots précédents)
TEXTE0 = 9098000
SQL = os.path.join(R, '_OUTILS', 'serveur', 'import274')
T = d2i.load(os.path.join(N, 'data', 'i18n', 'i18n_fr.d2i'))


def classer(fichier, o):
    cid = next(c for c, (_, ch) in fichier.classes.items() if {n for n, _ in ch} == set(o) - {'__classe'})
    return {'__classe': cid, **o}


SN = d2o.load(os.path.join(N, 'data', 'common', 'EmblemSymbols.d2o'))[1]
CN = d2o.load(os.path.join(N, 'data', 'common', 'EmblemSymbolCategories.d2o'))[1]
SW = D2O(os.path.join(A, 'data', 'common', 'EmblemSymbols.d2o'))
CW = D2O(os.path.join(A, 'data', 'common', 'EmblemSymbolCategories.d2o'))
nouveaux = sorted(set(SN) - set(SW.index))
for i in nouveaux:
    SW.poser(i, classer(SW, SN[i]))
textes = {}
for c in sorted(set(CN) - set(CW.index)):
    textes[str(TEXTE0 + len(textes))] = T.get(CN[c]['nameId']) or 'Dofus'
    CW.poser(c, classer(CW, {'id': c, 'nameId': TEXTE0 + len(textes) - 1}))
os.makedirs(os.path.join(SORTIE, 'data', 'common'), exist_ok=True)
SW.ecrire(os.path.join(SORTIE, 'data', 'common', 'EmblemSymbols.d2o'))
CW.ecrire(os.path.join(SORTIE, 'data', 'common', 'EmblemSymbolCategories.d2o'))
if textes:
    jf = os.path.join(ICI, 'textes_lot6.json')
    json.dump(textes, open(jf, 'w', encoding='utf-8'), ensure_ascii=False)
    tmp = I18N + '.tmp'
    subprocess.run([sys.executable, '-I', os.path.join(CB, 'd2i_ajout.py'), I18N, tmp, jf], check=True)
    os.replace(tmp, I18N)
# images
manque = []
for i in nouveaux:
    ic = SN[i]['iconId']
    for rel in (f'gfx/emblems/up/{ic}.swf', f'gfx/emblems/icons/up/{ic}.png'):
        src = os.path.join(N, 'content', rel)
        if not os.path.exists(src):
            manque.append(rel); continue
        if not os.path.exists(os.path.join(A, 'content', rel)):
            os.makedirs(os.path.dirname(os.path.join(SORTIE, 'content', rel)), exist_ok=True)
            shutil.copy2(src, os.path.join(SORTIE, 'content', rel))
# serveur
b = lambda v: 1 if v else 0
lignes = ',\n'.join(f"({s['id']}, {s['iconId']}, {s['skinId']}, {s['order']}, {s['categoryId']}, {b(s['colorizable'])})" for s in (SN[i] for i in nouveaux))
ids = ', '.join(map(str, nouveaux))
open(os.path.join(SQL, '126_emblemes_274.sql'), 'w', encoding='utf-8', newline='\n').write(
    "-- PADGREF 23/10/2026 : SQL 126 — 170 symboles d'emblème du client 2.74 (guildes / alliances, catégorie « Dofus » comprise).\n"
    "-- Généré par import274/lot6_emblemes.py. Base world. Rejouable. Lu au démarrage du World.\n"
    "START TRANSACTION;\n"
    f"DELETE FROM guilds_emblems WHERE Id IN ({ids});\n"
    "INSERT INTO guilds_emblems (Id, IconId, SkinId, `Order`, CategoryId, Colorizable) VALUES\n" + lignes + ";\nCOMMIT;\n")
open(os.path.join(SQL, '126_emblemes_274-annuler.sql'), 'w', encoding='utf-8', newline='\n').write(
    "-- PADGREF 23/10 : annule 126\n" f"DELETE FROM guilds_emblems WHERE Id IN ({ids});\n")
print('symboles', len(nouveaux), '| catégories', textes, '| images manquantes', manque)
