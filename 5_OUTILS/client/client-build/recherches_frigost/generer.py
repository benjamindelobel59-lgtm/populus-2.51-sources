# PADGREF 22/10 (demande utilisateur) : recherchés de Frigost — import du client 2.74 (_OUTILS/client/client_27x) :
#   15 chasses légendaires (Bouflouth, Monsieur Pingouin, Katigrou, Fantômayte, Vengeuse Masquée, YeCh'Ti, Fuji Givrefoux recherchée,
#   Flasho, Viti Glourson, Docteur Eggob, Glourdorak, Chevalier de Glace, Psikopompe, Culbutœuf, Mekamouth) avec leur carte, leurs
#   fragments, leur coffre, leur ressource ultra rare et la panoplie fabriquée avec (objets, effets, bonus, recettes) + 12 ingrédients
#   d'après la 2.51 utilisés par ces recettes. Les numéros d'objets / panoplies / chasses sont ceux du 2.74 (libres chez nous).
#   Entrée : contenu.json (extraire.py). Sorties :
#     client/ : Items.d2o, ItemSets.d2o, Recipes.d2o, LegendaryTreasureHunts.d2o, i18n_fr.d2i, items/bitmap1_1.d2p, items/vector1_1.d2p
#     _OUTILS/serveur/recherches/96_recherches_frigost.sql (+ -annuler), liste.txt
#   Pas d'apparence portée (AppearanceId 0) : le lien objet -> skin n'est pas dans le client (à faire à part).
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
SORTIE = os.path.join(ICI, 'client'); os.makedirs(os.path.join(SORTIE, 'items'), exist_ok=True)
SQLDIR = os.path.join(RACINE, '_OUTILS', 'serveur', 'recherches'); os.makedirs(SQLDIR, exist_ok=True)
MYSQL = os.path.join(RACINE, '_OUTILS', 'logiciels', 'MariaDB', 'bin', 'mysql.exe')
BIN = os.path.join(RACINE, '_OUTILS', 'serveur', 'panoplies', 'bin')
TEXTE0 = 9010000
# PADGREF 22/10 (décision utilisateur) : 6 ingrédients d'après la 2.51 importés et VENDUS en kamas (boutique d'Ernest Laye, la Bourgade)…
INGREDIENTS_EN_PLUS = [26870, 29444, 31528, 31533, 31628, 31678]
BOUTIQUE = 10687                                   # Ernest Laye (La Bourgade, Frigost)
PRIX = {26870: 10000, 29444: 50000, 31528: 100000, 31533: 300000, 31628: 500000, 31678: 500000}
# … et les 6 autres REMPLACÉS dans les recettes par une ressource 2.51 de même type et niveau 200 (mêmes quantités)
REMPLACE = {22046: 19237,     # Plume de Misère -> Plume d'Ilyzaelle
            22204: 13994,     # Corne de Corruption -> Scapula du Comte Harebourg
            22219: 19404,     # Verrou de Servitude -> Cervelle de Solar
            22246: 19969,     # Bonnet de Guerre -> Épaulette de Dazak Martegel
            26037: 19411,     # Masque cauchemardesque de l'Éternel Conflit -> Queue de Bethel Akarna
            27518: 19233}     # Cœur de Cire Momore (rare) -> Brûlâme encapsulé (rare d'Ilyzaelle)
# types d'après la 2.51 -> type 2.51 le plus proche
TYPES = {271: 4, 228: 60, 229: 15, 266: 15}          # Lance -> Bâton ; Liquide -> Huile ; Ress. de combat / anomalies -> Ress. diverses


def rq(s):
    out = subprocess.run([MYSQL, '-uroot', '-h127.0.0.1', '-N', '-B', '--default-character-set=utf8', 'world', '-e', s],
                         capture_output=True, check=True).stdout.decode('utf-8')
    return [l.split('\t') for l in out.replace('\r', '').split('\n') if l]


