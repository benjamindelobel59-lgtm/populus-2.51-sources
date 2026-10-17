using Stump.Core.Attributes;
using Stump.Core.Extensions;
using Stump.Core.Reflection;
using Stump.Core.Threading;
using Stump.Core.Xml;
using Stump.Server.BaseServer.Initialization;
using Stump.Server.WorldServer.Database.World;
using Stump.Server.WorldServer.Database.World.Maps;
using Stump.Server.WorldServer.Game.Maps.Cells;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stump.Server.WorldServer.Game.Maps.Placements
{
    public class PlacementManager : Singleton<PlacementManager>
    {
        [Variable]
        public static string PatternsDir = "./patterns";

        [Variable]
        public static byte SearchDeep = 5;

        [Variable]
        public static bool SortByComplexity = true;

        private List<PlacementPattern> m_patterns = new List<PlacementPattern>();
        private List<PlacementPattern> m_relativePatterns = new List<PlacementPattern>();

        [Initialization(InitializationPass.Fourth)]
        public void Initialize()
        {
            List<PlacementPattern> temp_patterns = new List<PlacementPattern>();

            foreach (var file in Directory.EnumerateFiles(PatternsDir, "*.xml", SearchOption.AllDirectories)){
                var pattern = XmlUtils.Deserialize<PlacementPattern>(file);

                if (SortByComplexity)
                {
                    var calc = new PlacementComplexityCalculator(pattern.Blues.Concat(pattern.Reds).ToArray());
                    pattern.Complexity = calc.Compute();
                }

                temp_patterns.Add(pattern);
            }

            var fixPatternsComplx = temp_patterns.Where(x => !x.Relative).Select(x => x.Complexity).ToArray();
            m_patterns = temp_patterns.Where(x => !x.Relative).ShuffleWithProbabilities(fixPatternsComplx).ToList();

            fixPatternsComplx = temp_patterns.Where(x => x.Relative).Select(x => x.Complexity).ToArray();
            m_relativePatterns = temp_patterns.Where(x => x.Relative).ShuffleWithProbabilities(fixPatternsComplx).ToList();
        }

        

        #region NEKFEU : placements aleatoires

        private static readonly AsyncRandom m_random = new AsyncRandom();

        public static bool IsPlacementCell(Map map, Cell cell, ICollection<short> excluded)
            => cell.Walkable && !cell.NonWalkableDuringFight && !cell.NonWalkableDuringRP && !excluded.Contains(cell.Id);

        /// <summary>
        /// Tire un modele (patterns/*.xml) au hasard, avec rotation / symetrie / echange des equipes au hasard,
        /// et une position au hasard sur la map. Toutes les cases doivent etre praticables en combat (pas d'enclos, pas d'element interactif).
        /// </summary>
        public bool TryGetRandomPlacement(Map map, out Cell[] blue, out Cell[] red)
        {
            blue = red = null;
            var patterns = m_patterns.Concat(m_relativePatterns).ToArray();
            if (patterns.Length == 0)
                return false;

            var excluded = new HashSet<short>(map.GetInteractiveObjects().Where(x => x.Cell != null).Select(x => x.Cell.Id));
            var candidates = map.Cells.Where(x => IsPlacementCell(map, x, excluded)).ToArray();
            if (candidates.Length < 8)
                return false;
            if (candidates.Length < 16)
                return TryGetGeneratedPlacement(map, candidates, excluded, out blue, out red);

            // NEKFEU : ecart entre les equipes tire a chaque combat (forme du modele conservee) : 1/3 rapproche (2 a 6 cases
            // de moins, jamais au contact), 1/3 tel quel, 1/3 eloigne (3 a 8 cases de plus). Si ca ne rentre pas, on revient
            // progressivement vers l'ecart d'origine.
            var mode = m_random.Next(0, 3);
            var wanted = mode == 0 ? -m_random.Next(2, 7) : mode == 1 ? 0 : m_random.Next(3, 9);
            // passe 0 : modele complet ; passe 1 : modele tronque (cases sur un obstacle retirees, >= 8 cases par equipe
            // et >= la moitie du modele) pour les maps etroites
            for (var pass = 0; pass < 2; pass++)
            foreach (var pattern in patterns.Shuffle())
            {
                if (pattern.Blues == null || pattern.Reds == null || pattern.Blues.Length == 0 || pattern.Reds.Length == 0)
                    continue;

                var rotation = m_random.Next(0, 4);
                var mirror = m_random.Next(0, 2) == 1;
                var tries = pattern.Relative ? 25 : 1;

                // direction blues -> rouges du modele (apres rotation / symetrie), pour ecarter les equipes
                double ux = 0, uy = 0;
                if (pattern.Relative)
                {
                    var cb = Centroid(pattern.Blues, rotation, mirror);
                    var cr = Centroid(pattern.Reds, rotation, mirror);
                    ux = cr.Item1 - cb.Item1;
                    uy = cr.Item2 - cb.Item2;
                    var len = Math.Sqrt(ux * ux + uy * uy);
                    if (len < 0.5)
                    {
                        ux = rotation % 2 == 0 ? 1 : 0;
                        uy = 1 - ux;
                        len = 1;
                    }
                    ux /= len;
                    uy /= len;
                }

                for (var i = 0; i < tries; i++)
                {
                    var center = pattern.Relative ? new MapPoint(candidates[m_random.Next(0, candidates.Length)]) : null;
                    for (var spread = pattern.Relative ? wanted : 0; ; spread = spread > 0 ? Math.Max(0, spread - 2) : Math.Min(0, spread + 2))
                    {
                        int bx = (int)Math.Round(-ux * spread / 2), by = (int)Math.Round(-uy * spread / 2);
                        int rx = (int)Math.Round(ux * spread / 2), ry = (int)Math.Round(uy * spread / 2);
                        var b = Transform(map, pattern.Blues, center, rotation, mirror, bx, by, excluded, pass == 1);
                        var r = b == null ? null : Transform(map, pattern.Reds, center, rotation, mirror, rx, ry, excluded, pass == 1);
                        var ok = b != null && r != null && !b.Intersect(r).Any() &&
                                 (spread >= 0 || MinDistance(b, r) >= 2); // rapproche : jamais au contact
                        if (!ok)
                        {
                            if (spread == 0)
                                break;
                            continue;
                        }

                        if (m_random.Next(0, 2) == 1)
                        {
                            blue = r;
                            red = b;
                        }
                        else
                        {
                            blue = b;
                            red = r;
                        }
                        return true;
                    }
                }
            }

            // NEKFEU : aucun modele ne rentre -> deux blocs rectangulaires face a face, construits pour la map
            // (demande utilisateur : des formes propres comme nos modeles, plus de cases au hasard)
            return TryGetGeneratedPlacement(map, candidates, excluded, out blue, out red);
        }

        // tailles des blocs (largeur x hauteur sur la grille), du plus grand au plus petit
        private static readonly int[][] GeneratedBlocks = { new[] { 2, 4 }, new[] { 2, 3 }, new[] { 1, 4 }, new[] { 2, 2 } };

        /// <summary>
        /// NEKFEU : place deux blocs rectangulaires identiques face a face (ecart de 2 a 8 cases, orientation au hasard),
        /// toutes les cases praticables en combat (pas d'enclos, pas d'element interactif). Du plus grand bloc au plus petit.
        /// </summary>
        private static bool TryGetGeneratedPlacement(Map map, Cell[] candidates, ICollection<short> excluded, out Cell[] blue, out Cell[] red)
        {
            blue = red = null;
            foreach (var block in GeneratedBlocks)
            {
                int w = block[0], h = block[1];
                foreach (var gap in Enumerable.Range(2, 7).Shuffle())
                {
                    var blues = new List<System.Drawing.Point>();
                    var reds = new List<System.Drawing.Point>();
                    var shift = (2 * w + gap) / 2;
                    for (var x = 0; x < w; x++)
                        for (var y = 0; y < h; y++)
                        {
                            blues.Add(new System.Drawing.Point(x - shift, y - h / 2));
                            reds.Add(new System.Drawing.Point(w + gap + x - shift, y - h / 2));
                        }

                    for (var i = 0; i < 30; i++)
                    {
                        var center = new MapPoint(candidates[m_random.Next(0, candidates.Length)]);
                        var rotation = m_random.Next(0, 4);
                        var b = Transform(map, blues.ToArray(), center, rotation, false, 0, 0, excluded, false);
                        var r = b == null ? null : Transform(map, reds.ToArray(), center, rotation, false, 0, 0, excluded, false);
                        if (b == null || r == null || b.Intersect(r).Any())
                            continue;

                        if (m_random.Next(0, 2) == 1)
                        {
                            blue = r;
                            red = b;
                        }
                        else
                        {
                            blue = b;
                            red = r;
                        }
                        return true;
                    }
                }
            }

            return false;
        }

        private static uint MinDistance(Cell[] a, Cell[] b)
            => a.SelectMany(x => b.Select(y => new MapPoint(x).ManhattanDistanceTo(new MapPoint(y)))).Min();

        private static void Rotate(ref int x, ref int y, int rotation, bool mirror)
        {
            // rotation de 90 degres sur la grille en losange : (x, y) -> (-y, x)
            for (var k = 0; k < rotation; k++)
            {
                var t = x;
                x = -y;
                y = t;
            }
            if (mirror)
                x = -x;
        }

        private static Tuple<double, double> Centroid(System.Drawing.Point[] points, int rotation, bool mirror)
        {
            double sx = 0, sy = 0;
            foreach (var point in points)
            {
                int x = point.X, y = point.Y;
                Rotate(ref x, ref y, rotation, mirror);
                sx += x;
                sy += y;
            }
            return Tuple.Create(sx / points.Length, sy / points.Length);
        }

        private static Cell[] Transform(Map map, System.Drawing.Point[] points, MapPoint center, int rotation, bool mirror, int offsetX, int offsetY, ICollection<short> excluded, bool trim)
        {
            var result = new List<Cell>();
            foreach (var point in points)
            {
                int x = point.X, y = point.Y;
                if (center != null)
                {
                    Rotate(ref x, ref y, rotation, mirror);
                    x += center.X + offsetX;
                    y += center.Y + offsetY;
                }

                var cell = MapPoint.IsInMap(x, y) ? map.Cells[MapPoint.CoordToCellId(x, y)] : null;
                if (cell == null || !IsPlacementCell(map, cell, excluded) || result.Contains(cell))
                {
                    if (trim)
                        continue;
                    return null;
                }

                result.Add(cell);
            }

            if (trim && (result.Count < 8 || result.Count * 2 < points.Length))
                return null;

            return result.ToArray();
        }

        #endregion

        private static Cell[] GetCellsCircle(Cell centerCell, MapRecord map, int radius, int minradius)
        {
            var centerPoint = new MapPoint(centerCell);
            var result = new List<Cell>();

            if (radius == 0)
            {
                if (minradius == 0)
                    result.Add(centerCell);

                return result.ToArray();
            }

            int x = (int)(centerPoint.X - radius);
            int y = 0;
            int i = 0;
            int j = 1;
            while (x <= centerPoint.X + radius)
            {
                y = -i;

                while (y <= i)
                {
                    if (minradius == 0 || Math.Abs(centerPoint.X - x) + Math.Abs(y) >= minradius)
                        AddCellIfValid(x, y + centerPoint.Y, map, result);

                    y++;
                }

                if (i == radius)
                {
                    j = -j;
                }

                i = i + j;
                x++;
            }

            return result.ToArray();
        }

        private static void AddCellIfValid(int x, int y, MapRecord map, IList<Cell> container)
        {
            if (!MapPoint.IsInMap(x, y))
                return;

            container.Add(map.Cells[MapPoint.CoordToCellId(x, y)]);
        }
    }
}
