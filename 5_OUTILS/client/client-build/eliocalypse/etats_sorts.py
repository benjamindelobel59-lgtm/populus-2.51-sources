# PADGREF 23/10 (audit DPLN, Arbre de Mort) : conditions d'état des sorts de l'Eliocalypse PERDUES à l'import : le 2.74 les écrit
# « HS=1627 » (le lanceur doit avoir l'état) / « HS!1627 » (ne doit pas l'avoir), donnees.py ne lisait que « E123 » / « !E123 » ->
# statesRequired / statesForbidden vides (ex. Corruption lançait ses sorts « monté » et « à pied » en même temps).
# Corrige : client (SpellLevels.d2o de la sortie Eliocalypse, sur place) + serveur (SQL 121, spells_levels).
import json, os, re, shutil, subprocess, sys
sys.stdout.reconfigure(encoding='utf-8')
ICI = os.path.dirname(os.path.abspath(__file__))
CB = os.path.dirname(ICI)
R = os.path.abspath(os.path.join(CB, '..', '..', '..'))
sys.path.insert(0, CB)
from d2o_ecrire import D2O
sys.path.insert(0, os.path.join(R, '_OUTILS', 'serveur', 'donjons'))
import d2o
MYSQL = os.path.join(R, '_OUTILS', 'logiciels', 'MariaDB', 'bin', 'mysql.exe')
N = os.path.join(R, '_OUTILS', 'client', 'client_27x', 'data', 'common')
F = os.path.join(ICI, 'client', 'data', 'common', 'SpellLevels.d2o')
SPN, SLN = d2o.load(os.path.join(N, 'Spells.d2o'))[1], d2o.load(os.path.join(N, 'SpellLevels.d2o'))[1]
dn = json.load(open(os.path.join(ICI, 'donnees.json'), encoding='utf-8'))


def etats(crit):
    req = [int(x) for x in re.findall(r'(?:HS|E)=(\d+)', crit or '')]
    interdits = [int(x) for x in re.findall(r'(?:HS|E)!(\d+)', crit or '')] + [int(x) for x in re.findall(r'!E(\d+)', crit or '')]
    return req, interdits


sauve = os.path.join(R, 'SAVE', '3_CLIENT_64bits', '2026-10-23_spelllevels-etats-avant')
os.makedirs(sauve, exist_ok=True)
shutil.copy2(F, sauve)
SL = D2O(F)
lignes = []
for s in dn['sorts']:
    for l in SPN.get(s, {}).get('spellLevels', []):
        req, interdits = etats(SLN[l].get('statesCriterion'))
        if not req and not interdits:
            continue
        o = SL.objet(l)
        if o is None:
            continue
        o = dict(o); o['statesRequired'] = req; o['statesForbidden'] = interdits
        SL.poser(l, o)
        lignes.append(f"UPDATE spells_levels SET StatesRequiredCSV = '{','.join(map(str, req))}', StatesForbiddenCSV = '{','.join(map(str, interdits))}' "
                      f"WHERE Id = {l};  -- sort {s} : {SLN[l]['statesCriterion']}")
SL.ecrire(F)
D = os.path.join(R, '_OUTILS', 'serveur', 'donjons', 'eliocalypse')
open(os.path.join(D, '121_eliocalypse_etats_des_sorts.sql'), 'w', encoding='utf-8', newline='\n').write('\n'.join(
    ["-- PADGREF 23/10/2026 : SQL 121 — Eliocalypse : états requis / interdits des sorts (« HS=… » / « HS!… » du 2.74, perdus à l'import ;",
     "-- ex. Corruption : sorts monté / à pied). Généré par eliocalypse/etats_sorts.py. Base world. Rejouable.", "START TRANSACTION;"] + lignes + ["COMMIT;"]) + '\n')
open(os.path.join(D, '121_eliocalypse_etats_des_sorts-annuler.sql'), 'w', encoding='utf-8', newline='\n').write('\n'.join(
    ["-- PADGREF 23/10 : annule 121 (états vides, comme avant)", "START TRANSACTION;"] +
    [re.sub(r"SET .*? WHERE", "SET StatesRequiredCSV = '', StatesForbiddenCSV = '' WHERE", l) for l in lignes] + ["COMMIT;"]) + '\n')
print('niveaux corrigés :', len(lignes))
