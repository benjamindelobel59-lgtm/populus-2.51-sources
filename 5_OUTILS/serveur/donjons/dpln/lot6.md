# Lot 6 : audit des mécaniques (dofuspourlesnoobs → Mystic 2.51)

Méthode : page du site lue en entier, `analyse_mecaniques.py` sur tous les monstres de chaque donjon (+ mots-clés du site),
base `world`, code serveur (cerveaux, gestionnaires d'effets, déclencheurs). RÈGLE VERSION appliquée : tout ce qui n'existe pas
dans le client 2.51 (ou qui est un chiffre différent d'un sort 2.51 existant) est classé **[DOUTE VERSION]** et n'est PAS à coder.

Constats communs vérifiés pour les 13 donjons :
- **Sorts en base = sorts du client 2.51** pour tous les monstres du lot (aucun sort manquant, aucun en trop).
- **Grades (PV, PA, PM, résistances, niveau) = client 2.51** pour tous les monstres du lot.
- Les effets autrefois signalés non gérés sont maintenant gérés : 1067/1068/1069 (`DamagePercentTargetLife.cs`), 220, 240-244,
  418/420 (`StatsBuff.cs`), 1024 (`Illusions.cs`), 1125, zones `#`, `+`, `;`. Reste non géré dans ce lot : **effet 246** (Squelettes).
- `spells_effects_fix` est **vide** : la durée d'un buff créé par un déclencheur (Buff.cs l.25) vaut -1000 (= infini) dès que l'effet
  a la durée -1 (`TriggeredBuffDuration` n'est jamais renseigné). Ça touche Klime et Royalmouth ci-dessous.

---

## Repaire de Daïgoro (repaire-de-daiumlgoro) — boss Daïgoro (id 3208)
### Mécanique officielle (site)
- Kitsou Nere 530 / Nae 531 / Nakwa 523 / Nufeu 532 : 100 % de résistance dans un élément (terre / air / eau / feu), frappe en ligne
  à courte portée, boost PA/PM/dommages + soin des alliés, **invoque un double puis devient invisible**.
- Daïgoro : **état Lourd** (ni porté, ni jeté, ni Coopération), **réduit les dommages reçus de 25**, se transpose avec ses alliés,
  sort d'approche type « Reuche » avec vol de vie, CàC qui donne 15 % de faiblesse partout. Ces deux sorts posent des
  **marques** : **Première → … → Cinquième Marque** (une de plus par coup reçu) ; la **Cinquième met Affaibli** (pas de CàC),
  **même après la mort de Daïgoro**.
### État sur le serveur
- Kitsous : conformes 2.51. Kitnition (neutre 26-30, PO 1-2), Ruse du Kitsoune (+1 PA, +2 PM, +100 % dommages, soin 251-500,
  zone C63, relance 3), Illusion spectrale (invoque le double 562-565 + invisibilité 1 tour, relance 5 ou 7). Les doubles n'ont
  aucun sort, comme dans le client.
- Daïgoro : cerveau `DaigoroBrain` (PassifsDonjonsBrains.cs) qui pose **Corps de Dolomite 3037** = Lourd + réduction 5 sur les coups
  au CàC (105, décl. DCAC) + **un état Pandawush tiré au sort** (Aerdala/Akwadala/Feudala/Terrdala/Pandala, avec +15 % / -15 %
  de résistances) qui débloque la Marque et la Lance Estrale de cet élément. Paume de pain 3040 (avance jusqu'à la cible + recul
  1 à 6 selon la marque), Marques 3042-3046 (2 PA, CàC, dégâts 6-10 → 31-35 selon la marque, + effet d'élément), Lances
  Estrales 3047/3272-3275 (10 % → 20 % des PV de la cible), Permutation Pandawushu (Bûche 3209 + échange),
  Destruction de la montagne (la Bûche explose en zone, 100 → 150 % selon la marque).
- Marques 111 → 115 : l'empilement est correct (cibles calculées avant l'application, `StateCriterion.CheckWhenExecute = false`).
- **État 115 « Cinquième Marque » (preventsFight = vrai) n'empêche PAS l'arme** : `CharacterFighter.CanUseWeapon` ne teste que
  l'état 42 Affaibli (l.345). La mécanique « 5e marque → plus de CàC » ne marche donc pas côté serveur.
- Marques posées par Paume de pain : désenvoûtables 1 → retirées à la mort de Daïgoro (`RemoveAllCastedBuffs(DISPELLABLE_BY_DEATH)`) ;
  celles des sorts Marque : désenvoûtables 3 → elles restent. C'est la donnée 2.51 : conforme.
- Tirage du Pandawush : `DefaultSpellCastHandler.Initialize` pondère chaque groupe aléatoire par la SOMME des `random` de ses effets.
  Groupes 1-4 = 3 effets (21), groupe 5 Pandala = 1 effet (7) → Pandala sort 7,7 % du temps au lieu de 20 %.
- [DOUTE VERSION] « -25 dommages reçus » (2.51 : réduction 5 seulement au CàC), « Reuche avec vol de vie », « CàC 15 % de
  faiblesse » (2.51 : -15 % de résistances seulement avec Marque de Feudala) : version actuelle du jeu, pas la 2.51.
### À faire (précis, prêt à coder)
1. [IMPORTANT] Les états qui « empêchent de combattre » doivent bloquer l'arme. `game/actors/Fight/CharacterFighter.cs`,
   `CanUseWeapon` : remplacer
   `if (HasState((int)SpellStatesEnum.AFFAIBLI_42)) return false;` par
   `if (GetStates().Any(x => !x.IsDisabled && x.State.PreventsFight)) return false;`
   (couvre 42 Affaibli ET 115 Cinquième Marque de Daïgoro ; `System.Linq` est déjà importé). Le client grise peut-être déjà
   l'arme ; le serveur doit quand même refuser.
2. [MINEUR] (à valider, ça touche tous les sorts à groupes aléatoires) `game/spells/Casts/DefaultSpellCastHandler.cs`, `Initialize` :
   pondérer un groupe par la valeur `Random` d'UN de ses effets (`groupEffects.Max(e => e.Random)`), et faire de `totalRandSum`
   la somme de ce maximum sur les groupes (`groups.Sum(g => g.Max(e => e.Random))`). Effet ici : Pandala passe de 7,7 % à 20 %.
   Attention : ça change les probabilités de tous les sorts dont les groupes n'ont pas le même nombre d'effets (y compris
   peut-être Kolosso / Korriandre / Glourséleste) : à faire seulement si l'utilisateur est d'accord.

## Repaire de Skeunk (repaire-de-skeunk) — boss Skeunk (id 780)
### Mécanique officielle (site)
- Diamantine 681 : invoque la **Poupée Mortelle** (OS au CàC, faiblesse aléatoire), **Pesanteur dès le début du combat**, terre
  à distance, se boost en PV, **Affaibli** au CàC, faiblesse, désinvoque. Accompagnée de Warko Violet 745 / Koalak Farouche 756
  (invisible).
- Émeraude 673 : transpositions qui frappent ; Poupée Émeraude 674 : pareil avec retrait PA, 90 % de résistances sauf air.
- Rubise 677 : pousse en frappant, **Totémisation** (0 PA / 0 PM), totems (boost / soin / explosion). Avec Fauchalak 783 (OS),
  Koalak Sanguin 744 (soin), Guerrier Koalak 749.
- Saphira 675 : retrait PA, invisible. **Poupée Affamée 676** : « reconstitue » tous les monstres à chaque tour ; elle mange les
  invocations (+1 PA chacune, 2 max par tour) et **explose à 8 PA** (feu sur tous).
- Skeunk : « reconstitue » un allié ou lui-même, soigne, retrait PM, feu, désenvoûte, boost PA.
### État sur le serveur
- Tous conformes 2.51 et gérés (outil : rien à faire). Lourdeur 573 (Pesanteur 4 tours sur tous les ennemis, relance 4) est un
  sort normal ; Poupée Mortelle 682 Festin (tue au CàC) ; Poupée Affamée : Coupe-faim 710 (tue une invocation J/j, +1 PA 4 tours),
  Digestion Explosive 711 (8 PA : feu 101-150 sur tous + suicide), Saphir Apaisant 712 (soin 1250 sur tous les alliés) ;
  Camouflage Saphiresque (418/420 maintenant gérés) ; Skeunk : 5 chants (feu C2, soin, +2 PA, désenvoûtement, -4 PM).
- [DOUTE VERSION] Pesanteur « dès le début du combat », « reconstitue » (2.51 : soin fixe 1250), Skeunk qui ressuscite, faiblesse
  aléatoire de la Poupée Mortelle : absents du client 2.51.
### À faire (précis, prêt à coder)
Rien à faire.

## Repaire des Pandikazes (repaire-des-pandikazes) — boss Maître Pandore (id 612)
### Mécanique officielle (site)
- Pandikaze Vertige 579 (90 % de résistances sauf air -200 %, air à distance), Pandikaze Aérien 578 (200 % air, boost PA,
  coopération + eau, air au CàC), Pandikwakaze 580 (200 % dans un élément au hasard, pousse, invoque un Bwak), Pandulkaze 581
  (feu/air à distance, retrait PA), Guerrier Pandikaze 607 (invoque 2 **Pandamanis** 100 % partout, **Lourd/Pesanteur** tout le
  combat, 80 % de résistances aléatoires).
- Maître Pandore : invoque la **Bouate de Pandore** (coffre qui invoque un monstre de Pandala au hasard), vol de vie / PA.
### État sur le serveur
- Conforme 2.51 : Bouate 606 « Ouverture hasardeuse » = 1 monstre tiré parmi 23 (tous présents en base), Bambou Musical
  (échange + vol de vie eau + retrait PM/PA), Lourdeur 573, Pandamani 609 (sans sort).
- [DOUTE VERSION] résistances « aléatoires » du Guerrier Pandikaze et du Pandikwakaze : 2.51 = résistances fixes par grade.
### À faire (précis, prêt à coder)
Rien à faire.

## Ring du Capitaine Ekarlatte (ring-du-capitaine-ekarlatte) — boss Capitaine Ekarlatte (id 3753)
### Mécanique officielle (site)
- Page incomplète (sorts « à venir »). Donjon de dimension : 1 salle, **5 vagues** (le nombre de monstres suit le nombre de
  joueurs, une vague arrive plus tôt si la précédente est morte). Le Capitaine attire, échange de place, téléporte ; le piège du
  Tromblion pousse de 2.
### État sur le serveur
- Cerveaux présents : `EkarlatteBrain` (Kutcha Libre 4804 : quand il est touché il revient à sa position précédente, soin 15 % s'il
  y a téléfrag) et `TromblionBrain` (Isolement 4730 : piège en fin de tour). Vagues ajoutées (4 vagues, `ajouter_vagues_dimensions`).
- Sorts conformes 2.51 (Case départ, Tourbilaule, Dansorcellement, Enfumage ; Kartouche, Sramouraï, Tromblion, Elsoummo, Milimaître).
- Remarque (non corrigeable sans source) : Kutcha Libre contient DEUX effets 786 « soigne l'attaquant de 100 % des dommages » ;
  le serveur les applique tous les deux (`GiveHpPercentWhenAttack`) → l'attaquant récupère 200 % des dommages qu'il inflige. C'est la
  donnée 2.51 ; à vérifier en jeu si ça paraît anormal.
### À faire (précis, prêt à coder)
Rien à faire.

## Salons privés de Klime (salons-priveacutes-de-klime) — boss Klime (id 3384)
### Mécanique officielle (site)
- Cuirboule 3383 (1 PM ; Maboule terre très fort en ligne sans LdV à 11 PO si pas bougé, attire 10 / pousse 10 ; poison eau par PM ;
  +600 dommages et soin quand on se détacle de lui). Empaillé 3382 (téléportation + érosion 40 %, attirance + échange + 500 neutre
  + Pesanteur, +1000 dommages avec Pesanteur). Grodruche 3380 (1 tour sur 2 **Indéplaçable**, +2 PM en cercle 4 quand il est
  touché, renvoie les dégâts aux entités à exactement 5 PO). Harrogant 3379 (ligne à 10 PO, soin + renvoi à 6 PO 1 tour sur 2).
  Peunch 3381 (**Affaibli** + -100 fuite cercle 3, vol de vie feu + vol 100 intel anneau 2, eau = 50 % des PV manquants).
- Klime : **Invulnérable** ; **vulnérable 1 tour** quand il subit des **dommages de poussée indirects** (on pousse quelqu'un sur lui),
  **impossible avant le tour 3**. **Carcassetagne** à chaque début de tour d'un monstre : **glyphe blanc** (2 tours) sous le
  monstre + **Pacifiste 1 tour** aux ennemis sans **Peau lisse** ; glyphe = Peau lisse aux joueurs qui finissent dedans,
  +300 dommages aux monstres. **Peau lisse au début du combat** (2 tours). Cuir à feu doux seulement quand il est vulnérable.
### État sur le serveur
- Cerveau `KlimBrain` (Boss/KlimeBrain.cs) : lance **Galuchat 3485** au début du combat = Invulnérable + désactivation sur
  déclencheur PPD avec **délai 2** (donc pas avant le tour 3 : conforme) + Carcassetagne 3486 sur tous les alliés (début de tour) +
  Peau lisse 3 tours aux ennemis. Glyphe 3487 en effet 402 = fin de tour (conforme). Cuir à feu doux 3489 : état interdit 56
  (Invulnérable) → seulement vulnérable (conforme).
- **Bug : une fois vulnérable, Klime le reste pour tout le combat.** La désactivation (952, durée -1, déclencheur PPD) crée un
  `DisableStateBuff` avec `triggerer` → `Buff` l.25 → durée -1000 (infini), `spells_effects_fix` vide. Royalmouth et Grolloum
  ont le même cas, réglé par `VulnerableJusquAuTourDuDeclencheur` ; Klime ne l'a pas.
- [DOUTE VERSION] Peau lisse 3 tours en 2.51 (site : 2 tours ; avec le tour en cours ça revient sans doute au même).
### À faire (précis, prêt à coder)
1. [CRITIQUE] `AI/Fights/Brain/Custom/Boss/KlimeBrain.cs`, `Fight_FightStarted` : après le `CastAutoSpell(SHAGREEN)`, ajouter
   `new VulnerableJusquAuTourDuDeclencheur(Fighter);` (classe dans `Stump.Server.WorldServer.AI.Fights.Brain.Custom.Boss`, à
   importer : `KlimBrain` est dans le namespace `...Brain.Custom`) et `obj.FightStarted -= Fight_FightStarted;`. Résultat : le joueur
   qui a poussé quelqu'un sur Klime le rend vulnérable jusqu'au début de son tour suivant, puis Klime redevient invulnérable.

## Antre de la Crocabulia (antre-de-crocabulia) — boss Crocabulia (id 854)
### Mécanique officielle (site)
- Version actuelle : **Coquillettes Ula** (1 chance sur 4 par début de tour d'invoquer une Coquille Brutale/Protectrice/Soigneuse/
  Véloce, boost des alliés, « Coup quillesque » si on la tue), **Éruption** du Dragmatique (pression magmatique 3 → 0), monstres
  Dragacé, Dragmatique, Draguaindrop…, boss Crokenjambe / Crokabriole / Pied Tinement / Souffle. Pas d'invulnérabilité.
### État sur le serveur
- Donjon 2.51 complètement différent : Dragoss (Blanc 885, Saphir 884, Doré 886, Noir 848, et Éveillés 878/905/879/858),
  Dragoeuf Guerrier 853 / Volant 862, Dragueuse 855 ; Crocabulia : Cri (renvoi + dommages aux alliés quand ils sont touchés, effet
  220 géré), Souffle (feu L10 + recul 5), Libération, Appel (invoque un Dragoss / Dragoeuf au hasard), Coquillement 11457
  (Coquille Explosive 5494). Tout est géré (outil : rien).
- [DOUTE VERSION] Coquillettes Ula, Éruption, Pression magmatique, Dragacé/Dragmatique/Draguaindrop, sorts du boss du site :
  aucun dans le client 2.51.
### À faire (précis, prêt à coder)
Rien à faire.

## Donjon des Scarafeuilles (donjon-des-scarafeuilles) — boss Scarabosse Doré (id 797)
### Mécanique officielle (site)
- Scarafeuilles Blanc 241 / Bleu 198 / Rouge 194 / Vert 240 : 100 % dans leur élément, -50 % dans l'opposé, Scaraforce,
  Spirit Element, Scarinvoc (Flammèche), Elemental Dispersion. Noir 795 (100 % neutre, Scarinvi, Scarimmo). Immature 798
  (**Scaracroissance** : devient un Scarafeuille de couleur au bout de 2 tours ; Scarapoison, Scarasoin, Scarenvol).
- Boss : Brouillard Empoisonné (-6 PO -2 PM), Expulsion, Naissance (Immature), Picoti (vol de vie élément aléatoire),
  Premiers soins. Pas d'invulnérabilité.
### État sur le serveur
- Conforme 2.51 : Picoti tire bien 1 élément sur 4 (random 25 chacun), Naissance invoque l'Immature 798, Brouillard (-10 PO,
  -1 PA / -1 PM par tour 2 tours), Immature (Empoisonnement, Guigne, Envolupté), Noir (Disparition groupée, Immobilisation…).
- [DOUTE VERSION] Scaracroissance, Scarapoison, Scarasoin, Scarenvol, Scarinvi, Scarimmo : absents du client 2.51.
### À faire (précis, prêt à coder)
Rien à faire.

## Serre du Royalmouth (serre-du-royalmouth) — boss Royalmouth (id 2854)
### Mécanique officielle (site)
- Bouftonmouth 2853, Boufmouth 2850, Boufmouth de guerre 2851, Boufmouth légendaire 2852 (sorts classiques).
- Royalmouth : **Inimouth** = Invulnérable infini ; **poussé → vulnérable 1 tour** (du tour du pousseur au tour suivant du
  pousseur) **et +4 PM pour 1 tour (cumulable)** ; dommages de poussée sur lui → **OS des personnages en ligne** + Berserkmouth
  (+100 dommages aux alliés). Aléamouth : invoque un monstre du donjon (+ bonus / malus selon le monstre). Regroupmouth, Lichemouth.
### État sur le serveur
- `RoyalMouthBrain` : Inimouth 2356 au début + `VulnerableJusquAuTourDuDeclencheur` → invulnérabilité conforme.
- **Bug : le +4 PM du déclencheur « poussé » (128, durée -1) est permanent et s'empile à chaque poussée** : `MPBuff.TriggerBuff`
  → `AddStatBuffDirectly(..., triggerrer)` → durée -1000 (`TriggeredBuffDuration` jamais renseigné).
- [DOUTE VERSION] bonus / malus d'Aléamouth selon le monstre invoqué : pas dans le client 2.51 (Aléamouth = invocation seule).
### À faire (précis, prêt à coder)
1. [IMPORTANT] `AI/Fights/Brain/Custom/Boss/RoyalMouthBrain.cs`, dans `Fight_FightStarted` (après `new VulnerableJusquAuTourDuDeclencheur`) :
   `Fighter.BuffAdded += (f, b) => { if (b is StatBuff s && s.Spell.Id == (int)SpellIdEnum.INITIADON && s.Caracteristic == PlayerFields.MP && s.Duration == -1000) s.Duration = 1; };`
   (`using Stump.Server.WorldServer.Game.Fights.Buffs;` + `Stump.DofusProtocol.Enums`). La durée se décompte au début du tour du
   pousseur (`DecrementReference` = déclencheur) → +4 PM jusqu'au tour suivant du pousseur, cumulable, comme sur le site.
   Correctif ciblé exprès (un correctif général dans `MPBuff` toucherait d'autres monstres).

## Donjon des Squelettes (donjon-des-squelettes) — boss Chafer Rōnin (id 3238)
### Mécanique officielle (site)
- Chafer 54 / Fantassin 396 (Coup du Chafer terre CàC), Chafer Invisible 110 (Camouflage 2 tours, Pantalonnade), Chafer Primitif
  3240 (Curare + poison eau, **Élancé de Couteau : -10 fuite, -10 résistance eau, s'attire de 2**, Robustesse -40 au CàC),
  Rib 108 (renvoi 15, Embrochement recul 5), Chafer Draugr 3239 (Do Fus Rah ! : ±10 résistance poussée + recul 5 en ligne ;
  Hel : poison air par PM + glyphe ; Mjöllnir + Pesanteur).
- Chafer Rōnin : **Bunshin = 4 illusions** (l'illusion touchée disparaît ; si on touche le vrai, toutes disparaissent), le Rōnin
  passe son tour ; Bushido, Kikoha.
### État sur le serveur
- Sorts conformes 2.51. Bunshin : 1024 géré par `Illusions.cs` (illusions qui meurent quand on les touche, toutes quand le vrai est
  touché, au début de son tour) + passe son tour.
- **Effet 246 « -#1 réduction eau » NON GÉRÉ** (Élancé de Couteau 3171 du Chafer Primitif) : seuls 240-244 (bonus) sont gérés.
- Bunshin : `Illusions.cs` ignore la valeur (4) et place une image dans 4 directions dont celle du vrai Rōnin → 3 fausses + le vrai
  (2.51 : 4 illusions + le vrai).
- [DOUTE VERSION] « Bunshin seulement à partir du tour 2 », chiffres du site : version actuelle.
### À faire (précis, prêt à coder)
1. [IMPORTANT] `game/effects/handlers/Spells/Debuffs/StatsDebuff.cs` : ajouter
   `[EffectHandler(EffectsEnum.Effect_SubEarthElementReduction)]` … `Effect_SubNeutralElementReduction` (245 à 249) et, dans
   `GetEffectCaracteristic`, `case Effect_SubEarthElementReduction: return PlayerFields.EarthElementReduction;` (Water → 
   WaterElementReduction, Air → AirElementReduction, Fire → FireElementReduction, Neutral → NeutralElementReduction, mêmes champs
   que `StatsBuff.cs` l.123-132). La valeur est déjà négative (`-integerEffect.Value`).
2. [MINEUR] `Illusions.cs` pour 1024 seulement : créer `Dice.DiceNum` (4) images autour du Rōnin sur des cases libres au lieu
   de la règle « 4 directions dont la case visée ». À faire seulement si on veut la valeur exacte ; le principe marche déjà.

## Tanière Givrefoux (taniegravere-givrefoux) — boss Tengu Givrefoux (id 2967) [+ Fuji Givrefoux Nourricière 3234]
### Mécanique officielle (site)
- Kami 2982 (Foulette, Foux de la fortune, Foux ou rien), Maho 2889, Soryo 2890 (Fourniture), **Yokaï 2888 « Fourrure »**
  (Lourd ; poussé/attiré → **Floutage** 2 tours pour lui et les alliés en diagonale, +1 PM, +10 dommages, Affaibli aux ennemis en
  diagonale), **Yomi 2891 « Foudre »** (Lourd ; touché à l'ARME → **Fulguration** 2 tours en ligne, +6 PO, +50 soins, 20 % d'érosion).
- Tengu : **Croquette** (Invulnérable + Lourd) ; avec Floutage ET Fulguration au début de son tour → **Toison givrée = vulnérable
  4 tours** ; Fulguration seul → Mornifle (OS) ; Floutage seul → Torgnole (30 % d'érosion). Foux d'amour : 1 Yokaï + 1 Yomi.
- Fuji : **Instinct maternel** (mort d'un monstre → +1 PM à tous, +1000 dommages à Fuji et Tengu).
### État sur le serveur
- Déjà traité (`GivrefouxBrains.cs`) : Croquette 2677, Fourrure 668, Foudre 669, Instinct maternel 2676 (Fuji 2970 et Nourricière
  3234), Toison givrée lancée par le cerveau, 2 invocations max. Effets / zones (`#63`, `+63`, `X63`) gérés.
- [DOUTE VERSION] Foudre : en 2.51 le déclencheur est DCAC (coup au corps à corps), le site dit « dégâts d'arme ». On garde la 2.51.
### À faire (précis, prêt à coder)
Rien à faire.

## Terrier du Wa Wabbit (terrier-du-wa-wabbit) — boss Wa Wobot (id 3460)
(Sur le serveur : spawns 120-124 « Wa Wobot », maps 116654593 → 116654595. Monstres : Black Wo Wabbit 3461, Tiwobot 3474,
Wobot 181, Wobot Kiafin 3462, Blanc Pa Wabbit 3463, invocations Wobot Tamponneur 3459 et Cawotte Woyale 3458.)
### Mécanique officielle (site)
- Black Wo Wabbit : Aspiwation (attire 10, air, Cawotte Woyale à l'ancienne place, 1 seule), Étweinte (terre + Pesanteur),
  **Ventwe Webondi (passif)** : touché au CàC → repousse l'attaquant de 3 et -3 PM.
- Blanc Pa Wabbit : Cawotte Suwvitaminée, Mawtyw (touché à distance → +1 PA +20 dommages en cercle 2), Motivation Cawottique.
- Tiwobot : Coup de Boule (Affaibli + recul 1), Lancew de Cawotte, Wetouw du Wabbit.
- Wobot et Wobot Kiafin : **Louwdeuw (passif)** : poussé ou échangé de place → son attaquant subit 70 feu.
- Wa Wobot : Mékattwaction (attire 12 + invoque un Tamponneur à l'ancienne place), Twansmutation (Cawotte → Tamponneur),
  Substitution (échange avec un Tamponneur), Mékawapace (+30 % résistances), Wouste.
- **Wobot Tamponneur – Bumper (passif)** : quand il subit des **dommages de poussée indirects**, il repousse tout ce qui le touche
  de 6, le pousseur est **téléporté symétriquement** ; le Wa Wobot repoussé perd 500 résistance poussée ; Bump Incontrôlable
  entre Tamponneurs alignés ; **Entewwement** (téléportation impossible → les Tamponneurs en ligne meurent). C'est LA mécanique
  pour tuer le boss (rebonds).
### État sur le serveur
- Sorts actifs conformes 2.51 (outil : rien).
- **Les 3 passifs existent dans le client 2.51 mais ne sont JAMAIS lancés (aucun cerveau)** : Bumper 3788 (→ Bump 3787,
  Entewwement 3796, Bump Incotwôlable 3840), Louwdeuw 3798 (→ Inflexibilité 3805, déclencheur `MS|P`), Ventwe Webondi 3794
  (→ Webond 3793, déclencheur DM). L'ancien audit (« rien à coder ») était faux.
- **Cible `O` (dernier attaquant / déclencheur) jamais atteinte dans ces sorts déclenchés** : 792 fait lancer le sous-sort par le
  porteur sur SA case (`CastSpell.cs` DefaultBuffTrigger) ; Webond, Inflexibilité et Bump visent `…,O` en zone P1 → seule la case
  du porteur est regardée, l'attaquant n'est jamais dedans → recul / dégâts / téléportation symétrique sans effet.
- **Déclencheur `MS` (échange de place) jamais émis** : mappé sur `BuffTriggerType.Unknown_6` mais aucun code ne l'appelle
  (`SwitchPosition.cs` n'émet que OnMoved).
- [DOUTE] Le client 2.51 ne dit pas quels monstres portent Louwdeuw : on suit le site (Wobot 181 et Wobot Kiafin 3462).
### À faire (précis, prêt à coder)
1. [CRITIQUE] Cerveau du Wobot Tamponneur, dans `AI/Fights/Brain/Custom/Boss/PassifsDonjonsBrains.cs` :
   `[BrainIdentifier(3459)] public class WobotTamponneurBrain : BrainPassif { public WobotTamponneurBrain(AIFighter f) : base(f, 3788) { } }`
   (`SortPassif.Poser` gère l'arrivée en cours de combat : invocation 181 et 405 Twansmutation). À tester : que `FighterAdded`
   est bien émis aussi pour l'invocation par 405 (KillAndSummon).
2. [IMPORTANT] Même fichier :
   `[BrainIdentifier(3461)] public class BlackWoWabbitBrain : BrainPassif { public BlackWoWabbitBrain(AIFighter f) : base(f, 3794) { } }`
   `[BrainIdentifier(181)] public class WobotBrain : BrainPassif { public WobotBrain(AIFighter f) : base(f, 3798) { } }`
   `[BrainIdentifier(3462)] public class WobotKiafinBrain : BrainPassif { public WobotKiafinBrain(AIFighter f) : base(f, 3798) { } }`
3. [CRITIQUE] (nécessaire pour 1 et 2) cible `O` dans les sorts lancés par un monstre :
   - `game/effects/handlers/Spells/Targets/LastAttackerCriterion.cs`, `IsTargetValid` :
     `var reference = (handler.Caster is AIFighter ? handler.CastHandler?.Informations?.Triggerer : null) ?? handler.Caster.LastAttacker;`
     puis `return Required ? reference == actor : reference != actor;`
   - `game/effects/handlers/Spells/SpellEffectHandler.cs`, `GetAffectedActors()` : si `Caster is AIFighter` et qu'un critère
     `LastAttackerCriterion { Required: true }` est présent, ajouter la référence ci-dessus à la liste quand elle est vivante, n'y est
     pas déjà et passe `IsValidTarget` (même hors zone).
   Limité aux monstres exprès : 59 sorts du client utilisent `O`, dont beaucoup de sorts de classe (Bouclier Temporel 9917,
   Chifoumi…) qu'il ne faut pas changer. Aucun sort de Kolosso / Korriandre / Glourséleste n'utilise `O`.
4. [IMPORTANT] `game/effects/handlers/Spells/Move/SwitchPosition.cs`, après `target.TriggerBuffs(Caster, BuffTriggerType.OnMoved);` :
   ajouter `target.TriggerBuffs(Caster, BuffTriggerType.Unknown_6); // MS : échangé de place` (Louwdeuw sur échange de place).
5. À tester en jeu ensuite : un monstre poussé contre un Tamponneur → recul 6 + téléportation symétrique du pousseur ; deux
   Tamponneurs alignés à ≤ 6 cases → rebonds (le Wa Wobot à -500 résistance poussée).

## Théâtre de Dramak (theacuteacirctre-de-dramak) — mini-boss Dramak (id 3482), boss Maître des Pantins (id 3476)
### Mécanique officielle (site)
- Bouledogre 3487, Gobvious 3486, Molette 3485, Rhinoféroce 3484 (**Inébranlable**).
- Dramak : **Invulnérable** tout le combat, Lourd, Inébranlable, Intacleur ; Pantin à contrôler (seuil 10 %), « Dénouement » qui
  retire l'invulnérabilité 2 tours ; Entracte, Manipulation, **Marionnette** (0 PA / 0 PM, au début du tour de la victime : 200 eau
  autour + soin des monstres).
- Maître des Pantins : **Mise en scène** (Invulnérable + 5 Marionnettes ; **vulnérable 2 tours quand les 5 sont mortes**),
  Ensecret (réinvoque les mortes, à partir du tour 5), **Bunraku** (Petite Marionnette dans le camp du joueur, copie de ses
  stats ; retiré si un allié le frappe), Tirer les ficelles. Marionnettes : **Représentation** (-100 puissance +20 % résistances à
  l'arrivée) + sort propre à chaque couleur.
### État sur le serveur
- Déjà traité (`FrigostOtomaiBrains.cs`) : Dramak = Spectacle vivant 3861 (Invulnérable ; un joueur touché au CàC désactive
  l'invulnérabilité en croix autour de lui 1 tour), Maître des Pantins = Mise en scène 3875 au grade du monstre (Actes 1-5,
  vulnérable 2 tours sans Acte), Marionnettes = Représentation 3873 niveaux 1-5 (leur mort retire l'Acte correspondant).
  Bunraku 3961 → la victime invoque la Petite Marionnette 3522 dans son camp (donnée 2.51). Tout est géré (outil : rien).
- [DOUTE VERSION] Pantin à contrôler + « Dénouement » (absent du client 2.51 : Pantin 3483 n'a que Coup de pied), Petite
  Marionnette qui copie les stats, Bunraku retiré par un coup allié, Ensecret « à partir du tour 5 » (2.51 : relance 3, état
  Invulnérable requis).
### À faire (précis, prêt à coder)
Rien à faire.

## Tofulailler Royal (tofulailler-royal) — boss Tofu Royal (id 382)
### Mécanique officielle (site)
- Tofutoflamme 3300 (invisibilité de groupe, feu + poison), Vilain Petit Tofu 3299 (distance), Tofuzmo 807 (retrait PO),
  Tofubine 3301 (poison par PA, soin, boost PA), Tofu Dodu 3302 (boost PA/soins, retrait agilité/PA). Tofu Royal : « 30 PM »,
  terre au CàC, air en zone, -60 vitalité.
### État sur le serveur
- Conforme 2.51 et géré (Écrasement Royal, Déchiquetage, Invocation Royale de Tofu, Béco du Tofu Royal ; 1134 géré).
### À faire (précis, prêt à coder)
Rien à faire.

---

## Récapitulatif

| Donjon | CRITIQUE | IMPORTANT | MINEUR |
|---|---|---|---|
| Repaire de Daïgoro | 0 | 1 | 1 |
| Repaire de Skeunk | 0 | 0 | 0 |
| Repaire des Pandikazes | 0 | 0 | 0 |
| Ring du Capitaine Ekarlatte | 0 | 0 | 0 |
| Salons privés de Klime | 1 | 0 | 0 |
| Antre de la Crocabulia | 0 | 0 | 0 |
| Donjon des Scarafeuilles | 0 | 0 | 0 |
| Serre du Royalmouth | 0 | 1 | 0 |
| Donjon des Squelettes | 0 | 1 | 1 |
| Tanière Givrefoux | 0 | 0 | 0 |
| Terrier du Wa Wabbit | 2 | 2 | 0 |
| Théâtre de Dramak | 0 | 0 | 0 |
| Tofulailler Royal | 0 | 0 | 0 |
| **Total** | **3** | **5** | **2** |
