using System;
using NLog;
using Stump.Core.Attributes;
using Stump.Server.BaseServer.Initialization;
using Stump.Server.WorldServer.Core.Network;

namespace Stump.Server.WorldServer.Game.Ogrines
{
    /// <summary>
    /// PADGREF 20/10 : les ogrines appartiennent au COMPTE WEB (colonne Ogrines de auth.nekfeu_comptes_web), partagées par
    /// tous ses comptes de jeu. En jeu elles ne sont plus un objet (rien dans l'inventaire : Inventory.ActiveTokens = false),
    /// donc impossibles à échanger ou à vendre ; seuls le site (Espace Jeux) et le panel / boutique Ogrines s'en servent.
    ///  - à la sélection d'un personnage : anciens ogrines du compte de jeu (accounts.Tokens, une seule fois, table
    ///    nekfeu_ogrines_absorbes) et ogrines offerts (accounts.NewTokens) passent dans le porte-monnaie du compte web lié ;
    ///  - objets « donne des ogrines » : crédit du compte web (ou NewTokens si le compte de jeu n'est lié à aucun compte web).
    /// Même logique que le site : _OUTILS/web/site/padgref/api/ogrines.php. Journal : auth.nekfeu_ogrines_journal.
    /// </summary>
    public static class OgrinesWeb
    {
        static readonly Logger logger = LogManager.GetCurrentClassLogger();

        static Stump.ORM.Database Database => WorldServer.Instance.DBAccessor.Database;

        /// <summary>Nom de la base de l'AuthServer (même serveur MariaDB que le monde).</summary>
        [Variable(true)]
        public static string AuthDatabase = "auth";

        static bool m_pret;

        static string A => "`" + (AuthDatabase ?? "auth").Replace("`", "") + "`";

        [Initialization(InitializationPass.Last)]
        public static void Initialize()
        {
            try
            {
                Database.Execute($@"CREATE TABLE IF NOT EXISTS {A}.nekfeu_comptes_web (Id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
                    Email VARCHAR(190) NOT NULL, Pseudo VARCHAR(40) NOT NULL, MotDePasse VARCHAR(255) NOT NULL, CreeLe DATETIME NOT NULL,
                    DerniereConnexion DATETIME NULL, Bloque TINYINT NOT NULL DEFAULT 0, UNIQUE KEY (Email), UNIQUE KEY (Pseudo)) DEFAULT CHARSET=utf8");
                Database.Execute($@"CREATE TABLE IF NOT EXISTS {A}.nekfeu_comptes_web_jeu (AccountId INT NOT NULL PRIMARY KEY, CompteWebId INT NOT NULL,
                    LieLe DATETIME NOT NULL, KEY (CompteWebId)) DEFAULT CHARSET=utf8");
                if (Database.ExecuteScalar<int>("SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = @0 AND TABLE_NAME = 'nekfeu_comptes_web' AND COLUMN_NAME = 'Ogrines'", AuthDatabase) == 0)
                    Database.Execute($"ALTER TABLE {A}.nekfeu_comptes_web ADD COLUMN Ogrines BIGINT NOT NULL DEFAULT 0");
                Database.Execute($@"CREATE TABLE IF NOT EXISTS {A}.nekfeu_ogrines_journal (Id BIGINT NOT NULL AUTO_INCREMENT PRIMARY KEY,
                    CompteWebId INT NOT NULL, Delta BIGINT NOT NULL, Solde BIGINT NOT NULL, Origine VARCHAR(40) NOT NULL,
                    Detail VARCHAR(190) NULL, Date DATETIME NOT NULL, KEY (CompteWebId, Date)) DEFAULT CHARSET=utf8");
                Database.Execute($@"CREATE TABLE IF NOT EXISTS {A}.nekfeu_ogrines_absorbes (AccountId INT NOT NULL PRIMARY KEY, Tokens BIGINT NOT NULL,
                    CompteWebId INT NOT NULL, Date DATETIME NOT NULL) DEFAULT CHARSET=utf8");
                m_pret = true;
                logger.Info("[Ogrines] porte-monnaie des comptes web prêt (base {0})", AuthDatabase);
            }
            catch (Exception e)
            {
                logger.Error("[Ogrines] base « {0} » inaccessible depuis le World, ogrines du compte web désactivées : {1}", AuthDatabase, e.Message);
            }
        }

