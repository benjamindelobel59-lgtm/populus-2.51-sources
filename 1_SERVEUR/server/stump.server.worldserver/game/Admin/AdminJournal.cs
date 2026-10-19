using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using NLog;
using Stump.Core.Attributes;
using Stump.Core.Reflection;
using Stump.Server.BaseServer.Initialization;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Exchanges.Trades;
using Stump.Server.WorldServer.Game.Fights;

namespace Stump.Server.WorldServer.Game.Admin
{
    /// <summary>
    /// Panel admin (onglets Serveur / Surveillance) : journal tenu par le serveur de jeu.
    ///  - admin_panel_health : état du serveur chaque minute (joueurs, combats, mémoire, retard des tâches) ;
    ///  - admin_journal_snapshots : relevé des personnages connectés toutes les 10 minutes (niveau, xp, kamas, banque, IP) ;
    ///  - admin_journal_events : connexions, échanges, commandes MJ ;
    ///  - admin_panel_events : événements programmés depuis le panel (taux bonus, annonces), appliqués ici.
    /// </summary>
    public class AdminJournal : Singleton<AdminJournal>
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        [Variable(true)]
        public static int SnapshotMinutes = 10;

        [Variable(true)]
        public static int KeepDays = 30;

        /// <summary>Relancer le serveur de jeu après un redémarrage demandé par le panel (inutile si un .bat le relance déjà en boucle).</summary>
        [Variable(true)]
        public static bool RelaunchAfterRestart = false;

        private static Stump.ORM.Database Database => WorldServer.Instance.DBAccessor.Database;

        private int m_fights;
        private DateTime m_lastSnapshot = DateTime.MinValue;
        private float m_baseXp = -1, m_baseDrops = -1, m_baseKamas = -1;
        private int m_activeRatesEvent;
        private DateTime m_restartAt = DateTime.MaxValue;
        private string m_restartReason;
        private readonly HashSet<int> m_restartWarned = new HashSet<int>();

        [Initialization(InitializationPass.Last)]
        public void Initialize()
        {
            Database.Execute(@"CREATE TABLE IF NOT EXISTS admin_panel_health (Id INT AUTO_INCREMENT PRIMARY KEY, At DATETIME NOT NULL,
                Online INT NOT NULL, Fights INT NOT NULL, MemoryMb INT NOT NULL, Threads INT NOT NULL, KEY(At)) DEFAULT CHARSET=utf8");
            Database.Execute(@"CREATE TABLE IF NOT EXISTS admin_journal_snapshots (Id INT AUTO_INCREMENT PRIMARY KEY, At DATETIME NOT NULL,
                CharacterId INT NOT NULL, Name VARCHAR(64) NOT NULL, AccountId INT NOT NULL, Ip VARCHAR(64) NULL, Level INT NOT NULL,
                Experience BIGINT NOT NULL, Kamas BIGINT NOT NULL, BankKamas BIGINT NOT NULL, MapId INT NOT NULL, KEY(At), KEY(CharacterId)) DEFAULT CHARSET=utf8");
            Database.Execute(@"CREATE TABLE IF NOT EXISTS admin_journal_events (Id INT AUTO_INCREMENT PRIMARY KEY, At DATETIME NOT NULL,
                Kind VARCHAR(32) NOT NULL, CharacterId INT NOT NULL DEFAULT 0, Name VARCHAR(64) NULL, AccountId INT NOT NULL DEFAULT 0,
                Ip VARCHAR(64) NULL, Value BIGINT NOT NULL DEFAULT 0, Details TEXT NULL, Handled TINYINT NOT NULL DEFAULT 0,
                KEY(At), KEY(Kind), KEY(CharacterId)) DEFAULT CHARSET=utf8mb4");
            Database.Execute(@"CREATE TABLE IF NOT EXISTS admin_panel_events (Id INT AUTO_INCREMENT PRIMARY KEY, Kind VARCHAR(16) NOT NULL,
                StartAt DATETIME NOT NULL, EndAt DATETIME NULL, XpRate FLOAT NULL, DropsRate FLOAT NULL, KamasRate FLOAT NULL,
                Message TEXT NULL, RepeatMinutes INT NOT NULL DEFAULT 0, LastRun DATETIME NULL, Status VARCHAR(16) NOT NULL DEFAULT 'prevu',
                CreatedBy VARCHAR(64) NULL) DEFAULT CHARSET=utf8mb4");
            // un événement de taux resté « en cours » d'un ancien démarrage reprend normalement (les taux de base viennent de la config)
            Database.Execute("UPDATE admin_panel_events SET Status='prevu' WHERE Status='en cours'");

            FightManager.Instance.EntityAdded += f => m_fights++;
            FightManager.Instance.EntityRemoved += f => m_fights = Math.Max(0, m_fights - 1);

            WorldServer.Instance.IOTaskPool.CallPeriodically(60 * 1000, Minute);
            WorldServer.Instance.IOTaskPool.CallPeriodically(15 * 1000, CheckEvents);
            WorldServer.Instance.IOTaskPool.CallPeriodically(1000, CheckRestart);
        }

