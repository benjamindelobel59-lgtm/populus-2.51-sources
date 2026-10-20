// PADGREF 20/10/2026 : le launcher s'installe vraiment (demandes utilisateur : « quand on installe le launcher ça installe
// vraiment puis on met une icône sur le bureau », puis « un vrai installeur » et « il s'appelle encore Mystic »).
// - Fichier téléchargé : PadgrefLauncher.exe (n'importe quel nom marche). Lancé hors du dossier installé → fenêtre
//   d'installation Padgref (FenetreInstallation : Installer, progression, « Installation terminée », Lancer Padgref).
// - Copie installée : %LOCALAPPDATA%\Padgref\Launcher\Padgref.exe (sans droits administrateur), raccourcis « Padgref »
//   sur le Bureau et dans le menu Démarrer (icône du launcher). On lance ensuite toujours depuis l'icône.
// - Ancienne copie MysticLauncher.exe dans ce dossier (installations d'avant le 20/10 17h, qui se mettent à jour par
//   /maj/MysticLauncher.exe) : la nouvelle version s'y copie en Padgref.exe, repointe les raccourcis, relance Padgref.exe,
//   et Padgref.exe supprime ensuite MysticLauncher.exe.
// - Copie installée : recrée les raccourcis supprimés, puis se met à jour elle-même depuis <url de maj>/PadgrefLauncher.exe
//   (ou /maj/MysticLauncher.exe si le premier n'existe pas ; ETag gardé pour ne rien retélécharger) : l'exe en cours est
//   renommé en *.ancien.exe (Windows le permet), la nouvelle version prend sa place et est relancée.
//   launcher.ini (dans le dossier installé) « majlauncher=non » coupe la mise à jour ; option --portable = pas d'installation.
// - Le launcher.ini posé à côté du fichier téléchargé n'est PLUS recopié (un réglage de test local, api=127.0.0.1, avait
//   suivi l'installation et faisait perdre la connexion).

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
using static MysticLauncher.Theme;

namespace MysticLauncher
{
    static class InstallationLauncher
    {
        public const string NomExe = "Padgref.exe";
        public const string NomTelechargement = "PadgrefLauncher.exe";
        public const string AncienNom = "MysticLauncher.exe";
        public static string Dossier => System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Padgref", "Launcher");
        public static string Exe => System.IO.Path.Combine(Dossier, NomExe);
        static string FichierEtag => System.IO.Path.Combine(Dossier, "maj_launcher.etag");
        public static string Actuel => Assembly.GetExecutingAssembly().Location;

        public enum Suite { Continuer, Quitter, Installeur }

        static bool MemeDossier(string a, string b) =>
            string.Equals(System.IO.Path.GetFullPath(a).TrimEnd('\\'), System.IO.Path.GetFullPath(b).TrimEnd('\\'), StringComparison.OrdinalIgnoreCase);

        public static bool EstInstalle => File.Exists(Exe);

        /// <summary>Que faire au démarrage : continuer (copie installée), quitter (une autre copie vient d'être lancée), ou installer.</summary>
        public static Suite Preparer(Updater u, string[] args)
        {
            try
            {
                var source = System.IO.Path.GetDirectoryName(Actuel);
                if (!MemeDossier(source, Dossier))
                    return Suite.Installeur;

                if (!string.Equals(System.IO.Path.GetFileName(Actuel), NomExe, StringComparison.OrdinalIgnoreCase))
                {
                    // ancienne copie (MysticLauncher.exe) mise à jour par /maj/MysticLauncher.exe → devient Padgref.exe
                    if (!File.Exists(Exe) || Empreinte(Exe) != Empreinte(Actuel))
                        Remplacer(File.ReadAllBytes(Actuel));
                    Raccourcis(true);
                    u.Log("Launcher renommé : " + System.IO.Path.GetFileName(Actuel) + " → " + NomExe);
                    Lancer(args);
                    return Suite.Quitter;
                }

                // copie installée (Padgref.exe)
                foreach (var vieux in Directory.GetFiles(Dossier, "*.ancien.exe").Concat(new[] { System.IO.Path.Combine(Dossier, AncienNom) }))
                    try { if (File.Exists(vieux)) File.Delete(vieux); } catch { }
                Raccourcis(false);
                if (u.MajLauncher && MettreAJour(u))
                {
                    Lancer(args);
                    return Suite.Quitter;
                }
            }
            catch (Exception ex)
            {
                u.Log("Préparation du launcher : " + ex.Message + " (lancement sur place)");
            }
            return Suite.Continuer;
        }

