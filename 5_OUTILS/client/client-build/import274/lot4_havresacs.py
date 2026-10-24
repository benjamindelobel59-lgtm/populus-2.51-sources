# PADGREF 23/10 (import 2.74, lot 4 validé par l'utilisateur) : HAVRE-SACS.
#  - 8 thèmes du 2.74 (Ankama, Eliocalypse, Pandala, Sucreries, Brâkmar, Bonta, Forêt maléfique, Mercenaires) : carte (convertie
#    comme celles de l'Eliocalypse), position, décor (éléments + images) ; HavenbagThemes.d2o ;
#  - meubles : les 983 du 2.74 (HavenbagFurnitures.d2o + leurs éléments / images) ;
#  - serveur (SQL 106) : havenbags_themes (les 8 + les 11 thèmes 2.51 que la base n'avait pas), havenbags_furnitures_source
#    (tous les meubles du client absents de la base). Cartes du serveur : MapImporter sur le client de test _test_app (-> SQL 106b).
# Bases (versions les plus récentes) : sortie Eliocalypse (maps11_2, MapPositions, elements.ele, gfx11_53), sortie du lot 3 (i18n).
import glob, json, os, shutil, struct, subprocess, sys, zlib
sys.stdout.reconfigure(encoding='utf-8')
ICI = os.path.dirname(os.path.abspath(__file__))
CB = os.path.dirname(ICI)
R = os.path.abspath(os.path.join(CB, '..', '..', '..'))
sys.path.insert(0, CB); sys.path.insert(0, os.path.join(CB, 'eliocalypse'))
import maps_elements as me
import cellules
from d2o_ecrire import D2O
from cartes_decor_lib import lire_d2p, ecrire_d2p, lien
sys.path.insert(0, os.path.join(R, '_OUTILS', 'serveur', 'donjons'))
import d2i
APP = os.path.join(R, 'CLIENT_64bits', 'app')
N = os.path.join(R, '_OUTILS', 'client', 'client_27x')
ELIO = os.path.join(CB, 'eliocalypse', 'client')
SORTIE = os.path.join(ICI, 'client')
SQLDIR = os.path.join(R, '_OUTILS', 'serveur', 'import274')
MYSQL = os.path.join(R, '_OUTILS', 'logiciels', 'MariaDB', 'bin', 'mysql.exe')
TEXTE0 = 9070000
T = d2i.load(os.path.join(N, 'data', 'i18n', 'i18n_fr.d2i'))
q = lambda s: (s or '').replace('\\', '\\\\').replace("'", "''")


def rq(s):
    out = subprocess.run([MYSQL, '-uroot', '-h127.0.0.1', '-N', '-B', '--default-character-set=utf8', 'world', '-e', s],
                         capture_output=True, check=True).stdout.decode('utf-8')
    return [l.split('\t') for l in out.replace('\r', '').split('\n') if l]


def base(*p):
    """version la plus récente : sortie du lot (déjà écrite), sortie Eliocalypse, sinon client."""
    for b in (SORTIE, ELIO, APP):
        f = os.path.join(b, *p)
        if os.path.exists(f):
            return f


# ------------------------------------------------------------------ thèmes et meubles (client)
th_n, th_a = D2O(os.path.join(N, 'data', 'common', 'HavenbagThemes.d2o')), D2O(base('data', 'common', 'HavenbagThemes.d2o'))
fu_n, fu_a = D2O(os.path.join(N, 'data', 'common', 'HavenbagFurnitures.d2o')), D2O(base('data', 'common', 'HavenbagFurnitures.d2o'))
themes = [th_n.objet(k) for k in sorted(th_n.index) if k not in th_a.index]
meubles = [fu_n.objet(k) for k in sorted(fu_n.index) if k not in fu_a.index]
cartes = {int(t['mapId']) for t in themes}
textes = {}
mod_t = th_a.objet(next(iter(th_a.index)))
for n, t in enumerate(themes):
    textes[str(TEXTE0 + n)] = T.get(t['nameId']) or 'Havre-sac'
    o = dict(mod_t); o.update(id=t['id'], nameId=TEXTE0 + n, mapId=t['mapId']); th_a.poser(t['id'], o)
mod_f = fu_a.objet(next(iter(fu_a.index)))
for m in meubles:
    o = dict(mod_f)
    for c in o:
        if c in m and c != '__classe':
            o[c] = m[c]
    fu_a.poser(m['typeId'], o)
