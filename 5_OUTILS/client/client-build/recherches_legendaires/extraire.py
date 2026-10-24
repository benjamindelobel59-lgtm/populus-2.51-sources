# PADGREF 23/10 (demande utilisateur : « fais les 16 chasses légendaires manquantes ») : même extraction que recherches_frigost/extraire.py,
# pour les 16 chasses légendaires 2.74 restantes (recherchés hors Frigost). Contenu des chasses légendaires du client 2.74
# (_OUTILS/client/client_27x) : chasse, carte, fragments (+ monstres qui les lâchent), coffre (+ ressource ultra rare), panoplie
# fabriquée avec cette ressource (objets, effets, recettes). Sortie : contenu.json (lu par generer.py). Lecture seule.
import json, os, sys
sys.stdout.reconfigure(encoding='utf-8')
ICI = os.path.dirname(os.path.abspath(__file__))
RACINE = os.path.abspath(os.path.join(ICI, '..', '..', '..', '..'))
sys.path.insert(0, os.path.join(RACINE, '_OUTILS', 'serveur', 'donjons'))
import d2o, d2i
N = os.path.join(RACINE, '_OUTILS', 'client', 'client_27x', 'data')
A = os.path.join(RACINE, 'CLIENT_64bits', 'app', 'data')
T = d2i.load(os.path.join(N, 'i18n', 'i18n_fr.d2i'))
lire = lambda base, f: d2o.load(os.path.join(base, 'common', f + '.d2o'))[1]
IN, IA = lire(N, 'Items'), lire(A, 'Items')
MN, MA = lire(N, 'Monsters'), lire(A, 'Monsters')
LN = lire(N, 'LegendaryTreasureHunts')
SN = lire(N, 'ItemSets')
REC = lire(N, 'Recipes')
RD = lire(N, 'RandomDropGroups')
TY = lire(N, 'ItemTypes')
nom = lambda i: T.get(IN[i]['nameId']) if i in IN else None

# recherchés de Frigost : chasse légendaire 2.74 -> monstre (le 2.74 met monsterId = 0 : on prend le recherché du même nom)
RECHERCHES = {1000: 462, 1001: 554, 1002: 481, 1003: 4014, 1004: 552, 1005: 4814, 1006: 550, 1007: 4027, 1008: 4240, 1009: 4815,
              1011: 4041, 1013: 459, 1015: 4015, 1020: 4816, 1024: 4834, 1030: 446}
ROSE = 15263
recette = {r['resultId']: r for r in REC.values()}
utilise_dans = {}
for r in REC.values():
    for i in r['ingredientIds']:
        utilise_dans.setdefault(i, []).append(r['resultId'])
drop_par = {}
for mid, m in MN.items():
    for d in m.get('drops') or []:
        drop_par.setdefault(d['objectId'], []).append((mid, d))

sortie, manquants = [], set()
for lid, mid in RECHERCHES.items():
    l = LN[lid]
    coffre, carte = l['chestId'], l['mapItemId']
    groupe = next((e['value'] for e in IN[coffre]['possibleEffects'] if e['effectId'] == 222), None)
    contenu = RD[groupe]['randomDropItems'] if groupe in RD else []
    rare = [x for x in contenu if x['itemId'] != ROSE]
    rare_id = rare[0]['itemId'] if rare else None
    fragments = [i for i in recette[carte]['ingredientIds']] if carte in recette else []
    panoplie = None
    objets_pano = sorted({r for r in utilise_dans.get(rare_id, []) if IN[r].get('itemSetId', -1) > 0})
    if objets_pano:
        sid = IN[objets_pano[0]]['itemSetId']
        panoplie = {'id': sid, 'nom': T.get(SN[sid]['nameId']), 'objets': SN[sid]['items']}
    frag_drops = {}
    for f in fragments:
        frag_drops[f] = [{'monstre': m, 'nomMonstre': T.get(MN[m]['nameId']), 'dans251': m in MA,
                          'taux': [d.get(f'percentDropForGrade{g}', 0) for g in range(1, 6)], 'criteres': d.get('criteria')}
                         for m, d in drop_par.get(f, [])]
    nouveaux = [coffre, carte, rare_id] + fragments + (panoplie['objets'] if panoplie else [])
    ingr = set()
    for o in (panoplie['objets'] if panoplie else []) + [carte]:
        if o in recette:
            ingr |= set(recette[o]['ingredientIds'])
    for i in ingr - set(nouveaux):
        if i not in IA:
            manquants.add((i, nom(i)))
    sortie.append({'chasse': lid, 'nom': T.get(l['nameId']), 'niveau': l['level'], 'xpRatio': l['xpRatio'], 'monstre': mid,
                   'nomMonstre': T.get(MN[mid]['nameId']), 'monstreDans251': mid in MA,
                   'coffre': coffre, 'carte': carte, 'rare': rare_id, 'nomRare': nom(rare_id), 'groupe': contenu,
                   'fragments': fragments, 'dropsFragments': frag_drops, 'panoplie': panoplie,
                   'dejaDans251': [i for i in nouveaux if i in IA]})
json.dump(sortie, open(os.path.join(ICI, 'contenu.json'), 'w', encoding='utf-8'), ensure_ascii=False, indent=1)
for s in sortie:
    p = s['panoplie']
    print(f"{s['chasse']} {s['nom']} niv {s['niveau']} | monstre {s['monstre']} {s['nomMonstre']} (2.51 {s['monstreDans251']}) | "
          f"rare {s['rare']} {s['nomRare']} | {len(s['fragments'])} fragments | panoplie {p['id'] if p else None} {p['nom'] if p else ''} "
          f"{len(p['objets']) if p else 0} obj | déjà 2.51 : {s['dejaDans251']}")
    for f, ds in s['dropsFragments'].items():
        print('     ', f, nom(f), '<-', [(d['nomMonstre'], d['taux'][0], d['dans251'], d['criteres']) for d in ds])
print('ingrédients absents du 2.51 :', sorted(manquants))
