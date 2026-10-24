# PADGREF 23/10 (demande utilisateur : « un html où je valide les panoplies », puis « mets-moi les recettes en dessous et je
# choisis quelle ressource on enlève et on remplace par un équivalent, vu qu'on n'a pas tous les donjons ») : planche des 64
# panoplies d'équipement 2.74 absentes (liste_panoplies.json), rangées par zone. Pour chaque panoplie : objets avec icône (bitmap
# 2.74), niveau, effets, recette (ressource obtenable chez nous ou non + menu garder / retirer / équivalent), bonus de panoplie,
# case à cocher. En bas : le texte « choix panoplies : id,… | recettes : objet:ressource>remplaçant,… » à coller dans la conversation
# (remplaçant 0 = ressource retirée).
# Obtenable chez nous = butin d'un monstre existant, résultat d'une recette, vendu par un PNJ ou récoltable (Skills.d2o).
# Équivalents proposés : ressources obtenables du même type, au niveau le plus proche.
# Sortie : PLANCHE_PANOPLIES.html (racine du projet).
import base64, collections, glob, html, json, os, re, subprocess, sys
sys.stdout.reconfigure(encoding='utf-8')
ICI = os.path.dirname(os.path.abspath(__file__))
CB = os.path.dirname(ICI)
R = os.path.abspath(os.path.join(CB, '..', '..', '..'))
sys.path.insert(0, CB); sys.path.insert(0, os.path.join(R, '_OUTILS', 'serveur', 'donjons'))
import maps_elements as me, d2o, d2i
N = os.path.join(R, '_OUTILS', 'client', 'client_27x')
A = os.path.join(R, 'CLIENT_64bits', 'app', 'data', 'common')
MYSQL = os.path.join(R, '_OUTILS', 'logiciels', 'MariaDB', 'bin', 'mysql.exe')
T = d2i.load(os.path.join(N, 'data', 'i18n', 'i18n_fr.d2i'))
L = lambda f: d2o.load(os.path.join(N, 'data', 'common', f + '.d2o'))[1]
IN, SN, EF = L('Items'), L('ItemSets'), L('Effects')
RE = {r['resultId']: r for r in L('Recipes').values()}
IA = d2o.load(os.path.join(A, 'Items.d2o'))[1]
pan = json.load(open(os.path.join(ICI, 'liste_panoplies.json'), encoding='utf-8'))


def rq(s):
    out = subprocess.run([MYSQL, '-uroot', '-h127.0.0.1', '-N', '-B', 'world', '-e', s], capture_output=True, check=True).stdout.decode()
    return [int(l) for l in out.split() if l.strip().isdigit()]


obtenables = set(rq('SELECT DISTINCT d.ItemId FROM monsters_drops d JOIN monsters_templates m ON m.Id = d.MonsterOwnerId'))
obtenables |= set(rq('SELECT Id FROM recipes')) | set(rq('SELECT DISTINCT ItemId FROM npcs_items'))
obtenables |= {s['gatheredRessourceItem'] for s in d2o.load(os.path.join(A, 'Skills.d2o'))[1].values() if s.get('gatheredRessourceItem', -1) > 0}
obtenables &= set(IA)
# pour le commentaire des ressources introuvables : monstres du 2.74 qui la donnent, présents ou non chez nous
MN, SA_, AR_ = L('Monsters'), L('SubAreas'), L('Areas')
monstres_chez_nous = set(rq('SELECT Id FROM monsters_templates'))
grades_en_jeu = set()
for t in ('monsters_spawns_dungeons_groups', 'monsters_spawns_static_groups', 'monsters_spawns_dungeons_waves'):
    grades_en_jeu |= set(rq(f'SELECT DISTINCT MonsterGradeId FROM {t}'))
_g = rq('SELECT Id, MonsterId FROM monsters_grades')
monstres_en_jeu = set(rq('SELECT DISTINCT MonsterId FROM monsters_spawns')) | {m for g, m in zip(_g[::2], _g[1::2]) if g in grades_en_jeu}
RE_ressources = RE                                        # recettes 2.74 (une ressource peut être fabriquée)


