# -*- coding: utf-8 -*-
"""
MYSTIC 17/10 : ajoute la réponse « Utiliser le trousseau de clefs » (texte officiel) à des PNJ du client (Npcs.d2o)
qui ne l'ont pas dans les données 2.51 (ex. gardien de l'Œil de Vortex, PNJ 2662 : les dimensions n'ont pas de trousseau officiel).
L'objet PNJ modifié est réécrit en fin de fichier et l'index pointe vers lui (l'ancien reste, inutilisé). Rejouable.

Utilisation : python -I trousseau_pnj_client.py <Npcs.d2o> <pnj>[,<pnj>...]
"""
import os, struct, sys
ICI = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, os.path.join(os.path.dirname(ICI), "donjons"))
import d2o

REPONSE_TROUSSEAU = 8450      # id de réponse « Utiliser le trousseau de clefs » (Sylvie Barbe), réutilisé côté serveur
PNJ_MODELE = 1391             # Sylvie Barbe : on y lit le texte (i18n) de cette réponse


def utf(s):
    b = s.encode("utf-8")
    return struct.pack(">H", len(b)) + b


def ser(classes, cid, o):
    name, fields = classes[cid]
    out = struct.pack(">i", cid)
    for fname, t, sub in fields:
        out += val(classes, t, sub, o[fname])
    return out


def val(classes, t, sub, v):
    if t in (-1, -5):
        return struct.pack(">i", v)
    if t == -2:
        return b"\x01" if v else b"\x00"
    if t == -3:
        return utf(v)
    if t == -4:
        return struct.pack(">d", v)
    if t == -6:
        return struct.pack(">I", v)
    if t == -99:
        return struct.pack(">i", len(v)) + b"".join(val(classes, sub[0], sub[1:], x) for x in v)
    if v is None:
        return struct.pack(">i", -1431655766)
    return ser(classes, t, v)


def main(path, pnjs):
    classes, objs = d2o.load(path)
    data = bytearray(open(path, "rb").read())
    idx = struct.unpack_from(">i", data, 3)[0]
    n = struct.unpack_from(">i", data, idx)[0]
    index = {}
    for i in range(n // 8):
        k, pos = struct.unpack_from(">ii", data, idx + 4 + i * 8)
        index[k] = pos
    after = bytes(data[idx + 4 + n:])

    texte = next(r[1] for r in objs[PNJ_MODELE]["dialogReplies"] if r[0] == REPONSE_TROUSSEAU)
    blob = b""
    for pnj in pnjs:
        o = objs[pnj]
        if any(r[0] == REPONSE_TROUSSEAU for r in o["dialogReplies"]):
            print("PNJ", pnj, ": réponse trousseau déjà présente")
            continue
        o["dialogReplies"].append([REPONSE_TROUSSEAU, texte])
        cid = struct.unpack_from(">i", data, index[pnj])[0]
        index[pnj] = idx + len(blob)
        blob += ser(classes, cid, o)
        print("PNJ", pnj, ": réponse", REPONSE_TROUSSEAU, "ajoutée (texte", texte, ")")
    if not blob:
        return
    new_index = b"".join(struct.pack(">ii", k, p) for k, p in index.items())
    out = bytes(data[:3]) + struct.pack(">i", idx + len(blob)) + bytes(data[7:idx]) + blob + struct.pack(">i", len(new_index)) + new_index + after
    open(path, "wb").write(out)
    _, verif = d2o.load(path)
    assert len(verif) == len(objs)
    for pnj in pnjs:
        assert any(r[0] == REPONSE_TROUSSEAU for r in verif[pnj]["dialogReplies"])
    for k in objs:
        if k not in pnjs:
            assert verif[k] == objs[k]
    print("écrit", path)


if __name__ == "__main__":
    main(sys.argv[1], [int(x) for x in sys.argv[2].split(",")])
