// MYSTIC 18/10/2026 → PADGREF 20/10/2026 : grand launcher Padgref, maquette v2 « launcher-padgref.html » (thème Azur) :
// rail d'icônes à gauche (Comptes, Partages, Site, Boutique, Voter, Discord, Paramètres, Déconnexion) ; barre du haut
// (recherche, serveur en ligne / hors ligne, compte web) ; colonne Dossiers (glisser-déposer, double-clic = renommer) ;
// grille de cartes de comptes (cocher jusqu'à 40, double-clic sur le nom = renommer, double-clic sur la carte = jouer) ;
// dock du bas (pile des comptes cochés, Fenêtres, gros bouton JOUER avec la mise à jour dans le bouton) ; colonne de droite
// (prochain événement, notes de mise à jour, actualités : launcher.json du site si présent) ; tiroir Paramètres.
// Partage de comptes par code à 6 chiffres, création de comptes de jeu sans mot de passe, Zaap local, Dofus fantômes.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
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
using static MysticLauncher.Theme;
using Img = System.Windows.Controls.Image;

namespace MysticLauncher
{
    /// <summary>Dossiers de comptes et noms choisis (gardés sur le PC, par compte web) : %APPDATA%\Mystic\dossiers_&lt;empreinte&gt;.json</summary>
    class Dossiers
    {
        public class Dossier
        {
            public string Nom;
            public bool Replie;
            public List<string> Comptes = new List<string>();
            public string Icone;      // PADGREF 21/10 : icône officielle du jeu (Icones.Catalogue), null = dossier
        }

        public List<Dossier> Liste = new List<Dossier>();
        public bool SansDossierReplie;
        public Dictionary<string, string> Alias = new Dictionary<string, string>();
        public Dictionary<string, List<string>> Tags = new Dictionary<string, List<string>>();   // PADGREF 21/10 : login -> icônes
        string m_fichier;

        public List<string> TagsDe(string login) => Tags.TryGetValue(login.ToLowerInvariant(), out var l) && l != null ? l : new List<string>();

        public void BasculerTag(string login, string cle)
        {
            var k = login.ToLowerInvariant();
            if (!Tags.TryGetValue(k, out var l) || l == null) Tags[k] = l = new List<string>();
            if (!l.Remove(cle)) l.Add(cle);
            if (l.Count == 0) Tags.Remove(k);
            Sauver();
        }

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
                        d.Tags = lu.Tags ?? new Dictionary<string, List<string>>();
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

    /// <summary>PADGREF 20/10 : réglages du launcher (tiroir Paramètres) : %APPDATA%\Mystic\reglages_launcher.json</summary>
    class Reglages
    {
        public bool Fantomes = true, Placer = true, Reduire;
        public string Disposition = "auto";

        static string Fichier => System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Mystic", "reglages_launcher.json");

        public static Reglages Charger()
        {
            try
            {
                if (File.Exists(Fichier))
                    return new JavaScriptSerializer().Deserialize<Reglages>(File.ReadAllText(Fichier, Encoding.UTF8)) ?? new Reglages();
            }
            catch { }
            return new Reglages();
        }

