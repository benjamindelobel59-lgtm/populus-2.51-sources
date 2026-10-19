# MYSTIC 19/10 : version « vectorielle » (.swf) des icônes infinies : un SWF minimal (scène 52×52 px comme les icônes du client)
# qui affiche le PNG (DefineBitsJPEG2 avec données PNG + forme à remplissage bitmap). Sortie : icones/<id>.swf
import os, struct, zlib
ICI = os.path.dirname(os.path.abspath(__file__))

class Bits:
    def __init__(s): s.b = []
    def u(s, v, n): s.b += [(v >> (n - 1 - i)) & 1 for i in range(n)]
    def s_(s, v, n): s.u(v & ((1 << n) - 1), n)
    def octets(s):
        b = s.b + [0] * (-len(s.b) % 8)
        return bytes(int(''.join(map(str, b[i:i + 8])), 2) for i in range(0, len(b), 8))

def nbits(*vals):
    return max(max(abs(v) for v in vals).bit_length() + 1, 1)

def rect(x0, x1, y0, y1):
    n = nbits(x0, x1, y0, y1); b = Bits(); b.u(n, 5)
    for v in (x0, x1, y0, y1): b.s_(v, n)
    return b.octets()

def tag(t, data):
    if len(data) < 63 and t not in (21, 2):
        return struct.pack('<H', (t << 6) | len(data)) + data
    return struct.pack('<HI', (t << 6) | 63, len(data)) + data

def swf(png, px):
    T = 1040                                    # 52 px en twips
    echelle = int(T / px * 65536)               # 1 pixel du bitmap -> T/px twips (16.16)
    forme = Bits()
    # DefineShape : id, bornes, styles de remplissage (1 : bitmap découpé 0x41), styles de ligne (0), enregistrements
    entete = struct.pack('<H', 2) + rect(0, T, 0, T)
    fill = bytes([1, 0x41]) + struct.pack('<H', 1)
    m = Bits(); m.u(1, 1); n = nbits(echelle); m.u(n, 5); m.s_(echelle, n); m.s_(echelle, n); m.u(0, 1); m.u(0, 1)
    fill += m.octets() + bytes([0])             # matrice puis 0 style de ligne
    r = Bits(); r.u(1, 4); r.u(0, 4)            # NumFillBits=1, NumLineBits=0
    # StyleChange : moveTo(0,0) + fillStyle1=1
    r.u(0, 1); r.u(0, 1); r.u(0, 1); r.u(1, 1); r.u(0, 1); r.u(1, 1)
    n = nbits(0, 0); r.u(n, 5); r.s_(0, n); r.s_(0, n); r.u(1, 1)
    for dx, dy in ((T, 0), (0, T), (-T, 0), (0, -T)):   # 4 droites
        r.u(1, 1); r.u(1, 1); n = nbits(dx, dy); r.u(n - 2, 4)
        if dx and dy: r.u(1, 1); r.s_(dx, n); r.s_(dy, n)
        else:
            r.u(0, 1); r.u(1 if dy else 0, 1); r.s_(dy if dy else dx, n)
    r.u(0, 6)                                   # fin
    shape = entete + fill + r.octets()
    corps = (rect(0, T, 0, T) + struct.pack('<HH', 24 << 8, 1)
             + tag(69, struct.pack('<I', 0x08))
             + tag(21, struct.pack('<H', 1) + png)
             + tag(2, shape)
             + tag(26, bytes([0x02]) + struct.pack('<HH', 1, 2))
             + tag(1, b'') + tag(0, b''))
    return b'FWS' + bytes([9]) + struct.pack('<I', 8 + len(corps)) + corps

for f in sorted(os.listdir(os.path.join(ICI, 'icones'))):
    if f.endswith('.png'):
        png = open(os.path.join(ICI, 'icones', f), 'rb').read()
        open(os.path.join(ICI, 'icones', f[:-4] + '.swf'), 'wb').write(swf(png, 60))
        print(f[:-4] + '.swf')
