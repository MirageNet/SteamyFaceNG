#region Statements

using System;
using System.Collections.Concurrent;
using System.Threading;
using Cysharp.Threading.Tasks;
using Steamworks;
using Steamworks.Data;
using UnityEngine;
using Mirage.Logging;

#endregion

namespace Mirage.FizzySteam
{
    public abstract class SteamCommon
    {
        #region Variables

        private static readonly ILogger Logger = LogFactory.GetLogger(typeof(SteamCommon));

        private readonly CancellationTokenSource _cancellationToken = new CancellationTokenSource();
        internal readonly ConcurrentQueue<Message> QueuedData = new ConcurrentQueue<Message>();
        protected SteamOptions Options;
        public Action<ErrorCodes, string> Error;

        #endregion

        #region Class Specific

        public bool Connected => _cancellationToken.IsCancellationRequested != true;

        protected SteamCommon(SteamOptions options)
        {
            Options = options;
            SteamNetworking.OnP2PConnectionFailed = OnConnectionFailed;
            _ = UniTask.Run(ProcessIncomingMessages);
        }

        public virtual void Disconnect()
        {
            _cancellationToken?.Cancel();
        }

        /// <summary>
        ///     Connection request has failed to connect to user.
        /// </summary>
        /// <param name="result">The information back from steam.</param>
        protected virtual void OnConnectionFailed(SteamId steamId, P2PSessionError error)
        {
            string errorMessage;

            switch (error)
            {
                case P2PSessionError.NotRunningApp:
                    errorMessage = "Connection failed: The target user is not running the same game.";

                    Error?.Invoke((ErrorCodes)error, errorMessage);

                    if (Logger.logEnabled)
                        Logger.LogError(new Exception(errorMessage));
                    break;
                case P2PSessionError.NoRightsToApp:

                    errorMessage = "Connection failed: The local user doesn't own the app that is running.";

                    Error?.Invoke((ErrorCodes)error, errorMessage);

                    if (Logger.logEnabled)
                        Logger.LogError(
                        new Exception(errorMessage));
                    break;
                case P2PSessionError.DestinationNotLoggedIn:

                    errorMessage = "Connection failed: The target user is not running the same game.";

                    Error?.Invoke((ErrorCodes)error, errorMessage);

                    if (Logger.logEnabled)
                        Logger.LogError(new Exception(errorMessage));
                    break;
                case P2PSessionError.Timeout:

                    errorMessage = "Connection failed: The connection timed out because the target user didn't respond.";

                    Error?.Invoke((ErrorCodes)error, errorMessage);

                    if (Logger.logEnabled)
                        Logger.LogError(new Exception(errorMessage));
                    break;
                default:

                    errorMessage = $"Connection failed: Unknown: {error}";

                    Error?.Invoke((ErrorCodes)error, errorMessage);

                    if (Logger.logEnabled)
                        Logger.LogError(new Exception(errorMessage));
                    break;
            }

            _cancellationToken.Cancel();
        }

        /// <summary>
        ///     Send an internal message through system.
        /// </summary>
        /// <param name="target">The steam person we are sending internal message to.</param>
        /// <param name="type">The type of <see cref="InternalMessages"/> we want to send.</param>
        internal bool SteamSend(SteamId target, InternalMessages type)
        {
            return SteamNetworking.SendP2PPacket(target, new[] { (byte)type }, 1, Options.Channels.Length);
        }

        /// <summary>
        ///     Process our internal messages away from mirror.
        /// </summary>
        /// <param name="type">The <see cref="InternalMessages"/> type message we received.</param>
        /// <param name="clientSteamId">The client id which the internal message came from.</param>
        protected abstract void OnReceiveInternalData(InternalMessages type, SteamId clientSteamId);

        /// <summary>
        ///     Process data incoming from steam backend.
        /// </summary>
        /// <param name="data">The data that has come in.</param>
        /// <param name="clientSteamId">The client the data came from.</param>
        /// <param name="channel">The channel the data was received on.</param>
        protected abstract void OnReceiveData(byte[] data, SteamId clientSteamId, int channel);

        /// <summary>
        ///     Update method to be called by the transport.
        /// </summary>
        protected abstract void ProcessIncomingMessages();

        /// <summary>
        ///     Check to see if we have received any data from steam users.
        /// </summary>
        /// <param name="clientSteamId">Returns back the steam id of users who sent message.</param>
        /// <param name="receiveBuffer">The data that was sent to use.</param>
        /// <param name="channel">The channel the data was sent on.</param>
        /// <returns></returns>
        protected bool DataAvailable(out SteamId clientSteamId, out byte[] receiveBuffer, int channel)
        {
            if (!SteamClient.IsValid) {
                receiveBuffer = null;
                clientSteamId = 0;
                return false;
            }

            if (SteamNetworking.IsP2PPacketAvailable(channel))
            {
                P2Packet? data = SteamNetworking.ReadP2PPacket(channel);
                if (data.HasValue) {
                    clientSteamId = data.Value.SteamId;
                    receiveBuffer = data.Value.Data;
                    return true;
                }
            }

            receiveBuffer = null;
            clientSteamId = 0;
            return false;
        }

        #endregion
    }
}
