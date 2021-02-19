#region Statements

using Steamworks;

#endregion

namespace Mirage.FizzySteam
{
    public struct SteamOptions
    {
        public bool AllowSteamRelay;
        public int MaxConnections;
        public int ConnectionTimeOut;
        public SteamId ConnectionAddress;
        public P2PSend[] Channels;
    }
}