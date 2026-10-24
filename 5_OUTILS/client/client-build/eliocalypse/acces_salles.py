# PADGREF 23/10 (demande utilisateur : « PNJ à Astrub qui téléporte à l'Arche de Vili, portails vers les 4 zones, salles composées
# par toi avec les monstres de chaque donjon ») : Eliocalypse — ACCÈS, SALLES et APPARITIONS (SQL 99 + client).
#  - PNJ à nous : Passeur de l'Eliocalypse (Astrub, à côté du Chasseur de primes) -> Arche de Vili ; Passeur de l'Arche (Arche de
#    Vili) -> 4 zones des Cavaliers + retour Astrub ; le même passeur dans chaque zone (carte d'arrivée) -> retour à l'Arche ;
#  - gardiens OFFICIELS du 2.74 (Kapelache / Bree Caule / Rafflesia la Fétide / Miradar) devant chaque donjon : reprendre, donner la
#    clef, trousseau ; dans la salle de sortie : retour devant le donjon (Teleport + DungeonExit) ;
#  - 5 salles par donjon (4 + boss) : 4 monstres à 1-4 joueurs, +1 par joueur de 5 à 8 (comme nos autres donjons) ; après chaque
#    victoire, téléportation dans la salle suivante (boss -> salle de sortie) ;
#  - monstres de zone : les 4 monstres de chaque zone (sous-zones 925-928), grades 1 à 5.
# Client : Npcs.d2o + NpcMessages.d2o + i18n (textes cumulés) ; looks des gardiens -> looks_pnj.json (animations.py).
import json, os, subprocess, sys, zlib
sys.stdout.reconfigure(encoding='utf-8')
ICI = os.path.dirname(os.path.abspath(__file__))
CB = os.path.dirname(ICI)
R = os.path.abspath(os.path.join(CB, '..', '..', '..'))
sys.path.insert(0, CB); sys.path.insert(0, ICI)
sys.path.insert(0, os.path.join(R, '_OUTILS', 'serveur', 'donjons'))
from d2o_ecrire import D2O
import maps_elements as me, cellules
import d2o, d2i
APP = os.path.join(R, 'CLIENT_64bits', 'app')
RF = os.path.join(CB, 'recherches_frigost', 'client')
N = os.path.join(R, '_OUTILS', 'client', 'client_27x', 'data')
SORTIE = os.path.join(ICI, 'client', 'data')
SQLDIR = os.path.join(R, '_OUTILS', 'serveur', 'donjons', 'eliocalypse')
MYSQL = os.path.join(R, '_OUTILS', 'logiciels', 'MariaDB', 'bin', 'mysql.exe')
TEXTE0 = 9040000


def rq(s):
    out = subprocess.run([MYSQL, '-uroot', '-h127.0.0.1', '-N', '-B', '--default-character-set=utf8', 'world', '-e', s],
                         capture_output=True, check=True).stdout.decode('utf-8')
    return [l.split('\t') for l in out.replace('\r', '').split('\n') if l]


q = lambda s: "'" + str(s).replace('\\', '\\\\').replace("'", "''") + "'"
T = d2i.load(os.path.join(N, 'i18n', 'i18n_fr.d2i'))
NP74 = d2o.load(os.path.join(N, 'common', 'Npcs.d2o'))[1]
NM74 = d2o.load(os.path.join(N, 'common', 'NpcMessages.d2o'))[1]
ASTRUB, ARCHE = 191105026, 202899464
# donjon : (id, sous-zone, carte du gardien en zone, gardien 2.74, clef, salles dans l'ordre, salle de sortie, monstres, boss)
DONJONS = [
    dict(id=120, nom='Trône de Sang', zone=926, entree=201591042, gardien=5325, cle=22147, sortie=202379266, boss=6014,
         salles=[202375168, 202376192, 202377216, 202378240, 202379264], monstres=[6021, 6022, 6024, 6025], special=[6023]),
    dict(id=119, nom='Sentence de la Balance', zone=927, entree=201851392, gardien=5363, cle=22149, sortie=203427842, boss=5990,
         salles=[203423744, 203424768, 203425792, 203426816, 203427840], monstres=[5992, 5993, 5994, 5995], special=[5991]),
    dict(id=117, nom='Arbre de Mort', zone=928, entree=201328640, gardien=5367, cle=22148, sortie=202642432, boss=6026,
         salles=[202637312, 202638336, 202639360, 202640384, 202641408], monstres=[6028, 6029, 6030, 6031], special=[6027]),
    dict(id=118, nom='Fers de la Tyrannie', zone=925, entree=202115584, gardien=5319, cle=22028, sortie=203165698, boss=5955,
         salles=[203161600, 203162624, 203163648, 203164672, 203165696], monstres=[5978, 5980, 5983, 5989], special=[]),
]
ZONES = {926: 'Blessures de Guerre', 927: 'Désert de Misère', 928: 'Royaume Corrompu', 925: 'Galère de Servitude'}
PASSEUR_ASTRUB, PASSEUR_ARCHE = 10070, 10071
LOOK_PASSEUR = '{1001|||90}'

