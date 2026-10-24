# -*- coding: utf-8 -*-
# PADGREF 23/10 : construit les 6 paquets PUSH de la demande « combat-vote » (un par demande). Rien n'est envoye.
#   no-anim-1, vitesse-combat-1, respawn-rapide-1, pause-combat-1, replay-combat-1, vote-1
# Structure identique a PUSH/icones-sorts-1 (TOUT_INSTALLER.bat + outils/tout_installer.ps1 + LISEZ-MOI.txt).
import os, shutil, glob
ICI = os.path.dirname(os.path.abspath(__file__))
R = os.path.abspath(os.path.join(ICI, '..', '..', '..', '..'))
PUSH = os.path.join(R, 'PUSH')
PS1 = os.path.join(PUSH, 'icones-sorts-1', 'outils', 'tout_installer.ps1')
DLL = os.path.join(R, '_OUTILS', 'serveur', 'build-emu', 'combat-vote3')
SITE = os.path.join(R, '_OUTILS', 'web', 'site', 'padgref')
INV64 = os.path.join(ICI, 'DofusInvoker.V64.combat-vote.swf')
INV32 = os.path.join(ICI, 'DofusInvoker.V32.combat-vote.swf')
XML_FIGHT = os.path.join(ICI, 'client', 'ui', 'Ankama_Fight', 'ui')
XML_BARRE = os.path.join(R, 'CLIENT_64bits', 'app', 'ui', 'Ankama_Config', 'ui', 'populusButtons.xml')
C64, C32 = 'Client', 'Client 32 bits (a donner a part)'

NOTE_CLIENT = """LE CLIENT (DofusInvoker.swf) EST LE MEME dans no-anim-1, vitesse-combat-1 et replay-combat-1 : il contient les 3
nouveautes du jeu (No anim, vitesse x2, bouton Replay). Installer l'un d'eux suffit pour le jeu ; les installer tous
ne pose pas de probleme (meme fichier, juste un numero de mise a jour de plus)."""
NOTE_DLL = """LA DLL DU WORLD EST LA MEME dans respawn-rapide-1, pause-combat-1 et replay-combat-1 : elle contient les 3
nouveautes du serveur (respawn rapide, .pause, replay) ET tout le code actuel (dont import274-sons-emblemes-1).
=> si tu installes import274-sons-emblemes-1, installe-le AVANT ces paquets (sinon sa DLL, plus ancienne, retire
   le respawn rapide, .pause et le replay). Installer un seul de ces 3 paquets suffit pour le serveur."""


def bat(nom, ligne):
    return ("@echo off\r\nrem PADGREF - a double-cliquer SUR LE VPS, dans ce dossier (%s).\r\nrem %s\r\n"
            "rem Le detail et l annulation sont dans LISEZ-MOI.txt. Journal : installation_<date>.log dans ce dossier.\r\n"
            "net session >nul 2>&1\r\nif %%errorlevel%% neq 0 (\r\n  echo Demande des droits administrateur...\r\n"
            "  powershell -NoProfile -Command \"Start-Process -FilePath '%%~f0' -Verb RunAs\"\r\n  exit /b\r\n)\r\n"
            "cd /d \"%%~dp0\"\r\npowershell -NoProfile -ExecutionPolicy Bypass -File \"%%~dp0outils\\tout_installer.ps1\"\r\n") % (nom, ligne)


def paquet(nom, ligne, lisezmoi, client=False, barre=False, xml_fight=False, dll=False, site=()):
    d = os.path.join(PUSH, nom)
    if os.path.exists(d):
        shutil.rmtree(d)
    os.makedirs(os.path.join(d, 'outils'))
    shutil.copy2(PS1, os.path.join(d, 'outils', 'tout_installer.ps1'))
    open(os.path.join(d, 'TOUT_INSTALLER.bat'), 'w', encoding='ascii', newline='').write(bat(nom, ligne))
    open(os.path.join(d, 'LISEZ-MOI.txt'), 'w', encoding='utf-8', newline='\r\n').write(lisezmoi.strip() + '\n')
    if client:
        for c, inv in ((C64, INV64), (C32, INV32)):
            os.makedirs(os.path.join(d, c, 'app'), exist_ok=True)
            shutil.copy2(inv, os.path.join(d, c, 'app', 'DofusInvoker.swf'))
            if xml_fight:
                u = os.path.join(d, c, 'app', 'ui', 'Ankama_Fight', 'ui'); os.makedirs(u, exist_ok=True)
                for f in ('fightResult.xml', 'fightResultSimple.xml'):
                    shutil.copy2(os.path.join(XML_FIGHT, f), u)
        if barre:
            u = os.path.join(d, C64, 'app', 'ui', 'Ankama_Config', 'ui'); os.makedirs(u, exist_ok=True)
            shutil.copy2(XML_BARRE, u)
    if dll:
        s = os.path.join(d, 'Serveur - WorldServer'); os.makedirs(s)
        for f in ('Stump.Server.WorldServer.dll', 'Stump.Server.WorldServer.pdb'):
            shutil.copy2(os.path.join(DLL, f), s)
    for rel in site:
        for src in glob.glob(os.path.join(SITE, rel)):
            dst = os.path.join(d, 'Site web', os.path.relpath(src, SITE))
            os.makedirs(os.path.dirname(dst), exist_ok=True)
            shutil.copy2(src, dst)
    # jamais de secret dans un paquet
    for f in glob.glob(os.path.join(d, '**', '*'), recursive=True):
        assert 'config.local' not in os.path.basename(f), f
    taille = sum(os.path.getsize(f) for f in glob.glob(os.path.join(d, '**', '*'), recursive=True) if os.path.isfile(f))
    print('%-20s %6.1f Mo' % (nom, taille / 1e6))


