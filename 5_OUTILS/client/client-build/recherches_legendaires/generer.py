# PADGREF 23/10 (demande utilisateur : « fais les 16 chasses légendaires manquantes, fais bien leur carte etc. ») : import du client 2.74
# (_OUTILS/client/client_27x) des 16 chasses légendaires restantes (recherchés hors Frigost : Ogivol Scalarcin, Marzwel le Gobelin,
# Qil Bil, Tyranne la Terrible, Musha L'Oni, Simbadas, Rok Gnorok, Naganita, Trukipik, Carlita de l'Aguerfelde, Nenufor Tilotus,
# Padgref Demoël, Fojumo, Shushu Debruk'Sayl, Predagob, Aermyne) : chasse, carte (recette avec ses fragments), fragments (lâchés par
# les monstres de la zone, taux 2.74), coffre (Roses des sables + 1 ressource au nom du recherché, voir ChasseManager.OuvrirCoffre).
# En 2.74 ces ressources ne servent à aucune recette (pas de panoplie). Même méthode que recherches_frigost/generer.py.
# Entrée : contenu.json (extraire.py). Bases = derniers fichiers de chaque chaîne (import 2.74, chasses de Frigost, textes des succès
# Eliocalypse, icônes bitmap1_2 / vector1_2). Sorties : client/ (data/common/*.d2o, data/i18n/i18n_fr.d2i, content/gfx/items/*1_2.d2p),
# _OUTILS/serveur/recherches/137_chasses_legendaires_16.sql (+ -annuler), liste.txt
import json, os, struct, subprocess, sys, glob
sys.stdout.reconfigure(encoding='utf-8')
ICI = os.path.dirname(os.path.abspath(__file__))
CB = os.path.dirname(ICI)
RACINE = os.path.abspath(os.path.join(CB, '..', '..', '..'))
sys.path.insert(0, CB)
sys.path.insert(0, os.path.join(RACINE, '_OUTILS', 'serveur', 'donjons'))
from d2o_ecrire import D2O
import maps_elements as me
import d2o, d2i

APP = os.path.join(RACINE, 'CLIENT_64bits', 'app')
N = os.path.join(RACINE, '_OUTILS', 'client', 'client_27x')
BASE_OBJETS = os.path.join(CB, 'import274', 'client', 'data', 'common')            # Items / Recipes les plus récents
BASE_CHASSES = os.path.join(CB, 'recherches_frigost', 'client', 'LegendaryTreasureHunts.d2o')
BASE_D2I = os.path.join(CB, 'succes_eliocalypse', 'client', 'data', 'i18n', 'i18n_fr.d2i')
BASE_ICONES = os.path.join(CB, 'import274', 'client', 'content', 'gfx', 'items')   # fin de chaîne : bitmap1_2 / vector1_2
SORTIE = os.path.join(ICI, 'client')
SQLDIR = os.path.join(RACINE, '_OUTILS', 'serveur', 'recherches')
MYSQL = os.path.join(RACINE, '_OUTILS', 'logiciels', 'MariaDB', 'bin', 'mysql.exe')
BIN = os.path.join(RACINE, '_OUTILS', 'serveur', 'panoplies', 'bin')
TEXTE0 = 9110000
TYPES = {228: 60}          # Liquide (2.74) -> Huile
ICONES = {32571: 15875}    # Joyau des Scalptaras : icône 24520 absente (même du 2.74) -> icône du Joyau de la couronne du roi Nidas


def rq(s):
    out = subprocess.run([MYSQL, '-uroot', '-h127.0.0.1', '-N', '-B', '--default-character-set=utf8', 'world', '-e', s],
                         capture_output=True, check=True).stdout.decode('utf-8')
    return [l.split('\t') for l in out.replace('\r', '').split('\n') if l]


q = lambda s: (s or '').replace('\\', '\\\\').replace("'", "''")
T = d2i.load(os.path.join(N, 'data', 'i18n', 'i18n_fr.d2i'))
lire = lambda f: d2o.load(os.path.join(N, 'data', 'common', f + '.d2o'))[1]
IN = lire('Items')
REC = {r['resultId']: r for r in lire('Recipes').values()}
contenu = json.load(open(os.path.join(ICI, 'contenu.json'), encoding='utf-8'))

items_c = D2O(os.path.join(BASE_OBJETS, 'Items.d2o'))
rec_c = D2O(os.path.join(BASE_OBJETS, 'Recipes.d2o'))
leg_c = D2O(BASE_CHASSES)

ids = sorted({i for s in contenu for i in [s['coffre'], s['carte'], s['rare']] + s['fragments']})
assert all(items_c.objet(i) is None for i in ids), 'objet déjà présent dans le client'
assert not rq(f"SELECT Id FROM items_templates WHERE Id IN ({','.join(map(str, ids))})"), 'objet déjà présent en base'
assert all(leg_c.objet(s['chasse']) is None for s in contenu), 'chasse déjà présente'

