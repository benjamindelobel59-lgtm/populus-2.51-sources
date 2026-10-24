# PADGREF 22/10 : Eliocalypse — inventaire du client 2.74 (zones d'accès, donjons, cartes, monstres, sorts, effets, états).
# Sortie : inventaire.json + résumé à l'écran. Lecture seule.
import json, os, sys, glob, re, collections
sys.stdout.reconfigure(encoding='utf-8')
ICI = os.path.dirname(os.path.abspath(__file__))
R = os.path.abspath(os.path.join(ICI, '..', '..', '..', '..'))
sys.path.insert(0, os.path.join(R, '_OUTILS', 'serveur', 'donjons'))
sys.path.insert(0, os.path.join(R, '_OUTILS', 'client', 'client-build'))
import d2o, d2i
import maps_elements as me
N = os.path.join(R, '_OUTILS', 'client', 'client_27x')
A = os.path.join(R, 'CLIENT_64bits', 'app')
T = d2i.load(os.path.join(N, 'data', 'i18n', 'i18n_fr.d2i'))
L = lambda b, f: d2o.load(os.path.join(b, 'data', 'common', f + '.d2o'))[1]
AR, SA, DG, MN, SP, SL, EF, ST, MP = (L(N, f) for f in ('Areas', 'SubAreas', 'Dungeons', 'Monsters', 'Spells', 'SpellLevels', 'Effects',
                                                        'SpellStates', 'MapPositions'))
SAa, MNa, SPa, SLa, EFa, STa, MPa = (L(A, f) for f in ('SubAreas', 'Monsters', 'Spells', 'SpellLevels', 'Effects', 'SpellStates', 'MapPositions'))
nom = lambda d, k: T.get(d[k]['nameId']) if k in d else None

# zones d'accès des 4 Cavaliers + Arche de Vili + sous-zones des 4 donjons (117 à 120)
cartes_dj = {int(m) for k in (117, 118, 119, 120) for m in DG[k]['mapIds']}
voulues = {925, 926, 927, 928, 934} | {k for k, v in SA.items() if cartes_dj & {int(m) for m in v.get('mapIds', [])}}
area = sorted({SA[k]['areaId'] for k in voulues})
sz = {k: SA[k] for k in sorted(voulues)}
for k, v in sz.items():
    print(f"  sous-zone {k} {T.get(v['nameId'])} | {len(v.get('mapIds', []))} cartes | niveau {v.get('level')} | monstres {len(v.get('monsters', []))} | "
          f"déjà 2.51 : {k in SAa}")
# faille de la Mine Hable / Arche de Vili
for k, v in SA.items():
    n = T.get(v['nameId']) or ''
    if 'Mine Hable' in n or 'Arche' in n or 'Vili' in n:
        print('  autre :', k, n, 'zone', T.get(AR[v['areaId']]['nameId']), len(v.get('mapIds', [])), 'cartes', '2.51' if k in SAa else 'NOUVELLE')
cartes = sorted({int(m) for v in sz.values() for m in v.get('mapIds', [])})
dlm = {int(os.path.basename(n)[:-4]): f for f in glob.glob(os.path.join(N, 'content', 'maps', '*.d2p')) for n, _ in me.read_d2p(f) if n.endswith('.dlm')}
print('cartes :', len(cartes), '| fichiers présents 2.74 :', sum(1 for m in cartes if m in dlm), '| déjà dans notre client :',
      sum(1 for m in cartes if m in MPa))
mons = sorted({m for v in sz.values() for m in v.get('monsters', [])})
for d in DG.values():
    if any(m in cartes for m in d.get('mapIds', [])):
        pass
print('monstres :', len(mons), '| déjà 2.51 :', sum(1 for m in mons if m in MNa))
for m in mons:
    v = MN[m]
    print(f"    {m} {T.get(v['nameId'])} boss={v.get('isBoss')} grades={[g['level'] for g in v['grades']]} sorts={len(v.get('spells', []))} "
          f"look={v.get('look', '')[:40]}")
sorts = sorted({s for m in mons for s in MN[m].get('spells', [])})
niveaux = [lv for s in sorts if s in SP for lv in SP[s].get('spellLevels', [])]
eff, etats, invocs, sorts_lies = collections.Counter(), set(), set(), set()
for lv in niveaux:
    for e in SL[lv].get('effects', []) + SL[lv].get('criticalEffect', []):
        eff[e['effectId']] += 1
        if e['effectId'] in (950, 951, 952):           # états
            etats.add(e.get('value'))
        if e['effectId'] in (181, 180, 1008, 1011, 2796, 405):  # invocations
            invocs.add(e.get('diceNum'))
        if e['effectId'] in (792, 793, 1160, 2160, 2792, 2793, 2794, 2795, 1017, 1018, 1019, 1160):  # sorts déclenchés
            sorts_lies.add(e.get('diceNum'))
print('sorts :', len(sorts), '| niveaux :', len(niveaux), '| sorts déjà 2.51 :', sum(1 for s in sorts if s in SPa))
print('effets inconnus 2.51 :', {e: (T.get(EF[e]['descriptionId']) or '')[:60] for e in eff if e not in EFa})
print('états utilisés :', len(etats), '| inconnus 2.51 :', sorted(e for e in etats if e not in STa))
print('invocations :', sorted((i, nom(MN, i)) for i in invocs if i))
print('sorts déclenchés :', len(sorts_lies), '| inconnus 2.51 :', sum(1 for s in sorts_lies if s not in SPa))
json.dump({'area': area, 'sousZones': list(sz), 'cartes': cartes, 'monstres': mons, 'sorts': sorts, 'etats': sorted(etats),
           'invocations': sorted(invocs), 'sortsLies': sorted(sorts_lies)},
          open(os.path.join(ICI, 'inventaire.json'), 'w', encoding='utf-8'), ensure_ascii=False, indent=0)
