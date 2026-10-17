# NEKFEU : ajoute au CLIENT des copies de maps sous un nouvel id (ex. salle du boss -> salle de sortie sans monstres).
#  - content/maps/maps*.d2p : copie du .dlm d'origine (meme .d2p), id de l'en-tete remplace, entree « <id%10>/<id>.dlm »
#  - data/common/MapPositions.d2o : copie de l'objet MapPosition d'origine (coordonnees, sous-zone, nom), id remplace
# Rejouable : une copie deja presente est remplacee. Sauvegarde des fichiers modifies.
# Usage : py -I dupliquer_maps_client.py <copies.tsv> <dossier SAVE> <dossier app du client> [<autre dossier app>...]
#   copies.tsv : id_origine <TAB> id_copie
import os, shutil, struct, sys, time, zlib
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import maps_elements as me

STAMP = time.strftime("%Y-%m-%d_%Hh%M")


def backup(path, backup_dir):
    os.makedirs(backup_dir, exist_ok=True)
    dst = os.path.join(backup_dir, os.path.basename(path) + "-" + STAMP)
    if not os.path.exists(dst):
        shutil.copy2(path, dst)


def read_d2p_full(path):
    d = open(path, "rb").read()
    data_off, data_cnt, idx_off, idx_cnt, prop_off, prop_cnt = struct.unpack_from(">IIIIII", d, len(d) - 24)
    entries = []
    p = idx_off
    for _ in range(idx_cnt):
        n = struct.unpack_from(">H", d, p)[0]; p += 2
        name = d[p:p + n].decode("utf-8"); p += n
        off, ln = struct.unpack_from(">ii", d, p); p += 8
        entries.append([name, d[data_off + off:data_off + off + ln]])
    return d[:data_off], entries, d[prop_off:len(d) - 24], prop_cnt


def write_d2p(path, head, entries, props, prop_cnt):
    blob = bytearray(); index = bytearray()
    for name, content in entries:
        nb = name.encode("utf-8")
        index += struct.pack(">H", len(nb)) + nb + struct.pack(">ii", len(blob), len(content))
        blob += content
    data_off = len(head)
    footer = struct.pack(">IIIIII", data_off, len(blob), data_off + len(blob), len(entries), data_off + len(blob) + len(index), prop_cnt)
    with open(path + ".tmp", "wb") as o:
        o.write(head); o.write(blob); o.write(index); o.write(props); o.write(footer)
    os.replace(path + ".tmp", path)


def copy_dlm(raw, new_id):
    raw = bytearray(raw)
    struct.pack_into(">I", raw, 2, new_id)      # en-tete : 'M', version, mapId
    return bytes(raw)


def patch_maps(maps_dir, copies, backup_dir):
    todo = dict(copies)
    for f in sorted(os.listdir(maps_dir)):
        if not f.endswith(".d2p") or not todo:
            continue
        path = os.path.join(maps_dir, f)
        head, entries, props, prop_cnt = read_d2p_full(path)
        names = {e[0]: i for i, e in enumerate(entries)}
        changed = False
        for name, content in list(entries):
            if not name.endswith(".dlm"):
                continue
            mid = int(os.path.basename(name)[:-4])
            if mid not in todo:
                continue
            new_id = todo.pop(mid)
            new_name = "%d/%d.dlm" % (new_id % 10, new_id)
            new_blob = zlib.compress(copy_dlm(zlib.decompress(content), new_id), 9)
            if new_name in names:
                entries[names[new_name]][1] = new_blob
            else:
                entries.append([new_name, new_blob])
            changed = True
            print("  %s : %d -> %s" % (f, mid, new_name))
        if changed:
            backup(path, backup_dir)
            write_d2p(path, head, entries, props, prop_cnt)
    for mid in todo:
        print("  ATTENTION : map %d introuvable dans %s" % (mid, maps_dir))


def patch_positions(d2o_path, copies, backup_dir):
    d = open(d2o_path, "rb").read()
    assert d[:3] == b"D2O"
    idx = struct.unpack_from(">i", d, 3)[0]
    n = struct.unpack_from(">i", d, idx)[0]
    index = {}
    for p in range(idx + 4, idx + 4 + n, 8):
        k, off = struct.unpack_from(">ii", d, p)
        index[k] = off
    offsets = sorted(index.values()) + [idx]
    span = {off: offsets[i + 1] for i, off in enumerate(offsets[:-1])}
    blob = bytearray(); new_entries = []
    for orig, new_id in copies:
        if orig not in index:
            print("  ATTENTION : MapPosition %d absente" % orig); continue
        o = bytearray(d[index[orig]:span[index[orig]]])
        struct.pack_into(">d", o, 4, float(new_id))   # classe (int) puis id (double)
        new_entries.append((new_id, idx + len(blob)))
        blob += o
    kept = [(k, off) for k, off in index.items() if k not in dict(new_entries)]
    # objets deja copies (relance) : on garde leurs anciens octets dans le fichier mais l'index pointe sur les nouveaux
    entries = kept + new_entries
    new_index = b"".join(struct.pack(">ii", k, off) for k, off in entries)
    out = d[:3] + struct.pack(">i", idx + len(blob)) + d[7:idx] + bytes(blob) + struct.pack(">i", len(new_index)) + new_index + d[idx + 4 + n:]
    backup(d2o_path, backup_dir)
    with open(d2o_path + ".tmp", "wb") as o:
        o.write(out)
    os.replace(d2o_path + ".tmp", d2o_path)
    print("  MapPositions.d2o : %d position(s) ajoutee(s)" % len(new_entries))


def main(tsv, backup_root, *apps):
    copies = []
    for line in open(tsv, encoding="utf-8"):
        parts = line.split()
        if len(parts) >= 2 and parts[0].isdigit():
            copies.append((int(parts[0]), int(parts[1])))
    for app in apps:
        name = os.path.basename(os.path.dirname(os.path.abspath(app)))
        target = "3_CLIENT_64bits" if "64" in name else "4_CLIENT_32bits"   # rangement SAVE/ (16/10)
        print(app)
        patch_maps(os.path.join(app, "content", "maps"), copies, os.path.join(backup_root, target, "maps"))
        patch_positions(os.path.join(app, "data", "common", "MapPositions.d2o"), copies, os.path.join(backup_root, target, "d2o"))


if __name__ == "__main__":
    main(*sys.argv[1:])
