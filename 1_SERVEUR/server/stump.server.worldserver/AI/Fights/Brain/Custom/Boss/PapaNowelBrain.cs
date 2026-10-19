using System.Linq;
using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Fights.Buffs;
using Spell = Stump.Server.WorldServer.Game.Spells.Spell;

namespace Stump.Server.WorldServer.AI.Fights.Brain.Custom.Boss
{
    /// <summary>
    /// MYSTIC 18/10 (audit des donjons) : Papa Nowel. Le moteur limitait les monstres à 1 invocation : dès qu'un cadeau vivait,
    /// Dichotomie (Demi Papa Nowel) était bloquée et la phase 2 n'arrivait jamais → 4 invocations (décision utilisateur).
    /// Embûche de Nowel (883, soin) n'est lancée que sous 30 % de PV (avant : dès les premiers dégâts).
    /// Hotte d'or (2556, décision utilisateur, comme le site) : à chaque tentative de retrait de PM, +5 PO jusqu'à la fin de son
    /// prochain tour (non cumulable).
    /// </summary>
    [BrainIdentifier(872)]
    public class PapaNowelBrain : Brain
    {
        const int Embuche = 883;
        const int SeuilEmbuche = 30;
        const int HotteDor = 2556;

        public PapaNowelBrain(AIFighter fighter)
            : base(fighter)
        {
            LimiteInvocations.Fixer(fighter, 4);
            fighter.PMTentative += OnTentativePM;
        }

        void OnTentativePM(FightActor cible, FightActor source)
        {
            if (source == null || source.Team == Fighter.Team || !Fighter.IsAlive() || Fighter.GetBuffs(x => x.Spell.Id == HotteDor).Any())
                return;

            var handler = new Game.Effects.Handlers.Spells.Buffs.StatsBuff(
                new Game.Effects.Instances.EffectDice(EffectsEnum.Effect_AddRange, 5, 0, 0) { Duration = 2 }, Fighter, null, Fighter.Cell, false);
            var buff = new StatBuff(Fighter.PopNextBuffId(), Fighter, Fighter, handler, new Spell(HotteDor, 1), 5, PlayerFields.Range, false,
                FightDispellableEnum.DISPELLABLE) { Duration = 2 };
            Fighter.AddBuff(buff, true);
        }

        public override void Play()
        {
            Spell retire = null;
            if (Fighter.Spells.TryGetValue(Embuche, out var embuche) && Fighter.LifePoints * 100 > Fighter.MaxLifePoints * SeuilEmbuche)
            {
                retire = embuche;
                Fighter.Spells.Remove(Embuche);
            }

            try
            {
                base.Play();
            }
            finally
            {
                if (retire != null)
                    Fighter.Spells[Embuche] = retire;
            }
        }
    }
}