# modèles 2.51 par type (un objet officiel du même type présent dans le client)
modeles_sql = {}
for tid, iid in rq("SELECT TypeId, Id FROM items_templates WHERE NameId < 1000000 ORDER BY Id"):
    tid, iid = int(tid), int(iid)
    if tid not in modeles_sql and items_c.objet(iid) is not None:
        modeles_sql[tid] = iid
modeles_sql.update({172: 15486, 174: 15479, 175: 15480})   # coffre / carte / fragment = ceux du Trésor de Dremoan
effet_modele = items_c.objet(12117)['possibleEffects'][0]
hex_effets_modele = rq("SELECT HEX(PossibleEffectsBin) FROM items_templates WHERE Id = 12117")[0][0]

textes, langs, prochain = {}, [], [TEXTE0]


def texte(t):
    i = prochain[0]; prochain[0] += 1
    textes[str(i)] = t or ''
    langs.append((i, t or ''))
    return i


def effets_client(liste):
    res = []
    for e in liste or []:
        if e:
            x = dict(effet_modele)
            x.update(effectId=e['effectId'], diceNum=e.get('diceNum', 0), diceSide=e.get('diceSide', 0), value=e.get('value', 0))
            res.append(x)
    return res


nom_txt, desc_txt, typ = {}, {}, {}
for i in ids:
    if i in ICONES:
        IN[i] = dict(IN[i], iconId=ICONES[i])
    typ[i] = TYPES.get(IN[i]['typeId'], IN[i]['typeId'])
    assert typ[i] in modeles_sql, f'type {typ[i]} sans modèle'
    nom_txt[i] = texte(T.get(IN[i]['nameId']))
    desc_txt[i] = texte(T.get(IN[i]['descriptionId']))

cartes = sorted(s['carte'] for s in contenu)
assert all(c in REC for c in cartes), 'carte sans recette'

# ------------------------------------------------------------------ client : objets, recettes des cartes, chasses
for i in ids:
    v = IN[i]
    o = dict(items_c.objet(modeles_sql[typ[i]]))
    for k in o:
        if k in v and k not in ('__classe', 'possibleEffects', 'nameId', 'descriptionId', 'typeId', 'appearanceId', 'recipeIds',
                                'favoriteSubAreas', 'dropMonsterIds', 'containerIds', 'evolutiveEffectIds', 'itemSetId', 'recipeSlots'):
            o[k] = v[k]
    o.update(id=i, nameId=nom_txt[i], descriptionId=desc_txt[i], typeId=typ[i], appearanceId=0, itemSetId=-1,
             possibleEffects=[] if typ[i] == 172 else effets_client(v['possibleEffects']),
             recipeIds=[r for r in (v.get('recipeIds') or []) if r in cartes])
    if 'favoriteSubAreas' in o: o['favoriteSubAreas'] = []
    if 'dropMonsterIds' in o: o['dropMonsterIds'] = []
    items_c.poser(i, o)

modele_rec = rec_c.objet(12117)
for r in cartes:
    x = REC[r]
    o = dict(modele_rec)
    o.update(resultId=r, resultNameId=nom_txt[r], resultTypeId=typ[r], resultLevel=IN[r]['level'], ingredientIds=x['ingredientIds'],
             quantities=x['quantities'], jobId=x['jobId'], skillId=x['skillId'])
    rec_c.poser(r, o)

nom_chasse = {}
for s in contenu:
    nom_chasse[s['chasse']] = texte(s['nom'])
    o = dict(leg_c.objet(15))
    o.update(id=s['chasse'], nameId=nom_chasse[s['chasse']], level=s['niveau'], chestId=s['coffre'], monsterId=s['monstre'],
             mapItemId=s['carte'], xpRatio=float(s['xpRatio']))
    leg_c.poser(s['chasse'], o)

os.makedirs(os.path.join(SORTIE, 'data', 'common'), exist_ok=True)
os.makedirs(os.path.join(SORTIE, 'data', 'i18n'), exist_ok=True)
items_c.ecrire(os.path.join(SORTIE, 'data', 'common', 'Items.d2o'))
rec_c.ecrire(os.path.join(SORTIE, 'data', 'common', 'Recipes.d2o'))
leg_c.ecrire(os.path.join(SORTIE, 'data', 'common', 'LegendaryTreasureHunts.d2o'))
json.dump(textes, open(os.path.join(ICI, 'textes.json'), 'w', encoding='utf-8'), ensure_ascii=False, indent=0)
subprocess.run([sys.executable, '-I', os.path.join(CB, 'd2i_ajout.py'), BASE_D2I, os.path.join(SORTIE, 'data', 'i18n', 'i18n_fr.d2i'),
                os.path.join(ICI, 'textes.json')], check=True)

