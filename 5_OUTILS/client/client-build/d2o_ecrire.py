# MYSTIC 18/10 : ajoute ou remplace des objets dans un fichier .d2o du client (Spells.d2o, SpellLevels.d2o…).
# Format D2O : "D2O" | int position de l'index | objets | index (int taille, paires int clé / int position) | classes | recherche.
# Les nouveaux objets (ou les nouvelles versions d'objets existants) sont écrits après les objets d'origine ; l'index est réécrit
# (positions décalées de la place ajoutée) ; les classes et la zone de recherche sont recopiées telles quelles (la zone de
# recherche ne connaît pas les nouveaux objets : sans effet, le client les lit par identifiant).
# Usage (module) : d2o = D2O(chemin) ; o = d2o.objet(id) ; d2o.poser(id, dict) ; d2o.ecrire(sortie)
import struct

NUL = -1431655766


class D2O:
    def __init__(self, chemin):
        d = open(chemin, 'rb').read()
        assert d[:3] == b'D2O'
        self.d = d
        p_index = struct.unpack_from('>i', d, 3)[0]
        n = struct.unpack_from('>i', d, p_index)[0]
        self.index = {}
        for i in range(n // 8):
            k, pos = struct.unpack_from('>ii', d, p_index + 4 + 8 * i)
            self.index[k] = pos
        self.ordre = list(self.index)
        self.p_index = p_index
        p = p_index + 4 + n
        self.p_classes = p
        self.classes = {}
        nc = struct.unpack_from('>i', d, p)[0]; p += 4
        for _ in range(nc):
            cid = struct.unpack_from('>i', d, p)[0]; p += 4
            nom, p = self._utf(p); pkg, p = self._utf(p)
            nf = struct.unpack_from('>i', d, p)[0]; p += 4
            champs = []
            for _ in range(nf):
                fn, p = self._utf(p)
                t = struct.unpack_from('>i', d, p)[0]; p += 4
                ts = [t]
                while ts[-1] == -99:
                    _, p = self._utf(p)
                    ts.append(struct.unpack_from('>i', d, p)[0]); p += 4
                champs.append((fn, ts))
            self.classes[cid] = (nom, champs)
        self.nouveaux = {}

    def _utf(self, p):
        n = struct.unpack_from('>H', self.d, p)[0]
        return self.d[p + 2:p + 2 + n].decode('utf-8'), p + 2 + n

    # ---------------------------------------------------------------- lecture
    def _lire(self, ts, p):
        t = ts[0]
        d = self.d
        if t == -1: return struct.unpack_from('>i', d, p)[0], p + 4
        if t == -2: return d[p] != 0, p + 1
        if t == -3: return self._utf(p)
        if t == -4: return struct.unpack_from('>d', d, p)[0], p + 8
        if t == -5: return struct.unpack_from('>i', d, p)[0], p + 4
        if t == -6: return struct.unpack_from('>I', d, p)[0], p + 4
        if t == -99:
            n = struct.unpack_from('>i', d, p)[0]; p += 4
            v = []
            for _ in range(n):
                x, p = self._lire(ts[1:], p); v.append(x)
            return v, p
        cid = struct.unpack_from('>i', d, p)[0]; p += 4
        if cid == NUL:
            return None, p
        return self._objet(cid, p)

    def _objet(self, cid, p):
        o = {'__classe': cid}
        for fn, ts in self.classes[cid][1]:
            o[fn], p = self._lire(ts, p)
        return o, p

    def objet(self, k):
        if k in self.nouveaux:
            return self.nouveaux[k]
        pos = self.index.get(k)
        if pos is None:
            return None
        cid = struct.unpack_from('>i', self.d, pos)[0]
        return self._objet(cid, pos + 4)[0]

    # ---------------------------------------------------------------- écriture
    def _ecrire(self, ts, v, b):
        t = ts[0]
        if t in (-1, -5): b += struct.pack('>i', int(v)); return
        if t == -2: b += bytes([1 if v else 0]); return
        if t == -3:
            e = (v or '').encode('utf-8'); b += struct.pack('>H', len(e)) + e; return
        if t == -4: b += struct.pack('>d', float(v)); return
        if t == -6: b += struct.pack('>I', int(v) & 0xFFFFFFFF); return
        if t == -99:
            v = v or []
            b += struct.pack('>i', len(v))
            for x in v:
                self._ecrire(ts[1:], x, b)
            return
        if v is None:
            b += struct.pack('>i', NUL); return
        self._encoder(v, b)

    def _encoder(self, o, b):
        cid = o['__classe']
        b += struct.pack('>i', cid)
        for fn, ts in self.classes[cid][1]:
            self._ecrire(ts, o[fn], b)

    def classe(self, nom):
        return next(cid for cid, (n, _) in self.classes.items() if n == nom)

    def poser(self, k, o):
        """Ajoute ou remplace l'objet d'identifiant k (dict au format de objet(), '__classe' compris)."""
        self.nouveaux[k] = o
        if k not in self.index and k not in self.ordre:
            self.ordre.append(k)

    def ecrire(self, sortie):
        corps = bytearray(self.d[7:self.p_index])
        positions = dict(self.index)
        for k, o in self.nouveaux.items():
            positions[k] = 7 + len(corps)
            self._encoder(o, corps)
        p_index = 7 + len(corps)
        index = bytearray()
        for k in self.ordre:
            index += struct.pack('>ii', k, positions[k])
        reste = self.d[self.p_classes:]
        open(sortie, 'wb').write(b'D2O' + struct.pack('>i', p_index) + bytes(corps) + struct.pack('>i', len(index)) + bytes(index) + reste)
