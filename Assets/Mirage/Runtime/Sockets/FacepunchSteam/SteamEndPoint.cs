using System.Net;
using Steamworks;

namespace Mirage.Sockets.FacepunchSteam {
    public class SteamEndPoint : EndPoint {
        public SteamId address;

        public override string ToString() {
            return address.ToString();
        }
    }
}