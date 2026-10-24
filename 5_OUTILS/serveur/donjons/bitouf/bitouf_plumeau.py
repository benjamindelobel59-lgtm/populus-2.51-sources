# PADGREF 23/10 (demande utilisateur : « Plumeau Cécitant doit taper du 180 environ ») : Bitouf des Plaines (1019, 5704), sort 4534
# niveau 22277. 2.51 : 11-15 dommages Neutre (≈ 35-60 avec la Force 220-300 du Bitouf). Passé à 40-50 (critique : même rapport),
# soit ≈ 130-160 au grade 1 et 160-200 au grade 5. Serveur seulement (l'infobulle du client affiche encore 11-15). Sortie : SQL 131.
import json, os, subprocess, sys
sys.stdout.reconfigure(encoding='utf-8')
R = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..', '..', '..'))
sys.path.insert(0, os.path.join(R, '_OUTILS', 'client', 'client-build'))
from d2o_ecrire import D2O
EXPORT = os.path.join(R, '_OUTILS', 'serveur', 'export_d2o', 'bin', 'ExportD2o.exe')
MYSQL = os.path.join(R, '_OUTILS', 'logiciels', 'MariaDB', 'bin', 'mysql.exe')
TMP = sys.argv[1]
NIV = 22277
SRC = os.path.join(R, 'CLIENT_64bits', 'app', 'data', 'common', 'SpellLevels.d2o')
L = D2O(SRC)


def bins(chemin):
    out = subprocess.run([EXPORT, 'spelllevels', chemin, str(NIV)], capture_output=True).stdout.decode('utf-8')
    o = json.loads(out.splitlines()[0])
    return o['__EffectsBin'], o['__CriticalEffectsBin']


def base():
    out = subprocess.run([MYSQL, '-uroot', '-h127.0.0.1', '-N', '-B', 'world', '-e',
                          f'SELECT HEX(EffectsBin), HEX(CriticalEffectsBin) FROM spells_levels WHERE Id={NIV}'], capture_output=True)
    return out.stdout.decode().split()


avant = bins(SRC)
assert [x.upper() for x in avant] == base(), 'la base ne correspond pas au client 2.51 : vérifier avant de remplacer'
niv = L.objet(NIV)
for liste, borne in ((niv['effects'], (40, 50)), (niv['criticalEffect'], None)):
    for e in liste:
        if e['effectId'] == 100:
            print('avant', e['diceNum'], e['diceSide'])
            if borne is None:   # critique : même rapport que le normal
                mn, mx = round(e['diceNum'] * 40 / 11), round(e['diceSide'] * 50 / 15)
            else:
                mn, mx = borne
            e['diceNum'], e['diceSide'] = mn, mx
            print('apres', mn, mx)
L.poser(NIV, niv)
L.ecrire(os.path.join(TMP, 'SpellLevels.d2o'))
apres = bins(os.path.join(TMP, 'SpellLevels.d2o'))
D = os.path.dirname(os.path.abspath(__file__))
tete = "-- PADGREF 23/10/2026 : SQL 131 — Bitouf des Plaines : Plumeau Cécitant (niveau 22277) 11-15 -> 40-50 dommages Neutre\n" \
       "-- (≈ 180 au grade 5, demande utilisateur). Généré par donjons/bitouf/bitouf_plumeau.py. Base world. Rejouable.\n"
open(os.path.join(D, '131_bitouf_plumeau.sql'), 'w', encoding='utf-8', newline='\n').write(
    tete + f"USE `world`;\nUPDATE spells_levels SET EffectsBin = 0x{apres[0]}, CriticalEffectsBin = 0x{apres[1]} WHERE Id = {NIV};\n")
open(os.path.join(D, '131_bitouf_plumeau-annuler.sql'), 'w', encoding='utf-8', newline='\n').write(
    "-- PADGREF 23/10 : annule 131_bitouf_plumeau.sql (retour aux 11-15 de la 2.51)\n"
    f"USE `world`;\nUPDATE spells_levels SET EffectsBin = 0x{avant[0]}, CriticalEffectsBin = 0x{avant[1]} WHERE Id = {NIV};\n")
print('ok', avant[0] != apres[0], avant[1] != apres[1])
