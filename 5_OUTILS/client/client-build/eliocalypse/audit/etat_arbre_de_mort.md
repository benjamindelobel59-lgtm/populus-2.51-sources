# Arbre de Mort — état de nos données (2026-10-23)

## Dolid (monstre 6028)
look {5546|||135}

- grade 1 : niveau 200, 5800 PV, 12 PA, 4 PM, résistances neutre 9 terre 15 feu -11 eau -8 air 14
- grade 2 : niveau 203, 5900 PV, 12 PA, 4 PM, résistances neutre 9 terre 15 feu -11 eau -8 air 14
- grade 3 : niveau 206, 6000 PV, 12 PA, 4 PM, résistances neutre 9 terre 15 feu -11 eau -8 air 14
- grade 4 : niveau 209, 6200 PV, 12 PA, 4 PM, résistances neutre 9 terre 15 feu -11 eau -8 air 14
- grade 5 : niveau 212, 6300 PV, 12 PA, 4 PM, résistances neutre 9 terre 15 feu -11 eau -8 air 14

### Sorts (grade le plus bas)
- **Tétanysme** (sort 15394, niveau 1)
  4 PA, portée 1-8, ligne False, vue True, lancers/tour 0, lancers/cible 0, relance 2, relance initiale 0, critique 20%
    - 36 à 49 (dommages Eau) (cible A, zone X1)
    - 15299 [sort 15299 _STATE TÉTANYTE COUNTER +] (cible A,e1619,e1620,e1621,e1603,e1604,f6033,e1635, zone X1)
  - **_STATE TÉTANYTE COUNTER +** (sort 15299, niveau 1)
    1 PA, portée 0-63, ligne False, vue False, lancers/tour 0, lancers/cible 0, relance 0, relance initiale 0, critique 0%
      - 15307 [sort 15307 Corruption] (cible C,e1603,e1604,e1605, zone P1)
      - État 1605 [Tétanyte 3] (durée -1, cible C,e1603,e1604,e1605, zone P1)
      - 15333 [sort 15333 Tétanyte] (cible C, zone P1)
    - **Corruption** (sort 15307, niveau 1)
      1 PA, portée 0-63, ligne False, vue False, lancers/tour 0, lancers/cible 0, relance 0, relance initiale 0, critique 0%
        - État 1618 [Corruption 4] (durée -1, cible C,E1617, zone P1)
        - Enlève l'état 1617 [Corruption 3] (cible C,E1617, zone P1)
        - État 1617 [Corruption 3] (durée -1, cible C,E1616, zone P1)
        - Enlève l'état 1616 [Corruption 2] (cible C,E1616, zone P1)
        - État 1616 [Corruption 2] (durée -1, cible C,E1615, zone P1)
        - Enlève l'état 1615 [Corruption 1] (cible C,E1615, zone P1)
        - État 1615 [Corruption 1] (durée -1, cible C,e1615,e1616,e1617, zone P1)
        - 15413 [sort 15413 Phase Germinal] (cible C, zone P1)
        - Enlève les effets du sort (cible C, zone P1)
        - 15716 [sort 15716 Rêve de Corruption] (cible A,*E1671, zone a1)
        - 15464 [sort 15464 Corruption] (cible C, zone P1)
    - **Tétanyte** (sort 15333, niveau 1)
      1 PA, portée 0-63, ligne False, vue False, lancers/tour 0, lancers/cible 0, relance 0, relance initiale 0, critique 0%
        - État 1603 [Tétanyte 1] (durée -1, cible C,E1603, zone P1)
        - État 1604 [Tétanyte 2] (durée -1, cible C,E1604, zone P1)
        - État 1605 [Tétanyte 3] (durée -1, cible C,E1605, zone P1)
        - 15333 [sort 15333 Tétanyte] (durée -1, déclencheur M, cible C, zone P1)
        - 15333 [sort 15333 Tétanyte] (durée -1, déclencheur TP, cible C, zone P1)
        - 15430 [sort 15430 Si la cible est poussée, attirée, téléportée etc..., elle subit des dommages.] (durée -1, déclencheur TP, cible C, zone P1)
        - 15337 [sort 15337 Corruption] (cible C, zone P1)
- **Germintaïde** (sort 15393, niveau 1)
  4 PA, portée 3-10, ligne False, vue True, lancers/tour 2, lancers/cible 1, relance 0, relance initiale 0, critique 20%
    - 29 à 38 (dommages Air) (cible A, zone P1)
    - Soin : 5% des PV max (cible g, zone C2)
    - 100 Puissance (durée 1, cible g, zone C2)

## Nheur'Gueule (monstre 6029)
look {5541|||145}

