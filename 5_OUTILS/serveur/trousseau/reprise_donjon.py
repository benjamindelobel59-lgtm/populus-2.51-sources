# -*- coding: utf-8 -*-
"""
MYSTIC 17/10/2026 : « Reprendre le donjon où vous l'avez quitté » chez les gardiens + suppression de « Téléporter les membres
du groupe » (choix de l'utilisateur). Pour chaque entrée de donjon (dungeons_entries) : PNJ présents sur la carte d'entrée ->
messages qui ont une réponse DungeonEnter -> réponse officielle du client « Reprendre … où vous l'avez quitté(e) » de ce PNJ
ajoutée en type DungeonResume (Parameter0 = DungeonId). Les gardiens sans ce texte dans le client sont listés dans le rapport.
Sortie : 34_reprise_donjon.sql (rejouable) + reprise_donjon_rapport.txt
Utilisation : python -I reprise_donjon.py
"""
import collections
import os
import re
import subprocess
import sys

sys.stdout.reconfigure(encoding='utf-8')
ICI = os.path.dirname(os.path.abspath(__file__))
RACINE = os.path.dirname(os.path.dirname(os.path.dirname(ICI)))
sys.path.insert(0, os.path.join(RACINE, '_OUTILS', 'serveur', 'donjons'))
import d2o, d2i

B = os.path.join(RACINE, 'CLIENT_64bits', 'app', 'data')
MYSQL = os.path.join(RACINE, '_OUTILS', 'logiciels', 'MariaDB', 'bin', 'mysql.exe')
T = d2i.load(os.path.join(B, 'i18n', 'i18n_fr.d2i'))
_, N = d2o.load(os.path.join(B, 'common', 'Npcs.d2o'))


def q(sql):
    out = subprocess.run([MYSQL, '-uroot', '-h127.0.0.1', '-N', '-B', 'world', '-e', sql], capture_output=True, check=True).stdout.decode('utf-8')
    return [l.split('\t') for l in out.splitlines() if l]


REPRISE = re.compile(r"o[uù] (vous l'avez|tu l'as) quitt", re.I)
GROUPE = re.compile(r"^\s*t[ée]l[ée]porter les membres du groupe", re.I)

# réponses « Téléporter les membres du groupe » (tous PNJ du client)
groupe = sorted({r[0] for o in N.values() for r in o.get('dialogReplies', []) if GROUPE.search(T.get(r[1]) or '')})

entrees = q("SELECT DungeonId, EntranceMapId FROM dungeons_entries")
spawns = collections.defaultdict(list)
for npc, carte in q("SELECT NpcId, MapId FROM npcs_spawns"):
    spawns[int(carte)].append(int(npc))
talk = collections.defaultdict(list)
for npc, msg, cond in q("SELECT NpcId, Parameter0, IFNULL(`Condition`,'') FROM npcs_actions WHERE Type='Talk'"):
    if msg.isdigit():
        talk[int(npc)].append((int(msg), cond))
entrer = collections.defaultdict(set)
for msg, in q("SELECT DISTINCT MessageId FROM npcs_replies WHERE Type IN ('DungeonEnter','DungeonEnterTrousseau')"):
    entrer[int(msg)] = True

lignes = ["-- MYSTIC 17/10/2026 : « Reprendre le donjon où vous l'avez quitté » (DungeonResume, Parameter0 = donjon) chez les gardiens,",
          "-- et suppression de « Téléporter les membres du groupe » (choix de l'utilisateur). Généré par _OUTILS/serveur/trousseau/reprise_donjon.py.",
          "-- Nécessite le WorldServer du 17/10 17h10+ (RepriseDonjons). Rejouable.",
          "DELETE FROM npcs_replies WHERE Type = 'DungeonResume';",
          f"DELETE FROM npcs_replies WHERE Type = 'DungeonEnter' AND ReplyId IN ({','.join(str(x) for x in groupe)});"]
rapport, faits, manquants = [], 0, []
vus = set()
for did, carte in entrees:
    did, carte = int(did), int(carte)
    for npc in spawns.get(carte, []):
        o = N.get(npc)
        reps = [r for r in (o.get('dialogReplies', []) if o else []) if REPRISE.search(T.get(r[1]) or '')]
        msgs = [m for m, cond in talk.get(npc, []) if m in entrer and (not cond or 'Mp=' not in cond or str(carte) in cond)]
        if not msgs:
            continue
        if not reps:
            manquants.append(f"donjon {did} carte {carte} PNJ {npc} {T.get(o['nameId']) if o else '?'} : pas de texte « Reprendre » dans le client")
            continue
        rid = reps[0][0]
        for m in msgs:
            if (m, rid, did) in vus:
                continue
            vus.add((m, rid, did))
            lignes.append(f"INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('DungeonResume', {rid}, {m}, '{did}'); -- {T.get(reps[0][1])}")
            faits += 1
lignes += ["-- Contrôle : attendu groupe_restants = 0",
           f"SELECT (SELECT COUNT(*) FROM npcs_replies WHERE Type = 'DungeonResume') AS reprises,",
           f"       (SELECT COUNT(*) FROM npcs_replies WHERE Type = 'DungeonEnter' AND ReplyId IN ({','.join(str(x) for x in groupe)})) AS groupe_restants;"]
open(os.path.join(ICI, '34_reprise_donjon.sql'), 'w', encoding='utf-8', newline='\n').write('\n'.join(lignes) + '\n')
open(os.path.join(ICI, 'reprise_donjon_rapport.txt'), 'w', encoding='utf-8', newline='\n').write(
    f"{faits} réponses « Reprendre » ajoutées, {len(groupe)} réponses « Téléporter les membres du groupe » supprimées\n\n" + '\n'.join(manquants) + '\n')
print(faits, 'reprises ;', len(manquants), 'gardiens sans texte ;', len(groupe), 'replies groupe')
print('\n'.join(manquants))
