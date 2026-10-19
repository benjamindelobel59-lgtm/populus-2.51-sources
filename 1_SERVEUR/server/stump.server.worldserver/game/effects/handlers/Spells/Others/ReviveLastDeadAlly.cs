using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Database.World;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Effects.Instances;
using Stump.Server.WorldServer.Game.Spells.Casts;
using Stump.Server.WorldServer.Handlers.Actions;
using Stump.Server.WorldServer.Handlers.Context;
using Stump.Server.WorldServer.Game.Effects.Handlers.Spells.States;
using Stump.Server.WorldServer.Game.Fights.Buffs;
using Stump.Server.WorldServer.Game.Maps.Cells;
using Stump.Server.WorldServer.Game.Spells;
using System;
using System.Linq;

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

        const int Necromignon = 3607;

        /// <summary>Nécromignon (Sylargh) : le mort revient à 6 PO exactement du lanceur (1re case libre dans le sens horaire).</summary>
        Cell CaseASixPO()
        {
            var centre = Caster.Position.Point;
            return Map.Cells.Where(c => c.Walkable && !c.NonWalkableDuringFight && Fight.IsCellFree(c))
                .Select(c => new { Cell = c, Point = new MapPoint(c) })
                .Where(x => x.Point.ManhattanDistanceTo(centre) == 6)
                .OrderBy(x => (Math.Atan2(x.Point.X - centre.X, centre.Y - x.Point.Y) + 2 * Math.PI) % (2 * Math.PI))
                .Select(x => x.Cell).FirstOrDefault();
        }

        /// <summary>État Zombi permanent sur le relevé (comme ReviveActor / Laisse spirituelle).</summary>
        void PoserZombi(FightActor mort)
        {
            try
            {
                var handler = new AddState(new EffectDice(EffectsEnum.Effect_AddState, (short)SpellStatesEnum.ZOMBI_74, 0, 0), mort, null, mort.Cell, false);
                var buff = new StateBuff(mort.PopNextBuffId(), mort, Caster, handler, Spell, FightDispellableEnum.DISPELLABLE_BY_DEATH,
                    SpellManager.Instance.GetSpellState((uint)SpellStatesEnum.ZOMBI_74))
                {
                    Duration = -1000
                };
                mort.AddBuff(buff, true);
            }
            catch (Exception)
            {
            }
        }

        // MYSTIC 18/10 (demande utilisateur) : Sylargh relève TOUS les monstres morts, 3 au maximum (1 s'il n'y a qu'un mort), dans la
        // zone entourée sur la capture de l'utilisateur (couloir de droite de la salle), les plus proches de la case 418 d'abord.
        const int MaxRelevesNecromignon = 3, CarteSylargh = 110101506;
        static readonly short[] CasesRelevesSylargh = { 418, 404, 405, 390, 432, 433, 446, 417, 419, 376, 377, 389, 391, 445, 460, 362, 461,
            447, 403, 431, 474, 361, 363, 348, 349, 375, 473, 475, 459 };

        Cell CaseDeRelevee()
        {
            if (Map.Id == CarteSylargh)
                foreach (var id in CasesRelevesSylargh)
                {
                    var c = Map.Cells[id];
                    if (c.Walkable && !c.NonWalkableDuringFight && Fight.IsCellFree(c))
                        return c;
                }
            return CaseASixPO();
        }

        bool ReleverNecromignon(int pourcent)
        {
            var morts = Caster.Team.GetDeadMonstersRecentFirst().Take(MaxRelevesNecromignon).ToList();
            var releves = 0;
            foreach (var mort in morts)
            {
                var cell = CaseDeRelevee() ?? Map.GetRandomAdjacentFreeCell(TargetedPoint, true);
                if (cell == null)
                    break;
                mort.Revive(pourcent, Caster);
                mort.SummoningEffect = this; // critère « U » des effets suivants (+300, Indéplaçable, passe son tour, Zombigre)
                mort.Position.Cell = cell;
                PoserZombi(mort);
                ActionsHandler.SendGameActionFightReviveMessage(Fight.Clients, Caster, mort);
                releves++;
            }
            if (releves > 0)
                ContextHandler.SendGameFightTurnListMessage(Fight.Clients, Fight);
            return releves > 0;
        }

        protected override bool InternalApply()
        {
            var integerEffect = GenerateEffect();
            if (integerEffect == null)
                return false;

            if (Spell.Id == Necromignon)
                return ReleverNecromignon(integerEffect.Value);

            var mort = Caster.Team.GetLastDeadFighter();
            if (mort == null || mort.IsAlive())
                return false;

            var cell = Spell.Id == Necromignon ? CaseASixPO() : null;
            if (cell == null)
                cell = TargetedCell;
            if (cell == null || !Fight.IsCellFree(cell))
                cell = Map.GetRandomAdjacentFreeCell(TargetedPoint, true);
            if (cell == null)
                return false;

            mort.Revive(integerEffect.Value, Caster);
            // MYSTIC 18/10 : le relevé est l'invocation de cet effet (critère « U » des effets suivants de Nécromignon : Indéplaçable,
            // passe son tour, Zombidule…) ; sans ça le Sylargh restait invulnérable et le combat était impossible à gagner
            mort.SummoningEffect = this;
            mort.Position.Cell = cell;

            if (Spell.Id == Necromignon)
                PoserZombi(mort);

            ActionsHandler.SendGameActionFightReviveMessage(Fight.Clients, Caster, mort);
            ContextHandler.SendGameFightTurnListMessage(Fight.Clients, Fight);
            return true;
        }
    }
}