q = lambda s: (s or '').replace('\\', '\\\\').replace("'", "''")
T = d2i.load(os.path.join(N, 'data', 'i18n', 'i18n_fr.d2i'))
lire = lambda f: d2o.load(os.path.join(N, 'data', 'common', f + '.d2o'))[1]
IN, SN, LN, RD = lire('Items'), lire('ItemSets'), lire('LegendaryTreasureHunts'), lire('RandomDropGroups')
REC = {r['resultId']: r for r in lire('Recipes').values()}
for _r in REC.values():
    _r['ingredientIds'] = [REMPLACE.get(i, i) for i in _r['ingredientIds']]
contenu = json.load(open(os.path.join(ICI, 'contenu.json'), encoding='utf-8'))

items_c = D2O(os.path.join(APP, 'data', 'common', 'Items.d2o'))
sets_c = D2O(os.path.join(APP, 'data', 'common', 'ItemSets.d2o'))
rec_c = D2O(os.path.join(APP, 'data', 'common', 'Recipes.d2o'))
leg_c = D2O(os.path.join(APP, 'data', 'common', 'LegendaryTreasureHunts.d2o'))

# ------------------------------------------------------------------ objets à importer
ids = list(INGREDIENTS_EN_PLUS)
for s in contenu:
    ids += [s['coffre'], s['carte'], s['rare']] + s['fragments'] + s['panoplie']['objets']
ids = sorted(set(ids))
assert all(items_c.objet(i) is None for i in ids), 'objet déjà présent dans le client'

# modèles 2.51 (client + serveur) par type : un objet officiel du même type, avec des effets si possible
armes = {int(a) for (a,) in rq("SELECT Id FROM items_templates_weapons")}
modeles_sql = {}
for tid, iid, nb in rq("SELECT TypeId, Id, LENGTH(PossibleEffectsBin) FROM (SELECT TypeId, Id, PossibleEffectsBin, NameId FROM items_templates "
                       "UNION ALL SELECT TypeId, Id, PossibleEffectsBin, NameId FROM items_templates_weapons) x WHERE NameId < 1000000 ORDER BY Id"):
    tid, iid = int(tid), int(iid)
    if tid not in modeles_sql and items_c.objet(iid) is not None:
        modeles_sql[tid] = iid
MODELES_FIXES = {172: 15486, 174: 15479, 175: 15480}   # coffre / carte / fragment = ceux du Trésor de Dremoan
modeles_sql.update(MODELES_FIXES)
effet_modele = items_c.objet(12117)['possibleEffects'][0]      # EffectInstanceDice 2.51
hex_effets_modele = rq("SELECT HEX(PossibleEffectsBin) FROM items_templates WHERE Id = 12117")[0][0]

# textes
textes, langs = {}, []
prochain = [TEXTE0]


def texte(t):
    i = prochain[0]; prochain[0] += 1
    textes[str(i)] = t or ''
    langs.append((i, t or ''))
    return i


def effets_client(liste):
    res = []
    for e in liste or []:
        if not e:
            continue
        x = dict(effet_modele)
        x.update(effectId=e['effectId'], diceNum=e.get('diceNum', 0), diceSide=e.get('diceSide', 0), value=e.get('value', 0))
        res.append(x)
    return res


nom_txt, desc_txt, typ = {}, {}, {}
for i in ids:
    v = IN[i]
    typ[i] = TYPES.get(v['typeId'], v['typeId'])
    nom_txt[i] = texte(T.get(v['nameId']))
    desc_txt[i] = texte(T.get(v['descriptionId']))

