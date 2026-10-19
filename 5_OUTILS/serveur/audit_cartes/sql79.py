# MYSTIC 19/10 : SQL 79 — sorties vers des cartes inexistantes reliées à la vraie carte voisine (corrigeables.json, audit des cartes).
# Seule la destination SERVEUR (Top/Bottom/Left/RightNeighbourId) change : le client continue de demander l'ancien numéro, que le
# serveur reconnaît (Client*NeighbourId inchangé) et remplace par la vraie carte. Garde-fous : même grande zone, ou retour de la
# carte d'arrivée qui pointe déjà vers la carte de départ ou vers une carte inexistante.
import json, os, subprocess
ICI = os.path.dirname(os.path.abspath(__file__)); RACINE = os.path.abspath(os.path.join(ICI, '..', '..', '..'))
MYSQL = os.path.join(RACINE, '_OUTILS', 'logiciels', 'MariaDB', 'bin', 'mysql.exe')
def q(sql):
    out = subprocess.run([MYSQL, '-u', 'root', 'world', '-N', '-B', '-e', sql], capture_output=True).stdout.decode('utf-8')
    return [l.split('\t') for l in out.splitlines() if l]
cols = {'haut': 'TopNeighbourId', 'bas': 'BottomNeighbourId', 'gauche': 'LeftNeighbourId', 'droite': 'RightNeighbourId'}
opp = {'haut': 'bas', 'bas': 'haut', 'gauche': 'droite', 'droite': 'gauche'}
idx = {'haut': 0, 'bas': 1, 'gauche': 2, 'droite': 3}
maps = {int(r[0]): (int(r[1]), [int(x) for x in r[2:6]]) for r in q("select Id,SubAreaId,TopNeighbourId,BottomNeighbourId,LeftNeighbourId,RightNeighbourId from world_maps")}
area = {int(r[0]): int(r[1]) for r in q("select Id,AreaId from world_subareas")}
c = json.load(open(os.path.join(ICI, 'corrigeables.json'), encoding='utf-8'))
ok, refus = [], []
for m, sens, mort, vraie in c['corrigeable']:
    meme_zone = area.get(maps[m][0]) == area.get(maps[vraie][0])
    retour = maps[vraie][1][idx[opp[sens]]]
    if meme_zone or retour == m or retour not in maps:
        ok.append((m, sens, mort, vraie))
    else:
        refus.append((m, sens, mort, vraie, retour))
L = ["-- MYSTIC 19/10/2026 : SQL 79 — audit des cartes : %d sorties qui menaient à une carte inexistante reliées à la vraie carte voisine." % len(ok),
     "-- Généré par _OUTILS/serveur/audit_cartes/sql79.py (détails : RAPPORT_AUDIT_CARTES.md, section 2). Seule la destination serveur change.",
     "-- Base world, World ARRÊTÉ. Rejouable.", "SET NAMES utf8mb4;", "USE `world`;", "START TRANSACTION;"]
for m, sens, mort, vraie in ok:
    L.append(f"UPDATE world_maps SET {cols[sens]} = {vraie} WHERE Id = {m} AND {cols[sens]} = {mort}; -- {sens}")
L += ["COMMIT;"]
open(os.path.join(RACINE, '_OUTILS', 'serveur', 'sql', '79_audit_cartes_sorties.sql'), 'w', encoding='utf-8', newline='\n').write('\n'.join(L) + '\n')
json.dump(refus, open(os.path.join(ICI, 'corrigeables_refuses.json'), 'w'))
print('corrections', len(ok), 'écartées', len(refus)); print('exemples écartés', refus[:6])
