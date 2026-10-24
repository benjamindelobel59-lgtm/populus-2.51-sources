# AUDIT DPLN — Sentence de la Balance (boss Misère, Eliocalypse) — 23/10/2026

Source : https://www.dofuspourlesnoobs.com/sentence-de-la-balance.html (article d'Arkaw et Los, serveur Ush, mis en ligne le 09/12/2019, mis à jour le 17/06/2020, avec des notes sur les MàJ 2.67 / 2.68).
Notre état : `audit/etat_sentence_de_la_balance.md` + vérifications directes dans les données 2.74 (`_OUTILS/client/client_27x/data`), notre d2o converti (`eliocalypse/client/data/common/SpellLevels.d2o`), le SQL 97 / 99 / 113 et le code du serveur (`SERVEUR_Emu`).
Fichiers de travail : `audit/dpln_sentence/` (page.html, texte.txt = texte brut extrait, images.txt, img/ = 44 images du donjon + img/site/ = 11 images génériques du site, crop_* = agrandissements faits pour lire les timelines et la recette).

Aucun fichier du jeu n'a été modifié.

> Note : la retranscription ci-dessous est une reformulation structurée et exhaustive des **informations** de la page (chiffres, règles, conseils), pas une copie mot à mot. Le texte brut complet est dans `audit/dpln_sentence/texte.txt` (copie locale de travail).

---

## (a) Retranscription structurée de la page

### Sommaire (image `dj6-misere-sommaire_orig.jpg`)
Bannière : Misère sur son vautour (illustration), salle du boss en fond (balances à pointes, cages suspendues, tas de crânes). Plan : I Introduction ; II États des monstres ; III Monstres ; IV Salles ; V Boss (sorts, principe, conseils, stratégie) ; VI Succès (Hardi, Mains Propres, Spécial, Duo — + Score 200 dans le texte).

