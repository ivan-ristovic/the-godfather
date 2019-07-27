using System;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus;
using Serilog;

namespace TheGodfather.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    internal sealed class AsyncEventListenerAttribute : Attribute
    {
        public DiscordEventType Target { get; }


        public AsyncEventListenerAttribute(DiscordEventType targetType)
        {
            this.Target = targetType;
        }


        public void Register(TheGodfatherShard shard, DiscordClient client, MethodInfo mi)
        {
            Task OnEventWithArgs(object e)
            {
                if (!shard.IsListening)
                    return Task.CompletedTask;

                _ = Task.Run(async () => {
                    try {
                        await (Task)mi.Invoke(null, new object[] { shard, e });
                    } catch (Exception ex) {
                        Log.Error(ex, "Async listener");
                    }
                });
                return Task.CompletedTask;
            }

            Task OnEventVoid()
            {
                if (!shard.IsListening)
                    return Task.CompletedTask;

                _ = Task.Run(async () => {
                    try {
                        await (Task)mi.Invoke(null, new object[] { shard });
                    } catch (Exception ex) {
                        Log.Error(ex, "Async listener");
                    }
                });
                return Task.CompletedTask;
            }

            #region Event hooking
            switch (this.Target) {
                case DiscordEventType.ClientErrored:
                    client.ClientErrored += OnEventWithArgs;
                    break;
                case DiscordEventType.SocketErrored:
                    client.SocketErrored += OnEventWithArgs;
                    break;
                case DiscordEventType.SocketOpened:
                    client.SocketOpened += OnEventVoid;
                    break;
                case DiscordEventType.SocketClosed:
                    client.SocketClosed += OnEventWithArgs;
                    break;
                case DiscordEventType.Ready:
                    client.Ready += OnEventWithArgs;
                    break;
                case DiscordEventType.Resumed:
                    client.Resumed += OnEventWithArgs;
                    break;
                case DiscordEventType.ChannelCreated:
                    client.ChannelCreated += OnEventWithArgs;
                    break;
                case DiscordEventType.DmChannelCreated:
                    client.DmChannelCreated += OnEventWithArgs;
                    break;
                case DiscordEventType.ChannelUpdated:
                    client.ChannelUpdated += OnEventWithArgs;
                    break;
                case DiscordEventType.ChannelDeleted:
                    client.ChannelDeleted += OnEventWithArgs;
                    break;
                case DiscordEventType.DmChannelDeleted:
                    client.DmChannelDeleted += OnEventWithArgs;
                    break;
                case DiscordEventType.ChannelPinsUpdated:
                    client.ChannelPinsUpdated += OnEventWithArgs;
                    break;
                case DiscordEventType.GuildCreated:
                    client.GuildCreated += OnEventWithArgs;
                    break;
                case DiscordEventType.GuildAvailable:
                    client.GuildAvailable += OnEventWithArgs;
                    break;
                case DiscordEventType.GuildUpdated:
                    client.GuildUpdated += OnEventWithArgs;
                    break;
                case DiscordEventType.GuildDeleted:
                    client.GuildDeleted += OnEventWithArgs;
                    break;
                case DiscordEventType.GuildUnavailable:
                    client.GuildUnavailable += OnEventWithArgs;
                    break;
                case DiscordEventType.MessageCreated:
                    client.MessageCreated += OnEventWithArgs;
                    break;
                case DiscordEventType.PresenceUpdated:
                    client.PresenceUpdated += OnEventWithArgs;
                    break;
                case DiscordEventType.GuildBanAdded:
                    client.GuildBanAdded += OnEventWithArgs;
                    break;
                case DiscordEventType.GuildBanRemoved:
                    client.GuildBanRemoved += OnEventWithArgs;
                    break;
                case DiscordEventType.GuildEmojisUpdated:
                    client.GuildEmojisUpdated += OnEventWithArgs;
                    break;
                case DiscordEventType.GuildIntegrationsUpdated:
                    client.GuildIntegrationsUpdated += OnEventWithArgs;
                    break;
                case DiscordEventType.GuildMemberAdded:
                    client.GuildMemberAdded += OnEventWithArgs;
                    break;
                case DiscordEventType.GuildMemberRemoved:
                    client.GuildMemberRemoved += OnEventWithArgs;
                    break;
                case DiscordEventType.GuildMemberUpdated:
                    client.GuildMemberUpdated += OnEventWithArgs;
                    break;
                case DiscordEventType.GuildRoleCreated:
                    client.GuildRoleCreated += OnEventWithArgs;
                    break;
                case DiscordEventType.GuildRoleUpdated:
                    client.GuildRoleUpdated += OnEventWithArgs;
                    break;
                case DiscordEventType.GuildRoleDeleted:
                    client.GuildRoleDeleted += OnEventWithArgs;
                    break;
                case DiscordEventType.MessageAcknowledged:
                    client.MessageAcknowledged += OnEventWithArgs;
                    break;
                case DiscordEventType.MessageUpdated:
                    client.MessageUpdated += OnEventWithArgs;
                    break;
                case DiscordEventType.MessageDeleted:
                    client.MessageDeleted += OnEventWithArgs;
                    break;
                case DiscordEventType.MessagesBulkDeleted:
                    client.MessagesBulkDeleted += OnEventWithArgs;
                    break;
                case DiscordEventType.TypingStarted:
                    client.TypingStarted += OnEventWithArgs;
                    break;
                case DiscordEventType.UserSettingsUpdated:
                    client.UserSettingsUpdated += OnEventWithArgs;
                    break;
                case DiscordEventType.UserUpdated:
                    client.UserUpdated += OnEventWithArgs;
                    break;
                case DiscordEventType.VoiceStateUpdated:
                    client.VoiceStateUpdated += OnEventWithArgs;
                    break;
                case DiscordEventType.VoiceServerUpdated:
                    client.VoiceServerUpdated += OnEventWithArgs;
                    break;
                case DiscordEventType.GuildMembersChunked:
                    client.GuildMembersChunked += OnEventWithArgs;
                    break;
                case DiscordEventType.UnknownEvent:
                    client.UnknownEvent += OnEventWithArgs;
                    break;
                case DiscordEventType.MessageReactionAdded:
                    client.MessageReactionAdded += OnEventWithArgs;
                    break;
                case DiscordEventType.MessageReactionRemoved:
                    client.MessageReactionRemoved += OnEventWithArgs;
                    break;
                case DiscordEventType.MessageReactionsCleared:
                    client.MessageReactionsCleared += OnEventWithArgs;
                    break;
                case DiscordEventType.WebhooksUpdated:
                    client.WebhooksUpdated += OnEventWithArgs;
                    break;
                case DiscordEventType.Heartbeated:
                    client.Heartbeated += OnEventWithArgs;
                    break;
                case DiscordEventType.CommandExecuted:
                    shard.CNext.CommandExecuted += OnEventWithArgs;
                    break;
                case DiscordEventType.CommandErrored:
                    shard.CNext.CommandErrored += OnEventWithArgs;
                    break;
            }
            #endregion
        }
    }
}