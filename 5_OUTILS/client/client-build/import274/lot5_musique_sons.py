# PADGREF 23/10 (import 2.74, point 2 « Musique & sons » validé par l'utilisateur) : sons et musiques du client 2.74 absents du nôtre.
#   - fichiers audio (musiques, ambiances, sons de combat, bruitages des animations, cris) : NOUVELLES archives en bout de chaîne
#     audio (audio11_6.d2p reçoit « link » vers la première) ; même nom = on garde le nôtre ;
#   - Playlists.d2o : playlists manquantes (ex. musiques des zones de l'Eliocalypse, déjà référencées par nos SubAreas), converties
#     au format 2.51 (silenceDuration / iteration repris de nos playlists du même type) ;
#   - SoundBones.d2o : sons des animations (bones) manquants ;
#   - SubAreas.d2o : sous-zones SANS musique chez nous (ou dont les playlists n'existent pas) -> playlists du 2.74. Les sous-zones
#     qui ont déjà leur musique ne sont pas touchées.
# Sortie : import274/client/content/audio/… et import274/client/data/common/{Playlists,SoundBones,SubAreas}.d2o.
import collections, glob, json, os, shutil, struct, sys
sys.stdout.reconfigure(encoding='utf-8')
ICI = os.path.dirname(os.path.abspath(__file__))
CB = os.path.dirname(ICI)
R = os.path.abspath(os.path.join(CB, '..', '..', '..'))
sys.path.insert(0, CB); sys.path.insert(0, os.path.join(CB, 'eliocalypse'))
sys.path.insert(0, os.path.join(R, '_OUTILS', 'serveur', 'donjons'))
import maps_elements as me, d2o
from d2o_ecrire import D2O
from cartes_decor_lib import lire_d2p, ecrire_d2p, lien
A = os.path.join(R, 'CLIENT_64bits', 'app')
N = os.path.join(R, '_OUTILS', 'client', 'client_27x')
ELIO = os.path.join(CB, 'eliocalypse', 'client', 'data', 'common')
SORTIE = os.path.join(ICI, 'client')
TAILLE_MAX = 50 * 1048576
NOUVELLE = 'audio11_7.d2p'


def classer(fichier, o):
    """Objet lu ailleurs (dict sans classe) -> format D2O : '__classe' posé (classe du fichier dont les champs collent), récursif."""
    if isinstance(o, list):
        return [classer(fichier, x) for x in o]
    if not isinstance(o, dict):
        return o
    cid = next(c for c, (_, ch) in fichier.classes.items() if {n for n, _ in ch} <= set(o) | {'__classe'} and
               len(ch) == len([k for k in o if k != '__classe']) or {n for n, _ in ch} == set(o) - {'__classe'})
    return {'__classe': cid, **{n: classer(fichier, o[n]) for n, _ in fichier.classes[cid][1]}}


def version_data(nom):
    for d in (os.path.join(SORTIE, 'data', 'common'), ELIO):
        if os.path.exists(os.path.join(d, nom)):
            return os.path.join(d, nom)
    return os.path.join(A, 'data', 'common', nom)


# ------------------------------------------------------------------ fichiers audio
dossier = os.path.join(A, 'content', 'audio')
chaine, f = [], 'audio0.d2p'
while f and f not in chaine:
    chaine.append(f); f = lien(os.path.join(dossier, f))
deja = {n for a in chaine for n, _ in me.read_d2p(os.path.join(dossier, a))}
ajout = {}
for src in sorted(glob.glob(os.path.join(N, 'content', 'audio', '*.d2p'))):
    for n, b in me.read_d2p(src):
        if n not in deja and n not in ajout:
            ajout[n] = b
head, _, _, _ = lire_d2p(os.path.join(dossier, chaine[-1]))
parts, cur, taille = [], [], 0
for n, b in sorted(ajout.items()):
    if cur and taille + len(b) > TAILLE_MAX:
        parts.append(cur); cur, taille = [], 0
    cur.append((n, b)); taille += len(b)
parts.append(cur)
noms = [NOUVELLE] + [NOUVELLE[:-4] + chr(97 + i) + '.d2p' for i in range(len(parts) - 1)]
for i, (nom, part) in enumerate(zip(noms, parts)):
    assert not os.path.exists(os.path.join(dossier, nom)) and nom not in chaine, nom
    props, pc = b'', 0
    if i + 1 < len(noms):
        k, v = b'link', noms[i + 1].encode()
        props, pc = struct.pack('>H', len(k)) + k + struct.pack('>H', len(v)) + v, 1
    ecrire_d2p(os.path.join(SORTIE, 'content', 'audio', nom), head, part, props, pc)
