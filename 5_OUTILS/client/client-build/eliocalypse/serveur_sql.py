# PADGREF 23/10 : Eliocalypse — SQL serveur des DONNÉES de base (SQL 97) :
#   cartes + positions (insérées en local par tools/MapImporter, recopiées ici), zones / sous-zones / super-zone, monstres + grades +
#   sorts de monstre, sorts / niveaux / états / types de sort / races / donjons (outil ImporterD2o = AssignFields du serveur), textes.
# Entrées : client/data/common/*.d2o (donnees.py), donnees.json, textes.json, inventaire.json, client 2.74 (grades : spellGrades,
# tacle). Sorties : _OUTILS/serveur/donjons/eliocalypse/97_eliocalypse_donnees.sql (+ -annuler). Rejouable.
import json, os, re, subprocess, sys
sys.stdout.reconfigure(encoding='utf-8')
ICI = os.path.dirname(os.path.abspath(__file__))
CB = os.path.dirname(ICI)
R = os.path.abspath(os.path.join(CB, '..', '..', '..'))
sys.path.insert(0, os.path.join(R, '_OUTILS', 'serveur', 'donjons'))
import d2o
MYSQL = os.path.join(R, '_OUTILS', 'logiciels', 'MariaDB', 'bin', 'mysql.exe')
DUMP = os.path.join(R, '_OUTILS', 'logiciels', 'MariaDB', 'bin', 'mysqldump.exe')
OUTIL = os.path.join(R, '_OUTILS', 'serveur', 'panoplies', 'bin', 'ImporterD2o.exe')
CLI = os.path.join(ICI, 'client', 'data', 'common')
N = os.path.join(R, '_OUTILS', 'client', 'client_27x', 'data', 'common')
SORTIE = os.path.join(R, '_OUTILS', 'serveur', 'donjons', 'eliocalypse')
GRADE0, MSPELL0 = 970000, 970000             # identifiants fixes (rejouable) pour monsters_grades / monsters_spells


def rq(s):
    out = subprocess.run([MYSQL, '-uroot', '-h127.0.0.1', '-N', '-B', '--default-character-set=utf8', 'world', '-e', s],
                         capture_output=True, check=True).stdout.decode('utf-8')
    return [l.split('\t') for l in out.replace('\r', '').split('\n') if l]


q = lambda s: "'" + str(s).replace('\\', '\\\\').replace("'", "''") + "'"
d = json.load(open(os.path.join(ICI, 'donnees.json'), encoding='utf-8'))
textes = {int(k): v for k, v in json.load(open(os.path.join(ICI, 'textes.json'), encoding='utf-8')).items()}
inv = json.load(open(os.path.join(R, '_OUTILS', 'serveur', 'donjons', 'eliocalypse', 'inventaire.json'), encoding='utf-8'))
L = lambda f: d2o.load(os.path.join(CLI, f + '.d2o'))[1]
MN, SA, AR, SPS = L('Monsters'), L('SubAreas'), L('Areas'), L('Spells')
MN74 = d2o.load(os.path.join(N, 'Monsters.d2o'))[1]
colonnes = lambda t: ','.join(c[0] for c in rq(f"SHOW COLUMNS FROM {t}"))
sql, annuler = [], []


def outil(fichier, typ, ids, table):
    if not ids:
        return []
    out = subprocess.run([OUTIL, os.path.join(CLI, fichier + '.d2o'), typ, ','.join(map(str, sorted(ids))), colonnes(table)],
                         capture_output=True, cwd=os.path.dirname(OUTIL))
    if out.returncode != 0:
        raise SystemExit(out.stderr.decode('utf-8', 'replace'))
    lignes = [l for l in out.stdout.decode('utf-8').splitlines() if l.startswith('INSERT')]
    print('  ', out.stderr.decode('utf-8', 'replace').strip())
    return lignes


ids_l = lambda xs: ','.join(map(str, sorted(xs)))
monstres, sorts, niveaux, etats = d['monstres'], d['sorts'], d['niveaux'], d['etats']
etats_nouveaux = sorted(e for e in etats if not rq(f"SELECT 1 FROM spells_states WHERE Id = {e}") or e >= 1500)
sorts_nouveaux = [s for s in sorts if s != 999]
niveaux_nouveaux = sorted(int(l) for l in niveaux if not rq(f"SELECT 1 FROM spells_levels WHERE Id = {l} AND SpellId = 999"))
races = sorted({MN[m]['race'] for m in monstres})
races_nouvelles = [r for r in races if not rq(f"SELECT 1 FROM monsters_races WHERE Id = {r}")]
DONJONS = [117, 118, 119, 120]
cartes = inv['cartes']

