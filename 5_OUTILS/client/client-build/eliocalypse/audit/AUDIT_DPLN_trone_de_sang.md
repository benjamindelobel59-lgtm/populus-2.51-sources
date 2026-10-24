# Audit DPLN — Trône de Sang (boss Guerre, Eliocalypse) — 23/10/2026

Source : https://www.dofuspourlesnoobs.com/trone-de-sang.html (rédigé par Arkaw et Los, serveur Ush, mis en ligne le 09/12/2019, dernière mise à jour le 21/01/2026).
Comparé avec : `audit/etat_tr_ne_de_sang.md` (notre état du 23/10).
Fichiers de travail : `audit/dpln_trone/` (page.html, extraction brute texte.txt, imgs.txt, les 49 images de contenu téléchargées).

Rappel : aucun fichier du jeu n'a été modifié. DPLN donne les dégâts **au niveau du monstre (200 / Guerre 220), avec ses caractéristiques**, alors que notre état donne les valeurs de base de la fiche du sort : les écarts de valeurs brutes ne sont donc pas des erreurs en soi (voir partie D).

---

## A. Retranscription structurée de la page (faits, chiffres, mécaniques)

> Reformulation fidèle et complète des informations (le texte de DPLN étant protégé, il est restitué sous forme structurée et non copié mot pour mot ; l'extraction brute est dans `dpln_trone/texte.txt` pour référence locale).

### A.1 Introduction
- **Image dj8-guerre-sommaire** : bannière décorative, salle du trône (piliers, torches, tas de cadavres, flaques de sang) + illustration de Guerre en armure rouge à cornes, sur une monture noire et lave, tenant une hache d'armes.
- **Entrée** : zone **Blessures de Guerre**, en **[4,-2]**.
- **Image dj8-1** : capture de la carte [4,-2] (« Votre position [4,-2] ») avec en médaillon le PNJ **Kapelache** (démon rouge à cornes tenant une hallebarde/fourche, devant une tente) = gardien d'entrée. Médaillon de la recette : **« Clef du Trône de Sang », Niv. 200, 800 XP**, 8 ingrédients (icônes : plante verte, os/parchemin, épi doré, objet sombre, objet sombre, goutte rouge, objet gris, étoffe à motifs). Les petits chiffres visibles dans la capture (7, 7, 5, 5, 5, 5) ne collent pas avec la recette écrite : ce sont probablement les quantités possédées par l'auteur, pas la recette (doute D6).
- **Quêtes liées** : « Le jour le plus long », « Les totems de Maïmane ».
- **Recette de la clef** (texte) : 1 Perce-Neige, 1 Poisskaille, 1 Frostiz, 2 Viande Goûtue, 2 Étoffe sanglante, 2 Sang coagulé, 2 Canine tranchante, 2 Linothorax.
- **Accès** : les zones des 4 Cavaliers sont accessibles par le **zaap Arche de Vili [15,-20]**, lui-même atteint par la **faille de la Mine Hable [10,-19]**.
- **Guerre ne peut pas être capturée.**

### A.2 Légende des schémas (images lanceur.png / po_max.png / zone_effet.png)
- Case **verte** = position du lanceur (deux cases vertes numérotées 1 et 2 quand le monstre se déplace / échange : 1 = départ, 2 = arrivée).
- Case **bleue** = portée du sort (toutes les cases ciblables).
- Case **rouge** = zone d'effet (placée au hasard sur le schéma, peut être n'importe quelle case bleue).
- Symbole **∞** en haut à gauche d'un schéma = portée infinie.

### A.3 Monstres

#### Fléaunide (image dj8-fleaunide = fiche de combat)
- Niv. 200 — **5500 PV** (capture prise à 4998/5500), bouclier 0, **12 PA, 4 PM**.
- Ligne de caracs du haut : 90 (icône jaune), 64 (icône bleue), 64 (icône verte), 0, 0 — probablement initiative/tacle puis esquive PA / esquive PM (icônes non identifiables à coup sûr, doute D7).
- Résistances fixes : 0 partout. Résistances % : **neutre 5, terre 10, feu 5, eau 20, air 10**.
- **Cérébrute** (image dj8-cerebrute) : ~350 dégâts **air** en **zone étoile de distance 3** autour de la case ciblée ; puis la cible **attire ses alliés** situés dans une **étoile de distance 4** autour d'elle **de 1 case** vers elle. Portée **2 à 7**. Schéma : portée bleue large (cercle de 2 à 7, les 4 cases au contact du lanceur grisées = portée mini 2), zone rouge en étoile (croix + diagonales) autour de la cible.
- **Esprit Destructeur** (image dj8-esprit-destructeur) : donne **+2 PA (3 en critique) pendant 1 tour** aux **alliés dans un cercle de rayon 4 autour de lui**. Schéma : grand carré rouge (cercle 4 en vue isométrique) centré sur le lanceur vert, lanceur non compris.
- **Mortification** (image dj8-mortification) : ~400 dégâts **feu** + **-10 % résistances (15 en critique) pendant 1 tour**. **Corps à corps uniquement** (schéma : 4 cases bleues autour du lanceur, 1 rouge).

