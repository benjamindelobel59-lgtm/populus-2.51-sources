# PADGREF 23/10 (retour utilisateur : « il n'y a plus que 4 salles au lieu de 5 et les salles ne sont pas dans le bon ordre ») :
# donjon du Comte Harebourg = 4 salles + boss, ordre et compositions de Dofus pour les Noobs ; cartes dans l'ordre des
# identifiants du client 2.51 (Dungeons.d2o) : Hall des Sinistros -> Salle de Banquet -> Galerie des glaçons -> Atelier
# d'horlogerie -> Grande aiguille (Comte). Après la salle 4 (correction utilisateur, SQL 114) : salle de Dorléans (Salle de la Tocante 112202241, retour utilisateur) :
# « Comte seul » -> salle 2, ou un duo de boss (succès 892 du Comte, SQL 108) ; plus d'accès à Dorléans depuis l'entrée du donjon.
# Système modulaire : chaque palier (1, 5, 6, 7, 8 joueurs) = le groupe COMPLET pour ce nombre de joueurs. Grade 5 (comme la
# salle du boss d'origine) ; Comte grade 1 à 5 selon le palier. Sortie : SQL 112 (+ annuler, lignes actuelles de la base).
import os, subprocess
ICI = os.path.dirname(os.path.abspath(__file__))
R = os.path.abspath(os.path.join(ICI, '..', '..', '..', '..'))
DUMP = os.path.join(R, '_OUTILS', 'logiciels', 'MariaDB', 'bin', 'mysqldump.exe')
GRANDUK, STRIGIDE, CYCLOIDE, SINISTROFU, NOCTURLABE = 8465, 8470, 8475, 8487, 8492
COMTE = {1: 8493, 5: 8494, 6: 8495, 7: 8496, 8: 8497}
G, S, C, F, N = GRANDUK, STRIGIDE, CYCLOIDE, SINISTROFU, NOCTURLABE
# (spawn, carte, carte suivante, nom, base 1-4 joueurs, ajouts à 5 / 6 / 7 / 8)
SALLES = [
    (97010, 112198145, 112199169, 'Hall des Sinistros', [G, G, S, C], [G, F, C, G]),
    (382, 112199169, 112200193, 'Salle de Banquet', [S, S, N, C], [S, C, S, F]),
    (383, 112200193, 112201217, 'Galerie des glaçons', [F, F, N, C], [N, F, C, G]),
    (380, 112201217, 112202241, "Atelier d'horlogerie", [N, N, C, G], [S, N, C, N]),
    (384, 112203523, 112204547, 'Grande aiguille (Comte)', [G, C, N], [S, F, N, G]),
]
SORTIE_CASE = 271
CASE_DORLEANS, CASE_SALLE2 = 466, 314             # arrivée chez Dorléans (SQL 108) / case de placement de la salle 2
ids = [s[0] for s in SALLES]
sql = ["-- PADGREF 23/10/2026 : SQL 112 — donjon du Comte Harebourg : 4 salles + boss dans l'ordre et avec les compositions de Dofus",
       "-- pour les Noobs (système modulaire 4 / 5 / 6 / 7 / 8 monstres). Généré par donjons/comte_harebourg/salles_dpln.py. Base world. Rejouable.",
       "SET NAMES utf8mb4;", "START TRANSACTION;",
       "-- entrée (garde Dorléans) : 1re salle = Hall des Sinistros",
       "UPDATE dungeons_entries SET FirstRoomMapId = 112198145 WHERE Id = 48;",
       f"DELETE FROM monsters_spawns_dungeons_groups WHERE DungeonSpawnId IN ({', '.join(map(str, ids))});",
       "DELETE FROM monsters_spawns_dungeons WHERE Id = 97010;"]
for n, (sid, carte, suivante, nom, base, ajouts) in enumerate(SALLES, 1):
    cell = SORTIE_CASE if sid == 384 else (CASE_DORLEANS if sid == 380 else 'NULL')
    desc = f"Donjon du Comte Harebourg - Salle {n} {nom}".replace("'", "''")
    if sid == 97010:
        sql.append(f"INSERT INTO monsters_spawns_dungeons (Id, MapId, CellId, TeleportEvent, TeleportMapId, TeleportCell, TeleportDirection, Descripcion) "
                   f"VALUES ({sid}, {carte}, NULL, 1, {suivante}, {cell}, 7, '{desc}');")
    else:
        sql.append(f"UPDATE monsters_spawns_dungeons SET MapId = {carte}, TeleportMapId = {suivante}, TeleportCell = {cell}, Descripcion = '{desc}' "
                   f"WHERE Id = {sid};")
    lignes = []
    for k, nb in ((1, 0), (5, 1), (6, 2), (7, 3), (8, 4)):
        groupe = base + ajouts[:nb] + ([COMTE[k]] if sid == 384 else [])
        lignes += [f"({sid}, {g}, {k}, 'PADGREF DPLN salle {n}')" for g in groupe]
    sql.append("INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES\n" +
               ",\n".join(lignes) + ";")
sql += ["-- Dorléans (salle de la Tocante) : après la salle 4 ; « Comte seul » -> Grande aiguille ; duos : succès 892 du Comte",
        "DELETE FROM npcs_replies WHERE Id = 907001;",
        f"UPDATE npcs_replies SET Parameter0 = '112203523', Parameter1 = '468', Parameter2 = '7' WHERE Id = 907002;",
        "UPDATE npcs_replies SET Criteria = 'OA=892' WHERE Id IN (907003, 907004, 907005, 907006);"]