# ------------------------------------------------------------------ textes
sql.append(f"DELETE FROM langs WHERE Id BETWEEN 9020000 AND 9029999;")
sql.append("INSERT INTO langs (Id, French) VALUES " + ",\n ".join(f"({i}, {q(t)})" for i, t in sorted(textes.items())) + ";")
annuler.append("DELETE FROM langs WHERE Id BETWEEN 9020000 AND 9029999;")
nom = lambda i: textes.get(i, '')

# ------------------------------------------------------------------ cartes + positions (déjà insérées en local par MapImporter)
for table in ('world_maps', 'world_maps_positions'):
    out = subprocess.run([DUMP, '-uroot', '-h127.0.0.1', '--default-character-set=utf8', '--no-create-info', '--skip-extended-insert',
                          '--compact', '--hex-blob', 'world', table, f'--where=Id IN ({ids_l(cartes)})'], capture_output=True, check=True)
    lignes = [l for l in out.stdout.decode('utf-8').splitlines() if l.startswith('INSERT')]
    assert len(lignes) == len(cartes), (table, len(lignes))
    sql.append(f"DELETE FROM {table} WHERE Id IN ({ids_l(cartes)});")
    sql += lignes
    annuler.append(f"DELETE FROM {table} WHERE Id IN ({ids_l(cartes)});")

# ------------------------------------------------------------------ zones
for a, o in AR.items():
    if a in d['zones']:
        sql.append(f"DELETE FROM world_areas WHERE Id = {a};")
        sql.append(f"INSERT INTO world_areas (Id, NameId, SuperAreaId, ContainHouses, ContainPaddocks) VALUES ({a}, {o['nameId']}, {o['superAreaId']}, "
                   f"{int(o['containHouses'])}, {int(o['containPaddocks'])});")
        annuler.append(f"DELETE FROM world_areas WHERE Id = {a};")
        if not rq(f"SELECT 1 FROM world_superareas WHERE Id = {o['superAreaId']}"):
            SPA = d2o.load(os.path.join(CLI, 'SuperAreas.d2o'))[1]
            s = SPA.get(o['superAreaId'])
            if s:
                sql.append(f"INSERT IGNORE INTO world_superareas (Id, NameId, WorldmapId) VALUES ({o['superAreaId']}, {s['nameId']}, {s.get('worldmapId', 1)});")
                annuler.append(f"DELETE FROM world_superareas WHERE Id = {o['superAreaId']};")
for k in d['sousZones']:
    o = SA[k]
    forme = ','.join(map(str, o.get('shape', [])))
    sql.append(f"DELETE FROM world_subareas WHERE Id = {k};")
    sql.append(f"INSERT INTO world_subareas (Id, NameId, AreaId, MapsIdsCSV, ShapeCSV, CustomWorldMapCSV, PackId, Level, Difficulty, SpawnsLimit, "
               f"CustomSpawnInterval, AchievementId) VALUES ({k}, {o['nameId']}, {o['areaId']}, {q(','.join(str(int(x)) for x in o['mapIds']))}, "
               f"{q(forme)}, '', {o.get('packId', 0)}, {o.get('level', 200)}, 2, 3, NULL, 0);")
    annuler.append(f"DELETE FROM world_subareas WHERE Id = {k};")

# ------------------------------------------------------------------ sorts, niveaux, états, types, races, donjons (AssignFields)
print('outil ImporterD2o :')
sql.append(f"DELETE FROM spells_templates WHERE Id IN ({ids_l(sorts_nouveaux)});")
sql += outil('Spells', 'SpellTemplate', sorts_nouveaux, 'spells_templates')
sql.append(f"DELETE FROM spells_levels WHERE Id IN ({ids_l(niveaux_nouveaux)});")
sql += outil('SpellLevels', 'SpellLevelTemplate', niveaux_nouveaux, 'spells_levels')
etats_import = [e for e in etats if e >= 1500 or not rq(f"SELECT 1 FROM spells_states WHERE Id = {e}")]
if etats_import:
    sql.append(f"DELETE FROM spells_states WHERE Id IN ({ids_l(etats_import)});")
    sql += outil('SpellStates', 'SpellState', etats_import, 'spells_states')
    sql.append("UPDATE spells_states s JOIN langs l ON l.Id = s.NameId SET s.Name = l.French WHERE s.Id IN (" + ids_l(etats_import) + ");")
