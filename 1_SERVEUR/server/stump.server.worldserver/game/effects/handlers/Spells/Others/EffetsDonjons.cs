using System.Linq;
using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Database.World;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Effects.Instances;
using Stump.Server.WorldServer.Game.Fights.Triggers;
using Stump.Server.WorldServer.Game.Maps.Cells;
using Stump.Server.WorldServer.Game.Spells.Casts;
using Stump.Server.WorldServer.Handlers.Actions;
using Stump.Server.WorldServer.Handlers.Context;

namespace Stump.Server.WorldServer.Game.Effects.Handlers.Spells.Others
{
    /// <summary>
    /// MYSTIC 18/10 : effet 2024 (client 2.51 : ACTION_DISPEL_RUNES_OF_TARGET) : retire les runes posées par la cible
    /// (Salsa d'El Piko). Avant : effet non géré.
    /// </summary>
    [EffectHandler(EffectsEnum.Effect_2024)]
    public class RetirerRunesCible : SpellEffectHandler
    {
        public RetirerRunesCible(EffectDice effect, FightActor caster, SpellCastHandler castHandler, Cell targetedCell, bool critical)
            : base(effect, caster, castHandler, targetedCell, critical)
        {
        }

        protected override bool InternalApply()
        {
            foreach (var actor in GetAffectedActors())
                foreach (var rune in Fight.GetTriggers().OfType<Rune>().Where(x => x.Caster == actor).ToArray())
                    Fight.RemoveTrigger(rune);

            return true;
        }
    }

    /// <summary>
    /// MYSTIC 18/10 : effet 147 (client 2.51 : ACTION_CHARACTER_RESURECT_ALLY_IN_FIGHT) : ressuscite le dernier allié mort du lanceur
    /// avec #1 % de ses PV (100 % si non précisé) sur la 1re case libre de la zone (Pyrozomb de Solar / Bethel : zone C5,3).
    /// Le ressuscité est « l'invocation » de cet effet (critère « U » des effets suivants : Invulnérable, Lourd, passe son tour…).
    /// </summary>
    [EffectHandler(EffectsEnum.Effect_147)]
    public class RessusciterAllie : SpellEffectHandler
    {
        public RessusciterAllie(EffectDice effect, FightActor caster, SpellCastHandler castHandler, Cell targetedCell, bool critical)
            : base(effect, caster, castHandler, targetedCell, critical)
        {
        }

        protected override bool InternalApply()
        {
            // le dernier vrai monstre mort de l'équipe (pas une invocation)
            var mort = Caster.Team.GetLastDeadMonster();
            if (mort == null)
                return false;

            var centre = Caster.Position.Point;
            var cell = AffectedCells.Where(c => c.Walkable && !c.NonWalkableDuringFight && Fight.IsCellFree(c))
                .OrderBy(c => new MapPoint(c).ManhattanDistanceTo(centre)).FirstOrDefault()
                ?? Map.GetRandomAdjacentFreeCell(centre, true);
            if (cell == null)
                return false;

            var integerEffect = GenerateEffect();
            var pourcent = integerEffect != null && integerEffect.Value > 0 ? integerEffect.Value : 100;

            mort.Revive(pourcent, Caster);
            mort.SummoningEffect = this;
            mort.Position.Cell = cell;
            mort.ReinitialiserHistoriqueSorts();

            ActionsHandler.SendGameActionFightReviveMessage(Fight.Clients, Caster, mort);
            ContextHandler.SendGameFightTurnListMessage(Fight.Clients, Fight);
            return true;
        }
    }
}
