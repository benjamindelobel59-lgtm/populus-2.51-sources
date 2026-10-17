using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Spells;

namespace Stump.Server.WorldServer.AI.Fights.Brain.Custom.Boss
{
    /// <summary>
    /// Mansot Royal : passif « Mansomure » (invulnérable ; soigner un joueur au contact du Mansot le rend vulnérable 6 tours ;
    /// +1000 PV, +50 dommages, +1 PM à chaque allié mort).
    /// MYSTIC 17/10 : ressuscité par une Mamansot, il redevient invulnérable (avant : il revenait vulnérable). On ne relance PAS tout
    /// le passif : il reposerait le bonus de mort sur les monstres ressuscités (ils le redonneraient en remourant).
    /// </summary>
    [BrainIdentifier((int)MonsterIdEnum.MANSOT_ROYAL)]
    public class MansotRoyalBrain : Brain
    {
        bool m_mort;

        public MansotRoyalBrain(AIFighter fighter)
            : base(fighter)
        {
            fighter.Fight.FightStarted += Fight_FightStarted;
            fighter.Dead += (f, tueur) => m_mort = true;
            fighter.GetAlive += OnGetAlive;
        }

        private void Fight_FightStarted(IFight obj)
        {
            obj.FightStarted -= Fight_FightStarted;
            Fighter.CastAutoSpell(new Spell((int)SpellIdEnum.PINGWINDJAMMER, 1), Fighter.Cell);
        }

        private void OnGetAlive(FightActor fighter)
        {
            if (!m_mort)
                return;

            m_mort = false;
            using (Fighter.Fight.StartSequence(SequenceTypeEnum.SEQUENCE_TRIGGERED))
                EtatsBoss.Ajouter(Fighter, Fighter, EtatsBoss.Invulnerable, (int)SpellIdEnum.PINGWINDJAMMER);
        }
    }
}
