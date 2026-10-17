# -*- coding: utf-8 -*-
"""
NEKFEU : salles de donjon en 8 contre 8 (monstres).
Pour chaque salle (monsters_spawns_dungeons_groups) et chaque palier 5, 6, 7, 8 joueurs : si le groupe a moins de monstres
que de joueurs, on ajoute des copies des monstres NON boss du groupe (à tour de rôle) jusqu'à avoir autant de monstres
que de joueurs. Les salles de boss (au moins un boss dans la salle, ex. Kralamar, Maître des Pantins, Maître Corbac)
ne changent JAMAIS (demande du 16/10) : seulement les salles avant le boss (ex. Craqueleurs).

Utilisation :  python -I monstres_8v8.py            → rapport seulement
               python -I monstres_8v8.py --sql F    → écrit le script SQL F (INSERT …)
"""
import subprocess, sys, os
sys.stdout.reconfigure(encoding='utf-8', errors='replace')

# salles de boss à monstres uniques (pas de copies) : Maître Corbac et ses 3 serviteurs
SALLES_UNIQUES = {186}

MYSQL = os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..', 'logiciels', 'MariaDB', 'bin', 'mysql.exe')


def requete(sql):
    out = subprocess.run([MYSQL, '-uroot', '-h127.0.0.1', '--default-character-set=utf8mb4', '-N', '-B', 'world', '-e', sql],
                         capture_output=True, check=True).stdout.decode('utf-8')
    return [l.split('\t') for l in out.splitlines() if l]


def main():
    sql_out = sys.argv[sys.argv.index('--sql') + 1] if '--sql' in sys.argv else None
    lignes = requete("""select g.DungeonSpawnId, g.MinPartyMembers, g.MonsterGradeId, t.IsBoss, t.Name, ifnull(d.Descripcion,''), g.Id
                        from monsters_spawns_dungeons_groups g
                        join monsters_spawns_dungeons d on d.Id = g.DungeonSpawnId
                        join monsters_grades gr on gr.Id = g.MonsterGradeId
                        join monsters_templates t on t.Id = gr.MonsterId
                        where g.MinPartyMembers between 5 and 8 order by g.DungeonSpawnId, g.MinPartyMembers, g.Id""")
    groupes = {}
    for sid, p, grade, boss, nom, descr, gid in lignes:
        groupes.setdefault((int(sid), int(p)), []).append(dict(grade=int(grade), boss=boss == '1', nom=nom, descr=descr))

    salles_boss = {int(x[0]) for x in requete("""select distinct g.DungeonSpawnId from monsters_spawns_dungeons_groups g
        join monsters_grades gr on gr.Id = g.MonsterGradeId join monsters_templates t on t.Id = gr.MonsterId where t.IsBoss = 1""")}
    ajouts = []
    sans = set()
    for (sid, p), monstres in sorted(groupes.items()):
        if len(monstres) >= p:
            continue
        normaux = [m for m in monstres if not m['boss']]
        if sid in SALLES_UNIQUES or sid in salles_boss:
            sans.add((sid, monstres[0]['descr'], 'boss et serviteurs uniques'))
            continue
        # d'abord les monstres du groupe 8 joueurs de la salle qui manquent ici (composition normale), puis des copies
        manquants = [m for m in groupes.get((sid, 8), []) if not m['boss']]
        for m in monstres:
            for x in manquants:
                if x['grade'] == m['grade']:
                    manquants.remove(x)
                    break
        if not normaux and not manquants:
            sans.add((sid, monstres[0]['descr'], ', '.join(m['nom'] for m in monstres)))
            continue
        noms = []
        pool = manquants + (normaux or manquants)
        for i in range(p - len(monstres)):
            m = pool[i] if i < len(manquants) else pool[len(manquants) + (i - len(manquants)) % len(normaux or manquants)]
            ajouts.append((sid, p, m['grade'], m['descr']))
            noms.append(m['nom'])
        print(f'{sid:>4} {monstres[0]["descr"][:40]:<40} {p} joueurs : {len(monstres)} -> {p} monstres (+ {", ".join(noms)})')

    for sid, descr, noms in sorted(sans):
        print(f'{sid:>4} {descr[:40]:<40} inchangée : boss seuls ({noms})')
    print(f'{len(ajouts)} monstre(s) ajouté(s).')

    if sql_out and ajouts:
        with open(sql_out, 'w', encoding='utf-8', newline='\n') as f:
            f.write('-- NEKFEU : salles de donjon en 8 contre 8 (autant de monstres que de joueurs, paliers 5 à 8). Généré par monstres_8v8.py\n')
            for sid, p, grade, descr in ajouts:
                f.write(f"INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) "
                        f"VALUES ({sid}, {grade}, {p}, 'nekfeu 8v8');\n")
        print('SQL écrit :', sql_out)


main()
