# NEKFEU : lit le worldgraph.bin du client 2.51 (format de com.ankamagames.dofus.modules.utils.pathfinding.world.WorldGraph)
# et exporte toutes les transitions en TSV.
# Usage : python lire_worldgraph.py <worldgraph.bin> <sortie.tsv>
# Colonnes : from_map from_zone to_map to_zone type direction skill_id criterion transition_map cell element_id
# type : 1 SCROLL, 2 SCROLL_ACTION, 4 MAP_EVENT, 8 MAP_ACTION, 16 MAP_OBSTACLE, 32 INTERACTIVE, 64 NPC_ACTION
import struct
import sys


def main(src, dst):
    data = open(src, 'rb').read()
    pos = 0

    def read(fmt):
        nonlocal pos
        size = struct.calcsize(fmt)
        value = struct.unpack_from(fmt, data, pos)
        pos += size
        return value[0] if len(value) == 1 else value

    edges = read('>i')
    rows = 0
    with open(dst, 'w', encoding='utf-8', newline='') as out:
        out.write('from_map\tfrom_zone\tto_map\tto_zone\ttype\tdirection\tskill_id\tcriterion\ttransition_map\tcell\telement_id\n')
        for _ in range(edges):
            from_map, from_zone, to_map, to_zone = read('>d'), read('>i'), read('>d'), read('>i')
            for _ in range(read('>i')):
                direction, ttype, skill = read('>b'), read('>b'), read('>i')
                length = read('>i')
                criterion = data[pos:pos + length].decode('utf-8', 'replace')
                pos += length
                tmap, cell, element = read('>d'), read('>i'), read('>d')
                out.write('%d\t%d\t%d\t%d\t%d\t%d\t%d\t%s\t%d\t%d\t%d\n' % (
                    from_map, from_zone, to_map, to_zone, ttype, direction, skill,
                    criterion.replace('\t', ' '), tmap, cell, element))
                rows += 1
    print('%d aretes, %d transitions, %d octets lus sur %d' % (edges, rows, pos, len(data)))


if __name__ == '__main__':
    main(sys.argv[1], sys.argv[2])
