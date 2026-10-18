# Lot 8 — audit des mécaniques (dofuspourlesnoobs → Mystic 2.51)

Audit en lecture seule (rien n'a été modifié). Sources : pages du site (`scratchpad/dpln/txt/*.txt`), `analyse_mecaniques.py`
(client 2.51 + base `world`), code `stump.server.worldserver`.

**Règle version appliquée** : on ne propose de coder QUE ce qui existe dans le client 2.51. Tout ce qui est sur le site mais
absent du client 2.51, ou dont les chiffres diffèrent, est marqué **[DOUTE VERSION]** et n'est pas dans les corrections.

## Constat commun à tout le lot
- Les 13 donjons existent sur le serveur (`monsters_spawns_dungeons`), monstres et sorts « actifs » en base = sorts du client 2.51
  (listes `Monsters.d2o` identiques à `monsters_spells`).
- **Aucun boss du lot n'a de cerveau, sauf Ilyzaelle** (`IlyzaelleBrain.cs`). Or, dans les données 2.51, presque toute la
  mécanique de ces donjons est portée par des **sorts passifs non attribués** (présents dans le client, attribués à aucun
  monstre) : invulnérabilités, résurrections, états, glyphes, invocations de mécanique. Ils ne sont donc **jamais lancés**.
- Correctif général : mêmes outils que les lots précédents. Créer **un fichier**
  `SERVEUR_Emu/server/stump.server.worldserver/AI/Fights/Brain/Custom/Boss/Lot8PassifsBrains.cs` avec des cerveaux
  `BrainPassif` (classe existante dans `PassifsDonjonsBrains.cs`, qui appelle `SortPassif.Poser` : marche au début du combat
  ET pour un monstre invoqué / arrivé en cours de combat). Pour les passifs dont le niveau dépend du grade, ajouter dans ce
  fichier :

```csharp
using System;
using Stump.Server.WorldServer.Game.Actors.Fight;

namespace Stump.Server.WorldServer.AI.Fights.Brain.Custom.Boss
{
    /// <summary>MYSTIC lot 8 : pose un passif dont le niveau = grade du monstre (invoqué ou non), borné à [1, max].</summary>
    public abstract class BrainPassifGrade : Brain
    {
        protected BrainPassifGrade(AIFighter fighter, int spellId, int niveauMax) : base(fighter)
        {
            var grade = (fighter as MonsterFighter)?.MonsterGrade?.GradeId
                        ?? (fighter as SummonedMonster)?.MonsterGrade?.GradeId ?? 1;
            SortPassif.Poser(fighter, spellId, Math.Max(1, Math.Min(niveauMax, (int)grade)));
        }
    }
}
```

- Tous les effets / déclencheurs des passifs listés ci-dessous sont gérés par le moteur (sortie de l'outil = « rien »), sauf
  ceux signalés explicitement (147, 1126, `DT`, 1127, 1047, 2024, 421, et le comportement de 2794).
- Effets « délai » (ex. Goulala 8716, Rythme maudit 7840 qui se relancent eux-mêmes) : ils ont `delay = 2` dans le client, le
  moteur les gère (`Buff.DecrementDuration`), **pas de boucle infinie**.

---

## Tour de Bethel (tour-de-bethel) — boss Bethel Akarna (id 5110)
### Mécanique officielle (site)
- **Aquazomb (état de tous les monstres)** : chaque monstre tué est ressuscité par le monstre suivant. À partir du tour 2,
  chaque joueur doit finir son tour à ≤ 6 PO d'un monstre (invocations exclues), sinon tous les monstres gagnent +2 PM et
  +1 PO (1 tour, désenvoûtable) et chaque monstre a un bonus propre : Tournoyé invisible 2 t + téléportation aléatoire ;
  Crânonier +1 PO et +20 % dégâts distance 1 t ; Macrab bouclier 10 % PV 2 t ; Funespadon −120 dégâts distance subis 2 t ;
  Zombruth +40 esquive PM + **Inébranlable** 2 t.
- **Nécronyx** (sort joueur, quête « L'arme fatale ») : relance 2 t ; un monstre tué en état **Nécronyx** ne rejoue plus
  après résurrection. Il faut Nécronyx chaque monstre avant de le tuer, et Bethel avant sa mort sinon toute l'équipe meurt.
- Monstres : Crânonier 5114 (Morlusque air 200 + érosion 10 % + poison ; Hydraire eau 300 zone cercle 2, 6-12 PO, piège) ;
  Funespadon 5113 (Piquants renvoi 2 t ; Talion terre 300 + état **Talion** ; Profondeurs Marines −4 PO à > 9 PO) ;
  Macrab 5115 (Coup de Pince terre 350 + poison par PA ; Vapeur feu 300 cercle 2) ; Tournoyé 5112 (Franchissement
  téléport 7 cases + dommages subis ×120 % ; Restouté −2 PM, −40 fuite/tacle ; Monodent eau 450 + recul 2) ; Zombruth 5111
  (Siphon air 280 cercle 2 −300 puissance ; Cumul des Mandales terre 320 + −200 résistance poussée ; Aquaponey +3 PM +2 PO alliés).
- Boss : **Contrat Indicible** (début) : invoque un **Monolithe** (invulnérable à distance) et un **Dagon des Profondeurs**
  (invulnérable, **Intaclable**) ; Bethel **invulnérable** ; glyphe de bord de carte −100 PM + **Pesanteur**. Sorts : Pantang
  air 500-800 (1-12 PO) ; Vindicte (2 t) ; Offrande Monolithique (sur le Monolithe au càc : +300 puissance Bethel, +200 monstres,
  Amas de Tentacules 110 PV, relance 3) ; repousse si pas Nécronyx ; Étoile de Mer (glyphe étoile eau, −4 PA −2 PM, après la
  mort du Monolithe). Monolithe : **Hiéroglyphes** chaque tour (bouclier 20 % PV, repousse 2, +200 puissance invocations).
  Dagon : Présence Effrayante (−3 PA càc), Charge Aquatique (s'attire 8 PO en ligne, −60 fuite), Souffle Chaud (feu 600-900,
  −2 PA) ; poussé → **Inébranlable** jusqu'au tour suivant du pousseur. Tuer le Monolithe → le Dagon passe dans le camp
  joueur (« Dagon Insoumis ») ; **seul son Souffle Chaud au càc rend Bethel vulnérable**. Si ce Dagon meurt, Bethel réinvoque
  Monolithe + Dagon.
### État sur le serveur
- Sorts actifs des 6 monstres et de Bethel : conformes au client 2.51.
- **Rien de la mécanique n'est lancé** (pas de cerveau) : Bethel n'est pas invulnérable, pas de Monolithe/Dagon, pas d'Aquazomb.
  Le combat est donc un combat normal (beaucoup plus facile, pas conforme).
- Passifs 2.51 trouvés (client, non attribués) : **9542 Contrat Indicible** (5 niveaux = grade du Monolithe invoqué ;
  contient l'invulnérabilité de Bethel, le glyphe-aura 9558 de bord de carte, l'invocation Monolithe 5121 case 302 + Dagon 5122
  case 316, la résurrection 9350, la mort de toute l'équipe 9543 si Bethel meurt sans Nécronyx, le retrait de Nécronyx) ;
  **9450 Aquazomb** (monstres : mort → 9353, début de tour → 9350 résurrection/pétrification, fin de tour joueur → 9451 test des
  6 PO + bonus par monstre exactement comme le site) ; **9559 Hiéroglyphes** (Monolithe 5121) ; **9566 Présence Effrayante**
  (Dagon 5122 : invulnérable, Intaclable, Inébranlable sur poussée, à sa mort invoque le Dagon Insoumis 5123 côté joueurs).
- Souffle Chaud niveau 1 (5122/5123 invoqués en grade 1) contient bien `952 désactive état 56 (Invulnérable)` sur Bethel.
- Effets non gérés dans ces passifs : **147 « Ressuscite un allié »** (cœur de l'Aquazomb, dans 9350 niv. 3), **1126**
  (Pyrozomb 9352, sur coup reçu par Bethel, inconnu), déclencheur **`DT`** (dans `DG|DT` de Hiéroglyphes).
- **Nécronyx 9348 n'est obtenable nulle part** : la quête « L'arme fatale » n'existe pas dans `quests`, aucun
  `quests_rewards.SpellsRewardCSV` ne contient 9348, aucun personnage ne l'a.
- [DOUTE VERSION] 2.51 : Nécronyx **passe le tour** du lanceur (effet 1031) ; le site dit que non depuis la 3.7 et que Bethel
  retire Nécronyx au bout de 3 tours (MAJ 20/01/2026) : à ne pas coder.
### À faire (précis, prêt à coder)
1. [CRITIQUE] **Prérequis** : rendre le sort joueur **9348 Nécronyx** obtenable (ex. récompense de quête, PNJ d'entrée du
   donjon qui l'apprend, ou ajout en `characters_spells`). Sans lui, poser le passif du point 2 rend le boss **ingagnable**
   (9543 tue toute l'équipe si Bethel meurt sans Nécronyx). Ne pas livrer 2-5 sans 1.
2. [CRITIQUE] Cerveau Bethel : `[BrainIdentifier(5110)] class BethelAkarnaBrain : BrainPassifGrade { ...(f, 9542, 5) }`
   (le grade de Bethel choisit le niveau de Contrat Indicible = grade du Monolithe).
3. [CRITIQUE] Cerveaux des invocations de mécanique : `[BrainIdentifier(5121)]` Monolithe → `BrainPassif(f, 9559)` ;
   `[BrainIdentifier(5122)]` Dagon des Profondeurs → `BrainPassif(f, 9566)`. (5123 Dagon Insoumis : aucun passif en 2.51.)
4. [CRITIQUE] Aquazomb : `[BrainIdentifier(5111..5115)]` Zombruth, Tournoyé, Funespadon, Crânonier, Macrab →
   `BrainPassif(f, 9450)` (niveau 1 ; ne pas utiliser 10132, variante qui invoque 5150).
5. [CRITIQUE] Gérer l'effet **147** : nouveau handler `[EffectHandler(EffectsEnum.Effect_147)]` dans
   `game/effects/handlers/Spells/Others/` sur le modèle de `ReviveActor.cs` : prendre le dernier mort de l'équipe du lanceur qui
   est un `MonsterFighter` (pas une invocation), le placer sur une case libre de la zone de l'effet (sinon case adjacente libre),
   `Revive(100, Caster)` (diceNum = 0 dans les données : 100 % PV **[à confirmer]**), `ReinitialiserHistoriqueSorts()`,
   `SendGameActionFightReviveMessage` + `SendGameFightTurnListMessage` (copier `GrolloumBrain.Ressusciter`). Les sorts suivants
   de 9350 (pétrification si **Nécronyx**) s'appliquent ensuite tout seuls. **Fin de combat** : ajouter dans `BethelAkarnaBrain`
   une vérification à chaque mort / fin de tour : si tous les monstres vivants de l'équipe de Bethel sont en état 571 (Nécronyx)
   et pétrifiés (Bethel mort ou pétrifié), les tuer (`Die()`) pour terminer le combat — sinon des statues invulnérables
   bloquent la victoire [DOUTE : règle serveur officielle non visible dans les données].
6. [MINEUR] Effet **1126** (Pyrozomb, valeur 50, cible ennemis en carré 1 si Bethel n'est pas Nécronyx) : sens inconnu ;
   à laisser tant qu'il n'est pas identifié.
7. [MINEUR] Déclencheur **`DT`** : l'ajouter dans `TriggerBuff.cs` (probablement « dommages de piège », à mapper comme `DG`
   sur les dommages de piège) pour l'armure 5000 de Hiéroglyphes.

## Brasserie du roi Dazak (brasserie-du-roi-dazak) — boss Dazak Martegel (id 5319)
### Mécanique officielle (site)
- **Nimpatience** (tous les monstres) : chaque tour sans frapper un **personnage** → +1 niveau (**Nimbos 1/2/3**, infini,
  cumulable). Bonus par niveau : Barbélier +1 PM ; Boufbos +50 % dommages finaux ; Kasrok +1 PM ; Tanklume dommages subis ×75 % ;
  Vatenbière +3 PO ; Dazak +50 % dommages finaux et +1 PM. Au niveau 3, effet bonus (Coup de cornes double + −25 % vitalité,
  Lance-pierre double + Pacifiste, Marteau pilon double + −7 PA zone 5, Tanklume **Ninfluence**, Vatenbière **Nimplantation**
  −100 PM) puis retour à 0 quand le sort est utilisé.
- Monstres : Barbélier 5316 (Bélier s'attire 6 PO + bouclier ; Coup de cornes feu 1200 càc + 20 % érosion) ; Boufbos 5315
  (Forte-tête −4 PM, repousse 2, recule 2 ; Lance-pierre air 1000, 3-12 PO en ligne) ; Kasrok 5317 (Marteau pilon eau 700 cercle
  3, −4 PA ; Nimpulsion −50 % critique, pousse 4, s'attire 4) ; Tanklume 5314 (Ninrmure échange + intercepte + bouclier ;
  Triple attaque neutre 800 bâton, −50 fuite) ; Vatenbière 5318 (Gigantosité Pesanteur ; Hachis terre 800 croix 3, −6 PO).
- Boss : Empalement royal (neutre 1500 càc, 40 % érosion, −10 % résistances infini), Ninfiltration (bond 5, air 900, −4 PM),
  Nintrépidité (alliés +2 PM +1500 PB, tour 3 puis tous les 2 t). Résistances > 100 %. **Les 2 tours après son Nimpatience 3 :
  −5 % résistances (infini) à chaque fois qu'il est poussé/attiré, +10 % s'il subit des dommages de poussée.**
### État sur le serveur
- Sorts actifs des 6 monstres : conformes au 2.51 (les variantes Nimbos 3 sont déjà dans les sorts, conditionnées par l'état 695).
- Résistances de Dazak en base : 140/130/120/105/110 % (conforme : il est **intuable** sans la mécanique).
- **Passif 10346 Nimpatience jamais lancé** (pas de cerveau) : niv. 1 = fin de tour → 10346/2 (monte le niveau 10332 si le
  monstre n'a pas l'état 696 « ennemi touché ») ; début de tour (Dazak seulement, F5319) → 10346/3 (Nimbos 3 : +3 PM, +150 %
  dommages finaux 2 t, −5 % résistances sur déplacement `M`, +10 % sur dommages de poussée `PMD`). Les bonus par niveau
  (10332 niv. 2) ciblent bien 5314-5319. Effets/déclencheurs tous gérés (`M` = déplacé par un autre, `PMD` existe).
- Sans ce passif : aucun monstre ne monte en Nimpatience, les sorts « niveau 3 » (Ninfluence, Nimplantation) ne sont jamais
  utilisables et **Dazak ne perd jamais ses résistances → combat impossible à gagner**.
- [DOUTE VERSION] chiffres de dégâts du site (1200, 1000, 700…) vs 2.51 : non comparés au point près, ne pas corriger.
### À faire (précis, prêt à coder)
1. [CRITIQUE] `[BrainIdentifier(5319)]` DazakBrain → `BrainPassif(f, 10346)` (niveau 1).
2. [IMPORTANT] Même passif 10346 niveau 1 pour Tanklume 5314, Boufbos 5315, Barbélier 5316, Kasrok 5317, Vatenbière 5318
   (5 classes `BrainPassif(f, 10346)`).

## Chambre de Tal Kasha (chambre-de-tal-kasha) — boss Tal Kasha (id 4744)
### Mécanique officielle (site)
- **Maudification (tous les monstres)** : quand on frappe un monstre, le monstre le plus proche de lui entre en état
  **Maudification** 1 t : dommages subis à distance ×50 %, −2 PM ; retiré par des dommages de poussée.
- Monstres : Bandleth 4740 (**Morve érosive** : la pousser → 40 % érosion + poison 100 neutre 2 t ; Calin feu 700 s'attire 5 ;
  Danse comme une momie terre 450 cercle 3 + vol 100 puissance) ; Chakanoubis 4739 (**état Chakanoubis** : frappé → +10 PM 1 t ;
  Harcèlement air 200 −4 PA −3 PM PO infinie ; Pyrâmide eau 900 càc + Pacifiste, seulement le tour après avoir été frappé) ;
  Griffotep 4743 (Aigriffes terre 500 repousse 6 +100 dommages poussée ; Arrivée fracassante téléport diagonale air 350) ;
  Momistik 4741 (**Héritage Huppermage** : frappé dans un élément → état de l'élément, −50 % dans cet élément 1 t ; les 4 états
  → bonus sur ses sorts ; Frappe élémentaire neutre 500 ; Grenattrition neutre 400 ligne 4 + Pesanteur/Affaibli) ;
  Rykaon 4742 (Remplacement maudit échange + eau 600 ; Sabrupt feu 600 vol 100 puissance ; Indéplaçable au tour 1, +2 PM tous
  les 3 tours).
- Boss : **Malédiction de la pyramide** (invulnérable ; ressuscite les monstres morts au tour suivant sur leur case, glyphe
  marron à la mort, −1 PM au monstre ressuscité qui passe son tour ; échange de place avec qui lui retire des PM, 1×/tour).
  Faire passer Tal Kasha sur 3 glyphes de 3 monstres différents (états I-III) → vulnérable définitivement + sort Cheveux partir
  de là (téléport 4). Filature feu 500 attire 4 ; Transe-Perse neutre 1700 càc ; les deux lançables « depuis » n'importe quel allié.
### État sur le serveur
- Sorts actifs : conformes (Transe-Perse/Filature « via un allié » = 7909/7907 déjà en base).
- **Aucun passif lancé** (pas de cerveau). Passifs 2.51 trouvés : **7868 Malédiction de la pyramide** (boss ; invulnérable,
  rune 8082 de résurrection 50 % PV −1 PM à la mort d'un allié, déclenchement des runes en fin de tour, échange de place sur
  retrait PM, Sceau I/II → retrait de l'invulnérabilité) ; **8049 Maudification** (base commune) ; **7826 Pouvoir antique**
  (Chakanoubis : frappé → état Chakanoubis +10 PM, inclut 8049) ; **7829 Morve érosive** (Bandleth, inclut 8049) ;
  **7833 Héritage Huppermage** (Momistik, inclut 8049) ; **7840 Rythme maudit** (Rykaon : Indéplaçable 1 t, −2 PM différé,
  relance tous les 2 tours, inclut 8049). Griffotep : aucun passif propre → 8049 seul.
- Effets 2022 (rune), 2023 (déclenche runes), 1023, 666/2018 (visuels) : gérés.
- Sans ces passifs : Tal Kasha est tuable normalement, aucune résurrection, aucun état de monstre.
- [DOUTE] la résurrection 780 utilise `GetLastDeadFighter()` : une rune peut relever un autre monstre que celui mort sur cette
  case si deux meurent avant le tour de Tal Kasha. Acceptable.
### À faire (précis, prêt à coder)
1. [CRITIQUE] `[BrainIdentifier(4744)]` TalKashaBrain → `BrainPassif(f, 7868)`.
2. [IMPORTANT] Chakanoubis 4739 → `BrainPassif(f, 7826)` ; Bandleth 4740 → `BrainPassif(f, 7829)` ; Momistik 4741 →
   `BrainPassif(f, 7833)` ; Rykaon 4742 → `BrainPassif(f, 7840)` ; Griffotep 4743 → `BrainPassif(f, 8049)`.

## Fonderie des Waddicts (fonderie-des-waddicts) — boss Mawabouaino (id 5653)
### Mécanique officielle (site)
- Monstres (Twakeuf 5652, Wadnozéam 5689, Waccro 5690, Warkaïk 5691, Wadulant 5692) : sorts simples (Abwowption vol 100 vita,
  Souffle Liqueuwant, Spiwitueuw, Gwokwik, Stéwoïdes, Skwat, Afwiandage, Pouwsuite, Wagglutinant, Céléwité +2 PM, Secouwiste,
  Oeufowie, Chokobombawde, Ombwage, Twanchée).
- Boss : Cacaobstwuant terre 120 −1 PM ; Chocohowte invoque un Twakeuf ; Chocolave feu 50 −20 % rés. terre ; Éclat terre 120
  cercle 2 repousse 2. **Fondu Déchaîné** : à la mort d'un monstre, glyphe rouge sur sa case → **Chocomatose** (+300 puissance,
  +3 PA, 2 t) pour qui y passe ; glyphes disparaissent si le boss meurt. Joueur en Chocomatose → les monstres gagnent **Mowfal**
  (téléport au càc d'un ennemi).
### État sur le serveur
- Sorts actifs conformes (Mowfal 12216 est dans la liste de chaque monstre).
- Passif 2.51 **11944 Fondu Déchaîné** (boss : pose 11964 sur tous ses alliés → à leur mort, le boss pose le glyphe 11945
  Chocomatose) **jamais lancé**. Seul le cast de Chocohowte (12183) le pose, et seulement sur les alliés présents à ce moment.
- [DOUTE VERSION] Mowfal n'a **aucune condition d'état** dans le client 2.51 (ni état requis ni cible conditionnée) : en 2.51
  les monstres peuvent l'utiliser à tout moment (1×/tour). Ne pas ajouter la condition Chocomatose du site.
### À faire (précis, prêt à coder)
1. [IMPORTANT] `[BrainIdentifier(5653)]` MawabouainoBrain → `BrainPassif(f, 11944)`.

## Croquanterie (croquanterie) — boss Croqueleur (id 5664)
### Mécanique officielle (site)
- **Morcelage (tous les monstres)** : frappé → invoque parfois une **Pépite croquante** (200 PV, immobile) à 2 PO. Mort d'une
  pépite → alliés au càc +50 puissance et bouclier 2 t. Un monstre qui la mange (Croquanibalisme) gagne +1 PA, soin, et un bonus
  propre (Chocoskargo invisible, Glourson +bouclier/tacle, Kakoalak Pluie de Pépite +1 lancer, Kwakao +3 PO **Kwakalorie**,
  Mansocolat +50 puissance +1 PM).
- Monstres : Chocoskargo 5680 (Enrobage, Glycémie, Chocoskarfarce), Glourson Guimauve 5679 (Attrape Gourmand, Englumauve,
  Guimimauve), Kakoalak 5677 (Pluie de Pépite, Kakoaklake), Kwakao 5681 (Kwakaoust, Skwalala), Mansocolat 5678 (Mansaut,
  Maskansocolat, Mansocolère).
- Boss : **Croquelure** au début : **invulnérable** + 4 états. Le frapper dans l'élément de la phase (feu → air → eau →
  terre/neutre, états Fondu/Soufflé/Lavé/Brisé) puis le pousser → invoque une **Pépite du Croqueleur** (invulnérable à distance
  / 70 / 40 / 10 % rés. distance) à tuer en 1-2 tours ; chaque pépite tuée = −25 % PV du boss et changement de phase (+1 PA
  +1 PM −20 puissance) ; la 4e tuée → le Croqueleur meurt. Croustichoc, Total Impwâkt, Attraction gourmande changent
  d'élément/effets selon la phase.
### État sur le serveur
- Sorts actifs conformes (les 4 variantes par phase sont dans les sorts, conditionnées par les états 1088-1091).
- PV en base : Croqueleur 1 PV + vitalité 9999-19999 selon grade (= client 2.51).
- **Aucun passif lancé**. Passifs 2.51 trouvés : **12290 Croquelure** (boss : invulnérable, 4 états, chaîne élément → poussée
  `M|MS` → invocation Pépite 5665 de niveau 1→4, changement de phase) ; **12297 Rocher croquant** (Pépite du Croqueleur, 4
  niveaux = grade 1-4 avec lequel elle est invoquée : rés. distance / invulnérabilité distance ; à sa mort, phase suivante, et au
  niveau 4 **tue le Croqueleur** — effet 141 sur F5664) ; **12023 Morcelage** (monstres : sur dommage subi, invoque 5676) ;
  **12024 Croquez-moi** (Pépite croquante 5676 : à sa mort, +50 puissance et bouclier 5 % au càc).
- Sans 12290 le boss est un monstre normal ; avec 12290 mais sans 12297, le combat est **bloqué** (boss invulnérable, aucune
  pépite ne fait avancer la phase) → les deux vont ensemble.
- [DOUTE VERSION] Pépite du Croqueleur 5665 : 1 PV (client 2.51 et base), le site dit 25-55 % des PV du boss. Ne pas modifier.
- [DOUTE VERSION] Morcelage 2.51 invoque une pépite à **chaque** coup reçu (pas d'aléatoire dans les données).
- [DOUTE] Le site dit « tous les monstres » ont Morcelage ; le Croqueleur (invulnérable) n'est pas inclus ci-dessous.
### À faire (précis, prêt à coder)
1. [CRITIQUE] `[BrainIdentifier(5664)]` CroqueleurBrain → `BrainPassif(f, 12290)`.
2. [CRITIQUE] `[BrainIdentifier(5665)]` PepiteCroqueleurBrain → `BrainPassifGrade(f, 12297, 4)` (niveau = grade d'invocation).
3. [IMPORTANT] Kakoalak 5677, Mansocolat 5678, Glourson Guimauve 5679, Chocoskargo 5680, Kwakao 5681 → `BrainPassif(f, 12023)`.
4. [IMPORTANT] `[BrainIdentifier(5676)]` Pépite croquante → `BrainPassif(f, 12024)`.

## Temple du dieu Kao (temple-du-dieu-kao) — boss Prêtresse de Kao (id 5666)
### Mécanique officielle (site)
- **Chocolatomanie (tous les monstres)** : invoque un **Éclat Kao** (1 seul à la fois, 1000 PV +500/joueur 1 t, meurt seul au
  bout de 2 t). Tant qu'il vit : −50 % dommages subis aux monstres, bonus par monstre (Cabosseur +4 PO, Chocoligarque +2 PM,
  Pralicienne +50 tacle, Températurge +50 dommages, Torréfactueur **Indéplaçable**, Prêtresse **Intaclable**), invocateur
  **invulnérable**. Frapper l'éclat → **Chocolathée** 3 t (sorts des monstres boostés, Cloches du Kao +50/joueur).
- Monstres : Cabosseur 5672 (Piège à gourmands, Éclabossage, Fève du samedi soir) ; Chocoligarque 5668 (Ganache,
  Cacaobstruction, Chocolat Sperger) ; Pralicienne 5670 (Chococlier, Glaçage, Hydrolyse) ; Températurge 5671 (Refroidissement,
  Surchauffe, **Choc Thermique**, Chaudron Choco) ; Torréfactueur 5669 (Torréfaction, Torréador, Padbra).
- Boss : **Artisanat du Kao** (invulnérable) ; Cloches du Kao (neutre 300 toute la carte, tours impairs), Equador, Spatule
  Tranche-Gourmands, Commerce Inéquitable. Tuer 3 éclats → états **Forastero / Trinitario / Criollo** (+52/74/99 % dommages
  finaux) puis vulnérable.
### État sur le serveur
- Sorts actifs conformes (variantes Chocolathée = état 964 déjà dans les sorts).
- **Aucun passif lancé**. Passifs 2.51 : **12311 Artisanat du Kao** (boss : invulnérable + lance 11994) ; **11994
  Chocolatomanie** (monstres : début de tour → invoque l'Éclat 5667 si pas déjà invoqué, état 968) ; **11993 Éclat Kao**
  (Éclat : +500 vitalité 1 t par joueur, invulnérabilité de l'invocateur, bonus 11996, Chocolathée 12335 sur qui le frappe, à sa
  mort 12313 → Forastero/Trinitario/Criollo et retrait de l'invulnérabilité au 3e).
- Sans eux : Prêtresse tuable normalement, aucun éclat.
### À faire (précis, prêt à coder)
1. [CRITIQUE] `[BrainIdentifier(5666)]` PretresseKaoBrain → `BrainPassif(f, 12311)`.
2. [CRITIQUE] `[BrainIdentifier(5667)]` EclatKaoBrain → `BrainPassif(f, 11993)` (invocation : posé à l'arrivée par SortPassif).
3. [IMPORTANT] Chocoligarque 5668, Torréfactueur 5669, Pralicienne 5670, Températurge 5671, Cabosseur 5672 →
   `BrainPassif(f, 11994)`.

## Belvédère d'Ilyzaelle (belvedere-d-ilyzaelle) — boss Ilyzaelle (id 4967)
### Mécanique officielle (site)
- **Brûlure** : déplacer un monstre ou tenter de lui retirer des PM donne l'état **Brûlure**, consommé par un sort spécial :
  Chause Doublâme (−50 % critique), Crâme Lance lumière (retour position début de tour + dommages subis +150 %), Ectorche
  Combustion spontanée (feu 1100 croix 2 autour), Esprigné Sens spectral (révèle), Feutôme Revenant (s'attire + vol 4 PM).
- Monstres : Chause 4968 (Ébullition, **Flâmi** partage), Crâme 4972 (Boule d'eau, **Frustration** : +3 PM 2 t et +100 %
  dommages finaux 4 t s'il n'a pas frappé), Ectorche 4969, Esprigné 4970, Feutôme 4971.
- Boss : Hantâme, Lance de l'effroi (Pacifiste), Vers les ténèbres (attire 10). **Possession** : monstres +300 % vitalité,
  vagues de 2 ou 3 monstres tous les 3 tours selon le nombre de joueurs ; à < 75 % puis < 50 % PV, elle passe dans le camp joueur
  2 tours (glyphe +400 % dommages, monstres dedans 15000), puis à la 2e fois tue tous les monstres (victoire). 250 rés. critique.
### État sur le serveur
- Cerveau existant `IlyzaelleBrain` : lance **Possession 9052 niveau 1** au début, rés. critique +250, limite d'invocations.
  `KingsGuard` (9048) gère la téléportation « Défense des Rois ». Mécanique du boss : présente.
- Passifs de monstres 2.51 **jamais lancés** : **9078 Brûle-âme** (Chause, Ectorche, Esprigné, Feutôme : sur déplacement /
  retrait PM → Brûlure à l'attaquant) et **9075 Frustration** (Crâme : inclut Brûle-âme + bonus de frustration 9082). Sans eux
  aucun joueur ne reçoit Brûlure → les 5 sorts spéciaux ne servent jamais. (Ilyzaelle a Brûle-âme via Possession.)
- Possession a 5 niveaux : niv. 1-2 = vagues de 2 monstres, niv. 3-5 = 3 monstres. Le cerveau force le niveau 1.
### À faire (précis, prêt à coder)
1. [IMPORTANT] Chause 4968, Ectorche 4969, Esprigné 4970, Feutôme 4971 → `BrainPassif(f, 9078)` ; Crâme 4972 →
   `BrainPassif(f, 9075)` (les monstres arrivés en vague par les runes d'Invasion l'auront aussi grâce à SortPassif).
2. [MINEUR] [DOUTE] `IlyzaelleBrain.Fight_FightStarted` : lancer Possession au niveau = grade du boss
   (`(Fighter as MonsterFighter)?.MonsterGrade?.GradeId ?? 1`, borné 1-5) au lieu de 1, si le grade du boss suit la taille du
   groupe (à vérifier : le groupe de spawn 428 contient les grades 1-5).

## Boyau du Père Ver (boyau-du-pere-ver) — boss Père Ver (id 4726)
### Mécanique officielle (site)
- Passifs : Cycloporth **Lasccar** (Lourd ; au tour 2 puis tous les 4 tours, sur le dos 2 t : **Enraciné**, −100 PM,
  +20 % dégâts subis, glyphe feu, seul sort Aspiration Gourmande) ; Masticroc et Trémorse **Inébranlable + Lourd** ;
  Morsquale **Les Dents de la Mer de Sable** (Inébranlable ; +20 puissance 2 t à chaque fois qu'un ennemi subit des dégâts) + Lourd.
- Monstres : Cycloporth 4731, Masticroc 4729, Morsquale 4730, Pikténia 4727, Trémorse 4728 (sorts listés sur le site).
- Boss : **Digestion Lente** : **invulnérable à distance**, indéplaçable, case fixe ; monstres **Intaclables** ; joueurs 10 %
  érosion infinie, dégâts eau quand déplacés ; début de tour : frappe terre 450 au càc ; attire de 4 en ligne de vue, révèle ;
  glyphe de soin 15 % ; **Décès Traumatisant** (monstre mort → feu 500 en ligne de vue, Père Ver −10 % résistances +20 % PV) ;
  après sa mort, poison eau infini. Sorts : Bien vu l'Aveugle, Coup d'Œil, Paternalisme.
### État sur le serveur
- Sorts actifs conformes. Effet **421** (Pattes Harassent du Morsquale, −rés. critique) **non géré**.
- **Aucun passif lancé**. Passifs 2.51 : **7367 Digestion Lente** (boss : dégâts eau sur déplacement, 7368 en début de tour,
  état 96 aux alliés, Décès Traumatisant 7369 sur la mort d'un allié, Digestion Accélérée 7374 à sa mort, 7375/7376 début/fin
  de tour, téléportation sur la case 246) ; **7851 Lasccar** (Cycloporth) ; **7845 Les Dents de la Mer de Sable** (Morsquale).
- [DOUTE VERSION] Masticroc / Trémorse : aucun passif 2.51 trouvé qui donne Inébranlable + Lourd → ne rien ajouter.
- [DOUTE VERSION] Chiffres 2.51 ≠ site (ex. Digestion Accélérée 2.51 = 20 % érosion + −20 % vitalité en fin de tour, pas un
  poison eau ; 7375 = terre 41-50 croix 1 + dommages subis ×120 %) : suivre les données 2.51.
### À faire (précis, prêt à coder)
1. [CRITIQUE] `[BrainIdentifier(4726)]` PereVerBrain → `BrainPassif(f, 7367)`.
2. [IMPORTANT] Cycloporth 4731 → `BrainPassif(f, 7851)` ; Morsquale 4730 → `BrainPassif(f, 7845)`.
3. [MINEUR] Effet 421 : dans `game/effects/handlers/Spells/Debuffs/StatsDebuff.cs` ajouter
   `[EffectHandler(EffectsEnum.Effect_SubCriticalDamageReduction)]` et dans le `switch` `case
   EffectsEnum.Effect_SubCriticalDamageReduction: return PlayerFields.CriticalDamageReduction;`.

## Camp du Comte Razof (camp-du-comte-razof) — boss Comte Razof (id 4803)
### Mécanique officielle (site)
- État **Piégé** (piège Éblouissant du Nemroz : −6 PA, −9 PO) : bonus de dégâts des sorts sur les cibles piégées.
- Monstres : Brokouillon 4806, Chevrotine 4805 (Chienchien), Crâmbo 4808 (Lampe Bleue), Nemroz 4807, Viandargh 4804.
- Boss : Jeu Dangereux (invoque au début une **Minikrone** dans le camp joueur ; repousse Lampe/Bouteille), Archi-Pelle
  (eau 500 + chaque touché renvoie 100 % en croix), Chasse Gardée, Pelliste, Trophée de Chasse (tue une invocation ennemie).
  Résistances > 100 % : **chaque invocation non statique ennemie vivante à la fin de son tour → −20 % résistances 1 t**.
  Minikrone **Monokeros** : invulnérable ; Impureté, Bénédiction Arc-en-ciel, Captivité (glyphe), Mort Impure (−20 % rés.
  −2 PA à l'équipe).
### État sur le serveur
- Sorts actifs conformes ; résistances en base 120/105/115/135/100 % → **intuable** sans la mécanique.
- **Passifs jamais lancés** : **8512 Jeu Dangereux** (boss : invoque la Minikrone 4812 via un joueur, −20 % rés. par
  invocation ennemie `I` en fin de tour, −20 % rés. 10 t si un joueur meurt, repousse 4810/4811) ; **8530 Monokeros**
  (Minikrone 4812).
- Effet **1127** (Archi-Pelle, sous-sort 8537 : renvoi des dommages en croix) **non géré**.
### À faire (précis, prêt à coder)
1. [CRITIQUE] `[BrainIdentifier(4803)]` ComteRazofBrain → `BrainPassif(f, 8512)`.
2. [IMPORTANT] `[BrainIdentifier(4812)]` MinikroneBrain → `BrainPassif(f, 8530)`.
3. [MINEUR] Effet 1127 : à identifier (« inflige 100 % des dommages subis en croix 1 ») ; sans lui Archi-Pelle frappe normalement.

## Caverne d'El Piko (caverne-d-el-piko) — boss El Piko (id 4609)
### Mécanique officielle (site)
- **Guerillero** (boss, début) : à chaque début de tour d'El Piko, état aléatoire **Tactique A/B/C/D** à tous les monstres
  (A +10 % rés., B +25 tacle/fuite, C +25 esquive PA/PM, D +100 puissance) ; en fin de tour d'un joueur, frappe 110 les
  personnages adjacents (élément selon l'état). Les sorts des monstres changent d'élément/effets selon l'état.
- Monstres : Cactana 4605, Cactiflore 4604, Cactoblongo 4606, Lévito 4608, Pampactus 4607 (Milépines −1000 PV 1 t).
- Boss : Pikak, Piképik, Bamba (selon l'état Tactique).
### État sur le serveur
- Sorts actifs conformes (variantes Tactique = états 441-444 déjà dans les sorts).
- **Passif 7199 Guerillero jamais lancé** (tirage aléatoire par groupes `group/random` géré par `DefaultSpellCastHandler`).
  Sans lui aucun état Tactique : toutes les variantes sont inactives.
- Effet **1047** (Milépines, « −1000 PV » 1 t) **non géré** → le sort ne fait rien. Effet **2024** (Salsa en Tactique D) non géré.
- [DOUTE VERSION] 2.51 : la confusion est sur Salsa (2018/2019/2024) et non sur Maracac comme sur le site.
### À faire (précis, prêt à coder)
1. [CRITIQUE] `[BrainIdentifier(4609)]` ElPikoBrain → `BrainPassif(f, 7199)`.
2. [IMPORTANT] Effet 1047 : dans `StatsDebuff.cs` ajouter `[EffectHandler(EffectsEnum.Effect_1047)]` et
   `case EffectsEnum.Effect_1047: return PlayerFields.Vitality;` (perte de vitalité temporaire 1 t) [DOUTE léger sur le sens
   exact « −#1 PV » : vitalité retenue].
3. [MINEUR] Effet 2024 : probablement visuel (confusion), le déclarer dans `EffetSansAction` (`Confusion.cs`) pour ne plus
   être signalé, ou l'identifier.

## Chapiteau des Magik Riktus (chapiteau-des-magik-riktus) — boss Choudini (id 4860)
### Mécanique officielle (site)
- Monstres : Bozoteur 4861 (Bang, Baudruche, Dans la Boîte), Graboule 4865 (Balle Piégée, Jonglerie, Poirier), Pirolienne 4863
  (Chaudasse, Haleine Ardente, Roulette Infernale), Roukouto 4864 (Chapeau de Roue, Ériktion, Peintures), Tivelo 4862
  (Fusée Explosive, Monocycle, Roule ma Poule). 60 % de résistance dans un élément chacun.
- Boss : **Prends ma place** (passif, début de tour : échange avec un personnage adjacent ; feu 180 aux adjacents en
  Pesanteur/Enraciné, ou à tous s'il l'est lui-même). Reste assis, Vinriktus, Détriktus.
### État sur le serveur
- Sorts actifs conformes. **Passif 8630 Prends ma place jamais lancé** (pas de cerveau).
- [DOUTE] 8630 niv. 2 lance l'échange (2792 → 8631) sur **chaque** ennemi adjacent hors Pesanteur/Enraciné ; le site dit un
  seul par tour. Suivre les données ; à observer en jeu.
### À faire (précis, prêt à coder)
1. [CRITIQUE] `[BrainIdentifier(4860)]` ChoudiniBrain → `BrainPassif(f, 8630)`.

## Cimetière des Mastodontes (cimetiere-des-mastodontes) — boss Mantiscore (id 4621)
### Mécanique officielle (site)
- Monstres : Boulépique 4617, Fennex 4616, Léolhyène 4619 (Mort Sûre, état Sursis), Ouroboulos 4620 (Carapassable),
  Scordion Bleu 4615 (Creuse Sable). (Tournade 4618 hors donjon.)
- Boss : **Garde-Bouclier** (début, infini) : frappé à distance → se téléporte sur une case adjacente ; si aucune case libre →
  −50 % résistances, +200 puissance et **Dérèglement** 2 t (plus de téléportation). Darmoclès (**Sursis**, soin 600), Force
  Poigne, Provocaspion (attire tous de 3, tours 1/6/11…), Tombeau du Désert (**Enraciné**).
### État sur le serveur
- Sorts actifs conformes. **Passif 7223 Garde-bouclier jamais lancé**.
- 7223 niv. 2 = `4 Teleport 1 case (si pas Dérèglement)` puis `2794 → 7223/3` (malus + Dérèglement). Sur le serveur,
  `CastSpellEffect` lance **toujours** le sort d'un effet 2794 (sauf runes 10020/10023/10027/10035) : posé tel quel, le
  Mantiscore risque de prendre −50 % rés. + Dérèglement à chaque coup à distance, même s'il s'est téléporté (voir point 2).
### À faire (précis, prêt à coder)
1. [CRITIQUE] `[BrainIdentifier(4621)]` MantiscoreBrain → `BrainPassif(f, 7223)`.
2. [IMPORTANT] Garde-fou dans `game/effects/handlers/Spells/Others/CastSpell.cs`, branche `else if (Effect.EffectId ==
   EffectsEnum.Effect_2794)` de `InternalApply` : pour `Spell.Id == 7223`, ne lancer le sort que si la téléportation précédente
   a échoué : `if (Spell.Id == 7223 && Caster.Cell.Id != TargetedCell.Id) continue;` (7223/2 est lancé par le Mantiscore sur
   sa propre case — branche `else` de `DefaultBuffTrigger` ; s'il a bougé, la case n'est plus la sienne). [DOUTE] il est
   possible que ce soit déjà le comportement actuel si les cibles (zone P1 sur l'ancienne case) sont calculées après la
   téléportation ; le garde-fou ne casse rien dans les deux cas. À vérifier en jeu avec le point 1.

## Manoir des Katrepat (manoir-des-katrepat) — boss Anerice la Shushess (id 4882)
### Mécanique officielle (site)
- **Marche des goules (tous les monstres)** : une fois par tour et par monstre, quand on le frappe, les autres monstres
  avancent d'une case vers l'attaquant (se réinitialise au tour du monstre frappé).
- Monstres : Goulafre 4877 (**Goulala** : +4 PM en fin de tour, réactivé tous les 2 tours ; Bidonnage), Gouligane 4879,
  Goultime 4880, Kérigoule 4878 (Chauffetage partage), Pipisteuse 4881 (état Pipisteuse).
- Boss : **Résilience vampyrique** (début : dommages subis −90 % infini), Appétit sanguinaire, Goulification (**Goule**),
  Coopération, Vampyrisme.
### État sur le serveur
- Sorts actifs conformes. **Aucun passif lancé** : Anerice prend 100 % des dégâts (pas de −90 %), aucune Marche des goules.
- Passifs 2.51 : **8727 Résilience vampyrique** (boss : ×10 % dégâts subis `D|DTE|DTB` + Marche des goules) ; **8716
  Goulala** (Goulafre, inclut Marche des goules, relance par délai 2) ; **8726 Marche des goules** (autres monstres).
### À faire (précis, prêt à coder)
1. [CRITIQUE] `[BrainIdentifier(4882)]` AnericeBrain → `BrainPassif(f, 8727)`.
2. [IMPORTANT] Goulafre 4877 → `BrainPassif(f, 8716)` ; Kérigoule 4878, Gouligane 4879, Goultime 4880, Pipisteuse 4881 →
   `BrainPassif(f, 8726)`.

---

## Récapitulatif

| Donjon | CRITIQUE | IMPORTANT | MINEUR |
|---|---|---|---|
| Tour de Bethel | 5 | 0 | 2 |
| Brasserie du roi Dazak | 1 | 1 | 0 |
| Chambre de Tal Kasha | 1 | 1 | 0 |
| Fonderie des Waddicts | 0 | 1 | 0 |
| Croquanterie | 2 | 2 | 0 |
| Temple du dieu Kao | 2 | 1 | 0 |
| Belvédère d'Ilyzaelle | 0 | 1 | 1 |
| Boyau du Père Ver | 1 | 1 | 1 |
| Camp du Comte Razof | 1 | 1 | 1 |
| Caverne d'El Piko | 1 | 1 | 1 |
| Chapiteau des Magik Riktus | 1 | 0 | 0 |
| Cimetière des Mastodontes | 1 | 1 | 0 |
| Manoir des Katrepat | 1 | 1 | 0 |
| **Total** | **17** | **12** | **6** |

Ordre conseillé : Dazak, Razof (boss intuables aujourd'hui car résistances > 100 %), puis les cerveaux simples (Tal Kasha, Kao,
Croquanterie, Père Ver, El Piko, Choudini, Anerice, Mantiscore + 2794), puis Bethel en dernier (prérequis Nécronyx + effet 147).
Tout est à tester en jeu après compilation (aucune vérification en combat réel n'a été faite).
