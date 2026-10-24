# PADGREF 23/10 : change le masque de cible d'effets d'un niveau de sort côté serveur (EffectsBin / CriticalEffectsBin), à partir des
# données du client 2.51 (vérifiées identiques à la base avant). Effets visés : ceux dont le masque vaut exactement <avant>.
# Usage : py -I cible_sort.py <dossier temporaire> <id niveau> <masque avant> <masque après>  -> imprime UPDATE (nouveau) puis UPDATE (ancien)
import json, os, subprocess, sys
sys.stdout.reconfigure(encoding='utf-8')
R = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..', '..', '..'))
sys.path.insert(0, os.path.join(R, '_OUTILS', 'client', 'client-build'))
from d2o_ecrire import D2O
EXPORT = os.path.join(R, '_OUTILS', 'serveur', 'export_d2o', 'bin', 'ExportD2o.exe')
MYSQL = os.path.join(R, '_OUTILS', 'logiciels', 'MariaDB', 'bin', 'mysql.exe')
TMP, NIV, AVANT, APRES = sys.argv[1], int(sys.argv[2]), sys.argv[3], sys.argv[4]
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
n = 0
for liste in (niv['effects'], niv['criticalEffect']):
    for e in liste:
        if e['targetMask'] == AVANT:
            e['targetMask'] = APRES
            n += 1
assert n, 'aucun effet avec ce masque'
print(f'-- {n} effet(s) : cible « {AVANT} » -> « {APRES} »', file=sys.stderr)
L.poser(NIV, niv)
L.ecrire(os.path.join(TMP, 'SpellLevels.d2o'))
apres = bins(os.path.join(TMP, 'SpellLevels.d2o'))
print(f"UPDATE spells_levels SET EffectsBin = 0x{apres[0]}, CriticalEffectsBin = 0x{apres[1]} WHERE Id = {NIV};")
print(f"UPDATE spells_levels SET EffectsBin = 0x{avant[0]}, CriticalEffectsBin = 0x{avant[1]} WHERE Id = {NIV};")
