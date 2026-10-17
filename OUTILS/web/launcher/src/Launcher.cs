// Launcher Dofus Nekfeu : met à jour les fichiers modifiés du client 64 bits puis lance le jeu.
// À placer dans le dossier du client, à côté du dossier « app ».
//
// Sur le serveur (URL de base, par défaut http://IP_DU_VPS/maj/) :
//   manifest.txt          : « version=… » puis une ligne par fichier : sha256 <TAB> taille <TAB> chemin relatif à app
//   files/<sha256>.bin    : contenu de chaque fichier
// Fichier optionnel launcher.ini à côté du launcher : « url=http://… » pour changer de serveur.
// Option --auto : met à jour sans fenêtre, écrit launcher.log et quitte (code 0 = à jour).

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace NekfeuLauncher
{
    static class Program
    {
        public const string DefaultUrl = "http://IP_DU_VPS/maj/";

        [STAThread]
        static int Main(string[] args)
        {
            var auto = Array.IndexOf(args, "--auto") >= 0;
            var updater = new Updater(AppDomain.CurrentDomain.BaseDirectory);

            if (auto)
            {
                var ok = updater.Run(null);
                return ok ? 0 : 1;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm(updater));
            return 0;
        }
    }

    class ManifestEntry
    {
        public string Hash;
        public long Size;
        public string Path;
    }

    class Updater
    {
        public readonly string BaseDir;
        public readonly string AppDir;
        public readonly string BaseUrl;
        readonly string m_logPath;

        public string Version { get; private set; }
        public string LastError { get; private set; }

        public Updater(string baseDir)
        {
            BaseDir = baseDir;
            AppDir = System.IO.Path.Combine(baseDir, "app");
            m_logPath = System.IO.Path.Combine(baseDir, "launcher.log");
            BaseUrl = ReadUrl();
        }

        string ReadUrl()
        {
            var url = Program.DefaultUrl;
            var ini = System.IO.Path.Combine(BaseDir, "launcher.ini");
            try
            {
                if (File.Exists(ini))
                    foreach (var line in File.ReadAllLines(ini))
                        if (line.Trim().StartsWith("url=", StringComparison.OrdinalIgnoreCase))
                            url = line.Trim().Substring(4).Trim();
            }
            catch { }
            return url.EndsWith("/") ? url : url + "/";
        }

        public void Log(string text)
        {
            try { File.AppendAllText(m_logPath, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + text + Environment.NewLine, Encoding.UTF8); }
            catch { }
        }

        /// <summary>
        /// Met le client à jour. progress(texte, pourcentage) est appelé pendant le travail (peut être null).
        /// </summary>
        public bool Run(Action<string, int> progress)
        {
            Action<string, int> report = (t, p) => { if (progress != null) progress(t, p); };
            LastError = null;

            try
            {
                if (!File.Exists(System.IO.Path.Combine(AppDir, "Dofus.exe")))
                    return Fail("Dossier « app\\Dofus.exe » introuvable : placez le launcher dans le dossier du client (à côté du dossier app).");

                Log("Serveur de mise à jour : " + BaseUrl);
                report("Connexion au serveur de mise à jour…", 0);

                List<ManifestEntry> entries;
                using (var wc = new WebClient())
                {
                    wc.Encoding = Encoding.UTF8;
                    var text = wc.DownloadString(BaseUrl + "manifest.txt?t=" + DateTime.UtcNow.Ticks);
                    entries = ParseManifest(text);
                }
                Log("Manifest version " + Version + " : " + entries.Count + " fichiers");

                // 1. vérification des fichiers locaux
                var toDownload = new List<ManifestEntry>();
                for (var i = 0; i < entries.Count; i++)
                {
                    var e = entries[i];
                    report("Vérification des fichiers… (" + (i + 1) + "/" + entries.Count + ")", entries.Count == 0 ? 0 : (i + 1) * 30 / entries.Count);
                    if (!IsUpToDate(e))
                        toDownload.Add(e);
                }

                if (toDownload.Count == 0)
                {
                    Log("Client à jour");
                    report("Le jeu est à jour (version " + Version + ").", 100);
                    return true;
                }

                // 2. téléchargement
                long total = 0, done = 0;
                foreach (var e in toDownload) total += e.Size;
                Log(toDownload.Count + " fichier(s) à télécharger (" + total + " octets)");

                for (var i = 0; i < toDownload.Count; i++)
                {
                    var e = toDownload[i];
                    var percent = total == 0 ? 100 : (int)(30 + done * 70 / total);
                    report("Téléchargement " + (i + 1) + "/" + toDownload.Count + " : " + e.Path, percent);
                    Download(e);
                    done += e.Size;
                }

                Log("Mise à jour terminée");
                report("Mise à jour terminée (version " + Version + ").", 100);
                return true;
            }
            catch (WebException ex)
            {
                return Fail("Serveur de mise à jour injoignable (" + ex.Message + ").");
            }
            catch (IOException ex)
            {
                return Fail("Impossible d'écrire un fichier du jeu : fermez Dofus puis réessayez (" + ex.Message + ").");
            }
            catch (Exception ex)
            {
                return Fail("Erreur pendant la mise à jour : " + ex.Message);
            }
        }

        bool Fail(string message)
        {
            LastError = message;
            Log("ERREUR : " + message);
            return false;
        }

        List<ManifestEntry> ParseManifest(string text)
        {
            var list = new List<ManifestEntry>();
            Version = "?";
            var appFull = System.IO.Path.GetFullPath(AppDir).TrimEnd('\\') + "\\";

            foreach (var raw in text.Split('\n'))
            {
                var line = raw.Trim('\r', ' ', '﻿');
                if (line.Length == 0 || line.StartsWith("#"))
                    continue;
                if (line.StartsWith("version="))
                {
                    Version = line.Substring(8);
                    continue;
                }

                var parts = line.Split('\t');
                if (parts.Length != 3 || parts[0].Length != 64)
                    throw new InvalidDataException("ligne de manifest invalide : " + line);

                var rel = parts[2].Replace('/', '\\');
                var full = System.IO.Path.GetFullPath(System.IO.Path.Combine(AppDir, rel));
                // sécurité : aucun fichier en dehors du dossier app
                if (!full.StartsWith(appFull, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException("chemin interdit dans le manifest : " + parts[2]);

                list.Add(new ManifestEntry { Hash = parts[0].ToLowerInvariant(), Size = long.Parse(parts[1]), Path = rel });
            }
            return list;
        }

        string LocalPath(ManifestEntry e) => System.IO.Path.Combine(AppDir, e.Path);

        bool IsUpToDate(ManifestEntry e)
        {
            var path = LocalPath(e);
            if (!File.Exists(path))
                return false;
            if (new FileInfo(path).Length != e.Size)
                return false;
            return Sha256(path) == e.Hash;
        }

        void Download(ManifestEntry e)
        {
            var path = LocalPath(e);
            var dir = System.IO.Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var tmp = path + ".nekfeu-dl";
            using (var wc = new WebClient())
                wc.DownloadFile(BaseUrl + "files/" + e.Hash + ".bin", tmp);

            var hash = Sha256(tmp);
            if (hash != e.Hash)
            {
                File.Delete(tmp);
                throw new InvalidDataException("fichier corrompu pendant le téléchargement : " + e.Path);
            }

            if (File.Exists(path))
                File.Delete(path);
            File.Move(tmp, path);
            Log("Mis à jour : " + e.Path);
        }

        static string Sha256(string path)
        {
            using (var sha = SHA256.Create())
            using (var stream = File.OpenRead(path))
            {
                var bytes = sha.ComputeHash(stream);
                var sb = new StringBuilder(64);
                foreach (var b in bytes)
                    sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }

        public void StartGame()
        {
            var prefs = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Dofus-x64");
            var psi = new ProcessStartInfo(System.IO.Path.Combine(AppDir, "Dofus.exe"), "\"--prefs=" + prefs + "\"")
            {
                WorkingDirectory = AppDir,
                UseShellExecute = false
            };
            Process.Start(psi);
            Log("Jeu lancé");
        }
    }

    class MainForm : Form
    {
        readonly Updater m_updater;
        readonly Label m_status;
        readonly ProgressBar m_progress;
        readonly Button m_play;
        readonly Button m_retry;

        public MainForm(Updater updater)
        {
            m_updater = updater;

            Text = "Dofus Nekfeu - Launcher";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(480, 190);
            Font = new Font("Segoe UI", 9f);
            try { Icon = Icon.ExtractAssociatedIcon(Path.Combine(updater.AppDir, "Dofus.exe")); } catch { }

            var title = new Label
            {
                Text = "Dofus Nekfeu",
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Bounds = new Rectangle(10, 10, 460, 40)
            };
            m_status = new Label
            {
                Text = "Préparation…",
                AutoSize = false,
                AutoEllipsis = true,
                Bounds = new Rectangle(20, 60, 440, 36)
            };
            m_progress = new ProgressBar { Bounds = new Rectangle(20, 100, 440, 22), Minimum = 0, Maximum = 100 };
            m_retry = new Button { Text = "Réessayer", Bounds = new Rectangle(20, 138, 120, 36), Visible = false };
            m_play = new Button { Text = "Jouer", Bounds = new Rectangle(340, 138, 120, 36), Enabled = false, Font = new Font("Segoe UI", 11f, FontStyle.Bold) };

            m_retry.Click += (s, e) => StartUpdate();
            m_play.Click += (s, e) => Play();

            Controls.AddRange(new Control[] { title, m_status, m_progress, m_retry, m_play });
            Shown += (s, e) => StartUpdate();
        }

        void StartUpdate()
        {
            m_retry.Visible = false;
            m_play.Enabled = false;
            m_play.Text = "Jouer";

            new Thread(() =>
            {
                var ok = m_updater.Run((text, percent) => BeginInvoke((Action)(() =>
                {
                    m_status.Text = text;
                    m_progress.Value = Math.Max(0, Math.Min(100, percent));
                })));

                BeginInvoke((Action)(() =>
                {
                    if (ok)
                    {
                        m_progress.Value = 100;
                        m_play.Enabled = true;
                        m_play.Focus();
                    }
                    else
                    {
                        m_status.Text = m_updater.LastError;
                        m_retry.Visible = true;
                        // le jeu reste lançable sans mise à jour (serveur hors ligne, etc.)
                        m_play.Text = "Jouer quand même";
                        m_play.Enabled = File.Exists(Path.Combine(m_updater.AppDir, "Dofus.exe"));
                    }
                }));
            }) { IsBackground = true }.Start();
        }

        void Play()
        {
            try
            {
                m_updater.StartGame();
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Impossible de lancer le jeu : " + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