#### Macabrateur (image dj8-macabrateur)
- Niv. 200 — **5500 PV, 10 PA, 5 PM** ; 90 / 64 / 64 / 0 / 0 ; rés. % **5 / 10 / 5 / 20 / 10** (neutre/terre/feu/eau/air), rés. fixes 0.
- **Découpe** (image dj8-decoupe) : ~450 dégâts **air** en **cercle de rayon 2 autour de lui** et **repousse de 2 cases**. Schéma : 12 cases rouges autour du lanceur (cercle 2 sans le centre).
- **Hache-Tirante** (image dj8-hache-tirante) : ~600 dégâts **feu** en **ligne de 3 cases** et **attire de 2 cases**. Texte : « se lance uniquement depuis sa case càc ». Schéma : 3 cases bleues au contact du lanceur, et 3 cases rouges alignées partant de la 4ᵉ case au contact vers l'extérieur.
- **Timber** (image dj8-timber) : ~500 dégâts **air** sur **l'ennemi le plus proche**, **portée infinie, sans ligne de vue** (schéma ∞, tout bleu, 1 case rouge éloignée).

#### Olgoth (image dj8-olgoth)
- Niv. 200 — **5500 PV, 12 PA, 4 PM** ; 90 / 64 / 64 / 0 / 0 ; rés. % **5 / 10 / 5 / 20 / 10**.
- **Armutation** (passif, sans schéma) : lancé **dès le début du combat** ; le monstre est **invulnérable en mêlée (infini) tant qu'il a plus de la moitié de ses PV** ; une fois à la moitié, il passe **invulnérable à distance (infini)**. Le coup qui le fait passer à la moitié est **plafonné** : s'il ne reste que 500 PV avant la moitié, un coup de 1000 ne fait que 500.
- **Olguropoing** (image dj8-olguropoing) : ~500 dégâts **feu** et la cible voit **ses dommages réduits de 50 % sur les Olgoths** du combat **pendant 1 tour**. Portée **2 à 7** (schéma : 4 cases au contact grisées).
- **Presse** (image dj8-presse) : ~500 dégâts **feu** + **vol de 1 PM (2 en critique), esquivable**. **Corps à corps uniquement**.
- **Séisme** (image dj8-seisme) : ~600 dégâts **terre** en **cercle inversé de rayon 6** (schéma : grand carré gris = cercle de rayon 6 non touché autour du lanceur ; tout le reste de la carte en rouge ; symbole ∞).

#### Ravalame (image dj8-ravalame)
- Niv. 200 — **5500 PV, 10 PA, 4 PM** ; 90 / 64 / 64 / 0 / 0 ; rés. % **neutre 20, terre 25, feu 20, eau 35, air 25**.
- **Fanatisme** (passif) : **réduit de 1 le temps de relance de Frappe Cardinale à chaque coup reçu**.
- **Frappe Cardinale** (image dj8-frappe-cardinale) : ~450 dégâts sur **les 4 personnages les plus proches avec ligne de vue**, un élément par rang : 1ᵉʳ le plus proche = **feu**, 2ᵉ = **air**, 3ᵉ = **terre**, 4ᵉ = **eau** ; **s'il n'a aucune ligne de vue**, il frappe **les 4 éléments sur le personnage le plus proche**. **Portée infinie**, **relance 2 tours** (selon DPLN). Schéma : ∞, 4 cases rouges réparties autour du lanceur.
- **Pluie de Lames** (image dj8-pluie-de-lames) : ~400 dégâts **feu** + **vol de 30 fuite (40 en critique) pendant 2 tours**. **Corps à corps uniquement**.
- **Psyclope** (image dj8-psyclope) : ~500 dégâts **eau** + **-2 PO (3 en critique) pendant 1 tour**. Portée **2 à 7**.

#### Trancharnier (image dj8-trancharnier)
- Niv. 200 — **5500 PV, 10 PA, 5 PM** ; **160** (icône jaune) / 64 / 64 / 0 / 0 ; rés. % **neutre 18, terre 22, feu 17, eau 30, air 19**.
- **Catacombe** (image dj8-catacombe-fixed) : **se téléporte à côté d'un ennemi**, inflige ~500 **terre** à la cible et aux autres personnages **dans un cercle de rayon 2 autour de sa case d'arrivée**. **Portée infinie, sans ligne de vue, 1 fois par tour**. Schéma : lanceur en 1, arrivée en 2 à côté de l'« Ennemi », zone rouge de cercle 2 autour de la case d'arrivée. (Partie Duo : « le Trancharnier qui retire des PA ».)
- **Décharner** (image dj8-decharner) : ~600 **feu** + **-20 % critique (30 en critique) pendant 1 tour**. **Corps à corps uniquement**.
- **Euphorage** (image dj8-euphorage-fixed) : **se téléporte à côté d'un allié** et donne **+30 % dommages finaux** à cet allié **et à tous les monstres dans un cercle de rayon 2** autour de sa case d'arrivée. **Portée infinie, sans ligne de vue, 1 fois par tour**. Schéma identique à Catacombe avec « Allié ».

### A.4 Les salles (images dj8-map1 à map4, toutes à 4 joueurs + 4 monstres, frise de 8 combattants alternés)
| Salle | Monstres (ordre de la frise : 1, 3, 5, 7) |
|---|---|
| 1 | Ravalame (1), Trancharnier (3), Olgoth (5), Fléaunide (7) |
| 2 | Ravalame (1), Macabrateur (3), Olgoth (5), Fléaunide (7) |
| 3 | Ravalame (1), Trancharnier (3), Olgoth (5), Fléaunide (7) |
| 4 | Ravalame (1), Macabrateur (3), Olgoth (7), Olgoth (5) |
| Boss (image 86ark1mpazr) | **Guerre** (à pied, hache) + **Ravalame + Olgoth + Trancharnier** ; placements joueurs bleus en bas à gauche, monstres sur cases orange en haut, face au trône. |
- Aucune information sur les ajouts de monstres de 5 à 8 joueurs (DPLN ne montre que des combats à 4).
- Les cartes 1 à 4 : salles de dalles grises entourées de murets, crânes ; cases de placement rouges (joueurs) et bleues (monstres).

