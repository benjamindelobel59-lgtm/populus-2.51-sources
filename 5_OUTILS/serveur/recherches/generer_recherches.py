# -*- coding: utf-8 -*-
"""
MYSTIC : système des recherchés (avis de recherche) sans quête.
 - 1 recherché maximum par zone (sous-zone de ses apparitions d'origine), réapparition 4 à 6 h après sa mort
   (géré par le serveur : game/Recherches/RecherchesManager.cs, table nekfeu_recherches) ;
 - un succès par recherché (catégorie « Avis de recherche ») : gagné par chaque personnage du combat gagnant
   qui a le niveau requis ; récompense = les jetons de dofuspourlesnoobs (avitons = Doplons, Kamas de glace, Alitons).

Entrée : avis_dofuspourlesnoobs.tsv (nom, niveau minimum, jetons).
Sorties : recherches.sql (base world) et, avec --client, les 4 fichiers de succès des clients (sauvegardés avant).

Utilisation :  python -I generer_recherches.py            → rapport + recherches.sql
               python -I generer_recherches.py --client   → + Achievements*.d2o des clients 64 et 32 bits
"""
import csv, os, re, shutil, struct, subprocess, sys, unicodedata, datetime
sys.stdout.reconfigure(encoding='utf-8', errors='replace')

ICI = os.path.dirname(os.path.abspath(__file__))
RACINE = os.path.dirname(os.path.dirname(os.path.dirname(ICI)))
sys.path.insert(0, os.path.join(RACINE, '_OUTILS', 'serveur', 'donjons'))
import d2o, d2i

MYSQL = os.path.join(RACINE, '_OUTILS', 'logiciels', 'MariaDB', 'bin', 'mysql.exe')
CLIENTS = [os.path.join(RACINE, 'CLIENT_64bits', 'app'), os.path.join(RACINE, 'CLIENT_32bits', 'app')]
RACES = (32, 90, 127, 147, 156)          # races « Recherché » (Amakna, Frigost, dimensions, alignement, Sufokia)
DOPLON, KAMA_GLACE, ALITON = 13052, 11196, 17019
CATEGORIE = 42                            # « Avis de recherche »
ID_SUCCES, ID_OBJECTIF, ID_RECOMPENSE = 6001, 20001, 20001
ICONE = 225                               # icône des succès officiels de capture
POINTS = 10


def requete(sql):
    out = subprocess.run([MYSQL, '-uroot', '-h127.0.0.1', '--default-character-set=utf8mb4', '-N', '-B', 'world', '-e', sql],
                         capture_output=True, check=True).stdout.decode('utf-8')
    return [l.split('\t') for l in out.splitlines() if l]


def norm(s):
    s = s.lower().replace('œ', 'oe').replace('æ', 'ae')
    s = ''.join(c for c in unicodedata.normalize('NFD', s) if unicodedata.category(c) != 'Mn')
    s = re.sub(r"[^a-z0-9 ]", ' ', s)
    s = re.sub(r"^(le|la|les|l) ", '', s.strip())
    return re.sub(r' +', ' ', s).strip()


