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

        // MYSTIC 19/10 : Bump du Wobot Tamponneur (3787) : « W » = le combattant n'a pas pu être téléporté PENDANT CE Bump. Avant, le
        // marqueur restait d'un ancien échec et chaque Bump suivant détruisait tous les Tamponneurs (plus de rebonds sur le Wa Wobot).
        public override bool IsTargetValid(FightActor actor, SpellEffectHandler handler)
            => actor.TeleporteSurCaseInvalide &&
               (handler?.CastHandler?.Spell?.Id != 3787 || ReferenceEquals(actor.EchecTeleportSort, handler.CastHandler));
    }
}
