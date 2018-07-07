#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;
using TheGodfather.Extensions;
using TheGodfather.Services;
using TheGodfather.Services.Common;
#endregion

namespace TheGodfather.Common
{
    public sealed class SavedTaskExecuter
    {
        public int Id { get; private set; }
        public SavedTask SavedTask { get; }

        private readonly DiscordClient client;
        private readonly SharedData shared;
        private readonly DBService db;
        private Timer timer;


        public static async Task<bool> ScheduleAsync(DiscordClient client, SharedData shared, DBService db, 
            ulong uid, ulong cid, ulong gid, SavedTaskType type, string comment, DateTime exectime)
        {
            var task = new SavedTask() {
                ChannelId = cid,
                Comment = comment,
                ExecutionTime = exectime,
                GuildId = gid,
                Type = type,
                UserId = uid
            };

            try {
                int id = await db.AddSavedTaskAsync(task)
                    .ConfigureAwait(false);
                var texec = new SavedTaskExecuter(id, client, task, shared, db);
                texec.ScheduleExecution();
            } catch (Exception e) {
                shared.LogProvider.LogException(LogLevel.Warning, e);
                return false;
            }

            return true;
        }


        public SavedTaskExecuter(int id, DiscordClient client, SavedTask task, SharedData data, DBService db)
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

        private void Execute(object _)
        {
            try {
                switch (this.SavedTask.Type) {
                    case SavedTaskType.SendMessage:
                        SendMessageAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                        break;
                    case SavedTaskType.Unban:
                        UnbanUserAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                        break;
                }
                this.shared.LogProvider.LogMessage(LogLevel.Info,
                    $"| Saved task executed: {this.SavedTask.Type.GetType()} ({this.SavedTask.Comment})\n" +
                    $"| User ID: {this.SavedTask.UserId}\n" +
                    $"| Guild ID: {this.SavedTask.GuildId}\n" +
                    $"| Channel ID: {this.SavedTask.ChannelId}"
                );
            } catch (Exception e) {
                this.shared.LogProvider.LogException(LogLevel.Warning, e);
            } finally {
                RemoveTaskFromDatabase().ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }

        public void ScheduleExecution()
        {
            this.shared.SavedTasks.TryAdd(this.Id, this);
            this.timer = new Timer(this.Execute, null, (int)this.SavedTask.TimeUntilExecution.TotalMilliseconds, Timeout.Infinite);
        }


        #region CALLBACKS
        public async Task RemoveTaskFromDatabase()
        {
            if (this.shared.SavedTasks.ContainsKey(this.Id))
                this.shared.SavedTasks.TryRemove(this.Id, out var _);
            await this.db.RemoveSavedTaskAsync(this.Id)
                .ConfigureAwait(false);
        }

        public async Task HandleMissedExecutionAsync()
        {
            try {
                switch (this.SavedTask.Type) {
                    case SavedTaskType.SendMessage:
                        var channel = await this.client.GetChannelAsync(this.SavedTask.ChannelId)
                            .ConfigureAwait(false);
                        var user = await this.client.GetUserAsync(this.SavedTask.UserId)
                            .ConfigureAwait(false);
                        await channel.SendFailedEmbedAsync($"I have been asleep and failed to remind {user.Mention} to:\n\n{Formatter.Italic(this.SavedTask.Comment)}\n\nat {this.SavedTask.ExecutionTime.ToLongTimeString()} UTC")
                            .ConfigureAwait(false);
                        break;
                    case SavedTaskType.Unban:
                        await UnbanUserAsync()
                            .ConfigureAwait(false);
                        break;
                    default:
                        break;
                }
                this.shared.LogProvider.LogMessage(LogLevel.Warning, 
                    $"| Executed missed saved task of type {this.SavedTask.Type.GetType()}\n|" +
                    $"| User ID: {this.SavedTask.UserId}\n" +
                    $"| Guild ID: {this.SavedTask.GuildId}\n" +
                    $"| Channel ID: {this.SavedTask.ChannelId}"
                );
            } catch (Exception e) {
                this.shared.LogProvider.LogException(LogLevel.Warning, e);
            } finally {
                RemoveTaskFromDatabase().ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }

        private async Task SendMessageAsync()
        {
            var channel = await this.client.GetChannelAsync(this.SavedTask.ChannelId)
                .ConfigureAwait(false);
            var user = await this.client.GetUserAsync(this.SavedTask.UserId)
                .ConfigureAwait(false);
            await channel.SendIconEmbedAsync($"{user.Mention}'s reminder:\n\n{Formatter.Italic(this.SavedTask.Comment)}", DiscordEmoji.FromName(this.client, ":alarm_clock:"))
                .ConfigureAwait(false);
        }

        private async Task UnbanUserAsync()
        {
            var guild = await this.client.GetGuildAsync(this.SavedTask.GuildId)
                .ConfigureAwait(false);
            var user = await this.client.GetUserAsync(this.SavedTask.UserId)
                .ConfigureAwait(false);
            await guild.UnbanMemberAsync(user, $"Temporary ban time expired")
                .ConfigureAwait(false);
        }
        #endregion
    }
}
