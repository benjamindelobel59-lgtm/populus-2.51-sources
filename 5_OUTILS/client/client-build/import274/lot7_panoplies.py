# PADGREF 23/10 (import 2.74, point 6 : choix de l'utilisateur sur PLANCHE_PANOPLIES.html, 29 panoplies) :
#   - panoplies absentes de la base (995, 979, 965, 950, 997, 952, 953, 962, 59, 502) : créées (items_sets + ItemSetId des objets) ;
#   - objets absents (Mominotor 59, Directeur Grunob 502) : créés depuis le 2.74 (modèle 2.51 par type, comme eliocalypse/objets.py) ;
#   - recettes absentes : créées (Mominotor, Grunob, Perruque du Poolay, Eliocalypse 22188 / 22191 / 22202 / 22209) ;
#   - remplacements de ressources : choix de l'utilisateur (Galet rayonnant 29444 -> Galet Solaire 13367, 9 recettes) + Eliocalypse :
#     Écaille de Torkélonia (21207, boss absent) -> ressource de boss de Frigost (même quantité) ;
#   - panoplies déjà en base (Frigost, Eliocalypse…) : rien à refaire. Cape de Glourdorak (32237) et Manteau de Servitude (22201) restent
#     hors jeu (pas de dessin, SQL 120).
# Client : Items.d2o (import274, le plus récent), ItemSets.d2o / Recipes.d2o (eliocalypse, les plus récents), i18n cumulé (import274).
# Sortie : import274/client/data/... + _OUTILS/serveur/import274/127_panoplies_274.sql (+ annuler).
import json, os, shutil, subprocess, sys
sys.stdout.reconfigure(encoding='utf-8')
ICI = os.path.dirname(os.path.abspath(__file__))
CB = os.path.dirname(ICI)
R = os.path.abspath(os.path.join(CB, '..', '..', '..'))
sys.path.insert(0, CB); sys.path.insert(0, os.path.join(R, '_OUTILS', 'serveur', 'donjons'))
from d2o_ecrire import D2O
import d2o, d2i
N = os.path.join(R, '_OUTILS', 'client', 'client_27x')
SORTIE = os.path.join(ICI, 'client', 'data')
ELIO = os.path.join(CB, 'eliocalypse', 'client', 'data', 'common')
SQLDIR = os.path.join(R, '_OUTILS', 'serveur', 'import274')
MYSQL = os.path.join(R, '_OUTILS', 'logiciels', 'MariaDB', 'bin', 'mysql.exe')
BIN = os.path.join(R, '_OUTILS', 'serveur', 'panoplies', 'bin')
TEXTE0 = 9099000
CHOIX = [1034, 995, 1035, 1036, 979, 1037, 1038, 1039, 965, 1041, 1040, 1043, 950, 1044, 1045, 1046, 1047, 1048, 997, 952, 953, 59, 1042, 502, 962,
         474, 475, 476, 477]
REMPLACE = {(r, 29444): 13367 for r in (11600, 11626, 11631, 32212, 32213, 32217, 11757, 32223, 32221)}      # choix utilisateur
REMPLACE.update({(22188, 21207): 13923,     # Corset de Misère : Moustache de Klime
                 (22191, 21207): 13982,     # Forteresse de Guerre : Laine de Sylargh
                 (22202, 21207): 13947,     # Dora de Servitude : Broderie de Nileza
                 (22209, 21207): 13935})    # Ceinturonce de Corruption : Poil d'aisselle de Missiz Frizz
HORS_JEU = {32237, 22201}


def rq(s):
    out = subprocess.run([MYSQL, '-uroot', '-h127.0.0.1', '-N', '-B', '--default-character-set=utf8', 'world', '-e', s],
                         capture_output=True, check=True).stdout.decode('utf-8')
    return [l.split('\t') for l in out.replace('\r', '').split('\n') if l]