types = sorted({SPS[s]['typeId'] for s in sorts_nouveaux if s in SPS})
types_nouveaux = [t for t in types if not rq(f"SELECT 1 FROM spells_types WHERE Id = {t}")]
for t in types_nouveaux:
    sql.append(f"INSERT IGNORE INTO spells_types (Id, LongName, ShortName, LongNameId, ShortNameId) SELECT {t}, LongName, ShortName, LongNameId, "
               f"ShortNameId FROM spells_types WHERE Id = 0;")
if races_nouvelles:
    sql.append(f"DELETE FROM monsters_races WHERE Id IN ({ids_l(races_nouvelles)});")
    sql += outil('MonsterRaces', 'MonsterRace', races_nouvelles, 'monsters_races')
    sql.append("UPDATE monsters_races r JOIN langs l ON l.Id = r.NameId SET r.Name = l.French WHERE r.Id IN (" + ids_l(races_nouvelles) + ");")
    sr = sorted({L('MonsterRaces')[r]['superRaceId'] for r in races_nouvelles} - {int(x[0]) for x in rq("SELECT Id FROM monsters_superraces")})
    if sr:
        sql += outil('MonsterSuperRaces', 'MonsterSuperRace', sr, 'monsters_superraces')
        annuler.append(f"DELETE FROM monsters_superraces WHERE Id IN ({ids_l(sr)});")
sql.append(f"DELETE FROM dungeons WHERE Id IN ({ids_l(DONJONS)});")
DG = L('Dungeons')                            # (DungeonRecord est commenté dans notre serveur : écrit ici)
for k in DONJONS:
    o = DG[k]
    maps = [int(x) for x in o.get('mapIds', [])]
    sql.append(f"INSERT INTO dungeons (Id, Name, NameId, MapsIdsCSV, OptimalPlayerLevel, FirstRoom, EntranceMapId, ExitMapId) VALUES ({k}, '', "
               f"{o['nameId']}, {q(','.join(map(str, maps)))}, {o.get('optimalPlayerLevel', 200)}, {int(o.get('entranceMapId') or 0)}, "
               f"{int(o.get('entranceMapId') or 0)}, {int(o.get('exitMapId') or 0)});")
sql.append("UPDATE dungeons x JOIN langs l ON l.Id = x.NameId SET x.Name = l.French WHERE x.Id IN (" + ids_l(DONJONS) + ");")
annuler += [f"DELETE FROM spells_templates WHERE Id IN ({ids_l(sorts_nouveaux)});",
            f"DELETE FROM spells_levels WHERE Id IN ({ids_l(niveaux_nouveaux)});",
            f"DELETE FROM spells_states WHERE Id IN ({ids_l(etats_import)});" if etats_import else "",
            f"DELETE FROM spells_types WHERE Id IN ({ids_l(types_nouveaux)});" if types_nouveaux else "",
            f"DELETE FROM monsters_races WHERE Id IN ({ids_l(races_nouvelles)});" if races_nouvelles else "",
            f"DELETE FROM dungeons WHERE Id IN ({ids_l(DONJONS)});"]