# ------------------------------------------------------------------ client : objets
recettes_utiles = {r for r in ids if r in REC}
for i in ids:
    v = IN[i]
    m = items_c.objet(modeles_sql[typ[i]])
    o = dict(m)
    for k in o:
        if k in v and k not in ('__classe', 'possibleEffects', 'nameId', 'descriptionId', 'typeId', 'appearanceId', 'recipeIds',
                                'favoriteSubAreas', 'dropMonsterIds', 'containerIds', 'evolutiveEffectIds', 'itemSetId', 'recipeSlots'):
            o[k] = v[k]
    o.update(id=i, nameId=nom_txt[i], descriptionId=desc_txt[i], typeId=typ[i], appearanceId=0,
             possibleEffects=[] if typ[i] == 172 else effets_client(v['possibleEffects']),
             itemSetId=v.get('itemSetId', -1) if v.get('itemSetId', -1) in [s['panoplie']['id'] for s in contenu] else -1,
             recipeIds=[r for r in (v.get('recipeIds') or []) if r in recettes_utiles])
    if 'favoriteSubAreas' in o: o['favoriteSubAreas'] = []
    if 'dropMonsterIds' in o: o['dropMonsterIds'] = []
    if typ[i] != v['typeId'] and typ[i] == 4:
        o['twoHanded'] = True
    items_c.poser(i, o)

# panoplies
nom_set = {}
for s in contenu:
    p = s['panoplie']; sid = p['id']
    nom_set[sid] = texte(p['nom'])
    o = dict(sets_c.objet(348))
    o.update(id=sid, items=p['objets'], nameId=nom_set[sid], bonusIsSecret=SN[sid].get('bonusIsSecret', False),
             effects=[effets_client(pal) for pal in SN[sid]['effects']])
    sets_c.poser(sid, o)

# recettes (objets de panoplie + cartes)
modele_rec = rec_c.objet(12117)
for r in sorted(recettes_utiles):
    x = REC[r]
    o = dict(modele_rec)
    o.update(resultId=r, resultNameId=nom_txt[r], resultTypeId=typ[r], resultLevel=IN[r]['level'], ingredientIds=x['ingredientIds'],
             quantities=x['quantities'], jobId=x['jobId'], skillId=x['skillId'])
    rec_c.poser(r, o)

# chasses légendaires
nom_chasse = {}
for s in contenu:
    nom_chasse[s['chasse']] = texte(s['nom'])
    o = dict(leg_c.objet(15))
    o.update(id=s['chasse'], nameId=nom_chasse[s['chasse']], level=s['niveau'], chestId=s['coffre'], monsterId=s['monstre'],
             mapItemId=s['carte'], xpRatio=float(s['xpRatio']))
    leg_c.poser(s['chasse'], o)

# objets 2.51 utilisés dans les nouvelles recettes : « utilisé dans » (recipeIds) complété
for r in sorted(recettes_utiles):
    for ing in REC[r]['ingredientIds']:
        if ing in ids:
            continue
        o = items_c.objet(ing)
        if o is not None and r not in (o.get('recipeIds') or []):
            o = dict(o); o['recipeIds'] = list(o.get('recipeIds') or []) + [r]
            items_c.poser(ing, o)
items_c.ecrire(os.path.join(SORTIE, 'Items.d2o'))
sets_c.ecrire(os.path.join(SORTIE, 'ItemSets.d2o'))
rec_c.ecrire(os.path.join(SORTIE, 'Recipes.d2o'))
leg_c.ecrire(os.path.join(SORTIE, 'LegendaryTreasureHunts.d2o'))
json.dump(textes, open(os.path.join(ICI, 'textes.json'), 'w', encoding='utf-8'), ensure_ascii=False, indent=0)
subprocess.run([sys.executable, '-I', os.path.join(CB, 'd2i_ajout.py'), os.path.join(APP, 'data', 'i18n', 'i18n_fr.d2i'),
                os.path.join(SORTIE, 'i18n_fr.d2i'), os.path.join(ICI, 'textes.json')], check=True)

