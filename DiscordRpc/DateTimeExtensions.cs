using System;

namespace DiscordRpc {
    internal static class DateTimeExtensions
    {
        public static DateTime FromUnixTime(this long unixTime)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            //return epoch.AddSeconds(unixTime);
            var timeSpan = TimeSpan.FromSeconds(unixTime);
            return epoch.Add(timeSpan).ToLocalTime();
        }

        public static long ToUnixTime(this DateTime date)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            //return Convert.ToInt64((date - epoch).TotalSeconds);
            return (long)(date.ToUniversalTime() - epoch).TotalSeconds;
        }
    }
}