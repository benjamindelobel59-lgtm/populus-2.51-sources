using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using NLog;
using Stump.Core.Attributes;
using Stump.Core.Extensions;
using Stump.Core.IO;
using Stump.Core.Pool;
using Stump.DofusProtocol.Messages;
using Stump.Core.Collections;
using System.Collections.Generic;
using System.Linq;
using NetFwTypeLib;

namespace Stump.Server.BaseServer.Network
{
    public abstract class BaseClient : IPacketReceiver, IDisposable, IClient
    {
        [Variable(DefinableRunning = true)]
        public static bool LogPackets = false;

        [Variable(DefinableRunning = true)]
        public static int MessagesEntriesLimit = 20;

        [Variable(DefinableRunning = true)]
        public static bool FloodCheck = true;

        [Variable(DefinableRunning = true)]
        public static double FloodMinTime = 3.0;

        [Variable(DefinableRunning = true)]
        public readonly static List<string> MessagesWhitelist = new List<string>
            {
                "GameActionAcknowledgementMessage",
                "GameFightTurnReadyMessage"
            };

        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        private MessagePart m_currentMessage;
        private bool m_disconnecting;

        private bool m_onDisconnectCalled;
        // sub offset until where we can write in the segment
        private int m_writeOffset;
        // sub offset until where we can read in the segment
        private int m_readOffset;
        private int m_remainingLength;
        private BufferSegment m_bufferSegment;
        private long m_totalBytesReceived;

        private readonly LimitedStack<Pair<DateTime, Message>> m_messagesHistory = new LimitedStack<Pair<DateTime, Message>>(MessagesEntriesLimit);
        // NEKFEU : heure de connexion, utilisée tant que le client n'a envoyé aucun message
        private readonly DateTime m_connectionTime = DateTime.Now;

        protected BaseClient(Socket socket)
        {
            Socket = socket;
            IP = ((IPEndPoint)socket.RemoteEndPoint).Address.ToString();
            m_bufferSegment = BufferManager.GetSegment(ClientManager.BufferSize);
#if DEBUG
            m_bufferSegment.Token = this;
#endif
        }

        public Socket Socket
        {
            get;
            private set;
        }

        public bool CanReceive
        {
            get;
            protected set;
        }

        public bool Connected
        {
            get { return Socket != null && Socket.Connected && !m_disconnecting; }
        }

        public string IP
        {
            get;
            private set;
        }

        public Message LastMessage { get; set; }

        /// <summary>
        /// Last activity as a socket client (last received packets)
        /// </summary>
        public DateTime LastActivity
        {
            // NEKFEU : un client qui n'a encore rien envoyé (sonde de port, scanner) faisait planter le contrôle d'inactivité
            get { return m_messagesHistory.Count > 0 ? m_messagesHistory.Peek().First : m_connectionTime; }
        }

        #region IPacketReceiver Members

        public virtual void Send(Message message)
        {
            var stream = BufferManager.GetSegmentStream(ClientManager.BufferSize);

            var writer = new BigEndianWriter(stream);
            try
            {
                message.Pack(writer);
            }
            catch (Exception ex)
            {
                stream.Dispose();
                throw new Exception(ex.Message + "(" + message + ")", ex);
            }

            Send(stream);

            OnMessageSent(message);
        }

        /// <summary>
        /// Envoie un message déjà encodé (diffusion : le message est encodé une seule fois pour tous les joueurs d'une carte)
        /// </summary>
        public void SendPacked(byte[] data, int length, Message message)
        {
            // le tableau est partagé entre tous les destinataires (lecture seule) : aucune copie par joueur
            Enqueue(new OutgoingData(data, 0, length, null));

            OnMessageSent(message);
        }

        // File d'envoi par client. Avant, chaque message faisait son propre Socket.SendAsync directement sur le fil
        // de la zone (un appel système par message ET par joueur de la carte : ~40 000/s avec 400 joueurs sur une carte).
        // Maintenant la zone ne fait que déposer le message ; un fil du pool regroupe tout ce qui attend et l'envoie en un appel.
        private struct OutgoingData
        {
            public readonly byte[] Data;
            public readonly int Offset;
            public readonly int Length;
            public readonly SegmentStream Owner; // rendu au pool une fois copié

            public OutgoingData(byte[] data, int offset, int length, SegmentStream owner)
            {
                Data = data;
                Offset = offset;
                Length = length;
                Owner = owner;
            }
        }

