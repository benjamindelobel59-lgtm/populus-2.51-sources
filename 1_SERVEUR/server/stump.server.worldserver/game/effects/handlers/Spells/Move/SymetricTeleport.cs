using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Database.World;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Effects.Instances;
using Stump.Server.WorldServer.Game.Maps.Cells;
using Stump.Server.WorldServer.Handlers.Actions;
using Stump.Server.WorldServer.Game.Spells.Casts;
using System.Linq;
using System.Threading.Tasks;
using Stump.Core.Threading;
using System.Collections.Generic;

namespace Stump.Server.WorldServer.Game.Effects.Handlers.Spells.Move
{
    [EffectHandler(EffectsEnum.Effect_SymetricTargetTeleport)]
    [EffectHandler(EffectsEnum.Effect_SymetricCasterTeleport)]
    [EffectHandler(EffectsEnum.Effect_SymetricPointTeleport)]
    public class SymetricTeleport : SpellEffectHandler
    {
        public SymetricTeleport(EffectDice effect, FightActor caster, SpellCastHandler castHandler, Cell targetedCell, bool critical)
            : base(effect, caster, castHandler, targetedCell, critical)
        {
        }

        protected override bool InternalApply()
        {
            List<FightActor> TelefraggedActor = new List<FightActor>();

            // MYSTIC 18/10 : Fuseau / Gousset (Comte Harebourg) : le masque « T » (échangé) ne doit voir QUE l'échange de cette
            // téléportation ; avant, un ancien échange restait marqué et rendait le Comte vulnérable sans échange
            if (Spell.Id == 3649 || Spell.Id == 3657)
                foreach (var f in Fight.GetAllFighters())
                    f.NeedTelefragState = false;

            foreach (var target in GetAffectedActors())
            {
                var casterPoint = Caster.Position.Point;
                var targetPoint = target.Position.Point;

                switch (Effect.EffectId)
                {
                    case EffectsEnum.Effect_SymetricCasterTeleport:
                        casterPoint = target.Position.Point;
                        targetPoint = Caster.Position.Point;
                        break;
                    case EffectsEnum.Effect_SymetricPointTeleport:
                        casterPoint = target.Position.Point;
                        targetPoint = TargetedPoint;
                        break;
                }

                var cell = new MapPoint((2 * targetPoint.X - casterPoint.X), (2 * targetPoint.Y - casterPoint.Y));

                // MYSTIC 17/10 : celui qui devait bouger et n'a pas pu (hors carte / case non marchable) est repéré pour le masque « W »
                var deplace = Effect.EffectId == EffectsEnum.Effect_SymetricTargetTeleport ? Caster : target;
                deplace.TeleporteSurCaseInvalide = false;
                deplace.EchecTeleportSort = CastHandler;

                // PADGREF 24/10 : Enraciné (6) / Pesanteur (7) bloquent les téléportations (Stabilisation sur le Comte : il ne se téléporte plus)
                if (deplace.TeleportationBloquee())
                    continue;

                if (!cell.IsInMap())
                {
                    deplace.TeleporteSurCaseInvalide = true;
                    continue;
                }

                var dstCell = Map.GetCell(cell.CellId);

                if (dstCell == null)
                {
                    deplace.TeleporteSurCaseInvalide = true;
                    continue;
                }

                if (!dstCell.Walkable && !dstCell.NonWalkableDuringFight)
                {
                    deplace.TeleporteSurCaseInvalide = true;
                    continue;
                }

                var fighter = Fight.GetOneFighter(dstCell);

                // MYSTIC 19/10 : Bump du Tamponneur : on n'échange jamais sa place avec un indéplaçable (les Tamponneurs bougeaient et
                // la ligne de rebonds était cassée) ; le combattant reste où il est
                if (fighter != null && fighter != target && Spell.Id == 3787 && !fighter.CanSwitchPos())
                    continue;

                if (fighter != null && fighter != target)
                {
                    var caster = Caster;

                    if (Effect.EffectId == EffectsEnum.Effect_SymetricCasterTeleport || Effect.EffectId == EffectsEnum.Effect_SymetricPointTeleport)
                        caster = target;

                    if (!TelefraggedActor.Contains(caster) && Effect.EffectId == EffectsEnum.Effect_SymetricPointTeleport)
                    {
                        caster.Telefrag(Caster, fighter);
                        TelefraggedActor.Add(fighter);
                    }
                    else if(Effect.EffectId != EffectsEnum.Effect_SymetricPointTeleport)
                    {
                        caster.Telefrag(Caster, fighter);
                    }
                }
                else
                {
                    var caster = Caster;

                    if (Effect.EffectId == EffectsEnum.Effect_SymetricCasterTeleport || Effect.EffectId == EffectsEnum.Effect_SymetricPointTeleport)
                        caster = target;

                    caster.Position.Cell = dstCell;
                    //caster.MovementHistory.RegisterEntry(dstCell);
                    Fight.ForEach(entry => ActionsHandler.SendGameActionFightTeleportOnSameMapMessage(entry.Client, caster, caster, dstCell), true);
                }
            }

            return true;
        }
    }

