# Serveurs COMPILÉS (binaires prêts à l'emploi) — pas des sources

Ce dossier contient l'AuthServer et le WorldServer **déjà compilés** à partir des sources de `1_SERVEUR/` (même version que ce dépôt).
Ce sont des **fichiers binaires** (.NET Framework 4.8, Debug AnyCPU) : **il n'y a rien à lire ni à modifier ici**. Pour comprendre ou changer
le code, travailler dans `1_SERVEUR/` (sources C#) puis recompiler.

- Compilé le : **20/10/2026 15h36** (WorldServer, avec toutes les modifications du `JOURNAL_DES_MODIFICATIONS.txt` jusqu'à cette date).
- `AuthServer/` : `Stump.*.dll` / `.pdb`, `Stump.GUI.AuthConsole.exe` (+ `.config` de liaison des DLL), `swf_patchs/AuthPatch.swf`
  (connexion automatique du launcher).
- `WorldServer/` : `Stump.*.dll` / `.pdb`, `Stump.GUI.WorldConsole.exe` (+ `.config` de liaison des DLL).
- **Aucune configuration** (`auth_config.xml`, `world_config.xml`), aucun log, aucune donnée : garder celles de votre serveur.
- Les bibliothèques tierces (MySql.Data, NLog, Newtonsoft.Json…) ne sont pas incluses : elles ne changent pas, celles de votre serveur suffisent.

## Installer
1. Arrêter le WorldServer (après `save`) et l'AuthServer.
2. Sauvegarder vos dossiers AuthServer et WorldServer.
3. Copier `AuthServer/*` par-dessus votre dossier AuthServer et `WorldServer/*` par-dessus votre dossier WorldServer (remplacer).
4. Jouer les SQL manquants de `3_SQL_WORLD/` dans l'ordre (voir `3_SQL_WORLD/LISEZ-MOI.md`), puis relancer l'AuthServer et le WorldServer.
