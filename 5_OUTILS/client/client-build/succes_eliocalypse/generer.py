# PADGREF 23/10 (demande utilisateur : « ajoute les succès des 4 nouveaux donjons, visibles en combat + dans la fenêtre des succès »,
# puis « les 20 sans les 4 Spécial pour l'instant ») : succès 2.74 des donjons de l'Eliocalypse (Arbre de Mort / Corruption, Trône de
# Sang / Guerre, Fers de la Tyrannie / Servitude, Sentence de la Balance / Misère), absents du client 2.51.
#   - par donjon : victoire, 3 succès de challenge (dont Duo), 1 méta = 20 succès (les « Spécial » 6255-6258 écartés) ;
#   - challenges 422-433 ajoutés à Challenge.d2o au format 2.51 (nom « <Boss> (challenge N / duo) », description = celle du
#     challenge de boss 2.51 du même type, icône jaune du même type recopiée) ;
#   - récompenses : celles de la 2.74 (XP x4 / x8, kamas x1, ressource du boss) avec la quantité 2.51 des ressources de boss (10).
# Sorties : client/ (data/common/*.d2o, data/i18n/i18n_fr.d2i, content/gfx/achievements|challenges/*.png) et SQL 132 (+ annuler).
# Usage : py -I generer.py
import json, os, re, shutil, subprocess, sys
sys.stdout.reconfigure(encoding='utf-8')
ICI = os.path.dirname(os.path.abspath(__file__))
R = os.path.abspath(os.path.join(ICI, '..', '..', '..', '..'))
sys.path.insert(0, os.path.join(R, '_OUTILS', 'serveur', 'donjons'))
sys.path.insert(0, os.path.join(R, '_OUTILS', 'client', 'client-build'))
import d2o, d2i
from d2o_ecrire import D2O
from d2i_ajout import ajouter

N = os.path.join(R, '_OUTILS', 'client', 'client_27x')                 # client 2.74 (source)
A = os.path.join(R, 'CLIENT_64bits', 'app')                             # client 2.51 installé (base des .d2o)
D2I_BASE = os.path.join(R, '_OUTILS', 'client', 'client-build', 'import274', 'client', 'data', 'i18n', 'i18n_fr.d2i')  # dernier d2i
OUT = os.path.join(ICI, 'client')
SQL = os.path.join(R, '_OUTILS', 'serveur', 'donjons', 'eliocalypse')
TEXTE0 = 9100000

T = d2i.load(os.path.join(N, 'data', 'i18n', 'i18n_fr.d2i'))
Ta = d2i.load(os.path.join(A, 'data', 'i18n', 'i18n_fr.d2i'))
L = lambda b, f: d2o.load(os.path.join(b, 'data', 'common', f + '.d2o'))[1]
AC, AO, AR, CH = L(N, 'Achievements'), L(N, 'AchievementObjectives'), L(N, 'AchievementRewards'), L(N, 'Challenges')

DONJONS = [  # boss, victoire, 3 succès de challenge, méta
    ('Corruption', 2257, [2258, 2259, 2260], 2263),
    ('Guerre', 2264, [2265, 2266, 2267], 2270),
    ('Servitude', 2271, [2272, 2273, 2274], 2277),
    ('Misère', 2278, [2279, 2280, 2281], 2284),
]
# type du challenge -> challenge de boss 2.51 du même type (sa description et son icône jaune sont reprises)
MODELE = {'Collant': 240, 'Mains propres': 124, 'Duo': 397, 'Zombie': 396, 'Misanthrope': 262, 'Focus': 393, 'Versatile': 392,
          'Hardi': 333}
QTE_RESSOURCE = 10
NIVEAU_190 = 733930           # texte 2.51 « Niveau 190 » (objectif de niveau des succès d'Ilyzaelle)

textes = {}


def texte(s):
    k = TEXTE0 + len(textes)
    textes[k] = s
    return k


fa, fo, fr, fc, fch = (D2O(os.path.join(A, 'data', 'common', f + '.d2o')) for f in
                       ('Achievements', 'AchievementObjectives', 'AchievementRewards', 'AchievementCategories', 'Challenge'))
