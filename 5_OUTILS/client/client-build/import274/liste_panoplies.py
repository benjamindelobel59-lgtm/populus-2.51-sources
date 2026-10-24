# PADGREF 23/10 (demande utilisateur : « fais-moi un listing des objets par zone avant ») : panoplies du client 2.74 absentes de notre
# client, rangées par ZONE d'obtention : zone des monstres qui font tomber leurs objets (dropMonsterIds -> sous-zones -> zone),
# sinon « Artisanat » (recette sans monstre), sinon « Autre (événement, boutique, quête…) ». Pour chaque panoplie : niveau,
# objets (type, niveau, déjà chez nous ou non, apparence connue ou non dans le client 2.74), comment l'obtenir.
# Sortie : LISTE_PANOPLIES_274.md (racine du projet) + liste_panoplies.json.
import collections, json, os, sys
sys.stdout.reconfigure(encoding='utf-8')
ICI = os.path.dirname(os.path.abspath(__file__))
R = os.path.abspath(os.path.join(ICI, '..', '..', '..', '..'))
sys.path.insert(0, os.path.join(R, '_OUTILS', 'serveur', 'donjons'))
import d2o, d2i
N = os.path.join(R, '_OUTILS', 'client', 'client_27x', 'data')
A = os.path.join(R, 'CLIENT_64bits', 'app', 'data')
T = d2i.load(os.path.join(N, 'i18n', 'i18n_fr.d2i'))
L = lambda f: d2o.load(os.path.join(N, 'common', f + '.d2o'))[1]
IN, SN, MN, SA, AR, TY, RE = L('Items'), L('ItemSets'), L('Monsters'), L('SubAreas'), L('Areas'), L('ItemTypes'), L('Recipes')
IA = d2o.load(os.path.join(A, 'common', 'Items.d2o'))[1]
SA_ = d2o.load(os.path.join(A, 'common', 'ItemSets.d2o'))[1]
recette = {r['resultId']: r for r in RE.values()}
MA = d2o.load(os.path.join(A, 'common', 'Monsters.d2o'))[1]
COS = ('apparat', 'Harnachement', 'Objet vivant', 'Costume', 'Ailes', 'paulières')
nom = lambda o: T.get(o.get('nameId')) or '?'

pan = []
for sid, s in sorted(SN.items()):
    if sid in SA_:
        continue
    objets = [IN[i] for i in s['items'] if i in IN]
    if not objets:
        continue
    if all(any(k in nom(TY.get(o['typeId'], {})) for k in COS) for o in objets):
        continue                                   # panoplie d'apparat : déjà traitée à part (lot 2 / apparats en attente)
    zones = collections.Counter()
    monstres, absents = set(), set()
    sources = [(o, 1) for o in objets]
    if not any(o.get('dropMonsterIds') for o in objets):          # objets fabriqués : zone des monstres qui donnent les ressources
        sources = [(IN[i], 1) for o in objets if o['id'] in recette for i in recette[o['id']]['ingredientIds'] if i in IN]
    for o, _ in sources:
        for m in o.get('dropMonsterIds') or []:
            if m in MN and m not in MA:
                absents.add(nom(MN[m]))
            if m in MN:
                monstres.add(nom(MN[m]))
                for sa in MN[m].get('subareas') or []:
                    if sa in SA and SA[sa].get('areaId') in AR:
                        zones[nom(AR[SA[sa]['areaId']])] += 1
    if zones:
        zone = zones.most_common(1)[0][0]
        comment = ('recette, ressources de : ' if sources and sources[0][0] not in objets else 'butin : ') + ', '.join(sorted(monstres)[:6]) + (' …' if len(monstres) > 6 else '')
        if absents:
            comment += f' — **{len(absents)} monstre(s) absent(s) chez nous**'
    elif any(o['id'] in recette for o in objets):
        zone, comment = 'Artisanat (ressources sans monstre)', 'recette'
    else:
        zone, comment = 'Autre (événement, boutique, quête…)', '?'
    types = collections.Counter(nom(TY.get(o['typeId'], {})) for o in objets)
    pan.append({'id': sid, 'nom': nom(s), 'niveau': max(o['level'] for o in objets), 'zone': zone, 'comment': comment,
                'objets': [{'id': o['id'], 'nom': nom(o), 'type': nom(TY.get(o['typeId'], {})), 'niveau': o['level'], 'chez_nous': o['id'] in IA,
                            'recette': o['id'] in recette} for o in objets],
                'types': dict(types)})
par_zone = collections.defaultdict(list)
for p in pan:
    par_zone[p['zone']].append(p)
json.dump(pan, open(os.path.join(ICI, 'liste_panoplies.json'), 'w', encoding='utf-8'), ensure_ascii=False, indent=0)
lignes = ["# Panoplies d'équipement du client 2.74 absentes de Padgref — par zone", '',
          f'{len(pan)} panoplies, {sum(len(p["objets"]) for p in pan)} objets. Zone = celle des monstres qui font tomber les objets '
          '(sinon « Artisanat » ou « Autre »). « déjà » = objet déjà dans notre jeu.', '',
          '| Zone | Panoplies | Objets | Niveaux |', '|---|---:|---:|---|']
ordre = sorted(par_zone, key=lambda z: (z.startswith(('Artisanat', 'Autre')), -len(par_zone[z])))
for z in ordre:
    ps = par_zone[z]
    lignes.append(f'| {z} | {len(ps)} | {sum(len(p["objets"]) for p in ps)} | {min(p["niveau"] for p in ps)}-{max(p["niveau"] for p in ps)} |')
for z in ordre:
    lignes += ['', f'## {z} ({len(par_zone[z])} panoplies)', '']
    for p in sorted(par_zone[z], key=lambda p: p['niveau']):
        objs = ', '.join(f'{o["nom"]}{" (déjà)" if o["chez_nous"] else ""}' for o in p['objets'])
        lignes.append(f'- **{p["nom"]}** — niv. {p["niveau"]} — {len(p["objets"])} objets ({", ".join(f"{n} {t}" for t, n in p["types"].items())}) — '
                      f'{p["comment"]}  \n  {objs}')
open(os.path.join(R, 'LISTE_PANOPLIES_274.md'), 'w', encoding='utf-8').write('\n'.join(lignes) + '\n')
print(len(pan), 'panoplies')
for z in ordre:
    print(f'{len(par_zone[z]):4d}  {z}')
