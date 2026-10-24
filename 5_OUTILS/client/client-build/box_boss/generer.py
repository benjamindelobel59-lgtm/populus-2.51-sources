# PADGREF 21/10 : box de boss. Entrée : _OUTILS/serveur/box_boss/inventaire.json (inventaire.py : boss, ressources rares, panoplie).
#   Objets 31300+ (modèle 15266 « Coffre étincelant », type Coffre, utilisable, échangeable) : « Box de <boss> » ; CONSOMMÉS à l'ouverture
#   (serveur : game/items/player/Custom/BoxDeBoss.cs). Icônes 990100+ (composer.py : coffre officiel + portrait officiel du boss).
#   Textes 9001300+. Sorties : client/ (Items.d2o, i18n_fr.d2i, items/bitmap1_1.d2p, items/vector1_1.d2p), SQL 87, liste.txt.
import json, os, struct, subprocess, sys
sys.stdout.reconfigure(encoding='utf-8')
ICI = os.path.dirname(os.path.abspath(__file__))
CB = os.path.dirname(ICI)
RACINE = os.path.abspath(os.path.join(CB, '..', '..', '..'))
sys.path.insert(0, CB); sys.path.insert(0, ICI); sys.path.insert(0, os.path.join(CB, 'objets_infinis'))
from d2o_ecrire import D2O
from composer import icone
APP = os.path.join(RACINE, 'CLIENT_64bits', 'app')
SORTIE = os.path.join(ICI, 'client'); os.makedirs(SORTIE, exist_ok=True)
ICONES = os.path.join(ICI, 'icones'); os.makedirs(ICONES, exist_ok=True)
MYSQL = os.path.join(RACINE, '_OUTILS', 'logiciels', 'MariaDB', 'bin', 'mysql.exe')
ID0, ICONE0, TEXTE0, MODELE = 31300, 990100, 9001300, 15266
# boss sans rien à eux : on reprend la box d'un autre boss du même combat
MEME_BOX = {1072: 1086, 1085: 1086, 1087: 1086}          # Tynril Consterné / Déconcerté / Ahuri -> Tynril Perfide
EXCLUS = {5666, 5664, 5653}                               # contenus post-2.51 sans textes dans notre client
# PADGREF 21/10 : panoplies choisies par l'utilisateur (0 = pas de panoplie : les 5 % donnent 5 ressources rares)
PANO_CHOISIE = {289: 8, 3648: 331, 3652: 337, 4453: 958, 4444: 414, 943: 64,
                3556: 0, 3752: 0, 3232: 0, 792: 0, 605: 0, 568: 0, 612: 0, 1071: 0, 800: 0, 1086: 0}


def rq(s):
    out = subprocess.run([MYSQL, '-uroot', '-h127.0.0.1', '-N', '-B', '--default-character-set=utf8', 'world', '-e', s],
                         capture_output=True, check=True).stdout.decode('utf-8')
    return [l.split('\t') for l in out.replace('\r', '').split('\n') if l]


inv = json.load(open(os.path.join(RACINE, '_OUTILS', 'serveur', 'box_boss', 'inventaire.json'), encoding='utf-8'))
items_cli = D2O(os.path.join(APP, 'data', 'common', 'Items.d2o'))
officiels = {int(a) for a, n in rq("SELECT Id, NameId FROM (SELECT Id, NameId, Level FROM items_templates UNION ALL SELECT Id, NameId, Level FROM items_templates_weapons) AS x") if int(n) < 1000000}
noms = dict((int(a), b) for a, b in rq("SELECT t.Id, l.French FROM (SELECT Id, NameId, Level FROM items_templates UNION ALL SELECT Id, NameId, Level FROM items_templates_weapons) AS t JOIN langs l ON l.Id = t.NameId"))
niv = dict((int(a), int(b)) for a, b in rq("SELECT Id, Level FROM (SELECT Id, Level FROM items_templates UNION ALL SELECT Id, Level FROM items_templates_weapons) AS x"))
ok = lambda i: i in officiels and items_cli.objet(i) is not None
sets_db = {int(i): (n, [int(x) for x in c.split(',') if x]) for i, n, c in rq("SELECT s.Id, l.French, s.ItemsCSV FROM items_sets s JOIN langs l ON l.Id = s.NameId")}
for r in inv:
    if r['boss'] in PANO_CHOISIE:
        sid = PANO_CHOISIE[r['boss']]
        r['panoplie'], r['nomPano'], r['itemsPano'] = (sid, *sets_db[sid]) if sid else (0, '', [])
        r['choisie'] = True

