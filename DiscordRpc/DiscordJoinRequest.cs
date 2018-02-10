using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace DiscordRpc
{
    [SuppressMessage("ReSharper", "UnassignedReadonlyField")]
    public struct DiscordJoinRequest
    {
        [MarshalAs(UnmanagedType.LPStr, SizeConst = 24)]
        public readonly string UserId;

        [MarshalAs(UnmanagedType.LPStr, SizeConst = 344)]
        public readonly string Username;

        [MarshalAs(UnmanagedType.LPStr, SizeConst = 8)]
        public readonly string Discriminator;

        [MarshalAs(UnmanagedType.LPStr, SizeConst = 128)]
        public readonly string Avatar;
    }
}