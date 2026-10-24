using System;
using System.Collections.Generic;
using System.Net.Sockets;
using NLog;
using Stump.DofusProtocol.Messages;
using Stump.Server.BaseServer.IPC.Objects;
using Stump.Server.BaseServer.Network;
using Stump.Server.WorldServer.Database.Accounts;
using Stump.Server.WorldServer.Database.Characters;
using Stump.Server.WorldServer.Game.Accounts;
using Stump.Server.WorldServer.Game.Accounts.Startup;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Handlers.Approach;
using Stump.Server.WorldServer.Handlers.Basic;

namespace Stump.Server.WorldServer.Core.Network
{
    public sealed class WorldClient : BaseClient
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public WorldClient(Socket socket)
            : base(socket)
        {
            Send(new ProtocolRequired(VersionExtension.ProtocolRequired, VersionExtension.ActualProtocol));
            Send(new HelloGameMessage());

            CanReceive = true;
            StartupActions = new List<StartupAction>();

            lock (ApproachHandler.ConnectionQueue.SyncRoot)
                ApproachHandler.ConnectionQueue.Add(this);
                
            InQueueUntil = DateTime.Now;
        }

        public bool AutoConnect
        {
            get;
            set;
        }

        public AccountData Account
        {
            get;
            private set;
        }

        public DateTime InQueueUntil
        {
            get;
            set;
        }

        public bool QueueShowed
        {
            get;
            set;
        }

        public WorldAccount WorldAccount
        {
            get;
            internal set;
        }

        public List<StartupAction> StartupActions
        {
            get;
            private set;
        }

        public List<CharacterRecord> Characters
        {
            get;
            internal set;
        }

        public CharacterRecord ForceCharacterSelection
        {
            get;
            set;
        }

        public Character Character
        {
            get;
            internal set;
        }

        public UserGroup UserGroup
        {
            get;
            private set;
        }

        public void SetCurrentAccount(AccountData account)
        {
            if (Account != null)
                throw new Exception("Account already set");

            Account = account;
            Characters = CharacterManager.Instance.GetCharactersByAccount(this);
            UserGroup = AccountManager.Instance.GetGroupOrDefault(account.UserGroupId);

            if (UserGroup == AccountManager.DefaultUserGroup)
                logger.Error("Group {0} not found. Use default group instead !", account.UserGroupId);
        }

        /// <summary>PADGREF 23/10 : reçoit chaque message envoyé à ce client (replay : joueur « témoin » d'un combat).</summary>
        public Action<Message> Observateur;

        public override void OnMessageSent(Message message)
        {
            base.OnMessageSent(message);
            var o = Observateur;
            if (o != null)
            {
                try { o(message); }
                catch (Exception ex) { logger.Error("Observateur du client : {0}", ex.Message); }
            }
        }

        protected override void OnMessageReceived(Message message)
        {
            WorldPacketHandler.Instance.Dispatch(this, message);

            base.OnMessageReceived(message);
        }

        public void DisconnectAfk()
        {
            BasicHandler.SendSystemMessageDisplayMessage(this, true, 1);

            Disconnect();
        }

        protected override void OnDisconnect()
        {
            if (Character != null)
            {
                Character.LogOut();
            }

            WorldServer.Instance.IOTaskPool.AddMessage(() =>
            {
                if (WorldAccount == null)
                    return;

                WorldAccount.ConnectedCharacter = null;
                WorldServer.Instance.DBAccessor.Database.Update(WorldAccount);
            });

            base.OnDisconnect();
        }

        public override string ToString() => base.ToString() + (Account != null ? " (" + Account.Login + ")" : "");
    }
}