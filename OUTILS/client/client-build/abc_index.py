# Trouve l'index du corps de méthode (method body) d'une fonction AS3 dans un SWF, pour « ffdec -replace ... file.pcode <index> »
# Utilisation : py -I abc_index.py <fichier.swf> <morceau du nom, ex. UiModuleManager/private:parseNextXml>
import sys, zlib, struct, lzma

def lire_swf(p):
    d = open(p, "rb").read()
    sig = d[:3]
    if sig == b"CWS": d = d[:8] + zlib.decompress(d[8:])
    elif sig == b"ZWS": d = d[:8] + lzma.LZMADecompressor(format=lzma.FORMAT_RAW, filters=[{"id": lzma.FILTER_LZMA1, "dict_size": 1 << 24}]).decompress(d[17:])
    nbits = d[8] >> 3; pos = 8 + (5 + nbits*4 + 7)//8 + 4
    while pos < len(d):
        tc = struct.unpack_from("<H", d, pos)[0]; pos += 2; code, ln = tc >> 6, tc & 63
        if ln == 63: ln = struct.unpack_from("<I", d, pos)[0]; pos += 4
        yield code, d[pos:pos+ln]; pos += ln

class R:
    def __init__(s, b, p=0): s.b, s.p = b, p
    def u8(s): v = s.b[s.p]; s.p += 1; return v
    def u16(s): v = struct.unpack_from("<H", s.b, s.p)[0]; s.p += 2; return v
    def u30(s):
        v = 0; sh = 0
        while True:
            c = s.b[s.p]; s.p += 1; v |= (c & 127) << sh; sh += 7
            if not c & 128 or sh >= 35: return v
    def skip(s, n): s.p += n

def analyser(abc):
    r = R(abc); r.skip(4)
    n = r.u30(); [r.u30() for _ in range(max(0, n-1))]          # ints
    n = r.u30(); [r.u30() for _ in range(max(0, n-1))]          # uints
    n = r.u30(); r.skip(8*max(0, n-1))                          # doubles
    n = r.u30(); chaines = [""]
    for _ in range(max(0, n-1)): l = r.u30(); chaines.append(abc[r.p:r.p+l].decode("utf-8", "replace")); r.skip(l)
    n = r.u30(); [ (r.u8(), r.u30()) for _ in range(max(0, n-1))]   # namespaces
    n = r.u30()
    for _ in range(max(0, n-1)): c = r.u30(); [r.u30() for _ in range(c)]   # ns sets
    n = r.u30()
    for _ in range(max(0, n-1)):                                 # multinames
        k = r.u8()
        if k in (0x07, 0x0D): r.u30(); r.u30()
        elif k in (0x0F, 0x10): r.u30()
        elif k in (0x11, 0x12): pass
        elif k in (0x09, 0x0E): r.u30(); r.u30()
        elif k in (0x1B, 0x1C): r.u30()
        elif k == 0x1D: r.u30(); c = r.u30(); [r.u30() for _ in range(c)]
        else: raise ValueError("multiname inconnu %x" % k)
    nm = r.u30(); noms = []
    for _ in range(nm):                                          # method_info
        pc = r.u30(); r.u30(); [r.u30() for _ in range(pc)]; nom = r.u30(); fl = r.u8()
        if fl & 0x08:
            oc = r.u30()
            for _ in range(oc): r.u30(); r.u8()
        if fl & 0x80: [r.u30() for _ in range(pc)]
        noms.append(chaines[nom] if nom < len(chaines) else "")
    n = r.u30()
    for _ in range(n): r.u30(); c = r.u30(); [ (r.u30(), r.u30()) for _ in range(c)]   # metadata
    def traits():
        for _ in range(r.u30()):
            r.u30(); kind = r.u8(); t = kind & 15
            if t in (0, 6): r.u30(); r.u30(); vi = r.u30(); (r.u8() if vi else None)
            elif t in (1, 2, 3, 4, 5): r.u30(); r.u30()
            if kind & 0x40: [r.u30() for _ in range(r.u30())]
    nc = r.u30()
    for _ in range(nc):                                          # instances
        r.u30(); r.u30(); fl = r.u8()
        if fl & 0x08: r.u30()
        [r.u30() for _ in range(r.u30())]; r.u30(); traits()
    for _ in range(nc): r.u30(); traits()                        # classes
    for _ in range(r.u30()): r.u30(); traits()                   # scripts
    corps = []
    for i in range(r.u30()):                                     # method bodies
        m = r.u30(); r.u30(); r.u30(); r.u30(); r.u30(); cl = r.u30(); r.skip(cl)
        for _ in range(r.u30()): r.u30(); r.u30(); r.u30(); r.u30(); r.u30()
        traits(); corps.append(m)
    return noms, corps

swf, cherche = sys.argv[1], sys.argv[2]
for code, data in lire_swf(swf):
    if code not in (72, 82): continue
    abc = data[data.index(b"\0", 4) + 1:] if code == 82 else data
    try: noms, corps = analyser(abc)
    except Exception as e: print("ABC illisible :", e); continue
    for bi, mi in enumerate(corps):
        if cherche in noms[mi]: print("corps", bi, "méthode", mi, noms[mi])
