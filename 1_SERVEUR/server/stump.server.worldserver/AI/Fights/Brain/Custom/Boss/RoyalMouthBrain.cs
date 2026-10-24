using System.Linq;
using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Spells;

namespace Stump.Server.WorldServer.AI.Fights.Brain.Custom.Boss
{
    /// <summary>
    /// Royalmouth : passif « Inimouth » (invulnérable ; poussé → vulnérable + 4 PM ; dommages de poussée → tue les personnages
    /// en ligne avec lui).
    /// MYSTIC 17/10 (tofus.fr, choix utilisateur) : une fois poussé, il reste vulnérable seulement jusqu'au tour suivant du
    /// personnage qui l'a poussé, puis redevient invulnérable (avant : vulnérable pour tout le reste du combat).
    /// </summary>
    [BrainIdentifier((int)MonsterIdEnum.ROYALMOUTH)]
    public class RoyalMouthBrain : Brain
    {
        public RoyalMouthBrain(AIFighter fighter)
            : base(fighter)
        {
            fighter.Fight.FightStarted += Fight_FightStarted;
        }

        /// <summary>PADGREF 21/10 (log VPS) : « Lichemouth » (2231, dégâts en croix autour de lui) dès qu'un ennemi est au contact,
        /// « Regroupmouth » (2230, attire de 2 cases et -1 PM, en croix de 2 autour de lui) dès qu'un ennemi est en ligne à 2 cases :
        /// l'IA normale ne les lançait jamais.</summary>
        public override void Play()
        {
            var point = Fighter.Position.Point;
            var ennemis = SortsForces.Ennemis(Fighter).ToList();
            if (ennemis.Any(e => e.Position.Point.ManhattanDistanceTo(point) == 1))
                SortsForces.Lancer(Fighter, 2231, Fighter.Cell);
            if (ennemis.Any(e => (e.Position.Point.X == point.X || e.Position.Point.Y == point.Y) && e.Position.Point.ManhattanDistanceTo(point) == 2))
                SortsForces.Lancer(Fighter, 2230, Fighter.Cell);
            base.Play();
        }

        private void Fight_FightStarted(IFight obj)
        {
            obj.FightStarted -= Fight_FightStarted;
            Fighter.CastAutoSpell(new Spell((int)SpellIdEnum.INITIADON, 1), Fighter.Cell);
            new VulnerableJusquAuTourDuDeclencheur(Fighter);
            // MYSTIC 18/10 : les PM gagnés sur poussée (Initiadon, durée -1 sur déclencheur → infinie) ne durent qu'1 tour
            Fighter.BuffAdded += (f, b) =>
            {
                if (b is Game.Fights.Buffs.StatBuff s && s.Spell.Id == (int)SpellIdEnum.INITIADON && s.Caracteristic == PlayerFields.MP && s.Duration == -1000)
                    s.Duration = 1;
            };
        }
    }
}
