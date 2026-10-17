# NEKFEU : vagues pour les 9 donjons de dimension qui n'en avaient pas (modèle Malléfisk / Pounicheur / Fraktale) :
# 4 vagues x tailles de groupe 1 (1-4 joueurs) / 5 / 6 / 7 / 8, monstres du donjon au grade max, sans boss.
# Trône de la Cour Sombre (spawn 449) : son groupe (monstres d'autres donjons, sans la Reine des Voleurs) est remplacé
# par Reine des Voleurs grade 1→5 + monstres de la Cour Sombre, comme les autres salles de boss.
# Utilisation : py -I ajouter_vagues_dimensions.py -> ajouter_vagues_dimensions.sql, -annuler.sql, _rapport.tsv
import collections, os, sys
ICI = os.path.dirname(os.path.abspath(__file__)); sys.path.insert(0, ICI)
import d2o, d2i
C = os.path.join(os.path.dirname(os.path.dirname(ICI)), "CLIENT_64bits", "app", "data")
_, mon = d2o.load(os.path.join(C, "common", "Monsters.d2o"))
txt = d2i.load(os.path.join(C, "i18n", "i18n_fr.d2i"))
nom = lambda m: txt.get(mon[m]["nameId"], str(m))
grades = collections.defaultdict(dict)
for l in open(os.path.join(ICI, "exports", "monsters_grades.tsv")):
    m, g, i, lv = map(int, l.split()); grades[m][g] = i
TAG = "NEKFEU vagues"
# spawn de la salle du boss : (donjon, boss, monstres)
SALLES = {
    245: ("Ring du Capitaine Ekarlatte", 3753, [3730, 3731, 3732, 3733, 3734]),
    333: ("Cave du Toxoliath", 3752, [3735, 3736, 3737, 3738, 3739]),
    449: ("Trône de la Cour Sombre", 3726, [3746, 3747, 3748, 3749, 3750]),
    390: ("Palais du roi Nidas", 3648, [3653, 3654, 3655, 3656, 3657]),
    277: ("Galerie du Phossile", 3651, [3658, 3659, 3660, 3661, 3662]),
    302: ("Horologium de XLII", 3849, [3840, 3841, 3842, 3843, 3844]),
    397: ("Œil de Vortex", 3835, [3834, 3836, 3837, 3838, 3839]),
    294: ("Plateau de Ush", 4264, [4296, 4297, 4298, 4299, 4300]),
    398: ("Défi du Chalœil", 4263, [4283, 4284, 4285, 4286, 4287]),
}
REMPLACER_GROUPE = {449}
SIZES = (1, 5, 6, 7, 8)
sql = ["-- NEKFEU : vagues des donjons de dimension (généré par ajouter_vagues_dimensions.py)"]
und = ["-- annule ajouter_vagues_dimensions.sql",
       "DELETE FROM monsters_spawns_dungeons_waves WHERE MonsterName = '%s';" % TAG]
rap = ["spawn\tdonjon\tvague\tcomposition 1-4 joueurs"]
for sid, (dj, boss, pool) in SALLES.items():
    n = len(pool)
    top = lambda m: max(g for g in grades[m] if g <= 6)
    if sid in REMPLACER_GROUPE:
        sql.append("DELETE FROM monsters_spawns_dungeons_groups WHERE DungeonSpawnId = %d;" % sid)
        und.append("-- groupe d'origine du spawn %d : restaurer depuis la sauvegarde de monsters_spawns_dungeons_groups" % sid)
        und.append("DELETE FROM monsters_spawns_dungeons_groups WHERE DungeonSpawnId = %d AND Informations = '%s';" % (sid, TAG))
        for k, size in enumerate(SIZES):
            ms = [(boss, k + 1)] + [(pool[i % n], top(pool[i % n])) for i in range(max(size, 4) - 1)]
            for m, g in ms:
                sql.append("INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES (%d, %d, %d, '%s');" % (sid, grades[m][g], size, TAG))
        rap.append("%d\t%s\tdépart\t%s" % (sid, dj, ", ".join("%s g%d" % (nom(m), g) for m, g in [(boss, 1)] + [(p, top(p)) for p in pool[:3]])))
    for w in range(1, 5):
        for size in SIZES:
            ms = [pool[(w + i) % n] for i in range(max(size, 4))]
            for m in ms:
                sql.append("INSERT INTO monsters_spawns_dungeons_waves (DungeonSpawnId, MonsterGradeId, MinPlayerLevel, WaveNumber, MonsterId, MonsterName) VALUES (%d, %d, %d, %d, %d, '%s');"
                           % (sid, grades[m][top(m)], size, w, m, TAG))
        rap.append("%d\t%s\t%d\t%s" % (sid, dj, w, ", ".join("%s g%d" % (nom(pool[(w + i) % n]), top(pool[(w + i) % n])) for i in range(4))))
for name, lines in (("ajouter_vagues_dimensions.sql", sql), ("ajouter_vagues_dimensions-annuler.sql", und), ("ajouter_vagues_dimensions_rapport.tsv", rap)):
    open(os.path.join(ICI, name), "w", encoding="utf-8", newline="\n").write("\n".join(lines) + "\n")
print(len(SALLES), "salles,", sum(1 for l in sql if "INTO monsters_spawns_dungeons_waves" in l), "lignes de vagues")
