using System;
using System.Collections.Generic;
using System.Linq;
using Stump.Core.Threading;
using Stump.Server.WorldServer.AI.Fights.Actions;
using Stump.Server.WorldServer.AI.Fights.Spells;
using Stump.Server.WorldServer.Database.World;
using Stump.Server.WorldServer.Game.Actors;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Maps.Cells;
using Stump.Server.WorldServer.Game.Maps.Cells.Shapes;
using Stump.Server.WorldServer.Game.Maps.Cells.Shapes.Set;
using Stump.Server.WorldServer.Game.Maps.Pathfinding;
using Spell = Stump.Server.WorldServer.Game.Spells.Spell;

namespace Stump.Server.WorldServer.AI.Fights.Brain
{
    public class EnvironmentAnalyser
    {
        private MapPoint[] m_moveZone;

        public EnvironmentAnalyser(AIFighter fighter)
        {
            Fighter = fighter;
            CellInformationProvider = new AIFightCellsInformationProvider(Fighter.Fight, Fighter);
            Pathfinding = new Pathfinder(CellInformationProvider);
            fighter.PositionChanged += OnPositionChanged;
        }

        public Pathfinder Pathfinding
        {
            get;
            private set;
        }

        public AIFightCellsInformationProvider CellInformationProvider
        {
            get;
            private set;
        }

        public AIFighter Fighter
        {
            get;
            private set;
        }

        public IFight Fight
        {
            get { return Fighter.Fight; }
        }

        public Cell GetFreeAdjacentCell()
        {
            var cell = Fighter.Position.Point.GetAdjacentCells(CellInformationProvider.IsCellWalkable).FirstOrDefault();

            return cell != null ? CellInformationProvider.GetCellInformation(cell.CellId).Cell : null;
        }

        private void OnPositionChanged(ContextActor arg1, ObjectPosition arg2)
        {
            ResetMoveZone();
        }

        public void ResetMoveZone()
        {
            m_moveZone = null;
        }

        public Cell GetCellToCastSpell(TargetCell target, Spell spell, bool LoS, bool nearFirst = true)
        {
            var moveZone = GetMoveZone();
            Set castRange;
            if (spell.CurrentSpellLevel.CastInLine || spell.CurrentSpellLevel.CastInDiagonal)
                castRange = new CrossSet(target.Point, Fighter.GetSpellRange(spell.CurrentSpellLevel),
                    spell.CurrentSpellLevel.MinRange != 0 ? (int) spell.CurrentSpellLevel.MinRange : CellInformationProvider.IsCellWalkable(target.Cell.Id) ? 0 : 1)
                {
                    Diagonal = spell.CurrentSpellLevel.CastInDiagonal,
                    AllDirections = spell.CurrentSpellLevel.CastInLine && spell.CurrentSpellLevel.CastInDiagonal
                };
            else
             castRange = new LozengeSet(target.Point, Fighter.GetSpellRange(spell.CurrentSpellLevel), 
                spell.CurrentSpellLevel.MinRange != 0 ? (int)spell.CurrentSpellLevel.MinRange : CellInformationProvider.IsCellWalkable(target.Cell.Id) ? 0 : 1);

            var intersection = castRange.EnumerateValidPoints().Intersect(moveZone);

            var closestPoint = intersection.Where(x => Fight.Cells[x.CellId].Walkable && 
                (target.Direction == DirectionFlagEnum.ALL_DIRECTIONS || target.Direction == DirectionFlagEnum.NONE || (x.OrientationTo(target.Point).GetFlag() & target.Direction) != 0) &&
                (!LoS || Fight.CanBeSeen(x, MapPoint.GetPoint(target.Cell), false, Fighter))).
                OrderBy(x => (nearFirst ? 1 : -1)*x.ManhattanDistanceTo(Fighter.Position.Point)).FirstOrDefault();

            return closestPoint != null ? Fighter.Fight.Cells[closestPoint.CellId] : null;
        }

