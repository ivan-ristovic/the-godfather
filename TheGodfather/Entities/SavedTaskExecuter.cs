#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using TheGodfather.Extensions;
using TheGodfather.Services;
using TheGodfather.Services.Common;

using DSharpPlus;
#endregion

namespace TheGodfather.Entities
{
    public class SavedTaskExecuter
    {
        public int Id { get; }
        public SavedTask SavedTask { get; }

        private DiscordClient _client;
        private SharedData _shared;
        private DBService _db;
        private Timer _timer;


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
            _timer = new Timer(Execute, null, (int)SavedTask.TimeUntilExecution.TotalMilliseconds, Timeout.Infinite);
        }

        private void Execute(object _)
        {
            try {
                switch (SavedTask.Type) {
                    case SavedTaskType.SendMessage:
                        var channel = _client.GetChannelAsync(SavedTask.ChannelId)
                            .ConfigureAwait(false).GetAwaiter().GetResult();
                        _client.SendMessageAsync(channel, SavedTask.Comment)
                            .ConfigureAwait(false).GetAwaiter().GetResult();
                        break;
                    case SavedTaskType.Unban:
                        // TODO
                        break;
                }

                RemoveTaskFromDatabase().ConfigureAwait(false).GetAwaiter().GetResult();

            } catch (Exception e) {
                Logger.LogException(LogLevel.Warning, e);
            }
        }

        public async Task RemoveTaskFromDatabase()
        {
            if (_shared.SavedTasks.ContainsKey(Id))
                _shared.SavedTasks.TryRemove(Id, out var _);
            await _db.RemoveSavedTaskAsync(Id)
                .ConfigureAwait(false);
        }

        public async Task ReportMissedExecutionAsync()
        {
            var channel = await _client.GetChannelAsync(SavedTask.ChannelId)
                .ConfigureAwait(false);
            await channel.SendFailedEmbedAsync("Execution missed!")
                .ConfigureAwait(false);
        }
    }
}
