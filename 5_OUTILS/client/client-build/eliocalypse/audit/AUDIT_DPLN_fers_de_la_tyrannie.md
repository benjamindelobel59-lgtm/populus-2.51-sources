# AUDIT DPLN — Fers de la Tyrannie (boss Servitude, Eliocalypse) — 23/10

Source : https://www.dofuspourlesnoobs.com/fers-de-la-tyrannie.html (article d'Arkaw, serveur Ush, en ligne le 10/12/2019, dernière mise à jour le 26/03/2024).
Fichiers de travail : `audit/dpln_fers/` (page.html, texte.txt = texte brut extrait, 37 images du guide + 6 images du site dans `site/`).
Comparé avec : `audit/etat_fers_de_la_tyrannie.md` + lectures SQL en lecture seule (base `world` locale) + Spells/SpellLevels/Monsters.d2o (sortie Eliocalypse et client 2.74).
**Aucun fichier du jeu ni de la base n'a été modifié.**

> Note droit d'auteur : la page DPLN interdit la reproduction. La partie (a) est donc une **retranscription structurée, reformulée**, qui garde toutes les données chiffrées et toutes les mécaniques, sans copier le texte mot pour mot. Le texte brut reste dans `dpln_fers/texte.txt` pour usage interne.

---

## (a) Contenu de la page DPLN

### Sommaire
I Introduction, II Monstres, III Salles, IV Boss (sorts, principe du combat, conseils tactiques, stratégie globale), V Succès (Focus, Versatile, Spécial, Duo, + Score 200).
Image `dj9-servitude-sommairefixed1_orig.jpg` : Servitude sur sa monture (phase 2) dans sa salle (grande machine centrale, leviers).

### I. Introduction
- **Entrée** : zone *Galère de Servitude*, en **[4,-3]**.
- Image `dj9-1fixed2_orig.png` : vue de la carte [4,-3] (« Votre position [4,-3] »), PNJ gardien **Miradar** (petit personnage rouge et blanc enchaîné) posé sur une machine en haut de la carte ; encadré de la clef.
- **Quêtes liées** : « Briser ses chaînes », « Les totems de Maïmane ».
- **Clef (texte)** : 1 Perce-Neige, 1 Poisskaille, 1 Frostiz, 2 Viande Goûtue, 2 Baguette Rythmique, 2 Bois de pagaie usée, 2 Manche de Fouet, 2 Boulet lesté.
- **Clef (image, client récent)** : « Clef des Fers de la Tyrannie », niv. 200, 800 XP ; quantités affichées : Perce-Neige 1, 2e ingrédient ×7, Frostiz 1, Viande ×7, Baguette ×5, Bois de pagaie ×5, Manche de fouet ×5, Boulet ×5 (recette plus récente que le texte).
- **Accès** aux zones des 4 Cavaliers : zaap *Arche de Vili* [15,-20], atteignable par la faille de la *Mine Hable* [10,-19].
- Boss **non capturable**.

### II. Monstres (légende des schémas : vert = lanceur, bleu = portée, rouge = zone d'effet)
Images `lanceur.png` (losange vert), `po_max.png` (bleu), `zone_effet.png` (rouge).

#### Armécréante — image `dj9-armecreantefixed1_orig.png`
Fiche : niv. 203, **5600 PV, 10 PA, 6 PM**, résistances neutre 20 / terre 30 / feu 30 / eau 15 / air 20 ; 1re icône 83, esquive PA 166, esquive PM 66 (valeurs d'icônes lues sur la capture).
- **Domestication** : ~600 feu + état qui renvoie la cible à sa position de début de tour à la fin de son prochain tour (désenvoûtable). 3 à 10 PO, sans ligne de vue, 2 fois par tour.
- **Ancrépulsion** : ~700 terre ; repousse la cible de 3 cases et le lanceur de 2 ; si la cible est à 2 PO, elle est d'abord attirée d'1 case. En ligne, jusqu'à 2 PO, 1 fois par tour.
- **Escarmouche** (image `dj9-arme-renfortsfixed1_orig.jpg` : icône d'état « Appel des renforts » au-dessus d'une Armécréante, entourée d'un Tambourreau, d'un Ecaptif et de Servitude) : à la fin de chacun de ses tours, elle prend l'état *Appel des renforts* ; si elle n'a subi aucun dégât direct avant son tour suivant, elle invoque un **Iopprimé** ; **2 Iopprimés maximum en même temps par Armécréante**.

#### Iopprimé — image `dj9-iopprimefixed1_orig.png`
Fiche : « Iopprimé de Armécréante de Servitude », **niv. 220, 5500 PV, 9 PA, 4 PM**, rés. 15/30/30/15/30 ; icônes 160 / 64 / 64.
- **Barbarie** : ~600 air, attire d'1 case toutes les cibles d'un cercle de rayon 3 autour de lui.
- **Moribond** : se téléporte sur la case ciblée puis ~500 feu en cercle de rayon 2 autour de l'arrivée ; jusqu'à 5 PO.
- **Traumasque** : ~600 terre, repousse de 5 cases ; mêlée.

#### Boularbin — image `dj9-boularbinfixed1_orig.png`
Fiche : niv. 203, **9000 PV, 10 PA, 6 PM**, rés. 30/30/30/20/20 ; icônes 172 / 73 / 173.
- **Compression** (schéma `dj9-compression_orig.png` : lanceur au centre, portée bleue = losange de rayon 3, cible rouge à 3 cases en ligne) : ~500 terre et vole 5 % (7 % en critique) de résistances. **Texte** : mêlée uniquement, zone croix diagonale taille 1, 2 fois par tour. **Le schéma montre pourtant une portée 3 et une zone d'1 case.**
- **Évasion** : lancé en début de combat (dure tout le combat) : +1 PM pour 1 tour s'il commence son tour au contact d'un personnage.
- **Pénitence** (schéma `dj9-penitence_orig.png` : même losange de rayon 3, cible à 3 PO) : ~600 feu et −30 fuite 1 tour ; si le Boularbin est au contact, ligne neutre supplémentaire ~500. Jusqu'à 3 PO, 2 fois par tour.
- **Renfortiche** : 100 % de résistances, 100 résistance poussée et état *Protégé* à l'allié qui a le moins de PV ; il faut frapper le Boularbin 3 fois pour retirer l'effet. Jusqu'à 10 PO, sans ligne de vue. (Conseils : lancé quand l'allié est sous 50 % PV.)