- grade 1 : niveau 200, 8200 PV, 12 PA, 6 PM, résistances neutre 15 terre 10 feu 17 eau -12 air 20
- grade 2 : niveau 203, 8400 PV, 12 PA, 6 PM, résistances neutre 15 terre 10 feu 17 eau -12 air 20
- grade 3 : niveau 206, 8600 PV, 12 PA, 6 PM, résistances neutre 15 terre 10 feu 17 eau -12 air 20
- grade 4 : niveau 209, 8800 PV, 12 PA, 6 PM, résistances neutre 15 terre 10 feu 17 eau -12 air 20
- grade 5 : niveau 212, 9000 PV, 12 PA, 6 PM, résistances neutre 15 terre 10 feu 17 eau -12 air 20

### Sorts (grade le plus bas)
- **Germinator** (sort 15446, niveau 1)
  3 PA, portée 1-1, ligne False, vue False, lancers/tour 0, lancers/cible 0, relance 3, relance initiale 0, critique 0%
    - Invoque : 6032 (cible a,A, zone P1)
- **Crachacide** (sort 15399, niveau 1)
  3 PA, portée 1-6, ligne True, vue True, lancers/tour 2, lancers/cible 0, relance 0, relance initiale 0, critique 20%
    - 19 à 27 (dommages Air) (durée 1, déclencheur TE, cible A, zone P1)
    - Attire de 2 case(s) (cible A, zone P1)
- **Brutalysme** (sort 15398, niveau 1)
  4 PA, portée 1-2, ligne False, vue True, lancers/tour 1, lancers/cible 0, relance 0, relance initiale 0, critique 20%
    - 35 à 47 (vol Feu) (cible A, zone P1)
    - 15297 [sort 15297 _STATE BRUTALYTE COUNTER +] (cible A,e1619,e1620,e1621,e1600,e1601,f6033,e1635,*f6090, zone P1)
  - **_STATE BRUTALYTE COUNTER +** (sort 15297, niveau 1)
    1 PA, portée 0-63, ligne False, vue False, lancers/tour 0, lancers/cible 0, relance 0, relance initiale 0, critique 0%
      - 15307 [sort 15307 Corruption] (cible C,e1600,e1601,e1602, zone P1)
      - État 1602 [Brutalyte 3] (durée -1, cible C,e1600,e1601,e1602, zone P1)
      - 15284 (cible C,e1600,e1601,e1602, zone P1)
      - 15332 [sort 15332 Brutalyte] (cible C, zone P1)
    - **Corruption** (sort 15307, niveau 1)
      1 PA, portée 0-63, ligne False, vue False, lancers/tour 0, lancers/cible 0, relance 0, relance initiale 0, critique 0%
        - État 1618 [Corruption 4] (durée -1, cible C,E1617, zone P1)
        - Enlève l'état 1617 [Corruption 3] (cible C,E1617, zone P1)
        - État 1617 [Corruption 3] (durée -1, cible C,E1616, zone P1)
        - Enlève l'état 1616 [Corruption 2] (cible C,E1616, zone P1)
        - État 1616 [Corruption 2] (durée -1, cible C,E1615, zone P1)
        - Enlève l'état 1615 [Corruption 1] (cible C,E1615, zone P1)
        - État 1615 [Corruption 1] (durée -1, cible C,e1615,e1616,e1617, zone P1)
        - 15413 [sort 15413 Phase Germinal] (cible C, zone P1)
        - Enlève les effets du sort (cible C, zone P1)
        - 15716 [sort 15716 Rêve de Corruption] (cible A,*E1671, zone a1)
        - 15464 [sort 15464 Corruption] (cible C, zone P1)
    - **Brutalyte** (sort 15332, niveau 1)
      1 PA, portée 0-63, ligne False, vue False, lancers/tour 0, lancers/cible 0, relance 0, relance initiale 0, critique 0%
        - État 1600 [Brutalyte 1] (durée -1, cible C,E1600, zone P1)
        - État 1601 [Brutalyte 2] (durée -1, cible C,E1601, zone P1)
        - État 1602 [Brutalyte 3] (durée -1, cible C,E1602, zone P1)
        - 15332 [sort 15332 Brutalyte] (cible C, zone P1)
- **Tourbe Ylol** (sort 15397, niveau 1)
  4 PA, portée 0-0, ligne False, vue False, lancers/tour 1, lancers/cible 0, relance 0, relance initiale 0, critique 20%
    - 39 à 44 (dommages Terre) (cible A, zone G1)
    - -2{~1~2 à -} PA (durée 1, cible A, zone G1)

## Tentaclaque (monstre 6030)
look {5544|||135}

- grade 1 : niveau 200, 7700 PV, 12 PA, 3 PM, résistances neutre 24 terre -13 feu 14 eau 21 air -10
- grade 2 : niveau 203, 7900 PV, 12 PA, 3 PM, résistances neutre 24 terre -13 feu 14 eau 21 air -10
- grade 3 : niveau 206, 8100 PV, 12 PA, 3 PM, résistances neutre 24 terre -13 feu 14 eau 21 air -10
- grade 4 : niveau 209, 8200 PV, 12 PA, 3 PM, résistances neutre 24 terre -13 feu 14 eau 21 air -10
- grade 5 : niveau 212, 8400 PV, 12 PA, 3 PM, résistances neutre 24 terre -13 feu 14 eau 21 air -10

