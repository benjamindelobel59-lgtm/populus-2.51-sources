# Lot 2 : audit des mécaniques (dofuspourlesnoobs → Mystic 2.51)

Méthode : page du site lue en entier, puis `analyse_mecaniques.py` (sorts en base + effets client 2.51), comparaison des sorts
client/base par monstre (tous identiques dans ce lot, sauf mention), lecture du code (cerveaux, CastSpell.cs, Summon.cs,
KillAndSummon.cs, SpellSelector.cs, StatsFields.cs). Lecture seule, rien n'a été modifié.

**RÈGLE VERSION appliquée** : tout ce qui vient du site sans exister dans les données 2.51 est classé **[DOUTE VERSION]** et
n'est PAS dans les corrections à coder. Les chiffres du site (« frappe du 150 », etc.) sont ceux de la version actuelle du jeu ;
ils diffèrent presque partout des jets 2.51 → [DOUTE VERSION] global, je ne les liste pas un par un.

### Constat qui touche plusieurs donjons du lot (moteur)
- **Les monstres n'ont droit qu'à 1 invocation à la fois** : `StatsFields.Initialize(MonsterGrade)` (game/actors/stats/StatsFields.cs
  l.344) met `SummonLimit = 1`, et `Summon.cs` l.80 refuse toute invocation si `UseSummonSlot` et `!Caster.CanSummon()`. L'IA
  (`SpellSelector.AnalysePossibilities`) ne considère pas non plus un sort d'invocation tant que `CanSummon()` est faux.
  Seul le Tengu Givrefoux est relevé (`Stats[PlayerFields.SummonLimit].Base = 2` dans GivrefouxBrains.cs). Conséquences dans ce lot :
  Papa Nowel (ne peut plus se scinder dès qu'un cadeau est en vie), Meulou (Mulou d'Étripage bloqué si un Milimeulou vit),
  Chouque (un seul pirate invoqué par Coup de Sabre Maudit), Koulosse / Wo Wabbit (1 bouftou / 1 Tiwabbit au lieu de 2).
  La donnée 2.51 ne contient aucune limite d'invocation par monstre : le « 2 max » du site est classé [DOUTE VERSION] quand il ne
  bloque pas une mécanique 2.51.
- `KillAndSummon` (effet 405) ne vérifie pas la limite (normal) mais ignore les cibles déjà mortes (`GetAffectedActors` filtre
  `IsDead()`), ce qui casse « Mulanthropie » (voir Meulou).
- Effet 2160 (« lance un sort ») : le serveur le lance sur TOUTES les cibles de la zone ; la valeur `Value` du client (1 pour
  le Chouque) n'est pas lue. Pour le Chouque elle désigne visiblement « la cible la plus proche » (voir Chouque).

---

## Tanière du Meulou (taniere-du-meulou) — boss Meulou (id 232)
### Mécanique officielle (site)
- Cocholou (4867) : Dents Longues (air, PO 9 modifiable, +15 % dommages subis 2 t, 1/cible) ; Grand Méchant (taille +, -15 %
  dommages subis 2 t, **châtiment** +100 puissance par ligne reçue, relance 4) ; Faim de Mulou (eau, cercle 2, -2 PM 1 t, dévoile
  invisibles, PO 8).
- Mergranlou (4869) : Chaperon (soin 150 + 200 puissance + 4 PA à un allié 2 t, mais -20 fuite / -10 % rés.) ; Mergran (feu, -20
  retrait PA/PM 1 t, PO 10) ; Peur du Mulou (-30 tacle 1 t et repousse 1 les ennemis au contact).
- Mulou (102) : Césarienne (terre, mêlée, **Pesanteur** 1 t) ; Cri du Mulou (neutre sur invocations cercle 5, dévoile, relance 3) ;
  Soin Accéléré (soin de fin de tour 4 t, relance 6).
- Muloubard (4866) : Baïkeur (**Inébranlable**, +2 PM, réduction dommages à distance, -50 % soins reçus 2 t, relance 4) ;
  Métalour (terre mêlée + poison par PA utilisé 2 t, donne 2 PA) ; Riffes (eau vol de vie mêlée, -20 % vitalité 2 t, l'effet se
  propage aux voisins en fin de tour).