        /// <summary>Installation (fenêtre d'installation) : copie, dossier du jeu, raccourcis. Lève une exception en cas d'échec.</summary>
        public static void Installer(Updater u, bool bureau)
        {
            Directory.CreateDirectory(Dossier);
            var source = System.IO.Path.GetDirectoryName(Actuel);
            // ancien fonctionnement (launcher posé à côté de app\Dofus.exe) : le jeu reste là où il est
            if (MemeDossier(u.BaseDir, source) && !Updater.DossierChoisi)
                u.ChangerDossier(source);
            if (!File.Exists(Exe) || Empreinte(Exe) != Empreinte(Actuel))
                Remplacer(File.ReadAllBytes(Actuel));
            var ancien = System.IO.Path.Combine(Dossier, AncienNom);
            try { if (File.Exists(ancien)) File.Delete(ancien); } catch { }
            Raccourcis(true, bureau);
            u.Log("Launcher installé dans " + Dossier + " (depuis " + source + ")");
        }

        public static void Lancer(string[] args)
        {
            var suite = string.Join(" ", (args ?? new string[0]).Select(a => "\"" + a.Replace("\"", "") + "\""));
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
                    File.Move(Exe, System.IO.Path.Combine(Dossier, "Padgref." + DateTime.Now.Ticks + ".ancien.exe"));
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

        /// <summary>Télécharge le launcher du serveur s'il a changé ; vrai si l'exe installé a été remplacé.</summary>
        static bool MettreAJour(Updater u)
        {
            foreach (var nom in new[] { NomTelechargement, AncienNom })
            {
                var url = u.BaseUrl + nom;
                try
                {
                    var req = (HttpWebRequest)WebRequest.Create(url);
                    req.Timeout = 4000;
                    req.ReadWriteTimeout = 15000;
                    var garde = File.Exists(FichierEtag) ? File.ReadAllText(FichierEtag).Trim() : "";
                    var i = garde.LastIndexOf('|');
                    if (i > 0 && garde.Substring(0, i) == url) req.Headers[HttpRequestHeader.IfNoneMatch] = garde.Substring(i + 1);
                    byte[] octets;
                    string etag;
                    try
                    {
                        using (var rep = (HttpWebResponse)req.GetResponse())
                        using (var flux = rep.GetResponseStream())
                        using (var ms = new MemoryStream())
                        {
                            flux.CopyTo(ms);
                            octets = ms.ToArray();
                            etag = rep.Headers[HttpResponseHeader.ETag];
                        }
                    }
                    catch (WebException ex) when (ex.Response is HttpWebResponse r)
                    {
                        if (r.StatusCode == HttpStatusCode.NotModified) return false;
                        if (r.StatusCode == HttpStatusCode.NotFound) continue; // PadgrefLauncher.exe pas encore publié : ancien nom
                        throw;
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
                        u.Log("Launcher mis à jour depuis " + url);
                    }
                    if (!string.IsNullOrEmpty(etag)) File.WriteAllText(FichierEtag, url + "|" + etag);
                    return change;
                }
                catch (Exception ex)
                {
                    u.Log("Mise à jour du launcher impossible (" + url + ") : " + ex.Message);
                    return false;
                }
            }
            return false;
        }

        /// <summary>Raccourcis « Padgref » (Bureau + menu Démarrer) vers Padgref.exe ; recréés s'ils manquent ou pointent ailleurs.</summary>
        static void Raccourcis(bool forcer, bool bureau = true)
        {
            var dossiers = bureau
                ? new[] { Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), Environment.GetFolderPath(Environment.SpecialFolder.Programs) }
                : new[] { Environment.GetFolderPath(Environment.SpecialFolder.Programs) };
            foreach (var dossier in dossiers)
            {
                try
                {
                    if (string.IsNullOrEmpty(dossier)) continue;
                    var lnk = System.IO.Path.Combine(dossier, "Padgref.lnk");
                    var type = Type.GetTypeFromProgID("WScript.Shell");
                    var shell = Activator.CreateInstance(type);
                    var r = type.InvokeMember("CreateShortcut", BindingFlags.InvokeMethod, null, shell, new object[] { lnk });
                    var t = r.GetType();
                    var cible = (string)t.InvokeMember("TargetPath", BindingFlags.GetProperty, null, r, null);
                    if (File.Exists(lnk) && !forcer && string.Equals(cible, Exe, StringComparison.OrdinalIgnoreCase)) continue;
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

    /// <summary>PADGREF 20/10 : fenêtre d'installation (thème Azur) : Installer → progression → Installation terminée → Lancer Padgref.</summary>
    class FenetreInstallation : Window
    {
        const double L = 620, Ht = 440;
        readonly Updater m_updater;
        readonly string[] m_args;
        readonly string m_apercu;
        readonly Grid m_racine = new Grid();
        readonly StackPanel m_contenu = new StackPanel { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(40, 10, 40, 28) };
        Func<bool> m_bureau;

        public FenetreInstallation(Updater updater, string[] args, string apercu = null)
        {
            m_updater = updater;
            m_args = args;
            m_apercu = apercu;
            Title = "Installer Padgref";
            Width = L;
            Height = Ht;
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Background = Brushes.Transparent;
            ResizeMode = ResizeMode.CanMinimize;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            UseLayoutRounding = true;
            TextOptions.SetTextFormattingMode(this, TextFormattingMode.Ideal);
            var cadre = new Border { CornerRadius = new CornerRadius(16), Background = B(Fond), BorderBrush = B(Ligne), BorderThickness = new Thickness(1), Child = m_racine };
            m_racine.Clip = new RectangleGeometry(new Rect(0, 0, L - 2, Ht - 2), 15, 15);
            Content = cadre;
            m_racine.RowDefinitions.Add(new RowDefinition { Height = new GridLength(BARRE) });
            m_racine.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            // halo bleu derrière le contenu (comme le fond des pages Azur)
            var halo = new Rectangle { Fill = new RadialGradientBrush(A(0x55, Accent), Colors.Transparent) { Center = new Point(0.5, 0), GradientOrigin = new Point(0.5, 0), RadiusX = 0.8, RadiusY = 0.9 } };
            Grid.SetRowSpan(halo, 2);
            m_racine.Children.Add(halo);
            var barre = BarreTitre(this, null, Close);
            ((TextBlock)((StackPanel)barre.Children[1]).Children[2]).Text = "Installation";
            m_racine.Children.Add(barre);
            Grid.SetRow(m_contenu, 1);
            m_racine.Children.Add(m_contenu);
            Accueil();
            Loaded += (s, e) => { if (m_apercu != null) Apercu(); };
        }

        void Entete(string titre, string texte)
        {
            m_contenu.Children.Clear();
            var logo = new System.Windows.Controls.Image { Source = Theme.Image("p_logo.png"), Width = 72, Height = 72, HorizontalAlignment = HorizontalAlignment.Left };
            m_contenu.Children.Add(logo);
            var t = T(titre, 26, Texte, FontWeights.Bold, Titre);
            t.Margin = new Thickness(0, 14, 0, 0);
            m_contenu.Children.Add(t);
            var p = Para(texte, 14, Doux);
            p.Margin = new Thickness(0, 8, 0, 0);
            m_contenu.Children.Add(p);
        }

        void Accueil()
        {
            var deja = InstallationLauncher.EstInstalle;
            Entete(deja ? "Padgref est déjà installé" : "Installer Padgref",
                deja ? "Tu peux lancer Padgref directement, ou réinstaller cette version du launcher (tes comptes et ta connexion sont gardés)."
                     : "Le launcher Padgref va s'installer sur ce PC (aucun droit administrateur demandé), avec une icône sur le Bureau et dans le menu Démarrer.");
            var dossier = T(InstallationLauncher.Dossier, 12, Doux, FontWeights.Normal);
            dossier.FontFamily = new FontFamily("Consolas");
            dossier.Margin = new Thickness(0, 12, 0, 0);
            m_contenu.Children.Add(dossier);
            var c = CaseTexte("Créer une icône Padgref sur le Bureau", true, out m_bureau);
            c.Margin = new Thickness(0, 16, 0, 0);
            m_contenu.Children.Add(c);
            var boutons = new Grid { Margin = new Thickness(0, 22, 0, 0) };
            boutons.ColumnDefinitions.Add(new ColumnDefinition());
            boutons.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(12) });
            boutons.ColumnDefinitions.Add(new ColumnDefinition());
            var installer = Gros(deja ? "Réinstaller" : "Installer", null, Installer, 52);
            boutons.Children.Add(installer);
            var second = deja ? SecondTexte("Lancer Padgref", LancerEtFermer, 52, 14.5) : SecondTexte("Annuler", Close, 52, 14.5);
            Grid.SetColumn(second, 2);
            boutons.Children.Add(second);
            m_contenu.Children.Add(boutons);
        }

        void Installer()
        {
            var bureau = m_bureau();
            Entete("Installation…", "Copie du launcher et création des raccourcis.");
            var piste = new Grid { Height = 10, Margin = new Thickness(0, 22, 0, 0), ClipToBounds = true };
            piste.Children.Add(new Border { CornerRadius = new CornerRadius(5), Background = B(Carte) });
            var barre = new Border { CornerRadius = new CornerRadius(5), Background = Degrade(), HorizontalAlignment = HorizontalAlignment.Left, Width = 0 };
            piste.Children.Add(barre);
            m_contenu.Children.Add(piste);
            var etape = T("Préparation…", 13, Doux, FontWeights.SemiBold);
            etape.Margin = new Thickness(0, 10, 0, 0);
            m_contenu.Children.Add(etape);
            var largeur = L - 80;
            Action<double, string> avancer = (f, t) =>
            {
                etape.Text = t;
                barre.BeginAnimation(WidthProperty, new DoubleAnimation(largeur * f, TimeSpan.FromSeconds(0.35)) { EasingFunction = new PowerEase { Power = 2, EasingMode = EasingMode.EaseOut } });
            };
            avancer(0.15, "Copie du launcher dans " + InstallationLauncher.Dossier + "…");
            if (m_apercu != null) return;
            new Thread(() =>
            {
                Exception erreur = null;
                Thread.Sleep(500);
                try { InstallationLauncher.Installer(m_updater, bureau); } catch (Exception ex) { erreur = ex; }
                Dispatcher.BeginInvoke((Action)(() => avancer(0.7, bureau ? "Icône Padgref sur le Bureau et dans le menu Démarrer…" : "Raccourci dans le menu Démarrer…")));
                Thread.Sleep(600);
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    if (erreur != null)
                    {
                        m_updater.Log("Installation impossible : " + erreur.Message);
                        Entete("Installation impossible", erreur.Message + Environment.NewLine + "Ferme Padgref s'il est ouvert, puis réessaie.");
                        var r = Gros("Réessayer", null, Accueil, 50);
                        r.Margin = new Thickness(0, 20, 0, 0);
                        m_contenu.Children.Add(r);
                        return;
                    }
                    avancer(1, "Terminé");
                    var t = new DispatcherTimer { Interval = TimeSpan.FromSeconds(0.45) };
                    t.Tick += (s, e) => { t.Stop(); Termine(bureau); };
                    t.Start();
                }));
            }) { IsBackground = true }.Start();
        }

