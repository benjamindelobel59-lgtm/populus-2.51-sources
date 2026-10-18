# -*- coding: utf-8 -*-
"""
MYSTIC 18/10 : remplace la liste d'actions d'un PNJ du client (Npcs.d2o), ex. Marchand de Doplons (PNJ 42) : [3] (Parler) -> [1] (Acheter/Vendre).
Le client construit le menu du PNJ à partir de cette liste ; sans l'action 1, « Acheter/Vendre » n'apparaissait jamais.
L'objet modifié est réécrit en fin de fichier et l'index pointe vers lui (comme trousseau_pnj_client.py).

Utilisation : python -I pnj_actions_client.py <Npcs.d2o source> <Npcs.d2o sortie> <pnj> <action>[,<action>...]
"""
import os, struct, sys
ICI = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, ICI)
sys.path.insert(0, os.path.join(os.path.dirname(ICI), "donjons"))
import d2o
from trousseau_pnj_client import ser


def main(source, sortie, pnj, actions):
    classes, objs = d2o.load(source)
    data = bytearray(open(source, "rb").read())
    idx = struct.unpack_from(">i", data, 3)[0]
    n = struct.unpack_from(">i", data, idx)[0]
    index = {}
    for i in range(n // 8):
        k, pos = struct.unpack_from(">ii", data, idx + 4 + i * 8)
        index[k] = pos
    after = bytes(data[idx + 4 + n:])

    o = objs[pnj]
    print("PNJ", pnj, ": actions", o["actions"], "->", actions)
    o["actions"] = actions
    cid = struct.unpack_from(">i", data, index[pnj])[0]
    index[pnj] = idx
    blob = ser(classes, cid, o)
    new_index = b"".join(struct.pack(">ii", k, p) for k, p in index.items())
    out = bytes(data[:3]) + struct.pack(">i", idx + len(blob)) + bytes(data[7:idx]) + blob + struct.pack(">i", len(new_index)) + new_index + after
    open(sortie, "wb").write(out)
    _, verif = d2o.load(sortie)
    assert len(verif) == len(objs) and verif[pnj]["actions"] == actions
    print("ok :", sortie)


if __name__ == "__main__":
    main(sys.argv[1], sys.argv[2], int(sys.argv[3]), [int(x) for x in sys.argv[4].split(",")])
