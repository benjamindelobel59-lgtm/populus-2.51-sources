# -*- coding: utf-8 -*-
# PADGREF 23/10 (demande utilisateur) : menu admin du client (ui/Ankama_Admin/menuadmin.xml, PAS le panel .admin) ->
# Téléportation > Téléportation Donjon : ajoute TOUS les donjons qui n'y sont pas encore, avec leurs salles dans l'ordre du
# parcours (base world : dungeons_entries.FirstRoomMapId puis monsters_spawns_dungeons.TeleportMapId de salle en salle),
# la salle du boss (« Boss : <nom> ») et les salles en plus (ex. Comte Harebourg : salle de Dorléans + duos de boss).
# Noms français : Monsters.d2o + i18n_fr.d2i du client. Commande : « .go <carte> %p » (même forme que les entrées existantes).
# Utilisation : py -I teleports_donjons.py <menuadmin.xml source> <menuadmin.xml sortie>
import os, re, struct, subprocess, sys
from collections import OrderedDict
ICI = os.path.dirname(os.path.abspath(__file__))
R = os.path.abspath(os.path.join(ICI, '..', '..', '..', '..'))
sys.path.insert(0, os.path.dirname(ICI))
from d2o_ecrire import D2O
MYSQL = os.path.join(R, '_OUTILS', 'logiciels', 'MariaDB', 'bin', 'mysql.exe')
DATA = os.path.join(R, 'CLIENT_64bits', 'app', 'data')
src, out = sys.argv[1], sys.argv[2]

def sql(q):
    r = subprocess.run([MYSQL, '-u', 'root', 'world', '-N', '-B', '--default-character-set=utf8', '-e', q], capture_output=True)
    return [l.split('\t') for l in r.stdout.decode('utf-8').splitlines() if l]

# textes français
d = open(os.path.join(DATA, 'i18n', 'i18n_fr.d2i'), 'rb').read()
io = struct.unpack_from('>i', d, 0)[0]; taille = struct.unpack_from('>i', d, io)[0]; o = io + 4; T = {}
while o < io + 4 + taille:
    k = struct.unpack_from('>i', d, o)[0]; diac = d[o + 4]; p = struct.unpack_from('>i', d, o + 5)[0]
    l = struct.unpack_from('>H', d, p)[0]; T[k] = d[p + 2:p + 2 + l].decode('utf-8', 'replace'); o += 9 + (4 if diac else 0)
mo = D2O(os.path.join(DATA, 'common', 'Monsters.d2o'))
noms_base = {int(i): n for i, n in sql("SELECT Id, Name FROM monsters_templates")}
def nom_monstre(mid):
    ob = mo.objet(mid) if mid in mo.index else None
    return (T.get(ob.get('nameId'), '') if ob else '') or noms_base.get(mid, '')  # monstres 2.74 (Eliocalypse) : nom de la base
boss = {int(i) for (i,) in sql("SELECT Id FROM monsters_templates WHERE IsBoss = 1")}

xml = open(src, encoding='utf-8').read()
deja = {int(m) for m in re.findall(r'\.go (\d+) ', xml)}

donjons = OrderedDict()
for did, nom, cartes, premiere in sql("SELECT d.Id, IFNULL(d.Name,''), IFNULL(d.MapsIdsCSV,''), IFNULL(e.FirstRoomMapId,'') FROM dungeons d "
                                      "LEFT JOIN dungeons_entries e ON e.DungeonId = d.Id ORDER BY d.Id"):
    if not nom or nom in donjons:
        continue
    donjons[nom] = (int(did), {int(x) for x in cartes.split(',') if x.strip().isdigit()}, int(premiere) if premiere.isdigit() else None)

spawns = {}   # carte -> (id spawn, carte suivante, description)
for sid, carte, suite, desc in sql("SELECT Id, MapId, IFNULL(TeleportMapId,0), IFNULL(Descripcion,'') FROM monsters_spawns_dungeons"):
    spawns.setdefault(int(carte), (int(sid), int(suite), desc))
