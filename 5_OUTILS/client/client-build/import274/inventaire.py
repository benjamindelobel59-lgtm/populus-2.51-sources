# PADGREF 23/10 (import 2.74, lots 1-4) : inventaire des fichiers du client 2.74 absents du nôtre, par dossier de content/.
# Archives .d2p : entrées absentes (même nom) ; dossiers simples : fichiers absents. Sortie : inventaire.json + résumé.
import collections, glob, json, os, sys
sys.stdout.reconfigure(encoding='utf-8')
ICI = os.path.dirname(os.path.abspath(__file__))
CB = os.path.dirname(ICI)
R = os.path.abspath(os.path.join(CB, '..', '..', '..'))
sys.path.insert(0, CB)
import maps_elements as me
A = os.path.join(R, 'CLIENT_64bits', 'app', 'content')
N = os.path.join(R, '_OUTILS', 'client', 'client_27x', 'content')


def contenu(racine):
    """{dossier relatif: {nom: taille}} ; les .d2p d'un dossier sont fusionnés."""
    res = collections.defaultdict(dict)
    for dp, _, fs in os.walk(racine):
        rel = os.path.relpath(dp, racine).replace('\\', '/')
        for f in fs:
            p = os.path.join(dp, f)
            if f.endswith('.d2p'):
                try:
                    for n, b in me.read_d2p(p):
                        res[rel][n] = len(b)
                except Exception as e:
                    res[rel]['!' + f] = -1
            else:
                res[rel][f] = os.path.getsize(p)
    return res


nos, leur = contenu(A), contenu(N)
bilan = {}
for d in sorted(leur):
    manque = {n: t for n, t in leur[d].items() if n not in nos.get(d, {})}
    if manque:
        bilan[d] = manque
json.dump(bilan, open(os.path.join(ICI, 'inventaire.json'), 'w', encoding='utf-8'))
for d, m in sorted(bilan.items(), key=lambda x: -sum(x[1].values())):
    print(f'{d:45s} {len(m):6d} fichiers {sum(m.values()) // 1048576:5d} Mo   ex. {list(m)[:3]}')