# ------------------------------------------------------------------ icônes (dans la dernière archive de chaque chaîne : *1_2.d2p)


def lire_d2p(path):
    d = open(path, 'rb').read()
    data_off, _, idx_off, idx_cnt, prop_off, prop_cnt = struct.unpack_from('>6I', d, len(d) - 24)
    ent, p = [], idx_off
    for _ in range(idx_cnt):
        n = struct.unpack_from('>H', d, p)[0]; p += 2
        nom = d[p:p + n].decode('utf-8'); p += n
        off, ln = struct.unpack_from('>ii', d, p); p += 8
        ent.append([nom, d[data_off + off:data_off + off + ln]])
    return d[:data_off], ent, d[prop_off:len(d) - 24], prop_cnt


def ecrire_d2p(path, head, ent, props, prop_cnt):
    blob, index = bytearray(), bytearray()
    for nom, c in ent:
        nb = nom.encode('utf-8')
        index += struct.pack('>H', len(nb)) + nb + struct.pack('>ii', len(blob), len(c)); blob += c
    off = len(head)
    pied = struct.pack('>6I', off, len(blob), off + len(blob), len(ent), off + len(blob) + len(index), prop_cnt)
    open(path, 'wb').write(bytes(head) + bytes(blob) + bytes(index) + bytes(props) + pied)


icones = {IN[i]['iconId'] for i in ids}
copiees = {}
os.makedirs(os.path.join(SORTIE, 'content', 'gfx', 'items'), exist_ok=True)
for arch, ext in (('bitmap', 'png'), ('vector', 'swf')):
    deja = {n for f in glob.glob(os.path.join(APP, 'content', 'gfx', 'items', arch + '*.d2p')) + glob.glob(os.path.join(BASE_ICONES, arch + '*.d2p'))
            for n, _ in me.read_d2p(f)}
    source = {}
    for f in glob.glob(os.path.join(N, 'content', 'gfx', 'items', arch + '*.d2p')):
        for n, b in me.read_d2p(f):
            if n.endswith('.' + ext) and n[:-4].isdigit() and int(n[:-4]) in icones:
                source[n] = b
    nouveaux = {n: b for n, b in source.items() if n not in deja}
    head, ent, props, pc = lire_d2p(os.path.join(BASE_ICONES, f'{arch}1_2.d2p'))
    ent = [e for e in ent if e[0] not in nouveaux] + [[k, v] for k, v in sorted(nouveaux.items())]
    ecrire_d2p(os.path.join(SORTIE, 'content', 'gfx', 'items', f'{arch}1_2.d2p'), head, ent, props, pc)
    copiees[arch] = (len(nouveaux), len(icones) - len(source))

# ------------------------------------------------------------------ serveur
lignes_eff = [f"{i}\t{hex_effets_modele}\t" + ','.join(f"{e['effectId']}:{e.get('diceNum', 0)}:{e.get('diceSide', 0)}"
                                                         for e in IN[i]['possibleEffects'] if e) for i in ids if typ[i] != 172]
out = subprocess.run([os.path.join(BIN, 'EcrireObjets.exe')], input='\n'.join(lignes_eff).encode(), capture_output=True, check=True,
                     cwd=BIN).stdout.decode()
bin_effets = {}
for l in out.splitlines():
    if l.startswith('UPDATE'):
        i = int(l.rsplit('Id = ', 1)[1].rstrip(';'))
        bin_effets[i] = l.split('PossibleEffectsBin = ', 1)[1].split(' WHERE')[0]

cols = [c for (c, *_r) in rq("SHOW COLUMNS FROM items_templates")]
sql = []
for i in ids:
    v, mid = IN[i], modeles_sql[typ[i]]
    surcharges = {
        'Id': str(i), 'Name': f"'{q(T.get(v['nameId']))}'", 'AppearanceId': '0', 'NameId': str(nom_txt[i]), 'TypeId': str(typ[i]),
        'DescriptionId': str(desc_txt[i]), 'IconId': str(v['iconId']), 'Level': str(v['level']), 'Price': str(v.get('price', 0)),
        'Weight': str(v.get('realWeight', 1)), 'RealWeight': str(v.get('realWeight', 1)), 'ItemSetId': '-1',
        'Criteria': f"'{q(v.get('criteria') or '')}'", 'RecipeIdsCSV': f"'{','.join(map(str, [r for r in (v.get('recipeIds') or []) if r in cartes]))}'",
        'FavoriteSubAreasCSV': "''", 'PossibleEffectsBin': bin_effets.get(i, 'PossibleEffectsBin'), 'Etheral': '0', 'Cursed': '0', 'TwoHanded': '0'}
    sql.append(f"INSERT INTO items_templates SELECT {', '.join(surcharges.get(c, f'`{c}`') for c in cols)} FROM items_templates WHERE Id = {mid};")
