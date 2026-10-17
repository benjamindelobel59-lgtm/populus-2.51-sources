using System;
using System.Collections.Generic;
using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Database.World;

namespace Stump.Server.WorldServer.Game.Maps.Cells.Shapes
{
    /// <summary>
    /// MYSTIC 17/10 : zone « Z » du client 2.51 (SpellZone.fillReversedTrueCircleCells) : toutes les cases de la carte dont la
    /// distance euclidienne au centre est supérieure ou égale au rayon (ex. Tachi-Ai de la Gorgouille : glyphe partout sauf
    /// autour d'elle). Avant : forme inconnue, rien n'était touché.
    /// </summary>
    public class CercleInverse : IShape
    {
        public CercleInverse(byte radius)
        {
            Radius = radius;
        }

        public uint Surface => 560;

        public byte MinRadius
        {
            get;
            set;
        }

        public DirectionsEnum Direction
        {
            get;
            set;
        }

        public byte Radius
        {
            get;
            set;
        }

        public Cell[] GetCells(Cell centerCell, Map map)
        {
            var centre = new MapPoint(centerCell);
            var cases = new List<Cell>();

            foreach (var cell in map.Cells)
            {
                var point = new MapPoint(cell);
                int dx = point.X - centre.X, dy = point.Y - centre.Y;
                if (Math.Sqrt(dx * dx + dy * dy) >= Radius)
                    cases.Add(cell);
            }

            return cases.ToArray();
        }
    }
}
