using Mirage.SocketLayer;
using Steamworks;
using UnityEngine;

namespace Mirage.Sockets.FacepunchSteam {
    public class SteamSocketFactory : SocketFactory {
        public uint AppID = 480;

        [Tooltip("Set this to false if you want to initialize the SteamClient yourself.")]
        public bool InitSteam = true;

        [Tooltip("Set this to false if you want to run Steam callbacks yourself.")]
        public bool RunCallbacks = true;

        public override int MaxPacketSize => 1200;

        void Awake() {
            GetComponent<NetworkClient>().PeerConfig = new Config {
                ConnectAttemptInterval = 1f,
                MaxConnectAttempts = 20,
                TimeoutDuration = 30
            };
        }

        void Start() {
            if (InitSteam) SteamClient.Init(AppID, false);
        }

        public override ISocket CreateClientSocket() {
            return new SteamSocket(this);
        }

        public override ISocket CreateServerSocket() {
            return new SteamSocket(this);
        }

        public override IEndPoint GetBindEndPoint() {
            return new SteamEndPoint(SteamClient.SteamId);
        }

        public override IEndPoint GetConnectEndPoint(string address = null, ushort? port = null) {
            SteamId steamId = ulong.Parse(address);

            return new SteamEndPoint(steamId);
        }

        void OnDestroy() {
            if (InitSteam) {
                SteamClient.Shutdown();
            }
        }
    }
}