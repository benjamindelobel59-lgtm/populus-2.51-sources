using Stump.Server.WorldServer.Game.Maps.Pathfinding;
using System.Linq;
using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Effects.Handlers.Spells.Summon;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Spells;
using System;

namespace Stump.Server.WorldServer.AI.Fights.Brain.Custom.Boss
{
    [BrainIdentifier((int)MonsterIdEnum.DISCIPLE_DU_KIMBO)]
    public class DiscipleDuKimboBrain : Brain
    {
        public DiscipleDuKimboBrain(AIFighter fighter)
            : base(fighter)
        {
            fighter.Fight.TurnStarted += OnTurnStarted;
        }

        static readonly Random m_hasard = new Random();

        /// <summary>
        /// MYSTIC 17/10 : après sa glyphe (+1 PM), le Disciple se décale d'une case (avant : il gardait son PM sans bouger).
        /// </summary>
        public override void Play()
        {
            base.Play();

            if (!Fighter.IsAlive() || Fighter.MP <= 0 || !Fighter.CanMove())
                return;

            var depart = Fighter.Cell.Id;
            var cases = Fighter.Position.Point.GetAdjacentCells(id => Fighter.Fight.Map.Cells[id].Walkable && Fighter.Fight.IsCellFree(Fighter.Fight.Map.Cells[id]))
                .Select(x => x.CellId).ToList();
            if (cases.Count == 0)
                return;

            short cible;
            lock (m_hasard)
                cible = cases[m_hasard.Next(cases.Count)];

            // pas direct d'une case : le pathfinder de l'IA évite les cases sous glyphe, or toutes ses cases voisines sont sous
            // sa propre glyphe (impaire) -> il ne bougeait jamais
            var carte = Fighter.Fight.Map;
            if (!carte.Cells[cible].Walkable)
                return;
            Fighter.StartMove(new Path(carte, new[] { Fighter.Cell, carte.Cells[cible] }));
        }

        private void OnTurnStarted(IFight obj, FightActor actor)
        {
            // MYSTIC 17/10 : seulement au début de SON tour (avant : au début du tour de chaque combattant)
            if (actor != Fighter || !Fighter.IsAlive())
                return;

            if (Fighter.IsSummoned())
            {
                if(Fighter.HasState((int)SpellStatesEnum.GLYPHE_IMPAIRE_29))
                    Fighter.CastSpell(new Spell((int)SpellIdEnum.ODD_GLYPH, (short)(Fighter as SummonedMonster).MonsterGrade.GradeId), Fighter.Cell);
                else if (Fighter.HasState((int)SpellStatesEnum.GLYPHE_PAIRE_30))
                    Fighter.CastSpell(new Spell((int)SpellIdEnum.EVEN_GLYPH, (short)(Fighter as SummonedMonster).MonsterGrade.GradeId), Fighter.Cell);
            }
        }
    }
}