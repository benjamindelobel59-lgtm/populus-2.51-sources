# Scripts SQL Mystic (base `world`)

À jouer **dans l'ordre** (01, 02, 03…) sur la base `world`, serveur World **arrêté**. Tous sont rejouables.
Ils ne contiennent que des données de jeu (donjons, PNJ, sorts de monstres, succès, trousseau…), aucune donnée de joueur.

Avec la ligne de commande MariaDB / MySQL, un fichier à la fois :

    mysql -u root -p world < 01_kolosso.sql

Avec Navicat : base `world` → Requête → coller le fichier → Exécuter.

Le serveur compilé depuis ces sources est nécessaire (certains scripts utilisent des colonnes ou types de réponse ajoutés dans le code).
