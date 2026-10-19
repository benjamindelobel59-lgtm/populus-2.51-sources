using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Effects.Handlers.Spells.Summon;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Spells;
using System;

namespace Stump.Server.WorldServer.AI.Fights.Brain.Custom.Boss
{
    /// <summary>MYSTIC 18/10 (demande utilisateur) : Gourlo le Terrible : 1 tonneau par tour, au plus 3 tonneaux en vie.</summary>
    [BrainIdentifier((int)MonsterIdEnum.GOURLO_LE_TERRIBLE)]
    public class GourloBrain : Brain
    {
        public GourloBrain(AIFighter fighter)
            : base(fighter)
        {
            LimiteInvocations.Fixer(fighter, 3);
        }
    }

    [BrainIdentifier((int)MonsterIdEnum.TONNEAU_PIRATE)]
    public class TonneauPirateBrain : Brain
    {
        public TonneauPirateBrain(AIFighter fighter)
            : base(fighter)
        {
            fighter.Fight.TurnStarted += OnTurnStarted;
        }

        int m_tours;

        private void OnTurnStarted(IFight obj, FightActor actor)
        {
            // MYSTIC 17/10 : le tonneau n'a que 2 PA (le Rhum Fourbe en coûte 4 et n'était presque jamais lancé) : sort automatique à
            // chaque début de tour, au niveau de son grade (= son élément)
            if (actor != Fighter || !Fighter.IsAlive())
                return;

            // MYSTIC 18/10 (décision utilisateur, comme la 2.51) : en 2.51 le tonneau gagne +1 PA par tour (Motivation Naturelle) et
            // n'atteint les 4 PA du Rhum Fourbe qu'à son 3e tour → Rhum Fourbe à partir de son 3e tour, puis à chaque tour
            m_tours++;
            if (m_tours < 3)
                return;

            if(Fighter.IsSummoned())
                Fighter.CastAutoSpell(new Spell((int)SpellIdEnum.TREACHEROUS_RHUM, (short)(Fighter as SummonedMonster).MonsterGrade.GradeId), Fighter.Cell);
            else
                Fighter.CastAutoSpell(new Spell((int)SpellIdEnum.TREACHEROUS_RHUM, (short)(Fighter as MonsterFighter).MonsterGrade.GradeId), Fighter.Cell);
        }
    }
}