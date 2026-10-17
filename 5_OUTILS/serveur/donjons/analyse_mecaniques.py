# -*- coding: utf-8 -*-
"""
MYSTIC : analyse (lecture seule) des mécaniques des monstres d'un donjon d'après les données du client 2.51 et la base.
Pour chaque monstre (nom français) : ses sorts (base world, grade le plus haut), leurs effets, déclencheurs, sous-sorts,
et si le serveur sait les appliquer (gestionnaire d'effet / déclencheur connu). Liste aussi les sorts du client dont le nom
contient un mot donné (sorts passifs non attribués).

Utilisation : python -I analyse_mecaniques.py "Kolosso,Professeur Xa,Blérice" [motcle1,motcle2]
"""
import os, re, sys, subprocess
sys.stdout.reconfigure(encoding='utf-8', errors='replace')
ICI = os.path.dirname(os.path.abspath(__file__)); RACINE = os.path.dirname(os.path.dirname(os.path.dirname(ICI)))
sys.path.insert(0, ICI)
import d2o, d2i
B = os.path.join(RACINE, 'CLIENT_64bits', 'app', 'data')
SRV = os.path.join(RACINE, 'SERVEUR_Emu', 'server', 'stump.server.worldserver')
MYSQL = os.path.join(RACINE, '_OUTILS', 'logiciels', 'MariaDB', 'bin', 'mysql.exe')
T = d2i.load(os.path.join(B, 'i18n', 'i18n_fr.d2i'))
_, S = d2o.load(os.path.join(B, 'common', 'Spells.d2o'))
_, L = d2o.load(os.path.join(B, 'common', 'SpellLevels.d2o'))
_, ST = d2o.load(os.path.join(B, 'common', 'SpellStates.d2o'))
_, MO = d2o.load(os.path.join(B, 'common', 'Monsters.d2o'))

en = {}
for l in open(os.path.join(RACINE, 'SERVEUR_Emu', 'DofusProtocol', 'Enums', 'Custom', 'EffectsEnum.cs'), encoding='utf-8-sig'):
    m = re.match(r'\s*(\w+)\s*=\s*(\d+)', l)
    if m:
        en.setdefault(int(m.group(2)), []).append(m.group(1))
H = set()
for r, _, fs in os.walk(os.path.join(SRV, 'game', 'effects')):
    for f in fs:
        if f.endswith('.cs'):
            H |= set(re.findall(r'EffectHandler\(EffectsEnum\.(\w+)', open(os.path.join(r, f), encoding='utf-8', errors='ignore').read()))
TRIG = set(re.findall(r'\{"(\w+)",\s*BuffTriggerType', open(os.path.join(SRV, 'game', 'fights', 'Buffs', 'TriggerBuff.cs'), encoding='utf-8-sig').read()))
TRIG |= {'I', 'T', 'P'}
CAST = {1160, 2160, 1017, 1175, 792, 793, 2792, 2794}
manques = set()


def trig_ok(t):
    return all(x in TRIG or re.match(r'^(EON?|Eon|EOFF|Eo)\d+$', x) for x in (t or 'I').split('|'))


def montrer(sid, ind, lvl=1, vus=None):
    vus = vus if vus is not None else set()
    if (sid, lvl) in vus:
        return
    vus.add((sid, lvl))
    s = S.get(sid)
    if not s:
        print(f'{ind}{sid} ?'); return
    lv = L.get(s['spellLevels'][min(lvl, len(s['spellLevels'])) - 1])
    if not lv:
        return
    req = ','.join(T.get((ST.get(x) or {}).get('nameId', 0), str(x)) for x in lv.get('statesRequired') or [])
    print(f"{ind}{sid} {T.get(s['nameId'])} (PA {lv['apCost']}, PO {lv['minRange']}-{lv['range']}, relance {lv['minCastInterval']}"
          + (f", états requis {req}" if req else '') + ')')
    for e in lv['effects']:
        nm = (en.get(e['effectId']) or ['?'])[0]
        ok = any(n in H for n in en.get(e['effectId'], []))
        tok = trig_ok(e['triggers'])
        if not ok:
            manques.add(f"effet {e['effectId']} {nm}")
        if not tok:
            manques.add(f"déclencheur {e['triggers']}")
        etat = ''
        if e['effectId'] in (950, 951):
            etat = ' [' + T.get((ST.get(e['value']) or {}).get('nameId', 0), str(e['value'])) + ']'
        print(f"{ind}   {e['effectId']} {nm} {e['diceNum']}/{e['diceSide']} cible {e['targetMask']} zone {e['rawZone']} durée {e['duration']} décl {e['triggers']}"
              f"{'' if ok else '  <<< EFFET NON GERE'}{'' if tok else '  <<< DECLENCHEUR INCONNU'}{etat}")
        if e['effectId'] in CAST and e['diceNum'] != sid:
            montrer(e['diceNum'], ind + '      ', e['diceSide'] or 1, vus)


def requete(sql):
    out = subprocess.run([MYSQL, '-uroot', '-h127.0.0.1', '-N', '-B', 'world', '-e', sql], capture_output=True).stdout.decode('utf-8')
    return [l.split('\t') for l in out.replace('\r', '').split('\n') if l]


noms = [n.strip() for n in sys.argv[1].split(',')]
mots = [m.strip() for m in sys.argv[2].split(',')] if len(sys.argv) > 2 else []
par_nom = {}
for mid, m in MO.items():
    par_nom.setdefault(T.get(m['nameId'], '').lower(), []).append(mid)
for n in noms:
    for mid in par_nom.get(n.lower(), []):
        sorts = requete(f"select distinct s.SpellId, s.Level from monsters_spells s join monsters_grades g on g.Id=s.MonsterGradeId "
                        f"where g.MonsterId={mid} and g.GradeId=(select max(GradeId) from monsters_grades where MonsterId={mid})")
        print(f'===== {n} (monstre {mid}) : {len(sorts)} sort(s) en base')
        for sid, lvl in sorts:
            montrer(int(sid), '  ', int(lvl))
for mot in mots:
    print(f'===== sorts du client contenant « {mot} »')
    for sid, s in S.items():
        if mot.lower() in T.get(s['nameId'], '').lower():
            print(f"  {sid} {T.get(s['nameId'])}")
print('\nA FAIRE COTE SERVEUR :', sorted(manques) or 'rien')