for r in cartes:
    x = REC[r]
    qte = ','.join(map(str, x['quantities'])).encode().hex().upper()
    sql.append(f"INSERT INTO recipes (Id, ResultNameId, ResultTypeId, ResultLevel, IngredientIdsCSV, QuantitiesBin, JobId, SkillId) VALUES "
               f"({r}, {nom_txt[r]}, {typ[r]}, {IN[r]['level']}, '{','.join(map(str, x['ingredientIds']))}', 0x{qte}, {x['jobId']}, {x['skillId']});")
for s in contenu:
    sql.append(f"INSERT INTO mystic_chasses_legendaires (Id, NameId, Niveau, CoffreId, MonstreId, CarteId, RatioXp, RessourceId) VALUES "
               f"({s['chasse']}, {nom_chasse[s['chasse']]}, {s['niveau']}, {s['coffre']}, {s['monstre']}, {s['carte']}, {float(s['xpRatio'])}, {s['rare']});")
drops = []
for s in contenu:
    for f, ds in s['dropsFragments'].items():
        for d in ds:
            if d['dans251']:
                t = d['taux']
                drops.append(f"({d['monstre']}, {f}, 1, {t[0]}, {t[1]}, {t[2]}, {t[3]}, {t[4]}, 1, 0, NULL, 0, 1, 0)")
idl = ','.join(map(str, ids))
chl = ','.join(str(s['chasse']) for s in contenu)
efface = [f"DELETE FROM items_templates WHERE Id IN ({idl});", f"DELETE FROM recipes WHERE Id IN ({','.join(map(str, cartes))});",
          f"DELETE FROM mystic_chasses_legendaires WHERE Id IN ({chl});", f"DELETE FROM monsters_drops WHERE ItemId IN ({idl});",
          f"DELETE FROM langs WHERE Id BETWEEN {TEXTE0} AND {TEXTE0 + 9999};"]
corps = [
    "-- PADGREF 23/10/2026 : SQL 137 — 16 chasses légendaires 2.74 restantes (généré par _OUTILS/client/client-build/recherches_legendaires/generer.py) :",
    "-- " + ', '.join(s['nom'] for s in contenu) + '.',
    f"-- Carte (recette : fragments), {sum(len(s['fragments']) for s in contenu)} fragments lâchés par les monstres de la zone (taux 2.74), coffre",
    "-- (Roses des sables + 1 ressource au nom du recherché). Objets " + f"{min(ids)} à {max(ids)} (Id du 2.74), textes {TEXTE0}+. Base world. Rejouable.",
    "-- Client : Items.d2o, Recipes.d2o, LegendaryTreasureHunts.d2o, i18n_fr.d2i, items/bitmap1_2.d2p + vector1_2.d2p.",
    "SET NAMES utf8mb4;", "START TRANSACTION;"] + efface + [
    "INSERT INTO langs (Id, French) VALUES " + ",\n ".join(f"({i}, '{q(t)}')" for i, t in langs) + ";"] + sql + [
    "INSERT INTO monsters_drops (MonsterOwnerId, ItemId, DropLimit, DropRateForGrade1, DropRateForGrade2, DropRateForGrade3, DropRateForGrade4, "
    "DropRateForGrade5, RollsCounter, ProspectingLock, `Condition`, DropGroup, TaxCollectorCannotLoot, TauxFixe) VALUES\n" + ",\n".join(drops) + ";",
    "COMMIT;"]
open(os.path.join(SQLDIR, '137_chasses_legendaires_16.sql'), 'w', encoding='utf-8', newline='\r\n').write('\n'.join(corps) + '\n')
open(os.path.join(SQLDIR, '137_chasses_legendaires_16-annuler.sql'), 'w', encoding='utf-8', newline='\r\n').write('\n'.join(
    ["-- PADGREF 23/10/2026 : annule 137_chasses_legendaires_16.sql", "START TRANSACTION;"] + efface + ["COMMIT;"]) + '\n')
liste = [f"{s['chasse']} {s['nom']} (niv {s['niveau']}) : combat {s['nomMonstre']} ({s['monstre']}) ; carte {s['carte']} = {len(s['fragments'])} fragments ; "
         f"coffre {s['coffre']} -> Roses des sables + {s['nomRare']}" for s in contenu]
open(os.path.join(ICI, 'liste.txt'), 'w', encoding='utf-8').write('\n'.join(liste) + '\n')
print('ok :', len(ids), 'objets,', len(cartes), 'recettes de carte,', len(contenu), 'chasses,', len(drops), 'drops de fragments ; icônes', copiees,
      '; textes', len(langs))
