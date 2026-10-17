# HANDOFF — modifications Mystic (pour le Claude de l'autre dev)

Dépôt de **partage** des modifications faites côté Mystic (serveur Dofus 2.51, émulateur Stump C# .NET 4.8). Ce dépôt n'est relié à
aucun serveur : il sert seulement à récupérer nos changements. Le détail fichier par fichier, daté, est dans **`JOURNAL_DES_MODIFICATIONS.txt`** ;
ce fichier-ci donne la vue d'ensemble. Dernière mise à jour : **17/10/2026**.

## Contenu du dépôt
| Dossier | Contenu |
|---|---|
| `1_SERVEUR/` | sources de l'émulateur (World + Auth) : `Populus.sln`, `server/`, `core/`, `gui/`, `DofusProtocol/`, `Library/` — à compiler (VS 2022 / Build Tools, Debug AnyCPU) |
| `2_PANEL_WEB/populus-admin/` | panel web d'administration (PHP) — créer son propre `inc/config.local.php` (accès base), jamais versionné |
| `3_SQL_WORLD/` | nos scripts pour la base `world`, **à jouer dans l'ordre 01 → NN**, World arrêté, tous rejouables (voir `3_SQL_WORLD/LISEZ-MOI.md`) |
| `4_CLIENT/64bits/app`, `4_CLIENT/32bits/app` | **uniquement les fichiers du client modifiés** (même arborescence que le dossier `app` du client) |
| `5_OUTILS/` | nos scripts (générateurs SQL Python, outils client, launcher) ; `IP_DU_VPS` remplace l'IP du serveur |
| `JOURNAL_DES_MODIFICATIONS.txt` | détail daté, fichier par fichier, de toutes les modifications |

## Appliquer
1. Compiler `1_SERVEUR/gui/Stump.GUI.WorldConsole` et `Stump.GUI.AuthConsole`, copier les `Stump.*` dans les dossiers des serveurs (serveurs arrêtés).
2. Base `world` : jouer `3_SQL_WORLD/01_…` puis les suivants dans l'ordre. Certaines tables sont créées au démarrage du World (`nekfeu_trousseau`, `nekfeu_ia_soins`).
3. Client : copier le contenu de `4_CLIENT/64bits/app` (ou `32bits/app`) dans le dossier `app` du client.

## Modifications par thème (fichiers principaux)
- **Donjons** : Kolosso officiel avec reliques (`DungeonSpawningPool`, colonnes `RewardItemId/RewardMax`, critère `POQ<`) ; salles de sortie copiées après les boss (maps 990000001-12) ; salles 8 contre 8 (8 cases par équipe, monstres jusqu'à 8 avant le boss, salles de boss inchangées) — SQL 01, 04, 05, 08-10.
- **Portes / escaliers / zaaps** : interactifs reconstruits d'après le `worldgraph.bin` du client (SQL 03, 07), arrivée à côté des portes (`Map.GetArrivalCell`).
- **Trousseau de clefs** (`game/items/player/Custom/TrousseauManager.cs`, `Trousseau.cs`, `Database/Npcs/Replies/DungeonEnterReply.cs`) : objet non échangeable donné à tous ; réponse officielle « Utiliser le trousseau de clefs » chez les gardiens (`DungeonEnterTrousseau`, visible seulement si la clef est encore dans le trousseau) ; une entrée gratuite par donjon, reset 7 jours, état en base `nekfeu_trousseau`. Vrais gardiens : « Donner la clef » / « Utiliser le trousseau » (SQL 13, 14).
- **Recherchés** (`game/Recherches/RecherchesManager.cs`, table `nekfeu_recherches`, SQL 11) : 1 par zone, seul, réapparition 4-6 h ; succès « On recherche … » (ids 6001+) avec jetons (Doplons / Kamas de glace / Alitons), niveau minimum ; succès ajoutés aussi au client (`Achievement*.d2o`). Commande staff `.recherches`.
- **IA des monstres** (`AI/Fights/Brain/Brain.cs`, `AI/Fights/Spells/SpellSelector.cs`, `SpellIdentifier.cs`) : enchaîne les sorts, effets « lance le sort X » évalués avec le vrai sous-sort, soins en %, soin d'urgence (seuil par monstre : table `nekfeu_ia_soins`, commande `.seuilsoin`, 50 % par défaut), invocations des joueurs qui visent d'abord monstres / personnages. SQL 12 : 493 sorts de monstres remis au bon niveau (ils lançaient un sous-sort interne).
- **One Window / barre** : barre d'icônes (Groupe, One Window, IP Drop, Sasa auto `.sasa`, Passe-tour `.passturn`, Contrôle des invocations `.invo`) avec état gris / vert lu dans les messages serveur (module client `Ankama_Config`, sources dans `OUTILS/client/client-build/sources_module_config`) ; échange entre persos de même IP ouvert directement ; invocations contrôlées par défaut (One Window : depuis la fenêtre du maître), fiche de l'invocation corrigée (PO).
- **Combat** : drop = taux sur la chance seulement (1 tirage par objet) ; kamas 0-250 au hasard par personnage (`FightPvM`, variable `MaxKamasCombat`) ; relance initiale des sorts envoyée au maître One Window.
- **Mécaniques de donjons (17/10)** : audit `AUDIT_MECANIQUES.md` ; règle = données 2.51. Moteur : zones case fixe `;NNN` (`Database/Spells/CasesFixesSorts.cs` + `cases_fixes.tsv` à côté des DLL, `5_OUTILS/.../generer_cases_fixes.py`), zone `Z` (`Shapes/CercleInverse.cs`), confusion 770-775 (`Others/Confusion.cs`), masque `W` (`Targets/CaseInvalideCriterion.cs`), effets 1019/1021/1022/407/1024/1189/1063/1034/1067-1069/220/240-244/418/420. Passifs officiels posés par `SortPassif` : `PassifsDonjonsBrains.cs`, `FrigostOtomaiBrains.cs` (Comte, Merkator, Dramak, Moon), `DimensionsBrains.cs` (Chalœil, Ush, Ombre, Nidas, Baleine), `DonjonsRecentsBrains.cs` (Dantinéa, Koutoulou, Meno). SQL 26 : Tynril (élément par Tynril), Ush noir. Déclencheur `MA` (cible attirée, `BuffTriggerType.OnPulled`) ; effets 1124 / 1125 = renvoi d'un % des dommages bruts subis en Neutre / Air (`Damage/RenvoiDommagesBruts.cs`).
- **Signaler un bug (17/10)** : `.bug` / icône de la barre → `game/Bugs/` (journal de combat, jeton `nekfeu_bugs`) → `populus-admin/bug.php` (formulaire, log du client envoyé par `PanelWindow.envoyerLog`) → webhook Discord `bug_webhook` (dans `config.local.php`, jamais versionné).
- **Panel web** : interactifs éditables, cases en direct (`nekfeu_map_cells_live`).

## Points connus non faits
- Pas de système d'alliances dans l'émulateur (livre du temple inopérant).
- Changement d'apparence en combat (masques Zobal, Karcham) qui fige le client : non reproduit côté serveur.
