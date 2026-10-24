# PADGREF 23/10 (import 2.74, lot 2, 2e recherche demandée par l'utilisateur : « pour tous les skins à 0 ou non remplis refais une
# recherche ; attention, plusieurs skins sont caméléon, d'autres sont des genres d'obvijevan (à monter niveau 20, plusieurs skins pour
# 1 objet) »). Différences avec lot2_skins.py :
#  - candidats élargis : skins du lot 1 ET skins 2.51 qu'aucun objet n'utilise (AppearanceId en base), sauf ceux déjà choisis ;
#  - pièces CAMÉLÉON (classes « ColorGray_<n>_<Pièce>… » seulement, recolorées avec les couleurs du personnage) rendues aussi ;
#    comparaison : 3 propositions « couleurs + forme » + 3 propositions « forme seule » (les rendus caméléon sont gris) ;
#  - OBJETS VIVANTS 2.74 (type 113, absents du 2.51) : 20 skins par objet (1 par niveau, comme items_livingobjects.SkinsCSV) :
#    candidats = blocs de 20 skins consécutifs ayant la pièce ; comparaison avec l'icône de niveau 1.
# Sortie : PLANCHE_SKINS_2.html (le choix d'un objet vivant = 1er skin du bloc).
import base64, glob, io, json, os, re, struct, subprocess, sys, zlib, collections
from concurrent.futures import ThreadPoolExecutor
from PIL import Image
import numpy as np
sys.stdout.reconfigure(encoding='utf-8')
ICI = os.path.dirname(os.path.abspath(__file__))
CB = os.path.dirname(ICI)
R = os.path.abspath(os.path.join(CB, '..', '..', '..'))
sys.path.insert(0, CB)
import maps_elements as me
sys.path.insert(0, os.path.join(R, '_OUTILS', 'serveur', 'donjons'))
import d2o, d2i
JAVA = os.path.join(R, '_OUTILS', 'logiciels', 'Java17', 'bin', 'java.exe')
FFDEC = os.path.join(R, '_OUTILS', 'logiciels', 'FFDec', 'ffdec.jar')
MYSQL = os.path.join(R, '_OUTILS', 'logiciels', 'MariaDB', 'bin', 'mysql.exe')
N = os.path.join(R, '_OUTILS', 'client', 'client_27x')
DOS = os.path.join(ICI, 'rendu_skins2')
NOMS = {'Chapeau': 'coiffe', 'Cape': 'cape', 'Bouclier': 'bouclier', 'Arme': 'arme'}
CAT_VIVANT = {16: 'Chapeau', 17: 'Cape', 82: 'Bouclier'}
# PADGREF 23/10 (tour 4) : pièces de chaque catégorie, sans tenir compte des majuscules (« cape », « Masque », « Coiffure »…)
PIECES = {'Chapeau': ['Chapeau', 'Masque', 'MasqueB', 'Coiffure'], 'Cape': ['Cape', 'CapeBas'], 'Bouclier': ['Bouclier'], 'Arme': ['Arme']}
src = open(os.path.join(ICI, 'lot2_planche.py'), encoding='utf-8').read()
exec(''.join(re.search(rf'(?ms)^def {n}\(.*?(?=^\S)', src).group(0) for n in ('recadre', 'signature', 'distance')))


def distance_forme(s1, s2):
    return np.abs(s1[2] - s2[2]).mean() * 0.8 + min(abs(np.log(s1[3] / s2[3])), 1) * 0.2


def rq(s):
    out = subprocess.run([MYSQL, '-uroot', '-h127.0.0.1', '-N', '-B', '--default-character-set=utf8', 'world', '-e', s],
                         capture_output=True, check=True).stdout.decode('utf-8')
    return [l.split('\t') for l in out.replace('\r', '').split('\n') if l]


T = d2i.load(os.path.join(N, 'data', 'i18n', 'i18n_fr.d2i'))
IN = d2o.load(os.path.join(N, 'data', 'common', 'Items.d2o'))[1]
IA = d2o.load(os.path.join(R, 'CLIENT_64bits', 'app', 'data', 'common', 'Items.d2o'))[1]
LV = d2o.load(os.path.join(N, 'data', 'common', 'LivingObjectSkinJntMood.d2o'))[1]
# ------------------------------------------------------------------ objets
TOUR = int(sys.argv[1]) if len(sys.argv) > 1 else 2
SUFFIXE = '' if TOUR == 2 else f'_{TOUR}'
anciennes = {p['id']: p for p in json.load(open(os.path.join(ICI, 'lot2_skins_propositions.json')))}
fichiers_choix = ['choix_skins.txt'] + [f'choix_skins_{k}.txt' for k in range(2, TOUR)]
tous_choix = {}
for f in fichiers_choix:
    tous_choix.update({int(a): int(b) for a, b in re.findall(r'(\d+)=(\d+)', open(os.path.join(ICI, f)).read()) if int(b)})
