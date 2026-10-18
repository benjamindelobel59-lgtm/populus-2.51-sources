# -*- coding: utf-8 -*-
"""
MYSTIC 18/10 : audit des donjons (dpln/SYNTHESE.md + DECISIONS.md), phase 2 = données fausses en base.
Écrit 45_audit_donjons.sql et 45_audit_donjons-annuler.sql (annulation = lignes d'origine relues dans la base AVANT).
Usage : py -I generer_45_audit.py
"""
import os, subprocess
ICI = os.path.dirname(os.path.abspath(__file__))
RACINE = os.path.dirname(os.path.dirname(os.path.dirname(os.path.dirname(ICI))))
BIN = os.path.join(RACINE, '_OUTILS', 'logiciels', 'MariaDB', 'bin')


def dump(table, where):
    out = subprocess.run([os.path.join(BIN, 'mysqldump.exe'), '-uroot', '--no-create-info', '--skip-extended-insert', '--compact',
                          '--skip-triggers', f'--where={where}', 'world', table], capture_output=True).stdout.decode('utf-8')
    return out.strip()


sql, annuler = [], []

# ---------------------------------------------------------------- niveaux de sorts (monsters_spells)
NIVEAUX = [
    ("Maître Corbac 289 : Carapace d'Ailes 630 au niveau 1 (le niveau 3 n'est que le sous-sort de dégâts)",
     "UPDATE monsters_spells s JOIN monsters_grades g ON g.Id=s.MonsterGradeId SET s.Level=1 WHERE g.MonsterId=289 AND s.SpellId=630;",
     "SpellId=630 AND MonsterGradeId IN (SELECT Id FROM monsters_grades WHERE MonsterId=289)"),
    ("Blops / Biblops : niveau de sort = couleur, pas le grade",
     "UPDATE monsters_spells s JOIN monsters_grades g ON g.Id=s.MonsterGradeId SET s.Level = CASE g.MonsterId WHEN 273 THEN 1 WHEN 278 THEN 1 "
     "WHEN 275 THEN 2 WHEN 279 THEN 2 WHEN 274 THEN 3 WHEN 277 THEN 3 WHEN 276 THEN 4 WHEN 280 THEN 4 END "
     "WHERE g.MonsterId IN (273,274,275,276,277,278,279,280) AND s.SpellId IN (471,473,475,476);",
     "SpellId IN (471,473,475,476) AND MonsterGradeId IN (SELECT Id FROM monsters_grades WHERE MonsterId IN (273,274,275,276,277,278,279,280))"),
    ("Darkli Moon grade 1 (invoqué) : Totems 298-301 au niveau 2 (le niveau 1 n'invoque rien)",
     "UPDATE monsters_spells SET Level=2 WHERE MonsterGradeId=710 AND SpellId IN (298,299,300,301);",
     "MonsterGradeId=710 AND SpellId IN (298,299,300,301)"),
    ("Tortues de Sphincter Cell (grade 1 invoqué) : Kawabunga 1019 au niveau de leur élément",
     "UPDATE monsters_spells SET Level=3 WHERE MonsterGradeId=3242 AND SpellId=1019; "
     "UPDATE monsters_spells SET Level=4 WHERE MonsterGradeId=3247 AND SpellId=1019; "
     "UPDATE monsters_spells SET Level=1 WHERE MonsterGradeId=3252 AND SpellId=1019; "
     "UPDATE monsters_spells SET Level=2 WHERE MonsterGradeId=3257 AND SpellId=1019;",
     "SpellId=1019 AND MonsterGradeId IN (3242,3247,3252,3257)"),
    ("Craqueleurs : Manteau Tellurique 7120 au niveau 1 (niveaux 2-3 = sous-sorts)",
     "UPDATE monsters_spells SET Level=1 WHERE SpellId=7120 AND Id IN (5311,5312,5313,14530,14531,14532,19220,19221,19222,48864,48865,48866);",
     "SpellId=7120 AND Id IN (5311,5312,5313,14530,14531,14532,19220,19221,19222,48864,48865,48866)"),
    ("Grotte Hesque : élément des Crustorails / Palmifleurs selon le monstre (comme les Tynrils)",
     "UPDATE monsters_spells s JOIN monsters_grades g ON g.Id=s.MonsterGradeId SET s.Level=3 WHERE g.MonsterId IN (1060,1064) AND s.SpellId IN (1030,1031,1032); "
     "UPDATE monsters_spells s JOIN monsters_grades g ON g.Id=s.MonsterGradeId SET s.Level=5 WHERE g.MonsterId IN (1061,1065) AND s.SpellId IN (1030,1031,1032); "
     "UPDATE monsters_spells s JOIN monsters_grades g ON g.Id=s.MonsterGradeId SET s.Level=2 WHERE g.MonsterId IN (1063,1067) AND s.SpellId IN (1030,1031,1032); "
     "UPDATE monsters_spells s JOIN monsters_grades g ON g.Id=s.MonsterGradeId SET s.Level=4 WHERE g.MonsterId IN (1062,1066) AND s.SpellId IN (1030,1031,1032);",
     "SpellId IN (1030,1031,1032) AND MonsterGradeId IN (SELECT Id FROM monsters_grades WHERE MonsterId IN (1060,1061,1062,1063,1064,1065,1066,1067))"),
]
for titre, maj, where in NIVEAUX:
    sql += [f'-- {titre}', maj, '']
    annuler += [f'-- {titre}', dump('monsters_spells', where).replace('INSERT INTO', 'REPLACE INTO'), '']

