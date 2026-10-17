using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Security.Cryptography;
using NLog;
using Stump.Core.Attributes;
using Stump.Core.Reflection;
using Stump.Server.BaseServer.Initialization;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Admin;

namespace Stump.Server.WorldServer.Game.Bugs
{
    /// <summary>
    /// MYSTIC 17/10 : « Signaler un bug » (bouton de la barre / .bug). Le World garde le journal du combat du joueur dans
    /// nekfeu_bugs avec un jeton à usage unique ; le panel web (populus-admin/bug.php?t=jeton) affiche le formulaire (titre,
    /// description, logs), reçoit le log du client et poste le tout sur le webhook Discord du salon des bugs.
    /// </summary>
    public class SignalementsBugs : Singleton<SignalementsBugs>
    {
        static readonly Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>Durée de validité du lien du formulaire.</summary>
        [Variable(true)]
        public static int DureeLienMinutes = 30;

        /// <summary>Délai minimum entre deux signalements d'un même personnage.</summary>
        [Variable(true)]
        public static int DelaiEntreSignalementsSecondes = 60;

        readonly ConcurrentDictionary<int, DateTime> m_derniers = new ConcurrentDictionary<int, DateTime>();

        static Stump.ORM.Database Database => WorldServer.Instance.DBAccessor.Database;

        [Initialization(InitializationPass.Last)]
        public void Initialize()
        {
            Database.Execute(@"CREATE TABLE IF NOT EXISTS nekfeu_bugs (
                Token VARCHAR(64) NOT NULL PRIMARY KEY, AccountId INT NOT NULL, CharacterId INT NOT NULL,
                CharacterName VARCHAR(64) NOT NULL, MapId INT NOT NULL, CreatedAt DATETIME NOT NULL, ExpiresAt DATETIME NOT NULL,
                JournalCombat MEDIUMTEXT NULL, LogClient MEDIUMTEXT NULL, Titre VARCHAR(200) NULL, Description TEXT NULL,
                Envoye TINYINT NOT NULL DEFAULT 0, EnvoyeAt DATETIME NULL, KEY (CreatedAt)) DEFAULT CHARSET=utf8mb4");
            logger.Info("[Bugs] signalements prêts");
        }

        /// <summary>Crée le lien du formulaire, ou null si le joueur a déjà signalé il y a moins de DelaiEntreSignalementsSecondes.</summary>
        public string CreerLien(Character perso, out int attente)
        {
            attente = 0;
            if (m_derniers.TryGetValue(perso.Id, out var dernier))
            {
                var reste = DelaiEntreSignalementsSecondes - (int)(DateTime.Now - dernier).TotalSeconds;
                if (reste > 0)
                {
                    attente = reste;
                    return null;
                }
            }

            var octets = new byte[24];
            using (var rng = new RNGCryptoServiceProvider())
                rng.GetBytes(octets);
            var jeton = string.Concat(octets.Select(b => b.ToString("x2")));

            Database.Execute("DELETE FROM nekfeu_bugs WHERE Envoye = 0 AND ExpiresAt < NOW() - INTERVAL 1 DAY");
            Database.Execute(@"INSERT INTO nekfeu_bugs (Token, AccountId, CharacterId, CharacterName, MapId, CreatedAt, ExpiresAt, JournalCombat)
                VALUES (@0, @1, @2, @3, @4, NOW(), NOW() + INTERVAL @5 MINUTE, @6)",
                jeton, perso.Account.Id, perso.Id, perso.Name, perso.Map?.Id ?? 0, DureeLienMinutes, JournalCombat.Pour(perso));

            m_derniers[perso.Id] = DateTime.Now;
            return AdminPanelBridge.PanelUrl + "bug.php?t=" + jeton;
        }
    }
}
