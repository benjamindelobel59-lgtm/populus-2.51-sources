using System;
using System.Collections.Generic;
using System.Linq;
using Stump.Core.Extensions;
using Stump.DofusProtocol.Messages;
using Stump.DofusProtocol.Types;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Guilds;
using NetworkGuildEmblem = Stump.DofusProtocol.Types.GuildEmblem;

namespace Stump.Server.WorldServer.Game.Alliances
{
    /// <summary>
    /// MYSTIC 17/10 : alliance de guildes (phase 1 du plan _OUTILS/serveur/alliances/PLAN_ALLIANCES.md). Règles officielles : la
    /// guilde meneuse dirige (son meneur = chef de l'alliance) ; aucune limite de guildes. Données : tables nekfeu_alliances et
    /// nekfeu_alliances_guilds (AllianceManager).
    /// </summary>
    public class Alliance
    {
        readonly List<Guild> m_guilds = new List<Guild>();
        readonly Dictionary<int, DateTime> m_joinDates = new Dictionary<int, DateTime>();

        public Alliance(int id, string name, string tag, DateTime creationDate, NetworkGuildEmblem emblem)
        {
            Id = id;
            Name = name;
            Tag = tag;
            CreationDate = creationDate;
            Emblem = emblem;
        }

        public int Id { get; }

        public string Name { get; set; }

        public string Tag { get; set; }

        public DateTime CreationDate { get; }

        public NetworkGuildEmblem Emblem { get; set; }

        public int LeadingGuildId { get; set; }

        public string MotdContent { get; set; }
        public DateTime MotdDate { get; set; }
        public int MotdMemberId { get; set; }
        public string MotdMemberName { get; set; }

        public string BulletinContent { get; set; }
        public DateTime BulletinDate { get; set; }
        public int BulletinMemberId { get; set; }
        public string BulletinMemberName { get; set; }
        public DateTime LastNotifiedDate { get; set; }

        public IReadOnlyList<Guild> Guilds => m_guilds.AsReadOnly();

        public Guild LeadingGuild => m_guilds.FirstOrDefault(x => x.Id == LeadingGuildId) ?? m_guilds.FirstOrDefault();

        /// <summary>Chef de l'alliance : meneur de la guilde meneuse.</summary>
        public Guilds.GuildMember Leader => LeadingGuild?.Boss;

        public int MembersCount => m_guilds.Sum(x => x.Members.Count);

        public IEnumerable<Character> OnlineCharacters =>
            m_guilds.SelectMany(g => g.Members.Where(m => m.IsConnected).Select(m => m.Character));

        public DateTime JoinDate(Guild guild) => m_joinDates.TryGetValue(guild.Id, out var d) ? d : CreationDate;

        internal void AttachGuild(Guild guild, DateTime joinDate)
        {
            if (!m_guilds.Contains(guild))
                m_guilds.Add(guild);
            m_joinDates[guild.Id] = joinDate;
            guild.Alliance = this;
        }

        internal void DetachGuild(Guild guild)
        {
            m_guilds.Remove(guild);
            m_joinDates.Remove(guild.Id);
            if (guild.Alliance == this)
                guild.Alliance = null;
        }

        /// <summary>Le personnage est le chef de l'alliance (meneur de la guilde meneuse).</summary>
        public bool IsLeader(Character character) =>
            character?.GuildMember != null && character.Guild == LeadingGuild && character.GuildMember.IsBoss;

        /// <summary>Envoie un message à tous les membres connectés de toutes les guildes de l'alliance.</summary>
        public void Send(Message message)
        {
            foreach (var guild in m_guilds)
                guild.Clients.Send(message);
        }

        public AllianceInformations GetAllianceInformations() => new AllianceInformations((uint)Id, Tag, Name, Emblem);

        public BasicAllianceInformations GetBasicAllianceInformations() => new BasicAllianceInformations((uint)Id, Tag);

        public BasicNamedAllianceInformations GetBasicNamedAllianceInformations() => new BasicNamedAllianceInformations((uint)Id, Tag, Name);

        public AllianceFactSheetInformations GetAllianceFactSheetInformations() =>
            new AllianceFactSheetInformations((uint)Id, Tag, Name, Emblem, CreationDate.GetUnixTimeStamp());

        public AllianceVersatileInformations GetAllianceVersatileInformations() =>
            new AllianceVersatileInformations((uint)Id, (ushort)m_guilds.Count, (ushort)MembersCount, (ushort)AllianceManager.Instance.ControlledSubAreas(this).Count());

        public GuildInAllianceInformations GetGuildInAllianceInformations(Guild guild) =>
            new GuildInAllianceInformations((uint)guild.Id, guild.Name, (byte)guild.Level, guild.Emblem.GetNetworkGuildEmblem(),
                (byte)Math.Min(255, guild.Members.Count), JoinDate(guild).GetUnixTimeStamp());

        public GuildInsiderFactSheetInformations GetGuildInsiderFactSheetInformations(Guild guild)
        {
            var boss = guild.Boss;
            var lastActivity = guild.Members.Where(x => x.LastConnection.HasValue).Select(x => x.LastConnection.Value)
                .DefaultIfEmpty(guild.CreationDate).Max();
            return new GuildInsiderFactSheetInformations((uint)guild.Id, guild.Name, (byte)guild.Level, guild.Emblem.GetNetworkGuildEmblem(),
                (ulong)(boss?.Id ?? 0), (ushort)guild.Members.Count, boss?.Name ?? "", (ushort)guild.Members.Count(x => x.IsConnected),
                (sbyte)guild.TaxCollectors.Count, lastActivity.GetUnixTimeStamp());
        }
    }
}