### Sorts (grade le plus bas)
- **Ponction lombric** (sort 15405, niveau 1)
  3 PA, portée 1-5, ligne False, vue True, lancers/tour 2, lancers/cible 1, relance 0, relance initiale 0, critique 20%
    - 25 à 32 (vol Eau) (cible A, zone P1)
    - -3{~1~2 à -} PM (durée 1, cible A, zone P1)
- **Mucus** (sort 15404, niveau 1)
  4 PA, portée 0-0, ligne False, vue False, lancers/tour 1, lancers/cible 0, relance 0, relance initiale 0, critique 20%
    - 45 à 56 (dommages Terre) (cible A, zone X2)
    - 50% du niveau en bouclier (durée 1, cible a, zone X2)
    - 3 PM (durée 1, cible a, zone X2)
- **Parasitysme** (sort 15403, niveau 1)
  4 PA, portée 1-2, ligne False, vue True, lancers/tour 0, lancers/cible 0, relance 2, relance initiale 0, critique 20%
    - 35 à 42 (dommages Air) (cible A, zone P1)
    - Téléportation symétrique par rapport au lanceur (cible A, zone P1)
    - 15303 [sort 15303 _STATE PARASITYE COUNTER +] (cible A,e1619,e1620,e1621,e1609,e1610,f6033,e1635, zone P1)
  - **_STATE PARASITYE COUNTER +** (sort 15303, niveau 1)
    1 PA, portée 0-63, ligne False, vue False, lancers/tour 0, lancers/cible 0, relance 0, relance initiale 0, critique 0%
      - 15307 [sort 15307 Corruption] (cible C,e1609,e1610,e1611, zone P1)
      - État 1611 [Parasityte 3] (durée -1, cible C,e1609,e1610,e1611, zone P1)
      - 15284 (cible C,e1609,e1610,e1611, zone P1)
      - 15335 [sort 15335 Parasityte] (cible C, zone P1)
    - **Corruption** (sort 15307, niveau 1)
      1 PA, portée 0-63, ligne False, vue False, lancers/tour 0, lancers/cible 0, relance 0, relance initiale 0, critique 0%
        - État 1618 [Corruption 4] (durée -1, cible C,E1617, zone P1)
        - Enlève l'état 1617 [Corruption 3] (cible C,E1617, zone P1)
        - État 1617 [Corruption 3] (durée -1, cible C,E1616, zone P1)
        - Enlève l'état 1616 [Corruption 2] (cible C,E1616, zone P1)
        - État 1616 [Corruption 2] (durée -1, cible C,E1615, zone P1)
        - Enlève l'état 1615 [Corruption 1] (cible C,E1615, zone P1)
        - État 1615 [Corruption 1] (durée -1, cible C,e1615,e1616,e1617, zone P1)
        - 15413 [sort 15413 Phase Germinal] (cible C, zone P1)
        - Enlève les effets du sort (cible C, zone P1)
        - 15716 [sort 15716 Rêve de Corruption] (cible A,*E1671, zone a1)
        - 15464 [sort 15464 Corruption] (cible C, zone P1)
    - **Parasityte** (sort 15335, niveau 1)
      1 PA, portée 0-63, ligne False, vue False, lancers/tour 0, lancers/cible 0, relance 0, relance initiale 0, critique 0%
        - État 1609 [Parasityte 1] (durée -1, cible C,E1609, zone P1)
        - État 1610 [Parasityte 2] (durée -1, cible C,E1610, zone P1)
        - État 1611 [Parasityte 3] (durée -1, cible C,E1611, zone P1)
        - 15335 [sort 15335 Parasityte] (cible C, zone P1)
        - 15335 [sort 15335 Parasityte] (durée -1, déclencheur CI, cible C, zone P1)

## Gangredogue (monstre 6031)
look {5575|||135}

- grade 1 : niveau 200, 6000 PV, 12 PA, 4 PM, résistances neutre -10 terre -14 feu 9 eau 16 air 12
- grade 2 : niveau 203, 6200 PV, 12 PA, 4 PM, résistances neutre -10 terre -14 feu 9 eau 16 air 12
- grade 3 : niveau 206, 6300 PV, 12 PA, 4 PM, résistances neutre -10 terre -14 feu 9 eau 16 air 12
- grade 4 : niveau 209, 6500 PV, 12 PA, 4 PM, résistances neutre -10 terre -14 feu 9 eau 16 air 12
- grade 5 : niveau 212, 6600 PV, 12 PA, 4 PM, résistances neutre -10 terre -14 feu 9 eau 16 air 12

### Sorts (grade le plus bas)
- **Débarbouillie** (sort 15408, niveau 1)
  3 PA, portée 1-5, ligne False, vue True, lancers/tour 2, lancers/cible 1, relance 0, relance initiale 0, critique 20%
    - 35 à 45 (vol Eau) (cible A, zone P1)
    - Vole 2 PO (durée 1, cible A, zone P1)
    - Repousse de 1 case(s) (cible A, zone P1)
