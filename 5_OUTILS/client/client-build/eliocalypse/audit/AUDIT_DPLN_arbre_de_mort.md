# AUDIT DPLN — Arbre de Mort (boss Corruption, Eliocalypse) — 23/10/2026

Source : https://www.dofuspourlesnoobs.com/arbre-de-mort.html (article d'Arkaw et Los, serveur Ush, mis en ligne le 09/12/2019, mis à jour le 05/10/2026).
Page brute, texte extrait et 55 images : `_OUTILS/client/client-build/eliocalypse/audit/dpln_arbre/` (`page.html`, `texte.txt`, `images.txt`, `img/`).
Comparé avec : `audit/etat_arbre_de_mort.md` (notre état), et pour trancher : les données du client 2.74 (`_OUTILS/client/client_27x/data`), notre SQL `_OUTILS/serveur/donjons/eliocalypse/97/98/99_*.sql`, le d2o converti `eliocalypse/client/data/common`.
Aucun fichier du jeu n'a été modifié.

> La page DPLN est protégée (reproduction interdite) : la partie (a) restitue **tous les faits, chiffres et mécaniques** sous forme structurée et reformulée, sans recopier le texte mot pour mot.

---

## (a) Contenu complet de la page, structuré

### Images de la page (55, toutes téléchargées et ouvertes)
| # | Fichier | Contenu |
|---|---|---|
| 01 | 1426805183.png | Logo « Dofus pour les Noobs » (décor du site) |
| 02-05 | promo-*.jpg | Bannières du site (màj 3.7, raids de guilde, Saison Ivoire, outil chasse au trésor) — sans rapport |
| 06 | dj7-corruption-sommaire | Bandeau : Corruption sur sa monture (chariot/destrier purulent) devant la salle du boss |
| 07 | dj7-1 | Carte du Royaume Corrompu : donjon « Arbre de mort (Niv. 200) » en **[4,0]** ; encart PNJ **« Rafflesia la Fétide »** (gardienne, Sadida obscure) ; encart Clef de l'Arbre de Mort niv 200, 800 XP, ingrédients **1 / 7 / 1 / 7 / 5 / 5 / 5 / 5** (Perce-Neige, Poisskaille, Frostiz, Viande, puis 4 ressources des monstres) — **recette de la 3.x, différente du texte** (voir plus bas) |
| 08 | dj7-peau-pourissante | Icône de l'état Peau pourrissante (crâne vert) |
| 09 | dj7-etats-corruption | Chiffres violets 1-2-3 (niveau de Corruption affiché au-dessus du perso) |
| 10 | dj7-germinal | 2 Germinials (perso transformé + invocation) |
| 11-13 | lanceur / po_max / zone_effet | Légende : case verte = lanceur, bleue = portée, rouge = zone |
| 14 | dj7-dolid | Fiche Dolid niv 200 : **5800 PV, 12 PA, 4 PM**, 65 / 64 / 64 (tacle / esq. PA / esq. PM), rés. fixes 0, **9 % / 15 % / -11 % / -8 % / 14 %** (neutre/terre/feu/eau/air) |
| 15 | Germintaïde | Portée 3-10 (trou de 2 cases autour du lanceur), zone cercle 2 (rose) autour de la cible |
| 16 | Tétanysme | Portée 1-8 sans minimum visible, zone croix 1 (5 cases) |
| 17 | dj7-gangredogue | Fiche niv 200 : **6000 PV, 12 PA, 4 PM**, 60/64/64, **-10/-14/9/16/12 %** |
| 18 | Débarbouillie | Portée ≤ 5, 1 case, flèche de repoussée 1 |
| 19 | Gangraine | Portée 3-10 (trou autour), 1 case |
| 20 | Tir Purulent | Portée 3-12 (trou), zone croix 1 |
| 21 | dj7-nheur-gueule | Fiche niv 200 : **8200 PV, 12 PA, 6 PM**, 65/64/84, **15/10/17/-12/20 %** |
| 22 | Brutalysme | Portée 1-2, 1 case |
| 23 | Crachacide | En ligne uniquement ≤ 6, flèche d'attirance |
| 24 | Tourbe Ylol | Zone carré 3×3 autour du lanceur |
| 25 | dj7-pistilangue | Fiche **niv 203** : **7300 PV, 12 PA, 4 PM**, 82/65/65, **-13/26/10/17/-7 %** |
| 26 | Flemingysme | Portée ≤ 5, 1 case |
| 27 | Maturation | Portée ≤ 6, 1 case |
| 28 | Spore Addikt | Zone carré 5×5 autour du lanceur |
| 29 | dj7-tentaclaque | Fiche **niv 203** : **7900 PV, 102 bouclier (Mucus actif)**, 12 PA, 3 PM, 97/50/85, **24/-13/14/21/-10 %** |
| 30 | Mucus | Zone croix de taille 2 autour du lanceur (2 cases par branche) |
| 31 | Parasitysme | Portée ≤ 2 ; cible en 1 → téléportée en 2 (symétrique par rapport au lanceur) |
| 32 | Ponction lombric | Portée ≤ 5, 1 case |
| 33 | dj7-4 « Salle 1 » | Gangredogue, Gangredogue, Nheur'Gueule, Dolid (timeline : Gangredogue, perso, Gangredogue, Nheur'Gueule, Dolid) |
| 34 | dj7-5 « Salle 2 » | Tentaclaque, Nheur'Gueule, Dolid, Pistilangue |
| 35 | dj7-6 « Salle 3 » | Gangredogue, Tentaclaque, Nheur'Gueule, Dolid |
| 36 | dj7-7 « Salle 4 » | Tentaclaque, Nheur'Gueule, Nheur'Gueule, Pistilangue |
| 37 | 72ark1mpazr | Salle du boss : Corruption (monté), Nheur'Gueule, Dolid, Pistilangue ; placements orange (monstres) / bleus (joueurs), obstacles et 2 trous |
| 38 | corruption-fixed | Fiche Corruption niv 220 : **21000 PV, 20 PA, 4 PM**, 100/68/108, **22/16/5/11/25 %** |
| 39 | Bêche corrompue | En ligne 1-2 (croix), zone ligne de 3 cases dans l'axe |
| 40 | Bombe (monture) | « 20 PO max. », minimum 6 (grand carré gris autour du lanceur) |
| 41 | Bouillon de culture | Glyphe violet autour du lanceur (forme cercle, l'image est coupée) |
| 42 | Convalescence Prolifique | Portée 1-10, 1 case |
| 43 | Incu-Batteur | Portée 2-14 (4 cases adjacentes grises), 1 case |
| 44 | Bombe (à pied) | Portée ≤ 6, sans minimum |
| 45 | Liquéfaction | **Même image exacte que Incu-Batteur (doublon côté DPLN, md5 identique)** |
| 46 | Putréfaction | Portée 3-10 (trou), 1 case |
| 47 | Éclosion germinal | Portée 2-5, zone cercle 2 |
| 48-51 | collant / mains-propres / special / duo | Icônes de succès |
| 52 | dj7-placement-duo | Salle du boss vue sans monstres : cases vertes (cachettes bas-gauche) et rouges (cachettes haut-gauche) derrière les obstacles |
| 53-54 | arkaw / los3 | Avatars des auteurs |
| 55 | arrowup2 | Flèche « haut de page » |

### I — Introduction
- Entrée : **Royaume Corrompu [4,0]**. Gardienne (image) : **Rafflesia la Fétide**.
- Quêtes liées : « Mort et renouveau », « Les totems de Maïmane ».
- Clé (texte) : 1 Perce-Neige, 1 Poisskaille, 2 Frostiz, 2 Viande Goûtue, 2 Peau pourrie de Dolid, 2 Patte de Nheur'Gueule, 2 Tentacule de Tentaclaque, 2 Pétale de Gangredogue. (L'image montre la recette 3.x : 1/7/1/7/5/5/5/5.)
- Accès aux zones des 4 Cavaliers : zaap **Arche de Vili [15,-20]**, accessible par la faille de la **Mine Hable [10,-19]**.
- Boss non capturable.

