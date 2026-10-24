# PADGREF 23/10 (demande utilisateur : « les skins déjà liés à des objets, ne les mets pas ») : tous les skins déjà utilisés quelque
# part -> skins_exclus.json (lu par lot2_skins2.py) :
#   - base : AppearanceId des objets, SkinsCSV des objets vivants, looks des monstres / PNJ / classes / têtes / montures ;
#   - clients 2.51 et 2.74 : toutes les chaînes de look « {os|skins|…} » des fichiers de données (.d2o).
import glob, json, os, re, subprocess, sys
ICI = os.path.dirname(os.path.abspath(__file__))
R = os.path.abspath(os.path.join(ICI, '..', '..', '..', '..'))
MYSQL = os.path.join(R, '_OUTILS', 'logiciels', 'MariaDB', 'bin', 'mysql.exe')


def rq(s):
    out = subprocess.run([MYSQL, '-uroot', '-h127.0.0.1', '-N', '-B', '--default-character-set=utf8', 'world', '-e', s],
                         capture_output=True).stdout.decode('utf-8', 'replace')
    return [l.split('\t') for l in out.replace('\r', '').split('\n') if l]


def skins_de_look(texte):
    res = set()
    for m in re.finditer(r'\{\d+\|([0-9,]*)', texte):
        res |= {int(x) for x in m.group(1).split(',') if x}
    return res


exclus, sources = set(), {}
def ajout(nom, s):
    s = set(s); sources[nom] = len(s - exclus); exclus.update(s)


ajout('objets (AppearanceId)', (int(a) for (a,) in rq("SELECT AppearanceId FROM items_templates WHERE AppearanceId > 0 UNION "
                                                    "SELECT AppearanceId FROM items_templates_weapons WHERE AppearanceId > 0")))
ajout('objets vivants', (int(x) for (c,) in rq("SELECT SkinsCSV FROM items_livingobjects") for x in c.split(',') if x.strip().isdigit()))
looks = []
for req in ("SELECT LookAsString FROM monsters_templates", "SELECT LookAsString FROM npcs_templates",
            "SELECT CONCAT(MaleLookString, ' ', FemaleLookString) FROM breeds"):
    looks += [l[0] for l in rq(req)]
for t in ('breeds_heads', 'mounts_templates', 'mounts_harness'):
    cols = [c for (c, *_r) in rq(f"SHOW COLUMNS FROM {t}")]
    for c in cols:
        if re.search(r'look|skin', c, re.I):
            looks += [f'{{0|{v}}}' if re.fullmatch(r'[0-9,]+', v or '') else v for (v,) in rq(f"SELECT `{c}` FROM {t}")]
ajout('looks en base', skins_de_look(' '.join(looks)))
for nom, dossier in (('client 2.51', os.path.join(R, 'CLIENT_64bits', 'app', 'data', 'common')),
                     ('client 2.74', os.path.join(R, '_OUTILS', 'client', 'client_27x', 'data', 'common'))):
    s = set()
    for f in glob.glob(os.path.join(dossier, '*.d2o')):
        s |= skins_de_look(open(f, 'rb').read().decode('latin-1'))
    ajout(f'looks du {nom}', s)
json.dump(sorted(exclus), open(os.path.join(ICI, 'skins_exclus.json'), 'w'))
print('skins exclus :', len(exclus), sources)
