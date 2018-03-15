#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
        public bool Completed 
            => (SavedTask.DispatchAt.ToUniversalTime() - DateTime.UtcNow).CompareTo(TimeSpan.Zero) < 0;

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
            TimeSpan t = SavedTask.DispatchAt.ToUniversalTime() - DateTime.UtcNow;
            if (t.CompareTo(TimeSpan.Zero) < 0) {
                RemoveTaskFromDatabase();
                return; // TODO
            }

            _timer = new Timer(Execute, null, (int)t.TotalMilliseconds, Timeout.Infinite);
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

                RemoveTaskFromDatabase();

            } catch (Exception e) {
                Logger.LogException(LogLevel.Warning, e);
            }
        }

        private void RemoveTaskFromDatabase()
        {
            _shared.SavedTasks.TryRemove(Id, out var _);
            _db.RemoveSavedTaskAsync(Id).ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}
