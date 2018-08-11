#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;

using Microsoft.Extensions.DependencyInjection;

using System;
using System.Threading;
using System.Threading.Tasks;

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

        
        public static async Task<bool> TryScheduleAsync(SharedData shared, DBService db, DiscordClient client, SavedTaskInfo task)
        {
            SavedTaskExecutor texec = null;
            try {
                int id = await db.AddSavedTaskAsync(task);
                texec = new SavedTaskExecutor(id, client, task, shared, db);
                texec.Schedule();
            } catch (Exception e) {
                shared.LogProvider.LogException(LogLevel.Warning, e);
                await texec?.UnscheduleAsync();
                return false;
            }
            return true;
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
                case SendMessageTaskInfo _:
                    this.timer = new Timer(this.SendMessageCallback, this.TaskInfo, this.TaskInfo.TimeUntilExecution, TimeSpan.FromMilliseconds(-1));
                    break;
                case UnbanTaskInfo _:
                    this.timer = new Timer(this.UnbanUserCallback, this.TaskInfo, this.TaskInfo.TimeUntilExecution, TimeSpan.FromMilliseconds(-1));
                    break;
                case UnmuteTaskInfo _:
                    this.timer = new Timer(this.UnmuteUserCallback, this.TaskInfo, this.TaskInfo.TimeUntilExecution, TimeSpan.FromMilliseconds(-1));
                    break;
                default:
                    throw new ArgumentException("Unknown saved task info type!", nameof(this.TaskInfo));
            }

            this.shared.TaskExecuters.TryAdd(this.Id, this);
        }

        public async Task HandleMissedExecutionAsync()
        {
            try {
                switch (this.TaskInfo) {
                    case SendMessageTaskInfo smi:
                        DiscordChannel channel = await this.client.GetChannelAsync(smi.ChannelId);
                        DiscordUser user = await this.client.GetUserAsync(smi.InitiatorId);
                        await channel.InformFailureAsync($"I have been asleep and failed to remind {user.Mention} to:\n\n{Formatter.Italic(smi.Message)}\n\n {smi.ExecutionTime.ToUtcTimestamp()}");
                        break;
                    case UnbanTaskInfo _:
                        UnbanUserCallback(null);
                        break;
                    case UnmuteTaskInfo _:
                        UnmuteUserCallback(null);
                        break;
                }
                this.shared.LogProvider.LogMessage(LogLevel.Warning, $"| Executed missed task: {this.TaskInfo.GetType().ToString()}");
            } catch (Exception e) {
                this.shared.LogProvider.LogException(LogLevel.Warning, e);
            } finally {
                await UnscheduleAsync();
            }
        }


        private Task UnscheduleAsync()
        {
            if (this.shared.TaskExecuters.ContainsKey(this.Id))
                if (!this.shared.TaskExecuters.TryRemove(this.Id, out var _))
                    throw new ConcurrentOperationException("Failed to unschedule saved task!");
            this.Dispose();
            return this.db.RemoveSavedTaskAsync(this.Id);
        }


        #region CALLBACKS
        private void SendMessageCallback(object _)
        {
            var info = _ as SendMessageTaskInfo;

            try {
                DiscordChannel channel = Execute(this.client.GetChannelAsync(info.ChannelId));
                DiscordUser user = Execute(this.client.GetUserAsync(info.InitiatorId));
                Execute(channel.EmbedAsync($"{user.Mention}'s reminder:\n\n{Formatter.Italic(info.Message)}", StaticDiscordEmoji.AlarmClock));
            } catch (Exception e) {
                this.shared.LogProvider.LogException(LogLevel.Warning, e);
            } finally {
                Execute(UnscheduleAsync());
            }
        }

        private void UnbanUserCallback(object _)
        {
            var info = _ as UnbanTaskInfo;

            try { 
                DiscordGuild guild = Execute(this.client.GetGuildAsync(info.GuildId));
                Execute(guild.UnbanMemberAsync(info.UnbanId, $"Temporary ban time expired"));
            } catch (Exception e) {
                this.shared.LogProvider.LogException(LogLevel.Warning, e);
            } finally {
                Execute(UnscheduleAsync());
            }
        }

        private void UnmuteUserCallback(object _)
        {
            var info = _ as UnmuteTaskInfo;

            try {
                DiscordGuild guild = Execute(this.client.GetGuildAsync(info.GuildId));
                DiscordRole role = guild.GetRole(info.MuteRoleId);
                DiscordMember member = Execute(guild.GetMemberAsync(info.UserId));
                if (role == null)
                    return;
                Execute(member.RevokeRoleAsync(role, $"Temporary mute time expired"));
            } catch (Exception e) {
                this.shared.LogProvider.LogException(LogLevel.Warning, e);
            } finally {
                Execute(UnscheduleAsync());
            }
        }
        #endregion
    }
}
