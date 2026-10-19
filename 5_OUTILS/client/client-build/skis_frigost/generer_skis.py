# MYSTIC 19/10 : skis de Frigost pour chaque donjon (jusqu'à Frigost III), en plus des 4 skis officiels.
# Chaque ski est jetable, téléporte devant l'entrée du donjon et demande le succès de CE donjon (critère OA) ; l'accès à la zone
# reste contrôlé par le serveur (AccesFrigost.cs). Utilisable depuis Frigost (ou Sakaï).
# Sorties : Items.d2o + i18n_fr.d2i (partant de oto_mustam_depart/i18n_fr.d2i : contient aussi le texte du PNJ) dans ce dossier,
#           SQL 63 (_OUTILS/serveur/sql/63_skis_frigost.sql).
import json, os, subprocess, sys
ICI = os.path.dirname(os.path.abspath(__file__))
CB = os.path.dirname(ICI)
RACINE = os.path.abspath(os.path.join(CB, '..', '..', '..'))
sys.path.insert(0, CB)
from d2o_ecrire import D2O

MODELE = 11995                       # Paire de skis glissants jetables
PREMIER_ID, PREMIER_TEXTE = 31100, 9000910
USAGE = [600, 601, 602, 603, 604, 605, 606, 608, 609, 610, 611, 615, 616, 785, 786, 787, 788, 789, 795, 650, 651, 652, 653]
# (donjon, nom court, lieu dans la description, succès, carte, case)
SKIS = [
    ("Ben le Ripate", "jusqu'à l'épave du Grolandais violent", 58, 54163796, 286),
    ("Obsidiantre", "jusqu'à l'hypogée de l'Obsidiantre", 63, 56890883, 341),
    ("Tengu Givrefoux", "jusqu'à la tanière Givrefoux", 31, 59510784, 357),
    ("Tengu Givrefoux Fuji", "jusqu'aux cavernes nourricières de Sakaï", 134, 54159661, 335),
    ("Korriandre", "jusqu'à l'antre du Korriandre", 23, 60036612, 313),
    ("Kolosso", "jusqu'aux cavernes du Kolosso", 30, 54164293, 350),
    ("Glourséleste", "jusqu'à la ruche des Gloursons", 20, 62129672, 331),
    ("Nileza", "jusqu'au laboratoire de Nileza", 887, 108794630, 386),
    ("Sylargh", "jusqu'au transporteur de Sylargh", 877, 108793118, 294),
    ("Klime", "jusqu'aux salons privés de Klime", 882, 108925717, 294),
    ("Missiz Frizz", "jusqu'à la forgefroide de Missiz Frizz", 872, 108927238, 271),
    ("Comte Harebourg", "jusqu'au donjon du Comte Harebourg", 892, 112460048, 308),
]

ARTICLES = {"Obsidiantre": "de l'", "Nileza": "de ", "Sylargh": "de ", "Klime": "de ", "Missiz Frizz": "de "}

items = D2O(os.path.join(RACINE, 'CLIENT_64bits', 'app', 'data', 'common', 'Items.d2o'))
modele = items.objet(MODELE)
textes, sql = {}, []
usage = '(' + '|'.join(f'PB={s}' for s in USAGE) + ')'
for i, (nom, lieu, succes, carte, case) in enumerate(SKIS):
    iid, tn, td = PREMIER_ID + i, PREMIER_TEXTE + 2 * i, PREMIER_TEXTE + 2 * i + 1
    critere = f"{usage}&Pm!{carte}&OA={succes}"
    textes[str(tn)] = "Paire de skis " + ARTICLES.get(nom, "du ") + nom
    textes[str(td)] = (f"Grâce à cette paire de skis, vous pourrez glisser sur la neige {lieu}. "
                       f"Réservée aux aventuriers ayant déjà vaincu ce donjon.\nAttention ! Ces skis s'usant très vite, ils seront détruits après une utilisation.")
    o = dict(modele, id=iid, nameId=tn, descriptionId=td, criteria=critere, price=200.0,
             possibleEffects=[e for e in modele['possibleEffects'] if e['effectId'] != 999])
    items.poser(iid, o)
    sql.append(f"-- {iid} : {textes[str(tn)]} (succès {succes}) -> {carte} case {case}")
    sql.append(f"DELETE FROM items_templates WHERE Id = {iid};")
    sql.append(f"INSERT INTO items_templates SELECT {iid}, '{textes[str(tn)].replace(chr(39), chr(39)*2)}', `Type`, AppearanceId, Weight, RealWeight, {tn}, TypeId, {td}, IconId, `Level`, Cursed, "
               f"UseAnimationId, Usable, Targetable, 200, TwoHanded, Etheral, ItemSetId, '{critere}', HideEffects, RecipeIdsCSV, FavoriteSubAreasCSV, "
               f"BonusIsSecret, PossibleEffectsBin, FavoriteSubAreasBonus, CraftXpRatio, IsLinkedToOwner FROM items_templates WHERE Id = {MODELE};")
    sql.append(f"DELETE FROM items_teleport_potions WHERE ItemId = {iid};")
    sql.append(f"INSERT INTO items_teleport_potions (ItemId, MapId, CellId, Orientation) VALUES ({iid}, {carte}, {case}, 1);")
    sql.append(f"DELETE FROM npcs_items WHERE NpcShopId = 10692 AND ItemId = {iid};")
    sql.append(f"INSERT INTO npcs_items (NpcShopId, ItemId, MaxStats) VALUES (10692, {iid}, 0);")
items.ecrire(os.path.join(ICI, 'Items.d2o'))
json.dump(textes, open(os.path.join(ICI, 'textes.json'), 'w', encoding='utf-8'), ensure_ascii=False, indent=1)
subprocess.run([sys.executable, '-I', os.path.join(CB, 'd2i_ajout.py'), os.path.join(CB, 'oto_mustam_depart', 'i18n_fr.d2i'),
                os.path.join(ICI, 'i18n_fr.d2i'), os.path.join(ICI, 'textes.json')], check=True)

entete = ["-- MYSTIC 19/10/2026 : SQL 63 — skis de Frigost pour chaque donjon (généré par _OUTILS/client/client-build/skis_frigost/generer_skis.py).",
          "-- Base world, World ARRÊTÉ. Rejouable. Vendus par la Vendeuse de skis (boutique 10692), 200 kamas. Client : Items.d2o + i18n_fr.d2i.",
          "SET NAMES utf8mb4;", "USE `world`;", "START TRANSACTION;"]
fin = ["COMMIT;", f"SELECT Id, Name, Criteria FROM items_templates WHERE Id BETWEEN {PREMIER_ID} AND {PREMIER_ID + len(SKIS) - 1};"]
open(os.path.join(RACINE, '_OUTILS', 'serveur', 'sql', '63_skis_frigost.sql'), 'w', encoding='utf-8', newline='\n').write('\n'.join(entete + sql + fin) + '\n')
print(len(SKIS), 'skis')
