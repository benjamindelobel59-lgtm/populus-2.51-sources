using System.Linq;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Maps.Cells;
using Stump.Server.WorldServer.Game.Spells;

namespace Stump.Server.WorldServer.AI.Fights.Brain.Custom.Boss
{
    /// <summary>
    /// MYSTIC 20/10 : Galerie du Phossile (dofusdb donjon 197) : IA des 5 Phorreurs. L'IA normale ne lançait que les sorts de
    /// dégâts directs : les sorts sur soi (auras, renvois), sur un allié (Phormage, échange du Père Phorreur) et de saut (Phozami)
    /// étaient jugés « sans effet » et jamais lancés ; un monstre sans cible à portée passait son tour.
    /// Tour type : sorts sur soi, saut / sorts sur allié utiles, attaques avec déplacement (IA normale), puis depuis la nouvelle
    /// case : attaques restantes et sorts de soutien. Il bouge toujours (rapprochement, fuite à portée pour la Mère Veilleuse).
    /// </summary>
    public abstract class BrainPhorreur : Brain
    {
        const int Phossile = 3651;

        protected BrainPhorreur(AIFighter fighter) : base(fighter)
        {
        }

        /// <summary>Sorts lancés sur lui-même dès qu'ils sont disponibles.</summary>
        protected virtual int[] SortsSurSoi => new int[0];

        /// <summary>Sorts lancés sur un Phorreur allié (jamais le Phossile) ; le plus entouré d'ennemis d'abord.</summary>
        protected virtual int[] SortsSurAllie => new int[0];

        /// <summary>Le sort sur allié n'est utile que si des ennemis sont autour de l'allié (dégâts en zone autour de lui).</summary>
        protected virtual bool AllieDoitEtreEntoure => false;

        /// <summary>Sorts visant une case libre (saut) ; dégâts autour de la case d'arrivée.</summary>
        protected virtual int[] SortsSaut => new int[0];

        /// <summary>Sorts lancés sur l'ennemi le plus proche quand il n'est pas déjà au contact d'un ennemi (échange de place : Berserkoffre).</summary>
        protected virtual int[] SortsSurEnnemi => new int[0];

        /// <summary>false : le sort sur ennemi est lancé même au contact (Reflets placides du Lévitrof).</summary>
        protected virtual bool SurEnnemiSeulementSiLoin => true;

        /// <summary>Ennemis visables par les sorts sur ennemi (ex. joueurs seulement).</summary>
        protected virtual bool CibleEnnemiValide(FightActor ennemi) => true;

        protected bool Actif() => Fighter.IsAlive() && Fighter.IsFighterTurn() && Fighter.Fight.State == FightState.Fighting;

        FightActor[] Ennemis() => Fighter.Fight.GetAllFighters(e => e.IsAlive() && e.IsEnnemyWith(Fighter) && e.IsVisibleFor(Fighter)).ToArray();

        static int Autour(FightActor[] ennemis, MapPoint p, int distance) => ennemis.Count(e => e.Position.Point.ManhattanDistanceTo(p) <= distance);

        public override void Play()
        {
            LancerSurSoi();
            LancerSauts();
            LancerSurEnnemi();
            LancerSurAllies(true);

            // attaques (avec déplacement si besoin) puis déplacement de fin de tour
            base.Play();
            if (!Actif())
                return;

            // depuis la nouvelle case
            if (Fighter.AP > 0)
            {
                SpellSelector.AnalysePossibilities();
                ExecuteSpellCast();
            }
            LancerSauts();
            LancerSurEnnemi();
            LancerSurAllies(false);
            LancerSurSoi();
        }

        void LancerSurEnnemi()
        {
            foreach (var id in SortsSurEnnemi)
            {
                if (!Actif() || !Fighter.Spells.TryGetValue(id, out var sort))
                    continue;
                var ennemis = Ennemis();
                if (SurEnnemiSeulementSiLoin && Autour(ennemis, Fighter.Position.Point, 1) > 0)
                    continue;
                var cible = ennemis.Where(CibleEnnemiValide).OrderBy(e => e.GetBuffs(b => b.Spell.Id == id).Any()) // pas encore touché d'abord (Lévitrof)
                    .ThenBy(e => e.Position.Point.ManhattanDistanceTo(Fighter.Position.Point))
                    .FirstOrDefault(e => Fighter.CanCastSpell(sort, e.Cell) == SpellCastResult.OK);
                if (cible != null)
                    Fighter.CastSpell(sort, cible.Cell);
            }
        }

        protected void LancerSurSoi()
        {
            foreach (var id in SortsSurSoi)
                if (Actif() && Fighter.Spells.TryGetValue(id, out var sort) && Fighter.CanCastSpell(sort, Fighter.Cell) == SpellCastResult.OK)
                    Fighter.CastSpell(sort, Fighter.Cell);
        }

