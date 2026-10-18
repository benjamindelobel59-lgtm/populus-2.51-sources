# -*- coding: utf-8 -*-
"""
MYSTIC 18/10 : applique les fiches de sorts « comme le site » (dpln/sorts/*.json, format CONSIGNES_SORTS.md) :
- client : nouvelles versions de Spells.d2o / SpellLevels.d2o + textes (d2i) → _OUTILS/client/client-build/sorts_donjons/
  (les mêmes fichiers servent au client 64 et 32 bits ; textes construits sur marchand_doplons/i18n_fr.<n>.d2i) ;
- serveur : 48_sorts_donjons.sql + 48_sorts_donjons-annuler.sql (spells_templates, spells_levels avec EffectsBin calculé par le
  code du serveur via export_d2o, monsters_spells).
Usage : py -I appliquer_sorts.py   (ne touche ni au client installé ni à la base : il prépare les fichiers)
"""
import copy, glob, json, os, subprocess, sys

ICI = os.path.dirname(os.path.abspath(__file__))
RACINE = os.path.dirname(os.path.dirname(os.path.dirname(os.path.dirname(ICI))))
CB = os.path.join(RACINE, '_OUTILS', 'client', 'client-build')
sys.path.insert(0, CB)
from d2o_ecrire import D2O
from d2i_ajout import ajouter

DATA = os.path.join(RACINE, 'CLIENT_64bits', 'app', 'data', 'common')
SORTIE = os.path.join(CB, 'sorts_donjons')
EXPORT = os.path.join(RACINE, '_OUTILS', 'serveur', 'export_d2o', 'bin', 'ExportD2o.exe')
MYSQL = os.path.join(RACINE, '_OUTILS', 'logiciels', 'MariaDB', 'bin', 'mysql.exe')
DUMP = os.path.join(RACINE, '_OUTILS', 'logiciels', 'MariaDB', 'bin', 'mysqldump.exe')
os.makedirs(SORTIE, exist_ok=True)


def requete(sql):
    out = subprocess.run([MYSQL, '-uroot', '-N', '-B', 'world', '-e', sql], capture_output=True).stdout.decode('utf-8')
    return [l.split('\t') for l in out.replace('\r', '').split('\n') if l]


def dump(table, where):
    out = subprocess.run([DUMP, '-uroot', '--no-create-info', '--skip-extended-insert', '--compact', '--skip-triggers', '--hex-blob',
                          f'--where={where}', 'world', table], capture_output=True).stdout.decode('utf-8')
    return out.strip().replace('INSERT INTO', 'REPLACE INTO')


DOSSIER_FICHES = sys.argv[1] if len(sys.argv) > 1 else os.path.join(ICI, 'sorts')
if len(sys.argv) > 2:
    SORTIE = sys.argv[2]; os.makedirs(SORTIE, exist_ok=True)
SQL_SORTIE = sys.argv[3] if len(sys.argv) > 3 else ICI
fiches = [json.load(open(f, encoding='utf-8')) for f in sorted(glob.glob(os.path.join(DOSSIER_FICHES, '*.json')))]
print(len(fiches), 'fiche(s) :', ', '.join(f['donjon'] for f in fiches))

S = D2O(os.path.join(DATA, 'Spells.d2o'))
L = D2O(os.path.join(DATA, 'SpellLevels.d2o'))
DICE = L.classe('EffectInstanceDice')
textes = {}
uid = [990000]
sql, annuler = [], []
niveaux_touches, sorts_nouveaux, sorts_modifies = [], [], []

CHAMPS_NIVEAU = ['apCost', 'minRange', 'range', 'castInLine', 'castInDiagonal', 'castTestLos', 'criticalHitProbability', 'needFreeCell',
                 'needTakenCell', 'needFreeTrapCell', 'rangeCanBeBoosted', 'maxStack', 'maxCastPerTurn', 'maxCastPerTarget',
                 'minCastInterval', 'initialCooldown', 'globalCooldown', 'hidden', 'statesRequired', 'statesAuthorized', 'statesForbidden']


def effet(e, spell_id):
    uid[0] += 1
    return {'__classe': DICE, 'targetMask': e.get('targetMask', ''), 'diceNum': int(e.get('diceNum', 0)),
            'visibleInBuffUi': e.get('visibleInBuffUi', True), 'visibleInFightLog': e.get('visibleInFightLog', True),
            'targetId': int(e.get('targetId', 0)), 'effectElement': int(e.get('effectElement', -1)), 'effectUid': uid[0],
            'dispellable': int(e.get('dispellable', 1)), 'triggers': e.get('triggers', 'I') or 'I', 'spellId': int(e.get('spellId', spell_id)),
            'duration': int(e.get('duration', 0)), 'random': int(e.get('random', 0)), 'effectId': int(e['effectId']),
            'delay': int(e.get('delay', 0)), 'diceSide': int(e.get('diceSide', 0)), 'visibleInTooltip': e.get('visibleInTooltip', True),
            'rawZone': e.get('rawZone', 'P1'), 'forClientOnly': e.get('forClientOnly', False), 'value': int(e.get('value', 0)),
            'order': int(e.get('order', 0)), 'group': int(e.get('group', 0))}


