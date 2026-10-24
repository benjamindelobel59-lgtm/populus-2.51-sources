# PADGREF 24/10 : liste « SALLE <spawn> carte <map> : <monstre>=<case>, ... » (bouton Copier de PLANCHE_PLACEMENTS.html, ordre modulaire)
# -> SQL de la table padgref_placements_fixes (DungeonSpawnId, Rang, MonsterId, CellId ; CellId -1 = « ? » : placement normal).
# Contrôles : monstres de la ligne = groupe du plus grand palier du spawn ; cases 0-559 ; pas deux fois la même case dans une salle.
# Usage : py -I generer_sql.py <liste.txt> <num> <nom>   (écrit <num>_<nom>.sql et <num>_<nom>-annuler.sql à côté)
import sys, os, re, subprocess, collections
R = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..', '..', '..'))
MY = os.path.join(R, '_OUTILS', 'logiciels', 'MariaDB', 'bin', 'mysql.exe')
def sql(q): return [l.split('\t') for l in subprocess.run([MY, '-uroot', '-N', '-B', 'world', '-e', q], capture_output=True, text=True).stdout.splitlines() if l]
liste, num, nom = sys.argv[1], sys.argv[2], sys.argv[3]
salles = collections.OrderedDict(); erreurs = []
for l in open(liste, encoding='utf-8'):
    m = re.match(r'\s*SALLE (\d+) carte (\d+) : (.*)', l)
    if not m: continue
    sid, carte, reste = int(m.group(1)), int(m.group(2)), m.group(3)
    ent = []
    for x in reste.split(','):
        a, b = x.strip().split('=')
        ent.append((int(a), -1 if b.strip() == '?' else int(b)))
    salles[sid] = (carte, ent)
for sid, (carte, ent) in salles.items():
    g = sql(f'select g.MinPartyMembers, gr.MonsterId from monsters_spawns_dungeons_groups g join monsters_grades gr on gr.Id = g.MonsterGradeId '
            f'join monsters_spawns_dungeons s on s.Id = g.DungeonSpawnId where g.DungeonSpawnId = {sid} and s.MapId = {carte}')
    if not g: erreurs.append(f'salle {sid} : spawn absent ou carte {carte} différente'); continue
    pmax = max(int(p) for p, _ in g)
    attendu = collections.Counter(int(m) for p, m in g if int(p) == pmax)
    if collections.Counter(m for m, _ in ent) != attendu: erreurs.append(f'salle {sid} : monstres différents du palier {pmax}')
    cases = [c for _, c in ent if c >= 0]
    if any(c > 559 for c in cases): erreurs.append(f'salle {sid} : case hors carte')
    dbl = [c for c, n in collections.Counter(cases).items() if n > 1]
    if dbl: erreurs.append(f'salle {sid} : case(s) en double {dbl}')
if erreurs:
    print('\n'.join(erreurs)); sys.exit(1)
ids = ','.join(str(s) for s in salles)
lignes = [f'-- PADGREF 24/10 SQL {num} : placements FIXES des monstres dans {len(salles)} salles de boss (liste de l\'utilisateur, ordre modulaire :',
          '-- un palier prend les lignes jusqu\'à lui ; CellId -1 = case non donnée -> placement normal). Lu à chaque combat (Fight.PlacerPlacementFixe).',
          'CREATE TABLE IF NOT EXISTS padgref_placements_fixes (DungeonSpawnId INT NOT NULL, Rang INT NOT NULL, MonsterId INT NOT NULL, '
          'CellId INT NOT NULL, PRIMARY KEY (DungeonSpawnId, Rang)) DEFAULT CHARSET=utf8;',
          f'DELETE FROM padgref_placements_fixes WHERE DungeonSpawnId IN ({ids});',
          'INSERT INTO padgref_placements_fixes (DungeonSpawnId, Rang, MonsterId, CellId) VALUES']
vals = [f'({sid},{i},{m},{c})' for sid, (carte, ent) in salles.items() for i, (m, c) in enumerate(ent, 1)]
open(os.path.join(os.path.dirname(liste), f'{num}_{nom}.sql'), 'w', encoding='utf-8').write('\n'.join(lignes) + '\n' + ',\n'.join(vals) + ';\n')
open(os.path.join(os.path.dirname(liste), f'{num}_{nom}-annuler.sql'), 'w', encoding='utf-8').write(
    f'-- Annulation du SQL {num} : ces salles reviennent au placement aléatoire\nDELETE FROM padgref_placements_fixes WHERE DungeonSpawnId IN ({ids});\n')
print(len(salles), 'salles,', len(vals), 'lignes,', sum(1 for v in vals if v.endswith(',-1)')), 'cases « ? »')
