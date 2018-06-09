#region USING_DIRECTIVES
using System;
using System.Reflection;
using System.Threading.Tasks;

using DSharpPlus;
#endregion

namespace TheGodfather.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    internal class AsyncExecuterAttribute : Attribute
    {
        public EventTypes Target { get; }


        public AsyncExecuterAttribute(EventTypes targetType)
        {
            Target = targetType;
        }


        public void Register(TheGodfatherShard shard, DiscordClient client, MethodInfo mi)
        {
            Task OnEventWithArgs(object e)
            {
                _ = Task.Run(async () => {
                    try {
                        await (Task)mi.Invoke(null, new[] { shard, e });
                    } catch (Exception ex) {
                        TheGodfather.LogHandle.LogException(LogLevel.Error, ex);
                    }
                });
                return Task.CompletedTask;
            }

            Task OnEventVoid()
            {
                _ = Task.Run(async () => {
                    try {
                        await (Task)mi.Invoke(null, new object[] { shard });
                    } catch (Exception ex) {
                        TheGodfather.LogHandle.LogException(LogLevel.Error, ex);
                    }
                });
                return Task.CompletedTask;
            }

            switch (Target) {
                case EventTypes.ClientErrored:
                    client.ClientErrored += OnEventWithArgs;
                    break;
                case EventTypes.SocketErrored:
                    client.SocketErrored += OnEventWithArgs;
                    break;
                case EventTypes.SocketOpened:
                    client.SocketOpened += OnEventVoid;
                    break;
                case EventTypes.SocketClosed:
                    client.SocketClosed += OnEventWithArgs;
                    break;
                case EventTypes.Ready:
                    client.Ready += OnEventWithArgs;
                    break;
                case EventTypes.Resumed:
                    client.Resumed += OnEventWithArgs;
                    break;
                case EventTypes.ChannelCreated:
                    client.ChannelCreated += OnEventWithArgs;
                    break;
                case EventTypes.DmChannelCreated:
                    client.DmChannelCreated += OnEventWithArgs;
                    break;
                case EventTypes.ChannelUpdated:
                    client.ChannelUpdated += OnEventWithArgs;
                    break;
                case EventTypes.ChannelDeleted:
                    client.ChannelDeleted += OnEventWithArgs;
                    break;
                case EventTypes.DmChannelDeleted:
                    client.DmChannelDeleted += OnEventWithArgs;
                    break;
                case EventTypes.ChannelPinsUpdated:
                    client.ChannelPinsUpdated += OnEventWithArgs;
                    break;
                case EventTypes.GuildCreated:
                    client.GuildCreated += OnEventWithArgs;
                    break;
                case EventTypes.GuildAvailable:
                    client.GuildAvailable += OnEventWithArgs;
                    break;
                case EventTypes.GuildUpdated:
                    client.GuildUpdated += OnEventWithArgs;
                    break;
                case EventTypes.GuildDeleted:
                    client.GuildDeleted += OnEventWithArgs;
                    break;
                case EventTypes.GuildUnavailable:
                    client.GuildUnavailable += OnEventWithArgs;
                    break;
                case EventTypes.MessageCreated:
                    client.MessageCreated += OnEventWithArgs;
                    break;
                case EventTypes.PresenceUpdated:
                    client.PresenceUpdated += OnEventWithArgs;
                    break;
                case EventTypes.GuildBanAdded:
                    client.GuildBanAdded += OnEventWithArgs;
                    break;
                case EventTypes.GuildBanRemoved:
                    client.GuildBanRemoved += OnEventWithArgs;
                    break;
                case EventTypes.GuildEmojisUpdated:
                    client.GuildEmojisUpdated += OnEventWithArgs;
                    break;
                case EventTypes.GuildIntegrationsUpdated:
                    client.GuildIntegrationsUpdated += OnEventWithArgs;
                    break;
                case EventTypes.GuildMemberAdded:
                    client.GuildMemberAdded += OnEventWithArgs;
                    break;
                case EventTypes.GuildMemberRemoved:
                    client.GuildMemberRemoved += OnEventWithArgs;
                    break;
                case EventTypes.GuildMemberUpdated:
                    client.GuildMemberUpdated += OnEventWithArgs;
                    break;
                case EventTypes.GuildRoleCreated:
                    client.GuildRoleCreated += OnEventWithArgs;
                    break;
                case EventTypes.GuildRoleUpdated:
                    client.GuildRoleUpdated += OnEventWithArgs;
                    break;
                case EventTypes.GuildRoleDeleted:
                    client.GuildRoleDeleted += OnEventWithArgs;
                    break;
                case EventTypes.MessageAcknowledged:
                    client.MessageAcknowledged += OnEventWithArgs;
                    break;
                case EventTypes.MessageUpdated:
                    client.MessageUpdated += OnEventWithArgs;
                    break;
                case EventTypes.MessageDeleted:
                    client.MessageDeleted += OnEventWithArgs;
                    break;
                case EventTypes.MessagesBulkDeleted:
                    client.MessagesBulkDeleted += OnEventWithArgs;
                    break;
                case EventTypes.TypingStarted:
                    client.TypingStarted += OnEventWithArgs;
                    break;
                case EventTypes.UserSettingsUpdated:
                    client.UserSettingsUpdated += OnEventWithArgs;
                    break;
                case EventTypes.UserUpdated:
                    client.UserUpdated += OnEventWithArgs;
                    break;
                case EventTypes.VoiceStateUpdated:
                    client.VoiceStateUpdated += OnEventWithArgs;
                    break;
                case EventTypes.VoiceServerUpdated:
                    client.VoiceServerUpdated += OnEventWithArgs;
                    break;
                case EventTypes.GuildMembersChunked:
                    client.GuildMembersChunked += OnEventWithArgs;
                    break;
                case EventTypes.UnknownEvent:
                    client.UnknownEvent += OnEventWithArgs;
                    break;
                case EventTypes.MessageReactionAdded:
                    client.MessageReactionAdded += OnEventWithArgs;
                    break;
                case EventTypes.MessageReactionRemoved:
                    client.MessageReactionRemoved += OnEventWithArgs;
                    break;
                case EventTypes.MessageReactionsCleared:
                    client.MessageReactionsCleared += OnEventWithArgs;
                    break;
                case EventTypes.WebhooksUpdated:
                    client.WebhooksUpdated += OnEventWithArgs;
                    break;
                case EventTypes.Heartbeated:
                    client.Heartbeated += OnEventWithArgs;
                    break;
                case EventTypes.CommandExecuted:
                    shard.Commands.CommandExecuted += OnEventWithArgs;
                    break;
                case EventTypes.CommandErrored:
                    shard.Commands.CommandErrored += OnEventWithArgs;
                    break;
            }
        }
    }
}