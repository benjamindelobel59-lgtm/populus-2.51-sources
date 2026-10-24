# PADGREF 23/10 (import 2.74, lot 2, familiers et montiliers d'apparat d'abord) : retrouver le numéro d'os (AppearanceId du
# serveur, look « {os} ») des familiers (type 2.74 249) et montiliers (250) d'apparat absents de notre client.
# Le client 2.74 ne le donne pas (son appearanceId est un numéro du serveur d'Ankama). Sources, dans l'ordre :
#   1. un objet 2.51 du même nom en base (familier, montilier ou apparat) -> son AppearanceId ;
#   2. un monstre (2.74) du même nom -> os de son look ;
#   3. une monture / un PNJ (2.74) du même nom -> os de son look ;
#   4. sinon : à trouver (planche).
# Sortie : lot2_familiers.json (objet -> os proposé, source) ; la planche est faite par lot2_planche.py.
import json, os, re, subprocess, sys, unicodedata
sys.stdout.reconfigure(encoding='utf-8')
ICI = os.path.dirname(os.path.abspath(__file__))
R = os.path.abspath(os.path.join(ICI, '..', '..', '..', '..'))
sys.path.insert(0, os.path.join(R, '_OUTILS', 'serveur', 'donjons'))
import d2o, d2i
MYSQL = os.path.join(R, '_OUTILS', 'logiciels', 'MariaDB', 'bin', 'mysql.exe')
N = os.path.join(R, '_OUTILS', 'client', 'client_27x', 'data')
A = os.path.join(R, 'CLIENT_64bits', 'app', 'data')
T = d2i.load(os.path.join(N, 'i18n', 'i18n_fr.d2i'))
IN = d2o.load(os.path.join(N, 'common', 'Items.d2o'))[1]
IA = d2o.load(os.path.join(A, 'common', 'Items.d2o'))[1]


def cle(s):
    s = unicodedata.normalize('NFD', s or '').encode('ascii', 'ignore').decode().lower()
    return re.sub(r'[^a-z0-9]+', ' ', s).strip()


def rq(s):
    out = subprocess.run([MYSQL, '-uroot', '-h127.0.0.1', '-N', '-B', '--default-character-set=utf8', 'world', '-e', s],
                         capture_output=True, check=True).stdout.decode('utf-8')
    return [l.split('\t') for l in out.replace('\r', '').split('\n') if l]


def os_du_look(look):
    m = re.match(r'\{(\d+)', look or '')
    return int(m.group(1)) if m else None


# 1. objets 2.51 en base (nom 2.51 du client -> AppearanceId serveur), familiers 18 / montiliers 121 / apparats 177
app_base = {int(i): int(a) for i, a in rq("SELECT Id, AppearanceId FROM items_templates WHERE AppearanceId > 0 AND TypeId IN (18, 121, 177)")}
TA = d2i.load(os.path.join(A, 'i18n', 'i18n_fr.d2i'))
par_nom_251 = {}
for i, a in app_base.items():
    if i in IA:
        par_nom_251.setdefault(cle(TA.get(IA[i]['nameId'])), a)
# 2-3. monstres, montures, PNJ du 2.74
par_nom_look = {}
for f, src in (('Monsters', 'monstre'), ('Mounts', 'monture'), ('Npcs', 'PNJ')):
    for k, v in d2o.load(os.path.join(N, 'common', f + '.d2o'))[1].items():
        nom = T.get(v.get('nameId'))
        look = v.get('look')
        if not look and v.get('grades'):
            look = None
        o = os_du_look(look) if isinstance(look, str) else None
        if nom and o:
            par_nom_look.setdefault(cle(nom), (o, f'{src} {k}'))
res = []
for i, v in sorted(IN.items()):
    if v['typeId'] not in (249, 250) or i in IA:
        continue
    nom = T.get(v['nameId'])
    k = cle(nom)
    k2 = re.sub(r'^(mini|petit|petite|bebe) ', '', k)
    if k in par_nom_251:
        prop, src = par_nom_251[k], 'objet 2.51 du même nom'
    elif k in par_nom_look:
        prop, src = par_nom_look[k]
    elif k2 in par_nom_look:
        prop, src = par_nom_look[k2]; src += ' (nom proche)'
    else:
        prop, src = None, 'à trouver'
    res.append({'id': i, 'nom': nom, 'type': 'familier' if v['typeId'] == 249 else 'montilier', 'icone': v['iconId'], 'os': prop, 'source': src,
                'appearance274': v.get('appearanceId')})
json.dump(res, open(os.path.join(ICI, 'lot2_familiers.json'), 'w', encoding='utf-8'), ensure_ascii=False, indent=0)
import collections
print(len(res), collections.Counter(r['source'].split(' ')[0] for r in res))
for r in res[:12]:
    print(r)
