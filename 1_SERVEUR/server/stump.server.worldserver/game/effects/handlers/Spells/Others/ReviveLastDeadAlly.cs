using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Database.World;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Effects.Instances;
using Stump.Server.WorldServer.Game.Spells.Casts;
using Stump.Server.WorldServer.Handlers.Actions;
using Stump.Server.WorldServer.Handlers.Context;

namespace Stump.Server.WorldServer.Game.Effects.Handlers.Spells.Others
{
    /// <summary>
    /// MYSTIC 17/10 : effet 1034 « Invoque le dernier allié mort avec #1 à #2 % de ses PDV » (ex. Nécromignon de Sylargh).
    /// Avant : effet non géré, rien ne se passait. Le mort revient sur la case visée (ou une case libre à côté).
    /// </summary>
    [EffectHandler(EffectsEnum.Effect_1034)]
    public class ReviveLastDeadAlly : SpellEffectHandler
    {
        public ReviveLastDeadAlly(EffectDice effect, FightActor caster, SpellCastHandler castHandler, Cell targetedCell, bool critical)
            : base(effect, caster, castHandler, targetedCell, critical)
        {
        }

        protected override bool InternalApply()
        {
            var integerEffect = GenerateEffect();
            if (integerEffect == null)
                return false;

            var mort = Caster.Team.GetLastDeadFighter();
            if (mort == null || mort.IsAlive())
                return false;

            var cell = TargetedCell;
            if (cell == null || !Fight.IsCellFree(cell))
                cell = Map.GetRandomAdjacentFreeCell(TargetedPoint, true);
            if (cell == null)
                return false;

            mort.Revive(integerEffect.Value, Caster);
            mort.Position.Cell = cell;

            ActionsHandler.SendGameActionFightReviveMessage(Fight.Clients, Caster, mort);
            ContextHandler.SendGameFightTurnListMessage(Fight.Clients, Fight);
            return true;
        }
    }
}
