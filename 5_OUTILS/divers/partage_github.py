# -*- coding: utf-8 -*-
"""
MYSTIC : construit le dossier PARTAGE_GITHUB (dépôt GitHub public de partage avec l'autre dev) bien rangé, puis commit + push.
GitHub sert UNIQUEMENT à partager nos modifications (on ne travaille pas avec ; les mises en ligne VPS restent manuelles).

  1_SERVEUR/        sources de l'émulateur          ← SOURCES_Populus (sans panel-web)
  2_PANEL_WEB/      panel d'administration          ← SOURCES_Populus/panel-web (jamais config.local.php)
  3_SQL_WORLD/      nos scripts base world 01 → NN  ← PUSH/<dernier paquet>/2_BDD
  4_CLIENT/64bits|32bits/app  fichiers client MODIFIÉS seulement ← PUSH/<paquet>/6_CLIENT_64bits|7_CLIENT_32bits
  5_OUTILS/         nos scripts                      ← _OUTILS (sélection, IP du VPS masquée)
  6_SERVEURS_COMPILES/AuthServer|WorldServer  serveurs prêts à lancer ← SERVEUR_Emu/run/debug (sans journaux, sauvegardes,
                    ni état ; configs = celles du PC local, accès vides, clé WebAPIKey masquée)
  7_PAQUETS_VPS/<paquet>/  paquets de mise en ligne du VPS en cours (TOUT_INSTALLER.bat, COMPLEMENT.bat…) ← PUSH/
                    sans les fichiers du jeu (5_LAUNCHER/maj/files, 3,4 Go)
  HANDOFF.md        (pour le Claude de l'autre dev, à tenir à jour : _OUTILS/divers/partage_github_HANDOFF.md)
  JOURNAL_DES_MODIFICATIONS.txt ← SOURCES_Populus/LISEZ-MOI.txt

Utilisation : python -I _OUTILS/divers/partage_github.py ["message de commit"]   (sans message : prépare seulement, pas de push)
"""
import os, re, shutil, subprocess, sys, datetime
sys.stdout.reconfigure(encoding='utf-8', errors='replace')

RACINE = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
DEST = os.path.join(RACINE, 'PARTAGE_GITHUB')
SRC = os.path.join(RACINE, 'SOURCES_Populus')
IP_VPS = 'IP_DU_VPS'
LIEN = 'https://github.com/benjamindelobel59-lgtm/populus-2.51-sources/archive/refs/heads/main.zip'
IGNORE_SRC = shutil.ignore_patterns('.git', 'bin', 'obj', '.vs', 'packages', 'run', '*.user', '*.suo', 'config.local.php',
                                    'panel-web', 'CLIENT_64bits', 'CLIENT_32bits', 'OUTILS', 'SQL', 'HANDOFF.md', 'README.md',
                                    '.gitignore', 'LISEZ-MOI.txt')
OUTILS = ['serveur/donjons', 'serveur/recherches', 'serveur/ia', 'serveur/trousseau', 'serveur/interactifs', 'serveur/placements',
          'client/client-build', 'client/client-configs', 'web/launcher', 'divers/partage_github.py', 'serveur/importer_dump_vps.sh',
          'web/atelier', 'client/icone_exe']
EXT_OUTILS = ('.py', '.sql', '.tsv', '.ps1', '.bat', '.sh', '.md', '.as', '.cs')
EXCLUS_OUTILS = re.compile(r'(^|/)(verif[^/]*|x32|publication|build-emu|pcode[^/]*|__pycache__|installation|client_cumule|maquette)(/|$)'
                           r'|(^|/)compte_test_local\.md$')  # MYSTIC 18/10 : identifiants de test locaux, jamais sur le dépôt public


def paquets_en_cours():
    """MYSTIC 18/10 : tous les paquets de PUSH/ (un gros paquet + ses compléments), du plus ancien au plus récent"""
    return sorted((os.path.join(RACINE, 'PUSH', d) for d in os.listdir(os.path.join(RACINE, 'PUSH')) if d.startswith('PUSH_') and os.path.isdir(os.path.join(RACINE, 'PUSH', d))),
                  key=os.path.getmtime)


IGNORE_RUN = shutil.ignore_patterns('logs', '*.log', '*.bak', '* - Copie*', '*.rar', '*.vshost.*', 'monstergroups.save', 'Thumbs.db')