def main():
    T = d2i.load(os.path.join(CLIENTS[0], 'data', 'i18n', 'i18n_fr.d2i'))
    par_texte = {}
    for k, v in T.items():
        par_texte.setdefault(v, k)

    monstres = {}
    for mid, nid, race, niveau in requete("select m.Id, m.NameId, m.RaceId, ifnull((select max(g.Level) from monsters_grades g where g.MonsterId=m.Id),0) "
                                          f"from monsters_templates m where m.RaceId in {RACES}"):
        monstres[int(mid)] = dict(nameId=int(nid), nom=T.get(int(nid), '?'), race=int(race), niveau=int(niveau))
    spawns = {}
    for mid, sub, gmin, gmax, dis in requete("select s.MonsterId, s.SubAreaId, s.MinGrade, s.MaxGrade, s.IsDisabled from monsters_spawns s "
                                             f"join monsters_templates m on m.Id=s.MonsterId where m.RaceId in {RACES} and s.SubAreaId is not null"):
        spawns.setdefault(int(mid), []).append((int(sub), int(gmin), int(gmax), dis == '1'))

    index = {}
    for mid, m in monstres.items():
        index.setdefault(norm(m['nom']), []).append(mid)

    lignes = list(csv.DictReader(open(os.path.join(ICI, 'avis_dofuspourlesnoobs.tsv'), encoding='utf-8'), delimiter='\t'))
    retenus, ecartes = [], []
    for l in lignes:
        k = norm(l['nom'])
        ids = index.get(k) or next((v for kk, v in index.items() if k == kk or (len(k) > 4 and (k in kk or kk in k))), None)
        if not ids:
            ecartes.append((l['nom'], 'absent du jeu 2.51'))
            continue
        mid = ids[0]
        actifs = [s for s in spawns.get(mid, []) if not s[3]] or spawns.get(mid, [])  # apparitions coupees dans la base : on reprend leur zone
        if not actifs:
            ecartes.append((l['nom'], 'aucune zone d\'apparition active (monstre %d)' % mid))
            continue
        m = monstres[mid]
        niveau_min = int(l['niveau_min']) if l['niveau_min'] else max(1, m['niveau'] - 20)
        nom_fr = m['nom']
        # textes du client : « On recherche X » (nom) et « Capturer X. » (description)
        nom_id = par_texte.get('On recherche ' + nom_fr) or next((k for v, k in par_texte.items() if v.startswith('On recherche ') and norm(v[13:]) == norm(nom_fr)), None) or m['nameId']
        desc_id = next((k for v, k in par_texte.items() if v.startswith('Capturer ') and norm(v[9:].rstrip('.')) == norm(nom_fr)), None) or m['nameId']
        retenus.append(dict(monstre=mid, nom=nom_fr, zones=sorted({s[0] for s in actifs}), gmin=min(s[1] for s in actifs), gmax=max(s[2] for s in actifs),
                            niveau_min=niveau_min, doplons=int(l['doplons']), glace=int(l['kamas_de_glace']), alitons=int(l['alitons']),
                            nom_id=nom_id, desc_id=desc_id))

    for i, r in enumerate(retenus):
        r['succes'], r['objectif'], r['recompense'] = ID_SUCCES + i, ID_OBJECTIF + i, ID_RECOMPENSE + i
        objets = [(DOPLON, r['doplons']), (KAMA_GLACE, r['glace']), (ALITON, r['alitons'])]
        r['objets'] = [(o, q) for o, q in objets if q > 0]

    print(f"{len(retenus)} recherchés retenus :")
    for r in retenus:
        print(f"  {r['succes']} {r['nom']:<28} monstre {r['monstre']:>5} zones {','.join(map(str, r['zones'])):<12} niv >= {r['niveau_min']:>3}  "
              + ', '.join(f"{q} x {o}" for o, q in r['objets']))
    print(f"{len(ecartes)} écartés :")
    for n, why in ecartes:
        print(f"  {n} : {why}")

    ecrire_sql(retenus)
    if '--client' in sys.argv:
        for app in CLIENTS:
            if os.path.isdir(app):
                patch_client(app, retenus)


