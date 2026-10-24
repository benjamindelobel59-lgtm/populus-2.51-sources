# PADGREF 24/10 (demande utilisateur : « pour les recherchés une flamme orange au-dessus du groupe, recopie la bleue en orange ») :
# conquestIcon.swf : nouveau sprite 52 = la forme 50 de la flamme bleue « archmonsters » (sprite 51) avec un filtre ColorMatrix
# (rotation de teinte 188° : bleu -> orange), et le symbole « nugget » (jeton AvA, jamais utilisé sur Padgref) pointe dessus.
# Le serveur met hasAVARewardToken = vrai sur les groupes qui contiennent un recherché -> le client 2.51 affiche cette icône.
# Usage : py -I flamme_orange.py <conquestIcon.swf source> <sortie>
import sys, zlib, struct, math
src, out = sys.argv[1], sys.argv[2]
d = open(src, 'rb').read()
assert d[:3] == b'CWS'
corps = zlib.decompress(d[8:])
nbits = corps[0] >> 3
debut = (5 + 4 * nbits + 7) // 8 + 4
tags, p = [], debut
while p < len(corps):
    h = struct.unpack_from('<H', corps, p)[0]; s = p; p += 2; code, ln = h >> 6, h & 63
    if ln == 63: ln = struct.unpack_from('<I', corps, p)[0]; p += 4
    tags.append((code, corps[s:p + ln], p, ln)); p += ln
    if code == 0: break

def tag(code, data):
    return struct.pack('<HI', (code << 6) | 63, len(data)) + data if len(data) >= 63 else struct.pack('<H', (code << 6) | len(data)) + data

a = math.radians(188); c, s = math.cos(a), math.sin(a); lr, lg, lb = 0.213, 0.715, 0.072
m = [lr + c * (1 - lr) + s * (-lr), lg + c * (-lg) + s * (-lg), lb + c * (-lb) + s * (1 - lb), 0, 0,
     lr + c * (-lr) + s * 0.143, lg + c * (1 - lg) + s * 0.140, lb + c * (-lb) + s * (-0.283), 0, 0,
     lr + c * (-lr) + s * (-(1 - lr)), lg + c * (-lg) + s * lg, lb + c * (1 - lb) + s * lb, 0, 0,
     0, 0, 0, 1, 0]
place3 = bytes([0x06, 0x01]) + struct.pack('<HH', 5, 50) + b'\x00' + bytes([1, 6]) + struct.pack('<20f', *m)
sprite = struct.pack('<HH', 52, 1) + tag(70, place3) + tag(1, b'') + tag(0, b'')
nouveau = bytearray(corps[:debut])
for code, brut, p0, ln in tags:
    if code == 76:  # SymbolClass : « nugget » -> 52
        nouveau += tag(39, sprite)
        q = p0; n = struct.unpack_from('<H', corps, q)[0]; q += 2; data = bytearray(struct.pack('<H', n)); vu = False
        for _ in range(n):
            cid = struct.unpack_from('<H', corps, q)[0]; q += 2; e = corps.index(b'\0', q); nom = corps[q:e]; q = e + 1
            if nom == b'nugget': cid = 52; vu = True
            data += struct.pack('<H', cid) + nom + b'\0'
        assert vu
        nouveau += tag(76, bytes(data))
    else:
        nouveau += brut
# nombre de caractères : en-tête inchangé (FrameSize / rate / count) ; taille totale du fichier
total = 8 + len(nouveau)
open(out, 'wb').write(b'CWS' + bytes([d[3]]) + struct.pack('<I', total) + zlib.compress(bytes(nouveau), 9))
print('ok', total)