        public int Fights => m_fights;
        public DateTime RestartAt => m_restartAt;
        public string RestartReason => m_restartReason;

        /* ------------------------------------------------------------------ relevés */

        private void Minute()
        {
            try
            {
                var p = Process.GetCurrentProcess();
                Database.Execute("INSERT INTO admin_panel_health (At, Online, Fights, MemoryMb, Threads) VALUES (NOW(), @0, @1, @2, @3)",
                    World.Instance.CharacterCount, m_fights, (int)(p.WorkingSet64 / 1048576), p.Threads.Count);

                if ((DateTime.Now - m_lastSnapshot).TotalMinutes >= SnapshotMinutes)
                {
                    m_lastSnapshot = DateTime.Now;
                    foreach (var c in World.Instance.GetCharacters().ToList())
                        Database.Execute(@"INSERT INTO admin_journal_snapshots (At, CharacterId, Name, AccountId, Ip, Level, Experience, Kamas, BankKamas, MapId)
                            VALUES (NOW(), @0, @1, @2, @3, @4, @5, @6, @7, @8)",
                            c.Id, c.Name, c.Account?.Id ?? 0, c.Client?.IP, c.Level, (long)c.Experience, (long)c.Kamas, (long)(c.Bank?.Kamas ?? 0), c.Map?.Id ?? 0);

                    // copie quotidienne pour l'onglet Support (après 5 h du matin, une par jour)
                    if (DateTime.Now.Hour >= 5)
                    {
                        var today = DateTime.Now.ToString("yyyyMMdd");
                        if (Database.Fetch<string>("SELECT TABLE_NAME FROM information_schema.TABLES WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME LIKE @0", "admin_copy_" + today + "%_c").Count == 0)
                            logger.Info("[Journal] copie du jour : {0}", AdminPanelData.MakeCopy());
                    }

                    Database.Execute("DELETE FROM admin_panel_health WHERE At < NOW() - INTERVAL @0 DAY", KeepDays);
                    Database.Execute("DELETE FROM admin_journal_snapshots WHERE At < NOW() - INTERVAL @0 DAY", KeepDays);
                    Database.Execute("DELETE FROM admin_journal_events WHERE At < NOW() - INTERVAL @0 DAY", KeepDays * 3);
                }
            }
            catch (Exception ex)
            {
                logger.Error("[Journal] releve impossible : {0}", ex.Message);
            }
        }

        public void Log(string kind, Character c, long value, object details)
        {
            try
            {
                var text = details as string ?? (details == null ? null : JsonConvert.SerializeObject(details));
                var args = new object[] { kind, c?.Id ?? 0, c?.Name, c?.Account?.Id ?? 0, c?.Client?.IP, value, text };
                WorldServer.Instance.IOTaskPool.ExecuteInContext(() =>
                    Database.Execute("INSERT INTO admin_journal_events (At, Kind, CharacterId, Name, AccountId, Ip, Value, Details) VALUES (NOW(), @0, @1, @2, @3, @4, @5, @6)", args));
            }
            catch (Exception ex)
            {
                logger.Error("[Journal] {0} : {1}", kind, ex.Message);
            }
        }

        public void OnLoggedIn(Character c) => Log("connexion", c, (long)c.Kamas, new { level = c.Level, map = c.Map?.Id });

        public void OnTrade(Character first, IEnumerable<TradeItem> firstItems, ulong firstKamas, Character second, IEnumerable<TradeItem> secondItems, ulong secondKamas)
        {
            object Items(IEnumerable<TradeItem> items) => items.Select(x => new { id = x.Template?.Id, stack = x.Stack }).ToList();
            Log("echange", first, (long)Math.Max(firstKamas, secondKamas), new
            {
                avec = second.Name,
                avecId = second.Id,
                avecIp = second.Client?.IP,
                donne = Items(firstItems),
                donneKamas = firstKamas,
                recoit = Items(secondItems),
                recoitKamas = secondKamas,
            });
        }

        public void OnGmCommand(Character c, string command) => Log("commande", c, 0, "." + command);

        /* ------------------------------------------------------------------ événements programmés */

        private class PanelEvent
        {
            public int Id { get; set; }
            public string Kind { get; set; }
            public DateTime StartAt { get; set; }
            public DateTime? EndAt { get; set; }
            public float? XpRate { get; set; }
            public float? DropsRate { get; set; }
            public float? KamasRate { get; set; }
            public string Message { get; set; }
            public int RepeatMinutes { get; set; }
            public DateTime? LastRun { get; set; }
            public string Status { get; set; }
        }

