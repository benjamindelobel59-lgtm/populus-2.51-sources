// MYSTIC 18/10/2026 → PADGREF 20/10/2026 : launcher Padgref (WPF). Nouveau design « Azur » (maquette v2 launcher-padgref.html) :
// fenêtre de connexion = vue « Connexion Zaap » (image à gauche, formulaire à droite, « Se connecter avec le site Padgref »),
// puis grand launcher (Principal.cs) : comptes de jeu du compte web, installation complète et mises à jour du jeu
// (Installation.cs), JOUER avec connexion automatique. Plus d'intro animée.
//
// Options : --auto (met à jour sans fenêtre, code 0 = à jour) ; --portable (ne s'installe pas, voir Installeur.cs) ; --apercu <dossier> (captures PNG des écrans, aucun accès
// réseau au serveur) ; launcher.ini à côté de l'exe : « url=http://… » pour changer de serveur.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using static MysticLauncher.Theme;

// PADGREF 20/10 : nom affiché par Windows (Gestionnaire des tâches, propriétés du fichier, notifications) ; le fichier reste MysticLauncher.exe
[assembly: System.Reflection.AssemblyTitle("Padgref")]
[assembly: System.Reflection.AssemblyDescription("Launcher Padgref")]
[assembly: System.Reflection.AssemblyProduct("Padgref")]
[assembly: System.Reflection.AssemblyCompany("Padgref")]
[assembly: System.Reflection.AssemblyCopyright("Padgref")]
[assembly: System.Reflection.AssemblyVersion("2.0.0.0")]
[assembly: System.Reflection.AssemblyFileVersion("2.0.0.0")]

namespace MysticLauncher
{
    static class Program
    {
        // MYSTIC 20/10 : nom de domaine padgref.fr ; tant qu'il ne pointe pas vers le VPS (DNS pas encore actif, page
        // d'attente OVH…), le launcher garde l'IP. launcher.ini (url= / api= / site=) peut toujours tout remplacer.
        public const string Domaine = "padgref.fr";
        public const string IpVps = "IP_DU_VPS";
        static string s_hote;
        public static string Hote
        {
            get
            {
                if (s_hote != null) return s_hote;
                s_hote = IpVps;
                try
                {
                    var t = System.Threading.Tasks.Task.Run(() => System.Net.Dns.GetHostAddresses(Domaine));
                    if (t.Wait(2500) && System.Linq.Enumerable.Any(t.Result, a => a.ToString() == IpVps))
                        s_hote = Domaine;
                }
                catch { }
                return s_hote;
            }
        }
        public static string DefaultUrl => "http://" + Hote + "/maj/";
        public static string DefaultApi => "http://" + Hote + ":8090/";
        public static string DefaultSite => "http://" + Hote + "/";

        [STAThread]
        static int Main(string[] args)
        {
            var updater = new Updater(AppDomain.CurrentDomain.BaseDirectory);
            if (Array.IndexOf(args, "--auto") >= 0)
                return updater.Run(null) ? 0 : 1;
            // --installer <dossier> : installation complète sans fenêtre (tests), progression dans launcher.log
            var inst = Array.IndexOf(args, "--installer");
            if (inst >= 0 && inst + 1 < args.Length)
            {
                updater.UtiliserDossier(args[inst + 1]);
                var dernier = -1;
                return updater.Run((t, p) => { if (p / 10 != dernier) { dernier = p / 10; updater.Log(p + " % " + t); } }, true) ? 0 : 1;
            }

            string apercu = null;
            var i = Array.IndexOf(args, "--apercu");
            if (i >= 0 && i + 1 < args.Length)
                apercu = args[i + 1];

            // rendus des personnages en HTTPS (TLS 1.2)
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
            // PADGREF 20/10 : installation dans %LOCALAPPDATA%\Padgref\Launcher + raccourcis + mise à jour de l'exe (Installeur.cs)
            if (apercu == null && Array.IndexOf(args, "--portable") < 0 && InstallationLauncher.Preparer(updater, args))
                return 0;
            var app = new Application { ShutdownMode = ShutdownMode.OnMainWindowClose };
            var fenetre = new LauncherWindow(updater, apercu);
            app.Run(fenetre);
            return 0;
        }
    }

    // ------------------------------------------------------------------------------------------------------------------
    // Fenêtre de connexion (« Connexion Zaap »)
    // ------------------------------------------------------------------------------------------------------------------
    partial class LauncherWindow : Window
    {
        readonly Updater m_updater;
        readonly string m_apercu;
        readonly Grid m_racine = new Grid();

