# -*- coding: utf-8 -*-
# MYSTIC : genere ui/populusButtons.xml (barre One Window) du client 64 bits. Boutons : (nom, icone). Le script ui::PopulusButtons
# (PopulusButtons.as, meme dossier) doit avoir les memes noms (btn_<nom>, btn_<nom>2, f_<nom>, f_<nom>2) et la commande de chaque bouton.
import os
RACINE = os.path.dirname(os.path.dirname(os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))))
SORTIE = os.path.join(RACINE, "CLIENT_64bits", "app", "ui", "Ankama_Config", "ui", "populusButtons.xml")
B = [("groupe", "[config.ui.skin]texture/btnIcon/btnIcon_invite.png"),
     ("onewindow", "[config.ui.skin]texture/btnIcon/btnIcon_computer.png"),
     ("ipdrop", "[config.ui.skin]texture/btnIcon/btnIcon_chest.png"),
     ("sasa", "[config.ui.skin]texture/Characteristics/tx_wisdom.png"),
     ("pass", "[config.ui.skin]texture/Quetes_tx_PictoSablier.png"),
     ("invo", "[config.ui.skin]texture/Characteristics/tx_summonableCreaturesBoost.png"),
     ("bug", "[config.ui.skin]texture/btnIcon/btnIcon_bug.png")]  # Mystic : Signaler un bug (.bug)


def bouton(n, icon, suf, x, y):
    return f'''
		<!-- cadre d'état : gris = désactivé, vert = activé (couleur changée par PopulusButtons) -->
		<Container name="f_{n}{suf}">
			<Anchors><Anchor><AbsDimension x="{x}" y="{y}" /></Anchor></Anchors>
			<Size><AbsDimension x="32" y="32" /></Size>
			<bgColor>0x5A5A5A</bgColor>
			<bgAlpha>1</bgAlpha>
		</Container>
		<themeIconButton name="btn_{n}{suf}">
			<Anchors><Anchor><AbsDimension x="{x + 2}" y="{y + 2}" /></Anchor></Anchors>
			<Size><AbsDimension x="28" y="28" /></Size>
			<smooth>true</smooth>
			<bgThemeDataId>button_thin_border</bgThemeDataId>
			<iconUri>{icon}</iconUri>
			<iconAnchors>
				<Anchor relativePoint="TOPLEFT"><AbsDimension x="3" y="3" /></Anchor>
				<Anchor relativePoint="BOTTOMRIGHT"><AbsDimension x="-3" y="-3" /></Anchor>
			</iconAnchors>
		</themeIconButton>
'''


def rotate(suf, x, y, h):
    return f'''
		<themeIconButton name="btn_rotate{suf}">
			<Anchors><Anchor><AbsDimension x="{x}" y="{y}" /></Anchor></Anchors>
			<Size><AbsDimension x="26" y="{h}" /></Size>
			<smooth>true</smooth>
			<bgThemeDataId>button_thin_border</bgThemeDataId>
			<iconUri>[config.ui.skin]texture/btnIcon/btnIcon_refresh.png</iconUri>
			<iconAnchors>
				<Anchor relativePoint="TOPLEFT"><AbsDimension x="4" y="4" /></Anchor>
				<Anchor relativePoint="BOTTOMRIGHT"><AbsDimension x="-4" y="-4" /></Anchor>
			</iconAnchors>
		</themeIconButton>
'''


def drag(suf, x, y, w, h, target):
    return f'''
		<!-- Poignée : cliquer-glisser ici pour déplacer la barre -->
		<Container name="ctr_drag{suf}">
			<Anchors><Anchor><AbsDimension x="{x}" y="{y}" /></Anchor></Anchors>
			<Size><AbsDimension x="{w}" y="{h}" /></Size>
			<bgColor>0xC8A24A</bgColor>
			<bgAlpha>0.85</bgAlpha>
			<dragController>true</dragController>
			<dragTarget>{target}</dragTarget>
			<dragSavePosition>true</dragSavePosition>
		</Container>
'''


W, H = 50 + len(B) * 34, 36
VH = 34 + len(B) * 34
x = f'''<?xml version="1.0" encoding="UTF-8"?>
<!-- Mystic : barre d'icônes Groupe / One Window / IP Drop / Sasa auto / Passe-tour auto / Contrôle des invocations
     (générée par _OUTILS/client/client-build/sources_module_config/generer_barre.py)
     ctr_h = disposition horizontale, ctr_v = verticale (bouton de rotation),
     le bouton à côté de la minimap affiche / cache la barre.
     Cadre gris = désactivé, vert = activé (état lu dans les messages du serveur, voir ui::PopulusButtons). -->
<Definition fullscreen="true">

	<Import url="[config.mod.template]button/themeIconButton.xml" />

	<Container name="ctr_h">
		<Anchors>
			<Anchor relativePoint="TOP" point="TOP"><AbsDimension x="0" y="90" /></Anchor>
		</Anchors>
		<Size><AbsDimension x="{W}" y="{H}" /></Size>
		<visible>true</visible>

		<Container name="ctr_h_bg">
			<Size><AbsDimension x="{W}" y="{H}" /></Size>
			<bgColor>[colors.gamemenu.background]</bgColor>
			<alpha>[colors.gamemenu.background.alpha]</alpha>
		</Container>
'''
x += drag("", 2, 2, 16, 32, "ctr_h") + rotate("", 20, 3, 30)
for i, (n, ic) in enumerate(B):
    x += bouton(n, ic, "", 50 + i * 34, 2)
x += f'''
	</Container>

	<Container name="ctr_v">
		<Anchors>
			<Anchor relativePoint="TOPLEFT" point="TOPLEFT"><AbsDimension x="10" y="150" /></Anchor>
		</Anchors>
		<Size><AbsDimension x="62" y="{VH}" /></Size>
		<visible>false</visible>

		<Container name="ctr_v_bg">
			<Size><AbsDimension x="62" y="{VH}" /></Size>
			<bgColor>[colors.gamemenu.background]</bgColor>
			<alpha>[colors.gamemenu.background.alpha]</alpha>
		</Container>
'''
x += drag("2", 2, 2, 30, 28, "ctr_v") + rotate("2", 34, 2, 28)
for i, (n, ic) in enumerate(B):
    x += bouton(n, ic, "2", 15, 34 + i * 34)
x += '''
	</Container>

	<!-- Mystic : horloge de l'Oeil de Vortex, en haut de l'écran (sur la bande noire), visible seulement quand l'Auroraire
	     est dans le combat ; texte mis à jour par PopulusButtons (état « Xe heure » 221..232 de l'Auroraire). -->
	<Container name="ctr_horloge">
		<Anchors>
			<Anchor relativePoint="TOP" point="TOP"><AbsDimension x="0" y="2" /></Anchor>
		</Anchors>
		<Size><AbsDimension x="280" y="30" /></Size>
		<visible>false</visible>
		<bgColor>0x000000</bgColor>
		<bgAlpha>0.75</bgAlpha>

		<Label name="lbl_horloge">
			<Anchors><Anchor><AbsDimension x="0" y="3" /></Anchor></Anchors>
			<Size><AbsDimension x="280" y="24" /></Size>
			<css>[config.ui.skin]css/title.css</css>
			<cssClass>whitecenter</cssClass>
			<text>Auroraire</text>
		</Label>
	</Container>
</Definition>
'''
open(SORTIE, "w", encoding="utf-8", newline="\n").write(x)
print("ecrit", SORTIE, W, VH)