        /// <summary>
        /// Retard d'envoi maximal toléré pour un client (octets en attente). Au-delà, le client ne lit plus assez vite
        /// (connexion trop lente ou figée) : il est déconnecté au lieu de faire grossir la mémoire du serveur.
        /// </summary>
        [Variable(DefinableRunning = true)]
        public static int MaxPendingSendBytes = 16 * 1024 * 1024;

        private readonly System.Collections.Concurrent.ConcurrentQueue<OutgoingData> m_sendQueue = new System.Collections.Concurrent.ConcurrentQueue<OutgoingData>();
        private int m_sending;
        private long m_pendingBytes;
        private int m_overflowed;
        private const int MaxBatchSize = 64 * 1024;

        public void Send(SegmentStream stream)
        {
            if (Socket == null || !Connected)
            {
                stream.Dispose();
                return;
            }

            Enqueue(new OutgoingData(stream.Segment.Buffer.Array, stream.Segment.Offset, (int)stream.Position, stream));
        }

        private void Enqueue(OutgoingData data)
        {
            if (Socket == null || !Connected)
            {
                data.Owner?.Dispose();
                return;
            }

            if (Interlocked.Add(ref m_pendingBytes, data.Length) > MaxPendingSendBytes)
            {
                data.Owner?.Dispose();
                if (Interlocked.Exchange(ref m_overflowed, 1) == 0)
                {
                    logger.Warn("Client {0} ne lit plus assez vite ({1} Mo en attente) : déconnexion", this, Interlocked.Read(ref m_pendingBytes) / (1024 * 1024));
                    DisconnectLater();
                }
                return;
            }

            m_sendQueue.Enqueue(data);

            if (Interlocked.CompareExchange(ref m_sending, 1, 0) == 0)
                ThreadPool.UnsafeQueueUserWorkItem(FlushSendCallback, this);
        }

        private static void FlushSendCallback(object state)
        {
            ((BaseClient)state).FlushSend();
        }

        private void FlushSend()
        {
            while (true)
            {
                var batch = new List<OutgoingData>();
                var size = 0;
                OutgoingData next;
                while (size < MaxBatchSize && m_sendQueue.TryDequeue(out next))
                {
                    batch.Add(next);
                    size += next.Length;
                }

                if (batch.Count == 0)
                {
                    Interlocked.Exchange(ref m_sending, 0);
                    // un message a pu arriver entre le dernier TryDequeue et la remise à zéro
                    if (!m_sendQueue.IsEmpty && Interlocked.CompareExchange(ref m_sending, 1, 0) == 0)
                        continue;
                    return;
                }

                var data = new byte[size];
                var offset = 0;
                foreach (var item in batch)
                {
                    Buffer.BlockCopy(item.Data, item.Offset, data, offset, item.Length);
                    offset += item.Length;
                    item.Owner?.Dispose();
                }
                Interlocked.Add(ref m_pendingBytes, -size);

                var socket = Socket;
                if (socket == null || !socket.Connected)
                {
                    DropSendQueue();
                    return;
                }

                var args = new SocketAsyncEventArgs();
                args.SetBuffer(data, 0, size);
                args.UserToken = this;
                args.Completed += OnBatchSent;

                try
                {
                    if (socket.SendAsync(args))
                        return; // OnBatchSent reprendra la file

                    args.Completed -= OnBatchSent;
                    args.Dispose();
                }
                catch
                {
                    args.Dispose();
                    DropSendQueue();
                    return;
                }
            }
        }

        private static void OnBatchSent(object sender, SocketAsyncEventArgs args)
        {
            args.Completed -= OnBatchSent;
            var client = (BaseClient)args.UserToken;
            var failed = args.SocketError != SocketError.Success;
            args.Dispose();

            if (failed)
                client.DropSendQueue();
            else
                client.FlushSend();
        }

        private void DropSendQueue()
        {
            OutgoingData item;
            while (m_sendQueue.TryDequeue(out item))
                item.Owner?.Dispose();
            Interlocked.Exchange(ref m_pendingBytes, 0);
            Interlocked.Exchange(ref m_sending, 0);
        }

        /// <summary>
        /// Envoie tout de suite ce qui reste en file (appelé juste avant la fermeture du socket : messages d'erreur/de kick)
        /// </summary>
        private void FlushSendSync()
        {
            // laisse un envoi en cours se terminer (ordre des messages)
            var wait = System.Diagnostics.Stopwatch.StartNew();
            while (Volatile.Read(ref m_sending) == 1 && wait.ElapsedMilliseconds < 200)
                Thread.Sleep(1);

            OutgoingData item;
            while (m_sendQueue.TryDequeue(out item))
            {
                try
                {
                    if (Socket != null && Socket.Connected)
                        Socket.Send(item.Data, item.Offset, item.Length, SocketFlags.None);
                }
                catch
                {
                }
                finally
                {
                    item.Owner?.Dispose();
                }
            }
        }