deja_vus = collections.defaultdict(set)
for f in ['lot2_skins_propositions.json', 'lot2_skins2_propositions.json'] + [f'lot2_skins2_propositions_{k}.json' for k in range(3, TOUR)]:
    if os.path.exists(os.path.join(ICI, f)):
        for p in json.load(open(os.path.join(ICI, f))):
            deja_vus[p['id']] |= set(map(str, p['props']))
objets = []
for i in [x for x in json.load(open(os.path.join(ICI, 'skins_a_rechercher.json'))) if x not in tous_choix]:
    p = anciennes[i]
    objets.append({'id': i, 'nom': T.get(IN[i]['nameId']), 'cat': p['cat'], 'icone': IN[i]['iconId'], 'genre': p['genre'], 'vivant': False})
for i, v in sorted(IN.items()):
    if v['typeId'] == 113 and i not in IA:
        e = [x['value'] for x in v['possibleEffects'] if x['effectId'] == 973]
        if e and e[0] in CAT_VIVANT and i not in tous_choix:
            ic = LV[i]['moods'][0][0] if i in LV and LV[i]['moods'] and LV[i]['moods'][0] else v['iconId']
            objets.append({'id': i, 'nom': T.get(v['nameId']), 'cat': CAT_VIVANT[e[0]], 'icone': ic, 'genre': 'objet vivant', 'vivant': True})
print('objets :', len(objets), collections.Counter((o['cat'], o['genre']) for o in objets), flush=True)
os.makedirs(os.path.join(DOS, 'icones'), exist_ok=True)
voulues = {f"{o['icone']}.png" for o in objets}
for f in glob.glob(os.path.join(N, 'content', 'gfx', 'items', 'bitmap*.d2p')):
    for n, b in me.read_d2p(f):
        if n in voulues and not os.path.exists(os.path.join(DOS, 'icones', n)):
            open(os.path.join(DOS, 'icones', n), 'wb').write(b)

# ------------------------------------------------------------------ candidats
utilises = {int(a) for (a,) in rq("SELECT AppearanceId FROM items_templates WHERE AppearanceId > 0 UNION "
                                  "SELECT AppearanceId FROM items_templates_weapons WHERE AppearanceId > 0")}
utilises |= set(tous_choix.values())
archives = sorted(glob.glob(os.path.join(ICI, 'client', 'content', 'gfx', 'sprites', 'skins_3*.d2p'))) + \
           sorted(glob.glob(os.path.join(R, 'CLIENT_64bits', 'app', 'content', 'gfx', 'sprites', 'skins*.d2p')))
classes = {}
os.makedirs(os.path.join(DOS, 'swf'), exist_ok=True)
for f in archives:
    for n, b in me.read_d2p(f):
        if not n.endswith('.swl') or not n[:-4].isdigit() or int(n[:-4]) in utilises or n[:-4] in classes or len(b) < 12:
            continue
        p, cl = 10, []
        for _ in range(struct.unpack_from('>i', b, 6)[0]):
            l = struct.unpack_from('>H', b, p)[0]; cl.append(b[p + 2:p + 2 + l].decode('utf-8', 'replace')); p += 2 + l
        cats = {c for c in NOMS if any(re.match(rf'(?i)^((color|colorgray)_\d+_)?({"|".join(PIECES[c])})_', x) for x in cl)}
        if cats:
            classes[n[:-4]] = (cats, cl)
            swf = os.path.join(DOS, 'swf', n[:-4] + '.swf')
            if not os.path.exists(swf):
                i = min(q for q in (b.find(b'CWS'), b.find(b'FWS'), b.find(b'ZWS')) if q >= 0)
                open(swf, 'wb').write(b[i:])
print('skins candidats :', len(classes), flush=True)


def ids_classes(swf):
    d = open(swf, 'rb').read()
    if d[:3] == b'CWS':
        d = d[:8] + zlib.decompress(d[8:])
    elif d[:3] != b'FWS':
        return {}
    nb = d[8] >> 3; p = 8 + (5 + nb * 4 + 7) // 8 + 4; res = {}
    while p < len(d):
        h = struct.unpack_from('<H', d, p)[0]; p += 2; t, l = h >> 6, h & 63
        if l == 63:
            l = struct.unpack_from('<I', d, p)[0]; p += 4
        if t == 76:
            b = d[p:p + l]; q = 2
            for _ in range(struct.unpack_from('<H', b)[0]):
                cid = struct.unpack_from('<H', b, q)[0]; e = b.index(b'\0', q + 2); res[b[q + 2:e].decode('utf-8', 'replace')] = cid; q = e + 1
        p += l
    return res


