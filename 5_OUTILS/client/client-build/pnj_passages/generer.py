# PADGREF 24/10 : passages par PNJ (client + SQL 160).
#  1. Warko Linch (PNJ 1219) : retour utilisateur « le PNJ pour aller au Cimetière primitif ne fonctionne pas (il doit nous TP au
#     cimetière et de l'autre côté le même PNJ qui ramène à la forêt de Kaliptus) ». Celui de la forêt de Kaliptus (73402891) n'avait
#     aucune action (la condition visait sa carte officielle 73400320) -> son dialogue officiel (5 Feuilles + 5 Fleurs de Kaliptus,
#     réponse 83) ; un second Warko dans le cimetière (64487686 case 370) ramène à Kaliptus (message 907450, réponse 907451, textes neufs).
#  2. Agathe Mainate (PNJ 3732) : « pas de PNJ pour aller sur l'île de Nimotopia ». Dialogue officiel (textes déjà dans le client) :
#     à Cania [-28,-31] (164102659, devant le Muldobus), niveau 40, 3 questions (bonnes réponses : « Le rose », « Je leur fais tout
#     plein de bisous », « Je suis un aventurier avant tout ») puis Muldobus vers Nimotopia [-62,30] (179309058) ; sur l'île, la même
#     Agathe ramène à Cania. Point d'arrivée sur l'île : choisi (côté est), pas trouvé dans les guides.
# Sorties : client/data/common/Npcs.d2o + client/data/i18n/i18n_fr.d2i (base = pnj_padgref/client), SQL 160 (+ annuler).
import json, os, subprocess, sys
ICI = os.path.dirname(os.path.abspath(__file__)); CB = os.path.dirname(ICI)
R = os.path.abspath(os.path.join(CB, '..', '..', '..'))
sys.path.insert(0, CB)
from d2o_ecrire import D2O
BASE = os.path.join(CB, 'pnj_padgref', 'client', 'data')
SQLD = os.path.join(R, '_OUTILS', 'serveur', 'pnj')
SORTIE = os.path.join(ICI, 'client')
os.makedirs(os.path.join(SORTIE, 'data', 'common'), exist_ok=True); os.makedirs(os.path.join(SORTIE, 'data', 'i18n'), exist_ok=True)

WARKO, AGATHE = 1219, 3732
MSG_WARKO, REP_WARKO, T0 = 907450, 907451, 9121000
textes = {str(T0): "Le cimetière ne te réussit pas ? Je peux te ramener dans la forêt de Kaliptus.",
          str(T0 + 1): "Retourner dans la forêt de Kaliptus."}
json.dump(textes, open(os.path.join(ICI, 'textes.json'), 'w', encoding='utf-8'), ensure_ascii=False, indent=0)
subprocess.run([sys.executable, '-I', os.path.join(CB, 'd2i_ajout.py'), os.path.join(BASE, 'i18n', 'i18n_fr.d2i'),
                os.path.join(SORTIE, 'data', 'i18n', 'i18n_fr.d2i'), os.path.join(ICI, 'textes.json')], check=True)
N = D2O(os.path.join(BASE, 'common', 'Npcs.d2o'))
w = N.objet(WARKO)
w['dialogMessages'] = [m for m in w['dialogMessages'] if m[0] != MSG_WARKO] + [[MSG_WARKO, T0]]
w['dialogReplies'] = [r for r in w['dialogReplies'] if r[0] != REP_WARKO] + [[REP_WARKO, T0 + 1]]
N.poser(WARKO, w)
N.ecrire(os.path.join(SORTIE, 'data', 'common', 'Npcs.d2o'))

