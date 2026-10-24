# PADGREF 23/10 (retour utilisateur : icônes de sorts / de buffs violettes en combat) : le client 2.51 prend les icônes de sorts dans
# gfx/spells/all.swf (symbole « sort_<iconId> », une petite classe AS3 par symbole) ; les sorts importés du 2.74 ont des icônes
# absentes de ce fichier (le 2.74 les range en PNG dans spells0*.d2p). On AJOUTE au swf, pour chaque icône manquante utilisée par
# nos sorts (Eliocalypse + autres importés) : image (DefineBitsLossless2, 44 x 44 depuis le coin haut-gauche, comme les icônes existantes), forme
# (DefineShape remplie par l'image), sprite, classe (copie d'une classe « sort_… » existante, nom changé) et entrée SymbolClass.
# Icônes existantes non touchées. Sortie : client/content/gfx/spells/all.swf.
import glob, io, json, os, struct, sys, zlib
from PIL import Image
sys.stdout.reconfigure(encoding='utf-8')
ICI = os.path.dirname(os.path.abspath(__file__))
CB = os.path.dirname(ICI)
R = os.path.abspath(os.path.join(CB, '..', '..', '..'))
sys.path.insert(0, CB)
import maps_elements as me
sys.path.insert(0, os.path.join(R, '_OUTILS', 'serveur', 'donjons'))
import d2o
SRC = os.path.join(R, 'SAVE', '3_CLIENT_64bits', '2026-10-23_icones-sorts-avant', 'all.swf')   # all.swf d'origine (avant ajout)
SORTIE = os.path.join(ICI, 'client', 'content', 'gfx', 'spells', 'all.swf')
TAILLE = 44


# ------------------------------------------------------------------ lecture du swf
brut = open(SRC, 'rb').read()
version = brut[3]
d = brut[:8] + zlib.decompress(brut[8:]) if brut[:3] == b'CWS' else brut
nb = d[8] >> 3
debut = 8 + (5 + nb * 4 + 7) // 8 + 4
entete_film = d[8:debut]
tags, p = [], debut
while p < len(d):
    h = struct.unpack_from('<H', d, p)[0]; p += 2; t, l = h >> 6, h & 63
    if l == 63:
        l = struct.unpack_from('<I', d, p)[0]; p += 4
    tags.append([t, d[p:p + l]]); p += l
symboles = {}
for t, b in tags:
    if t == 76:
        q = 2
        for _ in range(struct.unpack_from('<H', b)[0]):
            cid = struct.unpack_from('<H', b, q)[0]; e = b.index(b'\0', q + 2); symboles[b[q + 2:e].decode()] = cid; q = e + 1
ids = [struct.unpack_from('<H', b)[0] for t, b in tags if t in (2, 6, 20, 21, 22, 32, 35, 36, 39, 83, 46, 84, 37, 34, 10, 48, 75, 11, 33)]
prochain = [max(ids) + 1]
modele = next(b for t, b in tags if t == 82 and b[4:b.index(b'\0', 4)].startswith(b'sort_'))
nom_modele = modele[4:modele.index(b'\0', 4)].decode()


# ------------------------------------------------------------------ icônes voulues
def icones_utilisees():
    res = set()
    for f in (os.path.join(ICI, 'client', 'data', 'common', 'Spells.d2o'),):
        for s in d2o.load(f)[1].values():
            if s.get('iconId', -1) and s['iconId'] > 0:
                res.add(s['iconId'])
    return res


voulues = sorted(i for i in icones_utilisees() if f'sort_{i}' not in symboles)
pngs = {}
for f in glob.glob(os.path.join(R, '_OUTILS', 'client', 'client_27x', 'content', 'gfx', 'spells', 'spells0*.d2p')):
    for n, b in me.read_d2p(f):
        pngs[n] = b