def pourquoi(ing):
    """Commentaire d'une ressource introuvable : d'où elle vient dans le 2.74 et s'il vaut la peine de la garder."""
    ms = [m for m in IN[ing].get('dropMonsterIds') or [] if m in MN]
    if ms:
        zones = collections.Counter(T.get(AR_[SA_[s]['areaId']]['nameId']) for m in ms for s in MN[m].get('subareas') or []
                                    if s in SA_ and SA_[s].get('areaId') in AR_)
        noms = ', '.join(sorted({T.get(MN[m]['nameId']) or str(m) for m in ms})[:4]) + (' …' if len(ms) > 4 else '')
        zone = f' ({zones.most_common(1)[0][0]})' if zones else ''
        en_jeu = [m for m in ms if m in monstres_en_jeu]
        presents = [m for m in ms if m in monstres_chez_nous]
        if en_jeu:
            qui = 'Ce monstre est' if len(ms) == 1 else f'{len(en_jeu)} de ces monstres sont'
            return f'Butin de {noms}{zone}. {qui} en jeu chez nous : on peut lui ajouter ce butin → à GARDER (je l\'ajoute à son butin).'
        if presents:
            return (f'Butin de {noms}{zone}. Monstre(s) dans notre base mais qui n\'apparaissent nulle part en jeu → garder seulement '
                    'si on les fait apparaître (lot 5), sinon remplacer.')
        return f'Butin de {noms}{zone}, monstres absents chez nous (lot 5) → garder seulement si tu importes le lot 5, sinon remplacer.'
    if ing in RE_ressources:
        sous = [i for i in RE_ressources[ing]['ingredientIds'] if i in IN]
        ok = all(i in obtenables for i in sous)
        return ('Ressource fabriquée dans le 2.74' + (' avec des ressources qu\'on a déjà → à GARDER (j\'importe aussi sa recette).'
                                                      if ok else ' mais avec d\'autres ressources introuvables → plutôt remplacer.'))
    return 'Aucun monstre ni recette dans le 2.74 (récolte, quête ou événement) → plutôt remplacer.'
par_type = collections.defaultdict(list)
for i in obtenables:
    if i in IN:
        par_type[IN[i]['typeId']].append(i)


def equivalents(ing):
    lv = IN[ing]['level']
    c = [i for i in par_type.get(IN[ing]['typeId'], []) if i != ing]
    return sorted(c, key=lambda i: (abs(IN[i]['level'] - lv), -IN[i]['level']))[:8]


ingredients = {i for p in pan for o in p['objets'] if o['id'] in RE for i in RE[o['id']]['ingredientIds'] if i in IN}
icones_voulues = {IN[o['id']]['iconId'] for p in pan for o in p['objets']} | {IN[i]['iconId'] for i in ingredients}
icones = {}
for f in sorted(glob.glob(os.path.join(N, 'content', 'gfx', 'items', 'bitmap*.d2p'))):
    for n, b in me.read_d2p(f):
        m = re.fullmatch(r'(\d+)\.png', n)
        if m and int(m.group(1)) in icones_voulues and int(m.group(1)) not in icones:
            icones[int(m.group(1))] = base64.b64encode(b).decode()


def effet(e):
    ef = EF.get(e.get('effectId'), {})
    t = T.get(ef.get('descriptionId')) or ''
    if not t or e.get('effectId') in (983, 984, 985, 811, 812, 813, 1151, 1176):
        return None
    a, b = e.get('diceNum', 0), e.get('diceSide', 0)
    t = re.sub(r'\{~1~2 (à|a) \}', ' à ' if b and b != a else '', t)
    t = t.replace('#1', str(a)).replace('#2', str(b) if b and b != a else '').replace('#3', str(e.get('value', 0)))
    return re.sub(r'\s+', ' ', re.sub(r'\{[^}]*\}', '', t)).strip()


def effets(liste):
    return [x for x in (effet(e) for e in liste or []) if x]


esc = html.escape
nom = lambda i: T.get(IN[i]['nameId']) or str(i)


