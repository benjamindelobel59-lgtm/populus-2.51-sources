using System;
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
    [BrainIdentifier(4460)]
    public class CapitaineMenoBrain : Brain
    {
        public CapitaineMenoBrain(AIFighter fighter)
            : base(fighter)
        {
            var grade = (fighter as MonsterFighter)?.MonsterGrade?.GradeId ?? 5;
            SortPassif.Poser(fighter, 6793, Math.Max(1, Math.Min(5, (int)grade)));
            SortPassif.Poser(fighter, 6758, 1);
        }
    }

    /// <summary>Gorgouille (Donjon des Canons) : Tachi-Ai (glyphe sur toute la carte hors de son rayon, Corrompu, Lourd).</summary>
    [BrainIdentifier(3137)] public class GorgouilleBrain : BrainPassif { public GorgouilleBrain(AIFighter f) : base(f, 2932) { } }
}
