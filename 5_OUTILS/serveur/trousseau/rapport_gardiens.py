# -*- coding: utf-8 -*-
# MYSTIC : rapport des PNJ devant chaque donjon (dungeons_entries) : message de dialogue sur la map d'entree,
# reponses serveur (npcs_replies) avec leur texte client, et reponses « clef » / « trousseau » disponibles dans le client.
import os, sys, subprocess, re
sys.stdout.reconfigure(encoding='utf-8', errors='replace')
ICI = os.path.dirname(os.path.abspath(__file__)); RACINE = os.path.dirname(os.path.dirname(os.path.dirname(ICI)))
sys.path.insert(0, os.path.join(RACINE, '_OUTILS', 'serveur', 'donjons'))
import d2o, d2i
MYSQL = os.path.join(RACINE, '_OUTILS', 'logiciels', 'MariaDB', 'bin', 'mysql.exe')
def q(sql):
    out = subprocess.run([MYSQL, '-uroot', '-h127.0.0.1', '-N', '-B', 'world', '-e', sql], capture_output=True, check=True).stdout.decode('utf-8')
    return [l.split('\t') for l in out.replace('\r', '').split('\n') if l]
T = d2i.load(os.path.join(RACINE, 'CLIENT_64bits', 'app', 'data', 'i18n', 'i18n_fr.d2i'))
_, npcs = d2o.load(os.path.join(RACINE, 'CLIENT_64bits', 'app', 'data', 'common', 'Npcs.d2o'))
def rtext(nid, rid):
    for r in (npcs.get(nid) or {}).get('dialogReplies') or []:
        if r[0] == rid: return T.get(r[1], '?')
    return None
entries = q("select DungeonId, EntranceMapId, KeyItemId from dungeons_entries order by DungeonId")
spawns = {}
for sid, nid, mid, cell in q("select Id, NpcId, MapId, CellId from npcs_spawns"):
    spawns.setdefault(int(mid), []).append((int(sid), int(nid), int(cell)))
actions = {}
for nid, cond, msg in q("select NpcId, replace(ifnull(`Condition`,''),char(10),' '), ifnull(Parameter0,0) from npcs_actions where Type='Talk'"):
    actions.setdefault(int(nid), []).append((cond, int(msg)))
replies = {}
for rid, typ, msg, p0 in q("select ReplyId, Type, ifnull(MessageId,0), replace(replace(ifnull(Parameter0,''),char(10),' '),char(9),' ') from npcs_replies"):
    replies.setdefault(int(msg), []).append((int(rid), typ, p0))
out = open(os.path.join(ICI, 'rapport_gardiens.txt'), 'w', encoding='utf-8')
for dj, emap, key in entries:
    emap = int(emap)
    out.write(f"\n=== donjon {dj} map {emap} clef {key}\n")
    for sid, nid, cell in spawns.get(emap, []):
        nom = T.get((npcs.get(nid) or {}).get('nameId', 0), '?')
        out.write(f"  PNJ {nid} {nom} (spawn {sid}, case {cell})\n")
        msgs = [m for c, m in actions.get(nid, []) if not c or str(emap) in c]
        for m in msgs:
            out.write(f"    message {m} : {T.get(dict((x[0], x[1]) for x in (npcs.get(nid) or {}).get('dialogMessages') or []).get(m, 0), '')[:80]}\n")
            for rid, typ, p0 in replies.get(m, []):
                out.write(f"      reponse {rid} {typ} {p0} : {rtext(nid, rid)}\n")
        cle = [(r[0], T.get(r[1], '')) for r in (npcs.get(nid) or {}).get('dialogReplies') or [] if re.search(r'clef|clé|trousseau', T.get(r[1], ''), re.I)]
        for rid, t in cle:
            out.write(f"    client : reponse {rid} = {t[:90]}\n")
out.close()
print('ok')
