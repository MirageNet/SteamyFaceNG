using System;
using Mirage.SocketLayer;
using Steamworks;

namespace Mirage.Sockets.FacepunchSteam {
    public struct SteamEndPoint : IEndPoint, IEquatable<SteamEndPoint> {
        public SteamId address;

        public SteamEndPoint(SteamId address) {
            this.address = address;
        }

        public IEndPoint CreateCopy() {
            return new SteamEndPoint(address);
        }

        public override string ToString() {
            return address.ToString();
        }

        public override bool Equals(object obj) {
            if (obj is SteamEndPoint endPoint) {
                return address == endPoint.address;
            }

            return false;
        }

        public override int GetHashCode() {
            return address.GetHashCode();
        }

        public bool Equals(SteamEndPoint other) {
            return address == other.address;
        }
    }
}