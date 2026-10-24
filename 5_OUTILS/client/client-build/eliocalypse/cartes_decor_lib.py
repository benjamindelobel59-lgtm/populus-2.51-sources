# PADGREF 23/10 : lecture / écriture des archives .d2p (repris de cartes_decor.py)
import os, struct


def lire_d2p(path):
    d = open(path, 'rb').read()
    data_off, _, idx_off, idx_cnt, prop_off, prop_cnt = struct.unpack_from('>6I', d, len(d) - 24)
    ent, p = [], idx_off
    for _ in range(idx_cnt):
        n = struct.unpack_from('>H', d, p)[0]; p += 2
        nom = d[p:p + n].decode('utf-8'); p += n
        off, ln = struct.unpack_from('>ii', d, p); p += 8
        ent.append([nom, d[data_off + off:data_off + off + ln]])
    return d[:data_off], ent, d[prop_off:len(d) - 24], prop_cnt


def ecrire_d2p(path, head, ent, props, prop_cnt):
    os.makedirs(os.path.dirname(path), exist_ok=True)
    blob, index = bytearray(), bytearray()
    for nom, c in ent:
        nb = nom.encode('utf-8')
        index += struct.pack('>H', len(nb)) + nb + struct.pack('>ii', len(blob), len(c)); blob += c
    off = len(head)
    pied = struct.pack('>6I', off, len(blob), off + len(blob), len(ent), off + len(blob) + len(index), prop_cnt)
    open(path, 'wb').write(bytes(head) + bytes(blob) + bytes(index) + bytes(props) + pied)


def lien(path):
    """Propriété « link » d'une archive .d2p (archive suivante de la chaîne), ou None."""
    d = open(path, 'rb').read()
    _, _, _, _, prop_off, prop_cnt = struct.unpack_from('>6I', d, len(d) - 24)
    q = prop_off
    for _ in range(prop_cnt):
        n = struct.unpack_from('>H', d, q)[0]; a = d[q + 2:q + 2 + n].decode(); q += 2 + n
        n = struct.unpack_from('>H', d, q)[0]; b = d[q + 2:q + 2 + n].decode(); q += 2 + n
        if a == 'link':
            return b
    return None


def fin_de_chaine(dossier, depart):
    """PADGREF 22/10 : dernière archive de la chaîne lue par le client (config.xml : pak://…/<depart>|, puis les « link »).
    Une archive hors chaîne (ex. maps1_8.d2p) n'est JAMAIS lue par le client."""
    f, vus = depart, []
    while f and f not in vus:
        vus.append(f)
        f = lien(os.path.join(dossier, f))
    return os.path.join(dossier, vus[-1]), vus
