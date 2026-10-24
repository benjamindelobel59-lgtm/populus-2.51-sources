# PADGREF 23/10 (demande utilisateur) : Eliocalypse — OBJETS importés du client 2.74 : 4 clés (+ recettes), ressources des monstres
# et des 4 Cavaliers, équipements des Cavaliers (panoplies 474-477, butin 0,01 %), Dofus du Cauchemar. Objets de quête / événement exclus.
# Client : part des fichiers du paquet recherches-frigost (recherches_frigost/client) s'ils existent, sinon du client installé,
# pour que les deux imports se cumulent. Sorties : client/data/common/{Items,ItemSets,Recipes}.d2o, client/data/i18n/i18n_fr.d2i
# (textes cumulés : recherches + eliocalypse), client/content/gfx/items/*.d2p, SQL 98 (+ annuler), objets.json.
# Pas d'apparence portée (AppearanceId 0) pour les équipements : lien objet -> skin à faire à part.
import glob, json, os, re, subprocess, sys
sys.stdout.reconfigure(encoding='utf-8')
ICI = os.path.dirname(os.path.abspath(__file__))
CB = os.path.dirname(ICI)
R = os.path.abspath(os.path.join(CB, '..', '..', '..'))
sys.path.insert(0, CB); sys.path.insert(0, ICI)
sys.path.insert(0, os.path.join(R, '_OUTILS', 'serveur', 'donjons'))
from d2o_ecrire import D2O
import maps_elements as me
from cartes_decor_lib import lire_d2p, ecrire_d2p
import d2o, d2i

APP = os.path.join(R, 'CLIENT_64bits', 'app')
RF = os.path.join(CB, 'recherches_frigost', 'client')
N = os.path.join(R, '_OUTILS', 'client', 'client_27x')
SORTIE = os.path.join(ICI, 'client')
SQLDIR = os.path.join(R, '_OUTILS', 'serveur', 'donjons', 'eliocalypse')
MYSQL = os.path.join(R, '_OUTILS', 'logiciels', 'MariaDB', 'bin', 'mysql.exe')
BIN = os.path.join(R, '_OUTILS', 'serveur', 'panoplies', 'bin')
TEXTE0 = 9030000
CLES = [22147, 22149, 22148, 22028]                      # Trône de Sang, Sentence de la Balance, Arbre de Mort, Fers de la Tyrannie
EXCLUS_TYPES = {126, 131, 220}                           # quêtes principales, événements, « Eliocalypse » (objets de quête)
TYPES = {271: 4, 228: 60, 229: 15, 266: 15}


def rq(s):
    out = subprocess.run([MYSQL, '-uroot', '-h127.0.0.1', '-N', '-B', '--default-character-set=utf8', 'world', '-e', s],
                         capture_output=True, check=True).stdout.decode('utf-8')
    return [l.split('\t') for l in out.replace('\r', '').split('\n') if l]


q = lambda s: (s or '').replace('\\', '\\\\').replace("'", "''")
base = lambda f, sous='common': os.path.join(RF, f) if os.path.exists(os.path.join(RF, f)) else os.path.join(APP, 'data', sous, f)
T = d2i.load(os.path.join(N, 'data', 'i18n', 'i18n_fr.d2i'))
lire = lambda f: d2o.load(os.path.join(N, 'data', 'common', f + '.d2o'))[1]
IN, SN, MN = lire('Items'), lire('ItemSets'), lire('Monsters')
REC = {r['resultId']: r for r in lire('Recipes').values()}
items_c, sets_c, rec_c = D2O(base('Items.d2o')), D2O(base('ItemSets.d2o')), D2O(base('Recipes.d2o'))
dn = json.load(open(os.path.join(ICI, 'donnees.json'), encoding='utf-8'))

# ------------------------------------------------------------------ objets
ids = set(CLES)
for m in dn['monstres']:
    for x in MN[m].get('drops', []):
        i = x['objectId']
        if i in IN and items_c.objet(i) is None and IN[i]['typeId'] not in EXCLUS_TYPES:
            ids.add(i)
for i in list(ids):                                       # panoplies complètes
    s = IN[i].get('itemSetId', -1)
    if s > 0:
        ids |= {x for x in SN[s]['items'] if items_c.objet(x) is None}
