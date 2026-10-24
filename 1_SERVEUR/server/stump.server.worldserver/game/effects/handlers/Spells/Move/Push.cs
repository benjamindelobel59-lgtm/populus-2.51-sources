using System.Linq;
using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Database.World;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Effects.Instances;
using Stump.Server.WorldServer.Game.Fights.Buffs;
using Stump.Server.WorldServer.Handlers.Actions;
using Stump.Server.WorldServer.Game.Spells.Casts;
using System;
using Stump.DofusProtocol.Enums.Extensions;
using Stump.Server.WorldServer.Game.Maps.Cells;
using System.Collections.Generic;
using Stump.Server.WorldServer.Game.Spells;
using Stump.Server.WorldServer.Game.Fights.Triggers;

namespace Stump.Server.WorldServer.Game.Effects.Handlers.Spells.Move
{
    [EffectHandler(EffectsEnum.Effect_PushBack)]
    [EffectHandler(EffectsEnum.Effect_PushBack_1103)]
    [EffectHandler(EffectsEnum.Effect_PullForward)]
    [EffectHandler(EffectsEnum.Effect_1021)] // MYSTIC 17/10 : « Repousse de #1 case(s) » (Merkator, totems…)
    [EffectHandler(EffectsEnum.Effect_1022)] // MYSTIC 17/10 : « Attire de #1 case(s) »
    public class Push : SpellEffectHandler
    {
        public Push(EffectDice effect, FightActor caster, SpellCastHandler castHandler, Cell targetedCell, bool critical)
            : base(effect, caster, castHandler, targetedCell, critical)
        {
            DamagesDisabled = effect.EffectId == EffectsEnum.Effect_PushBack_1103 ||
                effect.EffectId == EffectsEnum.Effect_PullForward || effect.EffectId == EffectsEnum.Effect_1022;
            Pull = effect.EffectId == EffectsEnum.Effect_PullForward || effect.EffectId == EffectsEnum.Effect_1022;
        }

        public bool DamagesDisabled
        {
            get;
            set;
        }

        public DirectionsEnum? PushDirection
        {
            get;
            set;
        }

        public bool Pull
        {
            get;
            set;
        }

        public int Distance
        {
            get;
            set;
        }

