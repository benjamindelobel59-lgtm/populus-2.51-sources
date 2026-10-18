using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;
using Stump.Core.Reflection;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Enums.Custom;
using Stump.Server.BaseServer.Initialization;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Guilds;
using Stump.Server.WorldServer.Game.Items;
using Stump.Server.WorldServer.Handlers.Alliances;
using NetworkGuildEmblem = Stump.DofusProtocol.Types.GuildEmblem;

namespace Stump.Server.WorldServer.Game.Alliances
{
    /// <summary>
    /// MYSTIC 17/10 : gestion des alliances (création au Temple des alliances avec une Alliogemme, guildes membres, chef, message du
    /// jour, bulletin, nom / tag / blason). Tables créées au démarrage (aucun SQL à jouer) : nekfeu_alliances, nekfeu_alliances_guilds.
    /// </summary>
    public class AllianceManager : Singleton<AllianceManager>
    {
        static readonly Logger logger = LogManager.GetCurrentClassLogger();

        static Stump.ORM.Database Database => WorldServer.Instance.DBAccessor.Database;

        readonly Dictionary<int, Alliance> m_alliances = new Dictionary<int, Alliance>();
        readonly object m_lock = new object();
        int m_nextId = 1;

        public class LigneAlliance
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Tag { get; set; }
            public DateTime CreationDate { get; set; }
            public int BgShape { get; set; }
            public int BgColor { get; set; }
            public int SymShape { get; set; }
            public int SymColor { get; set; }
            public int LeadingGuildId { get; set; }
            public string MotdContent { get; set; }
            public DateTime? MotdDate { get; set; }
            public int MotdMemberId { get; set; }
            public string MotdMemberName { get; set; }
            public string BulletinContent { get; set; }
            public DateTime? BulletinDate { get; set; }
            public int BulletinMemberId { get; set; }
            public string BulletinMemberName { get; set; }
            public DateTime? LastNotifiedDate { get; set; }
        }

        public class LigneGuilde
        {
            public int GuildId { get; set; }
            public int AllianceId { get; set; }
            public DateTime JoinDate { get; set; }
        }

        [Initialization(InitializationPass.Last)]
        public void Initialize()
        {
            Database.Execute(@"CREATE TABLE IF NOT EXISTS nekfeu_alliances (Id INT NOT NULL PRIMARY KEY, Name VARCHAR(64) NOT NULL,
                Tag VARCHAR(8) NOT NULL, CreationDate DATETIME NOT NULL, BgShape INT NOT NULL, BgColor INT NOT NULL, SymShape INT NOT NULL,
                SymColor INT NOT NULL, LeadingGuildId INT NOT NULL, MotdContent TEXT NULL, MotdDate DATETIME NULL, MotdMemberId INT NOT NULL DEFAULT 0,
                MotdMemberName VARCHAR(64) NULL, BulletinContent TEXT NULL, BulletinDate DATETIME NULL, BulletinMemberId INT NOT NULL DEFAULT 0,
                BulletinMemberName VARCHAR(64) NULL, LastNotifiedDate DATETIME NULL) DEFAULT CHARSET=utf8");
            Database.Execute(@"CREATE TABLE IF NOT EXISTS nekfeu_alliances_guilds (GuildId INT NOT NULL PRIMARY KEY, AllianceId INT NOT NULL,
                JoinDate DATETIME NOT NULL) DEFAULT CHARSET=utf8");

            foreach (var l in Database.Fetch<LigneAlliance>("SELECT * FROM nekfeu_alliances"))
            {
                var alliance = new Alliance(l.Id, l.Name, l.Tag, l.CreationDate,
                    new NetworkGuildEmblem((ushort)l.SymShape, l.SymColor, (sbyte)l.BgShape, l.BgColor))
                {
                    LeadingGuildId = l.LeadingGuildId,
                    MotdContent = l.MotdContent,
                    MotdDate = l.MotdDate ?? DateTime.MinValue,
                    MotdMemberId = l.MotdMemberId,
                    MotdMemberName = l.MotdMemberName,
                    BulletinContent = l.BulletinContent,
                    BulletinDate = l.BulletinDate ?? DateTime.MinValue,
                    BulletinMemberId = l.BulletinMemberId,
                    BulletinMemberName = l.BulletinMemberName,
                    LastNotifiedDate = l.LastNotifiedDate ?? DateTime.MinValue,
                };
                m_alliances[alliance.Id] = alliance;
                m_nextId = Math.Max(m_nextId, alliance.Id + 1);
            }

            foreach (var l in Database.Fetch<LigneGuilde>("SELECT * FROM nekfeu_alliances_guilds"))
            {
                var guild = GuildManager.Instance.TryGetGuild(l.GuildId);
                if (guild == null || !m_alliances.TryGetValue(l.AllianceId, out var alliance))
                {
                    logger.Warn("[Alliances] guilde {0} introuvable (ou alliance {1}) : retirée de l'alliance", l.GuildId, l.AllianceId);
                    Executer("DELETE FROM nekfeu_alliances_guilds WHERE GuildId = @0", l.GuildId);
                    continue;
                }

                alliance.AttachGuild(guild, l.JoinDate);
            }

            foreach (var vide in m_alliances.Values.Where(x => x.Guilds.Count == 0).ToArray())
            {
                logger.Warn("[Alliances] alliance {0} ({1}) sans guilde : supprimée", vide.Id, vide.Name);
                Supprimer(vide);
            }

            logger.Info("[Alliances] {0} alliances, {1} guildes membres", m_alliances.Count, m_alliances.Values.Sum(x => x.Guilds.Count));

            PrismManager.Instance.Charger();
        }