# ------------------------------------------------------------------ écriture de tags
class Bits:
    def __init__(self):
        self.v, self.n = 0, 0

    def u(self, val, nbits):
        self.v = (self.v << nbits) | (val & ((1 << nbits) - 1)); self.n += nbits

    def s(self, val, nbits):
        self.u(val & ((1 << nbits) - 1), nbits)

    def octets(self):
        pad = (-self.n) % 8
        return (self.v << pad).to_bytes((self.n + pad) // 8, 'big') if self.n else b''


def nbits_signe(*vals):
    return max(2, max(abs(v).bit_length() + 1 for v in vals))


def rect(xmin, xmax, ymin, ymax):
    n = nbits_signe(xmin, xmax, ymin, ymax); b = Bits(); b.u(n, 5)
    for v in (xmin, xmax, ymin, ymax):
        b.s(v, n)
    return b.octets()


def matrice(echelle=None, tx=0, ty=0):
    b = Bits()
    if echelle is None:
        b.u(0, 1)
    else:
        sc = int(echelle * 65536); n = nbits_signe(sc, sc); b.u(1, 1); b.u(n, 5); b.s(sc, n); b.s(sc, n)
    b.u(0, 1)
    n = nbits_signe(tx, ty) if (tx or ty) else 0
    b.u(n, 5)
    if n:
        b.s(tx, n); b.s(ty, n)
    return b.octets()


def tag(t, corps):
    if len(corps) >= 63 or t in (36, 20, 21, 35, 82):
        return struct.pack('<HI', (t << 6) | 63, len(corps)) + corps
    return struct.pack('<H', (t << 6) | len(corps)) + corps


def image(cid, png):
    im = Image.open(io.BytesIO(png)).convert('RGBA').resize((TAILLE, TAILLE), Image.LANCZOS)
    px = bytearray()
    for r, g, b_, a in im.getdata():               # ARGB prémultiplié
        px += bytes((a, r * a // 255, g * a // 255, b_ * a // 255))
    return tag(36, struct.pack('<HBHH', cid, 5, TAILLE, TAILLE) + zlib.compress(bytes(px), 9))


def forme(cid, bmp):
    demi = TAILLE * 20 // 2
    corps = struct.pack('<H', cid) + rect(0, 2 * demi, 0, 2 * demi)          # coin haut-gauche (comme les icônes d'origine)
    corps += bytes([1, 0x41]) + struct.pack('<H', bmp) + matrice(20.0, 0, 0) + bytes([0])   # 1 remplissage image, 0 ligne
    b = Bits(); b.u(1, 4); b.u(0, 4)                                                       # NumFillBits 1, NumLineBits 0
    n = 2
    b.u(0, 1); b.u(0, 1); b.u(0, 1); b.u(0, 1); b.u(1, 1); b.u(1, 1)                      # style : fillStyle0 + moveTo (0, 0)
    b.u(n, 5); b.s(0, n); b.s(0, n); b.u(1, 1)
    for dx, dy in ((2 * demi, 0), (0, 2 * demi), (-2 * demi, 0), (0, -2 * demi)):
        nn = nbits_signe(dx, dy)
        b.u(1, 1); b.u(1, 1); b.u(nn - 2, 4); b.u(1, 1); b.s(dx, nn); b.s(dy, nn)
    b.u(0, 6)
    return tag(2, corps + b.octets())


def sprite(cid, fid):
    place = bytes([0x06]) + struct.pack('<HH', 1, fid) + matrice()                     # PlaceObject2 : caractère + matrice
    return tag(39, struct.pack('<HH', cid, 1) + tag(26, place) + tag(1, b'') + tag(0, b''))


def u30(b, p):
    v = s = 0
    while True:
        x = b[p]; p += 1; v |= (x & 0x7f) << s; s += 7
        if not x & 0x80:
            return v, p


def e_u30(v):
    out = bytearray()
    while True:
        x = v & 0x7f; v >>= 7
        out.append(x | (0x80 if v else 0))
        if not v:
            return bytes(out)


def classe(nom):
    """DoABC du modèle, nom de classe remplacé dans la table des chaînes."""
    e = modele.index(b'\0', 4)
    abc = modele[e + 1:]
    p = 4
    n, p = u30(abc, p)
    for _ in range(max(0, n - 1)):
        _, p = u30(abc, p)
    n, p = u30(abc, p)
    for _ in range(max(0, n - 1)):
        _, p = u30(abc, p)
    n, p = u30(abc, p)
    p += 8 * max(0, n - 1)
    debut_chaines = p
    n, p = u30(abc, p)
    chaines = []
    for _ in range(max(0, n - 1)):
        l, p = u30(abc, p); chaines.append(abc[p:p + l]); p += l
    reste = abc[p:]
    chaines = [nom.encode() if c == nom_modele.encode() else c for c in chaines]
    nouv = abc[:debut_chaines] + e_u30(len(chaines) + 1) + b''.join(e_u30(len(c)) + c for c in chaines) + reste
    return tag(82, modele[:4] + nom.encode() + b'\0' + nouv)


ajouts, symb, absentes = [], [], []
for i in voulues:
    png = pngs.get(f'sort_{i}.png')
    if png is None:
        absentes.append(i); continue
    bmp, fid, sid = prochain[0], prochain[0] + 1, prochain[0] + 2
    prochain[0] += 3
    ajouts += [image(bmp, png), forme(fid, bmp), sprite(sid, fid), classe(f'sort_{i}')]
    symb.append((sid, f'sort_{i}'))

# ------------------------------------------------------------------ réassemblage (ajouts juste avant SymbolClass)
out = bytearray()
for t, b in tags:
    if t == 76:
        out += b''.join(ajouts)
        q = 2; n = struct.unpack_from('<H', b)[0]
        b = struct.pack('<H', n + len(symb)) + b[2:] + b''.join(struct.pack('<H', c) + s.encode() + b'\0' for c, s in symb)
    out += tag(t, b) if t not in (0,) else tag(0, b'')
corps = entete_film + bytes(out)
non_compresse = b'FWS' + bytes([version]) + struct.pack('<I', 8 + len(corps)) + corps
os.makedirs(os.path.dirname(SORTIE), exist_ok=True)
open(SORTIE, 'wb').write(b'CWS' + bytes([version]) + struct.pack('<I', 8 + len(corps)) + zlib.compress(corps, 9))
print('icônes manquantes :', len(voulues), '| ajoutées :', len(symb), '| sans PNG dans le 2.74 :', absentes, '| modèle de classe :', nom_modele)