# ---------------------------------------------------------------- compositions et salles (monsters_spawns_dungeons[_groups])
SPAWNS_TOUCHES = list(range(466, 499)) + [446, 499, 500, 501, 502, 239, 271, 40]
annuler.append('-- salles d\'origine (Bibliothèque 466-498, Hypogée 446, Grozilla 499-502, Kitsounes 239, Sphincter 271, Batofu 40)')
annuler.append("DELETE FROM monsters_spawns_dungeons_groups WHERE DungeonSpawnId IN (%s) OR Informations LIKE 'MYSTIC 45%%';" % ','.join(map(str, SPAWNS_TOUCHES)))
annuler.append("DELETE FROM monsters_spawns_dungeons WHERE Descripcion LIKE 'MYSTIC 45%';")
annuler.append(dump('monsters_spawns_dungeons', 'Id IN (%s)' % ','.join(map(str, SPAWNS_TOUCHES))).replace('INSERT INTO', 'REPLACE INTO'))
annuler.append(dump('monsters_spawns_dungeons_groups', 'DungeonSpawnId IN (%s)' % ','.join(map(str, SPAWNS_TOUCHES))).replace('INSERT INTO', 'REPLACE INTO'))


def groupe(spawn, tiers, info):
    """tiers = {palier: [gradeId, ...]}"""
    lignes = []
    for palier, grades in tiers.items():
        for g in grades:
            lignes.append(f"({spawn},{g},{palier},'{info}')")
    return ("INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId,MonsterGradeId,MinPartyMembers,Informations) VALUES\n  "
            + ',\n  '.join(lignes) + ';')


# Bibliothèque du Maître Corbac : donjon classique de 4 salles (décision utilisateur) :
# salle 1 159121408 → salle 2 159122432 → salle 3 159123456 → salle du Capsaaloocke 159123460 → Maître Corbac 159125512
BASE = {4579: 14000, 4580: 14005, 4578: 13995, 4581: 14010}  # Corbac Chef, Corbac Dressé, Buveur de Sang, Renarbo Parleur (grade 1)
CAPSA = 13975  # Capsaaloocke grade 1
ORDRE = [4579, 4580, 4578, 4581]


def salle_biblio(grade, capsa=False):
    tiers = {}
    for palier, n in ((1, 4), (5, 5), (6, 6), (7, 7), (8, 8)):
        mobs = [BASE[ORDRE[i % 4]] + grade - 1 for i in range(n)]
        if capsa:
            mobs[0] = CAPSA + 2  # Capsaaloocke grade 3 (niveau 110 à tous les grades)
        tiers[palier] = mobs
    return tiers


