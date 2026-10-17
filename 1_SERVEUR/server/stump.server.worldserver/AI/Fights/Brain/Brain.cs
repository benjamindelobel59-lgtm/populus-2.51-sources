using System.Diagnostics;
using System.Linq;
using NLog;
using Stump.Core.Attributes;
using Stump.Core.IO;
using Stump.DofusProtocol.Enums;
using Stump.Server.BaseServer.Benchmark;
using Stump.Server.WorldServer.AI.Fights.Actions;
using Stump.Server.WorldServer.AI.Fights.Spells;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Maps.Pathfinding;
using Stump.Server.WorldServer.Game.Spells;
using TreeSharp;

namespace Stump.Server.WorldServer.AI.Fights.Brain
{
    public class Brain
    {
        public const int MaxMovesTries = 20;
        public const int MaxCastLimit = 20;

        [Variable(true)]
        public static bool DebugMode = false;

        protected static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public Brain(AIFighter fighter)
        {
            Fighter = fighter;
            Environment = new EnvironmentAnalyser(Fighter);
            SpellSelector = new SpellSelector(Fighter, Environment);
        }

        public AIFighter Fighter
        {
            get;
            private set;
        }

        public SpellSelector SpellSelector
        {
            get;
            private set;
        }

        public EnvironmentAnalyser Environment
        {
            get;
            private set;
        }

        public bool IsRange
        {
            get;
            set;
        }

        /// <summary>MYSTIC : une ligne [IA] par tour de monstre dans le log du World (diagnostic des tests).</summary>
        [Variable(true)]
        public static bool LogTurns = true;

        // MYSTIC : resume du tour (sorts possibles, sorts lances, deplacement)
        private readonly System.Collections.Generic.List<string> m_turnLog = new System.Collections.Generic.List<string>();

        public virtual void Play()
        {
            Stopwatch sw = null;
            if (BenchmarkManager.Enable)
            {
                sw = Stopwatch.StartNew();
            }

            m_turnLog.Clear();
            var startAP = Fighter.AP;
            var startMP = Fighter.MP;
            var startCell = Fighter.Cell.Id;

            Environment.ResetMoveZone();

            SpellSelector.AnalysePossibilities();
            var possibles = SpellSelector.Possibilities.Select(x => x.Spell.Template.Id + ":" + x.Impacts.Count).ToCSV(",");

            //if (!Fighter.Fight.AIDebugMode)
            //{
            ExecuteSpellCast();
            ExecutePostMove();
            //}

            if (LogTurns)
            {
                logger.Info("[IA] {0} (monstre {1}) PA {2}->{3} PM {4}->{5} case {6}->{7} | sorts {8} | possibles [{9}] | {10} | ecartes [" + string.Join(" ; ", SpellSelector.Exclusions) + "]",
                    Fighter, (Fighter as MonsterFighter)?.Monster.Template.Id ?? 0, startAP, Fighter.AP, startMP, Fighter.MP, startCell, Fighter.Cell.Id,
                    Fighter.Spells.Count, possibles, string.Join(" ; ", m_turnLog));
            }

            if (sw != null)
            {
                sw.Stop();

                if (sw.ElapsedMilliseconds > 50)
                {
                    BenchmarkManager.Instance.Add(BenchmarkEntry.Create("[AI] " + Fighter, sw.Elapsed, "type", "ai",
                        "spells", SpellSelector.Possibilities.Select(x => x.Spell.ToString()).ToCSV(",")));
                }
            }
        }

