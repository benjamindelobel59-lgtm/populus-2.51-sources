using System;
using Stump.Server.WorldServer.Game.Actors.Fight;

namespace Stump.Server.WorldServer.AI.Fights.Brain.Custom.Boss
{
    // MYSTIC 17/10 : gros chantiers des dimensions divines (AUDIT_MECANIQUES.md). Passifs officiels 2.51 jamais lancés ; la
    // mécanique est portée par leurs sous-sorts (déclencheurs, états, zones « case fixe » désormais lues).

    /// <summary>
    /// Chalœil : Jeu du Chalœil (invulnérable 20 tours, lancer de dé : les cases du dé tombé deviennent dangereuses, Dés invoqués,
    /// glyphes) + La Routourne (cycle de bonus tous les 3 tours ; une des 5 variantes au hasard).
    /// </summary>
    [BrainIdentifier(4263)]
    public class ChaloeilBrain : Brain
    {
        static readonly Random m_hasard = new Random();

        public ChaloeilBrain(AIFighter fighter)
            : base(fighter)
        {
            SortPassif.Poser(fighter, 6231, 1);
            int variante;
            lock (m_hasard)
                variante = m_hasard.Next(1, 6);
            SortPassif.Poser(fighter, 6301, variante);
        }
    }

    /// <summary>
    /// Ush Galesh rouge : Rouge et Noir (dommages partagés avec l'Ush noir, l'un réduit les dommages à distance, l'autre au corps à
    /// corps, les états s'inversent ; la mort de l'un tue l'autre ; Négativisme) + Matoumorphose (Chachas).
    /// </summary>
    [BrainIdentifier(4264)] public class UshRougeBrain : BrainPassif { public UshRougeBrain(AIFighter f) : base(f, 6264, 6604) { } }

    /// <summary>Ombre : Ex Nihilo.</summary>
    [BrainIdentifier(3564)] public class OmbreBrain : BrainPassif { public OmbreBrain(AIFighter f) : base(f, 4104) { } }

    /// <summary>Silhouette : Détachement.</summary>
    [BrainIdentifier(3565)] public class SilhouetteBrain : BrainPassif { public SilhouetteBrain(AIFighter f) : base(f, 4117) { } }

    /// <summary>Roi Nidas : Boursicotage (50 % de ses dommages subis répartis sur sa Boubourse) + Changement de camp (à sa mort).</summary>
    [BrainIdentifier(3648)] public class RoiNidasBrain : BrainPassif { public RoiNidasBrain(AIFighter f) : base(f, 4920, 4485) { } }

    /// <summary>Boubourse volée par les joueurs : Boursicotage (Cadeau Empoisonné, Coffrage, retour chez Nidas à sa mort).</summary>
    [BrainIdentifier(3649)] public class BoubourseJoueursBrain : BrainPassif { public BoubourseJoueursBrain(AIFighter f) : base(f, 4482) { } }

    /// <summary>Boubourse du Roi Nidas : Boursicotage (Coffrage des joueurs ; à sa mort, change de camp).</summary>
    [BrainIdentifier(3650)] public class BoubourseNidasBrain : BrainPassif { public BoubourseNidasBrain(AIFighter f) : base(f, 4484) { } }

    /// <summary>Kamasterisk : Partoutatice (attire et alourdit les ennemis proches à chaque début de tour).</summary>
    [BrainIdentifier(3653)] public class KamasteriskBrain : BrainPassif { public KamasteriskBrain(AIFighter f) : base(f, 4514) { } }
    [BrainIdentifier(3719)] public class KamasteriskObeseBrain : BrainPassif { public KamasteriskObeseBrain(AIFighter f) : base(f, 4514) { } }

    /// <summary>Piloztère : Austérité (bouclier autour de lui quand il est frappé).</summary>
    [BrainIdentifier(3657)] public class PilozteresBrain : BrainPassif { public PilozteresBrain(AIFighter f) : base(f, 4529) { } }

    /// <summary>Ventre de la Baleine : les Restaurations (renaissance des microbes) de chaque monstre.</summary>
    [BrainIdentifier(3823)] public class TabacilleBrain : BrainPassif { public TabacilleBrain(AIFighter f) : base(f, 4936) { } }
    [BrainIdentifier(3824)] public class VerminoculeBrain : BrainPassif { public VerminoculeBrain(AIFighter f) : base(f, 4941) { } }
    [BrainIdentifier(3825)] public class BacterribBrain : BrainPassif { public BacterribBrain(AIFighter f) : base(f, 4946) { } }
    [BrainIdentifier(3826)] public class VirustineBrain : BrainPassif { public VirustineBrain(AIFighter f) : base(f, 4951) { } }
    [BrainIdentifier(3827)] public class PataugermeBrain : BrainPassif { public PataugermeBrain(AIFighter f) : base(f, 4956) { } }

    /// <summary>Protozorreur : Restauration infectieuse (Déphasage, Attraction cellulaire).</summary>
    [BrainIdentifier(3828)] public class ProtozorreurBrain : BrainPassif { public ProtozorreurBrain(AIFighter f) : base(f, 4972) { } }
}
