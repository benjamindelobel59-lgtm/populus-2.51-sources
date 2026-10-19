using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace Stump.Server.BaseServer
{
    /// <summary>
    /// Habillage « Mystic » de la console des serveurs : bannière animée (ailes d'ange et de démon),
    /// journal en français et coloré, encadré « prêt » en fin de démarrage.
    /// Les fichiers de logs (logs/*.txt) ne changent pas : ils restent en anglais pour les outils de diagnostic.
    /// </summary>
    public static class MysticConsole
    {
        private static readonly object Sync = new object();

        private static readonly string[] Logo =
        {
            "███╗   ███╗██╗   ██╗███████╗████████╗██╗ ██████╗",
            "████╗ ████║╚██╗ ██╔╝██╔════╝╚══██╔══╝██║██╔════╝",
            "██╔████╔██║ ╚████╔╝ ███████╗   ██║   ██║██║     ",
            "██║╚██╔╝██║  ╚██╔╝  ╚════██║   ██║   ██║██║     ",
            "██║ ╚═╝ ██║   ██║   ███████║   ██║   ██║╚██████╗",
            "╚═╝     ╚═╝   ╚═╝   ╚══════╝   ╚═╝   ╚═╝ ╚═════╝",
        };

        // aile d'ange (gauche) et aile de démon (droite, miroir)
        private static readonly string[] AngelWing =
        {
            "▀▀▀▀▀▀▀▀▀▄▄  ",
            " ▀▀▀▀▀▀▀▀▀▄▄ ",
            "  ▀▀▀▀▀▀▀▀▀▄▄",
            "    ▀▀▀▀▀▀▀▄▄",
            "      ▀▀▀▀▀▄▄",
            "         ▀▀▀▄",
        };

        private static readonly string[] DemonWing = AngelWing.Select(Mirror).ToArray();

        private static readonly ConsoleColor[] AngelColors =
            { ConsoleColor.White, ConsoleColor.White, ConsoleColor.Cyan, ConsoleColor.Cyan, ConsoleColor.DarkCyan, ConsoleColor.DarkCyan };

        private static readonly ConsoleColor[] DemonColors =
            { ConsoleColor.Red, ConsoleColor.Red, ConsoleColor.DarkRed, ConsoleColor.DarkRed, ConsoleColor.DarkMagenta, ConsoleColor.DarkMagenta };

        // dégradé des lettres, de l'ange (cyan) au démon (rouge)
        private static readonly ConsoleColor[] LetterGradient =
            { ConsoleColor.Cyan, ConsoleColor.Cyan, ConsoleColor.Blue, ConsoleColor.Magenta, ConsoleColor.Magenta, ConsoleColor.Red, ConsoleColor.Red };

        private static readonly ConsoleColor[] Pulse =
            { ConsoleColor.White, ConsoleColor.Magenta, ConsoleColor.DarkMagenta, ConsoleColor.Magenta };

        /// <summary>« Serveur de jeu » ou « Serveur de connexion », selon l'exécutable lancé.</summary>
        public static string Role
        {
            get
            {
                var name = (Assembly.GetEntryAssembly()?.GetName().Name ?? "").ToLowerInvariant();
                return name.Contains("auth") ? "Serveur de connexion" : "Serveur de jeu";
            }
        }

        private static string Mirror(string line)
        {
            var chars = line.ToCharArray();
            Array.Reverse(chars);
            return new string(chars);
        }

        private static bool Interactive
        {
            get
            {
                try
                {
                    return !Console.IsOutputRedirected && Console.WindowWidth > 0;
                }
                catch
                {
                    return false;
                }
            }
        }

        #region Bannière

        public static void DrawBanner()
        {
            try
            {
                Console.OutputEncoding = Encoding.UTF8;
            }
            catch
            {
            }

            lock (Sync)
            {
                try
                {
                    if (Interactive)
                        DrawAnimatedBanner();
                    else
                        DrawStaticBanner();
                }
                catch
                {
                    // console particulière (redirigée, trop étroite...) : version simple
                    try
                    {
                        Console.ResetColor();
                        Console.WriteLine();
                        DrawStaticBanner();
                    }
                    catch
                    {
                    }
                }
                finally
                {
                    try
                    {
                        Console.ResetColor();
                        Console.CursorVisible = true;
                    }
                    catch
                    {
                    }
                }
            }
        }

        private static string BannerLine(int row)
        {
            return AngelWing[row] + "  " + Logo[row] + "  " + DemonWing[row];
        }

        private static int BannerWidth
        {
            get { return BannerLine(0).Length; }
        }

        private static ConsoleColor ColorAt(int row, int col, ConsoleColor? letterOverride = null)
        {
            var wing = AngelWing[0].Length;
            var logoStart = wing + 2;
            var logoEnd = logoStart + Logo[0].Length;

            if (col < wing)
                return AngelColors[row];
            if (col >= logoEnd + 2)
                return DemonColors[row];

            var c = BannerLine(row)[col];
            if (letterOverride.HasValue && c == '█')
                return letterOverride.Value;

            var gradient = LetterGradient[Math.Min(LetterGradient.Length - 1, (col - logoStart) * LetterGradient.Length / Logo[0].Length)];
            // contours (╗ ║ ═ ...) plus sombres que les pleins
            return c == '█' ? gradient : ConsoleColor.DarkGray;
        }

        private static void DrawAnimatedBanner()
        {
            Console.CursorVisible = false;
            var width = Console.WindowWidth;
            var left = Math.Max(0, (width - BannerWidth) / 2);

            Console.WriteLine();
            for (var i = 0; i < Logo.Length; i++)
                Console.WriteLine();
            var top = Console.CursorTop - Logo.Length;

            // les ailes se déploient depuis le centre pendant que les lettres apparaissent
            var center = BannerWidth / 2;
            for (var step = 0; step <= center + 1; step += 2)
            {
                for (var row = 0; row < Logo.Length; row++)
                {
                    var line = BannerLine(row);
                    for (var d = Math.Max(0, step - 2); d <= step; d++)
                    {
                        foreach (var col in new[] { center - d, center + d })
                        {
                            if (col < 0 || col >= line.Length || left + col >= width)
                                continue;
                            Console.SetCursorPosition(left + col, top + row);
                            Console.ForegroundColor = ColorAt(row, col);
                            Console.Write(line[col]);
                        }
                    }
                }
                Thread.Sleep(9);
            }

            // pulsation des lettres, puis retour au dégradé
            foreach (var color in Pulse.Select(c => (ConsoleColor?)c).Concat(new ConsoleColor?[] { null }))
            {
                for (var row = 0; row < Logo.Length; row++)
                {
                    Console.SetCursorPosition(left, top + row);
                    var line = BannerLine(row);
                    for (var col = 0; col < line.Length && left + col < width; col++)
                    {
                        Console.ForegroundColor = ColorAt(row, col, color);
                        Console.Write(line[col]);
                    }
                }
                Thread.Sleep(70);
            }

            Console.SetCursorPosition(0, top + Logo.Length);
            DrawSubtitle(width);
        }

        private static void DrawStaticBanner()
        {
            Console.WriteLine();
            for (var row = 0; row < Logo.Length; row++)
            {
                var line = BannerLine(row);
                for (var col = 0; col < line.Length; col++)
                {
                    Console.ForegroundColor = ColorAt(row, col);
                    Console.Write(line[col]);
                }
                Console.WriteLine();
            }
            DrawSubtitle(BannerWidth);
        }

        private static void DrawSubtitle(int width)
        {
            var title = "✦  " + Role.ToUpperInvariant() + "  ✦";
            var rule = new string('─', Math.Max(4, (BannerWidth - title.Length - 2) / 2));
            var full = rule + " " + title + " " + rule;
            var pad = Math.Max(0, (width - full.Length) / 2);

            Console.WriteLine();
            Console.Write(new string(' ', pad));
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.Write(rule + " ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(title);
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine(" " + rule);

            var sub = "Dofus 2.51  ·  émulateur Stump  ·  démarrage en cours";
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(new string(' ', Math.Max(0, (width - sub.Length) / 2)) + sub);
            Console.WriteLine();
            Console.ResetColor();
        }

        #endregion

        #region Encadré « prêt »

        public static void DrawReady(int port)
        {
            double seconds;
            try
            {
                seconds = (DateTime.Now - Process.GetCurrentProcess().StartTime).TotalSeconds;
            }
            catch
            {
                seconds = 0;
            }

            var lines = new[]
            {
                "✔  " + Role + " prêt",
                "   Port " + port + "  ·  démarré en " + seconds.ToString("0.0") + " s",
                "   Tapez « help » pour la liste des commandes",
            };
            var inner = lines.Max(l => l.Length) + 4;

            lock (Sync)
            {
                try
                {
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("  ╔" + new string('═', inner) + "╗");
                    for (var i = 0; i < lines.Length; i++)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write("  ║  ");
                        Console.ForegroundColor = i == 0 ? ConsoleColor.White : ConsoleColor.Gray;
                        Console.Write(lines[i].PadRight(inner - 2));
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("║");
                    }
                    Console.WriteLine("  ╚" + new string('═', inner) + "╝");
                    Console.WriteLine();
                }
                catch
                {
                }
                finally
                {
                    try
                    {
                        Console.ResetColor();
                    }
                    catch
                    {
                    }
                }
            }
        }

        #endregion

        #region Journal console

        /// <summary>
        /// Remplace l'affichage console de NLog : messages traduits, colorés, sans le bruit de débogage
        /// (le niveau Debug reste écrit dans les fichiers de logs).
        /// </summary>
        public static void InstallConsoleLog()
        {
            var config = LogManager.Configuration;
            if (config == null)
                return;

            // l'ancienne cible console reste déclarée mais n'a plus de règle : elle n'affiche plus rien
            // (pas de RemoveTarget : NLog 4.5 ne mémorise pas le nom des cibles ajoutées par AddTarget(nom, cible))
            foreach (var rule in config.LoggingRules.Where(r => r.Targets.Any(t => t is ColoredConsoleTarget || t is ConsoleTarget)).ToList())
                config.LoggingRules.Remove(rule);

            var mystic = new MysticConsoleTarget { Name = "mystic" };
            config.AddTarget("mystic", mystic);
            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Info, mystic));

            LogManager.Configuration = config;
        }

        internal static void WriteLog(LogEventInfo e)
        {
            var message = Translate(e.FormattedMessage ?? "");
            if (e.Exception != null)
                message += " : " + e.Exception.Message;

            // erreurs sans gravité signalées comme telles par le noyau (pas de dossier plugins : normal)
            var level = message.StartsWith("Aucun dossier d'extensions", StringComparison.Ordinal) ? LogLevel.Info : e.Level;

            string icon;
            ConsoleColor iconColor, textColor;
            if (level >= LogLevel.Error)
            {
                icon = "✖";
                iconColor = ConsoleColor.Red;
                textColor = ConsoleColor.Red;
            }
            else if (level == LogLevel.Warn)
            {
                icon = "▲";
                iconColor = ConsoleColor.Yellow;
                textColor = ConsoleColor.Yellow;
            }
            else
            {
                icon = "•";
                iconColor = ConsoleColor.Magenta;
                textColor = ConsoleColor.Gray;
            }

            lock (Sync)
            {
                try
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write(" " + e.TimeStamp.ToString("HH:mm:ss") + " │ ");
                    Console.ForegroundColor = iconColor;
                    Console.Write(icon + " ");

                    // « Chargement : Sorts » -> le nom du module ressort en blanc
                    var sep = message.IndexOf(" : ", StringComparison.Ordinal);
                    if (textColor == ConsoleColor.Gray && message.StartsWith("Chargement : ", StringComparison.Ordinal) && sep > 0)
                    {
                        Console.ForegroundColor = textColor;
                        Console.Write(message.Substring(0, sep + 3));
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine(message.Substring(sep + 3));
                    }
                    else
                    {
                        Console.ForegroundColor = textColor;
                        Console.WriteLine(message);
                    }
                }
                catch
                {
                }
                finally
                {
                    try
                    {
                        Console.ResetColor();
                    }
                    catch
                    {
                    }
                }
            }
        }

        #endregion

        #region Traduction

        private static readonly Dictionary<string, string> Modules = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "ApproachHandler", "accueil des joueurs" },
            { "ChatManager", "discussion" },
            { "BreedManager", "classes" },
            { "EffectManager", "effets" },
            { "SpellManager", "sorts" },
            { "TinselManager", "titres et ornements" },
            { "IncarnationManager", "incarnations" },
            { "AlvoMapManager", "cartes d'Alvo" },
            { "CranioManager", "Cranio" },
            { "ItemManager", "objets" },
            { "PetManager", "familiers" },
            { "TeleportPotionManager", "potions de téléportation" },
            { "BidHouseManager", "hôtels de vente" },
            { "RuneManager", "runes de forgemagie" },
            { "IdolManager", "idoles" },
            { "ChallengeManager", "challenges" },
            { "DungeonItemsManager", "objets de donjon" },
            { "RankRewardManager", "récompenses de rang" },
            { "RankManager", "rangs" },
            { "ExperienceManager", "expérience" },
            { "WorldMapScrollActionManager", "défilement de la carte du monde" },
            { "MonsterGroupPersistence", "groupes de monstres sauvegardés" },
            { "PlacementManager", "placements de combat" },
            { "CellTriggerManager", "déclencheurs de cases" },
            { "InteractiveManager", "éléments interactifs" },
            { "BrainManager", "intelligences artificielles" },
            { "Looger", "journal" },
            { "MongoLogger", "journal MongoDB" },
            { "JobManager", "métiers" },
            { "MonsterNaniManager", "monstres nanis" },
            { "ArenaManager", "Kolizéum" },
            { "VersusManager", "combats en duel" },
            { "NpcManager", "PNJ" },
            { "TrousseauManager", "trousseau" },
            { "GuildManager", "guildes" },
            { "MountManager", "montures" },
            { "MonsterManager", "monstres" },
            { "MerchantManager", "marchands" },
            { "AchievementManager", "succès" },
            { "RecherchesManager", "avis de recherche" },
            { "SeuilsSoinManager", "seuils de soin de l'IA" },
            { "World", "monde" },
            { "HavenBagManager", "havre-sacs" },
            { "TaxCollectorManager", "percepteurs" },
            { "PaddockManager", "enclos" },
            { "MiniGamesManager", "mini-jeux" },
            { "QuestManager", "quêtes" },
            { "BrisageOfficiel", "brisage" },
            { "AllianceManager", "alliances" },
            { "RepriseDonjons", "reprise de donjon" },
            { "SignalementsBugs", "signalement de bugs" },
            { "AdminPanelBridge", "panel admin" },
            { "AdminJournal", "journal du panel admin" },
            { "MonsterStarLoop", "étoiles des monstres" },
            { "AutoAnnounceManager", "annonces automatiques" },
            { "TextManager", "textes" },
            { "ConnectionHandler", "connexions" },
            { "LauncherApi", "API du launcher" },
        };

        private static readonly List<KeyValuePair<Regex, string>> Rules = new List<KeyValuePair<Regex, string>>();

        private static void Rule(string pattern, string replacement)
        {
            Rules.Add(new KeyValuePair<Regex, string>(new Regex("^" + pattern + "$", RegexOptions.Compiled | RegexOptions.IgnoreCase), replacement));
        }

        static MysticConsole()
        {
            // démarrage commun
            Rule(@"Initializing Configuration\.*", "Lecture de la configuration");
            Rule(@"Config file created", "Fichier de configuration créé");
            Rule(@"Initialize Task Pool", "Démarrage du pool de tâches");
            Rule(@"Initializing Network Interfaces\.*", "Préparation du réseau");
            Rule(@"Register Plugins\.*", "Enregistrement des extensions");
            Rule(@"Loading Plugins\.*", "Chargement des extensions");
            Rule(@"Cannot load unexistant plugin path (.*)", "Aucun dossier d'extensions ($1)");
            Rule(@"Initializing Database\.*", "Préparation de la base de données");
            Rule(@"Opening Database\.*", "Connexion à la base de données");
            Rule(@"Register Messages\.*", "Enregistrement des messages du protocole");
            Rule(@"Register Packets? Handlers\.*", "Enregistrement des gestionnaires de paquets");
            Rule(@"Register Commands\.*", "Enregistrement des commandes");
            Rule(@"Initialize DataManagers", "Chargement des données du jeu");
            Rule(@"Register id providers", "Préparation des identifiants");
            Rule(@"Synchronize id providers", "Synchronisation des identifiants");
            Rule(@"Exceptions not handled cause Debugger is attatched", "Débogueur attaché : les exceptions ne sont pas interceptées");

            // monde
            Rule(@"Load maps\.*", "Chargement des cartes");
            Rule(@"Load sub areas\.*", "Chargement des sous-zones");
            Rule(@"Load areas\.*", "Chargement des zones");
            Rule(@"Load super areas\.*", "Chargement des continents");
            Rule(@"Load graveyards\.*", "Chargement des cimetières");
            Rule(@"Spawn npcs ?\.*", "Apparition des PNJ");
            Rule(@"Spawn interactives ?\.*", "Apparition des éléments interactifs");
            Rule(@"Spawn cell triggers ?\.*", "Mise en place des déclencheurs de cases");
            Rule(@"Spawn monsters ?\.*", "Apparition des monstres");
            Rule(@"Spawn merchants ?\.*", "Apparition des marchands");
            Rule(@"Load houses", "Chargement des maisons");
            Rule(@"Area '(.*)' started", "Zone « $1 » activée");
            Rule(@"Area '(.*)' stopped", "Zone « $1 » mise en veille");
            Rule(@"Skill (\d+) has unknown type '(.*)', it is shown as unavailable", "Compétence $1 : type « $2 » inconnu, affichée comme indisponible");

            // fin de démarrage et réseau
            Rule(@"Start Auto-Save Cyclic Task", "Sauvegarde automatique activée");
            Rule(@"Starting Console Handler Interface\.*", "Console de commandes prête");
            Rule(@"Starting IPC Communications ?\.*", "Liaison avec le serveur de connexion");
            Rule(@"Start listening on port : (\d+)\.*", "Ouverture du port $1");
            Rule(@"Attempt connection", "Tentative de liaison avec le serveur de connexion");
            Rule(@"IPC connection etablished", "Liaison établie avec le serveur de connexion");
            Rule(@"Access to auth\. server granted", "Accès au serveur de connexion accordé");
            Rule(@"IPC connection lost", "Liaison perdue avec le serveur de connexion");
            Rule(@"Client <(.*)> connected", "Connexion de $1");
            Rule(@"Client <(.*)> \((.*)\) disconnected", "Déconnexion de $1 ($2)");
            Rule(@"Client <(.*)> disconnected", "Déconnexion de $1");

            // serveur de connexion
            Rule(@"Start World Servers Manager", "Gestion des mondes");
            Rule(@"Initialize Account Manager", "Gestion des comptes");
            Rule(@"Initialize DDOS Firewall Manager", "Pare-feu anti-DDoS");
            Rule(@"DDOS Patched !", "Protection anti-DDoS active");
            Rule(@"Initialize IPC Server\.*", "Préparation de la liaison avec les serveurs de jeu");
            Rule(@"Start Ipc Server", "Liaison avec les serveurs de jeu ouverte");
            Rule(@"Registered World : ""(.*)"" <Id : (\d+)> <(.*)>", "Monde « $1 » (n° $2) enregistré depuis $3");
            Rule(@"Opening Database connection for '(.*)' server", "Connexion à la base du monde « $1 »");

            // réglages obsolètes du fichier de configuration
            Rule(@"Cannot found the class '(.*)', is the assembly loaded \?", "Réglage ignoré : la classe $1 n'existe pas");
            Rule(@"Field or property '(.*)' doesn't exist", "Réglage ignoré : $1 n'existe plus");
            Rule(@"Cannot define the variable (\S+) : .*", "Réglage ignoré : $1");
            Rule(@"Found two Commands with Alias ""(.*)"": (\S+) and (\S+)", "Deux commandes utilisent l'alias « $1 » : $2 et $3");
        }

        public static string Translate(string message)
        {
            var text = message.Trim();

            var init = Regex.Match(text, @"^Initialize '(.+)'$");
            if (init.Success)
            {
                string fr;
                return "Chargement : " + (Modules.TryGetValue(init.Groups[1].Value, out fr) ? fr : init.Groups[1].Value);
            }

            foreach (var rule in Rules)
            {
                if (rule.Key.IsMatch(text))
                    return rule.Key.Replace(text, rule.Value);
            }

            return message;
        }

        #endregion
    }

    internal sealed class MysticConsoleTarget : Target
    {
        protected override void Write(LogEventInfo logEvent)
        {
            MysticConsole.WriteLog(logEvent);
        }
    }
}
