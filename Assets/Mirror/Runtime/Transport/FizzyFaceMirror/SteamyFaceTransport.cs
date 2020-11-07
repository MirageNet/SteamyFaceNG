#region Statements

using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Steamworks;
using UnityEngine;

#endregion

namespace Mirror.FizzySteam
{
    public class SteamyFaceTransport : Transport
    {
        static readonly ILogger Logger = LogFactory.GetLogger(typeof(SteamyFaceTransport));

        #region Variables

        [Header("General"), Tooltip("Your Steam App ID"), SerializeField]
        private uint _appID = 480;

        [Header("Steam Server Config")] [SerializeField]
        private bool _allowSteamRelay = true;

        [SerializeField] public P2PSend[] Channels = new P2PSend[2]
            {P2PSend.Reliable, P2PSend.Unreliable};

        [SerializeField] private int _maxP2PConnections = 4;

        [Header("Steam Client Config")] [SerializeField]
        private int _clientConnectionTimeout = 30;

        private SteamServer _server;
        private SteamConnection _client;

        public Action<ErrorCodes, string> Error;

        #endregion

        #region Unity Methods

        private void Start() {
            if (!SteamClient.IsValid) {
                SteamClient.Init(_appID, false);
            }
        }

        private void Update() {
            if (!SteamClient.IsValid) return;

            SteamClient.RunCallbacks();
        }

        private void OnApplicationQuit()
        {
            _server?.Disconnect();
            _client?.Disconnect();
        }

        #endregion

        #region Overrides of Transport

        /// <summary>
        ///     Fires up our server and configs options for listening for connections.
        /// </summary>
        /// <returns></returns>
        public override UniTask ListenAsync()
        {
            var op = new SteamOptions
            {
                AllowSteamRelay = _allowSteamRelay, 
                MaxConnections = _maxP2PConnections, 
                Channels = Channels
            };

            _server = new SteamServer(op);

            _server.StartListening();

            return UniTask.CompletedTask;
        }

        /// <summary>
        ///     Disconnect the server and client and shutdown.
        /// </summary>
        public override void Disconnect()
        {
            if(Logger.logEnabled)
                Logger.Log("MirrorNGSteamTransport shutting down.");

            _server?.Disconnect();
            _server = null;
        }

        /// <summary>
        ///     Connect clients async to mirror backend.
        /// </summary>
        /// <param name="uri">The address we want to connect to using steam ids.</param>
        /// <returns></returns>
        public override async UniTask<IConnection> ConnectAsync(Uri uri)
        {
            var op = new SteamOptions
            {
                AllowSteamRelay = _allowSteamRelay,
                ConnectionAddress = ulong.Parse(uri.Host),
                ConnectionTimeOut = _clientConnectionTimeout,
                Channels = Channels
            };

            _client = new SteamConnection(op);
            _client.Error += (errorCode, message) => Error?.Invoke(errorCode, message);

            await _client.ConnectAsync();

            return _client;
        }

        /// <summary>
        ///     Server's different connection scheme's
        /// </summary>
        /// <returns>Returns back a array of supported scheme's</returns>
        public override IEnumerable<Uri> ServerUri()
        {
            var steamBuilder = new UriBuilder
            {
                Scheme = "steam",
                Host = SteamClient.SteamId.ToString()
            };

            return new[] {steamBuilder.Uri};
        }

        /// <summary>
        ///     Type of connection scheme transport supports.
        /// </summary>

        public override IEnumerable<string> Scheme => new[] {"steam"};

        /// <summary>
        ///     Does this transport support this specific platform.
        /// </summary>
        public override bool Supported => true;

        #endregion
    }
}
