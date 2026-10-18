# Lot 7 — audit des mécaniques (dofuspourlesnoobs → Mystic 2.51)

Méthode : page du site lue en entier, sorts réels en base (`monsters_spells`, grade max) + effets du client 2.51
(`analyse_mecaniques.py`), recherche des passifs non attribués (2e argument), lecture du code (`AI/Fights/Brain/Custom/Boss/*`,
`game/effects/handlers/**`, critères de cible). Règle VERSION appliquée : seul ce qui existe dans le client 2.51 est proposé à
coder ; tout écart de chiffres ou sort absent du client = **[DOUTE VERSION]**, hors corrections.

Rappels moteur utiles pour tout le lot (vérifiés dans le code) :
- `SortPassif.Poser(fighter, sort, grade)` / `BrainPassif` : pose le passif au début du combat ou à l'arrivée (invocation / vague).
- `793` (TriggerBuff_793) : c'est la **cible du buff** qui lance le sous-sort sur sa propre case (`CastSpell.cs`, DefaultBuffTrigger).
- Critère `U` (`JustSummonedCriterion`) = `actor.IsSummoned() && actor.SummoningEffect == un effet du sort en cours`.
- Critère `C` : la case du lanceur est toujours ajoutée à la zone (`SpellEffectHandler.GetAffectedActors`).
- `e<n>` = n'a PAS l'état n, `E<n>` = a l'état n ; `v<n>` = PV > n % (strict), `V<n>` = PV <= n %.

---