sql += ["-- Bibliothèque du Maître Corbac : donjon classique de 4 salles (vrais monstres ; Capsaaloocke dans la salle avant le boss)",
        "DELETE FROM monsters_spawns_dungeons_groups WHERE DungeonSpawnId BETWEEN 466 AND 498;",
        "DELETE FROM monsters_spawns_dungeons WHERE Id BETWEEN 469 AND 497;",
        "UPDATE monsters_spawns_dungeons SET TeleportMapId=159122432, TeleportCell=268, Descripcion='MYSTIC 45 Bibliotheque salle 1 - salle 2' WHERE Id=466;",
        "UPDATE monsters_spawns_dungeons SET TeleportMapId=159123456, TeleportCell=226, Descripcion='MYSTIC 45 Bibliotheque salle 2 - salle 3' WHERE Id=467;",
        "UPDATE monsters_spawns_dungeons SET TeleportMapId=159123460, TeleportCell=199, Descripcion='MYSTIC 45 Bibliotheque salle 3 - salle Capsaaloocke' WHERE Id=468;",
        "UPDATE monsters_spawns_dungeons SET TeleportMapId=159125512, TeleportCell=296, Descripcion='MYSTIC 45 Bibliotheque salle Capsaaloocke - Maitre Corbac' WHERE Id=498;",
        groupe(466, salle_biblio(2), 'MYSTIC 45 biblio salle 1'),
        groupe(467, salle_biblio(3), 'MYSTIC 45 biblio salle 2'),
        groupe(468, salle_biblio(4), 'MYSTIC 45 biblio salle 3'),
        groupe(498, salle_biblio(5, capsa=True), 'MYSTIC 45 biblio salle Capsaaloocke'),
        '']

# Grozilla et Grasmera : les 4 salles contenaient de mauvais monstres (Peluches, Gludotek…) — version normale
sql += ["-- Grozilla et Grasmera : vrais monstres (Krokilles), boss Grozilla + Grasmera, la salle du boss mène à la sortie",
        "DELETE FROM monsters_spawns_dungeons_groups WHERE DungeonSpawnId IN (499,500,501,502);",
        groupe(502, {1: [7292, 7297, 7302, 7307, 7312]}, 'MYSTIC 45 grozilla salle 1'),
        groupe(501, {1: [7294, 7299, 7304, 7309, 7314]}, 'MYSTIC 45 grozilla salle 2'),
        groupe(500, {1: [7202, 7207], 5: [7203, 7208], 6: [7204, 7209], 7: [7205, 7210], 8: [7206, 7211]}, 'MYSTIC 45 grozilla boss'),
        "UPDATE monsters_spawns_dungeons SET TeleportMapId=76285958, TeleportCell=371 WHERE Id=500;",
        "DELETE FROM monsters_spawns_dungeons WHERE Id=499;",
        '']

# Hypogée de l'Obsidiantre : la salle 1 contenait le groupe du Royalmouth (boss compris)
sql += ["-- Hypogée de l'Obsidiantre : salle 1 = vrais monstres du donjon",
        "DELETE FROM monsters_spawns_dungeons_groups WHERE DungeonSpawnId=446;",
        groupe(446, {1: [6463, 6468, 6473, 6478], 5: [6463, 6468, 6473, 6478, 6483], 6: [6463, 6468, 6473, 6478, 6483, 6463],
                     7: [6463, 6468, 6473, 6478, 6483, 6463, 6473], 8: [6463, 6468, 6473, 6478, 6483, 6463, 6473, 6478]}, 'MYSTIC 45 obsi salle 1'),
        '']