for c in CLES:                                            # ingrédients des clés absents
    ids |= {x for x in REC[c]['ingredientIds'] if items_c.objet(x) is None and x in IN}
ids = sorted(ids)
sets = sorted({IN[i]['itemSetId'] for i in ids if IN[i].get('itemSetId', -1) > 0})
recettes = sorted(r for r in ids if r in REC and all(items_c.objet(x) is not None or x in ids for x in REC[r]['ingredientIds']))
typ = {i: TYPES.get(IN[i]['typeId'], IN[i]['typeId']) for i in ids}

armes = {int(a) for (a,) in rq("SELECT Id FROM items_templates_weapons")}
modeles = {}
for tid, iid in rq("SELECT TypeId, Id FROM (SELECT TypeId, Id, NameId FROM items_templates UNION ALL SELECT TypeId, Id, NameId FROM items_templates_weapons) x "
                   "WHERE NameId < 1000000 ORDER BY Id"):
    if int(tid) not in modeles and items_c.objet(int(iid)) is not None:
        modeles[int(tid)] = int(iid)
modeles.update({84: 15475})                               # clé = Clef du Trône de la Cour Sombre
effet_modele = items_c.objet(12117)['possibleEffects'][0]
hex_modele = rq("SELECT HEX(PossibleEffectsBin) FROM items_templates WHERE Id = 12117")[0][0]
textes, langs, prochain = {}, [], [TEXTE0]


def texte(t):
    i = prochain[0]; prochain[0] += 1
    textes[str(i)] = t or ''; langs.append((i, t or ''))
    return i


def eff(liste):
    res = []
    for e in liste or []:
        if e:
            x = dict(effet_modele); x.update(effectId=e['effectId'], diceNum=e.get('diceNum', 0), diceSide=e.get('diceSide', 0), value=e.get('value', 0))
            res.append(x)
    return res


nt, dt = {}, {}
for i in ids:
    nt[i], dt[i] = texte(T.get(IN[i]['nameId'])), texte(T.get(IN[i]['descriptionId']))
for i in ids:
    v = IN[i]; o = dict(items_c.objet(modeles[typ[i]]))
    for k in o:
        if k in v and k not in ('__classe', 'possibleEffects', 'nameId', 'descriptionId', 'typeId', 'appearanceId', 'recipeIds', 'favoriteSubAreas',
                                'dropMonsterIds', 'itemSetId', 'containerIds', 'evolutiveEffectIds', 'recipeSlots'):
            o[k] = v[k]
    o.update(id=i, nameId=nt[i], descriptionId=dt[i], typeId=typ[i], appearanceId=0, possibleEffects=eff(v['possibleEffects']),
             itemSetId=v.get('itemSetId', -1) if v.get('itemSetId', -1) in sets else -1, recipeIds=[r for r in (v.get('recipeIds') or []) if r in recettes])
    for k in ('favoriteSubAreas', 'dropMonsterIds'):
        if k in o: o[k] = []
    items_c.poser(i, o)
ns = {}
for s in sets:
    ns[s] = texte(T.get(SN[s]['nameId']))
    o = dict(sets_c.objet(348)); o.update(id=s, items=SN[s]['items'], nameId=ns[s], bonusIsSecret=SN[s].get('bonusIsSecret', False),
                                         effects=[eff(p) for p in SN[s]['effects']])
    sets_c.poser(s, o)
mr = rec_c.objet(12117)
for r in recettes:
    x = REC[r]; o = dict(mr)
    o.update(resultId=r, resultNameId=nt[r], resultTypeId=typ[r], resultLevel=IN[r]['level'], ingredientIds=x['ingredientIds'], quantities=x['quantities'],
             jobId=x['jobId'], skillId=x['skillId'])
    rec_c.poser(r, o)
