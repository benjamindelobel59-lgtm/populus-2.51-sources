# MYSTIC 19/10 : objets infinis (ne se consomment jamais ; effets gérés par le serveur : game/Items/ObjetsInfinis.cs).
#   31200 Pain Infini (50 000 PV, utilisé tout seul à la fin de chaque combat), 31201 Potion de Rappel infinie,
#   31202 Potion de Foyer de Guilde infinie, 31203 Potion d'enclos de Guilde infinie, 31204 Potion de Chasse au Trésor infinie.
# Icônes : faire_icones.py (PNG + badge ∞) et faire_swf.py (SWF) -> ajoutées aux archives items/bitmap1_1.d2p et vector1_1.d2p.
# Sorties (dossier client/) : Items.d2o, i18n_fr.d2i, bitmap1_1.d2p, vector1_1.d2p ; SQL 73 (_OUTILS/serveur/sql/73_objets_infinis.sql).
import json, os, struct, subprocess, sys
ICI = os.path.dirname(os.path.abspath(__file__))
CB = os.path.dirname(ICI)
RACINE = os.path.abspath(os.path.join(CB, '..', '..', '..'))
sys.path.insert(0, CB)
from d2o_ecrire import D2O
APP = os.path.join(RACINE, 'CLIENT_64bits', 'app')
SORTIE = os.path.join(ICI, 'client'); os.makedirs(SORTIE, exist_ok=True)

# (id, modèle, icône, nom, description, critère)
OBJETS = [
    (31200, 11503, 990001, "Pain Infini",
     "Un pain qui ne s'épuise jamais : il rend 50 000 points de vie et reste dans l'inventaire.\nTant qu'il est dans votre inventaire, il est mangé automatiquement à la fin de chaque combat.", ""),
    (31201, 548, 990002, "Potion de Rappel infinie",
     "Vous téléporte à votre point de sauvegarde (dernier zaap enregistré).\nCette potion ne se vide jamais.", None),
    (31202, 8883, 990003, "Potion de Foyer de Guilde infinie",
     "Vous téléporte devant la maison de votre guilde.\nCette potion ne se vide jamais.", None),
    (31203, 9035, 990004, "Potion d'enclos de Guilde infinie",
     "Vous téléporte dans un enclos de votre guilde.\nCette potion ne se vide jamais.", None),
    (31204, 548, 990005, "Potion de Chasse au Trésor infinie",
     "Vous téléporte devant la Malle aux Trésors (Champs de Cania).\nCette potion ne se vide jamais.", None),
]
TEXTE0 = 9001200

items = D2O(os.path.join(APP, 'data', 'common', 'Items.d2o'))
textes, sql, langs = {}, [], []
for i, (iid, modele, icone, nom, desc, critere) in enumerate(OBJETS):
    tn, td = TEXTE0 + 2 * i, TEXTE0 + 2 * i + 1
    textes[str(tn)], textes[str(td)] = nom, desc
    m = items.objet(modele)
    crit = m.get('criteria') if critere is None else critere
    o = dict(m, id=iid, nameId=tn, descriptionId=td, iconId=icone, possibleEffects=[], price=0.0, level=1,
             criteria=crit, usable=True, exchangeable=True)
    items.poser(iid, o)
    q = lambda s: s.replace("'", "''")
    sql.append(f"-- {iid} : {nom} (modèle {modele}, icône {icone})")
    sql.append(f"DELETE FROM items_templates WHERE Id = {iid};")
    sql.append(f"INSERT INTO items_templates SELECT {iid}, '{q(nom)}', `Type`, AppearanceId, Weight, RealWeight, {tn}, TypeId, {td}, {icone}, 1, Cursed, "
               f"UseAnimationId, 1, Targetable, 0, TwoHanded, Etheral, ItemSetId, {'NULL' if crit is None else repr(crit) if crit else chr(39)*2}, HideEffects, RecipeIdsCSV, FavoriteSubAreasCSV, "
               f"BonusIsSecret, (SELECT PossibleEffectsBin FROM items_templates WHERE Id = 8883), FavoriteSubAreasBonus, CraftXpRatio, IsLinkedToOwner FROM items_templates WHERE Id = {modele};")
    langs += [(tn, nom), (td, desc)]
items.ecrire(os.path.join(SORTIE, 'Items.d2o'))
json.dump(textes, open(os.path.join(ICI, 'textes.json'), 'w', encoding='utf-8'), ensure_ascii=False, indent=1)
subprocess.run([sys.executable, '-I', os.path.join(CB, 'd2i_ajout.py'), os.path.join(APP, 'data', 'i18n', 'i18n_fr.d2i'),
                os.path.join(SORTIE, 'i18n_fr.d2i'), os.path.join(ICI, 'textes.json')], check=True)

# icônes : ajout (ou remplacement) dans la DERNIÈRE archive de chaque chaîne (pas de lien après elle)
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
    nouveaux = {f'{o[2]}.{ext}': open(os.path.join(ICI, 'icones', f'{o[2]}.{ext}'), 'rb').read() for o in OBJETS}
    ent = [e for e in ent if e[0] not in nouveaux] + [[k, v] for k, v in nouveaux.items()]
    ecrire(os.path.join(SORTIE, archive), head, ent, props, pc)

entete = ["-- MYSTIC 19/10/2026 : SQL 73 — objets infinis (généré par _OUTILS/client/client-build/objets_infinis/generer_objets.py).",
          "-- Base world, World ARRÊTÉ. Rejouable. Effets côté serveur : game/Items/ObjetsInfinis.cs (ne se consomment jamais).",
          "-- Client : Items.d2o + i18n_fr.d2i + items/bitmap1_1.d2p + items/vector1_1.d2p (dossier objets_infinis/client).",
          "SET NAMES utf8mb4;", "USE `world`;", "START TRANSACTION;"]
lg = ["DELETE FROM langs WHERE Id BETWEEN %d AND %d;" % (TEXTE0, TEXTE0 + 2 * len(OBJETS) - 1),
      "INSERT INTO langs (Id, French) VALUES " + ",\n ".join(f"({i}, '{t.replace(chr(39), chr(39)*2)}')" for i, t in langs) + ";"]
fin = ["COMMIT;", f"SELECT Id, Name, IconId, Criteria FROM items_templates WHERE Id BETWEEN {OBJETS[0][0]} AND {OBJETS[-1][0]};"]
open(os.path.join(RACINE, '_OUTILS', 'serveur', 'sql', '73_objets_infinis.sql'), 'w', encoding='utf-8', newline='\n').write('\n'.join(entete + sql + lg + fin) + '\n')
print('ok', len(OBJETS))
