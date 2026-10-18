# Lot 1 : audit des mécaniques (dofuspourlesnoobs → Mystic 2.51)

Lecture seule : rien n'a été modifié. Sources : pages du site (texte), `analyse_mecaniques.py` (données client 2.51 + base),
base `world`, cerveaux `AI/Fights/Brain/Custom/**`, gestionnaires d'effets.

**Règle version appliquée.** Le site décrit la version ACTUELLE (dégâts ×5 à ×10 par rapport à la 2.51, sorts refaits, expéditions de
guilde). Toutes les différences de chiffres sont classées **[DOUTE VERSION]** et ne sont pas à coder. Seuls les sorts / passifs qui
existent dans le client 2.51 sont proposés. Les expéditions de guilde (Audace / Bravoure) n'existent pas en 2.51 : ignorées.
Glourséleste et Korriandre (jugés parfaits) : aucune modification proposée.

Points généraux trouvés pendant l'audit (ils servent dans plusieurs sections) :
- **G1. Effets 1021 / 1022 = ACTION_CHARACTER_PUSH_FORCE / PULL_FORCE** (`ActionIds.as` du client). Ce sont des poussées « forcées »
  (elles déplacent même un Indéplaçable). Dans `game/effects/handlers/Spells/Move/Push.cs`, elles passent par
  `if (!actor.CanBePushed()) continue;` : une cible Indéplaçable n'est jamais bougée. 43 sorts du client utilisent 1021/1022 (des
  sorts de classe aussi, ex. Huppermage) : je propose une correction limitée à Mer Cure (Merkator), voir plus bas.
- **G2. Un sous-sort qui n'existe pas fait planter le lancer.** `CastSpellEffect.InternalApply` (`game/effects/handlers/Spells/Others/CastSpell.cs`)
  fait `new Spell(Dice.DiceNum, …)`. Le constructeur `Spell(int, short)` fait `Template.TypeId` sans tester null → NullReferenceException
  si le sort n'existe pas (aucun try/catch sur ce chemin). Cas trouvé : Tourbillon mortel (Arapex) → effet 2794 vers le sort 5612,
  absent du client 2.51 ET de `spells_templates`.