mod_a, mod_o, mod_r, mod_ch = fa.objet(1625), fo.objet(5156), fr.objet(2413), fch.objet(397)
cat = fc.objet(59)
ordre = 148
nom_succes = {}
sql, sql_ids = [], {'a': [], 'o': [], 'r': []}
q = lambda s: "'" + s.replace("\\", "\\\\").replace("'", "''") + "'"
challenges = {}

for boss, victoire, succ, meta in DONJONS:
    nchal = 0
    for aid in [victoire] + succ + [meta]:
        a = AC[aid]
        nom = T.get(a['nameId'])
        nom_id = texte(nom)
        nom_succes[aid] = nom_id
        desc = T.get(a['descriptionId']) or ''
        m = re.search(r'\[challenge,(\d+)\]', desc)
        if m:
            cid = int(m.group(1))
            typ = T.get(CH[cid]['nameId'])
            nchal += 1
            libelle = 'challenge duo' if typ == 'Duo' else 'challenge %d' % nchal
            desc_type = Ta.get(L(A, 'Challenge')[MODELE[typ]]['descriptionId'])
            challenges[cid] = (boss, typ, libelle, MODELE[typ])
            cdesc = T.get(CH[cid]['descriptionId']).replace('%2', '40').split('\n')[0].strip()
            desc = re.sub(r' en validant le challenge \[challenge,\d+\]\.', '. ' + cdesc, desc)
        objs = []
        for i, oid in enumerate(a['objectiveIds']):
            o = AO[oid]
            crit = o['criterion']
            mm = re.match(r'\(OA=(\d+)\)', crit)
            if crit.startswith('PL>'):
                onom = NIVEAU_190
            elif mm:
                onom = nom_succes[int(mm.group(1))]
            else:
                onom = texte(T.get(o['nameId']))
            ob = dict(mod_o, id=oid, achievementId=aid, order=o['order'], nameId=onom, criterion=crit)
            fo.poser(oid, ob)
            objs.append(oid)
            srv = re.sub(r'^\((.*)\)&Wo=0$', r'\1', crit)
            sql.append(f"INSERT INTO achievements_objectives (Id, Name, AchievementId, `Order`, NameId, Criterion) VALUES "
                       f"({oid}, {q(T.get(o['nameId']) if not crit.startswith('PL>') else 'Niveau 190')}, {aid}, {o['order']}, {onom}, {q(srv)});")
            sql_ids['o'].append(oid)
        kamas = xp = 0.0
        for rid in a['rewardIds']:
            r = AR[rid]
            qte = [QTE_RESSOURCE] * len(r['itemsReward'])
            rr = dict(mod_r, id=rid, achievementId=aid, criteria=r['criteria'], kamasRatio=r['kamasRatio'],
                      experienceRatio=r['experienceRatio'], kamasScaleWithPlayerLevel=r['kamasScaleWithPlayerLevel'],
                      itemsReward=r['itemsReward'], itemsQuantityReward=qte, emotesReward=r['emotesReward'],
                      spellsReward=r['spellsReward'], titlesReward=r['titlesReward'], ornamentsReward=r['ornamentsReward'])
            fr.poser(rid, rr)
            kamas, xp = max(kamas, r['kamasRatio']), max(xp, r['experienceRatio'])
            csv = lambda l: ','.join(map(str, l))
            sql.append(f"INSERT INTO achievements_rewards (Id, AchievementId, LevelMin, LevelMax, ItemsRewardCSV, ItemsQuantityRewardCSV, "
                       f"EmotesRewardCSV, SpellsRewardCSV, TitlesRewardCSV, OrnamentsRewardCSV) VALUES ({rid}, {aid}, -1, -1, "
                       f"{q(csv(r['itemsReward']))}, {q(csv(qte))}, {q(csv(r['emotesReward']))}, NULL, {q(csv(r['titlesReward']))}, "
                       f"{q(csv(r['ornamentsReward']))});")
            sql_ids['r'].append(rid)
        desc_id = texte(desc)
        fa.poser(aid, dict(mod_a, id=aid, nameId=nom_id, categoryId=59, descriptionId=desc_id, iconId=a['iconId'],
                           points=a['points'], level=a['level'], order=ordre, accountLinked=False, objectiveIds=objs,
                           rewardIds=a['rewardIds']))
        sql.append(f"INSERT INTO achievements_templates (Id, Name, NameId, CategoryId, DescriptionId, IconId, Points, Level, `Order`, "
                   f"KamasRatio, ExperienceRatio, KamasScaleWithPlayerLevel, ObjectiveIdsCSV, RewardIdsCSV) VALUES ({aid}, {q(nom)}, "
                   f"{nom_id}, 59, {desc_id}, {a['iconId']}, {a['points']}, {a['level']}, {ordre}, {kamas}, {xp}, 0, "
                   f"{q(','.join(map(str, objs)))}, {q(','.join(map(str, a['rewardIds'])))});")
        sql_ids['a'].append(aid)
        cat['achievementIds'].append(aid)
        ordre += 1

