using System;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Serilog;
using TheGodfather.Common;
using TheGodfather.Database.Models;
using TheGodfather.Extensions;

namespace TheGodfather.Services.Common
{
    public sealed class ScheduledTaskExecutor : IDisposable
    {
        public int Id => this.Job.Id;
        public ScheduledTask Job { get; }

        public delegate Task TaskExecuted(ScheduledTask task, bool force);
        public event TaskExecuted OnTaskExecuted;

        private readonly DiscordShardedClient client;
        private readonly LocalizationService lcs;
        private readonly AsyncExecutionService async;
        private Timer? timer;


        public ScheduledTaskExecutor(DiscordShardedClient client, LocalizationService lcs, AsyncExecutionService async, ScheduledTask task)
        {
            this.client = client;
            this.lcs = lcs;
            this.async = async;
            this.Job = task;
            this.OnTaskExecuted += (task, _) => Task.CompletedTask;
        }


        public void Dispose()
            => this.timer?.Dispose();

        public void ScheduleExecution()
        {
            switch (this.Job) {
                case GuildTask gt:
                    switch (gt.Type) {
                        case ScheduledTaskType.Unban:
                            this.timer = new Timer(this.UnbanUserCallback, this.Job, this.Job.TimeUntilExecution, TimeSpan.FromMilliseconds(-1));
                            break;
                        case ScheduledTaskType.Unmute:
                            this.timer = new Timer(this.UnmuteUserCallback, this.Job, this.Job.TimeUntilExecution, TimeSpan.FromMilliseconds(-1));
                            break;
                    }
                    break;
                case Reminder rem:
                    this.timer = new Timer(this.SendMessageCallback, this.Job, rem.TimeUntilExecution, rem.RepeatInterval);
                    break;
                default:
                    throw new ArgumentException("Unknown saved task info type!", nameof(this.Job));
            }
        }

        public async Task HandleMissedExecutionAsync()
        {
            try {
                switch (this.Job) {
                    case GuildTask gt:
                        switch (gt.Type) {
                            case ScheduledTaskType.Unban:
                                this.UnbanUserCallback(this.Job);
                                break;
                            case ScheduledTaskType.Unmute:
                                this.UnmuteUserCallback(this.Job);
                                break;
                        }
                        break;
                    case Reminder rem:
                        DiscordClient client = this.client.GetShard(0);
                        DiscordChannel? channel = rem.ChannelId != 0
                            ? await client.GetChannelAsync(rem.ChannelId)
                            : await client.CreateDmChannelAsync(rem.UserId);
                        if (channel is null) {
                            Log.Warning("Cannot find channel for reminder with id {ReminderId} (channel: {ChannelId}, user: {UserId})",
                                rem.Id, rem.ChannelId, rem.UserId
                            );
                            break;
                        }
                        await channel.LocalizedEmbedAsync(this.lcs, Emojis.X, DiscordColor.Red, "fmt-remind-miss",
                            this.lcs.GetLocalizedTimeString(channel.GuildId, rem.ExecutionTime), rem.Message
                        );
                        break;
                    default:
                        throw new ArgumentException("Unknown saved task info type!", nameof(this.Job));
                }
                Log.Debug("Executed missed saved task of type: {SavedTaskType}", this.Job.GetType().Name);
            } catch (Exception e) {
                Log.Debug(e, "Error while handling missed saved task");
            }
        }


        #region Callbacks
        private void SendMessageCallback(object? _)
        {
            Reminder? rem = _ as Reminder ?? throw new InvalidCastException("Failed to cast scheduled task to Reminder");

            try {
                DiscordClient client = this.client.GetShard(0);
                DiscordChannel? channel = rem.ChannelId != 0
                    ? this.async.Execute(client.GetChannelAsync(rem.ChannelId))
                    : this.async.Execute(client.CreateDmChannelAsync(rem.UserId));
                if (channel is null)
                    return;
                DiscordUser user = this.async.Execute(client.GetUserAsync(rem.UserId));

                this.async.Execute(
                    channel.LocalizedEmbedAsync(this.lcs, Emojis.AlarmClock, DiscordColor.Green, "fmt-remind-exec", user.Mention, rem.Message)
                );
            } catch (UnauthorizedException) {
                // Do nothing, user has disabled DM in meantime
            } catch (Exception e) {
                Log.Debug(e, "Error while handling send message saved task");
            } finally {
                try {
                    this.async.Execute(this.OnTaskExecuted(this.Job, false));
                } catch (Exception e) {
                    Log.Error(e, "Error while unscheduling send message saved task");
                }
            }
        }

        private void UnbanUserCallback(object? _)
        {
            GuildTask? gt = _ as GuildTask ?? throw new InvalidCastException("Failed to cast scheduled task to GuildTask");

            try {
                DiscordGuild guild = this.async.Execute(this.client.GetShard(gt.GuildId).GetGuildAsync(gt.GuildId));
                this.async.Execute(guild.UnbanMemberAsync(gt.UserId, $"Temporary ban time expired"));
            } catch (UnauthorizedException) {
                // Do nothing, perms to unban removed in meantime
            } catch (Exception e) {
                Log.Debug(e, "Error while handling unban saved task");
            } finally {
                try {
                    this.async.Execute(this.OnTaskExecuted(this.Job, false));
                } catch (Exception e) {
                    Log.Error(e, "Error while unscheduling unban saved task");
                }
            }
        }

        private void UnmuteUserCallback(object? _)
        {
            GuildTask? gt = _ as GuildTask ?? throw new InvalidCastException("Failed to cast scheduled task to GuildTask");

            try {
                DiscordGuild guild = this.async.Execute(this.client.GetShard(gt.GuildId).GetGuildAsync(gt.GuildId));
                DiscordRole? role = guild.GetRole(gt.RoleId);
                DiscordMember member = this.async.Execute(guild.GetMemberAsync(gt.UserId));
                if (role is null)
                    return;
                this.async.Execute(member.RevokeRoleAsync(role, $"Temporary mute time expired"));
            } catch (Exception e) when (e is UnauthorizedException or NotFoundException) {
                // Do nothing, perms to unmute removed in meantime or 404
            } catch (Exception e) {
                Log.Debug(e, "Error while handling unmute saved task");
            } finally {
                try {
                    this.async.Execute(this.OnTaskExecuted(this.Job, false));
                } catch (Exception e) {
                    Log.Error(e, "Error while unscheduling unmute saved task");
                }
            }
        }
        #endregion
    }
}
