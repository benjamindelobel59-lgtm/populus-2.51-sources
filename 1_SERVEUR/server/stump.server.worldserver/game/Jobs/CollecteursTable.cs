using System.Collections.Generic;

namespace Stump.Server.WorldServer.Game.Jobs
{
    /// <summary>MYSTIC 19/10 : ressource récoltée -> (protecteur officiel, sac de ressource ; 0 = pas de sac en 2.51).
    /// Générée par _OUTILS/serveur/collecteurs/generer_collecteurs.py.</summary>
    public static class CollecteursTable
    {
        public static readonly Dictionary<int, (int Monstre, int Sac)> Ressources = new Dictionary<int, (int, int)>
        {
            { 289, (684, 7941) }, // Blé
            { 303, (711, 7950) }, // Bois de Frêne
            { 312, (693, 7971) }, // Fer
            { 313, (685, 7980) }, // Or
            { 350, (700, 7978) }, // Argent
            { 380, (707, 7967) }, // Menthe Sauvage
            { 395, (706, 7966) }, // Trèfle à 5 feuilles
            { 400, (685, 7942) }, // Orge
            { 401, (687, 7944) }, // Houblon
            { 405, (691, 7948) }, // Malt
            { 421, (704, 7964) }, // Ortie
            { 423, (688, 7945) }, // Lin
            { 425, (692, 7949) }, // Chanvre
            { 428, (705, 7965) }, // Sauge
            { 441, (694, 7972) }, // Cuivre
            { 442, (695, 7973) }, // Bronze
            { 443, (696, 7974) }, // Kobalte
            { 444, (698, 7976) }, // Etain
            { 445, (697, 7975) }, // Manganèse
            { 446, (701, 7979) }, // Bauxite
            { 449, (722, 7960) }, // Bois d'Ebène
            { 460, (714, 7953) }, // Bois de Chêne
            { 461, (718, 7957) }, // Bois d'If
            { 470, (702, 7962) }, // Bois d'Orme
            { 471, (717, 7956) }, // Bois d'Erable
            { 472, (782, 8081) }, // Bois de Charme
            { 473, (712, 7951) }, // Bois de Châtaignier
            { 474, (721, 7959) }, // Bois de Merisier
            { 476, (713, 7952) }, // Bois de Noyer
            { 532, (689, 7946) }, // Seigle
            { 533, (686, 7943) }, // Avoine
            { 593, (708, 7968) }, // Orchidée Freyesque
            { 594, (709, 7969) }, // Edelweiss
            { 598, (729, 7985) }, // Greuvette
            { 600, (735, 7991) }, // Kralamoure
            { 602, (739, 7995) }, // Requin Marteau-Faucille
            { 603, (728, 7984) }, // Poisson-Chaton
            { 1750, (731, 7987) }, // Poisson Pané
            { 1757, (730, 7986) }, // Crabe Sourimi
            { 1779, (736, 7992) }, // Bar Rikain
            { 1782, (726, 7982) }, // Goujon
            { 1784, (737, 7993) }, // Raie Bleue
            { 1794, (733, 7989) }, // Carpe d'Iem
            { 1801, (738, 7994) }, // Perche
            { 1805, (734, 7990) }, // Sardine Brillante
            { 1844, (727, 7983) }, // Truite
            { 1847, (732, 7988) }, // Brochet
            { 2357, (716, 7955) }, // Bois d'Oliviolet
            { 2358, (715, 7954) }, // Bois de Bombu
            { 7013, (719, 7958) }, // Bois de Bambou
            { 7014, (719, 7963) }, // Bois de Bambou Sacré
            { 7016, (719, 7961) }, // Bois de Bambou Sombre
            { 7018, (690, 7947) }, // Riz
            { 7032, (699, 7977) }, // Silicate
            { 7033, (703, 7981) }, // Dolomite
            { 7059, (710, 7970) }, // Graine de Pandouille
            { 7925, (720, 7996) }, // Bois de Kaliptus
            { 11102, (2839, 11103) }, // Perce-Neige
            { 11106, (2841, 11111) }, // Poisskaille
            { 11107, (2842, 11112) }, // Bois de Tremble
            { 11109, (2843, 11113) }, // Frostiz
            { 11110, (2844, 11114) }, // Obsidienne
            { 16385, (4115, 16528) }, // Ginseng
            { 16387, (4116, 16529) }, // Belladone
            { 16389, (4117, 16530) }, // Mandragore
            { 16454, (4119, 16532) }, // Maïs
            { 16456, (4120, 16533) }, // Millet
            { 16461, (4183, 16534) }, // Anguille
            { 16463, (4184, 16535) }, // Dorade Grise
            { 16465, (4185, 16536) }, // Lotte
            { 16467, (4186, 16537) }, // Morue
            { 16469, (4187, 16538) }, // Tanche
            { 16471, (4188, 16539) }, // Espadon
            { 16488, (4118, 16531) }, // Bois de Noisetier
            { 17991, (4483, 18058) }, // Bois d'Aquajou
            { 17992, (4488, 18059) }, // Salikrone
            { 17993, (4484, 18060) }, // Quisnoa
            { 17994, (4486, 18061) }, // Patelle
            { 17995, (4485, 18062) }, // Écume de mer
        };
    }
}
