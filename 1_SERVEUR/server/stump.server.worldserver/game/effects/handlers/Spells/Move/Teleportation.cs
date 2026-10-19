using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Database.World;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Effects.Instances;
using Stump.Server.WorldServer.Handlers.Actions;
using Spell = Stump.Server.WorldServer.Game.Spells.Spell;

using Stump.Server.WorldServer.Game.Spells.Casts;
namespace Stump.Server.WorldServer.Game.Effects.Handlers.Spells.Move
{
    [EffectHandler(EffectsEnum.Effect_Teleport)]
    public class Teleportation : SpellEffectHandler
    {
        public Teleportation(EffectDice effect, FightActor caster, SpellCastHandler castHandler, Cell targetedCell, bool critical)
            : base(effect, caster, castHandler, targetedCell, critical)
        {
        }

        protected override bool InternalApply()
        {
            // MYSTIC 18/10 (audit XLII / Fraktale) : conditions portées par le LANCEUR (*E, *e, *v, *V, *F) : une téléportation n'a pas de
            // cible, elle ne part que si la condition est vraie. Les cadrans 5099 / 5107 / 5109 ont 12 téléportations (une par heure) :
            // avant, les 12 partaient et l'Auroraire finissait toujours sur la même case (sens inverse, puis immobile).
            foreach (var t in Targets)
            {
                var surLanceur = (t is Stump.Server.WorldServer.Game.Effects.Handlers.Spells.Targets.StateCriterion s && s.Caster) || (t is Stump.Server.WorldServer.Game.Effects.Handlers.Spells.Targets.LifeCriterion l && l.SurLanceur) ||
                                 (t is Stump.Server.WorldServer.Game.Effects.Handlers.Spells.Targets.MonsterCriterion m && m.Caster);
                if (surLanceur && !t.IsTargetValid(Caster, this))
                    return false;
            }

            var carryingActor = Caster.GetCarryingActor();                

            if (carryingActor != null)
                carryingActor.ThrowActor(TargetedCell);
            else
            {
                if (!Map.IsCellFreeActor(Caster, TargetedCell.Id))
                    Caster.Position.Cell = Map.GetFirstFreeCellInDirectionNear(Caster, TargetedCell);
                else
                    Caster.Position.Cell = TargetedCell;

                Fight.ForEach(entry => ActionsHandler.SendGameActionFightTeleportOnSameMapMessage(entry.Client, Caster, Caster, Caster.Position.Cell), true);

                // MYSTIC 17/10 : diagnostic de Cabriole (signalé : pas de téléportation quand une cible est dans la zone)
                if (Spell.Id == 2888 || Spell.Id == 7775)
                    NLog.LogManager.GetCurrentClassLogger().Info("[Cabriole] {0} : case visée {1}, libre={2}, arrivée {3}, ennemis en croix autour : {4}",
                        Caster, TargetedCell.Id, Map.IsCellFreeActor(Caster, TargetedCell.Id), Caster.Position.Cell.Id,
                        string.Join(",", System.Linq.Enumerable.Select(System.Linq.Enumerable.Where(Fight.GetAllFighters(),
                            f => f.IsAlive() && f.Position.Point.ManhattanDistanceTo(new Maps.Cells.MapPoint(TargetedCell)) == 1), f => f.ToString())));

            }

            return true;
        }
    }
}