using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Effects.Instances;
using Stump.Server.WorldServer.Game.Items.Player;

namespace Stump.Server.WorldServer.Game.Effects.Handlers.Usables
{
    [EffectHandler(EffectsEnum.Effect_AddOgrines)]
    public class GiveOgrines : UsableEffectHandler
    {
        public GiveOgrines(EffectBase effect, Character target, BasePlayerItem item)
            : base(effect, target, item)
        {
        }

        protected override bool InternalApply()
        {
            var integerEffect = Effect.GenerateEffect(EffectGenerationContext.Item) as EffectInteger;

            if (integerEffect == null)
                return false;

            var amount = (int)(integerEffect.Value * NumberOfUses);

            UsedItems = NumberOfUses;

            // PADGREF 20/10 : crédit du porte-monnaie du compte web (plus d'objet ogrines en jeu)
            Ogrines.OgrinesWeb.Donner(Target.Client, amount, "objet " + Item.Template.Id);

            Target.SendServerMessage($"Vous avez été crédité de {amount} Ogrines sur votre compte Padgref !");

            return true;
        }
    }
}