        protected override bool InternalApply()
        {
            var integerEffect = GenerateEffect();

            if (integerEffect == null)
                return false;

            var actors = new List<FightActor>();

            if (Pull)
                actors = GetAffectedActors().OrderBy(entry => entry.Position.Point.ManhattanDistanceTo(TargetedPoint)).ToList();
            else
                actors = GetAffectedActors().OrderByDescending(entry => entry.Position.Point.ManhattanDistanceTo(TargetedPoint)).ToList();

            foreach (var actor in actors)
            {
                // MYSTIC 19/10 : on ne pousse pas un mort (chaîne de Bump qui continuait sur le cadavre du Wa Wobot : ~10 s d'animations)
                if (!actor.IsAlive())
                    continue;

                // MYSTIC 18/10 : poussées « forcées » (1021 / 1022 = PUSH_FORCE / PULL_FORCE du client) qui doivent bouger un indéplaçable :
                // Mer Cure 4007 (Merkator : déplacer ses monstres le rend vulnérable) et l'attirance des Totems du Moon 6163-6167.
                // Limité à ces sorts (les autres 1021 / 1022 restent bloqués comme avant).
                var forcee = (Effect.EffectId == EffectsEnum.Effect_1021 || Effect.EffectId == EffectsEnum.Effect_1022) &&
                    (Spell.Id == 4007 || (Spell.Id >= 6163 && Spell.Id <= 6167));
                if (!actor.CanBePushed() && !forcee)
                    continue;

                var referenceCell = TargetedCell.Id == actor.Cell.Id ? CastPoint : TargetedPoint;

                if (Spell.Id == 1503)
                {
                    var glyph = Fight.GetTriggers().OfType<GlyphAura>().FirstOrDefault(x => x.GlyphSpell == Spell || x.GlyphSpell.Id == 5158);
                    if (glyph != null)
                    {
                        referenceCell = glyph.CenterCell;
                    }
                }

                if (Spell.Id == 5390)
                {
                    if (this is Retreat && CastHandler.IsCastByPortal) referenceCell = Map.GetCell(CastHandler.Informations.PortalEntryCellId);
                    DamagesDisabled = true;
                }

                if(Spell.Id == 120)
                {
                    if (this is Push)
                        referenceCell = Caster.Cell;
                }

                if (Spell.Id == 9843)
                {
                    referenceCell = Fight.Fighters.FirstOrDefault(x => x.HasState(610)).Cell;
                }

                if (referenceCell.CellId == actor.Position.Cell.Id)
                    continue;

                var pushDirection = Pull ? actor.Position.Point.OrientationTo(referenceCell) : referenceCell.OrientationTo(actor.Position.Point);

                if (PushDirection != null)
                    pushDirection = PushDirection.Value;

                var startCell = actor.Position.Point;
                var lastCell = startCell;

                if (Distance == 0)
                    Distance = (short)(pushDirection.IsDiagonal() ? Math.Ceiling(integerEffect.Value / 2.0) : integerEffect.Value);

                var stopCell = startCell.GetCellInDirection(pushDirection, Distance);
                
                for (var i = 0; i < Distance; i++)
                {
                    var nextCell = lastCell.GetNearestCellInDirection(pushDirection);

                    // the next cell is blocking, or an adjacent cell is blocking if it's in diagonal
                    if (IsBlockingCell(nextCell, actor) ||
                        (pushDirection.IsDiagonal() && pushDirection.GetDiagonalDecomposition().Any(x => IsBlockingCell(lastCell.GetNearestCellInDirection(x), actor))))
                    {
                        if (nextCell == null)
                        {
                            stopCell = lastCell;
                            nextCell = stopCell;
                        }

                        if (Fight.ShouldTriggerOnMove(Fight.Map.Cells[nextCell.CellId], actor) && Fight.GetOneFighter(Fight.Map.Cells[nextCell.CellId]) == null)
                        {
                            #region PORTALS
                            var PortalSpell = new Spell(5426, 1);
                            var portal = Fight.GetTriggers().FirstOrDefault(x => x is Portal && x.ContainsCell(Fight.Map.Cells[nextCell.CellId])) as Portal;
                            if (portal != null)
                            {
                                if (portal.IsActive && Fight.PortalsManager.GetActivableGamePortalsCount(portal.Caster.Team) >= 2)
                                {
                                    var mpWithPortals = Fight.PortalsManager.GetMarksMapPoint(portal.Caster.Team);
                                    var links = Fight.PortalsManager.GetLinks(new MapPoint(portal.CenterCell), mpWithPortals);
                                    if (links.Count > 0)
                                    {
                                        short finalPortal = 0;
                                        foreach (var link in links)
                                        {
                                            if (Fight.GetOneFighter(link) == null)
                                            {
                                                var finalPortalCellx = Fight.Map.GetCell(link);
                                                var portaloutx = Fight.GetTriggers().FirstOrDefault(x => x is Portal && x.ContainsCell(finalPortalCellx)) as Portal;
                                                if (portaloutx.IsActive)
                                                    finalPortal = link;
                                            }
                                        }
                                        
                                        var finalPortalCell = Fight.Map.GetCell(finalPortal);
                                        var portalout = Fight.GetTriggers().FirstOrDefault(x => x is Portal && x.ContainsCell(finalPortalCell)) as Portal;
                                        portal.Disable();
                                        portalout.Disable();
                                        ActionsHandler.SendGameActionFightSlideMessage(Fight.Clients, Caster, actor, startCell.CellId, Fight.Map.Cells[nextCell.CellId].Id);
                                        ActionsHandler.SendGameActionFightTeleportOnSameMapMessage(Fight.Clients, Caster, actor,
                                               finalPortalCell);
                                        actor.Position.Cell = finalPortalCell;
                                        var spellCastHandler = SpellManager.Instance.GetSpellCastHandler(Fight.FighterPlaying,
                                            PortalSpell, actor.Cell, false);
                                        spellCastHandler.MarkTrigger = portal;
                                        spellCastHandler.Initialize();
                                        spellCastHandler.Execute();
                                        Fight.PortalsManager.RefreshClientsPortals();
                                        actor.TriggerBuffs(actor, Game.Fights.Buffs.BuffTriggerType.UsedPortal);
                                        DamagesDisabled = true;
                                        lastCell = finalPortalCell;
                                        startCell = finalPortalCell;
                                        stopCell = startCell.GetCellInDirection(pushDirection, Distance - (i + 1));
                                        continue;
                                    }
                                    else
                                    {
                                        DamagesDisabled = true;
                                        stopCell = nextCell;
                                    }
                                }
                                else
                                {
                                    stopCell = nextCell;
                                }
                            }
                            #endregion
                            else
                            {
                                DamagesDisabled = true;
                                stopCell = nextCell;
                            }
                        }
                        else
                        {
                            nextCell = lastCell;
                            stopCell = lastCell;
                        }


                        break;
                    }

                    if (nextCell != null)
                        lastCell = nextCell;

                }

                if (actor.IsAlive())
                {
                    foreach (var character in Fight.GetCharactersAndSpectators().Where(actor.IsVisibleFor))
                        ActionsHandler.SendGameActionFightSlideMessage(character.Client, Caster, actor, startCell.CellId, stopCell.CellId);
                }

                // MYSTIC 18/10 : la cible est posée sur sa case d'arrivée AVANT les dommages de collision et les déclencheurs « PD » / « PPD ».
                // Avant : l'obstacle percuté réagissait alors que la cible était encore à son départ ; le Bumper du Wobot Tamponneur
                // (Terrier du Wa Wabbit) repoussait dans le vide et les allers-retours entre deux Tamponneurs n'avaient jamais lieu.
                // obstacles alignés calculés avant le déplacement (sinon la cible, déjà arrivée, serait comptée deux fois)
                var alignes = DamagesDisabled ? null : Fight.GetAllFightersInLine(startCell, Distance, pushDirection);

                if (actor.IsCarrying() && stopCell != startCell)
                    actor.ThrowActor(Map.Cells[startCell.CellId], true);

                actor.Position.Cell = Map.Cells[stopCell.CellId];

                // garde-fou : une chaîne de rebonds (poussée → déclencheur → poussée) s'arrête au bout de 40 niveaux
                if (!DamagesDisabled && s_profondeur < 40)
                {
                    s_profondeur++;
                    try
                    {
                    var fightersInline = alignes;
                    fightersInline.Insert(0, actor);
                    var distance = integerEffect.Value - startCell.ManhattanDistanceTo(stopCell);
                    var targets = 0;

                    foreach (var fighter in fightersInline)
                    {
                        var pushDamages = Formulas.FightFormulas.CalculatePushBackDamages(Caster, fighter, (int)distance, targets);

                        if (pushDamages > 0)
                        {
                            var pushDamage = new Fights.Damage(pushDamages)
                            {
                                Source = actor,
                                School = EffectSchoolEnum.Pushback,
                                IgnoreDamageBoost = true,
                                IgnoreDamageReduction = false,
                                SortPoussee = Spell?.Id
                            };

                            fighter.InflictDamage(pushDamage);
                            // MYSTIC 19/10 : diagnostic (dommages de poussée reçus par un joueur alors qu'il pousse)
                            if (fighter is CharacterFighter)
                                NLog.LogManager.GetCurrentClassLogger().Info($"[Poussée] {fighter} subit {pushDamages} (poussée de {actor} par {Caster}, sort {Spell?.Id}, carte {Fight.Map.Id})");
                            fighter.TriggerBuffs(Caster, BuffTriggerType.OnPushDamaged);
                        }

                        // MYSTIC 19/10 : Bumper du Wobot Tamponneur (3788 -> Bump 3787, cible « O » = déclencheur) : le déclencheur est le
                        // combattant qui PERCUTE le Tamponneur (téléporté de l'autre côté puis repoussé vers le Tamponneur suivant : rebonds
                        // jusqu'au one-shot). Avant : c'était le lanceur de la poussée (le joueur), téléporté et repoussé à la place du monstre.
                        var bumper = targets > 0 && actor.IsAlive() && fighter.GetBuffs(x => x.Spell != null && x.Spell.Id == 3788).Any();
                        if (bumper)
                            s_resynchroniser = true;
                        // le Bumper ne renvoie pas un combattant mort
                        if (targets > 0 && (bumper || !fighter.GetBuffs(x => x.Spell != null && x.Spell.Id == 3788).Any()))
                            fighter.TriggerBuffs(bumper ? actor : Caster, BuffTriggerType.OnInderctlyPush);

                        targets++;
                    }
                    }
                    finally
                    {
                        s_profondeur--;
                    }
                }

                if(Fight.Fighters.Any(x => actor.Position.Point.IsAdjacentTo(x.Position.Point)))
                    actor.TriggerBuffs(Caster, BuffTriggerType.OnPushDamagedInMelee);

                // MYSTIC 17/10 : attiree (6, 1022, Attract) = declencheur « MA », poussee = « P » (avant : « MA » inconnu, et une
                // attirance 1022 / Attract comptait comme une poussee)
                if (Pull)
                    actor.TriggerBuffs(Caster, BuffTriggerType.OnPulled);
                else
                    actor.TriggerBuffs(Caster, BuffTriggerType.OnPushed);
                actor.TriggerBuffs(Caster, BuffTriggerType.OnMoved);
            }

            // MYSTIC 19/10 : après une chaîne de Bumper (téléportations et poussées imbriquées DANS cette poussée), le client affichait
            // parfois un combattant à son ancienne case : on renvoie la vraie position de chaque combattant visible.
            if (s_profondeur == 0 && s_resynchroniser)
            {
                s_resynchroniser = false;
                foreach (var f in Fight.GetAllFighters(x => x.IsAlive() && x.Cell != null && x.VisibleState == GameActionFightInvisibilityStateEnum.VISIBLE).ToArray())
                    ActionsHandler.SendGameActionFightTeleportOnSameMapMessage(Fight.Clients, f, f, f.Cell);
            }

            return true;
        }

        [ThreadStatic] static bool s_resynchroniser;
        [ThreadStatic] static int s_profondeur;

        private bool IsBlockingCell(MapPoint cell, FightActor target)
        {
            return cell == null || !Fight.IsCellFree(Map.Cells[cell.CellId]) || Fight.ShouldTriggerOnMove(Fight.Map.Cells[cell.CellId], target);
        }
    }
}