os.makedirs(os.path.join(SORTIE, 'data', 'common'), exist_ok=True); os.makedirs(os.path.join(SORTIE, 'data', 'i18n'), exist_ok=True)
items_c.ecrire(os.path.join(SORTIE, 'data', 'common', 'Items.d2o'))
sets_c.ecrire(os.path.join(SORTIE, 'data', 'common', 'ItemSets.d2o'))
rec_c.ecrire(os.path.join(SORTIE, 'data', 'common', 'Recipes.d2o'))
# textes cumulés : i18n du paquet recherches (ou client) + textes de donnees.py + textes des objets
tout = dict(json.load(open(os.path.join(ICI, 'textes.json'), encoding='utf-8'))); tout.update(textes)
json.dump(tout, open(os.path.join(ICI, 'textes_tout.json'), 'w', encoding='utf-8'), ensure_ascii=False, indent=0)
src_i18n = os.path.join(RF, 'i18n_fr.d2i') if os.path.exists(os.path.join(RF, 'i18n_fr.d2i')) else os.path.join(APP, 'data', 'i18n', 'i18n_fr.d2i')
subprocess.run([sys.executable, '-I', os.path.join(CB, 'd2i_ajout.py'), src_i18n, os.path.join(SORTIE, 'data', 'i18n', 'i18n_fr.d2i'),
                os.path.join(ICI, 'textes_tout.json')], check=True)

# icônes : depuis l'archive du paquet recherches (ou du client), + icônes 2.74 manquantes
icones = {IN[i]['iconId'] for i in ids}
for arch, ext in (('bitmap', 'png'), ('vector', 'swf')):
    src_arch = os.path.join(RF, 'items', f'{arch}1_1.d2p')
    src_arch = src_arch if os.path.exists(src_arch) else os.path.join(APP, 'content', 'gfx', 'items', f'{arch}1_1.d2p')
    deja = {n for f in glob.glob(os.path.join(APP, 'content', 'gfx', 'items', arch + '*.d2p')) if not f.endswith(f'{arch}1_1.d2p') for n, _ in me.read_d2p(f)}
    head, ent, props, pc = lire_d2p(src_arch)
    deja |= {e[0] for e in ent}
    nouveaux = {}
    for f in glob.glob(os.path.join(N, 'content', 'gfx', 'items', arch + '*.d2p')):
        for n, b in me.read_d2p(f):
            if n.endswith('.' + ext) and n[:-4].isdigit() and int(n[:-4]) in icones and n not in deja:
                nouveaux[n] = b
    ecrire_d2p(os.path.join(SORTIE, 'content', 'gfx', 'items', f'{arch}1_1.d2p'), head, ent + [[k, v] for k, v in sorted(nouveaux.items())], props, pc)

# ------------------------------------------------------------------ serveur
lignes = [f"{i}\t{hex_modele}\t" + ','.join(f"{e['effectId']}:{e.get('diceNum', 0)}:{e.get('diceSide', 0)}" for e in IN[i]['possibleEffects'] if e) for i in ids]
out = subprocess.run([os.path.join(BIN, 'EcrireObjets.exe')], input='\n'.join(lignes).encode(), capture_output=True, check=True, cwd=BIN).stdout.decode()
binaire = {int(l.rsplit('Id = ', 1)[1].rstrip(';')): l.split('PossibleEffectsBin = ', 1)[1].split(' WHERE')[0] for l in out.splitlines() if l.startswith('UPDATE')}
out = subprocess.run([os.path.join(BIN, 'Ecrire.exe')], input='\n'.join(f"{s}\t" + '|'.join(','.join(f"{e['effectId']}:{e.get('diceNum', 0)}" for e in p if e)
                     for p in SN[s]['effects']) for s in sets).encode(), capture_output=True, check=True, cwd=BIN).stdout.decode()
bsets = {int(l.rsplit('Id = ', 1)[1].rstrip(';')): l.split('EffectsBin = ', 1)[1].split(' WHERE')[0] for l in out.splitlines() if l.startswith('UPDATE')}
COL = {t: [c[0] for c in rq(f"SHOW COLUMNS FROM {t}")] for t in ('items_templates', 'items_templates_weapons')}
sql = []
for i in ids:
    v, mid = IN[i], modeles[typ[i]]
    table = 'items_templates_weapons' if mid in armes else 'items_templates'
    s = {'Id': str(i), 'Name': f"'{q(T.get(v['nameId']))}'", 'AppearanceId': '0', 'NameId': str(nt[i]), 'TypeId': str(typ[i]), 'DescriptionId': str(dt[i]),
         'IconId': str(v['iconId']), 'Level': str(v['level']), 'Price': str(v.get('price', 0)), 'Weight': str(v.get('realWeight', 1)),
         'RealWeight': str(v.get('realWeight', 1)), 'ItemSetId': str(v.get('itemSetId', -1) if v.get('itemSetId', -1) in sets else -1),
         'Criteria': f"'{q(v.get('criteria') or '')}'", 'RecipeIdsCSV': f"'{','.join(str(r) for r in (v.get('recipeIds') or []) if r in recettes)}'",
         'FavoriteSubAreasCSV': "''", 'PossibleEffectsBin': binaire.get(i, 'PossibleEffectsBin'), 'Etheral': '0', 'Cursed': '0',
         'TwoHanded': str(int(bool(v.get('twoHanded'))))}
    sql.append(f"INSERT INTO {table} SELECT {', '.join(s.get(c, '`' + c + '`') for c in COL[table])} FROM {table} WHERE Id = {mid};")
