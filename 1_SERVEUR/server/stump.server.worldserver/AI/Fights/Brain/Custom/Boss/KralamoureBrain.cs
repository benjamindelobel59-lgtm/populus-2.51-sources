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
    [BrainIdentifier((int)MonsterIdEnum.KRALAMOURE_GEANT)]
    public class KralamoureBrain : Brain
    {
        public KralamoureBrain(AIFighter fighter) : base(fighter)
        {
            fighter.Fight.FightStarted += Fight_FightStarted;
            // MYSTIC 17/10 : passif officiel Kraken (coup élémentaire → état « Envie de… » → tentacule), jamais lancé avant
            Boss.SortPassif.Poser(fighter, 1103);
            // MYSTIC 18/10 (retour de test) : le Kraken posé au début du combat (durée 1) disparaissait au début du 1er tour du Kralamoure
            // (il joue en premier) et l'IA ne pouvait pas le relancer ce tour-là : pas d'« Envie de… » au tour 1
            fighter.Fight.TurnStarted += PremierTour;
            fighter.BuffAdded += OnBuffAdded;
        }

        void PremierTour(IFight fight, FightActor joueur)
        {
            if (joueur != Fighter)
                return;
            fight.TurnStarted -= PremierTour;
            if (!Fighter.GetBuffs(b => b is TriggerBuff && b.Spell.Id == 1103).Any())
                using (fight.StartSequence(SequenceTypeEnum.SEQUENCE_TRIGGERED))
                    Fighter.CastAutoSpell(new Spell(1103, 1), Fighter.Cell);
        }

        const int EncreQuaternaire = 34;

        // MYSTIC 18/10 : l'Encre quaternaire posée par la tentacule était décomptée au début du tour du Kralamoure (bonus d'une invocation
        // décomptés chez son invocateur) et disparaissait avant qu'il lance Vulnérabilité de la Tourbière : il n'était jamais déverrouillé
        void OnBuffAdded(FightActor cible, Buff buff)
        {
            if (buff is StateBuff s && s.State.Id == EncreQuaternaire && buff.Caster != cible)
            {
                buff.DecrementReference = cible;
                buff.Duration = 2;
            }
        }

        private void Fight_FightStarted(IFight obj)
        {
            Fighter.Stats.Fields.FirstOrDefault(x => x.Key == PlayerFields.SummonLimit).Value.Base = 4;
            Fighter.Stats.Fields.FirstOrDefault(x => x.Key == PlayerFields.Initiative).Value.Base = 99999;
        }
    }
}
