using System.Drawing;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Types;
using Stump.Server.WorldServer.Database;
using Stump.Server.WorldServer.Database.World;
using Stump.Server.WorldServer.Game.Maps.Cells.Shapes;
using System.Collections.Generic;
using System.Linq;

namespace Stump.Server.WorldServer.Game.Fights.Triggers
{
    public class MarkShape
    {
        readonly Zone m_zone;
        bool m_customForm;
        Cell[] m_cells;

        public MarkShape(IFight fight, Cell cell, SpellShapeEnum spellShape, GameActionMarkCellsTypeEnum shape, byte minSize, byte size, Color color)
        {
            Fight = fight;
            Cell = cell;
            Shape = shape;
            Size = size;
            MinSize = minSize;
            Color = color;

            m_zone = new Zone(spellShape, size, MinSize);
            CheckCells(m_zone.GetCells(Cell, fight.Map));
        }

        public MarkShape(IFight fight, Cell cell, GameActionMarkCellsTypeEnum shape, byte minSize, byte size, Color color)
            : this(fight, cell, SpellShapeEnum.C, shape, minSize, size, color)
        {
        }

        public IFight Fight
        {
            get;
        }

        public Cell Cell
        {
            get;
        }

        public GameActionMarkCellsTypeEnum Shape
        {
            get;
        }

        public byte Size
        {
            get;
        }

        public byte MinSize
        {
            get;
        }

        public Color Color
        {
            get;
        }

        public Cell[] GetCells() => m_cells;

        /// <summary>PADGREF 23/10 (Étoile de Mer de Bethel : -200 PA) : marque posée sur une liste de cases fixes (zone « ;a,b,c… ») :
        /// UNE seule marque sur toutes ces cases (avant : une glyphe par case, 53 auras cumulées).</summary>
        public void RemplacerCases(IEnumerable<Cell> cases)
        {
            m_cells = cases.Where(c => c != null && c.Walkable).Distinct().ToArray();
            m_customForm = true;
        }

        public GameActionMarkedCell[] GetGameActionMarkedCells()
        {
            if (!m_customForm && m_cells.Length > 1 && MinSize == 0 && Shape == GameActionMarkCellsTypeEnum.CELLS_CIRCLE || Shape == GameActionMarkCellsTypeEnum.CELLS_CROSS)
                return new[] {new GameActionMarkedCell((ushort)Cell.Id, (sbyte)Size, Color.ToArgb() & 0xFFFFFF, (sbyte) Shape)};
            
            return m_cells.Select(x => new GameActionMarkedCell((ushort)x.Id, 0, Color.ToArgb() & 0xFFFFFF, (sbyte)Shape)).ToArray();
        }

        void CheckCells(Cell[] cells)
        {
            var validCells = new List<Cell>();
            foreach (var cell in cells)
            {
                if (cell.Walkable)
                    validCells.Add(cell);
                else
                    m_customForm = true;
            }

            m_cells = validCells.ToArray();

            if (m_zone.ShapeType != SpellShapeEnum.C && m_zone.ShapeType != SpellShapeEnum.X)
                m_customForm = true;
        }
    }
}