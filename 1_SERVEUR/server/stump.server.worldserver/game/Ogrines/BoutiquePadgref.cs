using System;
using System.Linq;
using System.Security.Cryptography;
using NLog;
using Stump.Core.Attributes;
using Stump.Server.BaseServer.Initialization;
using Stump.Server.WorldServer.Core.Network;
using Stump.Server.WorldServer.Game.Items;

namespace Stump.Server.WorldServer.Game.Ogrines
{
    /// <summary>
    /// PADGREF 20/10 : « Boutique Padgref », la boutique du client (module Ankama_Web, ex « Hôtel de vente d'Artemis »).
    /// Le client parle à une API JSON-RPC en HTTP : sur Padgref c'est le site (boutique.php), qui lit les articles en base et
    /// débite les ogrines du COMPTE WEB. Le World s'occupe de :
    ///  - donner au client un jeton à usage unique (HaapiTokenRequestMessage → HaapiTokenMessage) que boutique.php vérifie
    ///    (table auth.nekfeu_boutique_jetons : compte, personnage, admin) ;
    ///  - livrer les achats : boutique.php écrit dans world.nekfeu_boutique_livraisons, le World donne l'objet au personnage
    ///    connecté (vérifié toutes les LivraisonIntervalMs ms ; un personnage hors ligne le reçoit dès sa connexion).
    /// </summary>
    public static class BoutiquePadgref
    {
        static readonly Logger logger = LogManager.GetCurrentClassLogger();

        static Stump.ORM.Database Database => WorldServer.Instance.DBAccessor.Database;
        static string A => "`" + (OgrinesWeb.AuthDatabase ?? "auth").Replace("`", "") + "`";

        /// <summary>Fréquence de vérification des achats à livrer (ms).</summary>
        [Variable(true)]
        public static int LivraisonIntervalMs = 3000;

        static bool m_pret, m_occupe;

        [Initialization(InitializationPass.Last)]
        public static void Initialize()
        {
            try
            {
                Database.Execute($@"CREATE TABLE IF NOT EXISTS {A}.nekfeu_boutique_jetons (Jeton CHAR(64) NOT NULL PRIMARY KEY, AccountId INT NOT NULL,
                    CharacterId INT NOT NULL, Admin TINYINT NOT NULL DEFAULT 0, ExpireLe DATETIME NOT NULL, Utilise TINYINT NOT NULL DEFAULT 0) DEFAULT CHARSET=utf8");
                Database.Execute(@"CREATE TABLE IF NOT EXISTS nekfeu_boutique_livraisons (Id INT NOT NULL AUTO_INCREMENT PRIMARY KEY, CharacterId INT NOT NULL,
                    ItemId INT NOT NULL, Quantite INT NOT NULL, AchatId INT NOT NULL DEFAULT 0, Etat TINYINT NOT NULL DEFAULT 0,
                    CreeLe DATETIME NOT NULL, LivreLe DATETIME NULL, KEY (Etat, CharacterId)) DEFAULT CHARSET=utf8");
                // une livraison restée « en cours » (World arrêté au mauvais moment) est retentée
                Database.Execute("UPDATE nekfeu_boutique_livraisons SET Etat = 0 WHERE Etat = 3");
                m_pret = true;
                WorldServer.Instance.IOTaskPool.CallPeriodically(LivraisonIntervalMs, Livrer);
                logger.Info("[Boutique] Boutique Padgref prête (livraisons toutes les {0} ms)", LivraisonIntervalMs);
            }
            catch (Exception e)
            {
                logger.Error("[Boutique] initialisation impossible : {0}", e.Message);
            }
        }

        /// <summary>Jeton à usage unique (5 min) pour que la boutique (boutique.php) reconnaisse ce compte et ce personnage.</summary>
        public static string CreerJeton(WorldClient client)
        {
            if (!m_pret || client?.Account == null || client.Character == null)
                return "";
            var octets = new byte[32];
            using (var rng = new RNGCryptoServiceProvider())
                rng.GetBytes(octets);
            var jeton = string.Concat(octets.Select(b => b.ToString("x2")));
            Database.Execute($"DELETE FROM {A}.nekfeu_boutique_jetons WHERE ExpireLe < NOW() - INTERVAL 1 DAY");
            Database.Execute($"INSERT INTO {A}.nekfeu_boutique_jetons (Jeton, AccountId, CharacterId, Admin, ExpireLe) VALUES (@0, @1, @2, @3, NOW() + INTERVAL 5 MINUTE)",
                jeton, client.Account.Id, client.Character.Id, client.UserGroup != null && client.UserGroup.IsGameMaster ? 1 : 0);
            return jeton;
        }

        class Livraison
        {
            public int Id { get; set; }
            public int CharacterId { get; set; }
            public int ItemId { get; set; }
            public int Quantite { get; set; }
        }

        static void Livrer()
        {
            if (!m_pret || m_occupe)
                return;
            m_occupe = true;
            try
            {
                foreach (var l in Database.Fetch<Livraison>("SELECT Id, CharacterId, ItemId, Quantite FROM nekfeu_boutique_livraisons WHERE Etat = 0 ORDER BY Id LIMIT 50"))
                {
                    var perso = World.Instance.GetCharacter(l.CharacterId);
                    if (perso == null || perso.Area == null)
                        continue;   // hors ligne : livré à sa prochaine connexion
                    var modele = ItemManager.Instance.TryGetTemplate(l.ItemId);
                    if (modele == null)
                    {
                        Database.Execute("UPDATE nekfeu_boutique_livraisons SET Etat = 2 WHERE Id = @0", l.Id);
                        logger.Error("[Boutique] livraison {0} : objet {1} inconnu", l.Id, l.ItemId);
                        continue;
                    }
                    if (Database.Execute("UPDATE nekfeu_boutique_livraisons SET Etat = 3 WHERE Id = @0 AND Etat = 0", l.Id) != 1)
                        continue;
                    var id = l.Id; var qte = Math.Max(1, l.Quantite);
                    perso.Area.ExecuteInContext(() =>
                    {
                        try
                        {
                            perso.Inventory.AddItem(modele, qte);
                            perso.SendServerMessage($"Boutique Padgref : vous avez reçu {qte} × {modele.Name}.");
                            WorldServer.Instance.IOTaskPool.AddMessage(() =>
                                Database.Execute("UPDATE nekfeu_boutique_livraisons SET Etat = 1, LivreLe = NOW() WHERE Id = @0", id));
                        }
                        catch (Exception e)
                        {
                            logger.Error("[Boutique] livraison {0} échouée : {1}", id, e.Message);
                            WorldServer.Instance.IOTaskPool.AddMessage(() => Database.Execute("UPDATE nekfeu_boutique_livraisons SET Etat = 0 WHERE Id = @0", id));
                        }
                    });
                }
            }
            catch (Exception e)
            {
                logger.Error("[Boutique] vérification des livraisons : {0}", e.Message);
            }
            finally
            {
                m_occupe = false;
            }
        }
    }
}
