using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Steamworks;
using Steamworks.Data;
using UnityEngine;

namespace Mirage.Sockets.FacepunchSteam {
    public class SteamSocketManager : SocketManager, ISteamManager {
        protected readonly Queue<SteamMessage> messageQueue = new Queue<SteamMessage>();
        protected readonly Dictionary<SteamId, Connection> connections = new Dictionary<SteamId, Connection>();

        public bool Poll() {
            SteamServer.RunCallbacks();
            Receive();
            /*foreach (var conn in Connected) {
                Debug.Log(conn);
                Debug.Log(conn.QuickStatus());
            }*/
            return messageQueue.Count > 0;
        }

        public SteamMessage GetNextMessage() {
            return messageQueue.Dequeue();
        }

        public unsafe void Send(byte[] data, int length, SteamEndPoint endPoint) {
            Debug.Log("Sending " + length);

            fixed (byte* aPtr = data)
            {
                IntPtr ptr = (IntPtr)aPtr;
                Result res = connections[endPoint.address].SendMessage(ptr, length, SendType.Reliable);
                if (res != Result.OK)
                {
                    Debug.Log($"Message Send Failed: {res}");
                }
            }
        }

        public void Dispose() {
            Close();
            SteamServer.Shutdown();
        }

        public override void OnConnecting(Connection connection, ConnectionInfo data) {
            base.OnConnecting(connection, data);
            connection.Accept();
            Debug.Log($"{data.Identity.SteamId} is connecting");
        }

        public override void OnConnected(Connection connection, ConnectionInfo data) {
            base.OnConnected(connection, data);
            Debug.Log($"{data.Identity.SteamId} has joined the game");
            connections.Add(data.Identity.SteamId, connection);
        }

        public override void OnDisconnected(Connection connection, ConnectionInfo data) {
            base.OnDisconnected(connection, data);
            connections.Remove(data.Identity.SteamId);
            Debug.Log($"{data.Identity.SteamId} is out of here");
        }

        public override void OnMessage(Connection connection, NetIdentity identity, IntPtr data, int size, Int64 messageNum, Int64 recvTime, int channel) {
            base.OnMessage(connection, identity, data, size, messageNum, recvTime, channel);
            Debug.Log($"We got a message from {identity.SteamId}!");

            byte[] mIn = new byte[size];
            Marshal.Copy(data, mIn, 0, size);

            messageQueue.Enqueue(new SteamMessage {
                address = identity.SteamId,
                data = mIn
            });
        }
    }
}