        public void Sauver()
        {
            try
            {
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Fichier));
                File.WriteAllText(Fichier, new JavaScriptSerializer().Serialize(this), Encoding.UTF8);
            }
            catch { }
        }
    }

    /// <summary>PADGREF 20/10 : place les fenêtres de Dofus lancées (multi-comptes) : automatique, grille, cascade, plein écran.</summary>
    static class PlacementFenetres
    {
        [DllImport("user32.dll")] static extern bool MoveWindow(IntPtr h, int x, int y, int l, int ht, bool repeindre);
        [DllImport("user32.dll")] static extern bool ShowWindow(IntPtr h, int cmd);

        public static readonly string[] Noms = { "auto", "grille", "cascade", "plein" };
        public static readonly string[] Libelles = { "Automatique", "Grille", "Cascade", "Plein écran" };

        /// <summary>Rectangles (fractions de l'écran) pour n fenêtres ; marge et écart en fractions.</summary>
        public static List<Rect> Positions(string dispo, int n, double marge, double ecart)
        {
            var l = new List<Rect>();
            if (n <= 0) return l;
            var utile = 1 - 2 * marge;
            if (dispo == "plein")
            {
                for (var i = 0; i < n; i++) l.Add(new Rect(marge, marge, utile, utile));
                return l;
            }
            if (dispo == "cascade")
            {
                var pas = Math.Min(0.06, (utile - 0.55) / Math.Max(1, n - 1));
                for (var i = 0; i < n; i++) l.Add(new Rect(marge + i * pas, marge + i * pas, 0.55, 0.55));
                return l;
            }
            var cols = dispo == "grille" ? (int)Math.Ceiling(Math.Sqrt(n)) : n <= 2 ? n : n <= 4 ? 2 : n <= 6 ? 3 : n <= 8 ? 4 : (int)Math.Ceiling(Math.Sqrt(n));
            var lignes = (int)Math.Ceiling(n / (double)cols);
            var w = (utile - (cols - 1) * ecart) / cols;
            var h = (utile - (lignes - 1) * ecart) / lignes;
            for (var i = 0; i < n; i++)
                l.Add(new Rect(marge + (i % cols) * (w + ecart), marge + (i / cols) * (h + ecart), w, h));
            return l;
        }

        /// <summary>Place les fenêtres (dans l'ordre de la liste) sur l'écran principal ; renvoie le nombre de fenêtres placées.</summary>
        public static int Placer(IList<Process> processus, string dispo)
        {
            var fenetres = new List<IntPtr>();
            foreach (var p in processus)
                try
                {
                    p.Refresh();
                    if (!p.HasExited && p.MainWindowHandle != IntPtr.Zero) fenetres.Add(p.MainWindowHandle);
                }
                catch { }
            var zone = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea;
            var pos = Positions(dispo, fenetres.Count, 0, 0);
            for (var i = 0; i < fenetres.Count; i++)
            {
                if (dispo == "plein") { ShowWindow(fenetres[i], 3); continue; } // SW_MAXIMIZE
                ShowWindow(fenetres[i], 9); // SW_RESTORE
                var r = pos[i];
                MoveWindow(fenetres[i], zone.X + (int)(r.X * zone.Width), zone.Y + (int)(r.Y * zone.Height), (int)(r.Width * zone.Width), (int)(r.Height * zone.Height), true);
            }
            return fenetres.Count;
        }
    }

    class PrincipalWindow : Window
    {
        const double W = Theme.W, H = Theme.H, RAIL = 74;
        const int MAX_COCHES = 40; // PADGREF 24/10 (demande utilisateur) : 8 -> 40

        static readonly Color[] CouleursDossiers =
        {
            Theme.Accent, Theme.Rgb(0xA5, 0x5C, 0xFF), Theme.Or, Theme.Rgb(0x22, 0xD3, 0x9A), Theme.Rouge, Theme.Rgb(0xFF, 0xB3, 0x47), Theme.Accent2
        };

        readonly Updater m_updater;
        readonly CompteApi m_api;
        ReponseWeb m_compte;
        readonly string m_session, m_apercu;
        readonly bool m_sessionGardee;
        readonly Dossiers m_dossiers;
        readonly Reglages m_reglages;

        readonly Grid m_racine = new Grid();
        readonly HashSet<int> m_coches = new HashSet<int>();
        readonly Dictionary<int, Process> m_enJeu = new Dictionary<int, Process>();
        readonly Dictionary<int, DateTime> m_lancement = new Dictionary<int, DateTime>();
        readonly List<int> m_ordre = new List<int>(); // ordre de lancement (placement des fenêtres)
        readonly Dictionary<string, ImageSource> m_avatars = new Dictionary<string, ImageSource>();
        Dossiers.Dossier m_dossierActif; // null = Tous les comptes
        Dossiers.Dossier m_renommerDossier;
        int m_renommerCompte = -1;
        bool m_vuePartages;
        string m_recherche = "";
        string m_signature;

        // interface
        readonly StackPanel m_listeDossiers = new StackPanel();
        readonly Grid m_centre = new Grid();
        ScrollViewer m_defile;
        readonly List<Tuple<Grid, Rectangle, FrameworkElement, SolidColorBrush>> m_boutonsRail = new List<Tuple<Grid, Rectangle, FrameworkElement, SolidColorBrush>>();
        TextBlock m_statut, m_dockTitre, m_boutonTexte, m_statutServeur;
        StackPanel m_pile;
        Ellipse m_point;
        Border m_bouton, m_calque;
        Rectangle m_remplissage, m_lueurJouer;
        Grid m_tiroir;
        Border m_panneauTiroir;
        StackPanel m_panEvenement, m_panMaj, m_panActus;
        TextBlock[] m_compteur;
        DateTime? m_finEvenement;
        string m_lienDiscord = Theme.Discord; // invitation (launcher.json « discord » peut la changer)

        enum Etat { Verification, AInstaller, Travail, Pret, Erreur }
        Etat m_etat = Etat.Verification;
        int m_pourcent;
        string m_travail = "MISE À JOUR";
        bool? m_serveurEnLigne;
        DateTime m_aJourJusqua;
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
            m_reglages = apercu != null ? new Reglages() : Reglages.Charger();
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

            Title = Theme.Nom;
            Width = W;
            Height = H;
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Background = Brushes.Transparent;
            ResizeMode = ResizeMode.CanMinimize;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            UseLayoutRounding = true;
            TextOptions.SetTextFormattingMode(this, TextFormattingMode.Ideal);

            var cadre = new Border { CornerRadius = new CornerRadius(14), Background = B(Fond), BorderBrush = B(Ligne), BorderThickness = new Thickness(1), Child = m_racine, Opacity = 0 };
            m_racine.Clip = new RectangleGeometry(new Rect(0, 0, W - 2, H - 2), 13, 13);
            Content = cadre;

            m_racine.RowDefinitions.Add(new RowDefinition { Height = new GridLength(Theme.BARRE) });
            m_racine.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            m_racine.Children.Add(Theme.BarreTitre(this, null, Fermer));

            var corps = new Grid();
            corps.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(RAIL) });
            corps.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            Grid.SetRow(corps, 1);
            m_racine.Children.Add(corps);
            corps.Children.Add(ConstruireRail());
            var principal = new Grid();
            principal.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            principal.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            Grid.SetColumn(principal, 1);
            corps.Children.Add(principal);
            principal.Children.Add(ConstruireHaut());

            var zone = new Grid { Margin = new Thickness(20, 0, 20, 18) };
            zone.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(228) });
            zone.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(14) });
            zone.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            zone.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(14) });
            zone.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(232) });
            Grid.SetRow(zone, 1);
            principal.Children.Add(zone);
            zone.Children.Add(ConstruireDossiers());
            var milieu = new Grid();
            milieu.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            milieu.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            Grid.SetColumn(milieu, 2);
            zone.Children.Add(milieu);
            milieu.Children.Add(m_centre);
            var dock = ConstruireDock();
            Grid.SetRow(dock, 1);
            milieu.Children.Add(dock);
            var droite = ConstruireDroite();
            Grid.SetColumn(droite, 4);
            zone.Children.Add(droite);

            m_calque = new Border { Background = B(Color.FromArgb(0xA0, 3, 6, 14)), Visibility = Visibility.Collapsed };
            m_calque.MouseLeftButtonDown += (s, e) => { if (e.OriginalSource == m_calque) FermerFenetre(); };
            Grid.SetRow(m_calque, 1);
            m_racine.Children.Add(m_calque);
            m_racine.Children.Add(ConstruireTiroir());

            Afficher();

            Loaded += (s, e) =>
            {
                cadre.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.35)));
                var suivi = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
                suivi.Tick += (o, a) => { if (Signature() != m_signature && Libre()) Afficher(); MajBouton(); };
                suivi.Start();
                // MYSTIC 20/10 : Dofus restés en mémoire sans fenêtre après fermeture → fermés (sinon impossible de relancer)
                if (m_apercu == null)
                {
                    var fantomes = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
                    var occupe = false;
                    fantomes.Tick += (o, a) =>
                    {
                        if (occupe || !m_reglages.Fantomes) return;
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
                var horloge = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
                horloge.Tick += (o, a) => MajCompteur();
                horloge.Start();
                ChargerInfos();
                if (m_apercu != null) LancerApercu();
                else { SuivreStatut(); Verifier(); }
            };
            Closed += (s, e) => RevoquerSessionTemporaire();
        }

        bool Libre() => !m_vuePartages && m_calque.Visibility != Visibility.Visible && m_renommerCompte < 0 && m_renommerDossier == null && Mouse.LeftButton != MouseButtonState.Pressed;

        string Signature() => string.Join(",", m_compte.Comptes.Select(c => EnJeu(c) ? (EnConnexion(c) ? "c" : "j") : "-"));

        void Toast(string texte, bool erreur = false) => Theme.Toast(m_racine, texte, erreur, 112);

        // ---------------------------------------------------------------- petits outils

        static ContextMenu Menu()
        {
            // PADGREF 21/10 : menu du clic droit stylé (cadre arrondi, survol, coche, flèche) au lieu du menu Windows standard
            var m = new ContextMenu { Foreground = B(Texte), FontFamily = Police };
            var styles = Icones.Styles();
            m.Resources.MergedDictionaries.Add(styles);
            if (styles[typeof(ContextMenu)] is Style st) m.Style = st;
            return m;
        }

        /// <summary>PADGREF 21/10 : sous-menus d'icônes officielles (classes, épées, divers) ; choix = coché.</summary>
        void AjouterIcones(ItemsControl parent, Func<string, bool> choisie, Action<string> clic)
        {
            foreach (var groupe in Icones.Catalogue(m_updater.AppDir).GroupBy(x => x.Groupe))
            {
                var sous = Element(null, groupe.Key, null);
                sous.Icon = Icones.Vue(m_updater.AppDir, groupe.First().Cle, 18);
                foreach (var ic in groupe)
                {
                    var cle = ic.Cle;
                    var e = Element(null, ic.Nom, () => clic(cle));
                    e.Icon = Icones.Vue(m_updater.AppDir, cle, 18);
                    e.IsChecked = choisie(cle);
                    sous.Items.Add(e);
                }
                parent.Items.Add(sous);
            }
        }

        static MenuItem Element(string icone, string texte, Action clic)
        {
            var i = new MenuItem { Header = texte, FontFamily = Police, FontSize = 13, Foreground = B(Texte), Icon = icone == null ? null : Ic(icone, 14, B(Doux)) };
            if (clic != null) i.Click += (s, e) => clic();
            return i;
        }

        static Border Panneau(UIElement contenu, string titre = null)
        {
            var pile = new StackPanel();
            if (titre != null)
            {
                var t = T(titre.ToUpperInvariant(), 11.5, Doux, FontWeights.Bold);
                t.Margin = new Thickness(0, 0, 0, 10);
                pile.Children.Add(t);
            }
            pile.Children.Add(contenu);
            return new Border { CornerRadius = new CornerRadius(18), BorderBrush = B(Ligne), BorderThickness = new Thickness(1), Background = B(Carte), Padding = new Thickness(14), Child = pile };
        }

        // ---------------------------------------------------------------- rail d'icônes

        FrameworkElement ConstruireRail()
        {
            var rail = new Grid { Background = B(Melange(Fond, 0.85, Colors.Black)) };
            rail.Children.Add(new Border { BorderBrush = B(Ligne), BorderThickness = new Thickness(0, 0, 1, 0) });
            var haut = new StackPanel { Margin = new Thickness(0, 14, 0, 0), HorizontalAlignment = HorizontalAlignment.Center };
            haut.Children.Add(new Img { Source = Theme.Image("p_logo.png"), Width = 44, Height = 44, Margin = new Thickness(0, 0, 0, 12) });
            haut.Children.Add(BoutonRail(Ic(IcJouer, 22, null, true), "Mes comptes", () => { m_vuePartages = false; Afficher(); }, true));
            haut.Children.Add(BoutonRail(Ic(IcPartages, 22, null), "Partages de comptes", () => { m_vuePartages = true; Afficher(); }, true));
            haut.Children.Add(BoutonRail(Ic(IcActus, 22, null), "Site Padgref", () => Outils.OuvrirSite(m_updater.SiteUrl), false));
            haut.Children.Add(BoutonRail(new Img { Source = Theme.Image("p_ogrine.png"), Width = 24, Height = 24 }, "Espace Jeux (ogrines)", () => Outils.OuvrirSite(m_updater.SiteUrl + "jeux.html"), false));
            haut.Children.Add(BoutonRail(Ic(IcEtoile, 22, null), "Classement", () => Outils.OuvrirSite(m_updater.SiteUrl + "classement.html"), false));
            haut.Children.Add(BoutonRail(new Img { Source = Theme.Image("p_groupe.png"), Width = 24, Height = 24 }, "Discord Padgref", () => Outils.OuvrirSite(m_lienDiscord), false));
            rail.Children.Add(haut);
            var bas = new StackPanel { VerticalAlignment = VerticalAlignment.Bottom, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 0, 0, 12) };
            bas.Children.Add(BoutonRail(Ic(IcReglages, 22, null), "Paramètres", OuvrirTiroir, false));
            bas.Children.Add(BoutonRail(Ic(IcDeconnexion, 20, null), "Se déconnecter (oublie la connexion sur ce PC)", DemanderDeconnexion, false));
            rail.Children.Add(bas);
            return rail;
        }

        /// <summary>Bouton du rail (50 × 50) ; navigation = fait partie des vues (indicateur actif).</summary>
        FrameworkElement BoutonRail(FrameworkElement icone, string infobulle, Action clic, bool navigation)
        {
            var couleur = B(Doux);
            if (icone is Viewbox vb && ((Grid)vb.Child).Children[0] is System.Windows.Shapes.Path p)
            {
                if (p.Fill != null || p.StrokeThickness == 0) p.Fill = couleur; else p.Stroke = couleur;
            }
            var fond = new SolidColorBrush(Colors.Transparent);
            var b = new Border { Width = 50, Height = 50, CornerRadius = new CornerRadius(15), Background = fond, Child = icone, Cursor = Cursors.Hand, ToolTip = infobulle };
            var barre = new Rectangle { Width = 3, RadiusX = 1.5, RadiusY = 1.5, Fill = B(Accent2), HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(-12, 14, 0, 14), Visibility = Visibility.Hidden };
            var g = new Grid { Margin = new Thickness(0, 0, 0, 6) };
            g.Children.Add(b);
            g.Children.Add(barre);
            b.MouseEnter += (s, e) => { if (barre.Visibility != Visibility.Visible) { fond.Color = Carte; couleur.Color = Texte; } };
            b.MouseLeave += (s, e) => { if (barre.Visibility != Visibility.Visible) { fond.Color = Colors.Transparent; couleur.Color = Doux; } };
            Clic(b, clic);
            if (navigation) m_boutonsRail.Add(Tuple.Create(g, barre, (FrameworkElement)b, couleur));
            return g;
        }

        void MajRail()
        {
            for (var i = 0; i < m_boutonsRail.Count; i++)
            {
                var on = (i == 1) == m_vuePartages; // 0 = Comptes, 1 = Partages
                var x = m_boutonsRail[i];
                x.Item2.Visibility = on ? Visibility.Visible : Visibility.Hidden;
                ((SolidColorBrush)((Border)x.Item3).Background).Color = on ? A(0x33, Accent) : Colors.Transparent;
                x.Item4.Color = on ? Accent2 : Doux;
            }
        }

        // ---------------------------------------------------------------- barre du haut

        FrameworkElement ConstruireHaut()
        {
            var g = new Grid { Margin = new Thickness(20, 14, 20, 14) };
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // recherche (filtre les cartes)
            var champ = new TextBox { MaxLength = 40 };
            var cadreRecherche = Theme.Champ(champ, 42);
            cadreRecherche.MaxWidth = 420;
            cadreRecherche.HorizontalAlignment = HorizontalAlignment.Left;
            cadreRecherche.Width = 420;
            var interieur = new Grid();
            cadreRecherche.Child = interieur;
            var loupe = Ic(IcRecherche, 16, B(Doux), false, 2.2);
            loupe.HorizontalAlignment = HorizontalAlignment.Left;
            loupe.Margin = new Thickness(14, 0, 0, 0);
            interieur.Children.Add(loupe);
            champ.Margin = new Thickness(30, 0, 0, 0);
            champ.FontSize = 14;
            interieur.Children.Add(champ);
            var indice = T("Rechercher un compte ou un personnage…", 14, Doux, FontWeights.Normal);
            indice.Margin = new Thickness(42, 0, 0, 0);
            indice.IsHitTestVisible = false;
            interieur.Children.Add(indice);
            champ.TextChanged += (s, e) =>
            {
                indice.Visibility = champ.Text.Length == 0 ? Visibility.Visible : Visibility.Collapsed;
                m_recherche = champ.Text.Trim().ToLowerInvariant();
                if (m_vuePartages) m_vuePartages = false;
                Afficher();
            };
            champ.KeyDown += (s, e) => { if (e.Key == Key.Escape) champ.Text = ""; };
            g.Children.Add(cadreRecherche);

            var droite = new StackPanel { Orientation = Orientation.Horizontal };
            var discord = BoutonDiscord(() => Outils.OuvrirSite(m_lienDiscord), 38);
            discord.Margin = new Thickness(0, 0, 10, 0);
            droite.Children.Add(discord);
            m_point = new Ellipse { Width = 8, Height = 8, VerticalAlignment = VerticalAlignment.Center };
            Theme.PointStatut(m_point, null);
            m_statutServeur = T("Serveur…", 13, Texte, FontWeights.SemiBold);
            m_statutServeur.Margin = new Thickness(8, 0, 0, 0);
            var serveur = new StackPanel { Orientation = Orientation.Horizontal };
            serveur.Children.Add(m_point);
            serveur.Children.Add(m_statutServeur);
            droite.Children.Add(Puce(serveur));
            var profil = new StackPanel { Orientation = Orientation.Horizontal };
            profil.Children.Add(new Img { Source = Theme.Image("p_icone_or.png"), Width = 20, Height = 20 });
            var pseudo = T(string.IsNullOrEmpty(m_compte.Pseudo) ? m_compte.Email ?? "" : m_compte.Pseudo, 13, Texte, FontWeights.SemiBold);
            pseudo.Margin = new Thickness(8, 0, 0, 0);
            profil.Children.Add(pseudo);
            var puceProfil = Puce(profil);
            puceProfil.Margin = new Thickness(10, 0, 0, 0);
            puceProfil.ToolTip = m_compte.Email;
            puceProfil.Cursor = Cursors.Hand;
            Clic(puceProfil, () => Outils.OuvrirSite(m_updater.SiteUrl + "compte.html"));
            droite.Children.Add(puceProfil);
            Grid.SetColumn(droite, 1);
            g.Children.Add(droite);
            return g;
        }

        // ---------------------------------------------------------------- colonne Dossiers

        FrameworkElement ConstruireDossiers()
        {
            var pile = new StackPanel();
            pile.Children.Add(m_listeDossiers);
            var ajout = new Grid { Height = 40, Margin = new Thickness(0, 8, 0, 0), Cursor = Cursors.Hand, Background = Brushes.Transparent };
            var pointilles = new Rectangle { RadiusX = 12, RadiusY = 12, Stroke = B(Ligne), StrokeThickness = 1.2, StrokeDashArray = new DoubleCollection { 4, 3 } };
            ajout.Children.Add(pointilles);
            var contenu = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(12, 0, 0, 0) };
            var plus = B(Doux);
            contenu.Children.Add(Ic(IcPlus, 16, plus, false, 2.2));
            var tAjout = T("Nouveau dossier", 13, Doux, FontWeights.SemiBold);
            tAjout.Margin = new Thickness(8, 0, 0, 0);
            contenu.Children.Add(tAjout);
            ajout.Children.Add(contenu);
            ajout.MouseEnter += (s, e) => { pointilles.Stroke = B(Accent); tAjout.Foreground = B(Texte); plus.Color = Texte; };
            ajout.MouseLeave += (s, e) => { pointilles.Stroke = B(Ligne); tAjout.Foreground = B(Doux); plus.Color = Doux; };
            Clic(ajout, NouveauDossier);
            pile.Children.Add(ajout);
            var aide = Para("Glisse un compte sur un dossier pour le ranger : les comptes cochés se déplacent ensemble. Double-clique sur un dossier pour le renommer, clic droit pour plus d'options.", 12, Doux);
            aide.Margin = new Thickness(2, 12, 2, 0);
            pile.Children.Add(aide);
            var defile = new ScrollViewer { Content = pile, VerticalScrollBarVisibility = ScrollBarVisibility.Hidden };
            var titre = T("DOSSIERS", 11.5, Doux, FontWeights.Bold);
            titre.Margin = new Thickness(0, 0, 0, 10);
            var g = new Grid();
            g.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            g.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            g.Children.Add(titre);
            Grid.SetRow(defile, 1);
            g.Children.Add(defile);
            return new Border { CornerRadius = new CornerRadius(18), BorderBrush = B(Ligne), BorderThickness = new Thickness(1), Background = B(Carte), Padding = new Thickness(14), Child = g };
        }

        void DessinerDossiers()
        {
            m_listeDossiers.Children.Clear();
            m_listeDossiers.Children.Add(ElementDossier(null, 0));
            for (var i = 0; i < m_dossiers.Liste.Count; i++)
                m_listeDossiers.Children.Add(ElementDossier(m_dossiers.Liste[i], i));
        }

        Color CouleurDossier(Dossiers.Dossier d) => d == null ? Accent2 : CouleursDossiers[Math.Max(0, m_dossiers.Liste.IndexOf(d)) % CouleursDossiers.Length];

        FrameworkElement ElementDossier(Dossiers.Dossier d, int index)
        {
            var actif = m_dossierActif == d && !m_vuePartages;
            var normal = actif ? A(0x2E, Accent) : Colors.Transparent;
            var fond = B(normal);
            var bord = B(Colors.Transparent);
            var echelle = new ScaleTransform(1, 1);
            var b = new Border
            {
                CornerRadius = new CornerRadius(12), Padding = new Thickness(8), Background = fond, BorderBrush = bord, BorderThickness = new Thickness(1.5), AllowDrop = true,
                Cursor = Cursors.Hand, Margin = new Thickness(0, 0, 0, 4), RenderTransformOrigin = new Point(0.5, 0.5), RenderTransform = echelle
            };
            var g = new Grid();
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(26) });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            b.Child = g;
            var couleur = CouleurDossier(d);
            var iconeDossier = d?.Icone == null ? null : Icones.Vue(m_updater.AppDir, d.Icone, 20);
            g.Children.Add(new Border { Width = 26, Height = 26, CornerRadius = new CornerRadius(8), Background = B(A(0x22, couleur)), Child = iconeDossier ?? Ic(IcDossier, 15, B(couleur), true) });
            var nb = d == null ? m_compte.Comptes.Count : ComptesDe(d).Count();
            if (d != null && m_renommerDossier == d)
            {
                var champ = ChampEnLigne(d.Nom, 24, nom =>
                {
                    m_renommerDossier = null;
                    if (!string.IsNullOrEmpty(nom)) { d.Nom = nom; m_dossiers.Sauver(); }
                    Afficher();
                    if (m_dossierACreer == d) { m_dossierACreer = null; ChoisirIconeDossier(d); }
                });
                champ.Margin = new Thickness(9, 0, 0, 0);
                Grid.SetColumn(champ, 1);
                Grid.SetColumnSpan(champ, 2);
                g.Children.Add(champ);
                return b;
            }
            var nom2 = T(d == null ? "Tous les comptes" : d.Nom, 14, Texte, FontWeights.SemiBold);
            nom2.Margin = new Thickness(9, 0, 6, 0);
            Grid.SetColumn(nom2, 1);
            g.Children.Add(nom2);
            var compte = new Border { CornerRadius = new CornerRadius(999), Background = B(Fond2), Padding = new Thickness(8, 1, 8, 2), Child = T(nb.ToString(), 12, Doux, FontWeights.SemiBold), VerticalAlignment = VerticalAlignment.Center };
            Grid.SetColumn(compte, 2);
            g.Children.Add(compte);

            b.MouseEnter += (s, e) => { if (!actif) fond.Color = Carte; };
            b.MouseLeave += (s, e) => fond.Color = normal;
            var appui = false;
            b.MouseLeave += (s, e) => appui = false;
            b.MouseLeftButtonDown += (s, e) =>
            {
                appui = true;
                if (e.ClickCount == 2 && d != null) { e.Handled = true; m_renommerDossier = d; Afficher(); }
            };
            b.MouseLeftButtonUp += (s, e) =>
            {
                if (!appui || m_renommerDossier != null) return;
                appui = false;
                m_dossierActif = d;
                m_vuePartages = false;
                Afficher();
            };
            b.ToolTip = d == null ? "Tous les comptes (dépose un compte ici pour le sortir de son dossier)" : null;
            b.DragOver += (s, e) =>
            {
                var ok = e.Data.GetData(typeof(CompteJeu)) is CompteJeu;
                e.Effects = ok ? DragDropEffects.Move : DragDropEffects.None;
                if (ok) { bord.Color = Accent2; fond.Color = A(0x24, Accent2); echelle.ScaleX = echelle.ScaleY = 1.03; }
                e.Handled = true;
            };
            b.DragLeave += (s, e) => { bord.Color = Colors.Transparent; fond.Color = normal; echelle.ScaleX = echelle.ScaleY = 1; };
            b.Drop += (s, e) =>
            {
                bord.Color = Colors.Transparent;
                if (e.Data.GetData(typeof(CompteJeu)) is CompteJeu c) Dispatcher.BeginInvoke((Action)(() => RangerGlisse(c, d)));
            };
            if (d != null)
            {
                b.ContextMenu = new ContextMenu();
                b.ContextMenuOpening += (s, e) => { e.Handled = true; MenuDossier(d, b); };
            }
            return b;
        }

        /// <summary>Champ de saisie dans la liste (renommer) : Entrée / perte du focus = valider, Échap = annuler (null).</summary>
        FrameworkElement ChampEnLigne(string valeur, int max, Action<string> fin)
        {
            var tb = new TextBox
            {
                Text = valeur ?? "", MaxLength = max, BorderThickness = new Thickness(0), Background = Brushes.Transparent, Foreground = B(Texte), FontFamily = Police, FontSize = 14,
                FontWeight = FontWeights.SemiBold, CaretBrush = B(Accent2), SelectionBrush = B(Accent), VerticalContentAlignment = VerticalAlignment.Center, Padding = new Thickness(6, 0, 6, 0)
            };
            var cadre = new Border { Height = 30, CornerRadius = new CornerRadius(8), BorderBrush = B(Accent2), BorderThickness = new Thickness(1.5), Background = B(Fond2), Child = tb };
            var fini = false;
            Action<string> terminer = v => { if (fini) return; fini = true; fin(v); };
            tb.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Enter) { e.Handled = true; terminer(tb.Text.Trim()); }
                else if (e.Key == Key.Escape) { e.Handled = true; terminer(null); }
            };
            tb.LostKeyboardFocus += (s, e) => Dispatcher.BeginInvoke((Action)(() => terminer(tb.Text.Trim())));
            tb.Loaded += (s, e) => { tb.Focus(); tb.SelectAll(); };
            cadre.MouseLeftButtonDown += (s, e) => e.Handled = true;
            cadre.MouseLeftButtonUp += (s, e) => e.Handled = true;
            return cadre;
        }

        void MenuDossier(Dossiers.Dossier d, FrameworkElement cible)
        {
            var m = Menu();
            m.Items.Add(Element(IcJouer, "Jouer le dossier", () => JouerListe(ComptesDe(d).ToList())));
            m.Items.Add(Element(IcCrayon, "Renommer", () => { m_renommerDossier = d; Afficher(); }));
            var icMenu = Element(null, "Icône du dossier", null);
            icMenu.Icon = d.Icone != null ? Icones.Vue(m_updater.AppDir, d.Icone, 18) : Ic(IcDossier, 14, B(Doux));
            AjouterIcones(icMenu, cle => d.Icone == cle, cle => { d.Icone = cle; m_dossiers.Sauver(); Afficher(); });
            if (d.Icone != null)
            {
                icMenu.Items.Add(new Separator());
                icMenu.Items.Add(Element(IcFermer, "Icône de dossier normale", () => { d.Icone = null; m_dossiers.Sauver(); Afficher(); }));
            }
            m.Items.Add(icMenu);
            m.Items.Add(Element(IcFermer, "Supprimer le dossier (les comptes restent)", () =>
            {
                m_dossiers.Liste.Remove(d);
                if (m_dossierActif == d) m_dossierActif = null;
                m_dossiers.Sauver();
                Afficher();
            }));
            m.PlacementTarget = cible;
            m.Placement = PlacementMode.Bottom;
            m.IsOpen = true;
        }

        Dossiers.Dossier m_dossierACreer;

        /// <summary>PADGREF 21/10 : à la création d'un dossier, menu des icônes officielles sous la souris (Échap = garder le dossier).</summary>
        void ChoisirIconeDossier(Dossiers.Dossier d)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                var m = Menu();
                var titre = Element(null, "Icône du dossier « " + d.Nom + " »", null);
                titre.IsEnabled = false;
                m.Items.Add(titre);
                m.Items.Add(new Separator());
                AjouterIcones(m, cle => d.Icone == cle, cle => { d.Icone = cle; m_dossiers.Sauver(); Afficher(); });
                m.Placement = PlacementMode.MousePoint;
                m.IsOpen = true;
            }), DispatcherPriority.Background);
        }

        void NouveauDossier()
        {
            var nom = "Nouveau dossier";
            for (var i = 2; m_dossiers.Liste.Any(x => string.Equals(x.Nom, nom, StringComparison.OrdinalIgnoreCase)); i++) nom = "Nouveau dossier " + i;
            var d = new Dossiers.Dossier { Nom = nom };
            m_dossiers.Liste.Add(d);
            // les comptes cochés vont dans le nouveau dossier
            var coches = m_compte.Comptes.Where(c => m_coches.Contains(c.Id)).ToList();
            foreach (var c in coches) m_dossiers.Ranger(c.Login, d);
            m_dossiers.Sauver();
            m_renommerDossier = d;
            m_dossierACreer = d;      // PADGREF 21/10 : le nom validé, on propose l'icône
            Afficher();
            if (coches.Count > 0) Toast(coches.Count + " compte" + (coches.Count > 1 ? "s cochés rangés" : " coché rangé") + " dans le nouveau dossier");
        }

        /// <summary>Dépôt d'un compte sur un dossier (null = sortir du dossier) : les comptes cochés suivent si celui-ci l'est.</summary>
        void RangerGlisse(CompteJeu c, Dossiers.Dossier d)
        {
            var groupe = m_coches.Contains(c.Id) ? m_compte.Comptes.Where(x => m_coches.Contains(x.Id)).ToList() : new List<CompteJeu> { c };
            foreach (var x in groupe) m_dossiers.Ranger(x.Login, d);
            Afficher();
            if (d == null) Toast(groupe.Count > 1 ? groupe.Count + " comptes sortis de leur dossier" : "« " + m_dossiers.Nom(c) + " » sorti de son dossier");
            else Toast(groupe.Count > 1 ? groupe.Count + " comptes rangés dans « " + d.Nom + " »" : "« " + m_dossiers.Nom(c) + " » rangé dans « " + d.Nom + " »");
        }

        IEnumerable<CompteJeu> ComptesDe(Dossiers.Dossier d) =>
            d == null ? m_compte.Comptes.Where(c => m_dossiers.DossierDe(c.Login) == null)
                      : m_compte.Comptes.Where(c => d.Comptes.Contains(c.Login, StringComparer.OrdinalIgnoreCase));

        List<CompteJeu> Visibles()
        {
            var l = m_dossierActif == null ? m_compte.Comptes : ComptesDe(m_dossierActif);
            if (m_recherche.Length > 0)
                l = l.Where(c => m_dossiers.Nom(c).ToLowerInvariant().Contains(m_recherche) || (c.Login ?? "").ToLowerInvariant().Contains(m_recherche)
                                 || (c.Pseudo ?? "").ToLowerInvariant().Contains(m_recherche) || c.Personnages.Any(p => (p.Nom ?? "").ToLowerInvariant().Contains(m_recherche)));
            return l.ToList();
        }

        // ---------------------------------------------------------------- affichage

        void Afficher()
        {
            if (m_dossierActif != null && !m_dossiers.Liste.Contains(m_dossierActif)) m_dossierActif = null;
            m_signature = Signature();
            MajRail();
            DessinerDossiers();
            var defilement = m_defile != null ? m_defile.VerticalOffset : 0;
            m_centre.Children.Clear();
            m_centre.Children.Add(m_vuePartages ? VuePartages() : VueComptes());
            if (m_defile != null && defilement > 0)
            {
                var d = m_defile;
                Dispatcher.BeginInvoke(DispatcherPriority.Loaded, (Action)(() => d.ScrollToVerticalOffset(defilement)));
            }
            MajDock();
            MajBouton();
        }

        FrameworkElement VueComptes()
        {
            m_defile = null;
            var vue = new Grid();
            vue.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            vue.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            vue.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            var entete = new Grid();
            var titres = new StackPanel();
            titres.Children.Add(T(m_dossierActif == null ? "Tous les comptes" : m_dossierActif.Nom, 26, Texte, FontWeights.Bold, Titre));
            var sous = new TextBlock { FontFamily = Police, FontSize = 13, Foreground = B(Doux), Margin = new Thickness(0, 4, 0, 0) };
            sous.Inlines.Add(new System.Windows.Documents.Run(m_coches.Count.ToString()) { Foreground = B(Texte), FontWeight = FontWeights.Bold });
            sous.Inlines.Add(" / " + MAX_COCHES + " cochés · double-clique sur un nom pour le renommer");
            titres.Children.Add(sous);
            entete.Children.Add(titres);
            var actualiser = SecondIcone(IcActualiser, null, Actualiser, 36);
            actualiser.Padding = new Thickness(10, 0, 10, 0);
            actualiser.ToolTip = "Actualiser la liste des comptes";
            actualiser.HorizontalAlignment = HorizontalAlignment.Right;
            actualiser.VerticalAlignment = VerticalAlignment.Top;
            entete.Children.Add(actualiser);
            vue.Children.Add(entete);

            var actions = new WrapPanel { Margin = new Thickness(0, 12, 0, 12) };
            Action<Border> ajouter = b => { b.Margin = new Thickness(0, 0, 8, 0); actions.Children.Add(b); };
            ajouter(SecondTexte("Tout cocher", ToutCocher, 36));
            ajouter(SecondTexte("Décocher", () => { foreach (var c in Visibles()) m_coches.Remove(c.Id); Afficher(); }, 36));
            var mesCoches = m_compte.Comptes.Where(c => m_coches.Contains(c.Id) && c.PartagePar == null).ToList();
            if (mesCoches.Count > 0) ajouter(SecondTexte("Partager (" + mesCoches.Count + ")", () => Partager(mesCoches), 36));
            ajouter(SecondTexte("Code partagé", AjouterComptePartage, 36));
            ajouter(SecondTexte("+ Créer un compte", CreerCompteJeu, 36));
            Grid.SetRow(actions, 1);
            vue.Children.Add(actions);

            var l = Visibles();
            FrameworkElement contenu;
            if (l.Count == 0)
            {
                var vide = new Border
                {
                    CornerRadius = new CornerRadius(16), BorderBrush = B(Ligne), BorderThickness = new Thickness(1.5), Padding = new Thickness(20, 40, 20, 40), VerticalAlignment = VerticalAlignment.Top,
                    Child = Para(m_recherche.Length > 0 ? "Aucun compte ne correspond à « " + m_recherche + " »."
                        : m_compte.Comptes.Count == 0 ? "Aucun compte de jeu pour l'instant : clique sur « + Créer un compte » (aucun mot de passe à retenir) ou ajoute un compte partagé avec « Code partagé »."
                        : "Ce dossier est vide." + Environment.NewLine + "Glisse des comptes ici depuis « Tous les comptes ».", 14, Doux)
                };
                ((TextBlock)vide.Child).TextAlignment = TextAlignment.Center;
                contenu = vide;
            }
            else
            {
                var grille = new UniformGrid { Columns = 3, Margin = new Thickness(-6, -6, -6, 0), VerticalAlignment = VerticalAlignment.Top };
                for (var i = 0; i < l.Count; i++) grille.Children.Add(CarteCompte(l[i]));
                m_defile = new ScrollViewer { Content = grille, VerticalScrollBarVisibility = ScrollBarVisibility.Hidden };
                contenu = m_defile;
            }
            Grid.SetRow(contenu, 2);
            vue.Children.Add(contenu);
            return vue;
        }

        void ToutCocher()
        {
            var reste = false;
            foreach (var c in Visibles())
            {
                if (m_coches.Contains(c.Id)) continue;
                if (m_coches.Count >= MAX_COCHES) { reste = true; break; }
                m_coches.Add(c.Id);
            }
            Afficher();
            if (reste) Toast(MAX_COCHES + " comptes maximum : les autres restent décochés");
        }

        void Basculer(CompteJeu c)
        {
            if (!m_coches.Contains(c.Id) && m_coches.Count >= MAX_COCHES) { Toast(MAX_COCHES + " comptes maximum en même temps"); return; }
            if (!m_coches.Remove(c.Id)) m_coches.Add(c.Id);
            Afficher();
        }

        bool EnJeu(CompteJeu c) => m_enJeu.TryGetValue(c.Id, out var p) && !p.HasExited;
        bool EnConnexion(CompteJeu c) => m_lancement.TryGetValue(c.Id, out var t) && (DateTime.Now - t).TotalSeconds < 12;

        static List<Personnage> Persos(CompteJeu c) => c.Personnages.OrderBy(p => p.Depuis < 0 ? long.MaxValue : p.Depuis).ToList();

        /// <summary>Tête du dernier personnage joué : rendu officiel (apparence sans dragodinde donnée par l'API), sinon tête de classe, sinon icône.</summary>
        ImageSource Avatar(CompteJeu c, Action<ImageSource> arrivee)
        {
            var p = Persos(c).FirstOrDefault();
            if (p == null) return IconeCompte(c.Id);
            if (!string.IsNullOrEmpty(p.Apparence) && m_avatars.TryGetValue(p.Apparence, out var deja)) return deja;
            if (!string.IsNullOrEmpty(p.Apparence) && arrivee != null)
            {
                var cle = p.Apparence;
                Outils.ChargerRendu(Dispatcher, cle, x => { m_avatars[cle] = x; arrivee(x); }, "face/2/100_100-10");
            }
            return Outils.TeteDeClasse(m_updater.AppDir, p.Classe, p.Sexe) ?? IconeCompte(c.Id);
        }

        FrameworkElement CarteCompte(CompteJeu c)
        {
            var coche = m_coches.Contains(c.Id);
            var enJeu = EnJeu(c);
            var connexion = EnConnexion(c);
            var deplace = new TranslateTransform();
            var halo = new DropShadowEffect { Color = Accent, BlurRadius = 30, ShadowDepth = 12, Direction = 270, Opacity = 0 };
            var carte = new Border
            {
                CornerRadius = new CornerRadius(16), BorderThickness = new Thickness(1.5), BorderBrush = B(coche ? Accent2 : Ligne), Background = B(coche ? Melange(Accent, 0.16, Fond2) : Carte),
                Margin = new Thickness(6), Padding = new Thickness(12, 14, 12, 12), Cursor = Cursors.Hand, RenderTransform = deplace, Effect = halo
            };
            var g = new Grid();
            carte.Child = g;
            var pile = new StackPanel { HorizontalAlignment = HorizontalAlignment.Stretch };
            g.Children.Add(pile);

            var img = new Img { Width = 60, Height = 60, Stretch = Stretch.Uniform, Effect = new DropShadowEffect { Color = Colors.Black, BlurRadius = 14, ShadowDepth = 6, Direction = 270, Opacity = 0.4 } };
            img.Source = Avatar(c, x => img.Source = x);
            pile.Children.Add(img);

            if (m_renommerCompte == c.Id)
            {
                var champ = ChampEnLigne(m_dossiers.Nom(c), 24, nom =>
                {
                    m_renommerCompte = -1;
                    if (nom != null)
                    {
                        if (nom.Length == 0 || nom == c.Login) { nom = ""; m_dossiers.Alias.Remove(c.Login.ToLowerInvariant()); }
                        else m_dossiers.Alias[c.Login.ToLowerInvariant()] = nom;
                        m_dossiers.Sauver();
                        if (nom.Length > 0) Toast("Compte renommé en « " + nom + " »");
                    }
                    Afficher();
                });
                ((TextBox)((Border)champ).Child).TextAlignment = TextAlignment.Center;
                champ.Margin = new Thickness(0, 6, 0, 0);
                pile.Children.Add(champ);
            }
            else
            {
                var nom = T(m_dossiers.Nom(c), 15, Texte, FontWeights.Bold);
                nom.HorizontalAlignment = HorizontalAlignment.Center;
                nom.Margin = new Thickness(0, 6, 0, 0);
                nom.ToolTip = "Double-clique pour renommer" + (m_dossiers.Nom(c) != c.Login ? " (compte « " + c.Login + " »)" : "");
                nom.MouseLeftButtonDown += (s, e) => { if (e.ClickCount == 2) { e.Handled = true; m_renommerCompte = c.Id; Afficher(); } };
                pile.Children.Add(nom);
            }

            var persos = Persos(c);
            var p0 = persos.FirstOrDefault();
            var ligne1 = T(p0 == null ? "Aucun personnage" : p0.Nom + " · " + Outils.Classe(p0.Classe) + " " + p0.Niveau, 12, Doux, FontWeights.Normal);
            ligne1.HorizontalAlignment = HorizontalAlignment.Center;
            ligne1.Margin = new Thickness(0, 3, 0, 0);
            ligne1.ToolTip = persos.Count == 0 ? null : string.Join("\n", persos.Select(p => p.Nom + "  Niv. " + p.Niveau + "  ·  " + Outils.Classe(p.Classe)));
            pile.Children.Add(ligne1);
            var ligne2 = c.PartagePar != null
                ? T("Partagé par " + c.PartagePar, 12, Accent2, FontWeights.SemiBold)
                : T(persos.Count > 1 ? "+" + (persos.Count - 1) + " autre" + (persos.Count > 2 ? "s" : "") + " personnage" + (persos.Count > 2 ? "s" : "") : "Lié · sans mot de passe", 12, A(0xB0, Doux), FontWeights.Normal);
            ligne2.HorizontalAlignment = HorizontalAlignment.Center;
            ligne2.Margin = new Thickness(0, 2, 0, 0);
            pile.Children.Add(ligne2);

            var etat = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 6, 0, 0) };
            var couleurEtat = enJeu && !connexion ? Vert : connexion ? Accent2 : Doux;
            if (enJeu || connexion) etat.Children.Add(new Ellipse { Width = 7, Height = 7, Fill = B(couleurEtat), Margin = new Thickness(0, 0, 5, 0), VerticalAlignment = VerticalAlignment.Center });
            etat.Children.Add(T(enJeu && !connexion ? "En jeu" : connexion ? "Connexion…" : "Prêt" + (p0 != null && p0.Depuis >= 0 ? " · " + Outils.IlYA(p0.Depuis) : ""), 11.5, couleurEtat, FontWeights.Bold));
            pile.Children.Add(etat);

            // PADGREF 21/10 : tags du compte (icônes officielles : classes, épées…)
            var tags = m_dossiers.TagsDe(c.Login);
            if (tags.Count > 0)
            {
                var rangee = new WrapPanel { HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 7, 0, 0) };
                foreach (var t in tags)
                {
                    var v = Icones.Vue(m_updater.AppDir, t, 20);
                    if (v == null) continue;
                    v.Margin = new Thickness(2, 0, 2, 0);
                    rangee.Children.Add(v);
                }
                pile.Children.Add(rangee);
            }

            var caseCoche = Case(coche);
            caseCoche.HorizontalAlignment = HorizontalAlignment.Right;
            caseCoche.VerticalAlignment = VerticalAlignment.Top;
            caseCoche.Margin = new Thickness(0, -5, -3, 0);
            g.Children.Add(caseCoche);

            var trait = B(Doux);
            var fondCrayon = B(Colors.Transparent);
            var crayon = new Border
            {
                Width = 26, Height = 26, CornerRadius = new CornerRadius(8), Background = fondCrayon, Child = Ic(IcCrayon, 14, trait, false, 2.2), HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top, Margin = new Thickness(-5, -7, 0, 0), Opacity = 0, ToolTip = "Renommer"
            };
            crayon.MouseEnter += (s, e) => { fondCrayon.Color = Fond2; trait.Color = Texte; };
            crayon.MouseLeave += (s, e) => { fondCrayon.Color = Colors.Transparent; trait.Color = Doux; };
            crayon.MouseLeftButtonDown += (s, e) => { e.Handled = true; m_renommerCompte = c.Id; Afficher(); };
            g.Children.Add(crayon);

            var ease = new PowerEase { Power = 3, EasingMode = EasingMode.EaseOut };
            carte.MouseEnter += (s, e) =>
            {
                crayon.Opacity = 1;
                deplace.BeginAnimation(TranslateTransform.YProperty, new DoubleAnimation(-3, TimeSpan.FromSeconds(0.25)) { EasingFunction = ease });
                halo.BeginAnimation(DropShadowEffect.OpacityProperty, new DoubleAnimation(0.5, TimeSpan.FromSeconds(0.25)));
            };
            carte.MouseLeave += (s, e) =>
            {
                crayon.Opacity = 0;
                deplace.BeginAnimation(TranslateTransform.YProperty, new DoubleAnimation(0, TimeSpan.FromSeconds(0.25)) { EasingFunction = ease });
                halo.BeginAnimation(DropShadowEffect.OpacityProperty, new DoubleAnimation(0, TimeSpan.FromSeconds(0.25)));
            };

            // clic = cocher / décocher, double-clic = jouer, glisser = ranger dans un dossier
            Point? depart = null;
            carte.MouseLeftButtonDown += (s, e) =>
            {
                if (m_renommerCompte == c.Id) return;
                if (e.ClickCount == 2) { depart = null; e.Handled = true; Jouer(c); return; }
                depart = e.GetPosition(this);
            };
            carte.MouseLeftButtonUp += (s, e) =>
            {
                if (depart == null) return;
                depart = null;
                Basculer(c);
            };
            carte.MouseMove += (s, e) =>
            {
                if (depart == null || e.LeftButton != MouseButtonState.Pressed) return;
                var p = e.GetPosition(this);
                if (Math.Abs(p.X - depart.Value.X) + Math.Abs(p.Y - depart.Value.Y) < 6) return;
                depart = null;
                carte.Opacity = 0.35;
                DragDrop.DoDragDrop(carte, new DataObject(typeof(CompteJeu), c), DragDropEffects.Move);
                carte.Opacity = 1;
            };
            carte.ContextMenu = new ContextMenu();
            carte.ContextMenuOpening += (s, e) => { e.Handled = true; MenuCompte(c, carte); };
            return carte;
        }

        void MenuCompte(CompteJeu c, FrameworkElement cible)
        {
            var m = Menu();
            m.Items.Add(Element(IcJouer, "Jouer", () => Jouer(c)));
            m.Items.Add(Element(IcCrayon, "Renommer", () => { m_renommerCompte = c.Id; Afficher(); }));
            var tagsMenu = Element(null, "Tags", null);
            tagsMenu.Icon = Icones.Vue(m_updater.AppDir, "epees", 18);
            var actuels = m_dossiers.TagsDe(c.Login);
            AjouterIcones(tagsMenu, cle => actuels.Contains(cle), cle => { m_dossiers.BasculerTag(c.Login, cle); Afficher(); });
            if (actuels.Count > 0)
            {
                tagsMenu.Items.Add(new Separator());
                tagsMenu.Items.Add(Element(IcFermer, "Retirer tous les tags", () => { m_dossiers.Tags.Remove(c.Login.ToLowerInvariant()); m_dossiers.Sauver(); Afficher(); }));
            }
            m.Items.Add(tagsMenu);
            var deplacer = Element(IcDossier, "Ranger dans", null);
            foreach (var d in m_dossiers.Liste)
            {
                var cible2 = d;
                var el = Element(null, d.Nom, () => RangerGlisse(c, cible2));
                if (d.Icone != null) el.Icon = Icones.Vue(m_updater.AppDir, d.Icone, 18);
                deplacer.Items.Add(el);
            }
            if (m_dossiers.Liste.Count > 0) deplacer.Items.Add(new Separator());
            deplacer.Items.Add(Element(IcPlus, "Nouveau dossier…", () =>
            {
                var nom = Saisir("Nouveau dossier", "");
                if (nom == null) return;
                var d = m_dossiers.Liste.FirstOrDefault(x => string.Equals(x.Nom, nom, StringComparison.OrdinalIgnoreCase));
                var nouveau = d == null;
                if (d == null) { d = new Dossiers.Dossier { Nom = nom }; m_dossiers.Liste.Add(d); }
                RangerGlisse(c, d);
                if (nouveau) ChoisirIconeDossier(d);
            }));
            m.Items.Add(deplacer);
            if (m_dossiers.DossierDe(c.Login) != null)
                m.Items.Add(Element(IcDossierOuvrir, "Sortir du dossier", () => RangerGlisse(c, null)));
            m.Items.Add(new Separator());
            if (c.PartagePar == null)
                m.Items.Add(Element(IcPartages, "Partager ce compte…", () => Partager(new List<CompteJeu> { c })));
            else
                m.Items.Add(Element(IcFermer, "Retirer ce compte partagé", () => RetirerPartageRecu(c.Id)));
            m.PlacementTarget = cible;
            m.Placement = PlacementMode.MousePoint;
            m.IsOpen = true;
        }

        void Actualiser()
        {
            if (m_session == null) return;
            new Thread(() =>
            {
                var r = m_api.Session(m_session);
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    if (r.Ok) { r.Session = m_session; m_compte = r; Toast("Liste des comptes à jour"); }
                    else Toast(r.Erreur ?? "Actualisation impossible.", true);
                    Afficher();
                }));
            }) { IsBackground = true }.Start();
        }

        // ---------------------------------------------------------------- vue Partages

        FrameworkElement VuePartages()
        {
            var vue = new StackPanel();
            vue.Children.Add(T("Partages de comptes", 26, Texte, FontWeights.Bold, Titre));
            var intro = Para("Les comptes que tu partages peuvent être joués par l'autre joueur depuis son launcher (rien d'autre : ni mot de passe, ni e-mail, ni tes autres comptes). Tu peux retirer un accès à tout moment, et celui qui a reçu un compte peut aussi le retirer de son côté.", 13, Doux);
            intro.Margin = new Thickness(0, 6, 0, 14);
            vue.Children.Add(intro);
            var boutons = new WrapPanel { Margin = new Thickness(0, 0, 0, 16) };
            var b1 = SecondTexte("Partager des comptes…", () => { m_vuePartages = false; Afficher(); Toast("Coche les comptes à partager, puis clique sur « Partager »."); }, 36);
            b1.Margin = new Thickness(0, 0, 8, 0);
            boutons.Children.Add(b1);
            boutons.Children.Add(SecondTexte("Ajouter un compte partagé", AjouterComptePartage, 36));
            vue.Children.Add(boutons);

            var donnes = new StackPanel();
            var recus = new StackPanel();
            vue.Children.Add(Panneau(donnes, "Comptes que je partage"));
            var p2 = Panneau(recus, "Comptes partagés avec moi");
            p2.Margin = new Thickness(0, 12, 0, 0);
            vue.Children.Add(p2);
            donnes.Children.Add(T("Chargement…", 13, Doux, FontWeights.Normal));

            if (m_apercu != null)
                RemplirPartages(donnes, recus, Apercu.Partages());
            else
            {
                var session = m_session;
                new Thread(() =>
                {
                    var l = session != null ? m_api.PartageListe(session) : null;
                    Dispatcher.BeginInvoke((Action)(() => RemplirPartages(donnes, recus, l)));
                }) { IsBackground = true }.Start();
            }
            m_defile = null;
            return new ScrollViewer { Content = vue, VerticalScrollBarVisibility = ScrollBarVisibility.Hidden };
        }

        void RemplirPartages(StackPanel donnes, StackPanel recus, Tuple<List<Partage>, List<Partage>> l)
        {
            donnes.Children.Clear();
            recus.Children.Clear();
            if (l == null)
            {
                donnes.Children.Add(T("Impossible de charger les partages (serveur injoignable).", 13, Rouge, FontWeights.SemiBold));
                return;
            }
            if (l.Item1.Count == 0) donnes.Children.Add(T("Tu ne partages aucun compte.", 13, Doux, FontWeights.Normal));
            foreach (var p in l.Item1)
            {
                var x = p;
                donnes.Children.Add(LignePartage(x.Login, "partagé avec " + x.Pseudo, "depuis le " + x.Depuis, "Retirer l'accès", () => RetirerPartage(x.Compte, x.Receveur)));
            }
            if (l.Item2.Count == 0) recus.Children.Add(Para("Aucun compte partagé avec toi : clique sur « Ajouter un compte partagé » et saisis le code reçu.", 13, Doux));
            foreach (var p in l.Item2)
            {
                var x = p;
                recus.Children.Add(LignePartage(x.Login, "partagé par " + x.Pseudo, "depuis le " + x.Depuis, "Retirer", () => RetirerPartageRecu(x.Compte)));
            }
        }

        FrameworkElement LignePartage(string compte, string qui, string depuis, string action, Action clic)
        {
            var g = new Grid { Height = 46 };
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.1, GridUnitType.Star) });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.2, GridUnitType.Star) });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            g.Children.Add(T(compte, 14, Texte, FontWeights.SemiBold));
            var c2 = T(qui, 13, Accent2, FontWeights.Normal);
            Grid.SetColumn(c2, 1);
            g.Children.Add(c2);
            var c3 = T(depuis, 12.5, Doux, FontWeights.Normal);
            Grid.SetColumn(c3, 2);
            g.Children.Add(c3);
            var b = SecondTexte(action, clic, 32, 12.5);
            ((TextBlock)b.Child).Foreground = B(Rouge);
            Grid.SetColumn(b, 3);
            g.Children.Add(b);
            return new Border { Child = g, BorderBrush = B(Ligne), BorderThickness = new Thickness(0, 0, 0, 1) };
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

        // ---------------------------------------------------------------- fenêtres modales

        void Ouvrir(FrameworkElement contenu)
        {
            FermerTiroir();
            m_calque.Child = contenu;
            m_calque.Visibility = Visibility.Visible;
            m_calque.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.2)));
            var monte = new TranslateTransform(0, 14);
            contenu.RenderTransform = monte;
            monte.BeginAnimation(TranslateTransform.YProperty, new DoubleAnimation(14, 0, TimeSpan.FromSeconds(0.3)) { EasingFunction = new PowerEase { Power = 3, EasingMode = EasingMode.EaseOut } });
        }

        void FermerFenetre()
        {
            m_calque.Visibility = Visibility.Collapsed;
            m_calque.Child = null;
            Afficher();
        }

        Border Fenetre(string surtitre, string titre, out StackPanel pile, double largeur = 480)
        {
            pile = new StackPanel();
            var entete = new Grid { Margin = new Thickness(0, 0, 0, 14) };
            var tt = new StackPanel();
            tt.Children.Add(T(surtitre, 11.5, Accent2, FontWeights.Bold));
            var h = T(titre, 22, Texte, FontWeights.Bold, Titre);
            h.Margin = new Thickness(0, 3, 0, 0);
            tt.Children.Add(h);
            entete.Children.Add(tt);
            var fermer = BoutonFenetre(IcFermer, false, FermerFenetre);
            fermer.HorizontalAlignment = HorizontalAlignment.Right;
            fermer.VerticalAlignment = VerticalAlignment.Top;
            entete.Children.Add(fermer);
            var tout = new StackPanel();
            tout.Children.Add(entete);
            tout.Children.Add(pile);
            return new Border
            {
                Width = largeur, Padding = new Thickness(24), CornerRadius = new CornerRadius(22), HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center,
                Background = B(Fond2), BorderBrush = B(Ligne), BorderThickness = new Thickness(1),
                Effect = new DropShadowEffect { Color = Colors.Black, BlurRadius = 80, ShadowDepth = 20, Direction = 270, Opacity = 0.6 }, Child = tout
            };
        }

        TextBlock Paragraphe(string t)
        {
            var p = Para(t, 13.5, Doux);
            p.Margin = new Thickness(0, 0, 0, 14);
            return p;
        }

        static Border Encadre(string icone, UIElement texte)
        {
            var g = new Grid();
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            var ic = Ic(icone, 22, B(Accent2));
            ic.VerticalAlignment = VerticalAlignment.Top;
            ic.Margin = new Thickness(0, 1, 12, 0);
            g.Children.Add(ic);
            Grid.SetColumn(texte, 1);
            g.Children.Add(texte);
            return new Border { Padding = new Thickness(14, 12, 14, 12), CornerRadius = new CornerRadius(12), Background = B(A(0x1F, Accent)), BorderBrush = B(Ligne), BorderThickness = new Thickness(1), Child = g };
        }

        /// <summary>« Code partagé » : saisie du code à 6 chiffres reçu.</summary>
        void AjouterComptePartage()
        {
            var fenetre = Fenetre("COMPTE PARTAGÉ", "Ajouter un compte", out var pile);
            pile.Children.Add(Paragraphe("Saisis le code à 6 chiffres que le propriétaire du compte t'a donné. Tu pourras jouer ce ou ces comptes de jeu depuis ton launcher."));
            var cases = new List<TextBox>();
            var ligne = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 0, 0, 16) };
            var erreur = T("", 13, Rouge, FontWeights.SemiBold);
            erreur.HorizontalAlignment = HorizontalAlignment.Center;
            Border valider = null;
            Action envoyer = null;
            for (var i = 0; i < 6; i++)
            {
                var tb = new TextBox
                {
                    Width = 48, Height = 60, Margin = new Thickness(5, 0, 5, 0), MaxLength = 1, FontFamily = Titre, FontSize = 26, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center,
                    VerticalContentAlignment = VerticalAlignment.Center, Foreground = B(Texte), Background = B(Carte), BorderBrush = B(Ligne), BorderThickness = new Thickness(1.5),
                    CaretBrush = B(Accent2), SelectionBrush = B(Accent)
                };
                var index = i;
                tb.GotKeyboardFocus += (s, e) => { tb.BorderBrush = B(Accent2); tb.SelectAll(); };
                tb.LostKeyboardFocus += (s, e) => tb.BorderBrush = B(tb.Text.Length > 0 ? Accent : Ligne);
                tb.PreviewTextInput += (s, e) => { if (!e.Text.All(char.IsDigit)) e.Handled = true; };
                tb.TextChanged += (s, e) =>
                {
                    if (tb.Text.Length == 1 && index < 5) cases[index + 1].Focus();
                    valider.Opacity = cases.All(x => x.Text.Length == 1) ? 1 : 0.5;
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
                erreur.Foreground = B(Doux);
                erreur.Text = "Vérification du code…";
                var session = m_session;
                new Thread(() =>
                {
                    var e = m_api.PartageUtiliser(session, code);
                    var r = e == null ? m_api.Session(session) : null;
                    Dispatcher.BeginInvoke((Action)(() =>
                    {
                        if (e != null) { erreur.Foreground = B(Rouge); erreur.Text = e; return; }
                        if (r != null && r.Ok) { r.Session = session; m_compte = r; }
                        m_vuePartages = false;
                        FermerFenetre();
                        Toast("Compte partagé ajouté : il apparaît dans « Tous les comptes ».");
                    }));
                }) { IsBackground = true }.Start();
            };
            valider = Gros("Ajouter le compte", null, () => envoyer(), 48);
            valider.Opacity = 0.5;
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
            if (comptes.Count == 0) { Toast("Tu ne peux partager que tes propres comptes.", true); return; }
            var titre = comptes.Count == 1 ? m_dossiers.Nom(comptes[0]) : comptes.Count + " comptes";
            var fenetre = Fenetre("PARTAGER " + (comptes.Count == 1 ? "UN COMPTE" : "DES COMPTES"), titre, out var pile, 500);
            if (comptes.Count > 1)
            {
                var liste = Para(string.Join("  ·  ", comptes.Select(c => m_dossiers.Nom(c))), 13, Accent2);
                liste.FontWeight = FontWeights.SemiBold;
                liste.Margin = new Thickness(0, -6, 0, 10);
                pile.Children.Add(liste);
            }
            pile.Children.Add(Paragraphe("Donne ce code au joueur : dans son launcher, il clique sur « Code partagé » et le saisit."));
            var chiffres = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 0, 0, 10) };
            pile.Children.Add(chiffres);
            var validite = T("Création du code…", 13, Doux, FontWeights.Normal);
            validite.HorizontalAlignment = HorizontalAlignment.Center;
            validite.Margin = new Thickness(0, 0, 0, 14);
            pile.Children.Add(validite);
            var boutons = new Grid { Margin = new Thickness(0, 0, 0, 14) };
            boutons.ColumnDefinitions.Add(new ColumnDefinition());
            boutons.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(10) });
            boutons.ColumnDefinitions.Add(new ColumnDefinition());
            string code = null;
            DispatcherTimer minuteur = null;
            var copier = Gros("Copier le code", null, () =>
            {
                if (code == null) return;
                try { Clipboard.SetText(code); Toast("Code copié : " + code); } catch { }
            }, 46, 14);
            var annuler = SecondTexte("Annuler le code", () =>
            {
                minuteur?.Stop();
                if (code != null && m_session != null) { var s = m_session; var c = code; new Thread(() => m_api.PartageAnnuler(s, c)) { IsBackground = true }.Start(); }
                FermerFenetre();
            }, 46, 14);
            boutons.Children.Add(copier);
            Grid.SetColumn(annuler, 2);
            boutons.Children.Add(annuler);
            pile.Children.Add(boutons);
            pile.Children.Add(Encadre(IcBouclier, Para("Il pourra jouer ce" + (comptes.Count > 1 ? "s comptes" : " compte") + " de jeu, rien d'autre : il n'a pas accès à ton compte Padgref (e-mail, mot de passe, autres comptes). Tu peux lui retirer l'accès à tout moment dans « Partages ».", 13, Doux)));
            Ouvrir(fenetre);

            Action<string, int> montrer = (c, secondes) =>
            {
                code = c;
                chiffres.Children.Clear();
                foreach (var ch in c)
                {
                    var t = T(ch.ToString(), 30, Texte, FontWeights.Bold, Titre);
                    t.HorizontalAlignment = HorizontalAlignment.Center;
                    chiffres.Children.Add(new Border { Width = 52, Height = 66, Margin = new Thickness(5, 0, 5, 0), CornerRadius = new CornerRadius(12), Background = B(Carte), BorderBrush = B(A(0x80, Accent)), BorderThickness = new Thickness(1.5), Child = t });
                }
                var fin = DateTime.Now.AddSeconds(secondes);
                minuteur = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
                Action maj = () =>
                {
                    var reste = fin - DateTime.Now;
                    if (reste <= TimeSpan.Zero) { minuteur.Stop(); validite.Text = "Code expiré : ferme et recrée-en un."; validite.Foreground = B(Rouge); return; }
                    validite.Inlines.Clear();
                    validite.Inlines.Add("Valable ");
                    validite.Inlines.Add(new System.Windows.Documents.Run(((int)reste.TotalMinutes).ToString("00") + ":" + reste.Seconds.ToString("00")) { Foreground = B(Accent2), FontWeight = FontWeights.Bold });
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
                    if (r.Item1 == null) { validite.Text = r.Item3; validite.Foreground = B(Rouge); return; }
                    montrer(r.Item1, r.Item2);
                }));
            }) { IsBackground = true }.Start();
        }

        /// <summary>MYSTIC 20/10 : crée un compte de jeu directement depuis le launcher (sans mot de passe, lié au compte web).</summary>
        void CreerCompteJeu()
        {
            var fenetre = Fenetre("COMPTE DE JEU", "Créer un compte de jeu", out var pile, 560);
            pile.Children.Add(Etiquette("Pseudo (affiché en jeu)"));
            var pseudo = new TextBox { MaxLength = 30 };
            var c1 = Champ(pseudo, 44);
            c1.Margin = new Thickness(0, 6, 0, 12);
            pile.Children.Add(c1);
            pile.Children.Add(Etiquette("Réponse secrète (demandée en jeu pour supprimer un personnage)"));
            var reponse = new TextBox { MaxLength = 60 };
            var c2 = Champ(reponse, 44);
            c2.Margin = new Thickness(0, 6, 0, 12);
            pile.Children.Add(c2);
            pile.Children.Add(Etiquette("Ranger dans le dossier"));
            var choix = new WrapPanel { Margin = new Thickness(0, 6, 0, 14) };
            var dossierChoisi = m_dossierActif;
            Action redessiner = null;
            redessiner = () =>
            {
                choix.Children.Clear();
                foreach (var d in new Dossiers.Dossier[] { null }.Concat(m_dossiers.Liste))
                {
                    var x = d;
                    var on = x == dossierChoisi;
                    var t = T(x == null ? "Aucun" : x.Nom, 13, on ? AccentTxt : Doux, FontWeights.SemiBold);
                    var puce = new Border
                    {
                        Child = t, Padding = new Thickness(12, 6, 12, 7), CornerRadius = new CornerRadius(10), Margin = new Thickness(0, 0, 6, 6), Cursor = Cursors.Hand,
                        Background = on ? (Brush)B(Accent) : Brushes.Transparent, BorderBrush = B(on ? Accent : Ligne), BorderThickness = new Thickness(1)
                    };
                    Clic(puce, () => { dossierChoisi = x; redessiner(); });
                    choix.Children.Add(puce);
                }
            };
            redessiner();
            pile.Children.Add(choix);
            var info = new TextBlock { FontFamily = Police, FontSize = 13.5, Foreground = B(Doux), TextWrapping = TextWrapping.Wrap, LineHeight = 20 };
            info.Inlines.Add(new System.Windows.Documents.Run("Aucun mot de passe à retenir. ") { Foreground = B(Texte), FontWeight = FontWeights.Bold });
            info.Inlines.Add("Le compte de jeu est lié à ton compte Padgref ");
            info.Inlines.Add(new System.Windows.Documents.Run(m_compte.Pseudo ?? m_compte.Email ?? "") { Foreground = B(Texte), FontWeight = FontWeights.Bold });
            info.Inlines.Add(" : il s'ouvre depuis le launcher, et personne ne peut s'y connecter sans ton compte web.");
            pile.Children.Add(Encadre(IcCadenas, info));
            var bas = new Grid { Margin = new Thickness(0, 16, 0, 0) };
            var nb = new TextBlock { FontFamily = Police, FontSize = 13, Foreground = B(Doux), VerticalAlignment = VerticalAlignment.Center };
            nb.Inlines.Add(new System.Windows.Documents.Run(m_compte.Comptes.Count(c => c.PartagePar == null).ToString()) { Foreground = B(Texte), FontWeight = FontWeights.Bold });
            nb.Inlines.Add(" compte(s) de jeu sur ton compte Padgref");
            bas.Children.Add(nb);
            var erreur = Para("", 13, Rouge);
            erreur.FontWeight = FontWeights.SemiBold;
            var occupe = false;
            Border creer = null;
            Action envoyer = () =>
            {
                if (occupe) return;
                var ps = pseudo.Text.Trim();
                var rep = reponse.Text.Trim();
                if (ps.Length == 0 || rep.Length == 0) { erreur.Text = "Indique un pseudo et une réponse secrète."; return; }
                if (m_session == null) { erreur.Text = "Aperçu : pas de serveur."; return; }
                occupe = true;
                creer.Opacity = 0.6;
                erreur.Foreground = B(Doux);
                erreur.Text = "Création du compte « " + ps + " »…";
                var session = m_session;
                var anciens = new HashSet<int>(m_compte.Comptes.Select(c => c.Id));
                var d = dossierChoisi;
                new Thread(() =>
                {
                    var r = m_api.CreerJeu(session, ps, rep);
                    Dispatcher.BeginInvoke((Action)(() =>
                    {
                        occupe = false;
                        creer.Opacity = 1;
                        if (!r.Ok) { erreur.Foreground = B(Rouge); erreur.Text = r.Erreur ?? "Création impossible."; return; }
                        r.Session = session;
                        m_compte = r;
                        var nouveau = r.Comptes.FirstOrDefault(c => !anciens.Contains(c.Id));
                        if (nouveau != null && d != null) m_dossiers.Ranger(nouveau.Login, d);
                        m_dossierActif = d;
                        m_statut.Text = "Compte « " + ps + " » créé : aucun mot de passe, il s'ouvre depuis le launcher.";
                        FermerFenetre();
                        Toast("Compte « " + ps + " » créé et lié à ton compte Padgref");
                    }));
                }) { IsBackground = true }.Start();
            };
            creer = Gros("Créer le compte", null, envoyer, 48, 15);
            creer.Padding = new Thickness(22, 0, 22, 0);
            creer.HorizontalAlignment = HorizontalAlignment.Right;
            bas.Children.Add(creer);
            pile.Children.Add(bas);
            erreur.Margin = new Thickness(0, 10, 0, 0);
            pile.Children.Add(erreur);
            pseudo.KeyDown += (s, e) => { if (e.Key == Key.Enter) reponse.Focus(); };
            reponse.KeyDown += (s, e) => { if (e.Key == Key.Enter) envoyer(); };
            Ouvrir(fenetre);
            fenetre.Loaded += (s, e) => pseudo.Focus();
        }

        /// <summary>« Fenêtres » : plan de placement des fenêtres de Dofus (jusqu'à 40 comptes).</summary>
        void OuvrirPlan()
        {
            var fenetre = Fenetre("MULTI-COMPTES", "Plan des fenêtres", out var pile, 660);
            pile.Children.Add(Paragraphe("Choisis comment ranger les fenêtres de Dofus (jusqu'à 40) : le launcher les place tout seul à chaque lancement."));
            const double l = 612, h = l * 9 / 16;
            var ecran = new Canvas { Width = l - 4, Height = h - 4, ClipToBounds = true };
            pile.Children.Add(new Border
            {
                Width = l, Height = h, CornerRadius = new CornerRadius(12), BorderBrush = B(Ligne), BorderThickness = new Thickness(2), Child = ecran,
                Background = new LinearGradientBrush(Fond, Fond2, new Point(0, 0), new Point(1, 1))
            });
            var dispos = new UniformGrid { Columns = 4, Margin = new Thickness(-5, 14, -5, 0) };
            pile.Children.Add(dispos);
            Action dessiner = null;
            dessiner = () =>
            {
                ecran.Children.Clear();
                var comptes = m_compte.Comptes.Where(c => m_coches.Contains(c.Id)).ToList();
                if (comptes.Count == 0) comptes = m_ordre.Distinct().Select(id => m_compte.Comptes.FirstOrDefault(c => c.Id == id)).Where(c => c != null && EnJeu(c)).ToList();
                if (comptes.Count == 0)
                {
                    var t = Para("Coche des comptes pour voir leur place.", 14, Doux);
                    Canvas.SetLeft(t, 20);
                    Canvas.SetTop(t, 20);
                    ecran.Children.Add(t);
                }
                var pos = PlacementFenetres.Positions(m_reglages.Disposition, Math.Min(MAX_COCHES, comptes.Count), 0.03, 0.02);
                for (var i = 0; i < pos.Count; i++)
                {
                    var r = pos[i];
                    var c = comptes[i];
                    var contenu = new StackPanel { VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                    contenu.Children.Add(new Img { Source = Avatar(c, null), Width = 30, Height = 30 });
                    var n = T(m_dossiers.Nom(c), 12, Texte, FontWeights.Bold);
                    n.HorizontalAlignment = HorizontalAlignment.Center;
                    contenu.Children.Add(n);
                    if (EnJeu(c))
                    {
                        var e = T("● En jeu", 11, Vert, FontWeights.SemiBold);
                        e.HorizontalAlignment = HorizontalAlignment.Center;
                        contenu.Children.Add(e);
                    }
                    var f = new Border
                    {
                        Width = r.Width * ecran.Width, Height = r.Height * ecran.Height, CornerRadius = new CornerRadius(8), BorderBrush = B(Accent2), BorderThickness = new Thickness(1.5),
                        Background = B(Melange(Accent, 0.22, Fond)), Child = contenu, Padding = new Thickness(4)
                    };
                    Canvas.SetLeft(f, r.X * ecran.Width);
                    Canvas.SetTop(f, r.Y * ecran.Height);
                    ecran.Children.Add(f);
                }
                dispos.Children.Clear();
                for (var i = 0; i < PlacementFenetres.Noms.Length; i++)
                {
                    var nom = PlacementFenetres.Noms[i];
                    var on = m_reglages.Disposition == nom;
                    var t = T(PlacementFenetres.Libelles[i], 13, on ? Accent2 : Texte, FontWeights.SemiBold);
                    t.HorizontalAlignment = HorizontalAlignment.Center;
                    var b = new Border { Height = 42, Margin = new Thickness(5, 0, 5, 0), CornerRadius = new CornerRadius(12), BorderBrush = B(on ? Accent2 : Ligne), BorderThickness = new Thickness(1.5), Background = B(Carte), Child = t, Cursor = Cursors.Hand };
                    Clic(b, () => { m_reglages.Disposition = nom; m_reglages.Sauver(); dessiner(); MajDock(); });
                    dispos.Children.Add(b);
                }
            };
            dessiner();
            var bas = new Grid { Margin = new Thickness(0, 16, 0, 0) };
            var auto = new StackPanel { Orientation = Orientation.Horizontal };
            auto.Children.Add(Bascule(m_reglages.Placer, on => { m_reglages.Placer = on; m_reglages.Sauver(); }));
            var ta = T("Placer automatiquement au lancement", 13.5, Texte, FontWeights.SemiBold);
            ta.Margin = new Thickness(10, 0, 0, 0);
            auto.Children.Add(ta);
            bas.Children.Add(auto);
            var boutons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            var placer = SecondIcone(IcFenetres, "Placer les fenêtres ouvertes", () => PlacerFenetres(true), 42);
            placer.Margin = new Thickness(0, 0, 8, 0);
            boutons.Children.Add(placer);
            boutons.Children.Add(SecondTexte("Fermer", FermerFenetre, 42));
            bas.Children.Add(boutons);
            pile.Children.Add(bas);
            Ouvrir(fenetre);
        }

        void PlacerFenetres(bool message)
        {
            var liste = m_ordre.Distinct().Where(id => m_enJeu.ContainsKey(id)).Select(id => m_enJeu[id]).ToList();
            var n = 0;
            try { n = PlacementFenetres.Placer(liste, m_reglages.Disposition); } catch (Exception ex) { m_updater.Log("Placement des fenêtres : " + ex.Message); }
            if (message) Toast(n == 0 ? "Aucune fenêtre de jeu ouverte par le launcher." : n + (n > 1 ? " fenêtres placées" : " fenêtre placée"));
        }

        /// <summary>Attend que la fenêtre du jeu existe puis replace toutes les fenêtres (deux fois : le jeu reprend sa taille au chargement).</summary>
        void SuivrePlacement(Process p)
        {
            if (!m_reglages.Placer || p == null) return;
            var essais = 0;
            var vue = 0;
            var t = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1.5) };
            t.Tick += (s, e) =>
            {
                essais++;
                try
                {
                    p.Refresh();
                    if (p.HasExited) { t.Stop(); return; }
                    if (p.MainWindowHandle != IntPtr.Zero) vue++;
                }
                catch { t.Stop(); return; }
                // PADGREF 24/10 : plus de placement automatique des fenêtres (les joueurs les rangent eux-mêmes)
                if (vue >= 6 || essais > 40) t.Stop();
            };
            t.Start();
        }

        // fenêtre de saisie (nouveau dossier depuis le menu)
        string Saisir(string titre, string valeur)
        {
            var f = new Window
            {
                Owner = this, Width = 400, Height = 190, WindowStyle = WindowStyle.None, AllowsTransparency = true, Background = Brushes.Transparent,
                WindowStartupLocation = WindowStartupLocation.CenterOwner, ResizeMode = ResizeMode.NoResize, ShowInTaskbar = false
            };
            var pile = new StackPanel { Margin = new Thickness(22) };
            pile.Children.Add(T(titre, 16, Texte, FontWeights.Bold, Titre));
            var champ = new TextBox { Text = valeur ?? "", MaxLength = 24 };
            var cadre = Champ(champ, 42);
            cadre.Margin = new Thickness(0, 12, 0, 14);
            pile.Children.Add(cadre);
            string resultat = null;
            var boutons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            boutons.Children.Add(SecondTexte("Annuler", () => f.Close(), 36));
            var ok = Gros("Valider", null, () => { resultat = champ.Text.Trim(); f.Close(); }, 36, 13.5);
            ok.Width = 110;
            ok.Margin = new Thickness(8, 0, 0, 0);
            boutons.Children.Add(ok);
            pile.Children.Add(boutons);
            champ.KeyDown += (s, e) => { if (e.Key == Key.Enter) { resultat = champ.Text.Trim(); f.Close(); } else if (e.Key == Key.Escape) f.Close(); };
            f.Content = new Border { CornerRadius = new CornerRadius(16), Background = B(Fond2), BorderBrush = B(Ligne), BorderThickness = new Thickness(1), Child = pile };
            f.Loaded += (s, e) => { champ.Focus(); champ.SelectAll(); };
            f.ShowDialog();
            return string.IsNullOrEmpty(resultat) ? null : resultat;
        }

        // ---------------------------------------------------------------- tiroir Paramètres

        FrameworkElement ConstruireTiroir()
        {
            m_tiroir = new Grid { Visibility = Visibility.Collapsed };
            Grid.SetRow(m_tiroir, 1);
            var voile = new Rectangle { Fill = B(Color.FromArgb(0x80, 3, 6, 14)) };
            voile.MouseLeftButtonDown += (s, e) => FermerTiroir();
            m_tiroir.Children.Add(voile);
            var pile = new StackPanel();
            m_panneauTiroir = new Border
            {
                Width = 390, HorizontalAlignment = HorizontalAlignment.Right, Background = B(Fond2), BorderBrush = B(Ligne), BorderThickness = new Thickness(1, 0, 0, 0), Padding = new Thickness(22),
                Child = new ScrollViewer { Content = pile, VerticalScrollBarVisibility = ScrollBarVisibility.Hidden }, RenderTransform = new TranslateTransform(390, 0),
                Effect = new DropShadowEffect { Color = Colors.Black, BlurRadius = 80, ShadowDepth = 0, Opacity = 0.5 }
            };
            m_tiroir.Children.Add(m_panneauTiroir);

            var entete = new Grid { Margin = new Thickness(0, 0, 0, 6) };
            entete.Children.Add(T("Paramètres", 22, Texte, FontWeights.Bold, Titre));
            var fermer = BoutonFenetre(IcFermer, false, FermerTiroir);
            fermer.HorizontalAlignment = HorizontalAlignment.Right;
            entete.Children.Add(fermer);
            pile.Children.Add(entete);

            Func<string, string, FrameworkElement, bool, FrameworkElement> reglage = (titre, detail, controle, trait) =>
            {
                var g = new Grid();
                g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                var textes = new StackPanel { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 12, 0) };
                textes.Children.Add(T(titre, 14, Texte, FontWeights.Bold));
                if (detail != null)
                {
                    var d = Para(detail, 12.5, Doux);
                    d.Margin = new Thickness(0, 2, 0, 0);
                    textes.Children.Add(d);
                }
                g.Children.Add(textes);
                if (controle != null)
                {
                    Grid.SetColumn(controle, 1);
                    g.Children.Add(controle);
                }
                return new Border { Child = g, Padding = new Thickness(0, 12, 0, 12), BorderBrush = B(Ligne), BorderThickness = new Thickness(0, 0, 0, trait ? 1 : 0) };
            };

            pile.Children.Add(reglage("Fermer les Dofus fantômes", "Ferme les jeux restés ouverts en arrière-plan sans fenêtre, pour toujours pouvoir relancer.",
                Bascule(m_reglages.Fantomes, on => { m_reglages.Fantomes = on; m_reglages.Sauver(); }), true));
            pile.Children.Add(reglage("Placer les fenêtres", "Range automatiquement les fenêtres en multi-comptes (bouton « Fenêtres » pour choisir la disposition).",
                Bascule(m_reglages.Placer, on => { m_reglages.Placer = on; m_reglages.Sauver(); }), true));
            pile.Children.Add(reglage("Réduire le launcher en jeu", "Le launcher se range dans la barre des tâches pendant que tu joues.",
                Bascule(m_reglages.Reduire, on => { m_reglages.Reduire = on; m_reglages.Sauver(); }), true));

            m_dossierJeu = T(m_updater.BaseDir, 12, Doux, FontWeights.Normal);
            m_dossierJeu.FontFamily = new FontFamily("Consolas");
            m_dossierJeu.TextWrapping = TextWrapping.Wrap;
            m_dossierJeu.Margin = new Thickness(0, 4, 0, 10);
            var dossier = new StackPanel();
            dossier.Children.Add(T("Dossier du jeu", 14, Texte, FontWeights.Bold));
            dossier.Children.Add(m_dossierJeu);
            var bd = new WrapPanel();
            Action<Border> ajouter = b => { b.Margin = new Thickness(0, 0, 8, 8); bd.Children.Add(b); };
            ajouter(SecondTexte("Vérifier les fichiers", () => { FermerTiroir(); Travailler(true); }, 34, 12.5));
            ajouter(SecondTexte("Changer…", ChangerDossier, 34, 12.5));
            ajouter(SecondTexte("Ouvrir", () => { try { Process.Start("explorer.exe", "\"" + m_updater.BaseDir + "\""); } catch { } }, 34, 12.5));
            dossier.Children.Add(bd);
            pile.Children.Add(new Border { Child = dossier, Padding = new Thickness(0, 12, 0, 6), BorderBrush = B(Ligne), BorderThickness = new Thickness(0, 0, 0, 1) });

            var compte = new StackPanel();
            compte.Children.Add(T("Compte Padgref", 14, Texte, FontWeights.Bold));
            var qui = T((m_compte.Pseudo ?? "") + (string.IsNullOrEmpty(m_compte.Email) ? "" : "  ·  " + m_compte.Email), 12.5, Doux, FontWeights.Normal);
            qui.Margin = new Thickness(0, 2, 0, 10);
            compte.Children.Add(qui);
            var bc = new WrapPanel();
            Action<Border> ajouter2 = b => { b.Margin = new Thickness(0, 0, 8, 8); bc.Children.Add(b); };
            ajouter2(SecondTexte("Mon compte sur le site", () => Outils.OuvrirSite(m_updater.SiteUrl + "compte.html"), 34, 12.5));
            ajouter2(SecondTexte("Créer un compte de jeu", CreerCompteJeu, 34, 12.5));
            ajouter2(SecondTexte("Code partagé", AjouterComptePartage, 34, 12.5));
            ajouter2(SecondTexte("Se déconnecter", DemanderDeconnexion, 34, 12.5));
            compte.Children.Add(bc);
            pile.Children.Add(new Border { Child = compte, Padding = new Thickness(0, 12, 0, 6) });

            m_versionTiroir = Para("", 12, Doux);
            m_versionTiroir.Margin = new Thickness(0, 14, 0, 0);
            pile.Children.Add(m_versionTiroir);
            return m_tiroir;
        }

        TextBlock m_dossierJeu, m_versionTiroir;

        void OuvrirTiroir()
        {
            m_dossierJeu.Text = m_updater.BaseDir;
            m_versionTiroir.Text = "Launcher Padgref" + (m_updater.Version != null && m_updater.Version != "?" ? " · version du jeu " + m_updater.Version : "");
            m_tiroir.Visibility = Visibility.Visible;
            m_tiroir.Children[0].BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.25)));
            m_panneauTiroir.RenderTransform.BeginAnimation(TranslateTransform.XProperty, new DoubleAnimation(390, 0, TimeSpan.FromSeconds(0.4)) { EasingFunction = new PowerEase { Power = 4, EasingMode = EasingMode.EaseOut } });
        }

        void FermerTiroir()
        {
            if (m_tiroir == null || m_tiroir.Visibility != Visibility.Visible) return;
            var a = new DoubleAnimation(390, TimeSpan.FromSeconds(0.25)) { EasingFunction = new PowerEase { Power = 2, EasingMode = EasingMode.EaseIn } };
            a.Completed += (s, e) => m_tiroir.Visibility = Visibility.Collapsed;
            m_panneauTiroir.RenderTransform.BeginAnimation(TranslateTransform.XProperty, a);
        }

        // ---------------------------------------------------------------- colonne de droite (événement, mise à jour, actualités)

        FrameworkElement ConstruireDroite()
        {
            var pile = new StackPanel();
            m_panEvenement = new StackPanel();
            m_panMaj = new StackPanel();
            m_panActus = new StackPanel();
            pile.Children.Add(Panneau(m_panEvenement, "Prochain événement"));
            var p2 = Panneau(m_panMaj, "Mise à jour");
            p2.Margin = new Thickness(0, 12, 0, 0);
            pile.Children.Add(p2);
            var p3 = Panneau(m_panActus, "Actualités");
            p3.Margin = new Thickness(0, 12, 0, 0);
            pile.Children.Add(p3);
            RemplirDroite(null);
            return new ScrollViewer { Content = pile, VerticalScrollBarVisibility = ScrollBarVisibility.Hidden };
        }

        static string Chaine(Dictionary<string, object> d, string cle) => d != null && d.TryGetValue(cle, out var v) && v != null ? v.ToString() : null;

        string Absolu(string url) => string.IsNullOrEmpty(url) ? null : url.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? url : m_updater.SiteUrl + url.TrimStart('/');

        /// <summary>Contenu de la colonne de droite : launcher.json du site (facultatif), sinon textes par défaut.</summary>
        void RemplirDroite(Dictionary<string, object> infos)
        {
            if (Chaine(infos, "discord") != null) m_lienDiscord = Absolu(Chaine(infos, "discord"));
            // événement
            m_panEvenement.Children.Clear();
            var ev = infos != null && infos.TryGetValue("evenement", out var e) ? e as Dictionary<string, object> : null;
            m_finEvenement = null;
            if (ev != null && DateTime.TryParse(Chaine(ev, "date"), CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var date) && date > DateTime.Now)
            {
                m_finEvenement = date;
                var nom = Para(Chaine(ev, "nom") ?? "Événement", 14, Texte);
                nom.FontWeight = FontWeights.Bold;
                nom.Margin = new Thickness(0, 0, 0, 10);
                m_panEvenement.Children.Add(nom);
                var grille = new UniformGrid { Columns = 4, Margin = new Thickness(-3, 0, -3, 0) };
                m_compteur = new TextBlock[4];
                var unites = new[] { "JOURS", "H", "MIN", "SEC" };
                for (var i = 0; i < 4; i++)
                {
                    var c = new StackPanel();
                    m_compteur[i] = T("00", 19, Texte, FontWeights.Bold, Titre);
                    m_compteur[i].HorizontalAlignment = HorizontalAlignment.Center;
                    c.Children.Add(m_compteur[i]);
                    var u = T(unites[i], 9.5, Doux, FontWeights.Bold);
                    u.HorizontalAlignment = HorizontalAlignment.Center;
                    c.Children.Add(u);
                    grille.Children.Add(new Border { Background = B(Fond2), CornerRadius = new CornerRadius(10), Padding = new Thickness(0, 7, 0, 7), Margin = new Thickness(3, 0, 3, 0), Child = c });
                }
                m_panEvenement.Children.Add(grille);
                var lien = Chaine(ev, "lien");
                if (lien != null)
                {
                    var l = Lien("Détails →", Accent2, () => Outils.OuvrirSite(Absolu(lien)), 13);
                    l.Margin = new Thickness(0, 10, 0, 0);
                    m_panEvenement.Children.Add(l);
                }
                MajCompteur();
            }
            else
            {
                m_panEvenement.Children.Add(Para("Aucun événement annoncé pour le moment. Les annonces arrivent d'abord sur le Discord.", 13, Doux));
                var l = Lien("Rejoindre le Discord →", Accent2, () => Outils.OuvrirSite(m_lienDiscord), 13);
                l.Margin = new Thickness(0, 8, 0, 0);
                m_panEvenement.Children.Add(l);
            }

            // notes de mise à jour
            m_panMaj.Children.Clear();
            var maj = infos != null && infos.TryGetValue("maj", out var m) ? m as Dictionary<string, object> : null;
            var titreMaj = Chaine(maj, "titre") ?? (m_updater.Version != null && m_updater.Version != "?" ? "Version du " + m_updater.Version : "Mises à jour automatiques");
            var tm = T(titreMaj, 14, Texte, FontWeights.Bold);
            tm.Margin = new Thickness(0, 0, 0, 6);
            m_panMaj.Children.Add(tm);
            var lignes = maj != null && maj.TryGetValue("lignes", out var lo) ? lo as object[] : null;
            if (lignes != null && lignes.Length > 0)
                foreach (var x in lignes.Take(6))
                {
                    var li = Para("•  " + x, 13, Doux);
                    li.Margin = new Thickness(0, 0, 0, 2);
                    m_panMaj.Children.Add(li);
                }
            else
                m_panMaj.Children.Add(Para("Le jeu se met à jour tout seul : seuls les fichiers modifiés sont téléchargés quand tu lances le launcher.", 13, Doux));
            var lienMaj = Lien("Notes complètes →", Accent2, () => Outils.OuvrirSite(Absolu(Chaine(maj, "lien")) ?? m_updater.SiteUrl), 13);
            lienMaj.Margin = new Thickness(0, 8, 0, 0);
            m_panMaj.Children.Add(lienMaj);

            // actualités
            m_panActus.Children.Clear();
            var actus = infos != null && infos.TryGetValue("actus", out var a) ? a as object[] : null;
            var liste = new List<Tuple<string, ImageSource, string>>();
            if (actus != null)
                foreach (var o in actus.Take(4))
                    if (o is Dictionary<string, object> d && Chaine(d, "titre") != null)
                    {
                        ImageSource img = null;
                        var url = Absolu(Chaine(d, "image"));
                        if (url != null) try { img = new BitmapImage(new Uri(url)); } catch { }
                        liste.Add(Tuple.Create(Chaine(d, "titre"), img ?? Theme.Image(liste.Count % 2 == 0 ? "p_actu1.jpg" : "p_actu2.jpg"), Absolu(Chaine(d, "lien")) ?? m_updater.SiteUrl));
                    }
            if (liste.Count == 0)
            {
                liste.Add(Tuple.Create("Giveaways et annonces sur le Discord Padgref", (ImageSource)Theme.Image("p_actu1.jpg"), m_lienDiscord));
                liste.Add(Tuple.Create("Classement et Espace Jeux sur le site", (ImageSource)Theme.Image("p_actu2.jpg"), m_updater.SiteUrl + "classement.html"));
            }
            for (var i = 0; i < liste.Count; i++)
            {
                var x = liste[i];
                var g = new Grid { Cursor = Cursors.Hand, Background = Brushes.Transparent };
                g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(56) });
                g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                g.Children.Add(new Border { Width = 56, Height = 40, CornerRadius = new CornerRadius(8), Background = new ImageBrush(x.Item2) { Stretch = Stretch.UniformToFill } });
                var t = T(x.Item1, 13.5, Texte, FontWeights.SemiBold);
                t.TextWrapping = TextWrapping.Wrap;
                t.TextTrimming = TextTrimming.CharacterEllipsis;
                t.MaxHeight = 40;
                t.Margin = new Thickness(10, 0, 0, 0);
                Grid.SetColumn(t, 1);
                g.Children.Add(t);
                g.MouseEnter += (s, ev2) => t.Foreground = B(Accent2);
                g.MouseLeave += (s, ev2) => t.Foreground = B(Texte);
                Clic(g, () => Outils.OuvrirSite(x.Item3));
                m_panActus.Children.Add(new Border { Child = g, Padding = new Thickness(0, 8, 0, 8), BorderBrush = B(Ligne), BorderThickness = new Thickness(0, i == 0 ? 0 : 1, 0, 0) });
            }
        }

        void MajCompteur()
        {
            if (m_finEvenement == null || m_compteur == null) return;
            var reste = m_finEvenement.Value - DateTime.Now;
            if (reste < TimeSpan.Zero) reste = TimeSpan.Zero;
            var v = new[] { (int)reste.TotalDays, reste.Hours, reste.Minutes, reste.Seconds };
            for (var i = 0; i < 4; i++) m_compteur[i].Text = v[i].ToString("00");
        }

        /// <summary>launcher.json du site (facultatif) : { discord, evenement:{nom,date,lien}, maj:{titre,lignes[],lien}, actus:[{titre,image,lien}] }.</summary>
        void ChargerInfos()
        {
            if (m_apercu != null)
            {
                RemplirDroite(new Dictionary<string, object>
                {
                    { "evenement", new Dictionary<string, object> { { "nom", "Tournoi PvP 3v3" }, { "date", DateTime.Now.AddDays(2).AddHours(4).ToString("s") } } },
                    { "maj", new Dictionary<string, object> { { "titre", "Mise à jour du 20/10" }, { "lignes", new object[] { "Donjons Nidas, Malléfisk, Phossile", "Nouveaux comportements de monstres", "Corrections de combat" } } } }
                });
                return;
            }
            var url = m_updater.SiteUrl + "launcher.json";
            new Thread(() =>
            {
                try
                {
                    using (var wc = new WebClient { Encoding = Encoding.UTF8 })
                    {
                        var d = new JavaScriptSerializer().DeserializeObject(wc.DownloadString(url)) as Dictionary<string, object>;
                        if (d != null) Dispatcher.BeginInvoke((Action)(() => RemplirDroite(d)));
                    }
                }
                catch { }
            }) { IsBackground = true }.Start();
        }

        // ---------------------------------------------------------------- dock du bas (pile, Fenêtres, JOUER)

        FrameworkElement ConstruireDock()
        {
            var g = new Grid();
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            m_pile = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(10, 0, 0, 0) };
            g.Children.Add(m_pile);
            var txt = new StackPanel { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(12, 0, 12, 0) };
            m_dockTitre = T("Aucun compte coché", 14.5, Texte, FontWeights.Bold);
            m_statut = T("Vérification du jeu…", 13, Doux, FontWeights.Normal);
            m_statut.Margin = new Thickness(0, 2, 0, 0);
            txt.Children.Add(m_dockTitre);
            txt.Children.Add(m_statut);
            Grid.SetColumn(txt, 1);
            g.Children.Add(txt);

            // PADGREF 24/10 (demande utilisateur : « enlève ce système de placement des fenêtres, laisse les joueurs faire ») : plus de bouton Fenêtres

            // PADGREF 21/10 : « Vérifier les mises à jour » à côté de JOUER (relit la liste du serveur et télécharge ce qui manque)
            var maj = SecondIcone(IcActualiser, "Mises à jour", VerifierMaintenant, 62, 14);
            maj.CornerRadius = new CornerRadius(16);
            maj.Padding = new Thickness(18, 0, 18, 0);
            maj.Margin = new Thickness(12, 0, 0, 0);
            maj.ToolTip = "Vérifier les mises à jour du jeu";
            Grid.SetColumn(maj, 3);
            g.Children.Add(maj);

            // JOUER : dégradé, remplissage = progression de la mise à jour, reflet qui balaye
            var interieur = new Grid();
            m_remplissage = new Rectangle { Fill = B(Color.FromArgb(0x4D, 0xFF, 0xFF, 0xFF)), HorizontalAlignment = HorizontalAlignment.Left, Width = 0 };
            interieur.Children.Add(m_remplissage);
            var balaye = new TranslateTransform(-300, 0);
            m_lueurJouer = new Rectangle
            {
                Width = 240, HorizontalAlignment = HorizontalAlignment.Left, RenderTransform = balaye,
                Fill = new LinearGradientBrush(new GradientStopCollection { new GradientStop(Colors.Transparent, 0.3), new GradientStop(Color.FromArgb(0x59, 0xFF, 0xFF, 0xFF), 0.5), new GradientStop(Colors.Transparent, 0.7) }, new Point(0, 0.3), new Point(1, 0.7))
            };
            var passe = new DoubleAnimationUsingKeyFrames { Duration = TimeSpan.FromSeconds(3.2), RepeatBehavior = RepeatBehavior.Forever };
            passe.KeyFrames.Add(new DiscreteDoubleKeyFrame(-260, KeyTime.FromTimeSpan(TimeSpan.Zero)));
            passe.KeyFrames.Add(new EasingDoubleKeyFrame(300, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(1.9)), new SineEase { EasingMode = EasingMode.EaseInOut }));
            passe.KeyFrames.Add(new DiscreteDoubleKeyFrame(300, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(3.2))));
            balaye.BeginAnimation(TranslateTransform.XProperty, passe);
            interieur.Children.Add(m_lueurJouer);
            var contenu = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(26, 0, 26, 0) };
            contenu.Children.Add(Ic(IcJouer, 20, B(AccentTxt), true));
            m_boutonTexte = T("JOUER", 19, AccentTxt, FontWeights.ExtraBold, Titre);
            m_boutonTexte.Margin = new Thickness(10, 0, 0, 0);
            contenu.Children.Add(m_boutonTexte);
            interieur.Children.Add(contenu);
            interieur.SizeChanged += (s, e) => interieur.Clip = new RectangleGeometry(new Rect(0, 0, e.NewSize.Width, e.NewSize.Height), 16, 16);
            var lueur = new DropShadowEffect { Color = Accent, BlurRadius = 40, ShadowDepth = 10, Direction = 270, Opacity = 0.5 };
            m_bouton = new Border { Height = 62, MinWidth = 230, Margin = new Thickness(12, 0, 0, 0), CornerRadius = new CornerRadius(16), Background = Degrade(), Child = interieur, Effect = lueur };
            m_bouton.MouseEnter += (s, e) => { if (Actif()) lueur.BeginAnimation(DropShadowEffect.OpacityProperty, new DoubleAnimation(0.9, TimeSpan.FromSeconds(0.15))); };
            m_bouton.MouseLeave += (s, e) => lueur.BeginAnimation(DropShadowEffect.OpacityProperty, new DoubleAnimation(0.5, TimeSpan.FromSeconds(0.2)));
            Clic(m_bouton, ClicBouton);
            Grid.SetColumn(m_bouton, 4);
            g.Children.Add(m_bouton);

            return new Border
            {
                Child = g, Padding = new Thickness(12), CornerRadius = new CornerRadius(20), BorderBrush = B(Ligne), BorderThickness = new Thickness(1),
                Background = B(A(0xD9, Fond2)), Margin = new Thickness(0, 14, 0, 0)
            };
        }

        void MajDock()
        {
            if (m_pile == null) return;
            var l = m_compte.Comptes.Where(c => m_coches.Contains(c.Id)).ToList();
            m_pile.Children.Clear();
            for (var i = 0; i < Math.Min(8, l.Count); i++)
            {
                var rond = new Grid { Width = 34, Height = 34, Margin = new Thickness(i == 0 ? 0 : -15, 0, 0, 0) };
                rond.Children.Add(new Ellipse { Fill = B(Fond2) });
                rond.Children.Add(new Ellipse { Stroke = B(Fond2), StrokeThickness = 2, Fill = new ImageBrush(Avatar(l[i], null)) { Stretch = Stretch.UniformToFill } });
                Panel.SetZIndex(rond, 10 - i);
                m_pile.Children.Add(rond);
            }
            m_pile.Visibility = l.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            m_dockTitre.Text = l.Count == 0 ? "Aucun compte coché" : l.Count == 1 ? m_dossiers.Nom(l[0]) : l.Count + " comptes prêts";
            m_dockTitre.ToolTip = l.Count > 1 ? string.Join(", ", l.Select(c => m_dossiers.Nom(c))) : null;
        }

        bool Actif() => m_etat == Etat.AInstaller || ((m_etat == Etat.Pret || m_etat == Etat.Erreur) && (m_updater.GameExists || m_apercu != null) && m_coches.Count > 0 && m_serveurEnLigne != false);

        void MajBouton()
        {
            if (m_bouton == null) return;
            var travail = m_etat == Etat.Travail || m_etat == Etat.Verification;
            switch (m_etat)
            {
                case Etat.AInstaller: m_boutonTexte.Text = "INSTALLER"; break;
                case Etat.Verification: m_boutonTexte.Text = "VÉRIFICATION…"; break;
                case Etat.Travail: m_boutonTexte.Text = m_travail + " " + m_pourcent + " %"; break;
                default:
                    if (DateTime.Now < m_aJourJusqua) m_boutonTexte.Text = "À JOUR ✓";
                    else if (m_serveurEnLigne == false) m_boutonTexte.Text = "SERVEUR HORS LIGNE";
                    else m_boutonTexte.Text = m_coches.Count > 1 ? "JOUER · " + m_coches.Count : "JOUER";
                    break;
            }
            var actif = Actif();
            m_bouton.Cursor = actif ? Cursors.Hand : Cursors.Arrow;
            m_bouton.Opacity = actif || travail ? 1 : 0.55;
            m_lueurJouer.Visibility = actif ? Visibility.Visible : Visibility.Hidden;
            m_bouton.ToolTip = (m_etat == Etat.Pret || m_etat == Etat.Erreur) && m_coches.Count == 0 ? "Coche un ou plusieurs comptes (8 au plus)" : null;
            if (!travail) Remplir(0);
        }

        void Remplir(int pourcent)
        {
            var largeur = Math.Max(0, m_bouton.ActualWidth) * pourcent / 100.0;
            m_remplissage.BeginAnimation(WidthProperty, new DoubleAnimation(largeur, TimeSpan.FromSeconds(0.2)));
        }

        void Progression(string texte, int pourcent)
        {
            m_statut.Text = texte;
            m_statut.Foreground = B(Doux);
            m_pourcent = Math.Max(0, Math.Min(100, pourcent));
            MajBouton();
            Remplir(m_pourcent);
        }

        // ---------------------------------------------------------------- installation / mise à jour / jeu

        void Verifier()
        {
            if (!m_updater.GameExists)
            {
                m_etat = Etat.AInstaller;
                m_statut.Text = "Le jeu n'est pas installé : clique sur INSTALLER (≈ 3,4 Go) ; dossier modifiable dans Paramètres.";
                m_statut.Foreground = B(Doux);
                MajBouton();
                return;
            }
            Travailler(false);
        }

        /// <summary>PADGREF 21/10 : bouton « Mises à jour » : relance la recherche de mises à jour (sauf pendant un téléchargement).</summary>
        void VerifierMaintenant()
        {
            if (m_etat == Etat.Travail || m_etat == Etat.Verification)
                return;
            m_derniereVerif = DateTime.MinValue;
            Verifier();
        }

        // PADGREF 21/10 : JOUER revérifie les mises à jour si la dernière vérification date de plus d'une minute (launcher resté
        // ouvert pendant une mise à jour : sinon le jeu, pas à jour, serait refusé à la connexion), puis lance le jeu
        DateTime m_derniereVerif = DateTime.MinValue;
        bool m_jouerApres;

        void Travailler(bool complet)
        {
            if (m_etat == Etat.Travail) return;
            m_etat = Etat.Travail;
            m_travail = complet ? (m_updater.GameExists ? "VÉRIFICATION" : "INSTALLATION") : "MISE À JOUR";
            Progression(complet ? (m_updater.GameExists ? "Vérification de tous les fichiers…" : "Préparation de l'installation…") : "Recherche de mises à jour…", 0);
            new Thread(() =>
            {
                var ok = m_updater.Run((texte, p) => Dispatcher.BeginInvoke((Action)(() => Progression(texte, p))), complet);
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    if (ok)
                    {
                        m_derniereVerif = DateTime.Now;
                        m_etat = Etat.Pret;
                        m_aJourJusqua = DateTime.Now.AddSeconds(1.6);
                        m_statut.Text = "Le jeu est à jour" + (m_updater.Version != null && m_updater.Version != "?" ? " · version du " + m_updater.Version : "") + ".";
                        m_statut.Foreground = B(Doux);
                    }
                    else
                    {
                        m_statut.Text = m_updater.LastError;
                        m_statut.Foreground = B(Rouge);
                        m_etat = m_updater.GameExists ? Etat.Erreur : Etat.AInstaller;
                    }
                    RemplirDroite(null);
                    ChargerInfos();
                    MajBouton();
                    if (m_jouerApres)
                    {
                        m_jouerApres = false;
                        if (ok && Actif())
                            JouerListe(m_compte.Comptes.Where(c => m_coches.Contains(c.Id)).ToList());
                    }
                }));
            }) { IsBackground = true }.Start();
        }

        void ChangerDossier()
        {
            if (m_etat == Etat.Travail) return;
            using (var d = new System.Windows.Forms.FolderBrowserDialog { Description = "Dossier d'installation de Padgref", SelectedPath = m_updater.BaseDir, ShowNewFolderButton = true })
            {
                if (d.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
                var choix = d.SelectedPath;
                if (!File.Exists(System.IO.Path.Combine(choix, "app", "Dofus.exe")) && Directory.EnumerateFileSystemEntries(choix).Any())
                    choix = System.IO.Path.Combine(choix, "Padgref");
                m_updater.ChangerDossier(choix);
            }
            m_dossierJeu.Text = m_updater.BaseDir;
            Verifier();
        }

        void ClicBouton()
        {
            if (!Actif()) return;
            if (m_etat == Etat.AInstaller) Travailler(true);
            else if (m_updater.GameExists && (DateTime.Now - m_derniereVerif).TotalSeconds > 60)
            {
                m_jouerApres = true;
                Travailler(false);
            }
            else JouerListe(m_compte.Comptes.Where(c => m_coches.Contains(c.Id)).ToList());
        }

        /// <summary>Lance plusieurs comptes (8 au plus), un toutes les 2 secondes (chacun avec son ticket de connexion automatique).</summary>
        void JouerListe(List<CompteJeu> comptes)
        {
            if (m_reglages.Fantomes) Fantomes.Nettoyer(m_updater.AppDir, TimeSpan.Zero, m_updater.Log);
            comptes = comptes.Where(c => !EnJeu(c)).ToList();
            if (comptes.Count > MAX_COCHES) { Toast(MAX_COCHES + " comptes maximum en même temps : les " + MAX_COCHES + " premiers sont lancés."); comptes = comptes.Take(MAX_COCHES).ToList(); }
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
            if (m_reglages.Fantomes) Fantomes.Nettoyer(m_updater.AppDir, TimeSpan.Zero, m_updater.Log);
            if (EnJeu(c)) { Toast("« " + m_dossiers.Nom(c) + " » est déjà en jeu."); return; }
            m_statut.Foreground = B(Doux);
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
                        if (r != null && !r.Ok) { m_lancement.Remove(c.Id); m_statut.Text = r.Erreur ?? "Connexion automatique impossible."; m_statut.Foreground = B(Rouge); Afficher(); return; }
                        var p = m_updater.StartGame(r?.Login, r?.Ticket, c.Id);
                        if (p != null)
                        {
                            m_enJeu[c.Id] = p;
                            m_ordre.Remove(c.Id);
                            m_ordre.Add(c.Id);
                            SuivrePlacement(p);
                        }
                        m_statut.Text = "« " + m_dossiers.Nom(c) + " » lancé.";
                        Afficher();
                        if (m_reglages.Reduire) WindowState = WindowState.Minimized;
                    }
                    catch (Exception ex)
                    {
                        m_statut.Text = "Impossible de lancer le jeu : " + ex.Message;
                        m_statut.Foreground = B(Rouge);
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
            m_serveurEnLigne = s != null && s.Item1;
            LauncherWindow.AfficherStatut(m_point, m_statutServeur, s);
            MajBouton();
        }

        /// <summary>
        /// PADGREF 23/10 (retour utilisateur : « il faut se reconnecter à chaque fois ») : le bouton « Se déconnecter » du rail,
        /// juste sous Paramètres, était pris pour « quitter » : un clic effaçait la connexion gardée. On demande confirmation.
        /// </summary>
        void DemanderDeconnexion()
        {
            if (m_etat == Etat.Travail) return;
            var fenetre = Fenetre("COMPTE PADGREF", "Se déconnecter ?", out var pile, 460);
            pile.Children.Add(Paragraphe("Le launcher oubliera ta connexion sur ce PC : au prochain lancement, il faudra te reconnecter." +
                Environment.NewLine + "Pour simplement fermer le launcher, utilise la croix en haut à droite : tu resteras connecté."));
            var boutons = new Grid { Margin = new Thickness(0, 6, 0, 0) };
            boutons.ColumnDefinitions.Add(new ColumnDefinition());
            boutons.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(10) });
            boutons.ColumnDefinitions.Add(new ColumnDefinition());
            boutons.Children.Add(SecondTexte("Rester connecté", FermerFenetre, 46, 14));
            var oui = SecondTexte("Se déconnecter", () => { FermerFenetre(); SeDeconnecter(); }, 46, 14);
            ((TextBlock)oui.Child).Foreground = B(Rouge);
            Grid.SetColumn(oui, 2);
            boutons.Children.Add(oui);
            pile.Children.Add(boutons);
            Ouvrir(fenetre);
        }

        void SeDeconnecter()
        {
            if (m_etat == Etat.Travail) return;
            var session = m_session;
            SessionGardee.Effacer();
            if (session != null) new Thread(() => m_api.Deconnexion(session)) { IsBackground = true }.Start();
            m_fini = true;
            var connexion = new LauncherWindow(m_updater, null) { WindowStartupLocation = WindowStartupLocation.Manual, Left = Left, Top = Top };
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
                Liste = new List<Dossiers.Dossier> { new Dossiers.Dossier { Nom = "Team donjon", Comptes = { "Pommes15", "Pommejte9", "pvpc" } }, new Dossiers.Dossier { Nom = "Mules", Comptes = { "xp6" } } },
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
            var etapes = new List<Tuple<double, Action, string>>
            {
                Tuple.Create(0.6, (Action)(() => { m_etat = Etat.Travail; m_travail = "MISE À JOUR"; Progression("Mise à jour : 2,18 Go / 3,42 Go  ·  12,4 Mo/s", 64); }), "principal_01_maj"),
                Tuple.Create(1.6, (Action)(() => { m_etat = Etat.Pret; m_statut.Text = "Le jeu est à jour · version du 20/10/2026."; m_coches.Add(1); m_coches.Add(2); m_coches.Add(3); Afficher(); }), "principal_02_comptes"),
                Tuple.Create(2.6, (Action)(() => { m_dossierActif = m_dossiers.Liste[0]; Afficher(); }), "principal_03_dossier"),
                Tuple.Create(3.6, (Action)(() => { m_dossierActif = null; Afficher(); OuvrirTiroir(); }), "principal_04_parametres"),
                Tuple.Create(5.6, (Action)(() => { FermerFenetre(); CreerCompteJeu(); }), "principal_06_creer"),
                Tuple.Create(6.6, (Action)(() => { FermerFenetre(); Partager(m_compte.Comptes.Take(2).ToList()); }), "principal_07_partager"),
                Tuple.Create(7.6, (Action)(() => { FermerFenetre(); m_vuePartages = true; Afficher(); }), "principal_08_partages"),
                Tuple.Create(8.6, (Action)(() => { m_vuePartages = false; m_renommerCompte = 2; Afficher(); }), "principal_09_renommer")
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
