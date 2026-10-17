# Change iconVisibilityMask d'un etat dans SpellStates.d2o (modif en place de 4 octets).
# Usage : py -I d2o_state_visibility.py <SpellStates.d2o> <stateId> <nouvelleValeur>
# Valeurs : 0 = tous, 1 = cible, 2 = lanceur, 3 = lanceur + allies
import struct, sys

path, state_id, new_val = sys.argv[1], int(sys.argv[2]), int(sys.argv[3])
d = bytearray(open(path, "rb").read())
assert d[:3] == b"D2O"
p = struct.unpack_from(">i", d, 3)[0]
n = struct.unpack_from(">i", d, p)[0]; p += 4
index = {}
for _ in range(n // 8):
    k, v = struct.unpack_from(">ii", d, p); p += 8; index[k] = v

def utf(p):
    ln = struct.unpack_from(">H", d, p)[0]
    return d[p + 2:p + 2 + ln].decode(), p + 2 + ln

ncls = struct.unpack_from(">i", d, p)[0]; p += 4
assert ncls == 1
p += 4; name, p = utf(p); _, p = utf(p)
assert name == "SpellState"
fields = []
nf = struct.unpack_from(">i", d, p)[0]; p += 4
for _ in range(nf):
    fname, p = utf(p); t = struct.unpack_from(">i", d, p)[0]; p += 4
    sub = []
    while t == -99 or (sub and sub[-1] == -99):
        _, p = utf(p); st = struct.unpack_from(">i", d, p)[0]; p += 4; sub.append(st)
        if st != -99: break
    fields.append((fname, t, sub))

pos = index[state_id] + 4  # saute classId
for fname, t, sub in fields:
    if fname == "iconVisibilityMask":
        old = struct.unpack_from(">i", d, pos)[0]
        struct.pack_into(">i", d, pos, new_val)
        open(path, "wb").write(d)
        print("etat %d : iconVisibilityMask %d -> %d (offset %d)" % (state_id, old, new_val, pos))
        break
    if t in (-1, -5, -6): pos += 4
    elif t == -2: pos += 1
    elif t == -4: pos += 8
    elif t == -3: pos += 2 + struct.unpack_from(">H", d, pos)[0]
    elif t == -99:
        assert sub == [-1], "vecteur non gere"
        pos += 4 + 4 * struct.unpack_from(">i", d, pos)[0]
    else:
        raise SystemExit("type non gere : %s %d" % (fname, t))
else:
    raise SystemExit("champ iconVisibilityMask introuvable")