os.makedirs(os.path.join(SORTIE, 'data', 'common'), exist_ok=True)
th_a.ecrire(os.path.join(SORTIE, 'data', 'common', 'HavenbagThemes.d2o'))
fu_a.ecrire(os.path.join(SORTIE, 'data', 'common', 'HavenbagFurnitures.d2o'))
json.dump(textes, open(os.path.join(ICI, 'textes_lot4.json'), 'w', encoding='utf-8'), ensure_ascii=False)
i18n = base('data', 'i18n', 'i18n_fr.d2i')
os.makedirs(os.path.join(SORTIE, 'data', 'i18n'), exist_ok=True)
tmp = os.path.join(SORTIE, 'data', 'i18n', 'i18n_fr.d2i.tmp')
subprocess.run([sys.executable, '-I', os.path.join(CB, 'd2i_ajout.py'), i18n, tmp, os.path.join(ICI, 'textes_lot4.json')], check=True)
shutil.move(tmp, os.path.join(SORTIE, 'data', 'i18n', 'i18n_fr.d2i'))

# ------------------------------------------------------------------ positions des cartes (format 2.51)
mp_n, mp_a = D2O(os.path.join(N, 'data', 'common', 'MapPositions.d2o')), D2O(base('data', 'common', 'MapPositions.d2o'))
mod_p = mp_a.objet(162791424)
for m in cartes:
    v = mp_n.objet(m); o = dict(mod_p)
    for c in o:
        if c in v and c not in ('__classe', 'nameId'):
            o[c] = v[c]
    o['id'] = m
    mp_a.poser(m, o)
mp_a.ecrire(os.path.join(SORTIE, 'data', 'common', 'MapPositions.d2o'))

# ------------------------------------------------------------------ cartes (converties 2.74 -> 2.51)


def xor(b):
    b = bytearray(b)
    for k in range(len(b)):
        b[k] ^= me.KEY[k % len(me.KEY)]
    return bytes(b)


def convertir_carte(raw):
    """même conversion que eliocalypse/cartes_decor.py : réinsère useLowPassFilter = 0 et useReverb = 0."""
    assert raw[0] == 77 and raw[1] == 11
    enc = raw[6] != 0
    n = struct.unpack_from('>i', raw, 8)[0]
    corps = raw[12:12 + n]
    if enc:
        corps = xor(corps)
    p = 4 + 1 + 4 + 16 + 4 + 8 + 6 + 4
    corps = corps[:p] + b'\x00\x00' + corps[p:]
    if enc:
        corps = xor(corps)
    return raw[:8] + struct.pack('>i', len(corps)) + corps + raw[12 + n:]


brutes = {}
for f in glob.glob(os.path.join(N, 'content', 'maps', '*.d2p')):
    for nom, blob in me.read_d2p(f):
        if nom.endswith('.dlm') and int(os.path.basename(nom)[:-4]) in cartes:
            brutes[int(os.path.basename(nom)[:-4])] = convertir_carte(zlib.decompress(blob))
assert len(brutes) == len(cartes), (len(brutes), len(cartes))
for raw in brutes.values():
    me.parse_map(raw)
chaine, f = [], 'maps0.d2p'
while f and f not in chaine:
    chaine.append(f); f = lien(base('content', 'maps', f))
head, ent, props, pc = lire_d2p(base('content', 'maps', chaine[-1]))
noms = {f'{m % 10}/{m}.dlm' for m in brutes}
ent = [e for e in ent if e[0] not in noms] + [[f'{m % 10}/{m}.dlm', zlib.compress(raw)] for m, raw in sorted(brutes.items())]
ecrire_d2p(os.path.join(SORTIE, 'content', 'maps', chaine[-1]), head, ent, props, pc)

# ------------------------------------------------------------------ éléments graphiques + images


def gfx_carte(raw):
    """éléments graphiques et fonds d'une carte 2.51 (convertie)."""
    r = me.R(raw); r.b(); version = r.b(); r.I()
    enc = r.b() != 0; r.b(); n = r.i()
    if enc:
        r = me.R(xor(r.d[r.p:r.p + n]))
    r.I(); r.b(); r.i(); r.i(); r.i(); r.i(); r.i(); r.I(); r.i(); r.I(); r.H(); r.h(); r.h(); r.i()
    r.b()
    if r.b() == 1: r.i()
    fonds = []
    for _ in range(2):
        for _ in range(r.B()):
            fonds.append(r.i()); r.skip(2 * 5 + 3 + 1)
    r.i(); r.i()
    out = []
    for _ in range(r.B()):
        r.b()
        for _ in range(r.h()):
            r.h()
            for _ in range(r.h()):
                t = r.b()
                if t == 2:
                    out.append(r.I()); r.skip(6); r.skip(4); r.b(); r.I()
                elif t == 33:
                    r.skip(18)
    return out, fonds


