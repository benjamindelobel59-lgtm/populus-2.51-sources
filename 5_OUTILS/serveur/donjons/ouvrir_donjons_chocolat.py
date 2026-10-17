# -*- coding: utf-8 -*-
"""
MYSTIC 17/10/2026 : ouvre le Temple du dieu Kao (111), la Croquanterie (112) et la Fonderie des Waddicts (113)
(choix de l'utilisateur). Données du client 2.51 : gardiens (Npcs.d2o), clefs, monstres des sous-zones, boss.
- salles 1-4 : monstres officiels de la zone, 4 joueurs -> 4 monstres, 5..8 joueurs -> autant de monstres ; salle 5 : boss + monstres
- gardiens créés (modèle + dialogue officiel) et placés sur une case libre de la carte d'entrée ; entrée de donjon déclarée
Sortie : 30_donjons_chocolat.sql (rejouable). Utilisation : python -I ouvrir_donjons_chocolat.py
"""
import os, random, subprocess, sys
sys.stdout.reconfigure(encoding='utf-8')
ICI = os.path.dirname(os.path.abspath(__file__)); RACINE = os.path.dirname(os.path.dirname(os.path.dirname(ICI)))
sys.path.insert(0, ICI)
import d2o, d2i
B = os.path.join(RACINE, 'CLIENT_64bits', 'app', 'data')
MYSQL = os.path.join(RACINE, '_OUTILS', 'logiciels', 'MariaDB', 'bin', 'mysql.exe')
T = d2i.load(os.path.join(B, 'i18n', 'i18n_fr.d2i'))
_, N = d2o.load(os.path.join(B, 'common', 'Npcs.d2o'))

def q(sql):
    out = subprocess.run([MYSQL, '-uroot', '-h127.0.0.1', '-N', '-B', 'world', '-e', sql], capture_output=True, check=True).stdout.decode('utf-8')
    return [l.split('\t') for l in out.splitlines() if l]

DONJONS = [
    # (id, nom, entrée, clef, salles dans l'ordre (spawn, map), boss, monstres, gardien, message, clef, trousseau, groupe, case gardien)
    (111, 'Temple du dieu Kao', 197133314, 20920, [461, 462, 463, 464, 465], 5666, [5668, 5669, 5670, 5671, 5672],
     4818, 33783, 43391, 43392, 43393),
    (112, 'Croquanterie', 197526528, 20919, [455, 454, 453, 452, 451], 5664, [5677, 5678, 5679, 5680, 5681],
     4817, 33782, 43408, 43409, 43410),
    (113, 'Fonderie des Waddicts', 196346371, 20918, [456, 457, 458, 459, 460], 5653, [5652, 5689, 5690, 5691, 5692],
     4819, 33781, 43399, 43400, 43401),
]

grades = {}
for mid, gid, grade in q("SELECT Id, MonsterId, GradeId FROM monsters_grades WHERE MonsterId IN (5653,5664,5666,5652,5689,5690,5691,5692,5677,5678,5679,5680,5681,5668,5669,5670,5671,5672)"):
    grades.setdefault(int(gid), {})[int(grade)] = int(mid)
salles = {int(r[0]): int(r[1]) for r in q("SELECT Id, MapId FROM monsters_spawns_dungeons WHERE Id BETWEEN 451 AND 465")}
libres = {}
for l in open(os.path.join(RACINE, 'SERVEUR_Emu', 'run', 'debug', 'WorldServer', 'cases_client.tsv'), encoding='utf-8'):
    p = l.rstrip('\n').split('\t')
    if len(p) > 1:
        libres[int(p[0])] = [int(c) for c in p[1].split(',') if c]

rnd = random.Random(17102026)
o = ['-- MYSTIC 17/10/2026 : ouverture du Temple du dieu Kao, de la Croquanterie et de la Fonderie des Waddicts.',
     '-- Généré par _OUTILS/serveur/donjons/ouvrir_donjons_chocolat.py (données du client 2.51). Rejouable.', '']
