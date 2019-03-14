#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.Entities;

using System;
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
                shared.LogProvider.LogException(LogLevel.Warning, e);
                throw;
            }
        }

        public static Task UnscheduleAsync(SharedData shared, ulong uid, int id)
        {
            if (shared.RemindExecuters.TryGetValue(uid, out ConcurrentHashSet<SavedTaskExecutor> texecs))
                return texecs.SingleOrDefault(t => t.Id == id)?.UnscheduleAsync() ?? Task.CompletedTask;
            else
                return Task.CompletedTask;
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
                    this.shared.RemindExecuters.AddOrUpdate(
                        smti.InitiatorId,
                        new ConcurrentHashSet<SavedTaskExecutor>() { this },
                        (k, v) => {
                            v.Add(this);
                            return v;
                        }
                    );
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
                        if (smti.ChannelId != 0)
                            channel = await this.client.GetChannelAsync(smti.ChannelId);
                        else
                            channel = await this.client.CreateDmChannelAsync(smti.InitiatorId);
                        DiscordUser user = await this.client.GetUserAsync(smti.InitiatorId);
                        await channel?.SendMessageAsync($"{user.Mention}'s reminder:", embed: new DiscordEmbedBuilder() {
                            Description = $"{StaticDiscordEmoji.BoardPieceX} I have been asleep and failed to remind {user.Mention} to:\n\n{smti.Message}\n\n{smti.ExecutionTime.ToUtcTimestamp()}",
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
                this.shared.LogProvider.LogMessage(LogLevel.Debug, $"| Executed missed task: {this.TaskInfo.GetType().ToString()}");
            } catch (Exception e) {
                this.shared.LogProvider.LogException(LogLevel.Debug, e);
            } finally {
                try {
                    if (unschedule)
                        await this.UnscheduleAsync();
                } catch (Exception e) {
                    this.shared.LogProvider.LogException(LogLevel.Debug, e);
                }
            }
        }


        private async Task UnscheduleAsync()
        {
            this.Dispose();

            switch (this.TaskInfo) {
                case SendMessageTaskInfo smti:
                    if (this.shared.RemindExecuters.TryGetValue(smti.InitiatorId, out ConcurrentHashSet<SavedTaskExecutor> texecs)) {
                        texecs.TryRemove(this);
                        if (texecs.Count == 0)
                            this.shared.RemindExecuters.TryRemove(smti.InitiatorId, out ConcurrentHashSet<SavedTaskExecutor> _);
                    }
                    using (DatabaseContext db = this.dbb.CreateContext()) {
                        db.Reminders.Remove(new DatabaseReminder() { Id = this.Id });
                        await db.SaveChangesAsync();
                    }
                    break;
                case UnbanTaskInfo _:
                case UnmuteTaskInfo _:
                    this.shared.TaskExecuters.TryRemove(this.Id, out SavedTaskExecutor _);
                    using (DatabaseContext db = this.dbb.CreateContext()) {
                        db.SavedTasks.Remove(new DatabaseSavedTask() { Id = this.Id });
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
                DiscordChannel channel;
                if (info.ChannelId != 0)
                    channel = this.Execute(this.client.GetChannelAsync(info.ChannelId));
                else
                    channel = this.Execute(this.client.CreateDmChannelAsync(info.InitiatorId));
                DiscordUser user = this.Execute(this.client.GetUserAsync(info.InitiatorId));
                this.Execute(channel.SendMessageAsync($"{user.Mention}'s reminder:", embed: new DiscordEmbedBuilder() {
                    Description = $"{StaticDiscordEmoji.AlarmClock} {info.Message}",
                    Color = DiscordColor.Orange
                }));
            } catch (Exception e) {
                this.shared.LogProvider.LogException(LogLevel.Warning, e);
            } finally {
                if (!info.IsRepeating)
                    this.Execute(this.UnscheduleAsync());
            }
        }

        private void UnbanUserCallback(object _)
        {
            var info = _ as UnbanTaskInfo;

            try {
                DiscordGuild guild = this.Execute(this.client.GetGuildAsync(info.GuildId));
                this.Execute(guild.UnbanMemberAsync(info.UnbanId, $"Temporary ban time expired"));
            } catch (Exception e) {
                this.shared.LogProvider.LogException(LogLevel.Warning, e);
            } finally {
                this.Execute(this.UnscheduleAsync());
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
            } catch (Exception e) {
                this.shared.LogProvider.LogException(LogLevel.Warning, e);
            } finally {
                this.Execute(this.UnscheduleAsync());
            }
        }
        #endregion
    }
}
