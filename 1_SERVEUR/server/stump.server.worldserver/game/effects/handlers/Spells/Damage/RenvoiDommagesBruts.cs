using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Database.World;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Effects.Instances;
using Stump.Server.WorldServer.Game.Spells.Casts;

namespace Stump.Server.WorldServer.Game.Effects.Handlers.Spells.Damage
{
    /// <summary>
    /// MYSTIC 17/10 : effets 1124 / 1125 (client 2.51 : ACTION_FIGHT_SPLASH_RAW_TAKEN_DAMAGE_NEUTRAL / _AIR). Le lanceur renvoie
    /// #1 % des derniers dommages bruts qu'il a subis (avant réductions) en dommages Neutre (1124) ou Air (1125) sur les cibles de
    /// la zone. Les résistances des cibles s'appliquent, pas les bonus du lanceur (validé par l'utilisateur).
    /// Sorts : Pinception (Krabouilleurs, Chiko), Poutchicide (Halouine), Transit (Grodruche), Eopoke (Eosonge), Mauvais augure,
    /// Neuterboum, Eoboum, Molalité illéplochable. Avant : effets non gérés, rien ne se passait.
    /// </summary>
    [EffectHandler(EffectsEnum.Effect_1124)]
    [EffectHandler(EffectsEnum.Effect_1125)]
    [EffectHandler(EffectsEnum.Effect_1126)] // MYSTIC 18/10 : _FIRE (Pyrozomb de Solar / Bethel)
    [EffectHandler(EffectsEnum.Effect_1127)] // _WATER (Rossée Matinale)
    [EffectHandler(EffectsEnum.Effect_1128)] // _EARTH (Manteau Tellurique du Craqueboule)
    public class RenvoiDommagesBruts : SpellEffectHandler
    {
        public RenvoiDommagesBruts(EffectDice effect, FightActor caster, SpellCastHandler castHandler, Cell targetedCell, bool critical)
            : base(effect, caster, castHandler, targetedCell, critical)
        {
        }

        protected override bool InternalApply()
        {
            var montant = Caster.DernierDommageBrut * Dice.DiceNum / 100;
            if (montant <= 0)
                return false;

            EffectSchoolEnum ecole;
            switch (Effect.EffectId)
            {
                case EffectsEnum.Effect_1125: ecole = EffectSchoolEnum.Air; break;
                case EffectsEnum.Effect_1126: ecole = EffectSchoolEnum.Fire; break;
                case EffectsEnum.Effect_1127: ecole = EffectSchoolEnum.Water; break;
                case EffectsEnum.Effect_1128: ecole = EffectSchoolEnum.Earth; break;
                default: ecole = EffectSchoolEnum.Neutral; break;
            }

            foreach (var actor in GetAffectedActors())
            {
                if (actor == Caster)
                    continue;

                // ReflectedDamages : un renvoi ne relance pas les renvois des monstres touchés (pas de boucle sans fin)
                actor.InflictDamage(new Fights.Damage(montant)
                {
                    Source = Caster,
                    School = ecole,
                    IsCritical = Critical,
                    IgnoreDamageBoost = true,
                    IgnoreDamageReduction = false,
                    Spell = Spell, // MYSTIC 18/10 : sans sort, le renvoi passait pour un coup d'arme (Truchage, déclencheurs DC)
                    ReflectedDamages = true,
                });
            }

            return true;
        }
    }
}