- **Gangraine** (sort 15407, niveau 1)
  4 PA, portée 3-10, ligne False, vue True, lancers/tour 0, lancers/cible 0, relance 2, relance initiale 0, critique 20%
    - 36 à 44 (dommages Terre) (cible A, zone P1)
    - 15301 [sort 15301 _STATE GANGRAINYTE COUNTER +] (cible A,e1619,e1620,e1621,e1606,e1607,f6033,e1635,*f6089, zone P1)
  - **_STATE GANGRAINYTE COUNTER +** (sort 15301, niveau 1)
    1 PA, portée 0-63, ligne False, vue False, lancers/tour 0, lancers/cible 0, relance 0, relance initiale 0, critique 0%
      - 15307 [sort 15307 Corruption] (cible C,e1606,e1607,e1608, zone P1)
      - État 1608 [Gangrainyte 3] (durée -1, cible C,e1606,e1607,e1608, zone P1)
      - 15284 (cible C,e1606,e1607,e1608, zone P1)
      - 15334 [sort 15334 Gangrainyte] (cible C, zone P1)
    - **Corruption** (sort 15307, niveau 1)
      1 PA, portée 0-63, ligne False, vue False, lancers/tour 0, lancers/cible 0, relance 0, relance initiale 0, critique 0%
        - État 1618 [Corruption 4] (durée -1, cible C,E1617, zone P1)
        - Enlève l'état 1617 [Corruption 3] (cible C,E1617, zone P1)
        - État 1617 [Corruption 3] (durée -1, cible C,E1616, zone P1)
        - Enlève l'état 1616 [Corruption 2] (cible C,E1616, zone P1)
        - État 1616 [Corruption 2] (durée -1, cible C,E1615, zone P1)
        - Enlève l'état 1615 [Corruption 1] (cible C,E1615, zone P1)
        - État 1615 [Corruption 1] (durée -1, cible C,e1615,e1616,e1617, zone P1)
        - 15413 [sort 15413 Phase Germinal] (cible C, zone P1)
        - Enlève les effets du sort (cible C, zone P1)
        - 15716 [sort 15716 Rêve de Corruption] (cible A,*E1671, zone a1)
        - 15464 [sort 15464 Corruption] (cible C, zone P1)
    - **Gangrainyte** (sort 15334, niveau 1)
      1 PA, portée 0-63, ligne False, vue False, lancers/tour 0, lancers/cible 0, relance 0, relance initiale 0, critique 0%
        - État 1606 [Gangrainyte 1] (durée -1, cible C,E1606, zone P1)
        - État 1607 [Gangrainyte 2] (durée -1, cible C,E1607, zone P1)
        - État 1608 [Gangrainyte 3] (durée -1, cible C,E1608, zone P1)
        - 15334 [sort 15334 Gangrainyte] (cible C, zone P1)
- **Tir Purulent** (sort 15406, niveau 1)
  2 PA, portée 3-12, ligne False, vue True, lancers/tour 3, lancers/cible 1, relance 0, relance initiale 0, critique 20%
    - Soin : 5% des PV max (cible a, zone X1)
    - 27 à 35 (dommages Eau) (cible A, zone X1)

## Pistilangue (monstre 6027)
look {5545|||135}

- grade 1 : niveau 200, 7100 PV, 12 PA, 4 PM, résistances neutre -13 terre 26 feu 10 eau 17 air -7
- grade 2 : niveau 203, 7300 PV, 12 PA, 4 PM, résistances neutre -13 terre 26 feu 10 eau 17 air -7
- grade 3 : niveau 206, 7500 PV, 12 PA, 4 PM, résistances neutre -13 terre 26 feu 10 eau 17 air -7
- grade 4 : niveau 209, 7700 PV, 12 PA, 4 PM, résistances neutre -13 terre 26 feu 10 eau 17 air -7
- grade 5 : niveau 212, 7800 PV, 12 PA, 4 PM, résistances neutre -13 terre 26 feu 10 eau 17 air -7

### Sorts (grade le plus bas)
- **Maturation** (sort 15402, niveau 1)
  3 PA, portée 1-6, ligne False, vue True, lancers/tour 3, lancers/cible 1, relance 0, relance initiale 0, critique 0%
    - 27 à 38 (dommages Feu) (cible A, zone P1)
    - -25{~1~2 à -} Tacle (durée 1, cible A, zone P1)
