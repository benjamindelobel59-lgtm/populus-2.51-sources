# NEKFEU 2026-10-16 : entrees de donjon -> vrai gardien du client a la place du PNJ generique Zamawa (707).
# Entree : gardiens.tsv produit par associer_gardiens.py (donjon, nom, map d'entree, case, gardien, nom, reponses, raison).
# Ne garde que les associations sures (gardien deja pose sur la map d'entree) ou probables (>= 2 mots du nom du donjon
# dans ses dialogues, + quelques noms tres distinctifs). Les autres entrees gardent Zamawa.
# Sorties : gardiens_entree.sql / -annuler.sql / _rapport.tsv
import os, re, subprocess, sys
D = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, D)
import d2o, d2i
MYSQL = os.path.join(os.path.dirname(D), "MariaDB", "bin", "mysql.exe")
ID0 = 900400  # npcs_actions / npcs_replies ajoutes
EXCLUS = {183}  # Oto Mustam : PNJ generique qui ressort pour plusieurs donjons
UN_MOT_OK = {"49": 783, "41": 781, "82": 2662}  # Salle du Minotot -> Lorkos, Scarafeuilles -> El Scarador, Vortex -> Tessie Oude
# relu a la main : PNJ de quete qui citent le donjon par hasard -> refuses ; gardiens evidents juges ambigus -> imposes
REFUSES = {"13", "68", "104"}  # Meulou -> Nosya la Ronge, Dramak -> Captain Amakna, Tour de Solar -> La Voix des Douze
MANUEL = {"11": 775, "12": 712, "23": 1384, "32": 795, "33": 795, "34": 795, "100": 3966, "15": 772}  # Bowork, Bworkie, G.O. Kanniboul, Hector Nowel x3, Guichetiere du Magik Riktus, Krakotte


def q(sql):
    r = subprocess.run([MYSQL, "-uroot", "world", "-B", "-N", "-e", sql], capture_output=True, text=True, encoding="utf-8")
    if r.returncode:
        raise SystemExit(r.stderr)
    return [l.split("\t") for l in r.stdout.splitlines() if l]


if q("SELECT 1 FROM npcs_replies WHERE Id = %d" % ID0):
    raise SystemExit("deja applique (npcs_replies %d existe)" % ID0)
txt = d2i.load(os.path.join(os.path.dirname(os.path.dirname(D)), "CLIENT_64bits", "app", "data", "i18n", "i18n_fr.d2i"))
_, npcs = d2o.load(os.path.join(D, "client", "Npcs.original.d2o"))
messages = {int(r[0]) for r in q("SELECT Id FROM npcs_messages")}
talks = {}
for nid, aid, cond, p0 in q("SELECT NpcId, Id, IFNULL(`Condition`,''), Parameter0 FROM npcs_actions WHERE Type='Talk'"):
    if cond == "":
        talks.setdefault(int(nid), int(p0))

PREF = [r"donner la clef et entrer", r"entrer dans (le|la|les) ", r"^entrer", r"accepter et utiliser la clef", r"utiliser la clef", r"trousseau"]


def entry_reply(nid):
    reps = [(r, txt.get(t, "") or "") for r, t in npcs[nid]["dialogReplies"]]
    for p in PREF:
        for r, s in reps:
            if re.search(p, s, re.I) and not re.search(r"ne pas|recommencer|sortir", s, re.I):
                return r, s
    return None, None


def entry_message(nid):
    msgs = [(m, txt.get(t, "") or "") for m, t in npcs[nid]["dialogMessages"] if m < 65536]
    for m, s in msgs:
        if re.search(r"clef|cl[ée]|donjon|entrer", s, re.I):
            return m
    return msgs[0][0] if msgs else None