paquet('combat-vote-1', 'No anim, vitesse x2, respawn rapide, .pause, replay, vote. Serveur + jeu + site.', r"""
PADGREF - combat-vote-1 - 23/10/2026 (remplace les 6 paquets no-anim-1, vitesse-combat-1, respawn-rapide-1,
pause-combat-1, replay-combat-1, vote-1 : tout est ici)
A installer SUR LE VPS, quand personne n'est en combat : double-clic sur TOUT_INSTALLER.bat ('save' puis fermer le
WorldServer quand il le demande). Il copie la DLL et relance le World, publie le jeu sur le launcher (mise a jour
obligatoire) et copie les fichiers du site dans le htdocs. Pas de SQL (les tables se creent toutes seules).
!! La DLL contient tout le code actuel, dont import274-sons-emblemes-1 : si tu installes ce paquet-la, fais-le AVANT
   celui-ci (sinon sa DLL, plus ancienne, retire les nouveautes ci-dessous).

1. NO ANIM (barre des boutons, client 64 bits)
   Nouveau bouton "No anim" (icone spirale) : active / desactive l'option du jeu "desactiver les animations de
   combat". Cadre vert = active, gris = desactive ; meme etat que la case des options.
   Client 32 bits : pas de barre -> l'option reste dans les options du jeu.

2. VITESSE DE COMBAT x2 (par defaut)
   En combat, animations des personnages / sorts, deplacements et poussees vont 2 fois plus vite. Hors combat rien ne
   change. Bouton "Vitesse x2" (icone fleche) dans la barre : x2 <-> vitesse normale (choix garde par le jeu).

3. RESPAWN RAPIDE
   Un groupe de monstres parti en combat est remplace 15 SECONDES apres le lancement du combat (avant : 60 a 220 s).
   Seulement les groupes normaux des zones (pas les donjons). Reglage : world_config.xml, Game > Maps > Spawns >
   ClassicalSpawningPool > <Variable name="DelaiRespawnApresCombat">15</Variable> (0 = comme avant).

4. .PAUSE (admins : Maitre du jeu / Administrateur)
   1er .pause : combat en pause (chrono arrete, aucun sort / deplacement / fin de tour, le tour suivant attend,
   message a tous). 2e .pause : reprise avec le temps qui restait. Refuse pendant le placement.

5. REPLAY DE COMBAT
   Fenetre de fin de combat : bouton "Replay" a cote de "Fermer" (fenetre reduite : icone fleche a cote de la croix).
   - En jeu : le combat est rejoue A L'IDENTIQUE, comme en spectateur (vraie carte, vrais personnages, sorts et
     animations, au rythme du combat, longues reflexions raccourcies a 1,5 s). "Quitter" pour arreter ; a la fin,
     retour sur sa carte. Aucune commande pour les joueurs.
   - Sur le site : lien dans le chat vers replay.html?r=<code> (a partager / joindre a un bug) : vraie image de la
     carte (1 962 cartes de combat, Frigost comprise), vrais personnages et monstres, actions pas a pas, journal
     du serveur, messages bruts. Replays gardes 14 jours (table world.padgref_replays).
   - Admins : "replaycombat <code>" rejoue en jeu le replay d'un joueur (code = fin du lien du site).
   - Liens vers http://IP_DU_VPS/ (quand padgref.fr marchera : world_config.xml, Game > Bugs > ReplayCombat >
     <Variable name="SiteUrl">http://padgref.fr/</Variable>).

6. VOTE (site, page vote.html, lien "Voter" dans le menu)
   Connecte a son compte Padgref : "Voter maintenant" -> code OTP de serveur-prive.net -> page de vote ; au retour
   sur l'onglet : +15 ogrines sur le compte. 1 vote recompense toutes les 1 h 30 par COMPTE et par IP. Journal :
   table auth.padgref_votes ; mouvement "Vote serveur-prive.net" dans l'historique des ogrines.
   !! TOKEN (jamais dans un paquet) : apres l'installation, ouvrir C:\xampp\htdocs\api\config.local.php
      - s'il existe : ajouter   'vote' => ['token' => 'TON_TOKEN'],   juste apres "return [" ;
      - sinon le creer :   <?php
                           return ['vote' => ['token' => 'TON_TOKEN']];
      Puis, quand la page marche, activer l'option "OTP" sur serveur-prive.net (onglet "Parametres").

Fichiers : Serveur - WorldServer (DLL) ; jeu 64 et 32 bits : app\DofusInvoker.swf, app\ui\Ankama_Fight\ui\fightResult*.xml,
(64) app\ui\Ankama_Config\ui\populusButtons.xml ; site : replay.html, vote.html, ogrines.html, assets\padgref.js,
api\replay.php, api\rendu.php, api\vote.php, img\cartes\*.webp (178 Mo).

A VERIFIER : combat de monstres (x2, bouton spirale, un groupe revient 15 s apres) ; .pause / .pause avec un admin ;
fin de combat -> "Replay" (rejoue en jeu) + lien du chat ; site > Voter (+15 ogrines, puis "prochain vote dans 1 h 29").
Annulation : <emulateur>\Sauvegardes_dll\combat-vote-1 (DLL, listes du launcher, \site = anciens fichiers du site).
""", client=True, barre=True, xml_fight=True, dll=True,
       site=('replay.html', 'vote.html', 'ogrines.html', 'assets/padgref.js', 'api/replay.php', 'api/rendu.php', 'api/vote.php', 'img/cartes/*.webp'))
print('fini')