def index_ele(d):
    r = me.R(d); assert r.B() == 69
    r.B(); n = r.I(); idx = {}
    for _ in range(n):
        taille = r.H(); eid = r.i()
        idx[eid] = d[r.p - 6:r.p - 4 + taille]; r.p += taille - 4
    return idx, [r.i() for _ in range(r.i())]


voulus, images = set(), set()
for raw in brutes.values():
    e, fo = gfx_carte(raw); voulus |= set(e); images |= set(fo)
voulus |= {m['elementId'] for m in meubles}
nos_raw = zlib.decompress(open(base('content', 'maps', 'elements.ele'), 'rb').read())
idx_a, jpg_a = index_ele(nos_raw)
idx_n, jpg_n = index_ele(open(os.path.join(N, 'content', 'maps', 'elements.ele'), 'rb').read())
manquants = sorted(e for e in voulus - set(idx_a) if e in idx_n)
for e in manquants:
    rec = idx_n[e]
    if rec[6] in (0, 1, 2, 5):
        images.add(struct.unpack_from('>i', rec, 7)[0])
for e in voulus & set(idx_a):                          # éléments déjà connus : leurs images doivent exister aussi
    rec = idx_a[e]
    if rec[6] in (0, 1, 2, 5):
        images.add(struct.unpack_from('>i', rec, 7)[0])
corps = bytearray(nos_raw[:6]); struct.pack_into('>I', corps, 2, len(idx_a) + len(manquants))
for rec in idx_a.values():
    corps += rec
for e in manquants:
    corps += idx_n[e]
jpg = sorted(set(jpg_a) | {g for g in jpg_n if g in images})
corps += struct.pack('>i', len(jpg)) + b''.join(struct.pack('>i', g) for g in jpg)
assert len(index_ele(bytes(corps))[0]) == len(idx_a) + len(manquants)
open(os.path.join(SORTIE, 'content', 'maps', 'elements.ele'), 'wb').write(zlib.compress(bytes(corps), 9))
# images gfx/world absentes -> nouvelle archive gfx11_54 en bout de chaîne gfx0
chaine, f = [], 'gfx0.d2p'
while f and f not in chaine:
    chaine.append(f); f = lien(base('content', 'gfx', 'world', f))
deja = {n for a in chaine for n, _ in me.read_d2p(base('content', 'gfx', 'world', a))}
ajout = {}
for f in glob.glob(os.path.join(N, 'content', 'gfx', 'world', '*.d2p')):
    for nom, b in me.read_d2p(f):
        x = os.path.basename(nom).split('.')[0]
        if x.isdigit() and int(x) in images and nom not in deja and nom not in ajout:
            ajout[nom] = b
nouvelle = 'gfx11_54.d2p'
assert nouvelle not in chaine
head, ent, props, pc = lire_d2p(base('content', 'gfx', 'world', chaine[-1]))
ecrire_d2p(os.path.join(SORTIE, 'content', 'gfx', 'world', nouvelle), head, sorted(ajout.items()), b'', 0)
k, v = b'link', nouvelle.encode()
ecrire_d2p(os.path.join(SORTIE, 'content', 'gfx', 'world', chaine[-1]), head, ent,
           bytes(props) + struct.pack('>H', len(k)) + k + struct.pack('>H', len(v)) + v, pc + 1)
manque_img = sorted(g for g in images if not any(os.path.basename(n).split('.')[0] == str(g) for n in list(ajout) + list(deja)))

# ------------------------------------------------------------------ serveur
fu_tout = [fu_a.objet(k) for k in set(fu_a.index) | set(fu_a.nouveaux)]
en_base = {int(a) for (a,) in rq("SELECT TypeId FROM havenbags_furnitures_source")}
th_base = {int(a) for (a,) in rq("SELECT Id FROM havenbags_themes")}
th_tout = {k: th_a.objet(k) for k in set(th_a.index) | set(th_a.nouveaux)}
TA = d2i.load(os.path.join(SORTIE, 'data', 'i18n', 'i18n_fr.d2i'))


