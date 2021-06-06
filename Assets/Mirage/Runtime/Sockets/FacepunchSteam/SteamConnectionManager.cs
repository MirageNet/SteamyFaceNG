using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Mirage.Logging;
using Mirage.SocketLayer;
using Steamworks;
using Steamworks.Data;
using UnityEngine;

namespace Mirage.Sockets.FacepunchSteam {
    public class SteamConnectionManager : ConnectionManager, ISteamManager {
        static readonly ILogger logger = LogFactory.GetLogger(typeof(SteamConnectionManager));

        protected readonly Queue<SteamMessage> messageQueue = new Queue<SteamMessage>();

        public bool Poll() {
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
                Result res = Connection.SendMessage(ptr, length, SendType.Unreliable);
                if (res != Result.OK && logger.WarnEnabled()) {
                    Debug.LogWarning($"Steam Client send failed: {res}");
                } else if (logger.LogEnabled()) {
                    Debug.Log("Steam client sent " + length + " bytes");
                }
            }
        }

        public void Dispose() {
            Close();
        }

        public override void OnConnected(ConnectionInfo info) {
            base.OnConnected(info);
            
            if (logger.LogEnabled()) Debug.Log("Steam Client connected");
        }

        public override void OnConnectionChanged(ConnectionInfo info) {
            base.OnConnectionChanged(info);

            if (logger.LogEnabled()) Debug.Log($"Steam Client connection state changed to {info.State}");
        }

        public override void OnMessage(IntPtr data, int size, Int64 messageNum, Int64 recvTime, int channel) {
            base.OnMessage(data, size, messageNum, recvTime, channel);

            if (logger.LogEnabled()) Debug.Log($"Steam Client received {size} bytes");

            byte[] mIn = new byte[size];
            Marshal.Copy(data, mIn, 0, size);

            messageQueue.Enqueue(new SteamMessage {
                address = ConnectionInfo.Identity.SteamId,
                data = mIn
            });
        }

        public override void OnDisconnected(ConnectionInfo info) {
            base.OnDisconnected(info);

            if (logger.LogEnabled()) Debug.Log("Steam Client disconnected");

            messageQueue.Enqueue(new SteamMessage {
                data = CreateDisconnectPacket(),
                address = ConnectionInfo.Identity.SteamId
            });
        }

        byte[] CreateDisconnectPacket() {
            byte[] data = new byte[3];
            data[0] = (byte)PacketType.Command;
            data[1] = (byte)Commands.Disconnect;
            data[2] = (byte)DisconnectReason.RequestedByRemotePeer;

            return data;
        }
    }
}