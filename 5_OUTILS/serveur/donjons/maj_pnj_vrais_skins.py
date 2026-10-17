# NEKFEU 2026-10-16 : PNJ de sortie de donjon -> nom + apparence du VRAI PNJ quand il existe (liste LISTE-PNJ.txt de
# l'utilisateur, ids du client), sinon apparence du boss (comme generer_pnj_boss.py). Ajoute aussi les PNJ des 3 donjons
# corriges a 11h18 (Minotoror, Brumen, Ekarlatte : Portier 800 -> PNJ dedie).
# Garde les ids existants (pnj_boss_rapport.tsv : 10016-10057) ; nouveaux PNJ a partir de 10058.
# Sorties : client/Npcs.d2o (reconstruit depuis Npcs.original.d2o), pnj_vrais_skins.sql / -annuler.sql / _rapport.tsv
import os, struct, sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import d2o, d2i

ICI = os.path.dirname(os.path.abspath(__file__))
RACINE = os.path.dirname(os.path.dirname(ICI))
SRC = os.path.join(ICI, "client", "Npcs.original.d2o")
MESSAGE, TEXTE_MESSAGE, REPONSE, TEXTE_REPONSE = 60001, 24359, 2962, 767152  # « Félicitations ! » / « Sortir du donjon. »
NEW0, ACTION_NEW0 = 10058, 900253

# boss (monstre) -> vrai PNJ du meme nom (LISTE-PNJ.txt, verifie par nom)
VRAI = {2994: 755, 2995: 760}  # rempli ci-dessous par nom ; Koulosse / Skeunk
# donjons ajoutes : (donjon, salle de fin, boss, vrai PNJ ou None, id npcs_spawns a mettre a jour)
EXTRA = [(4, 34473476, 121, None, 900300), (35, 176952320, 3100, 1621, 900301), (81, 136579072, 3753, 2449, 900302)]
# noms de boss -> id du vrai PNJ (LISTE-PNJ.txt)
PAR_NOM = {"Koulosse": 755, "Skeunk": 760, "Halouine": 1769, "Sylargh": 2046, "Klime": 2045, "Missiz Frizz": 2040, "Nileza": 2028,
           "Comte Harebourg": 2047, "Merkator": 2197, "Ombre": 2216, "Roi Nidas": 2355, "Malléfisk": 2395, "XLII": 2658, "Vortex": 2659,
           "Reine Nyée": 2846, "Le Chouque": 3017, "Moon": 160, "Qu'Tan": 3225, "Capitaine Meno": 3258, "Dantinéa": 3257,
           "Tal Kasha": 3685, "Comte Razof": 3748, "Anerice la Shushess": 3963, "Ilyzaelle": 4057, "Bethel Akarna": 4302,
           "Solar": 4301, "Dazak Martegel": 4500}

mt = {}
for l in open(os.path.join(ICI, "exports", "monsters_templates.tsv"), encoding="utf-8", errors="replace"):
    p = l.rstrip("\n").split("\t"); mt[int(p[0])] = (int(p[1]), p[3])
txt = d2i.load(os.path.join(RACINE, "CLIENT_64bits", "app", "data", "i18n", "i18n_fr.d2i"))
classes, objs = d2o.load(SRC)

rows = []  # (donjon, salle, boss, pnj, vrai pnj ou None, spawn a modifier ou None)
for l in list(open(os.path.join(ICI, "pnj_boss_rapport.tsv"), encoding="utf-8"))[1:]:
    p = l.rstrip("\n").split("\t")
    did, end, boss, pnj = int(p[0]), int(p[1]), int(p[2]), int(p[3])
    vrai = PAR_NOM.get(txt.get(mt[boss][0], ""))
    rows.append((did, end, boss, pnj, vrai, None))
for i, (did, end, boss, vrai, spawn) in enumerate(EXTRA):
    rows.append((did, end, boss, NEW0 + i, vrai, spawn))

# un seul PNJ par boss (plusieurs donjons peuvent partager le meme boss)
pnj_info = {}
for did, end, boss, pnj, vrai, spawn in rows:
    if vrai is not None and vrai not in objs:
        raise SystemExit("PNJ %d absent du client" % vrai)
    name_id = objs[vrai]["nameId"] if vrai else mt[boss][0]
    look = objs[vrai]["look"] if vrai else mt[boss][1]
    gender = objs[vrai]["gender"] if vrai else 0
    pnj_info[pnj] = (name_id, look, gender, vrai, boss)

