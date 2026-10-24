# PADGREF 24/10 SQL 161 (demande utilisateur : « rajouter 3 salles au donjon Minotoror, actuellement on spawn au Minotoror
# directement ») : ATTENTION, pas officiel 2.51 (en 2.51 on traverse le labyrinthe extérieur puis la clé ouvre directement la salle du
# Minotoror). Salles = cartes inutilisées du client dans la sous-zone du donjon (210, style du labyrinthe) :
#   136315650 (salle 1) -> 136315392 (salle 2) -> 136317698 (salle 3) -> 34473474 (Minotoror, inchangée) -> 34473476 (sortie).
# Monstres du labyrinthe (Mominotor 831, Déminoboule 832, Minokid 668, Serpiplume 829, Scaratos 830), même paliers que la salle du
# boss (MinPartyMembers 1 / 5 / 6 / 7 / 8 = 4 / 5 / 6 / 7 / 8 monstres, grade 1 à 5).
# Salles 1 et 2 : pas de cases de placement en base -> calcul automatique du serveur (à régler au panel si besoin).
# Usage : py -I minotoror_salles.py  (écrit 161 + annuler)
import os
ICI = os.path.dirname(os.path.abspath(__file__))
SALLES = [136315650, 136315392, 136317698]
BOSS, DONJON = 34473474, 3
SPAWN0, GROUPE0 = 900180, 61900
POOL = [831, 832, 668, 829, 830]
GRADES = {m: {g: 0 for g in range(1, 6)} for m in POOL}
# Ids des grades (monsters_grades) : 668 -> 2188.., 829 -> 2840.., 830 -> 2845.., 831 -> 2850.., 832 -> 2855..
BASE = {668: 2188, 829: 2840, 830: 2845, 831: 2850, 832: 2855}
PALIERS = [(1, 4, 1), (5, 5, 2), (6, 6, 3), (7, 7, 4), (8, 8, 5)]   # (MinPartyMembers, nombre de monstres, grade)
up = ["-- PADGREF 24/10 SQL 161 : 3 salles avant le Minotoror (pas officiel 2.51, demande utilisateur). Généré par",
      "-- _OUTILS/serveur/donjons/minotoror/minotoror_salles.py.",
      f"DELETE FROM monsters_spawns_dungeons_groups WHERE DungeonSpawnId BETWEEN {SPAWN0} AND {SPAWN0 + 9};",
      f"DELETE FROM monsters_spawns_dungeons WHERE Id BETWEEN {SPAWN0} AND {SPAWN0 + 9};"]
g = GROUPE0
for i, carte in enumerate(SALLES):
    suivante = SALLES[i + 1] if i + 1 < len(SALLES) else BOSS
    up.append(f"INSERT INTO monsters_spawns_dungeons (Id, MapId, CellId, TeleportEvent, TeleportMapId, TeleportCell, TeleportDirection, Descripcion) "
              f"VALUES ({SPAWN0 + i}, {carte}, NULL, 1, {suivante}, -1, 7, 'Minotoror salle {i + 1}');")
    for palier, n, grade in PALIERS:
        for k in range(n):
            m = POOL[(k + i) % len(POOL)]
            up.append(f"INSERT INTO monsters_spawns_dungeons_groups (Id, DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) "
                      f"VALUES ({g}, {SPAWN0 + i}, {BASE[m] + grade - 1}, {palier}, 'PADGREF 24/10 Minotoror salle {i + 1}');")
            g += 1
cartes = SALLES + [BOSS, 34473476]
up += [f"UPDATE dungeons SET FirstRoom = {SALLES[0]}, MapsIdsCSV = '{','.join(map(str, cartes))}' WHERE Id = {DONJON};",
       f"UPDATE dungeons_entries SET FirstRoomMapId = {SALLES[0]}, FirstRoomCellId = -1 WHERE DungeonId = {DONJON};"]
ann = ["-- Annulation du SQL 161 (on entre de nouveau directement dans la salle du Minotoror)",
       f"DELETE FROM monsters_spawns_dungeons_groups WHERE DungeonSpawnId BETWEEN {SPAWN0} AND {SPAWN0 + 9};",
       f"DELETE FROM monsters_spawns_dungeons WHERE Id BETWEEN {SPAWN0} AND {SPAWN0 + 9};",
       f"UPDATE dungeons SET FirstRoom = {BOSS}, MapsIdsCSV = '34473476,34473474' WHERE Id = {DONJON};",
       f"UPDATE dungeons_entries SET FirstRoomMapId = {BOSS}, FirstRoomCellId = -1 WHERE DungeonId = {DONJON};"]
open(os.path.join(ICI, '161_minotoror_3_salles.sql'), 'w', encoding='utf-8').write('\n'.join(up) + '\n')
open(os.path.join(ICI, '161_minotoror_3_salles-annuler.sql'), 'w', encoding='utf-8').write('\n'.join(ann) + '\n')
print(g - GROUPE0, 'monstres')
