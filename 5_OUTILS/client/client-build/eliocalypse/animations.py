# PADGREF 23/10 : Eliocalypse — copie des ANIMATIONS du client 2.74 absentes de notre 2.51 :
#   - os (sprites/bones*.d2p : monstres, effets de sorts), fichiers « <id>.swl » et « <id>-<animation>.swl » ;
#   - skins (sprites/skins*.d2p) du MÊME numéro que les os (le client charge le skin du numéro des os, comme en 2.51).
# Collisions : 15 skins 2.74 ont le numéro d'un skin 2.51 différent. 13 ne servent à rien chez nous (remplacés) ; 5600 et 5609
# servent à nos cosmétiques de boutique (objets 30771 / 30780) -> os + skin du Tambourreau (5600) et de l'Olgoth (5609)
# RENUMÉROTÉS en 9600 / 9601 (RENOMME, appliqué aussi aux apparences par donnees.py et au SQL serveur).
# Ajout à la dernière archive des chaînes bones1 et skins. Entrées : donnees.json, decor_importe.json.
import glob, json, os, re, struct, sys
sys.stdout.reconfigure(encoding='utf-8')
ICI = os.path.dirname(os.path.abspath(__file__))
CB = os.path.dirname(ICI)
R = os.path.abspath(os.path.join(CB, '..', '..', '..'))
sys.path.insert(0, CB); sys.path.insert(0, ICI)
import maps_elements as me
from cartes_decor_lib import lire_d2p, ecrire_d2p
from renomme import RENOMME
A = os.path.join(R, 'CLIENT_64bits', 'app', 'content', 'gfx', 'sprites')
N = os.path.join(R, '_OUTILS', 'client', 'client_27x', 'content', 'gfx', 'sprites')
SORTIE = os.path.join(ICI, 'client', 'content', 'gfx', 'sprites')
GARDES = {5600, 5609}                      # skins 2.51 utilisés par nos cosmétiques : jamais remplacés
d = json.load(open(os.path.join(ICI, 'donnees.json'), encoding='utf-8'))
dec = json.load(open(os.path.join(ICI, 'decor_importe.json'), encoding='utf-8'))
ids = set(d['gfxSorts'])
pnj = json.load(open(os.path.join(ICI, 'looks_pnj.json'))) if os.path.exists(os.path.join(ICI, 'looks_pnj.json')) else []
for look in d['looks'] + dec['looks'] + pnj:
    m = re.match(r'\{(\d+)\|?([^|]*)', look)
    if m:
        ids.add(RENOMME.get(int(m.group(1)), int(m.group(1))))
        ids |= {int(x) for x in m.group(2).split(',') if x.strip().isdigit()}
inverse = {v: k for k, v in RENOMME.items()}          # numéro chez nous -> numéro dans le 2.74


def swl_251(b):
    """PADGREF 23/10 : SWL 2.74 (version 11) -> SWL 2.51 (version 0). Au-delà de la version 10, le 2.74 a 4 octets de plus entre la
    liste des classes et le SWF ; le client 2.51 ne les saute pas (SWF illisible -> monstres affichés en « ? »)."""
    if b[0] != 76 or b[1] <= 10:
        return b
    p = 10
    for _ in range(struct.unpack_from('>i', b, 6)[0]):
        p += 2 + struct.unpack_from('>H', b, p)[0]
    return b[:1] + bytes(1) + b[2:p] + b[p + 4:]


def id_de(nom):
    m = re.match(r'^(\d+)((?:-[^.]*)?\.swl)$', os.path.basename(nom))
    return (int(m.group(1)), m.group(2)) if m else (None, None)


# os déjà présents en 2.51 : leurs skins 2.51 ne sont jamais remplacés (effets de sorts existants)
os_251 = {id_de(n)[0] for f in glob.glob(os.path.join(A, 'bones*.d2p')) for n, _ in me.read_d2p(f)} - {None}
bilan = {}
for motif, chaine, remplacer in (('bones*.d2p', 'bones1', False), ('skins*.d2p', 'skins', True)):
    deja = {n for f in glob.glob(os.path.join(A, motif)) for n, _ in me.read_d2p(f)}
    source = {}
    for f in glob.glob(os.path.join(N, motif)):
        for n, b in me.read_d2p(f):
            i, suite = id_de(n)
            if i is not None:
                source[(i, suite)] = swl_251(b)
    voulus = {}
    for i in ids:
        i274 = inverse.get(i, i)
        for (j, suite), b in source.items():
            if j != i274:
                continue
            nom = f'{i}{suite}'
            if nom in deja and (not remplacer or i in GARDES or i in os_251):
                continue
            voulus[nom] = b
    archives = sorted(glob.glob(os.path.join(A, chaine + '_*.d2p')), key=lambda x: int(x.rsplit('_', 1)[1][:-4]))
    dernier = archives[-1] if archives else os.path.join(A, chaine + '.d2p')
    if voulus:
        head, ent, props, pc = lire_d2p(dernier)
        ent = [e for e in ent if e[0] not in voulus] + [[k, v] for k, v in sorted(voulus.items())]
        ecrire_d2p(os.path.join(SORTIE, os.path.basename(dernier)), head, ent, props, pc)
    bilan[motif] = {'fichiers': len(voulus), 'Mo': sum(len(b) for b in voulus.values()) // 1048576, 'archive': os.path.basename(dernier),
                    'remplacés (existaient)': sorted(n for n in voulus if n in deja)}
print(json.dumps(bilan, ensure_ascii=False, indent=1))