q = lambda s: (s or '').replace('\\', '\\\\').replace("'", "''")
T = d2i.load(os.path.join(N, 'data', 'i18n', 'i18n_fr.d2i'))
lire = lambda f: d2o.load(os.path.join(N, 'data', 'common', f + '.d2o'))[1]
IN, SN = lire('Items'), lire('ItemSets')
REC = {r['resultId']: r for r in lire('Recipes').values()}
F_ITEMS = os.path.join(SORTIE, 'common', 'Items.d2o')
F_I18N = os.path.join(SORTIE, 'i18n', 'i18n_fr.d2i')
sauve = os.path.join(R, 'SAVE', '3_CLIENT_64bits', '2026-10-23_panoplies-274-avant')
os.makedirs(sauve, exist_ok=True)
for f in (F_ITEMS, F_I18N, os.path.join(ELIO, 'ItemSets.d2o'), os.path.join(ELIO, 'Recipes.d2o')):
    if not os.path.exists(os.path.join(sauve, os.path.basename(f))):
        shutil.copy2(f, sauve)
src = lambda f: os.path.join(sauve, f)           # toujours partir des versions d'avant ce script (rejouable)
items_c, sets_c, rec_c = D2O(src('Items.d2o')), D2O(src('ItemSets.d2o')), D2O(src('Recipes.d2o'))

en_base = {int(i) for (i,) in rq('SELECT Id FROM items_templates UNION SELECT Id FROM items_templates_weapons')}
sets_base = {int(i) for (i,) in rq('SELECT Id FROM items_sets')}
rec_base = {int(i) for (i,) in rq('SELECT Id FROM recipes')}
armes = {int(a) for (a,) in rq('SELECT Id FROM items_templates_weapons')}

nouv_sets = [s for s in CHOIX if s not in sets_base]
nouv_objets = sorted(i for s in CHOIX for i in SN[s]['items'] if i not in en_base)
objets_sets = sorted(i for s in nouv_sets for i in SN[s]['items'] if i not in HORS_JEU)
nouv_rec = sorted(i for s in CHOIX for i in SN[s]['items'] if i in REC and i not in rec_base and i not in HORS_JEU)
base_ou_nouveau = en_base | set(nouv_objets)


def ingredients(r):
    ids = [REMPLACE.get((r, g), g) for g in REC[r]['ingredientIds']]
    return ids, REC[r]['quantities']


for r in nouv_rec:
    manque = [g for g in ingredients(r)[0] if g not in base_ou_nouveau]
    assert not manque, (r, manque)

# ------------------------------------------------------------------ modèles 2.51 par type (comme eliocalypse/objets.py)
modeles = {}
for tid, iid in rq("SELECT TypeId, Id FROM (SELECT TypeId, Id, NameId FROM items_templates UNION ALL SELECT TypeId, Id, NameId FROM items_templates_weapons) x "
                   "WHERE NameId < 1000000 ORDER BY Id"):
    if int(tid) not in modeles and items_c.objet(int(iid)) is not None:
        modeles[int(tid)] = int(iid)
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
for i in nouv_objets:
    nt[i], dt[i] = texte(T.get(IN[i]['nameId'])), texte(T.get(IN[i]['descriptionId']))
for i in nouv_objets:
    v = IN[i]; o = dict(items_c.objet(modeles[v['typeId']]))
    for k in o:
        if k in v and k not in ('__classe', 'possibleEffects', 'nameId', 'descriptionId', 'typeId', 'appearanceId', 'recipeIds', 'favoriteSubAreas',
                                'dropMonsterIds', 'itemSetId', 'containerIds', 'evolutiveEffectIds', 'recipeSlots'):
            o[k] = v[k]
    o.update(id=i, nameId=nt[i], descriptionId=dt[i], typeId=v['typeId'], appearanceId=0, possibleEffects=eff(v['possibleEffects']),
             itemSetId=v['itemSetId'], recipeIds=[i] if i in nouv_rec else [])
    for k in ('favoriteSubAreas', 'dropMonsterIds'):
        if k in o: o[k] = []
    items_c.poser(i, o)
