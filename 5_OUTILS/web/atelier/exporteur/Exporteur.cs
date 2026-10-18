// MYSTIC 18/10/2026 : exporteur des données de l'Atelier (site Mystic).
// Lit la base « world » du serveur (tables Stump) et écrit des JSON compacts pour la page atelier.html :
//   data.json      : types, métiers, classes, textes d'effets, objets (effets décodés), panoplies, recettes
//   sources.json   : sous-zones, monstres (résumé), qui lâche quoi
//   bestiaire.json : monstres complets (grades, caracs, butins, sorts)
// Les effets d'objets / de panoplies sont au format BinaryFormatter : on les relit avec les DLL du serveur
// (dossier run/debug/WorldServer, résolu au chargement), exactement comme le serveur.
// Usage : Exporteur.exe <dossier WorldServer> <dossier de sortie> [chaîne de connexion]

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Web.Script.Serialization;

static class Exporteur
{
    static string s_serveur;

    static int Main(string[] args)
    {
        if (args.Length < 2) { Console.Error.WriteLine("Usage : Exporteur.exe <dossier WorldServer> <sortie> [connexion]"); return 1; }
        s_serveur = Path.GetFullPath(args[0]);
        AppDomain.CurrentDomain.AssemblyResolve += (s, e) =>
        {
            var f = Path.Combine(s_serveur, new AssemblyName(e.Name).Name + ".dll");
            return File.Exists(f) ? Assembly.UnsafeLoadFrom(f) : null;
        };
        return Lancer(args[1], args.Length > 2 ? args[2] : "server=127.0.0.1;port=3306;uid=root;database=world;charset=utf8;Allow User Variables=true");
    }