## Donjon des Tofus (donjon-des-tofus) — boss Batofu (id 800)
### Mécanique officielle (site)
- **Tofoune (804)** : Graine Empoisonnée (poison neutre de fin de tour, 5 dégâts par PA utilisé, 5 tours, mêlée, 1×/cible) ;
  Mot de Jeunesse (−1 tour d'envoûtement + soin 10 % PV max, 6 PO modifiable, 1×/cible) ; Picota (+2 à 3 PA et soin 40 aux alliés
  en cercle 2 autour d'elle, 1×/tour).
- **Tofukaz (806)** : Disparition groupée (invisibilité 2 tours sur lui et les alliés en cercle 2, relance 4, utilisable dès le
  tour 2) ; Provocation (70 air, −1 PM 2 tours, mêlée).
- **Tofu Mutant (4824)** : Bigle (60 feu, −4 PO 2 tours, zone bâton, 1 PO) ; Mutation Prolifique (soin 15-20 %, +2 PM et +60
  puissance 2 tours, relance 3).
- **Tofu Noir (796)** : Béco Rosif (60 eau, 20 % d'érosion 2 tours, 2 PO) ; Fougue Tofuesque (pousse 1, recule 1, vol 2 PM,
  soin 10 %, mêlée).
- **Tofu Ventripotent (808)** : Bénédiction du Tofulailler (tous les alliés : +21-50 vita, +1 PA, +31-50 agi, 4 tours,
  relance 2) ; Invocation de Bomberfu (1 PO). **Bomberfu (802)** : explose (30 feu, cercle 2) et meurt.
- **Batofu (800)** : Béco de Batofu (110 air, 2 PO) ; Gotame (invoque un Tofu Noir, en ligne 3 PO, relance 5) ; Lancer de Tofu
  Fugace (invoque un Tofu Fugace en ligne jusqu'à 10 PO, relance 2 ; la case derrière est repoussée d'1 et subit 30 neutre).
- **Tofu Fugace (4876)** : **Fugacité (passif) : meurt automatiquement à la fin de son 2e tour** ; Béco du Tofu (50 air).
- Règles : IA fuyarde de la plupart des Tofus (8-12 PM) ; Batofu 36 % rés. air ; aucun autre mécanisme spécial. Salle 2 = trous.

### État sur le serveur
- Spawns 36-40 (« Batofu ») : Tofu (98), Tofu Noir, Tofukaz, Tofoune, Tofu Ventripotent, Batofu. **Tofu Mutant (4824) absent des
  groupes** (remplacé par des Tofus 98).
- Sorts en base conformes au client 2.51 pour tous les monstres ; aucun effet ni déclencheur non géré (outil : « rien »).
- **Tofu Fugace (4876)** : le passif **Fugacité 8712** existe dans le client (Tuer soi-même, déclencheur `TE`, durée −1,
  `delay 1` = fin du 2e tour) mais n'est attribué à rien et **aucun cerveau** pour 4876 → les Tofus Fugaces ne meurent jamais
  et s'accumulent (un tous les 2 tours).
- Batofu a en plus « Liberté » 8714 (2.51, repousse 2 autour de lui), absent du site : normal en 2.51.
- Pas de cerveau Batofu (aucun passif officiel à lancer : conforme).
- [DOUTE VERSION] chiffres : Béco du Batofu 17-26 air (site 110), Picota soin 11-15 (site 40), Disparition groupée relance 6
  avec relance initiale 1 (site relance 4, dès le tour 2 → la règle « dès le tour 2 » est bien en 2.51), Mutation, etc. :
  valeurs 2.51 à garder.
- [DOUTE VERSION] IA fuyarde : aucune donnée « IA » dans le client 2.51 (Monsters.d2o) ; le site décrit le comportement actuel.
  Le moteur sait le faire (`Brain.Fuyard`, cf. `FongeurBrain`), mais ce n'est pas une donnée 2.51 : à décider par l'utilisateur.
- [DOUTE VERSION] composition : Tofu Mutant 4824 absent des groupes (le site le liste) ; choix de composition, pas une mécanique.

### À faire (précis, prêt à coder)
1. [IMPORTANT] Fugacité du Tofu Fugace. Dans `AI/Fights/Brain/Custom/Boss/PassifsDonjonsBrains.cs`, ajouter :
   `[BrainIdentifier((int)MonsterIdEnum.TOFU_FUGACE)] public class TofuFugaceBrain : BrainPassif { public TofuFugaceBrain(AIFighter f) : base(f, 8712) { } }`
   (4876, invoqué en cours de combat → `SortPassif.Poser` passe bien par `FighterAdded`). Test : le Tofu Fugace doit mourir à la
   fin de son 2e tour (le `delay 1` de l'effet 141 est géré par les buffs à délai). S'il meurt dès son 1er tour, c'est que le
   délai n'est pas décompté sur un buff déclenché : le signaler.

---
## Transporteur de Sylargh (transporteur-de-sylargh) — boss Sylargh (id 3409)
### Mécanique officielle (site)
- **Kanimate (3404)** : Arc éclectique (sur lui 1 tour : **Retour historique** = chaque ligne de dégâts subie renvoie l'attaquant
  à sa position précédente ; téléfrag → 950 eau à l'attaquant et à l'entité téléfraguée, relance 2) ; Machine Ception (550 terre
  vol de vie + renvoie la cible à sa position de début de tour, ligne 6 PO) ; Marionnettoyage (550 air, −6 PO 1 tour, 3-12 PO).
- **Brikoglours (3405)** : Clef battue (700 terre, attire 2, ligne 6 PO sans LdV) ; Clef en glaise (550 feu vol de vie + 40 %
  des PV manquants du lanceur en eau, 2 PO) ; Clef nergumène (+50 tacle, **Indéplaçable** 1 tour ; chaque coup en mêlée subi
  donne +500 dommages aux monstres alliés en ligne de 2 à 6 PO et +50 aux ennemis, relance 2).
- **Mansordide (3406)** : Engrenage de glace (550 eau, 30 % érosion, cercle 2 autour de lui) ; Envol de mort (soigne un allié de
  550, 550 feu + repousse 2 les ennemis au contact de l'allié, ligne 6 PO sans LdV) ; Rotaplumes (+200 soins 2 tours +
  **Rotativernal** : dégâts subis à distance renvoyés à 100 % à toutes les entités alignées puis soin 400 des alliés alignés,
  relance 3).
- **Mécanofoux (3407)** : Dé grippant (650 neutre, −4 PA, zone bâton, 1 PO) ; Pétarade (550 eau + pousse 1, ligne de 2,
  3-12 PO sans LdV, case vide possible) ; Rotapousse (+200 dommages de poussée 2 tours + **Expulsar** : quand il subit des
  dégâts, repousse de 4 toutes les entités en étoile de 4, relance 3).
- **Mérulor (3408)** : Brûlage bête (600 feu, −200 rés. poussée, 3-12 PO) ; Dégage limite (+500 dommages de poussée 3 tours puis
  repousse 3, mêlée) ; Vidange gardien (sacrifice sur les entités à 2 PO + 200 dommages 2 tours, relance 3).
- **Sylargh (3409)** : **Chambranle (passif) : invulnérable au départ, Lourd tout le combat**. Dégage de qualité (+500 dommages
  et +200 tacle 1 tour à un monstre allié puis le repousse de 3, mêlée, relance 2) ; Mortier (650 terre + 650 eau à chaque ligne
  de dégâts en mêlée subie, 10 PO modifiable) ; Poinçon (1 100 air vol de vie, désenvoûte, −100 fuite 2 tours, ligne de 2, 1 PO).
- **Nécromignon** (posé par le Sylargh sur tous les monstres) : quand un monstre meurt, **le prochain monstre qui joue le
  réinvoque à exactement 6 PO (1re case libre, sens horaire) avec 10 % de ses PV, état Zombi** ; le réinvoqué est
  **Indéplaçable 1 tour**, a **+300 dommages 2 tours**, **ne joue pas son 1er tour**, est l'invocation de celui qui l'a relevé
  (tuer le releveur tue la chaîne). Le Sylargh lui-même est relevé s'il meurt avant les autres.
- **Zombidule** (mort d'un Zombi) : si le Sylargh est entre 5 et 10 PO, il **perd son invulnérabilité 1 tour** ; 1 200 feu
  dégressifs en cercle 5 (alliés et ennemis) ; +100 dommages 2 tours (cumul 3) ; tue les autres Zombis du cercle ; état
  **Pacifiste** 1 tour aux personnages à 90 % PV ou plus.
- Victoire : tuer le Sylargh après lui avoir retiré l'invulnérabilité via les Zombis.

### État sur le serveur
- Spawns 360-364 conformes (Kanimate, Brikoglours, Mansordide, Mécanofoux, Mérulor, Sylargh). Sorts en base = client 2.51 ;
  aucun effet / déclencheur non géré (outil : « rien », l'effet 1034 a désormais un gestionnaire `ReviveLastDeadAlly.cs`).
- Cerveau `SylarghBrain` (PassifsDonjonsBrains.cs) : pose bien **Chambranle 3606** (invulnérable + Lourd + buff `TB` sur tous
  les monstres qui lance **Nécromignon 3607** à chaque début de tour).
- **Nécromignon ne marche qu'à moitié** à cause de `ReviveLastDeadAlly` (effet 1034) :
  - le relevé n'a pas `SummoningEffect` et n'est pas `IsSummoned()` → le critère `U` (`JustSummonedCriterion`) est faux → les
    effets `a,U` de 3607 ne s'appliquent pas : **pas de +300 dommages, pas d'Indéplaçable, pas de passage du 1er tour, pas de
    Zombigre 3691** (donc le relevé ne relèvera jamais personne et ne reçoit jamais Zombidule) ;
  - le relevé n'a pas l'**état Zombi (74)** → le buff **Zombidule 3608** (`793`, cible `C,E74`, déclencheur `X` = à la mort)
    n'est jamais posé (il exige que le lanceur de Nécromignon soit Zombi), et le `Kill a,A,E74` de Zombidule ne trouve personne ;
  - le relevé est placé sur la case visée ou une case **adjacente** au releveur, alors que toute la zone de 3607 est
    `C10,6` (anneau 6 à 10 PO) : même avec `U` corrigé, un relevé à 1 PO serait hors zone.
  - Conséquence : **le Sylargh ne perd jamais son invulnérabilité → combat impossible à gagner** (sauf s'il ne reste que lui
    et qu'on ne peut rien faire).
- La mort en chaîne est déjà prévue : `FightActor.KillAllSummons()` tue aussi les `RevivedBy == this`.
- [DOUTE VERSION] Pacifiste : critère 2.51 `v90` = PV **> 90 %** (strict), le site dit « 90 % ou plus ». Garder 2.51.
- [DOUTE VERSION] tous les chiffres du site (700, 550, 650, 1 100…) sont ceux de la version actuelle ; 2.51 en base (ex. Clef
  battue 71-90, Poinçon 101-140) : à garder.

### À faire (précis, prêt à coder)
1. [CRITIQUE] `game/effects/handlers/Spells/Others/ReviveLastDeadAlly.cs`, dans `InternalApply()` après `mort.Revive(...)` :
   - `mort.SummoningEffect = this;`
   - placement : si `Spell.Id == 3607` (Nécromignon), choisir la case parmi les cases `Walkable` et `Fight.IsCellFree(c)` à
     **distance exactement 6** (`new MapPoint(c).ManhattanDistanceTo(Caster.Position.Point) == 6`), triées dans le sens horaire
     (angle `Math.Atan2(dy, dx)` autour du lanceur, en partant de la case « devant » / en haut ; le site ne précise pas la
     case n°1 exacte : approximation acceptable) ; prendre la 1re. À défaut, garder l'ancien repli (case adjacente).
   - poser l'**état Zombi 74** permanent sur le relevé (même code que `ReviveActor.cs` lignes 71-79 : `AddState` 74,
     `StateBuff` `DISPELLABLE_BY_DEATH`, `Duration = -1000`), au moins quand `Spell.Id == 3607`.
2. [CRITIQUE] `game/effects/handlers/Spells/Targets/JustSummonedCriterion.cs` : remplacer `actor.IsSummoned() &&` par
   `(actor.IsSummoned() || actor.RevivedBy != null) &&` pour que le critère `U` reconnaisse un mort relevé par l'effet en cours
   (vérification `SummoningEffect == x` inchangée, donc sans effet de bord sur les autres sorts).
3. [CRITIQUE] Test complet à faire après 1-2 : tuer un monstre → au tour du monstre suivant, relevé à 6 PO avec 10 % PV,
   état Zombi, Indéplaçable, passe son tour ; au début de son tour suivant (Zombigre → Nécromignon) il reçoit Zombidule ; le
   tuer avec le Sylargh entre 5 et 10 PO (`C10,5`) → Sylargh vulnérable 1 tour (952 sur l'état 56). Si Zombidule doit marcher
   dès la 1re mort du Zombi (avant son 1er tour, comme le site le laisse entendre), poser aussi à la réinvocation, dans
   `ReviveLastDeadAlly` (cas 3607), un buff `X` qui lance 3608 : à valider en jeu avant de l'ajouter (doute sur l'ordre 2.51).

---
## Trône de la Cour Sombre (trone-de-la-cour-sombre) — boss Reine des Voleurs (id 3726)
### Mécanique officielle (site)
- Donjon de dimension (Srambad) : 1 salle, boss dès le départ + 5 vagues (invulnérables 1 tour à l'arrivée), modulaire.
- **Bourôliste (3749)** : Coupable (~1 000 eau, −2 tours d'effets, tue une invocation, mêlée, 2×/tour) ; Tournoyade (−10 PM
  esquivables + **Pesanteur** 2 tours ; si la cible est déplacée dans les 2 tours : 500 air, relance 3) ; Vers la Lumière (attire
  6, ligne 2-7 PO).
- **Doublure (3748)** : Doublage (en début de combat/vague : crée un double et devient invisible 1 tour ; double et Doublure
  partagent les dommages et gagnent +100 puissance à chaque coup ; le double meurt au bout de 3 tours en infligeant 600 terre
  cercle 2, relance 3) ; Coupe Circulaire (600 feu, vol 2 PM, −3 PA, cercle 2 ; le double la répète en fin de tour) ; Subtilité
  (échange avec le double à 6 PO : 500 neutre + pousse 2 autour de la Doublure, 500 neutre + attire 3 autour du double ; le double
  la relance en début de tour).
- **Mâchassin (3746)** : Piège à Le Ours (500 feu + pièges feu sur les 4 cases autour, 8 PO) ; Sans se Mouiller (~600 eau, −3 PA,
  ligne 4-10 PO).
- **Magouille (3750)** : Avertissement (envoûtement 1 tour sur elle et ses alliés à 3 PO : qui les frappe reçoit un poison 300
  eau 2 tours au début de son prochain tour, cumul 3, relance 3) ; Crâmes (650 neutre + piège en diagonale taille 2 : −10 PM et soin
  10 % autour, 4-8 PO) ; Crânéantissement (550 terre + soin 15 % de tout le monde en cercle 2, 3-7 PO).
- **Terristocrate (3747)** : Attentat (750 neutre cercle 4) ; Bombe Illicale (500 neutre vol de vie + invoque une **Bombe
  Illicale** indéplaçable au contact : elle **explose au prochain tour du Terristocrate ou à sa mort et tue les personnages et
  invocations à 2 PO** ; quand un allié frappe un monstre, la bombe se rapproche de lui de 3 cases, relance 3) ; Fumérus
  (téléport 4 PO en ligne + ~700 air vol de vie et −4 PO cercle 2, relance 2).
- **Reine des Voleurs (3726)** : **Péché Mignon** : chaque joueur invoque une **Bonbombe** à 2 PO au début de son tour (sens
  horaire si la case est prise) ; une Bonbombe explose (OS les joueurs en ligne à 10 PO, ~1 200 feu aux monstres) quand son
  joueur en a 2, en chaîne avec les bombes en ligne ; les bombes en diagonale deviennent **bleues (Désamorcée)** et soignent
  les joueurs au lieu de les tuer ; bombes invulnérables, Lourdes ; la Reine est immunisée aux bombes et subit −10 % de dégâts
  par Bonbombe présente ; à la mort de la Reine les joueurs subissent +50 % de dommages. Coup Critique (~900 eau + 20 PV par PM
  utilisé ; en CC −4 tours d'effets et ~1 500 multi-éléments, mêlée) ; Mort en Sursis (s'attire sur la cible puis échange ;
  état **Mort en sursis** : tue dans 6 tours sauf soin d'une Bonbombe bleue ou mort de la Reine ; + 20 % de ses PV érodés en
  air, ligne 5 PO) ; Brume (fin de tour : glyphe noir rayon 3, Reine invisible dedans sur une des 3 cases du bord, pas avant le
  tour 4, relance 3).

### État sur le serveur
- Spawn 391 + 120 lignes de vagues (`monsters_spawns_dungeons_waves`) ; invulnérabilité d'arrivée de vague gérée
  (`Fight.AddWaveInvulnerability`). Sorts en base = client 2.51, aucun effet / déclencheur non géré (1022 « rapprochement »
  désormais géré).
- `ReineDesVoleursBrain` pose **Péché Mignon 4693** (Bonbombes au TB des joueurs, +50 % à la mort via 4693 niv. 2) ;
  `BonbombeBrain` (3725) pose **Nova 4690** (grade selon le tour) : conforme.
- Mort en Sursis : `Kill` avec `delay 6` + état 197 6 tours ; Nova retire les effets (406) des joueurs en état 197 soignés :
  conforme. Brume : relance initiale 3 en base (= « pas avant le tour 4 ») : conforme.
- **Bombe Illicale (3745)** : 0 sort, **aucun cerveau**. Son passif 2.51 **Rapprochement dangereux 4773** existe dans le client
  (non attribué) : état Invulnérable, `Kill` de soi-même avec `delay 1`, et buff `X` (à la mort) qui lance **Décrochage de
  cordon 4772** (tue les ennemis — joueurs et invocations — en cercle 2). Sans lui la bombe (1 PV) ne fait rien : ni explosion,
  ni invulnérabilité (elle meurt au premier coup). Le rapprochement (4770, `1022` vers l'attaquant) est porté par le sort du
  Terristocrate : OK.
- Le délai d'un buff lancé par une invocation est décompté au début du tour de son invocateur
  (`Fight.cs` ~l.1957 `DecrementSummonsCastedBuffsDuration`) → `delay 1` = explosion au prochain tour du Terristocrate,
  conforme au site ; à sa mort, `KillAllSummons()` tue la bombe → `X` → explosion : conforme.
- [DOUTE VERSION] rapprochement de 2 cases en 2.51 (site : 3) ; dégâts de Nova en base `1/0` feu (site ~1 200) : garder 2.51.
- [DOUTE VERSION] composition exacte des 5 vagues non comparée au site (le site ne donne que des grandes lignes).

### À faire (précis, prêt à coder)
1. [IMPORTANT] Bombe Illicale : dans `PassifsDonjonsBrains.cs`, ajouter
   `[BrainIdentifier((int)MonsterIdEnum.BOMBE_ILLICALE)] public class BombeIllicaleBrain : BrainPassif { public BombeIllicaleBrain(AIFighter f) : base(f, 4773) { } public override void Play() { } }`
   (monstre `canPlay = false` côté client : `Play()` vide par sécurité). Test : la bombe est invulnérable, se rapproche de
   2 cases du joueur qui frappe un monstre, explose au début du tour suivant du Terristocrate (ou à sa mort) et tue les joueurs /
   invocations à 2 PO.

---

## Ventre de la Baleine (ventre-de-la-baleine) — boss Protozorreur (id 3828)
### Mécanique officielle (site)
- **Dislocation (zone)** : tous les Krobes sont **invulnérables** tout le combat. Le 1er monstre en initiative invoque une
  **Malamibe** qui **intercepte les dégâts subis par tous les Krobes** (à 200 %) ; elle a **2 000 PV par personnage vivant**
  (perd 2 000 PV à chaque mort de personnage), **réduit de 50 % les dégâts reçus directement** ; morte, elle est réinvoquée par
  le prochain monstre qui joue ; **si elle survit 2 tours complets, elle tue tous les joueurs** (combat perdu).
- **Restauration** : un Krobe meurt quand sa condition est remplie 3 tours de suite (état **Infection** 1 tour à chaque
  réussite, compteur remis à 0 sinon) :
  - **Bacterrib (3825)** — Restauration éloignée : aucun adversaire à 5 PO ou moins au début de son tour ; retire 1 tour
    d'envoûtement aux personnages à plus de 5 PO à chaque tour. Sécrétion (500 air, −3 PO −3 PM, 6-10 PO) ; Danse Répulsive
    (600 terre, pousse 3, ligne 6-10 PO sans LdV).
  - **Pataugerme (3827)** — Restauration élémentaire : frappé dans les 4 éléments chaque tour. Brûlure d'estomac (225 neutre
    cercle 2 + 225 par élément reçu au tour précédent, 4 PO) ; Attaque souterraine (650 terre, −3 PM, 3 PO).
  - **Tabacille (3823)** — Restauration curative : aucun soin pendant 3 tours (vol de vie autorisé) ; sinon +150 puissance
    (infini). Échange subtil (échange + 250 à 650 neutre selon les PV de la cible, ligne 4 PO) ; Remontée gastrique (−1 tour
    d'envoûtement + 300 à 650 eau selon PV, diagonale 2).
  - **Verminocule (3824)** — Restauration de proximité : un personnage à son contact au début de son tour. Gonflement (attire 3
    + 550 eau, étoile 3) ; Jet d'acide (1 050 neutre, mêlée).
  - **Virustine (3826)** — Restauration mobile : ni déplacée ni retrait PM 3 tours ; perd 1 PM (infini) par dommage de poussée,
    rendus si elle est déplacée / subit une tentative de retrait PM. Contamination (700 feu, 20 % érosion, tue une invocation,
    mêlée) ; Boulet Viral (jusqu'à 1 400 air selon ses PM restants, ligne 3 PO sans LdV).
- **Protozorreur (3828)** : **Restauration infectieuse / Protomatisme** (+100 puissance par tour, remis à 0 en Infection) ;
  invulnérable, **Indéplaçable** ; attiré au contact de qui le frappe en ligne (1re attaque du tour seulement) ;
  **Protoglyphe infectieux** : glyphe violet sous tout personnage qui subit des dégâts (550 air au passage ; met le
  Protozorreur en **Infection 3 tours**, +2 PM) ; il meurt après **6 tours consécutifs en Infection** (fin de son 6e tour).
  Jet Proto (500 air, +5 par lancer hors Infection / attire au contact en Infection, ligne 10 PO) ; Électrocution (1 000 feu,
  −6 PM hors Infection / −6 PA en Infection, 2 PO sans LdV) ; Infection (tue les invocations au contact ; hors Infection :
  20 % érosion à tous + pousse 3 en cercle 3 ; en Infection : eau = 50 % des PV érodés à tous + attire 3 en cercle 4 ; dès le
  tour 3, relance 3).

### État sur le serveur
- Spawns 392-396 conformes. Sorts en base = client 2.51 ; aucun effet / déclencheur non géré (1019, 1165 désormais gérés).
- Cerveaux existants (`DimensionsBrains.cs`) : Tabacille 4936, Verminocule 4941, Bacterrib 4946, Virustine 4951,
  Pataugerme 4956, Protozorreur 4972 → invulnérabilité, compteurs de Restauration, Dislocation (buff `D` = 4935 renvoi de 200 %
  des dégâts à l'invocation alliée en état Disloqué ; buff `TB` = 4934/4964 invoque la Malamibe si aucune), Protoglyphe,
  Protomatisme, Déphasage : conformes aux données 2.51.
- **Malamibe (3822)** : 0 sort, **aucun cerveau**, 1 PV en base et dans le client. Son passif 2.51 **Décellularisation 4933**
  existe dans le client (non attribué ; trouvé via l'état 214 « Disloqué ») :
  - 792 → chaque joueur (`L`) lance 4932 : **+2 000 vitalité** à l'invocation ennemie (la Malamibe), durée infinie (perdue à la
    mort du joueur, buffs du mort retirés par `RemoveAllCastedBuffs(DISPELLABLE_BY_DEATH)`) ;
  - 793 → 4931 `Kill` de tous les ennemis avec **`delay 2`** (= l'OS après 2 tours) ;
  - 1163 ×50 % sur les dégâts reçus (`D`) ;
  - état **Disloqué 214** sur tous ses alliés sauf elle (`g`) → c'est cet état qui active l'interception (4935 vise `j,*E214`)
    et qui empêche une 2e invocation (4964 exige `*e214`).
  - Conséquence aujourd'hui : la Malamibe a 1 PV, aucun Krobe n'est Disloqué → **l'interception 4935 ne trouve jamais de
    cible, la Malamibe ne tue jamais l'équipe, et un monstre réinvoque une Malamibe à chaque début de tour** (aucun n'est
    Disloqué) : la mécanique centrale du donjon est cassée.
- [DOUTE VERSION] tous les chiffres du site (500, 650, 1 050…) vs 2.51 en base ; le site parle d'un compteur « 5 tours » dans
  le tableau puis « 3 tours » : on garde les délais du client.

### À faire (précis, prêt à coder)
1. [CRITIQUE] Malamibe : dans `DimensionsBrains.cs`, ajouter
   `[BrainIdentifier((int)MonsterIdEnum.MALAMIBE)] public class MalamibeBrain : BrainPassif { public MalamibeBrain(AIFighter f) : base(f, 4933) { } public override void Play() { } }`
   (invoquée en cours de combat → `SortPassif.Poser` à l'arrivée ; 0 PA / 0 PM : ne joue pas). Points à vérifier en test :
   - la vitalité : elle doit avoir ~2 000 × nb de joueurs PV ; si `AddVitality` n'augmente que les PV max sans les PV courants,
     les PV resteront à 1 : il faudrait alors lui rendre tous ses PV juste après la pose (à signaler, ne pas deviner) ;
   - l'OS : le délai 2 du `Kill` (buff lancé par la Malamibe, invocation) est décompté au début du tour de son invocateur
     (le Krobe) ; vérifier que 2 tours complets après son apparition l'équipe meurt ;
   - à la mort de la Malamibe, l'état Disloqué des Krobes doit disparaître (sinon plus de réinvocation) : posé avec
     `dispellable 2` (= par la mort) → retiré par `RemoveAllCastedBuffs(DISPELLABLE_BY_DEATH)` dans `Fight.cs` ~l.2307 ;
   - mort d'un joueur → −2 000 PV (buff 4932 du joueur retiré).

---
## Volière de la Haute Truche (voliegravere-de-la-haute-truche) — boss Haute Truche (id 3618)
### Mécanique officielle (site)
- Page « à réviser » : sorts présentés uniquement en images (rien d'exploitable). Seule règle écrite : **frapper la Haute
  Truche avec une arme (« CaC ») la rend invulnérable 2 tours**.
- Monstres : Gruche (3619), Truchmuche (3620), Truchideur (3621), Truchtine (3622), Truchon (3623).

### État sur le serveur
- Spawns 240-244 conformes. Sorts en base = client 2.51 ; aucun effet / déclencheur non géré (1069 désormais géré dans
  `DamagePercentTargetLife.cs`).
- Cerveaux existants (`PassifsDonjonsBrains.cs`) : Haute Truche **Struchthio 4367**, Gruche 4376, Truchmuche 4388,
  Truchideur 4394, Truchtine 4399, Truchon 4404 : tous les passifs 2.51 sont posés.
- Struchthio : buff `DCAC` → **Truchage 4344** (Invulnérable 2 tours + 200 puissance et −20 % rés. aux alliés). Sur le serveur,
  `DCAC` est mappé sur `OnDamagedInCloseRange` (`TriggerBuff.cs` l.71, ajout MYSTIC « DCAC = frappe au corps à corps ») :
  **n'importe quel sort lancé au contact** déclenche l'invulnérabilité, alors que le site parle d'**arme**. `DM` existe déjà
  pour « subit des dégâts en mêlée », ce qui laisse penser que `DCAC` = « frappé à l'arme » (`DC` = `OnDamagedByWeapon`).
  Même chose pour Sdeurthio 4394 (Truchideur : +1 PA aux alliés quand il est frappé en `DCAC`).
- **Attention** : `DCAC` est utilisé par ~50 sorts du client, dont des sorts du Glourséleste (`Pelage hivernal` 3688…) que
  l'utilisateur juge parfait → **ne pas changer le mappage global**.
- [DOUTE VERSION] déclencheur `P` des « +100 puissance » des 6 passifs Truche : mappé `OnPushed` (`TriggerBuff.cs` l.192) ; sens
  officiel incertain (permanent ? poussé ?). Le site n'en dit rien : laisser tel quel, à observer en jeu.

### À faire (précis, prêt à coder)
1. [IMPORTANT] (à valider par l'utilisateur, doute sur le sens exact de `DCAC` en 2.51) Truchage uniquement sur coup d'arme, sans
   toucher au mappage global : dans `game/effects/handlers/Spells/Others/CastSpell.cs`, au début de `DefaultBuffTrigger`, ajouter
   `if ((buff.Spell.Id == 4344 || (buff.Spell.Id == 4375 && Spell.Id == 4394)) && !(token is Fights.Damage dmg && dmg.IsWeaponAttack)) return;`
   (4344 = Truchage de Struchthio, 4375 lancé par Sdeurthio 4394). Limite : une arme à distance (arc) ne déclenchera pas
   (le déclencheur reste « mêlée ») ; acceptable, sinon il faudrait un nouveau déclencheur dédié.

---

## Miausolée du Pounicheur (miausoleacutee-du-pounicheur) — boss Pounicheur (id 4278)
### Mécanique officielle (site)
- Dimension Ecaflipus : 1 salle, boss + 5 vagues, modulaire.
- **Gériatique (4294)** : Nuée de tiques (~130 eau cercle 2, vol 2 PO) ; Ponction revitalisante (~100 air vol de vie en ligne,
  sans LdV, PO mini) ; Parasitage (−10 % rés. 1 tour, sans LdV, PO infinie, cumul jusqu'à −100 %) ; IA fuyarde.
- **Grath (4295)** : Secouage (~150 feu, pousse 4, mêlée) ; Démangeaisons (~150 terre, −50 % dommages finaux 1 tour, mêlée) ;
  Saut de puce (téléport 4 PO + −2 PM autour de l'arrivée).
- **Morcac (4292)** : **Morzerk (état infini : +50 puissance par coup reçu)** ; Roupetkifouette (100-150 terre, 15 % érosion,
  3 PO) ; Invasion parasitaire (100-150 neutre en ligne + échange de place, ~15 PO).
- **Pikbul (4293)** : Déjection empoisonnée (poison feu 10 PV par PA, 1 tour) ; Infestation (~200 terre, diagonale, s'attire vers
  la cible) ; Ectoparasite (−200 fuite, Pesanteur, Indéplaçable, mêlée ; frappé au contact, les dégâts vont au joueur marqué).
- **Pupuce (4291)** : **Pupulsion (état infini : qui la frappe gagne +100 % critique 1 tour)** ; Pupussucion (~100 air, vol 2 PM,
  ligne 5 PO) ; Pupunition (~150 neutre, mêlée).
- **Pounicheur (4278)** : invoque au départ **3 puces invulnérables (Tipoune, Tipoude, Tipoutre)** aux états **I / II / III** ;
  à chacun de ses tours il change leurs états ; frapper une puce au contact incrémente son état (et décrémente les 2 autres) et
  repousse de 2 ; si les 3 puces ont le même état : **Pourrification** (−1 tour d'effets à tous les alliés des puces / ennemis) ;
  **sinon le Pounicheur renvoie 200 % des dégâts subis à toute l'équipe** (pas les poisons, pas un coup fatal). Poulvérisation
  (neutre au contact, pousse 4).

### État sur le serveur
- Spawn 207 (+ vagues). Sorts en base = client 2.51, aucun effet / déclencheur non géré.
- `PounicheurBrain` lance **Pougéniture 6278** (puces 4279-4281 invulnérables, mutations, Kissifrotsipik 6314 = renvoi 200 %
  si pas l'état Poutrage 340, Pourrification 6465 sur l'état 339) : conforme. `MorcacBrain` lance **Morzerk 6346** à
  l'apparition (`GetAlive` aussi appelé pour les vagues, `Fight.cs` l.1853) : conforme.
- **Pupuce (4291)** : **aucun cerveau** ; son passif 2.51 **Pupulsion 6336** existe dans le client (non attribué) : buff `D` →
  8684 (+100 % critique 1 tour **et ×200 % dégâts subis** à l'attaquant `A,O`). Jamais posé aujourd'hui.
- [DOUTE VERSION] Pupulsion 2.51 contient aussi un multiplicateur ×200 % (1163) que le site ne cite pas : on suit 2.51.
- [DOUTE VERSION] chiffres (~130, ~150, ~200) : valeurs 2.51 en base. IA fuyarde du Gériatique : pas une donnée 2.51.

### À faire (précis, prêt à coder)
1. [IMPORTANT] Pupuce : dans `PassifsDonjonsBrains.cs`, ajouter
   `[BrainIdentifier(4291)] public class PupuceBrain : BrainPassif { public PupuceBrain(AIFighter f) : base(f, 6336) { } }`
   (pas de nom dans `MonsterIdEnum` vérifié pour 4291 : utiliser l'id numérique ; arrive en vague → `SortPassif.Poser` via
   `FighterAdded`, comme Lucrane).

---
## Pitons Rocheux des Craqueleurs (pitons-rocheux-des-craqueleurs) — boss Craqueleur Légendaire (id 669)
### Mécanique officielle (site)
- Partie « donjon » (ancienne fiche) : Craqueleur (106) : Écrasement Handicapant (60 terre, −2 PA), Jet de pierre (30-40 feu,
  −4 à −6 PM, 10 PO) ; Craqueboule (483) : Martèlement (60 neutre au contact, +6 dommages 5 tours), Motivation Tellurique (+2 PM
  puis +4 PA au tour suivant) ; Craqueleur des Plaines (293) : Frappe Sismique (−1 PA 4 tours en rayon 5), Écrasement,
  Jet de pierre ; d'autres Craqueleurs cités (Caillassage, Percussion, **Plainitude** : +2 PM déclenché, Sismologie,
  Terpanation, Terraportation).
- **Craqueleur Légendaire (669)** : Peau de Granite (zone cercle 2, effet « ??? »), Pierre Étourdissante (50 feu, −100 PA 1 tour),
  Peau de Silex (+70 % rés. à lui et aux monstres à 2 PO), Invocation Montagnarde (invoque un monstre du donjon, tour 1 puis tous
  les 3 tours), Peau de Topaze (−1 000 dommages subis 3 tours sur un allié), Cœur de Craqueleur (+1 PA), Frappe du Craqueleur
  Légendaire (100 feu cercle 2, alliés et ennemis).
- Salle 5 : pont invisible (marcher sur la ligne du milieu, case par case).
- **Expédition de guilde** (Couche Rocailleuse, glyphes noirs, Craqueboulisation, Choc tellurique, bouclier 3 000 sur coup
  neutre, Pierre Étourdissante à 20 PA…) et la fiche détaillée par grade (17 000 PV…) : contenu de la version actuelle.

### État sur le serveur
- Spawns 105-109 : Craqueleur 106, Craqueboule 483, Craqueleur des Plaines 293, Craqueleur Légendaire 669 : conformes.
- Craqueleur Légendaire : 7 sorts 2.51 (Invocation Montagnarde 483/106/293, Peau de Topaze, Pierre Étourdissante, Peau de
  Granite, Peau de Silex, Cœur de Craqueleur, Frappe) = la liste du site. Aucun passif, pas de cerveau nécessaire : conforme.
- **Craqueboule (483) — Manteau Tellurique 7120** :
  - **effet 1128 non géré** (outil : « A FAIRE : effet 1128 »). Description client vide, catégorie dégâts, élément terre :
    c'est le renvoi « X % des dommages subis en dégâts terre » autour du Craqueboule (zone `X1` = croix de 1) ; niv. 2 = 30 %,
    niv. 3 = 45 % ;
  - **niveau de sort faux en base** : le niv. 1 de 7120 est le vrai sort (buff `D` 2 tours → lance 7120 niv. 2) ; les niv. 2 et 3
    sont des sous-sorts. `monsters_spells` donne le **niv. 3 aux grades 3-5** (ids 5311, 5312, 5313) → le Craqueboule lance
    directement le sous-sort (renvoi sans dégât déclencheur = rien). Même erreur pour Craquetuss 2326 (14530-14532), Craqueboule
    invoqué 2789 (19220-19222), Shaman craqueleur 5207 (48864-48866).
- **Plainitude 7124** existe dans le client 2.51 (buff `TB` → 7124 niv. 2) mais **n'est attribué à aucun monstre** dans
  `Monsters.d2o` 2.51 (ni au Craqueleur des Plaines) → [DOUTE VERSION], ne pas l'ajouter sans accord.
- [DOUTE VERSION] expédition de guilde (Couche Rocailleuse / Craqueboulisation absents du client 2.51) : hors périmètre.
- [DOUTE VERSION] chiffres (Peau de Silex 50 % en 2.51 vs 70 % site, Frappe du Légendaire 26-35, etc.).

### À faire (précis, prêt à coder)
1. [IMPORTANT] Effet 1128 (renvoi terre) : gestionnaire commun 1126/1127/1128 décrit dans la section Tour de Solar, point 4
   (copie de `DispatchDamages.cs` : buff `AfterDamaged` sur le lanceur, % = `Dice.DiceNum`, cibles = zone `X1` du sous-sort,
   `ReflectedDamages = true`, `School = EffectSchoolEnum.Earth` pour 1128).
2. [IMPORTANT] Base : `UPDATE monsters_spells SET Level=1 WHERE SpellId=7120 AND Id IN (5311,5312,5313,14530,14531,14532,19220,19221,19222,48864,48865,48866);`
   (le niv. 1 déclenche le niv. 2 à 30 % ; le 45 % du niv. 3 n'est atteignable par aucune donnée 2.51 du sort : [DOUTE VERSION]
   sur l'intention de grade). Prévoir le script d'annulation (`SET Level=3` sur les mêmes ids).

---
## Plateau de Ush (plateau-de-ush) — boss Ush Galesh (ids 4264 rouge / 4265 noir)
### Mécanique officielle (site)
- Dimension Ecaflipus : boss + 5 vagues, modulaire.
- **Matoumorphose** (tous les Chachas) : chaque monstre, à chaque tour, donne l'état **Matoumorphose** (infini) au monstre le plus
  proche (à lui-même s'il est seul) ; transmis au plus proche à la mort ; débloque une variante de sort.
- **Chacrebleu (4297)** : Fortifichation (230 feu sur les cases adjacentes + bouclier 380 ; en Matoumorphose vole aussi 2 PM) ;
  Retour de Chachaton (300 feu + état **C'est toi le Chacha !** 1 tour : renvoie les dégâts subis au contact) ; Chamboulement
  (avance jusqu'à la cible, −20 fuite, 200 terre, ligne 3 PO).
- **Chakaroze (4300)** : Griffure Ondulante (350 terre ligne 6 PO + 1-5 terre infini, retiré par un soin) ; Chatomisation (200
  terre cercle 2, −40 tacle) ; Miaoulement (260 eau, ligne 8 PO sans LdV, −1 tour d'effets).
- **Chakichan (4298)** : **Chakcélération** (+1 PM par coup reçu, infini) ; Mawatougeri (poison feu 15 PV/PM, 7 PO sans LdV) ;
  Griffe à un Pouce (300 neutre selon vitalité ; en Matoumorphose tue la cible au contact, 2×/tour).
- **Chargus (4299)** : Duplichation (double à 2 PO, meurt après 2 tours, relance 4) ; Vieillissement Prématuré (320 air en
  diagonale 3 PO, vol 200 puissance ; en Matoumorphose +400 puissance et Pacifiste à la cible) ; Queue de Poing (~500 eau,
  pousse 3).
- **Chasquatch (4296)** : **Ailurophilie** (boost infini : frappé, soigne ~400 les personnages à 5 PO, 1er coup de chaque
  joueur) ; Câlin Félin (350 eau croix 2, ligne 4-10 PO) ; Léchouille (280 feu 6 PO ; le prochain 1er coup sur lui soigne
  l'attaquant de 400).
- **Ush Galesh rouge / noir** : partagent leurs dommages ; **Rouge et Noir** (Intaclable, Indéplaçable, états Rouge / Noir) ;
  **Négativisme** (transfère l'état à l'autre) ; il faut alterner : Ush **Rouge** = frapper au contact, Ush **Noir** = frapper
  à distance (chaque coup inverse l'état). Éclair Rouge (300 feu ligne 12 PO, −15 % rés.) ; Frappe Noire (250 terre, −3 PM,
  mêlée) ; Pulsation Malsaine (rouge : 300 feu au contact + pousse 3 ; noir : 300 neutre + attire en croix ≥ 3 PO).

### État sur le serveur
- Spawn 294 avec les **deux** Ush (4264 et 4265) + les 5 Chachas (l'ancien audit disait « Ush noir absent » : c'est corrigé).
- Sorts en base = client 2.51 ; aucun effet / déclencheur non géré.
- `UshRougeBrain` (4264) pose **Rouge et Noir 6264** (partage 6239, Négativisme 6238, états, mort de l'un → mort de l'autre
  via 6568) et **Matoumorphose 6604** : conforme. Ailurophilie 6390 et Chakcélération 6401 sont des sorts lancés (relance 63)
  attribués en base, comme dans le client 2.51 : conforme.
- [DOUTE VERSION] chiffres (300, 250, 500…), « Pulsation Malsaine » unique en 2.51 (attire en croix 3) sans variante rouge.

### À faire (précis, prêt à coder)
Rien à faire.

---

## Défi du Chalœil (defi-du-chaloeil) — boss Chalœil (id 4263)
### Mécanique officielle (site)
- Dimension Ecaflipus : boss + 5 vagues (forcément aux tours 1/6/11/16/21), modulaire.
- **La Routourne** (tous les monstres du « Grand Jeu », dès le début) : bonus d'1 tour qui tourne sur un cycle de 3 tours,
  propre à chaque monstre : **Cavalier Chanceux** 2 PM → 100 % CC → 1 000 dommages de poussée ; **Dame du Hasard** 4 PA → 100 %
  CC → 200 dommages ; **Roi Joueur** 2 000 PB → 400 puissance → 100 % CC ; **Soldat de Fortune** 100 % CC → 100 fuite → 5 PO ;
  **Valet Veinard** 100 esquive PM → dommages subis ×80 % → 100 % CC.
- Monstres : Cavalier (4284) Carreausillon (300 neutre, pousse 6 puis attire 6, ligne 4 PO), Carreautage (300 feu, pousse 4 et
  s'attire, Pesanteur) ; Dame (4286) Crève-cœur (500 terre ; frappé en mêlée par un monstre → soin de la Dame), Tourne-Griffe
  (500 eau ; frappé → soin des monstres à 4 PO, dégâts répartis sur les alliés à 2-4 PO) ; Roi (4287) Adoubement brutal / Lueur
  royale (500 + poison 500) ; Soldat (4283) En plein cœur (300 eau + Pacifiste, retiré par un soin), Jet de Pique (250 feu, −3 PA) ;
  Valet (4285) Épique (450 terre, attire, Intacleur), Piquepocket (téléport + 280 air cercle 2, vol 3 PM).
- **Chalœil (4263)** : **Jeu du Chalœil** : **invulnérable 20 tours** (vulnérable au tour 21 : +200 esquive PM, Inébranlable) ;
  cases de la carte numérotées 1-6 (état « Dé n » selon la case de fin de tour de chaque entité) ; en fin de tour il invoque des
  **Dés** (2 000 PV, statiques) à distance paire et des **glyphes blancs** (soin total) à distance impaire sur les cases de son
  chiffre ; dès le tour 2 chaque monstre (sauf lui) inflige 20 % d'érosion + 450 dans un élément au hasard aux joueurs du même
  état de Dé ; dès le tour 2, −50 tacle 5 tours en cercle 2 autour de lui. Farce et Attrape (échange + 1 000 terre + Pesanteur,
  6 PO sans LdV, dès le tour 2) ; Toilette Ecaflip (1 000 eau, pousse 3, −4 PM, ligne 2 PO) ; Gros Yeux (tous les 3 tours : bonus
  selon l'état de Dé : 200 esquive PM / 5 PO / Indéplaçable / 3 PM / soin 20 % / 600 puissance).

### État sur le serveur
- Spawn 398 (Chalœil + 5 monstres du Grand Jeu, + vagues). Sorts en base = client 2.51, zones « cases fixes » `;NNN` désormais
  lues (Gros Yeux). Aucun effet / déclencheur non géré.
- `ChaloeilBrain` (DimensionsBrains.cs) pose **Jeu du Chalœil 6231** (invulnérable 20 tours, Inébranlable + 200 esquive PM,
  Dés, glyphes) : conforme.
- **La Routourne est mal posée** : le même cerveau pose **6301 à un niveau tiré au hasard (1-5) sur le Chalœil**. Or les 5
  niveaux de 6301 sont les 5 variantes **par monstre** (bonus à `delay` 0 / 1 / 2, relance du cycle à `delay 3`), vérifié dans
  le client 2.51 :
  - niv. 1 = 100 % CC / 100 fuite / 5 PO → **Soldat de Fortune 4283** ;
  - niv. 2 = 100 esquive PM / dommages subis ×80 % / 100 % CC → **Valet Veinard 4285** ;
  - niv. 3 = 2 PM / 100 % CC / 1 000 dommages de poussée → **Cavalier Chanceux 4284** ;
  - niv. 4 = 4 PA / 100 % CC / 200 dommages → **Dame du Hasard 4286** ;
  - niv. 5 = 2 000 bouclier / 400 puissance / 100 % CC → **Roi Joueur 4287**.
  Chaque niveau lance aussi 6301 niv. 6 sur le Chalœil (`1160` cible `a,F4263`) → buff « Jeu du Chalœil » 6234/3 (malus de
  l'état de Dé). Aujourd'hui : le Chalœil (déjà invulnérable) reçoit les bonus d'un seul monstre, **aucun monstre n'a son cycle**
  ni le malus de Dé qui en découle.
- Spawn 450 (« Reto de Miauvizor », carte 159383552 : Miki, Manifestations, Écureuil Fourbe, Chalœil grade 2) : combat de quête,
  hors périmètre.
- [DOUTE VERSION] chiffres (Toilette 101-120 en 2.51 vs 1 000, malus de Dé 50 par élément en 2.51 vs 450, etc.).

### À faire (précis, prêt à coder)
1. [IMPORTANT] `DimensionsBrains.cs`, `ChaloeilBrain` : supprimer le tirage `m_hasard` et la ligne
   `SortPassif.Poser(fighter, 6301, variante);` (ne garder que `SortPassif.Poser(fighter, 6231, 1);`).
2. [IMPORTANT] Ajouter un cerveau par monstre du Grand Jeu (ils arrivent en vagues → `SortPassif.Poser` gère l'arrivée) :
   ```
   public abstract class RoutourneBrain : Brain { protected RoutourneBrain(AIFighter f, int niveau) : base(f) { SortPassif.Poser(f, 6301, niveau); } }
   [BrainIdentifier(4283)] public class SoldatDeFortuneBrain : RoutourneBrain { public SoldatDeFortuneBrain(AIFighter f) : base(f, 1) { } }
   [BrainIdentifier(4285)] public class ValetVeinardBrain : RoutourneBrain { public ValetVeinardBrain(AIFighter f) : base(f, 2) { } }
   [BrainIdentifier(4284)] public class CavalierChanceuxBrain : RoutourneBrain { public CavalierChanceuxBrain(AIFighter f) : base(f, 3) { } }
   [BrainIdentifier(4286)] public class DameDuHasardBrain : RoutourneBrain { public DameDuHasardBrain(AIFighter f) : base(f, 4) { } }
   [BrainIdentifier(4287)] public class RoiJoueurBrain : RoutourneBrain { public RoiJoueurBrain(AIFighter f) : base(f, 5) { } }
   ```
   Test : chaque monstre affiche son bonus du tour (1er bonus au tour d'arrivée, 2e au suivant, 3e ensuite, puis on recommence).

---

## Temple de Koutoulou (temple-de-koutoulou) — boss Larve de Koutoulou (id 4453)
### Mécanique officielle (site)
- **États FOLIE 1 à 10** (tout le donjon) : Folie 2 = vos sorts touchent aussi vos alliés à 2 PO ; Folie 4 = soins reçus ×50 % ;
  Folie 6 = dommages subis ×100 % (+) ; **Folie 10 = mort**. +1 Folie : Folle Cacophonie, Sphère Corrosive, Ponction Morbide,
  attiré par Enfouissement, votre double qui joue, Frappe Koutonienne, fin de tour en LdV du Koutoulou, 2 échanges avec la même
  entité dans le tour ; −1 Folie au début de son tour (et quand un allié vous frappe, depuis la 3.7).
- **Grofond (4448)** : Attraction Répugnante (attire 5 + 500 eau, ligne 6 PO) ; Sphère Corrosive (450 terre cercle 2 + Pesanteur,
  +1 Folie). **Klûtiste (4452)** : Folle Cacophonie (700 feu demi-cercle 2, pousse 3, +1 Folie) ; Intensité Démoniaque (+2 PM,
  +200 puissance, +25 % vita aux alliés à 3 PO) ; Mélopée Pernicieuse (−4 PM + 500 eau, 5-12 PO). **Li-Fo (4451)** : Duplication
  Grotesque (double un monstre à 3 PO, pas au tour 1, relance 3) ; Ponction Morbide (vol 1 000 vita 2 tours + Insoignable,
  +1 Folie). **N'yalg (4449)** : Enfouissement (Indéplaçable + attire ceux qui passent en ligne à 4 PO) ; Expulsion Miasmatique
  (600 eau + invoque un **Miasme Polarisateur** au contact avec glyphe rayon 2 : −25 % dommages finaux, ~40 en fin de tour) ;
  Flagellation Paralysante (400 air, −4 PM). **Shokkoth (4450)** : Triangle Dément / Œil Horrifiant (dédoublent les joueurs ;
  le double avance, vous subissez ses dégâts).
- **Larve de Koutoulou (4453)** : Frappe Koutonienne (1 300 terre, mêlée, +1 Folie) ; Permutation Inquiétante (échange avec un
  allié, 6 PO sans LdV, dès le tour 2) ; **Vacillement de l'Univers** (dès le tour 2, au début de son tour, téléporte à son
  contact les joueurs dans sa LdV ; Pesanteur / Indéplaçable / Enraciné protègent) ; **Horreur Cosmique** (chaque ligne de
  dommages directs → l'attaquant échange avec l'entité la plus proche, état **Halluciné** ; 2 échanges avec le même ennemi →
  il devient invulnérable jusqu'au tour suivant de l'attaquant et +1 Folie ; limité à 10 fois par tour).

### État sur le serveur
- Spawns 404-408 conformes. Sorts en base = client 2.51 ; aucun effet / déclencheur non géré (1019, 1189 désormais gérés).
- `KoutoulouBrain` pose **Horreur Cosmique 6749** (TB Vacillement 6752, `1019` échange sur coup ennemi 6750/2, Vision
  Cauchemardesque 6864 en fin de tour, FOLIE 6724, invocation de la Larve Cauchemardesque 4528 en case 371) et **Protection
  Koutoulouesque 6820** : conforme. La FOLIE (6723) est portée par les sorts des monstres : conforme.
- [DOUTE VERSION] Miasme Polarisateur (4454) : en 2.51 il n'a **aucun sort ni passif** (pas de glyphe −25 %) → mécanique
  postérieure, ne pas l'ajouter. Retrait de Folie en frappant un allié : « depuis la 3.7 » (le site le dit) → hors 2.51.
- [DOUTE VERSION] chiffres (1 300, 700…).

### À faire (précis, prêt à coder)
Rien à faire.

---

## Vaisseau du Capitaine Meno (vaisseau-du-capitaine-meno) — boss Capitaine Meno (id 4460)
### Mécanique officielle (site)
- **Mutation Instable** (tous les monstres de la zone, à chaque début de tour) : état selon les PV : 75-51 % **Mutation I**,
  50-26 % **Mutation II** (+1 PM), 25-0 % **Mutation III** ; chaque monstre a un sort qui n'est lançable qu'en Mutation
  (Crabe : Yolosouage ; Gambaf : Enchaînement de Coups de Poing Normaux ; Mantaze : Foudre Marine ; Mol Usk : Roulemboule ;
  Tilamproie : Morsure Filtrante), effets croissants I → III.
- Monstres : **Crabe Yoloniste (4456)** Gesticulation Ridicule (400 eau carré 1 + invoque l'**Étoile Swagante** 1 000 PV qui
  renvoie 200 %, une seule à la fois, dès le tour 2), Pince Écrasante (1 100 neutre) ; **Gambaf (4457)** Pas Chassé Frontal
  (avance, 650 terre, pousse 4 ; si dommages de poussée : +400 puissance et ramène la cible), Point-météore ; **Mantaze (4458)**
  Éclair Obscur (250 feu, +15 % dégâts subis, état **Conducteur** 3 tours retiré par un soin), Électrochoc (10 % des PV érodés) ;
  **Mol Usk (4455)** Bave Acide (350 air, −2 PA), Traînée Collante (550 feu + glyphe −100 PM −50 % vita) ; **Tilamproie (4459)**
  Piège Parasite (piège diagonale, le Tilamproie se téléporte au contact), Succion Attractive (550 eau, attire 4).
- **Capitaine Meno (4460)** : pas d'invulnérabilité ; **dommages reçus ×50 %** (sauf poisons) ; les joueurs gagnent **+20 %
  dommages finaux par tour** (infini, cumulable) ; **chaque dégât des monstres / du Meno le soigne** ; à partir du tour 2 il
  invoque 2 monstres par tour (3 en butin 5+) selon un cycle fixe (runes) ; il **ajoute à la Mutation Instable de tous les
  monstres du combat** : **invulnérables**, **−25 % PV max par tour (morts au bout de 4 tours)**, −1 PM. Matière Volatile (500
  air, 4 PO sans LdV) ; Crystalisation (500 eau + glyphe infini « Crystalisation » ; au début de son tour, glyphes occupés →
  1 300 feu aux joueurs alignés à 5 PO, dévoile les invisibles, bouclier 1 000 au joueur sur un glyphe).

### État sur le serveur
- Spawns 399-403 conformes. Sorts en base = client 2.51 (sorts « Mutation » avec états requis) ; aucun effet / déclencheur non
  géré (2019, 2022, 1048 gérés).
- `CapitaineMenoBrain` (DonjonsRecentsBrains.cs) pose **Appel des Fonds Marins 6793** au grade du monstre (runes, ×50 %, soin
  sur dégâts, +20 % dommages finaux, Crystalisation 8812) : conforme. **Mais il pose aussi Mutation Instable 6758 sur le Meno**,
  ce qui est faux :
  - 6758 = passif des **monstres** de la zone (non attribué dans `Monsters.d2o` 2.51, comme tout passif) ; son 1er effet
    `1160 → 6759/2` vise `a,F4460` (le Meno) mais c'est **le lanceur** qui reçoit les effets `C` de 6759/2 : **Invulnérable
    infini** + 6759/3 (−1 PM, −25 / −33 / −50 % vita aux `delay` 1-3, **Kill de soi-même au `delay 4`**) ;
  - lancé par le Meno lui-même → **le Meno devient invulnérable en permanence puis se tue au bout de 4 tours** ; à l'inverse,
    **aucun monstre** n'a la Mutation Instable : ni états Mutation I/II/III (leurs sorts « Mutation » sont inutilisables), ni
    invulnérabilité / mort en 4 tours dans la salle du boss.
- [DOUTE VERSION] Étoile Swagante (4461) : aucun sort ni passif en 2.51 (pas de renvoi 200 %) → ne pas ajouter. Runes « depuis
  la 3.7 » : déjà portées par 6793 (2022) dans le client 2.51.
- [DOUTE VERSION] chiffres (1 100, 650, 1 300…).

### À faire (précis, prêt à coder)
1. [CRITIQUE] `DonjonsRecentsBrains.cs`, `CapitaineMenoBrain` : supprimer `SortPassif.Poser(fighter, 6758, 1);` (garder 6793).
2. [CRITIQUE] Poser **Mutation Instable 6758 niv. 1** sur les 5 monstres de la zone (salles normales et invoqués par le Meno :
   `SortPassif.Poser` gère l'arrivée) :
   ```
   [BrainIdentifier(4455)] public class MolUskBrain : BrainPassif { public MolUskBrain(AIFighter f) : base(f, 6758) { } }
   [BrainIdentifier(4456)] public class CrabeYolonisteBrain : BrainPassif { public CrabeYolonisteBrain(AIFighter f) : base(f, 6758) { } }
   [BrainIdentifier(4457)] public class GambafBrain : BrainPassif { public GambafBrain(AIFighter f) : base(f, 6758) { } }
   [BrainIdentifier(4458)] public class MantazeBrain : BrainPassif { public MantazeBrain(AIFighter f) : base(f, 6758) { } }
   [BrainIdentifier(4459)] public class TilamproieBrain : BrainPassif { public TilamproieBrain(AIFighter f) : base(f, 6758) { } }
   ```
   Fonctionnement attendu (données 2.51) : sans Meno, 6758 → buff `TB` 6758/2 (si pas Invulnérable) → états Mutation selon PV ;
   avec le Meno présent, le monstre devient Invulnérable, perd 1 PM, passe en Mutation I/II/III aux tours 1-3 (vita −25/−33/−50 %)
   et meurt au 4e (délais décomptés au tour de son invocateur, le Meno, pour les invoqués). Tester les deux cas.

---

## Palais de Dantinéa (palais-de-dantineacutea) — boss Dantinéa (id 4444)
### Mécanique officielle (site)
- **Diondin (4443)** : Projection Hydraulique (450 eau, pousse 2 en croix autour de la cible, ligne 2 PO) ; Fougou Rebondissant
  (500 terre cercle 2 + Intacleur, ligne 6 PO) ; Poussette Mortelle (500 feu, pousse 4, à 4 PO pile ; les Pacifistes alignés sont
  **tués par des dommages de poussée**).
- **Poulpée (4440)** : **Séduction Abyssale** (fin de tour en LdV : Insoignable 2 tours ; 2e fois : −100 PM 2 tours) ; Siphonage
  (échange avec un allié + soin 13 %) ; Tourbillonement (350 eau, −5 PO ; sur Pesanteur : poison 16 PV/PM).
- **Rilur (4442)** : Harpon-éclair (300 air + poison ; **tue les Affaiblis**) ; Charge Frontale (350 neutre, s'attire) ; chaque
  sort pose un **Glyphe Fulgurant** (Pesanteur + 600 feu).
- **Tourthon (4439)** : **Retour de Bulles** (renvoie 50 % des dégâts si l'attaquant est Insoignable) ; Attraction Lumineuse
  (250 feu, −4 PA, attire 4) ; Enlacement (800 neutre + Pacifiste, mêlée).
- **Tryde (4441)** : **Camouflage Sous-marin** (au début de son tour : invisible, +100 puissance cumulable, révèle les ennemis ;
  redevient visible et remet sa puissance à 0 s'il subit des dégâts) ; Prison Aqueuse (500 air + piège invisible en croix qui
  ramène au centre, se téléporte derrière sa cible).
- **Dantinéa (4444)** : **Pacte Maléfique** : invoque autant de **Grokillages** (0 PA/PM, 1 000 PV, places fixes) que de joueurs,
  chacun lié à un joueur ; tant que son Grokillage vit, le joueur fait **50 % de dégâts** et perd 1 PM par tour ; **tous les
  5 tours elle tue les Grokillages restants et leurs joueurs liés**, puis en réinvoque ; au début de son tour, 600 eau aux joueurs
  alignés avec les Grokillages ; à sa mort, les Grokillages meurent. Siphon d'Âme (1 000 neutre vol de vie, 3 PO) ; Danse
  Tentaculaire (600 terre + 600 air cercle 2) ; Cooquillation (échange avec un Grokillage, dès le tour 2, relance 2).
- Expéditions de guilde (Grokillages invulnérables, etc.) : version actuelle.

### État sur le serveur
- Spawns 409-413 conformes. Sorts en base = client 2.51 ; aucun effet / déclencheur non géré (zones « case fixe », 1165, 2018
  gérés).
- Cerveaux (`DonjonsRecentsBrains.cs`) : Dantinéa **Pacte Maléfique 6709**, Poulpée **Séduction Abyssale 6690**, Tryde
  **Camouflage Sous-marin 6695** (+ variantes 4502/4509/4510), Tourthon **Retour de Bulles 6689** : tous les passifs 2.51 posés.
  Rilur / Diondin : pas de passif (le Glyphe Fulgurant est dans leurs sorts) : conforme. Grokillage 4447 : pas de passif propre
  en 2.51 (tout est porté par 6709/6710/6819) : conforme.
- [DOUTE VERSION] expéditions de guilde, runes « depuis la 3.7 », chiffres (1 000, 600…).

### À faire (précis, prêt à coder)
Rien à faire (mécanique entièrement dans les données 2.51 déjà posées ; à valider en jeu à 4 joueurs : liaison Grokillage ↔
joueur, −50 %, OS au tour 6).

---

## Tour de Solar (tour-de-solar) — boss Solar (id 5100)
### Mécanique officielle (site)
- **Pyrozomb** (tous les monstres, dès le début) : un monstre tué est **ressuscité par le prochain monstre** ; frappé, un monstre
  **renvoie des dégâts en carré de 1** ; tué, il inflige des **dégâts terre en cercle 2**.
- **Nécronyx** (sort joueur, quête « L'arme fatale », tous les 2 tours) : sur un monstre, il annule Pyrozomb (renvoi, explosion,
  et le monstre ressuscité ne joue plus) ; sur Solar / Calciné : +20 % érosion 3 tours au lanceur.
- Monstres : **Hacharné (5103)** Visiosoufre (−7 PO 2 tours), Haleine Enflammée (250 feu demi-croix), Magmache (350 air ligne 3) ;
  **Halbardent (5105)** Chauffard (+200 puissance +3 PM), Geoassaut (430 terre, 30 % érosion), Lance-flammes (180 feu ligne 4) ;
  **Klémort (5101)** Conjuration Volcanique (350 feu aux joueurs à plus de 4 PO + attire 2), Magmagique (−40 tacle, −50 % dégâts
  subis 2 tours aux alliés adjacents), Épéruption (600 terre, −2 PM, case + 4 diagonales) ; **Moribombe (5104)** Grenade
  Collante (400 eau ligne 8 PO), Chaudière (bouclier 1 300 rayon 2), Zhen Tian Lei (400 air cercle 2, pousse 1) ; **Trépavois
  (5102)** Avancée Inexorable (**invulnérable à distance** + Indéplaçable 2 tours), Charge au Bouclier (400 terre carré 3),
  Glaive Sommaire (500 eau, s'attire).
- **Solar (5100)** : cycle de 4 états de 3 tours : **Aurore** (soin des alliés, +1 000 esquive PA/PM ; joueurs +1 PA) →
  **Zénith** (+200 puissance aux alliés, −6 PO, Lourd, Intaclable ; joueurs +2 PA) → **Crépuscule** (joueurs −1 PA) → **Nadir**
  (joueurs −2 PA) ; **invulnérable sauf en Nadir** ; Nécronyx sur Solar le fait passer à l'état suivant ; glyphe sur tout le
  pourtour, effet selon l'état (−300 puissance / 20 % érosion / −2 PA / −4 PO). **Si Solar meurt sans être sous Nécronyx,
  toute l'équipe meurt.** Sorts par état (Aube Saine, Lève-tôt, Rossée Matinale / Coup de Soleil, Éruption Solaire, Démons de
  Midi / Le Grand Soir, Cours du Soir, Coucher de Soleil / Nuit Blanche, Protecteur d'émoi, Démons de Minuit).

### État sur le serveur
- Spawns 434-438 conformes (5 monstres + Solar). Les 12 sorts de Solar (avec états requis Aurore / Zénith / Crépuscule / Nadir)
  et ceux des monstres sont en base = client 2.51.
- **Aucun cerveau** pour Solar ni pour les 5 monstres → **rien de la mécanique n'existe** : Solar n'entre jamais dans un état
  (donc **ne peut lancer aucun de ses 12 sorts**, tous à état requis) et n'est jamais invulnérable ; les monstres ne
  ressuscitent pas. Passifs 2.51 trouvés (non attribués, comme tout passif) :
  - **Cycle Solaire 9415** (Solar) : Padécédé 9509 en `TB`, Pyrozomb 9352 en `D`, **Aurore 9436** (puis Zénith 9437 →
    Crépuscule 9438 → Nadir 9439 → Aurore, glyphes 9440-9443 sur le pourtour), changement d'état sous Nécronyx (9415 niv. 2),
    **à sa mort sans Nécronyx : `Kill` de tous les ennemis (9415 niv. 3)**. Les états Aurore 575 / Zénith 576 / Crépuscule 577
    ont `invulnerable = true` dans SpellStates (Nadir 578 non) : `FightActor.IsInvulnerable` lit bien ce drapeau.
  - **Pyrozomb 9354** (les 5 monstres) : `X` → Padécédé 9353 ; `TB` → 9509 → **9350 niv. 3 : effet 147 « Ressuscite un
    allié »** (cible `m`, zone `C5,3`, valeur 100) puis états sur le ressuscité (`a,U`) : sous Nécronyx Invulnérable / Intacleur /
    Indéplaçable / Lourd / apparence Calciné / passe ses tours ; sinon Invulnérable 1 tour ; `D` → 9352 (renvoi) ; `EON571` → 9355
    (20 % érosion au lanceur de Nécronyx) ; `X` → 9356 (51-60 terre cercle 2).
- **Effets non gérés** (outil) : **147** (résurrection, cœur de Pyrozomb), **1126** (Pyrozomb 9352 : renvoi de 50 % des dégâts
  subis en **feu** zone carré 1, élément client 2), **1127** (Rossée Matinale 9431 : 20 % en **eau**, élément 3). (1128 = terre,
  voir Craqueleurs.)
- **Nécronyx 9348** : sort et objet 19395 « Nécronyx » présents en base, mais la quête « L'arme fatale » (1939) **n'existe pas
  dans `quests`** et aucune récompense de sort ne la donne → **les joueurs ne peuvent pas obtenir Nécronyx**. Une fois Cycle
  Solaire posé, tuer Solar sans Nécronyx tue toute l'équipe : **ne pas activer le point 1 avant d'avoir un moyen d'obtenir
  Nécronyx** (à décider par l'utilisateur : PNJ, objet d'apprentissage, sort donné à l'entrée…).
- [DOUTE VERSION] « Nécronyx ne passe plus le tour depuis la 3.7 » : en 2.51, 9348 contient `1031 Termine le tour` (le lanceur
  finit son tour) → garder 2.51. Chiffres (1 200, 900…).

### À faire (précis, prêt à coder)
1. [CRITIQUE] Cerveau Solar (après le point 5) : `[BrainIdentifier((int)MonsterIdEnum.SOLAR)] public class SolarBrain : BrainPassif { public SolarBrain(AIFighter f) : base(f, 9415) { } }`.
2. [CRITIQUE] Cerveaux Pyrozomb : `[BrainIdentifier(5101)]`, `(5102)`, `(5103)`, `(5104)`, `(5105)` → `BrainPassif(f, 9354)`
   (une classe par id, comme les Truches).
3. [CRITIQUE] Effet **147** : ajouter `[EffectHandler(EffectsEnum.Effect_147)]` sur `ReviveLastDeadAlly` (ou un gestionnaire
   jumeau) : % de PV = `GenerateEffect().Value` (dé 0/0 → `EffectDice.GenerateEffect` l.148 renvoie la valeur = 100), mort =
   `Caster.Team.GetLastDeadFighter()`, case = 1re case libre à distance 3 à 5 du lanceur (zone `C5,3`), puis
   `mort.SummoningEffect = this` — indispensable avec le correctif `JustSummonedCriterion` du Sylargh (point 2) pour que les
   effets `a,U` (Calciné, Invulnérable, passe son tour) s'appliquent au ressuscité.
4. [IMPORTANT] Effets **1126 / 1127** (et 1128) : un seul gestionnaire copié de `DispatchDamages.cs`,
   `[EffectHandler(EffectsEnum.Effect_1126)] [EffectHandler(EffectsEnum.Effect_1127)] [EffectHandler(EffectsEnum.Effect_1128)]`,
   `School = Effect.EffectId == EffectsEnum.Effect_1126 ? EffectSchoolEnum.Fire : Effect.EffectId == EffectsEnum.Effect_1127 ? EffectSchoolEnum.Water : EffectSchoolEnum.Earth`.
5. [CRITIQUE] Rendre **Nécronyx 9348** obtenable (décision utilisateur) avant d'activer 1-2.

---
## Récapitulatif

Les écarts de chiffres sont tous classés [DOUTE VERSION] (règle version) : aucun [MINEUR] à coder.
Les correctifs « moteur » sont partagés : `JustSummonedCriterion` + `ReviveLastDeadAlly` (Sylargh 1-2, réutilisés par Solar 3) ;
gestionnaire 1126/1127/1128 (Solar 4 = Craqueleurs 1).

| Donjon | nb CRITIQUE | nb IMPORTANT | nb MINEUR |
|---|---|---|---|
| Donjon des Tofus | 0 | 1 | 0 |
| Transporteur de Sylargh | 3 | 0 | 0 |
| Trône de la Cour Sombre | 0 | 1 | 0 |
| Ventre de la Baleine | 1 | 0 | 0 |
| Volière de la Haute Truche | 0 | 1 | 0 |
| Miausolée du Pounicheur | 0 | 1 | 0 |
| Pitons Rocheux des Craqueleurs | 0 | 2 | 0 |
| Plateau de Ush | 0 | 0 | 0 |
| Défi du Chalœil | 0 | 2 | 0 |
| Temple de Koutoulou | 0 | 0 | 0 |
| Vaisseau du Capitaine Meno | 2 | 0 | 0 |
| Palais de Dantinéa | 0 | 0 | 0 |
| Tour de Solar | 4 | 1 | 0 |
