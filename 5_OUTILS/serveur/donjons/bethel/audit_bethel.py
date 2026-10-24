# PADGREF 23/10 (demande utilisateur : « le boss Bethel ne fait pas tout, fais un audit de la mécanique du boss + du boss ») :
# parcourt l'arbre complet des sorts du combat de Bethel Akarna (passif Contrat Indicible 9542, sorts du boss, invocations Monolithe /
# Dagon / Dagon insoumis / Amas de tentacules, Padécédé, Aquazomb, glyphes) et vérifie pour chaque effet et chaque déclencheur que
# le serveur sait l'exécuter (gestionnaire d'effet [EffectHandler], déclencheur connu de TriggerBuff). Sortie : AUDIT_BETHEL.md.
import collections, glob, os, re, subprocess, sys
sys.stdout.reconfigure(encoding='utf-8')
ICI = os.path.dirname(os.path.abspath(__file__))
R = os.path.abspath(os.path.join(ICI, '..', '..', '..', '..'))
sys.path.insert(0, os.path.join(R, '_OUTILS', 'serveur', 'donjons'))
import d2o, d2i
A = os.path.join(R, 'CLIENT_64bits', 'app', 'data')
SRV = os.path.join(R, 'SERVEUR_Emu', 'server', 'stump.server.worldserver')
T = d2i.load(os.path.join(A, 'i18n', 'i18n_fr.d2i'))
SP = d2o.load(os.path.join(A, 'common', 'Spells.d2o'))[1]
SL = d2o.load(os.path.join(A, 'common', 'SpellLevels.d2o'))[1]
EF = d2o.load(os.path.join(A, 'common', 'Effects.d2o'))[1]
ST = d2o.load(os.path.join(A, 'common', 'SpellStates.d2o'))[1]
MYSQL = os.path.join(R, '_OUTILS', 'logiciels', 'MariaDB', 'bin', 'mysql.exe')
rq = lambda s: [l.split('\t') for l in subprocess.run([MYSQL, '-uroot', '-h127.0.0.1', '-N', '-B', '--default-character-set=utf8', 'world', '-e', s],
                                                        capture_output=True, check=True).stdout.decode().splitlines() if l]

# effets gérés par le serveur : nom EffectsEnum -> id
enum = {}
for l in open(glob.glob(os.path.join(R, 'SERVEUR_Emu', '**', 'EffectsEnum.cs'), recursive=True)[0], encoding='utf-8-sig'):
    m = re.match(r'\s*(Effect_\w+)\s*=\s*(\d+)', l)
    if m:
        enum[m.group(1)] = int(m.group(2))
geres = set()
for f in glob.glob(os.path.join(SRV, '**', '*.cs'), recursive=True):
    for n in re.findall(r'EffectHandler\(EffectsEnum\.(\w+)', open(f, encoding='utf-8', errors='ignore').read()):
        if n in enum:
            geres.add(enum[n])
src_tb = open(os.path.join(SRV, 'game', 'fights', 'Buffs', 'TriggerBuff.cs'), encoding='utf-8').read()
decl = {m.group(1): m.group(2) for m in re.finditer(r'\{"(\w+)", BuffTriggerType\.(\w+)\}', src_tb)}


def decl_ok(t):
    if t in decl:
        return decl[t] != 'Unknown'
    return bool(re.match(r'^(EON|Eon|EOFF|EO|Eo|TR)\d+$|^[TP]$', t))


nom_eff = lambda e: re.sub(r'\{[^}]*\}', '', T.get(EF.get(e, {}).get('descriptionId')) or f'effet {e}').replace('#1', 'x').replace('#2', 'y').replace('#3', 'z').strip()
SORT_LANCES = {792, 793, 1160, 1017, 1018, 1019, 2160, 2792, 2794, 1008}
INVOC = {181, 180, 1008, 1097, 2796, 405}

racines = [(9542, 'Contrat Indicible (passif de Bethel)')]
monstres = {5110: 'Bethel Akarna', 5121: 'Monolithe', 5122: 'Dagon des Profondeurs', 5123: 'Dagon insoumis', 5124: 'Amas de tentacules vaporeux',
            5112: 'Tournoyé'}
for m, n in monstres.items():
    for (s,) in rq(f"SELECT DISTINCT s.SpellId FROM monsters_spells s JOIN monsters_grades g ON g.Id = s.MonsterGradeId WHERE g.MonsterId = {m}"):
        racines.append((int(s), f'sort de {n}'))
racines += [(9559, 'passif du Monolithe'), (9566, 'passif du Dagon'), (9450, 'passif Aquazomb des Submergés'), (9349, 'Padécédé'), (9571, 'passif du Dagon insoumis'), (9574, 'passif des tentacules')]
vus, lignes, problemes = set(), [], collections.OrderedDict()
pile = list(racines)
while pile:
    s, origine = pile.pop(0)
    if s in vus or s not in SP:
        continue
    vus.add(s)
    lignes.append(f"\n### {s} — {T.get(SP[s]['nameId'])} ({origine})")
    for i, l in enumerate(SP[s]['spellLevels']):
        L = SL[l]
        lignes.append(f"- niveau {i + 1} : {L['apCost']} PA, portée {L['minRange']}-{L['range']}, états requis {L.get('statesRequired')} interdits {L.get('statesForbidden')}")
        for e in L['effects']:
            eid, trig = e['effectId'], e.get('triggers') or 'I'
            ok_e = eid in geres
            mauvais = [t for t in trig.split('|') if not decl_ok(t)]
            info = ''
            if eid in (950, 951, 952) and e.get('value') in ST:
                info = f" [état {e['value']} {T.get(ST[e['value']]['nameId'])}]"
            if eid in SORT_LANCES and e.get('diceNum') in SP:
                info = f" [sort {e['diceNum']} niv {e.get('diceSide')} {T.get(SP[e['diceNum']]['nameId'])}]"
                pile.append((e['diceNum'], f'lancé par {s}'))
            if eid in INVOC:
                info = f" [monstre {e.get('diceNum')} grade {e.get('diceSide')}]"
            drap = ('' if ok_e else ' **EFFET NON GÉRÉ**') + (f" **DÉCLENCHEUR INCONNU {mauvais}**" if mauvais else '')
            lignes.append(f"  - {eid} {nom_eff(eid)}{info} — cible {e.get('targetMask')}, zone {e.get('rawZone', '')[:40]}, durée {e.get('duration')}, "
                          f"déclencheur {trig}{drap}")
            if not ok_e:
                problemes.setdefault(f"effet {eid} « {nom_eff(eid)} » non géré", []).append(s)
            for t in mauvais:
                problemes.setdefault(f"déclencheur « {t} » inconnu", []).append(s)
tete = ["# Audit Bethel Akarna — mécanique du boss (serveur)", "",
        f"{len(vus)} sorts parcourus depuis le passif du boss, ses sorts et ses invocations.", "", "## Problèmes", ""]
tete += [f"- {k} : sorts {sorted(set(v))}" for k, v in problemes.items()] or ['- aucun']
open(os.path.join(ICI, 'AUDIT_BETHEL.md'), 'w', encoding='utf-8').write('\n'.join(tete + ['', '## Détail'] + lignes) + '\n')
print(len(vus), 'sorts')
for k, v in problemes.items():
    print('-', k, sorted(set(v)))
