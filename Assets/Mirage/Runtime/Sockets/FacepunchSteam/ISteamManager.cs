namespace Mirage.Sockets.FacepunchSteam {
    public interface ISteamManager {
        bool Poll();
        SteamMessage GetNextMessage();
        void Send(byte[] data, int length, SteamEndPoint endPoint);
        void Dispose();
    }
}