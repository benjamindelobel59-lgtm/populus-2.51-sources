# MYSTIC 19/10 : PNJ « Chasseur de primes » (10062) au zaap d'Astrub : liste des avis de recherche (présents / déjà vaincus).
# Part des fichiers ACTUELS du client 64 bits (qui contiennent déjà Oto Mustam et les skis) et écrit Npcs.d2o, NpcMessages.d2o et
# i18n_fr.d2i dans _OUTILS/client/client-build/installation_2026-10-19/ (même installateur), + SQL 65.
import json, os, subprocess, sys
ICI = os.path.dirname(os.path.abspath(__file__))
CB = os.path.dirname(ICI)
RACINE = os.path.abspath(os.path.join(CB, '..', '..', '..'))
sys.path.insert(0, CB)
from d2o_ecrire import D2O

PNJ, NOM, LOOK = 10062, 505404, '{1001|||90}'          # « Chasseur de primes », apparence du Chef Milicien
DIESE1 = 29920                                         # texte « #1 » (rempli par le serveur)
M_RESUME, M_LISTE = 60003, 60004
REPONSES = [(60010, 9000961, "Voir les recherchés en liberté.", M_RESUME, "libres"),
            (60011, 9000962, "Voir les recherchés déjà vaincus.", M_RESUME, "vaincus"),
            (60012, 9000963, "Page suivante.", M_LISTE, "suivant"),
            (60014, 9000962, None, M_LISTE, "vaincus"),
            (60015, 9000961, None, M_LISTE, "libres"),
            (60013, 9000964, "Revenir au résumé.", M_LISTE, "retour")]
ASTRUB, CASE = 191105026, 259
COMMON = os.path.join(RACINE, 'CLIENT_64bits', 'app', 'data', 'common')
I18N = os.path.join(RACINE, 'CLIENT_64bits', 'app', 'data', 'i18n', 'i18n_fr.d2i')
SORTIE = os.path.join(CB, 'installation_2026-10-19')

npcs = D2O(os.path.join(COMMON, 'Npcs.d2o'))
assert npcs.objet(10061) is not None, "le client 64 bits doit déjà contenir Oto Mustam (installation du 19/10)"
o = dict(npcs.objet(10055), id=PNJ, nameId=NOM, dialogMessages=[[M_RESUME, DIESE1], [M_LISTE, DIESE1]],
         dialogReplies=[[r, t] for r, t, _, _, _ in REPONSES], actions=[3], gender=0, look=LOOK)
npcs.poser(PNJ, o)
npcs.ecrire(os.path.join(SORTIE, 'Npcs.d2o'))

msgs = D2O(os.path.join(COMMON, 'NpcMessages.d2o'))
for m in (M_RESUME, M_LISTE):
    msgs.poser(m, dict(msgs.objet(60001), id=m, messageId=DIESE1, messageParams=[]))
msgs.ecrire(os.path.join(SORTIE, 'NpcMessages.d2o'))

textes = {str(t): x for _, t, x, _, _ in REPONSES if x}
json.dump(textes, open(os.path.join(ICI, 'textes.json'), 'w', encoding='utf-8'), ensure_ascii=False)
subprocess.run([sys.executable, '-I', os.path.join(CB, 'd2i_ajout.py'), I18N, os.path.join(SORTIE, 'i18n_fr.d2i'),
                os.path.join(ICI, 'textes.json')], check=True)

sql = [f"-- MYSTIC 19/10/2026 : SQL 65 — PNJ « Chasseur de primes » ({PNJ}) au zaap d'Astrub : avis de recherche (présents / vaincus).",
       "-- Base world, World ARRÊTÉ. Rejouable. Textes calculés par le serveur (RecherchesManager.ParametresDialogue).",
       "SET NAMES utf8mb4;", "USE `world`;", "START TRANSACTION;",
       f"DELETE FROM npcs_templates WHERE Id = {PNJ};",
       "INSERT INTO npcs_templates (Id, Name, NameId, DialogMessagesIdCSV, DialogRepliesIdCSV, ActionsIdsCSV, Gender, LookAsString, SpecialArtworkId, TokenShop) VALUES "
       f"({PNJ}, 'Chasseur de primes', {NOM}, '{M_RESUME},{DIESE1};{M_LISTE},{DIESE1}', '{';'.join(f'{r},{t}' for r, t, _, _, _ in REPONSES)}', '3', 0, '{LOOK}', 0, 0);",
       f"DELETE FROM npcs_messages WHERE Id IN ({M_RESUME}, {M_LISTE});",
       f"INSERT INTO npcs_messages (Id, MessageId, ParametersCSV) VALUES ({M_RESUME}, {DIESE1}, ''), ({M_LISTE}, {DIESE1}, '');",
       f"DELETE FROM npcs_actions WHERE NpcId = {PNJ};",
       f"INSERT INTO npcs_actions (NpcId, Type, Priority, Parameter0) VALUES ({PNJ}, 'Talk', 0, '{M_RESUME}');",
       f"DELETE FROM npcs_replies WHERE MessageId IN ({M_RESUME}, {M_LISTE});"]
for r, t, _, m, action in REPONSES:
    sql.append(f"INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0) VALUES ('AvisRecherche', {r}, {m}, '{action}');")
sql += [f"DELETE FROM npcs_spawns WHERE NpcId = {PNJ};",
        f"INSERT INTO npcs_spawns (NpcId, MapId, CellId, Direction, Npc) VALUES ({PNJ}, {ASTRUB}, {CASE}, 1, 'Mystic chasseur de primes');",
        "COMMIT;"]
open(os.path.join(RACINE, '_OUTILS', 'serveur', 'sql', '65_chasseur_de_primes.sql'), 'w', encoding='utf-8', newline='\n').write('\n'.join(sql) + '\n')
print('ok')
