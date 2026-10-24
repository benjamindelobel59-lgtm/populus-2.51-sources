# PADGREF 23/10 : lecture du world-graph.binary du client 2.74 (WorldGraph.as) : passages entre cartes de l'Eliocalypse.
# Arête : de (carte double, zone int) vers (carte double, zone int), puis transitions (type byte, direction byte, skillId int,
# critère (int + utf), transitionMapId double, case int, id double). Types : 1 bord, 2 bord scripté, 4 événement, 8 action de carte
# (case), 16 obstacle, 32 interactif, 64 PNJ.
import json, os, struct, sys
sys.stdout.reconfigure(encoding='utf-8')
ICI = os.path.dirname(os.path.abspath(__file__))
R = os.path.abspath(os.path.join(ICI, '..', '..', '..', '..'))
TYPES = {0: '?', 1: 'bord', 2: 'bord scripté', 4: 'événement', 8: 'action case', 16: 'obstacle', 32: 'interactif', 64: 'PNJ'}


def lire(chemin):
    d = open(chemin, 'rb').read()
    p = 0

    def u(f):
        nonlocal p
        v = struct.unpack_from(f, d, p)[0]; p += struct.calcsize(f); return v
    aretes = []
    for _ in range(u('>i')):
        de, zde, vers, zvers = int(u('>d')), u('>i'), int(u('>d')), u('>i')
        tr = []
        for _ in range(u('>i')):
            dr, t, sk = u('>b'), u('>b'), u('>i')
            n = u('>i'); crit = d[p:p + n].decode('utf-8'); p += n
            tr.append({'type': t, 'direction': dr, 'skill': sk, 'critere': crit, 'carte': int(u('>d')), 'case': u('>i'), 'id': int(u('>d'))})
        aretes.append({'de': de, 'zoneDe': zde, 'vers': vers, 'zoneVers': zvers, 'transitions': tr})
    return aretes


if __name__ == '__main__':
    inv = json.load(open(os.path.join(R, '_OUTILS', 'serveur', 'donjons', 'eliocalypse', 'inventaire.json'), encoding='utf-8'))
    cartes = set(inv['cartes'])
    a = [x for x in lire(os.path.join(R, '_OUTILS', 'client', 'client_27x', 'content', 'maps', 'world-graph.binary')) if x['de'] in cartes]
    json.dump(a, open(os.path.join(ICI, 'graphe_eliocalypse.json'), 'w', encoding='utf-8'), ensure_ascii=False, indent=0)
    filtre = [int(x) for x in sys.argv[1:]]
    for x in a:
        if not filtre or x['de'] in filtre:
            for t in x['transitions']:
                print(x['de'], '->', x['vers'], TYPES.get(t['type'], t['type']), 'dir', t['direction'], 'case', t['case'], 'skill', t['skill'],
                      'id', t['id'], t['critere'])