- **Flemingysme** (sort 15401, niveau 1)
  4 PA, portée 1-5, ligne False, vue True, lancers/tour 0, lancers/cible 0, relance 2, relance initiale 0, critique 20%
    - 20 à 30 (dommages Terre) (durée 1, déclencheur TE, cible A, zone P1)
    - 15305 [sort 15305 _STATE FLEMINGYTE COUNTER +] (cible A,e1619,e1620,e1621,e1612,e1613,f6033,e1635, zone P1)
  - **_STATE FLEMINGYTE COUNTER +** (sort 15305, niveau 1)
    1 PA, portée 0-63, ligne False, vue False, lancers/tour 0, lancers/cible 0, relance 0, relance initiale 0, critique 0%
      - 15307 [sort 15307 Corruption] (cible C,e1612,e1613,e1614, zone P1)
      - État 1614 [Flemingyte 3] (durée -1, cible C,e1612,e1613,e1614, zone P1)
      - 15336 [sort 15336 Flemingyte] (cible C, zone P1)
    - **Corruption** (sort 15307, niveau 1)
      1 PA, portée 0-63, ligne False, vue False, lancers/tour 0, lancers/cible 0, relance 0, relance initiale 0, critique 0%
        - État 1618 [Corruption 4] (durée -1, cible C,E1617, zone P1)
        - Enlève l'état 1617 [Corruption 3] (cible C,E1617, zone P1)
        - État 1617 [Corruption 3] (durée -1, cible C,E1616, zone P1)
        - Enlève l'état 1616 [Corruption 2] (cible C,E1616, zone P1)
        - État 1616 [Corruption 2] (durée -1, cible C,E1615, zone P1)
        - Enlève l'état 1615 [Corruption 1] (cible C,E1615, zone P1)
        - État 1615 [Corruption 1] (durée -1, cible C,e1615,e1616,e1617, zone P1)
        - 15413 [sort 15413 Phase Germinal] (cible C, zone P1)
        - Enlève les effets du sort (cible C, zone P1)
        - 15716 [sort 15716 Rêve de Corruption] (cible A,*E1671, zone a1)
        - 15464 [sort 15464 Corruption] (cible C, zone P1)
    - **Flemingyte** (sort 15336, niveau 1)
      1 PA, portée 0-63, ligne False, vue False, lancers/tour 0, lancers/cible 0, relance 0, relance initiale 0, critique 0%
        - État 1612 [Flemingyte 1] (durée -1, cible C,E1612, zone P1)
        - État 1613 [Flemingyte 2] (durée -1, cible C,E1613, zone P1)
        - État 1614 [Flemingyte 3] (durée -1, cible C,E1614, zone P1)
        - 15336 [sort 15336 Flemingyte] (cible C, zone P1)
- **Spore Addikt** (sort 15400, niveau 1)
  4 PA, portée 0-0, ligne False, vue False, lancers/tour 0, lancers/cible 0, relance 3, relance initiale 0, critique 0%
    - Interception des dommages (durée 2, déclencheur D, cible a, zone G2)
    - 2 PM (durée 2, cible a, zone G2)
    - 15400 [sort 15400 Spore Addikt] (cible a, zone G2)
    - 15472 [sort 15472 Spore Addikt ANIM] (cible a, zone G2)
  - **Spore Addikt** (sort 15400, niveau 2)
    4 PA, portée 0-0, ligne False, vue False, lancers/tour 0, lancers/cible 0, relance 3, relance initiale 0, critique 0%
      - 10% Vitalité (durée 2, cible C, zone P1)
      - Augmente la taille de 7% (durée 2, cible C, zone P1)
  - **Spore Addikt ANIM** (sort 15472, niveau 1)
    4 PA, portée 0-0, ligne False, vue False, lancers/tour 0, lancers/cible 0, relance 3, relance initiale 0, critique 0%
      - Pas d'effet supplémentaire (cible a,A, zone P1)

## Corruption (monstre 6026)
look {5542|||120}

- grade 1 : niveau 220, 21000 PV, 20 PA, 4 PM, résistances neutre 22 terre 16 feu 5 eau 11 air 25
- grade 2 : niveau 220, 23000 PV, 20 PA, 4 PM, résistances neutre 22 terre 16 feu 5 eau 11 air 25
- grade 3 : niveau 220, 25000 PV, 20 PA, 4 PM, résistances neutre 22 terre 16 feu 5 eau 11 air 25
- grade 4 : niveau 220, 27000 PV, 20 PA, 4 PM, résistances neutre 22 terre 16 feu 5 eau 11 air 25
- grade 5 : niveau 220, 29000 PV, 20 PA, 4 PM, résistances neutre 22 terre 16 feu 5 eau 11 air 25
- grade 6 : niveau 220, 21000 PV, 20 PA, 4 PM, résistances neutre 22 terre 16 feu 5 eau 11 air 25
- grade 7 : niveau 220, 22000 PV, 20 PA, 4 PM, résistances neutre 22 terre 16 feu 5 eau 11 air 25
- grade 8 : niveau 220, 23000 PV, 20 PA, 4 PM, résistances neutre 22 terre 16 feu 5 eau 11 air 25
- grade 9 : niveau 220, 24000 PV, 20 PA, 4 PM, résistances neutre 22 terre 16 feu 5 eau 11 air 25
- grade 10 : niveau 220, 25000 PV, 20 PA, 4 PM, résistances neutre 22 terre 16 feu 5 eau 11 air 25

