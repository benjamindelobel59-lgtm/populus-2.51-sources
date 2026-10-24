# PADGREF 23/10 (import 2.74, lot 2, choix de l'utilisateur sur PLANCHE_APPARATS.html) : FAMILIERS (2.74 type 249) et MONTILIERS
# (250) d'apparat -> objets d'apparat 2.51 (type 177) sur le modèle de nos apparats existants : Mini Champion 8071 (« Compatible
# avec : familier ») et Dragouf noir 15763 (« Compatible avec : montilier ») : mêmes effets, AppearanceId = os choisi sur la planche.
# Objets « aucun » (os 0) : pas importés pour l'instant. Entrée : choix_apparats.txt (texte collé par l'utilisateur).
# Client : Items.d2o (base : sortie Eliocalypse, la plus récente) + textes 9080000+ (base : i18n cumulé du lot 3-4) ; les icônes sont
# déjà dans le lot 1 (bitmap1_2 / vector1_2). Serveur : SQL 115 (items_templates copiés du modèle, + annuler).
import json, os, re, shutil, subprocess, sys
sys.stdout.reconfigure(encoding='utf-8')
ICI = os.path.dirname(os.path.abspath(__file__))
CB = os.path.dirname(ICI)
R = os.path.abspath(os.path.join(CB, '..', '..', '..'))
sys.path.insert(0, CB)
from d2o_ecrire import D2O
sys.path.insert(0, os.path.join(R, '_OUTILS', 'serveur', 'donjons'))
import d2o, d2i
MYSQL = os.path.join(R, '_OUTILS', 'logiciels', 'MariaDB', 'bin', 'mysql.exe')
N = os.path.join(R, '_OUTILS', 'client', 'client_27x', 'data')
SORTIE = os.path.join(ICI, 'client', 'data')
SQLDIR = os.path.join(R, '_OUTILS', 'serveur', 'import274')
TEXTE0 = 9080000
MODELES = {249: 8071, 250: 15763}
T = d2i.load(os.path.join(N, 'i18n', 'i18n_fr.d2i'))
IN = d2o.load(os.path.join(N, 'common', 'Items.d2o'))[1]
q = lambda s: (s or '').replace('\\', '\\\\').replace("'", "''")


def rq(s):
    out = subprocess.run([MYSQL, '-uroot', '-h127.0.0.1', '-N', '-B', '--default-character-set=utf8', 'world', '-e', s],
                         capture_output=True, check=True).stdout.decode('utf-8')
    return [l.split('\t') for l in out.replace('\r', '').split('\n') if l]


texte = open(os.path.join(ICI, 'choix_apparats.txt'), encoding='utf-8').read()
choix = {int(a): int(b) for a, b in re.findall(r'(\d+)=(\d+)', texte)}
retenus = {i: o for i, o in sorted(choix.items()) if o > 0}
items = D2O(os.path.join(CB, 'eliocalypse', 'client', 'data', 'common', 'Items.d2o'))
deja = [i for i in retenus if items.objet(i) is not None]
assert not deja, f'objets déjà présents : {deja}'
textes, prochain = {}, [TEXTE0]


def nouveau_texte(t):
    n = prochain[0]; prochain[0] += 1; textes[str(n)] = t or ''; return n


lignes = []
for i, os_choisi in retenus.items():
    v = IN[i]
    mid = MODELES[v['typeId']]
    o = dict(items.objet(mid))
    nom, desc = T.get(v['nameId']), T.get(v['descriptionId'])
    o.update(id=i, nameId=nouveau_texte(nom), descriptionId=nouveau_texte(desc), iconId=v['iconId'])
    items.poser(i, o)
    lignes.append((i, mid, nom, o['nameId'], o['descriptionId'], v['iconId'], os_choisi))
os.makedirs(os.path.join(SORTIE, 'common'), exist_ok=True)
items.ecrire(os.path.join(SORTIE, 'common', 'Items.d2o'))
json.dump(textes, open(os.path.join(ICI, 'textes_lot2_familiers.json'), 'w', encoding='utf-8'), ensure_ascii=False)
i18n = os.path.join(SORTIE, 'i18n', 'i18n_fr.d2i')
subprocess.run([sys.executable, '-I', os.path.join(CB, 'd2i_ajout.py'), i18n, i18n + '.tmp', os.path.join(ICI, 'textes_lot2_familiers.json')], check=True)
shutil.move(i18n + '.tmp', i18n)

# serveur : copie de la ligne du modèle (mêmes effets), id / nom / textes / icône / apparence changés
cols = [c for (c, *_r) in rq("SHOW COLUMNS FROM items_templates")]
sql = ["-- PADGREF 23/10/2026 : SQL 115 — import 2.74 (lot 2) : familiers et montiliers d'apparat choisis sur la planche (objets "
       "d'apparat type 177, modèles 8071 / 15763, AppearanceId = os choisi). Généré par client-build/import274/lot2_import_familiers.py.",
       "-- Base world. Rejouable.", "SET NAMES utf8mb4;", "START TRANSACTION;",
       f"DELETE FROM items_templates WHERE Id IN ({', '.join(str(l[0]) for l in lignes)});"]
for i, mid, nom, nid, did, icone, os_choisi in lignes:
    val = {'Id': str(i), 'Name': f"'{q(nom)}'", 'NameId': str(nid), 'DescriptionId': str(did), 'IconId': str(icone), 'AppearanceId': str(os_choisi)}
    sql.append(f"INSERT INTO items_templates ({', '.join('`' + c + '`' for c in cols)}) SELECT {', '.join(val.get(c, '`' + c + '`') for c in cols)} "
               f"FROM items_templates WHERE Id = {mid};")
sql.append("COMMIT;")
os.makedirs(SQLDIR, exist_ok=True)
open(os.path.join(SQLDIR, '115_import274_familiers_apparat.sql'), 'w', encoding='utf-8', newline='\n').write('\n'.join(sql) + '\n')
open(os.path.join(SQLDIR, '115_import274_familiers_apparat-annuler.sql'), 'w', encoding='utf-8', newline='\n').write(
    "-- PADGREF 23/10 : annule 115_import274_familiers_apparat.sql\n"
    f"DELETE FROM items_templates WHERE Id IN ({', '.join(str(l[0]) for l in lignes)});\n")
json.dump({'importes': [l[0] for l in lignes], 'aucun': sorted(i for i, o in choix.items() if o == 0)},
          open(os.path.join(ICI, 'lot2_familiers_importes.json'), 'w'), indent=0)
print('importés :', len(lignes), '| familiers', sum(1 for l in lignes if l[1] == 8071), '| montiliers', sum(1 for l in lignes if l[1] == 15763),
      '| sans dessin (pas importés) :', sum(1 for o in choix.values() if o == 0))
