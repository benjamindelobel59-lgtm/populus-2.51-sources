# PADGREF 23/10 (demande utilisateur) : Eliocalypse — import des DONNÉES du client 2.74 dans notre client 2.51 :
#   MapPositions, SubAreas, Areas, Dungeons, Monsters (+ invocations), MonsterRaces, Spells, SpellLevels (+ sorts déclenchés),
#   SpellStates, textes ; liste des animations (os, skins, effets de sorts) à copier.
# Conversion générique : chaque objet 2.74 est réécrit avec la classe 2.51 du même nom (champs communs recopiés, champs 2.51
# absents du 2.74 à leur valeur vide) + conversions particulières :
#   - Spell : scriptId / scriptParams (+ critique) = typeId / rawParams du 1er script lié (SpellScripts.d2o), default_zone,
#     verbose_cast ;
#   - SpellLevel : statesRequired / statesForbidden depuis statesCriterion (« E123 » / « !E123 », 2.74) ;
#   - SubArea : bounds calculées depuis les cartes ; Monster : gfxId = os du look.
# Textes : nouveaux identifiants à partir de TEXTE0. Sorties : client/data/common/*.d2o, client/data/i18n/i18n_fr.d2i, donnees.json
import json, os, re, subprocess, sys
sys.stdout.reconfigure(encoding='utf-8')
ICI = os.path.dirname(os.path.abspath(__file__))
CB = os.path.dirname(ICI)
R = os.path.abspath(os.path.join(CB, '..', '..', '..'))
sys.path.insert(0, CB)
from d2o_ecrire import D2O
sys.path.insert(0, ICI)
from renomme import RENOMME
sys.path.insert(0, os.path.join(R, '_OUTILS', 'serveur', 'donjons'))
import d2i

APP = os.path.join(R, 'CLIENT_64bits', 'app', 'data')
N = os.path.join(R, '_OUTILS', 'client', 'client_27x', 'data')
SORTIE = os.path.join(ICI, 'client', 'data')
os.makedirs(os.path.join(SORTIE, 'common'), exist_ok=True); os.makedirs(os.path.join(SORTIE, 'i18n'), exist_ok=True)
TEXTE0 = 9020000
inv = json.load(open(os.path.join(R, '_OUTILS', 'serveur', 'donjons', 'eliocalypse', 'inventaire.json'), encoding='utf-8'))
T = d2i.load(os.path.join(N, 'i18n', 'i18n_fr.d2i'))

FICHIERS = ['MapPositions', 'SubAreas', 'Areas', 'Dungeons', 'Monsters', 'MonsterRaces', 'MonsterSuperRaces', 'Spells', 'SpellLevels',
            'SpellStates', 'Effects', 'SpellScripts', 'SuperAreas', 'WorldMaps']
src = {f: D2O(os.path.join(N, 'common', f + '.d2o')) for f in FICHIERS}
dst = {f: D2O(os.path.join(APP, 'common', f + '.d2o')) for f in FICHIERS if os.path.exists(os.path.join(APP, 'common', f + '.d2o'))}
# Monsters.d2o : partir de la version du bestiaire (drops DofusDB, paquet recherches-frigost) pour ne pas l'écraser
_best = os.path.join(CB, 'bestiaire_drops', 'Monsters.d2o')
if os.path.exists(_best):
    dst['Monsters'] = D2O(_best)
VIDE = {-1: 0, -2: False, -3: '', -4: 0.0, -5: 0, -6: 0, -99: None}

# ------------------------------------------------------------------ textes
textes = {}
prochain = [TEXTE0]
cache_txt = {}


def texte(i):
    if not i:
        return 0
    if i in cache_txt:
        return cache_txt[i]
    t = T.get(i)
    if t is None:
        return 0
    n = prochain[0]; prochain[0] += 1
    textes[str(n)] = t
    cache_txt[i] = n
    return n


CHAMPS_TEXTE = {'nameId', 'descriptionId', 'shortNameId'}


def convertir(fichier, o):
    """objet du d2o 2.74 (src[fichier]) -> objet au format du d2o 2.51 (dst[fichier]), même nom de classe."""
    if o is None or not isinstance(o, dict):
        return o
    s, d = src[fichier], dst[fichier]
    nom = s.classes[o['__classe']][0]
    cid = d.classe(nom)
    res = {'__classe': cid}
    for champ, types in d.classes[cid][1]:
        if champ in o:
            v = o[champ]
            res[champ] = conv_val(fichier, types, v)
        else:
            res[champ] = [] if types[0] == -99 else VIDE.get(types[0])
        if champ in CHAMPS_TEXTE and isinstance(res[champ], int) and res[champ]:
            res[champ] = texte(res[champ])
    return res


def conv_val(fichier, types, v):
    if types[0] == -99:
        return [conv_val(fichier, types[1:], x) for x in (v or [])]
    if isinstance(v, dict):
        return convertir(fichier, v)
    return v


