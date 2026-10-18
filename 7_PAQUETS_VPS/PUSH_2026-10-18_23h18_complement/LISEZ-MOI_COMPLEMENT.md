# COMPLÉMENT du 18/10/2026 23h18 (≈ 27 Mo) — à ajouter au gros paquet PUSH_2026-10-18_22h37

Le gros paquet est déjà sur le VPS. Ce complément contient **seulement** ce qui a changé après sa copie :

| Quoi | Pourquoi c'est important |
|---|---|
| `3_SERVEUR\AuthServer` (build `launcher-auth5`) | **Sans lui, le launcher n'affiche aucun personnage sur le VPS** : le serveur laisse le numéro de compte des persos à 0, ils sont maintenant trouvés par `auth.worlds_characters`. |
| `4_SITE\htdocs` : `compte.html`, `assets\compte.js`, `api\compte_web.php`, `index.html` | « **Rattacher un compte existant** » dans Mon compte, pour que les joueurs d'avant les comptes web retrouvent leurs comptes et persos ; les persos sont aussi trouvés par `worlds_characters`. |
| `5_LAUNCHER\maj` : `MysticLauncher.exe`, `manifest.txt`, `complet.txt`, `files\aeb68458…bin` | Launcher avec l'**icône Mystic** ; `Dofus.exe` avec l'**œuf Dofus**, envoyé aux joueurs par la mise à jour. |
| `6_CLIENT_64bits\app\Dofus.exe`, `7_CLIENT_32bits\app\Dofus.exe` | Seulement pour les clients installés à la main, sans launcher. |
| `docs_a_jour_du_gros_paquet\` | LISEZ-MOI, CONTENU, PATCHNOTE et VÉRIFICATIONS du gros paquet à jour (pour remplacer ceux du gros paquet, si tu veux). |

## Cas 1 : tu n'as PAS encore lancé TOUT_INSTALLER.bat
1. Copie le **contenu** de ce dossier **par-dessus** `PUSH_2026-10-18_22h37` sur le VPS (Windows demande de remplacer : **Oui pour tout**).
2. Lance `TOUT_INSTALLER.bat` du gros paquet : il installe tout, complément compris.
   (`COMPLEMENT.bat` sera aussi copié dedans, ne le lance pas.)

## Cas 2 : tu as DÉJÀ lancé TOUT_INSTALLER.bat
1. Copie ce dossier sur le VPS (n'importe où).
2. Double-clique **`COMPLEMENT.bat`** (droits administrateur) :
   - il sauvegarde dans `C:\Populus\SAVE\2026-10-18_avant-complement-23h18\` ;
   - il te fait fermer l'AuthServer (le World reste ouvert) ;
   - il copie l'AuthServer, les 4 fichiers du site, le launcher et `Dofus.exe` ;
   - il relance l'Auth, teste et affiche un bilan.
   Si le gros paquet n'est pas encore installé, il s'arrête tout seul et te dit de faire le cas 1.

## Clients installés sans launcher
Copier `6_CLIENT_64bits\app\Dofus.exe` ou `7_CLIENT_32bits\app\Dofus.exe` par-dessus le `app\Dofus.exe` du client. Ceux qui utilisent un launcher n'ont rien à faire.

## Annuler
Remettre les fichiers de `C:\Populus\SAVE\2026-10-18_avant-complement-23h18\` à leur place.