# ------------------------------------------------------------------ cellules des cartes (client converti)
carte_cells = {}
for f in [os.path.join(ICI, 'client', 'content', 'maps', 'maps11_2.d2p')] + [os.path.join(APP, 'content', 'maps', x) for x in os.listdir(os.path.join(APP, 'content', 'maps')) if x.endswith('.d2p')]:
    for nom, b in me.read_d2p(f):
        mid = int(os.path.basename(nom)[:-4])
        if mid in carte_cells or mid not in {ASTRUB, ARCHE} | {x for d in DONJONS for x in d['salles'] + [d['entree'], d['sortie']]} | set(ZONES.values() and []):
            continue
        carte_cells[mid] = cellules.lire(zlib.decompress(b))[1]


def praticables(mid):
    cs = carte_cells[mid]
    return [i for i, c in enumerate(cs) if c and (c['f'] & 1) == 0 and not (c['f'] & 4) and not (c['f'] & 0x80)]


def pres_du_centre(mid, n=1, eviter=()):
    def dist(i):
        r, c = divmod(i, 14); return abs(r - 19) * 0.5 + abs(c - 7)
    return [i for i in sorted(praticables(mid), key=dist) if i not in eviter][:n]


def rouges(mid):
    return [i for i, c in enumerate(carte_cells[mid]) if c and c['f'] & 32] or pres_du_centre(mid, 8)


# carte d'arrivée de chaque zone : la carte la plus proche de (0,0) avec assez de cases
MP = d2o.load(os.path.join(ICI, 'client', 'data', 'common', 'MapPositions.d2o'))[1]
SA = d2o.load(os.path.join(ICI, 'client', 'data', 'common', 'SubAreas.d2o'))[1]
arrivee = {}
for z in ZONES:
    maps = sorted((int(m) for m in SA[z]['mapIds']), key=lambda m: abs(MP[m]['posX']) + abs(MP[m]['posY']))
    for m in maps:
        for f in [os.path.join(ICI, 'client', 'content', 'maps', 'maps11_2.d2p')]:
            if m not in carte_cells:
                for nom, b in me.read_d2p(f):
                    if int(os.path.basename(nom)[:-4]) == m:
                        carte_cells[m] = cellules.lire(zlib.decompress(b))[1]
        if len(praticables(m)) > 100:
            arrivee[z] = m; break

# ------------------------------------------------------------------ client : PNJ + messages + textes
npcs = D2O(os.path.join(APP, 'data', 'common', 'Npcs.d2o'))
msgs = D2O(os.path.join(APP, 'data', 'common', 'NpcMessages.d2o'))
textes, langs, prochain = {}, [], [TEXTE0]
cache = {}


def texte(t, cle=None):
    if cle is not None and cle in cache:
        return cache[cle]
    i = prochain[0]; prochain[0] += 1
    textes[str(i)] = t; langs.append((i, t))
    if cle is not None:
        cache[cle] = i
    return i


