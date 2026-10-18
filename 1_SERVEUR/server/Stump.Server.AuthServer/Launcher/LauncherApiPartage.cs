// MYSTIC 18/10/2026 : partage de comptes de jeu entre comptes web (launcher Mystic).
//   POST /web/partage/creer    session, comptes (ids séparés par des virgules, tous à soi) -> code à 6 chiffres, 15 min, usage unique
//   POST /web/partage/annuler  session, code                  -> le propriétaire annule un code pas encore utilisé
//   POST /web/partage/utiliser session, code                  -> les comptes du code sont partagés avec ce compte web
//   POST /web/partage/liste    session                        -> partages donnés (compte, receveur) et reçus (compte, propriétaire)
//   POST /web/partage/retirer  session, compte, receveur (id web, 0 = tous) -> le propriétaire retire l'accès ; le receveur se retire
// Le receveur peut seulement JOUER le compte (ticket de connexion) : jamais le mot de passe, l'e-mail ou les autres comptes du
// propriétaire. Codes tirés au hasard (RNG cryptographique), stockés en SHA-256, limite d'essais par IP / compte web.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using MySql.Data.MySqlClient;

namespace Stump.Server.AuthServer.Launcher
{
    public partial class LauncherApi
    {
        const int MAX_COMPTES_PAR_CODE = 8;
        static readonly TimeSpan DUREE_CODE = TimeSpan.FromMinutes(15);

        static string NouveauCode()
        {
            var b = new byte[4];
            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(b);
            return (BitConverter.ToUInt32(b, 0) % 1000000).ToString("000000");
        }

        bool Limite(string ip, ref int code, out string erreur)
        {
            erreur = null;
            if (!Bloque("ip:" + ip, ECHECS_MAX_IP))
                return false;
            code = 429;
            erreur = Erreur("Trop de tentatives. Réessayez dans quelques minutes.");
            return true;
        }

        string PartageCreer(Dictionary<string, string> form, string ip, ref int code)
        {
            if (Limite(ip, ref code, out var err)) return err;
            form.TryGetValue("session", out var session);
            form.TryGetValue("comptes", out var liste);
            var ids = (liste ?? "").Split(',').Select(x => int.TryParse(x.Trim(), out var v) ? v : 0).Where(x => x > 0).Distinct().ToList();
            if (ids.Count == 0 || ids.Count > MAX_COMPTES_PAR_CODE)
                return Erreur("Choisissez de 1 à " + MAX_COMPTES_PAR_CODE + " comptes à partager.");

            using (var c = Connexion())
            {
                var webId = CompteDeSession(c, session, ip);
                if (webId == 0) return Erreur("Session expirée, reconnectez-vous.");

                // seulement ses propres comptes (jamais un compte qu'on a reçu)
                var a_soi = 0;
                foreach (var id in ids)
                    using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM nekfeu_comptes_web_jeu WHERE AccountId = @a AND CompteWebId = @w", c))
                    {
                        cmd.Parameters.AddWithValue("@a", id);
                        cmd.Parameters.AddWithValue("@w", webId);
                        a_soi += Convert.ToInt32(cmd.ExecuteScalar());
                    }
                if (a_soi != ids.Count)
                    return Erreur("Vous ne pouvez partager que vos propres comptes de jeu.");

                using (var cmd = new MySqlCommand("DELETE FROM nekfeu_partages_codes WHERE ExpireLe < NOW()", c))
                    cmd.ExecuteNonQuery();
                using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM nekfeu_partages_codes WHERE ProprietaireWebId = @w", c))
                {
                    cmd.Parameters.AddWithValue("@w", webId);
                    if (Convert.ToInt32(cmd.ExecuteScalar()) >= 5)
                        return Erreur("Vous avez déjà 5 codes en cours : annulez-en un ou attendez qu'ils expirent.");
                }

                for (var essai = 0; essai < 5; essai++)
                {
                    var nouveau = NouveauCode();
                    try
                    {
                        using (var cmd = new MySqlCommand("INSERT INTO nekfeu_partages_codes (Empreinte, ProprietaireWebId, Comptes, ExpireLe) VALUES (@e, @w, @c, @x)", c))
                        {
                            cmd.Parameters.AddWithValue("@e", Sha256(nouveau));
                            cmd.Parameters.AddWithValue("@w", webId);
                            cmd.Parameters.AddWithValue("@c", string.Join(",", ids));
                            cmd.Parameters.AddWithValue("@x", DateTime.Now + DUREE_CODE);
                            cmd.ExecuteNonQuery();
                        }
                        logger.Info("[Launcher] code de partage créé par le compte web {0} ({1} compte(s))", webId, ids.Count);
                        return "{\"ok\":true,\"code\":" + Json(nouveau) + ",\"secondes\":" + (int)DUREE_CODE.TotalSeconds + "}";
                    }
                    catch (MySqlException)
                    {
                        // code déjà pris (très rare) : on en tire un autre
                    }
                }
                return Erreur("Impossible de créer un code, réessayez.");
            }
        }

