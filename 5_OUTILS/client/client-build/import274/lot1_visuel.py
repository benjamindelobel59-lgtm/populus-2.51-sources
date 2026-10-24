# PADGREF 23/10 (import 2.74, lot 1 validé par l'utilisateur) : VISUEL du client 2.74 absent du nôtre :
#   icônes d'objets (gfx/items bitmap + vector), animations (sprites/bones) et skins (sprites/skins), images du bestiaire
#   (gfx/monsters), images des cartes du monde (gfx/maps).
# Méthode : rien de nos fichiers n'est remplacé (même nom = on garde le nôtre). Le nouveau contenu va dans une NOUVELLE archive
# ajoutée en bout de chaîne : la dernière archive actuelle (sa version la plus récente, déjà modifiée par les paquets
# Eliocalypse) reçoit la propriété « link » vers la nouvelle. Les .swl 2.74 (version 11) sont convertis au format 2.51.
# Sortie : import274/client/content/... (fichiers à copier dans app/).
import glob, json, os, struct, sys
sys.stdout.reconfigure(encoding='utf-8')
ICI = os.path.dirname(os.path.abspath(__file__))
CB = os.path.dirname(ICI)
R = os.path.abspath(os.path.join(CB, '..', '..', '..'))
sys.path.insert(0, CB); sys.path.insert(0, os.path.join(CB, 'eliocalypse'))
import maps_elements as me
from cartes_decor_lib import lire_d2p, ecrire_d2p, fin_de_chaine, lien


def swl_251(b):
    """SWL 2.74 (version 11) -> 2.51 (version 0) : 4 octets de plus avant le SWF (même fonction que eliocalypse/animations.py)."""
    if b[0] != 76 or b[1] <= 10:
        return b
    p = 10
    for _ in range(struct.unpack_from('>i', b, 6)[0]):
        p += 2 + struct.unpack_from('>H', b, p)[0]
    return b[:1] + bytes(1) + b[2:p] + b[p + 4:]

A = os.path.join(R, 'CLIENT_64bits', 'app', 'content')
N = os.path.join(R, '_OUTILS', 'client', 'client_27x', 'content')
ELIO = os.path.join(CB, 'eliocalypse', 'client', 'content')       # dernières versions (paquets eliocalypse-1 à 5)
SORTIE = os.path.join(ICI, 'client', 'content')
TAILLE_MAX = 50 * 1048576

# (dossier, archive de départ (config.xml), archives 2.74 sources (motif), nouvelle archive)
CHAINES = [
    ('gfx/items', 'bitmap0.d2p', 'bitmap*.d2p', 'bitmap1_2.d2p'),
    ('gfx/items', 'vector0.d2p', 'vector*.d2p', 'vector1_2.d2p'),
    ('gfx/sprites', 'bones0.d2p', 'bones*.d2p', 'bones11_7.d2p'),
    ('gfx/sprites', 'skins.d2p', 'skins*.d2p', 'skins_3.d2p'),
    ('gfx/monsters', 'monsters0.d2p', 'monsters0*.d2p', 'monsters0_2.d2p'),
    ('gfx/maps', 'worldmap0.d2p', 'worldmap0*.d2p', 'worldmap0_7.d2p'),
]


def version(dossier, nom):
    """Version la plus récente d'une archive : sortie Eliocalypse si elle existe, sinon le client."""
    e = os.path.join(ELIO, dossier, nom)
    return e if os.path.exists(e) else os.path.join(A, dossier, nom)


bilan = {}
for dossier, depart, motif, nouvelle in CHAINES:
    # chaîne actuelle (avec les versions les plus récentes)
    chaine, f = [], depart
    while f and f not in chaine:
        chaine.append(f); f = lien(version(dossier, f))
    deja = {n for a in chaine for n, _ in me.read_d2p(version(dossier, a))}
    assert nouvelle not in chaine and not os.path.exists(os.path.join(A, dossier, nouvelle)), nouvelle
    ajout = {}
    for src in sorted(glob.glob(os.path.join(N, dossier, motif))):
        for n, b in me.read_d2p(src):
            if n not in deja and n not in ajout:
                ajout[n] = swl_251(b) if n.endswith('.swl') else b
    if not ajout:
        bilan[nouvelle] = 0; continue
    # nouvelles archives de ~50 Mo au plus, chaînées (nom de base, puis _a, _b… : bones11_7.d2p, bones11_7a.d2p…)
    head, _, _, _ = lire_d2p(version(dossier, chaine[-1]))
    parts, cur, taille = [], [], 0
    for n, b in sorted(ajout.items()):
        if cur and taille + len(b) > TAILLE_MAX:
            parts.append(cur); cur, taille = [], 0
        cur.append((n, b)); taille += len(b)
    parts.append(cur)
    noms = [nouvelle] + [nouvelle[:-4] + chr(97 + i) + '.d2p' for i in range(len(parts) - 1)]
    for i, (nom, part) in enumerate(zip(noms, parts)):
        assert not os.path.exists(os.path.join(A, dossier, nom)), nom
        props, pc = b'', 0
        if i + 1 < len(noms):
            k, v = b'link', noms[i + 1].encode()
            props, pc = struct.pack('>H', len(k)) + k + struct.pack('>H', len(v)) + v, 1
        ecrire_d2p(os.path.join(SORTIE, dossier, nom), head, part, props, pc)
    # dernière archive actuelle : + link vers la première nouvelle
    head, ent, props, pc = lire_d2p(version(dossier, chaine[-1]))
    k, v = b'link', nouvelle.encode()
    props = bytes(props) + struct.pack('>H', len(k)) + k + struct.pack('>H', len(v)) + v
    ecrire_d2p(os.path.join(SORTIE, dossier, chaine[-1]), head, ent, props, pc + 1)
    assert lien(os.path.join(SORTIE, dossier, chaine[-1])) == nouvelle
    nouvelle = ', '.join(noms)
    bilan[nouvelle] = {'fichiers': len(ajout), 'Mo': sum(len(b) for b in ajout.values()) // 1048576, 'lien depuis': chaine[-1]}
    print(dossier, nouvelle, bilan[nouvelle], flush=True)
json.dump(bilan, open(os.path.join(ICI, 'lot1_bilan.json'), 'w', encoding='utf-8'), ensure_ascii=False, indent=1)
