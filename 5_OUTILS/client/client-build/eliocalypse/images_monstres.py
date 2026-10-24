# PADGREF 23/10 (retour utilisateur : « dans le bestiaire aucun skin des mobs ») : Eliocalypse — images du bestiaire
# (gfx/monsters/<id>.png) des monstres importés, copiées du 2.74 dans la dernière archive de la chaîne monsters0.
import glob, json, os, sys
sys.stdout.reconfigure(encoding='utf-8')
ICI = os.path.dirname(os.path.abspath(__file__))
CB = os.path.dirname(ICI)
R = os.path.abspath(os.path.join(CB, '..', '..', '..'))
sys.path.insert(0, CB); sys.path.insert(0, ICI)
import maps_elements as me
from cartes_decor_lib import lire_d2p, ecrire_d2p, fin_de_chaine
A = os.path.join(R, 'CLIENT_64bits', 'app', 'content', 'gfx', 'monsters')
N = os.path.join(R, '_OUTILS', 'client', 'client_27x', 'content', 'gfx', 'monsters')
SORTIE = os.path.join(ICI, 'client', 'content', 'gfx', 'monsters')
d = json.load(open(os.path.join(ICI, 'donnees.json'), encoding='utf-8'))
voulus = {f'{m}.png' for m in d['monstres']}
deja = {n for f in glob.glob(os.path.join(A, '*.d2p')) for n, _ in me.read_d2p(f)}
images = {n: b for f in glob.glob(os.path.join(N, '*.d2p')) for n, b in me.read_d2p(f) if n in voulus and n not in deja}
dernier, _ = fin_de_chaine(A, 'monsters0.d2p')
head, ent, props, pc = lire_d2p(dernier)
ent = [e for e in ent if e[0] not in images] + [[k, v] for k, v in sorted(images.items())]
ecrire_d2p(os.path.join(SORTIE, os.path.basename(dernier)), head, ent, props, pc)
print('images ajoutées :', len(images), '/', len(voulus), '-> ', os.path.basename(dernier), '| absentes :', sorted(voulus - set(images) - deja))
