using DSharpPlus.Entities;

namespace TheGodfather.EventListeners.Common;

public enum DiscordEventType
{
    #region Event types
    ApplicationCommandCreated,
    ApplicationCommandDeleted,
    ApplicationCommandUpdated,
    ChannelCreated,
    ChannelDeleted,
    ChannelPinsUpdated,
    ChannelUpdated,
    ClientErrored,
    CommandErrored,
    CommandExecuted,
    ComponentInteractionCreated,
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
    GuildStickersUpdated,
    GuildUnavailable,
    GuildUpdated,
    Heartbeated,
    InviteCreated,
    InviteDeleted,
    MessageAcknowledged,
    MessageCreated,
    MessageDeleted,
    MessageReactionAdded,
    MessageReactionRemoved,
    MessageReactionRemovedEmoji,
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
    WebhooksUpdated,
    #endregion
}


public static class DiscordEventTypeExtensions
{
    public static DiscordColor ToDiscordColor(this DiscordEventType type)
    {
        switch (type) {
            #region Application commands
            case DiscordEventType.ApplicationCommandCreated:
            case DiscordEventType.ApplicationCommandDeleted:
            case DiscordEventType.ApplicationCommandUpdated:
            case DiscordEventType.ComponentInteractionCreated:
                return DiscordColor.Sienna;
            #endregion

            #region Channels
            case DiscordEventType.ChannelCreated:
            case DiscordEventType.ChannelDeleted:
            case DiscordEventType.ChannelPinsUpdated:
            case DiscordEventType.ChannelUpdated:
                return DiscordColor.Aquamarine;
            #endregion

            #region Errors and serious events
            case DiscordEventType.ClientErrored:
            case DiscordEventType.CommandErrored:
            case DiscordEventType.GuildBanAdded:
            case DiscordEventType.GuildDeleted:
            case DiscordEventType.GuildUnavailable:
                return DiscordColor.Red;
            #endregion

            #region Availability
            case DiscordEventType.DmChannelDeleted:
            case DiscordEventType.GuildAvailable:
            case DiscordEventType.GuildCreated:
            case DiscordEventType.GuildBanRemoved:
                return DiscordColor.Green;
            #endregion

            #region Successful execution
            case DiscordEventType.CommandExecuted:
            case DiscordEventType.GuildDownloadCompleted:
                return DiscordColor.SpringGreen;
            #endregion

            #region Emojis & Stickers
            case DiscordEventType.GuildEmojisUpdated:
            case DiscordEventType.GuildStickersUpdated:
                return DiscordColor.Yellow;
            #endregion

            #region Members and Users
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
            #endregion

            #region Roles
            case DiscordEventType.GuildRoleCreated:
            case DiscordEventType.GuildRoleDeleted:
            case DiscordEventType.GuildRoleUpdated:
                return DiscordColor.Orange;
            #endregion

            #region Guild
            case DiscordEventType.GuildIntegrationsUpdated:
            case DiscordEventType.GuildUpdated:
            case DiscordEventType.InviteCreated:
            case DiscordEventType.InviteDeleted:
            case DiscordEventType.VoiceServerUpdated:
            case DiscordEventType.VoiceStateUpdated:
            case DiscordEventType.WebhooksUpdated:
                return DiscordColor.SapGreen;
            #endregion

            #region Messages
            case DiscordEventType.MessageAcknowledged:
            case DiscordEventType.MessageCreated:
            case DiscordEventType.MessageDeleted:
            case DiscordEventType.MessageReactionAdded:
            case DiscordEventType.MessageReactionRemoved:
            case DiscordEventType.MessageReactionRemovedEmoji:
            case DiscordEventType.MessageReactionsCleared:
            case DiscordEventType.MessageUpdated:
            case DiscordEventType.MessagesBulkDeleted:
                return DiscordColor.CornflowerBlue;
            #endregion

            #region Application
            case DiscordEventType.Heartbeated:
            case DiscordEventType.Ready:
            case DiscordEventType.Resumed:
            case DiscordEventType.SocketClosed:
            case DiscordEventType.SocketErrored:
            case DiscordEventType.SocketOpened:
                return DiscordColor.White;
            #endregion

            #region Unknown and default
            case DiscordEventType.UnknownEvent:
            default:
                return DiscordColor.Black;
            #endregion
        }
    }
}