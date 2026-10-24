using System;
using System.Collections.Generic;
using System.Linq;
using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Database.World;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Maps.Cells;
using Stump.Server.WorldServer.Game.Spells;
using Stump.Server.WorldServer.Game.Spells.Casts;

namespace Stump.Server.WorldServer.AI.Fights.Brain.Custom.Boss
{
    /// <summary>
    /// PADGREF 21/10 (retours bêta-testeur) : monstres de la vague du Toxoliath. Leurs sorts sur soi / déclenchés n'étaient jamais
    /// lancés ou visaient le monstre lui-même. Chaque IA lance d'abord ses sorts particuliers, puis joue normalement.
    /// </summary>
    static class OutilsToxoliath
    {
        public static bool EnLigne(MapPoint a, MapPoint b) => a.X == b.X || a.Y == b.Y;

        public static IEnumerable<FightActor> EnnemisVivants(FightActor f) => f.OpposedTeam.Fighters.Where(x => x.IsAlive() && x.IsVisibleFor(f));
    }

    /// <summary>
    /// Scoliopode : « Poison incurable » (4737, sur lui, 2 tours, relance 3) : chaque ligne de dégâts qu'il subit applique Insoignable
    /// et vole 2 PM à l'ATTAQUANT pendant 1 tour (sort 4736 ; le déclenchement vise l'attaquant : voir CastSpell.cs). Lancé au début de
    /// son tour dès qu'il est disponible.
    /// </summary>
    [BrainIdentifier((int)MonsterIdEnum.SCOLIOPODE)]
    public class ScoliopodeBrain : Brain
    {
        public const int PoisonIncurable = 4737;

        public ScoliopodeBrain(AIFighter fighter) : base(fighter) { }

        public override void Play()
        {
            if (Fighter.Spells.TryGetValue(PoisonIncurable, out var sort) && !Fighter.GetBuffs(b => b.Spell != null && b.Spell.Id == 4736).Any()
                && Fighter.CanCastSpell(sort, Fighter.Cell) == SpellCastResult.OK)
                Fighter.CastSpell(sort, Fighter.Cell);
            base.Play();
        }
    }

    /// <summary>
    /// Lucrane : « Carapace gardée » codée ici (le passif officiel 4746 faisait subir le renvoi et le poison à la Lucrane elle-même) :
    ///   - frappée par un ennemi : RENVOIE 50 % des dégâts à l'attaquant et lui applique un poison neutre de 120 au début de son
    ///     prochain tour, par ligne de dégâts ;
    ///   - dégâts de POUSSÉE reçus : carapace coupée pendant 1 tour, mais la Lucrane devient Indéplaçable (état 97) pendant ce tour.
    /// Elle lance aussi « Pinsecte » (4748 : retire 50 % des PV actuels 1 tour, en ligne à 2 PO, 2 fois par tour), que l'IA normale
    /// n'utilisait pas. Toujours fuyarde après ses sorts (MonstresFuyards).
    /// </summary>
    [BrainIdentifier((int)MonsterIdEnum.LUCRANE)]
    public class LucraneBrain : Brain
    {
        public const int Pinsecte = 4748, CarapaceGardee = 4746, EtatIndeplacable = 97;
        public const double PartRenvoyee = 0.5;
        public const int PoisonParLigne = 120;

        readonly Dictionary<FightActor, int> m_poisons = new Dictionary<FightActor, int>();

        public LucraneBrain(AIFighter fighter)
            : base(fighter)
        {
            // PADGREF 23/10 (retour utilisateur : « la Lucrane ne lance toujours pas Carapace gardée ») : l'effet marchait mais ne se
            // voyait pas. Le passif officiel 4746 est posé pour l'affichage (icône + infobulle) ; ses effets restent neutralisés
            // (CastSpell.cs : 4745 / 4744, TriggerBuff.cs : Indéplaçable permanent) et codés ici.
            SortPassif.Poser(fighter, CarapaceGardee);
            fighter.DamageInflicted += CoupRecu;
            fighter.Fight.TurnStarted += DebutTour;
            fighter.Fight.FightEnded += f => { f.TurnStarted -= DebutTour; m_poisons.Clear(); };
        }

        void CoupRecu(FightActor lucrane, Damage degats)
        {
            var attaquant = degats?.Source;
            if (attaquant == null || attaquant == lucrane || !lucrane.IsEnnemyWith(attaquant) || degats.Generated || degats.ReflectedDamages || degats.Amount <= 0)
                return;
            if (lucrane.Fight.State != FightState.Fighting)
                return;

            if (degats.School == Game.Effects.EffectSchoolEnum.Pushback)
            {
                if (!lucrane.HasState(EtatIndeplacable))
                    EtatsBoss.AjouterPourTours(lucrane, lucrane, EtatIndeplacable, CarapaceGardee, 1);
                return;
            }
            if (lucrane.HasState(EtatIndeplacable) || !attaquant.IsAlive())
                return;

            var renvoi = (int)Math.Ceiling(degats.Amount * PartRenvoyee);
            attaquant.InflictDamage(new Damage(renvoi)
            {
                Source = lucrane,
                School = degats.School,
                IgnoreDamageBoost = true,
                IgnoreDamageReduction = true,
                Generated = true
            });
            m_poisons[attaquant] = (m_poisons.TryGetValue(attaquant, out var n) ? n : 0) + 1;
        }

