// MYSTIC 18/10/2026 : installation et mises à jour du jeu pour le launcher Mystic.
// Sur le serveur (URL de base, par défaut http://padgref.fr/maj/, ou l'IP du VPS tant que le domaine ne pointe pas dessus) :
//   complet.txt           : TOUS les fichiers du client (installation complète) : « version=… » puis sha256 <TAB> taille <TAB> chemin
//   manifest.txt          : les fichiers modifiés par Mystic (mises à jour), même format
//   files/<sha256>.bin    : contenu de chaque fichier
// Dossier du jeu : à côté du launcher s'il y trouve app\Dofus.exe (ancien fonctionnement), sinon celui choisi dans le launcher
// (%APPDATA%\Mystic\launcher.cfg), par défaut %LOCALAPPDATA%\Mystic\Jeu. Téléchargements en parallèle, reprise automatique
// (les fichiers déjà bons sont gardés), chaque fichier vérifié par son empreinte SHA-256 ; rien n'est écrit hors du dossier app.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace MysticLauncher
{
    class ManifestEntry
    {
        public string Hash;
        public long Size;
        public string Path;
    }

    class Updater
    {
        public readonly string LauncherDir;
        public string BaseDir { get; private set; }
        public string AppDir => System.IO.Path.Combine(BaseDir, "app");
        public readonly string BaseUrl;
        public string ApiUrl = Program.DefaultApi;
        public string SiteUrl = Program.DefaultSite;
        readonly string m_logPath;

        public string Version { get; private set; }
        public string LastError { get; private set; }

        static string DossierConfig => System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Mystic");
        static string FichierConfig => System.IO.Path.Combine(DossierConfig, "launcher.cfg");

        public Updater(string launcherDir)
        {
            LauncherDir = launcherDir;
            m_logPath = System.IO.Path.Combine(launcherDir, "launcher.log");
            BaseUrl = ReadIni();
            BaseDir = DossierDuJeu();
        }

        string ReadIni()
        {
            var url = Program.DefaultUrl;
            var ini = System.IO.Path.Combine(LauncherDir, "launcher.ini");
            try
            {
                if (File.Exists(ini))
                    foreach (var brut in File.ReadAllLines(ini))
                    {
                        var line = brut.Trim();
                        if (line.StartsWith("url=", StringComparison.OrdinalIgnoreCase)) url = line.Substring(4).Trim();
                        else if (line.StartsWith("api=", StringComparison.OrdinalIgnoreCase)) ApiUrl = line.Substring(4).Trim();
                        else if (line.StartsWith("site=", StringComparison.OrdinalIgnoreCase)) SiteUrl = line.Substring(5).Trim();
                    }
            }
            catch { }
            return url.EndsWith("/") ? url : url + "/";
        }

        string DossierDuJeu()
        {
            // ancien fonctionnement : launcher posé à côté du dossier app
            if (File.Exists(System.IO.Path.Combine(LauncherDir, "app", "Dofus.exe")))
                return LauncherDir;
            try
            {
                if (File.Exists(FichierConfig))
                    foreach (var l in File.ReadAllLines(FichierConfig))
                        if (l.StartsWith("dossier=") && l.Length > 8)
                            return l.Substring(8).Trim();
            }
            catch { }
            return System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Mystic", "Jeu");
        }

        /// <summary>Dossier du jeu pour cette exécution seulement (option --installer, tests).</summary>
        public void UtiliserDossier(string dossier) => BaseDir = dossier;

        public void ChangerDossier(string dossier)
        {
            BaseDir = dossier;
            try
            {
                Directory.CreateDirectory(DossierConfig);
                File.WriteAllText(FichierConfig, "dossier=" + dossier + Environment.NewLine, Encoding.UTF8);
            }
            catch { }
            Log("Dossier du jeu : " + dossier);
        }

        public void Log(string text)
        {
            try { File.AppendAllText(m_logPath, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + text + Environment.NewLine, Encoding.UTF8); }
            catch { }
        }

        public bool GameExists => File.Exists(System.IO.Path.Combine(AppDir, "Dofus.exe"));

        static string Taille(double octets)
        {
            if (octets >= 1 << 30) return (octets / (1 << 30)).ToString("0.00") + " Go";
            if (octets >= 1 << 20) return (octets / (1 << 20)).ToString("0.0") + " Mo";
            return (octets / 1024).ToString("0") + " Ko";
        }

        /// <summary>
        /// Installe (complet = true : tous les fichiers du client) puis met à jour (fichiers de Mystic).
        /// progress(texte, pourcentage) est appelé pendant le travail (peut être null).
        /// </summary>
        public bool Run(Action<string, int> progress, bool complet = false)
        {
            Action<string, int> report = (t, p) => { if (progress != null) progress(t, p); };
            LastError = null;

            try
            {
                if (!complet && !GameExists)
                    return Fail("Le jeu n'est pas installé dans ce dossier.");

                Directory.CreateDirectory(AppDir);
                Log("Serveur de mise à jour : " + BaseUrl + (complet ? " (installation complète)" : ""));
                report("Connexion au serveur de mise à jour…", 0);

                var entries = new List<ManifestEntry>();
                using (var wc = new WebClient())
                {
                    wc.Encoding = Encoding.UTF8;
                    if (complet)
                        entries.AddRange(ParseManifest(wc.DownloadString(BaseUrl + "complet.txt?t=" + DateTime.UtcNow.Ticks)));
                    // les fichiers de Mystic passent après le client complet (version la plus récente)
                    foreach (var e in ParseManifest(wc.DownloadString(BaseUrl + "manifest.txt?t=" + DateTime.UtcNow.Ticks)))
                    {
                        entries.RemoveAll(x => string.Equals(x.Path, e.Path, StringComparison.OrdinalIgnoreCase));
                        entries.Add(e);
                    }
                }
                Log("Version " + Version + " : " + entries.Count + " fichiers à vérifier");

                // 1. vérification des fichiers locaux
                var aTelecharger = new List<ManifestEntry>();
                for (var i = 0; i < entries.Count; i++)
                {
                    var e = entries[i];
                    if (i % 25 == 0 || i == entries.Count - 1)
                        report("Vérification des fichiers… (" + (i + 1) + "/" + entries.Count + ")", entries.Count == 0 ? 0 : (i + 1) * (complet ? 5 : 30) / entries.Count);
                    if (!IsUpToDate(e))
                        aTelecharger.Add(e);
                }

                if (aTelecharger.Count == 0)
                {
                    Log("Client à jour");
                    report("Le jeu est à jour.", 100);
                    return true;
                }

                long total = aTelecharger.Sum(x => x.Size), fait = 0;
                var lecteur = new DriveInfo(System.IO.Path.GetPathRoot(System.IO.Path.GetFullPath(AppDir)));
                if (lecteur.IsReady && lecteur.AvailableFreeSpace < total + (200L << 20))
                    return Fail("Espace disque insuffisant : il faut " + Taille(total) + " de libre sur " + lecteur.Name + ".");
                Log(aTelecharger.Count + " fichier(s) à télécharger (" + total + " octets)");

                // 2. téléchargement (4 en parallèle), progression à l'octet près
                var debutPct = complet ? 5 : 30;
                var file = new Queue<ManifestEntry>(aTelecharger.OrderByDescending(x => x.Size));
                Exception erreur = null;
                var chrono = Stopwatch.StartNew();
                var termines = 0;
                Action<long> avance = n => Interlocked.Add(ref fait, n);
                var ouvriers = Enumerable.Range(0, 4).Select(_ => new Thread(() =>
                {
                    while (erreur == null)
                    {
                        ManifestEntry e;
                        lock (file)
                        {
                            if (file.Count == 0) return;
                            e = file.Dequeue();
                        }
                        try
                        {
                            Download(e, avance);
                            Interlocked.Increment(ref termines);
                        }
                        catch (Exception ex)
                        {
                            erreur = ex;
                        }
                    }
                }) { IsBackground = true }).ToList();
                ouvriers.ForEach(t => t.Start());
                while (ouvriers.Any(t => t.IsAlive))
                {
                    Thread.Sleep(250);
                    var f = Interlocked.Read(ref fait);
                    var vitesse = f / Math.Max(0.5, chrono.Elapsed.TotalSeconds);
                    var reste = vitesse > 0 ? TimeSpan.FromSeconds((total - f) / vitesse) : TimeSpan.Zero;
                    var texte = (complet ? "Installation : " : "Mise à jour : ") + Taille(f) + " / " + Taille(total) + "  ·  " + Taille(vitesse) + "/s" +
                                (reste.TotalSeconds > 1 ? "  ·  reste " + (reste.TotalHours >= 1 ? ((int)reste.TotalHours) + " h " + reste.Minutes.ToString("00") : reste.Minutes + " min " + reste.Seconds.ToString("00") + " s") : "");
                    report(texte, (int)(debutPct + f * (100 - debutPct) / Math.Max(1, total)));
                }
                if (erreur != null)
                    throw erreur;

                Log((complet ? "Installation" : "Mise à jour") + " terminée (" + termines + " fichiers)");
                report(complet ? "Installation terminée." : "Mise à jour terminée.", 100);
                return true;
            }
            catch (WebException ex)
            {
                return Fail("Serveur de mise à jour injoignable (" + ex.Message + "). Relancez : le téléchargement reprendra où il s'est arrêté.");
            }
            catch (PathTooLongException)
            {
                return Fail("Le chemin du dossier est trop long : choisissez un dossier plus court (par exemple directement sur le disque C).");
            }
            catch (IOException ex)
            {
                return Fail("Impossible d'écrire un fichier du jeu : fermez Dofus puis réessayez (" + ex.Message + ").");
            }
            catch (UnauthorizedAccessException)
            {
                return Fail("Ce dossier est protégé : choisissez un autre dossier d'installation.");
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

        void Download(ManifestEntry e, Action<long> avance)
        {
            var path = LocalPath(e);
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
            var tmp = path + ".mystic-dl";
            long recus = 0;
            for (var essai = 1; ; essai++)
            {
                try
                {
                    var req = (HttpWebRequest)WebRequest.Create(BaseUrl + "files/" + e.Hash + ".bin");
                    req.Timeout = 30000;
                    req.ReadWriteTimeout = 30000;
                    using (var rep = req.GetResponse())
                    using (var src = rep.GetResponseStream())
                    using (var dst = File.Create(tmp))
                    {
                        var buf = new byte[81920];
                        int n;
                        while ((n = src.Read(buf, 0, buf.Length)) > 0)
                        {
                            dst.Write(buf, 0, n);
                            recus += n;
                            avance(n);
                        }
                    }
                    break;
                }
                catch (WebException) when (essai < 3)
                {
                    avance(-recus);
                    recus = 0;
                    Thread.Sleep(1000 * essai);
                }
            }

            if (Sha256(tmp) != e.Hash)
            {
                File.Delete(tmp);
                throw new InvalidDataException("fichier corrompu pendant le téléchargement : " + e.Path);
            }
            if (File.Exists(path))
                File.Delete(path);
            File.Move(tmp, path);
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

        /// <summary>Lance le jeu ; avec login + ticket, le client se connecte tout seul (arguments --username / --ticket du client 2.51).</summary>
        public Process StartGame(string login = null, string ticket = null, int compte = 0)
        {
            var prefs = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Dofus-x64");
            var args = "\"--prefs=" + prefs + "\"";
            if (!string.IsNullOrEmpty(login) && !string.IsNullOrEmpty(ticket) && login.IndexOfAny(new[] { '"', '=', ' ' }) < 0)
                args += " --username=" + login + " --ticket=" + ticket;
            // MYSTIC 20/10 : relie la fenêtre au Zaap local du launcher (reconnexion au même compte après une déconnexion)
            if (compte > 0 && !string.IsNullOrEmpty(login) && login.IndexOfAny(new[] { '"', '=', ' ' }) < 0)
                args += Zaap.Arguments(compte, login);
            var psi = new ProcessStartInfo(System.IO.Path.Combine(AppDir, "Dofus.exe"), args)
            {
                WorkingDirectory = AppDir,
                UseShellExecute = false
            };
            var p = Process.Start(psi);
            Log(ticket != null ? "Jeu lancé (connexion automatique de " + login + ")" : "Jeu lancé");
            return p;
        }
    }
}
