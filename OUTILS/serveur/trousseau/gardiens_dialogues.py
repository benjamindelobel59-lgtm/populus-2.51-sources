# -*- coding: utf-8 -*-
# MYSTIC 17/10 : dialogues des vrais gardiens de donjon (base VPS) : sur le message principal du gardien (map d'entree),
#  - reponse « donner / presenter / utiliser la clef » (texte client) → DungeonEnter (clef consommee ; sans clef si KeyItemId = 0)
#  - reponse « trousseau » (texte client) → DungeonEnterTrousseau (visible seulement si le trousseau a la clef)
#  - anciennes reponses Teleport vers la 1re salle (entree sans clef) retirees ; Zamawa (PNJ 707) retire la ou un vrai gardien existe.
# Exclus : labyrinthe du Minotoror (donjons 3 et 49, meme dialogue de Lorkos, mecanique propre).
# Une reponse n'est remplacee que si sa remplacante existe (ex. gardien Koalak : pas de texte « clef » → sa reponse clef actuelle est gardee). Sortie : gardiens_dialogues.sql + rapport.
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
EXCLUS = {3, 49}  # labyrinthe du Minotoror + Minotot (meme message de Lorkos)
ZAMAWA = 707
CLE = re.compile(r"^(Donner|Présenter|Presenter|Accepter et utiliser|Utiliser|Montrer|Entrer).*\bcl(ef|é)", re.I)

spawns = {}
for sid, nid, mid in q("select Id, NpcId, MapId from npcs_spawns"):
    spawns.setdefault(int(mid), []).append((int(sid), int(nid)))
actions = {}
for aid, nid, cond, msg in q("select Id, NpcId, replace(ifnull(`Condition`,''),char(10),' '), ifnull(Parameter0,0) from npcs_actions where Type='Talk' order by Id"):
    actions.setdefault(int(nid), []).append((cond, int(msg)))

sql, rapport, faits = [], [], 0
for dj, emap, key, first in q("select DungeonId, EntranceMapId, KeyItemId, FirstRoomMapId from dungeons_entries order by DungeonId"):
    dj, emap, key, first = int(dj), int(emap), int(key), int(first)
    if dj in EXCLUS:
        rapport.append(f"donjon {dj} : exclu (mecanique propre)"); continue
    reels = [(s, n) for s, n in spawns.get(emap, []) if n != ZAMAWA]
    for sid, nid in reels:
        reps = [(r[0], T.get(r[1], '')) for r in (npcs.get(nid) or {}).get('dialogReplies') or []]
        cle = [r for r in reps if CLE.search(r[1]) and 'trousseau' not in r[1].lower()]
        tr = [r for r in reps if 'trousseau' in r[1].lower() and not re.search(r"et la cl|deux cl|Affronter", r[1], re.I)]
        if not cle and not tr:
            continue
        msgs = [m for c, m in actions.get(nid, []) if not c or str(emap) in c]
        if not msgs:
            rapport.append(f"donjon {dj} : PNJ {nid} sans dialogue sur la map {emap}"); continue
        m = msgs[0]
        cle.sort(key=lambda r: (not r[1].lower().startswith('donner la clef'), len(r[1])))
        tr.sort(key=lambda r: ('uniquement' not in r[1].lower(), r[1] != 'Utiliser le trousseau de clefs.', len(r[1])))
        nom = T.get(npcs[nid]['nameId'], '?')
        sql.append(f"-- donjon {dj} : {nom} (PNJ {nid}), message {m}")
        types = (["'DungeonEnter'"] if cle else []) + (["'DungeonEnterTrousseau'"] if tr and key > 0 else [])
        cond = f"Type IN ({','.join(types)})" if types else "0"
        if cle:
            cond += f" OR (Type='Teleport' AND Parameter0='{first}')"
        sql.append(f"DELETE FROM npcs_replies WHERE MessageId={m} AND ({cond});")
        if cle:
            sql.append(f"INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnter', {cle[0][0]}, {m}); -- {cle[0][1]}")
        if tr and key > 0:
            sql.append(f"INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnterTrousseau', {tr[0][0]}, {m}); -- {tr[0][1]}")
        rapport.append(f"donjon {dj} : {nom} msg {m} | clef : {cle[0][1] if cle else '-'} | trousseau : {tr[0][1] if tr and key > 0 else '-'}")
        faits += 1
    if reels and any(n == ZAMAWA for _, n in spawns.get(emap, [])) and any(l.startswith(f'-- donjon {dj} :') for l in sql):
        sql.append(f"DELETE FROM npcs_spawns WHERE NpcId={ZAMAWA} AND MapId={emap}; -- Zamawa remplace par le vrai gardien")
with open(os.path.join(ICI, 'gardiens_dialogues.sql'), 'w', encoding='utf-8', newline='\n') as f:
    f.write("-- MYSTIC 17/10 : dialogues des vrais gardiens de donjon (clef / trousseau). Genere par gardiens_dialogues.py. Rejouable.\n")
    f.write('\n'.join(sql) + '\n')
open(os.path.join(ICI, 'gardiens_dialogues_rapport.txt'), 'w', encoding='utf-8').write('\n'.join(rapport) + '\n')
print(faits, 'gardiens mis a jour ;', sum(1 for l in sql if l.startswith('DELETE FROM npcs_spawns')), 'Zamawa retires')
print('\n'.join(rapport))
