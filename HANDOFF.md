# HANDOFF — modifications Mystic (pour le Claude de l'autre dev)

Dépôt de **partage** des modifications faites côté Mystic (serveur Dofus 2.51, émulateur Stump C# .NET 4.8). Ce dépôt n'est relié à
aucun serveur : il sert seulement à récupérer nos changements. Le détail fichier par fichier, daté, est dans **`JOURNAL_DES_MODIFICATIONS.txt`** ;
ce fichier-ci donne la vue d'ensemble. Dernière mise à jour : **20/10/2026**.

## Contenu du dépôt
| Dossier | Contenu |
|---|---|
| `1_SERVEUR/` | sources de l'émulateur (World + Auth) : `Populus.sln`, `server/`, `core/`, `gui/`, `DofusProtocol/`, `Library/` — à compiler (VS 2022 / Build Tools, Debug AnyCPU) |
| `2_PANEL_WEB/populus-admin/` | panel web d'administration (PHP) — créer son propre `inc/config.local.php` (accès base), jamais versionné |
| `3_SQL_WORLD/` | nos scripts pour la base `world`, **à jouer dans l'ordre 01 → NN**, World arrêté, tous rejouables (voir `3_SQL_WORLD/LISEZ-MOI.md`) |
| `4_CLIENT/64bits/app`, `4_CLIENT/32bits/app` | **uniquement les fichiers du client modifiés** (même arborescence que le dossier `app` du client) |
| `5_OUTILS/` | nos scripts (générateurs SQL Python, outils client, launcher) ; `IP_DU_VPS` remplace l'IP du serveur |
| `6_SERVEURS_COMPILES/AuthServer`, `…/WorldServer` | nos serveurs **déjà compilés** (fichiers `Stump.*` + `swf_patchs` de l'Auth) : à copier par-dessus les dossiers des serveurs, serveurs arrêtés. **Aucune config** : garder les vôtres |
| `JOURNAL_DES_MODIFICATIONS.txt` | détail daté, fichier par fichier, de toutes les modifications |

## Appliquer
1. Copier `6_SERVEURS_COMPILES/AuthServer/*` et `6_SERVEURS_COMPILES/WorldServer/*` dans les dossiers des serveurs (serveurs arrêtés) — ou compiler soi-même `1_SERVEUR/gui/Stump.GUI.WorldConsole` et `Stump.GUI.AuthConsole`.
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
- **Donjons (17/10 soir)** : reprise de donjon chez les gardiens (`game/Donjons/RepriseDonjons.cs`, table `nekfeu_reprise_donjon`, réponse `DungeonResume`, SQL 34, plus de téléportation de groupe) ; Boostache inébranlable + lourd (`BoostacheBrain`, SQL 35) ; Kimbo / Chalœil / Reine des Voleurs / Missiz Frizz corrigés (zone `D` = `Damier.cs`, cases fixes multiples, `DispelState`) ; fin de combat plus rapide (`ReadyChecker` 800 ms).
- **Métiers / ateliers (17/10 soir)** : tous les métiers niveau 200 à la création (`CharacterManager.cs`) et pour les persos existants (SQL 37) ; postes de forgemagie remis à leur vrai type d'après leur image (SQL 36) ; concasseurs sans compétence + ligne sans texte (SQL 38).
- **Forgemagie (17/10 soir)** : règles officielles dans `RuneMagicCraftDialog.cs` (limite 101 de poids par ligne, exo de poids ≥ 30 à 1 %, reliquat = effet 351 exclu du poids et des lignes qui tombent) ; poids Do Ren / Pi / Chasse = 5 (`EffectManager`) ; reliquat perdu à l'échange / HDV / monture. Client : écran de forgemagie (module Job : reliquat ± variation, case « Mode avancé », couleurs over / zéro, historique coloré + « Vider l'historique ») et poids des runes dans l'infobulle (module Tooltips), sources dans `5_OUTILS/client/client-build/forgemagie/src` ; `Ankama_Job.d2ui` sans la fenêtre `smithMagic` (`d2ui_retirer_ui.py`) pour que le client lise le XML. ⚠ Ne jamais modifier un fichier de `content/themes/darkStone` : `signature.xmls` vérifie leur taille (client bloqué à 89 %) — styles dans les nouveaux `mysticNormal2.css` / `mysticSmithMagic.css`.
- **Brisage (17/10 soir)** : `game/Exchanges/Craft/Runes/BrisageOfficiel.cs` : poids de ligne = (valeur × poids × niveau × 0,015 + 1) × coefficient, runes de base ; coefficient par objet (table `nekfeu_brisage_coef`) départ 4000 %, ×0,99 par objet brisé, remontée de 1 % de l'écart par heure, sans recette 50 % ; focus pris en compte (`FocusedExchangeReadyMessage` → `RuneTrader.Focus`) : runes de la stat focus seulement, 100 % de sa ligne + 50 % des autres.
- **Fluidité / capacité (17/10 soir, ton document « Prompt-fluidite-serveur.md »)** : appliqué tel quel, sections 1 à 8 (`BufferManager.TraceSegments`, `SegmentStream`, `WorldClientCollection` → `SendPacked`, file d'envoi groupée dans `BaseClient`, BasicNoOperation au seul joueur dans `Map`, `Character.LogOut` / `RejoinMap`, garde dans `ContextRoleplayHandler`, limites 6000 / 1000 / 5000). Base `auth` : `3_SQL_WORLD/39_auth_capacite_monde.sql` (⚠ à jouer sur **auth**, pas world). Configs à changer à la main (LogPackets False, etc.). Section 9 : nos clients étaient déjà en `renderMode` direct.
- **Icônes d'état (17/10 soir)** : Lourd / Enraciné / Indéplaçable / Inébranlable au-dessus de la tête (`SpellStates.d2o` + 3 symboles inutilisés de `stateBuffs.swf` redessinés, `d2o_state_icon.py`).

- **Launcher Mystic + comptes web (18/10)** : API HTTP dans l'AuthServer (`server/Stump.Server.AuthServer/Launcher/LauncherApi*.cs`, port 8090, variables `LauncherApi.Actif/Port/BaseMonde/DureeSessionJours`) :
  - comptes web (PBKDF2), sessions, ticket de connexion à usage unique (`CredentialManager` : mot de passe `#mystic-ticket:<ticket>`, envoyé par le `swf_patchs/AuthPatch.swf` modifié) ;
  - partage de comptes par code à 6 chiffres (tables `nekfeu_partages`, `nekfeu_partages_codes`) ;
  - les personnages sont trouvés via `auth.worlds_characters`, car le World laisse `characters.AccountId` à 0.
  - Tables : `3_SQL_WORLD/57_auth_comptes_web_launcher.sql` (⚠ base **auth**).
  - Sources du launcher WPF : `5_OUTILS/web/launcher/src_mystic` (compilé avec `csc`, icône `-win32icon`).
- **Atelier du site (18/10)** : exporteur base → JSON `5_OUTILS/web/atelier/exporteur` (C#, x86, relit les effets BinaryFormatter avec les DLL du serveur) + `extraire_icones.py`.
- **Icône de Dofus.exe (18/10)** : `5_OUTILS/client/icone_exe` (remplace l'icône d'un exe par l'API Windows) ; `4_CLIENT/*/app/Dofus.exe` = exe avec l'œuf Dofus.

- **Mise à jour du 19-20/10** (SQL 70 à 79, tous sur `world`) :
  - **Chasses au trésor** : `game/Chasses/` (`ChasseManager.cs`, `Autopilote.cs` = trajet sur la minimap via `CompassUpdatePartyMemberMessage`, départ autour d'un zaap au hasard, 5 min d'attente après abandon), commande `.chasse` ; SQL 70-71.
  - **Chasses aux portails** : `game/Chasses/Portails.cs` (`PortailManager`, table `mystic_portails`, 8 h ou 110 passages, sans combat) ; SQL 72.
  - **Objets infinis** 31200-31204 : `game/items/player/Custom/ObjetsInfinis.cs` ; SQL 73. Potion de Chasseur de Trésor normale : SQL 74.
  - **Frigost** : transporteur Asfog (Sakaï, `AccesFrigost`) SQL 75 ; maisons du Village enseveli selon l'accès à la zone.
  - **Maison-atelier des forgemages d'Astrub** [5,-17] (porte verte, `SkillHouse.MaisonsPubliques`) : SQL 76-78 + `maps0.d2p` / `Hints.d2o` du client ; forgemagie limitée au niveau 80 dans cet atelier (`RuneMagicCraftDialog`).
  - **Forgemagie simplifiée** (`RuneMagicCraftDialog.cs` : table `PALIERS_STATS`, part critique 0,30, pertes × 0,5).
  - **Collecteurs** des métiers de récolte : `game/Jobs/Collecteurs.cs` + `CollecteursTable.cs` (79 ressources → protecteur officiel + sac, 1 chance sur 100).
  - **Balance AvA** : `game/Alliances/BalanceAvA.cs` (marqueur `§BAL§` dans le message du jour, %) + client (onglet Balance du module Social, `alliance.xml`).
  - **Maisons** : un intérieur par maison (`HouseManager.CanBeSeenInHouse`).
  - **Combat / IA** : `PassifsAuditDonjonsBrains.cs` (Wa Wobot, Tamponneurs, Kolérat, Nelween), `MonstresFuyards.cs` + `Brain.FuirAPortee` (139 monstres à distance), résistances des monstres jusqu'à 100 %, monstres lourds, Duo / Trio de boss.
  - **Audit des cartes** : `5_OUTILS/serveur/audit_cartes` ; SQL 79 = 287 sorties réparées.
  - **Launcher** : `5_OUTILS/web/launcher/src_mystic/Fantomes.cs` (ferme les Dofus restés en mémoire sans fenêtre).

## Points connus non faits
- Pas de système d'alliances dans l'émulateur (livre du temple inopérant).
- Changement d'apparence en combat (masques Zobal, Karcham) qui fige le client : non reproduit côté serveur.

## 20/10/2026 14h00 — paquet PUSH_2026-10-20_14h00
- Sources à jour avec ton src.rar du 20/10 (déjà fusionné chez nous) + nos changements : AuthServer (connexion du launcher grâce au site `/web/appairage`, comptes de jeu sans mot de passe `/web/creer_jeu`, migration des anciens comptes du VPS à la 1re connexion, apparence sans dragodinde), nom **Padgref** (bannière console + titres des fenêtres ; les tables `mystic_*` ne changent pas).
- `6_SERVEURS_COMPILES` = ces serveurs compilés (AuthServer + WorldServer). Aucun SQL nouveau (dernier : 79).
- Côté VPS : dossiers renommés `D:\Padgref\Launcher - mises a jour` (ex-NekfeuMaj, lien `htdocs\maj` refait) et `D:\Padgref\Bot Discord` (bot relié au journal du World `admin_journal_events`).
- **Ogrines du compte web + Boutique Padgref (20/10 15h41)** : les ogrines sont sur le compte web (`auth.nekfeu_comptes_web.Ogrines`, `server/stump.server.worldserver/game/Ogrines/OgrinesWeb.cs`), plus d'objet ogrines en jeu (`Inventory.ActiveTokens = false`), anciens `accounts.Tokens` repris une fois à la connexion. Boutique du client 2.51 (ex « Hôtel de vente d'Artemis ») : `game/Ogrines/BoutiquePadgref.cs` + `Handlers/Approach/ApproachHandler.cs` (jeton `HaapiTokenRequestMessage`, fonction optionnelle 7, livraisons `world.nekfeu_boutique_livraisons`) ; l'API JSON-RPC est sur le site (`boutique.php`) ; client patché (`DofusInvoker.swf` : `RpcServiceCenter.apiDomain` → `http://<connection.host>/boutique.php?x=` ou réglage `padgref.boutique` du config.xml ; module Web : onglet Boutique seul ; `i18n_fr.d2i` : « Boutique Padgref »), sources dans `5_OUTILS/client/client-build/boutique_padgref`. Tables créées toutes seules, pas de SQL.
- **Correctifs 20/10 16h22-16h28** : boutique en jeu sans le « mode restreint » Ankama (`5_OUTILS/client/client-build/boutique_padgref/src/WebBase.as` : `shieldUp = true`) ; launcher Padgref (`5_OUTILS/web/launcher/src_mystic` : `PadgrefLauncher.exe` installé en `%LOCALAPPDATA%\Padgref\Launcher\Padgref.exe`, vrai installeur avec raccourcis, connexion gardée quand l'API ne répond pas, `launcher.ini` plus jamais recopié) ; site : lien de téléchargement `/maj/PadgrefLauncher.exe`.