for f in fiches:
    for s in f['sorts']:
        if s['action'] == 'nouveau':
            assert S.objet(s['id']) is None, f"sort {s['id']} déjà pris"
            modele = S.objet(s['copierDe'])
            assert modele, f"copierDe {s['copierDe']} introuvable"
            niv_modele = L.objet(modele['spellLevels'][0])
            sort = copy.deepcopy(modele)
            sort.update({'id': s['id'], 'nameId': s['nomTexteId'], 'descriptionId': s['descriptionTexteId'],
                         'spellLevels': [n['id'] for n in s['niveaux']]})
            textes[s['nomTexteId']] = s['nom']
            textes[s['descriptionTexteId']] = s['description']
            S.poser(s['id'], sort)
            for i, n in enumerate(s['niveaux']):
                assert L.objet(n['id']) is None, f"niveau {n['id']} déjà pris"
                niv = copy.deepcopy(niv_modele)
                niv.update({k: n[k] for k in CHAMPS_NIVEAU if k in n})
                niv.update({'id': n['id'], 'spellId': s['id'], 'grade': n.get('grade', i + 1), 'spellBreed': 0})
                niv['effects'] = [effet(e, s['id']) for e in n['effects']]
                niv['criticalEffect'] = [effet(e, s['id']) for e in n.get('criticalEffects', n['effects'])]
                L.poser(n['id'], niv)
                niveaux_touches.append(n['id'])
            sorts_nouveaux.append(s['id'])
        elif s['action'] == 'modifier':
            sort = S.objet(s['id'])
            assert sort, f"sort à modifier {s['id']} introuvable"
            if s.get('description'):
                sort = copy.deepcopy(sort)
                sort['descriptionId'] = s['descriptionTexteId']
                textes[s['descriptionTexteId']] = s['description']
                S.poser(s['id'], sort)
            for n in s['niveaux']:
                niv = copy.deepcopy(L.objet(n['id']))
                assert niv and niv['spellId'] == s['id'], f"niveau {n['id']} n'est pas un niveau du sort {s['id']}"
                niv.update(n.get('changements', {}))
                if 'effects' in n:
                    niv['effects'] = [effet(e, s['id']) for e in n['effects']]
                    niv['criticalEffect'] = [effet(e, s['id']) for e in n.get('criticalEffects', n['effects'])]
                L.poser(n['id'], niv)
                niveaux_touches.append(n['id'])
            sorts_modifies.append(s['id'])

S.ecrire(os.path.join(SORTIE, 'Spells.d2o'))
L.ecrire(os.path.join(SORTIE, 'SpellLevels.d2o'))
json.dump({str(k): v for k, v in textes.items()}, open(os.path.join(SORTIE, 'textes.json'), 'w', encoding='utf-8'), ensure_ascii=False, indent=1)
for n in (64, 32):
    base = os.path.join(CB, 'marchand_doplons', f'i18n_fr.{n}.d2i')
    r, a = ajouter(base, os.path.join(SORTIE, f'i18n_fr.{n}.d2i'), textes)
print(f'client : {len(sorts_nouveaux)} sort(s) nouveau(x), {len(sorts_modifies)} modifié(s), {len(niveaux_touches)} niveau(x), {len(textes)} texte(s)')

# ---------------------------------------------------------------- serveur
exp = subprocess.run([EXPORT, 'spelllevels', os.path.join(SORTIE, 'SpellLevels.d2o'), ','.join(map(str, niveaux_touches))],
                     capture_output=True).stdout.decode('utf-8')
bins = {}
for ligne in exp.splitlines():
    o = json.loads(ligne)
    bins[o['id']] = (o['__EffectsBin'], o['__CriticalEffectsBin'])


def csv(v):
    return ','.join(map(str, v or []))


def b(v):
    return 1 if v else 0


annuler.append('-- spells_templates / spells_levels d\'origine')
if sorts_modifies:
    annuler.append(dump('spells_templates', 'Id IN (%s)' % ','.join(map(str, sorts_modifies))))
annuler.append(dump('spells_levels', 'Id IN (%s)' % ','.join(map(str, niveaux_touches))))
annuler.append('DELETE FROM spells_templates WHERE Id IN (%s);' % ','.join(map(str, sorts_nouveaux or [0])))
annuler.append('DELETE FROM spells_levels WHERE SpellId IN (%s);' % ','.join(map(str, sorts_nouveaux or [0])))

for sid in sorts_nouveaux + sorts_modifies:
    s = S.objet(sid)
    sql.append("REPLACE INTO spells_templates (Id,NameId,DescriptionId,TypeId,ScriptParams,ScriptParamsCritical,ScriptId,ScriptIdCritical,"
               "IconId,SpellLevelsIdsCSV,UseParamCache,VerboseCast,DefaultZone) VALUES "
               f"({sid},{s['nameId']},{s['descriptionId']},{s['typeId']},'{s['scriptParams']}','{s['scriptParamsCritical']}',{s['scriptId']},"
               f"{s['scriptIdCritical']},{s['iconId']},'{csv(s['spellLevels'])}',1,{b(s['verbose_cast'])},'{s['default_zone']}');")
