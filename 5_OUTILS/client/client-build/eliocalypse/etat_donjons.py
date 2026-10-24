# PADGREF 23/10 (audit DPLN des 4 donjons de l'Eliocalypse) : état de NOS données par donjon -> audit/etat_<donjon>.md :
# monstres (base : grades, PV, PA, PM, résistances ; sorts de monsters_spells) et sorts (nom chez nous / nom 2.74, coût, portée,
# lancers par tour / par cible, relance, effets avec leur texte). Sorts : SpellLevels.d2o de la sortie Eliocalypse (= base, importée
# depuis ces fichiers), textes : i18n cumulé le plus récent.
import json, os, re, subprocess, sys
sys.stdout.reconfigure(encoding='utf-8')
ICI = os.path.dirname(os.path.abspath(__file__))
R = os.path.abspath(os.path.join(ICI, '..', '..', '..', '..'))
sys.path.insert(0, os.path.join(R, '_OUTILS', 'serveur', 'donjons'))
import d2o, d2i
MYSQL = os.path.join(R, '_OUTILS', 'logiciels', 'MariaDB', 'bin', 'mysql.exe')
C = os.path.join(ICI, 'client', 'data', 'common')
N = os.path.join(R, '_OUTILS', 'client', 'client_27x', 'data')
APP = os.path.join(R, 'CLIENT_64bits', 'app', 'data')
I18N = os.path.join(R, '_OUTILS', 'client', 'client-build', 'import274', 'client', 'data', 'i18n', 'i18n_fr.d2i')
TA, TN = d2i.load(I18N), d2i.load(os.path.join(N, 'i18n', 'i18n_fr.d2i'))
SP, SL = d2o.load(os.path.join(C, 'Spells.d2o'))[1], d2o.load(os.path.join(C, 'SpellLevels.d2o'))[1]
SPN = d2o.load(os.path.join(N, 'common', 'Spells.d2o'))[1]
EF = d2o.load(os.path.join(APP, 'common', 'Effects.d2o'))[1]
EFN = d2o.load(os.path.join(N, 'common', 'Effects.d2o'))[1]
ST = {**d2o.load(os.path.join(APP, 'common', 'SpellStates.d2o'))[1], **d2o.load(os.path.join(C, 'SpellStates.d2o'))[1]}
MN = d2o.load(os.path.join(C, 'Monsters.d2o'))[1]


def rq(s):
    out = subprocess.run([MYSQL, '-uroot', '-h127.0.0.1', '-N', '-B', '--default-character-set=utf8', 'world', '-e', s],
                         capture_output=True, check=True).stdout.decode('utf-8')
    return [l.split('\t') for l in out.replace('\r', '').split('\n') if l]


sys.path.insert(0, ICI)
src = open(os.path.join(ICI, 'acces_salles.py'), encoding='utf-8').read()
DONJONS = eval(re.search(r'DONJONS = (\[.*?\n\])', src, re.S).group(1))
dn = json.load(open(os.path.join(ICI, 'donnees.json'), encoding='utf-8'))


def texte_effet(e):
    ef = EF.get(e['effectId']) or EFN.get(e['effectId']) or {}
    t = TA.get(ef.get('descriptionId')) or TN.get(ef.get('descriptionId')) or f"effet {e['effectId']}"
    d1, d2, v = e.get('diceNum', 0), e.get('diceSide', 0), e.get('value', 0)
    t = re.sub(r'\{~1~2 à \}', ' à ' if d2 else '', t).replace('#1', str(d1)).replace('#2', str(d2) if d2 else '').replace('#3', str(v))
    if e['effectId'] in (950, 951, 952) and v in ST:
        t += f" [{TA.get(ST[v]['nameId']) or TN.get(ST[v]['nameId'])}]"
    if e['effectId'] in (792, 793, 1160, 2160, 1017, 1018, 1019) and d1 in SP:
        t += f" [sort {d1} {TA.get(SP[d1]['nameId'])}]"
    extra = []
    if e.get('duration'): extra.append(f"durée {e['duration']}")
    if e.get('triggers') and e['triggers'] != 'I': extra.append(f"déclencheur {e['triggers']}")
    if e.get('targetMask'): extra.append(f"cible {e['targetMask']}")
    if e.get('rawZone'): extra.append(f"zone {e['rawZone']}")
    return f"{t.strip()} ({', '.join(extra)})"


