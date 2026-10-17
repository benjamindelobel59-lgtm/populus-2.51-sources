using System;
using System.Linq;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Types;
using Stump.Server.WorldServer.Database.World;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Effects.Instances;
using Stump.Server.WorldServer.Game.Fights.Buffs;
using Stump.Server.WorldServer.Game.Maps.Cells;
using Spell = Stump.Server.WorldServer.Game.Spells.Spell;
using Stump.Server.WorldServer.Game.Spells.Casts;

namespace Stump.Server.WorldServer.Game.Effects.Handlers.Spells.Others
{
    /// <summary>
    /// MYSTIC 17/10 : effets 770 à 775 « Confusion horaire / contre horaire : #1 degrés | Pi/2 | Pi/4 » (Comtoise et Pendultimatum
    /// du Comte Harebourg). Tant que la confusion dure, la case visée par les sorts de la cible tourne autour d'elle de l'angle donné.
    /// Avant : effets non gérés, rien ne se passait.
    /// </summary>
    [EffectHandler(EffectsEnum.Effect_770)]
    [EffectHandler(EffectsEnum.Effect_771)]
    [EffectHandler(EffectsEnum.Effect_772)]
    [EffectHandler(EffectsEnum.Effect_773)]
    [EffectHandler(EffectsEnum.Effect_774)]
    [EffectHandler(EffectsEnum.Effect_775)]
    public class Confusion : SpellEffectHandler
    {
        public Confusion(EffectDice effect, FightActor caster, SpellCastHandler castHandler, Cell targetedCell, bool critical)
            : base(effect, caster, castHandler, targetedCell, critical)
        {
        }

        protected override bool InternalApply()
        {
            var integerEffect = GenerateEffect();
            if (integerEffect == null)
                return false;

            double unite;
            switch (Effect.EffectId)
            {
                case EffectsEnum.Effect_771:
                case EffectsEnum.Effect_774:
                    unite = Math.PI / 2;
                    break;
                case EffectsEnum.Effect_772:
                case EffectsEnum.Effect_775:
                    unite = Math.PI / 4;
                    break;
                default:
                    unite = Math.PI / 180;
                    break;
            }

            var horaire = Effect.EffectId == EffectsEnum.Effect_770 || Effect.EffectId == EffectsEnum.Effect_771 || Effect.EffectId == EffectsEnum.Effect_772;
            var angle = integerEffect.Value * unite * (horaire ? 1 : -1);

            foreach (var actor in GetAffectedActors())
                actor.AddBuff(new ConfusionBuff(actor.PopNextBuffId(), actor, Caster, this, Spell, Critical, FightDispellableEnum.DISPELLABLE, angle, integerEffect.Value));

            return true;
        }
    }

    public class ConfusionBuff : Buff
    {
        public ConfusionBuff(int id, FightActor target, FightActor caster, SpellEffectHandler effect, Spell spell, bool critical, FightDispellableEnum dispelable, double angle, int valeur)
            : base(id, target, caster, effect, spell, critical, dispelable)
        {
            Angle = angle;
            Valeur = valeur;
        }

        /// <summary>Angle en radians, positif = sens horaire à l'écran.</summary>
        public double Angle { get; }

        public int Valeur { get; }

        public override AbstractFightDispellableEffect GetAbstractFightDispellableEffect() =>
            new FightTemporaryBoostEffect((uint)Id, Target.Id, Duration, (sbyte)Dispellable, (ushort)Spell.Id, (uint)(EffectFix?.ClientEffectId ?? Effect.Id), 0, (short)Valeur);

        /// <summary>
        /// Case réellement touchée par un sort de <paramref name="lanceur"/> visant <paramref name="visee"/> : rotation autour du lanceur
        /// de la somme des confusions. Sur la grille du jeu, X et Y sont deux axes perpendiculaires à l'écran (X vers le bas-droite,
        /// Y vers le haut-droite) : une rotation horaire à l'écran est (dx, dy) → (dx·cos + dy·sin, −dx·sin + dy·cos).
        /// Une case hors carte garde la case visée.
        /// </summary>
        public static Cell CaseConfuse(FightActor lanceur, Cell visee)
        {
            var confusions = lanceur.GetBuffs(x => x is ConfusionBuff && x.Delay == 0).Cast<ConfusionBuff>().ToArray();
            if (confusions.Length == 0 || visee == null)
                return visee;

            var angle = confusions.Sum(x => x.Angle);
            var centre = lanceur.Position.Point;
            var cible = new MapPoint(visee);
            int dx = cible.X - centre.X, dy = cible.Y - centre.Y;
            var nx = centre.X + (int)Math.Round(dx * Math.Cos(angle) + dy * Math.Sin(angle));
            var ny = centre.Y + (int)Math.Round(-dx * Math.Sin(angle) + dy * Math.Cos(angle));

            if (!MapPoint.IsInMap(nx, ny))
                return visee;

            return lanceur.Fight.Map.Cells[MapPoint.CoordToCellId(nx, ny)];
        }
    }

    /// <summary>
    /// MYSTIC 17/10 : effets purement visuels / techniques du client 2.51 (333, 666, 2018, 2019) : rien à appliquer côté serveur,
    /// déclarés pour ne plus être signalés comme « effet non géré ».
    /// </summary>
    [EffectHandler(EffectsEnum.Effect_333)]
    [EffectHandler(EffectsEnum.Effect_666)]
    [EffectHandler(EffectsEnum.Effect_2018)]
    [EffectHandler(EffectsEnum.Effect_2019)]
    public class EffetSansAction : SpellEffectHandler
    {
        public EffetSansAction(EffectDice effect, FightActor caster, SpellCastHandler castHandler, Cell targetedCell, bool critical)
            : base(effect, caster, castHandler, targetedCell, critical)
        {
        }

        protected override bool InternalApply() => true;
    }
}