- Mulounoké (4868) : Esprits Vengeurs (feu bâton, -30 % soins reçus 1 t, dévoile) ; Froid de Mulou (20 % érosion, -20 tacle/fuite,
  -2 PO 2 t au contact, relance 2) ; Ruée Bestiale (s'attire de 5 et frappe air, en ligne 2-6).
- Boss : **Fureur (passif)** : la 1re attaque reçue au début du tour de chaque personnage est réduite de 50 % (« état invulnérable »
  qui absorbe la 1re ligne) ; chaque ligne de dégâts reçue par un monstre allié lui donne +30 puissance (3 ou 4 t selon le
  paragraphe). Étripage (neutre vol de vie mêlée ; si la cible meurt → remplacée par un Mulou). Invocation de Milimeulou (tour 1
  puis tous les 8 t ; Milimeulou 890 PV, terre mêlée, retire fuite, **Mulanthropie** : cible tuée → Mulou ; **à sa mort donne
  2 PM + 200 puissance à ses alliés 2 t**). Rage Reconstituante (soin 2000, +20 tacle +2 PM 2 t, seulement si ≤ 80 % PV, dès le
  tour 2 puis tous les 6 t). **Max 2 invocations**. Faiblesse feu (-5 %), 7 PM, 10 PA.
### État sur le serveur
- Cerveau `MeulouBrain` (PassifsDonjonsBrains.cs) : pose le passif 2.51 **Fureur 305** au début du combat. Conforme 2.51 :
  8685 (+30 puissance 4 t sur tout allié frappé, déclencheur D), 2792→8686 (au début du tour de chaque ennemi, le Meulou
  F232 prend l'état Invulnérable 56), 951 (l'état saute au 1er coup reçu). Dans le moteur, l'invulnérabilité absorbe 100 % du 1er
  coup puis les buffs D se déclenchent (FightActor.InflictDamage) → [DOUTE VERSION] « 50 % » du site ≠ « 100 % » en 2.51.
- Sorts en base = sorts client pour les 7 monstres, grades et résistances identiques (-5 % feu, 20/30/40/40).
- Rage Reconstituante 2.51 : soin fixe 400 (site 2000), condition V80 gérée → [DOUTE VERSION] chiffres.
- Étripage 8653 niv. 1 : état 846 → buff 1160 « à la mort » sur la cible → le Meulou lance 8653 niv. 2 (Invoque Mulou 102 g5,
  critère lanceur *E846). Chaîne gérée, MAIS l'invocation est refusée si le Meulou a déjà une invocation (limite 1) : dès qu'un
  Milimeulou est en vie, Étripage ne crée jamais de Mulou, et la 2e invocation de Milimeulou (tour 9) est impossible.
- **Mulanthropie (8657 → 8658) ne marche pas** : à la mort, c'est la cible morte qui lance 8658 niv. 1 (Force) ; son état 847 vise
  « C » mais `GetAffectedActors` exclut les morts → état jamais posé ; puis le Meulou lance 8658 niv. 2 (405 KillAndSummon sur
  « A,E847 ») qui, lui aussi, exclut les morts → aucun Mulou.
- Absent de 2.51 : effet de mort du Milimeulou (+2 PM +200 puissance), Milimeulou n'a que 8656/8657 dans le client
  → [DOUTE VERSION].
### À faire (précis, prêt à coder)
1. [IMPORTANT] Limite d'invocations du Meulou à 2 (sinon Étripage→Mulou est bloqué par le Milimeulou, mécanique 2.51).
   `AI/Fights/Brain/Custom/Boss/PassifsDonjonsBrains.cs`, remplacer la ligne MeulouBrain par :
   ```csharp
   [BrainIdentifier(232)]
   public class MeulouBrain : BrainPassif
   {
       public MeulouBrain(AIFighter f) : base(f, 305) { f.Fight.FightStarted += Debut; }
       void Debut(IFight fight) { fight.FightStarted -= Debut; Fighter.Stats[PlayerFields.SummonLimit].Base = 2; }
   }
   ```
   (usings déjà présents : Stump.DofusProtocol.Enums, Game.Fights). Le « 2 » vient du site ([DOUTE VERSION] sur la valeur exacte).
2. [IMPORTANT] Réparer Mulanthropie : dans `game/effects/handlers/Spells/Others/CastSpell.cs`, au début de
   `DefaultBuffTrigger(TriggerBuff buff, FightActor triggerrer, BuffTriggerType trigger, object token)` :
   ```csharp
   // MYSTIC : Mulanthropie (Milimeulou 8657 -> 8658) : la cible morte est remplacée par un Mulou (102 g5) invoqué par le Meulou
   if (buff.Spell.Id == 8658 && trigger == BuffTriggerType.OnDeath)
   {
       var fight = buff.Target.Fight;
       var meulou = fight.GetAllFighters<MonsterFighter>(x => x.IsAlive() && x.Monster.Template.Id == 232).FirstOrDefault();
       var grade = Actors.RolePlay.Monsters.MonsterManager.Instance.GetMonsterGrade(102, 5);
       if (meulou != null && grade != null && meulou.CanSummon() && fight.GetOneFighter(buff.Target.Cell) == null)
       {
           var mulou = new SummonedMonster(fight.GetNextContextualId(), meulou.Team, meulou, grade, buff.Target.Cell);
           Handlers.Actions.ActionsHandler.SendGameActionFightSummonMessage(fight.Clients, mulou);
           meulou.AddSummon(mulou);
           meulou.Team.AddFighter(mulou);
       }
       return;
   }
   ```
   (même schéma que KillAndSummon.cs ; noms complets à ajuster aux usings du fichier : `Stump.Server.WorldServer.Game.Actors.RolePlay.Monsters`,
   `Stump.Server.WorldServer.Handlers.Actions`). À tester : Milimeulou en mêlée d'une invocation de joueur, la tuer.
3. Rien d'autre (Fureur, Rage, Étripage conformes 2.51).

## Canopée du Kimbo (canopeacutee-du-kimbo) — boss Kimbo (id 1045)
### Mécanique officielle (site)
- Abrakleur clair (1047) : Pédoncule perfide (-3 PM, 70 % soin / sinon terre vol de vie, PO 3 stricte) ; Radicelle (+200 puissance
  +10 dommages à un allié 2 t, se répète quand on le frappe) ; Radicule (désenvoûte + terre mêlée).
- Bitouf aérien (1020) : Aéroportage (échange + eau, 2-10 PO) ; Tornade pernicieuse (air mêlée, repousse 3).
- Kaskargo (1044) : Bave (téléport + **glyphe qui OS** en début de tour, 2/tour) ; Coopération Baveuse (échange, état Déplacé,
  relance 3, pas au tour 1) ; Mucus (dégâts élément aléatoire croix 1, -2 PM -20 fuite, seulement en Déplacé / Pesanteur /
  Indéplaçable / Enraciné).
- Meupette (1046) : Meupette Choh (-50 esquive PA/PM 2 t, se répète quand frappés) ; Meurtrissure (eau, grande ligne).
- Poolay (1043) : Haleine de vers (feu/air/eau, -1 PO, -1 PA/PM par tour 5 t) ; Fureur de vivre (+20 dommages à l'allié frappé
  4 t) ; Poolay frit (air, attire 2).
- Boss : 400 % résistances partout. Frappé terre/eau → état **Glyphe impaire** ; air/feu → **Glyphe paire**. Il lance alors
  État pair/impair sur le Disciple, qui pose des glyphes sur toutes les cases à distance paire/impaire (OS les joueurs qui y
  commencent leur tour ; le Kimbo qui y commence perd 400 % dans le couple d'éléments choisi et gagne 800 % dans l'autre),
  gagne 9000 agilité / +1 PM, OS au contact. Disciple : 0 PM, intuable. Furie du Kimbo (air + vol de vie, repousse 2),
  Téléportation 8 cases, Boum Boh (+50 agilité).
### État sur le serveur
- Tout est dans les données 2.51 et en base : Boum Boh 1071 porte les déclencheurs DA/DF/DE/DW qui posent les états 29/30 ;
  Etat pair/impair 1075/1076 (états requis), Glyphe Pair/Impair 1072/1073 (zone D = damier, gérée : Damier.cs), sorts de glyphe
  1077/1078 (Kill des joueurs, -400 % / +800 % au Kimbo « g »). Résistances 400 % (Kimbo) / 800 % (Disciple) en base.
- Cerveau `DiscipleDuKimboBrain` : lance la glyphe au début de son tour selon l'état, puis se décale d'une case. Conforme.
- Différences : Mucus 2.51 = seulement si Pesanteur ; Bitouf a en plus « Envolupté » ; Kaskargo/Poolay sans Poolay frit
  → [DOUTE VERSION]. Le Kimbo doit relancer Boum Boh chaque tour (durée 1) : c'est l'IA normale (1 PA, sans relance).
### À faire (précis, prêt à coder)
Rien à faire (à valider en jeu : le Kimbo lance bien Boum Boh à chaque tour ; sinon le poser via SortPassif en début de tour).

## Cave du Toxoliath (cave-du-toxoliath) — boss Toxoliath (id 3752)
### Mécanique officielle (site)
- Éperfide (3738) : Brûlurgence (feu cercle 2, -5 PA, 4-10 PO) ; Drain (air vol de vie 2-5 PO) ; Étoile d'Arakne (terre + glyphe
  étoile 2 t : Pesanteur, -4 PM, +100 terre vol de vie à chaque dégât reçu, relance 3).
