# Populus 2.51 — sources (serveur Mystic)

Sources de l'émulateur (Stump C#, .NET Framework 4.8) et du panel web d'administration (`panel-web/populus-admin`, PHP).

- Solution : `Populus.sln` (Visual Studio 2022 / Build Tools).
- **À lire en premier : `HANDOFF.md`** (vue d'ensemble des modifications Mystic, comment les appliquer).
- Liste détaillée des modifications Mystic : **`LISEZ-MOI.txt`** (par date, fichier par fichier).
- Fichiers du client modifiés uniquement : `CLIENT_64bits/app`, `CLIENT_32bits/app` ; nos outils : `OUTILS/`.
- Scripts SQL de la base `world` : dossier **`SQL/`** (à jouer dans l'ordre, voir `SQL/LISEZ-MOI.md`).
- Le panel ne contient pas `inc/config.local.php` : le créer sur chaque machine avec ses propres accès à la base.

Ce dépôt sert seulement à partager les sources (il n'est relié à aucun serveur). Chaque mise à jour = un commit daté.

**Tout télécharger (zip, dernière version)** : https://github.com/benjamindelobel59-lgtm/populus-2.51-sources/archive/refs/heads/main.zip
