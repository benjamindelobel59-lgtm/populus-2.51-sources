# PADGREF 24/10 SQL 152 (retour utilisateur : « les combats double boss au Comte Harebourg + Nileza etc : faut pas ajouter les monstres
# du Nileza ou autre, c'est toujours les monstres du Comte ») : dans les duos 97001-97004, chaque accompagnateur d'une autre famille
# (non boss) est remplacé, au même grade, par un monstre du Comte : Strigide 3411, Sinistrofu 3414, Nocturlabe 3415 (à tour de rôle).
# Les deux boss, Granduk et Cycloïde ne changent pas. Usage : py -I duos_monstres_comte.py (écrit 152 + annuler)
import os, subprocess, collections
R = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..', '..', '..'))
MY = os.path.join(R, '_OUTILS', 'logiciels', 'MariaDB', 'bin', 'mysql.exe')
def sql(q): return [l.split('\t') for l in subprocess.run([MY, '-uroot', '-N', '-B', 'world', '-e', q], capture_output=True, text=True).stdout.splitlines() if l]
BOSS = {3416, 3397, 3409, 3384, 3391}
COMTE = {3410, 3411, 3412, 3414, 3415}
REMPLACE = [3411, 3414, 3415]
grades = {(int(m), int(g)): int(i) for i, m, g in sql('select Id, MonsterId, GradeId from monsters_grades where MonsterId in (3411,3414,3415)')}
lignes = sql('select g.Id, g.DungeonSpawnId, g.MinPartyMembers, g.MonsterGradeId, gr.MonsterId, gr.GradeId from monsters_spawns_dungeons_groups g '
             'join monsters_grades gr on gr.Id = g.MonsterGradeId where g.DungeonSpawnId in (97001,97002,97003,97004) order by g.DungeonSpawnId, g.MinPartyMembers, gr.MonsterId, g.Id')
paliers = collections.defaultdict(list)
for l in lignes:
    i, sp, mp, mg, m, gr = map(int, l)
    paliers[(sp, mp)].append((i, mg, m, gr))
up, ann = [], []
for cle, ls in paliers.items():
    k = 0
    for i, mg, m, gr in ls:
        if m in BOSS or m in COMTE:
            continue
        nv = grades[(REMPLACE[k % len(REMPLACE)], gr)]; k += 1
        up.append(f'UPDATE monsters_spawns_dungeons_groups SET MonsterGradeId = {nv} WHERE Id = {i} AND MonsterGradeId = {mg}; -- {cle} monstre {m} g{gr}')
        ann.append(f'UPDATE monsters_spawns_dungeons_groups SET MonsterGradeId = {mg} WHERE Id = {i};')
ent = ('-- PADGREF 24/10 SQL 152 : duos de boss du Comte Harebourg (97001-97004) : les accompagnateurs d\'une autre famille (Nileza, Sylargh,\n'
       '-- Klime, Missiz Frizz) sont remplacés au même grade par des monstres du Comte (Strigide, Sinistrofu, Nocturlabe). Boss inchangés.\n')
open(os.path.join(os.path.dirname(os.path.abspath(__file__)), '152_comte_duos_monstres_comte.sql'), 'w', encoding='utf-8').write(ent + '\n'.join(up) + '\n')
open(os.path.join(os.path.dirname(os.path.abspath(__file__)), '152_comte_duos_monstres_comte-annuler.sql'), 'w', encoding='utf-8').write('-- Annulation du SQL 152\n' + '\n'.join(ann) + '\n')
print(len(up), 'lignes')
