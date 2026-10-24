# PADGREF 23/10 (demandes utilisateur : « il faut ajouter le Galet rayonnant en jeu, il ira dans la boutique Doplons » puis « remets
# le Galet rayonnant à tous les endroits où j'ai changé avec Solaire ») :
#   - recettes (les 9 recettes changées sur la planche ; pas les trophées majeurs 2.51) : là où le 2.74 met le Galet rayonnant (29444) et où nous avons le Galet Solaire (13367) -> Galet rayonnant ;
#     recettes 2.51 des objets choisis (11600, 11626, 11631, 11757) : leur galet (rutilant 12738 / brasillant 12740) -> Galet
#     rayonnant, à la quantité du 2.74 ;
#   - boutique : Galet rayonnant vendu par le Marchand de Doplons (boutique 900525, jeton Doplon 13052) au lieu d'Ernest Leh (kamas).
# Client : Recipes.d2o (import274/client, le plus récent). Sortie : SQL 128 (+ annuler).
import os, shutil, subprocess, sys
sys.stdout.reconfigure(encoding='utf-8')
ICI = os.path.dirname(os.path.abspath(__file__))
CB = os.path.dirname(ICI)
R = os.path.abspath(os.path.join(CB, '..', '..', '..'))
sys.path.insert(0, CB); sys.path.insert(0, os.path.join(R, '_OUTILS', 'serveur', 'donjons'))
from d2o_ecrire import D2O
import d2o
MYSQL = os.path.join(R, '_OUTILS', 'logiciels', 'MariaDB', 'bin', 'mysql.exe')
F = os.path.join(ICI, 'client', 'data', 'common', 'Recipes.d2o')
SQLDIR = os.path.join(R, '_OUTILS', 'serveur', 'import274')
RAYONNANT, SOLAIRE, ANCIENS_GALETS = 29444, 13367, (12738, 12740)
ANCIENNES = (11600, 11626, 11631, 11757)
CHOISIES = set(ANCIENNES) | {32212, 32213, 32217, 32221, 32223}      # les 9 recettes changées par l'utilisateur sur la planche
BOUTIQUE_DOPLONS, PRIX_DOPLONS, BOUTIQUE_KAMAS = 900525, 250, 10687


def rq(s):
    out = subprocess.run([MYSQL, '-uroot', '-h127.0.0.1', '-N', '-B', 'world', '-e', s], capture_output=True, check=True).stdout.decode()
    return [l.split('\t') for l in out.replace('\r', '').split('\n') if l]


RE = {r['resultId']: r for r in d2o.load(os.path.join(R, '_OUTILS', 'client', 'client_27x', 'data', 'common', 'Recipes.d2o'))[1].values()}
base = {int(i): (c, bytes.fromhex(h).decode()) for i, c, h in rq('SELECT Id, IngredientIdsCSV, HEX(QuantitiesBin) FROM recipes')}
maj = {}
for r, (c, qt) in base.items():
    ing, qs = [int(x) for x in c.split(',')], [int(x) for x in qt.split(',')]
    n274 = RE.get(r)
    if r not in CHOISIES or not n274 or RAYONNANT not in n274['ingredientIds'] or RAYONNANT in ing:
        continue
    q274 = n274['quantities'][n274['ingredientIds'].index(RAYONNANT)]
    for ancien in (SOLAIRE,) + (ANCIENS_GALETS if r in ANCIENNES else ()):
        if ancien in ing:
            k = ing.index(ancien)
            ing[k] = RAYONNANT
            if ancien != SOLAIRE:
                qs[k] = q274
            maj[r] = (ing, qs, c, qt)
            break
sauve = os.path.join(R, 'SAVE', '3_CLIENT_64bits', '2026-10-23_galet-rayonnant-avant')
os.makedirs(sauve, exist_ok=True)
if not os.path.exists(os.path.join(sauve, 'Recipes.d2o')):
    shutil.copy2(F, sauve)
rec = D2O(os.path.join(sauve, 'Recipes.d2o'))
for r, (ing, qs, _, _) in maj.items():
    o = rec.objet(r)
    if o:
        o = dict(o); o.update(ingredientIds=ing, quantities=qs); rec.poser(r, o)
rec.ecrire(F)
hexq = lambda qs: '0x' + ','.join(map(str, qs)).encode().hex().upper()
sql = ["-- PADGREF 23/10/2026 : SQL 128 — Galet rayonnant (29444) : remis dans les recettes à la place du Galet Solaire (comme le 2.74) "
       f"({len(maj)} recettes) et vendu par le Marchand de Doplons ({PRIX_DOPLONS} Doplons) au lieu d'Ernest Leh.",
       "-- Généré par import274/lot8_galet_rayonnant.py. Base world. Rejouable. Lu au démarrage du World.", "START TRANSACTION;"]
sql += [f"UPDATE recipes SET IngredientIdsCSV = '{','.join(map(str, ing))}', QuantitiesBin = {hexq(qs)} WHERE Id = {r};" for r, (ing, qs, _, _) in sorted(maj.items())]
sql += [f"DELETE FROM npcs_items WHERE ItemId = {RAYONNANT} AND NpcShopId IN ({BOUTIQUE_KAMAS}, {BOUTIQUE_DOPLONS});",
        f"INSERT INTO npcs_items (NpcShopId, CustomPrice, BuyCriterion, MaxStats, ItemId) VALUES ({BOUTIQUE_DOPLONS}, {PRIX_DOPLONS}, NULL, 0, {RAYONNANT});",
        "COMMIT;"]
open(os.path.join(SQLDIR, '128_galet_rayonnant.sql'), 'w', encoding='utf-8', newline='\n').write('\n'.join(sql) + '\n')
prix_kamas = rq(f"SELECT CustomPrice FROM npcs_items WHERE ItemId = {RAYONNANT} AND NpcShopId = {BOUTIQUE_KAMAS}")
ann = ["-- PADGREF 23/10 : annule 128", "START TRANSACTION;"]
ann += [f"UPDATE recipes SET IngredientIdsCSV = '{c}', QuantitiesBin = {hexq([int(x) for x in qt.split(',')])} WHERE Id = {r};" for r, (_, _, c, qt) in sorted(maj.items())]
ann += [f"DELETE FROM npcs_items WHERE ItemId = {RAYONNANT} AND NpcShopId = {BOUTIQUE_DOPLONS};"]
if prix_kamas:
    ann += [f"INSERT INTO npcs_items (NpcShopId, CustomPrice, BuyCriterion, MaxStats, ItemId) VALUES ({BOUTIQUE_KAMAS}, {prix_kamas[0][0]}, NULL, 0, {RAYONNANT});"]
open(os.path.join(SQLDIR, '128_galet_rayonnant-annuler.sql'), 'w', encoding='utf-8', newline='\n').write('\n'.join(ann + ['COMMIT;']) + '\n')
print('recettes modifiées', len(maj), sorted(maj))
