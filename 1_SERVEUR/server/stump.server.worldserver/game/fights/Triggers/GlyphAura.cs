using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Database.World;
using Stump.Server.WorldServer.Game.Actors;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Effects.Instances;
using Stump.Server.WorldServer.Game.Maps.Cells;
using Stump.Server.WorldServer.Game.Spells;

namespace Stump.Server.WorldServer.Game.Fights.Triggers
{
    public class GlyphAura : Glyph
    {
        private List<FightActor> m_actorsInside = new List<FightActor>();

        public GlyphAura(short id, FightActor caster, Spell castedSpell, EffectDice originEffect, Spell glyphSpell, Cell centerCell, SpellShapeEnum shape, byte minSize, byte size, Color color)
            : base(id, caster, castedSpell, originEffect, glyphSpell, centerCell, shape, minSize, size, color, false)
        {
        }

        public override TriggerType TriggerType => TriggerType.MOVE | TriggerType.CREATION;
        public override bool StopMovement => false;

        private void Enter(FightActor actor)
        {
            try
            {
                var handler = SpellManager.Instance.GetSpellCastHandler(Caster, GlyphSpell, actor.Cell, false);
                handler.MarkTrigger = this;
                handler.Initialize();
                // MYSTIC 18/10 : respecter le masque de cible de chaque effet, comme Glyph.Trigger (avant : tous les effets touchaient
                // quiconque entrait : la glyphe d'un totem du Moon tuait le Darkli et les joueurs, l'aura du Globilum touchait tout le monde)
                foreach (var effectHandler in handler.GetEffectHandlers())
                {
                    effectHandler.SetAffectedActors(effectHandler.IsValidTarget(actor) ? new[] { actor } : new FightActor[0]);
                }
                handler.Execute();

                actor.PositionChanged += OnPositionChanged;
                m_actorsInside.Add(actor);
            }
            catch { }
        }

        /// <summary>PADGREF 23/10 (retour utilisateur : « on court sur la glyphe et on sort direct, on ne prend rien ») : glyphe-aura
        /// TRAVERSÉE pendant un déplacement (sans s'y arrêter) : ses effets sont appliqués et gardés jusqu'à la fin du tour de celui
        /// qui l'a traversée (ex. Étoile de Mer de Bethel : -4 PA / -2 PM).</summary>
        public void Traverser(FightActor actor)
        {
            if (!CanTrigger(actor) || ContainsCell(actor.Cell))
                return;
            try
            {
                NotifyTriggered(actor, GlyphSpell);
                var handler = SpellManager.Instance.GetSpellCastHandler(Caster, GlyphSpell, actor.Cell, false);
                handler.MarkTrigger = this;
                handler.Initialize();
                foreach (var effectHandler in handler.GetEffectHandlers())
                    effectHandler.SetAffectedActors(effectHandler.IsValidTarget(actor) ? new[] { actor } : new FightActor[0]);
                handler.Execute();
            }
            catch { }

            System.Action<Stump.Server.WorldServer.Game.Fights.IFight, FightActor> fin = null;
            fin = (fight, joueur) =>
            {
                if (joueur != actor)
                    return;
                fight.TurnStopped -= fin;
                if (m_actorsInside.Contains(actor) || ContainsCell(actor.Cell))
                    return;                                   // revenu dedans : l'aura normale gère
                foreach (var buff in actor.GetBuffs(x => x.Caster == Caster && x.Spell.Id == GlyphSpell.Id).ToArray())
                    actor.RemoveBuff(buff);
            };
            Fight.TurnStopped += fin;
        }

        /// <summary>PADGREF 24/10 (demande utilisateur : « je cours 5 cases dans la glyphe du Féca qui retire des PM : quand je rentre dedans,
        /// ça doit m'enlever les PM ») : PM retirés par cette aura à celui qui y ENTRE (0 s'il y est déjà ou n'est pas visé). Lu par
        /// FightPath pour raccourcir le déplacement dès la case d'entrée ; le malus réel est posé à l'arrivée (Enter), retiré en sortant.</summary>
        public int MalusPM(FightActor actor)
        {
            if (!CanTrigger(actor))
                return 0;
            try
            {
                var handler = SpellManager.Instance.GetSpellCastHandler(Caster, GlyphSpell, actor.Cell, false);
                handler.MarkTrigger = this;
                handler.Initialize();
                return handler.GetEffectHandlers().Where(h => (h.Dice.EffectId == EffectsEnum.Effect_SubMP_Roll || h.Dice.EffectId == EffectsEnum.Effect_SubMP ||
                    h.Dice.EffectId == EffectsEnum.Effect_LostMP) && h.IsValidTarget(actor)).Sum(h => (int)System.Math.Max(h.Dice.DiceNum, (short)0));
            }
            catch
            {
                return 0;
            }
        }

        private void OnPositionChanged(ContextActor actor, ObjectPosition position)
        {
            var fighter = actor as FightActor;
            if (!CanTrigger(fighter) && !ContainsCell(actor.Cell))
                Leave(fighter);
        }

        private void Leave(FightActor actor)
        {
            var buffs = actor.GetBuffs(x => x.Caster == Caster && x.Spell.Id == GlyphSpell.Id).ToArray();
            foreach(var buff in buffs)
            {
                actor.RemoveBuff(buff);
            }

            actor.PositionChanged -= OnPositionChanged;
            m_actorsInside.Remove(actor);
        }

        public override void Trigger(FightActor trigger, Cell triggerCell)
        {
            if (CanTrigger(trigger))
            {
                if ((GlyphSpell.Id == 9610 || GlyphSpell.CurrentSpellLevel.Effects.Any(x => x.EffectId == EffectsEnum.Effect_SwitchPosition || x.EffectId == EffectsEnum.Effect_SwitchPosition_1023)) && (trigger.Cell == Caster.Cell || trigger == Caster)) return;
                NotifyTriggered(trigger, GlyphSpell);
                Enter(trigger);
            }
        }

        public override bool CanTrigger(FightActor actor)
        {
            return !m_actorsInside.Contains(actor);
        }

        public override void Remove()
        {
            foreach (var actor in m_actorsInside.ToArray())
            {
                Leave(actor);
            }

            base.Remove();
        }
    }
}