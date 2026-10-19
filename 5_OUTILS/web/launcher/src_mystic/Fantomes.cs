// MYSTIC 20/10/2026 : Dofus « fantômes ». Après fermeture, certains Dofus.exe restent en mémoire sans fenêtre et empêchent de
// relancer le jeu. Le launcher ferme de force les Dofus.exe de SON dossier app qui n'ont plus de fenêtre visible
// (au démarrage, toutes les 5 secondes, et juste avant chaque lancement). Un Dofus qui démarre (moins de 45 s) n'est jamais touché.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace MysticLauncher
{
    public static class Fantomes
    {
        delegate bool EnumProc(IntPtr h, IntPtr l);
        [DllImport("user32.dll")] static extern bool EnumWindows(EnumProc p, IntPtr l);
        [DllImport("user32.dll")] static extern uint GetWindowThreadProcessId(IntPtr h, out uint pid);
        [DllImport("user32.dll")] static extern bool IsWindowVisible(IntPtr h);
        [DllImport("user32.dll")] static extern int GetWindowTextLength(IntPtr h);
        [DllImport("kernel32.dll")] static extern IntPtr OpenProcess(uint acces, bool herite, int pid);
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)] static extern bool QueryFullProcessImageName(IntPtr h, int f, StringBuilder s, ref int n);
        [DllImport("kernel32.dll")] static extern bool CloseHandle(IntPtr h);

        const int DemarrageSecondes = 45;
        static readonly object s_verrou = new object();
        static readonly Dictionary<int, DateTime> s_sansFenetre = new Dictionary<int, DateTime>();

        /// <summary>Processus qui ont au moins une fenêtre visible avec un titre (la fenêtre du jeu).</summary>
        static HashSet<uint> AvecFenetre()
        {
            var pids = new HashSet<uint>();
            EnumWindows((h, l) =>
            {
                if (IsWindowVisible(h) && GetWindowTextLength(h) > 0 && GetWindowThreadProcessId(h, out var pid) != 0)
                    pids.Add(pid);
                return true;
            }, IntPtr.Zero);
            return pids;
        }

        static string Chemin(int pid)
        {
            var h = OpenProcess(0x1000, false, pid); // PROCESS_QUERY_LIMITED_INFORMATION
            if (h == IntPtr.Zero) return null;
            try
            {
                var sb = new StringBuilder(1024);
                var n = sb.Capacity;
                return QueryFullProcessImageName(h, 0, sb, ref n) ? sb.ToString(0, n) : null;
            }
            finally { CloseHandle(h); }
        }

        /// <summary>Ferme les Dofus.exe de <paramref name="appDir"/> sans fenêtre visible depuis au moins <paramref name="delai"/>.
        /// Renvoie le nombre de processus fermés.</summary>
        public static int Nettoyer(string appDir, TimeSpan delai, Action<string> log)
        {
            lock (s_verrou)
            {
                var fermes = 0;
                try
                {
                    var exe = Path.GetFullPath(Path.Combine(appDir, "Dofus.exe"));
                    var fenetres = AvecFenetre();
                    var vus = new HashSet<int>();
                    foreach (var p in Process.GetProcessesByName("Dofus"))
                    {
                        using (p)
                        {
                            try
                            {
                                if (!string.Equals(Chemin(p.Id), exe, StringComparison.OrdinalIgnoreCase)) continue;
                                vus.Add(p.Id);
                                if (fenetres.Contains((uint)p.Id) || (DateTime.Now - p.StartTime).TotalSeconds < DemarrageSecondes)
                                {
                                    s_sansFenetre.Remove(p.Id);
                                    continue;
                                }
                                if (!s_sansFenetre.TryGetValue(p.Id, out var depuis))
                                    s_sansFenetre[p.Id] = depuis = DateTime.Now;
                                if (DateTime.Now - depuis < delai) continue;
                                p.Kill();
                                p.WaitForExit(3000);
                                s_sansFenetre.Remove(p.Id);
                                fermes++;
                                log?.Invoke("Dofus resté en mémoire sans fenêtre fermé (processus " + p.Id + ")");
                            }
                            catch { }
                        }
                    }
                    foreach (var pid in new List<int>(s_sansFenetre.Keys))
                        if (!vus.Contains(pid)) s_sansFenetre.Remove(pid);
                }
                catch { }
                return fermes;
            }
        }
    }
}
