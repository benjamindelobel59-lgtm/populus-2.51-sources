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

            // MYSTIC 18/10 : Comtoise pose TROIS effets pour UNE seule rotation (770 « degrés », 771 « Pi/2 », 772 « Pi/4 » avec la même valeur
            // n = 1, 2 ou 3) : ce sont trois écritures de n × 90° (le site cite « 90° horaire = 1Pi/2 = 2Pi/4 »). Avant, le serveur les
            // additionnait (1° + 90° + 45° = 136°) et les sorts partaient n'importe où. Seul l'effet Pi/2 (771 / 774) fait tourner ; les autres
            // ne servent qu'à l'affichage de l'envoûtement, avec la valeur officielle (n × 90 degrés, 2n × Pi/4).
            var n = integerEffect.Value;
            var horaire = Effect.EffectId == EffectsEnum.Effect_770 || Effect.EffectId == EffectsEnum.Effect_771 || Effect.EffectId == EffectsEnum.Effect_772;
            // MYSTIC 18/10 : le sort n'accepte qu'UN buff (la dernière écriture remplace les autres) : chaque écriture porte donc la rotation
            var angle = n * Math.PI / 2 * (horaire ? 1 : -1);

            int affichage;
            switch (Effect.EffectId)
            {
                case EffectsEnum.Effect_770:
                case EffectsEnum.Effect_773:
                    affichage = n * 90;
                    break;
                case EffectsEnum.Effect_772:
                case EffectsEnum.Effect_775:
                    affichage = n * 2;
                    break;
                default:
                    affichage = n;
                    break;
            }

            // MYSTIC 18/10 : la confusion ne se désenvoûte pas (demande utilisateur)
            foreach (var actor in GetAffectedActors())
                actor.AddBuff(new ConfusionBuff(actor.PopNextBuffId(), actor, Caster, this, Spell, Critical, FightDispellableEnum.REALLY_NOT_DISPELLABLE, angle, affichage));

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
            var confusions = lanceur.GetBuffs(x => x is ConfusionBuff c && c.Delay == 0).Cast<ConfusionBuff>().ToArray();
            if (confusions.Length == 0 || visee == null)
                return visee;

            // une seule rotation par sort (les écritures d'une même confusion ne s'additionnent pas) ; Comtoise + Pendultimatum s'additionnent
            var angle = confusions.GroupBy(x => x.Spell.Id).Sum(g => g.Last().Angle);
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
