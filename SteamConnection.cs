#region Statements

using System;
using System.IO;
using System.Net;
using Cysharp.Threading.Tasks;
using Steamworks;
using UnityEngine;

#endregion

namespace Mirror.FizzySteam
{
    public class SteamConnection : SteamCommon, IConnection
    {
        private static readonly ILogger Logger = LogFactory.GetLogger(typeof(SteamConnection));

        #region Variables

        private byte[] _clientSendPoolData;
        private Message _clientReceivePoolData;
        private Message _clientQueuePoolData;
        private UniTaskCompletionSource<UniTask> _connectedComplete;

        #endregion

        #region Class Specific

        /// <summary>
        ///     Connect to server.
        /// </summary>
        /// <returns></returns>
        public async UniTask<IConnection> ConnectAsync()
        {
            if (Logger.logEnabled) Logger.Log($"SteamConnection attempting connection to {Options.ConnectionAddress}");
            SteamNetworking.AllowP2PPacketRelay(Options.AllowSteamRelay);

            try
            {
                // Send a message to server to initiate handshake connection
                SteamSend(Options.ConnectionAddress, InternalMessages.Connect);

                _connectedComplete = new UniTaskCompletionSource<UniTask>();
                UniTask connectedCompleteTask = _connectedComplete.Task;

                while (
                    await UniTask.WhenAny(connectedCompleteTask,
                           UniTask.Delay(TimeSpan.FromSeconds(Math.Max(1, Options.ConnectionTimeOut)))) != 0)
                {
                    if (Logger.logEnabled)
                        Logger.LogError(
                            $"SteamConnection connection to {Options.ConnectionAddress} timed out.");

                    Error.Invoke(ErrorCodes.ConnectionFailed,
                        $"SteamConnection connection to {Options.ConnectionAddress} timed out.");

                    return null;
                }

                return this;
            }
            catch (FormatException)
            {
                Error?.Invoke(ErrorCodes.IncorrectStringFormat, $"Connection string was not in the correct format.");

                if (Logger.logEnabled)
                    Logger.LogError("SteamConnection connection string was not in the right format. Did you enter a SteamId?");
            }
            catch (Exception ex)
            {
                Error?.Invoke(ErrorCodes.None, $"Error: {ex.Message}");

                if (Logger.logEnabled)
                    Logger.LogError($"SteamConnection error: {ex.Message}");
            }

            return null;
        }

        #region Overrides of SteamCommon

        /// <summary>
        ///     Connection request has failed to connect to user.
        /// </summary>
        /// <param name="result">The information back from steam.</param>
        protected override void OnConnectionFailed(SteamId steamId, P2PSessionError error)
        {
            base.OnConnectionFailed(steamId, error);

            _connectedComplete.TrySetCanceled();
        }

        #endregion

        /// <summary>
        ///     Process our internal messages away from mirror.
        /// </summary>
        /// <param name="type">The <see cref="InternalMessages"/> type message we received.</param>
        /// <param name="clientSteamId">The client id which the internal message came from.</param>
        protected override void OnReceiveInternalData(InternalMessages type, SteamId clientSteamId)
        {
            if(!Connected) return;

            switch (type)
            {
                case InternalMessages.Accept:
                    if (Logger.logEnabled)
                        Logger.Log("Received internal message of server accepted our request to connect.");

                    _connectedComplete.TrySetResult(_connectedComplete.Task);

                    break;

                case InternalMessages.Disconnect:
                        Disconnect();

                        if (Logger.logEnabled)
                            Logger.Log("Received internal message to disconnect steam user.");

                        break;

                case InternalMessages.TooManyUsers:
                    if (Logger.logEnabled)
                        Logger.Log("Received internal message that there are too many users connected to server.");

                    // TODO Implement way to tell users server is full? Or does mirror do this?

                    break;

                default:
                    if (Logger.logEnabled)
                        Logger.Log(
                            $"SteamConnection cannot process internal message {type}. If this is anything other then {InternalMessages.Data} something has gone wrong.");
                    break;
            }
        }

        /// <summary>
        ///     Process incoming messages.
        /// </summary>
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

