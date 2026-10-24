using System;
using System.Collections.Generic;
using System.Linq;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Monsters;
using Stump.Server.WorldServer.Game.Fights;

namespace Stump.Server.WorldServer.Game.Maps.Spawns
{
    /// <summary>
    /// MYSTIC 19/10 : monstres rares limités à N exemplaires vivants par sous-zone (demande utilisateur : « 1 max par zone ») :
    /// Leprechaun, Hell Mina, Muldos sauvages et Volkornes sauvages (une seule famille = une seule limite).
    /// Vérifié au tirage d'un nouveau groupe (Map.GenerateRandomMonsterGroup) et à la restauration des groupes sauvegardés.
    /// </summary>
    public static class MonstresLimites
    {
        /// <summary>
        /// Familles limitées : tous les monstres d'une famille comptent ensemble (ex. 1 Muldo sauvage par zone, toutes couleurs).
        /// Global = le maximum vaut pour TOUTES les sous-zones où la famille apparaît réunies (PADGREF 22/10, Roissingue : « 2 maximum
        /// dans les 2 zones »), sinon par sous-zone.
        /// </summary>
        /// PADGREF 24/10 : Delai = minutes avant qu'un monstre de la famille réapparaisse dans la zone après avoir été TUÉ par des joueurs
        /// (0 = aucun délai) ; enregistré en base (padgref_limites_reapparition) pour tenir après un redémarrage.
        static readonly (int[] Monstres, int Max, bool Global, int Delai)[] Familles =
        {
            (new[] { 1247 }, 1, false, 0),                        // Leprechaun
            (new[] { 107 }, 1, false, 0),                         // Hell Mina
            (new[] { 4434, 4435, 4436, 4437, 4438 }, 1, false, 0),// Muldos sauvages (Indigo, Ebène, Orchidée, Pourpre, Doré)
            (new[] { 5308, 5309, 5311, 5313 }, 1, false, 0),      // Volkornes sauvages (Orchidée, Indigo, Ebène, Pourpre)
            (new[] { 1057 }, 2, true, 0),                         // Roissingue : 2 en tout (Tourbières sans fond + nauséabonde)
            // PADGREF 23/10 (demande utilisateur, Pandala) : Pandore 1 max par zone (Bordure de Terrdala / d'Aerdala...), Bambouto Sacré
            // 3 max en tout dans ses zones
            (new[] { 537 }, 1, false, 0),                         // Pandore
            (new[] { 546 }, 3, true, 0),                          // Bambouto Sacré
            // PADGREF 24/10 (demande utilisateur) : Dopeul Dark Vlad (Village et Mine des Dopeuls) 1 max par zone, réapparition 3 h
            (new[] { 422 }, 1, false, 180),
            // PADGREF 24/10 (demande utilisateur) : Gardienne des égouts, 1 dans les égouts de Bonta (sous-zone 75) et 1 dans ceux de
            // Brâkmar (73), réapparition 10 min
            (new[] { 450 }, 1, false, 10),
        };

        /// <summary>PADGREF 24/10 (demande utilisateur : « les archimonstres, c'est 1 par zone par espèce, repop toutes les 4 h ») :
        /// chaque archimonstre (table padgref_archimonstres, SQL 155 = isMiniBoss des fichiers du jeu) est sa propre famille.</summary>
        public const int DelaiArchimonstres = 240;

        static readonly object s_verrou = new object();
        static Dictionary<int, (int[] Monstres, int Max, bool Global, int Delai)> s_parMonstre;
        static Dictionary<string, DateTime> s_reapparitions = new Dictionary<string, DateTime>();
        static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        static Stump.ORM.Database Base => WorldServer.Instance.DBAccessor.Database;

        static Dictionary<int, (int[] Monstres, int Max, bool Global, int Delai)> ParMonstre
        {
            get
            {
                lock (s_verrou)
                {
                    if (s_parMonstre != null)
                        return s_parMonstre;
                    var d = Familles.SelectMany(f => f.Monstres.Select(m => new KeyValuePair<int, (int[], int, bool, int)>(m, f))).ToDictionary(x => x.Key, x => x.Value);
                    try
                    {
                        Base.Execute("CREATE TABLE IF NOT EXISTS padgref_archimonstres (MonsterId INT NOT NULL PRIMARY KEY) DEFAULT CHARSET=utf8");
                        Base.Execute("CREATE TABLE IF NOT EXISTS padgref_limites_reapparition (Cle VARCHAR(64) NOT NULL PRIMARY KEY, Fin DATETIME NOT NULL) DEFAULT CHARSET=utf8");
                        foreach (var id in Base.Fetch<int>("SELECT MonsterId FROM padgref_archimonstres"))
                            if (!d.ContainsKey(id))
                                d[id] = (new[] { id }, 1, false, DelaiArchimonstres);
                        foreach (var ligne in Base.Fetch<string>("SELECT CONCAT(Cle, '|', DATE_FORMAT(Fin, '%Y-%m-%d %H:%i:%s')) FROM padgref_limites_reapparition WHERE Fin > NOW()"))
                        {
                            var p = ligne.Split('|');
                            s_reapparitions[p[0]] = DateTime.Parse(p[1]);
                        }
                        logger.Info("[Limites] {0} monstres limités (dont archimonstres), {1} délai(s) de réapparition en cours", d.Count, s_reapparitions.Count);
                    }
                    catch (Exception ex)
                    {
                        logger.Error("[Limites] lecture des archimonstres / délais impossible : {0}", ex.Message);
                    }
                    return s_parMonstre = d;
                }
            }
        }

