#region Statements

using Steamworks;

#endregion

namespace Mirror.FizzySteam
{
    public struct Message
    {
        public readonly SteamId steamId;
        public readonly InternalMessages eventType;
        public readonly byte[] data;
        public int Channel;

        public Message(SteamId steamId, InternalMessages eventType, byte[] data, int channel)
        {
            this.steamId = steamId;
            this.eventType = eventType;
            this.data = data;
            this.Channel = channel;
        }
    }
}