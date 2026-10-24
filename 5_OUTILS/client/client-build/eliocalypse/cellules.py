# PADGREF 23/10 : lecture de l'en-tête et des 560 cellules d'une carte .dlm 2.51 (déchiffrée / décompressée) — pour le serveur
# (world_maps : voisins, CompressedCells, cases de placement bleues / rouges).
import struct
import maps_elements as me


def lire(raw):
    r = me.R(raw)
    assert r.b() == 77
    version = r.b(); map_id = r.I()
    if version >= 7:
        enc = r.b() != 0; r.b(); n = r.i()
        if enc:
            data = bytearray(r.d[r.p:r.p + n])
            for k in range(len(data)):
                data[k] ^= me.KEY[k % len(me.KEY)]
            r = me.R(bytes(data))
    h = {'version': version, 'id': map_id}
    h['relativeId'] = r.I(); h['mapType'] = r.b(); h['subareaId'] = r.i()
    h['top'] = r.i(); h['bottom'] = r.i(); h['left'] = r.i(); h['right'] = r.i()
    h['shadow'] = r.I()
    if version >= 9: r.i(); r.I()
    elif version >= 3: r.skip(3)
    if version >= 4: r.H(); r.h(); r.h()
    if version > 10: h['tactical'] = r.i()
    h['lowpass'] = r.b()
    h['reverb'] = r.b()
    h['preset'] = r.i() if h['reverb'] == 1 else -1
    for _ in range(2):
        for _ in range(r.B()):
            r.skip(4 + 2 * 5 + 3 + 1)
    r.i(); r.i()
    for _ in range(r.B()):
        r.b() if version >= 9 else r.i()
        for _ in range(r.h()):
            r.h()
            for _ in range(r.h()):
                t = r.b()
                if t == 2:
                    r.skip(4 + 6 + (2 if version <= 4 else 4) + 1 + 4)
                elif t == 33:
                    r.skip(18)
    cells = []
    for cid in range(560):
        floor = r.b()
        if floor == -128:
            cells.append(None); continue
        if version >= 9:
            f = r.h()
            mov = (f & 1) == 0; nwf = (f & 2) != 0; farm = (f & 0x80) != 0; haven = version >= 10 and (f & 0x100) != 0
        else:
            f = r.B()
            mov = (f & 1) == 1; nwf = (f & 4) != 0; farm = (f & 0x20) != 0; haven = False
        speed = r.b(); mcd = r.B()
        mz = r.B() if version > 5 else 0
        lz = 0
        if version > 10 and ((mov and not farm) or (mov and not nwf and not farm and not haven)):
            lz = r.B()
        if 7 < version < 9:
            r.b()
        cells.append({'floor': floor * 10, 'f': f & 0xFFFF, 'speed': speed, 'mcd': mcd, 'moveZone': mz, 'linkedZone': lz})
    return h, cells


def compresser(cells, version=11):
    """CompressedCells du serveur (Database/World/Cell.cs : 11 octets par case, gzip)."""
    import gzip
    b = bytearray()
    for i, c in enumerate(cells):
        c = c or {'floor': 0, 'f': 1, 'speed': 0, 'mcd': 0, 'moveZone': 0}
        b += struct.pack('>hhBBbI', i, c['floor'], c['f'] & 0xFF, c['mcd'], c['speed'], c['moveZone'])
    return gzip.compress(bytes(b))
