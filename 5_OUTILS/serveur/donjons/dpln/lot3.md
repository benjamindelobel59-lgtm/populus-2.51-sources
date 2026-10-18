# Lot 3 : audit des mécaniques (dofuspourlesnoobs → Mystic 2.51)

J'ai tout fait en lecture seule, avec la **RÈGLE VERSION** appliquée : seuls les sorts et passifs présents dans le client 2.51 (ou déjà en base) donnent lieu à une correction. Tout le reste est classé **[DOUTE VERSION]** et sort des corrections.
Mes sources : les pages du site, `analyse_mecaniques.py` (lancé une seule fois pour les ~90 monstres du lot, avec 35 mots-clés de passifs), la base `world` et le code serveur (cerveaux, `CastSpell.cs`, `Buff.cs`, `Fight.cs`, `TriggerBuff.cs`, `Heal.cs`, `SpellEffectHandler.cs`).
Seul effet non géré dans tout le lot d'après l'outil : `130 Effect_StealKamas` (Bandit du clan des Roublards). Les déclencheurs `P|MA`, l'effet `407` et le soin à durée signalés par l'ancien audit sont maintenant gérés (`TriggerBuff.cs` « MA », `Heal.cs` avec 407 et les déclencheurs propres).

Points de moteur vérifiés, qui valent pour tout le lot :
- Les effets aléatoires (random/group) sont gérés (`DefaultSpellCastHandler.Initialize`). `initialCooldown` est respecté (`SpellHistory`).
- Les sorts lancés par un buff (792/793) partent du porteur, sur sa case (`CastSpellEffect.DefaultBuffTrigger`). Les délais (`delay`) sont décomptés au début du tour du lanceur (`Fight.OnTurnStarted` → `DecrementAllCastedBuffsDuration`).
- Limite générale, rien à coder ici : un buff à déclencheur ne part qu'**une fois par séquence** (`TriggerBuff.Apply`, `m_lastTriggeredSequence`). Un sort à plusieurs lignes de dégâts ne déclenche donc qu'une fois un buff « D » : Ofranor, Spore Hanchambre, Bizarrerie, Hololole…

---

## Domaine Ancestral (domaine-ancestral) — boss Abraknyde Ancestral (id 173)
### Mécanique officielle (site)
- Abrakne Sombre (651) : se téléporte, frappe terre au CàC, retire des PA, invoque des Champas.
- Abraknyde Sombre (253) : frappe à distance (~150), retire des PA, invoque des Tronknydes.
- Abraknyde Vénérable (254) : seulement des noms de sorts (Écrasement Handicapant, Reconstitution, Abraknassion, Écorce Blindée).
- Araknotron (917) : booste ses alliés en dommages et en PO, frappe à distance.
- Boss :
  - Morsure Sylvestre : −2 PA 1 tour, 200 neutre, −100 puissance 2 tours, 2 PO sans LdV, 1 fois par tour.
  - Branche Paralysante : poison de fin de tour 3 tours, 28 neutre par PA utilisé, 8 PO, 2 fois par tour.
  - Invocation d'Arakne Majeure : relance 5, à partir du tour 3.
  - Reconstitution Abraknydienne : +2 000 PV, relance 5, à partir du tour 4.
  - Résistances 53 % sauf agilité et feu. **Aucune règle spéciale ni invulnérabilité.**
### État sur le serveur
- Groupe du boss (spawn 139) conforme. Pas de passif dans le client 2.51, donc pas besoin de cerveau.
- Sorts 2.51 présents :
  - 240 Branche Paralysante (1134, 1-4 par PA, TE 3 tours).
  - 231 Morsure Sylvestre (−1 PA, 21-30 neutre, −100 agilité 3 tours).
  - 466 Invocation d'Arakne Majeure (initialCooldown 2).
  - 5526 Reconstitution Abraknydienne (soin 181-230, initialCooldown 5).
  - Les monstres ont leurs sorts 2.51 : Motivation Sylvestre (état requis par Racine Paralysante), Écrasement, Bond affaiblissant, Complicité…
- **[DOUTE VERSION]** −2 PA, 200 dégâts, −puissance au lieu de −agilité, soin 2 000, noms des sorts du Vénérable : ce sont des valeurs de la version actuelle.
### À faire
Rien à faire.

## Château Ensablé (chateau-ensable) — boss Mob l'Éponge (id 928)
### Mécanique officielle (site, version donjon)
- Pichon Blanc (922) : Bouffée d'Air (15 air CàC, −10 esquive PM 2 tours) ; Nage Terrestre (cercle 2 : +5 fuite/tacle aux alliés, −50 agilité aux ennemis, 2 tours).
- Pichon Bleu (921) : Vaguelette (15 eau, −1 PM, ligne perpendiculaire 3, 8 PO) ; Résistivité (+10 % résistances aux alliés, −50 chance aux ennemis).
- Pichon Kloune (924) : Blag (5 terre + 5 air à 3 PO, poison air/feu 1 tour) ; Klounerie (renvoi 16, 1 tour).
- Pichon Orange (920) : Sable Brûlant (10 feu en croix 1, 5 PO) ; Sel Marin (soin 9-12, −50 intelligence, 2 tours).
- Pichon Vert (923) : Reflux (25 terre CàC, repousse 1) ; Onde Enrageante (+50 puissance aux alliés, −50 force aux ennemis).
- Boss :
  - Rinçage : désenvoûte les ennemis en cercle 2.
  - Dégraissage : 10 eau (50 en CC) en ligne de 9, −2 PO.
  - Régénération Spontanée : soin 60 en cercle 2.
  - **Aucune règle spéciale.**
