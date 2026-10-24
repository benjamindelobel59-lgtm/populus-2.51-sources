# PADGREF 22/10 (demande utilisateur) : Eliocalypse — import des CARTES et du DÉCOR du client 2.74 dans notre client 2.51.
#  - cartes (.dlm, version 11) : le 2.74 n'a plus les champs useLowPassFilter / useReverb (+ presetId) ; on réinsère 2 octets à 0
#    (dans la partie chiffrée), puis on les ajoute à la dernière archive de la chaîne maps1 (entrée « <id%10>/<id>.dlm ») ;
#  - décor : éléments graphiques absents de notre elements.ele (version 9, zlib ; enregistrements identiques au 2.74 version 32)
#    recopiés tels quels + liste des images JPG ; images (gfx/world png/jpg/swf) absentes ajoutées à la dernière archive gfx11.
# Entrée : _OUTILS/serveur/donjons/eliocalypse/inventaire.json. Sortie : client/content/... (fichiers complets à copier dans app/).
import glob, json, os, struct, sys, zlib
sys.stdout.reconfigure(encoding='utf-8')
ICI = os.path.dirname(os.path.abspath(__file__))
CB = os.path.dirname(ICI)
R = os.path.abspath(os.path.join(CB, '..', '..', '..'))
sys.path.insert(0, CB); sys.path.insert(0, ICI)
import maps_elements as me
APP = os.path.join(R, 'CLIENT_64bits', 'app')
N = os.path.join(R, '_OUTILS', 'client', 'client_27x')
SORTIE = os.path.join(ICI, 'client')
inv = json.load(open(os.path.join(R, '_OUTILS', 'serveur', 'donjons', 'eliocalypse', 'inventaire.json'), encoding='utf-8'))
CARTES = set(inv['cartes'])
KEY = me.KEY


def lire_d2p(path):
    d = open(path, 'rb').read()
    data_off, _, idx_off, idx_cnt, prop_off, prop_cnt = struct.unpack_from('>6I', d, len(d) - 24)
    ent, p = [], idx_off
    for _ in range(idx_cnt):
        n = struct.unpack_from('>H', d, p)[0]; p += 2
        nom = d[p:p + n].decode('utf-8'); p += n
        off, ln = struct.unpack_from('>ii', d, p); p += 8
        ent.append([nom, d[data_off + off:data_off + off + ln]])
    return d[:data_off], ent, d[prop_off:len(d) - 24], prop_cnt


def ecrire_d2p(path, head, ent, props, prop_cnt):
    os.makedirs(os.path.dirname(path), exist_ok=True)
    blob, index = bytearray(), bytearray()
    for nom, c in ent:
        nb = nom.encode('utf-8')
        index += struct.pack('>H', len(nb)) + nb + struct.pack('>ii', len(blob), len(c)); blob += c
    off = len(head)
    pied = struct.pack('>6I', off, len(blob), off + len(blob), len(ent), off + len(blob) + len(index), prop_cnt)
    open(path, 'wb').write(bytes(head) + bytes(blob) + bytes(index) + bytes(props) + pied)


def xor(b):
    b = bytearray(b)
    for k in range(len(b)):
        b[k] ^= KEY[k % len(KEY)]
    return bytes(b)


def convertir_carte(raw):
    """dlm 2.74 -> dlm 2.51 : réinsère useLowPassFilter = 0 et useReverb = 0 après tacticalModeTemplateId."""
    assert raw[0] == 77 and raw[1] == 11
    tete = raw[:6]
    enc = raw[6] != 0
    n = struct.unpack_from('>i', raw, 8)[0]
    corps = raw[12:12 + n]
    if enc:
        corps = xor(corps)
    # relativeId I, mapType b, subareaId i, 4 voisins i, shadow I, couleurs i + I, zoom H h h, tactical i
    p = 4 + 1 + 4 + 16 + 4 + 8 + 6 + 4
    corps = corps[:p] + b'\x00\x00' + corps[p:]
    if enc:
        corps = xor(corps)
    return tete + raw[6:8] + struct.pack('>i', len(corps)) + corps + raw[12 + n:]


# ------------------------------------------------------------------ cartes
cartes_274 = {}
for f in glob.glob(os.path.join(N, 'content', 'maps', '*.d2p')):
    for nom, blob in me.read_d2p(f):
        if nom.endswith('.dlm') and int(os.path.basename(nom)[:-4]) in CARTES:
            raw = zlib.decompress(blob)
            cartes_274[int(os.path.basename(nom)[:-4])] = convertir_carte(raw)
for mid, raw in cartes_274.items():           # contrôle : la carte convertie se lit avec le lecteur 2.51
    me.parse_map(raw)