# Kitsounes : salle du boss Tanukouï San (absente : la salle 5 menait directement à la sortie)
sql += ["-- Donjon des Kitsounes : salle du Tanukouï San (carte 18088964) entre la salle 5 et la sortie",
        "INSERT INTO monsters_spawns_dungeons (MapId,CellId,TeleportEvent,TeleportMapId,TeleportCell,TeleportDirection,Descripcion) "
        "VALUES (18088964,NULL,1,18089988,NULL,7,'MYSTIC 45 Kitsun boss-sortie');",
        "SET @b = LAST_INSERT_ID();",
        "UPDATE monsters_spawns_dungeons SET TeleportMapId=18088964 WHERE Id=239;",
        groupe('@b', {1: [1948, 1803, 1798], 5: [1949, 1803, 1798, 1808], 6: [1950, 1803, 1798, 1808, 1768],
                      7: [1951, 1803, 1798, 1808, 1768, 1803], 8: [1952, 1803, 1798, 1808, 1768, 1803, 1798]}, 'MYSTIC 45 kitsou boss'),
        '']

# Compositions (décision utilisateur, comme le site) : Rat Noir + Rat Blanc avec Sphincter Cell ; Tofu Mutant (à partir de 5 joueurs)
sql += ["-- Sphincter Cell : le Rat Noir et le Rat Blanc remplacent 2 rats à chaque palier (même grade que le Cell)",
        "UPDATE monsters_spawns_dungeons_groups SET MonsterGradeId=3215, Informations='MYSTIC 45 rat noir' WHERE Id=27341;",
        "UPDATE monsters_spawns_dungeons_groups SET MonsterGradeId=3221, Informations='MYSTIC 45 rat blanc' WHERE Id=27342;",
        "UPDATE monsters_spawns_dungeons_groups SET MonsterGradeId=3216, Informations='MYSTIC 45 rat noir' WHERE Id=27345;",
        "UPDATE monsters_spawns_dungeons_groups SET MonsterGradeId=3222, Informations='MYSTIC 45 rat blanc' WHERE Id=27346;",
        "UPDATE monsters_spawns_dungeons_groups SET MonsterGradeId=3217, Informations='MYSTIC 45 rat noir' WHERE Id=27350;",
        "UPDATE monsters_spawns_dungeons_groups SET MonsterGradeId=3223, Informations='MYSTIC 45 rat blanc' WHERE Id=27351;",
        "UPDATE monsters_spawns_dungeons_groups SET MonsterGradeId=3218, Informations='MYSTIC 45 rat noir' WHERE Id=27356;",
        "UPDATE monsters_spawns_dungeons_groups SET MonsterGradeId=3224, Informations='MYSTIC 45 rat blanc' WHERE Id=27357;",
        "UPDATE monsters_spawns_dungeons_groups SET MonsterGradeId=3219, Informations='MYSTIC 45 rat noir' WHERE Id=27363;",
        "UPDATE monsters_spawns_dungeons_groups SET MonsterGradeId=3225, Informations='MYSTIC 45 rat blanc' WHERE Id=27364;",
        "-- Batofu : le Tofu Mutant (grade 5) remplace un tofu à partir de 5 joueurs",
        "UPDATE monsters_spawns_dungeons_groups SET MonsterGradeId=15159, Informations='MYSTIC 45 tofu mutant' WHERE Id IN (48342,48347,48353,48360);",
        '']

entete = ["-- MYSTIC 18/10 : audit des donjons (dofuspourlesnoobs), phase 2 : données fausses en base.",
          "-- Généré par _OUTILS/serveur/donjons/dpln/generer_45_audit.py. Annulation : 45_audit_donjons-annuler.sql.",
          "-- À appliquer World arrêté (ou redémarrer le World ensuite : monstres et salles sont chargés au démarrage).", ""]
open(os.path.join(ICI, '45_audit_donjons.sql'), 'w', encoding='utf-8', newline='\n').write('\n'.join(entete + sql) + '\n')
open(os.path.join(ICI, '45_audit_donjons-annuler.sql'), 'w', encoding='utf-8', newline='\n').write(
    '\n'.join(["-- MYSTIC 18/10 : annulation de 45_audit_donjons.sql (lignes d'origine relues avant application).", ""] + annuler) + '\n')
print('ok')