modele_pnj, modele_msg = npcs.objet(10062), msgs.objet(60003)
gardiens = {}
for d in DONJONS:
    g = NP74[d['gardien']]
    dm = [[m, texte(T.get(t) or '', ('t', t))] for m, t in g['dialogMessages']]
    dr = [[r, texte(T.get(t) or '', ('t', t))] for r, t in g['dialogReplies']]
    look = g['look']
    npcs.poser(d['gardien'], dict(modele_pnj, id=d['gardien'], nameId=texte(T.get(g['nameId']), ('t', g['nameId'])), dialogMessages=dm,
                                  dialogReplies=dr, actions=[3], gender=g.get('gender', 0), look=look))
    for m, t in dm:
        p74 = NM74.get(m, {}).get('messageParams', [])
        msgs.poser(m, dict(modele_msg, id=m, messageId=t, messageParams=p74))
    txt = {m: T.get(t) or '' for m, t in g['dialogMessages']}
    rtx = {r: T.get(t) or '' for r, t in g['dialogReplies']}
    gardiens[d['id']] = dict(look=look, dm=dm, dr=dr, nom=T.get(g['nameId']),
                             intro=g['dialogMessages'][0][0],
                             retour=next(m for m, t in txt.items() if 'retourner' in t),
                             reprendre=next(r for r, t in rtx.items() if t.startswith('Reprendre')),
                             cle=next(r for r, t in rtx.items() if t.startswith('Donner la clef')),
                             trousseau=next(r for r, t in rtx.items() if 'trousseau' in t),
                             oui=[r for r, t in rtx.items() if t == 'Oui.'], non=[r for r, t in rtx.items() if t == 'Non.'])
# passeurs (nos PNJ) : messages 60020+, réponses 60030+
M_ASTRUB, M_ARCHE, M_ZONE = 60020, 60021, 60022
R_VERS_ARCHE, R_RETOUR_ASTRUB = 60030, 60031
R_ZONES = {z: 60032 + i for i, z in enumerate(ZONES)}
t_nom = texte("Passeur de l'Eliocalypse")
t_m_astrub = texte("Les quatre Cavaliers de l'Eliocalypse ont ouvert des failles vers leurs dimensions. Par l'Arche de Vili, tu pourras les "
                   "atteindre... si tu es prêt à affronter Guerre, Misère, Corruption et Servitude (niveau 200).")
t_m_arche = texte("Voici l'Arche de Vili. Chaque faille mène au domaine d'un Cavalier. Lequel veux-tu rejoindre ?")
t_m_zone = texte("Je peux te ramener à l'Arche de Vili quand tu le souhaites.")
t_r_arche = texte("Aller à l'Arche de Vili.")
t_r_astrub = texte("Retourner à Astrub.")
t_r_zone = {z: texte(f"Rejoindre {n}.") for z, n in ZONES.items()}
for pid, msgs_pnj, reps in ((PASSEUR_ASTRUB, [[M_ASTRUB, t_m_astrub]], [[R_VERS_ARCHE, t_r_arche]]),
                            (PASSEUR_ARCHE, [[M_ARCHE, t_m_arche], [M_ZONE, t_m_zone]],
                             [[R_ZONES[z], t_r_zone[z]] for z in ZONES] + [[R_RETOUR_ASTRUB, t_r_astrub], [R_VERS_ARCHE, t_r_arche]])):
    npcs.poser(pid, dict(modele_pnj, id=pid, nameId=t_nom, dialogMessages=msgs_pnj, dialogReplies=reps, actions=[3], gender=0, look=LOOK_PASSEUR))
    for m, t in msgs_pnj:
        msgs.poser(m, dict(modele_msg, id=m, messageId=t, messageParams=[]))
npcs.ecrire(os.path.join(SORTIE, 'common', 'Npcs.d2o'))
msgs.ecrire(os.path.join(SORTIE, 'common', 'NpcMessages.d2o'))
tout = json.load(open(os.path.join(ICI, 'textes_tout.json'), encoding='utf-8')); tout.update(textes)
json.dump(tout, open(os.path.join(ICI, 'textes_tout2.json'), 'w', encoding='utf-8'), ensure_ascii=False, indent=0)
src_i18n = os.path.join(RF, 'i18n_fr.d2i') if os.path.exists(os.path.join(RF, 'i18n_fr.d2i')) else os.path.join(APP, 'data', 'i18n', 'i18n_fr.d2i')
subprocess.run([sys.executable, '-I', os.path.join(CB, 'd2i_ajout.py'), src_i18n, os.path.join(SORTIE, 'i18n', 'i18n_fr.d2i'),
                os.path.join(ICI, 'textes_tout2.json')], check=True)
