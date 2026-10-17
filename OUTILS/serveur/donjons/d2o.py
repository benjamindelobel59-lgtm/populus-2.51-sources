import struct, sys, json

class R:
    def __init__(s, d, p=0): s.d, s.p = d, p
    def i(s): v = struct.unpack_from(">i", s.d, s.p)[0]; s.p += 4; return v
    def u(s): v = struct.unpack_from(">I", s.d, s.p)[0]; s.p += 4; return v
    def b(s): v = s.d[s.p] != 0; s.p += 1; return v
    def f(s): v = struct.unpack_from(">d", s.d, s.p)[0]; s.p += 8; return v
    def utf(s):
        n = struct.unpack_from(">H", s.d, s.p)[0]; s.p += 2
        v = s.d[s.p:s.p + n].decode("utf-8", "replace"); s.p += n; return v

def load(path):
    d = open(path, "rb").read()
    assert d[:3] == b"D2O"
    r = R(d, 3)
    r.p = r.i()
    n = r.i()
    index = {}
    for _ in range(n // 8):
        k = r.i(); index[k] = r.i()
    classes = {}
    for _ in range(r.i()):
        cid = r.i(); name = r.utf(); pkg = r.utf(); fields = []
        for _ in range(r.i()):
            fname = r.utf(); t = r.i(); sub = []
            tt = t
            while tt == -99:
                r.utf(); tt = r.i(); sub.append(tt)
            fields.append((fname, t, sub))
        classes[cid] = (name, fields)
    def val(r, t, sub):
        if t == -1: return r.i()
        if t == -2: return r.b()
        if t == -3: return r.utf()
        if t == -4: return r.f()
        if t == -5: return r.i()
        if t == -6: return r.u()
        if t == -99:
            return [val(r, sub[0], sub[1:]) for _ in range(r.i())]
        cid = r.i()
        if cid == -1431655766: return None
        return obj(r, cid)
    def obj(r, cid):
        name, fields = classes[cid]
        return {fn: val(r, t, sub) for fn, t, sub in fields}
    out = {}
    for k, pos in index.items():
        rr = R(d, pos); out[k] = obj(rr, rr.i())
    return classes, out

if __name__ == "__main__":
    classes, objs = load(sys.argv[1])
    print([ (c[0], [f[0] for f in c[1]]) for c in classes.values()])
    for k in sys.argv[2:]:
        print(k, json.dumps(objs.get(int(k)), ensure_ascii=False))
    icons = {}
    for k, o in objs.items():
        ic = o.get("icon")
        if ic: icons[k] = (ic, o.get("iconVisibilityMask"))
    print("etats avec icone:", len(icons), "/", len(objs))
    print(sorted(set(v[0] for v in icons.values()))[:80])
