using System.Collections.Generic;
using System.Linq;
using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Actors.Look;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;

namespace Stump.Server.WorldServer.Game.Maps
{
    /// <summary>
    /// MYSTIC 19/10 : zones sous l'eau = zone 63 « Profondeurs de Sufokia » (sous-zones 860 à 871) + « Épaves Silencieuses » (897)
    /// et donjon « Tour de Bethel » (899).
    /// - on y entre librement : sous l'eau, le personnage porte automatiquement un scaphandre sur la tête (skin du Scaphandre Ojine,
    ///   à la place de celui de son chapeau), retiré en remontant ;
    /// - seules les montures Muldo (famille 5) et les familiers sont autorisés : pas de Dragodinde (1) ni de Volkorne (6),
    ///   ni en montant, ni par double-clic sur un certificat ; en arrivant monté sur l'une d'elles, le joueur descend.
    /// </summary>
    public static class SousLEau
    {
        public const int ZoneSousLEau = 63;
        static readonly int[] SousZonesSousLEau = { 897, 899 };
        public const int FamilleMuldo = 5;
        public const short SkinScaphandre = 1951; // apparence du Scaphandre Ojine (objet 13261)

        static readonly System.Drawing.Color Couleur = System.Drawing.Color.FromArgb(0x3F, 0xA9, 0xF5);
        static readonly HashSet<int> m_scaphandres = new HashSet<int>();

        public static bool Est(Map map) =>
            map != null && ((map.Area != null && map.Area.Id == ZoneSousLEau) || (map.SubArea != null && SousZonesSousLEau.Contains(map.SubArea.Id)));

        public static bool MontureAutorisee(int familleMonture) => familleMonture == FamilleMuldo;

        public static void Message(Character c, string texte) => c?.SendServerMessage("<b>" + texte + "</b>", Couleur);

        /// <summary>Appelé par Character.UpdateLook : sous l'eau, skin du scaphandre à la place de celui du chapeau.</summary>
        public static void AppliquerScaphandre(Character c, ActorLook look)
        {
            var sousLEau = Est(c.Map);
            lock (m_scaphandres)
            {
                if (sousLEau) m_scaphandres.Add(c.Id); else m_scaphandres.Remove(c.Id);
            }
            if (!sousLEau)
                return;

            var chapeau = c.Inventory.TryGetItem(CharacterInventoryPositionEnum.ACCESSORY_POSITION_HAT);
            if (chapeau != null && chapeau.AppearanceId > 0)
                look.RemoveSkin((short)chapeau.AppearanceId);
            look.AddSkin(SkinScaphandre);
        }

        /// <summary>Arrivée sur une carte : scaphandre mis / retiré, et descente des Dragodindes et Volkornes sous l'eau.</summary>
        public static void SurArrivee(Character c, Map map)
        {
            var sousLEau = Est(map);
            bool avait;
            lock (m_scaphandres)
                avait = m_scaphandres.Contains(c.Id);

            if (sousLEau && c.IsRiding && c.EquippedMount != null && !MontureAutorisee(c.EquippedMount.Template.FamilyId))
            {
                c.ForceDismount();
                Message(c, "Sous l'eau, seuls les Muldos et les familiers sont autorisés : vous descendez de votre monture.");
            }

            if (sousLEau != avait)
                c.UpdateLook();
        }

        /// <summary>Monter (ou équiper par certificat) une monture de cette famille sur la carte actuelle : faux (et message) si interdit.</summary>
        public static bool PeutMonter(Character c, int familleMonture)
        {
            if (!Est(c.Map) || MontureAutorisee(familleMonture))
                return true;

            Message(c, "Sous l'eau, seuls les Muldos et les familiers sont autorisés : impossible d'utiliser une Dragodinde ou une Volkorne.");
            return false;
        }
    }
}
