# Groupes de monstres officiels (tofus.fr) — rapport

Source : https://www.tofus.fr/donjons (captures des salles : infobulle du groupe, lignes orange = groupe pour 1 à 4 joueurs, lignes grises = monstres ajoutés pour 5, 6, 7, 8 joueurs dans l'ordre).
Table : `monsters_spawns_dungeons_groups` (paliers MinPartyMembers 1 / 5 / 6 / 7 / 8, composition complète à chaque palier, Informations = `MYSTIC officiel <slug> <salle>`).
Pris en compte au **prochain redémarrage du World**.

## Bilan (17/10, mis à jour 13h15)
- **71 donjons / 326 salles appliqués** (lot 4 consolidé, qui reprend les lots 1-3) + Givrefoux et Korriandre faits avant.
- Les 17 donjons « en attente » sont appliqués avec la règle **position relative** (choix de l'utilisateur), détail salle par salle plus bas.
- Salles sans liste grise **complétées jusqu'à 8** (consigne utilisateur) : Bulbes salle 3 (base 2), Skeunk salle du boss (base 6). Les salles « boss seul » (Péki, Kralamoure, Maître des Pantins, 4 Tynrils) n'ont aucun monstre non-boss à reprendre : inchangées (pas de doublon de boss).
- **Non appliqués** (monstre absent du 2.51, absent, hors donjon, dimensions) : voir plus bas.
- **Raccourcissement à 4 salles + boss : appliqué** (Obsidiantre, Dragon Cochon, Ougah), voir la section dédiée.

## Méthode et outils
- `releves.txt` : une ligne par salle relevée (slug | spawn | nom de salle de la capture | orange / gris). `?` = spawn retrouvé par le nom de carte du client 2.51 ; numéro forcé quand le nom diffère (indiqué en commentaire). `#X` = ligne écartée.
- `generer_sql.py <lot> <slugs...>` : contrôle (nom de carte, monstre connu, niveau = un grade 2.51) et génère `groupes_officiels_<lot>.sql`. Une salle en erreur n'est ni effacée ni réécrite. Refuse les spawns interdits (Vortex 397, Givrefoux/Korriandre, donjons à vagues).
- Monstre → id par son nom français (Monsters.d2o) ; niveau affiché → grade 2.51 de même niveau. Boss (isBoss) : grade 1 → 5 selon le palier (1, 5, 6, 7, 8), comme le modèle Givrefoux/Korriandre.
- Plus de 4 lignes orange avec une liste grise (capture prise à 5-6 joueurs) : les 4 premières = base, les suivantes = paliers 5, 6… Plus de 4 orange **sans** gris : groupe fixe (Skeunk).
- Moins de 4 gris : les paliers suivants gardent la composition du dernier palier. Moins de 4 orange : base = les orange (ex. annexes Kitsounes, Craqueleurs).
- Aides : `exporter_donnees.py` (salles.tsv, monstres.tsv), `cartes_donjon.py <nom>` (cartes d'un donjon + spawns).

## Lots appliqués
| Lot | Heure | Donjons | Salles | Sauvegarde (SAVE/1_BASE_DE_DONNEES/) | Paquet |
|---|---|---|---|---|---|
| lot1 | 12h27 | 21 | 93 | 2026-10-17_12h27_world_avant-groupes-officiels-lot1 (restaurer_lot1.sql) | 2_BDD/21_groupes_officiels_lot1.sql |
| lot2 | 12h50 | 25 | 118 | 2026-10-17_12h50_world_avant-groupes-officiels-lot2 (restaurer_lot2.sql) | 2_BDD/22_groupes_officiels_lot2.sql |
| lot3 | 12h57 | 8 | 40 | 2026-10-17_12h57_world_avant-groupes-officiels-lot3 (restaurer_lot3.sql) | 2_BDD/23_groupes_officiels_lot3.sql |
| lot4 (consolidé) | 13h00 | 71 | 326 | 2026-10-17_13h00_world_avant-groupes-officiels-lot4 (restaurer_lot4.sql) | 2_BDD/24_groupes_officiels_lot4.sql (à passer après 21-23) |

## Détail des donjons appliqués
- **lot1** : gr (salle des Gelées Royales seulement : salles 1-4 = plusieurs groupes par salle sur tofus, non reproductible avec 1 spawn), larve (salles 1, 5, 6, 9, 11), chene-mou (= page cm), rb, mp, kitsou (salles 1, 2, annexes 1 ; salle 3 sans groupe sur tofus ; **boss Tanukouï San : pas de spawn en base**), rn, chouque, comte (Banquet, Glaçons, Horlogerie, Grande aiguille ; Tocante = dialogue), koulosse, tr, moon, merkator (ordre du parcours), mansot, dc (salles 3, 5, 13 ; salles 1, 2, 4 sans groupe sur tofus), forgeron (1, 2, 4, 7, finale), foux (Péki Péki seul), domaine-ancestral (= page aa), nowel2, kolosso, cache-de-kankreblath.
- **lot2** : antre-de-la-reine-nyee, chateau-wab (la page « wa » est une ancienne version aux niveaux ≠ 2.51), bwork, bulbe (salle 3 = 2 monstres fixes ; salle 5 sans groupe), multi, hesque, croca (salle 4 : capture absente de tofus ; dofuspourlesnoobs décrit la version refondue « Antre de Crocabulia » aux monstres absents du 2.51 → inexploitable, salle inchangée), mout (boss tofus « Septième salle » = Sixième salle 2.51), mine-sakai, kimbo, incarnam, dramak, tyn (boss : infobulle tofus coupée → **composition prise sur dofuspourlesnoobs** : 4 Tynrils seuls, appliquée au lot 4), violent, glourseleste, terrier-wa (la page « cawotte » est l'ancienne version), halouine, ventre-de-la-baleine, rasboul, missiz, kanigroula (salle de banquet non appliquée : capture tofus identique à la précédente ; dofuspourlesnoobs n'a qu'une image sans noms ni niveaux, version moderne avec Alhyène → inexploitable), nileza, obsi (Première salle : pas de capture), nowel3, nowel1.
- **lot3** : ombre, palais-de-dantinea, klime, skeunk, vaisseau-du-capitaine-meno, sylargh, temple-de-koutoulou, truche.
- Noms de salle absents de certaines captures (merkator, nowel1/2, chateau-wab, terrier-wa, kanigroula, halouine salle 4, ombre) : rattachement par l'ordre du parcours (chaîne des téléportations) et niveaux croissants.

## Appliqués avec la règle « position relative » (lot 4)
Règle : niveau tofus identique à un grade 2.51 → ce grade ; sinon le plus petit niveau vu pour ce monstre dans le donjon = grade 1, puis +1 grade par pas de niveau 2.51 (arrondi), borné à 1–5.

### Détail salle par salle ([spawn] monstre niveau tofus -> grade retenu)
```
[36] Tofukaz 30 -> grade 1 (32 en 2.51)
[36] Tofu Noir 20 -> grade 1 (32 en 2.51)
[36] Tofu 12 -> grade 1 (32 en 2.51)
[37] Tofu Noir 22 -> grade 2 (34 en 2.51)
[38] Tofoune 44 -> grade 1 (32 en 2.51)
[38] Tofu Ventripotent 44 -> grade 1 (32 en 2.51)
[38] Tofu 14 -> grade 2 (34 en 2.51)
[39] Tofoune 46 -> grade 2 (34 en 2.51)
[39] Tofu Ventripotent 46 -> grade 2 (34 en 2.51)
[39] Tofu Noir 26 -> grade 4 (38 en 2.51)
[39] Tofu 15 -> grade 3 (36 en 2.51)
[40] Tofoune 48 -> grade 3 (36 en 2.51)
[40] Tofu Ventripotent 48 -> grade 3 (36 en 2.51)
[40] Tofu Noir 28 -> grade 5 (40 en 2.51)
[40] Tofu 16 -> grade 3 (36 en 2.51)
[313] Champ à Gnons 179 -> grade 1 (172 en 2.51)
[313] Champodonte 173 -> grade 1 (172 en 2.51)
[314] Champmane 184 -> grade 1 (172 en 2.51)
[314] Champaknyde 169 -> grade 1 (172 en 2.51)
[317] Champodonte 177 -> grade 3 (176 en 2.51)
[318] Champmane 186 -> grade 2 (174 en 2.51)
[318] Champ à Gnons 183 -> grade 3 (176 en 2.51)
[318] Champodonte 177 -> grade 3 (176 en 2.51)
[319] Champmane 190 -> grade 4 (178 en 2.51)
[319] Champbis 184 -> grade 5 (180 en 2.51)
[319] Champ à Gnons 187 -> grade 5 (180 en 2.51)
[319] Champodonte 181 -> grade 5 (180 en 2.51)
[319] Champaknyde 175 -> grade 4 (178 en 2.51)
[33] Chafer Draugr 40 -> grade 1 (42 en 2.51)
[34] Chafer Draugr 43 -> grade 3 (46 en 2.51)
[35] Chafer Draugr 43 -> grade 3 (46 en 2.51)
[29] Scarafeuille Noir 46 -> grade 1 (36 en 2.51)
[30] Scarafeuille Noir 48 -> grade 2 (38 en 2.51)
[208] Gamino 48 -> grade 1 (120 en 2.51)
[208] Serpiplume 58 -> grade 1 (120 en 2.51)
[208] Scaratos 38 -> grade 1 (110 en 2.51)
[283] Gamino 48 -> grade 1 (120 en 2.51)
[283] Scaratos 38 -> grade 1 (110 en 2.51)
[11] Crabe 12 -> grade 1 (42 en 2.51)
[11] Étoile de la Mer d'Asse 10 -> grade 1 (42 en 2.51)
[12] Crabe 13 -> grade 2 (44 en 2.51)
[12] Étoile de la Mer d'Asse 10 -> grade 1 (42 en 2.51)
[12] Pichon Blanc 10 -> grade 1 (16 en 2.51)
[12] Pichon Bleu 10 -> grade 1 (16 en 2.51)
[12] Crabe 12 -> grade 1 (42 en 2.51)
[12] Pichon Vert 10 -> grade 1 (16 en 2.51)
[12] Pichon Orange 10 -> grade 1 (16 en 2.51)
[13] Pichon Bleu 15 -> grade 5 (20 en 2.51)
[13] Pichon Orange 15 -> grade 5 (20 en 2.51)
[13] Pichon Vert 15 -> grade 5 (20 en 2.51)
[13] Pichon Blanc 15 -> grade 5 (20 en 2.51)
[14] Pichon Kloune 25 -> grade 1 (16 en 2.51)
[14] Étoile de la Mer d'Asse 15 -> grade 4 (48 en 2.51)
[14] Pichon Orange 15 -> grade 5 (20 en 2.51)
[14] Pichon Vert 15 -> grade 5 (20 en 2.51)
[14] Pichon Blanc 15 -> grade 5 (20 en 2.51)
[14] Pichon Bleu 15 -> grade 5 (20 en 2.51)
[15] Raul Mops 40 -> grade 1 (42 en 2.51)
[15] Pichon Kloune 30 -> grade 5 (20 en 2.51)
[15] Étoile de la Mer d'Asse 20 -> grade 5 (50 en 2.51)
[320] Tourbassingue 107 -> grade 1 (100 en 2.51)
[320] Bourbassingue 97 -> grade 1 (95 en 2.51)
[320] Le Ouassingue Entourbé 93 -> grade 1 (92 en 2.51)
[321] Tourbassingue 114 -> grade 3 (106 en 2.51)
[321] Bourbassingue 102 -> grade 3 (101 en 2.51)
[308] Bwork Élémental d'Eau 171 -> grade 1 (172 en 2.51)
[308] Bwork Élémental de Terre 171 -> grade 1 (172 en 2.51)
[308] Mama Bwork 170 -> grade 1 (172 en 2.51)
[309] Bwork Élémental d'Air 173 -> grade 1 (172 en 2.51)
[309] Bwork Élémental de Feu 173 -> grade 1 (172 en 2.51)
[309] Bwork Élémental d'Eau 173 -> grade 2 (174 en 2.51)
[309] Bwork Élémental de Terre 173 -> grade 2 (174 en 2.51)
[310] Bwork Élémental de Terre 175 -> grade 3 (176 en 2.51)
[310] Bwork Élémental d'Eau 175 -> grade 3 (176 en 2.51)
[310] Bwork Élémental d'Air 175 -> grade 2 (174 en 2.51)
[310] Bwork Élémental de Feu 175 -> grade 2 (174 en 2.51)
[311] Cybwork 182 -> grade 1 (172 en 2.51)
[311] Bwork Élémental de Feu 177 -> grade 3 (176 en 2.51)
[311] Bwork Élémental de Terre 177 -> grade 4 (178 en 2.51)
[311] Bwork Élémental d'Eau 177 -> grade 4 (178 en 2.51)
[311] Bwork Élémental d'Air 177 -> grade 3 (176 en 2.51)
[312] Cybwork 185 -> grade 3 (176 en 2.51)
[312] Bwork Élémental d'Air 179 -> grade 4 (178 en 2.51)
[312] Bwork Élémental de Feu 179 -> grade 4 (178 en 2.51)
[16] Bouftou 13 -> grade 1 (22 en 2.51)
[16] Boufton Blanc 11 -> grade 1 (22 en 2.51)
[16] Boufton Noir 11 -> grade 1 (22 en 2.51)
[17] Bouftou 16 -> grade 3 (26 en 2.51)
[17] Boufton Blanc 12 -> grade 2 (24 en 2.51)
[17] Boufton Noir 12 -> grade 2 (24 en 2.51)
[18] Chef de Guerre Bouftou 29 -> grade 1 (22 en 2.51)
[18] Bouftou 19 -> grade 4 (28 en 2.51)
[19] Chef de Guerre Bouftou 31 -> grade 2 (24 en 2.51)
[20] Chef de Guerre Bouftou 33 -> grade 3 (26 en 2.51)
[20] Bouftou 25 -> grade 5 (30 en 2.51)
[105] Craqueleur 55 -> grade 1 (62 en 2.51)
[105] Craqueboule 50 -> grade 1 (62 en 2.51)
[106] Craqueboule 52 -> grade 2 (64 en 2.51)
[107] Craqueleur 58 -> grade 3 (66 en 2.51)
[108] Craqueleur des Plaines 65 -> grade 1 (62 en 2.51)
[108] Craqueleur 61 -> grade 4 (68 en 2.51)
[108] Craqueboule 54 -> grade 3 (66 en 2.51)
[109] Craqueleur des Plaines 71 -> grade 4 (68 en 2.51)
[109] Craqueleur 67 -> grade 5 (70 en 2.51)
[109] Craqueboule 58 -> grade 5 (70 en 2.51)
[6] Champ Champ 13 -> grade 1 (16 en 2.51)
[6] Moskito 11 -> grade 1 (16 en 2.51)
[7] Champ Champ 14 -> grade 2 (17 en 2.51)
[7] Pissenlit Diabolique 13 -> grade 1 (16 en 2.51)
[7] Moskito 12 -> grade 2 (17 en 2.51)
[8] Tournesol Sauvage 22 -> grade 1 (16 en 2.51)
[8] Champ Champ 15 -> grade 3 (18 en 2.51)
[8] Pissenlit Diabolique 14 -> grade 2 (17 en 2.51)
[8] Moskito 13 -> grade 3 (18 en 2.51)
[9] Tournesol Sauvage 24 -> grade 3 (18 en 2.51)
[9] Pissenlit Diabolique 15 -> grade 3 (18 en 2.51)
[10] Tournesol Sauvage 26 -> grade 5 (20 en 2.51)
[10] Moskito 15 -> grade 5 (20 en 2.51)
[81] Blopignon 48 -> grade 1 (52 en 2.51)
[81] Tronkoblop 44 -> grade 1 (52 en 2.51)
[81] Blop Reinette 40 -> grade 1 (52 en 2.51)
[81] Blop Coco 40 -> grade 1 (52 en 2.51)
[82] Blopignon 51 -> grade 3 (56 en 2.51)
[82] Tronkoblop 48 -> grade 3 (56 en 2.51)
[82] Blop Indigo 42 -> grade 1 (52 en 2.51)
[82] Blop Griotte 42 -> grade 1 (52 en 2.51)
[83] Blop Coco 44 -> grade 3 (56 en 2.51)
[83] Blop Griotte 44 -> grade 2 (54 en 2.51)
[83] Blop Reinette 44 -> grade 3 (56 en 2.51)
[83] Blop Indigo 44 -> grade 2 (54 en 2.51)
[84] Gloutoblop 65 -> grade 4 (58 en 2.51)
[84] Blopignon 57 -> grade 5 (60 en 2.51)
[21] Gargrouille 30 -> grade 1 (32 en 2.51)
[21] Kwoan 26 -> grade 1 (32 en 2.51)
[22] Gargrouille 33 -> grade 3 (36 en 2.51)
[22] Kwoan 28 -> grade 2 (34 en 2.51)
[23] Kwoan 30 -> grade 3 (36 en 2.51)
[24] Chafer d'Élite 49 -> grade 3 (46 en 2.51)
[24] Boostache Prépubère 42 -> grade 4 (38 en 2.51)
[24] Gargrouille 39 -> grade 5 (40 en 2.51)
[25] Chafer d'Élite 52 -> grade 4 (48 en 2.51)
[25] Boostache Prépubère 44 -> grade 5 (40 en 2.51)
[25] Gargrouille 42 -> grade 5 (40 en 2.51)
[25] Vampire 41 -> grade 5 (40 en 2.51)
[115] Canondorf 51 -> grade 1 (82 en 2.51)
[115] Nakunbra 51 -> grade 1 (82 en 2.51)
[115] Boomba 39 -> grade 1 (82 en 2.51)
[116] Canondorf 57 -> grade 4 (88 en 2.51)
[116] Nakunbra 57 -> grade 4 (88 en 2.51)
[117] Nakunbra 63 -> grade 5 (90 en 2.51)
[117] Canondorf 63 -> grade 5 (90 en 2.51)
[66] Kwak de Glace 38 -> grade 1 (42 en 2.51)
[66] Bwak de Glace 36 -> grade 1 (42 en 2.51)
[67] Kwak de Vent 41 -> grade 1 (42 en 2.51)
[67] Bwak de Vent 37 -> grade 1 (42 en 2.51)
[68] Bwak de Terre 38 -> grade 1 (42 en 2.51)
[69] Kwak de Flamme 47 -> grade 1 (42 en 2.51)
[69] Bwak de Flamme 39 -> grade 1 (42 en 2.51)
[125] Kitsou Nae 53 -> grade 1 (71 en 2.51)
[126] Kitsou Nae 55 -> grade 2 (73 en 2.51)
[126] Kitsou Nakwa 23 -> grade 1 (69 en 2.51)
[127] Kitsou Nae 57 -> grade 3 (75 en 2.51)
[127] Kitsou Nakwa 25 -> grade 2 (71 en 2.51)
[128] Kitsou Nufeu 109 -> grade 1 (75 en 2.51)
[128] Kitsou Nae 59 -> grade 4 (77 en 2.51)
[128] Kitsou Nakwa 27 -> grade 3 (73 en 2.51)
[129] Kitsou Nae 61 -> grade 5 (79 en 2.51)
[129] Kitsou Nakwa 29 -> grade 4 (75 en 2.51)
[129] Kitsou Nufeu 111 -> grade 2 (77 en 2.51)
```

### Donjons concernés (anciennement « en attente »)
- **tofu** (Tofu Noir 20-28 vs 32-40…), **ougah** (« Donjon Fungus » = Temple du Grand Ougah ; boss tofus « Huitième » = Neuvième salle 2.51), **chafer** (Chafer Draugr 40/43), **scara** (Scarafeuille Noir 46/48), **ensable**, **toror / tot** (Gamino 48, Scaratos 38…), **krala** (salles 1-2 décalées de 1-2 niveaux ; boss = Kralamoure seul, déjà ainsi en base), **bworker**, **bouftou**, **craq**, **champ**, **blop**, **maison_fantome**, **arche** (salles 4 et boss exactes, 1-3 avec Canon Dorf/Nakunbra/Boomba 39-63 alors que 2.51 = 82-90), **kwak** (boss exact, salles 1-4 décalées), **daigoro**.

## Non appliqués
- **Décision utilisateur (17/10) : « Ne pas toucher »** — ces donjons gardent leurs groupes actuels. Monstre absent du client 2.51 : **sc** (« Ratmane d'Égoutant »), **meu** (« Ouginak »), **kani** (boss : « Kanniboul Archer / Thierry », niveaux 61 ; salles 1-4 pourtant exactes), **nelween** (« Corbac Terrien », « Ouginak »).
- **mc** (Maître Corbac) : labyrinthe (34 spawns), captures = exemples sans salle précise, niveaux modernes.
- **fam** (Sanctuaire des Familiers) : absent du client 2.51.
- **vlad**, **hell-mina** (boss errants du monde), **mini-abra**, **mini-jorbak**, **mini-crypte**, **mini-cania** (mini-donjons du monde ouvert) : pas de spawn de donjon en base.
- **Dimensions à vagues** (non touchées sans accord) : toxoliath (relevé gardé en commentaire), defi-du-chaloeil, fabrique-mallefisk, phossile, horologium-de-xlii, miausolee-du-pounicheur, megalithe-de-fraktale, plateau-de-ush, palais-nidas, capitaine-ekarlatte, reine-des-voleurs, vortex.
- Déjà faits avant : tengu (Givrefoux), korriandre.

## Salles tofus sans spawn en base (non créées : hors périmètre « groupes uniquement »)
Fungus 5 bis / 6 bis / 7 ; Larves 4, 4 bis, 5 bis, 9 bis, 10, 10 bis ; Kitsounes annexes 2-3, 6e et boss ; Comte Hall des Sinistros ; Dragon Cochon 7, 9, 11 (Gorgouille) ; Forgerons 5, 9 ; Craqueleurs 5 ; Canidés 3 bis, 5.

## Raccourcissement à 4 salles de combat + boss — APPLIQUÉ (17/10 13h06, validé par l utilisateur)
Fichiers : `raccourcir_donjons.sql` / `raccourcir_donjons-annuler.sql`. Sauvegarde : `SAVE/1_BASE_DE_DONNEES/2026-10-17_13h06_world_avant-raccourcir-donjons/`. Paquet : `2_BDD/25_raccourcir_donjons.sql`.
Analyse des enchaînements (TeleportMapId) — seuls 3 donjons ont plus de 5 salles de combat atteintes :
- **Hypogée de l'Obsidiantre** : garde 446, 284, 285, 286 → boss 288 ; saute 287 (Cinquième salle).
- Choix utilisateur « Garder les salles officielles » pour Dragon Cochon et Ougah :
- **Antre du Dragon Cochon** : garde 155 (salle d'entrée, groupe de remplissage : changer l'entrée sortirait du périmètre), 157, 159 → boss 444 ; saute 156 et 158. Donne 3 salles de combat + boss.
- **Temple du Grand Ougah** : garde 313, 314, 317, 318 → boss 319 ; saute 315 et 316.
Non concernés / exceptions : Scarafeuilles (la Deuxième salle 445 n'est déjà plus atteinte : 5 salles), Clos des Blops (choix d'un des 4 Blops royaux), Tanière Givrefoux (salle du choix), Bibliothèque du Maître Corbac (labyrinthe), Cavernes du Kolosso (salles des reliques), dimensions à vagues (1 salle), tous les autres (≤ 5 salles).
