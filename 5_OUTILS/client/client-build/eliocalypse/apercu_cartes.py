# PADGREF 22/10 : Eliocalypse — rendu PNG de cartes importées (contrôle visuel) avec rendu_carte.py, sur un dossier de test :
# liens physiques vers les archives du client + fichiers importés (client/content) à la place des originaux.
# Utilisation : py -I apercu_cartes.py <sortie> <mapId> [...]
import glob, os, shutil, sys
ICI = os.path.dirname(os.path.abspath(__file__))
R = os.path.abspath(os.path.join(ICI, '..', '..', '..', '..'))
APP = os.path.join(R, 'CLIENT_64bits', 'app', 'content')
TEST = os.path.join(ICI, '_test_content')
for sous in ('maps', os.path.join('gfx', 'world')):
    os.makedirs(os.path.join(TEST, sous), exist_ok=True)
    for f in glob.glob(os.path.join(APP, sous, '*')):
        dst = os.path.join(TEST, sous, os.path.basename(f))
        imp = os.path.join(ICI, 'client', 'content', sous, os.path.basename(f))
        src = imp if os.path.isfile(imp) else f
        if os.path.exists(dst):
            os.remove(dst)
        try:
            os.link(src, dst)
        except OSError:
            shutil.copy2(src, dst)
sys.path.insert(0, os.path.join(R, '_OUTILS', 'client', 'rendu_carte'))
sys.argv = [sys.argv[0]] + sys.argv[1:]
import importlib.util
spec = importlib.util.spec_from_file_location('rc', os.path.join(R, '_OUTILS', 'client', 'rendu_carte', 'rendu_carte.py'))
src = open(spec.origin, encoding='utf-8').read()
src = src.split("\nif __name__")[0] if "\nif __name__" in src else src
ns = {'__file__': spec.origin, '__name__': 'rc'}
code = src
exec(compile(code.replace("CONTENU = os.path.join(R, 'CLIENT_64bits', 'app', 'content')", f"CONTENU = r'{TEST}'"), spec.origin, 'exec'), ns)
ns['cases_praticables'] = lambda m: [True] * 560
els = ns['lire_elements']()
idx = ns['index_gfx'](os.path.join(ICI, '_index_gfx_test.json'))
sortie = sys.argv[1]
os.makedirs(sortie, exist_ok=True)
for m in sys.argv[2:]:
    ns['rendre'](int(m), sortie, els, idx)
    print('ok', m)
