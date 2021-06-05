using System;
using System.Net;
using Mirage.SocketLayer;
using Steamworks;
using UnityEngine;

namespace Mirage.Sockets.FacepunchSteam {
    public class SteamSocket : ISocket {
        ISteamManager manager;
        SteamEndPoint tmpEndPoint = new SteamEndPoint();

        public void Bind(EndPoint endPoint) {

            SteamServer.Init(480, new SteamServerInit("Game", "Default Game")
            {
                DedicatedServer = false,
                GamePort = 27015,
                IpAddress = IPAddress.Any,
                QueryPort = 27016,
                Secure = true,
                SteamPort = 27017,
                VersionString = Application.version
            }, false);
            manager = SteamNetworkingSockets.CreateRelaySocket<SteamSocketManager>();
            Debug.Log("Steam bound");
        }

        public void Close() {
            manager.Dispose();
        }

        public void Connect(EndPoint endPoint) {
            SteamEndPoint steamEndPoint = (SteamEndPoint)endPoint;
            manager = SteamNetworkingSockets.ConnectRelay<SteamConnectionManager>(steamEndPoint.address);
            Debug.Log("Connected to " + steamEndPoint.address);
        }

        public bool Poll() {
            SteamClient.RunCallbacks();
            return manager.Poll();
        }

        public int Receive(byte[] buffer, out EndPoint endPoint) {
            Debug.Log("Receive");
            SteamMessage message = manager.GetNextMessage();

            Buffer.BlockCopy(message.data, 0, buffer, 0, message.data.Length);
            tmpEndPoint.address = message.address;
            endPoint = tmpEndPoint;

            return message.data.Length;
        }

        public void Send(EndPoint endPoint, byte[] packet, int length) {
            tmpEndPoint = (SteamEndPoint)endPoint;
            manager.Send(packet, length, tmpEndPoint);
        }
    }
}