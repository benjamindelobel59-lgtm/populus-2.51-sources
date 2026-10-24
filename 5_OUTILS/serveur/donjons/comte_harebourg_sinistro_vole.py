# PADGREF 23/10 (demande utilisateur) : Culbutœuf (3402), Sinistro vole (sort 3574, niveau 20749) : les buffs +PM / +Tacle vont aussi
# au Culbutœuf lui-même. En 2.51 la cible est « g » (alliés SAUF le lanceur) ; on passe à « a » (tous les alliés, lanceur compris).
# Serveur seulement (le masque ne sert côté client qu'à l'aperçu des sorts du joueur). Sortie : SQL 103 (+ annuler).
import json, os, subprocess, sys
sys.stdout.reconfigure(encoding='utf-8')
R = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..', '..'))
sys.path.insert(0, os.path.join(R, '_OUTILS', 'client', 'client-build'))
from d2o_ecrire import D2O
EXPORT = os.path.join(R, '_OUTILS', 'serveur', 'export_d2o', 'bin', 'ExportD2o.exe')
MYSQL = os.path.join(R, '_OUTILS', 'logiciels', 'MariaDB', 'bin', 'mysql.exe')
TMP = sys.argv[1]
NIV = 20749
L = D2O(os.path.join(R, 'CLIENT_64bits', 'app', 'data', 'common', 'SpellLevels.d2o'))


def bins(chemin):
    out = subprocess.run([EXPORT, 'spelllevels', chemin, str(NIV)], capture_output=True).stdout.decode('utf-8')
    o = json.loads(out.splitlines()[0])
    return o['__EffectsBin'], o['__CriticalEffectsBin']


def base():
    out = subprocess.run([MYSQL, '-uroot', '-h127.0.0.1', '-N', '-B', 'world', '-e',
                          f'SELECT HEX(EffectsBin), HEX(CriticalEffectsBin) FROM spells_levels WHERE Id={NIV}'], capture_output=True)
    return out.stdout.decode().split()


avant = bins(os.path.join(R, 'CLIENT_64bits', 'app', 'data', 'common', 'SpellLevels.d2o'))
assert [x.upper() for x in avant] == base(), 'la base ne correspond pas au client 2.51 : vérifier avant de remplacer'
niv = L.objet(NIV)
for liste in (niv['effects'], niv['criticalEffect']):
    for e in liste:
        assert e['targetMask'] == 'g', e['targetMask']
        e['targetMask'] = 'a'
L.poser(NIV, niv)
L.ecrire(os.path.join(TMP, 'SpellLevels.d2o'))
apres = bins(os.path.join(TMP, 'SpellLevels.d2o'))
D = os.path.join(R, '_OUTILS', 'serveur', 'donjons', 'comte_harebourg')
os.makedirs(D, exist_ok=True)
tete = "-- PADGREF 23/10/2026 : SQL 103 — Comte Harebourg, Culbutœuf : Sinistro vole (niveau 20749) donne aussi +PM / +Tacle au lanceur\n" \
       "-- (cible « g » alliés sauf lanceur -> « a » tous les alliés). Généré par donjons/comte_harebourg_sinistro_vole.py. Base world. Rejouable.\n"
open(os.path.join(D, '103_sinistro_vole.sql'), 'w', encoding='utf-8', newline='\n').write(
    tete + f"UPDATE spells_levels SET EffectsBin = 0x{apres[0]}, CriticalEffectsBin = 0x{apres[1]} WHERE Id = {NIV};\n")
open(os.path.join(D, '103_sinistro_vole-annuler.sql'), 'w', encoding='utf-8', newline='\n').write(
    "-- PADGREF 23/10 : annule 103_sinistro_vole.sql (retour à la cible 2.51 « g »)\n"
    f"UPDATE spells_levels SET EffectsBin = 0x{avant[0]}, CriticalEffectsBin = 0x{avant[1]} WHERE Id = {NIV};\n")
print('ok', len(avant[0]), len(apres[0]), avant[0] != apres[0])