# ------------------------------------------------------------------ monstres, grades, sorts de monstre
RN = {r: nom(o['nameId']) for r, o in L('MonsterRaces').items()}
sql.append(f"DELETE FROM monsters_spells WHERE Id BETWEEN {MSPELL0} AND {MSPELL0 + 99999};")
sql.append(f"DELETE FROM monsters_grades WHERE Id BETWEEN {GRADE0} AND {GRADE0 + 9999};")
sql.append(f"DELETE FROM monsters_templates WHERE Id IN ({ids_l(monstres)});")
gid, sid = GRADE0, MSPELL0
for m in monstres:
    o, o74 = MN[m], MN74[m]
    boss = o.get('isBoss', False)
    sql.append("INSERT INTO monsters_templates (Id, Name, Race, NameId, GfxId, RaceId, MinDroppedKamas, MaxDroppedKamas, LookAsString, UseSummonSlot, "
               "UseBombSlot, CanPlay, CanTackle, CanSwitchPos, CanBePushed, IsBoss, IsActive, AllIdolsDisabled, IncompatibleChallengesCSV, canBeCarried) "
               f"VALUES ({m}, {q(nom(o['nameId']))}, {q(RN.get(o['race'], ''))}, {o['nameId']}, {o.get('gfxId', 0)}, {o['race']}, "
               f"{1000 if boss else 100}, {5000 if boss else 1000}, {q(o['look'])}, {int(o['useSummonSlot'])}, {int(o['useBombSlot'])}, "
               f"{int(o['canPlay'])}, {int(o['canTackle'])}, {int(o['canSwitchPos'])}, {int(o['canBePushed'])}, {int(boss)}, 1, 0, "
               f"{q(','.join(map(str, o.get('incompatibleChallenges', []))))}, {int(o74.get('canBeCarried', True))});")
    niv_sorts = [int(x.split(',')[0]) if x else 1 for x in (o74.get('spellGrades') or [])]
    for gi, g in enumerate(o['grades']):
        b = (o74['grades'][gi].get('bonusCharacteristics') or {}) if gi < len(o74['grades']) else {}
        stats = {'SummonLimit': 3 if boss else 1, 'Initiative': 0}
        sql.append("INSERT INTO monsters_grades (Id, GradeId, GradeXp, MonsterId, Level, HiddenLevel, PaDodge, PmDodge, EarthResistance, AirResistance, "
                   "FireResistance, WaterResistance, NeutralResistance, LifePoints, ActionPoints, MovementPoints, TackleEvade, TackleBlock, Strength, "
                   f"Chance, Vitality, Wisdom, Intelligence, Agility, StatsJSON) VALUES ({gid}, {g['grade']}, {g['gradeXp']}, {m}, {g['level']}, "
                   f"{g.get('hiddenLevel', 0)}, {g['paDodge']}, {g['pmDodge']}, {g['earthResistance']}, {g['airResistance']}, {g['fireResistance']}, "
                   f"{g['waterResistance']}, {g['neutralResistance']}, {g['lifePoints']}, {g['actionPoints']}, {g['movementPoints']}, "
                   f"{b.get('tackleEvade', 0)}, {b.get('tackleBlock', 0)}, {g['strength']}, {g['chance']}, {g['vitality']}, {g['wisdom']}, "
                   f"{g['intelligence']}, {g['agility']}, {q(json.dumps(stats))});")
        for si, s in enumerate(o.get('spells', [])):
            sql.append(f"INSERT INTO monsters_spells (Id, MonsterGradeId, SpellId, Level) VALUES ({sid}, {gid}, {s}, "
                       f"{niv_sorts[si] if si < len(niv_sorts) else 1});")
            sid += 1
        gid += 1
annuler += [f"DELETE FROM monsters_spells WHERE Id BETWEEN {MSPELL0} AND {MSPELL0 + 99999};",
            f"DELETE FROM monsters_grades WHERE Id BETWEEN {GRADE0} AND {GRADE0 + 9999};",
            f"DELETE FROM monsters_templates WHERE Id IN ({ids_l(monstres)});"]

entete = ["-- PADGREF 23/10/2026 : SQL 97 — Eliocalypse, données de base importées du client 2.74 (générées par",
          "-- _OUTILS/client/client-build/eliocalypse/serveur_sql.py) : 128 cartes + positions, sous-zones 925-934, zones 75 / 84,",
          f"-- {len(monstres)} monstres ({len(range(GRADE0, gid))} grades), {len(sorts_nouveaux)} sorts, {len(niveaux_nouveaux)} niveaux, {len(etats_import)} états,",
          "-- donjons 117-120 (Arbre de Mort, Fers de la Tyrannie, Sentence de la Balance, Trône de Sang), textes 9020000+.",
          "-- PAS encore : apparitions, passages, accès, butin, mécaniques (SQL suivants). Base world. Rejouable.",
          "SET NAMES utf8mb4;", "START TRANSACTION;"]
open(os.path.join(SORTIE, '97_eliocalypse_donnees.sql'), 'w', encoding='utf-8', newline='\n').write('\n'.join(entete + sql + ['COMMIT;']) + '\n')
open(os.path.join(SORTIE, '97_eliocalypse_donnees-annuler.sql'), 'w', encoding='utf-8', newline='\n').write(
    '\n'.join(['-- PADGREF 23/10 : annule 97_eliocalypse_donnees.sql', 'START TRANSACTION;'] + [a for a in annuler if a] + ['COMMIT;']) + '\n')
print('ok :', len(sql), 'lignes ;', gid - GRADE0, 'grades ;', sid - MSPELL0, 'sorts de monstre ; types de sort ajoutés', types_nouveaux,
      '; races', races_nouvelles)
