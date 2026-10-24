# PADGREF 23/10 (retour utilisateur : « se téléporter sur une carte via la minimap (admin) nous téléporte dans Amakna ») :
# le client choisit la carte d'une position avec MapCoordinates.d2o (CharacterDisplacementManager.getOrderedMapIdsFromCoords,
# puis « moveto <carte> ») : les cartes de l'Eliocalypse n'y étaient pas -> seule la carte d'Amakna aux mêmes coordonnées.
# Ajout des cartes de l'Eliocalypse (et de l'Arche) aux coordonnées du 2.74.
import json, os, sys
sys.stdout.reconfigure(encoding='utf-8')
ICI = os.path.dirname(os.path.abspath(__file__))
CB = os.path.dirname(ICI)
R = os.path.abspath(os.path.join(CB, '..', '..', '..'))
sys.path.insert(0, CB)
from d2o_ecrire import D2O
inv = json.load(open(os.path.join(R, '_OUTILS', 'serveur', 'donjons', 'eliocalypse', 'inventaire.json'), encoding='utf-8'))
cartes = set(inv['cartes']) | {202899464}
nos = D2O(os.path.join(R, 'CLIENT_64bits', 'app', 'data', 'common', 'MapCoordinates.d2o'))
leur = D2O(os.path.join(R, '_OUTILS', 'client', 'client_27x', 'data', 'common', 'MapCoordinates.d2o'))
ajouts, crees = 0, 0
for k in list(leur.index):
    o = leur.objet(k)
    eux = [m for m in o['mapIds'] if int(m) in cartes]
    if not eux:
        continue
    a = nos.objet(k) if k in nos.index else None
    if a is None:
        a = {'__classe': o['__classe'], 'compressedCoords': k, 'mapIds': []}; crees += 1
    a = dict(a); a['mapIds'] = list(a['mapIds'])
    for m in eux:
        if m not in a['mapIds']:
            a['mapIds'].append(m); ajouts += 1
    nos.poser(k, a)
os.makedirs(os.path.join(ICI, 'client', 'data', 'common'), exist_ok=True)
nos.ecrire(os.path.join(ICI, 'client', 'data', 'common', 'MapCoordinates.d2o'))
print('cartes ajoutées :', ajouts, '| nouvelles coordonnées :', crees)
