# PADGREF 23/10 (import 2.74, lot 2) : planche « objet / dessin proposé » des familiers et montiliers d'apparat.
# Pour chaque objet (icône 2.74), les 5 os candidats (rendus AnimStatique_1 par FFDec, rendu/png) les plus proches : couleurs
# (histogramme teinte / saturation / valeur sur les pixels visibles) + silhouette (masque 24x24). L'utilisateur choisit sur la
# planche HTML (PLANCHE_APPARATS.html, à la racine du projet), puis recopie le texte « choix » produit par la page.
import base64, glob, io, json, os, re, sys
from PIL import Image
import numpy as np
sys.stdout.reconfigure(encoding='utf-8')
ICI = os.path.dirname(os.path.abspath(__file__))
R = os.path.abspath(os.path.join(ICI, '..', '..', '..', '..'))
objets = json.load(open(os.path.join(ICI, 'lot2_familiers.json'), encoding='utf-8'))


def rendu(os_id):
    for k in range(4):
        for d in glob.glob(os.path.join(ICI, 'rendu', 'png', str(k), f'{os_id}.swf', '*AnimStatique_1', '1.png')) + \
                 glob.glob(os.path.join(ICI, 'rendu', 'png', str(k), f'{os_id}.swf', 'sprites', '*AnimStatique_1', '1.png')):
            return d


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


def b64(path, taille=96):
    im = recadre(Image.open(path)); im.thumbnail((taille, taille))
    o = io.BytesIO(); im.save(o, 'PNG'); return base64.b64encode(o.getvalue()).decode()


cands = {}
for c in json.load(open(os.path.join(ICI, 'candidats_os.json'))):
    p = rendu(c)
    if p:
        s = signature(Image.open(p))
        if s:
            cands[c] = (p, s)
print('rendus utilisables :', len(cands))
lignes = []
for o in objets:
    ic = os.path.join(ICI, 'rendu', 'icones', f"{o['icone']}.png")
    if not os.path.exists(ic):
        continue
    s = signature(Image.open(ic))
    top = sorted(cands, key=lambda c: distance(s, cands[c][1]))[:5]
    lignes.append({'id': o['id'], 'nom': o['nom'], 'type': o['type'], 'icone': b64(ic), 'props': [(c, b64(cands[c][0])) for c in top]})
json.dump([{'id': l['id'], 'props': [c for c, _ in l['props']]} for l in lignes], open(os.path.join(ICI, 'lot2_propositions.json'), 'w'))
html = ['<!doctype html><html lang="fr"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1">',
        '<title>Planche apparats</title><style>',
        ':root{--bg:#f6f4ef;--fg:#222;--carte:#fff;--bord:#ddd6c8;--acc:#2f7d4f;--doux:#777}',
        '@media (prefers-color-scheme:dark){:root{--bg:#1d1c1a;--fg:#eee;--carte:#2a2926;--bord:#45423c;--acc:#5fc28a;--doux:#aaa}}',
        'body{background:var(--bg);color:var(--fg);font:14px system-ui,sans-serif;margin:0;padding:16px}',
        'h1{font-size:20px;margin:0 0 4px}.aide{color:var(--doux);margin:0 0 16px;max-width:70ch}',
        '.l{display:flex;gap:10px;align-items:center;background:var(--carte);border:1px solid var(--bord);border-radius:8px;padding:8px;margin-bottom:8px;flex-wrap:wrap}',
        '.ic{width:150px}.ic b{display:block}.ic small{color:var(--doux)}',
        'label{display:flex;flex-direction:column;align-items:center;gap:2px;border:2px solid transparent;border-radius:6px;padding:4px;cursor:pointer;font-size:11px;color:var(--doux)}',
        'label:has(input:checked){border-color:var(--acc);color:var(--fg)}input{display:none}img{max-height:96px}',
        '#bar{position:sticky;top:0;background:var(--bg);padding:8px 0;display:flex;gap:8px;align-items:center;z-index:2}',
        'button{background:var(--acc);color:#fff;border:0;border-radius:6px;padding:8px 14px;font-weight:600;cursor:pointer}',
        'textarea{width:100%;height:80px;margin-top:8px}</style></head><body>',
        '<h1>Familiers et montiliers d\'apparat : quel dessin ?</h1>',
        '<p class="aide">Pour chaque objet (icône à gauche), clique sur le dessin qui correspond. « Aucun » si aucun ne va. '
        'Puis « Copier mes choix » et colle le texte dans la conversation.</p>',
        '<div id="bar"><button onclick="copier()">Copier mes choix</button><span id="n"></span></div>']
for l in lignes:
    html.append(f'<div class="l"><div class="ic"><img src="data:image/png;base64,{l["icone"]}"><b>{l["nom"]}</b><small>{l["type"]} · {l["id"]}</small></div>')
    for c, im in l['props']:
        html.append(f'<label><input type="radio" name="o{l["id"]}" value="{c}"><img src="data:image/png;base64,{im}">os {c}</label>')
    html.append(f'<label><input type="radio" name="o{l["id"]}" value="0">aucun</label></div>')
html.append('''<textarea id="t" readonly placeholder="Tes choix apparaîtront ici"></textarea><script>
function choix(){return [...document.querySelectorAll('input:checked')].map(i=>i.name.slice(1)+'='+i.value).join(' ')}
function maj(){document.getElementById('n').textContent=document.querySelectorAll('input:checked').length+' / ''' + str(len(lignes)) + ''' choisis'}
document.addEventListener('change',maj);maj();
function copier(){const t=document.getElementById('t');t.value='choix apparats : '+choix();t.select();
try{navigator.clipboard.writeText(t.value).catch(()=>{})}catch(e){}}
</script></body></html>''')
open(os.path.join(R, 'PLANCHE_APPARATS.html'), 'w', encoding='utf-8').write('\n'.join(html))
print('planche :', len(lignes), 'objets')