# --- Npcs.d2o : original + nos PNJ (ordre des ids)
data = open(SRC, "rb").read()
idx = struct.unpack_from(">i", data, 3)[0]
n = struct.unpack_from(">i", data, idx)[0]
old_index, after = data[idx + 4: idx + 4 + n], data[idx + 4 + n:]
utf = lambda s: struct.pack(">H", len(s.encode("utf-8"))) + s.encode("utf-8")
blob, entries = b"", []
for pnj in sorted(pnj_info):
    name_id, look, gender, vrai, boss = pnj_info[pnj]
    o = struct.pack(">ii", 1, pnj) + struct.pack(">i", name_id)
    o += struct.pack(">i", 1) + struct.pack(">iii", 2, MESSAGE, TEXTE_MESSAGE)
    o += struct.pack(">i", 1) + struct.pack(">iii", 2, REPONSE, TEXTE_REPONSE)
    o += struct.pack(">iI", 1, 3) + struct.pack(">I", gender) + utf(look) + struct.pack(">i", 0) + struct.pack(">i", 0) + b"\x00"
    entries.append(struct.pack(">ii", pnj, idx + len(blob))); blob += o
new_index = old_index + b"".join(entries)
out = data[:3] + struct.pack(">i", idx + len(blob)) + data[7:idx] + blob + struct.pack(">i", len(new_index)) + new_index + after
open(os.path.join(ICI, "client", "Npcs.d2o"), "wb").write(out)
c2, o2 = d2o.load(os.path.join(ICI, "client", "Npcs.d2o"))
assert all(o2[k] == objs[k] for k in objs) and all(o2[p]["look"] == pnj_info[p][1] for p in pnj_info)

# --- SQL
q = lambda s: "'" + s.replace("\\", "\\\\").replace("'", "''") + "'"
sql = ["-- NEKFEU : PNJ de sortie de donjon avec le nom + l'apparence du vrai PNJ (genere par maj_pnj_vrais_skins.py). Apres pnj_boss.sql et sorties_astrub.sql."]
und = ["-- annule pnj_vrais_skins.sql (remet nom + apparence du boss, supprime les PNJ ajoutes)"]
rap = ["donjon\tsalle_fin\tboss\tpnj\tvrai_pnj\tnom\tlook"]
for k, pnj in enumerate(sorted(pnj_info)):
    name_id, look, gender, vrai, boss = pnj_info[pnj]
    nom = txt.get(name_id, "PNJ %d" % pnj)
    if pnj < NEW0:
        if vrai:
            sql.append("UPDATE npcs_templates SET Name=%s, NameId=%d, LookAsString=%s, Gender=%d WHERE Id=%d; -- vrai PNJ %d" % (q(nom), name_id, q(look), gender, pnj, vrai))
            und.append("UPDATE npcs_templates SET Name=%s, NameId=%d, LookAsString=%s, Gender=0 WHERE Id=%d;" % (q(txt.get(mt[boss][0], "?")), mt[boss][0], q(mt[boss][1]), pnj))
    else:
        sql.append("INSERT INTO npcs_templates (Id, Name, NameId, DialogMessagesIdCSV, DialogRepliesIdCSV, ActionsIdsCSV, Gender, LookAsString, SpecialArtworkId, TokenShop) VALUES (%d, %s, %d, '%d,%d', '%d,%d', '3', %d, %s, 0, 0);"
                   % (pnj, q(nom), name_id, MESSAGE, TEXTE_MESSAGE, REPONSE, TEXTE_REPONSE, gender, q(look)))
        sql.append("INSERT INTO npcs_actions (Id, NpcId, Type, Priority, Parameter0) VALUES (%d, %d, 'Talk', 0, '%d');" % (ACTION_NEW0 + pnj - NEW0, pnj, MESSAGE))
        und.append("DELETE FROM npcs_templates WHERE Id=%d;" % pnj)
        und.append("DELETE FROM npcs_actions WHERE Id=%d;" % (ACTION_NEW0 + pnj - NEW0))
for did, end, boss, pnj, vrai, spawn in rows:
    if spawn:
        sql.append("UPDATE npcs_spawns SET NpcId=%d WHERE Id=%d; -- donjon %d" % (pnj, spawn, did))
        und.append("UPDATE npcs_spawns SET NpcId=800 WHERE Id=%d;" % spawn)
    rap.append("%d\t%d\t%d\t%d\t%s\t%s\t%s" % (did, end, boss, pnj, vrai or "-", txt.get(pnj_info[pnj][0], "?"), pnj_info[pnj][1]))
for name, lines in (("pnj_vrais_skins.sql", sql), ("pnj_vrais_skins-annuler.sql", und), ("pnj_vrais_skins_rapport.tsv", rap)):
    open(os.path.join(ICI, name), "w", encoding="utf-8", newline="\n").write("\n".join(lines) + "\n")
print(len(pnj_info), "PNJ dont", sum(1 for v in pnj_info.values() if v[3]), "avec vrai skin ;", len(EXTRA), "ajoutes")