for (did, nom, entree, clef, spawns, boss, pool, pnj, msg, r_clef, r_tr, r_grp) in DONJONS:
    npc = N[pnj]
    cases = libres.get(entree) or [300]
    case = cases[len(cases) // 2]
    msgs = ';'.join(f'{m[0]},{m[1]}' for m in npc['dialogMessages'])
    reps = ';'.join(f'{r[0]},{r[1]}' for r in npc['dialogReplies'])
    nom_pnj = T.get(npc['nameId']).replace("'", "''")
    o += [f'-- ===== {nom} (donjon {did}) : gardien {nom_pnj} (PNJ {pnj}), clef {clef}',
          f"DELETE FROM npcs_templates WHERE Id = {pnj};",
          f"INSERT INTO npcs_templates (Id, Name, NameId, DialogMessagesIdCSV, DialogRepliesIdCSV, ActionsIdsCSV, Gender, LookAsString, SpecialArtworkId, TokenShop)"
          f" VALUES ({pnj}, '{nom_pnj}', {npc['nameId']}, '{msgs}', '{reps}', '{','.join(str(a) for a in npc['actions'])}', {npc['gender']}, '{npc['look']}', 0, 0);",
          "REPLACE INTO npcs_messages (Id, MessageId, ParametersCSV) VALUES " + ', '.join(f"({m[0]}, {m[1]}, '')" for m in npc['dialogMessages']) + ';',
          f"DELETE FROM npcs_spawns WHERE NpcId = {pnj};",
          f"INSERT INTO npcs_spawns (NpcId, MapId, CellId, Direction) VALUES ({pnj}, {entree}, {case}, 3);",
          f"DELETE FROM npcs_actions WHERE NpcId = {pnj};",
          f"INSERT INTO npcs_actions (NpcId, Type, `Condition`, Priority, Parameter0) VALUES ({pnj}, 'Talk', NULL, 0, '{msg}');",
          f"DELETE FROM npcs_replies WHERE MessageId = {msg};",
          f"INSERT INTO npcs_replies (Type, ReplyId, MessageId) VALUES ('DungeonEnter', {r_clef}, {msg}), ('DungeonEnterTrousseau', {r_tr}, {msg}), ('DungeonEnter', {r_grp}, {msg});",
          f"DELETE FROM dungeons_entries WHERE DungeonId = {did};",
          f"INSERT INTO dungeons_entries (DungeonId, EntranceMapId, KeyItemId, FirstRoomMapId, FirstRoomCellId, KeeperCellId) VALUES ({did}, {entree}, {clef}, {salles[spawns[0]]}, -1, {case});",
          f"UPDATE dungeons SET Name = '{nom.replace(chr(39), chr(39)*2)}' WHERE Id = {did};"]
    for i, sp in enumerate(spawns):
        dernier = i == len(spawns) - 1
        o.append(f"DELETE FROM monsters_spawns_dungeons_groups WHERE DungeonSpawnId = {sp};")
        vals = []
        for joueurs, n in ((1, 4), (5, 5), (6, 6), (7, 7), (8, 8)):
            ids = []
            if dernier:
                ids.append(grades[boss][min(5, max(1, joueurs - 3))])
            while len(ids) < n:
                m = pool[(len(ids) + i + rnd.randrange(len(pool))) % len(pool)]
                ids.append(grades[m][rnd.randint(1, 5)])
            vals += [f"({sp}, {g}, {joueurs}, 'MYSTIC officiel 17/10')" for g in ids]
        o.append("INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES\n " + ',\n '.join(vals) + ';')
    o.append('')
o += ["-- Contrôle : attendu entrees = 3, gardiens = 3, groupes = 450 (3 donjons x 5 salles x 30), monstres_hors_zone = 0",
      "SELECT (SELECT COUNT(*) FROM dungeons_entries WHERE DungeonId IN (111,112,113)) AS entrees,",
      "       (SELECT COUNT(*) FROM npcs_spawns WHERE NpcId IN (4817,4818,4819)) AS gardiens,",
      "       (SELECT COUNT(*) FROM monsters_spawns_dungeons_groups WHERE DungeonSpawnId BETWEEN 451 AND 465) AS groupes,",
      "       (SELECT COUNT(*) FROM monsters_spawns_dungeons_groups g JOIN monsters_grades m ON m.Id = g.MonsterGradeId WHERE g.DungeonSpawnId BETWEEN 451 AND 465",
      "          AND m.MonsterId NOT IN (5653,5664,5666,5652,5689,5690,5691,5692,5677,5678,5679,5680,5681,5668,5669,5670,5671,5672)) AS monstres_hors_zone;"]
sortie = os.path.join(ICI, '30_donjons_chocolat.sql')
open(sortie, 'w', encoding='utf-8', newline='\n').write('\n'.join(o) + '\n')
print('écrit', sortie)