def copier_serveurs(dest):
    """MYSTIC 18/10 (demande utilisateur) : dossiers complets des serveurs compilés (DLL, exe, configs, données)"""
    for nom in ('AuthServer', 'WorldServer'):
        shutil.copytree(os.path.join(RACINE, 'SERVEUR_Emu', 'run', 'debug', nom), os.path.join(dest, nom), ignore=IGNORE_RUN)
    # clés et mots de passe des configs : jamais sur le dépôt public
    for r, _, fs in os.walk(dest):
        for f in fs:
            if f.endswith('_config.xml'):
                q = os.path.join(r, f)
                t = open(q, encoding='utf-8-sig').read()
                t2 = re.sub(r'(<Variable name="WebAPIKey"[^>]*>)[^<]*', r'\1A_CHANGER', t)
                t2 = re.sub(r'(<Password>)[^<]+(</Password>)', r'\1\2', t2)
                if t2 != t:
                    open(q, 'w', encoding='utf-8').write(t2)


IGNORE_PAQUET = shutil.ignore_patterns('files', '*.log')  # files = fichiers du jeu (3,4 Go) : voir LISEZ-MOI


def copier_paquets(dest, paquets):
    """MYSTIC 19/10 (demande utilisateur) : paquets VPS en cours, tels quels, sans les gros fichiers du jeu"""
    for paquet in paquets:
        shutil.copytree(paquet, os.path.join(dest, os.path.basename(paquet)), ignore=IGNORE_PAQUET)
    open(os.path.join(dest, 'LISEZ-MOI.md'), 'w', encoding='utf-8').write(
        "# Paquets de mise en ligne du VPS\n\n"
        "Copie des paquets `PUSH/` en cours côté Mystic (procédure, `TOUT_INSTALLER.bat`, compléments, SQL, AuthServer, site…).\n"
        "Pas inclus : `5_LAUNCHER/maj/files` (fichiers du jeu, plusieurs Go).\n"
        "L'adresse du VPS est remplacée par `IP_DU_VPS` dans les textes.\n")


def vider(dossier):
    """vide PARTAGE_GITHUB sauf .git"""
    os.makedirs(dossier, exist_ok=True)
    for n in os.listdir(dossier):
        if n == '.git':
            continue
        p = os.path.join(dossier, n)
        shutil.rmtree(p) if os.path.isdir(p) else os.remove(p)


def copier_outils(dest):
    for rel in OUTILS:
        src = os.path.join(RACINE, '_OUTILS', *rel.split('/'))
        if os.path.isfile(src):
            fichiers = [src]
        elif os.path.isdir(src):
            fichiers = [os.path.join(r, f) for r, _, fs in os.walk(src) for f in fs]
        else:
            continue
        for f in fichiers:
            relf = os.path.relpath(f, os.path.join(RACINE, '_OUTILS')).replace('\\', '/')
            if EXCLUS_OUTILS.search(relf) or not f.endswith(EXT_OUTILS):
                continue
            if os.path.getsize(f) > 5 * 1024 * 1024:
                continue
            cible = os.path.join(dest, *relf.split('/'))
            os.makedirs(os.path.dirname(cible), exist_ok=True)
            shutil.copy2(f, cible)
    for nom in ('bin/NekfeuLauncher.exe', 'bin_mystic/MysticLauncher.exe'):
        exe = os.path.join(RACINE, '_OUTILS', 'web', 'launcher', *nom.split('/'))
        if os.path.isfile(exe):
            os.makedirs(os.path.join(dest, 'web', 'launcher', os.path.dirname(nom)), exist_ok=True)
            shutil.copy2(exe, os.path.join(dest, 'web', 'launcher', *nom.split('/')))


def masquer_ip(dossier):
    n = 0
    for r, _, fs in os.walk(dossier):
        if '.git' in r.split(os.sep):
            continue
        for f in fs:
            if not f.endswith(EXT_OUTILS + ('.txt', '.php', '.js', '.xml', '.config', '.json')):
                continue
            p = os.path.join(r, f)
            try:
                s = open(p, encoding='utf-8').read()
            except UnicodeDecodeError:
                continue
            if IP_VPS in s:
                open(p, 'w', encoding='utf-8', newline='').write(s.replace(IP_VPS, 'IP_DU_VPS'))
                n += 1
    return n


def secrets(dossier):
    motif = re.compile(r"passw(or)?d\s*[:=]\s*['\"](?!motdepasse['\"])[^'\"]{3,}|BEGIN (RSA|PRIVATE)|gh[op]_[A-Za-z0-9]{20,}|discord(app)?\.com/api/webhooks/\d+/|"
                       r"mot de passe( web)?\s*:\s*\S{6,}|" + re.escape(IP_VPS), re.I)
    alertes = []
    for r, _, fs in os.walk(dossier):
        if '.git' in r.split(os.sep) or 'Library' in r.split(os.sep) or 'Tools' in r.split(os.sep):
            continue
        for f in fs:
            if f.endswith(('.cs', '.php', '.py', '.ps1', '.bat', '.sh', '.config', '.json', '.md', '.txt', '.sql')) or f.endswith('_config.xml'):
                p = os.path.join(r, f)
                try:
                    if motif.search(open(p, encoding='utf-8', errors='ignore').read()):
                        alertes.append(os.path.relpath(p, dossier))
                except OSError:
                    pass
    return alertes