for lid in niveaux_touches:
    n = L.objet(lid)
    eb, cb = bins[lid]
    sql.append("REPLACE INTO spells_levels (Id,SpellId,SpellBreed,ApCost,`Range`,CastInLine,CastInDiagonal,CastTestLos,CriticalHitProbability,"
               "StatesRequiredCSV,CriticalFailureProbability,NeedFreeCell,NeedFreeTrapCell,NeedTakenCell,RangeCanBeBoosted,MaxStack,MaxCastPerTurn,"
               "MaxCastPerTarget,MinCastInterval,InitialCooldown,GlobalCooldown,MinPlayerLevel,CriticalFailureEndsTurn,HideEffects,Hidden,MinRange,"
               "StatesForbiddenCSV,EffectsBin,CriticalEffectsBin,AdditionalEffectsZonesCSV,StatesAuthorizedCSV) VALUES "
               f"({lid},{n['spellId']},{n['spellBreed']},{n['apCost']},{n['range']},{b(n['castInLine'])},{b(n['castInDiagonal'])},"
               f"{b(n['castTestLos'])},{n['criticalHitProbability']},'{csv(n['statesRequired'])}',0,{b(n['needFreeCell'])},"
               f"{b(n['needFreeTrapCell'])},{b(n['needTakenCell'])},{b(n['rangeCanBeBoosted'])},{n['maxStack']},{n['maxCastPerTurn']},"
               f"{n['maxCastPerTarget']},{n['minCastInterval']},{n['initialCooldown']},{n['globalCooldown']},{n['minPlayerLevel']},0,"
               f"{b(n['hideEffects'])},{b(n['hidden'])},{n['minRange']},'{csv(n['statesForbidden'])}',"
               f"{'0x' + eb if eb else 'NULL'},{'0x' + cb if cb else 'NULL'},'','{csv(n['statesAuthorized'])}');")

# monstres
for f in fiches:
    for m in f.get('monstres', []):
        grades = [int(r[0]) for r in requete(f"SELECT Id FROM monsters_grades WHERE MonsterId={m['id']} ORDER BY GradeId")]
        if not grades:
            print('ATTENTION monstre absent :', m['id']); continue
        annuler.append(dump('monsters_spells', 'MonsterGradeId IN (%s)' % ','.join(map(str, grades))))
        annuler.append('DELETE FROM monsters_spells WHERE MonsterGradeId IN (%s) AND SpellId IN (%s);' % (
            ','.join(map(str, grades)), ','.join(str(a['sort']) for a in m.get('ajouter', [])) or '0'))
        for r in m.get('retirer', []):
            sql.append(f"DELETE FROM monsters_spells WHERE SpellId={r} AND MonsterGradeId IN ({','.join(map(str, grades))});")
        # niveaux de sort 2.51 faux en base (niveau = sous-sort au lieu du sort) : correction par grade
        for c in m.get('corrigerNiveaux', []):
            for i, g in enumerate(grades):
                niv = c['niveauParGrade'][min(i, len(c['niveauParGrade']) - 1)]
                sql.append(f"UPDATE monsters_spells SET Level={niv} WHERE MonsterGradeId={g} AND SpellId={c['sort']};")
        for a in m.get('ajouter', []):
            par_grade = a.get('niveauParGrade') or [1] * len(grades)
            for i, g in enumerate(grades):
                niv = par_grade[min(i, len(par_grade) - 1)]
                sql.append(f"DELETE FROM monsters_spells WHERE MonsterGradeId={g} AND SpellId={a['sort']};")
                sql.append(f"INSERT INTO monsters_spells (MonsterGradeId,SpellId,Level) VALUES ({g},{a['sort']},{niv});")

entete = ["-- MYSTIC 18/10 : sorts des donjons « comme le site » (fiches dpln/sorts/*.json, généré par appliquer_sorts.py).",
          "-- À appliquer avec le client mis à jour (_OUTILS/client/client-build/sorts_donjons/). Annulation : 48_sorts_donjons-annuler.sql", ""]
open(os.path.join(SQL_SORTIE, '48_sorts_donjons.sql'), 'w', encoding='utf-8', newline='\n').write('\n'.join(entete + sql) + '\n')
open(os.path.join(SQL_SORTIE, '48_sorts_donjons-annuler.sql'), 'w', encoding='utf-8', newline='\n').write(
    '\n'.join(["-- MYSTIC 18/10 : annulation de 48_sorts_donjons.sql (lignes d'origine relues avant)", ""] + [a for a in annuler if a]) + '\n')
print('serveur :', len(sql), 'requêtes')
for f in fiches:
    for c in f.get('cerveaux', []):
        print('CERVEAU A CODER :', c)
