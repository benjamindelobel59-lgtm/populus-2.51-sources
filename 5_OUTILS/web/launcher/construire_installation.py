# -*- coding: utf-8 -*-
"""
MYSTIC 18/10/2026 : prépare l'INSTALLATION COMPLÈTE du jeu pour le launcher Mystic (bouton « Installer »).
Lit le client 64 bits (CLIENT_64bits/app) et écrit dans <sortie> :
  complet.txt          : « version=… » puis une ligne par fichier : sha256 <TAB> taille <TAB> chemin relatif à app
  files/<sha256>.bin   : contenu de chaque fichier (même format que les mises à jour : à copier dans C:\\NekfeuMaj\\files du VPS)
Exclus : sauvegardes (*.bak, *.avant_*), journaux (*.log, debug.txt), téléchargements en cours, anciens launchers.
config.xml : connection.host remplacé par l'adresse du VPS (le client local pointe sur 127.0.0.1).
Utilisation : py -I construire_installation.py <dossier app du client> <sortie> [ip du VPS]
"""
import hashlib, os, re, shutil, sys, time

src, sortie = sys.argv[1], sys.argv[2]
IP_VPS = sys.argv[3] if len(sys.argv) > 3 else 'IP_DU_VPS'

def exclu(rel):
    nom = os.path.basename(rel).lower()
    if nom.endswith(('.bak', '.log', '.mystic-dl', '.nekfeu-dl', '.tmp')) or '.avant_' in nom or '.avant-' in nom:
        return True
    if nom in ('debug.txt', 'nekfeulauncher.exe', 'mysticlauncher.exe', 'launcher.ini'):
        return True
    # ajouts locaux absents du client 2.51 d'origine (et des clients des joueurs)
    if rel in ('DofusCmd.exe', 'configa.xml', 'menuadmin.xml'):
        return True
    return False

os.makedirs(os.path.join(sortie, 'files'), exist_ok=True)
lignes, total, n, copies = [], 0, 0, 0
for racine, _, fichiers in os.walk(src):
    for f in fichiers:
        chemin = os.path.join(racine, f)
        rel = os.path.relpath(chemin, src).replace(os.sep, '/')
        if exclu(rel):
            continue
        data = open(chemin, 'rb').read()
        if rel == 'config.xml':
            texte = data.decode('utf-8')
            texte, nb = re.subn(r'(<entry key="connection\.host">)[^<]*(</entry>)', r'\g<1>' + IP_VPS + r'\g<2>', texte)
            assert nb == 1, 'connection.host introuvable dans config.xml'
            data = texte.encode('utf-8')
        h = hashlib.sha256(data).hexdigest()
        blob = os.path.join(sortie, 'files', h + '.bin')
        if not os.path.exists(blob):
            open(blob, 'wb').write(data)
            copies += 1
        lignes.append(f"{h}\t{len(data)}\t{rel}")
        total += len(data)
        n += 1
        if n % 500 == 0:
            print(n, 'fichiers…', flush=True)

lignes.sort(key=lambda l: l.split('\t')[2].lower())
version = time.strftime('%Y-%m-%d_%Hh%M')
open(os.path.join(sortie, 'complet.txt'), 'w', encoding='utf-8', newline='\n').write('version=' + version + '\n' + '\n'.join(lignes) + '\n')
print(f"{n} fichiers, {total / (1 << 30):.2f} Go, {copies} blocs écrits -> {sortie} (version {version})")
