# PADGREF 23/10 (import 2.74, lot 2, choix de l'utilisateur sur PLANCHE_SKINS.html, entrée choix_skins.txt) :
#  - APPARATS 2.74 (coiffes 246, capes 247, boucliers 248, armes 251) -> objets d'apparat 2.51 (type 177) sur le modèle d'un de nos
#    apparats de même emplacement (effet 1179 « Compatible avec : #1 » de même valeur : 16 coiffe, 17 cape, 82 bouclier, type d'arme),
#    AppearanceId = skin choisi ;
#  - ÉQUIPEMENTS déjà importés sans apparence (recherchés de Frigost, Cavaliers de l'Eliocalypse) : AppearanceId = skin choisi.
# Objets à 0 / non remplis : pas touchés (2e recherche). Client : Items.d2o (base : sortie du lot 2 familiers) + textes 9090000+.
# Serveur : SQL 116 (+ annuler).
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
TEXTE0 = int(sys.argv[3]) if len(sys.argv) > 3 else 9090000
CHOIX = sys.argv[1] if len(sys.argv) > 1 else 'choix_skins.txt'
NUM = sys.argv[2] if len(sys.argv) > 2 else '116'
MODELES = {16: 1090, 17: 2517, 82: 9481, 2: 14590, 3: 14614, 4: 9417, 5: 10187, 6: 8676, 7: 9403, 8: 12752, 19: 9926}
T = d2i.load(os.path.join(N, 'i18n', 'i18n_fr.d2i'))
IN = d2o.load(os.path.join(N, 'common', 'Items.d2o'))[1]
q = lambda s: (s or '').replace('\\', '\\\\').replace("'", "''")


def rq(s):
    out = subprocess.run([MYSQL, '-uroot', '-h127.0.0.1', '-N', '-B', '--default-character-set=utf8', 'world', '-e', s],
                         capture_output=True, check=True).stdout.decode('utf-8')
    return [l.split('\t') for l in out.replace('\r', '').split('\n') if l]


choix = {int(a): int(b) for a, b in re.findall(r'(\d+)=(\d+)', open(os.path.join(ICI, CHOIX), encoding='utf-8').read())}
retenus = {i: s for i, s in sorted(choix.items()) if s > 0}
equip = {int(i): t for i, t in rq("SELECT Id, 'objet' FROM items_templates WHERE NameId >= 9000000 UNION SELECT Id, 'arme' FROM items_templates_weapons "
                                  "WHERE NameId >= 9000000")}
items = D2O(os.path.join(SORTIE, 'common', 'Items.d2o'))
textes, prochain = {}, [TEXTE0]


def nouveau_texte(t):
    n = prochain[0]; prochain[0] += 1; textes[str(n)] = t or ''; return n


apparats, equipements, sans_modele = [], [], []
for i, skin in retenus.items():
    if i in equip:
        equipements.append((i, equip[i], skin)); continue
    v = IN[i]
    assert v['typeId'] in (246, 247, 248, 251) and items.objet(i) is None, i
    val = [e['value'] for e in v['possibleEffects'] if e['effectId'] == 1179]
    mid = MODELES.get(val[0] if val else None)
    if mid is None:
        sans_modele.append(i); continue
    o = dict(items.objet(mid))
    nom = T.get(v['nameId'])
    o.update(id=i, nameId=nouveau_texte(nom), descriptionId=nouveau_texte(T.get(v['descriptionId'])), iconId=v['iconId'])
    items.poser(i, o)
    apparats.append((i, mid, nom, o['nameId'], o['descriptionId'], v['iconId'], skin))
items.ecrire(os.path.join(SORTIE, 'common', 'Items.d2o'))
json.dump(textes, open(os.path.join(ICI, f'textes_lot2_skins_{NUM}.json'), 'w', encoding='utf-8'), ensure_ascii=False)
i18n = os.path.join(SORTIE, 'i18n', 'i18n_fr.d2i')
subprocess.run([sys.executable, '-I', os.path.join(CB, 'd2i_ajout.py'), i18n, i18n + '.tmp', os.path.join(ICI, f'textes_lot2_skins_{NUM}.json')], check=True)
shutil.move(i18n + '.tmp', i18n)

cols = [c for (c, *_r) in rq("SHOW COLUMNS FROM items_templates")]
ids_app = ', '.join(str(a[0]) for a in apparats)
sql = [f"-- PADGREF 23/10/2026 : SQL {NUM} — import 2.74 (lot 2) : coiffes / capes / boucliers / armes d'apparat choisis sur la planche des",
       "-- skins (objets d'apparat type 177 sur le modèle d'un apparat 2.51 de même emplacement, AppearanceId = skin choisi) + dessin des",
       "-- équipements importés sans apparence (recherchés de Frigost, Cavaliers). Généré par client-build/import274/lot2_import_skins.py.",
       "-- Base world. Rejouable.", "SET NAMES utf8mb4;", "START TRANSACTION;"] + ([f"DELETE FROM items_templates WHERE Id IN ({ids_app});"] if apparats else [])
for i, mid, nom, nid, did, icone, skin in apparats:
    val = {'Id': str(i), 'Name': f"'{q(nom)}'", 'NameId': str(nid), 'DescriptionId': str(did), 'IconId': str(icone), 'AppearanceId': str(skin)}
    sql.append(f"INSERT INTO items_templates ({', '.join('`' + c + '`' for c in cols)}) SELECT {', '.join(val.get(c, '`' + c + '`') for c in cols)} "
               f"FROM items_templates WHERE Id = {mid};")
for i, t, skin in equipements:
    sql.append(f"UPDATE {'items_templates_weapons' if t == 'arme' else 'items_templates'} SET AppearanceId = {skin} WHERE Id = {i};")
sql.append("COMMIT;")
open(os.path.join(SQLDIR, f'{NUM}_import274_apparats_skins.sql'), 'w', encoding='utf-8', newline='\n').write('\n'.join(sql) + '\n')
ann = [f"-- PADGREF 23/10 : annule {NUM}_import274_apparats_skins.sql", "START TRANSACTION;"] + ([f"DELETE FROM items_templates WHERE Id IN ({ids_app});"] if apparats else [])
ann += [f"UPDATE {'items_templates_weapons' if t == 'arme' else 'items_templates'} SET AppearanceId = 0 WHERE Id = {i};" for i, t, _ in equipements]
open(os.path.join(SQLDIR, f'{NUM}_import274_apparats_skins-annuler.sql'), 'w', encoding='utf-8', newline='\n').write('\n'.join(ann + ["COMMIT;"]) + '\n')
print('apparats importés :', len(apparats), '| équipements habillés :', len(equipements), '| sans modèle :', sans_modele)
