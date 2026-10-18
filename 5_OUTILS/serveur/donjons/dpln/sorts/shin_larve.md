# Shin Larve (Donjon des Larves) : fiche `shin_larve.json`

Décision utilisateur n° 16 : 3 invocations, Larvure 75, Carapace en bouclier, Retour de flamme en réduction de dégâts.

| Sort | Action | 2.51 | Fiche |
|---|---|---|---|
| 507 Convocation gluante (niv. 3304) | modifié | 7 larves (31, 412, 34, 414, 46, 413, 5225), 14 chacune | 3 larves : Saphir 412, Émeraude 414, Rubis 413 (33 chacune) |
| 653 Larvure (niv. 2813-2817) | modifié | soin 11-50 (critique 50 + 1 PA) | soin fixe 38/36/34/33/31 = environ 75 PV réels avec l'intelligence de la Saphir (critique : même soin + 1 PA, 4 tours) |
| 13150 Carapace (niv. 45750-45754) | nouveau (copie de 216) | 216 : réduction 4 à 8 sur coups terre/neutre | bouclier 150 à 190 % du niveau (critique 160 à 200 %), 2 tours |
| 13151 Retour de flamme (niv. 45755) | nouveau (copie de 215) | 215 : renvoi 15 | dommages subis x80 % (effet 1163), 2 tours, déclencheur D |

Monstres : Rubis 413 et Dorée 456 : -215 -216, +13150 (niveaux 1-5) +13151 ; Émeraude 414 : -215, +13151.
215 / 216 ne sont pas modifiés parce que beaucoup d'autres monstres les utilisent (Dragoeufs, Craqueleurs, archimonstres…).

À coder côté serveur (voir `cerveaux`) :
1. IA : classer l'effet 1163 dans `SpellIdentifier` (Buff si < 100), sinon Retour de flamme n'est jamais lancé.
2. Désenvoûtement : `DamageMultiplier` force « seulement à la mort » → ligne `spells_effects_fix` (13151, 1163, Dispellable 1) ou un cas dans le code.

Doute principal : le « 75 » du site inclut déjà l'intelligence de la larve (comme ses 120 de Déglutition, 40 de Frappe…).
Pour lire « 75 » dans l'infobulle, mettre diceNum 75 partout : le soin réel serait alors de 150 à 180 par lancer.
