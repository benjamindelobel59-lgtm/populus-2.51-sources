# Vérifications après installation (depuis TON PC, pas le VPS)

Le script a déjà testé les adresses en local sur le VPS (bilan à la fin). Il reste à vérifier depuis l'extérieur :

- [ ] `http://IP_DU_VPS/` affiche l'accueil Mystic (pas la page de XAMPP).
- [ ] `http://IP_DU_VPS:8090/statut` répond `{"ok":true,…}`. Sinon, ouvrir le port 8090 (TCP) chez l'hébergeur.
- [ ] Bouton « Télécharger le launcher » : `MysticLauncher.exe` se télécharge.
- [ ] Site → « Mon compte » : créer un compte web, puis un compte de jeu.
- [ ] « Rattacher un compte existant » avec un ANCIEN compte de jeu : il apparaît avec ses personnages, sur le site puis dans le launcher. Un mauvais mot de passe est refusé.
- [ ] Launcher : se connecter avec le compte web, le compte de jeu apparaît ; JOUER → arrivée en jeu **sans retaper le mot de passe**.
- [ ] Launcher sur un PC sans le jeu : « Installer » télécharge tout le jeu (≈ 3,4 Go) puis JOUER fonctionne.
- [ ] Partage : sur un compte web, « Partager » donne un code ; sur un 2e compte web, « Compte partagé » + le code ajoute le compte ; « Retirer l'accès » le fait disparaître.
- [ ] Atelier → « Lier mon perso » (connecté au compte web) : tes persos apparaissent ; « Partir de ce perso » met ton stuff et ton apparence sur le podium.
- [ ] Atelier → Établi de craft : les ressources de tes sacs et banques s'affichent avec « En direct ».
- [ ] Les anciens joueurs (ancien launcher) peuvent toujours se connecter normalement.
