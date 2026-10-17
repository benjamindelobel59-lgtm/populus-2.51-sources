# NEKFEU : remplit les salles de donjon sans monstre (monsters_spawns_dungeons sans ligne dans monsters_spawns_dungeons_groups)
# sur le modèle des donjons remplis : salle n -> monstres grade n (max 5), 4 monstres pour 1-4 joueurs puis 5/6/7/8 ;
# salle du boss : boss grade 1..5 selon le nombre de joueurs + monstres grade 5.
# Monstres : sous-zone du donjon (SubAreas.d2o), corrigés à la main (POOL / BOSS ci-dessous).
# Utilisation : py -I remplir_donjons_vides.py -> remplir_donjons_vides.sql, -annuler.sql, _rapport.tsv
import collections, os, sys
ICI = os.path.dirname(os.path.abspath(__file__)); sys.path.insert(0, ICI)
import d2o, d2i
RACINE = os.path.dirname(os.path.dirname(ICI)); C = os.path.join(RACINE, "CLIENT_64bits", "app", "data")
X = os.path.join(ICI, "exports")
TAG = "NEKFEU remplissage"
_, sub = d2o.load(os.path.join(C, "common", "SubAreas.d2o"))
_, mon = d2o.load(os.path.join(C, "common", "Monsters.d2o"))
_, dun = d2o.load(os.path.join(C, "common", "Dungeons.d2o"))
txt = d2i.load(os.path.join(C, "i18n", "i18n_fr.d2i"))
nom = lambda m: txt.get(mon[m]["nameId"], str(m))
pos = {int(l.split()[0]): int(l.split()[1]) for l in open(os.path.join(X, "maps_souszones.tsv"))}
grades = collections.defaultdict(dict)
for l in open(os.path.join(X, "monsters_grades.tsv")):
    m, g, i, lv = map(int, l.split()); grades[m][g] = i
tous = [tuple(map(int, l.split())) for l in open(os.path.join(X, "spawns_donjons_tous.tsv"))]
vides = [tuple(map(int, l.split())) for l in open(os.path.join(X, "spawns_vides.tsv"))]

# corrections à la main
POOL = {20: [527, 528, 529],                                   # Firefoux : pas les monstres de l'île
        46: [673, 674, 675, 676, 677, 681],                    # Skeunk : gemmes et poupées, pas les Koalaks
        94: [4296, 4297, 4298, 4299, 4300],                    # Ush : sans le double d'Ush Galesh
        96: [3516, 3517, 3518, 3519]}                          # Araknas : Shushus (marqués « quête » dans le client)
BOSS = {44: 943, 60: 2986, 71: 3416, 96: 1195}                 # Sphincter Cell, Kolosso, Comte Harebourg, Qu'Tan
BOSS_SALLE = {300: 2967, 301: 3234}                            # Givrefoux : Tengu / Fuji
PAS_BOSS = {298}                                               # Givrefoux 4-5 -> salle du PNJ, pas de boss

map2dj = {m: d["id"] for d in dun.values() for m in d["mapIds"]}
spawn_maps = {m for _, m, _ in tous}
pred = {tp: m for _, m, tp in tous if tp}
def rang(m):  # numéro de salle : 1 + nombre de salles avant
    n, seen = 1, set()
    while m in pred and pred[m] not in seen:
        seen.add(m); m = pred[m]; n += 1
    return n

def ok(m): return m in mon and m in grades and not mon[m]["isMiniBoss"]
sql = ["-- NEKFEU : remplissage des salles de donjon sans monstre (généré par remplir_donjons_vides.py)"]
und = ["DELETE FROM monsters_spawns_dungeons_groups WHERE Informations = '%s';" % TAG]
rap = ["spawn\tmap\tdonjon\tsalle\tboss\tcomposition 1-4 joueurs"]
rows = 0
for sid, mid, tp in sorted(vides):
    did = map2dj.get(mid)
    d = dun[did]
    subs = {pos.get(m) for m in d["mapIds"]} - {None}
    pool = set(); [pool.update(sub[s]["monsters"]) for s in subs if s in sub]
    norm = POOL.get(did) or sorted(m for m in pool if ok(m) and not mon[m]["isBoss"] and not mon[m]["isQuestMonster"])
    bosses = [m for m in pool if ok(m) and mon[m]["isBoss"]]
    is_boss = sid not in PAS_BOSS and (sid in BOSS_SALLE or (tp not in spawn_maps))
    boss = BOSS_SALLE.get(sid) or BOSS.get(did) or (bosses[0] if len(bosses) == 1 else None)
    if is_boss and boss is None:
        raise SystemExit("boss inconnu pour le spawn %d (donjon %d) : %s" % (sid, did, bosses))
    r = rang(mid); g = min(r, 5); n = len(norm)
    comp = {}
    for size in (4, 5, 6, 7, 8):
        if is_boss:
            comp[size] = [(boss, (4, 5, 6, 7, 8).index(size) + 1)] + [(norm[(r + i) % n], 5) for i in range(size - 1)]
        else:
            comp[size] = [(norm[(r + i) % n], g) for i in range(size)]
    for size, ms in comp.items():
        for m, gr in ms:
            gr = min(gr, max(grades[m]))
            sql.append("INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (%d, %d, %d, '%s');"
                       % (sid, grades[m][gr], 1 if size == 4 else size, TAG))
            rows += 1
    rap.append("%d\t%d\t%s\t%d\t%s\t%s" % (sid, mid, txt.get(d["nameId"]), r, nom(boss) if is_boss else "", ", ".join("%s g%d" % (nom(m), gr) for m, gr in comp[4])))
for name, lines in (("remplir_donjons_vides.sql", sql), ("remplir_donjons_vides-annuler.sql", und), ("remplir_donjons_vides_rapport.tsv", rap)):
    open(os.path.join(ICI, name), "w", encoding="utf-8", newline="\n").write("\n".join(lines) + "\n")
print(len(vides), "salles,", rows, "lignes")
