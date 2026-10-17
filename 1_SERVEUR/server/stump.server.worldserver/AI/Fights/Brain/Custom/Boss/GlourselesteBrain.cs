using System.Linq;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Effects;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Fights.Teams;
using Stump.Server.WorldServer.Game.Spells;

namespace Stump.Server.WorldServer.AI.Fights.Brain.Custom.Boss
{
    /// <summary>
    /// MYSTIC : Glourséleste (Antichambre des Gloursons). Au début du combat, il lance son sort passif officiel
    /// « Gloursongeur » (2511), qui porte toutes ses mécaniques (données du client) :
    ///  - Invulnérable à chaque début de son tour (Gloursomptueux) ;
    ///  - frappé au corps à corps → perd l'invulnérabilité et repousse de 4 cases tout ce qui est en croix autour (Hydromel) ;
    ///  - retrait de PM / PO ou déplacement → état Résuglours, qui débloque « Petit Glours Brun » (résurrection d'un allié).
    /// « Tout dommage de poussée tue » : le passif ne le pose qu'aux ennemis présents au début du combat, et pas toujours de façon
    /// fiable (test du 17/10 : personnage poussé contre son Lapino, ni l'un ni l'autre mort). Le cerveau l'assure lui-même :
    /// pendant tout le combat, même après la mort du Glourséleste (comme le buff officiel, illimité), tout ennemi (personnages et invocations, même arrivées en cours de combat) qui subit
    /// des dommages de poussée meurt.
    /// Le reste de son jeu est l'IA normale.
    /// </summary>
    [BrainIdentifier(2864)]
    public class GlourselesteBrain : Brain
    {
        public const int GloursongeurSpellId = 2511;

        public GlourselesteBrain(AIFighter fighter)
            : base(fighter)
        {
            fighter.Fight.FightStarted += OnFightStarted;
        }

        private void OnFightStarted(IFight fight)
        {
            fight.FightStarted -= OnFightStarted;
            Fighter.CastAutoSpell(new Spell(GloursongeurSpellId, 1), Fighter.Cell);

            // dommages de poussée mortels pour tous les ennemis, y compris les invocations ajoutées plus tard
            var ennemis = Fighter.OpposedTeam;
            foreach (var ennemi in ennemis.Fighters.ToArray())
                Surveiller(ennemi);
            ennemis.FighterAdded += OnEnnemiAjoute;
            fight.FightEnded += OnFightEnded;
        }

        private void OnEnnemiAjoute(FightTeam team, FightActor fighter) => Surveiller(fighter);

        private void Surveiller(FightActor fighter)
        {
            fighter.DamageInflicted -= OnDamageInflicted;
            fighter.DamageInflicted += OnDamageInflicted;
        }

        private void OnDamageInflicted(FightActor victime, Damage damage)
        {
            if (damage.School != EffectSchoolEnum.Pushback || damage.Amount <= 0)
                return;
            if (!victime.IsAlive() || victime.Team == Fighter.Team)
                return;

            victime.Die(Fighter);
        }

        private void OnFightEnded(IFight fight)
        {
            fight.FightEnded -= OnFightEnded;
            Fighter.OpposedTeam.FighterAdded -= OnEnnemiAjoute;
            foreach (var fighter in fight.GetAllFighters().ToArray())
                fighter.DamageInflicted -= OnDamageInflicted;
        }
    }
}
