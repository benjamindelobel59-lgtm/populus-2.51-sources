# Associe a chaque entree de donjon (dungeons_entries) son vrai gardien du client.
import os, re, subprocess, sys, unicodedata, collections
D = r"C:\Users\Beny\Desktop\DOFUS NEKFEU JTE\_OUTILS\donjons"
sys.path.insert(0, D)
import d2o, d2i
MYSQL = r"C:\Users\Beny\Desktop\DOFUS NEKFEU JTE\_OUTILS\MariaDB\bin\mysql.exe"
def q(sql):
    r = subprocess.run([MYSQL, "-uroot", "world", "-B", "-N", "-e", sql], capture_output=True, text=True, encoding="utf-8")
    return [l.split("\t") for l in r.stdout.splitlines() if l]
def norm(s):
    s = unicodedata.normalize("NFD", (s or "").lower())
    return re.sub(r"[^a-z0-9' ]+", " ", "".join(c for c in s if unicodedata.category(c) != "Mn"))
txt = d2i.load(r"C:\Users\Beny\Desktop\DOFUS NEKFEU JTE\CLIENT_64bits\app\data\i18n\i18n_fr.d2i")
_, npcs = d2o.load(os.path.join(D, "client", "Npcs.original.d2o"))
entry_re = re.compile(r"trousseau|donner la clef|utiliser la clef|entrer|p[ée]n[ée]trer|accepter et utiliser", re.I)
keepers = {}
for nid, o in npcs.items():
    reps = [(r, txt.get(t, "")) for r, t in o["dialogReplies"] if entry_re.search(txt.get(t, "") or "")]
    if reps:
        msgs = " ".join(txt.get(t, "") for _, t in o["dialogMessages"])
        keepers[nid] = (txt.get(o["nameId"], "?"), reps, norm(msgs + " " + " ".join(s for _, s in reps)), o["look"])
# donjons du client (nom francais, entree)
client = {}
for l in open(os.path.join(D, "exports", "client.tsv"), encoding="utf-8"):
    p = l.rstrip("\n").split("\t")
    client[int(p[0])] = (int(p[1]), int(p[3]))
names = {int(p[0]): p[1] for p in (l.rstrip("\n").split("\t") for l in open(os.path.join(D, "exports", "names.tsv"), encoding="utf-8")) if len(p) > 1}
fr_by_entrance = {ent: names.get(nid, "?") for cid, (nid, ent) in client.items()}
STOP = set("le la les du de des d l un une donjon salle antre repaire grotte caverne mazmorra et au aux en".split())
entries = q("SELECT e.Id, e.DungeonId, e.EntranceMapId, e.KeeperCellId FROM dungeons_entries e ORDER BY e.DungeonId")
spawned = collections.defaultdict(list)
for mid, nid, cell in q("SELECT MapId, NpcId, CellId FROM npcs_spawns"):
    spawned[int(mid)].append((int(nid), int(cell)))
out = open(sys.argv[1], "w", encoding="utf-8")
for eid, did, ent, kcell in entries:
    ent = int(ent)
    fr = fr_by_entrance.get(ent, "?")
    # 1) un gardien du client deja pose sur la map d'entree
    here = [n for n, c in spawned[ent] if n in keepers and n != 707]
    how, best = "", None
    if here:
        best, how = here[0], "deja sur la map"
    else:
        words = [w for w in norm(fr).split() if w not in STOP and len(w) > 3]
        scored = []
        for nid, (name, reps, text, look) in keepers.items():
            if nid == 707:
                continue
            s = sum(1 for w in words if re.search(r"\b" + re.escape(w), text))
            if s:
                scored.append((s, nid))
        scored.sort(reverse=True)
        if scored and (len(scored) == 1 or scored[0][0] > scored[1][0]):
            best, how = scored[0][1], "texte (%d mots : %s)" % (scored[0][0], ",".join(words))
        elif scored:
            how = "ambigu : " + ", ".join("%d %s(%d)" % (n, keepers[n][0], s) for s, n in scored[:4])
    if best:
        name, reps, _, look = keepers[best]
        out.write("%s\t%s\t%d\t%s\t%d\t%s\t%s\t%s\n" % (did, fr, ent, kcell, best, name, " | ".join("%d:%s" % r for r in reps), how))
    else:
        out.write("%s\t%s\t%d\t%s\t-\t-\t-\t%s\n" % (did, fr, ent, kcell, how or "aucun"))
print("ok")
