# MYSTIC 19/10 : lecture / écriture complète d'une carte du client (.dlm décompressé) : en-tête, calques (cellules + éléments bruts),
# données de cases. Permet d'ajouter des éléments graphiques (ateliers) puis de réécrire la carte (rechiffrée).
import os, struct, sys, zlib
CB = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
sys.path.insert(0, CB)
import maps_elements as me


class Carte:
    def __init__(self, raw):
        r = me.R(raw)
        assert r.b() == 77
        self.version = v = r.b()
        self.map_id = r.I()
        self.enc = False
        if v >= 7:
            self.enc = r.b() != 0; r.b(); n = r.i()
            self.head = bytearray(raw[:r.p])
            data = bytearray(raw[r.p:r.p + n])
            if self.enc:
                for k in range(len(data)):
                    data[k] ^= me.KEY[k % len(me.KEY)]
        else:
            self.head = bytearray(raw[:r.p]); data = bytearray(raw[r.p:])
        d = me.R(bytes(data))
        d.I(); d.b(); d.i(); d.i(); d.i(); d.i(); d.i(); d.I()
        if v >= 9: d.i(); d.I()
        elif v >= 3: d.skip(3)
        if v >= 4: d.H(); d.h(); d.h()
        if v > 10: d.i()
        d.b()
        if d.b() == 1: d.i()
        for _ in range(2):
            for _ in range(d.B()):
                d.skip(4 + 2 * 5 + 3 + 1)
        d.i(); d.i()
        self.pre = bytes(data[:d.p])
        self.layers = []                       # [layer_id, [[cell, [raw_element, ...]], ...]]
        self.elsize = 1 + 4 + 6 + (2 if v <= 4 else 4) + 1 + 4
        for _ in range(d.B()):
            lid = d.b() if v >= 9 else d.i()
            cells = []
            for _ in range(d.h()):
                c = d.h(); els = []
                for _ in range(d.h()):
                    t = data[d.p]
                    if t == 2:
                        els.append(bytes(data[d.p:d.p + self.elsize])); d.skip(self.elsize)
                    elif t == 33:
                        els.append(bytes(data[d.p:d.p + 19])); d.skip(19)
                    else:
                        raise ValueError('element %d' % t)
                cells.append([c, els])
            self.layers.append([lid, cells])
        self.post = bytes(data[d.p:])           # données de cases (inchangées ici)

    def elements(self):
        """(indice calque, cellule, brut, identifiant, gfx)"""
        for li, (lid, cells) in enumerate(self.layers):
            for c, els in cells:
                for e in els:
                    if e[0] == 2:
                        yield li, c, e, struct.unpack_from('>I', e, len(e) - 4)[0], struct.unpack_from('>I', e, 1)[0]

    def ajouter(self, li, cell, brut, ident):
        e = bytearray(brut); struct.pack_into('>I', e, len(e) - 4, ident)
        cells = self.layers[li][1]
        for c in cells:
            if c[0] == cell:
                c[1].append(bytes(e)); return
        cells.append([cell, [bytes(e)]]); cells.sort(key=lambda x: x[0])

    def octets(self, map_id=None):
        v = self.version
        out = bytearray(self.pre)
        out += bytes([len(self.layers)])
        for lid, cells in self.layers:
            out += struct.pack('>b', lid) if v >= 9 else struct.pack('>i', lid)
            out += struct.pack('>h', len(cells))
            for c, els in cells:
                out += struct.pack('>hh', c, len(els))
                for e in els: out += e
        out += self.post
        head = bytearray(self.head)
        if map_id is not None:
            struct.pack_into('>I', head, 2, map_id)
        if v >= 7:
            struct.pack_into('>i', head, len(head) - 4, len(out))
            if self.enc:
                for k in range(len(out)):
                    out[k] ^= me.KEY[k % len(me.KEY)]
        return bytes(head) + bytes(out)


def index_cartes(dossier):
    """map_id -> (fichier d2p, nom d'entrée)"""
    res = {}
    for f in sorted(os.listdir(dossier)):
        if f.endswith('.d2p'):
            for name, _ in me.read_d2p(os.path.join(dossier, f)):
                if name.endswith('.dlm'):
                    res[int(os.path.basename(name)[:-4])] = (f, name)
    return res


def lire(dossier, idx, map_id):
    f, name = idx[map_id]
    for n, blob in me.read_d2p(os.path.join(dossier, f)):
        if n == name:
            return zlib.decompress(blob)
