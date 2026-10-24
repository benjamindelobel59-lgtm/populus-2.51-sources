// MYSTIC 18/10/2026 : API du launcher Mystic (connexion « type Zaap »).
// Petit serveur HTTP intégré à l'AuthServer (port LauncherApi.Port) :
//   GET  /statut                         -> serveur en ligne + nombre de joueurs connectés
//   POST /connexion   login, motdepasse, souvenir (0/1)
//   POST /session     session            -> reconnexion « Se souvenir de moi » (aucun mot de passe stocké)
//   POST /deconnexion session            -> révoque la session
// Réponse de connexion : pseudo, personnages du compte (nom, niveau, classe, sexe) et un ticket à usage unique (2 min) que le
// launcher passe au client (--username=… --ticket=…). Le patch de connexion envoie ce ticket comme mot de passe, préfixé par
// PREFIXE_TICKET, et CredentialManager le vérifie ici.
// Sécurité : corps limité à 4 Ko (formulaire url-encoded), requêtes SQL uniquement paramétrées (aucune valeur du joueur dans le
// texte SQL), connexion MySQL dédiée (jamais celle du serveur de jeu), sessions stockées en SHA-256, limite d'échecs par IP et
// par compte, comparaisons en temps constant.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using NLog;
using Stump.Core.Attributes;
using Stump.Core.Extensions;
using Stump.Core.Reflection;
using Stump.DofusProtocol.Enums;
using Stump.Server.AuthServer.Managers;
using Stump.Server.BaseServer.Initialization;

namespace Stump.Server.AuthServer.Launcher
{
    public partial class LauncherApi : Singleton<LauncherApi>
    {
        static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public const string PREFIXE_TICKET = "#mystic-ticket:";

        [Variable(true)]
        public static bool Actif = true;

        [Variable(true)]
        public static int Port = 8090;

        /// <summary>Nom de la base du monde (personnages). Lu dans la config, jamais venant d'un joueur.</summary>
        [Variable(true)]
        public static string BaseMonde = "world";

        [Variable(true)]
        public static int DureeSessionJours = 30;

        const int TAILLE_MAX_CORPS = 4096;
        const int ECHECS_MAX_IP = 8;
        const int ECHECS_MAX_COMPTE = 10;
        static readonly TimeSpan FENETRE_ECHECS = TimeSpan.FromMinutes(10);
        static readonly TimeSpan DUREE_TICKET = TimeSpan.FromMinutes(2);

        class Ticket
        {
            public int AccountId;
            public string Login;
            public DateTime Expire;
        }

        readonly ConcurrentDictionary<string, Ticket> m_tickets = new ConcurrentDictionary<string, Ticket>();
        readonly ConcurrentDictionary<string, List<DateTime>> m_echecs = new ConcurrentDictionary<string, List<DateTime>>();
        TcpListener m_ecoute;
        string m_chaineConnexion;

        [Initialization(InitializationPass.Last)]
        static void Initialiser() => Instance.Demarrer();