        void DebutTour(IFight fight, FightActor joueur)
        {
            if (joueur == null || !m_poisons.TryGetValue(joueur, out var lignes))
                return;
            m_poisons.Remove(joueur);
            if (!joueur.IsAlive() || fight.State != FightState.Fighting)
                return;
            joueur.InflictDamage(new Damage(PoisonParLigne * lignes)
            {
                Source = Fighter,
                School = Game.Effects.EffectSchoolEnum.Neutral,
                IgnoreDamageBoost = true,
                Generated = true
            });
        }

        public override void Play()
        {
            LancerPinsecte();
            base.Play();
            LancerPinsecte();   // PADGREF 21/10 : aussi après s'être approchée
        }

        void LancerPinsecte()
        {
            if (Fighter.Spells.TryGetValue(Pinsecte, out var sort))
            {
                for (var i = 0; i < 2 && Fighter.IsAlive() && Fighter.IsFighterTurn(); i++)
                {
                    var point = Fighter.Position.Point;
                    var cible = OutilsToxoliath.EnnemisVivants(Fighter)
                        .Where(x => OutilsToxoliath.EnLigne(x.Position.Point, point) && x.Position.Point.ManhattanDistanceTo(point) <= 2)
                        .OrderByDescending(x => x.LifePoints)
                        .FirstOrDefault(x => Fighter.CanCastSpell(sort, x.Cell) == SpellCastResult.OK);
                    if (cible == null || !Fighter.CastSpell(sort, cible.Cell))
                        break;
                }
            }
        }
    }

    /// <summary>
    /// Puceronde : sorts que l'IA normale n'utilisait pas :
    ///   - « Langagement » (4741, à partir du tour 3, relance 3) : se téléporte en ligne de 2 à 7 PO (sans ligne de vue) sur la case
    ///     libre qui attire le plus d'ennemis dans un cercle de 3 cases autour d'elle ;
    ///   - « Brazéro » (4742, mêlée, 2 fois par tour) : dégâts feu = 30 % de ses PV manquants, lancé dès qu'elle a perdu 10 % de ses PV.
    /// </summary>
    [BrainIdentifier((int)MonsterIdEnum.PUCERONDE)]
    public class PuceronndeBrain : Brain
    {
        public const int Langagement = 4741, Brazero = 4742;

        public PuceronndeBrain(AIFighter fighter) : base(fighter) { }

        public override void Play()
        {
            LancerLangagement();
            LancerBrazero();
            base.Play();
            LancerBrazero();
        }

        void LancerLangagement()
        {
            if (!Fighter.Spells.TryGetValue(Langagement, out var sort) || !Fighter.IsFighterTurn())
                return;
            var point = Fighter.Position.Point;
            var ennemis = OutilsToxoliath.EnnemisVivants(Fighter).ToList();
            Cell meilleure = null;
            var meilleurScore = 0;
            foreach (var cell in Fighter.Map.Cells)
            {
                var p = new MapPoint(cell);
                var d = p.ManhattanDistanceTo(point);
                if (d < 2 || d > 7 || !OutilsToxoliath.EnLigne(p, point))
                    continue;
                var score = ennemis.Count(x => x.Position.Point.ManhattanDistanceTo(p) <= 3);
                if (score <= meilleurScore || Fighter.CanCastSpell(sort, cell) != SpellCastResult.OK)
                    continue;
                meilleure = cell;
                meilleurScore = score;
            }
            if (meilleure != null)
                Fighter.CastSpell(sort, meilleure);
        }

        void LancerBrazero()
        {
            if (!Fighter.Spells.TryGetValue(Brazero, out var sort))
                return;
            for (var i = 0; i < 2 && Fighter.IsAlive() && Fighter.IsFighterTurn(); i++)
            {
                if ((Fighter.MaxLifePoints - Fighter.LifePoints) * 10 < Fighter.MaxLifePoints)
                    return;
                var point = Fighter.Position.Point;
                var cible = OutilsToxoliath.EnnemisVivants(Fighter)
                    .Where(x => x.Position.Point.ManhattanDistanceTo(point) == 1)
                    .OrderBy(x => x.LifePoints)
                    .FirstOrDefault(x => Fighter.CanCastSpell(sort, x.Cell) == SpellCastResult.OK);
                if (cible == null || !Fighter.CastSpell(sort, cible.Cell))
                    return;
            }
        }
    }
}
