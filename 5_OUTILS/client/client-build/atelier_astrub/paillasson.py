# PADGREF 24/10 (retour utilisateur : « dans le nouvel atelier FM 191890437, il n'y a pas de soleil ni d'interactif pour retourner à
# Astrub ») : la sortie existe (cases 531, 544, 545, 557-559 = téléporteurs vers la porte verte) mais rien ne la montre. On pose le
# paillasson des sorties de maison (gfx 30991, copié de la sortie de 54169416 case 518, calque 1) sur la case 544, au milieu de la sortie.
# Part de la carte INSTALLÉE dans CLIENT_64bits ; écrit client/191890437.dlm puis l'archive de cartes (client_fichiers.py).
import os, sys, struct
ICI = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, ICI); sys.path.insert(0, os.path.dirname(ICI))
import dlm
RACINE = os.path.abspath(os.path.join(ICI, '..', '..', '..', '..'))
MAPS = os.path.join(RACINE, 'CLIENT_64bits', 'app', 'content', 'maps')
CIBLE, SOURCE, CASE_SOURCE, CASE = 191890437, 54169416, 518, 544
idx = dlm.index_cartes(MAPS)
source = dlm.Carte(dlm.lire(MAPS, idx, SOURCE))
li, brut = next((li, e) for li, c, e, i, g in source.elements() if c == CASE_SOURCE and g == 30991)
carte = dlm.Carte(dlm.lire(MAPS, idx, CIBLE))
if any(c == CASE and g == 30991 for _, c, _, _, g in carte.elements()):
    print('paillasson déjà posé')
else:
    carte.ajouter(li, CASE, brut, 0)
os.makedirs(os.path.join(ICI, 'client'), exist_ok=True)
open(os.path.join(ICI, 'client', f'{CIBLE}.dlm'), 'wb').write(carte.octets(CIBLE))
print('paillasson : calque', li, 'case', CASE)
