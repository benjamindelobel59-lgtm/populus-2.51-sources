using System;
using System.Linq;
using Stump.Server.WorldServer.Game.Actors.Fight;

namespace Stump.Server.WorldServer.AI.Fights.Brain.Custom.Boss
{
    // MYSTIC 17/10 : gros chantiers des donjons récents (Dantinéa, Koutoulou, Meno) + Gorgouille du Donjon des Canons.
    // Passifs officiels 2.51 jamais lancés ; zones « case fixe » et « Z » désormais gérées par le moteur.

    /// <summary>Dantinéa : Pacte Maléfique (Grokillages successifs, glyphes sur cases fixes, mort de qui a conclu le marché).</summary>
    [BrainIdentifier(4444)] public class DantineaBrain : BrainPassif { public DantineaBrain(AIFighter f) : base(f, 6709) { } }

    /// <summary>Poulpée : Séduction Abyssale.</summary>
    [BrainIdentifier(4440)] public class PoulpeeBrain : BrainPassif { public PoulpeeBrain(AIFighter f) : base(f, 6690) { } }

    /// <summary>Trydes : Camouflage Sous-marin (invisible quand ils sont frappés).</summary>
    [BrainIdentifier(4441)] public class TrydeBrain : BrainPassif { public TrydeBrain(AIFighter f) : base(f, 6695) { } }
    [BrainIdentifier(4502)] public class TrydeEmbusqueBrain : BrainPassif { public TrydeEmbusqueBrain(AIFighter f) : base(f, 6695) { } }
    [BrainIdentifier(4509)] public class AttaquantTrydeBrain : BrainPassif { public AttaquantTrydeBrain(AIFighter f) : base(f, 6695) { } }
    [BrainIdentifier(4510)] public class ChefTrydeBrain : BrainPassif { public ChefTrydeBrain(AIFighter f) : base(f, 6695) { } }

    /// <summary>Tourthon : Retour de Bulles.</summary>
    [BrainIdentifier(4439)] public class TourthonBrain : BrainPassif { public TourthonBrain(AIFighter f) : base(f, 6689) { } }

    /// <summary>Larve de Koutoulou : Horreur Cosmique + Protection Koutoulouesque.</summary>
    [BrainIdentifier(4453)] public class KoutoulouBrain : BrainPassif { public KoutoulouBrain(AIFighter f) : base(f, 6749, 6820) { } }

    /// <summary>
    /// Capitaine Meno : Appel des Fonds Marins (runes sur cases fixes, grade du sort = grade du monstre) + Mutation Instable.
    /// </summary>
    /// MYSTIC 18/10 (retour de test) : le Meno n'invoquait jamais (le moteur ne déclenche pas les runes de 6804 posées sur des cases vides
    /// et ignore les délais des invocations) : le cerveau invoque lui-même au début de son tour, dès le round 2, selon les sorts de rune
    /// 2.51 du grade (6808 / 6812 / 6813 : effet 181 dont le délai = étape du cycle), sur la case de la rune (ou la plus proche libre).
    /// Les invoqués reçoivent Mutation Instable 6758 par leur cerveau (PassifsAuditDonjonsBrains, à l'arrivée).
    [BrainIdentifier(4460)]
    public class CapitaineMenoBrain : Brain
    {
        readonly int m_grade;
        int m_dernierRound = -1;

        public CapitaineMenoBrain(AIFighter fighter)
            : base(fighter)
        {
            var grade = (fighter as MonsterFighter)?.MonsterGrade?.GradeId ?? 5;
            m_grade = Math.Max(1, Math.Min(5, (int)grade));
            SortPassif.Poser(fighter, 6793, m_grade);
            // MYSTIC 18/10 : Mutation Instable 6758 retirée d'ici : c'est le passif des 5 monstres de la zone (PassifsAuditDonjonsBrains.cs)
        }

        public override void Play()
        {
            InvoquerDuTour();
            base.Play();
        }

        // 6804 (2.51) : niv. 1 -> 6808@288, 6812@359 ; niv. 2-5 -> 6808@344, 6812@288, 6813@359
        int[][] Runes => m_grade == 1
            ? new[] { new[] { 6808, 288 }, new[] { 6812, 359 } }
            : new[] { new[] { 6808, 344 }, new[] { 6812, 288 }, new[] { 6813, 359 } };

        void InvoquerDuTour()
        {
            var fight = Fighter.Fight;
            int round = fight.TimeLine.RoundNumber;
            if (round < 2 || round == m_dernierRound)
                return;
            m_dernierRound = round;

            var runes = Runes;
            var effets = runes.Select(r => Game.Spells.SpellManager.Instance.GetSpellLevel(r[0], m_grade)?.Effects?
                    .Where(e => e.EffectId == Stump.DofusProtocol.Enums.EffectsEnum.Effect_Summon).ToList()
                    ?? new System.Collections.Generic.List<Game.Effects.Instances.EffectDice>()).ToArray();
            var cycle = 1 + effets.SelectMany(x => x).Select(e => e.Delay).DefaultIfEmpty(0).Max();
            var etape = (round - 2) % cycle;

            using (fight.StartSequence(Stump.DofusProtocol.Enums.SequenceTypeEnum.SEQUENCE_SPELL))
                for (var i = 0; i < runes.Length; i++)
                    foreach (var e in effets[i].Where(x => x.Delay == etape))
                        Invoquer(e.DiceNum, e.DiceFace, runes[i][1]);
        }

        void Invoquer(int monstreId, int gradeMonstre, int caseId)
        {
            var fight = Fighter.Fight;
            var grade = Game.Actors.RolePlay.Monsters.MonsterManager.Instance.GetMonsterGrade(monstreId, gradeMonstre);
            if (grade == null || caseId < 0 || caseId >= fight.Map.Cells.Length)
                return;

            var centre = fight.Map.Cells[caseId];
            var pCentre = new Game.Maps.Cells.MapPoint(centre);
            var cell = fight.IsCellFree(centre) ? centre
                : fight.Map.Cells.Where(c => c.Walkable && !c.NonWalkableDuringFight && fight.IsCellFree(c))
                    .OrderBy(c => new Game.Maps.Cells.MapPoint(c).ManhattanDistanceTo(pCentre)).FirstOrDefault();
            if (cell == null)
                return;

            var invoque = new SummonedMonster(fight.GetNextContextualId(), Fighter.Team, Fighter, grade, cell);
            Handlers.Actions.ActionsHandler.SendGameActionFightSummonMessage(fight.Clients, invoque);
            Fighter.AddSummon(invoque);
            Fighter.Team.AddFighter(invoque);
            fight.TriggerMarks(invoque.Cell, invoque, Game.Fights.Triggers.TriggerType.MOVE);
            logger.Info("[Meno] round {0} : invoque {1} (grade {2}) case {3}", fight.TimeLine.RoundNumber, monstreId, gradeMonstre, cell.Id);
        }
    }

    /// <summary>Gorgouille (Donjon des Canons) : Tachi-Ai (glyphe sur toute la carte hors de son rayon, Corrompu, Lourd).</summary>
    [BrainIdentifier(3137)] public class GorgouilleBrain : BrainPassif { public GorgouilleBrain(AIFighter f) : base(f, 2932) { } }
}
