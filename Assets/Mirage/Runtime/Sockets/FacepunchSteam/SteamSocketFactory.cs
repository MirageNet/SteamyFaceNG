using System.Net;
using Mirage.SocketLayer;
using Steamworks;

namespace Mirage.Sockets.FacepunchSteam {
    public class SteamSocketFactory : SocketFactory {
        public uint appID = 480;

        void Awake() {
            SteamClient.Init(appID, false);
            GetComponent<NetworkClient>().PeerConfig = new Config {
                ConnectAttemptInterval = 1f,
                MaxConnectAttempts = 20,
                TimeoutDuration = 30
            };
        }

        public override ISocket CreateClientSocket() {
            return new SteamSocket(this);
        }

        public override ISocket CreateServerSocket() {
            return new SteamSocket(this);
        }

        public override EndPoint GetBindEndPoint() {
            return new SteamEndPoint { address = SteamClient.SteamId };
        }

        public override EndPoint GetConnectEndPoint(string address = null, ushort? port = null) {
            SteamId steamId = ulong.Parse(address);

            return new SteamEndPoint { address = steamId };
        }

        void OnDestroy() {
            SteamClient.Shutdown();
        }
    }
}