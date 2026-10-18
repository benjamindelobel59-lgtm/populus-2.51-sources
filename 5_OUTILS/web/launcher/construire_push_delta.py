# -*- coding: utf-8 -*-
"""
MYSTIC 17/10/2026 : construit un paquet PUSH ne contenant QUE ce qui a changé depuis le dernier paquet envoyé (règle utilisateur).
- source : un paquet de travail (tout ce qui n'est pas encore envoyé, éventuellement cumulatif) ;
- référence : le dernier paquet envoyé (SAVE/pushs_envoyes/...) ;
- copie dans le nouveau paquet : 2_BDD (SQL absents de la référence), 3_SERVEUR / 4_PANEL_ADMIN / 6_CLIENT / 7_CLIENT
  (fichiers nouveaux ou différents), les textes (LISEZ-MOI, CONTENU, PATCHNOTE, 9_VERIFICATIONS, 1_SAUVEGARDER_LE_VPS) ;
- launcher : met à jour le client cumulé (_OUTILS/web/launcher/client_cumule/app = tous les fichiers client modifiés
  depuis le début), écrit 5_LAUNCHER/publication/manifest.txt COMPLET (liste de tous les fichiers) mais files/ ne contient
  QUE les fichiers nouveaux (sur le VPS, les anciens fichiers restent dans C:\\NekfeuMaj\\files).
Utilisation : py -I construire_push_delta.py <paquet de travail> <dernier paquet envoyé> <nouveau paquet>
"""
import hashlib
import os
import shutil
import sys
import time

source, reference, sortie = sys.argv[1:4]
ICI = os.path.dirname(os.path.abspath(__file__))
CUMUL = os.path.join(ICI, 'client_cumule', 'app')


def fichiers(d):
    for r, _, fs in os.walk(d):
        for f in fs:
            yield os.path.relpath(os.path.join(r, f), d)


def identique(a, b):
    if not os.path.isfile(b) or os.path.getsize(a) != os.path.getsize(b):
        return False
    return open(a, 'rb').read() == open(b, 'rb').read()


def copier(src, dst):
    os.makedirs(os.path.dirname(dst), exist_ok=True)
    shutil.copy2(src, dst)


assert not os.path.exists(sortie), sortie
os.makedirs(sortie)
bilan = {}

# textes
for nom in ('LISEZ-MOI.md', 'CONTENU.md', 'PATCHNOTE.md', '9_VERIFICATIONS.md'):
    if os.path.isfile(os.path.join(source, nom)):
        copier(os.path.join(source, nom), os.path.join(sortie, nom))
if os.path.isdir(os.path.join(source, '1_SAUVEGARDER_LE_VPS')):
    shutil.copytree(os.path.join(source, '1_SAUVEGARDER_LE_VPS'), os.path.join(sortie, '1_SAUVEGARDER_LE_VPS'))

# SQL nouveaux
deja = set(os.listdir(os.path.join(reference, '2_BDD'))) if os.path.isdir(os.path.join(reference, '2_BDD')) else set()
for f in sorted(os.listdir(os.path.join(source, '2_BDD'))):
    if f not in deja:
        copier(os.path.join(source, '2_BDD', f), os.path.join(sortie, '2_BDD', f))
        bilan.setdefault('2_BDD', []).append(f)

# fichiers nouveaux ou changés
for dossier in ('3_SERVEUR', '4_PANEL_ADMIN', '6_CLIENT_64bits', '7_CLIENT_32bits'):
    src = os.path.join(source, dossier)
    if not os.path.isdir(src):
        continue
    for rel in sorted(fichiers(src)):
        if rel.endswith('config.local.php'):
            continue
        if not identique(os.path.join(src, rel), os.path.join(reference, dossier, rel)):
            copier(os.path.join(src, rel), os.path.join(sortie, dossier, rel))
            bilan.setdefault(dossier, []).append(rel)

# launcher : client cumulé + manifeste complet + seulement les nouveaux fichiers
os.makedirs(CUMUL, exist_ok=True)
nouveaux = []
src64 = os.path.join(source, '6_CLIENT_64bits', 'app')
if os.path.isdir(src64):
    for rel in fichiers(src64):
        dst = os.path.join(CUMUL, rel)
        if not identique(os.path.join(src64, rel), dst):
            copier(os.path.join(src64, rel), dst)
            nouveaux.append(rel)
pub = os.path.join(sortie, '5_LAUNCHER', 'publication')
os.makedirs(os.path.join(pub, 'files'))
lignes = ['version=' + time.strftime('%Y-%m-%d_%Hh%M')]
envoyes = 0
for rel in sorted(fichiers(CUMUL), key=lambda x: x.replace(os.sep, '/').lower()):
    chemin = os.path.join(CUMUL, rel)
    h = hashlib.sha256(open(chemin, 'rb').read()).hexdigest()
    lignes.append(f"{h}\t{os.path.getsize(chemin)}\t{rel.replace(os.sep, '/')}")
    if rel in nouveaux or not identique(chemin, os.path.join(reference, '6_CLIENT_64bits', 'app', rel)):
        blob = os.path.join(pub, 'files', h + '.bin')
        if not os.path.exists(blob):
            shutil.copy2(chemin, blob)
            envoyes += 1
open(os.path.join(pub, 'manifest.txt'), 'w', encoding='utf-8', newline='\n').write('\n'.join(lignes) + '\n')
shutil.copy2(os.path.join(ICI, 'VPS_publier_maj.bat'), os.path.join(sortie, '5_LAUNCHER', 'VPS_publier_maj.bat'))
bilan['5_LAUNCHER'] = [f'manifest complet : {len(lignes) - 1} fichiers ; files/ : {envoyes} nouveau(x)']

for k, v in bilan.items():
    print(k, len(v), ':', ', '.join(v[:12]) + (' …' if len(v) > 12 else ''))
