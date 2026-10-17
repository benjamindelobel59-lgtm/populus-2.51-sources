# Liste les cellules praticables (hors combat) de maps du client : mapId <TAB> cellules séparées par des virgules.
# Praticable = mov et pas nonWalkableDuringRP et pas cellule d'élevage. Réutilise le lecteur de maps_elements.py.
# Utilisation : py -I maps_cellules.py <dossier content/maps> <fichier des mapIds (une par ligne ou séparées par des virgules)> <sortie .tsv>
import os, sys, zlib
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import maps_elements as me


def walkable_cells(raw):
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
    if version >= 9:
        r.i(); r.I()
    elif version >= 3:
        r.skip(3)
    if version >= 4:
        r.H(); r.h(); r.h()
    if version > 10:
        r.i()
    r.b()
    if r.b() == 1:
        r.i()
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
        if r.b() == -128:
            continue
        if version >= 9:
            f = r.h()
            mov = (f & 1) == 0; nwf = (f & 2) != 0; nwrp = (f & 4) != 0; farm = (f & 0x80) != 0
            haven = version >= 10 and (f & 0x100) != 0
        else:
            f = r.B()
            mov = (f & 1) == 1; nwf = (f & 4) != 0; nwrp = (f & 0x80) != 0; farm = (f & 0x20) != 0; haven = False
        r.b(); r.b()
        if version > 5:
            r.B()
        if version > 10 and ((mov and not farm) or (mov and not nwf and not farm and not haven)):
            r.B()
        if 7 < version < 9:
            r.b()
        if mov and not nwrp and not farm:
            cells.append(cid)
    return cells


def main(folder, ids_file, out):
    wanted = {int(x) for x in open(ids_file).read().replace("\n", ",").split(",") if x.strip()}
    found = 0
    with open(out, "w") as o:
        for f in sorted(os.listdir(folder)):
            if not f.endswith(".d2p"):
                continue
            for name, blob in me.read_d2p(os.path.join(folder, f)):
                if not name.endswith(".dlm"):
                    continue
                mid = int(os.path.basename(name)[:-4])
                if mid not in wanted:
                    continue
                o.write("%d\t%s\n" % (mid, ",".join(map(str, walkable_cells(zlib.decompress(blob))))))
                found += 1
    print("maps lues :", found, "/", len(wanted))


if __name__ == "__main__":
    main(*sys.argv[1:4])
