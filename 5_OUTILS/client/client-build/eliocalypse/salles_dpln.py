# PADGREF 23/10 (audit DPLN des 4 donjons de l'Eliocalypse) : composition des salles selon les captures de Dofus pour les Noobs
# (combats à 4 joueurs). Groupe de base (1-4 joueurs) = DPLN ; ajouts de 5 à 8 joueurs : DPLN n'en dit rien (sauf le Skentu, 1er
# ajout de la salle de Misère) -> on alterne les monstres du donjon. Système modulaire : chaque palier = groupe COMPLET.
# Grades : ceux déjà utilisés en base pour chaque monstre (SQL 099/113). Sortie : SQL 122 (+ annuler = groupes actuels).
import os, subprocess, sys
sys.stdout.reconfigure(encoding='utf-8')
ICI = os.path.dirname(os.path.abspath(__file__))
R = os.path.abspath(os.path.join(ICI, '..', '..', '..', '..'))
MYSQL = os.path.join(R, '_OUTILS', 'logiciels', 'MariaDB', 'bin', 'mysql.exe')
D = os.path.join(R, '_OUTILS', 'serveur', 'donjons', 'eliocalypse')


def rq(s):
    out = subprocess.run([MYSQL, '-uroot', '-h127.0.0.1', '-N', '-B', 'world', '-e', s], capture_output=True, check=True).stdout.decode()
    return [l.split('\t') for l in out.replace('\r', '').split('\n') if l]


# monstres
RAV, FLE, OLG, MAC, TRA, GUERRE = 6021, 6022, 6023, 6024, 6025, 6014
FER, KRE, DES, SKE, DAW, MISERE = 5991, 5992, 5993, 5994, 5995, 5990
PIS, DOL, NHE, TEN, GAN, CORRUPTION = 6027, 6028, 6029, 6030, 6031, 6026
TAM, ARM, GEN, BOU, ECA, SERVITUDE = 5978, 5979, 5980, 5983, 5989, 5955
# spawn : (base 1-4 joueurs, ajouts 5 / 6 / 7 / 8)
SALLES = {
    9700: ([RAV, TRA, OLG, FLE], [MAC, RAV, OLG, FLE]),           # Trône de Sang
    9701: ([RAV, MAC, OLG, FLE], [TRA, RAV, OLG, FLE]),
    9702: ([RAV, TRA, OLG, FLE], [MAC, OLG, RAV, FLE]),
    9703: ([RAV, MAC, OLG, OLG], [FLE, TRA, RAV, MAC]),
    9704: ([GUERRE, RAV, OLG, TRA], [FLE, MAC, RAV, OLG]),
    9705: ([SKE, DES, KRE, KRE], [DAW, FER, SKE, DES]),            # Sentence de la Balance
    9706: ([DAW, SKE, SKE, FER], [KRE, DES, DAW, FER]),
    9707: ([DAW, DES, DES, KRE], [SKE, FER, KRE, DAW]),
    9708: ([DAW, SKE, FER, FER], [DES, KRE, SKE, DAW]),
    9709: ([MISERE, FER, KRE, DES], [SKE, DAW, FER, KRE]),
    9710: ([GAN, GAN, NHE, DOL], [TEN, PIS, GAN, NHE]),            # Arbre de Mort
    9711: ([TEN, NHE, DOL, PIS], [GAN, TEN, DOL, NHE]),
    9712: ([GAN, TEN, NHE, DOL], [PIS, GAN, TEN, NHE]),
    9713: ([TEN, NHE, NHE, PIS], [GAN, DOL, TEN, PIS]),
    9714: ([CORRUPTION, NHE, DOL, PIS], [GAN, TEN, NHE, DOL]),
    9716: ([ARM, ECA, GEN, BOU], [TAM, ECA, BOU, GEN]),            # Fers de la Tyrannie (salle 1 9715 : déjà conforme)
    9717: ([ARM, ECA, TAM, BOU], [GEN, ECA, TAM, BOU]),
    9718: ([ARM, ECA, ECA, GEN], [TAM, BOU, ECA, GEN]),
    9719: ([SERVITUDE, ECA, TAM, BOU], [GEN, ECA, TAM, BOU]),
}
actuels = rq("SELECT g.DungeonSpawnId, g.MonsterGradeId, g.MinPartyMembers, gr.MonsterId FROM monsters_spawns_dungeons_groups g "
             "JOIN monsters_grades gr ON gr.Id = g.MonsterGradeId WHERE g.DungeonSpawnId BETWEEN 9700 AND 9719")
grade = {}                                  # (spawn, monstre) -> grade utilisé ; sinon grade utilisé ailleurs dans le donjon
for s, g, k, m in actuels:
    grade.setdefault((int(s), int(m)), int(g)); grade.setdefault(int(m), int(g))
for m in (ARM,):
    if m not in grade:
        grade[m] = int(rq(f"SELECT Id FROM monsters_grades WHERE MonsterId = {m} ORDER BY ABS(CAST(GradeId AS SIGNED) - 3) LIMIT 1")[0][0])
lignes = []
for sid, (base, ajouts) in SALLES.items():
    g = lambda m: grade.get((sid, m), grade[m])
    for i, k in enumerate((1, 5, 6, 7, 8)):
        lignes += [f"({sid}, {g(m)}, {k}, 'PADGREF DPLN')" for m in base + ajouts[:i]]
ids = ', '.join(map(str, SALLES))
sql = ["-- PADGREF 23/10/2026 : SQL 122 — Eliocalypse : composition des salles selon Dofus pour les Noobs (groupes à 1-4 joueurs ; ajouts",
       "-- 5-8 alternés, Skentu en 1er chez Misère). Système modulaire complet. Généré par eliocalypse/salles_dpln.py. Base world. Rejouable.",
       "START TRANSACTION;", f"DELETE FROM monsters_spawns_dungeons_groups WHERE DungeonSpawnId IN ({ids});",
       "INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES\n" + ",\n".join(lignes) + ";",
       "COMMIT;"]
open(os.path.join(D, '122_eliocalypse_salles_dpln.sql'), 'w', encoding='utf-8', newline='\n').write('\n'.join(sql) + '\n')
ann = ["-- PADGREF 23/10 : annule 122 (groupes d'avant)", "START TRANSACTION;", f"DELETE FROM monsters_spawns_dungeons_groups WHERE DungeonSpawnId IN ({ids});",
       "INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES\n" +
       ",\n".join(f"({s}, {g}, {k}, 'PADGREF modulaire')" for s, g, k, m in actuels if int(s) in SALLES) + ";", "COMMIT;"]
open(os.path.join(D, '122_eliocalypse_salles_dpln-annuler.sql'), 'w', encoding='utf-8', newline='\n').write('\n'.join(ann) + '\n')
print('salles', len(SALLES), 'lignes', len(lignes), '| grade Armécréante', grade[ARM])