def main():
    paquets = paquets_en_cours()
    # MYSTIC 17/10 : 3_SQL_WORLD cumulatif (les SQL déjà partagés restent, ceux du paquet sont ajoutés / remplacés)
    sql_deja = {}
    if os.path.isdir(os.path.join(DEST, '3_SQL_WORLD')):
        for f in os.listdir(os.path.join(DEST, '3_SQL_WORLD')):
            if f.endswith('.sql'):
                sql_deja[f] = open(os.path.join(DEST, '3_SQL_WORLD', f), 'rb').read()
    # MYSTIC 18/10 : 4_CLIENT cumulatif aussi (les fichiers client déjà partagés restent, ceux des paquets sont ajoutés / remplacés)
    garde_client = os.path.join(RACINE, 'PARTAGE_GITHUB_4_CLIENT.tmp')
    if os.path.isdir(garde_client):
        shutil.rmtree(garde_client)
    if os.path.isdir(os.path.join(DEST, '4_CLIENT')):
        shutil.move(os.path.join(DEST, '4_CLIENT'), garde_client)
    vider(DEST)
    if os.path.isdir(garde_client):
        shutil.move(garde_client, os.path.join(DEST, '4_CLIENT'))
    shutil.copytree(SRC, os.path.join(DEST, '1_SERVEUR'), ignore=IGNORE_SRC)
    shutil.copytree(os.path.join(SRC, 'panel-web'), os.path.join(DEST, '2_PANEL_WEB'), ignore=shutil.ignore_patterns('config.local.php'))
    os.makedirs(os.path.join(DEST, '3_SQL_WORLD'))
    for f, contenu in sql_deja.items():
        open(os.path.join(DEST, '3_SQL_WORLD', f), 'wb').write(contenu)
    for paquet in paquets:
        if os.path.isdir(os.path.join(paquet, '2_BDD')):
            for f in sorted(os.listdir(os.path.join(paquet, '2_BDD'))):
                shutil.copy2(os.path.join(paquet, '2_BDD', f), os.path.join(DEST, '3_SQL_WORLD', f))
        for src, dst in (('6_CLIENT_64bits', '64bits'), ('7_CLIENT_32bits', '32bits')):
            if os.path.isdir(os.path.join(paquet, src)):
                shutil.copytree(os.path.join(paquet, src), os.path.join(DEST, '4_CLIENT', dst), dirs_exist_ok=True)
    copier_outils(os.path.join(DEST, '5_OUTILS'))
    copier_serveurs(os.path.join(DEST, '6_SERVEURS_COMPILES'))
    copier_paquets(os.path.join(DEST, '7_PAQUETS_VPS'), paquets)
    shutil.copy2(os.path.join(SRC, 'LISEZ-MOI.txt'), os.path.join(DEST, 'JOURNAL_DES_MODIFICATIONS.txt'))
    for nom in ('partage_github_HANDOFF.md', 'partage_github_LISEZ-MOI.md', 'partage_github_SQL_LISEZ-MOI.md'):
        shutil.copy2(os.path.join(RACINE, '_OUTILS', 'divers', nom),
                     os.path.join(DEST, {'partage_github_HANDOFF.md': 'HANDOFF.md', 'partage_github_LISEZ-MOI.md': 'LISEZ-MOI.md',
                                         'partage_github_SQL_LISEZ-MOI.md': os.path.join('3_SQL_WORLD', 'LISEZ-MOI.md')}[nom]))
    open(os.path.join(DEST, '.gitignore'), 'w', encoding='utf-8').write("bin/\nobj/\n.vs/\n*.user\n*.suo\nconfig.local.php\n")
    n = masquer_ip(DEST)
    alertes = secrets(DEST)
    print(f"PARTAGE_GITHUB construit depuis {', '.join(os.path.basename(x) for x in paquets)} ; IP masquee dans {n} fichier(s).")
    if alertes:
        print("ARRET : secrets possibles dans :", *alertes, sep='\n  ')
        sys.exit(1)
    if len(sys.argv) > 1:
        git = lambda *a: subprocess.run(['git', '-C', DEST, *a], check=True)
        git('add', '-A')
        if subprocess.run(['git', '-C', DEST, 'diff', '--cached', '--quiet']).returncode == 0:
            print('Rien de nouveau a envoyer.')
        else:
            git('commit', '-q', '-m', sys.argv[1] + '\n\nCo-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>')
            git('push', '-q')
            print('Pousse. Lien :', LIEN)


main()