- Lucrane (3737) : **Carapace Gardée** (renvoie 50 %, poison neutre au début du tour de l'attaquant par ligne ; levée 1 t par
  dommages de poussée, mais Indéplaçable pendant ce temps) ; Offensivière (rush + air) ; Pinsecte (-50 % PV actuels 1 t).
- Morfrelon (3739) : Dardagnan (poison eau 3 t) ; Plongeon Quille (neutre, vole 3 PO 2 t) ; Puissance Cible (+100 puissance par
  ligne reçue 2 t, relance 3).
- Puceronde (3736) : Brazéro (feu = 30 % des PV manquants) ; Langagement (téléport, attire 2 en cercle 3, Pesanteur, poison air
  par PA quand elle est poussée, dès tour 3, relance 3) ; Largage Personnel (terre + 50 % PV érodés, cercle 2).
- Scoliopode (3735) : Coup Percutant (eau, poison par PM, tue les invocations) ; Poison Incurable (2 t : chaque ligne reçue →
  Insoignable + vol 2 PM à l'attaquant) ; Salivraison (neutre, attire 1, 15 % érosion).
- Boss : **Toxmose (passif)** : chaque ligne de dégâts directs reçue → poison eau 700 par PM utilisé 2 t sur l'attaquant ;
  Flacune (neutre, érosion, poison par PA) ; Poison Volatile (dès tour 6, relance 4 : +2 PM -2 PA, poison feu, glyphe cercle
  inversé 8 : +50 % dommages subis) ; Venin Salvateur (état, +50 % dommages subis, perd 1 t d'envoûtement par coup, soigne
  l'attaquant de 150 %, Jouvence 2 t) ; Vile Ruse (air + poison 5 t). 5 vagues (tours 1/6/11/16/21 ou dès que tout est mort),
  vague invulnérable 1 t.
### État sur le serveur
- Cerveaux `ToxoliathBrain` (Toxmose 4796 → 4795 : poison eau par PM 2 t sur l'attaquant) et `LucraneBrain` (Carapace gardée
  4746 : Indéplaçable sur poussée, 4745→4744 renvoi 1123 50 % + poison neutre, bloqué si l'état 97 est présent). Conformes 2.51.
- Poison Volatile : `initialCooldown` 5 en base → lançable à partir du tour 6, conforme. Effet 333 de Venin Salvateur = visuel
  (EffetSansAction). Vagues : `ajouter_vagues_dimensions.sql` (4 vagues, tours 6/11/16/21 dans Fight.WaveTurns, vague
  invulnérable : AddWaveInvulnerability). Conforme.
- Différences 2.51 : Poison Incurable ne se déclenche qu'à distance (déclencheur DR) ; Toxmose aussi sur dégâts indirects
  (déclencheur D) → [DOUTE VERSION].
### À faire (précis, prêt à coder)
Rien à faire.

## Caverne de Nowel (caverne-de-nowel) — boss Papa Nowel (id 872)
### Mécanique officielle (site)
- Monstres du site : Bonhomme de Neige (845 : Poussée d'Egul, Esprit de Nowel, Tire-Fesse), un monstre « Roulage / Boule /
  Neige Soignante », un monstre « Déchirement Intestinal / Ablation / Aboiement », un « Griffe du Kanigrou Hivernal / Chute
  Majestueuse », Minimini Nuit' (907 : Patinage, Marteaux d'Okim / d'Orelos).
- Boss phase 1 : **Hotte d'or** (lancé au début : +5 PO 1 t à chaque tentative de retrait PM) ; Aspir'nenfan (attire 3, ligne,
  PO infinie, 2/t) ; Infantophagie (OS en mêlée + soin 500) ; Engluement (neutre, -1 à 2 PM, PO 8, 3/t) ; Trak'nenfan (1 cadeau
  aléatoire sur 4, relance 4 : Kol'nenfan poison/attire, Koup'nenfan feu -2 PM, Pet'nenfan piège, Cadob'Onux +150 puissance).
- Phase 2 : à peu de PV (≈ 20-40 %), **Embûche de Nowel** (se soigne de 100 %, état **Fragilisé**) puis **Dichotomie**
  (sacrifie 50 % PV, invoque le Demi Papa Nowel, état **Séparé**, transfère Engluement et Trak'nenfan au Demi). Demi : Empaquetage
  (cadeau 2 t : -1 PM -4 PA, Affaibli), Attrap'nenfan (repousse 6). Tuer le Papa Nowel (jambes) termine le combat.
### État sur le serveur
- Pas de cerveau. Sorts en base = client pour Papa Nowel (883, 882, 287, 885, 881, 2792), Demi (869, 885, 884, 2790), cadeaux
  (880, 881, 882, 887). Trak'nenfan : 4 invocations « random 25 » → tirage d'une seule, géré (DefaultSpellCastHandler).
  Engluement niv. 5 interdit en Séparé (état 17) → transfert conforme ; Embûche niv. 5 interdite en Fragilisé, relance 63 ;
  Dichotomie exige Fragilisé.
- **Bug bloquant** : Dichotomie invoque le Demi (UseSummonSlot = 1) ; avec la limite d'1 invocation, dès qu'un cadeau est en vie
  (Trak'nenfan au tour 1), le Papa Nowel ne peut plus se scinder (Summon.cs refuse ; l'IA écarte le sort ou le lance sans
  invocation).
- **Embûche de Nowel n'a aucune condition de PV** dans les données (le seuil est géré par l'IA officielle) : sur Mystic, c'est un
  soin (catégorie Healing) que l'IA peut lancer dès qu'il lui manque des PV et qu'il lui reste 4 PA (pas seulement sous le seuil
  d'urgence de 50 %) → phase 2 déclenchée trop tôt, soin gâché.
- Monstres de salle sur le serveur (2.51) : Abominable Yiti 839, Bonhomme de Neige 845, Tofu Hiberné 847, Bouboule de Neige 867,
  Craqueleur des Glaces 869, Ver Getur 871, Minimini Nuit' 907. Les monstres « Roulage de Neige », « Aboiement » (le sort
  Déchirement Intestinal 638 appartient aux Croc Gland en 2.51) et « Griffe du Kanigrou Hivernal » n'existent pas en 2.51
  → [DOUTE VERSION].
- Hotte d'or : le sort 2556 existe dans le client 2.51 (+5 PO 1 t sur MPA) mais n'est attribué à aucun monstre en 2.51
  → [DOUTE VERSION] (ajout possible si l'utilisateur le veut : `SortPassif.Poser(f, 2556)` ne suffirait pas, la durée est 1 tour).
### À faire (précis, prêt à coder)
1. [CRITIQUE] Cerveau Papa Nowel : limite d'invocations relevée (sinon jamais de phase 2) + Embûche seulement sous 30 % PV.
   Nouveau fichier `AI/Fights/Brain/Custom/Boss/PapaNowelBrain.cs` :
   ```csharp
   using Stump.DofusProtocol.Enums;
   using Stump.Server.WorldServer.Game.Actors.Fight;
   using Stump.Server.WorldServer.Game.Fights;
   using Stump.Server.WorldServer.Game.Spells;

   namespace Stump.Server.WorldServer.AI.Fights.Brain.Custom.Boss
   {
       /// <summary>MYSTIC : Papa Nowel : 4 invocations (cadeaux + Demi Papa Nowel de Dichotomie) ; Embûche de Nowel (883)
       /// seulement sous 30 % PV (avant : soin lancé dès les premiers dégâts, phase 2 trop tôt).</summary>
       [BrainIdentifier(872)]
       public class PapaNowelBrain : Brain
       {
           const int Embuche = 883, Seuil = 30;
           public PapaNowelBrain(AIFighter f) : base(f) { f.Fight.FightStarted += Debut; }
           void Debut(IFight fight) { fight.FightStarted -= Debut; Fighter.Stats[PlayerFields.SummonLimit].Base = 4; }

           public override void Play()
           {
               Spell retire = null;
               if (Fighter.Spells.TryGetValue(Embuche, out var s) && Fighter.LifePoints * 100 > Fighter.MaxLifePoints * Seuil)
               { retire = s; Fighter.Spells.Remove(Embuche); }
               try { base.Play(); }
               finally { if (retire != null) Fighter.Spells[Embuche] = retire; }
           }
       }
   }
   ```
   (même technique de retrait temporaire que GrolloumBrain / VortexBrain). Le seuil 30 % vient du site (« 20-25 % », « < 40 % »)
   → valeur réglable. À tester : le Papa Nowel doit se soigner une seule fois vers 30 %, puis au tour suivant lancer Dichotomie
   (Demi Papa Nowel invoqué, -50 % PV, état Séparé) même avec un cadeau en vie.

## Caverne du Koulosse (caverne-du-koulosse) — boss Koulosse (id 670)
### Mécanique officielle (site)
- Accompagné uniquement de Bouftous des Cavernes (671 : Bavouille -2 PA, Crachouille -2 PM à 6 PO, Morsure neutre mêlée ;
  IA fuyarde). Boufcoul (672, invoqué en critique) : Bise (invisible 1 t + 5 PM + 10 % crit + 200 agi à un allié), Morsure (eau,
  -10 fuite).
- Boss (ne frappe pas) : Appel du Koulosse (attire 10, ligne 2-8, seul sort du tour 1) ; Souffle du Koulosse (dès tour 2,
  ligne 5 PO, 3/t, 1/cible : transforme en Boufcoul, -100 PA 1 t, OS les invocations, +10 dommages par coup reçu 2 t) ;
  Invocation de Bouftou des Cavernes (dès tour 2, relance 3, Boufcoul en critique 10 %) ; Calumet de la Paix (dès tour 2,
  relance 2 : sur ses invocations, apparence Boufcoul, +200 stats +5 PM 2 t). **Max 2 invocations**.
### État sur le serveur
- Sorts en base = client. `initialCooldown` 1 sur 604/605/607 → pas au tour 1 : conforme. 604 critique : invoque 672 : conforme.
  Souffle 607 : -100 PA, apparence, Kill sur « J » (invocations), +10 dommages sur D : conforme. Calumet sur « j » : conforme.
- Différence : le Koulosse n'a qu'1 invocation à la fois (limite moteur) au lieu de 2 → [DOUTE VERSION] (le « 2 max » n'est
  pas dans les données 2.51). Pas de cerveau (inutile).
### À faire (précis, prêt à coder)
Rien à faire. (Optionnel si l'utilisateur valide le « 2 max » : cerveau `[BrainIdentifier(670)]` qui met
`Fighter.Stats[PlayerFields.SummonLimit].Base = 2` sur FightStarted, comme le point 1 du Meulou.)

## Cavernes du Kolosso (cavernes-du-kolosso) — boss Kolosso (id 2986)
### Mécanique officielle (site)
- Page « à réviser » : 4 salles dans l'ordre libre (Blérices, Bléroms, Blérauves, Fleuros) pour 4 reliques ; boss avec Professeur
  Xa. Alliés à -10 000 soins ; Kolosso Lourd, frappe seulement avec Razepoutine (triangle terre, plus fort à bas PV) s'il a
  quelqu'un au contact, attire, soigne ; Xa : feu en ligne, retire PA, ressuscite, Kirbili (1 t / 2 : +100 dommages par coup reçu
  par un monstre mais soigne les joueurs à 4-5 PO) ; Baïkal (+10 % dommages finaux par coup en mêlée) ; Xa mort → Kolosso +1 PM,
  +200 puissance, sort Xalactite ; seul → Insoignable en ligne 3 PO ; vulnérable 1 t s'il est frappé par une invocation.
### État sur le serveur
- Jugé PARFAIT par l'utilisateur (cerveaux KolossoBrain / ProfesseurXabrain, kolosso.sql). Aucune modification proposée.
- [DOUTE VERSION] : Xalactite n'existe pas dans le client 2.51 (mot-clé introuvable) ; l'effet « Xa mort → +1 PM +200
  puissance » et l'Insoignable en solo ne sont pas vérifiables dans les données 2.51 du Kolosso (2470 Razepoutine, 2479 Baïkal,
  2480 Illyana ; Xa : 2481 Kirbili, 2482 Acolyte, 2483 Cerebro).
### À faire (précis, prêt à coder)
Rien à faire.

## Grange du Tournesol Affamé (grange-du-tournesol-affame) — boss Tournesol Affamé (id 799)
### Mécanique officielle (site)
- Épouvanteur (4820) : Fuyez ! (eau au contact, -40 puissance 1 t) ; Protection des Champs (air ligne 4, attire 3).
- Gardienne Champêtre (4821) : Désherbant (feu croix 1, ligne 5) ; Engrais (+50 puissance + soin 20 cercle 3, 1 t).
- Pissenlit Diabolique (79) : Herbe Sauvage (feu, -2 PM, ligne 5) ; Zizou (terre mêlée).
- Rose Démoniaque (78) : Pétale Empoisonné (terre + poison 3 t, 3 PO) ; Rose Épineuse (eau, -1 PA/PM mêlée).
- Tournesol Sauvage (48) : Poison Sauvage (feu par PA 3 t, 6 PO) ; Racine Pivotante (terre 5 PO).
- Boss : Appel des Champs (invoque un monstre du donjon tous les 6 t) ; Goinfrage (eau vol de vie mêlée, +10 dommages 2 t) ;
  Soin Feuillu (soin 50 cercle 2). Aucune règle spéciale.
### État sur le serveur
- Sorts en base = client pour tous. Appel des Champs (791) : 4 invocations « random » (Gardienne 4821, Pissenlit 79, Rose 78,
  Épouvanteur 4820), relance 3 (site : 6) → [DOUTE VERSION].
- Les salles du serveur contiennent aussi Champ Champ (59) et Moskito (61) et pas d'Épouvanteur / Gardienne (le site ne donne la
  composition qu'en images) → [DOUTE VERSION], sans effet sur la mécanique.
### À faire (précis, prêt à coder)
Rien à faire.

## Château du Wa Wabbit (chateau-du-wa-wabbit) — boss Wa Wabbit (id 180)
### Mécanique officielle (site)
- Black Tiwabbit (68) : Frappe des Wabbits, Petit Wabehameha (feu, repousse 1, ligne 8). Black Wabbit (65) : Frappe, Wabeha (air
  ligne 3, repousse 1, PO 11). Grand Pa Wabbit (99) : Cawotte Explosive (feu cercle 2), Paralysante (-1/2 PA, -2/3 PM cercle 2),
  Vitaminée (soin 80 cercle 2), Frappe. Tiwabbit (96) : Envoie la patate (+100 puissance +10 dommages croix 1), Frappe. Tiwabbit
  Kiafin (72) : Panique (repousse 1, +20 fuite), Vent de panique (poison neutre 4 t à tous, relance 5). Wabbit (64) : Farandole
  de cawottes (+1 PA/PM/PO puis soin 20 %), Frappe. Wo Wabbit (97) : invoque Tiwabbit Kiafin → Tiwabbit → Black Tiwabbit
  (relance 4, **max 2**), Ventroboom (neutre, repousse 6).
- Boss : Abrutissement (-150 puissance 1 t à tous, renforcé s'il est seul, relance 3) ; Awmuwe Woyale (lui + l'allié le plus proche
  -30 % dommages subis 2 t, bouclier s'il est seul, relance 5) ; Cawotte Woyale (invocation + glyphe carré 1 : soigne les monstres
  80, **Insoignable** 2 t aux joueurs, +10 % critique au Wa ; relance 2) ; WaWabehameha (neutre 11 PO, gros critique). IA fuyarde.
### État sur le serveur
- Sorts en base = client pour les 8 monstres. Version 2.51 : Abrutissement = -101 à 400 en Force/Intel/Agi/Chance 3 t, relance 6
  (le site le confirme dans le paragraphe succès) ; Awmuwe Woyale = 50 de réduction fixe en mêlée (DCAC) 4 t ; Cawotte Woyale =
  invocation 3458 (n'occupe pas de place d'invocation, ne joue pas) + glyphe 3782 « Soin Woyale » qui soigne les alliés
  seulement. Pas d'Insoignable, pas de bonus critique, pas de version « seul » → [DOUTE VERSION].
- Wo Wabbit : 1 Tiwabbit à la fois (limite moteur) au lieu de 2 → [DOUTE VERSION] (même remarque que le Koulosse).
### À faire (précis, prêt à coder)
Rien à faire. (Optionnel : limite 2 pour le Wo Wabbit 97, même code que le Meulou, si l'utilisateur valide.)

## Bateau du Chouque (bateau-du-chouque) — boss Le Chouque (id 230)
### Mécanique officielle (site)
- Boomba (228) : Bombe Pirate (feu croix 1, 15 % érosion, PO 8) ; Feu d'Artifesse (invisibilité + 30 % critique cercle 2, relance 3).
- Canondorf (231) : Canon Tournoyant (repousse 2, -25 % résistances, croix 1) ; Gros Boulet Pirate (feu, repousse 2, ligne PO infinie).
- Ivremor (4161) : Cocktail Rhumotov (feu croix 1 + **glyphe** feu 2 t) ; Crachat de Rhum (feu cône 2, -1 t d'envoûtement, retire
  ses bouteilles) ; Et une bouteille de Rhum ! (+100 intelligence cumulable).
- Nakunbra (229) : **À l'abordage ! (passif)** : +1 PM 1 t par dégât reçu (cumulable) ; Sus à l'ennemi ! (attire toute la ligne,
  +20 tacle par entité attirée aux monstres cercle 3) ; Tranchage Mortel (neutre ; OS si cible ≤ 50 % PV).
- Ricanif (4160) : Coupe-genoux (neutre, -100 fuite) ; Tornade de Lames (neutre cercle 2, vole 1 PM).
- Boss : **Malédiction Pirate (passif)** : au début de chaque tour du Chouque, sur le personnage le plus proche qui n'est pas
  **Maudit** et sur tous les Maudits : minimise leurs jets 1 tour, puis 110 eau en vol de vie s'ils ont > 30 % PV, sinon OS et un
  pirate aléatoire apparaît à leur place. Coup de Sabre Maudit (neutre mêlée, -150 puissance, état **Maudit** 2 t ; si cible
  ≤ 50 % PV, invoque un pirate au contact). Téléportation Spectrale (échange de place avec un allié, +200 puissance 2 t à
  l'allié, PO infinie, 1/t, impossible si Pesanteur).
### État sur le serveur
- Pas de cerveau pour le Chouque ni le Nakunbra : **Malédiction Pirate 6103 et À l'abordage 6115 existent dans le client 2.51
  mais ne sont lancés par rien** (attribués à aucun monstre, comme les autres passifs de l'audit).
  - 6103 : buff « début de tour » (TB) → 6145 niv. 1 : 2160 → 6145 niv. 2 sur « H,e307 » (joueurs non Maudits, `Value` = 1) et
    1160 → 6145 niv. 2 sur « H,E307 » (tous les Maudits). 6145 niv. 2 : 781 Minimise les jets 1 t ; 91 eau vol de vie 26-30 si
    > 30 % PV (v30) ; 405 KillAndSummon d'un pirate au hasard (228/231/229/4160/4161 g5, random 20) si ≤ 30 % (V30). Tous les
    effets ont un gestionnaire.
  - 6115 niv. 1 : buff D permanent → 6115 niv. 2 : +1 PM 1 t.
- **Effet 2160 mal appliqué** : `CastSpellEffect.InternalApply` lance le sous-sort sur toutes les cibles de la zone ; la valeur
  `Value` (= 1 ici : « la cible la plus proche ») est ignorée → posé tel quel, Malédiction Pirate frapperait TOUS les joueurs non
  Maudits au lieu du plus proche.
- **Téléportation Spectrale cassée** : 6119 (cible un ennemi) → 6144 niv. 1 : 2160 (Value 1) → 6144 niv. 2 sur « g » (alliés) :
  1101 + 138 (+200 puissance 2 t). Sur le serveur, 2160 vise tous les alliés (tous reçoivent +200 puissance) et l'effet 1101 est
  géré par `TeleportOnPremonition` qui ne sert qu'au sort Steamer 9900 → **le Chouque n'échange jamais de place**.
  Lecture 2.51 : le Chouque vise un ennemi, puis échange sa place avec l'allié le plus proche de cet ennemi (cohérent avec le site :
  « échange de place pour se rapprocher »).
- Coup de Sabre Maudit 6104 → 6142 (Invoque un pirate au hasard, cible L,V50) : conforme 2.51, mais un seul pirate à la fois
  (limite moteur) ; les pirates de la Malédiction (405) ne sont pas limités.
- [DOUTE VERSION] : glyphe de Cocktail Rhumotov et « Pesanteur interdit » de la Téléportation absents des données 2.51 ;
  -150 « puissance » = 186 SubDamageBonusPercent 150 en 2.51.
### À faire (précis, prêt à coder)
1. [CRITIQUE] Cerveaux qui posent les passifs, dans `PassifsDonjonsBrains.cs` :
   ```csharp
   /// <summary>Le Chouque : Malédiction Pirate (début de tour : joueur non Maudit le plus proche + Maudits).</summary>
   [BrainIdentifier(230)] public class ChouqueBrain : BrainPassif { public ChouqueBrain(AIFighter f) : base(f, 6103) { } }

   /// <summary>Nakunbra : À l'abordage ! (+1 PM 1 tour par dégât reçu).</summary>
   [BrainIdentifier(229)] public class NakunbraBrain : BrainPassif { public NakunbraBrain(AIFighter f) : base(f, 6115) { } }
   ```
   (Nakunbra = [IMPORTANT], compté à part.)
2. [CRITIQUE] Respecter `Value` de l'effet 2160 pour le Chouque (« les N plus proches de la case visée »), dans
   `game/effects/handlers/Spells/Others/CastSpell.cs`, `InternalApply()`, remplacer `foreach (var affectedActor in GetAffectedActors())` par :
   ```csharp
   var cibles = GetAffectedActors();
   // MYSTIC : Chouque (6145 Malédiction Pirate, 6144 Téléportation Spectrale) : 2160 avec Value = N -> les N cibles les plus
   // proches de la case visée (avant : toutes les cibles de la zone)
   if (Effect.EffectId == EffectsEnum.Effect_CastSpell_2160 && Dice.Value > 0 && (Spell.Id == 6145 || Spell.Id == 6144))
       cibles = cibles.OrderBy(x => x.Position.Point.ManhattanDistanceTo(TargetedPoint)).Take(Dice.Value).ToArray();
   foreach (var affectedActor in cibles)
   ```
   Limité à ces 2 sorts par prudence (le sens de `Value` pour 2160 n'est pas vérifié ailleurs). Vérifier au premier test que
   `Dice.Value` vaut bien 1 (la valeur vient d'EffectsBin en base ; sinon utiliser 1 en dur pour ces 2 sorts).
3. [CRITIQUE] Échange de place de la Téléportation Spectrale : `game/effects/handlers/Spells/Move/TeleportOnPremonition.cs`,
   au début de `InternalApply()` :
   ```csharp
   // MYSTIC : Téléportation Spectrale du Chouque (6144 niv. 2, effet 1101) : échange de place avec l'allié visé
   if (Spell.Id == 6144)
   {
       var allie = GetAffectedActors().FirstOrDefault(x => x != Caster);
       if (allie != null && allie.CanSwitchPos() && Caster.CanSwitchPos() && !allie.IsCarrying() && !Caster.IsCarrying())
       {
           Caster.ExchangePositions(allie);
           allie.TriggerBuffs(Caster, BuffTriggerType.OnMoved);
       }
       return true;
   }
   ```
   (ajouter `using Stump.Server.WorldServer.Game.Fights.Buffs;` si absent). À tester : le Chouque vise un joueur éloigné, échange
   avec le pirate le plus proche de ce joueur, et seul ce pirate prend +200 puissance.
4. [MINEUR] Pirates de Coup de Sabre Maudit limités à 1 en vie (limite moteur) : si l'utilisateur veut plusieurs pirates, ajouter
   dans ChouqueBrain `Fighter.Stats[PlayerFields.SummonLimit].Base = 5` sur FightStarted (pas de limite chiffrée en 2.51 → doute).

## Clairière du Chêne Mou (clairiegravere-du-checircne-mou) — boss Chêne Mou (id 257)
### Mécanique officielle (site)
- Page ancienne (texte sans noms de sorts) : Araknotron (boost, téléport, terre -PA, désenvoûte), Abraknyde Sombre (invoque
  Tronknydes / arbres, -PA), Abrakne Sombre (-PM à distance), Branche Invocatrice (invoque des monstres du donjon, même des
  branches), Branche Soignante (0 PM, gros soins).
- Boss : attire en ligne puis eau + repousse, frappe air en mêlée, terre ~500 à distance -PA 3/t, gros boost PO / critique /
  dommages. Pas de règle spéciale (l'invulnérabilité des branches n'existe qu'en expédition).
### État sur le serveur
- Sorts en base = client : Chêne Mou 976 Complicité (+200 puissance, +63 PO, +50 % critique aux alliés 3 t), 483 Embranchement,
  484 Tornade de Branches, 1021 Enracinement (si Porté), 977 Attirance Sylvestre (attire 12 + eau). Branches 258 / 260, Araknotron
  990, Abraknyde 991, Abrakne 992 conformes. Aucun effet non géré.
### À faire (précis, prêt à coder)
Rien à faire.

## Clos des Blops (clos-des-blops) — boss Blops Royaux (ids 1184 Coco, 1185 Griotte, 1186 Indigo, 1187 Reinette ; Multicolore 1188)
### Mécanique officielle (site)
- Blops (273/274/275/276) : Bibloperie (élément du blop, ligne 2), Biblopition (Biblop, relance 5), Blopition (7 PA), Blyphe
  (-1 PM croix 1 + glyphe), Cache Biblop (invisibilité des invocations). Biblops : Bibloperie, Biblopiment. Blopignon (1182) :
  Bloblo, Blopiction, Bloprojection. Gloutoblop (1181) : Gloutage (vole vitalité/PM puis OS, alliés compris sauf boss).
  Tronkoblop (1183) : Blopium, Blopsoin, Blopzone.
- Boss : Blopunition Royale, Blotection (+100 % dans son élément à un allié), Blovocation (blop aléatoire). 94 % dans son élément.
### État sur le serveur
- Sorts en base = client pour les 12 monstres. Rien de spécial à gérer.
### À faire (précis, prêt à coder)
Rien à faire.

## Donjon du Comte Harebourg (donjon-du-comte-harebourg) — boss Comte Harebourg (id 3416)
### Mécanique officielle (site)
- Cycloïde (3412) : Cyclore (neutre, -4 PM, attire 6, ligne 7) ; Cyclochette (air mêlée, échange, Indéplaçable) ; Cyclaustrophobe
  (tour 1 puis tous les 3 t, cercle 6 : coup reçu en mêlée → retour à la case de début de tour, +100 dommages cumulables).
- Granduk (3410) : Hibouffonnerie (feu ligne 4, vole 200 intelligence) ; Hibougie (neutre = 30 % PV manquants) ; Hiboumerang
  (**Hiboum** 2 t : coup reçu à distance → échange avec l'attaquant et renvoie les dégâts autour).
- Nocturlabe (3415) : Surchauffense, Tonnerre mécanique ; **Tactique du Tic Tac (passif)** : coup reçu en mêlée → dégâts renvoyés
  cercle 6 (100 % alliés / 50 % ennemis).
- Sinistrofu (3414) : Brumidité (**Brouillardent** : coup reçu → invisibilité et dégâts recopiés cercle 3) ; Diligence ; Sinistérie.
- Strigide (3411) : Stricannement, Strictus (échange), Stridicule (+6 PO ; **Strident** : coup reçu à distance → % PV retirés à
  4 PO et plus).
- Boss : **invulnérable** ; Mi-temps (glyphe croix 3 au début de son tour : OS ; case centrale glyphée depuis la 3.5) ;
  **confusion** de chaque joueur au début de son tour selon ses % PV (90° / 180° / -90°), décalée de -90° par ligne de dégâts
  au corps à corps ; tour impair : l'attaquant est téléporté symétriquement ; tour pair : c'est le Comte, et s'il échange avec une
  entité il devient vulnérable (l'allié échangé gagne +100 dommages ; si c'est une invocation : mort des alliés à 3 cases) ;
  case d'arrivée impossible → tous les joueurs meurent. Contretemps (terre + eau % PV manquants, renvoie à la case de début de
  tour s'il est vulnérable) ; Jaquemart (si invulnérable) ; Multicomte (si vulnérable : feu + illusions).
### État sur le serveur
- Cerveaux existants (FrigostOtomaiBrains.cs) : `ComteHarebourgBrain` pose **Carillon 3642** (Invulnérable 56, Pendultimatum 3643
  confusion au corps à corps, Bon vieux temps 3644 glyphe 3645 « Kill » croix 3 sans le centre, Comtoise 3646 confusion selon les
  PV 770-775, Alternance 3647 / Mi-temps 3648 avec délai 1 = alternance pair/impair, Fuseau 3649 téléport symétrique de
  l'attaquant + Air du temps « W » = mort si case invalide, Gousset 3657 téléport symétrique du Comte, désactivation de
  l'invulnérabilité 1 t, +100 dommages, Heure locale 3651 si invocation). `NocturlabeBrain` pose 3639. Délais d'effets gérés
  (Buff.DecrementDuration). Effets 770-775, 1063, 1097, 1099, 1105, 1018 gérés.
- Hiboumerang, Brumidité, Stridicule, Cyclaustrophobe sont des sorts normaux lancés par l'IA (relances 3/2/3/3) : conformes.
- [DOUTE VERSION] : case centrale du Mi-temps (ajout 3.5 ; en 2.51 la zone X3,1 exclut le centre), état indicateur pair/impair
  (3.6), chiffres.
### À faire (précis, prêt à coder)
Rien à faire de certain (gros chantier déjà codé : à valider en jeu, notamment l'alternance pair/impair et la mort sur case
invalide).

## Crypte de Kardorim (crypte-de-kardorim) — boss Kardorim (id 4051)
### Mécanique officielle (site)
- Chafer Débutant (4046) : Petit coup du Chafer (terre mêlée), Souffle Spectral (neutre ligne 2-3). Chafer Éclaireur (4048) :
  Flèche de Feu (feu + poison 1 t), Flèches Critiques (+40 % crit 2 t, relance 5). Chafer Furtif (4047) : Camouflage
  (invisible 1 t, dès tour 2, relance 3), Petit coup, Souffle. Chafer Piquier (4049) : Empalement (air ligne 3), Euphorie
  Malsaine (+10 dommages aux alliés). Sergent Chafer (4050) : Frénésie (+50 puissance aux alliés 2 t), Petit coup.
- Boss : Appel de Kardorib (Kardorib 4052 : Embrochement neutre repousse 2 ; relance 6), Casse-crâne (neutre ligne 2 mêlée),
  Craquement d'Os (+1 PM cercle 2, dès tour 2, relance 3). 140-200 PV.
### État sur le serveur
- Sorts en base = client pour les 7 monstres, PV identiques. Conforme.
### À faire (précis, prêt à coder)
Rien à faire.

---

## Récapitulatif

| Donjon | CRITIQUE | IMPORTANT | MINEUR |
|---|---|---|---|
| Tanière du Meulou | 0 | 2 | 0 |
| Canopée du Kimbo | 0 | 0 | 0 |
| Cave du Toxoliath | 0 | 0 | 0 |
| Caverne de Nowel (Papa Nowel) | 1 | 0 | 0 |
| Caverne du Koulosse | 0 | 0 | 0 |
| Cavernes du Kolosso (parfait) | 0 | 0 | 0 |
| Grange du Tournesol Affamé | 0 | 0 | 0 |
| Château du Wa Wabbit | 0 | 0 | 0 |
| Bateau du Chouque | 3 | 1 | 1 |
| Clairière du Chêne Mou | 0 | 0 | 0 |
| Clos des Blops | 0 | 0 | 0 |
| Donjon du Comte Harebourg | 0 | 0 | 0 |
| Crypte de Kardorim | 0 | 0 | 0 |

Hors corrections, en [DOUTE VERSION] (à décider par l'utilisateur) : limite « 2 invocations » du Koulosse et du Wo Wabbit,
Hotte d'or du Papa Nowel (sort 2556 présent dans le client mais donné à aucun monstre en 2.51), effet de mort du Milimeulou,
50 % (site) ≠ 100 % (2.51) de l'absorption du Meulou, glyphe de l'Ivremor, Insoignable de la Cawotte Woyale, Xalactite du Kolosso.
