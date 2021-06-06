using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Mirage.Logging;
using Steamworks;
using Steamworks.Data;
using UnityEngine;

namespace Mirage.Sockets.FacepunchSteam {
    public class SteamSocketManager : SocketManager, ISteamManager {
        static readonly ILogger logger = LogFactory.GetLogger(typeof(SteamSocketManager));

        protected readonly Queue<SteamMessage> messageQueue = new Queue<SteamMessage>();
        protected readonly Dictionary<SteamId, Connection> connections = new Dictionary<SteamId, Connection>();

        public bool Poll() {
            SteamServer.RunCallbacks();
            Receive();
            
            return messageQueue.Count > 0;
        }

        public SteamMessage GetNextMessage() {
            return messageQueue.Dequeue();
        }

        public unsafe void Send(byte[] data, int length, SteamEndPoint endPoint) {
            fixed (byte* aPtr = data)
            {
                IntPtr ptr = (IntPtr)aPtr;
                Result res = connections[endPoint.address].SendMessage(ptr, length, SendType.Unreliable);
                if (res != Result.OK) {
                    if (logger.WarnEnabled()) Debug.LogWarning($"Steam Server message send failed: {res}");
                } else {
                    if (logger.LogEnabled()) Debug.Log($"Steam server sent {length} bytes to {endPoint.address}");
                }
            }
        }

        public void Dispose() {
            foreach (Connection connection in Connected) {
                connection.Close();
            }

            connections.Clear();
            Close();
            SteamServer.Shutdown();
        }

        public override void OnConnecting(Connection connection, ConnectionInfo data) {
            base.OnConnecting(connection, data);
            connection.Accept();
            if (logger.LogEnabled()) Debug.Log($"Steam Server: {data.Identity.SteamId} is connecting");
        }

        public override void OnConnected(Connection connection, ConnectionInfo data) {
            base.OnConnected(connection, data);
            connections.Add(data.Identity.SteamId, connection);
            if (logger.LogEnabled()) Debug.Log($"Steam Server: {data.Identity.SteamId} is connected");
        }

        public override void OnDisconnected(Connection connection, ConnectionInfo data) {
            base.OnDisconnected(connection, data);
            connections.Remove(data.Identity.SteamId);
            if (logger.LogEnabled()) Debug.Log($"Steam Server: {data.Identity.SteamId} disconnected");
        }

        public override void OnMessage(Connection connection, NetIdentity identity, IntPtr data, int size, Int64 messageNum, Int64 recvTime, int channel) {
            base.OnMessage(connection, identity, data, size, messageNum, recvTime, channel);

            if (logger.LogEnabled()) Debug.Log($"Steam Server received {size} bytes from {identity.SteamId}");

            byte[] mIn = new byte[size];
            Marshal.Copy(data, mIn, 0, size);

            messageQueue.Enqueue(new SteamMessage {
                address = identity.SteamId,
                data = mIn
            });
        }
    }
}