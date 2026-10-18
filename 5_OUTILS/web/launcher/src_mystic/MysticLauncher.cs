// MYSTIC 18/10/2026 : launcher Mystic (WPF). Fenêtre de connexion en motion design (héros du site : ange / démon, couture
// lumineuse, étincelles, logo qui respire) puis grand launcher façon Zaap (Principal.cs) : comptes de jeu du compte web,
// installation complète et mises à jour du jeu (Installation.cs), JOUER avec connexion automatique.
//
// Options : --auto (met à jour sans fenêtre, code 0 = à jour) ; --apercu <dossier> (captures PNG de l'animation, progression
// simulée, aucun accès réseau) ; launcher.ini à côté de l'exe : « url=http://… » pour changer de serveur.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MysticLauncher
{
    static class Program
    {
        public const string DefaultUrl = "http://IP_DU_VPS/maj/";
        public const string DefaultApi = "http://IP_DU_VPS:8090/";
        public const string DefaultSite = "http://IP_DU_VPS/";

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
            var app = new Application { ShutdownMode = ShutdownMode.OnMainWindowClose };
            var fenetre = new LauncherWindow(updater, apercu);
            app.Run(fenetre);
            return 0;
        }
    }

    // ------------------------------------------------------------------------------------------------------------------
    // Fenêtre
    // ------------------------------------------------------------------------------------------------------------------
    partial class LauncherWindow : Window
    {
        const double W = 1100, H = 620;
        const double PANEL_H = 92, MARGE = 22;

        static readonly Color Nuit = Rgb(0x07, 0x06, 0x0b);
        static readonly Color Argent = Rgb(0xec, 0xe8, 0xf2);
        static readonly Color Argent2 = Rgb(0xb8, 0xb0, 0xc8);
        static readonly Color Brume = Rgb(0x8f, 0x86, 0xa3);
        static readonly Color Violet = Rgb(0x8b, 0x5c, 0xf6);
        static readonly Color VioletClair = Rgb(0xb7, 0x9c, 0xff);
        static readonly Color VioletSombre = Rgb(0x4c, 0x1d, 0x95);
        static readonly Color Ange = Rgb(0xa9, 0xcb, 0xff);
        static readonly Color Demon = Rgb(0xff, 0x6a, 0x4a);

        readonly Updater m_updater;
        readonly string m_apercu;

        readonly Canvas m_scene = new Canvas { Width = W, Height = H, ClipToBounds = true };
        readonly TranslateTransform m_secousse = new TranslateTransform();
        readonly TranslateTransform m_fondParallax = new TranslateTransform();
        readonly TranslateTransform m_peintParallaxG = new TranslateTransform();
        readonly TranslateTransform m_peintParallaxD = new TranslateTransform();
        readonly TranslateTransform m_logoParallax = new TranslateTransform();
        Rectangle m_voileG, m_voileD, m_eclatG, m_eclatD;
        TextBlock m_campG, m_campD;
        TranslateTransform m_campGT, m_campDT;
        int m_focus; // -1 gauche, 0 aucun, 1 droite

        // panneau
        TextBlock m_statut, m_pourcent, m_version, m_reessayer;
        Border m_barre, m_jouer;
        TextBlock m_jouerTexte;
        DropShadowEffect m_jouerLueur;
        ScaleTransform m_jouerEchelle;
        Rectangle m_jouerReflet;
        double m_largeurPiste;
        bool m_pret, m_erreur, m_lance;

        // particules
        readonly List<Particule> m_particules = new List<Particule>();
        readonly Random m_rand = new Random();
        readonly Stopwatch m_horloge = Stopwatch.StartNew();
        double m_dernier;
        Point m_souris = new Point(W / 2, H / 2);
        double m_px, m_py;
        Brush[] m_brossesAnge, m_brossesDemon;

        public LauncherWindow(Updater updater, string apercu)
        {
            m_updater = updater;
            m_apercu = apercu;

            Title = "Mystic";
            Width = W;
            Height = H;
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Background = Brushes.Transparent;
            ResizeMode = ResizeMode.CanMinimize;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            UseLayoutRounding = true;
            // MYSTIC 18/10 : icône des fenêtres = icône Mystic intégrée à l'exe (compilé avec -win32icon:ressources/mystic.ico)

            var image = Charger("ange_demon.jpg");
            var logo = Charger("logo_mystic.png");

            m_scene.Background = new SolidColorBrush(Colors.Black);
            m_scene.RenderTransform = m_secousse;
            m_scene.Clip = new RectangleGeometry(new Rect(0, 0, W, H), 16, 16);
            TextOptions.SetTextFormattingMode(m_scene, TextFormattingMode.Ideal);

            ConstruireFond(image);
            ConstruireMoities(image);
            ConstruireCouture();
            ConstruireParticules();
            ConstruireCamps();
            ConstruireVignette();
            var logoScene = ConstruireLogo(logo);
            var panneau = ConstruirePanneau();
            panneau.Visibility = Visibility.Collapsed; // MYSTIC 18/10 : mises à jour et JOUER dans le grand launcher
            var barreHaut = ConstruireBarreHaut();
            m_api = new CompteApi(updater.ApiUrl);
            ConstruireStatutServeur();
            ConstruireCarteCompte();
            var flash = new Rectangle { Width = W, Height = H, Fill = new SolidColorBrush(Rgb(0xf3, 0xec, 0xff)), Opacity = 0, IsHitTestVisible = false };
            m_scene.Children.Add(flash);

            Content = m_scene;

            m_scene.MouseMove += (s, e) => { m_souris = e.GetPosition(m_scene); MettreAJourFocus(); };
            m_scene.MouseLeave += (s, e) => { m_souris = new Point(W / 2, H / 2); DefinirFocus(0); };

            Loaded += (s, e) =>
            {
                Intro(logoScene, panneau, barreHaut, flash);
                CompositionTarget.Rendering += Image_Suivante;
                AfficherCarte();
                if (m_apercu != null)
                    LancerApercu();
                else
                {
                    SuivreStatut();
                    ReprendreSession();
                }
            };
        }

        // ---------------------------------------------------------------- construction

        static Color Rgb(byte r, byte g, byte b) => Color.FromRgb(r, g, b);
        static Color Argb(byte a, Color c) => Color.FromArgb(a, c.R, c.G, c.B);

        static BitmapImage Charger(string nom)
        {
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.StreamSource = Assembly.GetExecutingAssembly().GetManifestResourceStream(nom);
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.EndInit();
            bmp.Freeze();
            return bmp;
        }

        static string Espace(string s, string sep = " ")
        {
            var sb = new StringBuilder();
            foreach (var c in s)
            {
                if (sb.Length > 0) sb.Append(sep);
                sb.Append(c);
            }
            return sb.ToString();
        }

        void Placer(UIElement e, double x, double y)
        {
            Canvas.SetLeft(e, x);
            Canvas.SetTop(e, y);
            m_scene.Children.Add(e);
        }

        void ConstruireFond(ImageSource image)
        {
            // fond flou derrière les deux moitiés (visible pendant leur arrivée)
            var fond = new Rectangle
            {
                Width = W * 1.12,
                Height = H * 1.12,
                Fill = new ImageBrush(image) { Stretch = Stretch.UniformToFill },
                Effect = new BlurEffect { Radius = 18, KernelType = KernelType.Gaussian },
                Opacity = 0.32,
                RenderTransform = m_fondParallax
            };
            Placer(fond, -W * 0.06, -H * 0.06);

            var halo = new Rectangle
            {
                Width = W,
                Height = H,
                Fill = new RadialGradientBrush(Argb(0x70, VioletSombre), Colors.Transparent) { RadiusX = 0.6, RadiusY = 0.6 }
            };
            Placer(halo, 0, 0);
        }

        void ConstruireMoities(ImageSource image)
        {
            for (var cote = 0; cote < 2; cote++)
            {
                var gauche = cote == 0;
                var moitie = new Canvas { Width = W / 2, Height = H, ClipToBounds = true };
                var entree = new Canvas { Width = W / 2, Height = H };
                var arrivee = new TranslateTransform(gauche ? -W / 2 : W / 2, 0);
                entree.RenderTransform = arrivee;

                var peint = new Rectangle
                {
                    Width = W * 1.0,
                    Height = H * 1.08,
                    Fill = new ImageBrush(image) { Stretch = Stretch.UniformToFill },
                    RenderTransform = gauche ? m_peintParallaxG : m_peintParallaxD
                };
                Canvas.SetTop(peint, -H * 0.04);
                // la couture de l'image tombe exactement sous la couture lumineuse (W / 2)
                Canvas.SetLeft(peint, gauche ? 0 : -W / 2);
                entree.Children.Add(peint);

                // teinte du camp + fondu bas (comme .moitie::after du site)
                var teinte = new Rectangle
                {
                    Width = W / 2,
                    Height = H,
                    Fill = new LinearGradientBrush(
                        new GradientStopCollection
                        {
                            new GradientStop(gauche ? Color.FromArgb(46, 80, 130, 255) : Color.FromArgb(51, 255, 40, 20), 0),
                            new GradientStop(Colors.Transparent, 0.55)
                        }, gauche ? new Point(0, 0.5) : new Point(1, 0.5), gauche ? new Point(1, 0.5) : new Point(0, 0.5))
                };
                entree.Children.Add(teinte);

                var voile = new Rectangle { Width = W / 2, Height = H, Fill = Brushes.Black, Opacity = 0 };
                var eclat = new Rectangle { Width = W / 2, Height = H, Fill = new SolidColorBrush(gauche ? Ange : Demon), Opacity = 0 };
                entree.Children.Add(voile);
                entree.Children.Add(eclat);
                if (gauche) { m_voileG = voile; m_eclatG = eclat; } else { m_voileD = voile; m_eclatD = eclat; }

                moitie.Children.Add(entree);
                Placer(moitie, gauche ? 0 : W / 2, 0);

                arrivee.BeginAnimation(TranslateTransform.XProperty, new DoubleAnimation(gauche ? -W / 2 : W / 2, 0, TimeSpan.FromSeconds(1.5))
                {
                    EasingFunction = new PowerEase { Power = 4, EasingMode = EasingMode.EaseInOut }
                });
            }
        }

        void ConstruireCouture()
        {
            // lueur large derrière la couture
            var lueur = new Rectangle
            {
                Width = 180,
                Height = H,
                Fill = new LinearGradientBrush(new GradientStopCollection
                {
                    new GradientStop(Colors.Transparent, 0),
                    new GradientStop(Argb(0x40, Violet), 0.5),
                    new GradientStop(Colors.Transparent, 1)
                }, new Point(0, 0.5), new Point(1, 0.5)),
                Opacity = 0,
                IsHitTestVisible = false
            };
            Placer(lueur, W / 2 - 90, 0);

            var couture = new Rectangle
            {
                Width = 2,
                Height = H,
                Fill = new LinearGradientBrush(new GradientStopCollection
                {
                    new GradientStop(Colors.Transparent, 0),
                    new GradientStop(Colors.White, 0.18),
                    new GradientStop(Rgb(0xc9, 0xb6, 0xff), 0.5),
                    new GradientStop(Colors.White, 0.82),
                    new GradientStop(Colors.Transparent, 1)
                }, new Point(0.5, 0), new Point(0.5, 1)),
                Effect = new DropShadowEffect { Color = VioletClair, BlurRadius = 26, ShadowDepth = 0, Opacity = 1 },
                Opacity = 0,
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new ScaleTransform(1, 0),
                IsHitTestVisible = false
            };
            Placer(couture, W / 2 - 1, 0);

            // éclat qui descend le long de la couture
            var eclat = new Rectangle
            {
                Width = 4,
                Height = 120,
                RadiusX = 2,
                RadiusY = 2,
                Fill = new LinearGradientBrush(new GradientStopCollection
                {
                    new GradientStop(Colors.Transparent, 0),
                    new GradientStop(Colors.White, 0.5),
                    new GradientStop(Colors.Transparent, 1)
                }, new Point(0.5, 0), new Point(0.5, 1)),
                Effect = new DropShadowEffect { Color = Colors.White, BlurRadius = 18, ShadowDepth = 0, Opacity = 1 },
                Opacity = 0,
                IsHitTestVisible = false
            };
            var descente = new TranslateTransform(0, -140);
            eclat.RenderTransform = descente;
            Placer(eclat, W / 2 - 2, 0);

            var t0 = TimeSpan.FromSeconds(1.15);
            couture.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.25)) { BeginTime = t0 });
            couture.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.45))
            {
                BeginTime = t0,
                EasingFunction = new PowerEase { Power = 3, EasingMode = EasingMode.EaseOut }
            });
            lueur.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.6)) { BeginTime = t0 });

            // respiration de la lueur
            var respire = new DoubleAnimation(1, 0.55, TimeSpan.FromSeconds(2.4))
            {
                BeginTime = TimeSpan.FromSeconds(2),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever,
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };
            lueur.BeginAnimation(OpacityProperty, null);
            var sbLueur = new Storyboard();
            var entreeLueur = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.6)) { BeginTime = t0 };
            Storyboard.SetTarget(entreeLueur, lueur);
            Storyboard.SetTargetProperty(entreeLueur, new PropertyPath(OpacityProperty));
            sbLueur.Children.Add(entreeLueur);
            sbLueur.Completed += (s, e) => lueur.BeginAnimation(OpacityProperty, respire);
            sbLueur.Begin();

            // éclat : descend toutes les 4,5 s
            var chute = new DoubleAnimationUsingKeyFrames { BeginTime = TimeSpan.FromSeconds(2.2), RepeatBehavior = RepeatBehavior.Forever, Duration = TimeSpan.FromSeconds(4.5) };
            chute.KeyFrames.Add(new DiscreteDoubleKeyFrame(-140, KeyTime.FromTimeSpan(TimeSpan.Zero)));
            chute.KeyFrames.Add(new EasingDoubleKeyFrame(H + 20, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(1.6)), new SineEase { EasingMode = EasingMode.EaseInOut }));
            chute.KeyFrames.Add(new DiscreteDoubleKeyFrame(H + 20, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(4.5))));
            descente.BeginAnimation(TranslateTransform.YProperty, chute);
            eclat.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 0.9, TimeSpan.FromSeconds(0.3)) { BeginTime = TimeSpan.FromSeconds(2.2) });
        }

        void ConstruireParticules()
        {
            Func<Color, Brush> brosse = c =>
            {
                var b = new RadialGradientBrush(new GradientStopCollection
                {
                    new GradientStop(Colors.White, 0),
                    new GradientStop(c, 0.35),
                    new GradientStop(Argb(0, c), 1)
                });
                b.Freeze();
                return b;
            };
            m_brossesAnge = new[] { brosse(Rgb(0xdb, 0xe8, 0xff)), brosse(Ange), brosse(Rgb(0x7f, 0xb0, 0xff)), brosse(VioletClair) };
            m_brossesDemon = new[] { brosse(Rgb(0xff, 0xb3, 0x8a)), brosse(Demon), brosse(Rgb(0xff, 0x3b, 0x1f)), brosse(Rgb(0xff, 0x8c, 0x3a)) };

            var couche = new Canvas { Width = W, Height = H, IsHitTestVisible = false };
            for (var i = 0; i < 140; i++)
            {
                var p = new Particule { Forme = new Ellipse() };
                couche.Children.Add(p.Forme);
                Renaitre(p, true);
                m_particules.Add(p);
            }
            couche.Opacity = 0;
            couche.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromSeconds(1.2)) { BeginTime = TimeSpan.FromSeconds(1.2) });
            Placer(couche, 0, 0);
        }

        void Renaitre(Particule p, bool partout)
        {
            // plus de particules près de la couture
            var u = m_rand.NextDouble() * 2 - 1;
            var x = W / 2 + Math.Sign(u) * Math.Pow(Math.Abs(u), 1.6) * W * 0.5;
            var ange = x < W / 2;
            p.X = x;
            p.Y = partout ? m_rand.NextDouble() * H : H + 10 + m_rand.NextDouble() * 40;
            p.Taille = 3 + Math.Pow(m_rand.NextDouble(), 2) * 10;
            p.Vy = -(18 + m_rand.NextDouble() * 46) * (ange ? 0.85 : 1.15);
            p.Vx = (m_rand.NextDouble() - 0.5) * 10;
            p.Phase = m_rand.NextDouble() * Math.PI * 2;
            p.Amplitude = 6 + m_rand.NextDouble() * 18;
            p.Vie = 0;
            p.VieMax = 4 + m_rand.NextDouble() * 6;
            p.Alpha = 0.55 + m_rand.NextDouble() * 0.45;
            p.Scintille = ange;
            var brosses = ange ? m_brossesAnge : m_brossesDemon;
            p.Forme.Fill = brosses[m_rand.Next(brosses.Length)];
            p.Forme.Width = p.Taille;
            p.Forme.Height = p.Taille;
            if (partout)
                p.Vie = m_rand.NextDouble() * p.VieMax;
        }

        void ConstruireCamps()
        {
            Func<string, Color, TextBlock> nom = (t, c) => new TextBlock
            {
                Text = Espace(t, " "),
                FontFamily = new FontFamily("Segoe UI"),
                FontWeight = FontWeights.Black,
                FontSize = 30,
                Foreground = new SolidColorBrush(c),
                Opacity = 0,
                IsHitTestVisible = false,
                Effect = new DropShadowEffect { Color = Colors.Black, BlurRadius = 14, ShadowDepth = 2, Opacity = 1 }
            };
            m_campG = nom("BONTA", Ange);
            m_campD = nom("BRÂKMAR", Demon);
            m_campGT = new TranslateTransform(-24, 0);
            m_campDT = new TranslateTransform(24, 0);
            m_campG.RenderTransform = m_campGT;
            m_campD.RenderTransform = m_campDT;
            Placer(m_campG, W * 0.06, H * 0.2);
            m_campD.Loaded += (s, e) => Canvas.SetLeft(m_campD, W - MARGE - CARTE_L - 24 - m_campD.ActualWidth);
            Placer(m_campD, W * 0.7, H * 0.2);
        }

        void ConstruireVignette()
        {
            var vignette = new Rectangle
            {
                Width = W,
                Height = H,
                IsHitTestVisible = false,
                Fill = new RadialGradientBrush(new GradientStopCollection
                {
                    new GradientStop(Colors.Transparent, 0.55),
                    new GradientStop(Argb(0xb0, Nuit), 1)
                }) { RadiusX = 0.75, RadiusY = 0.8 }
            };
            Placer(vignette, 0, 0);

            var bas = new Rectangle
            {
                Width = W,
                Height = 300,
                IsHitTestVisible = false,
                Fill = new LinearGradientBrush(new GradientStopCollection
                {
                    new GradientStop(Colors.Transparent, 0),
                    new GradientStop(Argb(0xd9, Nuit), 0.75),
                    new GradientStop(Nuit, 1)
                }, new Point(0.5, 0), new Point(0.5, 1))
            };
            Placer(bas, 0, H - 300);

            var haut = new Rectangle
            {
                Width = W,
                Height = 90,
                IsHitTestVisible = false,
                Fill = new LinearGradientBrush(Argb(0xa0, Nuit), Colors.Transparent, 90)
            };
            Placer(haut, 0, 0);
        }

        FrameworkElement ConstruireLogo(ImageSource logo)
        {
            const double largeur = 330;
            var hauteur = largeur * logo.Height / logo.Width;

            var scene = new Grid { Width = largeur, Height = hauteur, RenderTransformOrigin = new Point(0.5, 0.5), IsHitTestVisible = false };
            var echelle = new ScaleTransform(0.82, 0.82);
            var groupe = new TransformGroup();
            groupe.Children.Add(echelle);
            groupe.Children.Add(m_logoParallax);
            scene.RenderTransform = groupe;

            var lueur = new DropShadowEffect { Color = Violet, BlurRadius = 30, ShadowDepth = 0, Opacity = 0.6 };
            var img = new Image { Source = logo, Stretch = Stretch.Uniform, Effect = lueur };
            scene.Children.Add(img);

            // reflet qui balaye le logo (masqué par le logo, comme .reflet du site)
            var balayage = new TranslateTransform(-1.3, 0);
            var reflet = new Rectangle
            {
                Fill = new LinearGradientBrush(new GradientStopCollection
                {
                    new GradientStop(Colors.Transparent, 0.42),
                    new GradientStop(Color.FromArgb(0xc0, 0xff, 0xff, 0xff), 0.5),
                    new GradientStop(Colors.Transparent, 0.58)
                }, new Point(0, 0.2), new Point(1, 0.8)) { RelativeTransform = balayage },
                OpacityMask = new ImageBrush(logo) { Stretch = Stretch.Uniform }
            };
            scene.Children.Add(reflet);

            var flou = new BlurEffect { Radius = 14 };
            var conteneur = new Grid { Width = largeur, Height = hauteur, Opacity = 0, Effect = flou, IsHitTestVisible = false };
            conteneur.Children.Add(scene);
            Placer(conteneur, (W - largeur) / 2, H - MARGE - hauteur - 46);

            // accroche
            var accroche = new TextBlock
            {
                Text = Espace("SERVEUR HÉROÏQUE  ·  DOFUS 2.51  ·  UNE SEULE VIE"),
                FontFamily = new FontFamily("Segoe UI"),
                FontWeight = FontWeights.Bold,
                FontSize = 11.5,
                Foreground = new SolidColorBrush(Argent),
                Opacity = 0,
                IsHitTestVisible = false,
                Effect = new DropShadowEffect { Color = Colors.Black, BlurRadius = 10, ShadowDepth = 1, Opacity = 1 }
            };
            accroche.Loaded += (s, e) => Canvas.SetLeft(accroche, (W - accroche.ActualWidth) / 2);
            Placer(accroche, 0, H - MARGE - 50);

            var t0 = TimeSpan.FromSeconds(1.25);
            var duree = TimeSpan.FromSeconds(1.4);
            var ease = new PowerEase { Power = 3, EasingMode = EasingMode.EaseOut };
            conteneur.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, duree) { BeginTime = t0, EasingFunction = ease });
            echelle.BeginAnimation(ScaleTransform.ScaleXProperty, new DoubleAnimation(0.82, 1, duree) { BeginTime = t0, EasingFunction = ease });
            echelle.BeginAnimation(ScaleTransform.ScaleYProperty, new DoubleAnimation(0.82, 1, duree) { BeginTime = t0, EasingFunction = ease });
            var defloute = new DoubleAnimation(14, 0, duree) { BeginTime = t0, EasingFunction = ease };
            defloute.Completed += (s, e) => conteneur.Effect = null;
            flou.BeginAnimation(BlurEffect.RadiusProperty, defloute);
            accroche.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 0.92, TimeSpan.FromSeconds(1)) { BeginTime = TimeSpan.FromSeconds(2.1) });

            // respiration de la lueur (5 s) et balayage du reflet (toutes les 6 s)
            lueur.BeginAnimation(DropShadowEffect.BlurRadiusProperty, new DoubleAnimation(30, 48, TimeSpan.FromSeconds(2.5))
            {
                BeginTime = TimeSpan.FromSeconds(2.6), AutoReverse = true, RepeatBehavior = RepeatBehavior.Forever,
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            });
            lueur.BeginAnimation(DropShadowEffect.OpacityProperty, new DoubleAnimation(0.6, 0.95, TimeSpan.FromSeconds(2.5))
            {
                BeginTime = TimeSpan.FromSeconds(2.6), AutoReverse = true, RepeatBehavior = RepeatBehavior.Forever,
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            });
            var passe = new DoubleAnimationUsingKeyFrames { BeginTime = TimeSpan.FromSeconds(2.8), Duration = TimeSpan.FromSeconds(6), RepeatBehavior = RepeatBehavior.Forever };
            passe.KeyFrames.Add(new DiscreteDoubleKeyFrame(-1.3, KeyTime.FromTimeSpan(TimeSpan.Zero)));
            passe.KeyFrames.Add(new EasingDoubleKeyFrame(1.3, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(2.1)), new SineEase { EasingMode = EasingMode.EaseInOut }));
            passe.KeyFrames.Add(new DiscreteDoubleKeyFrame(1.3, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(6))));
            balayage.BeginAnimation(TranslateTransform.XProperty, passe);

            return conteneur;
        }

        FrameworkElement ConstruirePanneau()
        {
            var largeur = W - 2 * MARGE;
            var panneau = new Border
            {
                Width = largeur,
                Height = PANEL_H,
                CornerRadius = new CornerRadius(14),
                Background = new LinearGradientBrush(Color.FromArgb(0xc8, 0x14, 0x10, 0x1f), Color.FromArgb(0xd8, 0x0c, 0x09, 0x14), 90),
                BorderThickness = new Thickness(1),
                BorderBrush = new LinearGradientBrush(new GradientStopCollection
                {
                    new GradientStop(Argb(0x80, Ange), 0),
                    new GradientStop(Argb(0x50, Violet), 0.5),
                    new GradientStop(Argb(0x80, Demon), 1)
                }, new Point(0, 0.5), new Point(1, 0.5)),
                Effect = new DropShadowEffect { Color = Colors.Black, BlurRadius = 30, ShadowDepth = 6, Opacity = 0.7 },
                Opacity = 0,
                RenderTransform = new TranslateTransform(0, 24)
            };

            var grille = new Grid { Margin = new Thickness(22, 14, 16, 14) };
            grille.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grille.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(24) });
            grille.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            panneau.Child = grille;

            var gauche = new Grid();
            gauche.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            gauche.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            gauche.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grille.Children.Add(gauche);

            m_statut = new TextBlock
            {
                Text = "Préparation…",
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 13.5,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Argent),
                TextTrimming = TextTrimming.CharacterEllipsis
            };
            m_pourcent = new TextBlock
            {
                Text = "0 %",
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 13.5,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(VioletClair),
                HorizontalAlignment = HorizontalAlignment.Right
            };
            var ligne1 = new Grid();
            ligne1.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            ligne1.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            ligne1.Children.Add(m_statut);
            Grid.SetColumn(m_pourcent, 1);
            ligne1.Children.Add(m_pourcent);
            gauche.Children.Add(ligne1);

            // piste de progression
            m_largeurPiste = largeur - 22 - 16 - 24 - 210 - 2;
            var piste = new Border
            {
                Height = 8,
                CornerRadius = new CornerRadius(4),
                Background = new SolidColorBrush(Color.FromArgb(0xff, 0x1f, 0x18, 0x2e)),
                VerticalAlignment = VerticalAlignment.Center,
                ClipToBounds = true
            };
            var contenuPiste = new Grid { ClipToBounds = true };
            piste.Child = contenuPiste;
            m_barre = new Border
            {
                Width = 0,
                HorizontalAlignment = HorizontalAlignment.Left,
                CornerRadius = new CornerRadius(4),
                Background = new LinearGradientBrush(new GradientStopCollection
                {
                    new GradientStop(Ange, 0),
                    new GradientStop(Violet, 0.5),
                    new GradientStop(Demon, 1)
                }, new Point(0, 0.5), new Point(1, 0.5)) { MappingMode = BrushMappingMode.Absolute, StartPoint = new Point(0, 0), EndPoint = new Point(m_largeurPiste, 0) },
                Effect = new DropShadowEffect { Color = Violet, BlurRadius = 12, ShadowDepth = 0, Opacity = 0.9 },
                ClipToBounds = true
            };
            // reflet qui court dans la barre
            var course = new TranslateTransform(-120, 0);
            var brillance = new Rectangle
            {
                Width = 120,
                HorizontalAlignment = HorizontalAlignment.Left,
                Fill = new LinearGradientBrush(new GradientStopCollection
                {
                    new GradientStop(Colors.Transparent, 0),
                    new GradientStop(Color.FromArgb(0x90, 0xff, 0xff, 0xff), 0.5),
                    new GradientStop(Colors.Transparent, 1)
                }, new Point(0, 0.5), new Point(1, 0.5)),
                RenderTransform = course
            };
            course.BeginAnimation(TranslateTransform.XProperty, new DoubleAnimation(-120, m_largeurPiste, TimeSpan.FromSeconds(1.8)) { RepeatBehavior = RepeatBehavior.Forever });
            var barreContenu = new Grid();
            barreContenu.Children.Add(brillance);
            m_barre.Child = barreContenu;
            contenuPiste.Children.Add(m_barre);
            Grid.SetRow(piste, 1);
            gauche.Children.Add(piste);

            var ligne3 = new StackPanel { Orientation = Orientation.Horizontal };
            m_version = new TextBlock
            {
                Text = "Serveur Mystic",
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 11.5,
                Foreground = new SolidColorBrush(Brume)
            };
            m_reessayer = new TextBlock
            {
                Text = "Réessayer",
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 11.5,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(VioletClair),
                TextDecorations = TextDecorations.Underline,
                Cursor = Cursors.Hand,
                Margin = new Thickness(14, 0, 0, 0),
                Visibility = Visibility.Collapsed
            };
            m_reessayer.MouseLeftButtonUp += (s, e) => LancerMiseAJour(0);
            ligne3.Children.Add(m_version);
            ligne3.Children.Add(m_reessayer);
            Grid.SetRow(ligne3, 2);
            gauche.Children.Add(ligne3);

            var bouton = ConstruireBoutonJouer();
            Grid.SetColumn(bouton, 2);
            grille.Children.Add(bouton);

            Placer(panneau, MARGE, H - PANEL_H - MARGE);
            return panneau;
        }

        FrameworkElement ConstruireBoutonJouer()
        {
            m_jouerEchelle = new ScaleTransform(1, 1);
            m_jouerLueur = new DropShadowEffect { Color = Violet, BlurRadius = 18, ShadowDepth = 0, Opacity = 0 };
            m_jouerTexte = new TextBlock
            {
                Text = Espace("JOUER", " "),
                FontFamily = new FontFamily("Segoe UI"),
                FontWeight = FontWeights.Black,
                FontSize = 22,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Effect = new DropShadowEffect { Color = VioletSombre, BlurRadius = 8, ShadowDepth = 1, Opacity = 0.9 }
            };

            var balayage = new TranslateTransform(-1.4, 0);
            m_jouerReflet = new Rectangle
            {
                Fill = new LinearGradientBrush(new GradientStopCollection
                {
                    new GradientStop(Colors.Transparent, 0.38),
                    new GradientStop(Color.FromArgb(0x70, 0xff, 0xff, 0xff), 0.5),
                    new GradientStop(Colors.Transparent, 0.62)
                }, new Point(0, 0), new Point(1, 1)) { RelativeTransform = balayage },
                Opacity = 0,
                IsHitTestVisible = false
            };
            var passe = new DoubleAnimationUsingKeyFrames { Duration = TimeSpan.FromSeconds(3.2), RepeatBehavior = RepeatBehavior.Forever };
            passe.KeyFrames.Add(new DiscreteDoubleKeyFrame(-1.4, KeyTime.FromTimeSpan(TimeSpan.Zero)));
            passe.KeyFrames.Add(new EasingDoubleKeyFrame(1.4, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(1.1)), new SineEase { EasingMode = EasingMode.EaseInOut }));
            passe.KeyFrames.Add(new DiscreteDoubleKeyFrame(1.4, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(3.2))));
            balayage.BeginAnimation(TranslateTransform.XProperty, passe);

            var contenu = new Grid();
            contenu.Children.Add(m_jouerReflet);
            contenu.Children.Add(m_jouerTexte);

            m_jouer = new Border
            {
                Width = 210,
                Height = 60,
                CornerRadius = new CornerRadius(11),
                Background = new LinearGradientBrush(new GradientStopCollection
                {
                    new GradientStop(Rgb(0x9d, 0x74, 0xff), 0),
                    new GradientStop(Violet, 0.45),
                    new GradientStop(Rgb(0x5b, 0x21, 0xb6), 1)
                }, new Point(0, 0), new Point(0, 1)),
                BorderThickness = new Thickness(1),
                BorderBrush = new SolidColorBrush(Argb(0xc0, VioletClair)),
                Effect = m_jouerLueur,
                Child = contenu,
                ClipToBounds = true,
                Cursor = Cursors.Arrow,
                Opacity = 0.42,
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = m_jouerEchelle,
                VerticalAlignment = VerticalAlignment.Center
            };
            m_jouer.MouseEnter += (s, e) => { if (m_pret) AnimerBouton(1.045, 34, 1); };
            m_jouer.MouseLeave += (s, e) => { if (m_pret) AnimerBouton(1, 22, 0.75); };
            m_jouer.MouseLeftButtonDown += (s, e) => { if (m_pret) AnimerBouton(0.97, 40, 1); };
            m_jouer.MouseLeftButtonUp += (s, e) => { if (m_pret) Jouer(); };
            return m_jouer;
        }

        void AnimerBouton(double echelle, double flou, double opacite)
        {
            var d = TimeSpan.FromSeconds(0.22);
            var ease = new PowerEase { Power = 2, EasingMode = EasingMode.EaseOut };
            m_jouerEchelle.BeginAnimation(ScaleTransform.ScaleXProperty, new DoubleAnimation(echelle, d) { EasingFunction = ease });
            m_jouerEchelle.BeginAnimation(ScaleTransform.ScaleYProperty, new DoubleAnimation(echelle, d) { EasingFunction = ease });
            m_jouerLueur.BeginAnimation(DropShadowEffect.BlurRadiusProperty, new DoubleAnimation(flou, d) { EasingFunction = ease });
            m_jouerLueur.BeginAnimation(DropShadowEffect.OpacityProperty, new DoubleAnimation(opacite, d) { EasingFunction = ease });
        }

        FrameworkElement ConstruireBarreHaut()
        {
            var barre = new Grid { Width = W, Height = 40, Background = Brushes.Transparent, Opacity = 0 };
            barre.MouseLeftButtonDown += (s, e) => { if (e.ButtonState == MouseButtonState.Pressed) DragMove(); };

            var titre = new TextBlock
            {
                Text = Espace("MYSTIC  ·  LAUNCHER"),
                FontFamily = new FontFamily("Segoe UI"),
                FontWeight = FontWeights.Bold,
                FontSize = 11,
                Foreground = new SolidColorBrush(Argent2),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(20, 0, 0, 0),
                IsHitTestVisible = false
            };
            barre.Children.Add(titre);

            var boutons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 4, 8, 0) };
            boutons.Children.Add(BoutonFenetre("", Color.FromArgb(0x30, 0xff, 0xff, 0xff), () => WindowState = WindowState.Minimized));
            boutons.Children.Add(BoutonFenetre("", Color.FromArgb(0xd0, 0xe8, 0x11, 0x23), Fermer));
            barre.Children.Add(boutons);

            Placer(barre, 0, 0);
            return barre;
        }

        Border BoutonFenetre(string glyphe, Color survol, Action action)
        {
            var fond = new SolidColorBrush(Colors.Transparent);
            var b = new Border
            {
                Width = 42,
                Height = 30,
                CornerRadius = new CornerRadius(7),
                Background = fond,
                Margin = new Thickness(2, 0, 0, 0),
                Child = new TextBlock
                {
                    Text = glyphe,
                    FontFamily = new FontFamily("Segoe MDL2 Assets"),
                    FontSize = 10,
                    Foreground = new SolidColorBrush(Argent),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                }
            };
            b.MouseEnter += (s, e) => fond.BeginAnimation(SolidColorBrush.ColorProperty, new ColorAnimation(survol, TimeSpan.FromSeconds(0.15)));
            b.MouseLeave += (s, e) => fond.BeginAnimation(SolidColorBrush.ColorProperty, new ColorAnimation(Colors.Transparent, TimeSpan.FromSeconds(0.2)));
            b.MouseLeftButtonDown += (s, e) => e.Handled = true;
            b.MouseLeftButtonUp += (s, e) => action();
            return b;
        }

        // ---------------------------------------------------------------- intro et animation continue

        void Intro(FrameworkElement logo, FrameworkElement panneau, FrameworkElement barreHaut, Rectangle flash)
        {
            // flash à l'impact des deux moitiés (1,35 s) + secousse
            var f = new DoubleAnimationUsingKeyFrames { BeginTime = TimeSpan.FromSeconds(1.35), Duration = TimeSpan.FromSeconds(1.1) };
            f.KeyFrames.Add(new LinearDoubleKeyFrame(0, KeyTime.FromPercent(0)));
            f.KeyFrames.Add(new LinearDoubleKeyFrame(0.85, KeyTime.FromPercent(0.08)));
            f.KeyFrames.Add(new EasingDoubleKeyFrame(0, KeyTime.FromPercent(1), new PowerEase { Power = 2, EasingMode = EasingMode.EaseOut }));
            flash.BeginAnimation(OpacityProperty, f);

            Action<DependencyProperty, double[]> secoue = (prop, valeurs) =>
            {
                var a = new DoubleAnimationUsingKeyFrames { BeginTime = TimeSpan.FromSeconds(1.4), Duration = TimeSpan.FromSeconds(0.5) };
                for (var i = 0; i < valeurs.Length; i++)
                    a.KeyFrames.Add(new LinearDoubleKeyFrame(valeurs[i], KeyTime.FromPercent(i / (double)(valeurs.Length - 1))));
                m_secousse.BeginAnimation(prop, a);
            };
            secoue(TranslateTransform.XProperty, new double[] { 0, -7, 6, -4, 2, 0 });
            secoue(TranslateTransform.YProperty, new double[] { 0, 4, -4, 2, -1, 0 });

            var ease = new PowerEase { Power = 3, EasingMode = EasingMode.EaseOut };
            panneau.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromSeconds(1)) { BeginTime = TimeSpan.FromSeconds(2.3), EasingFunction = ease });
            panneau.RenderTransform.BeginAnimation(TranslateTransform.YProperty, new DoubleAnimation(24, 0, TimeSpan.FromSeconds(1)) { BeginTime = TimeSpan.FromSeconds(2.3), EasingFunction = ease });
            barreHaut.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.8)) { BeginTime = TimeSpan.FromSeconds(2.0) });
        }

        void Image_Suivante(object sender, EventArgs e)
        {
            var t = m_horloge.Elapsed.TotalSeconds;
            var dt = Math.Min(0.05, t - m_dernier);
            m_dernier = t;

            // parallaxe à la souris (lissée)
            var cx = m_souris.X / W - 0.5;
            var cy = m_souris.Y / H - 0.5;
            m_px += (-cx * 18 - m_px) * Math.Min(1, dt * 5);
            m_py += (-cy * 10 - m_py) * Math.Min(1, dt * 5);
            m_peintParallaxG.X = m_peintParallaxD.X = m_px;
            m_peintParallaxG.Y = m_peintParallaxD.Y = m_py;
            m_fondParallax.X = -m_px * 0.6;
            m_fondParallax.Y = -m_py * 0.6;
            m_logoParallax.X = m_px * 0.35;
            m_logoParallax.Y = m_py * 0.35;

            foreach (var p in m_particules)
            {
                p.Vie += dt;
                p.Y += p.Vy * dt;
                p.X += p.Vx * dt;
                if (p.Vie >= p.VieMax || p.Y < -20)
                {
                    Renaitre(p, false);
                    continue;
                }
                var x = p.X + Math.Sin(p.Phase + p.Vie * 1.3) * p.Amplitude;
                var a = Math.Sin(Math.PI * p.Vie / p.VieMax) * p.Alpha;
                if (p.Scintille)
                    a *= 0.6 + 0.4 * Math.Sin(p.Phase * 3 + t * 7);
                else
                    a *= 0.75 + 0.25 * Math.Sin(p.Phase * 5 + t * 13);
                Canvas.SetLeft(p.Forme, x - p.Taille / 2);
                Canvas.SetTop(p.Forme, p.Y - p.Taille / 2);
                p.Forme.Opacity = Math.Max(0, a);
            }
        }

        void MettreAJourFocus()
        {
            if (m_souris.Y > H - PANEL_H - MARGE - 10 || m_souris.Y < 40 || m_souris.X > W - MARGE - CARTE_L - 8)
            {
                DefinirFocus(0);
                return;
            }
            DefinirFocus(m_souris.X < W * 0.4 ? -1 : m_souris.X > W * 0.6 ? 1 : 0);
        }

        void DefinirFocus(int focus)
        {
            if (focus == m_focus)
                return;
            m_focus = focus;
            var d = TimeSpan.FromSeconds(0.55);
            var ease = new PowerEase { Power = 2, EasingMode = EasingMode.EaseOut };
            Action<UIElement, double> opa = (el, v) => el.BeginAnimation(OpacityProperty, new DoubleAnimation(v, d) { EasingFunction = ease });

            opa(m_voileG, focus == 1 ? 0.6 : 0);
            opa(m_voileD, focus == -1 ? 0.6 : 0);
            opa(m_eclatG, focus == -1 ? 0.07 : 0);
            opa(m_eclatD, focus == 1 ? 0.07 : 0);
            opa(m_campG, focus == -1 ? 1 : 0);
            opa(m_campD, focus == 1 ? 1 : 0);
            m_campGT.BeginAnimation(TranslateTransform.XProperty, new DoubleAnimation(focus == -1 ? 0 : -24, TimeSpan.FromSeconds(0.45)) { EasingFunction = ease });
            m_campDT.BeginAnimation(TranslateTransform.XProperty, new DoubleAnimation(focus == 1 ? 0 : 24, TimeSpan.FromSeconds(0.45)) { EasingFunction = ease });
        }

        // ---------------------------------------------------------------- mise à jour

        void Progression(string texte, int pourcent)
        {
            m_statut.Text = texte;
            pourcent = Math.Max(0, Math.Min(100, pourcent));
            m_pourcent.Text = pourcent + " %";
            m_barre.BeginAnimation(WidthProperty, new DoubleAnimation(m_largeurPiste * pourcent / 100.0, TimeSpan.FromSeconds(0.35))
            {
                EasingFunction = new PowerEase { Power = 2, EasingMode = EasingMode.EaseOut }
            });
        }

        void LancerMiseAJour(double delai)
        {
            m_pret = false;
            m_erreur = false;
            m_reessayer.Visibility = Visibility.Collapsed;
            EtatBouton();
            Progression("Préparation…", 0);

            new Thread(() =>
            {
                Thread.Sleep(TimeSpan.FromSeconds(delai));
                var ok = m_updater.Run((texte, p) => Dispatcher.BeginInvoke((Action)(() => Progression(texte, p))));
                Dispatcher.BeginInvoke((Action)(() => FinMiseAJour(ok)));
            }) { IsBackground = true }.Start();
        }

        void FinMiseAJour(bool ok)
        {
            if (m_updater.Version != null && m_updater.Version != "?")
                m_version.Text = "Serveur Mystic  ·  version " + m_updater.Version;
            if (ok)
            {
                Progression(m_statut.Text, 100);
                m_pret = true;
            }
            else
            {
                m_statut.Text = m_updater.LastError;
                m_statut.Foreground = new SolidColorBrush(Rgb(0xff, 0x9a, 0x86));
                m_erreur = true;
                m_reessayer.Visibility = Visibility.Visible;
                // le jeu reste lançable sans mise à jour (serveur hors ligne…)
                m_pret = m_updater.GameExists;
            }
            EtatBouton();
        }

        void EtatBouton()
        {
            if (m_pret)
            {
                m_statut.Foreground = m_erreur ? m_statut.Foreground : new SolidColorBrush(Argent);
                m_jouerTexte.Text = m_erreur ? "JOUER QUAND MÊME" : Espace("JOUER", " ");
                m_jouerTexte.FontSize = m_erreur ? 14 : 22;
                m_jouer.Cursor = Cursors.Hand;
                m_jouer.BeginAnimation(OpacityProperty, new DoubleAnimation(1, TimeSpan.FromSeconds(0.4)));
                m_jouerReflet.BeginAnimation(OpacityProperty, new DoubleAnimation(1, TimeSpan.FromSeconds(0.4)));
                // pulsation de la lueur tant qu'on n'a pas cliqué
                m_jouerLueur.BeginAnimation(DropShadowEffect.OpacityProperty, new DoubleAnimation(0.35, 0.9, TimeSpan.FromSeconds(1.3))
                {
                    AutoReverse = true, RepeatBehavior = RepeatBehavior.Forever, EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
                });
                m_jouerLueur.BeginAnimation(DropShadowEffect.BlurRadiusProperty, new DoubleAnimation(16, 30, TimeSpan.FromSeconds(1.3))
                {
                    AutoReverse = true, RepeatBehavior = RepeatBehavior.Forever, EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
                });
                // petit rebond d'apparition
                var rebond = new DoubleAnimationUsingKeyFrames { Duration = TimeSpan.FromSeconds(0.5) };
                rebond.KeyFrames.Add(new LinearDoubleKeyFrame(1, KeyTime.FromPercent(0)));
                rebond.KeyFrames.Add(new EasingDoubleKeyFrame(1.08, KeyTime.FromPercent(0.4), new PowerEase { EasingMode = EasingMode.EaseOut }));
                rebond.KeyFrames.Add(new EasingDoubleKeyFrame(1, KeyTime.FromPercent(1), new ElasticEase { Oscillations = 1, Springiness = 4, EasingMode = EasingMode.EaseOut }));
                m_jouerEchelle.BeginAnimation(ScaleTransform.ScaleXProperty, rebond);
                m_jouerEchelle.BeginAnimation(ScaleTransform.ScaleYProperty, rebond);
            }
            else
            {
                m_jouerTexte.Text = "MISE À JOUR…";
                m_jouerTexte.FontSize = 15;
                m_jouer.Cursor = Cursors.Arrow;
                m_jouer.BeginAnimation(OpacityProperty, new DoubleAnimation(0.42, TimeSpan.FromSeconds(0.3)));
                m_jouerReflet.BeginAnimation(OpacityProperty, new DoubleAnimation(0, TimeSpan.FromSeconds(0.3)));
                m_jouerLueur.BeginAnimation(DropShadowEffect.OpacityProperty, new DoubleAnimation(0, TimeSpan.FromSeconds(0.3)));
            }
        }

        void Jouer()
        {
            if (m_lance)
                return;
            m_lance = true;
        }

        void Fermer()
        {
            var fondu = new DoubleAnimation(0, TimeSpan.FromSeconds(0.35)) { EasingFunction = new PowerEase { EasingMode = EasingMode.EaseIn } };
            fondu.Completed += (s, e) => Close();
            BeginAnimation(OpacityProperty, fondu);
        }

        // ---------------------------------------------------------------- aperçu (captures)

        void LancerApercu()
        {
            Directory.CreateDirectory(m_apercu);
            var captures = new[] { 0.4, 0.9, 1.45, 2.0, 2.8, 3.6 };
            var index = 0;
            var debut = m_horloge.Elapsed.TotalSeconds;
            var rempli = false;
            var minuteur = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(15) };
            minuteur.Tick += (s, e) =>
            {
                var t = m_horloge.Elapsed.TotalSeconds - debut;
                if (t > 3.2 && !rempli) { rempli = true; ApercuConnexion(); }
                if (index < captures.Length && t >= captures[index])
                {
                    Capturer(m_scene, (int)W, (int)H, System.IO.Path.Combine(m_apercu, string.Format("connexion_{0:00}_{1:0.0}s.png", index + 1, captures[index])));
                    index++;
                }
                if (t > 4.0)
                {
                    minuteur.Stop();
                    ApercuOuvrir();
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

    class Particule
    {
        public Ellipse Forme;
        public double X, Y, Vx, Vy, Taille, Phase, Amplitude, Vie, VieMax, Alpha;
        public bool Scintille;
    }
}
