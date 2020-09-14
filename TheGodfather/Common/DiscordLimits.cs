namespace TheGodfather.Common
{
    public static class DiscordLimits
    {
        public static int CategoryChannelLimit { get; } = 50;
        public static int ChannelLimit { get; } = 500;
        public static int ChannelNameLimit { get; } = 100;
        public static int ChannelTopicLimit { get; } = 1024;
        public static int EmbedAuthorLimit { get; } = 256;
        public static int EmbedDescriptionLimit { get; } = 2048;
        public static int EmbedFieldLimit { get; } = 25;
        public static int EmbedFieldNameLimit { get; } = 256;
        public static int EmbedFieldValueLimit { get; } = 1024;
        public static int EmbedFooterLimit { get; } = 2048;
        public static int EmbedTitleLimit { get; } = 256;
        public static int EmbedTotalCharLimit { get; } = 6000;
        public static int EmojiNameLimit { get; } = 32;
        public static int EmojiSizeLimit { get; } = 256000;
        public static int MessageContentLimit { get; } = 2000;
        public static int MessageReactionLimit { get; } = 20;
        public static int NameLimit { get; } = 256;
        public static int RoleLimit { get; } = 250;
        public static int TtsMessageContentLimit { get; } = 200;
        public static int UsernameLimit { get; } = 32;
        public static int VoiceChannelUserLimit { get; } = 99;
        public static int VoiceChannelMinBitrate { get; } = 8;
        public static int VoiceChannelMaxBitrate { get; } = 128;
    }
}