        static void Executer(string sql, params object[] args)
        {
            WorldServer.Instance.IOTaskPool.AddMessage(() => Database.Execute(sql, args));
        }

        public IEnumerable<Alliance> Alliances
        {
            get { lock (m_lock) return m_alliances.Values.ToArray(); }
        }

        public Alliance TryGetAlliance(int id)
        {
            lock (m_lock)
                return m_alliances.TryGetValue(id, out var a) ? a : null;
        }

        /// <summary>Sous-zones contrôlées par l'alliance (prismes, phase 2).</summary>
        public IEnumerable<int> ControlledSubAreas(Alliance alliance) => PrismManager.Instance.PrismesDe(alliance).Select(x => x.SubAreaId);

        bool NomExiste(string name, Alliance sauf = null) =>
            Alliances.Any(x => x != sauf && string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));

        bool TagExiste(string tag, Alliance sauf = null) =>
            Alliances.Any(x => x != sauf && string.Equals(x.Tag, tag, StringComparison.OrdinalIgnoreCase));

        bool BlasonExiste(NetworkGuildEmblem e, Alliance sauf = null) =>
            Alliances.Any(x => x != sauf && x.Emblem.SymbolShape == e.SymbolShape && x.Emblem.SymbolColor == e.SymbolColor &&
                               x.Emblem.BackgroundShape == e.BackgroundShape && x.Emblem.BackgroundColor == e.BackgroundColor);

        // nom : comme les guildes (lettres, espaces, tirets, apostrophes), 4 à 30 caractères ; tag : 3 à 5 lettres ou chiffres
        static bool NomValide(string name) =>
            !string.IsNullOrWhiteSpace(name) && Regex.IsMatch(name, "^[A-Z][A-Za-zÀ-ÿ'\\- ]{3,29}$") && !name.Contains("  ");

        static bool TagValide(string tag) => !string.IsNullOrEmpty(tag) && Regex.IsMatch(tag, "^[A-Za-z0-9]{3,5}$");

        SocialGroupCreationResultEnum VerifierNomTagBlason(string name, string tag, NetworkGuildEmblem emblem, Alliance sauf)
        {
            if (name != null)
            {
                if (!NomValide(name))
                    return SocialGroupCreationResultEnum.SOCIAL_GROUP_CREATE_ERROR_NAME_INVALID;
                if (NomExiste(name, sauf))
                    return SocialGroupCreationResultEnum.SOCIAL_GROUP_CREATE_ERROR_NAME_ALREADY_EXISTS;
            }

            if (tag != null)
            {
                if (!TagValide(tag))
                    return SocialGroupCreationResultEnum.SOCIAL_GROUP_CREATE_ERROR_TAG_INVALID;
                if (TagExiste(tag, sauf))
                    return SocialGroupCreationResultEnum.SOCIAL_GROUP_CREATE_ERROR_TAG_ALREADY_EXISTS;
            }

            if (emblem != null)
            {
                if (emblem.SymbolShape >= 324)
                    return SocialGroupCreationResultEnum.SOCIAL_GROUP_CREATE_ERROR_EMBLEM_INVALID;
                if (BlasonExiste(emblem, sauf))
                    return SocialGroupCreationResultEnum.SOCIAL_GROUP_CREATE_ERROR_EMBLEM_ALREADY_EXISTS;
            }

            return SocialGroupCreationResultEnum.SOCIAL_GROUP_CREATE_OK;
        }

        /// <summary>Création au Temple des alliances : meneur d'une guilde sans alliance, Alliogemme consommée.</summary>
        public SocialGroupCreationResultEnum CreateAlliance(Character character, string name, string tag, NetworkGuildEmblem emblem)
        {
            if (character.GuildMember == null || !character.GuildMember.IsBoss)
                return SocialGroupCreationResultEnum.SOCIAL_GROUP_CREATE_ERROR_REQUIREMENT_UNMET;

            if (character.Guild.Alliance != null)
                return SocialGroupCreationResultEnum.SOCIAL_GROUP_CREATE_ERROR_ALREADY_IN_GROUP;

            var gemme = character.Inventory.TryGetItem(ItemManager.Instance.TryGetTemplate(ItemIdEnum.ALLIOGEMME_14290));
            if (gemme == null && !character.IsGameMaster())
                return SocialGroupCreationResultEnum.SOCIAL_GROUP_CREATE_ERROR_REQUIREMENT_UNMET;

            Alliance alliance;
            lock (m_lock)
            {
                var verif = VerifierNomTagBlason(name, tag, emblem, null);
                if (verif != SocialGroupCreationResultEnum.SOCIAL_GROUP_CREATE_OK)
                    return verif;

                alliance = new Alliance(m_nextId++, name, tag, DateTime.Now, emblem) { LeadingGuildId = character.Guild.Id };
                m_alliances[alliance.Id] = alliance;
            }

            if (gemme != null && !character.IsGameMaster())
                character.Inventory.RemoveItem(gemme, 1);

            Executer(@"INSERT INTO nekfeu_alliances (Id, Name, Tag, CreationDate, BgShape, BgColor, SymShape, SymColor, LeadingGuildId)
                VALUES (@0, @1, @2, @3, @4, @5, @6, @7, @8)", alliance.Id, alliance.Name, alliance.Tag, alliance.CreationDate,
                (int)emblem.BackgroundShape, emblem.BackgroundColor, (int)emblem.SymbolShape, emblem.SymbolColor, alliance.LeadingGuildId);

            AjouterGuilde(alliance, character.Guild);
            return SocialGroupCreationResultEnum.SOCIAL_GROUP_CREATE_OK;
        }

        public void AjouterGuilde(Alliance alliance, Guild guild)
        {
            var date = DateTime.Now;
            lock (m_lock)
                alliance.AttachGuild(guild, date);

            Executer("REPLACE INTO nekfeu_alliances_guilds (GuildId, AllianceId, JoinDate) VALUES (@0, @1, @2)", guild.Id, alliance.Id, date);

            foreach (var membre in guild.Members.Where(x => x.IsConnected))
            {
                AllianceHandler.SendAllianceJoinedMessage(membre.Character.Client, alliance);
                AllianceHandler.SendMotdAndBulletin(membre.Character.Client, alliance);
                membre.Character.RefreshActor();
            }
        }

        /// <summary>Une guilde quitte l'alliance (départ volontaire ou exclusion). Sans guilde restante, l'alliance est dissoute.</summary>
        public void RetirerGuilde(Alliance alliance, Guild guild, bool exclue)
        {
            lock (m_lock)
            {
                if (!alliance.Guilds.Contains(guild))
                    return;
                alliance.DetachGuild(guild);
            }

            Executer("DELETE FROM nekfeu_alliances_guilds WHERE GuildId = @0", guild.Id);

            foreach (var membre in guild.Members.Where(x => x.IsConnected))
            {
                AllianceHandler.SendAllianceLeftMessage(membre.Character.Client);
                membre.Character.RefreshActor();
            }

            if (alliance.Guilds.Count == 0)
            {
                Supprimer(alliance);
                return;
            }

            alliance.Send(new DofusProtocol.Messages.AllianceGuildLeavingMessage(exclue, (uint)guild.Id));

            if (alliance.LeadingGuildId == guild.Id)
            {
                // la guilde meneuse part : la plus ancienne guilde restante prend la tête
                var suivante = alliance.Guilds.OrderBy(x => alliance.JoinDate(x)).First();
                ChangerGuildeMeneuse(alliance, suivante);
            }
        }

        public void ChangerGuildeMeneuse(Alliance alliance, Guild guild)
        {
            if (!alliance.Guilds.Contains(guild))
                return;

            alliance.LeadingGuildId = guild.Id;
            Executer("UPDATE nekfeu_alliances SET LeadingGuildId = @1 WHERE Id = @0", alliance.Id, guild.Id);

            foreach (var perso in alliance.OnlineCharacters)
                AllianceHandler.SendAllianceMembershipMessage(perso.Client, alliance);
        }

        void Supprimer(Alliance alliance)
        {
            lock (m_lock)
                m_alliances.Remove(alliance.Id);

            foreach (var guild in alliance.Guilds.ToArray())
                alliance.DetachGuild(guild);

            Executer("DELETE FROM nekfeu_alliances WHERE Id = @0", alliance.Id);
            Executer("DELETE FROM nekfeu_alliances_guilds WHERE AllianceId = @0", alliance.Id);
            PrismManager.Instance.OnAllianceDeleted(alliance);
        }

        /// <summary>Appelé quand une guilde est dissoute.</summary>
        public void OnGuildDeleted(Guild guild)
        {
            if (guild.Alliance != null)
                RetirerGuilde(guild.Alliance, guild, false);
        }

        public void ChangerMotd(Alliance alliance, Character auteur, string contenu)
        {
            alliance.MotdContent = contenu;
            alliance.MotdDate = DateTime.Now;
            alliance.MotdMemberId = auteur.Id;
            alliance.MotdMemberName = auteur.Name;
            Executer("UPDATE nekfeu_alliances SET MotdContent = @1, MotdDate = @2, MotdMemberId = @3, MotdMemberName = @4 WHERE Id = @0",
                alliance.Id, contenu, alliance.MotdDate, auteur.Id, auteur.Name);
            alliance.Send(AllianceHandler.MotdMessage(alliance));
        }

        public void ChangerBulletin(Alliance alliance, Character auteur, string contenu, bool notifier)
        {
            alliance.BulletinContent = contenu;
            alliance.BulletinDate = DateTime.Now;
            alliance.BulletinMemberId = auteur.Id;
            alliance.BulletinMemberName = auteur.Name;
            if (notifier)
                alliance.LastNotifiedDate = alliance.BulletinDate;
            Executer(@"UPDATE nekfeu_alliances SET BulletinContent = @1, BulletinDate = @2, BulletinMemberId = @3, BulletinMemberName = @4,
                LastNotifiedDate = @5 WHERE Id = @0", alliance.Id, contenu, alliance.BulletinDate, auteur.Id, auteur.Name, alliance.LastNotifiedDate);
            alliance.Send(AllianceHandler.BulletinMessage(alliance));
        }

        /// <summary>Potions de renommage / de blason (chef de l'alliance). name / tag / emblem null = inchangé.</summary>
        public SocialGroupCreationResultEnum Modifier(Character character, string name, string tag, NetworkGuildEmblem emblem)
        {
            var alliance = character.Guild?.Alliance;
            if (alliance == null || !alliance.IsLeader(character))
                return SocialGroupCreationResultEnum.SOCIAL_GROUP_CREATE_ERROR_REQUIREMENT_UNMET;

            var potionId = emblem != null ? ItemIdEnum.POTION_DE_CHANGEMENT_DE_BLASON_DALLIANCE_14292 : ItemIdEnum.POTION_DE_RENOMMAGE_DALLIANCE_14291;
            var potion = character.Inventory.TryGetItem(ItemManager.Instance.TryGetTemplate(potionId));
            if (potion == null && !character.IsGameMaster())
                return SocialGroupCreationResultEnum.SOCIAL_GROUP_CREATE_ERROR_REQUIREMENT_UNMET;

            lock (m_lock)
            {
                var verif = VerifierNomTagBlason(name, tag, emblem, alliance);
                if (verif != SocialGroupCreationResultEnum.SOCIAL_GROUP_CREATE_OK)
                    return verif;

                if (name != null) alliance.Name = name;
                if (tag != null) alliance.Tag = tag;
                if (emblem != null) alliance.Emblem = emblem;
            }

            if (potion != null && !character.IsGameMaster())
                character.Inventory.RemoveItem(potion, 1);

            var e = alliance.Emblem;
            Executer("UPDATE nekfeu_alliances SET Name = @1, Tag = @2, BgShape = @3, BgColor = @4, SymShape = @5, SymColor = @6 WHERE Id = @0",
                alliance.Id, alliance.Name, alliance.Tag, (int)e.BackgroundShape, e.BackgroundColor, (int)e.SymbolShape, e.SymbolColor);

            foreach (var perso in alliance.OnlineCharacters)
            {
                AllianceHandler.SendAllianceMembershipMessage(perso.Client, alliance);
                perso.RefreshActor();
            }

            return SocialGroupCreationResultEnum.SOCIAL_GROUP_CREATE_OK;
        }
    }
}
