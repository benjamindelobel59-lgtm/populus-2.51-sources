# Lot 5 : audit des mécaniques (dofuspourlesnoobs → Mystic 2.51)

Audit en lecture seule (18/10). Sources : pages du site, `analyse_mecaniques.py`, données du client 2.51 (Spells / SpellLevels / Monsters.d2o),
base `world` et code serveur tel qu'il est AUJOURD'HUI (d'autres agents l'ont déjà modifié : cerveaux `DimensionsBrains.cs`,
`FrigostOtomaiBrains.cs`, correctif `Heal.cs`, zones « case fixe »…).
Règle version appliquée : tout ce que le site décrit et qui n'existe pas dans le client 2.51 est classé **[DOUTE VERSION]** et n'est
PAS dans les corrections. Les chiffres du site différents de ceux du 2.51 sont aussi [DOUTE VERSION].

## Trois bugs moteur trouvés pendant ce lot (ils touchent plusieurs donjons, y compris hors lot)

**B1 — `GlyphAura` ignore les masques de cible.** `game/fights/Triggers/GlyphAura.cs`, méthode `Enter` :
`effectHandler.SetAffectedActors(new[] { actor })` est appliqué à TOUS les effets du sort de la glyphe, quel que soit leur masque
(`Glyph.Trigger` fait bien le test `IsValidTarget`, pas `GlyphAura`). Conséquences : Moon (une glyphe de totem tue quiconque y
entre, effet `141 Kill a,F225,T` ; le Darkli Moon meurt dès la pose de sa glyphe et le Moon perd ses résistances tout de suite),
Pyramide d'Ombre (l'aura du Globilum donne l'état « Ombre et Lumière » et +20 % dommages finaux à tout le monde).
Correctif : dans `Enter`, remplacer la boucle par
`effectHandler.SetAffectedActors(effectHandler.IsValidTarget(actor) ? new[] { actor } : new FightActor[0]);`
(même logique que `Glyph.Trigger`). À vérifier après coup : glyphes aura des classes (Féca, Eliotrope…) — elles ont des masques
corrects dans le client, elles ne devraient que gagner en exactitude.

**B2 — masque de cible `o` inversé.** `game/effects/handlers/Spells/Targets/TargetCriterion.cs` : `'o'` →
`LastAttackerCriterion(false)` = « n'importe qui SAUF le dernier attaquant ». Dans le client 2.51, `o` désigne au contraire
l'entité qui a déclenché l'effet (l'attaquant / le tueur / celui qui déplace) : Saute-Bouftou 5171 (`a,o` = l'allié qui a frappé),
Invocations incapacitantes 5166 (`o,I` = l'invocation qui a frappé), Boubourse 4483/4512 (`A,o`, `L,o`, `J,o`, `g,o` = le tueur
reçoit la Boubourse), Encrage d'Ombre 4115 (`A,o` = celui qui déplace Ombre), totems du Moon 6163-6167 niv. 2 (`A,o` X63 = l'attaquant
en ligne attire le totem). Avec le code actuel, à 1 joueur ces effets ne touchent personne, à plusieurs ils touchent tous les AUTRES.
Correctif proposé : nouvelle classe `TriggererCriterion` : `IsTargetValid(actor, handler) => actor == (handler.CastHandler?.Informations?.Triggerer ?? handler.Caster.LastAttacker)`
et `case 'o': return new TriggererCriterion();`. Ne pas toucher `'O'` (dernier attaquant : Lévitrof 4520, Électromagnétisme 6075,
cohérent avec le site). **Prévenir le coordinateur** : 86 effets du client utilisent `o` (Nidas, Ombre, Phossile/Rage Mahal 5038,
Sanctuaire…), d'autres lots sont concernés. [DOUTE] léger sur le sens exact de `o` (pas de documentation officielle), mais les
6 exemples ci-dessus ne sont cohérents qu'avec « le déclencheur ».

