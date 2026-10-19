# MYSTIC 19/10 : rapport lisible de l'audit des cartes -> RAPPORT_AUDIT_CARTES.md (racine du projet)
import json, os, subprocess, collections
ICI = os.path.dirname(os.path.abspath(__file__)); RACINE = os.path.abspath(os.path.join(ICI, '..', '..', '..'))
MYSQL = os.path.join(RACINE, '_OUTILS', 'logiciels', 'MariaDB', 'bin', 'mysql.exe')
def q(sql):
    out = subprocess.run([MYSQL, '-u', 'root', 'world', '--default-character-set=utf8mb4', '-N', '-B', '-e', sql], capture_output=True).stdout.decode('utf-8')
    return [l.split('\t') for l in out.splitlines() if l]
pos = {int(r[0]): (r[1], r[2], int(r[3])) for r in q("select p.Id,p.PosX,p.PosY,m.SubAreaId from world_maps_positions p join world_maps m on m.Id=p.Id")}
sz_nom = {int(r[0]): r[1] for r in q("select s.Id,l.French from world_subareas s join langs l on l.Id=s.NameId")}
sz_area = {int(r[0]): int(r[1]) for r in q("select Id,AreaId from world_subareas")}
a_nom = {int(r[0]): r[1] for r in q("select a.Id,l.French from world_areas a join langs l on l.Id=a.NameId")}
def lieu(m):
    if m not in pos: return f"{m}"
    x, y, sz = pos[m]
    return f"{m} [{x},{y}] {sz_nom.get(sz, '?')}"
def zone(m):
    return a_nom.get(sz_area.get(pos.get(m, (0, 0, 0))[2]), '?')
d = json.load(open(os.path.join(ICI, 'rapport.json'), encoding='utf-8'))
c = json.load(open(os.path.join(ICI, 'corrigeables.json'), encoding='utf-8'))
L = ["# Audit des cartes Mystic — 19/10/2026", "",
     "Script : `_OUTILS/serveur/audit_cartes/audit.py` (+ `corrigeables.py`, `rapport_md.py`). 13 201 cartes serveur, 13 203 cartes client, 1 888 redirections. Données brutes : `rapport.json`, `corrigeables.json`.", ""]
def section(titre, expl, lignes, fmt, maxi=60):
    L.append(f"## {titre} — {len(lignes)}"); L.append(""); L.append(expl); L.append("")
    par = collections.Counter(zone(x[0]) if isinstance(x[0], int) and x[0] else 'PNJ / objets' for x in lignes)
    L.append("Par zone : " + ", ".join(f"{z} {n}" for z, n in par.most_common(20))); L.append("")
    for x in lignes[:maxi]: L.append("- " + fmt(x))
    if len(lignes) > maxi: L.append(f"- … et {len(lignes) - maxi} autres (voir les fichiers .json)")
    L.append("")
section("1. Sortie qui mène au mauvais endroit (le client attend une autre carte)",
        "Le client affiche une sortie vers une carte, le serveur envoie le joueur vers une autre.",
        d.get('destination_differente_du_client', []), lambda x: f"{lieu(x[0])} — {x[1]} : client {lieu(x[2])} / serveur {lieu(x[3])}")
section("2. Sortie bloquée : carte voisine inexistante, mais une vraie carte existe juste à côté (CORRIGEABLE automatiquement)",
        "Le bord a une sortie, le voisin inscrit n'existe pas ; la carte aux coordonnées voisines existe → on peut relier les deux.",
        c['corrigeable'], lambda x: f"{lieu(x[0])} — {x[1]} : {x[2]} inexistante → {lieu(x[3])}", 120)
section("3. Sortie bloquée : plusieurs cartes possibles à côté (à trancher)",
        "Plusieurs cartes aux coordonnées voisines (étages, intérieurs) : choix à faire.",
        c['ambigu'], lambda x: f"{lieu(x[0])} — {x[1]} : {x[2]} inexistante → candidates {', '.join(lieu(k) for k in x[3])}")
section("4. Sortie vers le vide (aucune carte à côté)",
        "Bord de monde ou d'île, ou ancienne carte supprimée : la sortie ne mène nulle part. Le plus souvent sans gêne (bord de mer), parfois un passage manquant.",
        c['rien'], lambda x: f"{lieu(x[0])} — {x[1]} (voisin {x[2]} inexistant)", 80)
section("5. Coordonnées incohérentes", "La carte voisine n'est pas aux coordonnées attendues.",
        d.get('coordonnees_incoherentes', []), lambda x: f"{lieu(x[0])} — {x[1]} → {lieu(x[2])}")
section("6. Arrivée sans case praticable du bon côté", "En entrant, le joueur arrive d'un côté où aucune case n'est praticable près du bord (risque d'être placé n'importe où).",
        d.get('arrivee_sans_case_praticable', []), lambda x: f"{lieu(x[0])} — {x[1]} → {lieu(x[2])}", 60)
section("7. Passage à sens unique (le retour mène ailleurs)", "Aller A→B, mais le retour depuis B mène à C. Souvent voulu (grottes, dimensions), parfois une erreur.",
        d.get('retour_vers_autre_carte', []), lambda x: f"{lieu(x[0])} — {x[1]} → {lieu(x[2])}, retour vers {lieu(x[3])}", 60)
tp = [x for x in d.get('teleport_case_non_praticable', []) if x[4] != -1 and x[3]]
section("8. Téléportations (PNJ, potions, donjons) vers une case non praticable",
        "Ces téléportations ne corrigent pas la case d'arrivée : le joueur peut se retrouver dans un mur.",
        tp, lambda x: f"{x[1]} {x[2]} → {lieu(x[3])} case {x[4]}", 80)
section("9. Téléportation vers une carte inexistante", "", d.get('teleport_carte_inexistante', []), lambda x: f"{x[1]} {x[2]} → carte {x[3]} case {x[4]}")
open(os.path.join(RACINE, 'RAPPORT_AUDIT_CARTES.md'), 'w', encoding='utf-8').write("\n".join(L))
print('ok', len(tp))
