# NEKFEU : branche la sortie de donjon sur les vrais PNJ de fin de donjon (voir OFFICIELS)
# Utilisation : py -I generer_sorties_pnj_officiels.py <dossier exports> <sortie.sql> <annulation.sql> <rapport.tsv>
import collections, math, os, sys

X = sys.argv[1]
ID0 = 900001

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
# NEKFEU : PNJ officiels de fin de donjon qui ne faisaient rien (réponse « Sortir » sans action serveur,
# ou dialogue réservé à une autre map). Pour chaque donjon : (salle de fin, PNJ, message de victoire, réponse « Sortir »).
# Le message est mis sur une action Talk propre à la salle de fin (priorité -1, passe avant les dialogues d'entrée).
OFFICIELS = {
    6: (72363008, 784, 3231, 2855), 7: (27789318, 789, 3253, 2893), 9: (149689344, 1619, 11972, 13466),
    10: (149428224, 797, 3278, 2929), 20: (16515331, 710, 2944, 2580), 25: (5244422, 941, 4191, 3669),
    29: (18089988, 708, 2942, 2578), 32: (64754688, 1404, 8507, 8555), 39: (22285314, 942, 4196, 3674),
    40: (17310720, 705, 2938, 2575), 41: (22810626, 945, 4205, 3683), 42: (27001860, 799, 3292, 2963),
    43: (40110087, 800, 3294, 2962), 54: (55054336, 1285, 7695, 7712), 55: (56103936, 1300, 7837, 7902),
    56: (56365058, 1357, 8108, 8216), 57: (57157377, 1358, 8110, 8220), 58: (59515138, 1385, 8988, 8325),
    59: (62920704, 1391, 8428, 8451), 60: (61869060, 1393, 8458, 8478), 61: (62136840, 1394, 8460, 8479),
    62: (57939713, 1363, 8154, 8147), 64: (79435265, 1509, 9947, 10732), 75: (125832707, 2294, 17261, 20656),
    90: (152835072, 2936, 20842, 24994), 100: (174331392, 3646, 26019, 31197), 101: (174071808, 3657, 26234, 31477),
}
ID1 = 900200  # npcs_actions / npcs_replies ajoutés
OUT, UNDO, REPORT = sys.argv[2:5]
sql = ["-- NEKFEU : sorties par les vrais PNJ de fin de donjon (généré par generer_sorties_pnj_officiels.py), après sorties_donjons.sql"]
undo = ["-- annule sorties_pnj_officiels.sql"]
report = ["donjon\tnom\tsalle de fin\tpnj\tmessage\treponse\tmap sortie\tcase sortie"]
nid = ID1
for did, (end, npc, msg, reply) in sorted(OFFICIELS.items()):
    c = client[did]
    exit_map = c["exit"] if c["exit"] in maps else c["entrance"]
    dj_maps = set(c["maps"])
    anchors = [cc for cc, t, p0 in triggers.get(exit_map, []) if p0.isdigit() and int(p0) in dj_maps and cc >= 0]
    anchors += npc_on_map.get(exit_map, [])
    anchor = anchors[0] if anchors else CENTER
    exit_cell = nearest(exit_map, anchor, min_d=1.0) if exit_map in cells else -1
    if msg not in messages:
        raise SystemExit("message %d absent" % msg)
    sql.append("-- %d %s : PNJ %d" % (did, names.get(c["nameId"], "?"), npc))
    sql.append("INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES (%d, %d, %d, %d, 1);" % (did, end, exit_map, exit_cell))
    undo.append("DELETE FROM dungeons_exits WHERE DungeonId = %d AND EndMapId = %d;" % (did, end))
    sql.append("INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES (%d, %d, 'Talk', 'Mp=%d', -1, '%d');" % (nid, npc, end, msg))
    undo.append("DELETE FROM npcs_actions WHERE Id = %d;" % nid); nid += 1
    sql.append("INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) SELECT %d, 'DungeonExit', %d, %d FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM npcs_replies WHERE Type = 'DungeonExit' AND ReplyId = %d AND MessageId = %d);" % (nid, reply, msg, reply, msg))
    undo.append("DELETE FROM npcs_replies WHERE Id = %d;" % nid); nid += 1
    report.append("%d\t%s\t%d\t%d\t%d\t%d\t%d\t%d" % (did, names.get(c["nameId"], "?"), end, npc, msg, reply, exit_map, exit_cell))
for p, lines in ((OUT, sql), (UNDO, undo), (REPORT, report)):
    open(p, "w", encoding="utf-8", newline="\n").write("\n".join(lines) + "\n")
print(len(OFFICIELS), "donjons")
