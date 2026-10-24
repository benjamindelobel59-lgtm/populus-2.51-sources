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

        /// <summary>PADGREF 23/10 (retour utilisateur + capture : « les tentacules s'invoquent à 3 PO du Krala, normalement c'est vers ici ») :
        /// salle du boss 26740736, cases entourées par l'utilisateur ; l'IA de base prenait la case libre la plus proche du Kralamoure.</summary>
        static readonly short[] CasesTentacules = { 411, 425, 397, 398, 412, 426, 439, 440, 453, 454 };
        const int SalleDuBoss = 26740736;
        static readonly int[] InvocationsTentacules = { 1107, 1108, 1109, 1110 };

        public override void Play()
        {
            // invocation (1 par tour) d'abord sur ces cases ; si aucune n'est possible, l'IA de base invoque comme avant
            if (Fighter.Fight.Map.Id == SalleDuBoss)
                foreach (var id in InvocationsTentacules)
                {
                    if (!Fighter.Spells.TryGetValue(id, out var sort))
                        continue;
                    var cell = CasesTentacules.Select(c => Fighter.Fight.Map.Cells[c])
                        .FirstOrDefault(c => Fighter.CanCastSpell(sort, c) == Game.Spells.SpellCastResult.OK);
                    if (cell != null)
                        Fighter.CastSpell(sort, cell);
                }
            base.Play();
        }

        private void Fight_FightStarted(IFight obj)
        {
            Fighter.Stats.Fields.FirstOrDefault(x => x.Key == PlayerFields.SummonLimit).Value.Base = 4;
            Fighter.Stats.Fields.FirstOrDefault(x => x.Key == PlayerFields.Initiative).Value.Base = 99999;
        }
    }
}
