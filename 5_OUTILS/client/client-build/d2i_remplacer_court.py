# PADGREF 20/10 : remplace SUR PLACE un texte d'un fichier de langue .d2i par un texte PLUS COURT (ou de même taille).
# Marche aussi pour les textes d'interface nommés (ex. ui.banner.shopGifts), que d2i_texte.py / d2i_ajout.py ne gèrent pas :
# chaque texte est une chaîne UTF (longueur sur 2 octets + octets) ; on réécrit la longueur et le début, les positions ne
# bougent pas (les octets en trop restent, jamais lus). Toutes les occurrences exactes sont remplacées (y compris la copie
# sans accents utilisée pour la recherche).
# Usage : py -I d2i_remplacer_court.py <source.d2i> <sortie.d2i> <fichier.json {"ancien texte": "nouveau texte", ...}>
import json, struct, sys
sys.stdout.reconfigure(encoding='utf-8')

src, out, fichier = sys.argv[1], sys.argv[2], sys.argv[3]
d = bytearray(open(src, 'rb').read())
for ancien, nouveau in json.load(open(fichier, encoding='utf-8')).items():
    a, n = ancien.encode('utf-8'), nouveau.encode('utf-8')
    if len(n) > len(a):
        sys.exit(f'« {nouveau} » est plus long que « {ancien} » : impossible sur place')
    motif = struct.pack('>H', len(a)) + a
    nb, p = 0, d.find(motif)
    while p != -1:
        d[p:p + 2] = struct.pack('>H', len(n))
        d[p + 2:p + 2 + len(n)] = n
        nb += 1
        p = d.find(motif, p + 2)
    print(f'{nb} × « {ancien} » → « {nouveau} »')
open(out, 'wb').write(d)