json.dump(sorted({g['look'] for g in gardiens.values()}), open(os.path.join(ICI, 'looks_pnj.json'), 'w'))

# ------------------------------------------------------------------ serveur
grade = {(int(m), int(g)): int(i) for i, m, g in rq("SELECT Id, MonsterId, GradeId FROM monsters_grades WHERE Id >= 970000")}
sql, ann = [], []
tous_pnj = [d['gardien'] for d in DONJONS] + [PASSEUR_ASTRUB, PASSEUR_ARCHE]
pl = ','.join(map(str, tous_pnj))
sql += [f"DELETE FROM npcs_templates WHERE Id IN ({pl});", f"DELETE FROM npcs_actions WHERE NpcId IN ({pl});",
        f"DELETE FROM npcs_spawns WHERE NpcId IN ({pl});"]
for d in DONJONS:
    g = gardiens[d['id']]
    sql.append("INSERT INTO npcs_templates (Id, Name, NameId, DialogMessagesIdCSV, DialogRepliesIdCSV, ActionsIdsCSV, Gender, LookAsString, SpecialArtworkId, TokenShop) "
               f"VALUES ({d['gardien']}, {q(g['nom'])}, {npcs.objet(d['gardien'])['nameId']}, {q(';'.join(f'{a},{b}' for a, b in g['dm']))}, "
               f"{q(';'.join(f'{a},{b}' for a, b in g['dr']))}, '3', 0, {q(g['look'])}, 0, 0);")
for pid in (PASSEUR_ASTRUB, PASSEUR_ARCHE):
    o = npcs.objet(pid)
    sql.append("INSERT INTO npcs_templates (Id, Name, NameId, DialogMessagesIdCSV, DialogRepliesIdCSV, ActionsIdsCSV, Gender, LookAsString, SpecialArtworkId, TokenShop) "
               f"VALUES ({pid}, \"Passeur de l'Eliocalypse\", {t_nom}, {q(';'.join(f'{a},{b}' for a, b in o['dialogMessages']))}, "
               f"{q(';'.join(f'{a},{b}' for a, b in o['dialogReplies']))}, '3', 0, {q(LOOK_PASSEUR)}, 0, 0);")
messages = sorted({m for g in gardiens.values() for m, _ in g['dm']} | {M_ASTRUB, M_ARCHE, M_ZONE})
ml = ','.join(map(str, messages))
sql += [f"DELETE FROM npcs_messages WHERE Id IN ({ml});", f"DELETE FROM npcs_replies WHERE MessageId IN ({ml});"]
sql.append("INSERT INTO npcs_messages (Id, MessageId, ParametersCSV) VALUES " + ", ".join(
    f"({m}, {msgs.objet(m)['messageId']}, '')" for m in messages) + ";")
spawn = lambda pid, mid, cell, nom: f"INSERT INTO npcs_spawns (NpcId, MapId, CellId, Direction, Npc) VALUES ({pid}, {mid}, {cell}, 1, {q(nom)});"
rep = lambda typ, r, m, *p: (f"INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0, Parameter1, Parameter2) VALUES ({q(typ)}, {r}, {m}, "
                             + ', '.join(q(x) if x is not None else 'NULL' for x in (list(p) + [None, None, None])[:3]) + ");")
talk = lambda pid, m, cond=None, prio=0: (f"INSERT INTO npcs_actions (NpcId, Type, `Condition`, Priority, Parameter0) VALUES ({pid}, 'Talk', "
                                          f"{q(cond) if cond else 'NULL'}, {prio}, '{m}');")
# passeur d'Astrub (près du Chasseur de primes, case 259) et passeurs de l'Arche / des zones
# Astrub : case libre à au moins 2 cases des PNJ et interactifs déjà posés ; retour devant le zaap (260, comme Oto Mustam)
occupees = {int(c) for (c,) in rq(f"SELECT CellId FROM npcs_spawns WHERE MapId = {ASTRUB} AND NpcId NOT IN ({PASSEUR_ASTRUB}, {PASSEUR_ARCHE}) "
                                   f"UNION SELECT CellId FROM interactives_spawns WHERE MapId = {ASTRUB}")}
