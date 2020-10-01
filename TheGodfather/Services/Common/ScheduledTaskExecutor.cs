using System;
using System.Threading;
using System.Threading.Tasks;
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
        public int Id { get; private set; }
        public ScheduledTask Job { get; }

        public delegate Task TaskExecuted(int id, ScheduledTask task);
        public event TaskExecuted OnTaskExecuted;

        private readonly TheGodfatherShard shard;
        private readonly AsyncExecutionService async;
        private Timer? timer;


        public ScheduledTaskExecutor(int id, TheGodfatherShard shard, AsyncExecutionService async, ScheduledTask task)
        {
            this.Id = id;
            this.Job = task;
            this.shard = shard;
            this.async = async;
            this.OnTaskExecuted += (id, task) => Task.CompletedTask;
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
                        DiscordChannel? channel = rem.ChannelId != 0
                            ? await this.shard.Client.GetChannelAsync(rem.ChannelId)
                            : await this.shard.Client.CreateDmChannelAsync(rem.UserId);
                        if (channel is null) {
                            Log.Warning("Cannot find channel for reminder with id {ReminderId} (channel: {ChannelId}, user: {UserId})", rem.Id, rem.ChannelId, rem.UserId);
                            break;
                        }
                        DiscordUser user = await this.shard.Client.GetUserAsync(rem.UserId);
                        await channel.SendMessageAsync($"{user.Mention}'s reminder:", embed: new DiscordEmbedBuilder {
                            Description = $"{Emojis.X} I have been asleep and failed to remind {user.Mention} to:\n\n{rem.Message}\n\n{rem.ExecutionTime.ToUtcTimestamp()}",
                            Color = DiscordColor.Red
                        });
                        break;
                    default:
                        throw new ArgumentException("Unknown saved task info type!", nameof(this.Job));
                }
                Log.Debug("Executed missed saved task of type: {SavedTaskType}", this.Job.GetType().ToString());
            } catch (Exception e) {
                Log.Debug(e, "Error while handling missed saved task");
            }
        }


        #region Callbacks
        private void SendMessageCallback(object? _)
        {
            Reminder? rem = _ as Reminder ?? throw new InvalidCastException("Failed to cast scheduled task to Reminder");

            try {
                DiscordChannel? channel = rem.ChannelId != 0
                    ? this.async.Execute(this.shard.Client.GetChannelAsync(rem.ChannelId))
                    : this.async.Execute(this.shard.Client.CreateDmChannelAsync(rem.UserId));
                if (channel is null)
                    return;
                DiscordUser user = this.async.Execute(this.shard.Client.GetUserAsync(rem.UserId));
                this.async.Execute(channel.SendMessageAsync($"{user.Mention}'s reminder:", embed: new DiscordEmbedBuilder {
                    Description = $"{Emojis.AlarmClock} {rem.Message}",
                    Color = DiscordColor.Orange
                }));
            } catch (UnauthorizedException) {
                // Do nothing, user has disabled DM in meantime
            } catch (Exception e) {
                Log.Debug(e, "Error while handling send message saved task");
            } finally {
                if (!rem.IsRepeating) {
                    try {
                        this.async.Execute(this.OnTaskExecuted(this.Id, this.Job));
                    } catch (Exception e) {
                        Log.Error(e, "Error while unscheduling send message saved task");
                    }
                }
            }
        }

        private void UnbanUserCallback(object? _)
        {
            GuildTask? gt = _ as GuildTask ?? throw new InvalidCastException("Failed to cast scheduled task to GuildTask");

            try {
                DiscordGuild guild = this.async.Execute(this.shard.Client.GetGuildAsync(gt.GuildId));
                this.async.Execute(guild.UnbanMemberAsync(gt.UserId, $"Temporary ban time expired"));
            } catch (UnauthorizedException) {
                // Do nothing, perms to unban removed in meantime
            } catch (Exception e) {
                Log.Debug(e, "Error while handling unban saved task");
            } finally {
                try {
                    this.async.Execute(this.OnTaskExecuted(this.Id, this.Job));
                } catch (Exception e) {
                    Log.Error(e, "Error while unscheduling unban saved task");
                }
            }
        }

        private void UnmuteUserCallback(object? _)
        {
            GuildTask? gt = _ as GuildTask ?? throw new InvalidCastException("Failed to cast scheduled task to GuildTask");

            try {
                DiscordGuild guild = this.async.Execute(this.shard.Client.GetGuildAsync(gt.GuildId));
                DiscordRole role = guild.GetRole(gt.RoleId);
                DiscordMember member = this.async.Execute(guild.GetMemberAsync(gt.UserId));
                if (role is null)
                    return;
                this.async.Execute(member.RevokeRoleAsync(role, $"Temporary mute time expired"));
            } catch (Exception e) when (e is UnauthorizedException || e is NotFoundException) {
                // Do nothing, perms to unmute removed in meantime or 404
            } catch (Exception e) {
                Log.Debug(e, "Error while handling unmute saved task");
            } finally {
                try {
                    this.async.Execute(this.OnTaskExecuted(this.Id, this.Job));
                } catch (Exception e) {
                    Log.Error(e, "Error while unscheduling unmute saved task");
                }
            }
        }
        #endregion
    }
}