### A.5 Boss : Guerre (image dj8-guerre = fiche)
- Niv. **220** — **22000 PV** (« si vous êtes 4 elle a 22000 PV de base »), bouclier 0, **12 PA, 6 PM** ; **240** (icône jaune) / 64 / 64 / 0 / 0 ; rés. % **neutre 30, terre 35, feu 35, eau 25, air 25**.

#### Sorts
- **Arsenal** (lancé dès le début) : état **Indéplaçable (infini)** sur Guerre ; état **Compagnon de combat (infini)** sur tous les monstres qui l'accompagnent → **Guerre reste invulnérable tant qu'il reste des monstres** ; déclenche l'invocation des armes selon les seuils de PV.
- Lancés invulnérable ET vulnérable (sauf en état Brave) :
  - **Magmaléfice** (image dj8-magmalefice) : ~500 **feu** en **cercle de rayon 2**, **en vol de vie si vulnérable**. Portée **jusqu'à 6** (schéma : cercle bleu 1-6, zone rouge en carré iso = cercle 2). Interdit en état Brave.
  - **Martyre** (image dj8-martyre) : ~700 en **vol de vie**, **terre si invulnérable, feu si vulnérable**. Corps à corps. Interdit en état Brave. (Stratégie globale : en phase 1 « sort de zone ~600 rayon 2 » + « sort de càc ~700 ».)
- Lancés une fois vulnérable :
  - **Bravoure** : met **Guerre et la cible en état Brave 2 tours (indésenvoûtable)**. Guerre : **+200 tacle, +200 fuite, +2 PM** 2 tours. Cible : **Pesanteur, Insoignable et Lynchage** 2 tours (Pesanteur et Insoignable **désenvoûtables**). **Seule la cible Brave subit les sorts de Guerre.** Seule la cible Brave peut frapper Guerre plusieurs fois par tour ; **tout autre personnage qui frappe Guerre devient Pacifiste après son premier coup jusqu'au début de son prochain tour**. **À la sortie de l'état Brave, Guerre gagne +2 PM et +100 fuite pour 1 tour.** Portée **jusqu'à 3, sans ligne de vue, relance 3 tours** (donc 1 tour sur 3 sans Brave). Peut cibler une invocation.
  - **Lynchage** (image dj8-lynchage) : ~400 **feu**, **càc**, **uniquement sur la cible Brave**, **uniquement quand Guerre est Brave**.
  - **Impact** (image dj8-impact2) : ~450 **feu** + **20 % érosion (25 en critique) 2 tours** (érosion désenvoûtable). Portée **jusqu'à 3 sans ligne de vue**, **uniquement sur la cible Brave**, **uniquement quand Guerre est Brave**. Schéma : carré bleu = cercle 1-3.
  - **Célérité** : **échange de place avec une arme ET avec un personnage dans le même tour** (2 échanges max/tour, 1 arme + 1 personnage). Échange avec une arme → **l'arme déclenche son sort d'attaque**. Échange avec un personnage → **glyphe rouge en croix de taille 1 autour du personnage, 1000 dégâts feu quand on y passe**. **Portée infinie sans ligne de vue. Interdit en état Brave.**
- **Note** : si Guerre commence son tour sur un **glyphe de Cawotte**, elle frappe le détenteur du glyphe de **1000 neutre**.

#### Invocation de l'Arsenal
- Une arme tous les **20 % de la vitalité de base** (à 4 : tous les 4400 PV) :
  - **Arsenal 80** — 17 600 PV : **Épée Géante** (5500 PV)
  - **Arsenal 60** — 13 200 PV : **Marteau Géant** (5500 PV)
  - **Arsenal 40** — 8 800 PV : **Hache Géante** (5500 PV)
  - **Arsenal 20** — 4 400 PV : **Morgenstern Géant** (5500 PV)
- **Placement** (image dj8-invocation-arme) : à **2 PO « à 12h »** de Guerre, puis dans le **sens des aiguilles d'une montre** si occupé (schéma : 8 cases numérotées 1 à 8 autour d'une case centrale, 1 en haut à droite, puis 2, 3… en tournant ; les cases sont à 2 cases en ligne droite/diagonale à l'écran).
- **Plafond de PV** : au moment d'invoquer, sa vie se **bloque au seuil** pour l'attaque en cours (ex. 18 000 PV, gros coup → seulement 400 dégâts et invocation).
- **Zones des armes** (images) :
  - **Épée Géante** (dj8-epee-geante) : **étoile de taille 5** (8 branches de 5 cases). **Coup d'épée** (infini) ~**1400 feu**. Possède aussi **Postures** (infini).
  - **Marteau Géant** (dj8-marteau-geant) : **cercle de rayon 3**. **Coup de marteau** ~**1400 terre**.
  - **Hache Géante** (dj8-hache-geante) : **4 triangles de hauteur 4** dont chaque sommet est au càc de l'arme (schéma : grand losange rouge évidé le long des 4 axes diagonaux du grid = carré de 4 sans les diagonales). **Coup de hache** ~**1400 air**.
  - **Morgenstern Géant** (dj8-morgenstern-geant2) : **grille (damier) de taille 5**. **Coup de morgenstern** ~**1400 eau**.
