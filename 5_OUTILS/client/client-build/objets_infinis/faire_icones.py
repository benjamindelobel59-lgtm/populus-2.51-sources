# MYSTIC 19/10 : icônes des objets infinis = icône d'origine + petit badge « ∞ » en bas à droite.
# Entrée : source/<iconId>.png (extraits du client) ; sortie : icones/<nouvel iconId>.png + apercu.png
import os
from PIL import Image, ImageDraw, ImageFont
ICI = os.path.dirname(os.path.abspath(__file__))
ICONES = {990001: 33072, 990002: 12017, 990003: 12152, 990004: 12025, 990005: 14101}   # nouvel id -> icône d'origine
os.makedirs(os.path.join(ICI, 'icones'), exist_ok=True)

def badge(img):
    w, h = img.size
    r = int(w * 0.17)                      # rayon du badge
    cx, cy = w - r - 1, h - r - 1
    calque = Image.new('RGBA', img.size, (0, 0, 0, 0))
    d = ImageDraw.Draw(calque)
    d.ellipse([cx - r, cy - r, cx + r, cy + r], fill=(38, 26, 64, 235), outline=(232, 197, 71, 255), width=1)
    police = ImageFont.truetype('C:/Windows/Fonts/seguisym.ttf', int(r * 2.0))
    d.text((cx, cy + r * 0.05), '∞', font=police, fill=(255, 221, 102, 255), anchor='mm', stroke_width=1, stroke_fill=(90, 60, 10, 255))
    return Image.alpha_composite(img.convert('RGBA'), calque)

apercu = []
for nid, src in ICONES.items():
    im = Image.open(os.path.join(ICI, 'source', f'{src}.png'))
    out = badge(im)
    out.save(os.path.join(ICI, 'icones', f'{nid}.png'), optimize=True)
    apercu.append((im.convert('RGBA'), out))
    print(nid, src, im.size)
w, h = apercu[0][0].size
planche = Image.new('RGBA', (w * len(apercu), h * 2), (60, 60, 60, 255))
for i, (a, b) in enumerate(apercu):
    planche.alpha_composite(a.resize((w, h)), (i * w, 0)); planche.alpha_composite(b.resize((w, h)), (i * w, h))
planche.save(os.path.join(ICI, 'apercu.png'))
