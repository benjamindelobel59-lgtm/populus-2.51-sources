# MYSTIC 18/10/2026 : extrait du client les icônes utilisées par l'Atelier du site.
# Lit icones_objets.txt / icones_monstres.txt (écrits par l'exporteur) et copie les PNG des archives .d2p
# du client vers atelier/icones/objets/<IconId>.png et atelier/icones/monstres/<Id du monstre>.png.
# Usage : py extraire_icones.py   (chemins du projet par défaut)
import os, struct, sys

RACINE = os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..', '..'))
GFX = os.path.join(RACINE, 'CLIENT_64bits', 'app', 'content', 'gfx')
SITE = os.path.join(RACINE, '_OUTILS', 'web', 'site', 'mystic', 'atelier')


def archive(chemin):
    d = open(chemin, 'rb').read()
    data_off, _, idx_off, idx_cnt, _, _ = struct.unpack('>6I', d[-24:])
    o = idx_off
    for _ in range(idx_cnt):
        n = struct.unpack('>H', d[o:o + 2])[0]
        nom = d[o + 2:o + 2 + n].decode('utf-8')
        o += 2 + n
        off, ln = struct.unpack('>ii', d[o:o + 8])
        o += 8
        yield nom, d[data_off + off:data_off + off + ln]


def extraire(liste, archives, sortie):
    voulus = {l.strip() for l in open(os.path.join(SITE, 'donnees', liste), encoding='utf-8') if l.strip()}
    os.makedirs(sortie, exist_ok=True)
    trouves = set()
    for a in archives:
        for nom, octets in archive(os.path.join(GFX, a)):
            base = nom.rsplit('.', 1)[0]
            if base in voulus and base not in trouves and octets[:4] == b'\x89PNG':
                open(os.path.join(sortie, base + '.png'), 'wb').write(octets)
                trouves.add(base)
    print(f'{sortie} : {len(trouves)} / {len(voulus)} icônes ({len(voulus - trouves)} absentes du client)')


extraire('icones_objets.txt', ['items/bitmap0.d2p', 'items/bitmap0_1.d2p', 'items/bitmap1.d2p', 'items/bitmap1_1.d2p'],
         os.path.join(SITE, 'icones', 'objets'))
extraire('icones_monstres.txt', ['monsters/monsters0.d2p', 'monsters/monsters0_1.d2p'], os.path.join(SITE, 'icones', 'monstres'))
