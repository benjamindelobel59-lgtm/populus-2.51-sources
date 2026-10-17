using System.Linq;
using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Fights.Buffs;
using Stump.Server.WorldServer.Game.Spells;

namespace Stump.Server.WorldServer.AI.Fights.Brain.Custom.Boss
{
    /// <summary>
    /// Obsidiantre : passif « Obligation » (invulnérable ; un ennemi poussé contre lui le rend vulnérable 3 tours).
    /// MYSTIC 17/10 (choix utilisateur, comme tofus.fr) : seul un PERSONNAGE poussé sur lui compte, pas une invocation.
    /// </summary>
    [BrainIdentifier((int)MonsterIdEnum.OBSIDIANTRE)]
    public class ObsidiantreBrain : Brain
    {
        public ObsidiantreBrain(AIFighter fighter)
            : base(fighter)
        {
            fighter.Fight.FightStarted += Fight_FightStarted;
        }

        private void Fight_FightStarted(IFight obj)
        {
            obj.FightStarted -= Fight_FightStarted;
            Fighter.CastAutoSpell(new Spell((int)SpellIdEnum.OBLIGATION_673, 1), Fighter.Cell);

            // le déclencheur « poussé contre l'Obsidiantre » est posé sur tous les ennemis : on le retire aux invocations
            foreach (var ennemi in Fighter.OpposedTeam.Fighters.Where(x => !(x is CharacterFighter)))
                foreach (var buff in ennemi.GetBuffs(x => x is TriggerBuff && x.Spell?.Id == (int)SpellIdEnum.OBLIGATION_673).ToArray())
                    ennemi.RemoveBuff(buff);
        }
    }

    /// <summary>
    /// Pougnette (invoquée par l'Obsidiantre) : passif « Clako » (lourd, renvoie des dégâts autour quand on la frappe).
    /// MYSTIC 17/10 (choix utilisateur) : elle ne fait rien avant son 5e tour ; à partir du 5e tour elle explose (« Pikette ») et tue
    /// tout ce qui est à plus de 8 cases d'elle. Une Pougnette morte n'est jamais ressuscitée (FightTeam.GetLastDeadFighter).
    /// Avant le 17/10 son passif n'était posé qu'au début du combat, donc jamais (elle est toujours invoquée en cours de combat).
    /// </summary>
    [BrainIdentifier((int)MonsterIdEnum.POUGNETTE)]
    public class PougnetteBrain : Brain
    {
        public const int PiketteSpellId = 2601;
        public const int TourExplosion = 5;
        public const int DistanceSure = 8;

        int m_tours;

        public PougnetteBrain(AIFighter fighter)
            : base(fighter)
        {
            SortPassif.Poser(fighter, (int)SpellIdEnum.THWACK);
        }

        public override void Play()
        {
            if (++m_tours < TourExplosion)
                return;

            var point = Fighter.Position.Point;
            var loin = Fighter.Fight.GetAllFighters(x => x.IsAlive() && x != Fighter && x.Team != Fighter.Team &&
                                                         x.Position.Point.ManhattanDistanceTo(point) > DistanceSure).ToArray();
            foreach (var cible in loin)
                cible.Die(Fighter);

            if (Fighter.IsAlive())
                Fighter.CastAutoSpell(new Spell(PiketteSpellId, 1), Fighter.Cell);
        }
    }
}