    static int Lancer(string sortie, string connexion)
    {
        Directory.CreateDirectory(sortie);
        var bdd = new Bdd(connexion);
        var debut = DateTime.Now;

        var fr = new Dictionary<int, string>();
        bdd.Lire("SELECT Id, French FROM langs WHERE French IS NOT NULL", r => fr[r.Int(0)] = r.Str(1));
        Func<int, string> T = id => fr.TryGetValue(id, out var t) ? t : "";
        Console.WriteLine("Textes FR : " + fr.Count);

        // ---- types, métiers, classes, effets
        var types = new List<object>();
        bdd.Lire("SELECT Id, NameId, SuperTypeId FROM items_types ORDER BY Id", r => types.Add(new object[] { r.Int(0), T(r.Int(1)), r.Int(2) }));
        var metiers = new List<object>();
        bdd.Lire("SELECT Id, NameId, IconId FROM jobs_templates ORDER BY Id", r => metiers.Add(new object[] { r.Int(0), T(r.Int(1)), r.Int(2) }));
        var classes = new List<object>();
        bdd.Lire("SELECT Id, ShortNameId, StatsPointsForVitalityCSV, StatsPointsForWisdomCSV, StatsPointsForStrengthCSV, StatsPointsForIntelligenceCSV, StatsPointsForChanceCSV, StatsPointsForAgilityCSV, StartHealthPoint, StartActionPoints, StartMovementPoints, StartProspection, MaleLookString, FemaleLookString FROM breeds ORDER BY Id",
            r => classes.Add(new object[] { r.Int(0), T(r.Int(1)), Paliers(r.Str(2)), Paliers(r.Str(3)), Paliers(r.Str(4)), Paliers(r.Str(5)), Paliers(r.Str(6)), Paliers(r.Str(7)), r.Int(8), r.Int(9), r.Int(10), r.Int(11), r.Str(12) ?? "", r.Str(13) ?? "" }));
        var effets = new Dictionary<string, object>();
        bdd.Lire("SELECT Id, DescriptionId, Characteristic, Operator, BonusType, Category, ShowInTooltip, UseInFight FROM effects_templates",
            r => effets[r.Int(0).ToString()] = new object[] { T(r.Int(1)), r.Int(2), r.Str(3), r.Int(4), r.Int(5), r.Int(6), r.Int(7) });

        // ---- objets
        var armes = new Dictionary<int, object>();
        bdd.Lire("SELECT Id, ApCost, MinRange, WeaponRange, CriticalHitProbability, CriticalHitBonus, MaxCastPerTurn, CastInLine, CastTestLos FROM items_templates_weapons",
            r => armes[r.Int(0)] = new object[] { r.Int(1), r.Int(2), r.Int(3), r.Int(4), r.Int(5), r.Int(6), r.Int(7), r.Int(8) });
        var objets = new List<Dictionary<string, object>>();
        var nomsObjets = new Dictionary<int, string>();
        int echecs = 0;
        foreach (var table in new[] { "items_templates", "items_templates_weapons" })
            bdd.Lire("SELECT Id, NameId, TypeId, Level, IconId, Criteria, ItemSetId, DescriptionId, RealWeight, PossibleEffectsBin, Price, Etheral, TwoHanded, HideEffects FROM " + table,
                r =>
                {
                    var id = r.Int(0);
                    if (nomsObjets.ContainsKey(id)) return;
                    var nom = T(r.Int(1));
                    if (string.IsNullOrEmpty(nom)) return;
                    nomsObjets[id] = nom;
                    var o = new Dictionary<string, object> { { "id", id }, { "n", nom }, { "t", r.Int(2) }, { "l", r.Int(3) }, { "i", r.Int(4) } };
                    var crit = r.Str(5);
                    if (!string.IsNullOrWhiteSpace(crit) && crit != "null") o["c"] = crit;
                    if (r.Int(6) > 0) o["s"] = r.Int(6);
                    var desc = T(r.Int(7));
                    if (desc.Length > 0) o["d"] = desc;
                    if (r.Int(8) > 0) o["p"] = r.Int(8);
                    if (r.Int(13) != 0) o["h"] = 1;
                    try
                    {
                        var e = EffetsObjet(r.Bytes(9));
                        if (e.Count > 0) o["e"] = e;
                    }
                    catch { echecs++; }
                    if (armes.TryGetValue(id, out var a)) o["w"] = a;
                    objets.Add(o);
                });
        objets.Sort((a, b) => ((int)a["id"]).CompareTo((int)b["id"]));
        Console.WriteLine("Objets : " + objets.Count + (echecs > 0 ? " (" + echecs + " effets illisibles)" : ""));

        // ---- panoplies
        var panos = new List<object>();
        bdd.Lire("SELECT Id, NameId, ItemsCSV, EffectsBin FROM items_sets ORDER BY Id", r =>
        {
            var pieces = Csv(r.Str(2));
            List<object> bonus;
            try { bonus = EffetsPanoplie(r.Bytes(3)); } catch (Exception ex) { bonus = new List<object>(); if (echecs++ == 0) Console.WriteLine("Panoplie " + r.Int(0) + " : " + ex); }
            panos.Add(new object[] { r.Int(0), T(r.Int(1)), pieces, bonus });
        });

        // ---- recettes : [objet, métier, [[ingrédient, quantité]…]]
        var recettes = new List<object>();
        bdd.Lire("SELECT Id, JobId, IngredientIdsCSV, QuantitiesBin, ResultLevel FROM recipes ORDER BY Id", r =>
        {
            var ing = Csv(r.Str(2)); var qte = Csv(r.Str(3));
            var l = new List<object>();
            for (var i = 0; i < ing.Count && i < qte.Count; i++) l.Add(new[] { ing[i], qte[i] });
            recettes.Add(new object[] { r.Int(0), r.Int(1), l });
        });

        Ecrire(Path.Combine(sortie, "data.json"), new Dictionary<string, object>
        {
            { "v", 1 }, { "genere", DateTime.Now.ToString("yyyy-MM-dd HH:mm") },
            { "types", types }, { "metiers", metiers }, { "classes", classes }, { "effets", effets },
            { "objets", objets }, { "panoplies", panos }, { "recettes", recettes }
        });

        // ---- zones, races, monstres
        var zones = new Dictionary<int, string>();
        bdd.Lire("SELECT Id, NameId FROM world_areas", r => zones[r.Int(0)] = T(r.Int(1)));
        var sousZones = new List<object>();
        bdd.Lire("SELECT Id, NameId, AreaId, Level FROM world_subareas ORDER BY Id", r => sousZones.Add(new object[] { r.Int(0), T(r.Int(1)), zones.TryGetValue(r.Int(2), out var z) ? z : "", r.Int(3) }));
        var races = new List<object>();
        bdd.Lire("SELECT Id, NameId, SuperRaceId FROM monsters_races ORDER BY Id", r => races.Add(new object[] { r.Int(0), T(r.Int(1)), r.Int(2) }));

        var zonesMonstre = new Dictionary<int, SortedSet<int>>();
        bdd.Lire("SELECT DISTINCT MonsterId, SubAreaId FROM monsters_spawns WHERE IsDisabled = 0", r =>
        {
            if (!zonesMonstre.TryGetValue(r.Int(0), out var s)) zonesMonstre[r.Int(0)] = s = new SortedSet<int>();
            s.Add(r.Int(1));
        });
        var sortsMonstre = new Dictionary<int, List<object>>();
        var nomsSorts = new Dictionary<int, string>();
        bdd.Lire("SELECT Id, NameId FROM spells_templates", r => nomsSorts[r.Int(0)] = T(r.Int(1)));
        bdd.Lire("SELECT MonsterGradeId, SpellId, Level FROM monsters_spells ORDER BY Id", r =>
        {
            if (!sortsMonstre.TryGetValue(r.Int(0), out var l)) sortsMonstre[r.Int(0)] = l = new List<object>();
            l.Add(new[] { r.Int(1), r.Int(2) });
        });
        var champs = new[] { "GradeId", "Level", "LifePoints", "ActionPoints", "MovementPoints", "Vitality", "Wisdom", "Strength", "Intelligence", "Chance", "Agility",
                             "EarthResistance", "FireResistance", "WaterResistance", "AirResistance", "NeutralResistance", "PaDodge", "PmDodge", "TackleBlock", "TackleEvade", "GradeXp", "HiddenLevel" };
        var grades = new Dictionary<int, List<object>>();
        bdd.Lire("SELECT Id, MonsterId, " + string.Join(", ", champs) + ", StatsJSON FROM monsters_grades ORDER BY MonsterId, GradeId", r =>
        {
            var v = new List<object>();
            for (var i = 0; i < champs.Length; i++) v.Add(r.Int(2 + i));
            var json = r.Str(2 + champs.Length);
            object stats = null;
            if (!string.IsNullOrWhiteSpace(json) && json != "{}") try { stats = new JavaScriptSerializer().DeserializeObject(json); } catch { }
            if (!grades.TryGetValue(r.Int(1), out var l)) grades[r.Int(1)] = l = new List<object>();
            l.Add(new object[] { v, stats, sortsMonstre.TryGetValue(r.Int(0), out var so) ? so : new List<object>() });
        });
        var butins = new Dictionary<int, List<object>>();
        var sources = new Dictionary<int, List<object>>();
        bdd.Lire("SELECT MonsterOwnerId, ItemId, DropLimit, DropRateForGrade1, DropRateForGrade2, DropRateForGrade3, DropRateForGrade4, DropRateForGrade5, ProspectingLock, `Condition` FROM monsters_drops", r =>
        {
            int m = r.Int(0), o = r.Int(1);
            var taux = new[] { r.Dbl(3), r.Dbl(4), r.Dbl(5), r.Dbl(6), r.Dbl(7) };
            if (!butins.TryGetValue(m, out var l)) butins[m] = l = new List<object>();
            l.Add(new object[] { o, taux, r.Int(2), r.Int(8), r.Str(9) ?? "" });
            if (!sources.TryGetValue(o, out var s)) sources[o] = s = new List<object>();
            s.Add(new object[] { m, taux.Min(), taux.Max() });
        });

        var monstres = new List<object>();
        var resume = new List<object>();
        bdd.Lire("SELECT Id, NameId, RaceId, GfxId, IsBoss, MinDroppedKamas, MaxDroppedKamas, UseSummonSlot, AllIdolsDisabled, IncompatibleChallengesCSV, IsActive FROM monsters_templates ORDER BY Id", r =>
        {
            var id = r.Int(0);
            var nom = T(r.Int(1));
            if (string.IsNullOrEmpty(nom)) return;
            grades.TryGetValue(id, out var g);
            var niveaux = g == null ? new List<int>() : g.Select(x => (int)((List<object>)((object[])x)[0])[1]).ToList();
            var z = zonesMonstre.TryGetValue(id, out var zz) ? zz.ToList() : new List<int>();
            resume.Add(new object[] { id, nom, r.Int(2), niveaux.Count > 0 ? niveaux.Min() : 0, niveaux.Count > 0 ? niveaux.Max() : 0, z });
            monstres.Add(new object[] { id, nom, r.Int(2), r.Int(3), r.Int(4), new[] { r.Int(5), r.Int(6) }, r.Int(7), r.Int(8), Csv(r.Str(9)), z,
                                         g ?? new List<object>(), butins.TryGetValue(id, out var b) ? b : new List<object>() });
        });
        Ecrire(Path.Combine(sortie, "sources.json"), new Dictionary<string, object>
        {
            { "v", 1 }, { "sousZones", sousZones }, { "monstres", resume }, { "butins", sources.ToDictionary(k => k.Key.ToString(), k => (object)k.Value) }
        });
        Ecrire(Path.Combine(sortie, "bestiaire.json"), new Dictionary<string, object>
        {
            { "v", 1 }, { "champs", champs }, { "races", races }, { "sorts", nomsSorts.Where(s => s.Value.Length > 0).ToDictionary(k => k.Key.ToString(), k => (object)k.Value) },
            { "monstres", monstres }
        });

        // liste des icônes utilisées (pour l'extraction depuis le client)
        File.WriteAllLines(Path.Combine(sortie, "icones_objets.txt"), objets.Select(o => o["i"].ToString()).Distinct());
        File.WriteAllLines(Path.Combine(sortie, "icones_monstres.txt"), monstres.Select(m => ((object[])m)[0].ToString()).Distinct());
        Console.WriteLine("Monstres : " + monstres.Count + " · panoplies : " + panos.Count + " · recettes : " + recettes.Count + " · " + (DateTime.Now - debut).TotalSeconds.ToString("0.0") + " s");
        return echecs > 50 ? 2 : 0;
    }