#### Ecaptif — image `dj9-ecaptiffixed1_orig.png`
Fiche : niv. 204, **7900 PV, 8 PA, 4 PM**, rés. 25/20/25/30/30 ; icônes 184 / 66 / 66.
- **Déferlante** (schéma `dj9-deferlante_orig.png` : portée = 4 cases au contact, zone = ligne de 4 cases partant de la case visée) : ~700 eau + 30 % d'érosion 2 tours, zone ligne taille 4 ; contact ; 2 fois par tour.
- **Maltraitance** : lancé sur lui-même en début de combat ; renvoie une partie des dégâts subis au contact.
- **Mégalérien** : +4 PA, +3 PM, +100 % de dommages ; seulement à partir du tour 4.
- **Pagaifrénée** (schéma `dj9-pagaifrenee_orig.png` : contact, 1 case) : ~700 air, contact.

#### Gentyran — image `dj9-gentyranfixed1_orig.png`
Fiche : **niv. 212, 7200 PV, 11 PA, 5 PM**, rés. 30/25/20/20/30 ; icônes 133 / 68 / 68.
- **Fouet Cadencé** (DPLN écrit « Cadenacé » ; schéma `dj9-fouet-cadenace_orig.png` : losange bleu, cible au bord affiché à ~4 cases, image recadrée) : 3 lignes neutres de ~500 ; jusqu'à 7 PO ; 2 fois par tour.
- **Fouet Dopant** (schéma `dj9-fouet-dopant_orig.png` : lanceur, 4 cases de portée au contact, zone rouge en triangle 1+3+5 = 9 cases devant lui) : ~500 (élément non précisé) en triangle de « sommet 3 » partant du Gentyran + 10 % critique au lanceur 1 tour ; contact, cône taille 2 ; 1 fois par tour.
- **Fouet Palliatif** (schéma `dj9-fouet-palliatif_orig.png` : grand losange, cible au bord à ~6 cases, image recadrée) : soigne un allié de ~600 (1200 en critique) + **50 fuite et 50 tacle** 2 tours ; jusqu'à 7 PO.

