# MYSTIC 17/10 : rend « utilisables » (champ booléen usable de Items.d2o) les objets de certains types (ex. certificats de monture
# 97 / 196 / 207) : le client affiche alors « Utiliser » et envoie ObjectUseMessage. Taille du fichier inchangée (1 octet modifié par objet).
# Usage : py -I d2o_items_utilisables.py <Items.d2o> <sortie> <typeId>[,<typeId>...]
import struct, sys

src, out, types = sys.argv[1], sys.argv[2], {int(x) for x in sys.argv[3].split(',')}
d = bytearray(open(src, 'rb').read())
assert d[:3] == b'D2O'
p_index = struct.unpack_from('>i', d, 3)[0]
n_index = struct.unpack_from('>i', d, p_index)[0]
index = [struct.unpack_from('>ii', d, p_index + 4 + 8 * i) for i in range(n_index // 8)]


def utf(p):
    ln = struct.unpack_from('>H', d, p)[0]
    return d[p + 2:p + 2 + ln].decode('utf-8'), p + 2 + ln


p = p_index + 4 + n_index
classes = {}
for _ in range(struct.unpack_from('>i', d, p)[0] if False else 0):
    pass
nc = struct.unpack_from('>i', d, p)[0]; p += 4
for _ in range(nc):
    cid = struct.unpack_from('>i', d, p)[0]; p += 4
    _, p = utf(p); _, p = utf(p)
    nf = struct.unpack_from('>i', d, p)[0]; p += 4
    fields = []
    for _ in range(nf):
        nom, p = utf(p)
        t = struct.unpack_from('>i', d, p)[0]; p += 4
        ts = [t]
        while ts[-1] == -99:
            _, p = utf(p)
            ts.append(struct.unpack_from('>i', d, p)[0]); p += 4
        fields.append((nom, ts))
    classes[cid] = fields


def sauter(ts, p, positions, nom=None):
    t = ts[0]
    if t in (-1, -5, -6):
        return p + 4
    if t == -2:
        if nom is not None:
            positions[nom] = p
        return p + 1
    if t == -3:
        return p + 2 + struct.unpack_from('>H', d, p)[0]
    if t == -4:
        return p + 8
    if t == -99:
        n = struct.unpack_from('>i', d, p)[0]; p += 4
        for _ in range(n):
            p = sauter(ts[1:], p, {})
        return p
    cid = struct.unpack_from('>i', d, p)[0]; p += 4
    if cid == -1431655766:
        return p
    return objet(cid, p, {})


def objet(cid, p, positions, valeurs=None):
    for nom, ts in classes[cid]:
        if valeurs is not None and nom == 'typeId':
            valeurs['typeId'] = struct.unpack_from('>i', d, p)[0]
        p = sauter(ts, p, positions, nom)
    return p


modifies = 0
for key, pos in index:
    cid = struct.unpack_from('>i', d, pos)[0]
    positions, valeurs = {}, {}
    objet(cid, pos + 4, positions, valeurs)
    if valeurs.get('typeId') in types and 'usable' in positions:
        if d[positions['usable']] != 1:
            d[positions['usable']] = 1
            modifies += 1
open(out, 'wb').write(d)
print(modifies, 'objets rendus utilisables')