- **G3. Niveau de sort = grade du monstre alors que les niveaux ne sont pas des grades.** Chez certains monstres, les niveaux d'un sort
  sont des variantes (couleur / élément / sous-sort déclenché) et pas une montée en puissance. `monsters_spells.Level` a été rempli avec
  le grade → mauvais sort. Cas trouvés : Maître Corbac (Carapace d'Ailes), Blops / Biblops (élément). Vérifié par script sur tous les
  monstres du lot (les Blops Royaux, Tonneaux et Bworks élémentaires sont, eux, cohérents).
- **G4. Déclencheur « DCAC »** : mappé sur `OnDamagedInCloseRange` (mêlée) dans `TriggerBuff.cs`. Le site emploie « CàC » au sens
  « arme » pour Merkator (« frappez un allié avec une arme »). **[DOUTE]** : DCAC voudrait peut-être dire « dommages d'arme ». À ne pas
  toucher : Pollen (Apériglours, Glourséleste) et Spore Taporte (Abrazif, Korriandre) l'utilisent, et ces donjons sont jugés parfaits.

---

## Aquadôme de Merkator (aquadocircme-de-merkator) — boss Merkator (id 3534)
### Mécanique officielle (site)
- **Pikoleur (3535)** : Perforage : air, 30 % (35 % crit) des PV manquants du lanceur, zone cercle inversé 6, 1/tour. Par ici : échange
  de place (en ligne ≤ 6 PO), attire de 9 tous les ennemis en croix 10, Pesanteur + −4 PM esquivables 1 tour ; lui **Indéplaçable** 1 tour ;
  au tour suivant 1300 feu cercle 4 sur les ennemis, 50 % sur ses alliés. À partir du tour 2, relance 2.
- **Harpo (3538)** : Campeur et sans reproches : repousse de 4 en croix 2, état **Cessez le feu** 1 tour (soin 10 % PV à chaque dommage
  de poussée subi) ; au tour suivant −10 PM, +1000 dommages de poussée, Moule Chote +210 (à partir du tour 2, relance 2).
  Moule Chote : 550 eau, −3 PO 2 tours, 5-12 PO, 2/tour. Tireur Franc : 550 terre vol de vie, repousse 1, en ligne 4-6 PO, 2/tour.
- **Krabouilleur (3542)** : Flou de mer : état **Pinception** 2 tours (frappé à distance → l'attaquant revient à sa case de début de
  tour et devient indéplaçable 1 tour ; les entités à exactement 4 PO subissent 100 % des dégâts). Relance 3. Pince d'or : 550 terre vol
  de vie, Pesanteur 1 tour, ≤ 3 PO. Pincendie : 550 feu, attire de 3, en ligne 2-5 PO.
- **Eskoglyphe (3543)** : Prédateur de gloire : +2 PM, état **Filouteriposte** 2 tours (frappé à distance → +1 PM et +100 dommages aux
  ennemis à plus de 8 PO), relance 3. Glycol Roulé : 400 feu vol de vie croix 6 + glyphe 3 tours (tue les joueurs qui y finissent ;
  420 neutre aux monstres). Maraude à la Joie : 550 air vol de vie + 10 % (12 %) PV manquants en terre, ≤ 3 PO.
- **Cyclophandre (3544)** : Apnée Crochue : 750 eau + 30 % érosion 1 tour, mêlée, 2/tour. Gras de marée : attire de 4 puis repousse de
  4, 20 % des PV manquants en feu, en ligne ≤ 6 PO. Relou de mer : +1000 dommages de poussée 2 tours + état **Cycloporte** (frappé en
  mêlée → boucliers 1500 aux alliés au contact / à 3 / à 5 PO, 500 aux ennemis).
- **Merkator** : **Mer Kantile** (passif tout le combat) : −50 % dégâts subis à distance, +2 % dommages finaux par coup à distance subi,
  tous les monstres **Indéplaçables**. **Mer Veille** : tenter de retirer des PM à un monstre → ~500 feu vol de vie au lanceur.
  **Mer Cure** : un monstre frappé en mêlée repousse de 3 ses alliés en croix 3 (malgré Indéplaçable) et les joueurs en diagonale 3 ;
  un joueur frappé par l'arme d'un allié fait l'inverse. **Marée Descendante** (via Baphe Thysca, tours impairs à partir du 3) : attire
  en ligne ≤ 12 PO à sa mêlée et −4 PM. Baphe Thysca : Sondage de Bronze +30 par coup subi (×5). Bouche-à-Bouche : ressuscite le
  dernier allié mort à 50 % (à partir du tour 7, relance 5). Sondage de Bronze 950 terre ligne 5 ; Torpillage de glace 1100 eau,
  −50 % vitalité, Pesanteur. Sur le site, **pas d'invulnérabilité** en combat normal (seulement en expédition « Bravoure »).
### État sur le serveur
- Monstres et sorts en base = sorts du client 2.51 (aucun manquant). Outil : aucun effet / déclencheur non géré.
- Cerveau `MerkatorBrain` (`FrigostOtomaiBrains.cs`) : pose Mer Kantile 4009 + état Invulnérable permanent. Conforme à la 2.51 :
  dans 4009, Mer Cenaire (4006, au début du tour de chaque monstre) donne **Friture** aux monstres collés et **désactive
  l'invulnérabilité** de Merkator (952, état 56) si le monstre qui commence son tour a Friture et que Merkator est collé à lui ; Marée
  Descendante est liée à la fin d'invulnérabilité (EOFF56). Le site (version actuelle, sans invulnérabilité ni Friture) = **[DOUTE VERSION]**.
- **Bug (G1)** : en 2.51, le seul moyen pour les joueurs de rapprocher des monstres Indéplaçables (pour la Friture) est Mer Cure, faite
  d'effets 1021/1022 (poussées forcées). Le serveur les bloque sur les Indéplaçables → Mer Cure ne bouge jamais les monstres. Merkator
  ne devient vulnérable que si l'IA colle elle-même deux monstres.
- À vérifier en jeu : Mer Cenaire contient aussi `141 Kill cible A,*f3534 zone Q1` (un joueur collé à un monstre autre que Merkator au
  début du tour de ce monstre meurt). C'est la donnée 2.51 ; le site actuel n'en parle pas. Si c'est jugé trop dur, c'est une décision
  utilisateur, pas un bug.
- Chiffres (dégâts 121-140 en 2.51 contre 1100 sur le site, relance Campeur 3 contre 2, etc.) : **[DOUTE VERSION]**.
### À faire (précis, prêt à coder)
1. [CRITIQUE] Mer Cure doit déplacer les Indéplaçables. `game/effects/handlers/Spells/Move/Push.cs`, `InternalApply`, remplacer
   `if (!actor.CanBePushed()) continue;` par
   `if (!actor.CanBePushed() && !((Effect.EffectId == EffectsEnum.Effect_1021 || Effect.EffectId == EffectsEnum.Effect_1022) && Spell.Id == 4007)) continue;`
   (limité à Mer Cure 4007 pour ne pas changer les 42 autres sorts 1021/1022 sans test ; à généraliser seulement après décision).
   Test : frapper au corps à corps un Krabouilleur collé (croix 3) à Merkator → Merkator doit reculer de 3.

## Antichambre des Gloursons (antichambre-des-gloursons) — boss Glourséleste (id 2864)
### Mécanique officielle (site)
- **Glourmand (2987)** : se donne +1000 dommages de poussée à chaque coup reçu ; feu au CàC + repousse 2.
- **Boulglours (2862)** : retirer des PM à un monstre → +PM à ses alliés ; frappe à distance et au CàC.
- **Apériglours (2861)** : attaqué au CàC → soigne les monstres autour ; soin au CàC ; feu au CàC + −500 soins.
- **Gloursaya (2863)** : fuyard, vole des PM à distance, +100 dommages par coup reçu, vol de vie au CàC.
- **Glouragan (2988)** : sous son sort, attire tout ce qui est en ligne quand il est frappé ; bâton qui touche aussi ses alliés.
- **Meliglours (2989)** : sacrifie ses alliés, +1000 PV aux alliés, vol de vie chance.
- **Glourséleste** : **invulnérable** ; frappé au CàC → vulnérable pour le tour de ce joueur seulement, et repousse de 4 tout ce qui est
  en ligne à 4 PO ; tout dommage de poussée tue ; retrait PM / PO ou déplacement → état **Résuglours** → ressuscite des alliés ;
  **Taclours** (esquive ratée = mort, avec les alliés au contact) ; frappe 10 % des PV manquants au CàC ; vole ~700-800 PV
  (2 fois par tour, 1 fois par cible).
### État sur le serveur
- Conforme : `GlourselesteBrain` pose Gloursongeur 2511 (Gloursomptueux = invulnérable au début du tour de chaque joueur, Hydromel =
  repousse 4 + perte d'invulnérabilité au corps à corps, Résuglours sur R|MPA|M, Petit Glours Brun) + mort sur dommages de poussée.
  Rattrapage / Taclours (CT), Gloursondulation 2/tour 1/cible : conformes. Tous les sorts des monstres sont en base.
- Différences : « 1000 dommages de poussée à chaque coup reçu » du Glourmand = en 2.51 seulement sur coup à distance (décl. DR) ;
  chiffres : **[DOUTE VERSION]**. Pollen (Apériglours) utilise DCAC (voir G4) : ne pas toucher.
### À faire (précis, prêt à coder)
Rien à faire (donjon jugé parfait).

## Antre de la Reine Nyée (antre-de-la-reine-nyeacutee) — boss Reine Nyée (id 3996)
### Mécanique officielle (site)
- **Arapex (3991)** : Estocade : terre, avance de 2, état **Proie** 2 tours, +5 % crit 3 tours, dégâts augmentés si déjà Proie
  (en ligne 2-3 PO, 2/tour, 1/cible). Exécution : tue une cible en **Cocon** (≤ 2 PO sans LdV, relance 3). Tourbillon mortel : terre
  cercle 2, attire de 1, +10 (15) dégâts au tour suivant.
- **Dardalaine (3992)** : Dards paralysants : neutre + poison 1 tour, −15 fuite, état **Empoisonné**, cercle 2. **Kissifrotsipik
  (passif)** : frappée en mêlée → neutre à tous ses voisins (croix 1), doublé sur les Empoisonnés. Sécrétion acide : neutre + poison,
  −200 puissance, Empoisonné, ligne 4, ≤ 7 PO sans LdV.
- **Gargantûl (3995)** : Blindage (frappe un monstre allié, +2 PM, bouclier 25 % PV 2 tours) ; Mastication (−20 tacle, bâton, +20
  dommages et +10 % taille cumulables 5 fois).
- **Néfileuse (3993)** : Fil collant (attire de 5, −5 % rés.) ; Prison de soie (**Cocon** 2 tours : −100 PA / −100 PM, intacleur ; un
  coup d'un allié l'enlève) ; Toile paralysante : invoque une **Toile** indéplaçable qui pose un glyphe cercle 2 : −100 PM, −10 % rés.,
  **Inébranlable** + **Pesanteur** tant qu'elle vit (à partir du tour 2, relance 3).
- **Saltik (3994)** : Bond étourdissant (repousse 1, −2 PA, avance de 3) ; Pressage (téléporte, repousse de 3 en cercle 2).
- **Reine Nyée** : **Appel de la Reine (passif)** : frappée → les Trégénaires deviennent **Dévoué** (couleur, +2 PM tout le combat) et
  +50 puissance 2 tours (×4). Cisaillage (bâton, repousse 1, −2 PM), Mitraille de soie (cercle 2, −15 % rés.), Ponte d'Oeuf (relance 2).
  **Oeuf de Trégénaire** : **Incubation accélérée (passif)** : grossit, vole 35 neutre à tous les ennemis à chaque début de tour, soigne
  de 50 % des dégâts qu'on lui fait ; éclot en Trégénaire au bout de 2 tours. **Trégénaire** : soigne la Reine si non Dévoué ;
  Sacrifice pour la Reine (Dévoué : explose en cercle 2 et meurt).
### État sur le serveur
- Cerveaux présents : `ReineNyeeBrain` (5520) et `OeufTregenaireBrain` (5515) — conformes. Chaîne d'éclosion 2.51 :
  5515 → (délai 2) 5616 lancé par la Reine → 5616 niv. 2 = 405 KillAndSummon 3998. Effet 666 « Éclosion du serviteur » = ACTION_NOOP
  (normal). À tester en jeu (chaîne fragile : buff 792 avec délai sur la Reine).
- **Toile (3999)** : aucun sort, aucun cerveau → son passif **Toile paralysante 5507** (GlyphAura 1091 → 5508 : Pesanteur,
  Inébranlable, −100 PM, −10 % rés. ; Indéplaçable sur elle) n'est **jamais posé**. La Toile ne fait rien.
- **Dardalaine (3992)** : **Kissifrotsipik 5523** existe dans le client 2.51 (5523 niv. 1 : 792 → 5523 niv. 2 sur DM ; niv. 2 : 21-25
  neutre aux voisins, 46-50 sur l'état Empoisonné 264) mais n'est lancé par rien.
- **Arapex** : Tourbillon mortel 5512 contient `2794 → sort 5612`, inexistant en 2.51 et en base → plantage (G2) ; le bonus « +10
  dégâts au tour suivant » du site est probablement ce sort 5612 ajouté après la 2.51 → **[DOUTE VERSION]**, on ne l'ajoute pas.
- Chiffres (160 terre sur le site, 46-50 en 2.51…) : **[DOUTE VERSION]**.
### À faire (précis, prêt à coder)
1. [IMPORTANT] Toile de la Néfileuse : dans `AI/Fights/Brain/Custom/Boss/PassifsDonjonsBrains.cs` ajouter
   `[BrainIdentifier(3999)] public class ToileNefileuseBrain : BrainPassif { public ToileNefileuseBrain(AIFighter f) : base(f, 5507) { } public override void Play() { } }`
   (invoquée en cours de combat : `SortPassif.Poser` gère l'arrivée ; sort 5507 à 1 seul niveau).
2. [IMPORTANT] Dardalaine : même fichier,
   `[BrainIdentifier(3992)] public class DardalaineBrain : BrainPassif { public DardalaineBrain(AIFighter f) : base(f, 5523) { } }`.
3. [IMPORTANT] Garde-fou G2 : `game/effects/handlers/Spells/Others/CastSpell.cs`, début de `InternalApply()` :
   `if (SpellManager.Instance.GetSpellTemplate(Dice.DiceNum) == null) return false;` (le `using Stump.Server.WorldServer.Game.Spells;`
   est déjà là). Tourbillon mortel garde ses dégâts et son attirance, sans plantage.

## Antre du Blop Multicolore Royal (antre-du-blop-multicolore-royal) — boss Blop Multicolore Royal (id 1188)
### Mécanique officielle (site, partie donjon ; l'expédition est hors 2.51)
- **Blops Royaux Coco (1184) / Griotte (1185) / Indigo (1186) / Reinette (1187)** : Blotection (+100 % résistance dans leur élément,
  3 tours, une cible), Blovocation (invoque un blop), Blopunition Royale (~80 vol de vie dans leur élément + −1 PM).
- **Blopignon (1182)** : Blopiction (150 eau, −4 PO (6 crit) 3 tours, croix 1), Bloprojection (20 terre sur les cases à 2 PO).
- **Tronkoblop (1183)** : Blopzone (+1 PM aux alliés à 3 PO, 40 feu aux ennemis), Blopium (+10 esquive PM aux alliés à 3 PO).
- **Blop Multicolore Royal** : Blopacification (+50 dommages aux Royaux 2 tours), Blopagation (ennemis **Insoignables** 1 tour),
  Blopoutrage Royal (~80 dans les 4 éléments en vol de vie, −2 PM, ≤ 4 PO). Pas de mécanique spéciale.
- Blops invoqués (expédition) : le Blop Coco frappe **air**, pose un glyphe qui tue ; chaque blop a l'élément de sa couleur.
### État sur le serveur
- Sorts des Royaux, du Multicolore, du Blopignon, du Tronkoblop et du Gloutoblop : conformes 2.51 (niveaux des Royaux = couleur, OK).
- **Bug (G3)** : les blops invoqués par Blovocation (Coco 273, Indigo 274, Griotte 275, Reinette 276) et leurs Biblops
  (Indigo 277, Coco 278, Griotte 279, Reinette 280) ont `Level = grade` pour Bibloperie 473, Biblopition 471, Blopition 475, Blyphe 476.
  Or en 2.51 le niveau de ces sorts = la couleur : 473 niv. 1 air / 2 feu / 3 eau / 4 terre ; 471 niv. 1→Biblop Coco, 2→Griotte,
  3→Indigo, 4→Reinette ; 475 idem avec les Blops ; 476 niv. 1→glyphe air, 2→feu, 3→eau, 4→terre. Résultat actuel : un Blop Coco de
  grade 3 frappe eau, invoque des Indigo, etc. (touche aussi le Clos des Blops).
- Composition : jusqu'à 4 joueurs le Blop Coco Royal est absent (Multicolore + 3 Royaux) ; il apparaît à 5. Le site parle des 4 Royaux.
  **[DOUTE]** : composition 2.51 inconnue, à confirmer par l'utilisateur.
- Chiffres : **[DOUTE VERSION]**.
### À faire (précis, prêt à coder)
1. [IMPORTANT] Niveaux par couleur (base `world`, rejouable) :
   ```sql
   UPDATE monsters_spells s JOIN monsters_grades g ON g.Id = s.MonsterGradeId
   SET s.Level = CASE g.MonsterId WHEN 273 THEN 1 WHEN 278 THEN 1 WHEN 275 THEN 2 WHEN 279 THEN 2
                                  WHEN 274 THEN 3 WHEN 277 THEN 3 WHEN 276 THEN 4 WHEN 280 THEN 4 END
   WHERE g.MonsterId IN (273,274,275,276,277,278,279,280) AND s.SpellId IN (471,473,475,476);
   ```
   (Biblops : seul 473 est concerné ; 472 et 474 ont des niveaux identiques, ne pas toucher.) Redémarrer le World.
2. [MINEUR] (sur décision utilisateur) Ajouter le Blop Coco Royal dès 1 joueur dans le spawn 89 (grade 1 = MonsterGradeId 4053, vérifié).

## Antre du Dragon Cochon (antre-du-dragon-cochon) — boss Dragon Cochon (id 113)
### Mécanique officielle (site, partie « donjon » ancienne)
- **Cochon de Farle (603)** : CàC + retrait de PA. **Porsalu (3304)** : tape agilité à distance, état **Affaibli** (pas d'arme).
- **Don Dorgan (601)** : feu, invoque des Cochons de Farle, malus esquive PA ; au CàC malus à toutes les caractéristiques.
- **Don Duss Ang (600)** : attire au CàC, vol de vie eau, malus de critique.
- **Salle 11 : Gorgouille (mini-boss)** : **Lourde** ; un glyphe se pose au début du combat tout autour des joueurs (retire jusqu'à
  4 PA à qui commence son tour dessus) ; elle pousse / coopère dessus.
- **Dragon Cochon** : **Étourderie Mortelle** au 1er tour : poison 2 tours sur tous (chaque PA utilisé fait perdre beaucoup de PV),
  désenvoûtable ; ne frappe qu'au CàC (pousse, gros dégâts contre un obstacle) ; Immobilisation (retire presque tous les PM).
- Le bloc « Sorts » détaillé du site (Écrasement 2200, Croutage, Porkorosouffle…) est la version actuelle / expédition :
  **[DOUTE VERSION]**.
### État sur le serveur
- Dragon Cochon et monstres : sorts 2.51 conformes à l'ancienne description (Étourderie Mortelle 228 = 1134 neutre par PA, C63,
  2 tours, relance 12 ; Croutage, Immobilisation, Écrasement). Aucun effet non géré.
- **Gorgouille (3137)** : cerveau `GorgouilleBrain` (passif 2932) déjà prêt, mais **placée dans aucune salle** (seulement en
  `monsters_spawns` sous-zone 154, sans effet en donjon). La carte de la salle 11 existe dans le client : **72359936** « Onzième salle »,
  sans spawn. Chemin actuel : 155 → 157 → 159 (72355840) → boss 444 (72361984). La carte 72359936 n'a qu'1 case bleue en `world_maps`.
- Note : l'utilisateur a choisi « garder les salles officielles » pour ce donjon (RAPPORT groupes officiels) ; ajouter la salle 11
  change le parcours → **demander l'accord**.
### À faire (précis, prêt à coder)
1. [IMPORTANT] (avec accord) Salle de la Gorgouille entre 159 et le boss :
   ```sql
   INSERT INTO monsters_spawns_dungeons (MapId, CellId, TeleportEvent, TeleportMapId, TeleportCell, TeleportDirection, Descripcion)
   VALUES (72359936, NULL, 1, 72361984, 430, 7, 'MYSTIC Dragon Cochon 11 (Gorgouille) - boss');
   -- groupe : la Gorgouille seule (grade 5 = 7186 ; grades 7182..7186)
   INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId, MonsterGradeId, MinPartyMembers, Informations)
   VALUES (LAST_INSERT_ID(), 7186, 1, 'MYSTIC Gorgouille salle 11');
   UPDATE monsters_spawns_dungeons SET TeleportMapId = 72359936, TeleportCell = <case d'arrivée à choisir> WHERE Id = 159;
   ```
   puis lancer `placements_8v8.py` sur 72359936 (cases bleues / rouges). Composition 2.51 de la salle inconnue (le site dit
   « un mini-boss ») : **[DOUTE]** si elle était accompagnée.

## Antre du Korriandre (antre-du-korriandre) — boss Korriandre (id 2968)
### Mécanique officielle (site)
- Abrazif (2898) : +100 dommages frappé au CàC, invoque une Motte (frappe toute l'équipe si on la tape), zone à distance −PA.
  Mérulette (2900) : riposte quand on la frappe avec un sort, −100 soins au CàC. Fongeur (2897) : frappe toute l'équipe selon ses PV
  manquants. Dramanite (2895) : se booste, se reconstitue, −PM au CàC, poison par PA utilisé. Fistulor (2896) : vol de vie CàC /
  diagonale, transfert de vie aux alliés.
- Korriandre : chaque joueur pose en début de tour un glyphe mortel (disparaît à sa mort) ; **invulnérable** jusqu'à ce que la
  **Sporakne** (passif **Haimji** : poussée → pousse sa ligne de 1 ; −1 PM par coup reçu ×3) finisse son tour dans un glyphe ; il
  gagne alors 1 PM, 200 puissance, Riraule ; Hairpay, Loute, Paixe (si invulnérable), réinvocation de la Sporakne au bout de 3 tours.
### État sur le serveur
- Conforme (cerveaux Korriandre / Sporakne / Mérulette / Fongeur, tous les sorts en base, aucun effet non géré). Chiffres du site
  (450 / 550…) : **[DOUTE VERSION]**.
### À faire (précis, prêt à coder)
Rien à faire (donjon jugé parfait).

## Antre du Kralamoure Géant (antre-du-kralamoure-geacuteant) — boss Kralamoure Géant (id 423)
### Mécanique officielle (site)
- Tourbassingue (1058) : Tourbe réparatrice (+25 % crit, soin cercle 2, 1 tour sur 2), Tourbe malveillante (air marteau, −3 PO).
  Ouassingue Entourbé (1096) : Équarrissage CàC, Reformation (soin ~800 + Insoignable 3 tours), **Abnégation** (sacrifie ses alliés
  cercle 2, 2 tours), Permutation (+ état **Déplacé**). Bourbassingue (1059) : Attraction boueuse, Boue sirupeuse (poison 4 tours
  cumulable), Bourbier (−3 PM cercle 3). Roissingue (1057) : Retour du Roi (+120 puissance à un allié, −220 aux joueurs au contact),
  Déchaussage (terre marteau, −7 PM), Dépouillage (−50 soins, −420 agilité croix 3).
- Kralamoure : 900 % de résistances, immobile, cases voisines inaccessibles. Frappé dans un élément → au tour suivant tentacule de cet
  élément (air → Primaire, terre → Secondaire, feu → Tertiaire, eau → Quaternaire). Chaîne : Primaire tue une invocation, les 3 autres
  frappent la même cible (sans la tuer), Primaire tue encore → Vulnérabilité de la Tourbière (−900 % 1 tour, tentacules +1000 PV et
  +2 PM, ennemis +8000 dommages / 63 PO / +1000 PV). Tourbe écrasante (Pesanteur infinie, +1000 sagesse 6 tours), Kracheau immobilisant
  (désenvoûte, 450 feu, perte de PM si soigné), Kraken (+500 vitalité 2 tours). Le −900 % est désenvoûtable.
### État sur le serveur
- Conforme 2.51 : `KralamoureBrain` pose Kraken 1103 (états « Envie de… » sur DA/DW/DF/DE, durée 1) + limite 4 invocations + initiative ;
  Kraken est aussi dans ses sorts (1 PA, relance 0) donc relancé par l'IA. Chaîne Encre primaire → secondaire → tertiaire →
  quaternaire → 1106 : présente. Résistances 900 en base. Tous les sorts en base, aucun effet non géré.
- À tester en jeu : l'IA doit relancer Kraken à chaque tour (le buff posé au départ ne dure qu'1 tour). Si elle ne le fait pas, les
  tentacules n'apparaissent plus après le tour 1 (correction alors : relancer 1103 en `CastAutoSpell` au début de chaque tour du
  Kralamoure dans `KralamoureBrain`).
- Chiffres : **[DOUTE VERSION]**.
### À faire (précis, prêt à coder)
Rien à faire (test en jeu recommandé, voir ci-dessus).

## Arche d'Otomaï (arche-dotomaiuml) — boss Gourlo le Terrible (id 1051)
### Mécanique officielle (site)
- Le Flib (1050) : Passton Tournoube (fait passer le tour au CàC, chaque tour), Prestidigitation (tout le monde invisible).
  Nakunbra (229) : CàC + poussée. Canondorf (231). Boomba (228) : CàC. Barbroussa (1049) : coopération à travers la carte + frappe.
  Sparo (1048) : −agilité, −esquive PM, CàC −1 PM.
- Gourlo : 200 % de résistances partout. Invoque un **tonneau** (≈ chaque tour) et gagne 200 agilité ; chaque tonneau a 100 % de
  résistance dans un élément au hasard, 0 PM. **Un tonneau donne −200 % dans son élément aux entités à son contact (croix 1), 3 tours,
  seulement s'il est invoqué depuis 3 tours** (« au moment où il va jouer son troisième tour »).
### État sur le serveur
- Sorts conformes 2.51 (Rhum Fourbe niveau = grade = élément, cohérent avec les résistances des grades). Résistances Gourlo 200.
- `TonneauPirateBrain` lance Rhum Fourbe **à chaque début de tour dès le 1er tour** (commentaire : le tonneau n'a que 2 PA).
  En 2.51, le tonneau a 2 PA + Motivation Naturelle (+1 PA par tour, 6 tours) → il atteint 4 PA (Rhum Fourbe) à son 3e tour :
  c'est le délai du site. Aujourd'hui les faiblesses arrivent dès le 1er tour : mécanique trop facile.
- `Summon.cs` : Invocation de Tonneau pose **3 tonneaux** (choix « MYSTIC 17/10 ») alors que le 2.51 et le site en posent 1 au hasard
  (5 effets à 20 %). **[DOUTE]** : changement volontaire ? À confirmer avec l'utilisateur, pas de correction proposée.
- Chiffres : **[DOUTE VERSION]**.
### À faire (précis, prêt à coder)
1. [IMPORTANT] `AI/Fights/Brain/Custom/Boss/TonneauPirateBrain.cs` : ajouter un compteur `int m_tours;` ; dans `OnTurnStarted`,
   après le test `actor != Fighter`, faire `m_tours++; if (m_tours < 3) return;` avant le `CastAutoSpell` du Rhum Fourbe (Rhum
   Fourbe à partir de son 3e tour, puis à chaque tour). Test : un tonneau collé au Gourlo ne doit rien faire à ses tours 1 et 2.

## Bibliothèque du Maître Corbac (bibliotheque-du-maitre-corbac) — boss Maître Corbac (id 289)
### Mécanique officielle (site, version janvier 2025)
- Trouver 3 monstres uniques dans 16 salles : **Rono le Renarbo (823)**, **Horace le Corbac Apprivoisé (824)**, **Kapotie le Buveur
  (825)** → 3 altérations → Virgil → salle du **Capsaaloocke** puis salle du Maître Corbac. (Fonctionnement 2025 : **[DOUTE VERSION]**.)
- Buveur : Buvette (+75 % rés. lui + croix 1, 2 tours, relance 5), Daudoh (−PA/−PM cercle 5, soin des monstres), Parchotage (eau vol
  de vie, repousse 5 croix 1). Corbac Apprivoisé : Bousculade plumeuse (attire puis repousse de 12, en ligne ≤ 12), Plumeau
  Déstabilisant (désenvoûte lui + croix 1), Plumette (−30 esquive PA/PM). Renarbo : Croassement (−100 % rés. élément au hasard
  2 tours), Déplumage (CàC, −esquive PA, vol de PA), Ramage (+4 PO croix 1). Corbac : Cri Déstabilisateur (désenvoûte), Dilacération,
  Lancer d'œuf.
- **Capsaaloocke (4572)** : mini-boss : Éclaireurs (+fuite, +1 PM, esquive 100 % des coups de mêlée en reculant, dévoile les
  invisibles, cercle 5), Lancenglanté (air, vol d'agilité, −1 PM par dommage de poussée subi), Saut du Bison (étoile 2, recul).
- **Maître Corbac** : **Carapace d'Ailes** (tour 3 puis tous les 4 tours) : 2 tours, change d'apparence, −12 PA −5 PM, **renvoie
  400 dégâts**, vole de la vie air à tous à chacun de ses débuts de tour ; désenvoûtable. Invocation de Corbac (1 seul à la fois),
  Lien Volatile (+crit cercle 2, 6 tours), Sanction Ténébreuse (CàC, doublée en critique).
### État sur le serveur
- **Bug (G3) [boss]** : `monsters_spells` du Maître Corbac : Carapace d'Ailes 630 au **niveau 3 pour les grades 3, 4 et 5**. Le
  niveau 3 de 630 n'est que le sous-sort déclenché (93 vol de vie air 50 sur C63). Les vrais effets (apparence, −5 PM, −12 PA,
  renvoi 400, déclencheur TB → 630 niv. 2) sont au niveau 1 → aux grades 3-5, **pas de carapace ni de renvoi**, juste un vol de vie
  à toute la carte tous les 4 tours. Grades 1-2 : niveau 1, correct. (Le niveau 3 = version plus forte du tic de dégâts ; le
  serveur ne sait pas choisir le niveau du sous-sort selon le grade.)
- **Salles de la Bibliothèque fausses** : les 33 spawns 466-498 (16 salles, cartes 159121408…159124486) contiennent **Miss Teigne
  (4556), Robot Pousse-Pousse (4557), Robot Fléau (4558), Malle Outillée (4559)**, niveau 200, sous-zone 54 (Massif de Cania) :
  999 lignes de groupes, données d'origine (Informations vide). Dans le client 2.51, la sous-zone 211 (Bibliothèque) a
  **Capsaaloocke 4572, Buveur de Sang 4578, Corbac Chef 4579, Corbac Dressé 4580, Renarbo Parleur 4581** (niveaux 102-110), présents
  en base mais jamais placés. Le spawn 498 (« Sala 11 - Sala Final (Capslocke) ») devait contenir le Capsaaloocke : il contient
  Miss Teigne.
- Salle du trône (186) : Maître Corbac + Rono + Horace + Kapotie : cohérent avec le site. Sorts des monstres : conformes 2.51.
- Les 3 monstres uniques à trouver (altérations, Virgil) : système 2025 → **[DOUTE VERSION]**, rien à coder.
### À faire (précis, prêt à coder)
1. [CRITIQUE] Carapace d'Ailes au bon niveau :
   ```sql
   UPDATE monsters_spells s JOIN monsters_grades g ON g.Id = s.MonsterGradeId
   SET s.Level = 1 WHERE g.MonsterId = 289 AND s.SpellId = 630;
   ```
   Redémarrer le World. Test : au tour 3, le Maître Corbac change d'apparence et renvoie 400 dégâts.
2. [IMPORTANT] Remplacer les robots par les monstres 2.51 de la Bibliothèque (tous les groupes sont au grade 3) :
   ```sql
   UPDATE monsters_spawns_dungeons_groups SET MonsterGradeId = CASE MonsterGradeId
       WHEN 13901 THEN 13977                              -- Miss Teigne g3 -> Capsaaloocke g3
       WHEN 13906 THEN 14007                              -- Robot Pousse-Pousse g3 -> Corbac Dressé g3
       WHEN 13911 THEN 14002                              -- Robot Fléau g3 -> Corbac Chef g3
       WHEN 13916 THEN IF(Id % 2 = 0, 13997, 14012)       -- Malle Outillée g3 -> Buveur de Sang / Renarbo Parleur g3
     END
   WHERE DungeonSpawnId BETWEEN 466 AND 498 AND MonsterGradeId IN (13901, 13906, 13911, 13916);
   ```
   (sauvegarde avant ; composition exacte 2.51 inconnue → **[DOUTE]** sur les proportions, les monstres eux sont sûrs.)

## Cour du Bouftou Royal (cour-du-bouftou-royal) — boss Bouftou Royal (id 147)
### Mécanique officielle (site)
- Boufton Blanc (134) : Bavouille (−2 PA à 3 PO), Machouillage (air CàC). Boufton Noir (149) : Crachouille (−2 PM à 3 PO), Mordillement.
  Bouftou (101) : Bêlement (+2 PM aux alliés au contact), Morsure (feu CàC). **Bouftou Noir** : Grognement (+20 % dégâts subis
  2 tours), Haleine (attire de 2), Morsure Obscure. Chef de Guerre (148) : Fureur (avance de 3 + terre), Morsure de Guerre, Ralliement.
- Bouftou Royal : Cuirasse Laineuse (un allié : dommages subis ×0,7 1 tour), Guérison Bouftou (soin 200, 2/tour, 1/cible, ≤ 6 PO),
  Morsure du Bouftou Royal (feu CàC, vol de vie sur les invocations). **IA fuyarde** (recule après avoir frappé).
### État sur le serveur
- Sorts 2.51 conformes dans l'esprit ; plusieurs sorts du site n'existent pas en 2.51 (Bêlement, Fureur « avance », sorts du Bouftou
  Noir actuel) → **[DOUTE VERSION]**. Cuirasse Laineuse 2.51 = 1163 ×70 % sur D : conforme.
- Le **Bouftou Noir 2.51 (4822**, sous-zone 82 = Cour du Bouftou Royal, sorts Malveillance 8567 = dégâts subis ×120 % 2 tours,
  Secousses 8568) existe en base mais n'est dans aucun groupe des spawns 16-20.
- IA fuyarde du Bouftou Royal : pas de cerveau (comportement d'IA, absent des données client) → **[DOUTE VERSION]** ; si voulu, même
  méthode que `FongeurBrain` (`public override bool Fuyard => true;`) avec `[BrainIdentifier(147)]`.
### À faire (précis, prêt à coder)
1. [MINEUR] (sur décision utilisateur, composition 2.51 inconnue) ajouter le Bouftou Noir 4822 dans les salles 18-20 :
   grades 15145 à 15149 (grade 1 à 5) ; insérer quelques lignes dans
   `monsters_spawns_dungeons_groups` (DungeonSpawnId 18, 19, 20).

## Grotte du Bworker (grotte-du-bworker) — boss Bworker (id 478)
### Mécanique officielle (site)
- Bworks élémentaires Air (486) / Eau (485) / Feu (484) / Terre (487) : un sort de frappe de leur élément (Épée Céleste, Larme
  Bwork, Poussière Temporelle −4 PA, Épée du Bwork érosion), Sollicitude Élémentaire (invoque une Flammèche : attire de 4 puis se
  fait exploser au contact, 150 % de ses PV), Communion Élémentaire (+50 % vitalité à l'invocation, 4 tours).
- Cybwork (488) : Cybarmure (bouclier, réduction), Don de Soi (intercepte les dommages des alliés cercle 2), Pulsation (terre, −2 PM).
  Mama Bwork (479) : Couperet (bâton, +200 puissance cumulable), Cri Revitalisant (soin cercle 3, 3/tour), Cri de Reconstitution
  (soin ~2700 + Insoignable 3 tours, relance 7).
- Bworker : Correction Bwork (30 % des PV manquants, CàC), Fauchoir (repousse 2), Sanction Bwork (CàC), **Poursuite** (+3 PM
  « tous les x tours »). Ne frappe qu'au CàC.
### État sur le serveur
- Sorts en base conformes 2.51 ; aucun effet non géré.
- **Poursuite (8675)** existe dans le client 2.51 mais n'est pas dans les sorts du Bworker et aucun cerveau ne la pose. 2.51 :
  8675 = buff permanent, en fin de tour du Bworker → 8676 : +3 PM au début de son tour suivant, sauf s'il finit son tour collé à
  un ennemi (8676 niv. 2 lancé par l'ennemi collé retire l'effet, 406 sur F478). → passif jamais lancé.
- Communion Élémentaire : un effet différent par grade en 2.51 (50 % vita / +2 PM + fuite / crit / résistances / vitalité fixe) ;
  le site ne parle que de la vitalité → **[DOUTE VERSION]**, ne pas toucher.
### À faire (précis, prêt à coder)
1. [IMPORTANT] `PassifsDonjonsBrains.cs` :
   `[BrainIdentifier(478)] public class BworkerBrain : BrainPassif { public BworkerBrain(AIFighter f) : base(f, 8675) { } }`.
   Test : le Bworker qui finit son tour loin des joueurs a +3 PM au tour suivant ; collé à un joueur, non.

## Donjon des Bworks (donjon-des-bworks) — boss Bworkette (id 792)
### Mécanique officielle (site)
- Bwork (876) : Éventration (neutre CàC, 2/tour), Rage (+6 à 20 dommages 3 tours), Soufflette (repousse autour d'une case, 2-8 PO).
  Bwork Archer (74) : Flèche Trouveuse, Maîtrise des Armes de Jet (+60 dommages), Projectile Puissant (+250 puissance).
  Bwork Mage (53) : Éclair en Série (feu cercle 2), Invocation de Tofu Maléfique, Tornade (repousse 5). Tofu Maléfique : Béco
  Maléfique, Maléfice (érosion + **Insoignable** croix 1).
- Bworkette : Abolition (500 à une invocation), Charge, Mot Croisé (dégâts ennemis / soin alliés croix 3, 2/tour), Rage,
  Reconstitution Bwork (50 % PV, relance 6). Pas de mécanique spéciale.
### État sur le serveur
- Conforme 2.51 (tous les sorts en base, aucun effet non géré ; niveaux = grades cohérents). Chiffres : **[DOUTE VERSION]**.
### À faire (précis, prêt à coder)
Rien à faire.

## Cache de Kankreblath (cache-de-kankreblath) — boss Kankreblath (id 3945)
### Mécanique officielle (site)
- Cafarcher (3940) : cure-dent CàC / 3-6 PO. Céglumen (3944) : Attrape coton-tige (attire de 3), Badigeonnage de cérumen (glyphe
  cercle 2 : −1 PM, −fuite, terre en début de tour). Mirgrillon (3942) : Coup de boutonclier (repousse, −2 PA), Mini empalement
  (ligne 3). Pyrasite (3941) : Cri de l'insecte ardent (+2 PM, +50 puissance à tous les alliés), Souffle brûlant (cône 2 + poison).
  Sakarien (3943) : Frappe circulaire (cercle 2, plus fort sur les invocations), Invocation de Poupoussière.
- Kankreblath : Sfvc%$*$R ? (1 chance sur 3 : invisible / téléportation / invoque un monstre de la zone ; relance 3, dès le tour 2),
  Blathération (feu ligne 6), Kankroulahoup (feu cercle 3, soigne les alliés). Pas de mécanique spéciale.
### État sur le serveur
- Conforme 2.51 (aléatoire 33/33/6×5 géré par `DefaultSpellCastHandler`). Frappe circulaire 2.51 = tue les invocations dans la zone
  (le site : dégâts augmentés) → **[DOUTE VERSION]**.
### À faire (précis, prêt à coder)
Rien à faire.

---

## Récapitulatif

| Donjon | CRITIQUE | IMPORTANT | MINEUR |
|---|---|---|---|
| Aquadôme de Merkator | 1 | 0 | 0 |
| Antichambre des Gloursons | 0 | 0 | 0 |
| Antre de la Reine Nyée | 0 | 3 | 0 |
| Antre du Blop Multicolore Royal | 0 | 1 | 1 |
| Antre du Dragon Cochon | 0 | 1 | 0 |
| Antre du Korriandre | 0 | 0 | 0 |
| Antre du Kralamoure Géant | 0 | 0 | 0 |
| Arche d'Otomaï | 0 | 1 | 0 |
| Bibliothèque du Maître Corbac | 1 | 1 | 0 |
| Cour du Bouftou Royal | 0 | 0 | 1 |
| Grotte du Bworker | 0 | 1 | 0 |
| Donjon des Bworks | 0 | 0 | 0 |
| Cache de Kankreblath | 0 | 0 | 0 |
| **Total** | **2** | **8** | **2** |

À décider par l'utilisateur (pas codé) : Gorgouille du Dragon Cochon (change le parcours « officiel » choisi), 3 tonneaux du Gourlo
au lieu de 1, Blop Coco Royal absent jusqu'à 4 joueurs, Bouftou Noir 4822, IA fuyarde du Bouftou Royal, sens de DCAC (G4),
généraliser la poussée forcée 1021/1022 à tous les sorts (G1).
