# NEKFEU 2026-10-16 : donjons dont le boss renvoyait a Astrub (map 191105026) au lieu de leur salle de fin.
# - Minotoror (4), Brumen (35), Ekarlatte (81) : boss -> salle de fin ; Portier 800 (« Sortir du donjon. », reponse
#   DungeonExit deja en base) pose dans la salle de fin ; dungeons_exits : salle de fin -> devant le donjon.
# - Kralamoure (31) : pas de salle de fin dans les donnees -> boss -> devant le donjon directement.
# Utilisation : py -I generer_sorties_astrub.py <cellules.tsv (maps_cellules.py)> <sortie.sql> <annulation.sql>
import math, subprocess, sys

CELLS, OUT, UNDO = sys.argv[1:4]
MYSQL = r"C:\Users\Beny\Desktop\DOFUS NEKFEU JTE\_OUTILS\MariaDB\bin\mysql.exe"
ID0 = 900300   # npcs_spawns ajoutes


def q(sql):
    r = subprocess.run([MYSQL, "-uroot", "world", "-B", "-N", "-e", sql], capture_output=True, text=True, encoding="utf-8")
    if r.returncode:
        raise SystemExit(r.stderr)
    return [l.split("\t") for l in r.stdout.splitlines() if l]


cells = {}
for line in open(CELLS, encoding="utf-8"):
    p = line.strip().split("\t")
    if len(p) == 2:
        cells[int(p[0])] = [int(c) for c in p[1].split(",") if c]


def xy(c):
    row = c // 14
    return (c % 14) * 2 + (row & 1), row


def dist(a, b):
    (x1, y1), (x2, y2) = xy(a), xy(b)
    return math.hypot(x1 - x2, y1 - y2)


def busy(m):
    b = {int(r[0]) for r in q("SELECT CellId FROM npcs_spawns WHERE MapId = %d" % m)}
    b |= {int(r[0]) for r in q("SELECT CellId FROM interactives_spawns WHERE MapId = %d" % m)}
    b |= {int(r[0]) for r in q("SELECT CellId FROM world_maps_triggers WHERE MapId = %d AND CellId IS NOT NULL" % m)}
    return b


def nearest(m, anchor, exclude=(), min_d=0.0):
    b = busy(m) | set(exclude)
    cand = [c for c in cells[m] if c not in b and dist(c, anchor) >= min_d]
    return min(cand, key=lambda c: dist(c, anchor))


def anchor_of(m):
    npc = q("SELECT CellId FROM npcs_spawns WHERE MapId = %d LIMIT 1" % m)
    return int(npc[0][0]) if npc else 300


if q("SELECT 1 FROM npcs_spawns WHERE Id = %d" % ID0):
    raise SystemExit("deja applique (npcs_spawns %d existe)" % ID0)

# (donjon, nom, spawn du boss, salle de fin ou None, map devant le donjon)
DJ = [(4, "Centre du Labyrinthe du Minotoror", 34473474, 34473476, 34476294),
      (35, "Laboratoire de Brumen Tinctorias", 176951296, 176952320, 177079296),
      (81, "Ring du Capitaine Ekarlatte", 136578048, 136579072, 134221826),
      (31, "Antre du Kralamoure Geant", 26740736, None, 162056)]

sql = ["-- NEKFEU : donjons dont le boss renvoyait a Astrub (191105026). Genere par _OUTILS/donjons/generer_sorties_astrub.py. Serveur World ARRETE."]
undo = ["-- annulation de sorties_astrub.sql"]
nid = ID0
for did, name, boss_map, end, exit_map in DJ:
    spawns = q("SELECT Id, IFNULL(TeleportMapId,'NULL'), IFNULL(TeleportCell,'NULL') FROM monsters_spawns_dungeons WHERE MapId = %d AND TeleportEvent = 1" % boss_map)
    exit_cell = nearest(exit_map, anchor_of(exit_map), min_d=1.0)
    sql.append("-- %d %s" % (did, name))
    if end:
        npc_cell = nearest(end, 300)
        arrival = nearest(end, 300, exclude={npc_cell}, min_d=2.5)
        target, tcell = end, arrival
        sql.append("INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (%d, %d, %d, %d, 1);" % (did, end, exit_map, exit_cell))
        undo.append("DELETE FROM dungeons_exits WHERE EndMapId = %d AND DungeonId = %d;" % (end, did))
        sql.append("INSERT INTO npcs_spawns (Id, NpcId, MapId, CellId, Direction, Npc) VALUES (%d, 800, %d, %d, 1, 'Nekfeu sortie donjon %d');" % (nid, end, npc_cell, did))
        undo.append("DELETE FROM npcs_spawns WHERE Id = %d;" % nid)
        nid += 1
    else:
        target, tcell = exit_map, exit_cell
    for sid, old_map, old_cell in spawns:
        sql.append("UPDATE monsters_spawns_dungeons SET TeleportMapId = %d, TeleportCell = %d WHERE Id = %s; -- avant %s / %s" % (target, tcell, sid, old_map, old_cell))
        undo.append("UPDATE monsters_spawns_dungeons SET TeleportMapId = %s, TeleportCell = %s WHERE Id = %s;" % (old_map, old_cell, sid))
    print(did, name, "| boss spawns", [s[0] for s in spawns], "| arrivee", target, tcell, "| sortie", exit_map, exit_cell)

open(OUT, "w", encoding="utf-8").write("\n".join(sql) + "\n")
open(UNDO, "w", encoding="utf-8").write("\n".join(undo) + "\n")
