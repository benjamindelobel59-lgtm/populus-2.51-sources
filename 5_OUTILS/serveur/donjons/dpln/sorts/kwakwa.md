# Nid du Kwakwa : fiche `kwakwa.json`

Décision utilisateur n° 7 : sorts du site / version actuelle (DofusDB 2995 : 29784, 29786-29789, passifs 29790 Kwapoussée et 29791 Kwayauté).

| Sort | Niveaux | Rôle |
|---|---|---|
| 13000 Kwapoussée (passif) | 45000-45009 | niv. 1-4 = Kwaks / Kwakeres / Bwaks Flamme, Glace, Terre, Vent ; niv. 5 = Kwakwa ; niv. 6 = malus déclenché (-10 % rés, +10 dommages de poussée, 3 tours) ; niv. 7-10 = à la mort, 1017 : le Kwakwa lance Kwayauté de l'élément |
| 13001 Kwayauté (passif du Kwakwa) | 45010-45014 | niv. 1 = pose Kwapoussée niv. 5 ; niv. 2-5 = -40 % rés Feu / Eau / Terre / Air et +40 dommages de poussée, tout le combat, cumulable |
| 13002 Kwakoukas Kwayal | 45015 | neutre 11-15 (crit. 17-23), repousse 4, ligne 1-7, 1 fois par cible |
| 13003 Wakpot Kwayal | 45016 | vole 50 puissance 3 tours (crit. 75, x3) + vol de vie 8-11 d'un élément au hasard |
| 13004 Kwabolition | 45017 | 7-9 Feu, Eau, Terre, Air au corps à corps (crit. 10-13), 1 fois par tour |
| 13005 Kwaristocratie | 45018 | +2 PA, +30 puissance 1 tour aux alliés, relance 4, à partir du tour 2 |
| 13006 Kwarmée Kwayal | 45019-45023 (par grade) | 4 Bwaks du grade du Kwakwa à 2 PO en croix, -20 % vitalité, +2 PA ; interdit sous l'état 924 (posé par chaque Bwak invoqué, retiré à sa mort) |

Kwakwa : retire ses 20 sorts 2.51 (variantes Kwa*), reçoit 13002-13006.
À coder (voir `cerveaux`) : KwakwaBrain lance 13001 niv. 1 au lieu de 3253 + limite d'invocations 4 ; 12 cerveaux BrainPassif pour
les Kwaks / Kwakeres / Bwaks (Kwapoussée niveau de leur élément).

Doutes principaux : désenvoûtement (le serveur ignore `dispellable` → spells_effects_fix), état 924 réutilisé, zone X2,2,
stats hors sorts (80 % rés, -50 rés poussée) non traitées, clé `criticalEffects`.
