# PADGREF 22/10 : décors des cartes de l'Eliocalypse — éléments graphiques (elements.ele) et images (gfx/world) absents de notre 2.51.
import json, os, sys, glob, zlib, struct, collections
sys.stdout.reconfigure(encoding='utf-8')
ICI = os.path.dirname(os.path.abspath(__file__))
R = os.path.abspath(os.path.join(ICI, '..', '..', '..', '..'))
sys.path.insert(0, os.path.join(R, '_OUTILS', 'client', 'client-build'))
import maps_elements as me
N = os.path.join(R, '_OUTILS', 'client', 'client_27x')
A = os.path.join(R, 'CLIENT_64bits', 'app')
inv = json.load(open(os.path.join(ICI, 'inventaire.json'), encoding='utf-8'))
cartes = set(inv['cartes'])


def tous_gfx(raw):
    """(élément graphique, cellule) de toutes les couches (même lecture que gfx_interieurs.py)."""
    r = me.R(raw)
    r.b(); version = r.b(); r.I()
    if version >= 7:
        enc = r.b() != 0; r.b(); n = r.i()
        if enc:
            data = bytearray(r.d[r.p:r.p + n])
            for k in range(len(data)):
                data[k] ^= me.KEY[k % len(me.KEY)]
            r = me.R(bytes(data))
    r.I(); r.b(); r.i(); r.i(); r.i(); r.i(); r.i(); r.I()
    if version >= 9: r.i(); r.I()
    elif version >= 3: r.skip(3)
    if version >= 4: r.H(); r.h(); r.h()
    if version > 10: r.i()
    if not SANS_SON:                      # 2.51 : useLowPassFilter + useReverb (+ presetId) ; retirés en 2.74
        r.b()
        if r.b() == 1: r.i()
    fonds = []
    for _ in range(2):
        for _ in range(r.B()):
            fonds.append(r.i()); r.skip(2 * 5 + 3 + 1)
    r.i(); r.i()
    out = []
    for _ in range(r.B()):
        r.b() if version >= 9 else r.i()
        for _ in range(r.h()):
            r.h()
            for _ in range(r.h()):
                t = r.b()
                if t == 2:
                    out.append(r.I()); r.skip(6); r.skip(2 if version <= 4 else 4); r.b(); r.I()
                elif t == 33:
                    r.skip(18)
    return version, out, fonds


SANS_SON = True
vers = collections.Counter()
elems, fonds = set(), set()
for f in glob.glob(os.path.join(N, 'content', 'maps', '*.d2p')):
    for name, blob in me.read_d2p(f):
        if name.endswith('.dlm') and int(os.path.basename(name)[:-4]) in cartes:
            v, e, fo = tous_gfx(zlib.decompress(blob))
            vers[v] += 1; elems |= set(e); fonds |= set(fo)
print('versions des cartes 2.74 :', dict(vers), '| éléments graphiques utilisés :', len(elems), '| fonds :', len(fonds))
SANS_SON = False
elems251, fonds251 = set(), set()
for f in glob.glob(os.path.join(A, 'content', 'maps', '*.d2p')):
    for name, blob in me.read_d2p(f):
        if name.endswith('.dlm'):
            try:
                v, e, fo = tous_gfx(zlib.decompress(blob)); elems251 |= set(e); fonds251 |= set(fo)
            except Exception:
                pass
print('éléments absents de toutes nos cartes 2.51 :', len(elems - elems251), '/', len(elems), '| fonds absents :', len(fonds - fonds251), '/', len(fonds))


def ele_ids(path):
    d = zlib.decompress(open(path, 'rb').read()) if open(path, 'rb').read(2) == b'\x78\x9c' else open(path, 'rb').read()
    return d


for nom in ('elements.ele',):
    for base in (A, N):
        p = os.path.join(base, 'content', 'maps', nom)
        raw = open(p, 'rb').read()
        print(base[-25:], nom, len(raw), 'octets, début', raw[:8].hex())
gfx = {}
for base, cle in ((A, '251'), (N, '274')):
    s = set()
    for f in glob.glob(os.path.join(base, 'content', 'gfx', 'world', '*.d2p')):
        for n, _ in me.read_d2p(f):
            s.add(n)
    gfx[cle] = s
    print(cle, 'images gfx/world :', len(s), sorted(s)[:3])
json.dump({'elements': sorted(elems), 'fonds': sorted(fonds)}, open(os.path.join(ICI, 'decors.json'), 'w'))
