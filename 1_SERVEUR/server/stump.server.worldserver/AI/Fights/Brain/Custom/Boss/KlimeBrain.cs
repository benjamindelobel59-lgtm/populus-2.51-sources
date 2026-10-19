using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Database.Spells;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Effects.Handlers.Spells.Buffs;
using Stump.Server.WorldServer.Game.Effects.Handlers.Spells.Debuffs;
using Stump.Server.WorldServer.Game.Effects.Handlers.Spells.Others;
using Stump.Server.WorldServer.Game.Effects.Handlers.Spells.States;
using Stump.Server.WorldServer.Game.Effects.Instances;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Fights.Buffs;
using Stump.Server.WorldServer.Game.Fights.Buffs.Customs;
using Stump.Server.WorldServer.Game.Fights.Teams;
using Stump.Server.WorldServer.Game.Spells;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stump.Server.WorldServer.AI.Fights.Brain.Custom
{
    [BrainIdentifier((int)MonsterIdEnum.KLIME)]
    public class KlimBrain : Brain
    {
        public KlimBrain(AIFighter fighter) : base(fighter)
        {
            fighter.Fight.FightStarted += Fight_FightStarted;
        }

        private void Fight_FightStarted(IFight obj)
        {
            obj.FightStarted -= Fight_FightStarted;
            Fighter.CastAutoSpell(new Spell((int)SpellIdEnum.SHAGREEN, 1), Fighter.Cell);
            // MYSTIC 18/10 : une fois rendu vulnérable (poussée sur lui), Klime le restait tout le combat (durée -1 sur déclencheur
            // → infinie) : il redevient invulnérable au début du tour suivant du joueur qui l'a déclenché (comme Royalmouth)
            new Boss.VulnerableJusquAuTourDuDeclencheur(Fighter, true);
        }
    }
}
