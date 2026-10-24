# PADGREF 23/10 : rendu de la seule image utile (AnimStatique_1) des os candidats pas encore rendus (rendu/swf/<k>/<os>.swf ->
# rendu/png/<k>/<os>.swf/DefineSprite_<id>_AnimStatique_1/1.png). FFDec « -selectid » : un fichier par appel, 4 en parallèle
# (l'export de tous les sprites restait bloqué sur des animations de 32 768 images).
import glob, os, struct, subprocess, sys, zlib
from concurrent.futures import ThreadPoolExecutor
ICI = os.path.dirname(os.path.abspath(__file__))
R = os.path.abspath(os.path.join(ICI, '..', '..', '..', '..'))
JAVA = os.path.join(R, '_OUTILS', 'logiciels', 'Java17', 'bin', 'java.exe')
FFDEC = os.path.join(R, '_OUTILS', 'logiciels', 'FFDec', 'ffdec.jar')


def id_statique(swf):
    d = open(swf, 'rb').read()
    if d[:3] == b'CWS':
        d = d[:8] + zlib.decompress(d[8:])
    elif d[:3] != b'FWS':
        return None
    nb = d[8] >> 3; p = 8 + (5 + nb * 4 + 7) // 8 + 4
    while p < len(d):
        h = struct.unpack_from('<H', d, p)[0]; p += 2; t, l = h >> 6, h & 63
        if l == 63:
            l = struct.unpack_from('<I', d, p)[0]; p += 4
        if t == 76:
            b = d[p:p + l]; q = 2
            for _ in range(struct.unpack_from('<H', b)[0]):
                cid = struct.unpack_from('<H', b, q)[0]; e = b.index(b'\0', q + 2)
                if b[q + 2:e] == b'AnimStatique_1':
                    return cid
                q = e + 1
        p += l
    return None


def fait(k, nom):
    return bool(glob.glob(os.path.join(ICI, 'rendu', 'png', k, nom, '*AnimStatique_1', '1.png')))


travaux = []
for swf in sorted(glob.glob(os.path.join(ICI, 'rendu', 'swf', '[0-3]', '*.swf'))):
    k, nom = os.path.basename(os.path.dirname(swf)), os.path.basename(swf)
    if not fait(k, nom):
        travaux.append((k, nom, swf))
print('à rendre :', len(travaux), flush=True)


def rendre(t):
    k, nom, swf = t
    cid = id_statique(swf)
    if cid is None:
        return nom, 'pas de AnimStatique_1'
    sortie = os.path.join(ICI, 'rendu', 'png', k, nom)
    try:
        subprocess.run([JAVA, '-jar', FFDEC, '-selectid', str(cid), '-zoom', '2', '-format', 'sprite:png', '-export', 'sprite', sortie, swf],
                       capture_output=True, timeout=120)
    except subprocess.TimeoutExpired:
        return nom, 'délai dépassé'
    return nom, 'ok' if fait(k, nom) else 'échec'


with ThreadPoolExecutor(4) as ex:
    res = list(ex.map(rendre, travaux))
import collections
print(collections.Counter(r for _, r in res))
