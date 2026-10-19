# MYSTIC 19/10 : fichiers client de la maison-atelier d'Astrub : archive de cartes contenant 191890437 (nouvel intérieur) et
# Hints.d2o (repère « Atelier des forgemages », icône 308, en [5,-17]). Sortie : client/ ; installation : INSTALLER_jeu_ferme.bat
import os, sys, zlib
ICI = os.path.dirname(os.path.abspath(__file__)); CB = os.path.dirname(ICI)
sys.path.insert(0, ICI); sys.path.insert(0, CB)
import dlm
from d2o_ecrire import D2O
from dupliquer_maps_client import read_d2p_full, write_d2p
RACINE = os.path.abspath(os.path.join(ICI, '..', '..', '..', '..'))
APP = os.path.join(RACINE, 'CLIENT_64bits', 'app')
CIBLE = 191890437
idx = dlm.index_cartes(os.path.join(APP, 'content', 'maps'))
f, name = idx[CIBLE]
head, entries, props, pc = read_d2p_full(os.path.join(APP, 'content', 'maps', f))
nouveau = zlib.compress(open(os.path.join(ICI, 'client', f'{CIBLE}.dlm'), 'rb').read(), 9)
for e in entries:
    if e[0] == name:
        e[1] = nouveau
write_d2p(os.path.join(ICI, 'client', f), head, entries, props, pc)
h = D2O(os.path.join(APP, 'data', 'common', 'Hints.d2o'))
modele = h.objet(2885)
h.poser(3480, dict(modele, id=3480, mapId=191105028, x=5, y=-17, outdoor=True, subareaId=95, worldMapId=1, realMapId=0))
h.ecrire(os.path.join(ICI, 'client', 'Hints.d2o'))
open(os.path.join(ICI, 'client', 'archive_cartes.txt'), 'w').write(f)
print('archive', f, '; Hints 3480')
