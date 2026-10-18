# MYSTIC 17/10 : remplace le texte d'un ou plusieurs identifiants d'un fichier de langue .d2i du client.
# Le nouveau texte est ajouté à la fin de la zone des textes ; seuls les pointeurs de l'identifiant changent (les index, placés après
# les textes, sont recopiés avec le nouveau décalage). Les autres textes ne bougent pas.
# Format : int position de l'index | textes (UTF : short longueur + octets) | index : int taille, entrées (int clé, bool diacritique,
# int pointeur [, int pointeur sans diacritique]) | index des textes d'interface | liste triée.
# Usage : py -I d2i_texte.py <source.d2i> <sortie.d2i> <id>=<texte> [<id>=<texte> ...]
import struct, sys
import unicodedata

src, out = sys.argv[1], sys.argv[2]
changes = {int(a.split('=', 1)[0]): a.split('=', 1)[1] for a in sys.argv[3:]}
d = bytearray(open(src, 'rb').read())

io = struct.unpack_from('>i', d, 0)[0]
textes = bytearray(d[4:io])
fin = bytes(d[io:])


def utf(s):
    b = s.encode('utf-8')
    return struct.pack('>H', len(b)) + b


def sans_accents(s):
    return ''.join(c for c in unicodedata.normalize('NFD', s) if unicodedata.category(c) != 'Mn').lower()


nouveaux = {}
for k, texte in changes.items():
    nouveaux[k] = (4 + len(textes), None)
    textes += utf(texte)
    p2 = 4 + len(textes)
    textes += utf(sans_accents(texte))
    nouveaux[k] = (nouveaux[k][0], p2)

decalage = (4 + len(textes)) - io
fin = bytearray(fin)
taille = struct.unpack_from('>i', fin, 0)[0]
o, trouves = 4, set()
while o < 4 + taille:
    k = struct.unpack_from('>i', fin, o)[0]
    diac = fin[o + 4]
    if k in nouveaux:
        struct.pack_into('>i', fin, o + 5, nouveaux[k][0])
        if diac:
            struct.pack_into('>i', fin, o + 9, nouveaux[k][1])
        trouves.add(k)
    o += 9 + (4 if diac else 0)

manquants = set(nouveaux) - trouves
assert not manquants, f'identifiants introuvables : {manquants}'
open(out, 'wb').write(struct.pack('>i', 4 + len(textes)) + bytes(textes) + bytes(fin))
print(f'{len(trouves)} texte(s) remplacé(s), index décalé de {decalage} octets')
