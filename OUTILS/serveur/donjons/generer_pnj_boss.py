# NEKFEU : PNJ de sortie de donjon = skin + nom du boss (remplace le Portier 800 generique)
# Entrees : exports/boss_donjons.tsv (donjon, salle de fin, boss), exports/monsters_templates.tsv, Npcs.d2o du client
# Sorties : client/Npcs.d2o (PNJ ajoutes), pnj_boss.sql, pnj_boss-annuler.sql, pnj_boss_rapport.tsv
import os, struct, sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import d2o, d2i
ICI = os.path.dirname(os.path.abspath(__file__))
RACINE = os.path.dirname(os.path.dirname(ICI))
SRC = os.path.join(RACINE, "_OUTILS", "donjons", "client", "Npcs.original.d2o")
PREMIER_PNJ, MESSAGE, TEXTE_MESSAGE, REPONSE, TEXTE_REPONSE = 10016, 60001, 24359, 2962, 767152  # « Félicitations ! » / « Sortir du donjon. »
# MESSAGE < 65536 : le serveur envoie l'id du message en ushort (ContextRoleplayNpcHandler), 900001 arrivait tronque
ACTION0, REPLY_ID = 900067, 900068

mt = {}
for l in open(os.path.join(ICI, "exports", "monsters_templates.tsv"), encoding="utf-8", errors="replace"):
    p = l.rstrip("\n").split("\t"); mt[int(p[0])] = (int(p[1]), p[3])
txt = d2i.load(os.path.join(RACINE, "CLIENT_64bits", "app", "data", "i18n", "i18n_fr.d2i"))
rows = []
for l in open(os.path.join(ICI, "exports", "boss_donjons.tsv"), encoding="latin-1"):
    p = l.rstrip("\n").split("\t"); rows.append((int(p[0]), int(p[1]), int(p[2])))
bosses = sorted({b for _, _, b in rows})
npc_of = {b: PREMIER_PNJ + i for i, b in enumerate(bosses)}

classes, objs = d2o.load(SRC)
assert max(objs) < PREMIER_PNJ
data = open(SRC, "rb").read()
idx = struct.unpack_from(">i", data, 3)[0]
n = struct.unpack_from(">i", data, idx)[0]
old_index = data[idx + 4: idx + 4 + n]
after = data[idx + 4 + n:]
def utf(s): b = s.encode("utf-8"); return struct.pack(">H", len(b)) + b
blob, entries = b"", []
for b in bosses:
    o = struct.pack(">ii", 1, npc_of[b]) + struct.pack(">i", mt[b][0])
    o += struct.pack(">i", 1) + struct.pack(">iii", 2, MESSAGE, TEXTE_MESSAGE)
    o += struct.pack(">i", 1) + struct.pack(">iii", 2, REPONSE, TEXTE_REPONSE)
    o += struct.pack(">iI", 1, 3) + struct.pack(">I", 0) + utf(mt[b][1]) + struct.pack(">i", 0) + struct.pack(">i", 0) + b"\x00"
    entries.append(struct.pack(">ii", npc_of[b], idx + len(blob))); blob += o
new_index = old_index + b"".join(entries)
out = data[:3] + struct.pack(">i", idx + len(blob)) + data[7:idx] + blob + struct.pack(">i", len(new_index)) + new_index + after
os.makedirs(os.path.join(ICI, "client"), exist_ok=True)
open(os.path.join(ICI, "client", "Npcs.d2o"), "wb").write(out)
c2, o2 = d2o.load(os.path.join(ICI, "client", "Npcs.d2o"))
assert len(o2) == len(objs) + len(bosses) and all(o2[k] == objs[k] for k in objs)

