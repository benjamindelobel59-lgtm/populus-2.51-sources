# PADGREF 23/10 (import 2.74, lot 2, suite) : dessins PORTÉS (skins) des coiffes / capes / boucliers / armes :
#   - apparats 2.74 absents de notre client (types 246 chapeau, 247 cape, 248 bouclier, 251 arme) ;
#   - équipements déjà importés sans apparence (recherchés de Frigost, Cavaliers de l'Eliocalypse : AppearanceId 0).
# Candidats : skins du 2.74 ajoutés au lot 1 (skins_3*) dont les classes contiennent la pièce de la catégorie (Chapeau, Cape,
# Bouclier, Arme). Rendu (FFDec -selectid) d'UNE classe de la pièce (direction 1 de préférence, pas la version « ColorGray »),
# puis planche : les 5 skins les plus proches de l'icône (même comparateur que lot2_planche.py). Sortie : PLANCHE_SKINS.html.
import base64, glob, io, json, os, re, struct, subprocess, sys, zlib
from concurrent.futures import ThreadPoolExecutor
from PIL import Image
sys.stdout.reconfigure(encoding='utf-8')
ICI = os.path.dirname(os.path.abspath(__file__))
CB = os.path.dirname(ICI)
R = os.path.abspath(os.path.join(CB, '..', '..', '..'))
sys.path.insert(0, CB)
import maps_elements as me
sys.path.insert(0, os.path.join(R, '_OUTILS', 'serveur', 'donjons'))
import d2o, d2i
import numpy as np


# comparateur identique à lot2_planche.py
def recadre(im):
    im = im.convert('RGBA'); b = im.getchannel('A').point(lambda a: 255 if a > 40 else 0).getbbox()
    return im.crop(b) if b else im


def signature(im):
    im = recadre(im)
    petit = im.resize((24, 24), Image.LANCZOS)
    a = np.asarray(petit.getchannel('A'), dtype=float) / 255
    hsv = np.asarray(petit.convert('RGB').convert('HSV'), dtype=float)
    m = a > 0.3
    if m.sum() == 0:
        return None
    h = np.histogram2d(hsv[..., 0][m], hsv[..., 1][m], bins=(12, 4), range=((0, 256), (0, 256)), weights=a[m])[0].ravel()
    v = np.histogram(hsv[..., 2][m], bins=6, range=(0, 256), weights=a[m])[0]
    h = h / (h.sum() or 1); v = v / (v.sum() or 1)
    return h, v, (a > 0.3).astype(float), im.size[0] / max(1, im.size[1])


def distance(s1, s2):
    h = 1 - np.minimum(s1[0], s2[0]).sum()
    v = 1 - np.minimum(s1[1], s2[1]).sum()
    forme = np.abs(s1[2] - s2[2]).mean()
    ratio = abs(np.log(s1[3] / s2[3]))
    return h * 0.55 + v * 0.15 + forme * 0.2 + min(ratio, 1) * 0.1


JAVA = os.path.join(R, '_OUTILS', 'logiciels', 'Java17', 'bin', 'java.exe')
FFDEC = os.path.join(R, '_OUTILS', 'logiciels', 'FFDec', 'ffdec.jar')
MYSQL = os.path.join(R, '_OUTILS', 'logiciels', 'MariaDB', 'bin', 'mysql.exe')
N = os.path.join(R, '_OUTILS', 'client', 'client_27x')
DOS = os.path.join(ICI, 'rendu_skins')
CAT_274 = {246: 'Chapeau', 247: 'Cape', 248: 'Bouclier', 251: 'Arme'}
CAT_251 = {16: 'Chapeau', 17: 'Cape', 82: 'Bouclier'}           # armes : tous les autres types d'arme
NOMS = {'Chapeau': 'coiffe', 'Cape': 'cape', 'Bouclier': 'bouclier', 'Arme': 'arme'}


def rq(s):
    out = subprocess.run([MYSQL, '-uroot', '-h127.0.0.1', '-N', '-B', '--default-character-set=utf8', 'world', '-e', s],
                         capture_output=True, check=True).stdout.decode('utf-8')
    return [l.split('\t') for l in out.replace('\r', '').split('\n') if l]


T = d2i.load(os.path.join(N, 'data', 'i18n', 'i18n_fr.d2i'))
IN = d2o.load(os.path.join(N, 'data', 'common', 'Items.d2o'))[1]
IA = d2o.load(os.path.join(R, 'CLIENT_64bits', 'app', 'data', 'common', 'Items.d2o'))[1]
# ------------------------------------------------------------------ objets à habiller
objets = []
for i, v in sorted(IN.items()):
    if v['typeId'] in CAT_274 and i not in IA:
        objets.append({'id': i, 'nom': T.get(v['nameId']), 'cat': CAT_274[v['typeId']], 'icone': v['iconId'], 'genre': 'apparat'})