    // ---------------------------------------------------------------- effets

    static object Lire(byte[] bin)
    {
        var f = new BinaryFormatter(null, new StreamingContext(StreamingContextStates.All)) { AssemblyFormat = FormatterAssemblyStyle.Simple, Binder = new SansVersion() };
        using (var m = new MemoryStream(bin)) return f.Deserialize(m);
    }

    /// <summary>Retrouve les types sérialisés quelle que soit la version des DLL du serveur.</summary>
    class SansVersion : SerializationBinder
    {
        static readonly Dictionary<string, Assembly> s_charges = new Dictionary<string, Assembly>();

        static Assembly Charger(AssemblyName n)
        {
            if (s_charges.TryGetValue(n.Name, out var a)) return a;
            a = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.GetName().Name == n.Name);
            if (a == null)
            {
                var f = Path.Combine(s_serveur, n.Name + ".dll");
                a = File.Exists(f) ? Assembly.UnsafeLoadFrom(f) : Assembly.Load(n.Name);
            }
            return s_charges[n.Name] = a;
        }

        public override Type BindToType(string assemblyName, string typeName) =>
            Type.GetType(typeName + ", " + assemblyName, Charger, (a, n, ic) => a == null ? Type.GetType(n, false, ic) : a.GetType(n, false, ic), true);
    }

    static object Champ(object o, string nom)
    {
        for (var t = o.GetType(); t != null; t = t.BaseType)
        {
            var f = t.GetField(nom, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            if (f != null) return f.GetValue(o);
            var p = t.GetProperty(nom, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            if (p != null && p.GetIndexParameters().Length == 0) return p.GetValue(o);
        }
        return null;
    }

    static int Entier(object o, string nom) { var v = Champ(o, nom); return v == null ? 0 : Convert.ToInt32(v); }

    /// <summary>Effets d'un objet : [effet, min, max] (+ valeur si texte / créature).</summary>
    static List<object> EffetsObjet(byte[] bin)
    {
        var l = new List<object>();
        if (bin == null || bin.Length == 0) return l;
        foreach (var e in (IEnumerable)Lire(bin))
        {
            if (e == null) continue;
            int id = Entier(e, "effectId"), min, max;
            var nom = e.GetType().Name;
            if (nom.Contains("Dice")) { min = Entier(e, "diceNum"); max = Entier(e, "diceSide"); var v = Entier(e, "value"); if (min == 0 && max == 0) min = v; }
            else if (nom.Contains("MinMax")) { min = Entier(e, "min"); max = Entier(e, "max"); }
            else if (nom.Contains("Integer")) { min = Entier(e, "value"); max = 0; }
            else { min = 0; max = 0; }
            if (max < min) max = 0;
            l.Add(new[] { id, min, max });
        }
        return l;
    }

    /// <summary>Bonus d'une panoplie : pour chaque nombre de pièces (index = pièces - 1), [[effet, valeur]…].</summary>
    static List<object> EffetsPanoplie(byte[] bin)
    {
        var l = new List<object>();
        if (bin == null || bin.Length == 0) return l;
        foreach (var palier in (IEnumerable)Lire(bin))
        {
            var p = new List<object>();
            if (palier != null)
                foreach (var e in (IEnumerable)palier)
                {
                    if (e == null) continue;
                    var id = Entier(e, "m_id");
                    if (id == 0) id = Entier(e, "Id");
                    // EffectDice hérite d'EffectInteger : la valeur peut être dans m_value, m_dicenum ou m_diceface
                    var v = Entier(e, "m_value");
                    if (v == 0) v = Entier(e, "m_dicenum");
                    if (v == 0) v = Entier(e, "m_diceface");
                    if (v == 0) v = Entier(e, "m_maxvalue");
                    p.Add(new[] { id, v });
                }
            l.Add(p);
        }
        return l;
    }

    // ---------------------------------------------------------------- outils

    static List<int> Csv(string s)
    {
        var l = new List<int>();
        if (string.IsNullOrEmpty(s)) return l;
        foreach (var x in s.Split(','))
            if (int.TryParse(x.Trim(), out var v)) l.Add(v);
        return l;
    }

    /// <summary>« 0,1|100,2|200,3 » -> [[0,1],[100,2],[200,3]] (coût d'un point de carac par palier).</summary>
    static List<int[]> Paliers(string s)
    {
        var l = new List<int[]>();
        if (string.IsNullOrEmpty(s)) return l;
        foreach (var p in s.Split('|'))
        {
            var x = p.Split(',');
            if (x.Length == 2 && int.TryParse(x[0], out var a) && int.TryParse(x[1], out var b)) l.Add(new[] { a, b });
        }
        return l;
    }

    static void Ecrire(string fichier, object o)
    {
        var js = new JavaScriptSerializer { MaxJsonLength = int.MaxValue, RecursionLimit = 200 };
        File.WriteAllText(fichier, js.Serialize(o), new UTF8Encoding(false));
        Console.WriteLine("  " + Path.GetFileName(fichier) + " : " + (new FileInfo(fichier).Length / 1024) + " Ko");
    }

    /// <summary>Accès MySQL par réflexion (MySql.Data du serveur), en lecture seule.</summary>
    class Bdd
    {
        readonly string m_connexion;
        readonly Type m_typeConnexion, m_typeCommande;

        public Bdd(string connexion)
        {
            m_connexion = connexion;
            var a = Assembly.UnsafeLoadFrom(Path.Combine(s_serveur, "MySql.Data.dll"));
            m_typeConnexion = a.GetType("MySql.Data.MySqlClient.MySqlConnection");
            m_typeCommande = a.GetType("MySql.Data.MySqlClient.MySqlCommand");
        }

        public void Lire(string sql, Action<Ligne> action)
        {
            using (var c = (IDisposable)Activator.CreateInstance(m_typeConnexion, m_connexion))
            {
                m_typeConnexion.GetMethod("Open", Type.EmptyTypes).Invoke(c, null);
                var cmd = (System.Data.IDbCommand)Activator.CreateInstance(m_typeCommande, sql, c);
                cmd.CommandTimeout = 600;
                using (var r = cmd.ExecuteReader())
                {
                    var l = new Ligne(r);
                    while (r.Read()) action(l);
                }
            }
        }
    }

    class Ligne
    {
        readonly System.Data.IDataReader m_r;
        public Ligne(System.Data.IDataReader r) { m_r = r; }
        public int Int(int i) => m_r.IsDBNull(i) ? 0 : Convert.ToInt32(m_r.GetValue(i), CultureInfo.InvariantCulture);
        public double Dbl(int i) => m_r.IsDBNull(i) ? 0 : Convert.ToDouble(m_r.GetValue(i), CultureInfo.InvariantCulture);
        public string Str(int i) => m_r.IsDBNull(i) ? null : Convert.ToString(m_r.GetValue(i), CultureInfo.InvariantCulture);
        public byte[] Bytes(int i) => m_r.IsDBNull(i) ? null : (byte[])m_r.GetValue(i);
    }
}