    [EffectHandler(EffectsEnum.Effect_2017)]
    public class Recurse : SpellEffectHandler
    {
        public Recurse(EffectDice effect, FightActor caster, SpellCastHandler castHandler, Cell targetedCell, bool critical)
            : base(effect, caster, castHandler, targetedCell, critical)
        {
        }

        protected override bool InternalApply()
        {
            // PADGREF 23/10 (retour utilisateur, Cycloporth) : dans Aspiration Gourmande (7857), 2017 = chaque cible (ici les personnages
            // collés au Cycloporth après l'attirance) lance le sort 7858 (interception des dommages) sur le lanceur. Avant : saut symétrique
            // + poussée -> le Cycloporth bougeait alors qu'il est immobile (Lasccar) et l'interception n'existait pas.
            if (Spell.Template.Id == 7857)
            {
                var sort = new Stump.Server.WorldServer.Game.Spells.Spell(Dice.DiceNum, (short)System.Math.Max(1, (int)Dice.DiceFace));
                foreach (var cible in GetAffectedActors().Where(x => x != Caster && x.IsAlive()).ToArray())
                    cible.CastAutoSpell(sort, Caster.Cell);
                return true;
            }

            DoIt();
            return true;
        }

        // PADGREF 21/10 : crash du World (Ninfluence 10335 du Tanklume, lancé sur lui-même) : cible = lanceur → pas de direction,
        // case voisine nulle → exception dans un async void = arrêt du serveur. Lanceur / même case / hors carte ignorés, et toute
        // erreur est maintenant journalisée au lieu de tuer le serveur.
        public async void DoIt()
        {
            foreach (var target in GetAffectedActors())
            {
                try
                {
                    if (target == Caster || target.Position.Point.CellId == Caster.Position.Point.CellId)
                        continue;

                    var custtarget = target;

                    var casterPoint = Caster.Position.Point;
                    var targetPoint = custtarget.Position.Point;

                    var direction = casterPoint.OrientationToAdjacent(targetPoint);
                    var Fighter = Fight.GetOneFighter(Map.GetCell(targetPoint.GetCellInDirection(direction, 1).CellId));
                    if (Fighter != null && Fighter is SummonedTurret)
                    {
                        casterPoint = custtarget.Position.Point;
                        custtarget = Fighter;
                        targetPoint = custtarget.Position.Point;
                        direction = casterPoint.OrientationToAdjacent(targetPoint);
                        Fighter = Fight.GetOneFighter(Map.GetCell(targetPoint.GetCellInDirection(direction, 1).CellId));
                    }
                    if (Fighter != null && Fighter is SummonedTurret)
                    {
                        casterPoint = custtarget.Position.Point;
                        custtarget = Fighter;
                        targetPoint = custtarget.Position.Point;
                        direction = casterPoint.OrientationToAdjacent(targetPoint);
                        Fighter = Fight.GetOneFighter(Map.GetCell(targetPoint.GetCellInDirection(direction, 1).CellId));
                    }
                    if (Fighter != null && Fighter is SummonedTurret)
                    {
                        casterPoint = custtarget.Position.Point;
                        custtarget = Fighter;
                        targetPoint = custtarget.Position.Point;
                        direction = casterPoint.OrientationToAdjacent(targetPoint);
                    }
                    bool newtowr = false;
                    Cell cdstcell = null;
                    var nextturret = target.Team.GetAllFighters().FirstOrDefault(x => x is SummonedTurret && x != target && x.IsAlive() && x.Position.Point.IsOnSameLine(target.Position.Point) && x.Position.Point.ManhattanDistanceTo(target.Position.Point) > 1 && !(x as SummonedTurret).AlreadyRecursive);
                    if (nextturret != null)
                    {
                        var direc2 = target.Position.Point.OrientationTo(nextturret.Position.Point);

                        var cell1 = Map.GetCell(nextturret.Position.Point.GetCellInDirection(direc2, 1).CellId);
                        if (cell1.Walkable)
                        {
                            if (target.Position.Point.GetCellsOnLineBetween(nextturret.Position.Point).Where(v => v.CellId != target.Position.Point.CellId && v.CellId != nextturret.Position.Point.CellId).Any(x => !Map.GetCell(x.CellId).Walkable))
                            {

                            }
                            else
                            {
                                if(target is SummonedTurret)
                                {
                                    (target as SummonedTurret).AlreadyRecursive = true;
                                }
                                cdstcell = Map.GetCell(target.Position.Point.GetAdjacentCells().FirstOrDefault(x => target.Position.Point.OrientationTo(x) == target.Position.Point.OrientationTo(nextturret.Position.Point)).CellId);
                                newtowr = true;
                            }
                        }
                    }

                    var cell = new MapPoint((2 * targetPoint.X - casterPoint.X), (2 * targetPoint.Y - casterPoint.Y));

                    if (!cell.IsInMap())
                        continue;

                    var dstCell = newtowr ? cdstcell : Map.GetCell(cell.CellId);

                    if (dstCell == null)
                        continue;

                    if (!dstCell.Walkable && !dstCell.NonWalkableDuringFight)
                        continue;

                    // PADGREF 24/10 : Enraciné (6) / Pesanteur (7) bloquent les téléportations
                    if (Caster.TeleportationBloquee())
                        continue;

                    var fighter = Fight.GetOneFighter(dstCell);
                    if (fighter != null && fighter != custtarget)
                    {
                        Caster.Telefrag(Caster, fighter);
                    }
                    else
                    {
                        Caster.Position.Cell = dstCell;
                        Fight.ForEach(entry => ActionsHandler.SendGameActionFightTeleportOnSameMapMessage(entry.Client, Caster, Caster, dstCell), true);
                    }

                    EffectDice d = new EffectDice(EffectsEnum.Effect_PushBack, 4, 0, 0);
                    var hand = EffectManager.Instance.GetSpellEffectHandler(d, custtarget, CastHandler, Caster.Cell, false);
                    if (newtowr) (hand as Move.Push).PushDirection = target.Position.Point.OrientationTo(nextturret.Position.Point);
                    hand.SetAffectedActors(new FightActor[] { Caster });
                    hand.Apply();

                    if (newtowr) await Task.Factory.StartNewDelayed(1000, () =>
                    {
                        EffectDice d2 = new EffectDice(EffectsEnum.Effect_2017, 0, 0, 0);
                        var hand2 = EffectManager.Instance.GetSpellEffectHandler(d2, Caster, CastHandler, nextturret.Cell, false);
                        hand2.SetAffectedActors(new FightActor[] { nextturret });
                        using (Fight.StartSequence(SequenceTypeEnum.SEQUENCE_SPELL)) hand2.Apply();
                    });
                }
                catch (System.Exception ex)
                {
                    NLog.LogManager.GetCurrentClassLogger().Warn($"[Recurse] {Caster} -> {target} (sort {CastHandler?.Spell?.Id}) : {ex.Message}");
                }
            }
        }
    }
}
