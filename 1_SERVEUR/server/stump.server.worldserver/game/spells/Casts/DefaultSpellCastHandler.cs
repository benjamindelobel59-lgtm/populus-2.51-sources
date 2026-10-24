using System.Collections.Generic;
using System.Linq;
using Stump.Core.Threading;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Effects;
using Stump.Server.WorldServer.Game.Effects.Handlers.Spells;
using Stump.Server.WorldServer.Game.Fights;

namespace Stump.Server.WorldServer.Game.Spells.Casts
{
    [DefaultSpellCastHandler]
    public class DefaultSpellCastHandler : SpellCastHandler
    {
        protected bool m_initialized;

        public DefaultSpellCastHandler(SpellCastInformations cast)
            : base(cast)
        {
        }

        // MYSTIC 18/10 : jamais null : un échec critique (forcé par la confusion du Comte Harebourg sur un mur) annonce le sort sans
        // Initialize() ; SeeCast plantait (ArgumentNullException) et le joueur était déconnecté
        public SpellEffectHandler[] Handlers
        {
            get;
            protected set;
        } = new SpellEffectHandler[0];

        public override bool SilentCast => m_initialized && Handlers.Any(entry => entry.RequireSilentCast());

        public override bool SeeCast(Character character) => Handlers.All(entry => entry.SeeCast(character));

        protected bool CheckWhenExecute;

        public override bool Initialize()
        {
            var random = new AsyncRandom();

            var effects = Critical && SpellLevel.CriticalEffects.Any() ? SpellLevel.CriticalEffects : SpellLevel.Effects;
            var handlers = new List<SpellEffectHandler>();

            var groups = effects.GroupBy(x => x.Group);
            double totalRandSum = effects.Sum(entry => entry.Random);
            var randGroup = random.NextDouble();
            var stopRandGroup = false;

            foreach (var groupEffects in groups)
            {
                double randSum = groupEffects.Sum(entry => entry.Random);

                if (randSum > 0)
                {
                    if (stopRandGroup)
                        continue;

                    if (randGroup > randSum / totalRandSum)
                    {
                        // group ignored
                        randGroup -= randSum / totalRandSum;
                        continue;
                    }

                    // random group found, there can be only one
                    stopRandGroup = true;
                }

                var randEffect = random.NextDouble();
                var stopRandEffect = false;

                foreach (var effect in groupEffects)
                {
                    if (groups.Count() <= 1)
                    {
                        if (effect.Random > 0)
                        {
                            if (stopRandEffect)
                                continue;

                            if (randEffect > effect.Random / randSum)
                            {
                                // effect ignored
                                randEffect -= effect.Random / randSum;
                                continue;
                            }

                            // random effect found, there can be only one
                            stopRandEffect = true;
                        }
                    }

                    var handler = EffectManager.Instance.GetSpellEffectHandler(effect, Caster, this, TargetedCell, Critical);

                    if (MarkTrigger != null)
                        handler.MarkTrigger = MarkTrigger;

                    if (!handler.CanApply())
                        return false;

                    if (!CheckWhenExecute && handler.Targets.All(x => !x.CheckWhenExecute))
                        handler.SetAffectedActors(handler.GetAffectedActors());
                    else
                        handler.MemoriserCandidatsInitiaux();

                    handler.Efficiency = Informations.Efficiency;

                    handlers.Add(handler);

                    // MYSTIC 17/10 : zone « ;A,B,C… » : une copie de l'effet sur chaque autre case fixe
                    var cases = effect.CasesFixes;
                    // PADGREF 23/10 : sauf les glyphes, posées en UNE marque sur toutes les cases (GlyphSpawn)
                    // PADGREF 23/10 (journal VPS : Étoile de Mer -4 PA × 53) : ni pour un sort déclenché par une marque (glyphe, aura) : ses
                    // effets ne touchent que celui qui la déclenche, une seule fois
                    if (cases != null && cases.Length > 1 && Caster.Fight?.Map != null && !(handler is Effects.Handlers.Spells.Marks.GlyphSpawn) &&
                        MarkTrigger == null)
                    {
                        foreach (var c in cases.Skip(1))
                        {
                            if (c < 0 || c >= Caster.Fight.Map.Cells.Length)
                                continue;

                            var copie = EffectManager.Instance.GetSpellEffectHandler(effect, Caster, this, TargetedCell, Critical);
                            copie.ViserCase(Caster.Fight.Map.Cells[c]);
                            if (MarkTrigger != null)
                                copie.MarkTrigger = MarkTrigger;
                            if (!CheckWhenExecute && copie.Targets.All(x => !x.CheckWhenExecute))
                                copie.SetAffectedActors(copie.GetAffectedActors());
                            copie.Efficiency = Informations.Efficiency;
                            handlers.Add(copie);
                        }
                    }
                }
            }


            Handlers = handlers.ToArray();
            m_initialized = true;

            return true;
        }

        public override void Execute()
        {
            if (!m_initialized)
                Initialize();

            // MYSTIC 19/10 : Bump (3787) et Bump Incotwôlable (3840) des Wobots Tamponneurs : effets dans l'ORDRE du sort (malus de
            // -500 résistance poussée / +200 dommages / +1000 résistance AVANT la poussée). Avant, la priorité faisait pousser d'abord :
            // le Wa Wobot ne prenait le malus qu'après les rebonds et n'était jamais one-shot.
            var ordre = Spell != null && (Spell.Id == 3787 || Spell.Id == 3840) ? Handlers : Handlers.OrderBy(x => x.Priority).ToArray();
            foreach (var handler in ordre)
            {
                handler.Apply();
            }
        }

        public override IEnumerable<SpellEffectHandler> GetEffectHandlers() => Handlers;
    }
}