for i in objets_sets:                                     # objets existants : rattachés à leur panoplie
    if i not in nouv_objets:
        o = dict(items_c.objet(i)); o['itemSetId'] = IN[i]['itemSetId']
        if i in nouv_rec:
            o['recipeIds'] = sorted(set(o.get('recipeIds') or []) | {i})
        items_c.poser(i, o)
for i in nouv_rec:
    if i not in nouv_objets and i not in objets_sets:
        o = dict(items_c.objet(i)); o['recipeIds'] = sorted(set(o.get('recipeIds') or []) | {i}); items_c.poser(i, o)
ns = {}
for s in nouv_sets:
    ns[s] = texte(T.get(SN[s]['nameId']))
    o = dict(sets_c.objet(348)); o.update(id=s, items=[i for i in SN[s]['items'] if i not in HORS_JEU], nameId=ns[s],
                                         bonusIsSecret=SN[s].get('bonusIsSecret', False), effects=[eff(p) for p in SN[s]['effects']])
    sets_c.poser(s, o)
mr = rec_c.objet(12117)
nom_rec = {i: nt.get(i) or items_c.objet(i)['nameId'] for i in nouv_rec}
for r in nouv_rec:
    ing, qt = ingredients(r)
    o = dict(mr); o.update(resultId=r, resultNameId=nom_rec[r], resultTypeId=IN[r]['typeId'], resultLevel=IN[r]['level'], ingredientIds=ing, quantities=qt,
                           jobId=REC[r]['jobId'], skillId=REC[r]['skillId'])
    rec_c.poser(r, o)
maj_rec = sorted({r for r, _ in REMPLACE if r in rec_base})
for r in maj_rec:
    o = dict(rec_c.objet(r)); o['ingredientIds'] = [REMPLACE.get((r, g), g) for g in o['ingredientIds']]; rec_c.poser(r, o)
os.makedirs(os.path.join(SORTIE, 'common'), exist_ok=True)
items_c.ecrire(F_ITEMS)
sets_c.ecrire(os.path.join(SORTIE, 'common', 'ItemSets.d2o'))
rec_c.ecrire(os.path.join(SORTIE, 'common', 'Recipes.d2o'))
json.dump(textes, open(os.path.join(ICI, 'textes_lot7.json'), 'w', encoding='utf-8'), ensure_ascii=False)
subprocess.run([sys.executable, '-I', os.path.join(CB, 'd2i_ajout.py'), src('i18n_fr.d2i'), F_I18N, os.path.join(ICI, 'textes_lot7.json')], check=True)

# ------------------------------------------------------------------ serveur
lignes = [f"{i}\t{hex_modele}\t" + ','.join(f"{e['effectId']}:{e.get('diceNum', 0)}:{e.get('diceSide', 0)}" for e in IN[i]['possibleEffects'] if e)
          for i in nouv_objets]
out = subprocess.run([os.path.join(BIN, 'EcrireObjets.exe')], input='\n'.join(lignes).encode(), capture_output=True, check=True, cwd=BIN).stdout.decode()
binaire = {int(l.rsplit('Id = ', 1)[1].rstrip(';')): l.split('PossibleEffectsBin = ', 1)[1].split(' WHERE')[0] for l in out.splitlines() if l.startswith('UPDATE')}
out = subprocess.run([os.path.join(BIN, 'Ecrire.exe')], input='\n'.join(f"{s}\t" + '|'.join(','.join(f"{e['effectId']}:{e.get('diceNum', 0)}" for e in p if e)
                     for p in SN[s]['effects']) for s in nouv_sets).encode(), capture_output=True, check=True, cwd=BIN).stdout.decode()
