using System;
using System.Collections.Generic;
using System.Linq;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Fights.Teams;

namespace Stump.Server.WorldServer.Game.Maps.Spawns
{
    /// <summary>
    /// PADGREF 24/10 (demande utilisateur : « plus aucun placement aléatoire dans les salles de boss, toujours le même placement, même en
    /// relançant le combat ») : cases fixes des monstres par salle de donjon, table padgref_placements_fixes (SQL 154) remplie depuis
    /// PLANCHE_PLACEMENTS.html. Lignes dans l'ordre MODULAIRE (groupe des paliers 1-4, puis le monstre ajouté au palier 5, 6…) : chaque
    /// ligne prend le premier monstre de ce type pas encore placé, donc un palier plus petit reprend exactement les cases de ses lignes.
    /// CellId -1 (case non donnée) ou case impraticable : ce monstre garde le placement normal (case bleue libre).
    /// </summary>
    public static class PlacementsFixes
    {
        static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        static readonly object s_verrou = new object();
        static Dictionary<int, List<KeyValuePair<int, int>>> s_salles;

        static Stump.ORM.Database Base => WorldServer.Instance.DBAccessor.Database;

        static Dictionary<int, List<KeyValuePair<int, int>>> Salles
        {
            get
            {
                lock (s_verrou)
                {
                    if (s_salles != null)
                        return s_salles;

                    s_salles = new Dictionary<int, List<KeyValuePair<int, int>>>();
                    try
                    {
                        Base.Execute("CREATE TABLE IF NOT EXISTS padgref_placements_fixes (DungeonSpawnId INT NOT NULL, Rang INT NOT NULL, " +
                                     "MonsterId INT NOT NULL, CellId INT NOT NULL, PRIMARY KEY (DungeonSpawnId, Rang)) DEFAULT CHARSET=utf8");
                        foreach (var ligne in Base.Fetch<string>("SELECT CONCAT(DungeonSpawnId, ';', MonsterId, ';', CellId) FROM padgref_placements_fixes " +
                                                                 "ORDER BY DungeonSpawnId, Rang"))
                        {
                            var p = ligne.Split(';');
                            if (!s_salles.TryGetValue(int.Parse(p[0]), out var liste))
                                s_salles[int.Parse(p[0])] = liste = new List<KeyValuePair<int, int>>();
                            liste.Add(new KeyValuePair<int, int>(int.Parse(p[1]), int.Parse(p[2])));
                        }
                        logger.Info("[Placement fixe] {0} salle(s) chargée(s)", s_salles.Count);
                    }
                    catch (Exception ex)
                    {
                        logger.Error("[Placement fixe] lecture de padgref_placements_fixes impossible : {0}", ex.Message);
                    }
                    return s_salles;
                }
            }
        }

        /// <summary>Relit la table au prochain combat (après un nouveau SQL de placements).</summary>
        public static void Recharger()
        {
            lock (s_verrou)
                s_salles = null;
        }

        /// <summary>Place l'équipe de monstres sur ses cases fixes ; false si la salle n'a pas de placement fixe (placement normal).</summary>
        public static bool Placer(IFight fight, FightTeam team)
        {
            if (!(team is FightMonsterTeam))
                return false;

            var monstres = team.GetAllFighters().OfType<MonsterFighter>().ToList();
            var groupe = monstres.FirstOrDefault()?.Monster?.Group;
            var spawnId = (groupe?.SpawningPool as DungeonSpawningPool)?.SpawnIdDe(groupe);
            if (spawnId == null || !Salles.TryGetValue(spawnId.Value, out var lignes))
                return false;

            var adverse = fight.GetAllFighters(x => x.Team != team).ToList();
            var prises = new HashSet<int>(adverse.Select(x => (int)x.Cell.Id));
            var restants = new List<MonsterFighter>(monstres);
            var normaux = new List<MonsterFighter>();
            var places = 0;

            foreach (var ligne in lignes)
            {
                var monstre = restants.FirstOrDefault(x => x.Monster.Template.Id == ligne.Key);
                if (monstre == null)
                    continue;
                restants.Remove(monstre);

                var cell = ligne.Value >= 0 && ligne.Value < fight.Map.Cells.Length ? fight.Map.Cells[ligne.Value] : null;
                if (cell == null || !cell.Walkable || cell.NonWalkableDuringFight || prises.Contains(cell.Id) && !adverse.Any(x => x.Cell.Id == cell.Id))
                {
                    if (ligne.Value >= 0)
                        logger.Warn("[Placement fixe] spawn {0} : case {1} inutilisable pour {2}, placement normal", spawnId, ligne.Value, monstre);
                    normaux.Add(monstre);
                    continue;
                }

                // un joueur déjà posé sur cette case (case rouge) : il passe sur une autre case libre de son équipe
                var joueur = adverse.FirstOrDefault(x => x.Cell.Id == cell.Id);
                if (joueur != null)
                {
                    var libre = joueur.Team.PlacementCells.FirstOrDefault(c => !prises.Contains(c.Id) && c.Id != cell.Id &&
                                                                               !lignes.Any(l => l.Value == c.Id));
                    if (libre == null)
                    {
                        normaux.Add(monstre);
                        continue;
                    }
                    joueur.ForcerPrePlacement(libre);
                    prises.Remove(cell.Id);
                    prises.Add(libre.Id);
                }

                monstre.ForcerPrePlacement(cell);
                prises.Add(cell.Id);
                places++;
            }

            // monstres sans case donnée : cases bleues libres, comme avant
            var hasard = new Random();
            foreach (var monstre in normaux.Concat(restants))
            {
                var libres = team.PlacementCells.Where(c => !prises.Contains(c.Id)).ToArray();
                if (libres.Length == 0)
                    break;
                var cell = libres[hasard.Next(libres.Length)];
                monstre.ForcerPrePlacement(cell);
                prises.Add(cell.Id);
            }

            logger.Info("[Placement fixe] spawn {0} (map {1}) : {2} monstre(s) sur case fixe, {3} placement(s) normal(aux)",
                spawnId, fight.Map.Id, places, normaux.Count + restants.Count);
            return true;
        }
    }
}