        string PartageAnnuler(Dictionary<string, string> form, string ip, ref int code)
        {
            if (Limite(ip, ref code, out var err)) return err;
            form.TryGetValue("session", out var session);
            form.TryGetValue("code", out var codePartage);
            using (var c = Connexion())
            {
                var webId = CompteDeSession(c, session, ip);
                if (webId == 0) return Erreur("Session expirée, reconnectez-vous.");
                using (var cmd = new MySqlCommand("DELETE FROM nekfeu_partages_codes WHERE Empreinte = @e AND ProprietaireWebId = @w", c))
                {
                    cmd.Parameters.AddWithValue("@e", Sha256((codePartage ?? "").Trim()));
                    cmd.Parameters.AddWithValue("@w", webId);
                    cmd.ExecuteNonQuery();
                }
                return "{\"ok\":true}";
            }
        }

        string PartageUtiliser(Dictionary<string, string> form, string ip, ref int code)
        {
            if (Limite(ip, ref code, out var err)) return err;
            form.TryGetValue("session", out var session);
            form.TryGetValue("code", out var codePartage);
            codePartage = (codePartage ?? "").Trim();
            using (var c = Connexion())
            {
                var webId = CompteDeSession(c, session, ip);
                if (webId == 0) return Erreur("Session expirée, reconnectez-vous.");
                if (Bloque("partage:" + webId, ECHECS_MAX_COMPTE))
                {
                    code = 429;
                    return Erreur("Trop de codes faux. Réessayez dans quelques minutes.");
                }
                if (codePartage.Length != 6 || !codePartage.All(char.IsDigit))
                    return Erreur("Le code fait 6 chiffres.");

                int proprietaire;
                string comptes;
                using (var cmd = new MySqlCommand("SELECT ProprietaireWebId, Comptes FROM nekfeu_partages_codes WHERE Empreinte = @e AND ExpireLe > NOW() LIMIT 1", c))
                {
                    cmd.Parameters.AddWithValue("@e", Sha256(codePartage));
                    using (var r = cmd.ExecuteReader())
                    {
                        if (!r.Read())
                        {
                            Echec("ip:" + ip);
                            Echec("partage:" + webId);
                            return Erreur("Code inconnu ou expiré.");
                        }
                        proprietaire = r.GetInt32(0);
                        comptes = r.GetString(1);
                    }
                }
                if (proprietaire == webId)
                    return Erreur("Ce code partage vos propres comptes : donnez-le à un autre joueur.");

                // usage unique
                using (var cmd = new MySqlCommand("DELETE FROM nekfeu_partages_codes WHERE Empreinte = @e", c))
                {
                    cmd.Parameters.AddWithValue("@e", Sha256(codePartage));
                    cmd.ExecuteNonQuery();
                }
                var ajoutes = 0;
                foreach (var id in comptes.Split(',').Select(x => int.TryParse(x, out var v) ? v : 0).Where(x => x > 0))
                    using (var cmd = new MySqlCommand("INSERT IGNORE INTO nekfeu_partages (AccountId, CompteWebId, ProprietaireWebId, CreeLe) " +
                                                      "SELECT @a, @w, @p, NOW() FROM nekfeu_comptes_web_jeu WHERE AccountId = @a AND CompteWebId = @p", c))
                    {
                        cmd.Parameters.AddWithValue("@a", id);
                        cmd.Parameters.AddWithValue("@w", webId);
                        cmd.Parameters.AddWithValue("@p", proprietaire);
                        ajoutes += cmd.ExecuteNonQuery();
                    }
                logger.Info("[Launcher] compte web {0} : {1} compte(s) partagé(s) par le compte web {2}", webId, ajoutes, proprietaire);
                return "{\"ok\":true,\"ajoutes\":" + ajoutes + "}";
            }
        }

