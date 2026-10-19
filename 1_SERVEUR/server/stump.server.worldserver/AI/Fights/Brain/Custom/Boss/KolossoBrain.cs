using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Spells;

namespace Stump.Server.WorldServer.AI.Fights.Brain.Custom.Boss
{
    [BrainIdentifier((int)MonsterIdEnum.KOLOSSO)]
    public class KolossoBrain : Brain
    {
        public KolossoBrain(AIFighter fighter)
            : base(fighter)
        {
            fighter.Fight.FightStarted += Fight_FightStarted;
        }

        private void Fight_FightStarted(IFight obj)
        {
            obj.FightStarted -= Fight_FightStarted;
            Fighter.CastAutoSpell(new Spell((int)SpellIdEnum.PIAUTRE, 1), Fighter.Cell);
            // MYSTIC 18/10 (demande utilisateur) : frappé par une invocation, il n'est vulnérable qu'un tour (jusqu'au tour suivant de
            // l'invocateur) ; avant : vulnérable jusqu'à la fin du combat (durée -1 du déclencheur)
            new VulnerableJusquAuTourDuDeclencheur(Fighter);
        }
    }
}