boites, vus = [], {}
for r in inv:
    b = r['boss']
    if b in EXCLUS or b in MEME_BOX or b in vus:
        continue
    rares = [i for i in r['rares'] if ok(i)]
    pano = [i for i in r['itemsPano'] if ok(i)] if r['panoplie'] and r['nomPano'] and len(r['itemsPano']) >= 2 else []
    if not rares and not pano:
        continue
    vus[b] = len(boites)
    boites.append(dict(boss=[b], nom=r['nomBoss'], donjon=r['nomDonjon'], rares=rares, pano=pano, nomPano=r['nomPano'] if pano else '',
                       sur=bool(pano) and (r.get('choisie') or r['nomBoss'].lower() in r['nomPano'].lower())))
for b, cible in MEME_BOX.items():
    if cible in vus:
        boites[vus[cible]]['boss'].append(b)

textes, sql, langs, liste = {}, [], [], []
q = lambda s: s.replace("'", "''")
m = items_cli.objet(MODELE)
for i, bx in enumerate(boites):
    iid, ic, tn, td = ID0 + i, ICONE0 + i, TEXTE0 + 2 * i, TEXTE0 + 2 * i + 1
    nom = f"Box de {bx['nom']}"
    lr = ', '.join(noms.get(x, str(x)) for x in bx['rares']) or 'aucune'
    desc = (f"Butin du donjon {bx['donjon']}." if bx['donjon'] and bx['donjon'] != 'NULL' else "Butin de donjon.") + \
           "\nÀ ouvrir (la box disparaît) :\n• 50 % : 1 ressource rare\n• 25 % : 2 ressources rares\n• 20 % : 3 ressources rares\n" + \
           (f"• 5 % : 1 objet de la {bx['nomPano']}\n• JACKPOT 0,0001 % : la panoplie entière !" if bx['pano'] else "• 5 % : 5 ressources rares") + \
           f"\nRessources rares : {lr}."
    level = max([niv.get(x, 1) for x in bx['rares'] + bx['pano']] or [1])
    textes[str(tn)], textes[str(td)] = nom, desc
    items_cli.poser(iid, dict(m, id=iid, nameId=tn, descriptionId=td, iconId=ic, level=level, possibleEffects=[], price=1.0,
                              usable=True, exchangeable=True, criteria='', needUseConfirm=False))
    icone(bx['boss'][0]).save(os.path.join(ICONES, f'{ic}.png'), optimize=True)
    sql.append(f"DELETE FROM items_templates WHERE Id = {iid};")
    sql.append(f"INSERT INTO items_templates SELECT {iid}, '{q(nom)}', `Type`, AppearanceId, Weight, RealWeight, {tn}, TypeId, {td}, {ic}, {level}, Cursed, "
               f"UseAnimationId, 1, Targetable, 1, TwoHanded, Etheral, -1, '', HideEffects, '', '', "
               f"BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, 0 FROM items_templates WHERE Id = {MODELE};")
    sql.append(f"INSERT INTO padgref_box_boss (ItemId, BossIdsCSV, RaresCSV, PanoplieCSV) VALUES ({iid}, '{','.join(map(str, bx['boss']))}', "
               f"'{','.join(map(str, bx['rares']))}', '{','.join(map(str, bx['pano']))}');")
    langs += [(tn, nom), (td, desc)]
    liste.append(f"{iid}  {nom:34} rares : {lr[:70]:70} | {bx['nomPano'] or '(pas de panoplie : 5 % = 5 ressources)'}"
                 )   # PADGREF 21/10 : toutes les panoplies relues et validées par l'utilisateur

items_cli.ecrire(os.path.join(SORTIE, 'Items.d2o'))
json.dump(textes, open(os.path.join(ICI, 'textes.json'), 'w', encoding='utf-8'), ensure_ascii=False, indent=1)
subprocess.run([sys.executable, '-I', os.path.join(CB, 'd2i_ajout.py'), os.path.join(APP, 'data', 'i18n', 'i18n_fr.d2i'),
                os.path.join(SORTIE, 'i18n_fr.d2i'), os.path.join(ICI, 'textes.json')], check=True)