def mini(i):
    ic = icones.get(IN[i]['iconId'])
    return f'<img class="mini" src="data:image/png;base64,{ic}" alt="">' if ic else '<span class="mini sans">?</span>'


def recette(obj):
    r = RE.get(obj)
    if not r:
        return '<div class="rec vide">Pas de recette (objet de butin / boutique)</div>'
    lignes, manque = [], 0
    for ing, q in zip(r['ingredientIds'], r['quantities']):
        if ing not in IN:
            continue
        ok = ing in obtenables
        manque += not ok
        etat = 'obtenable chez nous' if ok else ('INTROUVABLE chez nous' if ing in IA else "N'EXISTE PAS chez nous")
        opts = '<option value="">garder</option><option value="0">retirer (sans remplacement)</option>' + ''.join(
            f'<option value="{e}">{esc(nom(e))} (niv. {IN[e]["level"]})</option>' for e in equivalents(ing))
        lignes.append(f'<tr class="{"ok" if ok else "ko"}"><td>{mini(ing)}</td><td>{q} × <b>{esc(nom(ing))}</b>'
                      f'<span class="meta">niv. {IN[ing]["level"]} · {etat}</span>'
                      + ('' if ok else f'<span class="pourquoi">{esc(pourquoi(ing))}</span>') + '</td>'
                      f'<td><select data-obj="{obj}" data-ing="{ing}">{opts}</select></td></tr>')
    resume = f'<span class="ko">{manque} ressource(s) introuvable(s)</span>' if manque else '<span class="ok">tout est obtenable</span>'
    return f'<details class="rec"{" open" if manque else ""}><summary>Recette — {resume}</summary><table>{"".join(lignes)}</table></details>'


blocs = []
par_zone = collections.defaultdict(list)
for p in pan:
    par_zone[p['zone']].append(p)
ordre = sorted(par_zone, key=lambda z: (z.startswith(('Artisanat', 'Autre')), -len(par_zone[z])))
for z in ordre:
    cartes = []
    for p in sorted(par_zone[z], key=lambda p: p['niveau']):
        absents = 'absent' in p['comment']
        objets = []
        for o in p['objets']:
            it = IN[o['id']]
            ic = icones.get(it['iconId'])
            img = f'<img src="data:image/png;base64,{ic}" alt="">' if ic else '<div class="sans">?</div>'
            ef = ''.join(f'<li>{esc(x)}</li>' for x in effets(it.get('possibleEffects')))
            objets.append(f'<div class="bloc"><div class="obj">{img}<div class="txt"><b>{esc(o["nom"])}</b>'
                          f'<span class="meta">{esc(o["type"])} · niv. {o["niveau"]}{" · <em>déjà en jeu</em>" if o["chez_nous"] else ""}</span>'
                          f'<ul>{ef}</ul></div></div>{recette(o["id"])}</div>')
        bonus = ''
        for i, b in enumerate(SN[p['id']].get('effects') or []):
            t = effets(b)
            if t:
                bonus += f'<div class="bonus"><b>{i + 1} objets</b> : {esc(", ".join(t))}</div>'
        alerte = '<div class="alerte">Monstres de cette zone absents chez nous (lot 5) : butin / ressources introuvables pour l\'instant</div>' if absents else ''
        cartes.append(f'''<div class="pan{' warn' if absents else ''}" data-id="{p['id']}">
<label class="tete"><input type="checkbox" value="{p['id']}"><div><h3>{esc(p['nom'])}</h3>
<span class="meta">niv. {p['niveau']} · {len(p['objets'])} objets · panoplie {p['id']}</span></div></label>
<div class="comment">{esc(re.sub(r"[*]", "", p['comment']))}</div>{alerte}
<div class="objs">{''.join(objets)}</div><details><summary>Bonus de panoplie</summary>{bonus or '<i>aucun</i>'}</details></div>''')
    blocs.append(f'<section><h2>{esc(z)} <span>{len(par_zone[z])}</span> '
                 f'<button type="button" class="tout" data-z="{len(blocs)}">tout cocher</button></h2><div class="grille" id="z{len(blocs)}">{"".join(cartes)}</div></section>')

