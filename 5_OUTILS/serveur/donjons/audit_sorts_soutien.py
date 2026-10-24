# PADGREF 21/10 : audit des sorts de SOUTIEN (sur un allié ou sur soi) des monstres qui gardent l'IA de base (pas de BrainIdentifier).
# Même règle que Brain.SortsDeSoutien côté serveur : aucun effet visant un ennemi (« A »), au moins un effet sur allié / soi,
# pas d'invocation, coût en PA > 0. Sortie : audit_sorts_soutien.txt (lecture seule).
import os, re, subprocess, sys, collections
sys.stdout.reconfigure(encoding='utf-8')
ICI = os.path.dirname(os.path.abspath(__file__))
R = os.path.dirname(os.path.dirname(os.path.dirname(ICI)))
sys.path.insert(0, ICI)
import d2o, d2i
B = os.path.join(R, 'CLIENT_64bits', 'app', 'data')
T = d2i.load(os.path.join(B, 'i18n', 'i18n_fr.d2i'))
_, S = d2o.load(os.path.join(B, 'common', 'Spells.d2o'))
_, L = d2o.load(os.path.join(B, 'common', 'SpellLevels.d2o'))
MYSQL = os.path.join(R, '_OUTILS', 'logiciels', 'MariaDB', 'bin', 'mysql.exe')
SRC = os.path.join(R, 'SERVEUR_Emu')
INVOC = {180, 181, 185, 405, 1008, 1011, 1097, 2796}


def rq(s):
    out = subprocess.run([MYSQL, '-uroot', '-h127.0.0.1', '-N', '-B', '--default-character-set=utf8', 'world', '-e', s], capture_output=True).stdout.decode('utf-8')
    return [l.split('\t') for l in out.replace('\r', '').split('\n') if l]


enum = dict(re.findall(r'\b([A-Z0-9_]+)\s*=\s*(\d+)', open(os.path.join(SRC, 'DofusProtocol', 'Enums', 'Custom', 'MonsterIdEnum.cs'), encoding='utf-8-sig').read()))
avec_ia = set()
for dp, _, fs in os.walk(os.path.join(SRC, 'server', 'stump.server.worldserver')):
    for f in fs:
        if f.endswith('.cs'):
            for arg in re.findall(r'\[BrainIdentifier\(([^\]]*)\)\]', open(os.path.join(dp, f), encoding='utf-8-sig', errors='ignore').read()):
                for x in re.findall(r'MonsterIdEnum\.([A-Z0-9_]+)|\b(\d+)\b', arg):
                    v = enum.get(x[0]) if x[0] else x[1]
                    if v: avec_ia.add(int(v))


def jetons(mask):
    return [t.strip() for t in (mask or '').split(',') if t.strip()]


def soutien(sid):
    sp = S.get(sid)
    if not sp or not sp['spellLevels']:
        return None
    l = L[sp['spellLevels'][0]]
    if l['apCost'] <= 0:
        return None
    effets = l['effects']
    if not effets or any(e['effectId'] in INVOC for e in effets):
        return None
    cibles = set()
    for e in effets:
        tj = [t for t in jetons(e.get('targetMask')) if t[0] not in 'EeFf*']
        if not tj or any(t[0] == 'A' for t in tj):
            return None
        cibles |= {'soi' if t[0] in 'cC' else 'allie' for t in tj}
    return 'allie' if 'allie' in cibles else 'soi'


noms = dict((int(a), b) for a, b in rq("SELECT m.Id, l.French FROM monsters_templates m JOIN langs l ON l.Id = m.NameId"))
sorts = collections.defaultdict(set)
for m, s in rq("SELECT DISTINCT g.MonsterId, s.SpellId FROM monsters_grades g JOIN monsters_spells s ON s.MonsterGradeId = g.Id"):
    sorts[int(m)].add(int(s))
lignes = []
for m in sorted(sorts):
    if m in avec_ia:
        continue
    for s in sorted(sorts[m]):
        k = soutien(s)
        if k:
            lignes.append(f"{m:5} {noms.get(m, '?')[:30]:30} | {s:5} {T.get(S[s]['nameId']) or '?':30} | {k}")
open(os.path.join(ICI, 'audit_sorts_soutien.txt'), 'w', encoding='utf-8').write('\n'.join(lignes) + '\n')
print(len(avec_ia), 'monstres avec IA à nous ;', len(lignes), 'sorts de soutien sur',
      len({l[:5] for l in lignes}), 'monstres à IA de base')
