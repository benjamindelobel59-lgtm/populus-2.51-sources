# PADGREF 24/10 : réponses de PNJ ajoutées au client + SQL 156.
#  1. Chasseur de primes (PNJ 10062, messages 60003 / 60004) : demande utilisateur « un bouton par recherché (plusieurs pages) au lieu
#     de tout afficher dans le dialogue + 50 000 kamas pour savoir s'il est là » : une réponse par recherché (ReplyId 60100 + rang,
#     texte 9120000 + rang = son nom), affichées 10 par page par le serveur (RecherchesManager), action « voir:<MonsterId> ».
#  2. Marin frigostien (PNJ 4176, message 29533, Mer Kantil) : « le PNJ ne propose pas de retourner au Berceau d'Alma » : réponse
#     907200 (texte 9120900) -> téléportation 54160201 [-53,-73] case 300.
#  3. Barque du Berceau d'Alma (compétence 3001133) : menait à La Bourgade 60817408 case -1 (invalide) -> bateau de la Mer Kantil
#     186646528 case 200 (Marin frigostien, cloche de plongée vers l'Épave).
# Sorties : client/data/common/Npcs.d2o, client/data/i18n/i18n_fr.d2i, SQL 156 (+ annuler) dans _OUTILS/serveur/pnj/.
# Base des fichiers : ceux INSTALLÉS dans CLIENT_64bits (i18n = recherches_legendaires, déjà installé).
import json, os, subprocess, sys
ICI = os.path.dirname(os.path.abspath(__file__)); CB = os.path.dirname(ICI)
R = os.path.abspath(os.path.join(CB, '..', '..', '..'))
sys.path.insert(0, CB)
from d2o_ecrire import D2O
APP = os.path.join(R, 'CLIENT_64bits', 'app')
MY = os.path.join(R, '_OUTILS', 'logiciels', 'MariaDB', 'bin', 'mysql.exe')
SQLD = os.path.join(R, '_OUTILS', 'serveur', 'pnj')
SORTIE = os.path.join(ICI, 'client')
os.makedirs(os.path.join(SORTIE, 'data', 'common'), exist_ok=True); os.makedirs(os.path.join(SORTIE, 'data', 'i18n'), exist_ok=True)
os.makedirs(SQLD, exist_ok=True)

CHASSEUR, MARIN = 10062, 4176
REPLY0, TEXTE0 = 60100, 9120000
REPLY_MARIN, TEXTE_MARIN = 907200, 9120900

out = subprocess.run([MY, '-uroot', '-N', '-B', '--default-character-set=utf8mb4', 'world', '-e',
                      'SELECT MonsterId, Nom FROM nekfeu_recherches ORDER BY Nom, MonsterId'], capture_output=True).stdout.decode('utf-8')
recherches = [(int(l.split('\t')[0]), l.split('\t')[1]) for l in out.splitlines() if l.strip()]
assert recherches, 'nekfeu_recherches vide'

textes = {str(TEXTE0 + i): nom for i, (_, nom) in enumerate(recherches)}
textes[str(TEXTE_MARIN)] = "Retourner au Berceau d'Alma."
json.dump(textes, open(os.path.join(ICI, 'textes.json'), 'w', encoding='utf-8'), ensure_ascii=False, indent=0)
subprocess.run([sys.executable, '-I', os.path.join(CB, 'd2i_ajout.py'), os.path.join(APP, 'data', 'i18n', 'i18n_fr.d2i'),
                os.path.join(SORTIE, 'data', 'i18n', 'i18n_fr.d2i'), os.path.join(ICI, 'textes.json')], check=True)

# Base des PNJ = celle de l'Éliocalypse (installée sur le VPS, pas sur ce PC) : Passeurs 10070 / 10071 (dont « Retourner à Astrub. »,
# demande utilisateur 24/10) et gardiens 5319 / 5325 / 5363 / 5367 ; sinon ce fichier les effacerait du client du VPS.
N = D2O(os.path.join(CB, 'eliocalypse', 'client', 'data', 'common', 'Npcs.d2o'))
ch = N.objet(CHASSEUR)
ch['dialogReplies'] = [r for r in ch['dialogReplies'] if not (REPLY0 <= r[0] < REPLY0 + 1000)] + \
                      [[REPLY0 + i, TEXTE0 + i] for i in range(len(recherches))]
N.poser(CHASSEUR, ch)
ma = N.objet(MARIN)
ma['dialogReplies'] = [r for r in ma['dialogReplies'] if r[0] != REPLY_MARIN] + [[REPLY_MARIN, TEXTE_MARIN]]
N.poser(MARIN, ma)
N.ecrire(os.path.join(SORTIE, 'data', 'common', 'Npcs.d2o'))

sql = ["-- PADGREF 24/10 SQL 156 : Chasseur de primes (un bouton par recherché, 10 par page, 50 000 kamas le renseignement), retour du",
       "-- Marin frigostien vers le Berceau d'Alma, barque du Berceau d'Alma vers le bateau de la Mer Kantil. Client : Npcs.d2o + i18n_fr.d2i.",
       "-- Généré par _OUTILS/client/client-build/pnj_padgref/generer.py.",
       f"DELETE FROM npcs_replies WHERE Id BETWEEN 960000 AND 960999 OR Id = 907103;"]
lignes = []
k = 960000
for msg in (60003, 60004):
    for i, (mid, nom) in enumerate(recherches):
        lignes.append(f"({k}, 'AvisRecherche', {REPLY0 + i}, {msg}, 'voir:{mid}')"); k += 1
sql.append("INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId, Parameter0) VALUES\n" + ',\n'.join(lignes) + ';')
sql.append(f"INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId, Parameter0, Parameter1, Parameter2) VALUES (907103, 'Teleport', {REPLY_MARIN}, 29533, '54160201', '300', '1');")
sql.append("UPDATE interactives_skills SET Parameter0 = '186646528', Parameter1 = '200' WHERE Id = 3001133;")
open(os.path.join(SQLD, '156_pnj_chasseur_marin_barque.sql'), 'w', encoding='utf-8').write('\n'.join(sql) + '\n')
open(os.path.join(SQLD, '156_pnj_chasseur_marin_barque-annuler.sql'), 'w', encoding='utf-8').write(
    "-- Annulation du SQL 156\nDELETE FROM npcs_replies WHERE Id BETWEEN 960000 AND 960999 OR Id = 907103;\n"
    "UPDATE interactives_skills SET Parameter0 = '60817408', Parameter1 = '-1' WHERE Id = 3001133;\n")
print(len(recherches), 'recherchés ; réponses', REPLY0, '->', REPLY0 + len(recherches) - 1)
