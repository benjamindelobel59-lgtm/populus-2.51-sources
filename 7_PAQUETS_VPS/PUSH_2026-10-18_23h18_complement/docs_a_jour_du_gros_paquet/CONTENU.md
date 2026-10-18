# Contenu exact du paquet PUSH_2026-10-18_22h37
(tout ce qui a changé depuis le paquet envoyé le 18/10 18h46 — rien de ce paquet-là n'est repris)

## TOUT_INSTALLER.bat + outils/tout_installer.ps1
Installation automatique sur le VPS, avec sauvegarde, tests et bilan. Voir `LISEZ-MOI.md`.

## 2_BDD (base `auth`)
- `57_auth_comptes_web_launcher.sql` - tables `nekfeu_comptes_web`, `nekfeu_comptes_web_jeu`, `nekfeu_web_sessions`, `nekfeu_partages`, `nekfeu_partages_codes` (CREATE IF NOT EXISTS)

## 3_SERVEUR/AuthServer (build `launcher-auth5`, 18/10 22h53)
- DLL `Stump.*` + `Stump.GUI.AuthConsole.exe`. Toutes du même build (envoyées ensemble pour éviter un mélange de versions) :
  - API du launcher (`Launcher/LauncherApi*.cs`, port 8090) : connexion au compte web, session gardée, ticket à usage unique, statut du serveur ;
  - connexion automatique en jeu par ticket (`CredentialManager`) ;
  - partage de comptes par code (créer, annuler, utiliser, lister, retirer, côté partageur ou receveur) ;
  - les personnages sont trouvés par `auth.worlds_characters`, car le serveur laisse `characters.AccountId` à 0. Sans cela, le launcher n'afficherait aucun personnage.
- `swf_patchs/AuthPatch.swf` - patch de connexion envoyé au client, qui transmet le ticket du launcher.
- Configs : **aucune** (celles du VPS restent). Les réglages de l'API ont des valeurs par défaut (port 8090, base `world`).
- WorldServer : **inchangé** (build `fm7` déjà sur le VPS).

## 4_SITE/htdocs → C:\xampp\htdocs (site jamais envoyé jusqu'ici)
- `index.html`, `compte.html`, `assets/` (style, app, compte) : site Mystic, inscription au compte web, bouton « Télécharger le launcher » → `/maj/MysticLauncher.exe`.
  - « Mon compte » : créer ses comptes de jeu, et **rattacher un compte existant** (nom de compte + mot de passe du jeu) pour les joueurs d'avant les comptes web.
  - Les personnages sont aussi trouvés par `auth.worlds_characters`.
- `atelier.html`, `assets/atelier.*` : l'Atelier « La Forge » :
  - création de stuff, panoplies, établi de craft avec stock en jeu, recherche, bestiaire, codex, armoire ;
  - partage de build par lien ;
  - « Lier mon perso ».
- `atelier/donnees/*.json` (≈ 8 Mo) : objets, panoplies, recettes, monstres, exportés de la base du serveur (`_OUTILS/web/atelier/exporteur`).
- `atelier/icones/` (≈ 41 Mo) : icônes des objets, des monstres, des caractéristiques et des classes, extraites du client.
- `api/` :
  - `compte_web.php` (comptes web), `atelier.php` (lier mon perso, lecture seule), `statut.php`, `classement.php` ;
  - `inscription.php` (renvoie 410 : l'inscription passe par le compte web), `commun.php`, `config.php`, `.htaccess`.
  - **Sans** `config.local.php` (créé sur le VPS par le script, modèle dans `4_SITE/config.local.php.modele`).
- `img/` : images du site.

## 5_LAUNCHER/maj → C:\NekfeuMaj (servi à /maj/)
- `MysticLauncher.exe` - nouveau launcher Mystic :
  - connexion au compte web, comptes groupés, apparence des persos, connexion automatique ;
  - partage de comptes, installation complète du jeu ;
  - adresses du VPS intégrées.
- `complet.txt` - liste de l'installation complète (6 875 fichiers, ≈ 3,4 Go ; `config.xml` réglé sur le VPS).
- `manifest.txt` - celui du VPS + `Dofus.exe` avec l'icône Dofus (version 18/10 23h15, 413 fichiers).
- `files/` - les 6 338 fichiers du jeu absents du VPS (≈ 3,37 Go) ; les 98 déjà envoyés ne sont pas repris.

## Icônes (18/10 23h15)
- `MysticLauncher.exe` compilé avec l'icône Mystic (exe, fenêtres, barre des tâches).
- `Dofus.exe` avec l'œuf Dofus officiel (icône intégrée dans l'exe par `_OUTILS/client/icone_exe/IconeExe.exe` ; le jeu démarre normalement).
  - Dans `5_LAUNCHER/maj` : ajouté à `manifest.txt` (les joueurs qui ont déjà le jeu le reçoivent à la prochaine mise à jour) et à `complet.txt`.

## 6_CLIENT_64bits / 7_CLIENT_32bits
- `app/Dofus.exe` (avec l'icône Dofus) : pour les clients installés à la main, sans launcher. À copier par-dessus `app\Dofus.exe`.