#### Tambourreau — image `dj9-tambourreaufixed1_orig.png`
Fiche : niv. 203, **7300 PV, 9 PA, 4 PM**, rés. 20/25/15/20/25 ; icônes 75 / 64 / 64.
- **Arbaguette** (titre de l'image « Abraguette » ; schéma `dj9-abraguette_orig.png` : losange bleu avec trou autour du lanceur = portée mini 2, cible au coin à **5** cases) : ~600 air, −2 PO 2 tours ; **2 à 5 PO**.
- **Tambourre-Pif** (schéma `dj9-tambourre-pif_orig.png` : contact) : ~600 terre en vol de vie, −30 % critique 1 tour ; contact.
- **Décadanse** (« Décadance ») : boost des alliés 2 tours selon les PV restants du Tambourreau :
  - 100 % : +10 % dommages finaux ;
  - 100-80 % : +10 % DF, +1 PO ;
  - 80-60 % : +10 % DF, +1 PO, +1 PM ;
  - 60-40 % : +10 % DF, +1 PO, +1 PM, +10 % CC ;
  - 40-20 % : +20 % DF, +2 PO, +2 PM, +20 % CC ;
  - < 20 % : +30 % DF, +3 PO, +3 PM, +30 % CC.

### III. Salles (captures à 1 personnage, frise de jeu en bas à droite — vérifiées en agrandissant)
| Salle | Image | Monstres (ordre de la frise) |
|---|---|---|
| 1 | `dj9-4fixed1_orig.jpg` | Gentyran, [joueur], Ecaptif, Boularbin, Tambourreau |
| 2 | `dj9-5fixed1_orig.jpg` | **Armécréante**, [joueur], Ecaptif, Gentyran, Boularbin |
| 3 | `dj9-6fixed1_orig.jpg` | **Armécréante**, [joueur], Ecaptif, Tambourreau, Boularbin |
| 4 | `dj9-7fixed1_orig.jpg` | **Armécréante**, [joueur], **Ecaptif, Ecaptif**, Gentyran |
| Boss | `28ark1mpazr_orig.jpg` | **Servitude, Ecaptif, Tambourreau, Boularbin** (6 cases de départ orange côté monstres, 8 cases bleues côté joueurs ; grande machine au centre de la carte) |

Toutes les salles = 4 monstres pour 1 joueur. La page ne donne pas les ajouts à 5-8 joueurs ; elle dit seulement que le **Gentyran n'est pas dans la salle du boss en butin 4**.

### IV. Boss : Servitude — image `dj9-servitudefixed1_orig.png`
Fiche : **niv. 220, 26 000 PV, 11 PA, 7 PM**, rés. 20/15/30/25/25 ; icônes 230 / 70 / 70 / 0 / **200** (dernière icône orange = renvoi de dommages 200).

Sorts :
- **Esclavagisme** : invoque une Armécréante à 2 PO de lui ; **6 Armécréantes maximum** ; relance 2 tours.
- **Asservissement** : attire la cible de 9 cases (s'arrête au contact de Servitude) ; Servitude gagne 100 tacle 1 tour, moins 10 tacle par case qui le séparait de la cible au lancer. **Phase 2** : vole aussi 1 PM (1 tour) et ~650 air. En ligne, 2 à 10 PO, 1 fois par tour.
- **Joug Protecteur** : ~700 eau, puis 1000 de bouclier (infini) et +1 PM (1 tour) à l'allié de Servitude **le plus proche de la cible**. **Phase 2** : Servitude reçoit lui aussi le bouclier et le PM. Jusqu'à 5 PO.
- **Tyrannie** : ~900 eau, −1 PA en cercle de rayon 2 autour de lui. **Phase 2** : peut être lancé sur une Armécréante, qui **échange sa place avec le personnage le plus proche de Servitude** (quelle que soit la distance). 1 fois par tour.
- **Trahison** : uniquement s'il existe un personnage en état « Traître » ; cible un porteur de l'état n'importe où sur la carte, l'attire, et le **force à frapper son allié le plus proche dans un élément au hasard (25 % chacun)** ; dégâts calculés sur les caractéristiques du personnage. **Phase 2** : sur tous les porteurs de l'état Traître.

Principe du combat :
- Donjon le plus dur des 4 Cavaliers. Plus simple à 8 qu'à 4 : **le nombre et les caractéristiques des Armécréantes ne dépendent pas du nombre de joueurs**.
- **Armécréantes** : invoquées **à chaque tour impair dès le tour 1**, 6 en vie maximum, à 2 PO de Servitude, **en tournant dans le sens horaire** (case suivante si la case prévue est prise). Image `dj9-invocation-armecreantefixed1_orig.png` : Servitude (rouge) et l'Armécréante (bleu) à 2 cases **en ligne**.
- Les Armécréantes invoquées ont **−20 % de PV** ; leur **niveau dépend du butin** (niv. 200 à 4 joueurs, niv. 212 à 8).
- **Renvoi** : Servitude renvoie **200 %** des dégâts reçus (pas les poisons). Protection = état **Traître**.
- **Glyphes blancs** (image `dj9-glyphe-traitrefixed1_orig.jpg` : case blanche entourée près d'un personnage) : finir son tour dessus donne **Traître 1** (image `dj9-etats-de-traitrefixed1_orig.jpg` : icône d'état rouge « 1 » au-dessus du personnage ; image `dj9-esclavagismefixed1_orig.png` : infobulle « Esclavagisme — Lanceur : Servitude — État Traître 1 (Infini) »).
- Chaque niveau de Traître = 1 ligne de dégâts sur Servitude sans renvoi ; chaque ligne sur Servitude **consomme** 1 niveau (Traître 3 → 3 lignes protégées, la 4e est renvoyée). Durée infinie, seulement perdu en tapant Servitude.
- Pour **monter** de niveau : le porteur frappe un allié (**+1 par ligne de dégâts, maximum 5**). Il faut déjà Traître 1 (obtenu par le glyphe) ; taper un allié sans l'état ne donne rien ; finir plusieurs fois sur un glyphe ne dépasse pas Traître 1.
- Immunités qui contournent le renvoi : Bastion, Barricade, Prohibition, Ataraxie.
- **Phases** : 1 = jusqu'à 50 % PV ; 2 = de 50 % à 1 PV (monte sur sa monture, sorts améliorés) ; 3 = à 1 PV : **rollback de toute la carte (toutes les entités à leur position de début de combat) + soin de 25 % de ses PV** ; il faut alors finir ces 25 %.
- L'état **Insoignable** posé avant le passage à 1 PV annule le soin ; un poison posé avant le fait mourir au début de son tour.

Classes conseillées : Pandawa (placement, insoignable, pesanteur, Stabilisation contre le rollback des Armécréantes), Féca (protections = taper sans Traître, pesanteur), Eliotrope (portails, focus), Iop (Colère).

Conseils tactiques (utiles pour l'IA) :
- Dofus Nébuleux pour achever les Armécréantes (invoquées tours impairs).
- Domestication est exploitable : la cible sait qu'elle sera ramenée à sa case de début de tour.
- Ataraxie (pesanteur) empêche le retour forcé de l'Armécréante si on n'a pas été retouché.
- Monter Traître en frappant ses alliés avec des sorts peu chers ; ne jamais consommer son dernier niveau.
- **Armécréantes : seulement 15 % de résistance eau.**
- Pesanteur / Enraciné / Indéplaçable ou un désenvoûtement contrent le retour de Domestication.
- **Frapper chaque Armécréante au moins une fois par tour** pour lui retirer *Appel des renforts* (sinon Iopprimé).
- **Ecaptif dangereux au tour 4** (+3 PM, +100 % dommages, beaucoup d'érosion).
- Ne pas taper le Tambourreau ou le tuer d'un coup (Décadanse augmente quand ses PV baissent).
- Renfortiche : 100 % rés. à un allié sous 50 % PV, 10 PO, 3 coups sur le Boularbin pour l'annuler.

Stratégie globale : tuer chaque Armécréante dès son apparition ; ordre Ecaptif (avant tour 4) → Gentyran (absent en butin 4, frappe fort et soigne) → Boularbin → Tambourreau en dernier ; garder Boularbin et Ecaptif à distance ; puis Servitude avec Traître 5 (ou Féca / poisons) ; en phase 2 il risque d'avoir 1000 de bouclier à chaque tour ; Insoignable + poison avant 1 PV.

### V. Succès
- **Focus** (icône `focus.png`, cadenas) : finir l'ennemi entamé avant d'en toucher un autre. Avant la 2.68, l'idole **Djim** aidait (image `dj9-focus-with-djimfixed1_orig.png` : « Djim » niv. 161, monstres invulnérables tant qu'ils n'ont pas été déplacés, dommages de poussée = +200 % dommages pour eux). Ordre : Ecaptif, Gentyran, Boularbin, Tambourreau ; finir Servitude un tour **pair** (pas d'Armécréante) ; Insoignable + poison ; attention au seuil de 50 %. Variante Sram/Féca : réseau de pièges pour tuer Servitude au tour 5 ou 7.
- **Versatile** (icône `versatile.png`) : chaque action une seule fois par tour ; stratégie globale ; Féca ou Pandawa tank + 2 DD + soigneur.
- **Spécial « Bain de foule »** (icône `special.png`) : **ne pas achever les Armécréantes** ; il faut quand même les taper chaque tour (Appel des renforts) ; tuer Servitude entre le tour 5 et le tour 8 ; jouer terre (**Servitude n'a que 15 % de résistance terre**).
- **Score 200** (icône `dj9-score200fixed1_orig.png`) : compositions d'idoles `dj9-composition1fixed1_orig.jpg` (Yoche magistrale, Yoche majeure, Yoche, Yoche mineure, Nékinéko, Hulhu mineure — score total 211) ou `dj9-composition2fixed1_orig.jpg` (Yoche magistrale, Yoche majeure, Nékinéko, Djim — 217). Aucun monstre ne doit jouer après avoir subi des dégâts ; finir Servitude en 1 tour.
- **Duo** (icône `duo.png`) : 2 personnages, moins de 40 tours. Sram terre/réseau + Féca (tuer Servitude d'abord, au tour 7 dans le réseau : il passe ses 3 phases, rollback puis meurt) ; Eliotrope + Iop ; Eliotrope + autre classe. Images `dj9-duo-elio1..4` : carte de la salle du boss vue de dessus, portails (bleu), personnages (vert), Servitude (rouge), monstre/Armécréante (violet) ; boucle de portails pour que l'Armécréante invoquée en tour impair traverse un portail, frappe et se repousse elle-même dans un autre portail. Servitude avance de 7 cases par tour (7 PM) ; un retrait de PM casse la boucle. Placement repris d'une vidéo de « Keity ».

Images de décor du site (ouvertes, aucune donnée de jeu) : logo `1426805183.png`, avatar `arkaw.jpg`, bannières `promo-maj307`, `promo-raids-guilde`, `promo-saison-ivoirec`, `promo267-resolution-chasse`. `arrowup2.png` (flèche de thème) non téléchargée. **Toutes les images de contenu ont été ouvertes.**

---

## (b) ÉCARTS DPLN / nos données

Rappel d'échelle : DPLN donne les dégâts **au niveau du monstre** (environ base × 8 à 9 avec 700-800 de caractéristique). Exemples cohérents : Tyrannie 101-110 → ~900 ; Joug 76-85 → ~700 ; Domestication 61-70 → ~600 ; Barbarie 61-75 → ~600 ; Moribond 51-65 → ~500 ; Déferlante 66-85 → ~700. **Aucun écart de dégâts significatif** sauf ceux listés plus bas.

### BLOQUANT

| # | Sujet | DPLN | Nous | Correction proposée |
|---|---|---|---|---|
| B1 | **Passif de Servitude (monstre 5955)** | Invocation d'Armécréantes, glyphes blancs, renvoi 200 %, Traître 1-5, phases 50 % / 1 PV, rollback + soin 25 % | Tout est porté par **Esclavagisme 15104** (niveau de sort 45305 = `startingSpellId` du grade dans Monsters.d2o 2.74). **15104 n'est pas dans `monsters_spells`**, `monsters_grades` n'a pas de colonne de sort de départ, et **aucun `BrainIdentifier(5955)`** dans le serveur → le passif n'est jamais posé. Sans lui : pas d'Armécréante, pas de glyphe, pas de renvoi, pas de phase 2 ni 3. | Créer un brain Servitude (modèle `BrainPassif` / `SortPassif.Poser(fighter, 15104, 1)`) et vérifier toute la chaîne 15104 → 15679 / 15105 / 15106 / 15107 / 15108 / 15278 / 15445 / 28796 (voir partie c). |
| B2 | **Composition salle 2** (map 203162624) | Armécréante, Ecaptif, Gentyran, Boularbin | Gentyran, Boularbin, Ecaptif, **Tambourreau** | Remplacer le Tambourreau du groupe de base (MinPartyMembers 1) par une **Armécréante (5979)**. |
| B3 | **Composition salle 3** (map 203163648) | Armécréante, Ecaptif, Tambourreau, Boularbin | Boularbin, Ecaptif, Tambourreau, **Gentyran** | Gentyran → **Armécréante (5979)**. |
| B4 | **Composition salle 4** (map 203164672) | Armécréante, **2 Ecaptif**, Gentyran | Ecaptif, **Tambourreau**, Gentyran, **Boularbin** | Base = Armécréante + Ecaptif + Ecaptif + Gentyran. |
| B5 | **Composition salle du boss** (map 203165696) | Servitude, **Ecaptif**, Tambourreau, Boularbin (Gentyran absent en butin 4) | Servitude, Tambourreau, **Gentyran**, Boularbin ; l'Ecaptif n'apparaît qu'à 8 | Base : Gentyran → Ecaptif ; le Gentyran ne vient qu'en ajout (5+). |
| B6 | **Armécréante (5979) absente des salles** | Monstre normal des salles 2-4 + invocation du boss | Existe dans `monsters_templates` / `monsters_grades` / `monsters_spells` (15143, 15144, 15150), mais n'est placée dans aucun groupe et n'est pas dans l'état d'audit | Ajouter aux groupes (B2-B4). |
| B7 | **Passif de l'Armécréante : Escarmouche 15142** (niveau 45393) | Appel des renforts → Iopprimé si elle n'a pas été touchée | Seul le sort 15143 (pose l'état 1551) est dans ses sorts ; le passif 15142 (début de tour : invoque 5981 si état 1551 ; dégât d'ennemi : retire 1551) n'est jamais posé. Pas de brain 5979. | Brain 5979 : `SortPassif.Poser(f, 15142, 1)`. |
| B8 | **IA de Servitude** | Sorts de phase 1 ou de phase 2 selon la phase ; Trahison seulement s'il y a un Traître | Les 12 sorts (14984, 14987, 14988, 14991, 15109, 15110, 15112-15115, 15129, 15133) sont tous dans `monsters_spells`, **sans état requis** : l'IA standard pioche dans les deux phases et lance Trahison sans cible valable | Brain : phase 1 (état 1524) → 14987, 14988, 14991, Trahison 14984/15112-15115 ; phase 2 (état 1525) → 15109, 15110, 15133, 15129. |

### IMPORTANT

| # | Sujet | DPLN | Nous | Correction proposée |
|---|---|---|---|---|
| I1 | Limite d'invocations de Servitude | 6 Armécréantes en vie | `StatsJSON SummonLimit = 3` (5955) | Mettre 6, ou ignorer la limite pour l'invocation du passif. |
| I2 | Limite d'invocations de l'Armécréante | 2 Iopprimés par Armécréante | `SummonLimit = 1` (5979) | Mettre 2. |
| I3 | Niveau / PV des Armécréantes invoquées | Niv. 200 à 4 joueurs, 212 à 8 ; −20 % PV | Effet 181 « Invoque 5979 » sans grade précisé | Choisir le grade selon le nombre de joueurs (1-4 → grade 1, 8 → grade 5) et appliquer −20 % PV. |
| I4 | Rythme des invocations | Tours impairs dès le tour 1, à 2 PO en ligne, sens horaire, max 6 | 15679 (durée 1, début de tour) lance 15105 (zone C5,2, cible `*f6783`) puis se retire. Rien de visible ne le relance tous les 2 tours. | Vérifier en jeu ; sinon gérer le cycle dans le brain (tour impair → 15105) et le placement horaire. |
| I5 | **Pénitence 15174** (Boularbin) | Jusqu'à **3 PO**, **2/tour**, feu ~600, **−30 fuite**, ligne neutre en plus au contact | **1-1 PO**, 1/tour, neutre cible + feu et **−50 fuite** en zone `+1` (croix diagonale) | À trancher : DPLN décrit une version plus ancienne (le schéma montre portée 3). Garder le 2.74 sauf décision contraire, mais l'IA doit aller au contact. |
| I6 | **Fouet Palliatif 15151** (Gentyran) | +**50** fuite / +**50** tacle 2 tours, soin ~600 | +30 tacle / +30 fuite | Écart de valeur (DPLN 2019-2024 ou client 2.74) : à trancher, le soin est cohérent. |
| I7 | **Fouet Dopant 15156** niv. 2 | +10 % CC **au lanceur** | « 10 % Critique, cible A » (A = ennemis), relancé sur les cibles de la zone V2 | Vérifier qui reçoit le bonus : si notre moteur l'applique aux joueurs touchés, c'est un bug (cible attendue : le Gentyran). |
| I8 | **Arbaguette 15138** (Tambourreau) | 2 à **5** PO (texte + schéma) | 2 à **6** PO, LdV, 2/tour, 1/cible | À trancher (2.74 probablement à 6). |
| I9 | Tyrannie phase 2 (15133) | Lancé **sur une Armécréante** : elle échange sa place avec le joueur le plus proche de Servitude | 15133 niv. 1 = dégâts + vol PA autour de lui + état 1634 ; niv. 2 / 3 = échange de positions (zone C63, cibles `e1634`, `f6120`) | La logique « Armécréante ↔ joueur le plus proche » doit être vérifiée en jeu (effet 2160 + masques) ; l'IA doit viser une Armécréante. |
| I10 | Joug Protecteur (14988 / 15110) | Bouclier 1000 + 1 PM à l'allié **le plus proche de la cible** | 14988 se relance (2160) sur les alliés en zone C63,1 ; niv. 2 = bouclier + PM (masque `g,f5955,f6783`) | Vérifier que le moteur choisit l'allié le plus proche (effet 2160) ; phase 2 : Servitude se donne aussi le bouclier (15110 cible C) ✔. |
| I11 | Passif Boularbin **Évasion 15165** (niveau 45446) | +1 PM 1 tour s'il commence au contact | Pas dans `monsters_spells`, jamais posé | Brain 5983 : `SortPassif.Poser(f, 15165, 1)`. |
| I12 | Passif Ecaptif **Maltraitance 15177** (niveau 45466) | Renvoie une partie des dégâts reçus au contact | Pas dans `monsters_spells`, jamais posé ; niv. 2 = « Répartit 30 % des dommages subis » au déclencheur DM (dommages de mêlée) | Brain 5989 : `SortPassif.Poser(f, 15177, 1)`. |
| I13 | Trahison | Attire, force à taper l'allié le plus proche, élément au hasard 25 % | Sort par classe (14985, 15035…, 24905 Forgelance) avec 4 sous-sorts à `random 25` ✔ ; `Attire de 1 case` ✔ | Vérifier en jeu les effets 792 / 2160 (lancer **par la cible**) et le ciblage « allié le plus proche » (`cible g`, zone a1). |
| I14 | Ancrépulsion 15144 (Armécréante) | Le **lanceur recule de 2 cases** | Attire 1 + dégâts + repousse 3 + niv. 2 « Pousse de 3 » (effet 1103) ; aucun recul du lanceur visible | Vérifier l'effet 1103 et l'animation 15462 ; recul du lanceur absent des données ? |

### DÉTAIL

| # | Sujet | DPLN | Nous | Remarque |
|---|---|---|---|---|
| D1 | Compression 15169 | Texte : contact, croix diagonale, 2/tour ; schéma : portée 3, 1 case | 1-3 PO, case occupée obligatoire, 2/tour, 1/cible, 61-70 terre, vole 5 % rés. | Nos données = schéma DPLN. Le texte DPLN semble mélanger Compression et Pénitence (la croix diagonale est chez nous dans Pénitence). OK. |
| D2 | Pagaifrénée 15183 | ~700 air | 41-60 air (~450) + 40 % de chance de se relancer | Moyenne cohérente avec 1 à 2 lignes. OK. |
| D3 | Barbarie 15161 | 600 air, attire 1, cercle rayon 3 | **Vol** de vie air, zone `C3,2` (anneau 2-3 ?) | DPLN ne parle pas de vol de vie ; vérifier si la zone C3,2 exclut les cases proches. |
| D4 | Asservissement 14987 / 15109 | +100 tacle 1 tour, −10 par case | 14987 : +100 tacle 1 tour ✔ ; 15109 : +100 tacle **sans durée** + malus −10 à −90 en anneaux (niv. 2) ✔ | Vérifier la durée de 15109 (0 = permanent ?). |
| D5 | Niveaux de grade | Ecaptif niv. 204 (7900 PV), Gentyran niv. 212 (7200 PV) | Ecaptif 200/203/206/209/212 (7900 PV = 203) ; Gentyran 212 = 7200 ✔ | Négligeable. Les salles utilisent le **grade 3** pour tous les monstres. |
| D6 | Noms | « Fouet Cadenacé », « Abraguette », « Décadance », « Bourlarbin » | Fouet Cadencé, Arbaguette, Décadanse, Boularbin | Fautes de frappe DPLN, nos noms sont bons. |
| D7 | Décadanse 15135/15137 | 6 paliers | 6 paliers identiques ✔, relance 2 | OK. |
| D8 | Mégalérien 15182 | À partir du tour 4 | Relance initiale 3, relance 63, +4 PA / +3 PM / +100 % DF infinis ✔ | OK. |
| D9 | Déferlante 15179 | Ligne de 4, 30 % érosion 2 tours, 2/tour | Zone L3, 30 % érosion 2 tours, 2/tour ✔ | OK. |
| D10 | Domestication 15150 | 3-10 PO sans LdV, 2/tour | Identique ✔ (+ « Téléporte à la position de début de tour », déclencheur TE) | Vérifier le déclencheur TE (fin de tour). |
| D11 | Renfortiche 15167 / 15168 | 100 % rés., 100 rés. poussée, Protégé, 3 coups | Identique ✔ (états 1555 → 1554 → 1553) | IA : viser l'allié < 50 % PV. |
| D12 | Clef 22028 | Texte : 1/1/1/2/2/2/2/2 | Recette : 22218, 22216, 22224, 22223, 17160 ×2 ; 11106, 11102, 11109 ×1 ✔ | Conforme au texte (l'image montre la recette plus récente 7/7/5 : à ignorer). Noms d'objets en espagnol dans `items_templates` (Campanilla de invierno, Inverluza, Carne sabrosa…) : hors sujet ici. |
| D13 | Entrée | [4,-3], Galère de Servitude, PNJ Miradar | Map 202115584 [4,-3] (sous-zone 925), PNJ 5319 « Miradar » cellule 245 ; sortie 203165698 cellule 273 ✔ | OK. |
| D14 | Stats Servitude | 26000 PV, 11 PA, 7 PM, 20/15/30/25/25 | Grade 1 identique ✔ (spawn = grade 1, 970000) | OK. Les grades 6-10 (26000-28000, rés. de base) : inutilisés ? |
| D15 | Stats Iopprimé 5981 | Niv. 220, 5500 PV, 9 PA, 4 PM, 15/30/30/15/30 | Grade 1 : niv. 200, 5500 PV ✔ | Le niveau 220 affiché vient de l'invocateur ; sans effet. |
| D16 | Succès | Focus, Versatile, Spécial « Bain de foule » (ne pas achever les Armécréantes), Duo (< 40 tours), Score 200 | Aucun succès « Servitude / Fers / Bain de foule » trouvé dans `achievements_templates` (les noms y sont peut-être dans une autre langue) | À vérifier avec l'outil des succès ; le challenge « Bain de foule » demande du code (Armécréante achevée = échec). |

---

## (c) Mécaniques qui demandent du code serveur

1. **Brain Servitude (5955)** : poser le passif 15104 au début du combat ; choisir les sorts selon l'état de phase 1524 / 1525 ; Trahison seulement si un ennemi a un état 1537-1541 ; en phase 2, Tyrannie sur une Armécréante si c'est utile.
2. **Chaîne Esclavagisme** (tout est dans les données 2.74, effets à vérifier un par un) :
   - 15104 niv. 1 : état 1524, 15679 (début de tour → 15105 = invocation + glyphes), 15107 au déclencheur DBE (dégâts d'ennemi → **renvoi 200 %** sauf porteur Traître, et Traître N → N−1), 15108 sur tous les ennemis (déclencheur DBA : **un Traître qui frappe un allié monte de 1, maximum 5**), 15278 (**seuil 50 %**), 15445 (**seuil 1 PV**), 28796.
   - 15105 niv. 2/3 : effet **402 « glyphe de rang 1 », couleur blanche 16777215, sort 15106, durée infinie** → 15106 niv. 1 : Traître 1 si on n'a aucun Traître ; niv. 2 : effet **2018** sur Servitude (à identifier).
   - Seuil 50 % → 15104 niv. 2 : état 1525, retire 1524, retire 15278, **change l'apparence (effet 149, monture)**, état 5156 « Blocage Seuil », 28796.
   - Seuil 1 PV → 15104 niv. 3 : **soin 25 % des PV max (1109)**, retire 15445, **retour de toutes les entités à leur position de départ (784, zone A1)**, retire 5156. DPLN : l'état Insoignable doit empêcher le soin.
   - Effets à confirmer dans le moteur : 2872 (seuil, déjà codé dans `EffetsEliocalypse.cs`), renvoi 200 % (déjà codé, effet 1223), 402 avec sort lié, 2160, 2018, 784, 1109, 406, déclencheurs TB / DBE / DBA / TR<sort>.
   - **Cycle « tours impairs »** et **placement horaire à 2 PO en ligne** : à vérifier ; sinon à coder.
   - Limite de 6 Armécréantes ; grade selon le nombre de joueurs ; −20 % PV.
3. **Brain Armécréante (5979)** : passif 15142 (Escarmouche : début de tour → Iopprimé si 1551 ; dégât d'ennemi → retire 1551) ; limite de 2 Iopprimés ; IA : Domestication de loin (3-10), Ancrépulsion au contact.
4. **Domestication** : retour à la case de début de tour à la fin du tour suivant (déclencheur TE) ; désenvoûtable ; bloqué par pesanteur / enraciné / indéplaçable.
5. **Brains Boularbin (5983) et Ecaptif (5989)** : poser Évasion 15165 et Maltraitance 15177.
6. **Renfortiche** : compteur de 3 coups (états 1555 / 1554 / 1553 avec 15168 au déclencheur DBE) : à vérifier en jeu ; IA : allié qui a le moins de PV (ou < 50 %).
7. **Trahison** : sorts lancés par la **cible** (effets 792 / 2160) sur son allié le plus proche, élément au hasard (random 25) ; attire 1 case ; en phase 2 (15129), sur tous les porteurs de Traître.
8. **Challenge / succès « Bain de foule »** : échec si un joueur achève une Armécréante.
9. **Groupes de salles** : corriger les groupes de base des salles 2, 3, 4 et du boss (SQL `monsters_spawns_dungeons_groups`), à mettre dans `PUSH/fers-de-la-tyrannie-N` comme toute correction de donjon.

### Notes pour l'IA (tirées des tactiques DPLN)
- Servitude : attirer avec Asservissement (2-10 en ligne), Tyrannie quand ≥ 1 ennemi à 2 cases ou moins, Joug sur la cible la plus proche d'un allié blessé, Trahison dès qu'un Traître existe.
- Armécréante : Domestication de préférence sur les joueurs qui avancent (3-10 PO, sans LdV) ; Ancrépulsion en ligne au contact.
- Ecaptif : aller au contact, Déferlante ×2 en ligne ; Mégalérien au tour 4.
- Boularbin : Renfortiche sur l'allié le plus bas ; Compression / Pénitence.
- Gentyran : Fouet Cadencé à distance (≤ 7), Fouet Palliatif sur un allié blessé, Fouet Dopant au contact.
- Tambourreau : Décadanse dès que possible (relance 2) ; Arbaguette à distance ; Tambourre-Pif au contact.

---

## (d) Doutes

1. **Version** : l'article date de 2019, mis à jour le 26/03/2024 (vers la 2.71). Nos sorts viennent du client 2.74. Pour Pénitence (portée 3 → 1), Fouet Palliatif (50 → 30), Arbaguette (5 → 6) et le texte de Compression, la version plus récente est peut-être la bonne. **À trancher par toi.**
2. Les captures de salles sont à 1 joueur : les ajouts à 5-8 joueurs ne sont pas sur la page ; seul « Gentyran absent en butin 4 » est écrit. Nos groupes 5-8 n'ont pas été vérifiés.
3. Schémas de Fouet Cadencé et de Fouet Palliatif : les images sont recadrées (bord à ~4 et ~6 cases), elles ne prouvent rien contre 7 PO.
4. Effet 2018 (15106 niv. 2 sur Servitude) et masque `*f6783` / `F6783` (monstre 6783 = autre apparence de Servitude, monté ?) : non identifiés.
5. Le passage d'un tour impair au suivant pour l'invocation (15679 se retire au début du tour) : je n'ai pas trouvé ce qui relance l'invocation 2 tours plus tard dans les données ; c'est peut-être dans le niv. 2 de 15105 (effets 2160 avec l'état `*e2688`), à tester.
6. Les icônes chiffrées des fiches (1re colonne : 230 Servitude, 83 Armécréante…) ne sont pas identifiées avec certitude (tacle ? initiative ?). Le « 200 » de Servitude = renvoi de dommages, très probablement.
7. Succès : non trouvés dans `achievements_templates` par leur nom français ; à confirmer.
8. Si le moteur applique `SummonLimit` aux invocations faites par effet de sort, la limite de 3 (Servitude) et de 1 (Armécréante) bloque la mécanique ; sinon c'est sans effet.
