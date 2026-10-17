# -*- coding: utf-8 -*-
"""
MYSTIC : construit le dossier PARTAGE_GITHUB (dépôt GitHub public de partage avec l'autre dev) bien rangé, puis commit + push.
GitHub sert UNIQUEMENT à partager nos modifications (on ne travaille pas avec ; les mises en ligne VPS restent manuelles).

  1_SERVEUR/        sources de l'émulateur          ← SOURCES_Populus (sans panel-web)
  2_PANEL_WEB/      panel d'administration          ← SOURCES_Populus/panel-web (jamais config.local.php)
  3_SQL_WORLD/      nos scripts base world 01 → NN  ← PUSH/<dernier paquet>/2_BDD
  4_CLIENT/64bits|32bits/app  fichiers client MODIFIÉS seulement ← PUSH/<paquet>/6_CLIENT_64bits|7_CLIENT_32bits
  5_OUTILS/         nos scripts                      ← _OUTILS (sélection, IP du VPS masquée)
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
          'client/client-build', 'client/client-configs', 'web/launcher', 'divers/partage_github.py', 'serveur/importer_dump_vps.sh']
EXT_OUTILS = ('.py', '.sql', '.tsv', '.ps1', '.bat', '.sh', '.md', '.as', '.cs')
EXCLUS_OUTILS = re.compile(r'(^|/)(verif[^/]*|x32|publication|build-emu|pcode[^/]*|__pycache__)(/|$)')


def dernier_paquet():
    paquets = sorted((os.path.join(RACINE, 'PUSH', d) for d in os.listdir(os.path.join(RACINE, 'PUSH')) if d.startswith('PUSH_')),
                     key=os.path.getmtime)
    return paquets[-1]


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
            if EXCLUS_OUTILS.search(relf) or not (f.endswith(EXT_OUTILS) or relf == 'web/launcher/bin/NekfeuLauncher.exe'):
                continue
            if os.path.getsize(f) > 5 * 1024 * 1024:
                continue
            cible = os.path.join(dest, *relf.split('/'))
            os.makedirs(os.path.dirname(cible), exist_ok=True)
            shutil.copy2(f, cible)
    exe = os.path.join(RACINE, '_OUTILS', 'web', 'launcher', 'bin', 'NekfeuLauncher.exe')
    if os.path.isfile(exe):
        os.makedirs(os.path.join(dest, 'web', 'launcher', 'bin'), exist_ok=True)
        shutil.copy2(exe, os.path.join(dest, 'web', 'launcher', 'bin'))


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
    motif = re.compile(r"passw(or)?d\s*[:=]\s*['\"][^'\"]{3,}|BEGIN (RSA|PRIVATE)|gh[op]_[A-Za-z0-9]{20,}|" + re.escape(IP_VPS), re.I)
    alertes = []
    for r, _, fs in os.walk(dossier):
        if '.git' in r.split(os.sep) or 'Library' in r.split(os.sep) or 'Tools' in r.split(os.sep):
            continue
        for f in fs:
            if f.endswith(('.cs', '.php', '.py', '.ps1', '.bat', '.sh', '.config', '.json', '.md', '.txt', '.sql')):
                p = os.path.join(r, f)
                try:
                    if motif.search(open(p, encoding='utf-8', errors='ignore').read()):
                        alertes.append(os.path.relpath(p, dossier))
                except OSError:
                    pass
    return alertes


def main():
    paquet = dernier_paquet()
    # MYSTIC 17/10 : 3_SQL_WORLD cumulatif (les SQL déjà partagés restent, ceux du paquet sont ajoutés / remplacés)
    sql_deja = {}
    if os.path.isdir(os.path.join(DEST, '3_SQL_WORLD')):
        for f in os.listdir(os.path.join(DEST, '3_SQL_WORLD')):
            if f.endswith('.sql'):
                sql_deja[f] = open(os.path.join(DEST, '3_SQL_WORLD', f), 'rb').read()
    vider(DEST)
    shutil.copytree(SRC, os.path.join(DEST, '1_SERVEUR'), ignore=IGNORE_SRC)
    shutil.copytree(os.path.join(SRC, 'panel-web'), os.path.join(DEST, '2_PANEL_WEB'), ignore=shutil.ignore_patterns('config.local.php'))
    os.makedirs(os.path.join(DEST, '3_SQL_WORLD'))
    for f, contenu in sql_deja.items():
        open(os.path.join(DEST, '3_SQL_WORLD', f), 'wb').write(contenu)
    for f in sorted(os.listdir(os.path.join(paquet, '2_BDD'))):
        shutil.copy2(os.path.join(paquet, '2_BDD', f), os.path.join(DEST, '3_SQL_WORLD', f))
    for src, dst in (('6_CLIENT_64bits', '64bits'), ('7_CLIENT_32bits', '32bits')):
        if os.path.isdir(os.path.join(paquet, src)):
            shutil.copytree(os.path.join(paquet, src), os.path.join(DEST, '4_CLIENT', dst))
    copier_outils(os.path.join(DEST, '5_OUTILS'))
    shutil.copy2(os.path.join(SRC, 'LISEZ-MOI.txt'), os.path.join(DEST, 'JOURNAL_DES_MODIFICATIONS.txt'))
    for nom in ('partage_github_HANDOFF.md', 'partage_github_LISEZ-MOI.md', 'partage_github_SQL_LISEZ-MOI.md'):
        shutil.copy2(os.path.join(RACINE, '_OUTILS', 'divers', nom),
                     os.path.join(DEST, {'partage_github_HANDOFF.md': 'HANDOFF.md', 'partage_github_LISEZ-MOI.md': 'LISEZ-MOI.md',
                                         'partage_github_SQL_LISEZ-MOI.md': os.path.join('3_SQL_WORLD', 'LISEZ-MOI.md')}[nom]))
    open(os.path.join(DEST, '.gitignore'), 'w', encoding='utf-8').write("bin/\nobj/\n.vs/\n*.user\n*.suo\nconfig.local.php\n")
    n = masquer_ip(DEST)
    alertes = secrets(DEST)
    print(f"PARTAGE_GITHUB construit depuis {os.path.basename(paquet)} ; IP masquee dans {n} fichier(s).")
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