# ------------------------------------------------------------------ icônes (copiées du 2.74 dans la dernière archive de chaque chaîne)


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
for arch, ext in (('bitmap', 'png'), ('vector', 'swf')):
    deja = {n for f in glob.glob(os.path.join(APP, 'content', 'gfx', 'items', arch + '*.d2p')) for n, _ in me.read_d2p(f)}
    source = {}
    for f in glob.glob(os.path.join(N, 'content', 'gfx', 'items', arch + '*.d2p')):
        for n, b in me.read_d2p(f):
            if n.endswith('.' + ext) and n[:-4].isdigit() and int(n[:-4]) in icones:
                source[n] = b
    nouveaux = {n: b for n, b in source.items() if n not in deja}
    head, ent, props, pc = lire_d2p(os.path.join(APP, 'content', 'gfx', 'items', f'{arch}1_1.d2p'))
    ent = [e for e in ent if e[0] not in nouveaux] + [[k, v] for k, v in sorted(nouveaux.items())]
    ecrire_d2p(os.path.join(SORTIE, 'items', f'{arch}1_1.d2p'), head, ent, props, pc)
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
lignes_set = []
for s in contenu:
    sid = s['panoplie']['id']
    pal = SN[sid]['effects']
    lignes_set.append(f"{sid}\t" + '|'.join(','.join(f"{e['effectId']}:{e.get('diceNum', 0)}" for e in p if e) for p in pal))
out = subprocess.run([os.path.join(BIN, 'Ecrire.exe')], input='\n'.join(lignes_set).encode(), capture_output=True, check=True,
                     cwd=BIN).stdout.decode()
bin_sets = {}
for l in out.splitlines():
    if l.startswith('UPDATE'):
        i = int(l.rsplit('Id = ', 1)[1].rstrip(';'))
        bin_sets[i] = l.split('EffectsBin = ', 1)[1].split(' WHERE')[0]
assert len(bin_sets) == len(contenu), 'bonus de panoplie'

COLONNES = {t: [c for c in rq(f"SHOW COLUMNS FROM {t}")] for t in ('items_templates', 'items_templates_weapons')}
assert all(COLONNES.values())
sql = []
for i in ids:
    v, mid = IN[i], modeles_sql[typ[i]]
    arme = mid in armes
    table = 'items_templates_weapons' if arme else 'items_templates'
    eff = bin_effets.get(i, 'PossibleEffectsBin')
    cols = COLONNES[table]
    sel = []
    surcharges = {
        'Id': str(i), 'Name': f"'{q(T.get(v['nameId']))}'", 'AppearanceId': '0', 'NameId': str(nom_txt[i]), 'TypeId': str(typ[i]),
        'DescriptionId': str(desc_txt[i]), 'IconId': str(v['iconId']), 'Level': str(v['level']), 'Price': str(v.get('price', 0)),
        'Weight': str(v.get('realWeight', 1)), 'RealWeight': str(v.get('realWeight', 1)),
        'ItemSetId': str(v.get('itemSetId', -1) if v.get('itemSetId', -1) in bin_sets else -1),
        'Criteria': f"'{q(v.get('criteria') or '')}'", 'RecipeIdsCSV': f"'{','.join(map(str, [r for r in (v.get('recipeIds') or []) if r in recettes_utiles]))}'",
        'FavoriteSubAreasCSV': "''", 'PossibleEffectsBin': eff, 'Etheral': '0', 'Cursed': '0',
        'TwoHanded': '1' if (v.get('twoHanded') or typ[i] == 4 and v['typeId'] == 271) else '0',
    }
    if arme:
        surcharges.update(ApCost=str(v.get('apCost', 4)), MinRange=str(v.get('minRange', 1)), WeaponRange=str(v.get('range', 1)),
                          CastInLine=str(int(v.get('castInLine', False))), CastInDiagonal=str(int(v.get('castInDiagonal', False))),
                          CastTestLos=str(int(v.get('castTestLos', True))), CriticalHitProbability=str(v.get('criticalHitProbability', 0)),
                          CriticalHitBonus=str(v.get('criticalHitBonus', 0)), CriticalFailureProbability=str(v.get('criticalFailureProbability', 0)),
                          MaxCastPerTurn=str(v.get('maxCastPerTurn', 1)))
    for (c, *_r) in cols:
        sel.append(surcharges.get(c, f"`{c}`"))
    sql.append(f"INSERT INTO {table} SELECT {', '.join(sel)} FROM {table} WHERE Id = {mid};")
