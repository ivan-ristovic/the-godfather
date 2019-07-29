using DSharpPlus.Entities;

namespace TheGodfather.Common
{
    public enum DiscordEventType
    {
        ChannelCreated,
        ChannelDeleted,
        ChannelPinsUpdated,
        ChannelUpdated,
        ClientErrored,
        CommandErrored,
        CommandExecuted,
        DmChannelCreated,
        DmChannelDeleted,
        GuildAvailable,
        GuildBanAdded,
        GuildBanRemoved,
        GuildCreated,
        GuildDeleted,
        GuildDownloadCompleted,
        GuildEmojisUpdated,
        GuildIntegrationsUpdated,
        GuildMemberAdded,
        GuildMemberRemoved,
        GuildMemberUpdated,
        GuildMembersChunked,
        GuildRoleCreated,
        GuildRoleDeleted,
        GuildRoleUpdated,
        GuildUnavailable,
        GuildUpdated,
        Heartbeated,
        MessageAcknowledged,
        MessageCreated,
        MessageDeleted,
        MessageReactionAdded,
        MessageReactionRemoved,
        MessageReactionsCleared,
        MessageUpdated,
        MessagesBulkDeleted,
        PresenceUpdated,
        Ready,
        Resumed,
        SocketClosed,
        SocketErrored,
        SocketOpened,
        TypingStarted,
        UnknownEvent,
        UserSettingsUpdated,
        UserUpdated,
        VoiceServerUpdated,
        VoiceStateUpdated,
        WebhooksUpdated
    }


    public static class DiscordEventTypeExtensions
    {
        public static DiscordColor ToDiscordColor(this DiscordEventType type)
        {
            // TODO
            switch (type) {
                case DiscordEventType.ChannelCreated:
                case DiscordEventType.ChannelDeleted:
                case DiscordEventType.ChannelPinsUpdated:
                case DiscordEventType.ChannelUpdated:
                case DiscordEventType.ClientErrored:
                case DiscordEventType.CommandErrored:
                case DiscordEventType.CommandExecuted:
                case DiscordEventType.DmChannelCreated:
                case DiscordEventType.DmChannelDeleted:
                case DiscordEventType.GuildAvailable:
                case DiscordEventType.GuildBanAdded:
                case DiscordEventType.GuildBanRemoved:
                case DiscordEventType.GuildCreated:
                case DiscordEventType.GuildDeleted:
                case DiscordEventType.GuildDownloadCompleted:
                case DiscordEventType.GuildEmojisUpdated:
                case DiscordEventType.GuildIntegrationsUpdated:
                case DiscordEventType.GuildMemberAdded:
                case DiscordEventType.GuildMemberRemoved:
                case DiscordEventType.GuildMemberUpdated:
                case DiscordEventType.GuildMembersChunked:
                case DiscordEventType.GuildRoleCreated:
                case DiscordEventType.GuildRoleDeleted:
                case DiscordEventType.GuildRoleUpdated:
                case DiscordEventType.GuildUnavailable:
                case DiscordEventType.GuildUpdated:
                case DiscordEventType.Heartbeated:
                case DiscordEventType.MessageAcknowledged:
                case DiscordEventType.MessageCreated:
                case DiscordEventType.MessageDeleted:
                case DiscordEventType.MessageReactionAdded:
                case DiscordEventType.MessageReactionRemoved:
                case DiscordEventType.MessageReactionsCleared:
                case DiscordEventType.MessageUpdated:
                case DiscordEventType.MessagesBulkDeleted:
                case DiscordEventType.PresenceUpdated:
                case DiscordEventType.Ready:
                case DiscordEventType.Resumed:
                case DiscordEventType.SocketClosed:
                case DiscordEventType.SocketErrored:
                case DiscordEventType.SocketOpened:
                case DiscordEventType.TypingStarted:
                case DiscordEventType.UnknownEvent:
                case DiscordEventType.UserSettingsUpdated:
                case DiscordEventType.UserUpdated:
                case DiscordEventType.VoiceServerUpdated:
                case DiscordEventType.VoiceStateUpdated:
                case DiscordEventType.WebhooksUpdated:
                default:
                    return DiscordColor.Aquamarine;
            }
        }
    }
}