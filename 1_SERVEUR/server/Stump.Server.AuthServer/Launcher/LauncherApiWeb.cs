// MYSTIC 18/10/2026 : comptes web Mystic pour le launcher (un compte web regroupe les comptes de jeu créés depuis le site).
//   POST /web/connexion   email, motdepasse, souvenir (0/1) -> session web + comptes de jeu liés (et leurs personnages)
//   POST /web/session     session                          -> idem, sans nouvelle session
//   POST /web/ticket      session, compte (id du compte de jeu lié) -> ticket à usage unique pour ce compte (connexion auto en jeu)
//   POST /web/deconnexion session
// Mots de passe web : PBKDF2-SHA256 « pbkdf2_sha256$<itérations>$<sel base64>$<empreinte base64> », écrits par le site
// (_OUTILS/web/site/mystic/api/compte_web.php) et vérifiés ici. Même sécurité que l'API du launcher : SQL paramétré,
// limite d'échecs par IP / par e-mail, sessions stockées en SHA-256.

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using MySql.Data.MySqlClient;

namespace Stump.Server.AuthServer.Launcher
{
    static class CompteWeb
    {
        public const string TABLES = @"
CREATE TABLE IF NOT EXISTS nekfeu_comptes_web (
    Id INT NOT NULL AUTO_INCREMENT PRIMARY KEY, Email VARCHAR(190) NOT NULL, Pseudo VARCHAR(40) NOT NULL,
    MotDePasse VARCHAR(255) NOT NULL, CreeLe DATETIME NOT NULL, DerniereConnexion DATETIME NULL,
    Bloque TINYINT NOT NULL DEFAULT 0, UNIQUE KEY (Email), UNIQUE KEY (Pseudo)) DEFAULT CHARSET=utf8;
CREATE TABLE IF NOT EXISTS nekfeu_comptes_web_jeu (
    AccountId INT NOT NULL PRIMARY KEY, CompteWebId INT NOT NULL, LieLe DATETIME NOT NULL, KEY (CompteWebId)) DEFAULT CHARSET=utf8;
CREATE TABLE IF NOT EXISTS nekfeu_web_sessions (
    Empreinte CHAR(64) NOT NULL PRIMARY KEY, CompteWebId INT NOT NULL, CreeLe DATETIME NOT NULL, ExpireLe DATETIME NOT NULL,
    DerniereIp VARCHAR(64) NOT NULL DEFAULT '', KEY (CompteWebId)) DEFAULT CHARSET=utf8;
CREATE TABLE IF NOT EXISTS nekfeu_partages (
    AccountId INT NOT NULL, CompteWebId INT NOT NULL, ProprietaireWebId INT NOT NULL, CreeLe DATETIME NOT NULL,
    PRIMARY KEY (AccountId, CompteWebId), KEY (CompteWebId), KEY (ProprietaireWebId)) DEFAULT CHARSET=utf8;
CREATE TABLE IF NOT EXISTS nekfeu_partages_codes (
    Empreinte CHAR(64) NOT NULL PRIMARY KEY, ProprietaireWebId INT NOT NULL, Comptes VARCHAR(255) NOT NULL,
    ExpireLe DATETIME NOT NULL, KEY (ProprietaireWebId)) DEFAULT CHARSET=utf8;";

        /// <summary>Vérifie un mot de passe contre « pbkdf2_sha256$it$sel$empreinte ».</summary>
        public static bool Verifier(string motDePasse, string stocke)
        {
            try
            {
                var p = (stocke ?? "").Split('$');
                if (p.Length != 4 || p[0] != "pbkdf2_sha256")
                    return false;
                var iterations = int.Parse(p[1]);
                if (iterations < 10000 || iterations > 2000000)
                    return false;
                var sel = Convert.FromBase64String(p[2]);
                var attendu = Convert.FromBase64String(p[3]);
                using (var kdf = new Rfc2898DeriveBytes(Encoding.UTF8.GetBytes(motDePasse), sel, iterations, HashAlgorithmName.SHA256))
                {
                    var calcule = kdf.GetBytes(attendu.Length);
                    var diff = 0;
                    for (var i = 0; i < attendu.Length; i++)
                        diff |= calcule[i] ^ attendu[i];
                    return diff == 0;
                }
            }
            catch
            {
                return false;
            }
        }
    }