for i, t, nid in rq("SELECT Id, TypeId, NameId FROM items_templates WHERE NameId >= 9000000 AND AppearanceId = 0 AND TypeId IN (16, 17, 82) "
                    "UNION SELECT Id, TypeId, NameId FROM items_templates_weapons WHERE NameId >= 9000000 AND AppearanceId = 0"):
    i = int(i)
    if i in IN:
        objets.append({'id': i, 'nom': T.get(IN[i]['nameId']), 'cat': CAT_251.get(int(t), 'Arme'), 'icone': IN[i]['iconId'], 'genre': 'équipement'})
print('objets :', len(objets), {c: sum(1 for o in objets if o['cat'] == c) for c in NOMS}, flush=True)
# icônes
os.makedirs(os.path.join(DOS, 'icones'), exist_ok=True)
voulues = {f"{o['icone']}.png" for o in objets}
for f in glob.glob(os.path.join(N, 'content', 'gfx', 'items', 'bitmap*.d2p')):
    for n, b in me.read_d2p(f):
        if n in voulues and not os.path.exists(os.path.join(DOS, 'icones', n)):
            open(os.path.join(DOS, 'icones', n), 'wb').write(b)

# ------------------------------------------------------------------ candidats (skins du lot 1) et classe à dessiner
classes = {}
os.makedirs(os.path.join(DOS, 'swf'), exist_ok=True)
for f in sorted(glob.glob(os.path.join(ICI, 'client', 'content', 'gfx', 'sprites', 'skins_3*.d2p'))):
    for n, b in me.read_d2p(f):
        p, cl = 10, []
        for _ in range(struct.unpack_from('>i', b, 6)[0]):
            l = struct.unpack_from('>H', b, p)[0]; cl.append(b[p + 2:p + 2 + l].decode('utf-8', 'replace')); p += 2 + l
        cats = {c for c in NOMS if any(re.match(rf'^{c}_', x) for x in cl)}
        if cats:
            sid = n[:-4]
            classes[sid] = (cats, cl)
            swf = os.path.join(DOS, 'swf', sid + '.swf')
            if not os.path.exists(swf):
                i = min(q for q in (b.find(b'CWS'), b.find(b'FWS'), b.find(b'ZWS')) if q >= 0)
                open(swf, 'wb').write(b[i:])
print('skins candidats :', len(classes), flush=True)


def ids_classes(swf):
    d = open(swf, 'rb').read()
    if d[:3] == b'CWS':
        d = d[:8] + zlib.decompress(d[8:])
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
    """classe de la pièce : <cat>_<n>_1 de préférence (de face), sinon la première de la pièce."""
    c = [x for x in cl if re.match(rf'^{cat}_[^_]*_?\d*$', x) or x.startswith(cat + '_')]
    pref = [x for x in c if x.endswith('_1')] or c
    return pref[0] if pref else None


def rendre(t):
    sid, cat = t
    out = os.path.join(DOS, 'png', cat, sid)
    if glob.glob(os.path.join(out, '*', '1.png')):
        return 'déjà'
    swf = os.path.join(DOS, 'swf', sid + '.swf')
    cls = choisir(classes[sid][1], cat)
    ids = ids_classes(swf)
    if not cls or cls not in ids:
        return 'sans classe'
    try:
        subprocess.run([JAVA, '-jar', FFDEC, '-selectid', str(ids[cls]), '-zoom', '3', '-format', 'sprite:png', '-export', 'sprite', out, swf],
                       capture_output=True, timeout=120)
    except subprocess.TimeoutExpired:
        return 'délai'
    return 'ok' if glob.glob(os.path.join(out, '*', '1.png')) else 'échec'


travaux = [(sid, cat) for sid, (cats, _) in classes.items() for cat in cats]
with ThreadPoolExecutor(4) as ex:
    res = list(ex.map(rendre, travaux))
import collections
print('rendus :', collections.Counter(res), flush=True)

# ------------------------------------------------------------------ planche


def b64(path, taille=96):
    im = recadre(Image.open(path)); im.thumbnail((taille, taille))
    o = io.BytesIO(); im.save(o, 'PNG'); return base64.b64encode(o.getvalue()).decode()


cands = collections.defaultdict(dict)
for cat in NOMS:
    for d in glob.glob(os.path.join(DOS, 'png', cat, '*')):
        p = glob.glob(os.path.join(d, '*', '1.png'))
        if p:
            s = signature(Image.open(p[0]))
            if s:
                cands[cat][os.path.basename(d)] = (p[0], s)
