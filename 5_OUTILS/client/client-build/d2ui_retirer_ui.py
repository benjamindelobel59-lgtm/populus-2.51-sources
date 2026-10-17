# MYSTIC 17/10 : retire une ou plusieurs fenêtres précompilées d'un fichier .d2ui (module d'interface du client).
# Le client (UiRenderManager.loadUi) lit alors le fichier XML de la fenêtre (xml/<nom>.xml) au lieu de la version précompilée :
# c'est ce qui permet de modifier une fenêtre en éditant son XML. Les autres fenêtres restent précompilées.
# Format (PreCompiledUiModule.as) : UTF "D2UI", UTF xml du module, short nombre, nombre × (UTF nom, int position absolue), données AMF3.
# Usage : py -I d2ui_retirer_ui.py <source.d2ui> <sortie.d2ui> <nom> [<nom> ...]
import struct, sys

src, out, noms = sys.argv[1], sys.argv[2], set(sys.argv[3:])
d = open(src, 'rb').read()


def utf(p):
    n = struct.unpack_from('>H', d, p)[0]
    return d[p + 2:p + 2 + n], p + 2 + n


head, p = utf(0)
assert head == b'D2UI'
xml, p = utf(p)
debut_compte = p
n = struct.unpack_from('>h', d, p)[0]; p += 2
entrees = []
for _ in range(n):
    nom, p = utf(p)
    pos = struct.unpack_from('>i', d, p)[0]; p += 4
    entrees.append((nom, pos))
donnees = d[p:]
debut_donnees = p
gardees = [(nom, pos) for nom, pos in entrees if nom.decode() not in noms]
retirees = [nom.decode() for nom, _ in entrees if nom.decode() in noms]
assert len(retirees) == len(noms), ('introuvable', noms - set(retirees))
liste = b''.join(struct.pack('>H', len(nom)) + nom + b'\0\0\0\0' for nom, _ in gardees)
nouveau_debut = debut_compte + 2 + len(liste)
decalage = nouveau_debut - debut_donnees
liste = b''.join(struct.pack('>H', len(nom)) + nom + struct.pack('>i', pos + decalage) for nom, pos in gardees)
res = d[:debut_compte] + struct.pack('>h', len(gardees)) + liste + donnees
open(out, 'wb').write(res)
print('fenetres :', n, '->', len(gardees), '; retirees :', retirees, '; decalage', decalage)