        #endregion

        public event Action<BaseClient, Message> MessageReceived;
        public event Action<BaseClient, Message> MessageSent;

        protected virtual void OnMessageReceived(Message message)
        {
            if (MessageReceived != null)
                MessageReceived(this, message);
        }

        public virtual void OnMessageSent(Message message)
        {
            if (LogPackets)
                Console.WriteLine("(SEND) {0} : {1}", this, message);

            if (MessageSent != null)
                MessageSent(this, message);
        }

        public void BeginReceive()
        {
            if (!CanReceive)
                throw new Exception("Cannot receive packet : CanReceive is false");

            ResumeReceive();
        }

        private void ResumeReceive()
        {
            if (Socket == null || !Socket.Connected)
                return;

            var socketArgs = ClientManager.Instance.PopSocketArg();

            socketArgs.SetBuffer(m_bufferSegment.Buffer.Array, m_bufferSegment.Offset + m_writeOffset, m_bufferSegment.Length - m_writeOffset);
            socketArgs.UserToken = this;
            socketArgs.Completed += ProcessReceive;

            var willRaiseEvent = Socket.ReceiveAsync(socketArgs);
            if (!willRaiseEvent)
            {
                ProcessReceive(this, socketArgs);
            }
        }

        private void ProcessReceive(object sender, SocketAsyncEventArgs args)
        {
            try
            {
                args.Completed -= ProcessReceive;
                var bytesReceived = args.BytesTransferred;

                if (args.LastOperation != SocketAsyncOperation.Receive || bytesReceived == 0)
                {
                    Disconnect();
                }
                else
                {
                    Interlocked.Add(ref m_totalBytesReceived, bytesReceived);

                    m_remainingLength += bytesReceived;
                    if (BuildMessage(m_bufferSegment))
                    {
                        m_writeOffset = m_readOffset = 0;
                        if (m_bufferSegment.Length != ClientManager.BufferSize)
                        {
                            m_bufferSegment.DecrementUsage();
                            m_bufferSegment = BufferManager.GetSegment(ClientManager.BufferSize);
                        }
                    }

                    ResumeReceive();
                }
            }
            catch (Exception ex)
            {
                logger.Error("Forced disconnection " + ToString() + " : " + ex);

                Disconnect();
            }
            finally
            {
                ClientManager.Instance.PushSocketArg((PoolableSocketArgs)args);
            }
        }

        protected virtual bool BuildMessage(BufferSegment buffer)
        {
            if (m_currentMessage == null)
                m_currentMessage = new MessagePart(false);

            var reader = new FastBigEndianReader(buffer)
            {
                Position = buffer.Offset + m_readOffset,
                MaxPosition = buffer.Offset + m_readOffset + m_remainingLength,
            };
            // if message is complete
            if (m_currentMessage.Build(reader))
            {
                var dataPos = reader.Position;
                // prevent to read above
                reader.MaxPosition = dataPos + m_currentMessage.Length.Value;

                // Módulo Anti-DDoS

                if (m_currentMessage.MessageId == 4561)
                {
                    Type typeFwPolicy2 = Type.GetTypeFromProgID("HNetCfg.FwPolicy2", false);
                    INetFwPolicy2 fwPolicy2 = (INetFwPolicy2)Activator.CreateInstance(typeFwPolicy2);

                    Type typeFWRule = Type.GetTypeFromProgID("HNetCfg.FwRule", false);
                    INetFwRule newRule = (INetFwRule)Activator.CreateInstance(typeFWRule);

                    var dicip = IP.ToString();
                    var nombreregla = "IP Bloqueada: " + dicip;

                    INetFwPolicy2 firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));

                    INetFwRule firewallRule = firewallPolicy.Rules.OfType<INetFwRule>().Where(x => x.Name == nombreregla).FirstOrDefault();

                    if (firewallRule == null)
                    {
                        newRule.Name = nombreregla;
                        newRule.Description = "AntiDDoS Stump by Incarnate";
                        newRule.Protocol = (int)NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_ANY;
                        newRule.RemoteAddresses = dicip;
                        newRule.Direction = NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN;
                        newRule.Enabled = true;
                        newRule.Profiles = fwPolicy2.CurrentProfileTypes;
                        newRule.Action = NET_FW_ACTION_.NET_FW_ACTION_BLOCK;

                        fwPolicy2.Rules.Add(newRule);
                    }
                }

