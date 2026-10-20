// PADGREF 20/10/2026 : le launcher s'installe vraiment (demande utilisateur : « quand on installe le launcher ça installe
// vraiment puis on met une icône sur le bureau pour lancer le launcher »).
// - Lancé depuis n'importe où (Téléchargements…) : il se copie dans %LOCALAPPDATA%\Padgref\Launcher\MysticLauncher.exe
//   (sans droits administrateur, avec launcher.ini s'il y en a un à côté), crée les raccourcis « Padgref » (Bureau + menu
//   Démarrer, icône du launcher), relance la copie installée et se ferme.
// - Lancé depuis le dossier installé : recrée les raccourcis supprimés, puis se met à jour lui-même depuis le serveur
//   (<url de maj>/MysticLauncher.exe, ETag gardé pour ne rien retélécharger) : l'exe en cours est renommé en *.ancien.exe
//   (Windows le permet), la nouvelle version prend sa place et est relancée. launcher.ini « majlauncher=non » coupe la mise à jour.
// - Option --portable : ne s'installe pas (lancement direct, comme avant).

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;

namespace MysticLauncher
{
    static class InstallationLauncher
    {
        public const string NomExe = "MysticLauncher.exe";
        public static string Dossier => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Padgref", "Launcher");
        public static string Exe => Path.Combine(Dossier, NomExe);
        static string FichierEtag => Path.Combine(Dossier, "maj_launcher.etag");

        static bool MemeDossier(string a, string b) =>
            string.Equals(Path.GetFullPath(a).TrimEnd('\\'), Path.GetFullPath(b).TrimEnd('\\'), StringComparison.OrdinalIgnoreCase);

        /// <summary>Renvoie vrai si une autre copie du launcher vient d'être lancée (celle-ci doit alors se fermer).</summary>
        public static bool Preparer(Updater u, string[] args)
        {
            try
            {
                var actuel = Assembly.GetExecutingAssembly().Location;
                var source = Path.GetDirectoryName(actuel);
                if (!MemeDossier(source, Dossier))
                {
                    Directory.CreateDirectory(Dossier);
                    // ancien fonctionnement (launcher posé à côté de app\Dofus.exe) : le jeu reste là où il est
                    if (MemeDossier(u.BaseDir, source) && !Updater.DossierChoisi)
                        u.ChangerDossier(source);
                    if (!File.Exists(Exe) || Empreinte(Exe) != Empreinte(actuel))
                        Remplacer(File.ReadAllBytes(actuel));
                    var ini = Path.Combine(source, "launcher.ini");
                    if (File.Exists(ini)) File.Copy(ini, Path.Combine(Dossier, "launcher.ini"), true);
                    Raccourcis(true);
                    u.Log("Launcher installé dans " + Dossier + " (lancé depuis " + source + ")");
                    Relancer(args);
                    return true;
                }

                // copie installée
                foreach (var vieux in Directory.GetFiles(Dossier, "*.ancien.exe"))
                    try { File.Delete(vieux); } catch { }
                Raccourcis(false);
                if (u.MajLauncher && MettreAJour(u))
                {
                    Relancer(args);
                    return true;
                }
            }
            catch (Exception ex)
            {
                u.Log("Installation du launcher impossible (lancement sur place) : " + ex.Message);
            }
            return false;
        }

        static void Relancer(string[] args)
        {
            var suite = string.Join(" ", args.Select(a => "\"" + a.Replace("\"", "") + "\""));
            Process.Start(new ProcessStartInfo(Exe, suite) { WorkingDirectory = Dossier, UseShellExecute = false });
        }

        /// <summary>Met la nouvelle version à la place de l'exe installé, même s'il est en cours d'exécution.</summary>
        static void Remplacer(byte[] octets)
        {
            var nouveau = Exe + ".nouveau";
            File.WriteAllBytes(nouveau, octets);
            if (File.Exists(Exe))
            {
                try { File.Delete(Exe); }
                catch
                {
                    // exe en cours d'exécution : on peut le renommer, pas l'écraser
                    File.Move(Exe, Path.Combine(Dossier, "MysticLauncher." + DateTime.Now.Ticks + ".ancien.exe"));
                }
            }
            File.Move(nouveau, Exe);
        }