for s_ in sets:
    sql.append(f"INSERT INTO items_sets (Id, ItemsCSV, NameId, BonusIsSecret, EffectsBin) VALUES ({s_}, '{','.join(map(str, SN[s_]['items']))}', {ns[s_]}, "
               f"{int(SN[s_].get('bonusIsSecret', False))}, {bsets[s_]});")
for r in recettes:
    x = REC[r]
    sql.append(f"INSERT INTO recipes (Id, ResultNameId, ResultTypeId, ResultLevel, IngredientIdsCSV, QuantitiesBin, JobId, SkillId) VALUES ({r}, {nt[r]}, "
               f"{typ[r]}, {IN[r]['level']}, '{','.join(map(str, x['ingredientIds']))}', 0x{','.join(map(str, x['quantities'])).encode().hex().upper()}, "
               f"{x['jobId']}, {x['skillId']});")
idl = ','.join(map(str, ids))
tete = ["-- PADGREF 23/10/2026 : SQL 98 — Eliocalypse, objets importés du client 2.74 (généré par eliocalypse/objets.py) : "
        f"{len(ids)} objets ({len(CLES)} clés, ressources, équipements des Cavaliers, Dofus du Cauchemar), {len(sets)} panoplies, {len(recettes)} recettes.",
        "-- Butin : SQL suivant. Base world. Rejouable. Annulation : 98_eliocalypse_objets-annuler.sql", "SET NAMES utf8mb4;", "START TRANSACTION;",
        f"DELETE FROM items_templates WHERE Id IN ({idl});", f"DELETE FROM items_templates_weapons WHERE Id IN ({idl});",
        f"DELETE FROM items_sets WHERE Id IN ({','.join(map(str, sets))});", f"DELETE FROM recipes WHERE Id IN ({','.join(map(str, recettes))});",
        f"DELETE FROM langs WHERE Id BETWEEN {TEXTE0} AND {TEXTE0 + 9999};",
        "INSERT INTO langs (Id, French) VALUES " + ",\n ".join(f"({i}, '{q(t)}')" for i, t in langs) + ";"]
open(os.path.join(SQLDIR, '98_eliocalypse_objets.sql'), 'w', encoding='utf-8', newline='\n').write('\n'.join(tete + sql + ['COMMIT;']) + '\n')
open(os.path.join(SQLDIR, '98_eliocalypse_objets-annuler.sql'), 'w', encoding='utf-8', newline='\n').write('\n'.join([
    "-- PADGREF 23/10 : annule 98_eliocalypse_objets.sql", "START TRANSACTION;", f"DELETE FROM items_templates WHERE Id IN ({idl});",
    f"DELETE FROM items_templates_weapons WHERE Id IN ({idl});", f"DELETE FROM items_sets WHERE Id IN ({','.join(map(str, sets))});",
    f"DELETE FROM recipes WHERE Id IN ({','.join(map(str, recettes))});", f"DELETE FROM langs WHERE Id BETWEEN {TEXTE0} AND {TEXTE0 + 9999};", "COMMIT;"]) + '\n')
json.dump({'objets': ids, 'cles': CLES, 'panoplies': sets, 'recettes': recettes}, open(os.path.join(ICI, 'objets.json'), 'w'), indent=0)
print('ok :', len(ids), 'objets,', len(sets), 'panoplies,', len(recettes), 'recettes,', len(langs), 'textes ; recettes écartées (ingrédient absent) :',
      sorted(r for r in ids if r in REC and r not in recettes))