mobs = {}     # id spawn -> monstres
for sid, mid in sql("SELECT g.DungeonSpawnId, mg.MonsterId FROM monsters_spawns_dungeons_groups g JOIN monsters_grades mg ON mg.Id = g.MonsterGradeId"):
    mobs.setdefault(int(sid), []).append(int(mid))

def boss_de(carte):
    s = spawns.get(carte)
    if not s: return []
    vus = []
    for m in mobs.get(s[0], []):
        if m in boss and m not in vus: vus.append(m)
    return vus

ajouts, comptes = [], 0
for nom, (did, cartes, premiere) in donjons.items():
    salles = [c for c in cartes if c in spawns]
    if not salles:
        continue
    cibles = {spawns[c][1] for c in salles}
    if premiere not in cartes:
        debuts = [c for c in salles if c not in cibles]
        premiere = debuts[0] if debuts else salles[0]
    ordre, c = [], premiere
    while c in cartes and c not in ordre:
        ordre.append(c)
        if c not in spawns: break
        c = spawns[c][1]
    autres = sorted(x for x in salles if x not in ordre)
    if (set(ordre) | set(autres)) & deja:
        continue        # déjà dans le menu
    lignes, n, apres_boss = [], 0, False
    for c in ordre + autres:
        b = boss_de(c)
        if b:
            lib = 'Boss : ' + ' + '.join(nom_monstre(x) or str(x) for x in b); apres_boss = True
        elif c in spawns:
            n += 1; lib = 'Salle %d' % n
        else:
            lib = 'Après le boss' if apres_boss else 'Salle sans combat'
        lignes.append('\t\t\t\t\t<item type="sendChat" label="%s" command=".go %d %%p" />' % (lib.replace('&', '&amp;').replace('"', '&quot;'), c))
    ajouts.append((nom, '\t\t\t\t<item type="menu" label="Donjon %s">\n%s\n\t\t\t\t</item>\n' % (nom.replace('&', '&amp;'), '\n'.join(lignes))))
    comptes += 1

# sous-menus de 20 donjons, par ordre alphabétique
ajouts.sort(key=lambda x: x[0].lower())
blocs = []
for i in range(0, len(ajouts), 20):
    part = ajouts[i:i + 20]
    blocs.append('\t\t\t<item type="menu" label="&lt;font color=\'#5CF39F\'&gt;Autres donjons %d (%s - %s)">\n\n%s\n\t\t\t</item>\n\n' % (
        i // 20 + 1, part[0][0][:12], part[-1][0][:12], '\n'.join(a for _, a in part)))

# Comte Harebourg : boss + duos après « Choix Boss »
comte = '\t\t\t\t\t<item type="sendChat" label="Choix Boss" command=".go 112202241 %p" />'
plus = [comte]
for c, lib in ((112203523, 'Boss : Comte Harebourg'), (112203271, 'Duo : Comte + Nileza'), (112203521, 'Duo : Comte + Sylargh'),
               (112203265, 'Duo : Comte + Klime'), (112203269, 'Duo : Comte + Missiz Frizz')):
    if c not in deja:
        plus.append('\t\t\t\t\t<item type="sendChat" label="%s" command=".go %d %%p" />' % (lib, c))
assert comte in xml, 'entrée « Choix Boss » du Comte introuvable'
xml = xml.replace(comte, '\n'.join(plus), 1)

# insertion juste avant le sous-menu « Dimensions » (fin de « Téléportation Donjon »)
ancre = '\t\t\t<item type="menu" label="&lt;font color=\'#F39F5C\'&gt;Dimensions">'
assert ancre in xml, 'sous-menu Dimensions introuvable'
xml = xml.replace(ancre, ''.join(blocs) + ancre, 1)
open(out, 'w', encoding='utf-8', newline='').write(xml)
print('%d donjons ajoutés en %d sous-menus ; Comte : %d entrées en plus' % (comptes, len(blocs), len(plus) - 1))
for nom, _ in ajouts: print(' -', nom)
