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
        public SavedTask SavedTask { get; }

        private readonly DiscordClient client;
        private readonly SharedData shared;
        private readonly DBService db;
        private Timer timer;


        public static async Task<bool> TryScheduleAsync(CommandContext ctx, SavedTask task)
        {
            var shared = ctx.Services.GetService<SharedData>();
            var db = ctx.Services.GetService<DBService>();
            SavedTaskExecutor texec = null;
            try {
                int id = await db.AddSavedTaskAsync(task);
                texec = new SavedTaskExecutor(id, ctx.Client, task, shared, db);
                texec.Schedule();
            } catch (Exception e) {
                shared.LogProvider.LogException(LogLevel.Warning, e);
                await texec?.UnscheduleAsync();
                return false;
            }
            return true;
        }


        public SavedTaskExecutor(int id, DiscordClient client, SavedTask task, SharedData data, DBService db)
        {
            this.Id = id;
            this.client = client;
            this.SavedTask = task;
            this.shared = data;
            this.db = db;
        }


        public void Dispose()
        {
            this.timer.Dispose();
        }

        public void Schedule()
        {
            switch (this.SavedTask.Type) {
                case SavedTaskType.SendMessage:
                    this.timer = new Timer(this.SendMessageCallback, null, this.SavedTask.TimeUntilExecution, TimeSpan.FromMilliseconds(-1));
                    break;
                case SavedTaskType.Unban:
                    this.timer = new Timer(this.UnbanUserCallback, null, this.SavedTask.TimeUntilExecution, TimeSpan.FromMilliseconds(-1));
                    break;
            }

            this.shared.TaskExecuters.TryAdd(this.Id, this);
        }

        public async Task HandleMissedExecutionAsync()
        {
            try {
                switch (this.SavedTask.Type) {
                    case SavedTaskType.SendMessage:
                        var channel = await this.client.GetChannelAsync(this.SavedTask.ChannelId);
                        var user = await this.client.GetUserAsync(this.SavedTask.UserId);
                        await channel.InformFailureAsync($"I have been asleep and failed to remind {user.Mention} to:\n\n{Formatter.Italic(this.SavedTask.Comment)}\n\nat {this.SavedTask.ExecutionTime.ToLongTimeString()} UTC");
                        break;
                    case SavedTaskType.Unban:
                        UnbanUserCallback(null);
                        break;
                    default:
                        break;
                }
                this.shared.LogProvider.LogMessage(LogLevel.Warning,
                    $"| Executed missed task: {this.SavedTask.Type.ToTypeString()}\n" +
                    $"| Task comment: {this.SavedTask.Comment}\n" +
                    $"| User ID: {this.SavedTask.UserId}\n" +
                    $"| Guild ID: {this.SavedTask.GuildId}\n" +
                    $"| Channel ID: {this.SavedTask.ChannelId}"
                );
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
            try {
                DiscordChannel channel = Execute(this.client.GetChannelAsync(this.SavedTask.ChannelId));
                DiscordUser user = Execute(this.client.GetUserAsync(this.SavedTask.UserId));
                Execute(channel.EmbedAsync($"{user.Mention}'s reminder:\n\n{Formatter.Italic(this.SavedTask.Comment)}", StaticDiscordEmoji.AlarmClock));
            } catch (Exception e) {
                this.shared.LogProvider.LogException(LogLevel.Warning, e);
            } finally {
                Execute(UnscheduleAsync());
            }
        }

        private void UnbanUserCallback(object _)
        {
            try { 
                DiscordGuild guild = Execute(this.client.GetGuildAsync(this.SavedTask.GuildId));
                Execute(guild.UnbanMemberAsync(this.SavedTask.UserId, $"Temporary ban time expired"));
            } catch (Exception e) {
                this.shared.LogProvider.LogException(LogLevel.Warning, e);
            } finally {
                Execute(UnscheduleAsync());
            }
        }
        #endregion
    }
}
