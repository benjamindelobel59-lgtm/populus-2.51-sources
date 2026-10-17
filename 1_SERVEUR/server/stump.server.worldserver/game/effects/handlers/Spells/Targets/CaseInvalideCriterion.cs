using Stump.Server.WorldServer.Game.Actors.Fight;

namespace Stump.Server.WorldServer.Game.Effects.Handlers.Spells.Targets
{
    /// <summary>
    /// MYSTIC 17/10 : masque « W » du client 2.51 : la cible vient d'être téléportée vers une case invalide (hors carte, case non
    /// marchable). Fuseau / Gousset du Comte Harebourg : la cible meurt (Air du temps / Epoque).
    /// </summary>
    public class CaseInvalideCriterion : TargetCriterion
    {
        public override bool CheckWhenExecute => true;

        public override bool IsTargetValid(FightActor actor, SpellEffectHandler handler)
            => actor.TeleporteSurCaseInvalide;
    }
}
