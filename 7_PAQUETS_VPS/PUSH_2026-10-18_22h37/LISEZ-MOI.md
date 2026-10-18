# PUSH du 18/10/2026 22h37 — mise en ligne sur le VPS

Ce paquet contient **seulement ce qui a changé depuis le paquet envoyé le 18/10 à 18h46** (VPS à jour jusqu'au SQL 56).
Le **WorldServer ne change pas** : il peut rester ouvert, les joueurs en jeu ne sont pas déconnectés.

## La méthode simple : un seul fichier

1. Copier tout le dossier `PUSH_2026-10-18_22h37` sur le VPS (Bureau à distance), par exemple sur le Bureau.
   Il pèse environ 3,5 Go : ce sont surtout les fichiers du jeu pour l'installation complète par le launcher.
2. Sur le VPS, double-cliquer **`TOUT_INSTALLER.bat`** et accepter la demande de droits administrateur.
3. Le script :
   - **vérifie** :
     - les dossiers du VPS (`C:\Populus\VPS\Populus`, `C:\xampp\htdocs`) ;
     - les accès MySQL, lus dans `auth_config.xml` du VPS ;
     - PHP de XAMPP ;
   - te demande de **fermer l'AuthServer** s'il est ouvert. Ferme sa fenêtre, puis appuie sur Entrée. Le World reste ouvert.
   - **sauvegarde** dans `C:\Populus\SAVE\2026-10-18_avant-push-22h37\` :
     - l'AuthServer (DLL, configs, swf_patchs) ;
     - la base `auth` (`auth.sql`) ;
     - les fichiers du site déjà présents dans `htdocs` ;
     - les listes du launcher ;
   - applique le **SQL 57** sur la base `auth` (tables des comptes web, sessions du launcher, partage de comptes) ;
   - copie le **nouvel AuthServer** (API du launcher sur le port 8090, connexion automatique, comptes web, partage) et `swf_patchs\AuthPatch.swf`. Les configs du VPS ne sont pas touchées.
   - copie le **site** dans `C:\xampp\htdocs` :
     - les pages : accueil, Mon compte, l'Atelier « La Forge » avec ses données et icônes, l'API PHP ;
     - il met de côté le `index.php` de XAMPP (sinon il passerait avant l'accueil du site) ;
     - il crée `api\config.local.php` avec les accès MySQL du VPS et un secret aléatoire.
   - publie dans `C:\NekfeuMaj` (adresse `/maj/`) le **nouveau launcher** `MysticLauncher.exe` et l'**installation complète du jeu** (`complet.txt` + 6 338 fichiers) ;
   - ouvre le port **8090** dans le pare-feu Windows ;
   - **relance l'AuthServer** et attend que l'API du launcher réponde ;
   - **teste** chaque adresse (site, API PHP, Atelier, données, `/maj/`, téléchargement du launcher) et affiche un **bilan** `OK` / `A VOIR` / `ERREUR`.
4. Depuis **ton PC**, ouvrir `http://IP_DU_VPS/` puis `http://IP_DU_VPS:8090/statut`.
   Si la 2e adresse ne répond pas, ouvrir le port **8090 (TCP)** dans le panneau de l'hébergeur du VPS. Le pare-feu Windows, lui, est déjà fait.

Un journal complet est écrit à côté du `.bat` (`installation_<date>.log`). Le mot de passe MySQL n'y apparaît pas.

## Icônes de la barre des tâches
- Rien à faire à la main sur le VPS : le script publie le nouveau `MysticLauncher.exe` (icône Mystic) et le nouveau `Dofus.exe` (œuf Dofus) dans `/maj/`.
- Les joueurs qui passent par un launcher reçoivent `Dofus.exe` tout seuls, à la prochaine mise à jour.
- Clients installés **sans** launcher : copier `6_CLIENT_64bits\app\Dofus.exe` ou `7_CLIENT_32bits\app\Dofus.exe` par-dessus le `app\Dofus.exe` du client.
- Si Windows affiche encore l'ancienne icône : Win+R → `ie4uinit.exe -show` (rafraîchit le cache des icônes).

## Les joueurs qui avaient déjà un compte de jeu
Leurs comptes **marchent comme avant** (ancien launcher, connexion classique). Pour les avoir dans le nouveau launcher et dans l'Atelier :
1. créer un compte web sur le site (accueil → « Rejoindre ») ;
2. dans « Mon compte », bloc **« Rattacher un compte existant »** : nom de compte et mot de passe du jeu.

Le site vérifie le mot de passe comme le serveur (en clair ou ancien MD5). Un compte de jeu ne peut appartenir qu'à un seul compte web, 8 au maximum ; les comptes bannis sont refusés, et les essais sont limités (8 échecs par IP sur 15 min).

## Les redirections : tout pointe déjà vers le VPS

| Quoi | Adresse | Où c'est réglé |
|---|---|---|
| Site (accueil, Mon compte, Atelier) | `http://IP_DU_VPS/` | `C:\xampp\htdocs` (Apache de XAMPP, port 80) |
| API PHP du site | `http://IP_DU_VPS/api/…` | `htdocs\api`, base MySQL du VPS via `config.local.php` |
| Mises à jour + installation du jeu | `http://IP_DU_VPS/maj/` | lien `htdocs\maj` → `C:\NekfeuMaj` (déjà en place) |
| Téléchargement du launcher | `http://IP_DU_VPS/maj/MysticLauncher.exe` | boutons « Télécharger le launcher » du site |
| API du launcher (connexion, comptes, partage) | `http://IP_DU_VPS:8090/` | AuthServer, port 8090 |
| Jeu | Auth `IP_DU_VPS:446`, World inchangé | configs du VPS (inchangées) |

Le launcher a ces adresses **intégrées** : les joueurs n'ont rien à régler. Pas de `launcher.ini` à fournir.
Pour passer un jour à un nom de domaine, il suffira de recompiler le launcher avec la nouvelle adresse et de remplacer `MysticLauncher.exe` dans `C:\NekfeuMaj`. Le site, lui, n'utilise que des adresses relatives.

## Méthode manuelle (si le script s'arrête)

Le script s'arrête au premier problème grave et affiche ce qui manque. Rien n'est perdu, tout est dans la sauvegarde. Étapes équivalentes à la main :
1. Fermer l'AuthServer.
2. Navicat : sauvegarder `auth`, puis exécuter `2_BDD\57_auth_comptes_web_launcher.sql` sur `auth`.
3. Copier `3_SERVEUR\AuthServer\*` dans `C:\Populus\VPS\Populus\AuthServer` (remplacer).
4. Copier `4_SITE\htdocs\*` dans `C:\xampp\htdocs`, renommer `htdocs\index.php` en `index.php.xampp`, et créer `htdocs\api\config.local.php` (modèle : `4_SITE\config.local.php.modele`).
5. Copier `5_LAUNCHER\maj\*` dans `C:\NekfeuMaj`, avec `manifest.txt` en dernier.
6. Pare-feu : autoriser TCP 8090 entrant.
7. Relancer l'AuthServer.

## Annuler

Dans `C:\Populus\SAVE\2026-10-18_avant-push-22h37\` :
- remettre `AuthServer\*` dans l'AuthServer du VPS ;
- réimporter `auth.sql` (les nouvelles tables ne gênent pas l'ancien serveur, l'annulation n'est donc pas obligatoire) ;
- remettre `htdocs\index.php.xampp` en `index.php` ;
- remettre les `.txt` de `NekfeuMaj`.

Liste des fichiers : `CONTENU.md`. Texte pour les joueurs : `PATCHNOTE.md`. Contrôles en jeu : `9_VERIFICATIONS.md`.
