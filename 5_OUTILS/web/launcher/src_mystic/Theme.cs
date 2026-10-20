// PADGREF 20/10/2026 : thème « Azur » du launcher (maquette v2 launcher-padgref.html validée par l'utilisateur) :
// couleurs, polices, icônes vectorielles (tracés de la maquette), boutons, champs, cases, interrupteurs, petits messages.
// Utilisé par la fenêtre de connexion (MysticLauncher.cs / Compte.cs) et par le grand launcher (Principal.cs).

using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MysticLauncher
{
    static class Theme
    {
        public const string Nom = "Padgref";
        public const string Discord = "https://discord.gg/padgref"; // PADGREF 20/10 : invitation du serveur Discord
        public const double W = 1260, H = 720, BARRE = 40;

        // palette azur (themes.css)
        public static readonly Color Fond = Rgb(0x07, 0x0B, 0x18);
        public static readonly Color Fond2 = Rgb(0x0C, 0x13, 0x28);
        public static readonly Color Carte = Color.FromArgb(0x8C, 20, 32, 64);
        public static readonly Color CartePleine = Rgb(0x11, 0x1B, 0x36);
        public static readonly Color Ligne = Color.FromArgb(0x29, 120, 160, 255);
        public static readonly Color Texte = Rgb(0xEA, 0xF0, 0xFF);
        public static readonly Color Doux = Rgb(0x9D, 0xAD, 0xD6);
        public static readonly Color Accent = Rgb(0x4D, 0x8D, 0xFF);
        public static readonly Color Accent2 = Rgb(0x38, 0xE1, 0xFF);
        public static readonly Color AccentTxt = Rgb(0x04, 0x10, 0x2A);
        public static readonly Color Lueur = Color.FromArgb(0x73, 77, 141, 255);
        public static readonly Color Vert = Rgb(0x3E, 0xE5, 0x8C);
        public static readonly Color Rouge = Rgb(0xFF, 0x4D, 0x6D);
        public static readonly Color Or = Rgb(0xF5, 0xD0, 0x61);

        // Sora / Inter ne sont pas installées sous Windows : Segoe UI Variable (Windows 11), sinon Segoe UI
        public static readonly FontFamily Titre = new FontFamily("Segoe UI Variable Display, Segoe UI");
        public static readonly FontFamily Police = new FontFamily("Segoe UI Variable Text, Segoe UI");

        public static Color Rgb(byte r, byte g, byte b) => Color.FromRgb(r, g, b);
        public static Color A(byte a, Color c) => Color.FromArgb(a, c.R, c.G, c.B);
        public static SolidColorBrush B(Color c) => new SolidColorBrush(c);

        /// <summary>Mélange c sur le fond (équivalent de color-mix).</summary>
        public static Color Melange(Color c, double part, Color fond) => Color.FromRgb(
            (byte)(c.R * part + fond.R * (1 - part)), (byte)(c.G * part + fond.G * (1 - part)), (byte)(c.B * part + fond.B * (1 - part)));

        /// <summary>Dégradé de la maquette (135°, accent → accent2).</summary>
        public static LinearGradientBrush Degrade() => new LinearGradientBrush(Accent, Accent2, new Point(0, 0), new Point(1, 1));

        public static BitmapImage Image(string nom)
        {
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.StreamSource = Assembly.GetExecutingAssembly().GetManifestResourceStream(nom);
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.EndInit();
            bmp.Freeze();
            return bmp;
        }

        static BitmapImage[] s_icones;
        /// <summary>Icônes par défaut des comptes (or, bleu, rouge, gris), quand le personnage n'a pas encore de rendu.</summary>
        public static ImageSource IconeCompte(int i)
        {
            if (s_icones == null)
                s_icones = new[] { Image("p_icone_or.png"), Image("p_icone_bleu.png"), Image("p_icone_rouge.png"), Image("p_icone_gris.png") };
            return s_icones[Math.Abs(i) % 4];
        }

        public static TextBlock T(string t, double taille, Color c, FontWeight poids, FontFamily f = null) => new TextBlock
        {
            Text = t, FontFamily = f ?? Police, FontSize = taille, FontWeight = poids, Foreground = new SolidColorBrush(c),
            TextTrimming = TextTrimming.CharacterEllipsis, VerticalAlignment = VerticalAlignment.Center
        };

        public static TextBlock Para(string t, double taille, Color c)
        {
            var p = T(t, taille, c, FontWeights.Normal);
            p.TextWrapping = TextWrapping.Wrap;
            p.TextTrimming = TextTrimming.None;
            p.LineHeight = taille * 1.55;
            return p;
        }

        // ---------------------------------------------------------------- icônes (tracés 24×24 de la maquette)

        public const string IcJouer = "M8,5 V19 L19,12 Z";
        public const string IcActus = "M4,5 H17 V19 H6 A2,2 0 0 1 4,17 Z M17,9 H20 V17 A2,2 0 0 1 18,19 M8,9 H13 M8,13 H13";
        public const string IcReglages = "M9,12 A3,3 0 1 0 15,12 A3,3 0 1 0 9,12 M12,2 V5 M12,19 V22 M4.2,4.2 L6.3,6.3 M17.7,17.7 L19.8,19.8 M2,12 H5 M19,12 H22 M4.2,19.8 L6.3,17.7 M17.7,6.3 L19.8,4.2";
        public const string IcDeconnexion = "M15,4 H19 V20 H15 M10,8 L6,12 L10,16 M6,12 H16";
        public const string IcDossier = "M3,6 A2,2 0 0 1 5,4 H9 L11,6 H19 A2,2 0 0 1 21,8 V18 A2,2 0 0 1 19,20 H5 A2,2 0 0 1 3,18 Z";
        public const string IcPlus = "M12,5 V19 M5,12 H19";
        public const string IcCrayon = "M4,20 H8 L19,9 L15,5 L4,16 Z";
        public const string IcFenetres = "M3,4 H11 V11 H3 Z M13,4 H21 V11 H13 Z M3,13 H11 V20 H3 Z M13,13 H21 V20 H13 Z";
        public const string IcRecherche = "M4,11 A7,7 0 1 0 18,11 A7,7 0 1 0 4,11 M20,20 L16.5,16.5";
        public const string IcPartages = "M9,11 A4,4 0 1 0 9,3 A4,4 0 1 0 9,11 M2,21 V19 A4,4 0 0 1 6,15 H12 A4,4 0 0 1 16,19 V21 M16,3.2 A4,4 0 0 1 16,10.8 M22,21 V19 A4,4 0 0 0 19,15.2";
        public const string IcEtoile = "M12,3 L14.8,8.8 L21,9.6 L16.5,14 L17.6,20.2 L12,17.3 L6.4,20.2 L7.5,14 L3,9.6 L9.2,8.8 Z";
        public const string IcActualiser = "M20,11 A8,8 0 1 0 18,17 M20,4 V11 H13";
        public const string IcFermer = "M6,6 L18,18 M18,6 L6,18";
        public const string IcReduire = "M5,12 H19";
        public const string IcZaap = "M3,12 A9,9 0 1 0 21,12 A9,9 0 1 0 3,12 M8,12 A4,4 0 1 0 16,12 A4,4 0 1 0 8,12";
        public const string IcCadenas = "M6,10 H18 A2,2 0 0 1 20,12 V19 A2,2 0 0 1 18,21 H6 A2,2 0 0 1 4,19 V12 A2,2 0 0 1 6,10 Z M8,10 V7 A4,4 0 0 1 16,7 V10";
        public const string IcCoche = "M5,12 L10,17 L19,7";
        public const string IcPlus3 = "M5,12 H5.01 M12,12 H12.01 M19,12 H19.01";
        public const string IcDossierOuvrir = "M3,7 V18 A2,2 0 0 0 5,20 H19 A2,2 0 0 0 21,18 V9 A2,2 0 0 0 19,7 H11 L9,5 H5 A2,2 0 0 0 3,7 Z";
        public const string IcBouclier = "M12,3 L19,6 V11 C19,16 15.5,19.5 12,21 C8.5,19.5 5,16 5,11 V6 Z M9,12 L11,14 L15,10";

        /// <summary>Icône vectorielle : trait (par défaut) ou remplie. La brosse peut être changée ensuite (survol).</summary>
        public static FrameworkElement Ic(string tracé, double taille, Brush couleur, bool plein = false, double epaisseur = 2)
        {
            var p = new Path
            {
                Data = Geometry.Parse(tracé), StrokeThickness = plein ? 0 : epaisseur, StrokeStartLineCap = PenLineCap.Round, StrokeEndLineCap = PenLineCap.Round,
                StrokeLineJoin = PenLineJoin.Round
            };
            if (plein) p.Fill = couleur; else p.Stroke = couleur;
            var g = new Grid { Width = 24, Height = 24 };
            g.Children.Add(p);
            return new Viewbox { Width = taille, Height = taille, Child = g, IsHitTestVisible = false, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
        }

        // ---------------------------------------------------------------- boutons

        /// <summary>Clic fiable : l'action n'a lieu que si le bouton de la souris a été ENFONCÉ sur l'élément (un relâchement seul,
        /// par exemple à l'ouverture de la fenêtre sous le curseur, ne déclenche rien).</summary>
        public static void Clic(UIElement e, Action action)
        {
            var appui = false;
            e.MouseLeftButtonDown += (s, a) => { appui = true; a.Handled = true; };
            e.MouseLeave += (s, a) => appui = false;
            e.MouseLeftButtonUp += (s, a) => { if (!appui) return; appui = false; action(); };
        }

        static void Cliquable(Border b, Action clic)
        {
            var appui = false;
            b.Cursor = Cursors.Hand;
            b.PreviewMouseLeftButtonDown += (s, e) => { e.Handled = true; appui = true; };
            b.MouseLeave += (s, e) => appui = false;
            b.PreviewMouseLeftButtonUp += (s, e) => { e.Handled = true; if (!appui) return; appui = false; if (b.IsEnabled) clic?.Invoke(); };
        }

        /// <summary>Gros bouton dégradé (« Se connecter avec le site Padgref »).</summary>
        public static Border Gros(string texte, string icone, Action clic, double hauteur = 54, double taille = 15.5)
        {
            var pile = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            if (icone != null)
            {
                var ic = Ic(icone, 20, B(AccentTxt), false, 2.4);
                ic.Margin = new Thickness(0, 0, 10, 0);
                pile.Children.Add(ic);
            }
            var t = T(texte, taille, AccentTxt, FontWeights.Bold, Titre);
            pile.Children.Add(t);
            var lueur = new DropShadowEffect { Color = Accent, BlurRadius = 28, ShadowDepth = 6, Direction = 270, Opacity = 0.45 };
            var b = new Border { Height = hauteur, CornerRadius = new CornerRadius(14), Background = Degrade(), Child = pile, Effect = lueur, Tag = t };
            b.MouseEnter += (s, e) => lueur.BeginAnimation(DropShadowEffect.OpacityProperty, new DoubleAnimation(0.8, TimeSpan.FromSeconds(0.15)));
            b.MouseLeave += (s, e) => lueur.BeginAnimation(DropShadowEffect.OpacityProperty, new DoubleAnimation(0.45, TimeSpan.FromSeconds(0.2)));
            Cliquable(b, clic);
            return b;
        }

        /// <summary>Bouton secondaire (fond carte, contour, contour accent au survol).</summary>
        public static Border Second(UIElement contenu, Action clic, double hauteur = 38, double rayon = 12)
        {
            var bord = B(Ligne);
            var b = new Border
            {
                Height = hauteur, Padding = new Thickness(14, 0, 14, 0), CornerRadius = new CornerRadius(rayon), Background = B(Carte), BorderBrush = bord,
                BorderThickness = new Thickness(1), Child = contenu
            };
            b.MouseEnter += (s, e) => bord.Color = Accent;
            b.MouseLeave += (s, e) => bord.Color = Ligne;
            Cliquable(b, clic);
            return b;
        }

        public static Border SecondTexte(string texte, Action clic, double hauteur = 38, double taille = 13)
        {
            var t = T(texte, taille, Texte, FontWeights.SemiBold);
            t.HorizontalAlignment = HorizontalAlignment.Center;
            return Second(t, clic, hauteur);
        }

        public static Border SecondIcone(string icone, string texte, Action clic, double hauteur = 38, double taille = 13)
        {
            var p = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center };
            p.Children.Add(Ic(icone, 16, B(Doux)));
            if (texte != null)
            {
                var t = T(texte, taille, Texte, FontWeights.SemiBold);
                t.Margin = new Thickness(8, 0, 0, 0);
                p.Children.Add(t);
            }
            return Second(p, clic, hauteur);
        }

        /// <summary>« Rejoindre le Discord » : icône de groupe de la maquette (ic_groupe.png) + texte, contour violet Discord au survol.</summary>
        public static Border BoutonDiscord(Action clic, double hauteur = 38)
        {
            var p = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center };
            p.Children.Add(new System.Windows.Controls.Image { Source = Image("p_groupe.png"), Width = 20, Height = 20, VerticalAlignment = VerticalAlignment.Center });
            var t = T("Rejoindre le Discord", 13, Texte, FontWeights.SemiBold);
            t.Margin = new Thickness(8, 0, 0, 0);
            p.Children.Add(t);
            var b = Second(p, clic, hauteur, 999);
            b.Padding = new Thickness(12, 0, 14, 0);
            b.ToolTip = Discord;
            var violet = Rgb(0x58, 0x65, 0xF2);
            b.Background = B(A(0x40, violet));
            b.MouseEnter += (s, e) => ((SolidColorBrush)b.BorderBrush).Color = violet;
            return b;
        }

        /// <summary>Bouton de la barre de titre (réduire, fermer).</summary>
        public static Border BoutonFenetre(string icone, bool fermer, Action action)
        {
            var fond = new SolidColorBrush(Colors.Transparent);
            var trait = B(Doux);
            var b = new Border { Width = 40, Height = 30, CornerRadius = new CornerRadius(8), Background = fond, Child = Ic(icone, 14, trait, false, 2.2), VerticalAlignment = VerticalAlignment.Center };
            b.MouseEnter += (s, e) => { fond.Color = fermer ? Rgb(0xE5, 0x48, 0x4D) : Carte; trait.Color = fermer ? Colors.White : Texte; };
            b.MouseLeave += (s, e) => { fond.Color = Colors.Transparent; trait.Color = Doux; };
            Clic(b, action);
            return b;
        }

        public static TextBlock Lien(string texte, Color c, Action clic, double taille = 13)
        {
            var t = T(texte, taille, c, FontWeights.SemiBold);
            t.Cursor = Cursors.Hand;
            t.MouseEnter += (s, e) => t.TextDecorations = TextDecorations.Underline;
            t.MouseLeave += (s, e) => t.TextDecorations = null;
            Clic(t, clic);
            return t;
        }

        // ---------------------------------------------------------------- champs

        /// <summary>Habille un TextBox / PasswordBox comme .champ input de la maquette (48 px, coins 12, contour accent2 au focus).</summary>
        public static Border Champ(Control c, double hauteur = 46)
        {
            c.BorderThickness = new Thickness(0);
            c.Background = Brushes.Transparent;
            c.Foreground = B(Texte);
            c.FontFamily = Police;
            c.FontSize = 15;
            c.VerticalContentAlignment = VerticalAlignment.Center;
            c.Padding = new Thickness(10, 0, 10, 0);
            if (c is TextBox tb) { tb.CaretBrush = B(Accent2); tb.SelectionBrush = B(Accent); }
            if (c is PasswordBox pb) { pb.CaretBrush = B(Accent2); pb.SelectionBrush = B(Accent); }
            var bord = B(Ligne);
            var halo = new DropShadowEffect { Color = Accent2, BlurRadius = 0, ShadowDepth = 0, Opacity = 0 };
            var b = new Border { Height = hauteur, CornerRadius = new CornerRadius(12), Background = B(Carte), BorderBrush = bord, BorderThickness = new Thickness(1), Child = c, Effect = halo };
            c.GotKeyboardFocus += (s, e) => { bord.Color = Accent2; halo.BlurRadius = 10; halo.Opacity = 0.35; };
            c.LostKeyboardFocus += (s, e) => { bord.Color = Ligne; halo.Opacity = 0; };
            b.MouseLeftButtonDown += (s, e) => c.Focus();
            return b;
        }

        public static TextBlock Etiquette(string t) => T(t, 13, Doux, FontWeights.SemiBold);

        /// <summary>Case à cocher dessinée (accent quand cochée).</summary>
        public static Border Case(bool cochee, double taille = 20)
        {
            return new Border
            {
                Width = taille, Height = taille, CornerRadius = new CornerRadius(taille * 0.33), BorderThickness = new Thickness(1.5),
                BorderBrush = B(cochee ? Accent2 : Ligne), Background = cochee ? (Brush)B(Accent2) : Brushes.Transparent,
                Child = cochee ? Ic(IcCoche, taille * 0.65, B(AccentTxt), false, 3.2) : null
            };
        }

        /// <summary>Case + texte, cliquable. Renvoie l'élément ; l'état est lu par etat().</summary>
        public static FrameworkElement CaseTexte(string texte, bool depart, out Func<bool> etat)
        {
            var coche = depart;
            var p = new StackPanel { Orientation = Orientation.Horizontal, Background = Brushes.Transparent, Cursor = Cursors.Hand };
            var c = Case(coche, 18);
            p.Children.Add(c);
            var t = T(texte, 14, Doux, FontWeights.Normal);
            t.Margin = new Thickness(9, 0, 0, 0);
            p.Children.Add(t);
            Clic(p, () =>
            {
                coche = !coche;
                p.Children[0] = Case(coche, 18);
            });
            etat = () => coche;
            return p;
        }

        /// <summary>Interrupteur (.bascule) : 44 × 26.</summary>
        public static Border Bascule(bool depart, Action<bool> change)
        {
            var on = depart;
            var fond = B(on ? Accent : Carte);
            var rond = new Ellipse { Width = 20, Height = 20, Fill = Brushes.White, HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(3, 0, 0, 0) };
            var deplace = new TranslateTransform(on ? 18 : 0, 0);
            rond.RenderTransform = deplace;
            var b = new Border { Width = 44, Height = 26, CornerRadius = new CornerRadius(13), Background = fond, BorderBrush = B(Ligne), BorderThickness = new Thickness(1), Child = rond, Cursor = Cursors.Hand, VerticalAlignment = VerticalAlignment.Center };
            Clic(b, () =>
            {
                on = !on;
                fond.Color = on ? Accent : Carte;
                deplace.BeginAnimation(TranslateTransform.XProperty, new DoubleAnimation(on ? 18 : 0, TimeSpan.FromSeconds(0.22)) { EasingFunction = new PowerEase { Power = 3, EasingMode = EasingMode.EaseOut } });
                change(on);
            });
            return b;
        }

        /// <summary>Séparateur « —— texte —— ».</summary>
        public static FrameworkElement Separateur(string texte)
        {
            var g = new Grid();
            g.ColumnDefinitions.Add(new ColumnDefinition());
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            g.ColumnDefinitions.Add(new ColumnDefinition());
            g.Children.Add(new Rectangle { Height = 1, Fill = B(Ligne), VerticalAlignment = VerticalAlignment.Center });
            var t = T(texte, 12, Doux, FontWeights.Normal);
            t.Margin = new Thickness(12, 0, 12, 0);
            Grid.SetColumn(t, 1);
            g.Children.Add(t);
            var r = new Rectangle { Height = 1, Fill = B(Ligne), VerticalAlignment = VerticalAlignment.Center };
            Grid.SetColumn(r, 2);
            g.Children.Add(r);
            return g;
        }

        /// <summary>Pastille arrondie (.puce).</summary>
        public static Border Puce(UIElement contenu) => new Border
        {
            Child = contenu, Padding = new Thickness(12, 7, 13, 7), CornerRadius = new CornerRadius(999), BorderBrush = B(Ligne), BorderThickness = new Thickness(1),
            Background = B(Carte), VerticalAlignment = VerticalAlignment.Center
        };

        /// <summary>Point vert « respire » (en ligne) ou rouge fixe (hors ligne).</summary>
        public static void PointStatut(Ellipse point, bool? enLigne)
        {
            point.BeginAnimation(UIElement.OpacityProperty, null);
            var c = enLigne == null ? Doux : enLigne.Value ? Vert : Rouge;
            point.Fill = B(c);
            point.Effect = enLigne == null ? null : new DropShadowEffect { Color = c, BlurRadius = 10, ShadowDepth = 0, Opacity = 0.9 };
            if (enLigne == true)
                point.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation(1, 0.45, TimeSpan.FromSeconds(1)) { AutoReverse = true, RepeatBehavior = RepeatBehavior.Forever, EasingFunction = new SineEase() });
        }

        /// <summary>Message éphémère en bas au centre (.toast).</summary>
        public static void Toast(Panel hote, string texte, bool erreur = false, double bas = 22)
        {
            var b = new Border
            {
                Padding = new Thickness(18, 12, 18, 12), CornerRadius = new CornerRadius(14), Background = B(Fond2), BorderBrush = B(erreur ? A(0x90, Rouge) : Ligne),
                BorderThickness = new Thickness(1), HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Bottom, Margin = new Thickness(0, 0, 0, bas),
                Child = T(texte, 14, erreur ? Rouge : Texte, FontWeights.SemiBold), IsHitTestVisible = false, MaxWidth = 720,
                Effect = new DropShadowEffect { Color = Colors.Black, BlurRadius = 40, ShadowDepth = 12, Direction = 270, Opacity = 0.5 }
            };
            if (hote is Grid)
            {
                Grid.SetColumnSpan(b, 99);
                Grid.SetRowSpan(b, 99);
            }
            Panel.SetZIndex(b, 1000);
            var monte = new TranslateTransform(0, 30);
            b.RenderTransform = monte;
            hote.Children.Add(b);
            var ease = new PowerEase { Power = 3, EasingMode = EasingMode.EaseOut };
            monte.BeginAnimation(TranslateTransform.YProperty, new DoubleAnimation(30, 0, TimeSpan.FromSeconds(0.35)) { EasingFunction = ease });
            var a = new DoubleAnimationUsingKeyFrames { Duration = TimeSpan.FromSeconds(3.2) };
            a.KeyFrames.Add(new LinearDoubleKeyFrame(0, KeyTime.FromPercent(0)));
            a.KeyFrames.Add(new LinearDoubleKeyFrame(1, KeyTime.FromPercent(0.08)));
            a.KeyFrames.Add(new LinearDoubleKeyFrame(1, KeyTime.FromPercent(0.88)));
            a.KeyFrames.Add(new LinearDoubleKeyFrame(0, KeyTime.FromPercent(1)));
            a.Completed += (s, e) => hote.Children.Remove(b);
            b.BeginAnimation(UIElement.OpacityProperty, a);
        }

        /// <summary>Barre de titre commune (logo, PADGREF Launcher, statut serveur, réduire / fermer).</summary>
        public static Grid BarreTitre(Window f, UIElement milieu, Action fermer)
        {
            var barre = new Grid { Height = BARRE, Background = B(Melange(Fond, 0.8, Colors.Black)) };
            barre.Children.Add(new Border { BorderBrush = B(Ligne), BorderThickness = new Thickness(0, 0, 0, 1) });
            barre.MouseLeftButtonDown += (s, e) => { if (e.ButtonState == MouseButtonState.Pressed) try { f.DragMove(); } catch { } };
            var gauche = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(16, 0, 0, 0), IsHitTestVisible = false };
            gauche.Children.Add(new System.Windows.Controls.Image { Source = Image("p_logo.png"), Width = 20, Height = 20, VerticalAlignment = VerticalAlignment.Center });
            var nom = T("PADGREF", 13, Texte, FontWeights.Bold, Titre);
            nom.Margin = new Thickness(10, 0, 8, 0);
            nom.Typography.Capitals = FontCapitals.Normal;
            gauche.Children.Add(nom);
            gauche.Children.Add(T("Launcher", 13, Doux, FontWeights.Normal));
            barre.Children.Add(gauche);
            var droite = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 0, 6, 0) };
            if (milieu != null)
            {
                ((FrameworkElement)milieu).Margin = new Thickness(0, 0, 10, 0);
                droite.Children.Add(milieu);
            }
            droite.Children.Add(BoutonFenetre(IcReduire, false, () => f.WindowState = WindowState.Minimized));
            droite.Children.Add(BoutonFenetre(IcFermer, true, fermer));
            barre.Children.Add(droite);
            return barre;
        }
    }
}