assert len(cartes_274) == len(CARTES), (len(cartes_274), len(CARTES))
# PADGREF 22/10 : fin de la chaîne lue par le client (maps0 -> … -> maps11_2) ; maps1_8.d2p est HORS chaîne (« impossible de
# charger les données de la carte » au VPS)
from cartes_decor_lib import fin_de_chaine
dernier_maps, _ = fin_de_chaine(os.path.join(APP, 'content', 'maps'), 'maps0.d2p')
head, ent, props, pc = lire_d2p(dernier_maps)
noms = {f'{m % 10}/{m}.dlm' for m in cartes_274}
ent = [e for e in ent if e[0] not in noms] + [[f'{m % 10}/{m}.dlm', zlib.compress(raw)] for m, raw in sorted(cartes_274.items())]
ecrire_d2p(os.path.join(SORTIE, 'content', 'maps', os.path.basename(dernier_maps)), head, ent, props, pc)

# ------------------------------------------------------------------ éléments graphiques


def index_ele(d):
    r = me.R(d)
    assert r.B() == 69
    v = r.B(); n = r.I()
    idx = {}
    for _ in range(n):
        taille = r.H()
        eid = r.i()
        idx[eid] = d[r.p - 4 - 2:r.p - 4 + taille]          # enregistrement complet (taille + id + données)
        r.p += taille - 4
    nj = r.i()
    jpg = [r.i() for _ in range(nj)]
    return v, idx, jpg


nos = zlib.decompress(open(os.path.join(APP, 'content', 'maps', 'elements.ele'), 'rb').read())
v51, idx51, jpg51 = index_ele(nos)
v74, idx74, jpg74 = index_ele(open(os.path.join(N, 'content', 'maps', 'elements.ele'), 'rb').read())
decors = json.load(open(os.path.join(R, '_OUTILS', 'serveur', 'donjons', 'eliocalypse', 'decors.json')))
manquants = sorted(set(decors['elements']) - set(idx51))
assert all(e in idx74 for e in manquants), 'élément absent du 2.74'
images, looks = set(decors['fonds']), set()
for e in manquants:
    rec = idx74[e]
    t = rec[6]
    if t in (0, 1, 2, 5):
        images.add(struct.unpack_from('>i', rec, 7)[0])
    elif t == 3:
        ln = struct.unpack_from('>I', rec, 7)[0]
        looks.add(rec[11:11 + ln].decode('utf-8'))
for e in decors['elements']:                    # images des éléments déjà connus : déjà chez nous
    pass
corps = bytearray(nos[:6])
struct.pack_into('>I', corps, 2, len(idx51) + len(manquants))
for e, rec in idx51.items():
    corps += rec
for e in manquants:
    corps += idx74[e]
jpg = sorted(set(jpg51) | {g for g in jpg74 if g in images})
corps += struct.pack('>i', len(jpg)) + b''.join(struct.pack('>i', g) for g in jpg)
os.makedirs(os.path.join(SORTIE, 'content', 'maps'), exist_ok=True)
open(os.path.join(SORTIE, 'content', 'maps', 'elements.ele'), 'wb').write(zlib.compress(bytes(corps), 9))
v, idx_relu, _ = index_ele(bytes(corps))
assert len(idx_relu) == len(idx51) + len(manquants)

# ------------------------------------------------------------------ images gfx/world
deja = set()
for f in glob.glob(os.path.join(APP, 'content', 'gfx', 'world', '*.d2p')):
    for nom, _ in me.read_d2p(f):
        deja.add(nom)
voulues = {}
for f in glob.glob(os.path.join(N, 'content', 'gfx', 'world', '*.d2p')):
    for nom, b in me.read_d2p(f):
        base = os.path.basename(nom).split('.')[0]
        if base.isdigit() and int(base) in images and nom not in deja:
            voulues[nom] = b
dernier_gfx = sorted(glob.glob(os.path.join(APP, 'content', 'gfx', 'world', 'gfx11_*.d2p')), key=lambda x: int(x.rsplit('_', 1)[1][:-4]))[-1]
head, ent, props, pc = lire_d2p(dernier_gfx)
ent = [e for e in ent if e[0] not in voulues] + [[k, b] for k, b in sorted(voulues.items())]
ecrire_d2p(os.path.join(SORTIE, 'content', 'gfx', 'world', os.path.basename(dernier_gfx)), head, ent, props, pc)
manquantes = sorted(g for g in images if not any(n.split('/')[-1].split('.')[0] == str(g) for n in list(voulues) + list(deja)))
json.dump({'elementsAjoutes': manquants, 'images': sorted(images), 'imagesAjoutees': sorted(voulues), 'looks': sorted(looks),
           'imagesIntrouvables': manquantes}, open(os.path.join(ICI, 'decor_importe.json'), 'w'), indent=0)
print(f"cartes : {len(cartes_274)} -> {os.path.basename(dernier_maps)} | éléments ajoutés : {len(manquants)} (ele {len(idx51)} -> {len(idx_relu)}) | "
      f"images ajoutées : {len(voulues)} ({sum(len(b) for b in voulues.values()) // 1048576} Mo) -> {os.path.basename(dernier_gfx)} | "
      f"apparences (éléments animés) : {len(looks)} | images introuvables : {len(manquantes)}")
