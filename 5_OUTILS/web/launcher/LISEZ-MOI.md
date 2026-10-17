# Launcher Dofus Nekfeu (mises à jour automatiques du client 64 bits)

Le launcher compare les fichiers modifiés du client avec ceux publiés sur le VPS (empreinte SHA256),
télécharge seulement ceux qui manquent ou ont changé, puis lance le jeu avec le bouton **Jouer**.

## Contenu de ce dossier
- `bin/NekfeuLauncher.exe` : le launcher à donner aux joueurs (15 Ko, .NET Framework 4.8, déjà présent sur Windows 10/11).
- `src/Launcher.cs` : son code source.
- `preparer_maj.ps1` : prépare une mise à jour (dossier `publication/`).
- `publication/` : ce qu'il faut copier sur le VPS (`manifest.txt` + `files/`).
- `VPS_installer_serveur_web.ps1` : installe le serveur web sur le VPS (une seule fois).

## 1. Installation sur le VPS (une seule fois)
1. Copier **les deux fichiers** `VPS_installer_serveur_web.bat` et `VPS_installer_serveur_web.ps1` dans un même dossier du VPS (Bureau à distance).
2. **Double-cliquer sur le `.bat`** (pas sur le `.ps1` : Windows ouvre les `.ps1` dans le Bloc-notes). Accepter la demande d'administrateur, attendre « Serveur web prêt », puis appuyer sur une touche.
3. Si le launcher d'un joueur dit « serveur injoignable » : ouvrir aussi le **port 80** dans le panneau de l'hébergeur du VPS.

### ⚠ VPS actuel : le port 80 est déjà pris par Apache (XAMPP)
Sur IP_DU_VPS, Apache de XAMPP répond déjà sur le port 80 : IIS ne peut pas servir `/maj/`.
→ Double-cliquer sur **`VPS_utiliser_apache_xampp.bat`** (sur le VPS) : il désactive IIS (pour qu'il ne prenne jamais
le port 80 à la place d'Apache au redémarrage) et relie `C:\xampp\htdocs\maj` à `C:\NekfeuMaj`.
Les mises à jour se copient toujours dans `C:\NekfeuMaj`.

## 2. Publier une mise à jour (à chaque fois)
1. Mettre les fichiers du client modifiés dans le paquet : `PUSH\<paquet>\6_CLIENT_64bits\app`.
2. Sur le PC : double-cliquer sur `_OUTILS\web\launcher\preparer_maj.bat` → remplit `PUSH\<paquet>\5_LAUNCHER\publication`
   et y copie `VPS_publier_maj.bat`.
3. Copier le dossier `5_LAUNCHER` du paquet sur le VPS, puis **double-cliquer sur `VPS_publier_maj.bat`** :
   il copie la mise à jour dans `C:\NekfeuMaj` (manifest en dernier).
4. Vérifier depuis n'importe quel PC : http://IP_DU_VPS/maj/manifest.txt doit afficher la nouvelle version.

⚠ Le paquet ne contient pas de `config.xml` (volontaire : le launcher ne touche pas à l'adresse du serveur des joueurs). Ne pas en ajouter un en version locale (127.0.0.1).

## 3. Pour les joueurs (une seule fois)
- Mettre `NekfeuLauncher.exe` dans le dossier du client 64 bits, **à côté du dossier `app`** (là où sont les raccourcis).
- Lancer le jeu par `NekfeuLauncher.exe` au lieu du raccourci : il met à jour puis lance `app\Dofus.exe` (préférences dans `%APPDATA%\Dofus-x64`).
- Si le serveur de mise à jour est hors ligne : bouton « Jouer quand même ».
- Journal : `launcher.log` à côté du launcher.

## Notes
- Un fichier optionnel `launcher.ini` (à côté du launcher) avec `url=http://…/` change l'adresse du serveur (utilisé pour les tests).
- `NekfeuLauncher.exe --auto` : met à jour sans fenêtre (code de sortie 0 = à jour, 1 = erreur).
- Le launcher ne supprime jamais de fichier et refuse tout chemin hors du dossier `app`.
- Pas de HTTPS : l'intégrité des fichiers est vérifiée par empreinte, mais le manifest lui-même n'est pas signé (suffisant pour une bêta).
