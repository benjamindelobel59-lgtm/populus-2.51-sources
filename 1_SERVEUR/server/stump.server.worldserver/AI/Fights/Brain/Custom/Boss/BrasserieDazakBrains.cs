using System.Linq;
using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Spells;

namespace Stump.Server.WorldServer.AI.Fights.Brain.Custom.Boss
{
    /// <summary>
    /// PADGREF 21/10 (log VPS, demande utilisateur) : Brasserie du roi Dazak. Ces monstres gardent le passif Nimpatience (10346) et
    /// lancent en plus, au début de leur tour, les sorts que l'IA normale ne lançait jamais ; ensuite ils jouent normalement.
    /// </summary>
    static class SortsForces
    {
        /// <summary>Lance le sort sur la case si c'est possible (portée, ligne de vue, relance, PA, états).</summary>
        public static bool Lancer(AIFighter f, int sortId, Database.World.Cell cell)
        {
            if (cell == null || !f.IsFighterTurn() || !f.Spells.TryGetValue(sortId, out var sort))
                return false;
            return f.CanCastSpell(sort, cell) == SpellCastResult.OK && f.CastSpell(sort, cell);
        }

        public static System.Collections.Generic.IEnumerable<FightActor> Ennemis(FightActor f) =>
            f.OpposedTeam.Fighters.Where(x => x.IsAlive() && x.IsVisibleFor(f));
    }

    /// <summary>Dazak Martegel : « Nintrépidité » (10350 : à ses alliés, +2 PM et bouclier 1500 pendant 1 tour ; relance 2).</summary>
    [BrainIdentifier(5319)]
    public class DazakBrain : BrainPassif
    {
        public DazakBrain(AIFighter f) : base(f, 10346) { }

        public override void Play()
        {
            if (Fighter.Team.Fighters.Any(x => x != Fighter && x.IsAlive()))
                SortsForces.Lancer(Fighter, 10350, Fighter.Cell);
            base.Play();
        }
    }

    /// <summary>
    /// Tanklume : « Ninfluence » (10335, à l'état Nimpatience 3 : sur lui) et « Ninrmure » (10334 : échange de place avec un allié en
    /// ligne à 3 PO et se sacrifie pour lui 1 tour) sur l'allié le plus blessé qui a un ennemi au contact.
    /// </summary>
    [BrainIdentifier(5314)]
    public class TanklumeBrain : BrainPassif
    {
        public TanklumeBrain(AIFighter f) : base(f, 10346) { }

        public override void Play()
        {
            if (SortsForces.Lancer(Fighter, 10335, Fighter.Cell))
                AttirerAuContact(4);
            var point = Fighter.Position.Point;
            var ennemis = SortsForces.Ennemis(Fighter).ToList();
            var allie = Fighter.Team.Fighters
                .Where(x => x != Fighter && x.IsAlive() && ennemis.Any(e => e.Position.Point.IsAdjacentTo(x.Position.Point)))
                .OrderBy(x => (double)x.LifePoints / System.Math.Max(1, x.MaxLifePoints))
                .FirstOrDefault(x => Fighter.Spells.TryGetValue(10334, out var s) && Fighter.CanCastSpell(s, x.Cell) == SpellCastResult.OK);
            if (allie != null && !ennemis.Any(e => e.Position.Point.IsAdjacentTo(point)))
                SortsForces.Lancer(Fighter, 10334, allie.Cell);
            base.Play();
        }
        /// <summary>PADGREF 21/10 (retour utilisateur) : Ninfluence téléporte aussi les 4 ennemis les plus proches sur les cases libres au
        /// contact du Tanklume (effet 2017 du sort 10335 non géré par le serveur : il sert aux tourelles du Steamer). Un ennemi
        /// Indéplaçable (état 97) n'est pas déplacé.</summary>
        void AttirerAuContact(int nombre)
        {
            var fight = Fighter.Fight;
            var point = Fighter.Position.Point;
            var ennemis = Fighter.OpposedTeam.Fighters
                .Where(x => x.IsAlive() && !x.HasState(97) && x.GetCarryingActor() == null && !x.Position.Point.IsAdjacentTo(point))
                .OrderBy(x => x.Position.Point.ManhattanDistanceTo(point))
                .Take(nombre).ToList();
            foreach (var ennemi in ennemis)
            {
                var libre = point.GetAdjacentCells(id => fight.IsCellFree(fight.Map.Cells[id]))
                    .OrderBy(c => c.ManhattanDistanceTo(ennemi.Position.Point))
                    .FirstOrDefault();
                if (libre == null)
                    return;
                var cell = fight.Map.Cells[libre.CellId];
                ennemi.Position.Cell = cell;
                fight.ForEach(entry => Handlers.Actions.ActionsHandler.SendGameActionFightTeleportOnSameMapMessage(entry.Client, Fighter, ennemi, cell), true);
            }
        }
    }

    /// <summary>Boufbos : Nimpatience seulement (PADGREF 21/10 : Lance-pierre est bien lancé par l'IA normale, vérifié en jeu).</summary>
    [BrainIdentifier(5315)] public class BoufbosBrain : BrainPassif { public BoufbosBrain(AIFighter f) : base(f, 10346) { } }

    /// <summary>
    /// Vatenbière : « Nimplantation » (10344, à l'état Nimpatience 3 : sur lui) et « Gigantosité » (10345 : état sur un ennemi en
    /// ligne de 1 à 8 PO, 1 tour) sur l'ennemi le plus proche qui ne l'a pas déjà.
    /// </summary>
    [BrainIdentifier(5318)]
    public class VatenbiereBrain : BrainPassif
    {
        public VatenbiereBrain(AIFighter f) : base(f, 10346) { }

        public override void Play()
        {
            SortsForces.Lancer(Fighter, 10344, Fighter.Cell);
            var point = Fighter.Position.Point;
            var cible = SortsForces.Ennemis(Fighter)
                .Where(x => !x.GetBuffs(b => b.Spell != null && b.Spell.Id == 10345).Any())
                .OrderBy(x => x.Position.Point.ManhattanDistanceTo(point))
                .FirstOrDefault(x => Fighter.Spells.TryGetValue(10345, out var s) && Fighter.CanCastSpell(s, x.Cell) == SpellCastResult.OK);
            if (cible != null)
                SortsForces.Lancer(Fighter, 10345, cible.Cell);
            base.Play();
        }
    }
}
