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

            // PADGREF 23/10 : sorts de départ des monstres de l'Eliocalypse (passifs : voir SortsDeDepart)
            SortsDeDepart.Poser(fighter);

            // PADGREF 21/10 : journal des sorts jamais lancés pendant le combat
            try
            {
                Fighter.SpellCasted += (caster, handler) => { if (handler?.Spell != null) m_sortsLances.Add(handler.Spell.Id); };
                if (Fighter.Fight != null)
                    Fighter.Fight.FightEnded += JournalSortsJamaisLances;
            }
            catch (System.Exception ex)
            {
                logger.Warn("[IA-sorts] journal impossible pour {0} : {1}", Fighter, ex.Message);
            }
        }

        /// <summary>PADGREF 21/10 : l'IA de base lance aussi ses sorts de SOUTIEN (sur un allié ou sur soi) que le choix de sorts
        /// normal ignorait (ex. Démère, Crapitulation, Soufre douleur, Neuthron). Seulement les monstres SANS IA à nous.</summary>
        [Variable(true)]
        public static bool SoutienAuto = true;

        /// <summary>PADGREF 21/10 : fin de combat : une ligne [IA-sorts] par monstre qui a joué et n'a jamais lancé certains sorts.</summary>
        [Variable(true)]
        public static bool LogSortsJamaisLances = true;

        readonly System.Collections.Generic.HashSet<int> m_sortsLances = new System.Collections.Generic.HashSet<int>();
        int m_toursJoues;

        static readonly System.Collections.Concurrent.ConcurrentDictionary<int, int> s_typeSoutien =
            new System.Collections.Concurrent.ConcurrentDictionary<int, int>();

        static readonly System.Collections.Generic.HashSet<int> s_effetsInvocation = new System.Collections.Generic.HashSet<int> { 180, 181, 185, 405, 1008, 1011, 1097, 2796 };
        static readonly System.Collections.Generic.HashSet<int> s_effetsSoin = new System.Collections.Generic.HashSet<int> { 81, 108, 143, 1109 };

        /// <summary>0 = pas un sort de soutien ; bit 1 = sur un allié (sinon sur soi) ; bit 2 = soigne.
        /// Soutien = aucun effet visant un ennemi (« A »), au moins une cible alliée / soi, pas d'invocation, coût en PA &gt; 0.</summary>
        static int TypeSoutien(Spell sort)
        {
            return s_typeSoutien.GetOrAdd(sort.Id * 100 + sort.CurrentLevel, _ =>
            {
                var niveau = sort.CurrentSpellLevel;
                if (niveau == null || niveau.ApCost == 0 || niveau.Effects == null || niveau.Effects.Count == 0)
                    return 0;
                bool allie = false, soin = false, autre = false;
                foreach (var e in niveau.Effects)
                {
                    if (s_effetsInvocation.Contains((int)e.EffectId))
                        return 0;
                    var jetons = (e.TargetMask ?? "").Split(',').Select(t => t.Trim()).Where(t => t.Length > 0 && "EeFf*".IndexOf(t[0]) < 0).ToList();
                    if (jetons.Count == 0 || jetons.Any(t => t[0] == 'A'))
                        return 0;
                    allie |= jetons.Any(t => t[0] != 'c' && t[0] != 'C');
                    soin |= s_effetsSoin.Contains((int)e.EffectId);
                    autre |= !s_effetsSoin.Contains((int)e.EffectId);
                }
                // PADGREF 23/10 (Repousse de la Brouture : soin + portée) : un sort qui donne aussi un bonus n'attend pas qu'un allié soit blessé
                return 4 | (allie ? 1 : 0) | (soin && !autre ? 2 : 0);
            });
        }

        /// <summary>PADGREF 21/10 : début de tour (IA de base) : UN sort de soutien au plus (d'abord ceux jamais lancés), sur l'allié blessé
        /// le plus proche puis l'allié le plus proche (sinon soi), jamais sur une cible qui a déjà son effet ; un soin seulement si la
        /// cible a perdu au moins 10 % de ses PV. Portée, ligne de vue, relance et PA : règles normales (CanCastSpell).</summary>
        void LancerSoutien()
        {
            var point = Fighter.Position.Point;
            foreach (var sort in Fighter.Spells.Values.Where(x => TypeSoutien(x) != 0).OrderBy(x => m_sortsLances.Contains(x.Id)).ToList())
            {
                var type = TypeSoutien(sort);
                if (Fighter.AP < sort.CurrentSpellLevel.ApCost)
                    continue;
                var cibles = (type & 1) != 0
                    ? Fighter.Team.Fighters.Where(x => x != Fighter && x.IsAlive())
                        .OrderByDescending(x => x.LifePoints < x.MaxLifePoints)
                        .ThenBy(x => x.Position.Point.ManhattanDistanceTo(point))
                        .Concat(new FightActor[] { Fighter })
                    : new FightActor[] { Fighter };
                foreach (var cible in cibles)
                {
                    if ((type & 2) != 0 && cible.LifePoints * 10 >= cible.MaxLifePoints * 9)
                        continue;
                    if (cible.GetBuffs(b => b.Spell != null && b.Spell.Id == sort.Id).Any())
                        continue;
                    if (Fighter.CanCastSpell(sort, cible.Cell) != SpellCastResult.OK)
                        continue;
                    if (Fighter.CastSpell(sort, cible.Cell))
                    {
                        m_turnLog.Add(string.Format("soutien {0} -> {1}", sort.Id, cible));
                        return;
                    }
                }
            }
        }

        void JournalSortsJamaisLances(Game.Fights.IFight fight)
        {
            fight.FightEnded -= JournalSortsJamaisLances;
            if (!LogSortsJamaisLances || m_toursJoues == 0 || !(Fighter is MonsterFighter monstre))
                return;
            var jamais = Fighter.Spells.Values.Where(x => x.CurrentSpellLevel != null && x.CurrentSpellLevel.ApCost > 0 && !m_sortsLances.Contains(x.Id)).ToList();
            if (jamais.Count > 0)
                logger.Info("[IA-sorts] {0} (monstre {1}, IA {2}, {3} tours) jamais lances : {4}", Fighter, monstre.Monster.Template.Id, GetType().Name,
                    m_toursJoues, string.Join(", ", jamais.Select(x => x.Id + " " + x.Template.Name + (TypeSoutien(x) != 0 ? " (soutien)" : ""))));
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
            m_lancesCeTour = 0;
            m_toursJoues++;
            var startAP = Fighter.AP;
            var startMP = Fighter.MP;
            var startCell = Fighter.Cell.Id;

            Environment.ResetMoveZone();
            MemoireInvisibles.NoterVisibles(Fighter);

            // PADGREF 21/10 : sorts de soutien (IA de base seulement : les IA à nous ne changent pas)
            if (SoutienAuto && GetType() == typeof(Brain) && Fighter is MonsterFighter)
            {
                try { LancerSoutien(); }
                catch (System.Exception ex) { logger.Warn("[IA] soutien {0} : {1}", Fighter, ex.Message); }
                Environment.ResetMoveZone();
            }

            SpellSelector.AnalysePossibilities();
            var possibles = SpellSelector.Possibilities.Select(x => x.Spell.Template.Id + ":" + x.Impacts.Count).ToCSV(",");

            //if (!Fighter.Fight.AIDebugMode)
            //{
            ExecuteSpellCast();
            ExecutePostMove();
            ExecuteSpellCastAveugle();
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

        /// <summary>MYSTIC 20/10 : sorts lancés depuis le début du tour (un fuyard ne recule qu'après avoir tapé).</summary>
        int m_lancesCeTour;

        /// <summary>MYSTIC 17/10 : IA fuyarde (après ses sorts, elle s'éloigne des ennemis au lieu de s'en approcher).</summary>
        public virtual bool Fuyard => false;

        /// <summary>PADGREF 23/10 : Corruption (6026) fuit tant qu'il est monté (état 1627 : sorts « monté »), comme dans les vidéos.</summary>
        bool FuyardSelonEtat => Fighter is MonsterFighter mc && mc.Monster.Template.Id == 6026 && Fighter.HasState(1627);

        /// <summary>PADGREF 23/10 : monstre ancré (MonstresAncres) qui a encore un allié non invoqué en vie : il reste sur place.</summary>
        bool Ancre
        {
            get
            {
                var id = (Fighter as MonsterFighter)?.Monster?.Template?.Id ?? (Fighter as Game.Actors.Fight.SummonedMonster)?.MonsterGrade?.MonsterId ?? 0;
                return MonstresAncres.Ids.Contains(id) &&
                       Fighter.Team.Fighters.Any(x => x != Fighter && x.IsAlive() && !(x is Game.Actors.Fight.SummonedFighter));
            }
        }

        /// <summary>MYSTIC 18/10 : va vers la dernière case connue de l'invisible le plus proche ; sur place ou bloqué : case au hasard
        /// (de préférence parmi les cases où il peut être).</summary>
        void ChercherInvisibles()
        {
            var avant = Fighter.Cell.Id;
            var plausibles = Environment.GetCasesPlausiblesInvisibles(out var derniere);
            var ids = new System.Collections.Generic.HashSet<short>(plausibles.Select(c => c.Id));

            if (derniere != null && new Game.Maps.Cells.MapPoint(derniere).ManhattanDistanceTo(Fighter.Position.Point) > 1)
                foreach (var r in new MoveNearCell(Fighter, derniere).Execute(this))
                    if (r == RunStatus.Failure)
                        break;

            if (Fighter.Cell.Id == avant && Fighter.CanMove())
            {
                var zone = Environment.GetMoveZone().Where(p => p.CellId != Fighter.Cell.Id).ToList();
                var prefere = zone.Where(p => ids.Contains((short)p.CellId)).ToList();
                var choix = prefere.Count > 0 ? prefere : zone;
                if (choix.Count > 0)
                    foreach (var r in new MoveAction(Fighter, choix[new Stump.Core.Threading.AsyncRandom().Next(choix.Count)]).Execute(this))
                        if (r == RunStatus.Failure)
                            break;
            }
            m_turnLog.Add(string.Format("recherche invisibles {0}->{1} (derniere case {2})", avant, Fighter.Cell.Id, derniere?.Id.ToString() ?? "inconnue"));
        }

        /// <summary>MYSTIC 18/10 : aucun ennemi visible -> sorts de dégâts « à l'aveugle » sur les cases où un invisible peut être (sorts de
        /// zone favorisés), jamais sur un allié ; la vraie position de l'invisible n'est pas utilisée.</summary>
        public void ExecuteSpellCastAveugle()
        {
            if (Fighter.IsDead() || !Fighter.IsFighterTurn() || !Environment.AucunEnnemiVisible())
                return;

            var plausibles = Environment.GetCasesPlausiblesInvisibles(out _);
            if (plausibles.Count == 0)
                plausibles = Environment.GetMovementCells(6).Where(c => c.Walkable && Fighter.Fight.GetOneFighter(c) == null).ToList();
            var ids = new System.Collections.Generic.HashSet<short>(plausibles.Select(c => c.Id));
            var alea = new Stump.Core.Threading.AsyncRandom();
            var exclus = new System.Collections.Generic.HashSet<Spell>();
            var lances = 0;

            while (lances < MaxCastLimit && Fighter.AP > 0 && Fighter.IsFighterTurn() && Environment.AucunEnnemiVisible())
            {
                Spell meilleurSort = null;
                Game.Maps.Cells.MapPoint meilleurPoint = null;
                double meilleurScore = 0;
                foreach (var sort in Fighter.Spells.Values)
                {
                    if (exclus.Contains(sort))
                        continue;
                    var cat = SpellIdentifier.GetSpellCategories(sort);
                    if ((cat & SpellCategory.Damages) == 0 || (cat & SpellCategory.Summoning) != 0)
                        continue;
                    var niv = sort.CurrentSpellLevel;
                    if (niv.ApCost > Fighter.AP || !Fighter.SpellHistory.CanCastSpell(niv))
                        continue;

                    foreach (var cible in plausibles.OrderBy(x => alea.Next()).Take(25))
                    {
                        if (Fighter.CanCastSpell(sort, cible) != SpellCastResult.OK)
                            continue;
                        var handler = SpellManager.Instance.GetSpellCastHandler(Fighter, sort, cible, false);
                        if (!handler.Initialize())
                            continue;
                        var zone = handler.GetEffectHandlers().SelectMany(h => h.AffectedCells).Distinct().ToList();
                        if (Fighter.Fight.GetAllFighters(zone).Any(f => f.IsAlive() && f.IsFriendlyWith(Fighter)))
                            continue; // jamais sur un allié
                        var score = zone.Count(c => ids.Contains(c.Id)) + alea.NextDouble();
                        if (score > meilleurScore)
                        {
                            meilleurScore = score;
                            meilleurSort = sort;
                            meilleurPoint = new Game.Maps.Cells.MapPoint(cible);
                        }
                    }
                }
                if (meilleurSort == null)
                    break;
                var caseVisee = Fighter.Fight.Map.Cells[meilleurPoint.CellId];
                if (!Fighter.CastSpell(meilleurSort, caseVisee))
                {
                    exclus.Add(meilleurSort);
                    continue;
                }
                lances++;
                m_turnLog.Add(string.Format("sort a l'aveugle {0} case {1}", meilleurSort.Template.Id, caseVisee.Id));
                if (Fighter.IsDead() || Fighter.Fight.State != Game.Fights.FightState.Fighting)
                    break;
            }

            // un coup a révélé quelqu'un : IA normale avec les PA restants
            if (!Fighter.IsDead() && Fighter.IsFighterTurn() && Fighter.AP > 0 && !Environment.AucunEnnemiVisible())
            {
                SpellSelector.AnalysePossibilities();
                ExecuteSpellCast();
            }
        }

        /// <summary>
        /// MYSTIC 19/10 : case atteignable la plus éloignée de l'ennemi visible le plus proche, sans dépasser la portée maximale de ses
        /// attaques (il pourra retaper au tour suivant) ; aucune case à portée -> il ne bouge pas.
        /// </summary>
        /// MYSTIC 20/10 (Mère Veilleuse du Phossile) : renvoie false si aucune case atteignable n'est à portée d'un ennemi ; avant, il
        /// restait alors sur place et passait son tour tant qu'on ne venait pas à lui (il se rapproche maintenant comme les autres).
        bool FuirAPortee()
        {
            if (!SpellSelector.GetRangeAttack(out _, out var portee) || portee <= 0)
                portee = 6;
            var ennemis = Fighter.Fight.GetAllFighters(e => e.IsAlive() && e.IsEnnemyWith(Fighter) && e.IsVisibleFor(Fighter)).ToArray();
            if (ennemis.Length == 0)
                return true;
            int Proche(Game.Maps.Cells.MapPoint p) => ennemis.Min(e => (int)e.Position.Point.ManhattanDistanceTo(p));
            var alea = new Stump.Core.Threading.AsyncRandom();
            var meilleure = Environment.GetMovementCells()
                .Where(c => c.Id == Fighter.Cell.Id || (CellIsFree(c)))
                .Select(c => new { c, d = Proche(new Game.Maps.Cells.MapPoint(c)) })
                .Where(x => x.d <= portee)
                .OrderByDescending(x => x.d).ThenBy(x => alea.Next())
                .FirstOrDefault();
            if (meilleure == null)
                return false;
            if (meilleure.c.Id == Fighter.Cell.Id || meilleure.d <= Proche(Fighter.Position.Point))
                return true;
            foreach (var r in new MoveAction(Fighter, meilleure.c).Execute(this))
                if (r == RunStatus.Failure)
                    break;
            return true;
        }

        bool CellIsFree(Stump.Server.WorldServer.Database.World.Cell c) =>
            Environment.CellInformationProvider.IsCellWalkable(c.Id) && Fighter.Fight.IsCellFree(c);

        bool PeutLancerUnSort()
        {
            SpellSelector.AnalysePossibilities();
            return SpellSelector.Possibilities.Any(x => x.Impacts.Count > 0 && x.Spell.CurrentSpellLevel.ApCost <= Fighter.AP);
        }

        public void ExecutePostMove()
        {
            if (!Fighter.CanMove()) 
                return;

            if (Ancre)
            {
                m_turnLog.Add("ancré : reste sur place");
                return;
            }

            if (Fuyard || FuyardSelonEtat)
            {
                var avantFuite = Fighter.Cell.Id;
                foreach (var resultat in new FleeAction(Fighter).Execute(this))
                    if (resultat == RunStatus.Failure)
                        break;
                m_turnLog.Add("fuite " + avantFuite + "->" + Fighter.Cell.Id);
                return;
            }

            // MYSTIC 19/10 : monstres sans attaque au contact (MonstresFuyards) : ils s'éloignent en restant à portée
            // MYSTIC 20/10 (Paspartou) : seulement s'il a lancé un sort ce tour ; sinon (cible hors ligne de vue...) il avançait quand même
            // « à portée », restait loin et passait son tour : il s'approche maintenant comme les autres
            if (Fighter is MonsterFighter mf && MonstresFuyards.Ids.Contains(mf.Monster.Template.Id) && m_lancesCeTour > 0 && !Environment.AucunEnnemiVisible())
            {
                var avant = Fighter.Cell.Id;
                if (FuirAPortee())
                {
                    m_turnLog.Add("fuite a portee " + avant + "->" + Fighter.Cell.Id);
                    return;
                }
                m_turnLog.Add("trop loin pour fuir a portee : il se rapproche");
            }

            // MYSTIC 18/10 (retour de test) : tous les ennemis sont invisibles -> il les cherche (avant : aucun ennemi « le plus proche »,
            // il ne bougeait plus et ne tapait plus)
            if (Environment.AucunEnnemiVisible())
            {
                ChercherInvisibles();
                return;
            }

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
            // MYSTIC 18/10 : idem s'il a bougé mais garde des PM sans pouvoir lancer de sort depuis sa nouvelle case (petit muret :
            // il s'arrêtait devant après 3 cases au lieu de contourner)
            var resteBloque = action is StayInRange && Fighter.Cell.Id != before && Fighter.MP > 0 && !PeutLancerUnSort();
            if (action is StayInRange && (Fighter.Cell.Id == before || resteBloque) && Fighter.CanMove() && Environment.GetNearestEnemy() != null)
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
                // PADGREF 23/10 : monstre ancré : pas de déplacement pour lancer un sort (il tire depuis sa case ou passe au suivant)
                if (cast.MoveBefore != null && Ancre)
                {
                    excluded.Add(cast.Spell);
                    continue;
                }

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
                m_lancesCeTour += i;

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