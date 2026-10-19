using System.Collections.Generic;
using System.Linq;
using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Achievements;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;

namespace Stump.Server.WorldServer.Game.Maps
{
    /// <summary>
    /// MYSTIC 19/10 : conditions d'accès à Frigost (ordre donné par l'utilisateur), vérifiées à CHAQUE changement de carte
    /// (Character.Teleport) : PNJ transporteurs, gardiens de donjon (entrée, trousseau, reprise), interactifs (ex. passage de la
    /// Forêt des pins perdus vers le Berceau d'Alma), skis, potions, bords de carte… et dans la liste des zaaps (ZaapDialog).
    /// La règle s'applique en entrant dans une sous-zone depuis une AUTRE sous-zone (on n'est jamais bloqué à l'intérieur),
    /// pas au staff en mode dieu / invisible.
    ///   - Île de Frigost (zone 48) et Île de Sakaï (zone 49) : niveau 50 (maisons et enclos de Frigost aussi) ;
    ///   - Royalmouth + Mansot Royal -> Ben le Ripate (Berceau d'Alma + donjon) ;
    ///   - Ben -> Obsidiantre (Larmes d'Ouronigride + donjon) ;
    ///   - Obsidiantre -> Tengu (Crevasse Perge, Cavernes des Givrefoux + donjon) ;
    ///   - Tengu OU Tengu Fuji -> zaap du Village enseveli + zone du Korriandre (Forêt pétrifiée) + donjon ;
    ///   - Korriandre -> Kolosso (Crocs de verre + donjon) ;
    ///   - Kolosso -> Gloursons (Mont Torrideau + donjon) ;
    ///   - Glourséleste -> Frigost III (zaap + zones du château + donjons Nileza, Sylargh, Klime, Missiz Frizz) ;
    ///   - Missiz Frizz + Sylargh + Nileza + Klime -> zone et donjon du Comte Harebourg.
    /// </summary>
    public static class AccesFrigost
    {
        public const int NiveauMinimum = 50;
        static readonly int[] ZonesFrigost = { 48, 49 };

        // succès de donjon (achievements_templates)
        const uint Royalmouth = 72, Mansot = 59, Ben = 58, Obsidiantre = 63, Tengu = 31, TenguFuji = 134, Korriandre = 23, Kolosso = 30,
            Glourseleste = 20, Nileza = 887, Sylargh = 877, Klime = 882, Frizz = 872;

        /// <summary>Une condition = une liste d'alternatives (OU), chacune une liste de succès à avoir tous (ET).</summary>
        static uint[][] Et(params uint[] succes) => new[] { succes };
        static uint[][] Ou(params uint[] succes) => succes.Select(x => new[] { x }).ToArray();

        static readonly uint[][] ConditionBen = Et(Royalmouth, Mansot);
        static readonly uint[][] ConditionObsidiantre = Et(Ben);
        static readonly uint[][] ConditionTengu = Et(Obsidiantre);
        static readonly uint[][] ConditionKorriandre = Ou(Tengu, TenguFuji);
        static readonly uint[][] ConditionKolosso = Et(Korriandre);
        static readonly uint[][] ConditionGloursons = Et(Kolosso);
        static readonly uint[][] ConditionFrigost3 = Et(Glourseleste);
        static readonly uint[][] ConditionComte = Et(Frizz, Sylargh, Nileza, Klime);

        /// <summary>sous-zone -> condition</summary>
        static readonly Dictionary<int, uint[][]> ParSousZone = new Dictionary<int, uint[][]>
        {
            { 609, ConditionBen },          // Berceau d'Alma (zone du Ben, gardien Fantômahawk)
            { 619, ConditionBen },          // Épave du Grolandais violent
            { 610, ConditionObsidiantre },  // Larmes d'Ouronigride (gardien Lave Azza)
            { 620, ConditionObsidiantre },  // Hypogée de l'Obsidiantre
            { 606, ConditionTengu },        // Crevasse Perge (gardien Givrihaltès)
            { 622, ConditionTengu },        // Cavernes des Givrefoux
            { 621, ConditionTengu },        // Tanière Givrefoux (Tengu)
            { 605, ConditionKorriandre },   // Forêt pétrifiée (gardienne Sylvie Barbe)
            { 623, ConditionKorriandre },   // Antre du Korriandre
            { 608, ConditionKolosso },      // Crocs de verre (Professeur Xa)
            { 624, ConditionKolosso },      // Cavernes du Kolosso
            { 611, ConditionGloursons },    // Mont Torrideau
            { 752, ConditionGloursons },    // Antichambre des Gloursons
            { 753, ConditionGloursons },    // Ruche des Gloursons
            { 616, ConditionFrigost3 },     // Entrée du château de Harebourg
            { 785, ConditionFrigost3 },     // Jardins d'Hiver
            { 786, ConditionFrigost3 },     // Remparts à vent
            { 787, ConditionFrigost3 },     // Tannerie Écarlate
            { 788, ConditionFrigost3 },     // Bastion des froides légions
            { 790, ConditionFrigost3 },     // Laboratoire de Nileza
            { 791, ConditionFrigost3 },     // Transporteur de Sylargh
            { 792, ConditionFrigost3 },     // Salons privés de Klime
            { 793, ConditionFrigost3 },     // Forgefroide de Missiz Frizz
            { 789, ConditionComte },        // Tour de la Clepsydre
            { 795, ConditionComte },        // Pied de la Tour de la Clepsydre
            { 794, ConditionComte },        // Donjon du Comte Harebourg
        };