- Les armes frappent **à leur invocation**, **pendant leur tour**, et quand **Guerre échange de place** avec elles.
- **Bonus si une arme est vivante au début du tour de Guerre** (1 tour, selon la **dernière** arme invoquée) : Épée **+2 PM, +100 fuite** ; Marteau **+10 % dommages finaux, +100 tacle** ; Hache **+10 % résistances, +1000 bouclier** ; Morgenstern **+3 PA, soin de 5 % de ses PV**.
- **Mort d'une arme après que Guerre a rejoué** : Guerre gagne **résistances (infini), PM (infini) et se soigne** (~1000/1500 PV), **cumulable** par arme. Si l'arme est tuée **avant** que Guerre ne rejoue, rien.

#### Principe du combat
- Phase 1 : Guerre **invulnérable** et **indéplaçable**, seulement Magmaléfice + Martyre, peu dangereuse.
- **Le tour suivant la mort du dernier monstre** : Guerre **monte sur sa monture** (changement d'apparence), devient **vulnérable**, et lance **Bravoure** ce même tour.
- Puis cycle : 2 tours Brave (focus sur la cible) / 1 tour sans Brave (Célérité + armes).
- Danger principal : dégâts des armes, érosion.

#### Conseils tactiques (utiles pour l'IA)
- Se placer hors zone des armes avant de franchir un seuil ; tuer l'arme **avant** le tour de Guerre.
- Guerre ne peut pas utiliser ses armes en état Brave (2 tours sur 3).
- Un Pandawa qui **porte** un allié non Brave permet à cet allié de frapper Guerre sans devenir Pacifiste.
- **Guerre a peu d'esquive PM** (retrait PM facile).
- Désenvoûter l'érosion et l'Insoignable du Brave.
- Ordre conseillé pour les monstres : **Trancharnier** (se téléporte sur vous) → **Olgoth** (descendre à **2750 PV à distance**, puis l'autre moitié au càc) → **Ravalame**.
- Méthode 1 (tuer les armes) : descendre Guerre vers 18-19 000, se placer, passer 17 600, tuer l'épée ; idem vers 14 000/13 200, 9 500/8 800, 5 000/4 400 ; inutile de tuer le Morgenstern.
- Méthode 2 (ne pas tuer les armes) : pousser les armes loin ; quand Guerre sort de Brave, s'éloigner d'elle et des armes (elle transposera 1-2 persos + 1 arme), puis revenir près d'elle pour qu'elle relance Bravoure.

### A.6 Succès
- **Zombie** (1 PM exactement par tour) : avoir 80-90 fuite mini pour détacler (même avec +200 tacle en Brave) ou se téléporter ; tuer les armes avant le tour de Guerre ; descendre Guerre au seuil +500 puis tuer l'arme ; inutile de tuer la dernière ; alternative : retrait PM (0-1 PM par tour).
- **Misanthrope** (ne jamais finir adjacent à un allié) : aucune difficulté particulière.
- **Spécial « Arsenal total »** : **aucune arme de Guerre ne doit être achevée ou déplacée par un allié** (identique à l'expédition avec l'idole Bravoure). Guerre, elle, peut échanger avec les armes. Hors Brave : jusqu'à 2 échanges/tour (1 arme, 1 personnage). Astuce : être hors de portée de Bravoure (3 PO sans LdV) quand le dernier monstre meurt pour gagner un tour. Pacifiste : invocations limitées à 1 attaque/tour, bombes de Roublard OK.
- **Score 200** (image dj8-score200 = pastille « 200 » ; image dj8-compo1 = 4 idoles **Yoche magistrale, Yoche majeure, Hulhu magistrale, Hulhu majeure**, score total 204) : frapper uniquement au càc ; tuer l'Olgoth en un tour après qu'il a joué. Anachorète faisable en même temps.
- **Duo** (2 persos max, moins de 40 tours) : Pandawa tank (Prohibition = invulnérable en mêlée 1 tour sur 3) + DPS ; ordre Trancharnier (retire des PA) → Olgoth / Ravalame ; Olgoth à distance pour ses 2750 premiers PV. Solution 1 (càc + tuer les armes) : image **dj8-solution1-tuer-armes** = 3 cases alignées : bleu = Guerre, rouge = Pandawa au contact, vert = case d'invocation des armes au contact du Pandawa (donc la case « 12h » à 2 PO de Guerre). Solution 2 (distance sans tuer les armes) : Pandanlku + Stabilisation, porter l'allié en fin de tour ; Guerre échangera avec le Pandawa et avec une arme. Stratégie retrait PM : invoquer beaucoup pour que Guerre transpose les invocations.
- Images d'illustration : zombie.png (botte « 1 »), misanthrope.png (bonhomme + cœur barré), special.png (casque à cornes), duo.png (deux bonhommes) — icônes génériques DPLN.

### A.7 Expéditions de guilde (images audace.png = médaillon guitare/notes ; bravoure.png = médaillon épée)
- **Audace** : armes **invulnérables 1 tour** après invocation ; **à la fin de chaque tour d'un personnage, il échange sa place avec l'arme la plus proche**. Conseils : éloigner les armes de Guerre, utiliser l'échange pour fuir ; tuer une arme donnerait des boosts à Guerre (si après son tour) ; invoquer l'arme le tour où Guerre passe Brave et la tuer avant qu'il ne perde Brave.
- **Bravoure** : armes **indéplaçables et invulnérables tout le combat** ; Guerre peut quand même échanger avec ; mêmes conseils que le succès Spécial.

### A.8 Images non liées au contenu (non ouvertes, décoratives)
Logo du site (1426805183.png), 4 bannières promo (promo-maj307, promo-raids-guilde, promo-saison-ivoirec, promo267-resolution-chasse), avatars des auteurs (arkaw.jpg, los3.jpg), flèche « haut de page » (arrowup2.png). Les **49 images de contenu ont toutes été téléchargées et ouvertes**, aucune n'a échoué.

---

## B. Écarts avec notre état (classés par gravité)

### B.1 BLOQUANT (le combat ne fonctionne pas comme le vrai sans ça)

| # | Sujet | DPLN | Chez nous | Correction |
|---|---|---|---|---|
| B1 | Passif **Arsenal** de Guerre (monstre 6014) | Lancé au début : Indéplaçable infini, état Compagnon de combat sur les monstres → **Guerre invulnérable tant qu'il reste un monstre** ; invocations à 80/60/40/20 % | **Absent** de l'état (aucun sort Arsenal, aucun état Indéplaçable / Compagnon / invulnérabilité) | Ajouter le sort Arsenal (nom client 9020381) + code serveur (partie C). Sans ça, Guerre est tuable dès le tour 1. |
| B2 | **Invocation des 4 armes** (Épée/Marteau/Hache/Morgenstern Géant, 5500 PV) | Aux seuils 17 600 / 13 200 / 8 800 / 4 400 (à 4 joueurs), à 2 PO « 12h » puis horaire, PV de Guerre bloqués au seuil | Les monstres armes ne sont **pas dans l'état** (seulement cités comme cibles F6010-F6013 dans Célérité 15266) ; pas de sort d'invocation | Vérifier que les monstres 6010-6013 existent (grades, 5500 PV, sorts 15262-15265, Postures) et coder l'invocation par seuil. |
| B3 | Passif **Armutation** de l'Olgoth (6023) | Invulnérable mêlée tant que > 50 % PV, puis invulnérable distance ; coup plafonné à 50 % | **Absent** (seuls Séisme, Presse, Olguropoing) | Ajouter (nom client 9020442) + code serveur (états invulnérable mêlée / distance + plafond). |
| B4 | Passif **Fanatisme** du Ravalame (6021) | -1 tour de relance de Frappe Cardinale par coup reçu | **Absent** | Ajouter (nom client 9020433) + déclencheur « coup reçu ». |
| B5 | **Phase 2 de Guerre** (monture + vulnérabilité) | Le tour suivant la mort du dernier monstre : monte sur sa monture, devient vulnérable, lance Bravoure | Rien dans l'état (look unique {5585\|\|\|150}, pas de 2ᵉ look, pas de bascule) | Code serveur : bascule de look + retrait de l'invulnérabilité + choix des sorts de phase 2. |

### B.2 IMPORTANT

| # | Sujet | DPLN | Chez nous | Correction |
|---|---|---|---|---|
| I1 | **PM Ravalame** (6021) | **4 PM** | 5 PM (grades 1-5) | Passer à 4 PM — sauf si la valeur 5 vient bien du client 2.74 (DPLN = captures 2019) → voir doute D1. |
| I2 | **PM Fléaunide** (6022) | **4 PM** | 5 PM | Idem → 4 PM (doute D1). |
| I3 | **PM Macabrateur** (6024) | **5 PM** | 6 PM | Idem → 5 PM (doute D1). |
| I4 | **PM Olgoth** (6023) | **4 PM** | 5 PM | Idem → 4 PM (doute D1). |
| I5 | **Frappe Cardinale** (15342) relance | **2 tours** (+ Fanatisme -1 par coup reçu) | **relance 5**, relance initiale 0 | Si on garde 5 (valeur client 2.74), Fanatisme est indispensable ; sinon descendre à 2. Vérifier le sort déclenché 15343 (4 cibles les plus proches avec LdV, 1 élément par rang feu/air/terre/eau ; 4 éléments sur le plus proche sans LdV) — **ses effets ne figurent pas dans l'état**. |
| I6 | **Timber** (15367) | ~500 **air** sur **l'ennemi le plus proche**, portée infinie sans LdV | Portée 0-0, effet unique « lance 15367 (cible A, zone a1) » ; **aucun dégât visible** (le niveau 2 du sort n'apparaît pas dans l'état) | Vérifier que 15367 niveau 2 existe et contient les dégâts air ; vérifier que notre serveur gère la zone `a` (« ennemi le plus proche ») en 2.51. |
| I7 | **Célérité** (15267 / 15266) | **2 échanges par tour** (1 avec une arme + 1 avec un personnage), interdit en état Brave ; glyphe croix 1, **1000 feu** au passage | lancers/tour **1**, glyphe « de rang 1 » sans détail, pas de condition d'état Brave visible | Vérifier : nombre de lancers, état interdit (Brave 1567), sort du glyphe (zone croix 1, dégâts feu au passage). |
| I8 | **Fin de Brave** (Bravoure 15249) | À la sortie de l'état Brave : Guerre **+2 PM et +100 fuite pour 1 tour** | Absent (Bravoure ne donne que les bonus pendant 2 tours + Pacifiste) | Ajouter un effet déclenché en fin d'état (ou code serveur). |
| I9 | **État Lynchage** sur la cible de Bravoure | La cible reçoit Pesanteur + Insoignable + **Lynchage** | Bravoure met Brave (1567) + Pesanteur (7) + Insoignable (76), **pas d'état Lynchage** | Probablement remplacé par l'état Brave sur la cible (Lynchage/Impact « uniquement sur la cible Brave ») → vérifier les états requis de Lynchage 15259 et Impact 15257 (non visibles dans l'état). |
| I10 | **Restrictions d'état des sorts de Guerre** | Magmaléfice, Martyre, Célérité **interdits en Brave** ; Lynchage, Impact **seulement en Brave** et **seulement sur la cible Brave** ; Bravoure seulement quand vulnérable | L'état ne montre aucun état requis / interdit | Vérifier les champs états requis/interdits (statesRequired/statesForbidden) de 15249, 15257, 15259, 15260/15261, 15275/15276, 15267. Si la 2.51 ne les lit pas → IA serveur. |
| I11 | **Martyre / Magmaléfice : version invulnérable vs vulnérable** | Martyre **terre** (invulnérable) / **feu en vol de vie** (vulnérable) ; Magmaléfice feu, **vol de vie si vulnérable** | 15275 = terre (dommages), 15276 = vol feu ; 15260 = feu, 15261 = vol feu → bon découpage | Rien sur les données, mais **l'IA doit choisir** 15275/15260 en phase 1 et 15276/15261 en phase 2 (code). |
| I12 | **Bonus des armes au début du tour de Guerre** et **boost à la mort d'une arme** | Épée +2 PM +100 fuite ; Marteau +10 % dommages finaux +100 tacle ; Hache +10 % rés +1000 bouclier ; Morgenstern +3 PA + soin 5 % ; arme tuée après le tour de Guerre → rés + PM infinis + soin ~1000-1500, cumulable | Absent | Code serveur ou sorts passifs des armes (Postures = probablement ce mécanisme). |
| I13 | **Pacifiste** (Bravoure niveau 2) | S'applique à **tout autre personnage qui frappe Guerre** pendant que Guerre est Brave, **après son premier coup, jusqu'au début de son prochain tour** | Niveau 2 : Pacifiste durée 1 sur `A,e1567` (ennemis **sans** Brave), déclencheur « DBE » | Vérifier que notre serveur gère le déclencheur DBE (dommages reçus ?) et le masque `e1567`. Vérifier qu'un personnage **porté** par un Pandawa n'est pas affecté (subtilité DPLN, à priori non reproductible). |
| I14 | **Composition des salles** | S1 : Ravalame, Trancharnier, Olgoth, Fléaunide ; S2 : Ravalame, Macabrateur, Olgoth, Fléaunide ; S3 : Ravalame, Trancharnier, Olgoth, Fléaunide ; S4 : Ravalame, Macabrateur, 2× Olgoth ; Boss : Guerre + Ravalame + Olgoth + Trancharnier | Non décrit dans l'état | Comparer avec nos groupes de monstres par salle (le boss **ne doit pas** avoir de Fléaunide ni de Macabrateur à 4). |

### B.3 DÉTAIL

| # | Sujet | DPLN | Chez nous | Remarque |
|---|---|---|---|---|
| D-a | **Hache-Tirante** (15368) portée | « uniquement depuis sa case càc », schéma = portée 1 | Portée **1-3 en ligne**, attire 2 | L'attirance de 2 n'a de sens qu'avec une portée de 3 : notre donnée (client 2.74) est probablement juste et DPLN imprécis. Ne pas toucher sans autre source. |
| D-b | Cérébrute (15349) LdV | Non précisé (2-7) | portée 2-7, **vue False** | OK probablement. Zone *3 et attirance *4 de 1 case : conforme. |
| D-c | Esprit Destructeur (15346) | +2 PA (**3 en critique**) 1 tour, cercle 4 | +2 PA durée 1, zone C4,1, critique 20 % | Vérifier que l'effet critique donne bien 3 PA. |
| D-d | Mortification (15350) | -10 % rés (**15 en critique**) | -10 % durée 1, crit 20 % | Vérifier la valeur critique. |
| D-e | Presse (15357) | vol 1 PM (**2 en critique**), **esquivable** | « Vole 1 PM » | Vérifier que l'effet est bien le vol PM esquivable (et pas inesquivable) et la valeur critique. |
| D-f | Pluie de Lames (15345) | vol 30 fuite (**40 en critique**) 2 tours | -30 / +30 fuite, durée 2 | Conforme ; vérifier critique 40. |
| D-g | Psyclope (15344) | -2 PO (**3 en critique**) 1 tour | -2 PO durée 1, lancers/tour 2, /cible 1 | Conforme. |
| D-h | Décharner (15379) | -20 % critique (**30 en critique**) | -20 % durée 1, lancers/tour 3 | Conforme. |
| D-i | Impact (15257) | érosion 20 % (**25 en critique**) 2 tours, 1-3 sans LdV | 20 % érosion durée 2, portée 1-3, vue False | Conforme. |
| D-j | Olguropoing (15354) | réduction 50 % des dommages de la cible sur les Olgoths **pendant 1 tour** | Mécanique via 15355/15356/15423 (durée 2, déclencheurs TB/TE), vue True | Mécanique complexe (sort déclenché au début du tour de la cible, retiré en fin) : vérifier que la 2.51 gère les déclencheurs TB/TE et la cible `F6023`. |
| D-k | Catacombe (15377/15378) | Téléport à côté d'un ennemi, cercle 2 terre ; (Duo : retire des PA) | Téléporte sur la case ciblée, puis 15378 : terre C2,1 + vol 2 PA | Conforme (la zone C2,1 n'inclut pas la case du Trancharnier, normal). L'IA doit choisir une case **adjacente à un ennemi**. |
| D-l | Euphorage (15375/15376) | +30 % dommages finaux à l'allié et aux monstres en cercle 2 | +30 % durée 1, cible g, zone C2,1 | Conforme ; vérifier que l'effet « dommages finaux % » existe en 2.51. |
| D-m | Séisme (15358) | cercle inversé rayon 6 | zone C63,7 | Conforme (touche à partir de 7). |
| D-n | Découpe (15366) | cercle 2 autour de lui, repousse 2 | C2,1 + repousse 2 | Conforme. |
| D-o | Armes : zones | Épée étoile 5 ; Marteau cercle 3 ; Hache 4 triangles hauteur 4 ; Morgenstern damier 5 | *5 ; C3 ; **W4** ; **D5** | Conformes, mais **vérifier que la forme `W` existe dans notre moteur 2.51** (forme ajoutée tardivement ; sinon la Hache ne touche rien ou plante). |
| D-p | Armes : dégâts | ~1400 par coup (niveau du monstre) | 300 de base, déclencheur TB durée -1 | Rapport ~4,7 : dépend des caracs des armes ; à vérifier en jeu. Les armes doivent aussi frapper **à l'invocation**. |
| D-q | Guerre : grades | 22000 PV « à 4 » | Grades 1-5 : 22 000 / 24 000 / 25 000 / 26 000 / 28 000 ; grades 6-10 : 22 000 / 23 000 / 24 000 / 24 000 / 25 000 | Les seuils d'arme sont **relatifs à la vitalité de base** : coder en % (80/60/40/20), pas en valeurs fixes. Grades 6-10 bizarres (doublon 24 000) : vérifier lequel est utilisé. |
| D-r | Guerre : caracs | 12 PA, 6 PM, rés 30/35/35/25/25 | Identique | Conforme. |
| D-s | Trancharnier : caracs | 5500 PV, 10 PA, 5 PM, rés 18/22/17/30/19 | Identique | Conforme. |
| D-t | Rés. et PA des autres | Ravalame 10 PA 20/25/20/35/25 ; Fléaunide 12 PA ; Macabrateur 10 PA ; Olgoth 12 PA ; 5/10/5/20/10 | Identique | Conforme (seuls les PM diffèrent). |
| D-u | Martyre / Magmaléfice / Lynchage : sous-niveau « 31 dégâts feu » en fin de tour (TE) | Rien dans DPLN | 15276 nv2, 15261 nv2, 15259 nv2 : 31 feu (cible C ou A), déclencheur TE durée 2 | Effet non décrit par DPLN (petit DoT ? ou mécanique de vol de vie ?) — à vérifier, voir D4. |
| D-v | Note Cawotte | Guerre sur glyphe de Cawotte → 1000 neutre au détenteur | — | Mécanique générale d'une classe/sort, pas spécifique au donjon : ignorer probablement. |
| D-w | Clef | 1 Perce-Neige, 1 Poisskaille, 1 Frostiz, 2 Viande Goûtue, 2 Étoffe sanglante, 2 Sang coagulé, 2 Canine tranchante, 2 Linothorax ; Clef Niv. 200, 800 XP | Non décrit dans l'état | Vérifier notre recette (objets.json) et le PNJ Kapelache. |

---

## C. Mécaniques qui demandent du code serveur (IA, passifs, déclencheurs)

1. **Arsenal (Guerre)** : au début du combat, Indéplaçable infini + Compagnon de combat sur les monstres ; **invulnérabilité de Guerre tant qu'un monstre (hors armes) est vivant**.
2. **Bascule de phase** : le **tour suivant** la mort du dernier monstre → changement de look (monture), suppression de l'invulnérabilité, lancement immédiat de Bravoure.
3. **Seuils d'invocation** à 80/60/40/20 % de la vitalité de base : **plafond des dégâts** au seuil pour le coup en cours, invocation de l'arme correspondante à 2 PO « 12h » puis sens horaire si occupé, l'arme frappe à l'invocation.
4. **Bonus de début de tour de Guerre** selon la dernière arme invoquée si une arme est vivante ; **boost cumulatif** (rés infini, PM infini, soin) quand une arme meurt **après** que Guerre a rejoué depuis son invocation.
5. **Bravoure** : état Brave sur Guerre et la cible ; **focus exclusif** de l'IA sur la cible Brave (Lynchage/Impact seulement sur elle) ; Pacifiste pour tout autre attaquant après 1 coup (jusqu'au début de son tour) ; **à la fin de Brave : +2 PM +100 fuite 1 tour** ; relance 3.
6. **IA de Guerre** : phase 1 → Magmaléfice 15260 / Martyre 15275 ; phase 2 → si Brave : Lynchage + Impact sur la cible Brave ; sinon : Célérité (1 échange avec une arme pour déclencher son coup sur un maximum de joueurs + 1 échange avec un joueur avec glyphe), Magmaléfice 15261 / Martyre 15276, puis Bravoure dès qu'elle est disponible (3 PO sans LdV). Guerre est indéplaçable : elle avance au contact.
7. **Armutation (Olgoth)** : invulnérable mêlée > 50 % PV, invulnérable distance ≤ 50 %, coup plafonné au passage des 50 %.
8. **Fanatisme (Ravalame)** : -1 relance de Frappe Cardinale à chaque coup reçu ; **Frappe Cardinale** : choix des 4 cibles les plus proches avec LdV et attribution des éléments par rang, ou 4 éléments sur le plus proche sans LdV.
9. **Olguropoing** : réduction de 50 % des dommages de la cible **contre les Olgoths** pendant son tour (déclencheurs début/fin de tour de la cible, filtre par monstre 6023).
10. **Timber** : cible automatique = ennemi le plus proche (zone `a`).
11. **Catacombe / Euphorage** : l'IA doit téléporter sur une **case adjacente** à un ennemi (Catacombe) ou à un allié (Euphorage) pour maximiser la zone ; 1 fois par tour chacun.
12. **Comportements d'IA des monstres** (déduits de DPLN) :
    - **Trancharnier** : agressif, se téléporte sur les joueurs (Catacombe), boost ses alliés (Euphorage), puis Décharner au càc ; c'est la menace prioritaire (vol de PA).
    - **Olgoth** : au contact (Presse), Olguropoing à 2-7, Séisme quand les joueurs sont loin (≥ 7 cases) → IA « au contact » mais qui tire profit de la distance.
    - **Ravalame** : Frappe Cardinale dès qu'elle est disponible (portée infinie), Psyclope à distance, Pluie de Lames au càc.
    - **Fléaunide** : soutien (Esprit Destructeur +2 PA aux alliés à 4 cases) + Cérébrute à distance + Mortification au càc.
    - **Macabrateur** : au contact : Hache-Tirante pour attirer, Découpe pour repousser autour de lui, Timber sur le plus proche.
13. **Armes** : frappent à leur tour (déclencheur début de tour), sont déplaçables (sauf idoles), PV 5500 ; le succès « Arsenal total » demande qu'aucun allié n'achève ni ne déplace une arme (vérification du challenge côté serveur).
14. **Succès** à coder/valider : Zombie, Misanthrope, Arsenal total (spécial), Duo (2 persos, < 40 tours), éventuellement Score 200 (idoles : n'existe pas en 2.51 → probablement ignoré).

---

## D. Doutes

- **D1 — PM des monstres** : DPLN (captures de 2019) donne 4 PM au Ravalame, Fléaunide et Olgoth, 5 au Macabrateur ; nous avons 1 PM de plus partout (sauf Trancharnier et Guerre, identiques). Soit Ankama a ajouté 1 PM entre 2019 et la 2.74 (nos données viennent du client 2.74), soit notre import ajoute un PM. Le fait que seuls 4 monstres sur 6 soient décalés rend une erreur d'import peu probable : à trancher avec la fiche du monstre dans les données 2.74 brutes (ou DofusDB).
- **D2 — Frappe Cardinale relance 5 vs 2** : même logique ; avec Fanatisme (-1 par coup reçu), 5 en données peut être la valeur actuelle. Ne pas modifier sans Fanatisme.
- **D3 — Hache-Tirante portée 1-3 vs càc** : voir D-a, DPLN probablement imprécis.
- **D4 — Sous-niveaux « 31 dégâts feu » en fin de tour** sur Martyre 15276, Magmaléfice 15261 et Lynchage 15259 : non décrit par DPLN ; pourrait être un ancien mécanisme ou un effet invisible. À tester.
- **D5 — État Lynchage** : DPLN parle d'un état « Lynchage » sur la cible ; nos données n'ont que Brave/Pesanteur/Insoignable. Probablement pas un vrai état distinct.
- **D6 — Recette de clef** : la capture montre des petits chiffres (7, 7, 5, 5, 5, 5) qui ne correspondent pas à la recette écrite (1/1/1/2/2/2/2/2) ; ce sont sans doute les quantités possédées par l'auteur. La recette écrite fait foi.
- **D7 — Ligne de caracs « 90 / 64 / 64 »** (160 pour le Trancharnier, 240 pour Guerre) : icônes non identifiables avec certitude (probablement tacle ou initiative, puis esquive PA / esquive PM). Notre état ne donne pas ces valeurs : à comparer si le serveur les stocke.
- **D8 — Formes de zones** `W` (Hache Géante), `a` (Timber), `C63,0,10,4,1` (Frappe Cardinale, 4 plus proches), `l1,2,50,1,1` (Hache-Tirante) et masques `e1567`, `F6023`, `f6120`, déclencheurs `TB`, `TE`, `DBE` : formats du client 2.74, **pas forcément gérés par un émulateur 2.51**. À tester un par un.
- **D9 — Ajouts 5 à 8 joueurs** : DPLN ne donne que des combats à 4 joueurs / 4 monstres ; aucune info sur les monstres supplémentaires.
- **D10 — Dégâts DPLN** : les valeurs (~350 à ~700, armes ~1400) dépendent des caracs des monstres ; les rapports avec nos valeurs de fiche varient (×5 à ×9, ex. Lynchage 71-85 → ~400 mais Martyre 71-80 → ~700). Pas exploitable pour corriger des valeurs précises, seulement l'élément (tous les éléments concordent).
- **D11 — Guerre « elle/il »** : DPLN alterne les deux ; aucune incidence.
- **D12 — Images 2019** : la page a été mise à jour en 01/2026 mais les captures datent de la sortie (2019) ; certaines valeurs ont pu évoluer depuis (relances, PM).