                for (int chNum = 0; chNum < Options.Channels.Length; chNum++)
                {
                    while (DataAvailable(out SteamId clientSteamId, out byte[] receiveBuffer, chNum))
                    {
                        OnReceiveData(receiveBuffer, clientSteamId, chNum);
                    }
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
            if(!Connected) return;

            _clientQueuePoolData = new Message(clientSteamId, InternalMessages.Data, data, channel);

            if (Logger.logEnabled)
                Logger.Log(
                    $"SteamConnection: Queue up message Event Type: {_clientQueuePoolData.eventType} data: {BitConverter.ToString(_clientQueuePoolData.data)}");

            QueuedData.Enqueue(_clientQueuePoolData);
        }

        /// <summary>
        ///     Send data through steam network.
        /// </summary>
        /// <param name="host">The person we want to send data to.</param>
        /// <param name="msgBuffer">The data we are sending.</param>
        /// <param name="channel">The channel we are going to send data on.</param>
        /// <returns></returns>
        private bool Send(SteamId host, byte[] msgBuffer, int channel)
        {
            return Connected && SteamNetworking.SendP2PPacket(host, msgBuffer, msgBuffer.Length, channel, Options.Channels[channel]);
        }

        /// <summary>
        ///     Initialize <see cref="SteamConnection"/>
        /// </summary>
        /// <param name="options"></param>
        public SteamConnection(SteamOptions options) : base(options)
        {
            Options = options;
            SteamNetworking.AllowP2PPacketRelay(Options.AllowSteamRelay);
        }

        #endregion

        #region Implementation of IConnection

        /// <summary>
        ///     Check if we have data in the pipe line that we need to process
        /// </summary>
        /// <param name="buffer">The buffer we need to write data too.</param>
        /// <returns></returns>
        public async UniTask<int> ReceiveAsync(MemoryStream buffer)
        {
            try
            {
                if (!Connected) throw new EndOfStreamException();

                while (QueuedData.Count <= 0)
                {
                    // Due to how steam works we have no connection state to be able to
                    // know when server disconnects us truly. So when steam sends a internal disconnect
                    // message we disconnect as normal but the _cancellation Token will trigger and we can exit cleanly
                    // using mirror.
                    if (!Connected) throw new EndOfStreamException();

                    await UniTask.Delay(1);
                }

                QueuedData.TryDequeue(out _clientReceivePoolData);

                buffer.SetLength(0);

                if (Logger.logEnabled)
                    Logger.Log(
                        $"SteamConnection processing message: {BitConverter.ToString(_clientReceivePoolData.data)}");

                await buffer.WriteAsync(_clientReceivePoolData.data, 0, _clientReceivePoolData.data.Length);

                return _clientReceivePoolData.Channel;
            }
            catch (EndOfStreamException)
            {
                throw new EndOfStreamException();
            }
        }

        /// <summary>
        ///     Disconnect steam user and close P2P session.
        /// </summary>
        public override async void Disconnect()
        {
            if(!Connected) return;

            if (Logger.logEnabled)
                Logger.Log("SteamConnection shutting down.");

            _clientSendPoolData = null;

            SteamSend(Options.ConnectionAddress, InternalMessages.Disconnect);

            // Wait 1 seconds to make sure the disconnect message gets fired.
            await UniTask.Delay(1000);

            base.Disconnect();

            SteamNetworking.CloseP2PSessionWithUser(Options.ConnectionAddress);
        }

        /// <summary>
        ///     Get the network address using steams id.
        /// </summary>
        /// <returns></returns>
        public EndPoint GetEndPointAddress()
        {
            return new DnsEndPoint(Options.ConnectionAddress.ToString(), 0);
        }

        /// <summary>
        ///     Send data on a specific channel.
        /// </summary>
        /// <param name="data">The data we want to send.</param>
        /// <param name="channel">The channel we want to send it on.</param>
        /// <returns></returns>
        public UniTask SendAsync(ArraySegment<byte> data, int channel)
        {
            if (!Connected) return UniTask.CompletedTask;

            _clientSendPoolData = new byte[data.Count];

            Array.Copy(data.Array, data.Offset, _clientSendPoolData, 0, data.Count);

            Send(Options.ConnectionAddress, _clientSendPoolData, channel);

            return UniTask.CompletedTask;
        }

        #endregion
    }
}
