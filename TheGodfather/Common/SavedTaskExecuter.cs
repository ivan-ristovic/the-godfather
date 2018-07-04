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

namespace TheGodfather.Common
{
    public sealed class SavedTaskExecuter
    {
        public int Id { get; private set; }
        public SavedTask SavedTask { get; }

        private DiscordClient _client;
        private SharedData _shared;
        private DBService _db;
        private Timer _timer;


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
                var id = await db.AddSavedTaskAsync(task)
                    .ConfigureAwait(false);
                var texec = new SavedTaskExecuter(id, client, task, shared, db);
                texec.ScheduleExecution();
            } catch (Exception e) {
                TheGodfather.LogProvider.LogException(LogLevel.Warning, e);
                return false;
            }

            return true;
        }


        public SavedTaskExecuter(int id, DiscordClient client, SavedTask task, SharedData data, DBService db)
        {
            Id = id;
            _client = client;
            SavedTask = task;
            _shared = data;
            _db = db;
        }


        public void ScheduleExecution()
        {
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
                TheGodfather.LogProvider.LogMessage(LogLevel.Info, 
                    $"| Saved task executed: {SavedTask.Type.GetType()} ({SavedTask.Comment})\n" +
                    $"| User ID: {SavedTask.UserId}\n" +
                    $"| Guild ID: {SavedTask.GuildId}\n" +
                    $"| Channel ID: {SavedTask.ChannelId}"
                );
            } catch (Exception e) {
                TheGodfather.LogProvider.LogException(LogLevel.Warning, e);
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

        public async Task HandleMissedExecutionAsync()
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
                    default:
                        break;
                }
                TheGodfather.LogProvider.LogMessage(LogLevel.Warning, 
                    $"| Executed missed saved task of type {SavedTask.Type.GetType()}\n|" +
                    $"| User ID: {SavedTask.UserId}\n" +
                    $"| Guild ID: {SavedTask.GuildId}\n" +
                    $"| Channel ID: {SavedTask.ChannelId}"
                );
            } catch (Exception e) {
                TheGodfather.LogProvider.LogException(LogLevel.Warning, e);
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
            await channel.SendIconEmbedAsync($"{user.Mention}'s reminder:\n\n{Formatter.Italic(SavedTask.Comment)}", DiscordEmoji.FromName(_client, ":alarm_clock:"))
                .ConfigureAwait(false);
        }

        private async Task UnbanUserAsync()
        {
            var guild = await _client.GetGuildAsync(SavedTask.GuildId)
                .ConfigureAwait(false);
            var user = await _client.GetUserAsync(SavedTask.UserId)
                .ConfigureAwait(false);
            await guild.UnbanMemberAsync(user, $"Temporary ban time expired")
                .ConfigureAwait(false);
        }

        public void Dispose()
        {
            _timer.Dispose();
        }
    }
}
