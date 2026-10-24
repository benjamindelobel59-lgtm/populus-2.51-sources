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

        /// <summary>
        /// PADGREF 22/10 (demande utilisateur) : un boss compte comme VAINCU (sans condition de niveau) si l'une de ces preuves existe :
        ///   - l'objectif « vaincre le boss » (critère EM, enregistré par le serveur à chaque victoire en donjon, même si le succès
        ///     entier n'est pas validé à cause de son niveau : Royalmouth niv. 120, Mansot 140, Ben 150, Obsidiantre 160, Tengu 170…) ;
        ///   - la quête de donjon du boss terminée, ou son objectif « vaincre » déjà passé (619 à 626, 1309 pour les 4 maîtres) ;
        ///   - le succès du donjon (ancienne règle, gardée pour ceux qui l'ont déjà).
        /// Avant, seul le succès comptait : un niveau 50 ne pouvait jamais avancer.
        /// </summary>
        class Boss
        {
            public string Nom;
            public int MonstreId;
            public int QueteId, ObjectifKill;
        }

        static readonly Dictionary<uint, Boss> BossParSucces = new Dictionary<uint, Boss>
        {
            { Royalmouth,   new Boss { Nom = "le Royalmouth", MonstreId = 2854, QueteId = 619, ObjectifKill = 3667 } },
            { Mansot,       new Boss { Nom = "le Mansot Royal", MonstreId = 2848, QueteId = 620, ObjectifKill = 3813 } },
            { Ben,          new Boss { Nom = "Ben le Ripate", MonstreId = 2877, QueteId = 621, ObjectifKill = 4057 } },
            { Obsidiantre,  new Boss { Nom = "l'Obsidiantre", MonstreId = 2924, QueteId = 622, ObjectifKill = 4060 } },
            { Tengu,        new Boss { Nom = "le Tengu Givrefoux", MonstreId = 2967, QueteId = 623, ObjectifKill = 4143 } },
            { TenguFuji,    new Boss { Nom = "la Fuji Givrefoux Nourricière", MonstreId = 3234 } },
            { Korriandre,   new Boss { Nom = "le Korriandre", MonstreId = 2968, QueteId = 624, ObjectifKill = 4256 } },
            { Kolosso,      new Boss { Nom = "le Kolosso", MonstreId = 2986, QueteId = 625, ObjectifKill = 4281 } },
            { Glourseleste, new Boss { Nom = "le Glourséleste", MonstreId = 2864, QueteId = 626, ObjectifKill = 4284 } },
            { Nileza,       new Boss { Nom = "Nileza", MonstreId = 3397, QueteId = 1309, ObjectifKill = 7164 } },
            { Sylargh,      new Boss { Nom = "Sylargh", MonstreId = 3409, QueteId = 1309, ObjectifKill = 7166 } },
            { Klime,        new Boss { Nom = "Klime", MonstreId = 3384, QueteId = 1309, ObjectifKill = 7168 } },
            { Frizz,        new Boss { Nom = "Missiz Frizz", MonstreId = 3391, QueteId = 1309, ObjectifKill = 7170 } },
        };

        static bool BossVaincu(Character c, uint succes)
        {
            if (c.Achievement.AchievementIsCompleted(succes))
                return true;
            if (!BossParSucces.TryGetValue(succes, out var b))
                return false;
            // objectif de succès « EM>monstre,0,d » validé à la victoire (sans condition de niveau)
            if (c.Achievement.ContainsCriterion("EM>" + b.MonstreId + ",0,d"))
                return true;
            if (b.QueteId == 0)
                return false;
            var quete = c.Quests.FirstOrDefault(q => q.Template != null && q.Template.Id == b.QueteId);
            if (quete == null)
                return false;
            if (quete.Finished)
                return true;
            var etape = quete.CurrentStep;
            if (etape == null)
                return false;
            var objectif = etape.Objectives.FirstOrDefault(o => o.ObjectiveRecord.ObjectiveId == b.ObjectifKill);
            if (objectif != null)
                return objectif.Finished;
            // objectif « vaincre » dans une étape déjà passée : vaincu si l'étape en cours vient après la sienne
            var etapes = quete.Template.Steps;
            if (etapes == null)
                return false;
            var iKill = System.Array.FindIndex(etapes, st => st.Objectives != null && st.Objectives.Any(o => o.Id == b.ObjectifKill));
            var iEnCours = System.Array.FindIndex(etapes, st => st.Id == etape.Id);
            return iKill >= 0 && iEnCours > iKill;
        }

        static string NomBoss(uint succes) => BossParSucces.TryGetValue(succes, out var b) ? b.Nom
            : "« " + (AchievementManager.Instance.TryGetAchievement(succes)?.Name ?? ("succès " + succes)) + " »";

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
            { 603, ConditionKorriandre },   // Village enseveli (PADGREF 22/10, demande utilisateur : Tengu OU Tengu + Fuji, comme son zaap)
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

        // PADGREF 23/10 (demande utilisateur) : les personnages ayant reçu l'accès libre (commande « .frigost acces ») sont exemptés
        // de toutes les conditions de Frigost (niveau, boss vaincus), comme le staff en mode dieu / invisible.
        static bool EstStaff(Character c) => (c?.Account != null && c.Account.UserGroupId >= (int)RoleEnum.Moderator && (c.GodMode || c.Invisible)) || AccesLibre(c);

        /// <summary>PADGREF 23/10 : personnages avec l'accès libre à Frigost (testeurs), table mystic_acces_frigost (gardée au redémarrage).</summary>
        static HashSet<int> s_accesLibre;
        static readonly object s_verrou = new object();
        static Stump.ORM.Database Base => WorldServer.Instance.DBAccessor.Database;

        static HashSet<int> Libres
        {
            get
            {
                lock (s_verrou)
                {
                    if (s_accesLibre == null)
                    {
                        Base.Execute("CREATE TABLE IF NOT EXISTS mystic_acces_frigost (CharacterId INT NOT NULL PRIMARY KEY, " +
                                     "Depuis DATETIME NOT NULL, Par VARCHAR(64) NOT NULL DEFAULT '') DEFAULT CHARSET=utf8");
                        s_accesLibre = new HashSet<int>(Base.Fetch<int>("SELECT CharacterId FROM mystic_acces_frigost"));
                    }
                    return s_accesLibre;
                }
            }
        }

        public static bool AccesLibre(Character c)
        {
            if (c == null)
                return false;
            var libres = Libres;
            lock (s_verrou)
                return libres.Contains(c.Id);
        }

        /// <summary>Donne (<paramref name="oui"/>) ou retire l'accès libre à toutes les zones de Frigost.</summary>
        public static void DonnerAccesLibre(Character c, bool oui, string par)
        {
            var libres = Libres;
            lock (s_verrou)
            {
                if (oui)
                    libres.Add(c.Id);
                else
                    libres.Remove(c.Id);
            }
            if (oui)
                Base.Execute("REPLACE INTO mystic_acces_frigost (CharacterId, Depuis, Par) VALUES (@0, NOW(), @1)", c.Id, par ?? "");
            else
                Base.Execute("DELETE FROM mystic_acces_frigost WHERE CharacterId = @0", c.Id);
        }

        static bool EstFrigost(Map map) => map?.SubArea != null && ZonesFrigost.Contains(map.SubArea.Record.AreaId);

        // PADGREF 24/10 (demande utilisateur) : quêtes d'accès. Frigost : « Bienvenue à Frigost » (649), donnée par le Capitaine Ardier
        // (baie de Cania) au départ et terminée en lui reparlant à l'arrivée. Otomaï : « Le nouveau monde » (231, donnée à la création du
        // personnage) puis « L'île des naufragés » (232), avec le Capitaine Kradoc. Sans elles : ni maison, ni enclos, ni zaap, ni prisme
        // de l'île (l'accès à l'île elle-même passe par ces capitaines).
        public const int QueteFrigost = 649;
        public static readonly int[] QuetesOtomai = { 231, 232 };
        const int ZoneOtomai = 46;

        static bool EstOtomai(Map map) => map?.SubArea != null && map.SubArea.Record.AreaId == ZoneOtomai;
        static bool QueteFinie(Character c, int id) => c.Quests.Any(q => q.Finished && q.Id == id);

        /// <summary>Quête d'accès de l'île de la carte faite ? (vrai hors Frigost / Otomaï)</summary>
        public static bool QueteAcces(Character c, Map map, bool message)
        {
            if (c == null || map == null || EstStaff(c))
                return true;
            if (EstFrigost(map) && !QueteFinie(c, QueteFrigost))
            {
                if (message)
                    Message(c, "Accès refusé : termine d'abord la quête « Bienvenue à Frigost » (Capitaine Ardier, baie de Cania).");
                return false;
            }
            if (EstOtomai(map) && !QuetesOtomai.All(q => QueteFinie(c, q)))
            {
                if (message)
                    Message(c, "Accès refusé : termine d'abord les quêtes « Le nouveau monde » et « L'île des naufragés » (Capitaine Kradoc, port de Sufokia).");
                return false;
            }
            return true;
        }

        /// <summary>PADGREF 24/10 : téléportation par prisme vers cette carte : quête d'accès + règles de Frigost (niveau, zones).</summary>
        public static bool PrismeAutorise(Character c, Map map, bool message)
        {
            if (map == null || EstStaff(c))
                return true;
            if (!QueteAcces(c, map, message))
                return false;
            if (ZoneAutorisee(c, map))
                return true;
            if (message)
                Message(c, "Accès refusé : cette zone de Frigost n'est pas encore débloquée.");
            return false;
        }

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
            if (zaap == null || EstStaff(c))
                return true;
            if (!QueteAcces(c, zaap, message))
                return false;
            if (!EstFrigost(zaap))
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
            if (EstStaff(c))
                return true;
            if (!QueteAcces(c, map, message))
                return false;
            if (!EstFrigost(map))
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
            // PADGREF 24/10 (demande utilisateur) : maisons / enclos de chaque zone de Frigost = mêmes conditions que l'accès à la zone
            if (map.SubArea != null && ParSousZone.TryGetValue(map.SubArea.Id, out var condition))
                return Verifier(c, condition, message);
            return true;
        }

        const int SousZoneVillageEnseveli = 603;

        static bool Verifier(Character c, uint[][] condition, bool message)
        {
            if (condition.Any(alt => alt.All(id => BossVaincu(c, id))))
                return true;

            if (message)
            {
                var texte = condition.Length > 1
                    ? "il faut d'abord avoir vaincu " + string.Join(" ou ", condition.Select(a => NomBoss(a[0])))
                    : "il faut d'abord avoir vaincu " + string.Join(", ", condition[0].Where(id => !BossVaincu(c, id)).Select(NomBoss));
                Message(c, "Accès refusé : " + texte + " (quête de son donjon).");
            }
            return false;
        }

        static void Message(Character c, string texte) => c?.SendServerMessage("<b>" + texte + "</b>", Couleur);
    }
}
