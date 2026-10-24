# PADGREF 21/10 : remplace un texte d'INTERFACE nommé (ex. ui.popup.accessDenied.badVersion) d'un fichier de langue .d2i.
# Le nouveau texte est ajouté à la fin de la zone des textes ; seul le pointeur du nom change dans l'index des textes d'interface
# (les index, placés après les textes, sont recopiés avec le nouveau décalage).
# Usage : py -I d2i_ui_texte.py <source.d2i> <sortie.d2i> <nom>=<texte> [<nom>=<texte> ...]
import struct, sys

src, out = sys.argv[1], sys.argv[2]
changes = dict(a.split('=', 1) for a in sys.argv[3:])
d = open(src, 'rb').read()
io = struct.unpack_from('>i', d, 0)[0]
textes = bytearray(d[4:io])
fin = bytearray(d[io:])

nouveaux = {}
for nom, texte in changes.items():
    b = texte.encode('utf-8')
    nouveaux[nom] = 4 + len(textes)
    textes += struct.pack('>H', len(b)) + b
decal = 4 + len(textes) - io          # tous les pointeurs existants restent valides (les textes ne bougent pas)

# index des identifiants : pointeurs inchangés (ils visent la zone des textes, qui ne bouge pas)
t = struct.unpack_from('>i', fin, 0)[0]
o = 4 + t
u = struct.unpack_from('>i', fin, o)[0]
o += 4
e = o + u
vus = set()
while o < e:
    l = struct.unpack_from('>H', fin, o)[0]
    nom = fin[o + 2:o + 2 + l].decode('utf-8')
    o += 2 + l
    if nom in nouveaux:
        struct.pack_into('>i', fin, o, nouveaux[nom])
        vus.add(nom)
    o += 4
manque = set(nouveaux) - vus
assert not manque, ('noms introuvables', manque)
open(out, 'wb').write(struct.pack('>i', 4 + len(textes)) + bytes(textes) + bytes(fin))
print(len(vus), 'texte(s) d\'interface remplace(s)')