### II — Maladies
- Les monstres infligent des maladies (états) ; on les retire par **soin direct ou vol de vie**.
- **Soigner au CàC d'un monstre repousse d'1 case le(s) monstre(s) au contact de la cible soignée** (sert contre le boss).
- **Peau pourrissante** : état de départ de tous les persos (et de toute invocation dès son arrivée). Poison 190-265 feu fixe (à partir du tour suivant, puis à chaque tour, infini) ; **dommages finaux -90 %** ; à partir du tour 2, **-10 % des PV actuels en fin de tour**. Retirée définitivement par n'importe quel soin (vol de vie compris), même à PV pleins.
- **Corruption** : chaque maladie différente fait monter un état général : 1 maladie = Corruption 1, 2 = Corruption 2, 3 = Corruption 3, plus de 3 = **Phase Germinal**. Chiffre violet au-dessus du perso.
  - Corruption 1 : soins reçus ×80 %, dégâts subis ×125 %, 10 % érosion (infini).
  - Corruption 2 : ×60 %, ×150 %, 20 %.
  - Corruption 3 : ×40 %, ×175 %, 30 %.
  - Phase Germinal : ×20 %, ×200 %, 40 %. Le perso se transforme en Germinial et invoque un Germinial qui frappe alliés comme ennemis avec « Morsure Putride » et « Langue Morte ».
- 9 maladies ; on les contracte **au stade 3** ; il faut **3 soins** pour descendre 3 → 2 → 1 → 0 (et perdre un niveau de Corruption). Un soin fait baisser **toutes** les maladies d'un cran. Si on a déjà la maladie au stade 1 ou 2, une nouvelle application ne la remet pas à 3. Les soins marchent même à PV pleins.
- **Plus de contraction via des effets alliés** (ex. Picole du Pandawa, retrait PA par un allié).
- Tableau des maladies :

