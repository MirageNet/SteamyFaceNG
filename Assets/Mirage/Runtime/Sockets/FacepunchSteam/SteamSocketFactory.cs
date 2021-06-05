using System.Net;
using Mirage.SocketLayer;
using Steamworks;
using UnityEngine;
using Mirage;

namespace Mirage.Sockets.FacepunchSteam {
    public class SteamSocketFactory : SocketFactory {
        void Awake() {
            SteamClient.Init(480, false);
            GetComponent<NetworkClient>().PeerConfig = new Config {
                ConnectAttemptInterval = 1f,
                MaxConnectAttempts = 20,
                TimeoutDuration = 30
            };
        }

        void Start() {
            Debug.Log("Your steamId is: " + SteamClient.SteamId);
        }

        public override ISocket CreateClientSocket() {
            return new SteamSocket();
        }

        public override ISocket CreateServerSocket() {
            return new SteamSocket();
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