        /// <summary>carte de zaap -> condition (en plus du niveau 50)</summary>
        static readonly Dictionary<int, uint[][]> ParZaap = new Dictionary<int, uint[][]>
        {
            { 54172489, ConditionKorriandre },  // zaap du Village enseveli
            { 108789760, ConditionFrigost3 },   // zaap de l'entrée du château de Harebourg
        };

        static readonly System.Drawing.Color Couleur = System.Drawing.Color.FromArgb(0x7F, 0xD3, 0xF7);

        static bool EstStaff(Character c) => c?.Account != null && c.Account.UserGroupId >= (int)RoleEnum.Moderator && (c.GodMode || c.Invisible);

        static bool EstFrigost(Map map) => map?.SubArea != null && ZonesFrigost.Contains(map.SubArea.Record.AreaId);

        public static bool PeutEntrer(Character c, Map destination, bool message = true)
        {
            var sz = destination?.SubArea;
            if (sz == null || c.Map?.SubArea?.Id == sz.Id || EstStaff(c) || !EstFrigost(destination))
                return true;

            if (c.Level < NiveauMinimum && !EstFrigost(c.Map))
            {
                if (message)
                    Message(c, $"Frigost est réservée aux aventuriers de niveau {NiveauMinimum} et plus.");
                return false;
            }

            return !ParSousZone.TryGetValue(sz.Id, out var condition) || Verifier(c, condition, message);
        }

        /// <summary>Zaap : destination proposée / utilisable ? (message seulement si <paramref name="message"/>)</summary>
        /// <summary>
        /// MYSTIC 19/10 (chasses au trésor) : le personnage a-t-il le droit d'être sur cette carte, d'où qu'il vienne (sans message) ?
        /// Sert à générer les chasses et l'autopilote sans jamais passer par une zone de Frigost interdite.
        /// </summary>
        public static bool ZoneAutorisee(Character c, Map map)
        {
            if (map?.SubArea == null || EstStaff(c) || !EstFrigost(map))
                return true;
            if (c.Level < NiveauMinimum)
                return false;
            return !ParSousZone.TryGetValue(map.SubArea.Id, out var condition) || Verifier(c, condition, false);
        }

        /// <summary>
        /// MYSTIC 19/10 : conditions propres à UNE réponse de PNJ (npcs_replies.Id), en plus de la règle de la sous-zone :
        /// transporteur de la compagnie Asfog et Fils (message 8145) « Se rendre sur l'île de Sakaï » = Royalmouth + Mansot Royal
        /// (seulement ce PNJ : le bateau et les autres accès à Sakaï ne changent pas).
        /// </summary>
        static readonly Dictionary<int, uint[][]> ParReponse = new Dictionary<int, uint[][]>
        {
            { 24, ConditionBen },
        };

        public static bool ReponseAutorisee(Character c, int replyId, bool message)
        {
            if (EstStaff(c) || !ParReponse.TryGetValue(replyId, out var condition))
                return true;
            return Verifier(c, condition, message);
        }

        public static bool ZaapAutorise(Character c, Map zaap, bool message)
        {
            if (zaap == null || EstStaff(c) || !EstFrigost(zaap))
                return true;

            if (c.Level < NiveauMinimum)
            {
                if (message)
                    Message(c, $"Frigost est réservée aux aventuriers de niveau {NiveauMinimum} et plus.");
                return false;
            }

            if (ParZaap.TryGetValue(zaap.Id, out var condition) && !Verifier(c, condition, message))
                return false;

            return !ParSousZone.TryGetValue(zaap.SubArea.Id, out var cz) || Verifier(c, cz, message);
        }

        /// <summary>Maisons et enclos de Frigost : niveau 50 (ailleurs, aucune limite).</summary>
        public static bool NiveauMaisonEnclos(Character c, Map map, bool message)
        {
            if (!EstFrigost(map) || EstStaff(c))
                return true;
            if (c.Level < NiveauMinimum)
            {
                if (message)
                    Message(c, $"Les maisons et les enclos de Frigost sont réservés aux aventuriers de niveau {NiveauMinimum} et plus.");
                return false;
            }
            // MYSTIC 19/10 (demande utilisateur) : maisons / enclos du Village enseveli (603) = mêmes conditions que l'accès à la zone
            // (zaap du Village enseveli : Tengu OU Tengu Fuji)
            if (map.SubArea?.Id == SousZoneVillageEnseveli)
                return Verifier(c, ConditionKorriandre, message);
            return true;
        }

        const int SousZoneVillageEnseveli = 603;

        static bool Verifier(Character c, uint[][] condition, bool message)
        {
            if (condition.Any(alt => alt.All(id => c.Achievement.AchievementIsCompleted(id))))
                return true;

            if (message)
            {
                string Nom(uint id) => "« " + (AchievementManager.Instance.TryGetAchievement(id)?.Name ?? ("succès " + id)) + " »";
                var texte = condition.Length > 1
                    ? "il faut d'abord avoir validé l'un de ces succès : " + string.Join(" ou ", condition.Select(a => Nom(a[0])))
                    : "il faut d'abord avoir validé le succès " + string.Join(", ", condition[0].Where(id => !c.Achievement.AchievementIsCompleted(id)).Select(Nom));
                Message(c, "Accès refusé : " + texte + ".");
            }
            return false;
        }

        static void Message(Character c, string texte) => c?.SendServerMessage("<b>" + texte + "</b>", Couleur);
    }
}
