using System.Collections.Generic;
using System.Linq;
using Stump.Server.WorldServer.Game.Maps.Cells;
using Stump.Server.WorldServer.Game.Actors.Fight;
using TreeSharp;

namespace Stump.Server.WorldServer.AI.Fights.Actions
{
    /// <summary>MYSTIC 18/10 : se rapprocher d'une CASE (dernière case connue d'un invisible), selon la vraie distance à pied.</summary>
    public class MoveNearCell : AIAction
    {
        public MoveNearCell(AIFighter fighter, Database.World.Cell cible)
            : base(fighter)
        {
            Cible = cible;
        }

        public Database.World.Cell Cible { get; }

        protected override RunStatus Run(object context)
        {
            if (!Fighter.CanMove() || Cible == null)
                return RunStatus.Failure;
            var info = new AIFightCellsInformationProvider(Fighter.Fight, Fighter);

            var distCible = new Dictionary<short, int> { [Cible.Id] = 0 };
            var file = new Queue<MapPoint>();
            file.Enqueue(new MapPoint(Cible));
            while (file.Count > 0)
            {
                var p = file.Dequeue();
                var d = distCible[(short)p.CellId];
                foreach (var n in p.GetAdjacentCells(info.IsCellWalkable))
                    if (!distCible.ContainsKey((short)n.CellId))
                    {
                        distCible[(short)n.CellId] = d + 1;
                        file.Enqueue(n);
                    }
            }

            var cout = new Dictionary<short, int> { [Fighter.Cell.Id] = 0 };
            file.Enqueue(Fighter.Position.Point);
            while (file.Count > 0)
            {
                var p = file.Dequeue();
                var d = cout[(short)p.CellId];
                if (d >= Fighter.MP)
                    continue;
                foreach (var n in p.GetAdjacentCells(info.IsCellWalkable))
                    if (!cout.ContainsKey((short)n.CellId))
                    {
                        cout[(short)n.CellId] = d + 1;
                        file.Enqueue(n);
                    }
            }

            var cibleP = new MapPoint(Cible);
            var atteignables = cout.Keys.Where(distCible.ContainsKey).ToList();
            var meilleure = atteignables.Count > 0
                ? atteignables.OrderBy(c => distCible[c]).ThenBy(c => cout[c]).First()
                : cout.Keys.OrderBy(c => new MapPoint(Fighter.Fight.Map.Cells[c]).ManhattanDistanceTo(cibleP)).First();
            if (meilleure == Fighter.Cell.Id)
                return RunStatus.Failure;
            return new MoveAction(Fighter, Fighter.Fight.Map.Cells[meilleure]).YieldExecute(context);
        }
    }

    public class MoveNearTo : AIAction
    {
        public MoveNearTo(AIFighter fighter, FightActor target)
            : base(fighter)
        {
            Target = target;
        }

        public FightActor Target
        {
            get;
            private set;
        }

        protected override RunStatus Run(object context)
        {
            if (!Fighter.CanMove())
                return RunStatus.Failure;

            if (Target == null)
                return RunStatus.Failure;

            var cellInfoProvider = new AIFightCellsInformationProvider(Fighter.Fight, Fighter);

            if (Fighter.Position.Point.IsAdjacentTo(Target.Position.Point))
                return RunStatus.Success;

            // MYSTIC 18/10 : la case visée était choisie « à vol d'oiseau » (case derrière un mur, ou case inaccessible : le monstre
            // avançait tout droit puis gardait ses PM). Maintenant : vraie distance à pied jusqu'à la cible (parcours depuis elle),
            // et case la plus proche de la cible que le monstre peut atteindre avec ses PM.
            var distCible = new Dictionary<short, int> { [Target.Cell.Id] = 0 };
            var file = new Queue<MapPoint>();
            file.Enqueue(Target.Position.Point);
            while (file.Count > 0)
            {
                var p = file.Dequeue();
                var d = distCible[(short)p.CellId];
                foreach (var n in p.GetAdjacentCells(cellInfoProvider.IsCellWalkable))
                    if (!distCible.ContainsKey((short)n.CellId))
                    {
                        distCible[(short)n.CellId] = d + 1;
                        file.Enqueue(n);
                    }
            }

            var cout = new Dictionary<short, int> { [Fighter.Cell.Id] = 0 };
            file.Enqueue(Fighter.Position.Point);
            while (file.Count > 0)
            {
                var p = file.Dequeue();
                var d = cout[(short)p.CellId];
                if (d >= Fighter.MP)
                    continue;
                foreach (var n in p.GetAdjacentCells(cellInfoProvider.IsCellWalkable))
                    if (!cout.ContainsKey((short)n.CellId))
                    {
                        cout[(short)n.CellId] = d + 1;
                        file.Enqueue(n);
                    }
            }

            var atteignables = cout.Keys.Where(distCible.ContainsKey).ToList();
            if (atteignables.Count == 0)
            {
                // cible injoignable à pied : ancien comportement (case adjacente la plus proche à vol d'oiseau)
                var proche = Target.Position.Point.GetAdjacentCells(cellInfoProvider.IsCellWalkable).OrderBy(entry => entry.ManhattanDistanceTo(Fighter.Position.Point)).FirstOrDefault();
                if (proche == null)
                    return RunStatus.Failure;
                return new MoveAction(Fighter, proche).YieldExecute(context);
            }

            var meilleure = atteignables.OrderBy(c => distCible[c]).ThenBy(c => cout[c]).First();
            if (meilleure == Fighter.Cell.Id)
                return RunStatus.Failure;

            var moveAction = new MoveAction(Fighter, Fighter.Fight.Map.Cells[meilleure]);
            return moveAction.YieldExecute(context);
        }
    }
}