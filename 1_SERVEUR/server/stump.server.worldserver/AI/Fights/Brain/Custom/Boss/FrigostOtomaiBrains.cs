using System;
using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Fights;

namespace Stump.Server.WorldServer.AI.Fights.Brain.Custom.Boss
{
    // MYSTIC 17/10 : gros chantiers Frigost / Otomaï (AUDIT_MECANIQUES.md). Les passifs officiels 2.51 portent toute la mécanique
    // (déclencheurs, états, sous-sorts) ; ils n'étaient lancés par rien. Effets manquants ajoutés au moteur : confusion 770-775,
    // masque « W », repousse / attire 1021 / 1022, dommages Terre fixes 1063.

    /// <summary>
    /// Comte Harebourg : Carillon (invulnérable ; Pendultimatum = confusion de qui le frappe au corps à corps ; Bon vieux temps =
    /// glyphe à chaque début de tour ; Comtoise = confusion des joueurs selon leurs PV ; Alternance Fuseau / Mi-temps : téléportation
    /// symétrique de l'attaquant, mort si la case est invalide, vulnérable un tour pendant le Mi-temps).
    /// </summary>
    [BrainIdentifier(3416)] public class ComteHarebourgBrain : BrainPassif { public ComteHarebourgBrain(AIFighter f) : base(f, 3642) { } }

    /// <summary>Nocturlabe : Tactique du Tic Tac (répartit les dommages subis au corps à corps autour de lui).</summary>
    [BrainIdentifier(3415)] public class NocturlabeBrain : BrainPassif { public NocturlabeBrain(AIFighter f) : base(f, 3639) { } }

    /// <summary>
    /// Merkator : invulnérable ; Mer Kantile (alliés indéplaçables ; à chaque début de tour, Mer Cenaire : un joueur collé à un
    /// allié prend l'état Friture et rend Merkator vulnérable ; Mer Cure / Mer Veille au corps à corps ; Marée Descendante quand il
    /// redevient invulnérable ; dommages subis à distance réduits).
    /// </summary>
    [BrainIdentifier(3534)]
    public class MerkatorBrain : Brain
    {
        // MYSTIC 18/10 (retour de test, site + données 2.51) : Merkator n'est PAS invulnérable dans le donjon normal (seulement en
        // expédition) ; l'état Invulnérable posé à la main le rendait quasi intuable.
        public MerkatorBrain(AIFighter fighter)
            : base(fighter)
        {
            SortPassif.Poser(fighter, 4009);
        }
    }

    /// <summary>
    /// Maître des Pantins : Mise en scène (invulnérable ; un Acte par marionnette invoquée ; vulnérable quand les 5 Actes sont
    /// tombés). Grade du sort = grade du monstre (grade des marionnettes invoquées).
    /// </summary>
    [BrainIdentifier(3476)]
    public class MaitreDesPantinsBrain : Brain
    {
        public MaitreDesPantinsBrain(AIFighter fighter)
            : base(fighter)
        {
            var grade = (fighter as MonsterFighter)?.MonsterGrade?.GradeId ?? 5;
            SortPassif.Poser(fighter, 3875, Math.Max(1, Math.Min(5, (int)grade)));
        }
    }

    /// <summary>Marionnettes : Représentation (à leur mort, l'Acte correspondant tombe chez le Maître des Pantins).</summary>
    [BrainIdentifier(3477)] public class MarionnetteBlancheBrain : Brain { public MarionnetteBlancheBrain(AIFighter f) : base(f) { SortPassif.Poser(f, 3873, 1); } }
    [BrainIdentifier(3478)] public class MarionnetteBleueBrain : Brain { public MarionnetteBleueBrain(AIFighter f) : base(f) { SortPassif.Poser(f, 3873, 2); } }
    [BrainIdentifier(3479)] public class MarionnetteRougeBrain : Brain { public MarionnetteRougeBrain(AIFighter f) : base(f) { SortPassif.Poser(f, 3873, 3); } }
    [BrainIdentifier(3480)] public class MarionnetteVerteBrain : Brain { public MarionnetteVerteBrain(AIFighter f) : base(f) { SortPassif.Poser(f, 3873, 4); } }
    [BrainIdentifier(3481)] public class MarionnetteGriseBrain : Brain { public MarionnetteGriseBrain(AIFighter f) : base(f) { SortPassif.Poser(f, 3873, 5); } }

    /// <summary>Dramak : Spectacle vivant (invulnérable ; vulnérable un tour pour qui l'attaque au corps à corps).</summary>
    [BrainIdentifier(3482)] public class DramakBrain : BrainPassif { public DramakBrain(AIFighter f) : base(f, 3861) { } }

    /// <summary>
    /// Totems du Darkli Moon : aura (grade 1 du sort) : renvoie le sort de l'élément quand le totem est frappé dans son élément,
    /// attire les ennemis vers le totem en fin de tour du Darkli. Les totems ne jouent pas.
    /// </summary>
    public abstract class TotemMoonBrain : Brain
    {
        protected TotemMoonBrain(AIFighter fighter, int sort)
            : base(fighter)
        {
            SortPassif.Poser(fighter, sort, 1);
        }

        public override void Play()
        {
        }
    }

    [BrainIdentifier(2815)] public class TotemFeuBrain : TotemMoonBrain { public TotemFeuBrain(AIFighter f) : base(f, 6163) { } }
    [BrainIdentifier(2816)] public class TotemEauBrain : TotemMoonBrain { public TotemEauBrain(AIFighter f) : base(f, 6167) { } }
    [BrainIdentifier(2817)] public class TotemAirBrain : TotemMoonBrain { public TotemAirBrain(AIFighter f) : base(f, 6166) { } }
    [BrainIdentifier(2818)] public class TotemTerreBrain : TotemMoonBrain { public TotemTerreBrain(AIFighter f) : base(f, 6165) { } }
}
