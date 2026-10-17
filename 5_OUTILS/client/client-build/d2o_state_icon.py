# MYSTIC 17/10 : change l'icone (au-dessus de la tete en combat) d'un ou plusieurs etats dans SpellStates.d2o.
# La chaine change de longueur : les donnees suivantes sont decalees et l'index (positions des objets) est recalcule.
# Usage : py -I d2o_state_icon.py <SpellStates.d2o source> <sortie> <stateId>=<icone> [<stateId>=<icone> ...]
# Icones disponibles : noms des symboles de content/gfx/icons/stateBuffs.swf (ex. stateImmovable = enclume rouge/blanche).
import struct, sys

src, out = sys.argv[1], sys.argv[2]
changes = {int(a.split("=")[0]): a.split("=")[1] for a in sys.argv[3:]}
d = bytearray(open(src, "rb").read())
assert d[:3] == b"D2O"


def lire_index(d):
    p = struct.unpack_from(">i", d, 3)[0]
    n = struct.unpack_from(">i", d, p)[0]
    return p, n, {struct.unpack_from(">i", d, p + 4 + 8 * i)[0]: struct.unpack_from(">i", d, p + 8 + 8 * i)[0] for i in range(n // 8)}


def utf(d, p):
    ln = struct.unpack_from(">H", d, p)[0]
    return d[p + 2:p + 2 + ln].decode(), p + 2 + ln


def champs(d):
    p, n, _ = lire_index(d)
    p += 4 + n
    assert struct.unpack_from(">i", d, p)[0] == 1
    p += 8
    name, p = utf(d, p); _, p = utf(d, p)
    assert name == "SpellState"
    res = []
    nf = struct.unpack_from(">i", d, p)[0]; p += 4
    for _ in range(nf):
        fname, p = utf(d, p); t = struct.unpack_from(">i", d, p)[0]; p += 4
        sub = []
        while t == -99 or (sub and sub[-1] == -99):
            _, p = utf(d, p); st = struct.unpack_from(">i", d, p)[0]; p += 4; sub.append(st)
            if st != -99:
                break
        res.append((fname, t, sub))
    return res


def position_icone(d, fields, pos):
    pos += 4  # classId
    for fname, t, sub in fields:
        if fname == "icon":
            return pos
        if t in (-1, -5, -6): pos += 4
        elif t == -2: pos += 1
        elif t == -4: pos += 8
        elif t == -3: pos += 2 + struct.unpack_from(">H", d, pos)[0]
        elif t == -99:
            assert sub == [-1], "vecteur non gere"
            pos += 4 + 4 * struct.unpack_from(">i", d, pos)[0]
        else:
            raise SystemExit("type non gere : %s %d" % (fname, t))
    raise SystemExit("champ icon introuvable")


fields = champs(d)
for sid, icone in changes.items():
    pidx, n, index = lire_index(d)
    pos = position_icone(d, fields, index[sid])
    ancien, fin = utf(d, pos)
    neuf = icone.encode()
    nouveau = struct.pack(">H", len(neuf)) + neuf
    delta = len(nouveau) - (fin - pos)
    d[pos:fin] = nouveau
    pidx += delta
    struct.pack_into(">i", d, 3, pidx)
    for i in range(n // 8):
        k, v = struct.unpack_from(">ii", d, pidx + 4 + 8 * i)
        if v > pos:
            struct.pack_into(">i", d, pidx + 8 + 8 * i, v + delta)
    print("etat %d : icon %r -> %r (decalage %+d)" % (sid, ancien, icone, delta))
open(out, "wb").write(d)
