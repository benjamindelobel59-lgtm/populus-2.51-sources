# MYSTIC 17/10 : liste les cartes d'un donjon (Dungeons.d2o) avec nom FR et spawn eventuel. Usage : cartes_donjon.py <motif nom donjon>
import sys, os, csv
sys.stdout.reconfigure(encoding="utf-8")
B = r"C:\Users\Beny\Desktop\DOFUS NEKFEU JTE"
sys.path.insert(0, os.path.join(B, r"_OUTILS\serveur\donjons"))
import d2o, d2i
C = os.path.join(B, r"CLIENT_64bits\app\data")
T = d2i.load(os.path.join(C, r"i18n\i18n_fr.d2i"))
_, mp = d2o.load(os.path.join(C, r"common\MapPositions.d2o"))
_, dj = d2o.load(os.path.join(C, r"common\Dungeons.d2o"))
sp = {}
for r in csv.DictReader(open(os.path.join(os.path.dirname(os.path.abspath(__file__)), "salles.tsv"), encoding="utf-8"), delimiter="\t"):
    sp.setdefault(int(r["map"]), []).append(r["spawn"] + "(" + r["nb_groupes"] + ")")
pat = sys.argv[1].lower()
for k, o in dj.items():
    n = T.get(o["nameId"], "?")
    if pat in n.lower():
        print("==", k, n, "niveau", o.get("optimalPlayerLevel"), "entree", o.get("entranceMapId"), "sortie", o.get("exitMapId"))
        for m in o.get("mapIds", []):
            print("  ", m, T.get((mp.get(m) or {}).get("nameId", -1), ""), " ".join(sp.get(m, [])))
