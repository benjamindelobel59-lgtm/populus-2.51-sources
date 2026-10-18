# MYSTIC 17/10 : exporte les pépites de recyclage des objets (Items.d2o, champ officiel nuggetsBySubarea = [[sousZone, pépites], ...])
# en SQL pour le serveur (table nekfeu_items_pepites).
# Usage : py -I d2o_pepites.py <Items.d2o> <sortie.sql>
import struct, sys

d = open(sys.argv[1], "rb").read()
assert d[:3] == b"D2O"
p_index = struct.unpack_from(">i", d, 3)[0]
n_index = struct.unpack_from(">i", d, p_index)[0]
index = [struct.unpack_from(">ii", d, p_index + 4 + 8 * i) for i in range(n_index // 8)]


def utf(p):
    ln = struct.unpack_from(">H", d, p)[0]
    return d[p + 2:p + 2 + ln].decode("utf-8"), p + 2 + ln


# définitions des classes
p = p_index + 4 + n_index
n_classes = struct.unpack_from(">i", d, p)[0]; p += 4
classes = {}
for _ in range(n_classes):
    cid = struct.unpack_from(">i", d, p)[0]; p += 4
    name, p = utf(p); _, p = utf(p)
    nf = struct.unpack_from(">i", d, p)[0]; p += 4
    fields = []
    for _ in range(nf):
        fname, p = utf(p)
        t = struct.unpack_from(">i", d, p)[0]; p += 4
        types = [t]
        while types[-1] == -99:
            _, p = utf(p)
            types.append(struct.unpack_from(">i", d, p)[0]); p += 4
        fields.append((fname, types))
    classes[cid] = (name, fields)


def lire(types, p):
    t = types[0]
    if t == -1 or t == -5:
        return struct.unpack_from(">i", d, p)[0], p + 4
    if t == -6:
        return struct.unpack_from(">I", d, p)[0], p + 4
    if t == -2:
        return d[p] != 0, p + 1
    if t == -3:
        return utf(p)
    if t == -4:
        return struct.unpack_from(">d", d, p)[0], p + 8
    if t == -99:
        n = struct.unpack_from(">i", d, p)[0]; p += 4
        res = []
        for _ in range(n):
            v, p = lire(types[1:], p)
            res.append(v)
        return res, p
    # objet : classId puis champs (-1431655766 = null)
    cid = struct.unpack_from(">i", d, p)[0]; p += 4
    if cid == -1431655766:
        return None, p
    return lire_objet(cid, p)


def lire_objet(cid, p):
    obj = {}
    for fname, types in classes[cid][1]:
        obj[fname], p = lire(types, p)
    return obj, p


lignes = []
for key, pos in index:
    cid = struct.unpack_from(">i", d, pos)[0]
    obj, _ = lire_objet(cid, pos + 4)
    for sub in obj.get("nuggetsBySubarea") or []:
        if len(sub) >= 2 and sub[1] > 0:
            lignes.append("(%d,%d,%s)" % (obj["id"], int(sub[0]), repr(float(sub[1]))))

with open(sys.argv[2], "w", encoding="utf-8", newline="\n") as f:
    f.write("-- MYSTIC 17/10 : pépites de recyclage par objet et sous-zone (Items.d2o, nuggetsBySubarea)\n")
    f.write("CREATE TABLE IF NOT EXISTS nekfeu_items_pepites (ItemId INT NOT NULL, SubAreaId INT NOT NULL, Pepites DOUBLE NOT NULL, PRIMARY KEY (ItemId, SubAreaId));\n")
    f.write("DELETE FROM nekfeu_items_pepites;\n")
    for i in range(0, len(lignes), 500):
        f.write("INSERT INTO nekfeu_items_pepites VALUES " + ",".join(lignes[i:i + 500]) + ";\n")
print(len(lignes), "lignes ;", len({l.split(',')[0] for l in lignes}), "objets")
