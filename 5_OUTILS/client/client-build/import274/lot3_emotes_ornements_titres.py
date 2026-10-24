# PADGREF 23/10 (import 2.74, lot 3 validé par l'utilisateur) : ÉMOTES, ORNEMENTS et TITRES du client 2.74 absents du nôtre.
# Client : Emoticons / Ornaments / Titles.d2o (+ textes 9060000+ dans l'i18n cumulé, version Eliocalypse la plus récente),
#   images des ornements (gfx/ornaments), index des animations des personnages (sprites/bones/index.xml, 1.xml, 2.xml :
#   FUSION, nos entrées sont gardées, seuls les fichiers nouveaux du 2.74 sont ajoutés ; les .swl sont dans le lot 1).
# Serveur : SQL 105 (emotes, tinsel_ornaments, tinsel_titles). Les joueurs les obtiennent comme les autres (récompenses, boutique,
# commandes admin) : rien n'est donné automatiquement.
import json, os, re, shutil, subprocess, sys
sys.stdout.reconfigure(encoding='utf-8')
ICI = os.path.dirname(os.path.abspath(__file__))
CB = os.path.dirname(ICI)
R = os.path.abspath(os.path.join(CB, '..', '..', '..'))
sys.path.insert(0, CB)
from d2o_ecrire import D2O
sys.path.insert(0, os.path.join(R, '_OUTILS', 'serveur', 'donjons'))
import d2i
APP = os.path.join(R, 'CLIENT_64bits', 'app')
N = os.path.join(R, '_OUTILS', 'client', 'client_27x')
ELIO_I18N = os.path.join(CB, 'eliocalypse', 'client', 'data', 'i18n', 'i18n_fr.d2i')
SORTIE = os.path.join(ICI, 'client')
SQLDIR = os.path.join(R, '_OUTILS', 'serveur', 'import274')
TEXTE0 = 9060000
T = d2i.load(os.path.join(N, 'data', 'i18n', 'i18n_fr.d2i'))
q = lambda s: (s or '').replace('\\', '\\\\').replace("'", "''")
textes, prochain = {}, [TEXTE0]


def texte(i):
    if not i or i <= 0:
        return 0
    n = prochain[0]; prochain[0] += 1
    textes[str(n)] = T.get(i) or ''
    return n


os.makedirs(os.path.join(SORTIE, 'data', 'common'), exist_ok=True)
nouveaux = {}
for f in ('Emoticons', 'Ornaments', 'Titles'):
    nos, leur = D2O(os.path.join(APP, 'data', 'common', f + '.d2o')), D2O(os.path.join(N, 'data', 'common', f + '.d2o'))
    modele = nos.objet(next(iter(nos.index)))
    ajout = []
    for k in sorted(leur.index):
        if k in nos.index:
            continue
        v = leur.objet(k); o = dict(modele)
        for c in o:
            if c in v and c != '__classe':
                o[c] = v[c]
        for c in ('nameId', 'shortcutId', 'nameMaleId', 'nameFemaleId'):
            if c in o:
                o[c] = texte(v.get(c))
        if 'rarity' in o and 'rarity' not in v:
            o['rarity'] = 0
        nos.poser(k, o); ajout.append(o)
    nos.ecrire(os.path.join(SORTIE, 'data', 'common', f + '.d2o'))
    nouveaux[f] = ajout
json.dump(textes, open(os.path.join(ICI, 'textes_lot3.json'), 'w', encoding='utf-8'), ensure_ascii=False)
os.makedirs(os.path.join(SORTIE, 'data', 'i18n'), exist_ok=True)
subprocess.run([sys.executable, '-I', os.path.join(CB, 'd2i_ajout.py'), ELIO_I18N, os.path.join(SORTIE, 'data', 'i18n', 'i18n_fr.d2i'),
                os.path.join(ICI, 'textes_lot3.json')], check=True)

# images des ornements absentes
dst = os.path.join(SORTIE, 'content', 'gfx', 'ornaments'); os.makedirs(dst, exist_ok=True)
deja = set(os.listdir(os.path.join(APP, 'content', 'gfx', 'ornaments')))
img = [f for f in os.listdir(os.path.join(N, 'content', 'gfx', 'ornaments')) if f not in deja]
for f in img:
    shutil.copy2(os.path.join(N, 'content', 'gfx', 'ornaments', f), dst)

