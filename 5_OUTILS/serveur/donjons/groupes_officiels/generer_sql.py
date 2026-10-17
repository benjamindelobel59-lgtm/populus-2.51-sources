# MYSTIC 17/10 : genere le SQL des groupes officiels (tofus.fr) a partir de releves.txt.
# Ligne : slug | spawnId | nom de salle (capture) | orange1, orange2, ... / gris1, gris2, ...   (chaque monstre = "Nom niveau")
# Usage : generer_sql.py <lot> slug1 slug2 ...   -> <lot>.sql + verification a l'ecran
import sys, os, csv, re, unicodedata, subprocess
sys.stdout.reconfigure(encoding="utf-8")
D = os.path.dirname(os.path.abspath(__file__))
B = os.path.abspath(os.path.join(D, r"..\..\..\.."))
def norm(s):
    s = unicodedata.normalize("NFD", s.lower().replace("œ", "oe"))
    return re.sub(r"[^a-z0-9]", "", "".join(c for c in s if unicodedata.category(c) != "Mn"))
mons = {}; byname = {}
for r in csv.DictReader(open(os.path.join(D, "monstres.tsv"), encoding="utf-8"), delimiter="\t"):
    gs = [tuple(map(int, g.split(":"))) for g in r["grades(grade:niveau:id)"].split()]
    m = int(r["monstre"]); mons[m] = (r["nom"], int(r["boss"]), gs)
    byname.setdefault(norm(r["nom"]), []).append(m)
salles = {r["spawn"]: r for r in csv.DictReader(open(os.path.join(D, "salles.tsv"), encoding="utf-8"), delimiter="\t")}
def q(sql):
    r = subprocess.run([os.path.join(B, r"_OUTILS\logiciels\MariaDB\bin\mysql.exe"), "-uroot", "-h127.0.0.1", "world", "--default-character-set=utf8mb4", "-N", "-B", "-e", sql], capture_output=True, text=True, encoding="utf-8", errors="replace")
    return [l.split("\t") for l in r.stdout.splitlines()]
lot, slugs = sys.argv[1], sys.argv[2:]
lines = [l.rstrip("\n") for l in open(os.path.join(D, "releves.txt"), encoding="utf-8") if l.strip() and not l.startswith("#")]
rows = [[x.strip() for x in l.split("|")] for l in lines]
rows = [r for r in rows if r[0] in slugs]
byroom = {}
for k, v in salles.items(): byroom.setdefault(norm(v["nom_carte"]), []).append(k)
for r in rows:
    if r[1] == "?":
        c = byroom.get(norm(r[2]), [])
        if len(c) == 1: r[1] = c[0]
        else: print("!! salle introuvable/ambigue en base :", r[2], c)
rows = [r for r in rows if r[1] != "?"]
spawns = [r[1] for r in rows]
# monstres deja presents dans les spawns (pour departager les homonymes)
cur = {}
if spawns:
    for sid, mid in q("select g.DungeonSpawnId, mg.MonsterId from monsters_spawns_dungeons_groups g join monsters_grades mg on mg.Id=g.MonsterGradeId where g.DungeonSpawnId in (%s)" % ",".join(spawns)):
        cur.setdefault(sid, set()).add(int(mid))
allcur = set().union(*cur.values()) if cur else set()
erreurs = []; out = []
complements = []
def parse(txt):
    m = re.match(r"^(.*?)\s*\(?(\d+)\)?$", txt.strip())
    return m.group(1).strip(), int(m.group(2))
def pick(sid, txt):
    """monstre retenu pour une ligne : (id, niveau, boss, exact) ou None"""
    name, lvl = parse(txt)
    forced = re.match(r"^#(\d+)\s+(.*)$", name)
    cands = [int(forced.group(1))] if forced else byname.get(norm(name), [])
    cands = [c for c in cands if mons[c][2]]
    if not cands: erreurs.append(f"[{sid}] monstre inconnu : {txt}"); return None
    withlvl = [c for c in cands if any(g[1] == lvl for g in mons[c][2])]
    bosses = [c for c in cands if mons[c][1]]
    pool = withlvl or bosses or cands
    if len(pool) > 1:
        p2 = [c for c in pool if c in cur.get(sid, set())] or [c for c in pool if c in allcur]
        if p2: pool = p2
    if len(pool) > 1 and not withlvl and not bosses:  # le plus proche en niveau
        pool.sort(key=lambda c: min(abs(g[1] - lvl) for g in mons[c][2]))
    c = pool[0]
    return (c, lvl, bool(mons[c][1]), bool(withlvl))
# 1er passage : niveaux vus par monstre et par donjon (regle « position relative »)
vus = {}
for slug, sid, nom, comp in rows:
    if comp.strip() == "-": continue
    for x in [x for part in comp.split("/") for x in part.split(",") if x.strip()]:
        n, l = parse(x)
        cands = byname.get(norm(n), [])
        for c in cands: vus.setdefault((slug, c), set()).add(l)
