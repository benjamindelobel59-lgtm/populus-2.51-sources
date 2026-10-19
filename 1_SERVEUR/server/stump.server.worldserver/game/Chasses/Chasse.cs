using System.Collections.Generic;
using System.Linq;

namespace Stump.Server.WorldServer.Game.Chasses
{
    /// <summary>
    /// MYSTIC 19/10 : une chasse au trésor en cours (sauvegardée en JSON dans mystic_chasses, une ligne par personnage).
    /// Cahier des charges : _OUTILS/serveur/chasses/CAHIER_DES_CHARGES.md
    /// </summary>
    public class Chasse
    {
        /// <summary>0 = classique, 1 = portail, 2 = légendaire (TreasureHuntTypeEnum).</summary>
        public int Type { get; set; }
        public int Niveau { get; set; }
        /// <summary>Chasse légendaire : id dans mystic_chasses_legendaires (0 sinon).</summary>
        public int LegendaireId { get; set; }
        /// <summary>Chasse aux portails (Type 1) : AreaId de la dimension (53 Enutrosor, 54 Srambad, 55 Xélorium, 59 Ecaflipus).</summary>
        public int Dimension { get; set; }
        public int CarteDepart { get; set; }
        public List<List<Indice>> Etapes { get; set; } = new List<List<Indice>>();
        /// <summary>Étape en cours ; = Etapes.Count : étape du combat final.</summary>
        public int EtapeCourante { get; set; }
        /// <summary>Jalons posés dans l'étape en cours (carte où le joueur a posé chaque jalon).</summary>
        public List<int> Jalons { get; set; } = new List<int>();
        /// <summary>Jalons faux de la dernière fouille ratée (affichés en rouge).</summary>
        public List<int> JalonsFaux { get; set; } = new List<int>();
        public int Essais { get; set; } = ChasseManager.EssaisParEtape;
        /// <summary>Nombre de coffres gagnés : 1, 2 (passage par une cité) ou 4 (Frigost, zone du Ben le Ripate ou plus loin).</summary>
        public int Coffres { get; set; } = 1;

        public bool EtapeDuCombat => EtapeCourante >= Etapes.Count;

        public List<Indice> IndicesEnCours => EtapeDuCombat ? new List<Indice>() : Etapes[EtapeCourante];

        /// <summary>Indice à trouver maintenant (null si tous les jalons de l'étape sont posés).</summary>
        public Indice IndiceCourant => !EtapeDuCombat && Jalons.Count < IndicesEnCours.Count ? IndicesEnCours[Jalons.Count] : null;

        /// <summary>Carte de départ de l'étape en cours : départ de la chasse ou dernier indice de l'étape précédente.</summary>
        public int DepartEtape => EtapeCourante == 0 ? CarteDepart : Etapes[System.Math.Min(EtapeCourante, Etapes.Count) - 1].Last().Carte;
    }

    public class Indice
    {
        /// <summary>Direction depuis l'indice précédent : 0 est, 2 sud, 4 ouest, 6 nord (le client n'a que ces 4 flèches).</summary>
        public int Direction { get; set; }
        /// <summary>Nombre de cartes (1 à 10) depuis l'indice précédent.</summary>
        public int Distance { get; set; }
        /// <summary>Point d'intérêt affiché (PointOfInterest.d2o).</summary>
        public int PointInteret { get; set; }
        public int Carte { get; set; }
    }
}