        // MYSTIC 21/10 : ExecuteScalar<int?> / <long?> plantait (« Cast non valide de Int32 en Nullable<Int32> » : PetaPoco fait un
        // Convert.ChangeType qui ne connaît pas les types nullables) : les ogrines du compte n'étaient jamais repris ni lus.
        // Valeur brute lue puis convertie ; null si aucune ligne.
        static long? Nombre(string sql, params object[] args)
        {
            var v = Database.ExecuteScalar<object>(sql, args);
            return v == null || v is DBNull ? (long?)null : Convert.ToInt64(v);
        }

        /// <summary>Compte web auquel ce compte de jeu est lié (0 = aucun).</summary>
        public static int CompteWeb(int accountId)
        {
            if (!m_pret) return 0;
            return (int)(Nombre($"SELECT CompteWebId FROM {A}.nekfeu_comptes_web_jeu WHERE AccountId = @0", accountId) ?? 0);
        }

        public static long Solde(int compteWeb) =>
            !m_pret || compteWeb <= 0 ? 0 : Nombre($"SELECT Ogrines FROM {A}.nekfeu_comptes_web WHERE Id = @0", compteWeb) ?? 0;

        /// <summary>Crédit (delta &gt; 0) ou débit (delta &lt; 0, seulement si le solde suffit). Renvoie le nouveau solde, null si refusé.</summary>
        public static long? Mouvement(int compteWeb, long delta, string origine, string detail)
        {
            if (!m_pret || compteWeb <= 0) return null;
            if (Database.Execute($"UPDATE {A}.nekfeu_comptes_web SET Ogrines = Ogrines + @0 WHERE Id = @1 AND Ogrines + @0 >= 0", delta, compteWeb) != 1)
                return null;
            var solde = Solde(compteWeb);
            if (delta != 0)
                Database.Execute($"INSERT INTO {A}.nekfeu_ogrines_journal (CompteWebId, Delta, Solde, Origine, Detail, Date) VALUES (@0, @1, @2, @3, @4, NOW())",
                    compteWeb, delta, solde, Couper(origine, 40), Couper(detail, 190));
            return solde;
        }

        static string Couper(string s, int n) => s == null ? null : (s.Length > n ? s.Substring(0, n) : s);

        /// <summary>Ramène dans le porte-monnaie du compte web les ogrines restés sur ce compte de jeu (sélection d'un personnage).</summary>
        public static void Absorber(WorldClient client)
        {
            try
            {
                var compte = client.Account;
                var web = CompteWeb(compte.Id);
                if (web <= 0) return;
                if (Database.Execute($@"INSERT IGNORE INTO {A}.nekfeu_ogrines_absorbes (AccountId, Tokens, CompteWebId, Date)
                        SELECT Id, GREATEST(Tokens, 0), @1, NOW() FROM {A}.accounts WHERE Id = @0", compte.Id, web) == 1)
                {
                    var anciens = Nombre($"SELECT Tokens FROM {A}.nekfeu_ogrines_absorbes WHERE AccountId = @0", compte.Id) ?? 0;
                    if (anciens > 0) Mouvement(web, anciens, "anciens ogrines", "compte " + compte.Login);
                }
                var offerts = (int)(Nombre($"SELECT NewTokens FROM {A}.accounts WHERE Id = @0", compte.Id) ?? 0);
                if (offerts > 0 && Database.Execute($"UPDATE {A}.accounts SET NewTokens = 0 WHERE Id = @0 AND NewTokens = @1", compte.Id, offerts) == 1)
                    Mouvement(web, offerts, "ogrines offerts", "compte " + compte.Login);
            }
            catch (Exception e)
            {
                logger.Error("[Ogrines] reprise des ogrines du compte {0} impossible : {1}", client.Account?.Login, e.Message);
            }
        }

        /// <summary>Ogrines données en jeu (objet) : au compte web, sinon gardées sur le compte de jeu (reprises dès qu'il sera lié).</summary>
        public static long? Donner(WorldClient client, int montant, string origine)
        {
            var web = CompteWeb(client.Account.Id);
            if (web > 0) return Mouvement(web, montant, origine, "compte " + client.Account.Login);
            if (m_pret) Database.Execute($"UPDATE {A}.accounts SET NewTokens = NewTokens + @0 WHERE Id = @1", montant, client.Account.Id);
            return null;
        }
    }
}
