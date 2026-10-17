# NEKFEU : reporte dans les maps du CLIENT (content/maps/maps*.d2p) les cases modifiees cote serveur (panel admin, onglet Cartes :
# praticable, ligne de vue, interdite en combat / hors combat, passages de bord). Remplace le « Mettre-a-jour-cases-client.bat »
# de l'autre dev (absent). Le client garde sa propre copie des cases : sans ca, un joueur marche toujours sur une case bloquee.
#
# Usage : py -I maj_cases_client.py <cases_serveur.tsv> <dossier SAVE> <dossier content/maps> [<autre dossier content/maps>...]
#   cases_serveur.tsv : mapId <TAB> version <TAB> HEX(CompressedCells)   (export de world_maps pour les maps voulues)
#
# Pour chaque case qui differe du serveur : octet bas des drapeaux (v>=9) / octet losmov (v<9) = LosMov du serveur,
# mapChangeData = celui du serveur, fleches de bord (v>=9) recalculees si les passages ont change, octet « zone liee » (v>10)
# ajoute / retire selon que la case est praticable (mov et pas enclos). La map est rechiffree, recompressee, le .d2p reconstruit.
import gzip, os, shutil, struct, sys, time, zlib
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import maps_elements as me

MC_TOP, MC_BOTTOM, MC_RIGHT, MC_LEFT = 64, 4, 1, 16


def server_cells(hexdata):
    blob = bytes.fromhex(hexdata)
    try:
        raw = gzip.decompress(blob)
    except OSError:
        raw = zlib.decompress(blob)
    return [(raw[i + 4], raw[i + 5]) for i in range(0, len(raw) - 10, 11)]


def patch_dlm(raw, srv, report):
    """raw = .dlm decompresse ; srv = [(losmov, mapchange)] x 560. Renvoie le nouveau .dlm ou None si rien a changer."""
    r = me.R(raw)
    r.b(); version = r.b(); map_id = r.I()
    enc = False
    if version >= 7:
        enc = r.b() != 0; r.b(); n = r.i()
        head = bytearray(raw[:r.p])
        data = bytearray(raw[r.p:r.p + n])
        if enc:
            for k in range(len(data)):
                data[k] ^= me.KEY[k % len(me.KEY)]
    else:
        head = bytearray(raw[:r.p])
        data = bytearray(raw[r.p:])
    d = me.R(bytes(data))
    d.I(); d.b(); d.i(); d.i(); d.i(); d.i(); d.i(); d.I()
    if version >= 9:
        d.i(); d.I()
    elif version >= 3:
        d.skip(3)
    if version >= 4:
        d.H(); d.h(); d.h()
    if version > 10:
        d.i()
    d.b()
    if d.b() == 1:
        d.i()
    for _ in range(2):
        for _ in range(d.B()):
            d.skip(4 + 2 * 5 + 3 + 1)
    d.i(); d.i()
    for _ in range(d.B()):
        d.b() if version >= 9 else d.i()
        for _ in range(d.h()):
            d.h()
            for _ in range(d.h()):
                t = d.b()
                if t == 2:
                    d.skip(4 + 6 + (2 if version <= 4 else 4) + 1 + 4)
                elif t == 33:
                    d.skip(18)
    cells_start = d.p
    cells = []          # (floor, flags, speed, mc, movezone, linked, arrowbyte) ou (floor,) pour une case vide
    for cid in range(560):
        floor = d.b()
        if floor == -128:
            cells.append((floor,))
            continue
        flags = d.h() if version >= 9 else d.B()
        speed = d.b(); mc = d.B()
        movezone = d.B() if version > 5 else None
        if version >= 9:
            mov = (flags & 1) == 0; farm = (flags & 0x80) != 0
        else:
            mov = (flags & 1) == 1; farm = (flags & 0x20) != 0
        linked = d.B() if version > 10 and mov and not farm else None
        arrow = d.b() if 7 < version < 9 else None
        cells.append((floor, flags, speed, mc, movezone, linked, arrow))
    if d.p != len(data):
        raise ValueError("map %d : %d octets apres les cases (format inattendu)" % (map_id, len(data) - d.p))
    zones = [c[5] for c in cells if len(c) > 1 and c[5] is not None]
    default_zone = max(set(zones), key=zones.count) if zones else 1

    out = bytearray()
    changed = 0
    for cid, c in enumerate(cells):
        if len(c) == 1:
            out += struct.pack(">b", c[0])
            continue
        floor, flags, speed, mc, movezone, linked, arrow = c
        s_los, s_mc = srv[cid] if cid < len(srv) else (None, None)
        nflags, nmc = flags, mc
        if s_los is not None:
            if version >= 9:
                nflags = (flags & 0xFF00) | s_los
                if s_mc != mc:
                    bits = (0x200, 0x400, 0x800, 0x1000) if version >= 10 else (0x100, 0x200, 0x400, 0x800)
                    nflags &= ~(bits[0] | bits[1] | bits[2] | bits[3]) & 0xFFFF
                    for b, m in zip(bits, (MC_TOP, MC_BOTTOM, MC_RIGHT, MC_LEFT)):
                        if s_mc & m:
                            nflags |= b
            else:
                nflags = s_los
            nmc = s_mc
        if (nflags, nmc) != (flags, mc):
            changed += 1
        if version >= 9:
            mov = (nflags & 1) == 0; farm = (nflags & 0x80) != 0
        else:
            mov = (nflags & 1) == 1; farm = (nflags & 0x20) != 0
        out += struct.pack(">b", floor)
        out += struct.pack(">H", nflags & 0xFFFF) if version >= 9 else bytes([nflags & 0xFF])
        out += struct.pack(">bB", speed, nmc & 0xFF)
        if movezone is not None:
            out += bytes([movezone])
        if version > 10 and mov and not farm:
            out += bytes([linked if linked is not None else default_zone])
        if arrow is not None:
            out += struct.pack(">b", arrow)
    if not changed:
        return None
    report.append((map_id, version, changed))
    data = data[:cells_start] + out
    if version >= 7:
        struct.pack_into(">i", head, len(head) - 4, len(data))
        if enc:
            for k in range(len(data)):
                data[k] ^= me.KEY[k % len(me.KEY)]
    return bytes(head) + bytes(data)


