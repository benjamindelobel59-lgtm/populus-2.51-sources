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


paquet('no-anim-1', 'Bouton No anim dans la barre (client 64 bits). Jeu seulement (mise a jour obligatoire).', """
PADGREF - no-anim-1 - 23/10/2026 (independant des autres paquets)
Demande : un bouton "No anim" dans la barre du jeu, pour ne plus devoir le cocher dans les options.

Dans la barre des boutons (celle de One Window / IP Drop / Signaler un bug), 2 nouveaux boutons :
  - "No anim" (icone spirale) : active / desactive l'option du jeu "desactiver les animations de combat"
    (les animations des sorts ne sont plus jouees en combat). Cadre vert = active, gris = desactive.
  - "Vitesse x2" (icone fleche) : voir vitesse-combat-1.
L'etat est celui de l'option du jeu : cocher la case dans les options change aussi le bouton, et inversement.
Client 32 bits : il n'a pas la barre de boutons -> l'option reste dans les options du jeu (rien ne change pour lui,
sauf le DofusInvoker commun ci-dessous).

""" + NOTE_CLIENT + """

Fichiers : app\\DofusInvoker.swf (64 et 32 bits), app\\ui\\Ankama_Config\\ui\\populusButtons.xml (64 bits).
A installer SUR LE VPS : double-clic sur TOUT_INSTALLER.bat (publie le jeu sur le launcher, mise a jour obligatoire).
Pas de SQL, pas de redemarrage du serveur.
A VERIFIER : clic sur le bouton spirale en combat -> plus d'animation de sort ; re-clic -> animations revenues.
Annulation : <emulateur>\\Sauvegardes_dll\\no-anim-1 (listes du launcher).
""", client=True, barre=True, xml_fight=True)

paquet('vitesse-combat-1', 'Combats deux fois plus rapides par defaut. Jeu seulement (mise a jour obligatoire).', """
PADGREF - vitesse-combat-1 - 23/10/2026 (independant des autres paquets)
Demande : vitesse de combat x2 par defaut (combats plus rapides).

En combat, le jeu va deux fois plus vite : animations des personnages et des sorts, deplacements, poussees.
Le serveur attend deja la fin des animations des joueurs pour passer au tour suivant -> les combats sont plus courts.
Hors combat (carte, ville...) : rien ne change.
Reglage : option du jeu "vitesseCombat" (2 par defaut). Le bouton "Vitesse x2" de la barre (64 bits) passe de x2 a
x1 (vitesse normale) et inversement ; le choix est garde par le jeu.

""" + NOTE_CLIENT + """

Fichiers : app\\DofusInvoker.swf (64 et 32 bits), app\\ui\\Ankama_Config\\ui\\populusButtons.xml (64 bits).
A installer SUR LE VPS : double-clic sur TOUT_INSTALLER.bat (mise a jour obligatoire du jeu). Pas de SQL ni de redemarrage.
A VERIFIER : un combat contre des monstres : animations et deplacements 2 fois plus rapides ; bouton fleche -> normal.
Annulation : <emulateur>\\Sauvegardes_dll\\vitesse-combat-1 (listes du launcher).
""", client=True, barre=True, xml_fight=True)

paquet('respawn-rapide-1', 'Respawn rapide des groupes de monstres. Serveur seulement (World redemarre).', """
PADGREF - respawn-rapide-1 - 23/10/2026
Demande : respawn plus rapide des groupes de monstres une fois qu'un combat est lance.

Avant : un groupe parti en combat etait remplace au prochain passage du minuteur de la zone (60 a 220 secondes selon la
difficulte de la zone, parfois plus). Maintenant : un nouveau groupe apparait 15 SECONDES apres le lancement du combat.
Seulement les groupes "normaux" des zones (pas les salles de donjon, pas les monstres fixes).
Reglage sans recompiler : world_config.xml, Game > Maps > Spawns > ClassicalSpawningPool >
  <Variable name="DelaiRespawnApresCombat">15</Variable>   (en secondes ; 0 = comme avant)

""" + NOTE_DLL + """

A installer SUR LE VPS, quand personne n'est en combat : double-clic sur TOUT_INSTALLER.bat ('save' puis fermer le
WorldServer quand il le demande). Pas de SQL, pas de mise a jour du jeu.
A VERIFIER : lancer un combat sur une carte de monstres, attendre 15 s : un nouveau groupe apparait sur la carte.
Annulation : remettre les DLL de <emulateur>\\Sauvegardes_dll\\respawn-rapide-1.
""", dll=True)