# ------------------------------------------------------------------ ensembles à importer
LS = lambda f: {k: src[f].objet(k) for k in src[f].index}
SP, SL, MN, EF, SS, ST = (LS(f) for f in ('Spells', 'SpellLevels', 'Monsters', 'Effects', 'SpellScripts', 'SpellStates'))
desc = {k: (T.get(v.get('descriptionId')) or '').lower() for k, v in EF.items()}
EFFETS_SORT = {k for k, t in desc.items() if t.strip() == '#1' or any(m in t for m in ('sort', 'glyphe', 'piège', 'rune', 'portail', 'mur'))}
EFFETS_INVOC = {k for k, t in desc.items() if 'invoque' in t}
monstres, sorts, etats = set(inv['monstres']), set(), set()
a_voir = list(monstres)
while a_voir:
    m = a_voir.pop()
    # sort de départ du grade (2.74 : startingSpellId = un NIVEAU de sort, lancé au début du combat : Arsenal de Guerre…)
    depart = {SL[g['startingSpellId']]['spellId'] for g in MN[m].get('grades', []) if g.get('startingSpellId', 0) in SL}
    for s in list(MN[m].get('spells', [])) + sorted(depart):
        pile = [s]
        while pile:
            s2 = pile.pop()
            if s2 in sorts or s2 not in SP:
                continue
            sorts.add(s2)
            for lv in SP[s2].get('spellLevels', []):
                l = SL.get(lv)
                if not l:
                    continue
                for e in l.get('effects', []) + l.get('criticalEffect', []):
                    if e['effectId'] in EFFETS_SORT:
                        for x in (e.get('diceNum'), e.get('value')):
                            if x in SP and x > 0:
                                pile.append(x)
                    if e['effectId'] in EFFETS_INVOC and e.get('diceNum') in MN and e['diceNum'] not in monstres:
                        monstres.add(e['diceNum']); a_voir.append(e['diceNum'])
                    if e['effectId'] in (950, 951, 952) and e.get('value') in ST:
                        etats.add(e['value'])
                for x in re.findall(r'(?:HS|E)[!=<>]?(\d+)', l.get('statesCriterion') or ''):
                    if int(x) in ST:
                        etats.add(int(x))
niveaux = sorted({lv for s in sorts for lv in SP[s].get('spellLevels', []) if lv in SL})
races = {MN[m]['race'] for m in monstres}
RC = LS('MonsterRaces')
superraces = {RC[r]['superRaceId'] for r in races if r in RC}
cartes = set(inv['cartes'])
sz_ids = set()
for k in src['SubAreas'].index:
    o = src['SubAreas'].objet(k)
    if cartes & {int(x) for x in o.get('mapIds', [])}:
        sz_ids.add(k)
areas = {src['SubAreas'].objet(k)['areaId'] for k in sz_ids}
superareas = {src['Areas'].objet(a)['superAreaId'] for a in areas}
DONJONS = [117, 118, 119, 120]
print(f"monstres {len(monstres)} (dont invocations {len(monstres) - len(inv['monstres'])}), sorts {len(sorts)}, niveaux {len(niveaux)}, "
      f"états {len(etats)}, races {len(races)}, super-races {superraces}, sous-zones {len(sz_ids)}, zones {areas}, super-zones {superareas}")


def deja(f, k):
    return dst[f].objet(k) is not None


PROTEGES = {'Spells', 'SpellLevels', 'Monsters', 'MapPositions', 'SubAreas', 'Dungeons'}


def poser(f, k, o):
    # jamais écraser un objet 2.51 existant (ex. sort générique 999 « Radical », utilisé aussi par le 2.74)
    if f in PROTEGES and dst[f].index.get(k) is not None:
        return
    dst[f].poser(k, o)


# états (seulement les nouveaux) ; races / super-zones si absentes
for e in sorted(etats):
    if not deja('SpellStates', e):
        poser('SpellStates', e, convertir('SpellStates', ST[e]))
for r in sorted(races):
    if not deja('MonsterRaces', r):
        poser('MonsterRaces', r, convertir('MonsterRaces', RC[r]))
for r in sorted(superraces):
    if 'MonsterSuperRaces' in dst and not deja('MonsterSuperRaces', r):
        poser('MonsterSuperRaces', r, convertir('MonsterSuperRaces', src['MonsterSuperRaces'].objet(r)))
for a in sorted(superareas):
    if 'SuperAreas' in dst and not deja('SuperAreas', a):
        poser('SuperAreas', a, convertir('SuperAreas', src['SuperAreas'].objet(a)))
for a in sorted(areas):
    if not deja('Areas', a):
        poser('Areas', a, convertir('Areas', src['Areas'].objet(a)))
# cartes
MP = src['MapPositions']
for m in sorted(cartes):
    o = MP.objet(m)
    if o is None:
        continue
    poser('MapPositions', m, convertir('MapPositions', o))