def rebuild_d2p(path, replace, backup_dir):
    d = open(path, "rb").read()
    data_off, data_cnt, idx_off, idx_cnt, prop_off, prop_cnt = struct.unpack_from(">IIIIII", d, len(d) - 24)
    entries = []
    p = idx_off
    for _ in range(idx_cnt):
        n = struct.unpack_from(">H", d, p)[0]; p += 2
        name = d[p:p + n]; p += n
        off, ln = struct.unpack_from(">ii", d, p); p += 8
        entries.append((name, off, ln))
    props = d[prop_off:len(d) - 24]
    blob = bytearray()
    index = bytearray()
    for name, off, ln in entries:
        content = replace.get(name.decode("utf-8"), d[data_off + off:data_off + off + ln])
        index += struct.pack(">H", len(name)) + name + struct.pack(">ii", len(blob), len(content))
        blob += content
    head = d[:data_off]
    new_idx_off = data_off + len(blob)
    new_prop_off = new_idx_off + len(index)
    footer = struct.pack(">IIIIII", data_off, len(blob), new_idx_off, idx_cnt, new_prop_off, prop_cnt)
    os.makedirs(backup_dir, exist_ok=True)
    shutil.copy2(path, os.path.join(backup_dir, os.path.basename(path) + "-" + time.strftime("%Y-%m-%d_%Hh%M")))
    with open(path + ".tmp", "wb") as o:
        o.write(head); o.write(blob); o.write(index); o.write(props); o.write(footer)
    os.replace(path + ".tmp", path)


def save_dir(folder):
    # NEKFEU : rangement SAVE/ (16/10) : un dossier par client
    client = os.path.basename(os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(folder)))))
    return "3_CLIENT_64bits" if "64" in client else "4_CLIENT_32bits"


def main(tsv, backup_root, *folders):
    srv = {}
    for line in open(tsv, encoding="utf-8"):
        parts = line.rstrip("\r\n").split("\t")
        if len(parts) == 3 and parts[0].isdigit() and parts[2]:
            srv[int(parts[0])] = server_cells(parts[2])
    print("%d map(s) a reporter" % len(srv))
    for folder in folders:
        total = []
        for f in sorted(os.listdir(folder)):
            if not f.endswith(".d2p"):
                continue
            path = os.path.join(folder, f)
            replace = {}
            for name, blob in me.read_d2p(path):
                if not name.endswith(".dlm"):
                    continue
                mid = int(os.path.basename(name)[:-4])
                if mid not in srv:
                    continue
                new = patch_dlm(zlib.decompress(blob), srv[mid], total)
                if new is not None:
                    zlib.decompress(zlib.compress(new, 9))
                    replace[name] = zlib.compress(new, 9)
            if replace:
                rebuild_d2p(path, replace, os.path.join(backup_root, save_dir(folder), "maps_cases"))
                print("  %s : %d map(s) reecrite(s)" % (path, len(replace)))
        for map_id, version, n in total:
            print("  map %d (v%d) : %d case(s) mise(s) a jour" % (map_id, version, n))
        if not total:
            print("  %s : rien a changer" % folder)


if __name__ == "__main__":
    main(*sys.argv[1:])
