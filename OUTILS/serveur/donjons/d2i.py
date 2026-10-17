import struct
def load(path):
    d=open(path,"rb").read(); p=struct.unpack_from(">i",d,0)[0]
    n=struct.unpack_from(">i",d,p)[0]; q=p+4; end=q+n; out={}
    def s(pos):
        l=struct.unpack_from(">H",d,pos)[0]; return d[pos+2:pos+2+l].decode("utf-8","replace")
    while q<end:
        k,dia,pos=struct.unpack_from(">i?i",d,q); q+=9
        if dia: q+=4
        out[k]=s(pos)
    return out