        void Demarrer()
        {
            if (!Actif)
                return;
            if (!Regex.IsMatch(BaseMonde ?? "", "^[A-Za-z0-9_]+$"))
            {
                logger.Error("[Launcher] nom de base du monde invalide : {0}", BaseMonde);
                return;
            }

            try
            {
                m_chaineConnexion = AuthServer.DatabaseConfiguration.GetConnectionString();
                using (var c = Connexion())
                {
                    using (var cmd = new MySqlCommand(@"CREATE TABLE IF NOT EXISTS nekfeu_launcher_sessions (
                        Empreinte CHAR(64) NOT NULL PRIMARY KEY, AccountId INT NOT NULL, CreeLe DATETIME NOT NULL,
                        ExpireLe DATETIME NOT NULL, DerniereIp VARCHAR(64) NOT NULL DEFAULT '', KEY (AccountId)) DEFAULT CHARSET=utf8", c))
                        cmd.ExecuteNonQuery();
                    // MYSTIC 18/10 : comptes web (site + launcher) ; mêmes définitions dans _OUTILS/web/site/mystic/api/compte_web.php
                    using (var cmd = new MySqlCommand(CompteWeb.TABLES, c))
                        cmd.ExecuteNonQuery();
                }

                m_ecoute = new TcpListener(IPAddress.Any, Port);
                m_ecoute.Start();
                new Thread(Boucle) { IsBackground = true, Name = "LauncherApi" }.Start();
                logger.Info("[Launcher] API du launcher sur le port {0}", Port);
            }
            catch (Exception ex)
            {
                logger.Error("[Launcher] démarrage impossible : {0}", ex.Message);
            }
        }

        MySqlConnection Connexion()
        {
            var c = new MySqlConnection(m_chaineConnexion);
            c.Open();
            return c;
        }

        // ------------------------------------------------------------------ tickets (appelé par CredentialManager)

        /// <summary>Vérifie et consomme un ticket du launcher (usage unique). Le login doit être celui du compte du ticket.</summary>
        /// <summary>PADGREF 21/10 : compte du ticket (usage unique, non expiré), null sinon. Le ticket (24 octets aléatoires) suffit à
        /// prouver le compte : après « Déconnecter », le jeu renvoie l'identifiant mémorisé de l'ancien écran de connexion (autre compte)
        /// au lieu de celui du launcher, la connexion était refusée (« identifiant ou mot de passe incorrect »).</summary>
        public string LoginDuTicket(string ticket)
        {
            if (string.IsNullOrEmpty(ticket) || !m_tickets.TryRemove(ticket, out var t))
                return null;
            return t.Expire > DateTime.Now ? t.Login : null;
        }

        public bool ConsommerTicket(string login, string ticket)
        {
            if (string.IsNullOrEmpty(ticket) || !m_tickets.TryRemove(ticket, out var t))
                return false;
            return t.Expire > DateTime.Now && string.Equals(t.Login, login, StringComparison.OrdinalIgnoreCase);
        }

        string NouveauTicket(int accountId, string login)
        {
            foreach (var vieux in m_tickets.Where(x => x.Value.Expire < DateTime.Now).Select(x => x.Key).ToList())
                m_tickets.TryRemove(vieux, out _);
            var ticket = Aleatoire(24);
            m_tickets[ticket] = new Ticket { AccountId = accountId, Login = login, Expire = DateTime.Now + DUREE_TICKET };
            return ticket;
        }

        static string Aleatoire(int octets)
        {
            var b = new byte[octets];
            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(b);
            return string.Concat(b.Select(x => x.ToString("x2")));
        }

        /// <summary>MYSTIC 20/10 : apparence sans la monture (monté : la monture porte le personnage en sous-entité « 2@N={…} », catégorie cavalier).</summary>
        static string SansMonture(string look)
        {
            if (string.IsNullOrEmpty(look)) return look;
            var prof = 0;
            for (var i = 0; i < look.Length; i++)
            {
                var c = look[i];
                if (c == '{') prof++;
                else if (c == '}') prof--;
                else if (prof == 1 && (c == '|' || c == ',') && i + 3 < look.Length && look[i + 1] == '2' && look[i + 2] == '@')
                {
                    var eg = look.IndexOf('=', i + 3);
                    if (eg < 0 || eg + 1 >= look.Length || look[eg + 1] != '{') continue;
                    var ok = true;
                    for (var k = i + 3; k < eg; k++) if (!char.IsDigit(look[k])) { ok = false; break; }
                    if (!ok) continue;
                    var p = 0;
                    for (var j = eg + 1; j < look.Length; j++)
                    {
                        if (look[j] == '{') p++;
                        else if (look[j] == '}' && --p == 0) return look.Substring(eg + 1, j - eg);
                    }
                    return look;
                }
            }
            return look;
        }

        static string Sha256(string s)
        {
            using (var sha = SHA256.Create())
                return string.Concat(sha.ComputeHash(Encoding.UTF8.GetBytes(s)).Select(x => x.ToString("x2")));
        }

        static bool EgalTempsConstant(string a, string b)
        {
            if (a == null || b == null)
                return false;
            var x = Encoding.UTF8.GetBytes(a);
            var y = Encoding.UTF8.GetBytes(b);
            var diff = x.Length ^ y.Length;
            for (var i = 0; i < Math.Min(x.Length, y.Length); i++)
                diff |= x[i] ^ y[i];
            return diff == 0;
        }

        // ------------------------------------------------------------------ limite d'échecs

        bool Bloque(string cle, int max)
        {
            if (!m_echecs.TryGetValue(cle, out var l))
                return false;
            lock (l)
            {
                l.RemoveAll(d => d < DateTime.Now - FENETRE_ECHECS);
                return l.Count >= max;
            }
        }

        void Echec(string cle)
        {
            var l = m_echecs.GetOrAdd(cle, _ => new List<DateTime>());
            lock (l)
                l.Add(DateTime.Now);
        }

        // ------------------------------------------------------------------ serveur HTTP minimal

        void Boucle()
        {
            while (true)
            {
                try
                {
                    var client = m_ecoute.AcceptTcpClient();
                    Task.Run(() => Traiter(client));
                }
                catch (Exception ex)
                {
                    logger.Error("[Launcher] écoute : {0}", ex.Message);
                    Thread.Sleep(1000);
                }
            }
        }

        void Traiter(TcpClient client)
        {
            using (client)
            {
                try
                {
                    client.ReceiveTimeout = 5000;
                    client.SendTimeout = 5000;
                    var flux = client.GetStream();
                    var ip = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();

                    // en-têtes (8 Ko max)
                    var tampon = new MemoryStream();
                    var fin = -1;
                    var buf = new byte[1024];
                    while (fin < 0 && tampon.Length < 8192)
                    {
                        var n = flux.Read(buf, 0, buf.Length);
                        if (n <= 0)
                            return;
                        tampon.Write(buf, 0, n);
                        fin = IndexFinEntetes(tampon.GetBuffer(), (int)tampon.Length);
                    }
                    if (fin < 0)
                    {
                        Repondre(flux, 431, Erreur("Requête trop grande."));
                        return;
                    }

                    var entetes = Encoding.ASCII.GetString(tampon.GetBuffer(), 0, fin).Split(new[] { "\r\n" }, StringSplitOptions.None);
                    var premiere = entetes[0].Split(' ');
                    if (premiere.Length < 2)
                        return;
                    var methode = premiere[0];
                    var chemin = premiere[1].Split('?')[0].ToLowerInvariant();

                    var longueur = 0;
                    foreach (var h in entetes.Skip(1))
                        if (h.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase))
                            int.TryParse(h.Substring(15).Trim(), out longueur);
                    if (longueur < 0 || longueur > TAILLE_MAX_CORPS)
                    {
                        Repondre(flux, 413, Erreur("Requête trop grande."));
                        return;
                    }

                    var corps = new byte[longueur];
                    var deja = (int)tampon.Length - (fin + 4);
                    Array.Copy(tampon.GetBuffer(), fin + 4, corps, 0, Math.Min(deja, longueur));
                    var lus = Math.Min(deja, longueur);
                    while (lus < longueur)
                    {
                        var n = flux.Read(corps, lus, longueur - lus);
                        if (n <= 0)
                            break;
                        lus += n;
                    }
                    var form = LireFormulaire(Encoding.UTF8.GetString(corps, 0, lus));

                    string reponse;
                    var code = 200;
                    if (methode == "GET" && chemin == "/statut")
                        reponse = Statut();
                    else if (methode == "POST" && chemin == "/connexion")
                        reponse = Connexion(form, ip, ref code);
                    else if (methode == "POST" && chemin == "/session")
                        reponse = Session(form, ip, ref code);
                    else if (methode == "POST" && chemin == "/deconnexion")
                        reponse = Deconnexion(form);
                    else if (methode == "POST" && chemin == "/web/connexion")
                        reponse = WebConnexion(form, ip, ref code);
                    else if (methode == "POST" && chemin == "/web/session")
                        reponse = WebSession(form, ip, ref code);
                    else if (methode == "POST" && chemin == "/web/ticket")
                        reponse = WebTicket(form, ip, ref code);
                    else if (methode == "POST" && chemin == "/web/deconnexion")
                        reponse = WebDeconnexion(form);
                    else if (methode == "POST" && chemin == "/web/creer_jeu")
                        reponse = WebCreerJeu(form, ip, ref code);
                    else if (methode == "POST" && chemin == "/web/appairage")
                        reponse = WebAppairage(form, ip, ref code);
                    else if (methode == "POST" && chemin == "/web/partage/creer")
                        reponse = PartageCreer(form, ip, ref code);
                    else if (methode == "POST" && chemin == "/web/partage/annuler")
                        reponse = PartageAnnuler(form, ip, ref code);
                    else if (methode == "POST" && chemin == "/web/partage/utiliser")
                        reponse = PartageUtiliser(form, ip, ref code);
                    else if (methode == "POST" && chemin == "/web/partage/liste")
                        reponse = PartageListe(form, ip, ref code);
                    else if (methode == "POST" && chemin == "/web/partage/retirer")
                        reponse = PartageRetirer(form, ip, ref code);
                    else
                    {
                        code = 404;
                        reponse = Erreur("Inconnu.");
                    }
                    Repondre(flux, code, reponse);
                }
                catch (Exception ex)
                {
                    logger.Warn("[Launcher] requête : {0}", ex.Message);
                }
            }
        }