def ecrire_sql(retenus):
    sql = os.path.join(ICI, 'recherches.sql')
    ids = ','.join(str(r['monstre']) for r in retenus)
    succes = ','.join(str(r['succes']) for r in retenus)
    with open(sql, 'w', encoding='utf-8', newline='\n') as f:
        f.write("-- MYSTIC : recherchés (1 par zone, réapparition 4 à 6 h, un succès chacun). Généré par generer_recherches.py. Rejouable.\n")
        f.write("CREATE TABLE IF NOT EXISTS nekfeu_recherches (MonsterId INT NOT NULL PRIMARY KEY, Nom VARCHAR(100) NOT NULL, "
                "SubAreasCSV VARCHAR(255) NOT NULL, GradeMin INT NOT NULL, GradeMax INT NOT NULL, NiveauMin INT NOT NULL, "
                "AchievementId INT NOT NULL, ProchainSpawn DATETIME NULL) DEFAULT CHARSET=utf8;\n")
        f.write("DELETE FROM nekfeu_recherches;\n")
        for r in retenus:
            f.write("INSERT INTO nekfeu_recherches (MonsterId, Nom, SubAreasCSV, GradeMin, GradeMax, NiveauMin, AchievementId) VALUES "
                    f"({r['monstre']}, '{r['nom'].replace(chr(39), chr(39)*2)}', '{','.join(map(str, r['zones']))}', {r['gmin']}, {r['gmax']}, {r['niveau_min']}, {r['succes']});\n")
        f.write("-- les recherchés n'apparaissent plus dans les groupes normaux de leur zone\n")
        f.write(f"UPDATE monsters_spawns SET IsDisabled=1 WHERE MonsterId IN ({ids});\n")
        f.write("-- succès\n")
        f.write(f"DELETE FROM achievements_templates WHERE Id BETWEEN {ID_SUCCES} AND {ID_SUCCES + 199};\n")
        f.write(f"DELETE FROM achievements_objectives WHERE Id BETWEEN {ID_OBJECTIF} AND {ID_OBJECTIF + 199};\n")
        f.write(f"DELETE FROM achievements_rewards WHERE Id BETWEEN {ID_RECOMPENSE} AND {ID_RECOMPENSE + 199};\n")
        for i, r in enumerate(retenus):
            nom = ('Recherché : ' + r['nom']).replace("'", "''")
            f.write("INSERT INTO achievements_templates (Id, Name, NameId, CategoryId, DescriptionId, IconId, Points, Level, `Order`, KamasRatio, ExperienceRatio, "
                    "KamasScaleWithPlayerLevel, ObjectiveIdsCSV, RewardIdsCSV) VALUES "
                    f"({r['succes']}, '{nom}', {r['nom_id']}, {CATEGORIE}, {r['desc_id']}, {ICONE}, {POINTS}, {r['niveau_min']}, {100 + i}, 0, 0, 0, '{r['objectif']}', '{r['recompense']}');\n")
            # condition « EM » (tuer le monstre) : jamais validée seule hors donjon ; c'est RecherchesManager qui donne le succès
            f.write("INSERT INTO achievements_objectives (Id, Name, AchievementId, `Order`, NameId, Criterion) VALUES "
                    f"({r['objectif']}, '{nom}', {r['succes']}, 0, {r['desc_id']}, 'EM>{r['monstre']},0,d');\n")
            f.write("INSERT INTO achievements_rewards (Id, AchievementId, LevelMin, LevelMax, ItemsRewardCSV, ItemsQuantityRewardCSV, EmotesRewardCSV, "
                    "SpellsRewardCSV, TitlesRewardCSV, OrnamentsRewardCSV) VALUES "
                    f"({r['recompense']}, {r['succes']}, -1, -1, '{','.join(str(o) for o, q in r['objets'])}', '{','.join(str(q) for o, q in r['objets'])}', '', '', '', '');\n")
        cat = requete(f"select AchievementsIdsCSV from achievements_categories where Id={CATEGORIE}")[0][0]
        base = [x for x in cat.split(',') if x and not (ID_SUCCES <= int(x) < ID_SUCCES + 200)]
        f.write(f"UPDATE achievements_categories SET AchievementsIdsCSV='{','.join(base + succes.split(','))}' WHERE Id={CATEGORIE};\n")
    print('SQL écrit :', sql)


# ---------- client : fichiers d2o ----------
NULL_OBJ = -1431655766


def enc(v, t, sub):
    if t in (-1, -5): return struct.pack('>i', int(v))
    if t == -2: return b'\x01' if v else b'\x00'
    if t == -3:
        b = (v if v is not None else 'null').encode('utf-8'); return struct.pack('>H', len(b)) + b
    if t == -4: return struct.pack('>d', float(v))
    if t == -6: return struct.pack('>I', int(v))
    if t == -99:
        return struct.pack('>i', len(v)) + b''.join(enc(x, sub[0], sub[1:]) for x in v)
    raise ValueError('type objet non gere : %d' % t)