proche = lambda a, b: abs(a // 14 - b // 14) <= 2 and abs(a % 14 - b % 14) <= 1
c_astrub = next(c for c in pres_du_centre(ASTRUB, 200) if not any(proche(c, o) for o in occupees | {246, 260}))
c_arche = pres_du_centre(ARCHE, 2)
sql += [spawn(PASSEUR_ASTRUB, ASTRUB, c_astrub, 'Padgref passeur Eliocalypse (Astrub)'), talk(PASSEUR_ASTRUB, M_ASTRUB),
        rep('Teleport', R_VERS_ARCHE, M_ASTRUB, ARCHE, c_arche[1], 1),
        spawn(PASSEUR_ARCHE, ARCHE, c_arche[0], "Padgref passeur Eliocalypse (Arche de Vili)"),
        talk(PASSEUR_ARCHE, M_ARCHE, f"Mp={ARCHE}", 1), talk(PASSEUR_ARCHE, M_ZONE, None, 0),
        rep('Teleport', R_RETOUR_ASTRUB, M_ARCHE, ASTRUB, 260, 1),
        rep('Teleport', R_VERS_ARCHE, M_ZONE, ARCHE, c_arche[1], 1)]
for z in ZONES:
    m = arrivee[z]
    cz = pres_du_centre(m, 2)
    sql += [rep('Teleport', R_ZONES[z], M_ARCHE, m, cz[1], 1), spawn(PASSEUR_ARCHE, m, cz[0], f"Padgref passeur Eliocalypse ({ZONES[z]})")]
# donjons : gardiens, entrées, sorties, salles
dl = ','.join(str(d['id']) for d in DONJONS)
salles_toutes = [s for d in DONJONS for s in d['salles']]
sql += [f"DELETE FROM dungeons_entries WHERE DungeonId IN ({dl});", f"DELETE FROM dungeons_exits WHERE DungeonId IN ({dl});",
        f"DELETE FROM monsters_spawns_dungeons_groups WHERE DungeonSpawnId BETWEEN 9700 AND 9799;",
        f"DELETE FROM monsters_spawns_dungeons WHERE Id BETWEEN 9700 AND 9799;"]
sid = 9700
for d in DONJONS:
    g = gardiens[d['id']]
    ck = pres_du_centre(d['entree'], 3)
    cs = pres_du_centre(d['sortie'], 3)
    r1 = rouges(d['salles'][0])
    sql += [spawn(d['gardien'], d['entree'], ck[0], f"Padgref gardien {d['nom']}"), talk(d['gardien'], g['intro']),
            spawn(d['gardien'], d['sortie'], cs[0], f"Padgref gardien {d['nom']} (sortie)"),
            talk(d['gardien'], g['retour'], f"Mp={d['sortie']}", -1),
            rep('DungeonResume', g['reprendre'], g['intro'], d['id']), rep('DungeonEnter', g['cle'], g['intro']),
            rep('DungeonEnterTrousseau', g['trousseau'], g['intro'])]
    for o in g['oui']:
        sql += [rep('Teleport', o, g['retour'], d['entree'], ck[1], 1), rep('DungeonExit', o, g['retour'])]
    sql.append(f"INSERT INTO dungeons_entries (DungeonId, EntranceMapId, KeyItemId, FirstRoomMapId, FirstRoomCellId, KeeperCellId) VALUES "
               f"({d['id']}, {d['entree']}, {d['cle']}, {d['salles'][0]}, {r1[0]}, {ck[0]});")
    sql.append(f"INSERT INTO dungeons_exits (DungeonId, EndMapId, ExitMapId, ExitCellId, ExitDirection) VALUES ({d['id']}, {d['sortie']}, {d['entree']}, {ck[1]}, 1);")
    sql.append(f"UPDATE dungeons SET EntranceMapId = {d['entree']}, ExitMapId = {d['entree']}, FirstRoom = {d['salles'][0]} WHERE Id = {d['id']};")
    mons, spec = d['monstres'], d['special']
    for i, salle in enumerate(d['salles']):
        suivante = d['salles'][i + 1] if i + 1 < len(d['salles']) else d['sortie']
        cell_suiv = rouges(suivante)[0] if i + 1 < len(d['salles']) else cs[1]
        boss = i == len(d['salles']) - 1
        base = ([d['boss']] + (spec + mons)[:3]) if boss else (mons[i % 4:] + mons[:i % 4])[:4 - (1 if spec and i >= 2 else 0)] + (spec if i >= 2 else [])
        extra = [(spec + mons)[(i + k) % len(spec + mons)] for k in range(4)]
        sql.append(f"INSERT INTO monsters_spawns_dungeons (Id, MapId, CellId, TeleportEvent, TeleportMapId, TeleportCell, TeleportDirection, Descripcion, "
                   f"RewardItemId, RewardMax) VALUES ({sid}, {salle}, NULL, 1, {suivante}, {cell_suiv}, 7, {q(d['nom'] + ' - Salle ' + str(i + 1))}, NULL, NULL);")
        # PADGREF 23/10 : système modulaire = chaque palier contient le groupe COMPLET (4 / 5 / 6 / 7 / 8 monstres), pas les ajouts seuls
        groupe = [(m, 1) for m in base] + [(m, 5 + k) for k in range(4) for m in base + extra[:k + 1]]
        for m, mini in groupe:
            gr = grade[(m, 1)] if m == d['boss'] else grade.get((m, 3), grade[(m, 1)])
            sql.append(f"INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations) VALUES ({sid}, {gr}, {mini}, NULL);")
        sid += 1
# monstres de zone
sql.append("DELETE FROM monsters_spawns WHERE Id BETWEEN 9700001 AND 9700099;")
mid = 9700001
for z in ZONES:
    for m in [x for d in DONJONS if d['zone'] == z for x in d['monstres']]:
        sql.append(f"INSERT INTO monsters_spawns (Id, MapId, SubAreaId, MonsterId, Frequency, MinGrade, MaxGrade, IsDisabled) VALUES ({mid}, NULL, {z}, {m}, 1.00, 1, 5, 0);")
        mid += 1
sql.append(f"DELETE FROM langs WHERE Id BETWEEN {TEXTE0} AND {TEXTE0 + 9999};")
sql.append("INSERT INTO langs (Id, French) VALUES " + ",\n ".join(f"({i}, {q(t)})" for i, t in langs) + ";")
tete = ["-- PADGREF 23/10/2026 : SQL 99 — Eliocalypse : accès (passeurs Astrub / Arche de Vili / zones), gardiens officiels, entrées / sorties,",
        "-- salles des 4 donjons (spawns 9700+), monstres de zone (9700001+). Généré par eliocalypse/acces_salles.py. Base world. Rejouable.",
        "SET NAMES utf8mb4;", "START TRANSACTION;"]
open(os.path.join(SQLDIR, '99_eliocalypse_acces.sql'), 'w', encoding='utf-8', newline='\n').write('\n'.join(tete + sql + ['COMMIT;']) + '\n')
open(os.path.join(SQLDIR, '99_eliocalypse_acces-annuler.sql'), 'w', encoding='utf-8', newline='\n').write('\n'.join([
    "-- PADGREF 23/10 : annule 99_eliocalypse_acces.sql", "START TRANSACTION;",
    f"DELETE FROM npcs_templates WHERE Id IN ({pl});", f"DELETE FROM npcs_actions WHERE NpcId IN ({pl});", f"DELETE FROM npcs_spawns WHERE NpcId IN ({pl});",
    f"DELETE FROM npcs_messages WHERE Id IN ({ml});", f"DELETE FROM npcs_replies WHERE MessageId IN ({ml});",
    f"DELETE FROM dungeons_entries WHERE DungeonId IN ({dl});", f"DELETE FROM dungeons_exits WHERE DungeonId IN ({dl});",
    "DELETE FROM monsters_spawns_dungeons_groups WHERE DungeonSpawnId BETWEEN 9700 AND 9799;", "DELETE FROM monsters_spawns_dungeons WHERE Id BETWEEN 9700 AND 9799;",
    "DELETE FROM monsters_spawns WHERE Id BETWEEN 9700001 AND 9700099;", f"DELETE FROM langs WHERE Id BETWEEN {TEXTE0} AND {TEXTE0 + 9999};", "COMMIT;"]) + '\n')
print('ok :', len(sql), 'lignes ; arrivées', arrivee, '; Astrub case', c_astrub, '; Arche', c_arche, '; looks', [g['look'] for g in gardiens.values()])