ch251 = L(A, 'Challenge')
for cid, (boss, typ, libelle, modele) in challenges.items():
    fch.poser(cid, dict(mod_ch, id=cid, nameId=texte('%s (%s)' % (boss, libelle)), descriptionId=texte('Misanthrope') if typ == 'Misanthrope' else ch251[modele]['descriptionId'],  # 2.51 : « Anachorète », même règle
                        dareAvailable=False, incompatibleChallenges=[]))
fc.poser(59, cat)

os.makedirs(os.path.join(OUT, 'data', 'common'), exist_ok=True)
os.makedirs(os.path.join(OUT, 'data', 'i18n'), exist_ok=True)
for f, nom in ((fa, 'Achievements'), (fo, 'AchievementObjectives'), (fr, 'AchievementRewards'), (fc, 'AchievementCategories'),
               (fch, 'Challenge')):
    f.ecrire(os.path.join(OUT, 'data', 'common', nom + '.d2o'))
ajouter(D2I_BASE, os.path.join(OUT, 'data', 'i18n', 'i18n_fr.d2i'), textes)
for sous, couples in (('achievements', [(i, i) for i in sorted({AC[a]['iconId'] for b in DONJONS for a in [b[1]] + b[2] + [b[3]]})]),
                      ('challenges', [(m, c) for c, (_, _, _, m) in challenges.items()])):
    d = os.path.join(OUT, 'content', 'gfx', sous)
    os.makedirs(d, exist_ok=True)
    src = os.path.join(N if sous == 'achievements' else A, 'content', 'gfx', sous)
    for de, vers in couples:
        if os.path.exists(os.path.join(A, 'content', 'gfx', sous, '%d.png' % vers)):
            continue                  # déjà dans le client 2.51 (ex. icône 82 des succès méta)
        shutil.copyfile(os.path.join(src, '%d.png' % de), os.path.join(d, '%d.png' % vers))

ids = ','.join(map(str, sql_ids['a']))
tete = ("-- PADGREF 23/10/2026 : SQL 132 — succès des 4 donjons de l'Eliocalypse (2.74, 20 succès sans les « Spécial »), catégorie 59.\n"
        "-- Généré par _OUTILS/client/client-build/succes_eliocalypse/generer.py (client : mêmes ids, textes 9100000+). Base world. Rejouable.\n"
        "USE `world`;\n")
efface = (f"DELETE FROM achievements_templates WHERE Id IN ({ids});\n"
          f"DELETE FROM achievements_objectives WHERE Id IN ({','.join(map(str, sql_ids['o']))});\n"
          f"DELETE FROM achievements_rewards WHERE Id IN ({','.join(map(str, sql_ids['r']))});\n")
cat_sql = (f"UPDATE achievements_categories SET AchievementsIdsCSV = CONCAT(AchievementsIdsCSV, ',{ids}') "
           f"WHERE Id = 59 AND FIND_IN_SET('{sql_ids['a'][0]}', AchievementsIdsCSV) = 0;\n")
open(os.path.join(SQL, '132_eliocalypse_succes.sql'), 'w', encoding='utf-8', newline='\n').write(tete + efface + '\n'.join(sql) + '\n' + cat_sql)
open(os.path.join(SQL, '132_eliocalypse_succes-annuler.sql'), 'w', encoding='utf-8', newline='\n').write(
    "-- PADGREF 23/10/2026 : annule 132_eliocalypse_succes.sql\nUSE `world`;\n" + efface +
    f"UPDATE achievements_categories SET AchievementsIdsCSV = REPLACE(AchievementsIdsCSV, ',{ids}', '') WHERE Id = 59;\n")
print('succès', len(sql_ids['a']), 'objectifs', len(sql_ids['o']), 'récompenses', len(sql_ids['r']), 'challenges', sorted(challenges),
      'textes', len(textes))
