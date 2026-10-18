# -*- coding: utf-8 -*-
"""
MYSTIC 18/10 : placements OFFICIELS des salles de donjon = cases rouges / bleues des maps du client 2.51 d'origine
(_ARCHIVES/client 2.51.rar, content/maps/*.d2p, CellData : v>=9 bleu 0x10 / rouge 0x20, sinon bleu 0x10 / rouge 0x08).
Compare avec ce que le serveur utilise (world_maps.BlueCellsCSV / RedCellsCSV, sinon les cases de CompressedCells).

Utilisation : py -I placements_officiels.py <dossier maps d'origine (.d2p)> <rapport.tsv> [--sql F]
  --sql F : écrit F = UPDATE world_maps remettant les placements officiels (dans l'ordre des cases du client).
"""
import os, sys, zlib
sys.stdout.reconfigure(encoding="utf-8", errors="replace")
ICI = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, os.path.join(ICI, '..', '..', 'client', 'client-build'))
sys.path.insert(0, ICI)
import maps_elements as me
from placements_8v8 import requete, cellules


def cases_officielles(raw):
    r = me.R(raw)
    r.b(); version = r.b(); map_id = r.I()
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
        if version >= 9:
            r.b()
        else:
            r.i()
        for _ in range(r.h()):
            r.h()
            for _ in range(r.h()):
                t = r.b()
                if t == 2:
                    r.I(); r.skip(6); r.skip(2 if version <= 4 else 4); r.b(); r.I()
                elif t == 33:
                    r.skip(4 + 2 + 4 + 4 + 2 + 2)
                else:
                    raise ValueError("element %d" % t)
    bleu, rouge = [], []
    for cid in range(560):
        if r.b() == -128:
            continue
        if version >= 9:
            f = r.h()
            mov = (f & 1) == 0; nwf = (f & 2) != 0; farm = (f & 0x80) != 0
            haven = version >= 10 and (f & 0x100) != 0
            b, rr = (f & 0x10) != 0, (f & 0x20) != 0
        else:
            f = r.B()
            mov = (f & 1) == 1; nwf = (f & 4) != 0; farm = (f & 0x20) != 0; haven = False
            b, rr = (f & 0x10) != 0, (f & 0x08) != 0
        if b:
            bleu.append(cid)
        if rr:
            rouge.append(cid)
        r.b(); r.b()
        if version > 5:
            r.B()
        if version > 10 and ((mov and not farm) or (mov and not nwf and not farm and not haven)):
            r.B()
        if 7 < version < 9:
            r.b()
    return map_id, bleu, rouge


def main():
    dossier, rapport = sys.argv[1], sys.argv[2]
    sql_out = sys.argv[sys.argv.index('--sql') + 1] if '--sql' in sys.argv else None
    salles = requete("""select m.Id, m.Version, ifnull(m.BlueCellsCSV,''), ifnull(m.RedCellsCSV,''), hex(m.CompressedCells),
                               group_concat(distinct d.Descripcion separator ' / ')
                        from world_maps m join monsters_spawns_dungeons d on d.MapId = m.Id group by m.Id order by m.Id""")
    voulues = {int(x[0]) for x in salles}
    officiel = {}
    for f in sorted(os.listdir(dossier)):
        if f.endswith('.d2p'):
            for name, blob in me.read_d2p(os.path.join(dossier, f)):
                if name.endswith('.dlm') and int(os.path.basename(name)[:-4]) in voulues:
                    mid, b, r = cases_officielles(zlib.decompress(blob))
                    officiel[mid] = (b, r)
    stats = dict(identique=0, ordre=0, different=0, absente=0, sans_officiel=0)
    sql = []
    with open(rapport, 'w', encoding='utf-8') as o:
        o.write("MapId\tSalle\tEtat\tBleuOfficiel\tRougeOfficiel\tBleuServeur\tRougeServeur\n")
        for mid, version, bcsv, rcsv, hexa, descr in salles:
            mid = int(mid)
            bs = [int(x) for x in bcsv.split(',') if x.strip()]
            rs = [int(x) for x in rcsv.split(',') if x.strip()]
            if not bs or not rs:
                cells = cellules(int(version), hexa)
                bs = [c['id'] for c in cells if c['blue']]; rs = [c['id'] for c in cells if c['red']]
            if mid not in officiel:
                etat = 'map absente du client'; stats['absente'] += 1; bo = ro = []
            else:
                bo, ro = officiel[mid]
                if not bo or not ro:
                    etat = 'pas de placement dans le client'; stats['sans_officiel'] += 1
                elif bo == bs and ro == rs:
                    etat = 'identique'; stats['identique'] += 1
                elif set(bo) == set(bs) and set(ro) == set(rs):
                    etat = 'memes cases, ordre different'; stats['ordre'] += 1
                else:
                    etat = 'different'; stats['different'] += 1
                if bo and ro and etat != 'identique':
                    sql.append(f"UPDATE world_maps SET BlueCellsCSV='{','.join(map(str, bo))}', RedCellsCSV='{','.join(map(str, ro))}' WHERE Id={mid}; -- {descr}")
            o.write(f"{mid}\t{descr}\t{etat}\t{','.join(map(str, bo))}\t{','.join(map(str, ro))}\t{','.join(map(str, bs))}\t{','.join(map(str, rs))}\n")
    print(len(salles), 'salles ;', stats)
    if sql_out:
        open(sql_out, 'w', encoding='utf-8').write("-- MYSTIC : placements officiels du client 2.51 (salles de donjon)\n" + '\n'.join(sql) + '\n')
        print(len(sql), 'UPDATE ->', sql_out)


if __name__ == '__main__':
    main()