KALIPTUS, CIMETIERE, CANIA, NIMO = 73402891, 64487686, 164102659, 179309058
sql = ["-- PADGREF 24/10 SQL 160 : Warko Linch (Kaliptus <-> Cimetière primitif) et Agathe Mainate (Cania <-> Nimotopia).",
       "-- Généré par _OUTILS/client/client-build/pnj_passages/generer.py (client : Npcs.d2o + i18n_fr.d2i).",
       "DELETE FROM npcs_actions WHERE Id BETWEEN 907400 AND 907449;",
       "DELETE FROM npcs_spawns WHERE Id BETWEEN 907400 AND 907449;",
       "DELETE FROM npcs_replies WHERE Id BETWEEN 907400 AND 907499;",
       "DELETE FROM npcs_messages WHERE Id = 907450;",
       f"INSERT INTO npcs_messages (Id, MessageId, ParametersCSV) VALUES ({MSG_WARKO}, {T0}, '');",
       "INSERT INTO npcs_spawns (Id, NpcId, MapId, CellId, Direction) VALUES",
       f" (907400, {WARKO}, {CIMETIERE}, 370, 1),     -- Warko du cimetière (retour)",
       f" (907401, {AGATHE}, {CANIA}, 291, 1),       -- Agathe devant le Muldobus de Cania",
       f" (907402, {AGATHE}, {NIMO}, 283, 1);        -- Agathe sur Nimotopia (retour)",
       "INSERT INTO npcs_actions (Id, NpcId, Type, `Condition`, Priority, Parameter0) VALUES",
       f" (907403, {WARKO}, 'Talk', 'Mp={KALIPTUS}', 0, '5325'),",
       f" (907404, {WARKO}, 'Talk', 'Mp={CIMETIERE}', 0, '{MSG_WARKO}'),",
       f" (907405, {AGATHE}, 'Talk', 'Mp={CANIA}', 0, '26861'),",
       f" (907406, {AGATHE}, 'Talk', 'Mp={NIMO}', 0, '26860');",
       "INSERT INTO npcs_replies (Id, Type, ReplyId, MessageId, Criteria, Parameter0, Parameter1, Parameter2) VALUES",
       f" (907410, 'Teleport', {REP_WARKO}, {MSG_WARKO}, NULL, '{KALIPTUS}', '372', '1'),",
       # Agathe : « Tu veux aller sur Nimotopia ? »
       " (907411, 'Dialog', 32280, 26861, 'PL>39', '26862', NULL, NULL),          -- « Oui ! » -> couleur préférée",
       f" (907412, 'Teleport', 32281, 26861, 'PL>39', '{NIMO}', '297', '1'),     -- déjà répondu -> l'île",
       " (907413, 'EndDialog', 32282, 26861, NULL, NULL, NULL, NULL),",
       " (907414, 'Dialog', 32277, 26862, NULL, '26864', NULL, NULL),             -- « Le rose. » (bonne réponse)",
       " (907415, 'Dialog', 32278, 26862, NULL, '26863', NULL, NULL),",
       " (907416, 'Dialog', 32279, 26862, NULL, '26863', NULL, NULL),",
       " (907417, 'Dialog', 32283, 26863, NULL, '26861', NULL, NULL),             -- reprendre depuis le début",
       " (907418, 'Dialog', 32288, 26864, NULL, '26866', NULL, NULL),             -- « tout plein de bisous » (bonne réponse)",
       " (907419, 'Dialog', 32286, 26864, NULL, '26865', NULL, NULL),",
       " (907420, 'Dialog', 32287, 26864, NULL, '26865', NULL, NULL),",
       " (907421, 'EndDialog', 32284, 26864, NULL, NULL, NULL, NULL),",
       " (907422, 'EndDialog', 32285, 26864, NULL, NULL, NULL, NULL),",
       " (907423, 'Dialog', 32289, 26865, NULL, '26861', NULL, NULL),",
       " (907424, 'Dialog', 32293, 26866, NULL, '26867', NULL, NULL),             -- « aventurier avant tout » (bonne réponse)",
       " (907425, 'Dialog', 32290, 26866, NULL, '27298', NULL, NULL),",
       " (907426, 'Dialog', 32291, 26866, NULL, '27298', NULL, NULL),",
       " (907427, 'Dialog', 32772, 27298, NULL, '26861', NULL, NULL),",
       f" (907428, 'Teleport', 32292, 26867, NULL, '{NIMO}', '297', '1'),       -- Muldobus -> Nimotopia",
       f" (907429, 'Teleport', 32276, 26860, NULL, '{CANIA}', '305', '1');      -- quitter Nimotopia -> Cania"]
open(os.path.join(SQLD, '160_pnj_cimetiere_nimotopia.sql'), 'w', encoding='utf-8').write('\n'.join(sql) + '\n')
open(os.path.join(SQLD, '160_pnj_cimetiere_nimotopia-annuler.sql'), 'w', encoding='utf-8').write(
    "-- Annulation du SQL 160\nDELETE FROM npcs_actions WHERE Id BETWEEN 907400 AND 907449;\n"
    "DELETE FROM npcs_spawns WHERE Id BETWEEN 907400 AND 907449;\nDELETE FROM npcs_replies WHERE Id BETWEEN 907400 AND 907499;\n"
    "DELETE FROM npcs_messages WHERE Id = 907450;\n")
print('ok')
