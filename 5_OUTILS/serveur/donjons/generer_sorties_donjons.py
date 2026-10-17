# Génère le SQL des sorties de donjon (salles de fin sans sortie) :
#  - table dungeons_exits (salle de fin -> devant le donjon), lue par la réponse PNJ « DungeonExit » (DungeonExitReply.cs)
#  - un PNJ par salle de fin (vrai PNJ du donjon si connu, sinon le Portier 800) + dialogue + réponse DungeonExit
#  - case d'arrivée après le boss quand elle manque ; Canopée du Kimbo : boss -> sa vraie salle de fin 21498882
#  - les 8 donjons absents de la table dungeons
# Utilisation : py -I generer_sorties_donjons.py <dossier exports> <sortie.sql> <annulation.sql> <rapport.tsv>
import collections, math, os, sys

X, OUT, UNDO, REPORT = sys.argv[1:5]
ID0 = 900001  # identifiants des lignes ajoutées (npcs_spawns, npcs_actions, npcs_replies)


def rows(name):
    with open(os.path.join(X, name), encoding="utf-8") as f:
        for line in f:
            line = line.rstrip("\n")
            if line:
                yield line.split("\t")


# --- données
client = {int(r[0]): {"nameId": int(r[1]), "maps": [int(m) for m in r[2].split(",") if m], "entrance": int(r[3]), "exit": int(r[4])} for r in rows("client.tsv")}
for r in rows("client_niveaux.tsv"):
    if int(r[0]) in client:
        client[int(r[0])]["level"] = int(r[1])
names = {int(r[0]): r[1] for r in rows("names.tsv")}
maps = {int(r[0]) for r in rows("maps.tsv")}
cells = {int(r[0]): [int(c) for c in r[1].split(",") if c] for r in rows("cellules.tsv") if len(r) > 1}
fins = [r for r in list(rows("fins_de_donjon.tsv"))[1:]]
spawns = [dict(id=int(r[0]), map=int(r[1]), cell=int(r[2]), ev=int(r[3]), tp=int(r[4]), tpcell=int(r[5]), tpdir=int(r[6])) for r in rows("spawns_complet.tsv")]
triggers = collections.defaultdict(list)
for r in rows("triggers_complet.tsv"):
    triggers[int(r[0])].append((int(r[1]) if r[1] not in ("", "NULL") else -1, r[2], r[3]))
busy = collections.defaultdict(set)
for f in ("npcs_cells.tsv", "interactifs_cells.tsv"):
    for r in rows(f):
        busy[int(r[0])].add(int(r[1]))
for r in rows("elements_maps.tsv"):
    busy[int(r[0])].add(int(r[3]))
npc_on_map = collections.defaultdict(list)
for r in rows("npcs_cells.tsv"):
    npc_on_map[int(r[0])].append(int(r[1]))
templates = {int(r[0]): {"name": r[1], "msgs": [int(x.split(",")[0]) for x in r[2].split(";") if x]} for r in rows("pnj_templates.tsv")}
talk = collections.defaultdict(list)
for r in rows("pnj_talk.tsv"):
    talk[int(r[0])].append((r[2], int(r[3])))
messages = {int(r[0]) for r in rows("messages_ids.tsv")}

# PNJ de sortie : (pnj, réponse « Sortir »)
REAL = {13: (712, 14834), 14: (712, 14834), 17: (137, 18856), 52: (2151, 18921), 27: (1384, 8307),
        36: (795, 8720), 37: (795, 8720), 38: (795, 8720), 53: (783, 2835), 79: (2352, 21438), 105: (3994, 33360)}
GENERIC = (800, 2962)  # Portier du Donjon des Rats de Brâkmar : « Sortir du donjon. »
KIMBO = (28, 282, 21498882)  # donjon, spawn du boss, vraie salle de fin


def xy(c):
    row = c // 14
    return (c % 14) * 2 + (row & 1), row


def dist(a, b):
    (x1, y1), (x2, y2) = xy(a), xy(b)
    return math.hypot(x1 - x2, y1 - y2)


def free_cells(m):
    trig = {c for c, t, p0 in triggers.get(m, [])}
    return [c for c in cells.get(m, []) if c not in busy[m] and c not in trig]


def nearest(m, anchor, exclude=(), min_d=0.0):
    cand = [c for c in free_cells(m) if c not in exclude and dist(c, anchor) >= min_d]
    return min(cand, key=lambda c: dist(c, anchor)) if cand else -1


CENTER = 300
sql, undo, report = [], [], []
sql.append("-- Nekfeu : sorties des donjons (salles de fin sans sortie) — généré par _OUTILS/donjons/generer_sorties_donjons.py")
sql.append("CREATE TABLE IF NOT EXISTS dungeons_exits (Id INT NOT NULL AUTO_INCREMENT PRIMARY KEY, DungeonId INT NOT NULL, EndMapId INT NOT NULL, ExitMapId INT NOT NULL, ExitCellId INT NOT NULL, ExitDirection INT NOT NULL DEFAULT 1, KEY (EndMapId)) ENGINE=MyISAM DEFAULT CHARSET=utf8;")
undo.append("-- Annule le script des sorties de donjon")
undo.append("DROP TABLE IF EXISTS dungeons_exits;")

ends = []  # (donjon, salle de fin)
for r in fins:
    if r[4] == "AUCUNE SORTIE":
        did, end = int(r[0]), int(r[2])
        if did == KIMBO[0]:
            end = KIMBO[2]
        ends.append((did, end))

