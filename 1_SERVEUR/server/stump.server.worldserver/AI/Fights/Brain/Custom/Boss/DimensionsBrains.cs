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
        public ChaloeilBrain(AIFighter fighter)
            : base(fighter)
        {
            SortPassif.Poser(fighter, 6231, 1);
            fighter.Fight.TurnStarted += VerifierInvulnerabilite;
            fighter.Fight.FightEnded += Fin;
            // MYSTIC 18/10 : La Routourne (6301) n'est plus posée ici : ses 5 niveaux sont les variantes des 5 monstres du Grand Jeu
            // (voir RoutourneBrain dans PassifsAuditDonjonsBrains.cs) ; avant, le Chalœil (déjà invulnérable) en recevait une au hasard
        }

        /// <summary>MYSTIC 20/10 (décision utilisateur) : le Chalœil ne devient vulnérable qu'au tour 20 : jusque-là, s'il a perdu son
        /// invulnérabilité (désenvoûtement, désactivation…), elle est remise (non désenvoûtable) ; au tour 20 elle est retirée.</summary>
        const int TourDeblocage = 20, JeuDuChaloeil = 6231;

        void VerifierInvulnerabilite(Game.Fights.IFight fight, FightActor joueur)
        {
            if (!Fighter.IsAlive())
                return;
            if (fight.TimeLine.RoundNumber < TourDeblocage)
            {
                EtatsBoss.RetirerDesactivations(Fighter, EtatsBoss.Invulnerable);
                if (!Fighter.HasState(EtatsBoss.Invulnerable))
                    using (fight.StartSequence(Stump.DofusProtocol.Enums.SequenceTypeEnum.SEQUENCE_TRIGGERED))
                        EtatsBoss.Ajouter(Fighter, Fighter, EtatsBoss.Invulnerable, JeuDuChaloeil);
                return;
            }

            fight.TurnStarted -= VerifierInvulnerabilite;
            using (fight.StartSequence(Stump.DofusProtocol.Enums.SequenceTypeEnum.SEQUENCE_TRIGGERED))
                foreach (var buff in System.Linq.Enumerable.ToArray(Fighter.GetBuffs(x => x is Game.Fights.Buffs.StateBuff e && e.State.Id == EtatsBoss.Invulnerable)))
                    Fighter.RemoveBuff(buff);
        }

        void Fin(Game.Fights.IFight fight)
        {
            fight.FightEnded -= Fin;
            fight.TurnStarted -= VerifierInvulnerabilite;
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

    // Roi Nidas, Boubourses, Kamastérisk : voir NidasBrains.cs (MYSTIC 20/10)
    [BrainIdentifier(3719)] public class KamasteriskObeseBrain : BrainPassif { public KamasteriskObeseBrain(AIFighter f) : base(f, 4514) { } }

    // Piloztère : voir NidasBrains.cs

    /// <summary>Ventre de la Baleine : les Restaurations (renaissance des microbes) de chaque monstre.</summary>
    [BrainIdentifier(3823)] public class TabacilleBrain : BrainPassif { public TabacilleBrain(AIFighter f) : base(f, 4936) { } }
    [BrainIdentifier(3824)] public class VerminoculeBrain : BrainPassif { public VerminoculeBrain(AIFighter f) : base(f, 4941) { } }
    [BrainIdentifier(3825)] public class BacterribBrain : BrainPassif { public BacterribBrain(AIFighter f) : base(f, 4946) { } }
    [BrainIdentifier(3826)] public class VirustineBrain : BrainPassif { public VirustineBrain(AIFighter f) : base(f, 4951) { } }
    [BrainIdentifier(3827)] public class PataugermeBrain : BrainPassif { public PataugermeBrain(AIFighter f) : base(f, 4956) { } }

    /// <summary>Protozorreur : Restauration infectieuse (Déphasage, Attraction cellulaire).</summary>
    [BrainIdentifier(3828)] public class ProtozorreurBrain : BrainPassif { public ProtozorreurBrain(AIFighter f) : base(f, 4972) { } }
}
