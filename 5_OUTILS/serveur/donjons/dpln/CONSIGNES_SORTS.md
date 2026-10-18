# Consignes : concevoir les sorts « comme le site » (décision utilisateur) pour le serveur Mystic 2.51

Tu écris UNIQUEMENT une fiche JSON (et une note .md). Tu ne modifies AUCUN fichier du serveur, du client ni la base. Français.

## Contexte
L'utilisateur a décidé que certains donjons doivent avoir la mécanique ACTUELLE du jeu (site dofuspourlesnoobs + DofusDB) alors que
le serveur et le client sont en 2.51. Les nouveaux sorts seront AJOUTÉS au client (Spells.d2o / SpellLevels.d2o / textes) et au
serveur par un outil qui lira ta fiche. Les sorts 2.51 existants peuvent aussi être MODIFIÉS (nouvelle version d'un niveau).
Le but : que ça marche du premier coup. Sois exhaustif et précis ; en cas de doute, écris-le dans `doutes` (pas de supposition muette).

## Sources
- Pages du site en texte : `C:\Users\Beny\AppData\Local\Temp\claude\C--Users-Beny-Desktop-DOFUS-NEKFEU-JTE\c7cc6475-2f5d-4f2a-91f1-9a4d62fc6ad9\scratchpad\dpln\txt\<slug>.txt`
- DofusDB (version actuelle, chiffres et effets exacts) : `cd "C:\Users\Beny\AppData\Local\Temp\claude\C--Users-Beny-Desktop-DOFUS-NEKFEU-JTE\c7cc6475-2f5d-4f2a-91f1-9a4d62fc6ad9\scratchpad" && py -I dofusdb.py <idMonstre>` → `dofusdb\<id>.txt`
  (les ids de monstres DofusDB peuvent différer des ids 2.51 : cherche aussi par nom : `https://api.dofusdb.fr/monsters?name.fr=<nom>&lang=fr`
  avec curl ; sorts : `https://api.dofusdb.fr/spells/<id>?lang=fr`, niveaux : `https://api.dofusdb.fr/spell-levels?spellId=<id>&lang=fr`).
  Zones DofusDB : `{'shape': 80, 'param1': 1, 'param2': 0}` → rawZone 2.51 = `chr(shape)` + param1 [+ `,` + param2 si utile]
  (80 'P' point, 67 'C' cercle, 88 'X' croix, 76 'L' ligne, 84 'T', 81 'Q', 97 'a' toute la carte, 65 'A', 71 'G' carré, 68 'D', 79 'O', 35 '#', 43 '+', 59 ';' cases fixes).
- Sorts 2.51 (client + base) : `cd "C:/Users/Beny/Desktop/DOFUS NEKFEU JTE/_OUTILS/serveur/donjons" && py -I analyse_mecaniques.py "Nom1,Nom2" [motclé]`
  et pour un sort précis à un niveau : `py -I "C:\Users\Beny\AppData\Local\Temp\claude\C--Users-Beny-Desktop-DOFUS-NEKFEU-JTE\c7cc6475-2f5d-4f2a-91f1-9a4d62fc6ad9\scratchpad\s3.py" <sort>:<niveau>,<sort>:<niveau>`
  (l'outil signale « EFFET NON GERE » / « DECLENCHEUR INCONNU » : n'utilise QUE des effets et déclencheurs gérés par le serveur).
- Base : `"C:/Users/Beny/Desktop/DOFUS NEKFEU JTE/_OUTILS/logiciels/MariaDB/bin/mysql.exe" -uroot world -B -e "..."` (noms espagnols ;
  `monsters_spells` (MonsterGradeId, SpellId, Level), `monsters_grades` (Id, MonsterId, GradeId)).
- Code serveur (effets gérés) : `C:/Users/Beny/Desktop/DOFUS NEKFEU JTE/SERVEUR_Emu/server/stump.server.worldserver/game/effects/handlers/**`,
  déclencheurs : `game/fights/Buffs/TriggerBuff.cs`. Fiches d'audit : `_OUTILS/serveur/donjons/dpln/lot*.md`, décisions : `DECISIONS.md`.

## Règles
1. Effets : uniquement des effectId existants dans le client 2.51 ET gérés par le serveur. Si DofusDB utilise un effet récent, le
   remplacer par l'équivalent 2.51 (et le dire dans `doutes`). Pour un passif : sort à effet 792/793 (« lance le sort … ») avec
   durée -1 et déclencheur, comme les passifs 2.51 (exemple : regarde un passif existant avec s3.py, ex. 3798, 7075, 6103).
2. Visuel : chaque nouveau sort copie un sort 2.51 proche pour l'icône / l'animation (`copierDe`).
3. Plages d'identifiants RÉSERVÉES à ta fiche (ne pas en sortir) : sorts, niveaux de sort et textes indiqués dans ton message.
4. Un niveau de sort par grade du monstre quand les chiffres changent par grade, sinon un seul niveau.
5. Les monstres doivent recevoir les sorts (`monstres.ajouter`) et perdre les sorts remplacés (`monstres.retirer`).
6. Si un comportement ne peut pas s'exprimer en sorts (ex. « au début du combat », IA particulière), décris-le dans `cerveaux`
   (le coordinateur le codera) avec le passif à poser.

## Format de la fiche (`_OUTILS/serveur/donjons/dpln/sorts/<nom>.json`, JSON valide, UTF-8)
```json
{
  "donjon": "Nid du Kwakwa",
  "sorts": [
    {
      "action": "nouveau",
      "id": 13000, "nomTexteId": 9000000, "nom": "Kwapoussée",
      "descriptionTexteId": 9000001, "description": "texte du tooltip (reformulé, chiffres exacts)",
      "copierDe": 2414,
      "niveaux": [
        {"id": 45000, "grade": 1, "apCost": 3, "minRange": 1, "range": 4, "castInLine": false, "castInDiagonal": false,
         "castTestLos": true, "criticalHitProbability": 0, "needFreeCell": false, "needTakenCell": false, "needFreeTrapCell": false,
         "rangeCanBeBoosted": false, "maxStack": -1, "maxCastPerTurn": 1, "maxCastPerTarget": 0, "minCastInterval": 0,
         "initialCooldown": 0, "globalCooldown": 0, "hidden": false, "statesRequired": [], "statesAuthorized": [], "statesForbidden": [],
         "effects": [
           {"effectId": 97, "diceNum": 20, "diceSide": 25, "value": 0, "targetMask": "A", "rawZone": "P1", "duration": 0,
            "delay": 0, "triggers": "I", "random": 0, "group": 0, "dispellable": 1, "effectElement": -1}
         ]}
      ]
    },
    {"action": "modifier", "id": 2414, "descriptionTexteId": 9000002, "description": "nouveau texte (optionnel)",
     "niveaux": [{"id": 12345, "changements": {"apCost": 4}, "effects": [ ...liste complète des effets du niveau... ]}]}
  ],
  "monstres": [{"id": 2995, "ajouter": [{"sort": 13000, "niveauParGrade": [1, 1, 1, 1, 1]}], "retirer": [1234]}],
  "cerveaux": [{"monstre": 2995, "description": "pose le passif 13001 au début du combat / à l'arrivée", "passif": 13001}],
  "doutes": ["…"]
}
```
`niveauParGrade` = niveau du sort pour les grades 1..5 du monstre (index du niveau dans `niveaux`, à partir de 1).
Pour « modifier », `effects` = la liste COMPLÈTE des effets du niveau (pas seulement les changés) ; reprends les effets 2.51 avec s3.py.

Ta réponse finale (au coordinateur) : 10 lignes max : chemin de la fiche, nombre de sorts nouveaux / modifiés, doutes importants.
