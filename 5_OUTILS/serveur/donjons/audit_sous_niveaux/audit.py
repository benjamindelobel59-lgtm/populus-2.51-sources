# PADGREF 23/10 : audit des monstres qui ont, dans monsters_spells, un SOUS-NIVEAU d'un sort (niveau interne relance par le sort lui-meme)
# au lieu du vrai sort. Sort « a sous-sorts » = un de ses niveaux relance le meme numero de sort. Un niveau L>1 utilise est suspect s'il
# ne ressemble pas au niveau 1 (liste d'effets differente). Usage : py -I audit.py <sortie.tsv>
import sys, os, struct, subprocess, collections
sys.stdout.reconfigure(encoding='utf-8')
R = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..', '..', '..'))
sys.path.insert(0, os.path.join(R, '_OUTILS', 'client', 'client-build'))
from d2o_ecrire import D2O
C = os.path.join(R, 'CLIENT_64bits', 'app', 'data')
S = D2O(os.path.join(C, 'common', 'Spells.d2o')); L = D2O(os.path.join(C, 'common', 'SpellLevels.d2o')); M = D2O(os.path.join(C, 'common', 'Monsters.d2o'))
d = open(os.path.join(C, 'i18n', 'i18n_fr.d2i'), 'rb').read()
io = struct.unpack_from('>i', d, 0)[0]; n = struct.unpack_from('>i', d, io)[0]; p = io + 4; T = {}
while p < io + 4 + n:
    k, dia, ptr = struct.unpack_from('>i?i', d, p); p += 9
    if dia: p += 4
    T[k] = ptr
def t(k):
    q = T.get(k)
    if q is None: return '?'
    l = struct.unpack_from('>H', d, q)[0]; return d[q + 2:q + 2 + l].decode('utf-8')
LANCE = {792, 793, 1160, 1017, 2160, 1175, 2017, 1018, 1019, 2792, 2794, 401, 402, 1165, 2022, 2023, 1091}
MYSQL = os.path.join(R, '_OUTILS', 'logiciels', 'MariaDB', 'bin', 'mysql.exe')
rows = subprocess.run([MYSQL, '-uroot', '-h127.0.0.1', '-N', '-B', 'world', '-e',
    'select g.MonsterId, g.GradeId, s.SpellId, s.Level, s.Id from monsters_spells s join monsters_grades g on g.Id=s.MonsterGradeId where s.Level>1'],
    capture_output=True).stdout.decode().split('\n')
cache = {}
def info(sid):
    if sid in cache: return cache[sid]
    s = S.objet(sid); r = None
    if s:
        lv = [L.objet(x) for x in s['spellLevels']]
        auto = any(e['effectId'] in LANCE and e['diceNum'] == sid for n_ in lv if n_ for e in n_['effects'])
        r = (s, lv, auto)
    cache[sid] = r; return r
res = collections.OrderedDict()
for r in rows:
    if not r.strip(): continue
    mid, gr, sid, lvl, rid = map(int, r.split('\t'))
    i = info(sid)
    if not i or not i[2]: continue
    s, lv, _ = i
    if lvl > len(lv) or lv[0] is None: continue
    e1 = [e['effectId'] for e in lv[0]['effects']]; eL = [e['effectId'] for e in lv[lvl - 1]['effects']]
    if e1 == eL: continue
    res.setdefault((mid, sid), []).append((gr, lvl, rid))
out = open(sys.argv[1], 'w', encoding='utf-8')
out.write('monstre\tnom\tsort\tnom du sort\tgrades:niveau\tids\n')
for (mid, sid), v in sorted(res.items()):
    m = M.objet(mid); s = S.objet(sid)
    out.write(f"{mid}\t{t(m['nameId']) if m else '?'}\t{sid}\t{t(s['nameId'])}\t{' '.join(f'{g}:{l}' for g, l, _ in sorted(v))}\t{','.join(str(x[2]) for x in v)}\n")
print(len(res), 'couples monstre/sort,', sum(len(v) for v in res.values()), 'lignes')