        void Termine(bool bureau)
        {
            Entete("Installation terminée", bureau
                ? "Padgref est installé. Pour jouer, lance-le depuis l'icône Padgref sur ton Bureau (ou le menu Démarrer)."
                : "Padgref est installé. Pour jouer, lance-le depuis le menu Démarrer (Padgref).");
            var ok = new Border { Width = 34, Height = 34, CornerRadius = new CornerRadius(17), Background = B(Vert), Child = Ic(IcCoche, 20, B(AccentTxt), false, 3), HorizontalAlignment = HorizontalAlignment.Left };
            ok.Margin = new Thickness(0, 14, 0, 0);
            m_contenu.Children.Insert(1, ok);
            var lancer = Gros("Lancer Padgref", IcJouer, LancerEtFermer, 54, 16);
            lancer.Margin = new Thickness(0, 24, 0, 0);
            m_contenu.Children.Add(lancer);
        }

        void LancerEtFermer()
        {
            if (m_apercu != null) return;
            try { InstallationLauncher.Lancer(null); } catch (Exception ex) { m_updater.Log("Lancement impossible : " + ex.Message); }
            Close();
        }

        void Apercu()
        {
            Directory.CreateDirectory(m_apercu);
            var etapes = new Action[] { () => { }, Installer, () => Termine(true) };
            var noms = new[] { "installeur_01_accueil", "installeur_02_progression", "installeur_03_termine" };
            var i = 0;
            var faite = false;
            var t = new DispatcherTimer { Interval = TimeSpan.FromSeconds(0.6) };
            t.Tick += (s, e) =>
            {
                if (i >= etapes.Length) { t.Stop(); Close(); return; }
                if (!faite) { etapes[i](); faite = true; return; }
                LauncherWindow.Capturer((Visual)Content, (int)L, (int)Ht, System.IO.Path.Combine(m_apercu, noms[i] + ".png"));
                i++;
                faite = false;
            };
            t.Start();
        }
    }
}