def choisir(cl, cat):
    """pièce de la catégorie, de face (_1) de préférence ; version normale avant la version caméléon (ColorGray)."""
    norm = [x for p in PIECES[cat] for x in cl if re.match(rf'(?i)^{p}_', x)]
    gris = [x for p in PIECES[cat] for x in cl if re.match(rf'(?i)^(color|colorgray)_\d+_{p}_', x)]
    for liste in (norm, gris):
        pref = [x for x in liste if x.endswith('_1')] or liste
        if pref:
            return pref[0], liste is gris
    return None, False


def rendre(t):
    sid, cat = t
    out = os.path.join(DOS, 'png', cat, sid)
    if glob.glob(os.path.join(out, '*', '1.png')):
        return 'déjà'
    cls, _ = choisir(classes[sid][1], cat)
    ids = ids_classes(os.path.join(DOS, 'swf', sid + '.swf'))
    if not cls or cls not in ids:
        return 'sans classe'
    try:
        subprocess.run([JAVA, '-jar', FFDEC, '-selectid', str(ids[cls]), '-zoom', '3', '-format', 'sprite:png', '-export', 'sprite', out,
                        os.path.join(DOS, 'swf', sid + '.swf')], capture_output=True, timeout=120)
    except subprocess.TimeoutExpired:
        return 'délai'
    return 'ok' if glob.glob(os.path.join(out, '*', '1.png')) else 'échec'


besoin = {o['cat'] for o in objets}
# rendus du 1er tour réutilisés (copie des dossiers déjà rendus)
for cat in besoin:
    for d in glob.glob(os.path.join(ICI, 'rendu_skins', 'png', cat, '*')):
        dst = os.path.join(DOS, 'png', cat, os.path.basename(d))
        if os.path.basename(d) in classes and not os.path.exists(dst):
            import shutil; shutil.copytree(d, dst)
travaux = [(sid, cat) for sid, (cats, _) in classes.items() for cat in cats if cat in besoin]
with ThreadPoolExecutor(4) as ex:
    res = list(ex.map(rendre, travaux))
print('rendus :', collections.Counter(res), flush=True)

# ------------------------------------------------------------------ planche


def b64(path, taille=96):
    im = recadre(Image.open(path)); im.thumbnail((taille, taille))
    o = io.BytesIO(); im.save(o, 'PNG'); return base64.b64encode(o.getvalue()).decode()


cands = collections.defaultdict(dict)
for cat in besoin:
    for d in glob.glob(os.path.join(DOS, 'png', cat, '*')):
        p = glob.glob(os.path.join(d, '*', '1.png'))
        sid = os.path.basename(d)
        if p and sid in classes:
            s = signature(Image.open(p[0]))
            if s:
                cands[cat][sid] = (p[0], s, choisir(classes[sid][1], cat)[1])
lignes = []
for o in objets:
    ic = os.path.join(DOS, 'icones', f"{o['icone']}.png")
    if not os.path.exists(ic):
        continue
    pool = cands[o['cat']]
    if o['vivant']:                               # début d'un bloc de 20 skins consécutifs de la même pièce
        pool = {c: v for c, v in pool.items() if c.isdigit() and all(str(int(c) + k) in classes for k in range(20))}
    pool = {c: v for c, v in pool.items() if c not in deja_vus[o['id']]}     # tours suivants : propositions nouvelles seulement
    if not pool:
        continue
    s = signature(Image.open(ic))
    top = sorted(pool, key=lambda c: distance(s, pool[c][1]))[:3]
    top += [c for c in sorted(pool, key=lambda c: distance_forme(s, pool[c][1])) if c not in top][:3]
    lignes.append({**o, 'img': b64(ic), 'props': [(c, b64(pool[c][0]), pool[c][2]) for c in top]})
json.dump([{'id': l['id'], 'cat': l['cat'], 'genre': l['genre'], 'vivant': l['vivant'], 'props': [c for c, _, _ in l['props']]} for l in lignes],
          open(os.path.join(ICI, f'lot2_skins2_propositions{SUFFIXE}.json'), 'w'))
