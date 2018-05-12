using System;

namespace TheGodfather.Common
{
    public sealed class PartialGuildConfig
    {
        public string Prefix { get; set; }
        public bool SuggestionsEnabled { get; set; }


        public static PartialGuildConfig Default
        {
            get {
                return new PartialGuildConfig {
                    Prefix = null,
                    SuggestionsEnabled = false
                };
            }
        }
    }
}