        static int IndexFinEntetes(byte[] b, int n)
        {
            for (var i = 0; i + 3 < n; i++)
                if (b[i] == '\r' && b[i + 1] == '\n' && b[i + 2] == '\r' && b[i + 3] == '\n')
                    return i;
            return -1;
        }

        static Dictionary<string, string> LireFormulaire(string s)
        {
            var d = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var paire in s.Split('&'))
            {
                var i = paire.IndexOf('=');
                if (i <= 0)
                    continue;
                var k = Uri.UnescapeDataString(paire.Substring(0, i).Replace('+', ' '));
                var v = Uri.UnescapeDataString(paire.Substring(i + 1).Replace('+', ' '));
                if (k.Length <= 32 && v.Length <= 256)
                    d[k] = v;
            }
            return d;
        }

        static void Repondre(Stream flux, int code, string json)
        {
            var corps = Encoding.UTF8.GetBytes(json);
            var texte = code == 200 ? "OK" : code == 404 ? "Not Found" : code == 429 ? "Too Many Requests" : "Error";
            var entete = "HTTP/1.1 " + code + " " + texte + "\r\nContent-Type: application/json; charset=utf-8\r\nCache-Control: no-store\r\n" +
                         "Connection: close\r\nContent-Length: " + corps.Length + "\r\n\r\n";
            var e = Encoding.ASCII.GetBytes(entete);
            flux.Write(e, 0, e.Length);
            flux.Write(corps, 0, corps.Length);
        }

