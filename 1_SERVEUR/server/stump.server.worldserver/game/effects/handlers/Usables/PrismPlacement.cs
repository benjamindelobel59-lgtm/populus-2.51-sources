using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Alliances;
using Stump.Server.WorldServer.Game.Effects.Instances;
using Stump.Server.WorldServer.Game.Items.Player;

namespace Stump.Server.WorldServer.Game.Effects.Handlers.Usables
{
    /// <summary>MYSTIC 17/10 : Prisme d'alliance (14293, effet 513) : pose le prisme dans la sous-zone du personnage.</summary>
    [EffectHandler(EffectsEnum.Effect_513)]
    public class PrismPlacement : UsableEffectHandler
    {
        public PrismPlacement(EffectBase effect, Character target, BasePlayerItem item)
            : base(effect, target, item)
        {
        }

        protected override bool InternalApply()
        {
            var erreur = PrismManager.Instance.Poser(Target);
            if (erreur != null)
            {
                Target.SendServerMessage(erreur);
                return false;
            }

            Target.SendServerMessage("Prisme d'alliance posé : la zone appartient maintenant à votre alliance.");
            UsedItems = 1;
            return true;
        }
    }
}
