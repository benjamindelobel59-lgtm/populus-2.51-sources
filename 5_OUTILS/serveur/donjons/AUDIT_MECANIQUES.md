# Audit des mécaniques de donjons (17/10/2026)

Audit en lecture seule de ~90 donjons de tofus.fr (5 lots), comparés aux données 2.51 du client et au code serveur.
Déjà faits avant l'audit : Glourséleste, Kolosso, Korriandre, Givrefoux, Vortex, Obsidiantre, Ben le Ripate, Mansot Royal, Royalmouth, Grolloum.

## Constat général
- La plupart des boss n'ont **pas de cerveau** : leur passif officiel (dans le client) n'est **jamais lancé**. Table `ai_init_brains` vide.
- Un passif lancé sur `FightStarted` ne marche pas pour un monstre invoqué ou arrivé en vague → `SortPassif.Poser`.
- Noms de `monsters_templates` en espagnol : utiliser les noms français du client.

## Bugs qui touchent plusieurs donjons
| Bug | Où | Donjons touchés |
|---|---|---|
| Soin avec durée lancé tout de suite au lieu d'à chaque déclenchement | `Heal.cs` (AddTriggerBuff Instant) | Ougah (Spore Hanchambre), Rat Noir (Kackisoigne), Rats du Château |
| Zone `;NNN` (case fixe) non gérée, zone `Z` non gérée | `Zone.cs`, `EffectBase.ParseRawZone` | Chalœil, Dantinéa, Nidas, Koutoulou, Meno, Gorgouille (DC) |
| Effets 1067/1068/1069 (% PV Air/Eau/Feu) | `DamagePercentTargetLife.cs` (seuls 1070/1071) | Daïgoro, Dramak, Truche |
| Effet 220 (renvoi de dommages) en sort | seulement objets | Dragoeufs (Crocabulia), Ombre (Panterreur) |
| Effets 240-244 (réductions fixes) en sort | seulement objets | Kitsounes (Tanuki chan) |
| Effet 1034 (invoque le dernier allié mort) | non géré | Sylargh, (Grolloum : fait à la main) |
| Effet 1024 (illusions) | non géré (1097 existe) | Chafer Ronin (Bunshin) |
| Effets 418/420 (dommages / résistances critiques) | non gérés | Skeunk, (Vortex : fait à la main) |
| Autres effets non gérés | 1125, 1019, 1021, 1022, 1124, 333, 407, 666, 1063, 770-775, 1189, 2018, 2019 ; déclencheurs `MA`, `P\|MA` | Halouine, Klime, Nileza, Merkator, Baleine, Reine, Nidas, Phossile, Reine Nyée, Comte, Koutoulou, Dantinéa, Meno, Ombre |

## Rien à coder (combat normal ou déjà conforme)
Bworks, Tournesol, Larves, Pandikazes, Scarafeuilles, Tofus, Blops, Multicolore, Bworker, Bouftou, Bulbes, Domaine Ancestral, Kankreblath,
Chêne Mou, Chouque, Koulosse, Minotoror, Minotot, Tofulailler, Château Ensablé, Forgerons, Kardorim, Kanniboul, Maison Fantôme, Grotte Hesque,
Rasboul, Hell Mina, Nelween, Nowel (Sapik, Père Fwetar), Wa Wobot / Terrier.
Déjà gérés : Craqueleurs, Firefoux, Rat Blanc, Corbac (version simple), Arche (Tonneaux), Château Wa Wabbit, Kwakwa, Kimbo (à vérifier), Pounicheur, Klime (détail 1125).
Absents du 2.51 : Familiers, Dark Vlad, Abraknydes (ancien), 4 mini-donjons (Crypte, Abra, Jorbak, Cania).

## À faire : passif officiel jamais lancé (correctif simple : cerveau qui le lance)
Kralamoure 1103 · Toxoliath 4796 + Lucrane 4746 · Malléfisk 4505 · Kanigroula 4093 · Missiz Frizz 3517 · Daïgoro 3037 (+1067-1069) ·
Ekarlatte 4804 + Tromblion 4730 · Haute Truche 4367 + 5 passifs de monstres (+1069) · Meulou 305 « Fureur » · Gorgouille (DC) 2932 (+zone Z) ·
Phossile 4497 (+407, P|MA) · XLII 5106 + Auroraire 3850 (5104) · Fraktale : AuroraireBrain lance 5104 au lieu de 5111 · Sylargh 3606 (+1034) ·
Reine des Voleurs 4693/4868 (Bonbombe sans sort) · Reine Nyée 5515 (oeuf) / 5520 · Nocturlabe 3639.

## À faire : gros chantiers (plusieurs passifs, effets non gérés, choix page / 2.51)
Comte Harebourg (Carillon 3642, confusion 770-775, cible W) · Défi du Chalœil (6231, zones `;`) · Aquadôme de Merkator (4009, 1021/1022/1124) ·
Théâtre de Dramak (3875/3873/3861) · Ventre de la Baleine (4972 + passifs Restauration) · Palais de Dantinéa (6709…) · Palais de Nidas (Boubourse) ·
Temple de Koutoulou (6749) · Vaisseau du Capitaine Meno (6793) · Plateau de Ush (Ush noir absent) · Pyramide d'Ombre (4104/4117) · Moon (totems) ·
Tynril (élément du sort = grade).

## Questions ouvertes (page ≠ 2.51)
Voir la conversation : par défaut on suit les données 2.51, sauf décision contraire de l'utilisateur donjon par donjon.
