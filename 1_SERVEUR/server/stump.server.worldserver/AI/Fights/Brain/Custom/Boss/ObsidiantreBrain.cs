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
        /// <summary>PADGREF 21/10 (retour bêta-testeur) : « Scie lisse » invoque la Pougnette : UNE SEULE Pougnette par combat (même
        /// si elle meurt) : le sort est retiré à l'Obsidiantre dès qu'il l'a lancé une fois.</summary>
        public const int ScieLisseSpellId = 2619;

        public ObsidiantreBrain(AIFighter fighter)
            : base(fighter)
        {
            fighter.Fight.FightStarted += Fight_FightStarted;
            fighter.SpellCasted += (caster, handler) =>
            {
                if (handler?.Spell != null && handler.Spell.Id == ScieLisseSpellId)
                    Fighter.Spells.Remove(ScieLisseSpellId);
            };
        }

        public override void Play()
        {
            // sécurité : une Pougnette déjà présente (vivante ou morte) dans son équipe = plus d'invocation
            if (Fighter.Spells.ContainsKey(ScieLisseSpellId) &&
                Fighter.Fight.GetAllFighters(x => x.Team == Fighter.Team && x is SummonedMonster m && m.Monster.Template.Id == (int)MonsterIdEnum.POUGNETTE).Any())
                Fighter.Spells.Remove(ScieLisseSpellId);
            base.Play();
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
    /// <summary>
    /// PADGREF 21/10 (retour bêta-testeur) : Mofette (donjon de l'Obsidiantre) : l'IA normale ne lançait jamais « Démère » (2269 :
    /// sur un ALLIÉ, 2 tours, les dommages qu'il subit sont soit soignés à 50 %, soit doublés ; relance 5 tours).
    /// Au début de son tour, elle le lance sur l'allié le plus proche qui ne l'a pas déjà (pas l'Obsidiantre, invulnérable), puis joue normalement.
    /// </summary>
    [BrainIdentifier((int)MonsterIdEnum.MOFETTE)]
    public class MofetteBrain : Brain
    {
        public const int DemereSpellId = 2269;

        public MofetteBrain(AIFighter fighter)
            : base(fighter)
        {
        }

        public override void Play()
        {
            LancerDemere();
            base.Play();
        }

        void LancerDemere()
        {
            if (!Fighter.Spells.TryGetValue(DemereSpellId, out var sort))
                return;
            var point = Fighter.Position.Point;
            var allies = Fighter.Team.Fighters
                .Where(x => x != Fighter && x.IsAlive() && !(x is MonsterFighter m && m.Monster.Template.Id == (int)MonsterIdEnum.OBSIDIANTRE)
                            && !x.GetBuffs(b => b.Spell != null && b.Spell.Id == DemereSpellId).Any())
                .OrderBy(x => x.Position.Point.ManhattanDistanceTo(point));
            foreach (var allie in allies)
            {
                if (Fighter.CanCastSpell(sort, allie.Cell) == SpellCastResult.OK)
                {
                    Fighter.CastSpell(sort, allie.Cell);
                    return;
                }
            }
        }
    }
    /// <summary>
    /// PADGREF 21/10 (retour bêta-testeur) : Crapeur (donjon de l'Obsidiantre) : l'IA normale ne lançait jamais « Crapitulation »
    /// (2263 : sur un ALLIÉ, portée 1 à 8 : soin, +2 PM et +100 dommages 3 tours, attire l'allié). Au début de son tour il le lance
    /// sur l'allié blessé le plus proche (sinon l'allié le plus proche) qui n'a pas déjà le bonus, puis joue normalement.
    /// </summary>
    [BrainIdentifier((int)MonsterIdEnum.CRAPEUR)]
    public class CrapeurBrain : Brain
    {
        public const int CrapitulationSpellId = 2263;

        public CrapeurBrain(AIFighter fighter)
            : base(fighter)
        {
        }

        public override void Play()
        {
            LancerCrapitulation();
            base.Play();
        }

        void LancerCrapitulation()
        {
            if (!Fighter.Spells.TryGetValue(CrapitulationSpellId, out var sort))
                return;
            var point = Fighter.Position.Point;
            var allies = Fighter.Team.Fighters
                .Where(x => x != Fighter && x.IsAlive() && !x.GetBuffs(b => b.Spell != null && b.Spell.Id == CrapitulationSpellId).Any())
                .OrderByDescending(x => x.LifePoints < x.MaxLifePoints)
                .ThenBy(x => x.Position.Point.ManhattanDistanceTo(point));
            foreach (var allie in allies)
            {
                if (Fighter.CanCastSpell(sort, allie.Cell) == SpellCastResult.OK)
                {
                    Fighter.CastSpell(sort, allie.Cell);
                    return;
                }
            }
        }
    }

    /// <summary>
    /// PADGREF 21/10 (retour bêta-testeur) : Solfataré (donjon de l'Obsidiantre) : l'IA normale ne lançait jamais « Soufre douleur »
    /// (2275 : sur lui-même, 1 tour : quand il est frappé il lance le sort 2599 ; relance 3 tours). Lancé au début de son tour dès
    /// qu'il est disponible, puis il joue normalement.
    /// </summary>
    [BrainIdentifier((int)MonsterIdEnum.SOLFATARE)]
    public class SolfatareBrain : Brain
    {
        public const int SoufreDouleurSpellId = 2275;

        public SolfatareBrain(AIFighter fighter)
            : base(fighter)
        {
        }

        public override void Play()
        {
            if (Fighter.Spells.TryGetValue(SoufreDouleurSpellId, out var sort) && Fighter.CanCastSpell(sort, Fighter.Cell) == SpellCastResult.OK)
                Fighter.CastSpell(sort, Fighter.Cell);
            base.Play();
        }
    }
    /// <summary>
    /// PADGREF 21/10 : Atomystique (donjon de l'Obsidiantre) : même souci que le Crapeur pour « Neuthron » (2267 : sur un ALLIÉ,
    /// +200 dommages 4 tours et un sort déclenché ; relance 2 tours) : lancé au début de son tour sur l'allié le plus proche qui ne
    /// l'a pas déjà, puis il joue normalement.
    /// </summary>
    [BrainIdentifier((int)MonsterIdEnum.ATOMYSTIQUE)]
    public class AtomystiqueBrain : Brain
    {
        public const int NeuthronSpellId = 2267;

        public AtomystiqueBrain(AIFighter fighter)
            : base(fighter)
        {
        }

        public override void Play()
        {
            if (Fighter.Spells.TryGetValue(NeuthronSpellId, out var sort))
            {
                var point = Fighter.Position.Point;
                var cible = Fighter.Team.Fighters
                    .Where(x => x != Fighter && x.IsAlive() && !x.GetBuffs(b => b.Spell != null && b.Spell.Id == NeuthronSpellId).Any())
                    .OrderBy(x => x.Position.Point.ManhattanDistanceTo(point))
                    .FirstOrDefault(x => Fighter.CanCastSpell(sort, x.Cell) == SpellCastResult.OK);
                if (cible != null)
                    Fighter.CastSpell(sort, cible.Cell);
            }
            base.Play();
        }
    }
}