for s in contenu:
    sid = s['panoplie']['id']
    sql.append(f"INSERT INTO items_sets (Id, ItemsCSV, NameId, BonusIsSecret, EffectsBin) VALUES ({sid}, '{','.join(map(str, s['panoplie']['objets']))}', "
               f"{nom_set[sid]}, {int(SN[sid].get('bonusIsSecret', False))}, {bin_sets[sid]});")
for r in sorted(recettes_utiles):
    x = REC[r]
    qte = ','.join(map(str, x['quantities'])).encode().hex().upper()
    sql.append(f"INSERT INTO recipes (Id, ResultNameId, ResultTypeId, ResultLevel, IngredientIdsCSV, QuantitiesBin, JobId, SkillId) VALUES "
               f"({r}, {nom_txt[r]}, {typ[r]}, {IN[r]['level']}, '{','.join(map(str, x['ingredientIds']))}', 0x{qte}, {x['jobId']}, {x['skillId']});")
for s in contenu:
    sql.append(f"INSERT INTO mystic_chasses_legendaires (Id, NameId, Niveau, CoffreId, MonstreId, CarteId, RatioXp, RessourceId) VALUES "
               f"({s['chasse']}, {nom_chasse[s['chasse']]}, {s['niveau']}, {s['coffre']}, {s['monstre']}, {s['carte']}, {float(s['xpRatio'])}, {s['rare']});")
# ressource rare des 20 chasses 2.51 (contenu de leur coffre dans le 2.74, si l'objet existe chez nous)
existants = {int(a) for (a,) in rq("SELECT Id FROM items_templates UNION SELECT Id FROM items_templates_weapons")}
maj_251 = []
for lid, l in LN.items():
    if lid in {s['chasse'] for s in contenu}:
        continue
    g = next((e['value'] for e in IN.get(l['chestId'], {}).get('possibleEffects', []) if e['effectId'] == 222), None)
    rare = [x['itemId'] for x in RD.get(g, {}).get('randomDropItems', []) if x['itemId'] != 15263 and x['itemId'] in existants]
    if rare:
        maj_251.append(f"UPDATE mystic_chasses_legendaires SET RessourceId = {rare[0]} WHERE Id = {lid};")
# fragments : lâchés par les monstres de la zone (taux 2.74, taux normal du serveur : TauxFixe 0)
drops = []
for s in contenu:
    for f, ds in s['dropsFragments'].items():
        for d in ds:
            if d['dans251']:
                t = d['taux']
                drops.append(f"({d['monstre']}, {f}, 1, {t[0]}, {t[1]}, {t[2]}, {t[3]}, {t[4]}, 1, 0, NULL, 0, 1, 0)")
boutique = [f"DELETE FROM npcs_items WHERE NpcShopId = {BOUTIQUE} AND ItemId IN ({','.join(map(str, PRIX))});",
            "INSERT INTO npcs_items (NpcShopId, CustomPrice, BuyCriterion, MaxStats, ItemId, Type, Level) VALUES " +
            ", ".join(f"({BOUTIQUE}, {p}, NULL, 0, {i}, NULL, NULL)" for i, p in PRIX.items()) + ";"]
