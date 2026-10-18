# MYSTIC 18/10 : ajoute (ou remplace) des textes dans un fichier de langue .d2i du client, identifiants nouveaux compris.
# Même format que d2i_texte.py : les nouveaux textes vont à la fin de la zone des textes, l'index des identifiants est recopié
# (entrées existantes repointées, nouvelles entrées ajoutées à la fin) ; l'index des textes d'interface et la liste triée sont
# recopiés tels quels.
# Usage : py -I d2i_ajout.py <source.d2i> <sortie.d2i> <fichier.json {"id": "texte", ...}>
import json, struct, sys, unicodedata


def utf(s):
    b = s.encode('utf-8')
    return struct.pack('>H', len(b)) + b


def sans_accents(s):
    return ''.join(c for c in unicodedata.normalize('NFD', s) if unicodedata.category(c) != 'Mn').lower()


def ajouter(src, out, textes_voulus):
    d = open(src, 'rb').read()
    io = struct.unpack_from('>i', d, 0)[0]
    textes = bytearray(d[4:io])
    fin = d[io:]
    taille = struct.unpack_from('>i', fin, 0)[0]
    reste = fin[4 + taille:]

    entrees, o = [], 4
    while o < 4 + taille:
        k = struct.unpack_from('>i', fin, o)[0]
        diac = fin[o + 4]
        p1 = struct.unpack_from('>i', fin, o + 5)[0]
        p2 = struct.unpack_from('>i', fin, o + 9)[0] if diac else None
        entrees.append([k, diac, p1, p2])
        o += 9 + (4 if diac else 0)

    nouveaux = {}
    for k, texte in textes_voulus.items():
        p1 = 4 + len(textes); textes += utf(texte)
        p2 = 4 + len(textes); textes += utf(sans_accents(texte))
        nouveaux[int(k)] = (p1, p2)

    existants = set()
    for e in entrees:
        if e[0] in nouveaux:
            e[1], e[2], e[3] = 1, nouveaux[e[0]][0], nouveaux[e[0]][1]
            existants.add(e[0])
    for k, (p1, p2) in nouveaux.items():
        if k not in existants:
            entrees.append([k, 1, p1, p2])

    index = bytearray()
    for k, diac, p1, p2 in entrees:
        index += struct.pack('>i', k) + bytes([diac]) + struct.pack('>i', p1)
        if diac:
            index += struct.pack('>i', p2)
    open(out, 'wb').write(struct.pack('>i', 4 + len(textes)) + bytes(textes) + struct.pack('>i', len(index)) + bytes(index) + reste)
    return len(existants), len(nouveaux) - len(existants)


if __name__ == '__main__':
    r, a = ajouter(sys.argv[1], sys.argv[2], json.load(open(sys.argv[3], encoding='utf-8')))
    print(f'{r} texte(s) remplacé(s), {a} ajouté(s)')