        private void CheckEvents()
        {
            try
            {
                var now = DateTime.Now;
                foreach (var e in Database.Fetch<PanelEvent>("SELECT * FROM admin_panel_events WHERE Status IN ('prevu','en cours') ORDER BY StartAt"))
                {
                    if (e.Kind == "annonce")
                    {
                        if (e.StartAt > now)
                            continue;
                        var due = e.LastRun == null || (e.RepeatMinutes > 0 && (now - e.LastRun.Value).TotalMinutes >= e.RepeatMinutes);
                        if (due && (e.EndAt == null || now < e.EndAt))
                        {
                            Announce(e.Message);
                            Database.Execute("UPDATE admin_panel_events SET LastRun=NOW(), Status=@0 WHERE Id=@1", e.RepeatMinutes > 0 && e.EndAt != null ? "en cours" : "fini", e.Id);
                        }
                        else if (e.EndAt != null && now >= e.EndAt)
                            Database.Execute("UPDATE admin_panel_events SET Status='fini' WHERE Id=@0", e.Id);
                        continue;
                    }

                    // taux bonus
                    if (e.Status == "prevu" && e.StartAt <= now && (e.EndAt == null || now < e.EndAt))
                    {
                        if (m_activeRatesEvent != 0)
                            continue; // un seul à la fois : le suivant attend la fin du précédent
                        m_baseXp = Rates.XpRate; m_baseDrops = Rates.DropsRate; m_baseKamas = Rates.KamasRate;
                        if (e.XpRate > 0) Rates.XpRate = e.XpRate.Value;
                        if (e.DropsRate > 0) Rates.DropsRate = e.DropsRate.Value;
                        if (e.KamasRate > 0) Rates.KamasRate = e.KamasRate.Value;
                        m_activeRatesEvent = e.Id;
                        Database.Execute("UPDATE admin_panel_events SET Status='en cours', LastRun=NOW() WHERE Id=@0", e.Id);
                        Announce(e.Message ?? $"Événement : XP x{Rates.XpRate}, drop x{Rates.DropsRate}, kamas x{Rates.KamasRate} !");
                    }
                    else if (e.Status == "en cours" && e.EndAt != null && now >= e.EndAt)
                        EndRatesEvent(e.Id, "fini");
                }
            }
            catch (Exception ex)
            {
                logger.Error("[Evenements] {0}", ex.Message);
            }
        }

        public void EndRatesEvent(int id, string status)
        {
            if (m_activeRatesEvent == id && m_baseXp >= 0)
            {
                Rates.XpRate = m_baseXp; Rates.DropsRate = m_baseDrops; Rates.KamasRate = m_baseKamas;
                m_activeRatesEvent = 0;
                Announce("Fin de l'événement : les taux reviennent à la normale.");
            }
            Database.Execute("UPDATE admin_panel_events SET Status=@0 WHERE Id=@1", status, id);
        }

        public static void Announce(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;
            World.Instance.SendAnnounce("[Annonce] " + message, System.Drawing.Color.Orange);
        }

        /* ------------------------------------------------------------------ redémarrage propre */

        public void ScheduleRestart(int minutes, string reason)
        {
            m_restartAt = DateTime.Now.AddMinutes(Math.Max(0, minutes));
            m_restartReason = reason;
            m_restartWarned.Clear();
            Announce($"Redémarrage du serveur dans {minutes} minute(s){(string.IsNullOrWhiteSpace(reason) ? "" : " : " + reason)}.");
        }

        public void CancelRestart()
        {
            if (m_restartAt == DateTime.MaxValue)
                return;
            m_restartAt = DateTime.MaxValue;
            Announce("Le redémarrage du serveur est annulé.");
        }

        private void CheckRestart()
        {
            if (m_restartAt == DateTime.MaxValue)
                return;
            var left = (int)Math.Ceiling((m_restartAt - DateTime.Now).TotalSeconds);
            foreach (var mark in new[] { 600, 300, 120, 60, 30, 10 })
                if (left <= mark && left > mark - 2 && m_restartWarned.Add(mark))
                    Announce(mark >= 60 ? $"Redémarrage du serveur dans {mark / 60} minute(s)." : $"Redémarrage du serveur dans {mark} secondes.");
            if (left > 0)
                return;

            m_restartAt = DateTime.MaxValue;
            logger.Info("[Panel admin] redemarrage demande : sauvegarde puis arret");
            World.Instance.Save();
            if (RelaunchAfterRestart)
            {
                var exe = Process.GetCurrentProcess().MainModule.FileName;
                // pause de 8 s avant la relance (« timeout » échoue tout de suite dans une fenêtre cachée : on utilise ping)
                Process.Start(new ProcessStartInfo("cmd.exe", $"/c ping -n 9 127.0.0.1 >nul & start \"\" \"{exe}\"")
                    { WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory, UseShellExecute = false, CreateNoWindow = true });
            }
            WorldServer.Instance.IOTaskPool.CallDelayed(3000, () => WorldServer.Instance.Shutdown());
        }
    }
}