def ajouter_objets(path, objets, sauvegarde):
    """ajoute / remplace des objets (même classe que le fichier) : octets ajoutés à la fin des données, index mis à jour"""
    classes, _ = d2o.load(path)
    (cid, (cname, fields)), = [(k, v) for k, v in classes.items()][:1]
    d = open(path, 'rb').read()
    idx = struct.unpack_from('>i', d, 3)[0]
    n = struct.unpack_from('>i', d, idx)[0]
    index = dict(struct.unpack_from('>ii', d, p) for p in range(idx + 4, idx + 4 + n, 8))
    blob = bytearray()
    for o in objets:
        index[o['id']] = idx + len(blob)
        blob += struct.pack('>i', cid) + b''.join(enc(o[fn], t, sub) for fn, t, sub in fields)
    entrees = sorted(index.items())
    new_index = b''.join(struct.pack('>ii', k, off) for k, off in entrees)
    out = d[:3] + struct.pack('>i', idx + len(blob)) + d[7:idx] + bytes(blob) + struct.pack('>i', len(new_index)) + new_index + d[idx + 4 + n:]
    os.makedirs(sauvegarde, exist_ok=True)
    cible = os.path.join(sauvegarde, os.path.basename(path))
    if not os.path.exists(cible):
        shutil.copy2(path, cible)
    with open(path + '.tmp', 'wb') as f:
        f.write(out)
    os.replace(path + '.tmp', path)
    verif_c, verif_o = d2o.load(path)
    assert all(verif_o.get(o['id']) == {fn: o[fn] for fn, _, _ in fields} for o in objets), 'relecture differente : ' + path
    print(f"  {os.path.basename(path)} : {len(objets)} objet(s)")


def patch_client(app, retenus):
    common = os.path.join(app, 'data', 'common')
    nom_client = '3_CLIENT_64bits' if '64' in app else '4_CLIENT_32bits'
    sauvegarde = os.path.join(RACINE, 'SAVE', nom_client, 'd2o', datetime.datetime.now().strftime('%Y-%m-%d_%Hh%M') + '_avant-succes-recherches')
    print('Client', app)
    succes, objectifs, recompenses = [], [], []
    for i, r in enumerate(retenus):
        succes.append(dict(id=r['succes'], nameId=r['nom_id'], categoryId=CATEGORIE, descriptionId=r['desc_id'], iconId=ICONE, points=POINTS,
                           level=r['niveau_min'], order=100 + i, accountLinked=False, objectiveIds=[r['objectif']], rewardIds=[r['recompense']]))
        objectifs.append(dict(id=r['objectif'], achievementId=r['succes'], order=0, nameId=r['desc_id'], criterion='EM>%d,0,d' % r['monstre']))
        recompenses.append(dict(id=r['recompense'], achievementId=r['succes'], criteria='null', kamasRatio=0.0, experienceRatio=0.0,
                                kamasScaleWithPlayerLevel=False, itemsReward=[o for o, q in r['objets']], itemsQuantityReward=[q for o, q in r['objets']],
                                emotesReward=[], spellsReward=[], titlesReward=[], ornamentsReward=[]))
    ajouter_objets(os.path.join(common, 'Achievements.d2o'), succes, sauvegarde)
    ajouter_objets(os.path.join(common, 'AchievementObjectives.d2o'), objectifs, sauvegarde)
    ajouter_objets(os.path.join(common, 'AchievementRewards.d2o'), recompenses, sauvegarde)
    _, cats = d2o.load(os.path.join(common, 'AchievementCategories.d2o'))
    cat = dict(cats[CATEGORIE])
    cat['achievementIds'] = [x for x in cat['achievementIds'] if not (ID_SUCCES <= x < ID_SUCCES + 200)] + [r['succes'] for r in retenus]
    ajouter_objets(os.path.join(common, 'AchievementCategories.d2o'), [cat], sauvegarde)


main()
