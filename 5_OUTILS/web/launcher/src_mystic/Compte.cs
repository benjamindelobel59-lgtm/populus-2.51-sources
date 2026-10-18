// MYSTIC 18/10/2026 : connexion du launcher Mystic avec le COMPTE WEB (créé sur le site, il regroupe les comptes de jeu).
// API : AuthServer, Launcher/LauncherApiWeb.cs (/web/connexion, /web/session, /web/ticket, /web/deconnexion) + /statut.
// « Se souvenir de moi » : seule la clé de session est gardée, chiffrée par Windows (DPAPI, utilisateur courant) ; jamais le mot
// de passe. Après connexion : grand launcher (Principal.cs).

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;
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
    class Personnage
    {
        public string Nom, Apparence;
        public int Classe, Sexe, Niveau;
        public long Depuis = -1; // minutes depuis la dernière connexion (-1 = jamais)
    }

    class CompteJeu
    {
        public int Id;
        public string Login, Pseudo;
        public string PartagePar; // compte reçu en partage : pseudo web du propriétaire (null = à soi)
        public List<Personnage> Personnages = new List<Personnage>();
    }

    class Partage
    {
        public int Compte, Receveur;
        public string Login, Pseudo, Depuis;
    }

    class ReponseWeb
    {
        public bool Ok;
        public string Erreur, Email, Pseudo, Session, Login, Ticket;
        public List<CompteJeu> Comptes = new List<CompteJeu>();
    }

    /// <summary>Client de l'API du launcher (formulaires url-encoded, réponses JSON).</summary>
    class CompteApi
    {
        public readonly string Url;

        public CompteApi(string url)
        {
            Url = url.EndsWith("/") ? url : url + "/";
        }

        class ClientWeb : WebClient
        {
            protected override WebRequest GetWebRequest(Uri address)
            {
                var r = base.GetWebRequest(address);
                r.Timeout = 8000;
                return r;
            }
        }

        static Dictionary<string, object> Lire(byte[] octets) =>
            new JavaScriptSerializer().DeserializeObject(Encoding.UTF8.GetString(octets)) as Dictionary<string, object> ?? new Dictionary<string, object>();

        Dictionary<string, object> Appel(string chemin, NameValueCollection valeurs)
        {
            try
            {
                using (var wc = new ClientWeb())
                    return Lire(valeurs == null ? wc.DownloadData(Url + chemin) : wc.UploadValues(Url + chemin, "POST", valeurs));
            }
            catch (WebException ex)
            {
                // les erreurs de l'API (429…) ont aussi un corps JSON
                if (ex.Response is HttpWebResponse rep)
                    using (var flux = rep.GetResponseStream())
                    using (var ms = new MemoryStream())
                    {
                        flux.CopyTo(ms);
                        try { return Lire(ms.ToArray()); } catch { }
                    }
                return new Dictionary<string, object> { { "ok", false }, { "erreur", "Serveur de connexion injoignable." } };
            }
            catch (Exception)
            {
                return new Dictionary<string, object> { { "ok", false }, { "erreur", "Réponse du serveur de connexion illisible." } };
            }
        }

        static string Chaine(Dictionary<string, object> d, string cle) => d.TryGetValue(cle, out var v) && v != null ? v.ToString() : null;
        static int Entier(Dictionary<string, object> d, string cle) => d.TryGetValue(cle, out var v) && v != null ? Convert.ToInt32(v) : 0;
        static long Long(Dictionary<string, object> d, string cle) => d.TryGetValue(cle, out var v) && v != null ? Convert.ToInt64(v) : -1;
        static bool Vrai(Dictionary<string, object> d, string cle) => d.TryGetValue(cle, out var v) && v is bool && (bool)v;

        static ReponseWeb Web(Dictionary<string, object> d)
        {
            var r = new ReponseWeb
            {
                Ok = Vrai(d, "ok"), Erreur = Chaine(d, "erreur"), Email = Chaine(d, "email"), Pseudo = Chaine(d, "pseudo"),
                Session = Chaine(d, "session"), Login = Chaine(d, "login"), Ticket = Chaine(d, "ticket")
            };
            if (d.TryGetValue("comptes", out var comptes) && comptes is object[])
                foreach (var o in (object[])comptes)
                {
                    if (!(o is Dictionary<string, object> c))
                        continue;
                    var cj = new CompteJeu { Id = Entier(c, "id"), Login = Chaine(c, "login"), Pseudo = Chaine(c, "pseudo"), PartagePar = Chaine(c, "partagePar") };
                    if (c.TryGetValue("personnages", out var persos) && persos is object[])
                        foreach (var po in (object[])persos)
                            if (po is Dictionary<string, object> p)
                                cj.Personnages.Add(new Personnage
                                {
                                    Nom = Chaine(p, "nom"), Classe = Entier(p, "classe"), Sexe = Entier(p, "sexe"), Niveau = Entier(p, "niveau"),
                                    Apparence = Chaine(p, "apparence"), Depuis = Long(p, "depuis")
                                });
                    r.Comptes.Add(cj);
                }
            return r;
        }

        public ReponseWeb Connexion(string email, string motDePasse, bool souvenir) =>
            Web(Appel("web/connexion", new NameValueCollection { { "email", email }, { "motdepasse", motDePasse }, { "souvenir", souvenir ? "1" : "0" } }));

        public ReponseWeb Session(string session) => Web(Appel("web/session", new NameValueCollection { { "session", session } }));

        public ReponseWeb Ticket(string session, int compte) =>
            Web(Appel("web/ticket", new NameValueCollection { { "session", session }, { "compte", compte.ToString() } }));

        public void Deconnexion(string session) => Appel("web/deconnexion", new NameValueCollection { { "session", session } });

        // ---- partage de comptes (MYSTIC 18/10) ----

        /// <summary>Crée un code de partage pour ces comptes : (code, secondes de validité) ou erreur.</summary>
        public Tuple<string, int, string> PartageCreer(string session, IEnumerable<int> comptes)
        {
            var d = Appel("web/partage/creer", new NameValueCollection { { "session", session }, { "comptes", string.Join(",", comptes) } });
            return Vrai(d, "ok") ? Tuple.Create(Chaine(d, "code"), Entier(d, "secondes"), (string)null) : Tuple.Create((string)null, 0, Chaine(d, "erreur") ?? "Partage impossible.");
        }

        public void PartageAnnuler(string session, string code) => Appel("web/partage/annuler", new NameValueCollection { { "session", session }, { "code", code } });

        /// <summary>Utilise un code reçu : null si OK, sinon le message d'erreur.</summary>
        public string PartageUtiliser(string session, string code)
        {
            var d = Appel("web/partage/utiliser", new NameValueCollection { { "session", session }, { "code", code } });
            return Vrai(d, "ok") ? null : Chaine(d, "erreur") ?? "Code refusé.";
        }

        /// <summary>(partages donnés, partages reçus), null si erreur.</summary>
        public Tuple<List<Partage>, List<Partage>> PartageListe(string session)
        {
            var d = Appel("web/partage/liste", new NameValueCollection { { "session", session } });
            if (!Vrai(d, "ok")) return null;
            Func<string, List<Partage>> lire = cle =>
            {
                var l = new List<Partage>();
                if (d.TryGetValue(cle, out var o) && o is object[])
                    foreach (var x in (object[])o)
                        if (x is Dictionary<string, object> p)
                            l.Add(new Partage { Compte = Entier(p, "compte"), Receveur = Entier(p, "receveur"), Login = Chaine(p, "login"), Pseudo = Chaine(p, "pseudo"), Depuis = Chaine(p, "depuis") });
                return l;
            };
            return Tuple.Create(lire("donnes"), lire("recus"));
        }

        public void PartageRetirer(string session, int compte, int receveur) =>
            Appel("web/partage/retirer", new NameValueCollection { { "session", session }, { "compte", compte.ToString() }, { "receveur", receveur.ToString() } });

        /// <summary>(serveur ouvert, joueurs en ligne), null si l'API ne répond pas.</summary>
        public Tuple<bool, int> Statut()
        {
            var d = Appel("statut", null);
            return Vrai(d, "ok") ? Tuple.Create(Vrai(d, "ouvert"), Entier(d, "enLigne")) : null;
        }
    }

    /// <summary>Clé de session « Se souvenir de moi », chiffrée pour l'utilisateur Windows courant.</summary>
    static class SessionGardee
    {
        static readonly byte[] Sel = Encoding.UTF8.GetBytes("MysticLauncher-session-web");
        static string Fichier => System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Mystic", "session.dat");

        public static string Lire()
        {
            try
            {
                return File.Exists(Fichier) ? Encoding.UTF8.GetString(ProtectedData.Unprotect(File.ReadAllBytes(Fichier), Sel, DataProtectionScope.CurrentUser)) : null;
            }
            catch { return null; }
        }

        public static void Ecrire(string session)
        {
            try
            {
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Fichier));
                File.WriteAllBytes(Fichier, ProtectedData.Protect(Encoding.UTF8.GetBytes(session), Sel, DataProtectionScope.CurrentUser));
            }
            catch { }
        }

        public static void Effacer()
        {
            try { if (File.Exists(Fichier)) File.Delete(Fichier); } catch { }
        }
    }

    static class Outils
    {
        public static readonly string[] Classes =
        {
            "?", "Féca", "Osamodas", "Enutrof", "Sram", "Xélor", "Ecaflip", "Eniripsa", "Iop", "Crâ", "Sadida", "Sacrieur",
            "Pandawa", "Roublard", "Zobal", "Steamer", "Eliotrope", "Huppermage", "Ouginak"
        };

        public static string Classe(int c) => c > 0 && c < Classes.Length ? Classes[c] : "Classe " + c;

        public static string IlYA(long minutes)
        {
            if (minutes < 0) return "jamais connecté";
            if (minutes < 1) return "à l'instant";
            if (minutes < 60) return "il y a " + minutes + " min";
            if (minutes < 60 * 24) return "il y a " + (minutes / 60) + " h";
            if (minutes < 60 * 24 * 30) return "il y a " + (minutes / (60 * 24)) + " j";
            return "il y a " + (minutes / (60 * 24 * 30)) + " mois";
        }

        public static TextBlock Texte(string t, double taille, Color c, FontWeight poids) => new TextBlock
        {
            Text = t,
            FontFamily = new FontFamily("Segoe UI"),
            FontSize = taille,
            FontWeight = poids,
            Foreground = new SolidColorBrush(c),
            TextWrapping = TextWrapping.Wrap
        };

        public static void OuvrirSite(string url)
        {
            try { Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); } catch { }
        }

        /// <summary>
        /// Rendu officiel d'un personnage à partir de son apparence (service de rendu d'Ankama, seule l'apparence est envoyée),
        /// mis en cache 7 jours dans %LOCALAPPDATA%\Mystic\rendus. Appelle pret(image) sur le fil de l'interface.
        /// </summary>
        public static void ChargerRendu(System.Windows.Threading.Dispatcher ui, string apparence, Action<ImageSource> pret, string mode = "full/1/150_220-10")
        {
            if (string.IsNullOrEmpty(apparence) || apparence.Length > 2000)
                return;
            new Thread(() =>
            {
                try
                {
                    string cle;
                    using (var sha = SHA1.Create())
                        cle = BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(mode + apparence))).Replace("-", "").ToLowerInvariant();
                    var dossier = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Mystic", "rendus");
                    Directory.CreateDirectory(dossier);
                    var fichier = System.IO.Path.Combine(dossier, cle + ".png");
                    if (!File.Exists(fichier) || File.GetLastWriteTime(fichier) < DateTime.Now.AddDays(-7))
                    {
                        var hex = BitConverter.ToString(Encoding.UTF8.GetBytes(apparence)).Replace("-", "").ToLowerInvariant();
                        using (var wc = new WebClient())
                        {
                            var octets = wc.DownloadData("https://static.ankama.com/dofus/renderer/look/" + hex + "/" + mode + ".png");
                            if (octets.Length < 100 || octets[0] != 0x89)
                                return;
                            File.WriteAllBytes(fichier, octets);
                        }
                    }
                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.UriSource = new Uri(fichier);
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.EndInit();
                    bmp.Freeze();
                    ui.BeginInvoke((Action)(() => pret(bmp)));
                }
                catch { }
            }) { IsBackground = true }.Start();
        }

        /// <summary>Tête officielle de la classe, lue dans le client (content/gfx/heads/SmallHead_&lt;classe&gt;&lt;sexe&gt;.png).</summary>
        public static ImageSource TeteDeClasse(string appDir, int classe, int sexe)
        {
            try
            {
                var chemin = System.IO.Path.Combine(appDir, "content", "gfx", "heads", "SmallHead_" + classe + sexe + ".png");
                if (!File.Exists(chemin))
                    return null;
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.UriSource = new Uri(chemin);
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.EndInit();
                bmp.Freeze();
                return bmp;
            }
            catch { return null; }
        }
    }

    partial class LauncherWindow
    {
        static readonly Color Vert = Color.FromRgb(0x4a, 0xde, 0x80);
        static readonly Color Rouge = Color.FromRgb(0xf8, 0x71, 0x71);

        CompteApi m_api;
        Border m_carte;
        TextBox m_champEmail;
        PasswordBox m_champMdp;
        CheckBox m_souvenir;
        TextBlock m_erreurConnexion;
        Border m_boutonConnexion;
        TextBlock m_boutonConnexionTexte;
        Ellipse m_point;
        TextBlock m_statutServeur;
        Border m_pastille;
        bool m_connexionEnCours, m_ouvert;

        const double CARTE_L = 320;

        // ---------------------------------------------------------------- statut du serveur

        void ConstruireStatutServeur()
        {
            m_point = new Ellipse { Width = 8, Height = 8, Fill = new SolidColorBrush(Brume), VerticalAlignment = VerticalAlignment.Center };
            m_statutServeur = new TextBlock
            {
                Text = "Connexion au serveur…",
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 11.5,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Argent2),
                Margin = new Thickness(8, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            var ligne = new StackPanel { Orientation = Orientation.Horizontal };
            ligne.Children.Add(m_point);
            ligne.Children.Add(m_statutServeur);
            m_pastille = PastilleStatut(ligne);
            Placer(m_pastille, 18, 44);
            m_pastille.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.8)) { BeginTime = TimeSpan.FromSeconds(2.2) });
        }

        public static Border PastilleStatut(UIElement contenu) => new Border
        {
            Child = contenu,
            Padding = new Thickness(11, 5, 13, 5),
            CornerRadius = new CornerRadius(12),
            Background = new SolidColorBrush(Color.FromArgb(0x99, 0x0c, 0x09, 0x14)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(0x40, 0xb7, 0x9c, 0xff)),
            BorderThickness = new Thickness(1),
            Opacity = 0,
            IsHitTestVisible = false
        };

        public static void AfficherStatut(Ellipse point, TextBlock texte, Tuple<bool, int> statut)
        {
            point.BeginAnimation(OpacityProperty, null);
            if (statut == null || !statut.Item1)
            {
                point.Fill = new SolidColorBrush(Rouge);
                point.Effect = null;
                texte.Text = "Serveur hors ligne";
                return;
            }
            point.Fill = new SolidColorBrush(Vert);
            point.Effect = new DropShadowEffect { Color = Vert, BlurRadius = 8, ShadowDepth = 0, Opacity = 0.9 };
            point.BeginAnimation(OpacityProperty, new DoubleAnimation(1, 0.35, TimeSpan.FromSeconds(1.1)) { AutoReverse = true, RepeatBehavior = RepeatBehavior.Forever });
            texte.Text = "Serveur en ligne";
        }

        void SuivreStatut()
        {
            new Thread(() =>
            {
                while (!m_ouvert)
                {
                    var s = m_api.Statut();
                    Dispatcher.BeginInvoke((Action)(() => AfficherStatut(m_point, m_statutServeur, s)));
                    Thread.Sleep(30000);
                }
            }) { IsBackground = true }.Start();
        }

        // ---------------------------------------------------------------- carte de connexion

        Control StyleChamp(Control c)
        {
            c.Height = 40;
            c.FontFamily = new FontFamily("Segoe UI");
            c.FontSize = 14;
            c.Padding = new Thickness(10, 0, 10, 0);
            c.VerticalContentAlignment = VerticalAlignment.Center;
            c.Foreground = new SolidColorBrush(Argent);
            c.Background = new SolidColorBrush(Color.FromArgb(0x80, 0x07, 0x06, 0x0b));
            var bord = new SolidColorBrush(Color.FromArgb(0x60, 0xb7, 0x9c, 0xff));
            c.BorderBrush = bord;
            c.BorderThickness = new Thickness(1);
            c.Margin = new Thickness(0, 5, 0, 12);
            if (c is TextBox tb) tb.CaretBrush = new SolidColorBrush(VioletClair);
            if (c is PasswordBox pb) pb.CaretBrush = new SolidColorBrush(VioletClair);
            c.GotKeyboardFocus += (s, e) => c.BorderBrush = new SolidColorBrush(VioletClair);
            c.LostKeyboardFocus += (s, e) => c.BorderBrush = bord;
            return c;
        }

        TextBlock Lien(string texte, Action action)
        {
            var l = Outils.Texte(texte, 12, VioletClair, FontWeights.SemiBold);
            l.TextDecorations = TextDecorations.Underline;
            l.Cursor = Cursors.Hand;
            l.MouseLeftButtonUp += (s, e) => action();
            return l;
        }

        FrameworkElement ConstruireCarteCompte()
        {
            m_carte = new Border
            {
                Width = CARTE_L,
                Height = H - MARGE - 52 - 18,
                CornerRadius = new CornerRadius(14),
                Background = new LinearGradientBrush(Color.FromArgb(0xc8, 0x14, 0x10, 0x1f), Color.FromArgb(0xe0, 0x0c, 0x09, 0x14), 90),
                BorderThickness = new Thickness(1),
                BorderBrush = new LinearGradientBrush(Argb(0x70, VioletClair), Argb(0x50, Demon), 90),
                Effect = new DropShadowEffect { Color = Colors.Black, BlurRadius = 30, ShadowDepth = 6, Opacity = 0.7 },
                Opacity = 0,
                RenderTransform = new TranslateTransform(30, 0)
            };
            var pile = new StackPanel { Margin = new Thickness(26, 26, 26, 22), VerticalAlignment = VerticalAlignment.Center };
            m_carte.Child = pile;

            pile.Children.Add(Outils.Texte(Espace("CONNEXION"), 13, VioletClair, FontWeights.Bold));
            var sous = Outils.Texte("Connectez-vous avec votre compte Mystic : tous vos comptes de jeu vous attendent, sans retaper de mot de passe.", 12, Brume, FontWeights.Normal);
            sous.Margin = new Thickness(0, 8, 0, 22);
            pile.Children.Add(sous);
            pile.Children.Add(Outils.Texte("E-mail", 12, Argent2, FontWeights.SemiBold));
            m_champEmail = (TextBox)StyleChamp(new TextBox { MaxLength = 190 });
            pile.Children.Add(m_champEmail);
            pile.Children.Add(Outils.Texte("Mot de passe", 12, Argent2, FontWeights.SemiBold));
            m_champMdp = (PasswordBox)StyleChamp(new PasswordBox { MaxLength = 128 });
            pile.Children.Add(m_champMdp);
            m_souvenir = new CheckBox
            {
                Content = Outils.Texte("Se souvenir de moi", 12, Argent2, FontWeights.Normal),
                IsChecked = true,
                Margin = new Thickness(0, 0, 0, 18),
                Cursor = Cursors.Hand
            };
            pile.Children.Add(m_souvenir);

            m_boutonConnexionTexte = Outils.Texte(Espace("SE CONNECTER"), 14, Colors.White, FontWeights.Bold);
            m_boutonConnexionTexte.HorizontalAlignment = HorizontalAlignment.Center;
            m_boutonConnexionTexte.VerticalAlignment = VerticalAlignment.Center;
            var lueur = new DropShadowEffect { Color = Violet, BlurRadius = 16, ShadowDepth = 0, Opacity = 0.55 };
            m_boutonConnexion = new Border
            {
                Height = 46,
                CornerRadius = new CornerRadius(10),
                Background = new LinearGradientBrush(Color.FromRgb(0x9d, 0x74, 0xff), Color.FromRgb(0x5b, 0x21, 0xb6), 90),
                BorderBrush = new SolidColorBrush(Argb(0xc0, VioletClair)),
                BorderThickness = new Thickness(1),
                Child = m_boutonConnexionTexte,
                Cursor = Cursors.Hand,
                Effect = lueur
            };
            m_boutonConnexion.MouseEnter += (s, e) => lueur.BeginAnimation(DropShadowEffect.OpacityProperty, new DoubleAnimation(1, TimeSpan.FromSeconds(0.2)));
            m_boutonConnexion.MouseLeave += (s, e) => lueur.BeginAnimation(DropShadowEffect.OpacityProperty, new DoubleAnimation(0.55, TimeSpan.FromSeconds(0.2)));
            m_boutonConnexion.MouseLeftButtonUp += (s, e) => SeConnecter();
            pile.Children.Add(m_boutonConnexion);

            m_erreurConnexion = Outils.Texte("", 12, Rouge, FontWeights.SemiBold);
            m_erreurConnexion.Margin = new Thickness(0, 12, 0, 0);
            pile.Children.Add(m_erreurConnexion);

            var creer = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 18, 0, 0) };
            creer.Children.Add(Outils.Texte("Pas encore de compte ?  ", 12, Brume, FontWeights.Normal));
            creer.Children.Add(Lien("Créer un compte Mystic", () => Outils.OuvrirSite(m_updater.SiteUrl + "index.html#rejoindre")));
            pile.Children.Add(creer);

            m_champEmail.KeyDown += (s, e) => { if (e.Key == Key.Enter) m_champMdp.Focus(); };
            m_champMdp.KeyDown += (s, e) => { if (e.Key == Key.Enter) SeConnecter(); };

            Placer(m_carte, W - MARGE - CARTE_L, 52);
            return m_carte;
        }

        void AfficherCarte()
        {
            var ease = new PowerEase { Power = 3, EasingMode = EasingMode.EaseOut };
            m_carte.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.9)) { BeginTime = TimeSpan.FromSeconds(2.3), EasingFunction = ease });
            m_carte.RenderTransform.BeginAnimation(TranslateTransform.XProperty, new DoubleAnimation(30, 0, TimeSpan.FromSeconds(0.9)) { BeginTime = TimeSpan.FromSeconds(2.3), EasingFunction = ease });
        }

        void ConnexionEnCours(bool oui)
        {
            m_connexionEnCours = oui;
            m_boutonConnexionTexte.Text = oui ? "CONNEXION…" : Espace("SE CONNECTER");
            m_boutonConnexion.Opacity = oui ? 0.6 : 1;
        }

        void SecouerCarte()
        {
            var a = new DoubleAnimationUsingKeyFrames { Duration = TimeSpan.FromSeconds(0.4) };
            double[] v = { 0, -8, 7, -5, 3, 0 };
            for (var i = 0; i < v.Length; i++)
                a.KeyFrames.Add(new LinearDoubleKeyFrame(v[i], KeyTime.FromPercent(i / (double)(v.Length - 1))));
            m_carte.RenderTransform.BeginAnimation(TranslateTransform.XProperty, a);
        }

        void SeConnecter()
        {
            if (m_connexionEnCours)
                return;
            var email = m_champEmail.Text.Trim();
            var mdp = m_champMdp.Password;
            var souvenir = m_souvenir.IsChecked == true;
            m_erreurConnexion.Text = "";
            if (email.Length == 0 || mdp.Length == 0)
            {
                m_erreurConnexion.Text = "Indiquez votre e-mail et votre mot de passe.";
                SecouerCarte();
                return;
            }
            ConnexionEnCours(true);
            new Thread(() =>
            {
                var r = m_api.Connexion(email, mdp, souvenir);
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    ConnexionEnCours(false);
                    if (!r.Ok)
                    {
                        m_erreurConnexion.Text = r.Erreur ?? "Connexion impossible.";
                        SecouerCarte();
                        return;
                    }
                    m_champMdp.Clear();
                    if (souvenir && r.Session != null)
                        SessionGardee.Ecrire(r.Session);
                    else
                        SessionGardee.Effacer();
                    m_updater.Log("Connecté au compte web " + r.Email);
                    OuvrirPrincipal(r, r.Session, souvenir);
                }));
            }) { IsBackground = true }.Start();
        }

        void ReprendreSession()
        {
            var session = SessionGardee.Lire();
            if (session == null)
                return;
            new Thread(() =>
            {
                var r = m_api.Session(session);
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    if (!r.Ok)
                    {
                        if (r.Erreur != null && r.Erreur.StartsWith("Session"))
                            SessionGardee.Effacer();
                        return;
                    }
                    // laisse l'intro se jouer un instant avant d'ouvrir le grand launcher
                    var attente = Math.Max(0.05, 2.6 - m_horloge.Elapsed.TotalSeconds);
                    var t = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromSeconds(attente) };
                    t.Tick += (s, e) => { t.Stop(); OuvrirPrincipal(r, session, true); };
                    t.Start();
                }));
            }) { IsBackground = true }.Start();
        }

        /// <summary>Ferme la fenêtre de connexion (fondu) et ouvre le grand launcher.</summary>
        void OuvrirPrincipal(ReponseWeb r, string session, bool gardee)
        {
            if (m_ouvert)
                return;
            m_ouvert = true;
            var principal = new PrincipalWindow(m_updater, m_api, r, session, gardee, m_apercu);
            Application.Current.MainWindow = principal;
            principal.Show();
            var fondu = new DoubleAnimation(0, TimeSpan.FromSeconds(0.45)) { EasingFunction = new PowerEase { Power = 2, EasingMode = EasingMode.EaseIn } };
            fondu.Completed += (s, e) => Close();
            BeginAnimation(OpacityProperty, fondu);
        }

        // aperçu : connexion fictive
        void ApercuConnexion()
        {
            m_champEmail.Text = "joueur@mystic.fr";
            m_champMdp.Password = "motdepasse";
            AfficherStatut(m_point, m_statutServeur, Tuple.Create(true, 47));
        }

        void ApercuOuvrir()
        {
            var r = new ReponseWeb
            {
                Ok = true, Email = "joueur@mystic.fr", Pseudo = "Beny",
                Comptes = new List<CompteJeu>
                {
                    new CompteJeu { Id = 1, Login = "Pommes15", Pseudo = "Pommes", Personnages = { new Personnage { Nom = "Pelep", Classe = 12, Niveau = 200, Depuis = 3, Apparence = "{1|120,2188|1=32235200,2=38751540,3=58004758,4=82566848,5=87901995|160}" } } },
                    new CompteJeu { Id = 2, Login = "Pommejte9", Pseudo = "Pommejte", Personnages = { new Personnage { Nom = "Otifapu", Classe = 7, Sexe = 1, Niveau = 187, Depuis = 22, Apparence = "{1|71,2116,238,146|1=14785337,2=16707772,3=16707772,4=11941144,5=15388778|115}" }, new Personnage { Nom = "Mocahe", Classe = 9, Niveau = 156 } } },
                    new CompteJeu { Id = 3, Login = "pvpc", Pseudo = "Huser", Personnages = { new Personnage { Nom = "Mocahe", Classe = 9, Niveau = 156, Depuis = 75, Apparence = "{1|90,2140,238,146|1=16762983,2=14774542,3=5792768,4=5845776,5=16422159|140}" } } },
                    new CompteJeu { Id = 4, Login = "xp6", Pseudo = "Jujuben", Personnages = { new Personnage { Nom = "Mecabixuf", Classe = 7, Sexe = 1, Niveau = 133, Depuis = 1100, Apparence = "{1|71,2116|1=33342321,2=50307866,3=56324130,4=73112394,5=100655420|115}" } } },
                    new CompteJeu { Id = 5, Login = "xp5", Pseudo = "Caros" }
                }
            };
            // comptes supplémentaires (aperçu de la densité : 8-9 comptes par colonne)
            var looks = new[] { r.Comptes[0].Personnages[0].Apparence, r.Comptes[1].Personnages[0].Apparence, r.Comptes[2].Personnages[0].Apparence, r.Comptes[3].Personnages[0].Apparence };
            var noms = new[] { "Portos", "Pomme-jungle", "Melan", "Phobelle", "Wyllin-Daya", "Damy", "Cyria", "Bound", "Glissade", "Cobro", "Firefoux", "Boustiflore", "Jujuben", "Huser" };
            for (var i = 0; i < noms.Length; i++)
                r.Comptes.Add(new CompteJeu { Id = 10 + i, Login = "pvp" + (char)('a' + i), Pseudo = noms[i], Personnages = { new Personnage { Nom = noms[i], Classe = 1 + i % 18, Niveau = 140 + i * 4, Depuis = 5 + i * 37, Apparence = looks[i % 4] } } });
            OuvrirPrincipal(r, null, false);
        }
    }
}