def case_entree(mid):
    raw = brutes.get(mid)
    if raw is None:
        for f in glob.glob(os.path.join(APP, 'content', 'maps', '*.d2p')):
            for nom, b in me.read_d2p(f):
                if nom.endswith(f'/{mid}.dlm'):
                    raw = zlib.decompress(b)
    cs = cellules.lire(raw)[1]
    ok = [i for i, c in enumerate(cs) if c and (c['f'] & 1) == 0 and not (c['f'] & 4)]

    def dist(i):
        r, c = divmod(i, 14); return abs(r - 22) * 0.5 + abs(c - 7)
    return min(ok, key=dist)


b = lambda x: 1 if x else 0
sql = ["-- PADGREF 23/10/2026 : SQL 106 — import 2.74 (lot 4) : havre-sacs (thèmes + meubles). Généré par client-build/import274/lot4_havresacs.py.",
       "-- Les cartes des 8 nouveaux thèmes : SQL 106b (MapImporter). Thèmes donnés par la commande admin (HavenBagCommands). Base world. Rejouable.",
       "SET NAMES utf8mb4;", "START TRANSACTION;"]
ajout_th = [(k, t) for k, t in sorted(th_tout.items()) if k not in th_base]
if ajout_th:
    sql.append(f"DELETE FROM havenbags_themes WHERE Id IN ({','.join(str(k) for k, _ in ajout_th)});")
    sql.append("INSERT INTO havenbags_themes (Id, MapId, NameId, CellId, Name) VALUES\n" + ",\n".join(
        f"({k}, {int(t['mapId'])}, {t['nameId']}, {case_entree(int(t['mapId']))}, '{q(TA.get(t['nameId']))}')" for k, t in ajout_th) + ";")
ajout_fu = [m for m in fu_tout if m['typeId'] not in en_base]
if ajout_fu:
    sql.append(f"DELETE FROM havenbags_furnitures_source WHERE TypeId IN ({','.join(str(m['typeId']) for m in ajout_fu)});")
    sql.append("INSERT INTO havenbags_furnitures_source (TypeId, ThemeId, ElementId, Color, SkillId, LayerId, BlocksMovement, IsStackable, CellsWidth, "
               "CellsHeight, `Order`) VALUES\n" + ",\n".join(
                   f"({m['typeId']}, {m['themeId']}, {m['elementId']}, {m['color']}, {m['skillId']}, {m['layerId']}, {b(m['blocksMovement'])}, "
                   f"{b(m['isStackable'])}, {m['cellsWidth']}, {m['cellsHeight']}, {m['order']})" for m in ajout_fu) + ";")
sql.append("COMMIT;")
os.makedirs(SQLDIR, exist_ok=True)
open(os.path.join(SQLDIR, '106_import274_havresacs.sql'), 'w', encoding='utf-8', newline='\n').write('\n'.join(sql) + '\n')
open(os.path.join(SQLDIR, '106_import274_havresacs-annuler.sql'), 'w', encoding='utf-8', newline='\n').write('\n'.join(
    ["-- PADGREF 23/10 : annule 106_import274_havresacs.sql", "START TRANSACTION;"] +
    ([f"DELETE FROM havenbags_themes WHERE Id IN ({','.join(str(k) for k, _ in ajout_th)});"] if ajout_th else []) +
    ([f"DELETE FROM havenbags_furnitures_source WHERE TypeId IN ({','.join(str(m['typeId']) for m in ajout_fu)});"] if ajout_fu else []) +
    [f"DELETE FROM world_maps WHERE Id IN ({','.join(map(str, sorted(cartes)))});",
     f"DELETE FROM world_maps_positions WHERE Id IN ({','.join(map(str, sorted(cartes)))});", "COMMIT;"]) + '\n')
json.dump({'cartes': sorted(cartes)}, open(os.path.join(ICI, 'lot4_cartes.json'), 'w'))
print('thèmes nouveaux (client) :', [textes[str(TEXTE0 + n)] for n in range(len(themes))])
print('meubles nouveaux (client) :', len(meubles), '| éléments ajoutés :', len(manquants), '| images ajoutées :', len(ajout),
      sum(len(x) for x in ajout.values()) // 1048576, 'Mo | images introuvables :', len(manque_img))
print('serveur : thèmes ajoutés', [TA.get(t['nameId']) for _, t in ajout_th], '| meubles ajoutés en base :', len(ajout_fu))