# PADGREF 23/10 (retour utilisateur : « les doubles boss n'ont pas leurs autres monstres, ils sont juste 2 ») : arènes des duos
# (SQL 108, spawns 97001-97004) : Comte + partenaire + leurs accompagnateurs (salle du Comte / salle du boss partenaire), système
# modulaire : 4 monstres à 1-4 joueurs (2 boss + 1 accompagnateur chacun), puis +1 par palier en alternant partenaire / Comte.
# Boss grade 1 à 5 selon le palier, accompagnateurs grade 5.
DUOS = {  # spawn : (nom, grades du partenaire 1..5, accompagnateurs de sa salle de boss)
    97001: ('Nileza', [8388, 8389, 8390, 8391, 8392], [8377, 8387, 8367]),
    97002: ('Sylargh', [8456, 8457, 8458, 8459, 8460], [8440, 8445, 8450]),
    97003: ('Klime', [8328, 8329, 8330, 8331, 8332], [8327, 8322, 8312]),
    97004: ('Missiz Frizz', [8358, 8359, 8360, 8361, 8362], [8347, 8342, 8352]),
}
COMTE_ACC = [GRANDUK, CYCLOIDE, NOCTURLABE]
sql.append(f"DELETE FROM monsters_spawns_dungeons_groups WHERE DungeonSpawnId IN ({', '.join(map(str, DUOS))});")
for sid, (nom, grades, acc) in DUOS.items():
    lignes = []
    for i, k in enumerate((1, 5, 6, 7, 8)):
        ajouts = [acc[1], COMTE_ACC[1], acc[2], COMTE_ACC[2]][:i]
        groupe = [COMTE[k], grades[i], COMTE_ACC[0], acc[0]] + ajouts
        lignes += [f"({sid}, {g}, {k}, 'PADGREF duo Comte + {nom}')" for g in groupe]
    sql.append("INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES\n" +
               ",\n".join(lignes) + ";")
# PADGREF 23/10 (retour utilisateur : « il y a aussi une salle avec les 5 réunis, combat pas modulaire, ils sont 5 ») : arène des
# 5 boss (Comte, Nileza, Sylargh, Klime, Missiz Frizz, grade 5) sur le balcon de réserve 112204545, un seul palier (toujours 5
# monstres). Accès : Dorléans, réponse du jeu 2.51 « Monter en haut de la tour. » (17826, libérée de l'entrée), succès 892 du Comte.
CINQ, CARTE_CINQ = 97005, 112204545
sql += [f"DELETE FROM monsters_spawns_dungeons_groups WHERE DungeonSpawnId = {CINQ};",
        f"DELETE FROM monsters_spawns_dungeons WHERE Id = {CINQ};",
        f"INSERT INTO monsters_spawns_dungeons (Id, MapId, CellId, TeleportEvent, TeleportMapId, TeleportCell, TeleportDirection, Descripcion) "
        f"VALUES ({CINQ}, {CARTE_CINQ}, NULL, 1, 112204547, {SORTIE_CASE}, 7, 'Donjon du Comte Harebourg - les 5 boss reunis - Sortie');",
        "INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES\n" + ",\n".join(
            f"({CINQ}, {g}, 1, 'PADGREF les 5 boss reunis')" for g in (8497, 8392, 8460, 8332, 8362)) + ";",
        "DELETE FROM npcs_replies WHERE Id = 907008;",
        f"INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId, Criteria, Parameter0, Parameter1, Parameter2) VALUES "
        f"(907008, 'Teleport', 17826, 15169, 'OA=892', '{CARTE_CINQ}', '174', '1');"]
sql.append("COMMIT;")
open(os.path.join(ICI, '112_comte_salles_dpln.sql'), 'w', encoding='utf-8', newline='\n').write('\n'.join(sql) + '\n')


def dump(table, where):
    out = subprocess.run([DUMP, '-uroot', '-h127.0.0.1', '--no-create-info', '--skip-extended-insert', '--compact', '--skip-triggers',
                          '--replace', 'world', table, f'--where={where}'], capture_output=True, check=True).stdout.decode('utf-8')
    return [l for l in out.splitlines() if l.startswith('REPLACE')]          # sans la ligne « sandbox » de MariaDB


ann = ["-- PADGREF 23/10 : annule 112_comte_salles_dpln.sql (lignes de la base avant le 112)", "START TRANSACTION;",
       "DELETE FROM monsters_spawns_dungeons WHERE Id = 97010;",
       f"DELETE FROM monsters_spawns_dungeons_groups WHERE DungeonSpawnId IN ({', '.join(map(str, ids))});"]
ann += ['DELETE FROM npcs_replies WHERE Id = 907008;', 'DELETE FROM monsters_spawns_dungeons_groups WHERE DungeonSpawnId = 97005;', 'DELETE FROM monsters_spawns_dungeons WHERE Id = 97005;']
ann += dump('npcs_replies', 'Id BETWEEN 907001 AND 907007') + dump('dungeons_entries', 'Id = 48') + dump('monsters_spawns_dungeons', 'Id IN (380, 382, 383, 384)')
ann.append('DELETE FROM monsters_spawns_dungeons_groups WHERE DungeonSpawnId IN (97001, 97002, 97003, 97004);')
ann += dump('monsters_spawns_dungeons_groups', 'DungeonSpawnId IN (380, 382, 383, 384, 97001, 97002, 97003, 97004)')
ann.append("COMMIT;")
os.makedirs(os.path.join(ICI, 'annuler'), exist_ok=True)
open(os.path.join(ICI, 'annuler', '112_comte_salles_dpln-annuler.sql'), 'w', encoding='utf-8', newline='\n').write('\n'.join(ann) + '\n')
print('ok :', len(SALLES), 'salles')
