# PADGREF 23/10 (retour utilisateur : « [Moqihura] lance Les soins reçus guérissent les corruptions… » dans le chat) : le client 2.51
# écrit « X lance <sort> » pour chaque sort lancé dont verbose_cast est vrai ; les sorts importés de l'Eliocalypse déclenchés tout
# seuls (passifs, auras, états de donjon posés sur les joueurs) gardaient verbose_cast = vrai. On le met à faux pour tous les sorts
# de l'Eliocalypse qu'aucun monstre ne lance lui-même (absents de monsters_spells) : seuls les vrais sorts des monstres restent
# affichés. Modifie sur place le Spells.d2o de la sortie Eliocalypse (sauvegarde dans SAVE avant).
import json, os, shutil, subprocess, sys
sys.stdout.reconfigure(encoding='utf-8')
ICI = os.path.dirname(os.path.abspath(__file__))
CB = os.path.dirname(ICI)
R = os.path.abspath(os.path.join(CB, '..', '..', '..'))
sys.path.insert(0, CB)
from d2o_ecrire import D2O
MYSQL = os.path.join(R, '_OUTILS', 'logiciels', 'MariaDB', 'bin', 'mysql.exe')
F = os.path.join(ICI, 'client', 'data', 'common', 'Spells.d2o')
dn = json.load(open(os.path.join(ICI, 'donnees.json'), encoding='utf-8'))
out = subprocess.run([MYSQL, '-uroot', '-h127.0.0.1', '-N', '-B', 'world', '-e', 'SELECT DISTINCT SpellId FROM monsters_spells'],
                     capture_output=True, check=True).stdout.decode()
lances = {int(x) for x in out.split()}
sauve = os.path.join(R, 'SAVE', '3_CLIENT_64bits', '2026-10-23_sorts-verbose-avant')
os.makedirs(sauve, exist_ok=True)
if not os.path.exists(os.path.join(sauve, 'Spells.d2o')):
    shutil.copy2(F, sauve)
sp = D2O(os.path.join(sauve, 'Spells.d2o'))
caches = []
for s in sorted(set(dn['sorts'])):
    o = sp.objet(s)
    if o is None or s in lances or not o.get('verbose_cast'):
        continue
    o = dict(o); o['verbose_cast'] = False
    sp.poser(s, o); caches.append(s)
sp.ecrire(F)
print('sorts cachés du chat :', len(caches), '| sorts de monstres gardés :', len(set(dn['sorts']) & lances))
