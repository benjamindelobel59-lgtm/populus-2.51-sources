# MYSTIC 17/10 : exporte salles de donjon (spawns + noms de carte FR) et monstres (noms FR + grades) pour les groupes officiels.
import sys, os, subprocess, csv
B = r"C:\Users\Beny\Desktop\DOFUS NEKFEU JTE"
sys.path.insert(0, os.path.join(B, r"_OUTILS\serveur\donjons"))
import d2o, d2i
C = os.path.join(B, r"CLIENT_64bits\app\data")
T = d2i.load(os.path.join(C, r"i18n\i18n_fr.d2i"))
_, mp = d2o.load(os.path.join(C, r"common\MapPositions.d2o"))
_, mons = d2o.load(os.path.join(C, r"common\Monsters.d2o"))
_, dj = d2o.load(os.path.join(C, r"common\Dungeons.d2o"))
OUT = os.path.join(B, r"_OUTILS\serveur\donjons\groupes_officiels")
def q(sql):
    r = subprocess.run([os.path.join(B, r"_OUTILS\logiciels\MariaDB\bin\mysql.exe"), "-uroot", "-h127.0.0.1", "world", "--default-character-set=utf8mb4", "-N", "-B", "-e", sql], capture_output=True, text=True, encoding="utf-8", errors="replace")
    return [l.split("\t") for l in r.stdout.splitlines()]
map2dj = {}
for k, o in dj.items():
    for m in o.get("mapIds", []): map2dj[m] = T.get(o["nameId"], "?")
with open(os.path.join(OUT, "salles.tsv"), "w", encoding="utf-8", newline="") as f:
    w = csv.writer(f, delimiter="\t")
    w.writerow(["spawn", "map", "donjon_d2o", "nom_carte", "descr", "nb_groupes"])
    cnt = {r[0]: r[1] for r in q("select DungeonSpawnId,count(*) from monsters_spawns_dungeons_groups group by DungeonSpawnId")}
    for sid, mid, desc in q("select Id,MapId,Descripcion from monsters_spawns_dungeons order by Id"):
        m = mp.get(int(mid)) or {}
        w.writerow([sid, mid, map2dj.get(int(mid), ""), T.get(m.get("nameId", -1), ""), desc, cnt.get(sid, "0")])
grades = {}
for gid, g, mid, lvl in q("select Id,GradeId,MonsterId,Level from monsters_grades"):
    grades.setdefault(int(mid), []).append((int(g), int(lvl), int(gid)))
with open(os.path.join(OUT, "monstres.tsv"), "w", encoding="utf-8", newline="") as f:
    w = csv.writer(f, delimiter="\t")
    w.writerow(["monstre", "nom", "boss", "grades(grade:niveau:id)"])
    for k, o in sorted(mons.items()):
        gs = sorted(grades.get(k, []))
        w.writerow([k, T.get(o["nameId"], ""), int(bool(o.get("isBoss"))), " ".join(f"{a}:{b}:{c}" for a, b, c in gs)])
print("ok", len(mons))
