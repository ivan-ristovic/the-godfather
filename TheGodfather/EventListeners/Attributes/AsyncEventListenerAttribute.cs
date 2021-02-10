using System;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using TheGodfather.EventListeners.Common;
using TheGodfather.Services;

namespace TheGodfather.EventListeners.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class AsyncEventListenerAttribute : Attribute
    {
        public DiscordEventType EventType { get; }


        public AsyncEventListenerAttribute(DiscordEventType eventType)
        {
            this.EventType = eventType;
        }


        public void Register(TheGodfatherBot bot, MethodInfo mi)
        {
            BotActivityService bas = bot.Services.GetRequiredService<BotActivityService>();


            Task OnEventWithArgs(object _, object e)
            {
                if (!bas.IsBotListening)
                    return Task.CompletedTask;

                _ = Task.Run(async () => {
                    try {
                        await (Task)mi.Invoke(null, new object[] { bot, e })!;
                    } catch (Exception ex) {
                        Log.Error(ex, "Listener threw an exception");
                    }
                });

                return Task.CompletedTask;
            }


            #region Event hooking
            switch (this.EventType) {
                case DiscordEventType.ChannelCreated:
                    bot.Client.ChannelCreated += OnEventWithArgs;
                    break;
                case DiscordEventType.ChannelDeleted:
                    bot.Client.ChannelDeleted += OnEventWithArgs;
                    break;
                case DiscordEventType.ChannelPinsUpdated:
                    bot.Client.ChannelPinsUpdated += OnEventWithArgs;
                    break;
                case DiscordEventType.ChannelUpdated:
                    bot.Client.ChannelUpdated += OnEventWithArgs;
                    break;
                case DiscordEventType.ClientErrored:
                    bot.Client.ClientErrored += OnEventWithArgs;
                    break;
                case DiscordEventType.CommandErrored:
                    foreach (CommandsNextExtension cnext in bot.CNext.Values)
                        cnext.CommandErrored += OnEventWithArgs;
                    break;
                case DiscordEventType.CommandExecuted:
                    foreach (CommandsNextExtension cnext in bot.CNext.Values)
                        cnext.CommandExecuted += OnEventWithArgs;
                    break;
                case DiscordEventType.DmChannelDeleted:
                    bot.Client.DmChannelDeleted += OnEventWithArgs;
                    break;
                case DiscordEventType.GuildAvailable:
                    bot.Client.GuildAvailable += OnEventWithArgs;
                    break;
                case DiscordEventType.GuildBanAdded:
                    bot.Client.GuildBanAdded += OnEventWithArgs;
                    break;
                case DiscordEventType.GuildBanRemoved:
                    bot.Client.GuildBanRemoved += OnEventWithArgs;
                    break;
                case DiscordEventType.GuildCreated:
                    bot.Client.GuildCreated += OnEventWithArgs;
                    break;
                case DiscordEventType.GuildDeleted:
                    bot.Client.GuildDeleted += OnEventWithArgs;
                    break;
                case DiscordEventType.GuildDownloadCompleted:
                    bot.Client.GuildDownloadCompleted += OnEventWithArgs;
                    break;
                case DiscordEventType.GuildEmojisUpdated:
                    bot.Client.GuildEmojisUpdated += OnEventWithArgs;
                    break;
                case DiscordEventType.GuildIntegrationsUpdated:
                    bot.Client.GuildIntegrationsUpdated += OnEventWithArgs;
                    break;
                case DiscordEventType.GuildMemberAdded:
                    bot.Client.GuildMemberAdded += OnEventWithArgs;
                    break;
                case DiscordEventType.GuildMemberRemoved:
                    bot.Client.GuildMemberRemoved += OnEventWithArgs;
                    break;
                case DiscordEventType.GuildMemberUpdated:
                    bot.Client.GuildMemberUpdated += OnEventWithArgs;
                    break;
                case DiscordEventType.GuildMembersChunked:
                    bot.Client.GuildMembersChunked += OnEventWithArgs;
                    break;
                case DiscordEventType.GuildRoleCreated:
                    bot.Client.GuildRoleCreated += OnEventWithArgs;
                    break;
                case DiscordEventType.GuildRoleUpdated:
                    bot.Client.GuildRoleUpdated += OnEventWithArgs;
                    break;
                case DiscordEventType.GuildRoleDeleted:
                    bot.Client.GuildRoleDeleted += OnEventWithArgs;
                    break;
                case DiscordEventType.GuildUnavailable:
                    bot.Client.GuildUnavailable += OnEventWithArgs;
                    break;
                case DiscordEventType.GuildUpdated:
                    bot.Client.GuildUpdated += OnEventWithArgs;
                    break;
                case DiscordEventType.Heartbeated:
                    bot.Client.Heartbeated += OnEventWithArgs;
                    break;
                case DiscordEventType.InviteCreated:
                    bot.Client.InviteCreated += OnEventWithArgs;
                    break;
                case DiscordEventType.InviteDeleted:
                    bot.Client.InviteDeleted += OnEventWithArgs;
                    break;
                case DiscordEventType.MessageAcknowledged:
                    foreach (DiscordClient client in bot.Client.ShardClients.Values)
                        client.MessageAcknowledged += OnEventWithArgs;
                    break;
                case DiscordEventType.MessagesBulkDeleted:
                    bot.Client.MessagesBulkDeleted += OnEventWithArgs;
                    break;
                case DiscordEventType.MessageCreated:
                    bot.Client.MessageCreated += OnEventWithArgs;
                    break;
                case DiscordEventType.MessageReactionAdded:
                    bot.Client.MessageReactionAdded += OnEventWithArgs;
                    break;
                case DiscordEventType.MessageReactionRemoved:
                    bot.Client.MessageReactionRemoved += OnEventWithArgs;
                    break;
                case DiscordEventType.MessageReactionRemovedEmoji:
                    bot.Client.MessageReactionRemovedEmoji += OnEventWithArgs;
                    break;
                case DiscordEventType.MessageReactionsCleared:
                    bot.Client.MessageReactionsCleared += OnEventWithArgs;
                    break;
                case DiscordEventType.MessageDeleted:
                    bot.Client.MessageDeleted += OnEventWithArgs;
                    break;
                case DiscordEventType.MessageUpdated:
                    bot.Client.MessageUpdated += OnEventWithArgs;
                    break;
                case DiscordEventType.PresenceUpdated:
                    bot.Client.PresenceUpdated += OnEventWithArgs;
                    break;
                case DiscordEventType.Ready:
                    bot.Client.Ready += OnEventWithArgs;
                    break;
                case DiscordEventType.Resumed:
                    bot.Client.Resumed += OnEventWithArgs;
                    break;
                case DiscordEventType.SocketClosed:
                    bot.Client.SocketClosed += OnEventWithArgs;
                    break;
                case DiscordEventType.SocketErrored:
                    bot.Client.SocketErrored += OnEventWithArgs;
                    break;
                case DiscordEventType.SocketOpened:
                    bot.Client.SocketOpened += OnEventWithArgs;
                    break;
                case DiscordEventType.TypingStarted:
                    bot.Client.TypingStarted += OnEventWithArgs;
                    break;
                case DiscordEventType.UnknownEvent:
                    bot.Client.UnknownEvent += OnEventWithArgs;
                    break;
                case DiscordEventType.UserSettingsUpdated:
                    bot.Client.UserSettingsUpdated += OnEventWithArgs;
                    break;
                case DiscordEventType.UserUpdated:
                    bot.Client.UserUpdated += OnEventWithArgs;
                    break;
                case DiscordEventType.VoiceServerUpdated:
                    bot.Client.VoiceServerUpdated += OnEventWithArgs;
                    break;
                case DiscordEventType.VoiceStateUpdated:
                    bot.Client.VoiceStateUpdated += OnEventWithArgs;
                    break;
                case DiscordEventType.WebhooksUpdated:
                    bot.Client.WebhooksUpdated += OnEventWithArgs;
                    break;
                default:
                    Log.Warning("No logic for handling event type: {EventType}", Enum.GetName(typeof(DiscordEventType), this.EventType));
                    break;
            }
            #endregion
        }
    }
}