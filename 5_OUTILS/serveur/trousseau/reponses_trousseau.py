# -*- coding: utf-8 -*-
# MYSTIC : ajoute aux gardiens de donjon la reponse officielle « Utiliser le trousseau de clefs » (texte du client),
# type serveur DungeonEnterTrousseau (visible seulement si le trousseau a encore la clef du donjon).
# Pour chaque reponse DungeonEnter (message M) : PNJ du client dont un message est M → sa reponse « trousseau ».
# Sortie : reponses_trousseau.sql (rejouable) + rapport des gardiens sans reponse trousseau dans le client.
import os, sys, subprocess
sys.stdout.reconfigure(encoding='utf-8', errors='replace')
ICI = os.path.dirname(os.path.abspath(__file__)); RACINE = os.path.dirname(os.path.dirname(os.path.dirname(ICI)))
sys.path.insert(0, os.path.join(RACINE, '_OUTILS', 'serveur', 'donjons'))
import d2o, d2i
MYSQL = os.path.join(RACINE, '_OUTILS', 'logiciels', 'MariaDB', 'bin', 'mysql.exe')
def q(sql):
    out = subprocess.run([MYSQL, '-uroot', '-h127.0.0.1', '-N', '-B', 'world', '-e', sql], capture_output=True, check=True).stdout.decode('utf-8')
    return [l.split('\t') for l in out.splitlines() if l]
T = d2i.load(os.path.join(RACINE, 'CLIENT_64bits', 'app', 'data', 'i18n', 'i18n_fr.d2i'))
_, npcs = d2o.load(os.path.join(RACINE, 'CLIENT_64bits', 'app', 'data', 'common', 'Npcs.d2o'))
trousseau_txt = {k for k, v in T.items() if 'trousseau de clefs' in v.lower() and len(v) < 90}
par_message = {}
for nid, npc in npcs.items():
    for m in npc.get('dialogMessages') or []:
        par_message.setdefault(m[0], []).append(nid)
lignes, manquants = [], []
for rid, mid in q("select ReplyId, MessageId from npcs_replies where Type='DungeonEnter'"):
    rid, mid = int(rid), int(mid)
    trouve = None
    for nid in par_message.get(mid, []):
        for r in npcs[nid].get('dialogReplies') or []:
            if len(r) > 1 and r[1] in trousseau_txt:
                trouve = (nid, r[0]); break
        if trouve: break
    if trouve:
        lignes.append((trouve[1], mid, trouve[0]))
    else:
        noms = ', '.join(T.get(npcs[n]['nameId'], '?') for n in par_message.get(mid, [])[:2]) or '?'
        manquants.append((rid, mid, noms))
with open(os.path.join(ICI, 'reponses_trousseau.sql'), 'w', encoding='utf-8', newline='\n') as f:
    f.write("-- MYSTIC 17/10 : reponse « Utiliser le trousseau de clefs » chez les gardiens de donjon (visible si le trousseau a la clef). Rejouable.\n")
    f.write("DELETE FROM npcs_replies WHERE Type='DungeonEnterTrousseau';\n")
    for rid, mid, nid in sorted(set(lignes)):
        f.write(f"INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnterTrousseau', {rid}, {mid}); -- PNJ {nid} {T.get(npcs[nid]['nameId'],'')}\n")
print(len(set(lignes)), 'reponses trousseau ;', len(manquants), 'gardiens sans reponse trousseau dans le client :')
for m in manquants: print('  ', m)