# sous-zones (bounds depuis les cartes)
for k in sorted(sz_ids):
    o = src['SubAreas'].objet(k)
    c = convertir('SubAreas', o)
    xs = [MP.objet(int(m))['posX'] for m in o['mapIds'] if MP.objet(int(m))]
    ys = [MP.objet(int(m))['posY'] for m in o['mapIds'] if MP.objet(int(m))]
    if 'bounds' in c and xs:
        b = dst['SubAreas'].objet(1)['bounds']
        c['bounds'] = dict(b, x=min(xs), y=min(ys), width=max(xs) - min(xs) + 1, height=max(ys) - min(ys) + 1)
    c['exploreAchievementId'] = -1
    poser('SubAreas', k, c)
# donjons
for k in DONJONS:
    poser('Dungeons', k, convertir('Dungeons', src['Dungeons'].objet(k)))
# monstres
for m in sorted(monstres):
    c = convertir('Monsters', MN[m])
    os_ = re.match(r'\{(\d+)', MN[m].get('look') or '')
    if os_ and int(os_.group(1)) in RENOMME:              # os + skin renumérotés (voir renomme.py)
        c['look'] = c['look'].replace('{' + os_.group(1), '{' + str(RENOMME[int(os_.group(1))]), 1)
        os_ = re.match(r'\{(\d+)', c['look'])
    if 'gfxId' in c:
        c['gfxId'] = int(os_.group(1)) if os_ else 0
    poser('Monsters', m, c)
# sorts
scripts_gfx = set()
for s in sorted(sorts):
    o = SP[s]
    c = convertir('Spells', o)
    for cle, src_cle in (('scriptId', 'boundScriptUsageData'), ('scriptIdCritical', 'criticalHitBoundScriptUsageData')):
        us = o.get(src_cle) or []
        sc = SS.get(us[0]['scriptId']) if us else None
        c[cle] = sc['typeId'] if sc else 0
        c['scriptParams' if cle == 'scriptId' else 'scriptParamsCritical'] = sc['rawParams'] if sc else ''
        for u in us:
            sc2 = SS.get(u['scriptId'])
            if sc2:
                scripts_gfx |= {int(x) for x in re.findall(r'[Gg]fxId\d?:(\d+)', sc2['rawParams'])}
    if 'default_zone' in c:
        c['default_zone'] = o.get('defaultPreviewZone') or ''
    if 'verbose_cast' in c:
        c['verbose_cast'] = bool(o.get('verboseCast'))
    poser('Spells', s, c)
for lv in niveaux:
    o = SL[lv]
    c = convertir('SpellLevels', o)
    crit = o.get('statesCriterion') or ''
    if 'statesRequired' in c:
        c['statesRequired'] = [int(x) for x in re.findall(r'(?:HS|E)=(\d+)', crit)]  # PADGREF 23/10 : le 2.74 écrit « HS=… » / « HS!… »
        c['statesForbidden'] = [int(x) for x in re.findall(r'!E(\d+)', crit)] + [int(x) for x in re.findall(r'(?:HS|E)!(\d+)', crit)]
    poser('SpellLevels', lv, c)

# cartes du monde des nouvelles cartes (24-27 : Dimensions des Cavaliers…) : sans elles l'interface de carte n'a rien à afficher
for w in sorted({MP.objet(m)['worldMap'] for m in cartes if MP.objet(m)} - {-1}):
    if dst['WorldMaps'].objet(w) is None and src['WorldMaps'].objet(w) is not None:
        poser('WorldMaps', w, convertir('WorldMaps', src['WorldMaps'].objet(w)))

for f, d in dst.items():
    if d.nouveaux:
        d.ecrire(os.path.join(SORTIE, 'common', f + '.d2o'))
json.dump(textes, open(os.path.join(ICI, 'textes.json'), 'w', encoding='utf-8'), ensure_ascii=False, indent=0)
subprocess.run([sys.executable, '-I', os.path.join(CB, 'd2i_ajout.py'), os.path.join(APP, 'i18n', 'i18n_fr.d2i'),
                os.path.join(SORTIE, 'i18n', 'i18n_fr.d2i'), os.path.join(ICI, 'textes.json')], check=True)
looks = sorted({dst['Monsters'].objet(m)['look'] for m in monstres if dst['Monsters'].objet(m) and dst['Monsters'].objet(m).get('look')})
json.dump({'monstres': sorted(monstres), 'sorts': sorted(sorts), 'niveaux': niveaux, 'etats': sorted(etats), 'sousZones': sorted(sz_ids),
           'zones': sorted(areas), 'looks': looks, 'gfxSorts': sorted(scripts_gfx), 'textes': len(textes),
           'ecrits': {f: len(d.nouveaux) for f, d in dst.items() if d.nouveaux}},
          open(os.path.join(ICI, 'donnees.json'), 'w', encoding='utf-8'), ensure_ascii=False, indent=0)
print('écrits :', {f: len(d.nouveaux) for f, d in dst.items() if d.nouveaux}, '| textes', len(textes), '| gfx de sorts', len(scripts_gfx),
      '| looks', len(looks))