bsets = {int(l.rsplit('Id = ', 1)[1].rstrip(';')): l.split('EffectsBin = ', 1)[1].split(' WHERE')[0] for l in out.splitlines() if l.startswith('UPDATE')}
assert set(binaire) == set(nouv_objets) and set(bsets) == set(nouv_sets), (sorted(binaire), sorted(bsets))
COL = {t: [c[0] for c in rq(f"SHOW COLUMNS FROM {t}")] for t in ('items_templates', 'items_templates_weapons')}
sql = []
for i in nouv_objets:
    v, mid = IN[i], modeles[IN[i]['typeId']]
    table = 'items_templates_weapons' if mid in armes else 'items_templates'
    s = {'Id': str(i), 'Name': f"'{q(T.get(v['nameId']))}'", 'AppearanceId': '0', 'NameId': str(nt[i]), 'TypeId': str(v['typeId']), 'DescriptionId': str(dt[i]),
         'IconId': str(v['iconId']), 'Level': str(v['level']), 'Price': str(int(v.get('price', 0))), 'Weight': str(v.get('realWeight', 1)),
         'RealWeight': str(v.get('realWeight', 1)), 'ItemSetId': str(v['itemSetId']), 'Criteria': f"'{q(v.get('criteria') or '')}'",
         'RecipeIdsCSV': f"'{i if i in nouv_rec else ''}'", 'FavoriteSubAreasCSV': "''", 'PossibleEffectsBin': binaire[i], 'Etheral': '0', 'Cursed': '0',
         'TwoHanded': str(int(bool(v.get('twoHanded'))))}
    if table == 'items_templates_weapons':
        s.update(ApCost=str(v['apCost']), MinRange=str(v['minRange']), WeaponRange=str(v['range']), CastInLine=str(int(v['castInLine'])),
                 CastInDiagonal=str(int(v['castInDiagonal'])), CastTestLos=str(int(v['castTestLos'])), CriticalHitProbability=str(v['criticalHitProbability']),
                 CriticalHitBonus=str(v['criticalHitBonus']), CriticalFailureProbability=str(v['criticalFailureProbability']), MaxCastPerTurn=str(v['maxCastPerTurn']))
    sql.append(f"INSERT INTO {table} SELECT {', '.join(s.get(c, '`' + c + '`') for c in COL[table])} FROM {table} WHERE Id = {mid};")
for s_ in nouv_sets:
    its = [i for i in SN[s_]['items'] if i not in HORS_JEU]
    sql.append(f"INSERT INTO items_sets (Id, ItemsCSV, NameId, BonusIsSecret, EffectsBin) VALUES ({s_}, '{','.join(map(str, its))}', {ns[s_]}, "
               f"{int(SN[s_].get('bonusIsSecret', False))}, {bsets[s_]});")
anciens_sets = {int(a): int(b) for a, b in rq(f"SELECT Id, ItemSetId FROM (SELECT Id, ItemSetId FROM items_templates UNION ALL SELECT Id, ItemSetId FROM "
                                               f"items_templates_weapons) x WHERE Id IN ({','.join(map(str, objets_sets))})")}
for i in objets_sets:
    if i not in nouv_objets:
        for t in ('items_templates', 'items_templates_weapons'):
            sql.append(f"UPDATE {t} SET ItemSetId = {IN[i]['itemSetId']} WHERE Id = {i};")
anciennes_rec = {int(a): b for a, b in rq(f"SELECT Id, IngredientIdsCSV FROM recipes WHERE Id IN ({','.join(map(str, maj_rec))})")}
for r in nouv_rec:
    ing, qt = ingredients(r)
    sql.append(f"INSERT INTO recipes (Id, ResultNameId, ResultTypeId, ResultLevel, IngredientIdsCSV, QuantitiesBin, JobId, SkillId) VALUES ({r}, {nom_rec[r]}, "
               f"{IN[r]['typeId']}, {IN[r]['level']}, '{','.join(map(str, ing))}', 0x{','.join(map(str, qt)).encode().hex().upper()}, "
               f"{REC[r]['jobId']}, {REC[r]['skillId']});")
    if r not in nouv_objets:
        for t in ('items_templates', 'items_templates_weapons'):
            sql.append(f"UPDATE {t} SET RecipeIdsCSV = '{r}' WHERE Id = {r} AND (RecipeIdsCSV IS NULL OR RecipeIdsCSV = '');")
