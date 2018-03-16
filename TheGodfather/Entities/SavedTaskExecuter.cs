#region USING_DIRECTIVES
using System;
using System.Threading;
using System.Threading.Tasks;

using TheGodfather.Extensions;
using TheGodfather.Services;
using TheGodfather.Services.Common;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Entities
{
    public class SavedTaskExecuter
    {
        public int Id { get; private set; }
        public SavedTask SavedTask { get; }

        private DiscordClient _client;
        private SharedData _shared;
        private DBService _db;
        private Timer _timer;


        public static async Task ScheduleAsync(SharedData shared, DBService db, CommandContext ctx, SavedTaskType type, string comment, DateTime exectime)
        {
            var task = new SavedTask() {
                ChannelId = ctx.Channel.Id,
                Comment = comment,
                ExecutionTime = exectime,
                GuildId = ctx.Guild.Id,
                Type = type,
                UserId = ctx.User.Id
            };

            var id = shared.GetAvailableTaskId();
            var texec = new SavedTaskExecuter(id, ctx.Client, task, shared, db);

            await texec.ScheduleExecutionAsync()
                .ConfigureAwait(false);
        }


        public SavedTaskExecuter(int id, DiscordClient client, SavedTask task, SharedData data, DBService db)
        {
            Id = id;
            _client = client;
            SavedTask = task;
            _shared = data;
            _db = db;
        }


        public async Task ScheduleExecutionAsync(bool add_to_db = true)
        {
            if (add_to_db)
                Id = await _db.AddSavedTaskAsync(SavedTask).ConfigureAwait(false);
            _shared.SavedTasks.TryAdd(Id, this);
            _timer = new Timer(Execute, null, (int)SavedTask.TimeUntilExecution.TotalMilliseconds, Timeout.Infinite);
        }

        private void Execute(object _)
        {
            try {
                switch (SavedTask.Type) {
                    case SavedTaskType.SendMessage:
                        SendMessageAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                        break;
                    case SavedTaskType.Unban:
                        UnbanUserAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                        break;
                }
                Logger.LogMessage(LogLevel.Info, $"Saved task executed: {nameof(SavedTask.Type)} ({SavedTask.Comment})<br>User ID: {SavedTask.UserId}<br>Guild ID: {SavedTask.GuildId}");
            } catch (Exception e) {
                Logger.LogException(LogLevel.Warning, e);
            } finally {
                RemoveTaskFromDatabase().ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }

        public async Task RemoveTaskFromDatabase()
        {
            if (_shared.SavedTasks.ContainsKey(Id))
                _shared.SavedTasks.TryRemove(Id, out var _);
            await _db.RemoveSavedTaskAsync(Id)
                .ConfigureAwait(false);
        }

        public async Task HandleMissedTaskExecutionAsync()
        {
            try {
                switch (SavedTask.Type) {
                    case SavedTaskType.SendMessage:
                        var channel = await _client.GetChannelAsync(SavedTask.ChannelId)
                            .ConfigureAwait(false);
                        var user = await _client.GetUserAsync(SavedTask.UserId)
                            .ConfigureAwait(false);
                        await channel.SendFailedEmbedAsync($"I have been asleep and failed to remind {user.Mention} to:\n\n{Formatter.Italic(SavedTask.Comment)}\n\nat {SavedTask.ExecutionTime.ToLongTimeString()} UTC")
                            .ConfigureAwait(false);
                        break;
                    case SavedTaskType.Unban:
                        await UnbanUserAsync()
                            .ConfigureAwait(false);
                        break;
                }
                Logger.LogMessage(LogLevel.Warning, $"Missed saved task: {nameof(SavedTask.Type)} ({SavedTask.Comment})<br>User ID: {SavedTask.UserId}<br>Guild ID: {SavedTask.GuildId}");
            } catch (Exception e) {
                Logger.LogException(LogLevel.Warning, e);
            } finally {
                RemoveTaskFromDatabase().ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }

        private async Task SendMessageAsync()
        {
            var channel = await _client.GetChannelAsync(SavedTask.ChannelId)
                .ConfigureAwait(false);
            var user = await _client.GetUserAsync(SavedTask.UserId)
                .ConfigureAwait(false);
            await channel.SendIconEmbedAsync($"{user.Mention} told to remind you to:\n\n{Formatter.Italic(SavedTask.Comment)}", DiscordEmoji.FromName(_client, ":alarm_clock:"))
                .ConfigureAwait(false);
        }

        private async Task UnbanUserAsync()
        {
            var guild = await _client.GetGuildAsync(SavedTask.GuildId)
                .ConfigureAwait(false);
            var user = await _client.GetUserAsync(SavedTask.UserId)
                .ConfigureAwait(false);
            await guild.UnbanMemberAsync(user, "Scheduled unban")
                .ConfigureAwait(false);
        }
    }
}