def sort_md(sid, niveau, vus, prof=0):
    if sid not in SP or (sid, niveau) in vus or prof > 2:
        return []
    vus.add((sid, niveau))
    s = SP[sid]
    lv = s['spellLevels'][min(niveau, len(s['spellLevels'])) - 1] if s['spellLevels'] else None
    L = SL.get(lv)
    nom, nom274 = TA.get(s['nameId']), TN.get(SPN[sid]['nameId']) if sid in SPN else None
    out = [f"{'  ' * prof}- **{nom}** (sort {sid}, niveau {niveau}{', NOM 2.74 : ' + nom274 if nom274 and nom274 != nom else ''})"]
    if L:
        out.append(f"{'  ' * prof}  {L['apCost']} PA, portée {L['minRange']}-{L['range']}, ligne {L['castInLine']}, vue {L['castTestLos']}, "
                   f"lancers/tour {L['maxCastPerTurn']}, lancers/cible {L['maxCastPerTarget']}, relance {L['minCastInterval']}, "
                   f"relance initiale {L['initialCooldown']}, critique {L['criticalHitProbability']}%"
                   + (f", états requis {L['statesRequired']}" if L.get('statesRequired') else '')
                   + (f", états interdits {L['statesForbidden']}" if L.get('statesForbidden') else ''))
        for e in L['effects']:
            out.append(f"{'  ' * prof}    - {texte_effet(e)}")
        for e in L['effects']:
            if e['effectId'] in (792, 793, 1160, 1017, 1018, 1019) and e.get('diceNum') in SP:
                out += sort_md(e['diceNum'], e.get('diceSide') or 1, vus, prof + 1)
    return out


os.makedirs(os.path.join(ICI, 'audit'), exist_ok=True)
for d in DONJONS:
    ids = d['monstres'] + d['special'] + [d['boss']]
    lignes = [f"# {d['nom']} — état de nos données ({__import__('datetime').date.today()})", ""]
    for m in ids:
        t = rq(f"SELECT Name, LookAsString FROM monsters_templates WHERE Id = {m}")
        nom = TA.get(MN[m]['nameId']) if m in MN else (t[0][0] if t else '?')
        lignes += [f"## {nom} (monstre {m})", f"look {t[0][1] if t else '?'}", ""]
        for g in rq(f"SELECT GradeId, Level, LifePoints, ActionPoints, MovementPoints, NeutralResistance, EarthResistance, FireResistance, "
                    f"WaterResistance, AirResistance FROM monsters_grades WHERE MonsterId = {m} ORDER BY GradeId"):
            lignes.append(f"- grade {g[0]} : niveau {g[1]}, {g[2]} PV, {g[3]} PA, {g[4]} PM, résistances neutre {g[5]} terre {g[6]} feu {g[7]} "
                          f"eau {g[8]} air {g[9]}")
        sorts = rq(f"SELECT DISTINCT s.SpellId, s.Level FROM monsters_spells s JOIN monsters_grades g ON g.Id = s.MonsterGradeId "
                   f"WHERE g.MonsterId = {m} AND g.GradeId = (SELECT MIN(GradeId) FROM monsters_grades WHERE MonsterId = {m})")
        lignes += ["", "### Sorts (grade le plus bas)"]
        vus = set()
        for sid, niv in sorts:
            lignes += sort_md(int(sid), int(niv), vus)
        if m in MN and MN[m].get('startingSpellId'):
            lignes.append(f"- sort de départ (passif) : niveau de sort {MN[m]['startingSpellId']}")
        lignes.append("")
    open(os.path.join(ICI, 'audit', f"etat_{re.sub(r'[^a-z]+', '_', d['nom'].lower())}.md"), 'w', encoding='utf-8').write('\n'.join(lignes))
    print(d['nom'], len(lignes), 'lignes')
