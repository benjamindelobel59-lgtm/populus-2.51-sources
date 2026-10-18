# Consignes : audit des mécaniques de donjons (dofuspourlesnoobs → serveur Mystic 2.51)

Travail en LECTURE SEULE : ne modifie aucun fichier du serveur, du client ni la base. Tu écris seulement ta fiche de sortie.
Réponds et écris en français.

## Entrées
- Pages du site converties en texte : `C:\Users\Beny\AppData\Local\Temp\claude\C--Users-Beny-Desktop-DOFUS-NEKFEU-JTE\c7cc6475-2f5d-4f2a-91f1-9a4d62fc6ad9\scratchpad\dpln\txt\<slug>.txt`
  (sections : I Introduction, II Monstres, III Salles, IV Boss, V Stratégie…). Les noms des monstres ne sont PAS dans le texte
  (images avec alt générique) : retrouve à quel monstre appartient chaque bloc de sorts en comparant les noms de sorts avec
  la sortie de l'outil ci-dessous. Lis la page EN ENTIER, y compris la stratégie : elle contient souvent des règles
  (invulnérabilité, états, conditions, glyphes, mécaniques de salle) absentes des listes de sorts.
- Outil serveur (sorts réels en base + effets + si le serveur sait les appliquer) :
  `cd "C:/Users/Beny/Desktop/DOFUS NEKFEU JTE/_OUTILS/serveur/donjons" && py -I analyse_mecaniques.py "Nom1,Nom2,..." [motcle1,motcle2]`
  (noms FRANÇAIS du client ; le 2e argument optionnel liste les sorts du client dont le nom contient un mot : utile pour
  retrouver un passif qui existe dans le client mais n'est attribué à aucun monstre). La ligne « A FAIRE COTE SERVEUR »
  liste les effets / déclencheurs non gérés. Lance l'outil en une fois pour tous les monstres d'un donjon (il est lent à charger).
- Base MariaDB : `"C:/Users/Beny/Desktop/DOFUS NEKFEU JTE/_OUTILS/logiciels/MariaDB/bin/mysql.exe" -uroot world -B -e "..."`
  (noms de `monsters_templates` en espagnol). Monstres d'un donjon sur le serveur :
  `SELECT DISTINCT d.Id,d.MapId,d.Descripcion,g.MonsterId,t.Name FROM monsters_spawns_dungeons d JOIN monsters_spawns_dungeons_groups dg ON dg.DungeonSpawnId=d.Id JOIN monsters_grades g ON g.Id=dg.MonsterGradeId JOIN monsters_templates t ON t.Id=g.MonsterId WHERE d.Descripcion LIKE '%motclé%'`
- Code serveur : `C:/Users/Beny/Desktop/DOFUS NEKFEU JTE/SERVEUR_Emu/server/stump.server.worldserver/`
  - cerveaux de boss : `AI/Fights/Brain/Custom/**` (attribut `[BrainIdentifier((int)MonsterIdEnum.X)]`), passifs posés par `SortPassif.Poser`
  - gestionnaires de sorts : `game/spells/Casts/**` (`[SpellCastHandler(...)]`), effets : `game/effects/handlers/**`
  - `MonsterIdEnum` : `SERVEUR_Emu/server/**/MonsterIdEnum.cs`
- Audit précédent (moins précis, d'après tofus.fr) : `_OUTILS/serveur/donjons/AUDIT_MECANIQUES.md`. Ne le crois pas sur parole.

## RÈGLE VERSION (demande utilisateur, prioritaire)
Le site décrit la version ACTUELLE du jeu ; le serveur est en 2.51. Ne propose PAS d'ajouter un passif / sort / mécanique
venant d'une mise à jour plus récente.
- Un passif ou sort du site n'est à ajouter que s'il EXISTE dans les données du client 2.51 (vérifie avec le 2e argument de
  `analyse_mecaniques.py` = mots-clés du nom du sort) ou si le monstre l'a déjà en base.
- S'il n'existe pas dans le client 2.51 → classe-le **[DOUTE VERSION]** (« probablement ajouté après la 2.51 ») et ne le mets
  pas dans les corrections à coder.
- Même chose pour les chiffres : une différence de valeur avec un sort 2.51 existant = [DOUTE VERSION], pas une correction.
- Korriandre, Kolosso et Glourséleste sont jugés PARFAITS par l'utilisateur : n'y propose aucune modification (signale
  seulement, en [DOUTE VERSION], ce qui diffère).
- En cas de doute, dis-le explicitement.

## Ce que tu dois produire
Fichier `C:/Users/Beny/Desktop/DOFUS NEKFEU JTE/_OUTILS/serveur/donjons/dpln/<lot>.md`, une section par donjon :

```
## <Donjon> (<slug>) — boss <nom> (id <id serveur>)
### Mécanique officielle (site)
- Par monstre (nom français + id serveur) : chaque sort avec les chiffres utiles (élément, dégâts, PO, zone, durée, relances,
  conditions, cible), en reformulant brièvement (pas de copier-coller du site). Passifs et états en GRAS.
- Boss : mécanique complète, règles spéciales, invulnérabilité, phases, conditions de victoire, mécaniques de salle.
### État sur le serveur
- Ce qui est conforme / absent / différent : sort manquant, passif jamais lancé (pas de cerveau ou sort non attribué),
  effet ou déclencheur non géré (sortie de l'outil), chiffres différents (signale-les, mais précise si c'est probablement
  une différence de version 2.51 ↔ version actuelle du jeu), cerveau existant ou pas.
### À faire (précis, prêt à coder)
- Liste numérotée de corrections concrètes : quoi, où (fichier / table / sort / effet), comment, avec les ids. Classe chaque
  point : [CRITIQUE] mécanique centrale du boss absente ou fausse · [IMPORTANT] passif / état de monstre manquant ·
  [MINEUR] chiffres. Si tout est conforme, écris « Rien à faire ».
```

Si un donjon de ton lot n'existe pas sur le serveur, écris juste « Absent du serveur » et passe.
Termine ta fiche par un tableau récapitulatif : donjon | nb CRITIQUE | nb IMPORTANT | nb MINEUR.
Ta réponse finale (au lanceur) : 10 lignes max : le chemin de ta fiche + les points CRITIQUES les plus importants.
