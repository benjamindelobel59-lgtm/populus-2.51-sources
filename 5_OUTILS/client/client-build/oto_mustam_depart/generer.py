# MYSTIC 19/10 : PNJ « Oto Mustam » (un peu agrandi) sur la carte de départ 154010883, qui emmène devant le zaap d'Astrub.
# Sorties : Npcs.d2o, NpcMessages.d2o, i18n_fr.d2i (à copier dans les DEUX clients, jeu fermé) + SQL 59 (base world).
# Usage : py -I generer.py   (relit les fichiers actuels du client 64 bits : à relancer si le client a changé)
import json, os, subprocess, sys
ICI = os.path.dirname(os.path.abspath(__file__))
RACINE = os.path.abspath(os.path.join(ICI, '..', '..', '..', '..'))
sys.path.insert(0, os.path.dirname(ICI))
from d2o_ecrire import D2O

PNJ, NOM_TEXTE, LOOK = 10061, 8424, '{1028|||100}'           # Oto Mustam d'origine : PNJ 183, look {1028|||80}
MESSAGE, TEXTE_MESSAGE = 60002, 9000900                        # message < 65536 (envoyé en ushort par le serveur)
REPONSE, TEXTE_REPONSE = 60002, 20319                          # « Se rendre à Astrub. »
TEXTE = "Bienvenue sur Mystic, aventurier ! Quand tu seras prêt, je peux t'emmener directement à Astrub, juste devant le zaap."
DEPART, CASE_PNJ, DIR_PNJ = 154010883, 356, 1
ASTRUB, CASE_ASTRUB, DIR_ASTRUB = 191105026, 260, 1           # case sud-ouest du zaap 246 (celle de ZaapDialog)
COMMON = os.path.join(RACINE, 'CLIENT_64bits', 'app', 'data', 'common')
I18N = os.path.join(RACINE, 'CLIENT_64bits', 'app', 'data', 'i18n', 'i18n_fr.d2i')

npcs = D2O(os.path.join(COMMON, 'Npcs.d2o'))
modele = npcs.objet(10055)                                     # un de nos PNJ (même classe, champs complets)
o = dict(modele, id=PNJ, nameId=NOM_TEXTE, dialogMessages=[[MESSAGE, TEXTE_MESSAGE]],
         dialogReplies=[[REPONSE, TEXTE_REPONSE]], actions=[3], gender=0, look=LOOK)
npcs.poser(PNJ, o)
npcs.ecrire(os.path.join(ICI, 'Npcs.d2o'))
assert D2O(os.path.join(ICI, 'Npcs.d2o')).objet(PNJ)['look'] == LOOK

msgs = D2O(os.path.join(COMMON, 'NpcMessages.d2o'))
msgs.poser(MESSAGE, dict(msgs.objet(60001), id=MESSAGE, messageId=TEXTE_MESSAGE, messageParams=[]))
msgs.ecrire(os.path.join(ICI, 'NpcMessages.d2o'))

json.dump({str(TEXTE_MESSAGE): TEXTE}, open(os.path.join(ICI, 'textes.json'), 'w', encoding='utf-8'), ensure_ascii=False)
subprocess.run([sys.executable, '-I', os.path.join(os.path.dirname(ICI), 'd2i_ajout.py'), I18N,
                os.path.join(ICI, 'i18n_fr.d2i'), os.path.join(ICI, 'textes.json')], check=True)

q = lambda s: "'" + s.replace("'", "''") + "'"
sql = f"""-- MYSTIC 19/10/2026 : SQL 59 — départ des nouveaux personnages + PNJ Oto Mustam vers Astrub (base world, World ARRÊTÉ). Rejouable.
-- Client : Npcs.d2o, NpcMessages.d2o et i18n_fr.d2i de _OUTILS/client/client-build/oto_mustam_depart/ (les deux clients).
SET NAMES utf8mb4;
USE `world`;
START TRANSACTION;

-- 1) Carte de départ de toutes les classes (création et retour au départ sans point de sauvegarde) : Route des âmes, case 370
UPDATE breeds SET StartMap = {DEPART}, StartCell = 370;

-- 2) PNJ Oto Mustam (look un peu agrandi : 80 % -> 100 %)
DELETE FROM npcs_templates WHERE Id = {PNJ};
INSERT INTO npcs_templates (Id, Name, NameId, DialogMessagesIdCSV, DialogRepliesIdCSV, ActionsIdsCSV, Gender, LookAsString, SpecialArtworkId, TokenShop)
VALUES ({PNJ}, 'Oto Mustam', {NOM_TEXTE}, '{MESSAGE},{TEXTE_MESSAGE}', '{REPONSE},{TEXTE_REPONSE}', '3', 0, {q(LOOK)}, 0, 0);
DELETE FROM npcs_messages WHERE Id = {MESSAGE};
INSERT INTO npcs_messages (Id, MessageId, ParametersCSV) VALUES ({MESSAGE}, {TEXTE_MESSAGE}, '');
DELETE FROM npcs_actions WHERE NpcId = {PNJ};
INSERT INTO npcs_actions (NpcId, Type, Priority, Parameter0) VALUES ({PNJ}, 'Talk', 0, '{MESSAGE}');
DELETE FROM npcs_replies WHERE MessageId = {MESSAGE};
INSERT INTO npcs_replies (Type, ReplyId, MessageId, Parameter0, Parameter1, Parameter2)
VALUES ('Teleport', {REPONSE}, {MESSAGE}, '{ASTRUB}', '{CASE_ASTRUB}', '{DIR_ASTRUB}');
DELETE FROM npcs_spawns WHERE NpcId = {PNJ};
INSERT INTO npcs_spawns (NpcId, MapId, CellId, Direction, Npc) VALUES ({PNJ}, {DEPART}, {CASE_PNJ}, {DIR_PNJ}, 'Mystic depart -> Astrub');

COMMIT;
SELECT Id, StartMap, StartCell FROM breeds;
"""
open(os.path.join(RACINE, '_OUTILS', 'serveur', 'sql', '59_depart_oto_mustam.sql'), 'w', encoding='utf-8', newline='\n').write(sql)
print('ok')
