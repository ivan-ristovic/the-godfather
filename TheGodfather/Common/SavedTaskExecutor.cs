#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;

using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TheGodfather.Common.Collections;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Services;
#endregion

namespace TheGodfather.Common
{
    public sealed class SavedTaskExecutor : AsyncExecutor, IDisposable
    {
        public int Id { get; private set; }
        public SavedTaskInfo TaskInfo { get; }

        private readonly DiscordClient client;
        private readonly SharedData shared;
        private readonly DBService db;
        private Timer timer;

        
        public static async Task ScheduleAsync(SharedData shared, DBService db, DiscordClient client, SavedTaskInfo task)
        {
            SavedTaskExecutor texec = null;
            try {
                int id = await db.AddSavedTaskAsync(task);
                texec = new SavedTaskExecutor(id, client, task, shared, db);
                texec.Schedule();
            } catch (Exception e) {
                await texec?.UnscheduleAsync();
                shared.LogProvider.LogException(LogLevel.Warning, e);
                throw;
            }
        }

        public static Task UnscheduleAsync(SharedData shared, ulong uid, int id)
        {
            if (shared.RemindExecuters.ContainsKey(uid))
                return shared.RemindExecuters[uid].FirstOrDefault(t => t.Id == id)?.UnscheduleAsync() ?? Task.CompletedTask;
            else
                return Task.CompletedTask;
        }


        public SavedTaskExecutor(int id, DiscordClient client, SavedTaskInfo task, SharedData data, DBService db)
        {
            this.Id = id;
            this.client = client;
            this.TaskInfo = task;
            this.shared = data;
            this.db = db;
        }


        public void Dispose()
        {
            this.timer?.Dispose();
        }

        public void Schedule()
        {
            switch (this.TaskInfo) {
                case SendMessageTaskInfo smti:
                    this.timer = new Timer(this.SendMessageCallback, this.TaskInfo, smti.IsRepeating ? smti.RepeatingInterval : smti.TimeUntilExecution, smti.RepeatingInterval);
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
                        DiscordChannel channel = await this.client.GetChannelAsync(smti.ChannelId);
                        DiscordUser user = await this.client.GetUserAsync(smti.InitiatorId);
                        if (smti.IsRepeating) {
                            unschedule = false;
                            this.Schedule();
                        } else {
                            await channel.SendMessageAsync($"{user.Mention}'s reminder:", embed: new DiscordEmbedBuilder() {
                                Description = $"{StaticDiscordEmoji.BoardPieceX} I have been asleep and failed to remind {user.Mention} to:\n\n{smti.Message}\n\n{smti.ExecutionTime.ToUtcTimestamp()}",
                                Color = DiscordColor.Red
                            });
                        }
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
                this.shared.LogProvider.LogException(LogLevel.Warning, e);
            } finally {
                if (unschedule)
                    await this.UnscheduleAsync();
            }
        }


        private Task UnscheduleAsync()
        {
            this.Dispose();

            switch (this.TaskInfo) {
                case SendMessageTaskInfo smti:
                    if (this.shared.RemindExecuters.ContainsKey(smti.InitiatorId)) {
                        if (!this.shared.RemindExecuters[smti.InitiatorId].TryRemove(this))
                            throw new ConcurrentOperationException("Failed to unschedule reminder!");
                        if (this.shared.RemindExecuters[smti.InitiatorId].Count == 0)
                            this.shared.RemindExecuters.TryRemove(smti.InitiatorId, out var _);
                    }
                    return this.db.RemoveReminderAsync(this.Id);
                case UnbanTaskInfo _:
                case UnmuteTaskInfo _:
                    if (this.shared.TaskExecuters.ContainsKey(this.Id))
                        if (!this.shared.TaskExecuters.TryRemove(this.Id, out var _))
                            throw new ConcurrentOperationException("Failed to unschedule saved task!");
                    return this.db.RemoveSavedTaskAsync(this.Id);
                default:
                    throw new ArgumentException("Unknown saved task info type!", nameof(this.TaskInfo));
            }
        }


        #region CALLBACKS
        private void SendMessageCallback(object _)
        {
            var info = _ as SendMessageTaskInfo;

            try {
                DiscordChannel channel = this.Execute(this.client.GetChannelAsync(info.ChannelId));
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
                if (role == null)
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
