using System.Linq;
using Stump.Server.WorldServer.Game.Actors.Fight;

namespace Stump.Server.WorldServer.Game.Effects.Handlers.Spells.Targets
{
    public class JustSummonedCriterion : TargetCriterion
    {
        public override bool CheckWhenExecute => true;

        public override bool IsTargetValid(FightActor actor, SpellEffectHandler handler)
        {
            // MYSTIC 18/10 : un mort relevé par l'effet en cours compte aussi (Nécromignon de Sylargh, Plantes Zombies d'Halouine)
            return (actor.IsSummoned() || actor.IsRevived) && handler.CastHandler.GetEffectHandlers()
                .Any(x => actor.SummoningEffect == x);
        }
    }
}