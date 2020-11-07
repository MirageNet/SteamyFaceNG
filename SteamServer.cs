#region Statements

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Steamworks;
using UnityEngine;

#endregion

namespace Mirror.FizzySteam
{
    public class SteamServer : SteamCommon
    {
        private static readonly ILogger Logger = LogFactory.GetLogger(typeof(SteamServer));

        #region Variables

        private readonly IDictionary<SteamId, SteamConnection> _connectedSteamUsers;
        private readonly ConcurrentQueue<Message> _connectionQueue = new ConcurrentQueue<Message>();
        private Message _msgBuffer;

        #endregion

        #region Class Specific

        /// <summary>
        ///     Initialize new <see cref="SteamServer"/> server connection.
        /// </summary>
        /// <param name="options">The options we want our server to run.</param>
        public SteamServer(SteamOptions options) : base(options)
        {
            Options = options;
            _connectedSteamUsers = new Dictionary<SteamId, SteamConnection>(Options.MaxConnections);

            SteamNetworking.AllowP2PPacketRelay(Options.AllowSteamRelay);
            SteamNetworking.OnP2PSessionRequest = OnConnectionRequest;
        }

        /// <summary>
        ///     Connection request from a Steam user.
        /// </summary>
        /// <param name="steamId">ID of the requesting user</param>
        private void OnConnectionRequest(SteamId steamId)
        {
            if (_connectedSteamUsers.ContainsKey(steamId)) {
                if (Logger.logEnabled) {
                    Logger.LogWarning($"SteamServer client {steamId} has already been added to connection list. Disconnecting old user.");
                }

                _connectedSteamUsers[steamId].Disconnect();
            }

            if (Logger.logEnabled)
                Logger.Log($"SteamServer request from {steamId}. Server accepting.");

            SteamNetworking.AcceptP2PSessionWithUser(steamId);
        }

        /// <summary>
        ///     Steam transport way of scanning for connections as steam itself
        ///     uses events to trigger connections versus a real listening connection.
        /// </summary>
        /// <returns></returns>
        public async UniTask<SteamConnection> QueuedConnectionsAsync()
        {
            // Check to see if we received a connection message.
            if (_connectionQueue.Count <= 0) return null;

            // It was data connection let's pull data out.
            _connectionQueue.TryDequeue(out _msgBuffer);

            if (_connectedSteamUsers.Count >= Options.MaxConnections)
            {
                SteamSend(_msgBuffer.steamId, InternalMessages.TooManyUsers);

                return null;
            }

            if (_connectedSteamUsers.ContainsKey(_msgBuffer.steamId)) return null;

            Options.ConnectionAddress = _msgBuffer.steamId;

            var client = new SteamConnection(Options);

            if (Logger.logEnabled)
                Logger.Log($"SteamServer connecting with {_msgBuffer.steamId} and accepting handshake.");

            _connectedSteamUsers.Add(_msgBuffer.steamId, client);

            SteamSend(_msgBuffer.steamId, InternalMessages.Accept);

            return await UniTask.FromResult(_msgBuffer.steamId == 0 ? null : client);
        }

        public void StartListening()
        {
            if (Logger.logEnabled) Logger.Log("SteamServer listening for incoming connections....");
        }

        #endregion

        #region Overrides of SteamCommon

        /// <summary>
        ///     Disconnect connection.
        /// </summary>
        public override void Disconnect()
        {
            if (Logger.logEnabled) Logger.Log("SteamServer shutting down.");

            base.Disconnect();
        }

        /// <summary>
        ///     Process our internal messages away from mirror.
        /// </summary>
        /// <param name="type">The <see cref="InternalMessages"/> type message we received.</param>
        /// <param name="clientSteamId">The client id which the internal message came from.</param>
        protected override void OnReceiveInternalData(InternalMessages type, SteamId clientSteamId)
        {
            switch (type)
            {
                case InternalMessages.Disconnect:
                    if (Logger.logEnabled)
                        Logger.Log("Received internal message to disconnect steam user.");

                    if (_connectedSteamUsers.TryGetValue(clientSteamId, out var connection))
                    {
                        connection.Disconnect();
                        SteamNetworking.CloseP2PSessionWithUser(clientSteamId);
                        _connectedSteamUsers.Remove(clientSteamId);

                        if (Logger.logEnabled)
                            Logger.Log($"Client with SteamID {clientSteamId} disconnected.");
                    }

                    break;

                case InternalMessages.Connect:
                    _msgBuffer = new Message(clientSteamId, InternalMessages.Connect, new[] {(byte) type}, Options.Channels.Length);
                    _connectionQueue.Enqueue(_msgBuffer);
                    break;
                    
                default:
                    if (Logger.logEnabled)
                        Logger.Log(
                            $"SteamConnection cannot process internal message {type}. If this is anything other then {InternalMessages.Data} something has gone wrong.");
                    break;
            }
        }

        protected override void ProcessIncomingMessages()
        {
            while (Connected)
            {
                while (DataAvailable(out SteamId clientSteamId, out byte[] internalMessage, Options.Channels.Length))
                {
                    if (internalMessage.Length != 1) continue;

                    OnReceiveInternalData((InternalMessages)internalMessage[0], clientSteamId);

                    break;
                }
            }
        }

        /// <summary>
        ///     Process data incoming from steam backend.
        /// </summary>
        /// <param name="data">The data that has come in.</param>
        /// <param name="clientSteamId">The client the data came from.</param>
        /// <param name="channel">The channel the data was received on.</param>
        protected override void OnReceiveData(byte[] data, SteamId clientSteamId, int channel)
        {
            var dataMsg = new Message(clientSteamId, InternalMessages.Data, data, channel);

            if (Logger.logEnabled)
                Logger.Log(
                    $"SteamConnection: Queue up message Event Type: {dataMsg.eventType} data: {BitConverter.ToString(dataMsg.data)}");
        }

        #endregion
    }
}