paquet('pause-combat-1', 'Commande .pause des admins. Serveur seulement (World redemarre).', """
PADGREF - pause-combat-1 - 23/10/2026
Demande : commande .pause reservee aux admins, qui met le combat en pause et le reprend.

.pause (comptes Maitre du jeu et Administrateur seulement), dans le combat ou en spectateur :
  - 1er appel : combat en pause. Le chrono du tour s'arrete (le jeu affiche la pause), plus aucun sort, deplacement
    ni fin de tour possible ("Combat en pause : attends la reprise."), le tour suivant attend. Message a tous les joueurs.
  - 2e appel : reprise ; le joueur dont c'est le tour retrouve le temps qui lui restait.
Refusee pendant le placement ("possible seulement une fois le combat commence").

""" + NOTE_DLL + """

A installer SUR LE VPS, quand personne n'est en combat : double-clic sur TOUT_INSTALLER.bat. Pas de SQL ni de mise a jour du jeu.
A VERIFIER : en combat avec un compte admin : .pause -> essayer de lancer un sort (refuse) -> .pause -> le tour continue.
Annulation : remettre les DLL de <emulateur>\\Sauvegardes_dll\\pause-combat-1.
""", dll=True)

paquet('replay-combat-1', 'Bouton Replay de fin de combat + page replay du site. Serveur + jeu + site.', """
PADGREF - replay-combat-1 - 23/10/2026
Demande : un bouton "Replay" pour revoir le combat entier (utile en cas de bug), avec le vrai rendu.

1. EN JEU (le replay EXACT) : sur la fenetre de fin de combat, nouveau bouton "Replay" a cote de "Fermer"
   (fenetre reduite : petite icone fleche a cote de la croix). Le combat est rejoue DANS LE JEU, comme si on le
   regardait en spectateur : vraie carte, vrais personnages et equipements, vrais sorts et animations, au rythme
   du combat (les longues reflexions sont raccourcies a 1,5 s). Bouton "Quitter" du combat pour arreter ; a la fin,
   retour sur sa carte. Aucune commande pour les joueurs.
2. SUR LE SITE : le chat donne aussi un lien (a partager / joindre a un bug) vers replay.html?r=<code> :
   vraie image de la carte (1 962 cartes de combat rendues depuis le client, Frigost comprise ; sinon grille simple),
   vrais personnages et monstres (rendu officiel Ankama, via api/rendu.php qui les garde en cache), actions pas a pas
   (deplacements, sorts, PV, morts, invocations), lecture / pause / vitesse, journal du serveur, messages bruts.
   Les animations de sorts n'existent qu'en jeu (le site montre la trajectoire et les degats).
Le serveur enregistre tous les messages de chaque combat ; le replay est ecrit dans la table world.padgref_replays
(creee toute seule, gardee 14 jours) seulement quand quelqu'un clique sur Replay.
Admins : "replaycombat <code>" rejoue dans le jeu le replay d'un joueur (code = fin du lien du site).
Adresse du site dans les liens : http://IP_DU_VPS/ (quand padgref.fr marchera : world_config.xml,
Game > Bugs > ReplayCombat > <Variable name="SiteUrl">http://padgref.fr/</Variable>).

""" + NOTE_DLL + """

""" + NOTE_CLIENT + """

Fichiers : DLL du World ; jeu : app\\DofusInvoker.swf + app\\ui\\Ankama_Fight\\ui\\fightResult*.xml (64 et 32 bits) ;
site : replay.html, api\\replay.php, api\\rendu.php, img\\cartes\\*.webp (178 Mo).
A installer SUR LE VPS, quand personne n'est en combat : double-clic sur TOUT_INSTALLER.bat (World redemarre, jeu publie
sur le launcher avec mise a jour obligatoire, fichiers du site copies dans le htdocs).
A VERIFIER : faire un combat, cliquer "Replay" a la fin : le combat se rejoue en jeu ; le lien du chat ouvre la page du site.
Annulation : DLL dans <emulateur>\\Sauvegardes_dll\\replay-combat-1 ; site : ...\\replay-combat-1\\site.
""", client=True, xml_fight=True, dll=True, site=('replay.html', 'api/replay.php', 'api/rendu.php', 'img/cartes/*.webp'))