nid = ID0
used_msgs = set()
spawn_updates = []
for did, end in ends:
    c = client[did]
    name = names.get(c["nameId"], "?")
    npc, reply = REAL.get(did, GENERIC)
    # case du PNJ : près du centre de la salle de fin
    npc_cell = nearest(end, CENTER)
    # cases d'arrivée après le boss : spawns qui téléportent vers cette salle (Kimbo : spawn corrigé)
    arrival = nearest(end, CENTER, exclude={npc_cell}, min_d=2.5)
    for s in spawns:
        target = KIMBO[2] if (did == KIMBO[0] and s["id"] == KIMBO[1]) else s["tp"]
        if s["ev"] and target == end:
            changed = []
            if did == KIMBO[0] and s["id"] == KIMBO[1]:
                changed.append("TeleportMapId = %d" % end)
            if s["tpcell"] < 0 or s["tpcell"] not in cells.get(end, []):
                changed.append("TeleportCell = %d" % arrival)
            if changed:
                spawn_updates.append((s, changed))
    # destination « devant le donjon » : map de sortie officielle, près de l'entrée (déclencheur vers le donjon ou PNJ gardien)
    exit_map = c["exit"] if c["exit"] in maps else c["entrance"]
    dj_maps = set(c["maps"])
    anchors = [cc for cc, t, p0 in triggers.get(exit_map, []) if p0.isdigit() and int(p0) in dj_maps and cc >= 0]
    anchors += npc_on_map.get(exit_map, [])
    anchor = anchors[0] if anchors else CENTER
    exit_cell = nearest(exit_map, anchor, min_d=1.0) if exit_map in cells else -1
    report.append((did, name, end, npc, templates.get(npc, {}).get("name", "?"), npc_cell, arrival, exit_map, exit_cell, "entree" if anchors else "centre"))

    sql.append("-- %d %s : salle de fin %d, PNJ %d" % (did, name, end, npc))
    sql.append("INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (%d, %d, %d, %d, 1);" % (did, end, exit_map, exit_cell))
    sql.append("INSERT INTO npcs_spawns (Id, NpcId, MapId, CellId, Direction, Npc) VALUES (%d, %d, %d, %d, 1, 'Nekfeu sortie donjon %d');" % (nid, npc, end, npc_cell, did))
    undo.append("DELETE FROM npcs_spawns WHERE Id = %d;" % nid)
    nid += 1

    # dialogue : action Talk existante sans condition, sinon une action par salle de fin (condition de map)
    open_talks = [m for cond, m in talk.get(npc, []) if cond == ""]
    if open_talks:
        msg = open_talks[0]
    else:
        msg = templates[npc]["msgs"][0]
        sql.append("INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES (%d, %d, 'Talk', 'Mp=%d', 0, '%d');" % (nid, npc, end, msg))
        undo.append("DELETE FROM npcs_actions WHERE Id = %d;" % nid)
        nid += 1
    if msg not in messages:
        raise SystemExit("message %d du PNJ %d absent de npcs_messages" % (msg, npc))
    if (msg, reply) not in used_msgs:
        used_msgs.add((msg, reply))
        sql.append("INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) VALUES (%d, 'DungeonExit', %d, %d);" % (nid, reply, msg))
        undo.append("DELETE FROM npcs_replies WHERE Id = %d;" % nid)
        nid += 1

sql.append("-- cases d'arrivée après le boss / Canopée du Kimbo")
for s, changed in spawn_updates:
    sql.append("UPDATE monsters_spawns_dungeons SET %s WHERE Id = %d;" % (", ".join(changed), s["id"]))
    undo.append("UPDATE monsters_spawns_dungeons SET TeleportMapId = %s, TeleportCell = %s WHERE Id = %d;" % (s["tp"] or "NULL", s["tpcell"] if s["tpcell"] >= 0 else "NULL", s["id"]))

# 8 donjons absents de la table dungeons
sql.append("-- donjons absents de la table dungeons (d'après Dungeons.d2o)")
spawn_by_map = collections.defaultdict(list)
for s in spawns:
    spawn_by_map[s["map"]].append(s)
for did in sorted(client):
    if did in {int(r[0]) for r in rows("dungeons.tsv")}:
        continue
    c = client[did]
    rooms = [m for m in c["maps"] if spawn_by_map.get(m)]
    if not rooms:
        continue
    targets = {s["tp"] for m in rooms for s in spawn_by_map[m] if s["ev"]}
    first = next((m for m in rooms if m not in targets), rooms[0])
    name = names.get(c["nameId"], "Donjon %d" % did).replace("'", "''")
    sql.append("INSERT INTO dungeons (Id, Name, NameId, MapsIdsCSV, OptimalPlayerLevel, FirstRoom, EntranceMapId, ExitMapId) VALUES (%d, '%s', %d, '%s', %d, %d, %d, %d);"
               % (did, name, c["nameId"], ",".join(map(str, c["maps"])), c.get("level", 0), first, c["entrance"], c["exit"]))
    undo.append("DELETE FROM dungeons WHERE Id = %d;" % did)

open(OUT, "w", encoding="utf-8").write("\n".join(sql) + "\n")
open(UNDO, "w", encoding="utf-8").write("\n".join(undo) + "\n")
with open(REPORT, "w", encoding="utf-8") as o:
    o.write("donjon\tnom\tsalle de fin\tpnj\tnom pnj\tcase pnj\tcase arrivee\tmap sortie\tcase sortie\tcase choisie pres de\n")
    for x in report:
        o.write("\t".join(map(str, x)) + "\n")
print("salles de fin :", len(ends), "| maj spawns :", len(spawn_updates), "| lignes SQL :", len(sql))
bad = [x for x in report if x[5] < 0 or x[6] < 0 or x[8] < 0]
print("cases introuvables :", len(bad))
for x in bad:
    print("  ", x)
