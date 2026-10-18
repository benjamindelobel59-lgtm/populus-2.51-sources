# Synthèse de l'audit des donjons (dofuspourlesnoobs → Mystic 2.51), 18/10/2026

105 donjons du site présents sur le serveur, 8 fiches détaillées : `lot1.md` … `lot8.md` (mécanique officielle, état serveur,
corrections prêtes à coder). Règle appliquée : rien de plus récent que la 2.51 n'est à coder ; le reste est [DOUTE VERSION].
Korriandre, Kolosso, Glourséleste : aucune modification.

| Lot | CRITIQUE | IMPORTANT | MINEUR |
|---|---|---|---|
| 1 (Merkator … Kankreblath) | 2 | 8 | 2 |
| 2 (Meulou … Kardorim) | 4 | 3 | 1 |
| 3 (Domaine Ancestral … Rasboul) | 1 | 4 | 1 |
| 4 (Grotte Hesque … Fraktale) | 5 | 6 | 1 |
| 5 (Sakaï … Sphincter Cell) | 8 | 3 | 1 |
| 6 (Daïgoro … Tofulailler) | 3 | 5 | 2 |
| 7 (Tofus … Solar) | 10 | 9 | 1 |
| 8 (Bethel … Katrepat) | 17 | 12 | 6 |
| **Total** | **50** | **50** | **15** |

## 1. Bugs du moteur qui touchent plusieurs donjons (à faire en premier, une seule fois)
| # | Bug | Donjons touchés | Fiche |
|---|---|---|---|
| M1 | Masques de cible `o` / `O` (dernier attaquant / déclencheur) mal lus ou jamais atteints dans les sous-sorts | Malléfisk, Moon (totems), Nidas (Boubourse), Ombre (Encrage), Terrier Wa Wabbit (Bumper, Louwdeuw, Ventwe Webondi), Harebourg (Pendultimatum) | lots 3, 5, 6 |
| M2 | `GlyphAura.Enter` ignore les masques de cible (glyphe qui tue tout le monde) | Moon, Ombre (Globilum) | lot 5 |
| M3 | Limite moteur : 1 invocation par monstre | Papa Nowel (Dichotomie bloquée), Meulou, (Koulosse, Wo Wabbit = doute) | lot 2 |
| M4 | Cibles mortes exclues des effets « à la mort » | Meulou (Mulanthropie) | lot 2 |
| M5 | Poussées forcées 1021 / 1022 bloquées sur les indéplaçables / `CanBePushed=0` | Merkator (Mer Cure), Moon (totems) | lots 1, 5 |
| M6 | Durée -1 sur déclencheur → -1000 (état permanent) | Klime (vulnérable pour toujours) | lot 6 |
| M7 | Déclencheur `MS` (échange de place) jamais émis | Wobot (Louwdeuw) | lot 6 |
| M8 | `LifeCriterion` ignore le préfixe `*` | XLII (Rage Mahal) | lot 4 |
| M9 | `CanUseWeapon` ne teste que l'état 42 | Daïgoro (Cinquième Marque) | lot 6 |
| M10 | Effet 2160 : `Value` (« la plus proche ») ignorée ; 1101 limité au sort 9900 | Chouque | lot 2 |
| M11 | Effets non gérés : 147 (résurrection), 1034 incomplet (Sylargh), 246, 1047, 1126/1127, 421, 2024, déclencheur DT | Solar, Bethel, Sylargh, Squelettes, Milépines… | lots 6, 7, 8 |
| M12 | Sort 5612 inexistant appelé (risque de plantage) | Arapex | lot 1 |

## 2. Passifs 2.51 jamais lancés (cerveau `BrainPassif` d'une ligne par monstre)
Chouque 6103 + Nakunbra 6115 · Wobot Tamponneur 3788, Black Wo Wabbit 3794, Wobot / Kiafin 3798 · Firefoux (Zilla 4574,
Maho 4585, Soryo 4577) · Poupée Kanniboul 2411 · Père Fwetar 2557 · XLII 5050 · Fraktale 5052 · Maison Fantôme 7075 ·
Toile 5507, Dardalaine 5523, Bworker 8675 · Darkli Moon 302 · Malamibe 4933 · Solar 9415 / 9354 · Dazak 10346 · Razof 8512 +
Minikrone 8530 · Croquanterie 12290 + 12297 · Kao 12311 + 11993 · Tal Kasha 7868 · Père Ver 7367 · El Piko 7199 ·
Choudini 8630 · Mantiscore 7223 · Anerice 8727 · Bethel 9542 / 9450 / 9559 / 9566 · Lien Fusionnel 2982 (Grozilla/Grasmera).
Erreur de cerveau : Meno pose sur lui-même le passif 6758 de ses monstres (invulnérable puis mort au tour 4).

## 3. Données fausses en base (SQL)
Niveaux de sort : Maître Corbac 630, Blops / Biblops, Darkli Moon (Totems), tortues de Sphincter Cell, Craqueboule.
Mauvais monstres : Bibliothèque du Maître Corbac (robots de Miss Teigne), Grozilla/Grasmera (Peluches…), Hypogée salle 1
(groupe du Royalmouth), Kitsounes (pas de salle du Tanukouï San), Gorgouille placée nulle part. Grotte Hesque : élément des
Crustorails / Palmifleurs selon la salle.

## 4. Bloquant
Solar et Bethel : sans le sort joueur Nécronyx 9348 (quête absente), poser leur passif rend le combat ingagnable.

## Ordre proposé
1) M1 à M12 (moteur) → 2) SQL (§3) → 3) cerveaux de passifs (§2), par lots de donjons testables → 4) Solar / Bethel en dernier.
