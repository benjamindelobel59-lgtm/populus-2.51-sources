using System.Linq;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Spells;

namespace Stump.Server.WorldServer.AI.Fights.Brain.Custom.Boss
{
    /// <summary>PADGREF 23/10 (retours utilisateur, Pandala) : outils communs : lance un sort sur l'ennemi visible le plus proche, ou sur une
    /// case libre voisine, seulement si le serveur accepte le lancer (portée, ligne de vue, PA, relance).</summary>
    static class LancerPandala
    {
        public static bool SurEnnemi(AIFighter f, int sortId)
        {
            if (!f.IsAlive() || !f.Spells.TryGetValue(sortId, out var sort))
                return false;
            foreach (var e in f.OpposedTeam.Fighters.Where(x => x.IsAlive() && x.IsVisibleFor(f)).OrderBy(x => x.Position.Point.ManhattanDistanceTo(f.Position.Point)))
                if (f.CanCastSpell(sort, e.Cell) == SpellCastResult.OK && f.CastSpell(sort, e.Cell))
                    return true;
            return false;
        }

        public static bool SurCaseVoisine(AIFighter f, int sortId)
        {
            if (!f.IsAlive() || !f.Spells.TryGetValue(sortId, out var sort))
                return false;
            var carte = f.Fight.Map;
            foreach (var p in f.Position.Point.GetAdjacentCells())
            {
                var c = carte.Cells[p.CellId];
                if (c.Walkable && !c.NonWalkableDuringFight && f.Fight.IsCellFree(c) && f.CanCastSpell(sort, c) == SpellCastResult.OK && f.CastSpell(sort, c))
                    return true;
            }
            return false;
        }
    }

    /// <summary>PADGREF 23/10 (retour utilisateur : « Pandore ne lance aucun sort sauf sa boîte », idem Maître Pandore) : il pose sa Bouate
    /// (571) quand aucune n'est en vie, puis frappe avec Bambou Musical (577 : échange de place + vol de vie Eau) avant l'IA normale.</summary>
    [BrainIdentifier(537), BrainIdentifier(612)]
    public class PandoreBrain : Brain
    {
        const int SortBouate = 571, SortBambou = 577, Bouate = 606;

        public PandoreBrain(AIFighter f) : base(f) { }

        public override void Play()
        {
            var bouateEnVie = Fighter.Summons.OfType<SummonedMonster>().Any(x => x.IsAlive() && x.Monster.Template.Id == Bouate);
            if (!bouateEnVie)
                LancerPandala.SurCaseVoisine(Fighter, SortBouate);
            while (LancerPandala.SurEnnemi(Fighter, SortBambou)) { }
            base.Play();
        }
    }

    /// <summary>PADGREF 23/10 (retour utilisateur : « la boîte n'invoque rien ») : la Bouate de Pandore ouvre sa boîte (570 : invoque au
    /// hasard un monstre de Pandala) sur une case libre à côté d'elle à chaque fois qu'elle le peut.</summary>
    [BrainIdentifier(606)]
    public class BouateDePandoreBrain : Brain
    {
        public BouateDePandoreBrain(AIFighter f) : base(f) { }

        public override void Play()
        {
            LancerPandala.SurCaseVoisine(Fighter, 570);
            base.Play();
        }
    }
}