paquet('vote-1', 'Page de vote serveur-prive.net (15 ogrines). Site seulement.', """
PADGREF - vote-1 - 23/10/2026 (independant des autres paquets)
Demande : page de vote sur le site, toutes les 1 h 30, liee a https://serveur-prive.net/dofus/padgref/vote,
15 ogrines par vote et par IP, avec l'API OTP de serveur-prive.net.

Page vote.html (lien "Voter" dans le menu du site et en bas de page) :
  - il faut etre connecte a son compte Padgref (les ogrines vont dans le porte-monnaie du compte, comme la boutique) ;
  - "Voter maintenant" : le site demande un code OTP a serveur-prive.net (valable 5 min) et ouvre la page de vote avec
    ce code ; quand le joueur revient sur l'onglet, le site verifie le code aupres de serveur-prive.net et donne
    15 ogrines (bouton "J'ai vote" aussi) ;
  - 1 vote recompense toutes les 1 h 30 par COMPTE et par ADRESSE IP (2 comptes sur la meme IP : 1 seule recompense) ;
    compte a rebours affiche, historique des votes, mouvement "Vote serveur-prive.net" dans l'historique des ogrines ;
  - journal : table auth.padgref_votes (creee toute seule).

!! LE TOKEN DE SERVEUR-PRIVE.NET (jamais dans un paquet ni sur GitHub) !!
  Ou le trouver : sur https://serveur-prive.net, connecte-toi, va dans ton espace membre (gestion de tes serveurs),
  ouvre la fiche de Padgref > onglet "Parametres" : le "Token" de ton serveur y est affiche (meme token pour toutes
  leurs API). Ne le donne a personne.
  Ou le mettre (sur le VPS) : fichier C:\\xampp\\htdocs\\api\\config.local.php
    - s'il existe deja : ajoute   'vote' => ['token' => 'TON_TOKEN'],   juste apres "return [" ;
    - sinon cree-le avec ce contenu :
        <?php
        return ['vote' => ['token' => 'TON_TOKEN']];
  Tant que le token est vide, la page affiche "Le vote n'est pas encore ouvert sur le site".
  ENSUITE SEULEMENT (la page marche avec le token) : sur serveur-prive.net, onglet "Parametres", active l'option "OTP"
  (serveur-prive.net : ne pas l'activer avant, sinon les votes sont bloques).
Reglages possibles dans le meme 'vote' => [...] : 'ogrines' => 15, 'delai_s' => 5400 (1 h 30),
'page' => 'https://serveur-prive.net/dofus/padgref/vote'.

Fichiers du site : vote.html, api\\vote.php, assets\\padgref.js (lien "Voter" du menu), ogrines.html (nom du mouvement).
A installer SUR LE VPS : double-clic sur TOUT_INSTALLER.bat (copie les fichiers dans le htdocs ; pas de SQL, pas de
redemarrage, pas de mise a jour du jeu), puis le token ci-dessus.
A VERIFIER : se connecter sur le site > Voter > voter sur serveur-prive.net > revenir : +15 ogrines ; revoter tout de
suite : "prochain vote dans 1 h 29".
Annulation : <emulateur>\\Sauvegardes_dll\\vote-1\\site (anciens fichiers du site).
""", site=('vote.html', 'api/vote.php', 'assets/padgref.js', 'ogrines.html'))
print('fini')
