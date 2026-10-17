using System;
using System.Collections.Generic;
using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Database.World;

namespace Stump.Server.WorldServer.Game.Maps.Cells.Shapes
{
    /// <summary>
    /// MYSTIC 17/10 : zone « D » du client 2.51 (SpellZone.fillCheckerboard) : damier. Cases à une distance (en cases) comprise
    /// entre le rayon minimum et le rayon, de même parité que le rayon : D60 = distance paire, D61 = distance impaire
    /// (glyphes paire / impaire du Disciple du Kimbo). Avant : traité comme une croix (une ligne).
    /// </summary>
    public class Damier : IShape
    {
        public Damier(byte minRadius, byte radius)
        {
            MinRadius = minRadius;
            Radius = radius;
        }

        public uint Surface => 560;

        public byte MinRadius { get; set; }

        public DirectionsEnum Direction { get; set; }

        public byte Radius { get; set; }

        public Cell[] GetCells(Cell centerCell, Map map)
        {
            var centre = new MapPoint(centerCell);
            var pair = Radius % 2 == 0;
            var cases = new List<Cell>();

            for (var dx = -Radius; dx <= Radius; dx++)
                for (var dy = -Radius; dy <= Radius; dy++)
                {
                    var distance = Math.Abs(dx) + Math.Abs(dy);
                    if (distance > Radius || distance < MinRadius)
                        continue;

                    // même formule que le client (le reste garde le signe, comme en AS3)
                    if (pair ? (dx + dy % 2) % 2 != 0 : (dx + 1 + dy % 2) % 2 != 0)
                        continue;

                    int x = centre.X + dx, y = centre.Y + dy;
                    if (!MapPoint.IsInMap(x, y))
                        continue;

                    cases.Add(map.Cells[MapPoint.CoordToCellId(x, y)]);
                }

            return cases.ToArray();
        }
    }
}
