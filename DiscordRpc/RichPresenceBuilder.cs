using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordRpc
{
    public class RichPresenceBuilder
    {
        //TODO: add parameter length validation

        private RichPresenceInfo _presence;

        public RichPresenceBuilder WithTimeInfo(DateTime? start, DateTime? stop)
        {
            _presence.StartTimestamp = start?.ToUnixTime() ?? 0;
            _presence.EndTimestamp = stop?.ToUnixTime() ?? 0;

            return this;
        }

        public RichPresenceBuilder WithState(string state, string details = null)
        {
            _presence.State = state;
            _presence.Details = details;

            return this;
        }

        public RichPresenceBuilder WithSpectate(string spectateSecret)
        {
            _presence.SpectateSecret = spectateSecret;

            return this;
        }

        public RichPresenceBuilder WithNotification(string matchSecret, bool instance)
        {
            _presence.Instance = instance;
            _presence.MatchSecret = matchSecret;

            return this;
        }

        public RichPresenceBuilder WithArtwork(string largeImageKey, string largeImageText, string smallImageKey, string smallImageText)
        {
            _presence.LargeImageKey = largeImageKey;
            _presence.LargeImageText = largeImageText;
            _presence.SmallImageKey = smallImageKey;
            _presence.SmallImageText = smallImageText;

            return this;
        }

        public RichPresenceBuilder WithPartyInfo(int partySize, int partyMax = 0, string partyId = null, string joinSecret = null)
        {
            _presence.PartySize = partySize;
            _presence.PartyMax = partyMax;
            _presence.PartyId = partyId;
            _presence.JoinSecret = joinSecret;

            return this;
        }

        public RichPresenceInfo Build() => _presence;
    }
}
