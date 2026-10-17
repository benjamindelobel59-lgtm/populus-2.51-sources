# -*- coding: utf-8 -*-
# MYSTIC 17/10 : zones « ;NNN » (case fixe) des sorts, perdues dans spells_levels (l'import ne lisait pas les cases > 255).
# Ecrit SERVEUR_Emu/run/debug/WorldServer/cases_fixes.tsv : levelId TAB critique(0/1) TAB index TAB effectId TAB case.
# Le serveur (Database/Spells/CasesFixesSorts.cs) le lit au chargement des sorts.
import os, sys
ICI = os.path.dirname(os.path.abspath(__file__)); RACINE = os.path.dirname(os.path.dirname(os.path.dirname(ICI)))
sys.path.insert(0, ICI)
import d2o
B = os.path.join(RACINE, 'CLIENT_64bits', 'app', 'data', 'common')
_, L = d2o.load(os.path.join(B, 'SpellLevels.d2o'))
lignes = []
for lid, l in sorted(L.items()):
    for crit, cle in ((0, 'effects'), (1, 'criticalEffect')):
        for i, e in enumerate(l.get(cle) or []):
            z = e['rawZone']
            if z.startswith(';'):
                cases = [c.strip() for c in z[1:].split(',') if c.strip().isdigit()]
                if not cases:
                    continue
                # MYSTIC 17/10 : toutes les cases (avant : seulement la 1re -> Dés / glyphes du Chalœil sur une seule case)
                lignes.append(f"{lid}\t{crit}\t{i}\t{e['effectId']}\t{','.join(cases)}")
sortie = os.path.join(RACINE, 'SERVEUR_Emu', 'run', 'debug', 'WorldServer', 'cases_fixes.tsv')
open(sortie, 'w', encoding='utf-8', newline='\n').write('\n'.join(lignes) + '\n')
print(len(lignes), 'effets ->', sortie)
