using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Messages;
using Stump.DofusProtocol.Types;
using Stump.Server.WorldServer.Database.Mounts;
using Stump.Server.WorldServer.Database.World;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Mounts;
using Stump.Server.WorldServer.Game.Guilds;

namespace Stump.Server.WorldServer.Game.Maps.Paddocks
{
    public class Paddock
    {
        private readonly List<Mount> m_mounts;

        public Paddock(WorldMapPaddockRecord record)
        {
            Record = record;

            if (record.Map == null)
                throw new Exception(string.Format("Paddock's map({0}) not found", record.MapId));

            m_mounts = Record.Mounts.Select(x => new Mount(x)).ToList();

            // NEKFEU : pendant le chargement de PaddockManager, Mount(record) ne trouve pas encore l'enclos
            // (GetPaddock renvoie null) et efface son PaddockId -> on le remet.
            foreach (var mount in m_mounts)
            {
                mount.Paddock = this;
                mount.IsDirty = false;
            }
        }

        /// <summary>
        /// NEKFEU : GuildId en base : -1 (ou NULL) = enclos public, 0 = sans proprietaire (a vendre), > 0 = guilde proprietaire.
        /// </summary>
        public const int NO_OWNER_GUILD_ID = 0;

        private WorldMapPaddockRecord Record
        {
            get;
        }

        public int Id => Record.Id;

        public int? GuildId => Record.GuildId;

        /// <summary>NEKFEU : 0 = enclos sur sa map ; 1 a 4 = un des 4 enclos derriere la porte d'entree (Map = map de la porte).</summary>
        public int Slot => Record.Slot;

        public Guild Guild
        {
            get { return Record.Guild; }
            protected set
            {
                IsRecordDirty = true;
                Record.Guild = value;
            }
        }

        public Map Map
        {
            get { return Record.Map; }
            protected set
            {
                IsRecordDirty = true;
                Record.Map = value;
            }
        }

        public ReadOnlyCollection<Mount> PaddockedMounts => m_mounts.AsReadOnly();

        public uint MaxOutdoorMount
        {
            get { return Record.MaxOutdoorMount; }
            protected set
            {
                IsRecordDirty = true;
                Record.MaxOutdoorMount = value;
            }
        }

        public uint MaxItems
        {
            get { return Record.MaxItems; }
            protected set
            {
                IsRecordDirty = true;
                Record.MaxItems = value;
            }
        }

        public bool Abandonned
        {
            get { return Record.Abandonned; }
            protected set
            {
                IsRecordDirty = true;
                Record.Abandonned = value;
            }
        }

        public bool OnSale
        {
            get { return Record.OnSale; }
            protected set
            {
                IsRecordDirty = true;
                Record.OnSale = value;
            }
        }

        public bool Locked
        {
            get { return Record.Locked; }
            protected set
            {
                IsRecordDirty = true;
                Record.Locked = value;
            }
        }

        public int Price
        {
            get { return Record.Price; }
            protected set
            {
                IsRecordDirty = true;
                Record.Price = value;
            }
        }

        public bool IsRecordDirty
        {
            get;
            private set;
        }

        // NEKFEU : avant, "public" = sans guilde ; maintenant un enclos sans guilde peut aussi etre a vendre (GuildId = 0).
        public bool IsPublicPaddock() => Record.GuildId == null || Record.GuildId < 0;

        /// <summary>Enclos achetable possede par une guilde existante.</summary>
        public bool HasOwner => !IsPublicPaddock() && Guild != null;

        public void Save(ORM.Database database)
        {
            if (IsRecordDirty)
            {
                database.Update(Record);
                IsRecordDirty = false;
            }

            // NEKFEU : database.Save(mount) ne sauvegardait pas l'enregistrement de la monture
            foreach (var mount in PaddockedMounts.Where(x => x.IsDirty || x.Record.IsNew))
                mount.Save(database);
        }

        public bool IsPaddockOwner(Character character)
        {
            if (IsPublicPaddock())
                return true;

            return HasOwner && character.Guild?.Id == Guild.Id;
        }

        #region NEKFEU : achat / vente par les guildes

        public static bool IsGuildBoss(Character character) => character.Guild != null && character.GuildMember != null && character.GuildMember.IsBoss;

        /// <summary>Le personnage est le meneur de la guilde proprietaire.</summary>
        public bool IsOwnerBoss(Character character) => HasOwner && character.Guild?.Id == Guild.Id && IsGuildBoss(character);

