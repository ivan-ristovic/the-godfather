using System;

namespace TheGodfather.Common
{
    public sealed class PartialGuildConfig
    {
        public bool LinkfilterEnabled { get; set; }
        public bool BlockInvites { get; set; }
        public ulong LogChannelId { get; set; }
        public bool LoggingEnabled => LogChannelId != 0;
        public string Prefix { get; set; }
        public bool SuggestionsEnabled { get; set; }


        public static PartialGuildConfig Default
        {
            get {
                return new PartialGuildConfig {
                    LinkfilterEnabled = false,
                    BlockInvites = false,
                    LogChannelId = 0,
                    Prefix = null,
                    SuggestionsEnabled = false
                };
            }
        }
    }
}
