# Audit des donjons : compare la liste du client (Dungeons.d2o) avec la base du serveur
# (dungeons, monsters_spawns_dungeons, world_maps, déclencheurs, PNJ, éléments interactifs)
# et repère les salles de fin manquantes ou cassées.
#
# Règles :
# - salle de fin (côté client) = map de la liste du donjon SANS groupe de monstres en base ;
# - elle est « reliée » si une téléportation d'après-combat d'une salle du donjon y mène ;
# - elle a une « sortie » si elle a un déclencheur de téléportation, un élément de téléportation ou un PNJ ;
# - salle du boss = salle avec monstres dont la téléportation sort du donjon (ou ne mène nulle part).
#
# Utilisation : py -I audit_donjons.py <dossier des exports .tsv> <rapport .tsv>
import collections, os, sys

D = sys.argv[1]


def rows(name):
    with open(os.path.join(D, name), encoding="utf-8") as f:
        for line in f:
            line = line.rstrip("\n")
            if line:
                yield line.split("\t")


names = {int(r[0]): r[1] for r in rows("names.tsv")}
maps = {int(r[0]) for r in rows("maps.tsv")}
db = {int(r[0]): r for r in rows("dungeons.tsv")}

spawns = collections.defaultdict(list)                  # map -> [(id, tpEvent, tpMap, tpCell)]
for r in rows("spawns.tsv"):
    spawns[int(r[1])].append((int(r[0]), int(r[2]), int(r[3]), int(r[4])))

exits = collections.defaultdict(list)
for r in rows("triggers.tsv"):
    if r[1] in ("Teleport", "HouseExit"):
        exits[int(r[0])].append("declencheur")
for r in rows("iskills.tsv"):
    if r[1] in ("Teleport", "ZaapTeleport", "ZaapiTeleport"):
        exits[int(r[0])].append("element")
for r in rows("npcs.tsv"):
    exits[int(r[0])].append("pnj")

report = []
for r in rows("client.tsv"):
    did, name_id = int(r[0]), int(r[1])
    cmaps = [int(x) for x in r[2].split(",") if x]
    exit_ = int(r[4])
    name = names.get(name_id, "?")
    cset = set(cmaps)
    problems = []

    if did not in db:
        problems.append("donjon absent de la table dungeons")
    missing = [m for m in cmaps if m not in maps]
    if missing:
        problems.append("maps absentes du serveur : " + ",".join(map(str, missing)))

    rooms = [m for m in cmaps if spawns.get(m)]
    ends = [m for m in cmaps if not spawns.get(m) and m in maps]
    targets = {tp for m in rooms for sid, ev, tp, cell in spawns[m] if ev and tp}

    if not rooms:
        problems.append("aucune salle avec monstres en base")

    # salles boss : teleportation hors du donjon, ou aucune
    boss = []
    for m in rooms:
        tps = [(ev, tp) for sid, ev, tp, cell in spawns[m]]
        if all(not ev or not tp or tp not in cset for ev, tp in tps):
            boss.append((m, tps))
    for m, tps in boss:
        if all(not ev or not tp for ev, tp in tps):
            problems.append("salle %d : aucune teleportation apres le combat" % m)
        for ev, tp in tps:
            if ev and tp and tp not in maps:
                problems.append("salle %d : teleporte vers une map inexistante %d" % (m, tp))

    for e in ends:
        if e not in targets:
            problems.append("salle de fin %d jamais atteinte (aucune salle n'y teleporte)" % e)
        if not exits.get(e):
            problems.append("salle de fin %d sans sortie" % e)

    report.append((did, name, len(cmaps), len(rooms), ",".join(map(str, ends)) or "-",
                   ";".join("%d->%s" % (m, ",".join(str(tp) for ev, tp in t if ev)) for m, t in boss) or "-",
                   " | ".join(problems) if problems else "OK"))

with open(sys.argv[2], "w", encoding="utf-8") as o:
    o.write("id\tnom\tmaps client\tsalles avec monstres\tsalles de fin (client)\tsalle(s) boss -> destination\tdiagnostic\n")
    for x in report:
        o.write("\t".join(map(str, x)) + "\n")

ok = sum(1 for x in report if x[6] == "OK")
print("donjons :", len(report), "| OK :", ok, "| avec probleme :", len(report) - ok)
kinds = collections.Counter()
for x in report:
    for p in x[6].split(" | "):
        if p != "OK":
            k = p
            for word in ("salle de fin ", "salle "):
                if k.startswith(word):
                    k = word + "X" + k[len(word):].split(" ", 1)[1][0:0] + " " + k[len(word):].split(" ", 1)[1] if " " in k[len(word):] else k
            kinds[k.split(" : ")[0]] += 1
for k, v in kinds.most_common():
    print("  ", v, k)