head, ent, props, pc = lire_d2p(os.path.join(dossier, chaine[-1]))
k, v = b'link', NOUVELLE.encode()
ecrire_d2p(os.path.join(SORTIE, 'content', 'audio', chaine[-1]), head, ent,
           bytes(props) + struct.pack('>H', len(k)) + k + struct.pack('>H', len(v)) + v, pc + 1)
assert lien(os.path.join(SORTIE, 'content', 'audio', chaine[-1])) == NOUVELLE
bilan = {'audio': {'fichiers': len(ajout), 'Mo': sum(len(b) for b in ajout.values()) // 1048576, 'archives': noms, 'lien depuis': chaine[-1],
                   'par dossier': dict(collections.Counter(n.split('/')[0] for n in ajout))}}
print(bilan['audio'], flush=True)

# ------------------------------------------------------------------ Playlists
os.makedirs(os.path.join(SORTIE, 'data', 'common'), exist_ok=True)
PA = d2o.load(version_data('Playlists.d2o'))[1]
PN = d2o.load(os.path.join(N, 'data', 'common', 'Playlists.d2o'))[1]
defaut = {}
for p in PA.values():
    defaut.setdefault(p['type'], collections.Counter())[(p['silenceDuration'], p['iteration'])] += 1
defaut = {t: c.most_common(1)[0][0] for t, c in defaut.items()}
PL = D2O(version_data('Playlists.d2o'))
nouvelles_pl = sorted(set(PN) - set(PA))
for i in nouvelles_pl:
    p = PN[i]
    sil, it = defaut.get(p['type'], (600, 3))
    PL.poser(i, classer(PL, {'id': i, 'silenceDuration': sil, 'iteration': it, 'type': p['type'],
                 'sounds': [{'id': s['id'], 'volume': s['volume']} for s in sorted(p['sounds'], key=lambda s: s.get('soundOrder', 0))]}))
PL.ecrire(os.path.join(SORTIE, 'data', 'common', 'Playlists.d2o'))
toutes_pl = set(PA) | set(nouvelles_pl)

# ------------------------------------------------------------------ SoundBones
SBA = D2O(version_data('SoundBones.d2o'))
SBN = d2o.load(os.path.join(N, 'data', 'common', 'SoundBones.d2o'))[1]
nouveaux_sb = sorted(set(SBN) - set(SBA.index))
for i in nouveaux_sb:
    SBA.poser(i, classer(SBA, SBN[i]))
SBA.ecrire(os.path.join(SORTIE, 'data', 'common', 'SoundBones.d2o'))

# ------------------------------------------------------------------ SubAreas sans musique
src_sa = version_data('SubAreas.d2o')
SAA = d2o.load(src_sa)[1]
SAN = d2o.load(os.path.join(N, 'data', 'common', 'SubAreas.d2o'))[1]
SAW = D2O(src_sa)
maj_sa = []
for k, v in SAA.items():
    actuelles = [pl for pl in (v.get('playlists') or []) if len(pl) > 1 and pl[1] in toutes_pl]
    nouv = [pl for pl in (SAN.get(k, {}).get('playlists') or []) if len(pl) > 1 and pl[1] in toutes_pl]
    if not actuelles and nouv:
        o = dict(SAW.objet(k)); o['playlists'] = nouv              # listes d'entiers : pas de classe
        SAW.poser(k, o); maj_sa.append(k)
if maj_sa:
    SAW.ecrire(os.path.join(SORTIE, 'data', 'common', 'SubAreas.d2o'))
# sons des playlists ajoutées : présents ?
fichiers = deja | set(ajout)
manquants = sorted({s['id'] for i in nouvelles_pl for s in PN[i]['sounds']
                    if not any(f'{d}/{s["id"]}.mp3' in fichiers for d in ('music', 'ambient', 'fight', 'barks', 'local', 'foleys', 'sfx'))})
bilan.update({'playlists ajoutées': len(nouvelles_pl), 'soundbones ajoutés': len(nouveaux_sb), 'sous-zones avec musique ajoutée': maj_sa,
              'sons de playlist introuvables': manquants, 'SubAreas source': src_sa})
json.dump(bilan, open(os.path.join(ICI, 'lot5_musique_sons_bilan.json'), 'w', encoding='utf-8'), ensure_ascii=False, indent=1)
print({k: (v if not isinstance(v, list) or len(v) < 30 else f'{len(v)} éléments') for k, v in bilan.items() if k != 'audio'})
