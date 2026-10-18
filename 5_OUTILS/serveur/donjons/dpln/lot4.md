# Lot 4 : audit des mécaniques (dofuspourlesnoobs → Mystic 2.51)

Audit fait le 17-18/10 en lecture seule (aucun fichier serveur, client ni base modifié).
Sources : pages `txt/<slug>.txt`, `analyse_mecaniques.py` (sorts en base + effets du client 2.51), base `world`, code `stump.server.worldserver`.
**RÈGLE VERSION appliquée** : rien n'est proposé à coder qui n'existe pas dans le client 2.51 ou en base. Ce qui vient d'une version
plus récente du jeu ou ne diffère que par les chiffres est marqué **[DOUTE VERSION]** et reste hors corrections.

Constats valables pour tout le lot (vérifiés dans le code, l'audit précédent est en partie dépassé) :
- Déjà gérés aujourd'hui : zone `;NNN` (case fixe, `SpellEffectHandler` → `effect.CaseFixe`), zone `I`, effets 1069/1071, 240-244, 418,
  1123, 1125, 1163, 1171/1172, 1023, déclencheurs `X` (mort), `D`, `DR`, `DM`, `TE`, `TB`, `MPA`, `PMD`, `delay` des effets (Buff.DecrementDuration).
- Seul effet non géré du lot : 130 `Effect_StealKamas` (sort « Tuerie » du Poutch Ingball) : sans conséquence (les 5 lignes de 9999 dégâts tuent quand même).
- Les sorts passifs ne sont jamais listés sur les monstres dans le client (Monsters.d2o) : l'attribution d'un passif à un monstre
  se déduit du nom, des filtres `F<id>` du sort et de la page.
- **Bug transversal 1 (sûr)** : `LifeCriterion` (`game/effects/handlers/Spells/Targets/LifeCriterion.cs`) ignore le préfixe `*`
  (« vérifier sur le lanceur ») : `TargetCriterion.ParseCriterion` lit `caster` mais fait `new LifeCriterion(x, true/false)` sans le passer.
  Un masque `A,o,*v50` teste donc la vie de l'ATTAQUANT au lieu de celle du monstre. Touche Rage Mahal (XLII).
- **Bug transversal 2 (à confirmer en jeu)** : masque `o`. Le serveur traduit `o` par `LastAttackerCriterion(false)` = « tout sauf le
  dernier attaquant » et `O` par « le dernier attaquant ». Or dans le client 2.51, `o` sert partout à viser l'attaquant d'un sort déclenché
  (Rage Mahal 5038/5044/5047, Heure du temps 5101 « téléporte symétriquement celui qui frappe l'Auroraire », Bulle agrippante 5091,
  Refoulage 2552, Carapace gardée 4745, Frikt 3356). Avec la traduction actuelle, Rage Mahal du Seith attirerait TOUS les ennemis sauf l'attaquant.

---

## Grotte Hesque (grotte-hesque) — boss Corailleur Magistral (id 1027)
### Mécanique officielle (site)
- **Corailleur** (1022) : Coraillement 60 eau croix 1 autour de lui, -10 % rés. eau 3 tours ; Frappe de Corail 90 eau cercle 6 autour de lui,
  -1 PM 1 tour, relance 6 ; Lancer de Corail 50 eau, -1 PM, 10 PO.
- **Crustorails** (Kouraçao 1060 eau, Malibout 1061 terre, Morito 1063 air, Passaoh 1062 feu) : Pince de Corail 60 dégâts dans SON élément
  au CàC + 3 à 8 dommages aux alliés cercle 5 pendant 5 tours ; Protection de Corail +20 % rés. dans SON élément cercle 5, 3 tours, relance 5.
- **Palmifleurs** (Kouraçao 1064, Malibout 1065, Morito 1067, Passaoh 1066) : Décapsulation -100 % rés. tous éléments 2 tours, 8 PO,
  relance 4 (désenvoûtable) ; Fleur des îles 90 vol de vie dans SON élément cercle 2 + soin 60 aux alliés, 2 PO.
- **Boss** : 8 PA de base. Coraillement Magistral : +12 PA et +400 puissance 4 tours, perd ses 3 PM à chaque fin de tour pendant 4 tours,
  relance 5. Frappe de Corail Magistrale (16 PA, CàC) : 20 000 vol de vie air puis OS (même invulnérable). Lancer de Corail Magistral 25 air,
  -30 agilité 2 tours, 5 PO. Inoffensif à distance.
### État sur le serveur
- Boss conforme au 2.51 : 1026 (+12 PA, +400, -3 PM déclencheur TE), 1028 (9000 vol de vie air + effet 141 Tuer), 1027. Corailleur conforme.
- **Élément des Crustorails / Palmifleurs faux.** Dans le client, l'élément est porté par le NIVEAU du sort : 1030 Pince de Corail, 1031
  Protection de Corail, 1032 Fleur des îles → niv 1 neutre, niv 2 air, niv 3 eau, niv 4 feu, niv 5 terre. En base (`monsters_spells`)
  Level = grade pour les 8 monstres : l'élément dépend donc de la salle (salle 1 = neutre, salle 2 = air…), plus du type de monstre.
  C'est exactement le problème déjà corrigé pour les Tynrils (Level fixe par monstre : 1, 2, 3, 5).
- Décapsulation 933 : un seul niveau, conforme.
### À faire (précis, prêt à coder)
1. [IMPORTANT] `monsters_spells` : Level fixe par variante (même méthode que les Tynrils), puis redémarrage du World :
   ```sql
   UPDATE monsters_spells s JOIN monsters_grades g ON g.Id=s.MonsterGradeId SET s.Level=3 WHERE g.MonsterId IN (1060,1064) AND s.SpellId IN (1030,1031,1032); -- Kouraçao = eau
   UPDATE monsters_spells s JOIN monsters_grades g ON g.Id=s.MonsterGradeId SET s.Level=5 WHERE g.MonsterId IN (1061,1065) AND s.SpellId IN (1030,1031,1032); -- Malibout = terre
   UPDATE monsters_spells s JOIN monsters_grades g ON g.Id=s.MonsterGradeId SET s.Level=2 WHERE g.MonsterId IN (1063,1067) AND s.SpellId IN (1030,1031,1032); -- Morito = air
   UPDATE monsters_spells s JOIN monsters_grades g ON g.Id=s.MonsterGradeId SET s.Level=4 WHERE g.MonsterId IN (1062,1066) AND s.SpellId IN (1030,1031,1032); -- Passaoh = feu
   ```
   (Annulation : `SET s.Level=g.GradeId` sur les mêmes lignes.) Les dégâts sont identiques à tous les niveaux : seul l'élément change.

---

## Grozilla et Grasmera (grozilla-et-grasmera) — boss Grozilla (id 3142) et Grasmera (id 3143)
### Mécanique officielle (site)
- Accès par quête + énigme des dalles (événement Vulkania), 1 fois par jour. 2 salles de Krokilles (une de chaque sorte : Humide, Insipide,
  Boueuse, Incandescente, Sèche, quel que soit le nombre de joueurs ; Juvéniles/Novices/Matures/Vénérables selon la tranche d'entrée),
  puis G.O. Malikila fait choisir la version : Somnambules (3295/3298), Épuisés (3294/3297), Fatigués (3293/3296), normaux (3142/3143).
- **Grozilla** : Gravité attire de 4 (ligne, 2-5 PO, 2/tour) ; sur cible **Météoroïde** → annule son prochain tour, sinon minimise ses jets 1 tour.
  Tyrannisation 150/250/375/500 neutre croix 1 au CàC + état **Tyrannisé** ; si la cible est Météoroïde : +50 % de ses PV actuels et perd l'état.
- **Grasmera** : Immaturation invoque une Krokille Juvénile aléatoire (1 PO) ; Magnétisme se tire de 4 sur la cible (mêmes règles avec
  **Tyrannisé**) ; Météore 100/200/350/500 feu 4 PO + **Météoroïde** (+50 % PV actuels si Tyrannisé).
- Version normale seulement : **Lien Fusionnel** (si l'un meurt, l'autre reçoit **Rupture** infini) + Fission (en Rupture, OS un ennemi par tour) ;
  **Atomes Crochus** (Grozilla, tour 5 puis tous les 5 tours) : si au tour suivant ils ne sont pas à 2 PO ou moins l'un de l'autre, Grasmera
  passe **Apocalypse** et lance Apocalypse (OS toute l'équipe). Ni portables ni échangeables, mais poussables.
### État sur le serveur
- **Boss absents du jeu.** Les 4 spawns du donjon pointent sur de mauvais grades :
  499 (74977793, « nivel 200 ») = Peluche Tofu 3107 + Peluche Bouftou 3108 ; 500 (74977797) = Greg Emilioner 3227 + Chef Gludotek 3230 ;
  501 (74975745) = Spik Izzy 3228 + Glupperkut 3231 ; 502 (74976771) = Znarfak Fak 3229 + Bulbig Brozeur 3232.
  Aucun Grozilla / Grasmera / Krokille n'est posé nulle part (`monsters_spawns_dungeons_groups`).
- Chaîne actuelle : 502 (Antre de guerre) → 501 (Antre de guerre) → 500 (Antre de Grozilla et Grasmera) → 499 (Antre de G&G) → 76285958 (sortie).
  Le client a 8 cartes « Antre de guerre » et 11 « Antre de Grozilla et Grasmera » : le choix de tranche / de version (G.O. Malikila) n'est pas reproduit.
- Sorts en base conformes au client pour les 8 boss (Gravité 3015, Atomes Crochus 2990 → Réaction en Chaîne 3005 zone `I3/I4`, Fission 2984,
  Tyrannisation 2986, Maturation 2988, Magnétisme 3016, Apocalypse 2991, Météore 2987, Immaturation 2989). Atomes Crochus a bien
  `init 4` (tour 5) et relance 5 ; ses états ne visent que `F3142,F3143` → inactif sur les 3 versions faibles, comme le dit le site.
- **Lien Fusionnel 2982 (2.51)** : passif (déclencheur `X` → lance 3000 Division = état Rupture 104 sur `F3142,F3143`) attribué à personne,
  aucun cerveau → Fission ne se déclenche jamais.
- Client : `canBeCarried = false` pour 3142/3143 ; le serveur n'a aucune notion de « portable » par monstre (aucune occurrence de CanBeCarried).
### À faire (précis, prêt à coder)
1. [CRITIQUE] Remplacer les groupes faux (choix utilisateur à confirmer : quelle version mettre, le PNJ de choix n'existant pas ; proposé = version normale) :
   ```sql
   DELETE FROM monsters_spawns_dungeons_groups WHERE DungeonSpawnId IN (499,500,501,502);
   -- salle 1 (502) : 5 Krokilles Vénérables grade 1, groupe fixe
   INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId,MonsterGradeId,MinPartyMembers,Informations) VALUES
    (502,7292,1,'MYSTIC lot4 grozilla salle 1'),(502,7297,1,'MYSTIC lot4 grozilla salle 1'),(502,7302,1,'MYSTIC lot4 grozilla salle 1'),
    (502,7307,1,'MYSTIC lot4 grozilla salle 1'),(502,7312,1,'MYSTIC lot4 grozilla salle 1'),
   -- salle 2 (501) : 5 Krokilles Vénérables grade 3
    (501,7294,1,'MYSTIC lot4 grozilla salle 2'),(501,7299,1,'MYSTIC lot4 grozilla salle 2'),(501,7304,1,'MYSTIC lot4 grozilla salle 2'),
    (501,7309,1,'MYSTIC lot4 grozilla salle 2'),(501,7314,1,'MYSTIC lot4 grozilla salle 2'),
   -- boss (500) : Grozilla + Grasmera, grade 1→5 selon le palier (modèle Givrefoux)
    (500,7202,1,'MYSTIC lot4 grozilla boss'),(500,7207,1,'MYSTIC lot4 grozilla boss'),(500,7203,5,'MYSTIC lot4 grozilla boss'),(500,7208,5,'MYSTIC lot4 grozilla boss'),
    (500,7204,6,'MYSTIC lot4 grozilla boss'),(500,7209,6,'MYSTIC lot4 grozilla boss'),(500,7205,7,'MYSTIC lot4 grozilla boss'),(500,7210,7,'MYSTIC lot4 grozilla boss'),
    (500,7206,8,'MYSTIC lot4 grozilla boss'),(500,7211,8,'MYSTIC lot4 grozilla boss');
   UPDATE monsters_spawns_dungeons SET TeleportMapId=76285958, TeleportCell=371 WHERE Id=500;  -- la salle du boss mène à la sortie
   DELETE FROM monsters_spawns_dungeons WHERE Id=499;                                          -- 2e salle de boss inutile
   ```
   Autres versions (même forme, remplacer les grades du boss) : Fatigués 3293 = 7754-7758 / 3296 = 7769-7773 ; Épuisés 3294 = 7759-7763 /
   3297 = 7774-7778 ; Somnambules 3295 = 7764-7768 / 3298 = 7779-7783. Krokilles Juvéniles 3145-3149 g1 = 7217/7222/7227/7232/7237 ;
   Novices 3150-3154 g1 = 7242/7247/7252/7257/7262 ; Matures 3155-3159 g1 = 7267/7272/7277/7282/7287. Sauvegarde SAVE/ avant passage.
2. [CRITIQUE] Cerveau Lien Fusionnel, dans `AI/Fights/Brain/Custom/Boss/PassifsDonjonsBrains.cs` :
   ```csharp
   /// <summary>Grozilla / Grasmera : Lien Fusionnel (à la mort de l'un, l'autre passe Rupture et peut lancer Fission).</summary>
   [BrainIdentifier((int)MonsterIdEnum.GROZILLA)] public class GrozillaBrain : BrainPassif { public GrozillaBrain(AIFighter f) : base(f, 2982) { } }
   [BrainIdentifier((int)MonsterIdEnum.GRASMERA)] public class GrasmeraBrain : BrainPassif { public GrasmeraBrain(AIFighter f) : base(f, 2982) { } }
   ```
   (Ne pas le mettre sur les 6 versions faibles : 3000 ne vise que `F3142,F3143`, ce serait sans effet.) Test : tuer Grozilla seul → Grasmera
   doit avoir l'état Rupture et lancer Fission au tour suivant. Le déclencheur `X` lance le sort depuis le mort avec `Force = true` (accepté par `CastSpell`).
3. [MINEUR] Empêcher le porter du Pandawa sur 3142/3143 (client `canBeCarried=false`) : à défaut d'un drapeau serveur, ajouter dans les 2 cerveaux
   `EtatsBoss.Ajouter(Fighter, Fighter, (int)SpellStatesEnum.LOURD_63, 2982)` au début du combat (comme BoostacheBrain). Le site précise qu'ils
   ne sont pas « Lourds » visuellement : petite différence d'affichage, à valider par l'utilisateur.
- [DOUTE VERSION] Valeurs de dégâts par version (150/250/375/500…) : le 2.51 a un seul niveau de sort par boss ; non touché.

---

## Horologium de XLII (horologium-de-xlii) — boss XLII (id 3849)
### Mécanique officielle (site)
- Dimension à vagues (5 vagues, invulnérables 1 tour à l'arrivée).
- **Rage Mahal** sur tous les monstres sauf XLII, à chaque ligne de dégâts subie : Seith (3843) >50 % : attire l'attaquant de 1 / ≤50 % : +50 dommages
  critiques 1 tour ; Soissanth (3844) >50 % : -2 PO à l'attaquant / ≤50 % : +1 PM et +10 fuite 1 tour ; Trantroa (3842) >50 % : +1 PM, +10 tacle /
  ≤50 % : +10 % vitalité 2 tours ; Trezz (3840) >50 % : +3 % rés. 2 tours / ≤50 % : +50 puissance infini ; Vindeux (3841) >50 % : soigne les alliés
  cercle 2 de 10 % de ses PV / ≤50 % : l'attaquant répartit 50 % des dégâts en cercle 2.
- Sorts : Seith Tourbête 100 air + 100 eau 3 PO, Massue 100 terre/feu/neutre CàC ; Soissanth Poinzon poison 45 neutre 2 tours 5-10 PO,
  Taplaudissement poison air par PA ligne 5-10 PO ; Trantroa Brute Haleine 250 neutre croix 1 + 33 % PV manquants (ligne 3 PO), Poing terne idem feu en diagonale ;
  Trezz Rayons entravants 350 feu, vole 4 PM, ligne 5 PO sans LdV, Rebond manqué 400 terre vol de vie + Insoignable ; Vindeux Massurance 300 air +
  Pesanteur, Mécontentement 300 eau -3 PA.
- **Boss** : Buff heure (début de combat) invoque l'Auroraire, Lourd infini, état **Dérèglement** s'il est déplacé (débloque Dérèglement = échange
  avec un allié à portée infinie). Coquetterie 500 air cercle 2, -1 tour d'effets ; Souffle démoniaque 650 eau + **Pacifiste** 1 tour (3 PO).
- **Auroraire** : invulnérable, indéplaçable ; avance d'une heure à chaque tour de joueur ; heure 12 → 11 : dégâts subis des monstres ×40 % … ×150 %,
  dégâts finaux des monstres -60 % … +60 % ; frapper l'Auroraire téléporte symétriquement ; meurt avec XLII.
### État sur le serveur
- XLII : `XliiBrain` pose 5106 (invocation Auroraire 3850 case 227 + Lourd + Dérèglement sur déplacement) ; `AuroraireXliiBrain` (3850) pose 5104
  (lance 5099 à chaque début de tour de joueur : multiplicateurs 1163 / 1171 / 1172 selon l'état d'heure, déplacement sur le cadran, 5101 quand on la frappe).
  Tous les effets et la zone `;NNN` sont gérés : conforme sur le papier.
- **Rage Mahal jamais lancé** : 2.51 = 5050 (passif maître : 5 déclencheurs `D`, chacun filtré `F3840…F3844`, qui lancent 5034 Trezz, 5038 Vindeux,
  5041 Trantroa, 5044 Seith, 5047 Soissanth). Aucun cerveau pour 3840-3844. Les monstres arrivent aussi en vagues → il faut `SortPassif.Poser`.
- Ces sous-sorts utilisent `*v50` / `*V50` (vie du lanceur) et `o` (attaquant) : voir bugs transversaux 1 et 2 en tête de fiche.
- Vagues : 4 vagues en base (`monsters_spawns_dungeons_waves` 302) : pas de mécanique à corriger ici.
### À faire (précis, prêt à coder)
1. [IMPORTANT] Cerveaux Rage Mahal (`PassifsDonjonsBrains.cs`) :
   ```csharp
   /// <summary>Monstres de l'Horologium : Rage Mahal 5050 (effet selon la vie du monstre à chaque ligne de dégâts subie).</summary>
   [BrainIdentifier((int)MonsterIdEnum.TREZZ)] public class TrezzBrain : BrainPassif { public TrezzBrain(AIFighter f) : base(f, 5050) { } }
   [BrainIdentifier((int)MonsterIdEnum.VINDEUX)] public class VindeuxBrain : BrainPassif { public VindeuxBrain(AIFighter f) : base(f, 5050) { } }
   [BrainIdentifier((int)MonsterIdEnum.TRANTROA)] public class TrantroaBrain : BrainPassif { public TrantroaBrain(AIFighter f) : base(f, 5050) { } }
   [BrainIdentifier((int)MonsterIdEnum.SEITH)] public class SeithBrain : BrainPassif { public SeithBrain(AIFighter f) : base(f, 5050) { } }
   [BrainIdentifier((int)MonsterIdEnum.SOISSANTH)] public class SoissanthBrain : BrainPassif { public SoissanthBrain(AIFighter f) : base(f, 5050) { } }
   ```
2. [IMPORTANT] Corriger `LifeCriterion` pour le préfixe `*` (bug transversal 1) :
   - `TargetCriterion.ParseCriterion` : `case 'v': return new LifeCriterion(int.Parse(str.Remove(0, 1)), true) { Caster = caster };` et idem `'V'` avec `false`.
   - `LifeCriterion` : ajouter `public bool Caster { get; set; }` et dans `IsTargetValid` : `var cible = Caster ? handler.Caster : actor;` puis calculer
     le pourcentage sur `cible`. Sans ce correctif, la branche >50 % / ≤50 % de Seith, Soissanth et Vindeux suit la vie de l'attaquant.
3. [IMPORTANT, à tester d'abord] Masque `o` (bug transversal 2) : frapper un Seith à plus de 50 % ; si ce n'est pas l'attaquant qui est attiré
   (mais tous les autres ennemis), changer dans `TargetCriterion.ParseCriterion` `case 'o': return new LastAttackerCriterion(true);`.
   Changement global (48 sorts du client utilisent `o`) : retester ensuite Kolosso (Frikt 3356, jugé parfait), Lucrane (Carapace gardée 4745) et l'Auroraire de XLII.
- [DOUTE VERSION] Expéditions de guilde (Audace / Bravoure) : modificateurs absents du 2.51.

---

## Hypogée de l'Obsidiantre (hypogeacutee-de-lobsidiantre) — boss Obsidiantre (id 2924)
### Mécanique officielle (site)
- **Atomystique** (2866) : Daimocritique ressuscite le dernier monstre mort à 10 % (relance 2, meurt avec l'Atomystique) ; Neuthron +200 dommages 4 tours + état **Nukléon** ;
  Prothon 260 air + Affaibli au CàC. **Crapeur** (2865) : Crapitulation (soin ~250, +2 PM, +100 dommages 3 tours, attire de 8), Crapoute 250 terre vol de vie CàC
  -2 PA, Crapture 300 eau + attire de 8. **Fumrirolle** (2868) : Fumigène (minimise 2 tours), Fumisterie 250 neutre -100 fuite, Fumoir 150 terre vole 2 PO.
  **Mofette** (2867) : Démère (soin ou double dégâts 50/50, 2 tours), Dépère (téléport 6 PO + soin 200 autour), Duslipe 250 eau + 20 % érosion.
  **Solfataré** (2869) : Soufre douleur (**Solfasoin** : soigne les alliés cercle 3 quand il est frappé), Soufre hance 200 air ligne 3, Soufre latté 150 feu -3 PM.
- **Boss** : **Obligation** = invulnérable infini ; un PERSONNAGE poussé contre lui avec dommages de poussée → vulnérable 3 tours et +50 dommages 4 tours.
  Objurgation : pousser un monstre contre lui le soigne (2000) + 50 dommages. Objection 500 feu ligne 10 PO ; Scie licate échange 3 PO + 100-500 terre
  selon PV manquants ; Scie lisse : Pougnette (Lourd) tous les 3 tours, renvoie les coups (Tikette), explose à son 5e tour et OS tout ennemi à plus de 7 PO.
### État sur le serveur
- Déjà traité : `ObsidiantreBrain` (Obligation 673 → 676 / 672, réservé aux personnages) et `PougnetteBrain` (Clako 2602, explosion au 5e tour,
  distance 8 = choix utilisateur). Sorts 2.51 conformes (Scie lisse et Objection exigent l'état Invulnérable ; Andésite 2620 en plus).
- **Première salle (spawn 446, carte 57148161 « Hypogée - Première salle ») = groupe du Royalmouth** : Boufmouth légendaire 2852, Bouftonmouth 2853,
  **Royalmouth 2854 (boss, avec son cerveau)**, Shamansot 2855, grade 4, tous paliers. Ce n'est pas l'Hypogée (le Royalmouth a son propre
  donjon, spawns 229-233). Le rapport groupes_officiels note « obsi : Première salle : pas de capture » → jamais corrigée.
- Spawn 287 (Cinquième salle) sauté volontairement (raccourcissement validé) : rien à faire.
### À faire (précis, prêt à coder)
1. [IMPORTANT] Remplacer le groupe Royalmouth de la salle 1 par des monstres de l'Hypogée grade 1 (composition complète à chaque palier, à valider par l'utilisateur) :
   ```sql
   DELETE FROM monsters_spawns_dungeons_groups WHERE DungeonSpawnId=446;
   INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId,MonsterGradeId,MinPartyMembers,Informations) VALUES
    (446,6463,1,'MYSTIC lot4 obsi salle 1'),(446,6468,1,'MYSTIC lot4 obsi salle 1'),(446,6473,1,'MYSTIC lot4 obsi salle 1'),(446,6478,1,'MYSTIC lot4 obsi salle 1'),
    (446,6463,5,'MYSTIC lot4 obsi salle 1'),(446,6468,5,'MYSTIC lot4 obsi salle 1'),(446,6473,5,'MYSTIC lot4 obsi salle 1'),(446,6478,5,'MYSTIC lot4 obsi salle 1'),(446,6483,5,'MYSTIC lot4 obsi salle 1'),
    (446,6463,6,'MYSTIC lot4 obsi salle 1'),(446,6468,6,'MYSTIC lot4 obsi salle 1'),(446,6473,6,'MYSTIC lot4 obsi salle 1'),(446,6478,6,'MYSTIC lot4 obsi salle 1'),(446,6483,6,'MYSTIC lot4 obsi salle 1'),(446,6463,6,'MYSTIC lot4 obsi salle 1'),
    (446,6463,7,'MYSTIC lot4 obsi salle 1'),(446,6468,7,'MYSTIC lot4 obsi salle 1'),(446,6473,7,'MYSTIC lot4 obsi salle 1'),(446,6478,7,'MYSTIC lot4 obsi salle 1'),(446,6483,7,'MYSTIC lot4 obsi salle 1'),(446,6463,7,'MYSTIC lot4 obsi salle 1'),(446,6473,7,'MYSTIC lot4 obsi salle 1'),
    (446,6463,8,'MYSTIC lot4 obsi salle 1'),(446,6468,8,'MYSTIC lot4 obsi salle 1'),(446,6473,8,'MYSTIC lot4 obsi salle 1'),(446,6478,8,'MYSTIC lot4 obsi salle 1'),(446,6483,8,'MYSTIC lot4 obsi salle 1'),(446,6463,8,'MYSTIC lot4 obsi salle 1'),(446,6473,8,'MYSTIC lot4 obsi salle 1'),(446,6478,8,'MYSTIC lot4 obsi salle 1');
   ```
   (6463 Crapeur, 6468 Atomystique, 6473 Mofette, 6478 Fumrirolle, 6483 Solfataré, tous grade 1 comme la salle suivante 284.)
- [DOUTE VERSION] Objurgation : 301-400 PV en 2.51 contre 2000 sur le site. Pougnette « Tikette » 2603 (site) ≠ Clako 2602 utilisé : choix déjà fait, non touché.

---

## Village Kanniboul (village-kanniboul) — boss Kanniboul Ebil (id 2960)
### Mécanique officielle (site)
- **Ark** (211) : Coup de Masque 50 feu, repousse 5 (CàC) ; Tir à l'Arc Primitif 60 feu, -5 % rés., 6 PO sans LdV. **Eth** (212) : Éternuement Collatéral 60 air +
  état : la cible intercepte tous les dégâts reçus par l'Eth 1 tour (CàC, relance 2, débuff sur l'Eth) ; Fronde Poche 60 air, vole 50 agi ; Inspiration Vaudou
  +100 puissance cercle 2 (relance 4). **Jav** (213) : Ioushonotpasse bouclier 10 % PV cercle 2 ; Javeline Fulgurante 80 eau vol de vie ; Poussée Amicale 60 eau, repousse 1.
  **Sarbak** (214) : Cri Sauvage +3 PO cercle 2 ; Sarbakanniboul 50 terre + poison 12 terre par PA utilisé. **Tam** (4151) : Batou Kada 60 neutre cercle 2 ; Tambour Motivant (boost aléatoire).
- **Boss** : **Bénédiction Moonesque (passif)** : un monstre frappé soigne ses alliés cercle 2 de 50 % des dégâts. Bouboule 110 neutre + **Maudit** 1 tour
  (les dégâts subis par la Poupée sont renvoyés au Maudit), ligne 6 PO ; Ebilation (attire croix 3, 80 neutre, -20 % critique aux Maudits) ;
  Inspiration Moonesque : Poupée Aycetroy (une seule, réinvocable dès sa mort).
- **Poupée Aycetroy** (2961) : **Frénésie Moonesque** : à chaque fin de tour +50 puissance infini et 45 neutre cercle 2 (elle comprise) ; à sa mort, le boss peut la
  réinvoquer. Myopie Poupesque : échange de place + -5 PO 2 tours, relance 4.
### État sur le serveur
- Monstres conformes (sorts 2.51 identiques à la page, Éternuement 6095 → 6169 interception 765). Bouboule 2414 : Maudit + renvoi 100 % via 6170 sur la Poupée.
- **Frénésie Moonesque 2411 (2.51) jamais posée** : passif de la Poupée (`TE` → 6171 niv 1 : +50 puissance infini et 21-25 neutre cercle 2 sur elle et
  les ennemis ; `X` → 6171 niv 2 : remet à 0 la relance d'Inspiration Moonesque 2412 du boss). Or 2412 a une relance de 63 tours : sans ce passif
  le boss n'invoque **qu'une seule Poupée de tout le combat**, elle ne grossit pas et ne s'inflige rien (donc rien à renvoyer au Maudit).
- Bénédiction Moonesque 2413 est en 2.51 un sort actif (relance 4, effet 2 tours) lancé par l'IA, pas un passif.
- Susej (393) dans les salles 4 et 5 (absent du site) : composition gardée sur décision utilisateur (« Ne pas toucher », rapport groupes_officiels).
### À faire (précis, prêt à coder)
1. [CRITIQUE] Cerveau de la Poupée (invoquée en cours de combat → `SortPassif` via `BrainPassif`) :
   ```csharp
   /// <summary>Poupée Aycetroy (Kanniboul Ebil) : Frénésie Moonesque (+50 puissance et dégâts autour en fin de tour ; à sa mort le boss peut la réinvoquer).</summary>
   [BrainIdentifier((int)MonsterIdEnum.POUPEE_AYCETROY)] public class PoupeeAycetroyBrain : BrainPassif { public PoupeeAycetroyBrain(AIFighter f) : base(f, 2411) { } }
   ```
   Test : tuer la Poupée → le boss doit pouvoir en réinvoquer une au tour suivant (CooldownSet 1045 sur `F2960`).
- [DOUTE VERSION] Ebilation (absente du 2.51), Bénédiction Moonesque passive (2.51 = sort actif), chiffres (Poupée 21-25 au lieu de 45).

---

## Donjon des Kitsounes (donjon-des-kitsounes) — boss Tanukouï San (id 568)
### Mécanique officielle (site)
- Page « à réviser ». Kitsous Nere / Nae / Nufeu / Nakwa (100 % rés. dans un élément) : boost PA/PM/dommages + soin des alliés, frappe en ligne courte,
  invoquent un double puis deviennent invisibles. Salles 3-5 : équipe séparée en 3 (dalles). Salle 6 : les 4 sortes ensemble.
- **Boss** : Tanukouï San frappe à distance en zone (~600-700) ; invoque des Tanuki Chan qui poussent de 4 au CàC (gros dégâts contre un obstacle), donnent
  de la réduction fixe au boss dans un élément (et une faiblesse à un joueur). Faible en agilité.
### État sur le serveur
- **Boss absent** : aucun spawn ne contient Tanukouï San (rapport groupes_officiels : « boss Tanukouï San : pas de spawn en base »). La chaîne
  235 → 236 → 237 → 238 → 239 (Cinquième salle annexe 1, 18087940) téléporte directement à la Sortie 18089988 : le donjon se termine sans boss.
  Salle du boss du client = « Septième salle » 18088964 (Sixième salle = 18087938).
- Sorts 2.51 présents et gérés : Tanukick 559 (101-200 terre cercle 2), Tanushi 557 (101-200 air cercle 2, -1-2 PA), Phéromone 558 (Tanuki Chan aléatoire) ;
  Tanuki Chan : Pousse-pousse (repousse 8) + Bise (soin + réduction fixe 90 effets 240-244, désormais gérés). Doubles des Kitsous (562-565) sans sort (2.51).
### À faire (précis, prêt à coder)
1. [CRITIQUE] Créer la salle du boss et la brancher (composition à valider : le site parle de Kitsous avec le boss ; proposé = boss grade 1→5 + 2 Kitsous grade 5) :
   ```sql
   INSERT INTO monsters_spawns_dungeons (MapId,CellId,TeleportEvent,TeleportMapId,TeleportCell,TeleportDirection,Descripcion)
     VALUES (18088964,NULL,1,18089988,NULL,7,'Kitsun boss-sortie');
   SET @b = LAST_INSERT_ID();
   UPDATE monsters_spawns_dungeons SET TeleportMapId=18088964 WHERE Id=239;
   INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId,MonsterGradeId,MinPartyMembers,Informations) VALUES
    (@b,1948,1,'MYSTIC lot4 kitsou boss'),(@b,1803,1,'MYSTIC lot4 kitsou boss'),(@b,1798,1,'MYSTIC lot4 kitsou boss'),
    (@b,1949,5,'MYSTIC lot4 kitsou boss'),(@b,1803,5,'MYSTIC lot4 kitsou boss'),(@b,1798,5,'MYSTIC lot4 kitsou boss'),(@b,1808,5,'MYSTIC lot4 kitsou boss'),
    (@b,1950,6,'MYSTIC lot4 kitsou boss'),(@b,1803,6,'MYSTIC lot4 kitsou boss'),(@b,1798,6,'MYSTIC lot4 kitsou boss'),(@b,1808,6,'MYSTIC lot4 kitsou boss'),(@b,1768,6,'MYSTIC lot4 kitsou boss'),
    (@b,1951,7,'MYSTIC lot4 kitsou boss'),(@b,1803,7,'MYSTIC lot4 kitsou boss'),(@b,1798,7,'MYSTIC lot4 kitsou boss'),(@b,1808,7,'MYSTIC lot4 kitsou boss'),(@b,1768,7,'MYSTIC lot4 kitsou boss'),(@b,1803,7,'MYSTIC lot4 kitsou boss'),
    (@b,1952,8,'MYSTIC lot4 kitsou boss'),(@b,1803,8,'MYSTIC lot4 kitsou boss'),(@b,1798,8,'MYSTIC lot4 kitsou boss'),(@b,1808,8,'MYSTIC lot4 kitsou boss'),(@b,1768,8,'MYSTIC lot4 kitsou boss'),(@b,1803,8,'MYSTIC lot4 kitsou boss'),(@b,1798,8,'MYSTIC lot4 kitsou boss');
   ```
   (Tanukouï San 568 g1-5 = 1948-1952 ; Kitsou grade 5 : Nae 1803, Nere 1798, Nufeu 1808, Nakwa 1768.) Vérifier ensuite la sortie / le PNJ
   de fin (fins_de_donjon.tsv, sorties_donjons) : la carte de sortie ne change pas (18089988).
- [DOUTE VERSION] Sorts 11044-11048 (« Rêve / Paradoxe / Cauchemar du Tanukouï San ») = contenu récent, ne pas utiliser.

---

## Laboratoire de Brumen Tinctorias (laboratoire-de-brumen-tinctorias) — boss Nelween (id 3100)
### Mécanique officielle (site)
- Page « à réviser ». Monstres « modifiés » : Ouginak Déchaîné (désenvoûte, poison air, CàC eau, poussée air), Scorbute Renforcé (retrait de caractéristiques,
  poison), Kolérat de Laboratoire (renvoi, CàC), Corbac Terrien (distance, désenvoûte) + leurs formes normales.
- **Boss** : état posé au début du combat : soigner un allié lui fait perdre 1 PM ou 1 PA et minimise ses jets ; retrait PA, -% rés., frappe faible en vol de vie,
  minimise, renvoi de sorts sur ses alliés.
### État sur le serveur
- Salles : Scorbute 298, Croc Gland 299, Kolérat 300, Macien 301, Crowneille 3096 (formes 2.51 ; Corbac Terrien et « Ouginak » n'existent pas en 2.51) :
  décision utilisateur « Ne pas toucher » (rapport groupes_officiels).
- Nelween 2.51 : Mord Mollet (19-24 terre, -2 PM), Soin Diffus 8625 (vol de vie neutre + déclencheur `H` : si la cible est soignée, 8625 niv 2
  la blesse de 100 % du soin reçu en croix 1), Exhalation Toxique 2529 (-30 % rés. cercle 3 + interception 765). Tout est géré. Pas de passif 2.51.
### À faire (précis, prêt à coder)
Rien à faire.
- [DOUTE VERSION] État de début de combat « soin → perte PA/PM + minimise », renvoi de sorts : absents du 2.51 (le 2.51 punit le soin via Soin Diffus).
  Sorts 10894-10896 (« Rêve / Paradoxe / Cauchemar de Nelween ») = contenu récent.

---

## Laboratoire de Nileza (laboratoire-de-nileza) — boss Nileza (id 3397)
### Mécanique officielle (site)
- **Dodox** (3394) : Duvet Téran (500 bouclier + Lourd, état **Plumide** : 500 bouclier aux entités en croix 4 quand il est frappé), Picorection 650 feu + 30 % érosion CàC,
  Pligeon (téléport 3 PO, 550 feu croix 1, -1000 soins). **Drosérâle** (3396) : Bractérie (**Sporange mécanique** : l'attaquant est repoussé de 2 par ligne), Caduk 500 neutre
  vole 4 PM ligne 10 PO, Stigmatraque 500 terre vol de vie croix 2-4 + recul 2. **Krakal** (3393) : Jappement Dragor 650 terre vol de vie -50 fuite, Morsure de soie 20 % PV
  manquants eau + attire 1, Pelage de glace (**Figel** : déplacé → +200 dommages alliés cercle 3, +50 dommages et Pesanteur aux ennemis). **Nessil** (3392) : Bâfre 500 eau
  vol de vie attire 6 puis repousse 6, Drako (renvoi 400 par ligne à distance croix 1), Loch 600 terre +40 % dégâts subis. **Termystique** (3395) : Dard dard 600 air vol de vie -2 PM,
  Glusure poison 160 neutre par PM, Sérum à tout faire (+100 tacle, **Dard contemporain** : 900 eau -3 PA à l'attaquant au CàC).
- **Boss (passif Cohobation)** : Chifouchimie (finir son tour au contact d'un allié l'OS) ; Ogavodra (frappé à distance → échange avec l'attaquant ; glyphe rose) ;
  Molalité illéplochable (frappé à distance → 40 % érosion et 200 % des dégâts renvoyés en air cercle 2, après l'échange) ; Pacifiste aux joueurs éloignés
  aux tours 3 (>11 PO, -200 fuite), 5 (>9, -200 tacle), 7 (>7, Pesanteur), 9 puis tous les 2 tours (>5, Insoignable) ; Liqueur de Fée Ling (2 tours sur 3 : +300 dommages
  et 5 % PV par coup reçu au CàC) ; Mélange Instable (effets selon ses PV). Fraction de molaire 550 eau + 10 % PV manquants terre, repousse 3, ligne 12 PO ; Glace sèche
  échange 2 PO + 750 feu croix 1.
### État sur le serveur
- `NilezaBrain` lance Cohobation 3545 au début du combat : 3566 Ovagodra (`DR`), 3587 Molalité (`DR`, effet 1125 géré par RenvoiDommagesBruts.cs),
  3565 Chifouchimie (`TE` sur tous les ennemis), 3568 / 3569 / 3570 / 3583 avec `delay` 2/4/6/8 (zones `I12/I10/I8/I6`, Pacifiste 218). Tous les
  monstres et sorts conformes au 2.51.
### À faire (précis, prêt à coder)
Rien à faire (à tester en jeu : ordre échange puis renvoi, et apparition du Pacifiste aux tours 3/5/7/9).
- [DOUTE VERSION] Mélange Instable (aucun sort 2.51), glyphe rose d'Ogavodra (le 3566 2.51 n'a qu'un échange), chiffres (ex. Drako : renvoi fixe de 50 par coup à distance en 2.51 au lieu de 400).

---

## Laboratoire du Tynril (laboratoire-du-tynril) — boss Tynril Perfide (1086), Déconcerté (1085), Consterné (1072), Ahuri (1087)
### Mécanique officielle (site)
- Salles : Fécorce (poison par PA), Brouture (poison air), Nerbe (zone), Chiendent (boost PA/PM), Floribonde (faiblesses -50 %), Bitouf Sombre (retrait PA/PO, CàC),
  Abrakleur Sombre (CàC, gros sort).
- **Boss** : 4 Tynrils seuls, 200 % de rés. sauf dans un élément (Perfide feu, Déconcerté eau, Consterné air, Ahuri terre). Chacun met -2000 % de rés. dans son élément
  au joueur le plus proche (Hiffe) et frappe au CàC dans cet élément (OS) ; deux Tynrils au contact se reconstituent ; un Tynril se transpose avec un autre à son tour. 2 PM.
### État sur le serveur
- Conforme : Level fixe par Tynril en base (1086 → 3 feu, 1085 → 2 eau, 1072 → 1 air, 1087 → 5 terre) pour Hiffe 1058 / Helse 1059 ; Bisouille 1054
  (soin 10 000 allié au CàC) ; Forque 1060 (échange avec un allié, portée infinie) ; `TynrilBrain` pose l'icône d'élément. Salle du boss = 4 Tynrils seuls.
### À faire (précis, prêt à coder)
Rien à faire.
- [DOUTE VERSION] Sorts 11076-11078 (« Rêve / Paradoxe / Cauchemar des Tynrils ») = contenu récent.

---

## Donjon des Larves (donjon-des-larves) — boss Shin Larve (id 457)
### Mécanique officielle (site)
- Larves Saphir / Rubis / Émeraude / Dorée : Carapace (bouclier selon le niveau), Frappe 35-45 terre CàC, Larvement 20 eau -1 PA, Retour de flamme -20 % dégâts subis,
  Larvage (désenvoûte un allié), Larvure (soin 75).
- **Boss** : Convocation gluante (invoque Émeraude / Rubis / Saphir, 1 PO, relance 2, à partir du tour 2) ; Déglutition 120 neutre -3 PA repousse 4 (CàC) ;
  Enlisement -6 PM aux ennemis et +4 PA à tous, tous les 3 tours à partir du tour 2. Faible eau/air. 11 salles dont 7 combats, équipe séparée deux fois.
### État sur le serveur
- Conforme au 2.51 : Enlisement 506 (-6 PM aux ennemis, +4 PA à tous), Déglutition 510, Convocation gluante 507 (init 1, relance 2, 7 larves possibles
  dont Bleue / Verte / Orange / Jaune). Larves : Retour de flamme 2.51 = renvoi 15 (pas -20 %), Carapace = réduction 8 sur coups terre/neutre.
- 5 salles de combat en base (11 cartes dans le client) : choix déjà documenté par groupes_officiels.
### À faire (précis, prêt à coder)
Rien à faire.
- [DOUTE VERSION] Liste des invocations (3 sur le site, 7 en 2.51), Larvure 11-50 (2.51) au lieu de 75, Carapace en bouclier, Retour de flamme en réduction de dégâts.

---

## Maison du Papa Nowel (maison-du-papa-nowel) — boss Père Fwetar (id 1194)
### Mécanique officielle (site)
- Monstres de la page (version actuelle) : un Yiti (Colère du Yiti, Gueuleton), Cadob' (Surprise drainante / empoisonnante, invocation de Cadob'Imbo / Omb / Onux),
  un monstre glacé (Écrasement glacial, Pic de glace), **Peluche Bouftou** (3108 : Coutures Renforcées +500 vitalité quand on la détacle, Ecchymose 380 terre + Pesanteur,
  Traumatisme : érosion 30 % à chaque coup d'invocation), **Peluche Tofu** (3107 : Entorse -1 à 3 PM, Pique-Couic : minimise si on détacle), **Peluche Wabbit**
  (3109 : Cawotte, Malédiction de la Cawotte, Rembourrage soin 1000 / 1600 sur le Père Fwetar, Ruse du Wabbit).
- **Boss** : **Vilain Garnement** (début de combat, infini) : +1 PM à chaque tentative de retrait de PM. Aspir'nenfan attire 3 (ligne, 2 PO à infini) ;
  Embûche de Nowel se soigne entièrement + 100 puissance aux monstres (relance 10) ; Fwetage 700 élément aléatoire, 8 PO à infini sans LdV ; Infantophagie OS au CàC
  + soin 2100 ; Invocation de Jouet Cassé (Poutch 1500 PV, +1 PM) ; Parade des Vieux Jouets : 1/3 +2 PA au Poutch (à 4 PA il lance Tuerie = OS de tous),
  2/3 transformation en Marionnette (Bouftou Royal, Dark Vlad, Dragon Cochon, Minotoror, Mulou Meulé).
### État sur le serveur
- Sorts du boss conformes au 2.51 : 883, 882, 912, 913 (aléatoire 33 % PA / 5 × 13 % marionnette, `F5499`), 914 (Poutch 5499 grade 3 = 1500 PV, 0 PA de base), 2792.
  Marionnettes 1189-1193 et Poutch (Tuerie 411) présents ; effet 130 (vol de kamas) de Tuerie non géré, sans conséquence.
- **Vilain Garnement 2557 (2.51 : +1 PM infini, déclencheur `MPA` = tentative de retrait PM, géré)** attribué à personne, aucun cerveau.
- Salles = groupes tofus (Cadob', Cadob'Omb, Sakai Firefoux, Mini Nuit', peluches) : choix déjà appliqué, différent de la page (version récente).
### À faire (précis, prêt à coder)
1. [CRITIQUE] Cerveau du Père Fwetar :
   ```csharp
   /// <summary>Père Fwetar : Vilain Garnement (gagne 1 PM jusqu'à la fin du combat à chaque tentative de retrait de PM).</summary>
   [BrainIdentifier((int)MonsterIdEnum.PERE_FWETAR)] public class PereFwetarBrain : BrainPassif { public PereFwetarBrain(AIFighter f) : base(f, 2557) { } }
   ```
   Test : un retrait de PM esquivé ou réussi doit donner +1 PM durable au boss.
- [DOUTE VERSION] Infantophagie : soin 500 en 2.51 (2100 site) ; Embûche de Nowel : relance 63 en 2.51 (10 site) ; monstres des salles de la page (Yiti, monstre glacé) ≠ 2.51.

---

## Maison Fantôme (maison-fantome) — boss Boostache (id 2975)
### Mécanique officielle (site)
- Boostache Prépubère (Boorrade 40 neutre CàC ; **Rancune Totale** : état Rancune infini aux monstres, +2 PM), Gargrouille (Souffle gargouillesque 15 feu ligne 4,
  Gargouilli 30 air -100 puissance demi-cercle 2, Couteau ~3000 neutre CàC), Kwoan (Confusion Optique -6 PO 5 tours, Frappe Gerbante 60 neutre, Déplacement Libre),
  Tofu Maléfique (Béco Maléfique 60 air -15 tacle, Maléfice 20 % érosion + Insoignable croix 1, Punksnotdède esquive 100 % des coups en reculant), Vampire / Maître Vampire
  (Vol de Vie 35 eau, Force des Âmes Putrides +50 puissance, Nosfuraté 20 terre anneau 3-4 vole 50 force/chance).
- **Boss** : **Une nuit en enfer** : Lourd + Enraciné tout le combat (indéplaçable). Frayeurs repousse de 3 en ligne ; L'Enfer des Zombies ressuscite un monstre mort (chaque tour) ;
  Le Dentiste 60 air + 30 % érosion 2 tours ; Esprit empêtrant : Ashi-magari (-100 PM + Pesanteur aux 4 cases adjacentes).
### État sur le serveur
- Boss : `BoostacheBrain` pose Inébranlable + Lourd (choix utilisateur, à la place de 2443 Enraciné + Lourd) : rien à changer. Sorts 2444-2447 et Ashi-magari 2448 conformes.
- **Rancune Totale 7075 (2.51) jamais posée** : à la mort de son porteur (`X`) elle lance 7076 = état Rancune 437 + 2 PM infinis aux Gargrouilles (`F498`) ; elle donne aussi
  au porteur une faiblesse aléatoire -100 % dans un élément (12 effets `rnd 8`). Couteau 7077 de la Gargrouille EXIGE l'état Rancune 437 : aujourd'hui il n'est jamais lançable.
  Porteur déduit de la page (Rancune Totale listée avec Boorrade, sort du Boostache Prépubère 3235) : **attribution non certaine**.
- Tofu Maléfique 111 absent des salles : groupes tofus appliqués, choix utilisateur, non touché.
### À faire (précis, prêt à coder)
1. [IMPORTANT] Cerveau du Boostache Prépubère (porteur supposé, à confirmer par l'utilisateur) :
   ```csharp
   /// <summary>Boostache Prépubère : Rancune Totale (à sa mort, les Gargrouilles passent Rancune +2 PM et débloquent Couteau).</summary>
   [BrainIdentifier((int)MonsterIdEnum.BOOSTACHE_PREPUBERE)] public class BoostachePrepubereBrain : BrainPassif { public BoostachePrepubereBrain(AIFighter f) : base(f, 7075) { } }
   ```
   Test : tuer un Prépubère à côté d'une Gargrouille → la Gargrouille a l'état Rancune et peut lancer Couteau (90 % des PV de la cible, effet 1071) 2 tours plus tard (init 2).
- [DOUTE VERSION] Valeurs des sorts des monstres (ex. Couteau « 3000 » = 90 % PV en 2.51).

---

## Mégalithe de Fraktale (meacutegalithe-de-fraktale) — boss Fraktale (id 3852)
### Mécanique officielle (site)
- Dimension à vagues (tous les 5 tours : tours 6, 11, 16, 21). Tous les Xélomorphes (Gromorso 3854, Farfacette 3855, Brikablak 3856, Éklatleth 3857, Segmantid 3858) ont
  **Fragmentation** (début de combat, infini) : dégâts subis ×70 % et renvoi des dégâts en cercle 2 (alliés compris). Sorts : Gromorso Massassine (OS au CàC), Transmission
  instantanée (échange 2 PO + 130 air), Échauffement songeur ; Farfacette Farce cachée, Tornadhésive (vole 30 terre, -2 PM), Cercle de lumière (100 feu, dégâts subis ×150 %) ;
  Brikablak Assurance, Pluixel 100 neutre, Attaque surprise 150 eau (soigne ceux qui frappent la cible) ; Éklatleth Bulle agrippante, Secoust (repousse 4), Éklatlatête (repousse 6) ;
  Segmantid Feu usé, Segmentation 100 air + 20 % érosion.
- **Boss** : invoque une Auroraire sur le I du cadran ; à chaque tour de joueur elle avance (dégâts normaux) ou recule (les sorts de dégâts soignent tout le monde) selon la
  distance Fraktale-Auroraire (4 PO) et les PV du boss (>50 % : loin = avance, près = recule ; ≤50 % : l'inverse).
### État sur le serveur
- Boss : `FraktaleBrain` lance 5112 (case 283 : invocation Auroraire 3853, +1 PM sur déplacement, ×70 %, renvoi 5051) ; `AuroraireBrain` (3853) lance 5111 → 5110 → 5108 →
  5107 (avance) / 5109 (recule + HealWhenAttack 1164) avec `F3852,V50/v50` et zones `C4,1` / `C63,5` : conforme.
- **Fragmentation des monstres 5052 (2.51 : ×70 % + renvoi 5051 cercle 2) jamais posée** sur 3854-3858 (aucun cerveau ; ils arrivent aussi en vagues).
### À faire (précis, prêt à coder)
1. [IMPORTANT] Cerveaux des Xélomorphes (`PassifsDonjonsBrains.cs`) :
   ```csharp
   /// <summary>Xélomorphes du Mégalithe de Fraktale : Fragmentation 5052 (dégâts subis x70 %, renvoi en cercle 2).</summary>
   [BrainIdentifier((int)MonsterIdEnum.GROMORSO)] public class GromorsoBrain : BrainPassif { public GromorsoBrain(AIFighter f) : base(f, 5052) { } }
   [BrainIdentifier((int)MonsterIdEnum.FARFACETTE)] public class FarfacetteBrain : BrainPassif { public FarfacetteBrain(AIFighter f) : base(f, 5052) { } }
   [BrainIdentifier((int)MonsterIdEnum.BRIKABLAK)] public class BrikablakBrain : BrainPassif { public BrikablakBrain(AIFighter f) : base(f, 5052) { } }
   [BrainIdentifier((int)MonsterIdEnum.EKLATLETH)] public class EklatlethBrain : BrainPassif { public EklatlethBrain(AIFighter f) : base(f, 5052) { } }
   [BrainIdentifier((int)MonsterIdEnum.SEGMANTID)] public class SegmantidBrain : BrainPassif { public SegmantidBrain(AIFighter f) : base(f, 5052) { } }
   ```
   (Ne pas utiliser 5112 pour eux : il invoquerait une Auroraire.) Les sous-sorts 5091 (Bulle agrippante) et 5058 (Farce cachée) utilisent `o` / `O` : voir bug transversal 2.

---

## Récapitulatif
| Donjon | CRITIQUE | IMPORTANT | MINEUR |
|---|---|---|---|
| Grotte Hesque | 0 | 1 | 0 |
| Grozilla et Grasmera | 2 | 0 | 1 |
| Horologium de XLII | 0 | 3 | 0 |
| Hypogée de l'Obsidiantre | 0 | 1 | 0 |
| Village Kanniboul | 1 | 0 | 0 |
| Donjon des Kitsounes | 1 | 0 | 0 |
| Laboratoire de Brumen Tinctorias | 0 | 0 | 0 |
| Laboratoire de Nileza | 0 | 0 | 0 |
| Laboratoire du Tynril | 0 | 0 | 0 |
| Donjon des Larves | 0 | 0 | 0 |
| Maison du Papa Nowel | 1 | 0 | 0 |
| Maison Fantôme | 0 | 1 | 0 |
| Mégalithe de Fraktale | 0 | 1 | 0 |
| **Total** | **5** | **7** | **1** |
