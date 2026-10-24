# PADGREF 21/10 : icônes des box de boss = coffre officiel du jeu (icône 172009, « Coffre magistral ») + médaillon avec le portrait
# officiel du boss (images du bestiaire du client, gfx/monsters, extraites pour le site : site/padgref/atelier/icones/monstres).
import os, sys
from PIL import Image, ImageDraw
ICI = os.path.dirname(os.path.abspath(__file__))
R = os.path.abspath(os.path.join(ICI, '..', '..', '..', '..'))
MON = os.path.join(R, '_OUTILS', 'web', 'site', 'padgref', 'atelier', 'icones', 'monstres')
COFFRE = os.path.join(ICI, 'source', '172009.png')
T = 120


def icone(boss):
    c = Image.open(COFFRE).convert('RGBA')
    c = c.crop(c.getbbox()); k = 86 / max(c.size); c = c.resize((round(c.width * k), round(c.height * k)), Image.LANCZOS)
    m = Image.open(os.path.join(MON, f'{boss}.png')).convert('RGBA')
    m = m.crop(m.getbbox())
    r = 74
    m.thumbnail((r - 8, r - 8), Image.LANCZOS)
    out = Image.new('RGBA', (T, T), (0, 0, 0, 0))
    med = Image.new('RGBA', (r, r), (0, 0, 0, 0)); d = ImageDraw.Draw(med)
    d.ellipse([0, 0, r - 1, r - 1], fill=(44, 30, 16, 240), outline=(240, 196, 84, 255), width=4)
    mask = Image.new('L', (r, r), 0); ImageDraw.Draw(mask).ellipse([4, 4, r - 5, r - 5], fill=255)
    lay = Image.new('RGBA', (r, r), (0, 0, 0, 0)); lay.alpha_composite(m, ((r - m.width) // 2, (r - m.height) // 2))
    lay.putalpha(Image.composite(lay.getchannel('A'), Image.new('L', (r, r), 0), mask))
    med.alpha_composite(lay)
    out.alpha_composite(med, (T - r, 0))
    out.alpha_composite(c, (0, T - c.height))
    return out


if __name__ == '__main__':
    ims = [icone(int(b)) for b in sys.argv[1:]]
    pl = Image.new('RGBA', (130 * len(ims), 130 + 70), (70, 70, 70, 255))
    for i, im in enumerate(ims):
        pl.alpha_composite(im, (i * 130 + 5, 5)); pl.alpha_composite(im.resize((52, 52), Image.LANCZOS), (i * 130 + 39, 140))
    pl.save(os.path.join(ICI, 'apercu.png'))
