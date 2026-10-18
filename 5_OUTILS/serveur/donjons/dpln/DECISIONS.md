# Décisions de l'utilisateur sur les doutes de version (18/10/2026)

Référence pour coder les corrections de `SYNTHESE.md` / `lot1..8.md`. En cas de conflit avec une fiche, CE FICHIER gagne.

## Règles générales
- Chiffres (dégâts, soins, relances, PV) : **garder la 2.51** partout, sauf exception ci-dessous.
- Korriandre, Kolosso, Glourséleste : ne pas toucher.
- Expéditions de guilde : ignorées.
- Masques `o` / `O` : correction **limitée aux sorts des donjons cités** (Malléfisk, Moon, Nidas, Ombre, Terrier du Wa Wabbit,
  Pendultimatum du Comte), pas de changement global.
- Limite d'invocations : relevée **seulement** pour Papa Nowel (4) et Meulou (2). Koulosse et Wo Wabbit restent à 1.

## Gardés en 2.51 (rien à ajouter)
Gelaxième Dimension (pas de Tartinade) · sorts Rêve/Paradoxe/Cauchemar (Tynril, Tanukouï, Nelween) non utilisés ·
Comte Harebourg : centre du Mi-temps libre · Nécronyx termine le tour · Vortex : monstres réinvoqués (sans préférence → 2.51) ·
Dramak · Nileza · Brumen/Nelween · Sapik · Kanniboul (sans préférence → 2.51, seule la Poupée est corrigée) · Phossile (seul le
tour d'Abîme est corrigé) · Ombre · Royalmouth / Aléamouth (sans préférence → 2.51) · Meno / Étoile Swagante · Craqueleur des
Plaines (pas de Plainitude) · Roi Nidas · Crocabulia.

## À faire comme le site (à créer côté serveur)
1. **Bibliothèque du Maître Corbac** : donjon classique de **4 salles** avec les vrais monstres du donjon ; la salle juste avant
   le boss contient le **Capsaaloocke** ; puis la salle du Maître Corbac.
2. **IA fuyarde** (comme le Fongeur) : Bouftou Royal, Batofu, Gériatique.
3. **Gourlo** : Rhum Fourbe des tonneaux à partir du **3e tour** (comme la 2.51 ; annule mon réglage « dès le 1er tour »).
4. **Compositions** : Rat Noir + Rat Blanc avec Sphincter Cell ; Tofu Mutant dans le donjon des Tofus.
5. **Papa Nowel** : lui donner Hotte d'or (2556).
6. **Nécronyx (Solar, Bethel)** : objet qui apprend le sort, vendu en **Doplons** par un nouveau **Marchand de Doplons**
   (avec les autres objets officiels vendus en Doplons).
7. **Kwakwa** : sorts du site (Kwapoussée, Kwayauté -40 % par mort, Kwarmée / Kwaristocratie).
8. **Scarafeuilles** : sorts du site (Scaracroissance, Scarapoison, Scarasoin, Scarenvol, Scarinvi, Scarimmo).
9. **Haute Truche** : Truchage seulement sur coup d'arme, pour la Truche seulement (Glourséleste inchangé).
10. **Koutoulou** : Miasme Polarisateur pose un glyphe -25 % de dégâts ; frapper un allié retire de la Folie.
11. **Père Ver** : Masticroc et Trémorse Inébranlables + Lourds.
12. **Chouque** : glyphe de Cocktail Rhumotov ; Téléportation Spectrale impossible sur un joueur en Pesanteur.
13. **Skeunk** : Pesanteur dès le début du combat, Skeunk se reconstitue / ressuscite, faiblesse élémentaire aléatoire de la
    Poupée Mortelle.
14. **Daïgoro** : -25 dégâts reçus, Reuche avec vol de vie, 15 % de faiblesse au corps à corps.
15. **Moon** : +100 puissance au Moon quand on frappe le Darkli.
16. **Shin Larve** : 3 invocations, Larvure 75, Carapace en bouclier, Retour de flamme en réduction de dégâts.
17. **Pandikazes** : résistances aléatoires (Guerrier Pandikaze, Pandikwakaze) tirées à chaque combat.
18. **Chafer Ronin** : Bunshin à partir du tour 2.

Les mécaniques « à inventer » (7, 8, 10, 12-16) : relire la page du site du donjon au moment de coder pour les chiffres exacts.
**Source de vérité pour les sorts à inventer (consigne utilisateur)** : DofusDB, https://dofusdb.fr/fr/database/monsters/
(chercher le monstre). Données lisibles directement par l'API : `https://api.dofusdb.fr/monsters?name.fr=<nom>` (grades,
caractéristiques, ids de sorts), puis `https://api.dofusdb.fr/spells/<id>` et `https://api.dofusdb.fr/spell-levels?spellId=<id>`
(effets exacts). Attention : un effet récent inconnu du client 2.51 ne s'affichera pas : le remplacer par l'effet 2.51 équivalent.