        public void ExecutePostMove()
        {
            if (!Fighter.CanMove()) 
                return;

            Action action;
            int minRange;
            int maxRange;
            var hasRangeAttack = SpellSelector.GetRangeAttack(out minRange, out maxRange);

            /*if ((Fighter.Stats.MP.Base - Fighter.Stats.MP.Used) > 6)
            {
                action = new FleeAction(Fighter);
            }*/
            if (hasRangeAttack && maxRange > 3 && minRange < maxRange)
            {
                action = new StayInRange(Fighter, minRange, maxRange, Fighter.Spells.Values.Any(x => x.CurrentSpellLevel.CastTestLos));
            }
            else
            {
                action = new MoveNearTo(Fighter, Environment.GetNearestEnemy());
            }

            var before = Fighter.Cell.Id;
            foreach (var result in action.Execute(this))
            {
                if (result == RunStatus.Failure)
                    break;
            }
            m_turnLog.Add(string.Format("deplacement {0} {1}->{2}", action.GetType().Name, before, Fighter.Cell.Id));

            // MYSTIC 17/10 : StayInRange vise une case a portee « a vol d'oiseau » ; si un obstacle (rocher central du Korriandre)
            // la rend inaccessible, le monstre restait sur place tous les tours. Il se rapproche alors de l'ennemi le plus proche.
            if (action is StayInRange && Fighter.Cell.Id == before && Fighter.CanMove() && Environment.GetNearestEnemy() != null)
            {
                var repli = new MoveNearTo(Fighter, Environment.GetNearestEnemy());
                foreach (var result in repli.Execute(this))
                {
                    if (result == RunStatus.Failure)
                        break;
                }
                m_turnLog.Add(string.Format("repli MoveNearTo {0}->{1}", before, Fighter.Cell.Id));
            }
        }

        /// <summary>
        /// MYSTIC : tant qu'il reste des PA et un sort lancable, le monstre continue (avant : arret apres le 1er sort,
        /// et arret complet si le 1er sort choisi echouait → monstre qui ne lance qu'un sort ou rien).
        /// </summary>
        public void ExecuteSpellCast()
        {
            AISpellCastPossibility cast;
            var excluded = new System.Collections.Generic.HashSet<Spell>();
            var totalCasts = 0;
            var rounds = 0;

            while (rounds++ < MaxCastLimit && totalCasts < MaxCastLimit && Fighter.AP > 0 &&
                   (cast = SpellSelector.FindFirstSpellCast(excluded)) != null)
            {
                if (cast.MoveBefore != null)
                {
                    var success = Fighter.StartMove(cast.MoveBefore);
                    var lastPos = Fighter.Cell.Id;

                    var tries = 0;
                    var destinationId = cast.MoveBefore.EndCell.Id;
                    // re-attempt to move if we didn't reach the cell i.e as we trigger a trap
                    while (success && Fighter.Cell.Id != destinationId && Fighter.CanMove() && tries <= MaxMovesTries)
                    {
                        var pathfinder = new Pathfinder(Environment.CellInformationProvider);
                        var path = pathfinder.FindPath(Fighter.Position.Cell.Id, destinationId, false, Fighter.MP);

                        if (path == null || path.IsEmpty())
                        {
                            break;
                        }

                        if (path.MPCost > Fighter.MP)
                        {
                            break;
                        }

                        success = Fighter.StartMove(path);

                        // the mob didn't move so we give up
                        if (Fighter.Cell.Id == lastPos)
                        {
                            break;
                        }

                        lastPos = Fighter.Cell.Id;
                        tries++; // avoid infinite loops
                    }

                }

                var targets = Fighter.Fight.GetAllFighters(cast.Target.AffectedCells).ToArray();

                var i = 0;
                SpellCastResult result;
                while ((result = Fighter.CanCastSpell(cast.Spell, cast.TargetCell.Cell)) == SpellCastResult.OK && i < System.Math.Max(1, cast.MaxConsecutiveCast))
                {
                    if (!Fighter.CastSpell(cast.Spell, cast.TargetCell.Cell))
                        break;

                    i++;

                    if (Fighter.AP > 0 && targets.All(x => !cast.Target.AffectedCells.Contains(x.Cell)) ||
                        targets.Any(x => x.IsDead())) // target has moved, we re-analyse the situation
                    {
                        break;
                    }
                }

                m_turnLog.Add(string.Format("sort {0} x{1}{2}", cast.Spell.Template.Id, i, i == 0 ? " (" + result + ")" : ""));
                totalCasts += i;

                // sort rate : on l'ecarte pour ce tour et on essaie les autres
                if (i == 0)
                    excluded.Add(cast.Spell);

                if (Fighter.IsDead() || Fighter.Fight.State != Game.Fights.FightState.Fighting || !Fighter.IsFighterTurn())
                    break;

                SpellSelector.AnalysePossibilities();
            }
        }

        public void Log(string log, params object[] args)
        {
            logger.Debug("Brain " + Fighter + " : " + log, args);

            if (DebugMode)
                Fighter.Say(string.Format(log, args));
        }
    }
}