**B3 — critère `U` (« vient d'être invoqué ») faux pour un monstre ressuscité.** `Targets/JustSummonedCriterion.cs` exige
`actor.IsSummoned()` (Summoner != null) ; un monstre normal ressuscité par `ReviveAndGiveHP` (effet 780) a bien `SummoningEffect`
mais pas de Summoner → l'état Invulnérable de « Plantes Zombies » (Halouine) n'est jamais posé. Correctif :
`return (actor.IsSummoned() || actor.IsRevived) && handler.CastHandler.GetEffectHandlers().Any(x => actor.SummoningEffect == x);`

---

## Donjon de la Mine de Sakaï (donjon-de-la-mine-de-sakaiuml) — boss Grolloum (id 2942)
Base : « Mazmorra de la Mina de Kéfriho » (spawns 345-349). Monstres : Gobosteur 2932, Sapeur 2933, Ouilleur 2934, Marôdeur 2935,
Perkü 2936, Courtilieur 2937, Gobus 2941 (invoqué par le Perkü, aussi en salle 1 et 5).
### Mécanique officielle (site)
- Courtilieur 2937 : Minage (terre ~250 càc + poison terre 26-35 par ligne de dégâts subie, 1 tour, 1×/cible) ; Détourage (eau vol de
  vie ~150 sur les 4 cases adjacentes + **Affaibli** 2 t, relance 3) ; Chaupage (air = 10 % de SES PV manquants + attire 4, en ligne 3-5 PO).
- Gobosteur 2932 : Piolaid (air ~250 càc, -2 PA, 2×/tour) ; Piochekour (+50 dommages à lui et aux alliés au contact 4 t, relance 3) ;
  Piochetron (terre ~150, -1 PM, ligne 4-6 PO).
- Marôdeur 2935 : Chapardage (eau vol de vie ligne 3 cases, repousse 2, ligne ≤2 PO) ; Escamotage (+25 dommages par ligne de dégâts
  reçue et **esquive 100 % des attaques de mêlée en reculant d'1 case**, lui + alliés au contact, 1 t, relance 3) ; Filoutage (eau vol
  de vie ligne 3 cases, vole 1 PM, ligne 3-5 PO).
- Ouilleur 2934 : Daipe (terre vol de vie ~350 càc, +20 % érosion 1 t) ; **Shatte** (1 t : à chaque ligne de dégâts reçue, bonus
  aléatoire +2 PM / +6 PA / +50 dmg / +50 % CC / soin 300, relance 3) ; Krape (soin allié 200 + soin 250 par ligne reçue 1 t, ≤4 PO).
- Perkü 2936 : Djatte (eau ~300 + poison eau déclenché à chaque soin reçu, ligne 2-10 PO) ; Béni (+50 dmg et jets max, cercle 4, 2 t,
  relance 3) ; Lautte (invoque un Gobus, ligne ≤10 PO, relance 3).
- Gobus 2941 : 0 PM ; Gobtimisation (+200 dmg aux alliés au contact 2 t) ; **Gobstruction** : à la fin de son 2e tour, tue tous les
  personnages en ligne avec lui et meurt (n'explose pas si personne en ligne).
- Sapeur 2933 : Lipopette (feu ~300 au contact, +25 dmg 4 t) ; **Nowel/Goblosion** (1 t : par ligne de dégâts reçue +10 dmg 4 t et
  terre ~150 sur les cases adjacentes, relance 3) ; Nicheuze (échange avec un allié à 5 PO pile, eau ~150 autour de l'arrivée).
- Grolloum 2942 : **Dur comme la roche** (Invulnérable tout le combat ; +8000 résistances 1 t si on tente de lui retirer des PM) ;
  Cycle (ressuscite le dernier monstre mort, état **Zombi**, 5-10 PO, relance 2) ; Gelée blanche (+200 dmg et effet **Instant** 1 t sur un
  Zombi ; si ce Zombi meurt au contact de Grolloum, Grolloum perd l'invulnérabilité jusqu'au début du tour suivant du tueur) ;
  Banquise (forme invulnérable : eau vol de vie 660 ligne 5, -4 PO, se soigne 1500 PV par personnage touché) ; Frimas (forme
  vulnérable : échange de place, feu 300 + 10 % de ses PV manquants).
- Expéditions de guilde (Audace / Bravoure) : [DOUTE VERSION] (expéditions postérieures à la 2.51).
### État sur le serveur
- Tous les sorts 2.51 présents en base, aucun effet ni déclencheur non géré (outil : « rien »). Le correctif « soin avec
  déclencheur » de `Heal.cs` (Krape, Shatte) est déjà en place.
- Cerveau `GrolloumBrain.cs` (fait le 17/10 selon les choix de l'utilisateur) : invulnérable, résurrection tous les 2 tours (25 %, Zombi),
  Gelée blanche/Instant suivi par `VulnerableJusquAuTourDuDeclencheur`, 8000 résistances sur retrait / tentative de retrait de PM.
  Le passif client « Dur comme la roche » 2408 (déclencheur MPA) est remplacé par du code : conforme à la description.
- Gobstruction 2409 : délai initial 1 (lançable au 2e tour), dégâts 1001-1500 feu en croix 63 + mort du Gobus : conforme 2.51.
- [DOUTE VERSION] Banquise 2.51 : soin 101-150 PV sur Grolloum (pas 1500 par personnage) ; l'utilisateur a choisi de ne pas mettre le
  soin de 1500 → ne pas toucher. Chiffres des monstres (250, 350…) = valeurs du site actuel, non comparées.
### À faire (précis, prêt à coder)
Rien à faire.

## Centre du Labyrinthe du Minotoror (centre-du-labyrinthe-du-minotoror) — boss Minotoror (id 121)
Base : spawn 208 « Centre Labyrinthe Minotor » : Minotoror 121, Déminoboule 832, Mominotor 831, Gamino 668, Scaratos 830, Serpiplume 829.
### Mécanique officielle (site)
- Déminoboule 832 : Coup de Boulet (terre ~200, ligne 2 PO pile) ; Minorage (+201-300 puissance aux alliés au contact 2 t, relance 6) ;
  Souffle Étourdissant (càc : terre ~170, -201-250 puissance 3 t, poison de début de tour -2-3 PM 3 t, relance 3) ; Souffle Libératoire
  (repousse 3 tout ce qui est au contact).
- Mominotor 831 : Embaumement (air ~200 càc + poison air vol de vie 50 / début de tour 5 t) ; Kanope (attire 7, ligne 2-8 PO,
  1×/cible) ; Lancer de Dégelée (invoque une Dégelée, eau ~80 + repousse 1 la cible derrière, ligne 2-8 PO) ; **Malédiction du
  Mominotor** (seulement s'il y a une invocation ennemie : feu vol de vie 650 sur toutes les invocations puis les tue, transforme les
  joueurs en momie et -3 PM en début de tour 2 t, relance 6).
- Gamino 668 : Chevauchée Malicieuse (+10 PM +100 agi 4 t) ; Coup de Sceptre (terre ~150, -3 PM, ligne ≤3) ; Esprit d'équipe (+50 dmg
  +2 PM cercle 2, 75 dmg pour Déminoboule/Mominotor/Minotoror/Minotot).
- Scaratos 830 : Cuticule (+21-25 rés + renvoi 15, cercle 2, 2 t, relance 6) ; Défonce (air ~100, -1 PA càc) ; Recueillement (+1-2 PA
  6 t + soin 200 au début du tour 1 t, cercle 2) ; Scaracornos (terre ~60, repousse 3 càc).
- Serpiplume 829 : Boulette (eau ~150, -11-15 % rés eau 5 t, ligne 2-5) ; Sonnette (soin 250, ≤5 PO, relance 3) ; Venin
  Déstabilisateur (-1 PM -1 PA -1 PO 1 t, ligne ≤5).
- Minotoror 121 : pas d'invulnérabilité ; rés 40-60 % sauf terre -20 % ; Lancer de Tofu chaque tour (Tofu lancé 99 PV, neutre ~150 +
  repousse 1, ligne ≤10) ; Graines Magiques (+100 vita +50 dmg à une invocation alliée 2 t) ; Sabotage (neutre 600 / 1000 CC + repousse
  6, càc, 2×/tour).
- Labyrinthe (leviers, reliques du Déminoboule / Mominotor) : hors périmètre des sorts (navigation / clef).
- Expédition de guilde (Emplafonnage, Toromachine…) : [DOUTE VERSION] (sorts absents du client 2.51 : recherche « Emplafonnage » vide).
### État sur le serveur
- Tous les sorts 2.51 présents, rien de non géré. Effets aléatoires (Dégelée Décassé / Papouille) gérés par `DefaultSpellCastHandler`
  (champ `Random`). Résistances du Minotoror en base = client (40/-20/50/60/50). Pas de cerveau nécessaire.
- [DOUTE VERSION] chiffres 2.51 plus bas (ex. Sabotage 71-110 neutre, Malédiction 301-350) : ne pas modifier.
### À faire (précis, prêt à coder)
Rien à faire.

## Salle du Minotot (minotot) — boss Minotot (id 827)
Base : spawn 283 « Minotot » : Minotot 827 + Minotoror 121, Déminoboule 832, Mominotor 831, Gamino 668, Scaratos 830.
### Mécanique officielle (site)
- Monstres : mêmes sorts que ci-dessus.
- Minotot 827 : **Destinos** (tour 1 puis tous les 3 tours : désenvoûte, transforme en tofu, -100 PA 3 t, poison feu 400/tour 3 t,
  repousse 5 ; lui et ses alliés au contact +3 PM 4 t et +500 agi 3 t) ; Mythos (400 dans un élément aléatoire + vol 2 PA, ligne ≤4,
  2×/tour) ; Kitos (lançable seulement à 15 PA : soin 2000 au début de tour 3 t pour tous ses alliés) ; Motivatos (+1 PA 6 t, 1×/tour) ;
  Lancer de Tofu. Faiblesses eau / air ; « on ne peut pas lui retirer de PM ni de PO ».
- Expéditions : [DOUTE VERSION].
### État sur le serveur
- Tous les sorts 2.51 présents (11458, 814, 3441, 813, 812), rien de non géré. Mythos : 4 lignes « random 25 » = un élément au hasard,
  géré. Kitos coûte 15 PA (10 PA de base + Motivatos) : conforme. Destinos 2.51 : zone X1 pour les malus + poussée X5 (site : croix 5).
- Retrait PM / PO : le Minotot 2.51 a 900 de sagesse (esquive très élevée), pas d'état d'immunité dans le client : conforme 2.51.
- [DOUTE VERSION] chiffres (Kitos 201-300 au lieu de 2000, poison Destinos 51-75 au lieu de 400).
### À faire (précis, prêt à coder)
Rien à faire.

## Arbre de Moon (arbre-de-moon) — boss Moon (id 226)
Base : spawns 181-185 « Moon » : Trukikol 215, Dostrogo 4162, Gloutovore 216, Domoizelle 4163, Fourbasse 217, Moon 226 (salle 5).
Invocations : Darkli Moon 225 (Face Cachée 6141, grade 1), Totems du Feu 2815 / de l'Eau 2816 / de l'Air 2817 / de la Terre 2818
(grade 1, 500 PV, 100 % rés dans leur élément, ne jouent pas, `CanBePushed=0`), Dokachu 4177, Dolbinos 4176, Dolivar 4178.
### Mécanique officielle (site, partie « donjon » ; l'expédition est [DOUTE VERSION])
- Domoizelle 4163 : Invocation de Dodoune (Dokachu / Dolbinos / Dolivar) ; Poussée Dormone (+dommages de poussée à elle et aux
  alliés 1 t) ; Protection Maternelle. Dokachu : Dorage (+ **Affaibli** 1 t, -50 % PV) ; Dolbinos : Coup de Bec Affaiblissant (jets
  minimisés, -200 puissance 1 t), Dotite (zone + poussée).
- Dostrogo 4162 : Dochirure (-PM ; effet qui frappe quand la cible subit des dommages de poussée) ; Coup de bec Dominant (poussée).
- Fourbasse 217 : Tir Embusqué (-2 PA, plus fort sur un **Chassé**) ; L'Attaque du Chasseur (-63 PO et état **Chassé**).
- Gloutovore 216 : Crasse Piration (attire, vol de vie, PO infinie) ; **Gobage** (tue la cible au càc, se soigne de 100 %, +1 PM) ;
  site : passif **Corps Gluant** (soin 15 % aux alliés à chaque tentative de retrait PM, +1 PM sur tentative de retrait PA).
- Trukikol 215 : Électromagnétisme (zone terre + effet : les monstres s'attirent de 3 vers la cible frappée) ; Virevoltage collant
  (glyphe -100 PM).
- Moon 226 : 100 % (2.51 : 200 %) de résistances partout ; Marteau de Moon (-3 PA -3 PM -1 PO) ; Choc Sismique (zone + téléportation
  symétrique) ; Face Cachée (invoque le Darkli Moon). Le Darkli invoque 4 totems (rouge feu, vert air, bleu eau, jaune terre) et pose
  à chaque fois une glyphe de la couleur du totem ; un totem frappé dans son élément par un personnage EN LIGNE est attiré à son
  contact ; un totem posé sur sa glyphe retire au Moon ses résistances dans cet élément. Les totems ne sont pas déplaçables autrement.
  Site : frapper le Darkli échange de place et donne +100 puissance au Moon ; « Rituel d'Emprisonnement ».
### État sur le serveur
- Mécanique 2.51 (vérifiée dans le client) : Darkli Moon 225 a le passif **Puissance Obscure 302** (non listé dans ses sorts) : 24
  variantes tirées au hasard qui remettent à 0, au début de ses tours 1 à 4 (délais 0,1,2,3), la relance de ses 4 sorts « Totem »
  298-301 (délai initial 63 = jamais lançables sans ça). Niveau 2 de ces sorts = invocation du totem + relance remise à 63.
  Chaque totem a son passif (6163 feu, 6167 eau, 6166 air, 6165 terre) : niv.1 → (a) frappé dans son élément (DF/DW/DA/DE) : l'attaquant
  en ligne (X63, masque `A,o`) le tire jusqu'à lui (6163 niv.3, effet 1022 attire 63) ; (b) à la fin du tour du Darkli : glyphe aura
  d'une case (6163 niv.4 → sort 294-297) posée sous le Darkli, qui ne réagit qu'au totem de la bonne couleur (`a,F2815`) : il donne
  au Moon -200 % de résistance dans l'élément (294-297 niv.2, permanent) ; le Darkli qui y arrive par échange (télé-frag `T`) meurt.
- Cerveaux existants : `TotemMoonBrain` (FrigostOtomaiBrains.cs) pose bien 6163/6165/6166/6167 niv.1 et empêche les totems de jouer.
- **Ce qui casse la mécanique aujourd'hui** (le Moon reste à 200 % partout = boss intuable) :
  1. aucun cerveau pour le Darkli Moon 225 → Puissance Obscure 302 jamais posée → les sorts Totem 298-301 gardent leur délai initial 63
     → aucun totem n'est jamais invoqué ;
  2. le Darkli est invoqué au GRADE 1 (`181 225/1`) et, en base, le grade 1 (`monsters_spells.MonsterGradeId=710`) a les sorts 298-301
     au NIVEAU 1, qui ne fait que « remettre la relance » (pas d'invocation). Même avec 302, rien ne serait invoqué ;
  3. `Push.cs` saute toute cible dont `CanBePushed()` est faux ; les totems ont `CanBePushed=0` (client et base) → l'attirance 1022 de
     leur propre passif ne les déplace jamais ;
  4. bugs moteur B1 (glyphe aura sans masque : tue le Darkli à la pose de la glyphe, et tue tout joueur qui marche dessus) et B2
     (masque `o` inversé : ce n'est pas l'attaquant qui attire le totem mais les autres joueurs en ligne).
- Gloutovore : **Corps Gluant 6120** existe dans le client 2.51 (soin 15 % zone a1 sur MPA, +1 PM sur APA) mais n'est attribué à aucun
  monstre, ni dans le client ni en base. [DOUTE] : présent dans les données 2.51 donc autorisé par la règle, mais rien ne prouve qu'il
  était actif en 2.51 dans le donjon (il peut n'avoir servi qu'à l'expédition) → à décider par l'utilisateur.
- [DOUTE VERSION] : « Rituel d'Emprisonnement » 6150 (effet 666, non attribué, inutile en 2.51), +100 puissance au Moon quand on frappe
  le Darkli, toute l'expédition (glyphes noires, 200 % / 5 tours…). Chiffres des monstres (450-500 du site) = expédition.
### À faire (précis, prêt à coder)
1. [CRITIQUE] Base : donner au Darkli Moon grade 1 les sorts Totem au niveau 2 :
   `UPDATE monsters_spells SET Level=2 WHERE MonsterGradeId=710 AND SpellId IN (298,299,300,301);` (grade 1 = Id 710 ; les grades 2-5,
   Id 711-714, ont déjà le niveau 2). Fichier SQL à créer dans `_OUTILS/serveur/donjons/` avec son -annuler (Level=1).
2. [CRITIQUE] Cerveau du Darkli Moon (dans `FrigostOtomaiBrains.cs`, à côté des totems) :
   `[BrainIdentifier(225)] public class DarkliMoonBrain : BrainPassif { public DarkliMoonBrain(AIFighter f) : base(f, 302) { } }`
   (le Darkli est une invocation : `SortPassif.Poser` passe par `FighterAdded`, c'est prévu). Puissance Obscure n'a qu'un niveau (1).
3. [CRITIQUE] `game/effects/handlers/Spells/Move/Push.cs` (boucle `foreach (var actor in actors)`, test `if (!actor.CanBePushed())`) :
   laisser passer l'attirance des totems : `if (!actor.CanBePushed() && !(Effect.EffectId == EffectsEnum.Effect_1022 && Spell.Id >= 6163 && Spell.Id <= 6167)) continue;`
   (6164 n'est pas un totem mais n'a pas d'effet 1022 : sans risque). Les autres poussées / attirances restent bloquées (officiel).
4. [CRITIQUE] Bug moteur B1 (`GlyphAura.Enter`, voir en tête de fiche).
5. [CRITIQUE] Bug moteur B2 (masque `o`, voir en tête de fiche).
6. [IMPORTANT, à valider par l'utilisateur] Gloutovore 216 : poser Corps Gluant 6120
   (`[BrainIdentifier(216)] public class GloutovoreBrain : BrainPassif { public GloutovoreBrain(AIFighter f) : base(f, 6120) { } }`).
   Ne pas le faire sans accord (doute version ci-dessus).
Test après 1-5 : au 1er tour du Darkli un totem apparaît (puis un par tour, ordre aléatoire) ; à la fin de son tour une glyphe d'une
case de la couleur apparaît sous lui ; frapper le totem dans son élément en ligne le tire au contact ; le poser sur sa glyphe →
le Moon passe à 0 % dans cet élément (vérifier dans ses caractéristiques). Point incertain à surveiller : la glyphe dépend d'un buff de
durée 1 déclenché en fin de tour du Darkli (TE) ; d'après `Fight.OnTurnStopped` la fin de tour est déclenchée avant toute
décrémentation, ça devrait marcher.

## Nid du Kwakwa (nid-du-kwakwa) — boss Kwakwa (id 2995)
Base : spawns 66-70 « Kwakwa » : Kwaks 84/81/235/83, Kwakeres 271/272/270/269, Bwaks 3282/3281/3284/3283, Kwakwa 2995 (salle 5).
### Mécanique officielle (site)
- Bwaks : Bwakikui (50, ligne ≤5) ; Bwakitori (repousse 2). Kwaks : Éventrement (80 neutre càc), Kwakoukas (50 + repousse 3, ligne ≤7),
  Wakpot (vole 100 de stat d'élément 2 t). Kwakeres : Griffes Acérées, Wakolanterne (+10 % dégâts subis, repousse 4), Wakzefeute
  (vole 50 puissance 3 t).
- **Kwapoussée** (tous les monstres : -10 % rés et +10 dommages de poussée 3 t par dommage de poussée subi) ; **Kwayauté** (le Kwakwa
  perd 40 % de rés dans l'élément de chaque monstre mort et gagne 40 dommages de poussée) ; Kwakwa 80 % rés ; Kwarmée Kwayal
  (4 Bwaks au début) ; Kwaristocratie, Kwabolition, Kwakoukas Kwayal, Wakpot Kwayal.
### État sur le serveur
- Version 2.51 (client) différente : **Kwayauté 3253** = au début du combat, UN élément tiré au hasard (5 groupes « random ») : état
  Kwaflamme / Kwaglace / Kwaterre / Kwavent / Kwaneutre et -50 % de rés dans cet élément (75 % → 25 %) ; tous ses sorts existent en
  5 versions et ne sont lançables que dans l'état de son élément (Kwamagueul, Kwakalarme = invoque le Bwak et le Kwakere de son
  élément, Kwakoukas, Wakpot).
- Cerveau `KwakwaBrain.cs` : lance Kwayauté 3253 niv.1 au début du combat : conforme 2.51. Tous les sorts présents, rien de non géré.
- [DOUTE VERSION] Kwapoussée (aucun sort de ce nom dans le client 2.51), Kwayauté « -40 % par mort », Kwarmée / Kwaristocratie /
  Kwabolition (absents du client 2.51), 80 % de résistances (2.51 : 75 %), sorts des Bwaks (2.51 : Bwakitori = boost PA/rés, pas une poussée).
### À faire (précis, prêt à coder)
Rien à faire.

## Donjon de Nowel (donjon-de-nowel) — boss Sapik (id 1179)
Base : spawns 76-80 « Nowel Sapik » : Ver Glacé 870, Pokipik 1180, Tofu Givré 841, Black Tiwabbitus 899, Tofu Enneigé 868, Kwakus 901,
Kipik 849, Kitsou Nakwatus 896, Kanigrou Hivernal 840, Sapik 1179 (salle 5).
### Mécanique officielle (site)
- Black Tiwabbitus / Kitsou Nakwatus / Kwakus : **Dégel** (seuil à 50 % PV : état **Gelé** puis **Dégel**, sorts différents selon
  l'état). Tofu Enneigé : Tombée de Neige (téléportation + zone terre), Débarbouillage (eau + -2 PO cercle 2). Kipik : Savapiké, Branche
  Kipik. Kitsou : Ruse Hivernale (+2 PM à tous les alliés).
- Sapik : 0 PM, pas d'invulnérabilité ; Kokapik (invoque un Kipik de Sapik chaque tour, max 5) ; Câlin Kipik (air, -10 fuite, croix 1) ;
  Enguirlandage (eau 35, -15 % CC, 2 PO → infinie).
### État sur le serveur
- 2.51 : les « Dégel du … » (898/899/900) existent mais demandent l'état **Fissuré**, qui n'est posé que par les marteaux d'Orelos
  (arme d'événement, sorts 897 / 3677) : pas de seuil de PV dans le client 2.51. Kokapik 911 invoque Pokipik 1180 et Kipik 849.
  Sapik 0 PM. Tous les sorts présents, rien de non géré, pas de cerveau nécessaire.
- [DOUTE VERSION] seuil 50 % / états Gelé-Dégel automatiques, sorts « gelés / dégelés » des monstres, Kipik de Sapik, limite de 5.
### À faire (précis, prêt à coder)
Rien à faire.

## Œil de Vortex (oeil-de-vortex) — boss Vortex (id 3835)
Base : spawn 397 « Ojo de Vórtex » : Vortex 3835, Ikargn 3834, Méjaire 3836, Harpille 3837, Buboxor 3838, Brabuzar 3839 ; Auroraire 3833.
### Mécanique officielle (site)
- Brabuzar : Décollage (-300 rés poussée), Mise en Situation (repousse 4 +200 dommages de poussée), Neutralisation (téléportation
  symétrique, -3 PM si dommages de poussée). Buboxor : **Bouclier Absorbant** (+100 puissance +1 PM par attaque à distance, max 3),
  Feinterception, Hoxor. Harpille : Petit poison (eau 550 sur tous, retiré par un soin, à partir du tour 4), Superfidie, Tirs optiques
  (dégâts ×2 de la prochaine attaque). Ikargn : Attraction ailée (attire 3, -3 PM, **Pesanteur**, vol agi), Cercle de feu, Terre Mythe.
  Méjaire : Envolupté, Plumière (+50 % dégâts subis), Rayonirique (**Pacifiste**).
- Vortex : 0 PM, indéplaçable, invulnérable ; ressuscite chaque monstre tué ; un monstre tué 2 fois à la même heure de l'**Auroraire**
  est éliminé ; bonus d'heure (12 heures) ; glyphes des monstres (échange + l'heure avance + bonus) ; « En temps et en heure » dans les
  lignes de l'Auroraire ; 5 vagues ; tour à vide puis vulnérable, Heurage (téléportation à l'Auroraire tous les 3 tours), Heuristique,
  Morfaille, Contamination zombie.
### État sur le serveur
- `VortexBrain.cs` (fait le 17/10 avec les choix validés par l'utilisateur, voir son en-tête) : Auroraire avancée au début du tour de
  chaque personnage, résurrection 50 % / Zombi, élimination à la même heure, bonus d'heure au Vortex, glyphes, vagues, tour à vide,
  Heurage. Tous les sorts 2.51 présents, rien de non géré.
- Écarts connus et VOULUS (choix utilisateur) : les monstres ressuscités ne prennent pas le bonus d'heure ; bonus d'heure pour le Vortex.
- [DOUTE VERSION] « depuis la 3.7, les monstres éliminés ne sont plus réinvoqués », chiffres du site (700-1200).
### À faire (précis, prêt à coder)
Rien à faire (mécanique codée sur choix de l'utilisateur ; ne pas y toucher sans sa demande).

## Palais du Roi Nidas (palais-du-roi-nidas) — boss Roi Nidas (id 3648)
Base : spawn 390 « Palacio del Rey Nidas » : Roi Nidas 3648, Kamastérisk 3653, Barbétoal 3654, Lévitrof 3655, Paspartou 3656,
Piloztère 3657 ; Boubourse du Nidas 3650 / des joueurs 3649.
### Mécanique officielle (site)
- Barbétoal 3654 : Barbe à trucs (neutre 400, +15 de base à chaque lancer) ; Super saillant (tour suivant : +2 PM +300 puissance
  **Intaclable**). Kamastérisk 3653 : Assomnolence (terre 700 + 20 % érosion) ; Clignotement (téléportation 3 PO + air vol de vie
  cercle 2) ; **Partoutatice** (début de tour : **Pesanteur** cercle 4, attire au contact les cibles en ligne). Lévitrof 3655 : Kama
  Haméa (feu + 50 % des PV érodés) ; **Reflets placides** (la cible meurt au tour suivant du Lévitrof si elle ne le frappe pas avant ;
  le Lévitrof échange de place avec le prochain qui le frappe). Paspartou 3656 : Clémence (eau ; **Invulnérable** aux ennemis au contact de
  la cible… côté monstres), Guidéal (+400 puissance cercle 2), La clef du succès (+1 PM aux ennemis au contact). Piloztère 3657 :
  **Austérité** (frappé : 1000 de bouclier à tout le monde à 1-2 PO), Claque (air 800, tue une invocation), Sourire Ravageur (-3 PM, +2 PA).
- Roi Nidas : Attrape-mutin (attire 6 + échange), Confusion (zone air + état **Zizanie** : au tour suivant les touchés frappent et
  désenvoûtent de 2 tours autour d'eux), Liquidation (feu + eau 20 % de ses PV érodés, croix diagonale), Mon Précieux (échange avec la
  Boubourse). **Boubourse** : au contact du Nidas au départ, boost les monstres ; à 1 PV change de camp 2 tours, puis revient.
- Vagues : tours 1, 6, 11, 16, 21 (vagues 2+ à -25 % PV, invulnérables jusqu'à leur 1er tour).
### État sur le serveur
- Version 2.51 (client, différente du site) : **Toucher de Nidas 4486** (relance 63, une fois) sur un joueur → 4478 niv.2 invoque la
  Boubourse 3650 sur la case fixe 316 ; **Boursicotage 4920** (Nidas : 50 % des dégâts subis répartis sur la Boubourse 3650, effet 1123) ;
  **Changement de camp 4485** ; Boubourses 4484 / 4482 (Coffrage, Cadeau Empoisonné, Vol de Boubourse au tueur, Retour de Boubourse) ;
  Kamastérisk Partoutatice 4514 ; Piloztère Austérité 4529 ; Lévitrof Reflets placides 4521 (mort différée de 1 tour, effet 333).
- Cerveaux existants (`DimensionsBrains.cs`) : RoiNidas (4920, 4485), BoubourseJoueurs 3649 (4482), BoubourseNidas 3650 (4484),
  Kamastérisk 3653/3719 (4514), Piloztère 3657 (4529). Zones « ;316 » gérées (`CasesFixesSorts.cs`). Outil : rien de non géré.
- Problèmes restants :
  - bug moteur B2 : tous les transferts de Boubourse reposent sur `o` (4483 `A,o` / `a,o`, 4512 `L,o` / `J,o` / `g,o`) : aujourd'hui la
    Boubourse va (ou est invoquée) chez tous les joueurs SAUF le tueur, et chez personne si on est seul ;
  - Toucher de Nidas 4486 est laissé à l'IA : sa catégorie vient du sous-sort (invocation) alors que la cible doit être un JOUEUR (`L`) ;
    si l'IA le lance sur une case vide, la relance 63 est consommée et la Boubourse n'apparaît jamais. Non vérifié en jeu.
- [DOUTE VERSION] seuil à 1 PV / retour au bout de 2 tours, « Cadeau Empoisonné » +200 puissance +30 % CC (2.51 : bouclier 500 /
  érosion 40 / +2 PM selon le camp), Super saillant à effet différé, Guidéal à délai initial, tableau des vagues et PV du site.
### À faire (précis, prêt à coder)
1. [CRITIQUE] Bug moteur B2 (masque `o`, voir en tête de fiche).
2. [IMPORTANT] Si en jeu la Boubourse n'apparaît pas au tour 1 : dans `RoiNidasBrain` (DimensionsBrains.cs), au `FightStarted`, retirer
   4486 de l'IA (`Fighter.Spells.Remove(4486)`) et le lancer soi-même sur le premier personnage vivant :
   `Fighter.CastAutoSpell(new Spell(4486, 1), cible.Cell)` avec `cible = Fighter.OpposedTeam.Fighters.OfType<CharacterFighter>().FirstOrDefault(x => x.IsAlive())`.
   (classe à transformer en `Brain` avec constructeur + `SortPassif.Poser(f, 4920)` / `Poser(f, 4485)` pour garder les passifs.)

## Potager d'Halouine (potager-dhalouine) — boss Halouine (id 3306)
Base : spawns 170-174 « Halouine » : Chauffe-Soutrille 3311, Lanverne 3310, Champêtrouille 3309, Cauchemarakne 3308, Dévhorreur 3307
(salle 4), Halouine 3306 (salle 5).
### Mécanique officielle (site)
- Cauchemarakne : Mort Sûre (feu, -10 esquive PM, -10 % rés, **Affaibli** 2 t) ; Toile Tisse-Trouille (glyphe damier 4, 2 t :
  **Emberlificoté** + -4 PM aux joueurs qui y finissent leur tour, +2 PM aux monstres). Champêtrouille : Cueillette (attire 4, -10 fuite),
  Frappe à Dingues (eau, -5 % CC ; en CC état **Dingue**), Dinguerie (seulement en Dingue : repousse 4 puis s'attire de 3, -40 rés poussée).
  Chauffe-Soutrille : Faux-Fuyant (esquive 50 % en mêlée en reculant, +10 tacle cercle 2), Pus des Pieds (poison air par PM utilisé).
  Dévhorreur : Malheur (croix 1 : si un porteur est soigné, -50 % vitalité à tous les porteurs), Terreur (-20 fuite à chaque tentative de
  détacler), Torpeur (-4 PM, +1 PM à chaque coup reçu). Lanverne : Subterfuge (invisible + poison 25 % de ses PV manquants), Vermifuge.
- Halouine : Moissonnage (terre vol de vie + repousse 1, +20 % vitalité 3 t cumulable 5×, peut le tuer à l'expiration) ; **Plantes
  Zombies** (dès le tour 3, tous les 2 tours : ressuscite le dernier allié mort à 1 % PV, **Invulnérable** 1 t) ; Râttirance (attire 5 +
  **Pesanteur**) ; Citwouille Explosive ; Peste Issyde (**Poutchicide** : 50 % des dégâts subis renvoyés en air autour de la cible).
### État sur le serveur
- Tous les sorts présents et identiques à la description (la refonte du site est déjà celle du client 2.51). Outil : rien de non géré
  (1125 Poutchicide géré par `RenvoiDommagesBruts.cs`, 780 par `ReviveActor.cs`).
- Plantes Zombies 3364 : la résurrection marche (initialCooldown 2, relance 2) mais l'état Invulnérable (`950 a,U`) n'est jamais
  posé : bug moteur B3 (un monstre ressuscité n'est pas « invoqué »).
- À vérifier en jeu (non bloquant) : à l'expiration du bonus de vitalité de Moissonnage (1078, 3 t), Halouine doit mourir si le bonus
  dépasse ses PV restants (comportement officiel décrit par le site, non vérifié dans le moteur).
### À faire (précis, prêt à coder)
1. [IMPORTANT] Bug moteur B3 (`JustSummonedCriterion`, voir en tête de fiche) : rend l'invulnérabilité 1 tour du monstre ressuscité.

## Pyramide d'Ombre (pyramide-dombre) — boss Ombre (id 3564)
Base : spawns 350-354 « Pirámide de Sombra » : Noctulule 3566, Sombléro 3570, Brutopak 3568, Panterreur 3567, Caznoar 3569, Ombre 3564
(salle 5). Invocations : Globilum 3563, Silhouette 3565.
### Mécanique officielle (site)
- Brutopak : Manigance (**Pesanteur** à tous, **Indéplaçable** sur lui, attire + eau), Mesquin Pacte (terre càc, -rés poussée), Brutalité
  (échange + air). Caznoar : Détentrave (retire des PA à qui le frappe à distance, +2 PM), Beclipse (eau, -PA, repousse 3), Aigriffure (-1
  tour d'effets). Noctulule : début de tour soin 10 % / érosion cercle 2 (site), Timatum, Bouligane (vol PM). Panterreur : **Planque**
  (invisible, renvoi 200 2 t ; poussé : redevient invisible avec ses alliés cercle 2), Vent d'état (-10 PM), Mesquinerie. Sombléro :
  Vaguichage (boosts ciblés : Brutopak **Invulnérable**, Caznoar/Noctulule +PO et retrait, Panterreur +200 puissance), Résonance, Faille.
- Ombre : **Invulnérable** et **Lourd** ; invoque le **Globilum** (invulnérable, indéplaçable, glyphe blanche cercle 3 à la fin de son tour,
  +20 % dommages finaux dedans) qui invoque la **Silhouette** (Lourde, feu % PV érodés, -10 PM + 20 % érosion) ; Éclairage chaque tour
  (dégâts autour, +50 dmg, +1 PM au Globilum, repousse 1) ; **Encrage** (Ombre et Silhouette déplacés : Indéplaçable + puissance 1 t) ;
  Ombre vulnérable tant que la Silhouette est DANS la glyphe et Ombre DEHORS. Sorts : Liaison, Distorsion, Pénombre, Paranoïa.
### État sur le serveur
- 2.51 (client) : passifs **Ex Nihilo 4104** (Ombre : invoque le Globilum, Invulnérable, Lourd, Éclairage 4105 relancé chaque tour par
  délai 1, glyphe aura 8830 → 4105 niv.2 = état « Ombre et Lumière » 536 + 20 % dommages finaux, Encrage 4115, Annulation 4108,
  EON536 / EOFF536 : invulnérable dans la glyphe) et **Détachement 4117** (Silhouette : dans la glyphe → retire l'invulnérabilité d'Ombre
  s'il n'y est pas, état Lumière 537). Cerveaux `OmbreBrain` (4104) et `SilhouetteBrain` (4117) déjà en place. Outil : rien de non géré
  (220 renvoi, 1171, MA|P gérés).
- Problèmes restants :
  - bug moteur B1 : la glyphe aura du Globilum applique l'état 536 et les +20 % à tout le monde (monstres compris) au lieu des masques
    (`a,F3565,F3564` pour l'état, `A` pour les +20 %) ;
  - bug moteur B2 : Encrage (`A,o,*F3564`) : l'indéplaçabilité + puissance d'Ombre après un déplacement est lancée par les joueurs
    qui ne l'ont PAS déplacé (seul : jamais ; à 4 : 3 fois, cumulée) ;
  - Annulation 4108 (retour à la case précédente) a le déclencheur `MS`, mappé sur `BuffTriggerType.Unknown_6` jamais déclenché →
    jamais active. [DOUTE] sens exact de `MS` inconnu (probablement « déplacé par échange / téléportation ») : non codable proprement.
- [DOUTE VERSION] soin/érosion de début de tour du Noctulule (absent du sort 2.51 Timatum), Paranoïa (absent du client 2.51),
  chiffres de Liaison / Distorsion du site.
### À faire (précis, prêt à coder)
1. [CRITIQUE] Bug moteur B1 (`GlyphAura.Enter`) : sans lui l'état 536 touche aussi les joueurs et les monstres ; Ombre / Silhouette
   fonctionnent quand même, mais les +20 % vont aux mauvaises cibles.
2. [IMPORTANT] Bug moteur B2 (masque `o`) pour Encrage.
3. [MINEUR] [DOUTE] Déclencheur `MS` d'Annulation 4108 : à laisser tant que son sens n'est pas établi.

## Donjon des Rats de Bonta (donjon-des-rats-de-bonta) — boss Rat Blanc (id 940)
Base : spawns 197-201 « Rat Bonta » : Rat Basher 938, Rat Croc 942, Rat Bajoie 941, Rat Blanc 940.
### Mécanique officielle (site, ancienne version, supprimée du jeu en 2.62 donc bien celle de la 2.51)
- Rat Croc (boost PA, terre/feu en ligne), Rat Basher (soin, eau vol de vie), Rat Bajoie (boost PM, terre càc).
- Rat Blanc : 0 % rés ; +40 CC ; **Peste Blanche** (soigner la cible la désenvoûte) ; **Rascasse** (2 t : +50 % de rés dans l'élément de
  chaque coup reçu, cumulable, relance 3 – 2.51 : relance 5).
### État sur le serveur
- Tous les sorts présents : Rascasse 1010 (210-214 avec déclencheurs DE/DW/DF/DA/DN, gérés par `AddStatBuff` → buff déclenché),
  Peste Blanche 1011 (132 sur soin H). Rien de non géré, pas de cerveau nécessaire. Conforme.
### À faire (précis, prêt à coder)
Rien à faire.

## Donjon des Rats de Brâkmar (donjon-des-rats-de-brakmar) — boss Rat Noir (id 939)
Base : spawns 202-206 « Rat Brak » : Rat Klure 937, Rat de Marais 935, Rat Batteur 936, Rat Noir 939.
### Mécanique officielle (site, ancienne version = 2.51)
- Rat de Marais (faiblesse terre/feu, terre/feu en ligne), Rat Klure (vol PA, eau vol de vie + repousse 2), Rat Batteur (boost dmg, air càc).
- Rat Noir : **Kackitu** (le porteur meurt s'il reçoit un coup), **Peste Noire** (OS ou presque si le porteur est soigné), frappe fort au càc.
### État sur le serveur
- Kackitu 1006 (Kill déclenché DE|DN), Kackisoigne 1007 (soin déclenché DE|DN : corrigé dans `Heal.cs` le 17/10), Peste Noire 1009
  (87 = 50 % PV air sur soin), Rafale. Rien de non géré. Conforme.
### À faire (précis, prêt à coder)
Rien à faire.

## Repaire de Sphincter Cell / Rats du Château d'Amakna (donjon-des-rats-du-chacircteau-damakna) — boss Sphincter Cell (id 943)
Base : spawns 267-271 « Sphincter Cell » : Rat Goûtant 3346, Rat Masseur 3347, Rat Colleur 3348, Rat Pine 3349, Rat Fraîchi 3350,
Rat Caille 3351, Rat Botteur 3352, Ramane 3345, Sphincter Cell 943 (salle 5). Tortues invoquées : Raphaela 944, Donatella 945,
Michelangela 946, Leonardawa 947.
### Mécanique officielle (site, page ancienne « à réviser »)
- Monstres : Rat Colleur (-PA -PO), Rat Masseur (poussée, vol de vie, soin de zone), Ramane (malus PA/stats, boost PM), Rat Pine
  (malus retrait PM), Rat Fraîchi (malus fuite), Rat Botteur (**Affaibli**, boost dmg), Rat Caille (boost PM, +force quand frappée).
- Sphincter Cell : 200 % de rés partout ; se téléporte ; frappe dans tous les éléments ; invoque des tortues. Chaque tortue placée AU
  CONTACT du Cell, avec un personnage à son contact, lance **Kawabunga** : -200 % de rés dans SON élément (rouge feu, jaune air,
  verte terre, bleue eau) au Cell et aux personnages en croix 1, 2 tours. Site : accompagné du Rat Noir et du Rat Blanc.
### État sur le serveur
- Sphincter Cell 943 : rés 200 % (base = client), Mutagen 1018 (invoque UNE tortue au hasard, 4 × random 25, toujours au grade 1),
  Raccourci 1016, Rasoir 1017. Tortues : sort unique Kawabunga 1019 dont l'ÉLÉMENT dépend du NIVEAU : niv.1 air (217), niv.2 eau (216),
  niv.3 feu (218), niv.4 terre (215), niv.5 air ; zone croix 1, masque `A,g`, 3 tours, +600 vita sur elle 4 t.
- **Bug** : les 4 tortues sont toujours invoquées au grade 1 et, en base, le grade 1 de chacune a Kawabunga NIVEAU 1 → elles retirent
  TOUTES 200 % de rés AIR. On ne peut donc jamais ouvrir le Cell en feu, eau ou terre.
  Couleur / élément de chaque tortue déduits de deux indices concordants : noms (Raphaela = rouge, Leonardawa = bleue,
  Michelangela = jaune/orange, Donatella = verte) ET résistances en base (chaque tortue résiste à l'élément opposé du sien :
  Raphaela eau 30, Leonardawa feu 30, Michelangela terre 30, Donatella air 30). → Raphaela feu (niv.3), Leonardawa eau (niv.2),
  Michelangela air (niv.1), Donatella terre (niv.4).
- Pas de cerveau nécessaire (Kawabunga = sort d'IA : la tortue le lance quand un ennemi est à son contact, ce qui colle au site).
- [DOUTE VERSION] Rat Noir et Rat Blanc avec le Cell (absents du groupe en base, page « à réviser ») : ne pas ajouter sans décision.
  [DOUTE] « Tortues Rouge/Jaune/Verte/Bleue » 220-223 (sorts 6129-6140) : autres monstres du client, non utilisés ici.
### À faire (précis, prêt à coder)
1. [CRITIQUE] Base : niveau de Kawabunga des tortues au grade 1 (ids de grade vérifiés) :
   `UPDATE monsters_spells SET Level=3 WHERE MonsterGradeId=3242 AND SpellId=1019;` (Raphaela 944, feu)
   `UPDATE monsters_spells SET Level=4 WHERE MonsterGradeId=3247 AND SpellId=1019;` (Donatella 945, terre)
   `UPDATE monsters_spells SET Level=1 WHERE MonsterGradeId=3252 AND SpellId=1019;` (Michelangela 946, air — déjà 1, ligne pour mémoire)
   `UPDATE monsters_spells SET Level=2 WHERE MonsterGradeId=3257 AND SpellId=1019;` (Leonardawa 947, eau)
   + fichier -annuler (remet Level=1). [DOUTE] léger sur l'attribution couleur ↔ tortue : la vérifier une fois en jeu (couleur de la
   tortue invoquée et élément du malus affiché sur le Cell) ; si elle est fausse il suffit d'échanger les niveaux.

---

## Récapitulatif

| Donjon | CRITIQUE | IMPORTANT | MINEUR |
|---|---|---|---|
| Mine de Sakaï (Grolloum) | 0 | 0 | 0 |
| Centre du Labyrinthe (Minotoror) | 0 | 0 | 0 |
| Salle du Minotot | 0 | 0 | 0 |
| Arbre de Moon | 5 | 1 (à valider) | 0 |
| Nid du Kwakwa | 0 | 0 | 0 |
| Donjon de Nowel (Sapik) | 0 | 0 | 0 |
| Œil de Vortex | 0 | 0 | 0 |
| Palais du Roi Nidas | 1 | 1 | 0 |
| Potager d'Halouine | 0 | 1 | 0 |
| Pyramide d'Ombre | 1 | 1 | 1 |
| Rats de Bonta (Rat Blanc) | 0 | 0 | 0 |
| Rats de Brâkmar (Rat Noir) | 0 | 0 | 0 |
| Sphincter Cell | 1 | 0 | 0 |

Les bugs moteur B1 (GlyphAura) et B2 (masque `o`) sont comptés dans chaque donjon touché mais ne sont à coder qu'une fois.
