#region Statements

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Steamworks;
using UnityEngine;

#endregion

namespace Mirage.FizzySteam
{
    public class SteamServer : SteamCommon
    {
        private static readonly ILogger Logger = LogFactory.GetLogger(typeof(SteamServer));

        #region Variables

        private readonly Transport _transport;
        private readonly IDictionary<SteamId, SteamConnection> _connectedSteamUsers;

        #endregion

        #region Class Specific

        /// <summary>
        ///     Initialize new <see cref="SteamServer"/> server connection.
        /// </summary>
        /// <param name="options">The options we want our server to run.</param>
        public SteamServer(SteamOptions options, Transport transport) : base(options)
        {
            Options = options;
            _transport = transport;
            _connectedSteamUsers = new Dictionary<SteamId, SteamConnection>();
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

        public void StartListening()
        {
            if (Logger.logEnabled) Logger.Log("SteamServer listening for incoming connections....");

            SteamNetworking.AllowP2PPacketRelay(Options.AllowSteamRelay);
            SteamNetworking.OnP2PSessionRequest = OnConnectionRequest;

            _transport.Started.Invoke();
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
            
            _connectedSteamUsers.Clear();
            SteamNetworking.OnP2PSessionRequest = null;
            SteamNetworking.OnP2PConnectionFailed = null;
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
                    if (_connectedSteamUsers.Count >= Options.MaxConnections)
                    {
                        SteamSend(clientSteamId, InternalMessages.TooManyUsers);

                        return;
                    }

                    if (_connectedSteamUsers.ContainsKey(clientSteamId)) return;

                    Options.ConnectionAddress = clientSteamId;

                    var client = new SteamConnection(Options);

                    _transport.Connected.Invoke(client);

                    if (Logger.logEnabled)
                        Logger.Log($"SteamServer connecting with {clientSteamId} and accepting handshake.");

                    _connectedSteamUsers.Add(clientSteamId, client);

                    SteamSend(clientSteamId, InternalMessages.Accept);
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