# icônes (png + swf) ajoutées à la dernière archive de chaque chaîne (même méthode que objets_infinis/generer_objets.py)
import importlib.util
spec = importlib.util.spec_from_file_location('fswf', os.path.join(CB, 'objets_infinis', 'faire_swf.py'))
src_swf = open(os.path.join(CB, 'objets_infinis', 'faire_swf.py'), encoding='utf-8').read().split('\nfor f in sorted')[0]
ns = {'__file__': os.path.join(CB, 'objets_infinis', 'faire_swf.py')}; exec(src_swf, ns)
for f in os.listdir(ICONES):
    if f.endswith('.png'):
        open(os.path.join(ICONES, f[:-4] + '.swf'), 'wb').write(ns['swf'](open(os.path.join(ICONES, f), 'rb').read(), 120))


def lire(path):
    d = open(path, 'rb').read()
    data_off, _, idx_off, idx_cnt, prop_off, prop_cnt = struct.unpack_from('>6I', d, len(d) - 24)
    ent, p = [], idx_off
    for _ in range(idx_cnt):
        n = struct.unpack_from('>H', d, p)[0]; p += 2
        nom = d[p:p + n].decode('utf-8'); p += n
        off, ln = struct.unpack_from('>ii', d, p); p += 8
        ent.append([nom, d[data_off + off:data_off + off + ln]])
    return d[:data_off], ent, d[prop_off:len(d) - 24], prop_cnt


def ecrire(path, head, ent, props, prop_cnt):
    blob, index = bytearray(), bytearray()
    for nom, c in ent:
        nb = nom.encode('utf-8')
        index += struct.pack('>H', len(nb)) + nb + struct.pack('>ii', len(blob), len(c)); blob += c
    off = len(head)
    pied = struct.pack('>6I', off, len(blob), off + len(blob), len(ent), off + len(blob) + len(index), prop_cnt)
    open(path, 'wb').write(bytes(head) + bytes(blob) + bytes(index) + bytes(props) + pied)


for archive, ext in (('bitmap1_1.d2p', 'png'), ('vector1_1.d2p', 'swf')):
    head, ent, props, pc = lire(os.path.join(APP, 'content', 'gfx', 'items', archive))
    nouveaux = {f'{ICONE0 + i}.{ext}': open(os.path.join(ICONES, f'{ICONE0 + i}.{ext}'), 'rb').read() for i in range(len(boites))}
    ent = [e for e in ent if e[0] not in nouveaux] + [[k, v] for k, v in nouveaux.items()]
    ecrire(os.path.join(SORTIE, archive), head, ent, props, pc)

entete = ["-- PADGREF 21/10/2026 : SQL 87 — box de boss (généré par _OUTILS/client/client-build/box_boss/generer.py).",
          f"-- {len(boites)} box (objets {ID0} à {ID0 + len(boites) - 1}). Base world. Rejouable. Serveur : game/items/player/Custom/BoxDeBoss.cs.",
          "-- Client : Items.d2o + i18n_fr.d2i + items/bitmap1_1.d2p + items/vector1_1.d2p (dossier box_boss/client).",
          "SET NAMES utf8mb4;", "USE `world`;", "START TRANSACTION;",
          "CREATE TABLE IF NOT EXISTS padgref_box_boss (ItemId INT NOT NULL PRIMARY KEY, BossIdsCSV VARCHAR(255) NOT NULL DEFAULT '', "
          "RaresCSV VARCHAR(1000) NOT NULL DEFAULT '', PanoplieCSV VARCHAR(1000) NOT NULL DEFAULT '');",
          "DELETE FROM padgref_box_boss;",
          f"DELETE FROM items_templates WHERE Id BETWEEN {ID0} AND {ID0 + 199};"]
lg = [f"DELETE FROM langs WHERE Id BETWEEN {TEXTE0} AND {TEXTE0 + 399};",
      "INSERT INTO langs (Id, French) VALUES " + ",\n ".join(f"({i}, '{q(t)}')" for i, t in langs) + ";"]
fin = ["COMMIT;", f"SELECT COUNT(*) AS box FROM padgref_box_boss;"]
open(os.path.join(RACINE, '_OUTILS', 'serveur', 'sql', '87_box_boss.sql'), 'w', encoding='utf-8', newline='\n').write('\n'.join(entete + sql + lg + fin) + '\n')
open(os.path.join(ICI, 'liste.txt'), 'w', encoding='utf-8').write('\n'.join(liste) + '\n')
print('ok', len(boites), 'box ;', sum(1 for b in boites if b['pano'] and not b['sur']), 'panoplies à vérifier ;',
      sum(1 for b in boites if not b['pano']), 'sans panoplie')