        public IEnumerable<Cell> GetCellsWithLoS(Cell target)
        {
            return GetCellsWithLoS(target, new LozengeSet(Fighter.Position.Point, Fighter.MP));
        }

        public IEnumerable<Cell> GetCellsWithLoS(Cell target, Set searchZone)
        {            
            foreach (var cell in searchZone.EnumerateValidPoints())
            {
                if (Fight.CanBeSeen(cell, MapPoint.GetPoint(target)))
                    yield return Fight.Cells[cell.CellId];
            }
        }

        public MapPoint[] GetMoveZone()
        {
            return m_moveZone ?? (m_moveZone = Pathfinding.FindReachableCells(Fighter.Position.Point, Fighter.MP));
        }

        public bool TryToReach(MapPoint point, out Path path)
        {
            var dist = Fighter.Position.Point.ManhattanDistanceTo(point);

            // todo : take in account teleport spells
            if (dist > Fighter.MP)
            {
                path = null;
                return false;
            }

            path = Pathfinding.FindPath(Fighter.Position.Point, point, false, Fighter.MP);
            return path.EndCell.Id == point.CellId;
        }

        public Cell GetCellToFlee()
        {
            var rand = new AsyncRandom();
            var movementsCells = GetMovementCells();
            var fighters = Fight.GetAllFighters(entry => entry.IsEnnemyWith(Fighter));

            var currentCellIndice = fighters.Sum(entry => entry.Position.Point.ManhattanDistanceTo(Fighter.Position.Point)); 
            Cell betterCell = null;
            long betterCellIndice = 0;
            foreach (var c in movementsCells)
            {
                if (!CellInformationProvider.IsCellWalkable(c.Id))
                    continue;

                var indice = fighters.Sum(entry => entry.Position.Point.ManhattanDistanceTo(new MapPoint(c)));

                if (betterCellIndice < indice)
                {
                    betterCellIndice = indice;
                    betterCell = c;
                }
                else if (betterCellIndice == indice && rand.Next(2) == 0)
                    // random factory
                {
                    betterCellIndice = indice;
                    betterCell = c;
                }
            }

            return currentCellIndice == betterCellIndice ? Fighter.Cell : betterCell;
        }

        public Cell[] GetMovementCells()
        {
            return GetMovementCells(Fighter.MP);
        }

        public Cell[] GetMovementCells(int mp)
        {
            if (mp <= 0)
                return new Cell[0];

            if (mp > 63)
                return Fight.Map.Cells;

            var circle = new Lozenge(0, (byte) mp);

            return circle.GetCells(Fighter.Cell, Fight.Map);
        }

        public FightActor GetNearestFighter()
        {
            return GetNearestFighter(entry => true);
        }

        public FightActor GetNearestAlly()
        {
            return GetNearestFighter(entry => entry.IsFriendlyWith(Fighter) && entry != Fighter);
        }

        public FightActor GetNearestEnemy()
        {
            // MYSTIC : une invocation de joueur vise d'abord les monstres / personnages, les invocations adverses seulement s'il n'y a rien d'autre
            if (IsPlayerSummon(Fighter))
                return GetNearestFighter(entry => entry.IsEnnemyWith(Fighter) && !(entry is SummonedFighter)) ?? GetNearestFighter(entry => entry.IsEnnemyWith(Fighter));

            return GetNearestFighter(entry => entry.IsEnnemyWith(Fighter));
        }

        /// <summary>MYSTIC : invocation (directe ou invoquee par une invocation) d'un personnage joueur.</summary>
        public static bool IsPlayerSummon(FightActor fighter)
        {
            var current = fighter as SummonedFighter;
            for (var i = 0; current != null && i < 5; i++)
            {
                if (current.Summoner is CharacterFighter)
                    return true;
                current = current.Summoner as SummonedFighter;
            }
            return false;
        }

        public FightActor GetNearestFighter(Predicate<FightActor> predicate)
        {
            return Fight.GetAllFighters(entry => predicate(entry) && Fighter.CanSee(entry)).
                OrderBy(entry => entry.Position.Point.ManhattanDistanceTo(Fighter.Position.Point)).FirstOrDefault();
        }