        // anneau du Zaap : tourne lentement, vite pendant une connexion, vert une fois connecté
        Ellipse m_anneau;
        RotateTransform m_rotation;
        DropShadowEffect m_lueurZaap;

        public LauncherWindow(Updater updater, string apercu)
        {
            m_updater = updater;
            m_apercu = apercu;

            Title = Theme.Nom;
            Width = Theme.W;
            Height = Theme.H;
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Background = Brushes.Transparent;
            ResizeMode = ResizeMode.CanMinimize;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            UseLayoutRounding = true;
            TextOptions.SetTextFormattingMode(this, TextFormattingMode.Ideal);
            // icône des fenêtres = icône Padgref intégrée à l'exe (compilé avec -win32icon:ressources/padgref.ico)

            m_api = new CompteApi(updater.ApiUrl);

            var cadre = new Border
            {
                CornerRadius = new CornerRadius(14), Background = B(Fond), BorderBrush = B(Ligne), BorderThickness = new Thickness(1), Child = m_racine
            };
            m_racine.Clip = new RectangleGeometry(new Rect(0, 0, Theme.W - 2, Theme.H - 2), 13, 13);
            Content = cadre;

            m_racine.RowDefinitions.Add(new RowDefinition { Height = new GridLength(Theme.BARRE) });
            m_racine.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            m_racine.Children.Add(Theme.BarreTitre(this, ConstruireStatutServeur(), Fermer));

            var corps = new Grid();
            corps.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.25, GridUnitType.Star) });
            corps.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            Grid.SetRow(corps, 1);
            m_racine.Children.Add(corps);
            corps.Children.Add(ConstruireArt());
            var formulaire = ConstruireCarteCompte();
            Grid.SetColumn(formulaire, 1);
            corps.Children.Add(formulaire);

            Loaded += (s, e) =>
            {
                cadre.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.3)));
                if (m_apercu != null)
                    LancerApercu();
                else
                {
                    SuivreStatut();
                    ReprendreSession();
                    m_champEmail.Focus();
                }
            };
        }

        /// <summary>Image d'accueil à gauche, fondue vers le formulaire, avec la légende « Bienvenue sur Padgref ».</summary>
        FrameworkElement ConstruireArt()
        {
            var art = new Grid { ClipToBounds = true };
            var zoom = new ScaleTransform(1.02, 1.02);
            var img = new System.Windows.Controls.Image { Source = Theme.Image("p_bienvenue.jpg"), Stretch = Stretch.UniformToFill, RenderTransformOrigin = new Point(0.5, 0.5), RenderTransform = zoom };
            art.Children.Add(img);
            // très lent mouvement de caméra (ambiance, pas une intro)
            var lent = new DoubleAnimation(1.02, 1.08, TimeSpan.FromSeconds(24)) { AutoReverse = true, RepeatBehavior = RepeatBehavior.Forever, EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut } };
            zoom.BeginAnimation(ScaleTransform.ScaleXProperty, lent);
            zoom.BeginAnimation(ScaleTransform.ScaleYProperty, lent);
            art.Children.Add(new Rectangle
            {
                Fill = new LinearGradientBrush(new GradientStopCollection { new GradientStop(Colors.Transparent, 0.4), new GradientStop(Fond, 1) }, new Point(0, 0.5), new Point(1, 0.5))
            });
            art.Children.Add(new Rectangle
            {
                Fill = new LinearGradientBrush(new GradientStopCollection { new GradientStop(A(0xCC, Fond), 1), new GradientStop(Colors.Transparent, 0.5) }, new Point(0.5, 0), new Point(0.5, 1))
            });
            var legende = new StackPanel { VerticalAlignment = VerticalAlignment.Bottom, HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(36, 0, 0, 34), MaxWidth = 420 };
            legende.Children.Add(new System.Windows.Controls.Image { Source = Theme.Image("p_logo.png"), Width = 64, Height = 64, HorizontalAlignment = HorizontalAlignment.Left });
            var titre = T("Bienvenue sur Padgref", 30, Texte, FontWeights.Bold, Titre);
            titre.Margin = new Thickness(0, 10, 0, 0);
            legende.Children.Add(titre);
            var p = Para("Un seul compte Padgref pour le site, le launcher et tous tes comptes de jeu.", 15, Doux);
            p.Margin = new Thickness(0, 8, 0, 0);
            legende.Children.Add(p);
            // PADGREF 20/10 : invitation Discord bien visible dès l'écran de connexion
            var discord = Theme.BoutonDiscord(() => Outils.OuvrirSite(Theme.Discord), 42);
            discord.HorizontalAlignment = HorizontalAlignment.Left;
            discord.Margin = new Thickness(0, 16, 0, 0);
            legende.Children.Add(discord);
            art.Children.Add(legende);
            return art;
        }

        /// <summary>Le Zaap de la maquette : anneau dégradé qui tourne autour de l'icône.</summary>
        FrameworkElement ConstruireZaap()
        {
            const double t = 104;
            var g = new Grid { Width = t, Height = t, HorizontalAlignment = HorizontalAlignment.Left };
            m_rotation = new RotateTransform(0);
            m_anneau = new Ellipse
            {
                StrokeThickness = 6, RenderTransformOrigin = new Point(0.5, 0.5), RenderTransform = m_rotation,
                Stroke = new LinearGradientBrush(new GradientStopCollection
                {
                    new GradientStop(Accent2, 0), new GradientStop(Colors.Transparent, 0.32), new GradientStop(Accent, 0.62), new GradientStop(Colors.Transparent, 0.86), new GradientStop(Accent2, 1)
                }, new Point(0, 0), new Point(1, 1))
            };
            g.Children.Add(m_anneau);
            m_lueurZaap = new DropShadowEffect { Color = Accent, BlurRadius = 22, ShadowDepth = 0, Opacity = 0.9 };
            g.Children.Add(new System.Windows.Controls.Image { Source = Theme.Image("p_zaap.png"), Width = 66, Height = 66, Effect = m_lueurZaap });
            VitesseZaap(6);
            return g;
        }

        void VitesseZaap(double secondes)
        {
            m_rotation.BeginAnimation(RotateTransform.AngleProperty, new DoubleAnimation(m_rotation.Angle, m_rotation.Angle + 360, TimeSpan.FromSeconds(secondes)) { RepeatBehavior = RepeatBehavior.Forever });
        }

        /// <summary>Zaap : 0 = repos, 1 = connexion en cours, 2 = connecté.</summary>
        void EtatZaap(int etat)
        {
            if (m_anneau == null) return;
            if (etat == 2)
            {
                m_rotation.BeginAnimation(RotateTransform.AngleProperty, null);
                m_anneau.Stroke = B(Vert);
                m_lueurZaap.Color = Vert;
                return;
            }
            VitesseZaap(etat == 1 ? 0.9 : 6);
        }

        void Fermer()
        {
            var fondu = new DoubleAnimation(0, TimeSpan.FromSeconds(0.25)) { EasingFunction = new PowerEase { EasingMode = EasingMode.EaseIn } };
            fondu.Completed += (s, e) => Close();
            BeginAnimation(OpacityProperty, fondu);
        }

        // ---------------------------------------------------------------- aperçu (captures)

        void LancerApercu()
        {
            Directory.CreateDirectory(m_apercu);
            var etapes = new List<Tuple<double, Action, string>>
            {
                Tuple.Create(0.5, (Action)(() => AfficherStatut(m_point, m_statutServeur, Tuple.Create(true, 47))), "connexion_01"),
                Tuple.Create(1.2, (Action)ApercuConnexion, "connexion_02_rempli"),
                Tuple.Create(1.9, (Action)(() => { EtatZaap(1); m_message.Foreground = B(Accent2); m_message.Text = "Valide la connexion dans la page du site qui vient de s'ouvrir." + Environment.NewLine + "Code à vérifier : 3F9-A41"; }), "connexion_03_site"),
            };
            var debut = DateTime.Now;
            var index = 0;
            var faite = false;
            var minuteur = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(30) };
            minuteur.Tick += (s, e) =>
            {
                var t = (DateTime.Now - debut).TotalSeconds;
                if (index >= etapes.Count) { minuteur.Stop(); ApercuOuvrir(); return; }
                if (!faite && t >= etapes[index].Item1) { etapes[index].Item2(); faite = true; }
                if (faite && t >= etapes[index].Item1 + 0.5)
                {
                    Capturer((Visual)Content, (int)Theme.W, (int)Theme.H, System.IO.Path.Combine(m_apercu, etapes[index].Item3 + ".png"));
                    index++;
                    faite = false;
                }
            };
            minuteur.Start();
        }

        public static void Capturer(Visual v, int l, int h, string fichier)
        {
            var rtb = new RenderTargetBitmap(l, h, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(v);
            var enc = new PngBitmapEncoder();
            enc.Frames.Add(BitmapFrame.Create(rtb));
            using (var fs = File.Create(fichier))
                enc.Save(fs);
        }
    }
}