lignes = []
for o in objets:
    ic = os.path.join(DOS, 'icones', f"{o['icone']}.png")
    if not os.path.exists(ic) or not cands[o['cat']]:
        continue
    s = signature(Image.open(ic))
    top = sorted(cands[o['cat']], key=lambda c: distance(s, cands[o['cat']][c][1]))[:5]
    lignes.append({**o, 'img': b64(ic), 'props': [(c, b64(cands[o['cat']][c][0])) for c in top]})
json.dump([{'id': l['id'], 'cat': l['cat'], 'genre': l['genre'], 'props': [c for c, _ in l['props']]} for l in lignes],
          open(os.path.join(ICI, 'lot2_skins_propositions.json'), 'w'))
html = ['<!doctype html><html lang="fr"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1">',
        '<title>Planche skins</title><style>',
        ':root{--bg:#f6f4ef;--fg:#222;--carte:#fff;--bord:#ddd6c8;--acc:#2f7d4f;--doux:#777}',
        '@media (prefers-color-scheme:dark){:root{--bg:#1d1c1a;--fg:#eee;--carte:#2a2926;--bord:#45423c;--acc:#5fc28a;--doux:#aaa}}',
        'body{background:var(--bg);color:var(--fg);font:14px system-ui,sans-serif;margin:0;padding:16px}',
        'h1{font-size:20px;margin:0 0 4px}h2{font-size:16px;margin:20px 0 8px}.aide{color:var(--doux);margin:0 0 16px;max-width:70ch}',
        '.l{display:flex;gap:10px;align-items:center;background:var(--carte);border:1px solid var(--bord);border-radius:8px;padding:8px;margin-bottom:8px;flex-wrap:wrap}',
        '.ic{width:160px}.ic b{display:block}.ic small{color:var(--doux)}',
        'label{display:flex;flex-direction:column;align-items:center;gap:2px;border:2px solid transparent;border-radius:6px;padding:4px;cursor:pointer;font-size:11px;color:var(--doux)}',
        'label:has(input:checked){border-color:var(--acc);color:var(--fg)}input{display:none}img{max-height:96px;max-width:110px}',
        '#bar{position:sticky;top:0;background:var(--bg);padding:8px 0;display:flex;gap:8px;align-items:center;z-index:2}',
        'button{background:var(--acc);color:#fff;border:0;border-radius:6px;padding:8px 14px;font-weight:600;cursor:pointer}',
        'textarea{width:100%;height:80px;margin-top:8px}</style></head><body>',
        '<h1>Coiffes, capes, boucliers, armes : quel dessin porté ?</h1>',
        '<p class="aide">Pour chaque objet (icône à gauche), clique sur le dessin qui correspond (vu de face, en grand). « Aucun » si aucun '
        'ne va. Puis « Copier mes choix » et colle le texte dans la conversation.</p>',
        '<div id="bar"><button onclick="copier()">Copier mes choix</button><span id="n"></span></div>']
for cat in NOMS:
    sel = [l for l in lignes if l['cat'] == cat]
    if not sel:
        continue
    html.append(f'<h2>{NOMS[cat].capitalize()}s ({len(sel)})</h2>')
    for l in sorted(sel, key=lambda x: (x['genre'] != 'équipement', x['id'])):
        html.append(f'<div class="l"><div class="ic"><img src="data:image/png;base64,{l["img"]}"><b>{l["nom"]}</b><small>{l["genre"]} · {l["id"]}</small></div>')
        for c, im in l['props']:
            html.append(f'<label><input type="radio" name="o{l["id"]}" value="{c}"><img src="data:image/png;base64,{im}">skin {c}</label>')
        html.append(f'<label><input type="radio" name="o{l["id"]}" value="0">aucun</label></div>')
html.append('''<textarea id="t" readonly placeholder="Tes choix apparaîtront ici"></textarea><script>
function choix(){return [...document.querySelectorAll('input:checked')].map(i=>i.name.slice(1)+'='+i.value).join(' ')}
function maj(){document.getElementById('n').textContent=document.querySelectorAll('input:checked').length+' / ''' + str(len(lignes)) + ''' choisis'}
document.addEventListener('change',maj);maj();
function copier(){const t=document.getElementById('t');t.value='choix skins : '+choix();t.select();
try{navigator.clipboard.writeText(t.value).catch(()=>{})}catch(e){}}
</script></body></html>''')
open(os.path.join(R, 'PLANCHE_SKINS.html'), 'w', encoding='utf-8').write('\n'.join(html))
print('planche :', len(lignes), 'objets', flush=True)
