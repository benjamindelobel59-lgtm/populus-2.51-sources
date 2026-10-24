# PADGREF 23/10 : Eliocalypse — BUTIN des 33 monstres (SQL 100) : drops du client 2.74 (Monsters.d2o) dont l'objet existe chez
# nous (2.51 ou importé par objets.py). Écartés : objets de quête / événement, conditions de quête, d'événement (Sc=), Temporis (Mt=1),
# métier (Pj / CU). Taux 2.74, taux normal du serveur (TauxFixe 0, comme les drops d'origine) ; kamas déjà dans monsters_templates.
import json, os, re, subprocess, sys
sys.stdout.reconfigure(encoding='utf-8')
ICI = os.path.dirname(os.path.abspath(__file__))
CB = os.path.dirname(ICI)
R = os.path.abspath(os.path.join(CB, '..', '..', '..'))
sys.path.insert(0, os.path.join(R, '_OUTILS', 'serveur', 'donjons'))
import d2o, d2i
MYSQL = os.path.join(R, '_OUTILS', 'logiciels', 'MariaDB', 'bin', 'mysql.exe')
N = os.path.join(R, '_OUTILS', 'client', 'client_27x', 'data')
SQLDIR = os.path.join(R, '_OUTILS', 'serveur', 'donjons', 'eliocalypse')


def rq(s):
    out = subprocess.run([MYSQL, '-uroot', '-h127.0.0.1', '-N', '-B', '--default-character-set=utf8', 'world', '-e', s],
                         capture_output=True, check=True).stdout.decode('utf-8')
    return [l.split('\t') for l in out.replace('\r', '').split('\n') if l]


T = d2i.load(os.path.join(N, 'i18n', 'i18n_fr.d2i'))
MN = d2o.load(os.path.join(N, 'common', 'Monsters.d2o'))[1]
IN = d2o.load(os.path.join(N, 'common', 'Items.d2o'))[1]
dn = json.load(open(os.path.join(ICI, 'donnees.json'), encoding='utf-8'))
existants = {int(a) for (a,) in rq("SELECT Id FROM items_templates UNION SELECT Id FROM items_templates_weapons")}
EXCLUS_TYPES = {126, 131, 220}
lignes, ecartes = [], []
for m in dn['monstres']:
    for x in MN[m].get('drops', []):
        i, c = x['objectId'], (x.get('criteria') or '')
        c = re.sub(r'(^|&)(Ms=0|Mt=0|Sc!\d+)(?=&|$)', '', c).strip('&')
        taux = [x.get(f'percentDropForGrade{g}', 0) or 0 for g in range(1, 6)]
        if max(taux) <= 0 and (x.get('maxPercentDrop') or 0) > 0:
            taux = [x['maxPercentDrop']] * 5
        raison = None
        if i not in existants: raison = 'objet absent'
        elif IN.get(i, {}).get('typeId') in EXCLUS_TYPES: raison = 'objet de quête'
        elif re.search(r'Q[aosf]|OA|Sc=|Mt=1|Pj|CU|DD|PB', c): raison = 'condition'
        elif max(taux) <= 0: raison = 'taux 0'
        if raison:
            ecartes.append((T.get(MN[m]['nameId']), T.get(IN[i]['nameId']) if i in IN else i, raison)); continue
        cond = "'" + c.replace("'", "''") + "'" if c else 'NULL'
        lignes.append(f"({m}, {i}, 0, {', '.join(str(round(t, 5)) for t in taux)}, 1, 0, {cond}, 0, 0, 0)")
ml = ','.join(map(str, dn['monstres']))
sql = ["-- PADGREF 23/10/2026 : SQL 100 — Eliocalypse, butin des 33 monstres (taux du client 2.74, taux normal du serveur). Généré par",
       "-- eliocalypse/butin.py. Ressources 12 % sur leur monstre, Plume / Corne / Verrou / Bonnet 12 % sur le Cavalier, équipements des",
       "-- Cavaliers 0,01 %, Dofus du Cauchemar 0,05 %, sachets 1-3 %. Base world. Rejouable.", "SET NAMES utf8mb4;", "START TRANSACTION;",
       f"DELETE FROM monsters_drops WHERE MonsterOwnerId IN ({ml});",
       "INSERT INTO monsters_drops (MonsterOwnerId, ItemId, DropLimit, DropRateForGrade1, DropRateForGrade2, DropRateForGrade3, DropRateForGrade4, "
       "DropRateForGrade5, RollsCounter, ProspectingLock, `Condition`, DropGroup, TaxCollectorCannotLoot, TauxFixe) VALUES\n" + ",\n".join(lignes) + ";",
       "COMMIT;"]
open(os.path.join(SQLDIR, '100_eliocalypse_butin.sql'), 'w', encoding='utf-8', newline='\n').write('\n'.join(sql) + '\n')
open(os.path.join(SQLDIR, '100_eliocalypse_butin-annuler.sql'), 'w', encoding='utf-8', newline='\n').write(
    f"-- PADGREF 23/10 : annule 100_eliocalypse_butin.sql\nDELETE FROM monsters_drops WHERE MonsterOwnerId IN ({ml});\n")
print('ok :', len(lignes), 'drops ;', len(ecartes), 'écartés :', sorted({e[2] for e in ecartes}), [e for e in ecartes if e[2] == 'objet absent'][:5])
