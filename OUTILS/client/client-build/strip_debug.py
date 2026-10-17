import struct, sys, zlib
DEBUG_TAGS = {58, 63, 64}   # EnableDebugger, DebugID, EnableDebugger2
src, dst = sys.argv[1], sys.argv[2]
data = open(src, "rb").read()
sig, version = data[:3], data[3]
if sig == b"CWS":
    body = zlib.decompress(data[8:])
elif sig == b"FWS":
    body = data[8:]
else:
    raise SystemExit("format non pris en charge : %r" % sig)
nbits = body[0] >> 3
header_len = (5 + nbits * 4 + 7) // 8 + 4
out, pos, removed = [body[:header_len]], header_len, []
while pos < len(body):
    start = pos
    cl = struct.unpack_from("<H", body, pos)[0]; pos += 2
    code, ln = cl >> 6, cl & 0x3F
    if ln == 0x3F:
        ln = struct.unpack_from("<I", body, pos)[0]; pos += 4
    pos += ln
    if code in DEBUG_TAGS:
        removed.append(code)
    else:
        out.append(body[start:pos])
    if code == 0:
        break
new_body = b"".join(out)
head = sig + bytes([version]) + struct.pack("<I", 8 + len(new_body))
open(dst, "wb").write(head + (zlib.compress(new_body, 9) if sig == b"CWS" else new_body))
print("tags retirés :", removed)
