# PADGREF 23/10 : noms de classes de chaque fichier d'os (.swl) du 2.74 absent de notre client (lot 1) -> classes_os.json
import glob, json, os, struct, sys
ICI = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, os.path.dirname(ICI))
import maps_elements as me
res = {}
for f in sorted(glob.glob(os.path.join(ICI, 'client', 'content', 'gfx', 'sprites', 'bones11_7*.d2p'))):
    for n, b in me.read_d2p(f):
        if not n.endswith('.swl') or '-' in n:
            continue
        p, cl = 10, []
        for _ in range(struct.unpack_from('>i', b, 6)[0]):
            l = struct.unpack_from('>H', b, p)[0]; cl.append(b[p + 2:p + 2 + l].decode('utf-8', 'replace')); p += 2 + l
        res[int(n[:-4])] = cl
json.dump(res, open(os.path.join(ICI, 'classes_os.json'), 'w'), indent=0)
print(len(res))
