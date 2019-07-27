#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TheGodfather.Common.Collections;
using TheGodfather.Database;
using TheGodfather.Database.Entities;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
#endregion

namespace TheGodfather.Common
{
    public sealed class SavedTaskExecutor : AsyncExecutor, IDisposable
    {
        public int Id { get; private set; }
        public SavedTaskInfo TaskInfo { get; }

        private readonly DiscordClient client;
        private readonly SharedData shared;
        private readonly DatabaseContextBuilder dbb;
        private Timer timer;


        public static async Task ScheduleAsync(SharedData shared, DatabaseContextBuilder dbb, DiscordClient client, SavedTaskInfo task)
        {
            SavedTaskExecutor texec = null;
            try {
                using (DatabaseContext db = dbb.CreateContext()) {
                    if (task is SendMessageTaskInfo) {
                        var dbtask = DatabaseReminder.FromSavedTaskInfo(task);
                        db.Reminders.Add(dbtask);
                        await db.SaveChangesAsync();
                        texec = new SavedTaskExecutor(dbtask.Id, client, task, shared, dbb);
                    } else {
                        var dbtask = DatabaseSavedTask.FromSavedTaskInfo(task);
                        db.SavedTasks.Add(dbtask);
                        await db.SaveChangesAsync();
                        texec = new SavedTaskExecutor(dbtask.Id, client, task, shared, dbb);
                    }
                }
                texec.Schedule();
            } catch (Exception e) {
                await texec?.UnscheduleAsync();
                Log.Warning(e, "Saved Task schedule failed");
                throw;
            }
        }

        public static Task UnscheduleAsync(SharedData shared, ulong uid, int id)
        {
            return shared.RemindExecuters.TryGetValue(uid, out ConcurrentDictionary<int, SavedTaskExecutor> texecs)
                ? texecs.TryGetValue(id, out SavedTaskExecutor texec) ? texec.UnscheduleAsync() : Task.CompletedTask
                : Task.CompletedTask;
        }


        public SavedTaskExecutor(int id, DiscordClient client, SavedTaskInfo task, SharedData data, DatabaseContextBuilder dbb)
        {
            this.Id = id;
            this.client = client;
            this.TaskInfo = task;
            this.shared = data;
            this.dbb = dbb;
        }


        public void Dispose() 
            => this.timer?.Dispose();

        public void Schedule()
        {
            switch (this.TaskInfo) {
                case SendMessageTaskInfo smti:
                    this.timer = new Timer(this.SendMessageCallback, this.TaskInfo, smti.TimeUntilExecution, smti.RepeatingInterval);
                    if (!this.shared.RemindExecuters.TryGetValue(smti.InitiatorId, out ConcurrentDictionary<int, SavedTaskExecutor> texecs))
                        texecs = new ConcurrentDictionary<int, SavedTaskExecutor>();
                    if (!texecs.TryAdd(this.Id, this))
                        throw new ConcurrentOperationException("Failed to add reminder!");
                    this.shared.RemindExecuters.AddOrUpdate(smti.InitiatorId, texecs, (k, v) => texecs);
                    break;
                case UnbanTaskInfo _:
                    this.timer = new Timer(this.UnbanUserCallback, this.TaskInfo, this.TaskInfo.TimeUntilExecution, TimeSpan.FromMilliseconds(-1));
                    if (!this.shared.TaskExecuters.TryAdd(this.Id, this))
                        throw new ConcurrentOperationException("Failed to schedule the task.");
                    break;
                case UnmuteTaskInfo _:
                    this.timer = new Timer(this.UnmuteUserCallback, this.TaskInfo, this.TaskInfo.TimeUntilExecution, TimeSpan.FromMilliseconds(-1));
                    if (!this.shared.TaskExecuters.TryAdd(this.Id, this))
                        throw new ConcurrentOperationException("Failed to schedule the task.");
                    break;
                default:
                    throw new ArgumentException("Unknown saved task info type!", nameof(this.TaskInfo));
            }
        }

        public async Task HandleMissedExecutionAsync()
        {
            bool unschedule = true;

            try {
                switch (this.TaskInfo) {
                    case SendMessageTaskInfo smti:
                        DiscordChannel channel;
                        channel = smti.ChannelId != 0
                            ? await this.client.GetChannelAsync(smti.ChannelId)
                            : await this.client.CreateDmChannelAsync(smti.InitiatorId);
                        DiscordUser user = await this.client.GetUserAsync(smti.InitiatorId);
                        await channel?.SendMessageAsync($"{user.Mention}'s reminder:", embed: new DiscordEmbedBuilder {
                            Description = $"{StaticDiscordEmoji.X} I have been asleep and failed to remind {user.Mention} to:\n\n{smti.Message}\n\n{smti.ExecutionTime.ToUtcTimestamp()}",
                            Color = DiscordColor.Red
                        });
                        break;
                    case UnbanTaskInfo _:
                        this.UnbanUserCallback(this.TaskInfo);
                        break;
                    case UnmuteTaskInfo _:
                        this.UnmuteUserCallback(this.TaskInfo);
                        break;
                }
                Log.Debug($"Executed missed saved task of type: {this.TaskInfo.GetType().ToString()}");
            } catch (Exception e) {
                Log.Debug(e, "Error while handling missed saved task");
            } finally {
                try {
                    if (unschedule)
                        await this.UnscheduleAsync();
                } catch (Exception e) {
                    Log.Error(e, "Error while unscheduling missed saved task");
                }
            }
        }