        static string Json(string s)
        {
            var sb = new StringBuilder("\"");
            foreach (var c in s ?? "")
            {
                if (c == '"' || c == '\\') sb.Append('\\').Append(c);
                else if (c < 0x20) sb.AppendFormat("\\u{0:x4}", (int)c);
                else sb.Append(c);
            }
            return sb.Append('"').ToString();
        }

        static string Erreur(string message) => "{\"ok\":false,\"erreur\":" + Json(message) + "}";

        // ------------------------------------------------------------------ points d'entrée

        string Statut()
        {
            var mondes = WorldServerManager.Instance.Mondes().ToList();
            var enLigne = mondes.Where(w => w.Status == ServerStatusEnum.ONLINE).Sum(w => w.CharsCount ?? 0);
            var ouvert = mondes.Any(w => w.Status == ServerStatusEnum.ONLINE);
            return "{\"ok\":true,\"ouvert\":" + (ouvert ? "true" : "false") + ",\"enLigne\":" + enLigne + "}";
        }

        string Connexion(Dictionary<string, string> form, string ip, ref int code)
        {
            form.TryGetValue("login", out var login);
            form.TryGetValue("motdepasse", out var motDePasse);
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(motDePasse))
                return Erreur("Indiquez votre compte et votre mot de passe.");

            if (Bloque("ip:" + ip, ECHECS_MAX_IP) || Bloque("compte:" + login.ToLowerInvariant(), ECHECS_MAX_COMPTE))
            {
                code = 429;
                return Erreur("Trop de tentatives. Réessayez dans quelques minutes.");
            }