        void LancerSurAllies(bool seulementUtile)
        {
            foreach (var id in SortsSurAllie)
            {
                if (!Fighter.Spells.TryGetValue(id, out var sort))
                    continue;
                var ennemis = Ennemis();
                var allies = Fighter.Team.Fighters
                    .Where(a => a != Fighter && a.IsAlive() && !(a is MonsterFighter m && m.Monster.Template.Id == Phossile))
                    .Select(a => new { a, n = Autour(ennemis, a.Position.Point, 3) })
                    .Where(x => !(AllieDoitEtreEntoure || seulementUtile) || x.n > 0)
                    .OrderByDescending(x => x.n).ThenBy(x => x.a.Position.Point.ManhattanDistanceTo(Fighter.Position.Point))
                    .Select(x => x.a).ToArray();
                foreach (var allie in allies)
                {
                    if (!Actif() || Fighter.AP < sort.CurrentSpellLevel.ApCost)
                        break;
                    if (Fighter.CanCastSpell(sort, allie.Cell) == SpellCastResult.OK)
                        Fighter.CastSpell(sort, allie.Cell);
                }
            }
        }

        void LancerSauts()
        {
            foreach (var id in SortsSaut)
            {
                if (!Actif() || !Fighter.Spells.TryGetValue(id, out var sort) || Fighter.AP < sort.CurrentSpellLevel.ApCost)
                    continue;
                var ennemis = Ennemis();
                var niveau = sort.CurrentSpellLevel;
                var ici = Fighter.Position.Point;
                var meilleure = Fighter.Fight.Map.Cells
                    .Where(c => { var d = new MapPoint(c).ManhattanDistanceTo(ici); return d >= niveau.MinRange && d <= niveau.Range; })
                    .Select(c => new { c, n = Autour(ennemis, new MapPoint(c), 2) })
                    .Where(x => x.n > Autour(ennemis, ici, 2) && Fighter.CanCastSpell(sort, x.c) == SpellCastResult.OK)
                    .OrderByDescending(x => x.n).FirstOrDefault();
                if (meilleure != null)
                    Fighter.CastSpell(sort, meilleure.c);
            }
        }
    }

    /// <summary>Phorrêveur : Phorrêve 4537 (glyphe +PA) et 4539 (glyphe de soin) par l'IA normale ; 4541 sur lui (attire ses alliés).</summary>
    [BrainIdentifier(3658)]
    public class PhorreveurBrain : BrainPhorreur
    {
        public PhorreveurBrain(AIFighter f) : base(f) { }
        protected override int[] SortsSurSoi => new[] { 4541 };
    }

    /// <summary>Métaphorreur : 4547 (5 à 8 PO) et 4548 (1 à 2 PO) par l'IA normale ; 4546 sur lui (armure des alliés proches).</summary>
    [BrainIdentifier(3659)]
    public class MetaphorreurBrain : BrainPhorreur
    {
        public MetaphorreurBrain(AIFighter f) : base(f) { }
        protected override int[] SortsSurSoi => new[] { 4546 };
    }

    /// <summary>Père Phorreur : 4543 au contact par l'IA normale ; 4542 échange avec un allié entouré d'ennemis (dégâts autour) ;
    /// 4545 sur lui (soin des alliés quand il est frappé au contact).</summary>
    [BrainIdentifier(3660)]
    public class PerePhorreurBrain : BrainPhorreur
    {
        public PerePhorreurBrain(AIFighter f) : base(f) { }
        protected override int[] SortsSurSoi => new[] { 4545 };
        protected override int[] SortsSurAllie => new[] { 4542 };
        protected override bool AllieDoitEtreEntoure => true;
    }

    /// <summary>Phozami : 4549 saut (3 à 5 PO en ligne) au milieu des ennemis, 4550 (1 à 2 PO) par l'IA normale ; 4552 sur lui (+PM aux
    /// alliés quand il est frappé à distance).</summary>
    [BrainIdentifier(3661)]
    public class PhozamiBrain : BrainPhorreur
    {
        public PhozamiBrain(AIFighter f) : base(f) { }
        protected override int[] SortsSurSoi => new[] { 4552 };
        protected override int[] SortsSaut => new[] { 4549 };
    }

    /// <summary>Mère Veilleuse : Éphort de guerre 4557 (2 à 7 PO) par l'IA normale, avec fuite à portée ; Phormage 4554 sur ses Phorreurs ;
    /// Phouille 4556 sur elle.</summary>
    [BrainIdentifier(3662)]
    public class MereVeilleuseBrain : BrainPhorreur
    {
        public MereVeilleuseBrain(AIFighter f) : base(f) { }
        protected override int[] SortsSurSoi => new[] { 4556 };
        protected override int[] SortsSurAllie => new[] { 4554 };
    }
}
