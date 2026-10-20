// MYSTIC 18/10/2026 : grand launcher Mystic, maquette « A · Sanctuaire » choisie par l'utilisateur :
// barre d'icônes à gauche (Comptes, Partages, Réglages, Déconnexion) ; en-tête (titre, serveur en ligne, compte web) ;
// onglets de groupes (Tous, groupes créés dans le launcher, + Groupe) ; tableau des comptes (case, compte, personnages, état,
// dernière partie, Jouer) avec lignes de groupe (Jouer le groupe) ; dock du bas (mise à jour, version, dossier, Vérifier les
// fichiers, Ouvrir le dossier, liens, JOUER · N). Partage de comptes par code à 6 chiffres (plusieurs comptes à la fois),
// retrait du partage par le propriétaire ou par le receveur (vue Partages).

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MysticLauncher
{
    /// <summary>Groupes de comptes et noms choisis (gardés sur le PC, par compte web) : %APPDATA%\Mystic\dossiers_&lt;empreinte&gt;.json</summary>
    class Dossiers
    {
        public class Dossier
        {
            public string Nom;
            public bool Replie;
            public List<string> Comptes = new List<string>();
        }

        public List<Dossier> Liste = new List<Dossier>();
        public bool SansDossierReplie;
        public Dictionary<string, string> Alias = new Dictionary<string, string>();
        string m_fichier;

        public static Dossiers Charger(string email)
        {
            var d = new Dossiers();
            try
            {
                string cle;
                using (var sha = SHA1.Create())
                    cle = BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes((email ?? "").ToLowerInvariant()))).Replace("-", "").Substring(0, 16).ToLowerInvariant();
                d.m_fichier = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Mystic", "dossiers_" + cle + ".json");
                if (File.Exists(d.m_fichier))
                {
                    var lu = new JavaScriptSerializer().Deserialize<Dossiers>(File.ReadAllText(d.m_fichier, Encoding.UTF8));
                    if (lu != null)
                    {
                        d.Liste = lu.Liste ?? new List<Dossier>();
                        d.SansDossierReplie = lu.SansDossierReplie;
                        d.Alias = lu.Alias ?? new Dictionary<string, string>();
                    }
                }
            }
            catch { }
            return d;
        }

        public void Sauver()
        {
            try
            {
                if (m_fichier == null) return;
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(m_fichier));
                File.WriteAllText(m_fichier, new JavaScriptSerializer().Serialize(new { Liste, SansDossierReplie, Alias }), Encoding.UTF8);
            }
            catch { }
        }

        public string Nom(CompteJeu c) => Alias.TryGetValue(c.Login.ToLowerInvariant(), out var n) && !string.IsNullOrEmpty(n) ? n : c.Login;

        public Dossier DossierDe(string login) => Liste.FirstOrDefault(x => x.Comptes.Contains(login, StringComparer.OrdinalIgnoreCase));

        public void Ranger(string login, Dossier cible)
        {
            foreach (var d in Liste)
                d.Comptes.RemoveAll(x => string.Equals(x, login, StringComparison.OrdinalIgnoreCase));
            cible?.Comptes.Add(login);
            Sauver();
        }
    }

    class PrincipalWindow : Window
    {
        const double W = 1120, H = 700, RAIL = 68, HAUT = 56, DOCK = 92;

        static readonly Color Fond = Color.FromRgb(0x0a, 0x09, 0x10);
        static readonly Color Nuit = Color.FromRgb(0x07, 0x06, 0x0b);
        static readonly Color Trait = Color.FromRgb(0x1f, 0x1a, 0x2b);
        static readonly Color Trait2 = Color.FromRgb(0x2c, 0x24, 0x40);
        static readonly Color Carte = Color.FromRgb(0x12, 0x0e, 0x1c);
        static readonly Color LigneGroupe = Color.FromRgb(0x0f, 0x0c, 0x17);
        static readonly Color Texte1 = Color.FromRgb(0xec, 0xe8, 0xf2);
        static readonly Color Texte2 = Color.FromRgb(0xb8, 0xb0, 0xc8);
        static readonly Color Brume = Color.FromRgb(0x8f, 0x86, 0xa3);
        static readonly Color Gris = Color.FromRgb(0x6f, 0x68, 0x80);
        static readonly Color Violet = Color.FromRgb(0x8b, 0x5c, 0xf6);
        static readonly Color VioletClair = Color.FromRgb(0xb7, 0x9c, 0xff);
        static readonly Color VioletPale = Color.FromRgb(0xd8, 0xcc, 0xff);
        static readonly Color Ange = Color.FromRgb(0xa9, 0xcb, 0xff);
        static readonly Color Demon = Color.FromRgb(0xff, 0x6a, 0x4a);
        static readonly Color Vert = Color.FromRgb(0x86, 0xef, 0xac);
        static readonly Color Jaune = Color.FromRgb(0xfc, 0xd3, 0x4d);
        static readonly Color Rouge = Color.FromRgb(0xf8, 0x71, 0x71);
        static readonly FontFamily Police = new FontFamily("Segoe UI");
        static readonly FontFamily Titre = new FontFamily("Segoe UI Semibold");
        static readonly FontFamily Icones = new FontFamily("Segoe Fluent Icons, Segoe MDL2 Assets");

        readonly Updater m_updater;
        readonly CompteApi m_api;
        ReponseWeb m_compte;
        readonly string m_session, m_apercu;
        readonly bool m_sessionGardee;
        readonly Dossiers m_dossiers;

        readonly Grid m_racine = new Grid();
        readonly Grid m_corps = new Grid();
        readonly HashSet<int> m_coches = new HashSet<int>();
        readonly Dictionary<int, Process> m_enJeu = new Dictionary<int, Process>();
        readonly Dictionary<int, DateTime> m_lancement = new Dictionary<int, DateTime>();
        Dossiers.Dossier m_onglet; // null = Tous
        bool m_vuePartages;
        readonly List<Border> m_boutonsRail = new List<Border>();

        TextBlock m_titre, m_statut, m_pourcent, m_infos, m_boutonTexte, m_statutServeur;
        Ellipse m_point;
        Border m_barre, m_bouton;
        Grid m_piste;
        Border m_calque; // fenêtre modale
        enum Etat { Verification, AInstaller, Travail, Pret, Erreur }
        Etat m_etat = Etat.Verification;
        bool m_fini;

        public PrincipalWindow(Updater updater, CompteApi api, ReponseWeb compte, string session, bool sessionGardee, string apercu)
        {
            m_updater = updater;
            m_api = api;
            m_compte = compte;
            m_session = session;
            m_sessionGardee = sessionGardee;
            m_apercu = apercu;
            m_dossiers = apercu != null ? Apercu.Dossiers() : Dossiers.Charger(compte.Email);
            // MYSTIC 20/10 : « Zaap » local → après une déconnexion, le client redemande un ticket pour SON compte (bouton « Se connecter avec »)
            if (apercu == null)
            {
                Zaap.Journal = t => m_updater.Log(t);
                Zaap.DemanderTicket = compteJeu =>
                {
                    var sess = m_session;
                    if (sess == null) return null;
                    var r = m_api.Ticket(sess, compteJeu);
                    return r != null && r.Ok ? Tuple.Create(r.Login, r.Ticket) : null;
                };
                Zaap.Demarrer();
            }

            Title = "Mystic";
            Width = W;
            Height = H;
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Background = Brushes.Transparent;
            ResizeMode = ResizeMode.CanMinimize;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            UseLayoutRounding = true;
            TextOptions.SetTextFormattingMode(this, TextFormattingMode.Display);

            var cadre = new Border
            {
                CornerRadius = new CornerRadius(12), Background = new SolidColorBrush(Fond), Child = m_racine, Opacity = 0,
                RenderTransformOrigin = new Point(0.5, 0.5), RenderTransform = new ScaleTransform(0.98, 0.98)
            };
            m_racine.Clip = new RectangleGeometry(new Rect(0, 0, W, H), 12, 12);
            Content = cadre;

            m_racine.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(RAIL) });
            m_racine.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            var principal = new Grid();
            principal.RowDefinitions.Add(new RowDefinition { Height = new GridLength(HAUT) });
            principal.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            principal.RowDefinitions.Add(new RowDefinition { Height = new GridLength(DOCK) });
            Grid.SetColumn(principal, 1);
            m_racine.Children.Add(principal);

            m_racine.Children.Add(ConstruireRail());
            principal.Children.Add(ConstruireHaut());
            Grid.SetRow(m_corps, 1);
            principal.Children.Add(m_corps);
            principal.Children.Add(ConstruireDock());

            m_calque = new Border { Background = new SolidColorBrush(Color.FromArgb(0xb8, 0x07, 0x06, 0x0b)), Visibility = Visibility.Collapsed };
            Grid.SetColumnSpan(m_calque, 2);
            m_racine.Children.Add(m_calque);

            Afficher();

            Loaded += (s, e) =>
            {
                var ease = new PowerEase { Power = 3, EasingMode = EasingMode.EaseOut };
                cadre.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.45)) { EasingFunction = ease });
                cadre.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, new DoubleAnimation(0.98, 1, TimeSpan.FromSeconds(0.5)) { EasingFunction = ease });
                cadre.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, new DoubleAnimation(0.98, 1, TimeSpan.FromSeconds(0.5)) { EasingFunction = ease });
                var suivi = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
                suivi.Tick += (o, a) => { if (!m_vuePartages && m_calque.Visibility != Visibility.Visible) Afficher(); };
                suivi.Start();
                // MYSTIC 20/10 : Dofus restés en mémoire sans fenêtre après fermeture → fermés (sinon impossible de relancer)
                if (m_apercu == null)
                {
                    var fantomes = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
                    var occupe = false;
                    fantomes.Tick += (o, a) =>
                    {
                        if (occupe) return;
                        occupe = true;
                        var appDir = m_updater.AppDir;
                        ThreadPool.QueueUserWorkItem(_ =>
                        {
                            Fantomes.Nettoyer(appDir, TimeSpan.FromSeconds(8), m_updater.Log);
                            occupe = false;
                        });
                    };
                    fantomes.Start();
                }
                if (m_apercu != null) LancerApercu();
                else { SuivreStatut(); Verifier(); }
            };
            Closed += (s, e) => RevoquerSessionTemporaire();
        }

        // ---------------------------------------------------------------- petits outils d'interface

        static Color A(byte a, Color c) => Color.FromArgb(a, c.R, c.G, c.B);

        static TextBlock T(string t, double taille, Color c, FontWeight poids, FontFamily f = null) => new TextBlock
        {
            Text = t, FontFamily = f ?? Police, FontSize = taille, FontWeight = poids, Foreground = new SolidColorBrush(c),
            TextTrimming = TextTrimming.CharacterEllipsis, VerticalAlignment = VerticalAlignment.Center
        };

        static TextBlock Ic(string glyphe, double taille, Color c) => new TextBlock
        {
            Text = glyphe, FontFamily = Icones, FontSize = taille, Foreground = new SolidColorBrush(c), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center
        };

        static string Espace(string s) => string.Join(" ", s.ToCharArray().Select(x => x.ToString()));

        static BitmapImage Ressource(string nom)
        {
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.StreamSource = Assembly.GetExecutingAssembly().GetManifestResourceStream(nom);
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.EndInit();
            bmp.Freeze();
            return bmp;
        }

        /// <summary>Bouton à contour (style de la maquette).</summary>
        Border Bouton(UIElement contenu, Action clic, Color? fond = null, Color? bord = null, double hauteur = 32, Color? survol = null)
        {
            var f = fond ?? Carte;
            var brosse = new SolidColorBrush(f);
            var b = new Border
            {
                Height = hauteur, Padding = new Thickness(12, 0, 12, 0), CornerRadius = new CornerRadius(8), Background = brosse,
                BorderBrush = new SolidColorBrush(bord ?? Trait2), BorderThickness = new Thickness(1), Child = contenu, Cursor = Cursors.Hand
            };
            b.MouseEnter += (s, e) => brosse.BeginAnimation(SolidColorBrush.ColorProperty, new ColorAnimation(survol ?? Color.FromRgb(0x1c, 0x16, 0x2b), TimeSpan.FromSeconds(0.12)));
            b.MouseLeave += (s, e) => brosse.BeginAnimation(SolidColorBrush.ColorProperty, new ColorAnimation(f, TimeSpan.FromSeconds(0.18)));
            b.PreviewMouseLeftButtonDown += (s, e) => e.Handled = true;
            b.PreviewMouseLeftButtonUp += (s, e) => { e.Handled = true; clic?.Invoke(); };
            return b;
        }

        Border BoutonTexte(string texte, Action clic, Color? fond = null, Color? bord = null, Color? couleur = null, double hauteur = 32) =>
            Bouton(T(texte, 12, couleur ?? Texte1, FontWeights.Medium), clic, fond, bord, hauteur);

        Border BoutonViolet(string texte, Action clic, double hauteur = 28) =>
            Bouton(T(texte, 11.5, Colors.White, FontWeights.SemiBold), clic, Violet, Violet, hauteur, Color.FromRgb(0x9d, 0x74, 0xff));

        Border Pastille(string texte, Color fond, Color couleur, bool point = false)
        {
            var p = new StackPanel { Orientation = Orientation.Horizontal };
            if (point) p.Children.Add(new Ellipse { Width = 6, Height = 6, Fill = new SolidColorBrush(couleur), Margin = new Thickness(0, 0, 5, 0), VerticalAlignment = VerticalAlignment.Center });
            p.Children.Add(T(texte, 11, couleur, FontWeights.SemiBold));
            return new Border { Child = p, Background = new SolidColorBrush(fond), CornerRadius = new CornerRadius(999), Padding = new Thickness(8, 2, 9, 3), HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center };
        }

        Border Case(bool cochee)
        {
            return new Border
            {
                Width = 15, Height = 15, CornerRadius = new CornerRadius(4), BorderThickness = new Thickness(1.5), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center,
                BorderBrush = new SolidColorBrush(cochee ? Violet : Color.FromRgb(0x4a, 0x40, 0x60)), Background = new SolidColorBrush(cochee ? Violet : Colors.Transparent),
                Child = cochee ? Ic("", 9, Colors.White) : null
            };
        }

        static ContextMenu Menu() => new ContextMenu
        {
            Background = new SolidColorBrush(Color.FromRgb(0x17, 0x12, 0x23)), BorderBrush = new SolidColorBrush(Trait2), Foreground = new SolidColorBrush(Texte1), Padding = new Thickness(4)
        };

        static MenuItem Element(string glyphe, string texte, Action clic)
        {
            var i = new MenuItem { Header = texte, FontFamily = Police, FontSize = 12.5, Foreground = new SolidColorBrush(Texte1), Icon = glyphe == null ? null : Ic(glyphe, 12, Brume) };
            if (clic != null) i.Click += (s, e) => clic();
            return i;
        }

        // ---------------------------------------------------------------- barre d'icônes

        FrameworkElement ConstruireRail()
        {
            var rail = new Grid { Background = new SolidColorBrush(Nuit) };
            rail.Children.Add(new Border { BorderBrush = new SolidColorBrush(Trait), BorderThickness = new Thickness(0, 0, 1, 0) });
            var haut = new StackPanel { Margin = new Thickness(0, 12, 0, 0), HorizontalAlignment = HorizontalAlignment.Center };
            haut.Children.Add(new Image { Source = Ressource("logo_mystic.png"), Width = 50, Margin = new Thickness(0, 0, 0, 16) });
            haut.Children.Add(BoutonRail("", "Comptes", () => { m_vuePartages = false; Afficher(); }, true));
            haut.Children.Add(BoutonRail("", "Partages", () => { m_vuePartages = true; Afficher(); }, false));
            rail.Children.Add(haut);
            var bas = new StackPanel { VerticalAlignment = VerticalAlignment.Bottom, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 0, 0, 12) };
            Border reglages = null;
            reglages = BoutonRail("", "Réglages", () =>
            {
                var m = Menu();
                m.Items.Add(Element("", "Créer un compte de jeu…", CreerCompteJeu));
                m.Items.Add(Element("", "Dossier du jeu…", ChangerDossier));
                m.Items.Add(Element("", "Vérifier les fichiers", () => Travailler(true)));
                m.Items.Add(Element("", "Mon compte sur le site", () => Outils.OuvrirSite(m_updater.SiteUrl + "compte.html")));
                m.PlacementTarget = reglages;
                m.Placement = PlacementMode.Right;
                m.IsOpen = true;
            }, false, false);
            bas.Children.Add(reglages);
            bas.Children.Add(BoutonRail("", "Se déconnecter", SeDeconnecter, false, false));
            rail.Children.Add(bas);
            Grid.SetColumn(rail, 0);
            rail.MouseLeftButtonDown += (s, e) => { if (e.ButtonState == MouseButtonState.Pressed) DragMove(); };
            return rail;
        }

        Border BoutonRail(string glyphe, string infobulle, Action clic, bool actif, bool navigation = true)
        {
            var fond = new SolidColorBrush(actif ? A(0x40, Violet) : Colors.Transparent);
            var icone = Ic(glyphe, 18, actif ? Colors.White : Brume);
            var b = new Border { Width = 44, Height = 44, CornerRadius = new CornerRadius(12), Background = fond, Child = icone, Margin = new Thickness(0, 0, 0, 8), Cursor = Cursors.Hand, ToolTip = infobulle };
            b.Tag = navigation;
            b.MouseEnter += (s, e) => { if (fond.Color.A < 0x40) fond.Color = A(0x20, Violet); };
            b.MouseLeave += (s, e) => { if (fond.Color.A < 0x40) fond.Color = Colors.Transparent; };
            b.MouseLeftButtonDown += (s, e) => e.Handled = true;
            b.MouseLeftButtonUp += (s, e) => clic();
            if (navigation) m_boutonsRail.Add(b);
            return b;
        }

        // ---------------------------------------------------------------- en-tête

        FrameworkElement ConstruireHaut()
        {
            var haut = new Grid { Background = new SolidColorBrush(Fond) };
            haut.Children.Add(new Border { BorderBrush = new SolidColorBrush(Trait), BorderThickness = new Thickness(0, 0, 0, 1) });
            haut.MouseLeftButtonDown += (s, e) => { if (e.ButtonState == MouseButtonState.Pressed) DragMove(); };
            var gauche = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(22, 0, 0, 0) };
            m_titre = T("Mes comptes", 18, Texte1, FontWeights.SemiBold, Titre);
            gauche.Children.Add(m_titre);
            m_point = new Ellipse { Width = 7, Height = 7, Fill = new SolidColorBrush(Gris), Margin = new Thickness(0, 0, 7, 0), VerticalAlignment = VerticalAlignment.Center };
            m_statutServeur = T("Serveur…", 11.5, Brume, FontWeights.SemiBold);
            var pill = new StackPanel { Orientation = Orientation.Horizontal };
            pill.Children.Add(m_point);
            pill.Children.Add(m_statutServeur);
            gauche.Children.Add(new Border { Child = pill, Margin = new Thickness(14, 0, 0, 0), Padding = new Thickness(11, 5, 12, 5), CornerRadius = new CornerRadius(999), BorderThickness = new Thickness(1), BorderBrush = new SolidColorBrush(A(0x50, Color.FromRgb(0x4a, 0xde, 0x80))), Background = new SolidColorBrush(A(0x1a, Color.FromRgb(0x4a, 0xde, 0x80))), VerticalAlignment = VerticalAlignment.Center });
            haut.Children.Add(gauche);

            var droite = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 0, 8, 0) };
            var textes = new StackPanel { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 10, 0) };
            var p = T(m_compte.Pseudo ?? "", 13, Texte1, FontWeights.SemiBold);
            p.HorizontalAlignment = HorizontalAlignment.Right;
            var em = T(m_compte.Email ?? "", 11, Brume, FontWeights.Normal);
            em.HorizontalAlignment = HorizontalAlignment.Right;
            textes.Children.Add(p);
            textes.Children.Add(em);
            droite.Children.Add(textes);
            droite.Children.Add(new Border
            {
                Width = 30, Height = 30, CornerRadius = new CornerRadius(15), VerticalAlignment = VerticalAlignment.Center,
                Background = new LinearGradientBrush(new GradientStopCollection { new GradientStop(Ange, 0), new GradientStop(Violet, 0.5), new GradientStop(Demon, 1) }, 45),
                Child = T(string.IsNullOrEmpty(m_compte.Pseudo) ? "?" : m_compte.Pseudo.Substring(0, 1).ToUpperInvariant(), 13, Colors.White, FontWeights.Bold)
            });
            ((TextBlock)((Border)droite.Children[droite.Children.Count - 1]).Child).HorizontalAlignment = HorizontalAlignment.Center;
            droite.Children.Add(BoutonFenetre("", Color.FromArgb(0x30, 0xff, 0xff, 0xff), () => WindowState = WindowState.Minimized));
            droite.Children.Add(BoutonFenetre("", Color.FromArgb(0xd0, 0xe8, 0x11, 0x23), Fermer));
            haut.Children.Add(droite);
            return haut;
        }

        Border BoutonFenetre(string glyphe, Color survol, Action action)
        {
            var fond = new SolidColorBrush(Colors.Transparent);
            var b = new Border { Width = 38, Height = 30, CornerRadius = new CornerRadius(6), Background = fond, Margin = new Thickness(6, 0, 0, 0), Child = Ic(glyphe, 10, Texte2), VerticalAlignment = VerticalAlignment.Center };
            b.MouseEnter += (s, e) => fond.BeginAnimation(SolidColorBrush.ColorProperty, new ColorAnimation(survol, TimeSpan.FromSeconds(0.12)));
            b.MouseLeave += (s, e) => fond.BeginAnimation(SolidColorBrush.ColorProperty, new ColorAnimation(Colors.Transparent, TimeSpan.FromSeconds(0.18)));
            b.MouseLeftButtonDown += (s, e) => e.Handled = true;
            b.MouseLeftButtonUp += (s, e) => action();
            return b;
        }

        // ---------------------------------------------------------------- vue Comptes

        void Afficher()
        {
            m_corps.Children.Clear();
            if (m_titre != null) m_titre.Text = m_vuePartages ? "Partages" : "Mes comptes";
            for (var i = 0; i < m_boutonsRail.Count; i++)
            {
                var on = (i == 1) == m_vuePartages; // 0 = Comptes, 1 = Partages
                ((SolidColorBrush)m_boutonsRail[i].Background).Color = on ? A(0x40, Violet) : Colors.Transparent;
                ((TextBlock)m_boutonsRail[i].Child).Foreground = new SolidColorBrush(on ? Colors.White : Brume);
            }
            m_corps.Children.Add(m_vuePartages ? VuePartages() : VueComptes());
            MajBouton();
        }

        IEnumerable<CompteJeu> ComptesDe(Dossiers.Dossier d) =>
            d == null ? m_compte.Comptes.Where(c => m_dossiers.DossierDe(c.Login) == null)
                      : m_compte.Comptes.Where(c => d.Comptes.Contains(c.Login, StringComparer.OrdinalIgnoreCase));

        FrameworkElement VueComptes()
        {
            if (m_onglet != null && !m_dossiers.Liste.Contains(m_onglet)) m_onglet = null;
            var vue = new Grid { Margin = new Thickness(22, 16, 22, 8) };
            vue.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            vue.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            vue.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            // onglets + actions
            var barre = new Grid { Margin = new Thickness(0, 0, 0, 10) };
            barre.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            barre.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            var onglets = new StackPanel { Orientation = Orientation.Horizontal };
            onglets.Children.Add(Onglet("Tous · " + m_compte.Comptes.Count, null, m_onglet == null));
            foreach (var d in m_dossiers.Liste)
                onglets.Children.Add(Onglet(d.Nom + " · " + ComptesDe(d).Count(), d, m_onglet == d));
            var plus = T("+ Groupe", 12.5, Texte2, FontWeights.Normal);
            plus.Margin = new Thickness(12, 0, 0, 0);
            plus.Cursor = Cursors.Hand;
            plus.MouseLeftButtonUp += (s, e) => NouveauGroupe(null);
            onglets.Children.Add(plus);
            barre.Children.Add(onglets);

            var actions = new StackPanel { Orientation = Orientation.Horizontal };
            var mesCoches = m_compte.Comptes.Where(c => m_coches.Contains(c.Id) && c.PartagePar == null).ToList();
            if (mesCoches.Count > 0)
            {
                var partager = BoutonTexte("Partager (" + mesCoches.Count + ")", () => Partager(mesCoches), A(0x30, Violet), A(0x90, VioletClair), VioletPale);
                partager.Margin = new Thickness(0, 0, 8, 0);
                actions.Children.Add(partager);
            }
            var ajouter = BoutonTexte("Compte partagé", AjouterComptePartage);
            ajouter.Margin = new Thickness(0, 0, 8, 0);
            actions.Children.Add(ajouter);
            var visibles = (m_onglet == null ? m_compte.Comptes : ComptesDe(m_onglet)).ToList();
            var tous = visibles.Count > 0 && visibles.All(c => m_coches.Contains(c.Id));
            var toutSel = BoutonTexte(tous ? "Tout désélectionner" : "Tout sélectionner", () =>
            {
                if (tous) visibles.ForEach(c => m_coches.Remove(c.Id)); else visibles.ForEach(c => m_coches.Add(c.Id));
                Afficher();
            });
            toutSel.Margin = new Thickness(0, 0, 8, 0);
            actions.Children.Add(toutSel);
            actions.Children.Add(Bouton(Ic("", 12, Texte2), Actualiser));
            Grid.SetColumn(actions, 1);
            barre.Children.Add(actions);
            vue.Children.Add(barre);

            // en-têtes de colonnes
            var entetes = LigneColonnes();
            string[] noms = { "", "COMPTE", "PERSONNAGES", "ÉTAT", "DERNIÈRE PARTIE", "" };
            for (var i = 0; i < noms.Length; i++)
            {
                var t = T(Espace(noms[i]), 10, Gris, FontWeights.SemiBold);
                t.Margin = new Thickness(i == 0 ? 0 : 0, 0, 0, 0);
                Grid.SetColumn(t, i);
                entetes.Children.Add(t);
            }
            var ent = new Border { Child = entetes, Height = 30, BorderBrush = new SolidColorBrush(Trait), BorderThickness = new Thickness(0, 0, 0, 1) };
            Grid.SetRow(ent, 1);
            vue.Children.Add(ent);

            // lignes
            var liste = new StackPanel();
            if (m_compte.Comptes.Count == 0)
            {
                var vide = T("Aucun compte de jeu : crée-en un sur le site (Réglages › Mon compte sur le site) ou ajoute un compte partagé.", 13, Brume, FontWeights.Normal);
                vide.TextWrapping = TextWrapping.Wrap;
                vide.Margin = new Thickness(4, 16, 0, 0);
                liste.Children.Add(vide);
            }
            else if (m_onglet != null)
            {
                liste.Children.Add(LigneGroupeUi(m_onglet));
                foreach (var c in ComptesDe(m_onglet)) liste.Children.Add(LigneCompte(c));
            }
            else
            {
                foreach (var d in m_dossiers.Liste)
                {
                    liste.Children.Add(LigneGroupeUi(d));
                    if (!d.Replie)
                        foreach (var c in ComptesDe(d)) liste.Children.Add(LigneCompte(c));
                }
                var libres = ComptesDe(null).ToList();
                liste.Children.Add(LigneGroupeUi(null));
                foreach (var c in libres) liste.Children.Add(LigneCompte(c));
            }
            var defile = new ScrollViewer { Content = liste, VerticalScrollBarVisibility = ScrollBarVisibility.Hidden };
            Grid.SetRow(defile, 2);
            vue.Children.Add(defile);
            return vue;
        }

        Grid LigneColonnes()
        {
            var g = new Grid();
            foreach (var l in new double[] { 34, 250, 240, 130, 130 })
                g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(l) });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            return g;
        }

        FrameworkElement Onglet(string texte, Dossiers.Dossier d, bool actif)
        {
            var fond = new SolidColorBrush(actif ? Color.FromRgb(0x17, 0x12, 0x23) : Colors.Transparent);
            var b = new Border
            {
                Child = T(texte, 12.5, actif ? Colors.White : Texte2, FontWeights.Normal), Padding = new Thickness(13, 6, 13, 7), CornerRadius = new CornerRadius(9),
                Background = fond, BorderThickness = new Thickness(1), BorderBrush = new SolidColorBrush(actif ? Trait2 : Colors.Transparent), Cursor = Cursors.Hand, AllowDrop = true
            };
            b.MouseLeftButtonUp += (s, e) => { m_onglet = d; Afficher(); };
            b.DragOver += (s, e) => { e.Effects = e.Data.GetData(typeof(CompteJeu)) is CompteJeu ? DragDropEffects.Move : DragDropEffects.None; fond.Color = A(0x40, Violet); e.Handled = true; };
            b.DragLeave += (s, e) => fond.Color = actif ? Color.FromRgb(0x17, 0x12, 0x23) : Colors.Transparent;
            b.Drop += (s, e) => { if (e.Data.GetData(typeof(CompteJeu)) is CompteJeu c) { m_dossiers.Ranger(c.Login, d); Dispatcher.BeginInvoke((Action)Afficher); } };
            if (d != null)
            {
                b.ContextMenu = new ContextMenu();
                b.ContextMenuOpening += (s, e) => { e.Handled = true; MenuGroupe(d, b); };
            }
            return b;
        }

        void MenuGroupe(Dossiers.Dossier d, FrameworkElement cible)
        {
            var m = Menu();
            m.Items.Add(Element("", "Jouer le groupe", () => JouerListe(ComptesDe(d).ToList())));
            m.Items.Add(Element("", "Renommer le groupe…", () => { var n = Saisir("Renommer le groupe", d.Nom); if (n != null) { d.Nom = n; m_dossiers.Sauver(); Afficher(); } }));
            m.Items.Add(Element("", "Supprimer le groupe", () => { m_dossiers.Liste.Remove(d); m_dossiers.Sauver(); Afficher(); }));
            m.PlacementTarget = cible;
            m.Placement = PlacementMode.Bottom;
            m.IsOpen = true;
        }

        FrameworkElement LigneGroupeUi(Dossiers.Dossier d)
        {
            var comptes = ComptesDe(d).ToList();
            var fond = new SolidColorBrush(LigneGroupe);
            var g = LigneColonnes();
            var b = new Border { Child = g, Height = 36, Background = fond, BorderBrush = new SolidColorBrush(Trait), BorderThickness = new Thickness(0, 0, 0, 1), AllowDrop = true, Cursor = Cursors.Hand };
            var texte = d != null ? d.Nom.ToUpperInvariant() + " · " + comptes.Count + " COMPTE" + (comptes.Count > 1 ? "S" : "") : "SANS GROUPE · GLISSE UN COMPTE ICI";
            var t = T(Espace(texte), 11, VioletClair, FontWeights.SemiBold);
            t.Margin = new Thickness(10, 0, 0, 0);
            Grid.SetColumnSpan(t, 4);
            g.Children.Add(t);
            if (d != null)
            {
                var actions = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 0, 4, 0) };
                var editer = Bouton(Ic("", 11, Brume), () => { var n = Saisir("Renommer le groupe", d.Nom); if (n != null) { d.Nom = n; m_dossiers.Sauver(); Afficher(); } }, Colors.Transparent, Colors.Transparent, 26);
                var suppr = Bouton(Ic("", 11, Brume), () => { m_dossiers.Liste.Remove(d); m_dossiers.Sauver(); Afficher(); }, Colors.Transparent, Colors.Transparent, 26);
                editer.Opacity = suppr.Opacity = 0;
                actions.Children.Add(editer);
                actions.Children.Add(suppr);
                if (comptes.Count > 0)
                {
                    var jouer = BoutonViolet("▶  Jouer le groupe", () => JouerListe(comptes), 26);
                    jouer.Margin = new Thickness(6, 0, 0, 0);
                    actions.Children.Add(jouer);
                }
                Grid.SetColumn(actions, 4);
                Grid.SetColumnSpan(actions, 2);
                g.Children.Add(actions);
                b.MouseEnter += (s, e) => { editer.Opacity = suppr.Opacity = 1; };
                b.MouseLeave += (s, e) => { editer.Opacity = suppr.Opacity = 0; };
                b.MouseLeftButtonUp += (s, e) => { d.Replie = !d.Replie; m_dossiers.Sauver(); Afficher(); };
            }
            b.DragOver += (s, e) => { e.Effects = e.Data.GetData(typeof(CompteJeu)) is CompteJeu ? DragDropEffects.Move : DragDropEffects.None; fond.Color = A(0x40, Violet); e.Handled = true; };
            b.DragLeave += (s, e) => fond.Color = LigneGroupe;
            b.Drop += (s, e) => { if (e.Data.GetData(typeof(CompteJeu)) is CompteJeu c) { m_dossiers.Ranger(c.Login, d); Dispatcher.BeginInvoke((Action)Afficher); } };
            return b;
        }

        bool EnJeu(CompteJeu c) => m_enJeu.TryGetValue(c.Id, out var p) && !p.HasExited;
        bool EnConnexion(CompteJeu c) => m_lancement.TryGetValue(c.Id, out var t) && (DateTime.Now - t).TotalSeconds < 12;

        FrameworkElement LigneCompte(CompteJeu c)
        {
            var coche = m_coches.Contains(c.Id);
            var normal = coche ? A(0x16, Violet) : Colors.Transparent;
            var fond = new SolidColorBrush(normal);
            var g = LigneColonnes();
            var ligne = new Border { Child = g, Height = 46, Background = fond, BorderBrush = new SolidColorBrush(Color.FromRgb(0x16, 0x12, 0x1f)), BorderThickness = new Thickness(0, 0, 0, 1), Cursor = Cursors.Hand };

            g.Children.Add(Case(coche));

            // compte : nom choisi, crayon (renommer), badge partagé
            var nom = new StackPanel { Orientation = Orientation.Horizontal };
            nom.Children.Add(T(m_dossiers.Nom(c), 13, Texte1, FontWeights.SemiBold));
            var crayon = Ic("", 10, Brume);
            crayon.Margin = new Thickness(7, 1, 0, 0);
            crayon.Cursor = Cursors.Hand;
            crayon.ToolTip = "Renommer";
            crayon.MouseLeftButtonUp += (s, e) => { e.Handled = true; RenommerCompte(c); };
            nom.Children.Add(crayon);
            if (c.PartagePar != null)
            {
                var badge = Pastille("Partagé par " + c.PartagePar, A(0x1f, Ange), Ange);
                badge.Margin = new Thickness(8, 0, 0, 0);
                nom.Children.Add(badge);
            }
            Grid.SetColumn(nom, 1);
            g.Children.Add(nom);

            // personnages : bustes officiels (3 au plus) + le dernier joué
            var persos = c.Personnages.OrderBy(p => p.Depuis < 0 ? long.MaxValue : p.Depuis).ToList();
            var colPersos = new StackPanel { Orientation = Orientation.Horizontal };
            var i = 0;
            foreach (var p in persos.Take(3))
            {
                var rond = new Border { Width = 26, Height = 26, CornerRadius = new CornerRadius(13), Background = new SolidColorBrush(Color.FromRgb(0x1b, 0x15, 0x29)), BorderBrush = new SolidColorBrush(Fond), BorderThickness = new Thickness(2), Margin = new Thickness(i++ == 0 ? 0 : -8, 0, 0, 0), ClipToBounds = true };
                var img = new Image { Stretch = Stretch.UniformToFill, Source = Outils.TeteDeClasse(m_updater.AppDir, p.Classe, p.Sexe) };
                rond.Child = img;
                Outils.ChargerRendu(Dispatcher, p.Apparence, x => img.Source = x, "face/2/100_100-10");
                colPersos.Children.Add(rond);
            }
            var resume = persos.Count == 0 ? "Aucun personnage" : persos[0].Nom + " " + persos[0].Niveau + (persos.Count > 1 ? "  ·  +" + (persos.Count - 1) : "");
            var tr = T(resume, 11.5, Brume, FontWeights.Normal);
            tr.Margin = new Thickness(persos.Count > 0 ? 8 : 0, 0, 0, 0);
            tr.ToolTip = string.Join("\n", persos.Select(p => p.Nom + "  Lv." + p.Niveau + "  ·  " + Outils.Classe(p.Classe)));
            colPersos.Children.Add(tr);
            Grid.SetColumn(colPersos, 2);
            g.Children.Add(colPersos);

            // état
            Border etat;
            if (EnJeu(c) && !EnConnexion(c)) etat = Pastille("En jeu", A(0x1f, Color.FromRgb(0x4a, 0xde, 0x80)), Vert, true);
            else if (EnConnexion(c)) etat = Pastille("Connexion…", A(0x1f, Color.FromRgb(0xfa, 0xcc, 0x15)), Jaune, true);
            else etat = Pastille("Hors ligne", Color.FromRgb(0x1b, 0x15, 0x29), Brume);
            Grid.SetColumn(etat, 3);
            g.Children.Add(etat);

            var derniere = T(EnJeu(c) ? "maintenant" : persos.Count > 0 ? Outils.IlYA(persos[0].Depuis) : "jamais", 11.5, Brume, FontWeights.Normal);
            Grid.SetColumn(derniere, 4);
            g.Children.Add(derniere);

            // actions : Jouer / En jeu, menu …
            var actions = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Center };
            var plus = Bouton(Ic("", 12, Brume), null, Colors.Transparent, Colors.Transparent, 28);
            plus.Padding = new Thickness(8, 0, 8, 0);
            plus.PreviewMouseLeftButtonUp += (s, e) => MenuCompte(c, plus);
            actions.Children.Add(plus);
            var jouer = EnJeu(c)
                ? Bouton(T("En jeu", 11.5, Texte1, FontWeights.SemiBold), null, Colors.Transparent, Color.FromRgb(0x4a, 0x3a, 0x72), 28)
                : Bouton(T("▶  Jouer", 11.5, VioletPale, FontWeights.SemiBold), () => Jouer(c), Colors.Transparent, Color.FromRgb(0x4a, 0x3a, 0x72), 28);
            jouer.Margin = new Thickness(4, 0, 0, 0);
            actions.Children.Add(jouer);
            Grid.SetColumn(actions, 5);
            g.Children.Add(actions);

            ligne.MouseEnter += (s, e) => fond.Color = coche ? A(0x24, Violet) : Color.FromRgb(0x13, 0x10, 0x1c);
            ligne.MouseLeave += (s, e) => fond.Color = normal;

            // clic = cocher / décocher, double-clic = jouer, glisser = ranger dans un groupe
            Point? depart = null;
            ligne.PreviewMouseLeftButtonDown += (s, e) => { depart = e.GetPosition(this); if (e.ClickCount == 2) { Jouer(c); e.Handled = true; } };
            ligne.MouseLeftButtonUp += (s, e) =>
            {
                if (depart == null) return;
                depart = null;
                if (!m_coches.Remove(c.Id)) m_coches.Add(c.Id);
                Afficher();
            };
            ligne.MouseMove += (s, e) =>
            {
                if (depart == null || e.LeftButton != MouseButtonState.Pressed) return;
                var p = e.GetPosition(this);
                if (Math.Abs(p.X - depart.Value.X) + Math.Abs(p.Y - depart.Value.Y) < 6) return;
                depart = null;
                ligne.Opacity = 0.45;
                DragDrop.DoDragDrop(ligne, new DataObject(typeof(CompteJeu), c), DragDropEffects.Move);
                ligne.Opacity = 1;
            };
            ligne.ContextMenu = new ContextMenu();
            ligne.ContextMenuOpening += (s, e) => { e.Handled = true; MenuCompte(c, ligne); };
            return ligne;
        }

        void MenuCompte(CompteJeu c, FrameworkElement cible)
        {
            var m = Menu();
            m.Items.Add(Element("", "Jouer", () => Jouer(c)));
            m.Items.Add(Element("", "Renommer…", () => RenommerCompte(c)));
            var deplacer = Element("", "Déplacer vers", null);
            foreach (var d in m_dossiers.Liste)
            {
                var cible2 = d;
                deplacer.Items.Add(Element("", d.Nom, () => { m_dossiers.Ranger(c.Login, cible2); Afficher(); }));
            }
            if (m_dossiers.Liste.Count > 0) deplacer.Items.Add(new Separator());
            deplacer.Items.Add(Element("", "Nouveau groupe…", () => NouveauGroupe(c)));
            m.Items.Add(deplacer);
            if (m_dossiers.DossierDe(c.Login) != null)
                m.Items.Add(Element("", "Sortir du groupe", () => { m_dossiers.Ranger(c.Login, null); Afficher(); }));
            m.Items.Add(new Separator());
            if (c.PartagePar == null)
                m.Items.Add(Element("", "Partager ce compte…", () => Partager(new List<CompteJeu> { c })));
            else
                m.Items.Add(Element("", "Retirer ce compte partagé", () => RetirerPartageRecu(c.Id)));
            m.PlacementTarget = cible;
            m.Placement = PlacementMode.Bottom;
            m.IsOpen = true;
        }

        void RenommerCompte(CompteJeu c)
        {
            var nom = Saisir("Renommer « " + c.Login + " »", m_dossiers.Nom(c) == c.Login ? "" : m_dossiers.Nom(c), true);
            if (nom == null) return;
            if (nom.Length == 0) m_dossiers.Alias.Remove(c.Login.ToLowerInvariant());
            else m_dossiers.Alias[c.Login.ToLowerInvariant()] = nom;
            m_dossiers.Sauver();
            Afficher();
        }

        void NouveauGroupe(CompteJeu ranger)
        {
            var nom = Saisir("Nouveau groupe", "");
            if (nom == null) return;
            var d = m_dossiers.Liste.FirstOrDefault(x => string.Equals(x.Nom, nom, StringComparison.OrdinalIgnoreCase));
            if (d == null) { d = new Dossiers.Dossier { Nom = nom }; m_dossiers.Liste.Add(d); }
            // les comptes cochés (ou celui du menu) vont dans le nouveau groupe
            var aRanger = ranger != null ? new List<CompteJeu> { ranger } : m_compte.Comptes.Where(c => m_coches.Contains(c.Id)).ToList();
            foreach (var c in aRanger) m_dossiers.Ranger(c.Login, d);
            m_dossiers.Sauver();
            Afficher();
        }

        void Actualiser()
        {
            if (m_session == null) return;
            new Thread(() =>
            {
                var r = m_api.Session(m_session);
                Dispatcher.BeginInvoke((Action)(() => { if (r.Ok) { r.Session = m_session; m_compte = r; } Afficher(); }));
            }) { IsBackground = true }.Start();
        }

        // ---------------------------------------------------------------- vue Partages

        FrameworkElement VuePartages()
        {
            var vue = new StackPanel { Margin = new Thickness(22, 18, 22, 8) };
            var intro = T("Les comptes que tu partages peuvent être joués par l'autre joueur depuis son launcher (rien d'autre : ni mot de passe, ni e-mail, ni tes autres comptes). Tu peux retirer un accès à tout moment, et celui qui a reçu un compte peut aussi le retirer de son côté.", 12.5, Brume, FontWeights.Normal);
            intro.TextWrapping = TextWrapping.Wrap;
            intro.Margin = new Thickness(0, 0, 0, 14);
            vue.Children.Add(intro);
            var boutons = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 16) };
            var b1 = BoutonTexte("Partager des comptes…", () => { m_vuePartages = false; Afficher(); MessageInfo("Coche les comptes à partager dans « Mes comptes », puis clique sur « Partager ».", false); });
            b1.Margin = new Thickness(0, 0, 8, 0);
            boutons.Children.Add(b1);
            boutons.Children.Add(BoutonTexte("Ajouter un compte partagé", AjouterComptePartage));
            vue.Children.Add(boutons);

            var donnes = new StackPanel();
            var recus = new StackPanel();
            vue.Children.Add(TitreSection("COMPTES QUE JE PARTAGE"));
            vue.Children.Add(donnes);
            var t2 = TitreSection("COMPTES PARTAGÉS AVEC MOI");
            t2.Margin = new Thickness(0, 18, 0, 0);
            vue.Children.Add(t2);
            vue.Children.Add(recus);
            donnes.Children.Add(T("Chargement…", 12, Brume, FontWeights.Normal));

            if (m_apercu != null)
            {
                RemplirPartages(donnes, recus, Apercu.Partages());
                return new ScrollViewer { Content = vue, VerticalScrollBarVisibility = ScrollBarVisibility.Hidden };
            }
            var session = m_session;
            new Thread(() =>
            {
                var l = session != null ? m_api.PartageListe(session) : null;
                Dispatcher.BeginInvoke((Action)(() => RemplirPartages(donnes, recus, l)));
            }) { IsBackground = true }.Start();
            return new ScrollViewer { Content = vue, VerticalScrollBarVisibility = ScrollBarVisibility.Hidden };
        }

        TextBlock TitreSection(string t)
        {
            var x = T(Espace(t), 10.5, Gris, FontWeights.SemiBold);
            x.Margin = new Thickness(0, 0, 0, 6);
            return x;
        }

        void RemplirPartages(StackPanel donnes, StackPanel recus, Tuple<List<Partage>, List<Partage>> l)
        {
            donnes.Children.Clear();
            recus.Children.Clear();
            if (l == null)
            {
                donnes.Children.Add(T("Impossible de charger les partages (serveur injoignable).", 12, Rouge, FontWeights.Normal));
                return;
            }
            if (l.Item1.Count == 0) donnes.Children.Add(T("Tu ne partages aucun compte.", 12, Brume, FontWeights.Normal));
            foreach (var p in l.Item1)
            {
                var x = p;
                donnes.Children.Add(LignePartage(x.Login, "partagé avec " + x.Pseudo, "depuis le " + x.Depuis, "Retirer l'accès", () => RetirerPartage(x.Compte, x.Receveur)));
            }
            if (l.Item2.Count == 0) recus.Children.Add(T("Aucun compte partagé avec toi : clique sur « Ajouter un compte partagé » et saisis le code reçu.", 12, Brume, FontWeights.Normal));
            foreach (var p in l.Item2)
            {
                var x = p;
                recus.Children.Add(LignePartage(x.Login, "partagé par " + x.Pseudo, "depuis le " + x.Depuis, "Retirer", () => RetirerPartageRecu(x.Compte)));
            }
        }

        FrameworkElement LignePartage(string compte, string qui, string depuis, string action, Action clic)
        {
            var g = new Grid { Height = 44 };
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(220) });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(260) });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            var c1 = T(compte, 13, Texte1, FontWeights.SemiBold);
            c1.Margin = new Thickness(10, 0, 0, 0);
            g.Children.Add(c1);
            var c2 = T(qui, 12, Ange, FontWeights.Normal);
            Grid.SetColumn(c2, 1);
            g.Children.Add(c2);
            var c3 = T(depuis, 11.5, Brume, FontWeights.Normal);
            Grid.SetColumn(c3, 2);
            g.Children.Add(c3);
            var b = BoutonTexte(action, clic, Colors.Transparent, Color.FromRgb(0x5a, 0x2a, 0x2a), Color.FromRgb(0xff, 0xb3, 0x9e), 28);
            Grid.SetColumn(b, 3);
            g.Children.Add(b);
            return new Border { Child = g, BorderBrush = new SolidColorBrush(Color.FromRgb(0x16, 0x12, 0x1f)), BorderThickness = new Thickness(0, 0, 0, 1) };
        }

        void RetirerPartage(int compte, int receveur)
        {
            if (m_session == null) return;
            var session = m_session;
            new Thread(() => { m_api.PartageRetirer(session, compte, receveur); Dispatcher.BeginInvoke((Action)Afficher); }) { IsBackground = true }.Start();
        }

        void RetirerPartageRecu(int compte)
        {
            if (m_session == null) return;
            var session = m_session;
            new Thread(() =>
            {
                m_api.PartageRetirer(session, compte, 0);
                var r = m_api.Session(session);
                Dispatcher.BeginInvoke((Action)(() => { if (r.Ok) { r.Session = session; m_compte = r; } m_coches.Remove(compte); Afficher(); }));
            }) { IsBackground = true }.Start();
        }

        // ---------------------------------------------------------------- fenêtres (partage)

        void Ouvrir(FrameworkElement contenu)
        {
            m_calque.Child = contenu;
            m_calque.Visibility = Visibility.Visible;
            m_calque.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.18)));
            contenu.RenderTransformOrigin = new Point(0.5, 0.5);
            contenu.RenderTransform = new ScaleTransform(0.96, 0.96);
            var ease = new PowerEase { Power = 3, EasingMode = EasingMode.EaseOut };
            contenu.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, new DoubleAnimation(0.96, 1, TimeSpan.FromSeconds(0.22)) { EasingFunction = ease });
            contenu.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, new DoubleAnimation(0.96, 1, TimeSpan.FromSeconds(0.22)) { EasingFunction = ease });
        }

        void FermerFenetre()
        {
            m_calque.Visibility = Visibility.Collapsed;
            m_calque.Child = null;
        }

        Border Fenetre(string surtitre, string titre, out StackPanel pile, double largeur = 440)
        {
            pile = new StackPanel();
            var entete = new Grid { Margin = new Thickness(0, 0, 0, 12) };
            var tt = new StackPanel();
            tt.Children.Add(T(Espace(surtitre), 10.5, VioletClair, FontWeights.SemiBold));
            var h = T(titre, 21, Texte1, FontWeights.SemiBold, Titre);
            h.Margin = new Thickness(0, 2, 0, 0);
            tt.Children.Add(h);
            entete.Children.Add(tt);
            var fermer = Bouton(Ic("", 10, Texte2), FermerFenetre, Carte, Trait2, 30);
            fermer.Padding = new Thickness(9, 0, 9, 0);
            fermer.HorizontalAlignment = HorizontalAlignment.Right;
            fermer.VerticalAlignment = VerticalAlignment.Top;
            entete.Children.Add(fermer);
            var tout = new StackPanel();
            tout.Children.Add(entete);
            tout.Children.Add(pile);
            return new Border
            {
                Width = largeur, Padding = new Thickness(24, 20, 24, 22), CornerRadius = new CornerRadius(18), HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center,
                Background = new LinearGradientBrush(Color.FromRgb(0x1a, 0x14, 0x28), Color.FromRgb(0x0f, 0x0b, 0x18), 90),
                BorderBrush = new SolidColorBrush(A(0x70, VioletClair)), BorderThickness = new Thickness(1),
                Effect = new DropShadowEffect { Color = Violet, BlurRadius = 60, ShadowDepth = 0, Opacity = 0.45 },
                Child = tout
            };
        }

        TextBlock Paragraphe(string t)
        {
            var p = T(t, 12.5, Texte2, FontWeights.Normal);
            p.TextWrapping = TextWrapping.Wrap;
            p.TextTrimming = TextTrimming.None;
            p.LineHeight = 19;
            p.Margin = new Thickness(0, 0, 0, 14);
            return p;
        }

        /// <summary>« Compte partagé » : saisie du code à 6 chiffres reçu.</summary>
        void AjouterComptePartage()
        {
            var fenetre = Fenetre("COMPTE PARTAGÉ", "Ajouter un compte", out var pile);
            pile.Children.Add(Paragraphe("Saisis le code à 6 chiffres que le propriétaire du compte t'a donné. Tu pourras jouer ce ou ces comptes de jeu depuis ton launcher."));
            var cases = new List<TextBox>();
            var ligne = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 0, 0, 14) };
            var erreur = T("", 12, Rouge, FontWeights.SemiBold);
            erreur.HorizontalAlignment = HorizontalAlignment.Center;
            Border valider = null;
            Action envoyer = null;
            for (var i = 0; i < 6; i++)
            {
                var tb = new TextBox
                {
                    Width = 46, Height = 58, Margin = new Thickness(5, 0, 5, 0), MaxLength = 1, FontFamily = Titre, FontSize = 26, TextAlignment = TextAlignment.Center,
                    VerticalContentAlignment = VerticalAlignment.Center, Foreground = Brushes.White, Background = new SolidColorBrush(Color.FromRgb(0x0d, 0x0a, 0x15)),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(0x33, 0x2a, 0x48)), BorderThickness = new Thickness(1.5), CaretBrush = new SolidColorBrush(VioletClair)
                };
                var index = i;
                tb.GotKeyboardFocus += (s, e) => { tb.BorderBrush = new SolidColorBrush(VioletClair); tb.SelectAll(); };
                tb.LostKeyboardFocus += (s, e) => tb.BorderBrush = new SolidColorBrush(tb.Text.Length > 0 ? Violet : Color.FromRgb(0x33, 0x2a, 0x48));
                tb.PreviewTextInput += (s, e) => { if (!e.Text.All(char.IsDigit)) e.Handled = true; };
                tb.TextChanged += (s, e) =>
                {
                    if (tb.Text.Length == 1 && index < 5) cases[index + 1].Focus();
                    var plein = cases.All(x => x.Text.Length == 1);
                    valider.Opacity = plein ? 1 : 0.5;
                };
                tb.PreviewKeyDown += (s, e) =>
                {
                    if (e.Key == Key.Back && tb.Text.Length == 0 && index > 0) { cases[index - 1].Focus(); cases[index - 1].Text = ""; e.Handled = true; }
                    else if (e.Key == Key.Enter) envoyer();
                    else if (e.Key == Key.V && Keyboard.Modifiers == ModifierKeys.Control)
                    {
                        // coller un code complet
                        var colle = new string((Clipboard.GetText() ?? "").Where(char.IsDigit).ToArray());
                        if (colle.Length >= 6) { for (var k = 0; k < 6; k++) cases[k].Text = colle[k].ToString(); cases[5].Focus(); }
                        e.Handled = true;
                    }
                };
                cases.Add(tb);
                ligne.Children.Add(tb);
            }
            pile.Children.Add(ligne);
            envoyer = () =>
            {
                var code = string.Concat(cases.Select(x => x.Text));
                if (code.Length != 6) { erreur.Text = "Le code fait 6 chiffres."; return; }
                if (m_session == null) { erreur.Text = "Aperçu : pas de serveur."; return; }
                erreur.Foreground = new SolidColorBrush(Brume);
                erreur.Text = "Vérification du code…";
                var session = m_session;
                new Thread(() =>
                {
                    var e = m_api.PartageUtiliser(session, code);
                    var r = e == null ? m_api.Session(session) : null;
                    Dispatcher.BeginInvoke((Action)(() =>
                    {
                        if (e != null) { erreur.Foreground = new SolidColorBrush(Rouge); erreur.Text = e; return; }
                        if (r != null && r.Ok) { r.Session = session; m_compte = r; }
                        FermerFenetre();
                        m_vuePartages = false;
                        Afficher();
                        MessageInfo("Compte partagé ajouté : il apparaît dans « Mes comptes ».", false);
                    }));
                }) { IsBackground = true }.Start();
            };
            valider = Bouton(T(Espace("AJOUTER LE COMPTE"), 13.5, Colors.White, FontWeights.SemiBold, Titre), () => envoyer(), Violet, A(0x99, VioletClair), 44, Color.FromRgb(0x9d, 0x74, 0xff));
            valider.Opacity = 0.5;
            ((TextBlock)valider.Child).HorizontalAlignment = HorizontalAlignment.Center;
            pile.Children.Add(valider);
            erreur.Margin = new Thickness(0, 10, 0, 0);
            pile.Children.Add(erreur);
            Ouvrir(fenetre);
            fenetre.Loaded += (s, e) => cases[0].Focus();
        }

        /// <summary>« Partager » : crée un code pour les comptes choisis (plusieurs à la fois) et l'affiche.</summary>
        void Partager(List<CompteJeu> comptes)
        {
            comptes = comptes.Where(c => c.PartagePar == null).ToList();
            if (comptes.Count == 0) { MessageInfo("Tu ne peux partager que tes propres comptes.", true); return; }
            var titre = comptes.Count == 1 ? m_dossiers.Nom(comptes[0]) : comptes.Count + " comptes";
            var fenetre = Fenetre("PARTAGER " + (comptes.Count == 1 ? "UN COMPTE" : "DES COMPTES"), titre, out var pile, 480);
            if (comptes.Count > 1)
            {
                var liste = T(string.Join("  ·  ", comptes.Select(c => m_dossiers.Nom(c))), 12, VioletPale, FontWeights.SemiBold);
                liste.TextWrapping = TextWrapping.Wrap;
                liste.Margin = new Thickness(0, -6, 0, 10);
                pile.Children.Add(liste);
            }
            pile.Children.Add(Paragraphe("Donne ce code au joueur : dans son launcher, il clique sur « Compte partagé » et le saisit."));
            var chiffres = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 0, 0, 10) };
            pile.Children.Add(chiffres);
            var validite = T("Création du code…", 12, Brume, FontWeights.Normal);
            validite.HorizontalAlignment = HorizontalAlignment.Center;
            validite.Margin = new Thickness(0, 0, 0, 14);
            pile.Children.Add(validite);
            var boutons = new Grid { Margin = new Thickness(0, 0, 0, 14) };
            boutons.ColumnDefinitions.Add(new ColumnDefinition());
            boutons.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(10) });
            boutons.ColumnDefinitions.Add(new ColumnDefinition());
            string code = null;
            DispatcherTimer minuteur = null;
            var copier = Bouton(T("Copier le code", 13, Colors.White, FontWeights.SemiBold), () =>
            {
                if (code == null) return;
                try { Clipboard.SetText(code); MessageInfo("Code copié : " + code, false); } catch { }
            }, Violet, A(0x99, VioletClair), 42, Color.FromRgb(0x9d, 0x74, 0xff));
            ((TextBlock)copier.Child).HorizontalAlignment = HorizontalAlignment.Center;
            var annuler = Bouton(T("Annuler le code", 13, Texte1, FontWeights.Medium), () =>
            {
                minuteur?.Stop();
                if (code != null && m_session != null) { var s = m_session; var c = code; new Thread(() => m_api.PartageAnnuler(s, c)) { IsBackground = true }.Start(); }
                FermerFenetre();
            }, Carte, Trait2, 42);
            ((TextBlock)annuler.Child).HorizontalAlignment = HorizontalAlignment.Center;
            boutons.Children.Add(copier);
            Grid.SetColumn(annuler, 2);
            boutons.Children.Add(annuler);
            pile.Children.Add(boutons);
            var info = new Border
            {
                Padding = new Thickness(14, 11, 14, 11), CornerRadius = new CornerRadius(10), Background = new SolidColorBrush(A(0x10, Ange)), BorderBrush = new SolidColorBrush(A(0x35, Ange)), BorderThickness = new Thickness(1),
                Child = Paragraphe("Il pourra jouer ce" + (comptes.Count > 1 ? "s comptes" : " compte") + " de jeu, rien d'autre : il n'a pas accès à ton compte Mystic (e-mail, mot de passe, autres comptes). Tu peux lui retirer l'accès à tout moment dans « Partages ».")
            };
            ((TextBlock)info.Child).Margin = new Thickness(0);
            pile.Children.Add(info);
            Ouvrir(fenetre);

            Action<string, int> montrer = (c, secondes) =>
            {
                code = c;
                chiffres.Children.Clear();
                foreach (var ch in c)
                    chiffres.Children.Add(new Border
                    {
                        Width = 50, Height = 64, Margin = new Thickness(5, 0, 5, 0), CornerRadius = new CornerRadius(10), Background = new SolidColorBrush(Color.FromRgb(0x0d, 0x0a, 0x15)),
                        BorderBrush = new SolidColorBrush(Color.FromRgb(0x3a, 0x2f, 0x55)), BorderThickness = new Thickness(1), Child = T(ch.ToString(), 30, Colors.White, FontWeights.Bold, Titre)
                    });
                foreach (Border b in chiffres.Children) ((TextBlock)b.Child).HorizontalAlignment = HorizontalAlignment.Center;
                var fin = DateTime.Now.AddSeconds(secondes);
                minuteur = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
                Action maj = () =>
                {
                    var reste = fin - DateTime.Now;
                    if (reste <= TimeSpan.Zero) { minuteur.Stop(); validite.Text = "Code expiré : ferme et recrée-en un."; validite.Foreground = new SolidColorBrush(Rouge); return; }
                    validite.Inlines.Clear();
                    validite.Inlines.Add("Valable ");
                    validite.Inlines.Add(new System.Windows.Documents.Run(((int)reste.TotalMinutes).ToString("00") + ":" + reste.Seconds.ToString("00")) { Foreground = new SolidColorBrush(Color.FromRgb(0xff, 0xb3, 0x9e)), FontWeight = FontWeights.Bold });
                    validite.Inlines.Add("  ·  une seule utilisation");
                };
                minuteur.Tick += (s, e) => maj();
                maj();
                minuteur.Start();
            };
            if (m_session == null) { montrer("247976", 899); return; } // aperçu
            var session = m_session;
            var ids = comptes.Select(c => c.Id).ToList();
            new Thread(() =>
            {
                var r = m_api.PartageCreer(session, ids);
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    if (r.Item1 == null) { validite.Text = r.Item3; validite.Foreground = new SolidColorBrush(Rouge); return; }
                    montrer(r.Item1, r.Item2);
                }));
            }) { IsBackground = true }.Start();
        }

        // petit message en bas à droite (« Code copié », erreurs)
        void MessageInfo(string texte, bool erreur)
        {
            var b = new Border
            {
                Padding = new Thickness(16, 10, 16, 10), CornerRadius = new CornerRadius(10), Background = new SolidColorBrush(Color.FromRgb(0x1a, 0x14, 0x28)),
                BorderBrush = new SolidColorBrush(erreur ? A(0x80, Rouge) : A(0x80, Color.FromRgb(0x4a, 0xde, 0x80))), BorderThickness = new Thickness(1),
                HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Bottom, Margin = new Thickness(0, 0, 22, DOCK + 14),
                Child = T(texte, 12.5, erreur ? Rouge : Vert, FontWeights.SemiBold), IsHitTestVisible = false
            };
            Grid.SetColumnSpan(b, 2);
            m_racine.Children.Add(b);
            var a = new DoubleAnimationUsingKeyFrames { Duration = TimeSpan.FromSeconds(3.2) };
            a.KeyFrames.Add(new LinearDoubleKeyFrame(0, KeyTime.FromPercent(0)));
            a.KeyFrames.Add(new LinearDoubleKeyFrame(1, KeyTime.FromPercent(0.06)));
            a.KeyFrames.Add(new LinearDoubleKeyFrame(1, KeyTime.FromPercent(0.85)));
            a.KeyFrames.Add(new LinearDoubleKeyFrame(0, KeyTime.FromPercent(1)));
            a.Completed += (s, e) => m_racine.Children.Remove(b);
            b.BeginAnimation(OpacityProperty, a);
        }

        /// <summary>MYSTIC 20/10 : crée un compte de jeu directement depuis le launcher (sans mot de passe, lié au compte web).</summary>
        void CreerCompteJeu()
        {
            if (m_session == null) return;
            var pseudo = Saisir("Pseudo du nouveau compte (affiché en jeu)", "");
            if (pseudo == null) return;
            var reponse = Saisir("Réponse secrète (demandée en jeu pour supprimer un personnage)", "");
            if (reponse == null) return;
            var session = m_session;
            m_statut.Text = "Création du compte « " + pseudo + " »…";
            new Thread(() =>
            {
                var r = m_api.CreerJeu(session, pseudo, reponse);
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    if (!r.Ok) { MessageInfo(r.Erreur ?? "Création impossible.", true); m_statut.Text = ""; return; }
                    r.Session = session;
                    m_compte = r;
                    m_statut.Text = "Compte « " + pseudo + " » créé : aucun mot de passe, il s'ouvre depuis le launcher.";
                    Afficher();
                }));
            }) { IsBackground = true }.Start();
        }

        // fenêtre de saisie (nom de groupe, renommer)
        string Saisir(string titre, string valeur, bool videPermis = false)
        {
            var f = new Window
            {
                Owner = this, Width = 380, Height = 176, WindowStyle = WindowStyle.None, AllowsTransparency = true, Background = Brushes.Transparent,
                WindowStartupLocation = WindowStartupLocation.CenterOwner, ResizeMode = ResizeMode.NoResize, ShowInTaskbar = false
            };
            var pile = new StackPanel { Margin = new Thickness(20) };
            pile.Children.Add(T(titre, 14, Texte1, FontWeights.SemiBold));
            var champ = new TextBox
            {
                Text = valeur ?? "", MaxLength = 30, Height = 36, Margin = new Thickness(0, 12, 0, 14), FontFamily = Police, FontSize = 13.5, Padding = new Thickness(8, 0, 8, 0),
                VerticalContentAlignment = VerticalAlignment.Center, Foreground = new SolidColorBrush(Texte1), Background = new SolidColorBrush(Color.FromRgb(0x0d, 0x0a, 0x15)),
                BorderBrush = new SolidColorBrush(Trait2), CaretBrush = new SolidColorBrush(VioletClair)
            };
            pile.Children.Add(champ);
            string resultat = null;
            var boutons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            boutons.Children.Add(BoutonTexte("Annuler", () => f.Close()));
            var ok = BoutonViolet("Valider", () => { resultat = champ.Text.Trim(); f.Close(); }, 32);
            ok.Margin = new Thickness(8, 0, 0, 0);
            boutons.Children.Add(ok);
            pile.Children.Add(boutons);
            champ.KeyDown += (s, e) => { if (e.Key == Key.Enter) { resultat = champ.Text.Trim(); f.Close(); } else if (e.Key == Key.Escape) f.Close(); };
            f.Content = new Border { CornerRadius = new CornerRadius(12), Background = new SolidColorBrush(Color.FromRgb(0x17, 0x12, 0x23)), BorderBrush = new SolidColorBrush(Trait2), BorderThickness = new Thickness(1), Child = pile };
            f.Loaded += (s, e) => { champ.Focus(); champ.SelectAll(); };
            f.ShowDialog();
            if (resultat == null) return null;
            return resultat.Length == 0 && !videPermis ? null : resultat;
        }

        // ---------------------------------------------------------------- dock du bas

        FrameworkElement ConstruireDock()
        {
            var dock = new Grid
            {
                Background = new LinearGradientBrush(new GradientStopCollection
                {
                    new GradientStop(A(0x10, Ange), 0), new GradientStop(Fond, 0.3), new GradientStop(Fond, 0.7), new GradientStop(A(0x14, Demon), 1)
                }, new Point(0, 0.5), new Point(1, 0.5))
            };
            Grid.SetRow(dock, 2);
            dock.Children.Add(new Border { BorderBrush = new SolidColorBrush(Trait), BorderThickness = new Thickness(0, 1, 0, 0) });
            var g = new Grid { Margin = new Thickness(22, 0, 22, 0) };
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            dock.Children.Add(g);

            var gauche = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            var l1 = new Grid();
            m_statut = T("Vérification du jeu…", 12.5, Texte1, FontWeights.SemiBold);
            m_pourcent = T("", 12, VioletClair, FontWeights.SemiBold);
            m_pourcent.HorizontalAlignment = HorizontalAlignment.Right;
            l1.Children.Add(m_statut);
            l1.Children.Add(m_pourcent);
            gauche.Children.Add(l1);
            m_piste = new Grid { Height = 5, Margin = new Thickness(0, 7, 0, 7), Background = new SolidColorBrush(Trait), ClipToBounds = true };
            m_barre = new Border { Width = 0, HorizontalAlignment = HorizontalAlignment.Left, CornerRadius = new CornerRadius(3), Background = new LinearGradientBrush(new GradientStopCollection { new GradientStop(Ange, 0), new GradientStop(Violet, 0.5), new GradientStop(Demon, 1) }, 0) };
            m_piste.Children.Add(m_barre);
            gauche.Children.Add(m_piste);
            var l3 = new StackPanel { Orientation = Orientation.Horizontal };
            m_infos = T("", 11, Brume, FontWeights.Normal);
            m_infos.MaxWidth = 330;
            l3.Children.Add(m_infos);
            l3.Children.Add(Lien("Vérifier les fichiers", () => Travailler(true)));
            l3.Children.Add(Lien("Ouvrir le dossier", () => { try { Process.Start("explorer.exe", "\"" + m_updater.BaseDir + "\""); } catch { } }));
            gauche.Children.Add(l3);
            g.Children.Add(gauche);
            AfficherInfos();

            var liens = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(20, 0, 20, 0), VerticalAlignment = VerticalAlignment.Center };
            liens.Children.Add(LienIcone("", "Site", m_updater.SiteUrl));
            liens.Children.Add(LienIcone("", "Boutique", m_updater.SiteUrl + "boutique.html"));
            liens.Children.Add(LienIcone("", "Voter", m_updater.SiteUrl + "voter.html"));
            liens.Children.Add(LienIcone("", "Discord", m_updater.SiteUrl + "#discord"));
            Grid.SetColumn(liens, 1);
            g.Children.Add(liens);

            m_boutonTexte = T("", 19, Colors.White, FontWeights.Bold, Titre);
            m_boutonTexte.HorizontalAlignment = HorizontalAlignment.Center;
            m_bouton = new Border
            {
                Width = 230, Height = 58, CornerRadius = new CornerRadius(14), VerticalAlignment = VerticalAlignment.Center, Child = m_boutonTexte,
                Background = new LinearGradientBrush(new GradientStopCollection { new GradientStop(Ange, 0), new GradientStop(Violet, 0.4), new GradientStop(Demon, 1.2) }, 20),
                Effect = new DropShadowEffect { Color = Violet, BlurRadius = 26, ShadowDepth = 0, Opacity = 0.7 }, Opacity = 0.45
            };
            m_bouton.MouseEnter += (s, e) => { if (Actif()) m_bouton.Effect.BeginAnimation(DropShadowEffect.OpacityProperty, new DoubleAnimation(1, TimeSpan.FromSeconds(0.15))); };
            m_bouton.MouseLeave += (s, e) => m_bouton.Effect.BeginAnimation(DropShadowEffect.OpacityProperty, new DoubleAnimation(0.7, TimeSpan.FromSeconds(0.2)));
            m_bouton.MouseLeftButtonUp += (s, e) => ClicBouton();
            Grid.SetColumn(m_bouton, 2);
            g.Children.Add(m_bouton);
            return dock;
        }

        TextBlock Lien(string texte, Action clic)
        {
            var t = T(texte, 11, Texte2, FontWeights.Normal);
            t.TextDecorations = TextDecorations.Underline;
            t.Cursor = Cursors.Hand;
            t.Margin = new Thickness(10, 0, 0, 0);
            t.MouseLeftButtonUp += (s, e) => clic();
            return t;
        }

        Border LienIcone(string glyphe, string nom, string url)
        {
            var b = Bouton(Ic(glyphe, 15, VioletClair), () => Outils.OuvrirSite(url), Colors.Transparent, Trait2, 38);
            b.Width = 38;
            b.Padding = new Thickness(0);
            b.CornerRadius = new CornerRadius(10);
            b.Margin = new Thickness(4, 0, 4, 0);
            b.ToolTip = nom;
            return b;
        }

        void AfficherInfos() => m_infos.Text = (m_updater.Version != null && m_updater.Version != "?" ? "Version du " + m_updater.Version + "  ·  " : "") + m_updater.BaseDir + "  ·";

        bool Actif() => m_etat == Etat.AInstaller || ((m_etat == Etat.Pret || m_etat == Etat.Erreur) && (m_updater.GameExists || m_apercu != null) && m_coches.Count > 0);

        void MajBouton()
        {
            if (m_bouton == null) return;
            switch (m_etat)
            {
                case Etat.AInstaller: m_boutonTexte.Text = Espace("INSTALLER"); break;
                case Etat.Verification:
                case Etat.Travail: m_boutonTexte.Text = "PATIENTEZ…"; break;
                default: m_boutonTexte.Text = m_coches.Count > 0 ? Espace("JOUER") + "  ·  " + m_coches.Count : Espace("JOUER"); break;
            }
            m_bouton.Cursor = Actif() ? Cursors.Hand : Cursors.Arrow;
            m_bouton.Opacity = Actif() ? 1 : 0.45;
            m_bouton.ToolTip = m_etat == Etat.Pret && m_coches.Count == 0 ? "Coche un ou plusieurs comptes" : null;
        }

        void Progression(string texte, int pourcent)
        {
            m_statut.Text = texte;
            pourcent = Math.Max(0, Math.Min(100, pourcent));
            m_pourcent.Text = pourcent + " %";
            m_barre.BeginAnimation(WidthProperty, new DoubleAnimation(Math.Max(0, m_piste.ActualWidth) * pourcent / 100.0, TimeSpan.FromSeconds(0.3)) { EasingFunction = new PowerEase { Power = 2, EasingMode = EasingMode.EaseOut } });
        }

        // ---------------------------------------------------------------- installation / mise à jour / jeu

        void Verifier()
        {
            m_statut.Foreground = new SolidColorBrush(Texte1);
            if (!m_updater.GameExists)
            {
                m_etat = Etat.AInstaller;
                m_statut.Text = "Le jeu n'est pas installé : Réglages › Dossier du jeu, puis INSTALLER (≈ 3,4 Go).";
                m_pourcent.Text = "";
                m_barre.BeginAnimation(WidthProperty, null);
                m_barre.Width = 0;
                MajBouton();
                return;
            }
            Travailler(false);
        }

        void Travailler(bool complet)
        {
            if (m_etat == Etat.Travail) return;
            m_etat = Etat.Travail;
            MajBouton();
            Progression(complet ? (m_updater.GameExists ? "Vérification de tous les fichiers…" : "Préparation de l'installation…") : "Recherche de mises à jour…", 0);
            new Thread(() =>
            {
                var ok = m_updater.Run((texte, p) => Dispatcher.BeginInvoke((Action)(() => Progression(texte, p))), complet);
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    if (ok) { m_etat = Etat.Pret; Progression(m_statut.Text, 100); }
                    else
                    {
                        m_statut.Text = m_updater.LastError;
                        m_statut.Foreground = new SolidColorBrush(Rouge);
                        m_etat = m_updater.GameExists ? Etat.Erreur : Etat.AInstaller;
                    }
                    AfficherInfos();
                    MajBouton();
                }));
            }) { IsBackground = true }.Start();
        }

        void ChangerDossier()
        {
            if (m_etat == Etat.Travail) return;
            using (var d = new System.Windows.Forms.FolderBrowserDialog { Description = "Dossier d'installation de Mystic", SelectedPath = m_updater.BaseDir, ShowNewFolderButton = true })
            {
                if (d.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
                var choix = d.SelectedPath;
                if (!File.Exists(System.IO.Path.Combine(choix, "app", "Dofus.exe")) && Directory.EnumerateFileSystemEntries(choix).Any())
                    choix = System.IO.Path.Combine(choix, "Mystic");
                m_updater.ChangerDossier(choix);
            }
            AfficherInfos();
            Verifier();
        }

        void ClicBouton()
        {
            if (!Actif()) return;
            if (m_etat == Etat.AInstaller) Travailler(true);
            else JouerListe(m_compte.Comptes.Where(c => m_coches.Contains(c.Id)).ToList());
        }

        /// <summary>Lance plusieurs comptes, un toutes les 2 secondes (chacun avec son ticket de connexion automatique).</summary>
        void JouerListe(List<CompteJeu> comptes)
        {
            Fantomes.Nettoyer(m_updater.AppDir, TimeSpan.Zero, m_updater.Log);
            comptes =comptes.Where(c => !EnJeu(c)).ToList();
            if (comptes.Count == 0) return;
            var file = new Queue<CompteJeu>(comptes);
            Jouer(file.Dequeue());
            if (file.Count == 0) return;
            var minuteur = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
            minuteur.Tick += (s, e) => { Jouer(file.Dequeue()); if (file.Count == 0) minuteur.Stop(); };
            minuteur.Start();
        }

        void Jouer(CompteJeu c)
        {
            if (c == null || !(m_etat == Etat.Pret || m_etat == Etat.Erreur) || !m_updater.GameExists) return;
            Fantomes.Nettoyer(m_updater.AppDir, TimeSpan.Zero, m_updater.Log);
            if (EnJeu(c)) { MessageInfo("« " + m_dossiers.Nom(c) + " » est déjà en jeu.", false); return; }
            m_statut.Foreground = new SolidColorBrush(Texte1);
            m_statut.Text = "Connexion automatique de « " + m_dossiers.Nom(c) + " »…";
            m_lancement[c.Id] = DateTime.Now;
            Afficher();
            var session = m_session;
            new Thread(() =>
            {
                ReponseWeb r = session != null ? m_api.Ticket(session, c.Id) : null;
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    try
                    {
                        if (r != null && !r.Ok) { m_lancement.Remove(c.Id); m_statut.Text = r.Erreur ?? "Connexion automatique impossible."; m_statut.Foreground = new SolidColorBrush(Rouge); Afficher(); return; }
                        var p = m_updater.StartGame(r?.Login, r?.Ticket, c.Id);
                        if (p != null) m_enJeu[c.Id] = p;
                        m_statut.Text = "« " + m_dossiers.Nom(c) + " » lancé.";
                        Afficher();
                    }
                    catch (Exception ex)
                    {
                        m_statut.Text = "Impossible de lancer le jeu : " + ex.Message;
                        m_statut.Foreground = new SolidColorBrush(Rouge);
                    }
                }));
            }) { IsBackground = true }.Start();
        }

        void SuivreStatut()
        {
            new Thread(() =>
            {
                while (!m_fini)
                {
                    var s = m_api.Statut();
                    Dispatcher.BeginInvoke((Action)(() => AfficherStatut(s)));
                    Thread.Sleep(30000);
                }
            }) { IsBackground = true }.Start();
        }

        void AfficherStatut(Tuple<bool, int> s)
        {
            var ok = s != null && s.Item1;
            m_point.Fill = new SolidColorBrush(ok ? Color.FromRgb(0x4a, 0xde, 0x80) : Rouge);
            m_statutServeur.Text = ok ? "Serveur en ligne" : "Serveur hors ligne";
            m_statutServeur.Foreground = new SolidColorBrush(ok ? Vert : Rouge);
        }

        void SeDeconnecter()
        {
            if (m_etat == Etat.Travail) return;
            var session = m_session;
            SessionGardee.Effacer();
            if (session != null) new Thread(() => m_api.Deconnexion(session)) { IsBackground = true }.Start();
            m_fini = true;
            var connexion = new LauncherWindow(m_updater, null);
            Application.Current.MainWindow = connexion;
            connexion.Show();
            Close();
        }

        void RevoquerSessionTemporaire()
        {
            m_fini = true;
            if (m_session == null || m_sessionGardee || Application.Current.MainWindow != this) return;
            var session = m_session;
            var t = new Thread(() => m_api.Deconnexion(session)) { IsBackground = true };
            t.Start();
            t.Join(1500);
        }

        void Fermer()
        {
            if (m_etat == Etat.Travail) m_updater.Log("Fermeture pendant le téléchargement : il reprendra au prochain lancement");
            var fondu = new DoubleAnimation(0, TimeSpan.FromSeconds(0.25));
            fondu.Completed += (s, e) => Close();
            BeginAnimation(OpacityProperty, fondu);
        }

        // ---------------------------------------------------------------- aperçu (captures)

        static class Apercu
        {
            public static Dossiers Dossiers() => new Dossiers
            {
                Liste = new List<Dossiers.Dossier> { new Dossiers.Dossier { Nom = "PVP", Comptes = { "Pommes15", "Pommejte9", "pvpc" } }, new Dossiers.Dossier { Nom = "Mules", Comptes = { "xp6" } } },
                Alias = new Dictionary<string, string> { { "pommes15", "Team Pandawa" } }
            };

            public static Tuple<List<Partage>, List<Partage>> Partages() => Tuple.Create(
                new List<Partage> { new Partage { Compte = 1, Login = "Pommes15", Pseudo = "Lucas", Depuis = "18/10/2026" }, new Partage { Compte = 2, Login = "Pommejte9", Pseudo = "Lucas", Depuis = "18/10/2026" } },
                new List<Partage> { new Partage { Compte = 9, Login = "compte_de_lucas", Pseudo = "Lucas", Depuis = "17/10/2026" } });
        }

        void LancerApercu()
        {
            AfficherStatut(Tuple.Create(true, 47));
            if (m_compte.Comptes.Count > 4) m_compte.Comptes[4].PartagePar = "Lucas";
            m_coches.Add(1);
            m_coches.Add(2);
            var etapes = new List<Tuple<double, Action, string>>
            {
                Tuple.Create(0.6, (Action)(() => { m_etat = Etat.Travail; Progression("Mise à jour : 2,18 Go / 3,42 Go  ·  12,4 Mo/s", 64); MajBouton(); }), "principal_01_maj"),
                Tuple.Create(1.6, (Action)(() => { m_etat = Etat.Pret; Progression("Le jeu est à jour.", 100); m_enJeu.Clear(); Afficher(); }), "principal_02_comptes"),
                Tuple.Create(2.6, (Action)(() => Partager(m_compte.Comptes.Take(2).ToList())), "principal_03_partager"),
                Tuple.Create(3.6, (Action)(() => { FermerFenetre(); AjouterComptePartage(); }), "principal_04_ajouter"),
                Tuple.Create(4.6, (Action)(() => { FermerFenetre(); m_vuePartages = true; Afficher(); }), "principal_05_partages")
            };
            var debut = DateTime.Now;
            var index = 0;
            var faite = false;
            var minuteur = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(30) };
            minuteur.Tick += (s, e) =>
            {
                var t = (DateTime.Now - debut).TotalSeconds;
                if (index >= etapes.Count) { minuteur.Stop(); Application.Current.Shutdown(); return; }
                if (!faite && t >= etapes[index].Item1) { etapes[index].Item2(); faite = true; }
                if (faite && t >= etapes[index].Item1 + 0.7)
                {
                    LauncherWindow.Capturer((Visual)Content, (int)W, (int)H, System.IO.Path.Combine(m_apercu, etapes[index].Item3 + ".png"));
                    index++;
                    faite = false;
                }
            };
            minuteur.Start();
        }
    }
}
