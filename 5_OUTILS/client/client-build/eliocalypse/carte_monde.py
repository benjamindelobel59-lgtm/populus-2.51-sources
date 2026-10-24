# PADGREF 23/10 (demande utilisateur : « on n'a pas l'affichage de la zone dans la minimap ») : Eliocalypse — images des CARTES DU
# MONDE (gfx/maps/worldmap*.d2p, dossier « <worldMap>/… ») des cartes de l'Eliocalypse absentes de notre client, copiées du 2.74
# dans la dernière archive de la chaîne worldmap0 (config.xml : gfx.path.maps). Les WorldMaps.d2o sont déjà posés par donnees.py.
import glob, json, os, sys
sys.stdout.reconfigure(encoding='utf-8')
ICI = os.path.dirname(os.path.abspath(__file__))
CB = os.path.dirname(ICI)
R = os.path.abspath(os.path.join(CB, '..', '..', '..'))
sys.path.insert(0, CB); sys.path.insert(0, ICI)
import maps_elements as me
from d2o_ecrire import D2O
from cartes_decor_lib import lire_d2p, ecrire_d2p, fin_de_chaine
A = os.path.join(R, 'CLIENT_64bits', 'app', 'content', 'gfx', 'maps')
N = os.path.join(R, '_OUTILS', 'client', 'client_27x')
SORTIE = os.path.join(ICI, 'client', 'content', 'gfx', 'maps')
inv = json.load(open(os.path.join(R, '_OUTILS', 'serveur', 'donjons', 'eliocalypse', 'inventaire.json'), encoding='utf-8'))
MP = D2O(os.path.join(N, 'data', 'common', 'MapPositions.d2o'))
mondes = {MP.objet(m)['worldMap'] for m in inv['cartes'] if MP.objet(m)} - {-1}
deja = {n.split('/')[0] for f in glob.glob(os.path.join(A, '*.d2p')) for n, _ in me.read_d2p(f)}
voulues = {}
for f in glob.glob(os.path.join(N, 'content', 'gfx', 'maps', 'worldmap0*.d2p')):
    for n, b in me.read_d2p(f):
        w = n.split('/')[0]
        if w.isdigit() and int(w) in mondes and w not in deja:
            voulues[n] = b
dernier, _ = fin_de_chaine(A, 'worldmap0.d2p')
head, ent, props, pc = lire_d2p(dernier)
ent = [e for e in ent if e[0] not in voulues] + [[k, v] for k, v in sorted(voulues.items())]
ecrire_d2p(os.path.join(SORTIE, os.path.basename(dernier)), head, ent, props, pc)
print('cartes du monde :', sorted(mondes), '| déjà chez nous :', sorted(int(w) for w in deja if w.isdigit() and int(w) in mondes),
      '| ajoutées :', sorted({k.split('/')[0] for k in voulues}), len(voulues), 'images',
      sum(len(b) for b in voulues.values()) // 1048576, 'Mo ->', os.path.basename(dernier))