        string PartageListe(Dictionary<string, string> form, string ip, ref int code)
        {
            if (Limite(ip, ref code, out var err)) return err;
            form.TryGetValue("session", out var session);
            using (var c = Connexion())
            {
                var webId = CompteDeSession(c, session, ip);
                if (webId == 0) return Erreur("Session expirée, reconnectez-vous.");
                var sb = new StringBuilder("{\"ok\":true,\"donnes\":[");
                using (var cmd = new MySqlCommand("SELECT p.AccountId, a.Login, p.CompteWebId, w.Pseudo, p.CreeLe FROM nekfeu_partages p JOIN accounts a ON a.Id = p.AccountId " +
                                                  "JOIN nekfeu_comptes_web w ON w.Id = p.CompteWebId WHERE p.ProprietaireWebId = @w ORDER BY a.Login, w.Pseudo", c))
                {
                    cmd.Parameters.AddWithValue("@w", webId);
                    using (var r = cmd.ExecuteReader())
                    {
                        var premier = true;
                        while (r.Read())
                        {
                            if (!premier) sb.Append(',');
                            premier = false;
                            sb.Append("{\"compte\":").Append(r.GetInt32(0)).Append(",\"login\":").Append(Json(r.GetString(1)))
                              .Append(",\"receveur\":").Append(r.GetInt32(2)).Append(",\"pseudo\":").Append(Json(r.GetString(3)))
                              .Append(",\"depuis\":").Append(Json(r.GetDateTime(4).ToString("dd/MM/yyyy"))).Append('}');
                        }
                    }
                }
                sb.Append("],\"recus\":[");
                using (var cmd = new MySqlCommand("SELECT p.AccountId, a.Login, w.Pseudo, p.CreeLe FROM nekfeu_partages p JOIN accounts a ON a.Id = p.AccountId " +
                                                  "JOIN nekfeu_comptes_web w ON w.Id = p.ProprietaireWebId WHERE p.CompteWebId = @w ORDER BY a.Login", c))
                {
                    cmd.Parameters.AddWithValue("@w", webId);
                    using (var r = cmd.ExecuteReader())
                    {
                        var premier = true;
                        while (r.Read())
                        {
                            if (!premier) sb.Append(',');
                            premier = false;
                            sb.Append("{\"compte\":").Append(r.GetInt32(0)).Append(",\"login\":").Append(Json(r.GetString(1)))
                              .Append(",\"pseudo\":").Append(Json(r.GetString(2))).Append(",\"depuis\":").Append(Json(r.GetDateTime(3).ToString("dd/MM/yyyy"))).Append('}');
                        }
                    }
                }
                return sb.Append("]}").ToString();
            }
        }

        string PartageRetirer(Dictionary<string, string> form, string ip, ref int code)
        {
            if (Limite(ip, ref code, out var err)) return err;
            form.TryGetValue("session", out var session);
            if (!form.TryGetValue("compte", out var compteTexte) || !int.TryParse(compteTexte, out var compte))
                return Erreur("Compte inconnu.");
            form.TryGetValue("receveur", out var receveurTexte);
            int.TryParse(receveurTexte, out var receveur);
            using (var c = Connexion())
            {
                var webId = CompteDeSession(c, session, ip);
                if (webId == 0) return Erreur("Session expirée, reconnectez-vous.");
                int n;
                // propriétaire : retire l'accès d'un receveur (ou de tous) ; receveur : se retire lui-même
                using (var cmd = new MySqlCommand("DELETE FROM nekfeu_partages WHERE AccountId = @a AND (" +
                                                  "(ProprietaireWebId = @w AND (@r = 0 OR CompteWebId = @r)) OR CompteWebId = @w)", c))
                {
                    cmd.Parameters.AddWithValue("@a", compte);
                    cmd.Parameters.AddWithValue("@w", webId);
                    cmd.Parameters.AddWithValue("@r", receveur);
                    n = cmd.ExecuteNonQuery();
                }
                logger.Info("[Launcher] compte web {0} : partage du compte {1} retiré ({2})", webId, compte, n);
                return "{\"ok\":true,\"retires\":" + n + "}";
            }
        }
    }
}
