using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Spells;

namespace Stump.Server.WorldServer.AI.Fights.Brain.Custom.Boss
{
    /// <summary>
    /// Royalmouth : passif « Inimouth » (invulnérable ; poussé → vulnérable + 4 PM ; dommages de poussée → tue les personnages
    /// en ligne avec lui).
    /// MYSTIC 17/10 (tofus.fr, choix utilisateur) : une fois poussé, il reste vulnérable seulement jusqu'au tour suivant du
    /// personnage qui l'a poussé, puis redevient invulnérable (avant : vulnérable pour tout le reste du combat).
    /// </summary>
    [BrainIdentifier((int)MonsterIdEnum.ROYALMOUTH)]
    public class RoyalMouthBrain : Brain
    {
        public RoyalMouthBrain(AIFighter fighter)
            : base(fighter)
        {
            fighter.Fight.FightStarted += Fight_FightStarted;
        }

        private void Fight_FightStarted(IFight obj)
        {
            obj.FightStarted -= Fight_FightStarted;
            Fighter.CastAutoSpell(new Spell((int)SpellIdEnum.INITIADON, 1), Fighter.Cell);
            new VulnerableJusquAuTourDuDeclencheur(Fighter);
        }
    }
}