### Sorts (grade le plus bas)
- **Bombe Bactériologique** (sort 15600, niveau 1)
  5 PA, portée 6-20, ligne False, vue True, lancers/tour 1, lancers/cible 0, relance 0, relance initiale 0, critique 0%
    - 300% du niveau en bouclier (durée 1, cible a, zone P1)
    - État 1587 [Péhatyte 1] (durée 1, cible a, zone P1)
    - État 1590 [Péhotyte 1] (durée 1, cible a, zone P1)
    - État 1594 [Péhaimyte 1] (durée 1, cible a, zone P1)
    - État 1597 [Taclyte 1] (durée 1, cible a, zone P1)
    - État 1600 [Brutalyte 1] (durée 1, cible a, zone P1)
    - État 1603 [Tétanyte 1] (durée 1, cible a, zone P1)
    - État 1606 [Gangrainyte 1] (durée 1, cible a, zone P1)
    - État 1609 [Parasityte 1] (durée 1, cible a, zone P1)
    - État 1612 [Flemingyte 1] (durée 1, cible a, zone P1)
    - 15390 [sort 15390 Bombe Bactériologique] (durée 1, déclencheur TE, cible a, zone P1)
    - 15418 [sort 15418 Explose à la fin du tour de la cible et applique la corruption que la cible porte dans une zone de 1 à 3 PO autour d'elle.] (durée 1, déclencheur TE, cible a, zone P1)
  - **Bombe Bactériologique** (sort 15390, niveau 1)
    1 PA, portée 0-63, ligne False, vue False, lancers/tour 0, lancers/cible 0, relance 0, relance initiale 0, critique 0%
      - 15289 [sort 15289 _STATE PÉHATYTE COUNTER +] (cible A,*E1587,e1619,e1620,e1621,e1587,e1588,f6033,e1635, zone C3)
      - 15291 [sort 15291 _STATE PÉHOTYTE COUNTER +] (cible A,*E1590,e1619,e1620,e1621,e1590,e1591,f6033,e1635, zone C3)
      - 15293 [sort 15293 _STATE PÉHAIMYTE COUNTER +] (cible A,*E1594,e1619,e1620,e1621,e1594,e1595,f6033,e1635, zone C3)
      - 15295 [sort 15295 _STATE TACLYTE COUNTER +] (cible A,*E1597,e1619,e1620,e1621,e1597,e1598,f6033,e1635, zone C3)
      - 15297 [sort 15297 _STATE BRUTALYTE COUNTER +] (cible A,*E1600,e1619,e1620,e1621,e1600,e1601,f6033,e1635, zone C3)
      - 15299 [sort 15299 _STATE TÉTANYTE COUNTER +] (cible A,*E1603,e1619,e1620,e1621,e1603,e1604,f6033,e1635, zone C3)
      - 15301 [sort 15301 _STATE GANGRAINYTE COUNTER +] (cible A,*E1606,e1619,e1620,e1621,e1606,e1607,f6033,e1635, zone C3)
      - 15303 [sort 15303 _STATE PARASITYE COUNTER +] (cible A,*E1609,e1619,e1620,e1621,e1609,e1610,f6033,e1635, zone C3)
      - 15305 [sort 15305 _STATE FLEMINGYTE COUNTER +] (cible A,*E1612,e1619,e1620,e1621,e1612,e1613,f6033,e1635, zone C3)
      - 47 à 62 (dommages Terre) (cible A, zone C3)
    - **_STATE PÉHATYTE COUNTER +** (sort 15289, niveau 1)
      1 PA, portée 0-63, ligne False, vue False, lancers/tour 0, lancers/cible 0, relance 0, relance initiale 0, critique 0%
        - 15307 [sort 15307 Corruption] (cible C,e1587,e1588,e1589, zone P1)
        - État 1589 [Péhatyte 3] (durée -1, cible C,e1587,e1588,e1589, zone P1)
        - 15284 (cible C,e1587,e1588,e1589, zone P1)
        - 15324 [sort 15324 Péhatyte] (cible C, zone P1)
    - **_STATE PÉHOTYTE COUNTER +** (sort 15291, niveau 1)
      1 PA, portée 0-63, ligne False, vue False, lancers/tour 0, lancers/cible 0, relance 0, relance initiale 0, critique 0%
        - 15307 [sort 15307 Corruption] (cible C,e1590,e1591,e1592, zone P1)
        - État 1592 [Péhotyte 3] (durée -1, cible C,e1590,e1591,e1592, zone P1)
        - 15284 (cible C,e1590,e1591,e1592, zone P1)
        - 15329 [sort 15329 Péhotyte] (cible C, zone P1)
    - **_STATE PÉHAIMYTE COUNTER +** (sort 15293, niveau 1)
      1 PA, portée 0-63, ligne False, vue False, lancers/tour 0, lancers/cible 0, relance 0, relance initiale 0, critique 0%
        - 15307 [sort 15307 Corruption] (cible C,e1594,e1595,e1596, zone P1)
        - État 1596 [Péhaimyte 3] (durée -1, cible C,e1594,e1595,e1596, zone P1)
        - 15284 (cible C,e1594,e1595,e1596, zone P1)
        - 15330 [sort 15330 Péhaimyte] (cible C, zone P1)
    - **_STATE TACLYTE COUNTER +** (sort 15295, niveau 1)
      1 PA, portée 0-63, ligne False, vue False, lancers/tour 0, lancers/cible 0, relance 0, relance initiale 0, critique 0%
        - 15307 [sort 15307 Corruption] (cible C,e1597,e1598,e1599, zone P1)
        - État 1599 [Taclyte 3] (durée -1, cible C,e1597,e1598,e1599, zone P1)
        - 15284 (cible C,e1597,e1598,e1599, zone P1)
        - 15331 [sort 15331 Taclyte] (cible C, zone P1)
    - **_STATE BRUTALYTE COUNTER +** (sort 15297, niveau 1)
      1 PA, portée 0-63, ligne False, vue False, lancers/tour 0, lancers/cible 0, relance 0, relance initiale 0, critique 0%
        - 15307 [sort 15307 Corruption] (cible C,e1600,e1601,e1602, zone P1)
        - État 1602 [Brutalyte 3] (durée -1, cible C,e1600,e1601,e1602, zone P1)
        - 15284 (cible C,e1600,e1601,e1602, zone P1)
        - 15332 [sort 15332 Brutalyte] (cible C, zone P1)
    - **_STATE TÉTANYTE COUNTER +** (sort 15299, niveau 1)
      1 PA, portée 0-63, ligne False, vue False, lancers/tour 0, lancers/cible 0, relance 0, relance initiale 0, critique 0%
        - 15307 [sort 15307 Corruption] (cible C,e1603,e1604,e1605, zone P1)
        - État 1605 [Tétanyte 3] (durée -1, cible C,e1603,e1604,e1605, zone P1)
        - 15333 [sort 15333 Tétanyte] (cible C, zone P1)
    - **_STATE GANGRAINYTE COUNTER +** (sort 15301, niveau 1)
      1 PA, portée 0-63, ligne False, vue False, lancers/tour 0, lancers/cible 0, relance 0, relance initiale 0, critique 0%
        - 15307 [sort 15307 Corruption] (cible C,e1606,e1607,e1608, zone P1)
        - État 1608 [Gangrainyte 3] (durée -1, cible C,e1606,e1607,e1608, zone P1)
        - 15284 (cible C,e1606,e1607,e1608, zone P1)
        - 15334 [sort 15334 Gangrainyte] (cible C, zone P1)
    - **_STATE PARASITYE COUNTER +** (sort 15303, niveau 1)
      1 PA, portée 0-63, ligne False, vue False, lancers/tour 0, lancers/cible 0, relance 0, relance initiale 0, critique 0%
        - 15307 [sort 15307 Corruption] (cible C,e1609,e1610,e1611, zone P1)
        - État 1611 [Parasityte 3] (durée -1, cible C,e1609,e1610,e1611, zone P1)
        - 15284 (cible C,e1609,e1610,e1611, zone P1)
        - 15335 [sort 15335 Parasityte] (cible C, zone P1)
    - **_STATE FLEMINGYTE COUNTER +** (sort 15305, niveau 1)
      1 PA, portée 0-63, ligne False, vue False, lancers/tour 0, lancers/cible 0, relance 0, relance initiale 0, critique 0%
        - 15307 [sort 15307 Corruption] (cible C,e1612,e1613,e1614, zone P1)
        - État 1614 [Flemingyte 3] (durée -1, cible C,e1612,e1613,e1614, zone P1)
        - 15336 [sort 15336 Flemingyte] (cible C, zone P1)
  - **Explose à la fin du tour de la cible et applique la corruption que la cible porte dans une zone de 1 à 3 PO autour d'elle.** (sort 15418, niveau 1)
    1 PA, portée 0-63, ligne False, vue False, lancers/tour 0, lancers/cible 0, relance 0, relance initiale 0, critique 0%
      - Pas d'effet supplémentaire (cible a,A, zone P1)
- **Éclosion germinal** (sort 15392, niveau 1)
  2 PA, portée 2-5, ligne False, vue True, lancers/tour 1, lancers/cible 0, relance 0, relance initiale 0, critique 20%
    - 24 à 30 (dommages Neutre) (cible A, zone C2)
    - -200{~1~2 à -} Puissance (durée 1, cible A, zone C2)
    - 150% du niveau en bouclier (durée 1, cible a, zone C2)
- **Bombe Bactériologique** (sort 15389, niveau 1)
  5 PA, portée 0-6, ligne False, vue False, lancers/tour 2, lancers/cible 1, relance 0, relance initiale 0, critique 0%
    - 300% du niveau en bouclier (durée 1, cible a, zone P1)
    - État 1587 [Péhatyte 1] (durée 1, cible a, zone P1)
    - État 1590 [Péhotyte 1] (durée 1, cible a, zone P1)
    - État 1594 [Péhaimyte 1] (durée 1, cible a, zone P1)
    - État 1597 [Taclyte 1] (durée 1, cible a, zone P1)
    - État 1600 [Brutalyte 1] (durée 1, cible a, zone P1)
    - État 1603 [Tétanyte 1] (durée 1, cible a, zone P1)
    - État 1606 [Gangrainyte 1] (durée 1, cible a, zone P1)
    - État 1609 [Parasityte 1] (durée 1, cible a, zone P1)
    - État 1612 [Flemingyte 1] (durée 1, cible a, zone P1)
    - 15390 [sort 15390 Bombe Bactériologique] (durée 1, déclencheur TE, cible a, zone P1)
    - 15418 [sort 15418 Explose à la fin du tour de la cible et applique la corruption que la cible porte dans une zone de 1 à 3 PO autour d'elle.] (durée 1, déclencheur TE, cible a, zone P1)
- **Putréfaction** (sort 15388, niveau 1)
  4 PA, portée 3-10, ligne False, vue True, lancers/tour 0, lancers/cible 0, relance 3, relance initiale 0, critique 20%
    - 56 à 68 (dommages Feu) (cible A, zone P1)
- **Liquéfaction** (sort 15387, niveau 1)
  1 PA, portée 2-14, ligne False, vue False, lancers/tour 4, lancers/cible 1, relance 0, relance initiale 0, critique 20%
    - 17 à 24 (dommages Eau) (cible A, zone P1)
    - effet 1101 (cible a,A, zone P1)
    - 15364 [sort 15364 _RANDOM DISEASE] (cible A,e1619,e1620,e1621,f6033,e1635, zone P1)
    - Pas d'effet supplémentaire (cible a,A, zone P1)
  - **_RANDOM DISEASE** (sort 15364, niveau 1)
    1 PA, portée 0-63, ligne False, vue False, lancers/tour 0, lancers/cible 0, relance 0, relance initiale 0, critique 0%
      - 15289 [sort 15289 _STATE PÉHATYTE COUNTER +] (cible C,e1587,e1588, zone P1)
      - 15291 [sort 15291 _STATE PÉHOTYTE COUNTER +] (cible C,e1590,e1591, zone P1)
      - 15293 [sort 15293 _STATE PÉHAIMYTE COUNTER +] (cible C,e1594,e1595, zone P1)
      - 15295 [sort 15295 _STATE TACLYTE COUNTER +] (cible C,e1597,e1598, zone P1)
      - 15297 [sort 15297 _STATE BRUTALYTE COUNTER +] (cible C,e1600,e1601, zone P1)
      - 15299 [sort 15299 _STATE TÉTANYTE COUNTER +] (cible C,e1603,e1604, zone P1)
      - 15301 [sort 15301 _STATE GANGRAINYTE COUNTER +] (cible C,e1606,e1607, zone P1)
      - 15303 [sort 15303 _STATE PARASITYE COUNTER +] (cible C,e1609,e1610, zone P1)
      - 15305 [sort 15305 _STATE FLEMINGYTE COUNTER +] (cible C,e1612,e1613, zone P1)
- **Convalescence Prolifique** (sort 15386, niveau 1)
  2 PA, portée 1-10, ligne False, vue True, lancers/tour 3, lancers/cible 2, relance 0, relance initiale 0, critique 20%
    - 28 à 35 (dommages Air) (cible A, zone P1)
    - Repousse de 2 case(s) (cible A, zone P1)
- **Incu-Batteur** (sort 15385, niveau 1)
  6 PA, portée 2-14, ligne False, vue True, lancers/tour 0, lancers/cible 0, relance 2, relance initiale 0, critique 20%
    - 48 à 56 (dommages Eau) (cible A, zone P1)
    - 15463 [sort 15463 Incu-Batteur] (durée 1, déclencheur D, cible A, zone P1)
    - 15417 [sort 15417 À chaque réception de dommages, la cible retourne à sa position précédente.] (durée 1, déclencheur D, cible A, zone P1)
  - **Incu-Batteur** (sort 15463, niveau 1)
    6 PA, portée 2-10, ligne False, vue True, lancers/tour 0, lancers/cible 0, relance 2, relance initiale 0, critique 0%
      - Téléporte à la position précédente (cible a,A, zone P1)
  - **À chaque réception de dommages, la cible retourne à sa position précédente.** (sort 15417, niveau 1)
    1 PA, portée 0-63, ligne False, vue False, lancers/tour 0, lancers/cible 0, relance 0, relance initiale 0, critique 0%
      - Pas d'effet supplémentaire (cible a,A, zone P1)
- **Bêche corrompue** (sort 15384, niveau 1)
  4 PA, portée 1-2, ligne True, vue True, lancers/tour 3, lancers/cible 1, relance 0, relance initiale 0, critique 20%
    - -1{~1~2 à -} Durée des effets (cible A, zone L2)
    - 34 à 46 (dommages Feu) (cible A, zone L2)
    - -3{~1~2 à -} PA (durée 2, cible A,E1629, zone L2)
    - État 1629 [Bêche Corrompue] (durée 1, cible A, zone L2)