        static string Cle((int[] Monstres, int Max, bool Global, int Delai) famille, Map map)
            => famille.Monstres[0] + ":" + (famille.Global ? "G" : (map.SubArea?.Id ?? map.Id).ToString());

        /// <summary>Appelé à la fin du combat d'un groupe : si les joueurs ont gagné, chaque famille à délai tuée repart pour son délai.</summary>
        public static void SurFinDeCombat(MonsterGroup groupe, IFight combat)
        {
            if (groupe?.Map == null || combat?.Winners == null || combat.Winners != combat.ChallengersTeam)
                return;
            var familles = groupe.GetMonsters().Where(m => ParMonstre.ContainsKey(m.Grade.MonsterId)).Select(m => ParMonstre[m.Grade.MonsterId])
                .Where(f => f.Delai > 0).Distinct().ToList();
            foreach (var f in familles)
            {
                var cle = Cle(f, groupe.Map);
                var fin = DateTime.Now.AddMinutes(f.Delai);
                lock (s_verrou)
                    s_reapparitions[cle] = fin;
                logger.Info("[Limites] monstre {0} tué (clé {1}) : pas de réapparition avant {2:HH:mm}", f.Monstres[0], cle, fin);
                WorldServer.Instance.IOTaskPool.AddMessage(() =>
                {
                    try { Base.Execute("REPLACE INTO padgref_limites_reapparition (Cle, Fin) VALUES (@0, @1)", cle, fin); }
                    catch (Exception ex) { logger.Error("[Limites] sauvegarde du délai {0} impossible : {1}", cle, ex.Message); }
                });
            }
        }

        /// <summary>Cartes où une famille « globale » peut apparaître (calculé une fois, après le chargement du monde).</summary>
        static readonly System.Collections.Concurrent.ConcurrentDictionary<int, Map[]> s_cartesGlobales =
            new System.Collections.Concurrent.ConcurrentDictionary<int, Map[]>();

        static IEnumerable<Map> Cartes(Map map, (int[] Monstres, int Max, bool Global, int Delai) famille)
        {
            if (!famille.Global)
                return map.SubArea != null ? map.SubArea.Maps : new[] { map };
            return s_cartesGlobales.GetOrAdd(famille.Monstres[0], _ =>
            {
                var ids = new HashSet<int>(famille.Monstres);
                var sousZones = new HashSet<int>(World.Instance.GetMaps()
                    .Where(m => m.SubArea != null && (m.MonsterSpawns.Any(x => ids.Contains(x.MonsterId)) || m.SubArea.MonsterSpawns.Any(x => ids.Contains(x.MonsterId))))
                    .Select(m => m.SubArea.Id));
                return World.Instance.GetMaps().Where(m => m.SubArea != null && sousZones.Contains(m.SubArea.Id)).ToArray();
            });
        }

        public static bool EstLimite(int monsterId) => ParMonstre.ContainsKey(monsterId);

        /// <summary>Vrai si la sous-zone de la map a déjà son maximum de ce monstre (en comptant aussi le groupe en cours de création).</summary>
        public static bool Atteinte(Map map, int monsterId, IEnumerable<int> dejaDansLeGroupe = null)
        {
            if (!ParMonstre.TryGetValue(monsterId, out var famille))
                return false;

            // PADGREF 24/10 : tué il y a moins que son délai -> pas encore de réapparition dans cette zone
            if (famille.Delai > 0)
                lock (s_verrou)
                    if (s_reapparitions.TryGetValue(Cle(famille, map), out var fin) && fin > DateTime.Now)
                        return true;

            var ids = new HashSet<int>(famille.Monstres);
            var n = dejaDansLeGroupe?.Count(x => ids.Contains(x)) ?? 0;
            foreach (var m in Cartes(map, famille))
                foreach (var g in m.GetActors<MonsterGroup>())
                    n += g.GetMonsters().Count(x => ids.Contains(x.Grade.MonsterId));

            return n >= famille.Max;
        }
    }
}
