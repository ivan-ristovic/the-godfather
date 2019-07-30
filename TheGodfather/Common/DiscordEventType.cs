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
            switch (type) {
                // Channels
                case DiscordEventType.ChannelCreated:
                case DiscordEventType.ChannelDeleted:
                case DiscordEventType.ChannelPinsUpdated:
                case DiscordEventType.ChannelUpdated:
                    return DiscordColor.Aquamarine;

                // Errors and serious events
                case DiscordEventType.ClientErrored:
                case DiscordEventType.CommandErrored:
                case DiscordEventType.GuildBanAdded:
                case DiscordEventType.GuildDeleted:
                case DiscordEventType.GuildUnavailable:
                    return DiscordColor.Red;

                // Availability
                case DiscordEventType.DmChannelCreated:
                case DiscordEventType.DmChannelDeleted:
                case DiscordEventType.GuildAvailable:
                case DiscordEventType.GuildCreated:
                case DiscordEventType.GuildBanRemoved:
                    return DiscordColor.Green;

                // Successful execution
                case DiscordEventType.CommandExecuted:
                case DiscordEventType.GuildDownloadCompleted:
                    return DiscordColor.SpringGreen;

                // Emojis
                case DiscordEventType.GuildEmojisUpdated:
                    return DiscordColor.Yellow;

                // Members and Users
                case DiscordEventType.GuildMemberUpdated:
                case DiscordEventType.PresenceUpdated:
                case DiscordEventType.TypingStarted:
                case DiscordEventType.UserSettingsUpdated:
                case DiscordEventType.UserUpdated:
                    return DiscordColor.DarkGreen;
                case DiscordEventType.GuildMemberAdded:
                case DiscordEventType.GuildMembersChunked:
                    return DiscordColor.Turquoise;
                case DiscordEventType.GuildMemberRemoved:
                    return DiscordColor.DarkRed;

                // Roles
                case DiscordEventType.GuildRoleCreated:
                case DiscordEventType.GuildRoleDeleted:
                case DiscordEventType.GuildRoleUpdated:
                    return DiscordColor.Orange;

                // Guild 
                case DiscordEventType.GuildIntegrationsUpdated:
                case DiscordEventType.GuildUpdated:
                case DiscordEventType.VoiceServerUpdated:
                case DiscordEventType.VoiceStateUpdated:
                case DiscordEventType.WebhooksUpdated:
                    return DiscordColor.SapGreen;

                // Messages
                case DiscordEventType.MessageAcknowledged:
                case DiscordEventType.MessageCreated:
                case DiscordEventType.MessageDeleted:
                case DiscordEventType.MessageReactionAdded:
                case DiscordEventType.MessageReactionRemoved:
                case DiscordEventType.MessageReactionsCleared:
                case DiscordEventType.MessageUpdated:
                case DiscordEventType.MessagesBulkDeleted:
                    return DiscordColor.CornflowerBlue;

                // Application
                case DiscordEventType.Heartbeated:
                case DiscordEventType.Ready:
                case DiscordEventType.Resumed:
                case DiscordEventType.SocketClosed:
                case DiscordEventType.SocketErrored:
                case DiscordEventType.SocketOpened:
                    return DiscordColor.White;

                case DiscordEventType.UnknownEvent:
                default:
                    return DiscordColor.Black;
            }
        }
    }
}