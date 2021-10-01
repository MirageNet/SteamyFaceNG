using System;
using Mirage.SocketLayer;
using Steamworks;

namespace Mirage.Sockets.FacepunchSteam {
    public class SteamEndPoint : IEndPoint, IEquatable<SteamEndPoint> {
        public SteamId Address;

        public SteamEndPoint() {}

        public SteamEndPoint(SteamId address) {
            Address = address;
        }

        public IEndPoint CreateCopy() {
            return new SteamEndPoint(Address);
        }

        public override string ToString() {
            return Address.ToString();
        }

        public override bool Equals(object obj) {
            if (obj is SteamEndPoint endPoint) {
                return Address == endPoint.Address;
            }

            return false;
        }

        public override int GetHashCode() {
            return Address.GetHashCode();
        }

        public bool Equals(SteamEndPoint other) {
            if (other == null) return false;
            
            return Address == other.Address;
        }
    }
}