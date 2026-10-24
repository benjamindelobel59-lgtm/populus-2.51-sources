# PADGREF 23/10 : change les valeurs (min-max) d'un effet d'un niveau de sort côté serveur (EffectsBin / CriticalEffectsBin de
# spells_levels), à partir des données du client 2.51 (vérifiées identiques à la base avant). Le critique garde le même rapport.
# Usage : py -I valeurs_sort.py <dossier temporaire> <id niveau> <id effet> <min> <max>  -> imprime UPDATE (nouveau) puis UPDATE (ancien)
import json, os, subprocess, sys
sys.stdout.reconfigure(encoding='utf-8')
R = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..', '..', '..'))
sys.path.insert(0, os.path.join(R, '_OUTILS', 'client', 'client-build'))
from d2o_ecrire import D2O
EXPORT = os.path.join(R, '_OUTILS', 'serveur', 'export_d2o', 'bin', 'ExportD2o.exe')
MYSQL = os.path.join(R, '_OUTILS', 'logiciels', 'MariaDB', 'bin', 'mysql.exe')
TMP, NIV, EFFET, MIN, MAX = sys.argv[1], int(sys.argv[2]), int(sys.argv[3]), int(sys.argv[4]), int(sys.argv[5])
SRC = os.path.join(R, 'CLIENT_64bits', 'app', 'data', 'common', 'SpellLevels.d2o')


def bins(chemin):
    o = json.loads(subprocess.run([EXPORT, 'spelllevels', chemin, str(NIV)], capture_output=True).stdout.decode('utf-8').splitlines()[0])
    return o['__EffectsBin'], o['__CriticalEffectsBin']


avant = bins(SRC)
base = subprocess.run([MYSQL, '-uroot', '-h127.0.0.1', '-N', '-B', 'world', '-e',
                       f'SELECT HEX(EffectsBin), HEX(CriticalEffectsBin) FROM spells_levels WHERE Id={NIV}'], capture_output=True).stdout.decode().split()
assert [x.upper() for x in avant] == base, 'la base ne correspond pas au client 2.51 : vérifier avant de remplacer'
L = D2O(SRC)
niv = L.objet(NIV)
rapport = None
for e in niv['effects']:
    if e['effectId'] == EFFET:
        rapport = (MIN / e['diceNum'], MAX / max(e['diceSide'], e['diceNum']))
        print(f'-- normal {e["diceNum"]}-{e["diceSide"]} -> {MIN}-{MAX}', file=sys.stderr)
        e['diceNum'], e['diceSide'] = MIN, MAX
assert rapport, 'effet absent'
for e in niv['criticalEffect']:
    if e['effectId'] == EFFET:
        mn, mx = round(e['diceNum'] * rapport[0]), round(max(e['diceSide'], e['diceNum']) * rapport[1])
        print(f'-- critique {e["diceNum"]}-{e["diceSide"]} -> {mn}-{mx}', file=sys.stderr)
        e['diceNum'], e['diceSide'] = mn, mx
L.poser(NIV, niv)
L.ecrire(os.path.join(TMP, 'SpellLevels.d2o'))
apres = bins(os.path.join(TMP, 'SpellLevels.d2o'))
print(f"UPDATE spells_levels SET EffectsBin = 0x{apres[0]}, CriticalEffectsBin = 0x{apres[1]} WHERE Id = {NIV};")
print(f"UPDATE spells_levels SET EffectsBin = 0x{avant[0]}, CriticalEffectsBin = 0x{avant[1]} WHERE Id = {NIV};")