        /// <summary>"Acceder" : enclos public, ou membre de la guilde proprietaire avec le droit d'utiliser les enclos.</summary>
        public bool CanUse(Character character)
        {
            if (IsPublicPaddock())
                return true;

            return HasOwner && character.Guild?.Id == Guild.Id && character.GuildMember != null &&
                   character.GuildMember.HasRight(GuildRightsBitEnum.GUILD_RIGHT_USE_PADDOCKS);
        }

        /// <summary>A vendre : sans proprietaire, ou mis en vente par sa guilde.</summary>
        public bool IsForSale => !IsPublicPaddock() && (!HasOwner || OnSale);

        /// <summary>"Acheter" : un meneur de guilde, enclos a vendre qui n'est pas deja a sa guilde.</summary>
        public bool CanBuy(Character character) => IsForSale && IsGuildBoss(character) && character.Guild.Id != (HasOwner ? Guild.Id : NO_OWNER_GUILD_ID);

        public int MountsCount => PaddockedMounts.Count;

        public bool IsFull => !IsPublicPaddock() && MountsCount >= MaxOutdoorMount;

        public void SetOwner(Guild guild)
        {
            Guild = guild;
            OnSale = false;
            Locked = false;
            Abandonned = false;
        }

        public void SetForSale(bool forSale, int price)
        {
            OnSale = forSale;
            if (forSale)
                Price = price;
        }

        /// <summary>L'enclos n'a plus de guilde (guilde supprimee) : il redevient a vendre, a son dernier prix.</summary>
        public void Release()
        {
            Record.Guild = null; // remet aussi GuildId a null (= public) -> corrige juste apres
            Record.GuildId = NO_OWNER_GUILD_ID;
            IsRecordDirty = true;
            OnSale = true;
            Locked = false;
        }

        #endregion

        public void AddMountToStable(Mount mount)
        {
            if (mount.Paddock == null && !mount.IsInStable)
            {
                mount.Paddock = this;
                mount.IsInStable = true;
            }
        }

        public void RemoveMountFromStable(Mount mount)
        {
            if (mount.Paddock == this)
            {
                mount.Paddock = null;
                mount.IsInStable = false;
            }
        }

        public void AddMountToPaddock(Mount mount)
        {
            if (!mount.IsInStable)
            {
                IsRecordDirty = true;

                if (!IsPublicPaddock())
                {
                    m_mounts.Add(mount);
                    Record.Mounts.Add(mount.Record);
                }
                else
                    mount.Owner.AddPublicPaddockedMount(mount);

                mount.Paddock = this;
            }
        }

        public void RemoveMountFromPaddock(Mount mount)
        {
            if (mount.Paddock == this && !mount.IsInStable)
            {
                IsRecordDirty = true;

                if (!IsPublicPaddock())
                {
                    m_mounts.Remove(mount);
                    Record.Mounts.Remove(mount.Record);
                }
                else
                    mount.Owner.RemovePublicPaddockedMount(mount);

                mount.Paddock = null;
            }
        }

        public Mount GetPaddockedMount(Character character, int mountId)
        {
            return IsPublicPaddock() ? character.GetPublicPaddockedMount(mountId) : PaddockedMounts.FirstOrDefault(x => x.Id == mountId);
        }

        #region Network

        public PaddockPropertiesMessage GetPaddockPropertiesMessage()
        {
            // NEKFEU : enclos de guilde -> nom/blason de la guilde ; prix affiche seulement s'il est a vendre
            var instances = IsPublicPaddock() ? new PaddockBuyableInformations[0] : new[] { GetBuyableInformations() };

            var properties = new PaddockInstancesInformations((ushort)MaxOutdoorMount, (ushort)MaxItems, instances);
            return new PaddockPropertiesMessage(properties);
        }

        public PaddockBuyableInformations GetBuyableInformations()
        {
            if (HasOwner)
                return new PaddockGuildedInformations(OnSale ? (ulong)Price : 0, Locked, false, Guild.GetGuildInformations());

            return new PaddockBuyableInformations((ulong)Price, Locked);
        }

        public PaddockContentInformations GetPaddockContentInformations()
            => new PaddockContentInformations((ushort)MaxOutdoorMount, (ushort)MaxItems, Id, (short)Map.Position.X, (short)Map.Position.Y,
                Map.Id, (ushort)(Map.SubArea?.Id ?? 0), Abandonned, PaddockedMounts.Select(x => x.GetMountInformationsForPaddock()).ToArray());

        #endregion
    }
}