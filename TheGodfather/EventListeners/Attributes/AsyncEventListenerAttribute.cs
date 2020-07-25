using System;
using System.Reflection;
using System.Threading.Tasks;
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


        public void Register(TheGodfatherShard shard, MethodInfo mi)
        {
            if (shard.Client is null || shard.CNext is null)
                throw new ArgumentException("Shard not initialized");
            
            BotActivityService bas = shard.Services.GetService<BotActivityService>();


            Task OnEventWithArgs(object e)
            {
                if (!bas.IsBotListening)
                    return Task.CompletedTask;

                _ = Task.Run(async () => {
                    try {
                        await (Task)mi.Invoke(null, new object[] { shard, e })!;
                    } catch (Exception ex) {
                        Log.Error(ex, "Listener threw an exception");
                    }
                });
                
                return Task.CompletedTask;
            }

            Task OnEventVoid()
            {
                if (!bas.IsBotListening)
                    return Task.CompletedTask;

                _ = Task.Run(async () => {
                    try {
                        await (Task)mi.Invoke(null, new object[] { shard })!;
                    } catch (Exception ex) {
                        Log.Error(ex, "Listener threw an exception");
                    }
                });

                return Task.CompletedTask;
            }

            
            #region Event hooking
            switch (this.EventType) {
                case DiscordEventType.ChannelCreated:
                    shard.Client.ChannelCreated += OnEventWithArgs;
                    break;
                case DiscordEventType.ChannelDeleted:
                    shard.Client.ChannelDeleted += OnEventWithArgs;
                    break;
                case DiscordEventType.ChannelPinsUpdated:
                    shard.Client.ChannelPinsUpdated += OnEventWithArgs;
                    break;
                case DiscordEventType.ChannelUpdated:
                    shard.Client.ChannelUpdated += OnEventWithArgs;
                    break;
                case DiscordEventType.ClientErrored:
                    shard.Client.ClientErrored += OnEventWithArgs;
                    break;
                case DiscordEventType.CommandErrored:
                    shard.CNext.CommandErrored += OnEventWithArgs;
                    break;
                case DiscordEventType.CommandExecuted:
                    shard.CNext.CommandExecuted += OnEventWithArgs;
                    break;
                case DiscordEventType.DmChannelCreated:
                    shard.Client.DmChannelCreated += OnEventWithArgs;
                    break;
                case DiscordEventType.DmChannelDeleted:
                    shard.Client.DmChannelDeleted += OnEventWithArgs;
                    break;
                case DiscordEventType.GuildAvailable:
                    shard.Client.GuildAvailable += OnEventWithArgs;
                    break;
                case DiscordEventType.GuildBanAdded:
                    shard.Client.GuildBanAdded += OnEventWithArgs;
                    break;
                case DiscordEventType.GuildBanRemoved:
                    shard.Client.GuildBanRemoved += OnEventWithArgs;
                    break;
                case DiscordEventType.GuildCreated:
                    shard.Client.GuildCreated += OnEventWithArgs;
                    break;
                case DiscordEventType.GuildDeleted:
                    shard.Client.GuildDeleted += OnEventWithArgs;
                    break;
                case DiscordEventType.GuildDownloadCompleted:
                    shard.Client.GuildDownloadCompleted += OnEventWithArgs;
                    break;
                case DiscordEventType.GuildEmojisUpdated:
                    shard.Client.GuildEmojisUpdated += OnEventWithArgs;
                    break;
                case DiscordEventType.GuildIntegrationsUpdated:
                    shard.Client.GuildIntegrationsUpdated += OnEventWithArgs;
                    break;
                case DiscordEventType.GuildMemberAdded:
                    shard.Client.GuildMemberAdded += OnEventWithArgs;
                    break;
                case DiscordEventType.GuildMemberRemoved:
                    shard.Client.GuildMemberRemoved += OnEventWithArgs;
                    break;
                case DiscordEventType.GuildMemberUpdated:
                    shard.Client.GuildMemberUpdated += OnEventWithArgs;
                    break;
                case DiscordEventType.GuildMembersChunked:
                    shard.Client.GuildMembersChunked += OnEventWithArgs;
                    break;
                case DiscordEventType.GuildRoleCreated:
                    shard.Client.GuildRoleCreated += OnEventWithArgs;
                    break;
                case DiscordEventType.GuildRoleUpdated:
                    shard.Client.GuildRoleUpdated += OnEventWithArgs;
                    break;
                case DiscordEventType.GuildRoleDeleted:
                    shard.Client.GuildRoleDeleted += OnEventWithArgs;
                    break;
                case DiscordEventType.GuildUnavailable:
                    shard.Client.GuildUnavailable += OnEventWithArgs;
                    break;
                case DiscordEventType.GuildUpdated:
                    shard.Client.GuildUpdated += OnEventWithArgs;
                    break;
                case DiscordEventType.Heartbeated:
                    shard.Client.Heartbeated += OnEventWithArgs;
                    break;
                case DiscordEventType.MessageAcknowledged:
                    shard.Client.MessageAcknowledged += OnEventWithArgs;
                    break;
                case DiscordEventType.MessagesBulkDeleted:
                    shard.Client.MessagesBulkDeleted += OnEventWithArgs;
                    break;
                case DiscordEventType.MessageCreated:
                    shard.Client.MessageCreated += OnEventWithArgs;
                    break;
                case DiscordEventType.MessageReactionAdded:
                    shard.Client.MessageReactionAdded += OnEventWithArgs;
                    break;
                case DiscordEventType.MessageReactionRemoved:
                    shard.Client.MessageReactionRemoved += OnEventWithArgs;
                    break;
                case DiscordEventType.MessageReactionsCleared:
                    shard.Client.MessageReactionsCleared += OnEventWithArgs;
                    break;
                case DiscordEventType.MessageDeleted:
                    shard.Client.MessageDeleted += OnEventWithArgs;
                    break;
                case DiscordEventType.MessageUpdated:
                    shard.Client.MessageUpdated += OnEventWithArgs;
                    break;
                case DiscordEventType.PresenceUpdated:
                    shard.Client.PresenceUpdated += OnEventWithArgs;
                    break;
                case DiscordEventType.Ready:
                    shard.Client.Ready += OnEventWithArgs;
                    break;
                case DiscordEventType.Resumed:
                    shard.Client.Resumed += OnEventWithArgs;
                    break;
                case DiscordEventType.SocketClosed:
                    shard.Client.SocketClosed += OnEventWithArgs;
                    break;
                case DiscordEventType.SocketErrored:
                    shard.Client.SocketErrored += OnEventWithArgs;
                    break;
                case DiscordEventType.SocketOpened:
                    shard.Client.SocketOpened += OnEventVoid;
                    break;
                case DiscordEventType.TypingStarted:
                    shard.Client.TypingStarted += OnEventWithArgs;
                    break;
                case DiscordEventType.UnknownEvent:
                    shard.Client.UnknownEvent += OnEventWithArgs;
                    break;
                case DiscordEventType.UserSettingsUpdated:
                    shard.Client.UserSettingsUpdated += OnEventWithArgs;
                    break;
                case DiscordEventType.UserUpdated:
                    shard.Client.UserUpdated += OnEventWithArgs;
                    break;
                case DiscordEventType.VoiceServerUpdated:
                    shard.Client.VoiceServerUpdated += OnEventWithArgs;
                    break;
                case DiscordEventType.VoiceStateUpdated:
                    shard.Client.VoiceStateUpdated += OnEventWithArgs;
                    break;
                case DiscordEventType.WebhooksUpdated:
                    shard.Client.WebhooksUpdated += OnEventWithArgs;
                    break;
                default:
                    Log.Warning("No logic for handling event type: {EventType}", Enum.GetName(typeof(DiscordEventType), this.EventType));
                    break;
            }
            #endregion
        }
    }
}