q = lambda s: "'" + s.replace("\\", "\\\\").replace("'", "''") + "'"
sql = ["-- NEKFEU : PNJ de sortie de donjon avec skin + nom du boss (genere par generer_pnj_boss.py)",
       "INSERT INTO npcs_messages (Id, MessageId, ParametersCSV) VALUES (%d, %d, '');" % (MESSAGE, TEXTE_MESSAGE),
       "INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) VALUES (%d, 'DungeonExit', %d, %d);" % (REPLY_ID, REPONSE, MESSAGE)]
und = ["-- annule pnj_boss.sql",
       "DELETE FROM npcs_messages WHERE Id=%d;" % MESSAGE, "DELETE FROM npcs_replies WHERE Id=%d;" % REPLY_ID,
       "DELETE FROM npcs_templates WHERE Id BETWEEN %d AND %d;" % (PREMIER_PNJ, PREMIER_PNJ + len(bosses) - 1),
       "DELETE FROM npcs_actions WHERE Id BETWEEN %d AND %d;" % (ACTION0 + 2, ACTION0 + 1 + len(bosses))]
for i, b in enumerate(bosses):
    nid = npc_of[b]; nom = txt.get(mt[b][0], "Boss %d" % b)
    sql.append("INSERT INTO npcs_templates (Id, Name, NameId, DialogMessagesIdCSV, DialogRepliesIdCSV, ActionsIdsCSV, Gender, LookAsString, SpecialArtworkId, TokenShop) VALUES (%d, %s, %d, '%d,%d', '%d,%d', '3', 0, %s, 0, 0);"
               % (nid, q(nom), mt[b][0], MESSAGE, TEXTE_MESSAGE, REPONSE, TEXTE_REPONSE, q(mt[b][1])))
    sql.append("INSERT INTO npcs_actions (Id, NpcId, Type, Priority, Parameter0) VALUES (%d, %d, 'Talk', 0, '%d');" % (ACTION0 + 2 + i, nid, MESSAGE))
rap = ["donjon\tsalle_fin\tboss\tpnj\tnom\tlook"]
for did, end, b in rows:
    sql.append("UPDATE npcs_spawns SET NpcId=%d WHERE Id>=900001 AND NpcId=800 AND MapId=%d;" % (npc_of[b], end))
    und.append("UPDATE npcs_spawns SET NpcId=800 WHERE Id>=900001 AND NpcId=%d AND MapId=%d;" % (npc_of[b], end))
    rap.append("%d\t%d\t%d\t%d\t%s\t%s" % (did, end, b, npc_of[b], txt.get(mt[b][0], "?"), mt[b][1]))
# le client lit le texte d'un message dans NpcMessages.d2o (id -> texte) : on y ajoute le message 900001
SRC_MSG = os.path.join(ICI, "client", "NpcMessages.original.d2o")
_, msgs = d2o.load(SRC_MSG)
assert MESSAGE not in msgs
data = open(SRC_MSG, "rb").read()
idx = struct.unpack_from(">i", data, 3)[0]
n = struct.unpack_from(">i", data, idx)[0]
blob = struct.pack(">iii", 1, MESSAGE, TEXTE_MESSAGE) + struct.pack(">i", 0)
new_index = data[idx + 4: idx + 4 + n] + struct.pack(">ii", MESSAGE, idx)
out = data[:3] + struct.pack(">i", idx + len(blob)) + data[7:idx] + blob + struct.pack(">i", len(new_index)) + new_index + data[idx + 4 + n:]
open(os.path.join(ICI, "client", "NpcMessages.d2o"), "wb").write(out)
_, m2 = d2o.load(os.path.join(ICI, "client", "NpcMessages.d2o"))
assert len(m2) == len(msgs) + 1 and m2[MESSAGE] == {"id": MESSAGE, "messageId": TEXTE_MESSAGE, "messageParams": []}

for name, lines in (("pnj_boss.sql", sql),("pnj_boss-annuler.sql", und), ("pnj_boss_rapport.tsv", rap)):
    open(os.path.join(ICI, name), "w", encoding="utf-8", newline="\n").write("\n".join(lines) + "\n")
print(len(bosses), "PNJ,", len(rows), "donjons")
