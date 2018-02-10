using System.Runtime.InteropServices;

namespace DiscordRpc
{
    public struct RichPresenceInfo
    {
        [MarshalAs(UnmanagedType.LPStr)]
        public string State;

        [MarshalAs(UnmanagedType.LPStr)]
        public string Details;

        [MarshalAs(UnmanagedType.I8)]
        public long StartTimestamp;

        [MarshalAs(UnmanagedType.I8)]
        public long EndTimestamp;

        [MarshalAs(UnmanagedType.LPStr)]
        public string LargeImageKey;

        [MarshalAs(UnmanagedType.LPStr)]
        public string LargeImageText;

        [MarshalAs(UnmanagedType.LPStr)]
        public string SmallImageKey;

        [MarshalAs(UnmanagedType.LPStr)]
        public string SmallImageText;

        [MarshalAs(UnmanagedType.LPStr)]
        public string PartyId;

        [MarshalAs(UnmanagedType.I4)]
        public int PartySize;

        [MarshalAs(UnmanagedType.I4)]
        public int PartyMax;

        [MarshalAs(UnmanagedType.LPStr)]
        public string MatchSecret;

        [MarshalAs(UnmanagedType.LPStr)]
        public string JoinSecret;

        [MarshalAs(UnmanagedType.LPStr)]
        public string SpectateSecret;

        [MarshalAs(UnmanagedType.I1)]
        public bool Instance;
    }
}