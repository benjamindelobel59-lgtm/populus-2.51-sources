using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Fights.Teams;
using Stump.Server.WorldServer.Game.Spells;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stump.Server.WorldServer.AI.Fights.Brain.Custom
{
    [BrainIdentifier((int)MonsterIdEnum.NILEZA)]
    public class NilezaBrain : Brain
    {
        public NilezaBrain(AIFighter fighter) : base(fighter)
        {
            fighter.Fight.FightStarted += Fight_FightStarted;
        }

        private void Fight_FightStarted(IFight obj)
        {
            Fighter.CastAutoSpell(new Spell((int)SpellIdEnum.COHOBATION, 1), Fighter.Cell);

            // PADGREF 21/10 (retour utilisateur) : Cohobation est posée AVANT son 1er tour, qui décomptait déjà 1 tour de retard : la
            // 1re zone Pacifiste partait au tour 2 au lieu du tour 3. +1 tour de retard aux 4 effets à retard (Alchymne à la joie,
            // Gromation, Carba Gnion, Cabalcaloïde) : tours 3, 5, 7, 9 puis tous les 2 tours (Cabalcaloïde se relance lui-même).
            foreach (var buff in Fighter.GetBuffs(b => b.Delay > 0 && b.Spell != null &&
                     (b.Spell.Id == 3568 || b.Spell.Id == 3569 || b.Spell.Id == 3570 || b.Spell.Id == 3583)).ToArray())
                buff.Delay++;
        }
    }
}