### I. Introduction (image `dj6-1_orig.jpg`)
- **Entrée** : zone Désert de Misère, carte **[1,0]**. La capture montre le PNJ **Bree Caule** (encadré, à gauche de l'entrée), l'icône « Sentence de la Balance (Niv. 200) » sur [1,0] et la fiche de la clé.
- **Quêtes liées** : « Avis de tempête », « Les totems de Maïmane ».
- **Clé (texte)** : 1 Perce-Neige, 1 Poisskaille, 1 Frostiz, 2 Viande Goûtue, 2 Capuche de Skentu, 2 Oeil de Dawaj, 2 Côtes de Désosseur, 2 Langue de Krèvladal.
- **Clé (image, ancienne version)** : « Clef de la Sentence de la Balance », niv. 200, 800 XP ; 8 ingrédients : plante blanche (Perce-Neige) ×1, viande roulée ×7, épi doré ×1, objet sombre ×7, capuche ×5, oeil ×5, os ×5, langue ×5 → quantités différentes du texte (image antérieure).
- **Accès** : zaap Arche de Vili [15,-20], via la faille dans la Mine Hable [10,-19].
- **Misère ne peut pas être capturée.**

### II. États des monstres
- **Collecte** (infini) — icône `dj6-etat-collecte-fixed_orig.png` : petite icône ronde dorée (main tenant un sac). Un monstre l'obtient dès qu'il utilise un sort qui vole des caractéristiques (presque tous). Dans la salle du boss, Misère retire l'état aux monstres **dans sa ligne de vue à son début de tour** et leur prend une partie du vol.
  - Dawaj : critique + Force ; Désosseur : PM + Agilité ; Ferrailleur : Vitalité + Dommages ; Krèvladal : PA + Chance ; Skentu : PO + Intelligence.
  - Les estimations de frappe sont « de base » : chaque vol les rend plus forts.
- **Thésaurisation** : toute tentative de retrait PA / PM / PO sur un monstre lui donne **+10 esquive PA/PM** (cumulable ; « 2 tours » dans l'intro, « infini » dans la partie boss).
- **Faim du Monde** : tous les monstres ont « Charognard » → états **Nécrophage Affamé** (infini) et **Charognard** (infini) ; **+4 PM pour 1 tour à chaque tour**. Si une **Charogne est à 4 PO ou moins** du monstre à son début de tour, il ne gagne pas ces +4 PM (il la consomme).

### III. Monstres (légende : case verte = lanceur, bleu = portée, rouge = zone)
Les 4 fiches de monstres montrent (capture « fiche de combat ») : PV, PA, PM puis une ligne de 5 valeurs (jaune / bleu / vert / rouge / orange : 80-81, 64-65, 64-65, 0, 0 — esquive/tacle dérivés de la sagesse et de l'agilité) et les résistances.

#### Dawaj — `dj6-dawaj_orig.png` : niv. 203, 6200 PV, 12 PA, **4 PM** ; 81 / 65 / 65 / 0 / 0 ; rés. neutre 29 %, terre 36 %, feu 18 %, eau -6 %, air 11 %.
- **Fouet Gadderfi** (`dj6-fouet-gadderfi_orig.png` : croix de portée 5 cases dans les 4 directions, cible rouge en bout de ligne, flèches 1 = repousse, 2 = attire) : ~150 feu, repousse 2 puis attire 4, vole 15 % critique (cumul, infini) ; dommages de poussée → état **Pacifiste** 1 tour ; Collecte ; **en ligne, jusqu'à 5 PO, 3/tour**.
- **Sol Aride** (`dj6-sol-aride_orig.png` : lanceur + 4 cases adjacentes en bleu, zone rouge = ligne de **6 cases** partant de la case adjacente) : ~200 terre en **vol de vie**, touche **alliés et ennemis**, vole 200 Force (cumul, infini) ; Collecte ; **au càc, 1/tour**.

#### Désosseur — `dj6-desosseur_orig.png` : niv. 200, 6000 PV, 12 PA, **4 PM** ; 80/64/64/0/0 ; rés. neutre 13, terre 21, feu -10, eau 19, air 37.
- **Désossage** (`dj6-desossage_orig.png` : losange de portée 2) : ~300 eau, vole 1 PM (cumul, infini) ; Collecte ; **jusqu'à 2 PO, 2/tour**.
- **Souffle du Désert** (`dj6-souffle-du-desert_orig.png` : losange de portée 3, « 1 » = lanceur, « 2 » = case d'arrivée collée à la cible) : se **téléporte au càc** de la cible, ~150 air en vol de vie, vole 200 Agilité ; Collecte ; **jusqu'à 3 PO sans ligne de vue, 3/tour**.

#### Ferrailleur — `dj6-ferrailleur_orig.png` : niv. 203, 6200 PV, 12 PA, **4 PM** ; 81/65/65/0/0 ; rés. neutre 34, terre -8, feu 17, eau 15, air 26.
- **Assembricolage** (`dj6-assembricolage_orig.png` : losange rouge de rayon 2 autour du lanceur) : bouclier ~650 pour lui et ses alliés (rayon 2) pendant 2 tours, **cumul max 2**, **relance 3 tours**.
- **Démantèlement** (`dj6-demantelement_orig.png` : càc) : ~300 neutre, retire 350 vitalité à la cible et gagne 650 vitalité (infini) ; Collecte **1 tour** ; **càc, 2/tour**.
- **Terminal Gris** (`dj6-terminal-gris-fixed_orig.png` : croix de portée 5) : ~200 neutre en vol de vie, vole 50 dommages (infini), **attire 4** ; Collecte ; **en ligne jusqu'à 5 PO, 3/tour**.

#### Krèvladal — `dj6-krevladal_orig.png` : niv. 200, 6000 PV, 12 PA, **4 PM** ; 80/64/64/0/0 ; rés. neutre 16, terre 19, feu 23, eau 35, air -11.
- **Crache-Misère** (`dj6-crache-misere_orig.png` : losange de portée 3, zone croix de 1 avec flèches de poussée vers l'extérieur sur les 3 branches) : ~200 eau en vol de vie en croix 1, repousse de 1 les personnages au càc de la cible, vole 200 Chance ; Collecte ; **jusqu'à 3 PO, 3/tour**.
- **Pouilleux Massacreur** (`dj6-pouilleux-massacreur_orig.png` : càc) : ~100 feu, vole 1 PA (cumul, infini) ; Collecte ; **càc, 2/tour**.

#### Skentu — `dj6-skentu_orig.png` : niv. 200, 6000 PV, 12 PA, **4 PM** ; 80/64/64/0/0 ; rés. neutre -15, terre 9, feu 34, eau 18, air 24.
- **Balle Vorace** (`dj6-balle-vorace_orig.png` : grand losange de portée 8 avec trou au centre = PO min 3) : ~250 feu en vol de vie, vole 200 Intelligence ; Collecte ; **3 à 8 PO, PO modifiable, 3/tour**.
- **Bantha** (`dj6-bantha-fixed_orig.png` : càc, zone rouge = ligne perpendiculaire de 3 cases) : ~200 air, repousse 1 ; **pas de vol, pas de Collecte** ; **càc, 1/tour**.
- **Grenade Aveuglante** (`dj6-grenade-aveuglante-fixed_orig.png` : portée 3-8, zone croix de 1) : ~400 feu en croix, vole 2 PO ; Collecte ; **3 à 8 PO sans ligne de vue, 1/tour**.

### IV. Salles (4 captures en mode tactique ; numéros = ordre de jeu ; rouge = placement joueurs, bleu = monstres ; timelines agrandies dans `img/crop_timeline_map*.png`)
| Salle | Image | Monstres vus (4 à chaque fois) | Joueurs |
|---|---|---|---|
| 1 | `dj6-map1_orig.jpg` | Skentu (1), Désosseur (3), Krèvladal (5), Krèvladal (7) | 4 |
| 2 | `dj6-map2_orig.jpg` | Dawaj (1), Skentu (3), Skentu (5), Ferrailleur (7) | 4 |
| 3 | `dj6-map3_orig.jpg` | Dawaj (1), Désosseur (3), Désosseur (5), Krèvladal (6) | 2 |
| 4 | `dj6-map4_orig.jpg` | Dawaj (1), Skentu (3), Ferrailleur (5), Ferrailleur (7) | 4 |
Cartes : désert, murets de sable, trous ; salle 1 avec 2 rangées de murets ; salle 4 coupée en deux par un long muret. 4 monstres même à 2 joueurs (salle 3).

### V. Boss — Misère
- **Salle du boss** (`5ark1mpazr_orig.jpg`) : grande salle carrelée (cages suspendues, balances géantes à pointes, tas de crânes, 4 bacs de plantes / blocs). Monstres sur cases orange : **Misère** (en haut), **Krèvladal** (gauche), **Ferrailleur** (droite, contre le tas de crânes), **Désosseur** (bas droite) + **4 cases orange vides** (ajouts 5-8 joueurs). Cases bleues de placement joueurs à gauche (un joueur dessus).
- **Fiche** (`dj6-misere-fixed_orig.png`) : niv. 220, **18000 PV**, **20 PA**, **6 PM** ; 110 / 72 / 72 / 0 / 0 ; rés. neutre 9, terre 32, feu 19, eau 12, air 28.
- **Phases** : phase 1 sans monture ; à **50 % de sa vitalité de base** (9000 PV à 4 joueurs) elle monte sur son vautour (phase 2) ; sorts différents selon la phase.

#### Sorts dans les deux phases
- **Attraction Vénale** (passif) : à chaque début de tour, attire **tous les monstres et personnages** vers elle de : 1 case (> 75 % PV), 2 (75-50 %), 3 (49-25 %), 4 (< 25 %) ; **aussi déclenchée à chaque ligne de dommages qu'elle subit**.
- **Balance-Fléau** (`dj6-balance-fleau-fixed_orig.png` : croix de portée 6 ; zone = demi-cercle de rayon 2, 5 cases) : ~1000 dégâts dans tous les éléments sauf neutre, vole 200 Puissance (infini, cumulable — **max 5 cumuls depuis la 2.68, soit 1000 Puissance**) ; **en ligne jusqu'à 6 PO, 2/tour**.
- **Barchan** (`dj6-barchan_orig.png` : losange de portée 6) : ~600 eau, -100 soins et soins reçus -50 % pendant 1 tour ; **jusqu'à 6 PO, 3/tour**.

#### Phase 1 seulement
- **Dakhma** (`dj6-dakhma_orig.png` : losange de portée 2) : ~600 feu, retire 100 PA / 100 PM, rend la cible **invulnérable 1 tour** et la **téléporte à sa position de début de tour** ; un allié qui frappe la cible retire ces effets ; **jusqu'à 2 PO, relance 2 tours**.

#### Phase 2 seulement
- **Funérailles Célestes** (`dj6-funerailles-celestes_orig.png` : portée 4-8, couronne bleue avec un trou de rayon 3) : ~600 air et **échange de place** avec la cible ; entités à 2 PO de sa case de départ : dégâts air (sauf ses alliés) + attirées de 2 ; entités à 2 PO de sa case d'arrivée : dégâts terre (sauf alliés) + repoussées de 2 ; **4 à 8 PO sans ligne de vue** ; texte : relance 3 tours / note 2.68 : **relance 2 tours et plus bloqué par la Pesanteur**.
- **Grand Urubu** (`dj6-grand-urubu-fixed_orig.png` : croix de portée 2, flèche 1 → 2 = repoussée longue) : ~500 neutre, **-150 rés. poussée (200 en CC) pendant 2 tours**, **repousse 6 (8 en CC)** ; **en ligne jusqu'à 2 PO, relance 2 tours**.

#### Principe du combat
- **MàJ 2.67** : au plus **4 Charognes** en même temps ; Misère **élimine toutes les Charognes** au début de son tour.
- **MàJ 2.68** : Balance-Fléau cumul max 5 ; Funérailles relance 2 et ignore Pesanteur ; **tous les Miséreux gagnent +2 PM de base** et **Misère leur retire 2 PM au début du combat** ; Misère **dévoile les invisibles** en début de tour ; Thésaurisation = +10 esquive PA/PM (infini).
- **Seuil 50 %** : elle monte sur le vautour et devient **invulnérable un tour** (jusqu'au tour du perso qui a passé le seuil) ; sur monture : **+4 PM** (total 10) et **Intaclable** jusqu'à la fin.
- **Collecte (début de tour de Misère)** : pour chaque monstre en Collecte **dans sa ligne de vue**, elle lui retire l'état et prend : Ferrailleur 10 % vita max (1800) + 50 dommages ; Dawaj 15 % crit + 200 Force ; Désosseur 1 PM + 200 Agi ; Krèvladal 1 PA + 200 Chance ; Skentu 2 PO + 200 Intel. **Une seule fois par monstre et par tour**, même si le monstre a volé plusieurs fois. Les gains restent sur elle (cumul infini) **tant que le monstre source est vivant** ; à sa mort elle perd tout ce qui vient de lui.
- **Gabelle (même moment)** : les monstres sans Collecte + ceux à qui elle vient de la retirer (donc en LdV) gagnent **+1 PM pour 1 tour** et elle leur **vole 5 % de résistances pour 1 tour**. Exemples détaillés : un monstre en Collecte hors LdV garde l'état, ne donne rien et ne reçoit ni le PM ni la perte de rés.
- **Charognes** (`dj6-charogne_orig.png` : « Charogne niv. 220 », 320 PV, 0 PA, 0 PM, 0 partout, rés. 0 %) : à la mort de toute entité (perso, monstre, invocation), une Charogne apparaît sur sa case. Au début du tour d'un monstre ou de Misère, les Charognes à **4 PO ou moins** sont consommées : monstre → **soin 1200 PV par Charogne** (et pas de +4 PM ce tour) ; Misère → les élimine **toutes, quelle que soit la distance**, et gagne **880 PB par Charogne**. Pour poser une Charogne : l'invocation doit **survivre jusqu'au tour de Misère**, ensuite sa mort laisse une Charogne. Si Misère est morte, plus aucune Charogne.

#### Conseils tactiques
- Garder Misère à distance (phase 1 : « 4 PM » selon l'article) pour éviter Balance-Fléau ; tuer les monstres d'abord.
- Stuff **eau** (sa rés. la plus faible, 12 %).
- La tuer vite en phase 2.
- Se cacher derrière des obstacles contre l'attirance (début de tour + chaque coup reçu).
- Tuer les Charognes à 4 PO d'elle (bouclier).
- Dakhma : frapper l'allié touché pour annuler.

#### Stratégie globale
- Focus le monstre le plus loin de Misère ; sinon **Ferrailleur** (plus de dégâts + vol de dommages), puis **Krèvladal** (PA), puis **Désosseur** (moins dangereux) ; tuer en priorité celui qui vole la carac de l'élément de la team ; **à plus de 4 persos, le Skentu** (vol d'Intel, gros dégâts).
- Phase 1 peu dangereuse : la repousser derrière un mur jusqu'à la phase 2.
- **Schéma** `dj6-placement-attirances_orig.png` : carte du boss vue de haut, case bleue au centre (Misère au début de son tour, qui avance de 6 cases), flèche bleue vers un muret en bas à gauche, 6 cases rouges alignées le long du bord inférieur gauche derrière le muret (la team, qui se décale pour frapper et repousser Misère au moins jusqu'à la case bleue). Phase 1 : 1 case d'attirance par coup au-dessus de 75 %, 2 entre 75 et 50 %. Prévoir ses PM pour se replacer ; un Pandawa peut porter un allié ; possible de faire 2 lignes.
- Phase 2 : 10 PM, coopération à 8 PO sans LdV (Pesanteur sans effet) → elle touchera (sauf Pandawa portant). Après la coopération, la replacer 4 cases plus loin : 3 tours pour la tuer.

### VI. Succès
Icônes : `hardi.png` (personnage + monstre, fond orange), `mains-propres.png` (piège à mâchoires), `special.png` (crâne à cornes), `duo.png` (deux silhouettes), `dj6-score200_orig.png` (« 200 » vert).
- **Hardi** (finir son tour au contact d'un ennemi) :
  - Stratégie 1 : Misère en premier, Pandawa (tank) + team chance/eau, Dofus Nébuleux / Ivoire ; Pandawa à l'initiative ramène Misère au T1 (double Vulnérabilité, enchaînement Picole / Pandanlku / Vulnérabilité ×2 / Picole / Karcham ×2, Dofus Abyssal) ; passer Misère à 50 % avant son tour 2, la tuer au tour 3 ; éloigner Ferrailleur (vitalité) et Krèvladal (chance).
  - Stratégie 2 : la bloquer dans un coin (`dj6-hardi-bloquer-misere_orig.png` : rouge = Misère dans le coin, violet = Krèvladal et Désosseur à côté d'elle, bleu = tank sous elle, vert = autres persos au contact des monstres) ; tank ~150-160 tacle ; Balance-Fléau impossible (ligne + LdV) ; attention au premier Dakhma ; en phase 2 elle n'a plus Dakhma et ne peut pas utiliser sa coopération (PO min 4) au contact.
  - Stratégie 3 : Misère en dernier avec les Charognes, 2 tanks qui la bloquent, autres persos à plus de 8 PO.
- **Mains Propres** : achever sans dommages directs (poussée, poisons, invocations…) ; Dofus Ébène utile ; attention au bouclier des Charognes avant un poison mortel.
- **Spécial « Abjecte opulence »** : Misère achevée en premier **et** elle doit entrer dans l'état Collecte au moins une fois par tour (elle l'obtient si, à son début de tour, un Miséreux en Collecte est dans sa LdV ; un Miséreux est en Collecte s'il a infligé des dégâts pendant son tour). Schéma `dj6-misere-special1_orig.png` : Misère (rouge) dans un coin en bas, 2 invocations (orange) de part et d'autre, un monstre en diagonale (vert, 3 cases), un tank (bleu) au-dessus, les autres persos (violet) loin à gauche (à 9 PO ou plus) ; ne pas mettre le Ferrailleur en diagonale (bouclier) ; tuer les Charognes chaque tour et replacer les invocations ; persos à ≤ 3 PO ou ≥ 9 PO à cause de Funérailles (4-8) ; variante : la bloquer entre deux monstres taclés (aléatoire, le Krèvladal peut décaler un bloqueur).
- **Score 200** (Nahuatl interdite) :
  - Compo 1 (`dj6-compo1_orig.jpg`, score 204) : Hulhu magistrale, Hulhu majeure, Pého magistrale, Pého majeure, Kyoub magistrale, Yoche — mêlée, compatible Hardi + Mains Propres (stratégie 1 de Hardi).
  - Compo 2 (`dj6-compo2_orig.jpg`, score 203) : Yoche magistrale, Yoche majeure, Yoche, Yoche mineure, Djim, Nyoro mineure — Crâ + Pandawa (+ Enutrof) ; pousser pour rendre vulnérable (Djim) ; au passage en phase 2, Représailles (Crâ) + repousser → elle passe son tour.
  - Compo 3 (`dj6-compo3_orig.jpg`, score 203) : Butor magistrale, Butor majeure, Butor, Butor mineure, Leukide, Nyoro mineure — même logique sans Djim, rester à distance.
- **Duo** (2 persos, < 40 tours) : Pandawa tank (50 % rés., Ivoire, Émeraude) + un perso full dégâts porté ; tuer les monstres loin de Misère, rester à plus de 6 PO d'elle ; Stabilisation (relance 3) contre l'attirance ; schéma `dj6-placement-attirances-duo_orig.png` : Misère (bleu, au centre-droite, 10 PM) → flèche vers le Pandawa portant (rouge, coin bas gauche derrière un muret).

### Images génériques du site (vues, sans rapport avec le donjon)
`1426805183.png` (logo « Dofus pour les Noobs »), `promo-maj307.jpg` (présentation MàJ 3.7), `promo-raids-guilde.jpg`, `promo-saison-ivoirec.jpg` (Saison Ivoire), `promo267-resolution-chasse.jpg` (outil chasse au trésor), `lanceur.png` (losange vert), `po_max.png` (losange bleu), `zone_effet.png` (losange rouge), `arkaw.jpg` / `losv5.png` (avatars des auteurs), `arrowup2.png` (flèche retour en haut). **Toutes les 55 images de la page ont été chargées et lues, aucune en échec.**

---

## (b) Tableau des ÉCARTS (DPLN / nos données / correction)

Rappel des identifiants : Misère 5990 (grade 1 = 970040), Ferrailleur 5991 (970050-54), Krèvladal 5992 (970055-59), Désosseur 5993 (970060-64), Skentu 5994 (970065-69), Dawaj 5995 (970070-74), Charogne 5997 (970075+). Donjon 119, salles 9705-9709.

### BLOQUANT
| # | DPLN | Chez nous | Correction |
|---|---|---|---|
| B1 | Toute la mécanique repose sur des passifs (Collecte prise par Misère, Gabelle, Attraction Vénale, seuil 50 %, Charognes, Faim du Monde, Thésaurisation, -2 PM, dévoilement des invisibles). | Dans le 2.74 ces passifs sont des **sorts de départ** (`startingSpellId`) : Misère → niveau 45565 = **sort 15213 « Sentence de la Balance »** ; chaque Miséreux → 66239 = **15212 « Miséreux » niv. 2** (→ Thésaurisation 15211) ; Charogne → 45693 = **15270 « Charogne »**. Les sorts sont importés (SQL 97) mais **rien ne les lance** : `monsters_grades` n'a pas de sort de départ et **aucun brain pour 5990 / 5991-5995 / 5997** (grep : seul CompanionActor gère un sort de départ). | Créer des brains `BrainPassif` (comme `PassifsDonjonsBrains.cs`) : 5990 → `SortPassif.Poser(f, 15213)` ; 5991-5995 → 15212 niveau 2 (Thésaurisation) ; 5997 → 15270. Vérifier ensuite chaque sous-sort (voir partie c). |
| B2 | Dakhma **seulement en phase 1** ; Funérailles Célestes et Grand Urubu **seulement en phase 2**. | Dans le 2.74 : `statesCriterion` = **`HS!1563`** (Dakhma 45644) et **`HS=1563`** (Funérailles 45640, Grand Urubu 45643), 1563 = état « Cavalier ». `donnees.py` ne reconnaît que le format `E123` / `!E123` → **StatesRequired / StatesForbidden vides** dans notre `SpellLevels.d2o` ET dans `spells_levels` (SQL 97, `StatesRequiredCSV` = '' et `StatesForbiddenCSV` = ''). Résultat : Misère peut lancer **les 5 sorts dès le tour 1** (téléportation 4-8 PO + repousse 6 en phase 1, Dakhma en phase 2). | `spells_levels` : 45640 et 45643 → `StatesRequiredCSV='1563'` ; 45644 → `StatesForbiddenCSV='1563'` ; idem dans le d2o client. Corriger la regex de `donnees.py` / `serveur_sql.py` pour le format `HS=` / `HS!` et **rescanner les 488 niveaux Eliocalypse** (les 3 autres donjons sont sûrement touchés). |
| B3 | Seuil 50 % : invulnérable 1 tour, +4 PM, Intaclable, apparence vautour, nouveaux sorts. | Données présentes : 15548 « Seuil PV 50 % Misère » (effet 2872, `dice 0,50`) + 15244 « Appel du Vautour » (trigger `TR15548` : apparence 1552, Invulnérable 56 1 tour, +4 PM, Intaclable 96, Cavalier 1563, Vautour 1564, désactive Pesanteur 7 et Indéplaçable 97 1 tour). Le moteur gère 2872 + `TR<sort>` (EffetsEliocalypse.cs). **Mais rien ne pose 15548** (dépend de B1). | Réglé par B1 (15213 lance 15548 et pose le déclencheur 15244). Tester : à 9000 PV, PV bloqués au seuil, changement de look, invulnérable jusqu'au tour du joueur suivant. |
| B4 | Charognes : apparition à chaque mort (si invocation vivante au tour de Misère), max 4, consommées au début du tour (monstre : soin + pas de +4 PM ; Misère : 880 PB chacune), toutes tuées au tour de Misère. | Monstre 5997 importé (320 PV, 0 PA/PM, niv. 220, look 5614) mais aucune apparition / consommation (dépend de B1 : « Charo Vie » 15269, « Charognard » 15351, « Charogne » 15270). | Réglé par B1 + vérifier : 15269 niv. 6 invoque 5997 (effet 181) sur la case du mort si libre et s'il n'est ni porté ni porteur (états 8 / 3) ; 15270 niv. 2 : Nécrophage Rassasié 1561 + désactive Affamé 1560 + soin 25 % PV max pour le monstre tueur, ou **bouclier 400 % du niveau = 880** pour Misère ; 15351 niv. 2 « Tue la cible ». Limite de 4 : **non trouvée dans les données** → à coder. |

### IMPORTANT
| # | DPLN | Chez nous | Correction |
|---|---|---|---|
| I1 | Salle du boss : à plus de 4 persos, **Skentu** présent (« pensez à le tuer rapidement »). | Salle 9709 (SQL 113) : 1-4 = Misère + Ferrailleur 970052 + Krèvladal 970057 + Désosseur 970062 (✓ conforme à la capture) ; ajouts : 5 → **Dawaj** 970072, 6 → Ferrailleur, 7 → Krèvladal, 8 → Désosseur. **Jamais de Skentu.** | Mettre le **Skentu (970067) comme 1er ajout à 5 joueurs** (puis Dawaj, …). Les 4 cases orange vides de la capture confirment 4 ajouts. |
| I2 | Compositions (captures, 4 monstres) : S1 Skentu + Désosseur + 2 Krèvladal ; S2 Dawaj + 2 Skentu + Ferrailleur ; S3 Dawaj + 2 Désosseur + Krèvladal ; S4 Dawaj + Skentu + 2 Ferrailleur. | 9705 : Krèvladal, Désosseur, Skentu, Dawaj ; 9706 : Désosseur, Skentu, Dawaj, Krèvladal ; 9707 : Skentu, Dawaj, Krèvladal, Ferrailleur ; 9708 : Dawaj, Krèvladal, Désosseur, Ferrailleur (un exemplaire de chaque, rotation). | Aligner sur DPLN (doublons inclus) si l'on considère les captures comme la compo officielle (voir doutes : possible tirage aléatoire). |
| I3 | Niveaux visibles en salle : 200 et 203 (grades 1-2). | Toutes les salles utilisent le **grade 3** (niv. 206, 6300 PV) : 970052 / 57 / 62 / 67 / 72. | Optionnel : grades variés (1-5) comme en officiel ; à décider. |
| I4 | Misère ne prend la Collecte que des monstres **dans sa ligne de vue**. | « Épreuve de la Balance » 15214 lance « Collecte des X » 15215-15219 sur `a,F599x` en zone **`C63,0,10,4,1`** (paramètres supplémentaires = probable LdV) ; notre `Zone` ne lit que forme / taille / taille min. | Coder le filtre LdV (brain Misère ou Zone) ; une seule prise par monstre et par tour (l'état est retiré). |
| I5 | Attraction Vénale : début de tour **et chaque ligne de dommages reçue**, 1-4 cases selon la tranche de PV. | 15250 : niv. 2 (début de tour, `g,A` en C63, masques `*v75`, `*V75,*v50`, `*V50,*v25`, `*V25`) ; niv. 3 sur l'attaquant via effet 1017 trigger `D`. Les masques V/v sont lus par `TargetCriterion.cs`. | Dépend de B1 ; vérifier en jeu le sens de `v` / `V` (PV < / ≥ %), l'attirance de **tous** (alliés compris) au début de tour, et l'attirance par ligne de dégâts. |
| I6 | Salle du boss : Misère retire 2 PM à tous les Miséreux au début du combat ; chaque tour +4 PM (Faim du Monde) sauf Charogne consommée ; +1 PM / -5 % rés. (Gabelle). | Sans passifs : monstres à **6 PM fixes** partout. (En salles normales le 2.74 ne donne que Thésaurisation → 6 PM, c'est conforme à la 2.68.) | Réglé par B1 (15212 niv. 1 → Faim du Monde 15209 + -2 PM ; 15245 Gabelle). |
| I7 | Thésaurisation : +10 esquive PA/PM à chaque tentative de retrait PA/PM/PO, infini. | 15211 niv. 2 (+10 / +10, durée -1) sur triggers `MPA|APA|R` — tous trois connus du serveur (`R` = OnRangeLost). Non posé (B1). | Réglé par B1 ; vérifier que `MPA/APA` se déclenchent aussi sur une tentative esquivée. |
| I8 | Phase 2 : Funérailles = coopération 4-8 PO sans LdV, sous-sorts air + attirance / terre + poussée en rayon 2. | 15239 → 15240 (attire 2, 31-35 air, C2) et 15241 (31-35 terre, repousse 2, C2), échange de place 1023, NeedTakenCell = 1. Données conformes ; IA à vérifier. | IA Misère (voir c). |
| I9 | Dégâts « estimés » : monstres 100-400, boss 500-1000. | Nos fiches de sort sont les valeurs de base ; avec Force/Intel/Chance/Agi **800** (monstres) et **901** (Misère) : Fouet ≈ 370-500, Désossage ≈ 370-540, Grenade ≈ 190-270, Dakhma ≈ 510-700, Barchan ≈ 310-400, Grand Urubu ≈ 410-600, Funérailles ≈ 410-500, Balance-Fléau ≈ 4 × 90-130. | Les chiffres DPLN datent de 2019 (stats plus basses) ; **garder le 2.74**. À valider en jeu (ordre de grandeur). |

### DÉTAIL
| # | DPLN | Chez nous | Correction |
|---|---|---|---|
| D1 | Fiches : **4 PM** pour tous les monstres. | 6 PM (2.74). | Conforme à la MàJ 2.68 (+2 PM de base, -2 PM retirés par Misère dans sa salle). Rien à faire hors B1. |
| D2 | Funérailles Célestes : « relance 3 tours » (texte) / 2 tours (note 2.68). | Relance 2 (15239). | OK (2.68 = 2.74). |
| D3 | Balance-Fléau cumul max 5 (2.68). | MaxStack 5 (45572) ✓. Assembricolage cumul 2 ✓ (45547), Grand Urubu 1, Barchan 1, Démantèlement 1. | OK. |
| D4 | Charogne : soin **1200 PV** par Charogne. | 15270 : **25 % des PV max** (1500 à 6000 PV, 1650 à 6600). | Garder le 2.74 (DPLN plus ancien). |
| D5 | Thésaurisation « 2 tours » (intro). | Durée -1 (infini). | Garder le 2.74 (et la partie boss de DPLN dit infini). |
| D6 | Barchan : « dégâts eau ». | « vol Eau » (effet 91) + soins reçus ×50 % (1159, trigger H) + -100 soins. | Garder le 2.74. Vérifier que l'effet **1159** (Effect_HealBuff) a un handler. |
| D7 | Souffle du Désert : **téléporte** au càc. | « Avance de 2 cases » (effet 1042, Advance.cs). | Équivalent avec une portée 3 ; OK. |
| D8 | Grand Urubu CC : 200 rés. poussée, repousse 8. | Notre état ne liste que les effets normaux. | Vérifier que `CriticalEffectsBin` de 45643 est rempli. |
| D9 | Esquive / tacle : 80 / 64 / 64 (niv. 200), 81 / 65 / 65 (203), Misère 110 / 72 / 72. | `PaDodge`, `PmDodge`, `TackleEvade`, `TackleBlock` = 0 ; Sagesse 640 / 721, Agilité 800 / 901. | Normalement dérivés des stats → vérifier en jeu que le serveur les calcule (sinon fuite / tacle à 0). |
| D10 | Clé (texte) : 1 Perce-Neige, 1 Poisskaille, 1 Frostiz, 2 Viande Goûtue, 2 × 4 ressources de monstres. | Recette 22149 : 22050, 22051, 22049, 22048 ×2, 17160 ×2, 11106 / 11102 / 11109 ×1 ✓ (même structure). L'image DPLN (7 / 5) est plus ancienne. | OK. |
| D11 | Entrée [1,0] Désert de Misère, PNJ Bree Caule. | Bree Caule 5363 sur **201851392 [1,0]**, cellule 273 ; donjon 119, clé 22149, 1re salle 203423744 cellule 327 ; sortie 203427842 → 201851392 ✓. | OK. |
| D12 | Misère non capturable. | Rien de spécifique vu (boss `IsBoss = 1`). | Vérifier que la capture de boss est refusée. |
| D13 | Challenges : non détaillés par DPLN. | Incompatibles 17, 5, 38, 974 ✓ (2.74). Succès (Hardi, Mains Propres, Spécial, Score 200, Duo) : pas dans nos données serveur vérifiées ici. | À traiter avec les succès du donjon si on les importe. |
| D14 | Noms : Dawaj, Désosseur, Ferrailleur, Krèvladal, Skentu, Misère, Charogne ; sorts identiques. | Identiques (15197-15208, 15220, 15239-15243, 15246). Aucun sort actif en trop ni manquant. | OK. |
| D15 | Portées / LdV / lancers : Fouet 1-5 ligne 3/t ; Sol Aride càc 1/t ; Désossage 2 PO 2/t ; Souffle 3 PO sans LdV 3/t ; Démantèlement càc 2/t ; Terminal Gris 1-5 ligne 3/t ; Crache 3 PO 3/t ; Pouilleux càc 2/t ; Balle 3-8 PO modifiable 3/t ; Bantha càc 1/t ; Grenade 3-8 sans LdV 1/t ; Balance-Fléau ligne 6 2/t ; Barchan 6 3/t ; Dakhma 2 relance 2 ; Funérailles 4-8 sans LdV ; Urubu ligne 2 relance 2. | **Tous identiques** (Balle Vorace RangeCanBeBoosted = 1 ✓). Zones conformes aux schémas (X1 croix, L5 = ligne de 6, T1 perpendiculaire 3, U2 demi-cercle, C2 rayon 2). | OK. |
| D16 | PV / PA / rés. des fiches. | Identiques (Dawaj 203 : 6200 ; Désosseur / Krèvladal / Skentu 200 : 6000 ; Ferrailleur 203 : 6200 ; Misère 18000 / 20 PA / 6 PM ; résistances identiques au % près). | OK. |

---

## (c) Mécaniques qui demandent du code serveur

1. **Brain Misère (5990)** : poser 15213 « Sentence de la Balance » au début du combat. Ce sort enchaîne (2.74) :
   - 25120 (Dakhma, état 3782 « Blocage TP Dakhma » le 1er tour, retiré en fin de tour) ;
   - 15548 seuil 50 % + déclencheur `TR15548` → 15244 Appel du Vautour ;
   - Thésaurisation sur elle ;
   - **début de tour** : 15250 Attraction Vénale niv. 2 ; 15214 Épreuve de la Balance (Collecte des Ferrailleurs / Krèvladals / Désosseurs / Skentus / Dawajs 15215-15219, puis Gabelle 15245) ; 15351 Charognard ; 15269 Charo Vie ; 15213 niv. 2 (dévoile les invisibles, effet 202) ;
   - **sur dommages reçus** (trigger D) : 15250 niv. 1 → attirance par ligne de dégâts ;
   - 15212 Miséreux niv. 1 → Faim du Monde 15209 + **-2 PM** sur tous les Miséreux (5991-5995, 6076, 6084, 6085).
2. **Brain Miséreux (5991-5995)** : poser 15212 niv. 2 (Thésaurisation). En salle du boss, Faim du Monde leur est donné par Misère.
3. **Charogne (5997)** : poser 15270 (au décès : soin 25 % / Rassasié pour un monstre, bouclier 880 pour Misère) ; **limite de 4 simultanées** (à coder, pas dans les données) ; Misère tue toutes les Charognes au début de son tour ; consommation par les monstres **à 4 PO** (DPLN) — les données 2.74 utilisent la zone C63 (voir doutes).
4. **Ligne de vue** pour la prise de Collecte (zone `C63,0,10,4,1`), **une prise par monstre et par tour**, et **perte des gains à la mort du monstre source** (stats stockées par source : à coder, les buffs `dur -1` actuels ne disparaissent pas à la mort du donneur).
5. **Trigger `CI`** (utilisé par 15907 « Rêve de Misère ») **absent** de `TriggerBuff.cs` → à ajouter ou à neutraliser.
6. **Corriger `HS=` / `HS!`** (B2) dans les deux exports (client + SQL).
7. **IA** :
   - **Misère phase 1** : chercher l'alignement + LdV pour Balance-Fléau (2/tour, vol de puissance), Barchan à 6 PO, Dakhma au contact (≤ 2 PO) ; avance de ses 6 PM vers la team.
   - **Misère phase 2** (10 PM, intaclable) : Funérailles Célestes à 4-8 PO sans LdV pour s'échanger avec un joueur caché (prioritaire dès que la relance est prête), puis Balance-Fléau / Barchan / Grand Urubu (repousse 6) au contact.
   - **Ferrailleur** : Assembricolage (0 PA, relance 3) quand des alliés sont à 2 cases ; Terminal Gris en ligne (attire 4) puis Démantèlement ×2 au càc.
   - **Dawaj** : Fouet Gadderfi en ligne (repousse 2 puis attire 4 → ramène la cible) ; Sol Aride au càc dans l'axe (touche aussi ses alliés : éviter de les viser).
   - **Désosseur** : Souffle du Désert ×3 (approche sans LdV) puis Désossage ×2.
   - **Krèvladal** : Crache-Misère à 3 PO puis Pouilleux Massacreur ×2 au càc.
   - **Skentu** : Grenade Aveuglante (3-8 sans LdV), Balle Vorace ×3 à distance, Bantha au càc.
   - **Spécial / Hardi** : DPLN suppose que Misère cherche à se libérer (Krèvladal qui pousse un bloqueur) et que les monstres viennent au contact si le tacle est faible.
8. **Effet 1159** (soins reçus ×%, trigger H) : vérifier le handler (Barchan).

---

## (d) Doutes

1. **Compositions de salles** : les captures DPLN montrent une seule disposition (avec doublons). En officiel, les groupes des donjons Eliocalypse pourraient être tirés au hasard ; je n'ai pas vérifié d'autre source.
2. **Consommation des Charognes** : DPLN dit 4 PO pour les monstres ; dans le 2.74, Charognard (15351) cible `a,F5997` en **C63** (toute la carte), avec une branche conditionnée par l'état 3534 « Rêve de Misère ». Soit la règle a changé après 2020, soit la zone est restreinte ailleurs.
3. **« Rêve de Misère » (15907)** : Charognes invulnérables + explosion 5 % des PV de l'attaquant en neutre dans un rayon 4 (effet 89, trigger `X`). **Absent de DPLN** (mécanique ajoutée après 2020 ?). Le déclencheur `CI` est inconnu du serveur.
4. **Limite de 4 Charognes** (2.67) : introuvable dans les données 2.74 (sans doute codée côté serveur officiel).
5. **Phase 1 « 4 PM »** (conseil DPLN) contre 6 PM sur la fiche du boss : sans doute un texte d'avant la 2.68.
6. **Zone `C63,0,10,4,1`** : je suppose que les paramètres supplémentaires codent la ligne de vue ; non confirmé.
7. **Sens des masques `v` / `V`** (PV en % du lanceur) dans Attraction Vénale : à confirmer en jeu (1 case au-dessus de 75 %, 4 cases sous 25 %).
8. **Dégâts** : les chiffres DPLN sont des estimations 2019 ; je n'ai pas recalculé avec les résistances.
9. **Collecte des Ferrailleurs** : le 2.74 donne 50 dommages + **10 % de vitalité** à Misère (DPLN : « 1800 », soit 10 % de 18000) — même chose.
10. Je n'ai pas vérifié si les **critiques** (CriticalEffectsBin) des sorts sont bien remplis dans le SQL 97.