html = ['<!doctype html><html lang="fr"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1">',
        f'<title>Planche skins {TOUR}</title><style>',
        ':root{--bg:#f6f4ef;--fg:#222;--carte:#fff;--bord:#ddd6c8;--acc:#2f7d4f;--doux:#777;--cam:#7b4fb0}',
        '@media (prefers-color-scheme:dark){:root{--bg:#1d1c1a;--fg:#eee;--carte:#2a2926;--bord:#45423c;--acc:#5fc28a;--doux:#aaa;--cam:#b996e0}}',
        'body{background:var(--bg);color:var(--fg);font:14px system-ui,sans-serif;margin:0;padding:16px}',
        'h1{font-size:20px;margin:0 0 4px}h2{font-size:16px;margin:20px 0 8px}.aide{color:var(--doux);margin:0 0 16px;max-width:75ch}',
        '.l{display:flex;gap:10px;align-items:center;background:var(--carte);border:1px solid var(--bord);border-radius:8px;padding:8px;margin-bottom:8px;flex-wrap:wrap}',
        '.ic{width:160px}.ic b{display:block}.ic small{color:var(--doux)}.sep{width:1px;align-self:stretch;background:var(--bord)}',
        'label{display:flex;flex-direction:column;align-items:center;gap:2px;border:2px solid transparent;border-radius:6px;padding:4px;cursor:pointer;font-size:11px;color:var(--doux)}',
        'label.cam{color:var(--cam)}label:has(input:checked){border-color:var(--acc);color:var(--fg)}input{display:none}img{max-height:96px;max-width:110px}',
        '#bar{position:sticky;top:0;background:var(--bg);padding:8px 0;display:flex;gap:8px;align-items:center;z-index:2}',
        'button{background:var(--acc);color:#fff;border:0;border-radius:6px;padding:8px 14px;font-weight:600;cursor:pointer}',
        'textarea{width:100%;height:80px;margin-top:8px}</style></head><body>',
        f'<h1>Recherche n°{TOUR} : coiffes, capes, boucliers, armes + objets vivants (nouvelles propositions)</h1>',
        '<p class="aide">Pour chaque objet : 3 propositions « couleurs + forme », puis (après le trait) 3 « forme seule ». Les dessins '
        'en <b>gris</b> (étiquette violette « caméléon ») prennent les couleurs du personnage en jeu. Objets vivants : le dessin montré est '
        'celui du niveau 1 ; le choix prend les 20 skins qui suivent (1 par niveau). « Aucun » si rien ne va. Puis « Copier mes choix ».</p>',
        '<div id="bar"><button onclick="copier()">Copier mes choix</button><span id="n"></span></div>']
for titre, filtre in [('Objets vivants (20 dessins chacun)', lambda l: l['vivant'])] + \
                     [(NOMS[c].capitalize() + 's', (lambda c: lambda l: not l['vivant'] and l['cat'] == c)(c)) for c in NOMS]:
    sel = [l for l in lignes if filtre(l)]
    if not sel:
        continue
    html.append(f'<h2>{titre} ({len(sel)})</h2>')
    for l in sel:
        html.append(f'<div class="l"><div class="ic"><img src="data:image/png;base64,{l["img"]}"><b>{l["nom"]}</b><small>{l["genre"]} · {l["id"]}</small></div>')
        for k, (c, im, gris) in enumerate(l['props']):
            if k == 3:
                html.append('<div class="sep"></div>')
            html.append(f'<label class="{"cam" if gris else ""}"><input type="radio" name="o{l["id"]}" value="{c}"><img src="data:image/png;base64,{im}">'
                        f'skin {c}{" · caméléon" if gris else ""}</label>')
        html.append(f'<label><input type="radio" name="o{l["id"]}" value="0">aucun</label></div>')
html.append('''<textarea id="t" readonly placeholder="Tes choix apparaîtront ici"></textarea><script>
function choix(){return [...document.querySelectorAll('input:checked')].map(i=>i.name.slice(1)+'='+i.value).join(' ')}
function maj(){document.getElementById('n').textContent=document.querySelectorAll('input:checked').length+' / ''' + str(len(lignes)) + ''' choisis'}
document.addEventListener('change',maj);maj();
function copier(){const t=document.getElementById('t');t.value='choix skins ''' + str(TOUR) + ''' : '+choix();t.select();
try{navigator.clipboard.writeText(t.value).catch(()=>{})}catch(e){}}
</script></body></html>''')
open(os.path.join(R, f'PLANCHE_SKINS_{TOUR}.html'), 'w', encoding='utf-8').write('\n'.join(html))
print('planche :', len(lignes), 'objets ; objets vivants sans bloc de 20 :', [o['nom'] for o in objets if o['vivant'] and o['id'] not in {l['id'] for l in lignes}], flush=True)