        private async Task UnscheduleAsync()
        {
            this.Dispose();

            switch (this.TaskInfo) {
                case SendMessageTaskInfo smti:
                    Exception ex = null;
                    if (this.shared.RemindExecuters.TryGetValue(smti.InitiatorId, out ConcurrentDictionary<int, SavedTaskExecutor> texecs)) {
                        if (!texecs.TryRemove(this.Id, out _))
                            ex = new ConcurrentOperationException("Failed to remove reminder from the dictionary!");
                        if (texecs.Count == 0)
                            this.shared.RemindExecuters.TryRemove(smti.InitiatorId, out ConcurrentDictionary<int, SavedTaskExecutor> _);
                    }
                    using (DatabaseContext db = this.dbb.CreateContext()) {
                        db.Reminders.Remove(new DatabaseReminder { Id = this.Id });
                        await db.SaveChangesAsync();
                    }
                    if (!(ex is null))
                        throw ex;
                    break;
                case UnbanTaskInfo _:
                case UnmuteTaskInfo _:
                    this.shared.TaskExecuters.TryRemove(this.Id, out SavedTaskExecutor _);
                    using (DatabaseContext db = this.dbb.CreateContext()) {
                        db.SavedTasks.Remove(new DatabaseSavedTask { Id = this.Id });
                        await db.SaveChangesAsync();
                    }
                    break;
                default:
                    throw new ArgumentException("Unknown saved task info type!", nameof(this.TaskInfo));
            }
        }


        #region CALLBACKS
        private void SendMessageCallback(object _)
        {
            var info = _ as SendMessageTaskInfo;

            try {
                DiscordChannel channel = info.ChannelId != 0
                    ? this.Execute(this.client.GetChannelAsync(info.ChannelId))
                    : this.Execute(this.client.CreateDmChannelAsync(info.InitiatorId));
                DiscordUser user = this.Execute(this.client.GetUserAsync(info.InitiatorId));
                this.Execute(channel.SendMessageAsync($"{user.Mention}'s reminder:", embed: new DiscordEmbedBuilder {
                    Description = $"{StaticDiscordEmoji.AlarmClock} {info.Message}",
                    Color = DiscordColor.Orange
                }));
            } catch (UnauthorizedException) {
                // Do nothing, user has disabled DM in meantime
            } catch (Exception e) {
                Log.Debug(e, "Error while handling send message saved task");
            } finally {
                if (!info.IsRepeating) {
                    try {
                        this.Execute(this.UnscheduleAsync());
                    } catch (Exception e) {
                        Log.Error(e, "Error while unscheduling send message saved task");
                    }
                }
            }
        }

        private void UnbanUserCallback(object _)
        {
            var info = _ as UnbanTaskInfo;

            try {
                DiscordGuild guild = this.Execute(this.client.GetGuildAsync(info.GuildId));
                this.Execute(guild.UnbanMemberAsync(info.UnbanId, $"Temporary ban time expired"));
            } catch (UnauthorizedException) {
                // Do nothing, perms to unban removed in meantime
            } catch (Exception e) {
                Log.Debug(e, "Error while handling unban saved task");
            } finally {
                try {
                    this.Execute(this.UnscheduleAsync());
                } catch (Exception e) {
                    Log.Error(e, "Error while unscheduling unban saved task");
                }
            }
        }

        private void UnmuteUserCallback(object _)
        {
            var info = _ as UnmuteTaskInfo;

            try {
                DiscordGuild guild = this.Execute(this.client.GetGuildAsync(info.GuildId));
                DiscordRole role = guild.GetRole(info.MuteRoleId);
                DiscordMember member = this.Execute(guild.GetMemberAsync(info.UserId));
                if (role is null)
                    return;
                this.Execute(member.RevokeRoleAsync(role, $"Temporary mute time expired"));
            } catch (UnauthorizedException) {
                // Do nothing, perms to unmute removed in meantime
            } catch (Exception e) {
                Log.Debug(e, "Error while handling unmute saved task");
            } finally {
                try {
                    this.Execute(this.UnscheduleAsync());
                } catch (Exception e) {
                    Log.Error(e, "Error while unscheduling unmute saved task");
                }
            }
        }
        #endregion
    }
}