sql = ["-- NEKFEU : vrais gardiens d'entree de donjon a la place de Zamawa (707). Genere par generer_gardiens_entree.py. Serveur World ARRETE."]
und = ["-- annule gardiens_entree.sql"]
rap = ["donjon\tnom\tmap entree\tgardien\tnom gardien\treponse\tmessage\traison"]
nid_seq = ID0
done_msg = set()
skipped = []
for line in open(sys.argv[1], encoding="utf-8"):
    p = line.rstrip("\n").split("\t")
    did, name, ent, kcell, keeper, kname, reps, how = p[0], p[1], int(p[2]), p[3], p[4], p[5], p[6], p[7]
    if did in MANUEL:
        keeper, how = str(MANUEL[did]), "manuel"
        kname = txt.get(npcs[MANUEL[did]]["nameId"], "?")
    if keeper == "-" or did in REFUSES:
        skipped.append((did, name, "refuse a la relecture" if did in REFUSES else how)); continue
    keeper = int(keeper)
    sure = how.startswith("deja")
    m = re.match(r"texte \((\d+) mots", how)
    ok = sure or how == "manuel" or (m and int(m.group(1)) >= 2) or UN_MOT_OK.get(did) == keeper
    if not ok or keeper in EXCLUS:
        skipped.append((did, name, "rejete : %s %s (%s)" % (keeper, kname, how))); continue
    rid, rtext = entry_reply(keeper)
    if rid is None:
        skipped.append((did, name, "rejete : %s %s sans reponse d'entree" % (keeper, kname))); continue
    msg = talks.get(keeper)
    if msg is None:
        msg = entry_message(keeper)
        if msg is None:
            skipped.append((did, name, "rejete : %s %s sans message" % (keeper, kname))); continue
        sql.append("INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES (%d, %d, 'Talk', 'Mp=%d', 0, '%d');" % (nid_seq, keeper, ent, msg))
        und.append("DELETE FROM npcs_actions WHERE Id = %d;" % nid_seq); nid_seq += 1
    if msg not in messages:
        text_id = dict(npcs[keeper]["dialogMessages"]).get(msg)
        sql.append("INSERT INTO npcs_messages (Id, MessageId, ParametersCSV) VALUES (%d, %d, '');" % (msg, text_id))
        und.append("DELETE FROM npcs_messages WHERE Id = %d;" % msg); messages.add(msg)
    if (msg, rid) not in done_msg:
        sql.append("INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId) VALUES (%d, 'DungeonEnter', %d, %d);" % (nid_seq, rid, msg))
        und.append("DELETE FROM npcs_replies WHERE Id = %d;" % nid_seq); nid_seq += 1
        done_msg.add((msg, rid))
    # PNJ sur la map : le gardien remplace Zamawa (ou Zamawa est retire si le gardien y est deja)
    zam = q("SELECT Id, CellId, Direction FROM npcs_spawns WHERE MapId = %d AND NpcId = 707" % ent)
    already = q("SELECT Id FROM npcs_spawns WHERE MapId = %d AND NpcId = %d" % (ent, keeper))
    for sid, cell, direction in zam:
        if already:
            sql.append("DELETE FROM npcs_spawns WHERE Id = %s; -- Zamawa (gardien %d deja sur la map)" % (sid, keeper))
            und.append("INSERT INTO npcs_spawns (Id, NpcId, MapId, CellId, Direction) VALUES (%s, 707, %d, %s, %s);" % (sid, ent, cell, direction))
        else:
            sql.append("UPDATE npcs_spawns SET NpcId = %d WHERE Id = %s; -- Zamawa -> %s" % (keeper, sid, kname))
            und.append("UPDATE npcs_spawns SET NpcId = 707 WHERE Id = %s;" % sid)
            already = [[sid]]
    rap.append("%s\t%s\t%d\t%d\t%s\t%d %s\t%d\t%s" % (did, name, ent, keeper, kname, rid, rtext, msg, how))

for name, lines in (("gardiens_entree.sql", sql), ("gardiens_entree-annuler.sql", und), ("gardiens_entree_rapport.tsv", rap)):
    open(os.path.join(D, name), "w", encoding="utf-8", newline="\n").write("\n".join(lines) + "\n")
with open(os.path.join(D, "gardiens_entree_restants.tsv"), "w", encoding="utf-8") as o:
    o.write("donjon\tnom\traison (garde Zamawa)\n")
    for x in skipped:
        o.write("\t".join(x) + "\n")
print("gardiens poses :", len(rap) - 1, "| restent sur Zamawa :", len(skipped))