textes_sql = [f"INSERT INTO langs (Id, French) VALUES " + ",\n ".join(f"({i}, '{q(t)}')" for i, t in langs) + ";"]
idl = ','.join(map(str, ids))
entete = [
    "-- PADGREF 22/10/2026 : SQL 96 — recherchés de Frigost (importés du client 2.74 ; généré par _OUTILS/client/client-build/recherches_frigost/generer.py).",
    f"-- {len(contenu)} chasses légendaires (Id 1010 à 1029) : carte (fragments sur les monstres de la zone), coffre (Roses des sables + ressource ultra rare),",
    f"-- panoplie fabriquée avec la ressource ({len(contenu)} panoplies, recettes). {len(ids)} objets ({min(ids)} à {max(ids)}), textes {TEXTE0}+.",
    "-- + ressource ultra rare des 20 chasses légendaires 2.51 (Esporme de Dremoan…). Base world. Rejouable. Annulation : 96_recherches_frigost-annuler.sql",
    "-- Client : Items.d2o, ItemSets.d2o, Recipes.d2o, LegendaryTreasureHunts.d2o, i18n_fr.d2i, items/bitmap1_1.d2p, items/vector1_1.d2p.",
    "SET NAMES utf8mb4;", "START TRANSACTION;",
    "ALTER TABLE mystic_chasses_legendaires ADD COLUMN IF NOT EXISTS RessourceId INT NOT NULL DEFAULT 0;",
    f"DELETE FROM items_templates WHERE Id IN ({idl});", f"DELETE FROM items_templates_weapons WHERE Id IN ({idl});",
    f"DELETE FROM items_sets WHERE Id IN ({','.join(str(s['panoplie']['id']) for s in contenu)});",
    f"DELETE FROM recipes WHERE Id IN ({','.join(map(str, sorted(recettes_utiles)))});",
    f"DELETE FROM mystic_chasses_legendaires WHERE Id IN ({','.join(str(s['chasse']) for s in contenu)});",
    f"DELETE FROM monsters_drops WHERE ItemId IN ({idl});",
    f"DELETE FROM langs WHERE Id BETWEEN {TEXTE0} AND {TEXTE0 + 9999};"]
corps = entete + textes_sql + sql + maj_251 + boutique + [
    "INSERT INTO monsters_drops (MonsterOwnerId, ItemId, DropLimit, DropRateForGrade1, DropRateForGrade2, DropRateForGrade3, DropRateForGrade4, "
    "DropRateForGrade5, RollsCounter, ProspectingLock, `Condition`, DropGroup, TaxCollectorCannotLoot, TauxFixe) VALUES\n" + ",\n".join(drops) + ";",
    "COMMIT;"]
open(os.path.join(SQLDIR, '96_recherches_frigost.sql'), 'w', encoding='utf-8', newline='\r\n').write('\n'.join(corps) + '\n')
open(os.path.join(SQLDIR, '96_recherches_frigost-annuler.sql'), 'w', encoding='utf-8', newline='\r\n').write('\n'.join([
    "-- PADGREF 22/10 : annule 96_recherches_frigost.sql (la colonne RessourceId reste, sans effet à 0)", "START TRANSACTION;",
    f"DELETE FROM items_templates WHERE Id IN ({idl});", f"DELETE FROM items_templates_weapons WHERE Id IN ({idl});",
    f"DELETE FROM items_sets WHERE Id IN ({','.join(str(s['panoplie']['id']) for s in contenu)});",
    f"DELETE FROM recipes WHERE Id IN ({','.join(map(str, sorted(recettes_utiles)))});",
    f"DELETE FROM mystic_chasses_legendaires WHERE Id IN ({','.join(str(s['chasse']) for s in contenu)});",
    f"DELETE FROM monsters_drops WHERE ItemId IN ({idl});", "UPDATE mystic_chasses_legendaires SET RessourceId = 0;",
    f"DELETE FROM npcs_items WHERE NpcShopId = {BOUTIQUE} AND ItemId IN ({','.join(map(str, PRIX))});",
    f"DELETE FROM langs WHERE Id BETWEEN {TEXTE0} AND {TEXTE0 + 9999};", "COMMIT;"]) + '\n')
liste = []
for s in contenu:
    liste.append(f"{s['chasse']} {s['nom']} (niv {s['niveau']}) : combat {s['nomMonstre']} ({s['monstre']}) ; carte {s['carte']} = {len(s['fragments'])} fragments ; "
                 f"coffre {s['coffre']} -> {s['nomRare']} ; {s['panoplie']['nom']} : " + ', '.join(T.get(IN[o]['nameId']) for o in s['panoplie']['objets']))
open(os.path.join(ICI, 'liste.txt'), 'w', encoding='utf-8').write('\n'.join(liste) + '\n')
print('ok :', len(ids), 'objets,', len(contenu), 'panoplies,', len(recettes_utiles), 'recettes,', len(contenu), 'chasses,', len(drops), 'drops de fragments,',
      len(maj_251), 'chasses 2.51 avec ressource ;', 'icônes copiées', copiees, '; textes', len(langs))
