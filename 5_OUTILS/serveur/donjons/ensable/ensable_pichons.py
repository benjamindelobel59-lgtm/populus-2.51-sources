# PADGREF 24/10 SQL 162 (demande utilisateur : « refais le donjon Ensablé, ce ne sont pas les bons mobs, normalement que des
# Pichons, là il y a des crabes ») : comme l'officiel (guides JOL / Gamosaurus), les 5 salles du Château Ensablé n'ont que des Pichons
# (Orange 920, Bleu 921, Blanc 922, Vert 923, Kloune 924) ; la dernière garde Mob l'Éponge 928 (Raul Mops retiré à la demande de l'utilisateur).
# Les Crabes (63) et Étoiles de la Mer d'Asse (927) des salles 1 à 5 (spawns 11-15) deviennent des Pichons du MÊME grade (mêmes
# paliers, même nombre de monstres). 24/10 (demande utilisateur : « enlève Raul Mops ») : Raul Mops 926 aussi. Usage : py -I ensable_pichons.py  (écrit 162 + annuler)
import os, subprocess
ICI = os.path.dirname(os.path.abspath(__file__))
R = os.path.abspath(os.path.join(ICI, '..', '..', '..', '..'))
MY = os.path.join(R, '_OUTILS', 'logiciels', 'MariaDB', 'bin', 'mysql.exe')
def sql(q): return [l.split('\t') for l in subprocess.run([MY, '-uroot', '-N', '-B', 'world', '-e', q], capture_output=True, text=True).stdout.splitlines() if l]
grades = {(int(m), int(g)): int(i) for m, g, i in sql('SELECT MonsterId, GradeId, Id FROM monsters_grades WHERE MonsterId BETWEEN 920 AND 924')}
lignes = sql('SELECT g.Id, g.DungeonSpawnId, g.MonsterGradeId, m.GradeId FROM monsters_spawns_dungeons_groups g '
             'JOIN monsters_grades m ON m.Id = g.MonsterGradeId WHERE g.DungeonSpawnId BETWEEN 11 AND 15 AND m.MonsterId IN (63, 927, 926) ORDER BY g.Id')
up = ["-- PADGREF 24/10 SQL 162 : Château Ensablé, que des Pichons (plus de Crabes, d'Étoiles de la Mer d'Asse ni de Raul Mops). Généré par",
      "-- _OUTILS/serveur/donjons/ensable/ensable_pichons.py."]
ann = ["-- Annulation du SQL 162 (Crabes, Étoiles de la Mer d'Asse et Raul Mops remis)"]
for k, (gid, spawn, ancien, grade) in enumerate(lignes):
    couleurs = [920, 921, 922, 923] + ([924] if int(spawn) >= 13 else [])   # Kloune à partir de la salle 3
    pichon = couleurs[k % len(couleurs)]
    up.append(f"UPDATE monsters_spawns_dungeons_groups SET MonsterGradeId = {grades[(pichon, min(5, int(grade)))]} WHERE Id = {gid};")
    ann.append(f"UPDATE monsters_spawns_dungeons_groups SET MonsterGradeId = {ancien} WHERE Id = {gid};")
open(os.path.join(ICI, '162_ensable_pichons.sql'), 'w', encoding='utf-8').write('\n'.join(up) + '\n')
open(os.path.join(ICI, '162_ensable_pichons-annuler.sql'), 'w', encoding='utf-8').write('\n'.join(ann) + '\n')
print(len(lignes), 'monstres remplacés')