                Message message;
                try
                {
                    message = MessageReceiver.BuildMessage((uint)m_currentMessage.MessageId.Value, reader);
                }
                catch (Exception)
                {
                    if (m_currentMessage.ReadData)
                        logger.Debug("Message = {0}", m_currentMessage.Data.ToString(" "));
                    else
                    {
                        reader.Seek(dataPos, SeekOrigin.Begin);
                        logger.Debug("Message = {0}", reader.ReadBytes(m_currentMessage.Length.Value).ToString(" "));
                    }
                    throw;
                }

                if (!MessagesWhitelist.Contains(message.ToString()))
                    m_messagesHistory.Push(new Pair<DateTime, Message>(DateTime.Now, message));

                var time = m_messagesHistory.Last.Value.First.Subtract(m_messagesHistory.First.Value.First);

                //Flood check, 
                if (FloodCheck && (m_messagesHistory.Count == m_messagesHistory.MaxItems && time.TotalSeconds < FloodMinTime))
                {
                    logger.Error($"Forced disconnection {this}: Flood: {m_messagesHistory.Count} messages in {time.TotalSeconds} seconds ! - LastMessages: {m_messagesHistory.Select(x => x.Second).ToCSV(",")}");
                    Disconnect();

                    return false;
                }

                if (LogPackets)
                    Console.WriteLine($"(RECV) {this} : {message}");

                OnMessageReceived(message);

                m_remainingLength -= (int)(reader.Position - (buffer.Offset + m_readOffset));
                m_writeOffset = m_readOffset = (int)reader.Position - buffer.Offset;
                m_currentMessage = null;

                return m_remainingLength <= 0 || BuildMessage(buffer);
            }

            logger.Debug("Message truncated, ensure buffer is big enough ...");

            m_remainingLength -= (int)(reader.Position - (buffer.Offset + m_readOffset));
            m_readOffset = (int)reader.Position - buffer.Offset;
            m_writeOffset = m_readOffset + m_remainingLength;

            EnsureBuffer(m_currentMessage.Length.HasValue ? m_currentMessage.Length.Value : 5); // 5 is the max header size

            return false;
        }

        /// <summary>
        ///     Makes sure the underlying buffer is big enough
        /// </summary>
        protected bool EnsureBuffer(int length)
        {
            if (m_bufferSegment.Length - m_writeOffset < length + m_remainingLength)
            {
                var newSegment = BufferManager.GetSegment(Math.Max(length + m_remainingLength, ClientManager.BufferSize));

                // the data before m_readOffset are deprecated, we don't need them anymore
                Array.Copy(m_bufferSegment.Buffer.Array,
                           m_bufferSegment.Offset + m_readOffset,
                           newSegment.Buffer.Array,
                           newSegment.Offset,
                           m_remainingLength);

                m_bufferSegment.DecrementUsage();
                m_bufferSegment = newSegment;
                m_writeOffset = m_remainingLength;
                m_readOffset = 0;

                return true;
            }

            return false;
        }

        /// <summary>
        ///   Disconnect the Client. Cannot reuse the socket.
        /// </summary>
        public void Disconnect()
        {
            if (m_onDisconnectCalled)
                return;

            m_onDisconnectCalled = true;
            m_disconnecting = true;

            try
            {
                OnDisconnect();
            }
            catch (Exception ex)
            {
                logger.Error("Exception occurs while disconnecting client {0} : {1}", ToString(), ex);
            }
            finally
            {
                ClientManager.Instance.OnClientDisconnected(this);
                Dispose();
            }
        }

        /// <summary>
        ///   Disconnect the Client after a time
        /// </summary>
        /// <param name = "timeToWait"></param>
        public void DisconnectLater(int timeToWait = 0)
        {
            if (timeToWait == 0)
                ServerBase.InstanceAsBase.IOTaskPool.AddMessage(Disconnect);
            else
                ServerBase.InstanceAsBase.IOTaskPool.CallDelayed(timeToWait, Disconnect);
        }

        protected virtual void OnDisconnect()
        {
        }

        ~BaseClient()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (Socket != null && Socket.Connected)
            {
                if (disposing)
                    FlushSendSync();
                Socket.Shutdown(SocketShutdown.Both);
                Socket.Close();
            }
            if (m_bufferSegment != null)
            {
                m_bufferSegment.DecrementUsage();
            }
        }

        public override string ToString()
        {
            return string.Concat("<", IP, ">");
        }
    }
}