- Section « Expédition de guilde » : Mob 100 % de résistances, Pichons invulnérables, renvoi des dégâts des Pichons sur le Mob, passif Poissirque, glyphes…
### État sur le serveur
- Tous les sorts 2.51 sont présents et conformes :
  - Mob : 990 (vol de vie eau 15-20 en L8, −2 PO), 991 (réduit la durée des effets de 2 en C2), 992 (soin 51-75 en C2).
  - Pichons : 10206-10213, 4276, 979. Le soin TB de Sel Marin marche avec `Heal.cs`.
- Pas de passif 2.51. Spawn 15 conforme.
- **[DOUTE VERSION]**
  - Toute l'expédition de guilde : Poissirque, Bélougaz, Montée Hydrothermale, état « Dégraissé », renvoi Pichons → Mob. Aucun de ces sorts n'existe dans le client 2.51 (mots-clés « Poissirque », « Dégraiss » : seul 990 existe).
  - « Désenvoûte » sur le site, « réduit la durée des effets de 2 » en 2.51.
- **[DOUTE]** Le sort 993 « Appel du large » (invoque un Pichon de chaque couleur) existe dans le client mais n'est attribué à personne. Je ne trouve pas son propriétaire : rien à coder.
### À faire
Rien à faire.

## Épave du Grolandais Violent (epave-du-grolandais-violent) — boss Ben le Ripate (id 2877)
### Mécanique officielle (site)
- Fancrôme (2874) : Ancre d'échine (250 terre ligne 3 PO, attire 1) ; Ancre Harton (+50 dommages alliés au CàC, 1 tour) ; Jet d'Ancre (170 eau, vole 2 PO, ligne 3-5).
- Fantimonier (2881) : Barre barre (200 eau vol de vie, ligne 4) ; Barre botage ; Barre Hikade (±4 PO, cercle 3, 3 tours).
- Fantomalamère (2875) : Bertha (invoque une Pitrouille) ; Bouhay (soin 51-70 aux alliés au CàC) ; Boulay (200 feu, +10 % CC).
- Fantômat (2880) : Boule (**état Paytank** : −30 PM et repousse de 4 quand il est frappé) ; Cédoine (200 neutre ligne 4) ; Kake (200 eau 2-10 PO, repousse 4).
- Harpirate (2878) : Hissage (200 air vol de vie, vole 100 agilité) ; Homiseur (poison par PA, 4-8 PO) ; Hure (soigne l'attaquant de la cible, 2 tours).
- Vigie pirate (2876) : Hardé (−1 000 soins, 20 % érosion, cercle 4) ; Hérissage (transposition + dégâts selon PV manquants) ; Roce (dégâts selon PV manquants, −63 PM, Pesanteur).
- Boss :
  - **Flibrute** au début du combat : **Invulnérable + Lourd infinis**.
  - Tore tue : invoque un **Hamrack** (Lourd, Invulnérable, Enraciné, 2 PM ; **Bon barde** : +3 PM quand on tente de lui retirer des PM).
  - Le Hamrack au contact de Ben lance **Gouverne ail** : Ben vulnérable 5 tours, 350 eau en zone. Le Hamrack explose au bout de 3 tours.
  - Autres sorts : Mâte l'eau (250 feu croix 4, attire 3, selon PV manquants), Mousse Haillon, Frère de la côte.
### État sur le serveur
- Conforme : `BenBrain` (Flibrute 2615 au début du combat) et `HamrackBrain` (Bon barde 2617 à l'arrivée, va au contact, Gouverne ail 2618 qui désactive l'invulnérabilité 5 tours, Gîte 682 au 4ᵉ tour).
- Sorts 2.51 de Ben :
  - 2298 Mousse Haillon et 2300 Frère de la côte (invoque l'équipage) demandent l'état Invulnérable : en 2.51, Ben ne les lance que tant qu'il est invulnérable.
  - 2299 Tore tue, 2616 Mâte l'eau.
- **[DOUTE VERSION]**
  - Frère de la côte « soigne 400 + 150 puissance » sur le site ; en 2.51 il invoque l'équipage.
  - « Tous les Hamrack explosent quand Ben devient vulnérable » : absent des données 2.51 (Gouverne ail ne tue que le lanceur).
  - Chiffres.
### À faire
Rien à faire.

## Excavation du Mansot Royal (excavation-du-mansot-royal) — boss Mansot Royal (id 2848)
### Mécanique officielle (site)
- Fu Mansot (2858) : Mansolfatare, Mansoron (Pesanteur), Mansoja (+57 dommages, 50 % soigné / dégâts ×2).
- Mamansot (2857) : Mansolex, **Mansovegarde** (ressuscite le dernier mort, état zombi), Mansoviétik.
- Mansobèse (2856) : Mansogrenu, Mansoldat (transposition + bouclier), Mansolotage.
- Shamansot (2855) : Mansorcier (malus aléatoires, cercle 2), Mansote-mouton (Affaibli), Mansovage.
- Timansot (2849) : Mansonnette, Mansotise, Mansovetage.
- Boss :
  - **Mansomure** au début du combat : invulnérable infini.
  - **Mansodah** : soigner un joueur au contact du Mansot le rend vulnérable 6 tours et frappe ~200 eau ceux qui sont au contact du soigné.
  - **Mansomon** : +1 000 PV, +dommages et +1 PM par allié mort.
  - **Mansomnambule** : poussé ou attiré, il soigne ses attaquants de 100 % des dégâts.
  - Sorts : Mansolénoïde, Mansoluble, Mansoxygène.
### État sur le serveur
- Conforme : `MansotRoyalBrain` (Mansomure 2607 → 2608 Mansodah sur déclencheur H, 2609 Mansomon sur déclencheur X ; ré-invulnérable s'il est ressuscité).
- Sorts 2.51 du boss : 2211 Mansoluble, 2212 Mansopoudrage (pose 2610 Mansomnambule sur déplacement), 2213 Mansolénoïde.
- **[DOUTE VERSION]**
  - Mansoxygène : absent du 2.51, remplacé par Mansopoudrage.
  - « 10 % de dommages finaux » de Mansomon : en 2.51, c'est +50 dommages.
  - Mansomon officiel non désenvoûtable : non vérifié en jeu.
### À faire
Rien à faire.

## Fabrique Mallefisk (fabrique-mallefisk) — boss Malléfisk (id 3652)
### Mécanique officielle (site)
- Donjon de dimension en 5 vagues (tours 6, 11, 16, 21, ou plus tôt si la vague est vidée).
- Berserkoffre (3666) : Fouetreinte (~200 terre, −4 PA) ; Langueur ; Saut de Joie (transposition + invulnérable 1 tour, relance 3).
- Boursoin (3663) : Pondération, Renflouage (soin), **Tact** (effet « Tique » : soigne autour de lui quand il est frappé).
- Mimikado (3664) : Mimirage, Mimissile, Pou.
- Précieux (3667) : Morsurprise, Bondulations, Haleine de Coffre.
- Trésantène (3665) : Éruption (érosion 20 %), Déferlante (vole 50 force), Décompression (+100 puissance, +1 PM).
- Boss :
  - **Ofranor** au début du combat, non désenvoûtable : **Intérêts**, à chaque coup reçu Malléfisk **et son attaquant** gagnent +100 puissance (infini). Sous 50 % de PV, il gagne 500 de bouclier par ligne de dégâts.
  - Ka Dabor (~150 air, −2 PM) ; Chté Hu ! (coopération).
### État sur le serveur
- `MallefiskBrain` (`PassifsDonjonsBrains.cs`) pose bien 4505 Ofranor. Vagues en base (`monsters_spawns_dungeons_waves`, spawn 180) et tours 6/11/16/21 par défaut (`Fight.WaveTurns`) : conforme.
- Données 2.51 de 4504 Intérêts :
  - 1040 bouclier 500 sur `C,V50` (PV ≤ 50 %).
  - 138 +100 puissance sur `C`.
  - 138 +100 puissance sur `a,A,O` en zone P1.
- **Anomalie** : le sort déclenché est lancé sur la case de Malléfisk (`CastSpellEffect.DefaultBuffTrigger`). La zone P1 ne contient que Malléfisk, et le critère `O` (dernier attaquant) exclut Malléfisk : **l'attaquant ne gagne jamais ses +100 puissance**. C'est le cas de 47 sorts du client 2.51 qui visent « O » en zone P1 (Imposture, Bump, Planque, Triche, Fuite mortelle…). Officiellement, « O » vise le dernier attaquant où qu'il soit.
- Le sort 4508 Nonoube Noharnak est en 2.51 mais pas sur le site. C'est normal.
- **[DOUTE VERSION]** « 500 PB par ligne » : le moteur ne déclenche qu'une fois par séquence (voir l'en-tête).
### À faire
1. **[IMPORTANT]** Faire viser le dernier attaquant par le critère `O`, quelle que soit la zone.
   - Où : `game/effects/handlers/Spells/SpellEffectHandler.cs`, `GetAffectedActors()`, juste après le bloc SELF_ONLY.
   - Code à ajouter :
     ```csharp
     // MYSTIC : critère « O » (dernier attaquant) = la cible est le dernier attaquant du lanceur, où qu'il soit (Intérêts de
     // Malléfisk : +100 puissance à l'attaquant ; 47 sorts 2.51 visent « O » en zone P1)
     var dernier = Caster.LastAttacker;
     if (dernier != null && dernier.IsAlive() && !AffectedCells.Contains(dernier.Cell) &&
         Targets.Any(x => x is LastAttackerCriterion l && l.Required))
         AffectedCells.Add(dernier.Cell);
     ```
     Ajouter `using Stump.Server.WorldServer.Game.Effects.Handlers.Spells.Targets;` s'il manque. `LastAttackerCriterion.Required` est déjà public. `LastAttacker` est renseigné avant les déclencheurs « D » (`FightActor.InflictDamage`, ligne ~971).
   - Effet de bord voulu : les 46 autres sorts « O » se mettent aussi à fonctionner (comportement officiel).
   - Variante limitée à Malléfisk si l'utilisateur préfère : dans `MallefiskBrain`, s'abonner à `Fighter.DamageInflicted` (ou `BuffAdded` du sort 4504) et poser à `damage.Source` un `StatBuff` `DamageBonusPercent` +100, durée −1000, sort 4504.

## Donjon des Firefoux (donjon-des-firefoux) — boss Péki Péki (id 605)
### Mécanique officielle (site)
- Salles dans l'ordre que l'on veut, puis le Péki avec les 6 reliques. Salle 1 : Touchparak à mettre sur la dalle.
- Zilla (599) : **95 % de résistance partout sauf un élément tiré au hasard à −95 %** ; soigne ses alliés ; donne la relique.
- Yokai Firefoux (527) : terre au CàC, +puissance à chaque coup reçu.
- Maho Firefoux (529) : **50 % partout sauf un élément au hasard** ; boost vitalité et puissance ; terre au CàC.
- Soryo Firefoux (528) : **70 % partout sauf un élément au hasard** ; soin de zone ; CàC terre avec −PM et −caractéristiques.
- Kitsou Nufeu : invoque un double, invisible, feu à distance.
- Léopardo : se transforme.
- Péki Péki :
  - 100 % feu, ~40 % ailleurs, 15 PM.
  - Pousse 2 fois par tour (dégâts de poussée).
  - Poison 100-200 en début de tour.
  - Transposition qui le rend invisible.
### État sur le serveur
- Le Péki (spawn 256) a 1280 Transpékisation (transpo, invisible, +agilité), 4587 Kroutage (repousse 6), 4588 Piaillement (air TB 5 tours, C63), 4589 Pékisation (demande Pesanteur). Pas de passif 2.51 : conforme.
- **Trois passifs 2.51 jamais lancés** (sorts du client attribués à personne, aucun cerveau) :
  - **4574 Mot Deste** (Zilla, comme ses sorts « Mot Rovach / Mot Râle / Mot Bilisation ») : −190 % dans **un** élément tiré au hasard (5 effets `random 20`, infini). Zilla a 95 % partout en base (`monsters_grades`), ce qui donne −95 % comme sur le site. Aujourd'hui, Zilla a 95 % partout sans faiblesse.
  - **4585 Culture** (Maho) : un des 5 états Pandawushae/ak/fe/te/ne + −55 % dans l'élément correspondant (groupes aléatoires). Ses sorts 4580-4584 « Révolution … » **demandent** ces états. Aujourd'hui, le Maho ne peut lancer que Métamourphose et garde 50 % partout.
  - **4577 Fuyumi** (très probablement Soryo) : −80 % dans un élément au hasard (`random 20`), ce qui donne −10 % sur une base de 70 %. Ça colle à la description du Soryo ; aucun autre monstre du donjon n'a 70 %. **[DOUTE léger]** sur le propriétaire.
- Effet 130 et autres : rien.
- **[DOUTE VERSION]**
  - PM du Péki (12 en base, 15 sur le site), poison 100-200 (2.51 : Piaillement 20 air en début de tour, 5 tours).
  - « Le Péki passe son tour au tour 2 » : absent des données.
- **[DOUTE]** 986 « Esprit du Péki Garou » (±1-6 PO en cercle 5, −401-600 intelligence) existe en 2.51 sans propriétaire et sans mention sur le site : rien à coder.
### À faire
1. **[IMPORTANT]** Cerveau de Zilla qui pose Mot Deste.
   - Où : `AI/Fights/Brain/Custom/Boss/PassifsDonjonsBrains.cs`.
   - Code : `[BrainIdentifier(599)] public class ZillaBrain : BrainPassif { public ZillaBrain(AIFighter f) : base(f, 4574) { } }`
2. **[IMPORTANT]** Cerveau du Maho Firefoux qui pose Culture : `[BrainIdentifier(529)] public class MahoFirefouxBrain : BrainPassif { public MahoFirefouxBrain(AIFighter f) : base(f, 4585) { } }`
3. **[IMPORTANT]** Cerveau du Soryo Firefoux qui pose Fuyumi : `[BrainIdentifier(528)] public class SoryoFirefouxBrain : BrainPassif { public SoryoFirefouxBrain(AIFighter f) : base(f, 4577) { } }`
   - Note : le Maho et le Soryo apparaissent aussi en Pandala (sous-zones 107 et 116). Le passif s'y appliquera aussi, comme pour un passif officiel.

## Forgefroide de Missiz Frizz (forgefroide-de-missiz-frizz) — boss Missiz Frizz (id 3391)
### Mécanique officielle (site)
- Monstres de la salle du boss : Stalak 3386, Karkanik 3387, Verglasseur 3389. Frimar 3390 et Ventrublion 3385 n'y sont qu'à 7-8 joueurs.
- Chacun a un sort « actif 2 tours sur 3 » :
  - Frimar : **Glacéré** (+4 PM et +500 dommages s'il est déplacé).
  - Karkanik : Pesanteur et −100 fuite en cercle 6 s'il est frappé à distance.
  - Stalak : renvoi de dégâts et 20 % d'érosion en cercle 2.
  - Ventrublion : renvoi en croix et +500 dommages.
  - Verglasseur : −25 % / −75 % de PV en cercle 5.
  - Plus leurs attaques.
- Boss :
  - **Tout le monde Lourd et Invulnérable.**
  - À chaque début de tour d'un joueur, **congères** sur les 4 cases adjacentes. Y marcher : soin 15 %, poussé de 1, pousse de 1 tout ce qui est en croix 6 et diagonale 3, révèle les invisibles.
  - **Un monstre poussé perd son invulnérabilité 1 tour.**
  - **Monogel** : toute entité poussée déclenche le même effet.
  - **Glace de pic** : Missiz frappée attire à elle les joueurs en ligne et diagonale et leur met l'état **Surgelé**.
  - Dommages de poussée : +20 % d'érosion infinie et ~800 neutre supplémentaires.
  - Sorts : Cristallisation (Intacleur aux monstres, +3 PM sur tentative de retrait PM, à partir du tour 2, relance 3) ; Sang froid (800 feu, +800 eau si Surgelé) ; Glace trop physique.
### État sur le serveur
- `MissizFrizzBrain` pose 3517 **Congère**. Les données 2.51 couvrent tout :
  - état 935 « Invulnérabilité glaciale » (Invulnerable = 1 en `spells_states`) aux monstres ;
  - 3518 Glace de pic (attire 6 + Surgelé) sur « D » ;
  - 11943 Monogel sur « P », chaînage par les états 949/950 et 3520 Antigel : pousse 1 en X6 et +3, soin 15 %, `952` désactive 935 1 tour ;
  - érosion 20 % et 100 neutre sur « PD » ;
  - 3521 Vergladiateur sur « TB » des joueurs : piège 3522 autour, révèle les invisibles ;
  - état Lourd aux monstres.
- Effets, déclencheurs et critères (`*e950`, `*E949`, `f3397…`) sont tous gérés d'après l'outil et le code. Sorts des monstres et de Missiz conformes au 2.51. Spawn 374 conforme.
- **[DOUTE VERSION]** 800 de dommages de poussée en plus (2.51 : 100 neutre), Surgelé « en ligne et diagonale » (2.51 : zone `*7`).
- **Non vérifiable sans test en jeu** : forme exacte du piège (zone `Q1` = 4 cases adjacentes ?) et sens de poussée du piège.
### À faire
Rien à faire. Un test en jeu de la chaîne congère → Antigel → monstre vulnérable est conseillé.

## Donjon des Forgerons (donjon-des-forgerons) — boss Coffre des Forgerons (id 252)
### Mécanique officielle (site)
- Bandit du clan des Roublards (155) : Lancer de Hachette (70 terre, −1 PM, ligne 9 PO) ; Persécution (130 neutre CàC).
- Boulanger Sombre (153) : Éventration (120 neutre CàC) ; Lancer de Pain (45 feu, −1 PM, 8 PO).
- Forgeron Sombre (119) : Coud'marto (90 neutre croix 1) ; Embrochement (30 neutre, repousse 5) ; Protection Magique (−10 dommages, +20 esquive PM, 2 tours, relance 4).
- Mineur Sombre (118) : Coup de Pioche (130) ; Escroquerie (100 air) ; Gaucherie de Masse (−2 PA 3 tours, cercle 8, relance 7) ; Rage (+6-20 dommages 3 tours).
- Boss : Avidité (90 terre vol de vie, vole 4 PA, CàC) ; Tchaiste (30 % de ses PV manquants en terre en **cercle inversé 4**, +30 dommages à ses alliés). IA fuyarde. **Pas d'invulnérabilité.**
- Expédition : Coffre invulnérable, OS…
### État sur le serveur
- Conforme : 2979 Avidité (vole 4 PA, 21-30 terre) et 2978 Tchaiste (276 « % PV perdus » 50 en I4, +30 dommages). Pas de passif 2.51. Sorts des monstres conformes. Spawn 60 conforme.
- Persécution (238) du Bandit contient l'effet **130 Effect_StealKamas, non géré** (vol de kamas, sans enjeu en combat).
- **[DOUTE VERSION]** Toute l'expédition de guilde, chiffres de Tchaiste (2.51 : 50 %).
### À faire
1. **[MINEUR]** Effet 130 (vol de kamas) du sort 238. Soit ne rien faire (aucun impact sur le combat ; les dégâts 100 du même sort partent), soit ajouter `[EffectHandler(EffectsEnum.Effect_StealKamas)]` à un gestionnaire vide qui renvoie `true` pour faire taire l'effet inconnu. **À ne faire que si les journaux montrent une erreur.**

## Temple du Grand Ougah (temple-du-grand-ougah) — boss Ougah (id 1159)
### Mécanique officielle (site)
- Champ à Gnons (1158) : poison neutre 11 PO, −3 PO ; invoque un Champa Explosif (feu cercle 3, +200 dommages par tour) ; +PM aux alliés frappés.
- Champaknyde (1154) : tous éléments au CàC, réduit les soins reçus.
- Champbis (1157) : 25 % de faiblesse à 8 PO, poison feu, +50 dommages, +5 PO à un allié.
- Champ Champ (59) : eau CàC, poison terre 3 tours.
- Champmane (1156, « Champagne » sur le site) : poison −agilité, soin, renvoi.
- Champodonte (1155) : 10 % de ses PV au CàC, −5 PA, poison air, +dommages aux alliés frappés.
- Tromperelle (1153) : poison air par PA, eau CàC, −dommages aux ennemis frappés.
- Salle 3 : équipe séparée, dalles.
- Boss :
  - Bizarrerie : +50 puissance par coup subi à la cible, +300 aux monstres en croix 2.
  - Opiniâtreté : 600 feu + 600 terre croix 1, −20 % résistance air, poison air.
  - Sirop Spore : 80 neutre par PA, 4 tours, à partir du tour 3.
  - **Spore Hanchambre** : +50 % CC par coup subi ; **soin complet sur UN déclencheur tiré au sort** (dommages, retrait PA ou retrait PM), à partir du tour 2.
  - Les Coprins d'abord : invoque un monstre au hasard à partir du tour 2.
### État sur le serveur
- Conforme 2.51 :
  - 1162 Bizarrerie.
  - 1163 Acharnement (= Opiniâtreté).
  - 1263 Sirop Spore (initialCooldown 2).
  - 1264 Spore Hanchambre : 115 sur « D » + un soin 5000 tiré au sort parmi APA / D / MPA (`random 33`), initialCooldown 1. Le soin déclenché marche depuis la correction `Heal.cs`.
  - 1265 Coprins (6 invocations `random 16`, initialCooldown 1).
- Pas de passif 2.51. Spawn 319 conforme.
- **[DOUTE VERSION]** Chiffres (600/600 → 41-60 en 2.51). « Jusqu'à 6 invocations » : limite non vérifiée.
### À faire
Rien à faire.

## Galerie du Phossile (galerie-du-phossile) — boss Phossile (id 3651)
### Mécanique officielle (site)
- Dimension en 5 vagues (tours 6, 11, 16, 21, ou plus tôt) ; une vague qui arrive est invulnérable 1 tour.
- Mère Veilleuse (3662) : Éphort de Guerre (300 eau, Pesanteur) ; **Phouille** (état Phobie : 350 terre vol de vie aux ennemis au contact en début de tour) ; **Phormage** (effet propre à chaque type de Phorreur).
- Métaphorreur (3659) : Phormol ; Fracturgescence ; Dispersion (−dommages à distance, à partir du tour 3).
- Père Phorreur (3660) : Tunnellipse ; Perphorrage ; Phormation (**Transfert** : soin 10 % quand il est frappé au CàC).
- Phorrêveur (3658) : Flaqueduc (glyphe +2 PA) ; Onde répulsive (glyphe soin) ; Appel à l'aide (glyphe −50 % de dommages).
- Phozami (3661) : Défense ; Phorrage ; Râle (+1 PM par coup à distance).
- Boss :
  - Sorts : Épicentre, Bain de lave, Phorce, Roc Phorreur.
  - **Phorreur de Gloire** : **Abîme** = OS de tous les joueurs à 3 PO ou moins **au début de son tour aux tours 5, 9, 13…** ; **Glyphiphi** = glyphe rouge sous toute entité poussée ou attirée, 3 tours, +30 % de dommages subis.
  - **Centrage** (version actuelle) : en fin de tour d'un joueur, +1 PM et bouclier aux monstres à plus de 8 PO.
### État sur le serveur
- `PhossileBrain` pose 4497 Phorreur de Gloire (données 2.51) :
  - 793 → 4498 Abîme avec **delay 4**, qui se relance lui-même avec delay 4 ;
  - 792 → 4510 sur tous les ennemis, qui leur pose 4496 Glyphiphi (sur « P|MA », glyphe-aura 4492 ×130 % de dommages subis) et 4495 Centrage (sur « TE »).
- Version 2.51 de Centrage : +300 PV aux monstres à 9 PO ou plus et aux alliés à 1-4 PO, et **−1 PM** aux monstres à 9 PO ou plus. Le site confirme que l'ancienne version retirait des PM.
- Vagues (spawn 277) conformes.
- **Anomalie de calendrier d'Abîme** :
  - Le passif est lancé au `FightStarted` (avant le tour 1). Le délai est décompté **au début de chaque tour du Phossile** (`Fight.OnTurnStarted` → `DecrementAllCastedBuffsDuration` → `Buff.DecrementDuration`) : 4 → 3 au tour 1, 2 au tour 2, 1 au tour 3, 0 au tour 4.
  - **Abîme part donc au début du tour 4** (puis 8, 12…) au lieu de 5, 9, 13 : un tour trop tôt, et les joueurs ne s'y attendent pas.
  - L'écart vient seulement du lancement « avant le tour 1 » ; la relance (+4 tours) est juste.
- **[DOUTE VERSION]** « Centrage : +1 PM et bouclier », « Glyphiphi aussi sur les monstres » (2.51 : seulement les ennemis du Phossile), tous les chiffres, l'invulnérabilité de vague 1 tour (rien dans les données), toute l'expédition.
### À faire
1. **[CRITIQUE]** Décaler le premier Abîme au tour 5.
   - Où : `AI/Fights/Brain/Custom/Boss/PassifsDonjonsBrains.cs`. Remplacer `PhossileBrain` par :
     ```csharp
     /// <summary>Phossile : Phorreur de Gloire. MYSTIC : le passif est posé avant le tour 1, alors que le délai (4) d'Abîme se
     /// décompte au début de chaque tour du Phossile -> Abîme partait au tour 4 ; officiel : tours 5, 9, 13…</summary>
     [BrainIdentifier(3651)]
     public class PhossileBrain : BrainPassif
     {
         public PhossileBrain(AIFighter f) : base(f, 4497)
         {
             if (f.Fight.State != FightState.Fighting)
                 f.Fight.FightStarted += Decaler;
         }

         void Decaler(IFight fight)
         {
             fight.FightStarted -= Decaler;
             foreach (var buff in Fighter.GetBuffs(x => x.Spell.Id == 4498 && x.Delay > 0).ToArray())
             {
                 buff.Delay++;
                 fight.UpdateBuff(buff, false);
             }
         }
     }
     ```
   - Ajouter `using System.Linq;` en tête du fichier.
   - L'abonnement est fait après celui de `SortPassif.Poser` (constructeur de base), donc il passe après la pose du passif.
   - À valider en jeu : premier OS au tour 5.
   - Si l'utilisateur constate que d'autres passifs à `delay` posés au début du combat ont le même décalage, la correction générale serait dans `SortPassif.Poser`. Je ne le propose pas sans vérification.

## Gelaxième Dimension (gelaxieme-dimension) — boss les 4 Gelées Royales (ids 58 Bleue, 85 Menthe, 86 Fraise, 430 Citron)
### Mécanique officielle (site)
- Accès par la Multygely, 4 groupes par salle, flaques de couleur, 1 ou 4 boss.
- Gelées Bleuet, Citron, Fraise, Menthe : Os (−caractéristique), Gelpikes, Isométrie (+3 PA), Fixation Béton (−24 dommages subis), Gélifiant, Pik-assaut (+2 PM), et surtout **Tartinade** (téléportation + glyphe de couleur : dégâts aux joueurs, soin à la gelée de même couleur, −15 % de résistance aux autres couleurs).
- Gelées Royales : −10 % dans leur élément et 70 % ailleurs ; invoquent une gelée au tour 1.
### État sur le serveur
- Le serveur a le donjon « Gelées » (spawns 1-5, salle du boss 94 avec les 4 Royales) avec les sorts 2.51 de l'ancienne version :
  - Bleu/Menthe/Fraise/Citron Os ;
  - 321 Gelpikes ;
  - 217 Gel Protecteur ;
  - 241 Reconstitution ;
  - 8225 Mot de Jeunesse ;
  - invocations 218/225/226/663.
- Résistances en base (70 / −10) conformes.
- **[DOUTE VERSION]** Tartinade, Isométrie, Fixation Béton, Gélifiant, Pik-assaut, nouvelles valeurs des Os, Multygely et système des flaques. Ni « Tartinade » ni « Isométrie » n'existe dans le client 2.51 : c'est la refonte d'après la 2.51.
### À faire
Rien à faire.

## Grotte de Kanigroula (grotte-de-kanigroula) — boss Kanigroula (id 3556)
### Mécanique officielle (site)
- Kaniblou (3561) : feu à distance, vole 50 intelligence, **fait passer le tour si la cible est poussée** après Griffes enflammées, soin.
- Kanihilan (3557) : rush 6 PO + air en zone, neutre −PM, Grrr (renvoi de sort 1 tour, relance 3).
- Orfélin (3562) : terre CàC, vole force, 20 % d'érosion, Boubou (−220 dommages au CàC, tous les 3 tours).
- Monstres hors salle du boss : Alhyène, Félygiène (3559), Panthègros (3560).
- Boss :
  - Chachyène cinglante (450 eau croix 1, −20 % eau).
  - Rugissement matriarcal (5 % de ses PV, ±2 PM et fuite/tacle, cercle 3).
  - Chachyène dominante (tue une invocation, à partir du tour 3).
  - Motivation captivante (+3 PA et bonus de sorts par monstre).
  - Passifs au début du combat : **Odorat bestial** (révèle les invisibles à chaque tour), **Sauvagerie** (à sa mort : +2 PM, +200 puissance, +10 % résistances aux monstres), **Mort méphitique** (à chaque mort de monstre : −10 % résistances, −50 puissance aux joueurs), **Phéromones sauvages** (en début et fin de tour d'un joueur, +10 % CC et +100 puissance aux ennemis en cercle 2).
### État sur le serveur
- Conforme : `KanigroulaBrain` pose 4093 Phéromones bestiales, qui contient en 2.51 exactement 4094 (TB), 4095 (X), 4096 (X sur les alliés), 4097 (TB et TE sur les joueurs).
- Le déclencheur « X » part bien à la mort (`FightActor`, `TriggerBuffs(OnDeath)` avant le retrait des buffs) et le sort est lancé avec `Force` même mort.
- Sorts 4441, 4092, 4091, 4090 et sorts des monstres conformes. Spawn 293 conforme.
- **[DOUTE VERSION]** Chiffres, expéditions.
### À faire
Rien à faire.

## Goulet du Rasboul (goulet-du-rasboul) — boss Silf le Rasboul Majeur (id 1071)
### Mécanique officielle (site)
- Monstres :
  - Kilibriss (1070) : saute ; s'il frappe au CàC, la cible passe son prochain tour.
  - Craqueleur Poli (1025) : CàC.
  - Craqueboule Poli (1026) : CàC.
  - Mufafah (1068) : CàC.
  - Kido (1069) : CàC.
  - Bitouf des Plaines (1019) : beaucoup de PM, frappe et fuit.
- Boss :
  - **200 % de résistance partout.** Coopère en frappant (Modération).
  - Invoque jusqu'à 2 Rasboul Mineurs par tour (75 % partout sauf un élément au hasard à −75 % ; soignent tous les PV d'un allié).
  - Chaque retrait de PA lui donne des PA. **À 14 PA ou plus, il lance Hololole** (2 tours) : chaque coup reçu dans un élément lui retire 50 % dans cet élément et lui ajoute 50 % dans les autres.
### État sur le serveur
- Conforme 2.51 :
  - 1038 Rasage (+2 PA sur tentative de retrait PA, 4 tours).
  - 1039 Hololole (PA 14 ; −50 % sur DA/DW/DF/DE, +50 % aux autres, +900 % puissance et +500 % neutre aux alliés).
  - 1041 Modération (transposition).
  - 1053 Recrutement : 5 grades `random 20`, 2 fois par tour. Le Rasboul Mineur 1084 a −75 % dans un élément différent selon son grade, donc l'élément est bien aléatoire.
  - 1054 Bisouille (soin 10 000).
- Kilibriss 1036 (passe le tour sur « D »). Pas de passif 2.51. Spawn 196 conforme.
- **Non vérifiable sans test** : que l'IA générique lance bien Hololole dès qu'elle a 14 PA (sort de buff à 14 PA, sans cible ennemie).
- **[DOUTE VERSION]** Hololole « 2 tours » : en 2.51, résistances 2 tours et puissance 3 tours, c'est cohérent.
### À faire
Rien à faire. Test en jeu conseillé : monter Silf à 14 PA et vérifier qu'il lance Hololole. Sinon, un cerveau qui force `CastSpell(1039)` quand `Fighter.AP >= 14` serait la correction.

---

## Récapitulatif

| Donjon | CRITIQUE | IMPORTANT | MINEUR |
|---|---|---|---|
| Domaine Ancestral | 0 | 0 | 0 |
| Château Ensablé | 0 | 0 | 0 |
| Épave du Grolandais Violent | 0 | 0 | 0 |
| Excavation du Mansot Royal | 0 | 0 | 0 |
| Fabrique Mallefisk | 0 | 1 | 0 |
| Donjon des Firefoux | 0 | 3 | 0 |
| Forgefroide de Missiz Frizz | 0 | 0 | 0 |
| Donjon des Forgerons | 0 | 0 | 1 |
| Temple du Grand Ougah | 0 | 0 | 0 |
| Galerie du Phossile | 1 | 0 | 0 |
| Gelaxième Dimension | 0 | 0 | 0 |
| Grotte de Kanigroula | 0 | 0 | 0 |
| Goulet du Rasboul | 0 | 0 | 0 |
| **Total** | **1** | **4** | **1** |