relatif = []
def grade_id(slug, sid, c, lvl, isboss, exact, k):
    gs = sorted(mons[c][2])
    if isboss: return next(g[2] for g in gs if g[0] == min(k + 1, max(x[0] for x in gs)))
    if exact: return next(g[2] for g in gs if g[1] == lvl)
    mn = min(vus[(slug, c)]); step = (gs[1][1] - gs[0][1]) if len(gs) > 1 and gs[1][1] > gs[0][1] else 1
    gr = max(1, min(5, len(gs), 1 + int((lvl - mn) / step + 0.5)))
    relatif.append(f"[{sid}] {mons[c][0]} {lvl} -> grade {gr} ({gs[gr-1][1]} en 2.51)")
    return gs[gr - 1][2]
sql_ins = []
ok_spawns = set()
for slug, sid, nom, comp in rows:
    s = salles.get(sid)
    if not s: erreurs.append(f"[{sid}] spawn absent"); continue
    if norm(s["nom_carte"]) != norm(nom): erreurs.append(f"[{sid}] nom carte '{s['nom_carte']}' != capture '{nom}'")
    if comp.strip() == "-": continue
    parts = comp.split("/")
    orange = [x for x in parts[0].split(",") if x.strip()]
    gris = [x for x in parts[1].split(",") if x.strip()] if len(parts) > 1 else []
    if len(orange) > 4 and gris: orange, gris = orange[:4], orange[4:] + gris
    O = [pick(sid, x) for x in orange]; G = [pick(sid, x) for x in gris]
    if None in O or None in G: continue
    info = f"MYSTIC officiel {slug} {nom}".replace("'", "''")
    base = [(c, l, b, e, grade_id(slug, sid, c, l, b, e, 0)) for c, l, b, e in O]
    extra = [(c, l, b, e, grade_id(slug, sid, c, l, b, e, 0)) for c, l, b, e in G]
    # sans liste grise : complement jusqu'a 8 en reprenant la base dans l'ordre, en boucle, sans boss (consigne 17/10)
    if not gris:
        loop = [m for m in base if not m[2]]
        need = max(0, 8 - len(base))
        extra = [loop[i % len(loop)] for i in range(need)] if loop else []
        if extra: complements.append(f"[{sid}] {nom} : base {len(base)} -> complétée jusqu'à {len(base) + len(extra)}")
    for k, tier in enumerate([1, 5, 6, 7, 8]):
        n = (len(base) if tier == 1 else max(len(base), tier)) if not gris else len(base) + k
        compo = base + extra[:max(0, n - len(base))]
        for (c, l, b, e, gid) in compo:
            if b: gid = grade_id(slug, sid, c, l, b, e, k)
            sql_ins.append(f" ({sid}, {gid}, {tier}, '{info}')")
    ok_spawns.add(sid)
    out.append(f"[{sid}] {nom} : " + ", ".join(f"{mons[c][0]}#{c}({l}){'B' if b else ''}" for c, l, b, e in O) + " / " + ", ".join(f"{mons[c][0]}#{c}({l})" for c, l, b, e in G))
done = sorted(ok_spawns, key=int)
# spawns interdits : Vortex (397), Givrefoux/Korriandre deja faits, donjons a vagues des dimensions
INTERDITS = {"397", "295", "296", "297", "298", "300", "301", "328", "329", "330", "331", "332"} | {r[0] for r in q("select distinct DungeonSpawnId from monsters_spawns_dungeons_waves")}
bad = [d for d in done if d in INTERDITS]
if bad: sys.exit("ARRET : spawns interdits dans le lot : %s" % bad)
with open(os.path.join(D, "groupes_officiels_" + lot + ".sql"), "w", encoding="utf-8") as f:
    f.write(f"-- MYSTIC 17/10 : groupes de monstres officiels (source tofus.fr/donjons, captures des salles : orange = 4 joueurs, gris = 5 a 8 joueurs).\n")
    f.write(f"-- Lot {lot} : {', '.join(slugs)}. Genere par generer_sql.py depuis releves.txt. Rejouable.\n")
    f.write(f"DELETE FROM monsters_spawns_dungeons_groups WHERE DungeonSpawnId IN ({','.join(done)});\n")
    f.write("INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES\n")
    f.write(",\n".join(sql_ins) + ";\n")
print("\n".join(out)); print("--- %d lignes" % len(sql_ins))
print("\n".join("!! " + e for e in erreurs))
relatif = list(dict.fromkeys(relatif))
print("\n".join("~ relatif " + e for e in relatif))
print("\n".join("+ " + e for e in complements))
open(os.path.join(D, "notes_" + lot + ".txt"), "w", encoding="utf-8").write("\n".join(["# grades par position relative (niveau tofus absent des grades 2.51)"] + relatif + ["# salles sans liste grise completees jusqu'a 8"] + complements + ["# erreurs / remarques"] + erreurs) + "\n")
