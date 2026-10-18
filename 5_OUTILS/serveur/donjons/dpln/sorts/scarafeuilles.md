# Donjon des Scarafeuilles : fiche `scarafeuilles.json`

Décision utilisateur n° 8 : sorts du site (DofusDB 29825-29833).

| Sort | Niveaux | Monstre | Remplace |
|---|---|---|---|
| 13020 Scaracroissance (passif) | 45030-45044 | Immature 798 | — (niv. 1-5 invoqué, 6-10 présent au début : état 78 + transformation retardée ; 11-15 transformation par grade) |
| 13021 Scarapoison | 45045-45049 (élément = grade) | Immature | Empoisonnement 219 |
| 13022 Scarasoin | 45050 | Immature | Guigne 487 |
| 13023 Scarenvol | 45051 | Immature | Envolupté 802 |
| 13024 Scarinvi | 45052 | Noir 795 (+ Immature grade 5) | Disparition groupée 786 |
| 13025 Scarimmo | 45053 | Noir 795 (+ Immature grade 5) | Immobilisation 244 |

Transformation de l'Immature (grade 1 Vert, 2 Rouge, 3 Bleu, 4 Blanc, 5 Noir) : +1 PA, +1 PM, +20 % vitalité, résistances du
Scarafeuille (exactes), état 78 retiré → il perd Scarapoison / Scarasoin / Scarenvol et utilise Scaraforce 343, Elemental
Dispersion 345, Scarinvoc 311, Spirit Element 349 (état 78 interdit ajouté par 4 « modifier », effets 2.51 inchangés) ou
Scarinvi / Scarimmo pour le grade 5.

À coder (voir `cerveaux`) : cerveau de l'Immature (niveau du passif selon invoqué / présent), 5 cas dans ChangeSkin.cs,
`niveauParGrade` 0 = pas de sort pour ce grade.
Bug vu hors décision : niveaux de 311/345/349 des Scarafeuilles colorés = grade au lieu de l'élément (voir `doutes`).