    public partial class LauncherApi
    {
        string WebConnexion(Dictionary<string, string> form, string ip, ref int code)
        {
            form.TryGetValue("email", out var email);
            form.TryGetValue("motdepasse", out var motDePasse);
            email = (email ?? "").Trim();
            if (email.Length == 0 || string.IsNullOrEmpty(motDePasse))
                return Erreur("Indiquez votre e-mail et votre mot de passe.");

            var cleCompte = "web:" + email.ToLowerInvariant();
            if (Bloque("ip:" + ip, ECHECS_MAX_IP) || Bloque(cleCompte, ECHECS_MAX_COMPTE))
            {
                code = 429;
                return Erreur("Trop de tentatives. Réessayez dans quelques minutes.");
            }

            using (var c = Connexion())
            {
                int id;
                string pseudo, hash;
                bool bloque;
                using (var cmd = new MySqlCommand("SELECT Id, Pseudo, MotDePasse, Bloque FROM nekfeu_comptes_web WHERE Email = @e LIMIT 1", c))
                {
                    cmd.Parameters.AddWithValue("@e", email);
                    using (var r = cmd.ExecuteReader())
                    {
                        if (!r.Read())
                        {
                            Echec("ip:" + ip);
                            Echec(cleCompte);
                            return Erreur("E-mail ou mot de passe incorrect.");
                        }
                        id = r.GetInt32(0);
                        pseudo = r.GetString(1);
                        hash = r.GetString(2);
                        bloque = r.GetBoolean(3);
                    }
                }
                if (!CompteWeb.Verifier(motDePasse, hash))
                {
                    Echec("ip:" + ip);
                    Echec(cleCompte);
                    logger.Info("[Launcher] échec de connexion web pour {0} depuis {1}", email, ip);
                    return Erreur("E-mail ou mot de passe incorrect.");
                }
                if (bloque)
                    return Erreur("Ce compte web est bloqué.");

                var souvenir = form.TryGetValue("souvenir", out var s) && s == "1";
                var session = Aleatoire(32);
                using (var cmd = new MySqlCommand("INSERT INTO nekfeu_web_sessions (Empreinte, CompteWebId, CreeLe, ExpireLe, DerniereIp) VALUES (@e, @a, NOW(), @x, @ip)", c))
                {
                    cmd.Parameters.AddWithValue("@e", Sha256(session));
                    cmd.Parameters.AddWithValue("@a", id);
                    cmd.Parameters.AddWithValue("@x", souvenir ? DateTime.Now.AddDays(DureeSessionJours) : DateTime.Now.AddHours(12));
                    cmd.Parameters.AddWithValue("@ip", ip);
                    cmd.ExecuteNonQuery();
                }
                using (var cmd = new MySqlCommand("UPDATE nekfeu_comptes_web SET DerniereConnexion = NOW() WHERE Id = @a; DELETE FROM nekfeu_web_sessions WHERE ExpireLe < NOW()", c))
                {
                    cmd.Parameters.AddWithValue("@a", id);
                    cmd.ExecuteNonQuery();
                }

                logger.Info("[Launcher] compte web {0} connecté depuis {1}", email, ip);
                return ReponseWeb(c, id, email, pseudo, session);
            }
        }

        /// <summary>Compte web de la session (0 si invalide / expirée).</summary>
        int CompteDeSession(MySqlConnection c, string session, string ip)
        {
            if (string.IsNullOrEmpty(session) || session.Length != 64)
                return 0;
            using (var cmd = new MySqlCommand("SELECT s.CompteWebId FROM nekfeu_web_sessions s JOIN nekfeu_comptes_web w ON w.Id = s.CompteWebId " +
                                              "WHERE s.Empreinte = @e AND s.ExpireLe > NOW() AND w.Bloque = 0 LIMIT 1", c))
            {
                cmd.Parameters.AddWithValue("@e", Sha256(session));
                var o = cmd.ExecuteScalar();
                if (o == null)
                {
                    Echec("ip:" + ip);
                    return 0;
                }
                return Convert.ToInt32(o);
            }
        }

        string WebSession(Dictionary<string, string> form, string ip, ref int code)
        {
            if (Bloque("ip:" + ip, ECHECS_MAX_IP))
            {
                code = 429;
                return Erreur("Trop de tentatives. Réessayez dans quelques minutes.");
            }
            form.TryGetValue("session", out var session);
            using (var c = Connexion())
            {
                var id = CompteDeSession(c, session, ip);
                if (id == 0)
                    return Erreur("Session expirée, reconnectez-vous.");
                string email, pseudo;
                using (var cmd = new MySqlCommand("SELECT Email, Pseudo FROM nekfeu_comptes_web WHERE Id = @a", c))
                {
                    cmd.Parameters.AddWithValue("@a", id);
                    using (var r = cmd.ExecuteReader())
                    {
                        r.Read();
                        email = r.GetString(0);
                        pseudo = r.GetString(1);
                    }
                }
                using (var cmd = new MySqlCommand("UPDATE nekfeu_web_sessions SET DerniereIp = @ip WHERE Empreinte = @e", c))
                {
                    cmd.Parameters.AddWithValue("@ip", ip);
                    cmd.Parameters.AddWithValue("@e", Sha256(session));
                    cmd.ExecuteNonQuery();
                }
                return ReponseWeb(c, id, email, pseudo, null);
            }
        }

