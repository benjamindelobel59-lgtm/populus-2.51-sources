# Extrait des fichiers de maps du client (content/maps/*.d2p) tous les elements graphiques "identifies"
# (= elements interactifs : portes, zaaps, enclos...), sous la forme : mapId <TAB> identifiant <TAB> elementId (image) <TAB> cellule
# Format d'apres le client decompile : com.ankamagames.atouin.data.map.Map/Layer/Cell/GraphicalElement.fromRaw
# Utilisation : py -I maps_elements.py <dossier content/maps> <fichier de sortie .tsv>
import os, struct, sys, zlib

KEY = b"649ae451ca33ec53bbcbcc33becf15f4"  # config.xml : maps.encryptionKey


class R:
    def __init__(s, d): s.d, s.p = d, 0
    def b(s): v = struct.unpack_from(">b", s.d, s.p)[0]; s.p += 1; return v
    def B(s): v = s.d[s.p]; s.p += 1; return v
    def h(s): v = struct.unpack_from(">h", s.d, s.p)[0]; s.p += 2; return v
    def H(s): v = struct.unpack_from(">H", s.d, s.p)[0]; s.p += 2; return v
    def i(s): v = struct.unpack_from(">i", s.d, s.p)[0]; s.p += 4; return v
    def I(s): v = struct.unpack_from(">I", s.d, s.p)[0]; s.p += 4; return v
    def skip(s, n): s.p += n


def parse_map(raw):
    r = R(raw)
    if r.b() != 77:
        raise ValueError("format inconnu")
    version = r.b()
    map_id = r.I()
    if version >= 7:
        encrypted = r.b() != 0
        r.b()  # encryptionVersion
        data_len = r.i()
        if encrypted:
            data = bytearray(r.d[r.p:r.p + data_len])
            for k in range(len(data)):
                data[k] ^= KEY[k % len(KEY)]
            r = R(bytes(data))
    r.I(); r.b(); r.i()                  # relativeId, mapType, subareaId
    r.i(); r.i(); r.i(); r.i()           # voisins
    r.I()                                # shadowBonusOnEntities
    if version >= 9:
        r.i(); r.I()                     # couleurs fond / grille
    elif version >= 3:
        r.skip(3)
    if version >= 4:
        r.H(); r.h(); r.h()              # zoom
    if version > 10:
        r.i()                            # tacticalModeTemplateId
    r.b()                                # useLowPassFilter
    reverb = r.b() == 1
    if reverb:
        r.i()                            # presetId
    for _ in range(2):                   # fonds puis avant-plans
        for _ in range(r.B()):
            r.skip(4 + 2 * 5 + 3 + 1)
    r.i(); r.i()                         # inconnu, groundCRC
    found = []
    for _ in range(r.B()):               # calques
        if version >= 9:
            r.b()
        else:
            r.i()
        for _ in range(r.h()):           # cellules
            cell = r.h()
            for _ in range(r.h()):       # elements
                t = r.b()
                if t == 2:               # graphique
                    gfx = r.I()
                    r.skip(6)            # teinte + ombre
                    r.skip(2 if version <= 4 else 4)
                    r.b()                # altitude
                    ident = r.I()
                    if ident:
                        found.append((ident, gfx, cell))
                elif t == 33:            # son
                    r.skip(4 + 2 + 4 + 4 + 2 + 2)
                else:
                    raise ValueError("element de type inconnu %d" % t)
    # cellules (CellData.fromRaw) : on compte les cellules d'elevage (farmCell = interieur d'un enclos)
    farm = 0
    for _ in range(560):
        if r.b() == -128:                # cellule vide
            continue
        if version >= 9:
            flags = r.h()
            mov = (flags & 1) == 0
            nwf = (flags & 2) != 0
            is_farm = (flags & 0x80) != 0
            haven = version >= 10 and (flags & 0x100) != 0
        else:
            flags = r.B()
            mov = (flags & 1) == 1
            nwf = (flags & 4) != 0
            is_farm = (flags & 0x20) != 0
            haven = False
        if is_farm:
            farm += 1
        r.b(); r.b()                     # speed, mapChangeData
        if version > 5:
            r.B()                        # moveZone
        if version > 10 and ((mov and not is_farm) or (mov and not nwf and not is_farm and not haven)):
            r.B()                        # linkedZone
        if 7 < version < 9:
            r.b()
    parse_map.farm = farm
    return map_id, found


def read_d2p(path):
    d = open(path, "rb").read()
    data_off, data_cnt, idx_off, idx_cnt = struct.unpack_from(">IIII", d, len(d) - 24)
    p = idx_off
    for _ in range(idx_cnt):
        n = struct.unpack_from(">H", d, p)[0]; p += 2
        name = d[p:p + n].decode("utf-8"); p += n
        off, ln = struct.unpack_from(">ii", d, p); p += 8
        yield name, d[data_off + off:data_off + off + ln]


def main(folder, out):
    maps = errors = 0
    farm_out = open(out + ".farm", "w", encoding="utf-8")  # mapId <TAB> nombre de cellules d'elevage
    with open(out, "w", encoding="utf-8") as o:
        for f in sorted(os.listdir(folder)):
            if not f.endswith(".d2p"):
                continue
            for name, blob in read_d2p(os.path.join(folder, f)):
                if not name.endswith(".dlm"):
                    continue
                try:
                    map_id, found = parse_map(zlib.decompress(blob))
                except Exception as e:
                    errors += 1
                    continue
                maps += 1
                for ident, gfx, cell in found:
                    o.write("%d\t%d\t%d\t%d\n" % (map_id, ident, gfx, cell))
                if parse_map.farm:
                    farm_out.write("%d\t%d\n" % (map_id, parse_map.farm))
    farm_out.close()
    print("maps lues :", maps, "erreurs :", errors, "(cellules d'elevage :", out + ".farm)")


if __name__ == "__main__":
    main(sys.argv[1], sys.argv[2])
