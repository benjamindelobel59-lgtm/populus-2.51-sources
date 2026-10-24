# PADGREF 22/10 : liste les classes (paquet.Classe) de tous les blocs ABC d'un SWF (DofusInvoker, modules d'interface).
# Lecture minimale du format ABC (pool de constantes + instances). Utilisation : py -I classes_abc.py <fichier.swf> [...] > liste.txt
import os, sys
import zlib, struct, lzma


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


class L:
    def __init__(s, d, p=0): s.d, s.p = d, p
    def u8(s): v = s.d[s.p]; s.p += 1; return v
    def u16(s): v = s.d[s.p] | s.d[s.p + 1] << 8; s.p += 2; return v
    def u30(s):
        r, sh = 0, 0
        while True:
            b = s.u8(); r |= (b & 0x7F) << sh; sh += 7
            if not b & 0x80: return r
    def d64(s): s.p += 8


def classes(abc):
    r = L(abc)
    mineur = r.u16(); majeur = r.u16()
    n = r.u30(); [r.u30() for _ in range(max(0, n - 1))]
    n = r.u30(); [r.u30() for _ in range(max(0, n - 1))]
    n = r.u30(); [r.d64() for _ in range(max(0, n - 1))]
    if majeur >= 47:                     # ABC 47.16 (AIR récent) : pool de « float » après les doubles
        n = r.u30(); r.p += 4 * max(0, n - 1)
    n = r.u30(); strings = ['']
    for _ in range(max(0, n - 1)):
        ln = r.u30(); strings.append(abc[r.p:r.p + ln].decode('utf-8', 'replace')); r.p += ln
    n = r.u30(); ns = [None]
    for _ in range(max(0, n - 1)):
        r.u8(); ns.append(strings[r.u30()])
    n = r.u30(); nss = [None]
    for _ in range(max(0, n - 1)):
        c = r.u30(); nss.append([r.u30() for _ in range(c)])
    n = r.u30(); mn = [None]
    for _ in range(max(0, n - 1)):
        k = r.u8()
        if k in (0x07, 0x0D): a = r.u30(); b = r.u30(); mn.append((ns[a] if a < len(ns) else '', strings[b]))
        elif k in (0x0F, 0x10): mn.append(('', strings[r.u30()]))
        elif k in (0x11, 0x12): mn.append(('', ''))
        elif k in (0x09, 0x0E): b = r.u30(); r.u30(); mn.append(('', strings[b]))
        elif k in (0x1B, 0x1C): r.u30(); mn.append(('', ''))
        elif k == 0x1D: r.u30(); c = r.u30(); [r.u30() for _ in range(c)]; mn.append(('', ''))
        else: raise ValueError(k)
    # méthodes
    n = r.u30()
    for _ in range(n):
        pc = r.u30(); r.u30(); [r.u30() for _ in range(pc)]; r.u30(); fl = r.u8()
        if fl & 0x08:
            oc = r.u30()
            for _ in range(oc): r.u30(); r.u8()
        if fl & 0x80: [r.u30() for _ in range(pc)]
    # métadonnées
    n = r.u30()
    for _ in range(n):
        r.u30(); c = r.u30(); [r.u30() for _ in range(2 * c)]
    n = r.u30()
    res = []
    for _ in range(n):
        name = r.u30(); r.u30(); fl = r.u8()
        if fl & 0x08: r.u30()
        ic = r.u30(); [r.u30() for _ in range(ic)]
        r.u30()
        tc = r.u30()
        for _ in range(tc):
            r.u30(); k = r.u8(); kind = k & 0x0F
            if kind in (0, 6):
                r.u30(); r.u30(); vi = r.u30()
                if vi: r.u8()
            else:
                r.u30(); r.u30()
            if k & 0x40:
                mc = r.u30(); [r.u30() for _ in range(mc)]
        p, nm = mn[name]
        res.append(f'{p}.{nm}' if p else nm)
    return res


for f in sys.argv[1:]:
    for code, data in lire_swf(f):
        if code == 82:          # DoABC2 : flags + nom + abc
            i = data.index(b'\0', 4)
            for c in classes(data[i + 1:]):
                print(c)
        elif code == 72:
            for c in classes(data):
                print(c)