            using (var c = Connexion())
            {
                int id;
                string vraiLogin, hash, pseudo;
                bool banni;
                DateTime? finBan;
                using (var cmd = new MySqlCommand("SELECT Id, Login, PasswordHash, Nickname, IsBanned, BanEndDate FROM accounts WHERE Login = @login LIMIT 1", c))
                {
                    cmd.Parameters.AddWithValue("@login", login);
                    using (var r = cmd.ExecuteReader())
                    {
                        if (!r.Read())
                        {
                            Echec("ip:" + ip);
                            Echec("compte:" + login.ToLowerInvariant());
                            return Erreur("Compte ou mot de passe incorrect.");
                        }
                        id = r.GetInt32(0);
                        vraiLogin = r.GetString(1);
                        hash = r.IsDBNull(2) ? "" : r.GetString(2);
                        pseudo = r.IsDBNull(3) ? vraiLogin : r.GetString(3);
                        banni = !r.IsDBNull(4) && r.GetBoolean(4);
                        finBan = r.IsDBNull(5) ? (DateTime?)null : r.GetDateTime(5);
                    }
                }

                // même règle que CredentialManager : mot de passe en clair, ou ancien MD5
                if (!EgalTempsConstant(hash, motDePasse) && !EgalTempsConstant(hash, motDePasse.GetMD5()))
                {
                    Echec("ip:" + ip);
                    Echec("compte:" + login.ToLowerInvariant());
                    logger.Info("[Launcher] échec de connexion pour {0} depuis {1}", login, ip);
                    return Erreur("Compte ou mot de passe incorrect.");
                }
                if (banni && (finBan == null || finBan > DateTime.Now))
                    return Erreur(finBan == null ? "Ce compte est banni." : "Ce compte est banni jusqu'au " + finBan.Value.ToString("dd/MM/yyyy HH:mm") + ".");

                // session toujours créée (ticket frais au clic sur JOUER) ; « Se souvenir de moi » = 30 jours et gardée par le
                // launcher, sinon 12 h et révoquée par le launcher à sa fermeture
                var souvenir = form.TryGetValue("souvenir", out var s) && s == "1";
                var session = Aleatoire(32);
                using (var cmd = new MySqlCommand("INSERT INTO nekfeu_launcher_sessions (Empreinte, AccountId, CreeLe, ExpireLe, DerniereIp) VALUES (@e, @a, NOW(), @x, @ip)", c))
                {
                    cmd.Parameters.AddWithValue("@e", Sha256(session));
                    cmd.Parameters.AddWithValue("@a", id);
                    cmd.Parameters.AddWithValue("@x", souvenir ? DateTime.Now.AddDays(DureeSessionJours) : DateTime.Now.AddHours(12));
                    cmd.Parameters.AddWithValue("@ip", ip);
                    cmd.ExecuteNonQuery();
                }
                using (var cmd = new MySqlCommand("DELETE FROM nekfeu_launcher_sessions WHERE ExpireLe < NOW()", c))
                    cmd.ExecuteNonQuery();

                logger.Info("[Launcher] {0} connecté depuis {1}", vraiLogin, ip);
                return Reponse(c, id, vraiLogin, pseudo, session);
            }
        }

        string Session(Dictionary<string, string> form, string ip, ref int code)
        {
            if (!form.TryGetValue("session", out var session) || session.Length != 64)
                return Erreur("Session expirée, reconnectez-vous.");
            if (Bloque("ip:" + ip, ECHECS_MAX_IP))
            {
                code = 429;
                return Erreur("Trop de tentatives. Réessayez dans quelques minutes.");
            }

            using (var c = Connexion())
            {
                int id;
                using (var cmd = new MySqlCommand("SELECT AccountId FROM nekfeu_launcher_sessions WHERE Empreinte = @e AND ExpireLe > NOW() LIMIT 1", c))
                {
                    cmd.Parameters.AddWithValue("@e", Sha256(session));
                    var o = cmd.ExecuteScalar();
                    if (o == null)
                    {
                        Echec("ip:" + ip);
                        return Erreur("Session expirée, reconnectez-vous.");
                    }
                    id = Convert.ToInt32(o);
                }

                string login, pseudo;
                bool banni;
                DateTime? finBan;
                using (var cmd = new MySqlCommand("SELECT Login, Nickname, IsBanned, BanEndDate FROM accounts WHERE Id = @a LIMIT 1", c))
                {
                    cmd.Parameters.AddWithValue("@a", id);
                    using (var r = cmd.ExecuteReader())
                    {
                        if (!r.Read())
                            return Erreur("Session expirée, reconnectez-vous.");
                        login = r.GetString(0);
                        pseudo = r.IsDBNull(1) ? login : r.GetString(1);
                        banni = !r.IsDBNull(2) && r.GetBoolean(2);
                        finBan = r.IsDBNull(3) ? (DateTime?)null : r.GetDateTime(3);
                    }
                }
                if (banni && (finBan == null || finBan > DateTime.Now))
                    return Erreur("Ce compte est banni.");

                using (var cmd = new MySqlCommand("UPDATE nekfeu_launcher_sessions SET DerniereIp = @ip WHERE Empreinte = @e", c))
                {
                    cmd.Parameters.AddWithValue("@ip", ip);
                    cmd.Parameters.AddWithValue("@e", Sha256(session));
                    cmd.ExecuteNonQuery();
                }
                return Reponse(c, id, login, pseudo, null);
            }
        }

        string Deconnexion(Dictionary<string, string> form)
        {
            if (form.TryGetValue("session", out var session) && session.Length == 64)
                using (var c = Connexion())
                using (var cmd = new MySqlCommand("DELETE FROM nekfeu_launcher_sessions WHERE Empreinte = @e", c))
                {
                    cmd.Parameters.AddWithValue("@e", Sha256(session));
                    cmd.ExecuteNonQuery();
                }
            return "{\"ok\":true}";
        }

        string Reponse(MySqlConnection c, int accountId, string login, string pseudo, string session)
        {
            var sb = new StringBuilder();
            sb.Append("{\"ok\":true,\"login\":").Append(Json(login)).Append(",\"pseudo\":").Append(Json(pseudo));
            sb.Append(",\"ticket\":").Append(Json(NouveauTicket(accountId, login)));
            if (session != null)
                sb.Append(",\"session\":").Append(Json(session));
            sb.Append(",\"personnages\":");
            AjouterPersonnages(sb, c, accountId);
            sb.Append('}');
            return sb.ToString();
        }

        /// <summary>Ajoute le tableau JSON des personnages du compte de jeu (20 au plus, le dernier joué en premier).</summary>
        void AjouterPersonnages(StringBuilder sb, MySqlConnection c, int accountId)
        {
            sb.Append('[');
            // BaseMonde vient de la config (vérifié au démarrage), jamais d'un joueur ; l'id du compte est paramétré
            // MYSTIC 18/10 : apparence (rendu officiel dans le launcher) et minutes depuis la dernière connexion du personnage
            var sql = "SELECT c.Name, c.Breed, c.Sex, " +
                      "(SELECT IFNULL(MAX(e.Level), 1) FROM `" + BaseMonde + "`.experiences e WHERE e.CharacterExp <= c.Experience) AS Niveau, " +
                      "IFNULL(NULLIF(c.LastLookString, ''), c.DefaultLookString) AS Apparence, " +
                      "IFNULL(TIMESTAMPDIFF(MINUTE, c.LastUsage, NOW()), -1) AS Depuis " +
                      // MYSTIC 18/10 : le WorldServer laisse characters.AccountId à 0 ; le vrai propriétaire est dans auth.worlds_characters
                      "FROM `" + BaseMonde + "`.characters c WHERE (c.AccountId = @a OR c.Id IN (SELECT w.CharacterId FROM worlds_characters w WHERE w.AccountId = @a)) " +
                      "AND c.DeletedDate IS NULL ORDER BY c.LastUsage DESC LIMIT 20";
            try
            {
                using (var cmd = new MySqlCommand(sql, c))
                {
                    cmd.Parameters.AddWithValue("@a", accountId);
                    using (var r = cmd.ExecuteReader())
                    {
                        var premier = true;
                        while (r.Read())
                        {
                            if (!premier) sb.Append(',');
                            premier = false;
                            sb.Append("{\"nom\":").Append(Json(r.GetString(0)))
                              .Append(",\"classe\":").Append(Convert.ToInt32(r.GetValue(1)).ToString(CultureInfo.InvariantCulture))
                              .Append(",\"sexe\":").Append(Convert.ToBoolean(r.GetValue(2)) ? "1" : "0")
                              .Append(",\"niveau\":").Append(Math.Min(200, Convert.ToInt32(r.GetValue(3))).ToString(CultureInfo.InvariantCulture))
                              .Append(",\"apparence\":").Append(Json(r.IsDBNull(4) ? "" : SansMonture(r.GetString(4))))
                              .Append(",\"depuis\":").Append(Convert.ToInt64(r.GetValue(5)).ToString(CultureInfo.InvariantCulture))
                              .Append('}');
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Warn("[Launcher] personnages du compte {0} : {1}", accountId, ex.Message);
            }
            sb.Append(']');
        }
    }
}
