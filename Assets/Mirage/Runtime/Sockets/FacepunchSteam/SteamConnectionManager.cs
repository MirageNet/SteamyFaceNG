using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Steamworks;
using Steamworks.Data;
using UnityEngine;

namespace Mirage.Sockets.FacepunchSteam {
    public class SteamConnectionManager : ConnectionManager, ISteamManager {
        protected readonly Queue<SteamMessage> messageQueue = new Queue<SteamMessage>();

        public bool Poll() {
            Receive();
            return messageQueue.Count > 0;
        }

        public SteamMessage GetNextMessage() {
            return messageQueue.Dequeue();
        }

        public unsafe void Send(byte[] data, int length, SteamEndPoint endPoint) {
            //Debug.Log("Sending bytes: " + data[0] + "-" + data[1] + "-" + data[2]);

            fixed (byte* aPtr = data)
            {
                IntPtr ptr = (IntPtr)aPtr;
                Result res = Connection.SendMessage(ptr, length, SendType.Reliable);
                if (res != Result.OK)
                {
                    Debug.Log($"Message Send Failed: {res}");
                }
            }
        }

        public void Dispose() {
            Close();
        }

        public override void OnConnected(ConnectionInfo info) {
            base.OnConnected(info);
            Debug.Log("Client OnConnected");
        }

        public override void OnConnectionChanged(ConnectionInfo info) {
            base.OnConnectionChanged(info);
            Debug.Log("Client connection state: "+ info.State);
        }

        public override void OnMessage(IntPtr data, int size, Int64 messageNum, Int64 recvTime, int channel) {
            base.OnMessage(data, size, messageNum, recvTime, channel);

            Debug.Log("Client got a message");
            
            byte[] mIn = new byte[size];
            Marshal.Copy(data, mIn, 0, size);

            messageQueue.Enqueue(new SteamMessage {
                address = ConnectionInfo.Identity.SteamId,
                data = mIn
            });
        }
    }
}