# index des animations des personnages : ajout des <file> nouveaux du 2.74
dst = os.path.join(SORTIE, 'content', 'gfx', 'sprites', 'bones'); os.makedirs(dst, exist_ok=True)
for x in ('index.xml', '1.xml', '2.xml'):
    nos = open(os.path.join(APP, 'content', 'gfx', 'sprites', 'bones', x), encoding='utf-8').read()
    leur = open(os.path.join(N, 'content', 'gfx', 'sprites', 'bones', x), encoding='utf-8').read()
    if x == 'index.xml':
        for g in re.findall(r'<group id="(\d+)">', leur):
            bloc_l = re.search(rf'<group id="{g}">(.*?)</group>', leur, re.S).group(1)
            m = re.search(rf'<group id="{g}">(.*?)</group>', nos, re.S)
            noms = set(re.findall(r'name="([^"]+)"', m.group(1))) if m else set()
            neufs = [l for l in re.findall(r'\s*<file name="[^"]+" />', bloc_l) if re.search(r'name="([^"]+)"', l).group(1) not in noms]
            if m and neufs:
                nos = nos[:m.end(1)] + ''.join(neufs) + '\n  ' + nos[m.end(1):].lstrip(' ')
    else:
        noms = set(re.findall(r'<file name="([^"]+)">', nos))
        neufs = [b for b in re.findall(r'\s*<file name="[^"]+">.*?</file>', leur, re.S) if re.search(r'name="([^"]+)"', b).group(1) not in noms]
        fin = nos.rindex('</index>')
        nos = nos[:fin].rstrip() + ''.join(neufs) + '\n' + nos[fin:]
    open(os.path.join(dst, x), 'w', encoding='utf-8', newline='\n').write(nos)

# serveur
b = lambda v: 1 if v else 0
sql = ["-- PADGREF 23/10/2026 : SQL 105 — import 2.74 (lot 3) : émotes, ornements, titres absents (généré par "
       "client-build/import274/lot3_emotes_ornements_titres.py). Base world. Rejouable.", "SET NAMES utf8mb4;", "START TRANSACTION;"]
E, O, Ti = nouveaux['Emoticons'], nouveaux['Ornaments'], nouveaux['Titles']
if E:
    sql.append(f"DELETE FROM emotes WHERE id IN ({','.join(str(e['id']) for e in E)});")
    sql.append("INSERT INTO emotes (id, Name, nameId, shortcutId, `order`, defaultAnim, persistancy, eight_directions, aura, anims, cooldown, duration, "
               "weight, spellLevelId) VALUES\n" + ",\n".join(
                   f"({e['id']}, '{q(textes.get(str(e['nameId'])))}', {e['nameId']}, {e['shortcutId']}, {e['order']}, '{q(e['defaultAnim'])}', "
                   f"{b(e['persistancy'])}, {b(e['eight_directions'])}, {b(e['aura'])}, '{q(','.join(map(str, e.get('anims') or [])))}', "
                   f"{e['cooldown']}, {e['duration']}, {e['weight']}, {e['spellLevelId']})" for e in E) + ";")
if O:
    sql.append(f"DELETE FROM tinsel_ornaments WHERE Id IN ({','.join(str(o['id']) for o in O)});")
    sql.append("INSERT INTO tinsel_ornaments (Id, Name, NameId, Visible, AssetId, IconId, Rarity, `Order`) VALUES\n" + ",\n".join(
        f"({o['id']}, '{q(textes.get(str(o['nameId'])))}', {o['nameId']}, {b(o['visible'])}, {o['assetId']}, {o['iconId']}, {o.get('rarity', 0)}, "
        f"{o['order']})" for o in O) + ";")
if Ti:
    sql.append(f"DELETE FROM tinsel_titles WHERE Id IN ({','.join(str(t['id']) for t in Ti)});")
    sql.append("INSERT INTO tinsel_titles (Id, Name, NameId, Visible, CategoryId) VALUES\n" + ",\n".join(
        f"({t['id']}, '{q(textes.get(str(t['nameMaleId'])))}', {t['nameMaleId']}, {b(t['visible'])}, {t['categoryId']})" for t in Ti) + ";")
sql.append("COMMIT;")
os.makedirs(SQLDIR, exist_ok=True)
open(os.path.join(SQLDIR, '105_import274_emotes_ornements_titres.sql'), 'w', encoding='utf-8', newline='\n').write('\n'.join(sql) + '\n')
open(os.path.join(SQLDIR, '105_import274_emotes_ornements_titres-annuler.sql'), 'w', encoding='utf-8', newline='\n').write('\n'.join(
    ["-- PADGREF 23/10 : annule 105_import274_emotes_ornements_titres.sql", "START TRANSACTION;"] +
    ([f"DELETE FROM emotes WHERE id IN ({','.join(str(e['id']) for e in E)});"] if E else []) +
    ([f"DELETE FROM tinsel_ornaments WHERE Id IN ({','.join(str(o['id']) for o in O)});"] if O else []) +
    ([f"DELETE FROM tinsel_titles WHERE Id IN ({','.join(str(t['id']) for t in Ti)});"] if Ti else []) + ["COMMIT;"]) + '\n')
print('émotes', len(E), '| ornements', len(O), '| titres', len(Ti), '| textes', len(textes), '| images d\'ornements', len(img))
print('ex. émotes :', [textes.get(str(e['nameId'])) for e in E[:8]])
print('ex. ornements :', [textes.get(str(o['nameId'])) for o in O[:8]])
print('ex. titres :', [textes.get(str(t['nameMaleId'])) for t in Ti[:8]])