page = f'''<!doctype html><html lang="fr"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1">
<title>Planche panoplies 2.74</title><style>
:root{{--bg:#f4f1ea;--carte:#fffdf8;--fg:#2b2620;--doux:#7a6f60;--trait:#e2dacb;--acc:#a0522d;--ok:#2f7d4f;--warn:#b7791f;--ko:#c0392b}}
@media (prefers-color-scheme:dark){{:root{{--bg:#1c1a17;--carte:#262320;--fg:#ece5d8;--doux:#a89c8a;--trait:#3a352f;--acc:#e08a5a;--ok:#5fbf86;--warn:#e0a84a;--ko:#ff7b6b}}}}
*{{box-sizing:border-box}}body{{margin:0;background:var(--bg);color:var(--fg);font:14px/1.45 system-ui,sans-serif;padding:0 max(16px,env(safe-area-inset-left)) 120px}}
header{{padding:24px 0 8px}}h1{{margin:0;font-size:24px}}header p{{color:var(--doux);max-width:80ch}}
h2{{font-size:18px;margin:28px 0 10px;display:flex;gap:10px;align-items:center}}h2 span{{color:var(--doux);font-weight:400}}
.tout{{margin-left:auto;font-size:12px;background:none;border:1px solid var(--trait);color:var(--fg);border-radius:6px;padding:3px 8px;cursor:pointer}}
.grille{{display:grid;grid-template-columns:repeat(auto-fill,minmax(420px,1fr));gap:12px}}
@media (max-width:480px){{.grille{{grid-template-columns:1fr}}}}
.pan{{background:var(--carte);border:2px solid var(--trait);border-radius:10px;padding:12px;min-width:0}}
.pan.warn{{border-style:dashed}}.pan:has(.tete input:checked){{border-color:var(--ok);border-style:solid;box-shadow:0 0 0 2px color-mix(in srgb,var(--ok) 25%,transparent)}}
.tete{{display:flex;gap:10px;align-items:flex-start;cursor:pointer}}.tete input{{width:20px;height:20px;margin-top:3px;accent-color:var(--ok)}}
h3{{margin:0;font-size:16px}}.meta{{color:var(--doux);font-size:12px;display:block}}
.comment{{font-size:12px;color:var(--doux);margin:6px 0}}.alerte{{font-size:12px;color:var(--warn);font-weight:600;margin:4px 0}}
.objs{{display:grid;gap:6px;margin-top:8px}}.bloc{{border-top:1px solid var(--trait);padding-top:6px}}.obj{{display:flex;gap:8px}}
.obj img,.sans{{width:48px;height:48px;flex:none;object-fit:contain}}.sans{{display:grid;place-items:center;border:1px dashed var(--trait);color:var(--doux)}}
.txt{{min-width:0}}.obj ul{{margin:2px 0 0;padding-left:16px;font-size:12px;columns:2;column-gap:14px}}
.rec{{margin:4px 0 2px 56px;font-size:12px}}.rec table{{border-collapse:collapse;width:100%}}
.rec td{{padding:3px 4px;border-top:1px solid var(--trait);vertical-align:middle}}
.rec select{{max-width:190px;font-size:12px;background:var(--bg);color:var(--fg);border:1px solid var(--trait);border-radius:4px}}
.rec select.choisi{{border-color:var(--acc);outline:1px solid var(--acc)}}
.mini{{width:28px;height:28px;object-fit:contain;display:inline-grid;place-items:center}}
.pourquoi{{display:block;font-size:11.5px;margin-top:2px;padding:3px 6px;border-left:3px solid var(--warn);background:color-mix(in srgb,var(--warn) 10%,transparent)}}
tr.ko b{{color:var(--ko)}}span.ko{{color:var(--ko);font-weight:600}}span.ok{{color:var(--ok)}}.rec.vide{{color:var(--doux);font-style:italic}}
details{{margin-top:6px;font-size:12px}}summary{{cursor:pointer;color:var(--acc)}}.bonus{{margin:3px 0}}
footer{{position:fixed;left:0;right:0;bottom:0;background:var(--carte);border-top:1px solid var(--trait);padding:10px max(16px,env(safe-area-inset-left));display:flex;gap:10px;align-items:center;flex-wrap:wrap}}
footer textarea{{flex:1;min-width:0;height:44px;font:12px monospace;background:var(--bg);color:var(--fg);border:1px solid var(--trait);border-radius:6px;padding:6px}}
footer button{{background:var(--acc);color:#fff;border:0;border-radius:6px;padding:10px 14px;font-weight:600;cursor:pointer}}#nb{{font-weight:700}}
</style></head><body>
<header><h1>Panoplies d'équipement du 2.74 à importer</h1>
<p>{len(pan)} panoplies absentes de Padgref, par zone. Coche celles que tu veux, puis « Copier » en bas et colle le texte dans la conversation.</p>
<p>Sous chaque objet : sa recette. Une ressource en <b style="color:var(--ko)">rouge</b> est introuvable chez nous (monstre / donjon absent) :
choisis « retirer » ou un équivalent obtenable (même type de ressource, niveau le plus proche). Tu peux aussi changer une ressource verte.</p>
<p>Cadre en pointillés = les monstres de la zone n'existent pas encore chez nous (lot 5) : l'objet de butin serait en jeu mais introuvable.</p></header>
{"".join(blocs)}
<footer><span><span id="nb">0</span> cochée(s) · <span id="nr">0</span> ressource(s) changée(s)</span><textarea id="out" readonly>choix panoplies : </textarea><button type="button" id="copier">Copier</button></footer>
<script>
const out=document.getElementById('out'),nb=document.getElementById('nb'),nr=document.getElementById('nr');
function maj(){{const v=[...document.querySelectorAll('.tete input:checked')].map(i=>i.value);
const sel=[...document.querySelectorAll('.rec select')];sel.forEach(s=>s.classList.toggle('choisi',s.value!==''));
const r=sel.filter(s=>s.value!=='').map(s=>s.dataset.obj+':'+s.dataset.ing+'>'+s.value);
nb.textContent=v.length;nr.textContent=r.length;
out.value='choix panoplies : '+v.join(',')+(r.length?' | recettes : '+r.join(','):'');
try{{localStorage.setItem('choixPanoplies',JSON.stringify(v));localStorage.setItem('choixRecettes',JSON.stringify(r))}}catch(e){{}}}}
try{{(JSON.parse(localStorage.getItem('choixPanoplies'))||[]).forEach(id=>{{const i=document.querySelector('.tete input[value="'+id+'"]');if(i)i.checked=true}});
(JSON.parse(localStorage.getItem('choixRecettes'))||[]).forEach(x=>{{const m=/^(\\d+):(\\d+)>(\\d+)$/.exec(x);if(!m)return;
const s=document.querySelector('select[data-obj="'+m[1]+'"][data-ing="'+m[2]+'"]');if(s)s.value=m[3]}})}}catch(e){{}}
document.addEventListener('change',maj);
document.querySelectorAll('.tout').forEach(b=>b.onclick=()=>{{const c=[...document.querySelectorAll('#z'+b.dataset.z+' .tete input')];const t=c.some(i=>!i.checked);c.forEach(i=>i.checked=t);maj()}});
document.getElementById('copier').onclick=async()=>{{try{{await navigator.clipboard.writeText(out.value)}}catch(e){{out.select();document.execCommand('copy')}}}};
maj();
</script></body></html>'''
open(os.path.join(R, 'PLANCHE_PANOPLIES.html'), 'w', encoding='utf-8').write(page)
manquantes = {i for i in ingredients if i not in obtenables}
print('panoplies', len(pan), '| ressources', len(ingredients), 'dont introuvables', len(manquantes),
      '| sans équivalent', sum(1 for i in manquantes if not equivalents(i)), '| obtenables chez nous', len(obtenables), '| Ko', len(page) // 1024)
