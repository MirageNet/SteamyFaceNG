using System;
using System.Net;
using Mirage.SocketLayer;
using Steamworks;
using UnityEngine;

namespace Mirage.Sockets.FacepunchSteam {
    public class SteamSocket : ISocket {
        ISteamManager manager;
        SteamEndPoint tmpEndPoint = new SteamEndPoint();
        readonly SteamSocketFactory socketFactory;

        public SteamSocket(SteamSocketFactory factory) {
            socketFactory = factory;
        }

        public void Bind(IEndPoint endPoint) {
            /*SteamServer.Init(socketFactory.AppID, new SteamServerInit("Game", Application.productName) {
                DedicatedServer = false,
                GamePort = 27015,
                IpAddress = IPAddress.Any,
                QueryPort = 27016,
                Secure = true,
                SteamPort = 27017,
                VersionString = Application.version
            }, false);*/

            manager = SteamNetworkingSockets.CreateRelaySocket<SteamSocketManager>();
        }

        public void Close() {
            manager.Dispose();
        }

        public void Connect(IEndPoint endPoint) {
            SteamEndPoint steamEndPoint = (SteamEndPoint)endPoint;
            manager = SteamNetworkingSockets.ConnectRelay<SteamConnectionManager>(steamEndPoint.address);
        }

        public bool Poll() {
            if (socketFactory.RunCallbacks) SteamClient.RunCallbacks();

            return manager.Poll();
        }

        public int Receive(byte[] buffer, out IEndPoint endPoint) {
            SteamMessage message = manager.GetNextMessage();

            Buffer.BlockCopy(message.data, 0, buffer, 0, message.data.Length);
            tmpEndPoint.address = message.address;
            endPoint = tmpEndPoint;

            return message.data.Length;
        }

        public void Send(IEndPoint endPoint, byte[] packet, int length) {
            var steamEndPoint = (SteamEndPoint)endPoint;
            manager.Send(packet, length, steamEndPoint);
        }

        public static byte[] CreateDisconnectPacket() {
            byte[] data = new byte[3];
            data[0] = (byte)PacketType.Command;
            data[1] = (byte)Commands.Disconnect;
            data[2] = (byte)DisconnectReason.RequestedByRemotePeer;

            return data;
        }
    }
}