        public List<FightActor> GetNearestFighters(Predicate<FightActor> predicate)
        {
            return Fight.GetAllFighters(entry => predicate(entry) && Fighter.CanSee(entry)).
                OrderBy(entry => entry.Position.Point.ManhattanDistanceTo(Fighter.Position.Point)).ToList();
        }

        public IEnumerable<FightActor> GetVisibleEnemies()
        {
            return Fighter.OpposedTeam.GetAllFighters(entry => entry.IsVisibleFor(Fighter));
        }

        /// <summary>MYSTIC 18/10 : il reste des ennemis vivants mais aucun n'est visible (tous invisibles).</summary>
        public bool AucunEnnemiVisible() =>
            Fighter.OpposedTeam.GetAllFighters(x => x.IsAlive()).Any() &&
            !Fighter.OpposedTeam.GetAllFighters(x => x.IsAlive() && x.IsVisibleFor(Fighter)).Any();

        /// <summary>MYSTIC 18/10 : cases où un invisible peut être (autour de sa dernière case connue ; le rayon grandit avec le temps).</summary>
        public List<Cell> GetCasesPlausiblesInvisibles(out Cell plusProche)
        {
            var round = Fight.TimeLine.RoundNumber;
            var cases = new HashSet<Cell>();
            plusProche = null;
            var dMin = int.MaxValue;
            foreach (var ennemi in Fighter.OpposedTeam.GetAllFighters(x => x.IsAlive() && !x.IsVisibleFor(Fighter)).ToArray())
            {
                var s = MemoireInvisibles.Obtenir(ennemi);
                if (s == null)
                    continue;
                var centre = Fight.Map.Cells[s.Case];
                var d = (int)new MapPoint(centre).ManhattanDistanceTo(Fighter.Position.Point);
                if (d < dMin)
                {
                    dMin = d;
                    plusProche = centre;
                }
                var rayon = Math.Min(6, 2 + 2 * Math.Max(0, round - s.Round));
                foreach (var c in new Lozenge(0, (byte)rayon).GetCells(centre, Fight.Map))
                {
                    if (!c.Walkable || c.NonWalkableDuringFight)
                        continue;
                    var occupant = Fight.GetOneFighter(c);
                    if (occupant != null && occupant.IsVisibleFor(Fighter))
                        continue;
                    cases.Add(c);
                }
            }
            return cases.ToList();
        }
    }

    /// <summary>MYSTIC 18/10 : dernière case connue de chaque combattant (vu par l'IA, devenu invisible, ou détecté en lançant un sort).</summary>
    public static class MemoireInvisibles
    {
        public class Souvenir
        {
            public short Case;
            public int Round;
        }

        static readonly System.Runtime.CompilerServices.ConditionalWeakTable<IFight, Dictionary<int, Souvenir>> s_combats =
            new System.Runtime.CompilerServices.ConditionalWeakTable<IFight, Dictionary<int, Souvenir>>();

        public static void Noter(FightActor actor)
        {
            if (actor?.Fight == null || actor.Cell == null || actor.IsDead())
                return;
            var d = s_combats.GetOrCreateValue(actor.Fight);
            lock (d)
                d[actor.Id] = new Souvenir { Case = actor.Cell.Id, Round = actor.Fight.TimeLine.RoundNumber };
        }

        public static void NoterVisibles(FightActor observateur)
        {
            foreach (var e in observateur.OpposedTeam.GetAllFighters(x => x.IsAlive() && x.IsVisibleFor(observateur)).ToArray())
                Noter(e);
        }

        public static Souvenir Obtenir(FightActor actor)
        {
            if (actor?.Fight == null || !s_combats.TryGetValue(actor.Fight, out var d))
                return null;
            lock (d)
                return d.TryGetValue(actor.Id, out var s) ? s : null;
        }
    }
}