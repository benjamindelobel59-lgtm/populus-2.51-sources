// MYSTIC 18/10/2026 → PADGREF 20/10 : connexion du launcher avec le COMPTE WEB (créé sur le site, il regroupe les comptes de jeu).
// API : AuthServer, Launcher/LauncherApiWeb.cs (/web/connexion, /web/session, /web/ticket, /web/deconnexion, /web/appairage) + /statut.
// « Rester connecté » : seule la clé de session est gardée, chiffrée par Windows (DPAPI, utilisateur courant) ; jamais le mot
// de passe. Interface : vue « Connexion Zaap » (thème Azur, Theme.cs). Après connexion : grand launcher (Principal.cs).

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
using System.Windows.Threading;
using static MysticLauncher.Theme;

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

        /// <summary>MYSTIC 20/10 « Se connecter grâce au site » : session si le joueur a validé ce code sur le site (Ok faux + Erreur nulle = en attente).</summary>
        public ReponseWeb Appairage(string code) => Web(Appel("web/appairage", new NameValueCollection { { "code", code } }));

        /// <summary>MYSTIC 20/10 : crée un compte de jeu sans mot de passe, lié au compte web (renvoie les comptes à jour).</summary>
        public ReponseWeb CreerJeu(string session, string pseudo, string reponse) =>
            Web(Appel("web/creer_jeu", new NameValueCollection { { "session", session }, { "pseudo", pseudo }, { "reponse", reponse } }));

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
        CompteApi m_api;
        FrameworkElement m_carte;
        TextBox m_champEmail;
        PasswordBox m_champMdp;
        Func<bool> m_souvenir;
        TextBlock m_message;
        Border m_boutonConnexion, m_boutonSite;
        TextBlock m_boutonConnexionTexte, m_boutonSiteTexte, m_sousTitre;
        Ellipse m_point;
        TextBlock m_statutServeur;
        bool m_connexionEnCours, m_ouvert;
        string m_codeSite;

        const string TexteSite = "Se connecter avec le site Padgref";
        const string TexteAccueil = "Connecte-toi une fois : le launcher retient ta session en toute sécurité.";

        // ---------------------------------------------------------------- statut du serveur (en ligne / hors ligne seulement)

        FrameworkElement ConstruireStatutServeur()
        {
            m_point = new Ellipse { Width = 8, Height = 8, VerticalAlignment = VerticalAlignment.Center };
            Theme.PointStatut(m_point, null);
            m_statutServeur = T("Serveur…", 12.5, Texte, FontWeights.SemiBold);
            m_statutServeur.Margin = new Thickness(8, 0, 0, 0);
            var ligne = new StackPanel { Orientation = Orientation.Horizontal };
            ligne.Children.Add(m_point);
            ligne.Children.Add(m_statutServeur);
            var p = Puce(ligne);
            p.Padding = new Thickness(11, 4, 12, 4);
            p.IsHitTestVisible = false;
            return p;
        }

        public static void AfficherStatut(Ellipse point, TextBlock texte, Tuple<bool, int> statut)
        {
            var ok = statut != null && statut.Item1;
            Theme.PointStatut(point, ok);
            texte.Text = ok ? "Serveur en ligne" : "Serveur hors ligne";
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

        // ---------------------------------------------------------------- formulaire « Connexion Zaap »

        FrameworkElement ConstruireCarteCompte()
        {
            var pile = new StackPanel { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(56, 20, 56, 20), RenderTransform = new TranslateTransform() };
            m_carte = pile;
            Action<FrameworkElement, double> ajouter = (e, marge) => { e.Margin = new Thickness(0, marge, 0, 0); pile.Children.Add(e); };

            pile.Children.Add(ConstruireZaap());
            ajouter(T("Connexion Zaap", 28, Texte, FontWeights.Bold, Titre), 12);
            m_sousTitre = Para(TexteAccueil, 14.5, Doux);
            ajouter(m_sousTitre, 6);

            // MYSTIC 20/10 : si le joueur est déjà connecté au site, un clic suffit (il confirme sur la page qui s'ouvre)
            m_boutonSite = Gros(TexteSite, IcZaap, ConnexionParLeSite);
            m_boutonSiteTexte = (TextBlock)m_boutonSite.Tag;
            ajouter(m_boutonSite, 18);
            ajouter(Separateur("ou avec ton identifiant"), 16);

            ajouter(Etiquette("E-mail ou nom de compte"), 14);
            m_champEmail = new TextBox { MaxLength = 190 };
            ajouter(Champ(m_champEmail, 44), 6);
            ajouter(Etiquette("Mot de passe"), 12);
            m_champMdp = new PasswordBox { MaxLength = 128 };
            ajouter(Champ(m_champMdp, 44), 6);
            ajouter(CaseTexte("Rester connecté sur ce PC", true, out m_souvenir), 12);

            m_boutonConnexionTexte = T("Se connecter", 14.5, Texte, FontWeights.SemiBold);
            m_boutonConnexionTexte.HorizontalAlignment = HorizontalAlignment.Center;
            m_boutonConnexion = Second(m_boutonConnexionTexte, SeConnecter, 46, 14);
            ajouter(m_boutonConnexion, 14);

            m_message = Para("", 13, Rouge);
            m_message.FontWeight = FontWeights.SemiBold;
            ajouter(m_message, 10);

            var liens = new Grid();
            liens.Children.Add(Lien("Créer un compte", Accent2, () => Outils.OuvrirSite(m_updater.SiteUrl + "connexion.html")));
            var oubli = Lien("Mot de passe oublié ?", Doux, () => Outils.OuvrirSite(m_updater.SiteUrl + "securite.html"));
            oubli.HorizontalAlignment = HorizontalAlignment.Right;
            liens.Children.Add(oubli);
            ajouter(liens, 8);

            m_champEmail.KeyDown += (s, e) => { if (e.Key == Key.Enter) m_champMdp.Focus(); };
            m_champMdp.KeyDown += (s, e) => { if (e.Key == Key.Enter) SeConnecter(); };
            return pile;
        }

        void ConnexionEnCours(bool oui)
        {
            m_connexionEnCours = oui;
            m_boutonConnexionTexte.Text = oui ? "Connexion…" : "Se connecter";
            m_boutonConnexion.Opacity = oui ? 0.6 : 1;
            EtatZaap(oui ? 1 : 0);
        }

        void Message(string texte, bool erreur)
        {
            m_message.Foreground = B(erreur ? Rouge : Accent2);
            m_message.Text = texte;
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
            var souvenir = m_souvenir();
            Message("", true);
            if (email.Length == 0 || mdp.Length == 0)
            {
                Message("Indique ton e-mail (ou nom de compte) et ton mot de passe.", true);
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
                        Message(r.Erreur ?? "Connexion impossible.", true);
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

        /// <summary>MYSTIC 20/10 : ouvre la page du site avec un code secret, puis attend (5 min max) que le joueur l'autorise.</summary>
        void ConnexionParLeSite()
        {
            if (m_connexionEnCours)
                return;
            var octets = new byte[32];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create()) rng.GetBytes(octets);
            var code = BitConverter.ToString(octets).Replace("-", "").ToLowerInvariant();
            m_codeSite = code;
            var court = (code.Substring(0, 3) + "-" + code.Substring(3, 3)).ToUpperInvariant();
            Message("Valide la connexion dans la page du site qui vient de s'ouvrir." + Environment.NewLine + "Code à vérifier : " + court, false);
            m_boutonSiteTexte.Text = "En attente du site…";
            EtatZaap(1);
            Outils.OuvrirSite(m_updater.SiteUrl + "connexion-launcher.html#" + code);
            Action finAttente = () => { m_boutonSiteTexte.Text = TexteSite; EtatZaap(0); };
            new Thread(() =>
            {
                var fin = DateTime.Now.AddMinutes(5);
                while (DateTime.Now < fin && m_codeSite == code && !m_ouvert)
                {
                    Thread.Sleep(2000);
                    var r = m_api.Appairage(code);
                    if (r.Ok)
                    {
                        Dispatcher.BeginInvoke((Action)(() =>
                        {
                            if (m_ouvert) return;
                            m_codeSite = null;
                            if (r.Session != null) SessionGardee.Ecrire(r.Session);
                            m_updater.Log("Connecté au compte web " + r.Email + " (par le site)");
                            OuvrirPrincipal(r, r.Session, true);
                        }));
                        return;
                    }
                    if (r.Erreur != null && !r.Erreur.StartsWith("Serveur"))
                    {
                        Dispatcher.BeginInvoke((Action)(() => { Message(r.Erreur, true); finAttente(); }));
                        return;
                    }
                }
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    if (m_codeSite != code || m_ouvert) return;
                    m_codeSite = null;
                    Message("Temps écoulé : clique de nouveau sur « " + TexteSite + " ».", true);
                    finAttente();
                }));
            }) { IsBackground = true }.Start();
        }

        void ReprendreSession()
        {
            var session = SessionGardee.Lire();
            if (session == null)
                return;
            m_sousTitre.Text = "Reprise de ta session…";
            EtatZaap(1);
            new Thread(() =>
            {
                var r = m_api.Session(session);
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    if (!r.Ok)
                    {
                        if (r.Erreur != null && r.Erreur.StartsWith("Session"))
                            SessionGardee.Effacer();
                        m_sousTitre.Text = TexteAccueil;
                        EtatZaap(0);
                        return;
                    }
                    OuvrirPrincipal(r, session, true);
                }));
            }) { IsBackground = true }.Start();
        }

        /// <summary>Anneau vert « Connecté », puis ouvre le grand launcher au même endroit et ferme la connexion (fondu).</summary>
        void OuvrirPrincipal(ReponseWeb r, string session, bool gardee)
        {
            if (m_ouvert)
                return;
            m_ouvert = true;
            EtatZaap(2);
            m_sousTitre.Text = "Connecté. Ouverture du launcher…";
            var attente = new DispatcherTimer { Interval = TimeSpan.FromSeconds(m_apercu != null ? 0.05 : 0.45) };
            attente.Tick += (s, e) =>
            {
                attente.Stop();
                var principal = new PrincipalWindow(m_updater, m_api, r, session, gardee, m_apercu);
                principal.WindowStartupLocation = WindowStartupLocation.Manual;
                principal.Left = Left;
                principal.Top = Top;
                Application.Current.MainWindow = principal;
                principal.Show();
                var fondu = new DoubleAnimation(0, TimeSpan.FromSeconds(0.3)) { EasingFunction = new PowerEase { Power = 2, EasingMode = EasingMode.EaseIn } };
                fondu.Completed += (o, a) => Close();
                BeginAnimation(OpacityProperty, fondu);
            };
            attente.Start();
        }

        // aperçu : connexion fictive
        void ApercuConnexion()
        {
            m_champEmail.Text = "joueur@padgref.fr";
            m_champMdp.Password = "motdepasse";
        }

        void ApercuOuvrir()
        {
            var r = new ReponseWeb
            {
                Ok = true, Email = "joueur@padgref.fr", Pseudo = "Beny",
                Comptes = new List<CompteJeu>
                {
                    new CompteJeu { Id = 1, Login = "Pommes15", Pseudo = "Pommes", Personnages = { new Personnage { Nom = "Pelep", Classe = 12, Niveau = 200, Depuis = 3, Apparence = "{1|120,2188|1=32235200,2=38751540,3=58004758,4=82566848,5=87901995|160}" } } },
                    new CompteJeu { Id = 2, Login = "Pommejte9", Pseudo = "Pommejte", Personnages = { new Personnage { Nom = "Otifapu", Classe = 7, Sexe = 1, Niveau = 187, Depuis = 22, Apparence = "{1|71,2116,238,146|1=14785337,2=16707772,3=16707772,4=11941144,5=15388778|115}" }, new Personnage { Nom = "Mocahe", Classe = 9, Niveau = 156 } } },
                    new CompteJeu { Id = 3, Login = "pvpc", Pseudo = "Huser", Personnages = { new Personnage { Nom = "Mocahe", Classe = 9, Niveau = 156, Depuis = 75, Apparence = "{1|90,2140,238,146|1=16762983,2=14774542,3=5792768,4=5845776,5=16422159|140}" } } },
                    new CompteJeu { Id = 4, Login = "xp6", Pseudo = "Jujuben", Personnages = { new Personnage { Nom = "Mecabixuf", Classe = 7, Sexe = 1, Niveau = 133, Depuis = 1100, Apparence = "{1|71,2116|1=33342321,2=50307866,3=56324130,4=73112394,5=100655420|115}" } } },
                    new CompteJeu { Id = 5, Login = "xp5", Pseudo = "Caros" }
                }
            };
            var looks = new[] { r.Comptes[0].Personnages[0].Apparence, r.Comptes[1].Personnages[0].Apparence, r.Comptes[2].Personnages[0].Apparence, r.Comptes[3].Personnages[0].Apparence };
            var noms = new[] { "Portos", "Pomme-jungle", "Melan", "Phobelle", "Wyllin-Daya", "Damy", "Cyria" };
            for (var i = 0; i < noms.Length; i++)
                r.Comptes.Add(new CompteJeu { Id = 10 + i, Login = "pvp" + (char)('a' + i), Pseudo = noms[i], Personnages = { new Personnage { Nom = noms[i], Classe = 1 + i % 18, Niveau = 140 + i * 4, Depuis = 5 + i * 37, Apparence = looks[i % 4] } } });
            OuvrirPrincipal(r, null, false);
        }
    }
}