for r in maj_rec:
    nouv = ','.join(str(REMPLACE.get((r, int(g)), int(g))) for g in anciennes_rec[r].split(','))
    sql.append(f"UPDATE recipes SET IngredientIdsCSV = '{nouv}' WHERE Id = {r};")
idl = ','.join(map(str, nouv_objets)) or '0'
tete = ["-- PADGREF 23/10/2026 : SQL 127 — panoplies du client 2.74 choisies par l'utilisateur (planche) : "
        f"{len(nouv_sets)} panoplies créées, {len(nouv_objets)} objets créés, {len(nouv_rec)} recettes créées, {len(maj_rec)} recettes modifiées.",
        "-- Généré par import274/lot7_panoplies.py. Base world. Rejouable. Annulation : 127_panoplies_274-annuler.sql. Lu au démarrage du World.",
        "SET NAMES utf8mb4;", "START TRANSACTION;",
        f"DELETE FROM items_templates WHERE Id IN ({idl});", f"DELETE FROM items_templates_weapons WHERE Id IN ({idl});",
        f"DELETE FROM items_sets WHERE Id IN ({','.join(map(str, nouv_sets))});", f"DELETE FROM recipes WHERE Id IN ({','.join(map(str, nouv_rec))});",
        f"DELETE FROM langs WHERE Id BETWEEN {TEXTE0} AND {TEXTE0 + 999};",
        "INSERT INTO langs (Id, French) VALUES " + ",\n ".join(f"({i}, '{q(t)}')" for i, t in langs) + ";"]
open(os.path.join(SQLDIR, '127_panoplies_274.sql'), 'w', encoding='utf-8', newline='\n').write('\n'.join(tete + sql + ['COMMIT;']) + '\n')
ann = ["-- PADGREF 23/10 : annule 127_panoplies_274.sql", "START TRANSACTION;", f"DELETE FROM items_templates WHERE Id IN ({idl});",
       f"DELETE FROM items_templates_weapons WHERE Id IN ({idl});", f"DELETE FROM items_sets WHERE Id IN ({','.join(map(str, nouv_sets))});",
       f"DELETE FROM recipes WHERE Id IN ({','.join(map(str, nouv_rec))});", f"DELETE FROM langs WHERE Id BETWEEN {TEXTE0} AND {TEXTE0 + 999};"]
for i, s_ in anciens_sets.items():
    if i not in nouv_objets:
        ann += [f"UPDATE {t} SET ItemSetId = {s_} WHERE Id = {i};" for t in ('items_templates', 'items_templates_weapons')]
ann += [f"UPDATE recipes SET IngredientIdsCSV = '{v}' WHERE Id = {r};" for r, v in anciennes_rec.items()]
open(os.path.join(SQLDIR, '127_panoplies_274-annuler.sql'), 'w', encoding='utf-8', newline='\n').write('\n'.join(ann + ['COMMIT;']) + '\n')
json.dump({'panoplies_creees': nouv_sets, 'objets_crees': nouv_objets, 'recettes_creees': nouv_rec, 'recettes_modifiees': maj_rec,
           'remplacements': {f'{r}:{g}': v for (r, g), v in REMPLACE.items()}},
          open(os.path.join(ICI, 'lot7_panoplies.json'), 'w', encoding='utf-8'), ensure_ascii=False, indent=1)
print('panoplies créées', nouv_sets, '| objets créés', nouv_objets, '| recettes créées', nouv_rec, '| recettes modifiées', maj_rec, '| textes', len(langs))