        static string Empreinte(string fichier)
        {
            using (var sha = SHA256.Create())
            using (var f = File.OpenRead(fichier))
                return BitConverter.ToString(sha.ComputeHash(f));
        }

        /// <summary>Télécharge le launcher du serveur s'il a changé (ETag) ; vrai si l'exe installé a été remplacé.</summary>
        static bool MettreAJour(Updater u)
        {
            try
            {
                var req = (HttpWebRequest)WebRequest.Create(u.BaseUrl + NomExe);
                req.Timeout = 4000;
                req.ReadWriteTimeout = 15000;
                var etag = File.Exists(FichierEtag) ? File.ReadAllText(FichierEtag).Trim() : null;
                if (!string.IsNullOrEmpty(etag)) req.Headers[HttpRequestHeader.IfNoneMatch] = etag;
                byte[] octets;
                string nouvelEtag;
                try
                {
                    using (var rep = (HttpWebResponse)req.GetResponse())
                    using (var flux = rep.GetResponseStream())
                    using (var ms = new MemoryStream())
                    {
                        flux.CopyTo(ms);
                        octets = ms.ToArray();
                        nouvelEtag = rep.Headers[HttpResponseHeader.ETag];
                    }
                }
                catch (WebException ex) when (ex.Response is HttpWebResponse r && r.StatusCode == HttpStatusCode.NotModified)
                {
                    return false;
                }
                // un vrai exe Windows d'une taille plausible, sinon on garde l'actuel
                if (octets.Length < 100 * 1024 || octets.Length > 50 * 1024 * 1024 || octets[0] != 'M' || octets[1] != 'Z')
                    return false;
                string distant;
                using (var sha = SHA256.Create()) distant = BitConverter.ToString(sha.ComputeHash(octets));
                var change = distant != Empreinte(Exe);
                if (change)
                {
                    Remplacer(octets);
                    u.Log("Launcher mis à jour depuis " + u.BaseUrl + NomExe);
                }
                if (!string.IsNullOrEmpty(nouvelEtag)) File.WriteAllText(FichierEtag, nouvelEtag);
                return change;
            }
            catch (Exception ex)
            {
                u.Log("Mise à jour du launcher impossible : " + ex.Message);
                return false;
            }
        }

        /// <summary>Raccourcis « Padgref » sur le Bureau et dans le menu Démarrer (recréés s'ils ont été supprimés).</summary>
        static void Raccourcis(bool forcer)
        {
            foreach (var dossier in new[] { Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), Environment.GetFolderPath(Environment.SpecialFolder.Programs) })
            {
                try
                {
                    if (string.IsNullOrEmpty(dossier)) continue;
                    var lnk = Path.Combine(dossier, "Padgref.lnk");
                    if (File.Exists(lnk) && !forcer) continue;
                    var type = Type.GetTypeFromProgID("WScript.Shell");
                    var shell = Activator.CreateInstance(type);
                    var r = type.InvokeMember("CreateShortcut", BindingFlags.InvokeMethod, null, shell, new object[] { lnk });
                    var t = r.GetType();
                    t.InvokeMember("TargetPath", BindingFlags.SetProperty, null, r, new object[] { Exe });
                    t.InvokeMember("WorkingDirectory", BindingFlags.SetProperty, null, r, new object[] { Dossier });
                    t.InvokeMember("IconLocation", BindingFlags.SetProperty, null, r, new object[] { Exe + ",0" });
                    t.InvokeMember("Description", BindingFlags.SetProperty, null, r, new object[] { "Launcher Padgref" });
                    t.InvokeMember("Save", BindingFlags.InvokeMethod, null, r, null);
                }
                catch { }
            }
        }
    }
}