| Maladie | Effets (DPLN) | Contraction |
|---|---|---|
| Brutalyte | inflige et subit +20 % ; quand on frappe (sort ou arme), les alliés à 3 PO prennent ~500 | Brutalysme (Nheur'Gueule) |
| Flemingyte | -30 tacle, -900 PV, -35 % crit, -250 puissance (infini) | Flemingysme (Pistilangue) |
| Gangrainyte | poison fin de tour 377-443 terre fixe ; -200 soins ; se propage en fin de tour sur tout ce qui est à 1-2 PO | Gangraine (Gangredogue) |
| Parasityte | poison début de tour 111-158 feu fixe | Parasitysme (Tentaclaque) |
| Péhaimyte | poison début de tour 177-244 terre fixe ; -30 esquive PM | tentative de retrait PM |
| Péhatyte | poison début de tour 177-244 eau fixe ; -40 esquive PA | tentative de retrait PA |
| Péhotyte | poison début de tour 177-244 air fixe ; -20 % dommages distance | retrait de PO |
| Taclyte | poison début de tour 177-244 feu fixe ; -20 fuite | se faire tacler |
| Tétanyte | si poussé / attiré / téléporté : 177-244 neutre | Tétanysme (Dolid) |

- Corruption applique en plus une maladie **au hasard** avec : Bombe Bactériologique, Bouillon de culture, Liquéfaction.

### III — Monstres (dégâts DPLN = au niveau du monstre)
**Dolid** — Germintaïde : ~300 air ; soigne ~350 PV les alliés en cercle 2 autour de la cible et +100 puissance (120 crit) 1 tour ; 3-10 PO ; 2/tour. Tétanysme : ~480 eau en croix 1 + Tétanyte 3 ; ≤ 8 PO ; relance 2. **Peau pourrie (passif)** : s'il prend des dégâts en mêlée, il riposte, recule d'1 case et retire 2 PM à l'attaquant (infini).
**Gangredogue** — Débarbouillie : ~380 eau vol de vie, repousse 1, vole 2 PO 1 tour, + Péhotyte 3 ; ≤ 5 ; 2/tour. Gangraine : ~350 terre + Gangrainyte 3 ; 3-10 ; relance 2. Tir Purulent : ~300 eau croix 1, soigne ~400 un allié touché ; 3-12 ; 3/tour.
**Nheur'Gueule** — Germinator : invoque un **Germinion 5100 PV** (Langue Morte ~180 feu, attire 2, en ligne 2-6 ; Morsure Putride ~200 terre CàC). Brutalysme : ~450 feu vol de vie + Brutalyte 3 ; ≤ 2 ; 1/tour. Crachacide : poison 19-27 air 1 tour + attire 2 (lançable 2 fois sur la même cible, poison 1 seule fois) ; en ligne ≤ 6 ; 2/tour. Tourbe Ylol : ~350 terre carré 3 autour de lui, -2 PA esquivables 1 tour + Péhatyte 3 ; 1/tour.
**Pistilangue** — Flemingysme : poison 20-30 terre 1 tour + Flemingyte (-30 tacle, -960 PV, -35 % crit, -250 puissance) ; ≤ 5 ; relance 2. Maturation : ~100 feu, -25 tacle 1 tour ; ≤ 6 ; 3/tour. Spore Addikt : lui et alliés en carré 5 : +2 PM (désenvoûtable) et **+1460 vitalité** 2 tours, et **intercepte les dégâts** des alliés de la zone 2 tours (le « sacrifice ») ; relance 3.
**Tentaclaque** — Mucus : ~400 terre croix 2 autour de lui ; lui et alliés de la zone +100 bouclier et +3 PM (désenvoûtable) 1 tour ; 1/tour. **Spongieux (passif)** : s'il subit des dommages de poussée, -3 PO aux ennemis à 1-2 PO du point d'impact. Parasitysme : ~350 air, téléporte la cible en symétrique + Parasityte 3 ; ≤ 2 ; relance 2. Ponction lombric : ~400 eau vol de vie, -3 PM esquivables 1 tour + Péhaimyte 3 ; ≤ 5 ; 2/tour.

### IV — Salles (captures, à 1 joueur)
Salle 1 : 2 Gangredogue + Nheur'Gueule + Dolid. Salle 2 : Tentaclaque + Nheur'Gueule + Dolid + Pistilangue. Salle 3 : Gangredogue + Tentaclaque + Nheur'Gueule + Dolid. Salle 4 : Tentaclaque + 2 Nheur'Gueule + Pistilangue. Boss : Corruption + Nheur'Gueule + Dolid + Pistilangue. (Aucune info DPLN sur les ajouts à 5-8 joueurs.)

### V — Boss Corruption
- Commence **sur sa monture** ; après son « délock » il descend **3 tours** ; ses sorts changent selon qu'il est monté ou non.
- **Aura du Destrier Purulent** (dès le début) : effet **Kissiphrot Sipique** (infini) sur lui et ses alliés : chaque coup porté à un porteur donne au frappeur un **poison terre 1 tour cumulable (~1400 PV par poison)**.
- **Bêche corrompue** (toujours) : ~600 feu en ligne de 3, -3 PA esquivables (4 crit) 2 tours, -1 tour de durée aux effets ; en ligne ≤ 2 ; 3/tour.
- Monté : **Bombe Bactériologique** (allié +660 bouclier + une maladie au hasard, 11 % chacune, 1 tour ; à la fin du tour de l'allié : explosion cercle 3, ~550 terre + la même maladie aux ennemis ; 6-20 ; 1/tour). **Bouillon de culture** (glyphe violet rayon 3 autour de lui ; y entrer = ~300 dégâts (150 eau + 150 air) + maladie au hasard). **Convalescence Prolifique** (~400 air, repousse 2 ; ≤ 10 ; 3/tour). **Incu-Batteur** (~500 eau ; 1 tour : à chaque dégât reçu la cible revient à sa position précédente ; 2-14 ; relance 2).
- À pied : **Bombe Bactériologique** (idem, ≤ 6, 2/tour). **Liquéfaction** (~350 eau, 50 % de chance de maladie au hasard, **échange de place** ; 2-14 sans LdV ; 4/tour). **Putréfaction** (~700 feu ; 3-10 ; relance 3). **Éclosion germinal** (~350 neutre cercle 2, -200 puissance (300 crit) 1 tour ; ses alliés dans la zone ~+350 bouclier (400 crit) ; 2-5 ; 1/tour).
- **Régénération de la monture** : 3 tours après le délock, il remonte et remet Kissiphrot Sipique à tout le monde (lui compris). Une fois à pied, il ne lance plus Bouillon de culture (plus de glyphe).

**Principe** : impossible de gagner sans retirer Kissiphrot. On le retire en **soignant un allié/une invocation/soi-même au CàC d'un monstre** (ce qui repousse d'1 case les monstres au contact) ; retiré **pour tout le combat** sur ce monstre ; à faire pour chacun. Sur Corruption, seulement 3 tours (puis remontée + réapplication à tous les vivants). Correctif : **les poisons appliquent aussi Kissiphrot**.

**Conseils** : ne pas retirer la Peau pourrissante des persos qui ne tapent pas (tank : tant qu'il l'a, il n'attrape pas de maladie ; souvent mieux de la garder 4-5 tours malgré les -10 % PV/tour depuis la 2.68). Surveiller la cible de la Bombe pour voir la maladie (Brutalyte : ne pas laisser ce monstre à 3 PO d'un perso). Éviter le glyphe (ou y aller avec un perso encore en Peau pourrissante). Pires maladies : Brutalyte et Gangrainyte. **Sacrifice du Pistilangue** : si le Pistilangue a encore Kissiphrot et intercepte les dégâts d'un monstre délocké, on prend quand même le poison. Prévoir du soin / vol de vie (armes multi-lignes de vol de vie). En Phase Germinal : tuer vite le Germinial, soigner 3 fois le perso transformé.

**Stratégie** : retirer la Peau pourrissante des attaquants ; tuer d'abord **Nheur'Gueule** (retrait PA, Brutalyte) puis Pistilangue ; perso ou invocation au CàC du monstre focus et le soigner ; garder Corruption à distance (repousser, retrait PM, sans dégâts pour éviter le poison) ; s'écarter de lui (Bêche) ; **ne délocker Corruption qu'à la fin** (invocation au CàC soignée pour éviter le glyphe) ; Pesanteur pour l'empêcher d'échanger de place ; le tuer en moins de 3 tours sinon il remonte.

### VI — Succès
- **Collant** : finir son tour à côté d'un allié (pas une invocation). Délocker via invocation soignée. Un porteur de Brutalyte doit s'écarter pour taper puis revenir.
- **Mains propres** : achever chaque ennemi en dégâts indirects (pièges, glyphes, poisons, poussée, renvoi, invocations). L'invocation doit d'abord être soignée (Peau pourrissante -90 %). Les poisons appliquent Kissiphrot : délocker avant.
- **Spécial « Immunité collective »** : seuls les combattants sans aucune maladie peuvent infliger des dégâts. Tank en Peau pourrissante ; focus Nheur'Gueule puis Dolid et Pistilangue (désenvoûter le sacrifice) ; à la fin soigner 3 fois par tour les persos malades.
- **Duo** : 2 persos max, moins de 40 tours. Soigneur + gros DD distance ; délock par invocation ; focus Nheur'Gueule ; se cacher derrière les obstacles (image 52 : bas-gauche puis haut-gauche). **IA** : monstres plutôt en haut-droite / milieu au départ, puis au milieu ; **Nheur'Gueule rush** ; **Corruption a une IA fuyarde** (tape en début de tour puis recule à l'opposé) ; **Dolid inoffensif** ; Pistilangue peu dangereux sauf Flemingysme et son sacrifice. Variante Pandawa tank (Peau pourrissante gardée, tacle le Nheur'Gueule) + Roublard (mur de bombes, Vulnérabilités, Corruption mis dans le mur à la fin).

---

## (b) Écarts avec nos données

Rappel identifiants de grade (SQL 99) : Corruption 970135-144, Pistilangue 970145-149, Dolid 970150-154, Nheur'Gueule 970155-159, Tentaclaque 970160-164, Gangredogue 970165-169.
**Méthode** : nos valeurs (PA, PO, LdV, ligne, lancers, relance, dés, zones) sont **identiques au client 2.74, sort par sort**. Les PV/PA/PM/résistances des 6 monstres sont **identiques aux fiches DPLN** (Dolid g1, Gangredogue g1, Nheur'Gueule g1, Pistilangue g2, Tentaclaque g2, Corruption g1). Les vrais écarts sont donc surtout ce qui **n'est pas dans les sorts des monstres** (passifs, auras, scripts de combat) et des **conditions perdues à la conversion**.

### BLOQUANT
| # | Sujet | DPLN / 2.74 | Nous | Correction |
|---|---|---|---|---|
| B1 | **Conditions monté / à pied de Corruption (6026)** | Monté seulement : Incu-Batteur 15385, Convalescence 15386, Bombe 6-20 PO 15600 (2.74 : `HS=1627`). À pied seulement : Liquéfaction 15387, Putréfaction 15388, Bombe ≤ 6 15389, Éclosion 15392 (2.74 : `HS!1627`). État 1627 = « Monture ». | **StatesRequiredCSV / StatesForbiddenCSV vides** en SQL (`spells_levels` 45863, 45864, 46178, 45865, 45866, 45867, 45870) **et** dans le d2o converti (statesRequired/statesForbidden = []). Le convertisseur ne lit que le format « E123 / !E123 » ; le 2.74 utilise « HS=… / HS!… ». → Corruption lance ses 8 sorts en même temps (2 Bombes, Putréfaction monté, etc.). | 15385, 15386, 15600 : StatesRequired = 1627 ; 15387, 15388, 15389, 15392 : StatesForbidden = 1627. Corriger aussi le convertisseur (`donnees.py`) pour « HS= / HS! » (autres donjons Eliocalypse concernés probablement : 15499 Germintaïde `HS!1637`). |
| B2 | **Script de combat « Corruption » (sort 15396) non branché** | Lancé au début du combat (hors liste de sorts des monstres) : état 1666 ; sur chaque joueur **15310 → 15311 Terrain fertile** (Peau pourrissante 15474, déclencheurs de maladies, soin = guérison, soin au CàC = repousse + délock) ; sur Corruption **15359** (hub monture) ; sur Dolid **15420 Peau pourrie** ; sur Tentaclaque **15411 Spongieux** ; sur les invocations 15412 (hub d'invocation). | Rien dans notre état ni dans le code serveur (aucune référence à 15396 / 15359 / 15310). Le donjon tourne **sans Peau pourrissante, sans Kissiphrot, sans contraction par malus, sans guérison par soin, sans passifs**. | Code serveur : au début d'un combat contre un groupe contenant 6026-6031 (ou sur la carte du donjon), lancer 15396 sur chaque combattant (critères F6026/F6028/F6030 = « est ce monstre », f… = « n'est pas »). Voir (c). |
| B3 | **Aura du Destrier Purulent / Kissiphrot Sipique** | 15360 (lancé par 15359 quand Corruption gagne l'état 1627) : 15361 posé sur lui et tous ses alliés (déclencheur D/DTB/DTE = dégâts, y compris poisons) → poison **147-198 terre** 1 tour sur l'attaquant (~1400-1700 au niveau 200-220), état 2701, apparence 1545. | Absent (ni dans la fiche de Corruption, ni ailleurs). | Code (c). Sans cela le combat est trivial. |
| B4 | **Délock / Régénération de la monture (3 tours)** | Soin au CàC → 15381 : enlève l'état 1627 et repousse d'1 case les monstres en croix 1 autour du soigné. Sur un monstre : 15450 retire Kissiphrot définitivement. Sur Corruption : 15383 Désarçonné (enlève l'aura à tous, état 2701 retiré, apparence 1546, 15441). **Remontée au bout de 3 tours** (DPLN) — **aucun effet de durée dans les données 2.74** (15441 « Régénération de la monture » = texte seul). | Absent. | Code : compteur de 3 tours côté serveur puis remettre 1627 sur Corruption (→ EON1627 → 15360 réappliqué à tous les vivants). |
| B5 | **Bouillon de culture** (monté) | 15362 / 15363 / 25605 : glyphe-aura cercle 3 qui suit Corruption (relancé en fin de tour, et sur déplacement/poussée/téléportation/porté), entrer dedans : **39-50 air** (~450 au niveau 220 ; DPLN ~150 eau + 150 air) + 15364 maladie au hasard ; états 4074/4075 ; disparaît quand il est à pied (critère *e2672). | Absent (n'est pas un sort de la liste du monstre en 2.74 : il est lancé par l'aura 15360). | Arrive avec B3 si les déclencheurs TE / TB / EOFF8 / EOFF4074 / M / PD / TP sont gérés ; sinon code. Vérifier l'effet 1091 « glyphe-aura » et 2018 « dissipe les glyphes » dans notre moteur. |
| B6 | **Contraction des maladies par malus** | Péhatyte (tentative de retrait PA, déclencheur APA → 15325), Péhaimyte (MPA → 15327), Péhotyte (retrait PO, R → 15326), Taclyte (taclé, T → 15328) ; seulement par un ennemi (DPLN). C'est ainsi que Tourbe Ylol 15397, Ponction lombric 15405, Débarbouillie 15408, Bêche 15384 donnent leurs maladies (aucun effet « maladie » dans ces sorts). | Absent (porté par Terrain fertile 15311). | Code (c) ; vérifier la correspondance des codes 2.74 APA / MPA / R / T avec nos BuffTriggerType (APA, MPA, R existent ; T 2.74 ↔ tF chez nous ?). |

### IMPORTANT
| # | Sujet | DPLN / 2.74 | Nous | Correction |
|---|---|---|---|---|
| I1 | Composition salle 1 (9710) | 2 Gangredogue, Nheur'Gueule, Dolid | Dolid 970152, Nheur'Gueule 970157, **Tentaclaque 970162**, Gangredogue 970167 | Remplacer Tentaclaque par un 2e Gangredogue (si on suit DPLN) |
| I2 | Salle 2 (9711) | Tentaclaque, Nheur'Gueule, Dolid, Pistilangue | Nheur'Gueule, Tentaclaque, **Gangredogue 970167**, Dolid | Gangredogue → Pistilangue (9701 45-49) |
| I3 | Salle 3 (9712) | Gangredogue, Tentaclaque, Nheur'Gueule, Dolid | Tentaclaque, Gangredogue, Dolid, **Pistilangue 970147** | Pistilangue → Nheur'Gueule |
| I4 | Salle 4 (9713) | Tentaclaque, 2 Nheur'Gueule, Pistilangue | Gangredogue 970167, Pistilangue 970147, Nheur'Gueule 970157, **Dolid 970152** | Gangredogue + Dolid → Tentaclaque + Nheur'Gueule |
| I5 | Salle 5 (9714) boss | Corruption + Nheur'Gueule + Dolid + Pistilangue | Corruption 970135 + Pistilangue + Dolid + Nheur'Gueule | **OK** |
| I6 | Kissiphrot et sacrifice | Spore Addikt 15400 (interception 765, carré 5, 2 tours) : le poison part si le **Pistilangue** porte encore Kissiphrot, même si le monstre frappé est délocké | — | Quand on code B3 : le déclencheur doit partir sur le porteur qui **reçoit** les dégâts après interception (le Pistilangue). |
| I7 | Peau pourrie (Dolid 6028, sort 15420 → 15395) | Dégâts en mêlée (DM) → riposte 17-27 terre, -2 PM 1 tour à l'attaquant, recule d'1 case | absent | Branché par B2 (déclencheur DM existe chez nous). |
| I8 | Spongieux (Tentaclaque 6030, 15411 → 15422) | Poussé (PD / PPD) → -3 PO 2 tours aux ennemis en cercle 2 du point d'impact | absent | Branché par B2. |
| I9 | Brutalyte (15332) | Déclencheur 2.74 « CDCAC / CDS » (frappe CàC ou sort) → 15332 niv 3 : 43-56 meilleur élément aux alliés en cercle 3 ; ×120 % subis, +20 % infligés | Commentaire « Brutalyte » dans TriggerBuff.cs : déjà en partie traité | Vérifier en jeu. |
| I10 | Tétanyte / Parasityte / Gangrainyte | Tétanyte : déclencheurs M et TP → 177-244 neutre fixe ; Parasityte : déclencheur CI (invocation) ; Gangrainyte : propagation 15301 en cercle 2 en fin de tour (15466) | dans les données | Vérifier que M / TP / CI / TE sont traduits. |
| I11 | Germinion (6032) / Germinial (6033) | Germinion : DPLN 5100 PV (Langue Morte ~180, Morsure ~200). 2.74 g1 : 4900 PV, Morsure Putride 15409 (2 PA CàC 19-26 terre), Langue Morte 15410 (3 PA 2-6 en ligne 23-30 feu, attire 2). Germinial 6033 : niveau 1, 1 PV en 2.74 (stats à calquer sur l'invocateur ?), sorts 15443/15444 qui touchent **alliés comme ennemis** | Pas dans notre état (seulement « Invoque : 6032 » dans Germinator 15446) | Vérifier la fiche de 6032/6033 en base ; Phase Germinal 15414 invoque 6033 (effet 181, zone C63,1). |
| I12 | Aléatoire des maladies | Bombe 15389/15600 : 9 états à 11,111 % chacun (une seule maladie) ; Liquéfaction 15387 : 50 % maladie / 50 % rien ; _RANDOM DISEASE 15364 : 11,111 % chacun | Notre état n'affiche pas l'aléatoire ; le d2o converti a 11 / 50 (entier) | Vérifier que `EffectsBin` SQL garde Random (sinon la Bombe met les 9 maladies d'un coup). 11 ≠ 11,111 : la somme fait 99 % (1 % sans maladie) — tolérable. |
| I13 | Soin = guérison | Soin (H / LPU, même à PV pleins) → 15314 → 15475 → 15315 : chaque maladie baisse d'un cran, retire la Peau pourrissante ; en Phase Germinal : 15316 (compteur Germinal) | absent | Code (c) : déclencheur « soin reçu » y compris soin nul à PV pleins (LPU). |
| I14 | IA | Corruption **fuyarde** (frappe puis s'éloigne) ; Nheur'Gueule **rush** ; Dolid support (Germintaïde soigne/booste) ; Pistilangue support (Spore Addikt) ; Gangredogue distance (3-12) ; Tentaclaque mêlée (Mucus, Parasitysme ≤ 2) | à vérifier | Choisir les IA en conséquence. |

### DÉTAIL (DPLN différent du 2.74 ; garder nos données 2.74 sauf avis contraire)
| # | Sujet | DPLN | 2.74 = nous | Remarque |
|---|---|---|---|---|
| D1 | Malus de Corruption 1/2/3/Germinal (15464, 15424) | soins ×80/60/40/20 % | **×85/70/55/40 %** ; dégâts ×125/150/175/200 et érosion 10/20/30/40 identiques ; -10/20/30/40 % vita seulement avec l'état 1671 (Songes) | Version DPLN plus ancienne. |
| D2 | Poisons Péhatyte / Péhotyte / Péhaimyte / Taclyte | 177-244 | **201-268** | idem |
| D3 | Flemingyte (15336) | -900 / -960 PV fixes | **-25 % PV** (+ -30 tacle, -35 % crit, -250 puissance) | idem |
| D4 | Peau pourrissante (15474 / 26015) | -10 % PV actuels / tour | 26015 : -5 % (niv 1, au lancement) puis niv 2 en fin de tour | vérifier la valeur du niv 2 ; 28-43 feu à l'application + 190-265 en fin de tour |
| D5 | Spore Addikt 15400 | +1460 vitalité | **+10 % vitalité** (niv 2) + taille +7 % | ok 2.74 |
| D6 | Dégâts (×9 pour 800 en stat, ×10 pour Corruption 901) | Tétanysme 480, Brutalysme 450, Ponction 400, Maturation 100, Bêche 600, Liquéfaction 350, Convalescence 400, Éclosion 350 | 36-49 → ~380 ; 35-47 → ~370 ; 25-32 → ~260 ; 27-38 → ~290 ; 34-46 → ~400 ; 17-24 → ~205 ; 28-35 → ~315 ; 24-30 → ~270 | Les autres sorts collent (Germintaïde 300, Débarbouillie 380, Gangraine 350, Tir Purulent 300, Tourbe 350, Mucus 400, Parasitysme 350, Bombe 550, bouclier 660 = 300 % niv, Incu 500, Putréfaction 700). Écarts = valeurs DPLN approximatives / autres grades / critiques. Pas de correction. |
| D7 | Germintaïde : soin | ~350 PV | 5 % PV max en cercle 2 (cible g) | cohérent |
| D8 | Mucus : bouclier | 100 | 50 % du niveau (= 102 à niv 203, visible sur la fiche Tentaclaque) | cohérent |
| D9 | Clé 22148 | texte : 1/1/2/2/2/2/2/2 ; image : 1/7/1/7/5/5/5/5 (3.x) | recette SQL = texte = 2.74 (Peau pourrie, Patte, Tentacule, Pétale ×2, Viande ×2, Poisskaille ×1, Perce-Neige ×1, Frostiz ×2) | OK |
| D10 | Esquives | fiches : 65/64/64 etc. | PM esq. 40 (Corruption), 20 (Nheur'Gueule, Tentaclaque), PA esq. -15 (Tentaclaque), sagesse 640-690 | cohérent (esquive = sagesse/10 + bonus) |
| D11 | Grades de Corruption | — | 10 grades : g1-5 21000-29000, **g6-10 21000-25000** | Doublon de grades venant du 2.74 ? Sans effet si les salles utilisent g1 (970135). |
| D12 | Image Liquéfaction | — | — | DPLN affiche par erreur le schéma d'Incu-Batteur ; portée 2-14 sans LdV confirmée par le texte et nos données. |
| D13 | Challenges incompatibles | — | 18, 43, 20 sur les 6 monstres (2.74) | à garder |

### Entrée du donjon
DPLN : Royaume Corrompu **[4,0]**, PNJ **Rafflesia la Fétide** (accès par le zaap Arche de Vili [15,-20], via la faille de la Mine Hable [10,-19]).
Nous : PNJ 5367 « Rafflesia la Fétide » sur la carte **201328640 = [4,0]**, cellule 413 ; donjon 117, clé 22148, première salle 202637312 cellule 339 ; sortie sur 201328640 cellule 282. **Conforme.**

---

## (c) Mécaniques qui demandent du code serveur
1. **Lancement du script 15396 « Corruption »** au début de chaque combat du donjon (hors liste des sorts). Il distribue : 15310 / 15601 (hub joueur), 15359 (hub Corruption), 15420 (Peau pourrie, Dolid), 15411 (Spongieux, Tentaclaque). Critères à interpréter : `F6026` = le combattant est le monstre 6026, `f6033` = n'est pas 6033, `*e1666` = n'a pas l'état 1666 (lancement unique).
2. **Terrain fertile 15311** (sur chaque joueur, et sur les invocations via CI → 15412) : Peau pourrissante 15474 ; déclencheurs APA → Péhatyte, MPA → Péhaimyte, R → Péhotyte, T → Taclyte (uniquement si la source est ennemie, DPLN) ; H|LPU → 15314 guérison + 15381 repousse/délock ; TB → 15427 / 15435 (textes) ; état 2706.
3. **Hub Corruption 15359** : 15442 met l'état 1627 « Monture » à Corruption et à ses alliés une seule fois ; EON1627 sur Corruption → 15360 Aura ; EOFF1627 sur Corruption → 15383 Désarçonné ; EOFF1627 sur un allié → 15450 retire Kissiphrot de cet allié.
4. **Minuteur de 3 tours** puis remontée (remettre 1627 sur Corruption) — absent des données.
5. **Kissiphrot 15361** : déclenché par D | DTB | DTE (dégâts directs **et** poisons) → poison 147-198 terre sur l'attaquant (cible A,f6120), désactive 1 tour les états d'invulnérabilité 56 / 375 / 376. Tenir compte de l'interception (Spore Addikt).
6. **Glyphe-aura Bouillon de culture** (effet 1091) qui suit Corruption, 2018 (dissipation), états 4074/4075, et déclencheurs EOFF8 (fin de « Porté »), EOFF4074, M, PD, TP pour le replacer.
7. **Compteurs de maladies** : 15289-15306 (COUNTER + / -), 15307 / 15308 / 15464 (niveau de Corruption, couleur 15457), 15413 / 15414 / 15424 (Phase Germinal : transformation 1547, invocation d'un Germinial 6033, compteurs 1619-1621, 15416 purge à la sortie). Le sort 15284 appelé par les COUNTER + **n'existe pas en 2.74** (ignorer).
8. **Traduction des codes de déclencheurs 2.74 → nos BuffTriggerType** : EON<état> / EOFF<état> (chez nous EO# / Eo#), T (taclé ; chez nous tF ?), TP (téléporté), LPU (gain de PV / soin même nul), CI (invocation créée), CDCAC / CDS (frappe CàC / par sort), DTB / DTE (dégâts de poison début / fin de tour ; chez nous « Unknown_3/4 »), PPD, DM, TR.
9. **Effets à vérifier dans notre moteur** : 1101 (téléporte ou échange — Liquéfaction), 1100 (retour à la position précédente — Incu-Batteur 15463, avec la valeur 3), 765 (interception), 1017 / 1018 / 1160 (lancer un sort sur la cible / le lanceur), 1091 / 2018, 2822 (dommages meilleur élément), 2805 (-% dommages distance), 1172 / 1171 / 1163 / 1159 (multiplicateurs), 776 (érosion), 1048 / 1033 / 1078 (% PV / vitalité), 2868 (taille, SQL 101).
10. **Conditions d'état des sorts** (B1) : faire respecter StatesRequired / StatesForbidden au lancer des monstres et dans l'IA.

## (d) Doutes
- Les compositions de salles DPLN viennent de captures (une partie, solo) : le 2.74 pourrait tirer des groupes différents ; à confirmer avant de changer les salles 1-4. Les ajouts 5-8 joueurs ne sont pas documentés par DPLN.
- La remontée « au bout de 3 tours » n'est portée par aucune donnée 2.74 : durée prise sur DPLN.
- 15383 Désarçonné enlève l'aura (Kissiphrot) à **tous** les alliés quand Corruption est délocké et leur remet l'état 1627 : cela ne contredit pas DPLN (à la remontée, tous reprennent Kissiphrot), mais l'ordre exact (délocker Corruption tôt = tous les monstres frappables 3 tours ?) est à vérifier en jeu.
- Valeurs DPLN plus anciennes que le 2.74 (malus de Corruption, poisons, Flemingyte, Spore Addikt, Peau pourrissante 10 %) : j'ai considéré nos valeurs 2.74 comme justes (consigne « version 2.51 + import 2.74 »). À trancher par toi.
- Germinial 6033 niveau 1 / 1 PV en 2.74 : probablement recopié depuis l'invocateur côté serveur officiel — à confirmer.
- Le code « HS » des critères 2.74 : j'ai supposé « le lanceur a l'état » ; sens de « T » (taclé) à confirmer face à nos codes.
- Fiches DPLN : Pistilangue et Tentaclaque en niv 203 (grade 2), les autres en grade 1 ; nos salles utilisent le grade 3 (niv 206) pour tous les monstres — c'est notre choix, pas un écart DPLN.
