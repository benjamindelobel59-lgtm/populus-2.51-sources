using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Spells;

namespace Stump.Server.WorldServer.AI.Fights.Brain.Custom.Boss
{
    /// <summary>
    /// MYSTIC 20/10 (Pounicheur) : Morcac : Morzerk = +50 puissance (permanents, cumulables) à chaque coup reçu, sur LUI SEUL.
    /// Avant : le passif Morzerk (6346 niv. 1) était lancé « sur sa case » à chaque retour à la vie, y compris au lancement du combat
    /// pour les Morcac des vagues, qui ne sont pas encore sur la carte : leur case provisoire pouvait être celle d'un autre monstre,
    /// qui recevait le passif à sa place (et en cumulait plusieurs : jusqu'à 1300 puissance). Maintenant : aucun passif posé ;
    /// chaque coup reçu lui lance directement Morzerk niv. 2 (+50 puissance) sur sa propre case.
    /// </summary>
    [BrainIdentifier((int)MonsterIdEnum.MORCAC)]
    public class MorcacBrain : Brain
    {
        const int Morzerk = (int)SpellIdEnum.LOUSSERK; // 6346 « Morzerk »

        public MorcacBrain(AIFighter fighter)
            : base(fighter)
        {
            fighter.DamageInflicted += CoupRecu;
        }

        void CoupRecu(FightActor morcac, Damage damage)
        {
            if (damage == null || damage.Source == null || damage.Source == morcac || damage.ReflectedDamages ||
                damage.School == Game.Effects.EffectSchoolEnum.Pushback)
                return;
            if (!morcac.IsAlive() || morcac.Fight.State != FightState.Fighting || !morcac.Team.Fighters.Contains(morcac))
                return;

            morcac.CastAutoSpell(new Spell(Morzerk, 2), morcac.Cell);
        }
    }
}