        string WebTicket(Dictionary<string, string> form, string ip, ref int code)
        {
            if (Bloque("ip:" + ip, ECHECS_MAX_IP))
            {
                code = 429;
                return Erreur("Trop de tentatives. Réessayez dans quelques minutes.");
            }
            form.TryGetValue("session", out var session);
            if (!form.TryGetValue("compte", out var compteTexte) || !int.TryParse(compteTexte, out var accountId))
                return Erreur("Compte de jeu inconnu.");

            using (var c = Connexion())
            {
                var webId = CompteDeSession(c, session, ip);
                if (webId == 0)
                    return Erreur("Session expirée, reconnectez-vous.");

                // le compte de jeu doit appartenir à ce compte web
                // MYSTIC 18/10 : ou partagé avec lui (code de partage)
                using (var cmd = new MySqlCommand("SELECT a.Login, a.IsBanned, a.BanEndDate FROM accounts a WHERE a.Id = @a AND (" +
                                                  "EXISTS (SELECT 1 FROM nekfeu_comptes_web_jeu l WHERE l.AccountId = a.Id AND l.CompteWebId = @w) OR " +
                                                  "EXISTS (SELECT 1 FROM nekfeu_partages p WHERE p.AccountId = a.Id AND p.CompteWebId = @w)) LIMIT 1", c))
                {
                    cmd.Parameters.AddWithValue("@a", accountId);
                    cmd.Parameters.AddWithValue("@w", webId);
                    using (var r = cmd.ExecuteReader())
                    {
                        if (!r.Read())
                            return Erreur("Ce compte de jeu n'est pas lié à votre compte web.");
                        var login = r.GetString(0);
                        var banni = !r.IsDBNull(1) && r.GetBoolean(1);
                        var finBan = r.IsDBNull(2) ? (DateTime?)null : r.GetDateTime(2);
                        if (banni && (finBan == null || finBan > DateTime.Now))
                            return Erreur(finBan == null ? "Ce compte de jeu est banni." : "Ce compte de jeu est banni jusqu'au " + finBan.Value.ToString("dd/MM/yyyy HH:mm") + ".");
                        return "{\"ok\":true,\"login\":" + Json(login) + ",\"ticket\":" + Json(NouveauTicket(accountId, login)) + "}";
                    }
                }
            }
        }

        string WebDeconnexion(Dictionary<string, string> form)
        {
            if (form.TryGetValue("session", out var session) && session.Length == 64)
                using (var c = Connexion())
                using (var cmd = new MySqlCommand("DELETE FROM nekfeu_web_sessions WHERE Empreinte = @e", c))
                {
                    cmd.Parameters.AddWithValue("@e", Sha256(session));
                    cmd.ExecuteNonQuery();
                }
            return "{\"ok\":true}";
        }

        string ReponseWeb(MySqlConnection c, int webId, string email, string pseudo, string session)
        {
            var comptes = new List<Tuple<int, string, string>>();
            var proprietaires = new Dictionary<int, string>(); // comptes reçus en partage -> pseudo du propriétaire
            using (var cmd = new MySqlCommand("SELECT a.Id, a.Login, a.Nickname, NULL FROM nekfeu_comptes_web_jeu l JOIN accounts a ON a.Id = l.AccountId " +
                                              "WHERE l.CompteWebId = @w " +
                                              "UNION ALL SELECT a.Id, a.Login, a.Nickname, w.Pseudo FROM nekfeu_partages p JOIN accounts a ON a.Id = p.AccountId " +
                                              "JOIN nekfeu_comptes_web w ON w.Id = p.ProprietaireWebId WHERE p.CompteWebId = @w", c))
            {
                cmd.Parameters.AddWithValue("@w", webId);
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                    {
                        comptes.Add(Tuple.Create(r.GetInt32(0), r.GetString(1), r.IsDBNull(2) ? r.GetString(1) : r.GetString(2)));
                        if (!r.IsDBNull(3))
                            proprietaires[r.GetInt32(0)] = r.GetString(3);
                    }
            }

            var sb = new StringBuilder();
            sb.Append("{\"ok\":true,\"email\":").Append(Json(email)).Append(",\"pseudo\":").Append(Json(pseudo));
            if (session != null)
                sb.Append(",\"session\":").Append(Json(session));
            sb.Append(",\"comptes\":[");
            for (var i = 0; i < comptes.Count; i++)
            {
                if (i > 0) sb.Append(',');
                sb.Append("{\"id\":").Append(comptes[i].Item1).Append(",\"login\":").Append(Json(comptes[i].Item2))
                  .Append(",\"pseudo\":").Append(Json(comptes[i].Item3));
                if (proprietaires.TryGetValue(comptes[i].Item1, out var proprio))
                    sb.Append(",\"partagePar\":").Append(Json(proprio));
                sb.Append(",\"personnages\":");
                AjouterPersonnages(sb, c, comptes[i].Item1);
                sb.Append('}');
            }
            sb.Append("]}");
            return sb.ToString();
        }
    }
}
